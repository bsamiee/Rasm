using System.Globalization;
using System.Numerics;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Timing {
    private Timing(Role role, Seq<float> handles, Option<float> length = default) => (Role, Handles, Length) = (role, handles, length);

    internal Role Role { get; }
    internal Seq<float> Handles { get; }
    internal Option<float> Length { get; }

    internal Timing Moved(int handle, float to) =>
        Switch<(int Handle, float To), Timing>(
            (handle, to),
            clip: static (held, clip) => (held.Handle switch {
                0 => clip.Span.Shifted(held.To - clip.Span.Mid),
                1 => AxisRange.Validated(held.To, clip.Span.To).ToOption(),
                _ => AxisRange.Validated(clip.Span.From, held.To).ToOption(),
            }).Filter(static span => span.From < span.To).Map(static span => (Timing)new Clip(span)).IfNone(clip),
            point: static (held, point) => point with { At = held.To });

    public sealed record Clip(AxisRange Span) : Timing(Role.Card, [Span.Mid, Span.From, Span.To], Span.Span);

    public sealed record Point(float At, Role Role) : Timing(Role, [At]);
}

public sealed record Lane<TItem>(Key Key, string Label, Seq<TItem> Items) where TItem : notnull {
    public Seq<Lane<TItem>> Children { get; init; }
    public bool Locked { get; init; }
    public bool Muted { get; init; }
    public bool Collapsed { get; init; }
}

public sealed record Timeline<TItem>(PlotPlane.Track View, Seq<Lane<TItem>> Lanes, Func<TItem, Key> Keyed, Lens<TItem, Timing> Timed, Func<Edit<Lane<TItem>>, IO<Unit>> Edited) where TItem : notnull {
    // --- [STATE]
    private const int Slots = 3;

    public Option<Part.Viewport> Navigation { get; init; }
    public Option<float> Playhead { get; init; }
    public Option<Func<Edit<float>, IO<Unit>>> Scrubbed { get; init; }
    public Option<AxisRange> Preview { get; init; }
    public Option<Func<Edit<AxisRange>, IO<Unit>>> Previewed { get; init; }
    public Selection Selection { get; init; } = new Selection.Many([]);
    public Func<Edit<Selection>, IO<Unit>> Selected { get; init; } = static _ => IO.pure(unit);
    public Option<Func<TItem, Node>> Face { get; init; }
    public Func<Lane<TItem>, Seq<Node>> Toggles { get; init; } = static _ => [];
    public Seq<Node> Corner { get; init; }

    // --- [DRAW]
    public Node Drawn {
        get {
            HashSet<Key> open = toHashSet(Every(Lanes).Filter(static lane => !lane.Collapsed).Map(static lane => lane.Key));
            Option<Part.Viewport> navigation = Navigation.Map(static viewport => viewport with { Free = [PhysicalAxis.Horizontal] });
            Seq<Node> ruled = [
                new Node.Flex(PhysicalAxis.Horizontal, Corner) { Sticks = PhysicalAxis.Vertical },
                new Node.Flex(PhysicalAxis.Vertical, Overview(navigation).ToSeq() + Seq<Node>(Ruler(navigation)) + Previewing.ToSeq()) { Width = Sizing.Grow.Unit, Sticks = PhysicalAxis.Vertical },
            ];
            return new Node.Scroll(PhysicalAxis.Vertical, new Node.Grid(
                new Tracks([Sizing.Fit.Content, Sizing.Grow.Unit]),
                ruled.Cons(Row<Lane<TItem>>.Flatten(Lanes, static lane => lane.Children, static lane => lane.Key, open, static _ => true).Map(row => Seq<Node>(Header(row), Track(row.Item, navigation))))) {
                Width = Sizing.Grow.Unit,
                Accessible = new Accessible { Role = AccessRole.Tree },
            }) {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            };
        }
    }

    private Node.Plot Ruler(Option<Part.Viewport> navigation) {
        Seq<Node> shade = Preview.Map(static span => (Node)new Node.Mark(new Figure.Region { First = span }) { Role = Role.Selection }).ToSeq();
        return new Node.Plot(View, Playhead.Match(
            Some: at => Scrubbed.Match(
                Some: scrubbed => Seq(new Handles<float>(at, Points<float>.Scalar(View, Lens.identity<float>()), scrubbed).Drawn(View, shade)),
                None: () => shade.Add(Needle(at))),
            None: () => shade)) {
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
            Guides = [new Guide(PhysicalAxis.Horizontal)],
            Part = navigation.Map(static viewport => (Part)viewport),
        };
    }

    private Option<Node> Overview(Option<Part.Viewport> navigation) =>
        navigation.Map(viewport => (Node)new Node.Plot(viewport.Rest, Playhead.Map(Needle).ToSeq().Add(new Brush(View, viewport.Edited).Drawn(viewport.Rest))) {
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
        });

    private Option<Node> Previewing =>
        from span in Preview
        from previewed in Previewed
        from plane in View.WithDomain(PhysicalAxis.Horizontal, span)
        select (Node)new Node.Plot(View, [
            new Brush(plane, edit => previewed(new Edit<AxisRange>(edit.Phase, edit.Value.Bind(static held => held.Dimension(PhysicalAxis.Horizontal)).Map(static axis => axis.Domain)))).Drawn(View),
        ]) {
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
        };

    private Node.Plot Track(Lane<TItem> lane, Option<Part.Viewport> navigation) {
        Seq<(HandlePoint Point, TItem Item, Timing Timing, int Handle)> placed = Placed(lane.Items).Strict();
        HashMap<Key, Node.Pin> faces = toHashMap(placed.Map(held => (held.Point.Key, Faced(lane, held))));
        Seq<Node> under = Playhead.Map(Needle).ToSeq();
        return new Node.Plot(View, lane.Locked
            ? under + placed.Map(held => (Node)faces[held.Point.Key])
            : Seq(new Handles<Seq<TItem>>(lane.Items, Handled, edit => Edited(new Edit<Lane<TItem>>(edit.Phase, edit.Value.Map(items => lane with { Items = items })))) {
                Selection = new Selection.Many(toHashSet(placed.Filter(held => held.Handle is 0 && Selection.Holds(Keyed(held.Item))).Map(static held => held.Point.Key))),
                Selected = Some<Func<Edit<Selection>, IO<Unit>>>(edit => Selected(new Edit<Selection>(edit.Phase, edit.Value.Map(held => held.Renamed(toHashMap(placed.Map(item => (item.Point.Key, Keyed(item.Item))))))))),
                Marquee = Marquee.Box,
                Handle = (key, _) => faces[key],
            }.Drawn(View, under))) {
            Key = lane.Key,
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
            Part = navigation.Map(static viewport => (Part)viewport),
            Accessible = new Accessible { Role = AccessRole.Group, Label = lane.Label },
        };
    }

    private Node.Flex Header(Row<Lane<TItem>> row) {
        Lane<TItem> lane = row.Item;
        return new Node.Flex(PhysicalAxis.Horizontal,
            toSeq(Range(0, row.Depth)).Map(static _ => Sources.Spacer(Role.Chevron))
            + Seq(row.Open.Map(open => new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, lane with { Collapsed = open })))
                .Drawn(new Node.Box { Role = Role.Chevron, Conditions = open ? [Condition.Selected] : [] }) with {
                Accessible = new Accessible { Role = AccessRole.Disclosure, Label = lane.Label, Expanded = open },
            }).IfNone(static () => Sources.Spacer(Role.Chevron)))
            + Seq<Node>(new Node.Text([lane.Label]) { Role = Role.Label, MaxLines = 1, Width = Sizing.Grow.Unit, Conditions = lane.Muted ? [Condition.Disabled] : [] })
            + Toggles(lane)
            + Seq(Flag(Role.Lock, lane.Locked, locked => lane with { Locked = locked }), Flag(Role.Visibility, !lane.Muted, audible => lane with { Muted = !audible }))) {
            Role = Role.Lane,
            Accessible = new Accessible { Role = AccessRole.TreeItem, Label = lane.Label, Expanded = row.Open },
        };
    }

    private Node Flag(Role glyph, bool on, Func<bool, Lane<TItem>> lane) =>
        new Toggle(on ? Check.On : Check.Off, edit => Edited(new Edit<Lane<TItem>>(edit.Phase, edit.Value.Map(check => lane(check == Check.On))))) {
            Content = [new Node.Box { Role = glyph, Conditions = on ? [] : [Condition.Disabled] }],
            Role = Role.Button,
        }.Drawn;

    private Node.Pin Faced(Lane<TItem> lane, (HandlePoint Point, TItem Item, Timing Timing, int Handle) placed) =>
        placed.Handle is 0
            ? new Node.Pin(placed.Point.At, Face.Map(face => face(placed.Item)).IfNone(static () => new Node.Box())) {
                Role = placed.Timing.Role,
                Span = placed.Timing.Length.Map(static length => new Vector2(length, (float)Fraction.MaxValue)),
                Align = UnitPoint.Center,
                Conditions = toHashSet(Seq(Some(Condition.Selected).Filter(_ => Selection.Holds(Keyed(placed.Item))), Some(Condition.Disabled).Filter(_ => lane.Muted)).Somes()),
            }
            : new Node.Pin(placed.Point.At, new Node.Box()) { Role = Role.Grip, Align = UnitPoint.Center };

    // --- [HANDLES]
    private Points<Seq<TItem>> Handled =>
        new(items => Placed(items).Map(static held => held.Point), (items, targets, _) => new Dragged<Seq<TItem>>(targets.Fold(items, Moved), targets.Map(static target => target.Key)));

    private Seq<(HandlePoint Point, TItem Item, Timing Timing, int Handle)> Placed(Seq<TItem> items) =>
        items.Map((item, index) => Timed.Get(item) switch {
            var timing => timing.Handles.Map((along, handle) => (Point: new HandlePoint(new Key.Ordinal((uint)((index * Slots) + handle)), new Vector2(along, (float)Fraction.Half)), Item: item, Timing: timing, Handle: handle)),
        }).Flatten();

    private Seq<TItem> Moved(Seq<TItem> items, HandlePoint target) =>
        Optional(target.Key as Key.Ordinal).Match(
            Some: ordinal => items.Map((item, index) => index == (int)(ordinal.Index / Slots) ? Timed.Set(Timed.Get(item).Moved((int)(ordinal.Index % Slots), target.At.X), item) : item),
            None: () => items);

    // --- [READ]
    private static Node Needle(float at) => new Node.Mark(new Figure.Path([new Vector2(at, (float)Fraction.MinValue), new Vector2(at, (float)Fraction.MaxValue)])) { Role = Role.Needle };

    private static Seq<Lane<TItem>> Every(Seq<Lane<TItem>> lanes) => lanes.Bind(static lane => lane.Cons(Every(lane.Children)));
}

public sealed record RangePicker(Axis Axis, Seq<Preset<AxisRange>> Presets, Func<Edit<AxisRange>, IO<Unit>> Edited) {
    public Node Drawn(CultureInfo culture) {
        AxisRange range = Axis.Domain;
        HashMap<Key, Preset<AxisRange>> keyed = toHashMap(Presets.Map(static preset => (preset.Key, preset)));
        Option<Preset<AxisRange>> chosen = Presets.Find(preset => preset.Value == range);
        Node face = new Node.Flex(PhysicalAxis.Horizontal,
            chosen.Match(
                Some: static preset => Seq<Node>(new Node.Text([preset.Label]) { MaxLines = 1 }),
                None: () => Seq<Node>(Said(range.From, culture), new Node.Box { Role = Role.Chevron }, Said(range.To, culture)))
            + (Presets.IsEmpty ? [] : Seq<Node>(new Node.Box { Width = Sizing.Grow.Unit }, new Node.Box { Role = Role.Chevron, Conditions = [Condition.Selected] }))) {
            Role = Role.Button,
            Width = Sizing.Grow.Unit,
        };
        Node listed = new Part.Choice(Presets.Map(static preset => preset.Key), new Selection.One(chosen.Map(static preset => preset.Key)),
                edit => Edited(new Edit<AxisRange>(edit.Phase, edit.Value.Bind(static held => held.Lead).Bind(keyed.Find).Map(static preset => preset.Value))))
            .Drawn(key => new Node.Text([keyed[key].Label]) { MaxLines = 1, Width = Sizing.Grow.Unit }, static rows => new Node.Flex(PhysicalAxis.Vertical, rows) { Role = Role.Menu, Width = Sizing.Grow.Unit });
        return new Node.Flex(PhysicalAxis.Horizontal, [Shift(-1f, culture), Presets.IsEmpty ? face : Controls.Dropdown(face, listed) with { Width = Sizing.Grow.Unit }, Shift(1f, culture)]) {
            Role = Role.Segment,
            Accessible = new Accessible { Role = AccessRole.Group, Label = Spoken(range, culture) },
        };
    }

    private Node Shift(float sign, CultureInfo culture) {
        Node face = new Node.Flex(PhysicalAxis.Horizontal, [
            new Node.Box { Role = Role.Chevron, Align = UnitPoint.Center, Transform = sign < 0f ? Some(Matrix4x4.CreateScale(-1f, 1f, 1f)) : None },
        ]) {
            Role = Role.Button,
        };
        return Axis.Domain.Shifted(sign * Axis.Domain.Span).Match(
            Some: target => new Part.Press(_ => Edited(Edit.Of(EditPhase.Commit, target))).Drawn(face with { Accessible = new Accessible { Label = Spoken(target, culture) } }),
            None: () => face.Conditioned(Condition.Disabled));
    }

    private Node.Text Said(float value, CultureInfo culture) => new([Axis.Spoken(value, culture)]) { MaxLines = 1 };

    private string Spoken(AxisRange range, CultureInfo culture) => $"{Axis.Spoken(range.From, culture)}{culture.TextInfo.ListSeparator} {Axis.Spoken(range.To, culture)}";
}
