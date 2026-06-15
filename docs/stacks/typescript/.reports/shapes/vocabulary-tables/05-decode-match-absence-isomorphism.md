# Decode Match Absence Isomorphism

[THE_RESIDUAL_IS_THE_EXCLUDE_OPERATOR_NOT_AN_ANALOGY]:
- The residual that surfaces in a partial dispatch is not LIKE `Exclude<keyof typeof Table, Covered>` — it is computed BY `Exclude`, the same operator the structural table reads: each `Match.when`/`Match.tag`/`Match.is` arm transforms the matcher's filter set through `AddWithout<F, X>` to `Without<X | priorWithout>`, and the matcher's `Remaining` parameter is `ApplyFilters<I, Without<X>>` which resolves to `Exclude<I, X>` — so the uncovered complement of a vocabulary dispatch and the unread key complement of a behavior table are one type-level subtraction under two spellings.
- The subtraction is monotone per arm: `AddWithout<Without<WX>, X> = Without<X | WX>` accumulates the handled members into the without-set, and `ApplyFilters` re-derives the residual as `Exclude<I, X | WX>` after every arm, so the residual SHRINKS toward `never` exactly as a table's unread-key set shrinks toward empty when each key is read — the dispatch's progress toward totality is the literal arithmetic of the table's coverage proof.
- `Match.not(pattern, f)` is the dual filter: it adds `Only<WhenMatch<I, P>>` through `AddOnly`, and `ApplyFilters<I, Only<X>>` resolves to `X` directly — so a `not`-arm narrows the residual to the COMPLEMENT it matched rather than subtracting from it, the same partition split a vocabulary expresses as `Exclude<Key, '<a>'>` versus the matched `'<a>'`, now carried as the matcher's live residual rather than written by hand.
- The proof the residual IS `Exclude`: `Match.is('<a>', '<b>')` passes a `SafeRefinement<Literals[number]>` whose `PForExclude` extracts `'<a>' | '<b>'`, so an input union `'<a>' | '<b>' | '<c>'` dispatched through that one arm leaves `Remaining = Exclude<'<a>' | '<b>' | '<c>', '<a>' | '<b>'> = '<c>'` — the matcher now holds `'<c>'` as its residual exactly as `Exclude<keyof typeof Table, '<a>' | '<b>'>` holds it at the table, the literal alphabet driving both subtractions from one source.

```typescript
import { Match, pipe } from 'effect'

const route = pipe( // Remaining after the is-arm = Exclude<'<a>'|'<b>'|'<c>', '<a>'|'<b>'> = '<c>', the matcher carrying it live
    Match.type<'<a>' | '<b>' | '<c>'>(),
    Match.when(Match.is('<a>', '<b>'), (k) => k.length), // k: '<a>'|'<b>'; AddWithout folds '<a>'|'<b>' into the Without set
    Match.either, // (input) => Either<number, '<c>'>; the Left type IS the Exclude residual, never a runtime undefined
)
// REJECT a hand-listed type AdmittedRest = Exclude<'<a>'|'<b>'|'<c>', '<a>'|'<b>'> beside it: the matcher already carries '<c>' as Remaining
```

[BOTH_LEFTS_ARE_EITHER_BUT_CARRY_DIFFERENT_MATTER]:
- The isomorphism is exact on the Right arm and on the carrier SHAPE — `Schema.decodeUnknownEither(Vocab)` returns `Either<A, ParseError>` and `Match.either(matcher)` returns `Either<A, Remaining>`, both putting the admitted value on `Right` and the absence on `Left` — but the two `Left` types are categorically different matter: the decode `Left` is a RUNTIME value (a `ParseError` carrying the structured issue of WHY the foreign value was rejected), the dispatch `Left` is a TYPE (the residual union of WHICH members went unhandled, inhabited at runtime by the unmatched input value itself).
- The decode `Left` answers "why this value is not a member" with a discriminable `ParseIssue` tree the gate produces from the schema; the dispatch `Left` answers "which members this matcher does not cover" with a type the checker subtracts and an inhabitant that is the raw unmatched input — so a decode rejection is recoverable structure about the value, a dispatch residual is recoverable structure about the COVERAGE, and conflating them loses the distinction the two owners exist to draw.
- The bridge that makes them interchangeable where the distinction is irrelevant is `Either.getLeft: Either<A, E> => Option<E>` and `Either.merge: Either<A, E> => E | A`: projecting either `Left` to `Option` discards the asymmetric matter and recovers the symmetric absence rail, so a consumer that needs only presence-or-absence reads both gates identically through `Either.getRight`, while a consumer that needs the rejection diagnostic OR the uncovered-member type reads the asymmetric `Left` of the owner whose matter it requires.
- The asymmetry dictates which gate a totality break surfaces through: adding a vocabulary member breaks `Match.exhaustive` at COMPILE time because the residual `Remaining` is a type that no longer subtracts to `never`, but a decode gate over a widened `Schema.Literal` keeps compiling and rejects the new-but-unlisted value at RUNTIME with a `ParseError` — the dispatch residual is a static proof obligation, the decode rejection is a dynamic one, and a vocabulary that must fail the build on an unhandled member uses the dispatch gate, one that must fail the request uses the decode gate.

[THE_R_AXIS_PARTITIONS_THE_EXIT_FORMS]:
- The decode family's absence carriers split on whether the carrier can THREAD a requirement, and the constraint is in the signature: `decodeUnknown`/`decode`/`validate` keep `Schema<A, I, R>` and carry the requirement on `Effect<A, ParseError, R>`, while `decodeUnknownEither`/`decodeUnknownOption`/`decodeUnknownSync`/`decodeEither`/`decodeOption`/`decodeSync` all constrain to `Schema<A, I, never>` — the synchronous carriers (Either, Option, the throwing form) have no channel to defer a requirement to, so a vocabulary whose decode needs a service is rejected at those exits at compile time, not at runtime.
- The constraint is the carrier admitting that absence and requirement are orthogonal channels the synchronous forms collapse: an `Either<A, ParseError>` or `Option<A>` is already resolved when it returns, so it cannot hold a pending `R` — the `never` constraint is the type system forbidding a context-carrying vocabulary from exiting through a carrier that has nowhere to put the context, the same way `Match.exhaustive` forbids a non-`never` residual from exiting as a total function.
- `validateOption`/`validateSync`/`validateEither` are the documented anomaly and they resolve it precisely: they keep `Schema<A, I, R>` while returning a synchronous `Option`/`A`/`Either`, which is sound ONLY because `validate*` runs the TYPE-side filters and refinements without the decode transform — the transform is what consumes context, so a validate against an interior value already of type `A` touches no requirement and the synchronous carrier is safe even with `R` in the schema's generic, the `R` inert because no transform fires.
- `Schema.is(Vocab): (u: unknown) => u is A` and `Schema.asserts(Vocab): asserts u is A` keep `Schema<A, I, R>` for the same reason validate does — the guard is a structural predicate over the TYPE side, running no transform, so its `R` is inert and the predicate is synchronous; the guard is the gate's form when the foreign value must NARROW in place for a downstream `Match.when(Schema.is(Vocab), f)` rather than be lifted into a carrier, and it is the only gate the structural `as const` table cannot synthesize because its key set is a compile-time fact with no runtime predicate.

```typescript
import { Schema, type Option } from 'effect'

const Vocab = Schema.Literal('<a>', '<b>', '<c>') // R = never; both exit families admit it

const admitOption: (u: unknown) => Option.Option<typeof Vocab.Type> = Schema.decodeUnknownOption(Vocab) // legal: Schema<A,I,never>
const guard: (u: unknown) => u is typeof Vocab.Type = Schema.is(Vocab) // legal even were R present: structural predicate runs no transform
// REJECT Schema.decodeUnknownOption(VocabWithContext): the Option carrier has no channel for R; the type system forbids the exit
```

[EFFECT_EITHER_AND_OPTION_RE_UNIFY_THE_FAMILIES_PAST_R]:
- The `R = never` constraint at the synchronous exits is not a ceiling but a routing rule: a context-carrying vocabulary admits through `decodeUnknown(Vocab)` on the Effect carrier, then `Effect.either: Effect<A, E, R> => Effect<Either<A, E>, never, R>` and `Effect.option: Effect<A, E, R> => Effect<Option<A>, never, R>` move the `ParseError` onto the `Either.Left`/`Option.None` INSIDE the effect — so the Either-of-absence and Option-of-absence shapes the synchronous decoders deny a context-carrying vocabulary are recovered through the Effect-carried adapter, the requirement preserved on `R` while the absence rail is the same `Either`/`Option`.
- This is the exact dual of the dispatch side: `Match.either`/`Match.option` produce the synchronous `Either`/`Option` directly because a matcher carries no requirement channel, while `Effect.either`/`Effect.option` produce the SAME carriers from inside an effect that does — so a decode gate and a dispatch gate over the one vocabulary land in the same `Either<value, absence>` algebra, one synchronously (matcher, `never`-context decode) and one effectfully (`decodeUnknown` then `Effect.either`), the carrier identical and only the requirement channel distinguishing them.
- The error matter carried through differs by family even after re-unification: `Effect.either ∘ decodeUnknown` puts a `ParseError` on the `Left` (the rejection diagnostic), `Match.either` puts the residual type on the `Left` (the uncovered members) — so the unify is structural, not semantic, and a pipeline that decodes then dispatches threads two distinct `Left` matters through two `Either` shapes, the decode `Left` for the boundary failure and the dispatch `Left` for the interior coverage gap.
- The composition order is fixed by provenance: a foreign value crosses the decode gate FIRST (admission, `ParseError` on absence), and the now-typed member crosses the dispatch gate SECOND (coverage, residual on absence) — `Effect.flatMap(decodeUnknown(Vocab), (m) => Effect.succeed(dispatcher(m)))` is the canonical chain, the decode discharging the membership obligation so the dispatcher's `Match.exhaustive` runs over a value the type already proves is in the alphabet, never re-validated.

```typescript
import { Effect, Match, Schema, type ParseResult } from 'effect'

const Vocab = Schema.Literal('<a>', '<b>', '<c>')
const decode = Schema.decodeUnknown(Vocab) // Effect<A, ParseResult.ParseError, never>

const dispatch = Match.type<typeof Vocab.Type>().pipe( // residual subtracts to never; exhaustive proves coverage at compile time
    Match.when('<a>', () => 1),
    Match.when('<b>', () => 2),
    Match.when('<c>', () => 3),
    Match.exhaustive,
)
const admitThenDispatch = (u: unknown): Effect.Effect<number, ParseResult.ParseError, never> =>
    Effect.map(decode(u), dispatch) // decode discharges membership; dispatch runs total over the proven member, no re-check
```

[THE_PROVENANCE_OF_THE_VALUE_SELECTS_THE_OWNER]:
- Admission-as-decode and admission-as-dispatch are one concept whose owner is chosen by where the value CAME FROM, never by the consumer's taste: a value of foreign provenance (`unknown` from the wire, a serialized payload, an external string) crosses the decode gate, which must produce the typed value from raw matter; a value already of the vocabulary's TYPE — produced by an earlier decode, a literal, a prior narrow — crosses the dispatch gate, which routes a value it does not need to construct, only to discriminate.
- The within-decode-family axis mirrors the within-Match axis exactly: `decodeUnknown(Vocab)` admits from `unknown` (the value's wire shape is unproven), `decode(Vocab)` admits from the schema's `Encoded` type (the value is trusted to already have the wire shape, only the transform runs) — the same split as `Match.type<I>()` (the value's type is annotated, the matcher built to receive it) versus `Match.value(i)` (the value is in hand, frozen by `<const I>` at the entry) — provenance-by-construction in both families, the gate's input type declaring how much the value is already known.
- The decode gate is mandatory exactly when `Encoded` differs from `Type`: a vocabulary whose wire alphabet is not its domain alphabet (a `transformLiterals` remap, a `NumberFromString` segment) carries a transform that MUST run to produce the member, so the value cannot reach the dispatch gate without first crossing decode — the dispatch gate has no transform and cannot bridge `Encoded` to `Type`, so a wire value is decoded into the domain alphabet before any `Match.exhaustive` reads it.
- The dispatch gate is mandatory exactly when the discrimination is on SHAPE beyond membership: a vocabulary already in `Type` whose routing depends on a refinement, a per-arm narrowing to `Extract<Union, tag>`, or a divergent return cannot collapse to a decode (decode admits, it does not branch), so the typed member crosses the dispatch gate to be routed — the decode answers "is this a member and what member", the dispatch answers "given the member, which arm", and a pipeline that needs both runs decode then dispatch, never one gate doing the other's job.

[THE_THREE_TOTALITY_TERMINALS_ARE_THREE_RESIDUAL_CLAIMS]:
- `Match.exhaustive`, `Match.orElse`, and `Match.orElseAbsurd` are not interchangeable finalizers but three distinct CLAIMS about the residual `Remaining`, and they pair one-to-one with the decode family's totality dispositions: `exhaustive` requires `Remaining` to be statically `never` (`Matcher<I, F, never, A, ...>`) and pairs with the `Schema.Literal` whose finite alphabet the checker exhausts — both claim the absence case type-level impossible; `orElse(f)` accepts ANY `Remaining`, the dispatch dual of admitting a value whose alphabet membership cannot be proven statically — both claim the absence case live and absorbable; `orElseAbsurd` returns the matched union claiming `Remaining` runtime-unreachable, pairing with `decodeUnknownSync` whose throw is the runtime assertion a non-member cannot arrive.
- The claim each makes is exactly the residual's STATUS in the isomorphism, independent of any resolution it performs: `exhaustive` claims the residual empty and pairs with the decode side's static `Schema.Literal` exhaustion, `orElseAbsurd` claims it phantom and pairs with `decodeUnknownSync`'s runtime throw, and the gap between the two is the same gap between a compile-time and a runtime totality proof the two `either` shapes already drew — so the terminal choice is a statement about WHERE the absence case is discharged, the dispatch side discharging by subtraction-to-`never` and the decode side by transform-or-throw.
- `Match.option` and `Match.either` are the NON-claiming terminals — they neither prove nor resolve the residual but CARRY it, `option` discarding it to `None` and `either` preserving it on `Left` — pairing with `decodeUnknownOption` (absence to `None`, reason discarded) and `decodeUnknownEither` (rejection on `Left`, reason preserved) — so the five-way decode family and the five-way Match terminal family are the same five absence dispositions: prove-impossible, resolve-to-fallback, assert-phantom, carry-as-Option, carry-as-Either, each disposition selecting the owner by the value's provenance and the form by the consumer's algebra.
- The disposition propagates the residual asymmetrically: `Match.option` and `decodeUnknownOption` both ERASE which member or reason was absent (the `None` carries nothing), while `Match.either` preserves the residual TYPE and `decodeUnknownEither` preserves the `ParseError` VALUE — so a partial gate that must report the specific uncovered members reaches for `either` on the dispatch side (the `Left` IS the residual union) and a partial gate that must report the rejection diagnostic reaches for `either` on the decode side (the `Left` IS the issue tree), the two `either`s carrying the two distinct matters the `option`s both throw away.

```typescript
import { Either, Match, Option, Schema } from 'effect'

const Vocab = Schema.Literal('<a>', '<b>')
const partial = Match.type<'<a>' | '<b>' | '<c>'>().pipe(
    Match.when(Match.is(...Vocab.literals), (k) => k), // covers '<a>'|'<b>'; Remaining = '<c>'
    Match.either, // (input) => Either<'<a>'|'<b>', '<c>'>: Left preserves the uncovered residual type
)
// REJECT Match.orElse(() => fallback) here when the caller must REPORT the uncovered member: orElse claims the residual absorbable and discards which member was absent
const uncovered: (i: '<a>' | '<b>' | '<c>') => Option.Option<'<c>'> = (i) => Either.getLeft(partial(i)) // getLeft projects the residual matter to the symmetric absence rail; the decode side projects ParseError the same way
```
