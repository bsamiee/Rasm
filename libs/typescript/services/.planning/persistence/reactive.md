# [SERVICES_REACTIVE]

The read-side reactive-query owner — `ReactiveQuery`, the single `SqlClient.reactive([keys], query)` surface bound over the `@effect/experimental` `Reactivity` key-scoped invalidation substrate, so a write invalidates exactly the touched query keys and every dependent read re-runs as a `Stream` rather than a poll, a timer, or a parallel change-feed. It is the read-side sibling of `folder:persistence/store#STORE_BOUNDARY` and `folder:persistence/work#WORK_AND_SIGNALS`: where those own the write tier and the durable signalling tables, this owns the push-on-key edge over the same `PgClient`. The `Reactivity.Reactivity` requirement is already discharged by the `@effect/sql-pg` `PgClient.make` layer through its bundled default, so the substrate is mined directly — `Reactivity` is the SECOND first-class `@effect/experimental` direct consumer beside the `folder:agent/mcp#MCP_TRANSPORT` `Sse` decode, not a transitive plumbing detail. Invalidation keys are entity-scoped rows, never per-query handlers, so a new reactive read is one `reactive([keys], query)` row keyed by entity. The owner rides the one `PgClient` and crosses no .NET wire.

## [1]-[INDEX]

- [1]-[REACTIVE_QUERY]: owns the `ReactiveQuery` `SqlClient.reactive`/`reactiveMailbox` surface, the entity-keyed `InvalidationKey` row table, the `Reactivity.mutation` key-publish edge writers wrap, and the dependent-read re-run `Stream`.

## [2]-[REACTIVE_QUERY]

- Owner: `ReactiveQuery`, the single read-side owner over `SqlClient.reactive([keys], query)` — a write wrapped in `Reactivity.mutation([keys], …)` (the `PgClient` discharges `Reactivity`) publishes the touched keys, and every `reactive([keys], query)` registered against those keys re-runs as a `Stream` element on each publish; `reactiveMailbox` is the same edge surfaced as a `ReadonlyMailbox` for a pull consumer. One owner over the read-invalidation concern, discriminated by the entity-keyed `InvalidationKey` rows, never a parallel change-feed per query.
- Cases: `InvalidationKey` is one `as const satisfies Record<…>` vocabulary keyed by entity — each row carries the channel anchor a write publishes and a dependent read subscribes — so a new reactive read is one entity row, never a hand-listed key string per call site; `keyOf(entity, scope)` projects a row plus the per-tenant/per-id scope tuple into the `ReadonlyArray<unknown>` key the substrate hashes. A write is `Reactivity.mutation(keysFor(entity, scope), writeEffect)` — the effect runs, then the keys publish atomically with its success; the `folder:interchange/Transport/gateway#COMMAND_GATEWAY` egress verb and the `folder:execution/outbox#TRANSACTIONAL_OUTBOX` relay wrap their writes in this mutation edge, so the one egress verb face is the sole key publisher. A dependent read is `sql.reactive(keysFor(entity, scope), query)` — the substrate runs `query` once on registration and re-runs it on every matching publish, emitting one `Stream` element per re-run, so a `folder:messaging/rpc#INTERNAL_RPC` query procedure or a `folder:execution/slo#SLO_BUDGET` signal read subscribes the touched keys and pushes the fresh rows without a poll. The re-run is the server-side leg of the one reactive spine — distinct from the browser projection's decoded-stream fold, which consumes the egress over the wire; this server leg invalidates over the `PgClient`.
- Entry: the owner rides the one `folder:persistence/store#STORE_BOUNDARY` `PgClient` — `sql.reactive`/`reactiveMailbox` are methods on the same `SqlClient` every query already acquires from the context tag, and the `Reactivity.Reactivity` requirement the bare `make`/`fromPool` carry is discharged by the layer's bundled default, so no separate `Reactivity` wiring is required; the reactive read is tenant-scoped through the same `folder:persistence/tenancy#TENANCY` `app.current_tenant` GUC every query reads, the scope tuple folding the tenant into the invalidation key so a cross-tenant publish never wakes another tenant's read.
- Wire: the owner crosses no .NET wire — the invalidation keys, the mutation publish, and the dependent re-run are node-internal over the one `PgClient`; the only cross-runtime contact is the `folder:interchange/Transport/gateway#COMMAND_GATEWAY` mutation that publishes the keys, itself a node owner, and the browser projection fold consumes the egress separately, never this server-side invalidation edge.
- Packages: `@effect/sql` for `SqlClient.reactive`/`reactiveMailbox` (the reactive-query methods on the one client) over the one `PgClient`; `@effect/sql-pg` for the `Reactivity.Reactivity` requirement the `PgClient.make` layer discharges via the bundled default; `@effect/experimental` for the `Reactivity` key-scoped `mutation`/`query`/`stream`/`invalidate` substrate (the second first-class direct consumer beside the `Sse` decode); `effect` for the `Stream` fold and the `InvalidationKey` vocabulary.
- Growth: a new reactive read lands as one `reactive([keys], query)` row keyed by an `InvalidationKey` entity, never a parallel change-feed; a new invalidated entity lands as one `InvalidationKey` vocabulary row whose channel a write publishes and a read subscribes; a new key-publishing write wraps its effect in `Reactivity.mutation(keysFor(…), …)`, never a manual `invalidate` call beside the write; a pull consumer takes the `reactiveMailbox` projection of the same edge.
- Boundary: the named defects — a poll loop or a timer-driven re-query instead of the `reactive` push; a parallel `Reactivity` instance wired beside the one the `PgClient` layer already discharges; a hand-listed key string per call site instead of the entity-keyed `InvalidationKey` vocabulary; a `Reactivity.invalidate` call detached from the write it follows instead of the `mutation` wrap that publishes atomically with the write's success; a per-query change-feed table instead of the one key-scoped substrate; a cross-tenant key omitting the GUC scope tuple so one tenant's write wakes another's read. This is a node-only surface, never browser-reachable.

```ts contract
import type { PgClient } from "@effect/sql-pg"
import type { SqlError } from "@effect/sql"
import { SqlClient } from "@effect/sql"
import { Reactivity } from "@effect/experimental"
import { Effect, Layer, Record as R, Stream } from "effect"

// --- [CONSTANTS] -----------------------------------------------------------------------

const _InvalidationKey = {
  session:      { channel: "rq.session",      tenantScoped: true  },
  outbox:       { channel: "rq.outbox",       tenantScoped: true  },
  notification: { channel: "rq.notification", tenantScoped: true  },
  workQueue:    { channel: "rq.work_queue",   tenantScoped: true  },
  featureFlag:  { channel: "rq.feature_flag", tenantScoped: false },
  signalSeries: { channel: "rq.signal_series", tenantScoped: true },
} as const satisfies Record<string, { readonly channel: string; readonly tenantScoped: boolean }>

type Entity = keyof typeof _InvalidationKey

interface KeyScope {
  readonly tenant: string
  readonly id?: string | undefined
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const keyOf = (entity: Entity, scope: KeyScope): ReadonlyArray<unknown> => {
  const row = _InvalidationKey[entity]
  return row.tenantScoped
    ? scope.id !== undefined ? [row.channel, scope.tenant, scope.id] : [row.channel, scope.tenant]
    : scope.id !== undefined ? [row.channel, scope.id] : [row.channel]
}

const keysFor = (entity: Entity, scopes: ReadonlyArray<KeyScope>): Record<string, ReadonlyArray<unknown>> =>
  R.fromEntries(
    (scopes.length === 0 ? [{ tenant: "*" }] : scopes).map((scope, i) => [`${entity}:${i}`, keyOf(entity, scope)] as const),
  )

// --- [SERVICES] ------------------------------------------------------------------------

class ReactiveQuery extends Effect.Service<ReactiveQuery>()("services/ReactiveQuery", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient

    const onWrite = <A, E, Rr>(
      entity: Entity,
      scopes: ReadonlyArray<KeyScope>,
      write: Effect.Effect<A, E, Rr>,
    ): Effect.Effect<A, E, Rr | Reactivity.Reactivity> =>
      Reactivity.mutation(write, keysFor(entity, scopes))

    const read = <A>(
      entity: Entity,
      scope: KeyScope,
      query: Effect.Effect<ReadonlyArray<A>, SqlError.SqlError>,
    ): Stream.Stream<ReadonlyArray<A>, SqlError.SqlError> =>
      sql.reactive(keyOf(entity, scope), query)

    const watch = <A>(
      entity: Entity,
      scope: KeyScope,
      query: Effect.Effect<ReadonlyArray<A>, SqlError.SqlError>,
    ): Effect.Effect<unknown, never, never> =>
      sql.reactiveMailbox(keyOf(entity, scope), query)

    const channels = (): ReadonlyArray<string> => R.values(_InvalidationKey).map((row) => row.channel)

    return { onWrite, read, watch, channels } as const
  }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const ReactiveQueryLayer: Layer.Layer<ReactiveQuery, never, SqlClient.SqlClient | PgClient.PgClient> =
  ReactiveQuery.Default
```
