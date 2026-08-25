# [RUNTIME_API]

The one public front door's declarative engine: a domain folder exports its `HttpApiGroup` or `RpcGroup` as data, the APP — never this module, never any lib module — assembles exactly one `HttpApi` value and crosses exactly one RPC protocol row with one serialization row at its root, and every secondary surface — the OpenAPI 3.1 document, the served Scalar reference UI, the byte-stable spec artifact, the typed HTTP SDK, the typed RPC caller, the fetch-shaped web handler — projects from that same assembled value so spec, docs, client, and server cannot drift. Auth is declarative into the emitted contract: the `Authn` middleware Tag carries its `HttpApiSecurity` schemes, so the bearer and API-key security requirements land in the OpenAPI document from the same declaration the handler set enforces, and a protected group's handlers receive `Principal` from the requirement channel. Every refusal is one `GateFault`; the ambient rows hold request identity, locale, and the one admitted credential selected by either the general seam or a protocol-specific entry. The god-contract is structurally impossible because `HttpApiBuilder.group` demands the assembled api value the lib never holds. The module ships on the `./server` exports subpath as `runtime/src/serve/api.ts`.

## [01]-[INDEX]

- [02]-[SURFACE]: version-prefix rows, the cursor brand, page-query and page-envelope constructors; `Surface`.
- [03]-[GATE_FAULT]: the reason-discriminated refusal family over the core family seam, the refusal fan; `GateFault`.
- [04]-[CURRENT_ROWS]: ambient stamp/locale/admitted references, locale negotiation, trace continuation; `Current`.
- [05]-[ADMISSION_ROWS]: `Principal`, the credential admission owner, HTTP/RPC Tags, pressure, idempotency; `Principal`, `Gate`.
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
  RateLimiter, Record, Redacted, Ref, Schedule, Schema, type Scope, pipe,
} from "effect"
import { Identity, Carrier, Convention, Fault, Shape } from "@rasm/core"
import { ApiKey, Claim, CookieSpec, Jwt, type Principal as Scoped, Session, TenantScope } from "@rasm/security"
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
- Law: the family is sized by outward route: `malformed` is a structurally invalid credential presentation, `unauthorized` an absent or unverifiable credential, `forbidden` a verified but insufficient one, `shed` an unavailable door, `rate` an exhausted window, and `conflict` an idempotency divergence.
- Law: each row renders its own subject — invalid presentation site, absent scheme, missing entitlement, pressure source, spent window, or divergent offer — and the refusal fan tags on that routed reason.
- Law: the refusal code falls out of the core class alone — 400, 401, 403, 503, 429, or 409 — so no serve-local status table can disagree with `problem#STATUS_RECORD`.
- Law: the stated window rides `after` — the core owner's VALUE-altitude word, so `Fault.Class.statedOf` reads it back at the problem door and `Fault.Budget.schedule` takes it as its third argument; the pressure rows stamp it from their own measured window, the ladder prefers it over the class default, and a 429/503 always carries the truthful figure rather than a guessed constant. The wire's `retry_after` and the response header's `retry-after` are the other two altitudes of one window, and none renames another.
- Law: every refusal emits before it fails — `_refuse` is the one fail seam, incrementing `Convention.metric.admitRefused` tagged by the reason the fault already carries, so the refusal fan needs zero call-site wiring and a new reason joins the series the moment its row lands; `Gate.refuse` publishes that same seam to the serving route module, so a refusal minted at the seam counts itself through the one fold rather than a second `Effect.fail`.
- Packages: `effect` (`Schema`, `Option`, `Duration`, `Metric`); `@rasm/core` (`Fault.Class`, `Convention`).

```typescript
const _LEG = "admission"

const _gate = Fault.Class.family(["malformed", "unauthorized", "forbidden", "shed", "rate", "conflict"] as const, {
  malformed: Fault.Class.row({
    class: "malformed",
    leg: _LEG,
    detail: Schema.Struct({ via: Schema.NonEmptyString }),
    render: ({ via }) => `credential presentation is structurally invalid at ${via}`,
  }),
  unauthorized: Fault.Class.row({
    class: "expired",
    leg: _LEG,
    detail: Schema.Struct({ via: Schema.NonEmptyString }),
    render: ({ via }) => `no credential this door accepts arrived on ${via}`,
  }),
  forbidden: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: Schema.Struct({ scope: Schema.NonEmptyString }),
    render: ({ scope }) => `the admitted credential carries no ${scope} entitlement`,
  }),
  shed: Fault.Class.row({
    class: "unavailable",
    leg: _LEG,
    detail: Schema.Struct({ source: Schema.Literal("cap", "store") }),
    render: ({ source }) =>
      source === "cap" ? "the in-flight cap admits nothing right now" : "a credential store this lift needed answered nothing",
  }),
  rate: Fault.Class.row({
    class: "exhausted",
    leg: _LEG,
    detail: Schema.Struct({ window: Schema.NonEmptyString }),
    render: ({ window }) => `the ${window} window is spent`,
  }),
  conflict: Fault.Class.row({
    class: "conflicted",
    leg: _LEG,
    detail: Schema.Struct({ detail: Schema.String }),
    render: ({ detail }) => `an idempotency key was replayed against a different offer — ${detail}`,
  }),
})

declare namespace GateFault {
  type Issue = typeof _gate.payload.Type
  type Reason = (typeof _gate.kinds)[number]
}

class GateFault extends Schema.TaggedError<GateFault>()("GateFault", {
  case: _gate.payload,
  after: Fault.Class.After,
}) {
  get class(): Fault.Class.Kind {
    return _gate.classOf(this.case.reason)
  }
  override get message(): string {
    return _gate.render(this.case)
  }
}

const _refused = Convention.mount(Convention.metric.admitRefused)

const _refuse = (fault: GateFault): Effect.Effect<never, GateFault> =>
  Effect.zipRight(
    Metric.increment(Metric.tagged(_refused, Convention.rasm.admitReason, fault.case.reason)),
    Effect.fail(fault),
  )
```

## [04]-[CURRENT_ROWS]

[CURRENT_ROWS]:
- Owner: `Current` — ambient request `Context.Reference` rows for the mark, negotiated locale, and one admitted credential. The general seam provides header/cookie admission; a protocol-specific entry may replace the same seat.
- Law: locale negotiation is one fold — `Current.negotiate(header, fallback)` splits the `Accept-Language` list, ranks by the case-insensitive `q` weight descending, and takes the first tag the core `Shape.Refined.Locale` schema admits — a malformed tag or an empty header lands on the fallback and negotiation can never fail; the negotiated value is BCP-47-canonical by the core brand's own filter.
- Law: trace continuation is composed, never re-derived — `Current.traced(effect, headers)` normalizes the request record once through core `Carrier.extract("http", ...)` and hands the resulting `Carrier.Extraction` WHOLE to `otel/emit#CONTINUATION`'s one ingress transformer, so the parse census rides beside the context and no destructuring here drops a count no second reader keeps; extract-and-continue at the HTTP door is the same transformer every other ingress composes (baggage annotations arrive pre-scrubbed by that owner) and a second `traceparent` decode cannot exist here.
- Law: the stamp mints at the door — `Current.provide(effect, mark, fallback)` provides the mark and the locale projected from it in one scoped provision, so a handler, a log annotation, and the problem fold read one coherent request identity; the `problem` page reads `Current.Stamp` for the `instance` member and the `requestId` extension.
- Law: `Current.Admitted` is the credential lift's one seat — the global seam resolves header and cookie credentials once, while the webhook's route-scoped query carriage lands the same `Admitted` value here after `Authn.webhook` resolves it. Scheme arms project this seat instead of re-verifying.
- Growth: a new ambient axis is one `Context.Reference` row plus its projection inside `provide`.
- Packages: `effect` (`Context`, `Option`, `Schema`, `Array`, `Order`, `Number`); `@rasm/core` (`Shape.Refined`); `../otel/emit.ts` (`Propagation`).

```typescript
const _byWeight: Order.Order<readonly [string, number]> = Order.mapInput(
  Order.reverse(Order.number),
  (pair: readonly [string, number]) => pair[1],
)

const _WEIGHT = /;\s*q=/i

const _negotiate = (header: Option.Option<string>, fallback: Shape.Refined.Locale): Shape.Refined.Locale =>
  pipe(
    Option.getOrElse(header, () => ""),
    (raw) => raw.split(","),
    Array.filterMap((part) => {
      const [tag, weight] = part.split(_WEIGHT)
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
- Law: `Authn.admit(identity, headers)` is the general request credential lift: bearer and cookie tokens share `_verified`, API keys cross the same claim projection, and every arm returns one `Admitted` pair. Explicit-to-ambient precedence remains bearer, API key, cookie; an absent credential remains `Option.none()` for endpoint policy.
- Law: `Authn.webhook(identity, headers, url)` is the sole CloudEvents Webhook token owner. It admits either the `Authorization: Bearer` carriage already lifted by the global seam or exactly one non-empty `access_token` query carriage through the same `_verified` rail. Both carriages together always refuse, even when their bytes match; repeated query tokens refuse; and an API-key or cookie cannot substitute for or accompany either required method. The query token never becomes a synthetic header, and its result returns the carriage so the route can stamp `private` on a successful response.
- Law: `Authn.live`'s scheme arms PROJECT, never verify — each arm reads `Current.Admitted`, admits the row whose `via` matches its own scheme, and refuses `unauthorized` otherwise, so a declared-but-unpresented scheme costs nothing and no contract can make one request pay two verifications; attachment stays `.middleware(Gate.Authn)` on the contributed group, and verification failure carries generic detail because the evidence rides telemetry, never the 401 body.
- Law: the cookie arm admits the LIFT alone — a cookie is the one credential a browser replays cross-site, so the double-submit proof for cookie-authenticated state-changing methods composes security `Cookie.verify` over the one `CookieSpec.csrf` pair at the seam providing `Current.Admitted` (`route#SEAM_ROWS`'s `Seam.admission`), refusing `unauthorized` on an absent or mismatched pair with the safe methods (`GET`/`HEAD`/`OPTIONS`) exempt; the ceremony routes' `_csrfed` fold stays their own gate over that same one `Cookie.verify` owner, because those round-trips run before any admission exists to read.
- Law: tenancy binds at that same lift and nowhere else — `Admitted.scope` is the security `Principal` the seam hands to `TenantScope.bind` and `TenantScope.metered`, so every `SessionCoordinate` GUC, every RLS predicate, and every security instrument's tenant dimension resolve from the one credential the edge proved; this module holds no tenancy value of its own and CONSTRUCTS none — every arm mints its scope through `Claim.principal`, which composes `TenantScope.of` at the tenancy owner, where an inline `{ context, subject }` literal at a call site is the drift security `access/tenant` forbids.
- Law: `_lifted` binds its error parameter to the classed shape, so its two-way partition is total by construction — every port fault it folds mints through `Fault.Class.family` and carries the `class` getter `Fault.Class.of` probes, and the probe reads `property in self`, so a prototype accessor answers. Any port contract failing with a bare `Error` refuses to compile at this fold rather than grading `defect`, whose `system` blame answers `shed` 503 where the presented credential earns 401.
- Law: the admission plane measures itself off its own partitions — `admitPassed` counts each lift tagged by the `via` scheme so the refusal series has a denominator, `admitRefused` fans on the `GateFault` reason at the one `_refuse` seam, and `idempotencyOutcome` fans on the bracket's own three-way fold; every instrument mounts from its `Convention` row, so this page carries no bucket ladder and no constructor pick.
- Law: `RpcAuthn` is the same admission on the RPC arm — `RpcMiddleware.Tag` with `failure: GateFault`, `provides: Principal`, `requiredForClient: true`, and `wrap: true`, so one definition governs both ends: the wrap reads the frame headers, composes the same `Authn.admit` lift, provides the same `Principal` into `next`, and binds `TenantScope` around it exactly as the HTTP seam does. `requiredForClient` is what makes `Emit.caller` refuse to derive a credential-less client, and `RpcMiddleware.layerClient` is the client arm the app root supplies; a `Contribution.rpc` group scoping this Tag through `.middleware` cannot ship unauthenticated by omission.
- Law: pressure rows bound two distinct axes — `Gate.shed` brackets a section under an in-flight cap whose refusal is immediate (`withPermitsIfAvailable` settling `Option.none` under saturation folds to `shed` with the declared grace: the queue-depth 503 lever), `Gate.window` prices calls against a scoped in-process `RateLimiter.make` row (the 429 lever) whose grace deadline bounds the TOKEN WAIT alone — the admitted work never races its own timeout, because the deadline gates the acquisition probe and the work sequences after it — conflating concurrency and throughput is the named selection error; both stamp `after` from their own measured window, and policy is one `Gate.Pressure` value row, never threaded knobs.
- Law: the distributed quota row is port-shaped by Layer — `Gate.fenced` yields the experimental accessor `Fleet.makeWithRateLimiter` (an `Effect` reading the `RateLimiter.RateLimiter` Tag the app root satisfies with `layerStoreMemory` on one node or a store-backed Layer on a fleet) and applies its transformer; both experimental faults share the one `"RateLimiterError"` tag discriminated by `reason` — the `"Exceeded"` arm re-spells as `rate` carrying the fault's own measured `retryAfter`, and the `"StoreError"` arm dies as a defect because a broken quota backend is never a caller 429.
- Law: `Gate.fenced` takes one `Gate.Spend` record speaking the branch's four-column quota grammar — `window`, `limit`, `key`, `cost` — with `cost` forwarded to the limiter's `tokens` slot and `algorithm` the row's own; `Gate.Pressure` keeps serving `shed` and `window`, whose in-flight and grace cells `fenced` never reads, and the store namespaces nothing, so the caller's `key` carries every scope join.
- Law: `Idempotency` is one polymorphic bracket, never a claim ceremony — `run(key, digest, outcome, execute)` owns the whole fresh/replay fold, so a handler composes one call and never orchestrates claim, settle, or park: the first execution per key runs `execute` and answers `{ disposition: "fresh", value }`; a same-digest duplicate parks on the cell and answers `{ disposition: "replay", value }` after the settled value re-proves through `Schema.validate(outcome)` (the fast lane carries the same schema evidence the fleet tier's `Schema.TaggedRequest` carries), a diverged replay refusing as `conflict`; any non-success exit settles every parked duplicate with a typed `conflict` refusal and conditionally releases only its own cell — no duplicate can hang on an interrupted or defective origin, an expired origin cannot delete a newer claimant's cell, the origin's own exit propagates unchanged, and the next claimant executes fresh; a replayed key whose payload digest differs refuses as `conflict` before any wait. `Idempotency.memory(retention)` is the single-node Layer sweeping expired cells inside the same atomic claim; the key admits through the `Gate.IdempotencyKey` brand at the header seam, and a GET carrying the header is ignored, never refused.
- Law: the fleet tier is `Idempotency.persisted` — `PersistedCache.make({ storeId, lookup, timeToLive })` over the store-owned `Persistence.layerResultKeyValueStore`, keyed by a `Schema.TaggedRequest` whose `PrimaryKey` fuses idempotency key and payload digest, so the first execution's exit persists for the retention window, every fleet duplicate replays the stored exit typed through the request's own success/failure schemas, and a divergent payload is a different key that executes fresh; the strict 409 divergence posture stays the memory gate composed in front, so both tiers ride one root and zero handler change.
- Growth: another general credential scheme is one security row, scheme reader, admission arm, and `via` literal; protocol-specific carriage stays an entry on its protocol's admission owner rather than widening every route.

```typescript
class _Principal extends Schema.Class<_Principal>("Principal")({
  subject: Schema.NonEmptyString,
  session: Schema.optionalWith(Session.fields.id, { as: "Option" }),
  tenant: Schema.optionalWith(Identity.Tenant.fields.tenant, { as: "Option" }),
  scopes: Schema.Array(Schema.NonEmptyString),
  via: Schema.Literal("session", "apikey", "cookie"),
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

const _paired = (raw: string, name: string): Option.Option<string> =>
  Option.map(
    Array.findFirst(raw.split("; "), (pair) => pair.startsWith(`${name}=`)),
    (pair) => pair.slice(name.length + 1),
  )

const _schemes = {
  session: (headers: Headers.Headers) =>
    Option.map(
      Option.flatMapNullable(Option.fromNullable(headers.authorization), (raw) => _BEARER.exec(raw)?.[1]),
      Redacted.make,
    ),
  apikey: (headers: Headers.Headers) => Option.map(Option.fromNullable(headers["x-api-key"]), Redacted.make),
  cookie: (headers: Headers.Headers) =>
    Option.map(
      Option.flatMap(Option.fromNullable(headers.cookie), (raw) => _paired(raw, CookieSpec.access.name)),
      Redacted.make,
    ),
} as const satisfies Record<_Principal["via"], (headers: Headers.Headers) => Option.Option<Redacted.Redacted<string>>>

const _lifted = <A, E extends { readonly class: Fault.Class.Kind }, R>(
  self: Effect.Effect<A, E, R>,
): Effect.Effect<Option.Option<A>, GateFault, R> =>
  Effect.matchEffect(self, {
    onFailure: (fault) =>
      Fault.Class.blameOf(fault) === "caller"
        ? Effect.succeedNone
        : _refuse(new GateFault({ case: { reason: "shed", source: "store" }, after: Option.none() })),
    onSuccess: Effect.succeedSome,
  })

const _verified = (
  identity: Identity.App,
  token: Redacted.Redacted<string>,
  via: "session" | "cookie",
): Effect.Effect<Option.Option<Authn.Admitted>, GateFault, Jwt | Claim> =>
  Effect.gen(function* () {
    const jwt = yield* Jwt
    const claim = yield* Claim
    return yield* jwt.verify(token).pipe(
      Effect.flatMap((held) => Effect.map(claim.resolve(held), (claims) => ({ held, claims }))),
      Effect.map(({ claims, held }): Authn.Admitted => ({
        principal: new _Principal({
          subject: claims.subject,
          session: Option.some(held.sid),
          tenant: claims.tenant,
          scopes: held.scope,
          via,
        }),
        scope: claim.principal(identity, claims),
      })),
      _lifted,
    )
  })

const _admit = (
  identity: Identity.App,
  headers: Headers.Headers,
): Effect.Effect<Option.Option<Authn.Admitted>, GateFault, Jwt | ApiKey | Claim> =>
  Effect.gen(function* () {
    const keys = yield* ApiKey
    const claim = yield* Claim
    const bearer = _schemes.session(headers)
    const key = _schemes.apikey(headers)
    const cookied = _schemes.cookie(headers)
    return yield* Option.match(bearer, {
      onSome: (token) => _verified(identity, token, "session"),
      onNone: () =>
        Option.match(key, {
          onSome: (presented) =>
            keys.resolve(presented).pipe(
              Effect.flatMap((record) => Effect.map(claim.resolve(record), (claims) => ({ claims, record }))),
              Effect.map(({ claims, record }): Authn.Admitted => ({
                principal: new _Principal({
                  subject: claims.subject,
                  session: Option.none(),
                  tenant: claims.tenant,
                  scopes: record.scopes,
                  via: "apikey",
                }),
                scope: claim.principal(identity, claims),
              })),
              _lifted,
            ),
          onNone: () =>
            Option.match(cookied, {
              onNone: () => Effect.succeedNone,
              onSome: (token) => _verified(identity, token, "cookie"),
            }),
        }),
    })
  }).pipe(
    Effect.tap(Option.match({
      onNone: () => Effect.void,
      onSome: ({ principal }) =>
        Metric.increment(Metric.tagged(_passed, Convention.rasm.admitScheme, principal.via)),
    })),
  )

const _held = (via: _Principal["via"]): Effect.Effect<Authn.Admitted, GateFault> =>
  Effect.flatMap(_Admitted, (held) =>
    Option.match(Option.filter(held, (admitted) => admitted.principal.via === via), {
      onNone: () => _refuse(new GateFault({ case: { reason: "unauthorized", via }, after: Option.none() })),
      onSome: Effect.succeed,
    }))

const _projected = (via: _Principal["via"]): Effect.Effect<_Principal, GateFault> =>
  Effect.map(_held(via), (admitted) => admitted.principal)

const _webhookRefused = (via: string): Effect.Effect<never, GateFault> =>
  _refuse(new GateFault({ case: { reason: "unauthorized", via }, after: Option.none() }))

const _webhookMalformed = (via: string): Effect.Effect<never, GateFault> =>
  _refuse(new GateFault({ case: { reason: "malformed", via }, after: Option.none() }))

const _webhook = (
  identity: Identity.App,
  headers: Headers.Headers,
  url: URL,
): Effect.Effect<Authn.Webhook, GateFault, Jwt | Claim> =>
  Effect.gen(function* () {
    const authorization = Option.fromNullable(headers.authorization)
    const header = _schemes.session(headers)
    const query = url.searchParams.getAll("access_token")
    const alternate = Option.isSome(_schemes.apikey(headers)) || Option.isSome(_schemes.cookie(headers))
    if (Option.isSome(authorization) && Option.isNone(header)) return yield* _webhookMalformed("webhook-header-token")
    if (Option.isSome(header) && query.length > 0) return yield* _webhookMalformed("webhook-token-conflict")
    if (alternate && (Option.isSome(header) || query.length > 0)) {
      return yield* _webhookMalformed("webhook-token-conflict")
    }
    if (query.length > 1) return yield* _webhookMalformed("webhook-query-token")
    if (Option.isSome(header)) {
      const admitted = yield* _held("session")
      return { admitted, carriage: "header" } satisfies Authn.Webhook
    }
    const raw = query[0]
    if (raw === undefined) return yield* _webhookRefused("webhook-token")
    if (raw.length === 0) return yield* _webhookMalformed("webhook-query-token")
    const ambient = yield* _Admitted
    if (Option.isSome(ambient)) return yield* _webhookMalformed("webhook-token-conflict")
    const admitted = yield* Option.match(yield* _verified(identity, Redacted.make(raw), "session"), {
      onNone: () => _webhookRefused("webhook-query-token"),
      onSome: Effect.succeed,
    })
    yield* Metric.increment(Metric.tagged(_passed, Convention.rasm.admitScheme, admitted.principal.via))
    return { admitted, carriage: "query" } satisfies Authn.Webhook
  })

declare namespace Authn {
  type Admitted = { readonly principal: Principal.Shape; readonly scope: Scoped }
  type Webhook = { readonly admitted: Admitted; readonly carriage: "header" | "query" }
}

class Authn extends HttpApiMiddleware.Tag<Authn>()("runtime/serve/Authn", {
  failure: GateFault,
  provides: Principal,
  security: {
    bearer: HttpApiSecurity.bearer,
    apiKey: HttpApiSecurity.apiKey({ in: "header", key: "x-api-key" }),
    cookie: HttpApiSecurity.apiKey({ in: "cookie", key: CookieSpec.access.name }),
  },
}) {
  static readonly admit = _admit
  static readonly webhook = _webhook
  static readonly live: Layer.Layer<Authn> = Layer.succeed(Authn, {
    bearer: () => _projected("session"),
    apiKey: () => _projected("apikey"),
    cookie: () => _projected("cookie"),
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
            onNone: () => _refuse(new GateFault({ case: { reason: "unauthorized", via: "rpc" }, after: Option.none() })),
            onSome: (admitted) =>
              TenantScope.metered(TenantScope.bind(
                admitted.scope,
                Effect.provideService(next, Principal, admitted.principal),
              )),
          })),
          Effect.provide(held),
        )),
    )
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

const _conflict = (detail: string): GateFault => new GateFault({ case: { reason: "conflict", detail }, after: Option.none() })

const _outcomes = Convention.mount(Convention.metric.idempotencyOutcome)

const _outcome = (kind: "fresh" | "replay" | "conflict"): Effect.Effect<void> =>
  Metric.increment(Metric.tagged(_outcomes, Convention.rasm.admitDisposition, kind))

class Idempotency extends Context.Tag("runtime/serve/Idempotency")<Idempotency, {
  readonly run: <A, I, E, R>(
    key: typeof _IdempotencyKey.Type,
    digest: string,
    outcome: Schema.Schema<A, I, never>,
    execute: Effect.Effect<A, E, R>,
  ) => Effect.Effect<Idempotency.Outcome<A>, E | GateFault, R>
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
                  Effect.map((value): Idempotency.Outcome<A> => ({ disposition: "fresh", value })),
                ),
              onSome: (cell) =>
                cell.digest === digest
                  ? Effect.zipRight(_outcome("replay"), Deferred.await(cell.slot)).pipe(
                      Effect.flatMap((value) =>
                        Schema.validate(outcome)(value).pipe(
                          Effect.mapError(() => _conflict("idempotency-key outcome divergence")),
                        )),
                      Effect.map((value): Idempotency.Outcome<A> => ({ disposition: "replay", value })),
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

declare namespace Idempotency {
  type Outcome<A> = { readonly disposition: "fresh" | "replay"; readonly value: A }
}

declare namespace Gate {
  type IdempotencyKey = typeof _IdempotencyKey.Type
  type Pressure = {
    readonly inFlight: number
    readonly grace: Duration.Duration
    readonly window: { readonly limit: number; readonly interval: Duration.Duration }
  }
  type Spend = {
    readonly key: string
    readonly cost: number
    readonly algorithm: "fixed-window" | "token-bucket"
    readonly window: Duration.DurationInput
    readonly limit: number
  }
}

const Gate = {
  Authn,
  Idempotency,
  IdempotencyKey: _IdempotencyKey,
  RpcAuthn,
  refuse: _refuse,
  shed: (pressure: Gate.Pressure): Effect.Effect<<A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, GateFault | E, R>> =>
    Effect.map(Effect.makeSemaphore(pressure.inFlight), (permits) =>
      <A, E, R>(self: Effect.Effect<A, E, R>) =>
        permits.withPermitsIfAvailable(1)(self).pipe(
          Effect.flatMap(Option.match({
            onNone: () => _refuse(new GateFault({ case: { reason: "shed", source: "cap" }, after: Option.some(pressure.grace) })),
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
            Effect.timeoutFail({
              duration: pressure.grace,
              onTimeout: () => new GateFault({ case: { reason: "rate", window: "in-process" }, after: Option.some(pressure.grace) }),
            }),
            Effect.catchTag("GateFault", _refuse),
            Effect.zipRight(self),
          )),
  fenced: <A, E, R>(
    self: Effect.Effect<A, E, R>,
    spend: Gate.Spend,
  ): Effect.Effect<A, GateFault | E, R | Fleet.RateLimiter> =>
    Effect.flatMap(Fleet.makeWithRateLimiter, (limit) =>
      limit({
        key: spend.key,
        window: spend.window,
        limit: spend.limit,
        tokens: spend.cost,
        algorithm: spend.algorithm,
        onExceeded: "fail",
      })(self).pipe(
        Effect.catchTag("RateLimiterError", (fault) =>
          fault.reason === "Exceeded"
            ? _refuse(new GateFault({ case: { reason: "rate", window: spend.key }, after: Option.some(fault.retryAfter) }))
            : Effect.die(fault)),
      )),
} as const
```

## [06]-[CONTRIBUTION]

[CONTRIBUTION]:
- Owner: `Contribution` — the pairing law as two constructors: `Contribution.http(group, handlers)` pairs an `HttpApiGroup` with its handler builder — a function OF the assembled api, because `HttpApiBuilder.group(api, name, build)` demands the api value only the app holds, the mechanical fact that makes the god-contract impossible; `Contribution.rpc(group, handlers)` pairs an `RpcGroup` with the handler Layer its `toLayer` already built, because RPC handlers bind to the group alone.
- Law: the app assembly is three chained folds stated here as law — `HttpApi.make(id).add(a.group).add(b.group)` builds the one api value; each http row's `handlers(api)` Layer merges under `Layer.provide` into `HttpApiBuilder.api(api)`; each rpc row's group merges through `group.merge(other)` into one served group, two contributions colliding on a tag disambiguating through the package's own `group.prefix(name)` at the contribution seam — and the assembled values exist only in the app's composition root, with `route#SERVE_FOLD` consuming the resulting Layer.
- Law: RPC serving splits on ONE discriminant — a transport either mounts into the branch's single front door or IS its own listener. The front-door arm is `serve/route#LAYER_ROUTES`'s `Router.rpc`, whose `RpcMount` elects HTTP or websocket and states the fan-out ceiling over `RpcServer.layerHttpRouter`, the package's own fusion of the server Layer with the router-native protocol row; `Contribution.protocols` therefore carries the listener rows alone — `socket` as the raw-socket-server row, `worker` as the runner row whose typed boot handshake is `RpcWorker.layerInitialMessage(schema, build)`, `stdio` as the child-process/MCP transport over its stdin Stream and stdout Sink — each composed at the root beside `RpcServer.layer(group)`. Both arms cross `Contribution.codecs` — serialization rows `json`, `jsonRpc`, `ndjson`, `ndjsonRpc`, `msgpack`, and the parameterized `msgpackWith`/`ndjsonWith` pair riding the package's own `layer*With` members — selected once at the app root; a transport or codec choice inside a handler, or a procedure re-declared per transport, is the named defect.
- Law: the fan-out ceiling is stated wherever a group is served, because the package defaults `concurrency` to unbounded and no Layer above an RPC mount can re-impose one; the front-door arm states it on its `RpcMount` and a listener row states it on the `RpcServer.layer(group, …)` the root composes.
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
  msgpackWith: RpcSerialization.layerMsgPackWith,
  ndjsonWith: RpcSerialization.layerNdjsonWith,
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
- Law: `Emit.dials` covers the serve arms a CLIENT can dial and states the ones it cannot — `RpcClient` publishes three protocol layers, the websocket arm riding the socket row over a `Socket` Layer rather than a fourth constructor, so both front-door transports and the raw-socket listener derive their peer. `stdio` derives none because its peer is the parent process that spawned the child and holds the pipe pair already, and `worker` derives one that frames its own; every arm but `worker` leaves `RpcSerialization` in the requirement channel, so peer and server select ONE `Contribution.codecs` value at the same root and a codec mismatch cannot be spelled.
- Law: a derived RPC peer cannot ship credential-less — a group scoping `Gate.RpcAuthn` carries `RpcMiddleware.ForClient<RpcAuthn>` into `RpcClient.make`'s requirement channel, which `Emit.caller` deliberately does NOT discharge, so the composition root supplies `RpcAuthn.caller(credential)` or the caller fails to type; `requiredForClient` is the whole mechanism and no `Emit` member weakens it with a default.
- Law: RPC egress is context-continuous — `Emit.traced(call)` reads `Propagation.current`, seeds the platform header frame from the live span, and injects the carried tracestate and baggage through core's `connect` dialect before `RpcClient.withHeadersEffect` derives and scopes the call headers (`RpcClient.currentHeaders` beneath it), so a distributed hop preserves the whole W3C triple rather than `traceparent` alone.
- Law: the web-handler edge form is the platform surface composed at the app root — `HttpApiBuilder.toWebHandler(layer, options)` takes the app's implementation Layer (the one carrying `HttpApi.Api`) and yields the `Request => Response` arrow plus its `dispose` for fetch-shaped runtimes, and no `Emit` member renames it because a forwarding member is the one-hop wrapper this corpus deletes; the full-server form (api beside raw routes) is `route#SERVE_FOLD`'s `HttpLayerRouter.toWebHandler`.
- Law: derivation is call-time and parameterized — nothing here caches, names, or holds an api instance, keeping the assembled value's no-lib-side-existence law intact; contract documentation is annotation material on the api value (`HttpApi.make(id).annotate`, endpoint schema annotations) flowing into the document through the derivation.
- Growth: a new documentation surface is one `_uis` row under the same two axes; a new dial is one `_dials` row; a new derivation family (a JSON-schema bundle per owner) is one member over the same api parameter.
- Boundary: `@connectrpc/connect-node` derives no peer at this owner — its `connectNodeAdapter` server half is `live#MOUNT_PORT`'s row under the one-front-door ruling, and `net/client#NODE_ADAPTER` supplies its public client factories to `core:interchange/invoke#DIAL_AXIS`, the branch's one Connect transport owner keyed by an emitted `DescService` rather than by a contributed `RpcGroup`; both dial families spend this branch's one egress budget and share nothing else.
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
  type Temper = <E, R>(client: HttpClient.HttpClient.With<E, R>) => HttpClient.HttpClient.With<E, R>
  type Origin = { readonly url: string }
  type Dial = keyof typeof _dials
  type Docs = { readonly api: HttpApi.HttpApi.Any; readonly path: `/${string}` }
  type Mount = Layer.Layer<never, never, HttpApi.Api | HttpLayerRouter.HttpRouter>
  type UiRow = { readonly native: boolean; readonly layer: (options: Emit.Docs) => Emit.Mount }
  type Ui = keyof typeof _uis
  type NativeUi = { [K in Ui]: (typeof _uis)[K]["native"] extends true ? K : never }[Ui]
}

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
    row.native ? row.layer(options) : Layer.mergeAll(HttpApiBuilder.middlewareOpenApi(), row.layer(options)))

const _client = <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
  api: HttpApi.HttpApi<Id, Groups, E, R>,
  options: { readonly baseUrl: string; readonly transform: Emit.Temper },
) => HttpApiClient.make(api, { baseUrl: options.baseUrl, transformClient: options.transform })

const _RECONNECT: Schedule.Schedule<unknown, Socket.SocketError> = Schedule.exponential(Duration.millis(500), 1.5).pipe(
  Schedule.union(Schedule.spaced(Duration.seconds(5))),
  Schedule.jittered,
)

const _socketDial = (dial: Layer.Layer<Socket.Socket>) =>
  RpcClient.layerProtocolSocket({ retrySchedule: _RECONNECT }).pipe(Layer.provide(dial))

const _dials = {
  http: (origin: Emit.Origin & { readonly transform?: Emit.Temper }) =>
    RpcClient.layerProtocolHttp({ url: origin.url, transformClient: origin.transform }),
  socket: _socketDial,
  websocket: (origin: Emit.Origin) =>
    _socketDial(Socket.layerWebSocket(origin.url).pipe(Layer.provide(Socket.layerWebSocketConstructorGlobal))),
  worker: (pool: Parameters<typeof RpcClient.layerProtocolWorker>[0]) => RpcClient.layerProtocolWorker(pool),
} as const

const _caller = <G extends RpcGroup.RpcGroup.Any, E, R>(group: G, dial: Layer.Layer<RpcClient.Protocol, E, R>) =>
  Effect.provide(RpcClient.make(group), dial)

const _traced = <A, E, R>(call: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
  RpcClient.withHeadersEffect(
    Effect.flatMap(Propagation.current, (context) =>
      Effect.map(
        Effect.optionFromOptional(Effect.currentSpan),
        (span) =>
          Carrier.inject(
            "connect",
            context,
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contribution, Current, Emit, Gate, GateFault, Principal, Surface }
```

## [08]-[RESEARCH]

(none)
