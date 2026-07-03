# [EDGE_SERVE]

`api/serve.ts` is the one serving surface: an engine roster (`node` over `node:http`, `bun` over `Bun.serve`) selected by one config read, the hostless `toWebHandler` row for fetch-shaped runtimes, and the SPA/static-asset row with immutable-asset cache headers — every row consuming the same app-assembled `Layer<HttpApi.Api>` and every response passing the `Problem.guard` net, the shield header table, and the ambient `Current` provision in one composed seam. A runtime change is a config row or a Layer selection at the app root, never a fork; this module is the only edge file that names a runtime binding, and the per-runtime subpath split keeps the `node:`-importing rows physically unreachable from browser resolution.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                              | [PUBLIC] |
| :-----: | :------------ | :---------------------------------------------------------------------- | :------- |
|  [01]   | [ENGINE_ROWS] | the engine roster, config-driven selection, the serve stack, web handler | `Serve`  |
|  [02]   | [ASSET_ROW]   | the SPA/static row: fingerprint predicate, cache-header table, fallback  | `Serve`  |

## [2]-[ENGINE_ROWS]

[ENGINE_ROWS]:
- Owner: `Serve` — the engine roster is one interior `as const` table of serve stacks: each row is `HttpApiBuilder.serve(Serve.seam)` provided with its runtime server Layer (`NodeHttpServer.layerConfig(createServer, { port })` | `BunHttpServer.layerConfig({ port })`), the port arriving through `Config.port` so the row is a Layer value with zero knobs; `Serve.stack` selects the row as `Layer.unwrapEffect` over `Config.literal(...Struct.keys(_engines))("SERVE_ENGINE")`, so admission, the engine union, and dispatch read one anchor and a new engine is one row.
- Law: `Serve.seam` is the one middleware composition — every request runs `Current.provide` (mark minted from the request identity), `Current.traced` (W3C continuation over the request headers), then the handler, then `Problem.guard` with the shield table stamped on every response — attached once here, so no handler, group, or app root re-states the cross-cutting stack and the served app's error channel is `never`.
- Law: the hostless row is `HttpApiBuilder.toWebHandler(layer)` composed at the app root — the `Request => Response` handler for fetch runtimes over the same api Layer and the same seam, no listener, no forwarding member here to rename it; a process whose whole life is the server boots `Serve.stack` under `Layer.launch`, and the boot module, never this one, calls a run method.
- Law: the mount fold is presence-as-data at the app's router assembly — the app root reads `Effect.serviceOption(Mount)` for `live/socket.ts`'s protocol-handler port and mounts the provided app at its prefix beside the served api (`HttpRouter.mountApp`); an unwired mount is a selection, never a crash, and this module never names the port because mounting composes router material only the app holds.
- Boundary: this module is the runtime seam — the `node:http` `createServer` import is legal here and nowhere else in the folder; engine Layers come from `@effect/platform-node`/`-bun`; the api value it serves is the app's, built under `api/group.ts`'s law.
- Growth: a new engine (a new runtime binding) is one roster row; a new cross-cutting response concern is one line in `Serve.seam`, inherited by every engine at once.
- Packages: `@effect/platform` (`HttpApiBuilder`, `HttpServerResponse`); `@effect/platform-node` (`NodeHttpServer`); `@effect/platform-bun` (`BunHttpServer`); `effect` (`Config`, `Layer`, `Struct`, `Effect`).

```typescript
import { createServer } from "node:http"
import { Etag, FileSystem, HttpApi, HttpApiBuilder, HttpServerRequest, HttpServerResponse, Path } from "@effect/platform"
import { BunHttpServer } from "@effect/platform-bun"
import { NodeHttpServer } from "@effect/platform-node"
import { Config, type ConfigError, DateTime, Effect, Layer, Option, Struct } from "effect"
import { Problem } from "../problem/detail.ts"
import { Current, Gate } from "./middleware.ts"

const _PORT = Config.port("PORT").pipe(Config.withDefault(8080))

const _seam = <E, R>(
  app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R | HttpServerRequest.HttpServerRequest>,
): Effect.Effect<HttpServerResponse.HttpServerResponse, never, R | HttpServerRequest.HttpServerRequest> =>
  Effect.gen(function* () {
    const request = yield* HttpServerRequest.HttpServerRequest
    const now = yield* DateTime.now
    const fallback = yield* Current.Locale
    const mark: Current.Mark = {
      id: crypto.randomUUID(),
      at: now,
      tenant: Option.none(),
      locale: Option.some(Current.negotiate(Option.fromNullable(request.headers["accept-language"]), fallback)),
    }
    return yield* Problem.guard(Current.traced(app, request.headers)).pipe(
      (guarded) => Current.provide(guarded, mark, fallback),
      Effect.map(HttpServerResponse.setHeaders(Gate.shield)),
    )
  })

const _engines = {
  node: HttpApiBuilder.serve(_seam).pipe(Layer.provide(NodeHttpServer.layerConfig(() => createServer(), { port: _PORT })), Layer.provide(Etag.layer)),
  bun: HttpApiBuilder.serve(_seam).pipe(Layer.provide(BunHttpServer.layerConfig({ port: _PORT }))),
} as const

declare namespace Serve {
  type Engine = keyof typeof _engines
}
```

## [3]-[ASSET_ROW]

[ASSET_ROW]:
- Owner: `Serve.assets` — the SPA/static row as one request fold: resolve the request path under the asset root through the `Path` capability, serve the file when it exists, fall back to the SPA entry for every path-shaped miss (client-rendered routes hydrate from one entry), and stamp the cache row the fingerprint predicate selects.
- Law: `_CACHE` is the cache-header table and `_FINGERPRINT` the predicate — a content-hashed filename (`app-3f9c2a1b.js`) is immutable for a year because its identity IS its content, the entry document and every un-fingerprinted path are `no-cache` because they are the mutable pointers INTO immutable content — two rows, total over every asset, with `Etag.layer` composed on the stack so conditional requests revalidate the mutable row for free.
- Law: traversal is structurally refused — the resolved target must remain under the root (the fold rejects any `..` segment before resolution), and refusal serves the SPA entry, never a distinct error that maps the filesystem for a probe.
- Law: the prerender posture is served, not rendered — build-time per-route static HTML lands in the asset root and this row serves it byte-identical; a streaming-SSR runtime is the named non-goal, structurally unreachable because `react*` cannot resolve inside `scope:edge`.
- Boundary: mounting is the app's — the asset app mounts beside the api under the app's router (`HttpRouter.mountApp` or the layer router); `browser` owns what the assets ARE (PWA shell, prerender output); this row owns only serving them.
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
    const target = clean.includes("..") ? options.entry : path.join(options.root, clean)
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

const Serve: {
  readonly engines: typeof _engines
  readonly assets: typeof _assets
  readonly seam: typeof _seam
  readonly stack: Layer.Layer<never, ConfigError.ConfigError, HttpApi.Api>
} = {
  engines: _engines,
  assets: _assets,
  seam: _seam,
  stack: Layer.unwrapEffect(
    Config.literal(...Struct.keys(_engines))("SERVE_ENGINE").pipe(
      Config.withDefault("node"),
      Effect.map((kind) => _engines[kind]),
    ),
  ),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Serve }
```
