# [TS_UI_API_CLASS_VARIANCE_AUTHORITY]

`class-variance-authority` (`cva`) is the declarative variant→class dispatch table the `token` plane authors component styling against: one `cva(base, config)` call owns a component's base classes, its `variants` axes, per-axis `defaultVariants`, and cross-axis `compoundVariants`, returning a `(props) => string` selector that folds the matched classes through `cx` (which IS `clsx`). It is framework-agnostic at runtime — only the `VariantProps` type utility touches React — and it resolves no Tailwind conflicts, so the shipped catalog-bound output MUST be wrapped in `tailwind-merge`'s `twMerge`. `cva` owns the variant table, `clsx`/`cx` folds, `twMerge` de-conflicts, and `VariantProps` bridges the axis vocabulary into a component's prop type: the four compose into the single `cn(variants(props))` styling rail, never four parallel mechanisms.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `class-variance-authority`
- package: `class-variance-authority` (Apache-2.0)
- deps: `clsx ^catalog` (re-exported as `cx`; `.api/clsx.md`)
- catalog-verdict: KEEP
- runtime: universal at runtime (framework-agnostic); `VariantProps` is a React-typed compile-time utility only
- exports: `.` (`cva`, `cx`, `VariantProps`, `CxOptions`, `CxReturn`), `./types` (the class-value + class-prop vocabulary)
- gap: catalog-bound does NOT merge Tailwind conflicts — the `twMerge` wrap is mandatory, not optional

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the variant vocabulary and the prop bridge
- rail: token/variant-dispatch
- `VariantProps<Component extends (...a: any) => any>` = `Omit<OmitUndefined<Parameters<Component>[0]>, "class" | "className">` lifts a `cva` selector's axis union into a component's prop type and `effect`'s `Schema`; `StringToBoolean` is why a `{ true, false }` variant axis accepts a `boolean` prop; `ClassProp` is the `class` XOR `className` union `{ class: ClassValue; className?: never } | { class?: never; className: ClassValue } | { class?: never; className?: never }` every generated selector accepts. These types are the contract a design page composes, not incidental exports.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                                             |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------------------------ |
|  [01]   | `VariantProps<Component>` | prop extractor    | lift a `cva` selector's axis union into a component's props + `Schema`          |
|  [02]   | `ClassValue`              | class input       | `= clsx.ClassValue`; every `variants[axis][value]` cell and `base`              |
|  [03]   | `ClassProp`               | XOR prop          | the ad-hoc class escape hatch on the selector + `compoundVariants`              |
|  [04]   | `ClassPropKey`            | prop key          | `"class" \| "className"` — the two accepted ad-hoc class prop names             |
|  [05]   | `StringToBoolean<T>`      | boolean-axis lift | `T extends "true"\|"false" ? boolean : T`; a `{ true, false }` axis → `boolean` |
|  [06]   | `OmitUndefined<T>`        | prop cleaner      | strips `undefined` from extracted axis unions                                   |
|  [07]   | `CxOptions` / `CxReturn`  | cx alias          | `Parameters`/`ReturnType` of `clsx`; re-exported so `cx` needs no import        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the dispatch factory and the fold re-export
- rail: token/variant-dispatch
- `cva<T>(base?: ClassValue, config?: Config<T>): (props?: Props<T>) => string` is one polymorphic factory: `base` alone, `base` + `variants`, `defaultVariants` for unspecified axes, and `compoundVariants` for cross-axis rules all flow through the single `config` shape. `cx` is the `clsx` fold surfaced so a `cva` module needs no second import.

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY]   | [CONSUMER_BOUNDARY]                                                        |
| :-----: | :------------------------ | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `cva(base?, config?)`     | dispatch factory | the per-component selector `(props) => string`; output feeds `twMerge`     |
|  [02]   | `config.variants`         | axis table       | `Record<Axis, Record<Value, ClassValue>>` — the variant→class map          |
|  [03]   | `config.defaultVariants`  | axis defaults    | `{ [Axis]?: Value }` — classes applied when a prop omits that axis         |
|  [04]   | `config.compoundVariants` | cross-axis rules | `(ConfigVariants \| ConfigVariantsMulti) & ClassProp` — several axes match |
|  [05]   | `cx`                      | fold re-export   | `= typeof clsx`; the `clsx` fold, imported from `cva` in a `cva` module    |

## [04]-[IMPLEMENTATION_LAW]

[DISPATCH_SEMANTICS]:
- the selector folds, in order, `base` → matched `variants` classes → matched `compoundVariants` classes → `props.class`/`props.className`, all through `cx`. A boolean axis is a `{ true, false }` variant map that `StringToBoolean` types as a `boolean` prop.
- `compoundVariants` rows carry a `ClassProp` payload and match either a single value per axis (`ConfigVariants`) or an array of values (`ConfigVariantsMulti`, e.g. `{ intent: ["primary","danger"], size: "lg", class: "..." }`) — one row owns a set of axis combinations, never one row per combination.
- no conflict resolution: `cva` concatenates via `cx`; two variants emitting conflicting Tailwind utilities both survive in the string. Correct last-wins requires the `twMerge` wrap. This is the single most load-bearing fact about catalog-bound.

[INTEGRATION_LAW]:
- Stack with `tailwind-merge` + `clsx` into the one `cn`: `const buttonVariants = cva(base, { variants, defaultVariants, compoundVariants }); const cn = (...i: ClassValue[]) => twMerge(clsx(i)); className={cn(buttonVariants({ intent, size }), props.className)}`. `cva` selects, `clsx` folds the selector output with ad-hoc classes, `twMerge` de-conflicts last. Register design-token utility groups with `extendTailwindMerge`/`fromTheme` so custom token classes participate in conflict resolution.
- Stack with `VariantProps` → React + `effect` `Schema`: `type ButtonVariants = VariantProps<typeof buttonVariants>` is the component's variant prop type; a `Schema.Struct` whose axis fields are `Schema.Literal("primary","secondary",...)` (and `Schema.Boolean` for boolean axes, aligned by `StringToBoolean`) decodes config-driven or `wire`-arriving variant selections into exactly `Props<T>`. This is the `wire`→`ui` styling seam: variant tokens are decoded values, not stringly guesses.
- Stack with `@radix-ui/react-slot`: apply the `cn(variants(props))` className to a `Slot` when `asChild`, so a `cva`-styled wrapper composes its classes onto any child element — the `view/primitive` polymorphic-element pattern shares one variant table across concrete tags.
- Stack with `effect` `Match` for the residual: when styling depends on cross-field logic beyond the `compoundVariants` table, compute a `ClassValue` with `Match.value(props).pipe(Match.when(...), Match.exhaustive)` and fold it through `cx` alongside the selector — but exhaust the declarative `variants`/`compoundVariants` table first; reach for `Match` only for what the table structurally cannot express.

[LOCAL_ADMISSION]:
- the variant table and its selector only; conflict resolution is `tailwind-merge`, folding is `clsx`/`cx`, icon glyphs are `lucide-react`.
- one `cva` selector per component surface; a new visual axis is a `variants` row, a new cross-axis rule is a `compoundVariants` row — never a second selector or a parallel class-name helper.
- the `cn = twMerge(clsx(...))` wrap is defined once at the `token` owner and imported; never inlined per component.

[RAIL_LAW]:
- Package: `class-variance-authority`
- Owns: the declarative `base`/`variants`/`defaultVariants`/`compoundVariants` dispatch table, its `(props) => string` selector, the `cx` fold re-export, and the `VariantProps` prop-type bridge
- Accept: one `cva` selector per component, axes as rows and cross-axis rules as `compoundVariants` (multi-valued where they collapse combinations), `VariantProps` feeding React props + a `Schema.Literal` axis union, the mandatory `twMerge(clsx(...))` wrap, `cx` as the in-module fold, `Slot` for `asChild` styling
- Reject: treating catalog-bound output as conflict-resolved (wrap `twMerge`), a hand-rolled variant-map + ternary ladder, a second class helper beside `cva`, importing `clsx` separately when `cx` is present, one `compoundVariants` row per combination where a multi-valued match collapses them, per-component `cn` duplication
