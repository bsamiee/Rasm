# [ELEMENT_QUANTITY]

The typed measured-scalar owner: one `MeasureValue` carrying its SI magnitude, its canonical unit string, and a `Dimension` `[ComplexValueObject]` over the seven SI base-dimension exponents that IS the quantity-type discriminator — admitted once through the `UnitsNet` quantity registry, coerced to its SI base, and read back through dimension-checked accessors. This REPLACES the six-value `QuantityKind` enum the migration source carried: a `MeasureValue` is no longer one of `Length`/`Area`/`Volume`/`Weight`/`Count`/`Time` but ANY measured quantity whose dimension the `UnitsNet` `BaseDimensions` 7-vector identifies — so `ThermalTransmittance` (W·m⁻²·K⁻¹), `Pressure` (Pa), `Force` (N), `MassDensity` (kg·m⁻³), `SoundPressureLevel` (dB), `PlaneAngle` (rad), and `Temperature` (K) all admit with units and SI base, and the six former kinds become convenience accessors on the quantity-takeoff fold, never a closed enum the next `Pset_*` measure cannot extend. The owner composes the admitted `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity` → `ToUnit(UnitSystem.SI)` → `Dimensions`) for the one-time SI coercion and the `Properties/property#PROPERTY_VALUE` `PropertyValue.Measure` carrier for the typed property arm; a non-finite or unresolvable measure rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`. The page realizes a `Dimension`-discriminated `MeasureValue` over the full `UnitsNet` quantity family, never a six-case enum.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` `[ComplexValueObject]` seven-SI-exponent quantity-type discriminator, the canonical-dimension static rows, the `SiSymbol` display projection, and the `Multiply`/`Divide` derived-dimension algebra.
- [02]-[MEASURE_VALUE]: the `MeasureValue` SI-coerced scalar carrier, the `UnitsNet` admission seam (the typed `Of(Enum)`, the abbreviation `Of(string)` wire-decode, and the SI-native `OfSi(Dimension, double)` factories), the dimension-checked QTO accessors, the `Quantize` content-hash projection, and the same-dimension `Sum` reducer.

## [02]-[DIMENSION]

- Owner: `Dimension` the `[ComplexValueObject]` carrying the seven SI base-dimension exponents (`Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity`), the quantity-type identity a `MeasureValue` discriminates on; the canonical-dimension static rows (`Length`/`Area`/`Volume`/`Mass`/`Duration`/`Dimensionless` and the derived `Pressure`/`Density`/`Force`/`ThermalTransmittance`) name the well-known dimensions a QTO accessor matches.
- Entry: `Dimension.Of(BaseDimensions dims)` lowers the `UnitsNet` 7-vector onto the value-object; `Dimension.Create(length, mass, time, current, temperature, amount, luminousIntensity)` is the generated factory; `Multiply`/`Divide` add/subtract the exponent vectors so a derived dimension composes (`Force.Divide(Area)` IS `Pressure`) without a per-quantity row.
- Auto: the `[ComplexValueObject]` generates structural equality over the seven exponents so two `MeasureValue`s share a dimension iff their exponent vectors match, the `Of` projection reads the `UnitsNet` `BaseDimensions` `Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity` integer exponents directly, and `Multiply`/`Divide` are pure exponent arithmetic mirroring the `UnitsNet` `BaseDimensions.Multiply`/`Divide` algebra so the seam never re-derives the dimensional product.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`), UnitsNet (`BaseDimensions` the 7-vector source).
- Growth: a new well-known dimension is one static row (a `LuminousFlux`/`Frequency` row composed from the exponents); a new measured quantity needs NO new dimension — its `UnitsNet` `BaseDimensions` resolves the exponents at admission; never a per-quantity dimension type and never a closed `QuantityKind` enum.
- Boundary: `Dimension` is the ONE quantity-type discriminator — a six-case `QuantityKind` enum (the migration source's form) is the deleted form, because it could not carry `ThermalTransmittance`/`Pressure`/`SoundPressureLevel` and forced an out-of-family measure to a dimensionless `Count`; the exponent vector is the identity so any `UnitsNet` quantity admits, the named static rows are convenience anchors a QTO accessor matches, and the `Multiply`/`Divide` algebra composes a derived dimension from base ones rather than enumerating every product; the exponents come from the `UnitsNet` `BaseDimensions` and a hand-coded dimension table that drifts from the unit registry is the named defect.

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
 // already SI-native (no UnitsNet unit to read); identity is the 7-vector exponents, so this string is display-only, never hashed.
 public string SiSymbol =>
 this == Dimensionless ? "" : this == LengthDim ? "m" : this == AreaDim ? "m2" : this == VolumeDim ? "m3"
 : this == MassDim ? "kg" : this == DurationDim ? "s" : this == ForceDim ? "N" : this == PressureDim ? "Pa"
 : this == DensityDim ? "kg/m3" : this == ThermalTransmittance ? "W/(m2.K)" : "SI";
}
```

## [03]-[MEASURE_VALUE]

- Owner: `MeasureValue` the SI-coerced measured-scalar carrier (`Dimension` + `double Si` + `string CanonicalUnit`) the `Properties/property#PROPERTY_VALUE` `Measure` arm, the `Composition/material#MATERIAL_NODE` measured columns, and the `Assessment/assessment#ASSESSMENT_NODE` result scalars read; the dimension-checked QTO accessors (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) replace the former `QuantityKind` switch.
- Entry: `MeasureValue.Of(double value, Enum unit, Op key)` admits a value-plus-`UnitsNet`-unit, coercing to SI base once through the `UnitsNet` registry (`Quantity.TryFrom` → `IQuantity.ToUnit(UnitSystem.SI)`), reading the `Dimensions` 7-vector into the typed `Dimension`, `Fin<T>` railing `ElementFault.ValueRejected` on a non-finite value or an unresolvable unit; `MeasureValue.Count(double value, Op key)` admits a dimensionless count; `MeasureValue.OfSi(Dimension dimension, double si)` admits an already-SI-base value-plus-dimension (the `Rasm.Compute` result + `SectionProperties` bake entry, no UnitsNet unit needed); `MeasureValue.Of(double value, string unit, Op key)` resolves a unit abbreviation off the wire; `MeasureValue.Zero` the canonical dimensionless zero.
- Auto: `Of` routes the raw value through `Quantity.From(value, unit)` to a boxed `IQuantity`, reprojects to SI base through `ToUnit(UnitSystem.SI)`, and reads `(double)si.Value` plus `si.Unit.ToString()` plus `Dimension.Of(si.Dimensions)` so the persisted scalar is always SI-base and the dimension is the registry's, never a hand-mapped kind; the QTO accessors test the stored `Dimension` against the canonical static rows (`IsArea` iff `Dimension == Dimension.AreaDim`) and return the `Si` as an `Option<double>` so a takeoff reads `measure.Area` rather than a `Kind == QuantityKind.Area` branch; `Quantize(tolerance)` rounds `Si` to the `Header.Tolerance` grid for the `Projection/address#CONTENT_ADDRESS` canonical-byte projection so two measures within tolerance share a content key; `Sum(measures, key)` reduces a same-dimension sequence by summing the SI scalars (already SI-base, so the dimensioned sum is the scalar sum), railing on a dimension mismatch.
- Receipt: a `MeasureValue` is the unit-checked evidence a takeoff, a property facet, and a cost join read — `measure.Si` is the SI magnitude, `measure.CanonicalUnit` the SI unit string, `measure.Dimension` the quantity type; a same-dimension element-set aggregate folds through `MeasureValue.Sum` so the dimensioned reduction is one typed fold, never a manual `double` accumulation, and the QTO accessor names (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) are the convenience reads the cost and quantity consumers compose.
- Packages: UnitsNet (`Quantity`/`IQuantity`/`UnitSystem.SI`/`BaseDimensions` the SI-coercion registry), Thinktecture.Runtime.Extensions, LanguageExt.Core, `Rasm` (the kernel `Op` op-key).
- Boundary: `MeasureValue` NEVER carries a bare `double` quantity — the value coerces to SI base once at `Of` through the `UnitsNet` registry and the interior carries the SI scalar plus the typed `Dimension`, a free `double` measure field being the named defect per the `UnitsNet` ownership of dimensioned scalars; the `Dimension` is the discriminator so any `UnitsNet` quantity admits (the migration `QuantityKind` six-case enum that degraded an out-of-family measure to `Count` is the deleted form); the SI coercion rides the `UnitsNet` `Quantity.TryFrom`/`ToUnit(UnitSystem.SI)` registry and a stringly-keyed unit switch or an ad-hoc conversion factor is the deleted form; aggregation flows through `MeasureValue.Sum` (an SI-base scalar sum guarded by dimension equality), never an unwrap-sum-rewrap, and a cross-dimension sum rails `ElementFault.ValueRejected`; `Quantize` is the ONLY rounding, applied at the content-hash boundary against `Header.Tolerance`, so the interior `Si` stays full-precision and the canonical key is tolerance-stable.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MeasureValue(Dimension Dimension, double Si, string CanonicalUnit) {
 public static readonly MeasureValue Zero = new(Dimension.Dimensionless, 0.0, "");

 public static Fin<MeasureValue> Of(double value, Enum unit, Op key) =>
 !double.IsFinite(value)
 ? ElementFault.ValueRejected(key, $"<measure-non-finite:{value:R}>")
 : Quantity.TryFrom(value, unit, out IQuantity? quantity) && quantity is { } q
 ? Coerce(q, key)
 : ElementFault.ValueRejected(key, $"<measure-unit-unresolved:{unit}>");

 // The wire-decode overload: a value plus a UnitsNet unit ABBREVIATION (the {value, unit:string} wire shape the
 // Python/TypeScript peers send) resolves through the UnitsNet abbreviation registry, then coerces to SI base.
 public static Fin<MeasureValue> Of(double value, string unit, Op key) =>
 !double.IsFinite(value)
 ? ElementFault.ValueRejected(key, $"<measure-non-finite:{value:R}>")
 : Quantity.TryFromUnitAbbreviation(value, unit, out IQuantity? quantity) && quantity is { } q
 ? Coerce(q, key)
 : ElementFault.ValueRejected(key, $"<measure-unit-abbreviation-unresolved:{unit}>");

 // The SI-native factory: an already-coerced SI magnitude plus its Dimension (no UnitsNet unit needed) — the entry a
 // Rasm.Compute analysis writes a computed result (an assembly U-value, a section modulus) through, and the entry a
 // SectionProperties bake stamps its m^4/m^3 columns through. Identity is Si + Dimension; the unit symbol is display-only.
 public static MeasureValue OfSi(Dimension dimension, double si) => new(dimension, si, dimension.SiSymbol);

 public static Fin<MeasureValue> Count(double value, Op key) =>
 double.IsFinite(value)
 ? Fin.Succ(new MeasureValue(Dimension.Dimensionless, value, ""))
 : ElementFault.ValueRejected(key, $"<count-non-finite:{value:R}>");

 static Fin<MeasureValue> Coerce(IQuantity quantity, Op key) =>
 quantity.ToUnit(UnitSystem.SI) is { } si && double.IsFinite((double)si.Value)
 ? Fin.Succ(new MeasureValue(Dimension.Of(si.Dimensions), (double)si.Value, si.Unit.ToString()))
 : ElementFault.ValueRejected(key, $"<measure-si-non-finite:{quantity.Unit}>");

 // QTO accessors: the six former QuantityKind cases as dimension-checked reads on the takeoff fold,
 // never a closed enum — an out-of-family measure (Pressure, ThermalTransmittance) simply has no
 // named accessor and is read through Si/Dimension directly.
 public Option<double> Length => Dimension == Dimension.LengthDim ? Some(Si) : None;
 public Option<double> Area => Dimension == Dimension.AreaDim ? Some(Si) : None;
 public Option<double> Volume => Dimension == Dimension.VolumeDim ? Some(Si) : None;
 public Option<double> Weight => Dimension == Dimension.MassDim ? Some(Si) : None;
 public Option<double> Time => Dimension == Dimension.DurationDim ? Some(Si) : None;
 public Option<double> Count => Dimension == Dimension.Dimensionless ? Some(Si) : None;

 // Tolerance quantization for the content-hash boundary: round the SI magnitude to the tolerance grid
 // so two measures within Header.Tolerance project to the same canonical bytes.
 public MeasureValue Quantize(double tolerance) =>
 tolerance > 0.0 ? this with { Si = Math.Round(Si / tolerance, MidpointRounding.AwayFromZero) * tolerance } : this;

 public static Fin<MeasureValue> Sum(Seq<MeasureValue> measures, Op key) =>
 measures.IsEmpty
 ? Fin.Succ(Zero)
 : measures.Tail.Exists(m => m.Dimension != measures.Head.Dimension)
 ? ElementFault.ValueRejected(key, "<measure-sum-dimension-mismatch>")
 : Fin.Succ(measures.Head with { Si = measures.Sum(static m => m.Si) });
}
```

## [04]-[RESEARCH]

- [UNITSNET_SI_COERCION]: the `UnitsNet` `Quantity.TryFrom(QuantityValue, Enum, out IQuantity?)` dynamic factory, the `IQuantity.ToUnit(UnitSystem.SI)` SI-base reprojection, the `IQuantity.Value`/`Unit` reads, and the `IQuantity.Dimensions` `BaseDimensions` 7-vector are catalogued in `.api/api-unitsnet.md`; the seam coerces every measured value once at `Of` through this registry so the persisted scalar is SI-base and the `Dimension` is the registry's, never a hand-mapped kind — the v5 removal of the `QuantityType` enum is exactly why the `Dimension` 7-vector is the discriminator, the `BaseDimensions` exponents the stable identity any of the ~120 quantity structs resolves.
- [DIMENSION_DISCRIMINATOR]: replacing the migration `QuantityKind` six-case enum with the `Dimension` `[ComplexValueObject]` admits the `Pset_*` measures the old enum forced to `Count` — `Pset_WallCommon.ThermalTransmittance` (W·m⁻²·K⁻¹), `Pset_*` `Pressure`/`Force`/`MassDensity`, `SoundPressureLevel` (dB), `PlaneAngle` (rad), `Temperature` (K) — each carrying its real dimension and SI unit; the named QTO accessors (`Length`/`Area`/`Volume`/`Weight`/`Time`/`Count`) stay as convenience reads on the takeoff fold so a `Qto_*BaseQuantities` consumer reads `measure.Volume` without a kind switch, the `Rasm.Bim` `QuantitySet.Derive` base-quantity fold composing this carrier for the geometry-true takeoff.
