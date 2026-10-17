using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Sparkles;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Geometry.Intersect;

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

    public bool Crosses(RectangleF box) => Switch(new BoundingBox(box.TopLeft.To3d(), box.BottomRight.To3d()),
        straight: static (bounds, span) => {
            using LineCurve curve = new(span.Start.To3d(), span.End.To3d());
            return Enters(curve, bounds);
        },
        curved: static (bounds, span) => {
            if (span.IsEmpty) return bounds.Contains(span.PointAt(span.Start).To3d());
            Plane plane = new(span.Centre.To3d(), Vector3d.XAxis, Vector3d.YAxis);
            _ = plane.Rotate(RhinoMath.ToRadians((double)span.Start + Math.Min(span.Sweep, 0)), Vector3d.ZAxis);
            using ArcCurve curve = new(new Arc(plane, span.Radius, RhinoMath.ToRadians(Math.Abs(span.Sweep))));
            return Enters(curve, bounds);
        });

    public (RouteSegment Head, RouteSegment Tail) Split(float fraction) => Switch<float, (RouteSegment Head, RouteSegment Tail)>(fraction,
        straight: static (at, span) => span.PointAt(at) switch {
            var point => (new LineF(span.Start, point), new LineF(point, span.End)),
        },
        curved: static (at, span) => (span.Sweep * at) switch {
            var sweep => (new ArcF(span.Centre, span.Radius, span.Start, sweep), new ArcF(span.Centre, span.Radius, span.Start + sweep, span.Sweep - sweep)),
        });

    public void Trace(GraphicsPath path) => Switch(path, straight: static (drawing, span) => drawing.AddLine(span), curved: static (drawing, span) => drawing.AddArc(span));

    private static bool Enters(Curve curve, BoundingBox bounds) {
        using NurbsCurve border = new Rectangle3d(Plane.WorldXY, bounds.Min, bounds.Max).ToNurbsCurve();
        using CurveIntersections intersections = Intersection.CurveCurve(curve, border, RhinoMath.ZeroTolerance, RhinoMath.ZeroTolerance);
        return bounds.Contains(curve.PointAtStart) || intersections.Count > 0;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireGeometry {
    public static readonly WireGeometry Linear = new("linear", typeof(LinearWire));
    public static readonly WireGeometry BiArc = new("bi-arc", typeof(BiArcWire));
    public static readonly WireGeometry Orthogonal = new("orthogonal", typeof(OrthogonalWire));

    public Type Shape { get; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class RoutedWire(PointF source, PointF target, IterableNE<RouteSegment> segments) : WireShape(source, target) {
    // --- [GEOMETRY]
    protected const float Stub = 60f;
    protected const float Flat = 3f;

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

public sealed class LinearWire(PointF source, PointF target) : RoutedWire(source, target, Lines(source, target));

public sealed class BiArcWire(PointF source, PointF target) : RoutedWire(source, target, Course(source, target)) {
    private static IterableNE<RouteSegment> Course(PointF source, PointF target) {
        LineF chord = new(source + new SizeF(Stub, 0f), target - new SizeF(Stub, 0f));
        float end = Math.Min(Stub / chord.Length, 0.5f);
        PointF first = chord.PointAt(end);
        PointF last = chord.PointAt(1f - end);
        return Math.Abs(target.Y - source.Y) < Math.Max(Flat, 0.01f * Math.Abs(target.X - source.X)) ? Lines(source, target)
            : IterableNE.create(ArcF.CreateTangent(source, first, chord.Start),
                (end < 0.5f ? Seq<RouteSegment>(new LineF(first, last)) : Seq<RouteSegment>())
                    .Add(ArcF.CreateTangent(target, last, chord.End).Reverse()));
    }
}

public sealed class OrthogonalWire(PointF source, PointF target) : RoutedWire(source, target, Course(source, target)) {
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

internal sealed class CanvasSparkle(
    FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, bool attachedToContent,
    Func<PlotCanvas, double, IO<Unit>> marks, CallbackSite site) : ISparkle {
    private readonly Animated<double> progress = Animators.Unfinished(0.0, 1.0, duration, motion);

    public bool Sparkling { get => field && progress.State is not Grasshopper2.UI.Animation.State.Finished; private set; } = true;

    public void Draw(ControlGraphics graphics) => Sparkling = Callbacks.Answer(
        from at in IO.lift(() => control.Animate(progress))
        from drawn in unless(progress.State is Grasshopper2.UI.Animation.State.Finished,
            CanvasPainting.Plotted(control, graphics, attachedToContent, Skin.Current.Shape.FontWord, plot => marks(plot, at))).As()
        select true,
        static () => false,
        site);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasPainting {
    // --- [PHASES]
    public static IO<Unit> Overlay<TArgs>(HostEvent<TArgs> phase, Func<PlotCanvas, TArgs, IO<Unit>> marks, IPlugInSink sink, Action<IDisposable> retain) where TArgs : CanvasPaintEventArgs =>
        phase.Inline(args => Plotted(args.Canvas, args.Graphics, content: true, args.Skin.Shape.FontWord, plot => marks(plot, args)), sink, retain);

    public static IO<Unit> Overlay(FlexControl control, Func<PlotCanvas, ControlDrawEventArgs, IO<Unit>> marks, IPlugInSink sink, Action<IDisposable> retain) =>
        GrasshopperEvents.FlexControlDraw(control).Inline(args => Plotted(control, args.Graphics, content: false, Skin.Current.Shape.FontWord, plot => marks(plot, args)), sink, retain);

    public static IO<Unit> Background(Canvas canvas, Func<CanvasSkin, CanvasSkin> paper, IPlugInSink sink, Action<IDisposable> retain) =>
        GrasshopperEvents.BeforePaintBackground(canvas).Inline(args =>
            IO.lift(static () => Settings.SandBox).Bind(sandbox => unless(sandbox,
                DisposalOps.Within(args.Graphics.ProjectionState, _ => IO.lift(() => {
                    paper(args.Skin.Canvas).DrawCanvas(args.Graphics.Content, args.Canvas.Projection, args.Canvas.VisibleFrame, 1f, 1f);
                    args.OverrideDefaultPainting();
                }))).As()), sink, retain);

    internal static IO<Unit> Plotted(FlexControl control, ControlGraphics graphics, bool content, Font font, Func<PlotCanvas, IO<Unit>> marks) =>
        DisposalOps.Within(graphics.ProjectionState, _ => PlotCanvas.Of(content ? graphics.Content : graphics.Control,
            Density(graphics.AssociatedControl).IfNone(graphics.ScreenScale) * (content ? graphics.Zoom : 1f),
            font, enabled: true, focused: control.HasFocus).Bind(marks));

    internal static IO<Unit> Plotted(Context context, Font font, bool focused, Func<PlotCanvas, IO<Unit>> marks) =>
        DisposalOps.Within(() => {
            context.Graphics.SaveTransform();
            return context.Graphics;
        }, graphics => PlotCanvas.Of(graphics, Scale(context), font, enabled: true, focused).Bind(marks), static graphics => IO.lift(graphics.RestoreTransform));

    internal static float Scale(Context context) => context.Zoom * Density(context.Control).IfNone(1f);

    internal static Option<float> Density(object? control) =>
        Optional(control as Control).Bind(static host => Optional(host.ParentWindow)).Map(static window => window.LogicalPixelSize);

    // --- [SPARKLES]
    public static IO<Unit> Spark(FlexControl control, ISparkle sparkle) =>
        IO.lift(static () => HostTheme.Accessibility.ReduceMotion).Bind(reduced => unless(reduced, IO.lift(() => control.AddSparkle(sparkle))).As());

    public static IO<Unit> Spark(
        FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, bool attachedToContent,
        Func<PlotCanvas, double, IO<Unit>> marks, IPlugInSink sink) =>
        IO.lift(() => new CanvasSparkle(control, duration, motion, attachedToContent, marks, new CallbackSite(sink, typeof(CanvasSparkle), nameof(ISparkle.Draw))))
            .Bind(sparkle => Spark(control, sparkle));

    // --- [WIRES]
    public static IO<Unit> Route(Type shape, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
        from free in IO.lift(static () => Optional(WireShape.ShapeType).Match(
            Some: static held => Fin.Fail<Unit>(Error.New($"{nameof(WireShape.ShapeType)} already holds {held}")),
            None: static () => Fin.Succ(unit)))
        from lease in DisposalOps.Acquire(IO.lift(() => new Disposal<Type>(shape, static held => {
            if (WireShape.ShapeType == held) WireShape.ShapeType = null;
            _ = Optional(Editor.Instance).Iter(static editor => editor.Canvas.ScheduleRedraw());
        })), held => { WireShape.ShapeType = shape; keep(held); })
        from redrawn in IO.lift(static () => Optional(Editor.Instance).Iter(static editor => editor.Canvas.ScheduleRedraw()))
        select unit, retain);

    public static IO<Unit> Highlight(Canvas canvas, Func<Seq<WireEnds>> wires, MarkColor color, IPlugInSink sink, Action<IDisposable> retain) =>
        Overlay(GrasshopperEvents.BeforePaintWires(canvas), (plot, args) =>
            IO.lift(() => Optional(args.Canvas.Document).ToSeq().Bind(document => wires().Choose(ends => Ends(document, ends))))
                .Bind(found => found.TraverseM(wire => Halo(plot, args.Skin.Wires, color, wire)).As())
                .Map(unit), sink, retain);

    private static Option<(IParameter Source, IParameter Target)> Ends(Document document, WireEnds ends) =>
        from source in Optional(document.Objects.FindParameter(ends.Source))
        from target in Optional(document.Objects.FindParameter(ends.Target))
        select (source, target);

    private static IO<Unit> Halo(PlotCanvas plot, WiresSkin skin, MarkColor color, (IParameter Source, IParameter Target) wire) =>
        from width in IO.lift(() => (float)(skin[WiresSkin.ResolveKind(wire.Source, wire.Target)].Outer.Width + 4f))
        from drawn in DisposalOps.Within(() => new Pen(Plots.Resolve(plot, new MarkStyle.Stroke(color, 1f, width)), width) { LineCap = PenLineCap.Round }, pen => IO.lift(() => WireShape.Create((IParameterAttributes)wire.Source.Attributes, (IParameterAttributes)wire.Target.Attributes).Draw(plot.Graphics, pen)))
        select unit;
}
