# Match Residual as a Forward Channel

[RESIDUAL_IS_THE_MATCHER_CARRIER]:
- The residual is not computed by the finalizer — it is the matcher's own internal carrier the finalizer exposes: a `ValueMatcher` holds `value: Either<Provided, Remaining>` seeded as `Either.left(input)`, and `Match.either(self)` on a value-matcher is the identity read `self.value`, never a fresh reconstruction — `either` and `option` are projections of a field that already existed before any arm ran.
- The seed is `Left`, the match is the flip to `Right`: `Match.value(i)` is `makeValueMatcher(i, Either.left(i))` — the input enters the residual channel as `Left`, and the *first* arm whose guard passes rewrites the carrier to `Either.right(evaluate(provided))`, so a matched value is `Right(result)` and an unmatched value is the original input still sitting in `Left`. The residual *is* the un-flipped input, typed at exactly the unconsumed cases.
- First-match-wins is structural short-circuit on the carrier, not arm ordering discipline: once the carrier is `Right`, `add` returns `this` unchanged for every subsequent arm (`if (this.value._tag === "Right") return this`), so arms after the winning one are dead code the carrier never evaluates — the `Either` left-bias *is* the dispatch semantics, and the residual a later arm sees is whatever earlier arms left in `Left`.

[FINALIZER_IS_A_CARRIER_PROJECTION_NOT_A_CLOSE]:
- The five finalizers are one carrier read at three altitudes, not five terminal closes: `option` is `either` post-composed with `Either.match({ onLeft: Option.none, onRight: Option.some })`, `orElse(f)` is the same `Either` read with `f(left)` on the residual, and `orElseAbsurd` is `orElse(() => absurd)` — `either` is the primitive and the rest collapse to it, so the residual is a *value* the entire family forwards, never a discarded branch.
- The residual type each finalizer reifies is the *applied* residual `R` (the third matcher slot `RemainingApplied = ApplyFilters<I, Filters>`), not the raw `Filters` lattice: `either` returns `Either<Unify<A>, R>` and `option` `Option<Unify<A>>` with `R` as the `Left`/`None` content — so a partially-handled tagged family hands forward the *exact* sub-union of unhandled arms as the `Left`, ready to drive a second dispatch keyed on the same `_tag`.
- `orElse(f)` binds `f: (_: RA) => Ret` at precisely `RemainingApplied`, so the fallback is not a catch-all over `I` — it is a handler over the residual sub-union the checker has already narrowed, and a fallback that pattern-matches the leftover `_tag` is total against `RA` with its own `exhaustive`-style obligation, not a defensive default.

```typescript
import { Either, Match } from 'effect'

type Cmd =
    | { readonly _tag: 'add'; readonly n: number }
    | { readonly _tag: 'sub'; readonly n: number }
    | { readonly _tag: 'mul'; readonly n: number }

const arith = Match.type<Cmd>().pipe(
    Match.tag('add', 'sub', (c) => (c._tag === 'add' ? c.n : -c.n)),
    Match.either,
)
const folded: Either.Either<number, Extract<Cmd, { _tag: 'mul' }>> = arith({ _tag: 'mul', n: 3 })
```

- `arith` consumes `add`/`sub` in one variadic arm and `Match.either` reifies the leftover as `Left<{ _tag: 'mul'; n: number }>` — the residual is the *exact* unconsumed arm, not the full `Cmd`, so the `Left` channel of a half-finished matcher already carries the narrowed sub-union the next stage dispatches on; spelling `Match.orElse(c => ...)` here would close the fold, while `Match.either` keeps the leftover live for re-entry.

[PROVENANCE_GATES_REUSABLE_VERSUS_APPLIED]:
- Every finalizer branches on `[Pr] extends [never]`, and `Provided` is the one slot that records whether the input was supplied: `Match.type<I>()` seeds `Provided = never` (no value yet) and `Match.value(i)` seeds `Provided = I` (the value is held), so the *same* `either`/`option`/`orElse` call yields a reusable `(input: I) => Either<A, R>` over a type-matcher and a concrete `Either<A, R>` over a value-matcher — provenance, not a second entrypoint, selects function-producing versus value-producing.
- The reusable matcher is the composable forward channel's *combinator* form and the applied value its *staged result* form: a `Match.type` matcher finalized to `either` is a partial dispatch function reusable across inputs and storable in a handler row, while a `Match.value` matcher finalized to `either` is one input's residual already computed — so a pipeline that threads many inputs through staged residuals builds reusable `Match.type` matchers and applies them, never re-seeding a `Match.value` per element.
- The trap: a `Match.value(i)` chain cannot be reused across inputs because its `Provided = I` is the *one* `i` baked into the carrier at construction — re-running it on a second value is impossible, the matcher is spent — so the difference is operational, not cosmetic: build the staged channel from `Match.type`, apply it at the element, and reserve `Match.value` for a one-shot residual whose only consumer is the immediately-following `Either` read.

[ONE_MATCHERS_LEFTOVER_IS_THE_NEXTS_INPUT_DOMAIN]:
- The forward channel composes through `Either.orElse(that: (left: E) => Either<A2, E2>)`: the residual `E` of stage one feeds `that`, which is a second `Match.type<E>()` matcher finalized to `either`, and the result is `Either<A | A2, E2>` — the union of both stages' matched results on the `Right`, and *only* stage two's leftover `E2` on the residual. The partial-match algebra is exact: one matcher's `Remaining` is literally the next matcher's `Input`, the checker proving stage two's `Match.type<E>` is keyed on precisely the arms stage one declined.
- Staging *narrows* the residual monotonically — `E -> E2 -> E3` — and `Either.orElse` is the fold step, so a chain of partial matchers each consuming a sub-vocabulary is one `Either.orElse`-threaded pipeline whose final `Left` is the arms no stage claimed; pairing the last stage's `Match.exhaustive` (residual `never`) collapses the final `Left` to `never`, turning the staged channel into a total dispatch assembled from partial pieces.
- `Either.flatMap(f: (right: A) => Either<A2, E2>)` is the dual direction — it threads the *matched* value forward and *accumulates* residuals as `E | E2` — so the success-side staging (refine a matched result through a second matcher that may itself decline) widens the leftover while the failure-side staging (`orElse`) shrinks it: the two combinators are the two axes of the residual lattice, `flatMap` growing it and `orElse` collapsing it, selected by which channel the next stage consumes.

```typescript
import { Either, Match } from 'effect'

type Token =
    | { readonly _tag: 'num'; readonly v: number }
    | { readonly _tag: 'op'; readonly v: string }
    | { readonly _tag: 'ws' }
    | { readonly _tag: 'eof' }

const operative = Match.type<Token>().pipe(
    Match.tag('num', (t) => `n:${t.v}`),
    Match.tag('op', (t) => `o:${t.v}`),
    Match.either,
)
const trivial = Match.type<Extract<Token, { _tag: 'ws' | 'eof' }>>().pipe(
    Match.tag('ws', () => 'skip'),
    Match.tag('eof', () => 'halt'),
    Match.exhaustive,
)
const classify = (t: Token): string => operative(t).pipe(Either.orElse((rest) => Either.right(trivial(rest))), Either.merge)
```

- `operative` reifies its leftover as `Left<{ _tag: 'ws' } | { _tag: 'eof' }>`, `trivial` is a *second* matcher whose `Match.type` input is exactly that residual — the checker rejects `trivial` if `operative` ever widens its consumed set, and `Either.orElse` re-enters the leftover into stage two, `Either.merge` collapsing the now-`never`-residual `Either<string, never>` to `string`; the loose spelling is one giant `Match.exhaustive` re-listing all four arms at one altitude, which forfeits the reusable `trivial` sub-matcher and the proof that the two stages partition the family.

[OPTION_RESIDUAL_IS_THE_DROP_DUAL]:
- `option` and `either` are one carrier read forking on whether the residual *identity* survives: both flip `Left -> Right` on match, but `option` post-composes `Either.match({ onLeft: None, onRight: Some })` and *erases* the `Left` content to `None`, so the choice between them is the choice between forwarding the leftover as a typed re-enterable domain (`either`) and forwarding only the bit that it was unmatched (`option`) — the matched `Unify<A>` is identical across both, the divergence is entirely on the residual channel.
- The duality is a forward-channel selection law, not two unrelated finalizers: a residual that *re-enters* a downstream matcher demands `either` (the `Left` is the next stage's input domain and must carry its type), a residual that is *abandoned* takes `option` (the leftover is dropped, its type dead weight) — so the finalizer is selected by the residual's downstream fate, and downgrading an `either` stage to `option` is the irreversible point past which the leftover can no longer drive a staged re-entry.
- The two duals fold over collections through their carrier's own first-success combinator: `Either.orElse` chains `either`-stages on the `Left` (the leftover threads to the next matcher), while `Option.firstSomeOf([...])` folds `option`-stages on the `Some` (the first matcher to produce a value wins, residuals vanishing as `None`) — so a recovery pipeline that must report *what* declined uses the `either`/`orElse` axis, a best-effort pipeline that only needs *a* result uses the `option`/`firstSomeOf` axis, the same staged dispatch on two residual disciplines.

[FORWARD_CHANNEL_FIDELITY_IS_THE_RESIDUAL_FIDELITY]:
- The forward channel's whole value is the identity `R = true leftover`, and any arm that matches at runtime without subtracting from `RemainingApplied` breaks it *silently at the arm and loudly two stages downstream*: a non-subtracting arm leaves its handled case in `R`, so `Either.orElse`'s stage-two `Match.type<R>` demands an arm for a case stage one already ran — the channel propagates a phantom domain forward, and the re-entry site, not the offending arm, is where the checker complains. Re-entry composition is the diagnostic that converts a runtime-only over-match into a compile error at the seam.
- `orElseAbsurd` is the deliberate close that forwards *no* channel: unlike `exhaustive` it does not demand `RemainingApplied = never`, so where the programmer knows the residual is empty but the type carries a phantom case the type system cannot subtract, `orElseAbsurd` asserts the leftover unreachable at runtime — the one finalizer in the family that produces neither a `Left` to re-enter nor a `None` to drop, terminating the channel on an empty-residual proof rather than threading it.
- The channel-fidelity law subsumes the residual-fidelity rule it builds on: a matcher used only as a terminal `exhaustive`/`orElse` tolerates a phantom residual that an empty `orElseAbsurd`-known case papers over, but the *same* matcher reused as a forward-channel stage cannot — the leftover is now a live input domain, so a stage that re-enters its residual must have every arm subtract exactly, the composition raising the precision bar a one-shot close never enforces.

[STAGED_RESIDUAL_VERSUS_TAGSEXHAUSTIVE]:
- The staged forward channel and `tagsExhaustive` are two answers to one obligation, selected by whether the leftover crosses a stage boundary: `Match.tagsExhaustive(handlers)` self-finalizes with no residual to forward (totality enforced on the handler record's key shape, returning `R` directly), so it is the collapse for a *single-altitude* total `_tag` dispatch, while the staged `either`/`orElse` channel is the collapse for a dispatch *split across stages* where each stage owns a sub-vocabulary and the leftover is the next stage's domain.
- The reusable-matcher provenance is what makes staging worth its weight: a `tagsExhaustive` matcher is total but monolithic — one record handling every arm — whereas a chain of `Match.type<sub>` matchers finalized to `either` and threaded by `Either.orElse` lets each stage be authored, named, and reused independently, the partition proof carried by the residual types rather than by one exhaustive record; the trade is monolithic-total versus composable-partial, and a dispatch whose stages have independent owners or reuse takes the staged form.
- `Either.all([...stages])` accumulates several independent partial matchers' residuals into one product `Either`: the success side is the tuple of every stage's matched result and the residual side the union `E1 | E2 | ...` of every stage's leftover, so a multi-axis classification (one matcher per orthogonal axis, each partial) folds to one `Either.all` whose `Left` names every axis that declined — the applicative fold over residuals the sequential `orElse` chain cannot express.

```typescript
import { Either, Match } from 'effect'

type Event =
    | { readonly _tag: 'click'; readonly x: number }
    | { readonly _tag: 'key'; readonly code: string }
    | { readonly _tag: 'tick' }

const spatial = Match.type<Event>().pipe(Match.tag('click', (e) => e.x), Match.either)
const textual = Match.type<Event>().pipe(Match.tag('key', (e) => e.code), Match.either)
const probe = (
    e: Event,
): Either.Either<readonly [number, string], Extract<Event, { _tag: 'key' | 'tick' }> | Extract<Event, { _tag: 'click' | 'tick' }>> =>
    Either.all([spatial(e), textual(e)] as const)
```

- `spatial` and `textual` are independent partial matchers over the *same* `Event`, each reifying its own leftover as `Left`; `Either.all` folds them so a value matching *both* axes yields `Right([x, code])` and the first axis to decline supplies the `Left` typed at the *union* of both axes' residuals — never `unknown`, the exact sub-unions each axis declined — so the applicative product of two residual channels replaces two sequential `if` ladders re-reading `e._tag` whose joint failure would need a hand-merged sentinel.
