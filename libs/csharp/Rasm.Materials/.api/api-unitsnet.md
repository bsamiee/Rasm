# [RASM_MATERIALS_API_UNITSNET]

`UnitsNet` supplies immutable quantity structs, unit enums, parsing, conversion, formatting, dimension metadata, generic-math aggregation, and unit-system policy for measured execution inputs and receipts. It is admitted IN-FOLDER through the Materials `MaterialUnits` boundary (`Appearance/photometric#PHOTOMETRIC`, `Properties/properties#PHYSICAL_PROPERTIES`) for the SI-base rescale plus the quantity family-membership gate — the strata-acyclic AEC-domain owns its own unit boundary and never reaches DOWN to the app-platform `Rasm.Compute` units owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- version: `5.75.0`
- license: MIT-0 (`licenses.nuget.org/MIT-0` — public-domain-equivalent, no attribution clause)
- assembly: `UnitsNet`
- namespace: `UnitsNet`, `UnitsNet.Units`, `UnitsNet.GenericMath`
- asset: runtime library (multi-TFM `net8.0` / `net9.0` / `netstandard2.0`; the consumer `net10.0` binds the `net9.0` asset — the `net8.0+` `GenericMath` static-abstract-interface surface is present. Per-culture `*.resources.dll` satellites ship under `lib/<tfm>/{fr-CA,nb-NO,ru-RU,zh-CN}/`; the in-Rhino ALC loads the managed `UnitsNet.dll` only. Determinism is a code invariant, not a build-config dependency: every parser/abbreviation surface takes an explicit `IFormatProvider?`, and a `null` provider defaults to `CultureInfo.CurrentCulture` (only the internal per-unit secondary degrade reaches the invariant culture), so the `MaterialUnits` boundary keeps determinism by passing `CultureInfo.InvariantCulture` explicitly to every parse/abbreviation call — it parses identically regardless of satellite presence or ambient `CurrentCulture` because it never relies on a `null`/`CurrentCulture`-defaulted lookup. Pinning `SatelliteResourceLanguages` is unnecessary for this boundary; it remains a build-config option the consuming photometric/properties pages own if they ever surface localized abbreviations.)
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
|  [17]   | `AreaMomentOfInertia` | quantity | second moment of area (mm⁴); `AreaMomentOfInertiaUnit.MeterToTheFourth` SI base — the section `MomentOfInertiaYy`/`Zz` (`Iyy`/`Izz`) carrier a `VividOrange.Sections` property exposes |

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
|  [17]   | `AreaMomentOfInertiaUnit` | unit enum | `MeterToTheFourth` canonical, `MillimeterToTheFourth` display (the `Iyy`/`Izz` section second-moment unit) |

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
|  [03]   | `Parse(string, IFormatProvider?)`    | parser call     | parses quantity text under an explicit culture |
|  [04]   | `TryParse(string, IFormatProvider?, out)` | parser call | parses quantity text under an explicit culture (non-throwing); the `MaterialUnits` boundary passes `CultureInfo.InvariantCulture` so parsing is satellite-independent |
|  [05]   | `As(Enum unit)` / `As(UnitSystem)` | conversion call | converts to a scalar in the named unit or the unit-system's unit         |
|  [06]   | `ToUnit(Enum unit)` / `ToUnit(UnitSystem)` | conversion call | converts to a quantity in the named unit or the unit-system's unit |
|  [07]   | `ToString` / `ToString(IFormatProvider)` | formatter call | formats quantity text                                                 |
|  [08]   | `CompareTo`                     | comparison call | compares quantities   |
|  [09]   | `Equals`                        | equality call   | compares quantities (with optional tolerance overload)                   |
|  [10]   | `UnitMath.Sum<T>` / `Average<T>` / `Min<T>` / `Max<T>` / `Abs<T>` / `Clamp<T>` | math surface | typed aggregation over `IEnumerable<T : IArithmeticQuantity>` — never a raw `double` reduce |
|  [11]   | per-unit `<Quantity>.From<Unit>(QuantityValue)` static factory family | factory call | the named-unit admit shortcut — `Length.FromMillimeters` / `Area.FromSquareMillimeters` / `Angle.FromDegrees` / `AreaMomentOfInertia.FromMillimetersToTheFourth`; the strongly-typed sibling of the generic `From(QuantityValue, Enum)` [01] the `MaterialUnits` edge constructs a quantity with |
|  [12]   | per-unit `<quantity>.<Unit>` value-readback property family (`-> double`) | conversion call | the named-unit egress shortcut `q.<Unit> => As(<Unit>)` — `Length.Millimeters` / `Area.SquareMillimeters` / `Volume.CubicMillimeters` / `Pressure.Megapascals` / `Force.Kilonewtons` / `Torque.KilonewtonMeters` / `Ratio.DecimalFractions` / `AreaMomentOfInertia.MillimetersToTheFourth`; the strongly-typed sibling of the generic `As(Enum)` [05] the edge reads the canonical SI scalar through |

[ENTRYPOINT_SCOPE]: setup and metadata
- rail: units

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]      | [CAPABILITY]                   |
| :-----: | :----------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `UnitsNetSetup.Default`                    | setup property    | provides setup root            |
|  [02]   | `UnitAbbreviationsCache.Default`           | cache property    | provides abbreviations         |
|  [03]   | `Quantity.Infos`                           | metadata property | lists quantity metadata        |
|  [04]   | `UnitConverter.Convert` / `ConvertByName` / `ConvertByAbbreviation` (+ `Try*`) | converter call | converts a scalar between units by enum, name, or abbreviation |
|  [05]   | `UnitConverter.CreateDefault` / `RegisterDefaultConversions` | converter setup   | builds/extends the conversion registry |
|  [06]   | `UnitParser.Default.Parse<TUnit>(string, IFormatProvider?)` / `TryParse<TUnit>(string, IFormatProvider?, out)` | parser call | resolves a unit enum under an explicit culture; a `null` provider resolves to `CultureInfo.CurrentCulture` first and only degrades per-unit to the invariant fallback when the current culture has no abbreviation, so the deterministic boundary passes `CultureInfo.InvariantCulture` explicitly rather than relying on `null` |
|  [07]   | `QuantityFormatter.Format`                 | formatter call    | writes quantity text           |
|  [08]   | `GenericMath.GenericMathExtensions.Sum<T>` / `Average<T>` | extension surface | `net8.0+` static-abstract-interface aggregation over `IAdditionOperators`/`IAdditiveIdentity`/`IDivisionOperators` quantities |
|  [09]   | `GenericMath.DecimalGenericMathExtensions` | extension surface | the `decimal`-quantity mirror of the `Sum`/`Average` aggregation (`net8.0+`) |

[ENTRYPOINT_SCOPE]: metadata, dimensions, and unit-system policy
- rail: units

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]       | [CAPABILITY]                                 |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------- |
|  [01]   | `Quantity.TryFrom`                  | factory call       | creates dynamic value from numeric plus enum                          |
|  [02]   | `Info` (per-quantity static)        | metadata property  | exposes the family `QuantityInfo<TUnit>` (e.g. `Illuminance.Info`)     |
|  [03]   | `QuantityInfo.ValueType`            | metadata property  | names the quantity CLR type                                           |
|  [04]   | `QuantityInfo.BaseDimensions`       | metadata property  | the family dimension vector (the family-membership reference)         |
|  [05]   | `IQuantity.Dimensions`              | metadata property  | the constructed quantity's dimension vector                           |
|  [06]   | `IQuantity.Unit`                    | unit property      | the constructed unit `Enum` value                                     |
|  [07]   | `BaseDimensions.Equals(other)`      | dimension predicate | the family-membership gate — `q.Dimensions.Equals(Fam.Info.BaseDimensions)` proves a measured row belongs to a quantity family |
|  [08]   | `BaseDimensions.IsDimensionless()`  | dimension predicate | tests the zero-dimension (ratio) vector                              |
|  [09]   | `BaseDimensions.Multiply` / `Divide` | dimension algebra  | composes dimension products and quotients                            |
|  [10]   | `BaseDimensions.Dimensionless`      | dimension constant | the zero-dimension vector                                            |
|  [11]   | `<Quantity>.BaseUnit`               | unit metadata      | the SI base unit the `MaterialUnits` rescale targets (e.g. `Illuminance.BaseUnit == IlluminanceUnit.Lux`) |
|  [12]   | `UnitSystem.SI`                     | unit policy        | the SI base-unit system; `As(UnitSystem.SI)`/`ToUnit(UnitSystem.SI)` coerce to SI base |
|  [13]   | `UnitParser.Default`                | parser property    | the default unit parser                                              |
|  [14]   | `UnitConverter.TryConvert`          | converter call     | converts a scalar between units without throwing (the `MaterialUnits.Coerce` SI-base rescale) |
|  [15]   | `UnitAbbreviationsCache.Default.GetUnitAbbreviations<TUnit>(unit, IFormatProvider?)` / `GetDefaultAbbreviation<TUnit>(unit, IFormatProvider?)` | abbreviation lookup | resolves a unit's abbreviation set under an explicit culture; absent a provider these default to `CurrentCulture` and may read a satellite, so the deterministic boundary passes `CultureInfo.InvariantCulture` |
|  [16]   | (internal) `UnitAbbreviationsCache.FallbackCulture` | abbreviation policy | `internal` constant `== CultureInfo.InvariantCulture`; the per-unit secondary degrade a non-invariant lookup falls back to — not a public surface, so the boundary must pass `CultureInfo.InvariantCulture` explicitly rather than read it |
|  [17]   | `IQuantity.QuantityInfo`            | metadata property  | the constructed quantity's `QuantityInfo` family descriptor (`Length.FromMeters(1).QuantityInfo`); the `Quantity.TryFrom`'d `typed.QuantityInfo.Name` the `MaterialUnits.Admit` family-membership check reads |
|  [18]   | `QuantityInfo.Name`                 | metadata property  | the family name string (e.g. `"Illuminance"`) — the `MaterialUnits.Admit` `typed.QuantityInfo.Name == family.Name` gate + the `UnitEvidence.Family` receipt token |
|  [19]   | `QuantityInfo.BaseUnitInfo`         | metadata property  | the `UnitInfo` of the family SI base unit (`Length.Info.BaseUnitInfo` is the `Meter` `UnitInfo`); `MaterialUnits` reads it to name the canonical rescale target |
|  [20]   | `UnitInfo.Value`                    | unit property      | the unit `Enum` the `UnitInfo` wraps — `BaseUnitInfo.Value` is the base-unit `Enum` fed to `As`/the SI-base rescale |

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
- abbreviation policy: `UnitAbbreviationsCache` — every accessor takes an explicit `IFormatProvider?` that defaults to `CurrentCulture` when `null` (the internal invariant `FallbackCulture` is only the per-unit secondary degrade); the `MaterialUnits` boundary passes `CultureInfo.InvariantCulture` explicitly to keep parse/lookup satellite- and `CurrentCulture`-independent
- setup policy: `UnitsNetSetup`
- generic math: `UnitsNet.GenericMath`

[LOCAL_ADMISSION]:
- Compute numeric inputs and receipts carry explicit quantities when units affect meaning.
- Unit conversion is rail policy and cannot be hidden inside numeric helper calls.
- Parsing and formatting are boundary operations, not internal numeric representation; the boundary passes `CultureInfo.InvariantCulture` to every `Parse`/`TryParse`/abbreviation call so admission is deterministic across the host's ambient culture and the loaded satellite set, never `CurrentCulture`-defaulted.
- Quantity metadata informs diagnostics, support output, and receipt projection.

[STACK]:
- in-folder boundary: `UnitsNet` is admitted ONLY through the Materials `MaterialUnits` owner, never crossed into an interior numeric signature (the `BOUNDARY_ADMISSION` law) and never reached DOWN to the app-platform `Rasm.Compute/Symbolic/units` owner (the strata-acyclic AEC-domain owns its own unit boundary). Conversion runs exactly once at admission; interior numerics are raw `double`.
- photometric seam: `photometric#PHOTOMETRIC` `MaterialUnits.Admit(Illuminance.Info, value, unit, …)` gates the illuminance row by `q.Dimensions.Equals(Illuminance.Info.BaseDimensions)`, rescales any sub/multiple to `Illuminance.BaseUnit` (`IlluminanceUnit.Lux`) through `UnitConverter.TryConvert`, and returns `Fin<UnitEvidence>` reading `evidence.CanonicalValue` — the membership gate and the SI-base rescale are one boundary call. The luminous↔radiometric divide (683 lm/W) is author-kernel, not a `UnitsNet` conversion.
- properties seam: `properties#PHYSICAL_PROPERTIES` coerces measured conductivity/specific-heat/thermal-transmittance to `ThermalConductivity.BaseUnit`/`SpecificEntropy.BaseUnit`/`HeatTransferCoefficient.BaseUnit` (the `WattPerMeterKelvin`/`JoulePerKilogramKelvin`/`WattPerSquareMeterKelvin` SI bases) through the same `MaterialUnits.Coerce` rescale; an aggregation over a measured-quantity series (e.g. layered-assembly resistance) folds through `UnitMath.Sum<T>`, never a raw-`double` accumulation that drops the unit.
- wire seam: a quantity that reaches the `interchange#MATERIAL_WIRE` projection serializes as its canonical SI-base scalar plus the unit `Enum` token (resolved through `Quantity.Infos`/`QuantityInfo`), not a localized formatted string — the TS/Python peers decode the SI scalar. The inverse path that admits a measured row from a text abbreviation (e.g. an IFC property string or `Quantity.FromUnitAbbreviation`) resolves the unit through `UnitParser.Default.Parse(abbr, CultureInfo.InvariantCulture)`, never the `CurrentCulture` default, so the same wire byte parses identically on every host regardless of which `*.resources.dll` satellites the runtime loaded.

[RAIL_LAW]:
- Package: `UnitsNet` 5.75.0 (MIT-0, multi-TFM, `net10.0` binds `net9.0`)
- Owns: the immutable quantity/unit algebra — quantity structs, `UnitsNet.Units` enums, parsing/formatting, `UnitConverter` rescale, `BaseDimensions` family-membership algebra, `UnitMath`/`GenericMath` typed aggregation, and `UnitSystem` SI policy
- Accept: measured unit-aware execution admitted through the `MaterialUnits` in-folder boundary; the `Dimensions.Equals(Info.BaseDimensions)` family gate plus the `BaseUnit` SI-base rescale
- Reject: raw numeric unit comments; a quantity type crossing into an interior signature; a reach DOWN to the Compute units owner; a raw-`double` reduce where `UnitMath` preserves the unit
