# [TYPESCRIPT_LANGUAGE]

TypeScript 7 on the `tsgo` native compiler is the active language surface, with the stable `tsc` peer-check as the dual-compiler floor. The workspace manifest owns `target`, `module`, `strict`, `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, and `verbatimModuleSyntax`; this page is the language-feature law for choosing type-expression, inference, dispatch, and module forms before adding a local abstraction. Runtime-authority shape lives in `shapes.md`; rail and effect composition lives in `rails-and-effects.md`.

## [1]-[ACTIVE_SURFACE]

[ACTIVE_SURFACE]:
- Compiler: `tsgo` native canonical typecheck; stable `tsc` peer-check is the second floor
- Module mode: `verbatimModuleSyntax` — `import type`/`export type` explicit, no elision
- Strictness: `strict`, `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noImplicitOverride`, `useUnknownInCatchVariables`
- Inference baseline: inference-first from runtime anchors; `const` type parameters preserve literal narrowing
- Absence baseline: `Option<T>` in domain; `null`/`undefined` only at the serialization seam
- Export baseline: one or two named exports per module; every other symbol `_`-prefixed; no barrel files, no default exports, no namespace re-export
- Ecosystem baseline: the `effect` core and the `@effect/*` set are the standard library

Treat source files as modern Effect-native TypeScript, not compatibility layers. Replace older spellings, overload families, standalone type aliases, branded-type exports, and JS stdlib reflexes whenever the active surface or the Effect ecosystem carries the concept directly.

[COMPILER_DIVERGENCE]:
- Law: `tsgo` is the canonical fast typecheck and `tsc` is the conformance floor — a construct accepts only when both agree, and `tsc` is authoritative on any divergence because it is the conformance reference the type system is specified against; a `tsgo`-only acceptance is a defect to repair at the shape, never a license to ship.
- Law: a construct that diverges between the two compilers is rewritten to the form both accept, not gated behind a directive — the doctrine's deep `infer`-decomposition, tuple-budgeted recursion, and `as const satisfies` vocabularies are chosen partly because they are the constructs least exposed to checker divergence.
- Reject: a `// @ts-expect-error` or `// @ts-ignore` masking a `tsc`/`tsgo` disagreement, a build that runs only one of the two checkers, and a type so baroque that only one compiler resolves it — collapse it to the form both prove.

## [2]-[CANONICAL_CHOOSER]

Use the active TypeScript surface directly. This chooser owns type-expression, inference, dispatch, and module forms. Runtime-authority construction (`Schema.Class`, `Model.Class`, `Data.TaggedEnum`) is owned by `shapes.md`; rail composition by `rails-and-effects.md`; JS-stdlib replacement by `system-apis.md`. Replace an older spelling or local machinery when the active surface owns the behavior.

[TYPE_DERIVATION_FORMS]: which inference primitive lifts a runtime anchor into the type the program already knows.

| [INDEX] | [CONCERN]                        | [USE]                                              | [REPLACE]                               |
| :-----: | :------------------------------- | :------------------------------------------------- | :-------------------------------------- |
|   [1]   | type from a runtime authority    | `typeof anchor.Type`, `typeof vocab`               | standalone `type X = { ... }`           |
|   [2]   | discriminant union from keys     | `keyof typeof vocabulary`                          | hand-listed string-literal union        |
|   [3]   | per-key output type              | indexed access `(typeof V)[K]["field"]`            | parallel mapped declarations            |
|   [4]   | function-shape decomposition     | `ReturnType`, `Parameters`                         | re-declared parameter/return interfaces |
|   [5]   | structural validation            | `as const satisfies Record<...>`                   | untyped `as const` then manual checks   |
|   [6]   | projection from a canonical type | `Pick`, `Omit`, `Partial`, `Extract`, `Exclude`    | parallel sibling interface declarations |
|   [7]   | accessor/event-name family       | mapped type with template-literal key remapping    | N hand-written accessor signatures      |
|   [8]   | deep structural transform        | one parameterized recursive type with depth budget | `DeepPartial`/`DeepReadonly` copies     |

[DISPATCH_FORMS]: how a value-returning decision states its whole law as one total expression.

| [INDEX] | [CONCERN]                         | [USE]                                          | [REPLACE]                               |
| :-----: | :-------------------------------- | :--------------------------------------------- | :-------------------------------------- |
|   [1]   | closed tagged domain fold         | `Data.taggedEnum().$match` / `Match.valueTags` | `switch (x._tag)` ladders               |
|   [2]   | structural/predicate dispatch     | `Match.type().pipe(Match.when, ...)`           | `if`/`else` chains                      |
|   [3]   | typed return constraint           | `Match.withReturnType` before any arm          | per-arm return-type drift               |
|   [4]   | multi-dimension predicate         | `Match.whenAnd`/`whenOr`/`not`                 | nested boolean branching                |
|   [5]   | literal-prefix routing            | `Match.discriminatorStartsWith`                | string `startsWith` chains              |
|   [6]   | non-`_tag` discriminant fold      | `Match.discriminatorsExhaustive(field)`        | hand-keyed `Record` dispatch + assert   |
|   [7]   | class-prototype narrowing         | `Match.instanceOf` + open terminal             | `instanceof` ladders                    |
|   [8]   | completion stance                 | `Match.exhaustive` / `option` / `either`       | catch-all `orElse` masking a variant    |
|   [9]   | keyed-domain classification       | vocabulary lookup / threshold iteration        | `Match.when` chains duplicating a vocab |
|  [10]   | conditional return over a generic | conditional type + generic constraint          | 3+ overload signatures                  |

[TYPE_PARAMETER_FORMS]: how a type parameter's distribution, variance, and inference are pinned.

| [INDEX] | [CONCERN]                   | [USE]                                      | [REPLACE]                            |
| :-----: | :-------------------------- | :----------------------------------------- | :----------------------------------- |
|   [1]   | per-member union evaluation | naked `T extends ... ? ... : ...`          | manual union enumeration             |
|   [2]   | union-as-unit evaluation    | `[T] extends [U]`                          | accidental distributive blow-up      |
|   [3]   | multi-channel decomposition | multi-clause `infer A`/`infer E`/`infer R` | sequential conditional extraction    |
|   [4]   | declaration-site variance   | `in`/`out` annotations                     | lazily-inferred variance surprises   |
|   [5]   | inference position control  | `NoInfer<T>`                               | ambiguous two-position widening      |
|   [6]   | recursion budget            | tuple accumulator at a depth limit         | unbudgeted recursion hitting the cap |

[MODULE_FORMS]: how a module declares its public surface and consumes another.

| [INDEX] | [CONCERN]                 | [USE]                                      | [REPLACE]                      |
| :-----: | :------------------------ | :----------------------------------------- | :----------------------------- |
|   [1]   | public surface            | one or two named exports                   | export sprawl, default exports |
|   [2]   | type-only import          | `import type` under `verbatimModuleSyntax` | value import elided at emit    |
|   [3]   | private substrate         | `_`-prefixed module-local declaration      | exported helper consumed once  |
|   [4]   | cross-module package edge | the Nx browser/node/neutral tag graph      | ad-hoc cross-package import    |

## [3]-[LANGUAGE_FORM_CONTRACTS]

Use these contracts when the chooser names the form but code still needs a placement rule.

[VOCABULARY_DERIVATION_SITE]:
- Use when: one `as const satisfies Record<...>` anchor can drive a discriminant union, per-key output types, and polymorphic dispatch from a single declaration.
- Accept: `keyof typeof` for the discriminant, indexed access for per-key output, a conditional return type carrying the discriminant to the result, and `NoInfer` where a second parameter must conform rather than widen.
- Reject: a hand-listed union beside the vocabulary, a parallel mapped declaration per axis, and an overload family where one conditional return type carries the polymorphism.
- Law: rename a vocabulary key and every derivation and dispatch site fails simultaneously; the anchor is the single source the keys, output types, and dispatch all read.
- Boundary: a vocabulary whose rows carry runtime behavior — schedule, status, log level — is a policy owner read by `surfaces-and-dispatch.md`; this site owns the type-level derivation algebra.

```ts conceptual
const _Handlers = {
  create: { method: "POST"   as const, idempotent: false, returns: "entity" as const },
  read:   { method: "GET"    as const, idempotent: true,  returns: "option" as const },
  remove: { method: "DELETE" as const, idempotent: true,  returns: "void"   as const },
} as const satisfies Record<string, { method: string; idempotent: boolean; returns: string }>

type _Action = keyof typeof _Handlers
type _ActionResult<K extends _Action> =
  (typeof _Handlers)[K]["returns"] extends "entity" ? Effect.Effect<_Entity, _NotFound>
  : (typeof _Handlers)[K]["returns"] extends "option" ? Effect.Effect<Option.Option<_Entity>, never>
  : Effect.Effect<void, _NotFound>

declare const _dispatch: <K extends _Action>(
  action: K,
  payload: NoInfer<{ readonly tenantId: string }>,
) => _ActionResult<K>
```

[TYPE_PARAMETER_SITE]:
- Use when: a type parameter's distribution, variance, or inference position determines whether one declaration yields the whole family or quietly mis-evaluates.
- Accept: naked `T` for per-member union evaluation, `[T] extends [U]` for union-as-unit, multi-clause `infer` to decompose a multi-channel carrier in one pass, `in`/`out` to fix variance at declaration, and `NoInfer` to pin which position drives inference.
- Reject: an unintended distributive evaluation when a union is passed as a unit, a lazily-inferred variance that surprises a consumer, and sequential conditional extraction where one multi-clause `infer` decomposes every channel at once.
- Law: `extends` in generic position is an upper-bound constraint; combined with a conditional return type it narrows input and output in one signature, deleting the overload family.
- Boundary: recursion past the instantiation depth limit uses a tuple-accumulator budget; a deeper transform is quarantined, never left to hit the cap.

```ts conceptual
type _Decompose<T> = T extends Effect.Effect<infer A, infer E, infer R>
  ? { readonly success: A; readonly error: E; readonly context: R }
  : never

type _Paths<T, Depth extends ReadonlyArray<unknown> = []> =
  Depth["length"] extends 8 ? never
  : T extends object
    ? { [K in keyof T & string]: K | `${K}.${_Paths<T[K], [...Depth, unknown]>}` }[keyof T & string]
    : never

type _ServiceOf<out A, out E, in R> = { readonly run: (deps: R) => Effect.Effect<A, E> }
```

[PROJECTION_ALGEBRA_SITE]:
- Use when: one canonical type yields a family of views, and a view derives by one set-theoretic operator.
- Accept: `Pick`/`Omit` for projection and elimination, `Partial` for optional lift, `Extract`/`Exclude` for union filtering by assignability, and a template-literal mapped type for an accessor or event-name family.
- Reject: a standalone declaration for a view that one utility application gives unless three or more consumers reference it; below that threshold the projection inlines at the call site.
- Law: when the canonical type has a `Schema`/`Model` anchor, `Schema.pick`/`Schema.omit`/`Schema.partial` own the projection and the TS utility forms do not — that boundary belongs to `shapes.md`; this site owns projection over a pure structural type with no runtime authority.
- Boundary: a homomorphic mapped type (`keyof T`) preserves source modifiers; a non-homomorphic one starts fresh — modifier inheritance is the selection axis, not a style choice.

```ts conceptual
type _Entity = {
  readonly id: string; readonly tenantId: string; readonly name: string
  readonly status: "active" | "archived" | "purging"; readonly createdAt: Date
}
type _EntityKey   = Pick<_Entity, "id" | "tenantId">
type _EntityPatch = Partial<Pick<_Entity, "name" | "status">>
type _Terminal    = Exclude<_Entity["status"], "active" | "archived">
type _Accessors   = { +readonly [K in keyof _Entity as `get${Capitalize<string & K>}`]-?: () => _Entity[K] }
```

[MODULE_SURFACE_SITE]:
- Use when: a module declares its public names or consumes another module's surface.
- Accept: one or two named exports, `import type`/`export type` under `verbatimModuleSyntax`, and every non-exported symbol carrying the `_` prefix integrated into the exports.
- Reject: default exports, barrel re-export files, namespace re-exports, a `const X = _X` alias that promotes a private symbol, and an exported `_`-prefixed symbol — name it correctly at the declaration site or remove the export.
- Law: a `_`-prefixed function exists only as a closure inside a scoped constructor or as a module-level semantic anchor serving two or more call sites; a single-caller private function inlines into the export.
- Boundary: package topology and the browser/node/neutral publication split belong to the Nx module-boundary tag graph in the workspace manifest; this site owns the per-module export shape.

[ABSENCE_FORM_SITE]:
- Use when: a value can be absent, and absence must stay distinct from failure and from a present `null`.
- Accept: `Option<T>` in domain code with `Option.fromNullable` at the boundary, `Option.match`/`Option.getOrElse` at consumption, `Schema.optionalWith(field, { as: "Option" })` as the only Schema absence form, and `undefined` only as the decoded shape under `exactOptionalPropertyTypes`.
- Reject: `null`/`undefined` crossing a domain boundary, a present-but-meaningless sentinel, `noUncheckedIndexedAccess` results on an open key consumed without an `Option` lift, and bare `Schema.optional`/`Schema.NullOr`/`Schema.UndefinedOr` on a domain field — each re-admits `undefined`/`null` into the decoded interior that `Schema.optionalWith({ as: "Option" })` would have closed.
- Law: indexed access into a closed vocabulary keyed by a `keyof typeof V` value carries its membership proof in the key type, so it is exempt from the `noUncheckedIndexedAccess`-to-`Option` rule — `_Policy[this.reason]` and `Severity[level]` are total; the rule applies to an open `string`/`number` index, a foreign array offset, or a `Record` whose key is not proven a member.
- Boundary: cause-bearing absence — unavailable, degraded, pending — is a tagged family owned by `boundaries.md`, never `Option.none`; this site owns the no-cause absence form.

```ts conceptual
const _firstActive = (rows: ReadonlyArray<_Entity>): Option.Option<_Entity> =>
  pipe(
    Array.findFirst(rows, (row) => row.status === "active"),
    Option.orElse(() => Array.head(rows)),
  )
```

[KERNEL_EXEMPTION_SITE]:
- Use when: a tight decode loop, a parser scanning a byte buffer, a `yield*` generator body inside `Effect.gen`, or a `Schema.transformOrFail` parse step is measurably faster as statements than as a combinator fold, and the seam is the platform-forced or measured kernel the `EXPRESSION_SPINE` law names.
- Accept: a `for`/`while` loop or mutable accumulator confined to one `_`-prefixed kernel function whose signature is total over its input and whose result is an immutable value or a typed rail; the `Effect.gen` generator body, where `yield*` sequencing is the idiomatic spine, not a statement violation.
- Reject: a statement-bearing loop outside a named kernel, a mutable accumulator that escapes the kernel as interior state, and a `// BOUNDARY ADAPTER` mark on ordinary domain logic that a `pipe`/`Array.reduce`/`Match` fold expresses without measurement pressure.
- Law: the kernel is closed — it admits a value and emits a value or rail, never a partial or a leaked mutable reference — and every statement-bearing kernel carries the `// BOUNDARY ADAPTER` mark or sits inside `Effect.gen`, so the exemption is recoverable from the declaration and the surrounding expression spine is unbroken.
- Boundary: the worker/marshal and event-callback statement seams are owned by `boundaries.md`; this site owns the in-process compute kernel exemption.

```ts conceptual
const _scanFrames = (bytes: Uint8Array): ReadonlyArray<number> => {
  const offsets: Array<number> = [] // BOUNDARY ADAPTER: measured parse kernel, result detaches as readonly
  for (let i = 0; i + 4 <= bytes.length; i += 4) {
    if (bytes[i] === 0x47 && bytes[i + 1] === 0x4c) offsets.push(i)
  }
  return offsets
}

const _admit = Schema.transformOrFail(Schema.String, Schema.Number, {
  decode: (s, _, ast) => {
    const n = Number(s) // dependent bind: parse must precede the range check
    return Number.isFinite(n) ? ParseResult.succeed(n) : ParseResult.fail(new ParseResult.Type(ast, s))
  },
  encode: (n) => ParseResult.succeed(String(n)),
})
```

## [4]-[ABSTRACTION_COLLAPSE_TESTS]

Use these tests before keeping a local abstraction beside a language or ecosystem primitive.

[TYPE_PROLIFERATION]:
- Smell: a standalone `type`/`interface` mirrors a shape that `typeof anchor.Type` or `keyof typeof vocab` already gives, or an `interface IService` sits beside an `Effect.Service` class.
- Collapse: delete the type and derive it from the runtime anchor; the `Effect.Service` class is both value and type.
- Done when: every consumer references the derived type and no parallel declaration restates the runtime shape.

[BRAND_SPRAWL]:
- Smell: a `type Id = string & Brand<"Id">` or a `const _Id = Schema.String.pipe(Schema.brand("Id"))` exists as a standalone module-level symbol before its owning class.
- Collapse: inline `Schema.brand("Id")` as a field modifier in the owning `Schema.Class`/`Model.Class`.
- Done when: the brand lives in field position and no standalone branded-primitive symbol is exported.

[MATCH_OVER_VOCAB]:
- Smell: a `Match.value`/`Match.when` chain classifies into tiers or categories that an `as const satisfies Record` vocabulary already maps.
- Collapse: classify through vocabulary field lookup or threshold iteration; reserve `Match` for structural and predicate dispatch on non-keyed shapes.
- Done when: the keyed dispatch reads the vocabulary and no `Match` arm re-encodes the vocabulary's knowledge.

[SURFACE_FACADE]:
- Smell: a module exists only to re-export another, collect names from several owners, or wrap an external API with a rename.
- Collapse: import the real owner by name; integrate the logic into the one or two exports the module owns.
- Done when: readers find the implementation owner without traversing a barrel, and no `_`-prefixed symbol is exported.

[STDLIB_LEAKAGE]:
- Smell: `new Map()`/`new Set()`/`Array.from`/`Object.entries`/`null` appear in domain code where the Effect ecosystem provides `HashMap`/`HashSet`/`Chunk`/the `Record` module/`Option`.
- Collapse: use the Effect data structure that carries structural equality and persistent update.
- Done when: JS stdlib collections and `null` appear only at FFI and serialization boundaries.
</content>
