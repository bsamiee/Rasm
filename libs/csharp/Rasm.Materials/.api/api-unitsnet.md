# [RASM_MATERIALS_API_UNITSNET]

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

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE] | [CAPABILITY]                |
| :-----: | :------------- | :------------- | :-------------------------- |
|  [01]   | `Length`       | quantity       | carries length values       |
|  [02]   | `Area`         | quantity       | carries area values         |
|  [03]   | `Volume`       | quantity       | carries volume values       |
|  [04]   | `Mass`         | quantity       | carries mass values         |
|  [05]   | `Duration`     | quantity       | carries duration values     |
|  [06]   | `Speed`        | quantity       | carries speed values        |
|  [07]   | `Acceleration` | quantity       | carries acceleration        |
|  [08]   | `Force`        | quantity       | carries force values        |
|  [09]   | `Pressure`     | quantity       | carries pressure values     |
|  [10]   | `Energy`       | quantity       | carries energy values       |
|  [11]   | `Power`        | quantity       | carries power values        |
|  [12]   | `Temperature`  | quantity       | carries temperature         |
|  [13]   | `Angle`        | quantity       | carries angular values      |
|  [14]   | `Torque`       | quantity       | carries torque values       |
|  [15]   | `Ratio`        | quantity       | carries ratio values        |
|  [16]   | `Density`      | quantity       | carries mass-density values |

[PUBLIC_TYPE_SCOPE]: admitted photometric and radiometric quantity families
- rail: units

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE] | [CAPABILITY]                                  |
| :-----: | :------------------ | :------------- | :-------------------------------------------- |
|  [01]   | `Illuminance`       | quantity       | `IlluminanceUnit.Lux` SI base                 |
|  [02]   | `Luminance`         | quantity       | `LuminanceUnit.CandelaPerSquareMeter` SI base |
|  [03]   | `LuminousFlux`      | quantity       | `LuminousFluxUnit.Lumen` SI base              |
|  [04]   | `LuminousIntensity` | quantity       | `LuminousIntensityUnit.Candela` SI base       |
|  [05]   | `Irradiance`        | quantity       | `IrradianceUnit.WattPerSquareMeter` SI base   |

[PUBLIC_TYPE_SCOPE]: admitted thermal and heat-transfer quantity families
- rail: units

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE] | [CAPABILITY]                                                   |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `ThermalConductivity`     | quantity       | `ThermalConductivityUnit.WattPerMeterKelvin` SI base           |
|  [02]   | `SpecificEntropy`         | quantity       | `SpecificEntropyUnit.JoulePerKilogramKelvin` SI base           |
|  [03]   | `HeatTransferCoefficient` | quantity       | `HeatTransferCoefficientUnit.WattPerSquareMeterKelvin` SI base |

[PUBLIC_TYPE_SCOPE]: admitted unit enum families (`UnitsNet.Units`)
- rail: units

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :----------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `LengthUnit`       | unit enum      | `Meter` canonical, `Millimeter` display                 |
|  [02]   | `AreaUnit`         | unit enum      | `SquareMeter` canonical and display                     |
|  [03]   | `VolumeUnit`       | unit enum      | `CubicMeter` canonical and display                      |
|  [04]   | `MassUnit`         | unit enum      | `Kilogram` canonical and display                        |
|  [05]   | `DurationUnit`     | unit enum      | `Second` canonical and display                          |
|  [06]   | `SpeedUnit`        | unit enum      | `MeterPerSecond` canonical and display                  |
|  [07]   | `AccelerationUnit` | unit enum      | `MeterPerSecondSquared` canonical and display           |
|  [08]   | `ForceUnit`        | unit enum      | `Newton` canonical and display                          |
|  [09]   | `PressureUnit`     | unit enum      | `Pascal` canonical, `Kilopascal` / `Megapascal` display |
|  [10]   | `EnergyUnit`       | unit enum      | `Joule` canonical, `KilowattHour` display               |
|  [11]   | `PowerUnit`        | unit enum      | `Watt` canonical and display                            |
|  [12]   | `TemperatureUnit`  | unit enum      | `Kelvin` canonical, `DegreeCelsius` display             |
|  [13]   | `AngleUnit`        | unit enum      | `Radian` canonical, `Degree` display                    |
|  [14]   | `TorqueUnit`       | unit enum      | `NewtonMeter` canonical and display                     |
|  [15]   | `RatioUnit`        | unit enum      | `DecimalFraction` canonical, `Percent` display          |
|  [16]   | `DensityUnit`      | unit enum      | `KilogramPerCubicMeter` canonical and display           |

[PUBLIC_TYPE_SCOPE]: admitted photometric and radiometric unit enum families (`UnitsNet.Units`)
- rail: units

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]                                                     |
| :-----: | :---------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `IlluminanceUnit`       | unit enum      | `Lux` canonical, `Kilolux` / `Millilux` display                  |
|  [02]   | `LuminanceUnit`         | unit enum      | `CandelaPerSquareMeter` canonical, `Nit` display                 |
|  [03]   | `LuminousFluxUnit`      | unit enum      | `Lumen` canonical and display                                    |
|  [04]   | `LuminousIntensityUnit` | unit enum      | `Candela` canonical and display                                  |
|  [05]   | `IrradianceUnit`        | unit enum      | `WattPerSquareMeter` canonical, `KilowattPerSquareMeter` display |

[PUBLIC_TYPE_SCOPE]: admitted thermal and heat-transfer unit enum families (`UnitsNet.Units`)
- rail: units

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE] | [CAPABILITY]                                                              |
| :-----: | :---------------------------- | :------------- | :------------------------------------------------------------------------ |
|  [01]   | `ThermalConductivityUnit`     | unit enum      | `WattPerMeterKelvin` canonical, `BtuPerHourFootFahrenheit` display        |
|  [02]   | `SpecificEntropyUnit`         | unit enum      | `JoulePerKilogramKelvin` canonical, `KilojoulePerKilogramKelvin` display  |
|  [03]   | `HeatTransferCoefficientUnit` | unit enum      | `WattPerSquareMeterKelvin` canonical, `WattPerSquareMeterCelsius` display |

[PUBLIC_TYPE_SCOPE]: parsing, metadata, and units
- rail: units

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :----------------------- | :----------------- | :----------------------- |
|  [01]   | `Quantity`               | dynamic quantity   | resolves quantity values |
|  [02]   | `QuantityParser`         | parser             | parses quantities        |
|  [03]   | `UnitParser`             | parser             | parses units             |
|  [04]   | `QuantityFormatter`      | formatter          | formats quantities       |
|  [05]   | `UnitFormatter`          | formatter          | formats units            |
|  [06]   | `UnitConverter`          | converter          | converts units           |
|  [07]   | `UnitAbbreviationsCache` | abbreviation store | resolves abbreviations   |
|  [08]   | `QuantityInfo`           | metadata           | describes quantities     |
|  [09]   | `UnitInfo`               | metadata           | describes units          |
|  [10]   | `UnitSystem`             | unit policy        | groups unit policy       |
|  [11]   | `UnitsNetSetup`          | configuration root | configures unit services |
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

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]      | [CAPABILITY]                   |
| :-----: | :----------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `UnitsNetSetup.Default`                    | setup property    | provides setup root            |
|  [02]   | `UnitAbbreviationsCache.Default`           | cache property    | provides abbreviations         |
|  [03]   | `Quantity.Infos`                           | metadata property | lists quantity metadata        |
|  [04]   | `UnitConverter.Convert`                    | converter call    | converts numeric values        |
|  [05]   | `UnitParser.Default.Parse`                 | parser call       | resolves unit enum             |
|  [06]   | `QuantityFormatter.Format`                 | formatter call    | writes quantity text           |
|  [07]   | `GenericMath.GenericMathExtensions`        | extension surface | applies generic math (net8.0+) |
|  [08]   | `GenericMath.DecimalGenericMathExtensions` | extension surface | applies decimal math (net8.0+) |

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

[PHOTOMETRIC_BASE_UNITS]:
- `Illuminance.BaseUnit` is `IlluminanceUnit.Lux`.
- `Luminance.BaseUnit` is `LuminanceUnit.CandelaPerSquareMeter`.
- `LuminousFlux.BaseUnit` is `LuminousFluxUnit.Lumen`.
- `LuminousIntensity.BaseUnit` is `LuminousIntensityUnit.Candela`.
- `Irradiance.BaseUnit` is `IrradianceUnit.WattPerSquareMeter`.
- The `photometric` author-kernel rescales measured photometric and radiometric inputs to these SI base units.

[THERMAL_BASE_UNITS]:
- `ThermalConductivity.BaseUnit` is `ThermalConductivityUnit.WattPerMeterKelvin`.
- `SpecificEntropy.BaseUnit` is `SpecificEntropyUnit.JoulePerKilogramKelvin`; the quantity carries specific-heat-capacity values at this base.
- `HeatTransferCoefficient.BaseUnit` is `HeatTransferCoefficientUnit.WattPerSquareMeterKelvin`.
- `ThermalConductivity.From`, `SpecificEntropy.From`, and `HeatTransferCoefficient.From` admit a `QuantityValue` plus the unit enum; the `physical-properties` author-kernel rescales measured conductivity, specific-heat, and thermal-transmittance inputs to these SI base units.

[QUANTITY_POLICY]:
- parsing: `QuantityParser` and `UnitParser`
- formatting: `QuantityFormatter` and `UnitFormatter`
- abbreviation policy: `UnitAbbreviationsCache`
- setup policy: `UnitsNetSetup`
- generic math: `UnitsNet.GenericMath`

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
