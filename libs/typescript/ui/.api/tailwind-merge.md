# [API_CATALOGUE] tailwind-merge

`tailwind-merge` resolves Tailwind CSS class-name conflicts at runtime by tracking class group membership and removing earlier classes when a later class in the same group is present. `twMerge` is the zero-config entry point; `extendTailwindMerge` and `createTailwindMerge` enable configuration extension or full replacement. `twJoin` is a class-joining primitive with no conflict resolution, equivalent to a lean `clsx`. Validator functions and `fromTheme` support custom `classGroups` definitions.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwind-merge`
- package: `tailwind-merge`
- namespace: `tailwind-merge`
- asset: runtime library
- rail: styling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: class-name value types
- rail: styling

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                                                              |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------------ |
|   [1]   | `ClassNameValue` | type alias    | `ClassNameArray \| string \| null \| undefined \| 0 \| 0n \| false` |
|   [2]   | `ClassNameArray` | type alias    | `readonly ClassNameValue[]`                                         |
|   [3]   | `ClassValidator` | type alias    | `(classPart: string) => boolean`                                    |
|   [4]   | `TailwindMerge`  | type alias    | `(...classLists: ClassNameValue[]) => string`                       |

[PUBLIC_TYPE_SCOPE]: configuration types
- rail: styling

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                                                                          |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------------------------------- |
|   [1]   | `Config<C, T>`          | interface     | full config: `ConfigStaticPart & ConfigGroupsPart<C,T>`                                         |
|   [2]   | `ConfigStaticPart`      | interface     | `{ cacheSize, prefix?, experimentalParseClassName? }`                                           |
|   [3]   | `ConfigExtension<C, T>` | interface     | `{ override?, extend? }` passed to `extendTailwindMerge`                                        |
|   [4]   | `DefaultClassGroupIds`  | type alias    | union of all built-in class group ID strings                                                    |
|   [5]   | `DefaultThemeGroupIds`  | type alias    | union of all built-in theme group ID strings                                                    |
|   [6]   | `ThemeGetter`           | interface     | `(theme) => ClassGroup & { isThemeGetter: true }`                                               |
|   [7]   | `ParsedClassName`       | interface     | `{ modifiers, hasImportantModifier, baseClassName, maybePostfixModifierPosition, isExternal? }` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: merge and join functions
- rail: styling

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :----------------------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `twMerge(...classLists: ClassNameValue[])` | primary merge  | conflict-resolving class join with default config |
|   [2]   | `twJoin(...classLists: ClassNameValue[])`  | join only      | class join with no conflict resolution            |

[ENTRYPOINT_SCOPE]: configuration factories
- rail: styling

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]  | [RAIL]                                              |
| :-----: | :------------------------------------------------ | :-------------- | :-------------------------------------------------- |
|   [1]   | `createTailwindMerge(createConfigFirst, ...rest)` | factory         | builds a `TailwindMerge` fn from a config creator   |
|   [2]   | `extendTailwindMerge(configExtension, ...rest)`   | factory         | extends the default config; returns `TailwindMerge` |
|   [3]   | `getDefaultConfig()`                              | config reader   | returns the full default configuration object       |
|   [4]   | `mergeConfigs(baseConfig, configExtension)`       | config composer | merges two configs into one `AnyConfig`             |
|   [5]   | `fromTheme<T>(key)`                               | theme ref       | creates a `ThemeGetter` for a given theme scale key |

[ENTRYPOINT_SCOPE]: class-part validators (for custom classGroups)
- rail: styling

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :----------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `validators.isNumber`          | predicate      | matches numeric class-part values          |
|   [2]   | `validators.isInteger`         | predicate      | matches integer class-part values          |
|   [3]   | `validators.isFraction`        | predicate      | matches fraction values like `1/2`         |
|   [4]   | `validators.isPercent`         | predicate      | matches percentage values                  |
|   [5]   | `validators.isTshirtSize`      | predicate      | matches `sm`, `md`, `lg`, `xl`, `2xl` etc. |
|   [6]   | `validators.isArbitraryValue`  | predicate      | matches `[...]` arbitrary value syntax     |
|   [7]   | `validators.isArbitraryLength` | predicate      | matches `[length:...]` arbitrary lengths   |
|   [8]   | `validators.isAny`             | predicate      | always returns `true`; open group          |

## [4]-[IMPLEMENTATION_LAW]

[MERGE_TOPOLOGY]:
- conflict resolution is based on class group IDs from `getDefaultConfig().classGroups`; last class in a group wins
- `cacheSize` defaults to `500`; set to `0` to disable memoization in SSR contexts with high class variety
- `prefix` mirrors the Tailwind CSS `--prefix` setting; must match the framework configuration
- `extendTailwindMerge({ extend: { classGroups: { ... } } })` adds groups; `override` replaces groups entirely
- `ConfigExtension.override` keys replace the built-in definition; `extend` keys merge into it
- `fromTheme(key)` binds a class group entry to a theme scale, enabling dynamic value matching via `ThemeGetter`

[LOCAL_ADMISSION]:
- `twMerge` is the default consumer entry point; assign to a project-local `cn` alias by composing with `cva` or `clsx`.
- Create one `extendTailwindMerge` instance per config variant and memoize at module load time; never re-create per render.
- When authoring custom class groups, prefer `validators.isArbitraryValue` for open-ended scales and `isTshirtSize` for named size steps.

[RAIL_LAW]:
- package: `tailwind-merge`
- owns: runtime Tailwind class conflict resolution and class joining
- accept: any `ClassNameValue` including arrays, falsy values, and `0n`
- reject: hand-rolling class-conflict deduplication logic against this package's resolved group table
