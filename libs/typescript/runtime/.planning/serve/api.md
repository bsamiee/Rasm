# [RUNTIME_API]

The one public front door's declarative engine: a domain folder exports its `HttpApiGroup` or `RpcGroup` as data, the APP — never this module, never any lib module — assembles exactly one `HttpApi` value and crosses exactly one RPC protocol row with one serialization row at its root, and every secondary surface — the OpenAPI 3.1 document, the served Scalar reference UI, the byte-stable spec artifact, the typed HTTP SDK, the typed RPC caller, the fetch-shaped web handler — projects from that same assembled value so spec, docs, client, and server cannot drift. Auth is declarative into the emitted contract: the `Authn` middleware Tag carries its `HttpApiSecurity` schemes, so the bearer and API-key security requirements land in the OpenAPI document from the same declaration the handler set enforces, and a protected group's handlers receive `Principal` from the requirement channel. Every refusal is one `GateFault` whose reason row carries the core fault class the `problem` record answers a status for; the ambient request rows — stamp, negotiated locale, the seam's one credential lift — are `Context.Reference` values any rail reads at zero requirement pressure, and the request's tenancy binds through the security wave's own reference from that lift. The god-contract is structurally impossible because `HttpApiBuilder.group` demands the assembled api value the lib never holds. The module ships on the `./server` exports subpath as `runtime/src/serve/api.ts`.

## [01]-[INDEX]

- [02]-[SURFACE]: version-prefix rows, the cursor brand, page-query and page-envelope constructors; `Surface`.
- [03]-[GATE_FAULT]: the reason-discriminated refusal family over the core family seam, the refusal fan; `GateFault`.
- [04]-[CURRENT_ROWS]: ambient stamp/locale/admitted references, locale negotiation, trace continuation; `Current`.
- [05]-[ADMISSION_ROWS]: `Principal`, the one credential lift, the HTTP and RPC admission Tags, pressure, idempotency; `Principal`, `Gate`.
- [06]-[CONTRIBUTION]: the http and rpc pairing constructors, protocol and codec rosters, upload modality; `Contribution`.
- [07]-[EMIT]: spec artifact, docs stack, derived HTTP and RPC clients, the web-handler edge form; `Emit`.

## [02]-[SURFACE]

[SURFACE]:
- Owner: `Surface` — the shared surface vocabulary both contribution families speak: the version tuple (`v1`; a new major is one tuple entry every consumer inherits), the opaque `Cursor` brand, the `PageParams` query schema (cursor as `Option`, limit defaulted and ceiling-bounded at the declaration so no handler re-checks bounds), and `Surface.page(item)` — one generic constructor deriving the page envelope for any item schema, so a per-shape page schema cannot exist.
- Law: `Surface` names the served-surface vocabulary alone, leaving `Convention` to the core telemetry owner this branch already imports.
- Law: pagination is cursor-only — the cursor is minted by the owning read surface, opaque to the caller, bounded at the brand; `next` absent means exhausted, spelled `Option` by `optionalWith`; offset pagination has no vocabulary here and cannot be contributed.
- Law: the version prefix attaches at the group — `group.prefix(Surface.prefix("v1"))` — so a group is versioned as contributed data and two majors of one group coexist as two contributions; a version segment hand-written into an endpoint path is the drift defect.
- Growth: a new convention axis (sort grammar, field selection) is one schema row on this owner, inherited by every group that composes it.
- Packages: `effect` (`Schema`, `Option`).

```typescript
import {
  Headers, HttpApi, HttpApiBuilder, HttpApiClient, type HttpApiGroup, HttpApiMiddleware, HttpApiScalar,
  HttpApiSecurity, HttpApiSwagger, type HttpClient, type HttpLayerRouter, HttpTraceContext, OpenApi, Socket,
} from "@effect/platform"
import { PersistedCache, type Persistence, RateLimiter as Fleet } from "@effect/experimental"
import { RpcClient, type RpcGroup, RpcMiddleware, RpcSerialization, RpcServer } from "@effect/rpc"
import {
  Array, Context, DateTime, Deferred, Duration, Effect, Exit, HashMap, Layer, Metric, Number, Option, Order, Predicate,
  RateLimiter, Record, Redacted, Ref, Schema, type Scope, pipe,
} from "effect"
import { Identity, Carrier, Convention, Fault, Shape } from "@rasm/ts/core"
import { ApiKey, Claim, Jwt, type Principal as Scoped, Session, TenantScope } from "@rasm/ts/security"
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

declare namespace Surface {
  type Version = (typeof _VERSIONS)[number]
  type Cursor = typeof _Cursor.Type
  type PageParams = typeof _PageParams.Type
}

const Surface: {
  readonly versions: typeof _VERSIONS
  readonly Cursor: typeof _Cursor
  readonly PageParams: typeof _PageParams
  readonly page: typeof _page
  readonly prefix: <V extends Surface.Version>(version: V) => `/${V}`
} = {
  versions: _VERSIONS,
  Cursor: _Cursor,
  PageParams: _PageParams,
  page: _page,
  prefix: (version) => `/${version}`,
}
```

## [03]-[GATE_FAULT]

[GATE_FAULT]:
- Law: the family is sized by refusal route, never by cause — `unauthorized` (credential absent or unverifiable) classifies `expired`, `forbidden` (verified but insufficient) classifies `denied`, `shed` (the door cannot admit right now — the in-flight cap refused, or a credential store the lift needed answered as unreachable infrastructure) classifies `unavailable`, `rate` (window exhausted) classifies `exhausted`, `conflict` (idempotency key replayed against a different payload, a diverged replay, or a released origin) classifies `conflicted` — a finer cause is `detail` text, never a sixth reason minted for one surface, because the route a refusal takes outward is the whole partition.
- Law: the refusal code falls out of the class alone — every reason's core row already carries the response code `problem#STATUS_RECORD` governs (401, 403, 503, 429, 409 in row order), so no serve-local `status` column and no `policy` probe exist to disagree with the branch's one class-to-status record.
- Law: `retryAfter` is an `Option<Duration>` stamped by the pressure rows from their own measured window — the grace hint the `problem` ladder prefers over the class default — so a 429/503 always carries the truthful window, never a guessed constant.
- Law: every refusal emits before it fails — `_refuse` is the one fail seam, incrementing `Convention.metric.admitRefused` tagged by the reason the fault already carries, so the refusal fan needs zero call-site wiring and a new reason joins the series the moment its row lands.
- Packages: `effect` (`Schema`, `Option`, `Duration`, `Metric`); `@rasm/ts/core` (`Fault.Class`, `Convention`).

```typescript
// One row per reason: the core kind alone. Retryability, blame, and the response code are the core row
// table's and problem#STATUS_RECORD's — `unauthorized` classifies `expired` (401, a credential the door
// will not accept) where `forbidden` classifies `denied` (403, a credential it accepted and refused).
const _gate = Fault.Class.family(["unauthorized", "forbidden", "shed", "rate", "conflict"] as const, {
  unauthorized: { class: "expired" },
  forbidden: { class: "denied" },
  shed: { class: "unavailable" },
  rate: { class: "exhausted" },
  conflict: { class: "conflicted" },
})

declare namespace GateFault {
  type Reason = (typeof _gate.reasons)[number]
}

class GateFault extends Schema.TaggedError<GateFault>()("GateFault", {
  reason: _gate.schema,
  detail: Schema.String,
  retryAfter: Schema.optionalWith(Schema.DurationFromSelf, { as: "Option" }),
}) {
  get class(): Fault.Class.Kind {
    return _gate.classOf(this.reason)
  }
  override get message(): string {
    return `<gate:${this.reason}> ${this.detail}`
  }
}

const _refused = Convention.mount(Convention.metric.admitRefused)

// the closed reason partition IS the axis fan, so the one fail seam carries the whole refusal series
const _refuse = (fault: GateFault): Effect.Effect<never, GateFault> =>
  Effect.zipRight(
    Metric.increment(Metric.tagged(_refused, Convention.rasm.admitReason, fault.reason)),
    Effect.fail(fault),
  )
```

## [04]-[CURRENT_ROWS]

[CURRENT_ROWS]:
- Owner: `Current` — the ambient request rows as `Context.Reference` classes: `Current.Stamp` carries `Option` of the per-request mark (`id`, `at`, locale), `Current.Locale` carries the negotiated `Shape.Refined.Locale` with the fleet default answering when no request provided one, and `Current.Admitted` carries `Option` of the one credential lift the serving seam performed — three rows, each readable from any rail at zero requirement pressure, overridden per request by scoped provision at the route seam.
- Law: locale negotiation is one fold — `Current.negotiate(header, fallback)` splits the `Accept-Language` list, ranks by `q` weight descending, and takes the first tag the core `Shape.Refined.Locale` schema admits — a malformed tag or an empty header lands on the fallback and negotiation can never fail; the negotiated value is BCP-47-canonical by the core brand's own filter.
- Law: trace continuation is composed, never re-derived — `Current.traced(effect, headers)` normalizes the request record once through core `Carrier.extract("http", ...)` and delegates the resulting context to `otel/emit#CONTINUATION`'s one ingress transformer, so extract-and-continue at the HTTP door is the same transformer every other ingress composes (baggage annotations arrive pre-scrubbed by that owner) and a second `traceparent` decode cannot exist here.
- Law: the stamp mints at the door — `Current.provide(effect, mark, fallback)` provides the mark and the locale projected from it in one scoped provision, so a handler, a log annotation, and the problem fold read one coherent request identity; the `problem` page reads `Current.Stamp` for the `instance` member and the `requestId` extension.
- Law: `Current.Admitted` is the credential lift's one seat — the seam resolves a presented credential exactly once per request and provides the result here, so `Authn`'s scheme arms PROJECT that lift instead of re-verifying: a protected endpoint pays one signature check and at most one KDF descent no matter how many schemes the contract declares.
- Growth: a new ambient axis is one `Context.Reference` row plus its projection inside `provide`.
- Packages: `effect` (`Context`, `Option`, `Schema`, `Array`, `Order`, `Number`); `@rasm/ts/core` (`Shape.Refined`); `../otel/emit.ts` (`Propagation`).

```typescript
const _byWeight: Order.Order<readonly [string, number]> = Order.mapInput(
  Order.reverse(Order.number),
  (pair: readonly [string, number]) => pair[1],
)

const _negotiate = (header: Option.Option<string>, fallback: Shape.Refined.Locale): Shape.Refined.Locale =>
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
    Array.filterMap(([tag]) => Option.getRight(Schema.decodeUnknownEither(Shape.Refined.Locale)(tag))),
    Array.head,
    Option.getOrElse(() => fallback),
  )

class _Stamp extends Context.Reference<_Stamp>()("runtime/serve/Current/Stamp", {
  defaultValue: () => Option.none<Current.Mark>(),
}) {}

class _Locale extends Context.Reference<_Locale>()("runtime/serve/Current/Locale", {
  defaultValue: () => Schema.decodeUnknownSync(Shape.Refined.Locale)("en"),
}) {}

// the seam's one credential lift lands here before dispatch; `none` is an anonymous request, never a failure
class _Admitted extends Context.Reference<_Admitted>()("runtime/serve/Current/Admitted", {
  defaultValue: () => Option.none<Authn.Admitted>(),
}) {}

const _provide = <A, E, R>(
  self: Effect.Effect<A, E, R>,
  mark: Current.Mark,
  fallback: Shape.Refined.Locale,
): Effect.Effect<A, E, R> =>
  self.pipe(
    Effect.provideService(_Stamp, Option.some(mark)),
    Effect.provideService(_Locale, Option.getOrElse(mark.locale, () => fallback)),
  )

const _traced = <A, E, R>(
  self: Effect.Effect<A, E, R>,
  headers: { readonly [key: string]: string | undefined },
): Effect.Effect<A, E, R> =>
  Propagation.ingress(
    self,
    Carrier.extract(
      "http",
      Record.fromEntries(Array.filterMap(Record.toEntries(headers), ([key, value]) =>
        Option.map(Option.fromNullable(value), (held) => [key.toLowerCase(), held] as const))),
    ),
  )

declare namespace Current {
  type Admitted = _Admitted
  type Mark = {
    readonly id: string
    readonly at: DateTime.Utc
    readonly locale: Option.Option<Shape.Refined.Locale>
  }
}

const Current: {
  readonly Admitted: typeof _Admitted
  readonly Locale: typeof _Locale
  readonly Stamp: typeof _Stamp
  readonly negotiate: typeof _negotiate
  readonly provide: typeof _provide
  readonly traced: typeof _traced
} = { Admitted: _Admitted, Locale: _Locale, Stamp: _Stamp, negotiate: _negotiate, provide: _provide, traced: _traced }
```

## [05]-[ADMISSION_ROWS]

[ADMISSION_ROWS]:
- Law: `Authn.admit(identity, headers)` is the ONE credential lift and it is the ONE place a verified credential first exists — the bearer arm verifies through `Jwt.verify` into `AccessClaims` then resolves `Claim.resolve` into a `ClaimSet` and `Claim.principal(identity, claims)` into the security `Principal`, the apiKey arm resolves through `ApiKey.resolve` into an `ApiKeyRecord` under the unscoped principal, and both lift into one `Admitted` pair carrying the served identity beside the tenancy binding. A request presenting nothing answers `Option.none()`, because credential refusal is the endpoint's decision, not the edge's.
- Law: `Authn.live`'s scheme arms PROJECT, never verify — each arm reads `Current.Admitted`, admits the row whose `via` matches its own scheme, and refuses `unauthorized` otherwise, so a declared-but-unpresented scheme costs nothing and no contract can make one request pay two verifications; attachment stays `.middleware(Gate.Authn)` on the contributed group, and verification failure carries generic detail because the evidence rides telemetry, never the 401 body.
- Law: tenancy binds at that same lift and nowhere else — `Admitted.scope` is the security `Principal` the seam hands to `TenantScope.bind` and `TenantScope.metered`, so every `SessionCoordinate` GUC, every RLS predicate, and every security instrument's tenant dimension resolve from the one credential the edge proved; this module holds no tenancy value of its own.
- Law: `_lifted` binds its error parameter to the classed shape, so its two-way partition is total by construction — every port fault it folds mints through `Fault.Class.family` and carries the `class` getter `Fault.Class.of` probes, and the probe reads `property in self`, so a prototype accessor answers. Any port contract failing with a bare `Error` refuses to compile at this fold rather than grading `defect`, whose `system` blame answers `shed` 503 where the presented credential earns 401.
- Law: the admission plane measures itself off its own partitions — `admitPassed` counts each lift tagged by the `via` scheme so the refusal series has a denominator, `admitRefused` fans on the `GateFault` reason at the one `_refuse` seam, and `idempotencyOutcome` fans on the bracket's own three-way fold; every instrument mounts from its `Convention` row, so this page carries no bucket ladder and no constructor pick.
- Law: `RpcAuthn` is the same admission on the RPC arm — `RpcMiddleware.Tag` with `failure: GateFault`, `provides: Principal`, `requiredForClient: true`, and `wrap: true`, so one definition governs both ends: the wrap reads the frame headers, composes the same `Authn.admit` lift, provides the same `Principal` into `next`, and binds `TenantScope` around it exactly as the HTTP seam does. `requiredForClient` is what makes `Emit.caller` refuse to derive a credential-less client, and `RpcMiddleware.layerClient` is the client arm the app root supplies; a `Contribution.rpc` group scoping this Tag through `.middleware` cannot ship unauthenticated by omission.
- Law: pressure rows bound two distinct axes — `Gate.shed` brackets a section under an in-flight cap whose refusal is immediate (`withPermitsIfAvailable` settling `Option.none` under saturation folds to `shed` with the declared grace: the queue-depth 503 lever), `Gate.window` prices calls against a scoped in-process `RateLimiter.make` row (the 429 lever) whose grace deadline bounds the TOKEN WAIT alone — the admitted work never races its own timeout, because the deadline gates the acquisition probe and the work sequences after it — conflating concurrency and throughput is the named selection error; both stamp `retryAfter` from their own measured window, and policy is one `Gate.Pressure` value row, never threaded knobs.
- Law: the distributed quota row is port-shaped by Layer — `Gate.fenced` yields the experimental accessor `Fleet.makeWithRateLimiter` (an `Effect` reading the `RateLimiter.RateLimiter` Tag the app root satisfies with `layerStoreMemory` on one node or a store-backed Layer on a fleet) and applies its transformer; both experimental faults share the one `"RateLimiterError"` tag discriminated by `reason` — the `"Exceeded"` arm re-spells as `rate` carrying the fault's own measured `retryAfter`, and the `"StoreError"` arm dies as a defect because a broken quota backend is never a caller 429.
- Law: `Idempotency` is one polymorphic bracket, never a claim ceremony — `run(key, digest, outcome, execute)` owns the whole fresh/replay fold, so a handler composes one call and never orchestrates claim, settle, or park: the first execution per key runs `execute` and settles the cell with its value; a same-digest duplicate parks on the cell and replays the settled value re-proven through `Schema.validate(outcome)` (the fast lane carries the same schema evidence the fleet tier's `Schema.TaggedRequest` carries), a diverged replay refusing as `conflict`; any non-success exit settles every parked duplicate with a typed `conflict` refusal and conditionally releases only its own cell — no duplicate can hang on an interrupted or defective origin, an expired origin cannot delete a newer claimant's cell, the origin's own exit propagates unchanged, and the next claimant executes fresh; a replayed key whose payload digest differs refuses as `conflict` before any wait. `Idempotency.memory(retention)` is the single-node Layer sweeping expired cells inside the same atomic claim; the key admits through the `Gate.IdempotencyKey` brand at the header seam, and a GET carrying the header is ignored, never refused.
- Law: the fleet tier is `Idempotency.persisted` — `PersistedCache.make({ storeId, lookup, timeToLive })` over the store-owned `Persistence.layerResultKeyValueStore`, keyed by a `Schema.TaggedRequest` whose `PrimaryKey` fuses idempotency key and payload digest, so the first execution's exit persists for the retention window, every fleet duplicate replays the stored exit typed through the request's own success/failure schemas, and a divergent payload is a different key that executes fresh; the strict 409 divergence posture stays the memory gate composed in front, so both tiers ride one root and zero handler change.
- Growth: a third credential scheme is one `security` record entry, one `_admit` arm, and one `via` literal; a fleet quota engine is a Layer swap on the `Idempotency` or limiter Tag at the root.

```typescript
class _Principal extends Schema.Class<_Principal>("Principal")({
  subject: Schema.NonEmptyString,
  session: Schema.optionalWith(Session.fields.id, { as: "Option" }),
  // the core owner's own field schema, so a tenant crossing this seam is unspellable as a bare string
  tenant: Schema.optionalWith(Identity.Tenant.fields.tenant, { as: "Option" }),
  scopes: Schema.Array(Schema.NonEmptyString),
  via: Schema.Literal("session", "apikey"),
}) {}

class Principal extends Context.Tag("runtime/serve/Principal")<Principal, _Principal>() {
  static readonly Shape = _Principal
  static readonly allows = (principal: _Principal, scope: string): boolean => Array.contains(principal.scopes, scope)
}

declare namespace Principal {
  type Shape = _Principal
}

const _passed = Convention.mount(Convention.metric.admitPassed)

const _BEARER = /^Bearer\s+(.+)$/i

// the two presented forms as data: a scheme is a row carrying its header read, so a third scheme joins the
// lift, the OpenAPI security record, and the `via` vocabulary together or not at all
const _schemes = {
  session: (headers: Headers.Headers) =>
    Option.map(
      Option.flatMapNullable(Option.fromNullable(headers.authorization), (raw) => _BEARER.exec(raw)?.[1]),
      Redacted.make,
    ),
  apikey: (headers: Headers.Headers) => Option.map(Option.fromNullable(headers["x-api-key"]), Redacted.make),
} as const satisfies Record<_Principal["via"], (headers: Headers.Headers) => Option.Option<Redacted.Redacted<string>>>

// `Effect.option` folds EVERY failure to an absent credential, so a claim store that is DOWN answers 401 and asks the
// caller to present something else. The core lattice already separates the two: a caller-blamed class is evidence
// ABOUT the presented credential, and anything else is this deployment's own dependency refusing to answer. Binding
// `E` to the classed shape makes that partition structural: an `unknown` channel grades every unclassed port fault
// `defect`, blaming this deployment for a credential the caller mis-presented, and states nothing in the type.
const _lifted = <A, E extends { readonly class: Fault.Class.Kind }>(
  self: Effect.Effect<A, E, Jwt | ApiKey | Claim>,
): Effect.Effect<Option.Option<A>, GateFault, Jwt | ApiKey | Claim> =>
  Effect.matchEffect(self, {
    onFailure: (fault) =>
      Fault.Class.at(Fault.Class.of(fault)).blame === "caller"
        ? Effect.succeedNone
        // no hint: the class default is the truthful window, and the problem ladder already prefers a measured one
        : _refuse(new GateFault({ reason: "shed", detail: "credential store unreachable", retryAfter: Option.none() })),
    onSuccess: Effect.succeedSome,
  })

const _admit = (
  identity: Identity.App,
  headers: Headers.Headers,
): Effect.Effect<Option.Option<Authn.Admitted>, GateFault, Jwt | ApiKey | Claim> =>
  Effect.gen(function* () {
    const jwt = yield* Jwt
    const keys = yield* ApiKey
    const claim = yield* Claim
    const bearer = _schemes.session(headers)
    const key = _schemes.apikey(headers)
    // the bearer arm is the only tenancy source: an api key resolves a machine subject under the unscoped
    // principal, exactly as `Claim.principal` projects an absent tenant
    return yield* Option.match(bearer, {
      onSome: (token) =>
        jwt.verify(token).pipe(
          // the token carries the live session id the ClaimSet does not, so both halves ride forward
          Effect.flatMap((verified) => Effect.map(claim.resolve(verified), (claims) => ({ verified, claims }))),
          Effect.map(({ claims, verified }): Authn.Admitted => ({
            principal: new _Principal({
              subject: claims.subject,
              session: Option.some(verified.sid),
              tenant: claims.tenant,
              scopes: verified.scope,
              via: "session",
            }),
            scope: claim.principal(identity, claims),
          })),
          _lifted,
        ),
      onNone: () =>
        Option.match(key, {
          onNone: () => Effect.succeedNone,
          onSome: (presented) =>
            keys.resolve(presented).pipe(
              Effect.map((record): Authn.Admitted => ({
                principal: new _Principal({
                  subject: record.subject,
                  session: Option.none(),
                  tenant: Option.none(),
                  scopes: record.scopes,
                  via: "apikey",
                }),
                scope: { context: Option.none(), subject: Option.some(record.subject) },
              })),
              _lifted,
            ),
        }),
    })
  }).pipe(
    Effect.tap(Option.match({
      onNone: () => Effect.void,
      onSome: ({ principal }) =>
        Metric.increment(Metric.tagged(_passed, Convention.rasm.admitScheme, principal.via)),
    })),
  )

// each arm PROJECTS the seam's single lift: a declared scheme the caller never presented costs nothing,
// and no contract can make one request pay two verifications
const _projected = (via: _Principal["via"]): Effect.Effect<_Principal, GateFault> =>
  Effect.flatMap(_Admitted, (held) =>
    Option.match(Option.filter(held, (admitted) => admitted.principal.via === via), {
      onNone: () => _refuse(new GateFault({ reason: "unauthorized", detail: via, retryAfter: Option.none() })),
      onSome: (admitted) => Effect.succeed(admitted.principal),
    }))

declare namespace Authn {
  type Admitted = { readonly principal: Principal.Shape; readonly scope: Scoped }
}

class Authn extends HttpApiMiddleware.Tag<Authn>()("runtime/serve/Authn", {
  failure: GateFault,
  provides: Principal,
  security: {
    bearer: HttpApiSecurity.bearer,
    apiKey: HttpApiSecurity.apiKey({ in: "header", key: "x-api-key" }),
  },
}) {
  static readonly admit = _admit
  static readonly live: Layer.Layer<Authn> = Layer.succeed(Authn, {
    bearer: () => _projected("session"),
    apiKey: () => _projected("apikey"),
  })
}

class RpcAuthn extends RpcMiddleware.Tag<RpcAuthn>()("runtime/serve/RpcAuthn", {
  failure: GateFault,
  provides: Principal,
  requiredForClient: true,
  wrap: true,
}) {
  static readonly live = (identity: Identity.App): Layer.Layer<RpcAuthn, never, Jwt | ApiKey | Claim> =>
    Layer.effect(
      RpcAuthn,
      Effect.map(Effect.context<Jwt | ApiKey | Claim>(), (held) => ({ headers, next }) =>
        _admit(identity, headers).pipe(
          Effect.flatMap(Option.match({
            onNone: () => _refuse(new GateFault({ reason: "unauthorized", detail: "rpc", retryAfter: Option.none() })),
            // one binding site on this arm too: the frame's downstream runs under the same tenancy the
            // HTTP seam binds, so a procedure and an endpoint pin identical session coordinates
            onSome: (admitted) =>
              TenantScope.metered(TenantScope.bind(
                admitted.scope,
                Effect.provideService(next, Principal, admitted.principal),
              )),
          })),
          Effect.provide(held),
        )),
    )
  // the client arm `requiredForClient` demands: the credential stamps onto the outbound frame's own headers,
  // so `Emit.caller` cannot derive a peer that ships without one
  static readonly caller = (credential: Headers.Headers): Layer.Layer<RpcMiddleware.ForClient<RpcAuthn>> =>
    RpcMiddleware.layerClient(RpcAuthn, ({ request }) =>
      Effect.succeed({ ...request, headers: Headers.merge(request.headers, credential) }))
}

const _IdempotencyKey = Schema.NonEmptyString.pipe(
  Schema.maxLength(128),
  Schema.pattern(/^[A-Za-z0-9_-]+$/),
  Schema.brand("IdempotencyKey"),
)

type _Cell = { readonly digest: string; readonly slot: Deferred.Deferred<unknown, GateFault>; readonly at: DateTime.Utc }

const _conflict = (detail: string): GateFault => new GateFault({ reason: "conflict", detail, retryAfter: Option.none() })

const _outcomes = Convention.mount(Convention.metric.idempotencyOutcome)

// the bracket's own three-way fold IS the axis: fresh execute, same-digest replay, diverged refusal
const _outcome = (kind: "fresh" | "replay" | "conflict"): Effect.Effect<void> =>
  Metric.increment(Metric.tagged(_outcomes, Convention.rasm.admitDisposition, kind))

class Idempotency extends Context.Tag("runtime/serve/Idempotency")<Idempotency, {
  readonly run: <A, I, E, R>(
    key: typeof _IdempotencyKey.Type,
    digest: string,
    outcome: Schema.Schema<A, I, never>,
    execute: Effect.Effect<A, E, R>,
  ) => Effect.Effect<A, E | GateFault, R>
}>() {
  static readonly memory = (retention: Duration.Duration): Layer.Layer<Idempotency> =>
    Layer.effect(
      Idempotency,
      Effect.map(Ref.make(HashMap.empty<string, _Cell>()), (cells) => ({
        run: <A, I, E, R>(
          key: typeof _IdempotencyKey.Type,
          digest: string,
          outcome: Schema.Schema<A, I, never>,
          execute: Effect.Effect<A, E, R>,
        ) =>
          Effect.gen(function* () {
            const slot = yield* Deferred.make<unknown, GateFault>()
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
                Effect.zipRight(_outcome("fresh"), execute).pipe(
                  Effect.tap((value) => Deferred.succeed(slot, value)),
                  Effect.onExit((exit) =>
                    Exit.isSuccess(exit)
                      ? Effect.void
                      : Effect.zipRight(
                          Deferred.fail(slot, _conflict("origin execution failed")),
                          Ref.update(cells, (table) =>
                            Option.match(HashMap.get(table, key), {
                              onNone: () => table,
                              onSome: (current) => current.slot === slot ? HashMap.remove(table, key) : table,
                            })),
                        )),
                ),
              onSome: (cell) =>
                cell.digest === digest
                  ? Effect.zipRight(_outcome("replay"), Deferred.await(cell.slot)).pipe(
                      Effect.flatMap((value) =>
                        Schema.validate(outcome)(value).pipe(
                          Effect.mapError(() => _conflict("idempotency-key outcome divergence")),
                        )),
                    )
                  : Effect.zipRight(_outcome("conflict"), _refuse(_conflict("idempotency-key payload mismatch"))),
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
  RpcAuthn,
  shed: (pressure: Gate.Pressure): Effect.Effect<<A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, GateFault | E, R>> =>
    Effect.map(Effect.makeSemaphore(pressure.inFlight), (permits) =>
      <A, E, R>(self: Effect.Effect<A, E, R>) =>
        permits.withPermitsIfAvailable(1)(self).pipe(
          Effect.flatMap(Option.match({
            onNone: () => _refuse(new GateFault({ reason: "shed", detail: "in-flight cap", retryAfter: Option.some(pressure.grace) })),
            onSome: Effect.succeed,
          })),
        )),
  window: (pressure: Gate.Pressure): Effect.Effect<
    <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, GateFault | E, R>,
    never,
    Scope.Scope
  > =>
    Effect.map(
      RateLimiter.make({ limit: pressure.window.limit, interval: pressure.window.interval, algorithm: "token-bucket" }),
      (limiter) =>
        <A, E, R>(self: Effect.Effect<A, E, R>) =>
          limiter(Effect.void).pipe(
            // the deadline bounds the token wait alone: the admitted work sequences after it and never races its own timeout
            Effect.timeoutFail({
              duration: pressure.grace,
              onTimeout: () => new GateFault({ reason: "rate", detail: "window", retryAfter: Option.some(pressure.grace) }),
            }),
            Effect.catchTag("GateFault", _refuse),
            Effect.zipRight(self),
          )),
  fenced: <A, E, R>(
    self: Effect.Effect<A, E, R>,
    key: string,
    pressure: Gate.Pressure,
  ): Effect.Effect<A, GateFault | E, R | Fleet.RateLimiter> =>
    Effect.flatMap(Fleet.makeWithRateLimiter, (limit) =>
      limit({
        key,
        window: pressure.window.interval,
        limit: pressure.window.limit,
        onExceeded: "fail",
      })(self).pipe(
        // both experimental faults share the "RateLimiterError" tag; reason discriminates the arms
        Effect.catchTag("RateLimiterError", (fault) =>
          fault.reason === "Exceeded"
            ? _refuse(new GateFault({ reason: "rate", detail: key, retryAfter: Option.some(fault.retryAfter) }))
            : Effect.die(fault)),
      )),
} as const
```

## [06]-[CONTRIBUTION]

[CONTRIBUTION]:
- Owner: `Contribution` — the pairing law as two constructors: `Contribution.http(group, handlers)` pairs an `HttpApiGroup` with its handler builder — a function OF the assembled api, because `HttpApiBuilder.group(api, name, build)` demands the api value only the app holds, the mechanical fact that makes the god-contract impossible; `Contribution.rpc(group, handlers)` pairs an `RpcGroup` with the handler Layer its `toLayer` already built, because RPC handlers bind to the group alone.
- Law: the app assembly is three chained folds stated here as law — `HttpApi.make(id).add(a.group).add(b.group)` builds the one api value; each http row's `handlers(api)` Layer merges under `Layer.provide` into `HttpApiBuilder.api(api)`; each rpc row's group merges through `group.merge(other)` into one served group — and the assembled values exist only in the app's composition root, with `route#SERVE_FOLD` consuming the resulting Layer.
- Law: `Contribution.protocols` crossed with `Contribution.codecs` is the RPC serve roster — protocol rows `http` and `websocket` as path-parameterized factories over the ROUTER-native constructors, `socket` as the raw-socket-server row, `worker` as the runner row whose typed boot handshake is `RpcWorker.layerInitialMessage(schema, build)`, `stdio` as the child-process/MCP transport over its stdin Stream and stdout Sink — crossed with serialization rows `json`, `jsonRpc`, `ndjson`, `ndjsonRpc`, `msgpack`, and parameterized `msgpackWith`, selected once at the app root; a transport or codec choice inside a handler, or a procedure re-declared per transport, is the named defect.
- Law: the mountable rows are router-native by construction — `layerProtocolHttpRouter`/`layerProtocolWebsocketRouter` require the `HttpLayerRouter.HttpRouter` `route#SERVE_FOLD` provides, where the legacy `layerProtocolHttp`/`layerProtocolWebsocket` pair requires an `HttpRouter.Default` Tag no serve Layer in this branch supplies; the branch's ONE-front-door ruling forecloses a second listener, so the legacy pair has no reachable row and does not appear here.
- Law: procedure rows carry their own semantics as `Rpc.make` options and wrappers — `primaryKey` states the request-dedup identity where a procedure is idempotent by value, `Rpc.fork` marks a fire-and-forget handler that answers without occupying the mailbox, `Rpc.uninterruptible` marks a settle that must not be torn by client disconnect — each a declaration on the contributed row, never a handler-interior branch.
- Law: the RPC arm carries its own principal-providing admission — `Gate.RpcAuthn` is the `[05]`-owned Tag, `RpcGroup`'s `.middleware` scopes it to the contributed procedures, and `RpcAuthn.caller` supplies the client arm `requiredForClient` demands — so the HTTP `Authn` and the RPC admission compose the one `Authn.admit` lift, provide the same `Principal`, bind the same tenancy, and neither arm ships unauthenticated by omission.
- Law: streaming procedures declare `stream: true` on `Rpc.make` and nothing else — the protocol row frames chunks and exit; hand-framing a stream over a unary procedure is rejected on sight.
- Law: upload endpoints are declared modality — `HttpApiSchema.Multipart(schema)` on the endpoint payload types the parts, and file parts decode through `Multipart.toPersisted` / `Multipart.schemaPersisted(schema)` so a persisted file part hands into the data rail's byte lift as a scoped filesystem fact; an untyped `request.multipart` read in a handler is the deleted spelling. The byte and part-count bound is `route#SEAM_ROWS`'s `uploads` policy row, applied once as fiber-ref policy by the one seam every mounted route inherits, so an endpoint declares its shape and never its ceiling.
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
  type Rpc<G, Out, E, R> = {
    readonly _tag: "Rpc"
    readonly group: G
    readonly handlers: Layer.Layer<Out, E, R>
  }
  type Protocol = keyof typeof _protocols
  type Codec = keyof typeof _codecs
}

const _protocols = {
  // the router-native pair: both demand the `HttpLayerRouter.HttpRouter` the serve fold provides, where the
  // legacy pair demands an `HttpRouter.Default` Tag no serve Layer in this branch supplies
  http: (path: `/${string}`) => RpcServer.layerProtocolHttpRouter({ path }),
  websocket: (path: `/${string}`) => RpcServer.layerProtocolWebsocketRouter({ path }),
  socket: () => RpcServer.layerProtocolSocketServer,
  worker: () => RpcServer.layerProtocolWorkerRunner,
  stdio: (options: Parameters<typeof RpcServer.layerProtocolStdio>[0]) => RpcServer.layerProtocolStdio(options),
} as const

const _codecs = {
  json: RpcSerialization.layerJson,
  jsonRpc: RpcSerialization.layerJsonRpc(),
  ndjson: RpcSerialization.layerNdjson,
  ndjsonRpc: RpcSerialization.layerNdJsonRpc(),
  msgpack: RpcSerialization.layerMsgPack,
  msgpackWith: (options?: Parameters<typeof RpcSerialization.makeMsgPack>[0]) =>
    Layer.succeed(RpcSerialization.RpcSerialization, RpcSerialization.makeMsgPack(options)),
} as const

const Contribution: {
  readonly http: <const G, Api, Out, E, R>(group: G, handlers: (api: Api) => Layer.Layer<Out, E, R>) => Contribution.Http<G, Api, Out, E, R>
  readonly rpc: <const G, Out, E, R>(group: G, handlers: Layer.Layer<Out, E, R>) => Contribution.Rpc<G, Out, E, R>
  readonly protocols: typeof _protocols
  readonly codecs: typeof _codecs
} = {
  http: (group, handlers) => ({ _tag: "Http", group, handlers }),
  rpc: (group, handlers) => ({ _tag: "Rpc", group, handlers }),
  protocols: _protocols,
  codecs: _codecs,
}
```

## [07]-[EMIT]

[EMIT]:
- Owner: `Emit` — the derivation surface over the app-assembled value, parameterized on it, never importing it. `Emit.artifact` is the canonical spec artifact: `OpenApi.fromApi(api)` serialized with sorted keys and fixed indentation so two emissions of one contract are byte-identical and the contract gate diffs bytes, never re-parses; the `cli` inspect verb and the drift check consume this one member. `Emit.docs(options)` is the served documentation stack — the `_uis` roster crossed with the document route — and `Emit.uis` publishes the row keys so a UI choice and its path are one value the app root or `route#LAYER_ROUTES` selects.
- Law: `_uis` is a six-row roster across two axes the platform genuinely ships — renderer (`scalar` | `swagger`) crossed with mount (standalone | router-native) crossed with the Scalar asset source (bundled | CDN, the row an air-gapped deploy inverts) — each row carrying its own `native` column: `false` rows mount under `HttpApiBuilder.serve` and take `HttpApiBuilder.middlewareOpenApi()` beside them, `true` rows mount beside `HttpLayerRouter.addHttpApi`, whose `openapiPath` already serves the document, so the document route can never mount twice. A UI the roster cannot express is a missing row, never a literal at a route.
- Law: the router-native rows demand the assembled api value — `layerHttpLayerRouter({ api, path })` takes both as REQUIRED options — so the docs choice and its path resolve through this owner alone and `route#LAYER_ROUTES` selects a `Emit.NativeUi` row instead of spelling a constructor and a path inline.
- Law: the security requirements in the emitted document are the declared schemes — `Authn`'s `security` record flows into the spec through the api value, so the published contract states bearer and API-key admission from the same declaration that enforces it; a hand-authored securitySchemes block restates what the declaration already emits.
- Law: `Emit.client` derives the typed HTTP SDK through `HttpApiClient.make(api, { baseUrl, transformClient })` and `Emit.caller` derives the RPC peer through `RpcClient.make(group)` under one `Emit.dials` row, and both take the same `Emit.Temper` value — the generic transform the rpc dial declares also inhabits the api client's narrower slot, so `net/client#DIAL_SEAM`'s tempering reaches both peers from one spelling and neither derivation dials raw. Contributed groups and assembled apis therefore ship their own consumers, and a hand-written fetch client beside a contract is unspellable. Client faults are the declared faults: each endpoint's `addError` family beside transport and decode, one error vocabulary spanning the wire.
- Law: `Emit.dials` answers `[06]`'s serve roster arm for arm — `RpcClient` publishes three client protocol layers, and the websocket arm rides the socket row over a `Socket` Layer rather than a fourth constructor, so a group served on any mounted protocol derives its peer instead of only the HTTP row deriving one. Every arm but `worker` leaves `RpcSerialization` in the requirement channel, so peer and server select ONE `Contribution.codecs` value at the same root and a codec mismatch cannot be spelled.
- Law: a derived RPC peer cannot ship credential-less — a group scoping `Gate.RpcAuthn` carries `RpcMiddleware.ForClient<RpcAuthn>` into `RpcClient.make`'s requirement channel, which `Emit.caller` deliberately does NOT discharge, so the composition root supplies `RpcAuthn.caller(credential)` or the caller fails to type; `requiredForClient` is the whole mechanism and no `Emit` member weakens it with a default.
- Law: RPC egress is context-continuous — `Emit.traced(call)` reads `Propagation.current`, seeds the platform header frame from the live span, and injects the carried tracestate and baggage through core's `connect` dialect before `RpcClient.withHeadersEffect` derives and scopes the call headers (`RpcClient.currentHeaders` beneath it), so a distributed hop preserves the whole W3C triple rather than `traceparent` alone.
- Law: the web-handler edge form is the platform surface composed at the app root — `HttpApiBuilder.toWebHandler(layer, options)` takes the app's implementation Layer (the one carrying `HttpApi.Api`) and yields the `Request => Response` arrow plus its `dispose` for fetch-shaped runtimes, and no `Emit` member renames it because a forwarding member is the one-hop wrapper this corpus deletes; the full-server form (api beside raw routes) is `route#SERVE_FOLD`'s `HttpLayerRouter.toWebHandler`.
- Law: derivation is call-time and parameterized — nothing here caches, names, or holds an api instance, keeping the assembled value's no-lib-side-existence law intact; contract documentation is annotation material on the api value (`HttpApi.make(id).annotate`, endpoint schema annotations) flowing into the document through the derivation.
- Growth: a new documentation surface is one `_uis` row under the same two axes; a new dial is one `_dials` row; a new derivation family (a JSON-schema bundle per owner) is one member over the same api parameter.
- Boundary: `@connectrpc/connect-node` derives no peer at this owner — its `connectNodeAdapter` server half is `live#MOUNT_PORT`'s row under the one-front-door ruling, and its `createConnectTransport`/`createGrpcTransport`/`createGrpcWebTransport` client half is `net/client#CONNECT_ROW`'s dialect roster keyed by an emitted `DescService` rather than by a contributed `RpcGroup`; both dial families spend this branch's one egress budget and share nothing else.
- Law: the span seed is LIVE-span-only by declaration — `HttpTraceContext.toHeaders` takes `Tracer.Span` where `Tracer.AnySpan` is the `Span | ExternalSpan` union, so a recovered `ExternalSpan` is unspellable at that member and no adapter lifts one into it; an ingress-recovered parent therefore crosses on the carried context alone, which is exactly why the inject site seeds the frame rather than deriving the hop from it.
- Packages: `@effect/platform` (`OpenApi`, `HttpApiBuilder`, `HttpApiScalar`, `HttpApiSwagger`, `HttpApiClient`, `HttpLayerRouter`, `HttpTraceContext`, `Headers`, `Socket`); `@effect/rpc` (`RpcClient`); `effect` (`Layer`, `Array`, `Record`, `Order`, `Predicate`).

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

declare namespace Emit {
  // ONE tempering shape both derivations take: the generic form `layerProtocolHttp` declares also inhabits
  // `HttpApiClient.make`'s narrower slot, so the branch's egress posture crosses to both peers as one value
  type Temper = <E, R>(client: HttpClient.HttpClient.With<E, R>) => HttpClient.HttpClient.With<E, R>
  type Origin = { readonly url: string }
  type Dial = keyof typeof _dials
  type Docs = { readonly api: HttpApi.HttpApi.Any; readonly path: `/${string}` }
  type Mount = Layer.Layer<never, never, HttpApi.Api | HttpLayerRouter.HttpRouter>
  type UiRow = { readonly native: boolean; readonly layer: (options: Emit.Docs) => Emit.Mount }
  type Ui = keyof typeof _uis
  type NativeUi = { [K in Ui]: (typeof _uis)[K]["native"] extends true ? K : never }[Ui]
}

// renderer x mount x asset source, each row carrying whether it mounts route-natively — the `native` column
// is what decides who serves the document, so no caller re-derives that pairing
const _uis = {
  scalar: { native: false, layer: ({ path }: Emit.Docs) => HttpApiScalar.layer({ path }) },
  scalarCdn: { native: false, layer: ({ path }: Emit.Docs) => HttpApiScalar.layerCdn({ path }) },
  swagger: { native: false, layer: ({ path }: Emit.Docs) => HttpApiSwagger.layer({ path }) },
  scalarRouter: { native: true, layer: ({ api, path }: Emit.Docs) => HttpApiScalar.layerHttpLayerRouter({ api, path }) },
  scalarRouterCdn: { native: true, layer: ({ api, path }: Emit.Docs) => HttpApiScalar.layerHttpLayerRouterCdn({ api, path }) },
  swaggerRouter: { native: true, layer: ({ api, path }: Emit.Docs) => HttpApiSwagger.layerHttpLayerRouter({ api, path }) },
} as const satisfies Record<string, Emit.UiRow>

const _docs = (options: Emit.Docs & { readonly ui?: Emit.Ui }): Emit.Mount =>
  pipe(_uis[options.ui ?? "scalar"], (row) =>
    // a router row rides addHttpApi's own openapiPath; merging the middleware there would mount the document twice
    row.native ? row.layer(options) : Layer.mergeAll(HttpApiBuilder.middlewareOpenApi(), row.layer(options)))

const _client = <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
  api: HttpApi.HttpApi<Id, Groups, E, R>,
  options: { readonly baseUrl: string; readonly transform: Emit.Temper },
) => HttpApiClient.make(api, { baseUrl: options.baseUrl, transformClient: options.transform })

// Client protocol rows answer `[06]`'s serve roster: `RpcClient` publishes three layers, `websocket` reached by
// satisfying `Socket` rather than by a fourth constructor. `http` and `websocket` leave `RpcSerialization` open
// so peer and server take one `Contribution.codecs` value at the same root; `worker` frames its own and takes none.
const _dials = {
  // tempering seats on the http arm alone, because it is the arm carrying an `HttpClient` to temper
  http: (origin: Emit.Origin & { readonly transform?: Emit.Temper }) =>
    RpcClient.layerProtocolHttp({ url: origin.url, transformClient: origin.transform }),
  websocket: (origin: Emit.Origin) =>
    RpcClient.layerProtocolSocket().pipe(
      Layer.provide(Socket.layerWebSocket(origin.url)),
      Layer.provide(Socket.layerWebSocketConstructorGlobal),
    ),
  worker: (pool: Parameters<typeof RpcClient.layerProtocolWorker>[0]) => RpcClient.layerProtocolWorker(pool),
} as const

const _caller = <G extends RpcGroup.RpcGroup.Any, E, R>(group: G, dial: Layer.Layer<RpcClient.Protocol, E, R>) =>
  Effect.provide(RpcClient.make(group), dial)

const _traced = <A, E, R>(call: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
  RpcClient.withHeadersEffect(
    Effect.flatMap(Propagation.current, (context) =>
      Effect.map(
        Effect.optionFromOptional(Effect.currentSpan),
        // one inject site: the span only seeds the platform frame — the carried context crosses on
        // both branches, so an inherited parent survives a span-less caller
        (span) =>
          Carrier.inject(
            "connect",
            context,
            // `toHeaders` names `Tracer.Span` alone, so a span-less caller seeds the dialect's own empty frame
            // rather than an untyped literal the `connect` row would then have to admit
            Option.match(span, { onNone: () => Headers.empty, onSome: (live) => HttpTraceContext.toHeaders(live) }),
          ),
      )),
  )(call)

const Emit = {
  artifact: _artifact,
  caller: _caller,
  client: _client,
  dials: _dials,
  docs: _docs,
  traced: _traced,
  uis: Record.keys(_uis),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Contribution, Current, Emit, Gate, GateFault, Principal, Surface }
```

## [08]-[RESEARCH]

(none)
