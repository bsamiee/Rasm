# [RASM_BIM_API_UNITSNET]

`UnitsNet` supplies dimensioned scalar quantities, their SI-base coercion, and the
abbreviation parser the BIM `PropertySet`/`QuantitySet` rail uses to collapse the IFC
`KindOf` axis (`IfcLengthMeasure`/`IfcAreaMeasure`/`IfcVolumeMeasure`/`IfcMassMeasure`/
`IfcTimeMeasure`) into one canonical, unit-checked quantity value. Each typed quantity is a
`readonly struct` implementing `IArithmeticQuantity<TSelf, TUnit, double>`, `IValueQuantity<double>`,
and the full generic-math operator set (`IAdditionOperators`/`ISubtractionOperators`/
`IMultiplyOperators<TSelf, double, TSelf>`/`IDivisionOperators<TSelf, double, TSelf>`/
`IComparisonOperators`/`IEqualityOperators`/`IParsable<TSelf>`), so `QuantitySet` coercion
stays an expression over typed values rather than a free `double`. Cross-dimensional operators
close the algebra (`Length * Length → Area`, `Length * Area → Volume`, `Length / Length → double`),
so a base-quantity derivation never hand-multiplies raw scalars. The `Bim/semantics/properties#PROPERTY_SETS`
`MeasureValue` value-carrier wraps exactly this surface: a `[Union]`/`[ValueObject]` collapse of the
`KindOf` axis onto the typed-struct family that persists the SI-base scalar.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- version: `5.75.0`
- license: MIT-0
- assembly: `UnitsNet`
- namespace: `UnitsNet`
- namespace: `UnitsNet.Units`
- asset: net8.0, net9.0, netstandard2.0; the net10.0 consumer binds the `lib/net9.0` asset (no net10.0 asset ships, so the bound public surface is the net9.0 one — the typed-quantity generic-math interfaces are identical across all three TFMs)
- asset: IL-only AnyCPU managed assembly; satellite `*.resources.dll` localization assemblies (fr-CA/nb-NO/ru-RU/zh-CN); no native binaries; ALC-safe inside the in-Rhino plugin assembly
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
|  [11]   | `UnitParser.Default`                           | parser singleton    | shared abbreviation parser (`= UnitsNetSetup.Default.UnitParser`) |
|  [12]   | `UnitParser.Parse<TUnit>`                      | unit parse          | abbreviation-to-unit resolve  |
|  [13]   | `UnitParser.TryParse<TUnit>`                   | unit try-parse      | non-throwing unit resolve     |
|  [14]   | `UnitConverter.Default`                        | converter singleton | shared conversion registry    |
|  [15]   | `UnitConverter.GetConversionFunction`          | conversion lookup   | unit-pair conversion delegate |
|  [16]   | `IQuantity.As(Enum)`                           | erased projection   | erased value read in unit     |
|  [17]   | `IQuantity.ToUnit(Enum)`                       | erased coercion     | erased re-based quantity      |

[ENTRYPOINT_SCOPE]: typed aggregation and arithmetic
- rail: quantity

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]     | [RAIL]                     |
| :-----: | :---------------------------------- | :----------------- | :------------------------- |
|  [01]   | `UnitMath.Sum<TQuantity>(this IEnumerable<TQuantity>, Enum)`               | typed aggregation  | unit-checked quantity sum at the SI unit |
|  [02]   | `UnitMath.Sum<TSource,TQuantity>(this IEnumerable<TSource>, Func<…>, Enum)` | projected aggregation | sum over a carrier-selector, no pre-projection |
|  [03]   | `UnitMath.Min<TQuantity>(this IEnumerable<TQuantity>, Enum)`               | typed aggregation  | unit-checked quantity min  |
|  [04]   | `UnitMath.Max<TQuantity>(this IEnumerable<TQuantity>, Enum)`               | typed aggregation  | unit-checked quantity max  |
|  [05]   | `UnitMath.Average<TQuantity>(this IEnumerable<TQuantity>, Enum)`           | typed aggregation  | unit-checked quantity mean |
|  [06]   | `UnitMath.Min/Max<TQuantity>(TQuantity, TQuantity)`                        | pairwise transform | binary `IComparable` quantity min/max |
|  [07]   | `UnitMath.Abs<TQuantity>(this TQuantity)`                                  | typed transform    | absolute quantity value    |
|  [08]   | `operator +` / `operator -`         | additive operators | same-quantity arithmetic   |
|  [09]   | `operator *` / `operator /` (scalar) | scaling operators  | `TSelf * double → TSelf`, `TSelf / double → TSelf` |
|  [10]   | `Length * Length → Area`, `Length * Area → Volume` | dimensional product | cross-quantity algebra (no raw-scalar multiply) |
|  [11]   | `Length / Length → double`          | dimensional ratio  | dimensionless ratio of like quantities |
|  [12]   | `IComparisonOperators<T,T,bool>` / `IEqualityOperators<T,T,bool>`          | comparison surface | quantity ordering and equality |
|  [13]   | `Length.Zero`                       | additive identity  | `IAdditiveIdentity` empty-quantity anchor |

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

[STACKING]:
- with `GeometryGymIFC_Core` (`.api/api-geometrygym-ifc`): the IFC ingest rail surfaces a `KindOf` axis as a raw `(double measure, string unit)` pair off `IfcPhysicalSimpleQuantity.MeasureValue`/`.Unit` and `IfcPropertySingleValue.NominalValue`; `UnitParser.Default.TryParse<TUnit>(unit)` resolves the abbreviation and `Quantity.From(measure, parsedUnit)` constructs the typed quantity — one rail from foreign string to typed struct, no stringly-keyed unit switch.
- with `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-json`): the `PROPERTY_SETS` `MeasureValue` value-carrier and the `QuantityKind`/`PropertyValue.Measure` `[Union]` arm own the discriminant; the UnitsNet typed struct is the payload the union case carries, and the persisted scalar is always `ToUnit(UnitSystem.SI)`-coerced before it enters the carrier — the `[Union]`/`[ValueObject]` owns shape, UnitsNet owns dimension.
- with `LanguageExt.Core`: an unparseable `IfcSIUnit`/`IfcConversionBasedUnit` abbreviation degrades through `UnitParser.Default.TryParse` to a dimensionless `QuantityKind.Count` (ingest-tolerant) or lowers onto `BimFault.CapabilityMiss`/`Fin<T>` at a hard boundary — never a thrown `UnitNotFoundException`; same-kind element-set reduction is `UnitMath.Sum(carriers, SiUnit)` lifted from the persisted SI scalar through `Length.FromMeters`/`Area.FromSquareMeters`/… , never a `Seq.Fold` over raw `double`.
- with the kernel `Rasm` geometry: `QuantitySet.Derive` reads `GeometryHandle.Volume`/`.Area`/`.Length` (already SI-base from the kernel) and wraps each through the matching `From*` factory so the takeoff is a typed quantity, and the cross-dimensional operators close a derivation (e.g. a sectional `Area * Length → Volume`) without leaving the dimensioned algebra.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: dimensioned scalar quantities and SI-base unit coercion
- Accept: `QuantitySet` quantities carry a typed value and unit, persisted SI-base
- Reject: bare `double` quantity fields, ad-hoc unit-conversion arithmetic, exception-driven unit parse, a stringly-keyed `KindOf` unit switch, a manual `double` aggregation fold
