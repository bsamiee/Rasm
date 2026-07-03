# [EDGE_EMIT]

`api/emit.ts` is the derivation surface over the app-assembled `HttpApi` value: the OpenAPI document, the served reference UI, the canonical spec artifact the contract gate diffs, and the typed client SDK all compute from the one value the app assembled — parameterized on it, never importing it — so spec, docs, SDK, and server are projections of a single source and drift is structurally impossible. Every member here EARNS existence over the raw platform call by binding edge policy to the derivation: the artifact is byte-stable for diffing, the docs stack is one Layer row pairing the document route with its reference UI, and the derived client arrives under the shared egress policy instead of a naked transport.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                          | [PUBLIC] |
| :-----: | :--------------- | :----------------------------------------------------------------- | :------- |
|  [01]   | [SPEC_ARTIFACT]  | the canonical document artifact and the served docs stack           | `Emit`   |
|  [02]   | [DERIVED_CLIENT] | the typed-SDK derivation under the shared client policy             | `Emit`   |

## [2]-[SPEC_ARTIFACT]

[SPEC_ARTIFACT]:
- Owner: `Emit.artifact` — the canonical spec artifact: `OpenApi.fromApi(api)` serialized with sorted keys and fixed indentation so two emissions of one contract are byte-identical and the CI contract gate diffs bytes, never re-parses; the `cli/verb.ts` inspect verb and the contract-drift check both consume this one member, so "the spec" has exactly one canonical byte form.
- Law: `Emit.docs` is the served documentation stack — one Layer merging `HttpApiBuilder.middlewareOpenApi()` (the document route derived from the served api) with `HttpApiScalar.layer()` (the interactive reference over that same route) — composed by the app root as one selection; serving docs without the document route, or hand-mounting a spec file the server did not derive, is the drift defect this row deletes.
- Law: contract documentation is annotation material on the api value — titles, descriptions, and identifiers ride the owners and groups where they are declared (`HttpApi.make(id).annotate`, endpoint schemas' own annotations) and flow into the document through the derivation; a description authored in a docs file restates what the declaration already emits.
- Law: the artifact is emission, not authority — the served contract IS the authority; the artifact exists for diffing and publication, is never committed as a second truth, and a consumer that wants the contract derives the client instead of parsing the artifact.
- Growth: a new documentation surface (a second reference UI, a JSON-schema bundle per owner) is one derivation member over the same api parameter.
- Packages: `@effect/platform` (`OpenApi`, `HttpApiBuilder`, `HttpApiScalar`); `effect` (`Layer`, `Array`, `Record`, `Order`, `Predicate`).

```typescript
import { type HttpApi, HttpApiBuilder, HttpApiClient, type HttpApiGroup, HttpApiScalar, type HttpClient, OpenApi } from "@effect/platform"
import { Array, Layer, Order, Predicate, Record, pipe } from "effect"

const _byKey: Order.Order<readonly [string, unknown]> = Order.mapInput(Order.string, (entry: readonly [string, unknown]) => entry[0])

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

const _artifact = <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(api: HttpApi.HttpApi<Id, Groups, E, R>): string =>
  JSON.stringify(_stable(OpenApi.fromApi(api)), null, 2)

const _docs: Layer.Layer<never, never, HttpApi.Api> = Layer.mergeAll(HttpApiBuilder.middlewareOpenApi(), HttpApiScalar.layer())
```

## [3]-[DERIVED_CLIENT]

[DERIVED_CLIENT]:
- Owner: `Emit.client` — the typed SDK derived from the same api value through `HttpApiClient.make(api, { baseUrl, transformClient })`, with the transform slot carrying the edge egress posture (the shared `host/net` client policy the app composed: retry-transient, status filtering, trace propagation) so a derived consumer inherits the same resilience as every other outbound call; a hand-written fetch client beside a contract, or a client typed separately from the served api, is the named defect.
- Law: the client faults are the declared faults — the SDK reconstructs each endpoint's `addError` tagged family plus the transport and decode faults, so a consumer dispatches `catchTag` on the exact vocabulary the handler failed with and one error language spans the wire.
- Law: derivation is call-time and parameterized — `Emit.client` takes the api value and the origin; nothing here caches, names, or holds an api instance, keeping the assembled value's no-lib-side-existence law intact.
- Boundary: the client policy rows themselves are `host/net`'s; in-repo service-to-service calls prefer the RPC family (`api/group.ts` rosters) — this derivation serves external and cross-runtime consumers of the public contract.
- Packages: `@effect/platform` (`HttpApiClient`, `HttpClient`).

```typescript
const _client = <Id extends string, Groups extends HttpApiGroup.HttpApiGroup.Any, E, R>(
  api: HttpApi.HttpApi<Id, Groups, E, R>,
  options: {
    readonly baseUrl: string
    readonly transform: (client: HttpClient.HttpClient) => HttpClient.HttpClient
  },
) => HttpApiClient.make(api, { baseUrl: options.baseUrl, transformClient: options.transform })

const Emit: {
  readonly artifact: typeof _artifact
  readonly client: typeof _client
  readonly docs: Layer.Layer<never, never, HttpApi.Api>
} = {
  artifact: _artifact,
  client: _client,
  docs: _docs,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Emit }
```
