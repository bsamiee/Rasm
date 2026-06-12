# [COMPUTE_UNITS_BOUNDARY]

The UnitsNet boundary for measured execution: fifteen frozen `QuantityFamily` rows admit every
unit-bearing input exactly once, canonicalize through `As`/`ToUnit`, and emit dual `UnitEvidence`;
interior numerics stay raw doubles owned by Rasm core, and no quantity type crosses an interior
signature or a wire. The page owns the `QuantityFamily` axis with canonical, display, and tolerance
columns, the SI dimensional law, and the culture-scoped parse and format edges. The spine is
UnitsNet, Thinktecture.Runtime.Extensions, and LanguageExt.Core over the settled configuration rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                             |
| :-----: | --------------- | ------------------------------------------------------------------ |
|   [1]   | QUANTITY_TABLE  | Fifteen frozen quantity rows; conversion exactly once at admission |
|   [2]   | DIMENSIONAL_LAW | Compound dimensional consistency and the SI baseline policy row    |
|   [3]   | PARSE_FORMAT    | Culture-scoped parse and format edges; dual unit evidence          |

## [2]-[QUANTITY_TABLE]

- Owner: `QuantityFamily` `[SmartEnum<string>]` fifteen rows; `QuantityKeyPolicy` single ordinal-ignore-case key accessor.
- Cases: length, area, volume, mass, duration, speed, acceleration, force, pressure, energy, power, temperature, angle, torque, ratio — each row carries `QuantityInfo` metadata, canonical unit, display unit, and a tolerance column feeding equivalence proofs.
- Entry: `Admit(IQuantity quantity, UnitPolicy policy, Guid correlation)` — `Fin<UnitEvidence>` aborts; text, value-plus-unit, and value-plus-abbreviation arities discriminate on payload shape and converge on the same rail.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one table row on `QuantityFamily` — the AEC successors HeatTransferCoefficient, ThermalInsulance, AreaMomentOfInertia, and Illuminance each land as one row; zero new surface.
- Boundary: conversion runs exactly once at admission and interior numerics are raw doubles owned by Rasm core — a quantity type in an interior signature is the seam violation this table deletes; unit-admission failures mint `ComputeFault` through the dual-tier `Create` text route on the 2200 code band, the units-boundary contribution to the intent-and-selection fault union; `UnitsNetSetup.Default` is the single setup root composed once at the composition root, with `UnitConverter` riding it and a second setup instance rejected; NodaTime owns interior time, so the duration row exists only to canonicalize boundary text to seconds before rail time takes over; `UnitProject` intents enter this entrypoint and the `Pipeline` intent case composes it.

```csharp signature
public sealed class QuantityKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[ValidationError<ComputeFault>]
[KeyMemberEqualityComparer<QuantityKeyPolicy, string>]
[KeyMemberComparer<QuantityKeyPolicy, string>]
public sealed partial class QuantityFamily {
    public static readonly QuantityFamily Length = new("length", info: UnitsNet.Length.Info, canonical: LengthUnit.Meter, display: LengthUnit.Millimeter, tolerance: 1e-9);
    public static readonly QuantityFamily Area = new("area", info: UnitsNet.Area.Info, canonical: AreaUnit.SquareMeter, display: AreaUnit.SquareMeter, tolerance: 1e-9);
    public static readonly QuantityFamily Volume = new("volume", info: UnitsNet.Volume.Info, canonical: VolumeUnit.CubicMeter, display: VolumeUnit.CubicMeter, tolerance: 1e-9);
    public static readonly QuantityFamily Mass = new("mass", info: UnitsNet.Mass.Info, canonical: MassUnit.Kilogram, display: MassUnit.Kilogram, tolerance: 1e-9);
    public static readonly QuantityFamily Duration = new("duration", info: UnitsNet.Duration.Info, canonical: DurationUnit.Second, display: DurationUnit.Second, tolerance: 1e-9);
    public static readonly QuantityFamily Speed = new("speed", info: UnitsNet.Speed.Info, canonical: SpeedUnit.MeterPerSecond, display: SpeedUnit.MeterPerSecond, tolerance: 1e-9);
    public static readonly QuantityFamily Acceleration = new("acceleration", info: UnitsNet.Acceleration.Info, canonical: AccelerationUnit.MeterPerSecondSquared, display: AccelerationUnit.MeterPerSecondSquared, tolerance: 1e-9);
    public static readonly QuantityFamily Force = new("force", info: UnitsNet.Force.Info, canonical: ForceUnit.Newton, display: ForceUnit.Newton, tolerance: 1e-9);
    public static readonly QuantityFamily Pressure = new("pressure", info: UnitsNet.Pressure.Info, canonical: PressureUnit.Pascal, display: PressureUnit.Kilopascal, tolerance: 1e-9);
    public static readonly QuantityFamily Energy = new("energy", info: UnitsNet.Energy.Info, canonical: EnergyUnit.Joule, display: EnergyUnit.KilowattHour, tolerance: 1e-9);
    public static readonly QuantityFamily Power = new("power", info: UnitsNet.Power.Info, canonical: PowerUnit.Watt, display: PowerUnit.Watt, tolerance: 1e-9);
    public static readonly QuantityFamily Temperature = new("temperature", info: UnitsNet.Temperature.Info, canonical: TemperatureUnit.Kelvin, display: TemperatureUnit.DegreeCelsius, tolerance: 1e-6);
    public static readonly QuantityFamily Angle = new("angle", info: UnitsNet.Angle.Info, canonical: AngleUnit.Radian, display: AngleUnit.Degree, tolerance: 1e-9);
    public static readonly QuantityFamily Torque = new("torque", info: UnitsNet.Torque.Info, canonical: TorqueUnit.NewtonMeter, display: TorqueUnit.NewtonMeter, tolerance: 1e-9);
    public static readonly QuantityFamily Ratio = new("ratio", info: UnitsNet.Ratio.Info, canonical: RatioUnit.DecimalFraction, display: RatioUnit.Percent, tolerance: 1e-9);

    public QuantityInfo Info { get; }

    public Enum Canonical { get; }

    public Enum Display { get; }

    public double Tolerance { get; }

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
        Quantity.TryParse(policy.Culture, Info.ValueType, string.Create(policy.Culture, $"1 {unit}"), out var probe)
            ? Some(probe.Unit)
            : None;

    public string Render(double canonicalValue, UnitPolicy policy, Option<Enum> target = default) =>
        Quantity.From(canonicalValue, Canonical).ToUnit(target.IfNone(Display)).ToString(policy.Culture);
}
```

## [3]-[DIMENSIONAL_LAW]

- Owner: `UnitPolicy` configuration-bound policy record; `UnitAlgebra` static dimensional surface.
- Cases: speed, acceleration, force, pressure, energy, power, torque — seven compound relation rows, each verifying the composed `BaseDimensions` of its factors equals the compound row's declared dimensions.
- Entry: `Consistency()` — `Fin<Unit>` aborts with the drifted row keys at composition.
- Packages: UnitsNet, LanguageExt.Core, BCL inbox
- Growth: one relation row in `Relations` per compound admission; zero new surface.
- Boundary: `UnitPolicy` binds at its `Section` symbol through the configuration rail and `CultureName` arrives as a settled row value; `Baseline` pins `UnitSystem.SI` as the unit-system policy row; per-admission dimensional equality rides `Admit` through `Dimensions.Equals(Info.BaseDimensions)` while the relation sweep runs once at composition; `UnitMath` and `GenericMathExtensions` are admitted for boundary-side aggregation only, aggregate at `Baseline`, and re-enter `Admit` before any value reaches the rails — per-quantity converter helpers and extension forests are the deleted form.

```csharp signature
public sealed record UnitPolicy(string CultureName) {
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
        (QuantityFamily.Torque, QuantityFamily.Force, QuantityFamily.Length, Product));

    public static Fin<Unit> Consistency() =>
        Relations.Filter(static row => !row.Compose(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).Equals(row.Compound.Info.BaseDimensions))
                .Map(static row => row.Compound.Key) is { IsEmpty: false } drift
            ? ComputeFault.Create($"unit-dimensions: {string.Join(", ", drift)} drift at composition")
            : Fin<Unit>.Succ(unit);
}
```

## [4]-[PARSE_FORMAT]

- Owner: `UnitEvidence` dual-evidence projection record.
- Entry: `Render(UnitPolicy policy)` — total display projection; formatting never round-trips into computation.
- Receipt: `UnitEvidence` — family key, original unit and value, canonical unit and value, correlation id; the receipt union's unit-projection case carries it verbatim.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `QuantityInfo` catalogue row per admitted family — the dashboard quantity picker derives from `Catalogue()`; zero new surface.
- Boundary: boundary text parses culture-scoped through `Quantity.TryParse(policy.Culture, ...)` with `Resolve` owning abbreviation resolution over the same parse route backed by `UnitAbbreviationsCache.Default`, whose lookup falls back to the invariant-culture abbreviation set when the policy culture lacks a localized one, a `UnitProject` target unit entering `Render` as the resolved target override, and `QuantityFormatter` owning the rendered text behind `ToString(policy.Culture)`; `Catalogue()` projects the fifteen `Info` rows for the dashboard quantity picker with zero call-site reflection; evidence fields are plain strings and doubles, so the record serializes through the package wire context while UnitsNet types never cross a JSON or proto wire — the recorded UnitsNet-serialization SKIP stays law and conversion-at-admission is what enforces it.

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

## [5]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                                                                             | [PROOF]                                                                                                                                                                               | [GATE]         |
| :-----: | -------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- |
|   [1]   | UnitsNet next-major QuantityInfo and QuantityValue reshape against the frozen row record (adoption gate is GA; pin stays at the charter admission) | dotnet package search UnitsNet --prerelease --format json, then uv run python -m tools.assay api query --key unitsnet --symbol UnitsNet.QuantityInfo over a staged next-major restore | QUANTITY_TABLE |
