using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Laid {
    internal required Seq<Key> Path { get; init; }
    internal required Node Node { get; init; }
    internal required Cascade Cascade { get; init; }
    internal required RoleStyle Style { get; init; }
    internal SKSize Intrinsic { get; init; }
    internal SKRect Frame { get; init; }
    internal Corners Corners { get; init; } = Corners.Square;
    internal Seq<Laid> Children { get; init; }
    internal Seq<Laid> Members { get; init; }

    internal SKRect ContentBox {
        get {
            float inset = Inset;
            return SKRect.Create(inset, inset, float.Max(0f, Frame.Width - (2f * inset)), float.Max(0f, Frame.Height - (2f * inset)));
        }
    }

    internal virtual SKRect Painted => SKRect.Inflate(Frame, Overhang, Overhang);

    internal SKRect Reach =>
        Children.Filter(child => !Node.Clips(child.Node)).Fold(Painted, (union, child) => {
            SKRect reach = child.Reach;
            SKRect moved = SKRect.Create(reach.Location + Frame.Location, reach.Size);
            return reach.IsEmpty ? union : union.IsEmpty ? moved : SKRect.Union(union, moved);
        });

    internal bool Timed =>
        Node.Role.IsSome && Style.Fills.Exists(static fill => fill.Timed);

    internal SKRect LocalReach {
        get {
            SKRect reach = Reach;
            return SKRect.Create(reach.Location - Frame.Location, reach.Size);
        }
    }

    internal float Alpha => (Style.Veiled && !Cascade.Enclosing.Veiled ? Cascade.Theme.Veil : 1f) * Node.Alpha;

    internal float Inset => Node.Role.IsSome ? Style.Metrics[Metric.Inset] : 0f;

    internal float Smoothing => Node.Role.IsSome ? Style.Metrics[Metric.Smoothing] : 0f;

    internal bool HasBackground => Node.Role.IsSome && Node.Painting.AcceptsBackground && (!Style.Fills.IsEmpty || !Style.Strokes.IsEmpty);

    internal bool Holds(SKRect frame, float reach, SKPoint at) {
        if (!SKRect.Inflate(frame, reach, reach).Contains(at)) return false;
        using Outline outline = Outline.Of(Corners, Smoothing, frame, reach);
        return outline.Holds(at);
    }

    internal Option<Text> Texted => Switch(box: static _ => Option<Text>.None, text: static text => Some(text));

    private float Overhang => Node.Role.IsSome || Node.Traced.IsSome ? Style.Overhang : 0f;

    internal sealed record Box : Laid;

    internal sealed record Text(Node.Text Source, Typeset Typeset, TextStyle TextStyle) : Laid {
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

public sealed record Adjuster(Seq<Key> Owner, PhysicalAxis Axis, float Value, float Min, float Max, float Gearing, float Step, float Page) {
    public float Clamped(float value) => float.Clamp(value, Min, Max);

    public Option<float> Navigated(NavigationDirection toward) =>
        toward.Along == Axis
            ? Some(toward.Grain.Stride(Step, Page).Match(
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

internal sealed record Owned(Seq<Key> Path, Laid Laid, Part Part, SKRect Bounds) {
    internal bool Enabled => !Laid.Style.Conditions.Contains(Condition.Disabled);
}

internal sealed record Claim(Owned Owner, Seq<Key> Over);

internal sealed record Contact(Laid Laid, SKPoint Local);

internal sealed record Framed(Laid Laid, SKRect Frame, SKRect Clip) {
    internal RoleStyle Style { get; init; } = Laid.Style;
}

internal sealed record Rearranged(Scene Scene, Seq<SKRect> Damage);

public sealed class Scene {
    // --- [STATE]
    private Scene(Laid layout, HashMap<Seq<Key>, Laid> index, HashMap<Seq<Key>, Place> places, HashMap<Seq<Key>, float> adjusted, Shaping shaping) {
        Layout = layout;
        Shaping = shaping;
        Index = index;
        Places = places;
        Adjusted = Clamped(adjusted, index);
        Order = Preorder(layout);
        Timed = Order.Filter(static laid => laid.Timed);
        Hosted = from laid in Order
                 from place in places.Find(laid.Path).ToSeq()
                 from hosted in Hosts(laid, place)
                 select hosted;
        Extended = Order.Exists(static laid => laid.Node.Imaged.Exists(static image => image.ColorType is SKColorType.RgbaF16 or SKColorType.RgbaF32 || image.ColorSpace is { IsSrgb: false }));
    }

    public HashMap<Seq<Key>, float> Adjusted { get; }
    public Seq<Hosted> Hosted { get; }
    public bool Extended { get; }
    internal Laid Layout { get; }
    internal HashMap<Seq<Key>, Laid> Index { get; }
    internal HashMap<Seq<Key>, Place> Places { get; }
    internal Seq<Laid> Order { get; }
    internal Seq<Laid> Timed { get; }
    internal Shaping Shaping { get; }

    // --- [TRANSITIONS]
    internal static Fin<Scene> Of(Cascade root, Node view, SKSize size, HashMap<Seq<Key>, float> adjusted, Shaping shaping) =>
        Layouts.Unique(Layouts.Arrange(root, view, size, adjusted, [], shaping), []).Map(unique => new Scene(unique, Indexed(unique), Placing(unique, Root(unique), adjusted, []), adjusted, shaping));

    internal Fin<Rearranged> Reconcile(Node root) =>
        Layouts.Unique(Layouts.Arrange(Layout.Cascade, root, Layout.Frame.Size, Adjusted, Index, Shaping), Index).Map(unique => Diffed(unique, Adjusted));

    internal Rearranged Resize(SKSize size) => Arranged(Layout.Cascade, size, Adjusted, Index);

    internal Rearranged Restyle(ColorScheme theme, Role root) => Arranged(Styles.Root(theme, Layout.Cascade.Defaults, root), Layout.Frame.Size, Adjusted, []);

    internal Rearranged Unchanged => new(this, []);

    internal Rearranged Adjust(Seq<Key> path, Func<float, float> move) =>
        (from adjuster in Adjuster(path)
         let value = adjuster.Clamped(move(adjuster.Value))
         where value.CompareTo(adjuster.Value) is not 0
         select Readjusted(Adjusted.AddOrUpdate(adjuster.Owner, value), Seq(adjuster.Owner)))
        .IfNone(Unchanged);

    internal Rearranged Reveal(Seq<Key> path) =>
        (Frame(path) | Slotted(path)).Map(target => Above(path).Tail.Choose(Adjuster).Fold(
                (Adjusted, Target: target, Moved: Seq<Seq<Key>>()),
                (held, adjuster) => Revealing(adjuster, held.Target).Filter(static moved => moved.CompareTo(0f) is not 0).Match(
                    Some: moved => (held.Adjusted.AddOrUpdate(adjuster.Owner, adjuster.Value + moved), Absolute(held.Target, (SKPoint)adjuster.Axis.With(Vector2.Zero, -moved)), held.Moved.Add(adjuster.Owner)),
                    None: () => held)))
            .Filter(static revealed => !revealed.Moved.IsEmpty)
            .Match(Some: revealed => Readjusted(revealed.Adjusted, revealed.Moved), None: () => Unchanged);

    internal Rearranged Revealed(Option<Scene> prior) =>
        Order.Choose(laid => Lead(laid).Filter(lead => prior.Bind(held => held.Index.Find(laid.Path)).Bind(Lead) != Some(lead)))
            .Fold(Unchanged, static (held, lead) => {
                Rearranged shown = held.Scene.Reveal(lead);
                return shown with { Damage = held.Damage.Concat(shown.Damage) };
            });

    private Rearranged Arranged(Cascade root, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> prior) =>
        Diffed(Layouts.Arrange(root, Layout.Node, size, adjusted, prior, Shaping), adjusted);

    private Rearranged Readjusted(HashMap<Seq<Key>, float> adjusted, Seq<Seq<Key>> changed) =>
        Arranged(Layout.Cascade, Layout.Frame.Size, adjusted, Index.RemoveRange(changed.Bind(Above)));

    private Rearranged Diffed(Laid next, HashMap<Seq<Key>, float> adjusted) {
        Diff diff = new(Adjusted, adjusted);
        Walked prior = new(Index, Places, []);
        Walked walked = Layout.Cascade.Theme.Equals(next.Cascade.Theme)
            ? Walk(diff, Layout, next, Root(Layout), Root(next), prior)
            : Replaced(diff, Layout, next, Root(Layout), Root(next), prior);
        return new Rearranged(new Scene(next, walked.Index, walked.Places, adjusted, Shaping), walked.Damage);
    }

    private Option<float> Revealing(Adjuster adjuster, SKRect target) =>
        from viewport in Placed(adjuster.Owner).Map(static held => Absolute(held.Laid.ContentBox, Absolute(held.Laid.Frame, held.Place.Origin).Location))
        let low = (Target: adjuster.Axis.Of(target.Location), View: adjuster.Axis.Of(viewport.Location))
        let high = (Target: adjuster.Axis.Of(new SKPoint(target.Right, target.Bottom)), View: adjuster.Axis.Of(new SKPoint(viewport.Right, viewport.Bottom)))
        let delta = low.Target < low.View ? low.Target - low.View : high.Target > high.View ? float.Min(high.Target - high.View, low.Target - low.View) : 0f
        select adjuster.Clamped(adjuster.Value + delta) - adjuster.Value;

    private Option<SKRect> Slotted(Seq<Key> path) =>
        from key in path.Last
        from lazy in Placed(path.Init)
        from node in lazy.Laid.Node.Windowed
        from slot in Layouts.Slot(lazy.Laid, node, key)
        select SKRect.Create(slot.Location + Absolute(lazy.Laid.Frame, lazy.Place.Origin).Location, slot.Size);

    // --- [QUERY]
    public Option<SKRect> Frame(Seq<Key> path) => Placed(path).Map(static held => Absolute(held.Laid.Frame, held.Place.Origin));

    public Option<SKRect> Bounds(Seq<Key> path) => Placed(path).Map(static held => Painted(held.Laid, held.Place));

    public Option<SKRect> Visible(Seq<Key> path) => Placed(path).Map(static held => SKRect.Intersect(Absolute(held.Laid.Frame, held.Place.Origin), held.Place.Clip)).Filter(static shown => !shown.IsEmpty);

    public bool Covers(SKPoint at) =>
        Order.Exists(laid => Places.Find(laid.Path).Exists(place => place.Clip.Contains(at) && (laid.HasBackground || laid.Node.Painting.Paints) && laid.Holds(Absolute(laid.Frame, place.Origin), 0f, at)));

    public SKSize Preferred(SKSize offered) => Layouts.Preferred(Layout, offered, Adjusted, Shaping);

    internal Option<Framed> Ring(Seq<Key> path) =>
        Placed(path).Map(static held => new Framed(held.Laid, Absolute(held.Laid.Frame, held.Place.Origin), held.Place.Clip) {
            Style = Styles.Resolve(held.Laid.Cascade, Role.FocusRing, held.Laid.Style.Conditions),
        });

    internal Option<Adjuster> Adjuster(Seq<Key> path) =>
        Index.Find(path).Bind(laid => Layouts.IsHandle(laid)
            ? Index.Find(path.Init).Bind(owner => Adjusting(owner, Some(laid), Adjusted.Find(owner.Path)))
            : Adjusting(laid, None, Adjusted.Find(path)));

    internal Seq<Contact> Hit(SKPoint at) =>
        Order.Rev().Choose(laid => Reached(laid, at)).Head
            .Map(first => first.Cons(Above(first.Laid.Path).Tail.Choose(Index.Find).Choose(laid => Reached(laid, at))))
            .IfNone([]);

    internal SKPoint Shift(Laid parent, Laid child) => Shift(parent, child, Adjusted);

    internal static Seq<Seq<Key>> Above(Seq<Key> path) => toSeq(Range(0, path.Count + 1)).Rev().Map(path.Take);

    internal static Seq<Laid> Preorder(Laid laid) => laid.Cons(laid.Children.Bind(Preorder));

    private sealed record Located(Laid Laid, Place Place);

    private Option<Located> Placed(Seq<Key> path) =>
        from laid in Index.Find(path)
        from place in Places.Find(path)
        select new Located(laid, place);

    private Option<Contact> Reached(Laid laid, SKPoint at) =>
        from place in Places.Find(laid.Path)
        where place.Clip.Contains(at) && (laid.Node.Key.IsSome || laid.Node.Part.IsSome || laid.Node.Hovering.IsSome)
        let frame = Absolute(laid.Frame, place.Origin)
        let reach = laid.Style.Metrics[Metric.Reach]
        where laid.Node.Traced.Match(
            Some: figure => place.Plot.Exists(plot => figure.Hits(plot, at, reach + laid.Style.HalfStroke)),
            None: () => laid.Holds(frame, laid.Node.Pinned.IsSome || Layouts.IsHandle(laid) ? reach : 0f, at))
        select new Contact(laid, at - frame.Location);

    private static Option<Adjuster> Adjusting(Laid owner, Option<Laid> handle, Option<float> stored) =>
        owner.Node.SwitchPartially(
            (Owner: owner, Handle: handle, Stored: stored),
            @default: static (_, _) => Option<Adjuster>.None,
            scroll: static (held, node) => Scrolled(held.Owner, held.Handle, held.Stored, node));

    private static Option<Adjuster> Scrolled(Laid owner, Option<Laid> handle, Option<float> stored, Node.Scroll node) =>
        Layouts.Scrollable(owner, node).Map(scroll => new Adjuster(
            owner.Path,
            node.Axis,
            Layouts.Scrolling(stored.IfNone(0f), scroll.Content, scroll.Viewport),
            Min: 0f,
            Max: float.Max(0f, scroll.Content - scroll.Viewport),
            Gearing: handle.Map(thumb => Layouts.Rescaled(1f, scroll.Viewport - node.Axis.Of(thumb.Frame.Size), scroll.Content - scroll.Viewport)).IfNone(1f),
            Step: owner.Style.Metrics[Metric.Spacing],
            Page: scroll.Viewport));

    private static HashMap<Seq<Key>, float> Clamped(HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> index) =>
        toHashMap(toSeq(adjusted.AsIterable()).Choose(entry => index.Find(entry.Key).Bind(laid => Adjusting(laid, None, Some(entry.Value))).Map(adjuster => (entry.Key, adjuster.Value))));

    private static Seq<Hosted> Hosts(Laid laid, Place place) =>
        laid.Node.SwitchPartially(new Framed(laid, Absolute(laid.Frame, place.Origin), place.Clip), @default: Backdrops, slot: Hosting);

    private static Seq<Hosted> Hosting(Framed held, Node.Slot slot) => [new Hosted.Slot(slot.Kind) { Path = held.Laid.Path, Frame = held.Frame, Clip = held.Clip }];

    private static Seq<Hosted> Backdrops(Framed held, Node node) =>
        node.Role.IsNone
            ? []
            : held.Laid.Style.Fills.Choose(static fill => fill.Backdropped)
                .Map(backdrop => (Hosted)new Hosted.Backdrop(held.Laid.Corners, backdrop.Clear, backdrop.Tint.Bind(tint => held.Laid.Cascade.Theme.Color(tint, held.Laid.Style))) {
                    Path = held.Laid.Path, Frame = held.Frame, Clip = held.Clip,
                });

    // --- [PARTS]
    internal Option<Owned> Parted(Seq<Key> path) =>
        from laid in Index.Find(path)
        from part in laid.Node.Part
        from bounds in Frame(path)
        select new Owned(path, laid, part, bounds);

    internal Option<Owned> Owning(Seq<Key> path) => Parted(path).Filter(static owner => owner.Enabled);

    internal Seq<Owned> Owners(Seq<Key> path) => Above(path).Choose(Owning);

    internal Option<Owned> Owner(Seq<Key> path) => Owners(path).Head;

    internal Seq<Claim> Chain(Seq<Seq<Key>> hits) => toSeq(hits.Choose(hit => Owner(hit).Map(owner => new Claim(owner, hit))).DistinctBy(static held => held.Owner.Path));

    internal Aim Aimed(Owned owner, Seq<Key> over) => new(this, owner.Laid, owner.Bounds, over);

    internal float Slop(Seq<Key> owner) => Index.Find(owner).Map(static laid => laid.Style.Metrics[Metric.Slop]).IfNone(0f);

    internal Seq<Adjuster> Scrollers(Seq<Seq<Key>> hits, Vector2 delta) => hits.Head.Map(Above).IfNone([]).Choose(Adjuster).Filter(adjuster => adjuster.Scrolls(delta));

    internal static Option<Seq<Key>> Lead(Laid laid) => laid.Node.Part.Bind(part => part.Revealed(laid));

    // --- [DAMAGE]
    private sealed record Diff(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now);

    private sealed record Walked(HashMap<Seq<Key>, Laid> Index, HashMap<Seq<Key>, Place> Places, Seq<SKRect> Damage);

    private static Walked Walk(Diff diff, Laid before, Laid after, Place was, Place now, Walked state) {
        bool same = ReferenceEquals(before, after);
        bool moved = was != now || before.Node.Plotted != after.Node.Plotted || before.Node.Alpha.CompareTo(after.Node.Alpha) is not 0;
        if (same && !moved) return state;
        if (moved) return Replaced(diff, before, after, was, now, state);
        bool own = before.Frame != after.Frame || before.Corners != after.Corners || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.Node.Equals(after.Node));
        HashMap<Seq<Key>, Laid> prior = toHashMap(before.Children.Map(static child => (child.Path, child)));
        HashSet<Seq<Key>> kept = toHashSet(after.Children.Map(static child => child.Path));
        Walked walked = after.Children.Fold(
            state with { Index = state.Index.AddOrUpdate(after.Path, after), Damage = own ? state.Damage.Add(Painted(before, was)).Add(Painted(after, now)) : state.Damage },
            (held, child) => prior.Find(child.Path).Match(
                Some: matched => Walk(diff, matched, child, Within(before, matched, was, diff.Was), Within(after, child, now, diff.Now), held),
                None: () => Added(diff, child, Within(after, child, now, diff.Now), held)));
        return before.Children.Filter(child => !kept.Contains(child.Path)).Fold(walked, (held, child) => Removed(child, Within(before, child, was, diff.Was), held));
    }

    private static Walked Replaced(Diff diff, Laid before, Laid after, Place was, Place now, Walked state) => Added(diff, after, now, Removed(before, was, state));

    private static Walked Added(Diff diff, Laid laid, Place place, Walked state) =>
        new(state.Index.AddOrUpdateRange(Indexed(laid).AsIterable()), Placing(laid, place, diff.Now, state.Places), state.Damage.Concat(Extents(laid, place)));

    private static Walked Removed(Laid laid, Place place, Walked state) {
        Seq<Seq<Key>> paths = Preorder(laid).Map(static held => held.Path);
        return new(state.Index.RemoveRange(paths), state.Places.RemoveRange(paths), state.Damage.Concat(Extents(laid, place)));
    }

    private static Seq<SKRect> Extents(Laid laid, Place place) =>
        SKRect.Intersect(Absolute(laid.Reach, place.Origin), place.Clip) switch {
            var reach when reach.IsEmpty => [],
            var reach => [reach],
        };

    private static SKRect Painted(Laid laid, Place place) => SKRect.Intersect(Absolute(laid.Painted, place.Origin), place.Clip);

    // --- [PLACEMENT]
    private static Place Root(Laid laid) => new(SKPoint.Empty, SKRect.Create(laid.Frame.Size), None);

    private static HashMap<Seq<Key>, Laid> Indexed(Laid laid) => toHashMap(Preorder(laid).Map(static held => (held.Path, held)));

    private static HashMap<Seq<Key>, Place> Placing(Laid laid, Place place, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Place> placed) =>
        laid.Children.Fold(placed.AddOrUpdate(laid.Path, place), (held, child) => Placing(child, Within(laid, child, place, adjusted), adjusted, held));

    private static Place Within(Laid parent, Laid child, Place place, HashMap<Seq<Key>, float> adjusted) {
        SKRect frame = Absolute(parent.Frame, place.Origin);
        return new Place(frame.Location + Shift(parent, child, adjusted), parent.Node.Clips(child.Node) ? SKRect.Intersect(place.Clip, frame) : place.Clip,
            Layouts.Plotted(parent).Map(own => own.Translated(frame.Location)) | place.Plot);
    }

    private static SKPoint Shift(Laid parent, Laid child, HashMap<Seq<Key>, float> adjusted) =>
        (from adjuster in Adjusting(parent, None, adjusted.Find(parent.Path))
         where !Layouts.IsHandle(child)
         select (SKPoint)adjuster.Axis.With(Vector2.Zero, -adjuster.Value)).IfNone(SKPoint.Empty);

    internal static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);
}
