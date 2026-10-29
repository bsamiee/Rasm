using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Skinning;
using SuperLinq;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IWireRoute {
    public static abstract Polyline Route(PointF source, PointF target);

    public static virtual float Radius => 0f;

    public static virtual float Opacity(Func<Option<(IParameter Source, IParameter Target)>> ends, float length) => 1f;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RoutedWire<TRoute> : WireShape where TRoute : IWireRoute {
    private readonly Polyline line;
    private readonly float opacity;

    public RoutedWire(PointF source, PointF target) : base(source, target) {
        line = Rounded(TRoute.Route(source, target), TRoute.Radius);
        Memo<Option<(IParameter Source, IParameter Target)>> ends = memo(() =>
            from objects in Optional((Canvas.CurrentlyDrawingCanvas ?? Editor.Instance?.Canvas)?.Document?.Objects)
            from start in Optional(objects.FindByOutlet(source))
            from end in Optional(objects.FindByInlet(target))
            select (start, end));
        opacity = TRoute.Opacity(() => ends.Value, (float)line.Length);
    }

    // --- [GEOMETRY]
    public override RectangleF Bounds => line.BoundingBox switch {
        var box => RectangleF.FromSides((float)box.Min.X, (float)box.Min.Y, (float)box.Max.X, (float)box.Max.Y),
    };

    public override PointF Project(PointF point) => line.ClosestPoint(point.To3d()).ToPointF();

    public override float DistanceTo(PointF point) => opacity > 0f ? Project(point).Distance(point) : float.PositiveInfinity;

    public override bool Intersects(RectangleF box) =>
        opacity > 0f && (box.Contains(Source) || toSeq(line.GetSegments()).Exists(segment => ((LineF)segment).Intersect(box, out _, out _) > 0));

    private static Polyline Rounded(Polyline route, float radius) =>
        toSeq(route).Map(static point => point.ToPointF()).Strict() switch {
            { Count: > 2 } points when radius > 0f => [.. (points.Take(1)
                + toSeq(points.Window(3, corner => Corner(corner[0], corner[1], corner[2], radius))).Flatten()
                + points.Skip(points.Count - 1)).Map(static point => point.To3d())],
            _ => route,
        };

    private static Seq<PointF> Corner(PointF before, PointF at, PointF after, float radius) =>
        float.Min(radius, float.Min(at.Distance(before), at.Distance(after)) / 2f) switch {
            > 0f and var reach => ArcF.CreateTangent(new LineF(at, before).PointAt(reach / at.Distance(before)), new LineF(at, after).PointAt(reach / at.Distance(after)), at) switch {
                { IsUnset: true } => Seq(at),
                var arc => toSeq(arc.Divide(deviation: 0.1)),
            },
            _ => Seq(at),
        };

    // --- [DRAWING]
    public override void Draw(Graphics graphics, Pen edge) {
        if (opacity <= 0f) return;
        using Pen? faded = opacity < 1f && edge.Brush is SolidBrush solid
            ? new(solid.Color.WithA(solid.Color.A * opacity), edge.Thickness) { DashStyle = edge.DashStyle, LineCap = edge.LineCap, LineJoin = edge.LineJoin, MiterLimit = edge.MiterLimit }
            : null;
        double middle = line.ParameterAtLength(line.Length / 2.0);
        Seq<Seq<Point3d>> runs = edge.DashStyle is { IsSolid: false }
            ? Seq(toSeq(line.Trim(0.0, middle)), toSeq(line.Trim(middle, line.Count - 1)).Rev())
            : Seq(toSeq(line));
        _ = runs.Iter(run => graphics.DrawLines(faded ?? edge, run.Map(static point => point.ToPointF())));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class StraightRoute : IWireRoute {
    public static Polyline Route(PointF source, PointF target) => new([source.To3d(), target.To3d()]);
}

public sealed class OrthogonalRoute : IWireRoute {
    private const float Slope = 0.01f;

    public static Polyline Route(PointF source, PointF target) => Route(source, target, stub: 60f, flat: 3f);

    public static Polyline Route(PointF source, PointF target, float stub, float flat) =>
        [.. ((target.X >= source.X, Math.Abs(target.Y - source.Y)) switch {
            (true, var rise) when rise < flat || rise < Slope * (target.X - source.X) => Seq(source, target),
            (true, _) => ((source.X + target.X) / 2f) switch {
                var middle => Seq(source, new PointF(middle, source.Y), new PointF(middle, target.Y), target),
            },
            (false, var rise) => ((rise >= 2f * stub) ? (source.Y + target.Y) / 2f : Math.Max(source.Y, target.Y) + stub) switch {
                var lane => Seq(source, source.Move(stub, 0f), new PointF(source.X + stub, lane), new PointF(target.X - stub, lane), target.Move(-stub, 0f), target),
            },
        }).Map(static point => point.To3d())];
}

public static class Wires {
    // --- [ROUTES]
    private static readonly IO<Unit> Redrawn = IO.lift(static () => Optional(Editor.Instance).Iter(static editor => editor.Canvas.ScheduleRedraw()));

    public static IO<Unit> Route<TRoute>() where TRoute : IWireRoute =>
        from free in IO.lift(static () => GrasshopperFault.Unless(WireShape.ShapeType is null, GrasshopperReason.Taken, WireShape.ShapeType))
        from routed in use(IO.lift(static () => WireShape.ShapeType = typeof(RoutedWire<TRoute>)), static held => IO.lift(() => {
            if (WireShape.ShapeType == held) WireShape.ShapeType = null;
        }) >> Redrawn)
        from redrawn in Redrawn
        select unit;

    // --- [HIGHLIGHTS]
    public static IO<Unit> Highlight(Canvas canvas, Func<Seq<WireEnds>> wires) =>
        Hooks.On<CanvasPaintEventArgs>(h => canvas.BeforePaintWires += h, h => canvas.BeforePaintWires -= h, args =>
            (from state in use(args.Graphics.ProjectionState)
             from drawn in IO.lift(() => Optional(args.Canvas.Document).Iter(document => wires()
                 .Choose(ends =>
                     from source in Optional(document.Objects.FindParameter(ends.Source))
                     from target in Optional(document.Objects.FindParameter(ends.Target))
                     from outlet in Optional(source.Attributes as IParameterAttributes).Filter(static attributes => attributes.HasOutlet)
                     from inlet in Optional(target.Attributes as IParameterAttributes).Filter(static attributes => attributes.HasInlet)
                     select (Kind: WiresSkin.ResolveKind(source, target), Shape: WireShape.Create(outlet, inlet)))
                 .Filter(wire => wire.Shape.Bounds.Intersects(args.Canvas.VisibleFrame))
                 .Iter(wire => {
                     using Pen pen = new(args.Skin.Wires.Glow, args.Skin.Wires[wire.Kind].Outer.Width + 4f);
                     wire.Shape.Draw(args.Graphics.Content, pen);
                 })))
             select unit).Bracket());
}
