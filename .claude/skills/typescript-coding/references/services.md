# [SERVICES]

Covers services and layers, from the service class to configuration, scoped resources, composition, and the runtime a host runs.

## [01]-[SERVICE_CLASSES]

Service classes are the keys programs yield, and each static layer builds the implementation `make` states:

```ts
class Pricing extends Context.Service<Pricing>()('@app/pricing/Pricing', {
    make: Effect.map(Catalog, (catalog) => ({
        quote: Effect.fn('Pricing.quote')(
            (lines: ReadonlyArray<Line>) => Effect.forEach(lines, (line) => catalog.price(line.sku).pipe(Effect.map(BigDecimal.multiply(BigDecimal.fromBigInt(BigInt(line.quantity))))), { concurrency: 'unbounded' }),
            Effect.map(BigDecimal.sumAll),
        ),
    })),
}) {
    static readonly layer = Layer.effect(Pricing, Pricing.make);
}
```

Trailing arguments of `Effect.fn` pipe the body's effect, so `quote` prices every line concurrently and folds the totals in one member:
- Programs bind a service through `yield* Pricing` in an `Effect.fn` body, a host call reaching one member uses `Pricing.use`
- `make` takes an argument only for a key that selects an instance (a tenant), and its static layer takes the same key
- `Context.Reference` with `defaultValue` holds a value every fiber reads with a default (a flag, a locale)
- `Layer.succeed(reference)(value)` overrides a reference for a graph, `Effect.provideService` for one effect
- Computations reading no resource, service, or host value are functions the services call

## [02]-[CONFIGURATION]

Configuration is a `Config` value a service yields inside `make`, so settings decode once when the layer builds and a missing or invalid key fails the build with `ConfigError` before any request runs. `ConfigProvider.ConfigProvider` is a reference defaulting to `fromEnv()`, and the root replaces it once:

```ts
const Env = ConfigProvider.layer(ConfigProvider.constantCase(ConfigProvider.fromEnv()));
```

- `Config.schema` over one `Schema.Struct` reads each field under one path, `constantCase` maps path `ledger.url` to `LEDGER_URL`
- Each service reads the keys it uses, its struct states every constraint and the decode enforces it
- Fields decode into their domain type (`Schema.URL`, `Schema.Int`, `Schema.DurationFromString`), readers receive the decoded value
- Defaults sit on their field through `Schema.withDecodingDefaultKey`, stated in the encoded form the provider supplies
- Secrets are `Schema.Redacted(Schema.String)` fields, their string, JSON, and inspected forms render as `<redacted>` in logs, errors, and spans
- `ConfigProvider.fromUnknown` supplies a fixed record in place of the environment, every setting enters through the provider

## [03]-[SCOPED_RESOURCES]

Resources acquired in `make` belong to the layer: `Layer.effect` removes `Scope` from its requirements, and each release runs when the program that built the layer ends:

```ts
class Ledger extends Context.Service<Ledger>()('@app/ledger/Ledger', {
    make: Effect.gen(function* () {
        const { url, token, timeout } = yield* Config.schema(
            Schema.Struct({
                url: Schema.URL,
                token: Schema.Redacted(Schema.String),
                timeout: Schema.DurationFromString.pipe(Schema.withDecodingDefaultKey(Effect.succeed('5 seconds'))),
            }),
            'ledger',
        );
        const client = yield* Effect.acquireRelease(
            Effect.tryPromise({ try: () => connect(url, Redacted.value(token)), catch: (cause) => LedgerError.make({ reason: 'unavailable', subject: url.host, cause }) }),
            (opened) => Effect.promise(() => opened.close()),
        );
        return {
            post: Effect.fn('Ledger.post')(
                (entry: Entry) =>
                    Effect.tryPromise({ try: (signal) => client.post(entry, { signal }), catch: (cause) => LedgerError.make({ reason: 'rejected', subject: entry.id, cause }) }),
                Effect.timeout(timeout),
            ),
        };
    }),
}) {
    static readonly layer = Layer.effect(Ledger, Ledger.make);
}
```

`Effect.timeout` interrupts `post` once `timeout` expires, and interruption aborts the host request through its `signal`. Each resource's lifetime selects where its scope opens:

| [INDEX] | [LIFETIME]   | [FORM]                                                                                             |
| :-----: | :----------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | Program      | `Effect.acquireRelease` in `make`, released with the layer                                         |
|  [02]   | One call     | `Effect.scoped` as a trailing argument of `Effect.fn`, released on return                          |
|  [03]   | One key      | `LayerMap.Service` whose `lookup` is a keyed static layer, released once idle for `idleTimeToLive` |
|  [04]   | Cleanup only | `Effect.addFinalizer` in the scope that owns the state it restores                                 |

- Releases receive the `Exit` and run on success, failure, and interruption alike
- `X.get(key)` on a `LayerMap` provides one key's services to a program, built on first use and shared while in use
- Host clients whose package publishes a layer (`NodeHttpClient.layerUndici`, `PgClient.layerConfig`) enter through that layer
- Values a scope owns are read inside it, a value read after the call is acquired at the longer lifetime

## [04]-[LAYERS]

Layers compose at the composition root into one graph, and the compiler rejects a graph that leaves a requirement open:

| [INDEX] | [OPERATOR]                      | [RESULT]                                                        |
| :-----: | :------------------------------ | :-------------------------------------------------------------- |
|  [01]   | `Layer.mergeAll(a, b)`          | Outputs and requirements of each                                |
|  [02]   | `a.pipe(Layer.provide(b))`      | Outputs of `a`, `b` satisfies `a` and stays hidden              |
|  [03]   | `a.pipe(Layer.provideMerge(b))` | Outputs of both, `b` satisfies `a` and the program reads it too |
|  [04]   | `a.pipe(Layer.provide([b, c]))` | Outputs of `a`, `b` and `c` merge into one hidden stage         |

```ts
const Domain = Layer.mergeAll(Orders.layer, Refunds.layer).pipe(Layer.provideMerge(Pricing.layer), Layer.provide([Ledger.layer, Catalog.layer]));

const Server = Domain.pipe(Layer.provide([NodeHttpClient.layerUndici, Env]));
const Browser = Domain.pipe(Layer.provide([FetchHttpClient.layer, Env]));
```

`Orders` and `Refunds` read `Pricing`, `Pricing` reads `Catalog`, and `Catalog` reads the `HttpClient` each root supplies:
- Layers inside one `mergeAll` or `provide` array build concurrently and satisfy none of each other's requirements, a dependency between them is a later stage
- Swappable dependencies stay in the requirements of the layer that reads them, each root provides the implementation its host supplies
- References stay out of `R` and the compiler checks no override, so `Env` sits in the last stage, reaching every layer the graph builds
- Configuration choosing an implementation is `Layer.unwrap` over a table of layers, its key decoded through `Config.schema(Schema.Literals(Struct.keys(table)), path)`
- `Layer.fresh` gives each consumer its own instance of one layer

## [05]-[RUNTIME]

Hosts run the composed program once at their entry point, and each host's lifecycle selects the runtime call:

| [INDEX] | [HOST]                              | [ENTRY]                                                               |
| :-----: | :---------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | Process the program owns            | `NodeRuntime.runMain` at the composition root                         |
|  [02]   | Host calling effects on its events  | `ManagedRuntime.make(Browser)`, one `runPromise` per event            |
|  [03]   | React tree reading effects as state | `Atom.runtime(Browser)` from `effect/reactivity`, its `atom` and `fn` |

```ts
const runtime = ManagedRuntime.make(Browser);

export const onSubmit = (lines: ReadonlyArray<Line>, signal: AbortSignal) => runtime.runPromise(Pricing.use((pricing) => pricing.quote(lines)), { signal });
export const onUnload = () => runtime.dispose();
```

- Host events pass their `AbortSignal` as `signal`, so a cancelled event interrupts its run
- `ManagedRuntime` builds its layer on the first run and every later run reads the same services, a failed build stays cached
- One runtime serves one host process, created beside the host's entry point and disposed in its teardown hook
- `dispose` releases every resource the layer acquired, and later runs fail with a defect
- `await using` binds a runtime to a block, its `Symbol.asyncDispose` disposes it at block exit
