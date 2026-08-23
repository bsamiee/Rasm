# [RUNTIME_ROUTE]

This serving assembly: routes are Layers under `HttpLayerRouter` — the app-assembled `HttpApi` mounts through `addHttpApi` beside raw routes, foreign realtime protocols mount through the `Mount` port fold, the resumable-upload rail mounts its tus dispatchers, the health trio serves the probe anchor, the webhook intake holds raw octets for signature verification, and the auth ceremonies lift the security wave's redirect and passkey round-trips into HTTP. One global seam owns mark, ambient provision, trace continuation, general credential admission, tenancy, upload bounds, priced admission, shield headers, route attribution, and the respondable net; the webhook's protocol-mandated query token composes the same auth owner at its matched route. Host and header dispatch across several apps is a `HttpMultiplex` catch-all route; static assets serve under an address-first immutable cache selector, list-aware revalidation, traversal refusal, and the weak `Etag.Generator` that row mounts for itself. This engine is never named here — `HttpLayerRouter.serve` demands the `HttpServer` the boot module provides from `proc/exec#RUNTIME_ROWS`, so a runtime change is a row selection at the root and the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same route Layers. This module ships on the `./server` exports subpath as `runtime/src/serve/route.ts`.

## [01]-[INDEX]

- [02]-[SEAM_ROWS]: `Seam` — mark, ambient, trace, admission, upload bound, priced admission, derived shield, route stamp, and net rows.
- [03]-[LAYER_ROUTES]: api/docs/health/tus/mount route Layers, the bounded webhook intake row; `Router`, `Inbound`.
- [04]-[CEREMONY_ROWS]: oauth redirect pair, webauthn enroll/assert, refresh/logout, cookie application; `Ceremony`.
- [05]-[ASSET_ROWS]: `Router.assets` — address-first cache selector, list-aware revalidation, traversal refusal.
- [06]-[SERVE_FOLD]: multiplex rows, the serve Layer, the web-handler twin; `Router`.

## [02]-[SEAM_ROWS]

[SEAM_ROWS]:
- Owner: `Seam` — the one cross-cutting composition over one `Seam.Policy` value: `Seam.guard(policy)(app)` mints the request mark (id, instant, negotiated locale from the `accept-language` header against the ambient fallback), provides the `Current` rows in one scoped provision, continues the W3C trace through `Current.traced` over the request headers, bounds every multipart read through the policy's `uploads` row as fiber-ref policy, folds every escaping cause through `Problem.net` — self-rendering first, total ladder as the floor — and stamps the derived shield headers on every response; the served app's error channel is `never` by construction. `Seam.admission(identity)` is its credential companion, `Seam.routed` the route-attribution row, and `Seam.priced`/`Seam.Priced` the priced-admission pair, so the whole cross-cutting stack composes once at the serve fold and nothing per-handler restates it.
- Law: pricing REFUSES at this edge — a burst caller leaves as a Problem-rendered 429 rather than as a held socket, because a delayed request occupies the connection the in-flight cap is separately defending; `work/queue#THROTTLE` takes the DELAYING posture over the same store and the same four columns (`window`, `limit`, `key`, `cost`), so the branch spends one quota vocabulary across two postures and a row's `scope` names which side it sits on. `api#ADMISSION_ROWS`'s one `Gate.fenced` price is the refusal — the limiter's `"Exceeded"` arm re-spelled as a `rate` `GateFault` carrying its own measured `after` — so this page mints no refusal, and the seam's net renders the class the governed record already grades with a truthful `retry-after`.
- Law: a quota row states its FAN AXIS and this seam derives both projections — `principal` prices a holder's whole traffic, `route` prices that holder on the matched pattern, and every row costs the pattern's own declared weight, so a row carries scope, algorithm, window, limit, and one word. Buckets join the row's `scope` to its projection because the limiter store takes `key` verbatim and namespaces nothing — the identical join the work plane folds, without which two rows fanning on different axes spend one another's tokens the moment their values coincide.
- Law: the priced coordinates first exist INSIDE the match, so pricing rides the same two seats route attribution does — `Seam.priced(policy)` for a raw row and the `Seam.Priced` api middleware for every `addHttpApi` endpoint — because `RouteContext` carries the pattern only after a match and `Current.Admitted` carries the credential the admission row already lifted. Patterns the policy's weight record never names spend nothing at all, so the probe trio, the asset tree, and the mounted foreign protocols stay unpriced by omission rather than through a second exempt roster kept in step by hand.
- Law: the holder is the credential where one exists and the caller address everywhere else, which is what makes `fronted` load-bearing a second time — an unfronted origin honoring `x-forwarded-for` hands one caller a fresh bucket per request and prices nobody, so the row deciding dispatch and audit coordinates decides whose bucket an anonymous request spends; exempting anonymous traffic instead exempts exactly the callers the table exists to bound.
- Law: the limiter store is a PORT this page never backs — the edge rows and the work plane's rows resolve the one `Fleet.RateLimiter` Tag the composition root binds (`layerStoreMemory` on a single node, a shared store-backed Layer across a fleet), so a fleet-wide bucket is a root selection and no concrete store sits below it, and that Tag rides every route Layer's requirement channel because a per-route seat cannot be conditional on a weight the record may not carry, which is the same unconditional shape `Mount.Lift` and `Life` already take here.
- Law: the shield splits by what a header IS — the four transport-fixed rows (`strict-transport-security`, `x-content-type-options`, `x-frame-options`, `referrer-policy`) are literals, and the CSP DERIVES from the export policy through `Seam.shield(shield)`: `default-src 'self'` and `frame-ancestors 'none'` fix the frame, `script-src` grants `'wasm-unsafe-eval'` because CSP3 demands it for the `WebAssembly.instantiate` the served decoder leaves run, `connect-src` assembles the collector origin beside the cross-origin API roster the deployment already declares, and `worker-src`/`img-src` join the asset origin wherever the multiplex row places assets on a second host. Moving a collector or adding an API origin therefore edits no header, and the standing clause — a handler hand-setting a shield header is the drift defect — becomes enforceable instead of aspirational.
- Law: the shield row derives, never restates — `Seam.shieldOf(policy, origins)` reads the collector ORIGIN off `otel/emit#POLICY`'s own `collector.baseUrl`, so the estate spells the collector once; the `propagate` roster stays anchored `RegExp` patterns for the SDK's `urlMatches` compare and cannot serve a CSP source list, so the cross-origin API origins arrive as the explicit `connect` roster the same app root assembles both from.
- Law: `Seam.admission` is the tenancy binder every sibling folder names as "the edge" — it composes `Gate.Authn.admit(identity, request.headers)` ONCE per request, provides the result through `Current.Admitted` so the api's scheme arms project rather than re-verify, and wraps the whole downstream in `TenantScope.bind` under `TenantScope.metered` on the admitted arm; an anonymous request binds nothing and the unscoped default answers, because refusal belongs to the endpoint that declares a scheme, never to this seam.
- Law: the cookie presentation pays its double-submit proof HERE and nowhere else — an admitted `via` of `"cookie"` on a method outside `GET`/`HEAD`/`OPTIONS` runs `Cookie.verify` over the one `CookieSpec.csrf` row (the cookie by `name`, the echo by `header`) before the downstream binds, and an absent or mismatched pair refuses `unauthorized` through `Gate.refuse` so the seam's own refusal series counts it; a bearer or api-key presentation carries a header no cross-site form can replay and pays nothing, and the ceremony rows' `_csrfed` fold gates the round-trips that run before any admission exists over that same one `Cookie.verify` owner.
- Law: `Seam.routed` is what makes `http.route` producible — `@opentelemetry/instrumentation-http` installs the `RPCMetadata` record under `RPCType.HTTP` and reads `route` off it at response end to build BOTH the span attribute and the duration histogram's own dimension, and no published hook fills that field: `startIncomingSpanHook` fires before any route matches, `requestHook`/`responseHook` see the node message alone, and `applyCustomAttributesOnSpan` runs PAST the metric-attribute build, so an attribute set there decorates the span while the RED plane still ships route-less. This row therefore writes the matched pattern onto the record under the same `RPCType.HTTP` discriminant the reader compares, attaching per route because `RouteContext` exists only after a match, and covering `addHttpApi`'s endpoints through the api-level `Seam.Routed` Tag.
- Law: forwarded-header trust is a policy ROW, never a default — `fronted` selects `HttpMiddleware.xForwardedHeaders`, which rewrites `host` from `x-forwarded-host` and the caller address from the first `x-forwarded-for` hop, so a proxied deployment dispatches and audits on the PUBLIC coordinates while an unfronted origin refuses the rewrite outright. Both header names stay caller-writable: a default-on row hands any caller its own virtual host, and a default-off row collapses every multiplex predicate onto the ingress hostname behind a load balancer.
- Law: CORS is delegated, never re-implemented — the assembly composes `HttpLayerRouter.cors()` (or `HttpApiBuilder.middlewareCors(options)` on the api mount) with the options row as its one policy value; no `Seam` member renames it, because a forwarding member is the one-hop wrapper the platform surface already owns.
- Growth: a new cross-cutting response concern is one line in `Seam.guard`, inherited by every route Layer at once; a new CSP directive is one `_directives` row; a new priced axis is one `_AXES` entry and the `_QUOTA` row spreading it.
- Packages: `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`, `HttpApiMiddleware`, `HttpMiddleware`, `HttpLayerRouter`, `Multipart`); `@effect/experimental` (`RateLimiter` — the store Tag the priced rows resolve); `@opentelemetry/core` (`getRPCMetadata`, `RPCType`); `@opentelemetry/api` (`context`); `effect` (`DateTime`, `Duration`, `Effect`, `Option`, `Array`, `identity`, `pipe`); `@rasm/ts/security` (`Cookie`, `CookieSpec`, `TenantScope`); `./api.ts` (`Current`, `Gate`, `GateFault`); `../otel/emit.ts` (`Export`).

```typescript signature
import { Buffer } from "node:buffer"
import { context } from "@opentelemetry/api"
import { getRPCMetadata, RPCType } from "@opentelemetry/core"
import { CONSTANTS, type CloudEvent, HTTP, type Message } from "cloudevents"
import { RateLimiter as Fleet } from "@effect/experimental"
import {
  type Cookies, Etag, FileSystem, type HttpApi, type HttpApiGroup, HttpApiMiddleware, HttpIncomingMessage,
  HttpLayerRouter, HttpMiddleware, HttpMultiplex, type HttpPlatform, HttpServerRequest, HttpServerResponse, Multipart, Path,
} from "@effect/platform"
import { type RpcGroup, RpcServer } from "@effect/rpc"
import {
  Array, Context, Data, DateTime, Duration, Effect, Encoding, Layer, Match, Number, Option, Predicate, Record, Redacted, Schema,
  identity, pipe,
} from "effect"
import { Carrier, Event, Format, type Identity } from "@rasm/ts/core"
import { Claim, Cookie, CookieSpec, Departed, Jwt, type MacKey, OAuth, TenantScope, Token, type Verified, Verify, WebAuthn } from "@rasm/ts/security"
import { Dataref, Journal, Rail } from "@rasm/ts/data"
import { Avro } from "../net/channel.ts"
import { WebhookOrigin } from "../net/client.ts"
import { InboundHeaders } from "../proc/exec.ts"
import { Life } from "../proc/life.ts"
import { type Export, Propagation } from "../otel/emit.ts"
import { Current, Emit, Gate, GateFault, type Principal } from "./api.ts"
import { Mount } from "./live.ts"
import { Problem } from "./problem.ts"

type _Idempotency = InstanceType<typeof Gate.Idempotency>

const _FIXED = {
  "strict-transport-security": "max-age=63072000; includeSubDomains",
  "x-content-type-options": "nosniff",
  "x-frame-options": "DENY",
  "referrer-policy": "strict-origin-when-cross-origin",
} as const satisfies Record<string, string>

declare namespace Seam {
  type Shield = {
    readonly collector: string
    readonly connect: ReadonlyArray<string>
    readonly assets: Option.Option<string>
  }
  // One priced subject per request: the holder the credential lift bound (the caller address when anonymous), the
  // matched pattern, and the cost that pattern declared — so a row projects what the edge already holds in hand.
  type Subject = {
    readonly holder: string
    readonly route: string
    readonly weight: number
  }
  // Four columns every rate posture in the branch shares, three of them projections a row's axis mints — `limit`
  // reads the subject exactly as `key` and `cost` do, matching `work/queue#THROTTLE`'s row so a ceiling this estate
  // fixes and a ceiling a peer states inhabit one grammar, and a row this estate bounds answers a constant.
  type Quota = {
    readonly scope: string
    readonly algorithm: "fixed-window" | "token-bucket"
    readonly window: Duration.DurationInput
    readonly limit: (subject: Subject) => number
    readonly key: (subject: Subject) => string
    readonly cost: (subject: Subject) => number
  }
  type Policy = {
    readonly shield: Shield
    // rows carry the ceilings a fleet tunes per environment; `weight` carries the per-pattern cost, and a pattern
    // this record never names is unpriced — the exemption is an absence, never a second roster
    readonly quota: {
      readonly rows: ReadonlyArray<Quota>
      readonly weight: Readonly<Record<string, number>>
    }
    readonly uploads: Multipart.withLimits.Options
    readonly fronted: boolean
  }
}

// every directive whose sources are deployment facts reads them here; `wasm-unsafe-eval` is what CSP3 demands
// for the decoder leaves the served asset tree instantiates
const _directives = (shield: Seam.Shield): ReadonlyArray<readonly [string, ReadonlyArray<string>]> => [
  ["default-src", ["'self'"]],
  ["frame-ancestors", ["'none'"]],
  ["script-src", ["'self'", "'wasm-unsafe-eval'", ...Option.toArray(shield.assets)]],
  ["connect-src", ["'self'", shield.collector, ...shield.connect]],
  ["worker-src", ["'self'", "blob:", ...Option.toArray(shield.assets)]],
  ["img-src", ["'self'", "data:", "blob:", ...Option.toArray(shield.assets)]],
]

const _shield = (shield: Seam.Shield): Record<string, string> => ({
  ..._FIXED,
  "content-security-policy": pipe(
    _directives(shield),
    Array.map(([directive, sources]) => `${directive} ${Array.join(Array.dedupe(sources), " ")}`),
    Array.join("; "),
  ),
})

// One spelling holds the collector, in the export policy: this projection is the only place the serving edge reads it
const _shieldOf = (
  policy: Export.Policy,
  origins: { readonly connect: ReadonlyArray<string>; readonly assets: Option.Option<string> },
): Seam.Shield => ({
  collector: new URL(policy.collector.baseUrl).origin,
  connect: origins.connect,
  assets: origins.assets,
})

// One partial application per policy: the directive join and the forwarded-trust selection resolve at composition,
// so a served response stamps a frozen header record rather than re-joining six directive source lists per hop.
const _guard = (policy: Seam.Policy) => {
  const stamped = _shield(policy.shield)
  // Fronted deployments read their PUBLIC host and the caller address off the proxy's own hops — the values the
  // multiplex predicates match and every audit key carries — while an unfronted origin refuses the same rewrite,
  // because those headers stay caller-writable and a forged one selects its own virtual host.
  const forwarded = policy.fronted ? HttpMiddleware.xForwardedHeaders : identity
  return <E, R>(
    app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R | HttpServerRequest.HttpServerRequest>,
  ): Effect.Effect<HttpServerResponse.HttpServerResponse, never, R | HttpServerRequest.HttpServerRequest> =>
    forwarded(Effect.gen(function* () {
      const request = yield* HttpServerRequest.HttpServerRequest
      const now = yield* DateTime.now
      // `try`, never `sync`: a host without a secure random source throws at the mint, and the seam's own net
      // renders that refusal instead of a defect the `never` channel cannot state
      const id = yield* Effect.try(() => crypto.randomUUID())
      const fallback = yield* Current.Locale
      const mark: Current.Mark = {
        id,
        at: now,
        locale: Option.some(Current.negotiate(Option.fromNullable(request.headers["accept-language"]), fallback)),
      }
      return yield* Current.traced(app, request.headers).pipe(
        // one fiber-ref bound for every mounted route, so an upload endpoint declares its shape and never its ceiling
        Multipart.withLimits(policy.uploads),
        Effect.catchAllCause(Problem.net),
        (guarded) => Current.provide(guarded, mark, fallback),
      )
    }).pipe(
      // Mark minting sits ABOVE its own provision, so a refusal there renders stamp-less through the same net,
      // and the shield stamps at the one edge both arms leave through
      Effect.catchAllCause(Problem.net),
      Effect.map(HttpServerResponse.setHeaders(stamped)),
    ))
}

const _SAFE: ReadonlyArray<string> = ["GET", "HEAD", "OPTIONS"]

// Cookie presentation is the one AMBIENT credential a browser replays on any origin's behalf, so it alone owes the
// double-submit proof; a bearer or api-key header cannot be replayed cross-site and the gate costs it nothing. Both
// reads come off ONE CookieSpec row, and the refusal re-spells onto the gate family through its own counting seam.
const _doubled = (
  request: HttpServerRequest.HttpServerRequest,
  via: Principal.Shape["via"],
): Effect.Effect<void, GateFault, Cookie> =>
  via !== "cookie" || Array.contains(_SAFE, request.method)
    ? Effect.void
    : Effect.flatMap(Cookie, (cookie) =>
      cookie.verify(
        Option.fromNullable(request.cookies[CookieSpec.csrf.name]),
        Option.fromNullable(request.headers[CookieSpec.csrf.header]),
      ).pipe(Effect.catchAll((fault) =>
        Gate.refuse(new GateFault({ case: { reason: "unauthorized", via: `csrf:${fault.case.reason}` }, after: Option.none() })))))

const _admission = (identity: Identity.App) =>
  HttpLayerRouter.middleware<{ provides: Current.Admitted | TenantScope }>()(
    <E, R>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>) =>
      Effect.gen(function* () {
        const request = yield* HttpServerRequest.HttpServerRequest
        const admitted = yield* Gate.Authn.admit(identity, request.headers)
        // Verified credentials first exist HERE, so this is the ONE place tenancy can bind
        return yield* Option.match(admitted, {
          onNone: () => Effect.provideService(app, Current.Admitted, Option.none()),
          onSome: (held) =>
            Effect.zipRight(
              _doubled(request, held.principal.via),
              TenantScope.metered(TenantScope.bind(
                held.scope,
                Effect.provideService(app, Current.Admitted, Option.some(held)),
              )),
            ),
        })
      }),
    { global: true },
  )

// Http instrumentation owns the record and reads `route` off it at response end to build the span attribute
// AND the duration histogram's dimension; the write guards on `RPCType.HTTP` because that is the discriminant the
// reader compares, and a stamp that throws folds here rather than widening every mounted route's error channel
// ONE pattern projection, two readers: the RED plane's route dimension and the priced bucket must name the same
// string, or a chart and a quota disagree about which route a caller actually hit
type _Matched = Effect.Effect.Success<typeof HttpLayerRouter.RouteContext>

const _pattern = (matched: _Matched): string =>
  `${Option.getOrElse(matched.route.prefix, () => "")}${matched.route.path}`

const _stamp: Effect.Effect<void, never, HttpLayerRouter.RouteContext> = Effect.flatMap(
  HttpLayerRouter.RouteContext,
  (matched) =>
    Effect.try(() =>
      Option.match(
        Option.filter(Option.fromNullable(getRPCMetadata(context.active())), (held) => held.type === RPCType.HTTP),
        {
          onNone: () => undefined,
          onSome: (held) => Object.assign(held, { route: _pattern(matched) }),
        },
      )).pipe(Effect.ignoreLogged),
)

const _routed = <E, R>(
  method: "*" | "GET" | "POST" | "PUT" | "PATCH" | "DELETE" | "OPTIONS",
  path: HttpLayerRouter.PathInput,
  handler: (request: HttpServerRequest.HttpServerRequest) => Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>,
  options?: { readonly uninterruptible?: boolean },
) => HttpLayerRouter.add(method, path, (request) => Effect.zipRight(_stamp, handler(request)), options)

// This api arm answers `addHttpApi`, which publishes no per-endpoint hook: an api-level middleware runs inside the
// match where `RouteContext` carries the endpoint's own pattern
class Routed extends HttpApiMiddleware.Tag<Routed>()("runtime/serve/Routed") {
  static readonly live: Layer.Layer<Routed> = Layer.succeed(Routed, _stamp)
}

// Both projections derive from the ONE axis a row fans on: `principal` prices a holder's whole traffic, `route`
// prices that holder on the matched pattern, and every row costs the pattern's declared weight — the same generator
// shape `work/queue#THROTTLE` mints its rows from, over the same four columns under the other posture.
const _AXES = {
  principal: (subject: Seam.Subject): string => subject.holder,
  route: (subject: Seam.Subject): string => `${subject.holder}:${subject.route}`,
} as const

const _keyed = (axis: keyof typeof _AXES): Pick<Seam.Quota, "cost" | "key"> => ({
  cost: (subject) => subject.weight,
  key: _AXES[axis],
})

// Shipped pair: an app root spreads these values into `policy.quota.rows`; the axis vocabulary closes here
// against the two coordinates a request carries, while the ceilings stay policy because a fleet tunes them per
// environment and a row is named so a deployment can drop one without re-deriving the other
const _QUOTA = {
  principal: { scope: "edge-principal", algorithm: "token-bucket", window: Duration.minutes(1), limit: () => 600, ..._keyed("principal") },
  route: { scope: "edge-route", algorithm: "fixed-window", window: Duration.minutes(1), limit: () => 120, ..._keyed("route") },
} as const satisfies Record<string, Seam.Quota>

// What a caller carrying this pattern's declared weight actually gets per minute under one row: the row's own
// ceiling divided by what the pattern costs, normalized off the row's window so a five-second row and a one-minute
// row answer in one unit. This is the figure the webhook handshake grants, so the promise and the enforcement read
// one arithmetic rather than a number an app typed beside the table.
const _perMinute = (row: Seam.Quota, subject: Seam.Subject): number =>
  Math.max(1, Math.floor(row.limit(subject) / Math.max(1, row.cost(subject)) / Math.max(Duration.toMinutes(row.window), 1 / 60)))

// This grant PROMISES what the edge enforces, so it derives from the rows enforcing it: an app free to
// type a rate beside the quota table would advertise a ceiling the seam never keeps, and pacing against that
// advertised figure is the only thing the specification's rate half exists for. The TIGHTEST row governs, because
// that is the one a sender meets first, and a pattern the weight record never names spends nothing at all — `*` is
// then the truthful grant rather than a cap guessed over an unpriced route.
const _granted = (policy: Seam.Policy, route: string): string =>
  Option.match(Option.fromNullable(policy.quota.weight[route]), {
    onNone: () => "*",
    onSome: (weight) =>
      Array.match(Array.map(policy.quota.rows, (row) => _perMinute(row, { holder: "", route, weight })), {
        onEmpty: () => "*",
        onNonEmpty: (rates) => String(Array.reduce(rates, rates[0], (held, rate) => Math.min(held, rate))),
      }),
  })

// `Fleet.RateLimiter` takes `key` verbatim and namespaces nothing, so the row's scope joins its projection HERE — the
// identical join the work plane folds, which is what keeps one declared row on one bucket across both postures
const _bucket = (row: Seam.Quota, subject: Seam.Subject): string => `${row.scope}:${row.key(subject)}`

type _Admitted = Effect.Effect.Success<typeof Current.Admitted>

// Credentials key their own bucket and everything else keys the caller address, so anonymous traffic still prices
// rather than exempting exactly the callers the table exists to bound; the address is the POST-forwarding one, which
// is why `fronted` decides more than dispatch — an unfronted origin trusting the hop hands out a bucket per request.
const _holder = (request: HttpServerRequest.HttpServerRequest, admitted: _Admitted): string =>
  Option.match(admitted, {
    onNone: () => `addr:${Option.getOrElse(request.remoteAddress, () => "unknown")}`,
    onSome: (held) => `sub:${held.principal.subject}`,
  })

// Rows nest outward-in — the last row reduced wraps first and therefore spends first, so a refusal at the coarser
// bucket never spends the finer one's tokens. Every arm refuses through `Gate.fenced`'s one price, so the 429 and
// its measured `retry-after` are the admission plane's spelling and this seam adds no second refusal.
const _quota = (policy: Seam.Policy) =>
<A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<
  A,
  E | GateFault,
  R | Fleet.RateLimiter | HttpLayerRouter.RouteContext | HttpServerRequest.HttpServerRequest
> =>
  Effect.flatMap(HttpLayerRouter.RouteContext, (matched) =>
    pipe(_pattern(matched), (route) =>
      Option.match(Option.fromNullable(policy.quota.weight[route]), {
        onNone: () => self, // an unpriced pattern spends nothing at all: the probe trio and the asset tree stay free
        onSome: (weight) =>
          Effect.gen(function* () {
            const request = yield* HttpServerRequest.HttpServerRequest
            const admitted = yield* Current.Admitted
            const subject: Seam.Subject = { holder: _holder(request, admitted), route, weight }
            return yield* Array.reduce(policy.quota.rows, self, (held, row) =>
              Gate.fenced(held, {
                algorithm: row.algorithm,
                cost: row.cost(subject),
                key: _bucket(row, subject),
                limit: row.limit(subject),
                window: row.window,
              }))
          }),
      })))

// `addHttpApi` publishes no per-endpoint hook, so pricing takes the same api-level seat route attribution does:
// an api middleware runs INSIDE the match, where `RouteContext` carries the endpoint's own pattern
class Priced extends HttpApiMiddleware.Tag<Priced>()("runtime/serve/Priced", { failure: GateFault }) {
  static readonly live = (policy: Seam.Policy): Layer.Layer<Priced> => Layer.succeed(Priced, _quota(policy)(Effect.void))
}

const Seam = {
  Priced,
  Routed,
  admission: _admission,
  guard: _guard,
  priced: _quota,
  quota: _QUOTA,
  routed: _routed,
  shield: _shield,
  shieldOf: _shieldOf,
  stamp: _stamp,
} as const
```

## [03]-[LAYER_ROUTES]

[LAYER_ROUTES]:
- Owner: `Router` — the route-Layer vocabulary the app root merges: `Router.api(api, docs)` mounts the assembled `HttpApi` through `HttpLayerRouter.addHttpApi(api, { openapiPath })` and selects its reference UI through `api#EMIT`'s `_uis` roster, so the derived document and the UI ride the same router and the docs choice lives at one owner; `Router.rpc(group, mount)` mounts a contributed RPC group beside the raw routes through the fused `RpcServer.layerHttpRouter` owner — the package's own composition of the server Layer with whichever router-native protocol row the mount elects — so one router serves api, RPC, and raw rows without a second server and `api#CONTRIBUTION`'s protocol roster keeps only the rows that ARE their own listener; `Router.health` mounts the probe trio from `proc/life#PROBE_ROUTES`'s anchor — `Life.route(kind)` is the path, `Life.report(kind)` the body encoded through the `Life.Report` schema, `pass`/`warn` encode 200 and `fail` encodes 503, so the path and the verdict never exist twice; `Router.mounts` folds `Effect.serviceOption(Mount)` and mounts every provided foreign-protocol row at its prefix under the `"*"` catch-all method literal — presence-as-data, an unwired port serves nothing and never crashes.
- Law: every raw route on this page mounts through `Seam.routed`, never `HttpLayerRouter.add` directly, so the matched pattern reaches the RED plane's route dimension from the one place that knows it; a bare `add` is the drift defect this constructor forecloses. Its api half is the assembly's one `HttpApi.middleware(Seam.Routed)` declaration against the Layer `Router.api` already merges, so every endpoint stamps its own pattern without a per-endpoint hook the platform does not publish; `HttpApi.middleware(Seam.Priced)` is that same seat spent a second time for the quota, and it declares beside the stamp because both read the one coordinate a match produces — the difference is that pricing carries a policy, so its Layer merges at the root rather than inside `Router.api`.
- Law: the tus rail mounts as dispatchers, never re-frames — `Router.rail(spec)` builds the data rail (`Rail.of(spec)`) and delegates its value to `Router.RailMount`, the port whose selected runtime row routes every method under the spec's route prefix into the rail's own dispatchers and schedules `rail.groom` through the lifecycle plane; the node lift is NOT this port's, it is `live#MOUNT_PORT`'s one `Mount.node` member, so a fetch engine drives `rail.web(request)` through `HttpApp.fromWebHandler` or `BunHttpServerRequest.toRequest` while the node engine composes that one adapter, and offset semantics, staging custody, and finalize stay the data rail's while this module names no binding.
- Law: `Inbound` is the held-octet webhook row — `Gate.Authn.webhook` first admits either the bearer header or `access_token` query carriage through the one credential owner, then the raw body reads once through the platform's byte accessor. The selected signature dialect verifies those exact octets before `settle`; authorization and signature remain independent proofs.
- Law: the spec's `ceiling` gates BEFORE and DURING the read — `HttpIncomingMessage.withMaxBodySize` scopes the platform collector itself and refuses an actual-byte overrun before an unbounded `arrayBuffer` can materialize. `Content-Length` is neither trusted nor required: chunked delivery remains legal and a lying declaration cannot bypass the bound. The platform read fault re-spells as the governed `malformed` class and answers 400; no local 413 override forks `problem#STATUS_RECORD`. The multipart ceiling every other route inherits is `[02]`'s `uploads` policy row.
- Law: detection precedes decode and reads the FRAME, never a trial parse — `Format.event.framed` recovers the core format owner's row-derived `Single | Batch` frame from one exact parsed media-type identity and the binding's own `ce-specversion` header names a binary frame whose `content-type` belongs to the data, so a frame naming neither refuses before any body is parsed. No route-owned `{ format, batch: boolean }` mirror exists; the package's `isEvent` pair runs a full deserialize inside `try`/`catch`, so a detect-then-decode pair parses every arrival twice.
- Law: binary and structured evidence are exclusive. A request carrying both refuses typed before decode; an unrecognized structured media type exits through `Problem.media` as 415 rather than malformed 400.
- Law: structured routing is explicit — JSON and Protobuf consume the admitted codecs under `Event.format`, while Avro consumes its bound singular codec; HTTP accepts only the optional batches those owners publish.
- Law: the header band is SANITIZED at the seam, never handed platform `Headers` — that abstraction has already lowercased names and comma-joined repeated field lines, so an array check over `request.headers` is dead code and cannot defend signature, origin, or CloudEvents attributes. `InboundHeaders` is the runtime row's duplicate-preserving capability: `_sanitized` refuses every value array whose cardinality is not exactly one and every case-fold collision before any binding read. Node supplies `IncomingMessage.headersDistinct`; Bun's Fetch source cannot recover the lost identity and fails this strict route closed rather than splitting commas that may belong to one legal value.
- Law: intake mints NOTHING — every structured codec crosses strict admission inside `Event.format`, binary binding results cross `Event.admit`, and `Event.rasm.Fact` plus `Event.rasm.read` supply the profile without a second model.
- Law: tenancy admits through the authenticated inverse, never inherits — the admitted webhook token supplies the scope, `Journal.carrier` re-proves the announcement's tenant claim against it, and the route seats that same `Admitted` value and `TenantScope` for downstream settlement regardless of token carriage.
- Law: authenticated identity does not prove event origin. Every `Inbound.Spec` supplies a `trust(principal, fact)` row that must admit the exact `(source,type)` claim before propagation or settlement; an app cannot omit source custody and a signature cannot authorize a producer namespace.
- Law: the application also supplies a nonempty classification roster. Missing or disallowed generated `dataclassification` refuses before source trust, propagation, or settlement.
- Law: webhook intake consumes the data plane's `Dataref` port before profile admission or application settlement. The port accepts only its configured HTTPS residence, verifies the resolved bytes against `subject`, and compares an inline twin byte-for-byte; a reference-only event is materialized through `Event.clone`, while a proved dual event retains its original carriage. No arbitrary URL fetch or route-local object client exists.
- Law: this route is the ONE ingress receiving W3C context as first-class ATTRIBUTES, so it runs `Carrier.extract("cloudevents", …)` over each admitted message envelope and hands the extraction WHOLE — parse census included — to `otel/emit#CONTINUATION`'s one ingress transformer, which continues that CREATION-time trace and spends the census once; the transport hop's own context already crossed at `Seam.guard`, and the two-trace law keeps both rather than folding either onto the other.
- Law: a batch settles per event — each admitted member first enters `Gate.Idempotency` under core `Event.address`, the one injective length-framed digest of `(source,id)`. A fresh address executes `Inbound.Spec.settle`; a replay bypasses it and answers `duplicate`. The 202 response carries one `Inbound.Settlement` per member, so accepted and duplicate members remain distinct, and a valid empty JSON batch answers an empty roster without inventing a refusal.
- Tests: HTTP admits JSON and generated Protobuf as single and batch, including an empty JSON batch; admits the exact Avro asset as single only; rejects Avro batch; re-admits every SDK/Avro result strictly; and compares cross-format event semantics rather than encoded bytes.
- Law: this route opts into the Webhook specification's abuse-protection handshake through its application-owned DNS-origin roster, and the granted RATE derives from `[02]`'s quota rows rather than standing as a field beside them — a promise about pacing is worth exactly what the edge enforces, so the same rows that refuse a burst caller state the ceiling a sender paces to, and an unpriced pattern grants `*` because it truthfully spends nothing. Origin is required and DNS-admitted on validation and delivery; a grant always carries allowed origin and rate together. Delivery supports both mandated bearer-token methods, refuses simultaneous or repeated token carriages, and stamps `Cache-Control: private` on a successful query-token response. Every delivery also carries a non-empty payload and `Content-Type`.
- Boundary: which groups the api value carries is the app's assembly under `api#CONTRIBUTION`; the `Mount` Tag is `live#MOUNT_PORT`'s; `InboundHeaders` is the selected `proc/exec#RUNTIME_ROWS` capability; the rail spec's cut policy and staging band are `data`'s; the attribute grammar, extension roster, and mint entry are `core:interchange/carrier#EVENT_ENVELOPE`'s.
- Growth: a new served surface is one route-Layer member composing an owning-page value; a second foreign protocol is a second `Mount` Layer at a different prefix, zero edits here.
- Packages: `@effect/platform`, `cloudevents` (`HTTP`, `CONSTANTS`), `effect`, `node:buffer`, `@rasm/ts/core` (`Carrier`, `Event`, `Format`), `@rasm/ts/data`, `@rasm/ts/security`, and `../net/channel.ts` (`Avro` — the lane-owned Avro codec).

```typescript signature
// The platform body collector reads this fiber-local ceiling while materializing `arrayBuffer`, so the bound applies
// to actual bytes (including chunked delivery) and a missing or lying Content-Length can neither refuse nor bypass it.
const _octets = (
  request: HttpServerRequest.HttpServerRequest,
  ceiling: FileSystem.SizeInput,
): Effect.Effect<Uint8Array, Problem> =>
  HttpIncomingMessage.withMaxBodySize(
    Effect.map(request.arrayBuffer, (buffer) => new Uint8Array(buffer)),
    Option.some(ceiling),
  ).pipe(
    Effect.mapError(() => Problem.of({ class: "malformed", message: "webhook body refused" })),
  )

const _health: Layer.Layer<never, never, Life | HttpLayerRouter.HttpRouter> = Layer.mergeAll(
  ...Array.map(["started", "ready", "live"] as const, (kind) =>
    _routed("GET", Life.route(kind), () =>
      Effect.flatMap(Life.report(kind), (report) =>
        HttpServerResponse.schemaJson(Life.Report)(report, { status: report.overall === "fail" ? 503 : 200 }).pipe(Effect.orDie))),
  ),
)

// Rows FOLD over this empty seat rather than spreading into `Layer.mergeAll`, whose tuple parameter no runtime row
// list can promise an arity for: a `Mount.of()` carrying nothing serves an app with nothing mounted.
const _UNMOUNTED: Layer.Layer<never, never, HttpLayerRouter.HttpRouter | Mount.Lift> = Layer.empty

// Runtime capability owns the lift rather than any route, so this fold reads it ONCE at Layer construction and
// hands it to every mounted row: a served route then carries a bare requirement channel, and a runtime row
// binding no node pair refuses at assembly instead of inside a request.
const _mounts: Layer.Layer<never, never, HttpLayerRouter.HttpRouter | Mount.Lift> = Layer.unwrapEffect(
  Effect.gen(function* () {
    const rows = yield* Effect.serviceOption(Mount)
    const lift = yield* Effect.context<Mount.Lift>()
    return Option.match(rows, {
      onNone: () => _UNMOUNTED,
      onSome: (mounts) =>
        Array.reduce(mounts, _UNMOUNTED, (held, mount) =>
          Layer.merge(
            held,
            // Mounted apps raise the realtime family: `Problem.of` reads the class the fault already carries,
            // so a foreign protocol's refusal answers at the same status every route on this page does
            _routed("*", `${mount.prefix}/*`, () => Effect.provide(Effect.mapError(mount.app, Problem.of), lift)),
          )),
    })
  }),
)

declare namespace Inbound {
  // One admitted member carries the arriving message envelope, its grammar-proven addressed record, the whole
  // roster read, every peer name this roster misses, and the CREATION-time context its extensions held.
  type Admitted = {
    readonly envelope: CloudEvent<unknown>
    readonly fact: Schema.Schema.Type<typeof Event.rasm.Fact>
    readonly roster: Event.Roster
    readonly dropped: Event.Read["dropped"]
    // the extraction WHOLE — its parse census rides beside the context, and `Propagation.ingress` is what spends it
    readonly carrier: Carrier.Extraction
  }
  type Frame = Data.TaggedEnum<{
    Structured: { readonly frame: Format.Event.Frame }
    Binary: {}
  }>
  type Spec = {
    readonly route: `/${string}`
    readonly ceiling: FileSystem.SizeInput
    readonly dialect: Verify.Dialect
    readonly header: string
    readonly mac: Option.Option<MacKey>
    readonly tolerance: Duration.Duration
    readonly classes: Array.NonEmptyReadonlyArray<Event.Class>
    // Application policy opts this route into abuse protection: one admitted DNS expression roster gates both the
    // validation grant and every delivery request.
    readonly origins: Array.NonEmptyReadonlyArray<typeof WebhookOrigin.Type>
    readonly trust: (
      principal: Principal.Shape,
      fact: Schema.Schema.Type<typeof Event.rasm.Fact>,
    ) => Effect.Effect<void, Problem>
    readonly settle: (admitted: Inbound.Admitted, verified: Verified) => Effect.Effect<void, Problem>
  }
  type Settlement = _Settlement
}

const _Frame = Data.taggedEnum<Inbound.Frame>()

class _Settlement extends Schema.Class<_Settlement>("Inbound.Settlement")({
  source: Schema.NonEmptyString,
  id: Schema.NonEmptyString,
  disposition: Schema.Literal("accepted", "duplicate"),
}) {}
// Abuse protection uses the specification's vocabulary; the origin roster, rate, and signature policy remain app data.
const _WEBHOOK = {
  origin: "webhook-request-origin",
  callback: "webhook-request-callback",
  rate: "webhook-request-rate",
  allowedOrigin: "webhook-allowed-origin",
  allowedRate: "webhook-allowed-rate",
} as const

const _eventProblem = (detail: string): Problem => Problem.of({ class: "malformed", message: detail })
const _eventRefused = (refusal: Event.Refusal): Problem => _eventProblem(refusal.message)

// Platform `Headers` has already joined duplicates. The selected runtime row must expose the field-line arrays before
// that lossy normalization; this fold then admits exactly one value per case-folded name.
const _sanitized: Effect.Effect<Message["headers"], Problem, InboundHeaders | HttpServerRequest.HttpServerRequest> =
  Effect.flatMap(InboundHeaders, ({ distinct }) => distinct).pipe(
    Effect.mapError((fault) => _eventProblem(fault.message)),
    Effect.flatMap((headers) => {
      const held = Array.filterMap(Object.entries(headers), ([name, values]) =>
        values === undefined ? Option.none() : Option.some({ name: name.toLowerCase(), values }))
      const names = Array.map(held, ({ name }) => name)
      const repeated = Array.findFirst(held, ({ name, values }) =>
        values.length !== 1 || values[0] === undefined || names.indexOf(name) !== names.lastIndexOf(name))
      return Option.match(repeated, {
        onSome: ({ name }) => Effect.fail(_eventProblem(`<repeated-header:${name}>`)),
        onNone: () => Effect.succeed(Record.fromEntries(Array.filterMap(held, ({ name, values }) =>
          Option.map(Option.fromNullable(values[0]), (value) => [name, value] as const)))),
      })
    }),
  )

const _header = (headers: Message["headers"], name: string): Option.Option<string> =>
  Option.filter(Option.fromNullable(headers[name.toLowerCase()]), Predicate.isString)

const _requestRate = (headers: Message["headers"]): boolean =>
  Option.match(_header(headers, _WEBHOOK.rate), {
    onNone: () => true,
    onSome: (value) => Option.exists(Number.parse(value), (rate) => globalThis.Number.isInteger(rate) && rate > 0),
  })

const _delivery = (headers: Message["headers"], body: Uint8Array): Effect.Effect<void, Problem> =>
  body.byteLength === 0 || Option.isNone(_header(headers, CONSTANTS.HEADER_CONTENT_TYPE))
    ? Effect.fail(_eventProblem("<webhook-payload-and-content-type-required>"))
    : Effect.void

// Detection reads the frame and nothing else: exact parsed media identity carries the core owner's closed frame for
// a structured or batch frame, while a binary frame declares itself with the binding's own specversion header and leaves
// `content-type` to the DATA — so a frame naming neither refuses before a single body byte is parsed.
const _framing = (headers: Message["headers"]): Effect.Effect<Inbound.Frame, Problem> => {
  const binary = Option.isSome(_header(headers, CONSTANTS.CE_HEADERS.SPEC_VERSION))
  const structured = Option.flatMap(_header(headers, CONSTANTS.HEADER_CONTENT_TYPE), Format.event.framed)
  if (binary && Option.isSome(structured)) {
    return Effect.fail(_eventProblem("<conflicting-binary-and-structured-event-evidence>"))
  }
  if (binary) return Effect.succeed(_Frame.Binary())
  return Option.match(structured, {
    onNone: () => Effect.fail(Problem.media("<unsupported-cloudevents-media>")),
    onSome: (frame) => Effect.succeed(_Frame.Structured({ frame })),
  })
}

const _jsonDecoded = (
  body: Uint8Array,
  frame: Extract<Format.Event.Frame, { readonly format: "json" }>,
): Effect.Effect<ReadonlyArray<CloudEvent<unknown>>, Problem> =>
  frame._tag === "Batch"
    ? Option.match(Event.format.json.batch, {
      onNone: () => Effect.fail(_eventProblem("<batch-unsupported:json>")),
      onSome: ({ codec }) => Schema.decodeUnknown(codec)(body).pipe(
        Effect.mapError((issue) => _eventProblem(`<json-rejected:${issue.message}>`)),
      ),
    })
    : Schema.decodeUnknown(Event.format.json.single)(body).pipe(
      Effect.mapError((issue) => _eventProblem(`<json-rejected:${issue.message}>`)),
      Effect.map(Array.of),
    )

const _protobufDecoded = (
  body: Uint8Array,
  frame: Extract<Format.Event.Frame, { readonly format: "protobuf" }>,
): Effect.Effect<ReadonlyArray<CloudEvent<unknown>>, Problem> =>
  frame._tag === "Batch"
    ? Option.match(Event.format.protobuf.batch, {
      onNone: () => Effect.fail(_eventProblem("<batch-unsupported:protobuf>")),
      onSome: ({ codec }) => Schema.decodeUnknown(codec)(body).pipe(
        Effect.mapError((issue) => _eventProblem(`<protobuf-rejected:${issue.message}>`)),
      ),
    })
    : Schema.decodeUnknown(Event.format.protobuf.single)(body).pipe(
      Effect.mapError((issue) => _eventProblem(`<protobuf-rejected:${issue.message}>`)),
      Effect.map(Array.of),
    )

const _avroDecoded = (
  body: Uint8Array,
  _frame: Extract<Format.Event.Frame, { readonly format: "avro" }>,
): Effect.Effect<ReadonlyArray<CloudEvent<unknown>>, Problem> =>
  Schema.decodeUnknown(Avro.single)(body).pipe(
    Effect.mapError((issue) => _eventProblem(`<avro-rejected:${issue.message}>`)),
    Effect.map(Array.of),
  )

const _binaryDecoded = (
  headers: Message["headers"],
  body: Uint8Array,
): Effect.Effect<ReadonlyArray<CloudEvent<unknown>>, Problem> =>
  Effect.try({
    try: () => HTTP.toEvent<unknown>({ headers, body: Buffer.from(body) }),
    catch: (caught) => _eventProblem(String(caught)),
  }).pipe(
    Effect.flatMap((decoded) =>
      globalThis.Array.isArray(decoded)
        ? Effect.fail(_eventProblem("<batch-unsupported:binary>"))
        : Event.admit(decoded)),
    Effect.mapError((fault) => fault instanceof Problem ? fault : _eventRefused(fault)),
    Effect.map(Array.of),
  )

// Detection decides routing once. Each structured format receives its exact codec, while binary mode remains the
// official transport binding; no structured payload is fabricated into an HTTP message for a decoder that owns a
// different format.
const _decoded = (
  headers: Message["headers"],
  body: Uint8Array,
  frame: Inbound.Frame,
): Effect.Effect<ReadonlyArray<CloudEvent<unknown>>, Problem> =>
  _Frame.$match(frame, {
    Binary: () => _binaryDecoded(headers, body),
    Structured: ({ frame: structured }) => Match.value(structured).pipe(
      Match.when({ format: "json" }, (frame) => _jsonDecoded(body, frame)),
      Match.when({ format: "protobuf" }, (frame) => _protobufDecoded(body, frame)),
      Match.when({ format: "avro" }, (frame) => _avroDecoded(body, frame)),
      Match.exhaustive,
    ),
  })

const _datarefUtf8 = new TextEncoder()
const _inline = (envelope: CloudEvent<unknown>): Effect.Effect<Option.Option<Uint8Array>, Problem> => {
  if (envelope.data instanceof Uint8Array) return Effect.succeedSome(envelope.data)
  if (Predicate.isString(envelope.data)) return Effect.succeedSome(_datarefUtf8.encode(envelope.data))
  if (envelope.data === undefined && Predicate.isString(envelope.data_base64)) {
    return Effect.mapError(
      Effect.map(Effect.fromEither(Encoding.decodeBase64(envelope.data_base64)), Option.some),
      () => _eventProblem("<dataref-inline-base64-refused>"),
    )
  }
  return envelope.data === undefined
    ? Effect.succeedNone
    : Effect.fail(_eventProblem("<dataref-inline-data-must-be-octets-or-text>"))
}

const _resolved = (
  envelope: CloudEvent<unknown>,
  roster: Event.Roster,
): Effect.Effect<CloudEvent<unknown>, Problem, Dataref> =>
  roster.dataref === undefined
    ? Effect.succeed(envelope)
    : Effect.gen(function* () {
      const subject = yield* Predicate.isString(envelope.subject)
        ? Schema.decode(Event.rasm.subject)(envelope.subject).pipe(Effect.mapError((issue) => _eventProblem(issue.message)))
        : Effect.fail(_eventProblem("<dataref-subject-required>"))
      const residence = yield* Dataref
      const receipt = yield* residence.resolve({
        source: envelope.source,
        id: envelope.id,
        subject,
        reference: roster.dataref,
        inline: yield* _inline(envelope),
      }).pipe(Effect.mapError((fault) => Problem.of(fault)))
      return yield* receipt.carriage === "dual"
        ? Effect.succeed(envelope)
        : Effect.mapError(Event.clone(envelope, { data: receipt.bytes }, ["data_base64"]), _eventRefused)
    })

// `data:journal/append#RELAY_ROWS` owns the authenticated inverse and this route is its consumer: the arriving
// announcement's tenant claim must EQUAL the admitted webhook token's scope before the fact enters, so a peer
// announcing another tenant's fact refuses HERE rather than at the projection it would otherwise reach. The extraction stays
// whole: the tenancy proof decides the context alone, the parse census survives it, and `Propagation.ingress` spends
// that census once at the settle seam.
const _carried = (
  envelope: CloudEvent<unknown>,
  scope: Identity.Tenant,
): Effect.Effect<Carrier.Extraction, Problem> =>
  pipe(Journal.carrier(envelope, scope), ({ context, dropped }) =>
    Option.match(context, {
      onNone: () => Effect.fail(Problem.of({ class: "denied", message: "<tenant-claim-mismatch>" })),
      onSome: (proved): Effect.Effect<Carrier.Extraction, Problem> => Effect.succeed({ context: proved, dropped }),
    }))

// Admission has already crossed the strict envelope boundary; the Rasm profile now projects its addressed record
// and generated extension roster without another transport-specific interpretation.
const _admitted = (
  envelope: CloudEvent<unknown>,
  scope: Identity.Tenant,
  principal: Principal.Shape,
  trust: Inbound.Spec["trust"],
  classes: Inbound.Spec["classes"],
): Effect.Effect<Inbound.Admitted, Problem, Dataref> =>
  Effect.flatMap(Effect.mapError(Event.rasm.read(envelope), _eventRefused), ({ roster, dropped }) =>
    Effect.flatMap(_resolved(envelope, roster), (resolved) => Effect.all({
      carrier: _carried(resolved, scope),
      fact: Effect.mapError(Schema.decodeUnknown(Event.rasm.Fact)(resolved, { errors: "all" }), (issue) =>
        _eventProblem(issue.message)),
    }).pipe(
      Effect.flatMap(({ carrier, fact }) =>
        Option.match(Option.fromNullable(roster.dataclassification), {
          onNone: () => Effect.fail(Problem.of({ class: "denied", message: "<dataclassification-required:webhook>" })),
          onSome: (classification) => Array.contains(classes, classification)
            ? Effect.as(trust(principal, fact), { envelope: resolved, fact, roster, dropped, carrier })
            : Effect.fail(Problem.of({ class: "denied", message: `<dataclassification-refused:${classification}>` })),
        })),
    )))

const _members = (
  headers: Message["headers"],
  body: Uint8Array,
  scope: Identity.Tenant,
  principal: Principal.Shape,
  trust: Inbound.Spec["trust"],
  classes: Inbound.Spec["classes"],
): Effect.Effect<ReadonlyArray<Inbound.Admitted>, Problem, Dataref> =>
  Effect.flatMap(
    Effect.flatMap(_framing(headers), (frame) => _decoded(headers, body, frame)),
    (decoded) => Effect.forEach(decoded, (envelope) => _admitted(envelope, scope, principal, trust, classes)),
  )

const _settled = (
  admitted: Inbound.Admitted,
  verified: Verified,
  settle: Inbound.Spec["settle"],
): Effect.Effect<Inbound.Settlement, Problem, _Idempotency> =>
  Effect.gen(function* () {
    const address = yield* Event.address(admitted.envelope)
    const key = yield* Schema.decode(Gate.IdempotencyKey)(String(address)).pipe(
      Effect.mapError((issue) => _eventProblem(issue.message)),
    )
    const idempotency = yield* Gate.Idempotency
    const outcome = yield* idempotency.run(
      key,
      String(address),
      Schema.Void,
      Propagation.ingress(settle(admitted, verified), admitted.carrier),
    ).pipe(Effect.mapError((fault) => Problem.of(fault)))
    return new _Settlement({
      source: admitted.envelope.source,
      id: admitted.envelope.id,
      disposition: outcome.disposition === "fresh" ? "accepted" : "duplicate",
    })
  })

// Every delivery request rides its claimed origin, so this read gates the POST as well as the handshake and a target
// that answered one validation exchange never inherits a trust the next message failed to re-present.
const _originDenied = (): Problem => Problem.of({ class: "denied", message: "<unallowed-webhook-origin>" })
const _origin = (
  headers: Message["headers"],
  spec: Inbound.Spec,
): Effect.Effect<typeof WebhookOrigin.Type, Problem> =>
  Option.match(_header(headers, _WEBHOOK.origin), {
    onNone: () => Effect.fail(_originDenied()),
    onSome: (claimed) => Schema.decode(WebhookOrigin)(claimed).pipe(
      Effect.mapError(_originDenied),
      Effect.flatMap((admitted) =>
        Array.some(spec.origins, (allowed) => allowed.toLowerCase() === admitted.toLowerCase())
          ? Effect.succeed(admitted)
          : Effect.fail(_originDenied())),
    ),
  })

const _inbound = (
  identity: Identity.App,
  spec: Inbound.Spec,
  policy: Seam.Policy,
): Layer.Layer<never, never, Claim | Jwt | Verify | Dataref | _Idempotency | InboundHeaders | HttpLayerRouter.HttpRouter> =>
  Layer.merge(
    // This handshake answers on the SAME path a delivery posts to. A handled but denied validation returns no grant
    // headers; status is not consent, and 405 belongs only to targets that do not handle OPTIONS at all.
    _routed("OPTIONS", spec.route, () =>
      Effect.match(Effect.flatMap(_sanitized, (headers) =>
        _requestRate(headers)
          ? _origin(headers, spec)
          : Effect.fail(_eventProblem("<invalid-webhook-request-rate>"))), {
        onFailure: () => HttpServerResponse.setHeader("allow", "POST, OPTIONS")(
          HttpServerResponse.empty({ status: 204 }),
        ),
        onSuccess: (claimed) =>
          HttpServerResponse.setHeaders({
            allow: "POST, OPTIONS",
            [_WEBHOOK.allowedOrigin]: claimed,
            [_WEBHOOK.allowedRate]: _granted(policy, spec.route),
          })(HttpServerResponse.empty({ status: 200 })),
      })),
    _routed("POST", spec.route, () =>
      Effect.gen(function* () {
        const request = yield* HttpServerRequest.HttpServerRequest
        const headers = yield* _sanitized
        const landed = yield* Effect.orDie(HttpServerRequest.toURL(request))
        const credential = yield* Gate.Authn.webhook(identity, headers, landed)
        const settled = Effect.gen(function* () {
          yield* _origin(headers, spec)
          const held = yield* _octets(request, spec.ceiling)
          yield* _delivery(headers, held)
          const verify = yield* Verify
          const presented = _header(headers, spec.header)
          const verified = yield* verify.verify(spec.dialect, held, presented, spec.mac, spec.tolerance)
          const members = yield* _members(
            headers,
            held,
            credential.admitted.scope,
            credential.admitted.principal,
            spec.trust,
            spec.classes,
          )
          // Each member enters the one address bracket before settling under its OWN creation-time parent: a batch
          // carries as many producer identities and traces as it has members, and every replay remains readable.
          const outcomes = yield* Effect.forEach(
            members,
            (admitted) => _settled(admitted, verified, spec.settle),
          )
          const accepted = yield* HttpServerResponse.schemaJson(Schema.Array(_Settlement))(outcomes, { status: 202 }).pipe(Effect.orDie)
          return credential.carriage === "query"
            ? HttpServerResponse.setHeader("cache-control", "private")(accepted)
            : accepted
        }).pipe(
          Effect.provideService(Current.Admitted, Option.some(credential.admitted)),
          (effect) => TenantScope.bind(credential.admitted.scope, effect),
          TenantScope.metered,
        )
        return yield* settled
      })),
  )

const Inbound = {
  of: _inbound,
  headers: _WEBHOOK,
  Origin: WebhookOrigin,
  Settlement: _Settlement,
} as const
```

## [04]-[CEREMONY_ROWS]

[CEREMONY_ROWS]:
- Owner: `Ceremony` — one `Context.Tag` carrying the application-owned identity projection for raw ceremony routes and the non-OIDC OAuth subject resolver, and the HTTP lift of the security wave's authentication round-trips under the fixed `/auth` cookie path: `authorize` redirects to `OAuth.authorize`'s minted URL (302, the state stash already held); `callback` hands the landed authorization-response URL whole to `OAuth.callback`, which exchanges it into a `TokenPair` the handler lands as cookies; `enroll`/`assert` each serve an `options` POST returning the RP-minted challenge JSON and a finish POST verifying through `WebAuthn.enrollFinish`/`assertFinish`; `refresh` rotates through `Token.refresh` reading the path-scoped refresh cookie under its `CookieSpec` name; `logout` revokes the authenticated session before writing the clearing set, and answers the upstream end-session redirect off `OAuth.logout` where the subject signed in through an issuer publishing one.
- Law: every mutating ceremony passes the CSRF gate BEFORE any state changes — the `_csrfed` fold reads the `CookieSpec.csrf` pair and runs `Cookie.verify`'s constant-time double-submit compare, so the webauthn finish pair, `refresh`, and `logout` are unreachable from ambient cookies alone; the oauth `callback` is exempt because its `state` round-trip is that flow's own anti-forgery evidence.
- Law: BOTH halves of the double-submit pair read one `CookieSpec.csrf` row — `name` for the cookie, `header` for the echo — so this gate and `browser/route#SESSION_PLANE`'s stamp cannot spell different fields; a route literal here, or the cookie name reused as the header name there, forks the pair into a mismatch that fails closed on every mutation with no type breaking.
- Law: cookie application is one fold — `_cookied(response, framed)` reduces the security wave's `Cookies.Cookie` set through `HttpServerResponse.setCookie(name, value, options)`, so the security attribute policy table decides every attribute and no route names `httpOnly`, `sameSite`, or a path.
- Law: ceremonies own HTTP shape only — redirect codes, query decode, body admission, cookie reads, and status; establishing, rotating, verifying, and framing are the security wave's (`OAuth`, `WebAuthn`, `Token`, `Cookie`), while `Ceremony.identity` projects the authenticated `Principal` from the application's chosen raw-route credential lift and `Ceremony.resolveSubject` handles only providers without OIDC subject evidence. Each handler is a decode, one security call, and one egress fold, and a security fault renders itself through the seam's net at its own class status. This `:provider` segment admits through the security vocabulary itself — `_Provider` decodes the param record against `Departed.fields.kind`, so `OAuth.authorize`/`callback` receive a proven `Provider.Kind` and an unrostered provider dies at the seam as a decode refusal, never inside the ceremony.
- Law: the oauth callback carries TWO channels onto one `_landOAuth` fold — the GET redirect reads the response off the query and the POST arm reads a `response_mode=form_post` body once into the URL search (Apple with a requested scope), so a form-post provider yields the same verified subject and cookie landing as a query provider and the exchange never re-reads a spent body; both stay CSRF-exempt because the single-use `state` round-trip is that flow's own anti-forgery evidence.
- Law: the passkey finish bodies admit through one Schema pair mirroring the verified `@simplewebauthn/server` wire shapes — `_Enroll` decodes the POSTed registration response (`id`, `rawId`, the attestation `response` block, optional attachment, extension outputs, `type: "public-key"`) into the `RegistrationResponseJSON` parameter `WebAuthn.enrollFinish` takes, `_Assert` the assertion twin for `assertFinish` — raw JSON crosses the decode seam exactly once and the browser collection half stays the ui wave's.
- Growth: a new ceremony (an OTP pair, a device-code flow) is one route pair under `_AUTH` composing its security owner; a new response-mode provider is one more channel onto `_landOAuth`; a new cookie role reframes through the same fold with zero route edits.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpServerRequest.schemaBodyUrlParams`/`toURL`, `HttpServerResponse`, `Cookies`); `@rasm/ts/security` (`OAuth`, `WebAuthn`, `Token`, `Cookie`, `CookieSpec`, `Departed` — the provider-kind decode anchor); `effect` (`Context`, `Schema`, `Option`, `Redacted`).

```typescript signature
const _csrfed: Effect.Effect<void, Problem, Cookie | HttpServerRequest.HttpServerRequest> = Effect.gen(function* () {
  const request = yield* HttpServerRequest.HttpServerRequest
  const cookie = yield* Cookie
  // both reads come off ONE CookieSpec row: the cookie under `name`, the echo under `header` — the browser dial
  // stamps that same `header`, so the pair cannot fork into the silent fail-closed mismatch a literal here mints
  yield* cookie.verify(
    Option.fromNullable(request.cookies[CookieSpec.csrf.name]),
    Option.fromNullable(request.headers[CookieSpec.csrf.header]),
  ).pipe(Effect.mapError((fault) => Problem.of(fault)))
})

const _cookied = (
  response: HttpServerResponse.HttpServerResponse,
  framed: ReadonlyArray<Cookies.Cookie>,
): Effect.Effect<HttpServerResponse.HttpServerResponse> =>
  Effect.reduce(framed, response, (held, cookie) =>
    HttpServerResponse.setCookie(held, cookie.name, cookie.value, cookie.options).pipe(Effect.orDie))

const _Provider = Schema.Struct({ provider: Departed.fields.kind })

const _Base64Url = Schema.NonEmptyString.pipe(Schema.pattern(/^[A-Za-z0-9_-]+$/))

const _Extensions = Schema.Struct({
  appid: Schema.optionalWith(Schema.Boolean, { exact: true }),
  credProps: Schema.optionalWith(Schema.Struct({ rk: Schema.optionalWith(Schema.Boolean, { exact: true }) }), { exact: true }),
  hmacCreateSecret: Schema.optionalWith(Schema.Boolean, { exact: true }),
})

const _Transports = Schema.mutable(Schema.Array(Schema.Literal("ble", "cable", "hybrid", "internal", "nfc", "smart-card", "usb")))

const _Enroll = Schema.Struct({
  id: _Base64Url,
  rawId: _Base64Url,
  response: Schema.Struct({
    clientDataJSON: _Base64Url,
    attestationObject: _Base64Url,
    authenticatorData: Schema.optionalWith(_Base64Url, { exact: true }),
    transports: Schema.optionalWith(_Transports, { exact: true }),
    publicKeyAlgorithm: Schema.optionalWith(Schema.Int, { exact: true }),
    publicKey: Schema.optionalWith(_Base64Url, { exact: true }),
  }),
  authenticatorAttachment: Schema.optionalWith(Schema.Literal("cross-platform", "platform"), { exact: true }),
  clientExtensionResults: _Extensions,
  type: Schema.Literal("public-key"),
})

const _Assert = Schema.Struct({
  id: _Base64Url,
  rawId: _Base64Url,
  response: Schema.Struct({
    clientDataJSON: _Base64Url,
    authenticatorData: _Base64Url,
    signature: _Base64Url,
    userHandle: Schema.optionalWith(_Base64Url, { exact: true }),
  }),
  authenticatorAttachment: Schema.optionalWith(Schema.Literal("cross-platform", "platform"), { exact: true }),
  clientExtensionResults: _Extensions,
  type: Schema.Literal("public-key"),
})

const _EnrollOptions = Schema.Struct({ userName: Schema.NonEmptyString })

class Ceremony extends Context.Tag("runtime/serve/Ceremony")<Ceremony, {
  readonly identity: Effect.Effect<Principal.Shape, GateFault, HttpServerRequest.HttpServerRequest>
  readonly resolveSubject: Parameters<OAuth["callback"]>[2]
}>() {
  static readonly of = () => _ceremony()
}

const _principal = Effect.flatMap(Ceremony, (ceremony) => ceremony.identity).pipe(Effect.mapError(Problem.of))

const _subject = Effect.map(_principal, (principal) => principal.subject)

const _cleared: Effect.Effect<
  ReadonlyArray<Cookies.Cookie>,
  Problem,
  Ceremony | Token | Cookie | HttpServerRequest.HttpServerRequest
> =
  Effect.gen(function* () {
    const principal = yield* _principal
    const token = yield* Token
    const cookie = yield* Cookie
    yield* Option.match(principal.session, {
      onNone: () => Effect.void,
      onSome: (session) => token.revoke(session).pipe(Effect.mapError(Problem.of)),
    })
    return yield* cookie.clear()
  })

const _AUTH = "/auth"

// Apple with a requested scope lands its authorization response in the urlencoded BODY, not the query, so this
// reads every field once into a record the URL search then carries. Reading the body once and rebuilding the URL
// keeps the one `OAuth.callback` reading `state`, `code`, and `id_token` from the same place both channels present
// them, and never a body the exchange leg then re-reads empty.
const _FormPost = Schema.Record({ key: Schema.String, value: Schema.String })

// One landing fold both callback channels share: the query arm and the form-post arm each build the response URL
// their own way, then hand it to the ONE `OAuth.callback` that consumes the snapshot, exchanges, verifies, and
// establishes — so the redirect, the framing, and the CSRF mint exist once rather than per channel.
const _landOAuth = (provider: Parameters<OAuth["callback"]>[0], landed: URL) =>
  Effect.gen(function* () {
    const oauth = yield* OAuth
    const cookie = yield* Cookie
    const ceremony = yield* Ceremony
    const pair = yield* oauth.callback(provider, landed, ceremony.resolveSubject)
    const framed = yield* cookie.frame(pair)
    const csrf = yield* cookie.csrf()
    return yield* _cookied(HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", "/")), [...framed, csrf])
  })

const _ceremony = () =>
  Layer.mergeAll(
    _routed("GET", `${_AUTH}/authorize/:provider`, () =>
      Effect.gen(function* () {
        const oauth = yield* OAuth
        const { provider } = yield* HttpLayerRouter.schemaPathParams(_Provider)
        const target = yield* oauth.authorize(provider)
        return HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", target.href))
      })),
    _routed("GET", `${_AUTH}/callback/:provider`, () =>
      Effect.gen(function* () {
        // Hand the WHOLE authorization response across, never a pair of picked params: the ceremony validates
        // an RFC 9207 `iss` wherever the issuer advertises one, reads the declined-consent `error`, and looks
        // its single-use snapshot up by `state` — then rebases that search onto its own registered redirect, so
        // no spoofable `Host` reaches the token request's `redirect_uri`.
        const landed = yield* Effect.orDie(HttpServerRequest.toURL(yield* HttpServerRequest.HttpServerRequest))
        const { provider } = yield* HttpLayerRouter.schemaPathParams(_Provider)
        return yield* _landOAuth(provider, landed)
      })),
    _routed("POST", `${_AUTH}/callback/:provider`, () =>
      Effect.gen(function* () {
        // form_post is the oauth callback's second channel and stays CSRF-exempt exactly as the GET arm does — its
        // single-use `state` round-trip is the anti-forgery evidence, so no double-submit precedes it. The body
        // reads once; its fields become the URL search the same `_landOAuth` rebases onto the registered redirect.
        const request = yield* HttpServerRequest.HttpServerRequest
        const fields = yield* HttpServerRequest.schemaBodyUrlParams(_FormPost).pipe(Effect.mapError((fault) => Problem.of(fault)))
        const { provider } = yield* HttpLayerRouter.schemaPathParams(_Provider)
        const landed = Option.getOrElse(HttpServerRequest.toURL(request), () => new URL(`https://form-post.invalid${_AUTH}/callback/${provider}`))
        landed.search = new URLSearchParams(fields).toString()
        return yield* _landOAuth(provider, landed)
      })),
    _routed("POST", `${_AUTH}/webauthn/enroll/options`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const webauthn = yield* WebAuthn
        const subject = yield* _subject
        const request = yield* HttpServerRequest.schemaBodyJson(_EnrollOptions).pipe(Effect.mapError((fault) => Problem.of(fault)))
        const options = yield* webauthn.enrollStart(subject, request.userName)
        return yield* HttpServerResponse.json(options).pipe(Effect.orDie)
      })),
    _routed("POST", `${_AUTH}/webauthn/enroll`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const webauthn = yield* WebAuthn
        const cookie = yield* Cookie
        const subject = yield* _subject
        const response = yield* HttpServerRequest.schemaBodyJson(_Enroll).pipe(Effect.mapError((fault) => Problem.of(fault)))
        const pair = yield* webauthn.enrollFinish(subject, response)
        const framed = yield* cookie.frame(pair)
        return yield* _cookied(HttpServerResponse.empty({ status: 204 }), framed)
      })),
    _routed("POST", `${_AUTH}/webauthn/assert/options`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const webauthn = yield* WebAuthn
        const subject = yield* _subject
        const options = yield* webauthn.assertStart(subject)
        return yield* HttpServerResponse.json(options).pipe(Effect.orDie)
      })),
    _routed("POST", `${_AUTH}/webauthn/assert`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const webauthn = yield* WebAuthn
        const cookie = yield* Cookie
        const subject = yield* _subject
        const response = yield* HttpServerRequest.schemaBodyJson(_Assert).pipe(Effect.mapError((fault) => Problem.of(fault)))
        const pair = yield* webauthn.assertFinish(subject, response)
        const framed = yield* cookie.frame(pair)
        return yield* _cookied(HttpServerResponse.empty({ status: 204 }), framed)
      })),
    _routed("POST", `${_AUTH}/logout`, () =>
      Effect.zipRight(_csrfed, Effect.flatMap(_cleared, (framed) => _cookied(HttpServerResponse.empty({ status: 204 }), framed)))),
    _routed("POST", `${_AUTH}/refresh`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const request = yield* HttpServerRequest.HttpServerRequest
        const cookie = yield* Cookie
        const token = yield* Token
        const presented = Option.fromNullable(request.cookies[CookieSpec.refresh.name])
        const pair = yield* token.refresh(Redacted.make(Option.getOrElse(presented, () => "")))
        const framed = yield* cookie.frame(pair)
        return yield* _cookied(HttpServerResponse.empty({ status: 204 }), framed)
      })),
  )

```

## [05]-[ASSET_ROWS]

[ASSET_ROWS]:
- Owner: `Router.assets` — the SPA/static row as one request fold: resolve the request path under the asset root through the `Path` capability, serve the file when it exists, fall back to the SPA entry for every path-shaped miss (client-rendered routes hydrate from one entry), and stamp the cache row the fingerprint predicate selects.
- Law: `_cached` selects by ADDRESS first, filename second — a leaf resolving under the content-addressed `assets/` prefix is immutable by its address (the digest is a DIRECTORY segment and its leaves are plain names a fingerprint pattern never matches), a `name-<hash>.ext` bundle leaf is immutable by its filename, and everything else including the entry document is `no-cache` because it is the mutable pointer INTO immutable content — one selector, total over every asset, ordered so the addressed tree can never fall to the pointer row.
- Law: the immutable value is TRANSCRIBED from `iac/program/source.md` `_CACHE_POSTURE`, the estate's one served-header roster — the unfronted origin and the fronting edge must answer identically on the same address, and no import crosses iac and runtime to enforce it, so the two ends carry one value and a divergence is a two-ended edit.
- Law: the row SELECTS `Etag.layerWeak` and mounts it itself — both engine server Layers export that generator while the `HttpPlatform` merged beside them mints from `Etag.layer`, and both fold the identical `<size>-<mtime>` pair, so validator STRENGTH is their whole difference: size and modification time carry no byte-identity evidence, and byte identity is the claim a strong validator makes. Mounting the weak row here drops `Etag.Generator` from the requirement channel, so strength becomes this row's decision rather than an app root's, and the served validator, the `304` hit, and the `_matched` compare read one mint.
- Law: revalidation reads the header RFC 9110 defines — `_matched` splits `if-none-match` on commas, trims, admits `*`, and compares with the weak prefix stripped from both sides, so a browser revalidating a weak validator and a client holding two variants both answer `304`; both the hit and the body response carry the same `etag` and cache row.
- Law: traversal is structurally refused — the request path resolves under the root and the fold asserts the resolved target still carries the root as its prefix, so an encoded, normalized, or absolute escape lands outside the prefix and serves the SPA entry, never a distinct error that maps the filesystem for a probe.
- Boundary: what the assets ARE (app shell, prerender output, the self-hosted wasm bundles the ui wave serves beside the shell) is the ui wave's; this row owns only serving them byte-identical.
- Packages: `@effect/platform` (`FileSystem`, `Path`, `HttpPlatform`, `HttpServerRequest`, `HttpServerResponse`, `Etag`).

```typescript signature
// transcribed from iac `program/source.md` `_CACHE_POSTURE`: the estate answers one Cache-Control on this
// address whether an edge fronts the origin or not
const _CACHE = {
  immutable: "public, max-age=31536000, immutable",
  entry: "no-cache",
} as const

const _ADDRESSED = "assets/"
const _FINGERPRINT = /-[0-9a-f]{8,}\.[a-z0-9]+$/
const _WEAK = /^W\//

// address before filename: `assets/<digest>/basis_transcoder.js` carries no fingerprint in its leaf, so a
// filename-only predicate stamps no-cache on exactly the tree the edge stamps immutable
const _cached = (relative: string): string =>
  relative.startsWith(_ADDRESSED) || _FINGERPRINT.test(relative) ? _CACHE.immutable : _CACHE.entry

const _matched = (presented: string | undefined, tag: string): boolean =>
  Option.match(Option.fromNullable(presented), {
    onNone: () => false,
    onSome: (held) =>
      Array.some(
        held.split(","),
        (entry) => pipe(entry.trim(), (candidate) =>
          candidate === "*" || candidate.replace(_WEAK, "") === tag.replace(_WEAK, "")),
      ),
  })

const _assets = (options: { readonly root: string; readonly entry: string }): Effect.Effect<
  HttpServerResponse.HttpServerResponse,
  never,
  FileSystem.FileSystem | HttpPlatform.HttpPlatform | Path.Path | HttpServerRequest.HttpServerRequest
> =>
  Effect.gen(function* () {
    const request = yield* HttpServerRequest.HttpServerRequest
    const path = yield* Path.Path
    const fs = yield* FileSystem.FileSystem
    const clean = request.url.split("?")[0] ?? "/"
    const anchor = path.resolve(options.root)
    const resolved = path.resolve(options.root, clean.replace(/^\/+/, ""))
    const target = resolved === anchor || resolved.startsWith(`${anchor}/`) ? resolved : path.resolve(anchor, options.entry)
    const held = yield* fs.exists(target).pipe(Effect.orElseSucceed(() => false))
    const chosen = held && clean !== "/" ? target : path.join(options.root, options.entry)
    // This selector reads the path RELATIVE to the anchor, because `assets/` is an address, not a disk prefix
    const cache = _cached(path.relative(anchor, chosen))
    const generator = yield* Etag.Generator
    // Stat runs first because a `304` answers without opening the file at all; `HttpServerResponse.file` then
    // stamps its OWN `etag` from the platform's internal strong mint, and this `setHeaders` replaces it, so hit
    // and body carry the one validator `_matched` compares
    const served = (file: string, policy: string) =>
      Effect.gen(function* () {
        const info = yield* fs.stat(file)
        const tag = Etag.toString(yield* generator.fromFileInfo(info))
        return _matched(request.headers["if-none-match"], tag)
          ? HttpServerResponse.empty({ status: 304 }).pipe(HttpServerResponse.setHeaders({ "cache-control": policy, etag: tag }))
          : yield* HttpServerResponse.file(file).pipe(
              Effect.map(HttpServerResponse.setHeaders({ "cache-control": policy, etag: tag })),
            )
      })
    return yield* served(chosen, cache).pipe(
      Effect.orElse(() => served(path.join(options.root, options.entry), _CACHE.entry)),
      Effect.orDie,
    )
  }).pipe(Effect.provide(Etag.layerWeak))
```

## [06]-[SERVE_FOLD]

[SERVE_FOLD]:
- Owner: the serve law — the app root merges its route Layers (`Router.api`, `Router.health`, ceremonies, intake rows, rail mounts, the asset route, `Router.mounts`), attaches `Seam.guard(policy)` and `Seam.admission(identity)` through `HttpLayerRouter.middleware` once as global rows, merges `Seam.Priced.live(policy)` beside them for the api mount's quota seat, and launches `HttpLayerRouter.serve` — a Layer whose `HttpServer` requirement the boot module satisfies from `proc/exec#RUNTIME_ROWS`'s `serve` member, so node-versus-bun is a row selection, the listener residency is that row's `Runtime.Bind` case, and this module names no binding; the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same merged Layers, which demands no `HttpServer` and therefore selects no runtime row at all — the residency that reaches an edge host owning no listener socket — and a process whose whole life is the server parks through `proc/life#PHASE_SPINE`'s boot law.
- Law: the two global rows compose in one order and only one — `Seam.guard` outermost so its net renders a `RouteNotFound` no matched route ever sees, `Seam.admission` beneath it so the credential lift and its tenancy binding wrap every dispatch; the route-attribution row is per-route by construction and rides `Seam.routed` and `Seam.Routed` instead.
- Law: pricing is no third global row — it reads a coordinate a global row runs too early to see, so it rides `Seam.priced` and `Seam.Priced` exactly as attribution does, and the root's whole remaining quota obligation is the PORT: `Fleet.RateLimiter` binds once (`layerStoreMemory` on a single node, a shared store-backed Layer across a fleet), because every route Layer carries that Tag whether its own pattern is priced or not.
- Law: the guard row declares `handles: GateFault` because the admission row RAISES one — a credential store the lift cannot reach refuses `shed` rather than reading as an anonymous request, and `HttpLayerRouter.middleware` carries that refusal outward as a `GlobalError` requirement only a declaring row discharges; the net's total cause fold already answers it, so the one door renders the 503 and no middleware grows a recovery arm of its own.
- Law: multiplex rows dispatch across whole apps — `Router.hosts` takes an ORDERED roster of `Router.HostRow` values, each an already-applied platform predicate across the two axes and four match forms the package publishes (`hostExact`/`hostRegex`/`hostStartsWith`/`hostEndsWith` and the `header*` quartet beside them), and lands the multiplex as the catch-all route Layer so dispatch reaches the one front door; a predicate is a row, dispatch is the platform's, and both a hand-rolled host `if` chain and a fixed two-key app record that no third origin can join are the deleted spellings.
- Law: a host row `fits` an origin whose whole app differs and `admit`s on its own predicate against the request's post-forwarding host or header; `lifetime` is the request, ended by the matched app. It DECIDES no `tenancy` — the credential lift at `Seam.admission` is the branch's one tenancy binder, so a subdomain row selects an app and never a tenant, and reading identity off the hostname here forks that owner. Its `degrade` is ordering: the multiplex answers the FIRST matching row, so two overlapping predicates resolve by declaration order and no row learns it was shadowed.
- Law: readiness gates intake — the serving edge stops accepting by `life.phase`: the drain fold flips the phase before finalizers run, the ready report fails by fold, and the load balancer routes away while in-flight requests finish under the drain bands; no connection-draining code exists here because the phase spine and the runtime row already own the choreography.
- Boundary: the `node:http`/`Bun.serve` construction is `proc/exec#RUNTIME_ROWS`'s row interior; TLS and unix-socket residency are `Runtime.Bind` cases the selected row's own `residency` column admits or refuses, so this fold reads that column and never assumes a port; `iac` mirrors the drain budget and the probe paths from their owners, never from here.
- Growth: a new virtual host is one multiplex row; a new engine is one runtime-row edit with zero serve-fold changes.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpMultiplex`); `@effect/experimental` (`RateLimiter` — the store Tag the root binds); `effect` (`Layer`).

```typescript signature
declare namespace Router {
  // One value carries the whole served-RPC selection: which front-door path the group answers on, which of the two
  // router-native transports carries it, and the fan-out ceiling. `protocol` is data because BOTH transports mount
  // through this one member and the package defaults it to websockets, so a caller elects its transport rather than
  // inheriting one; `concurrency` is stated because the package's own default is unbounded, and an unbounded RPC
  // fan-out is the one ceiling no route Layer above this mount can re-impose.
  type RpcMount = {
    readonly path: `/${string}`
    readonly protocol: "http" | "websocket"
    readonly concurrency: number
  }
  // one type covers every predicate the multiplex publishes, because each is already a `self => self` transform
  // once its match and app are applied: `HttpMultiplex.hostRegex(/^api\./, app)` and
  // `HttpMultiplex.headerExact("x-canary", "1", app)` inhabit it identically
  type HostRow<E, R> = (self: HttpMultiplex.HttpMultiplex<E, R>) => HttpMultiplex.HttpMultiplex<E, R>
}

type _Rail = Effect.Effect.Success<ReturnType<typeof Rail.of>>

class _RailMount extends Context.Tag("runtime/serve/Router/RailMount")<_RailMount, {
  readonly layer: (spec: Rail.Spec, rail: _Rail) => Layer.Layer<never, never, HttpLayerRouter.HttpRouter | Life>
}>() {}

const _rail = (spec: Rail.Spec) =>
  Layer.unwrapEffect(
    Effect.gen(function* () {
      const mount = yield* _RailMount
      const rail = yield* Rail.of(spec)
      return mount.layer(spec, rail)
    }),
  )

// Rows ARE the platform's own partially-applied predicates, so the whole vocabulary crosses verbatim — host axis
// and header axis, each under exact, regex, prefix, and suffix matching — and this page mints no wrapper over any
// of it. Rows apply in declaration order, the multiplex answers the FIRST match, and an unmatched request leaves
// `RouteNotFound` for the seam's net to render `absent`, so a catch-all is a DECLARED last row rather than a
// silent default some caller inherited.
const _hosts = <E, R>(rows: Array.NonEmptyReadonlyArray<Router.HostRow<E, R>>) =>
  // This catch-all mount lands the multiplex app in the served Layer set; `HttpLayerRouter.serve`
  // and `toWebHandler` both take Layers, so a bare HttpApp value reaches no front door
  _routed("*", "/*", () => Array.reduce(rows, HttpMultiplex.make<E, R>([]), (held, row) => row(held)))

const Router = {
  api: <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
    api: HttpApi.HttpApi<Id, Groups, E, R>,
    docs: { readonly openapiPath: `/${string}`; readonly path: `/${string}`; readonly ui?: Emit.NativeUi },
  ) =>
    Layer.mergeAll(
      HttpLayerRouter.addHttpApi(api, { openapiPath: docs.openapiPath }),
      // That roster owns the UI choice and its path; a constructor spelled here forks the docs decision in two
      Emit.docs({ api, path: docs.path, ui: docs.ui ?? "scalarRouter" }),
      Seam.Routed.live,
    ),
  assets: _assets,
  health: _health,
  hosts: _hosts,
  mounts: _mounts,
  rail: _rail,
  RailMount: _RailMount,
  rpc: <G extends RpcGroup.RpcGroup.Any>(group: G, mount: Router.RpcMount) =>
    RpcServer.layerHttpRouter({ group, path: mount.path, protocol: mount.protocol, concurrency: mount.concurrency }),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ceremony, Inbound, Router, Seam }
```

## [07]-[RESEARCH]

(none)
