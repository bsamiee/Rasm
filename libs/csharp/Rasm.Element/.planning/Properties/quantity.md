# [ELEMENT_QUANTITY]

The typed measured-scalar owner: one `MeasureValue` carrying its SI magnitude, its canonical unit string, a `Dimension` `[ComplexValueObject]` over the seven SI base-dimension exponents (the physical signature), and a `QuantityType` `[ValueObject<string>]` discriminator (the `UnitsNet` `QuantityInfo.Name`) that IS the quantity-type identity — admitted once through the `UnitsNet` quantity registry, coerced to its SI base, and read back through type-checked accessors. The discriminator is the NAME, not the dimension, because the seven-exponent vector is NOT injective over quantity types: `Torque` and `Energy` both reduce to `[L²·M·T⁻²]`, and `Angle` (rad), `Ratio`, and `SoundPressureLevel` (dB) all reduce to the zero vector shared with a bare `Count`. This REPLACES the six-value `QuantityKind` enum the migration source carried: a `MeasureValue` is no longer one of `Length`/`Area`/`Volume`/`Weight`/`Count`/`Time` but ANY measured quantity the `UnitsNet` registry names — so `ThermalTransmittance` (the `HeatTransferCoefficient` quantity, W·m⁻²·K⁻¹), `Pressure` (Pa), `Force` (N), `MassDensity` (the `Density` quantity, kg·m⁻³), `SoundPressureLevel` (the `Level` quantity, dB), `PlaneAngle` (the `Angle` quantity, rad), and `Temperature` (K) all admit with their real type, dimension, and unit, and the six former kinds become convenience accessors on the quantity-takeoff fold, never a closed enum the next `Pset_*` measure cannot extend. The owner composes the admitted `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → `ToUnit(UnitSystem.SI)` for a dimensional quantity, the as-constructed unit for a dimensionless one → `QuantityInfo.Name` + `Dimensions`) and the `Properties/property#PROPERTY_VALUE` `PropertyValue.Measure` carrier for the typed property arm; the SI reprojection rides the kernel `Op.Catch` funnel so the `UnitsNet` boundary throw never escapes the rail, and a non-finite or unresolvable measure rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page realizes a `QuantityType`-discriminated, `Dimension`-signed `MeasureValue` over the full `UnitsNet` quantity family — carrying the dimensioned cross-quantity algebra (`Multiply`/`Divide` over the composed `Dimension`, `Scale`, `Sum`) so a derived takeoff composes ON the carrier rather than re-deriving the dimensional product on bare doubles — never a six-case enum, never a dimension-only discriminator, and never a parallel checked-accessor family beside the one polymorphic `As(QuantityType)` read.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` `[ComplexValueObject]` seven-SI-exponent physical signature (canonical-dimension static rows, `SiSymbol` display, the `Multiply`/`Divide` derived-dimension algebra) and the `QuantityType` `[ValueObject<string>]` identity discriminator (the QTO-matchable rows, the open `Create` mint, the dimension-anonymous `OfDimension`).
- [02]-[MEASURE_VALUE]: the `MeasureValue` SI-coerced scalar carrier, the `UnitsNet` admission seam (the typed `Of(Enum)`, the abbreviation `Of(string)` external-source decode (the Rasm.Bim IFC/tabular/bSDD ingress convenience, NOT a cross-runtime wire shape), the SI-native `OfSi(QuantityType, Dimension, double)` + dimension-only `OfSi(Dimension, double)` factories, and the `OfCount` tally), the one polymorphic `As(QuantityType)` QTO read (the named `Length`/`Area`/`Volume`/`Weight`/`Time`/`Count` reads deriving from it), the unit-aware `In(Enum)` display egress (the `UnitsNet`-reconstructed reprojection a UI/wire consumer reads a stored measure back in `Millimeter`/`Foot`/`DegreeCelsius` through), the dimensioned `Multiply`/`Divide`/`Scale` cross-quantity algebra, the `Quantize` content-hash projection, and the same-type `Sum` reducer.

## [02]-[DIMENSION]

- Owner: `Dimension` the `[ComplexValueObject]` carrying the seven SI base-dimension exponents (`Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity`), the physical signature a `MeasureValue` coerces to, the `Multiply`/`Divide` algebra composes, and the `Projection/address#CONTENT_ADDRESS` writer hashes; `QuantityType` the `[ValueObject<string>]` quantity-type identity (the `UnitsNet` `QuantityInfo.Name`) a `MeasureValue` discriminates on. The canonical-dimension static rows (`LengthDim`/`AreaDim`/`VolumeDim`/`MassDim`/`DurationDim`/`Dimensionless` and the derived `PressureDim`/`DensityDim`/`ForceDim`/`ThermalTransmittance`) name the well-known dimensions; the QTO-matchable `QuantityType` rows (`Length`/`Area`/`Volume`/`Mass`/`Duration`/`Count`) name the well-known quantities a QTO accessor matches.
- Entry: `Dimension.Of(BaseDimensions dims)` lowers the `UnitsNet` 7-vector onto the value-object; `Dimension.Create(length, mass, time, current, temperature, amount, luminousIntensity)` is the generated factory; `Multiply`/`Divide` add/subtract the exponent vectors so a derived dimension composes (`ForceDim.Divide(AreaDim)` IS `PressureDim`) without a per-quantity row. `QuantityType.Create(name)` admits a quantity-type name — the `UnitsNet` `QuantityInfo.Name` at the `Of` boundary, or a consumer's domain name (`"SectionModulus"`, `"SecondMomentOfArea"`) where no `UnitsNet` quantity exists; `QuantityType.OfDimension(dimension)` derives the dimension-anonymous identity an `OfSi(Dimension, _)` value carries.
- Auto: the `[ComplexValueObject]` generates structural equality over the seven exponents so two `MeasureValue`s share a dimension iff their exponent vectors match, the `Of` projection reads the `UnitsNet` `BaseDimensions` integer exponents directly, and `Multiply`/`Divide` are pure exponent arithmetic mirroring the `BaseDimensions.Multiply`/`Divide` algebra so the seam never re-derives the dimensional product; the `[ValueObject<string>]` gives `QuantityType` ordinal value-equality so a `MeasureValue` discriminates on the registry name, and `OfDimension` encodes the 7-vector into a tilde-prefixed token that is dimension-unique yet NEVER equal to a QTO row, so an SI-native value admitted by dimension alone stays dimensioned-but-untyped and cannot false-match a QTO accessor.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`/`[ValueObject<string>]`), UnitsNet (`BaseDimensions` the 7-vector source, `QuantityInfo.Name` the quantity-type identity).
- Growth: a new well-known dimension is one static `Dimension` row (a `LuminousFlux`/`Frequency` row composed from exponents); a new QTO-matchable quantity is one `QuantityType` row; a consumer's domain quantity (`SectionModulus`, an analysis result scalar) mints through `QuantityType.Create(name)` with no seam row; a new measured quantity needs NO new dimension AND no new type — its `UnitsNet` `QuantityInfo` resolves both at admission; never a per-quantity dimension type and never a closed `QuantityKind` enum.
- Boundary: `QuantityType` is the ONE quantity-type discriminator and `Dimension` the physical signature — the six-case `QuantityKind` enum (the migration form) is the deleted form because it forced an out-of-family measure to a dimensionless `Count`, and the dimension-AS-discriminator form is the rejected form because distinct quantities share a dimension (`Torque`/`Energy` on `[L²·M·T⁻²]`; `Angle`/`Level`/`Ratio`/`Count` on the zero vector); the identity is the `UnitsNet` `QuantityInfo.Name` so any of the ~120 quantity structs admits with its real name, the named QTO rows the convenience anchors a QTO accessor matches, and the `Multiply`/`Divide` algebra composes a derived dimension from base ones rather than enumerating every product; the exponents come from the `UnitsNet` `BaseDimensions` and a hand-coded dimension table that drifts from the unit registry — or a name string parsed at a call site rather than minted through `QuantityType.Create` — is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Element;

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
 public static readonly Dimension ThermalTransmittance = Create(0, 1, -3, 0, -1, 0, 0);

 public static Dimension Of(BaseDimensions d) =>
 Create(d.Length, d.Mass, d.Time, d.Current, d.Temperature, d.Amount, d.LuminousIntensity);

 public Dimension Multiply(Dimension other) =>
 Create(Length + other.Length, Mass + other.Mass, Time + other.Time, Current + other.Current,
 Temperature + other.Temperature, Amount + other.Amount, LuminousIntensity + other.LuminousIntensity);

 public Dimension Divide(Dimension other) =>
 Create(Length - other.Length, Mass - other.Mass, Time - other.Time, Current - other.Current,
 Temperature - other.Temperature, Amount - other.Amount, LuminousIntensity - other.LuminousIntensity);

 // The SI coherent-unit symbol for the well-known rows — the display unit MeasureValue.OfSi stamps when a value is
 // already SI-native (no UnitsNet unit to read); identity is the QuantityType + the 7-vector, so this string is
 // display-only, never hashed (the Projection/address#CANONICAL_WRITER Measure primitive drops the unit string). The
 // symbol is a ONE-HOP frozen-table lookup keyed by the dimension's structural equality — the row data declared ONCE
 // beside the static rows it pairs with (DERIVED_LOGIC: a table, never a parallel this == X ternary chain re-stating
 // the rows), an unrostered dimension reading the "SI" default. The map keys on Dimension's [ComplexValueObject]
 // structural equality, so the seven-exponent vector resolves the symbol directly.
 static readonly FrozenDictionary<Dimension, string> SiSymbols = new Dictionary<Dimension, string> {
  [Dimensionless] = "", [LengthDim] = "m", [AreaDim] = "m2", [VolumeDim] = "m3", [MassDim] = "kg",
  [DurationDim] = "s", [ForceDim] = "N", [PressureDim] = "Pa", [DensityDim] = "kg/m3",
  [ThermalTransmittance] = "W/(m2.K)",
 }.ToFrozenDictionary();

 public string SiSymbol => SiSymbols.TryGetValue(this, out string? symbol) ? symbol : "SI";
}

[ValueObject<string>]
public sealed partial class QuantityType {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) => value = value.Trim();

 // The QTO-matchable identities — each .Value matches the UnitsNet QuantityInfo.Name for the geometric quantity (so a
 // value admitted through Of carries the registry's own name) plus two seam sentinels with no UnitsNet quantity.
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

- Owner: `MeasureValue` the SI-coerced measured-scalar carrier (`QuantityType Type` + `Dimension Dimension` + `double Si` + `string CanonicalUnit`) the `Properties/property#PROPERTY_VALUE` `Measure` arm, the `Composition/material#MATERIAL_PROPERTY` measured columns, and the `Assessment/assessment#ASSESSMENT_NODE` result scalars read; the one polymorphic `As(QuantityType)` QTO read (the named `Length`/`Area`/`Volume`/`Weight`/`Time`/`Count` reads deriving from it) replaces the former `QuantityKind` switch, and the `Multiply`/`Divide`/`Scale`/`Sum` algebra owns the dimensioned reductions a takeoff composes.
- Entry: `MeasureValue.Of(double value, Enum unit, Op key)` admits a value-plus-`UnitsNet`-unit, coercing to SI base once through the `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → `ToUnit(UnitSystem.SI)` for a dimensional quantity, the as-constructed unit for a dimensionless one), reading the `QuantityInfo.Name` into `Type` and the `Dimensions` 7-vector into `Dimension`, `Fin<T>` railing `ElementFault.ValueRejected` on a non-finite value, an unresolvable unit, or an SI reprojection the `UnitsNet` boundary throws; `MeasureValue.Of(double value, string unit, Op key)` resolves a unit abbreviation off the wire; `MeasureValue.OfSi(QuantityType type, Dimension dimension, double si)` admits an already-SI-base value with its named type (the `Rasm.Compute` result + the `SectionProperties` bake entry — a section modulus stamps `OfSi(QuantityType.Create("SectionModulus"), Dimension.VolumeDim, z)` so it never reads as a `Volume`); `MeasureValue.OfSi(Dimension dimension, double si)` the dimension-only convenience (dimensioned-but-untyped, read through `Si`); `MeasureValue.OfCount(double value, Op key)` admits a dimensionless count; `MeasureValue.Zero` the dimensionless additive identity. The reads and reductions: `As(QuantityType type)` the ONE quantity-type-checked SI read (`Some(Si)` iff the stored `Type` matches), `In(Enum unit)` the unit-aware DISPLAY egress (the stored SI scalar reprojected to a caller-chosen `UnitsNet` unit through the reconstructed `IQuantity`, `Some` for a registry quantity and `None` for a dimension-anonymous consumer-minted one the registry cannot rebuild), `Multiply`/`Divide` the dimensioned cross-quantity product/quotient over the composed `Dimension`, `Scale(factor)` the type-and-dimension-preserving scalar multiple, `Sum(measures, key)` the same-type reduction, `Quantize(tolerance)` the content-hash grid round.
- Auto: `Of` routes the raw value through `Quantity.TryFrom(value, unit)` to a boxed `IQuantity`, then `Coerce` reprojects a DIMENSIONAL quantity to SI base through `ToUnit(UnitSystem.SI)` and keeps a DIMENSIONLESS quantity (`Angle`/`Ratio`/`Level`/dB — no distinct SI unit) in its as-constructed unit, so `Admit` reads `(double)Value` plus `QuantityInfo.Name` plus `Dimension.Of(Dimensions)` and the persisted scalar is SI-base while the type is the registry's, never a hand-mapped kind; the one `As(QuantityType)` read tests the stored `Type` against the asked row and returns the `Si` as an `Option<double>` so a takeoff reads `measure.Area` (the named read deriving `As(QuantityType.Area)`) rather than a `Kind == QuantityKind.Area` branch, and a radian `Angle` or a dB `Level` never false-reads as a `Count`; `Multiply`/`Divide` compose `Dimension.Multiply`/`Divide` and mint the dimension-anonymous `QuantityType.OfDimension` over the SI-scalar product/quotient (a `Volume × Density` reads `MassDim` without the seam false-naming it `Mass`), so a geometry-true derived quantity composes ON the carrier; `Scale` preserves `Type` and `Dimension` (a basis-multiple of a measure is the same quantity); `Quantize(tolerance)` rounds `Si` to the `Header.Tolerance` grid (normalizing `-0.0`) for the `Projection/address#CONTENT_ADDRESS` canonical-byte projection so two measures within tolerance share a content key; `Sum(measures, key)` reduces a same-`Type` sequence by summing the SI scalars (already SI-base, so the dimensioned sum is the scalar sum), railing on a type mismatch.
- Receipt: a `MeasureValue` is the unit-checked evidence a takeoff, a property facet, and a cost join read — `measure.Si` is the SI magnitude, `measure.CanonicalUnit` the SI unit string, `measure.Type` the quantity-type identity, `measure.Dimension` the physical signature; a same-type element-set aggregate folds through `MeasureValue.Sum` and a derived quantity composes through `MeasureValue.Multiply`/`Divide`/`Scale` so the dimensioned reduction and the cross-quantity product are ONE typed algebra on the carrier, never a manual `double` accumulation or a hand-reconstructed result dimension, and the named QTO reads (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) are the one-hop convenience over `As(QuantityType)` the cost and quantity consumers compose.
- Packages: UnitsNet (`Quantity.TryFrom(double, Enum, out)` + `Quantity.TryFromUnitAbbreviation(culture, value, abbr, out)` the dynamic ingress, `Quantity.TryFrom(double, quantityName, unitName, out)` the SI-stored egress reconstruction `In` reads `IQuantity.As(Enum)` off, `IQuantity.ToUnit(UnitSystem.SI)`/`.Dimensions`/`.QuantityInfo.Name`/`.Value`/`.Unit` the coercion + identity surface, `BaseDimensions` the 7-vector + `IsDimensionless()`, the cross-quantity-operator algebra `MeasureValue.Multiply`/`Divide` mirrors onto the one carrier), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Option`/`Seq` + `Bind`/`MapFail`), `Rasm` (the kernel `Op` op-key + `Op.Catch` exception funnel).
- Growth: a new measured quantity needs NO seam edit — its `UnitsNet` `QuantityInfo` resolves type and dimension at `Of` admission; a new display unit a consumer renders in needs NO seam edit — it is a `UnitsNet` unit-enum member `In(unit)` already reprojects to; a new derived takeoff composes through the existing `Multiply`/`Divide`/`Scale`/`Sum` algebra (a `Force = Pressure × Area` is one `pressure.Multiply(area)`, never a new operation); a new named convenience read is one derived line over `As(QuantityType)`; never a per-quantity dimension type, a closed `QuantityKind` enum, or a parallel cross-quantity operation family.
- Boundary: `MeasureValue` NEVER carries a bare `double` quantity — the value coerces to SI base once at `Of` through the `UnitsNet` registry and the interior carries the SI scalar plus the typed `QuantityType`/`Dimension`, a free `double` measure field being the named defect per the `UnitsNet` ownership of dimensioned scalars; the discriminator is the `QuantityType` (the `QuantityInfo.Name`) so a `Torque` and an `Energy` that share a `Dimension` stay distinct and a dimensionless `Angle`/`Level`/`Ratio` never collapses onto `Count` — the dimension-as-discriminator form is the deleted form, and the `QuantityKind` six-case enum that degraded an out-of-family measure to `Count` is doubly deleted; the SI coercion rides the `UnitsNet` `Quantity.TryFrom`/`ToUnit(UnitSystem.SI)` registry and a stringly-keyed unit switch or an ad-hoc conversion factor is the deleted form; `ToUnit(UnitSystem.SI)` is the platform-forced boundary throw (`InvalidCastException`/`NotImplementedException` when a quantity has no SI unit), so the reprojection rides the kernel `Op.Catch` funnel and a residual throw rails `ElementFault.ValueRejected` — a foreign `Exception` never crosses the seam signature; a DIMENSIONLESS quantity has no SI reprojection and admits in its own unit (dB stays dB, rad stays rad), so `SoundPressureLevel`/`PlaneAngle` admit rather than throw; aggregation flows through `MeasureValue.Sum` (an SI-base scalar sum guarded by `Type` equality, stricter than a dimension guard) and a derived quantity through `MeasureValue.Multiply`/`Divide` (the SI product/quotient over the composed `Dimension`, the result dimension-anonymous so the seam never false-names a `Volume × Density` as `Mass` — the consumer that KNOWS the identity re-stamps through `OfSi(QuantityType.Mass, …)`), never an unwrap-compute-rewrap or a hand-reconstructed result dimension, and a cross-type sum rails `ElementFault.ValueRejected`; the type-and-dimension-preserving `Scale(factor)` is the basis-aware multiple a cost/environmental ply scaling composes, never a `this with { Si = Si * factor }` mutation at the call site; the dimension-only `OfSi(Dimension, double)` carries the dimension-anonymous `QuantityType` so a bare SI-native scalar is dimensioned-but-untyped and a QTO read fires ONLY for a measure admitted with an explicit quantity-type; the named QTO reads (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) DERIVE from the one `As(QuantityType)` body (DERIVED_LOGIC — a six-member parallel checked family is the deleted form, the convenience names one-hop over the polymorphic read); the interior carries SI ALONE while EGRESS is unit-parameterized — `In(Enum unit)` reconstructs the `UnitsNet` `IQuantity` from the stored `(Type.Value, Si, CanonicalUnit)` through the `Quantity.TryFrom(value, quantityName, unitName, out)` registry and reads `IQuantity.As(unit)` so a UI/wire consumer renders a stored U-value in `WattPerSquareMeterKelvin`, a length in `Millimeter`/`Foot`, or a temperature in `DegreeCelsius` WITHOUT the call site re-deriving a conversion factor (the deleted `si.Si * 1000.0` form) — `None` for a dimension-anonymous `OfSi(Dimension,_)` measure (a section modulus, an analysis-result scalar) the registry cannot rebuild, so the egress is total over the ~120 registry quantities and honestly absent for a consumer-minted one; `Quantize` is the ONLY rounding, applied at the content-hash boundary against `Header.Tolerance`, so the interior `Si` stays full-precision and the canonical key is tolerance-stable.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MeasureValue(QuantityType Type, Dimension Dimension, double Si, string CanonicalUnit) {
 // The dimensionless additive identity — Scalar-typed (a dimensionless untyped scalar), distinct from an explicit Count.
 public static readonly MeasureValue Zero = new(QuantityType.Scalar, Dimension.Dimensionless, 0.0, "");

 public static Fin<MeasureValue> Of(double value, Enum unit, Op key) =>
 !double.IsFinite(value)
 ? ElementFault.ValueRejected(key, $"<measure-non-finite:{value:R}>")
 : Quantity.TryFrom(value, unit, out IQuantity? quantity) && quantity is { } q
 ? Coerce(q, key)
 : ElementFault.ValueRejected(key, $"<measure-unit-unresolved:{unit}>");

 // The abbreviation-decode overload: a value plus a UnitsNet unit ABBREVIATION ({value, unit:string}) resolves through
 // the UnitsNet abbreviation registry under the INVARIANT culture — the entry the Rasm.Bim projector decodes an EXTERNAL
 // source through (an IFC/tabular/bSDD value that arrives as a unit string "mm"/"kN" rather than a typed IfcUnit), NOT a
 // cross-runtime Python/TypeScript wire shape (the peers decode the C#-minted SI-coerced graph payloads and reproduce
 // the content-byte parity, minting no quantity vocabulary of their own — the wire carries SI magnitude + Dimension,
 // never a {value, unit:string} the peer would re-mint). An ABBREVIATION decode MUST NOT resolve under the server's
 // ambient culture (the satellite fr-CA/ru-RU/zh-CN/nb-NO abbreviation tables would fork the decode), so the culture is
 // PINNED to invariant, then SI-coerced once at this admission.
 public static Fin<MeasureValue> Of(double value, string unit, Op key) =>
 !double.IsFinite(value)
 ? ElementFault.ValueRejected(key, $"<measure-non-finite:{value:R}>")
 : Quantity.TryFromUnitAbbreviation(System.Globalization.CultureInfo.InvariantCulture, value, unit, out IQuantity? quantity) && quantity is { } q
 ? Coerce(q, key)
 : ElementFault.ValueRejected(key, $"<measure-unit-abbreviation-unresolved:{unit}>");

 // The SI-native factory: an already-coerced SI magnitude plus its named QuantityType and Dimension (no UnitsNet unit
 // needed) — the entry a Rasm.Compute analysis writes a computed result (an assembly U-value, a section modulus) through,
 // and the entry a SectionProperties bake stamps its m^2/m^3/m^4 columns through. Identity is Type + Dimension; unit is display-only.
 public static MeasureValue OfSi(QuantityType type, Dimension dimension, double si) => new(type, dimension, si, dimension.SiSymbol);

 // The dimension-only convenience: a bare SI magnitude whose physical Dimension is known but whose semantic type is not —
 // it carries the dimension-anonymous QuantityType (never a QTO row), so it is dimensioned-but-untyped and reads through Si.
 public static MeasureValue OfSi(Dimension dimension, double si) => OfSi(QuantityType.OfDimension(dimension), dimension, si);

 public static Fin<MeasureValue> OfCount(double value, Op key) =>
 double.IsFinite(value)
 ? Fin.Succ(new MeasureValue(QuantityType.Count, Dimension.Dimensionless, value, ""))
 : ElementFault.ValueRejected(key, $"<count-non-finite:{value:R}>");

 // The UnitsNet admission seam: a dimensionless quantity (Angle/Ratio/Level/dB) has NO distinct SI reprojection — the
 // as-constructed unit IS canonical; a dimensional quantity reprojects to its SI base once. ToUnit(UnitSystem.SI) is the
 // platform-forced boundary throw (no SI unit maps), so the reprojection rides the kernel Op.Catch funnel and a residual
 // throw rails ValueRejected — never an exception across a seam signature.
 static Fin<MeasureValue> Coerce(IQuantity quantity, Op key) =>
 (quantity.Dimensions.IsDimensionless()
 ? Fin.Succ(quantity)
 : key.Catch(() => Fin.Succ(quantity.ToUnit(UnitSystem.SI)))
 .MapFail(_ => ElementFault.ValueRejected(key, $"<measure-si-unavailable:{quantity.QuantityInfo.Name}>")))
 .Bind(si => Admit(si, key));

 // The identity is the UnitsNet QuantityInfo.Name (Torque vs Energy, Angle vs Level vs Ratio — distinct names that SHARE
 // a Dimension), the Dimension the physical 7-vector, the Si the SI-base magnitude, the unit the SI unit token.
 static Fin<MeasureValue> Admit(IQuantity si, Op key) =>
 double.IsFinite((double)si.Value)
 ? Fin.Succ(new MeasureValue(QuantityType.Create(si.QuantityInfo.Name), Dimension.Of(si.Dimensions), (double)si.Value, si.Unit.ToString()))
 : ElementFault.ValueRejected(key, $"<measure-si-non-finite:{si.Unit}>");

 // The ONE polymorphic QTO read: the SI magnitude WHEN the stored Type matches the asked quantity-type, else None — so
 // a takeoff reads measure.As(QuantityType.Area) rather than a Kind == QuantityKind.Area branch, a radian Angle or a dB
 // Level never reads as a Count (it matches no geometric row), and an out-of-family measure (Pressure, ThermalTransmittance)
 // is read through Si/Dimension directly. This is the named-accessor family's ONE body (DERIVED_LOGIC + SEMANTIC_NAMING:
 // one polymorphic read discriminating on the QuantityType value, never six sibling Length/Area/Volume/Weight/Time/Count
 // members re-stating the rows the closed enum it replaced enumerated). As is the read-projection verb (the static Of
 // is the admission factory — distinct grammatical roles, distinct names), mirroring the UnitsNet IQuantity.As value read.
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

 // The unit-aware DISPLAY egress: reproject the stored SI scalar into a caller-chosen UnitsNet unit so a UI/wire
 // consumer reads a U-value in WattPerSquareMeterKelvin, a length in Millimeter/Foot, or a temperature in DegreeCelsius
 // WITHOUT re-deriving a conversion factor at the call site (the deleted `Si * 1000.0` form). The stored
 // (Type.Value, Si, CanonicalUnit) reconstructs the IQuantity through the UnitsNet registry — Quantity.TryFrom by the
 // registry NAME plus the SI unit token CanonicalUnit holds — and IQuantity.As(unit) reprojects; the INVARIANT culture
 // pins the unit-name resolution so a cross-runtime egress never forks on the server's ambient culture. None for a
 // dimension-anonymous OfSi(Dimension, _) measure (a SectionModulus, an analysis-result scalar) the registry cannot
 // rebuild — the read is total over the ~120 registry quantities and honestly absent for a consumer-minted one, never a
 // throw across the read. The ingress (Of) and the egress (In) are the two unit-boundary verbs; the interior is SI alone.
 public Option<double> In(Enum unit) =>
  Quantity.TryFrom(Si, Type.Value, CanonicalUnit, out IQuantity? q) && q is { } quantity
   ? Some(quantity.As(unit))
   : None;

 // Tolerance quantization for the content-hash boundary: round the SI magnitude to the tolerance grid so two measures
 // within Header.Tolerance project to the same canonical bytes; + 0.0 normalizes a -0.0 result to +0.0 (one grid zero).
 public MeasureValue Quantize(double tolerance) =>
 tolerance > 0.0 ? this with { Si = Math.Round(Si / tolerance, MidpointRounding.AwayFromZero) * tolerance + 0.0 } : this;

 // Same-quantity-type SI-scalar reduction; the Type guard is stricter than a dimension guard — a Torque and an Energy
 // share a Dimension but never sum, an Angle and a Count share the zero vector but never sum. LanguageExt v5 `Seq.Head`
 // is `Option<A>`, so the head reads through `Match` — never a direct `.Type`/`with` on the Option.
 public static Fin<MeasureValue> Sum(Seq<MeasureValue> measures, Op key) =>
 measures.Head.Match(
 None: () => Fin.Succ(Zero),
 Some: head => measures.Tail.Exists(m => m.Type != head.Type)
 ? ElementFault.ValueRejected(key, "<measure-sum-type-mismatch>")
 : Fin.Succ(head with { Si = measures.Sum(static m => m.Si) }));

 // The dimensioned cross-quantity algebra: the SI product/quotient of two measures is the SI-scalar product/quotient
 // over the COMPOSED Dimension (the mirror of UnitsNet's `Mass * Acceleration -> Force`, `Force / Area -> Pressure`,
 // `Area * Length -> Volume` cross-quantity operators, collapsed onto the one carrier), so a geometry-true takeoff
 // composes its derived quantity ON the seam rather than dropping to a bare double and re-deriving the dimensional
 // product by hand — the `Rasm.Bim` `QuantityDerivation.Derive` `NetWeight = NetVolume × Density` (VolumeDim × DensityDim
 // IS MassDim) is one `volume.Multiply(density)`, the result's Dimension composed and not hand-asserted. The product of
 // two NAMED quantities has a derived Dimension but NO registry name (the 7-vector is not injective — a VolumeDim×DensityDim
 // result is MassDim yet the seam cannot, without the registry, distinguish a "Mass" from another [M] quantity), so it
 // carries the dimension-anonymous `QuantityType.OfDimension` — type-honest (never a false-named row a QTO accessor
 // false-matches), the consumer naming it through `OfSi(QuantityType.Mass, result.Dimension, result.Si)` only where it
 // KNOWS the semantic identity (the `NetWeight` row does). The SI scalar product is the SI product (both operands SI-base),
 // so no re-coercion; identity is the composed Dimension, the display unit dropped (it has no coherent symbol).
 public MeasureValue Multiply(MeasureValue other) {
  Dimension product = this.Dimension.Multiply(other.Dimension);
  return new MeasureValue(QuantityType.OfDimension(product), product, Si * other.Si, product.SiSymbol);
 }

 public MeasureValue Divide(MeasureValue other) {
  Dimension quotient = this.Dimension.Divide(other.Dimension);
  return new MeasureValue(QuantityType.OfDimension(quotient), quotient, Si / other.Si, quotient.SiSymbol);
 }

 // Scalar scaling preserves the QuantityType AND the Dimension — a basis-matching multiple of a measure is the SAME
 // quantity (10 × one m³ is m³), the operation the `Composition/material#MATERIAL_PROPERTY` `AggregateEnvironmental`/cost
 // folds compose to scale a per-unit EPD/cost column by the basis-matching element quantity, never a `this with { Si =
 // Si * factor }` mutation-by-hand that re-spells the carrier at the call site.
 public MeasureValue Scale(double factor) => this with { Si = Si * factor };
}
```

## [04]-[RESEARCH]

- [UNITSNET_SI_COERCION]: the `UnitsNet` `Quantity.TryFrom(double, Enum, out IQuantity?)` dynamic factory, the `IQuantity.ToUnit(UnitSystem.SI)` SI-base reprojection, the `IQuantity.Value`/`Unit` reads, the `IQuantity.Dimensions` `BaseDimensions` 7-vector, and the `IQuantity.QuantityInfo` metadata are catalogued in `.api/api-unitsnet.md`; the seam coerces every DIMENSIONAL value once at `Of` through this registry so the persisted scalar is SI-base, and keeps a DIMENSIONLESS quantity (`Angle`/`Ratio`/`Level`) in its as-constructed unit because `ToUnit(UnitSystem.SI)` THROWS for a quantity with no SI unit (the documented `InvalidCastException`/`NotImplementedException` boundary) — the reprojection therefore rides the kernel `Op.Catch` funnel, so `SoundPressureLevel` (dB) and `PlaneAngle` (rad) admit through the dimensionless path rather than escaping the `Fin<T>` rail as an exception.
- [QUANTITY_TYPE_DISCRIMINATOR]: the `UnitsNet` v5 removal of the `QuantityType` ENUM does not remove the quantity-type identity — `IQuantity.QuantityInfo.Name` (the `Quantity.ByName` registry key) is the stable string identity, so the `Dimension` 7-vector is the physical SIGNATURE and the `QuantityType` `[ValueObject<string>]` (the name) is the DISCRIMINATOR; this is load-bearing because the seven-exponent vector is not injective over quantity types — `Torque` and `Energy` both resolve to `[L²·M·T⁻²]`, and `Angle`, `Ratio`, `SoundPressureLevel` (the `Level` quantity), and a bare `Count` all resolve to the zero vector, so a dimension-only discriminator would read a radian angle as a count and could not separate a torque from an energy. Replacing the migration `QuantityKind` six-case enum with this pair admits the `Pset_*` measures the old enum forced to `Count` — `Pset_WallCommon.ThermalTransmittance` (the `HeatTransferCoefficient` quantity, W·m⁻²·K⁻¹), `Pset_*` `Pressure`/`Force`/`Density` (`IfcMassDensityMeasure`), `SoundPressureLevel` (dB), `PlaneAngle` (rad), `Temperature` (K) — each carrying its real `QuantityInfo.Name`, dimension, and unit; the named QTO reads (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) DERIVE one-hop from the single polymorphic `As(QuantityType)` body — the convenience surface a `Qto_*BaseQuantities` consumer reads `measure.Volume` without a kind switch, never a six-member parallel checked family re-stating the rows the abolished enum carried — the `Rasm.Bim` `QuantityDerivation.Derive` base-quantity fold composing this carrier (admitting each base quantity through `Of(value, AreaUnit.SquareMeter, key)` or the explicit `OfSi(QuantityType.Area, …)` so the geometry-true takeoff is type-tagged at ingress, never inferred from an ambiguous dimension) AND composing the dimensioned `MeasureValue.Multiply` for its derived rows — `NetWeight = NetVolume.Multiply(massDensity)` (`VolumeDim × DensityDim` composes `MassDim` on the carrier, re-stamped `OfSi(QuantityType.Mass, …)` where the fold KNOWS the semantic identity), the seam owning the cross-quantity product the consumer otherwise re-derives on bare `.Si` doubles.
