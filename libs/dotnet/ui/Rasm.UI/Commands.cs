using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandPresentation(string Name, string Caption) {
    public Option<string> Help { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Shortcut { get; init; }
}

public sealed record CommandState(bool Enabled, Option<Check> Check);

public sealed record CommandHost<TGate, TInvoke>(
    Func<TGate, IO<bool>> Enabled,
    Func<TGate, IO<Check>> Checked,
    Func<TInvoke, SKRect, IO<Unit>> Run,
    Func<Seq<Seq<Command<TGate, TInvoke>>>, SKRect, IO<Unit>> Menu)
    where TGate : notnull where TInvoke : notnull;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Command<TGate, TInvoke> where TGate : notnull where TInvoke : notnull {
    private Command(CommandPresentation face, Option<TGate> enabled) => (Presentation, Enabled) = (face, enabled);

    public CommandPresentation Presentation { get; private init; }
    public Option<TGate> Enabled { get; }

    public Command<TGate, TInvoke> Presented(Func<CommandPresentation, Option<TInvoke>, CommandPresentation> face) =>
        Switch(
            face,
            run: static (into, run) => (Command<TGate, TInvoke>)(run with { Presentation = into(run.Presentation, Some(run.Invoke)) }),
            choice: static (into, choice) => choice with { Presentation = into(choice.Presentation, None), Members = choice.Members.Map(member => member with { Presentation = into(member.Presentation, Some(member.Invoke)) }) },
            menu: static (into, menu) => menu with {
                Presentation = into(menu.Presentation, None),
                Groups = menu.Groups.Map(group => group.Map(command => command.Presented(into))),
                Primary = menu.Primary.Map(primary => primary with { Presentation = into(primary.Presentation, Some(primary.Invoke)) }),
            });

    internal Option<TGate> CheckGate => Switch(run: static run => run.Checked, choice: static _ => Option<TGate>.None, menu: static _ => Option<TGate>.None);

    public sealed record Run(CommandPresentation Face, Option<TGate> Enabled, TInvoke Invoke) : Command<TGate, TInvoke>(Face, Enabled) {
        public Option<TGate> Checked { get; init; }
        public Option<TInvoke> Alternate { get; init; }
    }

    public sealed record Choice(CommandPresentation Face, Option<TGate> Enabled, Seq<Run> Members) : Command<TGate, TInvoke>(Face, Enabled);

    public sealed record Menu(CommandPresentation Face, Option<TGate> Enabled, Seq<Seq<Command<TGate, TInvoke>>> Groups) : Command<TGate, TInvoke>(Face, Enabled) {
        public Option<Run> Primary { get; init; }
    }
}

public sealed record ResolvedCommand<TGate, TInvoke>(Command<TGate, TInvoke> Command, CommandState State, Seq<ResolvedCommand<TGate, TInvoke>> Members) where TGate : notnull where TInvoke : notnull;

public sealed record Toolbar<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> Groups, CommandPresentation Overflow) where TGate : notnull where TInvoke : notnull {
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;
    public Func<CommandPresentation, Seq<Node>> Content { get; init; } = Commands.Fitting(Commands.IconAndLabel, Commands.IconOnly);
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
        select new ResolvedCommand<TGate, TInvoke>(command, state, members);

    private static IO<CommandState> State<TGate, TInvoke>(Command<TGate, TInvoke> command, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        from enabled in command.Enabled.Traverse(host.Enabled).As()
        from mark in command.CheckGate.Traverse(host.Checked).As()
        select new CommandState(enabled.IfNone(noneValue: true), mark);

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
                : Seq(Divider(toolbar.Axis), MenuButton(toolbar.Overflow, new CommandState(Enabled: true, Check: None), rest.Map(static group => group.Map(static held => held.Command)), host, toolbar.Content)))) {
            Role = Role.Toolbar,
            Accessible = new Accessible { Role = AccessRole.Toolbar },
        };

    private static Node Drawn<TGate, TInvoke>(ResolvedCommand<TGate, TInvoke> stated, CommandHost<TGate, TInvoke> host, Func<CommandPresentation, Seq<Node>> face, PhysicalAxis axis, Option<AccessRole> within)
        where TGate : notnull where TInvoke : notnull =>
        stated.Command.Switch(
            (Stated: stated, Host: host, Face: face, Axis: axis, Within: within),
            run: static (held, run) => Button(run.Presentation, held.Stated.State, held.Within, PressPart(run, held.Host), held.Face),
            choice: static (held, choice) => new Node.Flex(held.Axis, held.Stated.Members.Map(member => Drawn(member, held.Host, held.Face, held.Axis, AccessRole.RadioGroup.Owns))) {
                Key = new Key.Named(choice.Presentation.Name),
                Conditions = Conditioned(held.Stated.State),
                Accessible = new Accessible { Role = AccessRole.RadioGroup, Label = choice.Presentation.Caption, Help = choice.Presentation.Help, Shortcut = choice.Presentation.Shortcut },
            },
            menu: static (held, menu) => (from primary in menu.Primary
                                          from shown in held.Stated.Members.Head.Map(static member => member.State)
                                          where shown.Enabled
                                          select Button(menu.Presentation, held.Stated.State with { Check = shown.Check }, held.Within,
                                              new Part.Press(bounds => held.Host.Run(primary.Invoke, bounds)) { Secondary = bounds => Some(held.Host.Menu(menu.Groups, bounds)) }, held.Face))
                .IfNone(() => MenuButton(menu.Presentation, held.Stated.State, menu.Groups, held.Host, held.Face)));

    private static Part.Press PressPart<TGate, TInvoke>(Command<TGate, TInvoke>.Run run, CommandHost<TGate, TInvoke> host)
        where TGate : notnull where TInvoke : notnull =>
        new(bounds => host.Run(run.Invoke, bounds)) { Secondary = bounds => run.Alternate.Map(alternate => host.Run(alternate, bounds)) };

    private static Node MenuButton<TGate, TInvoke>(
        CommandPresentation shown, CommandState state, Seq<Seq<Command<TGate, TInvoke>>> groups, CommandHost<TGate, TInvoke> host, Func<CommandPresentation, Seq<Node>> face)
        where TGate : notnull where TInvoke : notnull =>
        Button(shown, state, Some(AccessRole.MenuButton), new Part.Press(bounds => host.Menu(groups, bounds)), face);

    private static Node Button(CommandPresentation shown, CommandState state, Option<AccessRole> within, Part.Press press, Func<CommandPresentation, Seq<Node>> face) {
        AccessRole role = within.IfNone(state.Check.IsSome ? AccessRole.CheckBox : AccessRole.Button);
        return press.Drawn(new Node.Flex(PhysicalAxis.Horizontal, face(shown)) {
            Key = new Key.Named(shown.Name),
            Role = Role.Button,
            Conditions = Conditioned(state),
            Accessible = state.Check.Fold(new Accessible { Role = role, Label = shown.Caption, Help = shown.Help, Shortcut = shown.Shortcut }, role.WithCheck),
        });
    }

    private static HashSet<Condition> Conditioned(CommandState state) =>
        toHashSet(Seq(Condition.Disabled).Filter(_ => !state.Enabled) + state.Check.Bind(static mark => mark.Conditioned).ToSeq());

    private static Node.Box Divider(PhysicalAxis axis) =>
        new() {
            Role = Role.Divider,
            Width = axis.Map(horizontal: Option<Sizing>.None, vertical: Sizing.Grow.Unit),
            Height = axis.Map(horizontal: Sizing.Grow.Unit, vertical: Option<Sizing>.None),
        };

    // --- [PRESENTATIONS]
    public static Seq<Node> IconOnly(CommandPresentation face) => face.Glyph.Match(Some: static art => Seq(Glyph(art)), None: () => LabelOnly(face));

    public static Seq<Node> LabelOnly(CommandPresentation face) => Seq<Node>(Caption(face));

    public static Seq<Node> IconAndLabel(CommandPresentation face) => face.Glyph.Map(Glyph).ToSeq().Add(Caption(face));

    public static Func<CommandPresentation, Seq<Node>> Fitting(params Seq<Func<CommandPresentation, Seq<Node>>> preferred) =>
        face => Seq<Node>(new Node.Choice(PhysicalAxis.Horizontal, preferred.Map(faced => (Node)new Node.Flex(PhysicalAxis.Horizontal, faced(face)))));

    private static Node Glyph(GlyphSource art) => new Node.Glyph(art) { Align = UnitPoint.Center };

    private static Node.Text Caption(CommandPresentation face) => new([face.Caption]) { Align = UnitPoint.Center };
}
