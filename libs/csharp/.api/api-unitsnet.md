# [RASM_APPUI_API_UNITSNET]

`UnitsNet` supplies ~120 strongly-typed quantity structs (`Length`, `Area`, `Angle`, …) over a `double`-or-`decimal` `QuantityValue`, each implementing the .NET 7+ generic-math static-abstract operator interfaces (`IAdditionOperators`, `IMultiplyOperators`, `IComparisonOperators`, `IParsable`) plus `IArithmeticQuantity<TSelf,TUnit,TValue>`. It carries unit enums, culture-aware parse/format, dynamic (boxed `IQuantity`) resolution, the `UnitMath` LINQ aggregate rail, cacheable `ConversionFunction` delegates, SI `BaseDimensions`/`BaseUnits` metadata, and `UnitSystem` policy — the unit algebra for measured execution inputs and receipts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet` (5.75.0, MIT-0, © Andreas Gullberg Larsen)
- assembly: `UnitsNet`
- namespace: `UnitsNet`, `UnitsNet.Units` (unit enums)
- asset: managed runtime library (`lib/net9.0` binds the `net10.0` consumer — highest available TFM; `lib/net8.0` fallback) plus satellite `*.resources.dll` for `fr-CA`/`nb-NO`/`ru-RU`/`zh-CN` abbreviations
- rail: units

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts and value carriers
- rail: units

| [INDEX] | [SYMBOL]                              | [SHAPE]                       | [CAPABILITY]                                                       |
| :-----: | :------------------------------------ | :---------------------------- | :---------------------------------------------------------------- |
|  [01]   | `IQuantity`                           | interface                     | boxed quantity: `Value`/`Unit`/`Dimensions`/`QuantityInfo`/`As`/`ToUnit` |
|  [02]   | `IQuantity<TUnit>`                    | interface                     | typed-unit boxed quantity (`As(TUnit)`/`ToUnit(TUnit)`)           |
|  [03]   | `IQuantity<TSelf, TUnit, TValue>`     | self-typed interface          | the strongly-typed quantity contract (`Length : IQuantity<Length, LengthUnit, double>`) |
|  [04]   | `IValueQuantity<TValue>`              | generic interface             | exposes the backing scalar (`double`/`decimal`) — `Length : IValueQuantity<double>` |
|  [05]   | `IDecimalQuantity`                    | interface                     | marks the decimal-precision quantity rail                         |
|  [06]   | `IArithmeticQuantity<TSelf,TUnit,TValue>` | generic interface         | adds the `+`/`-`/`*`/`/` operator contract over `TSelf`           |
|  [07]   | `QuantityValue`                       | `readonly struct`             | `double`-or-`decimal` union: `Type`/`IsDecimal`, implicit-in from every numeric primitive, explicit-out to `double`/`decimal`, `Zero` |
|  [08]   | `BaseDimensions`                      | sealed class                  | 7-vector SI exponents `Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity` |
|  [09]   | `BaseUnits`                           | sealed class                  | the 7 nullable SI base-unit selectors + `IsFullyDefined`/`IsSubsetOf`/`Undefined` |

[PUBLIC_TYPE_SCOPE]: admitted quantity families (each a `readonly struct` with native operators)
- rail: units

| [INDEX] | [SYMBOL]       | [CANONICAL UNIT] | [CAPABILITY]                                         |
| :-----: | :------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `Length`       | Meter            | length; `+`/`-`/scalar `*`÷, `Length/Length -> double`; `FeetInches` projection |
|  [02]   | `Area`         | SquareMeter      | area; `Length * Length -> Area` cross-quantity operator |
|  [03]   | `Volume`       | CubicMeter       | volume                                               |
|  [04]   | `Mass`         | Kilogram         | mass                                                 |
|  [05]   | `Duration`     | Second           | duration                                             |
|  [06]   | `Speed`        | MeterPerSecond   | speed; `Length / Duration -> Speed`                  |
|  [07]   | `Acceleration` | MeterPerSecondSquared | acceleration                                    |
|  [08]   | `Force`        | Newton           | force; `Mass * Acceleration -> Force`                |
|  [09]   | `Pressure`     | Pascal           | pressure; `Force / Area -> Pressure`                 |
|  [10]   | `Energy`       | Joule            | energy; `Power * Duration -> Energy`                 |
|  [11]   | `Power`        | Watt             | power; `Energy / Duration -> Power`                  |
|  [12]   | `Temperature`  | Kelvin           | temperature (affine — add via `TemperatureDelta`)    |
|  [13]   | `Angle`        | Radian           | angle (display `Degree`) — bridges Rasm geometry rads |
|  [14]   | `Torque`       | NewtonMeter      | torque; `Force * Length -> Torque` (not Energy)      |
|  [15]   | `Ratio`        | DecimalFraction  | ratio (display `Percent`)                            |

[PUBLIC_TYPE_SCOPE]: admitted unit enum families (`UnitsNet.Units`)
- rail: units

| [INDEX] | [SYMBOL]           | [CANONICAL / DISPLAY]                            |
| :-----: | :----------------- | :---------------------------------------------- |
|  [01]   | `LengthUnit`       | `Meter` canonical, `Millimeter` display         |
|  [02]   | `AreaUnit`         | `SquareMeter`                                   |
|  [03]   | `VolumeUnit`       | `CubicMeter`                                    |
|  [04]   | `MassUnit`         | `Kilogram`                                      |
|  [05]   | `DurationUnit`     | `Second`                                        |
|  [06]   | `SpeedUnit`        | `MeterPerSecond`                                |
|  [07]   | `AccelerationUnit` | `MeterPerSecondSquared`                         |
|  [08]   | `ForceUnit`        | `Newton`                                        |
|  [09]   | `PressureUnit`     | `Pascal` canonical, `Kilopascal` display        |
|  [10]   | `EnergyUnit`       | `Joule` canonical, `KilowattHour` display       |
|  [11]   | `PowerUnit`        | `Watt`                                          |
|  [12]   | `TemperatureUnit`  | `Kelvin` canonical, `DegreeCelsius` display     |
|  [13]   | `AngleUnit`        | `Radian` canonical, `Degree` display            |
|  [14]   | `TorqueUnit`       | `NewtonMeter`                                   |
|  [15]   | `RatioUnit`        | `DecimalFraction` canonical, `Percent` display  |

[PUBLIC_TYPE_SCOPE]: parsing, conversion, metadata, and setup
- rail: units

| [INDEX] | [SYMBOL]                 | [SHAPE]            | [CAPABILITY]                                                 |
| :-----: | :----------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `Quantity`               | static façade      | dynamic boxed resolution + the `ByName`/`Infos` registry      |
|  [02]   | `UnitConverter`          | sealed class       | dynamic `Convert`/`TryConvert` + cacheable `GetConversionFunction` |
|  [03]   | `ConversionFunction`     | delegate           | `IQuantity ConversionFunction(IQuantity input)` — the cached conversion |
|  [04]   | `QuantityParser`         | parser             | the parse engine behind `Quantity.Parse`/`TQuantity.Parse`    |
|  [05]   | `UnitParser`             | sealed class       | `Parse<TUnit>`/`TryParse<TUnit>` + reflective `Enum`/`Type` overloads |
|  [06]   | `QuantityFormatter`      | static formatter   | `Format<TUnit>(IQuantity<TUnit>, format, IFormatProvider?)`   |
|  [07]   | `UnitFormatter`          | static formatter   | abbreviation-aware unit-string formatting                     |
|  [08]   | `UnitAbbreviationsCache` | abbreviation store | `Default` + culture-keyed abbreviation registration/lookup    |
|  [09]   | `UnitMath`               | static extensions  | `Sum`/`Min`/`Max`/`Average`/`Clamp`/`Abs` over `IEnumerable<TQuantity>` |
|  [10]   | `QuantityInfo`           | metadata           | `UnitInfos`/`BaseUnitInfo`/`ValueType`/`UnitType`/`BaseDimensions`/`Zero`/`GetUnitInfoFor(BaseUnits)` |
|  [11]   | `UnitInfo`               | metadata           | per-unit name/abbreviation/`BaseUnits` descriptor             |
|  [12]   | `UnitSystem`             | sealed class       | `SI` static + `BaseUnits` policy for `As`/`ToUnit` reprojection |
|  [13]   | `UnitsNetSetup`          | sealed class       | `Default` root exposing `UnitConverter`/`UnitParser`/`QuantityParser`/`UnitAbbreviations` |
|  [14]   | `DefaultUnitAttribute` / `ConvertToUnitAttribute` / `DisplayAsUnitAttribute` | attributes | declare default/conversion/display unit on a member |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: typed quantity construction, conversion, and operators
- rail: units
- surface-root: per-quantity struct (e.g. `Length`)

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Length.FromMeters(QuantityValue)` / `FromMillimeters` / `From<unit>` | per-unit static factory (one per unit member) |
|  [02]   | `new Length(QuantityValue value, LengthUnit unit)`                 | ctor (value + unit)                           |
|  [03]   | `Length.FromFeetInches(double, double)` / `.FeetInches`            | composite imperial construct / projection     |
|  [04]   | `.As(LengthUnit)` / `.As(UnitSystem)` -> `double`                  | read in a target unit / by unit-system policy |
|  [05]   | `.ToUnit(LengthUnit)` / `.ToUnit(LengthUnit, UnitConverter)` / `.ToUnit(UnitSystem)` | reproject to a target unit / via explicit converter / by policy |
|  [06]   | `.Meters` / `.Millimeters` / `.<Unit>s` -> `double`                | direct per-unit accessor (`=> As(unit)`)      |
|  [07]   | `+` / `-` / `*` (scalar) / `/` (scalar) / `/` (`Length/Length->double`) / unary `-` | native arithmetic operators |
|  [08]   | `<` / `<=` / `>` / `>=` / `==` / `!=`, `CompareTo`, `Equals`       | native comparison (`IComparisonOperators`)    |
|  [09]   | `.ToString()` / `.ToString(format, IFormatProvider?)`              | culture-aware quantity rendering              |
|  [10]   | static `Zero` / `BaseUnit` / `Units` / `Info` / `BaseDimensions` / `DefaultConversionFunctions` | quantity statics |

[ENTRYPOINT_SCOPE]: parsing (typed and dynamic)
- rail: units

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Length.Parse(string, [IFormatProvider?])` / `TryParse(string?, [IFormatProvider?], out Length)` | typed parse (`IParsable<Length>`) |
|  [02]   | `Length.TryParseUnit(string, [IFormatProvider?], out LengthUnit)` | parse just the unit token                     |
|  [03]   | `Quantity.Parse(IFormatProvider?, Type, string)` / `TryParse(Type, string, out IQuantity?)` | dynamic parse to boxed `IQuantity` |
|  [04]   | `Quantity.From(QuantityValue, Enum)` / `From(QuantityValue, string quantityName, string unitName)` | dynamic factory |
|  [05]   | `Quantity.FromUnitAbbreviation([IFormatProvider?], QuantityValue, string)` / `TryFromUnitAbbreviation(...)` | parse value+abbreviation |
|  [06]   | `Quantity.TryFrom(QuantityValue, Enum?, out IQuantity?)` / `FromQuantityInfo(QuantityInfo, QuantityValue)` | guarded dynamic construct |
|  [07]   | `UnitParser.Default.Parse<TUnit>(string, [IFormatProvider?])` / `TryParse<TUnit>(...)` | unit-enum resolution from abbreviation |

[ENTRYPOINT_SCOPE]: conversion, aggregation, metadata, and setup
- rail: units

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `UnitConverter.Convert(QuantityValue, Enum from, Enum to) -> double` / `TryConvert(..., out double)` | dynamic numeric conversion (no boxing of a typed struct) |
|  [02]   | `UnitConverter.Default.GetConversionFunction<TQuantity>(Enum from, Enum to) -> ConversionFunction` / `TryGetConversionFunction(...)` | resolve a reusable cached conversion delegate |
|  [03]   | `UnitMath.Sum<TQuantity>(this IEnumerable<TQuantity>, Enum unitType)` / `Sum<TSource,TQuantity>(src, selector, unitType)` | LINQ sum in a chosen unit |
|  [04]   | `UnitMath.Min`/`Max`/`Average<TQuantity>(this IEnumerable<TQuantity>, Enum unitType)` / 2-arg `Min`/`Max`/`Clamp(value,min,max)` | LINQ + pairwise reductions |
|  [05]   | `UnitMath.Abs<TQuantity>(this TQuantity)` / `ToQuantityValue(this double\|decimal)` | absolute value / primitive lift |
|  [06]   | `BaseDimensions.Multiply(BaseDimensions)` / `Divide(...)` / `*` operator / `Dimensionless` / `IsBaseQuantity()`/`IsDerivedQuantity()`/`IsDimensionless()` | dimension algebra |
|  [07]   | `Quantity.Infos` / `Quantity.ByName` / `Quantity.GetUnitInfo(Enum)` / `GetQuantitiesWithBaseDimensions(BaseDimensions)` | the quantity/unit registry |
|  [08]   | `QuantityInfo.GetUnitInfoFor(BaseUnits)` / `.UnitInfos` / `.BaseUnitInfo` / `.ValueType` / `.BaseDimensions` | per-family metadata |
|  [09]   | `UnitSystem.SI` / `new UnitSystem(BaseUnits)` / `UnitsNetSetup.Default`           | unit-system policy + the configured service root |
|  [10]   | `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, format, [IFormatProvider?])`   | explicit quantity formatting                  |

## [04]-[IMPLEMENTATION_LAW]

[UNIT_ALGEBRA]:
- Every quantity is a `readonly struct` implementing the .NET generic-math static-abstract interfaces, so `a + b`, `2.5 * length`, `length / duration -> Speed`, and `length / length -> double` are native operators, not method calls. Cross-quantity operators are real (`Mass * Acceleration -> Force`, `Force / Area -> Pressure`, `Length * Length -> Area`): physical algebra is type-checked at compile time. A bare `double` carrying an implicit unit is the rejected form.
- `QuantityValue` is a `double`-or-`decimal` union (`Type`/`IsDecimal`): a quantity constructed from a `decimal` retains decimal precision through the `IDecimalQuantity` rail; from a `double` it stays double. It accepts every numeric primitive implicitly and surrenders to `double`/`decimal` only by explicit cast — keep values inside the quantity, project to the primitive only at the boundary.
- Conversion has three tiers by cost: the typed `ToUnit`/`As` on the struct (zero allocation, the default); the dynamic `UnitConverter.Convert(value, from, to)` for a runtime-chosen unit pair (no boxing of a typed struct); and `GetConversionFunction(from, to) -> ConversionFunction` for a hot loop converting many values across the same unit pair — resolve the delegate once, apply per element.
- `UnitSystem` is the policy projection: `As(UnitSystem.SI)`/`ToUnit(UnitSystem)` reproject a quantity into whatever base units a `BaseUnits` selects, so a receipt declares its unit system once and every quantity renders consistently. `BaseDimensions` (the 7 SI exponents) is the identity for derived-quantity discovery via `Quantity.GetQuantitiesWithBaseDimensions`.

[LOCAL_ADMISSION]:
- Compute numeric inputs and receipts carry explicit quantity structs whenever units affect meaning; the unit is part of the type, never a comment or a suffixed field name.
- Unit conversion is rail policy on the boundary; it is never hidden inside an interior numeric helper. Parse/format are boundary operations (culture-aware via `IFormatProvider`), not the internal representation.
- Aggregation over a collection of quantities flows through `UnitMath` (`Sum`/`Average`/`Min`/`Max`/`Clamp`), which folds in a chosen unit — never unwrap to `double`, sum, and rewrap.
- Quantity metadata (`QuantityInfo`/`UnitInfo`/`BaseDimensions`) drives diagnostics, support output, and receipt projection; the dynamic `Quantity`/`IQuantity` façade is the reflection seam for a unit not known at compile time.

[STACKING]:
- `Angle` is the bridge to the `Rasm` geometry kernel: kernel rotations/tolerances are radians (`AngleUnit.Radian` canonical), so an `Angle` from a UI degree field round-trips through `Angle.FromDegrees(d).Radians` into a kernel transform with no hand-rolled `* Math.PI / 180`.
- `System.Text.Json` / Thinktecture-JSON (`api-thinktecture-json.md`): persist a measured receipt as `{ value, unit }` (the `IQuantity.Value` + `IQuantity.Unit` pair) and rehydrate through `Quantity.From(value, unitEnum)` — the dynamic façade is the deserialization entry. Never serialize the struct's private fields.
- Avalonia binding (`api-reactiveui-avalonia.md`): a view-model exposes `length.ToString("0.0 mm", culture)` for display and parses user text through `Length.TryParse(text, culture, out _)`; an `IValueConverter` wrapping `As`/`Parse` is the XAML seam. The `UnitAbbreviationsCache.Default` (culture-keyed, with the satellite `.resources.dll` for `fr-CA`/`ru-RU`/`zh-CN`/`nb-NO`) localizes the abbreviation.
- `NodaTime` (`api-nodatime.md`): a `Duration` quantity (physical seconds) is distinct from a NodaTime `Duration` (a clock span) — keep the unit-bearing UnitsNet `Duration` for measured physical durations on receipts and the NodaTime type for wall-clock intervals; they meet only where a physical second is reinterpreted as elapsed time.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: the typed quantity/unit algebra — quantity structs, native generic-math operators, the `double`/`decimal` `QuantityValue` union, culture-aware parse/format, dynamic `IQuantity` resolution, the `UnitMath` LINQ rail, cacheable `ConversionFunction` conversion, SI `BaseDimensions`/`BaseUnits` metadata, and `UnitSystem` policy.
- Accept: measured unit-aware execution inputs and receipts; conversion as boundary rail policy; aggregation through `UnitMath`; the `{ value, unit }` wire shape via the dynamic façade.
- Reject: raw numeric values with unit-by-comment or unit-by-field-name; hand-rolled conversion factors when a quantity operator or `UnitConverter` owns it; unwrap-sum-rewrap aggregation; the phantom non-generic `IValueQuantity`/`IArithmeticQuantity` or the removed `GenericMathExtensions` (v5 uses native operators).
