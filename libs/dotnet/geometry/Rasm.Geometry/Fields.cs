namespace Rasm.Geometry;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Bounds {
    private Bounds(Vector3D<double> min, Vector3D<double> max) => (Min, Max) = (min, max);
    public Vector3D<double> Min { get; }
    public Vector3D<double> Max { get; }

    public Bounds Union(Bounds other) => new(Each(Min, other.Min, double.Min), Each(Max, other.Max, double.Max));
    public Option<Bounds> Intersection(Bounds other) => Validated(Each(Min, other.Min, double.Max), Each(Max, other.Max, double.Min)).ToOption();
    public Fin<Bounds> Inflated(double amount) => new Vector3D<double>(amount, amount, amount) switch { var margin => Validated(Min - margin, Max + margin) };

    public Fin<Bounds> Transformed(AffineMap map) =>
        (from x in Seq(Min.X, Max.X) from y in Seq(Min.Y, Max.Y) from z in Seq(Min.Z, Max.Z) select map.Apply(new Vector3D<double>(x, y, z))) switch {
            var corners => Validated(corners.Fold(corners[0], static (low, corner) => Each(low, corner, double.Min)), corners.Fold(corners[0], static (high, corner) => Each(high, corner, double.Max))),
        };

    public static Fin<Bounds> Validated(Vector3D<double> min, Vector3D<double> max) =>
        (GeometryFault.Unless(min.IsFinite && max.IsFinite, GeometryRule.Range, (min, max)), GeometryFault.Unless(min.X <= max.X && min.Y <= max.Y && min.Z <= max.Z, GeometryRule.Order, (min, max)))
            .Apply((_, _) => new Bounds(min, max)).As();

    private static Vector3D<double> Each(Vector3D<double> a, Vector3D<double> b, Func<double, double, double> pick) => new(pick(a.X, b.X), pick(a.Y, b.Y), pick(a.Z, b.Z));
}

[SmartEnum]
public sealed partial class Combination {
    public static readonly Combination Union = new(double.PositiveInfinity, double.Min, static operands => from found in operands.Traverse(static bounds => bounds).As() from head in found.Head select found.Tail.Fold(head, static (held, next) => held.Union(next)));
    public static readonly Combination Intersection = new(double.NegativeInfinity, double.Max, static operands => operands.Somes() switch { var found => found.Tail.Fold(found.Head, static (held, next) => held.Bind(bounds => bounds.Intersection(next))) });

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

    public sealed record HalfSpace : ScalarField {
        private HalfSpace(Vector3D<double> origin, Vector3D<double> normal) => (Origin, Normal) = (origin, normal);
        public Vector3D<double> Origin { get; }
        public Vector3D<double> Normal { get; }
        public override double Distance(Vector3D<double> point) => (point - Origin).Dot(Normal);

        public static Fin<HalfSpace> Validated(Vector3D<double> origin, Vector3D<double> normal) =>
            (GeometryFault.Unless(origin.IsFinite, GeometryRule.Range, origin), GeometryFault.Unless(normal.Length() is var length && double.IsFinite(length) && length > 0d, GeometryRule.Degenerate, normal))
                .Apply((_, _) => new HalfSpace(origin, normal.Normalize())).As();
    }

    public sealed record Ball : ScalarField {
        private Ball(Vector3D<double> center, double radius) => (Center, Radius) = (center, radius);
        public Vector3D<double> Center { get; }
        public double Radius { get; }
        public override Option<Bounds> Bounds => new Vector3D<double>(Radius, Radius, Radius) switch { var reach => Geometry.Bounds.Validated(Center - reach, Center + reach).ToOption() };
        public override double Distance(Vector3D<double> point) => (point - Center).Length() - Radius;

        public static Fin<Ball> Validated(Vector3D<double> center, double radius) =>
            (GeometryFault.Unless(center.IsFinite, GeometryRule.Range, center), GeometryFault.Unless(double.IsFinite(radius) && radius > 0d, GeometryRule.Range, radius))
                .Apply((_, _) => new Ball(center, radius)).As();
    }

    public sealed record Box(Bounds Extent) : ScalarField {
        public override Option<Bounds> Bounds => Extent;
        public override double Distance(Vector3D<double> point) =>
            new Vector3D<double>(double.Max(Extent.Min.X - point.X, point.X - Extent.Max.X), double.Max(Extent.Min.Y - point.Y, point.Y - Extent.Max.Y), double.Max(Extent.Min.Z - point.Z, point.Z - Extent.Max.Z)) switch {
                var reach => new Vector3D<double>(double.Max(reach.X, 0d), double.Max(reach.Y, 0d), double.Max(reach.Z, 0d)).Length() + double.Min(double.Max(reach.X, double.Max(reach.Y, reach.Z)), 0d),
            };
    }

    public sealed record Measured : ScalarField {
        private readonly Func<Vector3D<double>, double> sample;
        public Measured(Func<Vector3D<double>, double> sample, Option<Bounds> bounds) => (this.sample, Bounds) = (sample, bounds);
        public override Option<Bounds> Bounds { get; }
        public override double Distance(Vector3D<double> point) => sample(point);
    }

    public sealed record Level(Func<Vector3D<DScalar3<double>>, DScalar3<double>> Field) : ScalarField { public override double Distance(Vector3D<double> point) => DScalar3<double>.Variables(point.X, point.Y, point.Z) switch { var (x, y, z) => Field(new(x, y, z)) switch { var f => f.Value is 0d ? 0d : f.Value / new Vector3D<double>(f.G(0), f.G(1), f.G(2)).Length() } }; }

    public sealed record Uniform : ScalarField {
        private Uniform(double value) => Value = value;
        public double Value { get; }
        public override Option<double> Constant => Value;
        public override double Distance(Vector3D<double> point) => Value;

        public static Fin<Uniform> Validated(double value) => GeometryFault.Unless(double.IsFinite(value), GeometryRule.Range, value).Map(_ => new Uniform(value));
    }

    // --- [COMBINATORS]

    public sealed record Combined(Seq<ScalarField> Operands, Combination Combination) : ScalarField { public override Option<Bounds> Bounds => Combination.Bounded(Operands.Map(static operand => operand.Bounds)); public override double Distance(Vector3D<double> point) => Operands.Fold(Combination.Identity, (met, operand) => Combination.Meet(met, operand.Distance(point))); }

    public sealed record Difference(ScalarField Body, ScalarField Cutter) : ScalarField { public override Option<Bounds> Bounds => Body.Bounds; public override double Distance(Vector3D<double> point) => double.Max(Body.Distance(point), -Cutter.Distance(point)); }

    public sealed record Blend : ScalarField {
        private Blend(ScalarField a, ScalarField b, double radius) => (A, B, Radius) = (a, b, radius);
        public ScalarField A { get; }
        public ScalarField B { get; }
        public double Radius { get; }
        public override Option<Bounds> Bounds => from a in A.Bounds from b in B.Bounds from inflated in a.Union(b).Inflated(Radius / 4d).ToOption() select inflated;
        public override double Distance(Vector3D<double> point) => (A.Distance(point), B.Distance(point)) switch { var (a, b) => double.Max(Radius - double.Abs(a - b), 0d) switch { var overlap => double.Min(a, b) - (overlap * overlap / (4d * Radius)) } };

        public static Fin<Blend> Validated(ScalarField a, ScalarField b, double radius) => GeometryFault.Unless(double.IsFinite(radius) && radius > 0d, GeometryRule.Range, radius).Map(_ => new Blend(a, b, radius));
    }

    public sealed record Offset(ScalarField Field, ScalarField Amount) : ScalarField { public override Option<Bounds> Bounds => from amount in Amount.Constant from bounds in Field.Bounds from inflated in bounds.Inflated(double.Max(amount, 0d)).ToOption() select inflated; public override double Distance(Vector3D<double> point) => Field.Distance(point) - Amount.Distance(point); }

    public sealed record Shell(ScalarField Field, ScalarField Thickness) : ScalarField { public override Option<Bounds> Bounds => from thickness in Thickness.Constant from bounds in Field.Bounds from inflated in bounds.Inflated(double.Max(thickness / 2d, 0d)).ToOption() select inflated; public override double Distance(Vector3D<double> point) => double.Abs(Field.Distance(point)) - (Thickness.Distance(point) / 2d); }

    public sealed record Moved : ScalarField {
        private Moved(ScalarField field, AffineMap map, double scale) => (Field, Map, Scale, Inverse) = (field, map, scale, map.Inverse());
        public ScalarField Field { get; }
        public AffineMap Map { get; }
        private double Scale { get; }
        private AffineMap Inverse { get; }
        public override Option<Bounds> Bounds => Field.Bounds.Bind(bounds => bounds.Transformed(Map).ToOption());
        public override double Distance(Vector3D<double> point) => Scale * Field.Distance(Inverse.Apply(point));

        public static Fin<Moved> Of(ScalarField field, AffineMap map, double tolerance) =>
            map.UniformScale(tolerance).ToFin(new GeometryFault(map, GeometryRule.Distortion)).Map(scale => new Moved(field, map, scale));
    }
}
