# [TYPESCRIPT_SURFACES_AND_DISPATCH]

A concern with many call shapes keeps one dense surface, never a family of shallow ones. One entrypoint absorbs every modality — key, batch, and query call shapes are one input union under one declaration whose overload signatures give each shape its own return, plurality is proven by `NonEmptyReadonlyArray` arity rather than promised over a widened array, and pipe-versus-direct posture is one `Function.dual` definition, never a parallel pair. A closed tagged family dispatches through its own generated surface — `$match`, `$is` — while structural and predicate dispatch composes `Match` arms whose terminal is the architectural decision: `exhaustive` proves closure, `option` declares the unmatched residue non-failing absence, `either` hands the residue to the next stage as typed material. Per-kind behavior is one mapped handler record dispatched by one generic indexed call, and cross-cutting policy attaches at the `Effect.fn` definition seam, where the pipeline states deadline, resilience, and telemetry in wrap order and every step reads the original call arguments. The named defect this page refuses is surface spam: `resolve`/`resolveMany`/`resolveByKey` siblings, arity twins, suffix families, boolean mode knobs, data-first/data-last twins, consumer-side handler reassembly, and policy buried in bodies the declaration should state.

## [01]-[FORM_CHOOSER]

When a concern matches several rows, the most specific wins; the rail the arms return is orthogonal to the form and is read after the form is fixed.

| [INDEX] | [CONCERN_SIGNATURE]                       | [FORM]                                      | [REJECTED_FORM]                                  |
| :-----: | :---------------------------------------- | :------------------------------------------ | :----------------------------------------------- |
|  [01]   | one concern, several call shapes           | overloaded declaration over one input union | `resolve`/`resolveMany`/`resolveWhere` siblings   |
|  [02]   | plural call shape must prove plurality     | `NonEmptyReadonlyArray` modality            | `batch: boolean` beside a widened array           |
|  [03]   | closed family, arms local, coverage owed   | `$match` / `Match.exhaustive` terminal      | `orElse` fallback absorbing future tags           |
|  [04]   | partial dispatch, residue is absence       | `Match.option` terminal                     | `null` return or sentinel                         |
|  [05]   | staged dispatch, residue flows onward      | `Match.tag` arms + `Match.either` terminal  | pre-filtered parallel matchers per stage          |
|  [06]   | keyed static correspondence                | vocabulary row lookup                       | `Match`/`switch` arms restating the table         |
|  [07]   | operator with a pipe subject               | one `Function.dual` definition              | data-first plus curried twin pair                 |
|  [08]   | per-kind behavior owned familywide         | mapped handler record, one generic dispatch | `switch` per consumer; call-site record assembly  |
|  [09]   | cross-cutting policy on one function       | `Effect.fn` definition-seam pipeline        | policy hand-woven inside the body                 |
|  [10]   | open structural input, no tag              | `Match.when` predicate and refinement arms  | `typeof` ladder with casts                        |

## [02]-[ENTRYPOINT_COLLAPSE]

[OVERLOAD_COLLAPSE]:
- Law: one concern exposes one entrypoint; the call-shape family — by key, by batch, by query — is one input union discriminated inside one body, and each shape's return is declared by an overload signature above one wider implementation signature, so the return type follows the input shape and the sibling family collapses into one name whose next modality lands as one overload line plus one dispatch arm, never a new export.
- Law: overloaded entrypoints use the `function` declaration form — overload signatures tolerate the wider implementation signature there, while a `const` annotated with the same call signatures checks its arrow against every signature simultaneously and forces the cast the declaration form never needs; single-signature operators stay `const` arrows, and the declaration form is earned by the overload set alone.
- Law: a public conditional-return generic — `<I extends Key | Batch<Key>>(input: I) => I extends Batch<Key> ? Batch<Row> : Row` — states the same per-shape contract, but no implementation body checks against an unresolved conditional, so it compiles only through an `as`; where overloads can carry the shapes, overloads win, and the conditional form retreats to derived type surfaces.
- Law: discrimination reads evidence the value already carries — `Predicate.isString`, a `Predicate.hasProperty` shape probe, a `$is` tag probe — ordered so the tuple modality is the residue of elimination: no `isArray` guard preserves a `readonly` tuple in a union's true branch (the predicate narrows to mutable `Array` and drops it), so the batch member is never probed directly, every record shape is eliminated first, and the tuple is what remains. A `mode` parameter beside the input restates what the value answers: the smuggled knob.
- Exemption: the `function` keyword and its single `return` are the platform seam overload syntax forces; the body stays one expression.

[MODAL_ARITY]:
- Law: the batch modality is `NonEmptyReadonlyArray` — `readonly [A, ...Array<A>]` — so plurality is a fact of the type: the batch overload returns a non-empty result derived by construction (`Array.headNonEmpty` resolves the proven head, `Effect.forEach` sweeps the tail, `[head, ...tail] as const` recombines the tuple), never a claimed plurality the interior must re-prove over a possibly-empty array.
- Law: empty is not a batch — a caller holding a possibly-empty collection resolves emptiness at its own seam or routes the query shape; admitting bare `ReadonlyArray` as the batch modality discards non-emptiness for every downstream consumer at once.
- Boundary: how the swept rail combines — abort versus accumulate, concurrency degree — is the rail page's disposition; this surface owns only the arity discriminant and the shape-following return.

```typescript
import { Array, Data, Effect, HashMap, Predicate } from "effect"

type Key = string
type Row = { readonly key: Key; readonly rank: number; readonly label: string }
type Sweep = { readonly floor: number }
type Batch<A> = Array.NonEmptyReadonlyArray<A>
type Ledger = HashMap.HashMap<Key, Row>

class Missing extends Data.TaggedError("Missing")<{ readonly key: Key }> {}

const _fetched = (ledger: Ledger, key: Key): Effect.Effect<Row, Missing> =>
  Effect.mapError(HashMap.get(ledger, key), () => new Missing({ key }))

const _swept = (ledger: Ledger, sweep: Sweep): ReadonlyArray<Row> =>
  Array.filter(HashMap.values(ledger), (row) => row.rank >= sweep.floor)

function resolved(ledger: Ledger, input: Key): Effect.Effect<Row, Missing>
function resolved(ledger: Ledger, input: Batch<Key>): Effect.Effect<Batch<Row>, Missing>
function resolved(ledger: Ledger, input: Sweep): Effect.Effect<ReadonlyArray<Row>>
function resolved(
  ledger: Ledger,
  input: Key | Batch<Key> | Sweep,
): Effect.Effect<Row | Batch<Row> | ReadonlyArray<Row>, Missing> {
  return Predicate.isString(input)
    ? _fetched(ledger, input)
    : Predicate.hasProperty(input, "floor")
      ? Effect.succeed(_swept(ledger, input))
      : Effect.zipWith(
          _fetched(ledger, Array.headNonEmpty(input)),
          Effect.forEach(Array.tailNonEmpty(input), (key) => _fetched(ledger, key)),
          (head, tail) => [head, ...tail] as const,
        )
}

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Missing, resolved }
export type { Batch, Key, Ledger, Row, Sweep }
```

## [03]-[MATCH_TERMINAL]

[TERMINAL_SELECTION]:
- Law: the `Match` terminal is an architecture decision made by what the unmatched residue means: `Match.exhaustive` is the only terminal for a closed family the module owns — a missing arm is a compile error, so a new tag breaks every dispatch site; `Match.option` declares the residue non-failing absence; `Match.either` keeps the residue as typed material — the left channel carries the narrowed leftover union, so staged dispatch threads stages through `Either` and the next stage's input type is the proof of what remains; `Match.orElse` is lawful only over genuinely open input, because on a closed family the fallback silently absorbs every future tag the exhaustive terminal would have surfaced; `Match.orElseAbsurd` throws and is rejected outright.
- Law: `Match.withReturnType<Ret>()` composes immediately after `Match.type`/`Match.value` and before the first arm, so every arm is checked against the contract at the arm; placed later it only validates the accumulated union, and a misfit surfaces at the terminal instead of at the offending arm.
- Law: `Match.tag` subtracts exactly and its leading segment is variadic — one arm carries several tags and receives their extracted union; literal and type-refinement patterns subtract the matched shape from the residue, while bare boolean predicates admit without subtracting, so the residue the terminal sees is computed arm by arm and the `either` left type is read, not asserted.

[FAMILY_DISPATCH]:
- Law: a `Data.taggedEnum` family dispatches through its own generated surface — `$match` is the total record dispatch, data-first inside generic operations so the family's type parameters stay bound, and `$is` is the reusable refinement every filter and find site narrows through; a `_tag ===` comparison restated per site re-derives what the family already generated.
- Law: the `taggedEnum` surface is module-interior dispatch vocabulary — the factory-call `const` has no nameable annotation, so exporting it demands the hand-written type the export gate forbids inferring; the family's static type exports freely, dispatch operations export annotated, and a family that must export constructors is the class-shaped owner the shape page legislates.
- Law: `Match.valueTags(value, arms)` is the immediate exhaustive record dispatch over an already-held union value — its arm record types excess keys `never`, so a stale or misspelled arm is a compile error and no matcher value is built for a one-shot dispatch; `Match.discriminatorsExhaustive("<field>")` owns the family whose discriminant is a foreign field name, dispatching the wire shape without re-tagging it first.
- Law: record form versus pipeline: the record forms (`$match`, `Match.valueTags`, `Match.tagsExhaustive`) serve a closed family whose arms are all local — coverage read at a glance; the `Match.type` pipeline builds the reusable dispatch value and serves arms that mix tag, structural, and predicate patterns or a terminal other than exhaustive; `Match.value` opens the same pipeline over one already-held value and earns its matcher only when arms exceed what `valueTags` states in place.
- Boundary: a `Match` whose arms each return a static row restates a keyed table — a keyed correspondence dispatches through the vocabulary lookup the table already is; `Match` owns structural and predicate dispatch on non-keyed shapes.

```typescript
import { Array, Data, Match, Option, pipe } from "effect"
import type { Either } from "effect"

type Signal<A> = Data.TaggedEnum<{
  readonly Live: { readonly value: A }
  readonly Degraded: { readonly value: A; readonly cause: string }
  readonly Halted: { readonly cause: string }
}>

interface SignalDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: Signal<this["A"]>
}

const Signal = Data.taggedEnum<SignalDefinition>()

const carried = <A>(signal: Signal<A>): Option.Option<A> =>
  Signal.$match(signal, {
    Live: ({ value }) => Option.some(value),
    Degraded: ({ value }) => Option.some(value),
    Halted: () => Option.none(),
  })

const causes = <A>(signals: ReadonlyArray<Signal<A>>): ReadonlyArray<string> =>
  Array.map(Array.filter(signals, Signal.$is("Halted")), ({ cause }) => cause)

type Wire =
  | { readonly _tag: "Packet"; readonly body: string }
  | { readonly _tag: "Ack"; readonly seq: number }
  | { readonly _tag: "Nack"; readonly seq: number; readonly cause: string }

const settled: (wire: Wire) => Either.Either<number, { readonly _tag: "Packet"; readonly body: string }> = pipe(
  Match.type<Wire>(),
  Match.withReturnType<number>(),
  Match.tag("Ack", "Nack", ({ seq }) => seq),
  Match.either,
)

type Probe = { readonly channel: string; readonly score: number } | { readonly raw: string }

const scored: (probe: Probe) => Option.Option<number> = pipe(
  Match.type<Probe>(),
  Match.withReturnType<number>(),
  Match.when({ raw: Match.nonEmptyString }, ({ raw }) => raw.length),
  Match.when({ score: (value: number) => value >= 0.8 }, ({ score }) => score),
  Match.option,
)

// --- [EXPORTS] ---------------------------------------------------------------------------

export { carried, causes, scored, settled }
export type { Probe, Signal, Wire }
```

## [04]-[DUAL_ENTRY]

[DUAL_DEFINITION]:
- Law: an operator whose first data-first parameter is a pipe subject publishes both postures from one `Function.dual` definition — the data-first body is the single implementation, the `const` annotated with the two call signatures is the published contract, and the parallel curried twin or the consumer adapter lambda `(value) => operator(value, argument)` is the deleted pair.
- Law: the call-signature annotation carries what the body cannot state for the data-last side — the flowing generics and channel unions — and it is simultaneously the explicit export annotation the compiler demands, so one spelling serves contract, gate, and both postures; the `dual<DataLast, DataFirst>` type-argument spelling restates both parameter lists away from the declaration and survives only where no `const` annotation exists.
- Law: posture and modality are orthogonal axes — input modality lives in the entrypoint's input union, posture is one dual over the settled data-first shape; constructors and admission factories have no subject and never dual.

[DISCRIMINANT_SELECTION]:
- Law: arity discriminates when every call shape has a distinct length — `Function.dual(2, body)` routes the two-argument call to data-first; a trailing optional or variadic parameter collides the lengths, so the discriminant becomes a predicate over the first argument — `(args) => Array.isArray(args[0])`, `(args) => Effect.isEffect(args[0])`, a `Predicate.isTagged` probe — answering only whether the first argument is the subject, never selecting behavior: behavior routed through the discriminant is the boolean knob smuggled into the calling convention.
- Law: the predicate reads `IArguments` — the raw call — so it is total over both postures, including the zero-argument data-last call, where `args[0]` is `undefined` and must answer false.

```typescript
import { Array, Data, Effect, Function, Order, pipe } from "effect"

type Row = { readonly key: string; readonly rank: number }

class Ceiling extends Data.TaggedError("Ceiling")<{ readonly rank: number; readonly limit: number }> {}

const _BY_RANK: Order.Order<Row> = Order.mapInput(Order.number, (row: Row) => row.rank)

const gated: {
  (limit: number): <E, R>(self: Effect.Effect<Row, E, R>) => Effect.Effect<Row, Ceiling | E, R>
  <E, R>(self: Effect.Effect<Row, E, R>, limit: number): Effect.Effect<Row, Ceiling | E, R>
} = Function.dual(
  2,
  <E, R>(self: Effect.Effect<Row, E, R>, limit: number): Effect.Effect<Row, Ceiling | E, R> =>
    Effect.filterOrFail(self, (row) => row.rank <= limit, (row) => new Ceiling({ rank: row.rank, limit })),
)

const ranked: {
  (order?: Order.Order<Row>): (self: ReadonlyArray<Row>) => ReadonlyArray<Row>
  (self: ReadonlyArray<Row>, order?: Order.Order<Row>): ReadonlyArray<Row>
} = Function.dual(
  (args) => Array.isArray(args[0]),
  (self: ReadonlyArray<Row>, order: Order.Order<Row> = _BY_RANK): ReadonlyArray<Row> => Array.sort(self, order),
)

const swept = (rows: ReadonlyArray<Row>, limit: number): Effect.Effect<ReadonlyArray<Row>, Ceiling> =>
  Effect.forEach(ranked(rows), (row) => pipe(Effect.succeed(row), gated(limit)))

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Ceiling, gated, ranked, swept }
export type { Row }
```

## [05]-[HANDLER_RECORD]

[RECORD_CONTRACT]:
- Law: per-kind behavior is one record at the owner, keyed by the vocabulary and checked against one mapped contract — `{ readonly [K in Kind]: (payload: Payload[K]) => Rail }` — and dispatch is one generic indexed call, `HANDLERS[kind](payload)`: the mapped annotation is what resolves the indexed access to a single correlated signature, so the per-kind payload flows through without casts.
- Law: annotation versus `satisfies` is adjudicated by the record's consumer — the record backing a correlated generic dispatch is annotated with its mapped contract, because `satisfies` keeps the inferred per-row function types and the generic indexed call then faces a union of signatures it cannot satisfy; `satisfies` is the form for the vocabulary table whose row literals feed derivation (`keyof typeof`, indexed-access projections), where widening is the thing being prevented.
- Law: a new kind is one vocabulary row, one payload field, and one handler row — the mapped contract turns the missing handler into a compile error at the record while every consumer stays untouched; the diff of the next kind never leaves the owner.
- Reject: consumer-side reassembly — a call site assembling its own record over exported loose handlers, a `switch` over kinds repeated per consumer, an `Object.keys` iteration re-deriving what `Kind` already is.

[INLINE_ATTACHMENT]:
- Law: composition attaches at the row — each handler's admission, guard, and projection compose inside its own row value, and shared per-kind policy is a column on the vocabulary row the handler reads, so behavior variation is data the table already owns, never a wrapper stack applied after the record exists.
- Law: a method record on an owner object is the same law — the object is contract-annotated or `satisfies`-checked at its declaration and never assembled field-by-field afterward; a record built by staged mutation forfeits the missing-key compile error that is the record's reason to exist.

```typescript
import { Data, Effect } from "effect"

const ROUTE = {
  open: { weight: 1, label: "<label-a>" },
  amend: { weight: 2, label: "<label-b>" },
  close: { weight: 4, label: "<label-c>" },
} as const satisfies Record<string, { readonly weight: number; readonly label: string }>

type Kind = keyof typeof ROUTE

type Payload = {
  readonly open: { readonly key: string }
  readonly amend: { readonly key: string; readonly delta: number }
  readonly close: { readonly key: string; readonly seal: string }
}

type Receipt = { readonly kind: Kind; readonly weight: number; readonly note: string }

class Refused extends Data.TaggedError("Refused")<{ readonly kind: Kind; readonly cause: string }> {}

type Handlers = { readonly [K in Kind]: (payload: Payload[K]) => Effect.Effect<Receipt, Refused> }

const _receipt = (kind: Kind, note: string): Receipt => ({ kind, weight: ROUTE[kind].weight, note })

const HANDLERS: Handlers = {
  open: ({ key }) => Effect.succeed(_receipt("open", key)),
  amend: ({ key, delta }) =>
    delta === 0
      ? Effect.fail(new Refused({ kind: "amend", cause: "<zero-delta>" }))
      : Effect.succeed(_receipt("amend", `${key}+${delta}`)),
  close: ({ key, seal }) => Effect.succeed(_receipt("close", `${key}:${seal}`)),
}

const submitted = <K extends Kind>(kind: K, payload: Payload[K]): Effect.Effect<Receipt, Refused> =>
  HANDLERS[kind](payload)

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Refused, ROUTE, submitted }
export type { Kind, Payload, Receipt }
```

## [06]-[ASPECT_SEAM]

[SEAM_PLACEMENT]:
- Law: cross-cutting policy attaches at the `Effect.fn` definition seam — `Effect.fn("<span>")(body, ...pipeline)` — where the name opens a traced span per call, the body is the unit an outer retry re-runs, and every pipeline step wraps the whole declaration, so deadline, resilience, and annotation are recoverable by reading the declaration tail; policy woven inside the body, or re-added by a wrapper at the call site, stops being recoverable from the declaration and is the rejected placement.
- Law: every pipeline step receives the original call arguments after the effect — `(effect, ...args)` — so policy parameterizes on the call without threading arguments through the body or closing over module state: a deadline fault carrying the argument's key, log annotation carrying the call's inputs.
- Law: the seam splits by attempt scope — a concern that must observe one attempt composes before the retry step; a concern that must observe the whole call composes after it; the exported annotation on the seam's `const` states the full fault union the stack produces, readable without opening the body.
- Use: `Effect.fnUntraced` where span allocation is a measured cost on a hot path — the same seam and pipeline, no tracer.
- Boundary: `Schedule` composition algebra, the fault-family taxonomy, and telemetry semantics are the rail page's; this page owns only where the policy value attaches and the order it stacks.

[PIPELINE_ORDER]:
- Law: pipeline order is wrap order — the first step is innermost, each later step encloses everything before it — so the two resilience geometries are both spellable and the declaration states which was chosen: deadline-then-retry gives each attempt its own budget; retry-then-deadline places one budget over all attempts.
- Law: retry policy is a value — one `Schedule` composed once at the module and referenced by the seam — refined by `while`/`until` on the fault tag so only the transient family re-drives; a hand-rolled retry loop, or a schedule rebuilt inline per declaration, dissolves the policy the value form makes auditable.

```typescript
import { Data, Effect, Predicate, Schedule } from "effect"

type Quote = { readonly key: string; readonly grade: number }

class Flap extends Data.TaggedError("Flap")<{ readonly key: string }> {}
class Lapse extends Data.TaggedError("Lapse")<{ readonly key: string }> {}

const _PULSE = Schedule.exponential("80 millis").pipe(
  Schedule.jittered,
  Schedule.intersect(Schedule.recurs(4)),
)

const _graded = (key: string): Effect.Effect<Quote, Flap> =>
  key.length === 0 ? Effect.fail(new Flap({ key })) : Effect.succeed({ key, grade: key.length * 8 })

const resolved: (key: string, ceiling: number) => Effect.Effect<Quote, Flap | Lapse> = Effect.fn("<span-a>")(
  function* (key: string, ceiling: number) {
    const quote = yield* _graded(key)
    return quote.grade <= ceiling ? quote : yield* new Flap({ key })
  },
  (effect, key) => Effect.timeoutFail(effect, { duration: "250 millis", onTimeout: () => new Lapse({ key }) }),
  Effect.retry({ schedule: _PULSE, while: Predicate.isTagged("Flap") }),
  (effect, key, ceiling) => Effect.annotateLogs(effect, { key, ceiling }),
)

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Flap, Lapse, resolved }
export type { Quote }
```
