# Declaration-site collapse

[CHAIN_THESIS]:
- The naive shape walks the chain `value -> type -> const -> typeof -> instantiation`, each link a separate statement restating what the prior link fixed; the collapsed target fuses runtime value, static type, brand, codec, equality, keys, and constructor into the first declaration so no link follows it.
- A shape's first appearance is its only appearance: a follow-on `type X = typeof x`, a second `const` restating the value, or a separate `new X()` after the owner declares are the three deleted chain links, not stylistic variants.
- Six levers carry the collapse: `as const` (deep-`readonly` literal preservation), `satisfies` (non-widening conformance), `const`-modified type parameters (call-site literal inference), the class-as-both-type-and-value owner, declaration merging (one name, two namespaces), and the non-instantiable owner (`new (_: never)`). Each lever's reach is bounded; one declaration stacks the levers whose reaches union to its full need.
- The chain links are paid for in widening: each restatement exists because the prior link's type widened and the next link re-narrows it by hand, so the levers are not independent fusers but one inference contract with distinct entry points — `as const` on the value, `<const T>` on the parameter, rest-tuple capture on the argument list, and `satisfies` on the trailing clause — each pinning literals against a different widening pressure. Where two levers preserve the same literal they are redundant, not additive: stacking `as const` onto an argument already bound to a `<const T>` parameter buys nothing the parameter inferred, and the redundant operator is the noise the single-source rule deletes.
- The fused owner is consumed five ways the chain re-grows on, each its own collapse: as a value-constructor and type source (the base chain), as a projection source (a second sub-owner for a narrower shape), as a value-producer (`new X({ ...full field set... })` plus per-site default literals), as a derivation source for instances and metadata (sibling `Equivalence`/`Arbitrary`/`Pretty`/`JSONSchema`/message table), and against an unstated runtime invariant (the owner must be initialized at the point its first consumer runs). One owner named once absorbs all five.

[AS_CONST]:
- `as const` applied to an object or array literal recursively marks every property `readonly`, narrows every primitive to its literal type, and turns arrays into `readonly` tuples — one operator replaces a hand-written nested `readonly` interface plus literal-union restatement.
- The reach stops at the type system: `as const` carries no runtime validation, no decode, no `Equal`/`Hash`, and no nominal identity — the frozen-looking value is a plain object the runtime can still mutate, so a vocabulary that must reject unknown input is decoded through a schema, never asserted `const`.
- A `const`-asserted table is a behavior-column dispatch surface: rows are read by `keyof typeof` for the key union and indexed access for the row type, so the cap, threshold, or handler travels as data the checker tracks, never an inlined literal at the use site.
- The defect `as const` deletes is the parallel literal-union type written beside the table — `type Key = 'a' | 'b' | 'c'` next to `const t = { a, b, c }` restates what `keyof typeof t` already yields exactly.

[SATISFIES]:
- `satisfies T` checks a literal against `T` while preserving the literal's narrow inferred type, where an annotation `: T` would widen it — the expression keeps its precise type AND proves conformance in one statement, deleting the choice between "narrow but unchecked" and "checked but widened".
- `8080 satisfies number` has static type `8080`, not `number`; the literal survives so downstream `typeof` reads the exact value, and assigning a different literal to `typeof port` fails at compile time.
- `satisfies T` runs excess-property checking against `T` while leaving the inferred type untouched — an object-literal table checked `satisfies Record<K, Row>` rejects a stray key the contract omits AND keeps each row's narrow literal type for `keyof typeof`/indexed reads, where `: Record<K, Row>` would both admit the value and erase the per-row literals.
- `satisfies` carries no decode and no runtime presence — it is erased entirely; the conformance it proves holds only for the literal as written and proves nothing about a value reassigned later or arriving from outside, so it gates the source table at authoring time while a runtime-admitted vocabulary is still decoded through `Schema.decodeUnknown` — `satisfies` and `Schema.decodeUnknown` guard different seams, the source seam and the boundary seam, and the two are never swapped.
- The composition `as const satisfies T` is ordered and non-commutative in effect: `as const` first deep-freezes the inferred type to literals, then `satisfies` checks that frozen type against the contract — dropping `as const` lets `satisfies` check a widened literal that passes the contract while losing the literals downstream needs, and dropping `satisfies` proves no conformance, so a table that drives dispatch carries both clauses.

```typescript
const policy = {
    retry: { attempts: 3, base: 'exponential' },
    flush: { attempts: 1, base: 'linear' },
} as const satisfies Record<string, { readonly attempts: number; readonly base: 'exponential' | 'linear' }>
const budget = (k: keyof typeof policy): number => policy[k].attempts * (policy[k].base === 'exponential' ? 2 : 1)
```

[CONST_TYPE_PARAMETER]:
- A `const`-modified type parameter `<const T>` infers each argument as if the caller wrote `as const` — object literals become deep-`readonly`, arrays become `readonly` tuples, primitives narrow to their literal type — so the call-site assertion migrates into the signature once and every consumer inherits clean literal inference. `Struct.entries<const R>(obj)` returns `Array<[keyof R & string, R[keyof R & string]]>` with the key literals intact where a non-`const` parameter collapses the keys to `string`.
- `Match.value<const I>(i)` preserves the matched value's literal union into the matcher, so `Match.when`, `Match.whenOr`, and `Match.tag` narrow against the exact members and `Match.exhaustive` closes the union; `Match.when<R, const P>` marks the pattern parameter `const` so an inline literal pattern is captured as its literal type for the refinement, never widened.
- The modifier shapes inference for that one type parameter only: an argument passed through a prior `const`-typed binding already carries the literal, and a value handed in widened by an explicit `: string` annotation is widened before the parameter sees it — `<const T>` recovers literals the call expression would lose, never literals a prior statement already destroyed.
- The reach ends at type erasure: a `const` type parameter shapes inference only, leaving no runtime artifact, no `readonly` enforced at the value (the `readonly` is the inferred type's, not the object's), no persistence past compilation, and no validation — a value admitted from outside is still narrowed against the static literal the source spells, never the dynamic shape that arrived.

```typescript
import { Match } from 'effect'
type Phase = 'idle' | 'busy' | 'done'
const advance = (p: Phase) =>
    Match.value(p).pipe(
        Match.whenOr('idle', 'done', () => 0),
        Match.when('busy', () => 1),
        Match.exhaustive,
    )
```

[VALUE_SOURCE_VS_TYPE_SOURCE]:
- Two matcher entrypoints split on where the type originates: `Match.value<const I>(i)` reads the type from a runtime value and carries `const I` to stop the value's literal union from widening, while `Match.type<I>()` takes no value, supplies `I` as an explicit type argument, and carries no `const` because there is no value to widen — the type is named, not inferred.
- The split is the general rule for the no-instantiation owner: when the shape is sourced from an explicit type argument, no literal-preservation lever applies because nothing was inferred; when it is sourced from a passed value, the value-side lever (`as const` or a `<const T>` parameter) is mandatory or the match arms narrow against a widened union and `Match.exhaustive` fails to close.

```typescript
import { Match } from 'effect'
type Signal = { readonly _tag: 'open'; readonly at: number } | { readonly _tag: 'shut' }
const fold = Match.type<Signal>().pipe(
    Match.discriminator('_tag')('open', (s) => s.at),
    Match.discriminator('_tag')('shut', () => 0),
    Match.exhaustive,
)
const live = (s: Signal) => Match.value(s).pipe(Match.tag('open', 'shut', () => true), Match.orElse(() => false))
```

[REST_TUPLE_CAPTURE]:
- A rest parameter `<Literals extends ReadonlyArray<LiteralValue>>(...literals: Literals)` preserves each literal in tuple position without a `const` modifier — variadic capture is itself a literal-preservation mechanism distinct from `<const T>`: each spread position is inferred independently against the tuple element, so no whole-value `as const` is needed, while a single array argument collapses to the element type.
- `Schema.Literal('<a>', '<b>')` infers the union `'<a>' | '<b>'` from `Literals[number]`; `Match.is(...literals)`, `Match.discriminator(field)(...values, f)`, and `Schema.transformLiterals(...pairs)` all spell their vocabulary as spread literals, never one frozen array — an explicit array argument `["<a>", "<b>"]` widens to `string[]` and breaks the tag extraction the rest position preserves.
- `Schema.transformLiterals<const A extends Members<readonly [from, to]>>(...pairs): Union<{ ... }>` stacks both mechanisms: the rest position captures each `[from, to]` pair and the `<const A>` modifier pins the pair tuples deep, so the bidirectional map derives from the one spread argument list as a `Union` of `transformLiteral` members, never a parallel decode table — the single-pair overload `transformLiterals([from, to])` collapses to one `transformLiteral<to, from>`.
- The collapse the rest position deletes is the array-plus-cast pattern `Schema.Literal(...(['<a>', '<b>'] as const))` — the spread already carries the literals, so the surrounding array and its `as const` are dead surface.

[NOINFER_BARRIER]:
- `NoInfer<A> = [A][A extends any ? 0 : never]` walls a parameter out of the inference candidate set: a default-value or fallback callback typed `() => NoInfer<Type>` is checked against `Type` but contributes nothing to inferring it, so `Schema.withConstructorDefault(() => '<v>')` resolves `Type` from the property signature alone and the default never widens the field — the field owns the type, the default conforms to it.
- The barrier is the mechanism that lets one owner carry both the shape and a same-shaped policy value without the policy leaking into the shape's inference: a literal field `Schema.Literal('<x>')` with a `NoInfer`-guarded default forces the default to be `'<x>'`, where an un-guarded default would widen the inferred field to `string` to accommodate the callback's return.
- The reach ends at inference ordering: `NoInfer` removes a position from inference but adds no runtime check and no narrowing — it decides which argument is the source of truth, not what the truth is, so it appears on the secondary position (default, fallback, instrumentation) and never on the position that defines the shape; placing it on the primary argument inverts the source of truth and leaves the type with no inference source.

[EXCESS_KEY_FENCE]:
- A `<const Cases>` parameter intersected with `{ [K in Exclude<keyof Cases, A['_tag']]: never }` rejects every arm whose key is not a real tag at the declaration site — `Data.taggedEnum`'s generated `$match`, `Match.valueTags`, and `Match.tagsExhaustive` all carry this fence, so a misspelled or stale arm is a compile error on the handler object, not a silent dead branch.
- The `const` modifier on the cases record is load-bearing: it pins the handler object's key set to its literal keys so the `Exclude<keyof Cases, ...>` difference is computed against the exact keys written, where a widened key set would dissolve the `never` constraint and admit the excess arm.
- The fence collapses two checks into the parameter type: exhaustiveness (every tag has an arm, enforced by the mapped `[Tag in A['_tag']]` requirement) and exactness (no arm beyond the tags, enforced by the `never` intersection) — so the exhaustive `$match` finalizer needs no trailing `Match.exhaustive` and no `default` arm, the handler record alone closing the union.

```typescript
import { Data } from 'effect'
type Cmd = Data.TaggedEnum<{ readonly Push: { readonly v: number }; readonly Pop: {} }>
const { Push, Pop, $match } = Data.taggedEnum<Cmd>()
const run = $match({ Push: (c) => c.v, Pop: () => -1 })
const seq = [Push({ v: 7 }), Pop()] as const
const total = seq.reduce((acc, c) => acc + run(c), 0)
```

[CLASS_DUALITY]:
- `class X extends Schema.Class<X>('X')({ ... }) {}` is at once a static type, a value-constructor, a `Schema<X, Encoded, R>` codec, structural `Equal`/`Hash`, a `.fields` accessor, and a validating `.make` factory — one statement standing in for the type, the codec, the equality instance, the field record, and the constructor a naive chain would spell separately.
- `Schema.Schema.Type<typeof X>` and `Schema.Schema.Encoded<typeof X>` extract the decoded and wire types from the same declaration, so the static type is lifted from the owner, never restated as a sibling interface.
- `.extend<Y>('Y')({ ... })` and `.transformOrFail<Y>('Y')(...)` derive a child owner whose field set, encoded type, and requirement set union the parent's with the new fields — inheritance lands as one declaration carrying the merged `Fields & NewFields`, never a parallel class restating the parent's columns.
- The schema-class is the no-instantiation owner when used purely as a type and codec source: `Schema.Schema.Type<typeof X>` and `Schema.decodeUnknown(X)` consume it with no `new X()` ever following — the class names the shape, the boundary produces the values.

```typescript
import { Schema } from 'effect'
class Edge extends Schema.Class<Edge>('Edge')({
    source: Schema.String,
    target: Schema.String,
    weight: Schema.Number.pipe(Schema.positive()),
}) {
    get reversed(): Edge {
        return Edge.make({ source: this.target, target: this.source, weight: this.weight })
    }
}
const admit = Schema.decodeUnknown(Edge)
type EdgeWire = Schema.Schema.Encoded<typeof Edge>
```

[NOMINAL_SELF_AND_ENFORCER]:
- The `Self` generic is self-referential by construction: `Schema.Class<X>('X')` threads the class name `X` back as the factory's `Self` type argument before `X` finishes declaring, so the factory's `Class<Self, ...>` interface returns `Struct.Type<Fields> & Inherited & Proto` as the instance type and methods and getters in the class body see a fully-typed `this` that includes the fields the same declaration just named. The threaded `Self` decides what `this` and the constructor return but carries no runtime artifact of its own — codec, `Equal`/`Hash`, and `.make` come from the `Schema.Class` machinery, while `Self` only pins which type those surfaces are typed against.
- `Schema.Class<Self = never>` defaults `Self` to `never`, and the return type is `[Self] extends [never] ? MissingSelfGeneric<'Class'> : Class<...>` — omitting the generic resolves the base to the template-literal type `` `Missing \`Self\` generic - use \`class Self extends Class<Self>()({ ... })\`` ``, a `string` subtype no `extends` clause accepts, so the omission is a compile error whose text spells the exact correct re-declaration.
- `MissingSelfGeneric<Usage, Params>` is parameterized by factory name and leading-argument prefix, so each generated owner's enforcer names its own signature: `MissingSelfGeneric<'TaggedClass', '"Tag", '>` and `MissingSelfGeneric<'TaggedRequest', '"Tag", SuccessSchema, FailureSchema, '>` produce distinct guidance strings — the enforcer is one polymorphic type instantiated per owner family, not a copied diagnostic.
- The enforcement is structural, not disciplinary: omitting the self-reference does not produce a loose value, it produces a value whose type is an unusable template-literal diagnostic or a circular-inference error, so the missing collapse fails to compile rather than degrading to a working-but-redundant chain.
- A forward-declared `type Self = { ... }` written before the `Schema.Class` to feed its own `Self` argument is the broken-cycle restatement: the class IS its own type once `<X>` threads the name, so the standalone alias is the second statement the self-generic deletes.

[INHERITANCE_RE_THREADING]:
- `.extend<Next>('Next')({ ... })` re-runs the self-reference contract on the child: the method returns `[Next] extends [never] ? MissingSelfGeneric<'Base.extend'> : Class<Next, Fields & NewFields, ...>`, so a subclass that forgets its own `<Next>` argument hits the same template-literal wall as a forgetting base and the diagnostic names `Base.extend` specifically.
- The parent's `Self` migrates into the child's `Inherited` slot — `Class<Next, Fields & NewFields, I & Struct.Encoded<NewFields>, R | Struct.Context<NewFields>, C & Struct.Constructor<NewFields>, Self, Proto>` — so the child instance type is `Struct.Type<Fields & NewFields> & ParentSelf & Proto`, threading the parent's whole typed surface (its getters and methods) through one inheritance edge rather than a re-declared field list.
- `.transformOrFail<Next>('Next')` and `.transformOrFailFrom<Next>` carry the identical `[Next] extends [never]` enforcer and identical `Inherited = Self` re-threading, differing only in whether the decode/encode pair runs against the decoded `Struct.Type<Fields>` or the encoded `I` side — the self-reference mechanics are constant across the inheritance verbs, the transform direction is the only variable.
- The encoded type accumulates monotonically: the child's `I` is `ParentI & Struct.Encoded<NewFields>` for `extend`, but for `transformOrFail` the child reuses the parent `I` unchanged because the transform redefines the decoded side over the same wire shape — the inheritance edge that widens the wire and the one that re-decodes the same wire are distinguished by which slot the new fields land in.

```typescript
import { Schema } from 'effect'
class Account extends Schema.Class<Account>('Account')({
    id: Schema.String,
    balance: Schema.Number.pipe(Schema.nonNegative()),
}) {
    get solvent(): boolean {
        return this.balance > 0
    }
}
class Ledgered extends Account.extend<Ledgered>('Ledgered')({
    entries: Schema.Array(Schema.Number),
}) {
    get reconciled(): boolean {
        return this.solvent && this.entries.reduce((s, e) => s + e, 0) === this.balance
    }
}
const admit = Schema.decodeUnknown(Ledgered)
type LedgeredWire = Schema.Schema.Encoded<typeof Ledgered>
```

[STRUCTURAL_SELF_VIA_SUSPEND]:
- `Schema.suspend: <A, I, R>(f: () => Schema<A, I, R>) => suspend<A, I, R>` defers the self-reference behind a thunk so a recursive shape closes in one declaration — the field references the owner that is still being assigned, and the thunk carries the full codec so a decoded tree validates arbitrarily deep from `unknown` through one owner.
- The recursive owner's type cannot be inferred — a `const` initialized from an expression that references itself yields a circular `any`/error — so the decoded and encoded faces are declared as opaque self-referential interfaces and the schema is annotated `Schema.Schema<A, AEncoded>`; the two interfaces ARE the single-source type, lifted into the annotation, not a chain restating an inferred shape, and where the decoded face and the wire face diverge two interfaces are required because `Schema.Schema.Type` and `Schema.Schema.Encoded` of a self-referential schema each name a distinct recursive shape.
- The thunk's return annotation must restate the same `Schema.Schema<A, AEncoded>` because the arrow's body resolves to the still-`undefined` binding at definition time; the annotation on both the outer `const` and the inner thunk is load-bearing structural self-reference, not redundant — dropping the inner annotation collapses the recursive position to `Schema.Schema<unknown, unknown>`.
- A recursive shape spelled as a `Schema.transform` from a non-recursive intermediate, or a hand-written recursive constructor beside the schema, is the cycle the `suspend` thunk closes in place — the deferred self-reference is the one-declaration form, the intermediate is dead surface.

```typescript
import { Schema } from 'effect'
interface Tree {
    readonly label: string
    readonly children: ReadonlyArray<Tree>
}
interface TreeEncoded {
    readonly label: string
    readonly children: ReadonlyArray<TreeEncoded>
}
const Tree: Schema.Schema<Tree, TreeEncoded> = Schema.Struct({
    label: Schema.String,
    children: Schema.Array(Schema.suspend((): Schema.Schema<Tree, TreeEncoded> => Tree)),
})
const admitForest = Schema.decodeUnknown(Schema.Array(Tree))
```

[HIGHER_KINDED_SELF]:
- A generic tagged union parameterizes its own definition through `this`: an interface `extends Data.TaggedEnum.WithGenerics<2>` whose member `taggedEnum: Box<this['A'], this['B']>` uses the `A`/`B` slots the base interface declares (`readonly A: unknown`, `readonly B: unknown`, plus `C`/`D`) as higher-kinded placeholders, so one self-referential interface stands in for the generic constructor family across every type-argument instantiation.
- `numberOfGenerics: Count` pins the arity, and `Data.taggedEnum<Z>()` dispatches on it through the overload set: `Z extends WithGenerics<1>` returns constructors typed `<A>(args) => Kind<Z, A>`, `WithGenerics<2>` returns `<A, B>(args) => Kind<Z, A, B>` — each constructor is itself generic, so `Right<string, number>({ a: 7 })` supplies the union's type arguments at the construction site, never a separately declared concrete alias.
- `TaggedEnum.Kind<Z, A, B, C, D>` is the application operator: `(Z & { A; B; C; D })['taggedEnum']` substitutes the supplied arguments into the `this`-slots and projects the `taggedEnum` member — the higher-kinded self-application that other ecosystems spell with a defunctionalized type-level apply is one indexed access here.
- The reach is type-level only: the HKT encoding shapes which generic the constructors and matchers carry, but `Data.taggedEnum` still produces deep-structural `Equal`/`Hash` constructors with no codec — a generic union that must be admitted from `unknown` is a `Schema` union, not a `Data.TaggedEnum`, because `WithGenerics` adds parameterization, not boundary decode.
- A concrete monomorphic copy of a generic union (`type FailureString = Result<string, never>` declared as a sibling schema) is the instantiation the higher-kinded `this`-slots already cover: the generic constructors supply the type arguments at the construction site, so the per-instantiation alias is the restatement `WithGenerics` deletes.

[GENERIC_MATCH_BINDING]:
- A generic union's `$match` is the `GenericMatchers<Z>` surface, not the monomorphic `TaggedEnum.Constructor`'s `$match`: both `GenericMatchers` overloads quantify `<A, B, C, D, Cases>`, so leaving the type arguments unsupplied defaults `Kind<Z, A, B, C, D>` to `Kind<Z, unknown, unknown, unknown, unknown>` and every arm payload widens to the `unknown`-parameterized member — the generics bind at the match site or the arm accesses lose their field types.
- The curried five-type-argument finalizer `$match<A, B, C, D, Cases>({ ... })` binds each arm's payload to the instantiated `Kind<Z, A, B, C, D>` member; the trailing `Cases` carries the excess-key fence `[K in Exclude<keyof Cases, Z['taggedEnum']['_tag']>]: never` so a stray arm is still a compile error after the generics bind.
- The two-argument value-first overload `$match(self, cases)` is the form to reach for over the curried one when a concrete value is in hand: it carries the same `<A, B, C, D, Cases>` quantification, so it infers the four generics from `self: Kind<Z, A, B, C, D>` in one call and needs no explicit type arguments — the curried `$match<A, B, C, D, Cases>(cases)` is required only where the matcher is built ahead of any value to fold over. Calling the bare `$match({ ... })` that closes a monomorphic enum leaves `Kind<Z, A, B, C, D>` defaulted to `unknown` so every arm payload is untyped: one of the two binding spellings is mandatory and the unbound finalizer never types the arms.

```typescript
import { Data } from 'effect'
type Result<E, A> = Data.TaggedEnum<{
    readonly Failure: { readonly error: E }
    readonly Success: { readonly value: A }
}>
interface ResultDef extends Data.TaggedEnum.WithGenerics<2> {
    readonly taggedEnum: Result<this['A'], this['B']>
}
const { Failure, Success, $match } = Data.taggedEnum<ResultDef>()
const recover = $match<string, number, unknown, unknown, {
    Failure: (f: { readonly _tag: 'Failure'; readonly error: string }) => number
    Success: (s: { readonly _tag: 'Success'; readonly value: number }) => number
}>({
    Failure: (f) => f.error.length,
    Success: (s) => s.value,
})
const score = recover(Success<string, number>({ value: 7 }))
```

[CONSTRUCTOR_ONLY_OWNER]:
- `Data.Class` and `Data.TaggedClass(tag)` produce a constructor-and-`Equal`/`Hash` owner with no codec — `new X({ ... })` yields a deep-structural-equality instance, but there is no decode, no annotations, and no wire projection, so the reach is value identity, not boundary admission.
- The reach gap is the selection rule: a shape that must enter from `unknown` is a `Schema.Class` (codec included); a shape that only needs interior structural equality and a tagged constructor is a `Data.TaggedClass`, paying nothing for a codec it never uses.
- `Brand.nominal<Id>()` is the zero-runtime nominal owner: the brand is a phantom intersection `string & Brand.Brand<'Id'>` with no predicate, so the constructor is the identity function and the distinction lives only in the type checker — admitted only where a real nominal collision exists, since a brand with no runtime predicate adds a type and zero behavior.
- `Brand.refined` is the branch with a runtime predicate, returning a `Brand.Constructor` whose call throws on violation and whose `.option`/`.either` variants route failure as a value — the choice between `nominal` and `refined` is exactly the choice between erased identity and validated admission.

[STRUCT_METHOD_VS_PIPEABLE]:
- The fused owner is consumed as a projection source where the chain re-grows: a naive author declares the owner, then declares a second sub-owner beside it for a narrower shape, then writes `new X()` to pull a field. Every sub-shape, narrowed schema, erased face, and key union reads as a one-hop derivation off the owner, so the owner is named once and no second `const`, no parallel sub-schema, and no `new X()` projection-then-pick follows.
- Two consumption modes split on whether the derivation crosses the runtime boundary: a projection that returns a `Schema` is itself a codec that admits from `unknown` (`pick`, `omit`, `pluck`, `partial`, `required`, `typeSchema`, `encodedSchema`, `keyof`), while a projection that returns a type or a field record is pure erasure read at compile time only (`Schema.Schema.Type`, `Schema.Schema.Encoded`, `.fields`, indexed access). Derive a codec when the sub-shape must still admit input, derive a type when interior code already holds the value.
- `Schema.Struct({ ... })` returns a `Struct<Fields>` carrying `.fields`, `.make`, and the recursive methods `.pick<Keys>(...keys): Struct<Pick<Fields, Keys[number]>>` and `.omit<Keys>(...keys): Struct<Omit<Fields, Keys[number]>>` — a sub-struct derived through the method is itself a `Struct` that retains `.fields`, `.make`, and further `.pick`/`.omit`, so projection composes without bottoming out.
- The standalone `Schema.pick<A, I, Keys extends ReadonlyArray<keyof A & keyof I>>(...keys)` and `Schema.omit(...keys)` are pipeable over any `Schema<A, I, R>` and return `SchemaClass<Pick<A, Keys[number]>, Pick<I, Keys[number]>, R>` — a flat schema with `.annotations` but no `.fields`, no `.make`, and no recursive `.pick`, so chaining a further projection off the result requires the pipeable form again.
- The selection is structural, not stylistic: `.pick` the method preserves the field-record surface and is the spelling when the sub-shape is itself a constructor and further projection source; `Schema.pick` the pipeable applies to a non-`Struct` schema (a class, a union, a refined schema) where `.fields` was never present and a flat codec is all the projection can yield — reaching for the pipeable on a `Struct` that needs further projection re-enters the pipeable each hop and loses the recursive surface.
- The keys are fenced at both faces: `Keys extends ReadonlyArray<keyof A & keyof I>` rejects a key absent from either the decoded or the encoded shape, so a key that exists only post-transform cannot be picked through the encoded projection — the rest position captures the keys as literals and a single array argument widens them to `PropertyKey` and dissolves the `Pick`.

```typescript
import { Schema } from 'effect'
const Record = Schema.Struct({
    id: Schema.String,
    score: Schema.Number.pipe(Schema.between(0, 1)),
    label: Schema.optional(Schema.String),
})
const Key = Record.pick('id', 'score')
const admitKey = Schema.decodeUnknown(Key)
type KeyFields = keyof typeof Key.fields
const project = Record.pipe(Schema.pluck('score'))
```

[ERASED_FACE_DERIVATION]:
- `typeSchema(self: Schema<A, I, R>): SchemaClass<A>` derives the decoded face as a fresh schema dropping every transformation and encoding step, so a wire-decoding owner yields a codec over its already-decoded values for an interior boundary — re-admitting an in-memory `A` without re-running the `I -> A` transform, where a sibling schema for the decoded shape would restate the field set the owner already carries.
- `encodedSchema(self): SchemaClass<I>` derives the wire face dropping all refinements and transformations, and `encodedBoundSchema(self): SchemaClass<I>` derives the same wire type but preserves the refinements up to the first transformation point — the two differ only in whether pre-transform constraints survive into the projected codec, so a wire schema that must still enforce a leading format check uses the bound variant and one that admits any structurally-valid wire shape uses the plain one.
- These projections return a `SchemaClass`, so each is a live codec: `Schema.decodeUnknown(typeSchema(Owner))` and `Schema.encode(encodedSchema(Owner))` operate without a second declaration, and the erased-but-typed read `Schema.Schema.Type<typeof Owner>` / `Schema.Schema.Encoded<typeof Owner>` covers the case where interior code needs only the type — the codec projection and the type extraction are the two reaches off the same owner, and a parallel `interface OwnerDecoded` placed beside the owner is the restatement both delete.

[ARITY_AND_OPTIONALITY_PROJECTION]:
- `partial(self): SchemaClass<{ [K in keyof A]?: A[K] | undefined }, { [K in keyof I]?: I[K] | undefined }, R>` derives a fully-optional projection of both faces in one hop, and `required(self): SchemaClass<{ [K in keyof A]-?: A[K] }, { [K in keyof I]-?: I[K] }, R>` derives the all-required projection — the `-?` mapped modifier strips optionality from the encoded face too, so a patch-shaped codec and a strict codec both derive from the one owner rather than two parallel field lists with different optionality.
- `partialWith(self, { exact: true })` derives the optional projection without admitting `undefined` as a value — the mapped type becomes `{ [K in keyof A]?: A[K] }` with no `| undefined` arm, so the projected codec rejects an explicit `undefined` where the plain `partial` accepts it; the `exact` option is the seam between optional-key and optional-value semantics carried on one derivation.
- These compose with `pick`/`omit`: `Owner.pick('a', 'b').pipe(Schema.partial)` derives a two-field patch codec from the owner in two hops with no intermediate `const`, where the naive form declares a `Patch` struct restating the picked fields as optional — the projection chain is the executable derivation, the patch struct is the deleted parallel shape.

```typescript
import { Schema } from 'effect'
const Entity = Schema.Struct({
    id: Schema.String,
    name: Schema.String,
    weight: Schema.Number,
})
const Patch = Entity.pipe(Schema.omit('id'), Schema.partialWith({ exact: true }))
const admitPatch = Schema.decodeUnknown(Patch)
type PatchShape = Schema.Schema.Type<typeof Patch>
```

[KEY_UNION_AS_CODEC]:
- `keyof(self: Schema<A, I, R>): SchemaClass<keyof A>` derives the key union as a literal-union codec, so the owner's keys admit from `unknown` as a validated discriminant — a string arriving from outside is decoded against `keyof A` and rejected if it names no field, where `keyof typeof Owner.fields` yields the same union as an erased type only.
- The split is the boundary again: `keyof(Owner)` when a runtime value must be checked to be one of the owner's keys (a projection selector, a sort key, a column name from a request), and `keyof typeof Owner.fields` when interior code indexes the owner with a key it already holds — the codec validates an external key, the type indexes an internal one.
- `pickLiteral<A, L>(...literals)` narrows a literal-union schema to a sub-union of its own members in rest position, deriving a narrower vocabulary codec from the broader one without restating the members — the rest spread captures each retained literal and the result `Literal<[...L]>` is the sub-vocabulary the broader owner already contained.

[CONSTRUCTOR_FACE_DERIVATION]:
- The fused owner is consumed as a value-producer where the chain re-grows: a naive author declares the owner, then writes `new X({ ...full field set... })` restating every field, then a second literal for the default at each call site — the construction link multiplies the field list once per site and the default once per call. The construction argument shape, the defaulted fields, and the validated-vs-raw production mode pin onto the owner so the producer is named once.
- The construction argument type is a derived projection distinct from both the decoded type and the wire type: the owner yields three faces — `Struct.Type<F>` (decoded), `Struct.Encoded<F>` (wire), and `Struct.Constructor<F>` (what `.make`/`new` accepts) — and the third differs from the first precisely on the defaulted and optional fields, so the constructor argument is computed off the field set, never hand-listed.
- `Struct.Constructor<F> = UnionToIntersection<{ [K in keyof F]: F[K] extends OptionalTypePropertySignature ? { readonly [H in K]?: Schema.Type<F[H]> } : F[K] extends PropertySignatureWithDefault ? { readonly [H in K]?: Schema.Type<F[H]> } : { readonly [h in K]: Schema.Type<F[h]> } }[keyof F]>` makes both the optional-token fields AND the `HasDefault=true` fields optional in the constructor argument — a defaulted field is required in `Struct.Type<F>` but optional in `Struct.Constructor<F>`, so the producer omits it and the owner supplies it. `PropertySignatureWithDefault` is the union of `PropertySignature` instances with the seventh `HasDefault` type parameter pinned `true`, so a field gains optional-in-constructor status from the type-level `HasDefault` bit the default-applying combinator flips, not from a separate optional declaration.
- The decoded face is unaffected by the construction face: `Struct.Type<F>` keeps a defaulted field required because decoding is total over the wire shape, so the same owner reports a field as mandatory to an interior reader and optional to a producer — one declaration carrying two opposite arities of the same field for its two consumption modes.
- `make(props: RequiredKeys<Struct.Constructor<F>> extends never ? void | Simplify<Struct.Constructor<F>> : Simplify<Struct.Constructor<F>>, options?: MakeOptions)` collapses the zero-argument case structurally: `RequiredKeys<T> = { [K in keyof T]-?: {} extends Pick<T, K> ? never : K }[keyof T]` evaluates to `never` exactly when every constructor field is optional, and the `extends never ? void | ...` arm makes `Owner.make()` legal with no argument — an all-defaulted owner constructs from nothing, the empty call the field-by-field literal deletes.

[DEFAULT_AT_DECLARATION]:
- `withConstructorDefault(() => NoInfer<Type>)` raises a `PropertySignature<..., boolean, R>` to `PropertySignature<..., true, R>`, flipping the `HasDefault` bit so the field drops out of `Struct.Constructor<F>`'s required set — the default is declared once on the owner and every `.make`/`new` site omits the field, deleting the per-site literal the naive constructor repeats.
- The combinator requires a `PropertySignature`, so a bare schema is lifted first: `Schema.String.pipe(Schema.propertySignature, Schema.withConstructorDefault(() => '<v>'))` — `propertySignature<S>` produces `PropertySignature<':', Type<S>, never, ':', Encoded<S>, false, Context<S>>` and the default-applier consumes it; applying the default to a raw schema is a type error because the `HasDefault` slot exists only on the property-signature owner.
- The default fires only on the construction path: `withConstructorDefault` populates the field when `.make`/`new` omits it and never participates in decoding, so a wire value missing the field still fails decode — the construction default and the decoding default are separate combinators guarding separate seams.
- A second literal at each construction site supplying the same fallback value is the per-site-default link the declaration-time default deletes; the constant lives on the field once, not inlined per producer.

[DECODE_DEFAULT_AND_DUAL]:
- `withDecodingDefault(() => NoInfer<Exclude<Type, undefined>>)` consumes a `PropertySignature<'?:', Type, Key, '?:', Encoded, false, R>` and returns `PropertySignature<':', Exclude<Type, undefined>, Key, '?:', Encoded, false, R>` — the decoded-side token flips `?:`→`:` (the field becomes required and `undefined`-stripped in `Struct.Type`) while the encoded-side token stays `?:` (the wire may still omit it), so one declaration makes a field absent-on-the-wire yet always-present-in-memory.
- The `Exclude<Type, undefined>` in the return type is load-bearing: the decoded field is narrowed to its non-`undefined` type because the default guarantees presence, so an interior reader sees no `| undefined` arm where the optional wire field would have carried one — the default closes the optionality the wire shape opened.
- `withConstructorDefault` leaves both tokens untouched and only flips `HasDefault`; `withDecodingDefault` leaves `HasDefault` `false` and flips the decoded token — the two combinators move orthogonal bits, so a field defaulted on construction is still wire-required and a field defaulted on decode is still constructor-required unless both are applied.
- `withDefaults({ constructor: () => ..., decoding: () => ... })` applies both in one declaration, returning `PropertySignature<':', Exclude<Type, undefined>, Key, '?:', Encoded, true, R>` — decoded-required, wire-optional, and constructor-optional at once, the single owner-resident spelling of "absent on the wire, defaulted on decode, omittable on construct" that three separate default declarations would otherwise spell.

```typescript
import { Schema } from 'effect'
class Setting extends Schema.Class<Setting>('Setting')({
    key: Schema.String,
    retries: Schema.Number.pipe(Schema.nonNegative(), Schema.propertySignature, Schema.withConstructorDefault(() => 3)),
    label: Schema.optionalWith(Schema.String, { default: () => '<unnamed>' }),
}) {
    get retriable(): boolean {
        return this.retries > 0
    }
}
const built = Setting.make({ key: '<k>' })
const admit = Schema.decodeUnknown(Setting)
type Built = Schema.Schema.Type<typeof Setting>
```

[VALIDATED_VS_RAW_PRODUCTION]:
- The construction mode splits on whether the value crossed the boundary: a value arriving from `unknown` is produced by `Schema.decodeUnknown` and re-running construction-side checks on it is dead work, while a value built in-memory from known-valid parts is produced by `.make`/`new` which runs the field refinements once — the decode path and the construct path are the two producers off one owner, never a decode followed by a redundant re-construct.
- `MakeOptions = boolean | { readonly disableValidation?: boolean }` rides every `.make`/`new` as the second argument, and the default mode runs the field refinements at construction so `Owner.make({ ... })` rejects a value violating a `Schema.between`/`Schema.nonEmpty` filter — `.make` is a validating constructor, not a raw assignment, so the construction site cannot mint an invalid instance.
- `Owner.make(props, { disableValidation: true })` (or the boolean shorthand `Owner.make(props, true)`) skips the refinement pass, trading the validated-construction guarantee for speed on a value already proven valid upstream — exactly the decode-then-construct case: a value already decoded carries its invariants, so re-validating it on construction is the dead check `disableValidation` removes, while a value built from raw parts keeps validation on.
- The `Class` instance type is `new (props: RequiredKeys<C> extends never ? void | Simplify<C> : Simplify<C>, options?: MakeOptions) => Struct.Type<Fields> & Inherited & Proto` — `new Owner(...)` and `Owner.make(...)` are the same validating producer with the same `MakeOptions` and the same `void`-when-all-defaulted argument, so the choice between `new` and `.make` is calling convention only, never a difference in checks or in which fields are required.
- The static `make<C extends new (...args) => any>(this: C, ...args: ConstructorParameters<C>): InstanceType<C>` threads the concrete subclass through `this`, so `.make` on a class extended with body methods returns the extended instance type — the validated producer carries the owner's full surface, never a bare struct.
- `Schema.decodeUnknown(Owner)(v)` followed by `new Owner(decoded)` is the decode-then-reconstruct link: the decode already produced a validated instance, so the re-construction re-runs checks on a proven value — the decode is the producer, and where interior code holds known-valid parts the lone `.make`/`new` is, with `disableValidation` only when the parts are upstream-proven.

[TAGGED_AND_CODEC_FREE_PRODUCTION]:
- `Schema.TaggedClass`/`TaggedStruct` set `C = Struct.Constructor<Omit<Fields, '_tag'>>`, so the `_tag` field is omitted from the constructor argument and auto-supplied — the producer writes `Event.make({ at: 0 })` and the owner injects the discriminant, deleting the `_tag: '<Event>'` literal the naive tagged construction repeats and the mismatch a hand-typed tag invites.
- `Data.TaggedClass(tag)` is the codec-free validated-equality producer: `new <A>(args: VoidIfEmpty<{ [P in keyof A as P extends '_tag' ? never : P]: A[P] }>) => Readonly<A> & { readonly _tag: Tag }` omits `_tag` from args exactly as the schema tagged class does, supplies it on the instance, and yields a structural `Equal`/`Hash` value — but with no codec, so it never admits from `unknown` and pays nothing for a decoder it does not use.
- `VoidIfEmpty<S> = keyof S extends never ? void : S` is the codec-free analogue of the schema owner's `RequiredKeys<C> extends never ? void` arm: `Data.Class`/`Data.TaggedClass`/`Data.Structural`/`Data.Error` all gate the empty-fields constructor to `void`, so a fieldless tagged case constructs as `new Marker()` with no argument — the empty-object literal the naive form passes is deleted by the type.
- The two producers split on boundary need exactly as the owner choice does: a value entering from `unknown` is a `Schema.TaggedClass` (validating construction plus decode), a value built only in interior code is a `Data.TaggedClass` (validating construction, no decode) — `.make` versus `new` is convention, but `Schema` versus `Data` is whether the construction seam also carries a boundary seam.
- A hand-typed `_tag: '<Event>'` in a tagged constructor argument is the discriminant-restatement link `TaggedClass`'s `Omit<Fields, '_tag'>` constructor deletes — the tag is owner-supplied and a hand-passed tag is at best redundant and at worst a mismatch the omission forbids.

[VARIANT_CONSTRUCTOR_FACES]:
- A `VariantSchema.Field` config attaches a per-variant schema set drawn from the six variants `select`, `insert`, `update`, `json`, `jsonCreate`, `jsonUpdate` to one field so the owner derives a different construction argument shape per variant — `Model.Generated(IdSchema)` populates the `{ select, update, json }` set and omits `insert`, so the insert-variant constructor argument lacks the field the database supplies and the select-variant type carries it.
- `Model.DateTimeInsert`, `Model.GeneratedByApp`, and `Model.UuidV4Insert` are construction-default fields scoped to one variant: the value is auto-supplied on the insert producer and present on the select reader, so the field is omitted from the insert construction argument and required in the selected type — the per-variant default the naive form spells once per variant landing on the owner once.
- `Model.FieldOption(schema)` makes the field an `Option` on every variant, and `Model.Sensitive(schema)` carries the value on the in-memory variants while redacting it from the `json` projection — one field declaration yielding a present-in-construction, absent-in-serialization split, the construction and projection faces diverging from a single config.
- `Field: <const A extends Field.ConfigWithKeys<Variants[number]>>(config: A & { readonly [K in Exclude<keyof A, Variants[number]>]: never })` fences the variant config keys at the declaration site exactly as the excess-key fence guards a handler record — a misspelled variant key is a compile error on the field config, and the `const A` pins the variant set to its literal keys so the `never` difference is computed against the exact keys written.
- A parallel insert-shaped struct declared beside a model to drop the generated id is the per-variant-construction restatement `Model.Generated`/`DateTimeInsert` delete: the variant field already omits the value from the insert producer, so the sibling insert struct restates the field set the variant config derives.

[ANNOTATION_THESIS]:
- The fused owner is consumed as a derivation source for instances and metadata where the chain re-grows: a naive author declares the owner, then a sibling `Equivalence`, a sibling `Arbitrary` for tests, a sibling pretty-printer, a JSON-schema literal for the wire contract, and an error-message table keyed by the same shape — five parallel artifacts restating the structure the owner already fixes. Each pins onto the owner through its annotation channel, so the derived instance reads off the one declaration and no second `Equivalence`, `Arbitrary`, `Pretty`, `JSONSchema`, identifier const, or message table follows.
- Two annotation modalities split on direction: a derivation annotation (`equivalence`, `arbitrary`, `pretty`, `jsonSchema`) overrides what the library would otherwise compute structurally from the owner, while a metadata annotation (`identifier`, `title`, `description`, `documentation`, `examples`, `default`, `message`, `schemaId`) attaches a fact the structure cannot carry. The default-derived instance is the common case — `Schema.equivalence(Owner)`, `Arbitrary.make(Owner)`, `Pretty.make(Owner)`, `JSONSchema.make(Owner)` all synthesize from the AST with zero annotation — and the annotation is the targeted override at exactly the leaf the structural default gets wrong, never a wholesale restatement.
- The reach gap defines the lane edge: the annotation channel carries every projection a shape's identity, generation, presentation, and wire-contract need EXCEPT ordering — `Order` has no annotation slot and no `Schema`-level derivation, so a `SortedSet`/`SortedSetFromSelf` takes its `Order.Order<A>` as a separate value argument, the one shape-derived instance the owner does not absorb.

[ANNOTATIONS_SCHEMA_SLOTS]:
- `Annotations.Schema<A, TypeParameters>` is the full slot set every owner accepts through `.annotations(...)` or the second factory argument: `identifier`, `title`, `description`, `documentation`, `examples`, `default`, `message`, `schemaId`, `jsonSchema`, `arbitrary`, `pretty`, `equivalence`, `concurrency`, `batching`, `parseIssueTitle`, `parseOptions`, `decodingFallback`, `typeConstructor` — one record literal carrying every cross-cutting projection a separate file per concern would scatter.
- `Annotations.Doc<A>` is the metadata-only sub-interface (`title`, `description`, `documentation`, `examples`, `default`) that `Element` and `PropertySignature` accept where a tuple element or field carries documentation but not a full derivation override — the inherited slot set narrows by declaration position, so a field annotation cannot smuggle an `arbitrary` the struct owner owns.
- `Annotations.Filter<A, P = A>` is `Annotations.Schema<A, readonly [P]>` — a refinement's annotation carries exactly one type parameter, the pre-refinement type `P`, so a `filter`'s `arbitrary`/`equivalence`/`pretty` override receives the base instance for `P` and refines it, never re-derives from nothing.
- `.annotations(...)` returns `Self` for an `Annotable` owner (the struct, the class, the literal all preserve their api-interface type through annotation) but a bare `Schema<A, I, R>` widens to `Schema<A, I, R>` — so annotating a `Struct` keeps `.fields`/`.make`/`.pick`, while annotating after a pipe that already widened loses them; the annotation hop is placed before the surface-flattening hop, not after.

[DERIVATION_DEFAULT_AND_OVERRIDE]:
- `Schema.equivalence(schema): Equivalence.Equivalence<A>` synthesizes a structural equivalence from the AST — field-wise for a struct, tag-dispatched for a union, element-wise for a tuple — so the `Equal` a `Schema.Class` already carries and the standalone `Equivalence` a non-class schema needs both derive from the one owner, never a hand-written `(a, b) => a.x === b.x` beside it.
- The `equivalence` annotation overrides this synthesis: `EquivalenceAnnotation<A, TypeParameters> = (...equivalences: { [K in keyof TypeParameters]: Equivalence<TypeParameters[K]> }) => Equivalence<A>` receives one child equivalence per type parameter and returns the parent's — so a container schema composes its element's derived equivalence rather than restating it, the override threading the structural defaults of its parts through one callback.
- `Arbitrary.make(schema): FastCheck.Arbitrary<A>` and `Arbitrary.makeLazy(schema): LazyArbitrary<A>` synthesize a generator from the same AST, honoring every refinement constraint (`Schema.between`, `Schema.minLength`, `Schema.pattern`) as a fast-check constraint — so the property-test generator is the owner read through `Arbitrary.make`, never a parallel `fc.record({ ... })` duplicating the field set the schema fixes.
- The `arbitrary` annotation's callback receives `(...arbitraries, ctx: ArbitraryGenerationContext)` — one `LazyArbitrary` per type parameter plus a context carrying `maxDepth`, `depthIdentifier`, and a `constraints` union — so a recursive or constraint-sensitive override composes children and reads the recursion budget from `ctx.maxDepth`, the generation-depth control the structural default would otherwise have to guess.

```typescript
import { Arbitrary, Pretty, Schema } from 'effect'
const Bounded = Schema.Number.pipe(
    Schema.int(),
    Schema.between(1, 6),
    Schema.annotations({
        identifier: 'Pip',
        title: '<face-value>',
        examples: [1, 6],
        message: (issue) => ({ message: `<out-of-range>: ${String(issue.actual)}`, override: true }),
        arbitrary: (ctx) => (fc) => fc.integer({ min: 1, max: ctx.maxDepth > 0 ? 6 : 1 }),
        pretty: () => (n) => `<pip-${n}>`,
    }),
)
const gen = Arbitrary.make(Bounded)
const show = Pretty.make(Bounded)
const eq = Schema.equivalence(Bounded)
```

[PRETTY_AND_JSONSCHEMA_PROJECTION]:
- `Pretty.make(schema): (a: A) => string` synthesizes a structural printer from the AST, and the `pretty` annotation `PrettyAnnotation<A, TypeParameters> = (...pretties: { [K in keyof TypeParameters]: Pretty<TypeParameters[K]> }) => Pretty<A>` overrides one leaf — a branded id printed without its quoting, a duration printed as its human form — composing each type parameter's `Pretty` exactly as the equivalence override composes child equivalences, the two derivation channels sharing one fan-in shape.
- `JSONSchema.make(schema)` projects the owner to a JSON-Schema document, and the `jsonSchema` annotation (`JSONSchemaAnnotation = object`) overrides the projection at the node the structural form cannot express — a regex format string, a `format: "uuid"` keyword, an externally-fixed `$ref` — so the wire contract a separate hand-authored JSON file would hold is the owner read through `JSONSchema.make`, the annotation patching only the leaf the AST loses.
- On a refinement the `jsonSchema` annotation MERGES into the base schema's projection rather than replacing it: the refinement node's JSON-Schema fragment unions onto the underlying type's keywords, so `Schema.String.pipe(Schema.pattern(rx), Schema.annotations({ jsonSchema: { format: '<fmt>' } }))` yields a `{ type: "string", pattern, format }` document — one declaration accreting constraints into the wire contract, never a separate schema-per-constraint.
- `schemaId` (a `string | symbol`) tags a schema with a stable identity the JSON-Schema and equality machinery key on, distinct from `identifier` (the human-facing name in error paths and the JSON-Schema `$id`) — the two are separate slots because one drives machine de-duplication and the other drives human-readable diagnostics, and conflating them collapses two facts the channel keeps apart.

[MESSAGE_AND_DIAGNOSTIC_SLOTS]:
- `MessageAnnotation = (issue: ParseIssue) => string | Effect<string> | { readonly message: string | Effect<string>; readonly override: boolean }` attaches the decode-failure message to the owner, so the error string a parallel `errorMessages` table would key by shape rides the schema node that produces the failure — and the message may itself be an `Effect<string>`, so a localized or service-resolved message resolves through `R` at the boundary, not a synchronous literal.
- The `override: boolean` form is load-bearing: a refinement's message by default appends to the inner schema's message chain, but `{ message, override: true }` replaces the whole chain — so a composite refinement presents one authored sentence instead of the structural decode trace, the flag selecting append-versus-replace on one annotation rather than two message-construction paths.
- `parseIssueTitle = (issue: ParseIssue) => string | undefined` names the issue node in a multi-error tree (returning `undefined` falls back to `identifier`/`title`), and `decodingFallback = (issue: ParseIssue) => Effect<A, ParseIssue>` lets the owner recover a default-or-fail value from a decode failure inside the decode itself — so a tolerant field that supplies a value on malformed input carries the recovery on its own declaration, the fallback an `Effect` so it can succeed with a default OR re-raise the issue, never a try/catch around the decode at the call site.
- `examples: NonEmptyReadonlyArray<A>` and `default: A` are typed against the DECODED `A`, so an example or documented default that does not satisfy the owner's own type is a compile error on the annotation literal — the documentation cannot drift from the shape it documents because the slot is type-checked against the same `A` the owner fixes.

[CLASS_ANNOTATION_TUPLE_FACES]:
- The class factory's second argument is `ClassAnnotations<Self, A> = Annotations.Schema<Self> | readonly [Annotations.Schema<Self> | undefined, (Annotations.Schema<Self> | undefined)?, Annotations.Schema<A>?]` — a class is a transformation (decoded `Self` over an encoded struct), so it carries THREE annotation faces in tuple position where a plain schema carries one: the decoded-side `Self` annotations, the transformation-node annotations, and the encoded-side `A` (the struct face) annotations.
- The three positions map to the three nodes the class spans: position one annotates the `Self` type the class exposes (its `identifier`, `arbitrary` over instances), position two annotates the transformation between faces (decode/encode diagnostics), position three annotates the underlying struct (`A`) (the wire-face JSON-Schema and field documentation) — so a class targets a message at its construction boundary AND a JSON-Schema at its wire face from one declaration, the tuple splitting what a plain schema cannot.
- `.extend`, `.transformOrFail`, and `.transformOrFailFrom` each take the same `ClassAnnotations<Transformed, A>` tuple on the child, so an inheritance hop re-annotates all three faces of the derived owner — the annotation channel re-threads through inheritance exactly as the `Self` generic does, never a separate annotation pass after the child is declared.
- The record form `Annotations.Schema<Self>` is the abbreviation when only the decoded face needs annotation; reaching for the tuple is required precisely when the encoded/struct face (`A`) carries a fact the decoded face cannot, so the tuple is the absorbed-growth spelling and the record is the instance-sized one that breaks the moment a wire-face annotation is needed.

[EVALUATION_THESIS]:
- The one-statement collapse rests on an unstated runtime invariant: the fused owner must be fully initialized at the program point its first consumer runs, so the levers are governed not only by what they fuse at the type level but by WHEN their value side materializes during module evaluation — and the seam where one statement is structurally impossible is exactly where two evaluation orders conflict.
- The universal collapse spelling `class Self extends Factory<Self>(...args) {}` is the only form simultaneously hoisted as a lexical binding, TDZ-protected against premature read, eagerly evaluated at the declaration line, and self-referential through `Self` — the four properties that let one statement carry value, type, codec, equality, and constructor without a forward declaration or a later patch.
- The minimal-pair law bounds the collapse from below: where a single statement cannot close (a value-recursive shape, an owner whose identity depends on a runtime fingerprint, a type imported under `verbatimModuleSyntax`), the chain collapses to exactly TWO declaration points ordered by evaluation dependency — never three, never a `value -> type -> const -> typeof` walk — and the second point is a deferral thunk, a dual value/type export, or an opaque annotation, never a restatement.

[CLASS_HERITAGE_EAGER_TDZ]:
- `class Self extends Factory(...)` evaluates the heritage expression `Factory(...)` eagerly when the declaration line runs, before the `Self` binding leaves its temporal dead zone — so a `Schema.Class`/`Data.TaggedClass`/`Effect.Service` owner's codec, equality, and constructor are built at module-init, not lazily, and any owner the factory call reads must already be initialized above the line.
- The `Self` binding is hoisted to the top of its block scope but uninitialized until the declaration completes, so a reference to `Self` inside the heritage expression itself is a TDZ throw — the self-reference the `<Self>` type generic carries is purely type-level and never reads the value-side binding during heritage evaluation, which is why the nominal self closes without a value-level cycle while a field that named `Self` directly at the value level would throw.
- The eager-heritage rule fixes the source order of dependent owners: a `class B extends A.extend<B>('B')({ ... })` requires `A` initialized above `B`'s line, and a forward reference to a not-yet-declared base is a TDZ throw at evaluation even though the type checker resolves the names — declaration order is load-bearing at the value level in a way the type level hides.
- The collapse the eager rule forbids is the lazy-owner assumption: an owner referenced before its declaration line cannot be repaired by hoisting because `class` and `const` bindings do not hoist their initialization, so a circular owner dependency is broken by the deferral thunk, never by reordering alone when the cycle is genuine. A forward-declared `const Owner = ...` placed below a consumer that reads it, relying on hoisting, is the TDZ trap: the repair is source reordering when the dependency is acyclic and a `suspend` thunk only when the cycle is genuine, never a `var` hoist that would admit `undefined`.

[DEFERRAL_THUNK_AS_SECOND_POINT]:
- The `suspend` thunk `() => Owner` is constructed eagerly when its enclosing declaration line runs but its body executes only when the schema first decodes, so the owner can name itself or a mutually-recursive sibling that is still in TDZ at the declaration line — the deferral moves the read past module-init to first-use, where a direct value-level reference to the still-uninitialized binding would throw at the eager heritage line.
- A genuine value-level cycle (an owner whose field schema is the owner itself, or two owners each referencing the other) is the one case the single statement cannot close: the eager heritage would read a TDZ binding, so the collapse is a minimal pair — the owner declaration plus the thunk inside the recursive field, the value-side counterpart of the opaque type interfaces the uninferable recursive type already forces; the interface names the shape and the thunk closes the value cycle, neither a restatement of the other.
- The reach stops at recursion depth and entry: the thunk fires per-decode-traversal, so an owner with no genuine cycle gains nothing from `suspend` and pays a thunk allocation per decode — the deferral is admitted only where the eager read would actually throw, a `suspend` wrapped around a non-recursive owner being the precautionary-deferral defect that adds a per-decode allocation and a hop for no cycle it closes.

[DESTRUCTURED_CONSTRUCTOR_FAMILY]:
- `Data.taggedEnum<E>()` returns a record of constructor functions plus `$match` synthesized eagerly at the call line, and the destructuring `const { Push, Pop, $match } = Data.taggedEnum<E>()` is itself the collapse — one statement binding the whole constructor family and the matcher from the one type argument, where the naive chain would declare each variant constructor and the dispatcher separately.
- The constructors are plain functions evaluated at the destructuring line, so the family is available to every consumer below it with no TDZ window of its own — but the type argument `E` (a `Data.TaggedEnum<{ ... }>` alias) must be declared above the destructuring because the call reads it at the type level; the type alias and the value destructuring are a minimal pair where the alias is the type source and the destructuring is the value source, neither restating the other.
- The destructuring binds the variant tags as literal-keyed members, so a later `seq.reduce((acc, c) => acc + $match({ ... })(c), 0)` reads each constructor and the matcher through the one binding — the collapse keeps the constructor family and the fold dispatcher resolvable in one hop, and adding a variant lands as a new tag in the alias plus an arm in `$match`, the destructuring line untouched.
- The split from the schema owner is the boundary again: `Data.taggedEnum` has no codec so the destructured constructors build interior values only, while a variant family that must admit from `unknown` is a `Schema.Union` of `Schema.TaggedClass` owners declared as classes above their first decode — the destructuring collapse is for the codec-free family, the class-heritage collapse for the boundary family.

[NON_INSTANTIABLE_OWNER]:
- `Effect.Tag(id)<Self, Type>()` returns a `Context.TagClass<Self, Id, Type>` whose construct signature is `new (_: never)` — the class-as-value owner is structurally non-instantiable, so `class Self extends Effect.Tag('<id>')<Self, Type>() {}` declares a type, a value-`Tag`, and a key in one statement while the `never` parameter makes any `new Self()` a compile error, the apex no-instantiation owner where the chain's final link is forbidden by the constructor type itself.
- `Effect.Service<Self>()(key, make)` returns a `Service.Class` carrying the same `new (_: never)` non-instantiable shape plus a `Default` Layer member and optional generated accessors, and it gates `Self = never` through the same `MissingSelfGeneric` mechanism the schema owners use — the Effect module's own template `` `Missing \`Self\` generic - use \`class Self extends Effect.Service<Self>()...\`` ``, distinct from the parameterized schema enforcer — so the service owner is a type, a Tag, and a Layer fused in one heritage call, and forgetting `<Self>()` resolves the base to that diagnostic, a `string` subtype no `extends` clause accepts.
- `Service.AllowedType` forbids a `Service` property on the produced shape (a `ProhibitedType` whose `Service` slot is the error-string literal), so the service value cannot accidentally re-expose the constructor surface as a field — the owner's own type fences the one shape that would re-grow the instantiation link.
- The non-instantiable owner is the eager-heritage rule taken to its limit: the heritage call builds the Tag and Layer at the declaration line, the `Default` member is available immediately, and there is no value to construct later — the entire family materializes at module-init from one statement and the `never` constructor proves no second construction follows. A second concrete subclass minted to instantiate the owner, or a wrapper class re-exposing the constructor, is the surface the `ProhibitedType` fence already deletes.

```typescript
import { Effect } from 'effect'
class Clock extends Effect.Service<Clock>()('Clock', {
    sync: () => ({ now: () => 0 }),
    accessors: true,
}) {}
const elapsed = Effect.gen(function* () {
    const clock = yield* Clock
    return clock.now()
})
const wired = Effect.provide(elapsed, Clock.Default)
```

[DECLARATION_MERGING_AND_DUAL_EXPORT]:
- A `namespace X` declared after `class X` merges into the class's value side, attaching statics — seeds, smart constructors, sibling instances — under the same name, so the owner's static surface grows without a second exported symbol or a parallel companion object; an `interface X` declared alongside `class X` merges into the type side, widening the instance type the class already declares — the two-namespace identity of a class name lets one name carry both an extended value surface and an extended type surface from one declaration site.
- The namespace block must follow the class line: the eager heritage builds the class value first and the namespace augments it, so a `namespace Owner` placed ABOVE its `class Owner` line augments an uninitialized binding and the statics resolve against nothing — declaration order is class-then-namespace, the augmentation order the merge requires.
- The merge keeps `verbatimModuleSyntax` honest: the value-side `namespace` members export as values, the type-side `interface` members as types, and the merged owner exports both namespaces through one `export { Owner }` — a barrel re-export or wildcard is forbidden, each owner naming its value and type-only derivations explicitly at the file end so a consumer resolves the owner in one hop.
- Under `verbatimModuleSyntax` the value `export { Owner }` carries the type, so an `export type { Owner }` for a class-as-both-type-and-value owner is unnecessary and a `type X = typeof Owner` minted only to re-export it is the deleted redundant-type-export link — but a name that is genuinely type-only (an extracted `Schema.Schema.Type<typeof Owner>` alias, a self-referential opaque interface) MUST be exported `export type { ... }` or the missing-marker is a compile error, forcing a minimal pair the dual-namespace identity cannot fuse.
- A foreign type imported to annotate the owner is the inbound half of the same seam: `import type { Order } from 'effect/Order'` admits a type-only symbol that erases, and using it without `import type` errors — so an owner that names a non-shape-derived instance (the one `Order.Order<A>` the annotation channel does not absorb) imports it `type` and the owner declaration plus the type-only import are the minimal pair, the import carrying the type the owner cannot derive.

```typescript
import { Schema } from 'effect'
class Unit extends Schema.Class<Unit>('Unit')({ symbol: Schema.String, factor: Schema.Number }) {}
namespace Unit {
    export const base = new Unit({ symbol: '<base>', factor: 1 })
    export const of = (symbol: string, factor: number) => new Unit({ symbol, factor })
}
const scaled = Unit.of('<scaled>', 1000)
```

[LEVER_REACH_LEDGER]:
- `as const`: preserves deep-`readonly` literals and tuple shape; carries no validation, no decode, no equality, no nominal identity; vocabulary owner only.
- `satisfies`: proves conformance without widening, preserves the inferred literal, runs excess-property checking against the contract; fully erased, no decode, no runtime; literal-in-source only.
- `<const T>`: pulls literal/tuple inference to the call site so consumers omit `as const`; erased at compile, no persistence; inference-shaping for that one parameter only.
- rest-tuple capture: preserves each literal in spread position without a `const` modifier; a single array argument collapses to the element type; the spelling for `Schema.Literal`/`Match.is`/`transformLiterals` vocabularies.
- `NoInfer<A>`: removes a position from the inference candidate set so a secondary value (default, fallback) conforms without defining; no runtime check, no narrowing; never on the shape-defining position.
- `Schema.Class<Self>`: fuses type, codec, `Equal`/`Hash`, fields, constructor, and `.extend` inheritance; the deepest owner, consuming `unknown` at the boundary; `<Self>` enforced by `MissingSelfGeneric`.
- `Data.TaggedClass` / `Data.taggedEnum`: fuses tagged constructor and structural `Equal`/`Hash`; no codec, so no boundary admission — interior identity only.
- `Brand.nominal` vs `Brand.refined`: erased phantom identity vs predicate-validated admission; the brand earns its place only on a real nominal collision or a runtime predicate.
- `Effect.Tag` / `Effect.Service`: `new (_: never)` non-instantiable owner fusing type, Tag, and (for Service) `Default` Layer; `ProhibitedType` fences any re-exposed constructor; the apex no-instantiation owner.
- `Schema.suspend`: defers a recursive self-reference behind a per-decode thunk so one schema closes a tree or list; requires opaque self-interfaces on both faces; full codec, admits from `unknown`; admitted only on a genuine value-level cycle.
- higher-kinded `this['A']`: parameterizes a generic union's own definition through `WithGenerics` slots with `numberOfGenerics` arity; produces generic constructors and the `GenericMatchers` surface; structural identity only, no codec.
- declaration merging / dual export: `namespace`/`interface` merge into the value/type sides of one name; `verbatimModuleSyntax` forces `export type` for type-only derivations and `import type` for foreign types; order is class-then-namespace.

[PROJECTION_LEDGER]:
- `Struct.pick`/`.omit` (method): returns `Struct<...>` preserving `.fields`/`.make`/recursive projection; the sub-shape stays a constructor and further projection source; codec, admits from `unknown`.
- `Schema.pick`/`Schema.omit` (pipeable): returns flat `SchemaClass<Pick/Omit>` over any schema; loses `.fields`/`.make`; codec, admits from `unknown`; the spelling for non-`Struct` owners.
- `pluck(key)`: returns a transformation codec from `{ readonly [key]: I[K] }` to `A[K]`; pulls one field across the boundary as its own codec, never `new X().k`.
- `typeSchema` / `encodedSchema` / `encodedBoundSchema`: return `SchemaClass<A>` / `SchemaClass<I>` / `SchemaClass<I>`-with-leading-refinements; codecs over the decoded/wire faces; the runtime counterpart of `Schema.Schema.Type`/`Encoded`.
- `partial` / `required` / `partialWith({exact})`: return `SchemaClass` with mapped optional/required faces; codecs; `exact` splits optional-key from optional-value.
- `keyof`: returns `SchemaClass<keyof A>`; validates an external key against the owner's field set; the codec counterpart of `keyof typeof Owner.fields`.
- `pickLiteral(...literals)`: narrows a literal-union schema to a sub-union of its own members in rest position; the sub-vocabulary the broader owner already contained.
- `Schema.Schema.Type` / `Schema.Schema.Encoded` / `.fields` / indexed access: pure erasure, compile-time only, no codec; the reach when interior code already holds the value.

[CONSTRUCTION_LEDGER]:
- `Struct.Type<F>` / `Struct.Encoded<F>` / `Struct.Constructor<F>`: decoded / wire / construction-argument faces off one field set; the construction face makes defaulted and optional fields optional while the decoded face keeps them required.
- `withConstructorDefault(() => NoInfer<Type>)`: flips `HasDefault` false→true so the field leaves the constructor's required set; needs a prior `propertySignature` lift; fires on `.make`/`new` only, never on decode.
- `withDecodingDefault(() => NoInfer<Exclude<Type, undefined>>)`: flips the decoded token `?:`→`:` and strips `undefined` so the field is always present in memory; leaves `HasDefault` false and the wire token `?:`; fires on decode only.
- `withDefaults({ constructor, decoding })`: applies both in one declaration — decoded-required, wire-optional, constructor-optional; the single owner spelling of a fully-defaulted field.
- `RequiredKeys<C> extends never ? void` / `VoidIfEmpty<S>`: the all-defaulted/empty constructor gate; `Owner.make()` and `new Marker()` take no argument; the schema owner uses `RequiredKeys`, the `Data` owner uses `VoidIfEmpty`.
- `MakeOptions` / `disableValidation`: `.make`/`new` validates field refinements by default; `disableValidation: true` skips them for an already-validated value; `new X` and `X.make` are the same validating producer.
- `Schema.TaggedClass` / `Data.TaggedClass`: `_tag` omitted from the constructor argument and auto-supplied; the schema form admits from `unknown`, the `Data` form is codec-free interior identity only.
- `VariantSchema.Field` (`Generated`, `DateTimeInsert`, `Sensitive`, `FieldOption`): per-variant construction-argument and projection faces off one field config; the insert producer omits generated/auto fields the select reader carries.

[ANNOTATION_CHANNEL_LEDGER]:
- `equivalence` slot / `Schema.equivalence(Owner)`: structural equality derived from the AST by default; the annotation composes one child `Equivalence` per type parameter; replaces a hand-written `(a, b) => ...` beside the owner.
- `arbitrary` slot / `Arbitrary.make(Owner)` / `Arbitrary.makeLazy(Owner)`: fast-check generator honoring refinement constraints; the annotation receives child arbitraries plus `ArbitraryGenerationContext` (`maxDepth`); replaces a parallel `fc.record`.
- `pretty` slot / `Pretty.make(Owner)`: structural printer; the annotation composes one child `Pretty` per type parameter, same fan-in as equivalence; replaces a hand-written formatter.
- `jsonSchema` slot / `JSONSchema.make(Owner)`: wire contract projected from the AST; the annotation MERGES into a refinement's projection, replaces at a non-refinement node; replaces a hand-authored JSON-Schema file.
- `message` (with `override`) / `parseIssueTitle` / `decodingFallback`: decode-failure presentation and recovery on the owning node; `message` and `decodingFallback` may be `Effect`, resolving through `R`; replace an error-message table and a try/catch at the call site.
- `identifier` / `schemaId` / `title` / `description` / `documentation` / `examples` / `default`: identity and documentation facts the structure cannot carry; `examples`/`default` type-checked against the decoded `A`; replace a parallel metadata registry.
- `ClassAnnotations` tuple: three faces (decoded `Self`, transformation node, encoded `A`) where a plain schema carries one; re-threaded through `.extend`/`.transformOrFail`; the record form is the decoded-face abbreviation, the tuple the wire-face-carrying spelling.
- `Order`: NO annotation slot and NO `Schema`-level derivation; supplied as a separate `Order.Order<A>` argument to `SortedSet`/`SortedSetFromSelf`; the one shape-derived instance the channel does not absorb.

[MINIMAL_PAIR_LEDGER]:
- single statement closes: a non-recursive owner with no foreign type and no runtime fingerprint — `class Self extends Factory<Self>(...) {}` fuses value, type, codec, equality, and constructor at the eager heritage line; the only spelling, no second point.
- value-recursive cycle: the owner declaration plus a `Schema.suspend(() => Owner)` thunk inside the recursive field — the eager heritage would read a TDZ binding, so the thunk defers the read to first-decode; the type faces are opaque interfaces declared above, the value cycle closed by the thunk.
- codec-free variant family: the `Data.TaggedEnum` type alias plus the `Data.taggedEnum<E>()` destructuring — the alias is the type source, the destructuring the value source; the constructors and `$match` materialize eagerly at the destructuring line.
- type-only derivation or foreign type: the owner value declaration plus an `export type`/`import type` statement — `verbatimModuleSyntax` forbids fusing a type-only symbol into the value export, so the second point carries the erased type explicitly.
- static augmentation: the `class Owner` line plus a following `namespace Owner` block — both eager, the namespace augments the already-built class value, so order is class-then-namespace and the pair exports through one `export { Owner }`.

[CHAIN_ANTIPATTERNS]:
- `const x = { ... }; type X = typeof x` is the type-after-value link: collapse it by asserting `as const` on the literal and reading `keyof typeof x` / indexed access where the type is needed, never minting a standalone alias.
- A second `const` restating a declared value — a companion frozen copy, a re-export under a new name, a derived constant inlining what the table already holds — is the value-after-value link the indexed read deletes.
- A separate `new X()` following a schema declared as the type-and-codec source is the instantiation-after-declaration link: when the boundary decodes the value the manual construction is dead; when interior equality is the only need the owner is `Data.TaggedClass` and the `new` is the construction, not a restatement.
- A parallel `interface` mirroring a `Schema.Class`'s instance shape is the type-beside-owner link — the class already IS the type; `Schema.Schema.Type<typeof X>` lifts it, and the mirror interface is deleted.
- A sub-schema declared beside the owner for a narrower shape — `const OwnerKey = Schema.Struct({ id: Schema.String })` next to an `Owner` that already has `id` — is the parallel-sub-shape link `Owner.pick('id')` deletes; the projection derives the narrower codec, the parallel struct restates the field.
- `new Owner({ ...field }).field` written to read a single value is the instantiation-then-pick link `Owner.pipe(Schema.pluck('field'))` deletes at the codec level and indexed access deletes at the type level — the construction is dead surface when the only use is the projection.
- A standalone `const eq: Equivalence<A> = Equivalence.make((a, b) => ...)`, a hand-authored JSON-Schema literal, a `fc.record` restating the field set, or an external error-message map keyed by shape are the parallel-derivation links the annotation channel deletes: the projection IS the owner read through `Schema.equivalence`/`JSONSchema.make`/`Arbitrary.make`/`message`, and a per-leaf annotation patches the single node the structural default gets wrong.

[COLLISION_LEDGER]:
- `as const` argument into a `<const T>` parameter: redundant — the parameter already infers deep-`readonly` literals, so the argument-site `as const` is deletable noise; keep the parameter modifier, drop the call-site assertion.
- `: T` annotation on a value feeding any literal-preservation lever: defeats it — an explicit annotation widens before the lever runs, so `const x: string = '<v>'` passed to `<const T>` infers `string`; the annotation is removed and the lever or a `satisfies` clause carries the contract without widening.
- single array argument where a rest position expects spreads: defeats tag extraction — `f(['<a>', '<b>'])` widens to the element type while `f('<a>', '<b>')` preserves the tuple; the array and any `as const` on it are deleted in favor of the spread.
- `NoInfer` on the shape-defining position: inverts the source of truth — placing the barrier on the primary argument leaves the type with no inference source; it belongs only on the secondary position that must conform, never on the one that defines.
- `satisfies` standing in for decode at a runtime boundary: a false fence — it gates source literals at compile and is erased, so a value from `unknown` that is `satisfies`-asserted is unchecked at runtime; the boundary decodes, the source `satisfies`-checks, and the two are never swapped.
- a second `.annotations(...)` hop placed AFTER a pipe that widened the owner to a bare `Schema`: the surface-lost link — the `Annotable.Self` return preserves `.fields`/`.make` only while the owner is still its api-interface type, so the annotation hop precedes the flattening hop, never trails it.
- a `Schema.suspend` thunk wrapped around a non-recursive owner: the precautionary-deferral defect — the eager read never throws for an acyclic owner, so the thunk adds a per-decode allocation and a hop for no cycle it closes; `suspend` is admitted only at the field whose schema is genuinely the owner or a mutually-recursive sibling in TDZ.
- a `namespace Owner` block placed ABOVE its `class Owner` line: the order-inverted augmentation defect — the eager heritage has not yet built the class value, so the namespace augments an uninitialized binding; the namespace always follows the class.
