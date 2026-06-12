# [RASM_COMPUTE_API_UNITSNET]

`UnitsNet` supplies quantity structs, unit enums, parsing, conversion,
formatting, metadata, dimensions, generic math, and unit-system policy for
measured execution inputs and receipts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- assembly: `UnitsNet`
- namespace: `UnitsNet`
- asset: runtime library
- rail: units

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts
- rail: units

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]     | [CAPABILITY]            |
| :-----: | :-------------------- | :----------------- | :---------------------- |
|   [1]   | `IQuantity`           | quantity contract  | defines quantity values |
|   [2]   | `IQuantity<TUnit>`    | quantity contract  | defines typed units     |
|   [3]   | `IValueQuantity`      | quantity contract  | exposes scalar value    |
|   [4]   | `IDecimalQuantity`    | quantity contract  | exposes decimal value   |
|   [5]   | `IArithmeticQuantity` | quantity contract  | supports arithmetic     |
|   [6]   | `QuantityValue`       | value carrier      | carries numeric value   |
|   [7]   | `BaseDimensions`      | dimension metadata | describes dimensions    |
|   [8]   | `BaseUnits`           | unit metadata      | describes base units    |

[PUBLIC_TYPE_SCOPE]: admitted quantity families
- rail: units

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE] | [CAPABILITY]            |
| :-----: | :------------- | :------------- | :---------------------- |
|   [1]   | `Length`       | quantity       | carries length values   |
|   [2]   | `Area`         | quantity       | carries area values     |
|   [3]   | `Volume`       | quantity       | carries volume values   |
|   [4]   | `Mass`         | quantity       | carries mass values     |
|   [5]   | `Duration`     | quantity       | carries duration values |
|   [6]   | `Speed`        | quantity       | carries speed values    |
|   [7]   | `Acceleration` | quantity       | carries acceleration    |
|   [8]   | `Force`        | quantity       | carries force values    |
|   [9]   | `Pressure`     | quantity       | carries pressure values |
|  [10]   | `Energy`       | quantity       | carries energy values   |
|  [11]   | `Power`        | quantity       | carries power values    |
|  [12]   | `Temperature`  | quantity       | carries temperature     |
|  [13]   | `Angle`        | quantity       | carries angular values  |
|  [14]   | `Torque`       | quantity       | carries torque values   |
|  [15]   | `Ratio`        | quantity       | carries ratio values    |

[PUBLIC_TYPE_SCOPE]: parsing, metadata, and units
- rail: units

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :----------------------- | :----------------- | :----------------------- |
|   [1]   | `Quantity`               | dynamic quantity   | resolves quantity values |
|   [2]   | `QuantityParser`         | parser             | parses quantities        |
|   [3]   | `UnitParser`             | parser             | parses units             |
|   [4]   | `QuantityFormatter`      | formatter          | formats quantities       |
|   [5]   | `UnitFormatter`          | formatter          | formats units            |
|   [6]   | `UnitConverter`          | converter          | converts units           |
|   [7]   | `UnitAbbreviationsCache` | abbreviation store | resolves abbreviations   |
|   [8]   | `QuantityInfo`           | metadata           | describes quantities     |
|   [9]   | `UnitInfo`               | metadata           | describes units          |
|  [10]   | `UnitSystem`             | unit policy        | groups unit policy       |
|  [11]   | `UnitsNetSetup`          | configuration root | configures unit services |
|  [12]   | `DefaultUnitAttribute`   | attribute          | declares default units   |
|  [13]   | `ConvertToUnitAttribute` | attribute          | declares conversion unit |
|  [14]   | `DisplayAsUnitAttribute` | attribute          | declares display unit    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity operations
- rail: units

| [INDEX] | [SURFACE]   | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :---------- | :-------------- | :-------------------- |
|   [1]   | `From`      | factory call    | creates quantity      |
|   [2]   | `FromUnit`  | factory call    | creates dynamic value |
|   [3]   | `Parse`     | parser call     | parses quantity text  |
|   [4]   | `TryParse`  | parser call     | parses quantity text  |
|   [5]   | `As`        | conversion call | converts unit value   |
|   [6]   | `ToUnit`    | conversion call | converts unit value   |
|   [7]   | `ToString`  | formatter call  | formats quantity text |
|   [8]   | `CompareTo` | comparison call | compares quantities   |
|   [9]   | `Equals`    | equality call   | compares quantities   |
|  [10]   | `UnitMath`  | math surface    | executes unit math    |

[ENTRYPOINT_SCOPE]: setup and metadata
- rail: units

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]      | [CAPABILITY]            |
| :-----: | :------------------------------- | :---------------- | :---------------------- |
|   [1]   | `UnitsNetSetup.Default`          | setup property    | provides setup root     |
|   [2]   | `UnitAbbreviationsCache.Default` | cache property    | provides abbreviations  |
|   [3]   | `Quantity.Infos`                 | metadata property | lists quantity metadata |
|   [4]   | `UnitConverter.Convert`          | converter call    | converts numeric values |
|   [5]   | `UnitParser.Parse`               | parser call       | resolves unit enum      |
|   [6]   | `QuantityFormatter.Format`       | formatter call    | writes quantity text    |
|   [7]   | `GenericMathExtensions`          | extension surface | applies generic math    |
|   [8]   | `DecimalGenericMathExtensions`   | extension surface | applies decimal math    |

## [4]-[IMPLEMENTATION_LAW]

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
