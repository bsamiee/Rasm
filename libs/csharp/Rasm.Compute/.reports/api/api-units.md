# [RASM_COMPUTE_API_UNITS]

Unit APIs supply external physical-unit admission into Compute intent and measurement receipts.

## [1]-[SURFACES]

This table is a lookup by unit package.

| [INDEX] | [PACKAGE]  | [ASSEMBLY] | [LOCAL_RAIL] |
| :-----: | :--------- | :--------- | :----------- |
|   [1]   | `UnitsNet` | `UnitsNet` | units        |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY] | [NAMESPACE] | [USING]    | [API_LOCATOR] |
| :-----: | :--------- | :---------- | :--------- | :------------ |
|   [1]   | `UnitsNet` | `UnitsNet`  | `UnitsNet` | `.cache/nuget/packages/unitsnet/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]     | [ENTRY_SURFACE]          | [LOCAL_RAIL] |
| :-----: | :---------------- | :----------------------- | :----------- |
|   [1]   | quantity types    | physical-unit value      | units        |
|   [2]   | unit enums        | unit vocabulary          | units        |
|   [3]   | conversion APIs   | normalized measurement   | units        |
|   [4]   | parsing APIs      | boundary admission       | units        |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]              | [LOCAL_RAIL] | [REASON]             |
| :-----: | :-------------------- | :----------- | :------------------- |
|   [1]   | parallel scalar model | units        | UnitsNet owns units  |
