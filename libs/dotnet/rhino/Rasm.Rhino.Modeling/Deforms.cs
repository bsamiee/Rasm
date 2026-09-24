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

    public sealed record Twist(Line Axis, double AngleRadians, bool Infinite) : MorphKind;

    public sealed record Cage(Mesh Reference, Mesh Target) : MorphKind;

    public sealed record Control(NurbsCurve Origin, NurbsCurve Target) : MorphKind;
}

public sealed record MorphSettings(double Tolerance, bool QuickPreview, bool PreserveStructure);

public sealed record UnrollOptions(bool Explode, double ExplodeSpacing, double AbsoluteTolerance, double RelativeTolerance);

public sealed record UnrollResult(Seq<Brep> Flat, Seq<(Curve Curve, Option<int> Source)> Curves, Seq<Point3d> Points, Seq<(TextDot Dot, Option<int> Source)> Dots);

public sealed record SquishResult(GeometryBase Flat, Seq<GeometryBase> Marks, Seq<PolylineCurve> Curves, Seq<TextDot> Dots, Mesh Mesh2d, Mesh Mesh3d);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Deforms {
    // --- [MORPHS]
    public static IO<GeometryBase> Morph(GeometryBase target, MorphKind kind, MorphSettings settings) =>
        from morphable in IO.lift(() => Refused.Unless(SpaceMorph.IsMorphable(target), nameof(SpaceMorph.IsMorphable)))
        from morphed in kind.Switch(
            (Target: target, Settings: settings),
            bend: static (state, of) => Morphed(state, () => of.Angle.Match(
                Some: angle => new BendSpaceMorph(of.Start, of.End, of.Through, angle, of.Straight, of.Symmetric),
                None: () => new BendSpaceMorph(of.Start, of.End, of.Through, of.Straight, of.Symmetric))),
            flow: static (state, of) => Morphed(state, () => new FlowSpaceMorph(of.Base, of.Target, of.ReverseBase, of.ReverseTarget, of.PreventStretching)),
            maelstrom: static (state, of) => Morphed(state, () => new MaelstromSpaceMorph(of.Plane, of.Radius0, of.Radius1, of.Angle)),
            splop: static (state, of) => Morphed(state, () => new SplopSpaceMorph(of.Plane, of.Surface, of.Parameter, of.Scale, of.Angle)),
            sporph: static (state, of) => Disposal.Using(
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
            twist: static (state, of) => Morphed(state, () => new TwistSpaceMorph { TwistAxis = of.Axis, TwistAngleRadians = of.AngleRadians, InfiniteTwist = of.Infinite }),
            cage: static (state, of) => Morphed(state, () => new MeshCageMorph(of.Reference, of.Target)),
            control: static (state, of) => Disposal.Using(
                () => new MorphControl(of.Origin, of.Target),
                control =>
                    from configured in IO.lift(() => DeformMapper.Update(state.Settings, control))
                    from morphed in GeometryOps.EditCopy(state.Target, copy => Refused.Unless(control.Morph(copy), nameof(MorphControl.Morph)))
                    select morphed.Copy))
        select morphed;

    private static IO<GeometryBase> Morphed<TMorph>((GeometryBase Target, MorphSettings Settings) state, Func<TMorph> create) where TMorph : SpaceMorph, IDisposable =>
        Disposal.Using(create, morph => Configured(state, morph));

    private static IO<GeometryBase> Configured((GeometryBase Target, MorphSettings Settings) state, SpaceMorph morph) =>
        from configured in IO.lift(() => DeformMapper.Update(state.Settings, morph))
        from morphed in GeometryOps.EditCopy(state.Target, copy => Refused.Unless(morph.Morph(copy), nameof(SpaceMorph.Morph)))
        select morphed.Copy;

    // --- [FLATTENS]
    public static IO<UnrollResult> Unroll(GeometryBase source, UnrollOptions options, Seq<Curve> curves, Seq<Point3d> points, Seq<TextDot> dots) =>
        from unroller in IO.lift(() => source switch {
            Brep brep => Fin.Succ(new Unroller(brep)),
            Surface surface => Fin.Succ(new Unroller(surface)),
            _ => Fin.Fail<Unroller>(new WrongGeometry(typeof(Brep), source.ObjectType)),
        })
        from configured in IO.lift(() => {
            DeformMapper.Update(options, unroller);
            unroller.AddFollowingGeometry(curves);
            unroller.AddFollowingGeometry(points);
            unroller.AddFollowingGeometry(dots);
        })
        from answer in IO.lift(() => (Flat: unroller.PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots), Curves: unrolledCurves, Points: unrolledPoints, Dots: unrolledDots))
        from unrolled in GeometryOps.OnFailure(
            IO.lift(() =>
                from flat in GeometryResults.Kept(answer.Flat, nameof(Unroller.PerformUnroll))
                from followers in Followers(answer.Curves)
                from labels in Followers(answer.Dots)
                select new UnrollResult(
                    flat,
                    followers.Map(curve => (Curve: curve, Source: Answers.Present(unroller.FollowingGeometryIndex(curve)))).Strict(),
                    toSeq(answer.Points),
                    labels.Map(dot => (Dot: dot, Source: Answers.Present(unroller.FollowingGeometryIndex(dot)))).Strict())),
            Disposal.Release(Seq<GeometryBase?[]>(answer.Flat, answer.Curves, answer.Dots).Bind(static results => Answers.Present(results))))
        select unrolled;

    private static Fin<Seq<T>> Followers<T>(T?[] unrolled) where T : GeometryBase =>
        toSeq(unrolled)
            .Map(static (follower, index) => (Follower: follower, Index: index))
            .TraverseM(static row => Optional(row.Follower).ToFin(new InvalidElement(nameof(Unroller.PerformUnroll), row.Index)))
            .As();

    public static IO<SquishResult> Squish(GeometryBase source, SquishParameters parameters, Seq<GeometryBase> marks, Seq<Curve> curves, Seq<TextDot> dots) =>
        Disposal.Using(
            static () => new Squisher(),
            squisher =>
                from flattened in IO.lift(() => {
                    List<GeometryBase> squishedMarks = [];
                    return (source switch {
                        Surface surface => Missing.Unless<GeometryBase>(squisher.SquishSurface(parameters, surface, marks, squishedMarks), nameof(Squisher.SquishSurface)),
                        Mesh mesh => Missing.Unless<GeometryBase>(squisher.SquishMesh(parameters, mesh, marks, squishedMarks), nameof(Squisher.SquishMesh)),
                        _ => Fin.Fail<GeometryBase>(new WrongGeometry(typeof(Surface), source.ObjectType)),
                    }).Map(flat => (Flat: flat, Marks: toSeq(squishedMarks)));
                })
                from followers in IO.lift(() => curves.TraverseM(curve => Missing.Unless(squisher.SquishCurve(curve), nameof(Squisher.SquishCurve))).As())
                from labels in IO.lift(() => dots.TraverseM(dot => Missing.Unless(squisher.SquishTextDot(dot), nameof(Squisher.SquishTextDot))).As())
                from mesh2d in IO.lift(() => Missing.Unless(squisher.Get2dMesh(), nameof(Squisher.Get2dMesh)))
                from mesh3d in IO.lift(() => Missing.Unless(squisher.Get3dMesh(), nameof(Squisher.Get3dMesh)))
                select new SquishResult(flattened.Flat, flattened.Marks, followers, labels, mesh2d, mesh3d));

    public static IO<Seq<GeometryBase>> SquishBack(GeometryBase pattern, Seq<GeometryBase> marks) =>
        from squished in IO.lift(() => Refused.Unless(Squisher.Is2dPatternSquished(pattern), nameof(Squisher.Is2dPatternSquished)))
        from present in IO.lift(() => Invalid.Unless(!marks.IsEmpty, nameof(marks)))
        from back in IO.lift(() => Missing.Unless(Squisher.SquishBack2dMarks(pattern, marks), nameof(Squisher.SquishBack2dMarks)).Map(static back => toSeq(back).Strict()))
        select back;

    public static IO<Seq<Mesh>> Unwrap(Seq<Mesh> meshes, MeshUnwrapMethod method, Option<Plane> symmetry) =>
        from present in IO.lift(() => Invalid.Unless(!meshes.IsEmpty, nameof(meshes)))
        from handles in Disposal.AcquireAll(meshes.Map(static mesh => DuplicateMode.Duplicate.Acquire(mesh)))
        let copies = handles.Map(static handle => handle.Value)
        from unwrapped in GeometryOps.OnFailure(
            Disposal.Using(
                () => new MeshUnwrapper(copies),
                unwrapper =>
                    from placed in IO.lift(() => symmetry.Iter(plane => unwrapper.SymmetryPlane = plane))
                    from done in IO.lift(() => Refused.Unless(unwrapper.Unwrap(method), nameof(MeshUnwrapper.Unwrap)))
                    select done),
            Disposal.Release(handles))
        select copies;
}

[Mapper]
internal static partial class DeformMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(MorphSettings settings, SpaceMorph morph);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(MorphSettings.Tolerance), nameof(MorphControl.SpaceMorphTolerance))]
    internal static partial void Update(MorphSettings settings, MorphControl control);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(UnrollOptions.Explode), nameof(Unroller.ExplodeOutput))]
    internal static partial void Update(UnrollOptions options, Unroller unroller);
}
