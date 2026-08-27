# [COMPUTE_QUANTITIES]

UnitsNet boundary for measured execution: the frozen `QuantityFamily` rows admit every unit-bearing input exactly once, canonicalize through `As`/`ToUnit`, and emit the contract `Rasm.Element/Properties/quantity#UNIT_SCHEME` `MeasureEvidence` conversion result, while interior numerics stay raw doubles owned by Rasm core and no quantity type ever crosses an interior signature or a wire. Owned here: the `QuantityFamily` axis with its canonical, display, declared-dimension, and tolerance-class columns, the `UnitsEdge` boundary lifting every non-throwing UnitsNet verb to `Option`, the SI dimensional consistency law, and the culture-scoped parse and format edges.

Every Compute execution path admitting unit-bearing text — a solver tolerance, a quadrature step, a sampling rate — canonicalizes through these rows. `QuantityFamily` crosses NO wire: the contract `MeasureEvidence` result is the widest projection this owner publishes, it stays in-host, and no peer branch decodes it — a cross-runtime SI-scalar mirror is a boundary that would have to be declared at both ends before this owner spells a payload for it. AEC-domain folders own their unit admission IN-FOLDER — the strata graph is acyclic, so `Rasm.Fabrication/Process` admits its cut-parameter text at `RemovalParameter.Admit` and `Rasm.Materials/Appearance` coerces its photometric illuminance at its own boundary, each spelling UnitsNet in-folder rather than reaching DOWN to this owner; a `Rasm.Compute` reference from an AEC folder is the forbidden downward edge this owner never invites. Spine: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core over the settled configuration pipeline.

## [01]-[INDEX]

- [02]-[QUANTITY_TABLE]: frozen quantity rows with their declared SI dimensions; conversion exactly once at admission; relative-with-floor equivalence.
- [03]-[DIMENSIONAL_LAW]: total per-row declaration proof, cross-row compound identities, and the SI baseline policy row.
- [04]-[PARSE_FORMAT]: culture-scoped parse and format edges; the contract `MeasureEvidence` result and its family-indexed render.

## [02]-[QUANTITY_TABLE]

- Owner: `QuantityFamily` `[SmartEnum<string>]` rows, keyed ordinal-ignore-case through the shipped `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `UnitToken` the admitted unit abbreviation and `Tolerance` the three measured floor classes; `UnitsEdge` the ONE place a UnitsNet `Try*` out-param becomes an `Option`; `UnitMetadata` the metadata-sourcing owner over `QuantityInfo.BaseUnitInfo`/`UnitInfos`/`UnitType`.
- Cases: length, area, volume, mass, duration, speed, acceleration, force, pressure, energy, power, temperature, angle, torque, ratio, density, area-moment-of-inertia, heat-transfer-coefficient, thermal-resistance, illuminance, rotational-speed, frequency, thermal-conductivity, volume-flow, irradiance, luminance, luminous-flux, luminous-intensity — each row carries `QuantityInfo` metadata, a `BaseUnitInfo.Value`-sourced canonical unit, an explicit display unit defaulting to canonical, its own declared SI base-dimension vector, a `QuantityInfo.Name`-sourced contract `QuantityType` identity, and the `Tolerance` class the `Equivalent` proof reads; `Frequency` admits the `Stats/signal` modal and `Analysis/capacity` dynamic frequencies (Hz, distinct from the rev/min `RotationalSpeed`), `ThermalConductivity` the `Analysis/physics` material-λ inputs, `VolumeFlow` the `Analysis/energy` ventilation rates, and `Irradiance`/`Luminance`/`LuminousFlux`/`LuminousIntensity` the `Analysis/daylight` solar and sky-model photometrics beside the existing `Illuminance` row.
- Law: tolerance is ONE value carrying both terms and the compare that reads them — a relative term deciding at every scale and an absolute floor deciding near zero, compared as `|a-b| <= max(floor, relative * max(|a|,|b|))` on the SI canonical scalars. Lone absolutes read as exact equality on a metre-scale length and as everything-equal on a joule-scale energy, so the measured absolutes become floors and the relative term carries the compare. The floors are three CLASSES — `Metric` engineering scale, `Sensed` instrument scale, `Sectional` for the fourth-power and volumetric rows whose SI magnitudes are already small — so a row elects a class and a re-measured class moves every row that shares it; twenty-eight magnitudes spelled per row were one classification wearing a literal.
- Law: the canonical `Enum` column reads `QuantityInfo.BaseUnitInfo.Value` once per row at static construction — the metadata IS the column, so a hand-passed canonical arg is deleted, and UnitsNet emits no `DefaultUnitAttribute`/`DisplayAsUnitAttribute` on the generated types, so attribute reflection over them resolves to nothing; the display column is the explicit presentation `Enum` defaulting to canonical. `Probe()` is one of the four composition-time drift CENSUS legs — a row whose display `Enum` does not belong to its `QuantityInfo.UnitType` (a cross-family typo such as a `PressureUnit` on the `Length` row) surfaces its key there and `UnitAlgebra.Consistency` mints the arm, so the four legs report together and none re-wraps a fault another leg already minted; the canonical column needs no such census because it is read FROM `BaseUnitInfo.Value` and is type-correct by construction.
- Entry: `Admit(QuantityInput, UnitPolicy, CorrelationId)` — `Fin<MeasureEvidence>` aborts; the `QuantityInput` `[Union]` discriminates typed quantity, text, value-plus-unit, and value-plus-abbreviation payloads through one generated total `Switch`, and that case IS the result's `UnitResolution` — `Typed`/`UnitValue`/`Abbreviated` name a unit the caller DECLARED, `Text` a unit the parse INFERRED out of the glyph run; the correlation is the corpus-wide typed `CorrelationId` the admission spine threads (`AdmittedIntent.Correlation`), never a bare `Guid`, so `UnitProject` intents enter `Admit` carrying the identity the `Runtime/admission` path already minted. `Equivalent(IQuantity, IQuantity)` is `Fin<bool>` — family membership proves before the compare, so a cross-family pair is a `DimensionMismatch` and never a `false` a caller reads as a measured difference — and the compare itself is the row's `Tolerance`, run on the SI canonical scalars; `Aggregate(Seq<IQuantity>, AggregateOp, UnitPolicy, CorrelationId)` folds a same-family sequence at the canonical unit through the `AggregateOp` row's `UnitMath` delegate and re-enters `Admit`, so aggregates and single values ride one evidence result, and an empty sequence REFUSES because `Min`/`Max` have no identity a family zero could seed.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Option`, `Validation`, `Seq`), Rasm (project, kernel signal capsule — the typed `CorrelationId`), Rasm.Element (project — `MeasureEvidence` the ONE conversion result, `UnitResolution` its resolution vocabulary, `QuantityType` the family identity each row mints from its own `QuantityInfo.Name`, `Dimension` the ℤ⁷ carrier each row declares into), `Symbolic/expression` (in-branch — `Captured` the one foreign-throw funnel, `Finite` the admitted scalar), `Symbolic/dimensional` (in-branch — `DimensionMonomial.From`/`ToContract`, the one `BaseDimensions` transposition)
- Growth: one table row on `QuantityFamily` per further AEC quantity, admitted only when a consumer exists — its canonical column sourcing from `QuantityInfo.BaseUnitInfo.Value`, its contract `Type` from `QuantityInfo.Name`, and its display column an explicit per-row `Enum`; each row declaring its own SI dimension so the composition fold covers it with no edit at `DIMENSIONAL_LAW`; the rows close the families the current symbolic, solver, and analysis consumers admit, so a speculative row with no consumer is the rejected addition; zero new surface.
- Boundary: conversion runs exactly once at admission and interior numerics are raw doubles owned by Rasm core — a quantity type in an interior signature is the boundary violation this table deletes. Absence crosses the UnitsNet edge as `Option` and never as a null, so no interior fold owns a null test. Refusals ride the direct `ComputeFault` arms `Symbolic/expression` owns: `ParseRejected`, `DimensionMismatch`, and `SymbolUndefined` retain distinct numeric identities. `UnitsNetSetup.Default` is the single setup root composed once at the composition root: its `UnitParser`, `UnitAbbreviations`, `QuantityParser`, and `UnitConverter` properties are the reads this page spells, because the `UnitParser.Default`/`UnitAbbreviationsCache.Default`/`UnitConverter.Default` facades are shortcuts FORWARDING to that root and a second spelling of one instance is drift waiting for a second setup. One exception survives: `UnitConverter.Convert`/`TryConvert` stay static reads because the root's converter publishes no instance twin, and a second setup instance is rejected. NodaTime owns interior time, so the duration row exists only to canonicalize boundary text to seconds before interior time takes over. `UnitProject` intents enter `Admit` and the `Pipeline` intent case composes it.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct UnitToken {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { "<blank-unit-token>" }));
        }
    }
}

[ComplexValueObject]
public sealed partial class Tolerance {
    public static readonly Tolerance Metric = Create(floor: 1e-9, relative: 1e-12);
    public static readonly Tolerance Sensed = Create(floor: 1e-6, relative: 1e-12);
    public static readonly Tolerance Sectional = Create(floor: 1e-12, relative: 1e-12);

    public double Floor { get; }

    public double Relative { get; }

    public bool Equivalent(double left, double right) =>
        Math.Abs(left - right) <= Math.Max(Floor, Relative * Math.Max(Math.Abs(left), Math.Abs(right)));
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
internal static class UnitsEdge {
    public static Option<IQuantity> Parse(UnitPolicy policy, Type valueType, string text) =>
        Quantity.TryParse(policy.Culture, valueType, text, out IQuantity? parsed) ? Optional(parsed) : None;

    public static Option<IQuantity> From(double value, Enum unit) =>
        Quantity.TryFrom(value, unit, out IQuantity? typed) ? Optional(typed) : None;

    public static Option<IQuantity> FromAbbreviation(UnitPolicy policy, double value, UnitToken abbreviation) =>
        Quantity.TryFromUnitAbbreviation(policy.Culture, value, abbreviation.ToValue(), out IQuantity? typed) ? Optional(typed) : None;

    public static Option<Enum> Unit(UnitToken abbreviation, Type unitType, UnitPolicy policy) =>
        UnitsNetSetup.Default.UnitParser.TryParse(abbreviation.ToValue(), unitType, policy.Culture, out Enum? resolved) ? Optional(resolved) : None;

    public static Option<double> Convert(double value, Enum from, Enum to) =>
        UnitConverter.TryConvert(value, from, to, out double converted) && double.IsFinite(converted) ? Some(converted) : None;
}

internal static class UnitMetadata {
    public static Seq<UnitInfo> ConvertTargets(QuantityInfo info) =>
        toSeq(info.UnitInfos);

    public static Seq<string> Probe() =>
        toSeq(QuantityFamily.Items)
            .Filter(static row => row.Display.GetType() != row.Info.UnitType)
            .Map(static row => row.Key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class QuantityFamily {
    public static readonly QuantityFamily Length = new("length", UnitsNet.Length.Info, dimension: [1, 0, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric, display: LengthUnit.Millimeter);
    public static readonly QuantityFamily Area = new("area", UnitsNet.Area.Info, dimension: [2, 0, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Volume = new("volume", UnitsNet.Volume.Info, dimension: [3, 0, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Mass = new("mass", UnitsNet.Mass.Info, dimension: [0, 1, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Duration = new("duration", UnitsNet.Duration.Info, dimension: [0, 0, 1, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Speed = new("speed", UnitsNet.Speed.Info, dimension: [1, 0, -1, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Acceleration = new("acceleration", UnitsNet.Acceleration.Info, dimension: [1, 0, -2, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Force = new("force", UnitsNet.Force.Info, dimension: [1, 1, -2, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Pressure = new("pressure", UnitsNet.Pressure.Info, dimension: [-1, 1, -2, 0, 0, 0, 0], tolerance: Tolerance.Metric, display: PressureUnit.Kilopascal);
    public static readonly QuantityFamily Energy = new("energy", UnitsNet.Energy.Info, dimension: [2, 1, -2, 0, 0, 0, 0], tolerance: Tolerance.Metric, display: EnergyUnit.KilowattHour);
    public static readonly QuantityFamily Power = new("power", UnitsNet.Power.Info, dimension: [2, 1, -3, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Temperature = new("temperature", UnitsNet.Temperature.Info, dimension: [0, 0, 0, 0, 1, 0, 0], tolerance: Tolerance.Sensed, display: TemperatureUnit.DegreeCelsius);
    public static readonly QuantityFamily Angle = new("angle", UnitsNet.Angle.Info, dimension: [0, 0, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Torque = new("torque", UnitsNet.Torque.Info, dimension: [2, 1, -2, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Ratio = new("ratio", UnitsNet.Ratio.Info, dimension: [0, 0, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric, display: RatioUnit.Percent);
    public static readonly QuantityFamily Density = new("density", UnitsNet.Density.Info, dimension: [-3, 1, 0, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily AreaMomentOfInertia = new("area-moment-of-inertia", UnitsNet.AreaMomentOfInertia.Info, dimension: [4, 0, 0, 0, 0, 0, 0], tolerance: Tolerance.Sectional);
    public static readonly QuantityFamily HeatTransferCoefficient = new("heat-transfer-coefficient", UnitsNet.HeatTransferCoefficient.Info, dimension: [0, 1, -3, 0, -1, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily ThermalResistance = new("thermal-resistance", UnitsNet.ThermalResistance.Info, dimension: [0, -1, 3, 0, 1, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily Illuminance = new("illuminance", UnitsNet.Illuminance.Info, dimension: [-2, 0, 0, 0, 0, 0, 1], tolerance: Tolerance.Sensed);
    public static readonly QuantityFamily RotationalSpeed = new("rotational-speed", UnitsNet.RotationalSpeed.Info, dimension: [0, 0, -1, 0, 0, 0, 0], tolerance: Tolerance.Metric, display: RotationalSpeedUnit.RevolutionPerMinute);
    public static readonly QuantityFamily Frequency = new("frequency", UnitsNet.Frequency.Info, dimension: [0, 0, -1, 0, 0, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily ThermalConductivity = new("thermal-conductivity", UnitsNet.ThermalConductivity.Info, dimension: [1, 1, -3, 0, -1, 0, 0], tolerance: Tolerance.Metric);
    public static readonly QuantityFamily VolumeFlow = new("volume-flow", UnitsNet.VolumeFlow.Info, dimension: [3, 0, -1, 0, 0, 0, 0], tolerance: Tolerance.Sectional);
    public static readonly QuantityFamily Irradiance = new("irradiance", UnitsNet.Irradiance.Info, dimension: [0, 1, -3, 0, 0, 0, 0], tolerance: Tolerance.Sensed);
    public static readonly QuantityFamily Luminance = new("luminance", UnitsNet.Luminance.Info, dimension: [-2, 0, 0, 0, 0, 0, 1], tolerance: Tolerance.Sensed);
    public static readonly QuantityFamily LuminousFlux = new("luminous-flux", UnitsNet.LuminousFlux.Info, dimension: [0, 0, 0, 0, 0, 0, 1], tolerance: Tolerance.Sensed);
    public static readonly QuantityFamily LuminousIntensity = new("luminous-intensity", UnitsNet.LuminousIntensity.Info, dimension: [0, 0, 0, 0, 0, 0, 1], tolerance: Tolerance.Sensed);

    public QuantityInfo Info { get; }

    public Dimension Dimension { get; }

    public Enum Canonical { get; }

    public Enum Display { get; }

    public QuantityType Type { get; }

    public Tolerance Tolerance { get; }

    private QuantityFamily(string key, QuantityInfo info, ImmutableArray<int> dimension, Tolerance tolerance, Enum? display = null) : this() {
        Info = info;
        Dimension = Dimension.Create(dimension[0], dimension[1], dimension[2], dimension[3], dimension[4], dimension[5], dimension[6]);
        Canonical = info.BaseUnitInfo.Value;
        Display = display ?? Canonical;
        Tolerance = tolerance;
        Type = QuantityType.Create(info.Name);
    }

    public Fin<MeasureEvidence> Admit(QuantityInput input, UnitPolicy policy, CorrelationId correlation) =>
        Captured.Of(() => input.Switch(
                state: (Row: this, Policy: policy, Correlation: correlation),
                typed: static (value, state) => state.Row.AdmitQuantity(value.Value, UnitResolution.Declared, state.Correlation),
                text: static (value, state) => UnitsEdge.Parse(state.Policy, state.Row.Info.ValueType, value.Value.ToValue())
                    .ToFin(new ComputeFault.ParseRejected($"<unit-text:{state.Row.Key}:{value.Value.ToValue()}>"))
                    .Bind(parsed => state.Row.AdmitQuantity(parsed, UnitResolution.Inferred, state.Correlation)),
                unitValue: static (value, state) => UnitsEdge.From(value.Value.ToValue(), value.Unit)
                    .ToFin(new ComputeFault.ParseRejected($"<unit-declared:{state.Row.Key}:{value.Unit}>"))
                    .Bind(typed => state.Row.AdmitQuantity(typed, UnitResolution.Declared, state.Correlation)),
                abbreviated: static (value, state) => UnitsEdge.FromAbbreviation(state.Policy, value.Value.ToValue(), value.Unit)
                    .ToFin(new ComputeFault.ParseRejected($"<unit-abbreviation:{state.Row.Key}:{value.Unit.ToValue()}>"))
                    .Bind(typed => state.Row.AdmitQuantity(typed, UnitResolution.Declared, state.Correlation))));

    public Fin<MeasureEvidence> Aggregate(Seq<IQuantity> parts, AggregateOp op, UnitPolicy policy, CorrelationId correlation) =>
        parts.IsEmpty
            ? Fin.Fail<MeasureEvidence>(new ComputeFault.ParseRejected($"<unit-aggregate-empty:{Key}>"))
            : parts.Filter(part => part.QuantityInfo.Name != Info.Name).Map(static part => part.QuantityInfo.Name) is { IsEmpty: false } foreign
                ? Fin.Fail<MeasureEvidence>(new ComputeFault.DimensionMismatch($"<unit-aggregate-foreign:{Key}:{string.Join(",", foreign)}>"))
                : Captured.Of(() => Admit(new QuantityInput.Typed(op.Fold(parts, Canonical)), policy, correlation));

    public Option<Enum> Resolve(UnitToken unit, UnitPolicy policy) =>
        UnitsEdge.Unit(unit, Info.UnitType, policy);

    public Fin<string> Render(double canonicalValue, UnitPolicy policy, Option<Enum> target = default) =>
        Captured.Of(() => target.IfNone(Display) is Enum resolved && double.IsFinite(canonicalValue) && resolved.GetType() == Info.UnitType
                ? Fin.Succ(((IFormattable)Quantity.From(canonicalValue, Canonical).ToUnit(resolved)).ToString(policy.Format.ToValue(), policy.Culture))
                : Fin.Fail<string>(new ComputeFault.ParseRejected($"<unit-render-target:{Key}:{Info.Name}>")));

    public Fin<bool> Equivalent(IQuantity left, IQuantity right) =>
        !left.QuantityInfo.BaseDimensions.Equals(Info.BaseDimensions) || !right.QuantityInfo.BaseDimensions.Equals(Info.BaseDimensions)
            ? Fin.Fail<bool>(new ComputeFault.DimensionMismatch(
                $"<unit-equivalent-foreign:{Key}:{left.QuantityInfo.Name},{right.QuantityInfo.Name}>"))
            : Captured.Of(() => (left.As(Canonical), right.As(Canonical)) is var (a, b) && double.IsFinite(a) && double.IsFinite(b)
                    ? Fin.Succ(Tolerance.Equivalent(a, b))
                    : Fin.Fail<bool>(new ComputeFault.SymbolUndefined($"<unit-equivalent-nonfinite:{Key}>")));

    Fin<MeasureEvidence> AdmitQuantity(IQuantity quantity, UnitResolution resolution, CorrelationId correlation) =>
        Captured.Of(() =>
            quantity.QuantityInfo.Name != Info.Name || !quantity.Dimensions.Equals(Info.BaseDimensions)
                ? Fin.Fail<MeasureEvidence>(new ComputeFault.DimensionMismatch($"<unit-out-of-family:{Key}:{quantity.QuantityInfo.Name}>"))
            : !double.IsFinite(quantity.As(Canonical))
                ? Fin.Fail<MeasureEvidence>(new ComputeFault.SymbolUndefined($"<unit-nonfinite:{Key}>"))
            : Fin.Succ(Evidence(quantity, resolution, correlation)));

    MeasureEvidence Evidence(IQuantity quantity, UnitResolution resolution, CorrelationId correlation) =>
        new(Type, quantity.Unit.ToString(), (double)quantity.Value,
            Canonical.ToString(), quantity.As(Canonical), resolution, correlation);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QuantityInput {
    private QuantityInput() { }

    public sealed record Typed(IQuantity Value) : QuantityInput;
    public sealed record Text(UnitToken Value) : QuantityInput;
    public sealed record UnitValue(Finite Value, Enum Unit) : QuantityInput;
    public sealed record Abbreviated(Finite Value, UnitToken Unit) : QuantityInput;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class AggregateOp {
    public static readonly AggregateOp Sum = new("sum", static (parts, unit) => UnitMath.Sum(parts, unit));
    public static readonly AggregateOp Min = new("min", static (parts, unit) => UnitMath.Min(parts, unit));
    public static readonly AggregateOp Max = new("max", static (parts, unit) => UnitMath.Max(parts, unit));
    public static readonly AggregateOp Average = new("average", static (parts, unit) => UnitMath.Average(parts, unit));

    [UseDelegateFromConstructor]
    public partial IQuantity Fold(Seq<IQuantity> parts, Enum unit);
}
```

## [03]-[DIMENSIONAL_LAW]

- Owner: `UnitPolicy` the configuration-bound `[ComplexValueObject]` admitting culture and `FormatSpec` once; `UnitAlgebra` static dimensional surface.
- Cases: every admitted row proves its OWN declared dimension against `QuantityInfo.BaseDimensions` in one fold over `Items`; speed, acceleration, force, pressure, energy, power, torque, density — compound CROSS-ROW relations, each verifying the composed `BaseDimensions` of its two factors equals the compound row's; one reciprocal-pair row (heat-transfer-coefficient · thermal-resistance is dimensionless).
- Law: the two legs are disjoint by construction. Per-row soundness is TOTAL — the fold visits every `Items` member, so a row bound to the wrong `QuantityInfo` (a `speed` key pointing at `Acceleration.Info`) drifts at composition no matter how young the row is, and it needs no hand-written entry to be covered. Hand relations survive ONLY where a statement spans two or more rows and no single row's declaration carries it: `pressure = force / area` claims three rows at once. Self-only hand rows — a dimensionless list naming `ratio` — restate what that row's own declaration already proves and are the deleted form.
- Law: `UnitMath` owns explicit-unit sequence folds and pairwise arithmetic, coercing through `As(unit)` at the family canonical. `AggregateOp` exposes those folds through `QuantityFamily.Aggregate`, which re-enters the one `MeasureEvidence` result.
- Entry: `Consistency()` — `Fin<Unit>` at the boundary over an ACCUMULATING interior: the declaration census, the cross-row relation census, the reciprocal census, and `UnitMetadata.Probe()` are four independent proofs, none informing the next, so they join through one applicative and exit once through `ToFin()`; a consumer therefore reads a declaration drift, a relation drift, a reciprocal drift, and a display-unit drift apart, which a concatenated key string could not tell.
- Packages: UnitsNet, LanguageExt.Core (`Validation` applicative, `ToFin`, `Option`, `Seq`), Thinktecture.Runtime.Extensions (`[ComplexValueObject]`/`[ValueObject<string>]`), `Symbolic/dimensional` (in-branch — the seated `BaseDimensions` transposition), BCL inbox
- Growth: a new admitted family is one `QuantityFamily` row carrying its own dimension declaration, which the total fold covers with zero edit here; a new cross-row identity is one row in `Relations` or `Reciprocals`; zero new surface.
- Boundary: `UnitPolicy` binds at `Section` through the configuration pipeline and admits the resolved `CultureInfo` and `FormatSpec` ONCE, so no downstream fold re-tests a null culture or a blank format; a `Baseline` pinning `UnitSystem.SI` with no reader was decoration and is deleted — an `As(UnitSystem)` route is a row this owner adds when a consumer names it. Admission checks `Dimensions.Equals(Info.BaseDimensions)`, and `Consistency` proves declared, compound, and reciprocal claims through UnitsNet `BaseDimensions`, the declared leg comparing contract `Dimension` values so no local transposition exists to fall out of step. Numeric-only conversion rides `UnitsEdge.Convert` over the static `UnitConverter.TryConvert` — the one converter verb the setup root publishes no instance twin of — without constructing an `IQuantity`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class UnitPolicy {
    public const string Section = nameof(UnitPolicy);

    public CultureInfo Culture { get; }

    public FormatSpec Format { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref CultureInfo culture, ref FormatSpec format) {
        if (culture is null) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { "<unit-policy-culture>" }));
        }
    }
}

[ValueObject<string>]
public readonly partial struct FormatSpec {
    public static readonly FormatSpec General = Create("G");

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { "<unit-policy-format>" }));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UnitAlgebra {
    private static readonly Func<BaseDimensions, BaseDimensions, BaseDimensions> Product = static (left, right) => left.Multiply(right);

    private static readonly Func<BaseDimensions, BaseDimensions, BaseDimensions> Quotient = static (left, right) => left.Divide(right);

    public static readonly Seq<(QuantityFamily Compound, QuantityFamily Left, QuantityFamily Right, Func<BaseDimensions, BaseDimensions, BaseDimensions> Compose)> Relations = Seq(
        (QuantityFamily.Speed, QuantityFamily.Length, QuantityFamily.Duration, Quotient),
        (QuantityFamily.Acceleration, QuantityFamily.Speed, QuantityFamily.Duration, Quotient),
        (QuantityFamily.Force, QuantityFamily.Mass, QuantityFamily.Acceleration, Product),
        (QuantityFamily.Pressure, QuantityFamily.Force, QuantityFamily.Area, Quotient),
        (QuantityFamily.Energy, QuantityFamily.Force, QuantityFamily.Length, Product),
        (QuantityFamily.Power, QuantityFamily.Energy, QuantityFamily.Duration, Quotient),
        (QuantityFamily.Torque, QuantityFamily.Force, QuantityFamily.Length, Product),
        (QuantityFamily.Density, QuantityFamily.Mass, QuantityFamily.Volume, Quotient));

    public static readonly Seq<(QuantityFamily Left, QuantityFamily Right)> Reciprocals = Seq(
        (QuantityFamily.HeatTransferCoefficient, QuantityFamily.ThermalResistance));

    public static Fin<Unit> Consistency() =>
        (Leg(Declared(), static keys => new ComputeFault.DimensionMismatch($"<declared-drift:{keys}>")),
         Leg(Compound(), static keys => new ComputeFault.DimensionMismatch($"<relation-drift:{keys}>")),
         Leg(Reciprocal(), static keys => new ComputeFault.DimensionMismatch($"<reciprocal-drift:{keys}>")),
         Leg(UnitMetadata.Probe(), static keys => new ComputeFault.ParseRejected($"<display-outside-unit-type:{keys}>")))
            .Apply(static (_, _, _, _) => unit)
            .As()
            .ToFin();

    static Validation<Error, Unit> Leg(Seq<string> drift, Func<string, ComputeFault> arm) =>
        drift.IsEmpty
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(arm(string.Join(",", drift)));

    static Seq<string> Declared() =>
        toSeq(QuantityFamily.Items)
            .Filter(static row => DimensionMonomial.From(row.Info.BaseDimensions).ToContract() != Some(row.Dimension))
            .Map(static row => row.Key);

    static Seq<string> Compound() =>
        Relations
            .Filter(static row => !row.Compose(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).Equals(row.Compound.Info.BaseDimensions))
            .Map(static row => row.Compound.Key);

    static Seq<string> Reciprocal() =>
        Reciprocals
            .Filter(static row => !Product(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).IsDimensionless())
            .Map(static row => row.Left.Key);

    public static Fin<double> Numeric(Finite value, Enum from, Enum to) =>
        UnitsEdge.Convert(value.ToValue(), from, to)
            .ToFin(new ComputeFault.ParseRejected($"<unit-convert-unsupported:{from}->{to}>"));
}
```

## [04]-[PARSE_FORMAT]

- Owner: the projection half of `QuantityFamily` — the `ByType` contract-identity index, its `Of(QuantityType)` inverse, `Catalogue()`, the `Targets` picker, and the result-shaped `Render`.
- Law: `UnitFormatter` is `internal`, so the consumable bare-unit surface is the abbreviation cache; `Targets(UnitPolicy)` IS the metadata-driven picker — it holds each conversion target's `UnitInfo` from `ConvertTargets` and resolves the glyph and the full alias set through `UnitsNetSetup.Default.UnitAbbreviations.GetAbbreviations(UnitInfo, policy.Culture)`, the `UnitInfo`-keyed overload, because the generic `GetDefaultAbbreviation<TUnit>`/`GetUnitAbbreviations<TUnit>` cannot bind a `TUnit` over a runtime-enumerated unit. `Catalogue()` projects the `Info` rows a family chooser reads and `UnitMetadata.ConvertTargets` projects each family's `QuantityInfo.UnitInfos` as `Seq<UnitInfo>` (each carrying `.Value`/`.Name`/`.PluralName`), all with zero custom-attribute reflection — a picker described in prose with no fence behind it was a law with no producer.
- Law: the contract result's own fields are a `QuantityType` token, plain strings and doubles, and two contract value objects, so it serializes through the package wire context while UnitsNet types never cross a JSON or proto wire — conversion-at-admission is what enforces the recorded UnitsNet-serialization SKIP.
- Entry: `QuantityFamily.Render(MeasureEvidence, UnitPolicy, Option<Enum>)` — total display projection; formatting never round-trips into computation.
- Result: `MeasureEvidence` carries the `QuantityType` family, original unit and value, canonical unit and value, `UnitResolution`, and correlation id directly.
- Packages: UnitsNet (`QuantityInfo`, `UnitInfo`, `UnitsNetSetup.Default.UnitAbbreviations.GetAbbreviations(UnitInfo, CultureInfo)`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Map`, `Seq`, `Lazy`-backed index), Rasm.Element (project — `MeasureEvidence`, `UnitResolution`, `QuantityType`)
- Growth: one `QuantityInfo` catalogue row per admitted family — the dashboard quantity picker derives from `Catalogue()` and its unit list from `Targets`; zero new surface.
- Boundary: boundary text parses culture-scoped through `UnitsEdge.Parse` over `Quantity.TryParse(policy.Culture, Info.ValueType, ...)`, with `Resolve` owning abbreviation→`Enum` resolution through the same edge over `UnitsNetSetup.Default.UnitParser` and that root's `UnitAbbreviations`, whose lookup falls back to the invariant-culture abbreviation set when the policy culture lacks a localized one; both return `Option`, so an unresolvable abbreviation is absence at the boundary rather than a null the interior discovers. `Render` takes a `UnitProject` target unit as the resolved target override and renders through the boxed `IFormattable.ToString(format, culture)` face — the generic `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, …)` cannot bind a runtime-boxed `IQuantity`, so the boxed formattable face IS the dynamic rendering surface; the precision column is the format-string row carried on `UnitPolicy`, never a per-call-site `ToString` overload.

```csharp
public sealed partial class QuantityFamily {
    static readonly Lazy<Map<QuantityType, QuantityFamily>> ByType = new(static () =>
        toSeq(Items).Fold(Map<QuantityType, QuantityFamily>(), static (index, row) => index.Add(row.Type, row)));

    public static Fin<QuantityFamily> Of(QuantityType type) =>
        ByType.Value.Find(type)
            .ToFin(new ComputeFault.ParseRejected($"<unit-family-unknown:{type.ToValue()}>"));

    public static Seq<QuantityInfo> Catalogue() =>
        toSeq(Items).Map(static row => row.Info);

    public Seq<(UnitInfo Target, Seq<string> Aliases)> Targets(UnitPolicy policy) =>
        UnitMetadata.ConvertTargets(Info).Map(target =>
            (target, toSeq(UnitsNetSetup.Default.UnitAbbreviations.GetAbbreviations(target, policy.Culture))));

    public static Fin<string> Render(MeasureEvidence evidence, UnitPolicy policy, Option<Enum> target = default) =>
        Of(evidence.Family).Bind(row => row.Render(evidence.CanonicalValue, policy, target));
}
```
