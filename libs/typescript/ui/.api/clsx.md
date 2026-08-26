# [TS_UI_API_CLSX]

`clsx` folds `ClassValue` — strings, numbers, bigints, nested arrays, and truthy-keyed object maps — into one space-joined class `string`, dropping every falsy input in a single pure pass. It carries no framework, effect, or Tailwind awareness: `tailwind-merge` owns conflict resolution, `class-variance-authority` owns variant selection, and `clsx` is the fold both compose over as `cn = twMerge(clsx(...))`, the one class path every `view` row emits `className` through.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `ClassValue` input algebra every class-emitting surface types against — `ClassValue = ClassArray | ClassDictionary | string | number | bigint | null | boolean | undefined`, aliased by `class-variance-authority` as `CxOptions`, so it is the shared styling vocabulary, not clsx-local.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]                                                                   |
| :-----: | :---------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `ClassValue`      | recursive union | the one class-path input type; `cva`'s `CxOptions` is its `Parameters`         |
|  [02]   | `ClassDictionary` | conditional map | `Record<string, any>`; `{ "text-red-500": isError }` emits the key when truthy |
|  [03]   | `ClassArray`      | nested list     | `ClassValue[]`; arbitrary nesting flattened in one pass                        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one polymorphic fold discriminating on argument shape; `clsx/lite` narrows the same signature to string arguments.

| [INDEX] | [SURFACE]                              | [SHAPE] | [CAPABILITY]                                                           |
| :-----: | :------------------------------------- | :------ | :--------------------------------------------------------------------- |
|  [01]   | `clsx(...ClassValue[]) -> string`      | fold    | default + named export; the fold under `cn`; `cva.cx` is this function |
|  [02]   | `clsx/lite(...ClassValue[]) -> string` | fold    | string-only fast path; object/array args silently ignored              |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one falsy-dropping pass: `0`, `NaN`, `""`, `false`, `null`, `undefined` drop; every truthy string/number/bigint space-joins; nested arrays recurse; object keys emit on a truthy value; output is deterministic, order-preserving, pure, and synchronous with no effect or DOM read.
- `clsx/lite` trades the object/array arms for size — a conditional map handed to `lite` is a silent no-op, the named footgun; reserve full `clsx` for the conditional-map and nested-array inputs that are its reason to exist.

[STACKING]:
- `tailwind-merge` (`.api/tailwind-merge.md`): the joined output flows into `twMerge` as the one `cn = twMerge(clsx(...))` — `clsx` folds conditionals, `twMerge` resolves Tailwind last-wins; for provably-non-conflicting string inputs `twJoin`/`clsx/lite` skip both parses.
- `class-variance-authority` (`.api/class-variance-authority.md`): `cva`'s `cx` export IS `clsx`, and `cva` folds `base`, variant, and compound classes through it — a `cva` module consumes the fold as `cx` with no separate `clsx` import.
- `effect` (`libs/typescript/.api/effect.md`): drive a `ClassValue` from closed-family state with `Match.value(state).pipe(Match.when(...), Match.exhaustive)`, then fold the returned fragment through `clsx`/`cx`; exhaust `cva`'s declarative `variants` table first, reserving `Match` for cross-field logic the table cannot express.

[LOCAL_ADMISSION]:
- `clsx` admits as the class-fold only; conflict resolution routes to `tailwind-merge`, variant tables to `class-variance-authority`.
- one class-fold binding per module: a module with a `cva` dependency uses `cx`, one without uses `clsx`, never both.
