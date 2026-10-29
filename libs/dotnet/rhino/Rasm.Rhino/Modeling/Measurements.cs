using System.Collections.Generic;
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

public sealed record MassSubject {
    private MassSubject(MassSource source, double density, Seq<Transform> placements) => (Source, Density, Placements) = (source, density, placements);

    public MassSource Source { get; }

    public double Density { get; }

    public Seq<Transform> Placements { get; }

    public static Fin<MassSubject> Validated(MassSource source, double density, Seq<Transform> placements) =>
        RhinoFault.Unless(double.IsFinite(density) && density >= 0d, RhinoReason.Outside, density).Map(_ => new MassSubject(source, density, placements));
}

public sealed record PrincipalMoments(double X, Vector3d XAxis, double Y, Vector3d YAxis, double Z, Vector3d ZAxis) {
    public static Option<PrincipalMoments> Of(PrincipalRead read) =>
        Conversions.Present(read(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis), new PrincipalMoments(x, xAxis, y, yAxis, z, zAxis));
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
            Brep brep => Some(Products.Of(() => AreaMassProperties.Compute(brep, area: true, firstMoments, secondMoments, productMoments, tolerances.Relative, tolerances.Absolute))),
            Surface surface => Some(Products.Of(() => AreaMassProperties.Compute(surface, area: true, firstMoments, secondMoments, productMoments))),
            Mesh mesh => Some(Products.Of(() => AreaMassProperties.Compute(mesh, area: true, firstMoments, secondMoments, productMoments))),
            Hatch hatch => Some(Products.Of(() => AreaMassProperties.Compute(hatch))),
            Curve curve => Some(Products.Of(() => AreaMassProperties.Compute(curve, tolerances.Absolute))),
            _ => None,
        }, AreaMassProperties.WeightedSum, None).Of(subjects);

    public static IO<(Seq<Seq<VolumeMassProperties>> Parts, VolumeMassProperties Total)> Volume(Tolerances tolerances, Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments) =>
        new Measure<VolumeMassProperties>(geometry => geometry switch {
            Brep brep => Some(Products.Of(() => VolumeMassProperties.Compute(brep, volume: true, firstMoments, secondMoments, productMoments, tolerances.Relative, tolerances.Absolute))),
            Surface surface => Some(Products.Of(() => VolumeMassProperties.Compute(surface, volume: true, firstMoments, secondMoments, productMoments))),
            Mesh mesh => Some(Products.Of(() => VolumeMassProperties.Compute(mesh, volume: true, firstMoments, secondMoments, productMoments))),
            _ => None,
        }, VolumeMassProperties.WeightedSum,
            Some(fun(static (VolumeMassProperties part, Transform xform) => RhinoFault.Unless(part.Transform(xform), RhinoReason.Declined, xform).Map(_ => part)))).Of(subjects);

    public static IO<(Seq<Seq<LengthMassProperties>> Parts, LengthMassProperties Total)> Length(Seq<MassSubject> subjects, bool firstMoments, bool secondMoments, bool productMoments) =>
        new Measure<LengthMassProperties>(geometry => geometry is Curve curve
            ? Some(Products.Of(() => LengthMassProperties.Compute(curve, length: true, firstMoments, secondMoments, productMoments)))
            : None, LengthMassProperties.WeightedSum, None).Of(subjects);

    private sealed record Measure<T>(
        Func<GeometryBase, Option<IO<T>>> Overload,
        Func<IEnumerable<T>, IEnumerable<double>, T> Weighted,
        Option<Func<T, Transform, Fin<T>>> Moved) where T : class, IDisposable {
        public IO<(Seq<Seq<T>> Parts, T Total)> Of(Seq<MassSubject> subjects) =>
            from parts in subjects.Map(static (subject, index) => (Subject: subject, Index: index)).TraverseM(row => Placed(row.Subject, row.Index)).As()
            from total in Products.Of(() => Weighted(parts.Bind(static placed => placed), from row in subjects.Zip(parts) from _ in row.Second select row.First.Density))
            select (parts, total);

        private IO<Seq<T>> Placed(MassSubject subject, int index) =>
            from source in subject.Source.Switch(
                shape: static shape => IO.pure(shape.Geometry),
                limit: static limit => Products.Of<GeometryBase>(() => limit.Subdivision.ToBrep(limit.Options)))
            from parts in subject.Placements.IsEmpty
                ? Computed(source, index).Map(static part => Seq(part))
                : Moved.Filter(_ => subject.Placements.Exists(Similar)).Match(
                    Some: move => Computed(source, index).Bind(computed => subject.Placements.TraverseM(xform => Similar(xform)
                        ? Products.Of(() => Weighted([computed], [1.0])).Bind(copy => IO.lift(() => move(copy, xform)))
                        : Recomputed(source, xform, index)).As()),
                    None: () => subject.Placements.TraverseM(xform => Recomputed(source, xform, index)).As())
            select parts;

        private IO<T> Recomputed(GeometryBase source, Transform xform, int index) =>
            from copy in Products.Of(source.Duplicate)
            from placed in Transformations.Transformed(copy, xform)
            from part in Computed(placed, index)
            select part;

        private IO<T> Computed(GeometryBase geometry, int index) =>
            Overload(geometry).IfNone(() => IO.fail<T>(new RhinoFault(geometry.ObjectType.ToString(), RhinoReason.Mismatched, index)));

        private static bool Similar(Transform xform) => xform.SimilarityType is not TransformSimilarityType.NotSimilarity;
    }
}
