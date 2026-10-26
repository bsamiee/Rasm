using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PointGroup {
    private const double ElementTolerance = 1e-9;

    public static readonly PointGroup T = new(Closed([Turn(new(1d, 1d, 1d), 3), Turn(new(0d, 0d, 1d), 2)]));
    public static readonly PointGroup Td = new(Closed([Reflection(new(1d, -1d, 0d)), Turn(new(1d, 1d, 1d), 3), Turn(new(0d, 0d, 1d), 2)]));
    public static readonly PointGroup Th = new(Closed([Reflection(new(1d, 0d, 0d)), Turn(new(1d, 1d, 1d), 3)]));
    public static readonly PointGroup O = new(Closed([Turn(new(0d, 0d, 1d), 4), Turn(new(1d, 1d, 1d), 3)]));
    public static readonly PointGroup Oh = new(Closed([Reflection(new(1d, 0d, 0d)), Turn(new(1d, 1d, 1d), 3), Reflection(new(1d, -1d, 0d))]));
    public static readonly PointGroup I = new(Closed([Turn(new(1d, 0d, 0d), 2), Turn(new(1d, 1d, 1d), 3), Turn(new(0d, 1d, Constants.GoldenRatio), 5)]));
    public static readonly PointGroup Ih = new(Closed([Reflection(new(1d, 0d, 0d)), Turn(new(1d, 1d, 1d), 3), Turn(new(0d, 1d, Constants.GoldenRatio), 5)]));

    public Seq<Matrix<double>> Elements { get; }

    private static Matrix<double> Turn(Vector3D<double> axis, int order) => Transforms.Affine(Transforms.Rotation(axis, double.Tau / order), default);
    private static Matrix<double> Reflection(Vector3D<double> normal) => Transforms.Affine(Transforms.Mirror(normal), default);

    private static Seq<Matrix<double>> Closed(Seq<Matrix<double>> generators) => Grown(generators, [Matrix<double>.Build.DenseIdentity(4)], [Matrix<double>.Build.DenseIdentity(4)]);

    private static Seq<Matrix<double>> Grown(Seq<Matrix<double>> generators, Seq<Matrix<double>> elements, Seq<Matrix<double>> frontier) =>
        (from element in frontier from generator in generators select generator * element).Fold(elements, static (closed, product) =>
            closed.Exists(known => (known - product).InfinityNorm() <= ElementTolerance) ? closed : closed.Add(product)) switch {
                var closed when closed.Count == elements.Count => elements,
                var closed => Grown(generators, closed, closed.Skip(elements.Count)),
            };
}

[ComplexValueObject]
[ValidationError<SpatialFault>]
public sealed partial class Hull {
    public Seq<Vector3D<double>> Points { get; }
    public Seq<Seq<int>> Facets { get; }

    public static Fin<Hull> Validated(Seq<Vector3D<double>> points, Seq<Seq<int>> facets) => TryCreate(points, facets, out Hull? hull, out SpatialFault? fault) ? hull : fault;
    static partial void ValidateFactoryArguments(ref SpatialFault? validationError, ref Seq<Vector3D<double>> points, ref Seq<Seq<int>> facets) =>
        validationError = points.Count switch {
            var count => facets.Map((facet, index) => (Rule: facet.Count < 3 ? SpatialRule.Count : SpatialRule.Range, Held: facet.Count >= 3 && facet.ForAll(at => at >= 0 && at < count), Index: index))
                .Find(static facet => !facet.Held).Map(static facet => new SpatialFault("hull facet", facet.Rule, facet.Index)).ValueUnsafe(),
        };

    public Fin<Seq<Vector3D<double>>> Poles(Vector3D<double> centre, double tolerance) =>
        Matrix<double>.Build.DenseOfRowArrays(Points.Map(static point => new[] { point.X, point.Y, point.Z })) switch {
            var rows => (Inside: rows.ColumnSums() / rows.RowCount, Centre: Vector<double>.Build.Dense([centre.X, centre.Y, centre.Z])) switch {
                var at => Facets.Map((facet, index) => Pole(Matrix<double>.Build.DenseOfRowVectors(facet.Map(row => rows.Row(row))), at.Inside, at.Centre, tolerance, index)).Traverse(static pole => pole).As(),
            },
        };

    private static Fin<Vector3D<double>> Pole(Matrix<double> facet, Vector<double> inside, Vector<double> centre, double tolerance, int index) =>
        from middle in Fin.Succ(facet.ColumnSums() / facet.RowCount)
        let fit = facet.MapIndexed((_, column, value) => value - middle[column]).Svd(computeVectors: true)
        from planar in SpatialFault.Unless(fit.S[1] > tolerance, SpatialRule.Degenerate, index)
        let normal = fit.VT.Row(2)
        let outward = normal * Math.Sign(normal.DotProduct(middle - inside))
        let distance = outward.DotProduct(middle - centre)
        from clear in SpatialFault.Unless(distance > tolerance, SpatialRule.Range, index)
        let pole = centre + (outward / distance)
        select new Vector3D<double>(pole[0], pole[1], pole[2]);
}
