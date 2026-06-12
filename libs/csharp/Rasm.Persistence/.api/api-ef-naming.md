# [RASM_PERSISTENCE_API_EF_NAMING]

`EFCore.NamingConventions` supplies EF Core naming-convention extensions and
convention services for relational model naming policy.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `EFCore.NamingConventions`
- package: `EFCore.NamingConventions`
- assembly: `EFCore.NamingConventions`
- namespace: `Microsoft.EntityFrameworkCore`
- asset: runtime library
- rail: schema-tooling

## [2]-[PUBLIC_TYPES]

[NAMING_TYPES]: naming-convention services
- rail: schema-tooling

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]    | [CAPABILITY]            |
| :-----: | :---------------------------- | :---------------- | :---------------------- |
|   [1]   | `NamingConventionsExtensions` | builder extension | admits naming policy    |
|   [2]   | `NamingConventionSetPlugin`   | convention plugin | applies model naming    |
|   [3]   | `INameRewriter`               | rewriter contract | rewrites identifiers    |
|   [4]   | `SnakeCaseNameRewriter`       | naming service    | writes snake case       |
|   [5]   | `LowerCaseNameRewriter`       | naming service    | writes lower case       |
|   [6]   | `UpperCaseNameRewriter`       | naming service    | writes upper case       |
|   [7]   | `CamelCaseNameRewriter`       | naming service    | writes camel case       |
|   [8]   | `UpperSnakeCaseNameRewriter`  | naming service    | writes upper snake case |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: naming policy operations
- rail: schema-tooling

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]      | [CAPABILITY]          |
| :-----: | :---------------------------------- | :---------------- | :-------------------- |
|   [1]   | `UseSnakeCaseNamingConvention`      | builder extension | applies naming policy |
|   [2]   | `UseLowerCaseNamingConvention`      | builder extension | applies naming policy |
|   [3]   | `UseUpperCaseNamingConvention`      | builder extension | applies naming policy |
|   [4]   | `UseCamelCaseNamingConvention`      | builder extension | applies naming policy |
|   [5]   | `UseUpperSnakeCaseNamingConvention` | builder extension | applies naming policy |

## [4]-[IMPLEMENTATION_LAW]

[NAMING_POLICY]:
- namespace: `Microsoft.EntityFrameworkCore`
- entry root: `DbContextOptionsBuilder`
- convention root: EF model conventions
- schema role: relational table, column, key, index, and constraint names

[LOCAL_ADMISSION]:
- Naming convention is schema policy and cannot hide inside provider-specific setup.
- Store profiles share one naming policy unless a profile explicitly overrides it.
- Generated migrations reflect naming policy as schema facts, not formatting preferences.

[RAIL_LAW]:
- Package: `EFCore.NamingConventions`
- Owns: relational naming convention policy
- Accept: unified schema naming
- Reject: hand-written provider naming patches
