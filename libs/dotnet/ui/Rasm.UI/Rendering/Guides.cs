using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Guides {
    internal static LayoutNode Plot(Layouts.AvailableSpace available, Node.Plot plot) {
        Seq<LayoutNode> placed = Pins(available, available.Layout.Members, plot.Plane.In(available.Layout.ContentBox).Point, Layouts.Filled);
        return available.Layout with {
            Members = placed,
            Children = available.Along.Switch(
                (Placed: placed, Children: plot.Children.Count),
                horizontal: static held => held.Placed,
                vertical: static held => held.Placed.Take(held.Children) + held.Placed.Skip(held.Children).Fold(Seq<LayoutNode>(), static (kept, label) => kept.Exists(prior => prior.Frame.IntersectsWith(label.Frame)) ? kept : kept.Add(label))),
        };
    }

    internal static Seq<LayoutNode> Pins(Layouts.AvailableSpace available, Seq<LayoutNode> members, Func<Vector2, SKPoint> at, Func<Layouts.AvailableSpace, LayoutNode, LayoutNode> unpinned) =>
        members.Map(member => member.Node.Pinned.Match(
            Some: pin => available.Along.Switch((Room: available, Member: member, Pin: pin, At: at), horizontal: static held => held.Member, vertical: static held => Pinned(held.Room, held.Member, held.Pin, held.At)),
            None: () => unpinned(available, member)));

    private static LayoutNode Pinned(Layouts.AvailableSpace available, LayoutNode member, Node.Pin pin, Func<Vector2, SKPoint> at) {
        SKRect box = available.Layout.ContentBox;
        SKPoint point = at(pin.At);
        Option<SKPoint> end = pin.Span.Map(span => at(pin.At + span));
        return toSeq(PhysicalAxis.Items).Fold(member, (held, axis) => end.Match(
            Some: to => Layouts.Placed(available.Pass, held, axis, float.Min(axis.Of(point), axis.Of(to)), MathF.Abs(axis.Of(to) - axis.Of(point))),
            None: () => {
                float extent = Layouts.Cross(held, axis, axis.Of(box.Size));
                return Layouts.Placed(available.Pass, held, axis, Layouts.Sized(held.Node, axis).Stretches ? axis.Of(box.Location) : axis.Of(point) - (pin.Align.Of(axis) * extent), extent);
            }));
    }
}
