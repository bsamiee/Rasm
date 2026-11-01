using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CommandPresentation(string Name, string Caption) {
    // --- [STATE]
    public Option<string> Help { get; init; }
    public Option<GlyphSource> Glyph { get; init; }
    public Option<string> Shortcut { get; init; }
    public Option<string> Badge { get; init; }

    // --- [CONTENT]
    public static Seq<View> IconOnly(CommandPresentation presentation, HashSet<Condition> state) =>
        presentation.Glyph.Match(Some: art => Seq(Icon(art, state)), None: () => LabelOnly(presentation, state));

    public static Seq<View> LabelOnly(CommandPresentation presentation, HashSet<Condition> _) => Seq<View>(new View.Text([presentation.Caption]) { Align = UnitPoint.Center });

    public static Seq<View> IconAndLabel(CommandPresentation presentation, HashSet<Condition> state) => presentation.Labeled(state, [presentation.Caption]);

    public static Func<CommandPresentation, HashSet<Condition>, Seq<View>> Fitting(params Seq<Func<CommandPresentation, HashSet<Condition>, Seq<View>>> preferred) =>
        (presentation, state) => Seq<View>(new View.Choice(PhysicalAxis.Horizontal, preferred.Map(faced => (View)new View.Flex(PhysicalAxis.Horizontal, faced(presentation, state)))));

    // --- [FACES]
    internal View Button(HashSet<Condition> state, AccessRole access, Role look, Option<Part.Press> press, Seq<View> content, Seq<View> above = default) {
        Seq<View> pinned = Badge.Map(static badge => (View)new View.Pin(new Vector2(1f, 0f), new View.Text([badge]) { Role = Role.Badge, Align = UnitPoint.Center }) { Align = UnitPoint.Center }).ToSeq() + above;
        View framed = (pinned.IsEmpty ? new View.Flex(PhysicalAxis.Horizontal, content) : (View)new View.Overlay(new View.Flex(PhysicalAxis.Horizontal, content), pinned)) with {
            Key = new Key.Named(Name),
            Role = look,
            Conditions = state,
            Accessible = new Accessible {
                Role = access,
                Label = Badge.Filter(static badge => badge.Length > 0).Map(badge => $"{Caption} {badge}").IfNone(Caption),
                Help = Help,
                Shortcut = Shortcut,
            },
        };
        return press.Match(Some: part => part.Drawn(framed), None: () => framed);
    }

    internal View Item(HashSet<Condition> state, Option<Part.Press> press, Seq<Span> caption, Seq<View> above = default) =>
        Button(state, AccessRole.MenuItem, Role.ListRow, press,
            Labeled(state, caption) + Shortcut.ToSeq().Bind(static shortcut => Seq<View>(new View.Box { Width = Sizing.Grow.Unit }, new View.Text([shortcut]) { Role = Role.Keycap, Align = UnitPoint.Center })),
            above) with {
            Width = Sizing.Grow.Unit,
        };

    private Seq<View> Labeled(HashSet<Condition> state, Seq<Span> caption) => Glyph.Map(art => Icon(art, state)).ToSeq().Add(new View.Text(caption) { Align = UnitPoint.Center });

    private static View Icon(GlyphSource art, HashSet<Condition> state) =>
        state.Contains(Condition.Busy) ? new View.Box { Role = Role.Spinner, Align = UnitPoint.Center } : new View.Glyph(art) { Align = UnitPoint.Center };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Command<TGate, TInvoke> where TGate : notnull where TInvoke : notnull {
    private Command(CommandPresentation presentation, Option<TGate> enabled) => (Presentation, Enabled) = (presentation, enabled);

    // --- [STATE]
    public CommandPresentation Presentation { get; private init; }
    public Option<TGate> Enabled { get; }

    // --- [PRESENTATION]
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

    // --- [CASES]
    public sealed record Run(CommandPresentation Presentation, Option<TGate> Enabled, TInvoke Invoke) : Command<TGate, TInvoke>(Presentation, Enabled) {
        public Option<TGate> Checked { get; init; }
        public Option<TGate> Busy { get; init; }
        public Option<TInvoke> Alternate { get; init; }

        internal AccessRole Access => Checked.IsSome ? AccessRole.CheckBox : AccessRole.Button;
    }

    public sealed record Choice(CommandPresentation Presentation, Option<TGate> Enabled, Seq<Run> Members) : Command<TGate, TInvoke>(Presentation, Enabled);

    public sealed record Menu(CommandPresentation Presentation, Option<TGate> Enabled, Seq<Seq<Command<TGate, TInvoke>>> Groups) : Command<TGate, TInvoke>(Presentation, Enabled) {
        public Option<Run> Primary { get; init; }
    }
}

public sealed record CommandHost<TGate, TInvoke>(
    Func<TGate, IO<bool>> Holds,
    Func<TGate, IO<Check>> Checked,
    Func<TInvoke, SKRect, IO<Unit>> Run,
    Func<Seq<Seq<Command<TGate, TInvoke>>>, SKRect, IO<Unit>> Menu)
    where TGate : notnull where TInvoke : notnull {
    // --- [BAR]
    internal IO<View> Drawn(Command<TGate, TInvoke> command, Func<CommandPresentation, HashSet<Condition>, Seq<View>> content, PhysicalAxis axis, Option<AccessRole> within) =>
        command.Switch(
            (Host: this, Content: content, Axis: axis, Within: within),
            run: static (held, run) => held.Host.State(run.Enabled, run.Busy, run.Checked)
                .Map(state => run.Presentation.Button(state, held.Within.IfNone(run.Access), Role.Button, Some(held.Host.Pressing(run)), held.Content(run.Presentation, state))),
            choice: static (held, choice) =>
                from state in held.Host.State(choice.Enabled, None, None)
                from members in choice.Members.TraverseM(member => held.Host.Drawn(member, held.Content, held.Axis, AccessRole.RadioGroup.Owns)).As()
                select (View)new View.Flex(held.Axis, members) {
                    Key = new Key.Named(choice.Presentation.Name),
                    Conditions = state,
                    Accessible = new Accessible { Role = AccessRole.RadioGroup, Label = choice.Presentation.Caption, Help = choice.Presentation.Help, Shortcut = choice.Presentation.Shortcut },
                },
            menu: static (held, menu) =>
                from state in held.Host.State(menu.Enabled, None, None)
                from split in held.Host.Split(menu, state)
                select (from primary in menu.Primary
                        from shown in split
                        select menu.Presentation.Button(shown, held.Within.IfNone(primary.Access), Role.Button,
                            Some(new Part.Press(bounds => held.Host.Run(primary.Invoke, bounds)) { Secondary = bounds => Some(held.Host.Menu(menu.Groups, bounds)) }),
                            held.Content(menu.Presentation, shown)))
                    .IfNone(() => held.Host.MenuButton(menu.Presentation, state, menu.Groups, held.Content)));

    internal View.Flex Row(Seq<Seq<View>> shown, Seq<Seq<Command<TGate, TInvoke>>> rest, Toolbar<TGate, TInvoke> toolbar) =>
        new(toolbar.Axis, (rest.IsEmpty ? shown : shown.Add(Seq(MenuButton(toolbar.Overflow, [], rest, toolbar.Content)))).Intersperse(Seq(View.Divider(toolbar.Axis))).Flatten()) {
            Role = Role.Toolbar,
            Accessible = new Accessible { Role = AccessRole.Toolbar },
        };

    // --- [MENU]
    public IO<View.Layer> Popup(Seq<Seq<Command<TGate, TInvoke>>> groups, Trigger trigger) =>
        Listed(groups).Map(listed => new View.Layer(listed) { Trigger = trigger, Accessible = new Accessible { Role = AccessRole.Menu } });

    private IO<View.Flex> Listed(Seq<Seq<Command<TGate, TInvoke>>> groups) =>
        groups.TraverseM(group => group.TraverseM(command => Rows(command)).As().Map(static rows => rows.Flatten())).As()
            .Map(static rows => new View.Flex(PhysicalAxis.Vertical, rows.Intersperse(Seq(View.Divider(PhysicalAxis.Vertical))).Flatten()) { Role = Role.Menu });

    private IO<Seq<View>> Rows(Command<TGate, TInvoke> command) =>
        command.Switch(
            this,
            run: static (held, run) => held.State(run.Enabled, run.Busy, run.Checked).Map(state => Seq(run.Presentation.Item(state, Some(held.Pressing(run)), [run.Presentation.Caption]))),
            choice: static (held, choice) =>
                from state in held.State(choice.Enabled, None, None)
                from rows in choice.Members.TraverseM(member => held.Rows(member)).As()
                select rows.Flatten().Map(row => row with { Conditions = row.Conditions + state }),
            menu: static (held, menu) =>
                from state in held.State(menu.Enabled, None, None)
                from split in held.Split(menu, state)
                from listed in held.Listed(menu.Groups)
                select Seq(menu.Presentation.Item(split.IfNone(state), split.Bind(_ => menu.Primary).Map(held.Pressing), [menu.Presentation.Caption],
                    [new View.Layer(listed) {
                        Key = new Key.Named("menu"),
                        Side = new UnitPoint(Fraction.MaxValue, Fraction.MinValue),
                        Trigger = split.IsSome ? new Trigger.Hover(Duration.Zero) : (Trigger)new Trigger.Press { Delay = Duration.Zero },
                        Accessible = new Accessible { Role = AccessRole.Menu },
                    }])));

    // --- [STATE]
    private IO<HashSet<Condition>> State(Option<TGate> enabled, Option<TGate> busy, Option<TGate> check) =>
        from open in enabled.Traverse(Holds).As()
        from working in busy.Traverse(Holds).As()
        from mark in check.Traverse(Checked).As()
        select toHashSet(Seq(open.Filter(static holds => !holds).Map(static _ => Condition.Disabled), working.Filter(static holds => holds).Map(static _ => Condition.Busy), mark.Bind(static held => held.Conditioned)).Somes());

    private IO<Option<HashSet<Condition>>> Split(Command<TGate, TInvoke>.Menu menu, HashSet<Condition> state) =>
        menu.Primary.Traverse(primary => State(primary.Enabled, primary.Busy, primary.Checked)).As()
            .Map(primary => primary.Filter(static shown => !shown.Contains(Condition.Disabled)).Map(shown => state + shown));

    private View MenuButton(CommandPresentation shown, HashSet<Condition> state, Seq<Seq<Command<TGate, TInvoke>>> groups, Func<CommandPresentation, HashSet<Condition>, Seq<View>> content) =>
        shown.Button(state, AccessRole.MenuButton, Role.Button, Some(new Part.Press(bounds => Menu(groups, bounds))), content(shown, state));

    private Part.Press Pressing(Command<TGate, TInvoke>.Run run) => new(bounds => Run(run.Invoke, bounds)) { Secondary = bounds => run.Alternate.Map(alternate => Run(alternate, bounds)) };
}

public sealed record Toolbar<TGate, TInvoke>(Seq<Seq<Command<TGate, TInvoke>>> Groups, CommandPresentation Overflow) where TGate : notnull where TInvoke : notnull {
    // --- [STATE]
    public PhysicalAxis Axis { get; init; } = PhysicalAxis.Horizontal;
    public Func<CommandPresentation, HashSet<Condition>, Seq<View>> Content { get; init; } = CommandPresentation.Fitting(CommandPresentation.IconAndLabel, CommandPresentation.IconOnly);

    // --- [DRAW]
    public IO<View> Drawn(CommandHost<TGate, TInvoke> host) =>
        Groups.TraverseM(group => group.TraverseM(command => host.Drawn(command, Content, Axis, None)).As()).As().Map(drawn =>
            (View)new View.Choice(Axis, toSeq(Range(0, drawn.Count + 1)).Map(dropped => (View)host.Row(drawn.Take(drawn.Count - dropped), Groups.Skip(drawn.Count - dropped), this))));
}
