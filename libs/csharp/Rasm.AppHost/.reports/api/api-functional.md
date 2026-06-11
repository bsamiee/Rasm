# [RASM_APPHOST_API_FUNCTIONAL]

Functional APIs supply AppHost typed rails, generated shapes, policy values, and effect composition through the inherited workspace substrate.

## [1]-[SURFACES]

This table is a lookup by functional package.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `LanguageExt.Core`                | `LanguageExt.Core`                 | effects      |
|   [2]   | `Thinktecture.Runtime.Extensions` | `Thinktecture.Runtime.Extensions`  | generated    |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                        | [NAMESPACE]           | [USING]                | [API_LOCATOR] |
| :-----: | :-------------------------------- | :-------------------- | :--------------------- | :------------ |
|   [1]   | `LanguageExt.Core`                | `LanguageExt`         | `LanguageExt`          | `.cache/nuget/packages/languageext.core/` |
|   [2]   | `LanguageExt.Core`                | `LanguageExt.Common`  | `LanguageExt.Common`   | `.cache/nuget/packages/languageext.core/` |
|   [3]   | `LanguageExt.Core`                | `LanguageExt.Traits`  | `LanguageExt.Traits`   | `.cache/nuget/packages/languageext.core/` |
|   [4]   | `Thinktecture.Runtime.Extensions` | `Thinktecture`        | `Thinktecture`         | `.cache/nuget/packages/thinktecture.runtime.extensions/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]             | [ENTRY_SURFACE]               | [LOCAL_RAIL] |
| :-----: | :------------------------ | :---------------------------- | :----------- |
|   [1]   | `Fin<T>`                  | typed success or failure      | effects      |
|   [2]   | `Validation<Error,T>`     | accumulated boundary failure  | effects      |
|   [3]   | `Option<T>`               | explicit absence              | effects      |
|   [4]   | `Eff<T>`                  | effectful runtime operation   | effects      |
|   [5]   | `[Union]`                 | closed state families         | generated    |
|   [6]   | `[SmartEnum]`             | policy rows                   | generated    |
|   [7]   | `[ValueObject]`           | admitted value shape          | generated    |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]                 | [LOCAL_RAIL] | [REASON]                  |
| :-----: | :----------------------- | :----------- | :------------------------ |
|   [1]   | generic `IReceipt` ledger | receipts     | typed receipts own evidence |
|   [2]   | provider wrapper shell   | generated    | package surfaces are direct |
