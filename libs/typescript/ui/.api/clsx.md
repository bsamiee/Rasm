# [TS_UI_API_CLSX]

`clsx` is the pure class-name concatenation primitive the `token` plane folds through: it flattens strings, numbers, bigints, nested arrays, and truthy-keyed object maps into one space-joined `string`, dropping every falsy input. It carries no framework dependency, no effect rail, and no Tailwind awareness — conflict resolution is `tailwind-merge`'s job, variant selection is `class-variance-authority`'s job, and `clsx` is the fold both compose over. `class-variance-authority` re-exports this exact function as `cx`, so a folder module imports `clsx` OR `cva`'s `cx`, never both. The single canonical `cn` = `twMerge(clsx(inputs))` is the one class rail every view row emits `className` through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `clsx`
- package: `clsx` (MIT)
- deps: none — zero-dependency, framework-agnostic, pure synchronous fold
- catalog-verdict: KEEP
- runtime: universal — no React/DOM coupling; runs at render, worker, or build with identical output
- exports: `.` (full — string/number/bigint/array/object folding), `./lite` (string-only fast path, object/array args ignored)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the class-value input algebra
- rail: token/class-fold
- `ClassValue` = `ClassArray | ClassDictionary | string | number | bigint | null | boolean | undefined` is the recursive input union every class-emitting surface types against; `class-variance-authority` aliases `ClassValue`/`CxOptions`/`CxReturn` straight off this namespace, so this is the shared vocabulary of the whole styling rail, not a clsx-local type.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]         | [CONSUMER_BOUNDARY]                                                  |
| :-----: | :-------------------------------------- | :-------------------- | :------------------------------------------------------------------- |
|  [01]   | `ClassValue`                            | recursive input union | the one input type of the class rail; `cva`'s `ClassValue` = this    |
|  [02]   | `ClassDictionary = Record<string, any>` | conditional map       | `{ "text-red-500": isError }` — key emitted when its value is truthy |
|  [03]   | `ClassArray = ClassValue[]`             | nested list           | arbitrary nesting flattened in one pass                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the fold and its two entry lanes
- rail: token/class-fold
- one polymorphic entry discriminating on argument shape — variadic values, nested arrays, and conditional object maps all fold through the same call. `clsx/lite` is the same signature narrowed to string arguments only (objects/arrays silently skipped), for hot paths that never pass conditional maps.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]   | [CONSUMER_BOUNDARY]                                                        |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `clsx(...inputs: ClassValue[]): string` | fold             | default + named export; the fold under `cn`; `cva.cx` is this              |
|  [02]   | `clsx/lite` → `(...inputs): string`     | string fast path | object/array args ignored; smaller + faster when inputs are always strings |

## [04]-[IMPLEMENTATION_LAW]

[FOLD_SEMANTICS]:
- one pass, falsy-dropping: `0`, `NaN`, `""`, `false`, `null`, `undefined` are dropped; every truthy string/number/bigint is space-joined; nested arrays recurse; object keys emit when their value is truthy. Output is deterministic and order-preserving.
- `clsx/lite` trades the object/array arms for size and speed: pass it only strings; a conditional map handed to `lite` is a silent no-op, which is the named defect.
- pure and boundary-free: no `Effect`, no async, no DOM read. It composes inside a render or a `derive` selector without a rail; wrap in `Effect.sync` only when it must sit inside an effectful pipeline, never by default.

[INTEGRATION_LAW]:
- Stack with `tailwind-merge` as the one `cn`: `const cn = (...i: ClassValue[]) => twMerge(clsx(i))`. `clsx` flattens and resolves conditionals; `twMerge` applies Tailwind last-wins conflict resolution over the joined result. This ordered pair is the single class rail — `clsx` never merges conflicts, `twMerge` never folds conditionals, so both are load-bearing.
- Stack with `class-variance-authority`: `cva`'s `cx` export IS `clsx` (`cx = clsx.clsx`), and `cva` folds `base`, variant classes, compound classes, and `props.class/className` through it. Import `cx` from `cva` inside a module that already uses `cva`; import `clsx` directly only in modules with no `cva` dependency. Never import both — that is a duplicate binding of one function.
- Stack with `tailwind-merge` `twJoin`: when a call site only concatenates strings with no conditional object map, `twJoin` is the merge-free equivalent and `clsx/lite` the fold-free equivalent; reserve full `clsx` for the conditional-map and nested-array cases that are its reason to exist.
- Stack with `effect` `Match`: drive a `ClassValue` from closed-family state with `Match.value(state).pipe(Match.when(...), Match.exhaustive)` returning the class fragment, then hand that fragment to `clsx`/`cx`; prefer `cva`'s declarative `variants` table for the common variant→class mapping and reserve `Match` for cross-field logic the table cannot express.

[LOCAL_ADMISSION]:
- the concatenation primitive only; never the conflict resolver (that is `tailwind-merge`) and never the variant table (that is `class-variance-authority`).
- a module holding a `cva` dependency uses `cx`; a module without one uses `clsx` — one class-fold binding per module.

[RAIL_LAW]:
- Package: `clsx`
- Owns: the falsy-dropping, order-preserving fold of `ClassValue` (string/number/bigint/array/conditional-map) into one class `string`, and the `ClassValue` input vocabulary the whole styling rail shares
- Accept: `clsx`/`cx` as the fold under `cn`, `ClassValue` as the class-input type, `clsx/lite`/`twJoin` for string-only hot paths, folding inside render or a `derive` selector with no rail
- Reject: expecting conflict resolution from `clsx` (compose `twMerge`), importing both `clsx` and `cva`'s `cx` in one module, passing conditional maps to `clsx/lite`, wrapping the pure fold in `Effect` by default, a hand-rolled `[a, b && c].filter(Boolean).join(" ")` reimplementation
