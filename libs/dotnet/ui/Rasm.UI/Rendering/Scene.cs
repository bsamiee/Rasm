using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Hosting;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;
using Wacton.Unicolour;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record SceneChange(Scene Scene, Seq<SKRect> Damage);

internal sealed record PartOwner(Seq<Key> Path, LayoutNode Layout, Part Part, SKRect Bounds) {
    internal bool Enabled => !Layout.Style.Conditions.Contains(Condition.Disabled);
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed record Scene {
    // --- [STATE]
    private static readonly Key Overlaid = new Key.Named("fragment");
    private readonly HashMap<Seq<Key>, Option<SKPoint>> opened;
    private readonly HashMap<Seq<Key>, Seq<Seq<Key>>> readers;
    private readonly HashMap<Seq<Key>, Seq<PlacedLayer>> anchors;
    private readonly Seq<(SKRect Reach, SKRect Bounds)> sampling;

    private Scene(Host host, LayoutPass pass, Slot<OutlineKey, Outline> outlines, LayoutNode root, Seq<LayoutNode> layers, HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places,
        HashMap<Seq<Key>, Option<SKPoint>> opened, HashSet<Seq<Key>> open, HashMap<Seq<Key>, LayoutNode> fragments) {
        (Host, Shaping, Outlines, Layout, Layers, Open, Index, Places, Fragments, this.opened) = (host, pass.Shaping, outlines, root, layers, open, index, places, fragments, opened);
        Order = root.Preorder + layers.Bind(static layer => layer.Preorder);
        readers = Order.Fold(HashMap<Seq<Key>, Seq<Seq<Key>>>(), static (held, layout) =>
            layout.View is View.Scroll scroll ? held.AddOrUpdate(Layouts.Stored(layout, scroll), Some: paths => paths.Add(layout.Path), None: () => [layout.Path]) : held);
        Offsets = toHashMap(toSeq(pass.Offsets.AsIterable()).Choose(entry => readers.Find(entry.Key).IfNone([]).Choose(index.Find).Choose(layout => Layouts.Scroller(layout, None, pass.Offsets)).Head.Map(scroller => (entry.Key, scroller.Held(scroller.Value)))));
        Timed = Order.Filter(static layout => layout.Timed);
        Highlighted = Order.Filter(static layout => layout.View.Conditions.Contains(Condition.Highlighted)).Map(static layout => layout.Path);
        Triggered = toSeq(index.Values).Choose(PlacedLayer.Of).Filter(static held => held.Layer.Trigger.IsSome);
        anchors = Triggered.Fold(HashMap<Seq<Key>, Seq<PlacedLayer>>(), static (held, layer) => layer.Anchor.Map(anchor => held.AddOrUpdate(anchor, Some: layers => layers.Add(layer), None: () => [layer])).IfNone(held));
        Inert = LayerStack.Inert(root, layers);
        Hosted = Order.Fold(Seq<Hosted>(), (hosted, layout) => Places.Find(layout.Path).Map(place => hosted + Hosting(hosted, new PlacedNode(layout, place))).IfNone(hosted))
            + layers.Choose(layer => LayerStack.Popup(layer, SKRect.Create(root.Frame.Size)));
        Backdrops = Elected<Hosted.Backdrop>(Hosted);
        Popups = Elected<Hosted.Popup>(Hosted);
        sampling = Order.Choose(layout =>
            from glass in layout.Style.Glass.Filter(_ => layout.View.Role.IsSome && !Backdrops.Contains(layout.Path))
            from bounds in Bounds(layout.Path)
            let reach = Filter.Outset(glass.Backdrop) + glass.Refraction.IfNone(0f)
            select (SKRect.Inflate(bounds, reach, reach), bounds));
        Extended = (float)host.Headroom > 1f
            || ((host.Theme.Configuration.Rgb, RgbConfiguration.StandardRgb) is var (gamut, standard)
                && (gamut.ChromaticityR, gamut.ChromaticityG, gamut.ChromaticityB) != (standard.ChromaticityR, standard.ChromaticityG, standard.ChromaticityB))
            || Order.Exists(static layout => layout.View is View.Image { Source.Info: var info } && (info.ColorType is SKColorType.RgbaF16 or SKColorType.RgbaF32 || info.ColorSpace is { IsSrgb: false }));
    }

    internal Host Host { get; }
    internal Shaping Shaping { get; }
    internal Slot<OutlineKey, Outline> Outlines { get; }
    internal LayoutNode Layout { get; }
    internal Seq<LayoutNode> Layers { get; }
    internal HashSet<Seq<Key>> Open { get; }
    internal HashMap<Seq<Key>, LayoutNode> Index { get; }
    internal HashMap<Seq<Key>, Place> Places { get; }
    internal HashMap<Seq<Key>, LayoutNode> Fragments { get; private init; }
    internal HashMap<Seq<Key>, float> Offsets { get; }
    internal Seq<LayoutNode> Order { get; }
    internal Seq<LayoutNode> Timed { get; }
    internal Seq<Seq<Key>> Highlighted { get; }
    internal Seq<PlacedLayer> Triggered { get; }
    internal HashSet<Seq<Key>> Inert { get; }
    internal Seq<Hosted> Hosted { get; }
    internal HashSet<Seq<Key>> Backdrops { get; }
    internal HashSet<Seq<Key>> Popups { get; }
    internal bool Extended { get; }

    public bool Equals([NotNullWhen(true)] Scene? other) => ReferenceEquals(this, other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    // --- [TRANSITIONS]
    internal static Fin<Scene> Of(Cascade root, View view, Host host, Shaping shaping) {
        LayoutPass pass = new([], [], shaping, host.Culture);
        return Layouts.Unique(Layouts.Arrange(pass, root, view, host.Size), []).Map(main => Laid(host, pass, new Slot<OutlineKey, Outline>(static value => value.Dispose()), main, [], None).Scene);
    }

    internal Fin<SceneChange> Reconcile(View root) {
        LayoutPass pass = Pass(Index, Offsets, Host.Culture);
        return Layouts.Unique(Layouts.Arrange(pass, Layout.Cascade, root, Host.Size), Index).Map(main => Laid(Host, pass, Outlines, main, opened, Some(this)));
    }

    internal SceneChange Resize(Host host) => Arranged(Layout.Cascade, host, Offsets, host.Culture.Equals(Host.Culture) ? Index : [], opened);

    internal SceneChange Restyle(Host host, StyleSheet sheet) => Arranged(Cascade.Root(host.Theme, sheet, host.Root), host, Offsets, [], opened);

    internal SceneChange Opened(HashMap<Seq<Key>, Option<SKPoint>> next) => next == opened ? Unchanged : Arranged(Layout.Cascade, Host, Offsets, Index, next);

    internal SceneChange Unchanged => new(this, []);

    internal SceneChange Adjust(Seq<Key> path, Func<float, float> move) =>
        (from scroller in Scroller(path)
         let value = scroller.Overflow.Clamped(move(scroller.Value))
         where value.CompareTo(scroller.Value) is not 0
         select Offset(Offsets.AddOrUpdate(scroller.Stored, scroller.Held(value)), Seq(scroller.Stored)))
        .IfNone(Unchanged);

    internal SceneChange Reveal(Seq<Key> path) =>
        (Frame(path) | Reserved(path)).Map(target => path.Inits.Rev().Tail.Choose(Scroller).Fold(
                (Offsets, Target: target, Moved: Seq<Seq<Key>>()),
                (held, scroller) => Placed(scroller.Owner).Map(owner => scroller.Revealing(Place.Absolute(owner.Layout.ContentBox, owner.Frame.Location), held.Target))
                    .Filter(static moved => moved.CompareTo(0f) is not 0).Match(
                        Some: moved => (held.Offsets.AddOrUpdate(scroller.Stored, scroller.Held(scroller.Value + moved)), Place.Absolute(held.Target, (SKPoint)scroller.Axis.With(Vector2.Zero, -moved)), held.Moved.Add(scroller.Stored)),
                        None: () => held)))
            .Filter(static revealed => !revealed.Moved.IsEmpty)
            .Match(Some: revealed => Offset(revealed.Offsets, revealed.Moved), None: () => Unchanged);

    internal SceneChange Revealed(Option<Scene> prior) =>
        Order.Choose(layout => layout.Lead.Filter(lead => prior.Bind(held => held.Index.Find(layout.Path)).Bind(static held => held.Lead) != Some(lead)))
            .Fold(Unchanged, static (held, lead) => held.Scene.Reveal(lead) switch { var shown => shown with { Damage = held.Damage.Concat(shown.Damage) } });

    internal SceneChange Fragment(Seq<Key> plot, Option<View> overlay) {
        Option<LayoutNode> was = Fragments.Find(plot);
        Option<LayoutNode> now = from node in overlay from layout in Index.Find(plot) select Fragmented(Pass(Index, Offsets, Host.Culture), layout, node, was);
        return new SceneChange(this with { Fragments = now.Match(Some: laid => Fragments.AddOrUpdate(plot, laid), None: () => Fragments.Remove(plot)) },
            was.Bind(held => Reached(Index, Places, Offsets, plot, held)).ToSeq() + now.Bind(laid => Reached(Index, Places, Offsets, plot, laid)).ToSeq());
    }

    private LayoutPass Pass(HashMap<Seq<Key>, LayoutNode> prior, HashMap<Seq<Key>, float> offsets, CultureInfo culture) => new(prior, offsets, Shaping, culture);

    private SceneChange Arranged(Cascade root, Host host, HashMap<Seq<Key>, float> offsets, HashMap<Seq<Key>, LayoutNode> prior, HashMap<Seq<Key>, Option<SKPoint>> next) {
        LayoutPass pass = Pass(prior, offsets, host.Culture);
        return Laid(host, pass, Outlines, Layouts.Arrange(pass, root, Layout.View, host.Size), next, Some(this));
    }

    private SceneChange Offset(HashMap<Seq<Key>, float> offsets, Seq<Seq<Key>> stored) =>
        Arranged(Layout.Cascade, Host, offsets, Index.RemoveRange(stored.Bind(key => readers.Find(key).IfNone([])).Bind(static held => held.Inits)), opened);

    private static SceneChange Laid(Host host, LayoutPass pass, Slot<OutlineKey, Outline> outlines, LayoutNode main, HashMap<Seq<Key>, Option<SKPoint>> opened, Option<Scene> prior) {
        SKRect window = SKRect.Create(main.Frame.Size);
        SKRect was = prior.Map(static held => SKRect.Create(held.Layout.Frame.Size)).IfNone(window);
        WalkState walked = Matched(prior.Map(static held => held.Layout).ToSeq(), [main], layout => Place.Root(layout, was), layout => Place.Root(layout, window),
            new WalkState(prior.Map(static held => held.Offsets).IfNone(pass.Offsets), pass.Offsets, prior.Exists(held => !held.Layout.Cascade.Theme.Equals(main.Cascade.Theme)),
                prior.Map(static held => held.Index).IfNone([]), prior.Map(static held => held.Places).IfNone([]), []));
        Seq<LayoutNode> layers = LayerStack.Stacked(pass, LayerStack.Declared(main), opened, window, ([], walked.Index, walked.Places)).Laid;
        WalkState stacked = Matched(prior.Map(static held => held.Layers).IfNone([]), layers, layer => Place.Root(layer, was), layer => Place.Root(layer, window), walked);
        Seq<(Seq<Key> Plot, Option<SKRect> Was, LayoutNode Laid)> fragments = prior.Map(held => toSeq(held.Fragments.AsIterable()).Choose(entry =>
            stacked.Index.Find(entry.Key).Map(plot => (entry.Key, Reached(held.Index, held.Places, held.Offsets, entry.Key, entry.Value), Fragmented(pass, plot, entry.Value.View, Some(entry.Value)))))).IfNone([]);
        HashSet<Seq<Key>> open = toHashSet(layers.Map(static layer => layer.Path));
        HashMap<Seq<Key>, LayoutNode> index = stacked.Index.RemoveRange(prior.ToSeq().Bind(static held => held.Layout.Cons(held.Layers)).Bind(LayerStack.Declared).Map(static layer => layer.Path).Filter(path => !open.Contains(path)))
            .AddOrUpdateRange(main.Cons(layers).Bind(LayerStack.Declared).Filter(layer => !open.Contains(layer.Path)).Map(static layer => (layer.Path, layer)));
        Scene scene = new(host, pass, outlines, main, layers, index, stacked.Places, opened, open, toHashMap(fragments.Map(static held => (held.Plot, held.Laid))));
        return new SceneChange(scene, stacked.Damage + fragments.Bind(held => Reached(stacked.Index, stacked.Places, scene.Offsets, held.Plot, held.Laid) switch {
            var now when now == held.Was => Seq<SKRect>(),
            var now => held.Was.ToSeq() + now.ToSeq(),
        }));
    }

    private Option<SKRect> Reserved(Seq<Key> path) =>
        from key in path.Last
        from lazy in Placed(path.Init)
        from node in Optional(lazy.Layout.View as View.Lazy)
        from reserved in Layouts.Reserved(lazy.Layout, node, key)
        select Place.Absolute(reserved, lazy.Frame.Location);

    private static LayoutNode Fragmented(LayoutPass pass, LayoutNode plot, View overlay, Option<LayoutNode> held) =>
        Layouts.WithCorners(Layouts.Framed(pass, Layouts.Built(pass, plot.Inner, plot.Path.Add(Overlaid), overlay, held), SKRect.Create(plot.Frame.Size)), None);

    // --- [DAMAGE]
    private sealed record WalkState(HashMap<Seq<Key>, float> Was, HashMap<Seq<Key>, float> Now, bool Restyled, HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places, Seq<SKRect> Damage);

    internal Seq<SKRect> Sampled(Seq<SKRect> damage) => damage + sampling.Filter(glass => damage.Exists(rect => rect.IntersectsWith(glass.Reach))).Map(static glass => glass.Bounds);

    private static WalkState Walk(LayoutNode before, LayoutNode after, Place was, Place now, WalkState state) {
        bool same = ReferenceEquals(before, after);
        bool moved = state.Restyled || was != now || before.View.Plotted != after.View.Plotted || before.View.Opacity != after.View.Opacity;
        if (same && !moved) return state;
        if (moved) return Added(after, now, Removed(before, was, state));
        bool own = before.Frame != after.Frame || before.Corners != after.Corners || !before.Style.Equals(after.Style) || (before.Children.IsEmpty && after.Children.IsEmpty && !before.View.Equals(after.View));
        return Matched(before.Children, after.Children, child => was.Within(before, child, state.Was), child => now.Within(after, child, state.Now),
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
        state with { Index = state.Index.AddOrUpdateRange(layout.Indexed.AsIterable()), Places = Place.Placing(layout, place, state.Now, state.Places), Damage = state.Damage.Concat(Extents(layout, place)) };

    private static WalkState Removed(LayoutNode layout, Place place, WalkState state) {
        Seq<Seq<Key>> paths = layout.Preorder.Map(static held => held.Path);
        return state with { Index = state.Index.RemoveRange(paths), Places = state.Places.RemoveRange(paths), Damage = state.Damage.Concat(Extents(layout, place)) };
    }

    private static Seq<SKRect> Extents(LayoutNode layout, Place place) =>
        SKRect.Intersect(Place.Absolute(layout.SubtreeBounds, place.Origin), place.Clip) switch {
            var reach when reach.IsEmpty => [],
            var reach => [place.Transform.MapRect(reach)],
        };

    private static SKRect Painted(LayoutNode layout, Place place) => place.Transform.MapRect(SKRect.Intersect(Place.Absolute(layout.Painted, place.Origin), place.Clip));

    private static Option<SKRect> Reached(HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places, HashMap<Seq<Key>, float> offsets, Seq<Key> plot, LayoutNode fragment) =>
        from layout in index.Find(plot)
        from place in places.Find(plot)
        from reach in Extents(fragment, place.Within(layout, fragment, offsets)).Head
        select reach;

    // --- [QUERY]
    internal Option<SKRect> Frame(Seq<Key> path) => Placed(path).Map(static held => held.Place.Transform.MapRect(held.Frame));

    internal Option<SKRect> Bounds(Seq<Key> path) => Placed(path).Map(static held => Painted(held.Layout, held.Place));

    internal Option<SKRect> Visible(Seq<Key> path) => Placed(path).Map(static held => held.Place.Transform.MapRect(SKRect.Intersect(held.Frame, held.Place.Clip))).Filter(static shown => !shown.IsEmpty);

    internal SKSize Preferred(SKSize offered) => Layouts.Preferred(Pass([], Offsets, Host.Culture), Layout, offered);

    internal Seq<PlacedLayer> Anchoring(Seq<Key> path) => anchors.Find(path).IfNone([]);

    internal Option<PlacedNode> FocusRing(Seq<Key> path) =>
        Placed(path).Map(static held => held with { Style = held.Layout.Styled(Roles.FocusRing) });

    internal Option<Scroller> Scroller(Seq<Key> path) =>
        Index.Find(path).Bind(layout => Layouts.IsHandle(layout)
            ? Index.Find(path.Init).Bind(owner => Layouts.Scroller(owner, Some(layout), Offsets))
            : Layouts.Scroller(layout, None, Offsets));

    internal SKPoint Shift(LayoutNode parent, LayoutNode child) =>
        Places.Find(parent.Path).Map(place => place.Within(parent, child, Offsets).Origin - Place.Absolute(parent.Frame, place.Origin).Location).IfNone(() => Place.Scrolled(parent, child, Offsets));

    private Option<PlacedNode> Placed(Seq<Key> path) =>
        (from layout in Index.Find(path) from place in Places.Find(path) select new PlacedNode(layout, place)) switch {
            { IsSome: true } placed => placed,
            _ => from key in path.Last
                 from span in Optional(key as Key.Ordinal)
                 from held in Placed(path.Init)
                 from text in held.Layout.TextNode
                 from runs in text.Typeset.Parts.Find((int)span.Index)
                 let lines = runs.Map(static run => run.Bounds)
                 from laid in lines.Head.Map(first => lines.Tail.Fold(first, SKRect.Union))
                 select held with { Frame = SKRect.Create(held.Frame.Location + text.Origin + laid.Location, laid.Size) },
        };

    private Seq<Hosted> Hosting(Seq<Hosted> earlier, PlacedNode held) =>
        held.Layout.View is View.Slot or View.Text { Edited.IsSome: true }
            ? [new Hosted.Slot(Optional(held.Layout.View as View.Text)) { Path = held.Layout.Path, Frame = held.Frame, Clip = held.Place.Clip }]
            : (from glass in held.Layout.Style.Glass.Filter(_ => held.Layout.View.Role.IsSome && held.Layout.Corners.Uniform.IsSome)
               where Host.Blending.Native(earlier.Exists(prior => prior is Hosted.Slot && prior.Frame.IntersectsWith(held.Frame)))
               select (Hosted)new Hosted.Backdrop(held.Layout.Corners, glass.Clear, held.Layout.Style.Tint.Bind(tint => held.Layout.Cascade.Theme.Color(tint, held.Layout.Style))) {
                   Spacing = Index.Find(held.Layout.Path.Init).Map(static parent => parent.Style.Metrics[Metric.Gap]).IfNone(0f),
                   Path = held.Layout.Path, Frame = held.Frame, Clip = held.Place.Clip,
               }).ToSeq();

    private static HashSet<Seq<Key>> Elected<T>(Seq<Hosted> hosted) where T : Hosted => toHashSet(hosted.Filter(static held => held is T).Map(static held => held.Path));

    // --- [HITS]
    internal Seq<Seq<Key>> Hit(SKPoint at) =>
        Order.Rev().Choose(layout => HitAt(layout, at).Map(hit => (Layout: layout, Hit: hit))).Head
            .Map(first => first.Hit.Cons(first.Layout.Path.Inits.Rev().Tail.Choose(Index.Find).Choose(layout => HitAt(layout, at))))
            .IfNone([]);

    internal bool Covers(SKPoint at) =>
        Order.Exists(layout => (layout.HasBackground || layout.View.Painting.Paints)
            && Places.Find(layout.Path).Exists(place => place.Local(at).Exists(local => place.Clip.Contains(local) && Holds(layout, place, local, 0f))));

    internal Option<Nearest> Nearest(Seq<Key> plot, SKPoint at) =>
        from layout in Index.Find(plot)
        let data = toHashMap(layout.Preorder.Choose(mark =>
            from figure in mark.View.Traced
            from key in mark.Path.Last
            from place in Places.Find(mark.Path)
            from frame in place.Plot
            from local in place.Local(at)
            from datum in figure.Nearest(frame, local, None)
            select (key, datum)))
        where !data.IsEmpty
        select new Nearest(plot, data);

    private Option<Seq<Key>> HitAt(LayoutNode layout, SKPoint at) =>
        from place in Places.Find(layout.Path)
        where !Inert.Contains(layout.Path)
        from local in place.Local(at)
        where place.Clip.Contains(local)
        let frame = Place.Absolute(layout.Frame, place.Origin)
        let reach = layout.Style.Metrics[Metric.Reach]
        from hit in layout.TextNode.Bind(text => text.Typeset.PartAt(local - frame.Location - text.Origin)).Map(head => layout.Path.Add(new Key.Ordinal((uint)head)))
            | ((layout.View.Key.IsSome || layout.View.Part.IsSome || layout.View is View.Plot { Hovered.IsSome: true } || layout.View.Lift.IsSome || layout.View.Drop.IsSome || anchors.ContainsKey(layout.Path)) && layout.View.Traced.Match(
                Some: figure => place.Plot.Exists(plot => figure.Hits(plot, local, reach + layout.Style.HalfStroke)),
                None: () => Holds(layout, place, local, layout.View.Pinned.IsSome || Layouts.IsHandle(layout) ? reach : 0f))
                ? Some(layout.Path)
                : None)
        select hit;

    private bool Holds(LayoutNode layout, Place place, SKPoint local, float reach) {
        SKRect frame = Place.Absolute(layout.Frame, place.Origin);
        return layout.View.Role.IsNone
            ? SKRect.Inflate(frame, reach, reach).Contains(local)
            : Outline.Of(Outlines, layout, SKRect.Create(frame.Size), reach).Exists(outline => outline.Contains(local - frame.Location));
    }

    // --- [PARTS]
    internal Option<PartOwner> PartAt(Seq<Key> path) =>
        from owned in (from layout in Index.Find(path) from part in layout.View.Part select (Layout: layout, Part: part))
                      | (from key in path.Last
                         from span in Optional(key as Key.Ordinal)
                         from layout in Index.Find(path.Init)
                         from text in layout.TextNode
                         from spanned in text.Typeset.Text.Spans.At((int)span.Index)
                         from part in spanned.Part
                         select (Layout: layout, Part: part))
        from bounds in Frame(path)
        select new PartOwner(path, owned.Layout, owned.Part, bounds);

    internal Option<PartOwner> EnabledPartAt(Seq<Key> path) => PartAt(path).Filter(static owner => owner.Enabled);

    internal Seq<PartOwner> Owners(Seq<Key> path) => path.Inits.Rev().Choose(EnabledPartAt);

    internal Option<PartOwner> Owner(Seq<Key> path) => Owners(path).Head;

    internal Seq<(PartOwner Owner, Seq<Key> Hit)> Chain(Seq<Seq<Key>> hits) => toSeq(hits.Choose(hit => Owner(hit).Map(owner => (Owner: owner, Hit: hit))).DistinctBy(static held => held.Owner.Path));

    internal Seq<Scroller> Scrollers(Seq<Seq<Key>> hits, Vector2 delta) => hits.Head.Map(static hit => hit.Inits.Rev()).IfNone([]).Choose(Scroller).Filter(scroller => scroller.Scrolls(delta));
}
