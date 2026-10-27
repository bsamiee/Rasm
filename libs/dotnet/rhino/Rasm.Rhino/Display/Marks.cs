using System.Drawing;
using Rhino.ApplicationSettings;
using Rhino.Display;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FeedbackAppearance(Color Curve, Color Construction, Color ActivePoint, int Thickness) {
    public static FeedbackAppearance Of(DisplayPipeline pipeline) =>
        new(AppearanceSettings.FeedbackColor, AppearanceSettings.CrosshairColor, SmartTrackSettings.ActivePointColor, pipeline.DefaultCurveThickness);
}

[Union]
public abstract partial record RetainedMark {
    public sealed record CurveMark(Curve Curve, Color Color, int Thickness) : RetainedMark;
    public sealed record LineMark(Line Line, Color Color, int Thickness) : RetainedMark;
    public sealed record ArcMark(Arc Arc, Color Color, int Thickness) : RetainedMark;
    public sealed record CircleMark(Circle Circle, Color Color, int Thickness) : RetainedMark;
    public sealed record Points(Seq<Point3d> Locations, Color Color, Option<(PointStyle Style, float Radius)> Style) : RetainedMark;
    public sealed record VectorMark(Point3d Point, Vector3d Vector, Color Color, bool DrawPoint) : RetainedMark;
    public sealed record Polygon(Seq<Point3d> Corners, Color Color, bool Filled) : RetainedMark;
    public sealed record Label3d(Text3d Text, Color Color) : RetainedMark;
}

[Union]
public abstract partial record RenderState {
    public sealed record DepthTest(bool Enabled) : RenderState;

    public sealed record DepthWrite(bool Enabled) : RenderState;

    public sealed record CullFace(CullFaceMode Mode) : RenderState;

    public sealed record Model(Transform Xform) : RenderState;

    public sealed record Projection2d() : RenderState;

    public sealed record Depth(DepthMode Mode) : RenderState;

    public sealed record ZBias(ZBiasMode Mode) : RenderState;

    public sealed record ClippingPlane(Point3d Point, Vector3d Normal) : RenderState;

    public sealed record Overlay(Seq<ModeEdit> Edits) : RenderState;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Marks {
    // --- [IMMEDIATE]
    public static IO<Unit> Draw(DisplayPipeline pipeline, Seq<RetainedMark> marks) =>
        IO.lift(() => marks.Iter(mark => Draw(pipeline, mark)));

    private static void Draw(DisplayPipeline pipeline, RetainedMark mark) =>
        mark.Switch(
            pipeline,
            curveMark: static (target, curve) => target.DrawCurve(curve.Curve, curve.Color, curve.Thickness),
            lineMark: static (target, line) => target.DrawLine(line.Line, line.Color, line.Thickness),
            arcMark: static (target, arc) => target.DrawArc(arc.Arc, arc.Color, arc.Thickness),
            circleMark: static (target, circle) => target.DrawCircle(circle.Circle, circle.Color, circle.Thickness),
            points: static (target, points) => points.Style.Match(
                Some: style => target.DrawPoints(points.Locations, style.Style, style.Radius, points.Color),
                None: () => target.DrawPoints(points.Locations, target.DisplayPipelineAttributes.PointStyle, target.DisplayPipelineAttributes.PointRadius, points.Color)),
            vectorMark: static (target, vector) => {
                target.DrawArrow(new Line(vector.Point, vector.Vector), vector.Color);
                if (vector.DrawPoint)
                    target.DrawPoint(vector.Point, vector.Color);
            },
            polygon: static (target, polygon) => target.DrawPolygon(polygon.Corners, polygon.Color, polygon.Filled),
            label3d: static (target, label) => target.Draw3dText(label.Text, label.Color));

    // --- [RETAINED]
    public static IO<Unit> Retain(CustomDisplay display, Seq<RetainedMark> marks) =>
        IO.lift(() => marks.Iter(mark => Retain(display, mark)));

    private static void Retain(CustomDisplay display, RetainedMark mark) =>
        mark.Switch(
            display,
            curveMark: static (target, curve) => target.AddCurve(curve.Curve, curve.Color, curve.Thickness),
            lineMark: static (target, line) => target.AddLine(line.Line, line.Color, line.Thickness),
            arcMark: static (target, arc) => target.AddArc(arc.Arc, arc.Color, arc.Thickness),
            circleMark: static (target, circle) => target.AddCircle(circle.Circle, circle.Color, circle.Thickness),
            points: static (target, points) => points.Style.Match(
                Some: style => target.AddPoints(points.Locations, points.Color, style.Style, int.CreateSaturating(float.Round(style.Radius, MidpointRounding.ToEven))),
                None: () => target.AddPoints(points.Locations, points.Color)),
            vectorMark: static (target, vector) => target.AddVector(vector.Point, vector.Vector, vector.Color, vector.DrawPoint),
            polygon: static (target, polygon) => target.AddPolygon(polygon.Corners, polygon.Color, polygon.Color, polygon.Filled, !polygon.Filled),
            label3d: static (target, label) => target.AddText(label.Text, label.Color));

    // --- [STATE]
    public static IO<TValue> WithState<TValue>(DisplayPipeline pipeline, Seq<RenderState> state, IO<TValue> draw) =>
        state.FoldBack(draw, (inner, render) => render.Switch(
            (Host: pipeline, Body: inner),
            depthTest: static (state, depth) => IO.lift(() => state.Host.PushDepthTesting(depth.Enabled)) >> state.Body.Finally(IO.lift(state.Host.PopDepthTesting)),
            depthWrite: static (state, depth) => IO.lift(() => state.Host.PushDepthWriting(depth.Enabled)) >> state.Body.Finally(IO.lift(state.Host.PopDepthWriting)),
            cullFace: static (state, cull) => IO.lift(() => state.Host.PushCullFaceMode(cull.Mode)) >> state.Body.Finally(IO.lift(state.Host.PopCullFaceMode)),
            model: static (state, model) => IO.lift(() => state.Host.PushModelTransform(model.Xform)) >> state.Body.Finally(IO.lift(state.Host.PopModelTransform)),
            projection2d: static (state, _) => IO.lift(state.Host.Push2dProjection) >> state.Body.Finally(IO.lift(state.Host.PopProjection)),
            depth: static (state, depth) => IO.lift(() => state.Host.DepthMode).Bind(prior =>
                IO.lift(() => { state.Host.DepthMode = depth.Mode; }) >> state.Body.Finally(IO.lift(() => { state.Host.DepthMode = prior; }))),
            zBias: static (state, bias) => IO.lift(() => state.Host.ZBiasMode).Bind(prior =>
                IO.lift(() => { state.Host.ZBiasMode = bias.Mode; }) >> state.Body.Finally(IO.lift(() => { state.Host.ZBiasMode = prior; }))),
            clippingPlane: static (state, plane) => IO.lift(() => state.Host.AddClippingPlane(plane.Point, plane.Normal)).Bind(index =>
                state.Body.Finally(IO.lift(() => state.Host.RemoveClippingPlane(index)))),
            overlay: static (state, overlay) => DisplayModes.Overlaid(state.Host.DisplayPipelineAttributes, overlay.Edits, state.Body)));
}
