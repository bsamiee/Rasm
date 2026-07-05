# [RUNTIME_ROUTE]

The serving assembly: routes are Layers under `HttpLayerRouter` — the app-assembled `HttpApi` mounts through `addHttpApi` beside raw routes, foreign realtime protocols mount through the `Mount` port fold, the resumable-upload rail mounts its tus dispatchers, the health trio serves the probe anchor, the webhook intake holds raw octets for signature verification, and the auth ceremonies lift the security wave's redirect and passkey round-trips into HTTP — all under ONE seam: mark mint, ambient provision, trace continuation, the shield header table, and the respondable net composed once as middleware so no handler, group, or app root re-states the cross-cutting stack and the served app's error channel is `never`. Host and header dispatch across several apps is `HttpMultiplex` rows; static assets serve with fingerprint-immutable cache rows, traversal refusal, and the `Etag.Generator` the runtime server Layer already carries. The engine is never named here — `HttpLayerRouter.serve` demands the `HttpServer` the boot module provides from `exec#RUNTIME_ROWS`, so a runtime change is a row selection at the root and the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same route Layers. The module ships on the `./server` exports subpath as `runtime/src/serve/route.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                          | [PUBLIC]   |
| :-----: | :-------------- | :-------------------------------------------------------------------------------- | :--------- |
|  [01]   | `SEAM_ROWS`     | the one middleware composition: mark, ambient rows, trace, shield, respondable net | `Seam`     |
|  [02]   | `LAYER_ROUTES`  | api/docs/health/tus/mount route Layers, the webhook intake row                     | `Router`, `Intake` |
|  [03]   | `CEREMONY_ROWS` | oauth redirect pair, webauthn enroll/assert, refresh/logout, cookie application    | `Ceremony` |
|  [04]   | `ASSET_ROWS`    | the SPA/static fold: fingerprint predicate, cache-header table, traversal refusal  | `Router`   |
|  [05]   | `SERVE_FOLD`    | multiplex rows, the serve Layer, the web-handler twin                              | `Router`   |

## [2]-[SEAM_ROWS]

[SEAM_ROWS]:
- Owner: `Seam` — the one cross-cutting composition, attached exactly once through `HttpLayerRouter.middleware`: `Seam.guard(app)` mints the request mark (id, instant, negotiated locale from the `accept-language` header against the ambient fallback), provides the three `Current` rows in one scoped provision, continues the W3C trace through `Current.traced` over the request headers, folds every escaping cause through `Problem.net` — self-rendering first, total ladder as the floor — and stamps the shield table on every response; the served app's error channel is `never` by construction.
- Law: `_SHIELD` is the security-header table — `strict-transport-security`, `x-content-type-options`, `x-frame-options`, `referrer-policy`, plus the CSP row — applied to every response by this seam reading the table; a header tweak is a row edit, and a handler hand-setting a shield header is the drift defect.
- Law: CORS is delegated, never re-implemented — the assembly composes `HttpLayerRouter.cors()` (or `HttpApiBuilder.middlewareCors(options)` on the api mount) with the options row as its one policy value; no `Seam` member renames it, because a forwarding member is the one-hop wrapper the platform surface already owns.
- Growth: a new cross-cutting response concern is one line in `Seam.guard`, inherited by every route Layer at once.
- Packages: `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`DateTime`, `Effect`, `Option`); `./api.ts` (`Current`); `./problem.ts` (`Problem`).

```typescript
import {
  type Cookies, FileSystem, type HttpApi, type HttpApiGroup, HttpApiScalar, type HttpApp, HttpLayerRouter, HttpMultiplex,
  HttpServerRequest, HttpServerResponse, Path,
} from "@effect/platform"
import { type RpcGroup, RpcServer } from "@effect/rpc"
import { DateTime, Duration, Effect, Layer, Option, Redacted, Schema } from "effect"
import { Cookie, Departed, type MacKey, OAuth, Token, type Verified, Verify, WebAuthn } from "@rasm/ts/security"
import { Rail } from "@rasm/ts/data"
import { Life } from "../proc/life.ts"
import { Current } from "./api.ts"
import { Mount } from "./live.ts"
import { Problem } from "./problem.ts"

const _SHIELD = {
  "strict-transport-security": "max-age=63072000; includeSubDomains",
  "x-content-type-options": "nosniff",
  "x-frame-options": "DENY",
  "referrer-policy": "strict-origin-when-cross-origin",
  "content-security-policy": "default-src 'self'; frame-ancestors 'none'",
} as const satisfies Record<string, string>

const _guard = <E, R>(
  app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R | HttpServerRequest.HttpServerRequest>,
): Effect.Effect<HttpServerResponse.HttpServerResponse, never, R | HttpServerRequest.HttpServerRequest> =>
  Effect.gen(function* () {
    const request = yield* HttpServerRequest.HttpServerRequest
    const now = yield* DateTime.now
    const id = yield* Effect.sync(() => crypto.randomUUID())
    const fallback = yield* Current.Locale
    const mark: Current.Mark = {
      id,
      at: now,
      tenant: Option.none(),
      locale: Option.some(Current.negotiate(Option.fromNullable(request.headers["accept-language"]), fallback)),
    }
    return yield* Current.traced(app, request.headers).pipe(
      Effect.catchAllCause(Problem.net),
      (guarded) => Current.provide(guarded, mark, fallback),
      Effect.map(HttpServerResponse.setHeaders(_SHIELD)),
    )
  })

const Seam = { guard: _guard, shield: _SHIELD } as const
```

## [3]-[LAYER_ROUTES]

[LAYER_ROUTES]:
- Owner: `Router` — the route-Layer vocabulary the app root merges: `Router.api(api)` mounts the assembled `HttpApi` through `HttpLayerRouter.addHttpApi(api, { openapiPath })` with `HttpApiScalar.layerHttpLayerRouter` beside it so the derived document and the reference UI ride the same router; `Router.rpc(group, prefix)` mounts a contributed RPC group beside the raw routes through `RpcServer.toHttpApp(group)` — the `HttpApp` value form, so one router serves api, RPC, and raw rows without a second server; `Router.health` mounts the probe trio from `life#PROBE_ROUTES`'s anchor — `Life.route(kind)` is the path, `Life.report(kind)` the body encoded through the `Life.Report` schema, `pass`/`warn` encode 200 and `fail` encodes 503, so the path and the verdict never exist twice; `Router.mounts` folds `Effect.serviceOption(Mount)` and mounts the provided foreign-protocol app at its prefix — presence-as-data, an unwired port serves nothing and never crashes.
- Law: the tus rail mounts as dispatchers, never re-frames — `Router.rail(spec)` builds the data rail (`Rail.of(spec)`) and routes every method under the spec's route prefix into the rail's own dispatchers: the node engine drives `rail.node(req, res)` through the binding's raw adapters (`NodeHttpServerRequest.toIncomingMessage`/`toServerResponse`), the fetch engine drives `rail.web(request)` through `BunHttpServerRequest.toRequest` or the web-handler assembly where the request is already fetch-shaped — the raw lift is the runtime row's own adapter, composed where the boot module mounts the rail, so this module still names no binding; offset semantics, staging custody, and the finalize fold stay the data rail's, and the maintenance cadence schedules `rail.groom` beside the mount.
- Law: `Intake` is the held-octet webhook row — the raw body is read ONCE as bytes and held, the spec's named signature header lifts from the request as `Option`, `verify.verify(dialect, octets, header, mac, tolerance)` runs the security wave's dialect fold over exactly those octets, and only a `Verified` receipt releases the enqueue through the app-declared ingress port — byte identity end to end, so re-serialization drift between verify and enqueue is unspellable; verification failure folds to the security fault's own class and the seam's net renders it.
- RESEARCH: the raw-octet body member on `HttpServerRequest` (the `HttpIncomingMessage` byte accessor the `_octets` lift reads) is unverified until the branch catalogue rows its spelling; the held-octet fold and the `Verify` seam are settled, the accessor member is the research item. The catch-all method literal on `HttpLayerRouter.add` the mount and rail rows pass is the second research item — the per-method row set is the settled fallback.
- Boundary: which groups the api value carries is the app's assembly under `api#CONTRIBUTION`; the `Mount` Tag is `live#MOUNT_PORT`'s; the rail spec's cut policy and staging band are `data`'s.
- Growth: a new served surface is one route-Layer member composing an owning-page value; a second foreign protocol is a second `Mount` Layer at a different prefix, zero edits here.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpApiScalar`, `HttpServerResponse`); `@rasm/ts/data` (`Rail`); `@rasm/ts/security` (`Verify`); `../proc/life.ts` (`Life`); `./live.ts` (`Mount`).

```typescript
declare const _octets: (
  request: HttpServerRequest.HttpServerRequest,
) => Effect.Effect<Uint8Array, Problem>

const _health: Layer.Layer<never, never, Life | HttpLayerRouter.HttpRouter> = Layer.mergeAll(
  ...(["started", "ready", "live"] as const).map((kind) =>
    HttpLayerRouter.add("GET", Life.route(kind), () =>
      Effect.flatMap(Life.report(kind), (report) =>
        HttpServerResponse.json(report, { status: report.overall === "fail" ? 503 : 200 }).pipe(Effect.orDie))),
  ),
)

const _mounts: Layer.Layer<never, never, HttpLayerRouter.HttpRouter> = Layer.unwrapEffect(
  Effect.map(Effect.serviceOption(Mount), Option.match({
    onNone: () => Layer.empty,
    onSome: (mount) => HttpLayerRouter.add("ALL", `${mount.prefix}/*`, () => mount.app),
  })),
)

declare namespace Intake {
  type Spec = {
    readonly route: `/${string}`
    readonly dialect: Verify.Dialect
    readonly header: string
    readonly mac: Option.Option<MacKey>
    readonly tolerance: Duration.Duration
    readonly enqueue: (octets: Uint8Array, verified: Verified) => Effect.Effect<void, Problem>
  }
}

const _intake = (spec: Intake.Spec): Layer.Layer<never, never, Verify | HttpLayerRouter.HttpRouter> =>
  HttpLayerRouter.add("POST", spec.route, () =>
    Effect.gen(function* () {
      const request = yield* HttpServerRequest.HttpServerRequest
      const held = yield* _octets(request)
      const verify = yield* Verify
      const presented = Option.fromNullable(request.headers[spec.header])
      const verified = yield* verify.verify(spec.dialect, held, presented, spec.mac, spec.tolerance)
      yield* spec.enqueue(held, verified)
      return HttpServerResponse.empty({ status: 202 })
    }))

const Intake = { of: _intake } as const
```

## [4]-[CEREMONY_ROWS]

[CEREMONY_ROWS]:
- Owner: `Ceremony` — the HTTP lift of the security wave's authentication round-trips, four route pairs under one prefix: `authorize` redirects to `OAuth.authorize`'s minted URL (302, the state stash already held); `callback` decodes the provider's `code`/`state` query, exchanges through `OAuth.callback` into a `TokenPair`, and lands the session as cookies; `enroll`/`assert` serve the webauthn halves — the options POST returns the RP-minted challenge JSON, the finish POST verifies through `WebAuthn.enrollFinish`/`assertFinish` and lands the session; `refresh` rotates through `Token.refresh` reading the path-scoped refresh cookie; `logout` revokes and writes the clearing set.
- Law: cookie application is one fold — `_cookied(response, framed)` reduces the security wave's `Cookies.Cookie` set through `HttpServerResponse.setCookie(name, value, options)`, so the security attribute policy table decides every attribute and no route names `httpOnly`, `sameSite`, or a path; the CSRF pair verifies through `Cookie.verify` on the mutating ceremonies before any state changes.
- Law: ceremonies own HTTP shape only — redirect codes, query decode, cookie reads, and status; establishing, rotating, verifying, and framing are the security wave's (`OAuth`, `WebAuthn`, `Token`, `Cookie`), so a ceremony handler is a decode, one security call, and one egress fold, and a security fault renders itself through the seam's net at its own class status. The `:provider` segment admits through the security vocabulary itself — `_Provider` decodes the param record against `Departed.fields.kind`, so `OAuth.authorize`/`callback` receive a proven `Provider.Kind` and an unrostered provider dies at the seam as a decode refusal, never inside the ceremony.
- RESEARCH: the passkey response-body admission — one Schema pair decoding the POSTed registration and authentication response JSON into the `RegistrationResponseJSON`/`AuthenticationResponseJSON` parameters `WebAuthn.enrollFinish`/`assertFinish` take — is unverified until the security catalogue rows it; the route pair and the cookie fold are settled, the admission schema spelling is the research item and the browser collection half is the ui wave's.
- Growth: a new ceremony (an OTP pair, a device-code flow) is one route pair composing its security owner; a new cookie role reframes through the same fold with zero route edits.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpServerRequest`, `HttpServerResponse`, `Cookies`); `@rasm/ts/security` (`OAuth`, `WebAuthn`, `Token`, `Cookie`, `Departed` — the provider-kind decode anchor); `effect` (`Schema`, `Redacted`).

```typescript
const _cookied = (
  response: HttpServerResponse.HttpServerResponse,
  framed: ReadonlyArray<Cookies.Cookie>,
): Effect.Effect<HttpServerResponse.HttpServerResponse> =>
  Effect.reduce(framed, response, (held, cookie) =>
    HttpServerResponse.setCookie(held, cookie.name, cookie.value, cookie.options).pipe(Effect.orDie))

const _Callback = Schema.Struct({ code: Schema.NonEmptyString, state: Schema.NonEmptyString })

const _Provider = Schema.Struct({ provider: Departed.fields.kind })

declare const _asserted: Effect.Effect<
  { readonly subject: Parameters<WebAuthn["assertFinish"]>[0]; readonly response: Parameters<WebAuthn["assertFinish"]>[1] },
  Problem,
  HttpServerRequest.HttpServerRequest
>

declare const _enrolled: Effect.Effect<
  { readonly subject: Parameters<WebAuthn["enrollFinish"]>[0]; readonly response: Parameters<WebAuthn["enrollFinish"]>[1] },
  Problem,
  HttpServerRequest.HttpServerRequest
>

declare const _cleared: Effect.Effect<ReadonlyArray<Cookies.Cookie>, Problem, Token | Cookie>

declare const _subjectOf: Parameters<OAuth["callback"]>[3]

const _ceremony = (base: `/${string}`) =>
  Layer.mergeAll(
    HttpLayerRouter.add("GET", `${base}/authorize/:provider`, () =>
      Effect.gen(function* () {
        const oauth = yield* OAuth
        const { provider } = yield* Effect.flatMap(HttpLayerRouter.params, Schema.decodeUnknown(_Provider))
        const target = yield* oauth.authorize(provider)
        return HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", target.href))
      })),
    HttpLayerRouter.add("GET", `${base}/callback/:provider`, () =>
      Effect.gen(function* () {
        const oauth = yield* OAuth
        const cookie = yield* Cookie
        const query = yield* HttpServerRequest.schemaSearchParams(_Callback)
        const { provider } = yield* Effect.flatMap(HttpLayerRouter.params, Schema.decodeUnknown(_Provider))
        const pair = yield* oauth.callback(provider, query.code, query.state, _subjectOf)
        const framed = yield* cookie.frame(pair)
        const csrf = yield* cookie.csrf()
        return yield* _cookied(HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", "/")), [...framed, csrf])
      })),
    HttpLayerRouter.add("POST", `${base}/webauthn/enroll`, () =>
      Effect.gen(function* () {
        const webauthn = yield* WebAuthn
        const cookie = yield* Cookie
        const { subject, response } = yield* _enrolled
        const pair = yield* webauthn.enrollFinish(subject, response)
        const framed = yield* cookie.frame(pair)
        return yield* _cookied(HttpServerResponse.empty({ status: 204 }), framed)
      })),
    HttpLayerRouter.add("POST", `${base}/webauthn/assert`, () =>
      Effect.gen(function* () {
        const webauthn = yield* WebAuthn
        const cookie = yield* Cookie
        const { subject, response } = yield* _asserted
        const pair = yield* webauthn.assertFinish(subject, response)
        const framed = yield* cookie.frame(pair)
        return yield* _cookied(HttpServerResponse.empty({ status: 204 }), framed)
      })),
    HttpLayerRouter.add("POST", `${base}/logout`, () =>
      Effect.flatMap(_cleared, (framed) => _cookied(HttpServerResponse.empty({ status: 204 }), framed))),
    HttpLayerRouter.add("POST", `${base}/refresh`, () =>
      Effect.gen(function* () {
        const request = yield* HttpServerRequest.HttpServerRequest
        const cookie = yield* Cookie
        const token = yield* Token
        const presented = Option.fromNullable(request.cookies["__Secure-refresh"])
        const pair = yield* token.refresh(Redacted.make(Option.getOrElse(presented, () => "")))
        const framed = yield* cookie.frame(pair)
        return yield* _cookied(HttpServerResponse.empty({ status: 204 }), framed)
      })),
  )

const Ceremony = { of: _ceremony, cookied: _cookied } as const
```

## [5]-[ASSET_ROWS]

[ASSET_ROWS]:
- Owner: `Router.assets` — the SPA/static row as one request fold: resolve the request path under the asset root through the `Path` capability, serve the file when it exists, fall back to the SPA entry for every path-shaped miss (client-rendered routes hydrate from one entry), and stamp the cache row the fingerprint predicate selects.
- Law: `_CACHE` is the cache-header table and `_FINGERPRINT` the predicate — a content-hashed filename is immutable for a year because its identity IS its content, the entry document and every un-fingerprinted path are `no-cache` because they are the mutable pointers INTO immutable content — two rows, total over every asset, with the `Etag.Generator` the runtime server Layer already carries revalidating the mutable row for free.
- Law: traversal is structurally refused — the request path resolves under the root and the fold asserts the resolved target still carries the root as its prefix, so an encoded, normalized, or absolute escape lands outside the prefix and serves the SPA entry, never a distinct error that maps the filesystem for a probe.
- Boundary: what the assets ARE (app shell, prerender output, the self-hosted wasm bundles the ui wave serves beside the shell) is the ui wave's; this row owns only serving them byte-identical.
- Packages: `@effect/platform` (`FileSystem`, `Path`, `HttpServerRequest`, `HttpServerResponse`, `Etag`).

```typescript
const _CACHE = {
  immutable: "public, max-age=31536000, immutable",
  entry: "no-cache",
} as const

const _FINGERPRINT = /-[0-9a-f]{8,}\.[a-z0-9]+$/

const _assets = (options: { readonly root: string; readonly entry: string }): Effect.Effect<
  HttpServerResponse.HttpServerResponse,
  never,
  FileSystem.FileSystem | Path.Path | HttpServerRequest.HttpServerRequest
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
    const cache = _FINGERPRINT.test(chosen) ? _CACHE.immutable : _CACHE.entry
    return yield* HttpServerResponse.file(chosen).pipe(
      Effect.map(HttpServerResponse.setHeaders({ "cache-control": cache })),
      Effect.orElse(() =>
        HttpServerResponse.file(path.join(options.root, options.entry)).pipe(
          Effect.map(HttpServerResponse.setHeaders({ "cache-control": _CACHE.entry })),
          Effect.orDie,
        )),
    )
  })
```

## [6]-[SERVE_FOLD]

[SERVE_FOLD]:
- Owner: the serve law — the app root merges its route Layers (`Router.api`, `Router.health`, ceremonies, intake rows, rail mounts, the asset route, `Router.mounts`), attaches `Seam.guard` through `HttpLayerRouter.middleware` once, and launches `HttpLayerRouter.serve` — a Layer whose `HttpServer` requirement the boot module satisfies from `exec#RUNTIME_ROWS`'s `serve` member, so node-versus-bun is a row selection and this module names no binding; the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same merged Layers for edge runtimes, and a process whose whole life is the server parks through `life#PHASE_SPINE`'s boot law.
- Law: multiplex rows dispatch across whole apps — `HttpMultiplex.make` with `hostExact`/`hostRegex`/`headerStartsWith` predicates routes a virtual-host family (the api origin, the asset origin, a tenant subdomain family) to its own `HttpApp` before any router runs; a predicate is a row, dispatch is the platform's, and a hand-rolled host `if` chain in a handler is the deleted spelling.
- Law: readiness gates intake — the serving edge stops accepting by `life.phase`: the drain fold flips the phase before finalizers run, the ready report fails by fold, and the load balancer routes away while in-flight requests finish under the drain bands; no connection-draining code exists here because the phase spine and the runtime row already own the choreography.
- Boundary: the `node:http`/`Bun.serve` construction is `exec#RUNTIME_ROWS`'s row interior; TLS and unix-socket residency are the row's serve options; `iac` mirrors the drain budget and the probe paths from their owners, never from here.
- Growth: a new virtual host is one multiplex row; a new engine is one runtime-row edit with zero serve-fold changes.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpMultiplex`); `effect` (`Layer`).

```typescript
declare const _routes: Layer.Layer<never, never, HttpLayerRouter.HttpRouter | Life>

declare const _rail: (
  spec: Rail.Spec,
) => Layer.Layer<never, never, HttpLayerRouter.HttpRouter | Effect.Effect.Context<ReturnType<typeof Rail.of>>>

const _hosts = (apps: {
  readonly api: HttpApp.Default
  readonly assets: HttpApp.Default
}) =>
  HttpMultiplex.empty.pipe(
    HttpMultiplex.hostRegex(/^api\./, apps.api),
    HttpMultiplex.hostRegex(/./, apps.assets),
  )

const Router = {
  api: <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(api: HttpApi.HttpApi<Id, Groups, E, R>) =>
    Layer.mergeAll(HttpLayerRouter.addHttpApi(api, { openapiPath: "/openapi.json" }), HttpApiScalar.layerHttpLayerRouter({ path: "/docs" })),
  assets: _assets,
  health: _health,
  hosts: _hosts,
  mounts: _mounts,
  rail: _rail,
  rpc: <G extends RpcGroup.RpcGroup.Any>(group: G, prefix: `/${string}`) =>
    Layer.unwrapEffect(
      Effect.map(RpcServer.toHttpApp(group), (app) => HttpLayerRouter.add("POST", prefix, () => app)),
    ),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ceremony, Intake, Router, Seam }
```
