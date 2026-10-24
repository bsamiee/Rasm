using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Laid {
    public required Seq<Key> Path { get; init; }
    public required Node Node { get; init; }
    public required Cascade Cascade { get; init; }
    public required Style Style { get; init; }
    public SKSize Fit { get; init; }
    public SKRect Frame { get; init; }
    public Seq<Laid> Children { get; init; }

    public virtual SKRect Painted => SKRect.Inflate(Frame, Overhang, Overhang);

    public SKRect Reach =>
        Children.Filter(child => !Scene.Clips(this, child)).Fold(Painted, (union, child) => child.Reach switch {
            var reach when reach.IsEmpty => union,
            var reach => SKRect.Create(reach.Location + Frame.Location, reach.Size) switch {
                var moved when union.IsEmpty => moved,
                var moved => SKRect.Union(union, moved),
            },
        });

    public bool Timed => Node.Role.IsSome && Style.Look.Fill.Exists(static fill => fill is Material.Drawn { Timed: true });

    private float Overhang => Node.Role.IsSome || Node is Node.Mark ? Style.Overhang : 0f;

    public sealed record Box : Laid;

    public sealed record Text(Typeset Typeset, TextStyle TextStyle) : Laid {
        public SKPoint Origin { get; init; }

        public override SKRect Painted =>
            SKRect.Create(Frame.Location + Origin, Typeset.Size) switch {
                var set => SKRect.Union(base.Painted, new SKRect(set.Left - float.Max(Overhang, Typeset.Overhang.Left), set.Top - float.Max(Overhang, Typeset.Overhang.Top),
                    set.Right + float.Max(Overhang, Typeset.Overhang.Right), set.Bottom + float.Max(Overhang, Typeset.Overhang.Bottom))),
            };
    }

    public sealed record Choice(Seq<Laid> Alternatives) : Laid;
}

public sealed class Scene {
    private Scene(Laid layout, HashMap<Seq<Key>, Laid> index, HashMap<Seq<Key>, float> adjusted, HashMap<Style, TextStyle> texts) {
        Layout = layout;
        Index = index;
        Adjusted = Clamped(adjusted, index);
        Texts = texts;
    }

    public Laid Layout { get; }
    public HashMap<Seq<Key>, Laid> Index { get; }
    public HashMap<Seq<Key>, float> Adjusted { get; }
    public Seq<Laid> Order => Preorder(Layout);
    public Seq<(SKRect Frame, SKRect Clip, Laid Laid)> Hosted => Hosting(Layout, Root);
    internal HashMap<Style, TextStyle> Texts { get; }

    private (SKPoint Origin, SKRect Clip) Root => (SKPoint.Empty, SKRect.Create(Layout.Frame.Size));

    // --- [TRANSITION]
    public static Fin<Scene> Of(Theme theme, Shipped shipped, Node root, SKSize size, HashMap<Seq<Key>, float> adjusted) =>
        Layouts.Arrange(Styles.Root(theme, shipped), root, size, adjusted, [], []) switch {
            var (laid, texts) => Layouts.Unique(laid, []).Map(unique => new Scene(unique, toHashMap(Preorder(unique).Map(static held => (held.Path, held))), adjusted, texts)),
        };

    public Fin<(Scene Scene, Seq<SKRect> Damage)> Reconcile(Node root) =>
        Layouts.Arrange(Layout.Cascade, root, Layout.Frame.Size, Adjusted, Index, Texts) switch {
            var (laid, texts) => Layouts.Unique(laid, Index).Map(unique => Settled(unique, Adjusted, texts)),
        };

    public (Scene Scene, Seq<SKRect> Damage) Resize(SKSize size) => Arranged(Layout.Cascade, size, Adjusted, Index, Texts);

    public (Scene Scene, Seq<SKRect> Damage) Restyle(Theme theme) => Arranged(Styles.Root(theme, Layout.Cascade.Shipped), Layout.Frame.Size, Adjusted, [], []);

    private (Scene Scene, Seq<SKRect> Damage) Arranged(Cascade root, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> prior, HashMap<Style, TextStyle> texts) =>
        Layouts.Arrange(root, Layout.Node, size, adjusted, prior, texts) switch {
            var (laid, held) => Settled(laid, adjusted, held),
        };

    private (Scene Scene, Seq<SKRect> Damage) Settled(Laid next, HashMap<Seq<Key>, float> adjusted, HashMap<Style, TextStyle> texts) =>
        Walk(new Diff(Adjusted, adjusted), Layout, next, Root, (SKPoint.Empty, SKRect.Create(next.Frame.Size)), (Index, Seq<SKRect>())) switch {
            var (index, damage) => (new Scene(next, index, adjusted, texts), damage),
        };

    private (Scene Scene, Seq<SKRect> Damage) Readjusted(HashMap<Seq<Key>, float> adjusted, Seq<Seq<Key>> changed) =>
        Arranged(Layout.Cascade, Layout.Frame.Size, adjusted, Index.RemoveRange(changed.Bind(Above)), Texts);

    public (Scene Scene, Seq<SKRect> Damage) Adjust(Seq<Key> path, float delta) =>
        (from held in Held(path)
         let moved = held.Value + (held.Gearing * delta)
         let value = held.Owner.Node is Node.Scroll scroll ? Scrolling(held.Owner, scroll.Axis, moved) : moved
         where value.CompareTo(held.Value) is not 0
         select Readjusted(Adjusted.AddOrUpdate(held.Owner.Path, value), Seq(held.Owner.Path)))
        .IfNone((this, Seq<SKRect>()));

    private Option<(Laid Owner, float Value, float Gearing)> Held(Seq<Key> path) =>
        from laid in Index.Find(path)
        from owner in Adjustable(laid.Path) ? Index.Find(laid.Path.Init) : Some(laid)
        from value in Extent(owner, Adjusted.Find(owner.Path).IfNone(0f))
        select (owner, value, laid.Path.Last.Exists(static last => last is Key.Thumb) && owner.Node is Node.Scroll scroll ? Layouts.Scrollable(owner, owner.Children[0], scroll.Axis) switch {
            var (total, viewport) => Layouts.Rescaled(1f, viewport - scroll.Axis.Of(laid.Frame.Size), total - viewport),
        } : 1f);

    public (Scene Scene, Seq<SKRect> Damage) Reveal(Seq<Key> path) =>
        Above(path).Tail.Choose(Index.Find)
            .Choose(static laid => laid.Node is Node.Scroll scroll ? Some((Laid: laid, scroll.Axis)) : None) switch {
                var scrolls => Readjusted(scrolls.Fold(Adjusted, (adjusted, scroll) => Revealed(adjusted, scroll.Laid, scroll.Axis, path)), scrolls.Map(static scroll => scroll.Laid.Path)),
            };

    private HashMap<Seq<Key>, float> Revealed(HashMap<Seq<Key>, float> adjusted, Laid scroll, Orientation axis, Seq<Key> path) =>
        (from target in Located(path, adjusted)
         from viewport in Located(scroll.Path, adjusted).Map(frame => SKRect.Inflate(frame, -Layouts.Inset(scroll), -Layouts.Inset(scroll)))
         let low = (Target: axis.Of(target.Location), View: axis.Of(viewport.Location))
         let high = (Target: axis.Of(new SKPoint(target.Right, target.Bottom)), View: axis.Of(new SKPoint(viewport.Right, viewport.Bottom)))
         let delta = low.Target < low.View ? low.Target - low.View : high.Target > high.View ? float.Min(high.Target - high.View, low.Target - low.View) : 0f
         select adjusted.AddOrUpdate(scroll.Path, Scrolling(scroll, axis, adjusted.Find(scroll.Path).IfNone(0f) + delta))).IfNone(adjusted);

    // --- [QUERY]
    public static Seq<Seq<Key>> Above(Seq<Key> path) => toSeq(Range(0, path.Count + 1)).Rev().Map(path.Take);

    public Option<float> Adjustment(Seq<Key> path) => Held(path).Map(static held => held.Gearing > 0f ? held.Value / held.Gearing : 0f);

    public Option<SKRect> Frame(Seq<Key> path) => Located(path, Adjusted);

    public SKSize Preferred(SKSize offered) => Layouts.Preferred(Layout, offered);

    public Option<SKRect> Bounds(Seq<Key> path) => Placement(path, Adjusted).Map(static held => Painted(held.Laid, held.Place));

    public Seq<(Laid Laid, SKPoint Local)> Hit(SKPoint at) => Hits(Layout, Root, None, at);

    public bool Covers(SKPoint at) => Covering(Layout, Root, at);

    internal SKPoint Shift(Laid parent, Laid child) => Shift(parent, child, Adjusted);

    internal static bool Adjustable(Seq<Key> path) => path.Last.Exists(static last => last is Key.Divider or Key.Thumb);

    private Option<SKRect> Located(Seq<Key> path, HashMap<Seq<Key>, float> adjusted) => Placement(path, adjusted).Map(static held => Absolute(held.Laid.Frame, held.Place.Origin));

    private Option<(Laid Laid, (SKPoint Origin, SKRect Clip) Place)> Placement(Seq<Key> path, HashMap<Seq<Key>, float> adjusted) =>
        Range(0, path.Count).Fold(Index.Find([]).Map(root => (Laid: root, Place: Root)), (place, depth) =>
            from held in place
            from child in Index.Find(path.Take(depth + 1))
            select (Laid: child, Place: Within(held.Laid, child, held.Place, adjusted)));

    private Seq<(Laid Laid, SKPoint Local)> Hits(Laid laid, (SKPoint Origin, SKRect Clip) place, Option<(PlotPlane Plane, SKMatrix Frame)> plane, SKPoint at) {
        if (!place.Clip.Contains(at)) return [];
        SKRect frame = Absolute(laid.Frame, place.Origin);
        Option<(PlotPlane Plane, SKMatrix Frame)> inner = Layouts.Plotted(laid).Map(plot => (plot.Plane, Frame: plot.Frame.PostConcat(SKMatrix.CreateTranslation(frame.Left, frame.Top)))) | plane;
        Seq<(Laid Laid, SKPoint Local)> deeper = laid.Children.Rev().Map(child => Hits(child, Within(laid, child, place, Adjusted), inner, at)).Find(static hits => !hits.IsEmpty).IfNone([]);
        float reach = laid.Style.Metrics[Metric.Reach];
        bool reached = (laid.Node.Key.IsSome || laid.Node.Part.IsSome) && (laid.Node is Node.Mark mark
            ? plane.Exists(held => mark.Figure.Hits(held.Plane, held.Frame, at, reach + laid.Style.HalfStroke))
            : laid.Node is Node.Pin || Adjustable(laid.Path)
                ? SKRect.Inflate(frame, reach, reach).Contains(at)
                : frame.Contains(at));
        return reached ? deeper.Add((laid, at - frame.Location)) : deeper;
    }

    private bool Covering(Laid laid, (SKPoint Origin, SKRect Clip) place, SKPoint at) =>
        place.Clip.Contains(at)
        && ((Absolute(laid.Frame, place.Origin).Contains(at)
                && ((laid.Node.Role.IsSome && (laid.Style.Look.Fill.IsSome || laid.Style.Look.Stroke.IsSome)) || laid.Node is Node.Text or Node.Glyph or Node.Image or Node.Mark or Node.Slot))
            || laid.Children.Exists(child => Covering(child, Within(laid, child, place, Adjusted), at)));

    private Seq<(SKRect Frame, SKRect Clip, Laid Laid)> Hosting(Laid laid, (SKPoint Origin, SKRect Clip) place) =>
        (laid.Node is Node.Slot || (laid.Node.Role.IsSome && laid.Style.Look.Fill.Exists(static fill => fill is Material.Backdrop))
                ? Seq((Absolute(laid.Frame, place.Origin), place.Clip, laid))
                : [])
            .Concat(laid.Children.Bind(child => Hosting(child, Within(laid, child, place, Adjusted))));

    // --- [DAMAGE]
    private sealed record Diff(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now);

    private static (HashMap<Seq<Key>, Laid> Index, Seq<SKRect> Damage) Walk(Diff diff, Laid before, Laid after, (SKPoint Origin, SKRect Clip) was, (SKPoint Origin, SKRect Clip) now,
                                                                         (HashMap<Seq<Key>, Laid> Index, Seq<SKRect> Damage) state) {
        bool same = ReferenceEquals(before, after);
        bool moved = was != now || Planes(before) != Planes(after) || Alpha(before).CompareTo(Alpha(after)) is not 0 || !before.Cascade.Theme.Equals(after.Cascade.Theme);
        if (same && !moved) return state;
        if (moved) {
            return (same ? state.Index : state.Index.RemoveRange(Preorder(before).Map(static held => held.Path)).AddOrUpdateRange(Preorder(after).Map(static held => (held.Path, held))),
                state.Damage.Concat(Extents(before, was)).Concat(Extents(after, now)));
        }
        bool own = before.Frame != after.Frame || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.Node.Equals(after.Node));
        HashMap<Seq<Key>, Laid> prior = toHashMap(before.Children.Map(static child => (child.Path, child)));
        LanguageExt.HashSet<Seq<Key>> kept = toHashSet(after.Children.Map(static child => child.Path));
        (HashMap<Seq<Key>, Laid> Index, Seq<SKRect> Damage) walked = after.Children.Fold(
            (state.Index.AddOrUpdate(after.Path, after), own ? state.Damage.Add(Painted(before, was)).Add(Painted(after, now)) : state.Damage),
            (held, child) => prior.Find(child.Path).Match(
                Some: matched => Walk(diff, matched, child, Within(before, matched, was, diff.Was), Within(after, child, now, diff.Now), held),
                None: () => (held.Item1.AddOrUpdateRange(Preorder(child).Map(static added => (added.Path, added))), held.Item2.Concat(Extents(child, Within(after, child, now, diff.Now))))));
        return before.Children.Filter(child => !kept.Contains(child.Path)).Fold(walked, (held, child) =>
            (held.Index.RemoveRange(Preorder(child).Map(static removed => removed.Path)), held.Damage.Concat(Extents(child, Within(before, child, was, diff.Was)))));
    }

    private static Seq<SKRect> Extents(Laid laid, (SKPoint Origin, SKRect Clip) place) =>
        SKRect.Intersect(Absolute(laid.Reach, place.Origin), place.Clip) switch {
            var reach when reach.IsEmpty => [],
            var reach => [reach],
        };

    private static SKRect Painted(Laid laid, (SKPoint Origin, SKRect Clip) place) => SKRect.Intersect(Absolute(laid.Painted, place.Origin), place.Clip);

    private static Option<PlotPlane> Planes(Laid laid) => laid.Node is Node.Plot plot ? Some(plot.Plane) : None;

    private static float Alpha(Laid laid) => laid.Node is Node.Styled styled ? (float)styled.Opacity : 1f;

    // --- [PLACEMENT]
    private static (SKPoint Origin, SKRect Clip) Within(Laid parent, Laid child, (SKPoint Origin, SKRect Clip) place, HashMap<Seq<Key>, float> adjusted) =>
        Absolute(parent.Frame, place.Origin) switch {
            var frame => (frame.Location + Shift(parent, child, adjusted), Clips(parent, child) ? SKRect.Intersect(place.Clip, frame) : place.Clip),
        };

    internal static bool Clips(Laid parent, Laid child) => parent.Node is Node.Scroll || (parent.Node is Node.Plot && Layouts.Pinned(child.Node).IsNone);

    private static SKPoint Shift(Laid parent, Laid child, HashMap<Seq<Key>, float> adjusted) =>
        parent.Node is Node.Scroll scroll && !child.Path.Last.Exists(static last => last is Key.Thumb)
            ? scroll.Axis.With(Vector2.Zero, -Scrolling(parent, scroll.Axis, adjusted.Find(parent.Path).IfNone(0f)))
            : SKPoint.Empty;

    private static float Scrolling(Laid scroll, Orientation axis, float offset) =>
        Layouts.Scrollable(scroll, scroll.Children[0], axis) switch {
            var (total, viewport) => Layouts.Scrolling(offset, total, viewport),
        };

    private static Option<float> Extent(Laid laid, float stored) =>
        laid.Node is Node.Scroll scroll ? Some(Scrolling(laid, scroll.Axis, stored)) : laid.Node is Node.Split split ? Some(Layouts.Pane(laid, split)) : None;

    private static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);

    private static HashMap<Seq<Key>, float> Clamped(HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> index) =>
        toHashMap(toSeq(adjusted.AsIterable()).Choose(entry => index.Find(entry.Key).Bind(laid => Extent(laid, entry.Value)).Map(value => (entry.Key, value))));

    internal static Seq<Laid> Preorder(Laid laid) => laid.Cons(laid.Children.Bind(Preorder));
}
