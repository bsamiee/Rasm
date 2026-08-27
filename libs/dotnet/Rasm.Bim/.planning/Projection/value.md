# [BIM_PROJECTION_VALUE]

`Rasm.Bim` owns the IFC value and unit lowering the `Rasm.Element` contract delegates to it: the contract forbids an `IfcValue` or a dataType string crossing its signature, so narrowing an `IfcProperty` onto the closed `PropertyValue` family and an `IfcPhysicalSimpleQuantity` onto a `MeasureValue` is Bim's. This page owns both halves of that lowering — `IfcUnits` admitting the model's declared unit regime off `IfcUnitAssignment`, and `PropertyLowering` narrowing every value shape under it. GeometryGym stores every magnitude in the model's DECLARED units and never pre-coerces, so a magnitude that reaches the shared uncoerced is the mm-vs-metre import trap; every magnitude crosses through one coercion entry.

The unit regime is the shared `Rasm.Element/Properties/quantity#UNIT_SCHEME` `UnitScheme` — `Declare(DimensionAxis, UnitAxis)` builds it one IFC declaration at a time, `Coerce(native, QuantityType, Dimension)` lowers a project magnitude onto SI, and `UnitAxis(Factor, Offset, Token)` carries the affine arm `IfcConversionBasedUnitWithOffset` requires. Bim owns only what IFC declares: the assignment read, the declaration-to-UnitsNet-member roster, and the measure-type-to-`Dimension` signature the shared registry does not carry (its own registry keys UnitsNet quantity names, and an IFC measure type is an open `QuantityType` mint). Drops accumulate on `Projection/fidelity#FIDELITY_LEDGER`; faults return `Model/faults#FAULT_BAND` `BimFault` through their `Detail` row.

## [01]-[INDEX]

- [02]-[UNIT_INGRESS]: `IfcUnits` — the `IfcUnitAssignment` read that `Declare`s the model's shared `UnitScheme`, the declaration-keyed UnitsNet roster the axis token derives from, and the per-measure carrier override.
- [03]-[PROPERTY_LOWERING]: `PropertyLowering` — the measure-type `Dimension` table, the `ScalarKind` row set narrowing every `IfcValue` leaf, the eight-arm `IfcProperty` narrowing, and the `IfcPhysicalSimpleQuantity` measure mint.

## [02]-[UNIT_INGRESS]

- Owner: `IfcUnits` the IFC unit-declaration admission — `BaseAxes` the seven-row `IfcUnitEnum`-to-shared-`DimensionAxis` correspondence, `DeclaredLengths` the DECLARATION-keyed UnitsNet roster whose member IS the shared axis token, `AxisOf` the one `IfcUnit`-to-`UnitAxis` read, and `SchemeOf` the per-projection `UnitScheme` build.
- Entry: `IfcUnits.SchemeOf(db)` folds every declared assignment row onto `UnitScheme.Si` through the shared `Declare`, closing with the plane-angle axis whose factor is the `DatabaseIfc.ScaleAngle()` read the assignment publishes no scale for; `IfcUnits.AxisOf(unit)` resolves ONE `IfcUnit` to its shared `UnitAxis`, `None` where the select carries no convertible declaration.
- Law: the axis TOKEN is the UnitsNet member's own name, never a second spelling and never back-inferred from the factor — 0.001 spells millimetre, milligram, and millisecond alike, and 1.0 spells both a declared metre and an undeclared axis, so a float back-inference can only guess which unit the egress re-authors. An undeclared axis yields `None` and never lands, so the SI identity stands for it and a factor never outlives its declaration.
- Auto: the four rows a drawing may DECLARE are the `Rasm/Drawing/sheet#UNITS` `DrawingUnits` `Unit` column read the other way; the centimetre and US-survey-foot rows are IFC-declarable residue no sheet standard names, which is the whole discriminant separating this roster from that one.
- Packages: GeometryGymIFC_Core, Rasm.Element, UnitsNet, LanguageExt.Core
- Growth: a newly declarable unit is one `DeclaredLengths` row carrying both its UnitsNet member and its GeometryGym assignment family, so the egress declaration index derives with zero edit; a new base axis is one `BaseAxes` row; a coercion that is not a declared-axis affine belongs at the contract owner, never here.
- Boundary: `UnitScheme` is the ONE unit regime — this page DECLARES it from IFC and never re-implements coercion, so an eight-axis local record, a per-axis scale delegate, or a bare factor multiplied at a call site is the deleted form; the PER-MEASURE carrier override (`IfcPropertySingleValue.Unit`, `IfcPhysicalSimpleQuantity.Unit`) rides the shared `Coerce`'s `Option<UnitAxis>` declared tail (ingress-only — `Render`/`Invert` stay regime-scoped), so this page holds ZERO magnitude arithmetic and its one job is resolving the declaration (`IfcUnits.AxisOf`) the contract then applies; egress re-declaration (`Render`, `Declare` in the SI-to-declared direction) is the contract's and its IFC re-author is `Projection/egress#IFC_EGRESS`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element.Properties;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
internal readonly record struct LengthRegime(LengthUnit Metric, IfcUnitAssignment.Length Declared);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class IfcUnits {
    internal static readonly FrozenDictionary<string, LengthRegime> DeclaredLengths = new Dictionary<string, LengthRegime>(StringComparer.Ordinal) {
        [$"{IfcSIPrefix.NONE}.{IfcSIUnitName.METRE}"] = new(LengthUnit.Meter, IfcUnitAssignment.Length.Metre),
        [$"{IfcSIPrefix.CENTI}.{IfcSIUnitName.METRE}"] = new(LengthUnit.Centimeter, IfcUnitAssignment.Length.Centimetre),
        [$"{IfcSIPrefix.MILLI}.{IfcSIUnitName.METRE}"] = new(LengthUnit.Millimeter, IfcUnitAssignment.Length.Millimetre),
        [$"{IfcConversionBasedUnit.CommonUnitName.foot}"] = new(LengthUnit.Foot, IfcUnitAssignment.Length.Foot),
        [$"{IfcConversionBasedUnit.CommonUnitName.inch}"] = new(LengthUnit.Inch, IfcUnitAssignment.Length.Inch),
        [$"{IfcConversionBasedUnit.CommonUnitName.US_survey_foot}"] = new(LengthUnit.UsSurveyFoot, IfcUnitAssignment.Length.USSurveyFoot),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly Seq<(IfcUnitEnum Ifc, DimensionAxis Axis)> BaseAxes = Seq(
        (IfcUnitEnum.LENGTHUNIT, DimensionAxis.Length), (IfcUnitEnum.MASSUNIT, DimensionAxis.Mass),
        (IfcUnitEnum.TIMEUNIT, DimensionAxis.Time), (IfcUnitEnum.ELECTRICCURRENTUNIT, DimensionAxis.Current),
        (IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT, DimensionAxis.Temperature),
        (IfcUnitEnum.AMOUNTOFSUBSTANCEUNIT, DimensionAxis.Amount),
        (IfcUnitEnum.LUMINOUSINTENSITYUNIT, DimensionAxis.Luminous));

    public static UnitScheme SchemeOf(DatabaseIfc db) =>
        Optional(db.Context?.UnitsInContext).Match(
            None: () => UnitScheme.Si,
            Some: units => BaseAxes
                .Fold(UnitScheme.Si, (scheme, row) =>
                    scheme.Declare(row.Axis, new UnitAxis(units.ScaleSI(row.Ifc), OffsetOf(units[row.Ifc]), TokenOf(units[row.Ifc]))))
                .Declare(DimensionAxis.PlaneAngle, new UnitAxis(db.ScaleAngle(), 0.0, TokenOf(units[IfcUnitEnum.PLANEANGLEUNIT]))));

    public static Option<UnitAxis> AxisOf(IfcUnit? unit) => unit switch {
        IfcConversionBasedUnitWithOffset affine => Some(new UnitAxis(affine.SIFactor(), affine.ConversionOffset, TokenOf(affine))),
        IfcNamedUnit named                      => Some(new UnitAxis(named.SIFactor(), 0.0, TokenOf(named))),
        IfcDerivedUnit derived                  => Some(new UnitAxis(derived.SIFactor(), 0.0, "")),
        _                                       => None,
    };

    static double OffsetOf(IfcUnit? unit) => unit is IfcConversionBasedUnitWithOffset affine ? affine.ConversionOffset : 0.0;

    static string TokenOf(IfcUnit? unit) =>
        (unit switch {
            IfcSIUnit si                     => DeclaredLengths.TryGetValue($"{si.Prefix}.{si.Name}", out LengthRegime metric) ? Some(metric) : None,
            IfcConversionBasedUnit converted => DeclaredLengths.TryGetValue(converted.Name, out LengthRegime common) ? Some(common) : None,
            _                                => None,
        }).Match(Some: static row => row.Metric.ToString(), None: static () => "");
}
```

## [03]-[PROPERTY_LOWERING]

- Owner: `PropertyLowering` the Bim-internal IFC value narrowing — `MeasureDimensions` the measure-type-to-shared-`Dimension` signature table, `Angular` the two rows whose Dimensionless signature nonetheless coerces on the declared plane angle, `ScalarKind` the `[SmartEnum]` row set narrowing every `IfcValue` leaf that carries its own value domain, `Lower` the eight-arm `IfcProperty` narrowing, `LowerValue` the scalar narrowing both the list and table arms take, and `Measure` the `IfcPhysicalSimpleQuantity` mint.
- Entry: `PropertyLowering.Lower(property, rooted, scheme)` returns `WriterT<FidelityLog, Fin, PropertyValue>` — the narrowing's own drops RETURNED beside the value; `PropertyLowering.Measure(quantity, scheme)` returns `Fin<MeasureValue>` because a QTO quantity narrows losslessly or faults.
- Law: the `QuantityType` a measure carries is the IFC MEASURE-TYPE NAME, never the dimension — the seven-exponent vector is not injective over quantity types (an `IfcForceMeasure`, an `IfcLinearMomentMeasure`, and an `IfcModulusOfRotationalSubgradeReactionMeasure` all sign `ForceDim`, and angle, ratio, and count all sit at `Dimensionless`), so the measure-type identity round-trips and a dimension key fabricates one. The shared registry keys UnitsNet quantity names, so an IFC measure type is an OPEN mint whose dimension this table alone answers.
- Auto: `MeasureDimensions` rows are decompile-verified GG `IfcValue` types over their SI base, `Dimension.Create` exponent order `(L, M, T, I, Θ, N, J)`; the roster is closed by GG's SURFACE, not by the IFC schema — `IfcThermalResistanceMeasure` and `IfcTemperatureRateOfChangeMeasure` are absent from that surface and therefore carry no row, so a caller reaching for either takes the `MeasureUnmapped` Text drop rather than a row naming a type the assembly cannot produce. `ScalarKind` rows are keyed on the GG concrete and carry their own narrowing, so the row's key PROVES its delegate's cast and the index derives from the rows themselves.
- Output: two COUNTED identity narrows — an off-table measure type preserves its magnitude as Text rather than claiming a wrong dimension (`MeasureUnmapped`), a non-Label IFC string subtype narrows to Text and re-emits `IfcLabel` (`StringIdentity`), and the non-rooted reference resource whose entity does not round-trip (`ReferenceResource`).
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new IFC value kind is one `ScalarKind` row; a new measure type is one `MeasureDimensions` row carrying its shared `Dimension`, and an angular one adds its name to `Angular`; a new physical-quantity entity is one `QuantityTypes` row whose `Projection/raise#VALUE_RAISE` raiser resolves off that row's own key; a new `IfcProperty` shape is one `Lower` arm.
- Boundary: this narrowing is Bim's because an `IfcValue` or a dataType string crossing a contract signature is the deleted form — the contract carries only the typed `PropertyValue`/`MeasureValue` cases; the three-valued `IfcLogical` narrows to the shared `Logical`'s `Option<bool>` and coercing it to a two-valued Boolean is the deleted form; a typed table cell keeps its measure and logical identity through the SAME scalar narrowing the list arm takes, and the `ValueString` coercion that stripped every cell to Text is the deleted one-correspondence breach; a magnitude GG boxes as something no numeric conversion reaches is ABSENT and spells NaN so the contract's own finite gate refuses it on the result, a 0.0 fallback being the forged measurement that admits, content-keys, and round-trips as a real reading; the two table columns declare SEPARATE units, so each cell coerces on its own column's override and one shared unit read rescales the defined column by the defining column's factor.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Numerics;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ScalarKind {
    public static readonly ScalarKind Logical = new(typeof(IfcLogical),
        static value => new PropertyValue.Logical(PropertyLowering.LogicalOpt(((IfcLogical)value).Logical)));
    public static readonly ScalarKind Boolean = new(typeof(IfcBoolean),
        static value => new PropertyValue.Boolean(value.Value is bool flag && flag));
    public static readonly ScalarKind Integer = new(typeof(IfcInteger),
        static value => new PropertyValue.Integer(new BigInteger(((IfcInteger)value).Magnitude)));
    public static readonly ScalarKind Number = new(typeof(IfcReal),
        static value => new PropertyValue.Number(((IfcReal)value).Magnitude));
    public static readonly ScalarKind Binary = new(typeof(IfcBinary),
        static value => new PropertyValue.Binary(toSeq(((IfcBinary)value).Binary)));
    public static readonly ScalarKind Date = new(typeof(IfcDate),
        static value => new PropertyValue.Temporal(new TemporalValue.Date(LocalDate.FromDateTime((DateTime)value.Value))));
    public static readonly ScalarKind Moment = new(typeof(IfcDateTime),
        static value => new PropertyValue.Temporal(new TemporalValue.Moment(LocalDateTime.FromDateTime((DateTime)value.Value))));
    public static readonly ScalarKind Time = new(typeof(IfcTime),
        static value => new PropertyValue.Temporal(new TemporalValue.Time(LocalTime.FromDateTime((DateTime)value.Value))));
    public static readonly ScalarKind Span = new(typeof(IfcDuration),
        static value => new PropertyValue.Temporal(new TemporalValue.Span(PeriodOf((IfcDuration)value))));
    public static readonly ScalarKind Stamp = new(typeof(IfcTimeStamp),
        static value => new PropertyValue.Temporal(new TemporalValue.Stamp(Instant.FromUnixTimeSeconds((int)value.Value))));

    public Type Ifc { get; }

    [UseDelegateFromConstructor]
    public partial PropertyValue Narrow(IfcValue value);

    public static Option<ScalarKind> For(Type ifc) => ByType.Value.TryGetValue(ifc, out ScalarKind? row) ? Some(row) : None;

    static readonly Lazy<FrozenDictionary<Type, ScalarKind>> ByType =
        new(static () => Items.ToFrozenDictionary(static row => row.Ifc));

    static Period PeriodOf(IfcDuration span) =>
        Period.FromYears(span.Years) + Period.FromMonths(span.Months) + Period.FromDays(span.Days)
        + Period.FromHours(span.Hours) + Period.FromMinutes(span.Minutes)
        + Period.FromSeconds((long)Math.Truncate(span.Seconds))
        + Period.FromNanoseconds((long)((span.Seconds - Math.Truncate(span.Seconds)) * NodaConstants.NanosecondsPerSecond));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PropertyLowering {
    internal static readonly FrozenDictionary<string, Dimension> MeasureDimensions = new Dictionary<string, Dimension>(StringComparer.Ordinal) {
        ["IfcLengthMeasure"] = Dimension.LengthDim, ["IfcPositiveLengthMeasure"] = Dimension.LengthDim,
        ["IfcNonNegativeLengthMeasure"] = Dimension.LengthDim,
        ["IfcAreaMeasure"] = Dimension.AreaDim, ["IfcVolumeMeasure"] = Dimension.VolumeDim,
        ["IfcMassMeasure"] = Dimension.MassDim, ["IfcTimeMeasure"] = Dimension.DurationDim,
        ["IfcThermodynamicTemperatureMeasure"] = Dimension.Create(0, 0, 0, 0, 1, 0, 0),
        ["IfcElectricCurrentMeasure"] = Dimension.Create(0, 0, 0, 1, 0, 0, 0),
        ["IfcLuminousIntensityMeasure"] = Dimension.Create(0, 0, 0, 0, 0, 0, 1),
        ["IfcPlaneAngleMeasure"] = Dimension.Dimensionless, ["IfcSolidAngleMeasure"] = Dimension.Dimensionless,
        ["IfcCountMeasure"] = Dimension.Dimensionless, ["IfcNumericMeasure"] = Dimension.Dimensionless,
        ["IfcRatioMeasure"] = Dimension.Dimensionless, ["IfcPositiveRatioMeasure"] = Dimension.Dimensionless,
        ["IfcNormalisedRatioMeasure"] = Dimension.Dimensionless,
        ["IfcForceMeasure"] = Dimension.ForceDim, ["IfcPressureMeasure"] = Dimension.PressureDim,
        ["IfcMassDensityMeasure"] = Dimension.DensityDim, ["IfcModulusOfElasticityMeasure"] = Dimension.PressureDim,
        ["IfcPlanarForceMeasure"] = Dimension.PressureDim, ["IfcLinearForceMeasure"] = Dimension.Create(0, 1, -2, 0, 0, 0, 0),
        ["IfcLinearStiffnessMeasure"] = Dimension.Create(0, 1, -2, 0, 0, 0, 0),
        ["IfcTorqueMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        ["IfcRotationalStiffnessMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        ["IfcWarpingMomentMeasure"] = Dimension.Create(3, 1, -2, 0, 0, 0, 0),
        ["IfcMomentOfInertiaMeasure"] = Dimension.Create(4, 0, 0, 0, 0, 0, 0),
        ["IfcSectionModulusMeasure"] = Dimension.VolumeDim,
        ["IfcModulusOfSubgradeReactionMeasure"] = Dimension.Create(-2, 1, -2, 0, 0, 0, 0),
        ["IfcModulusOfLinearSubgradeReactionMeasure"] = Dimension.PressureDim,
        ["IfcModulusOfRotationalSubgradeReactionMeasure"] = Dimension.ForceDim,
        ["IfcLinearMomentMeasure"] = Dimension.ForceDim,
        ["IfcMassPerLengthMeasure"] = Dimension.LinearDensityDim,
        ["IfcAreaDensityMeasure"] = Dimension.Create(-2, 1, 0, 0, 0, 0, 0),
        ["IfcThermalTransmittanceMeasure"] = Dimension.ThermalTransmittanceDim,
        ["IfcThermalAdmittanceMeasure"] = Dimension.ThermalTransmittanceDim,
        ["IfcThermalConductivityMeasure"] = Dimension.Create(1, 1, -3, 0, -1, 0, 0),
        ["IfcSpecificHeatCapacityMeasure"] = Dimension.Create(2, 0, -2, 0, -1, 0, 0),
        ["IfcThermalExpansionCoefficientMeasure"] = Dimension.Create(0, 0, 0, 0, -1, 0, 0),
        ["IfcHeatFluxDensityMeasure"] = Dimension.IrradianceDim,
        ["IfcPowerMeasure"] = Dimension.Create(2, 1, -3, 0, 0, 0, 0),
        ["IfcEnergyMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        ["IfcVolumetricFlowRateMeasure"] = Dimension.Create(3, 0, -1, 0, 0, 0, 0),
        ["IfcMassFlowRateMeasure"] = Dimension.Create(0, 1, -1, 0, 0, 0, 0),
        ["IfcVaporPermeabilityMeasure"] = Dimension.Create(0, 0, 1, 0, 0, 0, 0),
        ["IfcMoistureDiffusivityMeasure"] = Dimension.Create(3, 0, -1, 0, 0, 0, 0),
        ["IfcIsothermalMoistureCapacityMeasure"] = Dimension.Create(3, -1, 0, 0, 0, 0, 0),
        ["IfcDynamicViscosityMeasure"] = Dimension.Create(-1, 1, -1, 0, 0, 0, 0),
        ["IfcKinematicViscosityMeasure"] = Dimension.Create(2, 0, -1, 0, 0, 0, 0),
        ["IfcMolecularWeightMeasure"] = Dimension.Create(0, 1, 0, 0, 0, -1, 0),
        ["IfcElectricVoltageMeasure"] = Dimension.Create(2, 1, -3, -1, 0, 0, 0),
        ["IfcFrequencyMeasure"] = Dimension.Create(0, 0, -1, 0, 0, 0, 0),
        ["IfcRotationalFrequencyMeasure"] = Dimension.Create(0, 0, -1, 0, 0, 0, 0),
        ["IfcAngularVelocityMeasure"] = Dimension.Create(0, 0, -1, 0, 0, 0, 0),
        ["IfcLuminousFluxMeasure"] = Dimension.Create(0, 0, 0, 0, 0, 0, 1),
        ["IfcIlluminanceMeasure"] = Dimension.Create(-2, 0, 0, 0, 0, 0, 1),
        ["IfcSoundPowerMeasure"] = Dimension.Create(2, 1, -3, 0, 0, 0, 0),
        ["IfcSoundPressureMeasure"] = Dimension.PressureDim,
        ["IfcLinearVelocityMeasure"] = Dimension.Create(1, 0, -1, 0, 0, 0, 0),
        ["IfcAccelerationMeasure"] = Dimension.Create(1, 0, -2, 0, 0, 0, 0),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenSet<string> Angular =
        new[] { "IfcPlaneAngleMeasure", "IfcSolidAngleMeasure" }.ToFrozenSet(StringComparer.Ordinal);

    // --- [PROPERTY_NARROWING]

    public static WriterT<FidelityLog, Fin, PropertyValue> Lower(IfcProperty property, Map<string, NodeId> rooted, UnitScheme scheme) =>
        property switch {
            IfcPropertySingleValue sv => LowerValue(sv.NominalValue, scheme, sv.Unit),
            IfcPropertyEnumeratedValue ev =>
                from selected in ev.EnumerationValues.AsIterable().ToSeq().Traverse(value => LowerValue(value, scheme, null)).As()
                from sanctioned in Optional(ev.EnumerationReference).Match(
                    Some: reference => reference.EnumerationValues.AsIterable().ToSeq().Traverse(value => LowerValue(value, scheme, null)).As(),
                    None: static () => Fidelity.Clean(Seq<PropertyValue>()))
                select (PropertyValue)new PropertyValue.Enumerated(selected, sanctioned),
            IfcPropertyReferenceValue rv =>
                Optional(rv.PropertyReference as IfcRoot).Bind(root => rooted.Find(root.GlobalId)).Match(
                    Some: Fidelity.Clean,
                    None: () => Fidelity.Drop(FidelityDrop.ReferenceResource, ResourceAnchor(rv), ResourceId(rv)))
                    .Map(id => (PropertyValue)new PropertyValue.Reference(id, Stated(rv.UsageName))),
            IfcPropertyBoundedValue bv => Fidelity.Lift(
                from lower in MeasureOpt(bv.LowerBoundValue, scheme, bv.Unit)
                from upper in MeasureOpt(bv.UpperBoundValue, scheme, bv.Unit)
                from setpoint in MeasureOpt(bv.SetPointValue, scheme, bv.Unit)
                select (PropertyValue)new PropertyValue.Bounded(lower, upper, setpoint)),
            IfcPropertyListValue lv => lv.ListValues.AsIterable().ToSeq()
                .Traverse(value => LowerValue(value, scheme, lv.Unit)).As()
                .Map(static rows => (PropertyValue)new PropertyValue.List(rows)),
            IfcPropertyTableValue tv => toSeq(tv.DefiningValues.Zip(tv.DefinedValues))
                .Traverse(pair =>
                    from defining in LowerValue(pair.First, scheme, tv.DefiningUnit)
                    from defined in LowerValue(pair.Second, scheme, tv.DefinedUnit)
                    select (defining, defined)).As()
                .Map(cells => (PropertyValue)new PropertyValue.Table(cells, InterpolationOf(tv.CurveInterpolation))),
            IfcComplexProperty cp => cp.HasProperties.Values.AsIterable().ToSeq()
                .Traverse(sub => Lower(sub, rooted, scheme).Map(lowered => (Name: RowName(sub), Value: lowered))).As()
                .Map(rows => (PropertyValue)new PropertyValue.Complex(cp.UsageName,
                    rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)))),
            _ => Fidelity.Clean<PropertyValue>(new PropertyValue.Text(Stated(property.Name).IfNone(""))),
        };

    static WriterT<FidelityLog, Fin, PropertyValue> LowerValue(IfcValue? value, UnitScheme scheme, IfcUnit? declared) =>
        Optional(value).Match(
            None: static () => Fidelity.Clean<PropertyValue>(new PropertyValue.Text("")),
            Some: present => ScalarKind.For(present.GetType()).Match(
                Some: row => Fidelity.Clean(row.Narrow(present)),
                None: () => Measured(present, scheme, declared)));

    static WriterT<FidelityLog, Fin, PropertyValue> Measured(IfcValue value, UnitScheme scheme, IfcUnit? declared) =>
        value switch {
            IfcMeasureValue or IfcDerivedMeasureValue => Signature(value).Match(
                Some: row => Fidelity.Lift(MeasureOf(value, row, scheme, declared)
                    .Map(static measure => (PropertyValue)new PropertyValue.Measure(measure))),
                None: () => Fidelity.Drop<PropertyValue>(FidelityDrop.MeasureUnmapped, value.GetType().Name, new PropertyValue.Text(value.ValueString))),
            IfcText or IfcIdentifier =>
                Fidelity.Drop<PropertyValue>(FidelityDrop.StringIdentity, value.GetType().Name, new PropertyValue.Text(value.ValueString)),
            _ => Fidelity.Clean<PropertyValue>(new PropertyValue.Text(value.ValueString)),
        };

    static Option<Dimension> Signature(IfcValue? value) =>
        value is IfcMeasureValue or IfcDerivedMeasureValue && MeasureDimensions.TryGetValue(value.GetType().Name, out Dimension row)
            ? Some(row)
            : None;

    // --- [MEASURE_ADMISSION]

    internal static double Coerce(UnitScheme scheme, double native, string measureType, Dimension dimension, IfcUnit? declared) =>
        scheme.Coerce(native, Elect(measureType), dimension, IfcUnits.AxisOf(declared));

    static QuantityType Elect(string measureType) =>
        Angular.Contains(measureType) ? QuantityType.Angle : QuantityType.Create(measureType);

    static Fin<MeasureValue> MeasureOf(IfcValue measure, Dimension dimension, UnitScheme scheme, IfcUnit? declared) =>
        MeasureValue.OfSi(QuantityType.Create(measure.GetType().Name), dimension,
            Coerce(scheme, AsDouble(measure.Value), measure.GetType().Name, dimension, declared));

    static Fin<Option<MeasureValue>> MeasureOpt(IfcValue? value, UnitScheme scheme, IfcUnit? declared) =>
        Signature(value).TraverseM(row => MeasureOf(value!, row, scheme, declared)).As();

    internal static readonly QuantityType Number = QuantityType.Create("Number");

    internal static readonly FrozenDictionary<Type, QuantityType> QuantityTypes = new Dictionary<Type, QuantityType> {
        [typeof(IfcQuantityLength)] = QuantityType.Length, [typeof(IfcQuantityArea)] = QuantityType.Area,
        [typeof(IfcQuantityVolume)] = QuantityType.Volume, [typeof(IfcQuantityWeight)] = QuantityType.Mass,
        [typeof(IfcQuantityTime)] = QuantityType.Duration, [typeof(IfcQuantityCount)] = QuantityType.Count,
        [typeof(IfcQuantityNumber)] = Number,
    }.ToFrozenDictionary();

    public static Fin<MeasureValue> Measure(IfcPhysicalSimpleQuantity quantity, UnitScheme scheme) =>
        QuantityTypes.TryGetValue(quantity.GetType(), out QuantityType? qto) && Signature(quantity.MeasureValue).Case is Dimension row
            ? MeasureValue.OfSi(qto, row,
                Coerce(scheme, AsDouble(quantity.MeasureValue.Value), quantity.MeasureValue.GetType().Name, row, quantity.Unit))
            : Fin.Fail<MeasureValue>(new BimFault.Refused(BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "quantity-kind-unmapped", quantity.GetType().Name })));

    // --- [BOUNDARY_ADMISSION]

    internal static Option<string> Stated(string? value) => string.IsNullOrEmpty(value) ? None : Some(value);

    static PropertyName RowName(IfcProperty property) => PropertyName.Create(Stated(property.Name).IfNone(""));

    static string ResourceAnchor(IfcPropertyReferenceValue rv) =>
        Stated(rv.UsageName).IfNone(() => Stated(rv.PropertyReference?.GetType().Name).IfNone(""));

    static NodeId ResourceId(IfcPropertyReferenceValue rv) =>
        NodeId.Of(new NodeSeed.Precomputed(ContentAddress.Of(Encoding.UTF8.GetBytes(
            rv.PropertyReference is IfcRoot root
                ? $"ifcroot:{root.GlobalId}"
                : $"{Stated(rv.PropertyReference?.GetType().Name).IfNone("")}:{Stated(rv.UsageName).IfNone("")}"))));

    internal static Option<bool> LogicalOpt(IfcLogicalEnum logical) => logical switch {
        IfcLogicalEnum.TRUE  => Some(true),
        IfcLogicalEnum.FALSE => Some(false),
        _                    => None,
    };

    static Interpolation InterpolationOf(IfcCurveInterpolationEnum curve) => curve switch {
        IfcCurveInterpolationEnum.LINEAR     => Interpolation.Linear,
        IfcCurveInterpolationEnum.LOG_LINEAR => Interpolation.LogLinear,
        IfcCurveInterpolationEnum.LOG_LOG    => Interpolation.LogLog,
        _                                    => Interpolation.NotDefined,
    };

    static double AsDouble(object? value) =>
        value is IConvertible convertible ? Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture) : double.NaN;
}
```

## [04]-[RESEARCH]

(none)
