# [TS_UI_API_CLASS_VARIANCE_AUTHORITY]

`cva` mints the declarative variant→class dispatch table the `token` plane styles components against: one `cva(base, config)` call owns a component's whole variant algebra and returns a `(props) => string` selector folding the matched classes through `cx` (which IS `clsx`). It resolves no Tailwind conflict, so a shipped selector binds the `twMerge` wrap; only `VariantProps` touches React, lifting the axis vocabulary into a component's prop type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `class-variance-authority`
- package: `class-variance-authority` (Apache-2.0)
- module: `.` barrel (`cva`, `cx`, and the prop-bridge types) and `./types` (the class-value and class-prop vocabulary)
- runtime: framework-agnostic at runtime; `VariantProps` is a React-typed compile-time utility only
- rail: token/variant-dispatch
- depends: `clsx`, re-exported as `cx` (`.api/clsx.md`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the variant vocabulary and the prop bridge

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                                       |
| :-----: | :------------------------ | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `VariantProps<Component>` | utility type     | lifts a `cva` selector's axis union into props and a `Schema`      |
|  [02]   | `ClassValue`              | union alias      | `= clsx.ClassValue`; every `variants[axis][value]` cell and `base` |
|  [03]   | `ClassProp`               | XOR union        | the `class` XOR `className` ad-hoc escape hatch on the selector    |
|  [04]   | `ClassPropKey`            | string union     | `"class" \| "className"` — the two accepted ad-hoc prop names      |
|  [05]   | `StringToBoolean<T>`      | conditional type | maps a `{ true, false }` axis to a `boolean` prop                  |
|  [06]   | `OmitUndefined<T>`        | conditional type | strips `undefined` from an extracted axis union                    |
|  [07]   | `CxOptions` / `CxReturn`  | clsx alias       | `Parameters`/`ReturnType` of `clsx`, so `cx` needs no import       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one polymorphic `cva` factory whose single `config` unifies every variant axis and cross-axis rule; `cx` surfaces `clsx` so a `cva` module needs no second import.

| [INDEX] | [SURFACE]                 | [SHAPE]  | [CAPABILITY]                                                           |
| :-----: | :------------------------ | :------- | :--------------------------------------------------------------------- |
|  [01]   | `cva(base?, config?)`     | factory  | the per-component selector `(props) => string`, feeding `twMerge`      |
|  [02]   | `config.variants`         | property | `Record<Axis, Record<Value, ClassValue>>`, the variant→class map       |
|  [03]   | `config.defaultVariants`  | property | classes applied when a prop omits that axis                            |
|  [04]   | `config.compoundVariants` | property | `(ConfigVariants \| ConfigVariantsMulti) & ClassProp` cross-axis rules |
|  [05]   | `cx`                      | fold     | `= typeof clsx`; the `clsx` fold imported from `cva`                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `cva`'s selector folds in order — `base`, matched `variants`, matched `compoundVariants`, then `props.class`/`className` — all through `cx`; a boolean axis is a `{ true, false }` map `StringToBoolean` types as a `boolean` prop.
- `compoundVariants` collapses axis combinations into one row: a row matches one value per axis (`ConfigVariants`) or an array (`ConfigVariantsMulti`), owning a set of combinations rather than one row per combination.
- `cva` concatenates through `cx` and resolves no Tailwind conflict — two variants emitting conflicting utilities both survive the string, so last-wins correctness binds the `twMerge` wrap.

[STACKING]:
- `tailwind-merge` + `clsx` (`.api/tailwind-merge.md`, `.api/clsx.md`): the selector output flows into `cn = twMerge(clsx(...))` — `cva` selects, `clsx` folds, `twMerge` de-conflicts last; `extendTailwindMerge`/`fromTheme` teaches custom token groups to the resolver.
- `VariantProps` → `.api/react.md` + `effect` `Schema`: `VariantProps<typeof selector>` types a component's variant props, and a `Schema.Struct` of `Schema.Literal` axis fields (`Schema.Boolean` where `StringToBoolean` lifts a `{ true, false }` axis) decodes `wire`/config selections into `Props<T>`, the `wire`→`ui` styling seam carrying decoded values.
- `@radix-ui/react-slot` (`.api/radix-ui-react-slot.md`): apply `cn(variants(props))` to a `Slot` under `asChild`, sharing one variant table across every concrete tag the wrapper styles.
- `effect` `Match`: a residual `ClassValue` beyond the `compoundVariants` table computes through `Match.value(props).pipe(Match.when(...), Match.exhaustive)` folded through `cx`, reached only for cross-field logic the declarative table cannot express.

[LOCAL_ADMISSION]:
- `cva`'s variant table and selector only; conflict resolution rides `tailwind-merge`, folding rides `clsx`/`cx`, icon glyphs ride `lucide-react`.
- one `cva` selector per component surface — a new visual axis lands as a `variants` row and a new cross-axis rule as a `compoundVariants` row, never a second selector or a parallel class-name helper.
- `token` owner binds the `cn = twMerge(clsx(...))` wrap once and imports it everywhere, never inlined per component.

[RAIL_LAW]:
- Package: `class-variance-authority`
- Owns: the declarative `base`/`variants`/`defaultVariants`/`compoundVariants` dispatch table, its `(props) => string` selector, the `cx` fold re-export, and the `VariantProps` prop-type bridge
- Accept: one `cva` selector per component with axes as `variants` rows and cross-axis rules as `compoundVariants` (multi-valued where they collapse combinations), `VariantProps` feeding React props and a `Schema.Literal` axis union, the mandatory `twMerge(clsx(...))` wrap, `cx` as the in-module fold, `Slot` for `asChild` styling
- Reject: catalog output treated as conflict-resolved, a hand-rolled variant-map plus ternary ladder, a second class helper beside `cva`, importing `clsx` separately when `cx` is present, one `compoundVariants` row per combination a multi-valued match collapses, per-component `cn` duplication
