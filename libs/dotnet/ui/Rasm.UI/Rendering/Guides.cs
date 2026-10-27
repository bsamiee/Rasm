using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Gutter(Vector2 Start, Vector2 End) {
    internal static Gutter Empty { get; } = new(Vector2.Zero, Vector2.Zero);

    internal static Gutter Of(LayoutNode plot) => Outside(plot.Data, plot.ContentBox);

    internal static Gutter Outside(SKRect data, SKRect reach) =>
        new(Vector2.Max(Vector2.Zero, (Vector2)data.Location - (Vector2)reach.Location), Vector2.Max(Vector2.Zero, new Vector2(reach.Right - data.Right, reach.Bottom - data.Bottom)));

    internal Gutter Max(Gutter other) => new(Vector2.Max(Start, other.Start), Vector2.Max(End, other.End));

    internal Gutter Along(PhysicalAxis axis) => new(axis.With(Vector2.Zero, axis.Of(Start)), axis.With(Vector2.Zero, axis.Of(End)));

    internal SKRect Inside(SKRect box) => SKRect.Create(box.Left + Start.X, box.Top + Start.Y, float.Max(0f, box.Width - Start.X - End.X), float.Max(0f, box.Height - Start.Y - End.Y));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Guides {
    // --- [PLOT]
    private static readonly Key Grids = new Key.Named("grid");
    private static readonly Key Labels = new Key.Named("guides");
    private static readonly Node Container = new Node.Box();

    private sealed record Marked(Guide Guide, float Spacing, Seq<(float Value, LayoutNode Layout)> Labels, Option<LayoutNode> Title) {
        internal static Gutter Reached(Seq<Marked> marks, PlotPlane plane, SKRect data, float gap) =>
            Gutter.Outside(data, marks.Bind(marked => marked.Seats(plane, data, gap)).Fold(data, static (reach, seat) => SKRect.Union(reach, seat.Reach)));

        internal Seq<(LayoutNode Layout, SKRect Frame, SKRect Reach)> Seats(PlotPlane plane, SKRect data, float gap) {
            PlotFrame frame = plane.In(data);
            PhysicalAxis cross = Guide.Along.Cross;
            float outward = frame.Sign(cross) * ((float)Guide.Edge < 0.5f ? -1f : 1f);
            float edge = outward < 0f ? cross.Of(data.Location) : cross.End(data);
            Seq<(LayoutNode Layout, SKRect Frame, SKRect Reach)> labels = Labels.Map(label =>
                Seated(label.Layout, Guide.Angle, Guide.Along.With(cross.With(Vector2.Zero, edge + (outward * gap)), Guide.Along.Of(frame.Point(Guide.Anchor(plane, label.Value)))), cross, outward));
            float depth = labels.Map(seat => outward < 0f ? edge - cross.Of(seat.Reach.Location) : cross.End(seat.Reach) - edge).Fold(gap, float.Max);
            return labels + Title.Map(title => Seated(title, Titled(Guide), Guide.Along.With(cross.With(Vector2.Zero, edge + (outward * (depth + gap))), Guide.Along.Of(new Vector2(data.MidX, data.MidY))), cross, outward)).ToSeq();
        }
    }

    internal static LayoutNode Plot(Layouts.AvailableSpace available, Node.Plot plot, Gutter least) {
        LayoutNode layout = available.Layout;
        SKRect content = layout.ContentBox;
        Seq<LayoutNode> callers = layout.Members.Filter(static member => !Generated(member));
        if (available.Along == PhysicalAxis.Horizontal) {
            Seq<LayoutNode> provisional = Pins(available, content, callers, plot.Plane.In(content).Point, Layouts.Filled);
            return layout with { Data = content, Members = provisional + layout.Members.Filter(Generated), Children = provisional };
        }
        RoleStyle tick = Styles.Resolve(layout.Inner, Role.Tick, layout.Style.Conditions);
        float gap = tick.Metrics[Metric.Gap];
        Seq<LayoutNode> earlier = layout.Members.Filter(Generated).Bind(static held => held.Members);
        SKRect first = least.Inside(content);
        Gutter rough = least.Max(Marked.Reached(Marks(available, plot, first, tick, refit: false, earlier), plot.Plane, first, gap));
        SKRect second = rough.Inside(content);
        Seq<Marked> marks = Marks(available, plot, second, tick, refit: true, earlier);
        SKRect data = rough.Max(Marked.Reached(marks, plot.Plane, second, gap)).Inside(content);
        Seq<LayoutNode> grids = toSeq(from marked in marks
                                      from role in Seq(Role.Grid, Role.GridFine)
                                      select (Node)new Node.Mark(new Figure.Grid(marked.Guide.Along) { Major = role == Role.Grid, Spacing = Some(marked.Spacing) }) { Role = role })
            .Map((node, index) => Built(available, layout.Path.Add(Grids).Add(new Key.Ordinal((uint)index)), node, earlier));
        Seq<LayoutNode> members = Contained(layout, Grids, grids.Map(grid => Layouts.Framed(available.Pass, grid, Local(data, content))))
            + Pins(available, data, callers, plot.Plane.In(data).Point, (room, member) => Layouts.Framed(room.Pass, member, data))
            + Contained(layout, Labels, marks.Bind(marked => marked.Seats(plot.Plane, data, gap)).Map(seat => Layouts.Framed(available.Pass, seat.Layout, Local(seat.Frame, content))));
        return layout with { Data = data, Members = members, Children = members };
    }

    internal static Seq<LayoutNode> Shared(Layouts.AvailableSpace available, Seq<LayoutNode> placed, PhysicalAxis axis, Func<int, int> lane) {
        if (available.Along != PhysicalAxis.Vertical) return placed;
        HashMap<int, Gutter> widest = placed.Map((child, index) => (Child: child, Lane: lane(index))).Filter(static held => held.Child.Node is Node.Plot)
            .Fold(HashMap<int, Gutter>(), (held, plot) => held.AddOrUpdate(plot.Lane, Some: shared => shared.Max(Gutter.Of(plot.Child).Along(axis)), None: () => Gutter.Of(plot.Child).Along(axis)));
        return placed.Map((child, index) => (from plot in Optional(child.Node as Node.Plot)
                                             from shared in widest.Find(lane(index))
                                             where shared != Gutter.Of(child).Along(axis)
                                             select Plot(available with { Layout = child, Start = child.ContentBox.Top, Space = child.ContentBox.Height }, plot, shared)).IfNone(child));
    }

    internal static Seq<LayoutNode> Pins(Layouts.AvailableSpace available, SKRect box, Seq<LayoutNode> members, Func<Vector2, SKPoint> at, Func<Layouts.AvailableSpace, LayoutNode, LayoutNode> unpinned) =>
        members.Map(member => member.Node.Pinned.Match(
            Some: pin => available.Along == PhysicalAxis.Horizontal ? member : Pinned(available, box, member, pin, at),
            None: () => unpinned(available, member)));

    private static LayoutNode Pinned(Layouts.AvailableSpace available, SKRect box, LayoutNode member, Node.Pin pin, Func<Vector2, SKPoint> at) {
        SKPoint point = at(pin.At);
        Option<SKPoint> end = pin.Span.Map(span => at(pin.At + span));
        return toSeq(PhysicalAxis.Items).Fold(member, (held, axis) => end.Match(
            Some: to => Layouts.Placed(available.Pass, held, axis, float.Min(axis.Of(point), axis.Of(to)), MathF.Abs(axis.Of(to) - axis.Of(point))),
            None: () => Layouts.Cross(held, axis, axis.Of(box.Size)) switch {
                var extent => Layouts.Placed(available.Pass, held, axis, Layouts.Sized(held.Node, axis).Stretches ? axis.Of(box.Location) : axis.Of(point) - (pin.Align.Of(axis) * extent), extent),
            }));
    }

    // --- [LABELS]
    private static Seq<Marked> Marks(Layouts.AvailableSpace available, Node.Plot plot, SKRect data, RoleStyle tick, bool refit, Seq<LayoutNode> earlier) =>
        plot.Guides.Fold((Offset: 0u, Marks: Seq<Marked>()), (held, guide) => plot.Plane.Dimension(guide.Along).Match(
            Some: axis => Mark(available, plot, guide, axis, plot.Plane.In(data), tick, refit, held.Offset, earlier) switch {
                var marked => (held.Offset + (uint)marked.Labels.Count + (marked.Title.IsSome ? 1u : 0u), held.Marks.Add(marked)),
            },
            None: () => held)).Marks;

    private static Marked Mark(Layouts.AvailableSpace available, Node.Plot plot, Guide guide, Axis axis, PlotFrame frame, RoleStyle tick, bool refit, uint offset, Seq<LayoutNode> earlier) {
        float pixels = frame.Pixels(guide.Along);
        float spacing = tick.Metrics[Metric.Spacing];
        Seq<(float Value, LayoutNode Layout)> labels = Label(available, guide, axis, spacing / pixels, offset, earlier);
        float widest = labels.Map(label => guide.Along.Of(Turned(label.Layout.Intrinsic, guide.Angle))).Fold(0f, float.Max) + tick.Metrics[Metric.Gap];
        float apart = labels.Map(label => guide.Along.Of(frame.Point(guide.Anchor(plot.Plane, label.Value)))).Window(2).Select(static pair => MathF.Abs(pair[1] - pair[0])).DefaultIfEmpty(float.PositiveInfinity).Min();
        float fitted = refit && widest > apart ? widest : spacing;
        Seq<(float Value, LayoutNode Layout)> kept = fitted > spacing ? Label(available, guide, axis, fitted / pixels, offset, earlier) : labels;
        return new Marked(guide, fitted, kept, guide.Title.Map(title => Built(available, available.Layout.Path.Add(Labels).Add(new Key.Ordinal(offset + (uint)kept.Count)),
            new Node.Text([title]) { Role = Role.Caption, Align = UnitPoint.Center, Transform = Rotation(Titled(guide)) }, earlier)));
    }

    private static Seq<(float Value, LayoutNode Layout)> Label(Layouts.AvailableSpace available, Guide guide, Axis axis, float least, uint offset, Seq<LayoutNode> earlier) =>
        axis.Ticks(least).Filter(static tick => tick.Major).Map((tick, index) => (tick.Value, Built(available, available.Layout.Path.Add(Labels).Add(new Key.Ordinal(offset + (uint)index)),
            new Node.Text([guide.Label(axis, tick.Value, available.Pass.Culture)]) { Role = Role.Tick, Align = UnitPoint.Center, Transform = Rotation(guide.Angle) }, earlier)));

    private static (LayoutNode Layout, SKRect Frame, SKRect Reach) Seated(LayoutNode layout, float degrees, Vector2 near, PhysicalAxis cross, float outward) {
        SKSize turned = Turned(layout.Intrinsic, degrees);
        Vector2 center = cross.With(near, cross.Of(near) + (outward * cross.Of(turned) / 2f));
        return (layout, Centered(center, layout.Intrinsic), Centered(center, turned));
    }

    private static SKSize Turned(SKSize size, float degrees) => SKMatrix.CreateRotationDegrees(degrees).MapRect(SKRect.Create(size)).Size;

    private static SKRect Centered(Vector2 center, SKSize size) => SKRect.Create(center.X - (size.Width / 2f), center.Y - (size.Height / 2f), size.Width, size.Height);

    private static float Titled(Guide guide) => guide.Along.Map(horizontal: 0f, vertical: -90f);

    private static Option<Matrix4x4> Rotation(float degrees) => degrees.CompareTo(0f) is 0 ? None : Some(Matrix4x4.CreateRotationZ(float.DegreesToRadians(degrees)));

    private static LayoutNode Built(Layouts.AvailableSpace available, Seq<Key> path, Node node, Seq<LayoutNode> earlier) =>
        Layouts.Built(available.Pass, available.Layout.Inner, path, node, Layouts.Held(available.Pass, earlier, path));

    private static Seq<LayoutNode> Contained(LayoutNode plot, Key key, Seq<LayoutNode> children) =>
        children.IsEmpty ? [] : [new LayoutNode.Box { Path = plot.Path.Add(key), Node = Container, Cascade = plot.Inner, Inner = plot.Inner, Style = plot.Inner.Enclosing, Frame = plot.ContentBox, Members = children, Children = children }];

    private static SKRect Local(SKRect rect, SKRect content) => SKRect.Create(rect.Location - content.Location, rect.Size);

    private static bool Generated(LayoutNode member) => member.Path.Last.Exists(static last => last == Grids || last == Labels);
}
