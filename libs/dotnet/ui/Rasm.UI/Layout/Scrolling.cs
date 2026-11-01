using System.Numerics;
using Rasm.UI.Input;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Layout;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Overflow(float Content, float Viewport) {
    internal float Max => float.Max(0f, Content - Viewport);

    internal float Clamped(float offset) => float.Clamp(offset, 0f, Max);
}

internal sealed record Scroller(Seq<Key> Owner, Seq<Key> Stored, PhysicalAxis Axis, float Value, Overflow Overflow, float ThumbRatio, float Step) {
    internal Seq<float> Stops { get; init; }
    internal bool KeepsEnd { get; init; }

    internal float Held(float value) => KeepsEnd && value >= Overflow.Max ? float.PositiveInfinity : value;

    internal Option<float> Navigated(Bearing toward) =>
        toward.Along == Axis
            ? Some(toward.Granularity.Distance(Step, Overflow.Viewport).Match(Some: stride => Snapped(Value + (toward.Sign * stride), toward.Sign), None: () => toward.Sign < 0 ? 0f : Overflow.Max))
            : None;

    internal float Revealing(SKRect viewport, SKRect target) {
        (float Target, float View) low = (Axis.Of(target.Location), Axis.Of(viewport.Location));
        (float Target, float View) high = (Axis.End(target), Axis.End(viewport));
        float delta = low.Target < low.View ? low.Target - low.View : high.Target > high.View ? float.Min(high.Target - high.View, low.Target - low.View) : 0f;
        return Overflow.Clamped(Value + delta) - Value;
    }

    internal bool Scrolls(Vector2 delta) => Overflow.Max > 0f && MathF.Abs(Axis.Of(delta)) >= MathF.Abs(Axis.Cross.Of(delta));

    private float Snapped(float target, int sign) =>
        Stops.Filter(stop => sign * (stop - Value) > 0f) switch {
            var ahead when ahead.IsEmpty => target,
            var ahead => ahead.MinBy(stop => MathF.Abs(stop - target)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Layouts {
    // --- [SCROLL]
    internal static Option<Scroller> Scroller(LayoutNode owner, Option<LayoutNode> thumb, HashMap<Seq<Key>, float> offsets) =>
        from node in Optional(owner.View as View.Scroll)
        from overflow in Panes(owner).Head.Map(content => new Overflow(node.Axis.Of(content.Frame.Size), node.Axis.Of(owner.ContentBox.Size)))
        select new Scroller(owner.Path, Stored(owner, node), node.Axis, Offset(offsets, owner, node, overflow), overflow,
            ThumbRatio: thumb.Map(held => Rescaled(1f, overflow.Viewport - node.Axis.Of(held.Frame.Size), overflow.Max)).IfNone(1f), Step: owner.Style.Metrics[Metric.Spacing]) {
            Stops = node.Snap.Map(snap => toSeq((from pane in Panes(owner)
                                                 from child in pane.Children
                                                 select overflow.Clamped(node.Axis.Of(pane.Frame.Location + child.Frame.Location - owner.ContentBox.Location) + (snap.Of(node.Axis) * (node.Axis.Of(child.Frame.Size) - overflow.Viewport)))).Distinct().Order())).IfNone([]),
            KeepsEnd = node.KeepsEnd,
        };

    internal static bool IsHandle(LayoutNode layout) => layout.Path.Last.Exists(static last => last is Key.Handle);

    internal static Seq<LayoutNode> Panes(LayoutNode owner) => owner.Children.Filter(static child => !IsHandle(child));

    internal static Seq<Key> Stored(LayoutNode scroll, View.Scroll node) => node.Follows.IfNone(scroll.Path);

    internal static float Offset(HashMap<Seq<Key>, float> offsets, LayoutNode scroll, View.Scroll node, Overflow overflow) =>
        overflow.Clamped(offsets.Find(Stored(scroll, node)).IfNone(node.KeepsEnd ? float.PositiveInfinity : 0f));

    internal static float Rescaled(float value, float span, float onto) => span > 0f ? value * onto / span : 0f;

    private static LayoutNode Scrolled(AvailableSpace available, View.Scroll scroll) {
        Seq<LayoutNode> panes = available.Layout.Members.Filter(static member => !IsHandle(member)).Map(content => scroll.Axis == available.Along
            ? Placed(available.Pass, content, available.Along, available.Start, float.Max(Basis(content, available.Along, float.PositiveInfinity), available.Space))
            : Filled(available, content));
        Overflow overflow = new(panes.Map(pane => scroll.Axis.Of(pane.Frame.Size)).Max(0f), scroll.Axis.Of(available.Layout.ContentBox.Size));
        Seq<LayoutNode> thumbs = available.Layout.Members.Filter(IsHandle).Map(thumb => Thumb(available, scroll, thumb, overflow));
        bool shown = available.Along.Map(horizontal: scroll.Axis.Map(horizontal: overflow.Max > 0f, vertical: true), vertical: overflow.Max > 0f);
        return available.Layout with { Members = panes + thumbs, Children = shown ? panes + thumbs : panes };
    }

    private static LayoutNode Thumb(AvailableSpace available, View.Scroll scroll, LayoutNode thumb, Overflow overflow) {
        float least = Basis(thumb, available.Along, float.PositiveInfinity);
        if (scroll.Axis != available.Along) return Placed(available.Pass, thumb, available.Along, available.Start + available.Space - least, least);
        float length = overflow.Max > 0f ? float.Min(overflow.Viewport, float.Max(least, overflow.Viewport * overflow.Viewport / overflow.Content)) : 0f;
        float offset = Offset(available.Pass.Offsets, available.Layout, scroll, overflow);
        return Placed(available.Pass, thumb, available.Along, available.Start + Rescaled(offset, overflow.Max, overflow.Viewport - length), length);
    }
}
