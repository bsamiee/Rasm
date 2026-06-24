# [COMPUTE_QUANTITIES]

The UnitsNet boundary for measured execution: twenty-one frozen `QuantityFamily` rows admit every unit-bearing input exactly once, canonicalize through `As`/`ToUnit`, and emit dual `UnitEvidence`; interior numerics stay raw doubles owned by Rasm core, and no quantity type crosses an interior signature or a wire. This is the Compute-internal quantity-ingress owner: every Compute execution path that admits unit-bearing text (a measured solver tolerance, a quadrature step, a sampling rate) canonicalizes through these rows, and the host-free peers (Python `compute`, TypeScript `interchange`) decode the canonicalized SI scalar over the wire — the wire layout this owner stamps onto `UnitEvidence`, never a UnitsNet type crossing. The AEC-domain folders own their unit admission IN-FOLDER, not over a reference to this owner: the strata graph is acyclic (app-platform consumes AEC-domain, never the reverse), so `Rasm.Fabrication/Process` admits its cut-parameter text at `RemovalParameter.Admit` and `Rasm.Materials/Appearance` photometric coerces its illuminance at its own boundary, each spelling UnitsNet in-folder rather than reaching DOWN to this Compute owner — a `Rasm.Compute` project reference from an AEC folder is the forbidden downward edge this owner never invites. The `CutParameterIngress` contract below is the Compute-internal slot-keyed canonicalization an in-process or host-free-peer caller composes, NOT an export an AEC project references. The page owns the `QuantityFamily` axis with canonical, display, and tolerance columns, the SI dimensional law, and the culture-scoped parse and format edges. The spine is UnitsNet, Thinktecture.Runtime.Extensions, and LanguageExt.Core over the settled configuration rail.

## [01]-[INDEX]

- [01]-[QUANTITY_TABLE]: twenty-one frozen quantity rows; conversion exactly once at admission.
- [02]-[DIMENSIONAL_LAW]: compound dimensional consistency and the SI baseline policy row.
- [03]-[PARSE_FORMAT]: culture-scoped parse and format edges; dual unit evidence.

## [02]-[QUANTITY_TABLE]

- Owner: `QuantityFamily` `[SmartEnum<string>]` twenty-one rows; `QuantityKeyPolicy` single ordinal-ignore-case key accessor; `UnitMetadata` metadata-sourcing owner over `QuantityInfo.BaseUnitInfo`/`UnitInfos`.
- Cases: length, area, volume, mass, duration, speed, acceleration, force, pressure, energy, power, temperature, angle, torque, ratio, density, area-moment-of-inertia, heat-transfer-coefficient, thermal-resistance, illuminance, rotational-speed — each row carries `QuantityInfo` metadata, a `BaseUnitInfo.Value`-sourced canonical unit, an explicit display unit defaulting to canonical, and a tolerance column feeding equivalence proofs; the `Speed` and `RotationalSpeed` rows plus the `Length` (depth) row are the cut-parameter ingress vocabulary the `CutParameterIngress` contract canonicalizes for the Fabrication consumer.
- Entry: `Admit(IQuantity quantity, UnitPolicy policy, Guid correlation)` — `Fin<UnitEvidence>` aborts; text, value-plus-unit, and value-plus-abbreviation arities discriminate on payload shape and converge on the same rail.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one table row on `QuantityFamily` — the structural Density row and AEC successors AreaMomentOfInertia, HeatTransferCoefficient, ThermalResistance, and Illuminance land as rows whose canonical column sources from `QuantityInfo.BaseUnitInfo.Value` and whose display column is an explicit per-row `Enum`; further AEC quantities land as one row each when a consumer exists; zero new surface.
- Boundary: conversion runs exactly once at admission and interior numerics are raw doubles owned by Rasm core — a quantity type in an interior signature is the seam violation this table deletes; each row's canonical `Enum` column sources from `QuantityInfo.BaseUnitInfo.Value` once at static construction and the display `Enum` is an explicit constructor arg that defaults to canonical — the hand-passed canonical `Enum` arg is deleted, the metadata is the column (UnitsNet emits no `DefaultUnitAttribute`/`DisplayAsUnitAttribute` on the generated types, so attribute reflection resolves to nothing); abbreviation resolution runs through `UnitParser.Default.TryParse(string, Type, IFormatProvider, out Enum)` rather than the `"1 {unit}"` probe-string parse, which is the deleted hack; unit-admission failures mint `ComputeFault` through the dual-tier `Create` text route on the 2200 code band, the units-boundary contribution to the intent-and-selection fault union; `UnitsNetSetup.Default` is the single setup root composed once at the composition root, with `UnitConverter` riding it and a second setup instance rejected; NodaTime owns interior time, so the duration row exists only to canonicalize boundary text to seconds before rail time takes over; `UnitProject` intents enter this entrypoint and the `Pipeline` intent case composes it; the `Illuminance` row canonicalizes any luminous-quantity text a Compute-internal path admits, and the `CutParameterIngress` contract is the Compute-internal slot-keyed canonicalization (feed/surface-speed/depth/spindle quantity text → SI raw double) an in-process or host-free-peer caller composes — neither is read by an AEC project over a reference. The strata graph is acyclic (app-platform consumes AEC-domain, never the reverse), so the AEC consumers own their unit admission IN-FOLDER and never reach DOWN to this Compute owner: `Rasm.Materials/Appearance` photometric coerces its emission luminance through UnitsNet at its own admission boundary, and `Rasm.Fabrication/Process/physics#CUT_PARAMETER` `RemovalParameter.Admit` admits its cut-parameter text through a direct in-folder `UnitsNet` reference (`Speed`/`Length`/`RotationalSpeed`/`Pressure` `TryParse` + SI accessor) — the strata-correct resolution the Fabrication page enforces. A `Rasm.Compute` project reference from an AEC folder, or a Compute page asserting an AEC consumer reads this units owner over a reference, is the forbidden downward edge.
- Metadata sourcing: the canonical `Enum` column reads `QuantityInfo.BaseUnitInfo.Value` directly — `BaseUnitInfo` is the base-`UnitInfo` and `.Value` is the base-unit `Enum`; the read runs once per row at static construction, the display column is the explicit presentation `Enum` (defaulting to canonical), the dashboard picker's conversion targets enumerate `QuantityInfo.UnitInfos` (each `UnitInfo` exposes `.Value`/`.Name`/`.PluralName`) with zero custom-attribute reflection, and a family whose `QuantityInfo` lacks a `BaseUnitInfo` mints `ComputeFault` at composition through `Probe()` rather than silently defaulting.

```csharp signature
public sealed class QuantityKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
internal static class UnitMetadata {
    public static Seq<Enum> ConvertTargets(QuantityInfo info) =>
        info.UnitInfos.ToSeq().Map(static u => u.Value);

    public static Fin<Unit> Probe() =>
        QuantityFamily.Items.ToSeq()
            .Filter(static row => row.Info.BaseUnitInfo is null)
            .Map(static row => row.Key) is { IsEmpty: false } missing
            ? ComputeFault.Create($"unit-metadata: {string.Join(", ", missing)} missing BaseUnitInfo")
            : FinSucc(unit);
}

[SmartEnum<string>]
[ValidationError<ComputeFault>]
[KeyMemberEqualityComparer<QuantityKeyPolicy, string>]
[KeyMemberComparer<QuantityKeyPolicy, string>]
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

    private QuantityFamily(string key, QuantityInfo info, double tolerance, Enum display = null) : base(key) {
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
}

// The Compute-internal cut-parameter quantity-ingress canonicalization: feed (mm/min), surface speed
// (m/min), depth (mm), and spindle (rpm) quantity text canonicalize to the SI raw double through the one
// QuantityFamily owner, the slot-keyed surface a Compute-internal path or a host-free peer (over the wire)
// composes. This is NOT an export an AEC project references: the strata graph is acyclic (app-platform
// consumes AEC-domain, never the reverse), so the AEC-domain Rasm.Fabrication owns its cut-parameter
// admission IN-FOLDER at RemovalParameter.Admit through a direct in-folder UnitsNet reference — the
// strata-correct resolution the Fabrication physics page enforces. A Rasm.Compute project reference from
// Rasm.Fabrication to read this surface is the forbidden AEC->app-platform downward edge; the contract
// here serves Compute's own canonicalization and the host-free peers that decode the SI scalar.
public static class CutParameterIngress {
    static readonly FrozenDictionary<string, (QuantityFamily Family, Enum Canonical)> Slots =
        new Dictionary<string, (QuantityFamily, Enum)>(StringComparer.OrdinalIgnoreCase) {
            ["feed"] = (QuantityFamily.Speed, SpeedUnit.MeterPerSecond),
            ["surface-speed"] = (QuantityFamily.Speed, SpeedUnit.MeterPerSecond),
            ["depth"] = (QuantityFamily.Length, LengthUnit.Meter),
            ["spindle"] = (QuantityFamily.RotationalSpeed, RotationalSpeedUnit.RadianPerSecond),
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // Admit the slot's quantity text and project the canonical SI scalar the toolpath interior reads; the
    // consumer never sees an IQuantity, only the raw double the slot canonicalizes to.
    public static Fin<double> Canonicalize(string slot, string text, UnitPolicy policy, Guid correlation) =>
        Slots.TryGetValue(slot, out var row)
            ? row.Family.Admit(text, policy, correlation).Map(evidence => evidence.CanonicalValue)
            : ComputeFault.Create($"cut-parameter-ingress: unknown slot '{slot}'").Map<double>(static _ => 0.0);

    public static Seq<string> Slot => Slots.Keys.ToSeq();
}
```

## [03]-[DIMENSIONAL_LAW]

- Owner: `UnitPolicy` configuration-bound policy record; `UnitAlgebra` static dimensional surface.
- Cases: speed, acceleration, force, pressure, energy, power, torque, density — eight compound relation rows, each verifying the composed `BaseDimensions` of its factors equals the compound row's declared dimensions; one reciprocal-pair row (heat-transfer-coefficient · thermal-resistance is dimensionless); one dimensionless-family row (ratio).
- Entry: `Consistency()` — `Fin<Unit>` aborts with the drifted row keys at composition, then chains `UnitMetadata.Probe()` so metadata-sourcing drift closes the same sweep.
- Packages: UnitsNet, LanguageExt.Core, BCL inbox
- Growth: one relation row in `Relations` per compound admission; zero new surface.
- Boundary: `UnitPolicy` binds at its `Section` symbol through the configuration rail and `CultureName` arrives as a settled row value; `Baseline` pins `UnitSystem.SI` as the unit-system policy row; per-admission dimensional equality rides `Admit` through `Dimensions.Equals(Info.BaseDimensions)` while the relation sweep runs once at composition; the `Density` row joins `Relations` as `Mass / Volume` so a structural-quantity drift surfaces at composition, while the `HeatTransferCoefficient`/`ThermalResistance` U-value/R-value pair asserts as a `Reciprocals` row whose dimension product must equal `BaseDimensions.Dimensionless` — `ThermalResistance` is the area-specific m²·K/W form (base unit `SquareMeterKelvinPerKilowatt`) whose dimensions are the exact inverse of `HeatTransferCoefficient`; the Ratio row's dimensionlessness asserts through the `Dimensionless` seq against `BaseDimensions.Dimensionless` in the same sweep so a dimension-bearing Ratio drifts at composition rather than silently at runtime; `UnitMetadata.Probe()` chains last in `Consistency()` so a row whose `QuantityInfo` lacks a `BaseUnitInfo` drifts in the same composition fault rather than at first admission; a numeric-only conversion that crosses no quantity type rides `UnitConverter.TryConvert` — the non-throwing converter that never constructs an `IQuantity`; boundary-side aggregation is admitted over three public surfaces that ride the generic-math operator set every quantity struct implements through `IArithmeticQuantity<TSelf,TUnitType,TValueType>` (`IAdditionOperators`/`IAdditiveIdentity`/`IMultiplyOperators`/`IDivisionOperators`/`IUnaryNegationOperators` plus `static abstract TSelf Zero`): `UnitMath` for the `Enum`-unit `Sum`/`Min`/`Max`/`Average`/`Clamp`/`Abs` sequence folds, `UnitsNet.GenericMath.GenericMathExtensions` for the operator-constrained `Sum<T>`/`Average<T>` (double divisor), and `UnitsNet.GenericMath.DecimalGenericMathExtensions` for the decimal-divisor `Average<T>` that owns the decimal-precision aggregation path for the Energy- and Power-class boundary sums where double rounding loses precision — the precision backing is `QuantityValue`'s native dual decimal/double store (`IsDecimal`, implicit `decimal` conversion) surfaced through `IValueQuantity<decimal>`, not a separate decimal converter; aggregate at `Baseline`, and re-enter `Admit` before any value reaches the rails — per-quantity converter helpers, throwing converters, and extension forests are the deleted form.

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
- Boundary: boundary text parses culture-scoped through `Quantity.TryParse(policy.Culture, ...)` with `Resolve` owning abbreviation resolution through `UnitParser.Default` backed by `UnitAbbreviationsCache.Default`, whose lookup falls back to the invariant-culture abbreviation set when the policy culture lacks a localized one, a `UnitProject` target unit entering `Render` as the resolved target override, and `QuantityFormatter.Format` owning the rendered text behind a culture-scoped format string — the precision column is the format-string row carried on `UnitPolicy`, never a per-call-site `ToString` overload, with bare-unit (abbreviation) rendering for the dashboard picker owned by the public `UnitAbbreviationsCache.GetDefaultAbbreviation<TUnit>(unit, policy.Culture)` (single canonical glyph) and `GetUnitAbbreviations<TUnit>(unit, policy.Culture)` (the full alias set a picker tooltip lists), each backed by `UnitsNetSetup.Default.UnitAbbreviations` — `UnitFormatter` is `internal` and never crosses into Compute, so the abbreviation cache is the consumable bare-unit surface; `Catalogue()` projects the twenty-one `Info` rows for the dashboard quantity picker with zero call-site reflection, and `UnitMetadata.ConvertTargets` supplies each picker row's conversion targets by enumerating `QuantityInfo.UnitInfos` (each `UnitInfo.Value`) so the picker mirrors the metadata surface with no custom-attribute reflection; evidence fields are plain strings and doubles, so the record serializes through the package wire context while UnitsNet types never cross a JSON or proto wire — the recorded UnitsNet-serialization SKIP stays law and conversion-at-admission is what enforces it.

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
