# [COMPUTE_QUANTITIES]

The UnitsNet boundary for measured execution. The frozen `QuantityFamily` rows admit every unit-bearing input exactly once, canonicalize through `As`/`ToUnit`, and emit dual `UnitEvidence`; interior numerics stay raw doubles owned by Rasm core, and no quantity type ever crosses an interior signature or a wire. The page owns the `QuantityFamily` axis with its canonical, display, and tolerance columns, the SI dimensional consistency law, and the culture-scoped parse and format edges.

This is the Compute-internal quantity-ingress owner. Every Compute execution path that admits unit-bearing text — a measured solver tolerance, a quadrature step, a sampling rate — canonicalizes through these rows, and the host-free peers (Python `compute`, TypeScript `interchange`) decode the SI scalar this owner stamps onto `UnitEvidence`, never a UnitsNet type. The `CutParameterIngress` contract is the Compute-internal slot-keyed canonicalization an in-process or host-free-peer caller composes, not an export an AEC project references.

The AEC-domain folders own their unit admission IN-FOLDER. The strata graph is acyclic — app-platform consumes AEC-domain, never the reverse — so `Rasm.Fabrication/Process` admits its cut-parameter text at `RemovalParameter.Admit` and `Rasm.Materials/Appearance` coerces its photometric illuminance at its own boundary, each spelling UnitsNet in-folder rather than reaching DOWN to this Compute owner; a `Rasm.Compute` project reference from an AEC folder is the forbidden downward edge this owner never invites. The spine is UnitsNet, Thinktecture.Runtime.Extensions, and LanguageExt.Core over the settled configuration rail.

## [01]-[INDEX]

- [01]-[QUANTITY_TABLE]: frozen quantity rows; conversion exactly once at admission; tolerance-keyed equivalence.
- [02]-[DIMENSIONAL_LAW]: compound dimensional consistency and the SI baseline policy row.
- [03]-[PARSE_FORMAT]: culture-scoped parse and format edges; dual unit evidence.

## [02]-[QUANTITY_TABLE]

- Owner: `QuantityFamily` `[SmartEnum<string>]` rows, keyed ordinal-ignore-case through the shipped `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `UnitMetadata` the metadata-sourcing owner over `QuantityInfo.BaseUnitInfo`/`UnitInfos`/`UnitType`.
- Cases: length, area, volume, mass, duration, speed, acceleration, force, pressure, energy, power, temperature, angle, torque, ratio, density, area-moment-of-inertia, heat-transfer-coefficient, thermal-resistance, illuminance, rotational-speed — each row carries `QuantityInfo` metadata, a `BaseUnitInfo.Value`-sourced canonical unit, an explicit display unit defaulting to canonical, and a tolerance column the `Equivalent` proof reads; the `Speed`, `RotationalSpeed`, and `Length` (depth) rows are the cut-parameter ingress vocabulary the `CutParameterIngress` contract canonicalizes for the Fabrication consumer.
- Entry: `Admit(IQuantity quantity, UnitPolicy policy, Guid correlation)` — `Fin<UnitEvidence>` aborts; the typed-quantity, text, value-plus-unit, and value-plus-abbreviation arities discriminate on payload shape and converge on the same rail. `Equivalent(IQuantity, IQuantity)` is the tolerance-keyed equivalence over the row's canonical scale.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one table row on `QuantityFamily` per further AEC quantity, admitted only when a consumer exists — its canonical column sourcing from `QuantityInfo.BaseUnitInfo.Value` and its display column an explicit per-row `Enum`; the rows close the families the current symbolic, solver, and cut-parameter consumers admit, so a speculative row with no consumer is the rejected addition; zero new surface.
- Boundary: conversion runs exactly once at admission and interior numerics are raw doubles owned by Rasm core — a quantity type in an interior signature is the seam violation this table deletes. Unit-admission failures mint `ComputeFault` through the dual-tier `Create` text route on the 2200 code band, the units-boundary contribution to the intent-and-selection fault union. `UnitsNetSetup.Default` is the single setup root composed once at the composition root, with `UnitConverter` riding it and a second setup instance rejected. NodaTime owns interior time, so the duration row exists only to canonicalize boundary text to seconds before rail time takes over. `UnitProject` intents enter `Admit` and the `Pipeline` intent case composes it; the `CutParameterIngress` contract canonicalizes feed/surface-speed/depth/spindle text to an SI raw double for the same in-process and host-free-peer callers, never an AEC project over a reference.
- Metadata sourcing: the canonical `Enum` column reads `QuantityInfo.BaseUnitInfo.Value` once per row at static construction — the metadata is the column, so the hand-passed canonical arg is deleted, and UnitsNet emits no `DefaultUnitAttribute`/`DisplayAsUnitAttribute` on the generated types so attribute reflection over them resolves to nothing. The display column is the explicit presentation `Enum` defaulting to canonical. `Probe()` is the composition-time coherence guard: a row whose display `Enum` does not belong to its `QuantityInfo.UnitType` (a cross-family display typo such as a `PressureUnit` on the `Length` row) drifts as a `ComputeFault` at composition rather than failing the first `Render`; the canonical column needs no such guard because it is read FROM `BaseUnitInfo.Value` and so is type-correct by construction.

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

    public Fin<UnitEvidence> Admit(IQuantity quantity, UnitPolicy policy, Guid correlation) =>
        quantity.QuantityInfo.Name == Info.Name && quantity.Dimensions.Equals(Info.BaseDimensions)
            ? (Fin<UnitEvidence>)UnitEvidence.From(quantity, this, correlation)
            : ComputeFault.Create($"unit-admission {Key}: {quantity.QuantityInfo.Name} out of family");

    public Fin<UnitEvidence> Admit(string text, UnitPolicy policy, Guid correlation) =>
        Quantity.TryParse(policy.Culture, Info.ValueType, text, out var parsed)
            ? Admit(parsed, policy, correlation)
            : ComputeFault.Create($"unit-admission {Key}: '{text}' outside {Info.Name}");

    public Fin<UnitEvidence> Admit(double value, Enum unit, UnitPolicy policy, Guid correlation) =>
        Quantity.TryFrom(value, unit, out var typed)
            ? Admit(typed, policy, correlation)
            : ComputeFault.Create($"unit-admission {Key}: {unit} outside {Info.Name}");

    public Fin<UnitEvidence> Admit(double value, string unit, UnitPolicy policy, Guid correlation) =>
        Resolve(unit, policy) is { IsSome: true, Case: Enum resolved }
            ? Admit(value, resolved, policy, correlation)
            : ComputeFault.Create($"unit-admission {Key}: '{unit}' outside {Info.Name}");

    public Option<Enum> Resolve(string unit, UnitPolicy policy) =>
        UnitParser.Default.TryParse(unit, Info.UnitType, policy.Culture, out var resolved)
            ? Some(resolved)
            : Option<Enum>.None;

    public string Render(double canonicalValue, UnitPolicy policy, Option<Enum> target = default) =>
        ((IFormattable)Quantity.From(canonicalValue, Canonical).ToUnit(target.IfNone(Display))).ToString(policy.Format, policy.Culture);

    public bool Equivalent(IQuantity left, IQuantity right) =>
        left.Equals(right, Quantity.From(Tolerance, Canonical));
}

// Compute-internal cut-parameter ingress: feed/surface-speed/depth/spindle text canonicalizes to an SI raw
// double through the one QuantityFamily owner. Composed by a Compute-internal path or a host-free peer over
// the wire, never referenced by an AEC project — Rasm.Fabrication admits its own cut parameters in-folder.
public static class CutParameterIngress {
    // The slot maps to its QuantityFamily ALONE — the canonical SI unit each slot coerces to is the family's
    // own `Canonical` (its `BaseUnitInfo.Value`), so a parallel per-slot canonical Enum is dead duplicate data
    // `Canonicalize` never reads and is collapsed out: the family owns the SI base unit, the slot only the family.
    static readonly FrozenDictionary<string, QuantityFamily> Slots =
        new Dictionary<string, QuantityFamily>(StringComparer.OrdinalIgnoreCase) {
            ["feed"] = QuantityFamily.Speed,
            ["surface-speed"] = QuantityFamily.Speed,
            ["depth"] = QuantityFamily.Length,
            ["spindle"] = QuantityFamily.RotationalSpeed,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // The consumer never sees an IQuantity, only the raw SI double the slot's family canonicalizes to.
    public static Fin<double> Canonicalize(string slot, string text, UnitPolicy policy, Guid correlation) =>
        Slots.TryGetValue(slot, out var family)
            ? family.Admit(text, policy, correlation).Map(static evidence => evidence.CanonicalValue)
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
- Boundary: `UnitPolicy` binds at its `Section` symbol through the configuration rail, `CultureName` arrives as a settled row value, and `Baseline` pins `UnitSystem.SI` as the unit-system policy row. Per-admission dimensional equality rides `Admit` through `Dimensions.Equals(Info.BaseDimensions)`; the relation sweep runs once at composition. The `Density` row joins `Relations` as `Mass / Volume`, the `HeatTransferCoefficient`/`ThermalResistance` U-value/R-value pair asserts as a `Reciprocals` row whose `BaseDimensions` product must equal `BaseDimensions.Dimensionless`, and the `Ratio` row asserts dimensionlessness through the `Dimensionless` seq in the same sweep — `ThermalResistance` is the area-specific m²·K/W form whose SI dimensions are the exact inverse of `HeatTransferCoefficient`, and the kilo prefix is irrelevant because `BaseDimensions` carries pure SI exponents. `UnitMetadata.Probe()` chains last in `Consistency()` so a display-unit/`UnitType` mismatch drifts in the same composition fault rather than at first `Render`. A numeric-only conversion that crosses no quantity type rides `UnitConverter.TryConvert`, the non-throwing converter that constructs no `IQuantity`.
- Aggregation: boundary-side aggregation rides exactly two real surfaces, both at `Baseline` and re-entering `Admit` before any value reaches the rails. `UnitMath` owns the explicit-unit sequence folds — `Sum`/`Min`/`Max`/`Average`/`Clamp`/`Abs` over an `IEnumerable<TQuantity>` at a chosen `Enum` unit — coercing each element through `As(unit)` in double precision. `UnitsNet.GenericMath.GenericMathExtensions` owns the generic-math sequence folds — `Sum<T>`/`Average<T>` over an `IEnumerable<T>` constrained on each quantity's own `IAdditionOperators`/`IAdditiveIdentity` (the decimal-domain `DecimalGenericMathExtensions.Average` its mirror) — so a boundary sum reduces in the quantity's native `IValueQuantity<TValueType>` store with no `As(unit)` round-trip, the value-preserving path the Energy/Power-class sums take where `UnitMath`'s double coercion would round. Those folds compose each quantity's `IArithmeticQuantity<TSelf,TUnitType,TValueType>` operator surface (`+`/`-`/`* scalar`/`/ scalar`/`Zero`/`IAdditiveIdentity`), present on the bound `net9.0` asset and absent only on the `netstandard2.0` fallback the workspace never binds; a hand-rolled per-quantity converter helper, a throwing converter, or an extension forest beside `UnitMath`/`GenericMathExtensions` is the deleted form.

```csharp signature
public sealed record UnitPolicy(string CultureName, string Format = "G") {
    public const string Section = nameof(UnitPolicy);

    public CultureInfo Culture => CultureInfo.GetCultureInfo(CultureName);

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
                + Reciprocals.Filter(static row => !Product(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).Equals(BaseDimensions.Dimensionless)).Map(static row => row.Left.Key)
                + Dimensionless.Filter(static row => !row.Info.BaseDimensions.Equals(BaseDimensions.Dimensionless)).Map(static row => row.Key)) is { IsEmpty: false } drift
            ? ComputeFault.Create($"unit-dimensions: {string.Join(", ", drift)} drift at composition")
            : UnitMetadata.Probe();

    public static Fin<double> Numeric(double value, Enum from, Enum to) =>
        UnitConverter.TryConvert(value, from, to, out var converted)
            ? Fin.Succ(converted)
            : ComputeFault.Create($"unit-convert: {from} to {to} unsupported");
}
```

## [04]-[PARSE_FORMAT]

- Owner: `UnitEvidence` dual-evidence projection record.
- Entry: `Render(UnitPolicy policy)` — total display projection; formatting never round-trips into computation.
- Receipt: `UnitEvidence` — family key, original unit and value, canonical unit and value, correlation id; the receipt union's unit-projection case carries it verbatim.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `QuantityInfo` catalogue row per admitted family — the dashboard quantity picker derives from `Catalogue()`; zero new surface.
- Boundary: boundary text parses culture-scoped through `Quantity.TryParse(policy.Culture, Info.ValueType, ...)`, with `Resolve` owning abbreviation→`Enum` resolution through `UnitParser.Default` backed by `UnitAbbreviationsCache.Default`, whose lookup falls back to the invariant-culture abbreviation set when the policy culture lacks a localized one. `Render` takes a `UnitProject` target unit as the resolved target override and `QuantityFormatter.Format` owns the rendered text behind the culture-scoped format string — the precision column is the format-string row carried on `UnitPolicy`, never a per-call-site `ToString` overload.
- Abbreviation: `UnitFormatter` is `internal`, so the consumable bare-unit surface is the abbreviation cache. The metadata-driven picker holds each conversion target's `UnitInfo` from `ConvertTargets`, so it resolves the glyph and the full alias set through `UnitAbbreviationsCache.GetAbbreviations(UnitInfo, policy.Culture)` — the `UnitInfo`-keyed overload, because the generic `GetDefaultAbbreviation<TUnit>`/`GetUnitAbbreviations<TUnit>` cannot bind a `TUnit` over a runtime-enumerated unit. `Catalogue()` projects the `Info` rows for the picker and `UnitMetadata.ConvertTargets` projects each family's `QuantityInfo.UnitInfos` as `Seq<UnitInfo>` (each carrying `.Value`/`.Name`/`.PluralName` and feeding `GetAbbreviations`), all with zero custom-attribute reflection.
- Wire: evidence fields are plain strings and doubles, so the record serializes through the package wire context while UnitsNet types never cross a JSON or proto wire — conversion-at-admission is what enforces the recorded UnitsNet-serialization SKIP.

```csharp signature
public sealed record UnitEvidence(
    string Family,
    string OriginalUnit,
    double OriginalValue,
    string CanonicalUnit,
    double CanonicalValue,
    Guid CorrelationId) {

    public static UnitEvidence From(IQuantity quantity, QuantityFamily row, Guid correlation) =>
        new(
            Family: row.Key,
            OriginalUnit: quantity.Unit.ToString(),
            OriginalValue: quantity.As(quantity.Unit),
            CanonicalUnit: row.Canonical.ToString(),
            CanonicalValue: quantity.As(row.Canonical),
            CorrelationId: correlation);

    public static Seq<QuantityInfo> Catalogue() =>
        QuantityFamily.Items.ToSeq().Map(static row => row.Info);

    public string Render(UnitPolicy policy) =>
        QuantityFamily.Get(Family).Render(CanonicalValue, policy);
}
```

## [05]-[RESEARCH]

- [NEXT_MAJOR]: the UnitsNet next-major `QuantityInfo` and `QuantityValue` reshape against the frozen row record, run as a staged-restore reshape check before adoption.
