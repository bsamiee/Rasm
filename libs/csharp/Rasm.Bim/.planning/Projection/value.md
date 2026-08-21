# [BIM_PROJECTION_VALUE]

`Rasm.Bim` owns the IFC value and unit lowering the `Rasm.Element` seam delegates to it: the seam forbids an `IfcValue` or a dataType string crossing its signature, so narrowing an `IfcProperty` onto the closed `PropertyValue` family and an `IfcPhysicalSimpleQuantity` onto a `MeasureValue` is Bim's. This page owns both halves of that lowering — `IfcUnits` admitting the model's declared unit regime off `IfcUnitAssignment`, and `PropertyLowering` narrowing every value shape under it. GeometryGym stores every magnitude in the model's DECLARED units and never pre-coerces, so a magnitude that reaches the seam uncoerced is the mm-vs-metre import trap; every magnitude crosses through one coercion entry.

The unit regime is the seam `Rasm.Element/Properties/quantity#UNIT_SCHEME` `UnitScheme` — `Declare(DimensionAxis, UnitAxis)` builds it one IFC declaration at a time, `Coerce(native, QuantityType, Dimension)` lowers a project magnitude onto SI, and `UnitAxis(Factor, Offset, Token)` carries the affine arm `IfcConversionBasedUnitWithOffset` requires. Bim owns only what IFC declares: the assignment read, the declaration-to-UnitsNet-member roster, and the measure-type-to-`Dimension` signature the seam registry does not carry (its own registry keys UnitsNet quantity names, and an IFC measure type is an open `QuantityType` mint). Drops accumulate on `Projection/fidelity#FIDELITY_LEDGER`; faults rail `Model/faults#FAULT_BAND` `BimFault` through their `Detail` row.

## [01]-[INDEX]

- [02]-[UNIT_INGRESS]: `IfcUnits` — the `IfcUnitAssignment` read that `Declare`s the model's seam `UnitScheme`, the declaration-keyed UnitsNet roster the axis token derives from, and the per-measure carrier override.
- [03]-[PROPERTY_LOWERING]: `PropertyLowering` — the measure-type `Dimension` table, the `ScalarKind` row set narrowing every `IfcValue` leaf, the eight-arm `IfcProperty` narrowing, and the `IfcPhysicalSimpleQuantity` measure mint.

## [02]-[UNIT_INGRESS]

- Owner: `IfcUnits` the IFC unit-declaration admission — `BaseAxes` the seven-row `IfcUnitEnum`-to-seam-`DimensionAxis` correspondence, `DeclaredLengths` the DECLARATION-keyed UnitsNet roster whose member IS the seam axis token, `AxisOf` the one `IfcUnit`-to-`UnitAxis` read, and `SchemeOf` the per-projection `UnitScheme` build.
- Entry: `IfcUnits.SchemeOf(db)` folds every declared assignment row onto `UnitScheme.Si` through the seam `Declare`, closing with the plane-angle axis whose factor is the `DatabaseIfc.ScaleAngle()` read the assignment publishes no scale for; `IfcUnits.AxisOf(unit)` resolves ONE `IfcUnit` to its seam `UnitAxis`, `None` where the select carries no convertible declaration.
- Law: the axis TOKEN is the UnitsNet member's own name, never a second spelling and never back-inferred from the factor — 0.001 spells millimetre, milligram, and millisecond alike, and 1.0 spells both a declared metre and an undeclared axis, so a float back-inference can only guess which unit the egress re-authors. An undeclared axis yields `None` and never lands, so the SI identity stands for it and a factor never outlives its declaration.
- Auto: the four rows a drawing may DECLARE are the `Rasm/Drawing/sheet#UNITS` `DrawingUnits` `Unit` column read the other way; the centimetre and US-survey-foot rows are IFC-declarable residue no sheet standard names, which is the whole discriminant separating this roster from that one.
- Packages: GeometryGymIFC_Core, Rasm.Element, UnitsNet, LanguageExt.Core
- Growth: a newly declarable unit is one `DeclaredLengths` row carrying both its UnitsNet member and its GeometryGym assignment family, so the egress declaration index derives with zero edit; a new base axis is one `BaseAxes` row; a coercion that is not a declared-axis affine belongs at the seam owner, never here.
- Boundary: `UnitScheme` is the ONE unit regime — this page DECLARES it from IFC and never re-implements coercion, so an eight-axis local record, a per-axis scale delegate, or a bare factor multiplied at a call site is the deleted form; the PER-MEASURE carrier override (`IfcPropertySingleValue.Unit`, `IfcPhysicalSimpleQuantity.Unit`) rides the seam `Coerce`'s `Option<UnitAxis>` declared tail (ingress-only — `Render`/`Invert` stay regime-scoped), so this page holds ZERO magnitude arithmetic and its one job is resolving the declaration (`IfcUnits.AxisOf`) the seam then applies; egress re-declaration (`Render`, `Declare` in the SI-to-declared direction) is the seam's and its IFC re-author is `Projection/egress#IFC_EGRESS`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element.Properties;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// One declaration row read BOTH ways: the UnitsNet member IS the seam axis token the ingress stamps, and the
// GeometryGym unit-assignment family is the declaration the Projection/egress#IFC_EGRESS re-author authors that
// token back into — the ReleaseMap two-direction law applied to units. The egress index DERIVES off this column,
// so the two directions cannot drift and a hand-kept token-to-family mirror on the egress leg is the deleted form.
internal readonly record struct LengthRegime(LengthUnit Metric, IfcUnitAssignment.Length Declared);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class IfcUnits {
    // The IFC declaration -> its regime row. The KEY is the declaration (an IfcSIUnit prefix+name pair with
    // IfcSIPrefix.NONE for an unprefixed unit, or an IfcConversionBasedUnit common-unit name) and derives from the
    // GG enum members themselves, so a schema rename breaks the build rather than silently missing a row.
    internal static readonly FrozenDictionary<string, LengthRegime> DeclaredLengths = new Dictionary<string, LengthRegime>(StringComparer.Ordinal) {
        [$"{IfcSIPrefix.NONE}.{IfcSIUnitName.METRE}"] = new(LengthUnit.Meter, IfcUnitAssignment.Length.Metre),
        [$"{IfcSIPrefix.CENTI}.{IfcSIUnitName.METRE}"] = new(LengthUnit.Centimeter, IfcUnitAssignment.Length.Centimetre),
        [$"{IfcSIPrefix.MILLI}.{IfcSIUnitName.METRE}"] = new(LengthUnit.Millimeter, IfcUnitAssignment.Length.Millimetre),
        [$"{IfcConversionBasedUnit.CommonUnitName.foot}"] = new(LengthUnit.Foot, IfcUnitAssignment.Length.Foot),
        [$"{IfcConversionBasedUnit.CommonUnitName.inch}"] = new(LengthUnit.Inch, IfcUnitAssignment.Length.Inch),
        [$"{IfcConversionBasedUnit.CommonUnitName.US_survey_foot}"] = new(LengthUnit.UsSurveyFoot, IfcUnitAssignment.Length.USSurveyFoot),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly Seq<(IfcUnitEnum Ifc, DimensionAxis Seam)> BaseAxes = Seq(
        (IfcUnitEnum.LENGTHUNIT, DimensionAxis.Length), (IfcUnitEnum.MASSUNIT, DimensionAxis.Mass),
        (IfcUnitEnum.TIMEUNIT, DimensionAxis.Time), (IfcUnitEnum.ELECTRICCURRENTUNIT, DimensionAxis.Current),
        (IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT, DimensionAxis.Temperature),
        (IfcUnitEnum.AMOUNTOFSUBSTANCEUNIT, DimensionAxis.Amount),
        (IfcUnitEnum.LUMINOUSINTENSITYUNIT, DimensionAxis.Luminous));

    // The assignment-level ScaleSI read is the axis factor (GG resolves the whole declaration chain for it) while
    // the OFFSET and the TOKEN come off the declaration itself, so the affine arm and the declared spelling land
    // on the same row the factor did.
    public static UnitScheme SchemeOf(DatabaseIfc db) =>
        Optional(db.Context?.UnitsInContext).Match(
            None: () => UnitScheme.Si,
            Some: units => BaseAxes
                .Fold(UnitScheme.Si, (scheme, row) =>
                    scheme.Declare(row.Seam, new UnitAxis(units.ScaleSI(row.Ifc), OffsetOf(units[row.Ifc]), TokenOf(units[row.Ifc]))))
                .Declare(DimensionAxis.PlaneAngle, new UnitAxis(db.ScaleAngle(), 0.0, TokenOf(units[IfcUnitEnum.PLANEANGLEUNIT]))));

    // The per-VALUE carrier override: both IfcUnit select branches publish SIFactor(), and the offset branch is
    // the ONE non-multiplicative carrier the schema declares. A select carrying no convertible declaration
    // (IfcMonetaryUnit) yields None and the model regime stands — the prior NaN sentinel read as a finite factor
    // on any consumer that skipped the finite gate, and its catch-all swallowed every future IfcUnit subtype.
    public static Option<UnitAxis> AxisOf(IfcUnit? unit) => unit switch {
        IfcConversionBasedUnitWithOffset affine => Some(new UnitAxis(affine.SIFactor(), affine.ConversionOffset, TokenOf(affine))),
        IfcNamedUnit named                      => Some(new UnitAxis(named.SIFactor(), 0.0, TokenOf(named))),
        IfcDerivedUnit derived                  => Some(new UnitAxis(derived.SIFactor(), 0.0, "")),
        _                                       => None,
    };

    static double OffsetOf(IfcUnit? unit) => unit is IfcConversionBasedUnitWithOffset affine ? affine.ConversionOffset : 0.0;

    // An unrostered declaration yields the empty token and lands the SI scheme, never a wrong unit.
    static string TokenOf(IfcUnit? unit) =>
        (unit switch {
            IfcSIUnit si                     => DeclaredLengths.TryGetValue($"{si.Prefix}.{si.Name}", out LengthRegime metric) ? Some(metric) : None,
            IfcConversionBasedUnit converted => DeclaredLengths.TryGetValue(converted.Name, out LengthRegime common) ? Some(common) : None,
            _                                => None,
        }).Match(Some: static row => row.Metric.ToString(), None: static () => "");
}
```

## [03]-[PROPERTY_LOWERING]

- Owner: `PropertyLowering` the Bim-internal IFC value narrowing — `MeasureDimensions` the measure-type-to-seam-`Dimension` signature table, `Angular` the two rows whose Dimensionless signature nonetheless coerces on the declared plane angle, `ScalarKind` the `[SmartEnum]` row set narrowing every `IfcValue` leaf that carries its own value domain, `Lower` the eight-arm `IfcProperty` narrowing, `LowerValue` the scalar narrowing both the list and table arms take, and `Measure` the `IfcPhysicalSimpleQuantity` mint.
- Entry: `PropertyLowering.Lower(property, rooted, scheme, key)` returns `WriterT<FidelityLog, Fin, PropertyValue>` — the narrowing's own drops RETURNED beside the value; `PropertyLowering.Measure(quantity, scheme, key)` returns `Fin<MeasureValue>` because a QTO quantity narrows losslessly or faults.
- Law: the `QuantityType` a measure carries is the IFC MEASURE-TYPE NAME, never the dimension — the seven-exponent vector is not injective over quantity types (an `IfcForceMeasure`, an `IfcLinearMomentMeasure`, and an `IfcModulusOfRotationalSubgradeReactionMeasure` all sign `ForceDim`, and angle, ratio, and count all sit at `Dimensionless`), so the measure-type identity round-trips and a dimension key fabricates one. The seam registry keys UnitsNet quantity names, so an IFC measure type is an OPEN mint whose dimension this table alone answers.
- Auto: `MeasureDimensions` rows are decompile-verified GG `IfcValue` types over their SI base, `Dimension.Create` exponent order `(L, M, T, I, Θ, N, J)`; the roster is closed by GG's SURFACE, not by the IFC schema — `IfcThermalResistanceMeasure` and `IfcTemperatureRateOfChangeMeasure` are absent from that surface and therefore carry no row, so a caller reaching for either takes the `MeasureUnmapped` Text drop rather than a row naming a type the assembly cannot produce. `ScalarKind` rows are keyed on the GG concrete and carry their own narrowing, so the row's key PROVES its delegate's cast and the index derives from the rows themselves.
- Receipt: two COUNTED identity narrows — an off-table measure type preserves its magnitude as Text rather than claiming a wrong dimension (`MeasureUnmapped`), a non-Label IFC string subtype narrows to Text and re-emits `IfcLabel` (`StringIdentity`), and the non-rooted reference resource whose entity does not round-trip (`ReferenceResource`).
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new IFC value kind is one `ScalarKind` row; a new measure type is one `MeasureDimensions` row carrying its seam `Dimension`, and an angular one adds its name to `Angular`; a new physical-quantity entity is one `QuantityTypes` row whose `Projection/raise#VALUE_RAISE` raiser resolves off that row's own key; a new `IfcProperty` shape is one `Lower` arm.
- Boundary: this narrowing is Bim's because an `IfcValue` or a dataType string crossing a seam signature is the deleted form — the seam carries only the typed `PropertyValue`/`MeasureValue` cases; the three-valued `IfcLogical` narrows to the seam `Logical`'s `Option<bool>` and coercing it to a two-valued Boolean is the deleted form; a typed table cell keeps its measure and logical identity through the SAME scalar narrowing the list arm takes, and the `ValueString` coercion that stripped every cell to Text is the deleted one-correspondence breach; a magnitude GG boxes as something no numeric conversion reaches is ABSENT and spells NaN so the seam's own finite gate refuses it on the rail, a 0.0 fallback being the forged measurement that admits, content-keys, and round-trips as a real reading; the two table columns declare SEPARATE units, so each cell coerces on its own column's override and one shared unit read rescales the defined column by the defining column's factor.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// One row per IfcValue leaf whose value DOMAIN the seam keeps distinct. The row owns both its GG concrete and its
// narrowing, so the delegate's cast is proved by the key that selected it; the IFC string family carries no row
// because its subtype does not change the domain consumed below the seam.
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

    // The index DERIVES from the rows, so the correspondence has one authority and a new row needs no second edit.
    public static Option<ScalarKind> For(Type ifc) => ByType.Value.TryGetValue(ifc, out ScalarKind? row) ? Some(row) : None;

    static readonly Lazy<FrozenDictionary<Type, ScalarKind>> ByType =
        new(static () => Items.ToFrozenDictionary(static row => row.Ifc));

    // GG splits the ISO 8601 duration across seven scalars; the fractional second carries into nanoseconds rather
    // than truncating, because a truncated schedule offset re-exports as a different duration.
    static Period PeriodOf(IfcDuration span) =>
        Period.FromYears(span.Years) + Period.FromMonths(span.Months) + Period.FromDays(span.Days)
        + Period.FromHours(span.Hours) + Period.FromMinutes(span.Minutes)
        + Period.FromSeconds((long)Math.Truncate(span.Seconds))
        + Period.FromNanoseconds((long)((span.Seconds - Math.Truncate(span.Seconds)) * NodaConstants.NanosecondsPerSecond));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class PropertyLowering {
    // GG splits the schema's two value SELECTs into SIBLING bases — IfcMeasureValue and IfcDerivedMeasureValue BOTH
    // derive IfcValue directly (decompile-verified), so every narrowing guard matches BOTH: a guard on
    // IfcMeasureValue alone dead-codes every derived row (Force/Pressure/Density/ThermalTransmittance and the whole
    // MEP set). INTERNAL, not private: the Projection/egress typed-measure mint derives its raise table from these
    // keys, so ingress narrowing and egress raising read one table and can never drift.
    internal static readonly FrozenDictionary<string, Dimension> MeasureDimensions = new Dictionary<string, Dimension>(StringComparer.Ordinal) {
        // IfcMeasureValue family — SI base + dimensionless tokens
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
        // IfcDerivedMeasureValue family — structural. A planar force is force per unit AREA (N/m2) and a linear
        // force force per unit LENGTH (N/m): the two are one exponent apart, and the shared vector that once
        // spelled both mis-scaled every planar-force magnitude by the model's length factor.
        ["IfcForceMeasure"] = Dimension.ForceDim, ["IfcPressureMeasure"] = Dimension.PressureDim,
        ["IfcMassDensityMeasure"] = Dimension.DensityDim, ["IfcModulusOfElasticityMeasure"] = Dimension.PressureDim,
        ["IfcPlanarForceMeasure"] = Dimension.PressureDim, ["IfcLinearForceMeasure"] = Dimension.Create(0, 1, -2, 0, 0, 0, 0),
        ["IfcLinearStiffnessMeasure"] = Dimension.Create(0, 1, -2, 0, 0, 0, 0),
        ["IfcTorqueMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        ["IfcRotationalStiffnessMeasure"] = Dimension.Create(2, 1, -2, 0, 0, 0, 0),
        // Warping stiffness is a warping moment per unit twist — force x length2, ONE length exponent above the
        // rotational row above it; the sealed IfcBoundaryNodeConditionWarping read stamps its row under this type.
        ["IfcWarpingMomentMeasure"] = Dimension.Create(3, 1, -2, 0, 0, 0, 0),
        ["IfcMomentOfInertiaMeasure"] = Dimension.Create(4, 0, 0, 0, 0, 0, 0),
        ["IfcSectionModulusMeasure"] = Dimension.VolumeDim,
        // The subgrade-reaction ladder is THREE distinct types one exponent apart — a face reaction N/m3, an edge
        // reaction N/m2, a node reaction N/m — each the declared measure of a StiffnessSelect<T> arm the
        // Model/structural#STRUCTURAL_PROJECTION restraint reader stamps from, so omitting the edge pair forced its
        // magnitudes through a signature this table never signed.
        ["IfcModulusOfSubgradeReactionMeasure"] = Dimension.Create(-2, 1, -2, 0, 0, 0, 0),
        ["IfcModulusOfLinearSubgradeReactionMeasure"] = Dimension.PressureDim,
        ["IfcModulusOfRotationalSubgradeReactionMeasure"] = Dimension.ForceDim,
        ["IfcLinearMomentMeasure"] = Dimension.ForceDim,
        ["IfcMassPerLengthMeasure"] = Dimension.LinearDensityDim,
        ["IfcAreaDensityMeasure"] = Dimension.Create(-2, 1, 0, 0, 0, 0, 0),
        // IfcDerivedMeasureValue family — thermal, energy, hygric, flow
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
        // The two hygric rows sign the schema's own derived-unit declarations, not the engineering conventions:
        // vapor permeability kg/(s m Pa) reduces to T¹, and moisture diffusivity is declared m3/s (L³T⁻¹), NOT the
        // diffusivity-conventional m2/s — the conventional vector mis-coerces by the model's length factor.
        ["IfcVaporPermeabilityMeasure"] = Dimension.Create(0, 0, 1, 0, 0, 0, 0),
        ["IfcMoistureDiffusivityMeasure"] = Dimension.Create(3, 0, -1, 0, 0, 0, 0),
        ["IfcIsothermalMoistureCapacityMeasure"] = Dimension.Create(3, -1, 0, 0, 0, 0, 0),
        ["IfcDynamicViscosityMeasure"] = Dimension.Create(-1, 1, -1, 0, 0, 0, 0),
        ["IfcKinematicViscosityMeasure"] = Dimension.Create(2, 0, -1, 0, 0, 0, 0),
        ["IfcMolecularWeightMeasure"] = Dimension.Create(0, 1, 0, 0, 0, -1, 0),
        // IfcDerivedMeasureValue family — electrical, lighting, acoustic, motion
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

    // SI carries no angle axis, so both rows sign Dimensionless and their coercion cannot come off the exponent
    // vector — the seam elects the PlaneAngle axis on the QuantityType.Angle family alone, and count, numeric, and
    // ratio sign the same vector while taking no angle factor, so this two-row set is the whole discriminant.
    static readonly FrozenSet<string> Angular =
        new[] { "IfcPlaneAngleMeasure", "IfcSolidAngleMeasure" }.ToFrozenSet(StringComparer.Ordinal);

    // --- [PROPERTY_NARROWING]

    // The IfcProperty family -> the seam PropertyValue union. An IfcComplexProperty RECURSES, so a layered glazing,
    // a multi-component rating, or a bSDD complex template is the seam Complex arm and never a flattened Text; only
    // a non-IfcProperty residue falls to Text.
    public static WriterT<FidelityLog, Fin, PropertyValue> Lower(IfcProperty property, Map<string, NodeId> rooted, UnitScheme scheme, Op key) =>
        property switch {
            IfcPropertySingleValue sv => LowerValue(sv.NominalValue, scheme, sv.Unit, key),
            // The SELECTED value LIST ([1:?]) and the optional allowed set both narrow through the same rail, so a
            // measured or numeric member keeps its discriminant; IfcPropertyEnumeratedValue declares no per-value
            // unit, so this arm alone takes the project regime.
            IfcPropertyEnumeratedValue ev =>
                from selected in ev.EnumerationValues.AsIterable().ToSeq().Traverse(value => LowerValue(value, scheme, null, key)).As()
                from sanctioned in Optional(ev.EnumerationReference).Match(
                    Some: reference => reference.EnumerationValues.AsIterable().ToSeq().Traverse(value => LowerValue(value, scheme, null, key)).As(),
                    None: static () => Fidelity.Clean(Seq<PropertyValue>()))
                select (PropertyValue)new PropertyValue.Enumerated(selected, sanctioned),
            IfcPropertyReferenceValue rv =>
                Optional(rv.PropertyReference as IfcRoot).Bind(root => rooted.Find(root.GlobalId)).Match(
                    Some: Fidelity.Clean,
                    // A non-rooted IfcObjectReferenceSelect resource — a table, an address, a time series — is never
                    // projected as a node, so its identity content-keys and its ENTITY does not round-trip; the
                    // UsageName is always carried, so the cycle drops the resource alone.
                    None: () => Fidelity.Drop(FidelityDrop.ReferenceResource, ResourceAnchor(rv), ResourceId(rv)))
                    .Map(id => (PropertyValue)new PropertyValue.Reference(id, Stated(rv.UsageName))),
            // The bounded arm cannot drop: every bound either narrows to a measure or is absent, so it stays a pure
            // Fin query lifted once.
            IfcPropertyBoundedValue bv => Fidelity.Lift(
                from lower in MeasureOpt(bv.LowerBoundValue, scheme, bv.Unit, key)
                from upper in MeasureOpt(bv.UpperBoundValue, scheme, bv.Unit, key)
                from setpoint in MeasureOpt(bv.SetPointValue, scheme, bv.Unit, key)
                select (PropertyValue)new PropertyValue.Bounded(lower, upper, setpoint)),
            IfcPropertyListValue lv => lv.ListValues.AsIterable().ToSeq()
                .Traverse(value => LowerValue(value, scheme, lv.Unit, key)).As()
                .Map(static rows => (PropertyValue)new PropertyValue.List(rows)),
            // The two table columns declare SEPARATE units (DefiningUnit / DefinedUnit), so each cell coerces on its
            // own column's override.
            IfcPropertyTableValue tv => toSeq(tv.DefiningValues.Zip(tv.DefinedValues))
                .Traverse(pair =>
                    from defining in LowerValue(pair.First, scheme, tv.DefiningUnit, key)
                    from defined in LowerValue(pair.Second, scheme, tv.DefinedUnit, key)
                    select (defining, defined)).As()
                .Map(cells => (PropertyValue)new PropertyValue.Table(cells, InterpolationOf(tv.CurveInterpolation))),
            IfcComplexProperty cp => cp.HasProperties.Values.AsIterable().ToSeq()
                .Traverse(sub => Lower(sub, rooted, scheme, key).Map(lowered => (Name: RowName(sub), Value: lowered))).As()
                .Map(rows => (PropertyValue)new PropertyValue.Complex(cp.UsageName,
                    rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)))),
            _ => Fidelity.Clean<PropertyValue>(new PropertyValue.Text(Stated(property.Name).IfNone(""))),
        };

    // An IfcValue -> the seam scalar family: one row lookup over the leaves that carry their own value domain, then
    // the measure lane, then the two counted identity narrows, then the shared string tail.
    static WriterT<FidelityLog, Fin, PropertyValue> LowerValue(IfcValue? value, UnitScheme scheme, IfcUnit? declared, Op key) =>
        Optional(value).Match(
            None: static () => Fidelity.Clean<PropertyValue>(new PropertyValue.Text("")),
            Some: present => ScalarKind.For(present.GetType()).Match(
                Some: row => Fidelity.Clean(row.Narrow(present)),
                None: () => Measured(present, scheme, declared, key)));

    static WriterT<FidelityLog, Fin, PropertyValue> Measured(IfcValue value, UnitScheme scheme, IfcUnit? declared, Op key) =>
        value switch {
            IfcMeasureValue or IfcDerivedMeasureValue => Signature(value).Match(
                Some: row => Fidelity.Lift(MeasureOf(value, row, scheme, declared, key)
                    .Map(static measure => (PropertyValue)new PropertyValue.Measure(measure))),
                None: () => Fidelity.Drop<PropertyValue>(FidelityDrop.MeasureUnmapped, value.GetType().Name, new PropertyValue.Text(value.ValueString))),
            IfcText or IfcIdentifier =>
                Fidelity.Drop<PropertyValue>(FidelityDrop.StringIdentity, value.GetType().Name, new PropertyValue.Text(value.ValueString)),
            _ => Fidelity.Clean<PropertyValue>(new PropertyValue.Text(value.ValueString)),
        };

    // The ONE table read: the measure-family guard and the row lookup travel together, so a caller cannot resolve a
    // dimension for a value the SELECT never admitted.
    static Option<Dimension> Signature(IfcValue? value) =>
        value is IfcMeasureValue or IfcDerivedMeasureValue && MeasureDimensions.TryGetValue(value.GetType().Name, out Dimension row)
            ? Some(row)
            : None;

    // --- [MEASURE_ADMISSION]

    // The ONE native->SI entry, seam-whole: the regime coercion AND the per-VALUE carrier override (IFC declares a
    // unit on the property or quantity itself, so a Pset row authored in kN inside a newton-declared model reads its
    // own declaration) both ride the seam Coerce — Some(declared) overrides the regime whole-quantity, covering the
    // IfcConversionBasedUnitWithOffset affine and the IfcDerivedUnit whole-quantity multiplier alike. Bim holds ZERO
    // magnitude arithmetic: every factor, offset, and composition executes at the seam owner.
    internal static double Coerce(UnitScheme scheme, double native, string measureType, Dimension dimension, IfcUnit? declared) =>
        scheme.Coerce(native, Elect(measureType), dimension, IfcUnits.AxisOf(declared));

    static QuantityType Elect(string measureType) =>
        Angular.Contains(measureType) ? QuantityType.Angle : QuantityType.Create(measureType);

    static Fin<MeasureValue> MeasureOf(IfcValue measure, Dimension dimension, UnitScheme scheme, IfcUnit? declared, Op key) =>
        MeasureValue.OfSi(QuantityType.Create(measure.GetType().Name), dimension,
            Coerce(scheme, AsDouble(measure.Value), measure.GetType().Name, dimension, declared), key: key);

    static Fin<Option<MeasureValue>> MeasureOpt(IfcValue? value, UnitScheme scheme, IfcUnit? declared, Op key) =>
        Signature(value).Match(
            Some: row => MeasureOf(value!, row, scheme, declared, key).Map(Some),
            None: static () => Fin.Succ(Option<MeasureValue>.None));

    // IFC4.3's real-valued tally, which the registry names no quantity for — the OPEN QuantityType.Create mint the
    // seam sanctions, declared ONCE here so the ingress stamp and the Projection/egress raiser row read one spelling
    // and an IfcQuantityNumber never re-emits as the integral IfcQuantityCount.
    internal static readonly QuantityType Number = QuantityType.Create("Number");

    // The IfcQuantity* subtype -> its QTO quantity-type identity: the IFC-schema correspondence, and the ONLY fact
    // the subtype decides. The GG roster is these SEVEN concretes. INTERNAL, not private: the Projection/raise
    // mint table resolves each concrete's own ctor off these KEYS, so the ingress stamp and the egress raiser read
    // one roster and a hand-kept raiser table cannot drift from it.
    internal static readonly FrozenDictionary<Type, QuantityType> QuantityTypes = new Dictionary<Type, QuantityType> {
        [typeof(IfcQuantityLength)] = QuantityType.Length, [typeof(IfcQuantityArea)] = QuantityType.Area,
        [typeof(IfcQuantityVolume)] = QuantityType.Volume, [typeof(IfcQuantityWeight)] = QuantityType.Mass,
        [typeof(IfcQuantityTime)] = QuantityType.Duration, [typeof(IfcQuantityCount)] = QuantityType.Count,
        [typeof(IfcQuantityNumber)] = Number,
    }.ToFrozenDictionary();

    // An IfcPhysicalSimpleQuantity -> the seam MeasureValue [H2]: the magnitude and its dimension come off the
    // base's OWN polymorphic IfcMeasureValue read resolved through the SAME table the property lane reads, so ONE
    // construction and ONE Coerce serve both lanes and the seven per-subtype value-property spellings never fan
    // into seven constructions. An unrostered simple quantity faults typed, never a fabricated zero.
    public static Fin<MeasureValue> Measure(IfcPhysicalSimpleQuantity quantity, UnitScheme scheme, Op key) =>
        QuantityTypes.TryGetValue(quantity.GetType(), out QuantityType? qto) && Signature(quantity.MeasureValue).Case is Dimension row
            ? MeasureValue.OfSi(qto, row,
                Coerce(scheme, AsDouble(quantity.MeasureValue.Value), quantity.MeasureValue.GetType().Name, row, quantity.Unit), key: key)
            : Fin.Fail<MeasureValue>(new BimFault.Refused(key, BimScope.Projection, BimReason.Codec, string.Join(':', new object?[] { "quantity-kind-unmapped", quantity.GetType().Name })));

    // --- [BOUNDARY_ADMISSION]

    // The ONE GG string admission: GeometryGym backs every optional string with an EMPTY-STRING default rather than
    // a null, so blank IS absence and lifts to None here — carrying "" past this read would re-author a qualifier
    // the source file never wrote, and coalescing it inside a domain body puts the same decision at every site.
    internal static Option<string> Stated(string? value) => string.IsNullOrEmpty(value) ? None : Some(value);

    static PropertyName RowName(IfcProperty property) => PropertyName.Create(Stated(property.Name).IfNone(""));

    static string ResourceAnchor(IfcPropertyReferenceValue rv) =>
        Stated(rv.UsageName).IfNone(() => Stated(rv.PropertyReference?.GetType().Name).IfNone(""));

    static NodeId ResourceId(IfcPropertyReferenceValue rv) =>
        NodeId.Of(new NodeSeed.Precomputed(ContentAddress.Of(Encoding.UTF8.GetBytes(
            rv.PropertyReference is IfcRoot root
                ? $"ifcroot:{root.GlobalId}"
                : $"{Stated(rv.PropertyReference?.GetType().Name).IfNone("")}:{Stated(rv.UsageName).IfNone("")}"))));

    // The three-valued IfcLogical -> the seam Logical's Option<bool>: UNKNOWN is None, the third state a bool
    // cannot model; the egress RaiseLogical reverses it.
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

    // A magnitude GG boxes as something no numeric conversion reaches is ABSENT, and absence spells NaN so the seam
    // OfSi finite gate refuses it on the rail this method already returns. The guard is unreachable for every
    // rostered measure type — GG boxes each as a numeric — which is exactly why a silent 0.0 would never be caught
    // by a run: the arm fires only when the package's own storage changes.
    static double AsDouble(object? value) =>
        value is IConvertible convertible ? Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture) : double.NaN;
}
```

## [04]-[RESEARCH]

(none)
