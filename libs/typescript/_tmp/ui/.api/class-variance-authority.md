# [API_CATALOGUE] class-variance-authority

`class-variance-authority` (`cva`) is the type-safe variant-driven class-name factory: one `cva(base, config)` call compiles a `{ variants, defaultVariants, compoundVariants }` schema into a `(props) => string` resolver whose prop union is INFERRED from the schema, and `VariantProps<typeof factory>` re-extracts that union at a component boundary. `cx` is `clsx` re-exported for falsy-safe joining with no conflict resolution. The `./types` subpath owns the `ClassValue`/`ClassProp`/`StringToBoolean` primitives that make boolean variants and the mutually-exclusive `class`/`className` prop work. cva carries no Tailwind conflict resolution — it is the lower half of the one `cn = (...a) => twMerge(cx(...a))` recipe the whole `ui` library reads (`interaction/command.md#COMMAND_SURFACE`), with `tailwind-merge` `twMerge` the conflict-resolving upper half.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `class-variance-authority`
- package / version: `class-variance-authority` @ `0.7.1`
- license: `Apache-2.0`
- module: dual ESM `dist/index.mjs` + CJS `dist/index.js`; framework-agnostic, no React dependency
- exports: `.` → `cva` / `cx` / `VariantProps` / `CxOptions` / `CxReturn`; `./types` → `ClassValue` / `ClassProp` / `ClassPropKey` / `OmitUndefined` / `StringToBoolean`
- dependency: `clsx` (re-exported verbatim as `cx`) — a bare `clsx` import beside `cx` is the redundant seam
- rail: styling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: main-entry variant types (`.`)
- rail: styling

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [NOTE]                                                                          |
| :-----: | :---------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `VariantProps<C>` | generic type  | `Omit<OmitUndefined<Parameters<C>[0]>, "class" \| "className">` — variant prop union of a `cva` factory, `class`/`className` stripped |
|  [02]   | `CxOptions`       | type alias    | `Parameters<typeof clsx>` — the rest-arg tuple `cx`/`cn` accept                 |
|  [03]   | `CxReturn`        | type alias    | `ReturnType<typeof clsx>` (`string`) — the join result                         |

[PUBLIC_TYPE_SCOPE]: class-value primitives (`./types` subpath)
- rail: styling

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [NOTE]                                                                                 |
| :-----: | :------------------- | :----------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `ClassValue`         | type alias         | `clsx.ClassValue` — string, array, or truthy-keyed record accepted anywhere a class is |
|  [02]   | `ClassPropKey`       | union              | `"class" \| "className"`                                                              |
|  [03]   | `ClassProp`          | discriminated union| XOR of `{ class }` / `{ className }` / `{}` — a variant call passes ONE prop, never both |
|  [04]   | `StringToBoolean<T>` | mapped type        | `T extends "true" \| "false" ? boolean : T` — a `{ true, false }` variant key becomes a `boolean` prop, not a string literal |
|  [05]   | `OmitUndefined<T>`   | mapped type        | strips `undefined` from a factory parameter before `VariantProps` extraction           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory and join
- rail: styling

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [NOTE]                                                                              |
| :-----: | :-------------------------- | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `cva<T>(base?, config?)`     | factory        | `(base?: ClassValue, config?: Config<T>) => (props?: Props<T>) => string`; `T` inferred from `config.variants` |
|  [02]   | `cx(...inputs: CxOptions)`   | join           | `clsx` re-export — falsy-safe, conflict-UNAWARE class joining; returns `CxReturn`   |

[ENTRYPOINT_SCOPE]: `cva` config shape (`Config<T>`)
- rail: styling

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [NOTE]                                                                                                    |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `config.variants`         | schema field   | `Record<string, Record<string, ClassValue>>` — the variant axes; the resolver prop union is inferred from this |
|  [02]   | `config.defaultVariants`  | schema field   | `ConfigVariants<T>` — fills a slot when the caller passes `undefined`; does NOT override an explicit `null` |
|  [03]   | `config.compoundVariants` | schema field   | `((ConfigVariants<T> \| ConfigVariantsMulti<T>) & ClassProp)[]` — conditional classes when all conditions match; a condition value may be an ARRAY (`ConfigVariantsMulti`) to match several variant values at once |

## [04]-[IMPLEMENTATION_LAW]

[CVA_TOPOLOGY]:
- resolution order is `base` → matched `variants` → matched `compoundVariants` → the call's `class`/`className`; later strings append, they do NOT dedupe or resolve Tailwind conflicts (that is `twMerge`'s job)
- boolean variants are the `StringToBoolean` mechanism: declare `variants: { <axis>: { true: "…", false: "…" } }` and the prop types as `<axis>?: boolean` — a caller passes `<axis>={true}`, never `"true"`
- `ClassProp` enforces `class` XOR `className`: a variant call and every `compoundVariants` row may carry ONE of the two keys, never both
- `defaultVariants` fill on `undefined` only; passing `null` opts a slot out with no default and no variant class
- `ConfigVariantsMulti` lets one `compoundVariants` row match a set of values (`size: ["sm", "md"]`) instead of duplicating the row per value
- `VariantProps<typeof factory>` is the canonical way to lift the inferred prop union into a component's own props; never re-declare the variant keys by hand
- `cx` IS `clsx` — no added behavior; it exists so a component never imports `clsx` directly and the join half of `cn` reads from one package

[STACKING]:
- `cn` recipe (canonical): `const cn = (...a: CxOptions) => twMerge(cx(...a))` — cva owns join (`cx`) and variant compilation (`cva`), `tailwind-merge` `twMerge` owns conflict resolution; this composed owner is the single styling entry `ui` reads (`interaction/command.md#COMMAND_SURFACE`), mounted once at the composition root, never re-derived per `.tsx`
- variant boundary: a `cva` factory's output feeds a component `className`; `VariantProps<typeof factory>` becomes part of the component prop type, and `@radix-ui/react-slot` `Slot`/`Slottable` carry that `className` onto an `asChild` child so one recipe composes any element shape without a `cloneElement` hand-roll
- vocabulary keying: when a variant axis mirrors a domain vocabulary (a `CommandAction` role, a `ThemeTokens` scale step), key the `variants` record off the same literal union the effect-tier `Schema.Literal`/`Data.TaggedEnum` owner already closes (`interaction/command.md`, `theming/tokens.md`) — the variant axis is a projection of the domain vocabulary, not a parallel enum
- token resolution: variant class strings reference the live CSS custom properties `theming/tokens.md#THEME_TOKENS` `CssVarSync` writes (`bg-[--accent]`, `data-[state=open]:animate-in`), so a `cva` row is theme-reactive with no re-render and no JS color math in the recipe

[LOCAL_ADMISSION]:
- one `cva` call per logical component variant surface; extend an existing factory with a variant row before minting a second factory for the same component
- compose `cva` output through the single `cn` recipe owner at the composition root, never `twMerge(cva(...)())` re-stitched per file
- lift the prop union with `VariantProps`; never hand-maintain a parallel prop interface that restates the variant keys

[RAIL_LAW]:
- package: `class-variance-authority`
- owns: type-safe variant-driven class-name composition and falsy-safe join (`cx` = `clsx`)
- accept: `base` string, the `variants`/`defaultVariants`/`compoundVariants` schema, boolean variants via `{ true, false }` keys, multi-value compound conditions, `VariantProps` extraction
- reject: a bare `clsx` import beside `cx`; a per-`.tsx` className soup beside the one `cn` recipe; a hand-maintained prop interface duplicating `VariantProps`; Tailwind conflict resolution inside `cva` (that is `twMerge`)
