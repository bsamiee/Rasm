using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AbsoluteTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value > RhinoMath.ZeroTolerance) switch {
            (true, true) => null,
            (false, _) => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AbsoluteTolerance must be finite (got {value}).")),
            (_, false) => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AbsoluteTolerance must be > {RhinoMath.ZeroTolerance}.")),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct RelativeTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value is >= 0.0 and < 1.0) switch {
            (true, true) => null,
            _ => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"RelativeTolerance must lie in [0,1) (got {value}).")),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AngleTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value is > RhinoMath.Epsilon and <= RhinoMath.TwoPI) switch {
            (true, true) => null,
            _ => new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"AngleTolerance must be in (epsilon, 2*pi] radians (got {value}).")),
        };
}

public sealed record Context {
    private const double DefaultFractionalTolerance = 1.0e-8;
    private Context(AbsoluteTolerance absolute, RelativeTolerance relative, AngleTolerance angle, UnitSystem units) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        Units = units;
    }
    public AbsoluteTolerance Absolute { get; }
    public RelativeTolerance Relative { get; }
    public AngleTolerance Angle { get; }
    public UnitSystem Units { get; }
    internal double Fractional => Relative.Value > 0.0 ? Relative.Value : DefaultFractionalTolerance;
    internal double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;
    internal static Validation<Error, Context> Of(double absolute, double relative, double angle, UnitSystem units) =>
        (absolute.TryCreateValidated<AbsoluteTolerance>(),
         relative.TryCreateValidated<RelativeTolerance>(),
         angle.TryCreateValidated<AngleTolerance>(),
         (units switch {
             UnitSystem.Unset or UnitSystem.None => Fin.Fail<UnitSystem>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be a Rhino model unit system")),
             _ => Fin.Succ(units),
         }).ToValidation())
            .Apply(static (a, r, n, u) => new Context(absolute: a, relative: r, angle: n, units: u))
            .As();
    public static Validation<Error, Context> Of(UnitSystem units) => units switch {
        UnitSystem.CustomUnits => Fin.Fail<Context>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be explicit when custom")).ToValidation(),
        UnitSystem.Unset or UnitSystem.None => Fin.Fail<Context>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be a Rhino model unit system")).ToValidation(),
        _ => RhinoMath.UnitScale(from: UnitSystem.Millimeters, to: units) switch {
            double scale when RhinoMath.IsValidDouble(x: scale) && scale > RhinoMath.ZeroTolerance => Of(
                absolute: RhinoMath.DefaultDistanceToleranceMillimeters * scale,
                relative: DefaultFractionalTolerance,
                angle: RhinoMath.DefaultAngleTolerance,
                units: units),
            _ => Fin.Fail<Context>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must resolve to a positive finite default scale")).ToValidation(),
        },
    };
    [BoundaryAdapter]
    public static Validation<Error, Context> Of(RhinoDoc? doc) =>
        Optional(doc).ToValidation<Error>(Fail: new Fault.MissingContext(Key: Op.Of(name: nameof(Of))))
            .Bind(static candidate => Of(
                absolute: candidate.ModelAbsoluteTolerance,
                relative: candidate.ModelRelativeTolerance,
                angle: candidate.ModelAngleToleranceRadians,
                units: candidate.ModelUnitSystem));
}
