using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Bounds(Vector3D<double> Min, Vector3D<double> Max) {
    public Bounds Union(Bounds other) => new(Each(Min, other.Min, Math.Min), Each(Max, other.Max, Math.Max));
    public Bounds Intersection(Bounds other) => new(Each(Min, other.Min, Math.Max), Each(Max, other.Max, Math.Min));
    public Bounds Inflated(double amount) => new Vector3D<double>(amount, amount, amount) switch { var margin => new(Min - margin, Max + margin) };

    public Bounds Transformed(Matrix<double> map) =>
        Transforms.Applied(map) switch { var applied => (from x in Seq(Min.X, Max.X) from y in Seq(Min.Y, Max.Y) from z in Seq(Min.Z, Max.Z) select applied(new Vector3D<double>(x, y, z))).Map(static corner => new Bounds(corner, corner)).Reduce(static (held, corner) => held.Union(corner)) };
    private static Vector3D<double> Each(Vector3D<double> a, Vector3D<double> b, Func<double, double, double> pick) => new(pick(a.X, b.X), pick(a.Y, b.Y), pick(a.Z, b.Z));
}

[SmartEnum]
public sealed partial class Combination {
    public static readonly Combination Union = new(Math.Min, double.PositiveInfinity, static operands =>
        from found in operands.Traverse(static bounds => bounds).As() from head in found.Head select found.Tail.Fold(head, static (held, next) => held.Union(next)));

    public static readonly Combination Intersection = new(Math.Max, double.NegativeInfinity, static operands =>
        operands.Somes() switch { var found => found.Head.Map(head => found.Tail.Fold(head, static (held, next) => held.Intersection(next))) });

    public Func<double, double, double> Meet { get; }
    public double Identity { get; }
    public Func<Seq<Option<Bounds>>, Option<Bounds>> Bounded { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ScalarField {
    // --- [EVALUATION]
    private ScalarField(Func<Vector3D<double>, double> distance, Option<Bounds> bounds) => (Distance, Bounds) = (distance, bounds);
    public Func<Vector3D<double>, double> Distance { get; }
    public Option<Bounds> Bounds { get; }
    public virtual Option<double> Constant => None;

    // --- [LEAVES]
    public sealed record HalfSpace(Vector3D<double> Origin, Vector3D<double> Normal) : ScalarField(
        Normal.Normalize() switch { var direction => Vector256.Create(direction.X, direction.Y, direction.Z, 0d) switch { var row => point => (point - Origin) switch { var at => Vector256.Dot(row, Vector256.Create(at.X, at.Y, at.Z, 0d)) } } }, None);

    public sealed record Ball(Vector3D<double> Center, double Radius) : ScalarField(point => (point - Center).Length() - Radius, new Bounds(Center, Center).Inflated(Radius));

    public sealed record Box(Vector3D<double> Min, Vector3D<double> Max) : ScalarField(
        point => new Vector3D<double>(Math.Max(Min.X - point.X, point.X - Max.X), Math.Max(Min.Y - point.Y, point.Y - Max.Y), Math.Max(Min.Z - point.Z, point.Z - Max.Z)) switch {
            var reach => new Vector3D<double>(Math.Max(reach.X, 0d), Math.Max(reach.Y, 0d), Math.Max(reach.Z, 0d)).Length() + Math.Min(Math.Max(reach.X, Math.Max(reach.Y, reach.Z)), 0d),
        }, new Bounds(Min, Max));

    public sealed record Measured : ScalarField {
        public Measured(Func<Vector3D<double>, double> distance, Option<Bounds> bounds) : base(distance, bounds) { }
    }

    public sealed record Level(Func<Vector3D<DScalar3<double>>, DScalar3<double>> Field) : ScalarField(
        point => DScalar3<double>.Variables(point.X, point.Y, point.Z) switch { var (x, y, z) => Field(new(x, y, z)) switch { var f => f.Value is 0d ? 0d : f.Value / new Vector3D<double>(f.G(0), f.G(1), f.G(2)).Length() } }, None);

    public sealed record Uniform(double Value) : ScalarField(_ => Value, None) {
        public override Option<double> Constant => Value;
    }

    // --- [COMBINATORS]
    public sealed record Combined(Seq<ScalarField> Operands, Combination Combination) : ScalarField(
        point => Operands.Fold(Combination.Identity, (met, operand) => Combination.Meet(met, operand.Distance(point))), Combination.Bounded(Operands.Map(static operand => operand.Bounds)));

    public sealed record Difference(ScalarField Body, ScalarField Cutter) : ScalarField(point => Math.Max(Body.Distance(point), -Cutter.Distance(point)), Body.Bounds);

    public sealed record Blend(ScalarField A, ScalarField B, double Radius) : ScalarField(
        point => (A.Distance(point), B.Distance(point)) switch { var (a, b) => Math.Max(Radius - Math.Abs(a - b), 0d) switch { var overlap => Math.Min(a, b) - (overlap * overlap / (4d * Radius)) } },
        from a in A.Bounds from b in B.Bounds select a.Union(b).Inflated(Radius / 4d));

    public sealed record Offset(ScalarField Field, ScalarField Amount) : ScalarField(point => Field.Distance(point) - Amount.Distance(point), from amount in Amount.Constant from bounds in Field.Bounds select bounds.Inflated(Math.Max(amount, 0d)));

    public sealed record Shell(ScalarField Field, ScalarField Thickness) : ScalarField(point => Math.Abs(Field.Distance(point)) - (Thickness.Distance(point) / 2d), from thickness in Thickness.Constant from bounds in Field.Bounds select bounds.Inflated(Math.Max(thickness / 2d, 0d)));

    public sealed record Moved : ScalarField {
        private Moved(ScalarField field, Matrix<double> map, Func<Vector3D<double>, Vector3D<double>> inverse)
            : base(point => field.Distance(inverse(point)), field.Bounds.Map(bounds => bounds.Transformed(map))) => (Field, Map) = (field, map);
        public ScalarField Field { get; }
        public Matrix<double> Map { get; }

        public static Fin<Moved> Of(ScalarField field, Matrix<double> map) => SpatialFault.Unless(map.Rank() is 4, SpatialRule.Singular).Map(_ => new Moved(field, map, Transforms.Applied(map.Inverse())));
    }
}
