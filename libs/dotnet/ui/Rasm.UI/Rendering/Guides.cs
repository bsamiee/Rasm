using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Guides {
    internal static Laid Charted(Layouts.Room room, Node.Plot plot) {
        Seq<Laid> placed = Pins(room, room.Laid.Members, plot.Plane.In(room.Laid.ContentBox).Point, Layouts.Filled);
        return room.Laid with {
            Members = placed,
            Children = room.Along.Switch(
                (Placed: placed, Grounded: plot.Children.Count),
                horizontal: static held => held.Placed,
                vertical: static held => held.Placed.Take(held.Grounded) + held.Placed.Skip(held.Grounded).Fold(Seq<Laid>(), static (kept, label) => kept.Exists(prior => prior.Frame.IntersectsWith(label.Frame)) ? kept : kept.Add(label))),
        };
    }

    internal static Seq<Laid> Pins(Layouts.Room room, Seq<Laid> members, Func<Vector2, SKPoint> at, Func<Layouts.Room, Laid, Laid> unpinned) =>
        members.Map(member => member.Node.Pinned.Match(
            Some: pin => room.Along.Switch((Room: room, Member: member, Pin: pin, At: at), horizontal: static held => held.Member, vertical: static held => Pinned(held.Room, held.Member, held.Pin, held.At)),
            None: () => unpinned(room, member)));

    private static Laid Pinned(Layouts.Room room, Laid member, Node.Pin pin, Func<Vector2, SKPoint> at) {
        SKRect box = room.Laid.ContentBox;
        SKPoint point = at(pin.At);
        Option<SKPoint> end = pin.Span.Map(span => at(pin.At + span));
        return toSeq(PhysicalAxis.Items).Fold(member, (held, axis) => end.Match(
            Some: to => Layouts.Placed(room.Pass, held, axis, float.Min(axis.Of(point), axis.Of(to)), MathF.Abs(axis.Of(to) - axis.Of(point))),
            None: () => {
                float extent = Layouts.Cross(held, axis, axis.Of(box.Size));
                return Layouts.Placed(room.Pass, held, axis, Layouts.Sized(held.Node, axis).Stretches ? axis.Of(box.Location) : axis.Of(point) - (pin.Align.Of(axis) * extent), extent);
            }));
    }
}
