using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Laid {
    public required Seq<Key> Path { get; init; }
    public required Node Node { get; init; }
    public required Cascade Cascade { get; init; }
    public required Style Style { get; init; }
    public SKSize Intrinsic { get; init; }
    public SKRect Frame { get; init; }
    public Seq<Laid> Children { get; init; }
    internal Seq<Laid> Members { get; init; }

    public SKRect ContentBox => Inset switch { var inset => SKRect.Create(inset, inset, float.Max(0f, Frame.Width - (2f * inset)), float.Max(0f, Frame.Height - (2f * inset))) };

    public virtual SKRect Painted => SKRect.Inflate(Frame, Overhang, Overhang);

    public SKRect Reach =>
        Children.Filter(child => !Node.Clips(child.Node)).Fold(Painted, (union, child) => child.Reach switch {
            var reach when reach.IsEmpty => union,
            var reach => SKRect.Create(reach.Location + Frame.Location, reach.Size) switch {
                var moved when union.IsEmpty => moved,
                var moved => SKRect.Union(union, moved),
            },
        });

    public bool Timed =>
        Node.Role.IsSome && Style.Fills.Exists(static fill => fill.SwitchPartially(@default: static _ => false, drawn: static drawn => drawn.Timed));

    internal SKRect LocalReach => Reach switch { var reach => SKRect.Create(reach.Location - Frame.Location, reach.Size) };

    internal float Inset => Node.Role.IsSome ? Style.Metrics[Metric.Inset] : 0f;

    internal Option<Text> Texted => Switch(box: static _ => Option<Text>.None, text: static text => Some(text));

    private float Overhang => Node.Role.IsSome || Node.Traced.IsSome ? Style.Overhang : 0f;

    public sealed record Box : Laid;

    public sealed record Text(Node.Text Source, Typeset Typeset, TextStyle TextStyle) : Laid {
        public SKPoint Origin { get; init; }
        public float MinContent { get; init; }

        public override SKRect Painted =>
            SKRect.Create(Frame.Location + Origin, Typeset.Size) switch {
                var set => SKRect.Union(base.Painted, new SKRect(set.Left - float.Max(Overhang, Typeset.Overhang.Left), set.Top - float.Max(Overhang, Typeset.Overhang.Top),
                    set.Right + float.Max(Overhang, Typeset.Overhang.Right), set.Bottom + float.Max(Overhang, Typeset.Overhang.Bottom))),
            };
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Adjuster {
    public required Seq<Key> Owner { get; init; }
    public required Orientation Axis { get; init; }
    public required float Value { get; init; }
    public required float Min { get; init; }
    public required float Max { get; init; }
    public required float Gearing { get; init; }
    public required float Step { get; init; }

    public Cursor Cursor => Switch(scroll: static _ => Cursor.Default, split: static split => Cursor.Resizing(split.Axis));

    public float Clamped(float value) => float.Clamp(value, Min, Max);

    public Option<float> Navigated(Navigation toward) =>
        toward.Along == Axis
            ? Some(toward.Grain.Switch(
                (Adjuster: this, toward.Sign),
                unit: static held => held.Adjuster.Value + (held.Sign * held.Adjuster.Step),
                page: static held => held.Adjuster.Value + (held.Sign * held.Adjuster.Switch(scroll: static scroll => scroll.Page, split: static split => split.Step)),
                edge: static held => held.Sign < 0 ? held.Adjuster.Min : held.Adjuster.Max))
            : None;

    internal bool Scrolls(Vector2 delta) =>
        Switch(delta, scroll: static (along, scroll) => scroll.Max > scroll.Min && MathF.Abs(scroll.Axis.Of(along)) >= MathF.Abs(scroll.Axis.Cross.Of(along)), split: static (_, _) => false);

    public sealed record Scroll(float Page) : Adjuster;

    public sealed record Split : Adjuster;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hosted {
    public required Seq<Key> Path { get; init; }
    public required SKRect Frame { get; init; }
    public required SKRect Clip { get; init; }

    public sealed record Slot : Hosted;

    public sealed record Backdrop(float Radius, Glass Glass, Option<SKColorF> Tint) : Hosted;
}

internal sealed record Place(SKPoint Origin, SKRect Clip, Option<PlotFrame> Plot);

public sealed class Scene {
    // --- [STATE]
    private Scene(Laid layout, HashMap<Seq<Key>, Laid> index, HashMap<Seq<Key>, Place> places, HashMap<Seq<Key>, float> adjusted) {
        Layout = layout;
        Index = index;
        Places = places;
        Adjusted = Clamped(adjusted, index);
        Order = Preorder(layout);
        Timed = Order.Filter(static laid => laid.Timed);
        Hosted = from laid in Order
                 from place in places.Find(laid.Path).ToSeq()
                 from hosted in Hosts(laid, place)
                 select hosted;
    }

    public HashMap<Seq<Key>, float> Adjusted { get; }
    public Seq<Hosted> Hosted { get; }
    internal Laid Layout { get; }
    internal HashMap<Seq<Key>, Laid> Index { get; }
    internal HashMap<Seq<Key>, Place> Places { get; }
    internal Seq<Laid> Order { get; }
    internal Seq<Laid> Timed { get; }

    // --- [TRANSITIONS]
    internal static Fin<Scene> Of(Theme theme, Shipped shipped, Node root, SKSize size, HashMap<Seq<Key>, float> adjusted) =>
        Layouts.Unique(Layouts.Arrange(Styles.Root(theme, shipped), root, size, adjusted, []), []).Map(unique => new Scene(unique, toHashMap(Indexed(unique)), toHashMap(Placing(unique, Root(unique), adjusted)), adjusted));

    internal Fin<(Scene Scene, Seq<SKRect> Damage)> Reconcile(Node root) =>
        Layouts.Unique(Layouts.Arrange(Layout.Cascade, root, Layout.Frame.Size, Adjusted, Index), Index).Map(unique => Settled(unique, Adjusted));

    internal (Scene Scene, Seq<SKRect> Damage) Resize(SKSize size) => Arranged(Layout.Cascade, size, Adjusted, Index);

    internal (Scene Scene, Seq<SKRect> Damage) Restyle(Theme theme) => Arranged(Styles.Root(theme, Layout.Cascade.Shipped), Layout.Frame.Size, Adjusted, []);

    internal (Scene Scene, Seq<SKRect> Damage) Adjust(Seq<Key> path, Func<float, float> move) =>
        (from adjuster in Adjuster(path)
         let value = adjuster.Clamped(move(adjuster.Value))
         where value.CompareTo(adjuster.Value) is not 0
         select Readjusted(Adjusted.AddOrUpdate(adjuster.Owner, value), Seq(adjuster.Owner)))
        .IfNone((this, Seq<SKRect>()));

    internal (Scene Scene, Seq<SKRect> Damage) Reveal(Seq<Key> path) =>
        (Frame(path) | Slotted(path)).Map(target => Above(path).Tail.Choose(Adjuster).Choose(static adjuster => adjuster.Switch(scroll: static scroll => Some(scroll), split: static _ => Option<Adjuster.Scroll>.None)).Fold(
                (Adjusted, Target: target, Moved: Seq<Seq<Key>>()),
                (held, adjuster) => Revealing(adjuster, held.Target).Filter(static moved => moved.CompareTo(0f) is not 0).Match(
                    Some: moved => (held.Adjusted.AddOrUpdate(adjuster.Owner, adjuster.Value + moved), Absolute(held.Target, (SKPoint)adjuster.Axis.With(Vector2.Zero, -moved)), held.Moved.Add(adjuster.Owner)),
                    None: () => held)))
            .Filter(static revealed => !revealed.Moved.IsEmpty)
            .Match(Some: revealed => Readjusted(revealed.Adjusted, revealed.Moved), None: () => (this, Seq<SKRect>()));

    private (Scene Scene, Seq<SKRect> Damage) Arranged(Cascade root, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> prior) =>
        Settled(Layouts.Arrange(root, Layout.Node, size, adjusted, prior), adjusted);

    private (Scene Scene, Seq<SKRect> Damage) Readjusted(HashMap<Seq<Key>, float> adjusted, Seq<Seq<Key>> changed) =>
        Arranged(Layout.Cascade, Layout.Frame.Size, adjusted, Index.RemoveRange(changed.Bind(Above)));

    private (Scene Scene, Seq<SKRect> Damage) Settled(Laid next, HashMap<Seq<Key>, float> adjusted) {
        Diff diff = new(Adjusted, adjusted);
        Walked prior = new(Index, Places, []);
        Walked walked = Layout.Cascade.Theme.Equals(next.Cascade.Theme)
            ? Walk(diff, Layout, next, Root(Layout), Root(next), prior)
            : Replaced(diff, Layout, next, Root(Layout), Root(next), prior);
        return (new Scene(next, walked.Index, walked.Places, adjusted), walked.Damage);
    }

    private Option<float> Revealing(Adjuster.Scroll adjuster, SKRect target) =>
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
        Order.Exists(laid => Places.Find(laid.Path).Exists(place => place.Clip.Contains(at) && Absolute(laid.Frame, place.Origin).Contains(at)
            && ((laid.Node.Role.IsSome && (!laid.Style.Fills.IsEmpty || !laid.Style.Strokes.IsEmpty)) || laid.Node.Paints)));

    public SKSize Preferred(SKSize offered) => Layouts.Preferred(Layout, offered, Adjusted);

    internal Option<(Laid Laid, SKRect Frame, SKRect Clip, Style Style)> Ring(Seq<Key> path) =>
        Placed(path).Map(static held => (held.Laid, Absolute(held.Laid.Frame, held.Place.Origin), held.Place.Clip, Styles.Resolve(held.Laid.Cascade, Role.FocusRing, held.Laid.Style.Conditions)));

    internal Option<Adjuster> Adjuster(Seq<Key> path) =>
        Index.Find(path).Bind(laid => Layouts.IsHandle(laid)
            ? Index.Find(path.Init).Bind(owner => Adjusting(owner, Some(laid), Adjusted.Find(owner.Path)))
            : Adjusting(laid, None, Adjusted.Find(path)));

    internal Seq<(Laid Laid, SKPoint Local)> Hit(SKPoint at) =>
        Order.Rev().Choose(laid => Reached(laid, at)).Head
            .Map(first => first.Cons(Above(first.Laid.Path).Tail.Choose(Index.Find).Choose(laid => Reached(laid, at))))
            .IfNone([]);

    internal SKPoint Shift(Laid parent, Laid child) => Shift(parent, child, Adjusted);

    internal static Seq<Seq<Key>> Above(Seq<Key> path) => toSeq(Range(0, path.Count + 1)).Rev().Map(path.Take);

    internal static Seq<Laid> Preorder(Laid laid) => laid.Cons(laid.Children.Bind(Preorder));

    private Option<(Laid Laid, Place Place)> Placed(Seq<Key> path) =>
        from laid in Index.Find(path)
        from place in Places.Find(path)
        select (laid, place);

    private Option<(Laid Laid, SKPoint Local)> Reached(Laid laid, SKPoint at) =>
        from place in Places.Find(laid.Path)
        where place.Clip.Contains(at) && (laid.Node.Key.IsSome || laid.Node.Part.IsSome)
        let frame = Absolute(laid.Frame, place.Origin)
        let reach = laid.Style.Metrics[Metric.Reach]
        where laid.Node.Traced.Match(
            Some: figure => place.Plot.Exists(plot => figure.Hits(plot, at, reach + laid.Style.HalfStroke)),
            None: () => (laid.Node.Pinned.IsSome || Layouts.IsHandle(laid) ? SKRect.Inflate(frame, reach, reach) : frame).Contains(at))
        select (laid, at - frame.Location);

    private static Option<Adjuster> Adjusting(Laid owner, Option<Laid> handle, Option<float> stored) =>
        owner.Node.SwitchPartially((Owner: owner, Handle: handle, Stored: stored), @default: static (_, _) => Option<Adjuster>.None, scroll: Scrolled, split: Divided);

    private static Option<Adjuster> Scrolled((Laid Owner, Option<Laid> Handle, Option<float> Stored) held, Node.Scroll node) =>
        Layouts.Scrollable(held.Owner, node).Map(scroll => (Adjuster)new Adjuster.Scroll(scroll.Viewport) {
            Owner = held.Owner.Path,
            Axis = node.Axis,
            Value = Layouts.Scrolling(held.Stored.IfNone(0f), scroll.Content, scroll.Viewport),
            Min = 0f,
            Max = float.Max(0f, scroll.Content - scroll.Viewport),
            Gearing = held.Handle.Map(thumb => Layouts.Rescaled(1f, scroll.Viewport - node.Axis.Of(thumb.Frame.Size), scroll.Content - scroll.Viewport)).IfNone(1f),
            Step = held.Owner.Style.Metrics[Metric.Spacing],
        });

    private static Option<Adjuster> Divided((Laid Owner, Option<Laid> Handle, Option<float> Stored) held, Node.Split node) =>
        Layouts.Travel(held.Owner, node).Map(split => (Adjuster)new Adjuster.Split {
            Owner = held.Owner.Path, Axis = node.Axis, Value = split.Extent, Min = split.Least, Max = split.Most, Gearing = 1f, Step = held.Owner.Style.Metrics[Metric.Spacing],
        });

    private static HashMap<Seq<Key>, float> Clamped(HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> index) =>
        toHashMap(toSeq(adjusted.AsIterable()).Choose(entry => index.Find(entry.Key).Bind(laid => Adjusting(laid, None, Some(entry.Value))).Map(adjuster => (entry.Key, adjuster.Value))));

    private static Seq<Hosted> Hosts(Laid laid, Place place) =>
        laid.Node.SwitchPartially((Laid: laid, Frame: Absolute(laid.Frame, place.Origin), place.Clip), @default: Backed, slot: Hosting);

    private static Seq<Hosted> Hosting((Laid Laid, SKRect Frame, SKRect Clip) held, Node.Slot _) => [new Hosted.Slot { Path = held.Laid.Path, Frame = held.Frame, Clip = held.Clip }];

    private static Seq<Hosted> Backed((Laid Laid, SKRect Frame, SKRect Clip) held, Node node) =>
        node.Role.IsNone
            ? []
            : held.Laid.Style.Fills.Choose(static fill => fill.SwitchPartially(@default: static _ => Option<Material.Backdrop>.None, backdrop: static backdrop => Some(backdrop)))
                .Map(backdrop => (Hosted)new Hosted.Backdrop(held.Laid.Style.Metrics[Metric.Radius], backdrop.Glass, backdrop.Tint.Bind(tint => held.Laid.Cascade.Theme.Color(tint, held.Laid.Style, None, None))) {
                    Path = held.Laid.Path, Frame = held.Frame, Clip = held.Clip,
                });

    // --- [DAMAGE]
    private sealed record Diff(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now);

    private sealed record Walked(HashMap<Seq<Key>, Laid> Index, HashMap<Seq<Key>, Place> Places, Seq<SKRect> Damage);

    private static Walked Walk(Diff diff, Laid before, Laid after, Place was, Place now, Walked state) {
        bool same = ReferenceEquals(before, after);
        bool moved = was != now || before.Node.Plotted != after.Node.Plotted || before.Node.Alpha.CompareTo(after.Node.Alpha) is not 0;
        if (same && !moved) return state;
        if (moved) return Replaced(diff, before, after, was, now, state);
        bool own = before.Frame != after.Frame || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.Node.Equals(after.Node));
        HashMap<Seq<Key>, Laid> prior = toHashMap(before.Children.Map(static child => (child.Path, child)));
        LanguageExt.HashSet<Seq<Key>> kept = toHashSet(after.Children.Map(static child => child.Path));
        Walked walked = after.Children.Fold(
            state with { Index = state.Index.AddOrUpdate(after.Path, after), Damage = own ? state.Damage.Add(Painted(before, was)).Add(Painted(after, now)) : state.Damage },
            (held, child) => prior.Find(child.Path).Match(
                Some: matched => Walk(diff, matched, child, Within(before, matched, was, diff.Was), Within(after, child, now, diff.Now), held),
                None: () => Added(diff, child, Within(after, child, now, diff.Now), held)));
        return before.Children.Filter(child => !kept.Contains(child.Path)).Fold(walked, (held, child) => Removed(child, Within(before, child, was, diff.Was), held));
    }

    private static Walked Replaced(Diff diff, Laid before, Laid after, Place was, Place now, Walked state) => Added(diff, after, now, Removed(before, was, state));

    private static Walked Added(Diff diff, Laid laid, Place place, Walked state) =>
        new(state.Index.AddOrUpdateRange(Indexed(laid)), state.Places.AddOrUpdateRange(Placing(laid, place, diff.Now)), state.Damage.Concat(Extents(laid, place)));

    private static Walked Removed(Laid laid, Place place, Walked state) =>
        Preorder(laid).Map(static held => held.Path) switch {
            var paths => new(state.Index.RemoveRange(paths), state.Places.RemoveRange(paths), state.Damage.Concat(Extents(laid, place))),
        };

    private static Seq<SKRect> Extents(Laid laid, Place place) =>
        SKRect.Intersect(Absolute(laid.Reach, place.Origin), place.Clip) switch {
            var reach when reach.IsEmpty => [],
            var reach => [reach],
        };

    private static SKRect Painted(Laid laid, Place place) => SKRect.Intersect(Absolute(laid.Painted, place.Origin), place.Clip);

    // --- [PLACEMENT]
    private static Place Root(Laid laid) => new(SKPoint.Empty, SKRect.Create(laid.Frame.Size), None);

    private static Seq<(Seq<Key> Path, Laid Laid)> Indexed(Laid laid) => Preorder(laid).Map(static held => (held.Path, held));

    private static Seq<(Seq<Key> Path, Place Place)> Placing(Laid laid, Place place, HashMap<Seq<Key>, float> adjusted) =>
        (laid.Path, place).Cons(laid.Children.Bind(child => Placing(child, Within(laid, child, place, adjusted), adjusted)));

    private static Place Within(Laid parent, Laid child, Place place, HashMap<Seq<Key>, float> adjusted) =>
        Absolute(parent.Frame, place.Origin) switch {
            var frame => new Place(frame.Location + Shift(parent, child, adjusted), parent.Node.Clips(child.Node) ? SKRect.Intersect(place.Clip, frame) : place.Clip,
                Layouts.Plotted(parent).Map(own => own.Translated(frame.Location)) | place.Plot),
        };

    private static SKPoint Shift(Laid parent, Laid child, HashMap<Seq<Key>, float> adjusted) =>
        (from adjuster in Adjusting(parent, None, adjusted.Find(parent.Path))
         where !Layouts.IsHandle(child)
         select adjuster.Switch(scroll: static scroll => (SKPoint)scroll.Axis.With(Vector2.Zero, -scroll.Value), split: static _ => SKPoint.Empty)).IfNone(SKPoint.Empty);

    private static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);
}
