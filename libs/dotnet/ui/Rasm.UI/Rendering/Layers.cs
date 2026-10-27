using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class LayerStack {
    // --- [PLACEMENT]
    internal static readonly Key Scrim = new Key.Named("scrim");

    private static float Start(PhysicalAxis axis, SKRect anchor, SKSize content, SKRect window, Node.Layer layer) {
        float extent = axis.Of(content);
        float low = axis.Of(anchor.Location);
        float high = axis.End(anchor);
        float side = layer.Side.Of(axis);
        Seq<float> starts = side is 0f ? Seq(low - layer.Gap - extent, high + layer.Gap)
            : side is 1f ? Seq(high + layer.Gap, low - layer.Gap - extent)
            : Seq(low + (layer.Align.Of(axis) * (high - low - extent)));
        return (layer.Flips ? starts : starts.Take(1)).Find(start => start >= axis.Of(window.Location) && start + extent <= axis.End(window)).IfNone(starts[0]);
    }

    // --- [STACK]
    internal static Seq<LayoutNode> Declared(LayoutNode root) => Scene.Preorder(root).Bind(static layout => layout.Layers);

    internal static Seq<LayoutNode> Stacked(Layouts.Pass pass, LayoutNode root, HashSet<Seq<Key>> opened, SKRect window, HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places) =>
        Stacking(pass, Declared(root), opened, window, (Laid: [], Index: index, Places: places)).Laid;

    private static (Seq<LayoutNode> Laid, HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places) Stacking(
        Layouts.Pass pass, Seq<LayoutNode> declared, HashSet<Seq<Key>> opened, SKRect window, (Seq<LayoutNode> Laid, HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places) stack) =>
        declared.Fold(stack, (held, layer) =>
            (from node in Optional(layer.Node as Node.Layer)
             where node.Hover.IsNone || opened.Contains(layer.Path)
             from anchor in node.Anchor.Match(
                 Some: path => from target in held.Index.Find(path) from place in held.Places.Find(path) select place.Transform.MapRect(Scene.Absolute(target.Frame, place.Origin)),
                 None: () => Some(window))
             let laid = Laid(pass, layer, node, anchor, window)
             select Stacking(pass, Declared(laid), opened, window, (held.Laid.Add(laid), held.Index.AddOrUpdateRange(Scene.Indexed(laid).AsIterable()),
                 Scene.Placing(laid, Scene.Root(laid, window), pass.Offsets, held.Places)))).IfNone(held));

    private static LayoutNode Laid(Layouts.Pass pass, LayoutNode layer, Node.Layer node, SKRect anchor, SKRect window) {
        SKSize size = Layouts.Preferred(pass, layer, window.Size);
        SKRect content = SKRect.Create(new SKPoint(Start(PhysicalAxis.Horizontal, anchor, size, window, node), Start(PhysicalAxis.Vertical, anchor, size, window, node)), size);
        SKRect frame = node.Modal ? window : content;
        Seq<LayoutNode> children = layer.Members.Map(member => Layouts.Framed(pass, member, IsScrim(member) ? SKRect.Create(frame.Size) : SKRect.Create(content.Location - frame.Location, content.Size)));
        return Layouts.WithCorners(layer with { Frame = frame, Members = children, Children = children }, None);
    }

    // --- [QUERY]
    internal static HashSet<Seq<Key>> Inert(LayoutNode root, Seq<LayoutNode> layers) =>
        toSeq(layers.Rev().SkipWhile(static layer => layer.Node is not Node.Layer { Modal: true })) switch {
            var modal when modal.IsEmpty => [],
            var modal => toHashSet(root.Cons(modal.Tail).Bind(Scene.Preorder).Map(static held => held.Path)),
        };

    internal static Option<Hosted> Popup(LayoutNode layer, SKRect window) =>
        from content in layer.Children.Find(static child => !IsScrim(child))
        let frame = Scene.Absolute(content.Frame, layer.Frame.Location)
        where !window.Contains(frame)
        select (Hosted)new Hosted.Popup(content.Corners) { Path = layer.Path, Frame = frame, Clip = frame };

    private static bool IsScrim(LayoutNode member) => member.Path.Last == Some(Scrim);
}
