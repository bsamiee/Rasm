using Rasm.Rhino.Document;
using Rhino.Geometry.Morphs;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MorphKind {
    public sealed record Bend(Point3d Start, Point3d End, Point3d Through, Option<double> Angle, bool Straight, bool Symmetric) : MorphKind;

    public sealed record Flow(Curve Base, Curve Target, bool ReverseBase, bool ReverseTarget, bool PreventStretching) : MorphKind;

    public sealed record Maelstrom(Plane Plane, double Radius0, double Radius1, double Angle) : MorphKind;

    public sealed record Splop(Plane Plane, Surface Surface, Point2d Parameter, double Scale, double Angle) : MorphKind;

    public sealed record Sporph(Surface Source, Surface Target, Option<(Point2d SourceParameter, Point2d TargetParameter)> Parameters, Option<Vector3d> ConstrainNormal) : MorphKind;

    public sealed record StretchToLength(Point3d Start, Point3d End, double Length) : MorphKind;

    public sealed record StretchToPoint(Point3d Start, Point3d End, Point3d Point) : MorphKind;

    public sealed record Taper(Point3d Start, Point3d End, double StartRadius, double EndRadius, bool Flat, bool Infinite) : MorphKind;

    public sealed record Twist(Line TwistAxis, double TwistAngleRadians, bool InfiniteTwist) : MorphKind;

    public sealed record Cage(Mesh Reference, Mesh Target) : MorphKind;

    public sealed record Control(NurbsCurve Origin, NurbsCurve Target) : MorphKind;
}

public sealed record MorphSettings(double Tolerance, bool QuickPreview, bool PreserveStructure);

public sealed record UnrollOptions(bool ExplodeOutput, double ExplodeSpacing, double AbsoluteTolerance, double RelativeTolerance);

public sealed record UnrollResult(Seq<Brep> Flat, Seq<(Curve Curve, Option<int> Source)> Curves, Seq<Point3d> Points, Seq<(TextDot Dot, Option<int> Source)> Dots);

public sealed record SquishResult(GeometryBase Flat, Seq<Option<GeometryBase>> Marks, Seq<Option<PolylineCurve>> Curves, Seq<Option<TextDot>> Dots, Mesh Mesh2d, Mesh Mesh3d);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class DeformMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial TwistSpaceMorph ToMorph(MorphKind.Twist twist);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(MorphSettings settings, SpaceMorph morph);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(MorphSettings.Tolerance), nameof(MorphControl.SpaceMorphTolerance))]
    internal static partial void Update(MorphSettings settings, MorphControl control);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(UnrollOptions options, Unroller unroller);
}

public static class Deforms {
    // --- [MORPHS]
    public static IO<GeometryBase> Morph(GeometryBase target, MorphKind kind, MorphSettings settings) =>
        kind.Switch(
            (Target: target, Settings: settings),
            bend: static (state, of) => Morphed(state, () => of.Angle.Match(
                Some: angle => new BendSpaceMorph(of.Start, of.End, of.Through, angle, of.Straight, of.Symmetric),
                None: () => new BendSpaceMorph(of.Start, of.End, of.Through, of.Straight, of.Symmetric))),
            flow: static (state, of) => Morphed(state, () => new FlowSpaceMorph(of.Base, of.Target, of.ReverseBase, of.ReverseTarget, of.PreventStretching)),
            maelstrom: static (state, of) => Morphed(state, () => new MaelstromSpaceMorph(of.Plane, of.Radius0, of.Radius1, of.Angle)),
            splop: static (state, of) => Morphed(state, () => new SplopSpaceMorph(of.Plane, of.Surface, of.Parameter, of.Scale, of.Angle)),
            sporph: static (state, of) => DisposalOps.Using(
                () => of.Parameters.Match(
                    Some: parameters => new SporphSpaceMorph(of.Source, of.Target, parameters.SourceParameter, parameters.TargetParameter),
                    None: () => new SporphSpaceMorph(of.Source, of.Target)),
                morph =>
                    from constrained in IO.lift(() => morph.ConstrainNormal = of.ConstrainNormal.IfNone(Vector3d.Unset))
                    from sound in IO.lift(() => Refused.Unless(morph.IsValid, nameof(SporphSpaceMorph.IsValid)))
                    from morphed in Configured(state, morph)
                    select morphed),
            stretchToLength: static (state, of) => Morphed(state, () => new StretchSpaceMorph(of.Start, of.End, of.Length)),
            stretchToPoint: static (state, of) => Morphed(state, () => new StretchSpaceMorph(of.Start, of.End, of.Point)),
            taper: static (state, of) => Morphed(state, () => new TaperSpaceMorph(of.Start, of.End, of.StartRadius, of.EndRadius, of.Flat, of.Infinite)),
            twist: static (state, of) => Morphed(state, () => DeformMapper.ToMorph(of)),
            cage: static (state, of) => Morphed(state, () => new MeshCageMorph(of.Reference, of.Target)),
            control: static (state, of) => DisposalOps.Using(
                () => new MorphControl(of.Origin, of.Target),
                control =>
                    from configured in IO.lift(() => DeformMapper.Update(state.Settings, control))
                    from morphed in GeometryResults.EditCopy(state.Target, copy => Refused.Unless(control.Morph(copy), nameof(MorphControl.Morph)))
                    select morphed.Copy));

    private static IO<GeometryBase> Morphed<TMorph>((GeometryBase Target, MorphSettings Settings) state, Func<TMorph> create) where TMorph : SpaceMorph, IDisposable =>
        DisposalOps.Using(create, morph => Configured(state, morph));

    private static IO<GeometryBase> Configured((GeometryBase Target, MorphSettings Settings) state, SpaceMorph morph) =>
        from configured in IO.lift(() => DeformMapper.Update(state.Settings, morph))
        from morphed in GeometryResults.EditCopy(state.Target, copy => Refused.Unless(morph.Morph(copy), nameof(SpaceMorph.Morph)))
        select morphed.Copy;

    // --- [FLATTENS]
    public static IO<UnrollResult> Unroll(GeometryBase source, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        from unroller in IO.lift(() => source switch {
            Brep brep => Fin.Succ(new Unroller(brep)),
            Surface surface => Fin.Succ(new Unroller(surface)),
            _ => Fin.Fail<Unroller>(new WrongType(typeof(Brep), source.GetType())),
        })
        from configured in IO.lift(() => {
            DeformMapper.Update(options, unroller);
            unroller.AddFollowingGeometry(curves);
            unroller.AddFollowingGeometry(points);
            unroller.AddFollowingGeometry(dots);
        })
        from answer in IO.lift(() => (
            Flat: unroller.PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots),
            Curves: Answers.Present(unrolledCurves),
            Points: toSeq(unrolledPoints),
            Dots: Answers.Present(unrolledDots)))
        from unrolled in DisposalOps.OnFailure(
            IO.lift(() => GeometryResults.Kept(answer.Flat, nameof(Unroller.PerformUnroll), emptyFails: false).Map(flat => new UnrollResult(
                flat,
                answer.Curves.Map(curve => (Curve: curve, Source: Answers.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
                answer.Points,
                answer.Dots.Map(dot => (Dot: dot, Source: Answers.Present(unroller.FollowingGeometryIndex(dot)))).Strict()))),
            DisposalOps.Release(Answers.Present<GeometryBase>([.. answer.Flat, .. answer.Curves, .. answer.Dots])))
        select unrolled;

    public static IO<SquishResult> Squish(GeometryBase source, SquishParameters parameters, Seq<GeometryBase> marks, Seq<Curve> curves, Seq<TextDot> dots) =>
        DisposalOps.Using(
            static () => new Squisher(),
            squisher =>
                from flat in IO.lift(() => {
                    List<GeometryBase?> squished = [];
                    return from geometry in source switch {
                        Surface surface => Missing.Unless<GeometryBase>(squisher.SquishSurface(parameters, surface, marks, squished), nameof(Squisher.SquishSurface)),
                        Mesh mesh => Missing.Unless<GeometryBase>(squisher.SquishMesh(parameters, mesh, marks, squished), nameof(Squisher.SquishMesh)),
                        _ => Fin.Fail<GeometryBase>(new WrongType(typeof(Surface), source.GetType())),
                    }
                           select (Geometry: geometry, Marks: toSeq(squished).Map(static mark => Optional(mark)).Strict());
                })
                from meshes in IO.lift(() => (Mesh2d: squisher.Get2dMesh(), Mesh3d: squisher.Get3dMesh()))
                from result in DisposalOps.OnFailure(
                    from present in IO.lift(() => (Missing.Unless(meshes.Mesh2d, nameof(Squisher.Get2dMesh)), Missing.Unless(meshes.Mesh3d, nameof(Squisher.Get3dMesh))).Apply(static (mesh2d, mesh3d) => (Mesh2d: mesh2d, Mesh3d: mesh3d)).As())
                    select new SquishResult(
                        flat.Geometry,
                        flat.Marks,
                        curves.Map(curve => Optional(squisher.SquishCurve(curve))).Strict(),
                        dots.Map(dot => Optional(squisher.SquishTextDot(dot))).Strict(),
                        present.Mesh2d,
                        present.Mesh3d),
                    DisposalOps.Release(Answers.Present([flat.Geometry, .. flat.Marks.Somes(), meshes.Mesh2d, meshes.Mesh3d])))
                select result);

    public static IO<Seq<Option<GeometryBase>>> SquishBack(GeometryBase pattern, Seq<GeometryBase> marks) =>
        IO.lift(() =>
            from valid in (Invalid.Unless(Squisher.Is2dPatternSquished(pattern), nameof(pattern)), Invalid.Unless(!marks.IsEmpty, nameof(marks))).Apply(static (_, _) => unit).As()
            from back in Missing.Unless(Squisher.SquishBack2dMarks(pattern, marks), nameof(Squisher.SquishBack2dMarks))
            select toSeq(back).Map(static mark => Optional(mark)).Strict());

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        from present in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)))
        from copies in DisposalOps.AcquireAll(meshes.Map(GeometryOps.Duplicated))
        from unwrapped in DisposalOps.OnFailure(
            DisposalOps.Using(() => new MeshUnwrapper(copies), unwrapper => IO.lift(() => {
                _ = symmetry.Iter(plane => unwrapper.SymmetryPlane = plane);
                return Refused.Unless(unwrapper.Unwrap(method), nameof(MeshUnwrapper.Unwrap));
            })),
            DisposalOps.Release(copies))
        select copies;
}
