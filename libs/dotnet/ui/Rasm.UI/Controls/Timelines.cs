using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Timing {
    internal const uint Slots = 3;

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

public sealed record Timeline<TItem>(PlotPlane.Track Plane, Seq<Lane<TItem>> Lanes, Func<TItem, Key> Keyed, Lens<TItem, Timing> Timed, Func<Edit<Lane<TItem>>, IO<Unit>> Edited) where TItem : notnull {
    // --- [STATE]
    public Option<Part.Viewport> Navigation { get; init; }
    public Option<float> Playhead { get; init; }
    public Option<Func<Edit<float>, IO<Unit>>> Scrubbed { get; init; }
    public Option<AxisRange> Preview { get; init; }
    public Option<Func<Edit<AxisRange>, IO<Unit>>> Previewed { get; init; }
    public Selection Selection { get; init; } = Selection.Empty;
    public Func<Edit<Selection>, IO<Unit>> Selected { get; init; } = static _ => IO.pure(unit);
    public Option<Func<TItem, View>> Face { get; init; }
    public Func<Lane<TItem>, Seq<View>> Toggles { get; init; } = static _ => [];
    public Seq<View> Corner { get; init; }

    // --- [DRAW]
    public View Drawn {
        get {
            Option<Part.Viewport> navigation = Navigation.Map(static viewport => viewport with { Free = [PhysicalAxis.Horizontal] });
            Seq<View> ruled = [
                new View.Flex(PhysicalAxis.Horizontal, Corner) { Sticks = PhysicalAxis.Vertical },
                new View.Flex(PhysicalAxis.Vertical, Overview(navigation).ToSeq() + Seq<View>(Ruler(navigation)) + Previewing.ToSeq()) { Width = Sizing.Grow.Unit, Sticks = PhysicalAxis.Vertical },
            ];
            return new View.Scroll(PhysicalAxis.Vertical, new View.Grid(
                new Tracks([Sizing.Fit.Content, Sizing.Grow.Unit]),
                ruled.Cons(Row<Lane<TItem>>.Flatten(Lanes, static lane => lane.Children, static lane => lane.Key, static lane => !lane.Collapsed, static _ => true).Map(row => Seq<View>(Header(row), Track(row.Item, navigation))))) {
                Width = Sizing.Grow.Unit,
                Accessible = new Accessible { Role = AccessRole.Tree },
            }).Grown;
        }
    }

    private View.Plot Ruler(Option<Part.Viewport> navigation) {
        Seq<View> shade = Preview.Map(static span => (View)new View.Mark(new Figure.Region { First = span }) { Role = Role.Selection }).ToSeq();
        return new View.Plot(Plane, Playhead.Match(
            Some: at => Scrubbed.Match(
                Some: scrubbed => Seq(new Handles<float>(at, Points<float>.Scalar(Plane, Lens.identity<float>()), scrubbed).Drawn(Plane, shade)),
                None: () => shade.Add(Needle(at))),
            None: () => shade)) {
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
            Guides = [new Guide(PhysicalAxis.Horizontal)],
            Part = navigation.Map(static viewport => (Part)viewport),
        };
    }

    private Option<View> Overview(Option<Part.Viewport> navigation) =>
        navigation.Map(viewport => (View)(viewport.Overview(Plane, Playhead.Map(Needle).ToSeq()) with {
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
        }));

    private Option<View> Previewing =>
        from span in Preview
        from previewed in Previewed
        from plane in Plane.WithDomain(PhysicalAxis.Horizontal, span)
        select (View)new View.Plot(Plane, [
            new Brush(plane, edit => previewed(edit.Choose(static held => held.Dimension(PhysicalAxis.Horizontal)).Map(static axis => axis.Domain).As())).Drawn(Plane),
        ]) {
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
        };

    private View.Plot Track(Lane<TItem> lane, Option<Part.Viewport> navigation) {
        Seq<HandlePoint> placed = Placed(lane.Items).Strict();
        HashMap<Key, View.Pin> faces = toHashMap(placed.Choose(point => Faced(lane, point).Map(face => (point.Key, face))));
        Seq<View> under = Playhead.Map(Needle).ToSeq();
        return new View.Plot(Plane, lane.Locked
            ? under + placed.Filter(static point => Leads(point.Key)).Choose(point => faces.Find(point.Key)).Map(static face => (View)face)
            : Seq(new Handles<Seq<TItem>>(lane.Items, Handled, edit => Edited(edit.Map(items => lane with { Items = items }).As())) {
                Selection = new Selection.Many(toHashSet(placed.Filter(point => Leads(point.Key) && Owner(lane.Items, point.Key).Exists(item => Selection.Holds(Keyed(item)))).Map(static point => point.Key))),
                Selected = Some<Func<Edit<Selection>, IO<Unit>>>(edit => Selected(edit.Map(held => held.Renamed(toHashMap(placed.Choose(point => Owner(lane.Items, point.Key).Map(item => (point.Key, Keyed(item))))))).As())),
                Marquee = Marquee.Box,
                Handle = (key, _) => faces[key],
            }.Drawn(Plane, under))) {
            Key = lane.Key,
            Role = Role.Lane,
            Width = Sizing.Grow.Unit,
            Part = navigation.Map(static viewport => (Part)viewport),
            Accessible = new Accessible { Role = AccessRole.Group, Label = lane.Label },
        };
    }

    private View.Flex Header(Row<Lane<TItem>> row) {
        Lane<TItem> lane = row.Item;
        return new View.Flex(PhysicalAxis.Horizontal,
            row.Lead(Some(Edited(Edit.Of(EditPhase.Commit, lane with { Collapsed = !lane.Collapsed }))))
            + Seq<View>(new View.Text([lane.Label]) { Role = Role.Label, MaxLines = 1, Width = Sizing.Grow.Unit, Conditions = lane.Muted ? [Condition.Disabled] : [] })
            + Toggles(lane)
            + Seq(Flag(Role.Lock, lane.Locked, locked => lane with { Locked = locked }), Flag(Role.Visibility, !lane.Muted, audible => lane with { Muted = !audible }))) {
            Role = Role.Lane,
            Accessible = new Accessible { Role = AccessRole.TreeItem, Label = lane.Label, Expanded = row.Open },
        };
    }

    private View Flag(Role glyph, bool on, Func<bool, Lane<TItem>> lane) =>
        new Toggle(on, edit => Edited(edit.Map(lane).As())) {
            Content = [Toggle.Glyphed(glyph, Check.Of(on))],
            Kind = ToggleKind.Button,
        }.Drawn;

    private Option<View.Pin> Faced(Lane<TItem> lane, HandlePoint point) =>
        Owner(lane.Items, point.Key).Map(item => Leads(point.Key)
            ? Timed.Get(item) switch {
                var timing => new View.Pin(point.At, Face.Map(face => face(item)).IfNone(static () => new View.Box())) {
                    Role = timing.Role,
                    Span = timing.Length.Map(static length => new Vector2(length, (float)Fraction.MaxValue)),
                    Align = UnitPoint.Center,
                    Conditions = toHashSet(Seq(Some(Condition.Selected).Filter(_ => Selection.Holds(Keyed(item))), Some(Condition.Disabled).Filter(_ => lane.Muted)).Somes()),
                },
            }
            : new View.Pin(point.At, new View.Box()) { Role = Role.Grip, Align = UnitPoint.Center });

    // --- [HANDLES]
    private Points<Seq<TItem>> Handled =>
        new(Placed, (items, targets, _) => new Dragged<Seq<TItem>>(targets.Fold(items, Moved), targets.Map(static target => target.Key)));

    private Seq<HandlePoint> Placed(Seq<TItem> items) =>
        items.Map((item, index) => Timed.Get(item).Handles.Map((along, handle) => new HandlePoint(new Key.Ordinal(((uint)index * Timing.Slots) + (uint)handle), new Vector2(along, (float)Fraction.Half)))).Flatten();

    private Seq<TItem> Moved(Seq<TItem> items, HandlePoint target) =>
        Slotted(target.Key).Match(
            Some: slot => items.Map((item, index) => index == slot.Item ? Timed.Set(Timed.Get(item).Moved(slot.Handle, target.At.X), item) : item),
            None: () => items);

    // --- [READ]
    private static Option<(int Item, int Handle)> Slotted(Key key) =>
        Optional(key as Key.Ordinal).Map(static ordinal => ((int)(ordinal.Index / Timing.Slots), (int)(ordinal.Index % Timing.Slots)));

    private static Option<TItem> Owner(Seq<TItem> items, Key key) => Slotted(key).Bind(slot => items.At(slot.Item));

    private static bool Leads(Key key) => Slotted(key).Exists(static slot => slot.Handle is 0);

    private static View Needle(float at) => new View.Mark(new Figure.Path([new Vector2(at, (float)Fraction.MinValue), new Vector2(at, (float)Fraction.MaxValue)])) { Role = Role.Needle };
}
