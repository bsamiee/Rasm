using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AbsoluteTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value > RhinoMath.ZeroTolerance) switch {
            (true, true) => null,
            (false, _) => new ValidationError(message: $"AbsoluteTolerance must be finite (got {value})."),
            (_, false) => new ValidationError(message: $"AbsoluteTolerance must be > {RhinoMath.ZeroTolerance}."),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct RelativeTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value is >= 0.0 and < 1.0) switch {
            (true, true) => null,
            _ => new ValidationError(message: $"RelativeTolerance must lie in [0,1) (got {value})."),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct AngleTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value is > RhinoMath.Epsilon and <= RhinoMath.TwoPI) switch {
            (true, true) => null,
            _ => new ValidationError(message: $"AngleTolerance must be in (epsilon, 2*pi] radians (got {value})."),
        };
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct CustomUnitScale {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = (double.IsFinite(d: value), value > RhinoMath.ZeroTolerance) switch {
            (true, true) => null,
            _ => new ValidationError(message: $"CustomUnitScale must be > {RhinoMath.ZeroTolerance} (got {value})."),
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UnitScale {
    private UnitScale(UnitSystem units, double metersPerUnit) {
        Units = units;
        MetersPerUnit = metersPerUnit;
    }
    public UnitSystem Units { get; }
    internal double MetersPerUnit { get; }
    public static Fin<UnitScale> Create(UnitSystem units) => units switch {
        UnitSystem.CustomUnits => Fin.Fail<UnitScale>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "custom units require meters-per-unit metadata")),
        UnitSystem.Unset or UnitSystem.None => Fin.Fail<UnitScale>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must be a Rhino model unit system")),
        _ => RhinoMath.MetersPerUnit(units: units) switch {
            double meters when RhinoMath.IsValidDouble(x: meters) && meters > RhinoMath.ZeroTolerance => Fin.Succ(new UnitScale(units: units, metersPerUnit: meters)),
            _ => Fin.Fail<UnitScale>(error: new Fault.InvalidUnitSystem(Units: units, Requirement: "must resolve to a positive finite meter scale")),
        },
    };
    internal static Fin<UnitScale> FromModelUnits(UnitSystem units, CustomUnitScale customScale) => units switch {
        UnitSystem.CustomUnits => Fin.Succ(new UnitScale(units: units, metersPerUnit: customScale.Value)),
        _ => Create(units: units),
    };
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed record Context {
    private Context(AbsoluteTolerance absolute, RelativeTolerance relative, AngleTolerance angle, UnitScale scale) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        Scale = scale;
    }
    public AbsoluteTolerance Absolute { get; }
    public RelativeTolerance Relative { get; }
    public AngleTolerance Angle { get; }
    public UnitScale Scale { get; }
    public UnitSystem Units => Scale.Units;
    internal double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;
    internal Validation<Error, T> Validate<T>(T? geometry, Requirement? requirement = null) where T : GeometryBase =>
        Verify.Apply(context: this, value: geometry, requirement: requirement);
    internal Validation<Error, (TA A, TB B)> ValidatePair<TA, TB>(TA a, TB b, Requirement requirementA, Requirement requirementB) where TA : notnull where TB : notnull =>
        Verify.Pair(context: this, a: a, b: b, requirementA: requirementA, requirementB: requirementB);
    internal static Validation<Error, Context> Create(double absolute, double relative, double angle, Fin<UnitScale> scale) =>
        (absolute.TryCreateValidated<AbsoluteTolerance>(),
         relative.TryCreateValidated<RelativeTolerance>(),
         angle.TryCreateValidated<AngleTolerance>(),
         scale.ToValidation())
            .Apply(static (AbsoluteTolerance a, RelativeTolerance r, AngleTolerance n, UnitScale s) => new Context(absolute: a, relative: r, angle: n, scale: s))
            .As();
    public static Validation<Error, Context> CreateDefault(UnitSystem units) =>
        UnitScale.Create(units: units).Match(
            Succ: static unitScale => Create(
                absolute: RhinoMath.DefaultDistanceToleranceMillimeters * RhinoMath.MetersPerUnit(units: UnitSystem.Millimeters) / unitScale.MetersPerUnit,
                relative: 0.0,
                angle: RhinoMath.DefaultAngleTolerance,
                scale: Fin.Succ(unitScale)),
            Fail: static error => Fin.Fail<Context>(error: error).ToValidation());
    [BoundaryAdapter]
    public static Validation<Error, Context> FromDocument(RhinoDoc? doc) =>
        Optional(doc).ToValidation<Error>(Fail: new Fault.MissingDocument())
            .Bind(static candidate => Create(
                absolute: candidate.ModelAbsoluteTolerance,
                relative: candidate.ModelRelativeTolerance,
                angle: candidate.ModelAngleToleranceRadians,
                scale: candidate.ModelUnitSystem switch {
                    UnitSystem.CustomUnits => candidate.GetCustomUnitSystem(modelUnits: true, customUnitName: out string _, metersPerCustomUnit: out double metersPerCustomUnit) switch {
                        true => metersPerCustomUnit.TryCreateValidated<CustomUnitScale>().ToFin().Bind(static unitScale => UnitScale.FromModelUnits(units: UnitSystem.CustomUnits, customScale: unitScale)),
                        false => Fin.Fail<UnitScale>(error: new Fault.MissingCustomUnitScale()),
                    },
                    _ => UnitScale.Create(units: candidate.ModelUnitSystem),
                }));
    internal static Validation<Error, Context> FromKnownUnits(double absolute, double relative, double angle, UnitSystem units) =>
        Create(absolute: absolute, relative: relative, angle: angle, scale: UnitScale.Create(units: units));
}
