# Union Member Projection Algebra

[PROJECTION_ALGEBRA_IS_CLOSED_OVER_THE_OWNER]:
- The projections form one closed algebra over a single owner: every operation — carve a literal sub-vocabulary, carve a sub-union of arms, filter a heterogeneous stream to one arm, recover one arm's codec — takes the owner (or a prior projection) as input and yields a value the *same* algebra accepts as input again, so a sub-union filtered from a partition feeds a `Match.exhaustive`, a picked literal subset feeds an arm `Extract`, and an arm codec feeds a selective decode — the projections compose, and no step produces a shape outside the family that would force a fresh declaration.
- The algebra is closed only at the *value* and *typed-schema* level, never at the erased-AST level: a projection that drops to `union.ast.types` exits the algebra (the entry is a stripped node carrying no codec and no static witness), so the composability that lets one projection feed the next holds for `members[i]` and the `Extract`/`pickLiteral`/`$is` family but breaks the instant a step reads `ast.types` — the AST surface is the algebra's boundary, not a member of it, and re-entering it requires re-declaring the class the typed projection already carried.
- Closure is what makes the next case land as one edit: because each projection's output re-enters the algebra, a new arm appended to the owner widens every downstream projection — the partition's residual sub-union, the `pickLiteral` subset's complement, the `filterMap`'s `$match` totality — at compile time with zero projection re-authored, the multiplicative collapse the per-site `x._tag === "<lit>"` partition forfeits.

[REFINEMENT_ARITY_GOVERNS_HIGHER_ORDER_FIT]:
- Whether `$is(tag)` composes into a higher-order filtering position is decided by the *arity of the refinement parameter each surface accepts*, not by the guard itself: `Array.filter`'s refinement overload is `(a: NoInfer<A>, i: number) => a is B`, `Stream.filter`'s is a single-argument `Refinement<NoInfer<A>, B>`, and the `@effect/typeclass` `Filterable.filter` lifts a single-argument `(a: A) => a is B` — the monomorphic `$is(tag)` typed `(u: unknown) => u is Extract<A, { _tag: Tag }>` satisfies all three because a unary callback is assignable where a binary one is expected (the surplus `i` parameter is simply unread), so one derived guard partitions an array, a stream, and any `Filterable` carrier with zero adapter.
- `NoInfer<A>` on the input parameter is the load-bearing bit that keeps the partition exact: it blocks the refinement's input from *driving* the element-type inference, so `values.filter(Sig.$is("Open"))` over `ReadonlyArray<Sig>` resolves `A = Sig` from the array and `B = Extract<Sig, { _tag: "Open" }>` from the guard's `u is` clause — without `NoInfer` the `unknown` input of `$is` would widen `A` to `unknown` and the result element type would collapse, so the surface's `NoInfer` annotation is what lets a `unknown`-input guard refine a concretely-typed carrier.
- The guard's `unknown` input is a *feature* of the projection algebra, not a defect to narrow away: because `$is` accepts `unknown`, the one guard is simultaneously a decode-boundary admission test (`Schema.is`-style over a foreign value) and an interior partition predicate (over an already-`Sig` array), so re-typing it to `(s: Sig) => s is Open` to "tighten" it would forfeit the boundary modality the single signature already owns.

[FILTER_PRESERVES_PARTITION_GROUPBY_ERASES]:
- The partition surfaces split cleanly on whether they *preserve the arm refinement* through the projection: `Array.filter`/`Stream.filter` over a refinement yield `Array<B>`/`Stream<B>` (the exact arm), and `Array.partition`/`Stream.partition` over a refinement yield `[excluded: Array<Exclude<A, B>>, satisfying: Array<B>]` — both halves typed, the satisfying side the named arm and the excluded side the *residual sub-union* — so one `partition($is("<lit>"))` splits a heterogeneous stream into the one arm and the rest-of-family with both projections exact.
- `Array.groupBy(values, v => v._tag)` is the trap that *looks* like the natural per-tag partition and silently erases it: its signature returns `Record<Record.ReadonlyRecord.NonLiteralKey<K>, NonEmptyArray<A>>`, so the literal `_tag` key is widened to its `NonLiteralKey` (bare `string`) and every bucket holds the **full union** `A`, not the per-tag `Extract` — the buckets exist at runtime but the type system gives back `NonEmptyArray<Sig>` per key, so reading `groups["Open"][0].at` is a compile error the `filter($is("Open"))` projection would have admitted.
- The law: a key-returning selector erases the discriminant to its widened key type and never threads the per-key arm narrowing, while a refinement-shaped predicate threads `Extract`/`Exclude` through the result — so partitioning a tagged stream into typed arms routes through `$is`-composed `filter`/`partition` (one call per arm, each arm exact) or `Array.filterMap` with a `$match` arm, never `groupBy` whose convenience costs the whole arm-type recovery.

```typescript
import { Array as Arr, Data, Option } from 'effect'

type Event = Data.TaggedEnum<{
    Open: { readonly at: number }
    Retry: { readonly count: number }
    Close: { readonly reason: string }
}>
const Event = Data.taggedEnum<Event>()
const stream: ReadonlyArray<Event> = [Event.Open({ at: 1 }), Event.Retry({ count: 2 }), Event.Close({ reason: 'x' })]
const [rest, opens] = Arr.partition(stream, Event.$is('Open'))
const ats: ReadonlyArray<number> = opens.map((o) => o.at)
const survivors: ReadonlyArray<Extract<Event, { _tag: 'Retry' | 'Close' }>> = rest
const counts = Arr.filterMap(stream, Event.$match({
    Open: () => Option.none<number>(),
    Retry: ({ count }) => Option.some(count),
    Close: () => Option.none<number>(),
}))
```

- `partition($is("Open"))` types `opens` as `ReadonlyArray<Open>` (`.at` reachable) and `rest` as `ReadonlyArray<Retry | Close>` — the excluded side is the residual sub-union, exact — so a downstream `Match.exhaustive` over `rest` already knows `Open` is gone; the loose `groupBy(stream, e => e._tag)` would hand back `Record<string, NonEmptyArray<Event>>`, losing both the arm refinement and the residual proof.

[FILTERMAP_IS_TOTAL_PROJECTION_PARTIAL_RESULT]:
- `Array.filterMap`/`Stream.filterMap`/`Filterable.filterMap` over `(a: A) => Option.Option<B>` is the projection that fuses *partition by tag* with *transform per arm* in one total dispatch: feed it a `$match` (or `Match.option`-finalized matcher) whose every arm returns `Option.some(projection)` for kept tags and `Option.none()` for dropped tags, and the result is `Array<B>` over the projected payload — the arm dispatch is total (every tag has an arm, the checker demands it) while the *result* is partial (the `None` arms drop), so the heterogeneous stream collapses to the homogeneous projection with totality preserved and no residual `unknown`.
- `Match.option` is the finalizer that makes a non-total matcher a `filterMap` callback directly: it returns `Option<Unify<A>>` with the unhandled residual collapsed to `None`, so `Match.type<Event>().pipe(Match.tag("Retry", r => r.count), Match.option)` is a ready `(e: Event) => Option<number>` whose unmatched arms (`Open`, `Close`) become the `None` `filterMap` drops — the partial projection authored as a partial matcher, the residual reified rather than discarded.
- The collapse: three sibling passes — a `filter($is("Retry"))` to keep, a `.map(r => r.count)` to project, and a second filter to drop a sub-case — are one `filterMap` over a `$match`/`Match.option`, because the kept-or-dropped decision and the projection share the one arm dispatch; the `Option` carrier is what fuses the keep predicate and the transform into a single pass, never two.

[MONOMORPHIC_FILTERS_GENERIC_NARROWS_DIRECTLY]:
- The monomorphic `$is(tag)` — `(u: unknown) => u is Extract<A, { _tag: Tag }>` — has *one* return refinement, so it slots into the higher-order `filter`/`partition`/`filterMap` callback position and the surface solves `B = Extract<A, { _tag: Tag }>` from that fixed `u is` clause; the generic-owner `TaggedEnum.GenericMatchers.$is` carries a *two-overload dual* the monomorphic form lacks — a first overload `<T extends Kind<Z, any, any, any, any>>(u: T) => u is T & { readonly _tag: Tag }` intersecting the caller's slot bindings with the tag, and a second `(u: unknown) => u is Extract<Kind<Z>, { _tag: Tag }>` collapsing the slots to `unknown` — and that very duality is what *breaks* it in a callback.
- The break is structural: a higher-order surface like `Array.filter` cannot solve the free `<T>` of the first overload from inside its own callback position (there is no caller value to bind `T` against), so it falls through to the second overload, the slots collapse to `unknown`, and `values.filter(Tree.$is("Leaf"))` over `ReadonlyArray<Tree<number>>` yields `ReadonlyArray<Extract<Tree<unknown>, { _tag: "Leaf" }>>` — `.value` on the result is `unknown`, not `number`. The generic guard *narrows* (it refines a known value at a direct `if (Tree.$is("Leaf")(node))` call where `T` solves to `Tree<number>`) but does not *filter* (it cannot thread the slot through a deferred callback).
- The boundary as one law: monomorphic guards filter and narrow both, generic guards narrow only when directly applied — so partitioning a *generic* family by arm pins a concrete instantiation first (`ReadonlyArray<Tree<number>>` filtered by a guard already specialized to `number`) or routes through `Array.filterMap` with a `$match` arm whose data-first `Tree.$match(node, cases)` infers the slot from the element, the `Option` projection recovering the per-arm payload at the concrete slot the callback never could.

```typescript
import { Array as Arr, Data, Option } from 'effect'

type Tree<A> = Data.TaggedEnum<{
    Leaf: { readonly value: A }
    Branch: { readonly left: Tree<A>; readonly right: Tree<A> }
}>
interface TreeDef extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: Tree<this['A']>
}
const Tree = Data.taggedEnum<TreeDef>()
const forest: ReadonlyArray<Tree<number>> = [Tree.Leaf({ value: 1 }), Tree.Branch({ left: Tree.Leaf({ value: 2 }), right: Tree.Leaf({ value: 3 }) })]
const leafValues = Arr.filterMap(forest, (t) =>
    Tree.$match(t, { Leaf: ({ value }) => Option.some(value), Branch: () => Option.none<number>() }))
```

- `forest.filter(Tree.$is("Leaf"))` would type its result at `Tree<unknown>` arms — `.value` lost to `unknown` — because the higher-order position cannot solve the generic `$is`'s `<T>`; the `filterMap` with data-first `Tree.$match(t, cases)` solves the slot from `t: Tree<number>`, so `leafValues` is `ReadonlyArray<number>` exact — the generic family keeps its slot only when the projection dispatches through `$match`, never through a deferred guard.

[LITERAL_SUBSET_DRIVES_ARM_SUBSET]:
- A literal sub-vocabulary projection and a union arm projection are *the same projection one level apart*: `pickLiteral(...subset)` carves a sub-union of the discriminant literals, and `Extract<Schema.Schema.Type<typeof Union>, { _tag: Sub }>` carves the corresponding sub-union of arms — so the literal subset *is* the type-level selector that partitions the family, and a guard built from the picked subset (`Schema.is(Elevated)` over the `_tag`, a `Match.tag(...subset, f)`) filters exactly the arms whose discriminant the subset admits, the two projections moving in lock-step off one parent.
- `pickLiteral`'s value is its *checked-against-parent* constraint, which the loose form forfeits: the signature `<A extends AST.LiteralValue, L extends NonEmptyReadonlyArray<A>>(...literals: L) => Literal<[...L]>` forces every picked literal to be an `A` of the parent union, so a tier-subset projection fails to type when a literal outside the parent is named — a `Schema.Literal('admin', 'owner')` declared beside the parent compiles identically yet carries *no* verified subset relationship, so renaming a parent tier silently desyncs the standalone copy while it breaks the `pickLiteral` projection loudly.
- The discriminant-remap and the arm-projection compose at the seam: `transformLiterals` decodes a wire code to a domain name (`Match.exhaustive` over the decoded names breaking until a new name is handled), and that decoded name is the exact `_tag` an arm-union projection keys on — so a wire-code payload, the domain-name vocabulary it projects to, and the arm family that vocabulary discriminates are one owner chain, the inverse encode map and the arm partition both derived from the same literal rows, never a second lookup beside a second guard.

```typescript
import { Match, Schema } from 'effect'

const Tier = Schema.Literal('guest', 'member', 'admin', 'owner')
const Elevated = Tier.pipe(Schema.pickLiteral('admin', 'owner'))
type Tier = Schema.Schema.Type<typeof Tier>
const route = Match.type<Tier>().pipe(
    Match.whenOr(...Elevated.literals, () => 'privileged' as const),
    Match.orElse(() => 'public' as const),
)
```

- `Match.whenOr(...Elevated.literals, ...)` spreads the *picked subset* directly as the match patterns — the `'admin' | 'owner'` literal projection and the dispatch arms it consumes are one declaration, so widening `Elevated` by one `pickLiteral` argument widens both the gate and the routed arm with zero second edit; spelling the patterns as a fresh `Match.whenOr('admin', 'owner', ...)` beside a hand-listed `Schema.Literal('admin', 'owner')` is the parallel-vocabulary restatement whose drift the compiler cannot catch.

[SELECTIVE_ARM_CODEC_OVER_HETEROGENEOUS_PAYLOAD]:
- The per-arm codec projection enables a *selective decode* the union-wide codec cannot express: feeding `union.members[i]` to `Schema.decodeUnknown` admits exactly the one variant and rejects every sibling arm as a `ParseError`, so a boundary that must accept only a subset of a published family (one element of a heterogeneous batch, one variant out of a polymorphic envelope) decodes against the member schema rather than the union — the member projection is the *narrowing decoder*, the union the *admitting decoder*, selected by which of the one owner's projections the boundary feeds.
- The selective-codec projection survives the effectful stream surface intact: `Stream.filterMapEffect((a) => Option<Effect<B, E2, R2>>)` and `Stream.partitionEither` thread a per-arm `Schema.decodeUnknown(member)` through the stream, so a wire stream of mixed-tag elements partitions into per-arm decoded sub-streams where each arm's decode failure rides the stream's own `E` channel — the heterogeneous stream's projection is total over arms and fallible per element, the residual reified into the partition's `Left` rather than dropped.
- A union persisted as `Schema.Array(union)` recovers its per-element family through `Schema.getNumberIndexedAccess`, whose lift is `SchemaClass<A[number], I[number], R>` — the same tagged union with the discriminant intact — so element-by-element decode reads the lifted element schema and each element still carries the `_tag` the arm projection keys on; reaching past the array wrapper to re-declare the union is the restatement the indexed-access lift deletes.

[SLOT_PIN_IS_THE_ONE_GENERIC_PROJECTION_LAW]:
- One law governs every generic-owner projection regardless of direction: the slot must be concrete at the *site the projection reads it*, because the opaque `Kind<Z, A>` indirection is `infer`-inert and the checker recovers no slot a projection does not already hold pinned — so filtering, mapping, arm-value recovery, and selective decode each succeed only where a concrete value or an explicit type argument has fixed the slot, and each fails identically to `unknown` where it has not.
- The law unifies three failures the surfaces present as unrelated: a higher-order `filter($is(tag))` cannot pin the slot (no caller value in the callback to bind `<T>`), a recursive-constructor arm cannot pin it (the child contributes `unknown` before any annotation back-propagates), and an `infer`-extraction over `Tree<A>` cannot pin it (the indexed access will not invert) — all three are the one missing-concrete-binding fault, and all three resolve by the same move: route the projection through a site holding a concrete value, the data-first `$match(value, cases)` (slot from the value), the pinned constructor `Tree.Leaf<B>` (slot from the argument), or a pre-specialized carrier `ReadonlyArray<Tree<number>>`.
- The consequence for the projection algebra: a generic family stays *inside* the closed algebra only along the pinned-slot path — a deferred-callback projection drops it out (to `unknown` arms, no longer re-enterable as a typed family), while a `filterMap` whose `$match` dispatches on the concrete element keeps it in, so the choice between `filter` and `filterMap` over a generic union is not stylistic but the difference between exiting and remaining within the algebra that lets the next projection compose.
