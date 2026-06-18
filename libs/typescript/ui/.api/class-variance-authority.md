# [API_CATALOGUE] class-variance-authority

`class-variance-authority` provides `cva` for authoring type-safe variant-driven class-name factories and `VariantProps` for extracting variant prop types from a `cva` call. `cx` is a re-export of `clsx` for class joining without conflict resolution. The package owns the variant configuration model: `variants`, `defaultVariants`, and `compoundVariants`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `class-variance-authority`
- package: `class-variance-authority`
- module: `class-variance-authority`
- asset: runtime library
- rail: styling

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: variant factory types
- rail: styling

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                                      |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------------------- |
|   [1]   | `VariantProps<C>` | generic type  | extracts variant prop map from a `cva` component, omits `class`/`className` |
|   [2]   | `CxOptions`       | type alias    | `Parameters<typeof clsx>` — accepted by `cx`                                |
|   [3]   | `CxReturn`        | type alias    | `ReturnType<typeof clsx>` — string result of `cx`                           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: variant factory and join
- rail: styling

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------------- |
|   [1]   | `cva(base?, config?)`       | factory        | returns `(props?) => string` variant class-name resolver |
|   [2]   | `cx(...options: CxOptions)` | join           | `clsx` re-export; falsy-safe class joining               |

[ENTRYPOINT_SCOPE]: cva config shape
- rail: styling

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [RAIL]                                                               |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------------- |
|   [1]   | `config.variants`         | config field   | `Record<string, Record<string, ClassValue>>` — variant definitions   |
|   [2]   | `config.defaultVariants`  | config field   | `ConfigVariants<T>` — default values per variant key                 |
|   [3]   | `config.compoundVariants` | config field   | array of `(ConfigVariants<T> \| ConfigVariantsMulti<T>) & ClassProp` |

## [4]-[IMPLEMENTATION_LAW]

[CVA_TOPOLOGY]:
- `cva` infers variant types from the `config.variants` schema; `VariantProps<typeof myComponent>` extracts the prop union
- `base` is applied unconditionally; variant classes layer on top; `compoundVariants` apply when all their conditions match
- `defaultVariants` fill variant slots when the consumer passes `undefined`; they do not override explicit `null`
- `compoundVariants` accept a `class` or `className` field alongside variant conditions; multiple matching entries accumulate
- `cx` is `clsx` and carries no Tailwind conflict resolution; compose with `twMerge` for conflict-safe class merging

[LOCAL_ADMISSION]:
- Define one `cva` call per logical component variant surface; pass its result type through `VariantProps` at the component boundary.
- When combining with `tailwind-merge`, wrap the `cva` output in a `cn = (...args) => twMerge(cx(...args))` helper at the composition root, not inside individual component files.

[RAIL_LAW]:
- package: `class-variance-authority`
- owns: type-safe variant-driven class-name composition
- accept: `base` string, `variants`, `defaultVariants`, `compoundVariants` config shape
- reject: duplicating variant dispatch logic outside `cva`, or re-implementing `VariantProps` extraction manually
