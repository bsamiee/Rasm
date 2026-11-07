# [SERVICES]

## [01]-[SERVICE_CLASSES]

Service classes are keys programs yield, and `make` states each implementation a static layer builds:

```ts
class Pricing extends Context.Service<Pricing>()('@app/pricing', {
    make: Catalog.useSync((catalog) => ({
        quote: Effect.fn('Pricing.quote')((lines: readonly Line[]) => Effect.forEach(lines, catalog.price, { concurrency: 'unbounded' }), Effect.map(BigDecimal.sumAll)),
    })),
}) {
    static readonly layer = Layer.effect(this, this.make);
}
```

`Catalog.price` returns one `Line`'s price, and `Catalog.useSync` reads `Catalog` once at layer build:
- Shapes derive from `make`, members close over the services `make` read, and each member requires nothing
- Keys are package and module path, followed by the class name where it differs from its file name
- `make` and its static layer take a parameter for each value the composition root or a `LayerMap` key supplies (a file path, a tenant)
- Facts the composition fixes (a driver dialect, a host version) resolve in `make`, an unsupported one failing layer build in place of each call
- Programs bind a service through `yield*`, and a call reaching one member uses `Pricing.use`
- Service members hold operations reading a resource, a service, or a host value, and computations over arguments stay functions
- `Context.Reference` holds a value every fiber reads, its `defaultValue` from a host or standard (`Intl.DateTimeFormat().resolvedOptions().locale`)
- `Layer.succeed(reference, value)` overrides a reference for a graph, `Effect.provideService` for one effect
- `Context.Reference` keys an annotation on a contract (`Tool`, `RpcGroup`), and `Context.get(x.annotations, Key)` reads it with its default
- Annotations differing from a shared set by one key derive through `Context.omit(Key)` or `Context.pick`, a rebuilt `Context.make` chain restating them

## [02]-[CONFIGURATION]

Settings decode once at layer build, and a missing or invalid key fails the build with `ConfigError` before any request runs:

```ts
class Ledger extends Context.Service<Ledger>()('@app/ledger', {
    make: Effect.gen(function* () {
        const { url, token } = yield* Config.schema(Schema.Struct({ url: Schema.URL, token: Schema.Redacted(Schema.String) }), 'ledger');
        const client = yield* Effect.acquireDisposable(Effect.tryPromise({ try: () => LedgerClient.connect(url, Redacted.value(token)), catch: (cause) => LedgerError.make({ reason: { _tag: 'Unavailable' }, subject: url.host, cause }) }));
        return { post: Effect.fn('Ledger.post')((entry: Entry) => Effect.tryPromise({ try: (signal) => client.post(entry, { signal }), catch: (cause) => LedgerError.make({ reason: { _tag: 'Rejected' }, subject: entry.id, cause }) })) };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}
```

`LedgerClient` is a host SDK's `AsyncDisposable` client, `Entry` a posted record, and `LedgerError` the package error:
- Path arguments and field names spell each key's path, and `ConfigProvider.constantCase` at a root reads `ledger.url` from `LEDGER_URL`
- Fields decode into their domain type (`Schema.URL`, `Schema.Int`, `Schema.DurationFromString`), readers receive the decoded value
- Separated lists in one variable (`a,b`) read through `Config.Array(schema, path)`, `key=value` pairs through `Config.Record`
- Secrets are `Schema.Redacted(Schema.String)` fields, rendered as `<redacted>` until `Redacted.value` reads one at the host call
- `ConfigProvider.ConfigProvider` is a reference defaulting to `fromEnv()`
- Roots replace the provider to transform paths (`constantCase`, `nested`), read a host record (`fromUnknown`), or add a source (`layerAdd`)
- Configs are effects a layer reads, `Layer.effect(<reference>, <config>)` setting a reference from a setting
- Layer parameters a setting supplies are `Config.Config<A>` the build yields, a root `Layer.unwrap` mapping a config restating them
- Entry points yield a setting (`Config.String('HOME')`) in one `Effect.all` with their service keys, a `process.env` or `node:os` read restating it
- Settings a consumer models as absent read through `Config.option`, an invalid value failing, where a required read fails the program
- Implementations a setting selects are `Layer.unwrap` over one table of layers, keyed by a `Schema.Literals(Struct.keys(table))` field

## [03]-[SCOPED_RESOURCES]

Layers remove `Scope` from their requirements. Resource lifetime selects where a scope opens:

| [INDEX] | [LIFETIME]   | [FORM]                                                                    |
| :-----: | :----------- | :------------------------------------------------------------------------ |
|  [01]   | Program      | Acquired in `make`, released with layer                                   |
|  [02]   | One call     | `Effect.scoped` as a trailing argument of `Effect.fn`, released on return |
|  [03]   | Cleanup only | `Effect.addFinalizer` in scope owning state it restores                   |
|  [04]   | Until change | `ScopedRef.set(ref, acquire)` acquires next, then closes previous scope   |

- Host handles implementing `Disposable` or `AsyncDisposable` enter through `Effect.acquireDisposable`, released by their own dispose
- Handles without a dispose method enter through `Effect.acquireRelease`, its release receiving the `Exit`
- Host clients with a package-published layer (`NodeHttpClient.layerUndici`) enter through the published layer
- Values a scope owns are read inside it, and a value read after a call is acquired at a longer lifetime

## [04]-[LAYERS]

Layers compose at a composition root into one graph, and the compiler rejects a graph leaving a requirement open:

| [INDEX] | [OPERATOR]                           | [RESULT]                                                |
| :-----: | :----------------------------------- | :------------------------------------------------------ |
|  [01]   | `Layer.mergeAll(a, b)`               | Outputs and requirements of each                        |
|  [02]   | `a.pipe(Layer.provide(b))`           | Outputs of `a`, `b` satisfies `a` and stays hidden      |
|  [03]   | `a.pipe(Layer.provideMerge(b))`      | Outputs of both, `b` satisfies `a` and stays readable   |
|  [04]   | `a.pipe(Layer.provide([b, c]))`      | Outputs of `a`, `b` and `c` merge into one hidden layer |
|  [05]   | `a.pipe(Layer.provideMerge([b, c]))` | Outputs of `a`, `b`, and `c`, `b` and `c` readable      |

```ts
const Store = Checkout.layer.pipe(Layer.provideMerge(Pricing.layer), Layer.provide([Ledger.layer, Catalog.layer]));
```

`Checkout` reads `Pricing` and `Ledger`, `Pricing` reads `Catalog`, and `Catalog` reads the `HttpClient` each host supplies:
- Layers inside one `mergeAll` or `provide` array build concurrently and satisfy none of each other's requirements
- Swappable dependencies stay in their reading layer's requirements, and each root provides its host's implementation
- References stay out of requirements, and a root provides `ConfigProvider` in its last `Layer.provide` to reach every layer
- Layers memoize by reference within one build, a layer function's result binds to one `const`, `Layer.fresh` builds one per consumer
- `Effect.provide` reuses a layer an enclosing `provide` or runtime built, `{ local: true }` and separate `provide` calls each building their own
- Graphs a request value selects (a tenant, a region) are one `LayerMap.Service` with a `lookup` mapping the key to a layer
- `Effect.provide(Tenants.get(tenant))` builds a key's graph on first use and releases it `idleTimeToLive` after its last user

## [05]-[RUNTIME]

Hosts run their composed graph once at an entry point, and each host's lifecycle selects its runtime call:

| [INDEX] | [HOST]                              | [ENTRY]                                                  |
| :-----: | :---------------------------------- | :------------------------------------------------------- |
|  [01]   | Process program owns                | `NodeRuntime.runMain` over `Layer.launch` or main effect |
|  [02]   | Host calling effects on its events  | `ManagedRuntime.make(layer)`, one `runPromise` per event |
|  [03]   | Platform calling a fetch handler    | `HttpRouter.toWebHandler(layer)` at module scope         |
|  [04]   | React tree reading effects as state | `Atom.runtime(layer)` from `effect/reactivity`           |

```ts
Api.pipe(Layer.provide(Store), Layer.provide([NodeHttpClient.layerUndici, ConfigProvider.layer(ConfigProvider.constantCase(ConfigProvider.fromEnv()))]), Layer.launch, NodeRuntime.runMain);
```

`Api` is a server layer reading `Checkout`, and `runMain` interrupts its program on `SIGINT` or `SIGTERM`, releasing every resource:
- Workers read bindings through `ConfigProvider.fromUnknown(env)` over the `cloudflare:workers` export, building once at module scope
- Host events pass their `AbortSignal` as `signal`, and a canceled event interrupts its run
- Importable Node modules can own CLI startup with a top-level `if (import.meta.main)` around `runMain`
- Library entries taking app values are `Effect.fnUntraced(body, Effect.provide(layer), NodeRuntime.runMain({ teardown }))`, a call running the program
- Host callbacks are one `Effect.fnUntraced(body, Effect.scoped, (effect) => runtime.runPromise(effect))` typed by the callback type
- Hosts reading a callback's parameters from its source take a literal arrow with an object-pattern first parameter
- Host thunks and synchronous callbacks run an exported `Effect` through `runPromiseWith` or `runSyncWith` over `yield* Effect.context<R>()`
- `ManagedRuntime` builds its layer on the first run, later runs read the same services, and a failed build stays cached
- One runtime serves one host process, created beside its entry point and disposed in its teardown hook
- `dispose` releases every resource the layer acquired, and later runs fail with a defect
- `await using` binds a runtime to a block, its `Symbol.asyncDispose` disposing it at block exit
