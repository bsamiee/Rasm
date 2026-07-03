# [EDGE_MIDDLEWARE]

`api/middleware.ts` is the closed cross-cutting row family every entry surface inherits: the ambient request rows (`Current` — request stamp, tenant, negotiated locale as `Context.Reference` rows any rail reads for free), the authenticated `Principal` with its `Authn` middleware Tag (bearer session and API-key admission dispatching on the presented credential, both delegating `security`), the W3C trace continuation composing `telemetry`'s one `Propagation.ingress`, the pressure rows (in-flight load-shed, in-process rate windows, the distributed-quota row against the experimental limiter Tag), the idempotency-key admission with its `Idempotency` port, and the shield rows (CORS delegation plus the security-header table). Every row is a policy value or a transformer recoverable from a declaration — a new concern is one row on the owning cluster, never a wrapper stack a handler assembles — and every refusal is a `GateFault` whose reason row carries the kernel class and the HTTP status the `problem` altitude folds.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                      | [PUBLIC]              |
| :-----: | :--------------- | :---------------------------------------------------------------------------- | :-------------------- |
|  [01]   | [GATE_FAULT]     | the reason-discriminated refusal family with class/status/retry rows           | `GateFault`           |
|  [02]   | [CONTEXT_ROWS]   | `Current` ambient rows, locale negotiation, the trace-continuation transformer | `Current`             |
|  [03]   | [ADMISSION_ROWS] | `Principal`, the `Authn` middleware Tag + Layer, the `Idempotency` port        | `Principal`, `Gate`   |
|  [04]   | [PRESSURE_ROWS]  | load-shed, rate window, distributed quota                                      | `Gate`                |
|  [05]   | [SHIELD_ROWS]    | CORS delegation and the security-header table                                  | `Gate`                |

## [2]-[GATE_FAULT]

[GATE_FAULT]:
- Owner: `GateFault` — one `Schema.TaggedError` for every middleware refusal, reason-discriminated with rows carrying the kernel `class`, the status override, and `retryAfter` evidence where the refusing row measured a window; `get class()` projects the row so `FaultClass.of` classifies any escaped instance, and `get policy()` carries the `{ status }` probe `problem/detail.ts`'s ladder reads first.
- Law: the family is sized by refusal route, not by cause — `unauthorized` (credential absent or unverifiable, 401), `forbidden` (verified but insufficient, 403), `shed` (in-flight cap refused admission, 503), `rate` (window exhausted, 429), `conflict` (idempotency key replayed against a different payload, 409) — a finer cause is `detail` text, never a sixth reason minted for one surface.
- Law: `retryAfter` is an `Option<Duration>` field stamped by the pressure rows from their own measured window — the grace-hint probe `problem/policy.ts`'s ladder prefers over the class default — so a 429/503 always carries the truthful window, never a guessed constant.
- Packages: `effect` (`Schema`, `Option`, `Duration`); `kernel/fault/classify` (`FaultClass`).

```typescript
import { FaultClass, Refined } from "@rasm/ts/kernel"
import { ApiKey, Jwt } from "@rasm/ts/security"
import { Propagation } from "@rasm/ts/telemetry"
import { HttpApiMiddleware, HttpServerRequest } from "@effect/platform"
import { Array, Context, DateTime, Deferred, Duration, Effect, HashMap, Layer, Number, Option, Order, RateLimiter, Redacted, Ref, Schema, type Scope, pipe } from "effect"

const _reasons = ["unauthorized", "forbidden", "shed", "rate", "conflict"] as const

const _faults = {
  unauthorized: { class: "denied", status: 401 },
  forbidden: { class: "denied", status: 403 },
  shed: { class: "unavailable", status: 503 },
  rate: { class: "exhausted", status: 429 },
  conflict: { class: "conflicted", status: 409 },
} as const

declare namespace GateFault {
  type Reason = keyof typeof _faults
  type Row = { readonly class: FaultClass.Kind; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof _faults> = T
}

class GateFault extends Schema.TaggedError<GateFault>()("GateFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
  retryAfter: Schema.optionalWith(Schema.DurationFromSelf, { as: "Option" }),
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  get policy(): GateFault.Row {
    return _faults[this.reason]
  }
  override get message(): string {
    return `<gate:${this.reason}> ${this.detail}`
  }
}
```

## [3]-[CONTEXT_ROWS]

[CONTEXT_ROWS]:
- Owner: `Current` — the ambient request rows as `Context.Reference` classes: `Current.Stamp` carries `Option` of the per-request mark (`id`, `at`), `Current.Tenant` carries `Option` of the tenant key, `Current.Locale` carries the negotiated `Refined.Locale` with the fleet default answering when no request provided one — three rows, each readable from any rail at zero requirement pressure, overridden per request by scoped provision at the serve seam.
- Law: locale negotiation is one fold — `Current.negotiate(header)` splits the `Accept-Language` list, ranks by `q` weight descending, and takes the first tag the kernel `Refined.Locale` schema admits — so a malformed tag or an empty header lands on the reference default and negotiation can never fail; the negotiated value is BCP-47-canonical by the kernel brand's own filter.
- Law: trace continuation is composed, never re-derived — `Current.traced(effect, headers)` delegates `telemetry`'s `Propagation.ingress` with the request headers as the carrier, so extract-and-continue at the HTTP door is the same one transformer every other ingress composes and a second `traceparent` decode cannot exist here.
- Law: the stamp mints at the door — `Current.provide(effect, stamp)` provides all three rows in one scoped provision (stamp as given, tenant and locale projected from it), so a handler, a log annotation, and the problem fold all read one coherent request identity; `problem/detail.ts` reads `Current.Stamp` for the `instance` member and the `requestId` extension.
- Growth: a new ambient axis is one `Context.Reference` row plus its projection inside `provide`.
- Packages: `effect` (`Context`, `Option`, `Schema`, `Array`, `Order`); `kernel/schema/brand` (`Refined.Locale`); `telemetry/otlp/context` (`Propagation`).

```typescript
const _byWeight: Order.Order<readonly [string, number]> = Order.mapInput(Order.reverse(Order.number), (pair: readonly [string, number]) => pair[1])

const _negotiate = (header: Option.Option<string>, fallback: Refined.Locale): Refined.Locale =>
  pipe(
    Option.getOrElse(header, () => ""),
    (raw) => raw.split(","),
    Array.filterMap((part) => {
      const [tag, weight] = part.split(";q=")
      const trimmed = (tag ?? "").trim()
      return trimmed.length === 0
        ? Option.none()
        : Option.some([trimmed, Option.getOrElse(Number.parse(weight ?? "1"), () => 0)] as const)
    }),
    Array.sort(_byWeight),
    Array.filterMap(([tag]) => Option.getRight(Schema.decodeUnknownEither(Refined.Locale)(tag))),
    Array.head,
    Option.getOrElse(() => fallback),
  )

class _Stamp extends Context.Reference<_Stamp>()("edge/api/Current/Stamp", {
  defaultValue: () => Option.none<Current.Mark>(),
}) {}

class _Tenant extends Context.Reference<_Tenant>()("edge/api/Current/Tenant", {
  defaultValue: () => Option.none<string>(),
}) {}

class _Locale extends Context.Reference<_Locale>()("edge/api/Current/Locale", {
  defaultValue: () => Schema.decodeUnknownSync(Refined.Locale)("en"),
}) {}

const _provide = <A, E, R>(self: Effect.Effect<A, E, R>, mark: Current.Mark, fallback: Refined.Locale): Effect.Effect<A, E, R> =>
  self.pipe(
    Effect.provideService(_Stamp, Option.some(mark)),
    Effect.provideService(_Tenant, mark.tenant),
    Effect.provideService(_Locale, Option.getOrElse(mark.locale, () => fallback)),
  )

const _traced = <A, E, R>(self: Effect.Effect<A, E, R>, headers: Readonly<Record<string, string | undefined>>): Effect.Effect<A, E, R> =>
  Propagation.ingress(self, headers)

const Current: {
  readonly Locale: typeof _Locale
  readonly Stamp: typeof _Stamp
  readonly Tenant: typeof _Tenant
  readonly negotiate: typeof _negotiate
  readonly provide: typeof _provide
  readonly traced: typeof _traced
} = {
  Locale: _Locale,
  Stamp: _Stamp,
  Tenant: _Tenant,
  negotiate: _negotiate,
  provide: _provide,
  traced: _traced,
}

declare namespace Current {
  type Mark = {
    readonly id: string
    readonly at: DateTime.Utc
    readonly tenant: Option.Option<string>
    readonly locale: Option.Option<Refined.Locale>
  }
}
```

## [4]-[ADMISSION_ROWS]

[ADMISSION_ROWS]:
- Owner: `Principal` — the one authenticated identity, declared as a `Context.Tag` whose service is the identity record (subject, live session as `Option`, tenant as `Option`, scopes, the `via` discriminant `session` | `apikey`) so the same name is the requirement a protected handler yields, the type its signatures speak (`Principal.Shape`), and the carrier of `Principal.allows` — the single scope probe no authorization read re-derives. The `Authn` middleware Tag provides it — `HttpApiMiddleware.Tag` with `failure: GateFault` and `provides: Principal` — attached per group through `.middleware(Authn)`, so a protected group's handlers receive `Principal` from the requirement channel and an unprotected group never pays the decode.
- Law: credential dispatch is one ladder over presented evidence — a `Bearer` authorization header verifies through `security`'s `Jwt.verify` into `AccessClaims`, an `x-api-key` header resolves through `ApiKey.resolve` into an `ApiKeyRecord`, and both lift into the same `Principal` shape; absence or verification failure folds to `unauthorized`, never a per-scheme fault family, and the security fault detail stays out of the refusal text (the 401 body is generic by policy, the evidence rides telemetry).
- Law: `Idempotency` is a port with in-process teeth — `claim(key, digest)` settles `Fresh` exactly once per key and parks every concurrent or later duplicate on the first execution's `Deferred`; `Idempotency.memory(retention)` is the single-node Layer whose retention window sweeps expired cells inside the same atomic claim, and a store-backed Layer replaces it at the app root for a fleet. A replayed key whose request payload digest differs refuses as `conflict`, so a client cannot ride one key across two different requests.
- Law: the idempotency key admits through a brand — `Gate.IdempotencyKey` bounds length and charset at the header seam — and admission is scoped to the mutating methods a group opts in; a GET carrying the header is ignored, not refused.
- Boundary: session and API-key semantics are `security`'s (`session/token`, `authn/apikey`); this cluster owns only the HTTP presentation lift and the middleware Tag; snapshot shape and replay response mechanics ride `problem`-mapped handler outcomes.
- Packages: `effect`; `@effect/platform` (`HttpApiMiddleware`, `HttpServerRequest`); `security` (`Jwt`, `ApiKey`).

```typescript
type _Principal = {
  readonly subject: string
  readonly session: Option.Option<string>
  readonly tenant: Option.Option<string>
  readonly scopes: ReadonlyArray<string>
  readonly via: "session" | "apikey"
}

class Principal extends Context.Tag("edge/api/Principal")<Principal, _Principal>() {
  static readonly allows = (principal: _Principal, scope: string): boolean => Array.contains(principal.scopes, scope)
}

declare namespace Principal {
  type Shape = _Principal
}

const _Credentials = Schema.Struct({
  authorization: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  "x-api-key": Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})

class Authn extends HttpApiMiddleware.Tag<Authn>()("edge/api/Authn", {
  failure: GateFault,
  provides: Principal,
}) {
  static readonly live: Layer.Layer<Authn, never, Jwt | ApiKey> = Layer.effect(
    Authn,
    Effect.gen(function* () {
      const jwt = yield* Jwt
      const keys = yield* ApiKey
      const unauthorized = (detail: string) => new GateFault({ reason: "unauthorized", detail, retryAfter: Option.none() })
      return Effect.gen(function* () {
        const presented = yield* HttpServerRequest.schemaHeaders(_Credentials).pipe(Effect.mapError(() => unauthorized("credentials")))
        const bearer = Option.filterMap(presented.authorization, (header) =>
          header.startsWith("Bearer ") ? Option.some(header.slice(7)) : Option.none())
        return yield* Option.match(bearer, {
          onSome: (token) =>
            jwt.verify(Redacted.make(token)).pipe(
              Effect.mapBoth({
                onFailure: () => unauthorized("bearer"),
                onSuccess: (claims): _Principal =>
                  ({ subject: claims.sub, session: Option.some(claims.sid), tenant: claims.tid, scopes: claims.scope, via: "session" }),
              }),
            ),
          onNone: () =>
            Option.match(presented["x-api-key"], {
              onNone: () => Effect.fail(unauthorized("absent")),
              onSome: (key) =>
                keys.resolve(Redacted.make(key)).pipe(
                  Effect.mapBoth({
                    onFailure: () => unauthorized("apikey"),
                    onSuccess: (record): _Principal =>
                      ({ subject: record.subject, session: Option.none(), tenant: Option.none(), scopes: record.scopes, via: "apikey" }),
                  }),
                ),
            }),
        })
      })
    }),
  )
}

const _IdempotencyKey = Schema.NonEmptyString.pipe(Schema.maxLength(128), Schema.pattern(/^[A-Za-z0-9_-]+$/), Schema.brand("IdempotencyKey"))

type _Cell = { readonly digest: string; readonly slot: Deferred.Deferred<unknown>; readonly at: DateTime.Utc }

class Idempotency extends Context.Tag("edge/api/Idempotency")<Idempotency, {
  readonly claim: <A>(key: typeof _IdempotencyKey.Type, digest: string) => Effect.Effect<Idempotency.Claim<A>, GateFault>
}>() {
  static readonly memory = (retention: Duration.Duration): Layer.Layer<Idempotency> =>
    Layer.effect(
      Idempotency,
      Effect.map(Ref.make(HashMap.empty<string, _Cell>()), (cells) => ({
        claim: <A>(key: typeof _IdempotencyKey.Type, digest: string) =>
          Effect.gen(function* () {
            const slot = yield* Deferred.make<unknown>()
            const now = yield* DateTime.now
            const held = yield* Ref.modify(cells, (table) => {
              const live = HashMap.filter(table, (cell) => DateTime.lessThan(now, DateTime.addDuration(cell.at, retention)))
              return Option.match(HashMap.get(live, key), {
                onNone: () => [Option.none<_Cell>(), HashMap.set(live, key, { digest, slot, at: now })] as const,
                onSome: (cell) => [Option.some(cell), live] as const,
              })
            })
            return yield* Option.match(held, {
              onNone: () =>
                Effect.succeed<Idempotency.Claim<A>>({ _tag: "Fresh", settle: (outcome) => Deferred.succeed(slot, outcome).pipe(Effect.asVoid) }),
              onSome: (cell) =>
                cell.digest === digest
                  ? Effect.succeed<Idempotency.Claim<A>>({ _tag: "Replay", outcome: Deferred.await(cell.slot).pipe(Effect.map((outcome) => outcome as A)) })
                  : Effect.fail(new GateFault({ reason: "conflict", detail: "idempotency-key payload mismatch", retryAfter: Option.none() })),
            })
          }),
      })),
    )
}

declare namespace Idempotency {
  type Claim<A> =
    | { readonly _tag: "Fresh"; readonly settle: (outcome: A) => Effect.Effect<void> }
    | { readonly _tag: "Replay"; readonly outcome: Effect.Effect<A> }
}
```

## [5]-[PRESSURE_AND_SHIELD]

[PRESSURE_ROWS]:
- Owner: `Gate` — the pressure transformer family assembled with the shield rows under one export: `Gate.shed` brackets a section under an in-flight cap whose refusal is immediate (`withPermitsIfAvailable` settling `Option.none` under saturation → `shed` with the declared grace), `Gate.window` prices calls against a scoped in-process `RateLimiter.make` row, and `Gate.fenced` consumes the store-backed experimental limiter Tag for a fleet-wide quota (`onExceeded: "fail"` surfacing the evidence this row re-spells as `rate` with the truthful `retryAfter`).
- Law: permits bound concurrency and windows bound throughput — `shed` is the queue-depth 503 lever (a saturated cap answers now, never parks a caller into a timeout), `window`/`fenced` are the 429 levers — and conflating the two axes is the named selection error; both stamp `retryAfter` from their own measured window.
- Law: policy is one value row — `Gate.Pressure` carries `inFlight`, `grace`, and the window fields; an app tunes pressure by constructing the row, never by threading knobs into transformers — and the scoped constructors mean one gate value serves every call site of a surface.
- Law: the distributed row is port-shaped by Layer — `Gate.fenced` types against the experimental `RateLimiter.RateLimiter` Tag; the app root satisfies it with `layerStoreMemory` on one node or a store-backed Layer on a fleet, so the fleet-quota decision is a root selection, never an edge rewrite. Both limiter faults share the `RateLimiterError` tag, so the fold discriminates on the `reason` literal — `Exceeded` re-spells as `rate` carrying the limiter's own `retryAfter`, `StoreError` dies as a defect because a broken quota backend is never a caller 429. Webhook intake quota is NOT this row — it types against `hook/admit.ts`'s `QuotaGate` port.

[SHIELD_ROWS]:
- Law: CORS is delegated, never re-implemented — the app assembly composes `HttpApiBuilder.middlewareCors(options)` directly with the options row as its one policy value; no `Gate` member renames it, because a forwarding member would be the one-hop wrapper the platform surface already owns.
- Law: `_SHIELD` is the security-header table — `strict-transport-security`, `x-content-type-options`, `x-frame-options`, `referrer-policy`, plus the CSP row — applied to every response by the serve row reading this table; a header tweak is a row edit, and a handler hand-setting a shield header is the drift defect.
- Boundary: attachment sites are owned elsewhere — group middleware attaches `Authn`, the serve row applies `_SHIELD` and provides `Current`; this page owns the rows and their semantics.
- Packages: `effect` (`RateLimiter`, `Effect`, `Duration`, `Option`); `@effect/experimental` (`RateLimiter` service Tag — the distributed row); `@effect/platform` (`HttpApiBuilder` — the CORS delegation named on the card, composed at assembly).

```typescript
import { RateLimiter as FleetLimiter } from "@effect/experimental"

const _SHIELD = {
  "strict-transport-security": "max-age=63072000; includeSubDomains",
  "x-content-type-options": "nosniff",
  "x-frame-options": "DENY",
  "referrer-policy": "strict-origin-when-cross-origin",
  "content-security-policy": "default-src 'self'; frame-ancestors 'none'",
} as const satisfies Record<string, string>

declare namespace Gate {
  type IdempotencyKey = typeof _IdempotencyKey.Type
  type Pressure = {
    readonly inFlight: number
    readonly grace: Duration.Duration
    readonly window: { readonly limit: number; readonly interval: Duration.Duration }
  }
}

const Gate = {
  Authn,
  Idempotency,
  IdempotencyKey: _IdempotencyKey,
  shield: _SHIELD,
  shed: (pressure: Gate.Pressure): Effect.Effect<<A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, GateFault | E, R>> =>
    Effect.map(Effect.makeSemaphore(pressure.inFlight), (permits) =>
      <A, E, R>(self: Effect.Effect<A, E, R>) =>
        permits.withPermitsIfAvailable(1)(self).pipe(
          Effect.flatMap(Option.match({
            onNone: () => Effect.fail(new GateFault({ reason: "shed", detail: "in-flight cap", retryAfter: Option.some(pressure.grace) })),
            onSome: Effect.succeed,
          })),
        )),
  window: (pressure: Gate.Pressure): Effect.Effect<<A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, GateFault | E, R>, never, Scope.Scope> =>
    Effect.map(RateLimiter.make({ limit: pressure.window.limit, interval: pressure.window.interval, algorithm: "token-bucket" }), (limiter) =>
      <A, E, R>(self: Effect.Effect<A, E, R>) =>
        limiter(self).pipe(
          Effect.timeoutFail({ duration: pressure.grace, onTimeout: () => new GateFault({ reason: "rate", detail: "window", retryAfter: Option.some(pressure.grace) }) }),
        )),
  fenced: <A, E, R>(self: Effect.Effect<A, E, R>, key: string, pressure: Gate.Pressure): Effect.Effect<A, GateFault | E, R | FleetLimiter.RateLimiter> =>
    Effect.flatMap(FleetLimiter.RateLimiter, (limiter) =>
      limiter.consume({ key, window: pressure.window.interval, limit: pressure.window.limit, onExceeded: "fail" }).pipe(
        Effect.catchAll((fault) =>
          fault.reason === "Exceeded"
            ? Effect.fail(new GateFault({ reason: "rate", detail: key, retryAfter: Option.some(fault.retryAfter) }))
            : Effect.die(fault)),
        Effect.zipRight(self),
      )),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Current, Gate, GateFault, Principal }
```
