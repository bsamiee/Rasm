using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class LayerStack {
    // --- [PLACEMENT]
    internal static readonly Key Veil = new Key.Named("veil");

    internal static SKRect Placed(SKRect anchor, SKSize content, SKRect window, Node.Layer layer) =>
        SKRect.Create(Start(PhysicalAxis.Horizontal, anchor, content, window, layer), Start(PhysicalAxis.Vertical, anchor, content, window, layer), content.Width, content.Height);

    private static float Start(PhysicalAxis axis, SKRect anchor, SKSize content, SKRect window, Node.Layer layer) {
        float extent = axis.Of(content);
        float low = axis.Of(anchor.Location);
        float high = axis.Of(Gutter.Far(anchor));
        Fraction side = axis.Map(horizontal: layer.Side.X, vertical: layer.Side.Y);
        Seq<float> starts = side == Fraction.MinValue ? Seq(low - layer.Gap - extent, high + layer.Gap)
            : side == Fraction.MaxValue ? Seq(high + layer.Gap, low - layer.Gap - extent)
            : Seq(low + (layer.Align.Of(axis) * (high - low - extent)));
        return (layer.Flips ? starts : starts.Take(1)).Find(start => start >= axis.Of(window.Location) && start + extent <= axis.Of(Gutter.Far(window))).IfNone(starts[0]);
    }

    // --- [STACK]
    private sealed record Stack(Seq<LayoutNode> Laid, HashMap<Seq<Key>, LayoutNode> Index, HashMap<Seq<Key>, Place> Places);

    internal static Seq<LayoutNode> Declared(LayoutNode root) => Scene.Preorder(root).Bind(static layout => layout.Layers);

    internal static Seq<LayoutNode> Stacked(Layouts.Pass pass, LayoutNode root, HashSet<Seq<Key>> opened, SKRect window, HashMap<Seq<Key>, LayoutNode> index, HashMap<Seq<Key>, Place> places) =>
        Stacking(pass, Declared(root), opened, window, new Stack([], index, places)).Laid;

    private static Stack Stacking(Layouts.Pass pass, Seq<LayoutNode> declared, HashSet<Seq<Key>> opened, SKRect window, Stack stack) =>
        declared.Fold(stack, (held, layer) =>
            (from node in Optional(layer.Node as Node.Layer)
             where node.Hover.IsNone || opened.Contains(layer.Path)
             from anchor in node.Anchor.Match(
                 Some: path => from target in held.Index.Find(path) from place in held.Places.Find(path) select place.Transform.MapRect(Scene.Absolute(target.Frame, place.Origin)),
                 None: () => Some(window))
             let laid = Laid(pass, layer, node, anchor, window)
             select Stacking(pass, Declared(laid), opened, window, new Stack(held.Laid.Add(laid), held.Index.AddOrUpdateRange(Scene.Indexed(laid).AsIterable()),
                 Scene.Placing(laid, Scene.Root(laid, window), pass.Adjusted, held.Places)))).IfNone(held));

    private static LayoutNode Laid(Layouts.Pass pass, LayoutNode layer, Node.Layer node, SKRect anchor, SKRect window) {
        SKRect content = Placed(anchor, Layouts.Preferred(pass, layer, window.Size), window, node);
        SKRect frame = node.Modal ? window : content;
        Seq<LayoutNode> children = layer.Members.Map(member => Layouts.Framed(pass, member, Veiled(member) ? SKRect.Create(frame.Size) : SKRect.Create(content.Location - frame.Location, content.Size)));
        return Layouts.WithCorners(layer with { Frame = frame, Members = children, Children = children }, None);
    }

    // --- [QUERY]
    internal static HashSet<Seq<Key>> Inert(LayoutNode root, Seq<LayoutNode> layers) =>
        layers.Map(static (layer, index) => (Layer: layer, Index: index)).Filter(static held => held.Layer.Node is Node.Layer { Modal: true }).Last
            .Map(top => toHashSet(root.Cons(layers.Take(top.Index)).Bind(Scene.Preorder).Map(static held => held.Path))).IfNone([]);

    internal static Option<Hosted> Popup(LayoutNode layer, SKRect window) =>
        from content in layer.Children.Find(static child => !Veiled(child))
        let frame = Scene.Absolute(content.Frame, layer.Frame.Location)
        where !window.Contains(frame)
        select (Hosted)new Hosted.Popup(content.Corners) { Path = layer.Path, Frame = frame, Clip = frame };

    private static bool Veiled(LayoutNode member) => member.Path.Last == Some(Veil);
}
