using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed partial class Scene {
    // --- [DAMAGE]
    private sealed record WalkState(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now, bool Restyled, HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places, Seq<SKRect> Damage);

    private static WalkState Walk(LayoutNode before, LayoutNode after, Place was, Place now, WalkState state) {
        bool same = ReferenceEquals(before, after);
        bool moved = state.Restyled || was != now || before.Node.Plotted != after.Node.Plotted || before.Node.Opacity != after.Node.Opacity;
        if (same && !moved) return state;
        if (moved) return Added(after, now, Removed(before, was, state));
        bool own = before.Frame != after.Frame || before.Corners != after.Corners || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.Node.Equals(after.Node));
        return Matched(before.Children, after.Children, child => Within(before, child, was, state.Was), child => Within(after, child, now, state.Now),
            state with { Index = state.Index.AddOrUpdate(after.Path, after), Damage = own ? state.Damage.Add(Painted(before, was)).Add(Painted(after, now)) : state.Damage });
    }

    private static WalkState Matched(Seq<LayoutNode> before, Seq<LayoutNode> after, Func<LayoutNode, Place> was, Func<LayoutNode, Place> now, WalkState state) {
        HashMap<Seq<Key>, LayoutNode> prior = toHashMap(before.Map(static held => (held.Path, held)));
        HashSet<Seq<Key>> kept = toHashSet(after.Map(static held => held.Path));
        WalkState walked = after.Fold(state, (held, laid) => prior.Find(laid.Path).Match(
            Some: matched => Walk(matched, laid, was(matched), now(laid), held),
            None: () => Added(laid, now(laid), held)));
        return before.Filter(held => !kept.Contains(held.Path)).Fold(walked, (held, gone) => Removed(gone, was(gone), held));
    }

    private static WalkState Added(LayoutNode layout, Place place, WalkState state) =>
        state with { Index = state.Index.AddOrUpdateRange(Indexed(layout).AsIterable()), Places = Placing(layout, place, state.Now, state.Places), Damage = state.Damage.Concat(Extents(layout, place)) };

    private static WalkState Removed(LayoutNode layout, Place place, WalkState state) {
        Seq<Seq<Key>> paths = Preorder(layout).Map(static held => held.Path);
        return state with { Index = state.Index.RemoveRange(paths), Places = state.Places.RemoveRange(paths), Damage = state.Damage.Concat(Extents(layout, place)) };
    }

    private static Seq<SKRect> Extents(LayoutNode layout, Place place) =>
        SKRect.Intersect(Absolute(layout.SubtreeBounds, place.Origin), place.Clip) switch {
            var reach when reach.IsEmpty => [],
            var reach => [place.Transform.MapRect(reach)],
        };

    private static SKRect Painted(LayoutNode layout, Place place) => place.Transform.MapRect(SKRect.Intersect(Absolute(layout.Painted, place.Origin), place.Clip));

    private static Option<SKRect> Reached(HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places, HashMap<Seq<Key>, float> offsets, Seq<Key> plot, LayoutNode fragment) =>
        from layout in index.Find(plot)
        from place in places.Find(plot)
        from reach in Extents(fragment, Within(layout, fragment, place, offsets)).Head
        select reach;

    private static LayoutNode Fragmented(Layouts.Pass pass, LayoutNode plot, Node overlay, Option<LayoutNode> held) =>
        Layouts.WithCorners(Layouts.Framed(pass, Layouts.Built(pass, plot.Inner, plot.Path.Add(Overlaid), overlay, held), SKRect.Create(plot.Frame.Size)), None);

    // --- [PLACEMENT]
    private static readonly Key Overlaid = new Key.Named("fragment");

    internal static Place Root(LayoutNode layout, SKRect window) => Turned(layout, new Place(SKPoint.Empty, window, None));

    private static Seq<LayoutNode> Declared(LayoutNode root, Seq<LayoutNode> layers) => LayerStack.Declared(root) + layers.Bind(LayerStack.Declared);

    internal static HashMap<Seq<Key>, LayoutNode> Indexed(LayoutNode layout) => toHashMap(Preorder(layout).Map(static held => (held.Path, held)));

    internal static HashMap<Seq<Key>, Place> Placing(LayoutNode layout, Place place, HashMap<Seq<Key>, float> offsets, HashMap<Seq<Key>, Place> placed) =>
        layout.Children.Fold(placed.AddOrUpdate(layout.Path, place), (held, child) => Placing(child, Within(layout, child, place, offsets), offsets, held));

    private static Place Within(LayoutNode parent, LayoutNode child, Place place, HashMap<Seq<Key>, float> offsets) {
        SKRect frame = Absolute(parent.Frame, place.Origin);
        SKPoint origin = frame.Location + Scrolled(parent, child, offsets);
        return Turned(child, new Place(origin + Stuck(parent, child, place.Clip, origin), parent.Node.Clips(child.Node) ? SKRect.Intersect(place.Clip, frame) : place.Clip,
            Layouts.Plotted(parent).Map(own => own.Translated(frame.Location)) | place.Plot) { Transform = place.Transform });
    }

    private static Place Turned(LayoutNode layout, Place place) =>
        layout.Node.Transform.Map(turn => place with { Transform = place.Transform.PreConcat(((SKMatrix44)layout.Turned(turn, Absolute(layout.Frame, place.Origin).Location)).Matrix) }).IfNone(place);

    private static SKPoint Stuck(LayoutNode parent, LayoutNode child, SKRect clip, SKPoint origin) =>
        child.Node.Sticks.Map(axis => {
            float start = axis.Of(origin + child.Frame.Location);
            float limit = toSeq(parent.Children.SkipWhile(sibling => !ReferenceEquals(sibling, child)).Skip(1)).Find(sibling => sibling.Node.Sticks == Some(axis))
                .Map(next => axis.Of(origin + next.Frame.Location)).IfNone(() => axis.Of(origin) + axis.End(parent.ContentBox));
            return (SKPoint)axis.With(Vector2.Zero, float.Clamp(axis.Of(clip.Location), start, float.Max(start, limit - axis.Of(child.Frame.Size))) - start);
        }).IfNone(SKPoint.Empty);

    private static SKPoint Scrolled(LayoutNode parent, LayoutNode child, HashMap<Seq<Key>, float> offsets) =>
        (from scroller in Scroller(parent, None, offsets)
         where !Layouts.IsHandle(child)
         select (SKPoint)scroller.Axis.With(Vector2.Zero, -scroller.Value)).IfNone(SKPoint.Empty);

    internal static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);
}
