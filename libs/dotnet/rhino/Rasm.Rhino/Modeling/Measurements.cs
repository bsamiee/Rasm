using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool PrincipalRead(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis);

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MassSource {
    public sealed record Shape(GeometryBase Geometry) : MassSource;
    public sealed record Limit(SubD Subdivision, SubDToBrepOptions Options) : MassSource;
}

[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class MassSubject {
    public MassSource Source { get; }
    public double Density { get; }
    public Seq<Transform> Placements { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref MassSource source, ref double density, ref Seq<Transform> placements) =>
        validationError = double.IsFinite(density) && density >= 0 ? null : new RhinoFault("mass subject needs a finite density at or above 0");
}

public sealed record PrincipalMoments(double X, Vector3d XAxis, double Y, Vector3d YAxis, double Z, Vector3d ZAxis) {
    public static Option<PrincipalMoments> Of(PrincipalRead read) =>
        Callbacks.Found(read(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis), new PrincipalMoments(x, xAxis, y, yAxis, z, zAxis));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Measurements {
    // --- [PARTS]
    public static uint Checksum<T>(Seq<T> parts) where T : GeometryBase =>
        parts.Fold(0u, static (remainder, part) => part.DataCRC(remainder));

    public static Option<BoundingBox> Bounds(Seq<BoundingBox> boxes) =>
        Conversions.Present(boxes.Fold(BoundingBox.Empty, BoundingBox.Union));

    // --- [MASS]
    public static IO<(Seq<Seq<AreaMassProperties>> Parts, AreaMassProperties Total)> Area(Tolerances tolerances, Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments, Action<IDisposable> retain) =>
        new Measure<AreaMassProperties>(nameof(AreaMassProperties.Compute), geometry => geometry switch {
            Brep brep => Some(fun(() => AreaMassProperties.Compute(brep, area: true, firstMoments, secondMoments, productMoments, tolerances.Relative, tolerances.Absolute))),
            Surface surface => Some(fun(() => AreaMassProperties.Compute(surface, area: true, firstMoments, secondMoments, productMoments))),
            Mesh mesh => Some(fun(() => AreaMassProperties.Compute(mesh, area: true, firstMoments, secondMoments, productMoments))),
            Hatch hatch => Some(fun(() => AreaMassProperties.Compute(hatch))),
            Curve curve => Some(fun(() => AreaMassProperties.Compute(curve, tolerances.Absolute))),
            _ => None,
        }, nameof(AreaMassProperties.WeightedSum), AreaMassProperties.WeightedSum, None).Of(subjects, retain);

    public static IO<(Seq<Seq<VolumeMassProperties>> Parts, VolumeMassProperties Total)> Volume(Tolerances tolerances, Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments, Action<IDisposable> retain) =>
        new Measure<VolumeMassProperties>(nameof(VolumeMassProperties.Compute), geometry => geometry switch {
            Brep brep => Some(fun(() => VolumeMassProperties.Compute(brep, volume: true, firstMoments, secondMoments, productMoments, tolerances.Relative, tolerances.Absolute))),
            Surface surface => Some(fun(() => VolumeMassProperties.Compute(surface, volume: true, firstMoments, secondMoments, productMoments))),
            Mesh mesh => Some(fun(() => VolumeMassProperties.Compute(mesh, volume: true, firstMoments, secondMoments, productMoments))),
            _ => None,
        }, nameof(VolumeMassProperties.WeightedSum), VolumeMassProperties.WeightedSum,
            Some(fun(static (VolumeMassProperties part, Transform xform) => Refused.Unless(part.Transform(xform), part, nameof(VolumeMassProperties.Transform))))).Of(subjects, retain);

    public static IO<(Seq<Seq<LengthMassProperties>> Parts, LengthMassProperties Total)> Length(Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments, Action<IDisposable> retain) =>
        new Measure<LengthMassProperties>(nameof(LengthMassProperties.Compute), geometry => geometry is Curve curve
            ? Some(fun(() => LengthMassProperties.Compute(curve, length: true, firstMoments, secondMoments, productMoments)))
            : None, nameof(LengthMassProperties.WeightedSum), LengthMassProperties.WeightedSum, None).Of(subjects, retain);

    private sealed record Measure<T>(
        string Member,
        Func<GeometryBase, Option<Func<T>>> Overload,
        string Sum,
        Func<IEnumerable<T>, IEnumerable<double>, T> Weighted,
        Option<Func<T, Transform, Fin<T>>> Moved) where T : class, IDisposable {
        public IO<(Seq<Seq<T>> Parts, T Total)> Of(Seq<MassSubject> subjects, Action<IDisposable> retain) =>
            DisposalOps.Acquire(owned => DisposalOps.Within<IDisposable, (Seq<Seq<T>> Parts, T Total)>(temporary =>
                from parts in subjects.Map((subject, index) => Placed(subject, index, owned, temporary)).TraverseM(static placed => placed).As()
                from total in Copies.Acquire(Sum, _ => Answer.One(Weighted(
                    parts.Bind(static placed => placed),
                    subjects.Zip(parts).Bind(static row => row.Second.Map(_ => row.First.Density)))), owned)
                select (parts, total), static held => DisposalOps.Release(held)), retain);

        private IO<Seq<T>> Placed(MassSubject subject, int index, Action<IDisposable> owned, Action<IDisposable> temporary) =>
            from source in subject.Source.Switch(temporary,
                shape: static (_, shape) => IO.pure(shape.Geometry),
                limit: static (scope, limit) => Copies.Acquire(nameof(SubD.ToBrep), _ => Answer.One<GeometryBase>(limit.Subdivision.ToBrep(limit.Options)), scope))
            from parts in subject.Placements.IsEmpty
                ? Computed(source, index, owned).Map(static part => Seq(part))
                : Moved.Filter(_ => subject.Placements.Exists(Similar)).Match(
                    Some: move => Computed(source, index, temporary).Bind(computed => subject.Placements.TraverseM(xform => Similar(xform)
                        ? Copies.Acquire(Sum, _ => Answer.One(Weighted([computed], [1.0])), owned).Bind(copy => IO.lift(() => move(copy, xform)))
                        : Recomputed(source, xform, index, owned, temporary)).As()),
                    None: () => subject.Placements.TraverseM(xform => Recomputed(source, xform, index, owned, temporary)).As())
            select parts;

        private IO<T> Recomputed(GeometryBase source, Transform xform, int index, Action<IDisposable> owned, Action<IDisposable> temporary) =>
            from copy in Copies.Duplicate(source, temporary)
            from placed in Transformations.Transformed(copy, xform, temporary)
            from part in Computed(placed, index, owned)
            select part;

        private IO<T> Computed(GeometryBase geometry, int index, Action<IDisposable> retain) =>
            Overload(geometry).Match(
                Some: compute => Copies.Acquire(Member, _ => Answer.One(compute()), retain),
                None: () => IO.fail<T>(Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{geometry.ObjectType} at element {index} has no {Member} overload"))));

        private static bool Similar(Transform xform) => xform.SimilarityType is not TransformSimilarityType.NotSimilarity;
    }
}
