using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Bounds(Vector3D<double> Min, Vector3D<double> Max) {
    public Bounds Union(Bounds other) => new(Each(Min, other.Min, double.Min), Each(Max, other.Max, double.Max));
    public Bounds Intersection(Bounds other) => new(Each(Min, other.Min, double.Max), Each(Max, other.Max, double.Min));
    public Bounds Inflated(double amount) => new Vector3D<double>(amount, amount, amount) switch { var margin => new(Min - margin, Max + margin) };

    public Bounds Transformed(Matrix<double> map) =>
        Transforms.Lanes(map) switch { var rows => (from x in Seq(Min.X, Max.X) from y in Seq(Min.Y, Max.Y) from z in Seq(Min.Z, Max.Z) select rows.Apply(new Vector3D<double>(x, y, z))).Map(static corner => new Bounds(corner, corner)).Reduce(static (held, corner) => held.Union(corner)) };
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
    private ScalarField(Option<Bounds> bounds) => Bounds = bounds;
    public Option<Bounds> Bounds { get; }
    public virtual Option<double> Constant => None;
    public abstract double Distance(Vector3D<double> point);

    // --- [LEAVES]
    public sealed record HalfSpace(Vector3D<double> Origin, Vector3D<double> Normal) : ScalarField(Option<Bounds>.None) { public override double Distance(Vector3D<double> point) => (point - Origin) switch { var at => ((at.X * Normal.X) + (at.Y * Normal.Y) + (at.Z * Normal.Z)) / Normal.Length() }; }

    public sealed record Ball(Vector3D<double> Center, double Radius) : ScalarField(new Bounds(Center, Center).Inflated(Radius)) { public override double Distance(Vector3D<double> point) => (point - Center).Length() - Radius; }

    public sealed record Box(Vector3D<double> Min, Vector3D<double> Max) : ScalarField(new Bounds(Min, Max)) { public override double Distance(Vector3D<double> point) => new Vector3D<double>(double.Max(Min.X - point.X, point.X - Max.X), double.Max(Min.Y - point.Y, point.Y - Max.Y), double.Max(Min.Z - point.Z, point.Z - Max.Z)) switch { var reach => new Vector3D<double>(double.Max(reach.X, 0d), double.Max(reach.Y, 0d), double.Max(reach.Z, 0d)).Length() + double.Min(double.Max(reach.X, double.Max(reach.Y, reach.Z)), 0d) }; }

    public sealed record Measured : ScalarField {
        private readonly Func<Vector3D<double>, double> _sample;
        public Measured(Func<Vector3D<double>, double> sample, Option<Bounds> bounds) : base(bounds) => _sample = sample;
        public override double Distance(Vector3D<double> point) => _sample(point);
    }

    public sealed record Level(Func<Vector3D<DScalar3<double>>, DScalar3<double>> Field) : ScalarField(Option<Bounds>.None) { public override double Distance(Vector3D<double> point) => DScalar3<double>.Variables(point.X, point.Y, point.Z) switch { var (x, y, z) => Field(new(x, y, z)) switch { var f => f.Value is 0d ? 0d : f.Value / new Vector3D<double>(f.G(0), f.G(1), f.G(2)).Length() } }; }

    public sealed record Uniform(double Value) : ScalarField(Option<Bounds>.None) { public override Option<double> Constant => Value; public override double Distance(Vector3D<double> point) => Value; }

    // --- [COMBINATORS]
    public sealed record Combined(Seq<ScalarField> Operands, Combination Combination) : ScalarField(Combination.Bounded(Operands.Map(static operand => operand.Bounds))) { public override double Distance(Vector3D<double> point) => Operands.Fold(Combination.Identity, (met, operand) => Combination.Meet(met, operand.Distance(point))); }

    public sealed record Difference(ScalarField Body, ScalarField Cutter) : ScalarField(Body.Bounds) { public override double Distance(Vector3D<double> point) => double.Max(Body.Distance(point), -Cutter.Distance(point)); }

    public sealed record Blend(ScalarField A, ScalarField B, double Radius) : ScalarField(from a in A.Bounds from b in B.Bounds select a.Union(b).Inflated(Radius / 4d)) { public override double Distance(Vector3D<double> point) => (A.Distance(point), B.Distance(point)) switch { var (a, b) => double.Max(Radius - double.Abs(a - b), 0d) switch { var overlap => double.Min(a, b) - (overlap * overlap / (4d * Radius)) } }; }

    public sealed record Offset(ScalarField Field, ScalarField Amount) : ScalarField(from amount in Amount.Constant from bounds in Field.Bounds select bounds.Inflated(double.Max(amount, 0d))) { public override double Distance(Vector3D<double> point) => Field.Distance(point) - Amount.Distance(point); }

    public sealed record Shell(ScalarField Field, ScalarField Thickness) : ScalarField(from thickness in Thickness.Constant from bounds in Field.Bounds select bounds.Inflated(double.Max(thickness / 2d, 0d))) { public override double Distance(Vector3D<double> point) => double.Abs(Field.Distance(point)) - (Thickness.Distance(point) / 2d); }

    public sealed record Moved : ScalarField {
        private Moved(ScalarField field, Matrix<double> map) : base(field.Bounds.Map(bounds => bounds.Transformed(map))) => (Field, Map, Inverse) = (field, map, Transforms.Lanes(map.Inverse()));
        public ScalarField Field { get; }
        public Matrix<double> Map { get; }
        private (Vector256<double> X, Vector256<double> Y, Vector256<double> Z) Inverse { get; }
        public override double Distance(Vector3D<double> point) => Field.Distance(Inverse.Apply(point));

        public static Fin<Moved> Of(ScalarField field, Matrix<double> map) => SpatialFault.Unless(map.Rank() is 4, SpatialRule.Singular, map.Rank()).Map(_ => new Moved(field, map));
    }
}
