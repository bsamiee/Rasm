# Construction-Admission Pipeline

[STATE_MACHINE]:
- The generated constructor is a fixed four-step transform with no branch on field shape: `props = { ...props }` (shallow copy so the caller's object is never mutated), then `delete props["_tag"]` gated on `kind !== "Class"`, then `props = lazilyMergeDefaults(fields, props)`, then the validation gate — the same ordered pipeline runs for `Schema.Class`, `TaggedClass`, `TaggedError`, and `TaggedRequest` because all four route through one `makeClass` body and differ only by which `kind` string and which `Base` they pass.
- The shallow copy is the immutability seam of the whole admission: every later step (`delete`, `lazilyMergeDefaults` assigning `out[key]`, the validator's coercions) mutates the copy, so the props literal a caller passes is unaliased from the instance — an owner constructed twice from one literal yields two independent instances, and the literal stays reusable.
- The `delete props["_tag"]` step runs before defaults, so the constructor first strips any tag a caller smuggled in, then `lazilyMergeDefaults` re-supplies it from the tag field's own `withConstructorDefault(() => tag)` thunk — the tag is removed and regenerated, never trusted from input, which is why `new Member({ _tag: "Wrong", ...fields })` cannot forge a member identity: the supplied tag is deleted and the canonical literal re-merged.
- `lazilyMergeDefaults` iterates `Reflect.ownKeys(fields)` (string AND symbol keys), and for each fills `out[key]` only when `out[key] === undefined && isPropertySignature(field)` — a plain `Schema.String` field carries no `PropertySignature` wrapper and thus never receives a default through this path, so only `propertySignature`-wrapped fields participate in construction-time default merging.
- The merge reads the default off the type-side node by tag: `ast._tag === "PropertySignatureDeclaration" ? ast.defaultValue : ast.to.defaultValue` — a declaration carries its default directly, a transformation (a `fromKey` rename, an `optionalWith` lift) carries it on the `.to` (decoded) side, so a `fromKey`-renamed defaulted field still merges its default at construction because the default rode the type-side `to`, not the encoded `from`.

[VALIDATION_TARGET_LAW]:
- The constructor validates against `constructorSchema` — `schema.annotations({ ...typeAnnotations })`, the **type side**, never the encoded side — through `ParseResult.validateSync(constructorSchema)(props)`, and `validateSync` is precisely `getSync(AST.typeAST(schema.ast), true, options)`: it lifts the AST to its type projection before parsing, so construction admits a value already in decoded shape and runs no decode transform.
- `AST.typeAST` collapses every `Transformation` node to `typeAST(ast.to)` (discarding the encode/decode functions) but **preserves every `Refinement` node** — it recurses into the refinement's `from` and re-wraps with the same `.filter` — so the type-side validation the constructor runs is exactly the set of field refinements and cross-field `Schema.filter` invariants, with all representation transforms erased; the constructor checks predicates, never wire-shape conversion.
- This is the law that splits the two admission events: `new Self` / `Self.make` validate the **decoded type plus its refinements**; `Schema.decodeUnknown(Self)` validates the **encoded type, runs every transform, then the refinements**. A field typed `Schema.NumberFromString` accepts a `number` at the constructor and a `string` at decode — the constructor's `typeAST` erased the `string → number` transform to its `number` target, so passing a `"5"` to `new Self` is a type error the constructor's validator rejects, while decode admits it.

```typescript
import { Schema } from 'effect';

class Sample extends Schema.Class<Sample>('Sample')({
    count: Schema.NumberFromString,
    ratio: Schema.Number.pipe(Schema.between(0, 1)),
}) {}

// constructor validates the TYPE side: count is `number`, ratio's `between` refinement runs
const built = new Sample({ count: 5, ratio: 0.5 });

// the same owner's decode validates the ENCODED side: count arrives as the `string` the transform
// decodes, the `between` refinement still runs — one declaration, two admission events
const admit = Schema.decodeUnknown(Sample);

// reject: the loose split — a parallel `parseInput` decoder + a separate `new SampleImpl` with no
// validation re-derives, by hand, the two events the one owner separates at `typeAST` vs full-AST parse.
```

[DEFAULT_SEAM_ALGEBRA]:
- The three default seams are three distinct AST writes, not one knob with modes: `withConstructorDefault` writes `defaultValue` onto the type-side node (`PropertySignatureDeclaration.defaultValue` or a transformation's `to.defaultValue`) — the exact slot `lazilyMergeDefaults` reads — so it fires at `new`/`make` and is invisible to decode; `withDecodingDefault` writes a `PropertySignatureTransformation` whose `decode: o => applyDefaultValue(o, default)` runs in the decode parse and leaves the constructor-read `defaultValue` slot untouched — so it fires at `Schema.decodeUnknown` and is invisible to the constructor; `withDefaults({ constructor, decoding })` is literally `self.pipe(withDecodingDefault(decoding), withConstructorDefault(constructor))` — both writes stacked, one default per event.
- The two seams share one missing-value predicate. `lazilyMergeDefaults` gates on `out[key] === undefined`; `applyDefaultValue` matches `onNone → some(default())` and `onSome value → value === undefined ? default() : value` — both treat an explicit `undefined` identically to an absent key, so a field passed as `{ key: undefined }` triggers its constructor default exactly as omitting it does, and a present `undefined` at decode triggers the decoding default. Absence and explicit-`undefined` are one admission case across both seams.
- The seams diverge at the type level, and the divergence is load-bearing for the constructor's required-set. `withConstructorDefault` keeps the type token unchanged but flips `_HasDefault` to `true`; `withDecodingDefault` flips the type token `"?:" → ":"` and `Type → Exclude<Type, undefined>` while keeping `_HasDefault` at `false`. Because `Struct.Constructor` makes a key optional precisely when the field is `OptionalTypePropertySignature` (token `"?:"`) **or** `PropertySignatureWithDefault` (`_HasDefault: true`), a `withConstructorDefault` field becomes constructor-optional, while a *pure* `withDecodingDefault` field stays constructor-**required** — a decoding default never relieves the constructor of demanding the key.

```typescript
import { Schema } from 'effect';

class Stamped extends Schema.Class<Stamped>('Stamped')({
    // construction default: fires at `new`, key optional in constructor, untouched on decode
    seq: Schema.Number.pipe(Schema.propertySignature, Schema.withConstructorDefault(() => 0)),
    // decoding default: fires at decode, key STILL required by the constructor (HasDefault stays false)
    tier: Schema.optional(Schema.String).pipe(Schema.withDecodingDefault(() => '<base>')),
    // both seams: optional in constructor AND filled on decode, decoded value never undefined
    mode: Schema.optional(Schema.String).pipe(
        Schema.withDefaults({ constructor: () => '<auto>', decoding: () => '<auto>' }),
    ),
}) {}

// constructor still requires `tier` — the decoding default did not relieve it
const made = new Stamped({ tier: '<explicit>' });
```

[VOIDABLE_ADMISSION_TYPE]:
- The constructor signature is `new (props: RequiredKeys<C> extends never ? void | Simplify<C> : Simplify<C>, options?: MakeOptions)`, and `RequiredKeys<T>` keeps key `K` only when `{} extends Pick<T, K>` is false (the key is not optional-or-defaulted). So the props argument becomes `void`-able — `new Self()` legal with zero arguments — exactly when every field is `optional`, `optionalWith`, or `withConstructorDefault`-backed; one required field collapses `RequiredKeys<C>` away from `never` and forces the props object at every call site.
- The runtime constructor matches the type with `constructor(props = {}, options = false)`: the `= {}` default makes the void-able form run `lazilyMergeDefaults(fields, {})` over an empty object, so an all-defaulted owner constructed argument-free still merges every constructor default and then validates the fully-materialized result — the empty-argument form is not a bypass, it is the maximal default-merge case.
- A refined struct whose every field is defaulted yields a `void`-able `new Self()` that still runs the cross-field refinement over the defaulted instance: the refinement is a `Refinement` node `typeAST` preserved, so the validator gate fires on the merged-from-defaults props, and an owner whose defaults violate its own invariant fails at the zero-argument construction it appeared to license.

[DISABLE_VALIDATION_BOUNDARY]:
- The second constructor argument is `MakeOptions = boolean | { readonly disableValidation?: boolean | undefined }`, normalized by `getDisableValidationMakeOption(options) = isBoolean(options) ? options : options?.disableValidation ?? false` — a bare `true` is the shorthand for `{ disableValidation: true }`, and the gate it controls is one `if`: `if (!getDisableValidationMakeOption(options)) props = ParseResult.validateSync(constructorSchema)(props)`.
- The short-circuit skips **only** the `validateSync` call — the type-side refinement pass. The shallow copy, the `_tag` delete, and `lazilyMergeDefaults` all run unconditionally **before** the gate, so a `disableValidation` instance still gets its constructor defaults merged and its forged `_tag` stripped-and-regenerated; trust suppresses the predicate check, never the structural normalization. An instance built with `disableValidation` is well-shaped (tag-correct, defaults-filled) but invariant-unverified.
- `super(props, true)` always passes `true` to the `Data.Class` base regardless of the caller's `options`, and the base constructor `Structural(args)` does `if (args) Object.assign(this, args)` — it ignores its second argument entirely. The base never validates; all admission lives in the `makeClass` wrapper, and the `true` is an inert holdover. There is no second validation layer to disable.
- `disableValidation` is reachable only at the in-process constructor seam. `Schema.decodeUnknown(Self)` takes `ParseOptions` (`errors`, `onExcessProperty`, `propertyOrder`), never a `disableValidation`, so a wire value can never bypass the refinement — and the asymmetry is structural, not a policy choice: the decode path runs `goMemo(Self.ast, true)` over the full `transform(encodedSide, declaration, …)`, whose **from**-side `encodedSide` is the refined schema parsed first (refinement fires there), and whose `decode: i => new this(i, true)` then constructs with validation off precisely because the encoded-side parse already validated. The constructor's `disableValidation: true` inside decode is safe because the gate already ran one node earlier; a caller granting `disableValidation` at `new Self` removes the only place it ever runs.

```typescript
import { Schema } from 'effect';

class Bounded extends Schema.Class<Bounded>('Bounded')({
    lower: Schema.Number,
    upper: Schema.Number,
}) {}

// trusted in-process seam: refinement skipped, but _tag-strip + default-merge still run
const trusted = new Bounded({ lower: 9, upper: 1 }, { disableValidation: true });

// the admission boundary has no escape hatch — the wire value is always refined
const admit = Schema.decodeUnknown(Bounded);

// reject: a sibling `unsafeBounded(props)` factory wrapping `new Bounded(props, true)` is the hop the
// boolean second argument already deletes — the trust grant is the option, never a parallel constructor.
```

[MAKE_VALIDATION_TARGET]:
- `make` forwards its varargs straight to `new this(...args)`, so it inherits the constructor's `typeAST`-lifted gate and its `MakeOptions` short-circuit unchanged — `make` is never a second validation policy, and a trust grant rides the same `MakeOptions` argument `new` takes, not a parallel unsafe-make entrypoint.
- A struct owner's `make` runs the same `validateSync(this)`, but `this.ast` is a transform-free `TypeLiteral`, so the `typeAST` lift inside `validateSync` is an identity and the validation target equals the struct itself — whereas the class constructor's `typeAST` does real work, collapsing the codec's encoded↔type transform to its type side. The construction-default merge is byte-identical between struct and class owners; only what `typeAST` erases differs, and that erasure is exactly why a class constructor rejects an encoded-shape input the same owner's decode would admit.

[TAGGED_CONSTRUCTOR_OBLIGATION]:
- The injected `_tag` field is `withConstructorDefault(propertySignature(Literal(tag)), () => tag)` — `HasDefault: true` — so it is `PropertySignatureWithDefault`, which removes `_tag` from `RequiredKeys` and from `Struct.Constructor`; the constructor parameter is `Struct.Constructor<Omit<Payload, "_tag">>` for tagged kinds, so a caller never supplies the tag and never can relieve a required field by supplying it. The runtime `delete props["_tag"]` and the type-level `Omit` enforce the same rule from both sides — the tag is owner-supplied, input-forbidden.
- For `TaggedError`, the `_tag` constructor default merges identically, and the `message` getter the owner synthesizes (when no `message` field is declared) is a non-enumerable prototype property — it never enters `props`, never participates in `lazilyMergeDefaults` or validation, and never reaches the field record's encoded form; the throwable capability is prototype-side, the admitted state is field-side, and the constructor admits only the latter.
