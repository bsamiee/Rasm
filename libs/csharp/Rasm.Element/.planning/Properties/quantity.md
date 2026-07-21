# [ELEMENT_QUANTITY]

The typed measured-scalar owner: one `MeasureValue` carrying its SI magnitude, its canonical unit string, a `Dimension` `[ComplexValueObject]` over the seven SI base-dimension exponents (the physical signature), and a `QuantityType` `[ValueObject<string>]` discriminator — a NAME-keyed identity admitted from the `UnitsNet` `QuantityInfo.Name` at the registry boundary OR minted as a consumer's engineering-domain name (`SectionModulus`/`SecondMomentOfArea`/`TorsionConstant`/`WarpingConstant`) through the OPEN `QuantityType.Create` where the registry carries no matching quantity — coerced to its SI base and read back through type-checked accessors. The discriminator is the NAME, not the dimension, for TWO reasons: the registry name disambiguates a non-injective seven-exponent vector (`Torque` and `Energy` both reduce to `[L²·M·T⁻²]`, and `Angle` (rad), `Ratio`, and `SoundPressureLevel` (dB) all reduce to the zero vector shared with a bare `Count`), AND a consumer-minted domain name discriminates a `SectionModulus` from a `Volume` and a `TorsionConstant` from a `SecondMomentOfArea` (both `L⁴`) where `UnitsNet` has no quantity at all — so the discriminator is the `UnitsNet` `QuantityInfo.Name` for a registry quantity and a `Create`-minted domain name for an engineering quantity the registry lacks, never a closed registry-only vocabulary the next consumer cannot extend. This REPLACES the six-value `QuantityKind` enum the migration source carried: a `MeasureValue` is no longer one of `Length`/`Area`/`Volume`/`Weight`/`Count`/`Time` but ANY measured quantity the `UnitsNet` registry names — so `ThermalTransmittance` (the `HeatTransferCoefficient` quantity, W·m⁻²·K⁻¹), `Pressure` (Pa), `Force` (N), `MassDensity` (the `Density` quantity, kg·m⁻³), `SoundPressureLevel` (the `Level` quantity, dB), `PlaneAngle` (the `Angle` quantity, rad), and `Temperature` (K) all admit with their real type, dimension, and unit, and the six former kinds become convenience accessors on the quantity-takeoff fold, never a closed enum the next `Pset_*` measure cannot extend. The owner composes the admitted `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → `ToUnit(UnitSystem.SI)` for a dimensional quantity, the registry base unit `ToUnit(QuantityInfo.BaseUnitInfo.Value)` for a dimensionless one → `QuantityInfo.Name` + `Dimensions`) and the `Properties/property#PROPERTY_VALUE` `PropertyValue.Measure` carrier for the typed property arm; the SI reprojection rides the kernel `Op.Catch` funnel so the `UnitsNet` boundary throw never escapes the rail, and a non-finite or unresolvable measure rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page realizes a `QuantityType`-discriminated, `Dimension`-signed `MeasureValue` over the full `UnitsNet` quantity family — carrying the dimensioned cross-quantity algebra (`Multiply`/`Divide` over the composed `Dimension`, `Scale`, `Sum`, each fold propagating the optional `MeasureBand` kind-dispatched: Gaussian quadrature for `Normal` operands, conservative corner intervals otherwise) so a derived takeoff composes ON the carrier rather than re-deriving the dimensional product on bare doubles — never a six-case enum, never a dimension-only discriminator, and never a parallel checked-accessor family beside the one polymorphic `As(QuantityType)` read.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` `[ComplexValueObject]` seven-SI-exponent physical signature (canonical-dimension static rows, `SiSymbol` display, the `Multiply`/`Divide` derived-dimension algebra) and the `QuantityType` `[ValueObject<string>]` identity discriminator (the QTO-matchable rows, the open `Create` mint, the dimension-anonymous `OfDimension`).
- [02]-[MEASURE_VALUE]: the `MeasureValue` SI-coerced scalar carrier, the `UnitsNet` admission seam (the typed `Of(Enum)`, the abbreviation `Of(string)` external-source decode (the Rasm.Bim IFC/tabular/bSDD ingress convenience, NOT a cross-runtime wire shape), the SI-native `OfSi(QuantityType, Dimension, double)` + dimension-only `OfSi(Dimension, double)` + labeled registry-less `OfSi(QuantityType, Dimension, double, string)` factories, and the `OfCount` tally), the one polymorphic `As(QuantityType)` QTO read (the named `Length`/`Area`/`Volume`/`Weight`/`Time`/`Count` reads deriving from it), the unit-aware `In(Enum)`/`In(string)` display egress (the once-built registry unit index plus the guarded `UnitConverter.TryConvert` reprojection a UI/wire consumer reads a stored measure back in `Millimeter`/`Foot`/`DegreeCelsius` through, the token form the one-body delegate a scheme token resolves by name), the `UnitScheme` model-level unit-presentation scheme (the `IfcUnitAssignment` counterpart the `Graph/element#ELEMENT_GRAPH` `Header` carries — `QuantityType` token → declared display-unit token, `Render` the one policy read over `In`, presentation-only and canonical-bytes-excluded), the dimensioned `Multiply`/`Divide`/`Scale` cross-quantity algebra with kind-dispatched `MeasureBand` propagation, the band-preserving `WithType` semantic re-stamp a derived product re-types through, the `Quantize` content-hash projection, and the same-type `Sum` reducer.

## [02]-[DIMENSION]

- Owner: `Dimension` the `[ComplexValueObject]` carrying the seven SI base-dimension exponents (`Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity`), the physical signature a `MeasureValue` coerces to, the `Multiply`/`Divide` algebra composes, and the `Projection/address#CONTENT_ADDRESS` writer hashes; `QuantityType` the `[ValueObject<string>]` quantity-type identity (the `UnitsNet` `QuantityInfo.Name`) a `MeasureValue` discriminates on. The canonical-dimension static rows (`LengthDim`/`AreaDim`/`VolumeDim`/`MassDim`/`DurationDim`/`Dimensionless` and the derived `PressureDim`/`DensityDim`/`ForceDim`/`IrradianceDim`/`ThermalTransmittanceDim`) name the well-known dimensions; the QTO-matchable `QuantityType` rows (`Length`/`Area`/`Volume`/`Mass`/`Duration`/`Count`) name the well-known quantities a QTO accessor matches.
- Entry: `Dimension.Of(BaseDimensions dims)` lowers the `UnitsNet` 7-vector onto the value-object; `Dimension.Create(length, mass, time, current, temperature, amount, luminousIntensity)` is the generated factory; `Multiply`/`Divide` add/subtract the exponent vectors so a derived dimension composes (`ForceDim.Divide(AreaDim)` IS `PressureDim`) without a per-quantity row. `QuantityType.Create(name)` admits a quantity-type name — the `UnitsNet` `QuantityInfo.Name` at the `Of` boundary, or a consumer's engineering-domain name where no `UnitsNet` quantity exists; the OPEN `Create` sanction is the SAME admission the `Composition/material#MATERIAL_PROPERTY` `SeamSection` chain mints its four engineering-section discriminators through — `SectionModulus` and `SecondMomentOfArea` plus `TorsionConstant` and `WarpingConstant` (the `UnitsNet` registry carries `AreaMomentOfInertia` but none of these four as distinct quantities, so each is a consumer-minted domain name `Create` blesses, two of them — `SecondMomentOfArea` and `TorsionConstant` — sharing the `L⁴` signature the NAME alone separates) — so a consumer-minted section discriminator is a first-class `QuantityType`, never a registry-only name the `Of` boundary alone admits; `QuantityType.OfDimension(dimension)` derives the dimension-anonymous identity an `OfSi(Dimension, _)` value carries.
- Auto: the `[ComplexValueObject]` generates structural equality over the seven exponents so two `MeasureValue`s share a dimension iff their exponent vectors match, the `Of` projection reads the `UnitsNet` `BaseDimensions` integer exponents directly, and `Multiply`/`Divide` are pure exponent arithmetic mirroring the `BaseDimensions.Multiply`/`Divide` algebra so the seam never re-derives the dimensional product; the `[ValueObject<string>]` gives `QuantityType` ordinal value-equality so a `MeasureValue` discriminates on the registry name, and `OfDimension` encodes the 7-vector into a tilde-prefixed token that is dimension-unique yet NEVER equal to a QTO row, so an SI-native value admitted by dimension alone stays dimensioned-but-untyped and cannot false-match a QTO accessor.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`/`[ValueObject<string>]`), UnitsNet (`BaseDimensions` the 7-vector source, `QuantityInfo.Name` the quantity-type identity).
- Growth: a new well-known dimension is one static `Dimension` row (a `LuminousFlux`/`Frequency` row composed from exponents, or the `L⁴`/`L⁶` signatures the `SeamSection` chain reaches through `Dimension.Create(int×7)` with no static row); a new QTO-matchable quantity is one `QuantityType` row; a consumer's engineering-domain quantity (the `SectionModulus`/`SecondMomentOfArea`/`TorsionConstant`/`WarpingConstant` the `Composition/material#MATERIAL_PROPERTY` `SeamSection` mints, an analysis result scalar) mints through `QuantityType.Create(name)` with no seam row; a new measured quantity the registry names needs NO new dimension AND no new type — its `UnitsNet` `QuantityInfo` resolves both at admission — and a new engineering quantity the registry lacks needs only the `Create`-minted name; never a per-quantity dimension type and never a closed `QuantityKind` enum.
- Boundary: `QuantityType` is the ONE quantity-type discriminator and `Dimension` the physical signature — the six-case `QuantityKind` enum (the migration form) is the deleted form because it forced an out-of-family measure to a dimensionless `Count`, and the dimension-AS-discriminator form is the rejected form because distinct quantities share a dimension (`Torque`/`Energy` on `[L²·M·T⁻²]`; `Angle`/`Level`/`Ratio`/`Count` on the zero vector; `SecondMomentOfArea`/`TorsionConstant` on `[L⁴]`); the identity is the `UnitsNet` `QuantityInfo.Name` for a registry quantity so any of the ~120 quantity structs admits with its real name, AND a consumer's engineering-domain name through the OPEN `Create` for an engineering quantity the registry lacks (the four `SectionModulus`/`SecondMomentOfArea`/`TorsionConstant`/`WarpingConstant` the `SeamSection` chain mints, the registry's `AreaMomentOfInertia` not being any of them) — `Create` is the sanctioned open mint, NOT a registry-only gate that phantoms the `SeamSection` discriminators — the named QTO rows the convenience anchors a QTO accessor matches, and the `Multiply`/`Divide` algebra composes a derived dimension from base ones rather than enumerating every product; the exponents come from the `UnitsNet` `BaseDimensions` (or the `Dimension.Create(int×7)` generated factory for the `L⁴`/`L⁶` signatures the static rows omit) and a hand-coded dimension table that drifts from the unit registry — or a name string parsed at a call site rather than minted through `QuantityType.Create` — is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Element.Properties;

// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class Dimension {
 public int Length { get; }
 public int Mass { get; }
 public int Time { get; }
 public int Current { get; }
 public int Temperature { get; }
 public int Amount { get; }
 public int LuminousIntensity { get; }

 public static readonly Dimension Dimensionless = Create(0, 0, 0, 0, 0, 0, 0);
 public static readonly Dimension LengthDim = Create(1, 0, 0, 0, 0, 0, 0);
 public static readonly Dimension AreaDim = Create(2, 0, 0, 0, 0, 0, 0);
 public static readonly Dimension VolumeDim = Create(3, 0, 0, 0, 0, 0, 0);
 public static readonly Dimension MassDim = Create(0, 1, 0, 0, 0, 0, 0);
 public static readonly Dimension DurationDim = Create(0, 0, 1, 0, 0, 0, 0);
 public static readonly Dimension ForceDim = Create(1, 1, -2, 0, 0, 0, 0);
 public static readonly Dimension PressureDim = Create(-1, 1, -2, 0, 0, 0, 0);
 public static readonly Dimension DensityDim = Create(-3, 1, 0, 0, 0, 0, 0);
 public static readonly Dimension IrradianceDim = Create(0, 1, -3, 0, 0, 0, 0);
 public static readonly Dimension ThermalTransmittanceDim = Create(0, 1, -3, 0, -1, 0, 0);

 public static Dimension Of(BaseDimensions d) =>
 Create(d.Length, d.Mass, d.Time, d.Current, d.Temperature, d.Amount, d.LuminousIntensity);

 public Dimension Multiply(Dimension other) =>
 Create(Length + other.Length, Mass + other.Mass, Time + other.Time, Current + other.Current,
 Temperature + other.Temperature, Amount + other.Amount, LuminousIntensity + other.LuminousIntensity);

 public Dimension Divide(Dimension other) =>
 Create(Length - other.Length, Mass - other.Mass, Time - other.Time, Current - other.Current,
 Temperature - other.Temperature, Amount - other.Amount, LuminousIntensity - other.LuminousIntensity);

 // SI coherent-unit symbol for the well-known rows — the display DEFAULT for a measure the registry cannot name
 // (consumer mints, OfDimension identities, Multiply/Divide products); a registry-named OfSi stamps the registry unit
 // NAME instead. Display-only, never hashed (CanonicalWriter.Measure drops the unit); an unrostered dimension reads "SI".
 static readonly FrozenDictionary<Dimension, string> SiSymbols = new Dictionary<Dimension, string> {
  [Dimensionless] = "", [LengthDim] = "m", [AreaDim] = "m2", [VolumeDim] = "m3", [MassDim] = "kg",
  [DurationDim] = "s", [ForceDim] = "N", [PressureDim] = "Pa", [DensityDim] = "kg/m3",
  [IrradianceDim] = "W/m2", [ThermalTransmittanceDim] = "W/(m2.K)",
 }.ToFrozenDictionary();

 public string SiSymbol => SiSymbols.TryGetValue(this, out string? symbol) ? symbol : "SI";
}

[ValueObject<string>]
public sealed partial class QuantityType {
 // A blank name would alias distinct measures under the content hash (the CanonicalWriter writes Type.Value as the
 // discriminator token), so admission rejects it — every registry name, QTO row, OfDimension mint, and QuantityRow
 // token is non-blank by construction.
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  value = value.Trim();
  validationError = value.Length == 0 ? new ValidationError("<quantity-type-blank>") : validationError;
 }

 // The QTO-matchable rows — each .Value matches the UnitsNet QuantityInfo.Name (an Of-admitted value carries the
 // registry's own name) plus two seam sentinels with no registry quantity. Seam-ROSTERED rows ONLY: the OPEN Create
 // additionally mints consumer engineering names — SectionModulus/SecondMomentOfArea/TorsionConstant/WarpingConstant,
 // two sharing the L⁴ signature the NAME alone separates — so a section discriminator needs NO static row here.
 public static readonly QuantityType Length = Create("Length");
 public static readonly QuantityType Area = Create("Area");
 public static readonly QuantityType Volume = Create("Volume");
 public static readonly QuantityType Mass = Create("Mass");
 public static readonly QuantityType Duration = Create("Duration");
 public static readonly QuantityType Count = Create("Count"); // IfcQuantityCount — a tally, no UnitsNet quantity
 public static readonly QuantityType Scalar = Create("Scalar"); // a dimensionless untyped scalar / additive identity

 // The dimension-anonymous identity an OfSi(Dimension, _) value carries: dimension-unique (so a same-dimension SI-native
 // Sum stays valid) yet NEVER a QTO row (so a section modulus admitted by VolumeDim never reads as a Volume).
 public static QuantityType OfDimension(Dimension d) =>
 d == Dimension.Dimensionless
 ? Scalar
 : Create($"~{d.Length}.{d.Mass}.{d.Time}.{d.Current}.{d.Temperature}.{d.Amount}.{d.LuminousIntensity}");
}
```

## [03]-[MEASURE_VALUE]

- Owner: `MeasureValue` the SI-coerced measured-scalar carrier (`QuantityType Type` + `Dimension Dimension` + `double Si` + `string CanonicalUnit` + `Option<MeasureBand> Uncertainty`) the `Properties/property#PROPERTY_VALUE` `Measure` arm, the `Composition/material#MATERIAL_PROPERTY` measured columns, and the `Assessment/assessment#ASSESSMENT_NODE` result scalars read; `MeasureBand` the neutral uncertainty interval/band carrier plus optional distribution metadata; `UncertaintyKind` the closed uncertainty-model vocabulary whose `Gaussian` column dispatches the propagation algebra, the token a package-specific uncertainty library lowers onto; `UnitScheme` the model-level unit-presentation scheme the `Graph/element#ELEMENT_GRAPH` `Header` carries — the `IfcUnitAssignment` counterpart mapping a `QuantityType` token to its declared display-unit token, `Render` composing the `In` egress, presentation-only and excluded from every canonical byte.
- Entry: `MeasureValue.Of` and `OfSi` admit finite magnitudes; `MeasureBand.Admit` validates ordered non-NaN bounds and kind-specific distribution metadata; `WithUncertainty(band, key)` additionally requires the band to contain the nominal magnitude. `WithType` re-stamps semantic identity without discarding the admitted band. Every construction and algebra exit preserves the finite-magnitude invariant or returns `ElementFault.ValueRejected`.
- Auto: `Of` routes the raw value through `Quantity.TryFrom(value, unit)` to a boxed `IQuantity`, then `Coerce` reprojects a DIMENSIONAL quantity to SI base through `ToUnit(UnitSystem.SI)` and normalizes a DIMENSIONLESS quantity (`Angle`/`Ratio`/`Level` — no `UnitSystem.SI` reprojection exists) to its registry base unit through `ToUnit(QuantityInfo.BaseUnitInfo.Value)` (rad for `Angle`, dB for `Level`, `DecimalFraction` for `Ratio`), so `Admit` reads `(double)Value` plus `QuantityInfo.Name` plus `Dimension.Of(Dimensions)` and the persisted scalar is base-normalized whatever the admission spelling, never a hand-mapped kind; uncertainty rides the optional `MeasureBand`, not a second deterministic value, and propagates through every algebra fold kind-dispatched on the `UncertaintyKind.Gaussian` column.
- Receipt: a `MeasureValue` is the unit-checked evidence a takeoff, a property facet, and a cost join read — `measure.Si` is the SI magnitude, `measure.CanonicalUnit` the SI unit string, `measure.Type` the quantity-type identity, `measure.Dimension` the physical signature, and `measure.Uncertainty` the optional neutral bounds plus distribution metadata; the source package that computed uncertainty stays above the seam.
- Packages: UnitsNet (`Quantity.TryFrom(double, Enum, out)` + `Quantity.TryFromUnitAbbreviation(culture, value, abbr, out)` the dynamic ingress, `Quantity.Infos`/`QuantityInfo.GetUnitInfosFor(BaseUnits)`/`UnitInfo.Name`/`UnitInfo.Value` the registry metadata the once-built unit index projects, `UnitConverter.TryConvert(QuantityValue, Enum, Enum, out double)` the guarded egress conversion `In` reads, `IQuantity.ToUnit(UnitSystem.SI)`/`.Dimensions`/`.QuantityInfo.Name`/`.Value`/`.Unit` the coercion + identity surface, `BaseDimensions` the 7-vector + `IsDimensionless()`, the cross-quantity-operator algebra `MeasureValue.Multiply`/`Divide` mirrors onto the one carrier), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Option`/`Seq` + `Bind`/`MapFail`/`Choose`), `Rasm` (the kernel `Op` op-key + `Op.Catch` exception funnel).
- Growth: a new measured quantity needs NO seam edit — its `UnitsNet` `QuantityInfo` resolves type and dimension at `Of` admission; a new display unit a consumer renders in needs NO seam edit — it is a `UnitsNet` unit-enum member `In(unit)` already reprojects to; a new derived takeoff composes through the existing `Multiply`/`Divide`/`Scale`/`Sum` algebra (a `Force = Pressure × Area` is one `pressure.Multiply(area)`, never a new operation); a new named convenience read is one derived line over `As(QuantityType)`; never a per-quantity dimension type, a closed `QuantityKind` enum, or a parallel cross-quantity operation family.
- Boundary: `MeasureValue` NEVER carries a bare `double` quantity — the value coerces to SI base once at `Of` through the `UnitsNet` registry and the interior carries the SI scalar plus the typed `QuantityType`/`Dimension`, a free `double` measure field being the named defect per the `UnitsNet` ownership of dimensioned scalars; the discriminator is the `QuantityType` (the `QuantityInfo.Name`) so a `Torque` and an `Energy` that share a `Dimension` stay distinct and a dimensionless `Angle`/`Level`/`Ratio` never collapses onto `Count` — the dimension-as-discriminator form is the deleted form, and the `QuantityKind` six-case enum that degraded an out-of-family measure to `Count` is doubly deleted; the SI coercion rides the `UnitsNet` `Quantity.TryFrom`/`ToUnit(UnitSystem.SI)` registry and a stringly-keyed unit switch or an ad-hoc conversion factor is the deleted form; `ToUnit(UnitSystem.SI)` is the platform-forced boundary throw (`InvalidCastException`/`NotImplementedException` when a quantity has no SI unit), so the reprojection rides the kernel `Op.Catch` funnel and a residual throw rails `ElementFault.ValueRejected` — a foreign `Exception` never crosses the seam signature; a DIMENSIONLESS quantity has no `UnitSystem.SI` reprojection and normalizes instead to its registry base unit through `ToUnit(QuantityInfo.BaseUnitInfo.Value)` — dB stays dB, rad stays rad, a degree-admitted `PlaneAngle` stores radians — so `SoundPressureLevel`/`PlaneAngle` admit rather than throw AND two same-magnitude admissions spelled in different units cannot fork the content key (the as-constructed keep was the deleted form that hashed a 30° and a 0.5236 rad angle apart under `CanonicalWriter.Measure`); aggregation flows through `MeasureValue.Sum` (an SI-base scalar sum guarded by `Type` equality, stricter than a dimension guard) and a derived quantity through `MeasureValue.Multiply`/`Divide` (the SI product/quotient over the composed `Dimension`, the result dimension-anonymous so the seam never false-names a `Volume × Density` as `Mass` — the consumer that KNOWS the identity re-stamps through the band-preserving `WithType(QuantityType.Mass)`, the registry canonical unit re-resolved through the ONE `CanonicalUnitFor` probe `OfSi` stamps and the propagated `MeasureBand` surviving the `with`, the bare re-mint that silently discarded the band being the deleted form), never an unwrap-compute-rewrap or a hand-reconstructed result dimension, and a cross-type sum rails `ElementFault.ValueRejected`; the FINITE invariant holds at EVERY construction path and algebra exit — `Of`/`OfCount` gate the raw ingress, `OfSi` gates the SI-native mint (the UnitsNet-ingress-only check that let a raw `OfSi` double or a hostile wire scalar bypass the invariant is the deleted form), and `Multiply`/`Divide`/`Scale`/`Sum` rail an overflowed product, a zero-divisor quotient, or a non-finite factor — so a NaN/∞ magnitude is unrepresentable in an admitted `MeasureValue` and can never enter the canonical bytes as domain evidence (an infinite `MeasureBand` BOUND stays honest uncertainty — the band axis is looser than the magnitude by design); the optional `MeasureBand` propagates through EVERY fold (`Multiply`/`Divide`/`Sum`) kind-dispatched on the `UncertaintyKind.Gaussian` column — Gaussian operands combine σ in first-order quadrature over the op's partial derivatives and re-expand `si ± k·σ` under the widest declared coverage factor, a bounds-only operand forces the conservative 4-corner interval (exact for monotone ops — a `Divide` whose bounds-only divisor band spans zero widens to the honest ±∞ interval, because corner extrema are exact only where the divisor excludes zero), and the head-band re-stamp that dressed a summed magnitude in an unsummed band (or the interval collapse that silently discarded a `Normal` operand's `StandardDeviationSi`/`CoverageFactor`) is the deleted form; the type-and-dimension-preserving `Scale(factor)` is the basis-aware multiple a cost/environmental ply scaling composes, never a `this with { Si = Si * factor }` mutation at the call site; the dimension-only `OfSi(Dimension, double)` carries the dimension-anonymous `QuantityType` so a bare SI-native scalar is dimensioned-but-untyped and a QTO read fires ONLY for a measure admitted with an explicit quantity-type; the named QTO reads (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) DERIVE from the one `As(QuantityType)` body (DERIVED_LOGIC — a six-member parallel checked family is the deleted form, the convenience names one-hop over the polymorphic read); the interior carries SI ALONE while EGRESS is unit-parameterized — `In(Enum unit)` resolves the stored `(Type.Value, CanonicalUnit)` through the once-built registry unit index (the `Lazy`/`FrozenDictionary` projection over `Quantity.Infos`; `UnitInfo.Name` IS `Unit.ToString()`, so the index keys exactly the token `Of` stamps) and converts through the guarded `UnitConverter.TryConvert(Si, stored, unit, out …)` so a UI/wire consumer renders a stored U-value in `WattPerSquareMeterKelvin`, a length in `Millimeter`/`Foot`, or a temperature in `DegreeCelsius` WITHOUT the call site re-deriving a conversion factor (the deleted `si.Si * 1000.0` form) and WITHOUT a throw across the read (`TryConvert` refuses a wrong-family unit as `None` — never an `As(Enum)` `UnitNotFoundException` escaping an `Option` read); `OfSi(QuantityType, Dimension, _)` stamps the registry canonical unit NAME for a registry-named type — resolved through the SAME `GetUnitInfosFor(UnitSystem.SI.BaseUnits).FirstOrDefault()` walk `ToUnit(UnitSystem.SI)` runs on the `Of` path for a dimensional quantity, and through `QuantityInfo.BaseUnitInfo.Name` for a dimensionless one exactly as `Coerce` normalizes it, so the two admission paths cannot drift — making the egress total over the ~120 registry quantities INCLUDING every `OfSi`-minted computed measure (the prior `SiSymbol` abbreviation stamp the registry can never resolve as a unit name is the deleted form that silently voided `In` for the whole `OfSi`/`QuantityRow`/Compute measure class), and honestly `None` for a consumer-minted `QuantityType` (a `SectionModulus`, an analysis-result scalar) or a dimension-anonymous `OfSi(Dimension,_)` measure the registry cannot resolve; a registry-less domain-basis scalar whose CONVENTIONAL label matters to a wire consumer (kgCO2e, dB, W/K) admits through the labeled `OfSi(QuantityType, Dimension, double, string)` — the label rides `CanonicalUnit` display-only under the same finite gate, and the overload REFUSES a registry-named type so per-call-site labels can never fork a registry quantity's canonical unit; `Quantize` is the ONLY rounding, applied at the content-hash boundary against `Header.Tolerance`, so the interior `Si` stays full-precision and the canonical key is tolerance-stable.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UncertaintyKind {
 public static readonly UncertaintyKind Exact = new("exact", gaussian: true);
 public static readonly UncertaintyKind Absolute = new("absolute", gaussian: false);
 public static readonly UncertaintyKind Relative = new("relative", gaussian: false);
 public static readonly UncertaintyKind Interval = new("interval", gaussian: false);
 public static readonly UncertaintyKind Normal = new("normal", gaussian: true);

 // The propagation-dispatch column (POLICY_VALUES): whether the kind carries first-order Gaussian evidence — Normal
 // a real σ, Exact the σ=0 identity — so two Gaussian operands combine in quadrature while any bounds-only kind
 // (Interval/Absolute/Relative) forces the conservative corner interval.
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

 public static readonly MeasureBand Exact = new(UncertaintyKind.Exact, 0.0, 0.0, Option<double>.None, Option<double>.None);

 public static Fin<MeasureBand> Admit(
  UncertaintyKind kind, double lowerSi, double upperSi,
  Option<double> standardDeviationSi, Option<double> coverageFactor, Op key) =>
  double.IsNaN(lowerSi) || double.IsNaN(upperSi) || lowerSi > upperSi
   ? ElementFault.ValueRejected(key, "<measure-band-bounds-invalid>")
   : kind == UncertaintyKind.Normal
    ? (standardDeviationSi, coverageFactor).Apply((sd, coverage) =>
       double.IsFinite(lowerSi) && double.IsFinite(upperSi) && double.IsFinite(sd) && sd > 0.0 && double.IsFinite(coverage) && coverage > 0.0
        ? Fin.Succ(new MeasureBand(kind, lowerSi, upperSi, Some(sd), Some(coverage)))
        : ElementFault.ValueRejected(key, "<measure-band-normal-invalid>"))
      .IfNone(ElementFault.ValueRejected(key, "<measure-band-normal-metadata-absent>"))
    : standardDeviationSi.IsSome || coverageFactor.IsSome
     ? ElementFault.ValueRejected(key, "<measure-band-metadata-kind-mismatch>")
     : Fin.Succ(new MeasureBand(kind, lowerSi, upperSi, None, None));

 internal static MeasureBand Interval(UncertaintyKind kind, double lowerSi, double upperSi) =>
  double.IsNaN(lowerSi) || double.IsNaN(upperSi)
   ? new(kind, double.NegativeInfinity, double.PositiveInfinity, Option<double>.None, Option<double>.None)
   : new(kind, Math.Min(lowerSi, upperSi), Math.Max(lowerSi, upperSi), Option<double>.None, Option<double>.None);

 internal static MeasureBand Normal(double lowerSi, double upperSi, double standardDeviationSi, double coverageFactor) =>
  new(UncertaintyKind.Normal, lowerSi, upperSi, Some(standardDeviationSi), Some(coverageFactor));
}

public sealed record MeasureValue {
 private MeasureValue(QuantityType type, Dimension dimension, double si, string canonicalUnit, Option<MeasureBand> uncertainty) =>
  (Type, Dimension, Si, CanonicalUnit, Uncertainty) = (type, dimension, si, canonicalUnit, uncertainty);

 public QuantityType Type { get; }
 public Dimension Dimension { get; }
 public double Si { get; }
 public string CanonicalUnit { get; }
 public Option<MeasureBand> Uncertainty { get; }

 // The empty-Sum result — Scalar-typed (a dimensionless untyped scalar), distinct from an explicit Count; it seeds
 // only the empty fold, because Sum's Type guard faults a Scalar head against any non-Scalar tail.
 public static readonly MeasureValue Zero = new(QuantityType.Scalar, Dimension.Dimensionless, 0.0, "", Option<MeasureBand>.None);

 public Fin<MeasureValue> WithUncertainty(MeasureBand band, Op key) =>
  band.LowerSi <= Si && band.UpperSi >= Si
   ? Fin.Succ(new MeasureValue(Type, Dimension, Si, CanonicalUnit, Some(band)))
   : ElementFault.ValueRejected(key, "<measure-band-excludes-nominal>");

 // The band-preserving semantic re-stamp a dimension-anonymous Multiply/Divide product re-types through when the
 // consumer KNOWS the identity (NetWeight IS volume×density re-typed Mass): TOTAL over an already-admitted value
 // (its finiteness is settled by the construction invariant, so no re-gate), the registry canonical unit re-resolved
 // through the SAME CanonicalUnitFor probe OfSi stamps (In round-trips the re-typed product) AND the propagated
 // Uncertainty surviving — a bare `with { Type = type }` that strands the stale unit, or a re-mint through the OfSi
 // gate that re-rails a settled value, is the deleted form.
 public Fin<MeasureValue> WithType(QuantityType type) =>
  Registry.Value.Dimensions.TryGetValue(type.Value, out Dimension? expected) && expected != Dimension
   ? ElementFault.ValueRejected(Op.Of(name: nameof(WithType)), $"<measure-type-dimension-mismatch:{type.Value}>")
   : Fin.Succ(new MeasureValue(type, Dimension, Si, CanonicalUnitFor(type, Dimension), Uncertainty));

 public static Fin<MeasureValue> Of(double value, Enum unit, Op key) =>
 !double.IsFinite(value)
 ? ElementFault.ValueRejected(key, $"<measure-non-finite:{value:R}>")
 : Quantity.TryFrom(value, unit, out IQuantity? quantity) && quantity is { } q
 ? Coerce(q, key)
 : ElementFault.ValueRejected(key, $"<measure-unit-unresolved:{unit}>");

 // Abbreviation decode ({value, unit:"mm"/"kN"}): the Rasm.Bim ingress for an external IFC/tabular/bSDD unit STRING —
 // NOT a cross-runtime wire shape (peers decode C#-minted SI payloads and re-mint nothing). The culture is PINNED
 // invariant — the satellite fr-CA/ru-RU/zh-CN/nb-NO abbreviation tables would fork an ambient-culture decode.
 public static Fin<MeasureValue> Of(double value, string unit, Op key) =>
 !double.IsFinite(value)
 ? ElementFault.ValueRejected(key, $"<measure-non-finite:{value:R}>")
 : Quantity.TryFromUnitAbbreviation(System.Globalization.CultureInfo.InvariantCulture, value, unit, out IQuantity? quantity) && quantity is { } q
 ? Coerce(q, key)
 : ElementFault.ValueRejected(key, $"<measure-unit-abbreviation-unresolved:{unit}>");

 // The ONE registry projection, built once over the process-fixed Quantity.Infos roster (DERIVED_LOGIC): the
 // (quantity, unit) NAME -> unit Enum index the In egress resolves through, and the quantity name -> canonical unit
 // NAME map the OfSi mint stamps — each row the exact mirror of the Coerce leg (the GetUnitInfosFor(SI) walk
 // ToUnit(UnitSystem.SI) runs for a dimensional quantity, BaseUnitInfo for a dimensionless one; UnitInfo.Name IS
 // Unit.ToString()), so Of and OfSi canonical units CANNOT drift.
 static readonly Lazy<(
  FrozenDictionary<(string Quantity, string Unit), Enum> Units,
  FrozenDictionary<string, string> SiNames,
  FrozenDictionary<string, Dimension> Dimensions)> Registry = new(static () => (
  Quantity.Infos.SelectMany(static info => info.UnitInfos.Select(unit => KeyValuePair.Create((info.Name, unit.Name), unit.Value))).ToFrozenDictionary(),
  Quantity.Infos.AsIterable()
   .Choose(static info => info.BaseDimensions.IsDimensionless()
    ? Some((info.Name, Si: info.BaseUnitInfo.Name))
    : Optional(info.GetUnitInfosFor(UnitSystem.SI.BaseUnits).FirstOrDefault()).Map(si => (info.Name, Si: si.Name)))
   .ToFrozenDictionary(static row => row.Name, static row => row.Si),
  Quantity.Infos.ToFrozenDictionary(static info => info.Name, static info => Dimension.Of(info.BaseDimensions))));

 // The SI-native ADMISSION — the entry a Rasm.Compute result, a SectionProperties bake, and the wire decode stamp
 // computed measures through, FINITE-GATED like every other construction path: a NaN/∞ scalar rails ValueRejected
 // under the keyless interior Op (the Classification ValidateFactoryArguments precedent — a keyed caller re-stamps
 // via MapFail), so a non-finite magnitude is unrepresentable in an admitted MeasureValue and the canonical bytes
 // hash admitted evidence only — the UnitsNet-ingress-only finite check that let a raw OfSi double bypass the
 // invariant is the deleted form. A registry-named type stamps the registry canonical unit NAME so In round-trips
 // it; a consumer-minted type keeps the SiSymbol display default and In honestly answers None — the index probe
 // falls back, never faults; identity is Type + Dimension, the unit display-only.
 public static Fin<MeasureValue> OfSi(QuantityType type, Dimension dimension, double si) =>
  !double.IsFinite(si)
   ? ElementFault.ValueRejected(Op.Of(name: nameof(OfSi)), $"<measure-si-non-finite:{si:R}>")
   : Registry.Value.Dimensions.TryGetValue(type.Value, out Dimension? expected) && expected != dimension
    ? ElementFault.ValueRejected(Op.Of(name: nameof(OfSi)), $"<measure-type-dimension-mismatch:{type.Value}>")
    : Fin.Succ(new MeasureValue(type, dimension, si, CanonicalUnitFor(type, dimension), Option<MeasureBand>.None));

 // The dimension-only convenience: a bare SI magnitude whose physical Dimension is known but whose semantic type is not —
 // it carries the dimension-anonymous QuantityType (never a QTO row), so it is dimensioned-but-untyped and reads through Si.
 public static Fin<MeasureValue> OfSi(Dimension dimension, double si) => OfSi(QuantityType.OfDimension(dimension), dimension, si);

 // The LABELED registry-less mint: a domain-basis scalar (kgCO2e, dB, W/K, 1/m) whose conventional display label the
 // registry cannot supply rides CanonicalUnit verbatim — same finite gate, display-only label, identity stays
 // Type + Dimension. A registry-named type REFUSES this path (its canonical unit is the one probe's, so per-call-site
 // labels can never fork a registry quantity), keeping the two admission paths drift-free.
 public static Fin<MeasureValue> OfSi(QuantityType type, Dimension dimension, double si, string unit) =>
  !double.IsFinite(si)
   ? ElementFault.ValueRejected(Op.Of(name: nameof(OfSi)), $"<measure-si-non-finite:{si:R}>")
   : Registry.Value.Dimensions.ContainsKey(type.Value)
    ? ElementFault.ValueRejected(Op.Of(name: nameof(OfSi)), $"<measure-labeled-mint-registry-type:{type.Value}>")
    : Fin.Succ(new MeasureValue(type, dimension, si, unit, Option<MeasureBand>.None));

 // The ONE canonical-unit resolution OfSi stamps and WithType re-stamps — a registry-named type reads the once-built
 // SiNames mirror, a consumer mint falls back to the composed dimension's SI symbol; total, never a fault source.
 static string CanonicalUnitFor(QuantityType type, Dimension dimension) =>
  Registry.Value.SiNames.TryGetValue(type.Value, out string? name) ? name : dimension.SiSymbol;

 public static Fin<MeasureValue> OfCount(double value, Op key) =>
 double.IsFinite(value)
 ? Fin.Succ(new MeasureValue(QuantityType.Count, Dimension.Dimensionless, value, "", Option<MeasureBand>.None))
 : ElementFault.ValueRejected(key, $"<count-non-finite:{value:R}>");

 // The UnitsNet admission seam: a dimensional quantity reprojects to SI base once; a dimensionless one (Angle/Ratio/
 // Level — ToUnit(UnitSystem.SI) throws on the zero vector) normalizes to its registry BASE unit instead, so a
 // degree-admitted and a radian-admitted angle store ONE magnitude and cannot fork the content key (the as-constructed
 // keep was the deleted form). Both legs ride the kernel Op.Catch funnel — a residual throw rails ValueRejected.
 static Fin<MeasureValue> Coerce(IQuantity quantity, Op key) =>
 key.Catch(() => Fin.Succ(quantity.Dimensions.IsDimensionless()
  ? quantity.ToUnit(quantity.QuantityInfo.BaseUnitInfo.Value)
  : quantity.ToUnit(UnitSystem.SI)))
 .MapFail(_ => ElementFault.ValueRejected(key, $"<measure-si-unavailable:{quantity.QuantityInfo.Name}>"))
 .Bind(si => Admit(si, key));

 // The identity is the UnitsNet QuantityInfo.Name (Torque vs Energy, Angle vs Level vs Ratio — distinct names that SHARE
 // a Dimension), the Dimension the physical 7-vector, the Si the SI-base magnitude, the unit the SI unit token.
 static Fin<MeasureValue> Admit(IQuantity si, Op key) =>
 double.IsFinite((double)si.Value)
 ? Fin.Succ(new MeasureValue(QuantityType.Create(si.QuantityInfo.Name), Dimension.Of(si.Dimensions), (double)si.Value, si.Unit.ToString(), Option<MeasureBand>.None))
 : ElementFault.ValueRejected(key, $"<measure-si-non-finite:{si.Unit}>");

 // The ONE polymorphic QTO read: Si WHEN the stored Type matches, else None — a takeoff reads As(QuantityType.Area),
 // a rad Angle or dB Level never reads as a Count, an out-of-family measure reads Si/Dimension directly. The one body
 // the named-accessor family derives from (DERIVED_LOGIC); As mirrors IQuantity.As — the read verb beside the Of admission.
 public Option<double> As(QuantityType type) => Type == type ? Some(Si) : None;

 // The geometric/temporal/tally reads the cost and quantity consumers compose — each DERIVES from the one As body
 // (the convenience-name surface the migration's QuantityKind switch carried, now one-hop reads over the polymorphic
 // owner, never a parallel checked body). Weight names the Mass row (the IfcQuantityWeight takeoff convention).
 public Option<double> Length => As(QuantityType.Length);
 public Option<double> Area => As(QuantityType.Area);
 public Option<double> Volume => As(QuantityType.Volume);
 public Option<double> Weight => As(QuantityType.Mass);
 public Option<double> Time => As(QuantityType.Duration);
 public Option<double> Count => As(QuantityType.Count);

 // Unit-aware DISPLAY egress: the stored (Type.Value, CanonicalUnit) resolves its unit Enum through the once-built
 // Registry index, then the guarded static UnitConverter.TryConvert reprojects — TOTAL: a wrong-family unit, a
 // consumer-minted QuantityType, and a dimension-anonymous OfSi(Dimension,_) all answer None, never an As(Enum) throw
 // across an Option read. Names are enum member names — no ambient culture forks. Ingress Of / egress In are the two
 // unit-boundary verbs; the interior is SI alone (the deleted call-site `Si * 1000.0` form).
 public Option<double> In(Enum unit) =>
  Registry.Value.Units.TryGetValue((Type.Value, CanonicalUnit), out Enum? stored)
   && UnitConverter.TryConvert(Si, stored, unit, out double converted)
   ? Some(converted)
   : None;

 // The token-keyed egress the Header UnitScheme composes — the SAME one In body, the target unit resolved by its
 // registry enum-member NAME through the once-built index (UnitInfo.Name IS Unit.ToString(), so the scheme token and
 // the enum member never drift); a wrong-family or unknown token answers None, never a second conversion body.
 public Option<double> In(string unitName) =>
  Registry.Value.Units.TryGetValue((Type.Value, unitName), out Enum? unit) ? In(unit) : None;

 // Tolerance quantization for the content-hash boundary: round the SI magnitude to the tolerance grid so two measures
 // within Header.Tolerance project to the same canonical bytes; + 0.0 normalizes a -0.0 result to +0.0 (one grid zero).
 public MeasureValue Quantize(double tolerance) =>
 tolerance > 0.0 && double.IsFinite(Si / tolerance)
  ? new MeasureValue(Type, Dimension, Math.Round(Si / tolerance, MidpointRounding.AwayFromZero) * tolerance + 0.0, CanonicalUnit, Uncertainty)
  : this;

 // Same-quantity-type SI reduction — the Type guard is stricter than dimension (Torque/Energy share a Dimension,
 // Angle/Count the zero vector; neither sums). The band folds pairwise through the additive propagation (bounds add
 // exactly; Normal σ in quadrature — both associative), never the head's band re-stamped onto a new magnitude; the
 // fold exit re-asserts the finite invariant so an overflowed reduction rails rather than hashing an ∞ magnitude.
 // LanguageExt v5 `Seq.Head` is `Option<A>`, so the head reads through `Match`.
 public static Fin<MeasureValue> Sum(Seq<MeasureValue> measures, Op key) =>
 measures.Head.Match(
 None: () => Fin.Succ(Zero),
 Some: head => measures.Tail.Exists(m => m.Type != head.Type)
 ? ElementFault.ValueRejected(key, "<measure-sum-type-mismatch>")
 : measures.Tail.Fold(head, static (acc, next) => acc.Add(next)) is { } sum && double.IsFinite(sum.Si)
 ? Fin.Succ(sum)
 : ElementFault.ValueRejected(key, "<measure-sum-non-finite>"));

 // The Sum fold's step — same-type addition with the band propagated through the additive partials (∂/∂l = ∂/∂r = 1);
 // private so Sum's Type guard stays the ONE cross-type gate.
 MeasureValue Add(MeasureValue other) {
  double si = Si + other.Si;
  return new MeasureValue(Type, Dimension, si, CanonicalUnit, CombineBand(this, other, si, static (l, r) => l + r, static (_, _) => (1.0, 1.0)));
 }

 // The dimensioned cross-quantity algebra (UnitsNet's Mass×Acceleration→Force operators collapsed onto the one
 // carrier): the SI product/quotient over the COMPOSED Dimension, so a takeoff derives ON the seam — NetWeight IS
 // volume.Multiply(density), VolumeDim×DensityDim composing MassDim — never a bare-double re-derivation. The result
 // carries the dimension-anonymous QuantityType.OfDimension (the 7-vector is non-injective; a false name would
 // false-match a QTO read); the consumer that KNOWS the identity re-stamps via WithType(QuantityType.Mass).
 // Operands are SI-base so no re-coercion; the display unit falls back to the composed dimension's SiSymbol.
 // Every algebra exit re-asserts the finite invariant: an overflowed product rails ValueRejected under the keyless
 // interior Op rather than minting an ∞ magnitude the content hash would treat as admitted evidence.
 public Fin<MeasureValue> Multiply(MeasureValue other) {
  Dimension product = this.Dimension.Multiply(other.Dimension);
  double si = Si * other.Si;
  return double.IsFinite(si)
   ? Fin.Succ(new MeasureValue(QuantityType.OfDimension(product), product, si, product.SiSymbol,
      CombineBand(this, other, si, static (l, r) => l * r, static (l, r) => (r, l))))
   : ElementFault.ValueRejected(Op.Of(name: nameof(Multiply)), "<measure-product-non-finite>");
 }

 // Interval quotient law: the 4-corner fold is exact ONLY where the divisor band excludes zero — when the corner
 // path will run (either operand bounds-only) over a zero-spanning divisor band, the true range is unbounded, so
 // the band honestly widens to ±∞ rather than the silently-wrong finite corners (a Gaussian÷Gaussian pair rides
 // the first-order GUM linearization instead). The MAGNITUDE is stricter than the band: a zero DIVISOR (or an
 // overflowed quotient) has no finite Si to carry, so it rails — an infinite BOUND is honest uncertainty, an
 // infinite magnitude is the rejected zero-divisor admission.
 public Fin<MeasureValue> Divide(MeasureValue other) {
  Dimension quotient = this.Dimension.Divide(other.Dimension);
  double si = Si / other.Si;
  if (!double.IsFinite(si)) { return ElementFault.ValueRejected(Op.Of(name: nameof(Divide)), $"<measure-quotient-non-finite:{other.Si:R}>"); }
  (MeasureBand lb, MeasureBand rb) = (EffectiveBand(this), EffectiveBand(other));
  return Fin.Succ(new MeasureValue(QuantityType.OfDimension(quotient), quotient, si, quotient.SiSymbol,
   (Uncertainty.IsSome || other.Uncertainty.IsSome) && !(lb.Kind.Gaussian && rb.Kind.Gaussian) && rb.LowerSi <= 0.0 && rb.UpperSi >= 0.0
    ? Some(MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity))
    : CombineBand(this, other, si, static (l, r) => l / r, static (l, r) => (1.0 / r, -l / (r * r)))));
 }

 // Scaling preserves Type AND Dimension (10 × one m³ is m³) — the op the AggregateEnvironmental/cost folds compose,
 // never a call-site `this with { Si = Si * factor }` re-spelling of the carrier; a non-finite factor or an
 // overflowed result rails on the same finite gate every construction and algebra exit holds.
 public Fin<MeasureValue> Scale(double factor) =>
  double.IsFinite(Si * factor)
   ? Fin.Succ(new MeasureValue(Type, Dimension, Si * factor, CanonicalUnit, Uncertainty.Map(band => ScaleBand(band, factor))))
   : ElementFault.ValueRejected(Op.Of(name: nameof(Scale)), $"<measure-scale-non-finite:{factor:R}>");

 // The ONE band-propagation algebra every fold (Multiply/Divide/Add) shares, dispatched on the UncertaintyKind
 // Gaussian column: two Gaussian operands combine σ in first-order quadrature over the op's partials (GUM:
 // σ² = (∂f/∂l·σl)² + (∂f/∂r·σr)²), re-expanded si ± k·σ under the widest coverage factor; any bounds-only operand
 // forces the conservative 4-corner interval — Normal σ/coverage is never silently flattened onto an Interval result.
 static Option<MeasureBand> CombineBand(MeasureValue left, MeasureValue right, double resultSi,
  Func<double, double, double> corner, Func<double, double, (double Dl, double Dr)> partials) =>
  left.Uncertainty.IsNone && right.Uncertainty.IsNone
   ? Option<MeasureBand>.None
   : Some(Propagate(EffectiveBand(left), EffectiveBand(right), left.Si, right.Si, resultSi, corner, partials));

 static MeasureBand Propagate(MeasureBand l, MeasureBand r, double leftSi, double rightSi, double resultSi,
  Func<double, double, double> corner, Func<double, double, (double Dl, double Dr)> partials) {
  if (l.Kind.Gaussian && r.Kind.Gaussian) {
   (double dl, double dr) = partials(leftSi, rightSi);
   double sigma = double.Hypot(dl * l.StandardDeviationSi.IfNone(0.0), dr * r.StandardDeviationSi.IfNone(0.0));
   double k = Math.Max(l.CoverageFactor.IfNone(1.0), r.CoverageFactor.IfNone(1.0));
   // σ=0 stays Exact — two exact operands never decay to a zero-width Normal the next fold re-reads as measured.
   return sigma == 0.0
    ? MeasureBand.Exact
    : double.IsFinite(sigma)
     ? MeasureBand.Normal(resultSi - k * sigma, resultSi + k * sigma, sigma, k)
     : MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity);
  }
  double[] corners = [corner(l.LowerSi, r.LowerSi), corner(l.LowerSi, r.UpperSi), corner(l.UpperSi, r.LowerSi), corner(l.UpperSi, r.UpperSi)];
  if (corners.Any(double.IsNaN)) {
   return (l.LowerSi == 0.0 && l.UpperSi == 0.0) || (r.LowerSi == 0.0 && r.UpperSi == 0.0)
    ? MeasureBand.Interval(UncertaintyKind.Exact, resultSi, resultSi)
    : MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity);
  }
  return MeasureBand.Interval(UncertaintyKind.Interval, corners.Min(), corners.Max());
 }

 // A None or declared-Exact operand contributes the degenerate si..si band (σ=0), so the corner fold and the
 // quadrature both read one effective shape — the raw Exact static's 0..0 bounds never leak into a corner product.
 static MeasureBand EffectiveBand(MeasureValue value) =>
  value.Uncertainty.Match(
   Some: band => band.Kind == UncertaintyKind.Exact ? MeasureBand.Interval(UncertaintyKind.Exact, value.Si, value.Si) : band,
   None: () => MeasureBand.Interval(UncertaintyKind.Exact, value.Si, value.Si));

 static MeasureBand ScaleBand(MeasureBand band, double factor) {
  if (factor == 0.0) { return MeasureBand.Exact; }
  double a = band.LowerSi * factor;
  double b = band.UpperSi * factor;
  double lower = Math.Min(a, b);
  double upper = Math.Max(a, b);
  double sigma = band.StandardDeviationSi.IfNone(0.0) * Math.Abs(factor);
  return band.Kind == UncertaintyKind.Normal && double.IsFinite(sigma) && sigma > 0.0
   ? MeasureBand.Normal(lower, upper, sigma, band.CoverageFactor.IfNone(1.0))
   : band.Kind == UncertaintyKind.Normal
    ? sigma == 0.0
     ? MeasureBand.Exact
     : MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity)
    : MeasureBand.Interval(band.Kind, lower, upper);
 }

}

// The model-level unit-presentation scheme — the IfcUnitAssignment counterpart the Graph/element#ELEMENT_GRAPH
// Header carries: QuantityType token -> registry unit-enum member NAME (the exact token the once-built Registry
// index keys), the empty default = SI. PRESENTATION ONLY — the interior stays SI, Header.CanonicalBytes EXCLUDES the
// scheme (a re-declared display unit never forks a snapshot identity), and the Bim ingress lowers IfcUnitAssignment
// onto it so the egress re-emits the model's declared units instead of forcing SI. Render is ONE policy read over the
// In egress — a declared, resolvable token converts; an undeclared or unresolvable one falls back to the SI magnitude
// and canonical unit, total either way — so a UI/schedule renders project units without per-call-site unit picks.
public readonly record struct UnitScheme(Map<string, string> Display) {
 public static readonly UnitScheme Si = new(Map<string, string>());
 public Option<string> UnitFor(QuantityType type) => Display.Find(type.Value);
 public (double Value, string Unit) Render(MeasureValue measure) =>
  UnitFor(measure.Type).Bind(unit => measure.In(unit).Map(value => (value, unit))).IfNone((measure.Si, measure.CanonicalUnit));
}
```

## [04]-[IMPLEMENTATION_LAW]

- [UNITSNET_SI_COERCION]: `Of` resolves dynamic unit input through `Quantity.TryFrom`, normalizes dimensional quantities through `ToUnit(UnitSystem.SI)`, and normalizes dimensionless quantities through `BaseUnitInfo`. `Registry` derives the inverse unit-name index from `Quantity.Infos`; `OfSi` stamps the same canonical unit name and `In` uses `UnitConverter.TryConvert` for total egress.
- [QUANTITY_TYPE_DISCRIMINATOR]: `QuantityType` carries `QuantityInfo.Name`, while `Dimension` carries the seven-exponent physical signature. Both are required because torque and energy share a dimension, and angle, ratio, level, and count are dimensionless. Named QTO reads derive from `As(QuantityType)`, and cross-quantity products compose dimensions before an informed consumer re-stamps semantic type through `WithType`.
- [BAND_PROPAGATION]: Gaussian operands propagate by first-order partial derivatives and quadrature; exact operands are the zero-variance identity. Bounds-only operands propagate through the four corners, and a divisor interval spanning zero widens to an unbounded interval. Non-finite propagated deviation also widens to an unbounded interval, while the nominal magnitude remains finite-gated.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
