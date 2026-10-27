using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LayoutNode {
    public virtual bool Equals([NotNullWhen(true)] LayoutNode? other) => ReferenceEquals(this, other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    internal required Seq<Key> Path { get; init; }
    internal required Node Node { get; init; }
    internal required Cascade Cascade { get; init; }
    internal required Cascade Inner { get; init; }
    internal required RoleStyle Style { get; init; }
    internal SKSize Intrinsic { get; init; }
    internal SKRect Frame { get; init; }
    internal SKRect Data { get; init; }
    internal Corners Corners { get; init; } = Corners.Square;
    internal Seq<LayoutNode> Children { get; init; }
    internal Seq<LayoutNode> Members { get; init; }
    internal Seq<LayoutNode> Layers { get; init; }

    internal SKRect ContentBox {
        get {
            float inset = Inset;
            return SKRect.Create(inset, inset, float.Max(0f, Frame.Width - (2f * inset)), float.Max(0f, Frame.Height - (2f * inset)));
        }
    }

    internal virtual SKRect Painted => SKRect.Inflate(Frame, Overhang, Overhang);

    internal SKRect SubtreeBounds =>
        Children.Filter(child => !Node.Clips(child.Node)).Fold(Painted, (union, child) => {
            SKRect reach = child.SubtreeBounds;
            SKRect moved = SKRect.Create(reach.Location + Frame.Location, reach.Size);
            return reach.IsEmpty ? union : union.IsEmpty ? moved : SKRect.Union(union, moved);
        });

    internal SKRect LocalSubtreeBounds {
        get {
            SKRect reach = SubtreeBounds;
            return SKRect.Create(reach.Location - Frame.Location, reach.Size);
        }
    }

    internal bool Timed =>
        (Node.Role.IsSome && Style.Timed) || Node.GlyphArt.Exists(static art => art is GlyphSource.Motion) || Node is Node.Image { Source: ImageSource.Encoded { FrameCount: > 1 } };

    internal Option<Matrix4x4> Transform =>
        Node.Transform.Map(turn => new Vector2(Frame.Width * (float)Node.Align.X, Frame.Height * (float)Node.Align.Y) switch {
            var pivot => Matrix4x4.CreateTranslation(-pivot.X, -pivot.Y, 0f) * turn * Matrix4x4.CreateTranslation(pivot.X, pivot.Y, 0f),
        });

    internal float Inset => Node.Role.IsSome ? Style.Metrics[Metric.Inset] : 0f;

    internal float Smoothing => Node.Role.IsSome ? Style.Metrics[Metric.Smoothing] : 0f;

    internal bool HasBackground => Node.Role.IsSome && Node.Painting.AcceptsBackground && (!Style.Fills.IsEmpty || !Style.Strokes.IsEmpty);

    internal Option<Text> TextNode => Switch(box: static _ => Option<Text>.None, text: static text => Some(text));

    internal float Alpha(SKColorSpace target) => (Style.Dimmed && !Cascade.Enclosing.Dimmed ? Cascade.Theme.DisabledOpacity(target) : 1f) * Node.Alpha;

    private float Overhang => Node.Role.IsSome || Node.Traced.IsSome ? Style.Overhang : 0f;

    internal sealed record Box : LayoutNode;

    internal sealed record Text(Node.Text Source, Typeset Typeset, TextStyle TextStyle) : LayoutNode {
        internal SKPoint Origin { get; init; }
        internal float MinContent { get; init; }

        internal override SKRect Painted {
            get {
                SKRect set = SKRect.Create(Frame.Location + Origin, Typeset.Size);
                SKRect painted = Typeset.Overhang;
                return SKRect.Union(base.Painted, new SKRect(set.Left - float.Max(Overhang, painted.Left), set.Top - float.Max(Overhang, painted.Top),
                    set.Right + float.Max(Overhang, painted.Right), set.Bottom + float.Max(Overhang, painted.Bottom)));
            }
        }
    }
}

internal sealed record Adjustment(Seq<Key> Owner, Seq<Key> Stored, PhysicalAxis Axis, float Value, Layouts.Overflow Overflow, float Gearing, float Step, float Page) {
    internal Seq<float> Stops { get; init; }

    internal float Snapped(float offset) => Stops.IsEmpty ? offset : Stops.MinBy(stop => MathF.Abs(stop - offset));

    internal Option<float> Navigated(NavigationDirection toward) =>
        toward.Along == Axis ? Some(toward.Granularity.Distance(Step, Page).Match(Some: stride => Value + (toward.Sign * stride), None: () => toward.Sign < 0 ? 0f : Overflow.Max)) : None;

    internal bool Scrolls(Vector2 delta) => Overflow.Max > 0f && MathF.Abs(Axis.Of(delta)) >= MathF.Abs(Axis.Cross.Of(delta));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hosted {
    public required Seq<Key> Path { get; init; }
    public required SKRect Frame { get; init; }
    public required SKRect Clip { get; init; }

    public sealed record Slot(SlotKind Kind) : Hosted;

    public sealed record Backdrop(Corners Corners, bool Clear, Option<SKColorF> Tint) : Hosted {
        public float Spacing { get; init; }
    }

    public sealed record Popup(Corners Corners) : Hosted;
}

internal sealed record Place(SKPoint Origin, SKRect Clip, Option<PlotFrame> Plot) {
    private readonly Option<SKMatrix> inverse = Some(SKMatrix.Identity);

    internal SKMatrix Transform { get; init { field = value; inverse = value.TryInvert(out SKMatrix inverted) ? Some(inverted) : None; } } = SKMatrix.Identity;

    internal Option<SKPoint> Local(SKPoint at) => inverse.Map(held => held.MapPoint(at));
}

internal sealed record PartOwner(Seq<Key> Path, LayoutNode Layout, Part Part, SKRect Bounds) {
    internal bool Enabled => !Layout.Style.Conditions.Contains(Condition.Disabled);
}

internal sealed record PartHit(PartOwner Owner, Seq<Key> Hit);

internal sealed record HitNode(LayoutNode Layout, SKPoint Local);

internal sealed record PlacedNode(LayoutNode Layout, Place Place) {
    internal RoleStyle Style { get; init; } = Layout.Style;

    internal SKRect Frame => Scene.Absolute(Layout.Frame, Place.Origin);
}

internal sealed record SceneChange(Scene Scene, Seq<SKRect> Damage);

public sealed class Scene {
    // --- [STATE]
    private sealed record Sampling(SKRect Reach, SKRect Bounds);

    private readonly Func<OutlineKey, Fin<Outline>> outlines = memo<OutlineKey, Fin<Outline>>(static key => Outline.Of(key.Shape, key.Corners, key.Smoothing, key.Frame, key.Offset));
    private readonly HashSet<Seq<Key>> opened;
    private readonly HashMap<Seq<Key>, Seq<Seq<Key>>> readers;
    private readonly Seq<Sampling> sampling;

    private Scene(Host host, Layouts.Pass pass, LayoutNode root, Seq<LayoutNode> layers, HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places, HashSet<Seq<Key>> opened, HashMap<Seq<Key>, LayoutNode> fragments) {
        (Host, Shaping, ColorCache, Layout, Layers, Index, Places, Fragments, this.opened) = (host, pass.Shaping, pass.ColorCache, root, layers, index, places, fragments, opened);
        Order = Preorder(root) + layers.Bind(Preorder);
        readers = Order.Fold(HashMap<Seq<Key>, Seq<Seq<Key>>>(), static (held, layout) =>
            layout.Node is Node.Scroll scroll ? held.AddOrUpdate(Layouts.Stored(layout, scroll), Some: paths => paths.Add(layout.Path), None: () => [layout.Path]) : held);
        Adjusted = toHashMap(toSeq(pass.Adjusted.AsIterable()).Choose(entry => readers.Find(entry.Key).IfNone([]).Choose(index.Find).Choose(layout => Adjusting(layout, None, pass.Adjusted)).Head.Map(adjuster => (entry.Key, adjuster.Value))));
        Timed = Order.Filter(static layout => layout.Timed);
        Inert = LayerStack.Inert(root, layers);
        Hosted = Order.Fold(Seq<Hosted>(), (hosted, layout) => Places.Find(layout.Path).Map(place => hosted + Hosting(hosted, new PlacedNode(layout, place))).IfNone(hosted))
            + layers.Choose(layer => LayerStack.Popup(layer, SKRect.Create(root.Frame.Size)));
        HashSet<Seq<Key>> elected = toHashSet(Hosted.Filter(static hosted => hosted is Hosted.Backdrop).Map(static hosted => hosted.Path));
        sampling = Order.Choose(layout =>
            from glass in layout.Style.Glass.Filter(_ => layout.Node.Role.IsSome && !elected.Contains(layout.Path))
            from bounds in Bounds(layout.Path)
            let reach = (3f * MathF.Sqrt(glass.Backdrop.Fold(0f, static (sum, filter) => filter is Filter.Css { Amount: var sigma } css && css.Function == FilterFunction.Blur ? sum + (sigma * sigma) : sum))) + glass.Refraction.IfNone(0f)
            select new Sampling(SKRect.Inflate(bounds, reach, reach), bounds));
        Extended = Order.Exists(static layout => layout.Node is Node.Image { Source.Info: var info } && (info.ColorType is SKColorType.RgbaF16 or SKColorType.RgbaF32 || info.ColorSpace is { IsSrgb: false }));
    }

    private Scene(Scene prior, HashMap<Seq<Key>, LayoutNode> fragments) {
        (Host, Shaping, ColorCache, Layout, Layers, Index, Places, Adjusted, Order, Timed, Inert, Hosted, Extended) =
            (prior.Host, prior.Shaping, prior.ColorCache, prior.Layout, prior.Layers, prior.Index, prior.Places, prior.Adjusted, prior.Order, prior.Timed, prior.Inert, prior.Hosted, prior.Extended);
        (opened, readers, sampling, outlines, Fragments) = (prior.opened, prior.readers, prior.sampling, prior.outlines, fragments);
    }

    public HashMap<Seq<Key>, float> Adjusted { get; }
    public Seq<Hosted> Hosted { get; }
    public bool Extended { get; }
    internal Host Host { get; }
    internal Shaping Shaping { get; }
    internal ColorCache ColorCache { get; }
    internal LayoutNode Layout { get; }
    internal Seq<LayoutNode> Layers { get; }
    internal HashMap<Seq<Key>, LayoutNode> Index { get; }
    internal HashMap<Seq<Key>, Place> Places { get; }
    internal HashMap<Seq<Key>, LayoutNode> Fragments { get; }
    internal Seq<LayoutNode> Order { get; }
    internal Seq<LayoutNode> Timed { get; }
    internal HashSet<Seq<Key>> Inert { get; }

    // --- [TRANSITIONS]
    internal static Fin<Scene> Of(Cascade root, Node view, Host host, HashMap<Seq<Key>, float> adjusted, HashSet<Seq<Key>> opened, Shaping shaping) {
        Layouts.Pass pass = new([], adjusted, shaping, new ColorCache(), host.Culture);
        return Layouts.Unique(Layouts.Arrange(pass, root, view, host.Size), []).Map(main => Laid(host, pass, main, opened, None).Scene);
    }

    internal Fin<SceneChange> Reconcile(Node root) {
        Layouts.Pass pass = new(Index, Adjusted, Shaping, new ColorCache(), Host.Culture);
        return Layouts.Unique(Layouts.Arrange(pass, Layout.Cascade, root, Host.Size), Index).Map(main => Laid(Host, pass, main, opened, Some(this)));
    }

    internal SceneChange Resize(Host host) => Arranged(Layout.Cascade, host, Adjusted, host.Culture.Equals(Host.Culture) ? Index : [], opened);

    internal SceneChange Restyle(Host host) => Arranged(Styles.Root(host.Theme, Layout.Cascade.Defaults, host.Placement.Root), host, Adjusted, [], opened);

    internal SceneChange Opened(HashSet<Seq<Key>> next) => next == opened ? Unchanged : Arranged(Layout.Cascade, Host, Adjusted, Index, next);

    internal SceneChange Unchanged => new(this, []);

    internal SceneChange Adjust(Seq<Key> path, Func<float, float> move) =>
        (from adjuster in Adjustment(path)
         let value = adjuster.Overflow.Clamped(move(adjuster.Value))
         where value.CompareTo(adjuster.Value) is not 0
         select Readjusted(Adjusted.AddOrUpdate(adjuster.Stored, value), Seq(adjuster.Stored)))
        .IfNone(Unchanged);

    internal SceneChange Reveal(Seq<Key> path) =>
        (Frame(path) | Slotted(path)).Map(target => AncestorsAndSelf(path).Tail.Choose(Adjustment).Fold(
                (Adjusted, Target: target, Moved: Seq<Seq<Key>>()),
                (held, adjuster) => Revealing(adjuster, held.Target).Filter(static moved => moved.CompareTo(0f) is not 0).Match(
                    Some: moved => (held.Adjusted.AddOrUpdate(adjuster.Stored, adjuster.Value + moved), Absolute(held.Target, (SKPoint)adjuster.Axis.With(Vector2.Zero, -moved)), held.Moved.Add(adjuster.Stored)),
                    None: () => held)))
            .Filter(static revealed => !revealed.Moved.IsEmpty)
            .Match(Some: revealed => Readjusted(revealed.Adjusted, revealed.Moved), None: () => Unchanged);

    internal SceneChange Revealed(Option<Scene> prior) =>
        Order.Choose(layout => Lead(layout).Filter(lead => prior.Bind(held => held.Index.Find(layout.Path)).Bind(Lead) != Some(lead)))
            .Fold(Unchanged, static (held, lead) => {
                SceneChange shown = held.Scene.Reveal(lead);
                return shown with { Damage = held.Damage.Concat(shown.Damage) };
            });

    internal SceneChange Fragment(Seq<Key> plot, Option<Node> overlay) {
        Option<LayoutNode> was = Fragments.Find(plot);
        Option<LayoutNode> now = from node in overlay from layout in Index.Find(plot) select Fragmented(Pass(Index), layout, node, was);
        return new SceneChange(new Scene(this, now.Match(Some: laid => Fragments.AddOrUpdate(plot, laid), None: () => Fragments.Remove(plot))),
            was.Bind(held => Reached(Index, Places, Adjusted, plot, held)).ToSeq() + now.Bind(laid => Reached(Index, Places, Adjusted, plot, laid)).ToSeq());
    }

    private Layouts.Pass Pass(HashMap<Seq<Key>, LayoutNode> prior) => new(prior, Adjusted, Shaping, ColorCache, Host.Culture);

    private SceneChange Arranged(Cascade root, Host host, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, LayoutNode> prior, HashSet<Seq<Key>> next) {
        Layouts.Pass pass = new(prior, adjusted, Shaping, new ColorCache(), host.Culture);
        return Laid(host, pass, Layouts.Arrange(pass, root, Layout.Node, host.Size), next, Some(this));
    }

    private SceneChange Readjusted(HashMap<Seq<Key>, float> adjusted, Seq<Seq<Key>> stored) =>
        Arranged(Layout.Cascade, Host, adjusted, Index.RemoveRange(stored.Bind(key => readers.Find(key).IfNone([])).Bind(AncestorsAndSelf)), opened);

    private static SceneChange Laid(Host host, Layouts.Pass pass, LayoutNode main, HashSet<Seq<Key>> opened, Option<Scene> prior) {
        SKRect window = SKRect.Create(main.Frame.Size);
        SKRect was = prior.Map(static held => SKRect.Create(held.Layout.Frame.Size)).IfNone(window);
        Diff diff = new(prior.Map(static held => held.Adjusted).IfNone(pass.Adjusted), pass.Adjusted, prior.Exists(held => !held.Layout.Cascade.Theme.Equals(main.Cascade.Theme)));
        WalkState walked = Matched(diff, prior.Map(static held => held.Layout).ToSeq(), [main], layout => Root(layout, was), layout => Root(layout, window),
            prior.Map(static held => new WalkState(held.Index, held.Places, [])).IfNone(new WalkState([], [], [])));
        Seq<LayoutNode> layers = LayerStack.Stacked(pass, main, opened, window, walked.Index, walked.Places);
        WalkState stacked = Matched(diff, prior.Map(static held => held.Layers).IfNone([]), layers, layer => Root(layer, was), layer => Root(layer, window), walked);
        Seq<(Seq<Key> Plot, Option<SKRect> Was, LayoutNode Laid)> fragments = prior.Map(held => toSeq(held.Fragments.AsIterable()).Choose(entry =>
            stacked.Index.Find(entry.Key).Map(plot => (entry.Key, Reached(held.Index, held.Places, held.Adjusted, entry.Key, entry.Value), Fragmented(pass, plot, entry.Value.Node, Some(entry.Value)))))).IfNone([]);
        HashSet<Seq<Key>> open = toHashSet(layers.Map(static layer => layer.Path));
        HashMap<Seq<Key>, LayoutNode> index = stacked.Index.RemoveRange(prior.ToSeq().Bind(static held => Declared(held.Layout, held.Layers)).Map(static layer => layer.Path).Filter(path => !open.Contains(path)))
            .AddOrUpdateRange(Declared(main, layers).Filter(layer => !open.Contains(layer.Path)).Map(static layer => (layer.Path, layer)));
        Scene scene = new(host, pass, main, layers, index, stacked.Places, opened, toHashMap(fragments.Map(static held => (held.Plot, held.Laid))));
        return new SceneChange(scene, stacked.Damage + fragments.Bind(held => Reached(stacked.Index, stacked.Places, scene.Adjusted, held.Plot, held.Laid) switch {
            var now when now == held.Was => Seq<SKRect>(),
            var now => held.Was.ToSeq() + now.ToSeq(),
        }));
    }

    private Option<float> Revealing(Adjustment adjuster, SKRect target) =>
        from viewport in Placed(adjuster.Owner).Map(static held => Absolute(held.Layout.ContentBox, held.Frame.Location))
        let low = (Target: adjuster.Axis.Of(target.Location), View: adjuster.Axis.Of(viewport.Location))
        let high = (Target: adjuster.Axis.Of(new SKPoint(target.Right, target.Bottom)), View: adjuster.Axis.Of(new SKPoint(viewport.Right, viewport.Bottom)))
        let delta = low.Target < low.View ? low.Target - low.View : high.Target > high.View ? float.Min(high.Target - high.View, low.Target - low.View) : 0f
        select adjuster.Overflow.Clamped(adjuster.Value + delta) - adjuster.Value;

    private Option<SKRect> Slotted(Seq<Key> path) =>
        from key in path.Last
        from lazy in Placed(path.Init)
        from node in lazy.Layout.Node.Windowed
        from slot in Layouts.Slot(lazy.Layout, node, key)
        select SKRect.Create(slot.Location + lazy.Frame.Location, slot.Size);

    // --- [QUERY]
    public Option<SKRect> Frame(Seq<Key> path) => Placed(path).Map(static held => held.Place.Transform.MapRect(held.Frame));

    public Option<SKRect> Bounds(Seq<Key> path) => Placed(path).Map(static held => Painted(held.Layout, held.Place));

    public Option<SKRect> Visible(Seq<Key> path) => Placed(path).Map(static held => held.Place.Transform.MapRect(SKRect.Intersect(held.Frame, held.Place.Clip))).Filter(static shown => !shown.IsEmpty);

    public bool Covers(SKPoint at) =>
        Order.Exists(layout => (layout.HasBackground || layout.Node.Painting.Paints)
            && Places.Find(layout.Path).Exists(place => place.Local(at).Exists(local => place.Clip.Contains(local) && Holds(layout, place, local, 0f))));

    public SKSize Preferred(SKSize offered) => Layouts.Preferred(Pass([]), Layout, offered);

    internal Seq<SKRect> Sampled(Seq<SKRect> damage) => damage + sampling.Filter(glass => damage.Exists(rect => rect.IntersectsWith(glass.Reach))).Map(static glass => glass.Bounds);

    internal Option<Nearest> Nearest(Seq<Key> plot, SKPoint at) =>
        from layout in Index.Find(plot)
        let data = toHashMap(Preorder(layout).Choose(mark =>
            from figure in mark.Node.Traced
            from key in mark.Path.Last
            from place in Places.Find(mark.Path)
            from frame in place.Plot
            from local in place.Local(at)
            from datum in figure.Nearest(frame, local, None)
            select (key, datum)))
        where !data.IsEmpty
        select new Nearest(plot, data);

    internal Option<PlacedNode> FocusRing(Seq<Key> path) =>
        Placed(path).Map(static held => held with { Style = Styles.Resolve(held.Layout.Cascade, Role.FocusRing, held.Layout.Style.Conditions) });

    internal Option<Adjustment> Adjustment(Seq<Key> path) =>
        Index.Find(path).Bind(layout => Layouts.IsHandle(layout)
            ? Index.Find(path.Init).Bind(owner => Adjusting(owner, Some(layout), Adjusted))
            : Adjusting(layout, None, Adjusted));

    internal Seq<HitNode> Hit(SKPoint at) =>
        Order.Rev().Choose(layout => HitAt(layout, at)).Head
            .Map(first => first.Cons(AncestorsAndSelf(first.Layout.Path).Tail.Choose(Index.Find).Choose(layout => HitAt(layout, at))))
            .IfNone([]);

    internal SKPoint Shift(LayoutNode parent, LayoutNode child) =>
        Places.Find(parent.Path).Map(place => Within(parent, child, place, Adjusted).Origin - Absolute(parent.Frame, place.Origin).Location).IfNone(() => Scrolled(parent, child, Adjusted));

    internal static Seq<Seq<Key>> AncestorsAndSelf(Seq<Key> path) => toSeq(Range(0, path.Count + 1)).Rev().Map(path.Take);

    internal static Seq<LayoutNode> Preorder(LayoutNode layout) => toSeq(SuperEnumerable.TraverseDepthFirst(layout, static held => held.Children));

    private Option<PlacedNode> Placed(Seq<Key> path) =>
        from layout in Index.Find(path)
        from place in Places.Find(path)
        select new PlacedNode(layout, place);

    private Option<HitNode> HitAt(LayoutNode layout, SKPoint at) =>
        from place in Places.Find(layout.Path)
        where !Inert.Contains(layout.Path) && (layout.Node.Key.IsSome || layout.Node.Part.IsSome || layout.Node.Hovering.IsSome || layout.Node.Lift.IsSome || layout.Node.Drop.IsSome)
        from local in place.Local(at)
        where place.Clip.Contains(local)
        let frame = Absolute(layout.Frame, place.Origin)
        let reach = layout.Style.Metrics[Metric.Reach]
        where layout.Node.Traced.Match(
            Some: figure => place.Plot.Exists(plot => figure.Hits(plot, local, reach + layout.Style.HalfStroke)),
            None: () => Holds(layout, place, local, layout.Node.Pinned.IsSome || Layouts.IsHandle(layout) ? reach : 0f))
        select new HitNode(layout, local - frame.Location);

    private bool Holds(LayoutNode layout, Place place, SKPoint local, float reach) {
        SKRect frame = Absolute(layout.Frame, place.Origin);
        return layout.Node.Role.IsNone || layout.Style.Shape is Shape.Rounded
            ? layout.Corners.Holds(frame, reach, local)
            : outlines(new OutlineKey(layout.Style.Shape, layout.Corners, layout.Smoothing, SKRect.Create(frame.Size), reach)).Exists(outline => outline.Contains(local - frame.Location));
    }

    private static Option<Adjustment> Adjusting(LayoutNode owner, Option<LayoutNode> handle, HashMap<Seq<Key>, float> adjusted) =>
        owner.Node is Node.Scroll node ? Scrolling(owner, handle, adjusted, node) : None;

    private static Option<Adjustment> Scrolling(LayoutNode owner, Option<LayoutNode> handle, HashMap<Seq<Key>, float> adjusted, Node.Scroll node) {
        Seq<Key> stored = Layouts.Stored(owner, node);
        return Layouts.Scrollable(owner, node).Map(overflow => new Adjustment(owner.Path, stored, node.Axis, overflow.Clamped(adjusted.Find(stored).IfNone(0f)), overflow,
            Gearing: handle.Map(thumb => Layouts.Rescaled(1f, overflow.Viewport - node.Axis.Of(thumb.Frame.Size), overflow.Max)).IfNone(1f), Step: owner.Style.Metrics[Metric.Spacing], Page: overflow.Viewport) {
            Stops = node.Snap.Map(snap => toSeq((from pane in Layouts.Panes(owner)
                                                 from child in pane.Children
                                                 select overflow.Clamped(node.Axis.Of(pane.Frame.Location + child.Frame.Location - owner.ContentBox.Location) + (snap.Of(node.Axis) * (node.Axis.Of(child.Frame.Size) - overflow.Viewport)))).Distinct().Order())).IfNone([]),
        });
    }

    private Seq<Hosted> Hosting(Seq<Hosted> earlier, PlacedNode held) =>
        held.Layout.Node.SwitchPartially(
            (Scene: this, Earlier: earlier, Held: held),
            @default: static (state, _) => (from glass in state.Held.Layout.Style.Glass.Filter(_ => state.Held.Layout.Node.Role.IsSome && state.Held.Layout.Corners.Uniform.IsSome)
                                            where state.Scene.Host.Blending.Native(state.Earlier.Exists(prior => prior is Hosted.Slot && prior.Frame.IntersectsWith(state.Held.Frame)))
                                            select (Hosted)new Hosted.Backdrop(state.Held.Layout.Corners, glass.Clear, state.Held.Layout.Style.Tint.Bind(tint => state.Held.Layout.Cascade.Theme.Color(tint, state.Held.Layout.Style))) {
                                                Spacing = state.Scene.Index.Find(state.Held.Layout.Path.Init).Map(static parent => parent.Style.Metrics[Metric.Gap]).IfNone(0f),
                                                Path = state.Held.Layout.Path, Frame = state.Held.Frame, Clip = state.Held.Place.Clip,
                                            }).ToSeq(),
            slot: static (state, slot) => Seq<Hosted>(new Hosted.Slot(slot.Kind) { Path = state.Held.Layout.Path, Frame = state.Held.Frame, Clip = state.Held.Place.Clip }));

    // --- [PARTS]
    internal Option<PartOwner> PartAt(Seq<Key> path) =>
        from layout in Index.Find(path)
        from part in layout.Node.Part
        from bounds in Frame(path)
        select new PartOwner(path, layout, part, bounds);

    internal Option<PartOwner> EnabledPartAt(Seq<Key> path) => PartAt(path).Filter(static owner => owner.Enabled);

    internal Seq<PartOwner> Owners(Seq<Key> path) => AncestorsAndSelf(path).Choose(EnabledPartAt);

    internal Option<PartOwner> Owner(Seq<Key> path) => Owners(path).Head;

    internal Seq<PartHit> Chain(Seq<Seq<Key>> hits) => toSeq(hits.Choose(hit => Owner(hit).Map(owner => new PartHit(owner, hit))).DistinctBy(static held => held.Owner.Path));

    internal HitTarget HitTarget(PartOwner owner, Seq<Key> over) => new(this, owner.Layout, owner.Bounds, over);

    internal float Slop(Seq<Key> owner) => Index.Find(owner).Map(static layout => layout.Style.Metrics[Metric.Slop]).IfNone(0f);

    internal Seq<Adjustment> Scrollers(Seq<Seq<Key>> hits, Vector2 delta) => hits.Head.Map(AncestorsAndSelf).IfNone([]).Choose(Adjustment).Filter(adjuster => adjuster.Scrolls(delta));

    internal static Option<Seq<Key>> Lead(LayoutNode layout) => layout.Node.Part.Bind(part => part.Revealed(layout));

    // --- [DAMAGE]
    private sealed record Diff(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now, bool Restyled);

    private sealed record WalkState(HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places, Seq<SKRect> Damage);

    private static WalkState Walk(Diff diff, LayoutNode before, LayoutNode after, Place was, Place now, WalkState state) {
        bool same = ReferenceEquals(before, after);
        bool moved = diff.Restyled || was != now || before.Node.Plotted != after.Node.Plotted || before.Node.Alpha.CompareTo(after.Node.Alpha) is not 0;
        if (same && !moved) return state;
        if (moved) return Replaced(diff, before, after, was, now, state);
        bool own = before.Frame != after.Frame || before.Corners != after.Corners || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.Node.Equals(after.Node));
        return Matched(diff, before.Children, after.Children, child => Within(before, child, was, diff.Was), child => Within(after, child, now, diff.Now),
            state with { Index = state.Index.AddOrUpdate(after.Path, after), Damage = own ? state.Damage.Add(Painted(before, was)).Add(Painted(after, now)) : state.Damage });
    }

    private static WalkState Matched(Diff diff, Seq<LayoutNode> before, Seq<LayoutNode> after, Func<LayoutNode, Place> was, Func<LayoutNode, Place> now, WalkState state) {
        HashMap<Seq<Key>, LayoutNode> prior = toHashMap(before.Map(static held => (held.Path, held)));
        HashSet<Seq<Key>> kept = toHashSet(after.Map(static held => held.Path));
        WalkState walked = after.Fold(state, (held, laid) => prior.Find(laid.Path).Match(
            Some: matched => Walk(diff, matched, laid, was(matched), now(laid), held),
            None: () => Added(diff, laid, now(laid), held)));
        return before.Filter(held => !kept.Contains(held.Path)).Fold(walked, (held, gone) => Removed(gone, was(gone), held));
    }

    private static WalkState Replaced(Diff diff, LayoutNode before, LayoutNode after, Place was, Place now, WalkState state) => Added(diff, after, now, Removed(before, was, state));

    private static WalkState Added(Diff diff, LayoutNode layout, Place place, WalkState state) =>
        new(state.Index.AddOrUpdateRange(Indexed(layout).AsIterable()), Placing(layout, place, diff.Now, state.Places), state.Damage.Concat(Extents(layout, place)));

    private static WalkState Removed(LayoutNode layout, Place place, WalkState state) {
        Seq<Seq<Key>> paths = Preorder(layout).Map(static held => held.Path);
        return new(state.Index.RemoveRange(paths), state.Places.RemoveRange(paths), state.Damage.Concat(Extents(layout, place)));
    }

    private static Seq<SKRect> Extents(LayoutNode layout, Place place) =>
        SKRect.Intersect(Absolute(layout.SubtreeBounds, place.Origin), place.Clip) switch {
            var reach when reach.IsEmpty => [],
            var reach => [place.Transform.MapRect(reach)],
        };

    private static SKRect Painted(LayoutNode layout, Place place) => place.Transform.MapRect(SKRect.Intersect(Absolute(layout.Painted, place.Origin), place.Clip));

    private static Option<SKRect> Reached(HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places, HashMap<Seq<Key>, float> adjusted, Seq<Key> plot, LayoutNode fragment) =>
        from layout in index.Find(plot)
        from place in places.Find(plot)
        from reach in Extents(fragment, Within(layout, fragment, place, adjusted)).Head
        select reach;

    private static LayoutNode Fragmented(Layouts.Pass pass, LayoutNode plot, Node overlay, Option<LayoutNode> held) =>
        Layouts.WithCorners(Layouts.Framed(pass, Layouts.Built(pass, plot.Inner, plot.Path.Add(Overlaid), overlay, held), SKRect.Create(plot.Frame.Size)), None);

    // --- [PLACEMENT]
    private static readonly Key Overlaid = new Key.Named("fragment");

    internal static Place Root(LayoutNode layout, SKRect window) => Turned(layout, new Place(SKPoint.Empty, window, None));

    private static Seq<LayoutNode> Declared(LayoutNode root, Seq<LayoutNode> layers) => LayerStack.Declared(root) + layers.Bind(LayerStack.Declared);

    internal static HashMap<Seq<Key>, LayoutNode> Indexed(LayoutNode layout) => toHashMap(Preorder(layout).Map(static held => (held.Path, held)));

    internal static HashMap<Seq<Key>, Place> Placing(LayoutNode layout, Place place, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Place> placed) =>
        layout.Children.Fold(placed.AddOrUpdate(layout.Path, place), (held, child) => Placing(child, Within(layout, child, place, adjusted), adjusted, held));

    private static Place Within(LayoutNode parent, LayoutNode child, Place place, HashMap<Seq<Key>, float> adjusted) {
        SKRect frame = Absolute(parent.Frame, place.Origin);
        SKPoint origin = frame.Location + Scrolled(parent, child, adjusted);
        return Turned(child, new Place(origin + Stuck(parent, child, place.Clip, origin), parent.Node.Clips(child.Node) ? SKRect.Intersect(place.Clip, frame) : place.Clip,
            Layouts.Plotted(parent).Map(own => own.Translated(frame.Location)) | place.Plot) { Transform = place.Transform });
    }

    private static Place Turned(LayoutNode layout, Place place) =>
        layout.Transform.Map(turn => Absolute(layout.Frame, place.Origin).Location switch {
            var at => place with { Transform = place.Transform.PreConcat(((SKMatrix44)(Matrix4x4.CreateTranslation(-at.X, -at.Y, 0f) * turn * Matrix4x4.CreateTranslation(at.X, at.Y, 0f))).Matrix) },
        }).IfNone(place);

    private static SKPoint Stuck(LayoutNode parent, LayoutNode child, SKRect clip, SKPoint origin) =>
        child.Node.Sticks.Map(axis => {
            float start = axis.Of(origin + child.Frame.Location);
            float limit = toSeq(parent.Children.SkipWhile(sibling => !ReferenceEquals(sibling, child)).Skip(1)).Find(sibling => sibling.Node.Sticks == Some(axis))
                .Map(next => axis.Of(origin + next.Frame.Location)).IfNone(() => axis.Of(origin) + axis.Of(new SKPoint(parent.ContentBox.Right, parent.ContentBox.Bottom)));
            return (SKPoint)axis.With(Vector2.Zero, float.Clamp(axis.Of(clip.Location), start, float.Max(start, limit - axis.Of(child.Frame.Size))) - start);
        }).IfNone(SKPoint.Empty);

    private static SKPoint Scrolled(LayoutNode parent, LayoutNode child, HashMap<Seq<Key>, float> adjusted) =>
        (from adjuster in Adjusting(parent, None, adjusted)
         where !Layouts.IsHandle(child)
         select (SKPoint)adjuster.Axis.With(Vector2.Zero, -adjuster.Value)).IfNone(SKPoint.Empty);

    internal static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);
}
