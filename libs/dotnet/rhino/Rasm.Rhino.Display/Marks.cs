using System.Drawing;
using Rasm.Rhino.Document;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BlendFunction(BlendMode Source, BlendMode Destination) {
    public static BlendFunction Over { get; } = new(BlendMode.SourceAlpha, BlendMode.OneMinusSourceAlpha);
}

[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class IsoBanding {
    private const int BandCapacity = 10;

    public IsoDrawMode Mode { get; }

    public Vector3d Direction { get; }

    public Point3d Point { get; }

    public int Frequency { get; }

    public double RotationRadians { get; }

    public double Falloff { get; }

    public double GapSize { get; }

    public Color GapColor { get; }

    public bool DiscardGap { get; }

    public Seq<Color> Bands { get; }

    public static Fin<IsoBanding> From(IsoDrawMode mode, Vector3d direction, Point3d point, int frequency, double rotationRadians, double falloff, double gapSize, Color gapColor, bool discardGap, Seq<Color> bands) =>
        Validate(mode, direction, point, frequency, rotationRadians, falloff, gapSize, gapColor, discardGap, bands, out IsoBanding? banding) is { } error ? error : banding!;

    static partial void ValidateFactoryArguments(
        ref ValidationFailure? validationError,
        ref IsoDrawMode mode,
        ref Vector3d direction,
        ref Point3d point,
        ref int frequency,
        ref double rotationRadians,
        ref double falloff,
        ref double gapSize,
        ref Color gapColor,
        ref bool discardGap,
        ref Seq<Color> bands) =>
        validationError = Limits.AtLeast(1).AtMost(BandCapacity).Violated(bands.Count, nameof(Bands))
            ?? Limits.AtLeast(1).Violated(frequency, nameof(Frequency))
            ?? Limits.AtLeast(0.0).Violated(gapSize, nameof(GapSize))
            ?? Limits.AtLeast(0.0).Violated(falloff, nameof(Falloff));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpriteAnchor {
    public sealed record Screen(Point2d At, float Width, float Height) : SpriteAnchor;

    public sealed record ScreenSized(Point2d At, float Size, Color Blend) : SpriteAnchor;

    public sealed record World(Point3d At, float Size, Color Blend, bool SizeInWorldSpace) : SpriteAnchor;

    public sealed record Cloud : SpriteAnchor {
        private Cloud(Seq<Point3d> points, Option<Seq<Color>> colors, float size, bool sizeInWorldSpace, Vector3d translation) =>
            (Points, Colors, Size, SizeInWorldSpace, Translation) = (points, colors, size, sizeInWorldSpace, translation);

        public Seq<Point3d> Points { get; }

        public Option<Seq<Color>> Colors { get; }

        public float Size { get; }

        public bool SizeInWorldSpace { get; }

        public Vector3d Translation { get; }

        public static Fin<SpriteAnchor> Create(Seq<Point3d> points, Option<Seq<Color>> colors, float size, bool sizeInWorldSpace, Vector3d translation) =>
            colors.Traverse(listed => CountMismatch.Unless(points.Count, listed.Count, nameof(Colors))).As()
                .Map<SpriteAnchor>(_ => new Cloud(points, colors, size, sizeInWorldSpace, translation));
    }
}

public sealed record SpriteMark(Bitmap Source, BlendFunction Blend, SpriteAnchor Anchor);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetainedMark {
    public sealed record CurveMark(Curve Curve, Color Color) : RetainedMark;

    public sealed record LineMark(Line Line, Color Color) : RetainedMark;

    public sealed record ArcMark(Arc Arc, Color Color) : RetainedMark;

    public sealed record Points(Seq<Point3d> Locations, Color Color) : RetainedMark;

    public sealed record StyledPoints(Seq<Point3d> Locations, Color Color, PointStyle Style, int Radius) : RetainedMark;

    public sealed record VectorMark(Point3d Point, Vector3d Vector, Color Color, bool DrawPoint) : RetainedMark;

    public sealed record Polygon : RetainedMark {
        private Polygon(Seq<Point3d> ring, Option<Color> fill, Option<Color> edge) =>
            (Ring, Fill, Edge) = (ring, fill, edge);

        public Seq<Point3d> Ring { get; }

        public Option<Color> Fill { get; }

        public Option<Color> Edge { get; }

        public static Fin<RetainedMark> Create(Seq<Point3d> ring, Option<Color> fill, Option<Color> edge) =>
            Limits.AtLeast(3).Check(ring.Count, nameof(Ring)).Map<RetainedMark>(_ => new Polygon(ring, fill, edge));
    }

    public sealed record Label3d(Text3d Text, Color Color) : RetainedMark;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WorldMark {
    public sealed record Retained(RetainedMark Mark) : WorldMark;

    public sealed record StrokedCurve(Curve Curve, Stroke Stroke) : WorldMark;

    public sealed record MeshShaded(Mesh Mesh, ShadedMaterial Material) : WorldMark;

    public sealed record MeshBanded(Mesh Mesh, Color Diffuse, Option<IsoBanding> Banding) : WorldMark;

    public sealed record MeshFalseColors(Mesh Mesh) : WorldMark;

    public sealed record SubDShaded(SubD SubD, ShadedMaterial Material) : WorldMark;

    public sealed record SubDWires(SubD SubD, Color Color, float Thickness) : WorldMark;

    public sealed record BrepShaded(Brep Brep, ShadedMaterial Material) : WorldMark;

    public sealed record BrepWires(Brep Brep, Color Color, int Density) : WorldMark;

    public sealed record Block(InstanceDefinition Definition, Transform Xform) : WorldMark;

    public sealed record ShadedBlock(InstanceDefinition Definition, Transform Xform, ShadedMaterial Material) : WorldMark;

    public sealed record ObjectMark(RhinoObject Object) : WorldMark;

    public sealed record TransformedObject(RhinoObject Object, Transform Xform) : WorldMark;

    public sealed record Clipping(ClippingPlaneSurface Plane, Color Color) : WorldMark;

    public sealed record HatchMark(Hatch Hatch, Color Lines, Option<Stroke> Boundary, Option<Color> Fill) : WorldMark;

    public sealed record TextMark(TextEntity Text, Color Color, double Scale) : WorldMark;

    public sealed record AnnotationMark(AnnotationBase Annotation, Option<RhinoObject> Parent, Color Color) : WorldMark;

    public sealed record ArrowheadMark(Arrowhead Arrowhead, Transform Xform, Color Color) : WorldMark;

    public sealed record Direction(SurfaceDirectionIndicators Indicators) : WorldMark;

    public sealed record Curvature(Brep Brep, Color Color) : WorldMark;

    public sealed record Draft(Mesh Mesh, Color Color) : WorldMark;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SpriteCache : IDisposable {
    private readonly AtomHashMap<(Bitmap Source, BlendFunction Blend), DisplayBitmap> bitmaps =
        AtomHashMap<(Bitmap Source, BlendFunction Blend), DisplayBitmap>();

    private readonly Disposal disposal;

    public SpriteCache(Action<Error> reject) =>
        disposal = new(() => _ = Answers.Answer(DisposalOps.Release(toSeq(bitmaps.Values)), reject, unit));

    public IO<DisplayBitmap> Get(Bitmap source, BlendFunction blend) =>
        IO.lift(() => bitmaps.FindOrAdd((source, blend), () => {
            DisplayBitmap bitmap = new(source);
            bitmap.SetBlendFunction(blend.Source, blend.Destination);
            return bitmap;
        }));

    public void Dispose() => disposal.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class BandingMapper {
    [MapProperty(nameof(IsoBanding.Mode), nameof(IsoDrawEffect.DrawMode))]
    [MapProperty(nameof(@IsoBanding.Bands.Count), nameof(IsoDrawEffect.UsedBandColorCount))]
    internal static partial IsoDrawEffect ToEffect(IsoBanding banding);
}

public static class Marks {
    // --- [PIPELINE]
    public static IO<Unit> DrawWorld(DisplayPipeline pipeline, Seq<WorldMark> marks) =>
        marks.TraverseM(mark => Draw(pipeline, mark)).As().Map(static _ => unit);

    public static IO<Unit> DrawWorld(DisplayPipeline pipeline, Seq<RetainedMark> marks) =>
        IO.lift(() => marks.Iter(mark => Immediate(pipeline, mark)));

    public static IO<Unit> DrawSprite(DisplayPipeline pipeline, SpriteCache cache, SpriteMark mark) =>
        from bitmap in cache.Get(mark.Source, mark.Blend)
        from drawn in mark.Anchor.Switch(
            (Pipeline: pipeline, Bitmap: bitmap),
            screen: static (state, screen) => IO.lift(() => state.Pipeline.DrawSprite(state.Bitmap, screen.At, screen.Width, screen.Height)),
            screenSized: static (state, sized) => IO.lift(() => state.Pipeline.DrawSprite(state.Bitmap, sized.At, sized.Size, sized.Blend)),
            world: static (state, world) => IO.lift(() => state.Pipeline.DrawSprite(state.Bitmap, world.At, world.Size, world.Blend, world.SizeInWorldSpace)),
            cloud: static (state, cloud) =>
                from list in IO.lift(static () => new DisplayBitmapDrawList())
                from set in IO.lift(() => cloud.Colors.Match(Some: colors => list.SetPoints(cloud.Points, colors), None: () => list.SetPoints(cloud.Points)))
                from sprites in IO.lift(() => state.Pipeline.DrawSprites(state.Bitmap, list, cloud.Size, cloud.Translation, cloud.SizeInWorldSpace))
                select sprites)
        select drawn;

    private static IO<Unit> Draw(DisplayPipeline pipeline, WorldMark mark) =>
        mark.Switch(
            pipeline,
            retained: static (target, retained) => IO.lift(() => Immediate(target, retained.Mark)),
            strokedCurve: static (target, stroked) => IO.lift(() => target.DrawCurve(stroked.Curve, Strokes.ToPen(stroked.Stroke))),
            meshShaded: static (target, mesh) => Strokes.Use(mesh.Material, material => target.DrawMeshShaded(mesh.Mesh, material)),
            meshBanded: static (target, banded) => IO.lift(() => target.DrawMeshShaded(banded.Mesh, banded.Diffuse, banded.Banding.Map(Effect).ValueUnsafe())),
            meshFalseColors: static (target, mesh) => IO.lift(() => target.DrawMeshFalseColors(mesh.Mesh)),
            subDShaded: static (target, subd) => Strokes.Use(subd.Material, material => target.DrawSubDShaded(subd.SubD, material)),
            subDWires: static (target, wires) => IO.lift(() => target.DrawSubDWires(wires.SubD, wires.Color, wires.Thickness)),
            brepShaded: static (target, brep) => Strokes.Use(brep.Material, material => target.DrawBrepShaded(brep.Brep, material)),
            brepWires: static (target, wires) => IO.lift(() => target.DrawBrepWires(wires.Brep, wires.Color, wires.Density)),
            block: static (target, block) => IO.lift(() => target.DrawInstanceDefinition(block.Definition, block.Xform)),
            shadedBlock: static (target, block) => Strokes.Use(block.Material, shaded => target.DrawInstanceDefinitionShaded(block.Definition, shaded, block.Xform)),
            objectMark: static (target, drawn) => IO.lift(() => target.DrawObject(drawn.Object)),
            transformedObject: static (target, drawn) => IO.lift(() => target.DrawObject(drawn.Object, drawn.Xform)),
            clipping: static (target, clipping) => IO.lift(() => target.DrawClippingPlaneWires(clipping.Plane, clipping.Color)),
            hatchMark: static (target, hatch) => IO.lift(() => target.DrawHatch(
                hatch.Hatch,
                hatch.Lines,
                hatch.Boundary.Map(Strokes.ToPen).ValueUnsafe(),
                hatch.Fill.IfNone(Color.Empty))),
            textMark: static (target, text) => IO.lift(() => target.DrawText(text.Text, text.Color, text.Scale)),
            annotationMark: static (target, annotation) => IO.lift(() => target.DrawAnnotation(annotation.Annotation, annotation.Parent.ValueUnsafe(), annotation.Color)),
            arrowheadMark: static (target, arrowhead) => IO.lift(() => target.DrawAnnotationArrowhead(arrowhead.Arrowhead, arrowhead.Xform, arrowhead.Color)),
            direction: static (target, direction) => IO.lift(() => target.DrawSurfaceDirectionIndicators(direction.Indicators)),
            curvature: static (target, curvature) => IO.lift(() => target.DrawCurvaturePreview(curvature.Brep, curvature.Color)),
            draft: static (target, draft) => IO.lift(() => target.DrawDraftAnglePreview(draft.Mesh, draft.Color)));

    private static void Immediate(DisplayPipeline pipeline, RetainedMark mark) =>
        mark.Switch(
            pipeline,
            curveMark: static (target, curve) => target.DrawCurve(curve.Curve, curve.Color, target.DefaultCurveThickness),
            lineMark: static (target, line) => target.DrawLine(line.Line, line.Color, target.DefaultCurveThickness),
            arcMark: static (target, arc) => target.DrawArc(arc.Arc, arc.Color, target.DefaultCurveThickness),
            points: static (target, points) => target.DrawPoints(points.Locations, target.DisplayPipelineAttributes.PointStyle, target.DisplayPipelineAttributes.PointRadius, points.Color),
            styledPoints: static (target, points) => target.DrawPoints(points.Locations, points.Style, points.Radius, points.Color),
            vectorMark: static (target, vector) => {
                target.DrawArrow(new Line(vector.Point, vector.Point + vector.Vector), vector.Color);
                if (vector.DrawPoint)
                    target.DrawPoint(vector.Point, vector.Color);
            },
            polygon: static (target, polygon) => {
                _ = polygon.Fill.Iter(fill => target.DrawPolygon(polygon.Ring, fill, filled: true));
                _ = polygon.Edge.Iter(edge => target.DrawPolygon(polygon.Ring, edge, filled: false));
            },
            label3d: static (target, label) => target.Draw3dText(label.Text, label.Color));

    private static IsoDrawEffect Effect(IsoBanding banding) {
        IsoDrawEffect effect = BandingMapper.ToEffect(banding);
        _ = banding.Bands.Iter((index, color) => _ = effect.SetBandColor(index, color));
        return effect;
    }

    // --- [RETAINED]
    public static IO<Unit> DrawRetained(CustomDisplay display, Seq<RetainedMark> marks) =>
        IO.lift(() => marks.Iter(mark => Retain(display, mark)));

    private static void Retain(CustomDisplay display, RetainedMark mark) =>
        mark.Switch(
            display,
            curveMark: static (target, curve) => target.AddCurve(curve.Curve, curve.Color),
            lineMark: static (target, line) => target.AddLine(line.Line, line.Color),
            arcMark: static (target, arc) => target.AddArc(arc.Arc, arc.Color),
            points: static (target, points) => target.AddPoints(points.Locations, points.Color),
            styledPoints: static (target, points) => target.AddPoints(points.Locations, points.Color, points.Style, points.Radius),
            vectorMark: static (target, vector) => target.AddVector(vector.Point, vector.Vector, vector.Color, vector.DrawPoint),
            polygon: static (target, polygon) => target.AddPolygon(
                polygon.Ring,
                polygon.Fill.IfNone(Color.Empty),
                polygon.Edge.IfNone(Color.Empty),
                polygon.Fill.IsSome,
                polygon.Edge.IsSome),
            label3d: static (target, label) => target.AddText(label.Text, label.Color));
}
