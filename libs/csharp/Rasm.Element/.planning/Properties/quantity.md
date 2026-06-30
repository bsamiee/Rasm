# [ELEMENT_QUANTITY]

The typed measured-scalar owner: one `MeasureValue` carrying its SI magnitude, its canonical unit string, a `Dimension` `[ComplexValueObject]` over the seven SI base-dimension exponents (the physical signature), and a `QuantityType` `[ValueObject<string>]` discriminator (the `UnitsNet` `QuantityInfo.Name`) that IS the quantity-type identity — admitted once through the `UnitsNet` quantity registry, coerced to its SI base, and read back through type-checked accessors. The discriminator is the NAME, not the dimension, because the seven-exponent vector is NOT injective over quantity types: `Torque` and `Energy` both reduce to `[L²·M·T⁻²]`, and `Angle` (rad), `Ratio`, and `SoundPressureLevel` (dB) all reduce to the zero vector shared with a bare `Count`. This REPLACES the six-value `QuantityKind` enum the migration source carried: a `MeasureValue` is no longer one of `Length`/`Area`/`Volume`/`Weight`/`Count`/`Time` but ANY measured quantity the `UnitsNet` registry names — so `ThermalTransmittance` (the `HeatTransferCoefficient` quantity, W·m⁻²·K⁻¹), `Pressure` (Pa), `Force` (N), `MassDensity` (the `Density` quantity, kg·m⁻³), `SoundPressureLevel` (the `Level` quantity, dB), `PlaneAngle` (the `Angle` quantity, rad), and `Temperature` (K) all admit with their real type, dimension, and unit, and the six former kinds become convenience accessors on the quantity-takeoff fold, never a closed enum the next `Pset_*` measure cannot extend. The owner composes the admitted `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → `ToUnit(UnitSystem.SI)` for a dimensional quantity, the as-constructed unit for a dimensionless one → `QuantityInfo.Name` + `Dimensions`) and the `Properties/property#PROPERTY_VALUE` `PropertyValue.Measure` carrier for the typed property arm; the SI reprojection rides the kernel `Op.Catch` funnel so the `UnitsNet` boundary throw never escapes the rail, and a non-finite or unresolvable measure rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page realizes a `QuantityType`-discriminated, `Dimension`-signed `MeasureValue` over the full `UnitsNet` quantity family, never a six-case enum and never a dimension-only discriminator.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` `[ComplexValueObject]` seven-SI-exponent physical signature (canonical-dimension static rows, `SiSymbol` display, the `Multiply`/`Divide` derived-dimension algebra) and the `QuantityType` `[ValueObject<string>]` identity discriminator (the QTO-matchable rows, the open `Create` mint, the dimension-anonymous `OfDimension`).
- [02]-[MEASURE_VALUE]: the `MeasureValue` SI-coerced scalar carrier, the `UnitsNet` admission seam (the typed `Of(Enum)`, the abbreviation `Of(string)` wire-decode, the SI-native `OfSi(QuantityType, Dimension, double)` + dimension-only `OfSi(Dimension, double)` factories, and the `OfCount` tally), the type-checked QTO accessors, the `Quantize` content-hash projection, and the same-type `Sum` reducer.

## [02]-[DIMENSION]

- Owner: `Dimension` the `[ComplexValueObject]` carrying the seven SI base-dimension exponents (`Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity`), the physical signature a `MeasureValue` coerces to, the `Multiply`/`Divide` algebra composes, and the `Projection/address#CONTENT_ADDRESS` writer hashes; `QuantityType` the `[ValueObject<string>]` quantity-type identity (the `UnitsNet` `QuantityInfo.Name`) a `MeasureValue` discriminates on. The canonical-dimension static rows (`LengthDim`/`AreaDim`/`VolumeDim`/`MassDim`/`DurationDim`/`Dimensionless` and the derived `PressureDim`/`DensityDim`/`ForceDim`/`ThermalTransmittance`) name the well-known dimensions; the QTO-matchable `QuantityType` rows (`Length`/`Area`/`Volume`/`Mass`/`Duration`/`Count`) name the well-known quantities a QTO accessor matches.
- Entry: `Dimension.Of(BaseDimensions dims)` lowers the `UnitsNet` 7-vector onto the value-object; `Dimension.Create(length, mass, time, current, temperature, amount, luminousIntensity)` is the generated factory; `Multiply`/`Divide` add/subtract the exponent vectors so a derived dimension composes (`ForceDim.Divide(AreaDim)` IS `PressureDim`) without a per-quantity row. `QuantityType.Create(name)` admits a quantity-type name — the `UnitsNet` `QuantityInfo.Name` at the `Of` boundary, or a consumer's domain name (`"SectionModulus"`, `"SecondMomentOfArea"`) where no `UnitsNet` quantity exists; `QuantityType.OfDimension(dimension)` derives the dimension-anonymous identity an `OfSi(Dimension, _)` value carries.
- Auto: the `[ComplexValueObject]` generates structural equality over the seven exponents so two `MeasureValue`s share a dimension iff their exponent vectors match, the `Of` projection reads the `UnitsNet` `BaseDimensions` integer exponents directly, and `Multiply`/`Divide` are pure exponent arithmetic mirroring the `BaseDimensions.Multiply`/`Divide` algebra so the seam never re-derives the dimensional product; the `[ValueObject<string>]` gives `QuantityType` ordinal value-equality so a `MeasureValue` discriminates on the registry name, and `OfDimension` encodes the 7-vector into a tilde-prefixed token that is dimension-unique yet NEVER equal to a QTO row, so an SI-native value admitted by dimension alone stays dimensioned-but-untyped and cannot false-match a QTO accessor.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`/`[ValueObject<string>]`), UnitsNet (`BaseDimensions` the 7-vector source, `QuantityInfo.Name` the quantity-type identity).
- Growth: a new well-known dimension is one static `Dimension` row (a `LuminousFlux`/`Frequency` row composed from exponents); a new QTO-matchable quantity is one `QuantityType` row; a consumer's domain quantity (`SectionModulus`, an analysis result scalar) mints through `QuantityType.Create(name)` with no seam row; a new measured quantity needs NO new dimension AND no new type — its `UnitsNet` `QuantityInfo` resolves both at admission; never a per-quantity dimension type and never a closed `QuantityKind` enum.
- Boundary: `QuantityType` is the ONE quantity-type discriminator and `Dimension` the physical signature — the six-case `QuantityKind` enum (the migration form) is the deleted form because it forced an out-of-family measure to a dimensionless `Count`, and the dimension-AS-discriminator form is the rejected form because distinct quantities share a dimension (`Torque`/`Energy` on `[L²·M·T⁻²]`; `Angle`/`Level`/`Ratio`/`Count` on the zero vector); the identity is the `UnitsNet` `QuantityInfo.Name` so any of the ~120 quantity structs admits with its real name, the named QTO rows the convenience anchors a QTO accessor matches, and the `Multiply`/`Divide` algebra composes a derived dimension from base ones rather than enumerating every product; the exponents come from the `UnitsNet` `BaseDimensions` and a hand-coded dimension table that drifts from the unit registry — or a name string parsed at a call site rather than minted through `QuantityType.Create` — is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
 // already SI-native (no UnitsNet unit to read); identity is the QuantityType + the 7-vector, so this string is display-only, never hashed.
 public string SiSymbol =>
 this == Dimensionless ? "" : this == LengthDim ? "m" : this == AreaDim ? "m2" : this == VolumeDim ? "m3"
 : this == MassDim ? "kg" : this == DurationDim ? "s" : this == ForceDim ? "N" : this == PressureDim ? "Pa"
 : this == DensityDim ? "kg/m3" : this == ThermalTransmittance ? "W/(m2.K)" : "SI";
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

- Owner: `MeasureValue` the SI-coerced measured-scalar carrier (`QuantityType Type` + `Dimension Dimension` + `double Si` + `string CanonicalUnit`) the `Properties/property#PROPERTY_VALUE` `Measure` arm, the `Composition/material#MATERIAL_PROPERTY` measured columns, and the `Assessment/assessment#ASSESSMENT_NODE` result scalars read; the type-checked QTO accessors (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) replace the former `QuantityKind` switch.
- Entry: `MeasureValue.Of(double value, Enum unit, Op key)` admits a value-plus-`UnitsNet`-unit, coercing to SI base once through the `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → `ToUnit(UnitSystem.SI)` for a dimensional quantity, the as-constructed unit for a dimensionless one), reading the `QuantityInfo.Name` into `Type` and the `Dimensions` 7-vector into `Dimension`, `Fin<T>` railing `ElementFault.ValueRejected` on a non-finite value, an unresolvable unit, or an SI reprojection the `UnitsNet` boundary throws; `MeasureValue.Of(double value, string unit, Op key)` resolves a unit abbreviation off the wire; `MeasureValue.OfSi(QuantityType type, Dimension dimension, double si)` admits an already-SI-base value with its named type (the `Rasm.Compute` result + the `SectionProperties` bake entry — a section modulus stamps `OfSi(QuantityType.Create("SectionModulus"), Dimension.VolumeDim, z)` so it never reads as a `Volume`); `MeasureValue.OfSi(Dimension dimension, double si)` the dimension-only convenience (dimensioned-but-untyped, read through `Si`); `MeasureValue.OfCount(double value, Op key)` admits a dimensionless count; `MeasureValue.Zero` the dimensionless additive identity.
- Auto: `Of` routes the raw value through `Quantity.TryFrom(value, unit)` to a boxed `IQuantity`, then `Coerce` reprojects a DIMENSIONAL quantity to SI base through `ToUnit(UnitSystem.SI)` and keeps a DIMENSIONLESS quantity (`Angle`/`Ratio`/`Level`/dB — no distinct SI unit) in its as-constructed unit, so `Admit` reads `(double)Value` plus `QuantityInfo.Name` plus `Dimension.Of(Dimensions)` and the persisted scalar is SI-base while the type is the registry's, never a hand-mapped kind; the QTO accessors test the stored `Type` against the canonical rows (`Area` iff `Type == QuantityType.Area`) and return the `Si` as an `Option<double>` so a takeoff reads `measure.Area` rather than a `Kind == QuantityKind.Area` branch, and a radian `Angle` or a dB `Level` never false-reads as a `Count`; `Quantize(tolerance)` rounds `Si` to the `Header.Tolerance` grid (normalizing `-0.0`) for the `Projection/address#CONTENT_ADDRESS` canonical-byte projection so two measures within tolerance share a content key; `Sum(measures, key)` reduces a same-`Type` sequence by summing the SI scalars (already SI-base, so the dimensioned sum is the scalar sum), railing on a type mismatch.
- Receipt: a `MeasureValue` is the unit-checked evidence a takeoff, a property facet, and a cost join read — `measure.Si` is the SI magnitude, `measure.CanonicalUnit` the SI unit string, `measure.Type` the quantity-type identity, `measure.Dimension` the physical signature; a same-type element-set aggregate folds through `MeasureValue.Sum` so the dimensioned reduction is one typed fold, never a manual `double` accumulation, and the QTO accessor names (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) are the convenience reads the cost and quantity consumers compose.
- Packages: UnitsNet (`Quantity`/`IQuantity`/`UnitSystem.SI`/`BaseDimensions`/`QuantityInfo` the SI-coercion + identity registry), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Option`/`Seq` + `Bind`/`MapFail`), `Rasm` (the kernel `Op` op-key + `Op.Catch` exception funnel).
- Boundary: `MeasureValue` NEVER carries a bare `double` quantity — the value coerces to SI base once at `Of` through the `UnitsNet` registry and the interior carries the SI scalar plus the typed `QuantityType`/`Dimension`, a free `double` measure field being the named defect per the `UnitsNet` ownership of dimensioned scalars; the discriminator is the `QuantityType` (the `QuantityInfo.Name`) so a `Torque` and an `Energy` that share a `Dimension` stay distinct and a dimensionless `Angle`/`Level`/`Ratio` never collapses onto `Count` — the dimension-as-discriminator form is the deleted form, and the `QuantityKind` six-case enum that degraded an out-of-family measure to `Count` is doubly deleted; the SI coercion rides the `UnitsNet` `Quantity.TryFrom`/`ToUnit(UnitSystem.SI)` registry and a stringly-keyed unit switch or an ad-hoc conversion factor is the deleted form; `ToUnit(UnitSystem.SI)` is the platform-forced boundary throw (`InvalidCastException`/`NotImplementedException` when a quantity has no SI unit), so the reprojection rides the kernel `Op.Catch` funnel and a residual throw rails `ElementFault.ValueRejected` — a foreign `Exception` never crosses the seam signature; a DIMENSIONLESS quantity has no SI reprojection and admits in its own unit (dB stays dB, rad stays rad), so `SoundPressureLevel`/`PlaneAngle` admit rather than throw; aggregation flows through `MeasureValue.Sum` (an SI-base scalar sum guarded by `Type` equality, stricter than a dimension guard), never an unwrap-sum-rewrap, and a cross-type sum rails `ElementFault.ValueRejected`; the dimension-only `OfSi(Dimension, double)` carries the dimension-anonymous `QuantityType` so a bare SI-native scalar is dimensioned-but-untyped and a QTO accessor fires ONLY for a measure admitted with an explicit quantity-type; `Quantize` is the ONLY rounding, applied at the content-hash boundary against `Header.Tolerance`, so the interior `Si` stays full-precision and the canonical key is tolerance-stable.

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

 // The wire-decode overload: a value plus a UnitsNet unit ABBREVIATION (the {value, unit:string} wire shape the
 // Python/TypeScript peers send) resolves through the UnitsNet abbreviation registry under the INVARIANT culture —
 // a cross-runtime wire boundary MUST NOT resolve a peer's abbreviation under the server's ambient culture (the
 // satellite fr-CA/ru-RU/zh-CN/nb-NO abbreviation tables would fork the decode), so the culture is PINNED, then SI-coerced.
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

 // QTO accessors: the six former QuantityKind cases as quantity-type-checked reads on the takeoff fold, never a closed
 // enum — they match the NAMED QuantityType (so a radian Angle or a dB Level never reads as a Count), and an out-of-family
 // measure (Pressure, ThermalTransmittance) simply has no named accessor and is read through Si/Dimension directly.
 public Option<double> Length => Type == QuantityType.Length ? Some(Si) : None;
 public Option<double> Area => Type == QuantityType.Area ? Some(Si) : None;
 public Option<double> Volume => Type == QuantityType.Volume ? Some(Si) : None;
 public Option<double> Weight => Type == QuantityType.Mass ? Some(Si) : None;
 public Option<double> Time => Type == QuantityType.Duration ? Some(Si) : None;
 public Option<double> Count => Type == QuantityType.Count ? Some(Si) : None;

 // Tolerance quantization for the content-hash boundary: round the SI magnitude to the tolerance grid so two measures
 // within Header.Tolerance project to the same canonical bytes; + 0.0 normalizes a -0.0 result to +0.0 (one grid zero).
 public MeasureValue Quantize(double tolerance) =>
 tolerance > 0.0 ? this with { Si = Math.Round(Si / tolerance, MidpointRounding.AwayFromZero) * tolerance + 0.0 } : this;

 // Same-quantity-type SI-scalar reduction; the Type guard is stricter than a dimension guard — a Torque and an Energy
 // share a Dimension but never sum, an Angle and a Count share the zero vector but never sum.
 public static Fin<MeasureValue> Sum(Seq<MeasureValue> measures, Op key) =>
 measures.IsEmpty
 ? Fin.Succ(Zero)
 : measures.Tail.Exists(m => m.Type != measures.Head.Type)
 ? ElementFault.ValueRejected(key, "<measure-sum-type-mismatch>")
 : Fin.Succ(measures.Head with { Si = measures.Sum(static m => m.Si) });
}
```

## [04]-[RESEARCH]

- [UNITSNET_SI_COERCION]: the `UnitsNet` `Quantity.TryFrom(double, Enum, out IQuantity?)` dynamic factory, the `IQuantity.ToUnit(UnitSystem.SI)` SI-base reprojection, the `IQuantity.Value`/`Unit` reads, the `IQuantity.Dimensions` `BaseDimensions` 7-vector, and the `IQuantity.QuantityInfo` metadata are catalogued in `.api/api-unitsnet.md`; the seam coerces every DIMENSIONAL value once at `Of` through this registry so the persisted scalar is SI-base, and keeps a DIMENSIONLESS quantity (`Angle`/`Ratio`/`Level`) in its as-constructed unit because `ToUnit(UnitSystem.SI)` THROWS for a quantity with no SI unit (the documented `InvalidCastException`/`NotImplementedException` boundary) — the reprojection therefore rides the kernel `Op.Catch` funnel, so `SoundPressureLevel` (dB) and `PlaneAngle` (rad) admit through the dimensionless path rather than escaping the `Fin<T>` rail as an exception.
- [QUANTITY_TYPE_DISCRIMINATOR]: the `UnitsNet` v5 removal of the `QuantityType` ENUM does not remove the quantity-type identity — `IQuantity.QuantityInfo.Name` (the `Quantity.ByName` registry key) is the stable string identity, so the `Dimension` 7-vector is the physical SIGNATURE and the `QuantityType` `[ValueObject<string>]` (the name) is the DISCRIMINATOR; this is load-bearing because the seven-exponent vector is not injective over quantity types — `Torque` and `Energy` both resolve to `[L²·M·T⁻²]`, and `Angle`, `Ratio`, `SoundPressureLevel` (the `Level` quantity), and a bare `Count` all resolve to the zero vector, so a dimension-only discriminator would read a radian angle as a count and could not separate a torque from an energy. Replacing the migration `QuantityKind` six-case enum with this pair admits the `Pset_*` measures the old enum forced to `Count` — `Pset_WallCommon.ThermalTransmittance` (the `HeatTransferCoefficient` quantity, W·m⁻²·K⁻¹), `Pset_*` `Pressure`/`Force`/`Density` (`IfcMassDensityMeasure`), `SoundPressureLevel` (dB), `PlaneAngle` (rad), `Temperature` (K) — each carrying its real `QuantityInfo.Name`, dimension, and unit; the named QTO accessors (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) stay as convenience reads on the takeoff fold so a `Qto_*BaseQuantities` consumer reads `measure.Volume` without a kind switch, the `Rasm.Bim` `QuantityDerivation.Derive` base-quantity fold composing this carrier (admitting each base quantity through `Of(value, AreaUnit.SquareMeter, key)` or the explicit `OfSi(QuantityType.Area, …)` so the geometry-true takeoff is type-tagged at ingress, never inferred from an ambiguous dimension).
