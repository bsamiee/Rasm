# [SERVICES_QUOTA_GOVERNOR]

The per-tenant aggregate-backpressure owner — `QuotaGovernor`, a `folder:messaging/entity#ENTITY` per-tenant actor placement over the `@effect/experimental` `RateLimiter` module, the aggregate-fairness layer ABOVE the per-key `@effect/workflow` `DurableRateLimiter` so a single tenant cannot starve the cluster. It admits ONLY the per-tenant aggregate backpressure neither per-key primitive covers: `DurableRateLimiter` caps one workflow key's rate and the `RateLimiter` module owns the `fixedWindow`/`tokenBucket` token algebra over a `RateLimiterStore` — this owner places a per-tenant governor actor that meters the tenant's total draw across the AI turns, the outbox external sinks, and the remote-lane callouts, a fairness contract the durable tier structurally lacks. The governor is a placement onto the existing addressable `Entity`, not a new actor framework: `folder:messaging/entity#ENTITY` already routes a per-tenant governor to its tenant's shard group via `getShardGroup`, so this is one entity type on the settled actor. The owner crosses no .NET wire.

## [1]-[INDEX]

One cluster: `[2]-[QUOTA_GOVERNOR]` owns the per-tenant governor `Entity` placement, the `TenantQuota` durable cap-row table, the `RateLimiter` `fixedWindow`/`tokenBucket` backpressure regulation over `RateLimiterStore`, and the boundary against the per-key `DurableRateLimiter`.

## [2]-[QUOTA_GOVERNOR]

- Owner: `QuotaGovernor`, the per-tenant aggregate governor — a `folder:messaging/entity#ENTITY` `Entity` placed per tenant through `getShardGroup` (one actor per tenant pinned to the tenant's shard group), holding the `TenantQuota` cap rows and metering each draw through the `@effect/experimental` `RateLimiter` token algebra over a shared `RateLimiterStore`. One owner over the per-tenant aggregate-backpressure concern, never a parallel rate service per surface and never a re-implementation of the per-key `DurableRateLimiter`.
- Cases: `TenantQuota` is one `as const satisfies Record<…>` vocabulary keyed by the governed surface (`aiTurn`/`outboxExternal`/`remoteLane`) — each row carries the `limit`, the refill `window`, the token count, and the `fixed-window`/`token-bucket` algorithm as behavior columns, so a new governed surface is one row and a `Match` chain re-deriving the cap is the rejected form. A draw is the one high-level `RateLimiter.consume({ algorithm, onExceeded, window, limit, key, tokens })` over the `RateLimiter` service keyed by `(tenant, surface)` — the row's `onExhaust` selects `consume`'s `onExceeded`, so a `"delay"` row passes `onExceeded: "delay"` and `consume` returns a `ConsumeResult` carrying the throttle `delay` (`Duration.zero` when allowed immediately), which the governor applies through `Effect.sleep` to shape the draw (the backpressure arm), while a `"reject"` row passes `onExceeded: "fail"` and consume raises the `RateLimitExceeded` (carrying `retryAfter`/`remaining`) the governor folds into a typed `QuotaRejected` (the shed arm); the `RateLimiterStore` `fixedWindow`/`tokenBucket` primitives the `RateLimiter` service is built over return a raw count/TTL and never raise on exhaustion, so the governor mines the `consume` surface that owns the exhaustion algebra, never the bare store. The backpressure regulation is an expression-shaped `Stream` throttle, `consume` gating each `Stream` element so the tenant's draw is shaped rather than dropped where the row admits delay. The governor actor's per-key mailbox serializes the tenant's concurrent draws so the aggregate count is single-writer-correct — two concurrent AI turns on one tenant meter against one running count rather than racing the store — and the actor's `getShardGroup` placement pins every one of that tenant's draws to the same shard so the aggregate is local, never a distributed counter.
- Entry: the governor rides the SAME `folder:runtime-backplane/backplane#RUNNER_AND_SCHEDULING` shard manager and `RateLimiterStore` the cluster layers onto — it is a `folder:messaging/entity#ENTITY` actor-placement candidate the entity page already names (`getShardGroup` routes a per-tenant governor to its tenant's shard group), so the placement is one entity type on the settled actor, sequenced after the entity page's `DeliverAt`/`EntityResource` ROWs settle the shared page; it caps the `folder:agent/agent-runtime#AGENT_RUNTIME` agent turns (a turn draws against the tenant's `aiTurn` quota before the `DurableAgent` step), the `folder:eventing/transactional-outbox#TRANSACTIONAL_OUTBOX` external sinks (the relay draws against `outboxExternal` before a `DeliverySink.External` publish), and the remote-lane callouts, all metering one tenant's aggregate draw through the one governor actor.
- Wire: the governor carries no .NET wire type — the quota rows, the token draws, and the backpressure regulation are node-internal; the surfaces it caps (the AI turn, the outbox external publish) carry their own wire contacts, but the metering is node-side.
- Packages: `@effect/experimental` for the `RateLimiter` module (the high-level `RateLimiter.consume` algebra over the `fixedWindow`/`tokenBucket` `RateLimiterStore` primitives, the `RateLimiterError` `RateLimitExceeded`/`RateLimitStoreError` union, the `RateLimiter.layer` over `layerStoreMemory`) — the THIRD first-class direct `@effect/experimental` consumer beside the `Sse` decode and the `Reactivity` invalidation; `@effect/cluster` for the per-tenant governor `Entity` placement (`getShardGroup` routing); `@effect/workflow` for the per-key `DurableRateLimiter` the governor sits ABOVE, recorded as the boundary; `effect` for the backpressure `Stream` regulation and the `TenantQuota` vocabulary.
- Growth: a new governed surface lands as one `TenantQuota` vocabulary row carrying its cap/window/algorithm, never a parallel rate service; a new tenant lands as one governor `Entity` placement through `getShardGroup`, never a new actor framework; a new exhaustion policy lands as one row column the `RateLimiterError` fold reads; the per-key `DurableRateLimiter` stays where it is — this owner never absorbs it.
- Boundary: the named defects — re-implementing the per-key `@effect/workflow` `DurableRateLimiter` (the per-key primitive this owner sits ABOVE, capping one workflow key while the governor caps the tenant's aggregate draw across surfaces) or the `RateLimiter` token algebra instead of metering through them; a distributed quota counter instead of the `getShardGroup`-local per-tenant aggregate; a `Match` chain re-deriving the cap instead of the `TenantQuota` vocabulary row; a hard drop on every exhaustion instead of the row-policy `"delay"`-versus-`"reject"` fold over `RateLimiterError`; a parallel governor per surface instead of one per-tenant actor metering every surface. This is a node-only surface, never browser-reachable.

```ts contract
import { RateLimiter } from "@effect/experimental"
import { Entity, Sharding } from "@effect/cluster"
import { Rpc, RpcGroup } from "@effect/rpc"
import { Duration, Effect, Layer, Schema as S, Stream } from "effect"

// --- [CONSTANTS] -----------------------------------------------------------------------

const _TenantQuota = {
  aiTurn:         { limit: 60,   window: Duration.minutes(1), tokens: 60,   algorithm: "token-bucket" as const, onExhaust: "delay"  as const },
  outboxExternal: { limit: 600,  window: Duration.minutes(1), tokens: 600,  algorithm: "fixed-window" as const, onExhaust: "delay"  as const },
  remoteLane:     { limit: 120,  window: Duration.minutes(1), tokens: 120,  algorithm: "token-bucket" as const, onExhaust: "reject" as const },
} as const satisfies Record<string, {
  readonly limit: number
  readonly window: Duration.Duration
  readonly tokens: number
  readonly algorithm: "fixed-window" | "token-bucket"
  readonly onExhaust: "delay" | "reject"
}>

type GovernedSurface = keyof typeof _TenantQuota

// --- [ERRORS] --------------------------------------------------------------------------

class QuotaRejected extends S.TaggedError<QuotaRejected>()("QuotaRejected", {
  tenant: S.String,
  surface: S.Literal("aiTurn", "outboxExternal", "remoteLane"),
  retryAfter: S.DurationFromMillis,
}) {}

// --- [SERVICES] ------------------------------------------------------------------------

class QuotaGovernor extends Effect.Service<QuotaGovernor>()("services/QuotaGovernor", {
  accessors: true,
  effect: Effect.gen(function* () {
    const limiter = yield* RateLimiter.RateLimiter

    const consume = (tenant: string, surface: GovernedSurface): Effect.Effect<void, QuotaRejected | RateLimiter.RateLimiterError> => {
      const row = _TenantQuota[surface]
      return limiter
        .consume({
          key: `${tenant}:${surface}`,
          algorithm: row.algorithm,
          onExceeded: row.onExhaust === "delay" ? "delay" : "fail",
          window: row.window,
          limit: row.limit,
          tokens: 1,
        })
        .pipe(
          Effect.flatMap((result) => Effect.sleep(result.delay)),
          Effect.catchTag("RateLimiterError", (error) =>
            error.reason === "Exceeded"
              ? Effect.fail(new QuotaRejected({ tenant, surface, retryAfter: error.retryAfter }))
              : Effect.fail(error),
          ),
        )
    }

    const regulate = <A, E>(tenant: string, surface: GovernedSurface) =>
      (self: Stream.Stream<A, E>): Stream.Stream<A, E | QuotaRejected | RateLimiter.RateLimiterError> =>
        self.pipe(Stream.mapEffect((a) => consume(tenant, surface).pipe(Effect.as(a))))

    return { consume, regulate } as const
  }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const GovernorRpcs = RpcGroup.make(
  Rpc.make("Consume", { payload: { tenant: S.String, surface: S.Literal("aiTurn", "outboxExternal", "remoteLane") }, success: S.Void, error: QuotaRejected }),
)

const GovernorEntity = Entity.fromRpcGroup("QuotaGovernor", GovernorRpcs)

const placeGovernor = (tenant: string): Effect.Effect<unknown, never, Sharding.Sharding> =>
  GovernorEntity.getShardId(tenant)

const QuotaGovernorLayer: Layer.Layer<QuotaGovernor, never, RateLimiter.RateLimiter> = QuotaGovernor.Default
```
