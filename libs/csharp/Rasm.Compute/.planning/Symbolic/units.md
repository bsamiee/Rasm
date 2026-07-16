# [COMPUTE_QUANTITIES]

UnitsNet boundary for measured execution: the frozen `QuantityFamily` rows admit every unit-bearing input exactly once, canonicalize through `As`/`ToUnit`, and emit dual `UnitEvidence`, while interior numerics stay raw doubles owned by Rasm core and no quantity type ever crosses an interior signature or a wire. Owned here: the `QuantityFamily` axis with its canonical, display, and tolerance columns, the SI dimensional consistency law, and the culture-scoped parse and format edges.

Every Compute execution path admitting unit-bearing text — a solver tolerance, a quadrature step, a sampling rate — canonicalizes through these rows, and the host-free peers (Python `compute`, TypeScript `interchange`) decode the SI scalar this owner stamps onto `UnitEvidence`, never a UnitsNet type; the `CutParameterIngress` contract is the Compute-internal slot-keyed canonicalization an in-process or host-free-peer caller composes, not an export an AEC project references. AEC-domain folders own their unit admission IN-FOLDER — the strata graph is acyclic, so `Rasm.Fabrication/Process` admits its cut-parameter text at `RemovalParameter.Admit` and `Rasm.Materials/Appearance` coerces its photometric illuminance at its own boundary, each spelling UnitsNet in-folder rather than reaching DOWN to this owner; a `Rasm.Compute` reference from an AEC folder is the forbidden downward edge this owner never invites. Spine: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core over the settled configuration rail.

## [01]-[INDEX]

- [01]-[QUANTITY_TABLE]: frozen quantity rows; conversion exactly once at admission; tolerance-keyed equivalence.
- [02]-[DIMENSIONAL_LAW]: compound dimensional consistency and the SI baseline policy row.
- [03]-[PARSE_FORMAT]: culture-scoped parse and format edges; dual unit evidence.

## [02]-[QUANTITY_TABLE]

- Owner: `QuantityFamily` `[SmartEnum<string>]` rows, keyed ordinal-ignore-case through the shipped `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `UnitMetadata` the metadata-sourcing owner over `QuantityInfo.BaseUnitInfo`/`UnitInfos`/`UnitType`.
- Cases: length, area, volume, mass, duration, speed, acceleration, force, pressure, energy, power, temperature, angle, torque, ratio, density, area-moment-of-inertia, heat-transfer-coefficient, thermal-resistance, illuminance, rotational-speed, frequency, thermal-conductivity, volume-flow, irradiance, luminance, luminous-flux, luminous-intensity — each row carries `QuantityInfo` metadata, a `BaseUnitInfo.Value`-sourced canonical unit, an explicit display unit defaulting to canonical, and a tolerance column the `Equivalent` proof reads; the `Speed`, `RotationalSpeed`, and `Length` (depth) rows are the cut-parameter ingress vocabulary the `CutParameterIngress` contract canonicalizes for the Fabrication consumer; `Frequency` admits the `Stats/signal` modal and `Analysis/structural` dynamic frequencies (Hz, distinct from the rev/min `RotationalSpeed`), `ThermalConductivity` the `Analysis/physics` material-λ inputs, `VolumeFlow` the `Analysis/energy` ventilation rates, and `Irradiance`/`Luminance`/`LuminousFlux`/`LuminousIntensity` the `Analysis/daylight` solar and sky-model photometrics beside the existing `Illuminance` row.
- Entry: `Admit(QuantityInput, UnitPolicy, CorrelationId)` — `Fin<UnitEvidence>` aborts; the `QuantityInput` `[Union]` discriminates typed quantity, text, value-plus-unit, and value-plus-abbreviation payloads through one generated total `Switch`; the correlation is the corpus-wide typed `CorrelationId` the admission spine threads (`AdmittedIntent.Correlation`), never a bare `Guid`, so `UnitProject` intents enter `Admit` carrying the identity the `Runtime/admission` rail already minted. `Equivalent(IQuantity, IQuantity)` is the tolerance-keyed equivalence over the row's canonical scale; `Aggregate(Seq<IQuantity>, AggregateOp, UnitPolicy, CorrelationId)` folds a same-family sequence at the canonical unit through the `AggregateOp` row's `UnitMath` delegate and re-enters `Admit`, so aggregates and single values ride one evidence rail.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project — the typed `CorrelationId`)
- Growth: one table row on `QuantityFamily` per further AEC quantity, admitted only when a consumer exists — its canonical column sourcing from `QuantityInfo.BaseUnitInfo.Value` and its display column an explicit per-row `Enum`; the rows close the families the current symbolic, solver, and cut-parameter consumers admit, so a speculative row with no consumer is the rejected addition; zero new surface.
- Boundary: conversion runs exactly once at admission and interior numerics are raw doubles owned by Rasm core — a quantity type in an interior signature is the seam violation this table deletes. Unit-admission failures mint `ComputeFault` through the dual-tier `Create` text route on the 2200 code band, the units-boundary contribution to the intent-and-selection fault union. `UnitsNetSetup.Default` is the single setup root composed once at the composition root, with `UnitConverter` riding it and a second setup instance rejected. NodaTime owns interior time, so the duration row exists only to canonicalize boundary text to seconds before rail time takes over. `UnitProject` intents enter `Admit` and the `Pipeline` intent case composes it; the `CutParameterIngress` contract canonicalizes feed/surface-speed/depth/spindle text to an SI raw double for the same in-process and host-free-peer callers, never an AEC project over a reference.
- Metadata sourcing: the canonical `Enum` column reads `QuantityInfo.BaseUnitInfo.Value` once per row at static construction — the metadata IS the column, so a hand-passed canonical arg is deleted, and UnitsNet emits no `DefaultUnitAttribute`/`DisplayAsUnitAttribute` on the generated types, so attribute reflection over them resolves to nothing; the display column is the explicit presentation `Enum` defaulting to canonical. `Probe()` is the composition-time coherence guard — a row whose display `Enum` does not belong to its `QuantityInfo.UnitType` (a cross-family typo such as a `PressureUnit` on the `Length` row) drifts as a `ComputeFault` at composition rather than failing the first `Render`; the canonical column needs no such guard because it is read FROM `BaseUnitInfo.Value` and is type-correct by construction.

```csharp signature
// --- [BOUNDARIES] ----------------------------------------------------------------------
internal static class UnitMetadata {
    public static Seq<UnitInfo> ConvertTargets(QuantityInfo info) =>
        info.UnitInfos.ToSeq();

    public static Fin<Unit> Probe() =>
        QuantityFamily.Items.ToSeq()
            .Filter(static row => row.Display.GetType() != row.Info.UnitType)
            .Map(static row => row.Key) is { IsEmpty: false } drift
            ? ComputeFault.Create($"unit-metadata: {string.Join(", ", drift)} display unit outside family UnitType")
            : Fin.Succ(unit);
}

[SmartEnum<string>]
[ValidationError<ComputeFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class QuantityFamily {
    public static readonly QuantityFamily Length = new("length", UnitsNet.Length.Info, display: LengthUnit.Millimeter, tolerance: 1e-9);
    public static readonly QuantityFamily Area = new("area", UnitsNet.Area.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Volume = new("volume", UnitsNet.Volume.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Mass = new("mass", UnitsNet.Mass.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Duration = new("duration", UnitsNet.Duration.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Speed = new("speed", UnitsNet.Speed.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Acceleration = new("acceleration", UnitsNet.Acceleration.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Force = new("force", UnitsNet.Force.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Pressure = new("pressure", UnitsNet.Pressure.Info, display: PressureUnit.Kilopascal, tolerance: 1e-9);
    public static readonly QuantityFamily Energy = new("energy", UnitsNet.Energy.Info, display: EnergyUnit.KilowattHour, tolerance: 1e-9);
    public static readonly QuantityFamily Power = new("power", UnitsNet.Power.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Temperature = new("temperature", UnitsNet.Temperature.Info, display: TemperatureUnit.DegreeCelsius, tolerance: 1e-6);
    public static readonly QuantityFamily Angle = new("angle", UnitsNet.Angle.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Torque = new("torque", UnitsNet.Torque.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Ratio = new("ratio", UnitsNet.Ratio.Info, display: RatioUnit.Percent, tolerance: 1e-9);
    public static readonly QuantityFamily Density = new("density", UnitsNet.Density.Info, tolerance: 1e-9);
    public static readonly QuantityFamily AreaMomentOfInertia = new("area-moment-of-inertia", UnitsNet.AreaMomentOfInertia.Info, tolerance: 1e-12);
    public static readonly QuantityFamily HeatTransferCoefficient = new("heat-transfer-coefficient", UnitsNet.HeatTransferCoefficient.Info, tolerance: 1e-9);
    public static readonly QuantityFamily ThermalResistance = new("thermal-resistance", UnitsNet.ThermalResistance.Info, tolerance: 1e-9);
    public static readonly QuantityFamily Illuminance = new("illuminance", UnitsNet.Illuminance.Info, tolerance: 1e-6);
    public static readonly QuantityFamily RotationalSpeed = new("rotational-speed", UnitsNet.RotationalSpeed.Info, display: RotationalSpeedUnit.RevolutionPerMinute, tolerance: 1e-9);
    public static readonly QuantityFamily Frequency = new("frequency", UnitsNet.Frequency.Info, tolerance: 1e-9);
    public static readonly QuantityFamily ThermalConductivity = new("thermal-conductivity", UnitsNet.ThermalConductivity.Info, tolerance: 1e-9);
    public static readonly QuantityFamily VolumeFlow = new("volume-flow", UnitsNet.VolumeFlow.Info, tolerance: 1e-12);
    public static readonly QuantityFamily Irradiance = new("irradiance", UnitsNet.Irradiance.Info, tolerance: 1e-6);
    public static readonly QuantityFamily Luminance = new("luminance", UnitsNet.Luminance.Info, tolerance: 1e-6);
    public static readonly QuantityFamily LuminousFlux = new("luminous-flux", UnitsNet.LuminousFlux.Info, tolerance: 1e-6);
    public static readonly QuantityFamily LuminousIntensity = new("luminous-intensity", UnitsNet.LuminousIntensity.Info, tolerance: 1e-6);

    public QuantityInfo Info { get; }

    public Enum Canonical { get; }

    public Enum Display { get; }

    public double Tolerance { get; }

    private QuantityFamily(string key, QuantityInfo info, double tolerance, Enum? display = null) : this(key) {
        Info = info;
        Canonical = info.BaseUnitInfo.Value;
        Display = display ?? Canonical;
        Tolerance = tolerance;
    }

    public Fin<UnitEvidence> Admit(QuantityInput input, UnitPolicy policy, CorrelationId correlation) =>
        input is null || policy is null || policy.Culture is null || string.IsNullOrWhiteSpace(policy.Format)
            ? ComputeFault.Create($"unit-admission {Key}: null input or policy")
            : Try.lift<Fin<UnitEvidence>>(() => input.Switch(
                    state: (Row: this, Policy: policy, Correlation: correlation),
                    typed: static (value, state) => value.Value is not null
                        ? state.Row.AdmitQuantity(value.Value, state.Correlation)
                        : ComputeFault.Create($"unit-admission {state.Row.Key}: null typed quantity"),
                    text: static (value, state) => !string.IsNullOrWhiteSpace(value.Value)
                        && Quantity.TryParse(state.Policy.Culture, state.Row.Info.ValueType, value.Value, out IQuantity? parsed) && parsed is not null
                            ? state.Row.AdmitQuantity(parsed, state.Correlation)
                            : ComputeFault.Create($"unit-admission {state.Row.Key}: '{value.Value}' outside {state.Row.Info.Name}"),
                    unitValue: static (value, state) => value.Unit is not null && double.IsFinite(value.Value)
                        && Quantity.TryFrom(value.Value, value.Unit, out IQuantity? typed) && typed is not null
                            ? state.Row.AdmitQuantity(typed, state.Correlation)
                            : ComputeFault.Create($"unit-admission {state.Row.Key}: {value.Unit} outside {state.Row.Info.Name}"),
                    abbreviated: static (value, state) => !string.IsNullOrWhiteSpace(value.Unit) && double.IsFinite(value.Value)
                        && Quantity.TryFromUnitAbbreviation(state.Policy.Culture, value.Value, value.Unit, out IQuantity? typed) && typed is not null
                            ? state.Row.AdmitQuantity(typed, state.Correlation)
                            : ComputeFault.Create($"unit-admission {state.Row.Key}: '{value.Unit}' outside {state.Row.Info.Name}")))
                .Run()
                .MapFail(error => (Error)ComputeFault.Create($"unit-admission {Key}: {error.Message}"))
                .Bind(identity);

    // Aggregation folds boxed quantities at the family's canonical unit through the `AggregateOp` delegate,
    // then re-enters the same `Admit` rail as a single value.
    public Fin<UnitEvidence> Aggregate(Seq<IQuantity> parts, AggregateOp op, UnitPolicy policy, CorrelationId correlation) =>
        op is null || policy is null || parts.IsEmpty || parts.Exists(static part => part is null)
            ? ComputeFault.Create($"unit-aggregate {Key}: null op/policy or empty sequence")
            : parts.Filter(part => part.QuantityInfo.Name != Info.Name).Map(static part => part.QuantityInfo.Name) is { IsEmpty: false } foreign
                ? ComputeFault.Create($"unit-aggregate {Key}: [{string.Join(", ", foreign)}] out of family")
                : Try.lift<Fin<UnitEvidence>>(() => Admit(new QuantityInput.Typed(op.Fold(parts, Canonical)), policy, correlation))
                    .Run()
                    .MapFail(error => (Error)ComputeFault.Create($"unit-aggregate {Key}: {error.Message}"))
                    .Bind(identity);

    public Option<Enum> Resolve(string unit, UnitPolicy policy) =>
        string.IsNullOrWhiteSpace(unit) || policy is null || policy.Culture is null
            ? None
            : Try.lift<Option<Enum>>(() => UnitParser.Default.TryParse(unit, Info.UnitType, policy.Culture, out Enum? resolved)
                    ? Some(resolved)
                    : None)
                .Run()
                .Match(Succ: static resolved => resolved, Fail: static _ => None);

    public Fin<string> Render(double canonicalValue, UnitPolicy policy, Option<Enum> target = default) =>
        policy is null || policy.Culture is null || string.IsNullOrWhiteSpace(policy.Format)
            ? ComputeFault.Create($"unit-render {Key}: invalid policy")
            : Try.lift<Fin<string>>(() => target.IfNone(Display) is Enum resolved && double.IsFinite(canonicalValue) && resolved.GetType() == Info.UnitType
                    ? Fin.Succ(((IFormattable)Quantity.From(canonicalValue, Canonical).ToUnit(resolved)).ToString(policy.Format, policy.Culture))
                    : ComputeFault.Create($"unit-render {Key}: target outside {Info.Name}"))
                .Run()
                .MapFail(error => (Error)ComputeFault.Create($"unit-render {Key}: {error.Message}"))
                .Bind(identity);

    public bool Equivalent(IQuantity left, IQuantity right) =>
        left is not null && right is not null && Try.lift(() => left.Equals(right, Quantity.From(Tolerance, Canonical))).Run()
            .Match(Succ: static equivalent => equivalent, Fail: static _ => false);

    Fin<UnitEvidence> AdmitQuantity(IQuantity quantity, CorrelationId correlation) =>
        quantity is null
            ? ComputeFault.Create($"unit-admission {Key}: null quantity")
            : Try.lift<Fin<UnitEvidence>>(() => quantity.QuantityInfo.Name == Info.Name && quantity.Dimensions.Equals(Info.BaseDimensions)
                    && double.IsFinite(quantity.As(Canonical))
                        ? Fin.Succ(UnitEvidence.From(quantity, this, correlation))
                        : ComputeFault.Create($"unit-admission {Key}: {quantity.QuantityInfo.Name} out of family or non-finite"))
                .Run()
                .MapFail(error => (Error)ComputeFault.Create($"unit-admission {Key}: {error.Message}"))
                .Bind(identity);
}

[Union]
public abstract partial record QuantityInput {
    private QuantityInput() { }

    public sealed record Typed(IQuantity Value) : QuantityInput;
    public sealed record Text(string Value) : QuantityInput;
    public sealed record UnitValue(double Value, Enum Unit) : QuantityInput;
    public sealed record Abbreviated(double Value, string Unit) : QuantityInput;
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

// Compute-internal cut-parameter ingress: feed/surface-speed/depth/spindle text canonicalizes to an SI raw double through the one QuantityFamily owner;
// composed by a Compute path or host-free peer over the wire, never an AEC project — Rasm.Fabrication admits its own cut parameters in-folder.
public static class CutParameterIngress {
    // A slot maps to its QuantityFamily ALONE — the SI unit it coerces to is the family's own Canonical (BaseUnitInfo.Value), so a parallel per-slot
    // canonical Enum is dead data Canonicalize never reads: the family owns the SI base unit, the slot only the family.
    static readonly FrozenDictionary<string, QuantityFamily> Slots =
        new Dictionary<string, QuantityFamily>(StringComparer.OrdinalIgnoreCase) {
            ["feed"] = QuantityFamily.Speed,
            ["surface-speed"] = QuantityFamily.Speed,
            ["depth"] = QuantityFamily.Length,
            ["spindle"] = QuantityFamily.RotationalSpeed,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // Consumers receive only the raw SI double the slot's family canonicalizes.
    public static Fin<double> Canonicalize(string slot, string text, UnitPolicy policy, CorrelationId correlation) =>
        !string.IsNullOrWhiteSpace(slot) && Slots.TryGetValue(slot, out QuantityFamily? family)
            ? family.Admit(new QuantityInput.Text(text), policy, correlation).Map(static evidence => evidence.CanonicalValue)
            : ComputeFault.Create($"cut-parameter-ingress: unknown slot '{slot}'");

    public static Seq<string> Slot => Slots.Keys.ToSeq();
}
```

## [03]-[DIMENSIONAL_LAW]

- Owner: `UnitPolicy` configuration-bound policy record; `UnitAlgebra` static dimensional surface.
- Cases: speed, acceleration, force, pressure, energy, power, torque, density — compound relation rows, each verifying the composed `BaseDimensions` of its factors equals the compound row's declared dimensions; one reciprocal-pair row (heat-transfer-coefficient · thermal-resistance is dimensionless); one dimensionless-family row (ratio).
- Entry: `Consistency()` — `Fin<Unit>` aborts with the drifted row keys at composition, then chains `UnitMetadata.Probe()` so metadata-sourcing drift closes the same sweep.
- Packages: UnitsNet, LanguageExt.Core, BCL inbox
- Growth: one relation row in `Relations` per compound admission; zero new surface.
- Boundary: `UnitPolicy` binds at `Section` through the configuration rail and carries the resolved `CultureInfo`; `Baseline` pins `UnitSystem.SI`. Admission checks `Dimensions.Equals(Info.BaseDimensions)`, and `Consistency` proves compound, reciprocal, and dimensionless rows through UnitsNet `BaseDimensions`. `UnitMetadata.Probe()` joins the same composition fault, while numeric-only conversion rides `UnitConverter.TryConvert` without constructing an `IQuantity`.
- Aggregation: `UnitMath` owns explicit-unit sequence folds and pairwise arithmetic, coercing through `As(unit)` at the family canonical. `AggregateOp` exposes those folds through `QuantityFamily.Aggregate`, which re-enters `UnitEvidence`; `GenericMathExtensions` and `DecimalGenericMathExtensions` are absent phantoms.

```csharp signature
public sealed record UnitPolicy(CultureInfo Culture, string Format = "G") {
    public const string Section = nameof(UnitPolicy);

    public UnitSystem Baseline => UnitSystem.SI;
}

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

    public static readonly Seq<QuantityFamily> Dimensionless = Seq(QuantityFamily.Ratio);

    public static Fin<Unit> Consistency() =>
        (Relations.Filter(static row => !row.Compose(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).Equals(row.Compound.Info.BaseDimensions)).Map(static row => row.Compound.Key)
                + Reciprocals.Filter(static row => !Product(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).IsDimensionless()).Map(static row => row.Left.Key)
                + Dimensionless.Filter(static row => !row.Info.BaseDimensions.IsDimensionless()).Map(static row => row.Key)) is { IsEmpty: false } drift
            ? ComputeFault.Create($"unit-dimensions: {string.Join(", ", drift)} drift at composition")
            : UnitMetadata.Probe();

    public static Fin<double> Numeric(double value, Enum from, Enum to) =>
        !double.IsFinite(value) || from is null || to is null
            ? ComputeFault.Create("unit-convert: invalid input")
            : Try.lift<Fin<double>>(() => UnitConverter.TryConvert(value, from, to, out double converted) && double.IsFinite(converted)
                    ? Fin.Succ(converted)
                    : ComputeFault.Create($"unit-convert: {from} to {to} unsupported"))
                .Run()
                .MapFail(error => (Error)ComputeFault.Create($"unit-convert: {error.Message}"))
                .Bind(identity);
}
```

## [04]-[PARSE_FORMAT]

- Owner: `UnitEvidence` dual-evidence projection record.
- Entry: `Render(UnitPolicy policy)` — total display projection; formatting never round-trips into computation.
- Receipt: `UnitEvidence` — family key, original unit and value, canonical unit and value, correlation id; the receipt union's unit-projection case carries it verbatim.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `QuantityInfo` catalogue row per admitted family — the dashboard quantity picker derives from `Catalogue()`; zero new surface.
- Boundary: boundary text parses culture-scoped through `Quantity.TryParse(policy.Culture, Info.ValueType, ...)`, with `Resolve` owning abbreviation→`Enum` resolution through `UnitParser.Default` backed by `UnitAbbreviationsCache.Default`, whose lookup falls back to the invariant-culture abbreviation set when the policy culture lacks a localized one. `Render` takes a `UnitProject` target unit as the resolved target override and renders through the boxed `IFormattable.ToString(format, culture)` face — the generic `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, …)` cannot bind a runtime-boxed `IQuantity`, so the boxed formattable face IS the dynamic rendering surface; the precision column is the format-string row carried on `UnitPolicy`, never a per-call-site `ToString` overload.
- Abbreviation: `UnitFormatter` is `internal`, so the consumable bare-unit surface is the abbreviation cache; the metadata-driven picker holds each conversion target's `UnitInfo` from `ConvertTargets`, so it resolves the glyph and the full alias set through `UnitAbbreviationsCache.GetAbbreviations(UnitInfo, policy.Culture)` — the `UnitInfo`-keyed overload, because the generic `GetDefaultAbbreviation<TUnit>`/`GetUnitAbbreviations<TUnit>` cannot bind a `TUnit` over a runtime-enumerated unit. `Catalogue()` projects the `Info` rows for the picker and `UnitMetadata.ConvertTargets` projects each family's `QuantityInfo.UnitInfos` as `Seq<UnitInfo>` (each carrying `.Value`/`.Name`/`.PluralName` and feeding `GetAbbreviations`), all with zero custom-attribute reflection.
- Wire: evidence fields are plain strings and doubles, so the record serializes through the package wire context while UnitsNet types never cross a JSON or proto wire — conversion-at-admission is what enforces the recorded UnitsNet-serialization SKIP.

```csharp signature
public sealed record UnitEvidence(
    string Family,
    string OriginalUnit,
    double OriginalValue,
    string CanonicalUnit,
    double CanonicalValue,
    CorrelationId CorrelationId) {

    public static UnitEvidence From(IQuantity quantity, QuantityFamily row, CorrelationId correlation) =>
        new(
            Family: row.Key,
            OriginalUnit: quantity.Unit.ToString(),
            OriginalValue: (double)quantity.Value,
            CanonicalUnit: row.Canonical.ToString(),
            CanonicalValue: quantity.As(row.Canonical),
            CorrelationId: correlation);

    public static Seq<QuantityInfo> Catalogue() =>
        QuantityFamily.Items.ToSeq().Map(static row => row.Info);

    public Fin<string> Render(UnitPolicy policy) =>
        Try.lift<Fin<string>>(() => QuantityFamily.Get(Family).Render(CanonicalValue, policy))
            .Run()
            .MapFail(static error => (Error)ComputeFault.Create($"unit-evidence-render: {error.Message}"))
            .Bind(identity);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
