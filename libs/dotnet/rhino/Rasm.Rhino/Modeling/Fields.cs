using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct BlendRadius : System.Numerics.IMinMaxValue<BlendRadius> {
    public static BlendRadius MinValue { get; } = new(double.BitIncrement(0d));
    public static BlendRadius MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct IsosurfaceResolution : System.Numerics.IMinMaxValue<IsosurfaceResolution> {
    public static IsosurfaceResolution MinValue { get; } = new(1);
    public static IsosurfaceResolution MaxValue { get; } = new(int.MaxValue - 2);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RootFindingSteps : System.Numerics.IMinMaxValue<RootFindingSteps> {
    public static RootFindingSteps MinValue { get; } = new(1);
    public static RootFindingSteps MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ScalarField {
    // --- [EVALUATION]
    public Func<Point3d, double> Evaluator(Tolerances tolerances) => Switch(
        tolerances,
        halfSpace: static (_, halfSpace) => halfSpace.Plane.DistanceTo,
        ball: static (_, ball) => ball.Sphere switch { var sphere => point => sphere.Center.DistanceTo(point) - sphere.Radius },
        solid: static (_, solid) => solid.Box switch { var box => point => (box.Contains(point) ? -1d : 1d) * point.DistanceTo(box.ClosestPoint(point, includeInterior: false)) },
        distance: static (_, distance) => Unsigned(distance.Source),
        signed: static (tolerances, signed) => (Boundary(signed.Region), signed.Region) switch {
            var (boundary, region) => point => region.Contains(tolerances, point) switch {
                PointContainment.Inside => -boundary(point),
                PointContainment.Coincident => 0d,
                PointContainment.Outside => boundary(point),
                PointContainment.Unset => double.NaN,
            },
        },
        level: static (_, level) => (level.Value, level.Gradient) switch { var (value, gradient) => point => value(point) switch { 0d => 0d, var at => at / gradient(point).Length } },
        uniform: static (_, uniform) => Constant(uniform.Value),
        union: static (tolerances, union) => union.Operands.Map(operand => operand.Evaluator(tolerances))
            .Fold(Constant(double.PositiveInfinity), static (met, next) => point => Math.Min(met(point), next(point))),
        intersection: static (tolerances, intersection) => intersection.Operands.Map(operand => operand.Evaluator(tolerances))
            .Fold(Constant(double.NegativeInfinity), static (met, next) => point => Math.Max(met(point), next(point))),
        difference: static (tolerances, difference) => (difference.Body.Evaluator(tolerances), difference.Cutter.Evaluator(tolerances)) switch { var (body, cutter) => point => Math.Max(body(point), -cutter(point)) },
        blend: static (tolerances, blend) => (blend.A.Evaluator(tolerances), blend.B.Evaluator(tolerances), (double)blend.Radius) switch { var (a, b, radius) => point => Smoothed(a(point), b(point), radius) },
        offset: static (tolerances, offset) => (offset.Field.Evaluator(tolerances), offset.Amount.Evaluator(tolerances)) switch { var (body, amount) => point => body(point) - amount(point) },
        shell: static (tolerances, shell) => (shell.Field.Evaluator(tolerances), shell.Thickness.Evaluator(tolerances)) switch { var (body, thickness) => point => Math.Abs(body(point)) - (thickness(point) / 2d) },
        moved: static (tolerances, moved) => (moved.Field.Evaluator(tolerances), moved.Inverse) switch { var (body, inverse) => point => body(inverse * point) });

    public Option<BoundingBox> Bounds => Switch(
        halfSpace: static _ => None,
        ball: static ball => ball.Sphere.BoundingBox,
        solid: static solid => solid.Box.BoundingBox,
        distance: static distance => distance.Source.Switch(
            curve: static curve => (GeometryBase)curve.Geometry,
            surface: static surface => surface.Geometry,
            face: static face => face.Geometry,
            brep: static brep => brep.Geometry,
            mesh: static mesh => mesh.Geometry,
            subD: static subd => subd.Geometry,
            cloud: static cloud => cloud.Geometry).GetBoundingBox(accurate: true),
        signed: static signed => signed.Region.Switch(
            solid: static solid => Some(solid.Brep.GetBoundingBox(accurate: true)),
            closed: static closed => Some(closed.Mesh.GetBoundingBox(accurate: true)),
            planar: static _ => Option<BoundingBox>.None),
        level: static _ => None,
        uniform: static _ => None,
        union: static union => union.Operands.Fold(Some(BoundingBox.Empty), static (held, operand) => from met in held from next in operand.Bounds select BoundingBox.Union(met, next)),
        intersection: static intersection => intersection.Operands.Fold(Option<BoundingBox>.None, static (held, operand) =>
            operand.Bounds.Map(next => held.Match(Some: met => BoundingBox.Intersection(met, next), None: () => next)) | held),
        difference: static difference => difference.Body.Bounds,
        blend: static blend => from a in blend.A.Bounds from b in blend.B.Bounds select Fields.Inflated(BoundingBox.Union(a, b), (double)blend.Radius / 4d),
        offset: static offset => offset.Amount.SwitchPartially(
            offset.Field,
            @default: static (_, _) => Option<BoundingBox>.None,
            uniform: static (body, uniform) => body.Bounds.Map(bounds => Fields.Inflated(bounds, Math.Max(uniform.Value, 0d)))),
        shell: static shell => shell.Thickness.SwitchPartially(
            shell.Field,
            @default: static (_, _) => Option<BoundingBox>.None,
            uniform: static (body, uniform) => body.Bounds.Map(bounds => Fields.Inflated(bounds, Math.Max(uniform.Value / 2d, 0d)))),
        moved: static moved => moved.Field.Bounds.Map(bounds => Transformed(bounds, moved.Xform)));

    private static Func<Point3d, double> Constant(double value) => _ => value;

    private static Func<Point3d, double> Unsigned(Proximity source) =>
        point => source.Nearest([point], None).Head.Bind(static found => found).Map(point.DistanceTo).IfNone(double.NaN);

    private static Func<Point3d, double> Boundary(Region region) => region.Switch(
        solid: static solid => Unsigned(new Proximity.Brep(solid.Brep)),
        closed: static closed => Unsigned(new Proximity.Mesh(closed.Mesh)),
        planar: static planar => (Unsigned(new Proximity.Curve(planar.Curve)), planar.Plane) switch { var (inPlane, plane) => point => inPlane(plane.ClosestPoint(point)) });

    private static double Smoothed(double a, double b, double radius) =>
        Math.Max(radius - Math.Abs(a - b), 0d) switch { var overlap => Math.Min(a, b) - (overlap * overlap / (4d * radius)) };

    private static BoundingBox Transformed(BoundingBox bounds, Transform xform) {
        _ = bounds.Transform(xform);
        return bounds;
    }

    // --- [LEAVES]
    public sealed record HalfSpace(Plane Plane) : ScalarField;
    public sealed record Ball(Sphere Sphere) : ScalarField;
    public sealed record Solid(Box Box) : ScalarField;
    public sealed record Distance(Proximity Source) : ScalarField;
    public sealed record Signed(Region Region) : ScalarField;
    public sealed record Level(Func<Point3d, double> Value, Func<Point3d, Vector3d> Gradient) : ScalarField;
    public sealed record Uniform(double Value) : ScalarField;

    // --- [COMBINATORS]
    public sealed record Union(Seq<ScalarField> Operands) : ScalarField;
    public sealed record Intersection(Seq<ScalarField> Operands) : ScalarField;
    public sealed record Difference(ScalarField Body, ScalarField Cutter) : ScalarField;
    public sealed record Blend(ScalarField A, ScalarField B, BlendRadius Radius) : ScalarField;
    public sealed record Offset(ScalarField Field, ScalarField Amount) : ScalarField;
    public sealed record Shell(ScalarField Field, ScalarField Thickness) : ScalarField;

    public sealed record Moved : ScalarField {
        private Moved(ScalarField field, Transform xform, Transform inverse) => (Field, Xform, Inverse) = (field, xform, inverse);

        public ScalarField Field { get; }

        public Transform Xform { get; }

        internal Transform Inverse { get; }

        public static Fin<Moved> Of(ScalarField field, Transform xform) =>
            xform.TryGetInverse(out Transform inverse) ? new Moved(field, xform, inverse) : new Refused(nameof(Transform.TryGetInverse), Refused.Declined);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fields {
    // --- [MESHING]
    public static IO<Mesh> Isosurface(Tolerances tolerances, ScalarField field, IsosurfaceResolution resolution, RootFindingSteps rootFindingSteps, Option<BoundingBox> box, Action<IDisposable> retain) =>
        from grid in IO.lift(() => Grid(field, resolution, box))
        from mesh in Copies.Acquire(nameof(Mesh.CreateFromIsosurface), _ => Answer.One(Mesh.CreateFromIsosurface(field.Evaluator(tolerances), grid.Box, grid.Resolution, (int)rootFindingSteps)), retain)
        select mesh;

    private static Fin<(BoundingBox Box, int Resolution)> Grid(ScalarField field, IsosurfaceResolution resolution, Option<BoundingBox> box) =>
        (box.Map(given => (Box: given, Resolution: (int)resolution))
            | field.Bounds.Map(bounds => (Box: Inflated(bounds, bounds.Diagonal.MinimumCoordinate / (int)resolution), Resolution: (int)resolution + 2)))
        .ToFin(Error.New(ErrorCodes.ValidationFailed, "neither the field nor the caller bounds the isosurface"))
        .Bind(static grid => grid.Box.IsValid && grid.Box.IsDegenerate(0d) is 0
            ? Fin.Succ(grid)
            : Error.New(ErrorCodes.ValidationFailed, $"isosurface box {grid.Box} encloses no volume"));

    // --- [BOUNDS]
    internal static BoundingBox Inflated(BoundingBox bounds, double amount) {
        bounds.Inflate(amount);
        return bounds;
    }
}
