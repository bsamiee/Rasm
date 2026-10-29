using System.Runtime.CompilerServices;
using Rasm.Spatial;
using Rasm.Rhino.Document.Units;
using Rhino.Geometry.Morphs;

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

    public sealed record Space(SpaceMorph Morph) : MorphKind;

    public sealed record Cage(Mesh Reference, Mesh Target) : MorphKind;

    public sealed record Curves(NurbsCurve Origin, NurbsCurve Target) : MorphKind;

    public sealed record Control(MorphControl Source) : MorphKind;

    public sealed record Sequence(Seq<MorphKind> Parts) : MorphKind;

    public sealed record Point(Func<Point3d, Point3d> Map) : MorphKind;

    public sealed record Lattice(Box Reference, BezierVolume Volume) : MorphKind;

    public sealed record Along(Rail Source, Rail Target, Spacing Sampling) : MorphKind {
        public Placement Placement { get; init; } = new();
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class PointSpaceMorph(Func<Point3d, Point3d> map) : SpaceMorph {
    public override Point3d MorphPoint(Point3d point) => map(point);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Morphs {
    // --- [GEOMETRY]
    public static IO<Seq<T>> Morph<T>(Tolerances tolerances, Seq<T> targets, MorphKind kind, bool preserveStructure, bool quickPreview) where T : GeometryBase =>
        Morpher(kind, tolerances, preserveStructure, quickPreview).Bind(morpher => targets.Map(static (target, index) => (Target: target, Index: index))
            .TraverseM(row => Morphed(row.Target, row.Index, morpher.Apply, preserveStructure)).As());

    public static IO<Seq<Plane>> Morph(Tolerances tolerances, Seq<Plane> planes, MorphKind kind, bool quickPreview) =>
        (from morpher in Morpher(kind, tolerances, preserveStructure: false, quickPreview)
         from space in IO.lift(() => morpher.Space.ToFin(new RhinoFault(kind.GetType().Name, RhinoReason.Unsupported)))
         from morphed in IO.lift(() => Products.Each(planes, (plane, index) => RhinoFault.Unless(space.Morph(ref plane), RhinoReason.Declined, plane, index).Map(_ => plane)))
         select morphed).Bracket();

    public static IO<Seq<Point3d>> MorphPoint(Tolerances tolerances, Seq<Point3d> points, MorphKind kind) =>
        Space(kind, tolerances).Bind(space => IO.lift(() => points.Map(space.MorphPoint).Strict())).Bracket();

    private static IO<T> Morphed<T>(T target, int index, Func<GeometryBase, bool> apply, bool preserveStructure) where T : GeometryBase =>
        target is Surface surface and not Extrusion && !preserveStructure
            ? Transformations.Successor<T>(
                from brep in Products.Of(surface.ToBrep)
                from moved in Moved(brep, index, apply)
                from face in Products.Of<GeometryBase>(brep.Faces[0].DuplicateSurface)
                select face)
            : from copy in Products.Of(() => target.Duplicate() as T)
              from deformable in Transformations.Deformable(copy, Some(index))
              from moved in Moved(deformable, index, apply)
              select deformable;

    private static IO<Unit> Moved(GeometryBase subject, int index, Func<GeometryBase, bool> apply) =>
        IO.lift(() => RhinoFault.Unless(apply(subject), RhinoReason.Declined, subject.ObjectType, index));

    // --- [MORPHERS]
    private static IO<(Func<GeometryBase, bool> Apply, Option<SpaceMorph> Space)> Morpher(MorphKind kind, Tolerances tolerances, bool preserveStructure, bool quickPreview) =>
        from morph in Made(kind, tolerances)
        from configured in morph.Match(
            Left: control => Products.Changed(() => control.SpaceMorphTolerance, value => control.SpaceMorphTolerance = value, tolerances.Absolute)
                >> Products.Changed(() => control.QuickPreview, value => control.QuickPreview = value, quickPreview)
                >> Products.Changed(() => control.PreserveStructure, value => control.PreserveStructure = value, preserveStructure),
            Right: space => Products.Changed(() => space.Tolerance, value => space.Tolerance = value, tolerances.Absolute)
                >> Products.Changed(() => space.QuickPreview, value => space.QuickPreview = value, quickPreview)
                >> Products.Changed(() => space.PreserveStructure, value => space.PreserveStructure = value, preserveStructure))
        select (Apply: morph.Match(Left: static Func<GeometryBase, bool> (control) => control.Morph, Right: static Func<GeometryBase, bool> (space) => space.Morph), Space: morph.ToOption());

    private static IO<SpaceMorph> Space(MorphKind kind, Tolerances tolerances) =>
        Made(kind, tolerances).Bind(morph => morph.Match(
            Left: _ => IO.fail<SpaceMorph>(new RhinoFault(kind.GetType().Name, RhinoReason.Unsupported)),
            Right: IO.pure));

    private static IO<Either<MorphControl, SpaceMorph>> Made(MorphKind kind, Tolerances tolerances) =>
        kind.Switch(
            tolerances,
            bend: static (_, of) => Hosted(() => new BendSpaceMorph(of.Start, of.End, of.Through, Conversions.Unset(of.Angle), of.Straight, of.Symmetric), static morph => morph.IsValid),
            flow: static (_, of) => Hosted(() => new FlowSpaceMorph(of.Base, of.Target, of.ReverseBase, of.ReverseTarget, of.PreventStretching), static morph => morph.IsValid),
            maelstrom: static (_, of) => Hosted(() => new MaelstromSpaceMorph(of.Plane, of.Radius0, of.Radius1, of.Angle), static morph => morph.IsValid),
            splop: static (_, of) => Hosted(() => new SplopSpaceMorph(of.Plane, of.Surface, of.Parameter, Conversions.Unset(of.Scale), Conversions.Unset(of.Angle)), static morph => morph.IsValid),
            sporph: static (_, of) => Hosted(
                () => of.Parameters.Case is (Point2d source, Point2d target)
                    ? new SporphSpaceMorph(of.Source, of.Target, source, target) { ConstrainNormal = Conversions.Unset(of.ConstrainNormal) }
                    : new SporphSpaceMorph(of.Source, of.Target) { ConstrainNormal = Conversions.Unset(of.ConstrainNormal) },
                static morph => morph.IsValid),
            stretchToLength: static (_, of) => Hosted(() => new StretchSpaceMorph(of.Start, of.End, of.Length), static morph => morph.IsValid),
            stretchToPoint: static (_, of) => Hosted(() => new StretchSpaceMorph(of.Start, of.End, of.NewEnd), static morph => morph.IsValid),
            taper: static (_, of) => Hosted(() => new TaperSpaceMorph(of.Start, of.End, of.StartRadius, of.EndRadius, of.Flat, of.Infinite), static morph => morph.IsValid),
            space: static (_, of) => IO.pure(Right<MorphControl, SpaceMorph>(of.Morph)),
            cage: static (_, of) => Hosted(() => new MeshCageMorph(of.Reference, of.Target), static morph => morph.IsValid),
            curves: static (_, of) =>
                from control in use(() => new MorphControl(of.Origin, of.Target))
                from valid in Products.Accepted(() => control.IsValid)
                select Left<MorphControl, SpaceMorph>(control),
            control: static (_, of) => Products.Accepted(() => of.Source.IsValid).Map(_ => Left<MorphControl, SpaceMorph>(of.Source)),
            sequence: static (held, of) => of.Parts.TraverseM(part => Space(part, held)).As()
                .Map(static chain => Right<MorphControl, SpaceMorph>(new PointSpaceMorph(point => chain.Fold(point, static (at, part) => part.MorphPoint(at))))),
            point: static (_, of) => IO.pure(Right<MorphControl, SpaceMorph>(new PointSpaceMorph(of.Map))),
            lattice: static (_, of) =>
                from valid in Products.Accepted(() => of.Reference.IsValid)
                let local = Transform.PlaneToPlane(of.Reference.Plane, Plane.WorldXY)
                select Right<MorphControl, SpaceMorph>(new PointSpaceMorph(point => (local * point) switch {
                    var at => of.Volume.At(new(of.Reference.X.NormalizedParameterAt(at.X), of.Reference.Y.NormalizedParameterAt(at.Y), of.Reference.Z.NormalizedParameterAt(at.Z))).ToPoint3d(),
                })),
            along: static (held, of) =>
                from source in IO.lift(() => new Frames.Along(of.Source, of.Sampling).Stations(held))
                from target in IO.lift(() => new Frames.Along(of.Target, new Spacing.Lengths(source.Map(static station => station.Normalized))).Stations(held))
                from paired in IO.lift(() => RhinoFault.Unless(source.Count >= 2 && source.Count == target.Count, RhinoReason.Mismatched, (source.Count, target.Count)))
                let parameters = source.Map(static station => station.Parameter).ToArray()
                select Right<MorphControl, SpaceMorph>(new PointSpaceMorph(point => Flowed(of, parameters, source, target, point))));

    private static Point3d Flowed(MorphKind.Along of, double[] parameters, Seq<Station> source, Seq<Station> target, Point3d point) =>
        of.Source.Path.ClosestPoint(point, out double t) && of.Source.Path.GetLength() is > 0d and var length
            ? (Station: System.Array.BinarySearch(parameters, t) switch { >= 0 and var exact => exact, var insert => Math.Max(~insert - 1, 0) }, Normalized: of.Source.Path.GetLength(new Interval(of.Source.Path.Domain.T0, t)) / length) switch {
                var at => of.Target.Path.NormalizedLengthParameter(at.Normalized, out double onTarget)
                    ? of.Placement.At(Transported(of.Source.Path, source[at.Station].Frame, t), Transported(of.Target.Path, target[at.Station].Frame, onTarget), at.Normalized) * point
                    : point,
            }
            : point;

    private static Plane Transported(Curve path, Plane frame, double t) =>
        path.TangentAt(t) switch {
            var tangent => (frame.XAxis - (frame.XAxis * tangent * tangent)) switch {
                var side => new Plane(path.PointAt(t), side, Vector3d.CrossProduct(tangent, side)),
            },
        };

    private static IO<Either<MorphControl, SpaceMorph>> Hosted<TMorph>(Func<TMorph> create, Func<TMorph, bool> valid, [CallerArgumentExpression(nameof(create))] string call = "")
        where TMorph : SpaceMorph, IDisposable =>
        from morph in use(create)
        from accepted in Products.Accepted(() => valid(morph), call)
        select Right<MorphControl, SpaceMorph>(morph);
}
