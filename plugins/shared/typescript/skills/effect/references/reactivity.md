# [REACTIVITY]

## [01]-[CLIENTS]

Atoms hold derived state in a registry that `@effect/atom-react` hooks read, and `Reactivity` keys connect each write to the reads it invalidates. Client services over a contract derive one atom per call:

```ts
class PortfolioClient extends AtomHttpApi.Service<PortfolioClient>()('@rasm/portfolio/editor/state/PortfolioClient', { api: Api, httpClient: FetchHttpClient.layer }) {}

const recoveryRequest = PortfolioClient.query('assets', 'recovery', { reactivityKeys: ['recovery'] });
const discardRequest = PortfolioClient.runtime.fn((id: string) => PortfolioClient.use((api) => api.assets.discard({ params: { id } })), { reactivityKeys: ['recovery'] });
```

`Api` is an `HttpApi` holding an `assets` group, and writing an id to `discardRequest` reruns `recoveryRequest` once the discard succeeds:
- `AtomRpc.Service<Self>()(id, { group, protocol })` derives `query(tag, payload, options)` and `mutation(tag)` over an `RpcGroup`, a stream query yielding a pull atom
- `AtomHttpApi.Service` derives `query(group, endpoint, request)` and `mutation(group, endpoint)`, and both services' queries take `staleTime`, `timeToLive`, and `serializationKey`
- Keys invalidate after a success alone, and a write whose failure also changes server state runs `Effect.tapError(() => Reactivity.invalidate(keys))`
- `{ assets: [id] }` keys invalidate `assets` and `assets:<id>`, and `Atom.withReactivity(keys)` attaches keys to any atom
- Interfaces merged into `Reactivity.Keys` type every key

## [02]-[ATOMS]

State an atom holds selects its constructor:

| [INDEX] | [STATE]                          | [ATOM]                                                                                   |
| :-----: | :------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | Value a view writes              | `Atom.make(initial)`                                                                     |
|  [02]   | Value derived from other atoms   | `Atom.make((get) => ...)`, `Atom.mapResult`                                              |
|  [03]   | Effect result over a service     | `Client.runtime.atom(effect)`, `Atom.runtime(layer).atom`                                |
|  [04]   | Write running an effect          | `runtime.fn((arg, get) => effect, { reactivityKeys })`                                   |
|  [05]   | Server read and write            | `Client.query`, `Client.mutation`                                                        |
|  [06]   | Browser storage or URL parameter | `Atom.kvs({ runtime, key, schema, defaultValue })`, `Atom.searchParam(name, { schema })` |
|  [07]   | External store                   | `Atom.make` with `get.addFinalizer(store.subscribe(...))`                                |

Atoms dispose when no component mounts them:
- `Atom.keepAlive` holds one for the registry's life, `Atom.setIdleTTL(d)` for `d` after its last reader
- `keepAlive` atoms over `Effect.acquireRelease` share one resource among their readers
- `Atom.family(key => atom)` memoizes atoms by key
- `runtime.fn` atoms interrupt a write on the next unless `concurrent: true`, and writing `Atom.Reset` or `Atom.Interrupt` resets or cancels one
- `Atom.writable(read, write, refresh)` declares one writable atom over a request atom, and a `runtime.fn` edits it through `get.registry.update` with an `Optic` lens
- `Atom.swr({ staleTime })`, `withRefresh(d)`, `debounce(d)`, `optimistic`, and `withEquality(eq)` set refresh, update, and notify policies
- `AtomRegistry.layer` and `Reactivity.layer` provide both services outside React, `Atom.get` and `Atom.getResult(atom)` reading an atom in an effect
- `AsyncResult.builder(result).onInitial(f).onSuccess(f).onErrorTag(tag, f).render()` renders a result
- Failed results keep their last success in `previousSuccess`

## [03]-[REACT]

`@effect/atom-react` reads atoms in components and passes server state to the client:
- `useAtomValue(atom, select)`, `useAtomSet`, `useAtomSuspense`, and `useAtomRefresh` read and write atoms
- `useAtom(atom, { mode: 'promise' })` and `useAtomSet(atom, { mode: 'promiseExit' })` await a write
- `useAtomMount(atom)` keeps an atom no component reads mounted for a component's life, and `useAtomSubscribe(atom, f)` runs `f` on each change
- `Atom.withServerValue(() => value)` renders `value` on the server and during hydration for an atom over browser state
- `Atom.withServerValueInitial` renders an async atom's initial state on the server and during hydration
- `Atom.serializable({ key, schema })` marks an atom for `Hydration.dehydrate`, and `HydrationBoundary` under `RegistryProvider` loads it as `state`
