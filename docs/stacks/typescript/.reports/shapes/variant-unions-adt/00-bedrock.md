# Variant Unions and ADTs

[OWNER_SELECTION]:
- `Data.taggedEnum<A>()` is the decode-free variant owner: one call over a `_tag` union yields per-tag constructors plus `$is` and `$match` as members of the returned record — value family, structural `Equal`/`Hash`, and total dispatch in one statement, no `type X = typeof x` chain.
- `Schema.Union(...members)` of `Schema.TaggedClass` is the boundary-carrying owner: each member is at once a static type, a value-constructor, a codec, and an `Equal`/`Hash` instance, and the union is itself a `Schema` whose `Type` decodes any member by `_tag` discriminant.
- The seam selects the owner, not the cases: a union that never crosses a wire takes `Data.taggedEnum` at zero codec weight; a union admitted through `Schema.decodeUnknown` takes `Schema.Union` of `TaggedClass`.
- `Data.taggedEnum` rejects a member already carrying `_tag`: `UntaggedChildren<A>` resolves to the string-literal error type `"It looks like you're trying to create a tagged enum, but one or more of its members already has a `_tag` property."`, so the constructor fails to type rather than silently double-tagging.

[TAGGED_ENUM_SURFACE]:
- The return of `Data.taggedEnum<A>()` is `TaggedEnum.Constructor<A>`: one constructor per tag keyed by `A["_tag"]`, each a `Case.Constructor` that omits `_tag` from its argument object and re-attaches it.
- `$is(tag)` returns `(u: unknown) => u is Extract<A, { _tag: tag }>` — a narrowing guard derived from the owner, deleting any hand-written `x._tag === "<tag>"` predicate.
- `$match` carries two arities in one member: data-last `$match(cases)` returns `(value: A) => R`, data-first `$match(value, cases)` returns `R` directly; both are total, and `cases` keyed by any tag outside `A["_tag"]` is constrained to `never`.
- `$match` is total-only by construction; partial dispatch with a fallback routes through `Match.tags` (optional handlers) or `Match.discriminators`, which carry the `orElse` rail.
- The per-tag constructor argument is `TaggedEnum.Args` — the member's fields minus `_tag` wrapped in `VoidIfEmpty` — so a fieldless variant constructs by calling with no argument (`Tree.Empty()`), never `Tree.Empty({})`: the argument type collapses to `void` and passing `{}` is an excess-argument error.

[GENERIC_ADT]:
- A generic ADT is an `interface` extending `Data.TaggedEnum.WithGenerics<N>` whose `taggedEnum` member is the parameterized union written against the self-type slots `this["A"]` through `this["D"]`; `WithGenerics` carries exactly four slots and a `numberOfGenerics: N` witness.
- `this["A"]` is the only legal slot reference inside `WithGenerics`; a free type parameter declared on the interface (`<E, A>`) is not threaded into `Kind` and is inert, breaking constructor inference — the self-type slots are the binding mechanism.
- The arity-`1` through arity-`4` overloads of `taggedEnum` each re-thread the slots through `TaggedEnum.Kind<Z, A, B, C, D>`, so a constructor for a generic owner is itself generic — `Tree.Leaf<A>(args)` infers `A` per call site, never at owner declaration.
- A bifunctor ADT is `WithGenerics<2>` whose `taggedEnum` reads two slots (`taggedEnum: Result<this["A"], this["B"]>`); `Data.taggedEnum<Def>()` returns constructors generic in both, `Result.Failure({ error })` inferring the `error` slot and `Result.Success({ value })` the `value` slot, each independently per call.
- The generic owner exposes `$is`/`$match` through `TaggedEnum.GenericMatchers<Z>`, a distinct shape from the monomorphic `Constructor`: generic `$match` pins slots either from a concrete `Self` (the data-first form infers all four) or from four explicit type arguments (the data-last form), because the matcher cannot infer slot bindings from the handler record alone.

[SCHEMA_UNION_SEAM]:
- `Schema.TaggedClass<Self>(identifier?)(tag, fields, annotations?)` demands the `Self` generic; omitting it resolves to `MissingSelfGeneric<"TaggedClass">` rather than a usable class, so `class Node extends Schema.TaggedClass<Node>()("Node", { ... }) {}` is the only well-typed spelling.
- The generated `_tag` field is `tag<Tag>`, omitted from the constructor argument via `Struct.Constructor<Omit<Fields, "_tag">>` — `new Node({ ... })` never passes `_tag`, and `_tag` is recoverable as a literal-typed member for downstream `Match.tag`.
- `Schema.Union(...members)` is overloaded by arity: a single member returns that member unchanged, zero members returns `typeof Schema.Never`, and `N >= 2` returns `Union<Members>` whose `Type` is `Schema.Type<Members[number]>` — the decoded value is the bare union, ready for `$match`-style dispatch.
- The union AST `types` is `Members<M>` = `readonly [A, A, ...Array<A>]` — the two-or-more arity is structural, mirroring the value-level overload where the `Union<Members>` interface materializes only at arity `>= 2`.
- `Schema.is(union)` yields `(u: unknown) => u is Type` and `Schema.asserts(union)` yields an assertion signature — the runtime guards derive from the union owner, never restated as a parallel predicate.
- `Schema.Union` is `AnnotableClass`, so `union.annotations({ identifier: "<name>" })` names the whole family for `$defs` emission and error messaging without dissolving the member partition — the annotation rides the union node, the per-arm discriminant buckets unchanged.

[TAG_AS_DEFAULTED_SIGNATURE]:
- `Schema.tag(literal)` returns `tag<Tag>` = `PropertySignature<":", Tag, never, ":", Tag, true, never>`: the sixth slot `HasDefault = true` is the load-bearing bit — the `_tag` field carries its own literal as a constructor default, so `Struct.Constructor<{ _tag: tag<Tag> } & Fields>` strips `_tag` to optional and `Class.make`/`new` never pass it, the discriminant materializing from the signature itself.
- `Schema.TaggedStruct(tag, fields)` is `Struct<{ _tag: tag<Tag> } & fields>` — the decode-free analogue of `TaggedClass` for a union arm that needs no nominal class identity: `TaggedStruct("Open", { at: Schema.Int }).make({ at: 1 })` yields `{ _tag: "Open", at: 1 }`, the tag supplied by the signature default, never spelled at the call.
- The `tag<Tag>` default differs from a `Schema.Literal` field: a bare `_tag: Schema.Literal("Open")` is a required constructor argument, so `make({ at: 1 })` fails — `Schema.tag` is the only spelling that makes the discriminant simultaneously decode-validated, encode-emitted, and constructor-elided.
- `Schema.tag` accepts `AST.LiteralValue` (string, number, boolean, bigint, `null`), so a numeric-tagged variant family (`_tag: Schema.tag(0)`) discriminates on a number literal with the same default elision — the discriminant axis is not string-bound.

[RECURSIVE_ADT]:
- A self-referential boundary variant closes through `Schema.suspend<A, I, R>(f: () => Schema<A, I, R>)`: the lazy thunk defers the recursive reference past the owner's own initialization, and the three type arguments are supplied explicitly because TypeScript cannot infer a type still being declared.
- The recursive reference points at the `Schema.Union` value, not at a sibling class, so each class is declared before the union and the `suspend` thunk closes over the union lazily — a class declared after the union it joins is a forward-reference error the value form forbids.
- The `suspend` annotation on the field is the recursion point: annotating the thunk return as `Schema.Schema<Expr>` is mandatory, since an un-annotated thunk infers `any` and erases the recursive structure.
- A `Data.taggedEnum` recursive owner needs no `suspend`: the recursive reference is a plain type-level self-reference (`Tree<A>` inside its own definition) because the decode-free owner carries no codec to bootstrap.

```typescript
import { Schema } from 'effect'

type Expr = Lit | Add
class Lit extends Schema.TaggedClass<Lit>()('Lit', { value: Schema.Number }) {}
class Add extends Schema.TaggedClass<Add>()('Add', {
    left: Schema.suspend((): Schema.Schema<Expr> => Expr),
    right: Schema.suspend((): Schema.Schema<Expr> => Expr),
}) {}
const Expr = Schema.Union(Lit, Add)
const decode = Schema.decodeUnknown(Expr)
```

[DECODE_SEARCH_TREE]:
- Decoding a `Schema.Union` does not try arms left-to-right by default: the parser builds a search tree partitioning members into `keys` (every member carrying at least one property whose schema is a `Literal`) and `otherwise` (members with no literal-valued property), so a tagged family decodes by a single discriminant lookup, not an N-arm structural scan.
- Within `keys`, the tree nests `propertyName -> literalHash -> bucket`: a `Schema.Union` of `TaggedClass` collapses to one namespace `_tag` whose buckets are singletons, so the decoder reads `actual["_tag"]`, hashes it, and attempts only the one member in that bucket — adding a tagged arm widens the bucket map by one key, never lengthening the per-decode scan.
- The partition is greedy per member: the builder walks a member's literal properties and stops at the first property whose literal value is not yet a bucket key, so two members sharing a `_tag` literal but differing on a second literal property (`status: "ok" | "fail"`) are disambiguated on the *first distinguishing* literal, not necessarily `_tag` — discriminant choice is positional over the literal set, not name-fixed.
- `otherwise` members are appended to every bucket's candidate list and tried after the discriminated candidates, so a union mixing `TaggedClass` arms with a bare `Schema.Struct` arm pays full structural cost only on the undiscriminated arm — the literal arms still short-circuit, and a non-literal arm placed first does not steal a decode that a later literal arm owns.
- On total failure the located `ParseIssue` is reconstructed against the minimal candidate set, not the whole union: when the failing candidates equal the full member count the error AST is the original union, otherwise it is `AST.Union.make(candidates)` — the diagnostic names only the arms the discriminant admitted, so a wrong `_tag` reports against that one bucket's expected shape rather than the entire family.
- A nested `Schema.Union(a, Schema.Union(b, c))` is admitted but the decode search tree walks `ast.types` of the outer union only; a nested union arm is itself an arm whose own `getLiterals` is read, so deep nesting still discriminates but allocates a member node per level — flatten at construction (`Schema.Union(a, b, c)`) so the search tree partitions one flat member list, not a tree of unions each re-entered.

[INJECTED_DISCRIMINANT]:
- `attachPropertySignature(key, value)` adds a discriminant to a discriminant-free struct at the decode boundary only: the result types as `SchemaClass<A & { readonly [k in K]: V }, I, R>` — the value `A` gains the literal-keyed field, the encoded `I` is unchanged, so the tag is computed on decode and stripped on encode, never stored on the wire.
- The injected `value` is constrained to `AST.LiteralValue | symbol`, so the attached field is a `Literal` schema and participates in `getSearchTree` exactly like a native `_tag` — a `Schema.Union` of `attachPropertySignature`-wrapped structs disambiguates by the injected key with the same single-lookup decode as a `TaggedClass` union, without the wire carrying the tag.
- The boundary it owns is the legacy seam: an external payload discriminated by shape (presence of `radius` vs `side`) rather than a stored tag is admitted once, the interior gaining a uniform `kind` discriminant the whole `Match`/`$is` family keys on — re-deriving the tag at every interior read is the deleted spelling.
- Two arities share the surface: data-last `schema.pipe(attachPropertySignature(key, value))` for pipe composition, data-first `attachPropertySignature(schema, key, value)` for a direct call — neither is a separate combinator, the input position selects the form.
- A `symbol` discriminant is legal where a string key would collide with a wire field; the attached signature is then unreachable by JSON but live for interior `Match.discriminator` dispatch — the discriminant axis is decoupled from the serialized field set.

```typescript
import { Schema } from 'effect'

const Circle = Schema.Struct({ radius: Schema.Positive })
const Square = Schema.Struct({ side: Schema.Positive })
const Shape = Schema.Union(
    Circle.pipe(Schema.attachPropertySignature('kind', 'circle')),
    Square.pipe(Schema.attachPropertySignature('kind', 'square')),
)
const decode = Schema.decodeUnknownSync(Shape)
const area = (s: Schema.Schema.Type<typeof Shape>): number =>
    s.kind === 'circle' ? Math.PI * s.radius ** 2 : s.side ** 2
const measured = area(decode({ radius: 2 }))
```

[LITERAL_TABLE_OWNER]:
- `Schema.Literal(...values)` is the closed discriminant vocabulary owner: `Literal<Literals>` exposes `literals` (the live tuple) and types as `Literals[number]`, so the legal-value set is the schema, not a parallel `as const` union restated beside it — `Schema.Schema.Type` of the literal is the bare string union.
- `pickLiteral(...subset)` narrows a literal owner to a sub-vocabulary returning `Literal<[...L]>`, so a permission tier or status subset is one projection off the parent literal table, never a second hand-spelled union — the subset's membership is checked against the parent at the type level.
- `transformLiterals(...[from, to])` is the bidirectional remap table: each pair becomes a `transformLiteral<To, From>` and the call returns a `Union` of them, so a wire-code-to-domain-name mapping (`0 -> "draft"`, `1 -> "live"`) is one owner that decodes the code to the name and encodes the name back to the code — the inverse map is derived, never authored as a second lookup.
- A single `transformLiteral(from, to)` is the unary cell of the same surface; `transformLiterals` over an array of pairs is the table form whose decoded type is `A[number][1]` and encoded type is `A[number][0]` — adding a code-name pair widens both projections with one row, the `Match.exhaustive` over the decoded names breaking loudly until the new name is handled.

```typescript
import { Schema } from 'effect'

const Status = Schema.transformLiterals(
    [0, 'draft'],
    [1, 'live'],
    [2, 'archived'],
)
const decodeStatus = Schema.decodeUnknownSync(Status)
const encodeStatus = Schema.encodeSync(Status)
const open: 'draft' | 'live' | 'archived' = decodeStatus(1)
const wire: 0 | 1 | 2 = encodeStatus('archived')
```

[MATCH_DISPATCH]:
- `Match.type<I>()` builds a reusable matcher whose finalizer returns `(i: I) => R`; `Match.value(i)` carries the input as `Provided`, so its finalizer returns `R` directly — a value-matcher cannot be reused across inputs.
- `Match.tag(...tags, f)` and `Match.discriminator(field)(...values, f)` are variadic in their leading tags: one arm handles several tags sharing a body, collapsing repeated near-identical handlers into one row.
- `Match.tags(handlers)` / `Match.discriminators(field)(handlers)` take a record of optional handlers and leave the matcher open for `Match.orElse`; `Match.tagsExhaustive` / `Match.discriminatorsExhaustive` take required handlers and self-finalize.
- `Match.tagStartsWith(prefix, f)` / `Match.discriminatorStartsWith(field)(prefix, f)` match by `_tag` prefix `` `${P}${string}` ``, folding a namespaced tag family (`"event.open"`, `"event.close"`) into one arm without enumerating each leaf.
- `Match.when(pattern, f)` matches by partial-structure pattern or refinement; `Match.whenOr(...patterns, f)` matches any of several; `Match.whenAnd(...patterns, f)` matches the intersection of several.
- Five finalizers close a `Match.type`/`Match.value` chain by residual handling: `Match.exhaustive` for a proven-total matcher, `Match.orElseAbsurd` throwing with no fallback handler, `Match.orElse(f)` supplying a typed fallback, `Match.either` reifying the residual as a `Left`, `Match.option` collapsing it to `None`.
- `Match.valueTags(handlers)` and `Match.typeTags()(handlers)` are standalone entrypoints, not pipe steps: each consumes a full required-handler record over `_tag` and returns the dispatch directly, the most collapsed spelling for total `_tag` dispatch with no intermediate matcher binding.
- The `tag`/`tags` family hardwires the `"_tag"` field name; a union discriminated on `"type"` or `"kind"` routes through `Match.discriminator(field)` / `Match.discriminators(field)`, which take the field as the first argument.

[FILTER_MONOID]:
- A `Match` pipeline is a type-level fold: `Match.type<I>()` seeds the matcher at `Matcher<I, Without<never>, I, never, never>` — `Filters` empty as `Without<never>`, `Remaining` the full input `I`, `Result` accumulating as `never` — and every combinator folds one filter into `Filters` and recomputes `Remaining` from it.
- `Filters` is a two-constructor lattice — `Without<X>` (everything except `X` still unconsumed) and `Only<X>` (only `X` still unconsumed) — and `Remaining` is never tracked directly: `ApplyFilters<I, Filters>` recomputes it each step, mapping `Without<X>` to `Exclude<I, X>` and `Only<X>` to `X` verbatim.
- `AddWithout<F, X>` is the positive-arm fold: a `Without<WX>` accumulates to `Without<X | WX>` (subtract another slice), an `Only<OX>` to `Only<Exclude<OX, X>>` (carve the slice from the surviving-only set) — the same arm shrinks `Remaining` whichever lattice side the matcher currently sits on.
- Every positive combinator (`when`/`whenOr`/`whenAnd`/`tag`/`tags`/`discriminator`/`discriminators`/`tagStartsWith`/`discriminatorStartsWith`) threads `Result` as `A | ReturnType<Fn>` and `Filters` as `AddWithout<F, <consumed>>`; the only axis that varies is *which* slice each computes as consumed.
- `whenAnd(...patterns, f)` consumes `UnionToIntersection<P[number]>` on both the handler match (`WhenMatch<R, UnionToIntersection<...>>`) and the residual — one arm subtracts the values satisfying *all* patterns, the conjunction computed at the type level, never an enumerated and-chain.
- `whenOr(...patterns, f)` consumes `PForExclude<P[number]>` — the union over the pattern array — so several disjoint slices collapse into one row and one `Result` contribution; the residual subtracts their union in a single fold step.
- Or-arm, and-arm, and tag-arm stack on the same `Matcher` carrier as `when`, interleaving freely, and the running `Remaining` is the cumulative `Exclude<I, <each consumed slice>>` regardless of arm kind — the dispatch surface is one fold over a heterogeneous filter sequence.

[EXHAUSTIVE_PROOF]:
- `Match.exhaustive` has signature `(self: Matcher<I, F, never, A, Pr, Ret>) => ...`: it accepts the matcher only when the third type parameter `Remaining` is literally `never`, so exhaustiveness is a structural constraint on the recomputed residual, not a runtime scan — a missing variant leaves `Remaining` non-`never` and the call fails to type with `Type '<Variant>' is not assignable to type 'never'`.
- The exhaustiveness obligation lives on `Remaining`, which `ApplyFilters` derives from `Filters` — adding a union member widens `I`, widens `ApplyFilters<I, Without<consumed>>`, and re-opens `Remaining` at every `exhaustive` site, so a new case breaks every total matcher loudly with one diagnostic each.
- `tagsExhaustive`/`discriminatorsExhaustive` skip the `Remaining = never` gate entirely: their handler record `P` is constrained to require a key for every `Tags<"_tag", R>` and forbid foreign keys via `[Tag in Exclude<keyof P, Tags<...>>]: never`, so totality is enforced on the record shape and the combinator self-finalizes — no trailing `exhaustive`, the function-vs-value return selected in the same step.

[REFINEMENT_VS_PREDICATE]:
- A pattern's consumed-slice is `PForExclude<P> = SafeRefinementR<ToSafeRefinement<P>>`, and `ToSafeRefinement` splits on the predicate kind: a `Predicate.Refinement<A, B>` becomes `SafeRefinement<B, B>` and a bare `Predicate.Predicate<A>` becomes `SafeRefinement<A, never>`.
- `SafeRefinementR` then reads the second slot: `SafeRefinement<B, B>` yields `B` (the refined type is subtracted from `Remaining`), but `SafeRefinement<A, never>` yields `never` (nothing subtracted) — a non-refining predicate matches at runtime yet consumes zero residual, so `exhaustive` still demands the case it visibly handled.
- The trap is silent at the arm and loud at the finalizer: `Match.when((x) => x.n > 0, f)` over a refinement-less guard leaves `Remaining` untouched, so the matcher reads as incomplete; only a `Refinement` (`(x): x is B`) or a literal/structural pattern shrinks the residual — route narrowing guards as `Refinement`, never `Predicate`, when totality is the goal.
- `Match.nonEmptyString` and `Match.instanceOf(C)` are `SafeRefinement<T, never>` — matched, not subtracted — whereas `Match.string`/`Match.number`/`Match.boolean` are real `Predicate.Refinement<unknown, T>` that do subtract; `Match.instanceOfUnsafe(C)` is `SafeRefinement<InstanceType<C>, InstanceType<C>>`, the residual-consuming counterpart of `instanceOf`.

[INVERTED_LATTICE]:
- `Match.not(pattern, f)` is the only combinator that switches the lattice constructor: its filter fold is `AddOnly<F, WhenMatch<R, P>>`, where `WhenMatch<R, P>` is the *matched* slice of `pattern` — and the handler `f` receives `NotMatch<R, P> = Exclude<R, ExtractMatch<R, PForNotMatch<P>>>`, the complement. The residual tracks the matched slice while the handler sees its inverse, so the two type parameters intentionally diverge.
- `AddOnly<Without<WX>, X>` is `[X] extends [WX] ? never : Only<X>`: applied to a fresh `Without<never>` matcher it flips `Remaining` to `Only<X>`, so `ApplyFilters<I, Only<X>>` becomes `X` — the slice `pattern` *matched*, left unconsumed because `not` handled its complement. A `not` does not complete a match; it leaves exactly the not-pattern still owing.
- After the flip, a positive arm consuming the surviving slice closes the lattice: `AddWithout<Only<X>, X>` is `Only<Exclude<X, X>>` = `Only<never>`, and `ApplyFilters<I, Only<never>>` is `never` — so the canonical complete spelling is `not(pattern, f)` paired with one arm that consumes `pattern`'s own slice.
- A second `not` intersects: `AddOnly<Only<OX>, X>` is `[X] extends [OX] ? Only<X> : never`, narrowing the surviving-only set toward the intersection — and a `not` whose matched slice escapes the current `Only` window collapses `Filters` to `never`, poisoning every downstream `ApplyFilters` to `never` and silently satisfying `exhaustive` for the wrong reason.

```typescript
import { Match } from 'effect'

type Signal =
    | { readonly _tag: 'open'; readonly at: number }
    | { readonly _tag: 'retry'; readonly count: number }
    | { readonly _tag: 'closed'; readonly reason: string }

const classify = Match.type<Signal>().pipe(
    Match.withReturnType<string>(),
    Match.not({ _tag: 'closed' }, (live) => `live:${live._tag}`),
    Match.tag('closed', (c) => `closed:${c.reason}`),
    Match.exhaustive,
)
```

- `not({ _tag: 'closed' }, live => ...)` hands `live` the complement `open | retry` and flips `Filters` to `Only<closed>`, so `Remaining` is `closed` — the not-pattern itself still owes; the trailing `tag('closed', ...)` folds `AddWithout<Only<closed>, closed>` to `Only<never>`, driving `Remaining` to `never` so `exhaustive` clears. Inverting the arm order (`tag` first, `not` second) leaves `Only<closed>` unconsumed and fails the finalizer.
- `withReturnType<string>()` placed first pins `Return` while `Result` is still `never` (its constraint `[Ret] extends [[A] extends [never] ? any : A]` passing `any`), then constrains each subsequent `ReturnType<Fn>` against `string`; placed after an arm it compares against an already-populated `Result` and stops gating later arms.

[NON_TOTAL_CLOSE]:
- `Match.orElse(f)` accepts any `Remaining` (`RA`) and binds `f: (_: RA) => Ret`: the fallback handler is typed at exactly the unconsumed residual, so it narrows to precisely the cases no arm claimed — never `unknown`, never the full `I`.
- `Match.either` and `Match.option` finalize a non-total matcher by reifying the residual as a typed payload: `either` returns `Either<Unify<A>, R>` with the unmatched `R` as the `Left`, `option` returns `Option<Unify<A>>` with the residual collapsed to `None` — the `Left`/`None` type is the computed `Remaining`, never a discarded value.
- `orElseAbsurd` finalizes with no handler by asserting the residual is unreachable at runtime; unlike `exhaustive` it does not require `Remaining = never` at compile time, so it is the escape hatch when a refinement-less predicate left a phantom residual the programmer knows is empty.
- Every finalizer keys its return on `[Pr] extends [never]`: a `Match.type<I>()` chain (`Provided = never`) finalizes to a reusable `(input: I) => R`, a `Match.value(i)` chain (`Provided = I`) finalizes to `R` directly — the same finalizer is function-producing or value-producing by the matcher's provenance, never a separate entrypoint.

[MEMBER_VALUE_RECOVERY]:
- `Data.TaggedEnum.Value<A, K>` is `Extract<A, { readonly _tag: K }>` — one arm's full value shape (tag plus fields) recovered by indexed tag, never a per-arm `interface` mirroring the same fields the owner already carries.
- `Data.TaggedEnum.Args<A, K, E>` is the same member minus `_tag`, wrapped in `Types.VoidIfEmpty` — the exact constructor-argument type for arm `K`, so a function accepting "what builds a `Branch`" types its parameter as `TaggedEnum.Args<Tree<number>, "Branch">` rather than re-listing `left`/`right`.
- `Schema.Schema.Type<S>` and `Schema.Schema.Encoded<S>` are `infer`-projections over `Schema.Variance<A, I, R>`; applied to one `TaggedClass` they yield that arm's decoded and wire shape, applied to the `Schema.Union` they yield `Schema.Type<Members[number]>` — the bare union — so the same extractor reads one arm or the whole family by which schema it is fed.
- The decode-free owner needs no schema extractor: `ReturnType<typeof Tree.Leaf>` recovers a constructed `Leaf` value directly off the constructor member, and `Parameters<typeof Tree.Leaf>[0]` recovers its argument — `typeof`-over-the-constructor is the `Data` analogue of `Schema.Schema.Type`-over-the-class.

[NARROW_PRESERVING_GUARD]:
- The monomorphic `$is(tag)` is one signature `(u: unknown) => u is Extract<A, { _tag: tag }>` — `unknown` input, recovered arm output — so it unifies with `Array.prototype.filter`'s predicate callback and `values.filter(Sig.$is("Open"))` over `ReadonlyArray<Sig>` yields `ReadonlyArray<Extract<Sig, { _tag: "Open" }>>`, deleting the hand-written `(x): x is Open => x._tag === "Open"` that restates the discriminant.
- `TaggedEnum.GenericMatchers.$is` carries a dual the monomorphic form lacks: a first overload `<T extends Kind<Z, any, any, any, any>>(u: T) => u is T & { readonly _tag: Tag }` intersecting the *caller's* slot bindings with the tag, and a second `(u: unknown) => u is Extract<Kind<Z>, { _tag: Tag }>` collapsing slots to `unknown` — so guarding a known `Tree<number>` keeps `number` in the leaf, guarding a foreign value recovers the arm with `unknown` slots.
- The generic dual narrows only at a direct call site: `if (Tree.$is("Leaf")(node))` over a known `node: Tree<number>` refines through the first overload and `node.value` is `number`, but the generic `<T>` cannot be solved in the higher-order `filter` callback position, so `values.filter(Tree.$is("Leaf"))` leaves the element at the full union and `.value` on the result is a compile error. The boundary: monomorphic guards filter, generic guards narrow only directly applied — a generic family filtered by arm pins a concrete instantiation first or routes through `Array.filterMap` with a `$match` arm.

[FIELD_SCHEMA_REENTRY]:
- A `Schema.TaggedClass` owner exposes `.fields` typed `{ readonly [K in keyof Fields]: Fields[K] }` — the live per-field schema map, `_tag` present as a `tag<Tag>` property signature — so a member-scoped re-derivation reads `Open.fields.at` to reach the `at` field's schema rather than re-declaring it.
- The `_tag` field is the `tag<Tag>` default-carrying signature, so `Schema.Schema.Type<typeof Open.fields._tag>` is the literal `"Open"`, the discriminant recoverable as a singleton type for a `satisfies`-anchored rank row.
- `union.members` is typed `Readonly<Members>` — the constituent classes in declaration order at their exact static type, `members[0]` recovering the `Open` class itself (a `Schema.decodeUnknownSync(members[0])` decodes precisely that arm), never an erased `Schema.Any` that would force re-declaring the class to recover its codec; `ast.types` carries the erased AST nodes the compiler walks, so a custom AST pass reads `types` and a typed-class projection indexes `members`.
- `Schema.getNumberIndexedAccess(arraySchema)` lifts an array schema to its element `SchemaClass<A[number], I[number], R>`; a union persisted as `Schema.Array(union)` recovers its element family through this, the lifted element being the same tagged union — the discriminant survives the array wrapping, and a wire payload's per-element decode reads the element schema rather than the array.

```typescript
import { Schema } from 'effect'

class Open extends Schema.TaggedClass<Open>()('Open', { at: Schema.Int }) {}
class Retry extends Schema.TaggedClass<Retry>()('Retry', { count: Schema.Int }) {}
const Signal = Schema.Union(Open, Retry)
const decodeOne = Schema.decodeUnknownSync(Signal.members[0])
const atField: Schema.Schema<number> = Open.fields.at
const project = (s: Schema.Schema.Type<typeof Signal>): number => (s._tag === 'Open' ? s.at : s.count)
```

[GENERIC_SLOT_REENTRY]:
- `TaggedEnum.Kind<Z, A, B, C, D>` is the slot-application operator: `(Z & { A; B; C; D })["taggedEnum"]` instantiates the parameterized union at concrete arguments, so the generic owner's value type at chosen slots is `Kind<TreeDef, number>` — the four-slot signature is fixed even for a one-generic owner, the unused slots resting at their `unknown` default.
- The generic constructor re-infers slots per call: each tag member is `<A>(args: TaggedEnum.Args<Kind<Z, A>, Tag, ...>) => TaggedEnum.Value<Kind<Z, A>, Tag>`, so `Tree.Leaf({ value: 1 })` binds `A = number` at the call, and the recovered value type is `Value<Kind<TreeDef, number>, "Leaf">` — the slot is never pinned at owner declaration, only at construction.
- Slot recovery from a constructed value is structural, not nominal: a function over `Tree<A>` cannot extract `A` by `infer` through the opaque `Kind` indirection, so a generic fold pins the result at a concrete instantiation (`Tree<number> -> number`) and lets `Value`/`Args` recover the per-arm shapes at that fixed slot — extracting the slot itself is the rejected move.

[OWNER_WIDENING_REENTRY]:
- `Class.extend<Extended>(identifier)(newFields)` widens one member to `Fields & NewFields`, re-threading `Struct.Type<Fields & NewFields>` through the value type, `I & Struct.Encoded<NewFields>` through the wire type, `C & Struct.Constructor<NewFields>` through the constructor, and `R | Struct.Context<NewFields>` through the requirement — a new field on one arm lands as one `extend` call and every projection of that arm recomputes, the sibling arms untouched.
- `extend` demands its own `Extended` self-generic: omitting it resolves to `MissingSelfGeneric<"Base.extend">`, the same self-type gate the base `TaggedClass<Self>` carries, so a widened arm is a first-class new class with its own identity, not a structural alias of the base.
- `Schema.extend(union)` over a struct distributes the extension across the union arms: the supported-extension set explicitly admits "a struct with a union of supported schemas", so `base.pipe(Schema.extend(Schema.Union(x, y)))` yields a union whose each arm is `base & x`, `base & y` — a common field set is factored into the base and folded into every variant by one combinator, not appended to each `TaggedClass` by hand.
- The distributed-extension type-level result is `Schema.Type<Self> & Schema.Type<That>` (intersection) while the AST-level result distributes the intersection over the union members, so the decoded type reads as `Base & (X | Y)` which TypeScript normalizes to `(Base & X) | (Base & Y)` — the discriminant of each arm survives the extension, so the extended union still disambiguates by the same `_tag`.
- Extension is partial by schema kind, not universal: it composes a struct with a struct, refinement, index signature, suspend-of-struct, or union-of-supported, and rejects unsupported pairings at construction — a transformation arm extends only when the from/to sides have no overlapping fields with the target, so a codec-bearing variant constrains what a shared-field factoring can fold in.
- The union absorbs a new arm by re-declaration, not mutation: a member added to the family is one new `TaggedClass` appended to `Schema.Union`, `Schema.Schema.Type<typeof Union>` widening to include it with zero edit to existing extractors; three `TaggedClass` arms repeating the same field block is the collapse trigger — the repeated columns factor into a base struct extended across the variant union by one `extend`, the shared field landing once and a new common field one base edit reaching every arm.

[ONE_OWNER_MANY_PROJECTIONS]:
- The variant family carries no hand-written secondary surface: `Schema.equivalence(union)` yields `Equivalence<Type>`, `Arbitrary.make(union)` yields `FastCheck.Arbitrary<Type>`, `Pretty.make(union)` yields `(a: Type) => string`, and `JSONSchema.make(union)` yields a `JsonSchema7Root` — every projection is read off the one `Schema.Union` of `TaggedClass`, never authored per member.
- Each derivation dispatches on `_tag` structurally off the same member array the decoder reads: the union AST stores its arms as `Union.types`, and the compiler producing an `Equivalence`, `Arbitrary`, or JSON shape walks that array — one source of truth for decode, equality, generation, and rendering.
- `Schema.typeSchema(union)` strips the encoding to a `SchemaClass<Type>` and `Schema.encodedSchema(union)` strips to `SchemaClass<Encoded>`; both preserve the union shape, so the wire projection of a tagged family is itself a tagged union, never a flattened record — the discriminant survives every projection.
- A standalone `Equivalence.struct({...})` or a hand-written `(a, b) => a._tag === b._tag && ...` beside the union is the defect this deletes: the equality already exists as `Schema.equivalence(union)`, dispatching `_tag` first then field-comparing the matched member.

[DERIVED_EQUALITY_DISPATCH]:
- `Schema.equivalence(union)` produces a `_tag`-keyed dispatch: unequal tags short-circuit to `false`, equal tags fold the matched member's field equivalences — the per-member field comparison is itself derived from each `TaggedClass`'s field schemas, so a new field on one arm widens that arm's equivalence with zero edit to the others.
- `Data.taggedEnum` constructors carry structural `Equal`/`Hash` natively — two `Tree.Leaf({ value: 1 })` values are `Equal.equals`-equal without any derived `Equivalence` — so the decode-free owner needs no `Schema.equivalence` call; the boundary-carrying `Schema.Union` owner derives one only when value equality must travel as a reusable `Equivalence<A>` (a `HashSet` element comparator, a dedup key).
- `Equivalence.mapInput(eq, f)` lifts a member equivalence to a wider carrier by projecting the compared field, and `Equivalence.combineAll(iterable)` intersects several into a conjunction — the union's derived equivalence composes with these, so a coarser "same tag, ignore one field" equality is `Equivalence.mapInput(derived, stripVolatile)`, never a parallel comparator.
- `Order` over a variant family is not auto-derived (no `Schema.order`); the variant ordering is authored as `Order.mapInput` over the `_tag` rank plus a per-member tiebreak, `Order.combineAll` folding the chain — the rank table is a `satisfies` row read by indexed access, the only hand-authored piece, and adding a tag adds one row.

[ARBITRARY_OVER_UNION]:
- `Arbitrary.make(union)` builds a generator that picks a member arm then generates that member's fields; the choice is uniform over `union.members`, so property tests over the whole variant family are one generator, never a `fc.oneof` enumerated per tag.
- `Arbitrary.makeLazy(union)` returns a `LazyArbitrary<A>` — a thunk `(fc) => Arbitrary<A>` — which is the recursion-safe form: a self-referential variant decoded through `Schema.suspend` derives its generator through `makeLazy`, whose `ArbitraryGenerationContext` carries `maxDepth` so the recursive arm terminates instead of diverging.
- A field annotated `Schema.Int.pipe(Schema.between(0, 9))` narrows that member's generated values automatically; the arbitrary reads the refinement annotations off the field schema, so the generation constraints ride the same owner as the decode constraints — never restated in the test.

```typescript
import { Arbitrary, FastCheck, Schema } from 'effect'

class Open extends Schema.TaggedClass<Open>()('Open', { at: Schema.Int.pipe(Schema.positive()) }) {}
class Retry extends Schema.TaggedClass<Retry>()('Retry', { count: Schema.Int.pipe(Schema.between(1, 9)) }) {}
class Closed extends Schema.TaggedClass<Closed>()('Closed', { reason: Schema.NonEmptyString }) {}
const Signal = Schema.Union(Open, Retry, Closed)
const arbitrary = Arbitrary.make(Signal)
const isSignal = Schema.is(Signal)
FastCheck.assert(FastCheck.property(arbitrary, (s) => isSignal(s) && (s._tag !== 'Retry' || s.count >= 1)))
```

[JSONSCHEMA_DISCRIMINATED_SHAPE]:
- `JSONSchema.make(union)` emits an `anyOf` whose entries are the per-member object schemas, each carrying its `_tag` as a `const`-valued property — the discriminant becomes a JSON `const`, so the emitted contract is a discriminated union a consumer validates by branching on `_tag`, structurally mirroring the runtime decoder.
- A member's `Schema.annotations({ identifier: "<name>" })` promotes that arm into a `$defs` entry referenced by `$ref`, so a recursive variant emits a finite cyclic JSON Schema (the suspended arm is a `$ref` back to its own `$defs` slot) rather than an infinite inline expansion — the `identifier` annotation is the cut point.
- `JSONSchema.fromAST(union.ast, { definitions })` is the lower entry when the `$defs` map must be threaded across several unions sharing arms; `JSONSchema.make` is the closed form that allocates its own definitions — the shared-definition case is the only reason to drop to `fromAST`.

[CATAMORPHISM_AS_CODEC]:
- `Schema.transformOrFail(from, to, { decode, encode, strict })` folds one variant family into another at the seam: `decode` receives the source `Type` and returns `Effect<Encoded<To>, ParseIssue, RD>`, so a normalizing fold (collapsing two legacy tags into one canonical arm) is a transform whose `decode` is a `$match` over the source union — the catamorphism rides the codec, evaluated once at admission, never on every interior read.
- The fold's failure rail is `ParseResult.ParseIssue`, so an arm that cannot map (a removed legacy tag) returns a `ParseResult.Type` issue and the whole decode fails with a located error — the partial fold is total at the type level because `decode` must produce `Encoded<To>` for the matched arm or fail in the rail.
- A pure structural fold uses `Schema.transform` (synchronous `decode`/`encode`, no `Effect`); the `transformOrFail` form is reserved for a fold whose arm dispatch can itself fail — the carrier selects which, never a boolean knob.

[TYPECLASS_COPRODUCT_IDENTITY]:
- A two-arm choice variant is the `Coproduct` of its arms: the `@effect/typeclass` `SemiCoproduct<F>` interface is exactly `coproduct(self, that): Kind<F, ..., A | B>` plus `coproductMany`, and `Coproduct<F>` extends it with `zero` and `coproductAll` — so the binary-choice variant inherits first-success combination from its instance without a hand-written `||`/`??` chain.
- `Option` ships full `Coproduct` (it has an empty, `None`, as `zero`), but `Either` ships only `SemiCoproduct` — `Either` carries no empty arm, so it has `coproduct`/`coproductMany` (first-`Right` wins, errors accumulate in the `E` slot via `E1 | E2`) but no `zero`, and `coproductAll` over an `Either` collection is not in its surface.
- The collapse trigger: three sibling functions combining the same choice variant (a first-non-empty, an all-or-nothing, an empty default) is the `SemiCoproduct`/`Coproduct` surface re-derived by hand — route them through the data module's instance, and the laws (associativity of `coproduct`, `zero` as two-sided identity where the instance carries one) hold by construction.

[CATAMORPHISM]:
- A fold over a recursive variant is a total `$match` (or `Match.tagsExhaustive`) closure that recurses into child fields and combines results: the dispatch surface is the algebra carrier, totality guaranteeing every constructor has an arm, so adding a constructor breaks every fold loudly — `$match` and `Match.tagsExhaustive` gain a required-but-missing key and the next variant lands as one new arm the checker demands, never a silent skip.

```typescript
import { Data } from 'effect'

type Tree<A> = Data.TaggedEnum<{
    Leaf: { readonly value: A }
    Branch: { readonly left: Tree<A>; readonly right: Tree<A> }
}>
interface TreeDef extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: Tree<this['A']>
}
const Tree = Data.taggedEnum<TreeDef>()
const sample = Tree.Branch({ left: Tree.Leaf({ value: 2 }), right: Tree.Leaf({ value: 3 }) })
const sum = (node: Tree<number>): number =>
    Tree.$match(node, {
        Leaf: ({ value }) => value,
        Branch: ({ left, right }) => sum(left) + sum(right),
    })
const total = sum(sample)
```

- Generic `$match` in data-first form `Tree.$match(node, cases)` infers every slot from `node`, so no type arguments are spelled; the data-last form `Tree.$match(cases)` cannot infer the slots and forces all five type arguments `<A, B, C, D, Cases>`, since partial type-argument lists are illegal — data-first is the only inference-clean recursion spelling.
- A self-recursive fold needs an explicit return annotation on the recursive function: referencing `sum` inside its own body without one raises `TS7023` (implicit `any` return), and the annotation is the fixed-point declaration inference cannot derive.
- `$match` returns `Unify<ReturnType<...>>`, so a fold instantiated at a free type parameter `B` yields `Unify<B>`, not assignable to `B` without a cast — fold at the concrete result type (`Tree<number> -> number`, where `Unify<number>` collapses to `number`) or push the seed and combine into a `Monoid` whose carrier is already concrete.
- `Foldable.reduce` collapses a foldable container of variant values to a seed `(b: B, a: A) => B`; a bespoke recursive variant carries no `Foldable` instance, so its catamorphism is the total `$match` recursion into child fields, never `reduce`.

[ENDOFUNCTOR_CLOSURE]:
- A variant-preserving map is a `$match` whose every arm returns a constructor of the *same* owner — `Tree.$match(t, { Leaf: ({ value }) => Tree.Leaf({ value: f(value) }), ... })` — so the dispatch surface is the functor's `map`, total dispatch guaranteeing every constructor is rewritten and the family closed over itself.
- The arm-return type is `Unify<ReturnType<Cases[A["_tag"]]>>`: `$match` unions the per-arm return types then collapses them through `Unify`, so heterogeneous arms each yielding a *different* family member fuse to the bare family union, not an enumerated `Leaf | Branch` the caller re-narrows — the map's result type is the owner's value type with zero restatement.
- `Unify` collapses by reading the `typeSymbol`/`unifySymbol` discriminant carriers off each arm's return: arms returning distinct `_tag` members merge structurally, so an arm returning `Tree.Leaf(...)` and an arm returning `Tree.Branch(...)` unify to `Tree<B>` rather than the looser cross-product — the closure type is exact, never widened to the supertype.
- A rewrite that *prunes* a tag (an arm returning a different family's member, or `Option`) is not an endofunctor: the arm-return union no longer closes over the source family, and `Unify` yields the foreign carrier — route a structure-changing fold through `Schema.transform`'s `decode`, reserve the `$match`-into-own-constructors spelling for the family-preserving map.

[MONOMORPHIC_RECURSIVE_REWRITE]:
- A recursive rewrite over a *non-generic* `Data.taggedEnum` closes with zero ceremony beyond the function's own `TS7023` return annotation: the recursive arm calls the map on the child field and feeds the result straight into the parent constructor, `Json.$match(j, { JArr: ({ items }) => Json.JArr({ items: items.map(negate) }), ... })`, because every constructor's slot is already concrete and `Unify` reconciles each arm against the one fixed `Json` — the slot pin the generic case demands never arises.

```typescript
import { Data } from 'effect'

type Node = Data.TaggedEnum<{
    Empty: {}
    Value: { readonly weight: number }
    Pair: { readonly head: Node; readonly tail: Node }
}>
const Node = Data.taggedEnum<Node>()
const scale = (factor: number) => {
    const go = (n: Node): Node =>
        Node.$match(n, {
            Empty: () => Node.Empty(),
            Value: ({ weight }) => Node.Value({ weight: weight * factor }),
            Pair: ({ head, tail }) => Node.Pair({ head: go(head), tail: go(tail) }),
        })
    return go
}
const doubled = scale(2)(Node.Pair({ head: Node.Value({ weight: 3 }), tail: Node.Empty() }))
```

- `scale` curries the factor outside `go` so the closed-over constant is captured once, the recursive `go` carrying only the structural recursion — the rewrite policy enters as a closure value, never a per-call parameter threaded through every recursive frame.

[GENERIC_SLOT_INFERENCE_TRAP]:
- A recursive rewrite over a *generic* `Data.taggedEnum` (`WithGenerics<1>`) does **not** close by inference: each generic constructor re-infers its slot per call as `<A>(args) => Value<Kind<Z, A>, Tag>`, so a recursive arm feeding a mapped child into `Tree.Branch({ left: go(left), ... })` lets `Branch` re-infer its slot to `unknown`, and `Unify` fails to reconcile the resulting `Tree<unknown>` against the declared `Tree<B>` — `Type 'unknown' is not assignable to type 'B'` at the `$match` site.
- The slot widens to `unknown` because the constructor's *argument* type drives inference, and the recursive child sits inside a field whose contribution to the slot is the unconstrained intersection default — the constructor cannot back-propagate the function's declared `B` return into its own slot binding.
- An arm-return annotation alone (`Branch: ({ left, right }): Tree<B> => Tree.Branch({ ... })`) does **not** fix it: the annotation constrains the arm's output but the inner `Tree.Branch({...})` still infers its slot to `unknown` from the child before the annotation can contravariantly pin it, so the error survives unchanged at the constructor call.
- The minimal fix is the explicit constructor slot argument `Tree.Branch<B>({ ... })`: passing the type argument pins the generic constructor's slot to `B`, the recursive child is then checked against `Tree<B>`, and `Unify` closes the arm union to `Tree<B>` — the arm-return annotation becomes redundant once the constructor is pinned.

```typescript
import { Data } from 'effect'

type Tree<A> = Data.TaggedEnum<{
    Leaf: { readonly value: A }
    Branch: { readonly left: Tree<A>; readonly right: Tree<A> }
}>
interface TreeDef extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: Tree<this['A']>
}
const Tree = Data.taggedEnum<TreeDef>()
const mapTree = <A, B>(f: (a: A) => B) => {
    const go = (t: Tree<A>): Tree<B> =>
        Tree.$match(t, {
            Leaf: ({ value }) => Tree.Leaf<B>({ value: f(value) }),
            Branch: ({ left, right }) => Tree.Branch<B>({ left: go(left), right: go(right) }),
        })
    return go
}
const stringified = mapTree((n: number) => `${n}`)(Tree.Leaf({ value: 1 }))
```

- Every constructor in a generic rewrite carries its slot argument — `Tree.Leaf<B>` and `Tree.Branch<B>` both — even the non-recursive `Leaf` arm, because each constructor re-infers independently and a single unpinned arm re-opens the `Unify` reconciliation against `unknown`.
- A whole-result `Tree.$match(t, { ... }) as Tree<B>` cast also compiles but erases the per-arm slot proof: the cast asserts the closure without the checker verifying each arm reconstructs `Tree<B>` — the per-constructor `<B>` pin is the spelling that keeps the rewrite checked, the cast the spelling that suppresses it.
- The law is direction-agnostic: every generic-owner constructor inside a recursive function (map, unfold, or any traversal feeding a recursive child into a parent constructor) carries its explicit slot argument, because the constructor's slot is driven by its argument and a recursive child always contributes `unknown` — the surrounding annotation never back-propagates into the constructor's own inference.

[MULTI_SLOT_BIFUNCTOR]:
- A bifunctor's value type at concrete arguments is `Kind<ResultDef, E, A>`; the four-slot signature is fixed even for a two-generic owner, the unused `C`/`D` resting at their `unknown` default, so a two-slot rewrite reads exactly `this["A"]`/`this["B"]` and the constructors infer the two slots independently.
- Each arm passes the *full* slot list `Result.Failure<E2, A2>` though the `Failure` arm touches only the `error` slot: a constructor's explicit type-argument list is all-or-nothing, so pinning one slot forces spelling the sibling slot the arm does not consume.

```typescript
import { Data } from 'effect'

type Result<E, A> = Data.TaggedEnum<{
    Failure: { readonly error: E }
    Success: { readonly value: A }
}>
interface ResultDef extends Data.TaggedEnum.WithGenerics<2> {
    readonly taggedEnum: Result<this['A'], this['B']>
}
const Result = Data.taggedEnum<ResultDef>()
const bimap = <E, A, E2, A2>(fe: (e: E) => E2, fa: (a: A) => A2) => (r: Result<E, A>): Result<E2, A2> =>
    Result.$match(r, {
        Failure: ({ error }) => Result.Failure<E2, A2>({ error: fe(error) }),
        Success: ({ value }) => Result.Success<E2, A2>({ value: fa(value) }),
    })
const remapped = bimap((e: string) => e.length, (n: number) => n > 0)(Result.Success({ value: 1 }))
```

- `bimap` curries both maps ahead of the value so the two policies enter as composition values, the closure rewriting both slots in one total dispatch — three sibling `mapError`/`mapValue`/`bimap` functions over the same owner collapse to this one slot-polymorphic arm set, the single-slot maps recovered by passing `identity` for the untouched function.

[ANAMORPHISM_UNFOLD]:
- The construction dual is an unfold driven by a coalgebra `(s: S) => Seed<S, A>` whose `Seed` is a *single-layer* shape — recursive positions are the seed type `S`, not the variant — so the coalgebra describes one node's expansion and the recursive `go` re-seeds each child, building the family bottom-up from a generating value rather than rewriting an existing one.
- The seed carrier is a flat `_tag`-discriminated shape, not the variant itself: a coalgebra returning `Tree<A>` would force the caller to already hold the structure it is meant to generate, so the `Seed<S, A>` layer carries `S` in the recursive slots and the unfold replaces each `S` with a recursive call — the one-layer functor is the anamorphism's algebra carrier.
- The `Seed` discriminant is read by a structural `step._tag === 'Leaf'` narrow, not `Tree.$match`: the seed is not a `Data.taggedEnum` value (it has no constructors), so its dispatch is the bare literal-tag check the closed `Seed` union admits — the coalgebra's branch is a guard, the variant's branch is `$match`, the two carriers never conflated.
- The generic-owner slot trap recurs in the *construction* direction identically: a fully typed `Seed<S, A>` coalgebra does **not** pin the `Tree.Branch({...})` slot — the constructor still infers `unknown` from the recursive `go(step.left)` child — so every constructor in the unfold carries its explicit `Tree.Leaf<A>` / `Tree.Branch<A>` slot argument, the same mandatory pin the map direction needs.

```typescript
import { Data } from 'effect'

type Tree<A> = Data.TaggedEnum<{
    Leaf: { readonly value: A }
    Branch: { readonly left: Tree<A>; readonly right: Tree<A> }
}>
interface TreeDef extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: Tree<this['A']>
}
const Tree = Data.taggedEnum<TreeDef>()
type Seed<S, A> =
    | { readonly _tag: 'Leaf'; readonly value: A }
    | { readonly _tag: 'Branch'; readonly left: S; readonly right: S }
const unfold = <S, A>(coalg: (s: S) => Seed<S, A>) => {
    const go = (s: S): Tree<A> => {
        const step = coalg(s)
        return step._tag === 'Leaf'
            ? Tree.Leaf<A>({ value: step.value })
            : Tree.Branch<A>({ left: go(step.left), right: go(step.right) })
    }
    return go
}
const built = unfold<number, number>((n) => (n <= 0 ? { _tag: 'Leaf', value: n } : { _tag: 'Branch', left: n - 1, right: n - 2 }))(3)
```

[HYLOMORPHISM_FUSION]:
- An unfold immediately consumed by a fold fuses to a hylomorphism: the coalgebra `(s: S) => Seed<S, A>` and the algebra `(node, kids: ReadonlyArray<R>) => R` compose into one recursion that never materializes the intermediate `Tree<A>` — the seed expands one layer, recurses into the children to `R`, and the algebra folds the layer, so the structure is virtual and the generic-owner slot trap never arises (no `Tree.Branch` is ever constructed).
- The algebra receives the recursed children as a `ReadonlyArray<R>` keyed positionally to the seed's recursive slots, so a `Branch` node folds `kids[0]` and `kids[1]` — the same seed layer drives both the expansion (which slots recurse) and the contraction (which results combine), one shape declared once for both directions.
- The fused form sidesteps the `Unify<R>`-at-free-`R` wall: a direct `$match`-into-recursion fold at a free result type `R` yields `Unify<R>` the checker cannot assign back to `R`, but the hylomorphism's algebra returns `R` from a plain function call (not a `$match` whose return is `Unify`-wrapped), so the generic result type closes without a cast — the fusion is both a performance collapse and the type-level escape from the `Unify` reconciliation.
- The collapse trigger: an `unfold(seed)` whose result is the sole argument to a `fold`, with the intermediate variant never inspected, is two passes and one allocation the hylomorphism deletes — the seed-to-result pipeline is one owner, the materialized tree the deleted artifact.

```typescript
import { Data } from 'effect'

type Tree<A> = Data.TaggedEnum<{
    Leaf: { readonly value: A }
    Branch: { readonly left: Tree<A>; readonly right: Tree<A> }
}>
interface TreeDef extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: Tree<this['A']>
}
const Tree = Data.taggedEnum<TreeDef>()
type Seed<S, A> =
    | { readonly _tag: 'Leaf'; readonly value: A }
    | { readonly _tag: 'Branch'; readonly left: S; readonly right: S }
const hylo = <S, A, R>(coalg: (s: S) => Seed<S, A>, alg: (node: Seed<S, A>, kids: ReadonlyArray<R>) => R) => {
    const go = (s: S): R => {
        const node = coalg(s)
        return node._tag === 'Leaf' ? alg(node, []) : alg(node, [go(node.left), go(node.right)])
    }
    return go
}
const counted = hylo<number, number, number>(
    (n) => (n <= 0 ? { _tag: 'Leaf', value: n } : { _tag: 'Branch', left: n - 1, right: n - 2 }),
    (node, kids) => (node._tag === 'Leaf' ? 1 : kids[0]! + kids[1]!),
)(3)
```

[ARM_OWNS_RESULT_PAIR]:
- `Schema.TaggedRequest<Self>(id?)(tag, { payload, success, failure }, annotations?)` is the variant arm whose `_tag` indexes not just fields but a `success`/`failure` schema pair: each arm is a `TaggedRequestClass` exposing `.success` and `.failure` as live member schemas, so the discriminant of the family correlates a *result type* to each constructor, not only a payload shape.
- `TaggedRequestClass` extends `Class<Self, Payload, ..., TaggedRequest<Tag, Self, ...>, {}>` whose proto parameter is `TaggedRequest<Tag, ...>`, itself extending `Request.Request<SuccessType, FailureType>` and `SerializableWithResult<...>` — so one `class X extends Schema.TaggedRequest<X>(...) {}` is at once the tagged value, its decode codec, an `Equal`/`Hash` instance, a runnable request carrying its own outcome types, and a serialization owner.
- `payload` is the constructor field set (the `_tag` woven in via `tag<Tag>` default), distinct from `success`/`failure`: the wire-decoded value is `Struct.Type<{ _tag: tag<Tag> } & Payload>`, while `success`/`failure` never enter the value — they are the schemas of what running the arm *returns*, recovered by trait accessor, never by reading a field.
- Omitting the `Self` generic resolves to `MissingSelfGeneric<"TaggedRequest", '"Tag", SuccessSchema, FailureSchema, '>` — the same self-type gate the base `TaggedClass<Self>` carries, so a request arm is a first-class nominal class, joinable to a `Schema.Union` exactly like a plain `TaggedClass` arm.

[FAMILY_DISPATCH_OVER_REQUESTS]:
- A `Schema.Union(QueryOne, QueryMany, Mutate)` of `TaggedRequest` arms decodes by the same `_tag` search tree a `TaggedClass` union uses — the result-pair carriage is invisible to the decoder, so `Match.tagsExhaustive` / `$is` / `Schema.is` close over the request family identically, and each arm's handler recovers its correlated outcome by `Schema.successSchema(arm)` rather than a parallel result-type lookup.
- `Match.tag("QueryOne", req => ...)` narrows `req` to the arm class, so `req.success` and `req.failure` are that arm's exact result schemas inside the handler — the dispatch surface and the result-type correlation are one move, the handler's return obligation pinned by the matched arm's own `Schema.Type<typeof req.success>`.
- The handler returns an `Effect` whose success channel is `Schema.Schema.Type<arm["success"]>` and failure channel is `Schema.Schema.Type<arm["failure"]>`: the per-arm result pair is the handler's typed contract, so a request executor is a `Match.exhaustive` whose every arm's `Effect<A, E>` is checked against the arm-declared pair, a wrong outcome type a compile error at that arm.
- `failure: Schema.Never` declares an infallible arm — the arm carries no failure schema beyond the empty type, so `Schema.failureSchema(arm)` is `Schema.Never` and the arm's `Effect` failure channel is `never`, the absence of a fault encoded in the family rather than a nullable error field.

```typescript
import { Effect, Match, Schema } from 'effect'

class FetchOne extends Schema.TaggedRequest<FetchOne>()('FetchOne', {
    payload: { id: Schema.NonEmptyString },
    success: Schema.Struct({ value: Schema.Int }),
    failure: Schema.TaggedStruct('Missing', { id: Schema.NonEmptyString }),
}) {}
class FetchAll extends Schema.TaggedRequest<FetchAll>()('FetchAll', {
    payload: { limit: Schema.Int.pipe(Schema.positive()) },
    success: Schema.Array(Schema.Int),
    failure: Schema.Never,
}) {}
const Query = Schema.Union(FetchOne, FetchAll)
const run = Match.type<Schema.Schema.Type<typeof Query>>().pipe(
    Match.tag('FetchOne', (q) => Effect.succeed({ value: q.id.length })),
    Match.tag('FetchAll', (q) => Effect.succeed(Array.from({ length: q.limit }, (_, i) => i))),
    Match.exhaustive,
)
```

- `FetchOne`'s arm returns `Effect<{ value: number }, never>` checked against its declared `success: Schema.Struct({ value: Schema.Int })`; `FetchAll`'s arm returns `Effect<ReadonlyArray<number>>` against `success: Schema.Array(Schema.Int)` — each arm's outcome contract rides its own declaration, and a fourth request lands as one `TaggedRequest` class plus one `Match.tag` arm, the `exhaustive` breaking until the arm is added.

[OUTCOME_IS_ITSELF_A_VARIANT]:
- `Schema.exitSchema(arm)` lifts a request arm's result pair to `Schema<Exit<SA, FA>, ExitEncoded<SI, FI, unknown>>` — the runnable outcome as a codec, where `ExitEncoded<A, E, D>` is itself a `_tag`-discriminated union `{ _tag: "Failure"; cause: CauseEncoded } | { _tag: "Success"; value: A }`, so the family's *outcome* is a second tagged variant nested inside the request variant, decoded by the same discriminant machinery.
- `Schema.serializeExit(arm, exit)` and `Schema.deserializeExit(arm, wire)` are the boundary pair for a request's result: `serializeExit` reads the arm's `WithResult` trait, encodes a live `Exit<SA, FA>` to `ExitEncoded` (the success value through `success`, the failure cause through `failure`), `deserializeExit` reverses — so a remote request family round-trips its outcomes through one accessor per arm, never a hand-written exit serializer.
- `Schema.serializeSuccess`/`deserializeSuccess` and `Schema.serializeFailure`/`deserializeFailure` are the half-channel pair when only one side crosses: each reads the arm's `success`/`failure` schema in isolation, so a fire-and-forget acknowledgement serializes the success branch alone without materializing the full `Exit` codec.
- `Schema.serialize(arm)` / `Schema.deserialize(arm, wire)` operate on the *payload* (the `Serializable` trait, the `_tag` + fields), orthogonal to the result channel (the `WithResult` trait): the request value and its outcome are two independently-serializable surfaces on the same arm, the payload crossing on dispatch, the exit crossing on return.

[TRAIT_RECOVERY_NOT_FIELD_READ]:
- The result-pair *type* is recovered through `WithResult.Success<typeof arm>` / `WithResult.Failure<typeof arm>` (and the encoded duals `SuccessEncoded`/`FailureEncoded`), `infer`-projections over the `WithResult` trait — a function "what does running this arm yield" types its return as `WithResult.Success<typeof FetchOne>` rather than `Schema.Type<typeof arm.success>` reaching through the live member or re-listing `{ value: number }`.
- `Serializable.Type<typeof arm>` / `Serializable.Encoded<typeof arm>` recover the payload's decoded and wire shapes from the `Serializable` trait, so the four corners of a request arm — payload-decoded, payload-wire, success, failure — are four trait extractors over one class, the request's full contract derived from its declaration with zero restated mirror type.
- `Schema.successSchema(arm)` / `Schema.failureSchema(arm)` are the value-level accessors yielding the live `Schema<SA, SI, R>` / `Schema<FA, FI, R>`, so a generic request handler that must decode an arm's wire outcome reads `Schema.successSchema(arm)` and feeds it to `Schema.decodeUnknown` — the codec recovered from the family member, never re-declared at the handler.
- `asSerializableWithResult` / `asWithResult` / `asSerializable` are the trait up-casts collapsing an arm to its bare trait view for a polymorphic processor over a heterogeneous request batch: a queue draining `TaggedRequest.All` values dispatches on the trait, each element's own `success`/`failure` schemas threaded through, no per-arm branch.

[FAILURE_ARM_AS_YIELDABLE]:
- `Schema.TaggedError<Self>(id?)(tag, fields)` is the failure-channel sibling: a `TaggedErrorClass` extends `Class<...>` and `cause_.YieldableError`, so the arm is at once a tagged union member, a codec, and a directly `yield*`-able error in `Effect.gen` — a request arm's `failure: SomeTaggedError` makes the fault both serializable through the result channel and throwable into the typed `E` channel without an `Effect.fail` wrapper.
- A `Schema.Union` of `TaggedError` arms is the closed fault family for a domain: each arm decodes by `_tag`, carries its own message getter, and joins the request family's `failure` slot — so the `E` alphabet a request can produce is itself a discriminated variant, `Match.tags` over the caught error narrowing to the exact fault arm.
- `TaggedError` and `TaggedRequest` share the `tag<Tag>` default and the self-generic gate, so a fault arm and a request arm are the same shape with different trait carriage — the failure family and the request family are one vocabulary, the `_tag` discriminant spanning both, never two parallel tag namespaces.
