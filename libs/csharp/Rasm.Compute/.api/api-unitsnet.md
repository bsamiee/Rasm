# [RASM_COMPUTE_API_UNITSNET]

`UnitsNet` supplies quantity structs, unit enums, parsing, conversion,
formatting, metadata, dimensions, generic math, and unit-system policy for
measured execution inputs and receipts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- assembly: `UnitsNet`
- namespace: `UnitsNet`
- asset: runtime library
- rail: units

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts
- rail: units

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]     | [CAPABILITY]            |
| :-----: | :-------------------- | :----------------- | :---------------------- |
|  [01]   | `IQuantity`           | quantity contract  | defines quantity values |
|  [02]   | `IQuantity<TUnit>`    | quantity contract  | defines typed units     |
|  [03]   | `IValueQuantity`      | quantity contract  | exposes scalar value    |
|  [04]   | `IDecimalQuantity`    | quantity contract  | exposes decimal value   |
|  [05]   | `IArithmeticQuantity` | quantity contract  | supports arithmetic     |
|  [06]   | `QuantityValue`       | value carrier      | carries numeric value   |
|  [07]   | `BaseDimensions`      | dimension metadata | describes dimensions    |
|  [08]   | `BaseUnits`           | unit metadata      | describes base units    |

[PUBLIC_TYPE_SCOPE]: admitted quantity families
- rail: units

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE] | [CAPABILITY]            |
| :-----: | :------------- | :------------- | :---------------------- |
|  [01]   | `Length`       | quantity       | carries length values   |
|  [02]   | `Area`         | quantity       | carries area values     |
|  [03]   | `Volume`       | quantity       | carries volume values   |
|  [04]   | `Mass`         | quantity       | carries mass values     |
|  [05]   | `Duration`     | quantity       | carries duration values |
|  [06]   | `Speed`        | quantity       | carries speed values    |
|  [07]   | `Acceleration` | quantity       | carries acceleration    |
|  [08]   | `Force`        | quantity       | carries force values    |
|  [09]   | `Pressure`     | quantity       | carries pressure values |
|  [10]   | `Energy`       | quantity       | carries energy values   |
|  [11]   | `Power`        | quantity       | carries power values    |
|  [12]   | `Temperature`  | quantity       | carries temperature     |
|  [13]   | `Angle`        | quantity       | carries angular values  |
|  [14]   | `Torque`       | quantity       | carries torque values   |
|  [15]   | `Ratio`        | quantity       | carries ratio values    |

[PUBLIC_TYPE_SCOPE]: admitted unit enum families (`UnitsNet.Units`)
- rail: units

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE] | [CAPABILITY]                                   |
| :-----: | :----------------- | :------------- | :--------------------------------------------- |
|  [01]   | `LengthUnit`       | unit enum      | `Meter` canonical, `Millimeter` display        |
|  [02]   | `AreaUnit`         | unit enum      | `SquareMeter` canonical and display            |
|  [03]   | `VolumeUnit`       | unit enum      | `CubicMeter` canonical and display             |
|  [04]   | `MassUnit`         | unit enum      | `Kilogram` canonical and display               |
|  [05]   | `DurationUnit`     | unit enum      | `Second` canonical and display                 |
|  [06]   | `SpeedUnit`        | unit enum      | `MeterPerSecond` canonical and display         |
|  [07]   | `AccelerationUnit` | unit enum      | `MeterPerSecondSquared` canonical and display  |
|  [08]   | `ForceUnit`        | unit enum      | `Newton` canonical and display                 |
|  [09]   | `PressureUnit`     | unit enum      | `Pascal` canonical, `Kilopascal` display       |
|  [10]   | `EnergyUnit`       | unit enum      | `Joule` canonical, `KilowattHour` display      |
|  [11]   | `PowerUnit`        | unit enum      | `Watt` canonical and display                   |
|  [12]   | `TemperatureUnit`  | unit enum      | `Kelvin` canonical, `DegreeCelsius` display    |
|  [13]   | `AngleUnit`        | unit enum      | `Radian` canonical, `Degree` display           |
|  [14]   | `TorqueUnit`       | unit enum      | `NewtonMeter` canonical and display            |
|  [15]   | `RatioUnit`        | unit enum      | `DecimalFraction` canonical, `Percent` display |

[PUBLIC_TYPE_SCOPE]: parsing, metadata, and units
- rail: units

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :----------------------- | :----------------- | :----------------------- |
|  [01]   | `Quantity`               | dynamic quantity   | resolves quantity values |
|  [02]   | `QuantityParser`         | parser             | parses quantities        |
|  [03]   | `UnitParser`             | parser             | parses units             |
|  [04]   | `QuantityFormatter`      | formatter          | formats quantities       |
|  [05]   | `UnitConverter`          | converter          | converts units           |
|  [06]   | `UnitAbbreviationsCache` | abbreviation store | resolves abbreviations   |
|  [07]   | `QuantityInfo`           | metadata           | describes quantities     |
|  [08]   | `UnitInfo`               | metadata           | describes units          |
|  [09]   | `UnitSystem`             | unit policy        | groups unit policy       |
|  [10]   | `UnitsNetSetup`          | configuration root | configures unit services |
|  [11]   | `UnitMath`               | quantity math      | sums and aggregates      |
|  [12]   | `DefaultUnitAttribute`   | attribute          | declares default units   |
|  [13]   | `ConvertToUnitAttribute` | attribute          | declares conversion unit |
|  [14]   | `DisplayAsUnitAttribute` | attribute          | declares display unit    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity operations
- rail: units

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :------------------------------ | :-------------- | :-------------------- |
|  [01]   | `From`                          | factory call    | creates quantity      |
|  [02]   | `Quantity.FromUnitAbbreviation` | factory call    | creates dynamic value |
|  [03]   | `Parse`                         | parser call     | parses quantity text  |
|  [04]   | `TryParse`                      | parser call     | parses quantity text  |
|  [05]   | `As`                            | conversion call | converts unit value   |
|  [06]   | `ToUnit`                        | conversion call | converts unit value   |
|  [07]   | `ToString`                      | formatter call  | formats quantity text |
|  [08]   | `CompareTo`                     | comparison call | compares quantities   |
|  [09]   | `Equals`                        | equality call   | compares quantities   |
|  [10]   | `UnitMath`                      | math surface    | executes unit math    |

[ENTRYPOINT_SCOPE]: setup and metadata
- rail: units

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]      | [CAPABILITY]             |
| :-----: | :------------------------------- | :---------------- | :----------------------- |
|  [01]   | `UnitsNetSetup.Default`          | setup property    | provides setup root      |
|  [02]   | `UnitAbbreviationsCache.Default` | cache property    | provides abbreviations   |
|  [03]   | `Quantity.Infos`                 | metadata property | lists quantity metadata  |
|  [04]   | `UnitConverter.Convert`          | converter call    | converts numeric values  |
|  [05]   | `UnitParser.Parse`               | parser call       | resolves unit enum       |
|  [06]   | `QuantityFormatter.Format`       | formatter call    | writes quantity text     |
|  [07]   | `UnitMath.Sum`                   | aggregation call  | sums a quantity sequence |
|  [08]   | `IArithmeticQuantity` operators  | operator surface  | applies generic math     |

[ENTRYPOINT_SCOPE]: metadata, dimensions, and unit-system policy
- rail: units

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]       | [CAPABILITY]                                 |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------- |
|  [01]   | `Quantity.TryFrom`             | factory call       | creates dynamic value from numeric plus enum |
|  [02]   | `Info` (per-quantity static)   | metadata property  | exposes the family `QuantityInfo`            |
|  [03]   | `QuantityInfo.ValueType`       | metadata property  | names the quantity CLR type                  |
|  [04]   | `QuantityInfo.BaseDimensions`  | metadata property  | exposes the dimension vector                 |
|  [05]   | `IQuantity.Dimensions`         | metadata property  | exposes constructed-quantity dimensions      |
|  [06]   | `IQuantity.Unit`               | unit property      | names the constructed unit enum value        |
|  [07]   | `BaseDimensions.Multiply`      | dimension algebra  | composes dimension products                  |
|  [08]   | `BaseDimensions.Divide`        | dimension algebra  | composes dimension quotients                 |
|  [09]   | `BaseDimensions.Dimensionless` | dimension constant | names the zero-dimension vector              |
|  [10]   | `UnitSystem.SI`                | unit policy        | provides the SI base-unit system             |
|  [11]   | `UnitParser.Default`           | parser property    | provides the default unit parser             |
|  [12]   | `UnitConverter.TryConvert`     | converter call     | converts numeric values without throwing     |

## [04]-[IMPLEMENTATION_LAW]

[UNIT_ALGEBRA]:
- namespace: `UnitsNet`
- quantity root: generated quantity structs
- unit root: `UnitsNet.Units` enum families
- metadata root: `QuantityInfo` and `UnitInfo`
- conversion root: `UnitConverter`, quantity `As`, and quantity `ToUnit`

[QUANTITY_POLICY]:
- parsing: `QuantityParser` and `UnitParser`
- formatting: `QuantityFormatter` and `UnitFormatter`
- abbreviation policy: `UnitAbbreviationsCache`
- setup policy: `UnitsNetSetup`
- generic math: `IArithmeticQuantity<TSelf, TUnit, TValue>` operators per quantity struct, plus `UnitMath.Sum`

[LOCAL_ADMISSION]:
- Compute numeric inputs and receipts carry explicit quantities when units affect meaning.
- Unit conversion is rail policy and cannot be hidden inside numeric helper calls.
- Parsing and formatting are boundary operations, not internal numeric representation.
- Quantity metadata informs diagnostics, support output, and receipt projection.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: quantity and unit algebra
- Accept: measured unit-aware execution
- Reject: raw numeric unit comments
