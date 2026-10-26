using CommunityToolkit.Mvvm.Input;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Invoke {
    public Option<Keys> Shortcut { get; init; }

    public sealed record Script(Macro Macro) : Invoke;

    public sealed record Effect(Func<RowScope, IO<Unit>> Run) : Invoke;

    public sealed record Listed(Func<RowScope, IO<Seq<Seq<Command<RowRule, Invoke>>>>> Groups) : Invoke;
}

public sealed record Watched(Seq<RowRule> Rules, IO<Unit> Refresh);

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
        new(face, None, new Invoke.Effect(run)) { Marked = new RowRule([], [], _ => IO.pure(marked)) };

    public static Command<RowRule, Invoke>.Run Hosted(Command command, Option<GlyphSource> glyph) =>
        new(new CommandFace(Conversions.Present(command.ID).IfNone(command.MenuText), command.MenuText, Conversions.Present(command.ToolTip), glyph),
            new RowRule([], [], _ => IO.lift(() => command.Enabled)), new Invoke.Effect(_ => IO.lift(command.Execute)) { Shortcut = Some(command.Shortcut).Filter(static keys => keys is not Keys.None) }) {
            Marked = command is CheckCommand check ? new RowRule([], [], _ => IO.lift(() => check.Checked)) : Option<RowRule>.None,
        };

    // --- [HOST]
    public static IO<CommandHost<RowRule, Invoke>> Host(RowScope scope, Action<IDisposable> retain) =>
        from shown in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        select new CommandHost<RowRule, Invoke>(
            rule => rule.Holds(scope),
            rule => rule.Holds(scope).Map(static holds => holds ? Check.On : Check.Off),
            (invoke, bounds) => invoke.Switch(
                (Scope: scope, Shown: shown, Bounds: bounds),
                script: static (at, script) => Scripted(script, at.Scope, None),
                effect: static (at, effect) => effect.Run(at.Scope),
                listed: static (at, listed) => listed.Groups(at.Scope).Bind(groups => Shown(groups, at.Bounds, at.Shown, at.Scope))),
            (groups, bounds) => Shown(groups, bounds, shown, scope));

    private static IO<Unit> Shown(Seq<Seq<Command<RowRule, Invoke>>> groups, SKRect bounds, Retained<IDisposable> shown, RowScope scope) =>
        from drained in shown.Drain
        from menu in DisposalOps.Acquire(keep => Menu(groups, scope, keep), (_, lease) => shown.Retain(lease))
        from opened in IO.lift(() => menu.Menu.Show(scope.Host.ValueUnsafe(), new PointF(bounds.Left, bounds.Bottom)))
        select unit;

    // --- [PRESSES]
    public static IO<IO<Unit>> Press(Command<RowRule, Invoke> command, RowScope scope, Action<IDisposable> retain) =>
        Localized(command, scope.Sink).Switch(
            (Scope: scope, Retain: retain),
            run: static (at, run) => Delivered(run, at.Scope, at.Retain).Map(deliver => Gated(run.Enabled, deliver, at.Scope)),
            choice: static (at, choice) => Opened([[choice]], at.Scope, at.Retain).Map(menu => Gated(choice.Enabled, IO.lift(menu.Menu.Show), at.Scope)),
            menu: static (at, menu) => Opened(menu.Groups, at.Scope, at.Retain).Map(opened => Gated(menu.Enabled, IO.lift(opened.Menu.Show), at.Scope)));

    public static IO<IO<Unit>> Delivered(Command<RowRule, Invoke>.Run run, RowScope scope, Action<IDisposable> retain) =>
        run.Invoke.Switch(
            (Run: run, Scope: scope, Retain: retain),
            script: static (at, script) => IO.pure(Scripted(script, at.Scope, Some(at.Run.Face.Caption))),
            effect: static (at, effect) => IO.pure(effect.Run(at.Scope)),
            listed: static (at, listed) => Popup(listed, at.Scope, at.Retain).Map(static menu => IO.lift(menu.Show)));

    public static IO<Func<Keys, IO<bool>>> Keyed(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, Action<IDisposable> retain) =>
        Runs(Localized(groups, scope.Sink))
            .Choose(static held => held.Run.Invoke.Shortcut.Map(keys => (Keys: keys, held.Run, held.Gate)))
            .TraverseM(held => Delivered(held.Run, scope, retain).Map(deliver => (held.Keys, Press: RowRules.Holds(held.Gate, scope).Bind(holds =>
                when(holds, IO.lift(() => Callbacks.Started(deliver, new CallbackSite(scope.Sink, typeof(Command), held.Run.Face.Name)))).As().Map(_ => holds)))))
            .As()
            .Map(static pressed => toHashMap(pressed) switch {
                var keyed => (Func<Keys, IO<bool>>)(keys => keyed.Find(keys).IfNone(IO.pure(value: false))),
            });

    private static IO<Unit> Gated(Option<RowRule> gate, IO<Unit> press, RowScope scope) =>
        RowRules.Holds(gate, scope).Bind(holds => when(holds, press).As());

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

    // --- [MENUS]
    public static IO<(ContextMenu Menu, IO<Unit> Refresh)> Menu(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from opened in Opened(Localized(groups, scope.Sink), scope, keep)
            from refresh in Listened(opened.Watched, scope, keep)
            select (opened.Menu, refresh), retain);

    public static IO<IO<Unit>> Fill(MenuItemCollection items, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep => Filled(items, Localized(groups, scope.Sink), scope, keep).Bind(watched => Listened(watched, scope, keep)), retain);

    private static IO<ContextMenu> Popup(Invoke.Listed listed, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from menu in DisposalOps.Acquire(IO.lift(static () => new ContextMenu()), keep)
            from relisted in Relisted(menu.Items, listed, h => menu.Opening += h, h => menu.Opening -= h, typeof(ContextMenu), scope, keep)
            select menu, retain);

    private static IO<(ContextMenu Menu, Seq<Watched> Watched)> Opened(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, Action<IDisposable> retain) =>
        from menu in DisposalOps.Acquire(IO.lift(static () => new ContextMenu()), retain)
        from watched in Filled(menu.Items, groups, scope, retain)
        from opening in Subscriptions.Attach(h => menu.Opening += h, h => menu.Opening -= h,
            Callbacks.Handler<EventArgs>(_ => Refreshed(watched), new CallbackSite(scope.Sink, typeof(ContextMenu), nameof(ContextMenu.Opening))), retain)
        select (menu, watched);

    private static IO<Seq<Watched>> Filled(MenuItemCollection items, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, Action<IDisposable> retain) =>
        from made in groups.TraverseM(cluster => cluster.TraverseM(command => Items(command, scope, retain)).As()).As()
        from placed in made.TraverseM(cluster =>
            from separator in DisposalOps.Acquire(IO.lift(static () => (MenuItem)new SeparatorMenuItem()), static held => IO.lift(held.Dispose), retain)
            from added in separator.Cons(cluster.Bind(static item => item.Items)).TraverseM(item => Added(items, item, retain)).As()
            select unit).As()
        from trimmed in IO.lift(items.Trim)
        select made.Flatten().Bind(static item => item.Watched);

    private static IO<Unit> Added(MenuItemCollection items, MenuItem item, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => items.Add(item)), _ => IO.lift(() => items.Remove(item)).Map(unit), retain);

    private static IO<(Seq<MenuItem> Items, Seq<Watched> Watched)> Items(Command<RowRule, Invoke> command, RowScope scope, Action<IDisposable> retain) =>
        command.Switch(
            (Scope: scope, Retain: retain),
            run: static (at, run) => run.Invoke.SwitchPartially(
                (Run: run, at.Scope, at.Retain),
                @default: static (held, _) =>
                    from made in Commanded(held.Run, held.Run.Enabled, held.Scope, held.Retain)
                    from item in Created(made.Command, held.Retain)
                    select (Seq(item), Seq(made.Watched)),
                listed: static (held, listed) =>
                    from sub in Submenu(held.Run.Face, trimmed: false, held.Scope, held.Retain)
                    from relisted in Relisted(sub.Items, listed, h => sub.Opening += h, h => sub.Opening -= h, typeof(SubMenuItem), held.Scope, held.Retain)
                    select (Seq<MenuItem>(sub), Seq(new Watched(held.Run.Enabled.ToSeq(), Enabling(sub, held.Run.Enabled, held.Scope))))),
            choice: static (at, choice) =>
                from radios in Radios(choice, at.Scope, at.Retain)
                from items in radios.TraverseM(radio => Created(radio.Command, at.Retain)).As()
                select (items, radios.Map(static radio => radio.Watched)),
            menu: static (at, menu) =>
                from sub in Submenu(menu.Face, trimmed: true, at.Scope, at.Retain)
                from watched in Filled(sub.Items, menu.Groups, at.Scope, at.Retain)
                select (Seq<MenuItem>(sub), watched.Add(new Watched(menu.Enabled.ToSeq(), Enabling(sub, menu.Enabled, at.Scope)))));

    private static IO<SubMenuItem> Submenu(CommandFace face, bool trimmed, RowScope scope, Action<IDisposable> retain) =>
        from sub in DisposalOps.Acquire(IO.lift(() => new SubMenuItem { Text = face.Caption, ToolTip = face.Help.ValueUnsafe(), Trim = trimmed }), retain)
        from themed in face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, IconSlot.Small, HostTheme.Changed, image => sub.Image = image, retain)).As()
        select sub;

    private static IO<MenuItem> Created(Command command, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(command.CreateMenuItem), static held =>
            Callbacks.Each(Seq(IO.lift(() => { held.Command = null; }), IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit), retain);

    private static IO<Unit> Relisted(
        MenuItemCollection items, Invoke.Listed listed, Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove, Type owner, RowScope scope, Action<IDisposable> retain) =>
        from shown in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from attached in Subscriptions.Attach(add, remove, Callbacks.Handler<EventArgs>(_ =>
            from drained in shown.Drain
            from groups in listed.Groups(scope)
            from refreshed in DisposalOps.Acquire(keep => Filled(items, Localized(groups, scope.Sink), scope, keep).Bind(Refreshed), (_, lease) => shown.Retain(lease))
            select refreshed, new CallbackSite(scope.Sink, owner, nameof(ContextMenu.Opening))), retain)
        select unit;

    private static IO<Unit> Enabling(MenuItem item, Option<RowRule> gate, RowScope scope) =>
        RowRules.Holds(gate, scope).Bind(holds => IO.lift(() => { item.Enabled = holds; }));

    // --- [BUTTONS]
    public static IO<(Control Control, IO<Unit> Refresh)> Button(Command<RowRule, Invoke> command, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from made in Pressed(Localized(command, scope.Sink), slot, scope, keep)
            from refresh in Listened(made.Watched, scope, keep)
            select (made.Control, refresh), retain);

    internal static IO<(Control Control, Seq<Watched> Watched)> Pressed(Command<RowRule, Invoke> command, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        command.Switch(
            (Slot: slot, Scope: scope, Retain: retain),
            run: static (at, run) => run.Invoke.SwitchPartially(
                (Run: run, at.Slot, at.Scope, at.Retain),
                @default: static (held, _) => held.Run.Marked.Match(
                    Some: mark => Toggled(held.Run, mark, held.Slot, held.Scope, held.Retain),
                    None: () => Pushed(held.Run, held.Slot, held.Scope, held.Retain)),
                listed: static (held, listed) => Popup(listed, held.Scope, held.Retain)
                    .Bind(menu => Dropped(held.Run.Face, held.Run.Enabled, menu, held.Slot, held.Scope, held.Retain))),
            choice: static (at, choice) => Segmented(choice, at.Slot, at.Scope, at.Retain),
            menu: static (at, menu) => menu.Primary.Match(
                Some: primary => Split(menu, primary, at.Slot, at.Scope, at.Retain),
                None: () => Opened(menu.Groups, at.Scope, at.Retain).Bind(opened =>
                    Dropped(menu.Face, menu.Enabled, opened.Menu, at.Slot, at.Scope, at.Retain).Map(dropped => (dropped.Control, opened.Watched + dropped.Watched)))));

    internal static IO<(TCommand Command, Watched Watched)> Bound<TCommand>(
        Command<RowRule, Invoke>.Run run, CommandFace face, Option<RowRule> enabled, TCommand command, IO<Unit> marking, RowScope scope, Action<IDisposable> retain)
        where TCommand : Command =>
        from faced in IO.lift(() => {
            (command.ID, command.MenuText, command.ToolBarText, command.ToolTip, command.Shortcut) =
                (face.Name, face.Caption, face.Caption, face.Help.IfNone(face.Caption), run.Invoke.Shortcut.IfNone(Keys.None));
        })
        from themed in face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, IconSlot.Small, HostTheme.Changed, image => command.Image = image, retain)).As()
        let refresh = Callbacks.Each(Seq(RowRules.Holds(enabled, scope).Bind(holds => IO.lift(() => { command.Enabled = holds; })), marking)).Map(unit)
        from deliver in Delivered(run, scope, retain)
        from delegated in Delegated(command, deliver.Bind(_ => refresh), new CallbackSite(scope.Sink, typeof(Command), face.Name), retain)
        select (command, new Watched(enabled.ToSeq() + run.Marked.ToSeq(), refresh));

    private static IO<(Command Command, Watched Watched)> Commanded(Command<RowRule, Invoke>.Run run, Option<RowRule> enabled, RowScope scope, Action<IDisposable> retain) =>
        run.Marked.Match(
            Some: mark => IO.lift(static () => new CheckCommand()).Bind(command => Bound(run, run.Face, enabled, command, Marking(command, mark, scope), scope, retain))
                .Map(static made => ((Command)made.Command, made.Watched)),
            None: () => IO.lift(static () => new Command()).Bind(command => Bound(run, run.Face, enabled, command, IO.pure(unit), scope, retain)));

    private static IO<Seq<(RadioCommand Command, Watched Watched)>> Radios(Command<RowRule, Invoke>.Choice choice, RowScope scope, Action<IDisposable> retain) =>
        choice.Members.FoldM(Seq<(RadioCommand Command, Watched Watched)>(), (made, member) =>
            from radio in IO.lift(() => made.Head.Match(Some: static first => new RadioCommand { Controller = first.Command }, None: static () => new RadioCommand()))
            from bound in Bound(member, member.Face, Gate(choice.Enabled, member.Enabled), radio, member.Marked.Traverse(mark => Marking(radio, mark, scope)).As().Map(unit), scope, retain)
            select made.Add(bound)).As();

    private static IO<Unit> Marking(CheckCommand command, RowRule mark, RowScope scope) =>
        mark.Holds(scope).Bind(holds => IO.lift(() => { command.Checked = holds; }));

    private static IO<Unit> Delegated(Command command, IO<Unit> deliver, CallbackSite site, Action<IDisposable> retain) =>
        from parent in cancelToken
        from lifetime in DisposalOps.Acquire(IO.lift(() => CancellationTokenSource.CreateLinkedTokenSource(parent)),
            static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit), retain)
        from attached in DisposalOps.Acquire(IO.lift(() => command.DelegatedCommand = new AsyncRelayCommand(async () => Callbacks.Run(
                await DisposalOps.Within(
                    () => EnvIO.New(token: lifetime.Token),
                    environment => IO.liftVAsync(() => deliver.RunAsync(environment)),
                    static environment => Callbacks.Each(Seq(IO.lift(environment.DisposeResources), IO.lift(environment.DisposeNonResources))).Map(unit))
                    .RunSafeAsync().ConfigureAwait(continueOnCapturedContext: true),
                site), AsyncRelayCommandOptions.AllowConcurrentExecutions)),
            _ => IO.lift(() => { command.DelegatedCommand = null; }), retain)
        select unit;

    private static IO<(Control Control, Seq<Watched> Watched)> Pushed(Command<RowRule, Invoke>.Run run, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        from made in IO.lift(static () => new Command()).Bind(command => Bound(run, run.Face, run.Enabled, command, IO.pure(unit), scope, retain))
        from control in run.Face.Glyph.Match(
            Some: glyph =>
                from button in DisposalOps.Acquire(IO.lift(() => new ImageButton { MaskImageWithBackgroundColorWhenDisabled = true, ToolTip = made.Command.ToolTip }), retain)
                from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, made.Command, retain)
                from themed in Icons.Themed(scope.Sink, glyph, slot, HostTheme.In(button), icon => button.Image = icon, retain)
                select (Control)button,
            None: () =>
                from button in DisposalOps.Acquire(IO.lift(() => new Button { Text = made.Command.ToolBarText, ToolTip = made.Command.ToolTip }), retain)
                from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, made.Command, retain)
                select (Control)button)
        select (control, Seq(made.Watched));

    private static IO<(Control Control, Seq<Watched> Watched)> Toggled(Command<RowRule, Invoke>.Run run, RowRule mark, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        from button in DisposalOps.Acquire(IO.lift(() => new ToggleButton { Text = run.Face.Glyph.Map(static _ => "").IfNone(run.Face.Caption), ToolTip = run.Face.Help.ValueUnsafe() }), retain)
        from command in IO.lift(static () => new CheckCommand())
        from made in Bound(run, run.Face, run.Enabled, command, Marking(command, mark, scope).Bind(_ => IO.lift(() => { button.Checked = command.Checked; })), scope, retain)
        from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, command, retain)
        from themed in run.Face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, slot, HostTheme.In(button), icon => button.Image = icon, retain)).As()
        select ((Control)button, Seq(made.Watched));

    private static IO<(Control Control, Seq<Watched> Watched)> Segmented(Command<RowRule, Invoke>.Choice choice, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.Single }), retain)
        from radios in Radios(choice, scope, retain)
        from segments in toSeq(choice.Members.Zip(radios, static (member, radio) => (First: member, Second: radio))).TraverseM(pair =>
            from segment in ButtonRows.Segment<ButtonSegmentedItem>(retain)
            from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (pair.First.Face.Glyph.Map(static _ => "").IfNone(pair.First.Face.Caption), pair.First.Face.Help.IfNone(pair.First.Face.Caption)); })
            from bound in DisposalOps.Change(() => segment.Command, value => segment.Command = value, pair.Second.Command, retain)
            from themed in pair.First.Face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, slot, HostTheme.In(strip), icon => segment.Image = icon, retain)).As()
            from selected in Subscriptions.Attach(h => pair.Second.Command.CheckedChanged += h, h => pair.Second.Command.CheckedChanged -= h,
                Callbacks.Handler<EventArgs>(_ => IO.lift(() => { segment.Selected = pair.Second.Command.Checked; }),
                    new CallbackSite(scope.Sink, typeof(RadioCommand), nameof(RadioCommand.CheckedChanged))), retain)
            select (SegmentedItem)segment).As()
        from added in IO.lift(() => strip.Items.AddRange(segments))
        select ((Control)strip, radios.Map(static radio => radio.Watched));

    private static IO<(Control Control, Seq<Watched> Watched)> Dropped(
        CommandFace face, Option<RowRule> enabled, ContextMenu menu, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.None }), retain)
        from segment in ButtonRows.Segment<MenuSegmentedItem>(retain)
        from placed in IO.lift(() => {
            (segment.Text, segment.ToolTip, segment.CanSelect, segment.Menu) = (face.Glyph.Map(static _ => "").IfNone(face.Caption), face.Help.IfNone(face.Caption), false, menu);
            strip.Items.Add(segment);
        })
        from themed in face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, slot, HostTheme.In(strip), icon => segment.Image = icon, retain)).As()
        select ((Control)strip, Seq(new Watched(enabled.ToSeq(), RowRules.Holds(enabled, scope).Bind(holds => IO.lift(() => { segment.Enabled = holds; })))));

    private static IO<(Control Control, Seq<Watched> Watched)> Split(
        Command<RowRule, Invoke>.Menu menu, Command<RowRule, Invoke>.Run primary, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        from opened in Opened(menu.Groups, scope, retain)
        from strip in DisposalOps.Acquire(IO.lift(() => new SegmentedButton { SelectionMode = primary.Marked.Map(static _ => SegmentedSelectionMode.Multiple).IfNone(SegmentedSelectionMode.None) }), retain)
        from pressed in ButtonRows.Segment<ButtonSegmentedItem>(retain)
        from dropdown in ButtonRows.Segment<MenuSegmentedItem>(retain)
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
            primary.Marked.Traverse(mark => Marking(command, mark, scope).Bind(_ => IO.lift(() => { pressed.Selected = command.Checked; }))).As().Map(unit).Bind(_ => shown), scope, retain)
        from bound in DisposalOps.Change(() => pressed.Command, value => pressed.Command = value, command, retain)
        from placed in IO.lift(() => {
            (pressed.Text, pressed.ToolTip) = (primary.Face.Glyph.Map(static _ => "").IfNone(primary.Face.Caption), primary.Face.Help.IfNone(primary.Face.Caption));
            (dropdown.ToolTip, dropdown.CanSelect, dropdown.Menu) = (menu.Face.Help.IfNone(menu.Face.Caption), false, opened.Menu);
            strip.Items.AddRange([pressed, dropdown]);
        })
        from themed in HostTheme.In(strip).Inline(_ => shown, scope.Sink, retain)
        select ((Control)strip, opened.Watched.Add(made.Watched with { Rules = made.Watched.Rules + chosen.Bind(static member => member.Marked.ToSeq()) }));

    // --- [REFRESH]
    public static IO<IO<Unit>> Listened(Seq<Watched> watched, RowScope scope, Action<IDisposable> retain) =>
        RuleIndex.Of(watched.Map(static held => (held.Rules.Bind(static rule => rule.Reads), held.Refresh))) switch {
            var index =>
                from heard in scope.Listen(changed => Callbacks.Each(RuleIndex.Affected(index, changed)).Map(unit), retain)
                from signals in watched.Bind(static held => held.Rules.Bind(static rule => rule.Sources)).Distinct().TraverseM(source =>
                    from events in source.Signals(scope)
                    from attached in events.TraverseM(signal =>
                        signal.Inline(_ => Refreshed(watched.Filter(held => held.Rules.Exists(rule => rule.Sources.Exists(read => read == source)))), scope.Sink, retain)).As()
                    select unit).As()
                from shown in Refreshed(watched)
                select Refreshed(watched),
        };

    private static IO<Unit> Refreshed(Seq<Watched> watched) => Callbacks.Each(watched.Map(static held => held.Refresh)).Map(unit);

    // --- [FACES]
    public static Seq<Seq<Command<RowRule, Invoke>>> Localized(Seq<Seq<Command<RowRule, Invoke>>> groups, IPlugInSink sink) =>
        groups.Map(cluster => cluster.Map(command => Localized(command, sink)));

    public static Command<RowRule, Invoke> Localized(Command<RowRule, Invoke> command, IPlugInSink sink) =>
        command.Faced((face, invoke) => Localized(face, invoke, sink));

    public static CommandFace Localized(CommandFace face, Option<Invoke> invoke, IPlugInSink sink) =>
        face with {
            Caption = RowText.Localize(face.Caption, sink).Local,
            Help = face.Help.Map(help => RowText.Localize(help, sink).Local),
            Shortcut = invoke.Bind(static held => held.Shortcut).Map(static keys => keys.ToShortcutString()) | face.Shortcut,
        };
}
