# [EDGE_GROUP]

`api/group.ts` is the contribution law every entry family instantiates: a domain folder exports its `HttpApiGroup` as data paired with a handler builder over the assembled api value, an `RpcGroup` as data paired with its handler Layer, and the APP — never this module, never any lib module — chains exactly one `HttpApi.make(id).add(group)…` value and crosses exactly one RPC protocol row with one serialization row at its root. The god-contract is structurally impossible because `HttpApiBuilder.group` demands the assembled api the lib never holds. `Convention` is the shared surface vocabulary both families speak: version prefixes as rows, cursor pagination as one generic schema constructor, so every contributed group paginates and versions identically without a convention document.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                        | [PUBLIC]       |
| :-----: | :------------- | :------------------------------------------------------------------------------ | :------------- |
|  [01]   | [CONVENTION]   | version-prefix rows, cursor brand, page-query and page-shape constructors        | `Convention`   |
|  [02]   | [CONTRIBUTION] | the http and rpc contribution pairings, the protocol and serialization rosters   | `Contribution` |

## [2]-[CONVENTION]

[CONVENTION]:
- Owner: `Convention` — the assembled vocabulary: the version tuple (`v1` today; a new major is one tuple entry every consumer inherits), the opaque `Cursor` brand, the `PageParams` query schema (cursor as `Option`, limit defaulted and ceiling-bounded at the declaration so no handler re-checks bounds), and `Convention.page(item)` — one generic constructor deriving the page envelope for any item schema, so a per-shape page schema cannot exist.
- Law: pagination is cursor-only — the cursor is minted by the owning read surface, opaque to the caller, and bounded at the brand; `next` absent means exhausted, spelled `Option` on the decoded side by `optionalWith` — offset pagination has no vocabulary here and cannot be contributed.
- Law: the version prefix attaches at the group — `group.prefix(Convention.prefix("v1"))` — so a group is versioned as contributed data and two majors of one group coexist as two contributions; a version segment hand-written into an endpoint path is the drift defect.
- Law: `PageParams` decodes at the endpoint through `.setPayload`/`HttpApiEndpoint` query admission — the handler receives `Option<Cursor>` and a total `limit`, never raw query strings.
- Growth: a new convention axis (sort grammar, field selection) is one schema row on this owner, inherited by every group that composes it.
- Packages: `effect` (`Schema`, `Option`).

```typescript
import { RpcSerialization, RpcServer } from "@effect/rpc"
import { Layer, Schema } from "effect"

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

## [3]-[CONTRIBUTION]

[CONTRIBUTION]:
- Owner: `Contribution` — the pairing law as two constructors: `Contribution.http(group, handlers)` pairs an `HttpApiGroup` with its handler builder — a function OF the assembled api, because `HttpApiBuilder.group(api, name, build)` demands the api value only the app holds, which is the mechanical fact that makes the god-contract impossible; `Contribution.rpc(group, handlers)` pairs an `RpcGroup` with the handler Layer its `toLayer` already built, because RPC handlers bind to the group alone.
- Law: the app assembly is three chained folds stated here as law — `HttpApi.make(id).add(a.group).add(b.group)` builds the one api value; each http row's `handlers(api)` Layer merges under `Layer.provide` into `HttpApiBuilder.api(api)`; each rpc row's group merges through `group.merge(other)` into one served group — and the assembled values exist only in the app's composition root, with `api/serve.ts` consuming the resulting `Layer<HttpApi.Api>`.
- Law: `Contribution.protocols` and `Contribution.codecs` are the RPC serve rosters — protocol rows (`http` and `websocket` as path-parameterized factories, `worker` as the zero-argument runner row, so every row shares one call shape) crossed with serialization rows (`json`, `ndjson`, `msgpack`) — two orthogonal axes selected once at the app root; a transport or codec choice inside a handler, or a procedure re-declared per transport, is the named defect.
- Law: group-scoped obligations ride the declaration — `.middleware(Gate.Authn)` on a protected `HttpApiGroup`, `.addError` for group-wide faults, `RpcGroup`'s own `.middleware` for the RPC arm — so a cross-cutting obligation is recoverable from the contributed value, never discovered in handler bodies.
- Law: streaming procedures declare `stream: true` on `Rpc.make` and nothing else — the protocol row frames chunks and exit; hand-framing a stream over a unary procedure is rejected on sight.
- Boundary: middleware row semantics are `api/middleware.ts`'s; serve-row selection is `api/serve.ts`'s; derived spec/client surfaces are `api/emit.ts`'s; the WS realtime endpoints ride `live/socket.ts`, not an rpc row, when the feed is a `state` Subscribable.
- Growth: a new entry family (a queue consumer surface, a cron surface) is one new pairing constructor on this owner under the same shape — group as data, handlers as Layer or reader — never a new assembly law.
- Packages: `@effect/platform` (`HttpApi`, `HttpApiGroup`, `HttpApiBuilder`); `@effect/rpc` (`Rpc`, `RpcGroup`, `RpcServer`, `RpcSerialization`); `effect` (`Layer`).

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
  worker: () => RpcServer.layerProtocolWorkerRunner,
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Contribution, Convention }
```
