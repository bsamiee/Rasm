using System.Drawing;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display.Feedback;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FeedbackAppearance {
    public Func<Color> Curve { get; init; } = static () => AppearanceSettings.FeedbackColor;
    public Func<Color> Construction { get; init; } = static () => AppearanceSettings.CrosshairColor;
    public Func<Color> ActivePoint { get; init; } = static () => SmartTrackSettings.ActivePointColor;
    public Option<int> CurveThickness { get; init; }
    public Option<int> ConstructionThickness { get; init; }
    public Option<(PointStyle Style, int Radius)> PointStyle { get; init; }
}

[Union]
public abstract partial record RetainedMark {
    public sealed record CurveMark(Curve Curve, Func<Color> Color, Option<int> Thickness) : RetainedMark;
    public sealed record LineMark(Line Line, Func<Color> Color, Option<int> Thickness) : RetainedMark;
    public sealed record ArcMark(Arc Arc, Func<Color> Color, Option<int> Thickness) : RetainedMark;
    public sealed record CircleMark(Circle Circle, Func<Color> Color, Option<int> Thickness) : RetainedMark;
    public sealed record Points(Seq<Point3d> Locations, Func<Color> Color, Option<(PointStyle Style, int Radius)> Style) : RetainedMark;
    public sealed record VectorMark(Point3d Point, Vector3d Vector, Func<Color> Color, bool DrawPoint) : RetainedMark;
    public sealed record Polygon(Seq<Point3d> Corners, Func<Color> Color, bool Filled) : RetainedMark;
    public sealed record Label3d(Text3d Text, Func<Color> Color) : RetainedMark;
}

[Union<Box, Sphere, Torus, Cylinder, Cone>(SkipEqualityComparison = true, SkipToString = true)]
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

[Union]
public abstract partial record MeshLook {
    public sealed record Shaded(Func<Action<IDisposable>, IO<DisplayMaterial>> Material, Option<Seq<int>> Faces) : MeshLook;
    public sealed record Banded(Func<Color> Diffuse, Option<IsoDrawEffect> Banding) : MeshLook;
    public sealed record FalseColors : MeshLook;
    public sealed record Gradient(GradientFill Fill) : MeshLook;
    public sealed record Wires(Func<Color> Color, Option<int> Thickness) : MeshLook;
    public sealed record Vertices(Func<Color> Color) : MeshLook;
    public sealed record Preview(AnalysisPreview Analysis, Func<Color> Color) : MeshLook;
}

[Union]
public abstract partial record BrepLook {
    public sealed record Shaded(Func<Action<IDisposable>, IO<DisplayMaterial>> Material) : BrepLook;
    public sealed record Wires(Func<Color> Color, Option<int> Density) : BrepLook;
    public sealed record Preview(AnalysisPreview Analysis, Func<Color> Color) : BrepLook;
}

[Union]
public abstract partial record SubDLook {
    public sealed record Shaded(Func<Action<IDisposable>, IO<DisplayMaterial>> Material) : SubDLook;
    public sealed record Wires(Func<Color> Color, float Thickness) : SubDLook;
    public sealed record Pens(Option<Func<DisplayPen>> Boundary, Option<Func<DisplayPen>> SmoothInterior, Option<Func<DisplayPen>> Crease, Option<Func<DisplayPen>> Nonmanifold) : SubDLook;
}

[Union]
public abstract partial record WorldMark {
    public sealed record Retained(RetainedMark Mark) : WorldMark;
    public sealed record StrokedCurve(Curve Curve, Func<DisplayPen> Pen) : WorldMark;
    public sealed record StrokedLines(Seq<Line> Segments, Func<DisplayPen> Pen) : WorldMark;
    public sealed record Lines(Seq<Line> Segments, Func<Color> Color, Option<int> Thickness) : WorldMark;
    public sealed record LinesNoClip(Seq<Line> Segments, Func<Color> Color, int Thickness) : WorldMark;
    public sealed record GradientLines(Seq<Line> Segments, float Width, GradientFill Fill) : WorldMark;
    public sealed record Arrows(Seq<Line> Shafts, Func<Color> Color) : WorldMark;
    public sealed record LineArrow(Line Line, Func<Color> Color, int Thickness, double TipSize) : WorldMark;
    public sealed record ArrowHead(Point3d Tip, Vector3d Direction, Func<Color> Color, ArrowHeadSize Size) : WorldMark;
    public sealed record DirectionArrow(Point3d At, Vector3d Direction, Func<Color> Color) : WorldMark;
    public sealed record Marker(Point3d Tip, Vector3d Direction, Func<Color> Color, Option<(int Thickness, double Size, double Rotation)> Shape) : WorldMark;
    public sealed record InferenceLine(Point3d From, Point3d Through, Func<Color> Color, DisplayPipeline.InferenceLineType Kind) : WorldMark;
    public sealed record InferencePoint(Point3d At, Func<Color> Color) : WorldMark;
    public sealed record ActivePoint(Point3d At) : WorldMark;
    public sealed record PointSet(DisplayPointSet Set, Option<(DisplayPointAttributes Fallback, DisplayPointAttributes Override)> Attributes) : WorldMark;
    public sealed record PointCloudMark(PointCloud Cloud, float Size, Option<Func<Color>> Color) : WorldMark;
    public sealed record Wires(PrimitiveSolid Solid, Func<Color> Color, Option<int> Thickness) : WorldMark;
    public sealed record BoxCorners(BoundingBox Box, Func<Color> Color, Option<(double Size, int Thickness)> Shape) : WorldMark;
    public sealed record CurvatureGraph(Curve Curve, Func<Color> Color, Option<(int HairScale, int HairDensity, int SampleDensity)> Sampling) : WorldMark;
    public sealed record MeshMark(Mesh Mesh, MeshLook Look) : WorldMark;
    public sealed record BrepMark(Brep Brep, BrepLook Look) : WorldMark;
    public sealed record SubDMark(SubD SubD, SubDLook Look) : WorldMark;
    public sealed record ExtrusionWires(Extrusion Extrusion, Func<Color> Color, Option<int> Density) : WorldMark;
    public sealed record SurfaceMark(Surface Surface, Func<Color> Color, int Density) : WorldMark;
    public sealed record Block(InstanceDefinition Definition, Option<Transform> Xform, Option<Func<Action<IDisposable>, IO<DisplayMaterial>>> Material) : WorldMark;
    public sealed record ObjectMark(RhinoObject Object, Option<Transform> Xform) : WorldMark;
    public sealed record Clipping(ClippingPlaneSurface Plane, Func<Color> Color) : WorldMark;
    public sealed record LightMark(Light Light, Func<Color> Wireframe) : WorldMark;
    public sealed record ConstructionPlaneMark(ConstructionPlane Plane) : WorldMark;
    public sealed record HatchMark(Hatch Hatch, Func<Color> Fill, Func<Color> Boundary) : WorldMark;
    public sealed record StrokedHatch(Hatch Hatch, Func<Color> Fill, Option<Func<DisplayPen>> Boundary, Func<Color> Background) : WorldMark;
    public sealed record GradientHatch(Hatch Hatch, GradientFill Fill, Option<Func<DisplayPen>> Boundary, Func<Color> Background) : WorldMark;
    public sealed record TextMark(TextEntity Text, Func<Color> Color, Option<TextPlacement> Placement) : WorldMark;
    public sealed record AnnotationMark(AnnotationBase Annotation, Option<RhinoObject> Parent, Func<Color> Color) : WorldMark;
    public sealed record AnnotationArrowhead(Arrowhead Arrowhead, Transform Xform, Func<Color> Color) : WorldMark;
    public sealed record DirectionIndicators(SurfaceDirectionIndicators Indicators) : WorldMark;
}

public sealed record BlendFunction(BlendMode Source, BlendMode Destination);

[Union]
public abstract partial record SpriteAnchor {
    public sealed record Screen(Point2d At, float Width, float Height) : SpriteAnchor;
    public sealed record ScreenSized(Point2d At, float Size, Option<Func<Color>> Tint) : SpriteAnchor;
    public sealed record World(Point3d At, float Size, Option<Func<Color>> Tint, bool SizeInWorldSpace) : SpriteAnchor;
    public sealed record Cloud(Seq<(Point3d At, Func<Color> Tint)> Points, float Size, Option<Vector3d> Translation, bool SizeInWorldSpace) : SpriteAnchor;
    public sealed record Particles(ParticleSystem System) : SpriteAnchor;
}

public sealed record SpriteMark(Bitmap Source, Option<BlendFunction> Blend, SpriteAnchor Anchor);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SpriteCache(IPlugInSink sink) : IDisposable {
    private readonly Disposal<AtomHashMap<(Bitmap Source, Option<BlendFunction> Blend), DisplayBitmap>> bitmaps = new(
        AtomHashMap<(Bitmap Source, Option<BlendFunction> Blend), DisplayBitmap>(),
        held => Callbacks.Run(DisposalOps.Release(held.Values.ToSeq()), new CallbackSite(sink, typeof(SpriteCache), nameof(Dispose))));

    internal IO<Option<DisplayBitmap>> Get(Bitmap source, Option<BlendFunction> blend) =>
        IO.lift(() => bitmaps.Held.Map(held => held.FindOrAdd((source, blend), () => {
            DisplayBitmap bitmap = new(source);
            if (blend.Case is BlendFunction selected)
                bitmap.SetBlendFunction(selected.Source, selected.Destination);
            return bitmap;
        })));

    public void Dispose() => bitmaps.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Marks {
    // --- [PIPELINE]
    public static IO<Unit> Draw(DisplayPipeline pipeline, Seq<WorldMark> marks) =>
        marks.TraverseM(mark => Draw(pipeline, mark)).As().Map(static _ => unit);

    public static IO<Unit> Draw(Func<Action<IDisposable>, IO<DisplayMaterial>> create, Action<DisplayMaterial> draw) =>
        IO.env.Bind(environment =>
            DisposalOps.Within((Action<IDisposable> retain) =>
                 from material in IO.liftVAsync(() => create(retain).RunAsync(environment))
                 from drawn in IO.lift(() => draw(material))
                 select drawn, DisposalOps.Release).Uninterruptible());

    private static IO<Unit> Draw(DisplayPipeline pipeline, WorldMark mark) =>
        mark.Switch(
            pipeline,
            retained: static (target, retained) => IO.lift(() => Draw(target, retained.Mark)),
            strokedCurve: static (target, stroked) => IO.lift(() => target.DrawCurve(stroked.Curve, stroked.Pen())),
            strokedLines: static (target, stroked) => IO.lift(() => target.DrawLines([.. stroked.Segments], stroked.Pen())),
            lines: static (target, lines) => IO.lift(() => lines.Thickness.Match(
                Some: thickness => target.DrawLines(lines.Segments, lines.Color(), thickness),
                None: () => target.DrawLines(lines.Segments, lines.Color()))),
            linesNoClip: static (target, lines) => IO.lift(() => target.DrawLinesNoClip(lines.Segments, lines.Color(), lines.Thickness)),
            gradientLines: static (target, lines) => IO.lift(() => target.DrawGradientLines(lines.Segments, lines.Width, lines.Fill.Stops, lines.Fill.Start, lines.Fill.End, lines.Fill.Linear, lines.Fill.Repeat)),
            arrows: static (target, arrows) => IO.lift(() => target.DrawArrows(arrows.Shafts, arrows.Color())),
            lineArrow: static (target, arrow) => IO.lift(() => target.DrawLineArrow(arrow.Line, arrow.Color(), arrow.Thickness, arrow.TipSize)),
            arrowHead: static (target, head) => IO.lift(() => head.Size.Switch(
                (Pipeline: target, Head: head),
                screen: static (state, pixels) => state.Pipeline.DrawArrowHead(state.Head.Tip, state.Head.Direction, state.Head.Color(), pixels, 0),
                world: static (state, units) => state.Pipeline.DrawArrowHead(state.Head.Tip, state.Head.Direction, state.Head.Color(), 0, units))),
            directionArrow: static (target, arrow) => IO.lift(() => target.DrawDirectionArrow(arrow.At, arrow.Direction, arrow.Color())),
            marker: static (target, marker) => IO.lift(() => marker.Shape.Match(
                Some: shape => target.DrawMarker(marker.Tip, marker.Direction, marker.Color(), shape.Thickness, shape.Size, shape.Rotation),
                None: () => target.DrawMarker(marker.Tip, marker.Direction, marker.Color()))),
            inferenceLine: static (target, line) => IO.lift(() => target.DrawInferenceLine(line.From, line.Through, line.Color(), line.Kind)),
            inferencePoint: static (target, point) => IO.lift(() => target.DraweInferencePoint(point.At, point.Color())),
            activePoint: static (target, point) => IO.lift(() => target.DrawActivePoint(point.At)),
            pointSet: static (target, points) => IO.lift(() => points.Attributes.Match(
                Some: attributes => target.DrawPoints(points.Set, attributes.Fallback, attributes.Override),
                None: () => target.DrawPoints(points.Set))),
            pointCloudMark: static (target, cloud) => IO.lift(() => cloud.Color.Match(
                Some: color => target.DrawPointCloud(cloud.Cloud, cloud.Size, color()),
                None: () => target.DrawPointCloud(cloud.Cloud, cloud.Size))),
            wires: static (target, wires) => IO.lift(() => wires.Solid.Switch(
                (Pipeline: target, Wires: wires),
                box: static (state, box) => state.Wires.Thickness.Match(
                    Some: thickness => state.Pipeline.DrawBox(box, state.Wires.Color(), thickness),
                    None: () => state.Pipeline.DrawBox(box, state.Wires.Color())),
                sphere: static (state, sphere) => state.Wires.Thickness.Match(
                    Some: thickness => state.Pipeline.DrawSphere(sphere, state.Wires.Color(), thickness),
                    None: () => state.Pipeline.DrawSphere(sphere, state.Wires.Color())),
                torus: static (state, torus) => state.Wires.Thickness.Match(
                    Some: thickness => state.Pipeline.DrawTorus(torus, state.Wires.Color(), thickness),
                    None: () => state.Pipeline.DrawTorus(torus, state.Wires.Color())),
                cylinder: static (state, cylinder) => state.Wires.Thickness.Match(
                    Some: thickness => state.Pipeline.DrawCylinder(cylinder, state.Wires.Color(), thickness),
                    None: () => state.Pipeline.DrawCylinder(cylinder, state.Wires.Color())),
                cone: static (state, cone) => state.Wires.Thickness.Match(
                    Some: thickness => state.Pipeline.DrawCone(cone, state.Wires.Color(), thickness),
                    None: () => state.Pipeline.DrawCone(cone, state.Wires.Color())))),
            boxCorners: static (target, corners) => IO.lift(() => corners.Shape.Match(
                Some: shape => target.DrawBoxCorners(corners.Box, corners.Color(), shape.Size, shape.Thickness),
                None: () => target.DrawBoxCorners(corners.Box, corners.Color()))),
            curvatureGraph: static (target, graph) => IO.lift(() => graph.Sampling.Match(
                Some: sampling => target.DrawCurvatureGraph(graph.Curve, graph.Color(), sampling.HairScale, sampling.HairDensity, sampling.SampleDensity),
                None: () => target.DrawCurvatureGraph(graph.Curve, graph.Color()))),
            meshMark: static (target, mesh) => mesh.Look.Switch(
                (Pipeline: target, mesh.Mesh),
                shaded: static (state, shaded) => shaded.Faces.Case is Seq<int> faces
                    ? Draw(shaded.Material, material => state.Pipeline.DrawMeshShaded(state.Mesh, material, faces))
                    : Draw(shaded.Material, material => state.Pipeline.DrawMeshShaded(state.Mesh, material)),
                banded: static (state, banded) => IO.lift(() => state.Pipeline.DrawMeshShaded(state.Mesh, banded.Diffuse(), banded.Banding.ValueUnsafe())),
                falseColors: static (state, _) => IO.lift(() => state.Pipeline.DrawMeshFalseColors(state.Mesh)),
                gradient: static (state, gradient) => IO.lift(() => state.Pipeline.DrawGradientMesh(state.Mesh, gradient.Fill.Stops, gradient.Fill.Start, gradient.Fill.End, gradient.Fill.Linear, gradient.Fill.Repeat)),
                wires: static (state, wires) => IO.lift(() => wires.Thickness.Match(
                    Some: thickness => state.Pipeline.DrawMeshWires(state.Mesh, wires.Color(), thickness),
                    None: () => state.Pipeline.DrawMeshWires(state.Mesh, wires.Color()))),
                vertices: static (state, vertices) => IO.lift(() => state.Pipeline.DrawMeshVertices(state.Mesh, vertices.Color())),
                preview: static (state, preview) => IO.lift(() => preview.Analysis.DrawMesh(state.Pipeline, state.Mesh, preview.Color()))),
            brepMark: static (target, brep) => brep.Look.Switch(
                (Pipeline: target, brep.Brep),
                shaded: static (state, shaded) => Draw(shaded.Material, material => state.Pipeline.DrawBrepShaded(state.Brep, material)),
                wires: static (state, wires) => IO.lift(() => wires.Density.Match(
                    Some: density => state.Pipeline.DrawBrepWires(state.Brep, wires.Color(), density),
                    None: () => state.Pipeline.DrawBrepWires(state.Brep, wires.Color()))),
                preview: static (state, preview) => IO.lift(() => preview.Analysis.DrawBrep(state.Pipeline, state.Brep, preview.Color()))),
            subDMark: static (target, subd) => subd.Look.Switch(
                (Pipeline: target, subd.SubD),
                shaded: static (state, shaded) => Draw(shaded.Material, material => state.Pipeline.DrawSubDShaded(state.SubD, material)),
                wires: static (state, wires) => IO.lift(() => state.Pipeline.DrawSubDWires(state.SubD, wires.Color(), wires.Thickness)),
                pens: static (state, pens) => IO.lift(() => state.Pipeline.DrawSubDWires(
                    state.SubD,
                    pens.Boundary.Map(static create => create()).ValueUnsafe(),
                    pens.SmoothInterior.Map(static create => create()).ValueUnsafe(),
                    pens.Crease.Map(static create => create()).ValueUnsafe(),
                    pens.Nonmanifold.Map(static create => create()).ValueUnsafe()))),
            extrusionWires: static (target, wires) => IO.lift(() => wires.Density.Match(
                Some: density => target.DrawExtrusionWires(wires.Extrusion, wires.Color(), density),
                None: () => target.DrawExtrusionWires(wires.Extrusion, wires.Color()))),
            surfaceMark: static (target, surface) => IO.lift(() => target.DrawSurface(surface.Surface, surface.Color(), surface.Density)),
            block: static (target, block) => block.Material.Case is Func<Action<IDisposable>, IO<DisplayMaterial>> material
                ? Draw(material, shaded => block.Xform.Match(
                    Some: xform => target.DrawInstanceDefinitionShaded(block.Definition, shaded, xform),
                    None: () => target.DrawInstanceDefinitionShaded(block.Definition, shaded)))
                : IO.lift(() => block.Xform.Match(
                    Some: xform => target.DrawInstanceDefinition(block.Definition, xform),
                    None: () => target.DrawInstanceDefinition(block.Definition))),
            objectMark: static (target, drawn) => IO.lift(() => drawn.Xform.Match(
                Some: xform => target.DrawObject(drawn.Object, xform),
                None: () => target.DrawObject(drawn.Object))),
            clipping: static (target, clipping) => IO.lift(() => target.DrawClippingPlaneWires(clipping.Plane, clipping.Color())),
            lightMark: static (target, light) => IO.lift(() => target.DrawLight(light.Light, light.Wireframe())),
            constructionPlaneMark: static (target, plane) => IO.lift(() => target.DrawConstructionPlane(plane.Plane)),
            hatchMark: static (target, hatch) => IO.lift(() => target.DrawHatch(hatch.Hatch, hatch.Fill(), hatch.Boundary())),
            strokedHatch: static (target, hatch) => IO.lift(() => target.DrawHatch(hatch.Hatch, hatch.Fill(), hatch.Boundary.Map(static create => create()).ValueUnsafe(), hatch.Background())),
            gradientHatch: static (target, hatch) => IO.lift(() => target.DrawGradientHatch(
                hatch.Hatch, hatch.Fill.Stops, hatch.Fill.Start, hatch.Fill.End, hatch.Fill.Linear, hatch.Fill.Repeat, hatch.Boundary.Map(static create => create()).ValueUnsafe(), hatch.Background())),
            textMark: static (target, text) => IO.lift(() => text.Placement.Match(
                Some: placement => placement.Switch(
                    (Pipeline: target, Text: text),
                    scaled: static (state, scale) => state.Pipeline.DrawText(state.Text.Text, state.Text.Color(), scale),
                    transformed: static (state, xform) => state.Pipeline.DrawText(state.Text.Text, state.Text.Color(), xform)),
                None: () => target.DrawText(text.Text, text.Color()))),
            annotationMark: static (target, annotation) => IO.lift(() => annotation.Parent.Match(
                Some: parent => target.DrawAnnotation(annotation.Annotation, parent, annotation.Color()),
                None: () => target.DrawAnnotation(annotation.Annotation, annotation.Color()))),
            annotationArrowhead: static (target, arrowhead) => IO.lift(() => target.DrawAnnotationArrowhead(arrowhead.Arrowhead, arrowhead.Xform, arrowhead.Color())),
            directionIndicators: static (target, direction) => IO.lift(() => target.DrawSurfaceDirectionIndicators(direction.Indicators)));

    private static void Draw(DisplayPipeline pipeline, RetainedMark mark) =>
        mark.Switch(
            pipeline,
            curveMark: static (target, curve) => curve.Thickness.Match(
                Some: thickness => target.DrawCurve(curve.Curve, curve.Color(), thickness),
                None: () => target.DrawCurve(curve.Curve, curve.Color())),
            lineMark: static (target, line) => line.Thickness.Match(
                Some: thickness => target.DrawLine(line.Line, line.Color(), thickness),
                None: () => target.DrawLine(line.Line, line.Color())),
            arcMark: static (target, arc) => arc.Thickness.Match(
                Some: thickness => target.DrawArc(arc.Arc, arc.Color(), thickness),
                None: () => target.DrawArc(arc.Arc, arc.Color())),
            circleMark: static (target, circle) => circle.Thickness.Match(
                Some: thickness => target.DrawCircle(circle.Circle, circle.Color(), thickness),
                None: () => target.DrawCircle(circle.Circle, circle.Color())),
            points: static (target, points) => points.Style.Match(
                Some: style => target.DrawPoints(points.Locations, style.Style, style.Radius, points.Color()),
                None: () => target.DrawPoints(points.Locations, target.DisplayPipelineAttributes.PointStyle, target.DisplayPipelineAttributes.PointRadius, points.Color())),
            vectorMark: static (target, vector) => {
                Color color = vector.Color();
                target.DrawArrow(new Line(vector.Point, vector.Vector), color);
                if (vector.DrawPoint)
                    target.DrawPoint(vector.Point, color);
            },
            polygon: static (target, polygon) => target.DrawPolygon(polygon.Corners, polygon.Color(), polygon.Filled),
            label3d: static (target, label) => target.Draw3dText(label.Text, label.Color()));

    // --- [SPRITES]
    public static IO<Unit> Draw(DisplayPipeline pipeline, SpriteCache sprites, Seq<SpriteMark> marks) =>
        marks.TraverseM(mark => Draw(pipeline, sprites, mark)).As().Map(static _ => unit);

    private static IO<Unit> Draw(DisplayPipeline pipeline, SpriteCache sprites, SpriteMark mark) =>
        from bitmap in sprites.Get(mark.Source, mark.Blend)
        from drawn in IO.lift(() => bitmap.Iter(held => mark.Anchor.Switch(
            (Pipeline: pipeline, Bitmap: held),
            screen: static (state, screen) => state.Pipeline.DrawSprite(state.Bitmap, screen.At, screen.Width, screen.Height),
            screenSized: static (state, sized) => sized.Tint.Match(
                Some: tint => state.Pipeline.DrawSprite(state.Bitmap, sized.At, sized.Size, tint()),
                None: () => state.Pipeline.DrawSprite(state.Bitmap, sized.At, sized.Size)),
            world: static (state, world) => world.Tint.Match(
                Some: tint => state.Pipeline.DrawSprite(state.Bitmap, world.At, world.Size, tint(), world.SizeInWorldSpace),
                None: () => state.Pipeline.DrawSprite(state.Bitmap, world.At, world.Size, world.SizeInWorldSpace)),
            cloud: static (state, cloud) => {
                DisplayBitmapDrawList list = new();
                list.SetPoints(cloud.Points.Map(static point => point.At), cloud.Points.Map(static point => point.Tint()));
                _ = cloud.Translation.Match(
                    Some: translation => state.Pipeline.DrawSprites(state.Bitmap, list, cloud.Size, translation, cloud.SizeInWorldSpace),
                    None: () => state.Pipeline.DrawSprites(state.Bitmap, list, cloud.Size, cloud.SizeInWorldSpace));
            },
            particles: static (state, particles) => state.Pipeline.DrawParticles(particles.System, state.Bitmap))))
        select drawn;

    // --- [RETAINED]
    public static IO<Unit> Retain(CustomDisplay display, Seq<RetainedMark> marks) =>
        IO.lift(() => marks.Iter(mark => Retain(display, mark)));

    private static void Retain(CustomDisplay display, RetainedMark mark) =>
        mark.Switch(
            display,
            curveMark: static (target, curve) => curve.Thickness.Match(
                Some: thickness => target.AddCurve(curve.Curve, curve.Color(), thickness),
                None: () => target.AddCurve(curve.Curve, curve.Color())),
            lineMark: static (target, line) => line.Thickness.Match(
                Some: thickness => target.AddLine(line.Line, line.Color(), thickness),
                None: () => target.AddLine(line.Line, line.Color())),
            arcMark: static (target, arc) => arc.Thickness.Match(
                Some: thickness => target.AddArc(arc.Arc, arc.Color(), thickness),
                None: () => target.AddArc(arc.Arc, arc.Color())),
            circleMark: static (target, circle) => circle.Thickness.Match(
                Some: thickness => target.AddCircle(circle.Circle, circle.Color(), thickness),
                None: () => target.AddCircle(circle.Circle, circle.Color())),
            points: static (target, points) => points.Style.Match(
                Some: style => target.AddPoints(points.Locations, points.Color(), style.Style, style.Radius),
                None: () => target.AddPoints(points.Locations, points.Color())),
            vectorMark: static (target, vector) => target.AddVector(vector.Point, vector.Vector, vector.Color(), vector.DrawPoint),
            polygon: static (target, polygon) => {
                Color color = polygon.Color();
                target.AddPolygon(polygon.Corners, color, color, polygon.Filled, !polygon.Filled);
            },
            label3d: static (target, label) => target.AddText(label.Text, label.Color()));
}
