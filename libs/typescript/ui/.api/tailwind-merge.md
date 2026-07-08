# [TS_UI_API_TAILWIND_MERGE]

`tailwind-merge` is the class-string resolver that makes composed `className`s correct: when a base style, a `cva` variant, a tw-rac state variant, and a caller override all contribute Tailwind utilities, `twMerge` parses them into class groups and keeps the *last* of each conflicting group, so `twMerge("px-2 px-4")` is `"px-4"` and `twMerge("bg-red-500", condition && "bg-blue-500")` resolves to the intended winner. It pairs with `clsx` (`.api/clsx.md`) — `clsx` folds truthy conditional inputs into a flat string, `twMerge` resolves the conflicts in it — and that pair is the folder's single class composer, conventionally `cn(...i) = twMerge(clsx(i))`. The default config is tuned to Tailwind catalog-bound's default theme (`.api/tailwindcss.md`); the moment `token/theme`/`token/scale` extend the theme with custom scales via `@theme`, `twMerge` no longer recognizes those utilities as a conflict group, so the folder builds ONE `extendTailwindMerge` instance teaching it the custom groups through `fromTheme`, shared everywhere — a per-call config or a raw `twMerge` on custom utilities silently mis-resolves. `twJoin` is the conflict-free concatenation (no parsing, faster) for when inputs provably cannot collide.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwind-merge`
- package: `tailwind-merge`
- license: `MIT` (© Dany Castillo)
- module format: ESM/CJS dual (`import`→`bundle-mjs.mjs`, `require`→`bundle-cjs.js`); an `./es5` subpath ships an ES5 transpile; `sideEffects` clean, tree-shakes to the used functions
- runtime target: isomorphic, zero dependencies, tiny; runs in the render path so results are LRU-memoized (`cacheSize`)
- asset: TypeScript library shipping `.d.ts`; the `Config`/`ConfigExtension` generics carry the `DefaultClassGroupIds`/`DefaultThemeGroupIds` string unions, so a custom class-group id is type-checked at `extendTailwindMerge`
- tailwind target: catalog default config is tuned for Tailwind CSS catalog (the folder's `tailwindcss@4`); catalog's own default groups match catalog's default theme
- rail: the `token`/`view` styling plane — the merge half of the `cn` composer every styled `view` row calls
- not-Effect: pure synchronous string→string; no rail lift needed — it sits inside the render, below the Effect boundary entirely

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the input type and the configuration algebra
- rail: shapes
- `ClassNameValue` is the recursive input `twMerge`/`twJoin` accept (composes directly with `clsx`'s output). The `Config`/`ConfigExtension` family is the extension surface: `override` replaces a default group, `extend` adds to it, and the id unions keep custom group names honest at the type level.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------- |:---------------- |:--------------------------------------------------------------- |
| [01] | `ClassNameValue` (string \| null \| undefined \| 0 \| 0n \| false \| `ClassNameValue[]`) | input union | the variadic argument of `twMerge`/`twJoin`; `clsx(...)` output flows straight in; falsy parts are dropped |
| [02] | `ClassValidator` (`(classPart: string) => boolean`) | class predicate | a custom class-group member — matches an arbitrary value shape; the `validators.*` set are ready-made ones |
| [03] | `Config<ClassGroupIds, ThemeGroupIds>` | full config | `cacheSize`/`prefix`/`experimentalParseClassName` + `theme`/`classGroups`/`conflictingClassGroups`/`conflictingClassGroupModifiers`/`orderSensitiveModifiers` |
| [04] | `ConfigExtension<ClassGroupIds, ThemeGroupIds>` | config extension | the `extendTailwindMerge` argument — `{ override?, extend?, cacheSize?, prefix?, ... }` where `override`/`extend` carry `PartialConfigGroupsPart` |
| [05] | `DefaultClassGroupIds` / `DefaultThemeGroupIds` | built-in id union | the string-literal union of default group/theme-scale ids; extend it to add a custom group id type-safely |
| [06] | `ExperimentalParseClassNameParam` / `ExperimentalParsedClassName` | parse hook shape | `experimentalParseClassName({ className, parseClassName })` — pre-parse transform for a nonstandard class syntax |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two composition primitives — resolve vs concatenate
- rail: surfaces-and-dispatch
- `twMerge` parses and resolves conflicts (render-path, memoized); `twJoin` only concatenates truthy parts (no parse, faster). Both are variadic over `ClassNameValue`, so `clsx`'s conditional-object/array forms pass through unchanged — the choice is whether conflict resolution is needed, not a different input shape.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `twMerge(...classLists: ClassNameValue[]) → string` | resolve merge | THE merge half of `cn` — base + cva variants + tw-rac variants + override collapse to the last-wins set |
| [02] | `twJoin(...classLists: ClassNameValue[]) → string` | fast concat | conflict-free assembly (static token strings that cannot collide) — skips the parser; `clsx`-shaped inputs |

[ENTRYPOINT_SCOPE]: custom-instance construction for the extended theme
- rail: surfaces-and-dispatch
- When `@theme` adds scales, the default `twMerge` cannot resolve the new utilities — build ONE extended instance. `extendTailwindMerge` layers a `ConfigExtension` (or a `createConfig` callback) onto the default; `createTailwindMerge` builds fully from a callback; `mergeConfigs` composes config objects; `getDefaultConfig` is the base to spread.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `extendTailwindMerge(extension \| createConfig, ...createConfig[]) → typeof twMerge` | extend default | the folder's ONE custom `cn` merger — `{ extend: { theme, classGroups } }` teaching it the `@theme` scales |
| [02] | `createTailwindMerge(() => Config, ...ConfigExtension[]) → typeof twMerge` | build from base| a fully custom instance when replacing (not extending) the default config wholesale |
| [03] | `mergeConfigs(baseConfig, configExtension) → Config` | compose config | fold a `ConfigExtension` into a `Config` inside a `createConfig` callback (`override` then `extend` semantics) |
| [04] | `getDefaultConfig() → Config` | base config | the default catalog-bound-tuned config to inspect or spread when composing a custom one |

[ENTRYPOINT_SCOPE]: class-group building blocks for custom group definitions
- rail: system-apis
- The parts that define a custom class group: `fromTheme` references a theme scale so members follow it, and `validators.*` are the ready-made class-part predicates (arbitrary value/length/number/… detection) a group's member list uses instead of a hand-written regex.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `fromTheme(themeGroupId) → ThemeGetter` | theme ref | a class-group member that follows a theme scale — the bridge from `@theme` custom scales to a merge group |
| [02] | `validators` — 25 predicates: `isArbitraryValue`/`isArbitraryLength`/`isArbitraryNumber`/`isArbitrarySize`/`isArbitraryPosition`/`isArbitraryImage`/`isArbitraryShadow`/`isArbitraryWeight`/`isArbitraryVariable*`/`isNumber`/`isInteger`/`isPercent`/`isFraction`/`isTshirtSize`/`isNamedContainerQuery`/`isAny`/`isAnyNonArbitrary`/… | class predicate | custom-group member matchers — reuse over a hand-rolled regex when defining a `classGroups` entry |

## [04]-[IMPLEMENTATION_LAW]

[TWMERGE_TOPOLOGY]:
- last-conflicting-wins over parsed class groups: `twMerge` splits each class into `(modifiers, base group, value)`, and within one conflict group the last occurrence wins — modifiers (`hover:`, `md:`, and any tw-rac variant like `selected:`) scope the conflict, so `hover:px-2 hover:px-4` resolves but `px-2 hover:px-4` do not conflict. Results are LRU-memoized (`cacheSize`); `cacheSize: 0` disables it.
- `twJoin` is the parse-free sibling: it concatenates the truthy parts of its `ClassNameValue` inputs with no conflict resolution — the `clsx`-equivalent fold built into the same package, used when inputs provably cannot collide (a fixed token string) so the render path skips the parser.
- the default config is theme-coupled: the group definitions encode Tailwind catalog-bound's default scales. Extend the theme and the default `twMerge` treats the new utility as unknown (it survives merge un-deduplicated) — correctness requires teaching it, once, via `extendTailwindMerge`. `ConfigExtension.extend` adds to a group, `override` replaces one; `fromTheme` ties a custom group to a custom `@theme` scale; `validators.*` supply the member matchers.
- it is a pure synchronous string→string function below the Effect boundary: no promise, no throw in normal use, no rail lift — it runs inside the render, memoized, and never appears in domain code.

[INTEGRATION_LAW]:
- Stack with `clsx` (`.api/clsx.md`): `clsx` folds conditional inputs (`clsx("base", isActive && "on", { hidden: !open })`) into a flat string; `twMerge` resolves the conflicts in it. The folder's ONE composer is `cn(...i) = twMerge(clsx(i))` — `clsx` for the boolean fold, `twMerge` for last-wins; neither is called alone on a composed override.
- Stack with `class-variance-authority` (`.api/class-variance-authority.md`): `cva` maps variant props to class strings (its own `cx` is `clsx`-shaped). The `cva` result is passed *through* `cn`/`twMerge` so a caller `className` override wins over the variant defaults — `cva` selects the variant set, `twMerge` resolves it against overrides.
- Stack with `tailwindcss@4` (`.api/tailwindcss.md`): the merge groups mirror the theme. A customized `@theme` (custom spacing/color/radius scales in `token/scale`, `token/theme`) is taught to ONE `extendTailwindMerge` instance via `fromTheme`; the same `prefix` set in the Tailwind config is set on the merge `Config` so prefixed utilities parse.
- Stack with `tailwindcss-react-aria-components` (`.api/tailwindcss-react-aria-components.md`): tw-rac variants are *modifiers* (`selected:`, `pressed:`, `placement-bottom:`) — `twMerge` preserves them as scope prefixes with no config change; only an order-sensitive variant interaction needs `orderSensitiveModifiers`. So RAC-state styling composes through the same `cn` rail unchanged.
- Stack with `tw-animate-css` (`.api/tw-animate-css.md`): its `animate-*`/`fade-*`/`slide-*` utilities are class groups `twMerge` deduplicates like any other; an extended-theme instance covers custom motion scales from `token/scale`.

[LOCAL_ADMISSION]:
- Compose every conflict-prone `className` through the ONE `cn`/`twMerge` rail; never raw string concatenation or template interpolation that lets two utilities of the same group both survive.
- Build ONE `extendTailwindMerge` instance for the customized `@theme` (via `fromTheme` + `validators`) and share it folder-wide; never call bare `twMerge` on custom utilities and never construct a per-component config.
- Use `twJoin` only when inputs provably cannot conflict (fixed token strings); default to `twMerge` whenever a variant or override can collide.
- Pass `cva` output through `twMerge` so caller overrides win; never let a `cva` result reach the DOM unmerged when an override prop exists.
- Set `prefix`/`cacheSize` on the shared config, not per call; treat `experimentalParseClassName` as an overlay escape hatch, pinned by version, only for a nonstandard class syntax.

[RAIL_LAW]:
- Package: `tailwind-merge`
- Owns: deterministic Tailwind class-conflict resolution — `twMerge` (last-wins resolve) and `twJoin` (parse-free concat), the custom-instance constructors (`extendTailwindMerge`/`createTailwindMerge`/`mergeConfigs`/`getDefaultConfig`), and the class-group building blocks (`fromTheme` theme reference + the 25 `validators.*` class-part predicates)
- Accept: the `cn = twMerge(clsx(...))` composer, ONE shared `extendTailwindMerge` instance teaching the custom `@theme`, `twJoin` for provably-non-conflicting inputs, `cva` output routed through `twMerge`, tw-rac variants preserved as modifiers
- Reject: raw string concat/interpolation for conflict-prone classes, bare `twMerge` on custom utilities, a per-component or per-call config, `cva` output reaching the DOM unmerged, and treating `experimentalParseClassName` as stable API
