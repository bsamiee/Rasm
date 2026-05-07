using System.Globalization;
using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
namespace Core.Domain;

// --- [MODELS] ----------------------------------------------------------------------------------

public sealed record GeometryContext {
    private GeometryContext(
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
    internal double MeshIntersectionTolerance =>
        Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;
    public UnitSystem Units =>
        ModelUnits.Units;
    internal static Validation<Error, GeometryContext> Create(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        Fin<ModelUnitSystem> modelUnits) =>
        (
            modelUnits.ToValidation(),
            Tolerance.Create(
                candidate: absoluteTolerance,
                label: "AbsoluteTolerance",
                accepts: static (double candidate) => candidate > RhinoMath.ZeroTolerance,
                requirement: "greater than Rhino zero tolerance").ToValidation(),
            Tolerance.Create(
                candidate: relativeTolerance,
                label: "RelativeTolerance",
                accepts: static (double candidate) => candidate is >= 0.0 and < 1.0,
                requirement: "in the range [0, 1)").ToValidation(),
            Tolerance.Create(
                candidate: angleToleranceRadians,
                label: "AngleTolerance",
                accepts: static (double candidate) => candidate is > RhinoMath.Epsilon and <= RhinoMath.TwoPI,
                requirement: "in the range (epsilon, 2*pi] radians").ToValidation()
        ).Apply(static (
                ModelUnitSystem modelUnits,
                Tolerance absolute,
                Tolerance relative,
                Tolerance angle) =>
            new GeometryContext(
                absolute: absolute,
                relative: relative,
                angle: angle,
                modelUnits: modelUnits))
        .As();
    public static Validation<Error, GeometryContext> CreateDefault(UnitSystem units) =>
        Create(
            absoluteTolerance: 0.01,
            relativeTolerance: 0.0,
            angleToleranceRadians: RhinoMath.ToRadians(1.0),
            modelUnits: ModelUnitSystem.Create(units: units));
    public static Validation<Error, GeometryContext> FromDocument(RhinoDoc? doc) =>
        Optional(doc)
            .ToValidation(ContextFault.MissingDocument())
            .Bind(static (RhinoDoc candidate) =>
                Create(
                    absoluteTolerance: candidate.ModelAbsoluteTolerance,
                    relativeTolerance: candidate.ModelRelativeTolerance,
                    angleToleranceRadians: candidate.ModelAngleToleranceRadians,
                    modelUnits: candidate.ModelUnitSystem switch {
                        UnitSystem.CustomUnits => candidate.GetCustomUnitSystem(
                            modelUnits: true,
                            customUnitName: out string _,
                            metersPerCustomUnit: out double metersPerCustomUnit) switch {
                                true => Tolerance
                                    .Create(
                                        candidate: metersPerCustomUnit,
                                        label: "CustomUnitScale",
                                        accepts: static (double candidate) => candidate > RhinoMath.ZeroTolerance,
                                        requirement: "greater than Rhino zero tolerance")
                                    .Bind(static (Tolerance customUnitScale) =>
                                        ModelUnitSystem.FromModelUnits(
                                            units: UnitSystem.CustomUnits,
                                            metersPerUnit: customUnitScale)),
                                false => Fin.Fail<ModelUnitSystem>(
                                    ContextFault.MissingCustomUnitScale()),
                            },
                        _ => ModelUnitSystem.Create(units: candidate.ModelUnitSystem),
                    }));
    internal Validation<Error, TGeometry> Validate<TGeometry>(
        TGeometry? geometry,
        GeometryRequirement? requirement = null) where TGeometry : GeometryBase =>
        GeometryValidation.Validate(
            context: this,
            geometry: geometry,
            requirement: requirement ?? GeometryRequirement.Strict);
    internal Validation<Error, (TA A, TB B)> ValidatePair<TA, TB>(
        (TA A, TB B) geometry,
        GeometryRequirement? a = null,
        GeometryRequirement? b = null) where TA : GeometryBase where TB : GeometryBase =>
        GeometryValidation.ValidatePair(
            context: this,
            geometry: geometry,
            a: a ?? GeometryRequirement.Basic,
            b: b ?? GeometryRequirement.Basic);
    internal Validation<Error, (TA A, TB B)> ValidateFirst<TA, TB>(
        (TA A, TB B) geometry,
        GeometryRequirement? requirement = null) where TA : GeometryBase =>
        GeometryValidation.ValidateFirst(
            context: this,
            geometry: geometry,
            requirement: requirement ?? GeometryRequirement.Basic);
    internal static Validation<Error, GeometryContext> FromKnownUnits(
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
        internal static Fin<ModelUnitSystem> Create(UnitSystem units) =>
            units switch {
                UnitSystem.CustomUnits => Fin.Fail<ModelUnitSystem>(
                    ContextFault.InvalidUnitSystem(
                        units: units,
                        requirement: "custom units require meters-per-unit metadata")),
                UnitSystem.Unset => Fin.Fail<ModelUnitSystem>(
                    ContextFault.InvalidUnitSystem(
                        units: units,
                        requirement: "must be a Rhino model unit system")),
                _ => RhinoMath.MetersPerUnit(units) switch {
                    double meters when RhinoMath.IsValidDouble(meters) && meters > RhinoMath.ZeroTolerance =>
                        Fin.Succ(new ModelUnitSystem(units: units, metersPerUnit: meters)),
                    _ => Fin.Fail<ModelUnitSystem>(
                        ContextFault.InvalidUnitSystem(
                            units: units,
                            requirement: "must resolve to a positive finite meter scale")),
                },
            };
        internal static Fin<ModelUnitSystem> FromModelUnits(UnitSystem units, Tolerance metersPerUnit) =>
            units switch {
                UnitSystem.CustomUnits => Fin.Succ(new ModelUnitSystem(
                    units: units,
                    metersPerUnit: metersPerUnit.Value)),
                _ => Create(units: units),
            };
    }
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Tolerance {
        private Tolerance(double value) => Value = value;
        internal double Value { get; }
        internal static Fin<Tolerance> Create(
            double candidate,
            string label,
            Func<double, bool> accepts,
            string requirement) =>
            (RhinoMath.IsValidDouble(candidate), accepts(arg: candidate)) switch {
                (false, _) => Fin.Fail<Tolerance>(ContextFault.NonFinite(label: label, scalar: candidate)),
                (_, false) => Fin.Fail<Tolerance>(ContextFault.OutOfRange(
                    label: label,
                    scalar: candidate,
                    requirement: requirement)),
                _ => Fin.Succ(new Tolerance(value: candidate)),
            };
    }
}

// --- [ERRORS] ----------------------------------------------------------------------------------

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
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Model unit system must be {requirement}; actual={units}."));
    internal static Error MissingDocument() =>
        Error.New(message: "Rhino document context is required.");
    internal static Error MissingCustomUnitScale() =>
        Error.New(message: "Rhino document custom model unit scale is required.");
}
