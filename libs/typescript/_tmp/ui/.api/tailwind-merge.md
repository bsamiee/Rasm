# [API_CATALOGUE] tailwind-merge

`tailwind-merge` resolves Tailwind CSS class conflicts at runtime by parsing each class into `{ modifiers, baseClassName, maybePostfixModifier }`, mapping the base to a class-group ID, and dropping earlier classes when a later class occupies the same group. `twMerge` is the zero-config entry; `twJoin` is a lean falsy-safe `clsx` with no conflict resolution; `extendTailwindMerge`/`createTailwindMerge` build configured or fully-replaced mergers; `fromTheme` + the `validators` predicate namespace author custom `classGroups`. The default config carries the full Tailwind v4 group/theme table (`DefaultClassGroupIds`, `DefaultThemeGroupIds`), so a merger must be configured to match a non-default `prefix` or a custom `@theme`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwind-merge`
- package: `tailwind-merge`
- version: `3.6.0`
- license: `MIT`
- module: dual ESM (`dist/bundle-mjs.mjs`) / CJS; types `dist/types.d.ts`; `./es5` subpath for legacy targets; `sideEffects: false`
- runtime: framework-agnostic (no peer); the default config tracks Tailwind CSS v4 group IDs — a custom `prefix`/`@theme` needs a matching `extendTailwindMerge`
- rail: styling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exported types (the importable surface)
- rail: styling

Only these names are exported; `ParsedClassName` is exported under the alias `ExperimentalParsedClassName`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [SHAPE]                                                                                         |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | `ClassNameValue`                | type alias    | `ClassNameArray \| string \| null \| undefined \| 0 \| 0n \| false` (accepts nested arrays + falsy) |
|  [02]   | `ClassValidator`                | type alias    | `(classPart: string) => boolean` — the `validators` predicate shape                            |
|  [03]   | `Config<C, T>`                  | interface     | full config `= ConfigStaticPart & ConfigGroupsPart<C, T>` (see [CONFIG_MODEL])                 |
|  [04]   | `ConfigExtension<C, T>`         | interface     | `Partial<ConfigStaticPart> & { override?, extend? }` passed to `extendTailwindMerge`           |
|  [05]   | `DefaultClassGroupIds`          | union         | every built-in class-group ID (`'p' \| 'm' \| 'bg-color' \| 'font-size' \| …` — v4 group table) |
|  [06]   | `DefaultThemeGroupIds`          | union         | built-in theme scales `'color' \| 'spacing' \| 'radius' \| 'shadow' \| 'font' \| …`            |
|  [07]   | `ExperimentalParseClassNameParam` | interface   | `{ className: string; parseClassName(className): ExperimentalParsedClassName }`                 |
|  [08]   | `ExperimentalParsedClassName`   | interface     | `{ modifiers, hasImportantModifier, baseClassName, maybePostfixModifierPosition, isExternal? }` (exported alias of `ParsedClassName`) |

## [03]-[CONFIG_MODEL]

[CONFIG_MODEL_SCOPE]: structural config shapes (part of `Config`, not separately importable)
- rail: styling

`Config` is `ConfigStaticPart & ConfigGroupsPart`; the sub-shapes below are structural — a merger reads them but a consumer never imports the names, so extension goes through `ConfigExtension`.

| [INDEX] | [FIELD / SHAPE]                     | [OWNER]           | [ROLE]                                                                    |
| :-----: | :---------------------------------- | :---------------- | :------------------------------------------------------------------------ |
|  [01]   | `cacheSize: number`                 | `ConfigStaticPart` | LRU memo size (default `500`; `<= 0` disables — SSR high-variety escape) |
|  [02]   | `prefix?: string`                   | `ConfigStaticPart` | mirrors the Tailwind `--prefix`; must equal the framework config          |
|  [03]   | `experimentalParseClassName?(param)` | `ConfigStaticPart` | override class parsing; `param.parseClassName` re-enters the default parser |
|  [04]   | `theme`                             | `ConfigGroupsPart` | `Record<ThemeGroupId, ClassGroup>` — scales referenced via `fromTheme`     |
|  [05]   | `classGroups`                       | `ConfigGroupsPart` | `Record<ClassGroupId, ClassGroup>`; a `ClassGroup` is `(string \| ClassValidator \| ThemeGetter \| ClassObject)[]` |
|  [06]   | `conflictingClassGroups`            | `ConfigGroupsPart` | `{ [id]: id[] }` — when the key group is present, listed groups are removed (e.g. `{ gap: ['gap-x','gap-y'] }`) |
|  [07]   | `conflictingClassGroupModifiers`    | `ConfigGroupsPart` | postfix-modifier conflicts (e.g. `{ 'font-size': ['leading'] }`)         |
|  [08]   | `orderSensitiveModifiers: string[]` | `ConfigGroupsPart` | modifiers whose relative order changes the target and must be preserved   |
|  [09]   | `postfixLookupClassGroups?`         | `ConfigGroupsPart` | groups re-resolved with their `/postfix` attached                        |
|  [10]   | `ThemeGetter`                       | callable          | `(theme) => ClassGroup & { isThemeGetter: true }` — produced by `fromTheme`, never imported by name |

## [04]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: merge and join
- rail: styling

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [SIGNATURE / BEHAVIOR]                                                     |
| :-----: | :----------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `twMerge(...classLists)`                    | primary merge  | `(...classLists: ClassNameValue[]) => string` — default-config conflict resolution |
|  [02]   | `twJoin(...classLists)`                     | join only      | `(...classLists: ClassNameValue[]) => string` — join, no conflict resolution (lean `clsx`) |

[ENTRYPOINT_SCOPE]: configuration factories
- rail: styling

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]  | [SIGNATURE / BEHAVIOR]                                                                |
| :-----: | :------------------------------------------------ | :-------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `extendTailwindMerge<AddCG, AddTG>(ext, ...rest)` | factory         | `ext: ConfigExtension \| CreateConfigSubsequent`, rest `CreateConfigSubsequent[]` → merger; extends the default config |
|  [02]   | `createTailwindMerge(createFirst, ...rest)`       | factory         | `(createFirst: () => AnyConfig, ...rest: ((c) => AnyConfig)[]) => merger` — builds from scratch, no default |
|  [03]   | `mergeConfigs(baseConfig, configExtension)`       | config composer | `(base: AnyConfig, ext: ConfigExtension) => AnyConfig` — **mutates `base`** and returns it |
|  [04]   | `getDefaultConfig()`                              | config reader   | returns the full frozen default `Config` (v4 group/theme table)                      |
|  [05]   | `fromTheme<AddTG>(key)`                            | theme ref       | `(key: DefaultThemeGroupIds \| AddTG) => ThemeGetter` — binds a group to a theme scale |

[ENTRYPOINT_SCOPE]: `validators` — the `ClassValidator` predicate namespace
- rail: styling

One namespace of `ClassValidator` predicates used as `classGroups` `ClassDefinition` entries; the full roster (25) partitions into three families — never a hand-picked subset.

| [INDEX] | [FAMILY]                    | [PREDICATES]                                                                                                    |
| :-----: | :-------------------------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | primitive / open scale      | `isNumber`, `isInteger`, `isFraction`, `isPercent`, `isTshirtSize`, `isNamedContainerQuery`, `isAny`, `isAnyNonArbitrary` |
|  [02]   | arbitrary `[...]`           | `isArbitraryValue`, `isArbitraryLength`, `isArbitrarySize`, `isArbitraryPosition`, `isArbitraryImage`, `isArbitraryNumber`, `isArbitraryShadow`, `isArbitraryWeight`, `isArbitraryFamilyName` |
|  [03]   | arbitrary variable `(...)`  | `isArbitraryVariable`, `isArbitraryVariableLength`, `isArbitraryVariableFamilyName`, `isArbitraryVariableImage`, `isArbitraryVariablePosition`, `isArbitraryVariableShadow`, `isArbitraryVariableSize`, `isArbitraryVariableWeight` |

## [05]-[IMPLEMENTATION_LAW]

[MERGE_TOPOLOGY]:
- resolution keys on class-group IDs from `getDefaultConfig().classGroups`; the last class in a group wins, and `conflictingClassGroups` removes cross-group predecessors.
- `cacheSize` defaults to `500` (LRU, up to ~2× in size); set `0` to disable in SSR contexts with high class variety.
- `prefix` must mirror the framework `--prefix`; a mismatched merger fails to recognise prefixed classes and stops resolving conflicts.
- `extend` merges into a built-in group; `override` replaces it; both ride `ConfigExtension`; `mergeConfigs` mutates its base, so clone before reuse.
- `experimentalParseClassName` and `orderSensitiveModifiers` are the advanced hooks for non-standard modifier grammars; `postfixLookupClassGroups` disambiguates `base/postfix` slashes.

[STACKING]:
- `interaction/command.md`: the ONE `cn = (...a: CxOptions) => twMerge(cx(...a))` recipe owner folds `class-variance-authority` `cx` (the `clsx` re-export producing a `ClassNameValue` string) through `twMerge` — `cva` variant rows resolve conflict-safe under one composition root, never a per-`.tsx` className soup and never a bare `clsx`.
- `theming/tokens.md`: the folder's Tailwind v4 `@theme` OKLCH tokens + custom `prefix` require a memoized `extendTailwindMerge({ prefix, extend: { theme, classGroups } })` instance so the merger recognises the token-backed scales — bind `fromTheme('color')`/`fromTheme('spacing')` for the perceptual scales and admit the `tw-animate-css` `animate-*` utilities as an `animate` group so enter/exit classes merge cleanly.
- `tailwindcss-react-aria-components`: the RAC `data-*` variant prefixes (`open:`, `selection-single:`) are ordinary modifiers on a class — `twMerge` resolves the base class inside those variants; keep those variant names out of `orderSensitiveModifiers` unless a specificity conflict demands it.

[LOCAL_ADMISSION]:
- `twMerge` is the default consumer entry; the project-local `cn` alias composes it with `cva`'s `cx` at the one composition root, never per component.
- build one `extendTailwindMerge` instance per config variant and memoize at module load — never re-create per render.
- author custom groups with `validators` predicates (`isArbitraryValue` for open scales, `isTshirtSize` for named steps) and `fromTheme` for theme-backed scales.

[RAIL_LAW]:
- package: `tailwind-merge`
- owns: runtime Tailwind class-conflict resolution, class joining, and the configured/custom merger factories
- accept: any `ClassNameValue` (nested arrays, falsy, `0n`); a `ConfigExtension` or a config-creator function
- reject: hand-rolled class-conflict dedupe against the resolved group table; a merger left on the default config when the framework uses a custom `prefix`/`@theme`; naming `ClassNameArray`/`TailwindMerge`/`ConfigStaticPart`/`ThemeGetter` as importable types (none are exported)
