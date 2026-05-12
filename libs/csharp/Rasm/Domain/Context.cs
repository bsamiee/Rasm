using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

public sealed record Context {
    private Context(
        Tolerance absolute,
        Tolerance relative,
        Tolerance angle,
        ModelUnitSystem modelUnits) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        ModelUnits = modelUnits;
    }
    internal Tolerance Absolute { get; }
    internal Tolerance Relative { get; }
    internal Tolerance Angle { get; }
    internal ModelUnitSystem ModelUnits { get; }
    internal double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;
    public UnitSystem Units => ModelUnits.Units;
    internal static Validation<Error, Context> Create(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        Fin<ModelUnitSystem> modelUnits) =>
        (modelUnits.ToValidation(),
         Tolerance.Absolute(candidate: absoluteTolerance).ToValidation(),
         Tolerance.Relative(candidate: relativeTolerance).ToValidation(),
         Tolerance.Angle(candidate: angleToleranceRadians).ToValidation())
            .Apply(static (units, absolute, relative, angle) => new Context(absolute: absolute, relative: relative, angle: angle, modelUnits: units))
            .As();
    public static Validation<Error, Context> CreateDefault(UnitSystem units) =>
        ModelUnitSystem.Create(units: units)
            .Match(
                Succ: static modelUnits => Create(
                    absoluteTolerance: RhinoMath.DefaultDistanceToleranceMillimeters * 0.001 / modelUnits.MetersPerUnit,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: RhinoMath.DefaultAngleTolerance,
                    modelUnits: Fin.Succ(modelUnits)),
                Fail: static error => Fin.Fail<Context>(error).ToValidation());
    [BoundaryAdapter]
    public static Validation<Error, Context> FromDocument(RhinoDoc? doc) =>
        Optional(doc)
            .ToValidation(ContextFault.MissingDocument())
            .Bind(static candidate => Create(
                    absoluteTolerance: candidate.ModelAbsoluteTolerance, relativeTolerance: candidate.ModelRelativeTolerance, angleToleranceRadians: candidate.ModelAngleToleranceRadians, modelUnits: candidate.ModelUnitSystem switch {
                        UnitSystem.CustomUnits => candidate.GetCustomUnitSystem(
                            modelUnits: true, customUnitName: out string _, metersPerCustomUnit: out double metersPerCustomUnit) switch {
                                true => Tolerance.CustomUnitScale(candidate: metersPerCustomUnit)
                                    .Bind(static customUnitScale => ModelUnitSystem.FromModelUnits(
                                        units: UnitSystem.CustomUnits, metersPerUnit: customUnitScale)),
                                false => Fin.Fail<ModelUnitSystem>(ContextFault.MissingCustomUnitScale()),
                            },
                        _ => ModelUnitSystem.Create(units: candidate.ModelUnitSystem),
                    }));
    internal Validation<Error, TGeometry> Validate<TGeometry>(
        TGeometry? geometry,
        Requirement? requirement = null) where TGeometry : GeometryBase =>
        Verify.Apply(context: this, geometry: geometry, requirement: requirement ?? Requirement.Strict);
    internal Validation<Error, (TA A, TB B)> ValidatePair<TA, TB>(
        TA a,
        TB b,
        Requirement requirementA,
        Requirement requirementB) where TA : notnull where TB : notnull =>
        Verify.Pair(context: this, a: a, b: b, requirementA: requirementA, requirementB: requirementB);
    internal static Validation<Error, Context> FromKnownUnits(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        UnitSystem units) =>
        Create(
            absoluteTolerance: absoluteTolerance,
            relativeTolerance: relativeTolerance,
            angleToleranceRadians: angleToleranceRadians,
            modelUnits: ModelUnitSystem.Create(units: units));
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct ModelUnitSystem {
        private ModelUnitSystem(UnitSystem units, double metersPerUnit) {
            Units = units;
            MetersPerUnit = metersPerUnit;
        }
        internal UnitSystem Units { get; }
        internal double MetersPerUnit { get; }
        internal static Fin<ModelUnitSystem> Create(UnitSystem units) => units switch {
            UnitSystem.CustomUnits => Fin.Fail<ModelUnitSystem>(
                ContextFault.InvalidUnitSystem(
                    units: units, requirement: "custom units require meters-per-unit metadata")),
            UnitSystem.Unset or UnitSystem.None => Fin.Fail<ModelUnitSystem>(
                ContextFault.InvalidUnitSystem(
                    units: units, requirement: "must be a Rhino model unit system")),
            _ => units switch {
                UnitSystem.Angstroms => 1e-10,
                UnitSystem.Nanometers => 1e-9,
                UnitSystem.Microns => 1e-6,
                UnitSystem.Millimeters => 0.001,
                UnitSystem.Centimeters => 0.01,
                UnitSystem.Decimeters => 0.1,
                UnitSystem.Meters => 1.0,
                UnitSystem.Dekameters => 10.0,
                UnitSystem.Hectometers => 100.0,
                UnitSystem.Kilometers => 1_000.0,
                UnitSystem.Megameters => 1_000_000.0,
                UnitSystem.Gigameters => 1_000_000_000.0,
                UnitSystem.Microinches => 0.0000000254,
                UnitSystem.Mils => 0.0000254,
                UnitSystem.Inches => 0.0254,
                UnitSystem.Feet => 0.3048,
                UnitSystem.Yards => 0.9144,
                UnitSystem.Miles => 1_609.344,
                UnitSystem.PrinterPoints => 0.0254 / 72.0,
                UnitSystem.PrinterPicas => 0.0254 / 6.0,
                UnitSystem.NauticalMiles => 1_852.0,
                UnitSystem.AstronomicalUnits => 149_597_870_700.0,
                UnitSystem.LightYears => 9_460_730_472_580_800.0,
                UnitSystem.Parsecs => 30_856_775_814_913_673.0,
                _ => double.NaN,
            } switch {
                double meters when RhinoMath.IsValidDouble(meters) && meters > RhinoMath.ZeroTolerance => Fin.Succ(new ModelUnitSystem(units: units, metersPerUnit: meters)),
                _ => Fin.Fail<ModelUnitSystem>(
                    ContextFault.InvalidUnitSystem(
                        units: units, requirement: "must resolve to a positive finite meter scale")),
            },
        };
        internal static Fin<ModelUnitSystem> FromModelUnits(UnitSystem units, Tolerance metersPerUnit) => units switch {
            UnitSystem.CustomUnits => Fin.Succ(new ModelUnitSystem(
                units: units, metersPerUnit: metersPerUnit.Value)),
            _ => Create(units: units),
        };
    }
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Tolerance {
        private Tolerance(double value) => Value = value;
        internal double Value { get; }
        internal static Fin<Tolerance> Absolute(double candidate) => Create(candidate: candidate, label: "AbsoluteTolerance", accepts: static c => c > RhinoMath.ZeroTolerance, requirement: "greater than Rhino zero tolerance");
        internal static Fin<Tolerance> Relative(double candidate) => Create(candidate: candidate, label: "RelativeTolerance", accepts: static c => c is >= 0.0 and < 1.0, requirement: "in the range [0, 1)");
        internal static Fin<Tolerance> Angle(double candidate) => Create(candidate: candidate, label: "AngleTolerance", accepts: static c => c is > RhinoMath.Epsilon and <= RhinoMath.TwoPI, requirement: "in the range (epsilon, 2*pi] radians");
        internal static Fin<Tolerance> CustomUnitScale(double candidate) => Create(candidate: candidate, label: "CustomUnitScale", accepts: static c => c > RhinoMath.ZeroTolerance, requirement: "greater than Rhino zero tolerance");
        internal static Fin<Tolerance> Create(double candidate, string label, Func<double, bool> accepts, string requirement) => (RhinoMath.IsValidDouble(candidate), accepts(arg: candidate)) switch {
            (false, _) => Fin.Fail<Tolerance>(ContextFault.NonFinite(label: label, scalar: candidate)),
            (_, false) => Fin.Fail<Tolerance>(ContextFault.OutOfRange(label: label, scalar: candidate, requirement: requirement)),
            _ => Fin.Succ(new Tolerance(value: candidate)),
        };
    }
}
internal static class ContextFault {
    internal static Error NonFinite(string label, double scalar) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry value '{label}' must be finite; actual={scalar:R}."));
    internal static Error OutOfRange(string label, double scalar, string requirement) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry value '{label}' must be {requirement}; actual={scalar:R}."));
    internal static Error InvalidUnitSystem(UnitSystem units, string requirement) =>
        Error.New(message: $"Model unit system must be {requirement}; actual={units}.");
    internal static Error MissingDocument() => Error.New(message: "Rhino document context is required.");
    internal static Error MissingCustomUnitScale() => Error.New(message: "Rhino document custom model unit scale is required.");
}
