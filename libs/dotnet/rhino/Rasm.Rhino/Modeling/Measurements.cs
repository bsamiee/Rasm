using System.Collections.Generic;
using System.Globalization;
using Rasm.RhinoCommon.Document.Units;

namespace Rasm.RhinoCommon.Modeling;

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
        validationError = double.IsFinite(density) && density >= 0
            ? null
            : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"density {density}"), RhinoReason.Outside);
}

public sealed record PrincipalMoments(double X, Vector3d XAxis, double Y, Vector3d YAxis, double Z, Vector3d ZAxis) {
    public static Option<PrincipalMoments> Of(PrincipalRead read) =>
        Conversions.Found(read(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis), new PrincipalMoments(x, xAxis, y, yAxis, z, zAxis));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Measurements {
    // --- [PARTS]
    public static uint Checksum<T>(Seq<T> parts) where T : GeometryBase =>
        parts.Fold(0u, static (remainder, part) => part.DataCRC(remainder));

    public static Option<BoundingBox> Bounds(Seq<BoundingBox> boxes) =>
        Conversions.Present(boxes.Fold(BoundingBox.Empty, BoundingBox.Union));

    // --- [MASS]
    public static IO<(Seq<Seq<AreaMassProperties>> Parts, AreaMassProperties Total)> Area(Tolerances tolerances, Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments) =>
        new Measure<AreaMassProperties>(geometry => geometry switch {
            Brep brep => Some(Products.Made(AreaMassProperties.Compute(brep, area: true, firstMoments, secondMoments, productMoments, tolerances.Relative, tolerances.Absolute))),
            Surface surface => Some(Products.Made(AreaMassProperties.Compute(surface, area: true, firstMoments, secondMoments, productMoments))),
            Mesh mesh => Some(Products.Made(AreaMassProperties.Compute(mesh, area: true, firstMoments, secondMoments, productMoments))),
            Hatch hatch => Some(Products.Made(AreaMassProperties.Compute(hatch))),
            Curve curve => Some(Products.Made(AreaMassProperties.Compute(curve, tolerances.Absolute))),
            _ => None,
        }, AreaMassProperties.WeightedSum, None).Of(subjects);

    public static IO<(Seq<Seq<VolumeMassProperties>> Parts, VolumeMassProperties Total)> Volume(Tolerances tolerances, Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments) =>
        new Measure<VolumeMassProperties>(geometry => geometry switch {
            Brep brep => Some(Products.Made(VolumeMassProperties.Compute(brep, volume: true, firstMoments, secondMoments, productMoments, tolerances.Relative, tolerances.Absolute))),
            Surface surface => Some(Products.Made(VolumeMassProperties.Compute(surface, volume: true, firstMoments, secondMoments, productMoments))),
            Mesh mesh => Some(Products.Made(VolumeMassProperties.Compute(mesh, volume: true, firstMoments, secondMoments, productMoments))),
            _ => None,
        }, VolumeMassProperties.WeightedSum,
            Some(fun(static (VolumeMassProperties part, Transform xform) => RhinoFault.Unless(part.Transform(xform)).Map(_ => part)))).Of(subjects);

    public static IO<(Seq<Seq<LengthMassProperties>> Parts, LengthMassProperties Total)> Length(Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments) =>
        new Measure<LengthMassProperties>(geometry => geometry is Curve curve
            ? Some(Products.Made(LengthMassProperties.Compute(curve, length: true, firstMoments, secondMoments, productMoments)))
            : None, LengthMassProperties.WeightedSum, None).Of(subjects);

    private sealed record Measure<T>(
        Func<GeometryBase, Option<IO<T>>> Overload,
        Func<IEnumerable<T>, IEnumerable<double>, T> Weighted,
        Option<Func<T, Transform, Fin<T>>> Moved) where T : class, IDisposable {
        public IO<(Seq<Seq<T>> Parts, T Total)> Of(Seq<MassSubject> subjects) =>
            from parts in subjects.Map(static (subject, index) => (Subject: subject, Index: index)).TraverseM(row => Placed(row.Subject, row.Index)).As()
            from total in Products.Made(Weighted(parts.Bind(static placed => placed), subjects.Zip(parts).Bind(static row => row.Second.Map(_ => row.First.Density))))
            select (parts, total);

        private IO<Seq<T>> Placed(MassSubject subject, int index) =>
            from source in subject.Source.Switch(
                shape: static shape => IO.pure(shape.Geometry),
                limit: static limit => Products.Made<GeometryBase>(limit.Subdivision.ToBrep(limit.Options)))
            from parts in subject.Placements.IsEmpty
                ? Computed(source, index).Map(static part => Seq(part))
                : Moved.Filter(_ => subject.Placements.Exists(Similar)).Match(
                    Some: move => Computed(source, index).Bind(computed => subject.Placements.TraverseM(xform => Similar(xform)
                        ? Products.Made(Weighted([computed], [1.0])).Bind(copy => IO.lift(() => move(copy, xform)))
                        : Recomputed(source, xform, index)).As()),
                    None: () => subject.Placements.TraverseM(xform => Recomputed(source, xform, index)).As())
            select parts;

        private IO<T> Recomputed(GeometryBase source, Transform xform, int index) =>
            from copy in Copies.Duplicate(source)
            from placed in Transformations.Transformed(copy, xform)
            from part in Computed(placed, index)
            select part;

        private IO<T> Computed(GeometryBase geometry, int index) =>
            Overload(geometry).IfNone(() => IO.fail<T>(new RhinoFault(geometry.ObjectType.ToString(), RhinoReason.Mismatched, index)));

        private static bool Similar(Transform xform) => xform.SimilarityType is not TransformSimilarityType.NotSimilarity;
    }
}
