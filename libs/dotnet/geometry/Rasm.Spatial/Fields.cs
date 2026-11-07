using System.Diagnostics.CodeAnalysis;
using System.Numerics.Tensors;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Bounds(Vector3D<double> Min, Vector3D<double> Max) {
    public Bounds Union(Bounds other) => new(Each(Min, other.Min, double.Min), Each(Max, other.Max, double.Max));
    public Bounds Intersection(Bounds other) => new(Each(Min, other.Min, double.Max), Each(Max, other.Max, double.Min));
    public Bounds Inflated(double amount) => new Vector3D<double>(amount, amount, amount) switch { var margin => new(Min - margin, Max + margin) };

    public Bounds Transformed(Matrix<double> map) {
        (Vector3D<double> X, Vector3D<double> Y, Vector3D<double> Z, Vector3D<double> Translation) rows = Transforms.Rows(map);
        ReadOnlySpan<double> xEnds = [Min.X, Max.X];
        ReadOnlySpan<double> yEnds = [Min.Y, Max.Y];
        ReadOnlySpan<double> zEnds = [Min.Z, Max.Z];
        int count = xEnds.Length * yEnds.Length * zEnds.Length;
        Span<double> xCoordinates = stackalloc double[count];
        Span<double> yCoordinates = stackalloc double[count];
        Span<double> zCoordinates = stackalloc double[count];
        int index = 0;
        foreach (double x in xEnds)
            foreach (double y in yEnds)
                foreach (double z in zEnds) {
                    Vector3D<double> corner = rows.Apply(new Vector3D<double>(x, y, z));
                    xCoordinates[index] = corner.X;
                    yCoordinates[index] = corner.Y;
                    zCoordinates[index++] = corner.Z;
                }
        return new(new(TensorPrimitives.Min(xCoordinates), TensorPrimitives.Min(yCoordinates), TensorPrimitives.Min(zCoordinates)), new(TensorPrimitives.Max(xCoordinates), TensorPrimitives.Max(yCoordinates), TensorPrimitives.Max(zCoordinates)));
    }
    private static Vector3D<double> Each(Vector3D<double> a, Vector3D<double> b, Func<double, double, double> pick) => new(pick(a.X, b.X), pick(a.Y, b.Y), pick(a.Z, b.Z));
}

[SmartEnum]
public sealed partial class Combination {
    public static readonly Combination Union = new(double.PositiveInfinity, double.Min, static operands => from found in operands.Traverse(static bounds => bounds).As() from head in found.Head select found.Tail.Fold(head, static (held, next) => held.Union(next)));
    public static readonly Combination Intersection = new(double.NegativeInfinity, double.Max, static operands => operands.Somes() switch { var found => found.Head.Map(head => found.Tail.Fold(head, static (held, next) => held.Intersection(next))) });

    public double Identity { get; }

    [UseDelegateFromConstructor]
    public partial double Meet(double held, double next);

    [UseDelegateFromConstructor]
    public partial Option<Bounds> Bounded(Seq<Option<Bounds>> operands);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ScalarField {

    // --- [EVALUATION]

    public virtual Option<Bounds> Bounds => None;
    public virtual Option<double> Constant => None;
    public abstract double Distance(Vector3D<double> point);

    // --- [LEAVES]

    public sealed record HalfSpace(Vector3D<double> Origin, Vector3D<double> Normal) : ScalarField {
        [SuppressMessage("Performance", "MA0168:Use readonly struct for in or ref readonly parameter", Justification = "HyperJet declares readonly Dot with an in parameter of mutable Vector3D<T>, preserving projection arithmetic.")]
        public override double Distance(Vector3D<double> point) => (point - Origin).Dot(Normal) / Normal.Length();
    }

    public sealed record Ball(Vector3D<double> Center, double Radius) : ScalarField { public override Option<Bounds> Bounds => new Bounds(Center, Center).Inflated(Radius); public override double Distance(Vector3D<double> point) => (point - Center).Length() - Radius; }

    public sealed record Box(Vector3D<double> Min, Vector3D<double> Max) : ScalarField { public override Option<Bounds> Bounds => new Bounds(Min, Max); public override double Distance(Vector3D<double> point) => new Vector3D<double>(double.Max(Min.X - point.X, point.X - Max.X), double.Max(Min.Y - point.Y, point.Y - Max.Y), double.Max(Min.Z - point.Z, point.Z - Max.Z)) switch { var reach => new Vector3D<double>(double.Max(reach.X, 0d), double.Max(reach.Y, 0d), double.Max(reach.Z, 0d)).Length() + double.Min(double.Max(reach.X, double.Max(reach.Y, reach.Z)), 0d) }; }

    public sealed record Measured : ScalarField {
        private readonly Func<Vector3D<double>, double> _sample;
        public Measured(Func<Vector3D<double>, double> sample, Option<Bounds> bounds) => (_sample, Bounds) = (sample, bounds);
        public override Option<Bounds> Bounds { get; }
        public override double Distance(Vector3D<double> point) => _sample(point);
    }

    public sealed record Level(Func<Vector3D<DScalar3<double>>, DScalar3<double>> Field) : ScalarField { public override double Distance(Vector3D<double> point) => DScalar3<double>.Variables(point.X, point.Y, point.Z) switch { var (x, y, z) => Field(new(x, y, z)) switch { var f => f.Value is 0d ? 0d : f.Value / new Vector3D<double>(f.G(0), f.G(1), f.G(2)).Length() } }; }

    public sealed record Uniform(double Value) : ScalarField { public override Option<double> Constant => Value; public override double Distance(Vector3D<double> point) => Value; }

    // --- [COMBINATORS]

    public sealed record Combined(Seq<ScalarField> Operands, Combination Combination) : ScalarField { public override Option<Bounds> Bounds => Combination.Bounded(Operands.Map(static operand => operand.Bounds)); public override double Distance(Vector3D<double> point) => Operands.Fold(Combination.Identity, (met, operand) => Combination.Meet(met, operand.Distance(point))); }

    public sealed record Difference(ScalarField Body, ScalarField Cutter) : ScalarField { public override Option<Bounds> Bounds => Body.Bounds; public override double Distance(Vector3D<double> point) => double.Max(Body.Distance(point), -Cutter.Distance(point)); }

    public sealed record Blend(ScalarField A, ScalarField B, double Radius) : ScalarField { public override Option<Bounds> Bounds => from a in A.Bounds from b in B.Bounds select a.Union(b).Inflated(Radius / 4d); public override double Distance(Vector3D<double> point) => (A.Distance(point), B.Distance(point)) switch { var (a, b) => double.Max(Radius - double.Abs(a - b), 0d) switch { var overlap => double.Min(a, b) - (overlap * overlap / (4d * Radius)) } }; }

    public sealed record Offset(ScalarField Field, ScalarField Amount) : ScalarField { public override Option<Bounds> Bounds => from amount in Amount.Constant from bounds in Field.Bounds select bounds.Inflated(double.Max(amount, 0d)); public override double Distance(Vector3D<double> point) => Field.Distance(point) - Amount.Distance(point); }

    public sealed record Shell(ScalarField Field, ScalarField Thickness) : ScalarField { public override Option<Bounds> Bounds => from thickness in Thickness.Constant from bounds in Field.Bounds select bounds.Inflated(double.Max(thickness / 2d, 0d)); public override double Distance(Vector3D<double> point) => double.Abs(Field.Distance(point)) - (Thickness.Distance(point) / 2d); }

    public sealed record Moved : ScalarField {
        private readonly Matrix<double> map;
        private Moved(ScalarField field, Matrix<double> map, Matrix<double> inverse) => (Field, this.map, Inverse) = (field, map, Transforms.Rows(inverse));
        public ScalarField Field { get; }
        public Matrix<double> Map => map.Clone();
        public override Option<Bounds> Bounds => Field.Bounds.Map(bounds => bounds.Transformed(map));
        private (Vector3D<double> X, Vector3D<double> Y, Vector3D<double> Z, Vector3D<double> Translation) Inverse { get; }
        public override double Distance(Vector3D<double> point) => Field.Distance(Inverse.Apply(point));

        public static Fin<Moved> Of(ScalarField field, Matrix<double> map) =>
            from shape in SpatialFault.Unless(map.RowCount is 4 && map.ColumnCount == map.RowCount, SpatialRule.Shape, (map.RowCount, map.ColumnCount))
            let owned = map.Clone()
            let factor = owned.Svd()
            let rank = factor.Rank
            from nonsingular in SpatialFault.Unless(rank == owned.RowCount, SpatialRule.Singular, rank)
            select new Moved(field, owned, factor.Solve(Matrix<double>.Build.DenseIdentity(owned.RowCount)));
    }
}
