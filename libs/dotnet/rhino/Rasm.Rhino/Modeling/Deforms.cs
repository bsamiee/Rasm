using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rhino.Geometry.Morphs;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
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

    [MapProperty(nameof(@UnrollOptions.Explode.IsSome), nameof(Unroller.ExplodeOutput))]
    [MapProperty(nameof(UnrollOptions.Explode), nameof(Unroller.ExplodeSpacing))]
    [MapProperty(nameof(@UnrollOptions.Tolerances.Absolute), nameof(Unroller.AbsoluteTolerance))]
    [MapProperty(nameof(@UnrollOptions.Tolerances.Relative), nameof(Unroller.RelativeTolerance))]
    internal static partial void Update(UnrollOptions options, Unroller unroller);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapPropertyFromSource(nameof(SporphSpaceMorph.ConstrainNormal))]
    internal static partial void Normal(Vector3d normal, SporphSpaceMorph morph);

    [MapPropertyFromSource(nameof(MeshUnwrapper.SymmetryPlane))]
    internal static partial void Symmetry(Plane plane, MeshUnwrapper unwrapper);
}

public static class Deforms {
    // --- [MORPHS]
    public static IO<Seq<T>> Morph<T>(Seq<T> targets, MorphKind kind, MorphSettings settings) where T : GeometryBase =>
        Morphing(kind, Some(settings), morph => Morphed(targets, morph.Morph, nameof(SpaceMorph.Morph)));

    public static IO<Seq<T>> Morph<T>(Seq<T> targets, NurbsCurve origin, NurbsCurve target, MorphSettings settings) where T : GeometryBase =>
        (from control in use(() => new MorphControl(origin, target))
         from valid in IO.lift(Refused.Unless(control.IsValid, nameof(MorphControl)))
         from configured in IO.lift(() => DeformMapper.Update(settings, control))
         from morphed in Morphed(targets, control.Morph, nameof(MorphControl.Morph))
         select morphed).Bracket();

    public static IO<Seq<Plane>> Morph(Seq<Plane> planes, MorphKind kind, MorphSettings settings) =>
        Morphing(kind, Some(settings), morph => IO.lift(() =>
            Callbacks.Each(planes, (plane, index) => RefusedElement.Unless(morph.Morph(ref plane), plane, nameof(SpaceMorph.Morph), index))));

    public static IO<Seq<Point3d>> MorphPoint(Seq<Point3d> points, MorphKind kind) =>
        Morphing(kind, None, morph => IO.lift(() => points.Map(morph.MorphPoint).Strict()));

    private static IO<A> Morphing<A>(MorphKind kind, Option<MorphSettings> settings, Func<SpaceMorph, IO<A>> apply) =>
        kind.Switch(
            (Settings: settings, Apply: apply),
            bend: static (state, of) => Morphing(() => new BendSpaceMorph(of.Start, of.End, of.Through, Conversions.Unset(of.Angle), of.Straight, of.Symmetric),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(BendSpaceMorph))), state),
            flow: static (state, of) => Morphing(() => new FlowSpaceMorph(of.Base, of.Target, of.ReverseBase, of.ReverseTarget, of.PreventStretching),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(FlowSpaceMorph))), state),
            maelstrom: static (state, of) => Morphing(() => new MaelstromSpaceMorph(of.Plane, of.Radius0, of.Radius1, of.Angle),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(MaelstromSpaceMorph))), state),
            splop: static (state, of) => Morphing(() => new SplopSpaceMorph(of.Plane, of.Surface, of.Parameter, Conversions.Unset(of.Scale), Conversions.Unset(of.Angle)),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(SplopSpaceMorph))), state),
            sporph: static (state, of) => Morphing(() => of.Parameters.Case is (Point2d source, Point2d target)
                    ? new SporphSpaceMorph(of.Source, of.Target, source, target)
                    : new SporphSpaceMorph(of.Source, of.Target),
                morph => IO.lift(Refused.Unless(morph.IsValid, nameof(SporphSpaceMorph)))
                    >> IO.lift(() => DeformMapper.Normal(Conversions.Unset(of.ConstrainNormal), morph)), state),
            stretchToLength: static (state, of) => Morphing(() => new StretchSpaceMorph(of.Start, of.End, of.Length),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(StretchSpaceMorph))), state),
            stretchToPoint: static (state, of) => Morphing(() => new StretchSpaceMorph(of.Start, of.End, of.Point),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(StretchSpaceMorph))), state),
            taper: static (state, of) => Morphing(() => new TaperSpaceMorph(of.Start, of.End, of.StartRadius, of.EndRadius, of.Flat, of.Infinite),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(TaperSpaceMorph))), state),
            twist: static (state, of) => Morphing(static () => new TwistSpaceMorph(), morph => IO.lift(() => DeformMapper.Update(of, morph)), state),
            cage: static (state, of) => Morphing(() => new MeshCageMorph(of.Reference, of.Target),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(MeshCageMorph))), state));

    private static IO<A> Morphing<TMorph, A>(Func<TMorph> create, Func<TMorph, IO<Unit>> prepare, (Option<MorphSettings> Settings, Func<SpaceMorph, IO<A>> Apply) state)
        where TMorph : SpaceMorph, IDisposable =>
        (from morph in use(create)
         from prepared in prepare(morph)
         from configured in IO.lift(() => state.Settings.Iter(settings => DeformMapper.Update(settings, morph)))
         from applied in state.Apply(morph)
         select applied).Bracket();

    private static IO<Seq<T>> Morphed<T>(Seq<T> targets, Func<GeometryBase, bool> morph, string member) where T : GeometryBase =>
        from morphable in IO.lift(() => Callbacks.Each(targets, SpaceMorph.IsMorphable, nameof(SpaceMorph.IsMorphable)))
        from morphed in DisposalOps.AcquireAll(
            targets.Map((target, index) => Copies.Edit(target, copy => RefusedElement.Unless(morph(copy), member, index), member)),
            DisposalOps.Release)
        select morphed;

    // --- [FLATTENS]
    public static IO<Seq<(GeometryBase Flat, Option<GeometryBase> Source)>> SquishBack(GeometryBase squished, Seq<GeometryBase> marks) =>
        Copies.AcquireSparse(() => Squisher.SquishBack2dMarks(squished, marks)?.ToArray(), nameof(Squisher.SquishBack2dMarks))
            .Map(back => marks.Zip(back, static (flat, source) => (Flat: flat, Source: source)));

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        from copies in DisposalOps.AcquireAll(meshes.Map(Copies.Duplicate), DisposalOps.Release)
        from unwrapped in DisposalOps.OnFailure(
            (from unwrapper in use(() => new MeshUnwrapper(copies))
             from configured in IO.lift(() => symmetry.Iter(plane => DeformMapper.Symmetry(plane, unwrapper)))
             from applied in IO.lift(() => Refused.Unless(unwrapper.Unwrap(method), copies, nameof(MeshUnwrapper.Unwrap)))
             select applied).Bracket(),
            DisposalOps.Release(copies))
        select unwrapped;

    public static IO<UnrollResult> Unroll(Func<Unroller> create, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
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
            static made => (
                    Measurements.Valid(toSeq<Brep?>(made.Flat), nameof(Unroller.PerformUnroll)).Bind(static flat => Conversions.NonEmpty(flat, nameof(Unroller.PerformUnroll))).ToValidation(),
                    Measurements.Valid(toSeq<Curve?>(made.Curves), nameof(Unroller.PerformUnroll)).ToValidation(),
                    Measurements.Valid(toSeq<TextDot?>(made.Dots), nameof(Unroller.PerformUnroll)).ToValidation())
                .Apply((flat, flatCurves, flatDots) => (Flat: flat, Curves: flatCurves, Points: Conversions.Rows(made.Points), Dots: flatDots))
                .As()
                .ToFin(),
            static made => DisposalOps.Release(Conversions.Rows<GeometryBase>([.. made.Flat, .. made.Curves, .. made.Dots])))
        select new UnrollResult(
            unrolled.Flat,
            unrolled.Curves.Map(curve => (Curve: curve, Source: Conversions.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
            unrolled.Points,
            unrolled.Dots.Map(dot => (Dot: dot, Source: Conversions.Present(unroller.FollowingGeometryIndex(dot)))).Strict());

    public static IO<SquishResult<T>> Squish<T>(Seq<GeometryBase> marks, Func<Squisher, Seq<GeometryBase>, List<GeometryBase>, T?> squish, string member) where T : GeometryBase =>
        (from squisher in use(static () => new Squisher())
         from squished in Copies.Owned(
             IO.lift(() => {
                 List<GeometryBase> flatMarks = [];
                 return (Flat: squish(squisher, marks, flatMarks), Marks: flatMarks.ToArray());
             }),
             made => (made.Flat is { } product ? Measurements.Valid(product, member) : Fin.Fail<T>(new Missing(member)))
                 .Map(flat => (Flat: flat, made.Marks)),
             static made => DisposalOps.Release(Conversions.Rows([made.Flat, .. made.Marks])))
         from flatMarks in DisposalOps.OnFailure(Copies.AcquireSparse(() => squished.Marks, member), DisposalOps.Release(Seq<GeometryBase>(squished.Flat)))
         from mesh2d in DisposalOps.OnFailure(Copies.Acquire(squisher.Get2dMesh, nameof(Squisher.Get2dMesh)), DisposalOps.Release([squished.Flat, .. flatMarks.Somes()]))
         from mesh3d in DisposalOps.OnFailure(Copies.Acquire(squisher.Get3dMesh, nameof(Squisher.Get3dMesh)), DisposalOps.Release([squished.Flat, .. flatMarks.Somes(), mesh2d]))
         select new SquishResult<T>(
             squished.Flat, marks.Zip(flatMarks, static (source, flat) => (Source: source, Flat: flat)), mesh2d, mesh3d,
             Conversions.Rows(squisher.GetLengthConstrained2dLines()).Zip(Conversions.Rows(squisher.GetLengthConstrained3dLines()), static (flat, source) => (Flat: flat, Source: source)),
             Conversions.Rows(squisher.GetAreaConstrainedTrianglesIndices()))).Bracket();
}
