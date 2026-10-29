using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Styling;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption) {
    public Option<string> Help { get; init; }
    public Option<IconResource> Icon { get; init; }
    public Option<string> Shortcut { get; init; }

    internal void Applied(Command command, Keys shortcut) =>
        (command.ID, command.MenuText, command.ToolBarText, command.ToolTip, command.Shortcut) = (Name, Caption, Icon.Map(static _ => "").IfNone(Caption), Help.IfNone(Caption), shortcut);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Invoke {
    public Option<Keys> Shortcut { get; init; }

    public sealed record Script(Macro Macro) : Invoke;

    public sealed record Effect(Func<RowScope, IO<Unit>> Run) : Invoke;

    public sealed record Popup(Func<RowScope, IO<Seq<Seq<Command<Invoke>>>>> Groups) : Invoke;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Command<TInvoke> where TInvoke : notnull {
    private Command(CommandFace face, Option<RowRule> enabled) => (Face, Enabled) = (face, enabled);

    public CommandFace Face { get; private init; }
    public Option<RowRule> Enabled { get; }

    public Command<TInvoke> Faced(Func<CommandFace, Option<TInvoke>, CommandFace> into) =>
        Switch(
            into,
            run: static (into, run) => (Command<TInvoke>)(run with { Face = into(run.Face, Some(run.Invoke)) }),
            choice: static (into, choice) => choice with { Face = into(choice.Face, None), Members = choice.Members.Map(member => member with { Face = into(member.Face, Some(member.Invoke)) }) },
            menu: static (into, menu) => menu with {
                Face = into(menu.Face, None),
                Groups = menu.Groups.Map(group => group.Map(command => command.Faced(into))),
                Primary = menu.Primary.Map(primary => primary with { Face = into(primary.Face, Some(primary.Invoke)) }),
            });

    public sealed record Run(CommandFace Face, Option<RowRule> Enabled, TInvoke Invoke) : Command<TInvoke>(Face, Enabled) {
        public Option<RowRule> Marked { get; init; }
        public Option<TInvoke> Alternate { get; init; }
    }

    public sealed record Choice(CommandFace Face, Option<RowRule> Enabled, Seq<Run> Members) : Command<TInvoke>(Face, Enabled);

    public sealed record Menu(CommandFace Face, Option<RowRule> Enabled, Seq<Seq<Command<TInvoke>>> Groups) : Command<TInvoke>(Face, Enabled) {
        public Option<Run> Primary { get; init; }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandRows {
    // --- [DECLARE]
    public static Command<Invoke>.Run Toggle(CommandFace face, Option<RowRule> enabled, RowRule marked, Func<RowScope, bool, IO<Unit>> write) =>
        new(face, enabled, new Invoke.Effect(scope => marked.Holds(scope).Bind(holds => write(scope, !holds)))) { Marked = marked };

    public static Command<Invoke>.Choice Choices<TValue>(
        CommandFace face, Option<RowRule> enabled, Func<TValue, RowRule> chosen, Func<RowScope, TValue, IO<Unit>> write, Func<TValue, CommandFace> member)
        where TValue : class, ISmartEnum<string, TValue, ValidationError> =>
        new(face, enabled, toSeq(TValue.Items).Map(item =>
            new Command<Invoke>.Run(member(item), None, new Invoke.Effect(scope => write(scope, item))) { Marked = chosen(item) }));

    public static Command<Invoke>.Run Picked(CommandFace face, bool marked, Func<RowScope, IO<Unit>> run) =>
        new(face, None, new Invoke.Effect(run)) { Marked = new RowRule(_ => IO.pure(marked)) };

    // --- [PRESSES]
    public static IO<IO<Unit>> Press(Command<Invoke> command, RowScope scope) =>
        Localized(command).Switch(
            scope,
            run: static (at, run) => Commanded(run, run.Enabled, at).Map(static made => Executed(made).Map(unit)),
            choice: static (at, choice) => Opened([[choice]], at).Map(menu => Gated(choice.Enabled, IO.lift(menu.Menu.Show), at)),
            menu: static (at, menu) => Opened(menu.Groups, at).Map(opened => Gated(menu.Enabled, IO.lift(opened.Menu.Show), at)));

    public static IO<Func<Keys, IO<bool>>> Keyed(Seq<Seq<Command<Invoke>>> groups, RowScope scope) =>
        Runs(Localized(groups))
            .Choose(static held => held.Run.Invoke.Shortcut.Map(keys => (Keys: keys, held.Run, held.Gate)))
            .TraverseM(held => Commanded(held.Run, held.Gate, scope).Map(made => (held.Keys, Press: Executed(made))))
            .As()
            .Map(static pressed => toHashMap(pressed) switch {
                var keyed => (Func<Keys, IO<bool>>)(keys => keyed.Find(keys).IfNone(IO.pure(value: false))),
            });

    public static IO<Unit> Pressed(Func<Keys, IO<bool>> keyed, KeyEventArgs args) =>
        keyed(args.KeyData).Bind(handled => IO.lift(() => { args.Handled = handled; }));

    public static Option<RowRule> Gate(Option<RowRule> parent, Option<RowRule> own) =>
        parent.Map(outer => own.Map(inner => outer & inner).IfNone(outer)) | own;

    public static IO<bool> Holds(Option<RowRule> gate, RowScope scope) => gate.Match(Some: held => held.Holds(scope), None: static () => IO.pure(value: true));

    private static IO<bool> Executed((Command Command, IO<Unit> Refresh) made) =>
        from refreshed in made.Refresh
        from enabled in IO.lift(() => made.Command.Enabled)
        from executed in when(enabled, IO.lift(made.Command.Execute)).As()
        select enabled;

    private static IO<Unit> Gated(Option<RowRule> gate, IO<Unit> press, RowScope scope) =>
        Holds(gate, scope).Bind(holds => when(holds, press).As());

    private static IO<IO<Unit>> Delivered(Command<Invoke>.Run run, RowScope scope) =>
        run.Invoke.Switch(
            (Run: run, Scope: scope),
            script: static (at, script) => IO.pure(from document in at.Scope.DocumentIO from ran in Documents.RunScript(document, script.Macro.Text, echo: false, Some(at.Run.Face.Caption)) select ran),
            effect: static (at, effect) => IO.pure(effect.Run(at.Scope)),
            popup: static (at, popup) => Popped(popup, at.Scope).Map(static menu => IO.lift(menu.Show)));

    private static Seq<(Command<Invoke>.Run Run, Option<RowRule> Gate)> Runs(Seq<Seq<Command<Invoke>>> groups) =>
        groups.Flatten().Bind(static command => command.Switch(
            run: static run => Seq((Run: run, Gate: run.Enabled)),
            choice: static choice => choice.Members.Map(member => (Run: member, Gate: Gate(choice.Enabled, member.Enabled))),
            menu: static menu => (menu.Primary.ToSeq().Map(static primary => (Run: primary, Gate: primary.Enabled)) + Runs(menu.Groups))
                .Map(held => (held.Run, Gate: Gate(menu.Enabled, held.Gate)))));

    // --- [COMMANDS]
    private static IO<(Command Command, IO<Unit> Refresh)> Commanded(Command<Invoke>.Run run, Option<RowRule> gate, RowScope scope) =>
        run.Marked.Match(
            Some: mark => Bound(run, gate, new CheckCommand(), command => Marking(command, mark, scope), scope).Map(static made => ((Command)made.Command, made.Refresh)),
            None: () => Bound(run, gate, new Command(), static _ => IO.pure(unit), scope));

    private static IO<(TCommand Command, IO<Unit> Refresh)> Bound<TCommand>(
        Command<Invoke>.Run run, Option<RowRule> gate, TCommand command, Func<TCommand, IO<Unit>> marking, RowScope scope)
        where TCommand : Command =>
        from applied in Applied(run.Face, command, run.Invoke.Shortcut.IfNone(Keys.None))
        from busy in atomIO(value: false)
        let refresh =
            from holds in Holds(gate, scope)
            from running in busy.ValueIO
            from enabled in IO.lift(() => { command.Enabled = holds && !running; })
            from marked in marking(command)
            select unit
        from deliver in Delivered(run, scope)
        from executed in Subscriptions.On<EventArgs>(h => command.Executed += h, h => command.Executed -= h).Attach(_ =>
            busy.SwapIO(static _ => true).Bind(_ => refresh).Bind(_ => deliver).Finally(busy.SwapIO(static _ => false).Bind(_ => refresh).OnUiThread()))
        from first in refresh
        select (command, refresh);

    private static IO<(Command Command, IO<Unit> Refresh)> Faced(CommandFace face, Option<RowRule> gate, RowScope scope) =>
        from command in IO.lift(static () => new Command())
        from applied in Applied(face, command, Keys.None)
        let refresh = Holds(gate, scope).Bind(holds => IO.lift(() => { command.Enabled = holds; }))
        from first in refresh
        select (command, refresh);

    private static IO<Unit> Applied(CommandFace face, Command command, Keys shortcut) =>
        from faced in IO.lift(() => face.Applied(command, shortcut))
        from themed in face.Icon.Traverse(icon => Icons.Themed(icon, IconSlot.Small, image => command.Image = image)).As()
        select unit;

    private static IO<Seq<(RadioCommand Command, IO<Unit> Refresh)>> Radios(Command<Invoke>.Choice choice, RowScope scope) =>
        choice.Members.FoldM(Seq<(RadioCommand Command, IO<Unit> Refresh)>(), (made, member) =>
            from radio in IO.lift(() => made.Head.Match(Some: static first => new RadioCommand { Controller = first.Command }, None: static () => new RadioCommand()))
            from bound in Bound(member, Gate(choice.Enabled, member.Enabled), radio, held => member.Marked.Traverse(mark => Marking(held, mark, scope)).As().Map(unit), scope)
            select made.Add(bound)).As();

    private static IO<Unit> Marking(CheckCommand command, RowRule mark, RowScope scope) =>
        mark.Holds(scope).Bind(holds => IO.lift(() => { command.Checked = holds; }));

    // --- [MENUS]
    public static IO<(ContextMenu Menu, IO<Unit> Refresh)> Menu(Seq<Seq<Command<Invoke>>> groups, RowScope scope) =>
        from opened in Opened(Localized(groups), scope)
        from refresh in Listened(opened.Refreshes, scope)
        select (opened.Menu, refresh);

    public static IO<IO<Unit>> Fill(MenuItemCollection items, Seq<Seq<Command<Invoke>>> groups, RowScope scope) =>
        Filled(items, Localized(groups), scope).Bind(watched => Listened(watched, scope));

    private static IO<ContextMenu> Popped(Invoke.Popup popup, RowScope scope) =>
        from menu in use(static () => new ContextMenu())
        from populated in Populated(menu.Items, popup, Subscriptions.On<EventArgs>(h => menu.Opening += h, h => menu.Opening -= h), scope)
        select menu;

    private static IO<(ContextMenu Menu, Seq<IO<Unit>> Refreshes)> Opened(Seq<Seq<Command<Invoke>>> groups, RowScope scope) =>
        from menu in use(static () => new ContextMenu())
        from watched in Filled(menu.Items, groups, scope)
        from opening in Subscriptions.On<EventArgs>(h => menu.Opening += h, h => menu.Opening -= h).Attach(_ => Refreshed(watched))
        select (menu, watched);

    private static IO<Seq<IO<Unit>>> Filled(MenuItemCollection items, Seq<Seq<Command<Invoke>>> groups, RowScope scope) =>
        from made in groups.TraverseM(cluster => cluster.TraverseM(command => Items(command, scope)).As()).As()
        from placed in made.TraverseM(cluster =>
            from separator in use(static () => (MenuItem)new SeparatorMenuItem())
            from added in separator.Cons(cluster.Bind(static item => item.Items)).TraverseM(item => Added(items, item)).As()
            select unit).As()
        from trimmed in IO.lift(items.Trim)
        select made.Flatten().Bind(static item => item.Refreshes);

    private static IO<Unit> Added(MenuItemCollection items, MenuItem item) =>
        IO.lift(() => items.Add(item)).Bind(_ => Products.Released(() => items.Remove(item)));

    private static IO<(Seq<MenuItem> Items, Seq<IO<Unit>> Refreshes)> Items(Command<Invoke> command, RowScope scope) =>
        command.Switch(
            scope,
            run: static (at, run) => run.Invoke.SwitchPartially(
                (Run: run, Scope: at),
                @default: static (held, _) =>
                    from made in Commanded(held.Run, held.Run.Enabled, held.Scope)
                    from item in Created(made.Command)
                    select (Seq(item), Seq(made.Refresh)),
                popup: static (held, popup) =>
                    from sub in Submenu(held.Run.Face, held.Run.Enabled, trimmed: false, held.Scope)
                    from populated in Populated(sub.Item.Items, popup, Subscriptions.On<EventArgs>(h => sub.Item.Opening += h, h => sub.Item.Opening -= h), held.Scope)
                    select (Seq<MenuItem>(sub.Item), Seq(sub.Refresh))),
            choice: static (at, choice) =>
                from radios in Radios(choice, at)
                from items in radios.TraverseM(static radio => Created(radio.Command)).As()
                select (items, radios.Map(static radio => radio.Refresh)),
            menu: static (at, menu) =>
                from sub in Submenu(menu.Face, menu.Enabled, trimmed: true, at)
                from watched in Filled(sub.Item.Items, menu.Groups, at)
                select (Seq<MenuItem>(sub.Item), watched.Add(sub.Refresh)));

    private static IO<(SubMenuItem Item, IO<Unit> Refresh)> Submenu(CommandFace face, Option<RowRule> gate, bool trimmed, RowScope scope) =>
        from faced in Faced(face, gate, scope)
        from sub in use(() => new SubMenuItem { Text = faced.Command.MenuText, ToolTip = faced.Command.ToolTip, Image = faced.Command.Image, Trim = trimmed, Command = faced.Command })
        select (sub, faced.Refresh);

    private static IO<MenuItem> Created(Command command) =>
        use(IO.lift(command.CreateMenuItem), static held =>
            Products.Each(Seq(IO.lift(() => { held.Command = null; }), IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit));

    private static IO<Unit> Populated(MenuItemCollection items, Invoke.Popup popup, HostEvent<EventArgs> opening, RowScope scope) =>
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from shown in IO.lift(static () => Ref(Option<Owned<Seq<IO<Unit>>>>.None))
        from attached in opening.Attach(_ =>
            from groups in popup.Groups(scope)
            from refreshes in Products.Clear(shown).Bind(_ => Products.Replace(shown, Filled(items, Localized(groups), scope))).Scoped(owned)
            from refreshed in Refreshed(refreshes)
            select refreshed)
        select unit;

    // --- [BUTTONS]
    public static IO<(Control Control, IO<Unit> Refresh)> Button(Command<Invoke> command, IconSlot slot, RowScope scope) =>
        from made in Presented(Localized(command), slot, scope)
        from refresh in Listened(made.Refreshes, scope)
        select (made.Control, refresh);

    public static IO<(Control Control, IO<Unit> Refresh)> Strip(Seq<Seq<Command<Invoke>>> groups, IconSlot slot, RowScope scope) =>
        from clusters in Localized(groups).Filter(static cluster => !cluster.IsEmpty).TraverseM(cluster =>
            from pressed in cluster.TraverseM(command => Presented(command, slot, scope)).As()
            from row in Containers.Frame(static () => new RhinoButtonRow { VerticalContentAlignment = VerticalAlignment.Center })
            from filled in IO.lift(() => pressed.Iter(made => row.Items.Add(made.Control)))
            select (Row: row, Refreshes: pressed.Bind(static made => made.Refreshes))).As()
        from strip in Containers.Frame(() => new StackLayout([.. clusters.Map(static cluster => (StackLayoutItem)cluster.Row)]) {
            Orientation = Orientation.Horizontal,
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Panel).Width,
            VerticalContentAlignment = VerticalAlignment.Center,
        })
        from refresh in Listened(clusters.Bind(static cluster => cluster.Refreshes), scope)
        select ((Control)strip, refresh);

    public static IO<(Control Control, IO<Unit> Refresh)> AddRemove(Command<Invoke>.Run add, Command<Invoke>.Run remove, RowScope scope) =>
        from adding in Commanded((Command<Invoke>.Run)Localized(add), add.Enabled, scope)
        from removing in Commanded((Command<Invoke>.Run)Localized(remove), remove.Enabled, scope)
        from pair in use(static () => new AddRemoveButton())
        from faced in IO.lift(() => { (pair.AddToolTip, pair.RemoveToolTip) = (adding.Command.ToolTip, removing.Command.ToolTip); })
        from added in Products.Changed(() => pair.AddCommand, value => pair.AddCommand = value, adding.Command)
        from removed in Products.Changed(() => pair.RemoveCommand, value => pair.RemoveCommand = value, removing.Command)
        from refresh in Listened(Seq(adding.Refresh, removing.Refresh), scope)
        select ((Control)pair, refresh);

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Presented(Command<Invoke> command, IconSlot slot, RowScope scope) =>
        command.Switch(
            (Slot: slot, Scope: scope),
            run: static (at, run) => run.Invoke.SwitchPartially(
                (Run: run, at.Slot, at.Scope),
                @default: static (held, _) => held.Run.Marked.Match(
                    Some: _ => Toggled(held.Run, held.Slot, held.Scope),
                    None: () => Pushed(held.Run, held.Slot, held.Scope)),
                popup: static (held, popup) => Popped(popup, held.Scope).Bind(menu => Dropped(held.Run.Face, held.Run.Enabled, menu, held.Slot, held.Scope))),
            choice: static (at, choice) => Segmented(choice, at.Slot, at.Scope),
            menu: static (at, menu) => menu.Primary.Match(
                Some: primary => Split(menu, primary, at.Slot, at.Scope),
                None: () =>
                    from opened in Opened(menu.Groups, at.Scope)
                    from dropped in Dropped(menu.Face, menu.Enabled, opened.Menu, at.Slot, at.Scope)
                    select (dropped.Control, opened.Refreshes + dropped.Refreshes)));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Pushed(Command<Invoke>.Run run, IconSlot slot, RowScope scope) =>
        from made in Commanded(run, run.Enabled, scope)
        from control in run.Face.Icon.Match(
            Some: icon =>
                from button in use(() => new ImageButton { MaskImageWithBackgroundColorWhenDisabled = true, ToolTip = made.Command.ToolTip })
                from bound in Products.Changed(() => button.Command, value => button.Command = value, made.Command)
                from themed in Icons.Themed(icon, slot, button, frames => button.Image = frames)
                select (Control)button,
            None: () =>
                from button in use(() => new Button { Text = made.Command.ToolBarText, ToolTip = made.Command.ToolTip })
                from bound in Products.Changed(() => button.Command, value => button.Command = value, made.Command)
                select (Control)button)
        select (control, Seq(made.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Toggled(Command<Invoke>.Run run, IconSlot slot, RowScope scope) =>
        from made in Commanded(run, run.Enabled, scope)
        from strip in use(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.Multiple })
        from segment in Segment(run.Face, made.Command, strip, slot, static command => new ButtonSegmentedItem(command))
        select ((Control)strip, Seq(made.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Segmented(Command<Invoke>.Choice choice, IconSlot slot, RowScope scope) =>
        from strip in use(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.Single })
        from radios in Radios(choice, scope)
        from segments in toSeq(choice.Members.Zip(radios, static (member, radio) => (Member: member, Radio: radio)))
            .TraverseM(pair => Segment(pair.Member.Face, pair.Radio.Command, strip, slot, static command => new ButtonSegmentedItem(command))).As()
        select ((Control)strip, radios.Map(static radio => radio.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Dropped(CommandFace face, Option<RowRule> enabled, ContextMenu menu, IconSlot slot, RowScope scope) =>
        from faced in Faced(face, enabled, scope)
        from strip in use(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.None })
        from segment in Segment(face, faced.Command, strip, slot, command => new MenuSegmentedItem(command) { CanSelect = false, Menu = menu })
        select ((Control)strip, Seq(faced.Refresh));

    private static IO<(Control Control, Seq<IO<Unit>> Refreshes)> Split(Command<Invoke>.Menu menu, Command<Invoke>.Run primary, IconSlot slot, RowScope scope) =>
        from opened in Opened(menu.Groups, scope)
        from made in Commanded(primary, Gate(menu.Enabled, primary.Enabled), scope)
        from faced in Faced(menu.Face, menu.Enabled, scope)
        from strip in use(() => new SegmentedButton { SelectionMode = primary.Marked.Map(static _ => SegmentedSelectionMode.Multiple).IfNone(SegmentedSelectionMode.None) })
        from pressed in Segment(primary.Face, made.Command, strip, slot, static command => new ButtonSegmentedItem(command))
        from dropdown in Segment(menu.Face with { Icon = None }, faced.Command, strip, slot, command => new MenuSegmentedItem(command) { CanSelect = false, Menu = opened.Menu })
        let chosen = menu.Groups.Flatten().Bind(static held => held.Switch(run: static _ => Seq<Command<Invoke>.Run>(), choice: static choice => choice.Members, menu: static _ => Seq<Command<Invoke>.Run>()))
        let shown =
            from marks in chosen.TraverseM(member => member.Marked.Traverse(mark => mark.Holds(scope)).As().Map(holds => (member.Face.Icon, On: holds.IfNone(noneValue: false)))).As()
            from frames in (marks.Find(static held => held.On).Bind(static held => held.Icon) | primary.Face.Icon).Traverse(art => Icons.Frames(art, slot)).As()
            from painted in IO.lift(() => { pressed.Image = frames.ValueUnsafe(); })
            select unit
        from themed in HostTheme.Applied(strip, shown)
        select ((Control)strip, opened.Refreshes + Seq(made.Refresh, faced.Refresh, shown));

    private static IO<TItem> Segment<TItem>(CommandFace face, Command command, SegmentedButton strip, IconSlot slot, Func<Command, TItem> create) where TItem : SegmentedItem =>
        from segment in use(IO.lift(() => create(command)), static held => Products.Each(Seq(IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit))
        from placed in IO.lift(() => strip.Items.Add(segment))
        from themed in face.Icon.Traverse(icon => Icons.Themed(icon, slot, strip, frames => segment.Image = frames)).As()
        select segment;

    // --- [REFRESH]
    public static IO<IO<Unit>> Listened(Seq<IO<Unit>> refreshes, RowScope scope) =>
        from heard in scope.Listen(_ => Refreshed(refreshes))
        from shown in Refreshed(refreshes)
        select Refreshed(refreshes);

    private static IO<Unit> Refreshed(Seq<IO<Unit>> refreshes) => Products.Each(refreshes).Map(unit);

    // --- [FACES]
    private static Seq<Seq<Command<Invoke>>> Localized(Seq<Seq<Command<Invoke>>> groups) =>
        groups.Map(static cluster => cluster.Map(Localized));

    private static Command<Invoke> Localized(Command<Invoke> command) =>
        command.Faced(static (face, invoke) => face with {
            Caption = LocalText.Of(face.Caption).Local,
            Help = face.Help.Map(static help => LocalText.Of(help).Local),
            Shortcut = invoke.Bind(static held => held.Shortcut).Map(static keys => keys.ToShortcutString()) | face.Shortcut,
        });
}
