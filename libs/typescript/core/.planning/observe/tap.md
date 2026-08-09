# [CORE_TAP]

`Tap` owns admitted point names, modality-discriminated handlers, app-scoped registries, and isolated subscriber breaches.

## [01]-[INDEX]

- [02]-[MODALITY_TABLE]: the closed modality tuple, the dispatch rows, the feedback/purity/buffer axes; `Tap` (modality reads).
- [03]-[POINT_VOCABULARY]: the point-name brand, the standing point rows, the fact-binding point mint; `Tap` (point mint).
- [04]-[RAIL_CONTRACT]: `Tap` subscriptions, registry admission, breach isolation, and veto evidence.

## [02]-[MODALITY_TABLE]

- Owner: `Shape.vocabulary` owns ordered modalities and dispatch rows; feedback, purity, and buffering are row columns.
- Law: executors dispatch from modality columns; a new modality is one tuple row, never a name switch.
- Law: veto is pure feedback through `(fact) => Option<Tap.Veto>` and never opens a capability or exporter.
- Law: observe delivery forks, and subscriber faults become isolated breaches rather than publisher failures.
- Law: replay adds the publisher's bounded retention window; a point without retention refuses replay.
- Growth: a new modality is one tuple entry with its row; a new execution axis is one `Row` field with its column on each row.
- Boundary: fibers, scheduling, and delivery mechanics are the runtime wave's executor — this table is the data it reads.
- Packages: `effect`, `Identity`, `Fault`, `Shape`, and `Convention`.

```typescript signature
import { Array, Cause, Data, type Effect, Either, Metric, Option, Predicate, Schema, Struct, type Types, pipe } from "effect"
import { Fault } from "../value/fault.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Convention } from "./convention.ts"

const _modalities = ["veto", "observe", "replay"] as const

const _rows = {
  veto: { feedback: true, pure: true, buffered: false },
  observe: { feedback: false, pure: false, buffered: false },
  replay: { feedback: false, pure: false, buffered: true },
} as const
const _Modality = Shape.vocabulary(_modalities, _rows)
```

## [03]-[POINT_VOCABULARY]

- Owner: `Tap.point` admits a branded name, a nonempty unique modality set, and the producer's fact schema on one `Either` rail.
- Law: `Tap.point` binds a producer-owned fact schema to name-and-modality data and stores only its decoded type side.
- Law: registries key rails by `Identity.App.Key`; `Identity.Tenant` remains outside hook identity.
- Law: declaration-time point admission proves the name and unique modalities; observe is the default modality.
- Growth: a new hook point is one folder-owned `Tap.point` declaration; a new modality widens that declaration's admitted set.
- Boundary: publishers own point declarations and publication; Convention owns attribute, metric, and event names.
- Packages: `effect`, `Identity`, and `Shape`.

```typescript signature
const _Name = Schema.String.pipe(
  Schema.pattern(/^rasm\.[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*$/),
  Schema.brand("TapPoint"),
)
const _Modalities = Schema.NonEmptyArray(_Modality.schema).pipe(
  Schema.filter((modalities) => Array.dedupe(modalities).length === modalities.length || "<duplicate-tap-modality>"),
)
const _PointRow = Schema.Struct({ name: _Name, modalities: _Modalities })
```

## [04]-[RAIL_CONTRACT]

- Owner: `Tap.Handler`, `Tap.subscription`, `Tap.emitter`, `Tap.registry`, `Tap.isolated`, `Tap.Veto`, and `Tap.Breach`.
- Law: `Tap.emitter` mounts one Convention row and derives its update input and frequency words from that row.
- Law: Tap may consume Convention names, but Convention never depends on Tap.
- Law: effectful handlers require an explicit error type and no environment; omitted errors normalize to `Tap.Fault`.
- Law: `Tap.registry` accumulates modality issues on one `Tap.Fault` rail and never publishes a partial registry.
- Law: `Tap.isolated` folds a delivery cause through `Fault.Class.dominant`; interruption-only causes produce no breach.
- Law: veto handlers return `Option<Tap.Veto>` as pure feedback; observe and replay handlers return effects.
- Growth: a new modality is one `Tap.Modality` vocabulary row and one `Tap.Handler` case.
- Boundary: runtime executes delivery; `Fault.Capture` remains the forensic owner for escalated `Tap.Breach` evidence.
- Packages: `effect`, `Fault`, `Identity`, and `Convention`.

```typescript signature
class _Veto extends Schema.Class<_Veto>("Veto")({
  point: _Name,
  reason: Schema.NonEmptyString,
}) {}

class _Breach extends Schema.Class<_Breach>("Breach")({
  point: _Name,
  label: Schema.NonEmptyString,
  class: Fault.Class.schema,
  detail: Schema.String,
}) {}

const _family = Fault.Class.family(["modality", "point"] as const, {
  modality: { class: "invalid" },
  point: { class: "invalid" },
})

const _Issue = Schema.Union(Schema.Struct({
  reason: Schema.Literal("modality"),
  point: _Name,
  label: Schema.String,
  expected: Schema.NonEmptyArray(_Modality.schema),
  actual: _Modality.schema,
}), Schema.Struct({ reason: Schema.Literal("point"), detail: Schema.String }))

class _Fault extends Schema.TaggedError<_Fault>()("Tap.Fault", {
  issues: Schema.NonEmptyArray(_Issue),
}) {
  get class(): Fault.Class.Kind {
    return Fault.Class.dominant(Array.map(this.issues, (issue) => _family.classOf(issue.reason)))
  }
  override get message(): string {
    return `<tap:refused> ${Array.join(Array.map(this.issues, (issue) =>
      issue.reason === "modality" ? `${issue.reason}@${issue.label}` : `${issue.reason}:${issue.detail}`), ",")}`
  }
}

declare namespace _Fault {
  type Issue = typeof _Issue.Type
}

declare namespace Tap {
  type Breach = InstanceType<typeof _Breach>
  type Veto = InstanceType<typeof _Veto>
  type Fault = InstanceType<typeof _Fault>
  type Modalities = typeof _Modality.kinds
  type Modality = Modalities[number]
  type Row = { readonly feedback: boolean; readonly pure: boolean; readonly buffered: boolean }
  type Contract = { readonly [K in Modalities[number]]: Row }
  type Name = typeof _Name.Type
  type Text = `rasm.${string}.${string}.${string}`
  type PointRow = { readonly name: Text; readonly modalities: Array.NonEmptyReadonlyArray<Modality> }
  type Point<A> = { readonly name: Name; readonly fact: Schema.Schema<A, unknown>; readonly modalities: Array.NonEmptyReadonlyArray<Modality> }
  type Handler<A, E = Fault> = Data.TaggedEnum<{
    veto: { readonly handle: (fact: A) => Option.Option<Veto> }
    observe: { readonly handle: (fact: A) => Effect.Effect<void, E> }
    replay: { readonly handle: (fact: A) => Effect.Effect<void, E> }
  }>
  type Subscription<A, E = Fault> = { readonly point: Point<A>; readonly handler: Handler<A, E> }
  type Registry<T extends Record<string, unknown>, E extends { readonly [K in keyof T]: unknown }> = {
    readonly app: Identity.App.Key
    readonly rows: { readonly [K in keyof T]: Subscription<T[K], E[K]> }
  }
  type Shape = Types.Simplify<{
    readonly Breach: typeof _Breach
    readonly Fault: typeof _Fault
    readonly Handler: typeof _Handler
    readonly Modality: typeof _Modality
    readonly Veto: typeof _Veto
    readonly emitter: <N extends Convention.MetricName, A>(
      point: Point<A>,
      metric: N,
      input: (fact: A) => Convention.Input<N>,
      ...words: Convention.Words<N>
    ) => Subscription<A, never>
    readonly isolated: <E>(point: Name, label: string) => (cause: Cause.Cause<E>) => Option.Option<Breach>
    readonly modality: <A, E>(handler: Handler<A, E>) => Modality
    readonly point: <A, I>(source: Text | PointRow, fact: Schema.Schema<A, I>) => Either.Either<Point<A>, _Fault>
    readonly registry: <T extends Record<string, unknown>, E extends { readonly [K in keyof T]: unknown }>(
      app: Identity.App.Key,
      rows: { readonly [K in keyof T]: Subscription<T[K], E[K]> },
    ) => Either.Either<Registry<T, E>, _Fault>
    readonly schema: typeof _Name
    readonly subscription: <A, E>(point: Point<A>, handler: Handler<A, E>) => Subscription<A, E>
  }>
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Modality> = K
}

interface _HandlerDefinition extends Data.TaggedEnum.WithGenerics<2> {
  readonly taggedEnum: Tap.Handler<this["A"], this["B"]>
}
const _Handler = Data.taggedEnum<_HandlerDefinition>()

const _subscription = <A, E>(point: Tap.Point<A>, handler: Tap.Handler<A, E>): Tap.Subscription<A, E> => ({ point, handler })

const _point = <A, I>(source: Tap.Text | Tap.PointRow, fact: Schema.Schema<A, I>): Either.Either<Tap.Point<A>, _Fault> =>
  pipe(
    Schema.decodeUnknownEither(_PointRow)(Predicate.isString(source) ? { name: source, modalities: ["observe"] } : source),
    Either.map((row) => ({ ...row, fact: Schema.typeSchema(fact) })),
    Either.mapLeft((issue) => new _Fault({ issues: [{ reason: "point", detail: String(issue) }] })),
  )

const _emitter = <N extends Convention.MetricName, A>(
  point: Tap.Point<A>,
  metric: N,
  input: (fact: A) => Convention.Input<N>,
  ...words: Convention.Words<N>
): Tap.Subscription<A, never> => {
  const mounted = Convention.mount(metric, ...words)
  return { point, handler: _Handler.observe({ handle: (fact) => Metric.update(mounted, input(fact)) }) }
}

const _registry = <T extends Record<string, unknown>, E extends { readonly [K in keyof T]: unknown }>(
  app: Identity.App.Key,
  rows: { readonly [K in keyof T]: Tap.Subscription<T[K], E[K]> },
): Either.Either<Tap.Registry<T, E>, _Fault> => {
  const issues = Array.filterMap(Struct.keys(rows), (label) => {
    const row = rows[label]
    return Array.contains(row.point.modalities, row.handler._tag)
      ? Option.none<_Fault.Issue>()
      : Option.some({ reason: "modality" as const, point: row.point.name, label, expected: row.point.modalities, actual: row.handler._tag })
  })
  return Array.isNonEmptyReadonlyArray(issues)
    ? Either.left(new _Fault({ issues }))
    : Either.right({ app, rows })
}

const _isolated = <E>(point: Tap.Name, label: string) => (cause: Cause.Cause<E>): Option.Option<Tap.Breach> =>
  Option.map(Fault.Class.dominant(cause), (kind) =>
    new _Breach({ point, label, class: kind, detail: Cause.pretty(cause) }))

const Tap: Tap.Shape = {
  Breach: _Breach,
  Fault: _Fault,
  Handler: _Handler,
  Modality: _Modality,
  Veto: _Veto,
  emitter: _emitter,
  isolated: _isolated,
  modality: (handler) => handler._tag,
  point: _point,
  registry: _registry,
  schema: _Name,
  subscription: _subscription,
}

export { Tap }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
