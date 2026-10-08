using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rhino.Geometry.Morphs;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record MorphKind {
    public sealed record Bend(Point3d Start, Point3d End, Point3d Through, Option<double> Angle, bool Straight, bool Symmetric) : MorphKind;

    public sealed record Flow(Curve Base, Curve Target, bool ReverseBase, bool ReverseTarget, bool PreventStretching) : MorphKind;

    public sealed record Maelstrom(Plane Plane, double Radius0, double Radius1, double Angle) : MorphKind;

    public sealed record Splop(Plane Plane, Surface Surface, Point2d Parameter, Option<double> Scale, Option<double> Angle) : MorphKind;

    public sealed record Sporph(Surface Source, Surface Target, Option<(Point2d Source, Point2d Target)> Parameters, Option<Vector3d> ConstrainNormal) : MorphKind;

    public sealed record StretchToLength(Point3d Start, Point3d End, double Length) : MorphKind;

    public sealed record StretchToPoint(Point3d Start, Point3d End, Point3d Point) : MorphKind;

    public sealed record Taper(Point3d Start, Point3d End, double StartRadius, double EndRadius, bool Flat, bool Infinite) : MorphKind;

    public sealed record Twist(Line TwistAxis, double TwistAngleRadians, bool InfiniteTwist) : MorphKind;

    public sealed record Cage(Mesh Reference, Mesh Target) : MorphKind;
}

public sealed record MorphSettings(Tolerances Tolerances, bool QuickPreview, bool PreserveStructure);

public sealed record UnrollOptions(Option<double> Explode, Tolerances Tolerances);

public sealed record UnrollResult(Seq<Brep> Flat, Seq<(Curve Curve, Option<int> Source)> Curves, Seq<Point3d> Points, Seq<(TextDot Dot, Option<int> Source)> Dots);

public sealed record SquishResult<T>(
    T Flat,
    Seq<(GeometryBase Source, Option<GeometryBase> Flat)> Marks,
    Mesh Mesh2d,
    Mesh Mesh3d,
    Seq<(Line Flat, Line Source)> LengthConstrainedLines,
    Seq<MeshFace> AreaConstrainedTriangles) where T : GeometryBase;

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class DeformMapper {
    [MapProperty(nameof(@MorphSettings.Tolerances.Absolute), nameof(SpaceMorph.Tolerance))]
    internal static partial void Update(MorphSettings settings, SpaceMorph morph);

    [MapProperty(nameof(@MorphSettings.Tolerances.Absolute), nameof(MorphControl.SpaceMorphTolerance))]
    internal static partial void Update(MorphSettings settings, MorphControl control);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(MorphKind.Twist twist, TwistSpaceMorph morph);

    internal static void Update(UnrollOptions options, Unroller unroller) {
        unroller.ExplodeOutput = options.Explode.IsSome;
        _ = options.Explode.IfSome(spacing => unroller.ExplodeSpacing = spacing);
        unroller.AbsoluteTolerance = options.Tolerances.Absolute;
        unroller.RelativeTolerance = options.Tolerances.Relative;
    }
}

public static class Deforms {
    // --- [MORPHS]
    public static IO<Seq<T>> Morph<T>(Seq<T> targets, MorphKind kind, MorphSettings settings) where T : GeometryBase =>
        Morphing(kind, Configured(settings, morph => Morphed(targets, morph.Morph, nameof(SpaceMorph.Morph))));

    public static IO<Seq<T>> Morph<T>(Seq<T> targets, NurbsCurve origin, NurbsCurve target, MorphSettings settings) where T : GeometryBase =>
        (from control in use(() => new MorphControl(origin, target))
         from valid in IO.lift(Refused.Unless(control.IsValid, nameof(MorphControl)))
         from configured in IO.lift(() => DeformMapper.Update(settings, control))
         from morphed in Morphed(targets, control.Morph, nameof(MorphControl.Morph))
         select morphed).Bracket();

    public static IO<Seq<Plane>> Morph(Seq<Plane> planes, MorphKind kind, MorphSettings settings) =>
        Morphing(kind, Configured(settings, morph => IO.lift(() =>
            Callbacks.Each(planes, (plane, index) => RefusedElement.Unless(morph.Morph(ref plane), plane, nameof(SpaceMorph.Morph), index)))));

    public static IO<Seq<Point3d>> MorphPoint(Seq<Point3d> points, MorphKind kind) =>
        Morphing(kind, morph => IO.lift(() => points.Map(morph.MorphPoint).Strict()));

    private static IO<A> Morphing<A>(MorphKind kind, Func<SpaceMorph, IO<A>> apply) =>
        kind.Switch(
            apply,
            bend: static (apply, of) => use(() => new BendSpaceMorph(of.Start, of.End, of.Through, Conversions.Unset(of.Angle), of.Straight, of.Symmetric))
                .Bind(morph => Valid(morph.IsValid, nameof(BendSpaceMorph), morph, apply)).Bracket(),
            flow: static (apply, of) => use(() => new FlowSpaceMorph(of.Base, of.Target, of.ReverseBase, of.ReverseTarget, of.PreventStretching))
                .Bind(morph => Valid(morph.IsValid, nameof(FlowSpaceMorph), morph, apply)).Bracket(),
            maelstrom: static (apply, of) => use(() => new MaelstromSpaceMorph(of.Plane, of.Radius0, of.Radius1, of.Angle))
                .Bind(morph => Valid(morph.IsValid, nameof(MaelstromSpaceMorph), morph, apply)).Bracket(),
            splop: static (apply, of) => use(() => new SplopSpaceMorph(of.Plane, of.Surface, of.Parameter, Conversions.Unset(of.Scale), Conversions.Unset(of.Angle)))
                .Bind(morph => Valid(morph.IsValid, nameof(SplopSpaceMorph), morph, apply)).Bracket(),
            sporph: static (apply, of) => use(() => of.Parameters.Match(
                    Some: parameters => new SporphSpaceMorph(of.Source, of.Target, parameters.Source, parameters.Target),
                    None: () => new SporphSpaceMorph(of.Source, of.Target)))
                .Bind(morph => Valid(morph.IsValid, nameof(SporphSpaceMorph), morph, valid =>
                    IO.lift(() => morph.ConstrainNormal = Conversions.Unset(of.ConstrainNormal)).Bind(_ => apply(valid)))).Bracket(),
            stretchToLength: static (apply, of) => use(() => new StretchSpaceMorph(of.Start, of.End, of.Length))
                .Bind(morph => Valid(morph.IsValid, nameof(StretchSpaceMorph), morph, apply)).Bracket(),
            stretchToPoint: static (apply, of) => use(() => new StretchSpaceMorph(of.Start, of.End, of.Point))
                .Bind(morph => Valid(morph.IsValid, nameof(StretchSpaceMorph), morph, apply)).Bracket(),
            taper: static (apply, of) => use(() => new TaperSpaceMorph(of.Start, of.End, of.StartRadius, of.EndRadius, of.Flat, of.Infinite))
                .Bind(morph => Valid(morph.IsValid, nameof(TaperSpaceMorph), morph, apply)).Bracket(),
            twist: static (apply, of) =>
                (from morph in use(static () => new TwistSpaceMorph())
                 from configured in IO.lift(() => DeformMapper.Update(of, morph))
                 from morphed in apply(morph)
                 select morphed).Bracket(),
            cage: static (apply, of) => use(() => new MeshCageMorph(of.Reference, of.Target))
                .Bind(morph => Valid(morph.IsValid, nameof(MeshCageMorph), morph, apply)).Bracket());

    private static IO<A> Valid<A>(bool valid, string member, SpaceMorph morph, Func<SpaceMorph, IO<A>> apply) =>
        IO.lift(Refused.Unless(valid, member)).Bind(_ => apply(morph));

    private static Func<SpaceMorph, IO<A>> Configured<A>(MorphSettings settings, Func<SpaceMorph, IO<A>> apply) =>
        morph => IO.lift(() => DeformMapper.Update(settings, morph)).Bind(_ => apply(morph));

    private static IO<Seq<T>> Morphed<T>(Seq<T> targets, Func<GeometryBase, bool> morph, string member) where T : GeometryBase =>
        from morphable in IO.lift(() => Callbacks.Each(targets, SpaceMorph.IsMorphable, nameof(SpaceMorph.IsMorphable)))
        from morphed in DisposalOps.AcquireAll(
            targets.Map((target, index) => Copies.Edit(target, copy => RefusedElement.Unless(morph(copy), member, index), member)),
            DisposalOps.Release)
        select morphed;

    // --- [FLATTENS]
    public static IO<UnrollResult> Unroll(Brep brep, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        Unrolled(() => new Unroller(brep), options, curves, points, dots);

    public static IO<UnrollResult> Unroll(Surface surface, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        Unrolled(() => new Unroller(surface), options, curves, points, dots);

    public static IO<SquishResult<Brep>> Squish(Surface surface, SquishParameters parameters, Seq<GeometryBase> marks) =>
        Squished(marks, (squisher, flat) => squisher.SquishSurface(parameters, surface, marks, flat), nameof(Squisher.SquishSurface));

    public static IO<SquishResult<Mesh>> Squish(Mesh mesh, SquishParameters parameters, Seq<GeometryBase> marks) =>
        Squished(marks, (squisher, flat) => squisher.SquishMesh(parameters, mesh, marks, flat), nameof(Squisher.SquishMesh));

    public static IO<Seq<(GeometryBase Flat, Option<GeometryBase> Source)>> SquishBack(GeometryBase squished, Seq<GeometryBase> marks) =>
        Copies.AcquireSparse(() => Squisher.SquishBack2dMarks(squished, marks)?.ToArray(), nameof(Squisher.SquishBack2dMarks))
            .Map(back => marks.Zip(back, static (flat, source) => (Flat: flat, Source: source)));

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        from copies in DisposalOps.AcquireAll(meshes.Map(Copies.Duplicate), DisposalOps.Release)
        from unwrapped in DisposalOps.OnFailure(
            use(() => new MeshUnwrapper(copies)).Bind(unwrapper => IO.lift(() => {
                _ = symmetry.Iter(plane => unwrapper.SymmetryPlane = plane);
                return Refused.Unless(unwrapper.Unwrap(method), copies, nameof(MeshUnwrapper.Unwrap));
            })).Bracket(),
            DisposalOps.Release(copies))
        select unwrapped;

    private static IO<UnrollResult> Unrolled(Func<Unroller> create, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        from unroller in IO.lift(() => {
            Unroller created = create();
            DeformMapper.Update(options, created);
            created.AddFollowingGeometry(curves);
            created.AddFollowingGeometry(points);
            created.AddFollowingGeometry(dots);
            return created;
        })
        from unrolled in Copies.Owned(
            IO.lift(() => (
                Flat: unroller.PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots),
                Curves: unrolledCurves,
                Points: unrolledPoints,
                Dots: unrolledDots)),
            made => (
                    Measurements.Valid(toSeq<Brep?>(made.Flat), nameof(Unroller.PerformUnroll)).Bind(static flat => Conversions.NonEmpty(flat, nameof(Unroller.PerformUnroll))).ToValidation(),
                    Measurements.Valid(toSeq<Curve?>(made.Curves), nameof(Unroller.PerformUnroll)).ToValidation(),
                    Measurements.Valid(toSeq<TextDot?>(made.Dots), nameof(Unroller.PerformUnroll)).ToValidation())
                .Apply((flat, flatCurves, flatDots) => new UnrollResult(
                    flat,
                    flatCurves.Map(curve => (Curve: curve, Source: Conversions.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
                    Conversions.Rows(made.Points),
                    flatDots.Map(dot => (Dot: dot, Source: Conversions.Present(unroller.FollowingGeometryIndex(dot)))).Strict()))
                .As()
                .ToFin(),
            static made => DisposalOps.Release(Conversions.Rows<GeometryBase>([.. made.Flat, .. made.Curves, .. made.Dots])))
        select unrolled;

    private static IO<SquishResult<T>> Squished<T>(Seq<GeometryBase> marks, Func<Squisher, List<GeometryBase>, T?> squish, string member) where T : GeometryBase =>
        (from squisher in use(static () => new Squisher())
         from squished in Copies.Owned(
             IO.lift(() => {
                 List<GeometryBase> flatMarks = [];
                 return (
                     Flat: squish(squisher, flatMarks),
                     Marks: flatMarks.ToArray(),
                     Lines: Conversions.Rows(squisher.GetLengthConstrained2dLines()).Zip(Conversions.Rows(squisher.GetLengthConstrained3dLines()), static (flat, source) => (Flat: flat, Source: source)),
                     Triangles: Conversions.Rows(squisher.GetAreaConstrainedTrianglesIndices()));
             }),
             made => Missing.Unless(made.Flat, member).Bind(flat => Measurements.Valid(flat, member)).Map(flat => (Flat: flat, made.Marks, made.Lines, made.Triangles)),
             static made => DisposalOps.Release(Conversions.Rows([made.Flat, .. made.Marks])))
         from flatMarks in DisposalOps.OnFailure(Copies.AcquireSparse(static () => squished.Marks, member), DisposalOps.Release(Seq(squished.Flat)))
         from mesh2d in DisposalOps.OnFailure(Copies.Acquire(squisher.Get2dMesh, nameof(Squisher.Get2dMesh)), DisposalOps.Release([squished.Flat, .. flatMarks.Somes()]))
         from mesh3d in DisposalOps.OnFailure(Copies.Acquire(squisher.Get3dMesh, nameof(Squisher.Get3dMesh)), DisposalOps.Release([squished.Flat, .. flatMarks.Somes(), mesh2d]))
         select new SquishResult<T>(squished.Flat, marks.Zip(flatMarks, static (source, flat) => (Source: source, Flat: flat)), mesh2d, mesh3d, squished.Lines, squished.Triangles)).Bracket();
}
