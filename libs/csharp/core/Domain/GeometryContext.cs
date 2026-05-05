using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;

namespace Core.Domain;

// --- [MODELS] ----------------------------------------------------------------------------------

public sealed record GeometryContext {
    private const double DefaultAbsoluteToleranceScalar = 0.01;
    private const double DefaultRelativeToleranceScalar = 0.0;
    private static readonly double DefaultAngleToleranceRadians = RhinoMath.ToRadians(1.0);

    private GeometryContext(
        AbsoluteTolerance absolute,
        RelativeTolerance relative,
        AngleTolerance angle,
        ModelUnitSystem modelUnits) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        ModelUnits = modelUnits;
    }

    internal AbsoluteTolerance Absolute { get; }
    internal RelativeTolerance Relative { get; }
    internal AngleTolerance Angle { get; }
    internal ModelUnitSystem ModelUnits { get; }
    public UnitSystem Units =>
        ModelUnits.Units;
    internal static Validation<Error, GeometryContext> Create(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        Fin<ModelUnitSystem> modelUnits) =>
        (
            modelUnits.ToValidation(),
            AbsoluteTolerance.Create(candidate: absoluteTolerance).ToValidation(),
            RelativeTolerance.Create(candidate: relativeTolerance).ToValidation(),
            AngleTolerance.Create(candidate: angleToleranceRadians).ToValidation()
        ).Apply(static (
                ModelUnitSystem modelUnits,
                AbsoluteTolerance absolute,
                RelativeTolerance relative,
                AngleTolerance angle) =>
            new GeometryContext(
                absolute: absolute,
                relative: relative,
                angle: angle,
                modelUnits: modelUnits))
        .As();

    internal static Validation<Error, GeometryContext> FromModelTolerances(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        Fin<ModelUnitSystem> modelUnits) =>
        Create(
            absoluteTolerance: absoluteTolerance,
            relativeTolerance: relativeTolerance,
            angleToleranceRadians: angleToleranceRadians,
            modelUnits: modelUnits);

    public static Validation<Error, GeometryContext> CreateDefault(UnitSystem units) =>
        FromModelTolerances(
            absoluteTolerance: DefaultAbsoluteToleranceScalar,
            relativeTolerance: DefaultRelativeToleranceScalar,
            angleToleranceRadians: DefaultAngleToleranceRadians,
            modelUnits: ModelUnitSystem.Create(units: units));

    public static Validation<Error, GeometryContext> FromDocument(RhinoDoc doc) =>
        Optional(doc)
            .ToValidation(ContextFault.MissingDocument())
            .Bind(static (RhinoDoc candidate) =>
                FromModelTolerances(
                    absoluteTolerance: candidate.ModelAbsoluteTolerance,
                    relativeTolerance: candidate.ModelRelativeTolerance,
                    angleToleranceRadians: candidate.ModelAngleToleranceRadians,
                    modelUnits: candidate.ModelUnitSystem switch {
                        UnitSystem.CustomUnits => candidate.GetCustomUnitSystem(
                            modelUnits: true,
                            customUnitName: out string _,
                            metersPerCustomUnit: out double metersPerCustomUnit) switch {
                                true => ModelUnitSystem.CustomUnitScale
                                    .Create(candidate: metersPerCustomUnit)
                                    .Bind(static (ModelUnitSystem.CustomUnitScale customUnitScale) =>
                                        ModelUnitSystem.FromModelUnits(
                                            units: UnitSystem.CustomUnits,
                                            metersPerUnit: customUnitScale)),
                                false => Fin.Fail<ModelUnitSystem>(
                                    ContextFault.MissingCustomUnitScale()),
                            },
                        _ => ModelUnitSystem.Create(units: candidate.ModelUnitSystem),
                    }));

    public Validation<Error, TGeometry> Validate<TGeometry>(TGeometry geometry) where TGeometry : GeometryBase =>
        GeometryValidation.Validate(context: this, geometry: geometry);

    internal Fin<LengthScale> ScaleTo(ModelUnitSystem targetUnits) =>
        EqualityComparer<ModelUnitSystem>.Default.Equals(x: ModelUnits, y: targetUnits) switch {
            true => Fin.Succ(LengthScale.Identity),
            false => RhinoMath.UnitScale(
                    from: Units,
                    fromMetersPerUnit: ModelUnits.MetersPerUnit,
                    to: targetUnits.Units,
                    toMetersPerUnit: targetUnits.MetersPerUnit) switch {
                        double scale => LengthScale.Create(candidate: scale)
                            .MapFail(static (Error _) => ContextFault.InvalidUnitScale()),
                    },
        };

    internal Fin<ModelLength> ConvertLength(ModelLength length, ModelUnitSystem targetUnits) =>
        (Fin.Succ(length), ScaleTo(targetUnits: targetUnits))
            .Apply(static (ModelLength modelLength, LengthScale scale) =>
                modelLength.Value * scale.Value)
            .Bind(static (double candidate) => ModelLength.Create(candidate: candidate))
            .As();

    internal static Validation<Error, GeometryContext> FromKnownUnits(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        UnitSystem units) =>
        FromModelTolerances(
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

        internal static Fin<ModelUnitSystem> FromModelUnits(UnitSystem units, CustomUnitScale metersPerUnit) =>
            units switch {
                UnitSystem.CustomUnits => Fin.Succ(new ModelUnitSystem(
                    units: units,
                    metersPerUnit: metersPerUnit.Value)),
                _ => Create(units: units),
            };

        [StructLayout(LayoutKind.Auto)]
        internal readonly record struct CustomUnitScale {
            private CustomUnitScale(double value) =>
                Value = value;

            internal double Value { get; }

            internal static Fin<CustomUnitScale> Create(double candidate) =>
                (RhinoMath.IsValidDouble(candidate), candidate > RhinoMath.ZeroTolerance) switch {
                    (true, true) => Fin.Succ(new CustomUnitScale(value: candidate)),
                    _ => Fin.Fail<CustomUnitScale>(
                        ContextFault.OutOfRange(
                            label: nameof(CustomUnitScale),
                            scalar: candidate,
                            requirement: "greater than Rhino zero tolerance")),
                };
        }
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct AbsoluteTolerance {
        private AbsoluteTolerance(double value) =>
            Value = value;

        internal double Value { get; }

        internal static Fin<AbsoluteTolerance> Create(double candidate) =>
            (RhinoMath.IsValidDouble(candidate), candidate > RhinoMath.ZeroTolerance) switch {
                (false, _) => Fin.Fail<AbsoluteTolerance>(
                    ContextFault.NonFinite(label: nameof(AbsoluteTolerance), scalar: candidate)),
                (_, false) => Fin.Fail<AbsoluteTolerance>(
                    ContextFault.OutOfRange(
                        label: nameof(AbsoluteTolerance),
                        scalar: candidate,
                        requirement: "greater than Rhino zero tolerance")),
                _ => Fin.Succ(new AbsoluteTolerance(value: candidate)),
            };
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct RelativeTolerance {
        private RelativeTolerance(double value) =>
            Value = value;

        internal double Value { get; }

        internal static Fin<RelativeTolerance> Create(double candidate) =>
            (RhinoMath.IsValidDouble(candidate), candidate is >= 0.0 and < 1.0) switch {
                (false, _) => Fin.Fail<RelativeTolerance>(
                    ContextFault.NonFinite(label: nameof(RelativeTolerance), scalar: candidate)),
                (_, false) => Fin.Fail<RelativeTolerance>(
                    ContextFault.OutOfRange(
                        label: nameof(RelativeTolerance),
                        scalar: candidate,
                        requirement: "in the range [0, 1)")),
                _ => Fin.Succ(new RelativeTolerance(value: candidate)),
            };
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct AngleTolerance {
        private AngleTolerance(double value) =>
            Value = value;

        internal double Value { get; }

        internal static Fin<AngleTolerance> Create(double candidate) =>
            (RhinoMath.IsValidDouble(candidate), candidate is > RhinoMath.Epsilon and <= RhinoMath.TwoPI) switch {
                (false, _) => Fin.Fail<AngleTolerance>(
                    ContextFault.NonFinite(label: nameof(AngleTolerance), scalar: candidate)),
                (_, false) => Fin.Fail<AngleTolerance>(
                    ContextFault.OutOfRange(
                        label: nameof(AngleTolerance),
                        scalar: candidate,
                        requirement: "in the range (epsilon, 2*pi] radians")),
                _ => Fin.Succ(new AngleTolerance(value: candidate)),
            };
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct ModelLength {
        private ModelLength(double value) =>
            Value = value;

        internal double Value { get; }

        internal static Fin<ModelLength> Create(double candidate) =>
            RhinoMath.IsValidDouble(candidate) switch {
                true => Fin.Succ(new ModelLength(value: candidate)),
                false => Fin.Fail<ModelLength>(
                    ContextFault.NonFinite(label: nameof(ModelLength), scalar: candidate)),
            };
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct LengthScale {
        private LengthScale(double value) =>
            Value = value;

        internal double Value { get; }
        internal static LengthScale Identity => new(value: 1.0);

        internal static Fin<LengthScale> Create(double candidate) =>
            (RhinoMath.IsValidDouble(candidate), candidate > RhinoMath.ZeroTolerance) switch {
                (false, _) => Fin.Fail<LengthScale>(
                    ContextFault.NonFinite(label: nameof(LengthScale), scalar: candidate)),
                (_, false) => Fin.Fail<LengthScale>(
                    ContextFault.OutOfRange(
                        label: nameof(LengthScale),
                        scalar: candidate,
                        requirement: "greater than Rhino zero tolerance")),
                _ => Fin.Succ(new LengthScale(value: candidate)),
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

    internal static Error InvalidUnitScale() =>
        Error.New(message: "Rhino unit conversion produced an invalid scale.");

    internal static Error InvalidUnitSystem(UnitSystem units, string requirement) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Model unit system must be {requirement}; actual={units}."));

    internal static Error MissingDocument() =>
        Error.New(message: "Rhino document context is required.");

    internal static Error MissingCustomUnitScale() =>
        Error.New(message: "Rhino document custom model unit scale is required.");
}
