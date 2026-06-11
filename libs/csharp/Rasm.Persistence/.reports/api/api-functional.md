# [RASM_PERSISTENCE_API_FUNCTIONAL]

Functional APIs supply Persistence typed rails, generated shapes, policy values, and effect composition through the inherited workspace substrate.

## [1]-[SURFACES]

This table is a lookup by functional package.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `LanguageExt.Core`                | `LanguageExt.Core`                 | effects      |
|   [2]   | `Thinktecture.Runtime.Extensions` | `Thinktecture.Runtime.Extensions`  | generated    |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                        | [NAMESPACE]          | [USING]               | [API_LOCATOR] |
| :-----: | :-------------------------------- | :------------------- | :-------------------- | :------------ |
|   [1]   | `LanguageExt.Core`                | `LanguageExt`        | `LanguageExt`         | `.cache/nuget/packages/languageext.core/` |
|   [2]   | `LanguageExt.Core`                | `LanguageExt.Common` | `LanguageExt.Common`  | `.cache/nuget/packages/languageext.core/` |
|   [3]   | `Thinktecture.Runtime.Extensions` | `Thinktecture`       | `Thinktecture`        | `.cache/nuget/packages/thinktecture.runtime.extensions/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]         | [ENTRY_SURFACE]              | [LOCAL_RAIL] |
| :-----: | :-------------------- | :--------------------------- | :----------- |
|   [1]   | `Fin<T>`              | store operation result       | effects      |
|   [2]   | `Validation<Error,T>` | boundary validation          | effects      |
|   [3]   | `Option<T>`           | optional projection          | effects      |
|   [4]   | `[Union]`             | operation and receipt cases  | generated    |
|   [5]   | `[SmartEnum]`         | store profile policy         | generated    |
|   [6]   | `[ValueObject]`       | admitted keys and paths      | generated    |

## [4]-[REJECTED]

This table is a lookup by rejected API.

| [INDEX] | [REJECT]                | [LOCAL_RAIL] | [REASON]                  |
| :-----: | :---------------------- | :----------- | :------------------------ |
|   [1]   | `IRepository<T>` family | query        | query algebra owns calls  |
|   [2]   | provider service wrappers | provider   | store profile owns provider |
