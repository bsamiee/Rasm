# [CORE_TAP]

`Tap` owns point names, modality-split handlers, app-scoped registries, the one delivery rail registrars seat on, and the accounted breach ledger.

`Tap.Rail` is the branch's single hook mechanism: one accounted channel per point minted from that point's own depth, composition-unique seating whose verdict carries the release token, and one census counting both loss halves. Registrars — `runtime/otel/emit#HOOKS`, `ui/system/hook`, `security/access/audit`, `data/journal/append#HOOK_RAIL` — own their point rosters, payload types, and policy rows; none owns delivery.

## [01]-[INDEX]

- [02]-[MODALITY_TABLE]: closed modality tuple, dispatch rows, and the feedback/purity/buffer axes; `Tap` (modality reads).
- [03]-[POINT_VOCABULARY]: point-name brand, retention depth column, and the fact-binding point mint; `Tap` (point mint).
- [04]-[RAIL_CONTRACT]: `Tap` subscriptions, registry admission, breach isolation, and the accumulating admission fault.
- [05]-[DELIVERY_RAIL]: scoped rail, accounted rings, mount and release verdicts, publish verdicts, and the breach ledger.

## [02]-[MODALITY_TABLE]

- Owner: `Shape.vocabulary` owns ordered modalities and dispatch rows; feedback, purity, and buffering are row columns.
- Law: executors dispatch from modality columns; a new modality is one tuple row, never a name switch.
- Law: veto is pure feedback through `(fact) => Option<Tap.Veto>` and never opens a capability or exporter.
- Law: observe delivery forks, and subscriber faults become isolated breaches rather than publisher failures.
- Law: `buffered` decides retention alone — that modality spends the point's declared depth as replay window, every other as capacity with none.
- Growth: a new modality is one tuple entry with its row; a new execution axis is one `Row` field with its column on each row.
- Boundary: `Tap` owns delivery whole — channels, seating, arbitration, isolation, accounting; a registrar owns its roster, payloads, and policy rows.
- Packages: `effect`, `@effect/typeclass`, `Identity`, `Fault`, `Shape`, and `Convention`.

```typescript signature
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as NumberInstances from "@effect/typeclass/data/Number"
import {
  Array, Cause, Data, Effect, Either, FiberSet, HashMap, Metric, Option, Predicate, PubSub, Record, Ref, Schema,
  type Scope, Stream, Struct, type Types, pipe,
} from "effect"
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

- Owner: `Tap.point` admits a branded name, a unique nonempty modality set, a positive depth, and the fact schema on one `Either` rail.
- Law: `Tap.point` binds a producer-owned fact schema to name-modality-depth data and stores only its decoded type side.
- Law: registries key rails by `Identity.App.Key`; `Identity.Tenant` remains outside hook identity.
- Law: declaration-time point admission proves the name, unique modalities, and a positive depth; observe is the default modality.
- Law: `depth` is the point's own channel width and `_retained` projects it into the replay window where `buffered` reads true.
- Law: retention is a declared point property — no rail policy overrides it and no registrar re-derives it.
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
const _Depth = Schema.Int.pipe(Schema.positive(), Schema.brand("TapDepth"))
const _PointRow = Schema.Struct({ name: _Name, modalities: _Modalities, depth: _Depth })

const _DEFAULT = { depth: 32, modalities: ["observe"] } as const

const _retained = (point: Tap.Rostered): number =>
  Array.some(point.modalities, (modality) => _Modality.at(modality).buffered) ? point.depth : 0
```

## [04]-[RAIL_CONTRACT]

- Owner: `Tap.Handler`, `Tap.subscription`, `Tap.emitter`, `Tap.registry`, `Tap.isolated`, `Tap.Veto`, and `Tap.Breach`.
- Law: `Tap.emitter` mounts one Convention row and derives its update input and frequency words from that row.
- Law: Tap may consume Convention names, but Convention never depends on Tap.
- Law: effectful handlers require an explicit error type and no environment; omitted errors normalize to `Tap.Fault`.
- Law: `Tap.registry` accumulates modality issues on one `Tap.Fault` rail and never publishes a partial registry.
- Law: two legs partition the census — `admission` refuses a bad point row or unadmitted handler, `seating` refuses duplicate and foreign rows.
- Law: each reason renders its own declared subject — a shared free-string detail re-opens the axis `reason` closed.
- Law: `Tap.isolated` folds a delivery cause through `Fault.Class.dominant`; interruption-only causes produce no breach.
- Law: veto handlers return `Option<Tap.Veto>` as pure feedback; observe and replay handlers return effects.
- Growth: a new modality is one `Tap.Modality` vocabulary row and one `Tap.Handler` case.
- Boundary: `Fault.Capture` remains the forensic owner for escalated `Tap.Breach` evidence.
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

const _family = Fault.Class.family(["duplicate", "foreign", "modality", "point", "unrostered"] as const, {
  duplicate: Fault.Class.row({
    class: "conflicted",
    leg: "seating",
    detail: Schema.Struct({ point: _Name, label: Schema.String }),
    render: (subject) => `${subject.point}@${subject.label} already seated`,
  }),
  foreign: Fault.Class.row({
    class: "denied",
    leg: "seating",
    detail: Schema.Struct({ app: Schema.NonEmptyString }),
    render: (subject) => `registry app ${subject.app} offered to another app's rail`,
  }),
  modality: Fault.Class.row({
    class: "invalid",
    leg: "admission",
    detail: Schema.Struct({
      point: _Name,
      label: Schema.String,
      expected: Schema.NonEmptyArray(_Modality.schema),
      actual: _Modality.schema,
    }),
    render: (subject) =>
      `${subject.point}@${subject.label} handles ${subject.actual}, point admits ${Array.join(subject.expected, "|")}`,
  }),
  point: Fault.Class.row({
    class: "invalid",
    leg: "admission",
    detail: Schema.Struct({ detail: Schema.String }),
    render: (subject) => `point row refused: ${subject.detail}`,
  }),
  unrostered: Fault.Class.row({
    class: "absent",
    leg: "seating",
    detail: Schema.Struct({ point: _Name, label: Schema.String }),
    render: (subject) => `${subject.point}@${subject.label} names no rail slot`,
  }),
})

const _Fault = _family.census("Tap.Fault")

type _Issue = Schema.Schema.Type<typeof _family.payload>

declare namespace Tap {
  type Breach = InstanceType<typeof _Breach>
  type Veto = InstanceType<typeof _Veto>
  type Fault = InstanceType<typeof _Fault>
  type Modalities = typeof _Modality.kinds
  type Modality = Modalities[number]
  type Row = { readonly feedback: boolean; readonly pure: boolean; readonly buffered: boolean }
  type Contract = { readonly [K in Modalities[number]]: Row }
  type Name = typeof _Name.Type
  type Depth = typeof _Depth.Type
  type Text = `rasm.${string}.${string}.${string}`
  type PointRow = {
    readonly name: Text
    readonly modalities: Array.NonEmptyReadonlyArray<Modality>
    readonly depth: number
  }
  type Rostered = {
    readonly name: Name
    readonly modalities: Array.NonEmptyReadonlyArray<Modality>
    readonly depth: Depth
  }
  type Point<A> = Rostered & { readonly fact: Schema.Schema<A, unknown> }
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
    Schema.decodeUnknownEither(_PointRow)(Predicate.isString(source) ? { name: source, ..._DEFAULT } : source),
    Either.map((row) => ({ ...row, fact: Schema.typeSchema(fact) })),
    Either.mapLeft((issue) => new _Fault({ issues: [{ reason: "point", detail: String(issue) }] })),
  )

const _emitter = <N extends Convention.MetricName, A, const W extends Convention.Roster>(
  point: Tap.Point<A>,
  metric: N,
  input: (fact: A) => Convention.Input<N>,
  ...words: Convention.Words<N, W>
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
      ? Option.none<_Issue>()
      : Option.some({ reason: "modality" as const, point: row.point.name, label, expected: row.point.modalities, actual: row.handler._tag })
  })
  return Array.isNonEmptyReadonlyArray(issues)
    ? Either.left(new _Fault({ issues }))
    : Either.right({ app, rows })
}

const _isolated = <E>(point: Tap.Name, label: string) => (cause: Cause.Cause<E>): Option.Option<Tap.Breach> =>
  Option.map(Fault.Class.dominant(cause), (kind) =>
    new _Breach({ point, label, class: kind, detail: Cause.pretty(cause) }))
```

## [05]-[DELIVERY_RAIL]

- Owner: `Tap.rail` mints the plane — a `Tap.Slot` per point, the `Tap.Ledger` ring, the seating tally — under `mount`, `release`, `publish`.
- Law: one constructor serves every channel — `PubSub.dropping({ capacity, replay })` over the point's depth and the retention `_retained` projects.
- Law: saturation refuses the newest fact and answers `false`; `bounded` suspends the publishing fold and `sliding` evicts unaccounted.
- Law: `shed` counts a fact the channel refused at admission, read off `PubSub.publish`'s answer and never a discarded boolean.
- Law: `lost` counts a retained fact the window displaced, derived as admissions past the retention; a windowless ring reports a structural zero.
- Law: `_charged` is the one accounting fold over `Tap.Ring`, so a point's fact hub and the breach ledger keep identical columns under identical law.
- Law: seating is composition-unique on `(point, label)` and admission accumulates before any seat lands.
- Law: one refused row refuses the WHOLE mount — a partial mount double-delivers every row seated ahead of the refusal.
- Law: `Tap.mount` answers a verdict, never a unit — the `mounted` arm carries the handle that IS the release token.
- Law: a duplicate registration reads as a typed refusal, never a silent second subscription double-counting every emitter.
- Law: release brackets the acquisition for every modality alike — `Effect.acquireRelease` binds each mount's `FiberSet` and seats to its scope.
- Law: `Tap.release` runs that same effect mid-life, answering seats dropped, fibers interrupted, and the summed census of every point it touched.
- Law: replay needs no journal — a fresh subscription drains the channel's retained window before live facts, so no rail holds a second history.
- Law: `Tap.publish` answers a closed verdict carrying arbitration, delivering arity, and census; an unrostered point is its own case.
- Law: the veto fold runs before any admission and first refusal wins — a vetoed fact charges the census without reaching the channel.
- Law: teardown shuts each channel down rather than flushing — a tap fact is a live signal the plane already sheds, never a durable row.
- Entry: the composition root mints one rail per app inside the app scope and hands it to every registrar.
- Growth: a new tally is one `Tap.Census` or `Tap.Seating` column with its `SemigroupSum` row; a new verdict case is one arm every `$match` breaks on.
- Boundary: instrument rows and dimension names are `observe/convention`'s; this cluster owns the counts and publishes them as one `Tap.Report` read.
- Packages: `effect` (`Data`, `FiberSet`, `HashMap`, `PubSub`, `Ref`, `Scope`, `Stream`); `@effect/typeclass` (`Monoid`, `Semigroup`, `data/Number`).

```typescript signature
declare namespace Tap {
  type Policy = { readonly ledger: number }
  type Census = { readonly admitted: number; readonly lost: number; readonly shed: number; readonly vetoed: number }
  type Seating = { readonly mounted: number; readonly refused: number; readonly released: number }
  type Ring<A> = {
    readonly census: Ref.Ref<Census>
    readonly hub: PubSub.PubSub<A>
    readonly replay: number
  }
  type Seat = {
    readonly arbiter: Option.Option<(fact: unknown) => Option.Option<Veto>>
    readonly label: string
    readonly modality: Modality
    readonly point: Name
  }
  type Slot = Ring<unknown> & { readonly seats: Ref.Ref<HashMap.HashMap<string, Seat>> }
  type Resolved = { readonly label: string; readonly slot: Slot; readonly sub: Subscription<unknown, unknown> }
  type Ledger = Ring<Breach>
  type Rail = {
    readonly app: Identity.App.Key
    readonly ledger: Ledger
    readonly seating: Ref.Ref<Seating>
    readonly slots: HashMap.HashMap<Name, Slot>
  }
  type Mounted = {
    readonly app: Identity.App.Key
    readonly release: Effect.Effect<Released>
    readonly seats: ReadonlyArray<Seat>
  }
  type Released = {
    readonly app: Identity.App.Key
    readonly census: Census
    readonly fibers: number
    readonly seats: number
  }
  type Mount = Data.TaggedEnum<{
    mounted: { readonly handle: Mounted }
    refused: { readonly fault: Fault }
  }>
  type Verdict = Data.TaggedEnum<{
    fanned: { readonly arity: number; readonly census: Census; readonly point: Name }
    unrostered: { readonly point: Name }
    vetoed: { readonly census: Census; readonly point: Name; readonly veto: Veto }
  }>
  type Report = {
    readonly ledger: Census
    readonly points: ReadonlyArray<readonly [Name, Census]>
    readonly seating: Seating
  }
  type Shape = Types.Simplify<{
    readonly Breach: typeof _Breach
    readonly Fault: typeof _Fault
    readonly Handler: typeof _Handler
    readonly Modality: typeof _Modality
    readonly Mount: typeof _Mount
    readonly Veto: typeof _Veto
    readonly Verdict: typeof _Verdict
    readonly breaches: (rail: Rail) => Stream.Stream<Breach>
    readonly census: (rail: Rail) => Effect.Effect<Report>
    readonly emitter: <N extends Convention.MetricName, A, const W extends Convention.Roster>(
      point: Point<A>,
      metric: N,
      input: (fact: A) => Convention.Input<N>,
      ...words: Convention.Words<N, W>
    ) => Subscription<A, never>
    readonly isolated: <E>(point: Name, label: string) => (cause: Cause.Cause<E>) => Option.Option<Breach>
    readonly modality: <A, E>(handler: Handler<A, E>) => Modality
    readonly mount: <T extends Record<string, unknown>, E extends { readonly [K in keyof T]: unknown }>(
      rail: Rail,
      registry: Registry<T, E>,
    ) => Effect.Effect<Mount, never, Scope.Scope>
    readonly point: <A, I>(source: Text | PointRow, fact: Schema.Schema<A, I>) => Either.Either<Point<A>, _Fault>
    readonly publish: <A>(rail: Rail, point: Point<A>, fact: A) => Effect.Effect<Verdict>
    readonly rail: (
      app: Identity.App.Key,
      points: ReadonlyArray<Rostered>,
      policy: Policy,
    ) => Effect.Effect<Rail, never, Scope.Scope>
    readonly registry: <T extends Record<string, unknown>, E extends { readonly [K in keyof T]: unknown }>(
      app: Identity.App.Key,
      rows: { readonly [K in keyof T]: Subscription<T[K], E[K]> },
    ) => Either.Either<Registry<T, E>, _Fault>
    readonly release: (mounted: Mounted) => Effect.Effect<Released>
    readonly schema: typeof _Name
    readonly subscription: <A, E>(point: Point<A>, handler: Handler<A, E>) => Subscription<A, E>
  }>
}

const _Mount = Data.taggedEnum<Tap.Mount>()
const _Verdict = Data.taggedEnum<Tap.Verdict>()

const _Census: Monoid.Monoid<Tap.Census> = Monoid.fromSemigroup(
  Semigroup.struct({
    admitted: NumberInstances.SemigroupSum,
    lost: NumberInstances.SemigroupSum,
    shed: NumberInstances.SemigroupSum,
    vetoed: NumberInstances.SemigroupSum,
  }),
  { admitted: 0, lost: 0, shed: 0, vetoed: 0 },
)

const _Seating: Monoid.Monoid<Tap.Seating> = Monoid.fromSemigroup(
  Semigroup.struct({
    mounted: NumberInstances.SemigroupSum,
    refused: NumberInstances.SemigroupSum,
    released: NumberInstances.SemigroupSum,
  }),
  { mounted: 0, refused: 0, released: 0 },
)

const _ring = <A>(capacity: number, replay: number): Effect.Effect<Tap.Ring<A>> =>
  Effect.map(
    Effect.all({ census: Ref.make(_Census.empty), hub: PubSub.dropping<A>({ capacity, replay }) }),
    ({ census, hub }) => ({ census, hub, replay }),
  )

const _charged = <A>(ring: Tap.Ring<A>, cell: A): Effect.Effect<Tap.Census> =>
  Effect.flatMap(PubSub.publish(ring.hub, cell), (admitted) =>
    Ref.updateAndGet(ring.census, (held) => ({
      ...held,
      admitted: held.admitted + (admitted ? 1 : 0),
      lost: held.lost + (admitted && ring.replay > 0 && held.admitted >= ring.replay ? 1 : 0),
      shed: held.shed + (admitted ? 0 : 1),
    })))

const _arbitrated = (seats: HashMap.HashMap<string, Tap.Seat>, fact: unknown): Option.Option<Tap.Veto> =>
  Array.reduce(
    Array.filterMap(HashMap.values(seats), (seat) => seat.arbiter),
    Option.none<Tap.Veto>(),
    (held, gate) => Option.orElse(held, () => gate(fact)),
  )

const _arity = (seats: HashMap.HashMap<string, Tap.Seat>): number =>
  HashMap.size(HashMap.filter(seats, (seat) => !_Modality.at(seat.modality).feedback))

const _rail = (
  app: Identity.App.Key,
  points: ReadonlyArray<Tap.Rostered>,
  policy: Tap.Policy,
): Effect.Effect<Tap.Rail, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const slots = yield* Effect.forEach(points, (point) =>
        Effect.map(
          Effect.all({
            ring: _ring<unknown>(point.depth, _retained(point)),
            seats: Ref.make(HashMap.empty<string, Tap.Seat>()),
          }),
          ({ ring, seats }) => [point.name, { ...ring, seats }] as const,
        ))
      return {
        app,
        ledger: yield* _ring<Tap.Breach>(policy.ledger, policy.ledger),
        seating: yield* Ref.make(_Seating.empty),
        slots: HashMap.fromIterable(slots),
      }
    }),
    (rail) =>
      Effect.zipRight(
        Effect.forEach(HashMap.values(rail.slots), (slot) => PubSub.shutdown(slot.hub), { discard: true }),
        PubSub.shutdown(rail.ledger.hub),
      ),
  )

const _admitted = (
  rail: Tap.Rail,
  label: string,
  sub: Tap.Subscription<unknown, unknown>,
): Effect.Effect<Either.Either<Tap.Resolved, _Issue>> =>
  Option.match(HashMap.get(rail.slots, sub.point.name), {
    onNone: () => Effect.succeed(Either.left<_Issue>({ reason: "unrostered", point: sub.point.name, label })),
    onSome: (slot) =>
      Effect.map(Ref.get(slot.seats), (seats) =>
        HashMap.has(seats, label)
          ? Either.left<_Issue>({ reason: "duplicate", point: sub.point.name, label })
          : Either.right<Tap.Resolved>({ label, slot, sub })),
  })

const _seat = (
  rail: Tap.Rail,
  fibers: FiberSet.FiberSet<void>,
  row: Tap.Resolved,
): Effect.Effect<Tap.Seat> =>
  Effect.gen(function* () {
    const { handler, point } = row.sub
    const seat: Tap.Seat = {
      arbiter: handler._tag === "veto" ? Option.some(handler.handle) : Option.none(),
      label: row.label,
      modality: handler._tag,
      point: point.name,
    }
    yield* handler._tag === "veto"
      ? Effect.void
      : Effect.asVoid(FiberSet.run(
        fibers,
        Stream.runForEach(Stream.fromPubSub(row.slot.hub), (fact) =>
          Effect.catchAllCause(handler.handle(fact), (cause) =>
            Option.match(_isolated(point.name, row.label)(cause), {
              onNone: () => Effect.void,
              onSome: (breach) => Effect.asVoid(_charged(rail.ledger, breach)),
            }))),
      ))
    yield* Ref.update(row.slot.seats, HashMap.set(row.label, seat))
    return seat
  })

const _released = (rail: Tap.Rail, mounted: Omit<Tap.Mounted, "release">, fibers: FiberSet.FiberSet<void>): Effect.Effect<Tap.Released> =>
  Effect.gen(function* () {
    const running = yield* FiberSet.size(fibers)
    yield* FiberSet.clear(fibers)
    yield* Effect.forEach(mounted.seats, (seat) =>
      Option.match(HashMap.get(rail.slots, seat.point), {
        onNone: () => Effect.void,
        onSome: (slot) => Ref.update(slot.seats, HashMap.remove(seat.label)),
      }), { discard: true })
    const census = _Census.combineAll(
      yield* Effect.forEach(Array.dedupe(Array.map(mounted.seats, (seat) => seat.point)), (point) =>
        Option.match(HashMap.get(rail.slots, point), {
          onNone: () => Effect.succeed(_Census.empty),
          onSome: (slot) => Ref.get(slot.census),
        })),
    )
    yield* Ref.update(rail.seating, (held) => ({ ...held, released: held.released + 1 }))
    return { app: mounted.app, census, fibers: running, seats: Array.length(mounted.seats) }
  })

const _seated = (
  rail: Tap.Rail,
  app: Identity.App.Key,
  rows: ReadonlyArray<Tap.Resolved>,
): Effect.Effect<Tap.Mounted, never, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const fibers = yield* FiberSet.make<void>()
      const handle = { app, seats: yield* Effect.forEach(rows, (row) => _seat(rail, fibers, row)) }
      yield* Ref.update(rail.seating, (held) => ({ ...held, mounted: held.mounted + 1 }))
      return { ...handle, release: _released(rail, handle, fibers) }
    }),
    (mounted) => mounted.release,
  )

const _mount = <T extends Record<string, unknown>, E extends { readonly [K in keyof T]: unknown }>(
  rail: Tap.Rail,
  registry: Tap.Registry<T, E>,
): Effect.Effect<Tap.Mount, never, Scope.Scope> =>
  Effect.gen(function* () {
    const rows = Record.toEntries(registry.rows) as ReadonlyArray<readonly [string, Tap.Subscription<unknown, unknown>]>
    const held: ReadonlyArray<Either.Either<Tap.Resolved, _Issue>> = registry.app === rail.app
      ? yield* Effect.forEach(rows, ([label, sub]) => _admitted(rail, label, sub))
      : [Either.left<_Issue>({ reason: "foreign", app: registry.app })]
    const issues = Array.getLefts(held)
    if (Array.isNonEmptyReadonlyArray(issues)) {
      yield* Ref.update(rail.seating, (row) => ({ ...row, refused: row.refused + 1 }))
      return _Mount.refused({ fault: new _Fault({ issues }) })
    }
    return _Mount.mounted({ handle: yield* _seated(rail, registry.app, Array.getRights(held)) })
  })

const _publish = <A>(rail: Tap.Rail, point: Tap.Point<A>, fact: A): Effect.Effect<Tap.Verdict> =>
  Option.match(HashMap.get(rail.slots, point.name), {
    onNone: () => Effect.succeed(_Verdict.unrostered({ point: point.name })),
    onSome: (slot) =>
      Effect.flatMap(Ref.get(slot.seats), (seats) =>
        Option.match(_arbitrated(seats, fact), {
          onNone: () =>
            Effect.map(_charged(slot, fact), (census) => _Verdict.fanned({ arity: _arity(seats), census, point: point.name })),
          onSome: (veto) =>
            Effect.map(
              Ref.updateAndGet(slot.census, (held) => ({ ...held, vetoed: held.vetoed + 1 })),
              (census) => _Verdict.vetoed({ census, point: point.name, veto }),
            ),
        })),
  })

const _census = (rail: Tap.Rail): Effect.Effect<Tap.Report> =>
  Effect.all({
    ledger: Ref.get(rail.ledger.census),
    points: Effect.forEach(
      HashMap.toEntries(rail.slots),
      ([point, slot]) => Effect.map(Ref.get(slot.census), (census) => [point, census] as const),
    ),
    seating: Ref.get(rail.seating),
  })

const Tap: Tap.Shape = {
  Breach: _Breach,
  Fault: _Fault,
  Handler: _Handler,
  Modality: _Modality,
  Mount: _Mount,
  Veto: _Veto,
  Verdict: _Verdict,
  breaches: (rail) => Stream.fromPubSub(rail.ledger.hub),
  census: _census,
  emitter: _emitter,
  isolated: _isolated,
  modality: (handler) => handler._tag,
  mount: _mount,
  point: _point,
  publish: _publish,
  rail: _rail,
  registry: _registry,
  release: (mounted) => mounted.release,
  schema: _Name,
  subscription: _subscription,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tap }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
