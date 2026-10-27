using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandPresentation(string Name, string Caption) {
    public Option<string> Help { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Shortcut { get; init; }
    public Option<string> Badge { get; init; }
}

public sealed record CommandState(HashSet<Condition> Conditions, Option<Check> Check);

public sealed record CommandHost<TGate, TInvoke>(
    Func<TGate, IO<bool>> Enabled,
    Func<TGate, IO<Check>> Checked,
    Func<TInvoke, SKRect, IO<Unit>> Run,
    Func<Seq<Seq<Command<TGate, TInvoke>>>, SKRect, IO<Unit>> Menu)
    where TGate : notnull where TInvoke : notnull;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Command<TGate, TInvoke> where TGate : notnull where TInvoke : notnull {
    private Command(CommandPresentation presentation, Option<TGate> enabled) => (Presentation, Enabled) = (presentation, enabled);

    public CommandPresentation Presentation { get; private init; }
    public Option<TGate> Enabled { get; }

    public Command<TGate, TInvoke> Presented(Func<CommandPresentation, Option<TInvoke>, CommandPresentation> into) =>
        Switch(
            into,
            run: static (into, run) => (Command<TGate, TInvoke>)(run with { Presentation = into(run.Presentation, Some(run.Invoke)) }),
            choice: static (into, choice) => choice with { Presentation = into(choice.Presentation, None), Members = choice.Members.Map(member => member with { Presentation = into(member.Presentation, Some(member.Invoke)) }) },
            menu: static (into, menu) => menu with {
                Presentation = into(menu.Presentation, None),
                Groups = menu.Groups.Map(group => group.Map(command => command.Presented(into))),
                Primary = menu.Primary.Map(primary => primary with { Presentation = into(primary.Presentation, Some(primary.Invoke)) }),
            });

    public sealed record Run(CommandPresentation Presentation, Option<TGate> Enabled, TInvoke Invoke) : Command<TGate, TInvoke>(Presentation, Enabled) {
        public Option<TGate> Checked { get; init; }
        public Option<TGate> Busy { get; init; }
        public Option<TInvoke> Alternate { get; init; }
    }

    public sealed record Choice(CommandPresentation Presentation, Option<TGate> Enabled, Seq<Run> Members) : Command<TGate, TInvoke>(Presentation, Enabled);

    public sealed record Menu(CommandPresentation Presentation, Option<TGate> Enabled, Seq<Seq<Command<TGate, TInvoke>>> Groups) : Command<TGate, TInvoke>(Presentation, Enabled) {
        public Option<Run> Primary { get; init; }
    }
}

public sealed record Toolbar<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> Groups, CommandPresentation Overflow) where TGate : notnull where TInvoke : notnull {
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;
    public Func<CommandPresentation, CommandState, Seq<Node>> Content { get; init; } = Commands.Fitting(Commands.IconAndLabel, Commands.IconOnly);

    public IO<Node> Drawn(CommandHost<TGate, TInvoke> host) =>
        Groups.TraverseM(group => group.TraverseM(command => host.Drawn(command, Content, Axis, None)).As()).As().Map(drawn => drawn.Count > 1
            ? (Node)new Node.Choice(Axis, toSeq(Range(0, drawn.Count)).Map(dropped => (Node)host.Row(drawn.Take(drawn.Count - dropped), Groups.Skip(drawn.Count - dropped), this)))
            : host.Row(drawn, [], this));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commands {
    // --- [MENU]
    private static readonly Key Items = new Key.Named("items");
    private static readonly Key Submenu = new Key.Named("menu");

    public static IO<Node.Layer> Menu<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Seq<Key> path, Option<Seq<Key>> anchor, IO<Unit> dismissed)
        where TGate : notnull where TInvoke : notnull =>
        host.Listed(groups, path, dismissed).Map(listed => new Node.Layer(anchor, listed) {
            Key = path.Last,
            Dismissed = Some(dismissed),
            Accessible = new Accessible { Role = AccessRole.Menu },
        });

    // --- [PRESENTATIONS]
    public static Seq<Node> IconOnly(CommandPresentation presentation, CommandState state) =>
        presentation.Glyph.Match(Some: art => Seq(Glyph(art, state)), None: () => LabelOnly(presentation, state));

    public static Seq<Node> LabelOnly(CommandPresentation presentation, CommandState _) => Seq<Node>(Caption(presentation));

    public static Seq<Node> IconAndLabel(CommandPresentation presentation, CommandState state) => presentation.Glyph.Map(art => Glyph(art, state)).ToSeq().Add(Caption(presentation));

    public static Func<CommandPresentation, CommandState, Seq<Node>> Fitting(params Seq<Func<CommandPresentation, CommandState, Seq<Node>>> preferred) =>
        (presentation, state) => Seq<Node>(new Node.Choice(PhysicalAxis.Horizontal, preferred.Map(faced => (Node)new Node.Flex(PhysicalAxis.Horizontal, faced(presentation, state)))));

    private static Node Glyph(GlyphSource art, CommandState state) =>
        state.Conditions.Contains(Condition.Busy) ? new Node.Box { Role = Role.Spinner, Align = UnitPoint.Center } : new Node.Glyph(art) { Align = UnitPoint.Center };

    private static Node.Text Caption(CommandPresentation presentation) => new([presentation.Caption]) { Align = UnitPoint.Center };

    // --- [HOSTED]
    extension<TGate, TInvoke>(CommandHost<TGate, TInvoke> host) where TGate : notnull where TInvoke : notnull {
        internal IO<Node> Drawn(Command<TGate, TInvoke> command, Func<CommandPresentation, CommandState, Seq<Node>> content, PhysicalAxis axis, Option<AccessRole> within) =>
            command.Switch(
                (Host: host, Content: content, Axis: axis, Within: within),
                run: static (held, run) => held.Host.State(run.Enabled, run.Busy, run.Checked)
                    .Map(state => Button(run.Presentation, state, held.Within, Role.Button, Some(Pressing(run, held.Host.Run)), held.Content(run.Presentation, state))),
                choice: static (held, choice) =>
                    from state in held.Host.State(choice.Enabled, None, None)
                    from members in choice.Members.TraverseM(member => held.Host.Drawn(member, held.Content, held.Axis, AccessRole.RadioGroup.Owns)).As()
                    select (Node)new Node.Flex(held.Axis, members) {
                        Key = new Key.Named(choice.Presentation.Name),
                        Conditions = state.Conditions,
                        Accessible = new Accessible { Role = AccessRole.RadioGroup, Label = choice.Presentation.Caption, Help = choice.Presentation.Help, Shortcut = choice.Presentation.Shortcut },
                    },
                menu: static (held, menu) =>
                    from state in held.Host.State(menu.Enabled, None, None)
                    from split in held.Host.Split(menu, state)
                    select (from primary in menu.Primary
                            from shown in split
                            select Button(menu.Presentation, shown, held.Within, Role.Button,
                                Some(new Part.Press(bounds => held.Host.Run(primary.Invoke, bounds)) { Secondary = bounds => Some(held.Host.Menu(menu.Groups, bounds)) }),
                                held.Content(menu.Presentation, shown)))
                        .IfNone(() => held.Host.MenuButton(menu.Presentation, state, menu.Groups, held.Content)));

        internal Node.Flex Row(Seq<Seq<Node>> shown, Seq<Seq<Command<TGate, TInvoke>>> rest, Toolbar<TGate, TInvoke> toolbar) =>
            new(toolbar.Axis,
                shown.Intersperse(Seq<Node>(Divider(toolbar.Axis))).Flatten()
                + (rest.IsEmpty ? [] : Seq(Divider(toolbar.Axis), host.MenuButton(toolbar.Overflow, new CommandState([], None), rest, toolbar.Content)))) {
                Role = Role.Toolbar,
                Accessible = new Accessible { Role = AccessRole.Toolbar },
            };

        internal IO<Node.Flex> Listed(Seq<Seq<Command<TGate, TInvoke>>> groups, Seq<Key> layer, IO<Unit> dismissed) =>
            groups.TraverseM(group => group.TraverseM(command => host.Rows(command, layer.Add(Items), dismissed)).As().Map(static rows => rows.Flatten())).As()
                .Map(static rows => new Node.Flex(PhysicalAxis.Vertical, rows.Intersperse(Seq<Node>(Divider(PhysicalAxis.Vertical))).Flatten()) { Key = Items, Role = Role.Menu });

        private IO<CommandState> State(Option<TGate> enabled, Option<TGate> busy, Option<TGate> check) =>
            from open in enabled.Traverse(host.Enabled).As()
            from working in busy.Traverse(host.Enabled).As()
            from mark in check.Traverse(host.Checked).As()
            select new CommandState(toHashSet(Seq(open.Filter(static holds => !holds).Map(static _ => Condition.Disabled), working.Filter(static holds => holds).Map(static _ => Condition.Busy), mark.Bind(static held => held.Conditioned)).Somes()), mark);

        private IO<Option<CommandState>> Split(Command<TGate, TInvoke>.Menu menu, CommandState state) =>
            menu.Primary.Traverse(primary => host.State(primary.Enabled, primary.Busy, primary.Checked)).As()
                .Map(primary => primary.Filter(static shown => !shown.Conditions.Contains(Condition.Disabled)).Map(shown => new CommandState(state.Conditions + shown.Conditions, shown.Check)));

        private Node MenuButton(CommandPresentation shown, CommandState state, Seq<Seq<Command<TGate, TInvoke>>> groups, Func<CommandPresentation, CommandState, Seq<Node>> content) =>
            Button(shown, state, Some(AccessRole.MenuButton), Role.Button, Some(new Part.Press(bounds => host.Menu(groups, bounds))), content(shown, state));

        private IO<Seq<Node>> Rows(Command<TGate, TInvoke> command, Seq<Key> within, IO<Unit> dismissed) =>
            command.Switch(
                (Host: host, Within: within, Dismissed: dismissed, Ran: (Func<TInvoke, SKRect, IO<Unit>>)((invoke, bounds) => host.Run(invoke, bounds).Bind(_ => dismissed))),
                run: static (held, run) => held.Host.State(run.Enabled, run.Busy, run.Checked).Map(state => Seq(Item(run.Presentation, state, Some(Pressing(run, held.Ran))))),
                choice: static (held, choice) =>
                    from state in held.Host.State(choice.Enabled, None, None)
                    from rows in choice.Members.TraverseM(member => held.Host.Rows(member, held.Within, held.Dismissed)).As()
                    select rows.Flatten().Map(row => row with { Conditions = row.Conditions + state.Conditions }),
                menu: static (held, menu) => held.Within.Add(new Key.Named(menu.Presentation.Name)) switch {
                    var row =>
                        from state in held.Host.State(menu.Enabled, None, None)
                        from split in held.Host.Split(menu, state)
                        from listed in held.Host.Listed(menu.Groups, row.Add(Submenu), held.Dismissed)
                        select Seq(Item(menu.Presentation, split.IfNone(state), split.Bind(_ => menu.Primary).Map(primary => Pressing(primary, held.Ran)),
                            [new Node.Layer(Some(row), listed) {
                                Key = Submenu,
                                Side = new UnitPoint(Fraction.MaxValue, Fraction.MinValue),
                                Hover = Some(Duration.Zero),
                                Accessible = new Accessible { Role = AccessRole.Menu },
                            }])),
                });
    }

    // --- [FACES]
    private static Part.Press Pressing<TGate, TInvoke>(Command<TGate, TInvoke>.Run run, Func<TInvoke, SKRect, IO<Unit>> ran) where TGate : notnull where TInvoke : notnull =>
        new(bounds => ran(run.Invoke, bounds)) { Secondary = bounds => run.Alternate.Map(alternate => ran(alternate, bounds)) };

    private static Node Button(CommandPresentation shown, CommandState state, Option<AccessRole> within, Role look, Option<Part.Press> press, Seq<Node> content, Seq<Node> above = default) {
        Seq<Node> pinned = shown.Badge.Map(static badge => (Node)new Node.Pin(new Vector2(1f, 0f), new Node.Text([badge]) { Role = Role.Badge, Align = UnitPoint.Center }) { Align = UnitPoint.Center }).ToSeq() + above;
        Node framed = (pinned.IsEmpty ? new Node.Flex(PhysicalAxis.Horizontal, content) : (Node)new Node.Overlay(new Node.Flex(PhysicalAxis.Horizontal, content), pinned)) with {
            Key = new Key.Named(shown.Name),
            Role = look,
            Conditions = state.Conditions,
            Accessible = new Accessible {
                Role = within.IfNone(state.Check.IsSome ? AccessRole.CheckBox : AccessRole.Button),
                Label = shown.Badge.Filter(static badge => badge.Length > 0).Map(badge => $"{shown.Caption} {badge}").IfNone(shown.Caption),
                Help = shown.Help,
                Shortcut = shown.Shortcut,
            },
        };
        return press.Match(Some: part => part.Drawn(framed), None: () => framed);
    }

    private static Node Item(CommandPresentation shown, CommandState state, Option<Part.Press> press, Seq<Node> above = default) =>
        Button(shown, state, Some(AccessRole.MenuItem), Role.ListRow, press,
            IconAndLabel(shown, state) + shown.Shortcut.ToSeq().Bind(static shortcut => Seq<Node>(new Node.Box { Width = Sizing.Grow.Unit }, new Node.Text([shortcut]) { Role = Role.Keycap, Align = UnitPoint.Center })),
            above) with {
            Width = Sizing.Grow.Unit,
        };

    private static Node.Box Divider(PhysicalAxis axis) =>
        new() {
            Role = Role.Divider,
            Width = axis.Map(horizontal: Option<Sizing>.None, vertical: Sizing.Grow.Unit),
            Height = axis.Map(horizontal: Sizing.Grow.Unit, vertical: Option<Sizing>.None),
        };
}
