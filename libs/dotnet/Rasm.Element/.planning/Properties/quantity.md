# [ELEMENT_QUANTITY]

`MeasureValue` carries its SI magnitude, its OPTIONAL canonical unit token, a `Dimension` `[ComplexValueObject]` over the seven SI base-dimension exponents, and a `QuantityType` `[ValueObject<string>]` discriminator — coerced to its SI base at admission and read back through type-checked accessors. Name beats dimension: the seven-exponent vector is non-injective (`Torque`/`Energy` share `[L²·M·T⁻²]`; `Angle`, `Ratio`, `Level`, and `Count` share the zero vector; `AreaMomentOfInertia`/`TorsionConstant` share `[L⁴]`), so the registry `QuantityInfo.Name` — or a consumer's engineering-domain name minted through the open path — is the identity and the vector the physical signature.

`UnitScheme` is the model-level unit regime the `Graph/element#ELEMENT_GRAPH` `Header` carries: a `QuantityType`-grained display override map, the eight-axis affine set an IFC unit assignment declares (the offset arm `IfcConversionBasedUnitWithOffset` requires), and a culture/format policy — ingress coercion and egress re-emission of the model's declared units, presentation-only against every canonical byte. `MeasureEvidence` is the ONE conversion outcome a foreign-unit admission mints, its `UnitResolution` naming whether the source unit was declared, inferred, assumed, or caller-overridden.

SI reprojection rides the kernel `Try.lift` funnel so the `UnitsNet` boundary throw never escapes the result; a non-finite or unresolvable measure returns `KernelFault.OutOfRange` or `KernelFault.InvalidValue`. Dimensioned cross-quantity algebra is ONE `Combine` over `DimensionOp` rows (forward and inverse of one correspondence, one owner), each fold propagating the optional `MeasureBand` kind-dispatched. `MeasureStat` composes the kernel `Stat<Scalar>` beside a `QuantitySignature`, and `MeasureCanon` is the dimensioned `CanonicalWriter` leg the kernel writer's card cedes to this contract.

## [01]-[INDEX]

- [02]-[DIMENSION]: `Dimension` the seven-SI-exponent signature as ONE declaration table (each row naming its exponents AND its SI symbol once, the symbol map derived), `DimensionAxis` the eight-axis vocabulary an IFC unit assignment keys, and `QuantityType` the open identity discriminator with its `[ValidationError]` result.
- [03]-[MEASURE_VALUE]: `MeasureValue` the SI-coerced carrier — the `UnitsNet` admission with its expected-family gate, the ONE provenance-keyed `OfSi` mint, the polymorphic `As` read, the unit-aware `In` egress, and the `Combine`/`Scale`/`Sum` algebra with kind-dispatched `MeasureBand` propagation.
- [04]-[UNIT_SCHEME]: `UnitScheme` the declared-unit regime (overrides, axes, culture), `MeasureEvidence` + `UnitResolution` the conversion outcome, and the `QuantityType` relation/reciprocal roster with its `Consistency` gate and the evidence-minting `Admit`.
- [05]-[MEASURE_STAT]: `QuantitySignature` the quantity triple as one value, `MeasureStat` the kernel-`Stat<Scalar>` composition over it, and `MeasureCanon` the dimensioned canonical-writer extension.

## [02]-[DIMENSION]

- Owner: `Dimension` the `[ComplexValueObject]` carrying the seven SI base-dimension exponents — ONE declaration per rostered dimension names its exponents and its SI coherent-unit symbol together, and the symbol lookup DERIVES from those rows through an accessor-backed lazy, so the two former parallel tables (static rows beside a hand symbol map) are one authority; `DimensionAxis` the `[SmartEnum<int>]` eight-axis vocabulary (`Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`Luminous` reading their exponent component, `PlaneAngle` the IFC display axis outside the SI vector); `QuantityType` the `[ValueObject<string>]` quantity-type identity under `[ValidationError]`, so its generated `Validate` returns the domain fault and the `Of` result lifts with no translation hop.
- Entry: `Dimension.Of(BaseDimensions)` lowers the `UnitsNet` 7-vector; `Dimension.Create(int×7)` is the generated factory for unrostered signatures (`L⁴`/`L⁶`); `Multiply`/`Divide` compose exponent vectors so `ForceDim.Divide(AreaDim)` IS `PressureDim` with no per-quantity row; `CanonicalBytes(CanonicalWriter)` writes the 7-vector once, owned HERE so no composer hand-spells it. An untrusted quantity name admits through `FactoryBridge.Accept<QuantityType>(name)`; generated `Create` serves compile-time roster literals; `OfDimension(dimension)` mints the dimension-anonymous identity (tilde-prefixed, dimension-unique, NEVER equal to a QTO row); the rostered rows spell `Create(nameof(X))` so member name and registry name are one fact.
- Auto: all SEVEN base dimensions roster (`Current` among them — a base SI dimension is never earned by consumer count), each with its coherent symbol; `Dimensionless` carries an explicit absent symbol because a ratio, an angle, and a tally have no SI token and a blank entry is a fabricated token one character shorter; `DimensionAxis.Pure(dimension)` answers the single axis a unit-basis vector lives on (the offset-legality read — offsets do not distribute over products, so only a pure single-axis quantity takes an affine).
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`/`[ValueObject<string>]`/`[SmartEnum<int>]`), UnitsNet (`BaseDimensions`, `QuantityInfo.Name`), `Rasm` (kernel `CanonicalWriter`).
- Growth: a new well-known dimension is ONE `Row` call naming exponents and symbol together; a registry quantity a second consumer keys by name is one `QuantityType` row spelled `Create(nameof(X))`; a consumer's engineering-domain quantity mints through generated `Create` with no shared row (`SectionModulus`/`TorsionConstant`/`WarpingConstant` — the section second moment is the registry's `AreaMomentOfInertia` and rosters); never a per-quantity dimension type and never a closed `QuantityKind` enum.
- Boundary: `QuantityType` is the ONE discriminator and `Dimension` the physical signature — a closed kind enum and dimension-as-discriminator are the two deleted forms; exponents come from `BaseDimensions` or the generated factory, and a hand table drifting from the registry — or a name parsed at a call site rather than admitted through the kernel generated-owner bridge — is the named defect; `PlaneAngle` participates in unit coercion through the `Angle` TYPE arm alone (its SI exponent vector is zero), the discriminant stated on its row.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Properties;

// --- [TYPES] ---------------------------------------------------------------------------
file static class Probe {
 internal static Option<TValue> Find<TKey, TValue>(FrozenDictionary<TKey, TValue> table, TKey key) where TKey : notnull =>
  table.TryGetValue(out TValue? held) ? Some(held!) : None;
}

[ComplexValueObject]
public sealed partial class Dimension {
 public int Length { get; }
 public int Mass { get; }
 public int Time { get; }
 public int Current { get; }
 public int Temperature { get; }
 public int Amount { get; }
 public int LuminousIntensity { get; }

 static readonly List<(Dimension Row, string? Symbol)> Rostered = [];
 static Dimension Row(int length, int mass, int time, int current, int temperature, int amount, int luminous, string? symbol) {
  Dimension row = Create(length, mass, time, current, temperature, amount, luminous);
  Rostered.Add((row, symbol));
  return row;
 }

 public static readonly Dimension Dimensionless = Row(0, 0, 0, 0, 0, 0, 0, symbol: null);
 public static readonly Dimension LengthDim = Row(1, 0, 0, 0, 0, 0, 0, "m");
 public static readonly Dimension MassDim = Row(0, 1, 0, 0, 0, 0, 0, "kg");
 public static readonly Dimension DurationDim = Row(0, 0, 1, 0, 0, 0, 0, "s");
 public static readonly Dimension CurrentDim = Row(0, 0, 0, 1, 0, 0, 0, "A");
 public static readonly Dimension TemperatureDim = Row(0, 0, 0, 0, 1, 0, 0, "K");
 public static readonly Dimension AmountDim = Row(0, 0, 0, 0, 0, 1, 0, "mol");
 public static readonly Dimension LuminousIntensityDim = Row(0, 0, 0, 0, 0, 0, 1, "cd");
 public static readonly Dimension AreaDim = Row(2, 0, 0, 0, 0, 0, 0, "m2");
 public static readonly Dimension VolumeDim = Row(3, 0, 0, 0, 0, 0, 0, "m3");
 public static readonly Dimension ForceDim = Row(1, 1, -2, 0, 0, 0, 0, "N");
 public static readonly Dimension PressureDim = Row(-1, 1, -2, 0, 0, 0, 0, "Pa");
 public static readonly Dimension DensityDim = Row(-3, 1, 0, 0, 0, 0, 0, "kg/m3");
 public static readonly Dimension LinearDensityDim = Row(-1, 1, 0, 0, 0, 0, 0, "kg/m");
 public static readonly Dimension IrradianceDim = Row(0, 1, -3, 0, 0, 0, 0, "W/m2");
 public static readonly Dimension ThermalTransmittanceDim = Row(0, 1, -3, 0, -1, 0, 0, "W/(m2.K)");

 static readonly Lazy<FrozenDictionary<Dimension, string>> Symbols = new(static () =>
  Rostered.Where(static entry => entry.Symbol is not null)
   .ToFrozenDictionary(static entry => entry.Row, static entry => entry.Symbol!));

 public Option<string> SiSymbol => Probe.Find(Symbols.Value, this);

 public static Dimension Of(BaseDimensions d) =>
  Create(d.Length, d.Mass, d.Time, d.Current, d.Temperature, d.Amount, d.LuminousIntensity);

 public Dimension Multiply(Dimension other) =>
  Create(Length + other.Length, Mass + other.Mass, Time + other.Time, Current + other.Current,
   Temperature + other.Temperature, Amount + other.Amount, LuminousIntensity + other.LuminousIntensity);

 public Dimension Divide(Dimension other) =>
  Create(Length - other.Length, Mass - other.Mass, Time - other.Time, Current - other.Current,
   Temperature - other.Temperature, Amount - other.Amount, LuminousIntensity - other.LuminousIntensity);

 public void CanonicalBytes(CanonicalWriter writer) =>
  writer.Ordinal(Length).Ordinal(Mass).Ordinal(Time).Ordinal(Current)
   .Ordinal(Temperature).Ordinal(Amount).Ordinal(LuminousIntensity);
}

[SmartEnum<int>]
public sealed partial class DimensionAxis {
 public static readonly DimensionAxis Length = new(0, static d => d.Length);
 public static readonly DimensionAxis Mass = new(1, static d => d.Mass);
 public static readonly DimensionAxis Time = new(2, static d => d.Time);
 public static readonly DimensionAxis Current = new(3, static d => d.Current);
 public static readonly DimensionAxis Temperature = new(4, static d => d.Temperature);
 public static readonly DimensionAxis Amount = new(5, static d => d.Amount);
 public static readonly DimensionAxis Luminous = new(6, static d => d.LuminousIntensity);
 public static readonly DimensionAxis PlaneAngle = new(7, static _ => 0);

 [UseDelegateFromConstructor]
 public partial int Exponent(Dimension dimension);

 public static Option<DimensionAxis> Pure(Dimension dimension) =>
  toSeq(Items).Filter(axis => axis.Exponent(dimension) != 0) is { Count: 1 } sole
  && sole.Head.ForAll(axis => axis.Exponent(dimension) == 1)
   ? sole.Head
   : None;
}

[ValueObject<string>]
[ValidationError]
public sealed partial class QuantityType {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim();
  validationError = value.Length == 0 ? new ValidationError("quantity type must be non-blank") : validationError;
 }

 public static readonly QuantityType Length = Create(nameof(Length));
 public static readonly QuantityType Area = Create(nameof(Area));
 public static readonly QuantityType Volume = Create(nameof(Volume));
 public static readonly QuantityType Mass = Create(nameof(Mass));
 public static readonly QuantityType Duration = Create(nameof(Duration));
 public static readonly QuantityType Angle = Create(nameof(Angle));
 public static readonly QuantityType AreaMomentOfInertia = Create(nameof(AreaMomentOfInertia));
 public static readonly QuantityType Count = Create(nameof(Count));
 public static readonly QuantityType Scalar = Create(nameof(Scalar));

 public static QuantityType OfDimension(Dimension d) =>
  d == Dimension.Dimensionless
   ? Scalar
   : Create($"~{d.Length}.{d.Mass}.{d.Time}.{d.Current}.{d.Temperature}.{d.Amount}.{d.LuminousIntensity}");
}
```

## [03]-[MEASURE_VALUE]

- Owner: `MeasureValue` the SI-coerced measured-scalar carrier (`QuantityType Type` + `Dimension Dimension` + `double Si` + `Option<string> CanonicalUnit` + `Option<MeasureBand> Uncertainty`); `MeasureBand` the neutral uncertainty carrier; `UncertaintyKind` the closed uncertainty vocabulary whose `Gaussian` column dispatches propagation; `UnitProvenance` the `[Union]` naming HOW an SI-native mint's canonical unit resolves — `Derive` (the registry/dimension probe), `Label(string)` (a conventional token the registry cannot supply), `Carried(Option<string>)` (a triple a prior admission already stamped) — so the former five mint arities are ONE `OfSi` whose provenance is a value; `DimensionOp` the `[SmartEnum]` rows (`Product`/`Quotient`) carrying exponent composition, scalar op, first-order partials, and the zero-spanning-divisor widening policy, so `Combine` is ONE algebra owner and `Multiply`/`Divide` one-hop reads over it.
- Entry: `Of(value, unit, expected)` admits through `Quantity.TryFrom` + the ONE `SiUnit` election, the optional `expected` family gating name identity (UnitsNet collapses lumen/candela/nit onto one signature, so NAME is load-bearing); `Of(value, unitAbbreviation, expected)` is the invariant-culture string ingress; `OfSi(type, dimension, si, provenance)` the SI-native mint (`provenance` defaults `Derive`); `OfSi(dimension, si)` the dimension-anonymous one-hop; `Combine(other, DimensionOp)` the cross-quantity algebra; `Scale(factor)` the type-preserving multiple; `Sum(measures)` the accumulating same-type reduction; `WithType` the band-preserving re-stamp; `WithUncertainty(band)` the contains-the-nominal band admission; `As(QuantityType)`/`In(Enum)`/`In(string)` the reads.
- Auto: `Coerce` reprojects every admission through the ONE `SiUnit` election — declared `BaseUnitInfo` by default, one `SiElection` row per departure (a different coherent unit, or `None` refusing by name) — so the persisted scalar is base-normalized whatever the admission spelling; `Sum` accumulates EVERY cross-type member on `Validation` (naming index and type per offender) before the fold runs; band propagation is `CombineBand` kind-dispatched on the `Gaussian` column, corner envelopes fold in ONE pass through `MeasureBand.Envelope`, and every arm that would mint a zero-width band answers `None` (EXACTNESS IS BAND ABSENCE).
- Output: a `MeasureValue` is the unit-checked magnitude a takeoff, a property facet, and a cost join read; `measure.Uncertainty` is absent exactly where the value is exact, and the uncertainty producer stays above the boundary.
- Packages: UnitsNet (`Quantity.TryFrom`/`TryFromUnitAbbreviation` ingress, `Quantity.Infos`/`UnitInfo` the once-built index, `UnitConverter.TryConvert` the guarded egress — the struct-native `As` conversion under the hood; the instance `TryGetConversionFunction` store serves CUSTOM registrations only, decompile-proven, so binding it would MISS every built-in conversion — `QuantityInfo.BaseUnitInfo`/`BaseDimensions`), Thinktecture.Runtime.Extensions, LanguageExt.Core, `Rasm` (kernel `Try.lift`).
- Growth: a new measured quantity with an SI-coherent declared base needs NO contract edit; a departing quantity is ONE `SiElection` row whichever kind (elected unit or refusal); a new derived takeoff composes `Combine`; a new mint provenance is one `UnitProvenance` case; a new uncertainty model is one `UncertaintyKind` row naming its `Gaussian` column; never a mint arity, a per-quantity branch, or a parallel operation family.
- Boundary: the interior NEVER carries a bare `double` quantity, and every construction path and algebra exit holds the FINITE invariant — a NaN/∞ magnitude is unrepresentable in an admitted value, while infinite BAND bounds stay honest uncertainty; `PlaneAngle` stores radians; the ADMITTED CLASS is stated, not discovered — `SiElection`'s `None` rows refuse by name (`<measure-si-incoherent:Name>`) because a decibel in the linear algebra is a physics error and a prefixed base persists wrong by a power of ten; `Derive` provenance gates a registry-named type's dimension, `Label` REFUSES a registry-named type (a per-call-site label can never fork a registry quantity's unit), and `Carried` gates finiteness alone because its triple is a prior admission's evidence — the three postures are union CASES, never sibling arities; `CanonicalUnit` is `Option<string>` and an unresolvable unit is ABSENCE, never a fabricated token; egress is `In` over the once-built index + `UnitConverter.TryConvert`, total (`None` for absent unit, wrong family, consumer mint, dimension-anonymous product), and the named QTO reads derive from the ONE `As` body; tolerance quantization is the KERNEL writer's — `CanonicalWriter.Double` grids every magnitude and band bound on the writer's own tolerance at the `MeasureCanon` write, so no second rounding owner exists on this page and the interior stays full-precision.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UncertaintyKind {
 public static readonly UncertaintyKind Exact = new("exact", gaussian: true);
 public static readonly UncertaintyKind Absolute = new("absolute", gaussian: false);
 public static readonly UncertaintyKind Relative = new("relative", gaussian: false);
 public static readonly UncertaintyKind Interval = new("interval", gaussian: false);
 public static readonly UncertaintyKind Normal = new("normal", gaussian: true);

 public bool Gaussian { get; }
}

public sealed record MeasureBand {
 private MeasureBand(UncertaintyKind kind, double lowerSi, double upperSi, Option<double> standardDeviationSi, Option<double> coverageFactor) =>
  (Kind, LowerSi, UpperSi, StandardDeviationSi, CoverageFactor) = (kind, lowerSi, upperSi, standardDeviationSi, coverageFactor);

 public UncertaintyKind Kind { get; }
 public double LowerSi { get; }
 public double UpperSi { get; }
 public Option<double> StandardDeviationSi { get; }
 public Option<double> CoverageFactor { get; }

 public static Fin<MeasureBand> Admit(
  UncertaintyKind kind, double lowerSi, double upperSi,
  Option<double> standardDeviationSi, Option<double> coverageFactor) =>
  double.IsNaN(lowerSi)
   ? new KernelFault.OutOfRange("measure-band-lower", lowerSi, "not be NaN")
   : double.IsNaN(upperSi)
    ? new KernelFault.OutOfRange("measure-band-upper", upperSi, "not be NaN")
    : lowerSi > upperSi
   ? new ElementFault.ValueRejected("<measure-band-bounds-invalid>")
   : kind == UncertaintyKind.Normal
    ? (standardDeviationSi, coverageFactor).Apply((sd, coverage) =>
       (Finite(("measure-band-lower", lowerSi), ("measure-band-upper", upperSi)),
        In(sd, Band.Positive, "measure-band-standard-deviation"),
        In(coverage, Band.Positive, "measure-band-coverage-factor"))
       .Apply((_, admittedSd, admittedCoverage) => new MeasureBand(kind, lowerSi, upperSi, Some(admittedSd), Some(admittedCoverage)))
       .As().ToFin())
      .As()
      .IfNone(new ElementFault.ValueRejected("<measure-band-normal-metadata-absent>"))
    : standardDeviationSi.IsSome || coverageFactor.IsSome
     ? new ElementFault.ValueRejected("<measure-band-metadata-kind-mismatch>")
     : Fin.Succ(new MeasureBand(kind, lowerSi, upperSi, None, None));

 internal static readonly MeasureBand Unbounded =
  new(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity, None, None);

 internal static MeasureBand Interval(UncertaintyKind kind, double lowerSi, double upperSi) =>
  new(kind, lowerSi, upperSi, None, None);

 internal static MeasureBand Normal(double lowerSi, double upperSi, double standardDeviationSi, double coverageFactor) =>
  new(UncertaintyKind.Normal, lowerSi, upperSi, Some(standardDeviationSi), Some(coverageFactor));

 internal static (double Floor, double Ceiling, bool Indeterminate) Envelope(params ReadOnlySpan<double> corners) {
  (double floor, double ceiling, bool indeterminate) = (double.PositiveInfinity, double.NegativeInfinity, false);
  foreach (double corner in corners) {
   indeterminate |= double.IsNaN(corner);
   floor = corner < floor ? corner : floor;
   ceiling = corner > ceiling ? corner : ceiling;
  }
  return (floor, ceiling, indeterminate);
 }
}

[Union]
public abstract partial record UnitProvenance {
 private UnitProvenance() { }
 public sealed record DeriveCase : UnitProvenance;
 public sealed record LabelCase(string Unit) : UnitProvenance;
 public sealed record CarriedCase(Option<string> Unit) : UnitProvenance;

 public static readonly UnitProvenance Derive = new DeriveCase();
 public static UnitProvenance Label(string unit) => new LabelCase(unit);
 public static UnitProvenance Carried(Option<string> unit) => new CarriedCase(unit);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DimensionOp {
 public static readonly DimensionOp Product = new("product", widensOnZeroSpanningDivisor: false,
  static (l, r) => l.Multiply(r), static (l, r) => l * r, static (l, r) => (r, l));
 public static readonly DimensionOp Quotient = new("quotient", widensOnZeroSpanningDivisor: true,
  static (l, r) => l.Divide(r), static (l, r) => l / r, static (l, r) => (1.0 / r, -l / (r * r)));

 public bool WidensOnZeroSpanningDivisor { get; }
 [UseDelegateFromConstructor] public partial Dimension Compose(Dimension left, Dimension right);
 [UseDelegateFromConstructor] public partial double Apply(double left, double right);
 [UseDelegateFromConstructor] public partial (double Dl, double Dr) Partials(double left, double right);
}

public sealed record MeasureValue {
 private MeasureValue(QuantityType type, Dimension dimension, double si, Option<string> canonicalUnit, Option<MeasureBand> uncertainty) =>
  (Type, Dimension, Si, CanonicalUnit, Uncertainty) = (type, dimension, si, canonicalUnit, uncertainty);

 public QuantityType Type { get; }
 public Dimension Dimension { get; }
 public double Si { get; }
 public Option<string> CanonicalUnit { get; }
 public Option<MeasureBand> Uncertainty { get; }

 public static readonly MeasureValue Zero =
  new(QuantityType.Scalar, Dimension.Dimensionless, 0.0, None, None);

 public Fin<MeasureValue> WithUncertainty(MeasureBand band) =>
  band.LowerSi <= Si && band.UpperSi >= Si
   ? Fin.Succ(new MeasureValue(Type, Dimension, Si, CanonicalUnit, Some(band)))
   : new ElementFault.ValueRejected("<measure-band-excludes-nominal>");

 public Fin<MeasureValue> WithType(QuantityType type) =>
  Probe.Find(Registry.Value.Dimensions, type.ToValue()).Filter(expected => expected != Dimension).IsSome
   ? new ElementFault.ValueRejected($"<measure-type-dimension-mismatch:{type.ToValue()}>")
   : Fin.Succ(new MeasureValue(type, Dimension, Si, CanonicalUnitFor(type, Dimension), Uncertainty));

 public static Fin<MeasureValue> Of(double value, Enum unit, Option<QuantityType> expected = default) =>
  !double.IsFinite(value)
   ? new KernelFault.OutOfRange("measure", value, "be finite")
   : Quantity.TryFrom(value, unit, out IQuantity? quantity) && quantity is { } q
    ? Coerce(q).Bind(admitted => Family(admitted, expected))
    : new KernelFault.InvalidValue("measure-unit", $"resolve {unit}");

 public static Fin<MeasureValue> Of(double value, string unit, Option<QuantityType> expected = default) =>
  !double.IsFinite(value)
   ? new KernelFault.OutOfRange("measure", value, "be finite")
   : Quantity.TryFromUnitAbbreviation(CultureInfo.InvariantCulture, value, unit, out IQuantity? quantity) && quantity is { } q
    ? Coerce(q).Bind(admitted => Family(admitted, expected))
    : new KernelFault.InvalidValue("measure-unit", $"resolve {unit}");

 static Fin<MeasureValue> Family(MeasureValue admitted, Option<QuantityType> expected) =>
  expected.Filter(family => family != admitted.Type).Match(
   Some: family => Fin.Fail<MeasureValue>(new ElementFault.ValueRejected($"<measure-family-mismatch:{family.ToValue()}:{admitted.Type.ToValue()}>")),
   None: () => Fin.Succ(admitted));

 static readonly FrozenDictionary<string, Option<Enum>> SiElection = new Dictionary<string, Option<Enum>>(StringComparer.Ordinal) {
  ["Angle"] = Some<Enum>(AngleUnit.Radian),
  ["MassFlow"] = Some<Enum>(MassFlowUnit.KilogramPerSecond),
  ["ThermalResistance"] = Some<Enum>(ThermalResistanceUnit.SquareMeterKelvinPerWatt),
  ["AmplitudeRatio"] = None, ["ApparentEnergy"] = None, ["ElectricApparentEnergy"] = None,
  ["ElectricReactiveEnergy"] = None, ["FuelEfficiency"] = None, ["Level"] = None,
  ["PowerRatio"] = None, ["ReactiveEnergy"] = None, ["RelativeHumidity"] = None,
  ["SpecificFuelConsumption"] = None,
 }.ToFrozenDictionary(StringComparer.Ordinal);

 static Option<Enum> SiUnit(QuantityInfo info) =>
  Probe.Find(SiElection, info.Name).IfNone(() => Some<Enum>(info.BaseUnitInfo.Value));

 static readonly Lazy<(
  FrozenDictionary<(string Quantity, string Unit), Enum> Units,
  FrozenDictionary<string, string> SiNames,
  FrozenDictionary<string, Dimension> Dimensions)> Registry = new(static () => (
  Quantity.Infos.SelectMany(static info => info.UnitInfos.Select(unit => KeyValuePair.Create((info.Name, unit.Name), unit.Value))).ToFrozenDictionary(),
  Quantity.Infos.AsIterable()
   .Choose(static info => SiUnit(info).Map(unit => (info.Name, Si: unit.ToString())))
   .ToFrozenDictionary(static row => row.Name, static row => row.Si),
  Quantity.Infos.ToFrozenDictionary(static info => info.Name, static info => Dimension.Of(info.BaseDimensions))));

 internal static Option<Dimension> DimensionOf(QuantityType type) => Probe.Find(Registry.Value.Dimensions, type.ToValue());

 public static Fin<MeasureValue> OfSi(QuantityType type, Dimension dimension, double si, Option<UnitProvenance> provenance = default) {
  return !double.IsFinite(si)
   ? new KernelFault.OutOfRange("measure-si", si, "be finite")
   : provenance.IfNone(UnitProvenance.Derive).Switch<Fin<MeasureValue>>(
    derive: _ => Probe.Find(Registry.Value.Dimensions, type.ToValue()).Filter(expected => expected != dimension).IsSome
     ? new ElementFault.ValueRejected($"<measure-type-dimension-mismatch:{type.ToValue()}>")
     : Fin.Succ(new MeasureValue(type, dimension, si, CanonicalUnitFor(type, dimension), None)),
    label: labeled => Registry.Value.Dimensions.ContainsKey(type.ToValue())
     ? new ElementFault.ValueRejected($"<measure-labeled-mint-registry-type:{type.ToValue()}>")
     : Fin.Succ(new MeasureValue(type, dimension, si, Some(labeled.Unit), None)),
    carried: carried => Fin.Succ(new MeasureValue(type, dimension, si, carried.Unit, None)));
 }

 public static Fin<MeasureValue> OfSi(Dimension dimension, double si) =>
  OfSi(QuantityType.OfDimension(dimension), dimension, si, Some(UnitProvenance.Derive));

 static Option<string> CanonicalUnitFor(QuantityType type, Dimension dimension) =>
  Probe.Find(Registry.Value.SiNames, type.ToValue()) | dimension.SiSymbol;

 static Fin<MeasureValue> Coerce(IQuantity quantity) =>
  SiUnit(quantity.QuantityInfo)
   .ToFin(new KernelFault.InvalidValue("measure-si-unit", $"resolve a coherent unit for {quantity.QuantityInfo.Name}"))
   .Bind(unit => Try.lift(() => Fin.Succ(quantity.ToUnit(unit))).Run().Bind(static inner => inner).Bind(si => Admit(si)));

 static Fin<MeasureValue> Admit(IQuantity si) =>
  double.IsFinite((double)si.Value)
   ? Fin.Succ(new MeasureValue(QuantityType.Create(si.QuantityInfo.Name), Dimension.Of(si.QuantityInfo.BaseDimensions), (double)si.Value, Some(si.Unit.ToString()), None))
   : new KernelFault.OutOfRange($"measure-si:{si.Unit}", (double)si.Value, "be finite");

 public Option<double> As(QuantityType type) => Type == type ? Some(Si) : None;

 public Option<double> Length => As(QuantityType.Length);
 public Option<double> Area => As(QuantityType.Area);
 public Option<double> Volume => As(QuantityType.Volume);
 public Option<double> Weight => As(QuantityType.Mass);
 public Option<double> Time => As(QuantityType.Duration);
 public Option<double> Count => As(QuantityType.Count);

 public Option<double> In(Enum unit) =>
  CanonicalUnit
   .Bind(stored => Probe.Find(Registry.Value.Units, (Type.ToValue(), stored)))
   .Bind(handle => UnitConverter.TryConvert(Si, handle, unit, out double converted) ? Some(converted) : None);

 public Option<double> In(string unitName) =>
  Probe.Find(Registry.Value.Units, (Type.ToValue(), unitName)).Bind(In);

 public static Fin<MeasureValue> Sum(Seq<MeasureValue> measures) {
  return measures.Head.Match(
   None: () => Fin.Succ(Zero),
   Some: head => Accumulate(measures.Tail.Map((member, index) => member.Type == head.Type
     ? Success<Error, Unit>(unit)
     : Fail<Error, Unit>(new ElementFault.ValueRejected($"<measure-sum-type-mismatch:index={index}:type={member.Type.ToValue()}>"))))
    .ToFin()
    .Bind(_ => {
     MeasureValue total = measures.Tail.Fold(head, static (acc, next) => acc.Add(next));
     return double.IsFinite(total.Si)
      ? Fin.Succ(total)
      : new KernelFault.OutOfRange("measure-sum", total.Si, "be finite");
    }));
 }

 MeasureValue Add(MeasureValue other) {
  double si = Si + other.Si;
  return new MeasureValue(Type, Dimension, si, CanonicalUnit, CombineBand(this, other, si, static (l, r) => l + r, static (_, _) => (1.0, 1.0)));
 }

 public Fin<MeasureValue> Combine(MeasureValue other, DimensionOp op) {
  Dimension composed = op.Compose(Dimension, other.Dimension);
  double si = op.Apply(Si, other.Si);
  if (!double.IsFinite(si)) {
   return new KernelFault.OutOfRange($"measure-{op.Key}", si, "be finite", Some());
  }
  (MeasureBand lb, MeasureBand rb) = (EffectiveBand(this), EffectiveBand(other));
  Option<MeasureBand> band =
   (Uncertainty.IsSome || other.Uncertainty.IsSome)
   && op.WidensOnZeroSpanningDivisor && !(lb.Kind.Gaussian && rb.Kind.Gaussian)
   && rb.LowerSi <= 0.0 && rb.UpperSi >= 0.0
    ? Some(MeasureBand.Unbounded)
    : CombineBand(other, si, op.Apply, op.Partials);
  return Fin.Succ(new MeasureValue(QuantityType.OfDimension(composed), composed, si, composed.SiSymbol, band));
 }

 public Fin<MeasureValue> Multiply(MeasureValue other) => Combine(other, DimensionOp.Product);
 public Fin<MeasureValue> Divide(MeasureValue other) => Combine(other, DimensionOp.Quotient);

 public Fin<MeasureValue> Scale(double factor) {
  double si = Si * factor;
  return double.IsFinite(si)
   ? Fin.Succ(new MeasureValue(Type, Dimension, si, CanonicalUnit,
      CombineBand(this, Exact(factor), si, static (l, r) => l * r, static (l, r) => (r, l))))
   : new KernelFault.OutOfRange("measure-scale", si, "be finite", Some());
 }

 static MeasureValue Exact(double scalar) => new(QuantityType.Scalar, Dimension.Dimensionless, scalar, None, None);

 internal static MeasureValue Reproject(QuantitySignature signature, double si) =>
  new(signature.Type, signature.Dimension, si, signature.CanonicalUnit, None);

 static Option<MeasureBand> CombineBand(MeasureValue left, MeasureValue right, double resultSi,
  Func<double, double, double> corner, Func<double, double, (double Dl, double Dr)> partials) =>
  left.Uncertainty.IsNone && right.Uncertainty.IsNone
   ? None
   : Propagate(EffectiveBand(left), EffectiveBand(right), left.Si, right.Si, resultSi, corner, partials);

 static Option<MeasureBand> Propagate(MeasureBand l, MeasureBand r, double leftSi, double rightSi, double resultSi,
  Func<double, double, double> corner, Func<double, double, (double Dl, double Dr)> partials) {
  if (l.Kind.Gaussian && r.Kind.Gaussian) {
   (double dl, double dr) = partials(leftSi, rightSi);
   double sigma = double.Hypot(dl * l.StandardDeviationSi.IfNone(0.0), dr * r.StandardDeviationSi.IfNone(0.0));
   double k = Math.Max(l.CoverageFactor.IfNone(1.0), r.CoverageFactor.IfNone(1.0));
   return sigma == 0.0
    ? None
    : double.IsFinite(sigma)
     ? Some(MeasureBand.Normal(resultSi - (k * sigma), resultSi + (k * sigma), sigma, k))
     : Some(MeasureBand.Unbounded);
  }
  (double floor, double ceiling, bool indeterminate) = MeasureBand.Envelope(
   corner(l.LowerSi, r.LowerSi), corner(l.LowerSi, r.UpperSi), corner(l.UpperSi, r.LowerSi), corner(l.UpperSi, r.UpperSi));
  return indeterminate
   ? (l.LowerSi == 0.0 && l.UpperSi == 0.0) || (r.LowerSi == 0.0 && r.UpperSi == 0.0)
    ? None
    : Some(MeasureBand.Unbounded)
   : floor == ceiling
    ? None
    : Some(MeasureBand.Interval(UncertaintyKind.Interval, floor, ceiling));
 }

 static MeasureBand EffectiveBand(MeasureValue value) =>
  value.Uncertainty
   .Filter(static band => band.Kind != UncertaintyKind.Exact)
   .IfNone(() => MeasureBand.Interval(UncertaintyKind.Exact, value.Si, value.Si));
}
```

## [04]-[UNIT_SCHEME]

- Owner: `UnitScheme` the model-level declared-unit regime the `Header` carries — `Overrides` the `QuantityType`-grained display map (an IFC derived unit is DECLARED, never synthesized, so a compound display unit is an override row), `Axes` the `DimensionAxis`-keyed affine set (`UnitAxis(Factor, Offset, Token)` — the offset arm `IfcConversionBasedUnitWithOffset` requires, without which every Fahrenheit/Rankine property is a wrong number with no fault), `CultureName`/`Format` the presentation policy (a culture NAME, not a `CultureInfo` reference — a mutable reference type is not an equality-safe value column); `MeasureEvidence` the ONE conversion outcome (`Family`, original pair, canonical pair, `UnitResolution`, `CorrelationId`); `UnitResolution` the declared/inferred/assumed/overridden policy vocabulary — the column that SAYS a unit was assumed or caller-forced, without which a STEP file with no `SI_UNIT` header silently becomes millimetres.
- Entry: `Coerce(native, type, dimension, Option<UnitAxis> declared = default)` lowers a project-unit magnitude onto SI — a per-VALUE declared axis overrides the regime whole-quantity (the IFC property/quantity-carried unit, affine and derived-multiplier forms included; ingress-only, so egress never reads it), else the `Angle` type arm reads the `PlaneAngle` axis, a pure single-axis dimension takes the affine `(native + Offset) × Factor`, a compound dimension composes the multiplicative factors per exponent (offsets do not distribute over products, the reason IFC carries them on base units alone); `Declare(axis, unit)`/`Declare(type, token)` build the regime one declaration at a time — the Bim ingress lowers `IfcUnitAssignment` row by row; `Render(measure)` re-emits declared units — override token first, declared-axis inverse for a pure single-axis measure, SI fallback; `Text(measure)` formats under the declared culture and format; `QuantityType.Admit(family, value, unit, resolution, correlation)` is the evidence-minting foreign admission — the expected-family gate plus the evidence mint in one entry.
- Auto: `QuantityType.Relations`/`Reciprocals` seat the cross-quantity roster where the rows live, and `Consistency()` proves every declared relation against the registry's own dimensions (a roster row wired to the wrong UnitsNet family is the defect every metadata-sourced column reads as correct) — accumulating, so one run names every inconsistent row.
- Output: `MeasureEvidence` is the audit row a Materials capture, a Fabrication solid ingress, and a Compute unit policy keep beside the admitted value; `Resolution` is its load-bearing column.
- Packages: LanguageExt.Core (`Map`/`Option`), `Rasm` (kernel `CorrelationId`), Thinktecture.Runtime.Extensions, BCL (`CultureInfo.GetCultureInfo`).
- Growth: a new declared unit is one `Declare` call; a new relation or reciprocal is one roster row `Consistency` immediately proves; a new resolution posture is one `UnitResolution` row; presentation never grows a second scheme type.
- Boundary: PRESENTATION ONLY — the interior stays SI, `Header.CanonicalBytes` EXCLUDES the scheme, and a re-declared display unit never forks a snapshot identity; the scheme never invents a token (`Render` falls back to the measure's own optional canonical unit); `UnitScheme` is the ONE unit-regime owner branch-wide — Bim's `UnitScale`/`UnitAxis`/`MeasureRow` twins are DELETED onto it (W3, retire-proofed; Bim's surviving IFC-name→`Dimension` signature is one column of genuine IFC-schema knowledge at `Projection/value.md`, never a unit algebra), while Materials' `MaterialUnits`/`EmissionEvidence`, Compute's `UnitPolicy`/`QuantityFamily`, and Fabrication's `SolidUnitPolicy` compose it at their waves (their mm-native `[U2]` columns stay bare doubles by their own ruling — EXTEND, never ABSORB); the `declared` tail is the ONE per-value override door — Bim `Projection/value#PROPERTY_LOWERING` is its one named consumer and recomposes its residue arithmetic onto it in its own W3 unit, so no boundary applies `(native + Offset) × Factor` by hand.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct UnitAxis(double Factor, double Offset, string Token);

public readonly record struct UnitScheme(
 Map<QuantityType, string> Overrides,
 Map<DimensionAxis, UnitAxis> Axes,
 string CultureName,
 string Format = "G") {

 public static readonly UnitScheme Si = new(Map<QuantityType, string>(), Map<DimensionAxis, UnitAxis>(), "");

 public UnitScheme Declare(DimensionAxis axis, UnitAxis unit) => this with { Axes = Axes.AddOrUpdate(axis, unit) };
 public UnitScheme Declare(QuantityType type, string unitToken) => this with { Overrides = Overrides.AddOrUpdate(type, unitToken) };

 public Option<string> UnitFor(QuantityType type) => Overrides.Find(type);

 public double Coerce(double native, QuantityType type, Dimension dimension, Option<UnitAxis> declared = default) =>
  (declared | (type == QuantityType.Angle ? Axes.Find(DimensionAxis.PlaneAngle) : Option<UnitAxis>.None))
   .Match(
    Some: axis => (native + axis.Offset) * axis.Factor,
    None: () => DimensionAxis.Pure(dimension).Bind(Axes.Find).Match(
     Some: axis => (native + axis.Offset) * axis.Factor,
     None: () => toSeq(Axes).Fold(native, (value, pair) => pair.Key.Exponent(dimension) switch {
      0 => value,
      var exponent => value * Math.Pow(pair.Value.Factor, exponent),
     })));

 public double Coerce(double native, QuantityType type) =>
  Coerce(native, type, MeasureValue.DimensionOf(type).IfNone(Dimension.Dimensionless));

 public (double Value, Option<string> Unit) Render(MeasureValue measure) =>
  UnitFor(measure.Type)
   .Bind(unit => measure.In(unit).Map(value => (Value: value, Unit: Some(unit))))
   .IfNone(() => Invert(measure).IfNone(() => (measure.Si, measure.CanonicalUnit)));

 Option<(double Value, Option<string> Unit)> Invert(MeasureValue measure) =>
  (measure.Type == QuantityType.Angle ? Some(DimensionAxis.PlaneAngle) : DimensionAxis.Pure(measure.Dimension))
   .Bind(Axes.Find)
   .Map(axis => (Value: (measure.Si / axis.Factor) - axis.Offset, Unit: Some(axis.Token)));

 public string Text(MeasureValue measure) {
  (double value, Option<string> unit) = Render(measure);
  string figure = value.ToString(Format, CultureInfo.GetCultureInfo(CultureName));
  return unit.Match(Some: token => $"{figure} {token}", None: () => figure);
 }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UnitResolution {
 public static readonly UnitResolution Declared = new("declared");
 public static readonly UnitResolution Inferred = new("inferred");
 public static readonly UnitResolution Assumed = new("assumed");
 public static readonly UnitResolution Overridden = new("overridden");
}

public readonly record struct MeasureEvidence(
 QuantityType Family, string OriginalUnit, double OriginalValue,
 string CanonicalUnit, double CanonicalValue,
 UnitResolution Resolution, CorrelationId Correlation);

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed partial class QuantityType {
 static readonly Lazy<Seq<(QuantityType Compound, QuantityType Left, QuantityType Right, DimensionOp Op)>> RelationRows = new(static () => Seq(
  (Create("Force"), Mass, Create("Acceleration"), DimensionOp.Product),
  (Create("Pressure"), Create("Force"), Area, DimensionOp.Quotient),
  (Create("Energy"), Create("Force"), Length, DimensionOp.Product),
  (Create("Power"), Create("Energy"), Duration, DimensionOp.Quotient),
  (Create("Density"), Mass, Volume, DimensionOp.Quotient),
  (Create("MassFlow"), Mass, Duration, DimensionOp.Quotient)));

 static readonly Lazy<Seq<(QuantityType Left, QuantityType Right)>> ReciprocalRows = new(static () => Seq(
  (Create("ThermalResistance"), Create("HeatTransferCoefficient")),
  (Create("ElectricResistance"), Create("ElectricConductance"))));

 public static Seq<(QuantityType Compound, QuantityType Left, QuantityType Right, DimensionOp Op)> Relations => RelationRows.Value;
 public static Seq<(QuantityType Left, QuantityType Right)> Reciprocals => ReciprocalRows.Value;

 public static Fin<Unit> Consistency() {
  return Accumulate(
   Relations.Map(row => Registered(row.Compound).Bind(compound =>
     (Registered(row.Left), Registered(row.Right)).Apply((left, right) =>
      compound == row.Op.Compose(left, right)).As()
      .Bind(holds => holds
       ? Success<Error, Unit>(unit)
       : Fail<Error, Unit>(new ElementFault.ValueRejected($"<quantity-relation-inconsistent:{row.Compound.ToValue()}>"))))
   + Reciprocals.Map(pair => (Registered(pair.Left), Registered(pair.Right)).Apply((left, right) =>
      left == Dimension.Dimensionless.Divide(right)).As()
      .Bind(holds => holds
       ? Success<Error, Unit>(unit)
       : Fail<Error, Unit>(new ElementFault.ValueRejected($"<quantity-reciprocal-inconsistent:{pair.Left.ToValue()}>")))))
   ).ToFin();
 }

 static Validation<Error, Dimension> Registered(QuantityType type) =>
  MeasureValue.DimensionOf(type).ToValidation<Error>(
   new ElementFault.ValueRejected($"<quantity-relation-unregistered:{type.ToValue()}>"));

 public static Fin<(MeasureValue Value, MeasureEvidence Evidence)> Admit(
  QuantityType family, double value, Enum unit, UnitResolution resolution, CorrelationId correlation) =>
  MeasureValue.Of(value, unit, Some(family)).Map(admitted =>
   (admitted, new MeasureEvidence(
    family, unit.ToString(), value,
    admitted.CanonicalUnit.IfNone(family.ToValue()), admitted.Si,
    resolution, correlation)));
}
```

## [05]-[MEASURE_STAT]

- Owner: `QuantitySignature` the `[ComplexValueObject]` quantity triple (`Type` + `Dimension` + `Option<string> CanonicalUnit`) as ONE value — coherence (a registry type's dimension matches) proves ONCE at its own admission, so no downstream fold re-checks it; `MeasureStat` the measured-fold composition of the kernel `Stat<Scalar>` (`Rasm/Domain/stats` S7) beside a signature — the kernel carrier constraint (`Amount<TCarrier,double>, DomainType<TCarrier,double>`) is unsatisfiable for `MeasureValue` because `From(double)` cannot recover the quantity triple from a scalar, so the composition IS the lawful form and a local Welford or moments tuple over `MeasureValue` is the deleted form; `MeasureCanon` the dimensioned `CanonicalWriter` extension the kernel writer's own Boundary cedes to this contract.
- Entry: `QuantitySignature.Of(MeasureValue)` derives the triple from an admitted value; `MeasureStat.Of(signature, values, key, weights)` gates every member against the signature (accumulating, naming each foreign member), lifts the SI run through `Scalar.From`, and runs the kernel `Stat<Scalar>.Of` fold — `Count`, `Mass`, `Variance(MomentNormalizer)`, `Skewness`, `Kurtosis`, `Rms` arrive free; `Of(signature, Seq<double> si, key, weights)` is the raw-run leg a decoded sample run takes (each scalar gates through `Scalar.From`), and `Merge(left, right, key)` the pairwise dual over one signature composing the kernel `Stat.Merge` Pebay join; `Minimum`/`Maximum`/`Mean` re-mint `MeasureValue` through the trusted `Reproject` (the triple and the scalar were both proved at admission — re-refusing re-checks a proof).
- Output: a `MeasureStat` is the measured aggregate an observation summary, a Materials measured fold, and a takeoff statistic carry — one kernel moments engine, one signature, no re-derived recurrence.
- Packages: `Rasm` (kernel `Stat<Scalar>`, `Scalar`, `MomentNormalizer`, `CanonicalWriter`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured aggregate is a kernel `Stat` read, never a column here; a new signature axis is one `QuantitySignature` member plus its `CanonicalBytes` write in the same edit.
- Boundary: `MeasureCanon.Measure` writes the IDENTITY axes — the type token, the SI magnitude, the 7-vector through `Dimension.CanonicalBytes`, the presence-prefixed band — and NEVER the display unit (`Torque`/`Energy` stay distinct; `1000 mm` and `1 m` address identically); magnitudes and band bounds grid on the WRITER's own tolerance (the kernel `Double` quantizes — one rounding owner branch-wide), while the coverage factor is declared policy on no physical axis and writes EXACT through `Bits`; a second dimensioned writer, or a hand seven-`Ordinal` spelling of the vector, is the deleted form.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class QuantitySignature {
 public QuantityType Type { get; }
 public Dimension Dimension { get; }
 public Option<string> CanonicalUnit { get; }

 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref QuantityType type, ref Dimension dimension, ref Option<string> canonicalUnit) {
  validationError = MeasureValue.DimensionOf(type).Filter(expected => expected != dimension).IsSome
   ? new ValidationError($"{type.ToValue()} must carry its declared dimension")
   : validationError;
 }

 public static QuantitySignature Of(MeasureValue value) => Create(value.Type, value.Dimension, value.CanonicalUnit);

 public void CanonicalBytes(CanonicalWriter writer) {
  writer.String(Type.ToValue());
  Dimension.CanonicalBytes(writer);
  writer.Optional(CanonicalUnit, static (unit, w) => w.String(unit));
 }
}

public sealed record MeasureStat(QuantitySignature Signature, Stat<Scalar> Figures) {
 public static Fin<MeasureStat> Of(QuantitySignature signature, Seq<MeasureValue> values, Option<Seq<double>> weights = default) =>
  Accumulate(values.Map((member, index) => member.Type == signature.Type && member.Dimension == signature.Dimension
    ? Success<Error, Unit>(unit)
    : Fail<Error, Unit>(new ElementFault.ValueRejected($"<measure-stat-signature-mismatch:index={index}:type={member.Type.ToValue()}>"))))
   .ToFin()
   .Bind(_ => values.Traverse(member => Scalar.From(member.Si)).As())
   .Bind(scalars => Stat<Scalar>.Of(scalars, weights))
   .Map(figures => new MeasureStat(signature, figures));

 public static Fin<MeasureStat> Of(QuantitySignature signature, Seq<double> si, Option<Seq<double>> weights = default) =>
  si.Traverse(sample => Scalar.From(sample)).As()
   .Bind(scalars => Stat<Scalar>.Of(scalars, weights))
   .Map(figures => new MeasureStat(signature, figures));

 public static Fin<MeasureStat> Merge(MeasureStat left, MeasureStat right) =>
  left.Signature == right.Signature
   ? Stat<Scalar>.Merge(left.Figures, right.Figures).Map(figures => new MeasureStat(left.Signature, figures))
   : new ElementFault.ValueRejected($"<measure-stat-merge-signature:{right.Signature.Type.ToValue()}>");

 public MeasureValue Minimum => MeasureValue.Reproject(Signature, Figures.Minimum.To());
 public MeasureValue Maximum => MeasureValue.Reproject(Signature, Figures.Maximum.To());
 public MeasureValue Mean => MeasureValue.Reproject(Signature, Figures.Mean);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MeasureCanon {
 extension(CanonicalWriter writer) {
  public CanonicalWriter Measure(MeasureValue measure) {
   CanonicalWriter signature = writer.String(measure.Type.ToValue()).Double(measure.Si);
   measure.Dimension.CanonicalBytes(signature);
   return signature.Optional(measure.Uncertainty, static (band, w) => {
    w.String(band.Kind.Key).Double(band.LowerSi).Double(band.UpperSi)
     .Optional(band.StandardDeviationSi, static (sd, deep) => deep.Double(sd))
     .Optional(band.CoverageFactor, static (k, deep) => deep.Bits(k));
   });
  }
 }
}
```

## [06]-[IMPLEMENTATION_LAW]

- [UNITSNET_SI_COERCION]: `Of` resolves dynamic unit input through `Quantity.TryFrom`, then normalizes EVERY quantity through the one `SiUnit` election — declared `BaseUnitInfo` by default, an `SiElection` row where that base is prefixed or convention-scaled, refusal by name where that row's `Option` is `None`.
- [SI_WALK_REFUSED]: registry `ToUnit(UnitSystem.SI)` cannot serve as that election, since its `GetUnitInfosFor` walk demands seven-axis `BaseUnits` equality against metadata most `UnitInfo` rows leave `Undefined` (`MassUnit.Kilogram` among them), so it throws `ArgumentException` across the majority of the roster; guarding it with an `IsDimensionless()` fork dies with it, routing `Angle` onto a `Degree` base and persisting degrees where radians are the law.
- [CONVERSION_STORE_REFUSED]: the instance `UnitConverter.TryGetConversionFunction` store holds CUSTOM registrations alone (decompile-proven — `TryConvert` is `Quantity.TryFrom` + the struct's own `As` arithmetic), so binding the "cached delegate" would MISS every built-in conversion; `TryConvert` is the deepest total member for the dynamic path.
- [SI_NAME_MIRROR]: `Registry.SiNames` IS the election, `OfSi` stamps the same name, and `In` reads `UnitConverter.TryConvert` for total egress over the admitted class.
- [QUANTITY_TYPE_DISCRIMINATOR]: `QuantityType` carries `QuantityInfo.Name`, `Dimension` the seven-exponent signature — both required because the vector is non-injective. Named QTO reads derive from `As(QuantityType)`, cross-quantity products compose dimensions through `DimensionOp` rows, and an informed consumer re-stamps semantic type through `WithType`.
- [BAND_PROPAGATION]: Gaussian operands propagate by first-order partials and quadrature; exact operands are the zero-variance identity whose result carries NO band (a minted zero-width band refuses its own re-admission); bounds-only operands fold the one-pass corner `Envelope`; a bounds-only divisor spanning zero widens to `Unbounded`; a non-finite propagated deviation widens the same way while the nominal stays finite-gated.
- [WRITER_GRID]: tolerance quantization is the KERNEL writer's — `CanonicalWriter.Double` grids every magnitude and band bound on the writer's own tolerance at the `MeasureCanon` write, the coverage factor rides `Bits` exact, and no second rounding owner exists on this contract.
- [UNIT_REGIME]: `UnitScheme` is the ONE declared-unit regime — ingress `Coerce` (per-value declared override first, affine on a pure axis, multiplicative composition on a compound), egress `Render` (override, declared-axis inverse, SI), `MeasureEvidence` with its `UnitResolution` policy column — and every peer folder's unit carrier composes it.

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
