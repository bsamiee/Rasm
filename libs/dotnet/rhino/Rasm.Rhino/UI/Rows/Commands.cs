using CommunityToolkit.Mvvm.Input;
using Eto.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Inputs;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Invoke {
    public Option<Keys> Shortcut { get; init; }

    public sealed record Script(Macro Macro) : Invoke;

    public sealed record Effect(Func<RowScope, IO<Unit>> Run) : Invoke;

    public sealed record Listed(Func<RowScope, IO<Seq<Seq<Command<RowRule, Invoke>>>>> Groups) : Invoke;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandRows {
    // --- [DECLARE]
    public static Command<RowRule, Invoke>.Run Toggle(CommandFace face, Option<RowRule> enabled, RowRule marked, Func<RowScope, bool, IO<Unit>> write) =>
        new(face, enabled, new Invoke.Effect(scope => marked.Holds(scope).Bind(holds => write(scope, !holds)))) { Marked = marked };

    public static Command<RowRule, Invoke>.Choice Choices<TValue>(
        CommandFace face, Option<RowRule> enabled, Func<TValue, RowRule> chosen, Func<RowScope, TValue, IO<Unit>> write, Func<TValue, CommandFace> member)
        where TValue : class, ISmartEnum<string, TValue, ValidationError> =>
        new(face, enabled, toSeq(TValue.Items).Map(item =>
            new Command<RowRule, Invoke>.Run(member(item), None, new Invoke.Effect(scope => write(scope, item))) { Marked = chosen(item) }));

    public static Command<RowRule, Invoke>.Run Picked(CommandFace face, bool marked, Func<RowScope, IO<Unit>> run) =>
        new(face, None, new Invoke.Effect(run)) { Marked = new RowRule(_ => IO.pure(marked)) };

    // --- [HOST]
    public static IO<CommandHost<RowRule, Invoke>> Host(RowScope scope) =>
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from shown in IO.lift(static () => Ref(Option<Lease<(ContextMenu Menu, IO<Unit> Refresh)>>.None))
        select new CommandHost<RowRule, Invoke>(
            rule => rule.Holds(scope),
            rule => rule.Holds(scope).Map(static holds => holds ? Check.On : Check.Off),
            (invoke, bounds) => invoke.Switch(
                (Scope: scope, Owned: owned, Shown: shown, Bounds: bounds),
                script: static (at, script) => Scripted(script, at.Scope, None),
                effect: static (at, effect) => effect.Run(at.Scope),
                listed: static (at, listed) => listed.Groups(at.Scope).Bind(groups => Shown(groups, at.Bounds, at.Owned, at.Shown, at.Scope))),
            (groups, bounds) => Shown(groups, bounds, owned, shown, scope));

    private static IO<Unit> Shown(
        Seq<Seq<Command<RowRule, Invoke>>> groups, SKRect bounds, EnvIO owned, Ref<Option<Lease<(ContextMenu Menu, IO<Unit> Refresh)>>> shown, RowScope scope) =>
        from menu in Products.Cleared(shown).Bind(_ => Products.Swapped(shown, Menu(groups, scope))).Under(owned)
        from opened in IO.lift(() => menu.Menu.Show(scope.Host.ValueUnsafe(), new PointF(bounds.Left, bounds.Bottom)))
        select unit;

    // --- [PRESSES]
    public static IO<IO<Unit>> Press(Command<RowRule, Invoke> command, RowScope scope) =>
        Localized(command, scope.Owner).Switch(
            scope,
            run: static (at, run) => Delivered(run, at).Map(deliver => Gated(run.Enabled, deliver, at)),
            choice: static (at, choice) => Opened([[choice]], at).Map(menu => Gated(choice.Enabled, IO.lift(menu.Menu.Show), at)),
            menu: static (at, menu) => Opened(menu.Groups, at).Map(opened => Gated(menu.Enabled, IO.lift(opened.Menu.Show), at)));

    public static IO<IO<Unit>> Delivered(Command<RowRule, Invoke>.Run run, RowScope scope) =>
        run.Invoke.Switch(
            (Run: run, Scope: scope),
            script: static (at, script) => IO.pure(Scripted(script, at.Scope, Some(at.Run.Face.Caption))),
            effect: static (at, effect) => IO.pure(effect.Run(at.Scope)),
            listed: static (at, listed) => Popup(listed, at.Scope).Map(static menu => IO.lift(menu.Show)));

    public static IO<Func<Keys, IO<bool>>> Keyed(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        Runs(Localized(groups, scope.Owner))
            .Choose(static held => held.Run.Invoke.Shortcut.Map(keys => (Keys: keys, held.Run, held.Gate)))
            .TraverseM(held => Delivered(held.Run, scope).Map(deliver => (held.Keys, Press: Holds(held.Gate, scope).Bind(holds =>
                when(holds, IO.lift(() => scope.Owner.Start(deliver))).As().Map(_ => holds)))))
            .As()
            .Map(static pressed => toHashMap(pressed) switch {
                var keyed => (Func<Keys, IO<bool>>)(keys => keyed.Find(keys).IfNone(IO.pure(value: false))),
            });

    private static IO<Unit> Gated(Option<RowRule> gate, IO<Unit> press, RowScope scope) =>
        Holds(gate, scope).Bind(holds => when(holds, press).As());

    private static IO<Unit> Scripted(Invoke.Script script, RowScope scope, Option<string> display) =>
        from document in scope.DocumentIO
        from ran in Documents.RunScript(document, script.Macro.Text, echo: false, display)
        select ran;

    private static Seq<(Command<RowRule, Invoke>.Run Run, Option<RowRule> Gate)> Runs(Seq<Seq<Command<RowRule, Invoke>>> groups) =>
        groups.Flatten().Bind(static command => command.Switch(
            run: static run => Seq((Run: run, Gate: run.Enabled)),
            choice: static choice => choice.Members.Map(member => (Run: member, Gate: Gate(choice.Enabled, member.Enabled))),
            menu: static menu => (menu.Primary.ToSeq().Map(static primary => (Run: primary, Gate: primary.Enabled)) + Runs(menu.Groups))
                .Map(held => (held.Run, Gate: Gate(menu.Enabled, held.Gate)))));

    public static Option<RowRule> Gate(Option<RowRule> parent, Option<RowRule> own) =>
        parent.Map(outer => own.Map(inner => outer & inner).IfNone(outer)) | own;

    public static IO<bool> Holds(Option<RowRule> gate, RowScope scope) => gate.Match(Some: held => held.Holds(scope), None: static () => IO.pure(value: true));

    // --- [MENUS]
    public static IO<(ContextMenu Menu, IO<Unit> Refresh)> Menu(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        from opened in Opened(Localized(groups, scope.Owner), scope)
        from refresh in Listened(opened.Refreshes, scope)
        select (opened.Menu, refresh);

    public static IO<IO<Unit>> Fill(MenuItemCollection items, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        Filled(items, Localized(groups, scope.Owner), scope).Bind(watched => Listened(watched, scope));

    private static IO<ContextMenu> Popup(Invoke.Listed listed, RowScope scope) =>
        from menu in use(static () => new ContextMenu())
        from relisted in Relisted(menu.Items, listed, Subscriptions.Host<EventArgs>(h => menu.Opening += h, h => menu.Opening -= h), scope)
        select menu;

    private static IO<(ContextMenu Menu, Seq<IO<Unit>> Refreshes)> Opened(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        from menu in use(static () => new ContextMenu())
        from watched in Filled(menu.Items, groups, scope)
        from opening in Subscriptions.Host<EventArgs>(h => menu.Opening += h, h => menu.Opening -= h).Inline(_ => Refreshed(watched), scope.Owner)
        select (menu, watched);

    private static IO<Seq<IO<Unit>>> Filled(MenuItemCollection items, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        from made in groups.TraverseM(cluster => cluster.TraverseM(command => Items(command, scope)).As()).As()
        from placed in made.TraverseM(cluster =>
            from separator in use(static () => (MenuItem)new SeparatorMenuItem())
            from added in separator.Cons(cluster.Bind(static item => item.Items)).TraverseM(item => Added(items, item)).As()
            select unit).As()
        from trimmed in IO.lift(items.Trim)
        select made.Flatten().Bind(static item => item.Refreshes);

    private static IO<Unit> Added(MenuItemCollection items, MenuItem item) =>
        IO.lift(() => items.Add(item)).Bind(_ => Products.Released(() => items.Remove(item)));

    private static IO<(Seq<MenuItem> Items, Seq<IO<Unit>> Refreshes)> Items(Command<RowRule, Invoke> command, RowScope scope) =>
        command.Switch(
            scope,
            run: static (at, run) => run.Invoke.SwitchPartially(
                (Run: run, Scope: at),
                @default: static (held, _) =>
                    from made in Commanded(held.Run, held.Run.Enabled, held.Scope)
                    from item in Created(made.Command)
                    select (Seq(item), Seq(made.Refresh)),
                listed: static (held, listed) =>
                    from sub in Submenu(held.Run.Face, trimmed: false, held.Scope)
                    from relisted in Relisted(sub.Items, listed, Subscriptions.Host<EventArgs>(h => sub.Opening += h, h => sub.Opening -= h), held.Scope)
                    select (Seq<MenuItem>(sub), Seq(Enabling(sub, held.Run.Enabled, held.Scope)))),
            choice: static (at, choice) =>
                from radios in Radios(choice, at)
                from items in radios.TraverseM(static radio => Created(radio.Command)).As()
                select (items, radios.Map(static radio => radio.Refresh)),
            menu: static (at, menu) =>
                from sub in Submenu(menu.Face, trimmed: true, at)
                from watched in Filled(sub.Items, menu.Groups, at)
                select (Seq<MenuItem>(sub), watched.Add(Enabling(sub, menu.Enabled, at))));

    private static IO<SubMenuItem> Submenu(CommandFace face, bool trimmed, RowScope scope) =>
        from sub in use(() => new SubMenuItem { Text = face.Caption, ToolTip = face.Help.ValueUnsafe(), Trim = trimmed })
        from themed in face.Glyph.Traverse(glyph => Icons.Themed(scope.Owner, glyph, IconSlot.Small, HostTheme.Changed, image => sub.Image = image)).As()
        select sub;

    private static IO<MenuItem> Created(Command command) =>
        use(IO.lift(command.CreateMenuItem), static held =>
            Callbacks.Each(Seq(IO.lift(() => { held.Command = null; }), IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit));

    private static IO<Unit> Relisted(MenuItemCollection items, Invoke.Listed listed, HostEvent<EventArgs> opening, RowScope scope) =>
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from shown in IO.lift(static () => Ref(Option<Lease<Seq<IO<Unit>>>>.None))
        from attached in opening.Inline(_ =>
            from groups in listed.Groups(scope)
            from refreshes in Products.Cleared(shown).Bind(_ => Products.Swapped(shown, Filled(items, Localized(groups, scope.Owner), scope))).Under(owned)
            from refreshed in Refreshed(refreshes)
            select refreshed, scope.Owner)
        select unit;

    private static IO<Unit> Enabling(MenuItem item, Option<RowRule> gate, RowScope scope) =>
        Holds(gate, scope).Bind(holds => IO.lift(() => { item.Enabled = holds; }));

    // --- [BUTTONS]
    public static IO<(Control Control, IO<Unit> Refresh)> Button(Command<RowRule, Invoke> command, IconSlot slot, RowScope scope) =>
        from made in Pressed(Localized(command, scope.Owner), slot, scope)
        from refresh in Listened(made.Refreshes, scope)
        select (made.Control, refresh);

    internal static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Pressed(Command<RowRule, Invoke> command, IconSlot slot, RowScope scope) =>
        command.Switch(
            (Slot: slot, Scope: scope),
            run: static (at, run) => run.Invoke.SwitchPartially(
                (Run: run, at.Slot, at.Scope),
                @default: static (held, _) => held.Run.Marked.Match(
                    Some: mark => Toggled(held.Run, mark, held.Slot, held.Scope),
                    None: () => Pushed(held.Run, held.Slot, held.Scope)),
                listed: static (held, listed) => Popup(listed, held.Scope)
                    .Bind(menu => Dropped(held.Run.Face, held.Run.Enabled, menu, held.Slot, held.Scope))),
            choice: static (at, choice) => Segmented(choice, at.Slot, at.Scope),
            menu: static (at, menu) => menu.Primary.Match(
                Some: primary => Split(menu, primary, at.Slot, at.Scope),
                None: () => Opened(menu.Groups, at.Scope).Bind(opened =>
                    Dropped(menu.Face, menu.Enabled, opened.Menu, at.Slot, at.Scope).Map(dropped => (dropped.Control, opened.Refreshes + dropped.Refreshes)))));

    internal static IO<(TCommand Command, IO<Unit> Refresh)> Bound<TCommand>(
        Command<RowRule, Invoke>.Run run, CommandFace face, Option<RowRule> enabled, TCommand command, IO<Unit> marking, RowScope scope)
        where TCommand : Command =>
        from faced in IO.lift(() => {
            (command.ID, command.MenuText, command.ToolBarText, command.ToolTip, command.Shortcut) =
                (face.Name, face.Caption, face.Caption, face.Help.IfNone(face.Caption), run.Invoke.Shortcut.IfNone(Keys.None));
        })
        from themed in face.Glyph.Traverse(glyph => Icons.Themed(scope.Owner, glyph, IconSlot.Small, HostTheme.Changed, image => command.Image = image)).As()
        let refresh = Callbacks.Each(Seq(Holds(enabled, scope).Bind(holds => IO.lift(() => { command.Enabled = holds; })), marking)).Map(unit)
        from deliver in Delivered(run, scope)
        from delegated in Delegated(command, deliver.Bind(_ => refresh), scope.Owner)
        select (command, refresh);

    private static IO<(Command Command, IO<Unit> Refresh)> Commanded(Command<RowRule, Invoke>.Run run, Option<RowRule> enabled, RowScope scope) =>
        run.Marked.Match(
            Some: mark => IO.lift(static () => new CheckCommand()).Bind(command => Bound(run, run.Face, enabled, command, Marking(command, mark, scope), scope))
                .Map(static made => ((Command)made.Command, made.Refresh)),
            None: () => IO.lift(static () => new Command()).Bind(command => Bound(run, run.Face, enabled, command, IO.pure(unit), scope)));

    private static IO<Seq<(RadioCommand Command, IO<Unit> Refresh)>> Radios(Command<RowRule, Invoke>.Choice choice, RowScope scope) =>
        choice.Members.FoldM(Seq<(RadioCommand Command, IO<Unit> Refresh)>(), (made, member) =>
            from radio in IO.lift(() => made.Head.Match(Some: static first => new RadioCommand { Controller = first.Command }, None: static () => new RadioCommand()))
            from bound in Bound(member, member.Face, Gate(choice.Enabled, member.Enabled), radio, member.Marked.Traverse(mark => Marking(radio, mark, scope)).As().Map(unit), scope)
            select made.Add(bound)).As();

    private static IO<Unit> Marking(CheckCommand command, RowRule mark, RowScope scope) =>
        mark.Holds(scope).Bind(holds => IO.lift(() => { command.Checked = holds; }));

    private static IO<Unit> Delegated(Command command, IO<Unit> deliver, IPlugInLifetime owner) =>
        from parent in cancelToken
        from lifetime in use(IO.lift(() => CancellationTokenSource.CreateLinkedTokenSource(parent)),
            static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit))
        from attached in Products.Changed(() => command.DelegatedCommand, value => command.DelegatedCommand = value, new AsyncRelayCommand(async () => owner.Run(IO.lift(
                await use(() => EnvIO.New(token: lifetime.Token)).Bind(environment => deliver.Under(environment)).SettledAsync().ConfigureAwait(continueOnCapturedContext: true))),
            AsyncRelayCommandOptions.AllowConcurrentExecutions))
        select unit;

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Pushed(Command<RowRule, Invoke>.Run run, IconSlot slot, RowScope scope) =>
        from made in IO.lift(static () => new Command()).Bind(command => Bound(run, run.Face, run.Enabled, command, IO.pure(unit), scope))
        from control in run.Face.Glyph.Match(
            Some: glyph =>
                from button in use(() => new ImageButton { MaskImageWithBackgroundColorWhenDisabled = true, ToolTip = made.Command.ToolTip })
                from bound in Products.Changed(() => button.Command, value => button.Command = value, made.Command)
                from themed in Icons.Themed(scope.Owner, glyph, slot, HostTheme.In(button), icon => button.Image = icon)
                select (Control)button,
            None: () =>
                from button in use(() => new Button { Text = made.Command.ToolBarText, ToolTip = made.Command.ToolTip })
                from bound in Products.Changed(() => button.Command, value => button.Command = value, made.Command)
                select (Control)button)
        select (control, Seq(made.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Toggled(Command<RowRule, Invoke>.Run run, RowRule mark, IconSlot slot, RowScope scope) =>
        from button in use(() => new ToggleButton { Text = run.Face.Glyph.Map(static _ => "").IfNone(run.Face.Caption), ToolTip = run.Face.Help.ValueUnsafe() })
        from command in IO.lift(static () => new CheckCommand())
        from made in Bound(run, run.Face, run.Enabled, command, Marking(command, mark, scope).Bind(_ => IO.lift(() => { button.Checked = command.Checked; })), scope)
        from bound in Products.Changed(() => button.Command, value => button.Command = value, command)
        from themed in run.Face.Glyph.Traverse(glyph => Icons.Themed(scope.Owner, glyph, slot, HostTheme.In(button), icon => button.Image = icon)).As()
        select ((Control)button, Seq(made.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Segmented(Command<RowRule, Invoke>.Choice choice, IconSlot slot, RowScope scope) =>
        from strip in use(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.Single })
        from radios in Radios(choice, scope)
        from segments in toSeq(choice.Members.Zip(radios, static (member, radio) => (First: member, Second: radio))).TraverseM(pair =>
            from segment in ButtonRows.Segment<ButtonSegmentedItem>()
            from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (pair.First.Face.Glyph.Map(static _ => "").IfNone(pair.First.Face.Caption), pair.First.Face.Help.IfNone(pair.First.Face.Caption)); })
            from bound in Products.Changed(() => segment.Command, value => segment.Command = value, pair.Second.Command)
            from themed in pair.First.Face.Glyph.Traverse(glyph => Icons.Themed(scope.Owner, glyph, slot, HostTheme.In(strip), icon => segment.Image = icon)).As()
            from selected in Subscriptions.Host<EventArgs>(h => pair.Second.Command.CheckedChanged += h, h => pair.Second.Command.CheckedChanged -= h)
                .Inline(_ => IO.lift(() => { segment.Selected = pair.Second.Command.Checked; }), scope.Owner)
            select (SegmentedItem)segment).As()
        from added in IO.lift(() => strip.Items.AddRange(segments))
        select ((Control)strip, radios.Map(static radio => radio.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Dropped(
        CommandFace face, Option<RowRule> enabled, ContextMenu menu, IconSlot slot, RowScope scope) =>
        from strip in use(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.None })
        from segment in ButtonRows.Segment<MenuSegmentedItem>()
        from placed in IO.lift(() => {
            (segment.Text, segment.ToolTip, segment.CanSelect, segment.Menu) = (face.Glyph.Map(static _ => "").IfNone(face.Caption), face.Help.IfNone(face.Caption), false, menu);
            strip.Items.Add(segment);
        })
        from themed in face.Glyph.Traverse(glyph => Icons.Themed(scope.Owner, glyph, slot, HostTheme.In(strip), icon => segment.Image = icon)).As()
        select ((Control)strip, Seq(Holds(enabled, scope).Bind(holds => IO.lift(() => { segment.Enabled = holds; }))));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Split(
        Command<RowRule, Invoke>.Menu menu, Command<RowRule, Invoke>.Run primary, IconSlot slot, RowScope scope) =>
        from opened in Opened(menu.Groups, scope)
        from strip in use(() => new SegmentedButton { SelectionMode = primary.Marked.Map(static _ => SegmentedSelectionMode.Multiple).IfNone(SegmentedSelectionMode.None) })
        from pressed in ButtonRows.Segment<ButtonSegmentedItem>()
        from dropdown in ButtonRows.Segment<MenuSegmentedItem>()
        from command in IO.lift(static () => new CheckCommand())
        let gate = Gate(menu.Enabled, primary.Enabled)
        let chosen = menu.Groups.Flatten().Bind(static held => held.Switch(run: static _ => Seq<Command<RowRule, Invoke>.Run>(), choice: static choice => choice.Members, menu: static _ => Seq<Command<RowRule, Invoke>.Run>()))
        let shown =
            from marks in chosen.TraverseM(member => member.Marked.Traverse(mark => mark.Holds(scope)).As().Map(holds => (member.Face.Glyph, On: holds.IfNone(noneValue: false)))).As()
            from glyph in IO.pure(marks.Find(static held => held.On).Bind(static held => held.Glyph) | primary.Face.Glyph)
            from icon in glyph.Traverse(art => Icons.Frames(art, slot)).As()
            from painted in IO.lift(() => { pressed.Image = icon.ValueUnsafe(); })
            select unit
        from made in Bound(primary, primary.Face, gate, command,
            primary.Marked.Traverse(mark => Marking(command, mark, scope).Bind(_ => IO.lift(() => { pressed.Selected = command.Checked; }))).As().Map(unit).Bind(_ => shown), scope)
        from bound in Products.Changed(() => pressed.Command, value => pressed.Command = value, command)
        from placed in IO.lift(() => {
            (pressed.Text, pressed.ToolTip) = (primary.Face.Glyph.Map(static _ => "").IfNone(primary.Face.Caption), primary.Face.Help.IfNone(primary.Face.Caption));
            (dropdown.ToolTip, dropdown.CanSelect, dropdown.Menu) = (menu.Face.Help.IfNone(menu.Face.Caption), false, opened.Menu);
            strip.Items.AddRange([pressed, dropdown]);
        })
        from themed in HostTheme.In(strip).Inline(_ => shown, scope.Owner)
        select ((Control)strip, opened.Refreshes.Add(made.Refresh));

    // --- [REFRESH]
    public static IO<IO<Unit>> Listened(Seq<IO<Unit>> refreshes, RowScope scope) =>
        from heard in scope.Listen(_ => Refreshed(refreshes))
        from shown in Refreshed(refreshes)
        select Refreshed(refreshes);

    private static IO<Unit> Refreshed(Seq<IO<Unit>> refreshes) => Callbacks.Each(refreshes).Map(unit);

    // --- [FACES]
    public static Seq<Seq<Command<RowRule, Invoke>>> Localized(Seq<Seq<Command<RowRule, Invoke>>> groups, IPlugInLifetime owner) =>
        groups.Map(cluster => cluster.Map(command => Localized(command, owner)));

    public static Command<RowRule, Invoke> Localized(Command<RowRule, Invoke> command, IPlugInLifetime owner) =>
        command.Faced((face, invoke) => Localized(face, invoke, owner));

    public static CommandFace Localized(CommandFace face, Option<Invoke> invoke, IPlugInLifetime owner) =>
        face with {
            Caption = RowText.Localize(face.Caption, owner).Local,
            Help = face.Help.Map(help => RowText.Localize(help, owner).Local),
            Shortcut = invoke.Bind(static held => held.Shortcut).Map(static keys => keys.ToShortcutString()) | face.Shortcut,
        };
}
