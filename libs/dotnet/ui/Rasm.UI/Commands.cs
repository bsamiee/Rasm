using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public enum FaceDisplay {
    Glyph,
    Caption,
    GlyphAndCaption,
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandFace(string Name, string Caption, Option<string> Help, Option<Either<GlyphRole, GlyphSource>> Glyph);

public sealed record CommandState(bool Enabled, Option<bool> Marked);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Command<TGate, TInvoke> where TGate : notnull where TInvoke : notnull {
    private Command(CommandFace face, Option<TGate> enabled) => (Face, Enabled) = (face, enabled);

    public CommandFace Face { get; private init; }
    public Option<TGate> Enabled { get; }

    public Command<TGate, TInvoke> Faced(Func<CommandFace, CommandFace> face) =>
        Switch(
            face,
            run: static (into, run) => (Command<TGate, TInvoke>)(run with { Face = into(run.Face) }),
            choice: static (into, choice) => choice with { Face = into(choice.Face), Members = choice.Members.Map(member => member with { Face = into(member.Face) }) },
            menu: static (into, menu) => menu with { Face = into(menu.Face), Groups = menu.Groups.Map(group => group.Map(command => command.Faced(into))) });

    internal Seq<Command<TGate, TInvoke>> Nested =>
        Switch(
            run: static _ => Seq<Command<TGate, TInvoke>>(),
            choice: static choice => choice.Members.Map(static member => (Command<TGate, TInvoke>)member),
            menu: static menu => menu.Groups.Flatten());

    internal Option<TGate> Mark => Switch(run: static run => run.Marked, choice: static _ => Option<TGate>.None, menu: static _ => Option<TGate>.None);

    public sealed record Run(CommandFace Face, Option<TGate> Enabled, TInvoke Invoke) : Command<TGate, TInvoke>(Face, Enabled) {
        public Option<TGate> Marked { get; init; }
        public Option<TInvoke> Alternate { get; init; }
    }

    public sealed record Choice(CommandFace Face, Option<TGate> Enabled, Seq<Run> Members) : Command<TGate, TInvoke>(Face, Enabled);

    public sealed record Menu(CommandFace Face, Option<TGate> Enabled, Seq<Seq<Command<TGate, TInvoke>>> Groups) : Command<TGate, TInvoke>(Face, Enabled) {
        public Option<(TInvoke Invoke, Option<TInvoke> Alternate)> Primary { get; init; }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commands {
    private sealed record Realized<TGate, TInvoke>(
        Func<TGate, IO<bool>> Holds, FaceDisplay Display, Func<TInvoke, SKRect, IO<Unit>> Run, Func<Seq<Seq<Command<TGate, TInvoke>>>, SKRect, IO<Unit>> Menu)
        where TGate : notnull where TInvoke : notnull;

    // --- [READ]
    public static IO<HashMap<string, CommandState>> Read<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> groups, Func<TGate, IO<bool>> holds)
        where TGate : notnull where TInvoke : notnull =>
        Stated(groups.Flatten(), parent: true, holds).Map(static rows => toHashMap(rows));

    private static IO<Seq<(string Name, CommandState State)>> Stated<TGate, TInvoke>(Seq<Command<TGate, TInvoke>> commands, bool parent, Func<TGate, IO<bool>> holds)
        where TGate : notnull where TInvoke : notnull =>
        commands.TraverseM(command =>
                from state in State(command, parent, holds)
                from nested in Stated(command.Nested, state.Enabled, holds)
                select (command.Face.Name, state).Cons(nested))
            .As()
            .Map(static rows => rows.Flatten());

    private static IO<CommandState> State<TGate, TInvoke>(Command<TGate, TInvoke> command, bool parent, Func<TGate, IO<bool>> holds)
        where TGate : notnull where TInvoke : notnull =>
        from enabled in parent ? command.Enabled.Map(holds).IfNone(IO.pure(value: true)) : IO.pure(value: false)
        from marked in command.Mark.Traverse(holds).As()
        select new CommandState(enabled, marked);

    // --- [BAR]
    public static IO<Node> Bar<TGate, TInvoke>(
        Seq<Seq<Command<TGate, TInvoke>>> groups, Func<TGate, IO<bool>> holds, CommandFace overflow, FaceDisplay display,
        Func<TInvoke, SKRect, IO<Unit>> run, Func<Seq<Seq<Command<TGate, TInvoke>>>, SKRect, IO<Unit>> menu, Orientation axis)
        where TGate : notnull where TInvoke : notnull =>
        new Realized<TGate, TInvoke>(holds, display, run, menu) switch {
            var realized => groups.TraverseM(group => group.TraverseM(command => Pressed(command, parent: true, None, realized)).As().Map(static nodes => nodes.Flatten()))
                .As()
                .Map(pressed => groups.Count > 1
                    ? (Node)new Node.Choice(axis, toSeq(Range(0, groups.Count)).Map(dropped => (Node)Row(pressed.Take(groups.Count - dropped), groups.Skip(groups.Count - dropped), overflow, realized, axis)))
                    : Row(pressed, [], overflow, realized, axis)),
        };

    private static Node.Flex Row<TGate, TInvoke>(Seq<Seq<Node>> shown, Seq<Seq<Command<TGate, TInvoke>>> rest, CommandFace overflow, Realized<TGate, TInvoke> realized, Orientation axis)
        where TGate : notnull where TInvoke : notnull =>
        new(axis,
            shown.Intersperse(Seq<Node>(Divider(axis))).Flatten()
            + (rest.IsEmpty
                ? Seq<Node>()
                : Seq<Node>(Divider(axis), Button(overflow, new CommandState(Enabled: true, Marked: None), realized.Display, Some(AccessRole.MenuButton),
                    new Part.Press(overflow, bounds => realized.Menu(rest, bounds)))))) {
            Role = Role.Toolbar,
        };

    private static IO<Seq<Node>> Pressed<TGate, TInvoke>(Command<TGate, TInvoke> command, bool parent, Option<AccessRole> role, Realized<TGate, TInvoke> realized)
        where TGate : notnull where TInvoke : notnull =>
        State(command, parent, realized.Holds).Bind(state => command.Switch(
            (State: state, Role: role, Realized: realized),
            run: static (held, run) => IO.pure(Seq<Node>(Button(run.Face, held.State, held.Realized.Display, held.Role, new Part.Press(run.Face, bounds => held.Realized.Run(run.Invoke, bounds)) {
                Secondary = run.Alternate.Map(alternate => (Func<SKRect, IO<Unit>>)(bounds => held.Realized.Run(alternate, bounds))),
                Marked = held.State.Marked,
            }))),
            choice: static (held, choice) => choice.Members.TraverseM(member => Pressed(member, held.State.Enabled, Some(AccessRole.Radio), held.Realized)).As()
                .Map(static nodes => nodes.Flatten()),
            menu: static (held, menu) => IO.pure(Seq<Node>(Button(menu.Face, held.State, held.Realized.Display, menu.Primary.IsSome ? held.Role : Some(AccessRole.MenuButton), menu.Primary.Match(
                Some: primary => new Part.Press(menu.Face, bounds => held.Realized.Run(primary.Invoke, bounds)) { Secondary = Some<Func<SKRect, IO<Unit>>>(bounds => held.Realized.Menu(menu.Groups, bounds)) },
                None: () => new Part.Press(menu.Face, bounds => held.Realized.Menu(menu.Groups, bounds))))))));

    private static Node.Flex Button(CommandFace face, CommandState state, FaceDisplay display, Option<AccessRole> role, Part part) =>
        new(Orientation.Horizontal, Faced(face, display)) {
            Key = new Key.Named(face.Name),
            Role = Role.Button,
            Conditions = toHashSet(Seq(Condition.Disabled).Filter(_ => !state.Enabled) + Seq(Condition.Selected).Filter(_ => state.Marked.Exists(static on => on))),
            Part = state.Enabled ? Some(part) : None,
            Accessible = state.Enabled ? role.Map(declared => new Accessible(declared, face.Caption)) : Some(Part.Press.Described(face, state.Marked, role, [])),
        };

    private static Seq<Node> Faced(CommandFace face, FaceDisplay display) =>
        (Glyph: face.Glyph.Map(static art => (Node)new Node.Glyph(art)), Caption: (Node)new Node.Text(face.Caption)) switch {
            var (glyph, caption) => display switch {
                FaceDisplay.Glyph => (glyph | Some(caption)).ToSeq(),
                FaceDisplay.Caption => Seq(caption),
                FaceDisplay.GlyphAndCaption => glyph.ToSeq().Add(caption),
            },
        };

    private static Node.Flex Divider(Orientation axis) =>
        new(axis, []) {
            Role = Role.Divider,
            Width = axis == Orientation.Vertical ? new Sizing.Grow(1f) : Sizing.Fit.Content,
            Height = axis == Orientation.Horizontal ? new Sizing.Grow(1f) : Sizing.Fit.Content,
        };
}
