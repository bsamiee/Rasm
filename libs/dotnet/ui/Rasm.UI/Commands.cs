using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption, Option<string> Help, Option<GlyphSource> Glyph) {
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Stated<TGate, TInvoke> where TGate : notnull where TInvoke : notnull {
    private Stated(CommandState state) => State = state;

    public CommandState State { get; }

    public Command<TGate, TInvoke> Command => Switch(run: static run => (Command<TGate, TInvoke>)run.Source, choice: static choice => choice.Source, menu: static menu => menu.Source);

    public sealed record Run(Command<TGate, TInvoke>.Run Source, CommandState State) : Stated<TGate, TInvoke>(State);

    public sealed record Choice(Command<TGate, TInvoke>.Choice Source, CommandState State, Seq<Run> Members) : Stated<TGate, TInvoke>(State);

    public sealed record Menu(Command<TGate, TInvoke>.Menu Source, CommandState State, Option<Run> Primary) : Stated<TGate, TInvoke>(State);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commands {
    // --- [READ]
    public static IO<Seq<Seq<Stated<TGate, TInvoke>>>> Read<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        groups.TraverseM(group => group.TraverseM(command => Stated(command, host)).As()).As();

    private static IO<Stated<TGate, TInvoke>> Stated<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        State(command, host).Bind(state => command.Switch(
            (State: state, Host: host),
            run: static (held, run) => IO.pure<Stated<TGate, TInvoke>>(new Stated<TGate, TInvoke>.Run(run, held.State)),
            choice: static (held, choice) => choice.Members.TraverseM(member => Ran(member, held.Host)).As()
                .Map(members => (Stated<TGate, TInvoke>)new Stated<TGate, TInvoke>.Choice(choice, held.State, members)),
            menu: static (held, menu) => menu.Primary.Traverse(primary => Ran(primary, held.Host)).As()
                .Map(primary => (Stated<TGate, TInvoke>)new Stated<TGate, TInvoke>.Menu(menu, held.State, primary))));

    private static IO<Stated<TGate, TInvoke>.Run> Ran<TGate, TInvoke>(Command<TGate, TInvoke>.Run run, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        State(run, host).Map(state => new Stated<TGate, TInvoke>.Run(run, state));

    private static IO<CommandState> State<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        from enabled in command.Enabled.Traverse(host.Holds).As()
        from mark in command.Mark.Traverse(host.Marks).As()
        select new CommandState(enabled.IfNone(noneValue: true), mark);

    // --- [BAR]
    public static Node Bar<TGate, TInvoke>(Seq<Seq<Stated<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, CommandFace overflow, Orientation axis, Func<CommandFace, Seq<Node>> face)
        where TGate : notnull where TInvoke : notnull =>
        groups.Map(group => group.Map(stated => Drawn(stated, host, face, axis, None))) switch {
            var drawn => groups.Count > 1
                ? new Node.Choice(axis, toSeq(Range(0, groups.Count)).Map(dropped =>
                    (Node)Row(drawn.Take(groups.Count - dropped), groups.Skip(groups.Count - dropped), overflow, host, face, axis)))
                : Row(drawn, [], overflow, host, face, axis),
        };

    private static Node.Flex Row<TGate, TInvoke>(
        Seq<Seq<Node>> shown, Seq<Seq<Stated<TGate, TInvoke>>> rest, CommandFace overflow, CommandHost<TGate, TInvoke> host, Func<CommandFace, Seq<Node>> face, Orientation axis)
        where TGate : notnull where TInvoke : notnull =>
        new(axis,
            shown.Intersperse(Seq<Node>(Divider(axis))).Flatten()
            + (rest.IsEmpty ? Seq<Node>() : Seq(Divider(axis), Opening(overflow, new CommandState(Enabled: true, Mark: None), rest.Map(static group => group.Map(static stated => stated.Command)), host, face)))) {
            Role = Role.Toolbar,
            Accessible = new Accessible { Role = AccessRole.Toolbar },
        };

    private static Node Drawn<TGate, TInvoke>(Stated<TGate, TInvoke> stated, CommandHost<TGate, TInvoke> host, Func<CommandFace, Seq<Node>> face, Orientation axis, Option<AccessRole> within)
        where TGate : notnull where TInvoke : notnull =>
        stated.Switch(
            (Host: host, Face: face, Axis: axis, Within: within),
            run: static (held, run) => Button(run.Source.Face, run.State, held.Within, Pressing(run.Source, held.Host), held.Face),
            choice: static (held, choice) => new Node.Flex(held.Axis, choice.Members.Map(member => Drawn(member, held.Host, held.Face, held.Axis, AccessRole.RadioGroup.Owns))) {
                Key = new Key.Named(choice.Source.Face.Name),
                Conditions = Conditioned(choice.State),
                Accessible = new Accessible { Role = AccessRole.RadioGroup, Label = choice.Source.Face.Caption, Help = choice.Source.Face.Help, Shortcut = choice.Source.Face.Shortcut },
            },
            menu: static (held, menu) => menu.Primary.Filter(static primary => primary.State.Enabled).Match(
                Some: primary => Button(menu.Source.Face, menu.State with { Mark = primary.State.Mark }, held.Within,
                    new Part.Press(bounds => held.Host.Run(primary.Source.Invoke, bounds)) { Secondary = Some<Func<SKRect, IO<Unit>>>(bounds => held.Host.Menu(menu.Source.Groups, bounds)) }, held.Face),
                None: () => Opening(menu.Source.Face, menu.State, menu.Source.Groups, held.Host, held.Face)));

    private static Part.Press Pressing<TGate, TInvoke>(Command<TGate, TInvoke>.Run run, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        new(bounds => host.Run(run.Invoke, bounds)) { Secondary = run.Alternate.Map(alternate => (Func<SKRect, IO<Unit>>)(bounds => host.Run(alternate, bounds))) };

    private static Node Opening<TGate, TInvoke>(
        CommandFace shown, CommandState state, Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Func<CommandFace, Seq<Node>> face)
        where TGate : notnull where TInvoke : notnull =>
        Button(shown, state, Some(AccessRole.MenuButton), new Part.Press(bounds => host.Menu(groups, bounds)), face);

    private static Node Button(CommandFace shown, CommandState state, Option<AccessRole> within, Part.Press press, Func<CommandFace, Seq<Node>> face) =>
        within.IfNone(state.Mark.IsSome ? AccessRole.CheckBox : AccessRole.Button) switch {
            var role => press.Drawn(new Node.Flex(Orientation.Horizontal, face(shown)) {
                Key = new Key.Named(shown.Name),
                Role = Role.Button,
                Conditions = Conditioned(state),
                Accessible = state.Mark.Fold(new Accessible { Role = role, Label = shown.Caption, Help = shown.Help, Shortcut = shown.Shortcut }, role.Marked),
            }),
        };

    private static LanguageExt.HashSet<Condition> Conditioned(CommandState state) =>
        toHashSet(Seq(Condition.Disabled).Filter(_ => !state.Enabled) + state.Mark.Bind(static mark => mark.Map(off: Option<Condition>.None, on: Condition.Selected, mixed: Condition.Mixed)).ToSeq());

    private static Node.Box Divider(Orientation axis) =>
        new() {
            Role = Role.Divider,
            Width = axis.Map(horizontal: Option<Sizing>.None, vertical: new Sizing.Grow(Factor.Create(1f))),
            Height = axis.Map(horizontal: new Sizing.Grow(Factor.Create(1f)), vertical: Option<Sizing>.None),
        };

    // --- [FACES]
    public static Seq<Node> Glyphed(CommandFace face) => face.Glyph.Match(Some: static art => Seq(Glyph(art)), None: () => Captioned(face));

    public static Seq<Node> Captioned(CommandFace face) => Seq<Node>(Caption(face));

    public static Seq<Node> Labelled(CommandFace face) => face.Glyph.Map(Glyph).ToSeq().Add(Caption(face));

    public static Func<CommandFace, Seq<Node>> Fitting(params Seq<Func<CommandFace, Seq<Node>>> preferred) =>
        face => Seq<Node>(new Node.Choice(Orientation.Horizontal, preferred.Map(faced => (Node)new Node.Flex(Orientation.Horizontal, faced(face)))));

    private static Node Glyph(GlyphSource art) => new Node.Glyph(art) { Align = UnitPoint.Center };

    private static Node.Text Caption(CommandFace face) => new([face.Caption]) { Align = UnitPoint.Center };
}
