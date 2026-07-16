# [RASM_API_UNITSNET]

`UnitsNet` owns strongly typed quantities over the `double`-or-`decimal` `QuantityValue` and binds native algebra, boundary conversion, registry identity, and unit-system policy across measured inputs and receipts. A quantity family extends the typed and boxed surfaces through one `QuantityInfo.Name` independently of convenience accessors.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet` (MIT-0, © Andreas Gullberg Larsen)
- assembly: `UnitsNet`
- namespace: `UnitsNet`, `UnitsNet.Units` (unit enums)
- asset: managed runtime library plus localized abbreviation satellite assemblies
- rail: units

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts and value carriers
- rail: units

`IQuantity` owns boxed projection and conversion; its generic forms progressively bind the unit, scalar, and self type. `QuantityValue` preserves each admitted scalar as `double` or `decimal` and projects to either primitive only by explicit cast.

| [INDEX] | [SYMBOL]                                    | [SHAPE]              | [CAPABILITY]               |
| :-----: | :------------------------------------------ | :------------------- | :------------------------- |
|  [01]   | `IQuantity`                                 | interface            | boxed quantity projection  |
|  [02]   | `IQuantity<TUnit>`                          | interface            | typed-unit projection      |
|  [03]   | `IQuantity<TSelf, TUnit, TValue>`           | self-typed interface | strongly typed contract    |
|  [04]   | `IValueQuantity<TValue>`                    | generic interface    | backing-scalar projection  |
|  [05]   | `IDecimalQuantity`                          | interface            | decimal-backed projection  |
|  [06]   | `IArithmeticQuantity<TSelf, TUnit, TValue>` | generic interface    | native arithmetic contract |
|  [07]   | `QuantityValue`                             | `readonly struct`    | scalar-type union          |
|  [08]   | `BaseDimensions`                            | sealed class         | seven-axis SI exponents    |
|  [09]   | `BaseUnits`                                 | sealed class         | nullable SI unit policy    |

[PUBLIC_TYPE_SCOPE]: admitted quantity families (each a `readonly struct` with native operators)
- rail: units

Each row binds one `QuantityInfo.Name` to its `BaseUnit` and physical concern. `TemperatureDelta` carries affine temperature differences. Material-specific heat capacity carries a consumer-minted identity beside its shared dimension instead of reusing `SpecificEntropy`.

| [INDEX] | [SYMBOL]                  | [BASE_UNIT]                | [CAPABILITY]              |
| :-----: | :------------------------ | :------------------------- | :------------------------ |
|  [01]   | `Length`                  | `Meter`                    | linear dimension          |
|  [02]   | `Area`                    | `SquareMeter`              | planar measure            |
|  [03]   | `Volume`                  | `CubicMeter`               | spatial measure           |
|  [04]   | `Mass`                    | `Kilogram`                 | inertial measure          |
|  [05]   | `Duration`                | `Second`                   | physical time             |
|  [06]   | `Speed`                   | `MeterPerSecond`           | linear rate               |
|  [07]   | `Acceleration`            | `MeterPerSecondSquared`    | speed derivative          |
|  [08]   | `Force`                   | `Newton`                   | mass-acceleration product |
|  [09]   | `Pressure`                | `Pascal`                   | force-area quotient       |
|  [10]   | `Energy`                  | `Joule`                    | power-duration product    |
|  [11]   | `Power`                   | `Watt`                     | energy-duration quotient  |
|  [12]   | `Temperature`             | `Kelvin`                   | affine temperature        |
|  [13]   | `Angle`                   | `Degree`                   | plane rotation            |
|  [14]   | `Torque`                  | `NewtonMeter`              | force-length moment       |
|  [15]   | `Ratio`                   | `DecimalFraction`          | dimensionless ratio       |
|  [16]   | `Density`                 | `KilogramPerCubicMeter`    | volumetric mass           |
|  [17]   | `ThermalConductivity`     | `WattPerMeterKelvin`       | material heat conduction  |
|  [18]   | `HeatTransferCoefficient` | `WattPerSquareMeterKelvin` | thermal transmittance     |
|  [19]   | `SpecificEntropy`         | `JoulePerKilogramKelvin`   | mass-specific entropy     |
|  [20]   | `Level`                   | `Decibel`                  | logarithmic level         |
|  [21]   | `Illuminance`             | `Lux`                      | incident luminous flux    |
|  [22]   | `Irradiance`              | `WattPerSquareMeter`       | radiant flux density      |
|  [23]   | `Luminance`               | `CandelaPerSquareMeter`    | directional surface light |
|  [24]   | `LuminousFlux`            | `Lumen`                    | perceived light power     |
|  [25]   | `LuminousIntensity`       | `Candela`                  | directional luminous flux |
|  [26]   | `RotationalSpeed`         | `RadianPerSecond`          | angular velocity          |

[PUBLIC_TYPE_SCOPE]: admitted unit enum families (`UnitsNet.Units`)
- rail: units

`[BASE_UNIT]` is each quantity type's `BaseUnit`; `[DISPLAY]` is the local projection policy.

| [INDEX] | [SYMBOL]                      | [BASE_UNIT]                | [DISPLAY]             |
| :-----: | :---------------------------- | :------------------------- | :-------------------- |
|  [01]   | `LengthUnit`                  | `Meter`                    | `Millimeter`          |
|  [02]   | `AreaUnit`                    | `SquareMeter`              | —                     |
|  [03]   | `VolumeUnit`                  | `CubicMeter`               | —                     |
|  [04]   | `MassUnit`                    | `Kilogram`                 | —                     |
|  [05]   | `DurationUnit`                | `Second`                   | —                     |
|  [06]   | `SpeedUnit`                   | `MeterPerSecond`           | —                     |
|  [07]   | `AccelerationUnit`            | `MeterPerSecondSquared`    | —                     |
|  [08]   | `ForceUnit`                   | `Newton`                   | —                     |
|  [09]   | `PressureUnit`                | `Pascal`                   | `Kilopascal`          |
|  [10]   | `EnergyUnit`                  | `Joule`                    | `KilowattHour`        |
|  [11]   | `PowerUnit`                   | `Watt`                     | —                     |
|  [12]   | `TemperatureUnit`             | `Kelvin`                   | `DegreeCelsius`       |
|  [13]   | `AngleUnit`                   | `Degree`                   | `Radian`              |
|  [14]   | `TorqueUnit`                  | `NewtonMeter`              | `KilonewtonMeter`     |
|  [15]   | `RatioUnit`                   | `DecimalFraction`          | `Percent`             |
|  [16]   | `DensityUnit`                 | `KilogramPerCubicMeter`    | —                     |
|  [17]   | `ThermalConductivityUnit`     | `WattPerMeterKelvin`       | —                     |
|  [18]   | `HeatTransferCoefficientUnit` | `WattPerSquareMeterKelvin` | —                     |
|  [19]   | `SpecificEntropyUnit`         | `JoulePerKilogramKelvin`   | —                     |
|  [20]   | `LevelUnit`                   | `Decibel`                  | —                     |
|  [21]   | `IlluminanceUnit`             | `Lux`                      | —                     |
|  [22]   | `IrradianceUnit`              | `WattPerSquareMeter`       | —                     |
|  [23]   | `LuminanceUnit`               | `CandelaPerSquareMeter`    | —                     |
|  [24]   | `LuminousFluxUnit`            | `Lumen`                    | —                     |
|  [25]   | `LuminousIntensityUnit`       | `Candela`                  | —                     |
|  [26]   | `RotationalSpeedUnit`         | `RadianPerSecond`          | `RevolutionPerMinute` |

[PUBLIC_TYPE_SCOPE]: parsing, conversion, metadata, and setup
- rail: units

| [INDEX] | [SYMBOL]                 | [SHAPE]            | [CAPABILITY]                |
| :-----: | :----------------------- | :----------------- | :-------------------------- |
|  [01]   | `Quantity`               | static façade      | boxed resolution registry   |
|  [02]   | `UnitConverter`          | sealed class       | dynamic conversion          |
|  [03]   | `ConversionFunction`     | delegate           | cached conversion           |
|  [04]   | `QuantityParser`         | parser             | quantity parsing            |
|  [05]   | `UnitParser`             | sealed class       | unit-enum parsing           |
|  [06]   | `QuantityFormatter`      | static formatter   | quantity rendering          |
|  [07]   | `UnitFormatter`          | static formatter   | unit rendering              |
|  [08]   | `UnitAbbreviationsCache` | abbreviation store | culture-keyed abbreviations |
|  [09]   | `UnitMath`               | static extensions  | quantity aggregation        |
|  [10]   | `QuantityInfo`           | metadata           | family metadata             |
|  [11]   | `UnitInfo`               | metadata           | unit metadata               |
|  [12]   | `UnitSystem`             | sealed class       | base-unit policy            |
|  [13]   | `UnitsNetSetup`          | sealed class       | configured service root     |
|  [14]   | `DefaultUnitAttribute`   | attribute          | default-unit declaration    |
|  [15]   | `ConvertToUnitAttribute` | attribute          | conversion-unit declaration |
|  [16]   | `DisplayAsUnitAttribute` | attribute          | display-unit declaration    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity contracts and scalar metadata
- rail: units

| [INDEX] | [SURFACE]                                          | [CAPABILITY]                |
| :-----: | :------------------------------------------------- | :-------------------------- |
|  [01]   | `IQuantity.Dimensions`                             | physical signature          |
|  [02]   | `IQuantity.QuantityInfo`                           | family metadata             |
|  [03]   | `IQuantity.Unit`                                   | constructed unit            |
|  [04]   | `IQuantity.Value`                                  | constructed scalar          |
|  [05]   | `IQuantity.As(Enum)`                               | scalar unit projection      |
|  [06]   | `IQuantity.As(UnitSystem)`                         | scalar policy projection    |
|  [07]   | `IQuantity.Equals(IQuantity?, IQuantity)`          | tolerance equality          |
|  [08]   | `IQuantity.ToUnit(Enum)`                           | boxed unit reprojection     |
|  [09]   | `IQuantity.ToUnit(UnitSystem)`                     | boxed policy reprojection   |
|  [10]   | `IQuantity.ToString(IFormatProvider?)`             | culture-aware rendering     |
|  [11]   | `QuantityValue.Type`                               | scalar storage kind         |
|  [12]   | `QuantityValue.IsDecimal`                          | decimal-kind test           |
|  [13]   | `QuantityValue.Zero`                               | scalar identity             |
|  [14]   | `implicit operator QuantityValue(<numeric-value>)` | numeric admission           |
|  [15]   | `explicit operator double(QuantityValue)`          | double projection           |
|  [16]   | `explicit operator decimal(QuantityValue)`         | decimal projection          |
|  [17]   | `BaseDimensions.Length`                            | length exponent             |
|  [18]   | `BaseDimensions.Mass`                              | mass exponent               |
|  [19]   | `BaseDimensions.Time`                              | time exponent               |
|  [20]   | `BaseDimensions.Current`                           | current exponent            |
|  [21]   | `BaseDimensions.Temperature`                       | temperature exponent        |
|  [22]   | `BaseDimensions.Amount`                            | substance exponent          |
|  [23]   | `BaseDimensions.LuminousIntensity`                 | luminous-intensity exponent |
|  [24]   | `BaseUnits.Length`                                 | length-unit selector        |
|  [25]   | `BaseUnits.Mass`                                   | mass-unit selector          |
|  [26]   | `BaseUnits.Time`                                   | time-unit selector          |
|  [27]   | `BaseUnits.Current`                                | current-unit selector       |
|  [28]   | `BaseUnits.Temperature`                            | temperature-unit selector   |
|  [29]   | `BaseUnits.Amount`                                 | substance-unit selector     |
|  [30]   | `BaseUnits.LuminousIntensity`                      | luminous-intensity selector |
|  [31]   | `BaseUnits.IsFullyDefined`                         | policy-completeness test    |
|  [32]   | `BaseUnits.IsSubsetOf(BaseUnits)`                  | policy-subset test          |
|  [33]   | `BaseUnits.Undefined`                              | undefined policy            |

[ENTRYPOINT_SCOPE]: typed quantity construction, conversion, and operators
- rail: units
- surface-root: per-quantity struct (e.g. `Length`)

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                |
| :-----: | :------------------------------------------- | :-------------------------- |
|  [01]   | `Length.FromMeters(QuantityValue)`           | per-unit construction       |
|  [02]   | `new Length(double, LengthUnit)`             | explicit-unit construction  |
|  [03]   | `Length.FromFeetInches(double, double)`      | composite construction      |
|  [04]   | `Length.FeetInches`                          | composite projection        |
|  [05]   | `Length.As(LengthUnit) -> double`            | scalar unit projection      |
|  [06]   | `Length.As(UnitSystem) -> double`            | scalar policy projection    |
|  [07]   | `Length.ToUnit(LengthUnit)`                  | quantity reprojection       |
|  [08]   | `Length.ToUnit(LengthUnit, UnitConverter)`   | converter reprojection      |
|  [09]   | `Length.ToUnit(UnitSystem)`                  | policy reprojection         |
|  [10]   | `Length.Meters -> double`                    | per-unit projection         |
|  [11]   | `operator +(Length, Length)`                 | quantity addition           |
|  [12]   | `operator -(Length, Length)`                 | quantity subtraction        |
|  [13]   | `operator -(Length)`                         | additive inversion          |
|  [14]   | `operator *(Length, double)`                 | right-scalar multiplication |
|  [15]   | `operator *(double, Length)`                 | left-scalar multiplication  |
|  [16]   | `operator /(Length, double)`                 | scalar division             |
|  [17]   | `operator /(Length, Length) -> double`       | quantity ratio              |
|  [18]   | `operator *(Length, Length) -> Area`         | area derivation             |
|  [19]   | `operator /(Length, Duration) -> Speed`      | speed derivation            |
|  [20]   | `operator *(Mass, Acceleration) -> Force`    | force derivation            |
|  [21]   | `operator /(Force, Area) -> Pressure`        | pressure derivation         |
|  [22]   | `operator *(Power, Duration) -> Energy`      | energy derivation           |
|  [23]   | `operator /(Energy, Duration) -> Power`      | power derivation            |
|  [24]   | `operator *(Force, Length) -> Torque`        | torque derivation           |
|  [25]   | `operator <(Length, Length)`                 | ordered comparison          |
|  [26]   | `operator <=(Length, Length)`                | ordered comparison          |
|  [27]   | `operator >(Length, Length)`                 | ordered comparison          |
|  [28]   | `operator >=(Length, Length)`                | ordered comparison          |
|  [29]   | `operator ==(Length, Length)`                | strict equality             |
|  [30]   | `operator !=(Length, Length)`                | strict inequality           |
|  [31]   | `Length.CompareTo(Length)`                   | quantity ordering           |
|  [32]   | `Length.Equals(Length)`                      | typed equality              |
|  [33]   | `Length.ToString()`                          | default rendering           |
|  [34]   | `Length.ToString(string?, IFormatProvider?)` | culture-aware rendering     |
|  [35]   | `Length.Zero`                                | additive identity           |
|  [36]   | `Length.BaseUnit`                            | base-unit metadata          |
|  [37]   | `Length.Units`                               | unit vocabulary             |
|  [38]   | `Length.Info`                                | quantity metadata           |
|  [39]   | `Length.BaseDimensions`                      | dimension metadata          |
|  [40]   | `Length.DefaultConversionFunctions`          | conversion registry         |

[ENTRYPOINT_SCOPE]: typed and dynamic parsing
- rail: units

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------------ | :-------------------------------- |
|  [01]   | `Length.Parse(string, IFormatProvider?)`                                                    | typed parse                       |
|  [02]   | `Length.TryParse(string?, IFormatProvider?, out Length)`                                    | guarded typed parse               |
|  [03]   | `Length.TryParseUnit(string, IFormatProvider?, out LengthUnit)`                             | unit-token parse                  |
|  [04]   | `Quantity.Parse(IFormatProvider?, Type, string)`                                            | boxed parse                       |
|  [05]   | `Quantity.TryParse(Type, string, out IQuantity?)`                                           | guarded boxed parse               |
|  [06]   | `Quantity.TryParse(IFormatProvider?, Type, string, out IQuantity?)`                         | guarded culture-scoped boxed parse |
|  [07]   | `Quantity.From(QuantityValue, Enum)`                                                        | enum-keyed construction           |
|  [08]   | `Quantity.From(QuantityValue, string quantityName, string unitName)`                        | name-keyed construction           |
|  [09]   | `Quantity.FromUnitAbbreviation(IFormatProvider?, QuantityValue, string)`                    | abbreviation construction         |
|  [10]   | `Quantity.TryFromUnitAbbreviation(IFormatProvider?, QuantityValue, string, out IQuantity?)` | guarded abbreviation construction |
|  [11]   | `Quantity.TryFrom(QuantityValue, Enum?, out IQuantity?)`                                    | guarded enum construction         |
|  [12]   | `Quantity.TryFrom(double, Enum, out IQuantity?)`                                            | guarded double construction       |
|  [13]   | `Quantity.FromQuantityInfo(QuantityInfo, QuantityValue)`                                    | metadata construction             |
|  [14]   | `UnitParser.Default.Parse<TUnit>(string, IFormatProvider?)`                                 | unit-enum parse                   |
|  [15]   | `UnitParser.Default.TryParse<TUnit>(string?, IFormatProvider?, out TUnit)`                  | guarded unit-enum parse           |
|  [16]   | `UnitParser.Default.TryParse(string?, Type, IFormatProvider?, out Enum?)`                   | guarded runtime-type unit parse   |

[ENTRYPOINT_SCOPE]: conversion, aggregation, metadata, and setup
- rail: units

| [INDEX] | [SURFACE]                                                                                        | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `UnitConverter.Convert(QuantityValue, Enum, Enum) -> double`                                     | unboxed dynamic conversion  |
|  [02]   | `UnitConverter.TryConvert(QuantityValue, Enum, Enum, out double)`                                | guarded dynamic conversion  |
|  [03]   | `UnitConverter.Default.GetConversionFunction<TQuantity>(Enum, Enum) -> ConversionFunction`       | cached conversion delegate  |
|  [04]   | `UnitConverter.Default.TryGetConversionFunction<TQuantity>(Enum, Enum, out ConversionFunction?)` | guarded delegate resolution |
|  [05]   | `UnitMath.Sum<TQuantity>(IEnumerable<TQuantity>, Enum)`                                          | chosen-unit sum             |
|  [06]   | `UnitMath.Sum<TSource, TQuantity>(IEnumerable<TSource>, Func<TSource, TQuantity>, Enum)`         | selector-based sum          |
|  [07]   | `UnitMath.Min<TQuantity>(IEnumerable<TQuantity>, Enum)`                                          | chosen-unit minimum         |
|  [08]   | `UnitMath.Max<TQuantity>(IEnumerable<TQuantity>, Enum)`                                          | chosen-unit maximum         |
|  [09]   | `UnitMath.Average<TQuantity>(IEnumerable<TQuantity>, Enum)`                                      | chosen-unit average         |
|  [10]   | `UnitMath.Min<TQuantity>(TQuantity, TQuantity)`                                                  | pairwise minimum            |
|  [11]   | `UnitMath.Max<TQuantity>(TQuantity, TQuantity)`                                                  | pairwise maximum            |
|  [12]   | `UnitMath.Clamp<TQuantity>(TQuantity, TQuantity, TQuantity)`                                     | bounded quantity            |
|  [13]   | `UnitMath.Abs<TQuantity>(TQuantity)`                                                             | absolute value              |
|  [14]   | `ToQuantityValue(this double)`                                                                   | double lift                 |
|  [15]   | `ToQuantityValue(this decimal)`                                                                  | decimal lift                |
|  [16]   | `BaseDimensions.Multiply(BaseDimensions)`                                                        | dimension multiplication    |
|  [17]   | `BaseDimensions.Divide(BaseDimensions)`                                                          | dimension division          |
|  [18]   | `operator *(BaseDimensions, BaseDimensions)`                                                     | multiplication operator     |
|  [19]   | `operator /(BaseDimensions, BaseDimensions)`                                                     | division operator           |
|  [20]   | `BaseDimensions.Dimensionless`                                                                   | dimension identity          |
|  [21]   | `BaseDimensions.IsBaseQuantity()`                                                                | base-quantity test          |
|  [22]   | `BaseDimensions.IsDerivedQuantity()`                                                             | derived-quantity test       |
|  [23]   | `BaseDimensions.IsDimensionless()`                                                               | dimensionless test          |
|  [24]   | `Quantity.Infos`                                                                                 | quantity metadata registry  |
|  [25]   | `Quantity.ByName`                                                                                | name-keyed registry         |
|  [26]   | `Quantity.GetUnitInfo(Enum)`                                                                     | unit metadata lookup        |
|  [27]   | `Quantity.GetQuantitiesWithBaseDimensions(BaseDimensions)`                                       | dimension-based discovery   |
|  [28]   | `QuantityInfo.GetUnitInfoFor(BaseUnits)`                                                         | policy unit lookup          |
|  [29]   | `QuantityInfo.GetUnitInfosFor(BaseUnits)`                                                        | policy unit enumeration     |
|  [30]   | `QuantityInfo.UnitInfos`                                                                         | unit metadata projection    |
|  [31]   | `QuantityInfo.BaseUnitInfo`                                                                      | base-unit projection        |
|  [32]   | `QuantityInfo.ValueType`                                                                         | scalar-type projection      |
|  [33]   | `QuantityInfo.BaseDimensions`                                                                    | dimension projection        |
|  [34]   | `UnitSystem.SI`                                                                                  | SI policy                   |
|  [35]   | `new UnitSystem(BaseUnits)`                                                                      | custom policy               |
|  [36]   | `UnitsNetSetup.Default`                                                                          | configured service root     |
|  [37]   | `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, string?, IFormatProvider?)`                   | explicit quantity rendering |

## [04]-[IMPLEMENTATION_LAW]

[UNIT_ALGEBRA]:
- Every quantity is a `readonly struct` implementing generic-math static-abstract interfaces; native operators preserve quantity identity across same-quantity, scalar, ratio, and cross-quantity algebra. Cross-quantity operators produce typed results at compile time, while a bare `double` carries no unit contract.
- `QuantityValue` preserves an admitted scalar as `double` or `decimal` until construction. `IValueQuantity<TValue>` fixes each quantity family's backing scalar, so the family rather than the input literal determines stored precision; primitive projection remains a boundary operation.
- Conversion dispatch follows input shape and reuse: typed structs convert directly, runtime-selected pairs route through `UnitConverter`, and repeated pairs reuse a `ConversionFunction`.
- `UnitSystem` projects policy through the units selected by `BaseUnits`, so a receipt declares one unit system for every quantity. `BaseDimensions` is the physical signature for derived-quantity discovery through `Quantity.GetQuantitiesWithBaseDimensions`.

[NAMED_IDENTITY]:
- `QuantityInfo.Name` is the stable quantity-type identity keyed by `Quantity.ByName`; boxed quantities expose it through `q.QuantityInfo.Name`.
- `QuantityInfo.Name` is the discriminator, while `BaseDimensions` is the physical signature for derived-dimension algebra and same-dimension validity.
- Same-dimension quantities remain distinct because dispatch uses `QuantityInfo.Name`, while `BaseDimensions` validates physical compatibility.
- A quantity outside the registry carries a consumer-minted name beside its composed `BaseDimensions`.
- `Quantity.From(value, quantityName, unitName)` admits registry-named quantities, while a consumer name remains the local discriminant beside its dimension.
- `q.Dimensions.IsDimensionless()` partitions SI reprojection. Dimensional quantities call `ToUnit(UnitSystem.SI)`; zero-vector quantities retain their constructed unit because unit-system reprojection raises `ArgumentException` when no unit matches the policy.

[LOCAL_ADMISSION]:
- Compute numeric inputs and receipts carry explicit quantity structs whenever units affect meaning; the quantity type carries unit identity with the scalar.
- Unit conversion is boundary rail policy, and culture-aware parse and format cross through `IFormatProvider` at the same boundary.
- Collection aggregation flows through `UnitMath`, which folds quantities in the selected unit without collapsing their type.
- Quantity metadata drives diagnostics, support output, and receipt projection; the `Quantity` and `IQuantity` façade is the reflection seam for a runtime-selected family.

[STACKING]:
- `Angle` bridges the `Rasm` geometry kernel: kernel rotation policy selects `AngleUnit.Radian`, so `Angle.FromDegrees(d).Radians` projects a degree input without reimplementing the conversion.
- `System.Text.Json` / Thinktecture-JSON (`api-thinktecture-json.md`): persist a measured receipt as `{ value, unit }` (the `IQuantity.Value` + `IQuantity.Unit` pair) and rehydrate through `Quantity.From(value, unitEnum)` — the dynamic façade is the deserialization entry. Never serialize the struct's private fields.
- Avalonia binding (`api-reactiveui-avalonia.md`): a view-model exposes `length.ToString(" mm", culture)` for display and parses user text through `Length.TryParse(text, culture, out _)`; an `IValueConverter` wrapping `As` and `Parse` is the XAML seam. `UnitAbbreviationsCache.Default` localizes culture-keyed abbreviations.
- `NodaTime` (`api-nodatime.md`): a `Duration` quantity (physical seconds) is distinct from a NodaTime `Duration` (a clock span) — keep the unit-bearing UnitsNet `Duration` for measured physical durations on receipts and the NodaTime type for wall-clock intervals; they meet only where a physical second is reinterpreted as elapsed time.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: typed quantity algebra, registry identity, boundary conversion, unit-system policy, and typed aggregation.
- Accept: unit-aware execution inputs and receipts; `QuantityInfo.Name` identity with `BaseDimensions` validation; boundary conversion; `UnitMath` aggregation; `{ value, unit }` wire projection.
- Reject: any rail that drops quantity identity before the boundary or dispatches solely on dimensions.
