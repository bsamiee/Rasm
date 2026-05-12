using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

public sealed record Context {
    private Context(
        Tolerance absolute,
        Tolerance relative,
        Tolerance angle,
        UnitScale unitScale) {
        Absolute = absolute;
        Relative = relative;
        Angle = angle;
        Scale = unitScale;
    }
    internal Tolerance Absolute { get; }
    internal Tolerance Relative { get; }
    internal Tolerance Angle { get; }
    internal UnitScale Scale { get; }
    internal double MeshIntersectionTolerance => Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient;
    public UnitSystem Units => Scale.Units;
    internal static Validation<Error, Context> Create(
        double absoluteTolerance,
        double relativeTolerance,
        double angleToleranceRadians,
        Fin<UnitScale> unitScale) =>
        (unitScale.ToValidation(),
         Tolerance.Absolute(candidate: absoluteTolerance).ToValidation(),
         Tolerance.Relative(candidate: relativeTolerance).ToValidation(),
         Tolerance.Angle(candidate: angleToleranceRadians).ToValidation())
            .Apply(static (scale, absolute, relative, angle) => new Context(absolute: absolute, relative: relative, angle: angle, unitScale: scale))
            .As();
    public static Validation<Error, Context> CreateDefault(UnitSystem units) =>
        UnitScale.Create(units: units)
            .Match(
                Succ: static unitScale => Create(
                    absoluteTolerance: RhinoMath.DefaultDistanceToleranceMillimeters * RhinoMath.MetersPerUnit(units: UnitSystem.Millimeters) / unitScale.MetersPerUnit,
                    relativeTolerance: 0.0,
                    angleToleranceRadians: RhinoMath.DefaultAngleTolerance,
                    unitScale: Fin.Succ(unitScale)),
                Fail: static error => Fin.Fail<Context>(error).ToValidation());
    [BoundaryAdapter]
    public static Validation<Error, Context> FromDocument(RhinoDoc? doc) =>
        Optional(doc)
            .ToValidation<Error>(new ContextFault.MissingDocument())
            .Bind(static candidate => Create(
                    absoluteTolerance: candidate.ModelAbsoluteTolerance, relativeTolerance: candidate.ModelRelativeTolerance, angleToleranceRadians: candidate.ModelAngleToleranceRadians, unitScale: candidate.ModelUnitSystem switch {
                        UnitSystem.CustomUnits => candidate.GetCustomUnitSystem(
                            modelUnits: true, customUnitName: out string _, metersPerCustomUnit: out double metersPerCustomUnit) switch {
                                true => Tolerance.CustomUnitScale(candidate: metersPerCustomUnit)
                                    .Bind(static customUnitScale => UnitScale.FromModelUnits(
                                        units: UnitSystem.CustomUnits, metersPerUnit: customUnitScale)),
                                false => Fin.Fail<UnitScale>(new ContextFault.MissingCustomUnitScale()),
                            },
                        _ => UnitScale.Create(units: candidate.ModelUnitSystem),
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
            unitScale: UnitScale.Create(units: units));
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct UnitScale {
        private UnitScale(UnitSystem units, double metersPerUnit) {
            Units = units;
            MetersPerUnit = metersPerUnit;
        }
        internal UnitSystem Units { get; }
        internal double MetersPerUnit { get; }
        internal static Fin<UnitScale> Create(UnitSystem units) => units switch {
            UnitSystem.CustomUnits => Fin.Fail<UnitScale>(
                new ContextFault.InvalidUnitSystem(
                    Units: units, Requirement: "custom units require meters-per-unit metadata")),
            UnitSystem.Unset or UnitSystem.None => Fin.Fail<UnitScale>(
                new ContextFault.InvalidUnitSystem(
                    Units: units, Requirement: "must be a Rhino model unit system")),
            _ => RhinoMath.MetersPerUnit(units: units) switch {
                double meters when RhinoMath.IsValidDouble(x: meters) && meters > RhinoMath.ZeroTolerance => Fin.Succ(new UnitScale(units: units, metersPerUnit: meters)),
                _ => Fin.Fail<UnitScale>(
                    new ContextFault.InvalidUnitSystem(
                        Units: units, Requirement: "must resolve to a positive finite meter scale")),
            },
        };
        internal static Fin<UnitScale> FromModelUnits(UnitSystem units, Tolerance metersPerUnit) => units switch {
            UnitSystem.CustomUnits => Fin.Succ(new UnitScale(
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
            (false, _) => Fin.Fail<Tolerance>(new ContextFault.NonFinite(Label: label, Scalar: candidate)),
            (_, false) => Fin.Fail<Tolerance>(new ContextFault.OutOfRange(Label: label, Scalar: candidate, Requirement: requirement)),
            _ => Fin.Succ(new Tolerance(value: candidate)),
        };
    }
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
internal abstract partial record ContextFault : Error {
    private ContextFault() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    internal sealed record NonFinite(string Label, double Scalar) : ContextFault {
        public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}' must be finite; actual={Scalar:R}.");
    }
    internal sealed record OutOfRange(string Label, double Scalar, string Requirement) : ContextFault {
        public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Geometry value '{Label}' must be {Requirement}; actual={Scalar:R}.");
    }
    internal sealed record InvalidUnitSystem(UnitSystem Units, string Requirement) : ContextFault {
        public override string Message => $"Model unit system must be {Requirement}; actual={Units}.";
    }
    internal sealed record MissingDocument : ContextFault {
        public override string Message => "Rhino document context is required.";
    }
    internal sealed record MissingCustomUnitScale : ContextFault {
        public override string Message => "Rhino document custom model unit scale is required.";
    }
}
