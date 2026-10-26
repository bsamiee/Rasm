using System.Numerics;
using System.Runtime.CompilerServices;
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Skinning;
using Rasm.Rhino;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union<LineF, ArcF>(T1Name = "Straight", T2Name = "Curved", SkipEqualityComparison = true, SkipToString = true)]
public sealed partial class RouteSegment {
    public float Length => Switch(straight: static span => span.Length, curved: static span => span.Length);
    public RectangleF Bounds => Switch(straight: static span => span.Bounds, curved: static span => span.Bounds);
    public RouteSegment Reversed => Switch<RouteSegment>(straight: static span => new LineF(span.End, span.Start), curved: static span => span.Reverse());

    public PointF Closest(PointF point) => Switch(point,
        straight: static (at, span) => span.ClosestPoint(at, finiteSegment: true),
        curved: static (at, span) => span.ClosestPoint(at));

    public bool Crosses(RectangleF box) => Switch(box,
        straight: static (bounds, span) => bounds.Contains(span.Start) || span.Intersect(bounds, out _, out _) > 0,
        curved: static (bounds, span) => bounds.Contains(span.PointAt(span.Start))
            || (!span.IsEmpty && Seq(RectangleEdge.Left, RectangleEdge.Top, RectangleEdge.Right, RectangleEdge.Bottom).Exists(side => Meets(span, bounds.Edge(side, clockwise: true)))));

    public (RouteSegment Head, RouteSegment Tail) Split(float fraction) => Switch<float, (RouteSegment Head, RouteSegment Tail)>(fraction,
        straight: static (at, span) => span.PointAt(at) switch {
            var point => (new LineF(span.Start, point), new LineF(point, span.End)),
        },
        curved: static (at, span) => (span.Sweep * at) switch {
            var sweep => (new ArcF(span.Centre, span.Radius, span.Start, sweep), new ArcF(span.Centre, span.Radius, span.Start + sweep, span.Sweep - sweep)),
        });

    public void Trace(GraphicsPath path) => Switch(path, straight: static (drawing, span) => drawing.AddLine(span), curved: static (drawing, span) => drawing.AddArc(span));

    private static bool Meets(ArcF arc, LineF edge) =>
        (Delta: new Vector2(edge.X1 - edge.X0, edge.Y1 - edge.Y0), Offset: new Vector2(edge.X0 - arc.Centre.X, edge.Y0 - arc.Centre.Y)) switch {
            var (delta, offset) => (A: Vector2.Dot(delta, delta), B: 2f * Vector2.Dot(offset, delta), C: Vector2.Dot(offset, offset) - (arc.Radius * arc.Radius)) switch {
                { A: > 0f } quadratic when (quadratic.B * quadratic.B) - (4f * quadratic.A * quadratic.C) is var discriminant and >= 0f =>
                    Seq(-1f, 1f).Map(sign => (-quadratic.B + (sign * MathF.Sqrt(discriminant))) / (2f * quadratic.A))
                        .Exists(at => at is >= 0f and <= 1f && Swept(arc, edge.PointAt(at))),
                _ => false,
            },
        };

    private static bool Swept(ArcF arc, PointF point) =>
        float.RadiansToDegrees(MathF.Atan2(point.Y - arc.Centre.Y, point.X - arc.Centre.X)) switch {
            var angle => (((float.Sign(arc.Sweep) * (angle - arc.Start) % 360f) + 360f) % 360f) <= MathF.Abs(arc.Sweep),
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class RoutedWire(PointF source, PointF target, IterableNE<RouteSegment> segments) : WireShape(source, target) {
    // --- [GEOMETRY]
    public IterableNE<RouteSegment> Segments { get; } = segments;

    public override RectangleF Bounds => Segments.Tail.Fold(static (box, segment) => RectangleF.Union(box, segment.Bounds), Segments.Head.Bounds);
    public override PointF Project(PointF point) => Segments.Tail.Fold((best, segment) => segment.Closest(point) switch {
        var next => next.Distance(point) < best.Distance(point) ? next : best,
    }, Segments.Head.Closest(point));
    public override float DistanceTo(PointF point) => point.Distance(Project(point));
    public override bool Intersects(RectangleF box) => Segments.Exists(segment => segment.Crosses(box));

    // --- [DRAWING]
    public override void Draw(Graphics graphics, Pen edge) =>
        (edge.DashStyle is { IsSolid: false } ? Halves : Seq(Segments.AsIterable())).Iter(run => {
            using GraphicsPath path = new();
            _ = run.Iter(segment => segment.Trace(path));
            graphics.DrawPath(edge, path);
        });

    private Seq<Iterable<RouteSegment>> Halves =>
        (Segments.Fold(static (sum, segment) => sum + segment.Length, 0f) / 2f) switch {
            var half => Segments.Fold((state, segment) =>
                state.Walked >= half ? (state.Head, segment.Reversed.Cons(state.Tail), state.Walked)
                : state.Walked + segment.Length <= half ? (state.Head.Add(segment), state.Tail, state.Walked + segment.Length)
                : segment.Split((half - state.Walked) / segment.Length) switch {
                    var parts => (state.Head.Add(parts.Head), parts.Tail.Reversed.Cons(state.Tail), state.Walked + segment.Length),
                }, (Head: Seq<RouteSegment>(), Tail: Seq<RouteSegment>(), Walked: 0f)) switch {
                    var split => Seq(split.Head.AsIterable(), split.Tail.AsIterable()),
                },
        };

    // --- [CONSTRUCTION]
    protected static IterableNE<RouteSegment> Lines(PointF a, PointF b, params PointF[] more) =>
        b.Cons(toSeq(more)) switch {
            var corners => IterableNE.create(new LineF(a, b), corners.Zip<PointF, PointF, RouteSegment>(corners.Tail, static (start, end) => new LineF(start, end))),
        };
}

public sealed class OrthogonalWire(PointF source, PointF target) : RoutedWire(source, target, Course(source, target)) {
    private const float Stub = 60f;
    private const float Flat = 3f;

    private static IterableNE<RouteSegment> Course(PointF source, PointF target) {
        float vertical = Math.Abs(target.Y - source.Y);
        return target.X >= source.X
            ? vertical < Flat ? Lines(source, target) : ((source.X + target.X) / 2f) switch {
                var x => Lines(source, new PointF(x, source.Y), new PointF(x, target.Y), target),
            }
            : (vertical >= 2f * Stub ? (source.Y + target.Y) / 2f : Math.Max(source.Y, target.Y) + Stub) switch {
                var lane => Lines(source, source + new SizeF(Stub, 0f), new PointF(source.X + Stub, lane), new PointF(target.X - Stub, lane), target - new SizeF(Stub, 0f), target),
            };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasWires {
    // --- [ROUTES]
    internal static readonly IO<Unit> Redrawn = IO.lift(static () => Optional(Editor.Instance).Iter(static editor => editor.Canvas.ScheduleRedraw()));

    private const string LinearShape = "Grasshopper2.UI.Canvas.WireShapeLinear, Grasshopper2";
    private const string BiArcShape = "Grasshopper2.UI.Canvas.WireShapeBiArc, Grasshopper2";

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(LinearShape)]
    private static extern object WireShapeLinear(PointF source, PointF target);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(BiArcShape)]
    private static extern object WireShapeBiArc(PointF source, PointF target);

    public static Type Linear => WireShapeLinear(PointF.Empty, PointF.Empty).GetType();

    public static Type BiArc => WireShapeBiArc(PointF.Empty, PointF.Empty).GetType();

    public static IO<Unit> Route(Type shape, Action<IDisposable> retain) =>
        from free in IO.lift(static Fin<Unit> () => WireShape.ShapeType is { } held ? Error.New($"{nameof(WireShape.ShapeType)} already holds {held}") : unit)
        from routed in DisposalOps.Acquire(IO.lift(() => WireShape.ShapeType = shape),
            static held => IO.lift(() => {
                if (WireShape.ShapeType == held) WireShape.ShapeType = null;
            }).Bind(static _ => Redrawn), retain)
        from redrawn in Redrawn
        select unit;

    // --- [HIGHLIGHTS]
    public static IO<Unit> Highlight(Canvas canvas, Func<Seq<WireEnds>> wires, Color color, float reach, IPlugInSink sink, Action<IDisposable> retain) =>
        GrasshopperEvents.BeforePaintWires(canvas).Inline(args =>
            DisposalOps.Within(args.Graphics.ProjectionState, _ =>
                IO.lift(() => Optional(args.Canvas.Document).ToSeq().Bind(document => wires().Choose(ends => Ends(document, ends))))
                    .Bind(found => found.TraverseM(wire => Halo(args.Graphics.Content, args.Skin.Wires, color, reach, wire)).As())
                    .Map(unit)), sink, retain);

    private static Option<(IParameter Source, IParameter Target)> Ends(Document document, WireEnds ends) =>
        from source in Optional(document.Objects.FindParameter(ends.Source))
        from target in Optional(document.Objects.FindParameter(ends.Target))
        select (source, target);

    private static IO<Unit> Halo(Graphics graphics, WiresSkin skin, Color color, float reach, (IParameter Source, IParameter Target) wire) =>
        DisposalOps.Within(
            () => new Pen(color, (float)skin[WiresSkin.ResolveKind(wire.Source, wire.Target)].Outer.Width + reach) { LineCap = PenLineCap.Round },
            pen => IO.lift(() => WireShape.Create((IParameterAttributes)wire.Source.Attributes, (IParameterAttributes)wire.Target.Attributes).Draw(graphics, pen)));
}
