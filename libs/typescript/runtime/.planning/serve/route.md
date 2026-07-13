# [RUNTIME_ROUTE]

The serving assembly: routes are Layers under `HttpLayerRouter` — the app-assembled `HttpApi` mounts through `addHttpApi` beside raw routes, foreign realtime protocols mount through the `Mount` port fold, the resumable-upload rail mounts its tus dispatchers, the health trio serves the probe anchor, the webhook intake holds raw octets for signature verification, and the auth ceremonies lift the security wave's redirect and passkey round-trips into HTTP — all under ONE seam: mark mint, ambient provision, trace continuation, the shield header table, and the respondable net composed once as middleware so no handler, group, or app root re-states the cross-cutting stack and the served app's error channel is `never`. Host and header dispatch across several apps is `HttpMultiplex` rows; static assets serve with fingerprint-immutable cache rows, traversal refusal, and the `Etag.Generator` the runtime server Layer already carries. The engine is never named here — `HttpLayerRouter.serve` demands the `HttpServer` the boot module provides from `exec#RUNTIME_ROWS`, so a runtime change is a row selection at the root and the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same route Layers. The module ships on the `./server` exports subpath as `runtime/src/serve/route.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                             | [PUBLIC]           |
| :-----: | :-------------- | :---------------------------------------------------------------------------------- | :----------------- |
|  [01]   | `SEAM_ROWS`     | the one middleware composition: mark, ambient rows, trace, shield, respondable net | `Seam`             |
|  [02]   | `LAYER_ROUTES`  | api/docs/health/tus/mount route Layers, the webhook intake row                     | `Router`, `Intake` |
|  [03]   | `CEREMONY_ROWS` | oauth redirect pair, webauthn enroll/assert, refresh/logout, cookie application    | `Ceremony`         |
|  [04]   | `ASSET_ROWS`    | the SPA/static fold: fingerprint predicate, cache-header table, traversal refusal  | `Router`           |
|  [05]   | `SERVE_FOLD`    | multiplex rows, the serve Layer, the web-handler twin                              | `Router`           |

## [02]-[SEAM_ROWS]

[SEAM_ROWS]:
- Owner: `Seam` — the one cross-cutting composition, attached exactly once through `HttpLayerRouter.middleware`: `Seam.guard(app)` mints the request mark (id, instant, negotiated locale from the `accept-language` header against the ambient fallback), provides the three `Current` rows in one scoped provision, continues the W3C trace through `Current.traced` over the request headers, folds every escaping cause through `Problem.net` — self-rendering first, total ladder as the floor — and stamps the shield table on every response; the served app's error channel is `never` by construction.
- Law: `_SHIELD` is the security-header table — `strict-transport-security`, `x-content-type-options`, `x-frame-options`, `referrer-policy`, plus the CSP row — applied to every response by this seam reading the table; a header tweak is a row edit, and a handler hand-setting a shield header is the drift defect.
- Law: CORS is delegated, never re-implemented — the assembly composes `HttpLayerRouter.cors()` (or `HttpApiBuilder.middlewareCors(options)` on the api mount) with the options row as its one policy value; no `Seam` member renames it, because a forwarding member is the one-hop wrapper the platform surface already owns.
- Growth: a new cross-cutting response concern is one line in `Seam.guard`, inherited by every route Layer at once.
- Packages: `@effect/platform` (`HttpServerRequest`, `HttpServerResponse`); `effect` (`DateTime`, `Effect`, `Option`); `./api.ts` (`Current`); `./problem.ts` (`Problem`).

```typescript
import {
  type Cookies, Etag, FileSystem, type HttpApi, type HttpApiGroup, HttpApiScalar, type HttpApp, HttpLayerRouter, HttpMultiplex,
  type HttpPlatform, HttpServerRequest, HttpServerResponse, Path,
} from "@effect/platform"
import { type RpcGroup, RpcServer } from "@effect/rpc"
import { Context, DateTime, Duration, Effect, Layer, Option, Redacted, Schema } from "effect"
import { Cookie, CookieSpec, Departed, type MacKey, OAuth, Token, type Verified, Verify, WebAuthn } from "@rasm/ts/security"
import { Rail } from "@rasm/ts/data"
import { Life } from "../proc/life.ts"
import { Current, type GateFault, type Principal } from "./api.ts"
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

## [03]-[LAYER_ROUTES]

[LAYER_ROUTES]:
- Owner: `Router` — the route-Layer vocabulary the app root merges: `Router.api(api)` mounts the assembled `HttpApi` through `HttpLayerRouter.addHttpApi(api, { openapiPath })` with `HttpApiScalar.layerHttpLayerRouter` beside it so the derived document and the reference UI ride the same router; `Router.rpc(group, prefix)` mounts a contributed RPC group beside the raw routes through `RpcServer.toHttpApp(group)` — the `HttpApp` value form, so one router serves api, RPC, and raw rows without a second server; `Router.health` mounts the probe trio from `life#PROBE_ROUTES`'s anchor — `Life.route(kind)` is the path, `Life.report(kind)` the body encoded through the `Life.Report` schema, `pass`/`warn` encode 200 and `fail` encodes 503, so the path and the verdict never exist twice; `Router.mounts` folds `Effect.serviceOption(Mount)` and mounts every provided foreign-protocol row at its prefix under the `"*"` catch-all method literal — presence-as-data, an unwired port serves nothing and never crashes.
- Law: the tus rail mounts as dispatchers, never re-frames — `Router.rail(spec)` builds the data rail (`Rail.of(spec)`) and delegates its value to `Router.RailMount`, the host-lift port whose selected runtime row routes every method under the spec's route prefix into the rail's own dispatchers: the node engine drives `rail.node(req, res)` through `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse`, the fetch engine drives `rail.web(request)` through `BunHttpServerRequest.toRequest` or the web-handler assembly where the request is already fetch-shaped. The same adapter schedules `rail.groom` through the lifecycle plane, so offset semantics, staging custody, finalize, and grooming stay the data rail's while this module names no binding.
- Law: `Intake` is the held-octet webhook row — the raw body reads ONCE as bytes through the platform's own `arrayBuffer` accessor (`HttpIncomingMessage`'s byte member, lifted to `Uint8Array` at the seam) and is held, the spec's named signature header lifts from the request as `Option`, `verify.verify(dialect, octets, header, mac, tolerance)` runs the security wave's dialect fold over exactly those octets, and only a `Verified` receipt releases the enqueue through the app-declared ingress port — byte identity end to end, so re-serialization drift between verify and enqueue is unspellable; verification failure folds to the security fault's own class and the seam's net renders it.
- Boundary: which groups the api value carries is the app's assembly under `api#CONTRIBUTION`; the `Mount` Tag is `live#MOUNT_PORT`'s; the rail spec's cut policy and staging band are `data`'s.
- Growth: a new served surface is one route-Layer member composing an owning-page value; a second foreign protocol is a second `Mount` Layer at a different prefix, zero edits here.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpApiScalar`, `HttpServerResponse`); `@rasm/ts/data` (`Rail`); `@rasm/ts/security` (`Verify`); `../proc/life.ts` (`Life`); `./live.ts` (`Mount`).

```typescript
const _octets = (
  request: HttpServerRequest.HttpServerRequest,
): Effect.Effect<Uint8Array, Problem> =>
  Effect.mapError(
    Effect.map(request.arrayBuffer, (buffer) => new Uint8Array(buffer)),
    (fault) => Problem.of(fault),
  )

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
    onSome: (mounts) => Layer.mergeAll(...mounts.map((mount) => HttpLayerRouter.add("*", `${mount.prefix}/*`, () => mount.app))),
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

## [04]-[CEREMONY_ROWS]

[CEREMONY_ROWS]:
- Owner: `Ceremony` — one `Context.Tag` carrying the application-owned identity projection for raw ceremony routes and the non-OIDC OAuth subject resolver, plus the HTTP lift of the security wave's authentication round-trips under the fixed `/auth` cookie path: `authorize` redirects to `OAuth.authorize`'s minted URL (302, the state stash already held); `callback` decodes the provider's `code`/`state` query, exchanges through `OAuth.callback` into a `TokenPair`, and lands the session as cookies; `enroll`/`assert` each serve an `options` POST returning the RP-minted challenge JSON and a finish POST verifying through `WebAuthn.enrollFinish`/`assertFinish`; `refresh` rotates through `Token.refresh` reading the path-scoped refresh cookie under its `CookieSpec` name; `logout` revokes the authenticated session before writing the clearing set.
- Law: every mutating ceremony passes the CSRF gate BEFORE any state changes — the `_csrfed` fold reads the `CookieSpec.csrf` cookie and the `x-csrf-token` header and runs `Cookie.verify`'s constant-time double-submit compare, so the webauthn finish pair, `refresh`, and `logout` are unreachable from ambient cookies alone; the oauth `callback` is exempt because its `state` round-trip is that flow's own anti-forgery evidence, and cookie names read from the security `CookieSpec` table, never a route literal.
- Law: cookie application is one fold — `_cookied(response, framed)` reduces the security wave's `Cookies.Cookie` set through `HttpServerResponse.setCookie(name, value, options)`, so the security attribute policy table decides every attribute and no route names `httpOnly`, `sameSite`, or a path.
- Law: ceremonies own HTTP shape only — redirect codes, query decode, body admission, cookie reads, and status; establishing, rotating, verifying, and framing are the security wave's (`OAuth`, `WebAuthn`, `Token`, `Cookie`), while `Ceremony.identity` projects the authenticated `Principal` from the application's chosen raw-route credential lift and `Ceremony.resolveSubject` handles only providers without OIDC subject evidence. A handler is a decode, one security call, and one egress fold, and a security fault renders itself through the seam's net at its own class status. The `:provider` segment admits through the security vocabulary itself — `_Provider` decodes the param record against `Departed.fields.kind`, so `OAuth.authorize`/`callback` receive a proven `Provider.Kind` and an unrostered provider dies at the seam as a decode refusal, never inside the ceremony.
- Law: the passkey finish bodies admit through one Schema pair mirroring the verified `@simplewebauthn/server` wire shapes — `_Enroll` decodes the POSTed registration response (`id`, `rawId`, the attestation `response` block, optional attachment, extension outputs, `type: "public-key"`) into the `RegistrationResponseJSON` parameter `WebAuthn.enrollFinish` takes, `_Assert` the assertion twin for `assertFinish` — raw JSON crosses the decode seam exactly once and the browser collection half stays the ui wave's.
- Growth: a new ceremony (an OTP pair, a device-code flow) is one route pair under `_AUTH` composing its security owner; a new cookie role reframes through the same fold with zero route edits.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpServerRequest`, `HttpServerResponse`, `Cookies`); `@rasm/ts/security` (`OAuth`, `WebAuthn`, `Token`, `Cookie`, `CookieSpec`, `Departed` — the provider-kind decode anchor); `effect` (`Context`, `Schema`, `Redacted`).

```typescript
const _CSRF_HEADER = "x-csrf-token"

const _csrfed: Effect.Effect<void, Problem, Cookie | HttpServerRequest.HttpServerRequest> = Effect.gen(function* () {
  const request = yield* HttpServerRequest.HttpServerRequest
  const cookie = yield* Cookie
  yield* cookie.verify(
    Option.fromNullable(request.cookies[CookieSpec.csrf.name]),
    Option.fromNullable(request.headers[_CSRF_HEADER]),
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
    HttpLayerRouter.add("GET", `${_AUTH}/authorize/:provider`, () =>
      Effect.gen(function* () {
        const oauth = yield* OAuth
        const { provider } = yield* Effect.flatMap(HttpLayerRouter.params, Schema.decodeUnknown(_Provider))
        const target = yield* oauth.authorize(provider)
        return HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", target.href))
      })),
    HttpLayerRouter.add("GET", `${_AUTH}/callback/:provider`, () =>
      Effect.gen(function* () {
        const oauth = yield* OAuth
        const cookie = yield* Cookie
        const query = yield* HttpServerRequest.schemaSearchParams(_Callback)
        const { provider } = yield* Effect.flatMap(HttpLayerRouter.params, Schema.decodeUnknown(_Provider))
        const ceremony = yield* Ceremony
        const pair = yield* oauth.callback(provider, query.code, query.state, ceremony.resolveSubject)
        const framed = yield* cookie.frame(pair)
        const csrf = yield* cookie.csrf()
        return yield* _cookied(HttpServerResponse.empty({ status: 302 }).pipe(HttpServerResponse.setHeader("location", "/")), [...framed, csrf])
      })),
    HttpLayerRouter.add("POST", `${_AUTH}/webauthn/enroll/options`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const webauthn = yield* WebAuthn
        const subject = yield* _subject
        const request = yield* HttpServerRequest.schemaBodyJson(_EnrollOptions).pipe(Effect.mapError((fault) => Problem.of(fault)))
        const options = yield* webauthn.enrollStart(subject, request.userName)
        return yield* HttpServerResponse.json(options).pipe(Effect.orDie)
      })),
    HttpLayerRouter.add("POST", `${_AUTH}/webauthn/enroll`, () =>
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
    HttpLayerRouter.add("POST", `${_AUTH}/webauthn/assert/options`, () =>
      Effect.gen(function* () {
        yield* _csrfed
        const webauthn = yield* WebAuthn
        const subject = yield* _subject
        const options = yield* webauthn.assertStart(subject)
        return yield* HttpServerResponse.json(options).pipe(Effect.orDie)
      })),
    HttpLayerRouter.add("POST", `${_AUTH}/webauthn/assert`, () =>
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
    HttpLayerRouter.add("POST", `${_AUTH}/logout`, () =>
      Effect.zipRight(_csrfed, Effect.flatMap(_cleared, (framed) => _cookied(HttpServerResponse.empty({ status: 204 }), framed)))),
    HttpLayerRouter.add("POST", `${_AUTH}/refresh`, () =>
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
- Law: `_CACHE` is the cache-header table and `_FINGERPRINT` the predicate — a content-hashed filename is immutable for a year because its identity IS its content, the entry document and every un-fingerprinted path are `no-cache` because they are the mutable pointers INTO immutable content — two rows, total over every asset. The supplied `Etag.Generator` derives the selected file validator, the fold answers an exact `if-none-match` hit with `304`, and both the hit and body response carry the same `etag` and cache row.
- Law: traversal is structurally refused — the request path resolves under the root and the fold asserts the resolved target still carries the root as its prefix, so an encoded, normalized, or absolute escape lands outside the prefix and serves the SPA entry, never a distinct error that maps the filesystem for a probe.
- Boundary: what the assets ARE (app shell, prerender output, the self-hosted wasm bundles the ui wave serves beside the shell) is the ui wave's; this row owns only serving them byte-identical.
- Packages: `@effect/platform` (`FileSystem`, `Path`, `HttpPlatform`, `HttpServerRequest`, `HttpServerResponse`, `Etag`).

```typescript
const _CACHE = {
  immutable: "public, max-age=31536000, immutable",
  entry: "no-cache",
} as const

const _FINGERPRINT = /-[0-9a-f]{8,}\.[a-z0-9]+$/

const _assets = (options: { readonly root: string; readonly entry: string }): Effect.Effect<
  HttpServerResponse.HttpServerResponse,
  never,
  Etag.Generator | FileSystem.FileSystem | HttpPlatform.HttpPlatform | Path.Path | HttpServerRequest.HttpServerRequest
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
    const generator = yield* Etag.Generator
    const served = (file: string, policy: string) =>
      Effect.gen(function* () {
        const info = yield* fs.stat(file)
        const tag = Etag.toString(yield* generator.fromFileInfo(info))
        return request.headers["if-none-match"] === tag
          ? HttpServerResponse.empty({ status: 304 }).pipe(HttpServerResponse.setHeaders({ "cache-control": policy, etag: tag }))
          : yield* HttpServerResponse.file(file).pipe(
              Effect.map(HttpServerResponse.setHeaders({ "cache-control": policy, etag: tag })),
            )
      })
    return yield* served(chosen, cache).pipe(
      Effect.orElse(() => served(path.join(options.root, options.entry), _CACHE.entry)),
      Effect.orDie,
    )
  })
```

## [06]-[SERVE_FOLD]

[SERVE_FOLD]:
- Owner: the serve law — the app root merges its route Layers (`Router.api`, `Router.health`, ceremonies, intake rows, rail mounts, the asset route, `Router.mounts`), attaches `Seam.guard` through `HttpLayerRouter.middleware` once, and launches `HttpLayerRouter.serve` — a Layer whose `HttpServer` requirement the boot module satisfies from `exec#RUNTIME_ROWS`'s `serve` member, so node-versus-bun is a row selection and this module names no binding; the fetch-shaped twin is `HttpLayerRouter.toWebHandler` over the same merged Layers for edge runtimes, and a process whose whole life is the server parks through `life#PHASE_SPINE`'s boot law.
- Law: multiplex rows dispatch across whole apps — `HttpMultiplex.make` with `hostExact`/`hostRegex`/`headerStartsWith` predicates routes a virtual-host family (the api origin, the asset origin, a tenant subdomain family) to its own `HttpApp` before any router runs; a predicate is a row, dispatch is the platform's, and a hand-rolled host `if` chain in a handler is the deleted spelling.
- Law: readiness gates intake — the serving edge stops accepting by `life.phase`: the drain fold flips the phase before finalizers run, the ready report fails by fold, and the load balancer routes away while in-flight requests finish under the drain bands; no connection-draining code exists here because the phase spine and the runtime row already own the choreography.
- Boundary: the `node:http`/`Bun.serve` construction is `exec#RUNTIME_ROWS`'s row interior; TLS and unix-socket residency are the row's serve options; `iac` mirrors the drain budget and the probe paths from their owners, never from here.
- Growth: a new virtual host is one multiplex row; a new engine is one runtime-row edit with zero serve-fold changes.
- Packages: `@effect/platform` (`HttpLayerRouter`, `HttpMultiplex`); `effect` (`Layer`).

```typescript
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
  RailMount: _RailMount,
  rpc: <G extends RpcGroup.RpcGroup.Any>(group: G, prefix: `/${string}`) =>
    Layer.unwrapEffect(
      Effect.map(RpcServer.toHttpApp(group), (app) => HttpLayerRouter.add("POST", prefix, () => app)),
    ),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ceremony, Intake, Router, Seam }
```
