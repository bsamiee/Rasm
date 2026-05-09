using System.Globalization;
using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
namespace Core.Domain;

// --- [ATTRIBUTES] ------------------------------------------------------------------------------

[AttributeUsage(validOn: AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class BoundaryAdapterAttribute : Attribute;

// --- [MODELS] ----------------------------------------------------------------------------------

public sealed partial record GeometryContext {
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
    public static Validation<Error, GeometryContext> CreateDefault(UnitSystem units) =>
        Create(
            absoluteTolerance: 0.01,
            relativeTolerance: 0.0,
            angleToleranceRadians: RhinoMath.DefaultAngleTolerance,
            modelUnits: ModelUnitSystem.Create(units: units));
    [BoundaryAdapter]
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
                                true => MetersPerUnit
                                    .Create(candidate: metersPerCustomUnit)
                                    .Bind(static (MetersPerUnit scale) =>
                                        ModelUnitSystem.FromModelUnits(
                                            units: UnitSystem.CustomUnits,
                                            metersPerUnit: scale)),
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
    [ComplexValueObject(SkipFactoryMethods = true, ConstructorAccessModifier = AccessModifier.Internal)]
    [StructLayout(LayoutKind.Auto)]
    internal readonly partial struct ModelUnitSystem {
        internal UnitSystem Units { get; }
        internal MetersPerUnit MetersPerUnit { get; }
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
                _ => (Fin.Succ(units), MetersPerUnit.Create(candidate: RhinoMath.MetersPerUnit(units)))
                    .Apply(static (UnitSystem u, MetersPerUnit scale) =>
                        new ModelUnitSystem(units: u, metersPerUnit: scale))
                    .As(),
            };
        internal static Fin<ModelUnitSystem> FromModelUnits(UnitSystem units, MetersPerUnit metersPerUnit) =>
            units switch {
                UnitSystem.CustomUnits => Fin.Succ(new ModelUnitSystem(
                    units: units,
                    metersPerUnit: metersPerUnit)),
                _ => Create(units: units),
            };
    }
    [ValueObject<double>(SkipFactoryMethods = true)]
    internal readonly partial struct AbsoluteTolerance {
        internal double Value =>
            _value;
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
    [ValueObject<double>(SkipFactoryMethods = true)]
    internal readonly partial struct RelativeTolerance {
        internal double Value =>
            _value;
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
    [ValueObject<double>(SkipFactoryMethods = true)]
    internal readonly partial struct AngleTolerance {
        internal double Value =>
            _value;
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
    [ValueObject<double>(SkipFactoryMethods = true, ConstructorAccessModifier = AccessModifier.Internal)]
    internal readonly partial struct MetersPerUnit {
        internal double Value =>
            _value;
        internal static Fin<MetersPerUnit> Create(double candidate) =>
            (RhinoMath.IsValidDouble(candidate), candidate > RhinoMath.ZeroTolerance) switch {
                (false, _) => Fin.Fail<MetersPerUnit>(
                    ContextFault.NonFinite(label: nameof(MetersPerUnit), scalar: candidate)),
                (_, false) => Fin.Fail<MetersPerUnit>(
                    ContextFault.OutOfRange(
                        label: nameof(MetersPerUnit),
                        scalar: candidate,
                        requirement: "greater than Rhino zero tolerance")),
                _ => Fin.Succ(new MetersPerUnit(value: candidate)),
            };
    }
}

// --- [RUNTIME] ---------------------------------------------------------------------------------

[ValueObject<int>(SkipFactoryMethods = true)]
public readonly partial struct IndexHint {
    public int Value =>
        _value;
    public static Fin<IndexHint> Create(int value) =>
        Fin.Succ(new IndexHint(value: value));
}

public sealed record AnalysisRuntime(GeometryContext Context, Option<IndexHint> Index = default);

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
