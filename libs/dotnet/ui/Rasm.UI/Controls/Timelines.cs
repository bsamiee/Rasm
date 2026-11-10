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
    private Timing(Key role) => Role = role;

    internal Key Role { get; }

    internal Timing Moved(int handle, float to) =>
        Switch<(int Handle, float To), Timing>(
            (handle, to),
            clip: static (held, clip) => (held.Handle switch {
                0 => clip.Span.Shifted(held.To - clip.Span.Mid),
                1 => AxisRange.Validated(held.To, clip.Span.To).ToOption(),
                _ => AxisRange.Validated(clip.Span.From, held.To).ToOption(),
            }).Filter(static span => span.From < span.To).Map(static span => (Timing)new Clip(span)).IfNone(clip),
            point: static (held, point) => point with { At = held.To });

    public sealed record Clip(AxisRange Span) : Timing(Roles.Card);

    public sealed record Point(float At, Key Role) : Timing(Role);
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

    // --- [DRAWING]

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
        Seq<View> shade = Preview.Map(static span => (View)new View.Mark(new Figure.Region { First = span }) { Role = Roles.Selection }).ToSeq();
        return new View.Plot(Plane, Playhead.Match(
            Some: at => Scrubbed.Match(
                Some: scrubbed => Seq(new Handles<float>(at, Points<float>.Scalar(Plane, Lens.identity<float>()), scrubbed).Drawn(Plane, shade)),
                None: () => shade.Add(Needle(at))),
            None: () => shade)) {
            Role = Roles.Lane,
            Width = Sizing.Grow.Unit,
            Guides = [new Guide(PhysicalAxis.Horizontal)],
            Part = navigation.Map(static viewport => (Part)viewport),
        };
    }

    private Option<View> Overview(Option<Part.Viewport> navigation) =>
        navigation.Map(viewport => (View)(viewport.Overview(Plane, Playhead.Map(Needle).ToSeq()) with {
            Role = Roles.Lane,
            Width = Sizing.Grow.Unit,
        }));

    private Option<View> Previewing =>
        from span in Preview
        from previewed in Previewed
        from plane in Plane.WithDomain(PhysicalAxis.Horizontal, span)
        select (View)new View.Plot(Plane, [
            new Brush(plane, edit => previewed(edit.Choose(static held => held.Dimension(PhysicalAxis.Horizontal)).Map(static axis => axis.Domain).As())).Drawn(Plane),
        ]) {
            Role = Roles.Lane,
            Width = Sizing.Grow.Unit,
        };

    private View.Plot Track(Lane<TItem> lane, Option<Part.Viewport> navigation) {
        Seq<(HandlePoint Point, int Item, int Handle)> placed = Placed(lane.Items).Strict();
        HashMap<Key, (int Item, int Handle)> slots = toHashMap(placed.Map(static held => (held.Point.Key, (held.Item, held.Handle))));
        HashMap<Key, View.Pin> faces = toHashMap(placed.Map(held => (held.Point.Key, Faced(lane, lane.Items[held.Item], held.Point, held.Handle is 0))));
        Seq<View> under = Playhead.Map(Needle).ToSeq();
        return new View.Plot(Plane, lane.Locked
            ? under + placed.Filter(static held => held.Handle is 0).Map(held => (View)faces[held.Point.Key])
            : Seq(new Handles<Seq<TItem>>(lane.Items, Handled(slots), edit => Edited(edit.Map(items => lane with { Items = items }).As())) {
                Selection = new Selection.Many(toHashSet(placed.Filter(held => held.Handle is 0 && Selection.Holds(Keyed(lane.Items[held.Item]))).Map(static held => held.Point.Key))),
                Selected = Some<Func<Edit<Selection>, IO<Unit>>>(edit => Selected(edit.Map(held => held.Renamed(toHashMap(placed.Map(point => (point.Point.Key, Some(Keyed(lane.Items[point.Item]))))))).As())),
                Marquee = Marquee.Box,
                Handle = (key, _) => faces[key],
            }.Drawn(Plane, under))) {
            Key = lane.Key,
            Role = Roles.Lane,
            Width = Sizing.Grow.Unit,
            Part = navigation.Map(static viewport => (Part)viewport),
            Accessible = new Accessible { Role = AccessRole.Group, Label = lane.Label },
        };
    }

    private View.Flex Header(Row<Lane<TItem>> row) {
        Lane<TItem> lane = row.Item;
        return new View.Flex(PhysicalAxis.Horizontal,
            row.Lead(Some(Edited(Edit.Of(EditPhase.Commit, lane with { Collapsed = !lane.Collapsed }))))
            + Seq<View>(new View.Text([lane.Label]) { Role = Roles.Label, MaxLines = 1, Width = Sizing.Grow.Unit, Conditions = lane.Muted ? [Condition.Disabled] : [] })
            + Toggles(lane)
            + Seq(Flag(Roles.Lock, lane.Locked, locked => lane with { Locked = locked }), Flag(Roles.Visibility, !lane.Muted, audible => lane with { Muted = !audible }))) {
            Role = Roles.Lane,
            Accessible = new Accessible { Role = AccessRole.TreeItem, Label = lane.Label, Expanded = row.Open },
        };
    }

    private View Flag(Key glyph, bool on, Func<bool, Lane<TItem>> lane) =>
        new Toggle(on, edit => Edited(edit.Map(lane).As())) {
            Content = [Toggle.Glyphed(glyph, Check.Of(on))],
            Kind = ToggleKind.Button,
        }.Drawn;

    private View.Pin Faced(Lane<TItem> lane, TItem item, HandlePoint point, bool lead) =>
        lead
            ? Timed.Get(item) switch {
                var timing => new View.Pin(point.At, Face.Map(face => face(item)).IfNone(static () => new View.Box())) {
                    Role = timing.Role,
                    Span = timing.Switch(clip: static clip => Some(new Vector2(clip.Span.Span, (float)Fraction.MaxValue)), point: static _ => None),
                    Align = UnitPoint.Center,
                    Conditions = toHashSet(Seq(Some(Condition.Selected).Filter(_ => Selection.Holds(Keyed(item))), Some(Condition.Disabled).Filter(_ => lane.Muted)).Somes()),
                },
            }
            : new View.Pin(point.At, new View.Box()) { Role = Roles.Grip, Align = UnitPoint.Center };

    // --- [HANDLES]

    private Points<Seq<TItem>> Handled(HashMap<Key, (int Item, int Handle)> slots) =>
        new(items => Placed(items).Map(static held => held.Point), (items, targets, _) =>
            toHashMap(targets.Map(target => (Slot: slots[target.Key], target.At)).GroupBy(static target => target.Slot.Item).Select(static group => (group.Key, Moves: toSeq(group)))) switch {
                var grouped => (items.Map((item, index) => grouped.Find(index)
                    .Map(moves => Timed.Set(moves.Fold(Timed.Get(item), static (timing, move) => timing.Moved(move.Slot.Handle, move.At.X)), item)).IfNone(item)), HashMap<Key, Option<Key>>()),
            });

    private Seq<(HandlePoint Point, int Item, int Handle)> Placed(Seq<TItem> items) =>
        items.Map((item, index) => Timed.Get(item).Switch<Seq<float>>(clip: static clip => [clip.Span.Mid, clip.Span.From, clip.Span.To], point: static point => [point.At])
                .Map((along, handle) => (At: new Vector2(along, (float)Fraction.Half), Item: index, Handle: handle))).Flatten()
            .Map(static (held, index) => (new HandlePoint(new Key.Ordinal((uint)index), held.At), held.Item, held.Handle));

    // --- [READS]

    private static View Needle(float at) => new View.Mark(new Figure.Path([new Vector2(at, (float)Fraction.MinValue), new Vector2(at, (float)Fraction.MaxValue)])) { Role = Roles.Needle };
}
