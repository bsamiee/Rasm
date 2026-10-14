using Rasm.Rhino.Document.Units;
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
internal static partial class MorphMapper {
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

public static class Morphs {
    // --- [MORPHS]
    public static IO<Seq<T>> Morph<T>(Seq<T> targets, MorphKind kind, MorphSettings settings, Action<IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(owned => Morphing(kind, Some(settings), morph => Morphed(targets, morph.Morph, nameof(SpaceMorph.Morph), owned)), (_, release) => retain(release));

    public static IO<Seq<T>> Morph<T>(Seq<T> targets, NurbsCurve origin, NurbsCurve target, MorphSettings settings, Action<IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(owned => DisposalOps.Within(
            () => new MorphControl(origin, target),
            control =>
                from valid in IO.lift(() => Refused.Unless(control.IsValid, nameof(MorphControl)))
                from configured in IO.lift(() => MorphMapper.Update(settings, control))
                from morphed in Morphed(targets, control.Morph, nameof(MorphControl.Morph), owned)
                select morphed,
            static control => IO.lift(control.Dispose)), (_, release) => retain(release));

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
                    >> IO.lift(() => MorphMapper.Normal(Conversions.Unset(of.ConstrainNormal), morph)), state),
            stretchToLength: static (state, of) => Morphing(() => new StretchSpaceMorph(of.Start, of.End, of.Length),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(StretchSpaceMorph))), state),
            stretchToPoint: static (state, of) => Morphing(() => new StretchSpaceMorph(of.Start, of.End, of.Point),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(StretchSpaceMorph))), state),
            taper: static (state, of) => Morphing(() => new TaperSpaceMorph(of.Start, of.End, of.StartRadius, of.EndRadius, of.Flat, of.Infinite),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(TaperSpaceMorph))), state),
            twist: static (state, of) => Morphing(static () => new TwistSpaceMorph(), morph => IO.lift(() => MorphMapper.Update(of, morph)), state),
            cage: static (state, of) => Morphing(() => new MeshCageMorph(of.Reference, of.Target),
                static morph => IO.lift(Refused.Unless(morph.IsValid, nameof(MeshCageMorph))), state));

    private static IO<A> Morphing<TMorph, A>(Func<TMorph> create, Func<TMorph, IO<Unit>> prepare, (Option<MorphSettings> Settings, Func<SpaceMorph, IO<A>> Apply) state)
        where TMorph : SpaceMorph, IDisposable =>
        DisposalOps.Within(create, morph =>
            from prepared in prepare(morph)
            from configured in IO.lift(() => state.Settings.Iter(settings => MorphMapper.Update(settings, morph)))
            from applied in state.Apply(morph)
            select applied,
            static morph => IO.lift(morph.Dispose));

    private static IO<Seq<T>> Morphed<T>(Seq<T> targets, Func<GeometryBase, bool> morph, string member, Action<IDisposable> retain) where T : GeometryBase =>
        from morphable in IO.lift(() => Callbacks.Each(targets, SpaceMorph.IsMorphable, nameof(SpaceMorph.IsMorphable)))
        from morphed in targets.Map(static (target, index) => (target, index))
            .TraverseM(row => Copies.Edit(row.target, copy => RefusedElement.Unless(morph(copy), member, row.index), member, retain)).As()
        select morphed;

    // --- [FLATTENS]
    public static IO<Seq<(GeometryBase Flat, Option<GeometryBase> Source)>> SquishBack(GeometryBase squished, Seq<GeometryBase> marks, Action<IDisposable> retain) =>
        Copies.AcquireSparse(() => Squisher.SquishBack2dMarks(squished, marks)?.ToArray(), nameof(Squisher.SquishBack2dMarks), retain)
            .Map(back => marks.Zip(back, static (flat, source) => (Flat: flat, Source: source)));

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from copies in meshes.TraverseM(mesh => Copies.Duplicate(mesh, owned)).As()
            from unwrapped in Unwrapped(copies, method, symmetry)
            select unwrapped, (_, release) => retain(release));

    public static IO<UnrollResult> Unroll(Func<Unroller> create, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots, Action<IDisposable> retain) =>
        from unroller in IO.lift(() => {
            Unroller created = create();
            MorphMapper.Update(options, created);
            created.AddFollowingGeometry(curves);
            created.AddFollowingGeometry(points);
            created.AddFollowingGeometry(dots);
            return created;
        })
        from unrolled in Copies.Owned(
            () => (
                Flat: unroller.PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots),
                Curves: unrolledCurves,
                Points: unrolledPoints,
                Dots: unrolledDots),
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
            static made => DisposalOps.Release(Conversions.Rows<GeometryBase>([.. made.Flat, .. made.Curves, .. made.Dots])), retain)
        select unrolled;

    public static IO<SquishResult<T>> Squish<T>(Seq<GeometryBase> marks, Func<Squisher, Seq<GeometryBase>, List<GeometryBase>, T?> squish, string member, Action<IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(owned => DisposalOps.Within(
            static () => new Squisher(),
            squisher => Squished(squisher, marks, squish, member, owned),
            static squisher => IO.lift(squisher.Dispose)), (_, release) => retain(release));

    private static IO<Seq<Mesh>> Unwrapped(Seq<Mesh> copies, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        DisposalOps.Within(
            () => new MeshUnwrapper(copies),
            unwrapper =>
                from configured in IO.lift(() => symmetry.Iter(plane => MorphMapper.Symmetry(plane, unwrapper)))
                from applied in IO.lift(() => Refused.Unless(unwrapper.Unwrap(method), copies, nameof(MeshUnwrapper.Unwrap)))
                select applied,
            static unwrapper => IO.lift(unwrapper.Dispose));

    private static IO<SquishResult<T>> Squished<T>(Squisher squisher, Seq<GeometryBase> marks, Func<Squisher, Seq<GeometryBase>, List<GeometryBase>, T?> squish, string member, Action<IDisposable> owned)
        where T : GeometryBase =>
        from squished in Copies.Owned(
            static () => new List<GeometryBase>(),
            flatMarks =>
                from flat in Copies.Acquire(() => squish(squisher, marks, flatMarks), member, owned)
                from validMarks in IO.lift(() => Measurements.Valid(toSeq(flatMarks).Map(static mark => Optional(mark)), member))
                select (Flat: flat, Marks: validMarks),
            static flatMarks => DisposalOps.Release(Conversions.Rows(flatMarks)), owned)
        from mesh2d in Copies.Acquire(squisher.Get2dMesh, nameof(Squisher.Get2dMesh), owned)
        from mesh3d in Copies.Acquire(squisher.Get3dMesh, nameof(Squisher.Get3dMesh), owned)
        select new SquishResult<T>(
            squished.Flat, marks.Zip(squished.Marks, static (source, flat) => (Source: source, Flat: flat)), mesh2d, mesh3d,
            Conversions.Rows(squisher.GetLengthConstrained2dLines()).Zip(Conversions.Rows(squisher.GetLengthConstrained3dLines()), static (flat, source) => (Flat: flat, Source: source)),
            Conversions.Rows(squisher.GetAreaConstrainedTrianglesIndices()));
}
