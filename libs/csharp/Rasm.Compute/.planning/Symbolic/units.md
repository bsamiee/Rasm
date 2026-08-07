# [COMPUTE_QUANTITIES]

UnitsNet boundary for measured execution: the frozen `QuantityFamily` rows admit every unit-bearing input exactly once, canonicalize through `As`/`ToUnit`, and emit dual `UnitEvidence`, while interior numerics stay raw doubles owned by Rasm core and no quantity type ever crosses an interior signature or a wire. Owned here: the `QuantityFamily` axis with its canonical, display, declared-dimension, and tolerance-pair columns, the SI dimensional consistency law, and the culture-scoped parse and format edges.

Every Compute execution path admitting unit-bearing text — a solver tolerance, a quadrature step, a sampling rate — canonicalizes through these rows, and the host-free peers (Python `compute`, TypeScript `interchange`) decode the SI scalar this owner stamps onto `UnitEvidence`, never a UnitsNet type. AEC-domain folders own their unit admission IN-FOLDER — the strata graph is acyclic, so `Rasm.Fabrication/Process` admits its cut-parameter text at `RemovalParameter.Admit` and `Rasm.Materials/Appearance` coerces its photometric illuminance at its own boundary, each spelling UnitsNet in-folder rather than reaching DOWN to this owner; a `Rasm.Compute` reference from an AEC folder is the forbidden downward edge this owner never invites. Spine: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core over the settled configuration rail.

## [01]-[INDEX]

- [02]-[QUANTITY_TABLE]: frozen quantity rows with their declared SI dimensions; conversion exactly once at admission; relative-with-floor equivalence.
- [03]-[DIMENSIONAL_LAW]: total per-row declaration proof, cross-row compound identities, and the SI baseline policy row.
- [04]-[PARSE_FORMAT]: culture-scoped parse and format edges; dual unit evidence.

## [02]-[QUANTITY_TABLE]

- Owner: `QuantityFamily` `[SmartEnum<string>]` rows, keyed ordinal-ignore-case through the shipped `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `UnitMetadata` the metadata-sourcing owner over `QuantityInfo.BaseUnitInfo`/`UnitInfos`/`UnitType`.
- Cases: length, area, volume, mass, duration, speed, acceleration, force, pressure, energy, power, temperature, angle, torque, ratio, density, area-moment-of-inertia, heat-transfer-coefficient, thermal-resistance, illuminance, rotational-speed, frequency, thermal-conductivity, volume-flow, irradiance, luminance, luminous-flux, luminous-intensity — each row carries `QuantityInfo` metadata, a `BaseUnitInfo.Value`-sourced canonical unit, an explicit display unit defaulting to canonical, its own declared SI base-dimension vector, and the relative/absolute tolerance pair the `Equivalent` proof reads; `Frequency` admits the `Stats/signal` modal and `Analysis/structural` dynamic frequencies (Hz, distinct from the rev/min `RotationalSpeed`), `ThermalConductivity` the `Analysis/physics` material-λ inputs, `VolumeFlow` the `Analysis/energy` ventilation rates, and `Irradiance`/`Luminance`/`LuminousFlux`/`LuminousIntensity` the `Analysis/daylight` solar and sky-model photometrics beside the existing `Illuminance` row.
- Law: the tolerance is a PAIR per row — a relative term deciding at every scale and an absolute floor deciding near zero, compared as `|a-b| <= max(floor, relative * max(|a|,|b|))` on the SI canonical scalars. Lone absolutes read as exact equality on a metre-scale length and as everything-equal on a joule-scale energy, so the measured absolutes become floors and one default relative carries the compare across every row.
- Law: the canonical `Enum` column reads `QuantityInfo.BaseUnitInfo.Value` once per row at static construction — the metadata IS the column, so a hand-passed canonical arg is deleted, and UnitsNet emits no `DefaultUnitAttribute`/`DisplayAsUnitAttribute` on the generated types, so attribute reflection over them resolves to nothing; the display column is the explicit presentation `Enum` defaulting to canonical. `Probe()` is the composition-time coherence guard — a row whose display `Enum` does not belong to its `QuantityInfo.UnitType` (a cross-family typo such as a `PressureUnit` on the `Length` row) drifts as a `ComputeFault` at composition rather than failing the first `Render`; the canonical column needs no such guard because it is read FROM `BaseUnitInfo.Value` and is type-correct by construction.
- Entry: `Admit(QuantityInput, UnitPolicy, CorrelationId)` — `Fin<UnitEvidence>` aborts; the `QuantityInput` `[Union]` discriminates typed quantity, text, value-plus-unit, and value-plus-abbreviation payloads through one generated total `Switch`; the correlation is the corpus-wide typed `CorrelationId` the admission spine threads (`AdmittedIntent.Correlation`), never a bare `Guid`, so `UnitProject` intents enter `Admit` carrying the identity the `Runtime/admission` rail already minted. `Equivalent(IQuantity, IQuantity)` is `Fin<bool>` — family membership proves before the compare, so a cross-family pair is a typed fault and never a `false` a caller reads as a measured difference — and the compare itself is relative with the row's absolute as its near-zero floor, run on the SI canonical scalars; `Aggregate(Seq<IQuantity>, AggregateOp, UnitPolicy, CorrelationId)` folds a same-family sequence at the canonical unit through the `AggregateOp` row's `UnitMath` delegate and re-enters `Admit`, so aggregates and single values ride one evidence rail.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule — the typed `CorrelationId`)
- Growth: one table row on `QuantityFamily` per further AEC quantity, admitted only when a consumer exists — its canonical column sourcing from `QuantityInfo.BaseUnitInfo.Value` and its display column an explicit per-row `Enum`; each row declaring its own SI dimension so the composition fold covers it with no edit at `DIMENSIONAL_LAW`; the rows close the families the current symbolic, solver, and analysis consumers admit, so a speculative row with no consumer is the rejected addition; zero new surface.
- Boundary: conversion runs exactly once at admission and interior numerics are raw doubles owned by Rasm core — a quantity type in an interior signature is the seam violation this table deletes. Unit-admission failures mint `ComputeFault` through the dual-tier `Create` text route on the 2200 code band, the units-boundary contribution to the intent-and-selection fault union. `UnitsNetSetup.Default` is the single setup root composed once at the composition root: its `UnitParser`, `UnitAbbreviations`, `QuantityParser`, and `UnitConverter` properties are the reads this page spells, because the `UnitParser.Default`/`UnitAbbreviationsCache.Default`/`UnitConverter.Default` facades are shortcuts FORWARDING to that root and a second spelling of one instance is drift waiting for a second setup. One exception survives: `UnitConverter.Convert`/`TryConvert` stay static reads because the root's converter publishes no instance twin, and a second setup instance is rejected. NodaTime owns interior time, so the duration row exists only to canonicalize boundary text to seconds before rail time takes over. `UnitProject` intents enter `Admit` and the `Pipeline` intent case composes it.

```csharp signature
// --- [BOUNDARIES] ----------------------------------------------------------------------
internal static class UnitMetadata {
    public static Seq<UnitInfo> ConvertTargets(QuantityInfo info) =>
        toSeq(info.UnitInfos);

    public static Fin<Unit> Probe() =>
        toSeq(QuantityFamily.Items)
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
    public static readonly QuantityFamily Length = new("length", UnitsNet.Length.Info, dimension: [1, 0, 0, 0, 0, 0, 0], floor: 1e-9, display: LengthUnit.Millimeter);
    public static readonly QuantityFamily Area = new("area", UnitsNet.Area.Info, dimension: [2, 0, 0, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Volume = new("volume", UnitsNet.Volume.Info, dimension: [3, 0, 0, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Mass = new("mass", UnitsNet.Mass.Info, dimension: [0, 1, 0, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Duration = new("duration", UnitsNet.Duration.Info, dimension: [0, 0, 1, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Speed = new("speed", UnitsNet.Speed.Info, dimension: [1, 0, -1, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Acceleration = new("acceleration", UnitsNet.Acceleration.Info, dimension: [1, 0, -2, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Force = new("force", UnitsNet.Force.Info, dimension: [1, 1, -2, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Pressure = new("pressure", UnitsNet.Pressure.Info, dimension: [-1, 1, -2, 0, 0, 0, 0], floor: 1e-9, display: PressureUnit.Kilopascal);
    public static readonly QuantityFamily Energy = new("energy", UnitsNet.Energy.Info, dimension: [2, 1, -2, 0, 0, 0, 0], floor: 1e-9, display: EnergyUnit.KilowattHour);
    public static readonly QuantityFamily Power = new("power", UnitsNet.Power.Info, dimension: [2, 1, -3, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Temperature = new("temperature", UnitsNet.Temperature.Info, dimension: [0, 0, 0, 0, 1, 0, 0], floor: 1e-6, display: TemperatureUnit.DegreeCelsius);
    public static readonly QuantityFamily Angle = new("angle", UnitsNet.Angle.Info, dimension: [0, 0, 0, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Torque = new("torque", UnitsNet.Torque.Info, dimension: [2, 1, -2, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Ratio = new("ratio", UnitsNet.Ratio.Info, dimension: [0, 0, 0, 0, 0, 0, 0], floor: 1e-9, display: RatioUnit.Percent);
    public static readonly QuantityFamily Density = new("density", UnitsNet.Density.Info, dimension: [-3, 1, 0, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily AreaMomentOfInertia = new("area-moment-of-inertia", UnitsNet.AreaMomentOfInertia.Info, dimension: [4, 0, 0, 0, 0, 0, 0], floor: 1e-12);
    public static readonly QuantityFamily HeatTransferCoefficient = new("heat-transfer-coefficient", UnitsNet.HeatTransferCoefficient.Info, dimension: [0, 1, -3, 0, -1, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily ThermalResistance = new("thermal-resistance", UnitsNet.ThermalResistance.Info, dimension: [0, -1, 3, 0, 1, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily Illuminance = new("illuminance", UnitsNet.Illuminance.Info, dimension: [-2, 0, 0, 0, 0, 0, 1], floor: 1e-6);
    public static readonly QuantityFamily RotationalSpeed = new("rotational-speed", UnitsNet.RotationalSpeed.Info, dimension: [0, 0, -1, 0, 0, 0, 0], floor: 1e-9, display: RotationalSpeedUnit.RevolutionPerMinute);
    public static readonly QuantityFamily Frequency = new("frequency", UnitsNet.Frequency.Info, dimension: [0, 0, -1, 0, 0, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily ThermalConductivity = new("thermal-conductivity", UnitsNet.ThermalConductivity.Info, dimension: [1, 1, -3, 0, -1, 0, 0], floor: 1e-9);
    public static readonly QuantityFamily VolumeFlow = new("volume-flow", UnitsNet.VolumeFlow.Info, dimension: [3, 0, -1, 0, 0, 0, 0], floor: 1e-12);
    public static readonly QuantityFamily Irradiance = new("irradiance", UnitsNet.Irradiance.Info, dimension: [0, 1, -3, 0, 0, 0, 0], floor: 1e-6);
    public static readonly QuantityFamily Luminance = new("luminance", UnitsNet.Luminance.Info, dimension: [-2, 0, 0, 0, 0, 0, 1], floor: 1e-6);
    public static readonly QuantityFamily LuminousFlux = new("luminous-flux", UnitsNet.LuminousFlux.Info, dimension: [0, 0, 0, 0, 0, 0, 1], floor: 1e-6);
    public static readonly QuantityFamily LuminousIntensity = new("luminous-intensity", UnitsNet.LuminousIntensity.Info, dimension: [0, 0, 0, 0, 0, 0, 1], floor: 1e-6);

    public QuantityInfo Info { get; }

    // Each row's OWN declaration of its SI base-dimension vector, in the `BaseDimensions` axis order, stated
    // independently of `Info` — which is exactly what lets the composition fold catch a row bound to the wrong
    // `QuantityInfo`, the one drift a metadata-sourced column can never see.
    public ImmutableArray<int> Dimension { get; }

    public Enum Canonical { get; }

    public Enum Display { get; }

    // Tolerance is a PAIR: its relative term decides at every scale and its absolute term is the near-zero
    // floor. Pure absolutes read as exact equality on a metre-scale length and as everything-equal on a
    // joule-scale energy, so each measured absolute becomes a floor and the relative term carries the compare.
    public double AbsoluteFloor { get; }

    public double RelativeTolerance { get; }

    const double RelativeDefault = 1e-12;

    private QuantityFamily(string key, QuantityInfo info, ImmutableArray<int> dimension, double floor, Enum? display = null, double relative = RelativeDefault) : this(key) {
        Info = info;
        Dimension = dimension;
        Canonical = info.BaseUnitInfo.Value;
        Display = display ?? Canonical;
        AbsoluteFloor = floor;
        RelativeTolerance = relative;
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
            : Try.lift<Option<Enum>>(() => UnitsNetSetup.Default.UnitParser.TryParse(unit, Info.UnitType, policy.Culture, out Enum? resolved)
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

    // FAMILY membership proves BEFORE the scalar compare: two quantities of different families are not
    // "unequal", they are incomparable, and a `false` there reads downstream as a measured difference the caller
    // then acts on. The compare itself is relative with the row's measured absolute as its near-zero floor, run
    // on the SI canonical scalars — the boxed `IQuantity.Equals(other, tolerance)` face carries only the absolute
    // form, so the relative rule has no spelling through it.
    public Fin<bool> Equivalent(IQuantity left, IQuantity right) =>
        left is null || right is null
            ? ComputeFault.Create($"unit-equivalent {Key}: null operand")
            : !left.QuantityInfo.BaseDimensions.Equals(Info.BaseDimensions) || !right.QuantityInfo.BaseDimensions.Equals(Info.BaseDimensions)
                ? ComputeFault.Create($"unit-equivalent {Key}: {left.QuantityInfo.Name} and {right.QuantityInfo.Name} are not both in family")
                : Try.lift<Fin<bool>>(() => (left.As(Canonical), right.As(Canonical)) is var (a, b) && double.IsFinite(a) && double.IsFinite(b)
                        ? Fin.Succ(Math.Abs(a - b) <= Math.Max(AbsoluteFloor, RelativeTolerance * Math.Max(Math.Abs(a), Math.Abs(b))))
                        : ComputeFault.Create($"unit-equivalent {Key}: non-finite canonical value"))
                    .Run()
                    .MapFail(error => (Error)ComputeFault.Create($"unit-equivalent {Key}: {error.Message}"))
                    .Bind(identity);

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
```

## [03]-[DIMENSIONAL_LAW]

- Owner: `UnitPolicy` configuration-bound policy record; `UnitAlgebra` static dimensional surface.
- Cases: every admitted row proves its OWN declared dimension against `QuantityInfo.BaseDimensions` in one fold over `Items`; speed, acceleration, force, pressure, energy, power, torque, density — compound CROSS-ROW relations, each verifying the composed `BaseDimensions` of its two factors equals the compound row's; one reciprocal-pair row (heat-transfer-coefficient · thermal-resistance is dimensionless).
- Law: the two legs are disjoint by construction. Per-row soundness is TOTAL — the fold visits every `Items` member, so a row bound to the wrong `QuantityInfo` (a `speed` key pointing at `Acceleration.Info`) drifts at composition no matter how young the row is, and it needs no hand-written entry to be covered. Hand relations survive ONLY where a statement spans two or more rows and no single row's declaration carries it: `pressure = force / area` claims three rows at once. Self-only hand rows — a dimensionless list naming `ratio` — restate what that row's own declaration already proves and are the deleted form.
- Law: `UnitMath` owns explicit-unit sequence folds and pairwise arithmetic, coercing through `As(unit)` at the family canonical. `AggregateOp` exposes those folds through `QuantityFamily.Aggregate`, which re-enters `UnitEvidence`.
- Entry: `Consistency()` — `Fin<Unit>` aborts with the drifted row keys at composition, chaining the total declaration fold, the cross-row relation fold, and `UnitMetadata.Probe()` so declaration, algebra, and metadata drift all close one sweep.
- Packages: UnitsNet, LanguageExt.Core, BCL inbox
- Growth: a new admitted family is one `QuantityFamily` row carrying its own dimension declaration, which the total fold covers with zero edit here; a new cross-row identity is one row in `Relations` or `Reciprocals`; zero new surface.
- Boundary: `UnitPolicy` binds at `Section` through the configuration rail and carries the resolved `CultureInfo`; `Baseline` pins `UnitSystem.SI`. Admission checks `Dimensions.Equals(Info.BaseDimensions)`, and `Consistency` proves declared, compound, and reciprocal claims through UnitsNet `BaseDimensions`. `UnitMetadata.Probe()` joins the same composition fault, while numeric-only conversion rides the static `UnitConverter.TryConvert` — the one converter verb the setup root publishes no instance twin of — without constructing an `IQuantity`.

```csharp signature
public sealed record UnitPolicy(CultureInfo Culture, string Format = "G") {
    public const string Section = nameof(UnitPolicy);

    public UnitSystem Baseline => UnitSystem.SI;
}

public static class UnitAlgebra {
    private static readonly Func<BaseDimensions, BaseDimensions, BaseDimensions> Product = static (left, right) => left.Multiply(right);

    private static readonly Func<BaseDimensions, BaseDimensions, BaseDimensions> Quotient = static (left, right) => left.Divide(right);

    // CROSS-ROW algebra only: each entry relates two rows to a third, which no single row's own declaration can
    // state. A relation restating one row's dimension against itself is covered by `Declared` and never listed.
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
        (Declared()
                + Relations.Filter(static row => !row.Compose(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).Equals(row.Compound.Info.BaseDimensions)).Map(static row => row.Compound.Key)
                + Reciprocals.Filter(static row => !Product(row.Left.Info.BaseDimensions, row.Right.Info.BaseDimensions).IsDimensionless()).Map(static row => row.Left.Key)) is { IsEmpty: false } drift
            ? ComputeFault.Create($"unit-dimensions: {string.Join(", ", drift)} drift at composition")
            : UnitMetadata.Probe();

    // TOTAL over `Items`: the row's own declared vector against the metadata it binds. This is the leg that
    // catches a row wired to the wrong `QuantityInfo`, which every metadata-sourced column reads as correct.
    static Seq<string> Declared() =>
        toSeq(QuantityFamily.Items)
            .Filter(static row => !row.Dimension.SequenceEqual(Axes(row.Info.BaseDimensions)))
            .Map(static row => row.Key);

    static ImmutableArray<int> Axes(BaseDimensions dims) =>
        [dims.Length, dims.Mass, dims.Time, dims.Current, dims.Temperature, dims.Amount, dims.LuminousIntensity];

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
- Law: `UnitFormatter` is `internal`, so the consumable bare-unit surface is the abbreviation cache; the metadata-driven picker holds each conversion target's `UnitInfo` from `ConvertTargets`, so it resolves the glyph and the full alias set through `UnitsNetSetup.Default.UnitAbbreviations.GetAbbreviations(UnitInfo, policy.Culture)` — the `UnitInfo`-keyed overload, because the generic `GetDefaultAbbreviation<TUnit>`/`GetUnitAbbreviations<TUnit>` cannot bind a `TUnit` over a runtime-enumerated unit. `Catalogue()` projects the `Info` rows for the picker and `UnitMetadata.ConvertTargets` projects each family's `QuantityInfo.UnitInfos` as `Seq<UnitInfo>` (each carrying `.Value`/`.Name`/`.PluralName` and feeding `GetAbbreviations`), all with zero custom-attribute reflection.
- Law: evidence fields are plain strings and doubles, so the record serializes through the package wire context while UnitsNet types never cross a JSON or proto wire — conversion-at-admission is what enforces the recorded UnitsNet-serialization SKIP.
- Entry: `Render(UnitPolicy policy)` — total display projection; formatting never round-trips into computation.
- Receipt: `UnitEvidence` — family key, original unit and value, canonical unit and value, correlation id; the receipt union's unit-projection case carries it verbatim.
- Packages: UnitsNet, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `QuantityInfo` catalogue row per admitted family — the dashboard quantity picker derives from `Catalogue()`; zero new surface.
- Boundary: boundary text parses culture-scoped through `Quantity.TryParse(policy.Culture, Info.ValueType, ...)`, with `Resolve` owning abbreviation→`Enum` resolution through `UnitsNetSetup.Default.UnitParser` over that root's `UnitAbbreviations`, whose lookup falls back to the invariant-culture abbreviation set when the policy culture lacks a localized one. `Render` takes a `UnitProject` target unit as the resolved target override and renders through the boxed `IFormattable.ToString(format, culture)` face — the generic `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, …)` cannot bind a runtime-boxed `IQuantity`, so the boxed formattable face IS the dynamic rendering surface; the precision column is the format-string row carried on `UnitPolicy`, never a per-call-site `ToString` overload.

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
        toSeq(QuantityFamily.Items).Map(static row => row.Info);

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
