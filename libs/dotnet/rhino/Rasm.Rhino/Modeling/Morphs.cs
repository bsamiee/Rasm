using System.Runtime.CompilerServices;
using Rasm.Spatial;
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

    public sealed record StretchToPoint(Point3d Start, Point3d End, Point3d NewEnd) : MorphKind;

    public sealed record Taper(Point3d Start, Point3d End, double StartRadius, double EndRadius, bool Flat, bool Infinite) : MorphKind;

    public sealed record Twist(Line TwistAxis, double TwistAngleRadians, bool InfiniteTwist) : MorphKind;

    public sealed record Cage(Mesh Reference, Mesh Target) : MorphKind;

    public sealed record Sequence(Seq<MorphKind> Parts) : MorphKind;

    public sealed record Point(Func<Point3d, Point3d> Map) : MorphKind;

    public sealed record Lattice(Box Reference, BezierVolume Volume) : MorphKind;
}

public sealed record MorphSettings(bool QuickPreview, bool PreserveStructure);

internal sealed record MorphStep(Func<GeometryBase, bool> Apply, string Member, bool PreserveStructure, int Index);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class PointSpaceMorph(Func<Point3d, Point3d> map) : SpaceMorph {
    public override Point3d MorphPoint(Point3d point) => map(point);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class MorphMapper {
    internal static partial void Update(MorphSettings settings, SpaceMorph morph, double tolerance);

    internal static partial void Update(MorphSettings settings, MorphControl control, double spaceMorphTolerance);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial TwistSpaceMorph Twist(MorphKind.Twist twist);
}

public static class Morphs {
    public static IO<Seq<T>> Morph<T>(Tolerances tolerances, Seq<T> targets, MorphKind kind, MorphSettings settings, Action<IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(owned => Morphing(kind, Some((Tolerances: tolerances, Settings: settings)), morph =>
            Morphed(targets, morph.Morph, settings.PreserveStructure, owned)), retain);

    public static IO<Seq<T>> Morph<T>(Tolerances tolerances, Seq<T> targets, NurbsCurve origin, NurbsCurve target, MorphSettings settings, Action<IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(owned => DisposalOps.Within(() => new MorphControl(origin, target), control =>
            from valid in IO.lift(() => Refused.Unless(control.IsValid, nameof(MorphControl)))
            from configured in IO.lift(() => MorphMapper.Update(settings, control, tolerances.Absolute))
            from morphed in Morphed(targets, control.Morph, settings.PreserveStructure, owned)
            select morphed), retain);

    public static IO<Seq<Plane>> Morph(Tolerances tolerances, Seq<Plane> planes, MorphKind kind, MorphSettings settings) =>
        Morphing(kind, Some((Tolerances: tolerances, Settings: settings)), morph => IO.lift(() =>
            Callbacks.Each(planes, (plane, index) => Refused.Unless(morph.Morph(ref plane), plane, nameof(SpaceMorph.Morph), index))));

    public static IO<Seq<Point3d>> MorphPoint(Seq<Point3d> points, MorphKind kind) =>
        Morphing(kind, None, morph => IO.lift(() => points.Map(morph.MorphPoint).Strict()));

    private static IO<A> Morphing<A>(MorphKind kind, Option<(Tolerances Tolerances, MorphSettings Settings)> settings, Func<SpaceMorph, IO<A>> apply) =>
        DisposalOps.Within((Action<IDisposable> parts) =>
            from morph in Space(kind, parts)
            from configured in IO.lift(() => settings.Iter(run => MorphMapper.Update(run.Settings, morph, run.Tolerances.Absolute)))
            from applied in apply(morph)
            select applied, DisposalOps.Release);

    private static IO<SpaceMorph> Space(MorphKind kind, Action<IDisposable> parts) =>
        kind.Switch(
            parts,
            bend: static (held, of) => Hosted(() => new BendSpaceMorph(of.Start, of.End, of.Through, Conversions.Unset(of.Angle), of.Straight, of.Symmetric), static morph => morph.IsValid, held),
            flow: static (held, of) => Hosted(() => new FlowSpaceMorph(of.Base, of.Target, of.ReverseBase, of.ReverseTarget, of.PreventStretching), static morph => morph.IsValid, held),
            maelstrom: static (held, of) => Hosted(() => new MaelstromSpaceMorph(of.Plane, of.Radius0, of.Radius1, of.Angle), static morph => morph.IsValid, held),
            splop: static (held, of) => Hosted(() => new SplopSpaceMorph(of.Plane, of.Surface, of.Parameter, Conversions.Unset(of.Scale), Conversions.Unset(of.Angle)), static morph => morph.IsValid, held),
            sporph: static (held, of) => Hosted(
                () => of.Parameters.Case is (Point2d source, Point2d target)
                    ? new SporphSpaceMorph(of.Source, of.Target, source, target)
                    : new SporphSpaceMorph(of.Source, of.Target),
                static morph => morph.IsValid, held, Some<Action<SporphSpaceMorph>>(morph => morph.ConstrainNormal = Conversions.Unset(of.ConstrainNormal))),
            stretchToLength: static (held, of) => Hosted(() => new StretchSpaceMorph(of.Start, of.End, of.Length), static morph => morph.IsValid, held),
            stretchToPoint: static (held, of) => Hosted(() => new StretchSpaceMorph(of.Start, of.End, of.NewEnd), static morph => morph.IsValid, held),
            taper: static (held, of) => Hosted(() => new TaperSpaceMorph(of.Start, of.End, of.StartRadius, of.EndRadius, of.Flat, of.Infinite), static morph => morph.IsValid, held),
            twist: static (held, of) => Hosted(() => MorphMapper.Twist(of), static _ => true, held),
            cage: static (held, of) => Hosted(() => new MeshCageMorph(of.Reference, of.Target), static morph => morph.IsValid, held),
            sequence: static (held, of) => of.Parts.TraverseM(part => Space(part, held)).As()
                .Map(static SpaceMorph (chain) => new PointSpaceMorph(point => chain.Fold(point, static (at, part) => part.MorphPoint(at)))),
            point: static (_, of) => IO.pure<SpaceMorph>(new PointSpaceMorph(of.Map)),
            lattice: static (_, of) => IO.lift(Refused.UnlessValid(of.Reference.IsValid, Transform.PlaneToPlane(of.Reference.Plane, Plane.WorldXY), nameof(Box))).Map(SpaceMorph (local) => new PointSpaceMorph(point => (local * point) switch {
                var at => of.Volume.At(new(of.Reference.X.NormalizedParameterAt(at.X), of.Reference.Y.NormalizedParameterAt(at.Y), of.Reference.Z.NormalizedParameterAt(at.Z))).ToPoint3d(),
            })));

    private static IO<SpaceMorph> Hosted<TMorph>(Func<TMorph> create, Func<TMorph, bool> valid, Action<IDisposable> held, Option<Action<TMorph>> configure = default)
        where TMorph : SpaceMorph, IDisposable =>
        from morph in DisposalOps.Acquire(IO.lift(create), held)
        from accepted in IO.lift(Refused.Unless(valid(morph), typeof(TMorph).Name))
        from constrained in IO.lift(() => configure.Iter(set => set(morph)))
        select (SpaceMorph)morph;

    private static IO<Seq<T>> Morphed<T>(Seq<T> targets, Func<GeometryBase, bool> morph, bool preserveStructure, Action<IDisposable> retain, [CallerArgumentExpression(nameof(morph))] string member = "")
        where T : GeometryBase =>
        from morphable in IO.lift(() => Callbacks.Each(targets, SpaceMorph.IsMorphable, nameof(SpaceMorph.IsMorphable)))
        from morphed in targets.Map(static (target, index) => (target, index))
            .TraverseM(row => Edits.Apply(row.target, Seq(morph), (copy, apply, held) =>
                Deformed(copy, new MorphStep(apply, member, preserveStructure, row.index), held),
                static _ => IO.pure(unit), retain).Map(static edited => edited.Result)).As()
        select morphed;

    private static IO<EditStep<T, int>> Deformed<T>(T copy, MorphStep morph, Action<IDisposable> held) where T : GeometryBase =>
        from subject in copy is Surface surface and not Extrusion && !morph.PreserveStructure
            ? Transformations.Successor<T>(DisposalOps.Within((Action<IDisposable> scratch) =>
                from made in Copies.Acquire(nameof(Surface.ToBrep), _ => Answer.One(surface.ToBrep()), scratch)
                from brep in Transformations.Deformable(made, Some(morph.Index), scratch)
                from moved in Moved(brep, morph)
                from face in Copies.Acquire(nameof(BrepFace.DuplicateSurface), _ => Answer.One<GeometryBase>(brep.Faces[0].DuplicateSurface()), held)
                select face, DisposalOps.Release), Some(morph.Index))
            : Transformations.Deformable(copy, Some(morph.Index), held).Bind(deformable => Moved(deformable, morph).Map(_ => deformable))
        from step in Copies.Acquire(morph.Member, _ => ReferenceEquals(subject, copy)
            ? Edits.Completed<T, int>(copy)
            : Edits.Replaced<T, int>(subject) with { Leaves = [] }, held)
        select step;

    private static IO<Unit> Moved(GeometryBase subject, MorphStep morph) =>
        IO.lift(() => Refused.Unless(morph.Apply(subject), morph.Member, morph.Index));
}
