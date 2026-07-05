# [RUNTIME_API]

The one public front door's declarative engine: a domain folder exports its `HttpApiGroup` or `RpcGroup` as data, the APP — never this module, never any lib module — assembles exactly one `HttpApi` value and crosses exactly one RPC protocol row with one serialization row at its root, and every secondary surface — the OpenAPI 3.1 document, the served Scalar reference UI, the byte-stable spec artifact, the typed HTTP SDK, the typed RPC caller, the fetch-shaped web handler — projects from that same assembled value so spec, docs, client, and server cannot drift. Auth is declarative into the emitted contract: the `Authn` middleware Tag carries its `HttpApiSecurity` schemes, so the bearer and API-key security requirements land in the OpenAPI document from the same declaration the handler set enforces, and a protected group's handlers receive `Principal` from the requirement channel. Every refusal is one `GateFault` whose reason row carries the fault class and the status probe the `problem` fold reads; the ambient request rows — stamp, tenant, negotiated locale — are `Context.Reference` values any rail reads at zero requirement pressure. The god-contract is structurally impossible because `HttpApiBuilder.group` demands the assembled api value the lib never holds. The module ships on the `./server` exports subpath as `runtime/src/serve/api.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                              | [PUBLIC]              |
| :-----: | :--------------- | :---------------------------------------------------------------------------------- | :-------------------- |
|  [01]   | `CONVENTION`     | version-prefix rows, the cursor brand, page-query and page-envelope constructors     | `Convention`          |
|  [02]   | `GATE_FAULT`     | the reason-discriminated refusal family with class/status/retry rows                 | `GateFault`           |
|  [03]   | `CURRENT_ROWS`   | ambient stamp/tenant/locale references, locale negotiation, trace continuation       | `Current`             |
|  [04]   | `ADMISSION_ROWS` | `Principal`, the scheme-threaded `Authn` security middleware, pressure, idempotency  | `Principal`, `Gate`   |
|  [05]   | `CONTRIBUTION`   | the http and rpc pairing constructors, protocol and codec rosters, upload modality   | `Contribution`        |
|  [06]   | `EMIT`           | spec artifact, docs stack, derived HTTP and RPC clients, the web-handler edge form   | `Emit`                |

## [2]-[CONVENTION]

[CONVENTION]:
- Owner: `Convention` — the shared surface vocabulary both contribution families speak: the version tuple (`v1`; a new major is one tuple entry every consumer inherits), the opaque `Cursor` brand, the `PageParams` query schema (cursor as `Option`, limit defaulted and ceiling-bounded at the declaration so no handler re-checks bounds), and `Convention.page(item)` — one generic constructor deriving the page envelope for any item schema, so a per-shape page schema cannot exist.
- Law: pagination is cursor-only — the cursor is minted by the owning read surface, opaque to the caller, bounded at the brand; `next` absent means exhausted, spelled `Option` by `optionalWith`; offset pagination has no vocabulary here and cannot be contributed.
- Law: the version prefix attaches at the group — `group.prefix(Convention.prefix("v1"))` — so a group is versioned as contributed data and two majors of one group coexist as two contributions; a version segment hand-written into an endpoint path is the drift defect.
- Growth: a new convention axis (sort grammar, field selection) is one schema row on this owner, inherited by every group that composes it.
- Packages: `effect` (`Schema`, `Option`).

```typescript
import {
  HttpApi, HttpApiBuilder, HttpApiClient, type HttpApiGroup, HttpApiMiddleware, HttpApiScalar, HttpApiSecurity,
  HttpApiSwagger, type HttpClient, HttpTraceContext, OpenApi,
} from "@effect/platform"
import { PersistedCache, type Persistence, RateLimiter as Fleet } from "@effect/experimental"
import { RpcClient, type RpcGroup, RpcSerialization, RpcServer } from "@effect/rpc"
import {
  Array, Context, Data, DateTime, Deferred, Duration, Effect, HashMap, Layer, Number, Option, Order, Predicate,
  RateLimiter, Record, Redacted, Ref, Schema, type Scope, pipe,
} from "effect"
import { type FaultClass, Refined } from "@rasm/ts/core"
import { ApiKey, Jwt } from "@rasm/ts/security"
import { Propagation } from "../otel/emit.ts"

const _VERSIONS = ["v1"] as const

const _Cursor = Schema.NonEmptyString.pipe(Schema.maxLength(512), Schema.pattern(/^[A-Za-z0-9_-]+$/), Schema.brand("Cursor"))

const _PageParams = Schema.Struct({
  cursor: Schema.optionalWith(_Cursor, { as: "Option" }),
  limit: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 200)), { default: () => 50 }),
})

const _page = <A, I, R>(item: Schema.Schema<A, I, R>) =>
  Schema.Struct({
    items: Schema.Array(item),
    next: Schema.optionalWith(_Cursor, { as: "Option" }),
  })

declare namespace Convention {
  type Version = (typeof _VERSIONS)[number]
  type Cursor = typeof _Cursor.Type
  type PageParams = typeof _PageParams.Type
}

const Convention: {
  readonly versions: typeof _VERSIONS
  readonly Cursor: typeof _Cursor
  readonly PageParams: typeof _PageParams
  readonly page: typeof _page
  readonly prefix: <V extends Convention.Version>(version: V) => `/${V}`
} = {
  versions: _VERSIONS,
  Cursor: _Cursor,
  PageParams: _PageParams,
  page: _page,
  prefix: (version) => `/${version}`,
}
```

## [3]-[GATE_FAULT]

[GATE_FAULT]:
- Owner: `GateFault` — one `Schema.TaggedError` for every front-door refusal, reason-discriminated with rows carrying the core `class`, the status override, and `retryAfter` evidence where the refusing row measured a window; `get class()` projects the row so `FaultClass.of` classifies any escaped instance, and `get policy()` carries the `{ status }` probe the `problem` ladder reads first.
- Law: the family is sized by refusal route, never by cause — `unauthorized` (credential absent or unverifiable, 401), `forbidden` (verified but insufficient, 403), `shed` (in-flight cap refused admission, 503), `rate` (window exhausted, 429), `conflict` (idempotency key replayed against a different payload, 409) — a finer cause is `detail` text, never a sixth reason minted for one surface.
- Law: `retryAfter` is an `Option<Duration>` stamped by the pressure rows from their own measured window — the grace hint the `problem` ladder prefers over the class default — so a 429/503 always carries the truthful window, never a guessed constant.
- Packages: `effect` (`Schema`, `Option`, `Duration`); `@rasm/ts/core` (`FaultClass`).

```typescript
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
  type Contract = { readonly [K in Reason]: Row }
  type _Rows<T extends Contract = typeof _faults> = T
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

## [4]-[CURRENT_ROWS]

[CURRENT_ROWS]:
- Owner: `Current` — the ambient request rows as `Context.Reference` classes: `Current.Stamp` carries `Option` of the per-request mark (`id`, `at`, tenant, locale), `Current.Tenant` carries `Option` of the tenant key, `Current.Locale` carries the negotiated `Refined.Locale` with the fleet default answering when no request provided one — three rows, each readable from any rail at zero requirement pressure, overridden per request by scoped provision at the route seam.
- Law: locale negotiation is one fold — `Current.negotiate(header, fallback)` splits the `Accept-Language` list, ranks by `q` weight descending, and takes the first tag the core `Refined.Locale` schema admits — a malformed tag or an empty header lands on the fallback and negotiation can never fail; the negotiated value is BCP-47-canonical by the core brand's own filter.
- Law: trace continuation is composed, never re-derived — `Current.traced(effect, headers)` delegates `emit#PROPAGATION`'s one ingress transformer with the request headers as the carrier, so extract-and-continue at the HTTP door is the same transformer every other ingress composes and a second `traceparent` decode cannot exist here.
- Law: the stamp mints at the door — `Current.provide(effect, mark, fallback)` provides all three rows in one scoped provision (stamp as given, tenant and locale projected from it), so a handler, a log annotation, and the problem fold read one coherent request identity; the `problem` page reads `Current.Stamp` for the `instance` member and the `requestId` extension.
- Growth: a new ambient axis is one `Context.Reference` row plus its projection inside `provide`.
- Packages: `effect` (`Context`, `Option`, `Schema`, `Array`, `Order`, `Number`); `@rasm/ts/core` (`Refined`); `../otel/emit.ts` (`Propagation`).

```typescript
const _byWeight: Order.Order<readonly [string, number]> = Order.mapInput(
  Order.reverse(Order.number),
  (pair: readonly [string, number]) => pair[1],
)

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

class _Stamp extends Context.Reference<_Stamp>()("runtime/serve/Current/Stamp", {
  defaultValue: () => Option.none<Current.Mark>(),
}) {}

class _Tenant extends Context.Reference<_Tenant>()("runtime/serve/Current/Tenant", {
  defaultValue: () => Option.none<string>(),
}) {}

class _Locale extends Context.Reference<_Locale>()("runtime/serve/Current/Locale", {
  defaultValue: () => Schema.decodeUnknownSync(Refined.Locale)("en"),
}) {}

const _provide = <A, E, R>(
  self: Effect.Effect<A, E, R>,
  mark: Current.Mark,
  fallback: Refined.Locale,
): Effect.Effect<A, E, R> =>
  self.pipe(
    Effect.provideService(_Stamp, Option.some(mark)),
    Effect.provideService(_Tenant, mark.tenant),
    Effect.provideService(_Locale, Option.getOrElse(mark.locale, () => fallback)),
  )

const _traced = <A, E, R>(
  self: Effect.Effect<A, E, R>,
  headers: { readonly [key: string]: string | undefined },
): Effect.Effect<A, E, R> => Propagation.ingress(self, headers)

declare namespace Current {
  type Mark = {
    readonly id: string
    readonly at: DateTime.Utc
    readonly tenant: Option.Option<string>
    readonly locale: Option.Option<Refined.Locale>
  }
}

const Current: {
  readonly Locale: typeof _Locale
  readonly Stamp: typeof _Stamp
  readonly Tenant: typeof _Tenant
  readonly negotiate: typeof _negotiate
  readonly provide: typeof _provide
  readonly traced: typeof _traced
} = { Locale: _Locale, Stamp: _Stamp, Tenant: _Tenant, negotiate: _negotiate, provide: _provide, traced: _traced }
```

## [5]-[ADMISSION_ROWS]

[ADMISSION_ROWS]:
- Owner: `Principal` — the one authenticated identity, a `Context.Tag` whose service is the identity record (subject, live session as `Option`, tenant as `Option`, scopes, the `via` discriminant `session | apikey`) so the same name is the requirement a protected handler yields, the type its signatures speak, and the carrier of `Principal.allows` — the single scope probe no authorization read re-derives. `Authn` is the scheme-threaded security middleware: `HttpApiMiddleware.Tag` with `failure: GateFault`, `provides: Principal`, and the `security` record naming `HttpApiSecurity.bearer` plus `HttpApiSecurity.apiKey({ in: "header", key: "x-api-key" })` — declared once, the schemes land in the emitted OpenAPI security requirements AND the implementation is one handler record keyed by scheme receiving the already-decoded credential, so declarative auth and enforced auth are one declaration.
- Law: credential verification delegates the security wave — the bearer arm verifies through `Jwt.verify` into `AccessClaims`, the apiKey arm resolves through `ApiKey.resolve` into an `ApiKeyRecord`, both lift into the one `Principal` shape; verification failure folds to `unauthorized` with generic detail (the evidence rides telemetry, never the 401 body), and attachment is `.middleware(Gate.Authn)` on the contributed group so an unprotected group never pays the decode.
- Law: pressure rows bound two distinct axes — `Gate.shed` brackets a section under an in-flight cap whose refusal is immediate (`withPermitsIfAvailable` settling `Option.none` under saturation folds to `shed` with the declared grace: the queue-depth 503 lever), `Gate.window` prices calls against a scoped in-process `RateLimiter.make` row (the 429 lever) — conflating concurrency and throughput is the named selection error; both stamp `retryAfter` from their own measured window, and policy is one `Gate.Pressure` value row, never threaded knobs.
- Law: the distributed quota row is port-shaped by Layer — `Gate.fenced` composes the experimental `RateLimiter.makeWithRateLimiter` transformer against the `RateLimiter.RateLimiter` Tag, the app root satisfies it with `layerStoreMemory` on one node or a store-backed Layer on a fleet, `RateLimitExceeded` re-spells as `rate` carrying the row's window, and `RateLimitStoreError` dies as a defect because a broken quota backend is never a caller 429.
- Law: `Idempotency` is a port with two teeth tiers — `claim(key, digest, outcome)` settles `Fresh` exactly once per key and parks every duplicate on the first execution's `Deferred`, the claim as one `Data.taggedEnum` a caller `$match`es; the `outcome` schema is the replay's type evidence — the parked value re-admits through `Schema.validate` so the fast lane carries the same schema proof the fleet tier's `Schema.TaggedRequest` carries, a rejected park refusing as `conflict` beside the digest mismatch; `Idempotency.memory(retention)` is the single-node Layer sweeping expired cells inside the same atomic claim, and a replayed key whose payload digest differs refuses as `conflict`. The key admits through the `Gate.IdempotencyKey` brand at the header seam; a GET carrying the header is ignored, never refused.
- Law: the fleet tier is `Idempotency.persisted` — `PersistedCache.make({ storeId, lookup, timeToLive })` over the store-owned `Persistence.layerResultKeyValueStore`, keyed by a `Schema.TaggedRequest` whose `PrimaryKey` fuses idempotency key and payload digest, so the first execution's exit persists for the retention window, every fleet duplicate replays the stored exit typed through the request's own success/failure schemas, and a divergent payload is a different key that executes fresh; the strict 409 divergence posture stays the memory gate composed in front, so both tiers ride one root and zero handler change.
- Boundary: session and API-key semantics are the security wave's (`Jwt`, `ApiKey`); this cluster owns only the HTTP presentation lift and the middleware Tag; response-shield headers and the serving seam are `route#SEAM_ROWS`'s.
- Growth: a third credential scheme is one `security` record entry plus its handler arm; a fleet quota engine is a Layer swap on the `Idempotency` or limiter Tag at the root.
- Packages: `effect` (`RateLimiter`, `Deferred`, `HashMap`, `Ref`, `Redacted`); `@effect/platform` (`HttpApiMiddleware`, `HttpApiSecurity`); `@effect/experimental` (`RateLimiter` — the distributed row); `@rasm/ts/security` (`Jwt`, `ApiKey`).

```typescript
type _Principal = {
  readonly subject: string
  readonly session: Option.Option<string>
  readonly tenant: Option.Option<string>
  readonly scopes: ReadonlyArray<string>
  readonly via: "session" | "apikey"
}

class Principal extends Context.Tag("runtime/serve/Principal")<Principal, _Principal>() {
  static readonly allows = (principal: _Principal, scope: string): boolean => Array.contains(principal.scopes, scope)
}

declare namespace Principal {
  type Shape = _Principal
}

class Authn extends HttpApiMiddleware.Tag<Authn>()("runtime/serve/Authn", {
  failure: GateFault,
  provides: Principal,
  security: {
    bearer: HttpApiSecurity.bearer,
    apiKey: HttpApiSecurity.apiKey({ in: "header", key: "x-api-key" }),
  },
}) {
  static readonly live: Layer.Layer<Authn, never, Jwt | ApiKey> = Layer.effect(
    Authn,
    Effect.gen(function* () {
      const jwt = yield* Jwt
      const keys = yield* ApiKey
      const unauthorized = (detail: string) =>
        new GateFault({ reason: "unauthorized", detail, retryAfter: Option.none() })
      return {
        bearer: (token: Redacted.Redacted<string>) =>
          jwt.verify(token).pipe(
            Effect.mapBoth({
              onFailure: () => unauthorized("bearer"),
              onSuccess: (claims): _Principal => ({
                subject: claims.sub,
                session: Option.some(claims.sid),
                tenant: claims.tid,
                scopes: claims.scope,
                via: "session",
              }),
            }),
          ),
        apiKey: (key: Redacted.Redacted<string>) =>
          keys.resolve(key).pipe(
            Effect.mapBoth({
              onFailure: () => unauthorized("apikey"),
              onSuccess: (record): _Principal => ({
                subject: record.subject,
                session: Option.none(),
                tenant: Option.none(),
                scopes: record.scopes,
                via: "apikey",
              }),
            }),
          ),
      }
    }),
  )
}

const _IdempotencyKey = Schema.NonEmptyString.pipe(
  Schema.maxLength(128),
  Schema.pattern(/^[A-Za-z0-9_-]+$/),
  Schema.brand("IdempotencyKey"),
)

type _Cell = { readonly digest: string; readonly slot: Deferred.Deferred<unknown>; readonly at: DateTime.Utc }

type _Claim<A> = Data.TaggedEnum<{
  Fresh: { readonly settle: (outcome: A) => Effect.Effect<void> }
  Replay: { readonly outcome: Effect.Effect<A, GateFault> }
}>

interface _ClaimDef extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: _Claim<this["A"]>
}

const _Claim = Data.taggedEnum<_ClaimDef>()

class Idempotency extends Context.Tag("runtime/serve/Idempotency")<Idempotency, {
  readonly claim: <A, I>(key: typeof _IdempotencyKey.Type, digest: string, outcome: Schema.Schema<A, I, never>) => Effect.Effect<Idempotency.Claim<A>, GateFault>
}>() {
  static readonly Claim = _Claim
  static readonly memory = (retention: Duration.Duration): Layer.Layer<Idempotency> =>
    Layer.effect(
      Idempotency,
      Effect.map(Ref.make(HashMap.empty<string, _Cell>()), (cells) => ({
        claim: <A, I>(key: typeof _IdempotencyKey.Type, digest: string, outcome: Schema.Schema<A, I, never>) =>
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
                Effect.succeed<Idempotency.Claim<A>>(_Claim.Fresh({
                  settle: (value: A) => Deferred.succeed(slot, value).pipe(Effect.asVoid),
                })),
              onSome: (cell) =>
                cell.digest === digest
                  ? Effect.succeed<Idempotency.Claim<A>>(_Claim.Replay({
                      outcome: Deferred.await(cell.slot).pipe(
                        Effect.flatMap(Schema.validate(outcome)),
                        Effect.mapError(() => new GateFault({ reason: "conflict", detail: "idempotency-key outcome divergence", retryAfter: Option.none() })),
                      ),
                    }))
                  : Effect.fail(new GateFault({ reason: "conflict", detail: "idempotency-key payload mismatch", retryAfter: Option.none() })),
            })
          }),
      })),
    )
  static readonly persisted = <Req extends Schema.TaggedRequest.Any>(options: {
    readonly storeId: string
    readonly retention: Duration.Duration
    readonly execute: (request: Req) => Effect.Effect<Schema.WithResult.Success<Req>, Schema.WithResult.Failure<Req>>
  }): Effect.Effect<
    (request: Req) => Effect.Effect<
      Schema.WithResult.Success<Req>,
      Schema.WithResult.Failure<Req> | Persistence.PersistenceError
    >,
    never,
    Persistence.ResultPersistence | Scope.Scope
  > =>
    Effect.map(
      PersistedCache.make({
        storeId: options.storeId,
        lookup: options.execute,
        timeToLive: () => options.retention,
      }),
      (cache) => (request: Req) => cache.get(request),
    )
}

declare namespace Idempotency {
  type Claim<A> = _Claim<A>
}

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
    Effect.map(
      RateLimiter.make({ limit: pressure.window.limit, interval: pressure.window.interval, algorithm: "token-bucket" }),
      (limiter) =>
        <A, E, R>(self: Effect.Effect<A, E, R>) =>
          limiter(self).pipe(
            Effect.timeoutFail({
              duration: pressure.grace,
              onTimeout: () => new GateFault({ reason: "rate", detail: "window", retryAfter: Option.some(pressure.grace) }),
            }),
          )),
  fenced: <A, E, R>(
    self: Effect.Effect<A, E, R>,
    key: string,
    pressure: Gate.Pressure,
  ): Effect.Effect<A, GateFault | E, R | Fleet.RateLimiter> =>
    Fleet.makeWithRateLimiter({
      key,
      window: pressure.window.interval,
      limit: pressure.window.limit,
      onExceeded: "fail",
    })(self).pipe(
      Effect.catchTag("RateLimitExceeded", () =>
        Effect.fail(new GateFault({ reason: "rate", detail: key, retryAfter: Option.some(pressure.window.interval) }))),
      Effect.catchTag("RateLimitStoreError", (fault) => Effect.die(fault)),
    ),
} as const
```

## [6]-[CONTRIBUTION]

[CONTRIBUTION]:
- Owner: `Contribution` — the pairing law as two constructors: `Contribution.http(group, handlers)` pairs an `HttpApiGroup` with its handler builder — a function OF the assembled api, because `HttpApiBuilder.group(api, name, build)` demands the api value only the app holds, the mechanical fact that makes the god-contract impossible; `Contribution.rpc(group, handlers)` pairs an `RpcGroup` with the handler Layer its `toLayer` already built, because RPC handlers bind to the group alone.
- Law: the app assembly is three chained folds stated here as law — `HttpApi.make(id).add(a.group).add(b.group)` builds the one api value; each http row's `handlers(api)` Layer merges under `Layer.provide` into `HttpApiBuilder.api(api)`; each rpc row's group merges through `group.merge(other)` into one served group — and the assembled values exist only in the app's composition root, with `route#SERVE_FOLD` consuming the resulting Layer.
- Law: `Contribution.protocols` crossed with `Contribution.codecs` is the RPC serve roster — protocol rows `http` and `websocket` as path-parameterized factories, `socket` as the raw-socket-server row, `worker` as the runner row whose typed boot handshake is `RpcWorker.layerInitialMessage(schema, build)`, `stdio` as the child-process/MCP transport over its stdin Stream and stdout Sink — crossed with serialization rows (`json`, `ndjson`, `msgpack`) selected once at the app root; a transport or codec choice inside a handler, or a procedure re-declared per transport, is the named defect.
- Law: procedure rows carry their own semantics as `Rpc.make` options and wrappers — `primaryKey` states the request-dedup identity where a procedure is idempotent by value, `Rpc.fork` marks a fire-and-forget handler that answers without occupying the mailbox, `Rpc.uninterruptible` marks a settle that must not be torn by client disconnect — each a declaration on the contributed row, never a handler-interior branch.
- Law: the RPC arm carries its own principal-providing admission — `RpcMiddleware.Tag` with `failure`/`provides` defines the auth middleware once for both ends, `RpcGroup`'s `.middleware` scopes it to the contributed procedures, and `RpcMiddleware.layerClient` supplies the client arm where `requiredForClient` demands it — so the HTTP `Authn` and the RPC admission provide the same `Principal` and neither arm ships unauthenticated by omission.
- Law: streaming procedures declare `stream: true` on `Rpc.make` and nothing else — the protocol row frames chunks and exit; hand-framing a stream over a unary procedure is rejected on sight.
- Law: upload endpoints are declared modality — `HttpApiSchema.Multipart(schema)` on the endpoint payload types the parts, `Multipart.withLimits` bounds bytes and part count as fiber-ref policy at the seam, and file parts decode through `Multipart.toPersisted` / `Multipart.schemaPersisted(schema)` so a persisted file part hands into the data rail's byte lift as a scoped filesystem fact; an untyped `request.multipart` read in a handler is the deleted spelling.
- Boundary: group-exercising specs ride `RpcTest.makeClient(group)` — the transport-free in-memory client — so a contributed group proves its handlers with zero protocol Layers; serve-row selection and mounting are `route`'s; derived surfaces are `[07]`'s.
- Growth: a new entry family (a queue consumer surface, a cron surface) is one new pairing constructor on this owner under the same shape — group as data, handlers as Layer or reader — never a new assembly law.
- Packages: `@effect/platform` (`HttpApi`, `HttpApiBuilder`); `@effect/rpc` (`Rpc`, `RpcGroup`, `RpcServer`, `RpcSerialization`, `RpcMiddleware`); `effect` (`Layer`).

```typescript
declare namespace Contribution {
  type Http<G, Api, Out, E, R> = {
    readonly _tag: "Http"
    readonly group: G
    readonly handlers: (api: Api) => Layer.Layer<Out, E, R>
  }
  type Remote<G, Out, E, R> = {
    readonly _tag: "Remote"
    readonly group: G
    readonly handlers: Layer.Layer<Out, E, R>
  }
  type Protocol = keyof typeof _protocols
  type Codec = keyof typeof _codecs
}

const _protocols = {
  http: (path: `/${string}`) => RpcServer.layerProtocolHttp({ path }),
  websocket: (path: `/${string}`) => RpcServer.layerProtocolWebsocket({ path }),
  socket: () => RpcServer.layerProtocolSocketServer,
  worker: () => RpcServer.layerProtocolWorkerRunner,
  stdio: (options: Parameters<typeof RpcServer.layerProtocolStdio>[0]) => RpcServer.layerProtocolStdio(options),
} as const

const _codecs = {
  json: RpcSerialization.layerJson,
  ndjson: RpcSerialization.layerNdjson,
  msgpack: RpcSerialization.layerMsgPack,
} as const

const Contribution: {
  readonly http: <const G, Api, Out, E, R>(group: G, handlers: (api: Api) => Layer.Layer<Out, E, R>) => Contribution.Http<G, Api, Out, E, R>
  readonly rpc: <const G, Out, E, R>(group: G, handlers: Layer.Layer<Out, E, R>) => Contribution.Remote<G, Out, E, R>
  readonly protocols: typeof _protocols
  readonly codecs: typeof _codecs
} = {
  http: (group, handlers) => ({ _tag: "Http", group, handlers }),
  rpc: (group, handlers) => ({ _tag: "Remote", group, handlers }),
  protocols: _protocols,
  codecs: _codecs,
}
```

## [7]-[EMIT]

[EMIT]:
- Owner: `Emit` — the derivation surface over the app-assembled value, parameterized on it, never importing it. `Emit.artifact` is the canonical spec artifact: `OpenApi.fromApi(api)` serialized with sorted keys and fixed indentation so two emissions of one contract are byte-identical and the contract gate diffs bytes, never re-parses; the `cli` inspect verb and the drift check consume this one member. `Emit.docs(ui)` is the served documentation stack — `HttpApiBuilder.middlewareOpenApi()` (the document route derived from the served api) merged with the reference UI the `ui` row selects (`scalar` → `HttpApiScalar.layer()`, `swagger` → `HttpApiSwagger.layer()`) — one Layer the app root selects; `HttpApiScalar.layerHttpLayerRouter` is the same row route-natively when the api mounts under `route#LAYER_ROUTES`.
- Law: the security requirements in the emitted document are the declared schemes — `Authn`'s `security` record flows into the spec through the api value, so the published contract states bearer and API-key admission from the same declaration that enforces it; a hand-authored securitySchemes block restates what the declaration already emits.
- Law: `Emit.client` derives the typed HTTP SDK through `HttpApiClient.make(api, { baseUrl, transformClient })` with the transform slot carrying the shared egress posture (`client#DIAL_SEAM`'s tempering), so a derived consumer inherits the same resilience as every other outbound call; `Emit.caller` is the RPC peer — `RpcClient.make(group)` under one `RpcClient.layerProtocolHttp({ url })` row — so in-repo service-to-service callers derive from the same contributed group and a hand-written fetch client beside a contract is unspellable. The client faults are the declared faults: each endpoint's `addError` family plus transport and decode, one error vocabulary spanning the wire.
- Law: RPC egress is trace-continuous — `Emit.traced(call)` stamps the live span's W3C headers onto the call through `RpcClient.withHeaders` (the `RpcClient.currentHeaders` FiberRef beneath it), so a derived RPC call carries `traceparent` exactly as `HttpClient.withTracerPropagation` does for HTTP and a distributed hop never drops causality.
- Law: the web-handler edge form is the platform surface composed at the app root — `HttpApiBuilder.toWebHandler(api, options)` yields the `Request => Response` arrow for fetch-shaped runtimes over the same assembled value, and no `Emit` member renames it because a forwarding member is the one-hop wrapper this corpus deletes; the full-server form (api beside raw routes) is `route#SERVE_FOLD`'s `HttpLayerRouter.toWebHandler`.
- Law: derivation is call-time and parameterized — nothing here caches, names, or holds an api instance, keeping the assembled value's no-lib-side-existence law intact; contract documentation is annotation material on the api value (`HttpApi.make(id).annotate`, endpoint schema annotations) flowing into the document through the derivation.
- Growth: a new documentation surface (a JSON-schema bundle per owner, a second reference UI) is one derivation member over the same api parameter.
- Packages: `@effect/platform` (`OpenApi`, `HttpApiBuilder`, `HttpApiScalar`, `HttpApiClient`); `@effect/rpc` (`RpcClient`); `effect` (`Layer`, `Array`, `Record`, `Order`, `Predicate`).

```typescript
const _byKey: Order.Order<readonly [string, unknown]> = Order.mapInput(
  Order.string,
  (entry: readonly [string, unknown]) => entry[0],
)

const _stable = (value: unknown): unknown =>
  Array.isArray(value)
    ? Array.map(value, _stable)
    : Predicate.isRecord(value)
      ? Record.fromEntries(pipe(
          Record.toEntries(value),
          Array.map(([key, held]) => [key, _stable(held)] as const),
          Array.sortBy(_byKey),
        ))
      : value

const _artifact = <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
  api: HttpApi.HttpApi<Id, Groups, E, R>,
): string => JSON.stringify(_stable(OpenApi.fromApi(api)), null, 2)

const _uis = {
  scalar: () => HttpApiScalar.layer(),
  swagger: () => HttpApiSwagger.layer(),
} as const

const _docs = (ui: keyof typeof _uis = "scalar"): Layer.Layer<never, never, HttpApi.Api> =>
  Layer.mergeAll(HttpApiBuilder.middlewareOpenApi(), _uis[ui]())

const _client = <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
  api: HttpApi.HttpApi<Id, Groups, E, R>,
  options: {
    readonly baseUrl: string
    readonly transform: (client: HttpClient.HttpClient) => HttpClient.HttpClient
  },
) => HttpApiClient.make(api, { baseUrl: options.baseUrl, transformClient: options.transform })

const _caller = <G extends RpcGroup.RpcGroup.Any>(group: G, origin: { readonly url: string }) =>
  Effect.provide(RpcClient.make(group), RpcClient.layerProtocolHttp({ url: origin.url }))

const _traced = <A, E, R>(call: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
  Effect.optionFromOptional(Effect.currentSpan).pipe(
    Effect.flatMap(Option.match({
      onNone: () => call,
      onSome: (span) => RpcClient.withHeaders(call, HttpTraceContext.toHeaders(span)),
    })),
  )

const Emit = {
  artifact: _artifact,
  caller: _caller,
  client: _client,
  docs: _docs,
  traced: _traced,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Contribution, Convention, Current, Emit, Gate, GateFault, Principal }
```
