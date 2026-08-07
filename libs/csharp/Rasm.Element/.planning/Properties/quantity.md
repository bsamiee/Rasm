# [ELEMENT_QUANTITY]

The typed measured-scalar owner: one `MeasureValue` carrying its SI magnitude, its OPTIONAL canonical unit token (absent wherever neither the registry nor the dimension roster names one, so no fabricated unit reaches a schedule cell or a unit-enum probe), a `Dimension` `[ComplexValueObject]` over the seven SI base-dimension exponents (the physical signature), and a `QuantityType` `[ValueObject<string>]` discriminator — a NAME-keyed identity admitted from the `UnitsNet` `QuantityInfo.Name` at the registry boundary OR minted as a consumer's engineering-domain name (`SectionModulus`/`TorsionConstant`/`WarpingConstant`) through the OPEN `QuantityType.Create` where the registry carries no matching quantity — coerced to its SI base and read back through type-checked accessors. The discriminator is the NAME, not the dimension, for TWO reasons: the registry name disambiguates a non-injective seven-exponent vector (`Torque` and `Energy` both reduce to `[L²·M·T⁻²]`, and `Angle` (rad), `Ratio`, and `SoundPressureLevel` (dB) all reduce to the zero vector shared with a bare `Count`), AND a consumer-minted domain name discriminates a `SectionModulus` from a `Volume` and a `TorsionConstant` from the registry's own `AreaMomentOfInertia` (both `L⁴`) where `UnitsNet` has no quantity at all — so the discriminator is the `UnitsNet` `QuantityInfo.Name` for a registry quantity and a `Create`-minted domain name for an engineering quantity the registry lacks, never a closed registry-only vocabulary the next consumer cannot extend. A `MeasureValue` is ANY measured quantity the `UnitsNet` registry names — `ThermalTransmittance` (the `HeatTransferCoefficient` quantity, W·m⁻²·K⁻¹), `Pressure` (Pa), `Force` (N), `MassDensity` (the `Density` quantity, kg·m⁻³), `PlaneAngle` (the `Angle` quantity, rad), and `Temperature` (K) all admit with their real type, dimension, and SI-COHERENT unit through the `SiUnit` election, while the logarithmic and prefixed-base minority — `SoundPressureLevel` (the `Level` quantity, dB) among them — REFUSES admission by name rather than persisting a magnitude the algebra cannot reduce; the six QTO names (`Length`/`Area`/`Volume`/`Weight`/`Count`/`Time`) are convenience accessors over the one polymorphic read, never a closed enum the next `Pset_*` measure cannot extend. The owner composes the admitted `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → the ONE `SiUnit` election → `ToUnit(elected)` → `QuantityInfo.Name` + `QuantityInfo.BaseDimensions`) and the `Properties/property#PROPERTY_VALUE` `PropertyValue.Measure` carrier for the typed property arm; the SI reprojection rides the kernel `Op.Catch` funnel so the `UnitsNet` boundary throw never escapes the rail, and a non-finite or unresolvable measure rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page realizes a `QuantityType`-discriminated, `Dimension`-signed `MeasureValue` over the full `UnitsNet` quantity family — carrying the dimensioned cross-quantity algebra (`Multiply`/`Divide` over the composed `Dimension`, `Scale`, `Sum`, each fold propagating the optional `MeasureBand` kind-dispatched: Gaussian quadrature for `Normal` operands, conservative corner intervals otherwise) so a derived takeoff composes ON the carrier rather than re-deriving the dimensional product on bare doubles — never a six-case enum, never a dimension-only discriminator, and never a parallel checked-accessor family beside the one polymorphic `As(QuantityType)` read.

## [01]-[INDEX]

- [02]-[DIMENSION]: the `Dimension` `[ComplexValueObject]` seven-SI-exponent physical signature (canonical-dimension static rows, the `Option`-valued `SiSymbol` display read, the `Multiply`/`Divide` derived-dimension algebra) and the `QuantityType` `[ValueObject<string>]` identity discriminator (the rostered registry and QTO rows, the open `Create` mint, the dimension-anonymous `OfDimension`).
- [03]-[MEASURE_VALUE]: the `MeasureValue` SI-coerced scalar carrier, the `UnitsNet` admission seam (the typed `Of(Enum)`, the abbreviation `Of(string)` external-source decode (the Rasm.Bim IFC/tabular/bSDD ingress convenience, NOT a cross-runtime wire shape), the SI-native `OfSi(QuantityType, Dimension, double)` + dimension-only `OfSi(Dimension, double)` + labeled registry-less `OfSi(QuantityType, Dimension, double, string)` factories, and the `OfCount` tally), the one polymorphic `As(QuantityType)` QTO read (the named `Length`/`Area`/`Volume`/`Weight`/`Time`/`Count` reads deriving from it), the unit-aware `In(Enum)`/`In(string)` display egress (the once-built registry unit index plus the guarded `UnitConverter.TryConvert` reprojection a UI/wire consumer reads a stored measure back in `Millimeter`/`Foot`/`DegreeCelsius` through, the token form the one-body delegate a scheme token resolves by name), the `UnitScheme` model-level unit-presentation scheme (the `IfcUnitAssignment` counterpart the `Graph/element#ELEMENT_GRAPH` `Header` carries — `QuantityType` token → declared display-unit token, `Render` the one policy read over `In`, presentation-only and canonical-bytes-excluded), the dimensioned `Multiply`/`Divide`/`Scale` cross-quantity algebra with kind-dispatched `MeasureBand` propagation, the band-preserving `WithType` semantic re-stamp a derived product re-types through, the `Quantize` content-hash projection, and the same-type `Sum` reducer.

## [02]-[DIMENSION]

- Owner: `Dimension` the `[ComplexValueObject]` carrying the seven SI base-dimension exponents (`Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity`), the physical signature a `MeasureValue` coerces to, the `Multiply`/`Divide` algebra composes, and the `Projection/address#CONTENT_ADDRESS` writer hashes; `QuantityType` the `[ValueObject<string>]` quantity-type identity (the `UnitsNet` `QuantityInfo.Name`) a `MeasureValue` discriminates on. The canonical-dimension static rows (`LengthDim`/`AreaDim`/`VolumeDim`/`MassDim`/`DurationDim`/`Dimensionless` and the derived `PressureDim`/`DensityDim`/`ForceDim`/`IrradianceDim`/`ThermalTransmittanceDim`) name the well-known dimensions; the rostered `QuantityType` rows carry the six QTO-matchable names (`Length`/`Area`/`Volume`/`Mass`/`Duration`/`Count`) a QTO accessor matches, the registry-named `AreaMomentOfInertia` a section consumer reads through `As`, and the `Scalar` additive identity.
- Entry: `Dimension.Of(BaseDimensions dims)` lowers the `UnitsNet` 7-vector onto the value-object; `Dimension.Create(length, mass, time, current, temperature, amount, luminousIntensity)` is the generated factory; `Multiply`/`Divide` add/subtract the exponent vectors so a derived dimension composes (`ForceDim.Divide(AreaDim)` IS `PressureDim`) without a per-quantity row. `QuantityType.Create(name)` admits a quantity-type name — the `UnitsNet` `QuantityInfo.Name` at the `Of` boundary, or a consumer's engineering-domain name where no `UnitsNet` quantity exists; the OPEN `Create` sanction is the SAME admission the `Composition/material#MATERIAL_PROPERTY` `SectionProperties` chain mints its engineering-section discriminators through — `SectionModulus`, `TorsionConstant`, and `WarpingConstant`, the three the registry genuinely lacks, while the section second moment IS the registry's `AreaMomentOfInertia` and rosters as a QTO row rather than minting a `Create` twin that would fork one identity into two spellings (`TorsionConstant` still shares that row's `L⁴` signature, the NAME alone separating them) — so a consumer-minted section discriminator is a first-class `QuantityType`, never a registry-only name the `Of` boundary alone admits; `QuantityType.OfDimension(dimension)` derives the dimension-anonymous identity an `OfSi(Dimension, _)` value carries. `OfAdmitted(type, dimension, si, canonicalUnit)` is the trusted RE-MINT beside the two admission paths — a decode, a re-lift, or a merge whose whole triple a prior admission already stamped — finite-gated and registry-probe-free, because re-deriving the unit would rewrite a consumer-minted token and the labeled path would refuse the registry-named type outright.
- Auto: the `[ComplexValueObject]` generates structural equality over the seven exponents so two `MeasureValue`s share a dimension iff their exponent vectors match, the `Of` projection reads the `UnitsNet` `BaseDimensions` integer exponents directly, and `Multiply`/`Divide` are pure exponent arithmetic mirroring the `BaseDimensions.Multiply`/`Divide` algebra so the seam never re-derives the dimensional product; the `[ValueObject<string>]` gives `QuantityType` ordinal value-equality so a `MeasureValue` discriminates on the registry name, and `OfDimension` encodes the 7-vector into a tilde-prefixed token that is dimension-unique yet NEVER equal to a QTO row, so an SI-native value admitted by dimension alone stays dimensioned-but-untyped and cannot false-match a QTO accessor.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`/`[ValueObject<string>]`), UnitsNet (`BaseDimensions` the 7-vector source, `QuantityInfo.Name` the quantity-type identity).
- Growth: a new well-known dimension is one static `Dimension` row (a `LuminousFlux`/`Frequency` row composed from exponents, or the `L⁴`/`L⁶` signatures the `SectionProperties` chain reaches through `Dimension.Create(int×7)` with no static row); a registry quantity a second consumer keys on by name is one `QuantityType` row (the `AreaMomentOfInertia` precedent — rostered because the registry names it, never `Create`-minted beside itself); a consumer's engineering-domain quantity (the `SectionModulus`/`TorsionConstant`/`WarpingConstant` the `Composition/material#MATERIAL_PROPERTY` `SectionProperties` mints, an analysis result scalar) mints through `QuantityType.Create(name)` with no seam row; a new measured quantity the registry names needs NO new dimension AND no new type — its `UnitsNet` `QuantityInfo` resolves both at admission — and a new engineering quantity the registry lacks needs only the `Create`-minted name; never a per-quantity dimension type and never a closed `QuantityKind` enum.
- Boundary: `QuantityType` is the ONE quantity-type discriminator and `Dimension` the physical signature — the six-case `QuantityKind` enum (the migration form) is the deleted form because it forced an out-of-family measure to a dimensionless `Count`, and the dimension-AS-discriminator form is the rejected form because distinct quantities share a dimension (`Torque`/`Energy` on `[L²·M·T⁻²]`; `Angle`/`Level`/`Ratio`/`Count` on the zero vector; `AreaMomentOfInertia`/`TorsionConstant` on `[L⁴]`); the identity is the `UnitsNet` `QuantityInfo.Name` for a registry quantity so any of the ~120 quantity structs admits with its real name, AND a consumer's engineering-domain name through the OPEN `Create` for an engineering quantity the registry lacks (`SectionModulus`/`TorsionConstant`/`WarpingConstant` — the section second moment is NOT among them, the registry naming it `AreaMomentOfInertia`, so it rosters as a QTO row and a `Create` twin beside it is the forked-identity defect) — `Create` is the sanctioned open mint, NOT a registry-only gate that phantoms the `SectionProperties` discriminators — the named QTO rows the convenience anchors a QTO accessor matches, and the `Multiply`/`Divide` algebra composes a derived dimension from base ones rather than enumerating every product; the exponents come from the `UnitsNet` `BaseDimensions` (or the `Dimension.Create(int×7)` generated factory for the `L⁴`/`L⁶` signatures the static rows omit) and a hand-coded dimension table that drifts from the unit registry — or a name string parsed at a call site rather than minted through `QuantityType.Create` — is the named defect.

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
 public static readonly Dimension LinearDensityDim = Create(-1, 1, 0, 0, 0, 0, 0);
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

 // SI coherent-unit symbol for the rostered rows — the display DEFAULT for a measure the registry cannot name
 // (consumer mints, OfDimension identities, Multiply/Divide products); a registry-named OfSi stamps the registry unit
 // NAME instead. Display-only, never hashed (CanonicalWriter.Measure writes the type token, never the unit).
 // Dimensionless carries NO row: a ratio, an angle, and a tally have no SI unit symbol, and a blank entry would be a
 // fabricated token wearing an empty spelling — the same lie the unrostered fallback tells, one character shorter.
 static readonly FrozenDictionary<Dimension, string> SiSymbols = new Dictionary<Dimension, string> {
  [LengthDim] = "m", [AreaDim] = "m2", [VolumeDim] = "m3", [MassDim] = "kg",
  [DurationDim] = "s", [ForceDim] = "N", [PressureDim] = "Pa", [DensityDim] = "kg/m3",
  [LinearDensityDim] = "kg/m",
  [IrradianceDim] = "W/m2", [ThermalTransmittanceDim] = "W/(m2.K)",
 }.ToFrozenDictionary();

 // ABSENCE is the honest answer for an unrostered signature. A fabricated "SI" token renders "42 SI" at every UI
 // and schedule, and — worse — a measure stamped with it can never resolve a unit enum, so it silently voids In for
 // its whole class. Option makes the unresolvable state explicit and the display fallback the renderer's choice.
 public Option<string> SiSymbol => SiSymbols.TryGetValue(this, out string? symbol) ? Some(symbol) : None;
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

 // The rostered rows — each .Value matches the UnitsNet QuantityInfo.Name (an Of-admitted value carries the
 // registry's own name) plus two seam sentinels with no registry quantity. Seam-ROSTERED rows ONLY: the OPEN Create
 // additionally mints consumer engineering names — SectionModulus/TorsionConstant/WarpingConstant, the three the
 // registry genuinely lacks — so a consumer-minted section discriminator needs NO static row here.
 public static readonly QuantityType Length = Create("Length");
 public static readonly QuantityType Area = Create("Area");
 public static readonly QuantityType Volume = Create("Volume");
 public static readonly QuantityType Mass = Create("Mass");
 public static readonly QuantityType Duration = Create("Duration");
 // The section second moment IS a registry quantity (AreaMomentOfInertia, base unit MeterToTheFourth), so it
 // rosters HERE: an Iyy/Izz admitted through Of already carries this exact name and In round-trips its display
 // unit, where a Create-minted twin would split one physical identity across two content-key spellings. It is not
 // QTO-matchable — the named QTO reads cover the six takeoff names alone and this row is read through As directly.
 public static readonly QuantityType AreaMomentOfInertia = Create("AreaMomentOfInertia");
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

- Owner: `MeasureValue` the SI-coerced measured-scalar carrier (`QuantityType Type` + `Dimension Dimension` + `double Si` + `Option<string> CanonicalUnit` + `Option<MeasureBand> Uncertainty`) the `Properties/property#PROPERTY_VALUE` `Measure` arm, the `Composition/material#MATERIAL_PROPERTY` measured columns, and the `Assessment/assessment#ASSESSMENT_NODE` result scalars read; `MeasureBand` the neutral uncertainty interval/band carrier plus optional distribution metadata; `UncertaintyKind` the closed uncertainty-model vocabulary whose `Gaussian` column dispatches the propagation algebra, the token a package-specific uncertainty library lowers onto; `UnitScheme` the model-level unit-presentation scheme the `Graph/element#ELEMENT_GRAPH` `Header` carries — the `IfcUnitAssignment` counterpart mapping a `QuantityType` token to its declared display-unit token, `Render` composing the `In` egress, presentation-only and excluded from every canonical byte.
- Entry: `MeasureValue.Of` and `OfSi` admit finite magnitudes; `MeasureBand.Admit` validates ordered non-NaN bounds and kind-specific distribution metadata; `WithUncertainty(band, key)` additionally requires the band to contain the nominal magnitude. `WithType` re-stamps semantic identity without discarding the admitted band. Every construction and algebra exit preserves the finite-magnitude invariant or returns `ElementFault.ValueRejected`.
- Auto: `Of` routes the raw value through `Quantity.TryFrom(value, unit)` to a boxed `IQuantity`, then `Coerce` reprojects through the ONE `SiUnit` election — the declared `BaseUnitInfo` by default, an `SiElection` row where that base carries a prefix or convention factor (`Radian` for `Angle`, `KilogramPerSecond` for `MassFlow`, `SquareMeterKelvinPerWatt` for `ThermalResistance`), and a `None`-valued row for a quantity whose roster holds no coherent unit at all, which refuses by name — so `Admit` reads `(double)Value` plus `QuantityInfo.Name` plus `Dimension.Of(Dimensions)` and the persisted scalar is base-normalized whatever the admission spelling, never a hand-mapped kind; uncertainty rides the optional `MeasureBand`, not a second deterministic value, and propagates through every algebra fold kind-dispatched on the `UncertaintyKind.Gaussian` column.
- Receipt: a `MeasureValue` is the unit-checked evidence a takeoff, a property facet, and a cost join read — `measure.Si` is the SI magnitude, `measure.CanonicalUnit` the optional SI unit token (absent where neither the registry nor the dimension roster names one), `measure.Type` the quantity-type identity, `measure.Dimension` the physical signature, and `measure.Uncertainty` the optional neutral bounds plus distribution metadata, absent exactly where the value is exact; the source package that computed uncertainty stays above the seam.
- Packages: UnitsNet (`Quantity.TryFrom(double, Enum, out)` + `Quantity.TryFromUnitAbbreviation(culture, value, abbr, out)` the dynamic ingress, `Quantity.Infos`/`QuantityInfo.GetUnitInfosFor(BaseUnits)`/`UnitInfo.Name`/`UnitInfo.Value` the registry metadata the once-built unit index projects, `UnitConverter.TryConvert(QuantityValue, Enum, Enum, out double)` the guarded egress conversion `In` reads, `QuantityInfo.BaseUnitInfo` the declared-base row the `SiUnit` election defaults to, `IQuantity.ToUnit(Enum)`/`.QuantityInfo.Name`/`.Value`/`.Unit` the coercion + identity surface, `BaseDimensions` the 7-vector (`ToUnit(UnitSystem)` and `GetUnitInfosFor(BaseUnits)` are NOT composed — the `BaseUnits` metadata they resolve through is `Undefined` on most rows), the cross-quantity-operator algebra `MeasureValue.Multiply`/`Divide` mirrors onto the one carrier), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Option`/`Seq` + `Bind`/`MapFail`/`Choose`), `Rasm` (the kernel `Op` op-key + `Op.Catch` exception funnel).
- Growth: a new measured quantity needs NO seam edit when its declared `BaseUnitInfo` is SI-coherent — its `UnitsNet` `QuantityInfo` resolves type and dimension at `Of` admission; a quantity whose declared base carries a scale prefix or convention factor and one whose roster holds no coherent unit are the SAME edit — ONE `SiElection` row whose `Option` value is the discriminant, read by the one `SiUnit` election the `SiNames` mirror projects, never a second table and never a per-quantity branch; a new display unit a consumer renders in needs NO seam edit — it is a `UnitsNet` unit-enum member `In(unit)` already reprojects to; a new derived takeoff composes through the existing `Multiply`/`Divide`/`Scale`/`Sum` algebra (a `Force = Pressure × Area` is one `pressure.Multiply(area)`, never a new operation); a new named convenience read is one derived line over `As(QuantityType)`; never a per-quantity dimension type, a closed `QuantityKind` enum, or a parallel cross-quantity operation family.
- Boundary: `MeasureValue` NEVER carries a bare `double` quantity — the value coerces to SI base once at `Of` through the `UnitsNet` registry and the interior carries the SI scalar plus the typed `QuantityType`/`Dimension`; a free `double` measure field is the named defect per the `UnitsNet` ownership of dimensioned scalars.
- Boundary: the discriminator is the `QuantityType` (the `QuantityInfo.Name`) so a `Torque` and an `Energy` sharing a `Dimension` stay distinct and a dimensionless `Angle`/`Level`/`Ratio` never collapses onto `Count` — dimension-as-discriminator, and a closed kind enum that degrades an out-of-family measure to `Count`, are the two deleted forms.
- Boundary: the SI coercion rides `Quantity.TryFrom` plus the ONE `SiUnit` election; a stringly-keyed unit switch or an ad-hoc conversion factor is the deleted form, and `[04]` `[UNITSNET_SI_COERCION]` owns why the registry's own SI walk cannot serve as that election.
- Boundary: the ADMITTED CLASS is stated, not discovered — a quantity admits iff `SiUnit` elects a coherent unit for it, which the declared `BaseUnitInfo` supplies for every quantity the `SiElection` roster does not name, while a `None`-valued row REFUSES by name (`<measure-si-incoherent:Name>`): the logarithmic `Level`/`AmplitudeRatio`/`PowerRatio`, the volt-ampere-hour `ApparentEnergy`/`ElectricApparentEnergy`/`ReactiveEnergy`/`ElectricReactiveEnergy`, the `Percent`-only `RelativeHumidity`, the inverted `FuelEfficiency`, and the kilonewton-based `SpecificFuelConsumption`. Refusing beats persisting: a decibel entering the `Sum`/`Multiply` algebra as a linear scalar is a physics error, and a kilowatt- or gram-based magnitude stored as SI is wrong by a power of ten.
- Boundary: `PlaneAngle` stores RADIANS, so a 30° and a 0.5236 rad admission hash identically under `CanonicalWriter.Measure`; an as-constructed keep is the deleted form. The conversion rides the kernel `Op.Catch` funnel so no foreign `Exception` crosses a seam signature.
- Boundary: aggregation flows through `MeasureValue.Sum` (an SI-base scalar sum guarded by `Type` equality, stricter than a dimension guard, a cross-type sum railing `ElementFault.ValueRejected`), and a derived quantity through `MeasureValue.Multiply`/`Divide` (the SI product/quotient over the composed `Dimension`, the result dimension-anonymous so the seam never false-names a `Volume × Density` as `Mass`) — never an unwrap-compute-rewrap or a hand-reconstructed result dimension.
- Boundary: the consumer that KNOWS a product's identity re-stamps through the band-preserving `WithType`, which re-resolves the canonical unit through the ONE `CanonicalUnitFor` probe `OfSi` stamps and carries the propagated `MeasureBand` across; a bare re-mint that strands the stale unit or discards the band is the deleted form.
- Boundary: the FINITE invariant holds at EVERY construction path and algebra exit — `Of`/`OfCount` gate the raw ingress, `OfSi` gates the SI-native mint, and `Multiply`/`Divide`/`Scale`/`Sum` rail an overflowed product, a zero-divisor quotient, or a non-finite factor — so a NaN/∞ magnitude is unrepresentable in an admitted `MeasureValue`. An infinite `MeasureBand` BOUND stays honest uncertainty: the band axis is looser than the magnitude by design.
- Boundary: the optional `MeasureBand` propagates through EVERY fold kind-dispatched on the `UncertaintyKind.Gaussian` column — Gaussian operands combine σ in first-order quadrature over the op's partial derivatives and re-expand `si ± k·σ` under the widest declared coverage factor, while any bounds-only operand forces the conservative 4-corner interval (exact for monotone ops, so a `Divide` whose bounds-only divisor band spans zero widens to the honest ±∞ interval). A head-band re-stamp dressing a summed magnitude in an unsummed band, and an interval collapse discarding a `Normal` operand's `StandardDeviationSi`/`CoverageFactor`, are the two deleted forms.
- Boundary: EXACTNESS IS BAND ABSENCE — every propagation and scaling arm that would mint a zero-width band answers `None`, because a `Si = 42, band 0..0` value refuses its own re-admission through the `WithUncertainty` contains-the-nominal gate, and a `band 42..42` beside a nominal of 42 is redundant evidence the next fold re-reads as measured.
- Boundary: `Quantize` is the ONE rounding owner, applied at the content-hash boundary against `Header.Tolerance` over the magnitude AND its band bounds and σ, so the interior stays full-precision, the canonical key is tolerance-stable, and the band the writer takes as preimage rides the same grid the magnitude does. The coverage factor is a declared policy scalar on no physical axis and never quantizes.
- Boundary: `Scale(factor)` is the type-and-dimension-preserving basis-aware multiple a cost or environmental ply scaling composes, never a call-site magnitude mutation; the dimension-only `OfSi(Dimension, double)` carries the dimension-anonymous `QuantityType`, so a bare SI-native scalar is dimensioned-but-untyped and a QTO read fires ONLY for a measure admitted with an explicit quantity type.
- Boundary: the named QTO reads (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) DERIVE from the one `As(QuantityType)` body (DERIVED_LOGIC) — a parallel checked-accessor family is the deleted form and the convenience names are one hop over the polymorphic read.
- Boundary: the interior carries SI ALONE while EGRESS is unit-parameterized — `In(Enum unit)` resolves the stored `(Type.Value, CanonicalUnit)` through the once-built registry unit index (`UnitInfo.Name` IS `Unit.ToString()`, so the index keys exactly the token `Of` stamps) and converts through the guarded `UnitConverter.TryConvert`, so a consumer renders a stored U-value in `WattPerSquareMeterKelvin`, a length in `Millimeter`/`Foot`, or a temperature in `DegreeCelsius` without re-deriving a factor at the call site and without a throw across the read.
- Boundary: `CanonicalUnit` is `Option<string>` and an unresolvable unit is ABSENCE, never a fabricated token — `OfSi(QuantityType, Dimension, _)` stamps the registry unit NAME for a registry-named type through the SAME `SiUnit` election `Coerce` reprojects through (so the two admission paths are one decision and an incoherent quantity is absent from BOTH rather than present on one), falls back to the composed dimension's rostered SI symbol, and answers `None` for a consumer-minted type or a dimension-anonymous product the registry cannot resolve. A stamped placeholder can never key a unit enum, so it voids `In` for its whole class while reading like a unit at every schedule cell.
- Boundary: a registry-less domain-basis scalar whose CONVENTIONAL label matters to a wire consumer (kgCO2e, dB, W/K) admits through the labeled `OfSi(QuantityType, Dimension, double, string)` — the label rides `CanonicalUnit` display-only under the same finite gate, and the overload REFUSES a registry-named type so a per-call-site label can never fork a registry quantity's canonical unit.

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

 public static Fin<MeasureBand> Admit(
  UncertaintyKind kind, double lowerSi, double upperSi,
  Option<double> standardDeviationSi, Option<double> coverageFactor, Op key) =>
  double.IsNaN(lowerSi) || double.IsNaN(upperSi) || lowerSi > upperSi
   ? ElementFault.ValueRejected(key, "<measure-band-bounds-invalid>")
   : kind == UncertaintyKind.Normal
    // The K-rail exit is MANDATORY: the tuple Apply lands K<Option, Fin<MeasureBand>>, and IfNone is Option's own
    // member — without .As() re-anchoring the join back onto the concrete carrier there is nothing to call it on.
    ? (standardDeviationSi, coverageFactor).Apply((sd, coverage) =>
       double.IsFinite(lowerSi) && double.IsFinite(upperSi) && double.IsFinite(sd) && sd > 0.0 && double.IsFinite(coverage) && coverage > 0.0
        ? Fin.Succ(new MeasureBand(kind, lowerSi, upperSi, Some(sd), Some(coverage)))
        : ElementFault.ValueRejected(key, "<measure-band-normal-invalid>"))
      .As()
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

 // The band's own grid projection, composed by MeasureValue.Quantize at the content-hash boundary. Bounds and the
 // standard deviation are magnitudes on the measure's own SI axis and ride the grid; the COVERAGE FACTOR is a
 // declared policy scalar on no physical axis (k = 1, 2, 3), so a length tolerance has nothing to say about it and
 // rounding it would quantize a policy against a geometry grid. Non-finite bounds pass through — an unbounded band
 // is honest uncertainty, and Math.Round over ±∞ is already the identity.
 internal MeasureBand Quantize(double tolerance) =>
  new(Kind,
      double.IsFinite(LowerSi) ? MeasureValue.Grid(LowerSi, tolerance) : LowerSi,
      double.IsFinite(UpperSi) ? MeasureValue.Grid(UpperSi, tolerance) : UpperSi,
      StandardDeviationSi.Map(sd => MeasureValue.Grid(sd, tolerance)),
      CoverageFactor);
}

public sealed record MeasureValue {
 private MeasureValue(QuantityType type, Dimension dimension, double si, Option<string> canonicalUnit, Option<MeasureBand> uncertainty) =>
  (Type, Dimension, Si, CanonicalUnit, Uncertainty) = (type, dimension, si, canonicalUnit, uncertainty);

 public QuantityType Type { get; }
 public Dimension Dimension { get; }
 public double Si { get; }
 // ABSENT where no registry name and no rostered dimension symbol resolves — a dimensionless tally, a consumer-minted
 // section discriminator, an OfDimension product. Absence is the model's state and a blank spelling the RENDERER's
 // choice, so no fabricated token ever reaches a content key, a schedule cell, or a unit-enum probe.
 public Option<string> CanonicalUnit { get; }
 public Option<MeasureBand> Uncertainty { get; }

 // The empty-Sum result — Scalar-typed (a dimensionless untyped scalar), distinct from an explicit Count; it seeds
 // only the empty fold, because Sum's Type guard faults a Scalar head against any non-Scalar tail.
 public static readonly MeasureValue Zero =
  new(QuantityType.Scalar, Dimension.Dimensionless, 0.0, Dimension.Dimensionless.SiSymbol, Option<MeasureBand>.None);

 public Fin<MeasureValue> WithUncertainty(MeasureBand band, Op key) =>
  band.LowerSi <= Si && band.UpperSi >= Si
   ? Fin.Succ(new MeasureValue(Type, Dimension, Si, CanonicalUnit, Some(band)))
   : ElementFault.ValueRejected(key, "<measure-band-excludes-nominal>");

 // The band-preserving semantic re-stamp a dimension-anonymous Multiply/Divide product re-types through when the
 // consumer KNOWS the identity (NetWeight IS volume×density re-typed Mass). It RAILS on exactly one thing — a target
 // type whose registry dimension contradicts the value's own — because a re-stamp is a claim about identity, not a
 // conversion: the magnitude's finiteness is already settled by the construction invariant and is never re-gated.
 // The registry canonical unit re-resolves through the SAME CanonicalUnitFor probe OfSi stamps (so In round-trips
 // the re-typed product) and the Uncertainty propagates — a bare `with { Type = type }` that strands the stale unit,
 // or a re-mint through the OfSi gate that re-rails a settled magnitude, is the deleted form.
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

 // The SI-COHERENT UNIT ELECTION — the ONE owner both the Coerce reprojection and the SiNames mirror read, so the
 // persisted magnitude and the stamped canonical unit are the same decision and cannot fork ([04]
 // UNITSNET_SI_COERCION owns why the registry's own SI walk cannot serve this). The declared BaseUnitInfo IS the
 // coherent unit for all but a named handful, so the election is that default plus ONE SiElection row per departing
 // quantity — whether it elects a different coherent unit or none at all — refused LOUDLY at admission, never
 // persisted wrong by a power of ten.
 // ONE row set, not two: every departure from the declared base is a named quantity mapped to the coherent unit it
 // actually elects, and a quantity whose whole roster holds NO coherent unit maps to None. The Option IS the
 // discriminant, so the election is one lookup with a default rather than a set probe feeding a dictionary probe,
 // and a new departure lands as one row whichever kind it is. No coherent scalar exists for the None rows: the
 // logarithmic trio carries no linear magnitude to sum or multiply (the algebra below would be a physics error on a
 // decibel), the volt-ampere-HOUR energies and the gram-per-KILOnewton-second consumption offer no unprefixed
 // sibling, RelativeHumidity ships Percent alone, and FuelEfficiency inverts its own dimension — each REFUSES
 // admission by name rather than entering the canonical bytes as a magnitude no consumer can reduce.
 static readonly FrozenDictionary<string, Option<Enum>> SiElection = new Dictionary<string, Option<Enum>>(StringComparer.Ordinal) {
  ["Angle"] = Some<Enum>(AngleUnit.Radian),                                            // declared base is Degree — the radian keep this makes real
  ["MassFlow"] = Some<Enum>(MassFlowUnit.KilogramPerSecond),                           // declared base is GramPerSecond (x1e-3)
  ["ThermalResistance"] = Some<Enum>(ThermalResistanceUnit.SquareMeterKelvinPerWatt),  // declared base is per-Kilowatt (x1e-3)
  ["AmplitudeRatio"] = None, ["ApparentEnergy"] = None, ["ElectricApparentEnergy"] = None,
  ["ElectricReactiveEnergy"] = None, ["FuelEfficiency"] = None, ["Level"] = None,
  ["PowerRatio"] = None, ["ReactiveEnergy"] = None, ["RelativeHumidity"] = None,
  ["SpecificFuelConsumption"] = None,
 }.ToFrozenDictionary(StringComparer.Ordinal);

 static Option<Enum> SiUnit(QuantityInfo info) =>
  SiElection.TryGetValue(info.Name, out Option<Enum> elected) ? elected : Some(info.BaseUnitInfo.Value);

 // The ONE registry projection, built once over the process-fixed Quantity.Infos roster (DERIVED_LOGIC): the
 // (quantity, unit) NAME -> unit Enum index the In egress resolves through, and the quantity name -> canonical unit
 // NAME map the OfSi mint stamps — the SiNames row is the SiUnit election VERBATIM, the same decision Coerce
 // reprojects through, and UnitInfo.Name IS Unit.ToString(), so the Of and OfSi canonical units CANNOT drift and an
 // incoherent quantity is absent from BOTH sides rather than present on one.
 static readonly Lazy<(
  FrozenDictionary<(string Quantity, string Unit), Enum> Units,
  FrozenDictionary<string, string> SiNames,
  FrozenDictionary<string, Dimension> Dimensions)> Registry = new(static () => (
  Quantity.Infos.SelectMany(static info => info.UnitInfos.Select(unit => KeyValuePair.Create((info.Name, unit.Name), unit.Value))).ToFrozenDictionary(),
  Quantity.Infos.AsIterable()
   .Choose(static info => SiUnit(info).Map(unit => (info.Name, Si: unit.ToString())))
   .ToFrozenDictionary(static row => row.Name, static row => row.Si),
  Quantity.Infos.ToFrozenDictionary(static info => info.Name, static info => Dimension.Of(info.BaseDimensions))));

 // The SI-native ADMISSION — the entry a Rasm.Compute result, a SectionProperties bake, and the wire decode stamp
 // computed measures through, FINITE-GATED like every other construction path: a NaN/∞ scalar rails ValueRejected
 // under the keyless interior Op (the Classification ValidateFactoryArguments precedent — a keyed caller re-stamps
 // via MapFail), so a non-finite magnitude is unrepresentable in an admitted MeasureValue and the canonical bytes
 // hash admitted evidence only — the UnitsNet-ingress-only finite check that let a raw OfSi double bypass the
 // invariant is the deleted form. A registry-named type stamps the registry canonical unit NAME so In round-trips
 // it; a consumer-minted type falls back to the dimension's rostered SI symbol and to ABSENCE beyond that, and In
 // honestly answers None — the probe falls back, never faults; identity is Type + Dimension, the unit display-only.
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
    : Fin.Succ(new MeasureValue(type, dimension, si, Some(unit), Option<MeasureBand>.None));

 // The TRUSTED RE-MINT: a magnitude whose (type, dimension, unit) triple came from a PRIOR admission — a decoded
 // observation sample re-lifted under its series' own stamped unit, a wire payload re-admitted, a merge re-forming a
 // combined magnitude — takes this path, which gates finiteness and nothing else. It is a third path only because the
 // discriminant is the CALLER'S evidence, unrecoverable from the arguments: the labeled mint must keep refusing a
 // registry-named type, since a per-call-site label there forks a registry quantity's unit, while a re-mint carries
 // no new label at all and re-deriving the unit through the 3-arg path would silently rewrite a consumer-minted
 // type's conventional token to its bare SI symbol. Routing a re-mint through the labeled path was the defect this
 // verb closes — every registry-named observation series refused on EVERY sample and the typed lift was dead.
 public static Fin<MeasureValue> OfAdmitted(QuantityType type, Dimension dimension, double si, Option<string> canonicalUnit) =>
  double.IsFinite(si)
   ? Fin.Succ(new MeasureValue(type, dimension, si, canonicalUnit, Option<MeasureBand>.None))
   : ElementFault.ValueRejected(Op.Of(name: nameof(OfAdmitted)), $"<measure-si-non-finite:{si:R}>");

 // The ONE canonical-unit resolution OfSi stamps and WithType re-stamps: the registry SiNames mirror for a
 // registry-named type, else the composed dimension's rostered SI symbol, else ABSENT. Total, never a fault source
 // and never a fabricated token — an unresolvable unit is a state the read surfaces, not a string it invents.
 static Option<string> CanonicalUnitFor(QuantityType type, Dimension dimension) =>
  Registry.Value.SiNames.TryGetValue(type.Value, out string? name) ? Some(name) : dimension.SiSymbol;

 // A tally is dimensionless and carries no unit token — DERIVED through the same dimensionless SiSymbol read Zero
 // takes, so the two agree by construction rather than by two authors spelling one blank literal the same way.
 public static Fin<MeasureValue> OfCount(double value, Op key) =>
 double.IsFinite(value)
 ? Fin.Succ(new MeasureValue(QuantityType.Count, Dimension.Dimensionless, value, Dimension.Dimensionless.SiSymbol, Option<MeasureBand>.None))
 : ElementFault.ValueRejected(key, $"<count-non-finite:{value:R}>");

 // The UnitsNet admission seam: ONE reprojection through the SiUnit election, dimensional and dimensionless alike, so
 // a degree-admitted and a radian-admitted angle store ONE radian magnitude and cannot fork the content key, and a
 // quantity with no coherent unit refuses BY NAME before the conversion rather than persisting a prefixed magnitude.
 // The conversion still rides the kernel Op.Catch funnel so no foreign Exception crosses the seam signature, though
 // the election leaves the whole (quantity, unit) roster convertible.
 static Fin<MeasureValue> Coerce(IQuantity quantity, Op key) =>
 SiUnit(quantity.QuantityInfo).Match(
  None: () => ElementFault.ValueRejected(key, $"<measure-si-incoherent:{quantity.QuantityInfo.Name}>"),
  Some: unit => key.Catch(() => Fin.Succ(quantity.ToUnit(unit)))
   .MapFail(_ => ElementFault.ValueRejected(key, $"<measure-si-unavailable:{quantity.QuantityInfo.Name}>"))
   .Bind(si => Admit(si, key)));

 // The identity is the UnitsNet QuantityInfo.Name (Torque vs Energy, Angle vs Level vs Ratio — distinct names that SHARE
 // a Dimension), the Dimension the physical 7-vector, the Si the SI-base magnitude, the unit the SI unit token.
 static Fin<MeasureValue> Admit(IQuantity si, Op key) =>
 double.IsFinite((double)si.Value)
 ? Fin.Succ(new MeasureValue(QuantityType.Create(si.QuantityInfo.Name), Dimension.Of(si.QuantityInfo.BaseDimensions), (double)si.Value, Some(si.Unit.ToString()), Option<MeasureBand>.None))
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
 // Registry index, then the guarded static UnitConverter.TryConvert reprojects — TOTAL: an ABSENT canonical unit, a
 // wrong-family target, a consumer-minted QuantityType, and a dimension-anonymous OfSi(Dimension,_) all answer None,
 // never an As(Enum) throw across an Option read. The absent-unit arm is the FIRST gate rather than a probe against a
 // fabricated token that could never key the index anyway. Names are enum member names — no ambient culture forks.
 public Option<double> In(Enum unit) =>
  CanonicalUnit
   .Bind(stored => Registry.Value.Units.TryGetValue((Type.Value, stored), out Enum? handle) ? Some(handle) : None)
   .Bind(handle => UnitConverter.TryConvert(Si, handle, unit, out double converted) ? Some(converted) : None);

 // The token-keyed egress the Header UnitScheme composes — the SAME one In body, the target unit resolved by its
 // registry enum-member NAME through the once-built index (UnitInfo.Name IS Unit.ToString(), so the scheme token and
 // the enum member never drift); a wrong-family or unknown token answers None, never a second conversion body.
 public Option<double> In(string unitName) =>
  Registry.Value.Units.TryGetValue((Type.Value, unitName), out Enum? unit) ? In(unit) : None;

 // Tolerance quantization for the content-hash boundary: round the SI magnitude to the tolerance grid so two measures
 // within Header.Tolerance project to the same canonical bytes; + 0.0 normalizes a -0.0 result to +0.0 (one grid zero).
 // The BAND rides the same grid, because the CanonicalWriter writes it as preimage: two bands within tolerance must
 // address identically or the quantization the magnitude enjoys is undone one column over. This is the ONE rounding
 // owner — a second grid at the writer would fork the two projections on the first tolerance edit.
 public MeasureValue Quantize(double tolerance) =>
 tolerance > 0.0 && double.IsFinite(Si / tolerance)
  ? new MeasureValue(Type, Dimension, Grid(Si, tolerance), CanonicalUnit, Uncertainty.Map(band => band.Quantize(tolerance)))
  : this;

 internal static double Grid(double value, double tolerance) =>
  (Math.Round(value / tolerance, MidpointRounding.AwayFromZero) * tolerance) + 0.0;

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
 // Operands are SI-base so no re-coercion; the display unit is the composed dimension's rostered SI symbol, ABSENT
  // where the product's signature carries no row — a composed dimension is exactly where a fabricated token would land.
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
   ? Fin.Succ(new MeasureValue(Type, Dimension, Si * factor, CanonicalUnit, Uncertainty.Bind(band => ScaleBand(band, factor))))
   : ElementFault.ValueRejected(Op.Of(name: nameof(Scale)), $"<measure-scale-non-finite:{factor:R}>");

 // The ONE band-propagation algebra every fold (Multiply/Divide/Add) shares, dispatched on the UncertaintyKind
 // Gaussian column: two Gaussian operands combine σ in first-order quadrature over the op's partials (GUM:
 // σ² = (∂f/∂l·σl)² + (∂f/∂r·σr)²), re-expanded si ± k·σ under the widest coverage factor; any bounds-only operand
 // forces the conservative 4-corner interval — Normal σ/coverage is never silently flattened onto an Interval result.
 static Option<MeasureBand> CombineBand(MeasureValue left, MeasureValue right, double resultSi,
  Func<double, double, double> corner, Func<double, double, (double Dl, double Dr)> partials) =>
  left.Uncertainty.IsNone && right.Uncertainty.IsNone
   ? Option<MeasureBand>.None
   : Propagate(EffectiveBand(left), EffectiveBand(right), left.Si, right.Si, resultSi, corner, partials);

 // EXACTNESS IS BAND ABSENCE. Every arm that would mint a zero-width band answers None instead, because a stored
 // `Si = 42, band 42..42` is redundant evidence and a stored `band 0..0` beside a non-zero nominal REFUSES its own
 // re-admission through WithUncertainty's contains-the-nominal gate — a value the algebra can mint and the
 // admission cannot accept is unrepresentable by construction here rather than by a caller remembering.
 static Option<MeasureBand> Propagate(MeasureBand l, MeasureBand r, double leftSi, double rightSi, double resultSi,
  Func<double, double, double> corner, Func<double, double, (double Dl, double Dr)> partials) {
  if (l.Kind.Gaussian && r.Kind.Gaussian) {
   (double dl, double dr) = partials(leftSi, rightSi);
   double sigma = double.Hypot(dl * l.StandardDeviationSi.IfNone(0.0), dr * r.StandardDeviationSi.IfNone(0.0));
   double k = Math.Max(l.CoverageFactor.IfNone(1.0), r.CoverageFactor.IfNone(1.0));
   // σ=0 is two exact operands — no band at all, never a zero-width Normal the next fold re-reads as measured.
   return sigma == 0.0
    ? Option<MeasureBand>.None
    : double.IsFinite(sigma)
     ? Some(MeasureBand.Normal(resultSi - k * sigma, resultSi + k * sigma, sigma, k))
     : Some(MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity));
  }
  double[] corners = [corner(l.LowerSi, r.LowerSi), corner(l.LowerSi, r.UpperSi), corner(l.UpperSi, r.LowerSi), corner(l.UpperSi, r.UpperSi)];
  if (corners.Any(double.IsNaN)) {
   // A degenerate zero operand annihilates the product exactly; anything else that produced a NaN corner has an
   // unbounded true range and says so.
   return (l.LowerSi == 0.0 && l.UpperSi == 0.0) || (r.LowerSi == 0.0 && r.UpperSi == 0.0)
    ? Option<MeasureBand>.None
    : Some(MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity));
  }
  return Some(MeasureBand.Interval(UncertaintyKind.Interval, corners.Min(), corners.Max()));
 }

 // A None or declared-Exact operand contributes the degenerate si..si band (σ=0) ANCHORED AT ITS OWN NOMINAL, so the
 // corner fold and the quadrature read one effective shape and no 0..0 interval ever leaks into a corner product.
 static MeasureBand EffectiveBand(MeasureValue value) =>
  value.Uncertainty
   .Filter(static band => band.Kind != UncertaintyKind.Exact)
   .IfNone(() => MeasureBand.Interval(UncertaintyKind.Exact, value.Si, value.Si));

 // Scaling by zero annihilates the magnitude exactly, and a Normal whose σ scales to zero is exact — both answer
 // absence under the same law the propagation folds hold.
 static Option<MeasureBand> ScaleBand(MeasureBand band, double factor) {
  if (factor == 0.0) { return Option<MeasureBand>.None; }
  double a = band.LowerSi * factor;
  double b = band.UpperSi * factor;
  double lower = Math.Min(a, b);
  double upper = Math.Max(a, b);
  double sigma = band.StandardDeviationSi.IfNone(0.0) * Math.Abs(factor);
  return band.Kind != UncertaintyKind.Normal
   ? Some(MeasureBand.Interval(band.Kind, lower, upper))
   : double.IsFinite(sigma) && sigma > 0.0
    ? Some(MeasureBand.Normal(lower, upper, sigma, band.CoverageFactor.IfNone(1.0)))
    : sigma == 0.0
     ? Option<MeasureBand>.None
     : Some(MeasureBand.Interval(UncertaintyKind.Interval, double.NegativeInfinity, double.PositiveInfinity));
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
 // The unit stays OPTIONAL through the render: a declared, resolvable token converts and names itself; an
 // undeclared or unresolvable one falls back to the SI magnitude beside whatever canonical unit the measure
 // actually holds — which is ABSENT for a tally, a consumer mint, or a dimension-anonymous product. A renderer
 // chooses its own blank; the scheme never invents one.
 public (double Value, Option<string> Unit) Render(MeasureValue measure) =>
  UnitFor(measure.Type)
   .Bind(unit => measure.In(unit).Map(value => (Value: value, Unit: Some(unit))))
   .IfNone((measure.Si, measure.CanonicalUnit));
}
```

## [04]-[IMPLEMENTATION_LAW]

- [UNITSNET_SI_COERCION]: `Of` resolves dynamic unit input through `Quantity.TryFrom`, then normalizes EVERY quantity through the one `SiUnit` election — declared `BaseUnitInfo` by default, an `SiElection` row where that base is prefixed or convention-scaled, and refusal by name where that row's `Option` is `None`. The registry's own `ToUnit(UnitSystem.SI)` cannot serve as that election and is the deleted form: it resolves through a `GetUnitInfosFor` walk demanding seven-axis `BaseUnits` equality against metadata most `UnitInfo` rows leave `Undefined` (`MassUnit.Kilogram` among them), so it throws `ArgumentException` for the majority of the roster — `Mass`, `Density`, `Torque`, `Frequency`, `HeatTransferCoefficient`, `ThermalConductivity`, `LinearDensity`, `ThermalResistance` included — and a `BaseUnits` subset-match fails for the same absent-metadata reason. An `IsDimensionless()` fork guarding it is deleted with it: it discriminates the WRONG axis, routing dimensional `Mass`/`Torque` onto the throwing leg while routing `Angle` onto a `BaseUnitInfo` that is `Degree`, persisting degrees under a page that promises radians. `Registry.SiNames` IS the election; `OfSi` stamps the same name and `In` uses `UnitConverter.TryConvert` for total egress over the admitted class.
- [QUANTITY_TYPE_DISCRIMINATOR]: `QuantityType` carries `QuantityInfo.Name`, while `Dimension` carries the seven-exponent physical signature. Both are required because torque and energy share a dimension, and angle, ratio, level, and count are dimensionless. Named QTO reads derive from `As(QuantityType)`, and cross-quantity products compose dimensions before an informed consumer re-stamps semantic type through `WithType`.
- [BAND_PROPAGATION]: Gaussian operands propagate by first-order partial derivatives and quadrature; exact operands are the zero-variance identity and their result carries NO band, because exactness is band absence and a minted zero-width band refuses its own re-admission through `WithUncertainty`. Bounds-only operands propagate through the four corners, and a divisor interval spanning zero widens to an unbounded interval. Non-finite propagated deviation also widens to an unbounded interval, while the nominal magnitude remains finite-gated. `Quantize` carries the band's bounds and σ onto the same tolerance grid as the magnitude, because the canonical writer takes the band as preimage; the coverage factor is declared policy on no physical axis and never quantizes.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
