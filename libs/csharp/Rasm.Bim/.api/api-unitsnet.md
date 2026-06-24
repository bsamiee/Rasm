# [RASM_BIM_API_UNITSNET]

`UnitsNet` supplies dimensioned scalar quantities, their SI-base coercion, and the
abbreviation parser the BIM `PropertySet`/`QuantitySet` rail uses to collapse the IFC
`KindOf` axis (`IfcLengthMeasure`/`IfcAreaMeasure`/`IfcVolumeMeasure`/`IfcMassMeasure`/
`IfcTimeMeasure`) into one canonical, unit-checked quantity value. Each quantity is a
`readonly struct` implementing generic-math operators, so `QuantitySet` coercion stays an
expression over typed values rather than a free `double`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- assembly: `UnitsNet`
- namespace: `UnitsNet`
- namespace: `UnitsNet.Units`
- asset: runtime library
- rail: quantity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dimensioned quantity family
- rail: quantity

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]           | [RAIL]                        |
| :-----: | :---------------- | :---------------------- | :---------------------------- |
|  [01]   | `Length`          | length quantity         | `IfcLengthMeasure` carrier    |
|  [02]   | `Area`            | area quantity           | `IfcAreaMeasure` carrier      |
|  [03]   | `Volume`          | volume quantity         | `IfcVolumeMeasure` carrier    |
|  [04]   | `Mass`            | mass quantity           | `IfcMassMeasure` carrier      |
|  [05]   | `Duration`        | time quantity           | `IfcTimeMeasure` carrier      |
|  [06]   | `QuantityValue`   | union numeric value     | `double`/`decimal` payload    |
|  [07]   | `IQuantity`       | quantity contract       | erased quantity seam          |
|  [08]   | `QuantityInfo`    | quantity metadata       | unit-set lookup               |
|  [09]   | `QuantityInfo<T>` | typed quantity metadata | typed unit-set lookup         |
|  [10]   | `UnitInfo`        | unit metadata           | abbreviation/base-unit record |

[PUBLIC_TYPE_SCOPE]: unit vocabulary and base-dimension family
- rail: quantity

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]         | [RAIL]                     |
| :-----: | :--------------- | :-------------------- | :------------------------- |
|  [01]   | `LengthUnit`     | length unit enum      | `Length` coercion target   |
|  [02]   | `AreaUnit`       | area unit enum        | `Area` coercion target     |
|  [03]   | `VolumeUnit`     | volume unit enum      | `Volume` coercion target   |
|  [04]   | `MassUnit`       | mass unit enum        | `Mass` coercion target     |
|  [05]   | `DurationUnit`   | time unit enum        | `Duration` coercion target |
|  [06]   | `BaseUnits`      | SI base-unit tuple    | `UnitSystem` definition    |
|  [07]   | `UnitSystem`     | unit-system policy    | `UnitSystem.SI` coercion   |
|  [08]   | `BaseDimensions` | dimensional exponents | dimension-equality check   |

[PUBLIC_TYPE_SCOPE]: conversion, parse, and aggregation family
- rail: quantity

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]        | [RAIL]                        |
| :-----: | :-------------- | :------------------- | :---------------------------- |
|  [01]   | `Quantity`      | dynamic quantity hub | type-erased construct/parse   |
|  [02]   | `UnitConverter` | conversion registry  | unit-to-unit conversion       |
|  [03]   | `UnitParser`    | unit-abbrev parser   | abbreviation-to-unit resolve  |
|  [04]   | `UnitMath`      | quantity aggregation | typed `Sum`/`Min`/`Max`/`Avg` |
|  [05]   | `UnitsNetSetup` | setup singleton      | default converter/parser seam |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: typed construction and coercion
- rail: quantity

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]     | [RAIL]                           |
| :-----: | :--------------------------------------- | :----------------- | :------------------------------- |
|  [01]   | `Length.FromMeters`                      | SI factory         | length intake from SI base       |
|  [02]   | `Area.FromSquareMeters`                  | SI factory         | area intake from SI base         |
|  [03]   | `Volume.FromCubicMeters`                 | SI factory         | volume intake from SI base       |
|  [04]   | `Mass.FromKilograms`                     | SI factory         | mass intake from SI base         |
|  [05]   | `Duration.FromSeconds`                   | SI factory         | time intake from SI base         |
|  [06]   | `Length.From(QuantityValue, LengthUnit)` | unit factory       | value-plus-unit intake           |
|  [07]   | `new Length(double, LengthUnit)`         | unit constructor   | value-plus-unit construction     |
|  [08]   | `new Length(double, UnitSystem)`         | system constructor | SI-system construction           |
|  [09]   | `Length.As(LengthUnit)`                  | unit projection    | value read in target unit        |
|  [10]   | `Length.ToUnit(LengthUnit)`              | unit coercion      | re-based quantity in target unit |
|  [11]   | `Length.ToUnit(UnitSystem)`              | system coercion    | re-based quantity in SI system   |
|  [12]   | `Length.BaseUnit`                        | base-unit anchor   | canonical SI unit identity       |
|  [13]   | `Length.Value`                           | scalar read        | constructed scalar payload       |
|  [14]   | `Length.Unit`                            | unit read          | constructed unit payload         |

[ENTRYPOINT_SCOPE]: type-erased construction, parse, and conversion
- rail: quantity

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]      | [RAIL]                        |
| :-----: | :--------------------------------------------- | :------------------ | :---------------------------- |
|  [01]   | `Quantity.From(QuantityValue, Enum)`           | erased factory      | unit-keyed quantity construct |
|  [02]   | `Quantity.From(QuantityValue, string, string)` | named factory       | name-keyed quantity construct |
|  [03]   | `Quantity.FromUnitAbbreviation`                | abbrev factory      | abbreviation-keyed construct  |
|  [04]   | `Quantity.FromQuantityInfo`                    | metadata factory    | info-keyed quantity construct |
|  [05]   | `Quantity.TryFrom`                             | erased try-factory  | non-throwing erased construct |
|  [06]   | `Quantity.Parse`                               | erased parse        | typed quantity-string parse   |
|  [07]   | `Quantity.TryParse`                            | erased try-parse    | non-throwing string parse     |
|  [08]   | `Quantity.Infos`                               | metadata roster     | registered-quantity inventory |
|  [09]   | `Quantity.ByName`                              | metadata lookup     | name-to-`QuantityInfo` map    |
|  [10]   | `Length.Parse` / `Length.TryParse`             | typed parse         | typed quantity-string parse   |
|  [11]   | `UnitParser.Parse<TUnit>`                      | unit parse          | abbreviation-to-unit resolve  |
|  [12]   | `UnitParser.TryParse<TUnit>`                   | unit try-parse      | non-throwing unit resolve     |
|  [13]   | `UnitConverter.Default`                        | converter singleton | shared conversion registry    |
|  [14]   | `UnitConverter.GetConversionFunction`          | conversion lookup   | unit-pair conversion delegate |
|  [15]   | `IQuantity.As(Enum)`                           | erased projection   | erased value read in unit     |
|  [16]   | `IQuantity.ToUnit(Enum)`                       | erased coercion     | erased re-based quantity      |

[ENTRYPOINT_SCOPE]: typed aggregation and arithmetic
- rail: quantity

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]     | [RAIL]                     |
| :-----: | :---------------------------------- | :----------------- | :------------------------- |
|  [01]   | `UnitMath.Sum<TQuantity>(Enum)`     | typed aggregation  | unit-checked quantity sum  |
|  [02]   | `UnitMath.Min<TQuantity>(Enum)`     | typed aggregation  | unit-checked quantity min  |
|  [03]   | `UnitMath.Max<TQuantity>(Enum)`     | typed aggregation  | unit-checked quantity max  |
|  [04]   | `UnitMath.Average<TQuantity>(Enum)` | typed aggregation  | unit-checked quantity mean |
|  [05]   | `UnitMath.Abs<TQuantity>`           | typed transform    | absolute quantity value    |
|  [06]   | `operator +` / `operator -`         | additive operators | same-quantity arithmetic   |
|  [07]   | `operator *` / `operator /`         | scaling operators  | scalar-quantity scaling    |
|  [08]   | `IComparisonOperators<T,T,bool>`    | comparison surface | quantity ordering          |
|  [09]   | `Length.Zero`                       | additive identity  | empty-quantity anchor      |

## [04]-[IMPLEMENTATION_LAW]

[QUANTITY_TOPOLOGY]:
- namespaces: `UnitsNet`, `UnitsNet.Units`
- typed quantities: `Length`, `Area`, `Volume`, `Mass`, `Duration` — each a `readonly struct`
- value carrier: `QuantityValue` (implicit from `double`/`decimal`/integral; explicit to `double`/`decimal`)
- unit vocabulary: `LengthUnit`, `AreaUnit`, `VolumeUnit`, `MassUnit`, `DurationUnit` enums
- SI anchors: `Length.BaseUnit` `Meter`, `Area.BaseUnit` `SquareMeter`, `Volume.BaseUnit` `CubicMeter`, `Mass.BaseUnit` `Kilogram`, `Duration.BaseUnit` `Second`
- SI system: `UnitSystem.SI`, `BaseUnits`, `BaseDimensions`
- erased seam: `IQuantity` (`Value`, `Unit`, `Dimensions`, `QuantityInfo`, `As`, `ToUnit`)
- dynamic hub: `Quantity.From`/`Parse`/`TryFrom`/`TryParse`/`Infos`/`ByName`
- aggregation: `UnitMath.Sum`/`Min`/`Max`/`Average`/`Abs`

[LOCAL_ADMISSION]:
- `QuantitySet` stores the typed quantity, never a bare `double`; the IFC `KindOf` axis selects the quantity type, and the carrier holds `Value` plus `Unit`.
- Coercion to the canonical persisted form is `ToUnit(UnitSystem.SI)` (or the typed `FromMeters`/`FromSquareMeters`/`FromCubicMeters`/`FromKilograms`/`FromSeconds` factory), so the persisted scalar is always SI-base.
- Foreign-unit ingest from an IFC `IfcSIUnit`/`IfcConversionBasedUnit` abbreviation routes through `UnitParser.TryParse<TUnit>` then `Quantity.From(value, unit)`; an unparseable unit lowers onto `BimFault.CapabilityMiss`, never an exception.
- Quantity equality and ordering use the struct's `IEqualityOperators`/`IComparisonOperators`; aggregation over an element set uses `UnitMath.Sum`/`Min`/`Max`/`Average` with the explicit SI base unit, never a manual `double` fold.
- The dynamic `Quantity`/`IQuantity` seam is the boundary-only erased path for unit-keyed IFC ingest; internal `QuantitySet` code holds the strongly-typed `Length`/`Area`/`Volume`/`Mass`/`Duration` value.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: dimensioned scalar quantities and SI-base unit coercion
- Accept: `QuantitySet` quantities carry a typed value and unit, persisted SI-base
- Reject: bare `double` quantity fields, ad-hoc unit-conversion arithmetic, exception-driven unit parse
