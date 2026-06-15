# Chooser Totality As Corpus Invariant

[TOTALITY_IS_A_CARRIED_TYPE_NOT_A_TERMINAL_CHECK]:
- The chooser's totality is not asserted at the pipe's close — it is a type the matcher *carries through every arm*, the third slot of `Matcher<I, F, R, A, Pr, Ret>`, where `R` is the residual set of signatures not yet routed. Each routing combinator subtracts its handled cases from `R` by `ApplyFilters<I, AddWithout<F, handled>>`, so the residual shrinks arm by arm and the terminal `exhaustive` is typed `(self: Matcher<I, F, never, A, Pr, Ret>)` — it applies *only* when `R` has already been driven to `never`. Totality is therefore a running invariant the arms maintain, and the terminal is the proof obligation that the invariant reached its fixed point, never the place the check happens.
- The residual slot is the load-bearing reason a widened signature breaks the chooser at the *terminal* rather than at any arm: appending a signature member widens `I`, the existing arms subtract the same cases they always did, the residual `R` is no longer `never`, and `exhaustive` fails to satisfy its `R extends never` constraint with the new member named in the unassignable type. The break names the exact unrouted regime, because the residual is the literal `Exclude<I, handled>` the new member now inhabits — the diagnostic is the inventory of what the chooser forgot, not an opaque mismatch.
- The invariant is monotone in the same direction the lattice grows: subtracting a handled case can only shrink `R`, never grow it, so a chooser that reached `never` stays total under arm *reordering* and breaks only under *signature widening* or *arm deletion*. This monotonicity is what makes the chooser a fixed shape whose only mutation vector is the signature it routes — the decision law has exactly one growth axis, and it is the axis the totality slot watches.

```typescript
type Signature =
    | { readonly _tag: 'Nominal'; readonly wire: boolean }
    | { readonly _tag: 'Reference' }
    | { readonly _tag: 'Structural'; readonly cardinality: 'one' | 'many'; readonly wire: boolean }
// the residual is driven to `never` arm by arm; `exhaustive` applies only at the fixed point
const choose: (s: Signature) => string = Match.type<Signature>().pipe(
    Match.tag('Nominal', (s) => (s.wire ? 'Schema.brand' : 'Brand.refined')),
    Match.tag('Reference', () => 'as-const-table'),
    Match.tag('Structural', (s) => (s.cardinality === 'many' ? (s.wire ? 'Schema.Union' : 'Data.taggedEnum') : 'Schema.Class')),
    Match.exhaustive, // adding a fourth `_tag` member leaves R != never; this line fails naming the unrouted tag
)
```

[THE_INVARIANT_HAS_TWO_INDEPENDENT_ENFORCEMENT_GATES]:
- Totality is enforced by two gates that fire on disjoint failure shapes, and a complete chooser passes both: the *subtractive* gate is the `R extends never` constraint on `exhaustive`/`orElseAbsurd`, which catches an *under-handled* union (a member no arm subtracted); the *additive* gate is the case-record mapped type `{ [Tag in Types.Tags<D, R> & string]: (...) => Ret }` on `discriminatorsExhaustive`/`tagsExhaustive`, which makes every live discriminant a *required key* so a missing arm is a missing property at the record literal. The subtractive gate watches the residual, the additive gate watches the record — and they catch the same widening at two different sites.
- The additive gate carries its own excess-rejection conjunct `{ [Tag in Exclude<keyof P, Types.Tags<D, R>>]: never }`, so a *stale* arm — one routing a discriminant the signature no longer carries after a member is renamed or removed — resolves to `never` and fails the record. The two halves of the mapped type are the bidirectional totality: every live case is required (no gap) and every record key is live (no dead arm), so the case record is structurally pinned to the union's exact discriminant set with no slack in either direction.
- The gates are not interchangeable and choosing the gate is part of routing the chooser: `discriminatorsExhaustive`/`tagsExhaustive` fuse both gates into one record literal that needs no terminal, while `Match.tag`/`Match.when` arms drive the subtractive gate alone and *require* the `exhaustive` terminal to discharge it. A chooser built on `Match.tag` arms without the terminal compiles as a partial function whose residual is silently non-`never` — the subtractive gate is armed but never fired, which is the defect the record-form's no-terminal totality forecloses by construction.

```typescript
// both gates in one record: missing arm = missing required key, stale arm = `never`-typed excess key
const choose: (s: Signature) => string = Match.type<Signature>().pipe(
    Match.discriminatorsExhaustive('_tag')({
        Nominal: (s) => (s.wire ? 'Schema.brand' : 'Brand.refined'),
        Reference: () => 'as-const-table',
        Structural: (s) => (s.cardinality === 'one' ? 'Schema.Class' : 'Data.taggedEnum'),
    }), // no `Match.exhaustive` tail: the mapped type is total by construction
)
```

[THE_SILENT_BREACH_SURFACE_IS_THE_FALLBACK_TAIL]:
- The one move that defeats both gates silently is the fallback terminal: `orElse` is typed `<RA, Ret, F>(f) => (self: Matcher<I, R, RA, A, Pr, Ret>) => ...` — it accepts *any* residual `RA` with no `extends never` constraint and folds it into the return, so a chooser closed by `Match.orElse(() => default)` swallows whatever the union widening left unrouted and routes the new regime to the default arm with no diagnostic. The fallback is the exact inverse of the totality invariant: it discharges the residual by *absorbing* it rather than *exhausting* it, so the chooser type-checks forever while silently mis-routing every member added after the fallback was written.
- The silent breach is the chooser-level analogue of the lone silent shape axis: a widening that an `exhaustive` chooser surfaces as a loud terminal failure, an `orElse` chooser surfaces as nothing — the new regime resolves to the default tier at runtime and the misroute is discovered only when the wrongly-tiered owner corrupts a downstream surface. The presence of an `orElse`/`orElseAbsurd-replaced-by-orElse` tail on the corpus chooser is therefore itself the defect, detectable by reading the terminal alone: a total chooser ends in `exhaustive` or a `*Exhaustive` record, never a catch-all.
- The non-exhaustive `Match.discriminators(D)` (singular, no `Exhaustive`) is the same breach wearing the discriminator form: its case record keys are `?`-optional (`((_) => Ret) | undefined`), so an omitted arm is legal and the unhandled discriminant falls through to a downstream `orElse`. The record-vs-record distinction is the whole decision — `discriminators` is the partial form whose optional keys admit a gap, `discriminatorsExhaustive` is the total form whose required keys forbid one — and reaching for the partial form on the corpus chooser re-opens the silent breach the exhaustive form sealed.

```typescript
// reject: the fallback absorbs the residual with no `R extends never` gate — a fourth `_tag` routes here silently
const choosePartial: (s: Signature) => string = Match.type<Signature>().pipe(
    Match.tag('Nominal', (s) => (s.wire ? 'Schema.brand' : 'Brand.refined')),
    Match.tag('Reference', () => 'as-const-table'),
    Match.orElse(() => 'Schema.Class'), // every unrouted regime collapses to this tier with no diagnostic
)
```

[THE_INVARIANT_IS_CORPUS_WIDE_BECAUSE_THE_CHOOSER_IS_ONE_VALUE]:
- Totality is a property of the corpus, not of one routing site, because the residual fixed point is discharged at exactly one place — the single reusable chooser's terminal — and every routing site applies that already-proven function rather than re-running the dispatch. The proof that "every concept is routed" is therefore discharged once and inherited everywhere, so a routing site cannot be individually partial: it has no terminal of its own to leave open, it only consumes the one total function. The reject is inlining the dispatch per concept, which scatters the residual across as many terminals as there are sites and makes totality a per-site discipline no single read can confirm.
- A second chooser owner splits the invariant the way a parallel schema splits single-source: a subset of concepts routed through a sibling matcher means a regime declared on one chooser's signature is invisible to the other's residual, so the corpus has two totality fixed points that can disagree, and a concept whose axes only the sibling models is mis-routed with no cross-chooser diagnostic. The chooser is one value across the whole corpus for the same reason the signature is one owner — duplicating the decision surface duplicates the place totality must hold and guarantees the two drift.
- The corpus-wide invariant has a definite location it is discharged: the single `exhaustive` terminal (or single `*Exhaustive` record) on the one chooser value. Auditing corpus totality is therefore reading one terminal, not scanning every routing call — the invariant is checkable in one hop because it is carried by one shared owner, and the absence of a fallback tail on that one owner is the whole audit. A corpus with N choosers has N terminals to audit and N residuals that can independently lapse.

[WIDENING_DRIVES_THE_BREAK_THROUGH_THE_LIVE_DISCRIMINANT_READ]:
- The break is loud because the case record reads the discriminant union *live* through `Types.Tags<D, R> = R extends Record<D, infer X> ? X : never` — the required-key set is computed from the current union, not a frozen copy, so widening the signature widens `Types.Tags` and the mapped type gains a required key the existing record literal does not satisfy. The chooser does not need to be re-read against a separate enumeration of regimes; the enumeration *is* the union, and the record is pinned to it by the mapped type at every compile.
- This is the absorbed-growth guarantee applied to the decision law itself: a new dominant-axis member (a fourth identity regime, a stream-payload signature) is one declaration on the signature union, and the live `Types.Tags` read propagates it into every `*Exhaustive` record's required keys simultaneously — the new regime lands as a required arm the checker demands, exactly as a new union member lands as a required arm in the data choosers the law routes. The decision law grows by the same one-declaration-inside-the-owner move it prescribes for the concepts it routes, and breaks loudly by the same mechanism.
- The widening break is split across the two gates by *which* growth occurred: a member *added* to the union fails the additive gate (the record is missing a now-required key) and the subtractive gate (the residual is now non-`never`) together, while an arm *deleted* from a passing chooser fails only the subtractive gate (the residual the deleted arm used to clear reappears). Reading which gate fired localizes the cause — a missing required key points at an unrouted regime, a reappeared residual points at a deleted arm — so the chooser's own diagnostic distinguishes "the law gained a case" from "an arm was lost".

[THE_INVARIANT_NESTS_AND_THE_RESIDUAL_COMPOSES]:
- Totality holds recursively because a chooser arm that delegates to a sub-chooser over the narrowed signature is itself a total function, so the parent's residual is cleared by an arm whose own residual is independently driven to `never`. The nesting composes the invariant: the outer `discriminatorsExhaustive` over the dominant axis is total, each arm's inner `Match.value(narrowed)` over the residual axes is total, and the conjunction is total — the proof obligation descends one axis-level per arm and terminates when one tier remains, each level discharging its own residual.
- The nested totality is checked at every level independently, never only at the root: each inner sub-chooser carries its own residual slot driven to `never` by its own terminal, so a sub-chooser that handles the dominant axis but leaves a residual axis partial breaks at *its* terminal while the outer record stays satisfied — the parent's totality does not vouch for the child's. A partial inner sub-chooser is a hole in totality that the outer `discriminatorsExhaustive` cannot see, because the outer gate proves only that the dominant axis is fully dispatched, not that each arm's body is itself total over what it received.
- The recursion has a hard floor the residual makes explicit: an arm whose narrowed signature has exactly one inhabitant needs no sub-chooser at all — the residual is already a singleton the arm resolves directly — and forcing a `Match.value` over a one-inhabitant residual is the degenerate sub-dispatch the demotion direction strips, a matcher whose `exhaustive` is satisfied by zero arms. The nesting descends only while the narrowed residual carries more than one routing tier; a single-tier residual terminates the recursion, and a sub-chooser there is dead ceremony the residual slot proves unnecessary by already being a singleton.

```typescript
// recursive totality: outer record total over the dominant axis, inner Match.value total over the narrowed residual
const choose: (s: Signature) => string = Match.type<Signature>().pipe(
    Match.discriminatorsExhaustive('_tag')({
        Nominal: (s) => (s.wire ? 'Schema.brand' : 'Brand.refined'),
        Reference: () => 'as-const-table', // one-inhabitant residual: resolved directly, no sub-chooser
        Structural: (s) =>
            Match.value(s).pipe(
                Match.when({ cardinality: 'many', wire: true }, () => 'Schema.Union'),
                Match.when({ cardinality: 'many', wire: false }, () => 'Data.taggedEnum'),
                Match.orElse(() => 'Schema.Class'), // Pr = narrowed member: orElse resolves the tier, the inner residual is the `one` cases
            ),
    }),
)
```

[THE_RETURN_TYPE_PIN_IS_TOTALITY_ON_THE_CODOMAIN_AXIS]:
- Totality has a second axis the residual slot does not watch — the codomain — and `Match.withReturnType<Tier>()` is the gate that pins it: placed as the first pipe step it fixes `Ret` before any arm is written, typed `[Ret] extends [...A...] ? Matcher<...Ret> : "withReturnType constraint does not extend Result type"`, so every arm's return is checked against the one declared tier shape and an arm drifting in return shape fails at the arm, not at the routing site downstream. The residual slot proves every *input* is routed; the return pin proves every *output* is a tier — the two axes together make the chooser total on both ends.
- The pin makes codomain totality fail *loud and early* the way the residual makes input totality fail loud and early: an arm whose body returns a value the declared `Ret` does not subsume fails at that arm against the pinned constraint, so a chooser whose arms drift in output shape is caught at the divergent arm rather than at the routing site downstream where an untyped chooser's mismatch first manifests. The reject is the unpinned chooser whose terminal infers `Ret` as the *union* of whatever the arms happened to return — that inference is total by accident, never by obligation, and admits an arm returning the wrong shape as a silent widening of the inferred codomain.
- The return pin interacts with the recursion the same way the residual does: a pinned outer `Ret` flows into every inner sub-chooser's terminal, so an inner `orElse` returning a tier shape that disagrees with the outer pin is caught at the inner terminal because both inherit the one `Ret`. Totality on the codomain is therefore corpus-wide by the same single-value mechanism as totality on the input — pinned once at the root chooser, checked at every arm and every nested terminal, and an arm at any depth returning the wrong codomain shape breaks against the one pin rather than drifting to a downstream mismatch.
