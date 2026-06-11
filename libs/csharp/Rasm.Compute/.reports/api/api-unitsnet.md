# [RASM_COMPUTE_API_UNITSNET]

`UnitsNet` supplies physical quantities, unit conversion, parsing, and formatting for external-unit boundaries.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- assembly: `UnitsNet`
- namespace: `UnitsNet`
- asset: runtime library
- rail: units

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: unit family
- rail: units

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :--------------- | :---------------- | :------------------------ |
|   [1]   | `IQuantity`      | contract surface  | defines boundary contract |
|   [2]   | `QuantityValue`  | unit scalar       | normalizes unit value     |
|   [3]   | `Length`         | length quantity   | normalizes unit value     |
|   [4]   | `Mass`           | mass quantity     | normalizes unit value     |
|   [5]   | `Duration`       | time value        | records semantic time     |
|   [6]   | `Angle`          | angle quantity    | normalizes unit value     |
|   [7]   | `UnitConverter`  | codec surface     | defines codec path        |
|   [8]   | `QuantityParser` | quantity parser   | normalizes unit value     |
|   [9]   | `QuantityInfo`   | quantity metadata | normalizes unit value     |
|  [10]   | `UnitInfo`       | unit metadata     | normalizes unit value     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unit operations
- rail: units

| [INDEX] | [SURFACE]             | [CALL_SHAPE]     | [CAPABILITY]         |
| :-----: | :-------------------- | :--------------- | :------------------- |
|   [1]   | `From`                | quantity factory | creates quantity     |
|   [2]   | `ToUnit`              | unit conversion  | converts unit        |
|   [3]   | `As`                  | unit projection  | reads unit value     |
|   [4]   | `Parse`               | operation call   | executes operation   |
|   [5]   | `TryParse`            | lookup call      | resolves typed value |
|   [6]   | `GetUnitAbbreviation` | lookup call      | resolves typed value |
|   [7]   | `Convert`             | unit conversion  | converts unit value  |
|   [8]   | `Quantity.From`       | quantity factory | creates quantity     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: physical-unit boundary values
- Accept: external units normalize at boundaries
- Reject: numeric values with unit strings

