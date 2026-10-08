using System.Drawing;
using Rasm.Imaging.Pixels;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display.Feedback;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class InkRole {
    public static readonly InkRole Feedback = new(static () => AppearanceSettings.FeedbackColor);
    public static readonly InkRole Crosshair = new(static () => AppearanceSettings.CrosshairColor);
    public static readonly InkRole Tracking = new(static () => AppearanceSettings.TrackingColor);
    public static readonly InkRole ActivePoint = new(static () => SmartTrackSettings.ActivePointColor);

    [UseDelegateFromConstructor]
    public partial Color Read();
}

[Union<InkRole, Color>(T1Name = "Role", T2Name = "Literal")]
public sealed partial class Ink {
    public Color Drawn => Switch(role: static role => role.Read(), literal: static color => color);
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class Ring {
    public Seq<Point3d> Corners { get; }

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Seq<Point3d> corners) =>
        validationError = corners.Count >= 3 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record RetainedMark {
    public sealed record CurveMark(Curve Curve, Ink Ink, Option<int> Thickness) : RetainedMark;
    public sealed record LineMark(Line Line, Ink Ink, Option<int> Thickness) : RetainedMark;
    public sealed record ArcMark(Arc Arc, Ink Ink, Option<int> Thickness) : RetainedMark;
    public sealed record CircleMark(Circle Circle, Ink Ink, Option<int> Thickness) : RetainedMark;
    public sealed record Points(Seq<Point3d> Locations, Ink Ink, Option<(PointStyle Style, int Radius)> Style) : RetainedMark;
    public sealed record VectorMark(Point3d Point, Vector3d Vector, Ink Ink, bool DrawPoint) : RetainedMark;
    public sealed record Polygon(Ring Ring, Ink Ink, bool Filled) : RetainedMark;
    public sealed record Label3d(Text3d Text, Ink Ink) : RetainedMark;
}

[Union<Box, Sphere, Torus, Cylinder, Cone>]
public sealed partial class PrimitiveSolid;

public sealed record GradientFill(Seq<ColorStop> Stops, Point3d Start, Point3d End, bool Linear, float Repeat);

[Union<double, double>(T1Name = "Screen", T2Name = "World", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ArrowHeadSize;

[Union<double, Transform>(T1Name = "Scaled", T2Name = "Transformed", MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class TextPlacement;

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AnalysisPreview {
    public static readonly AnalysisPreview Zebra = new(static (pipeline, mesh, color) => pipeline.DrawZebraPreview(mesh, color), static (pipeline, brep, color) => pipeline.DrawZebraPreview(brep, color));
    public static readonly AnalysisPreview Emap = new(static (pipeline, mesh, color) => pipeline.DrawEmapPreview(mesh, color), static (pipeline, brep, color) => pipeline.DrawEmapPreview(brep, color));
    public static readonly AnalysisPreview Curvature = new(static (pipeline, mesh, color) => pipeline.DrawCurvaturePreview(mesh, color), static (pipeline, brep, color) => pipeline.DrawCurvaturePreview(brep, color));
    public static readonly AnalysisPreview DraftAngle = new(static (pipeline, mesh, color) => pipeline.DrawDraftAnglePreview(mesh, color), static (pipeline, brep, color) => pipeline.DrawDraftAnglePreview(brep, color));

    [UseDelegateFromConstructor]
    public partial void DrawMesh(DisplayPipeline pipeline, Mesh mesh, Color color);

    [UseDelegateFromConstructor]
    public partial void DrawBrep(DisplayPipeline pipeline, Brep brep, Color color);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct BandCount : System.Numerics.IMinMaxValue<BandCount> {
    public static BandCount MinValue { get; } = new(1);
    public static BandCount MaxValue { get; } = new(10);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record BandColors {
    private BandColors(Seq<Color> colors) => Colors = colors;

    public Seq<Color> Colors { get; }

    public static BandColors Sampled(RampTable ramp, BandCount count) =>
        count.ToValue() switch {
            var bands => new(toSeq(Range(0, bands)).Map(index => FalseColorScale.Displayed(ramp.Sample(count == BandCount.MinValue ? 0.5 : index / (double)(bands - 1)))).Strict()),
        };
}

public sealed record IsoBanding(IsoDrawMode Mode, Vector3d Direction, Point3d Point, int Frequency, double RotationRadians, double Falloff, double GapSize, Option<Color> Gap, BandColors Bands);

[Union]
public abstract partial record MeshLook {
    public sealed record Shaded(ShadedMaterial Material, Option<Seq<int>> Faces) : MeshLook;
    public sealed record Banded(Ink Diffuse, Option<IsoBanding> Banding) : MeshLook;
    public sealed record FalseColors : MeshLook;
    public sealed record Gradient(GradientFill Fill) : MeshLook;
    public sealed record Wires(Ink Ink, Option<int> Thickness) : MeshLook;
    public sealed record Vertices(Ink Ink) : MeshLook;
    public sealed record Preview(AnalysisPreview Analysis, Ink Ink) : MeshLook;
}

[Union]
public abstract partial record BrepLook {
    public sealed record Shaded(ShadedMaterial Material) : BrepLook;
    public sealed record Wires(Ink Ink, Option<int> Density) : BrepLook;
    public sealed record Preview(AnalysisPreview Analysis, Ink Ink) : BrepLook;
}

[Union]
public abstract partial record SubDLook {
    public sealed record Shaded(ShadedMaterial Material) : SubDLook;
    public sealed record Wires(Ink Ink, float Thickness) : SubDLook;
    public sealed record Pens(Option<Stroke> Boundary, Option<Stroke> SmoothInterior, Option<Stroke> Crease, Option<Stroke> Nonmanifold) : SubDLook;
}

[Union]
public abstract partial record WorldMark {
    public sealed record Retained(RetainedMark Mark) : WorldMark;
    public sealed record StrokedCurve(Curve Curve, Stroke Stroke) : WorldMark;
    public sealed record StrokedLines(Seq<Line> Segments, Stroke Stroke) : WorldMark;
    public sealed record Lines(Seq<Line> Segments, Ink Ink, Option<int> Thickness) : WorldMark;
    public sealed record LinesNoClip(Seq<Line> Segments, Ink Ink, int Thickness) : WorldMark;
    public sealed record GradientLines(Seq<Line> Segments, float Width, GradientFill Fill) : WorldMark;
    public sealed record Arrows(Seq<Line> Shafts, Ink Ink) : WorldMark;
    public sealed record LineArrow(Line Line, Ink Ink, int Thickness, double TipSize) : WorldMark;
    public sealed record ArrowHead(Point3d Tip, Vector3d Direction, Ink Ink, ArrowHeadSize Size) : WorldMark;
    public sealed record DirectionArrow(Point3d At, Vector3d Direction, Ink Ink) : WorldMark;
    public sealed record Marker(Point3d Tip, Vector3d Direction, Ink Ink, Option<(int Thickness, double Size, double Rotation)> Shape) : WorldMark;
    public sealed record InferenceLine(Point3d From, Point3d Through, Ink Ink, DisplayPipeline.InferenceLineType Kind) : WorldMark;
    public sealed record InferencePoint(Point3d At, Ink Ink) : WorldMark;
    public sealed record ActivePoint(Point3d At) : WorldMark;
    public sealed record PointSet(DisplayPointSet Set, Option<(DisplayPointAttributes Fallback, DisplayPointAttributes Override)> Attributes) : WorldMark;
    public sealed record PointCloudMark(PointCloud Cloud, float Size, Option<Ink> Ink) : WorldMark;
    public sealed record Wires(PrimitiveSolid Solid, Ink Ink, Option<int> Thickness) : WorldMark;
    public sealed record BoxCorners(BoundingBox Box, Ink Ink, Option<(double Size, int Thickness)> Shape) : WorldMark;
    public sealed record CurvatureGraph(Curve Curve, Ink Ink, Option<(int HairScale, int HairDensity, int SampleDensity)> Sampling) : WorldMark;
    public sealed record MeshMark(Mesh Mesh, MeshLook Look) : WorldMark;
    public sealed record BrepMark(Brep Brep, BrepLook Look) : WorldMark;
    public sealed record SubDMark(SubD SubD, SubDLook Look) : WorldMark;
    public sealed record ExtrusionWires(Extrusion Extrusion, Ink Ink, Option<int> Density) : WorldMark;
    public sealed record SurfaceMark(Surface Surface, Ink Ink, int Density) : WorldMark;
    public sealed record Block(InstanceDefinition Definition, Option<Transform> Xform, Option<ShadedMaterial> Material) : WorldMark;
    public sealed record ObjectMark(RhinoObject Object, Option<Transform> Xform) : WorldMark;
    public sealed record Clipping(ClippingPlaneSurface Plane, Ink Ink) : WorldMark;
    public sealed record LightMark(Light Light, Ink Wireframe) : WorldMark;
    public sealed record ConstructionPlaneMark(ConstructionPlane Plane) : WorldMark;
    public sealed record HatchMark(Hatch Hatch, Ink Fill, Ink Boundary) : WorldMark;
    public sealed record StrokedHatch(Hatch Hatch, Ink Fill, Option<Stroke> Boundary, Ink Background) : WorldMark;
    public sealed record GradientHatch(Hatch Hatch, GradientFill Fill, Option<Stroke> Boundary, Ink Background) : WorldMark;
    public sealed record TextMark(TextEntity Text, Ink Ink, Option<TextPlacement> Placement) : WorldMark;
    public sealed record AnnotationMark(AnnotationBase Annotation, Option<RhinoObject> Parent, Ink Ink) : WorldMark;
    public sealed record AnnotationArrowhead(Arrowhead Arrowhead, Transform Xform, Ink Ink) : WorldMark;
    public sealed record DirectionIndicators(SurfaceDirectionIndicators Indicators) : WorldMark;
}

public sealed record BlendFunction(BlendMode Source, BlendMode Destination);

[Union]
public abstract partial record SpriteAnchor {
    public sealed record Screen(Point2d At, float Width, float Height) : SpriteAnchor;
    public sealed record ScreenSized(Point2d At, float Size, Option<Ink> Tint) : SpriteAnchor;
    public sealed record World(Point3d At, float Size, Option<Ink> Tint, bool SizeInWorldSpace) : SpriteAnchor;
    public sealed record Cloud(Seq<(Point3d At, Ink Tint)> Points, float Size, Option<Vector3d> Translation, bool SizeInWorldSpace) : SpriteAnchor;
    public sealed record Particles(ParticleSystem System) : SpriteAnchor;
}

public sealed record SpriteMark(Bitmap Source, Option<BlendFunction> Blend, SpriteAnchor Anchor);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SpriteCache(IPlugInSink sink) : IDisposable {
    private AtomHashMap<(Bitmap Source, Option<BlendFunction> Blend), DisplayBitmap>? bitmaps =
        AtomHashMap<(Bitmap Source, Option<BlendFunction> Blend), DisplayBitmap>();

    internal IO<Option<DisplayBitmap>> Get(Bitmap source, Option<BlendFunction> blend) =>
        IO.lift(() => bitmaps is { } held
            ? Some(held.FindOrAdd((source, blend), () => {
                DisplayBitmap bitmap = new(source);
                if (blend.Case is BlendFunction selected)
                    bitmap.SetBlendFunction(selected.Source, selected.Destination);
                return bitmap;
            }))
            : None);

    public void Dispose() =>
        Optional(Interlocked.Exchange(ref bitmaps, value: null)).Iter(held => Callbacks.Answer(
            DisposalOps.Release(held.Values.ToSeq()), static () => unit, new CallbackSite(sink, typeof(SpriteCache), nameof(Dispose))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class BandingMapper {
    [MapProperty(nameof(IsoBanding.Mode), nameof(IsoDrawEffect.DrawMode))]
    [MapProperty(nameof(@IsoBanding.Bands.Colors.Count), nameof(IsoDrawEffect.UsedBandColorCount))]
    [MapProperty(nameof(IsoBanding.Gap), nameof(IsoDrawEffect.GapColor))]
    [MapProperty(nameof(@IsoBanding.Gap.IsNone), nameof(IsoDrawEffect.DiscardGap))]
    internal static partial void Update(IsoBanding state, IsoDrawEffect host);
}

public static class Marks {
    // --- [PIPELINE]
    public static IO<Unit> Draw(DisplayPipeline pipeline, Seq<WorldMark> marks) =>
        marks.TraverseM(mark => Draw(pipeline, mark)).As().Map(static _ => unit);

    private static IO<Unit> Draw(DisplayPipeline pipeline, WorldMark mark) =>
        mark.Switch(
            pipeline,
            retained: static (target, retained) => IO.lift(() => Draw(target, retained.Mark)),
            strokedCurve: static (target, stroked) => IO.lift(() => target.DrawCurve(stroked.Curve, Strokes.Pen(stroked.Stroke))),
            strokedLines: static (target, stroked) => IO.lift(() => target.DrawLines([.. stroked.Segments], Strokes.Pen(stroked.Stroke))),
            lines: static (target, lines) => IO.lift(() => lines.Thickness.Match(
                Some: thickness => target.DrawLines(lines.Segments, lines.Ink.Drawn, thickness),
                None: () => target.DrawLines(lines.Segments, lines.Ink.Drawn))),
            linesNoClip: static (target, lines) => IO.lift(() => target.DrawLinesNoClip(lines.Segments, lines.Ink.Drawn, lines.Thickness)),
            gradientLines: static (target, lines) => IO.lift(() => target.DrawGradientLines(lines.Segments, lines.Width, lines.Fill.Stops, lines.Fill.Start, lines.Fill.End, lines.Fill.Linear, lines.Fill.Repeat)),
            arrows: static (target, arrows) => IO.lift(() => target.DrawArrows(arrows.Shafts, arrows.Ink.Drawn)),
            lineArrow: static (target, arrow) => IO.lift(() => target.DrawLineArrow(arrow.Line, arrow.Ink.Drawn, arrow.Thickness, arrow.TipSize)),
            arrowHead: static (target, head) => IO.lift(() => head.Size.Switch(
                (Pipeline: target, Head: head),
                screen: static (state, pixels) => state.Pipeline.DrawArrowHead(state.Head.Tip, state.Head.Direction, state.Head.Ink.Drawn, pixels, 0),
                world: static (state, units) => state.Pipeline.DrawArrowHead(state.Head.Tip, state.Head.Direction, state.Head.Ink.Drawn, 0, units))),
            directionArrow: static (target, arrow) => IO.lift(() => target.DrawDirectionArrow(arrow.At, arrow.Direction, arrow.Ink.Drawn)),
            marker: static (target, marker) => IO.lift(() => marker.Shape.Match(
                Some: shape => target.DrawMarker(marker.Tip, marker.Direction, marker.Ink.Drawn, shape.Thickness, shape.Size, shape.Rotation),
                None: () => target.DrawMarker(marker.Tip, marker.Direction, marker.Ink.Drawn))),
            inferenceLine: static (target, line) => IO.lift(() => target.DrawInferenceLine(line.From, line.Through, line.Ink.Drawn, line.Kind)),
            inferencePoint: static (target, point) => IO.lift(() => target.DraweInferencePoint(point.At, point.Ink.Drawn)),
            activePoint: static (target, point) => IO.lift(() => target.DrawActivePoint(point.At)),
            pointSet: static (target, points) => IO.lift(() => points.Attributes.Match(
                Some: attributes => target.DrawPoints(points.Set, attributes.Fallback, attributes.Override),
                None: () => target.DrawPoints(points.Set))),
            pointCloudMark: static (target, cloud) => IO.lift(() => cloud.Ink.Match(
                Some: ink => target.DrawPointCloud(cloud.Cloud, cloud.Size, ink.Drawn),
                None: () => target.DrawPointCloud(cloud.Cloud, cloud.Size))),
            wires: static (target, wires) => wires.Solid.Switch(
                (Pipeline: target, Wires: wires),
                box: static (state, box) => state.Wires.Thickness.Case is int thickness
                    ? IO.lift(() => state.Pipeline.DrawBox(box, state.Wires.Ink.Drawn, thickness))
                    : IO.lift(() => state.Pipeline.DrawBox(box, state.Wires.Ink.Drawn)),
                sphere: static (state, sphere) => state.Wires.Thickness.Case is int thickness
                    ? IO.lift(() => state.Pipeline.DrawSphere(sphere, state.Wires.Ink.Drawn, thickness))
                    : IO.lift(() => state.Pipeline.DrawSphere(sphere, state.Wires.Ink.Drawn)),
                torus: static (state, torus) => state.Wires.Thickness.Case is int thickness
                    ? IO.lift(() => state.Pipeline.DrawTorus(torus, state.Wires.Ink.Drawn, thickness))
                    : IO.lift(() => state.Pipeline.DrawTorus(torus, state.Wires.Ink.Drawn)),
                cylinder: static (state, cylinder) => state.Wires.Thickness.Case is int thickness
                    ? IO.lift(() => state.Pipeline.DrawCylinder(cylinder, state.Wires.Ink.Drawn, thickness))
                    : IO.lift(() => state.Pipeline.DrawCylinder(cylinder, state.Wires.Ink.Drawn)),
                cone: static (state, cone) => state.Wires.Thickness.Case is int thickness
                    ? IO.lift(() => state.Pipeline.DrawCone(cone, state.Wires.Ink.Drawn, thickness))
                    : IO.lift(() => state.Pipeline.DrawCone(cone, state.Wires.Ink.Drawn))),
            boxCorners: static (target, corners) => IO.lift(() => corners.Shape.Match(
                Some: shape => target.DrawBoxCorners(corners.Box, corners.Ink.Drawn, shape.Size, shape.Thickness),
                None: () => target.DrawBoxCorners(corners.Box, corners.Ink.Drawn))),
            curvatureGraph: static (target, graph) => IO.lift(() => graph.Sampling.Match(
                Some: sampling => target.DrawCurvatureGraph(graph.Curve, graph.Ink.Drawn, sampling.HairScale, sampling.HairDensity, sampling.SampleDensity),
                None: () => target.DrawCurvatureGraph(graph.Curve, graph.Ink.Drawn))),
            meshMark: static (target, mesh) => mesh.Look.Switch(
                (Pipeline: target, mesh.Mesh),
                shaded: static (state, shaded) => shaded.Faces.Case is Seq<int> faces
                    ? Strokes.Use(shaded.Material, material => state.Pipeline.DrawMeshShaded(state.Mesh, material, faces))
                    : Strokes.Use(shaded.Material, material => state.Pipeline.DrawMeshShaded(state.Mesh, material)),
                banded: static (state, banded) => IO.lift(() => state.Pipeline.DrawMeshShaded(state.Mesh, banded.Diffuse.Drawn, banded.Banding.Map(Effect).ValueUnsafe())),
                falseColors: static (state, _) => IO.lift(() => state.Pipeline.DrawMeshFalseColors(state.Mesh)),
                gradient: static (state, gradient) => IO.lift(() => state.Pipeline.DrawGradientMesh(state.Mesh, gradient.Fill.Stops, gradient.Fill.Start, gradient.Fill.End, gradient.Fill.Linear, gradient.Fill.Repeat)),
                wires: static (state, wires) => wires.Thickness.Case is int thickness
                    ? IO.lift(() => state.Pipeline.DrawMeshWires(state.Mesh, wires.Ink.Drawn, thickness))
                    : IO.lift(() => state.Pipeline.DrawMeshWires(state.Mesh, wires.Ink.Drawn)),
                vertices: static (state, vertices) => IO.lift(() => state.Pipeline.DrawMeshVertices(state.Mesh, vertices.Ink.Drawn)),
                preview: static (state, preview) => IO.lift(() => preview.Analysis.DrawMesh(state.Pipeline, state.Mesh, preview.Ink.Drawn))),
            brepMark: static (target, brep) => brep.Look.Switch(
                (Pipeline: target, brep.Brep),
                shaded: static (state, shaded) => Strokes.Use(shaded.Material, material => state.Pipeline.DrawBrepShaded(state.Brep, material)),
                wires: static (state, wires) => wires.Density.Case is int density
                    ? IO.lift(() => state.Pipeline.DrawBrepWires(state.Brep, wires.Ink.Drawn, density))
                    : IO.lift(() => state.Pipeline.DrawBrepWires(state.Brep, wires.Ink.Drawn)),
                preview: static (state, preview) => IO.lift(() => preview.Analysis.DrawBrep(state.Pipeline, state.Brep, preview.Ink.Drawn))),
            subDMark: static (target, subd) => subd.Look.Switch(
                (Pipeline: target, subd.SubD),
                shaded: static (state, shaded) => Strokes.Use(shaded.Material, material => state.Pipeline.DrawSubDShaded(state.SubD, material)),
                wires: static (state, wires) => IO.lift(() => state.Pipeline.DrawSubDWires(state.SubD, wires.Ink.Drawn, wires.Thickness)),
                pens: static (state, pens) => IO.lift(() => state.Pipeline.DrawSubDWires(
                    state.SubD,
                    pens.Boundary.Map(Strokes.Pen).ValueUnsafe(),
                    pens.SmoothInterior.Map(Strokes.Pen).ValueUnsafe(),
                    pens.Crease.Map(Strokes.Pen).ValueUnsafe(),
                    pens.Nonmanifold.Map(Strokes.Pen).ValueUnsafe()))),
            extrusionWires: static (target, wires) => IO.lift(() => wires.Density.Match(
                Some: density => target.DrawExtrusionWires(wires.Extrusion, wires.Ink.Drawn, density),
                None: () => target.DrawExtrusionWires(wires.Extrusion, wires.Ink.Drawn))),
            surfaceMark: static (target, surface) => IO.lift(() => target.DrawSurface(surface.Surface, surface.Ink.Drawn, surface.Density)),
            block: static (target, block) => block.Material.Case is ShadedMaterial material
                ? Strokes.Use(material, shaded => block.Xform.Match(
                    Some: xform => target.DrawInstanceDefinitionShaded(block.Definition, shaded, xform),
                    None: () => target.DrawInstanceDefinitionShaded(block.Definition, shaded)))
                : IO.lift(() => block.Xform.Match(
                    Some: xform => target.DrawInstanceDefinition(block.Definition, xform),
                    None: () => target.DrawInstanceDefinition(block.Definition))),
            objectMark: static (target, drawn) => IO.lift(() => drawn.Xform.Match(
                Some: xform => target.DrawObject(drawn.Object, xform),
                None: () => target.DrawObject(drawn.Object))),
            clipping: static (target, clipping) => IO.lift(() => target.DrawClippingPlaneWires(clipping.Plane, clipping.Ink.Drawn)),
            lightMark: static (target, light) => IO.lift(() => target.DrawLight(light.Light, light.Wireframe.Drawn)),
            constructionPlaneMark: static (target, plane) => IO.lift(() => target.DrawConstructionPlane(plane.Plane)),
            hatchMark: static (target, hatch) => IO.lift(() => target.DrawHatch(hatch.Hatch, hatch.Fill.Drawn, hatch.Boundary.Drawn)),
            strokedHatch: static (target, hatch) => IO.lift(() => target.DrawHatch(hatch.Hatch, hatch.Fill.Drawn, hatch.Boundary.Map(Strokes.Pen).ValueUnsafe(), hatch.Background.Drawn)),
            gradientHatch: static (target, hatch) => IO.lift(() => target.DrawGradientHatch(
                hatch.Hatch, hatch.Fill.Stops, hatch.Fill.Start, hatch.Fill.End, hatch.Fill.Linear, hatch.Fill.Repeat, hatch.Boundary.Map(Strokes.Pen).ValueUnsafe(), hatch.Background.Drawn)),
            textMark: static (target, text) => text.Placement.Case is TextPlacement placement
                ? placement.Switch(
                    (Pipeline: target, Text: text),
                    scaled: static (state, scale) => IO.lift(() => state.Pipeline.DrawText(state.Text.Text, state.Text.Ink.Drawn, scale)),
                    transformed: static (state, xform) => IO.lift(() => state.Pipeline.DrawText(state.Text.Text, state.Text.Ink.Drawn, xform)))
                : IO.lift(() => target.DrawText(text.Text, text.Ink.Drawn)),
            annotationMark: static (target, annotation) => IO.lift(() => annotation.Parent.Match(
                Some: parent => target.DrawAnnotation(annotation.Annotation, parent, annotation.Ink.Drawn),
                None: () => target.DrawAnnotation(annotation.Annotation, annotation.Ink.Drawn))),
            annotationArrowhead: static (target, arrowhead) => IO.lift(() => target.DrawAnnotationArrowhead(arrowhead.Arrowhead, arrowhead.Xform, arrowhead.Ink.Drawn)),
            directionIndicators: static (target, direction) => IO.lift(() => target.DrawSurfaceDirectionIndicators(direction.Indicators)));

    private static void Draw(DisplayPipeline pipeline, RetainedMark mark) =>
        mark.Switch(
            pipeline,
            curveMark: static (target, curve) => curve.Thickness.Match(
                Some: thickness => target.DrawCurve(curve.Curve, curve.Ink.Drawn, thickness),
                None: () => target.DrawCurve(curve.Curve, curve.Ink.Drawn)),
            lineMark: static (target, line) => line.Thickness.Match(
                Some: thickness => target.DrawLine(line.Line, line.Ink.Drawn, thickness),
                None: () => target.DrawLine(line.Line, line.Ink.Drawn)),
            arcMark: static (target, arc) => arc.Thickness.Match(
                Some: thickness => target.DrawArc(arc.Arc, arc.Ink.Drawn, thickness),
                None: () => target.DrawArc(arc.Arc, arc.Ink.Drawn)),
            circleMark: static (target, circle) => circle.Thickness.Match(
                Some: thickness => target.DrawCircle(circle.Circle, circle.Ink.Drawn, thickness),
                None: () => target.DrawCircle(circle.Circle, circle.Ink.Drawn)),
            points: static (target, points) => points.Style.Match(
                Some: style => target.DrawPoints(points.Locations, style.Style, style.Radius, points.Ink.Drawn),
                None: () => target.DrawPoints(points.Locations, target.DisplayPipelineAttributes.PointStyle, target.DisplayPipelineAttributes.PointRadius, points.Ink.Drawn)),
            vectorMark: static (target, vector) => {
                Color color = vector.Ink.Drawn;
                target.DrawArrow(new Line(vector.Point, vector.Vector), color);
                if (vector.DrawPoint)
                    target.DrawPoint(vector.Point, color);
            },
            polygon: static (target, polygon) => target.DrawPolygon(polygon.Ring.Corners, polygon.Ink.Drawn, polygon.Filled),
            label3d: static (target, label) => target.Draw3dText(label.Text, label.Ink.Drawn));

    private static IsoDrawEffect Effect(IsoBanding banding) {
        IsoDrawEffect effect = new();
        BandingMapper.Update(banding, effect);
        _ = banding.Bands.Colors.Iter((index, color) => effect.SetBandColor(index, color));
        return effect;
    }

    // --- [SPRITES]
    public static IO<Unit> Draw(DisplayPipeline pipeline, SpriteCache sprites, Seq<SpriteMark> marks) =>
        marks.TraverseM(mark => Draw(pipeline, sprites, mark)).As().Map(static _ => unit);

    private static IO<Unit> Draw(DisplayPipeline pipeline, SpriteCache sprites, SpriteMark mark) =>
        from bitmap in sprites.Get(mark.Source, mark.Blend)
        from drawn in bitmap.Case is DisplayBitmap held ? mark.Anchor.Switch(
            (Pipeline: pipeline, Bitmap: held),
            screen: static (state, screen) => IO.lift(() => state.Pipeline.DrawSprite(state.Bitmap, screen.At, screen.Width, screen.Height)),
            screenSized: static (state, sized) => IO.lift(() => sized.Tint.Match(
                Some: tint => state.Pipeline.DrawSprite(state.Bitmap, sized.At, sized.Size, tint.Drawn),
                None: () => state.Pipeline.DrawSprite(state.Bitmap, sized.At, sized.Size))),
            world: static (state, world) => IO.lift(() => world.Tint.Match(
                Some: tint => state.Pipeline.DrawSprite(state.Bitmap, world.At, world.Size, tint.Drawn, world.SizeInWorldSpace),
                None: () => state.Pipeline.DrawSprite(state.Bitmap, world.At, world.Size, world.SizeInWorldSpace))),
            cloud: static (state, cloud) => IO.lift(() => {
                DisplayBitmapDrawList list = new();
                list.SetPoints(cloud.Points.Map(static point => point.At), cloud.Points.Map(static point => point.Tint.Drawn));
                _ = cloud.Translation.Match(
                    Some: translation => state.Pipeline.DrawSprites(state.Bitmap, list, cloud.Size, translation, cloud.SizeInWorldSpace),
                    None: () => state.Pipeline.DrawSprites(state.Bitmap, list, cloud.Size, cloud.SizeInWorldSpace));
            }),
            particles: static (state, particles) => IO.lift(() => state.Pipeline.DrawParticles(particles.System, state.Bitmap))) : IO.pure(unit)
        select drawn;

    // --- [RETAINED]
    public static IO<Unit> Retain(CustomDisplay display, Seq<RetainedMark> marks) =>
        IO.lift(() => marks.Iter(mark => Retain(display, mark)));

    private static void Retain(CustomDisplay display, RetainedMark mark) =>
        mark.Switch(
            display,
            curveMark: static (target, curve) => curve.Thickness.Match(
                Some: thickness => target.AddCurve(curve.Curve, curve.Ink.Drawn, thickness),
                None: () => target.AddCurve(curve.Curve, curve.Ink.Drawn)),
            lineMark: static (target, line) => line.Thickness.Match(
                Some: thickness => target.AddLine(line.Line, line.Ink.Drawn, thickness),
                None: () => target.AddLine(line.Line, line.Ink.Drawn)),
            arcMark: static (target, arc) => arc.Thickness.Match(
                Some: thickness => target.AddArc(arc.Arc, arc.Ink.Drawn, thickness),
                None: () => target.AddArc(arc.Arc, arc.Ink.Drawn)),
            circleMark: static (target, circle) => circle.Thickness.Match(
                Some: thickness => target.AddCircle(circle.Circle, circle.Ink.Drawn, thickness),
                None: () => target.AddCircle(circle.Circle, circle.Ink.Drawn)),
            points: static (target, points) => points.Style.Match(
                Some: style => target.AddPoints(points.Locations, points.Ink.Drawn, style.Style, style.Radius),
                None: () => target.AddPoints(points.Locations, points.Ink.Drawn)),
            vectorMark: static (target, vector) => target.AddVector(vector.Point, vector.Vector, vector.Ink.Drawn, vector.DrawPoint),
            polygon: static (target, polygon) => {
                Color color = polygon.Ink.Drawn;
                target.AddPolygon(polygon.Ring.Corners, color, color, polygon.Filled, !polygon.Filled);
            },
            label3d: static (target, label) => target.AddText(label.Text, label.Ink.Drawn));
}
