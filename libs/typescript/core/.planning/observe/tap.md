# [CORE_TAP]

Hook-rail vocabulary of the observe plane, the fourth owner beside `convention`, `slo`, and `board`: every branch hook point is a `rasm.<pkg>.<domain>.<point>` name row on ONE typed registry — the `TapPoint` brand admits the spelling once, the closed veto/observe/replay modality table fixes dispatch semantics as data columns, the subscription contract types every subscriber against its point's fact schema, and subscriber failure isolates onto the `value` `FaultClass` lattice as `Breach` evidence, so a publisher can never be broken by its inspectors. Core defines every shape the rail runs and executes none of it — fibers, scheduling, and delivery live in the runtime wave — and the telemetry-as-tap law closes the plane: a signal emitter is an observe subscription over domain facts, never an emit call inside a domain fold, so the zero-exporter boundary holds by construction. Rails scope through the `AppIdentity.Key` app brand, so two apps composing identical point names occupy distinct rails; tenancy partitioning stays `TenantContext.scope` and never enters the hook plane. Its module is `core/src/observe/tap.ts`; a new hook point is one name row, a new modality is one tuple entry with its row, a new signal emitter is one subscription value.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                            | [PUBLIC]               |
| :-----: | :----------------- | :-------------------------------------------------------------------------------- | :--------------------- |
|  [01]   | `MODALITY_TABLE`   | the closed modality tuple, the dispatch rows, the feedback/purity/buffer axes     | `Tap` (modality reads) |
|  [02]   | `POINT_VOCABULARY` | the point-name brand, the standing point rows, the fact-binding point mint        | `Tap` (point mint)     |
|  [03]   | `RAIL_CONTRACT`    | subscription constructors, registry admission, breach isolation, the veto verdict | `Tap`, `TapFault`      |

## [02]-[MODALITY_TABLE]

- Owner: the modality anchors — `_modalities`, the closed dispatch tuple, and `_rows`, the dispatch table whose columns are the whole execution semantic: `feedback` (the subscriber's return re-enters the emitting fold), `pure` (the handler is a plain function with no effect carrier and no requirement channel), `buffered` (delivery replays the publisher's retained window before live facts); the merged-hub guard pair ties tuple and table closed in both directions.
- Law: dispatch reads columns, never names — the runtime executor selects pure-fold, forked-delivery, or window-then-live delivery from the row, so a `switch` over modality names in any executor restates the table and a new modality is one tuple entry with its row.
- Law: veto is the one feedback modality and pure by the table — `feedback: true` pairs with `pure: true`, the decision is `(fact) => Option<Veto>` with refusal evidence riding the `Veto` value, and an effectful veto is unspellable because the handler case carries no carrier; the zero-exporter boundary therefore survives feedback — a veto can gate a fold, never open a clock, capability, or exporter inside it.
- Law: observe is fire-and-forget — the executor forks each delivery, and a subscriber fault is `[04]`'s isolated breach, never the publisher's failure.
- Law: replay is observe with the buffered window — the publisher's own retention (the machine hub's sliding `lag`, a `PubSub` `{ replay }` construction) bounds what a late subscriber sees, so replay is a warm-up window, never durable history, and a point whose publisher retains nothing lawfully refuses the modality at its row.
- Growth: a new modality is one tuple entry with its row; a new execution axis is one `Row` field with its column on each row.
- Boundary: fibers, scheduling, and delivery mechanics are the runtime wave's executor — this table is the data it reads.
- Packages: `effect` (`Array`, `Cause`, `Data`, `Effect`, `Either`, `Metric`, `Option`, `Predicate`, `Schema`, `Struct`); `../value/identity.ts` (`AppIdentity`); `../value/fault.ts` (`FaultClass`).

```typescript signature
import { Array, Cause, Data, type Effect, Either, Metric, Option, Predicate, Schema, Struct, type Types } from "effect"
import type { AppIdentity } from "../value/identity.ts"
import { FaultClass } from "../value/fault.ts"

const _modalities = ["veto", "observe", "replay"] as const

const _rows = {
  veto: { feedback: true, pure: true, buffered: false },
  observe: { feedback: false, pure: false, buffered: false },
  replay: { feedback: false, pure: false, buffered: true },
} as const
```

## [03]-[POINT_VOCABULARY]

- Owner: `_Name`, the `TapPoint` brand — the dotted four-segment `rasm.<pkg>.<domain>.<point>` spelling under one pattern refinement, minted through the interior `decodeSync` over declaration-proven literals; `_points`, the standing name rows core's own publishers anchor — `macrostep` (the `state/machine` actor's settled-send fact hub), `quarantine` (the `interchange/codec` poison divert's held-frame evidence), `support` (the `interchange/invoke` gateway's delivered `SupportCapture`) — each row carrying its name and admitted modality set; and the polymorphic `point` mint pairing a name row or a fresh `rasm.`-templated literal with the fact schema at the seam where the fact type is visible.
- Law: the fact schema binds at the mint, never on the row — `observe` sits on the S1 wave and the standing publishers' fact owners live at `state` (`Transition.Fact`) and `interchange` (`SupportCapture`, `Quarantine.Frame`), so a schema-carrying row here is a peer or upward import; the row stays name-and-modality data, the app root mints the `Point` where both the row and the fact owner are in scope, and each publisher page carries the correspondence as its own anchor sentence. The mint stores the schema's type side — `Schema.typeSchema` — so a wire-twinned fact owner binds whole and its encoded dialect never enters the hook plane.
- Law: rails scope through the app brand — a registry is keyed by `AppIdentity.Key` beside its rows, so two apps composing identical point names occupy distinct rails and compose without collision; the branded scope key stays `TenantContext.scope`, the tenancy partition, and never enters the hook plane.
- Law: point mints are declaration-time — the mint re-proves the brand over the compile-checked `rasm.${string}.${string}.${string}` template, so a malformed name surfaces at module init on the authoring side, never inside a dispatch; a point defaults to the `observe` modality because observation is the one semantic every publisher admits, and veto or replay is a deliberate widening on the row.
- Growth: a new hook point is one `_points` row for a core publisher, or one app-side `point` mint over its own name; a publisher gaining veto or replay widens its row's modality set, never mints a second point.
- Boundary: publication stays the publisher's law — `state/machine.md#ACTOR`, `interchange/invoke.md#COMMAND_GATEWAY`, and `interchange/codec.md#FAULT_RAIL` each name their point beside the surface that feeds it; attribute, metric, and event name space stays `convention`'s — a point name is hook vocabulary, never a telemetry attribute key.
- Packages: `effect` (`Schema`, `Predicate`); `../value/identity.ts` (`AppIdentity`).

```typescript signature
const _Name = Schema.String.pipe(
  Schema.pattern(/^rasm\.[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*$/),
  Schema.brand("TapPoint"),
)
const _name = Schema.decodeSync(_Name)

const _points = {
  macrostep: { name: "rasm.core.state.macrostep", modalities: ["observe"] },         // the machine actor's settled-send fact hub
  quarantine: { name: "rasm.core.interchange.quarantine", modalities: ["observe"] }, // the codec poison divert's held-frame evidence
  support: { name: "rasm.core.interchange.support", modalities: ["observe"] },       // the gateway's delivered SupportCapture
} as const

const _point = <A, I>(source: Tap.Text | Tap.PointRow, fact: Schema.Schema<A, I>): Tap.Point<A> =>
  Predicate.isString(source)
    ? { name: _name(source), fact: Schema.typeSchema(fact), modalities: ["observe"] }
    : { name: _name(source.name), fact: Schema.typeSchema(fact), modalities: source.modalities }
```

## [04]-[RAIL_CONTRACT]

- Owner: the contract surface — `Tap.Handler`, the generic tagged handler family (`Veto` carries the pure decide, `Observe` and `Replay` carry the forked run) with its interior `WithGenerics` constructor; `subscription`, the one constructor discriminating modality on the single-key handle record; `emitter`, the telemetry-as-tap constructor mapping a fact into a metric's admitted update; `registry`, the reverse-mapped accumulated admission yielding `Tap.Registry` or the retained `TapFault`; `isolated`, the breach fold onto the `FaultClass` lattice; and the `Veto`/`Breach` evidence classes riding the owner.
- Law: telemetry-as-tap — a signal emitter is an observe subscription over domain facts: `emitter(point, metric, input)` composes `Metric.update` through the metric's own input mapping, so instrument names stay `convention` rows at the consumer, an emit call inside a domain fold is the deleted spelling, and domain code publishes facts while subscribers emit signals.
- Law: a subscription value is capability-free — handler channels are `Effect<void, unknown>` with an empty requirement tail, so requirements bake at the registration seam the way a resolver bakes context, and the registry stays pure data a runtime executes under any graph.
- Law: admission is accumulated — the registry fold retains every issue on one `TapFault` rail (a handler whose modality its point refuses), the subscriber label is the record key so a duplicate subscriber is unspellable, and a partial registry never lands; `Either` is the carrier because admission is pure.
- Law: breach isolation is a pure fold onto the class lattice — `isolated(point, label)` folds a delivery `Cause` through `FaultClass.dominant` into `Breach` evidence carrying the point, the label, the dominant class, and the pretty-rendered detail; `Option.none` lands exactly on an interruption-only cause, so a cancelled delivery never reads as failure and the publisher's channel stays `never` by construction — the runtime lifts a `Breach` into its own capture and metric emission through the floor's owners.
- Law: veto verdicts are values — `Veto` carries the refusing point and reason, the decide arm returns `Option<Veto>` so allowance is absence, and the emitting fold consumes the verdict as data; a veto that must read capability outgrew the modality and belongs to a port (`invoke`'s `AvailabilityGate` shape), never an effectful handler case.
- Growth: a new handler modality is one `Handler` case with its `_handlerModality` row and its constructor arm; a new admission axis is one `_ISSUES` row; a new breach evidence axis is one `Breach` field.
- Boundary: executed dispatch — forked observe deliveries, replay windows, the veto fold inside a publisher's step — is the runtime wave's; crash-grade capture stays `value/fault`'s `FaultCapture`, which a runtime constructs FROM a `Breach` when a breach escalates.
- Packages: `effect` (`Array`, `Cause`, `Data`, `Effect`, `Either`, `Metric`, `Option`, `Schema`, `Struct`); `../value/fault.ts` (`FaultClass`).

```typescript signature
class Veto extends Schema.Class<Veto>("Veto")({
  point: _Name,
  reason: Schema.NonEmptyString,
}) {}

class Breach extends Schema.Class<Breach>("Breach")({
  point: _Name,
  label: Schema.NonEmptyString,
  class: FaultClass.schema,
  detail: Schema.String,
}) {}

const _ISSUES = ["modality"] as const
const _Issue = Schema.Struct({
  reason: Schema.Literal(..._ISSUES),
  point: _Name,
  label: Schema.String,
})

class TapFault extends Schema.TaggedError<TapFault>()("TapFault", {
  issues: Schema.NonEmptyArray(_Issue),
}) {}

declare namespace TapFault {
  type Issue = typeof _Issue.Type
}

declare namespace Tap {
  type Modalities = typeof _modalities
  type Modality = keyof typeof _rows
  type Row = { readonly feedback: boolean; readonly pure: boolean; readonly buffered: boolean }
  type Contract = { readonly [K in Modalities[number]]: Row }
  type Name = typeof _Name.Type
  type Text = `rasm.${string}.${string}.${string}`
  type PointRow = { readonly name: Text; readonly modalities: Array.NonEmptyReadonlyArray<Modality> }
  type Point<A> = { readonly name: Name; readonly fact: Schema.Schema<A, unknown>; readonly modalities: Array.NonEmptyReadonlyArray<Modality> }
  type Handler<A> = Data.TaggedEnum<{
    Veto: { readonly decide: (fact: A) => Option.Option<Veto> }
    Observe: { readonly run: (fact: A) => Effect.Effect<void, unknown> }
    Replay: { readonly run: (fact: A) => Effect.Effect<void, unknown> }
  }>
  type Handle<A> =
    | { readonly veto: (fact: A) => Option.Option<Veto> }
    | { readonly observe: (fact: A) => Effect.Effect<void, unknown> }
    | { readonly replay: (fact: A) => Effect.Effect<void, unknown> }
  type Subscription<A> = { readonly point: Point<A>; readonly handler: Handler<A> }
  type Registry<T extends Record<string, unknown>> = {
    readonly app: AppIdentity.Key
    readonly rows: { readonly [K in keyof T]: Subscription<T[K]> }
  }
  type Shape = Types.Simplify<typeof _rows & {
    readonly Breach: typeof Breach
    readonly Veto: typeof Veto
    readonly emitter: <A, Type, In, Out>(
      point: Point<A>,
      metric: Metric.Metric<Type, In, Out>,
      input: (fact: A) => In,
    ) => Subscription<A>
    readonly isolated: (point: Name, label: string) => (cause: Cause.Cause<unknown>) => Option.Option<Breach>
    readonly modalities: Modalities
    readonly modality: <A>(handler: Handler<A>) => Modality
    readonly point: <A>(source: Text | PointRow, fact: Schema.Schema<A, unknown>) => Point<A>
    readonly points: typeof _points
    readonly registry: <T extends Record<string, unknown>>(
      app: AppIdentity.Key,
      rows: { readonly [K in keyof T]: Subscription<T[K]> },
    ) => Either.Either<Registry<T>, TapFault>
    readonly schema: typeof _Name
    readonly subscription: <A>(point: Point<A>, handle: Handle<A>) => Subscription<A>
  }>
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Modality> = K
  type _Points<T extends Record<keyof typeof _points, PointRow> = typeof _points> = T
}

interface _HandlerDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: Tap.Handler<this["A"]>
}
const _Handler = Data.taggedEnum<_HandlerDefinition>() // interior constructor: the annotation gate reaches the merged name, so only the type exports

const _handlerModality = {
  Observe: "observe",
  Replay: "replay",
  Veto: "veto",
} as const satisfies Record<Tap.Handler<unknown>["_tag"], Tap.Modality>

const _subscription = <A>(point: Tap.Point<A>, handle: Tap.Handle<A>): Tap.Subscription<A> => ({
  point,
  handler: Predicate.hasProperty(handle, "veto")
    ? _Handler.Veto({ decide: handle.veto })
    : Predicate.hasProperty(handle, "observe")
      ? _Handler.Observe({ run: handle.observe })
      : _Handler.Replay({ run: handle.replay }),
})

const _emitter = <A, Type, In, Out>(
  point: Tap.Point<A>,
  metric: Metric.Metric<Type, In, Out>,
  input: (fact: A) => In,
): Tap.Subscription<A> => ({ point, handler: _Handler.Observe({ run: (fact) => Metric.update(metric, input(fact)) }) })

const _registry = <T extends Record<string, unknown>>(
  app: AppIdentity.Key,
  rows: { readonly [K in keyof T]: Tap.Subscription<T[K]> },
): Either.Either<Tap.Registry<T>, TapFault> => {
  const issues = Array.filterMap(Struct.keys(rows), (label) => {
    const row = rows[label]
    return Array.contains(row.point.modalities, _handlerModality[row.handler._tag])
      ? Option.none<TapFault.Issue>()
      : Option.some({ reason: "modality" as const, point: row.point.name, label })
  })
  return Array.isNonEmptyReadonlyArray(issues)
    ? Either.left(new TapFault({ issues }))
    : Either.right({ app, rows })
}

const _isolated = (point: Tap.Name, label: string) => (cause: Cause.Cause<unknown>): Option.Option<Breach> =>
  Option.map(FaultClass.dominant(cause), (kind) =>
    new Breach({ point, label, class: kind, detail: Cause.pretty(cause) }))

const Tap: Tap.Shape = {
  ..._rows,
  Breach,
  Veto,
  emitter: _emitter,
  isolated: _isolated,
  modalities: _modalities,
  modality: (handler) => _handlerModality[handler._tag],
  point: _point,
  points: _points,
  registry: _registry,
  schema: _Name,
  subscription: _subscription,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Tap, TapFault }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
