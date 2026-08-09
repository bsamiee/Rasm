# [RUNTIME_ROUTE]

This serving assembly: routes are Layers under `HttpLayerRouter` — the app-assembled `HttpApi` mounts through `addHttpApi` beside raw routes, foreign realtime protocols mount through the `Mount` port fold, the resumable-upload rail mounts its tus dispatchers, the health trio serves the probe anchor, the webhook intake holds raw octets for signature verification, and the auth ceremonies lift the security wave's redirect and passkey round-trips into HTTP — all under ONE seam: mark mint, ambient provision, trace continuation, the one credential lift and its tenancy binding, the upload ceiling, the export-derived shield headers, route attribution, and the respondable net composed once as middleware so no handler, group, or app root re-states the cross-cutting stack and the served app's error channel is `never`. Host and header dispatch across several apps is a `HttpMultiplex` catch-all route; static assets serve under an address-first immutable cache selector, list-aware revalidation, traversal refusal, and the weak `Etag.Generator` that row mounts for itself. This engine is never named here — `HttpLayerRouter.serve` demands the `HttpServer` the boot module provides from `proc/exec#RUNTIME_ROWS`, so a runtime change is a row selection at the root and the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same route Layers. This module ships on the `./server` exports subpath as `runtime/src/serve/route.ts`.

## [01]-[INDEX]

- [02]-[SEAM_ROWS]: the middleware rows: mark, ambient, trace, admission, upload bound, derived shield, route stamp, net; `Seam`.
- [03]-[LAYER_ROUTES]: api/docs/health/tus/mount route Layers, the bounded webhook intake row; `Router`, `Intake`.
- [04]-[CEREMONY_ROWS]: oauth redirect pair, webauthn enroll/assert, refresh/logout, cookie application; `Ceremony`.
- [05]-[ASSET_ROWS]: the SPA/static fold: address-first cache selector, list-aware revalidation, traversal refusal; `Router`.
- [06]-[SERVE_FOLD]: multiplex rows, the serve Layer, the web-handler twin; `Router`.

## [02]-[SEAM_ROWS]

[SEAM_ROWS]:
- Owner: `Seam` — the one cross-cutting composition over one `Seam.Policy` value: `Seam.guard(policy)(app)` mints the request mark (id, instant, negotiated locale from the `accept-language` header against the ambient fallback), provides the `Current` rows in one scoped provision, continues the W3C trace through `Current.traced` over the request headers, bounds every multipart read through the policy's `uploads` row as fiber-ref policy, folds every escaping cause through `Problem.net` — self-rendering first, total ladder as the floor — and stamps the derived shield headers on every response; the served app's error channel is `never` by construction. `Seam.admission(identity)` is its credential companion and `Seam.routed` the route-attribution row, so the whole cross-cutting stack is three `HttpLayerRouter.middleware` rows composed once at the serve fold.
- Law: the shield splits by what a header actually is — the four transport-fixed rows (`strict-transport-security`, `x-content-type-options`, `x-frame-options`, `referrer-policy`) are literals, and the CSP DERIVES from the export policy through `Seam.shield(shield)`: `default-src 'self'` and `frame-ancestors 'none'` fix the frame, `script-src` grants `'wasm-unsafe-eval'` because CSP3 demands it for the `WebAssembly.instantiate` the served decoder leaves run, `connect-src` assembles the collector origin beside the cross-origin API roster the deployment already declares, and `worker-src`/`img-src` join the asset origin wherever the multiplex row places assets on a second host. A deployment that moves its collector or adds an API origin therefore edits no header, and the standing clause — a handler hand-setting a shield header is the drift defect — becomes enforceable instead of aspirational.
- Law: the shield row derives, never restates — `Seam.shieldOf(policy, origins)` reads the collector ORIGIN off `otel/emit#POLICY`'s own `collector.baseUrl`, so the estate spells the collector once; the `propagate` roster stays anchored `RegExp` patterns for the SDK's `urlMatches` compare and cannot serve a CSP source list, so the cross-origin API origins arrive as the explicit `connect` roster the same app root assembles both from.
- Law: `Seam.admission` is the tenancy binder every sibling folder names as "the edge" — it composes `Gate.Authn.admit(identity, request.headers)` ONCE per request, provides the result through `Current.Admitted` so the api's scheme arms project rather than re-verify, and wraps the whole downstream in `TenantScope.bind` plus `TenantScope.metered` on the admitted arm; an anonymous request binds nothing and the unscoped default answers, because refusal belongs to the endpoint that declares a scheme, never to this seam.
- Law: `Seam.routed` is what makes `http.route` producible — `@opentelemetry/instrumentation-http` installs the `RPCMetadata` record under `RPCType.HTTP` and reads `route` off it at response end to build BOTH the span attribute and the duration histogram's own dimension, and no published hook fills that field: `startIncomingSpanHook` fires before any route matches, `requestHook`/`responseHook` see the node message alone, and `applyCustomAttributesOnSpan` runs PAST the metric-attribute build, so an attribute set there decorates the span while the RED plane still ships route-less. This row therefore writes the matched pattern onto the record under the same `RPCType.HTTP` discriminant the reader compares, attaching per route because `RouteContext` exists only after a match, and covering `addHttpApi`'s endpoints through the api-level `Seam.Routed` Tag.
- Law: forwarded-header trust is a policy ROW, never a default — `fronted` selects `HttpMiddleware.xForwardedHeaders`, which rewrites `host` from `x-forwarded-host` and the caller address from the first `x-forwarded-for` hop, so a proxied deployment dispatches and audits on the PUBLIC coordinates while an unfronted origin refuses the rewrite outright. Both header names stay caller-writable: a default-on row hands any caller its own virtual host, and a default-off row collapses every multiplex predicate onto the ingress hostname behind a load balancer.
- Law: CORS is delegated, never re-implemented — the assembly composes `HttpLayerRouter.cors()` (or `HttpApiBuilder.middlewareCors(options)` on the api mount) with the options row as its one policy value; no `Seam` member renames it, because a forwarding member is the one-hop wrapper the platform surface already owns.
- Growth: a new cross-cutting response concern is one line in `Seam.guard`, inherited by every route Layer at once; a new CSP directive is one `_directives` row.
- Packages: `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`, `HttpApiMiddleware`, `HttpMiddleware`, `HttpLayerRouter`, `Multipart`); `@opentelemetry/core` (`getRPCMetadata`, `RPCType`); `@opentelemetry/api` (`context`); `effect` (`DateTime`, `Effect`, `Option`, `Array`, `identity`); `@rasm/ts/security` (`TenantScope`); `./api.ts` (`Current`, `Gate`); `../otel/emit.ts` (`Export`).

```typescript
import { Buffer } from "node:buffer"
import { context } from "@opentelemetry/api"
import { getRPCMetadata, RPCType } from "@opentelemetry/core"
import { CloudEvent, CONSTANTS, HTTP, type CloudEventV1, type Message, ValidationError } from "cloudevents"
import {
  type Cookies, Etag, FileSystem, type HttpApi, type HttpApiGroup, HttpApiMiddleware, HttpLayerRouter,
  HttpMiddleware, HttpMultiplex, type HttpPlatform, HttpServerRequest, HttpServerResponse, Multipart, Path,
} from "@effect/platform"
import { type RpcGroup, RpcServer } from "@effect/rpc"
import { Array, Context, DateTime, Duration, Effect, Layer, Number, Option, Redacted, Schema, identity, pipe } from "effect"
import type { Identity } from "@rasm/ts/core"
import { Cookie, CookieSpec, Departed, type MacKey, OAuth, TenantScope, Token, type Verified, Verify, WebAuthn } from "@rasm/ts/security"
import { Rail } from "@rasm/ts/data"
import { Life } from "../proc/life.ts"
import type { Export } from "../otel/emit.ts"
import { Current, Emit, Gate, type GateFault, type Principal } from "./api.ts"
import { Mount } from "./live.ts"
import { Problem } from "./problem.ts"

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
  type Policy = {
    readonly shield: Shield
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

// the collector is spelled once, in the export policy: this projection is the only place the serving edge reads it
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

const _admission = (identity: Identity.App) =>
  HttpLayerRouter.middleware<{ provides: Current.Admitted | TenantScope }>()(
    <E, R>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>) =>
      Effect.gen(function* () {
        const request = yield* HttpServerRequest.HttpServerRequest
        const admitted = yield* Gate.Authn.admit(identity, request.headers)
        // the ONE place a verified credential first exists, so the ONE place tenancy can bind
        return yield* Option.match(admitted, {
          onNone: () => Effect.provideService(app, Current.Admitted, Option.none()),
          onSome: (held) =>
            TenantScope.metered(TenantScope.bind(
              held.scope,
              Effect.provideService(app, Current.Admitted, Option.some(held)),
            )),
        })
      }),
    { global: true },
  )

// the http instrumentation owns the record and reads `route` off it at response end to build the span attribute
// AND the duration histogram's dimension; the write guards on `RPCType.HTTP` because that is the discriminant the
// reader compares, and a stamp that throws folds here rather than widening every mounted route's error channel
const _stamp: Effect.Effect<void, never, HttpLayerRouter.RouteContext> = Effect.flatMap(
  HttpLayerRouter.RouteContext,
  (matched) =>
    Effect.try(() =>
      Option.match(
        Option.filter(Option.fromNullable(getRPCMetadata(context.active())), (held) => held.type === RPCType.HTTP),
        {
          onNone: () => undefined,
          onSome: (held) =>
            Object.assign(held, {
              route: `${Option.getOrElse(matched.route.prefix, () => "")}${matched.route.path}`,
            }),
        },
      )).pipe(Effect.ignoreLogged),
)

const _routed = <E, R>(
  method: "*" | "GET" | "POST" | "PUT" | "PATCH" | "DELETE" | "OPTIONS",
  path: HttpLayerRouter.PathInput,
  handler: (request: HttpServerRequest.HttpServerRequest) => Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>,
  options?: { readonly uninterruptible?: boolean },
) => HttpLayerRouter.add(method, path, (request) => Effect.zipRight(_stamp, handler(request)), options)

// the api arm: `addHttpApi` publishes no per-endpoint hook, and an api-level middleware runs inside the match
// where `RouteContext` carries the endpoint's own pattern
class Routed extends HttpApiMiddleware.Tag<Routed>()("runtime/serve/Routed") {
  static readonly live: Layer.Layer<Routed> = Layer.succeed(Routed, _stamp)
}

const Seam = {
  Routed,
  admission: _admission,
  guard: _guard,
  routed: _routed,
  shield: _shield,
  shieldOf: _shieldOf,
  stamp: _stamp,
} as const
```

## [03]-[LAYER_ROUTES]

[LAYER_ROUTES]:
- Owner: `Router` — the route-Layer vocabulary the app root merges: `Router.api(api, docs)` mounts the assembled `HttpApi` through `HttpLayerRouter.addHttpApi(api, { openapiPath })` and selects its reference UI through `api#EMIT`'s `_uis` roster, so the derived document and the UI ride the same router and the docs choice lives at one owner; `Router.rpc(group, prefix)` mounts a contributed RPC group beside the raw routes through the fused `RpcServer.layerHttpRouter` owner, so one router serves api, RPC, and raw rows without a second server; `Router.health` mounts the probe trio from `proc/life#PROBE_ROUTES`'s anchor — `Life.route(kind)` is the path, `Life.report(kind)` the body encoded through the `Life.Report` schema, `pass`/`warn` encode 200 and `fail` encodes 503, so the path and the verdict never exist twice; `Router.mounts` folds `Effect.serviceOption(Mount)` and mounts every provided foreign-protocol row at its prefix under the `"*"` catch-all method literal — presence-as-data, an unwired port serves nothing and never crashes.
- Law: every raw route on this page mounts through `Seam.routed`, never `HttpLayerRouter.add` directly, so the matched pattern reaches the RED plane's route dimension from the one place that knows it; a bare `add` is the drift defect this constructor forecloses. The api half is the assembly's one `HttpApi.middleware(Seam.Routed)` declaration against the Layer `Router.api` already merges, so every endpoint stamps its own pattern without a per-endpoint hook the platform does not publish.
- Law: the tus rail mounts as dispatchers, never re-frames — `Router.rail(spec)` builds the data rail (`Rail.of(spec)`) and delegates its value to `Router.RailMount`, the port whose selected runtime row routes every method under the spec's route prefix into the rail's own dispatchers and schedules `rail.groom` through the lifecycle plane; the node lift is NOT this port's, it is `live#MOUNT_PORT`'s one `Mount.node` member, so a fetch engine drives `rail.web(request)` through `HttpApp.fromWebHandler` or `BunHttpServerRequest.toRequest` while the node engine composes that one adapter, and offset semantics, staging custody, and finalize stay the data rail's while this module names no binding.
- Law: `Intake` is the held-octet webhook row — the raw body reads ONCE as bytes through the platform's own `arrayBuffer` accessor (`HttpIncomingMessage`'s byte member, lifted to `Uint8Array` at the seam) and is held, the spec's named signature header lifts from the request as `Option`, `verify.verify(dialect, octets, header, mac, tolerance)` runs the security wave's dialect fold over exactly those octets, and only a `Verified` receipt releases the enqueue through the app-declared ingress port — byte identity end to end, so re-serialization drift between verify and enqueue is unspellable; verification failure folds to the security fault's own class and the seam's net renders it.
- Law: the spec's `ceiling` gates BEFORE the read — `Intake` is by design the one route admitting unauthenticated bytes and its held-octets law forces full materialization before any verification, so the declared `content-length` is probed against the ceiling and refused as `invalid` before `arrayBuffer` allocates, and an absent or unparseable length is itself the refusal. A 413 has no core fault class and a serve-local status override is the fork `problem#STATUS_RECORD` forecloses, so the governed 422 answers; the multipart ceiling every other route inherits is `[02]`'s `uploads` policy row.
- Law: `HTTP.toEvent` detects binary, structured, and batch mode from the held message.
- Law: Intake strict-validates every decoded event and carries `ValidationError.errors` into `Problem`.
- Law: `Intake.Spec.enqueue` receives one non-empty event batch and the signature receipt.
- Boundary: which groups the api value carries is the app's assembly under `api#CONTRIBUTION`; the `Mount` Tag is `live#MOUNT_PORT`'s; the rail spec's cut policy and staging band are `data`'s.
- Growth: a new served surface is one route-Layer member composing an owning-page value; a second foreign protocol is a second `Mount` Layer at a different prefix, zero edits here.
- Packages: `@effect/platform`, `cloudevents`, `effect`, `node:buffer`, `@rasm/ts/data`, and `@rasm/ts/security`.

```typescript
const _oversize = (detail: string): Problem =>
  // 413 has no core class; the governed record answers 422 and no serve-local status override exists to fork it
  Problem.of({ class: "invalid", message: detail })

// the declared length is the ONLY bound available before the body materializes, so an absent or unparseable
// one refuses rather than admitting an unbounded read on the one route that admits unauthenticated bytes
const _octets = (
  request: HttpServerRequest.HttpServerRequest,
  ceiling: FileSystem.SizeInput,
): Effect.Effect<Uint8Array, Problem> =>
  Option.match(
    Option.filter(
      Option.flatMap(Option.fromNullable(request.headers["content-length"]), Number.parse),
      (declared) => FileSystem.Size(Math.trunc(declared)) <= FileSystem.Size(ceiling),
    ),
    {
      onNone: () => Effect.fail(_oversize("intake body exceeds the declared ceiling")),
      onSome: () =>
        Effect.mapError(
          Effect.map(request.arrayBuffer, (buffer) => new Uint8Array(buffer)),
          (fault) => Problem.of(fault),
        ),
    },
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

declare namespace Intake {
  type Events = Array.NonEmptyReadonlyArray<CloudEventV1<unknown>>
  type Spec = {
    readonly route: `/${string}`
    readonly ceiling: FileSystem.SizeInput
    readonly dialect: Verify.Dialect
    readonly header: string
    readonly mac: Option.Option<MacKey>
    readonly tolerance: Duration.Duration
    readonly enqueue: (events: Events, verified: Verified) => Effect.Effect<void, Problem>
  }
}

const _eventProblem = (caught: unknown): Problem =>
  Problem.of({
    class: "malformed",
    message: caught instanceof ValidationError
      ? JSON.stringify({ message: caught.message, errors: caught.errors ?? [] })
      : String(caught),
  })

const _eventText = new TextDecoder()

const _events = (headers: Message["headers"], body: Uint8Array): Effect.Effect<Intake.Events, Problem> =>
  Effect.flatMap(
    Effect.try({
      try: () =>
        pipe(headers[CONSTANTS.HEADER_CONTENT_TYPE], (held) => globalThis.Array.isArray(held) ? held[0] : held, (media) =>
          HTTP.toEvent<unknown>({
            headers,
            body: typeof media === "string" &&
              (media.startsWith(CONSTANTS.MIME_CE) || media.startsWith(CONSTANTS.MIME_JSON))
              ? _eventText.decode(body)
              : Buffer.from(body),
          })),
      catch: _eventProblem,
    }),
    (decoded) =>
      pipe(
        globalThis.Array.isArray(decoded) ? decoded : [decoded],
        Option.liftPredicate(Array.isNonEmptyReadonlyArray),
        Option.match({
          onNone: () => Effect.fail(_eventProblem(new ValidationError("CloudEvents batch is empty"))),
          onSome: (events) =>
            Effect.try({ try: () => Array.map(events, (event) => new CloudEvent(event)), catch: _eventProblem }),
        }),
      ),
  )

const _intake = (spec: Intake.Spec): Layer.Layer<never, never, Verify | HttpLayerRouter.HttpRouter> =>
  _routed("POST", spec.route, () =>
    Effect.gen(function* () {
      const request = yield* HttpServerRequest.HttpServerRequest
      const held = yield* _octets(request, spec.ceiling)
      const verify = yield* Verify
      const presented = Option.fromNullable(request.headers[spec.header])
      const verified = yield* verify.verify(spec.dialect, held, presented, spec.mac, spec.tolerance)
      const events = yield* _events({ ...request.headers }, held)
      yield* spec.enqueue(events, verified)
      return HttpServerResponse.empty({ status: 202 })
    }))

const Intake = { of: _intake } as const
```

## [04]-[CEREMONY_ROWS]

[CEREMONY_ROWS]:
- Owner: `Ceremony` — one `Context.Tag` carrying the application-owned identity projection for raw ceremony routes and the non-OIDC OAuth subject resolver, and the HTTP lift of the security wave's authentication round-trips under the fixed `/auth` cookie path: `authorize` redirects to `OAuth.authorize`'s minted URL (302, the state stash already held); `callback` decodes the provider's `code`/`state` query, exchanges through `OAuth.callback` into a `TokenPair`, and lands the session as cookies; `enroll`/`assert` each serve an `options` POST returning the RP-minted challenge JSON and a finish POST verifying through `WebAuthn.enrollFinish`/`assertFinish`; `refresh` rotates through `Token.refresh` reading the path-scoped refresh cookie under its `CookieSpec` name; `logout` revokes the authenticated session before writing the clearing set.
- Law: every mutating ceremony passes the CSRF gate BEFORE any state changes — the `_csrfed` fold reads the `CookieSpec.csrf` pair and runs `Cookie.verify`'s constant-time double-submit compare, so the webauthn finish pair, `refresh`, and `logout` are unreachable from ambient cookies alone; the oauth `callback` is exempt because its `state` round-trip is that flow's own anti-forgery evidence.
- Law: BOTH halves of the double-submit pair read one `CookieSpec.csrf` row — `name` for the cookie, `header` for the echo — so this gate and `browser/route#SESSION_PLANE`'s stamp cannot spell different fields; a route literal here, or the cookie name reused as the header name there, forks the pair into a mismatch that fails closed on every mutation with no type breaking.
- Law: cookie application is one fold — `_cookied(response, framed)` reduces the security wave's `Cookies.Cookie` set through `HttpServerResponse.setCookie(name, value, options)`, so the security attribute policy table decides every attribute and no route names `httpOnly`, `sameSite`, or a path.
- Law: ceremonies own HTTP shape only — redirect codes, query decode, body admission, cookie reads, and status; establishing, rotating, verifying, and framing are the security wave's (`OAuth`, `WebAuthn`, `Token`, `Cookie`), while `Ceremony.identity` projects the authenticated `Principal` from the application's chosen raw-route credential lift and `Ceremony.resolveSubject` handles only providers without OIDC subject evidence. A handler is a decode, one security call, and one egress fold, and a security fault renders itself through the seam's net at its own class status. This `:provider` segment admits through the security vocabulary itself — `_Provider` decodes the param record against `Departed.fields.kind`, so `OAuth.authorize`/`callback` receive a proven `Provider.Kind` and an unrostered provider dies at the seam as a decode refusal, never inside the ceremony.
- Law: the passkey finish bodies admit through one Schema pair mirroring the verified `@simplewebauthn/server` wire shapes — `_Enroll` decodes the POSTed registration response (`id`, `rawId`, the attestation `response` block, optional attachment, extension outputs, `type: "public-key"`) into the `RegistrationResponseJSON` parameter `WebAuthn.enrollFinish` takes, `_Assert` the assertion twin for `assertFinish` — raw JSON crosses the decode seam exactly once and the browser collection half stays the ui wave's.
- Growth: a new ceremony (an OTP pair, a device-code flow) is one route pair under `_AUTH` composing its security owner; a new cookie role reframes through the same fold with zero route edits.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpServerRequest`, `HttpServerResponse`, `Cookies`); `@rasm/ts/security` (`OAuth`, `WebAuthn`, `Token`, `Cookie`, `CookieSpec`, `Departed` — the provider-kind decode anchor); `effect` (`Context`, `Schema`, `Redacted`).

```typescript
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

const _Callback = Schema.Struct({ code: Schema.NonEmptyString, state: Schema.NonEmptyString })

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
  readonly resolveSubject: Parameters<OAuth["callback"]>[3]
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
        const oauth = yield* OAuth
        const cookie = yield* Cookie
        const query = yield* HttpServerRequest.schemaSearchParams(_Callback)
        const { provider } = yield* HttpLayerRouter.schemaPathParams(_Provider)
        const ceremony = yield* Ceremony
        const pair = yield* oauth.callback(provider, query.code, query.state, ceremony.resolveSubject)
        const framed = yield* cookie.frame(pair)
        const csrf = yield* cookie.csrf()
        return yield* _cookied(HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", "/")), [...framed, csrf])
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
- Law: revalidation reads the header RFC 9110 actually defines — `_matched` splits `if-none-match` on commas, trims, admits `*`, and compares with the weak prefix stripped from both sides, so a browser revalidating a weak validator and a client holding two variants both answer `304`; both the hit and the body response carry the same `etag` and cache row.
- Law: traversal is structurally refused — the request path resolves under the root and the fold asserts the resolved target still carries the root as its prefix, so an encoded, normalized, or absolute escape lands outside the prefix and serves the SPA entry, never a distinct error that maps the filesystem for a probe.
- Boundary: what the assets ARE (app shell, prerender output, the self-hosted wasm bundles the ui wave serves beside the shell) is the ui wave's; this row owns only serving them byte-identical.
- Packages: `@effect/platform` (`FileSystem`, `Path`, `HttpPlatform`, `HttpServerRequest`, `HttpServerResponse`, `Etag`).

```typescript
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
    // the selector reads the path RELATIVE to the anchor, because `assets/` is an address, not a disk prefix
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
- Owner: the serve law — the app root merges its route Layers (`Router.api`, `Router.health`, ceremonies, intake rows, rail mounts, the asset route, `Router.mounts`), attaches `Seam.guard(policy)` and `Seam.admission(identity)` through `HttpLayerRouter.middleware` once as global rows, and launches `HttpLayerRouter.serve` — a Layer whose `HttpServer` requirement the boot module satisfies from `proc/exec#RUNTIME_ROWS`'s `serve` member, so node-versus-bun is a row selection, the listener residency is that row's `Runtime.Bind` case, and this module names no binding; the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same merged Layers, which demands no `HttpServer` and therefore selects no runtime row at all — the residency that reaches an edge host owning no listener socket — and a process whose whole life is the server parks through `proc/life#PHASE_SPINE`'s boot law.
- Law: the two global rows compose in one order and only one — `Seam.guard` outermost so its net renders a `RouteNotFound` no matched route ever sees, `Seam.admission` beneath it so the credential lift and its tenancy binding wrap every dispatch; the route-attribution row is per-route by construction and rides `Seam.routed` and `Seam.Routed` instead.
- Law: the guard row declares `handles: GateFault` because the admission row RAISES one — a credential store the lift cannot reach refuses `shed` rather than reading as an anonymous request, and `HttpLayerRouter.middleware` carries that refusal outward as a `GlobalError` requirement only a declaring row discharges; the net's total cause fold already answers it, so the one door renders the 503 and no middleware grows a recovery arm of its own.
- Law: multiplex rows dispatch across whole apps — `Router.hosts` takes an ORDERED roster of `Router.HostRow` values, each an already-applied platform predicate across the two axes and four match forms the package publishes (`hostExact`/`hostRegex`/`hostStartsWith`/`hostEndsWith` and the `header*` quartet beside them), and lands the multiplex as the catch-all route Layer so dispatch reaches the one front door; a predicate is a row, dispatch is the platform's, and both a hand-rolled host `if` chain and a fixed two-key app record that no third origin can join are the deleted spellings.
- Law: a host row `fits` an origin whose whole app differs and `admit`s on its own predicate against the request's post-forwarding host or header; `lifetime` is the request, ended by the matched app. It DECIDES no `tenancy` — the credential lift at `Seam.admission` is the branch's one tenancy binder, so a subdomain row selects an app and never a tenant, and reading identity off the hostname here forks that owner. Its `degrade` is ordering: the multiplex answers the FIRST matching row, so two overlapping predicates resolve by declaration order and no row learns it was shadowed.
- Law: readiness gates intake — the serving edge stops accepting by `life.phase`: the drain fold flips the phase before finalizers run, the ready report fails by fold, and the load balancer routes away while in-flight requests finish under the drain bands; no connection-draining code exists here because the phase spine and the runtime row already own the choreography.
- Boundary: the `node:http`/`Bun.serve` construction is `proc/exec#RUNTIME_ROWS`'s row interior; TLS and unix-socket residency are `Runtime.Bind` cases the selected row's own `residency` column admits or refuses, so this fold reads that column and never assumes a port; `iac` mirrors the drain budget and the probe paths from their owners, never from here.
- Growth: a new virtual host is one multiplex row; a new engine is one runtime-row edit with zero serve-fold changes.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpMultiplex`); `effect` (`Layer`).

```typescript
declare namespace Router {
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
  // the catch-all mount is what lands the multiplex app in the served Layer set; `HttpLayerRouter.serve`
  // and `toWebHandler` both take Layers, so a bare HttpApp value reaches no front door
  _routed("*", "/*", () => Array.reduce(rows, HttpMultiplex.make<E, R>([]), (held, row) => row(held)))

const Router = {
  api: <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
    api: HttpApi.HttpApi<Id, Groups, E, R>,
    docs: { readonly openapiPath: `/${string}`; readonly path: `/${string}`; readonly ui?: Emit.NativeUi },
  ) =>
    Layer.mergeAll(
      HttpLayerRouter.addHttpApi(api, { openapiPath: docs.openapiPath }),
      // the roster owns the UI choice and its path; a constructor spelled here forks the docs decision in two
      Emit.docs({ api, path: docs.path, ui: docs.ui ?? "scalarRouter" }),
      Seam.Routed.live,
    ),
  assets: _assets,
  health: _health,
  hosts: _hosts,
  mounts: _mounts,
  rail: _rail,
  RailMount: _RailMount,
  rpc: <G extends RpcGroup.RpcGroup.Any>(group: G, prefix: `/${string}`) =>
    RpcServer.layerHttpRouter({ group, path: prefix, protocol: "http" }),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ceremony, Intake, Router, Seam }
```

## [07]-[RESEARCH]

(none)
