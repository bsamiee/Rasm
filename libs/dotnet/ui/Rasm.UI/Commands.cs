using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption) {
    public Option<string> Help { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Shortcut { get; init; }
}

public sealed record CommandState(bool Enabled, Option<Check> Mark);

public sealed record CommandHost<TGate, TInvoke>(
    Func<TGate, IO<bool>> Holds,
    Func<TGate, IO<Check>> Marks,
    Func<TInvoke, SKRect, IO<Unit>> Run,
    Func<Seq<Seq<Command<TGate, TInvoke>>>, SKRect, IO<Unit>> Menu)
    where TGate : notnull where TInvoke : notnull;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Command<TGate, TInvoke> where TGate : notnull where TInvoke : notnull {
    private Command(CommandFace face, Option<TGate> enabled) => (Face, Enabled) = (face, enabled);

    public CommandFace Face { get; private init; }
    public Option<TGate> Enabled { get; }

    public Command<TGate, TInvoke> Faced(Func<CommandFace, Option<TInvoke>, CommandFace> face) =>
        Switch(
            face,
            run: static (into, run) => (Command<TGate, TInvoke>)(run with { Face = into(run.Face, Some(run.Invoke)) }),
            choice: static (into, choice) => choice with { Face = into(choice.Face, None), Members = choice.Members.Map(member => member with { Face = into(member.Face, Some(member.Invoke)) }) },
            menu: static (into, menu) => menu with {
                Face = into(menu.Face, None),
                Groups = menu.Groups.Map(group => group.Map(command => command.Faced(into))),
                Primary = menu.Primary.Map(primary => primary with { Face = into(primary.Face, Some(primary.Invoke)) }),
            });

    internal Option<TGate> Mark => Switch(run: static run => run.Marked, choice: static _ => Option<TGate>.None, menu: static _ => Option<TGate>.None);

    public sealed record Run(CommandFace Face, Option<TGate> Enabled, TInvoke Invoke) : Command<TGate, TInvoke>(Face, Enabled) {
        public Option<TGate> Marked { get; init; }
        public Option<TInvoke> Alternate { get; init; }
    }

    public sealed record Choice(CommandFace Face, Option<TGate> Enabled, Seq<Run> Members) : Command<TGate, TInvoke>(Face, Enabled);

    public sealed record Menu(CommandFace Face, Option<TGate> Enabled, Seq<Seq<Command<TGate, TInvoke>>> Groups) : Command<TGate, TInvoke>(Face, Enabled) {
        public Option<Run> Primary { get; init; }
    }
}

public sealed record Stated<TGate, TInvoke>(Command<TGate, TInvoke> Command, CommandState State, Seq<Stated<TGate, TInvoke>> Members) where TGate : notnull where TInvoke : notnull;

public sealed record Toolbar<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> Groups, CommandFace Overflow) where TGate : notnull where TInvoke : notnull {
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;
    public Func<CommandFace, Seq<Node>> Face { get; init; } = Commands.Fitting(Commands.Labelled, Commands.Glyphed);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commands {
    // --- [READ]
    public static IO<Seq<Seq<Stated<TGate, TInvoke>>>> Read<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        groups.TraverseM(group => group.TraverseM(command => Stated(command, host)).As()).As();

    private static IO<Stated<TGate, TInvoke>> Stated<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        from state in State(command, host)
        from members in command.Switch(run: static _ => Seq<Command<TGate, TInvoke>.Run>(), choice: static choice => choice.Members, menu: static menu => menu.Primary.ToSeq())
            .TraverseM(member => Stated(member, host)).As()
        select new Stated<TGate, TInvoke>(command, state, members);

    private static IO<CommandState> State<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        from enabled in command.Enabled.Traverse(host.Holds).As()
        from mark in command.Mark.Traverse(host.Marks).As()
        select new CommandState(enabled.IfNone(noneValue: true), mark);

    // --- [BAR]
    public static Node Bar<TGate, TInvoke>(Seq<Seq<Stated<TGate, TInvoke>>> stated, CommandHost<TGate, TInvoke> host, Toolbar<TGate, TInvoke> toolbar)
        where TGate : notnull where TInvoke : notnull {
        Seq<Seq<Node>> drawn = stated.Map(group => group.Map(held => Drawn(held, host, toolbar.Face, toolbar.Axis, None)));
        return stated.Count > 1
            ? new Node.Choice(toolbar.Axis, toSeq(Range(0, stated.Count)).Map(dropped => (Node)Row(drawn.Take(stated.Count - dropped), stated.Skip(stated.Count - dropped), host, toolbar)))
            : Row(drawn, [], host, toolbar);
    }

    private static Node.Flex Row<TGate, TInvoke>(Seq<Seq<Node>> shown, Seq<Seq<Stated<TGate, TInvoke>>> rest, CommandHost<TGate, TInvoke> host, Toolbar<TGate, TInvoke> toolbar)
        where TGate : notnull where TInvoke : notnull =>
        new(toolbar.Axis,
            shown.Intersperse(Seq<Node>(Divider(toolbar.Axis))).Flatten()
            + (rest.IsEmpty
                ? Seq<Node>()
                : Seq(Divider(toolbar.Axis), Opening(toolbar.Overflow, new CommandState(Enabled: true, Mark: None), rest.Map(static group => group.Map(static held => held.Command)), host, toolbar.Face)))) {
            Role = Role.Toolbar,
            Accessible = new Accessible { Role = AccessRole.Toolbar },
        };

    private static Node Drawn<TGate, TInvoke>(Stated<TGate, TInvoke> stated, CommandHost<TGate, TInvoke> host, Func<CommandFace, Seq<Node>> face, PhysicalAxis axis, Option<AccessRole> within)
        where TGate : notnull where TInvoke : notnull =>
        stated.Command.Switch(
            (Stated: stated, Host: host, Face: face, Axis: axis, Within: within),
            run: static (held, run) => Button(run.Face, held.Stated.State, held.Within, Pressing(run, held.Host), held.Face),
            choice: static (held, choice) => new Node.Flex(held.Axis, held.Stated.Members.Map(member => Drawn(member, held.Host, held.Face, held.Axis, AccessRole.RadioGroup.Owns))) {
                Key = new Key.Named(choice.Face.Name),
                Conditions = Conditioned(held.Stated.State),
                Accessible = new Accessible { Role = AccessRole.RadioGroup, Label = choice.Face.Caption, Help = choice.Face.Help, Shortcut = choice.Face.Shortcut },
            },
            menu: static (held, menu) => (from primary in menu.Primary
                                          from shown in held.Stated.Members.Head.Map(static member => member.State)
                                          where shown.Enabled
                                          select Button(menu.Face, held.Stated.State with { Mark = shown.Mark }, held.Within,
                                              new Part.Press(bounds => held.Host.Run(primary.Invoke, bounds)) { Secondary = bounds => Some(held.Host.Menu(menu.Groups, bounds)) }, held.Face))
                .IfNone(() => Opening(menu.Face, held.Stated.State, menu.Groups, held.Host, held.Face)));

    private static Part.Press Pressing<TGate, TInvoke>(Command<TGate, TInvoke>.Run run, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        new(bounds => host.Run(run.Invoke, bounds)) { Secondary = bounds => run.Alternate.Map(alternate => host.Run(alternate, bounds)) };

    private static Node Opening<TGate, TInvoke>(
        CommandFace shown, CommandState state, Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Func<CommandFace, Seq<Node>> face)
        where TGate : notnull where TInvoke : notnull =>
        Button(shown, state, Some(AccessRole.MenuButton), new Part.Press(bounds => host.Menu(groups, bounds)), face);

    private static Node Button(CommandFace shown, CommandState state, Option<AccessRole> within, Part.Press press, Func<CommandFace, Seq<Node>> face) {
        AccessRole role = within.IfNone(state.Mark.IsSome ? AccessRole.CheckBox : AccessRole.Button);
        return press.Drawn(new Node.Flex(PhysicalAxis.Horizontal, face(shown)) {
            Key = new Key.Named(shown.Name),
            Role = Role.Button,
            Conditions = Conditioned(state),
            Accessible = state.Mark.Fold(new Accessible { Role = role, Label = shown.Caption, Help = shown.Help, Shortcut = shown.Shortcut }, role.Marked),
        });
    }

    private static HashSet<Condition> Conditioned(CommandState state) =>
        toHashSet(Seq(Condition.Disabled).Filter(_ => !state.Enabled) + state.Mark.Bind(static mark => mark.Conditioned).ToSeq());

    private static Node.Box Divider(PhysicalAxis axis) =>
        new() {
            Role = Role.Divider,
            Width = axis.Map(horizontal: Option<Sizing>.None, vertical: Sizing.Grow.Unit),
            Height = axis.Map(horizontal: Sizing.Grow.Unit, vertical: Option<Sizing>.None),
        };

    // --- [FACES]
    public static Seq<Node> Glyphed(CommandFace face) => face.Glyph.Match(Some: static art => Seq(Glyph(art)), None: () => Captioned(face));

    public static Seq<Node> Captioned(CommandFace face) => Seq<Node>(Caption(face));

    public static Seq<Node> Labelled(CommandFace face) => face.Glyph.Map(Glyph).ToSeq().Add(Caption(face));

    public static Func<CommandFace, Seq<Node>> Fitting(params Seq<Func<CommandFace, Seq<Node>>> preferred) =>
        face => Seq<Node>(new Node.Choice(PhysicalAxis.Horizontal, preferred.Map(faced => (Node)new Node.Flex(PhysicalAxis.Horizontal, faced(face)))));

    private static Node Glyph(GlyphSource art) => new Node.Glyph(art) { Align = UnitPoint.Center };

    private static Node.Text Caption(CommandFace face) => new([face.Caption]) { Align = UnitPoint.Center };
}
