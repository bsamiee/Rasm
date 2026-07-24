# [TS_UI_API_TAILWIND_MERGE]

`tailwind-merge` resolves Tailwind class conflicts: `twMerge` parses composed `className`s into class groups and keeps the last utility of each conflicting group, so a caller override wins over a base style deterministically. It pairs with `clsx` (`.api/clsx.md`) as the folder's one class composer `cn(...i) = twMerge(clsx(i))`; `twJoin` is the parse-free concat for provably-non-conflicting inputs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwind-merge`
- package: `tailwind-merge` (MIT)
- module: ESM/CJS dual, `./es5` transpile subpath; sideEffects-clean, tree-shakes to the used functions
- runtime: isomorphic, zero-dependency; sits in the render path below the Effect boundary
- rail: the `token`/`view` styling plane — the merge half of the one `cn` composer every styled `view` row calls

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the input algebra `twMerge`/`twJoin` accept and the configuration algebra `extendTailwindMerge` layers.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [CAPABILITY]                                                                   |
| :-----: | :-------------------------------- | :---------------- | :----------------------------------------------------------------------------- |
|  [01]   | `ClassNameValue`                  | input union       | recursive `string`/falsy/array input; `clsx` output flows in, falsy dropped    |
|  [02]   | `ClassValidator`                  | class predicate   | `(classPart) => boolean` custom-group matcher; `validators` are ready-made     |
|  [03]   | `Config`                          | full config       | `cacheSize`, `prefix`, `theme`, `classGroups`, `conflictingClassGroups`        |
|  [04]   | `ConfigExtension`                 | config extension  | the `extendTailwindMerge` argument; `override` replaces a group, `extend` adds |
|  [05]   | `DefaultClassGroupIds`            | built-in id union | group/theme-scale id union; a custom group id type-checks at extension         |
|  [06]   | `ExperimentalParseClassNameParam` | parse hook shape  | pre-parse transform shape for a nonstandard class syntax                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the composition primitives (resolve vs concat), the custom-instance constructors for an extended theme, and the class-group building blocks.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------------------------------------------ |
|  [01]   | `twMerge(...ClassNameValue[]) -> string`               | fold     | parse + last-wins resolve; memoized — the merge half of `cn`        |
|  [02]   | `twJoin(...ClassNameValue[]) -> string`                | fold     | parse-free concat of truthy parts when they provably cannot collide |
|  [03]   | `extendTailwindMerge(ConfigExtension, ...) -> twMerge` | factory  | layer a `ConfigExtension` onto the default `cn` merger for `@theme` |
|  [04]   | `createTailwindMerge(() => Config, ...) -> twMerge`    | factory  | build a merger from a callback that replaces the default config     |
|  [05]   | `mergeConfigs(Config, ConfigExtension) -> Config`      | static   | fold a `ConfigExtension` into a `Config` (`override` then `extend`) |
|  [06]   | `getDefaultConfig() -> Config`                         | static   | the default theme-tuned config to inspect or spread                 |
|  [07]   | `fromTheme(themeGroupId) -> ThemeGetter`               | factory  | reference a theme scale so a custom group follows it                |
|  [08]   | `validators`                                           | property | ready-made class-part predicates a `classGroups` member reuses      |

- `validators`: `isArbitraryValue` `isArbitraryLength` `isArbitraryNumber` `isArbitrarySize` `isArbitraryPosition` `isArbitraryImage` `isArbitraryShadow` `isArbitraryWeight` `isArbitraryVariable*` `isNumber` `isInteger` `isPercent` `isFraction` `isTshirtSize` `isNamedContainerQuery` `isAny` `isAnyNonArbitrary`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- last-conflicting-wins over parsed class groups: `twMerge` splits each class into `(modifiers, group, value)` and keeps the last occurrence within a group; modifiers (`hover:`, `md:`, tw-rac variants like `selected:`) scope the conflict, so `hover:px-2 hover:px-4` resolves while `px-2 hover:px-4` do not. Results LRU-memoize on `cacheSize` (`0` disables).
- Group definitions encode Tailwind's default-theme scales; a `@theme` extension leaves the new utility unknown and it survives merge un-deduplicated — one `extendTailwindMerge` instance teaching the custom groups via `fromTheme`/`validators` is the only correct form.
- pure synchronous string→string below the Effect boundary — no promise, no throw, no rail lift; it runs inside render and never reaches domain code.

[STACKING]:
- `clsx` (`.api/clsx.md`): `clsx` folds conditional inputs into a flat string `twMerge` resolves — the ONE composer `cn(...i) = twMerge(clsx(i))`; neither runs alone on a composed override.
- `class-variance-authority` (`.api/class-variance-authority.md`): a `cva` variant result passes through `cn`/`twMerge` so a caller `className` override wins over the variant defaults.
- `tailwindcss` (`.api/tailwindcss.md`): the merge groups mirror the theme; a customized `@theme` teaches ONE `extendTailwindMerge` via `fromTheme`, and the Tailwind `prefix` sets `Config.prefix` so prefixed utilities parse.
- `tailwindcss-react-aria-components` (`.api/tailwindcss-react-aria-components.md`): tw-rac state variants (`selected:`, `pressed:`, `placement-bottom:`) are modifiers `twMerge` preserves as scope prefixes with no config change; only an order-sensitive interaction needs `orderSensitiveModifiers`.
- `tw-animate-css` (`.api/tw-animate-css.md`): its `animate-*`/`fade-*`/`slide-*` utilities dedupe as ordinary class groups; the extended-theme instance covers custom motion scales.
- `token`/`view` plane: the styling plane builds ONE shared `extendTailwindMerge` instance and every styled `view` row emits `className` through the `cn` rail.

[LOCAL_ADMISSION]:
- Compose every conflict-prone `className` through the ONE `cn`/`twMerge` rail.
- Build ONE `extendTailwindMerge` instance for the customized `@theme` (via `fromTheme` + `validators`), shared folder-wide; bare `twMerge` on custom utilities mis-resolves.
- Reserve `twJoin` for provably-non-conflicting fixed token strings; every collidable input takes `twMerge`.
- Route `cva` output through `twMerge` so caller overrides win.
- Set `prefix`/`cacheSize` on the shared config; `experimentalParseClassName` is an overlay escape hatch for nonstandard class syntax only.

[RAIL_LAW]:
- Package: `tailwind-merge`
- Owns: deterministic Tailwind class-conflict resolution — `twMerge` last-wins resolve, `twJoin` parse-free concat, the custom-instance constructors (`extendTailwindMerge`/`createTailwindMerge`/`mergeConfigs`/`getDefaultConfig`), `fromTheme`, and the `validators` class-part predicates
- Accept: the `cn = twMerge(clsx(...))` composer, one shared `extendTailwindMerge` teaching the custom `@theme`, `twJoin` for non-conflicting inputs, `cva` output routed through `twMerge`, tw-rac variants preserved as modifiers
- Reject: raw string concat/interpolation for conflict-prone classes, bare `twMerge` on custom utilities, a per-component or per-call config, `cva` output reaching the DOM unmerged, `experimentalParseClassName` treated as stable API
