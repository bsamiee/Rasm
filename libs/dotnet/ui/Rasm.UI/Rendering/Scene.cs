using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LayoutNode {
    internal required Seq<Key> Path { get; init; }
    internal required Node Node { get; init; }
    internal required Cascade Cascade { get; init; }
    internal required RoleStyle Style { get; init; }
    internal SKSize Intrinsic { get; init; }
    internal SKRect Frame { get; init; }
    internal Corners Corners { get; init; } = Corners.Square;
    internal Seq<LayoutNode> Children { get; init; }
    internal Seq<LayoutNode> Members { get; init; }

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

    internal bool Timed =>
        Node.Role.IsSome && Style.Fills.Exists(static fill => fill.Timed);

    internal SKRect LocalSubtreeBounds {
        get {
            SKRect reach = SubtreeBounds;
            return SKRect.Create(reach.Location - Frame.Location, reach.Size);
        }
    }

    internal float Alpha => (Style.Dimmed && !Cascade.Enclosing.Dimmed ? Cascade.Theme.DisabledOpacity : 1f) * Node.Alpha;

    internal float Inset => Node.Role.IsSome ? Style.Metrics[Metric.Inset] : 0f;

    internal float Smoothing => Node.Role.IsSome ? Style.Metrics[Metric.Smoothing] : 0f;

    internal bool HasBackground => Node.Role.IsSome && Node.Painting.AcceptsBackground && (!Style.Fills.IsEmpty || !Style.Strokes.IsEmpty);

    internal bool Contains(SKRect frame, float reach, SKPoint at) {
        if (!SKRect.Inflate(frame, reach, reach).Contains(at)) return false;
        using Outline outline = Outline.Of(Corners, Smoothing, frame, reach);
        return outline.Contains(at);
    }

    internal Option<Text> TextNode => Switch(box: static _ => Option<Text>.None, text: static text => Some(text));

    private float Overhang => Node.Role.IsSome || Node.Traced.IsSome ? Style.Overhang : 0f;

    internal sealed record Box : LayoutNode;

    internal sealed record Text(Node.Text Source, Typeset Typeset, TextStyle TextStyle) : LayoutNode {
        internal SKPoint Origin { get; init; }
        internal float MinContent { get; init; }

        internal override SKRect Painted {
            get {
                SKRect set = SKRect.Create(Frame.Location + Origin, Typeset.Size);
                SKRect ink = Typeset.Lines.MeasuredOverhang;
                return SKRect.Union(base.Painted, new SKRect(set.Left - float.Max(Overhang, ink.Left), set.Top - float.Max(Overhang, ink.Top),
                    set.Right + float.Max(Overhang, ink.Right), set.Bottom + float.Max(Overhang, ink.Bottom)));
            }
        }
    }
}

public sealed record Adjustment(Seq<Key> Owner, PhysicalAxis Axis, float Value, float Min, float Max, float Gearing, float Step, float Page) {
    public float Clamped(float value) => float.Clamp(value, Min, Max);

    public Option<float> Navigated(NavigationDirection toward) =>
        toward.Along == Axis
            ? Some(toward.Granularity.Distance(Step, Page).Match(
                Some: stride => Value + (toward.Sign * stride),
                None: () => toward.Sign < 0 ? Min : Max))
            : None;

    internal bool Scrolls(Vector2 delta) => Max > Min && MathF.Abs(Axis.Of(delta)) >= MathF.Abs(Axis.Cross.Of(delta));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hosted {
    public required Seq<Key> Path { get; init; }
    public required SKRect Frame { get; init; }
    public required SKRect Clip { get; init; }

    public sealed record Slot(SlotKind Kind) : Hosted;

    public sealed record Backdrop(Corners Corners, bool Clear, Option<SKColorF> Tint) : Hosted;
}

internal sealed record Place(SKPoint Origin, SKRect Clip, Option<PlotFrame> Plot);

internal sealed record PartOwner(Seq<Key> Path, LayoutNode Layout, Part Part, SKRect Bounds) {
    internal bool Enabled => !Layout.Style.Conditions.Contains(Condition.Disabled);
}

internal sealed record PartHit(PartOwner Owner, Seq<Key> Hit);

internal sealed record HitNode(LayoutNode Layout, SKPoint Local);

internal sealed record PlacedNode(LayoutNode Layout, SKRect Frame, SKRect Clip) {
    internal RoleStyle Style { get; init; } = Layout.Style;
}

internal sealed record SceneChange(Scene Scene, Seq<SKRect> Damage);

public sealed class Scene {
    // --- [STATE]
    private Scene(LayoutNode root, HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places, HashMap<Seq<Key>, float> adjusted, Shaping shaping) {
        Layout = root;
        Shaping = shaping;
        Index = index;
        Places = places;
        Adjusted = Clamped(adjusted, index);
        Order = Preorder(root);
        Timed = Order.Filter(static layout => layout.Timed);
        Hosted = from layout in Order
                 from place in places.Find(layout.Path).ToSeq()
                 from hosted in Hosts(layout, place)
                 select hosted;
        Extended = Order.Exists(static layout => layout.Node.SourceImage.Exists(static image => image.ColorType is SKColorType.RgbaF16 or SKColorType.RgbaF32 || image.ColorSpace is { IsSrgb: false }));
    }

    public HashMap<Seq<Key>, float> Adjusted { get; }
    public Seq<Hosted> Hosted { get; }
    public bool Extended { get; }
    internal LayoutNode Layout { get; }
    internal HashMap<Seq<Key>, LayoutNode> Index { get; }
    internal HashMap<Seq<Key>, Place> Places { get; }
    internal Seq<LayoutNode> Order { get; }
    internal Seq<LayoutNode> Timed { get; }
    internal Shaping Shaping { get; }

    // --- [TRANSITIONS]
    internal static Fin<Scene> Of(Cascade root, Node view, SKSize size, HashMap<Seq<Key>, float> adjusted, Shaping shaping) =>
        Layouts.Unique(Layouts.Arrange(root, view, size, adjusted, [], shaping), []).Map(unique => new Scene(unique, Indexed(unique), Placing(unique, Root(unique), adjusted, []), adjusted, shaping));

    internal Fin<SceneChange> Reconcile(Node root) =>
        Layouts.Unique(Layouts.Arrange(Layout.Cascade, root, Layout.Frame.Size, Adjusted, Index, Shaping), Index).Map(unique => Diffed(unique, Adjusted));

    internal SceneChange Resize(SKSize size) => Arranged(Layout.Cascade, size, Adjusted, Index);

    internal SceneChange Restyle(ColorScheme theme, Role root) => Arranged(Styles.Root(theme, Layout.Cascade.Defaults, root), Layout.Frame.Size, Adjusted, []);

    internal SceneChange Unchanged => new(this, []);

    internal SceneChange Adjust(Seq<Key> path, Func<float, float> move) =>
        (from adjuster in Adjustment(path)
         let value = adjuster.Clamped(move(adjuster.Value))
         where value.CompareTo(adjuster.Value) is not 0
         select Readjusted(Adjusted.AddOrUpdate(adjuster.Owner, value), Seq(adjuster.Owner)))
        .IfNone(Unchanged);

    internal SceneChange Reveal(Seq<Key> path) =>
        (Frame(path) | Slotted(path)).Map(target => AncestorsAndSelf(path).Tail.Choose(Adjustment).Fold(
                (Adjusted, Target: target, Moved: Seq<Seq<Key>>()),
                (held, adjuster) => Revealing(adjuster, held.Target).Filter(static moved => moved.CompareTo(0f) is not 0).Match(
                    Some: moved => (held.Adjusted.AddOrUpdate(adjuster.Owner, adjuster.Value + moved), Absolute(held.Target, (SKPoint)adjuster.Axis.With(Vector2.Zero, -moved)), held.Moved.Add(adjuster.Owner)),
                    None: () => held)))
            .Filter(static revealed => !revealed.Moved.IsEmpty)
            .Match(Some: revealed => Readjusted(revealed.Adjusted, revealed.Moved), None: () => Unchanged);

    internal SceneChange Revealed(Option<Scene> prior) =>
        Order.Choose(layout => Lead(layout).Filter(lead => prior.Bind(held => held.Index.Find(layout.Path)).Bind(Lead) != Some(lead)))
            .Fold(Unchanged, static (held, lead) => {
                SceneChange shown = held.Scene.Reveal(lead);
                return shown with { Damage = held.Damage.Concat(shown.Damage) };
            });

    private SceneChange Arranged(Cascade root, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, LayoutNode> prior) =>
        Diffed(Layouts.Arrange(root, Layout.Node, size, adjusted, prior, Shaping), adjusted);

    private SceneChange Readjusted(HashMap<Seq<Key>, float> adjusted, Seq<Seq<Key>> changed) =>
        Arranged(Layout.Cascade, Layout.Frame.Size, adjusted, Index.RemoveRange(changed.Bind(AncestorsAndSelf)));

    private SceneChange Diffed(LayoutNode next, HashMap<Seq<Key>, float> adjusted) {
        Diff diff = new(Adjusted, adjusted);
        WalkState prior = new(Index, Places, []);
        WalkState walked = Layout.Cascade.Theme.Equals(next.Cascade.Theme)
            ? Walk(diff, Layout, next, Root(Layout), Root(next), prior)
            : Replaced(diff, Layout, next, Root(Layout), Root(next), prior);
        return new SceneChange(new Scene(next, walked.Index, walked.Places, adjusted, Shaping), walked.Damage);
    }

    private Option<float> Revealing(Adjustment adjuster, SKRect target) =>
        from viewport in Placed(adjuster.Owner).Map(static held => Absolute(held.Layout.ContentBox, Absolute(held.Layout.Frame, held.Place.Origin).Location))
        let low = (Target: adjuster.Axis.Of(target.Location), View: adjuster.Axis.Of(viewport.Location))
        let high = (Target: adjuster.Axis.Of(new SKPoint(target.Right, target.Bottom)), View: adjuster.Axis.Of(new SKPoint(viewport.Right, viewport.Bottom)))
        let delta = low.Target < low.View ? low.Target - low.View : high.Target > high.View ? float.Min(high.Target - high.View, low.Target - low.View) : 0f
        select adjuster.Clamped(adjuster.Value + delta) - adjuster.Value;

    private Option<SKRect> Slotted(Seq<Key> path) =>
        from key in path.Last
        from lazy in Placed(path.Init)
        from node in lazy.Layout.Node.Windowed
        from slot in Layouts.Slot(lazy.Layout, node, key)
        select SKRect.Create(slot.Location + Absolute(lazy.Layout.Frame, lazy.Place.Origin).Location, slot.Size);

    // --- [QUERY]
    public Option<SKRect> Frame(Seq<Key> path) => Placed(path).Map(static held => Absolute(held.Layout.Frame, held.Place.Origin));

    public Option<SKRect> Bounds(Seq<Key> path) => Placed(path).Map(static held => Painted(held.Layout, held.Place));

    public Option<SKRect> Visible(Seq<Key> path) => Placed(path).Map(static held => SKRect.Intersect(Absolute(held.Layout.Frame, held.Place.Origin), held.Place.Clip)).Filter(static shown => !shown.IsEmpty);

    public bool Covers(SKPoint at) =>
        Order.Exists(layout => Places.Find(layout.Path).Exists(place => place.Clip.Contains(at) && (layout.HasBackground || layout.Node.Painting.Paints) && layout.Contains(Absolute(layout.Frame, place.Origin), 0f, at)));

    public SKSize Preferred(SKSize offered) => Layouts.Preferred(Layout, offered, Adjusted, Shaping);

    internal Option<PlacedNode> FocusRing(Seq<Key> path) =>
        Placed(path).Map(static held => new PlacedNode(held.Layout, Absolute(held.Layout.Frame, held.Place.Origin), held.Place.Clip) {
            Style = Styles.Resolve(held.Layout.Cascade, Role.FocusRing, held.Layout.Style.Conditions),
        });

    internal Option<Adjustment> Adjustment(Seq<Key> path) =>
        Index.Find(path).Bind(layout => Layouts.IsHandle(layout)
            ? Index.Find(path.Init).Bind(owner => Adjusting(owner, Some(layout), Adjusted.Find(owner.Path)))
            : Adjusting(layout, None, Adjusted.Find(path)));

    internal Seq<HitNode> Hit(SKPoint at) =>
        Order.Rev().Choose(layout => HitAt(layout, at)).Head
            .Map(first => first.Cons(AncestorsAndSelf(first.Layout.Path).Tail.Choose(Index.Find).Choose(layout => HitAt(layout, at))))
            .IfNone([]);

    internal SKPoint Shift(LayoutNode parent, LayoutNode child) => Shift(parent, child, Adjusted);

    internal static Seq<Seq<Key>> AncestorsAndSelf(Seq<Key> path) => toSeq(Range(0, path.Count + 1)).Rev().Map(path.Take);

    internal static Seq<LayoutNode> Preorder(LayoutNode layout) => layout.Cons(layout.Children.Bind(Preorder));

    private sealed record Located(LayoutNode Layout, Place Place);

    private Option<Located> Placed(Seq<Key> path) =>
        from layout in Index.Find(path)
        from place in Places.Find(path)
        select new Located(layout, place);

    private Option<HitNode> HitAt(LayoutNode layout, SKPoint at) =>
        from place in Places.Find(layout.Path)
        where place.Clip.Contains(at) && (layout.Node.Key.IsSome || layout.Node.Part.IsSome || layout.Node.Hovering.IsSome)
        let frame = Absolute(layout.Frame, place.Origin)
        let reach = layout.Style.Metrics[Metric.Reach]
        where layout.Node.Traced.Match(
            Some: figure => place.Plot.Exists(plot => figure.Hits(plot, at, reach + layout.Style.HalfStroke)),
            None: () => layout.Contains(frame, layout.Node.Pinned.IsSome || Layouts.IsHandle(layout) ? reach : 0f, at))
        select new HitNode(layout, at - frame.Location);

    private static Option<Adjustment> Adjusting(LayoutNode owner, Option<LayoutNode> handle, Option<float> stored) =>
        owner.Node.SwitchPartially(
            (Owner: owner, Handle: handle, Stored: stored),
            @default: static (_, _) => Option<Adjustment>.None,
            scroll: static (held, node) => Scrolled(held.Owner, held.Handle, held.Stored, node));

    private static Option<Adjustment> Scrolled(LayoutNode owner, Option<LayoutNode> handle, Option<float> stored, Node.Scroll node) =>
        Layouts.Scrollable(owner, node).Map(scroll => new Adjustment(
            owner.Path,
            node.Axis,
            Layouts.Scrolling(stored.IfNone(0f), scroll.Content, scroll.Viewport),
            Min: 0f,
            Max: float.Max(0f, scroll.Content - scroll.Viewport),
            Gearing: handle.Map(thumb => Layouts.Rescaled(1f, scroll.Viewport - node.Axis.Of(thumb.Frame.Size), scroll.Content - scroll.Viewport)).IfNone(1f),
            Step: owner.Style.Metrics[Metric.Spacing],
            Page: scroll.Viewport));

    private static HashMap<Seq<Key>, float> Clamped(HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, LayoutNode> index) =>
        toHashMap(toSeq(adjusted.AsIterable()).Choose(entry => index.Find(entry.Key).Bind(layout => Adjusting(layout, None, Some(entry.Value))).Map(adjuster => (entry.Key, adjuster.Value))));

    private static Seq<Hosted> Hosts(LayoutNode layout, Place place) =>
        layout.Node.SwitchPartially(new PlacedNode(layout, Absolute(layout.Frame, place.Origin), place.Clip), @default: Backdrops, slot: Hosting);

    private static Seq<Hosted> Hosting(PlacedNode held, Node.Slot slot) => [new Hosted.Slot(slot.Kind) { Path = held.Layout.Path, Frame = held.Frame, Clip = held.Clip }];

    private static Seq<Hosted> Backdrops(PlacedNode held, Node node) =>
        node.Role.IsNone
            ? []
            : held.Layout.Style.Fills.Choose(static fill => fill.AsBackdrop)
                .Map(backdrop => (Hosted)new Hosted.Backdrop(held.Layout.Corners, backdrop.Clear, backdrop.Tint.Bind(tint => held.Layout.Cascade.Theme.Color(tint, held.Layout.Style))) {
                    Path = held.Layout.Path, Frame = held.Frame, Clip = held.Clip,
                });

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
    private sealed record Diff(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now);

    private sealed record WalkState(HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places, Seq<SKRect> Damage);

    private static WalkState Walk(Diff diff, LayoutNode before, LayoutNode after, Place was, Place now, WalkState state) {
        bool same = ReferenceEquals(before, after);
        bool moved = was != now || before.Node.Plotted != after.Node.Plotted || before.Node.Alpha.CompareTo(after.Node.Alpha) is not 0;
        if (same && !moved) return state;
        if (moved) return Replaced(diff, before, after, was, now, state);
        bool own = before.Frame != after.Frame || before.Corners != after.Corners || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.Node.Equals(after.Node));
        HashMap<Seq<Key>, LayoutNode> prior = toHashMap(before.Children.Map(static child => (child.Path, child)));
        HashSet<Seq<Key>> kept = toHashSet(after.Children.Map(static child => child.Path));
        WalkState walked = after.Children.Fold(
            state with { Index = state.Index.AddOrUpdate(after.Path, after), Damage = own ? state.Damage.Add(Painted(before, was)).Add(Painted(after, now)) : state.Damage },
            (held, child) => prior.Find(child.Path).Match(
                Some: matched => Walk(diff, matched, child, Within(before, matched, was, diff.Was), Within(after, child, now, diff.Now), held),
                None: () => Added(diff, child, Within(after, child, now, diff.Now), held)));
        return before.Children.Filter(child => !kept.Contains(child.Path)).Fold(walked, (held, child) => Removed(child, Within(before, child, was, diff.Was), held));
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
            var reach => [reach],
        };

    private static SKRect Painted(LayoutNode layout, Place place) => SKRect.Intersect(Absolute(layout.Painted, place.Origin), place.Clip);

    // --- [PLACEMENT]
    private static Place Root(LayoutNode layout) => new(SKPoint.Empty, SKRect.Create(layout.Frame.Size), None);

    private static HashMap<Seq<Key>, LayoutNode> Indexed(LayoutNode layout) => toHashMap(Preorder(layout).Map(static held => (held.Path, held)));

    private static HashMap<Seq<Key>, Place> Placing(LayoutNode layout, Place place, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Place> placed) =>
        layout.Children.Fold(placed.AddOrUpdate(layout.Path, place), (held, child) => Placing(child, Within(layout, child, place, adjusted), adjusted, held));

    private static Place Within(LayoutNode parent, LayoutNode child, Place place, HashMap<Seq<Key>, float> adjusted) {
        SKRect frame = Absolute(parent.Frame, place.Origin);
        return new Place(frame.Location + Shift(parent, child, adjusted), parent.Node.Clips(child.Node) ? SKRect.Intersect(place.Clip, frame) : place.Clip,
            Layouts.Plotted(parent).Map(own => own.Translated(frame.Location)) | place.Plot);
    }

    private static SKPoint Shift(LayoutNode parent, LayoutNode child, HashMap<Seq<Key>, float> adjusted) =>
        (from adjuster in Adjusting(parent, None, adjusted.Find(parent.Path))
         where !Layouts.IsHandle(child)
         select (SKPoint)adjuster.Axis.With(Vector2.Zero, -adjuster.Value)).IfNone(SKPoint.Empty);

    internal static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);
}
