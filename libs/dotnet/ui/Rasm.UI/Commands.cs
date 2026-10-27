using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Badge(Option<uint> Count);

public sealed record CommandPresentation(string Name, string Caption) {
    public Option<string> Help { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Shortcut { get; init; }
    public Option<Badge> Badge { get; init; }
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

    internal Option<TGate> Gate(Func<Run, Option<TGate>> gate) =>
        Switch(gate, run: static (gate, run) => gate(run), choice: static (_, _) => Option<TGate>.None, menu: static (_, _) => Option<TGate>.None);

    internal Seq<Seq<Command<TGate, TInvoke>>> Nested => Switch(run: static _ => Seq<Seq<Command<TGate, TInvoke>>>(), choice: static _ => [], menu: static menu => menu.Groups);

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

public sealed record ResolvedCommand<TGate, TInvoke>(Command<TGate, TInvoke> Command, CommandState State, Seq<ResolvedCommand<TGate, TInvoke>> Members) where TGate : notnull where TInvoke : notnull {
    public Seq<Seq<ResolvedCommand<TGate, TInvoke>>> Groups { get; init; }
}

public sealed record Toolbar<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> Groups, CommandPresentation Overflow) where TGate : notnull where TInvoke : notnull {
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;
    public Func<CommandPresentation, CommandState, Seq<Node>> Content { get; init; } = Commands.Fitting(Commands.IconAndLabel, Commands.IconOnly);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commands {
    // --- [READ]
    public static IO<Seq<Seq<ResolvedCommand<TGate, TInvoke>>>> Read<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        groups.TraverseM(group => group.TraverseM(command => Stated(command, host)).As()).As();

    private static IO<ResolvedCommand<TGate, TInvoke>> Stated<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        from state in State(command, host)
        from members in command.Switch(run: static _ => Seq<Command<TGate, TInvoke>.Run>(), choice: static choice => choice.Members, menu: static menu => menu.Primary.ToSeq())
            .TraverseM(member => Stated(member, host)).As()
        from groups in Read(command.Nested, host)
        select new ResolvedCommand<TGate, TInvoke>(command, state, members) { Groups = groups };

    private static IO<CommandState> State<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        from enabled in command.Enabled.Traverse(host.Enabled).As()
        from busy in command.Gate(static run => run.Busy).Traverse(host.Enabled).As()
        from check in command.Gate(static run => run.Checked).Traverse(host.Checked).As()
        select new CommandState(
            toHashSet(Seq(Condition.Disabled).Filter(_ => enabled.Exists(static holds => !holds)) + Seq(Condition.Busy).Filter(_ => busy.Exists(static holds => holds)) + check.Bind(static mark => mark.Conditioned).ToSeq()),
            check);

    // --- [BAR]
    public static Node Bar<TGate, TInvoke>(Seq<Seq<ResolvedCommand<TGate, TInvoke>>> stated, CommandHost<TGate, TInvoke> host, Toolbar<TGate, TInvoke> toolbar)
        where TGate : notnull where TInvoke : notnull {
        Seq<Seq<Node>> drawn = stated.Map(group => group.Map(held => Drawn(held, host, toolbar.Content, toolbar.Axis, None)));
        return stated.Count > 1
            ? new Node.Choice(toolbar.Axis, toSeq(Range(0, stated.Count)).Map(dropped => (Node)Row(drawn.Take(stated.Count - dropped), stated.Skip(stated.Count - dropped), host, toolbar)))
            : Row(drawn, [], host, toolbar);
    }

    private static Node.Flex Row<TGate, TInvoke>(Seq<Seq<Node>> shown, Seq<Seq<ResolvedCommand<TGate, TInvoke>>> rest, CommandHost<TGate, TInvoke> host, Toolbar<TGate, TInvoke> toolbar)
        where TGate : notnull where TInvoke : notnull =>
        new(toolbar.Axis,
            shown.Intersperse(Seq<Node>(Divider(toolbar.Axis))).Flatten()
            + (rest.IsEmpty
                ? Seq<Node>()
                : Seq(Divider(toolbar.Axis), MenuButton(toolbar.Overflow, new CommandState([], None), rest.Map(static group => group.Map(static held => held.Command)), host, toolbar.Content)))) {
            Role = Role.Toolbar,
            Accessible = new Accessible { Role = AccessRole.Toolbar },
        };

    private static Node Drawn<TGate, TInvoke>(
        ResolvedCommand<TGate, TInvoke> stated, CommandHost<TGate, TInvoke> host, Func<CommandPresentation, CommandState, Seq<Node>> content, PhysicalAxis axis, Option<AccessRole> within)
        where TGate : notnull where TInvoke : notnull =>
        stated.Command.Switch(
            (Stated: stated, Host: host, Content: content, Axis: axis, Within: within),
            run: static (held, run) => Button(run.Presentation, held.Stated.State, held.Within, Role.Button, Some(PressPart(run, held.Host.Run)), held.Content(run.Presentation, held.Stated.State), []),
            choice: static (held, choice) => new Node.Flex(held.Axis, held.Stated.Members.Map(member => Drawn(member, held.Host, held.Content, held.Axis, AccessRole.RadioGroup.Owns))) {
                Key = new Key.Named(choice.Presentation.Name),
                Conditions = held.Stated.State.Conditions,
                Accessible = new Accessible { Role = AccessRole.RadioGroup, Label = choice.Presentation.Caption, Help = choice.Presentation.Help, Shortcut = choice.Presentation.Shortcut },
            },
            menu: static (held, menu) => (from primary in menu.Primary
                                          from state in Split(held.Stated)
                                          select Button(menu.Presentation, state, held.Within, Role.Button,
                                              Some(new Part.Press(bounds => held.Host.Run(primary.Invoke, bounds)) { Secondary = bounds => Some(held.Host.Menu(menu.Groups, bounds)) }),
                                              held.Content(menu.Presentation, state), []))
                .IfNone(() => MenuButton(menu.Presentation, held.Stated.State, menu.Groups, held.Host, held.Content)));

    private static Option<CommandState> Split<TGate, TInvoke>(ResolvedCommand<TGate, TInvoke> stated)
        where TGate : notnull where TInvoke : notnull =>
        from shown in stated.Members.Head.Map(static primary => primary.State)
        where !shown.Conditions.Contains(Condition.Disabled)
        select new CommandState(stated.State.Conditions + shown.Conditions, shown.Check);

    private static Part.Press PressPart<TGate, TInvoke>(Command<TGate, TInvoke>.Run run, Func<TInvoke, SKRect, IO<Unit>> ran)
        where TGate : notnull where TInvoke : notnull =>
        new(bounds => ran(run.Invoke, bounds)) { Secondary = bounds => run.Alternate.Map(alternate => ran(alternate, bounds)) };

    private static Node MenuButton<TGate, TInvoke>(
        CommandPresentation shown, CommandState state, Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Func<CommandPresentation, CommandState, Seq<Node>> content)
        where TGate : notnull where TInvoke : notnull =>
        Button(shown, state, Some(AccessRole.MenuButton), Role.Button, Some(new Part.Press(bounds => host.Menu(groups, bounds))), content(shown, state), []);

    private static Node Button(CommandPresentation shown, CommandState state, Option<AccessRole> within, Role look, Option<Part.Press> press, Seq<Node> content, Seq<Node> above) {
        AccessRole role = within.IfNone(state.Check.IsSome ? AccessRole.CheckBox : AccessRole.Button);
        Seq<Node> pinned = shown.Badge.Map(static badge => (Node)new Node.Pin(new Vector2(1f, 0f), Badged(badge)) { Align = UnitPoint.Center }).ToSeq() + above;
        Node framed = (pinned.IsEmpty ? new Node.Flex(PhysicalAxis.Horizontal, content) : (Node)new Node.Overlay(new Node.Flex(PhysicalAxis.Horizontal, content), pinned)) with {
            Key = new Key.Named(shown.Name),
            Role = look,
            Conditions = state.Conditions,
            Accessible = state.Check.Fold(new Accessible { Role = role, Label = Counted(shown), Help = shown.Help, Shortcut = shown.Shortcut }, role.WithCheck),
        };
        return press.Match(Some: part => part.Drawn(framed), None: () => framed);
    }

    private static Node Badged(Badge badge) =>
        badge.Count.Match(
            Some: static count => (Node)new Node.Text([count.ToString(CultureInfo.InvariantCulture)]) { Role = Role.Badge, Align = UnitPoint.Center },
            None: static () => new Node.Box { Role = Role.Badge });

    private static string Counted(CommandPresentation shown) =>
        shown.Badge.Bind(static badge => badge.Count).Match(Some: count => $"{shown.Caption} {count}", None: () => shown.Caption);

    private static Node.Box Divider(PhysicalAxis axis) =>
        new() {
            Role = Role.Divider,
            Width = axis.Map(horizontal: Option<Sizing>.None, vertical: Sizing.Grow.Unit),
            Height = axis.Map(horizontal: Sizing.Grow.Unit, vertical: Option<Sizing>.None),
        };

    // --- [MENU]
    private static readonly Key Items = new Key.Named("items");
    private static readonly Key Submenu = new Key.Named("menu");

    public static Node.Layer Menu<TGate, TInvoke>(Seq<Seq<ResolvedCommand<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Seq<Key> path, Option<Seq<Key>> anchor, IO<Unit> dismissed)
        where TGate : notnull where TInvoke : notnull =>
        new(anchor, Listed(groups, host, path, dismissed)) {
            Key = path.Last,
            Dismissed = Some(dismissed),
            Accessible = new Accessible { Role = AccessRole.Menu },
        };

    private static Node.Flex Listed<TGate, TInvoke>(Seq<Seq<ResolvedCommand<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Seq<Key> layer, IO<Unit> dismissed)
        where TGate : notnull where TInvoke : notnull =>
        new(PhysicalAxis.Vertical, groups.Map(group => group.Bind(held => Rows(held, host, layer.Add(Items), dismissed))).Intersperse(Seq<Node>(Divider(PhysicalAxis.Vertical))).Flatten()) {
            Key = Items,
            Role = Role.Menu,
        };

    private static Seq<Node> Rows<TGate, TInvoke>(ResolvedCommand<TGate, TInvoke> stated, CommandHost<TGate, TInvoke> host, Seq<Key> within, IO<Unit> dismissed)
        where TGate : notnull where TInvoke : notnull {
        Func<TInvoke, SKRect, IO<Unit>> ran = (invoke, bounds) => host.Run(invoke, bounds).Bind(_ => dismissed);
        return stated.Command.Switch(
            (Stated: stated, Host: host, Within: within, Dismissed: dismissed, Ran: ran),
            run: static (held, run) => Seq(Item(run.Presentation, held.Stated.State, Some(PressPart(run, held.Ran)), [])),
            choice: static (held, _) => held.Stated.Members.Bind(member =>
                Rows(member with { State = member.State with { Conditions = member.State.Conditions + held.Stated.State.Conditions } }, held.Host, held.Within, held.Dismissed)),
            menu: static (held, menu) => {
                Seq<Key> row = held.Within.Add(new Key.Named(menu.Presentation.Name));
                Option<CommandState> split = Split(held.Stated);
                return Seq(Item(menu.Presentation, split.IfNone(held.Stated.State), split.Bind(_ => menu.Primary).Map(primary => PressPart(primary, held.Ran)),
                    [new Node.Layer(Some(row), Listed(held.Stated.Groups, held.Host, row.Add(Submenu), held.Dismissed)) {
                        Key = Submenu,
                        Side = new UnitPoint(Fraction.MaxValue, Fraction.MinValue),
                        Hover = Some(Duration.Zero),
                        Accessible = new Accessible { Role = AccessRole.Menu },
                    }]));
            });
    }

    private static Node Item(CommandPresentation shown, CommandState state, Option<Part.Press> press, Seq<Node> above) =>
        Button(shown, state, Some(AccessRole.MenuItem), Role.ListRow, press,
            IconAndLabel(shown, state) + shown.Shortcut.ToSeq().Bind(static shortcut => Seq<Node>(new Node.Box { Width = Sizing.Grow.Unit }, new Node.Text([shortcut]) { Role = Role.Keycap, Align = UnitPoint.Center })),
            above) with {
            Width = Sizing.Grow.Unit,
        };

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
}
