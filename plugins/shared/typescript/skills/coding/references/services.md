# [SERVICES]

## [01]-[SERVICE_CLASSES]

Service classes are keys programs yield, and `make` states the implementation a static layer or a composition root builds:

```ts
class Shell extends Context.Service<Shell>()('@rasm/shell/shell', {
    make: Effect.fnUntraced(function* (facts: { readonly processor: Processor }) {
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const detect = (at: Parameters<typeof probe>[0]) => Effect.provideService(probe(at, facts.processor), ChildProcessSpawner.ChildProcessSpawner, spawner);
        return { dialect: Effect.fn('Shell.dialect')(detect) };
    }),
}) {}

const shells = Layer.effect(Shell, Effect.flatMap(request('Wasm', 'compile', './main.wasm', () => $.wasm.compile('./main.wasm')), (module) => Shell.make({ processor: getProcessor(module) })));
```

`probe` runs a shell binary to read its dialect, `request` is the root's `Effect.tryPromise` over an engine call, and `$` is the Claude Code engine handle a hook receives. `make` takes the parser the root compiles from the engine's WebAssembly and reads the spawner once, so `Shell.dialect` requires nothing:
- Shapes derive from `make`, members close over the services `make` read, and a member requiring a service adds it to every caller's `R`
- Classes state their shape as the second type argument where a root alone holds the host handle building it, the root building the layer
- Keys are package name and module path, followed by the class name where it differs from its file name (`'@rasm/policy/claude/platform/Session'`)
- `make` takes a parameter for each value a composition root or a `LayerMap` key supplies, and its static `layer` takes the same parameters
- Facts the composition fixes (a driver dialect, a host version) resolve in `make`, an unsupported one failing layer build in place of each call
- Programs bind a service through `yield*`, and a call reaching one member uses `<Class>.use` or `useSync`
- Members hold operations reading a resource, a service, or a host value, and computations over arguments stay functions or statics
- `Context.Reference` holds a value every fiber reads, its `defaultValue` from a host or standard (`{ defaultValue: process.cwd }`)
- `Layer.succeed(reference, value)` overrides a reference for a graph and `Effect.provideService` for one effect
- `Effect.updateService(key, f)` edits a service for one effect, a `flatMap` over the service into `provideService` of a spread restating it
- `Context.Reference` keys an annotation on a contract (`Tool`, `RpcGroup`), and `Context.get(x.annotations, Key)` reads it with its default
- Annotations differing from a shared set by one key derive through `Context.omit(Key)` or `Context.pick`, a `Context.make` chain restating them
- Registrations keeping a handler capture `yield* Effect.context<Exclude<R, Request>>()`, each call providing `Context.merge(captured, request)`

## [02]-[CONFIGURATION]

Settings decode once at layer build, and a missing or invalid key fails the build with `ConfigError` before any request runs. `Config<A>` is an `Effect<A, ConfigError>`, read wherever an effect goes:

```ts
const access = Config.all({
    endpoint: Config.String('AWS_ENDPOINT_URL_S3'),
    published: Config.option(Config.String('S3_PUBLIC_ENDPOINT_URL')),
    region: Config.String('AWS_REGION'),
    accessKeyId: Config.Redacted('AWS_ACCESS_KEY_ID'),
    secretAccessKey: Config.Redacted('AWS_SECRET_ACCESS_KEY'),
});
```

`ObjectStorage.make` yields `access` once under the variable names the AWS SDKs read. A bucket with no public endpoint reads `published` as `Option.none()`, and `Redacted.value` opens each key at the `AwsClient` constructor alone:
- Scalars take their typed constructor (`Config.Duration`, `Port`, `URL`, `ByteSize`, `LogLevel`), other values `Config.schema(S, path)`
- Separated lists in one variable (`a,b`) read through `Config.Array(S, path)`, `key=value` pairs through `Config.Record(K, V, path)`
- Literal settings reuse the set of the schema owning them (`Config.Literals(Unification.literals, path)`)
- `Config.option` reads an absent key as `Option.none()` and fails on an invalid value, where a required read fails the program
- `Config.withDefault` takes the default the setting's owner states, and `Config.orElse` reads an older key the owner still honors
- Libraries state a setting's path (`log.level`) and the root its spelling, a `String.constantCase(name) + '_LOG_LEVEL'` key restating the provider
- CLI flags fall back to the setting of their own name through `Flag.withFallbackConfig(Config.schema(S, name.split('-')))`
- Value services a setting supplies build through `Layer.effect(<Class>, config)`, and parameterized layers through `Layer.unwrap` over the config
- Implementations a setting selects are `Layer.unwrap` over one table of layers, keyed by a `Schema.Literals(Struct.keys(table))` setting
- Entry points yield a setting (`Config.String('HOME')`) in one `Effect.all` with their service keys, a `process.env` or `node:os` read restating it

`ConfigProvider.ConfigProvider` is a reference defaulting to `fromEnv()`, and a root replacing it states every source in lookup order:

```ts
const provider = ConfigProvider.fromEnv().pipe(
    ConfigProvider.nested(name),
    ConfigProvider.constantCase,
    ConfigProvider.orElse(ConfigProvider.fromUnknown(defaults).pipe(ConfigProvider.mapInput(flow(Array.span(Predicate.isString), ([keys, indices]) => [keys.join('.'), ...indices])))),
    ConfigProvider.orElse(ConfigProvider.fromEnv()),
);
```

`name` is the last segment of the app's bundle identifier, and `defaults` the record `defaults export` prints for it, keyed by dotted path. A path reads the environment under the app's name, then the macOS defaults domain, then the environment alone:
- `ConfigProvider.layer(provider)` replaces the provider in a root's last `Layer.provide`, `Effect.provideService` around one program
- `ConfigProvider.layerAdd(provider)` falls back to a source after the current one, `{ asPrimary: true }` reading it first
- Host records enter through `fromUnknown` (a Worker's `env` filtered to plain values), files through `fromDotEnv` and `fromDir` (one file per key)

## [03]-[SCOPED_RESOURCES]

Layers remove `Scope` from their requirements. Resource lifetime selects where a scope opens:

| [INDEX] | [LIFETIME]   | [FORM]                                                                    |
| :-----: | :----------- | :------------------------------------------------------------------------ |
|  [01]   | Program      | Acquired in `make`, released with layer                                   |
|  [02]   | One call     | `Effect.scoped` as a trailing argument of `Effect.fn`, released on return |
|  [03]   | First use    | `Effect.cached(Scope.provide(acquire, yield* Effect.scope))` in `make`    |
|  [04]   | Cleanup only | `Effect.addFinalizer` in scope owning state it restores                   |
|  [05]   | Until change | `ScopedRef.set(ref, acquire)` acquires next, then closes previous scope   |

`readTypeScript` starts a compiler process, and `Declarations.make` holds it through row 03: the first page read starts the process, later reads share it, and the layer scope stops it:
- Host handles implementing `Disposable` or `AsyncDisposable` enter through `Effect.acquireDisposable`, released by their own dispose
- Handles without a dispose method enter through `Effect.acquireRelease`, its release receiving the `Exit`
- Host clients with a package-published layer (`NodeHttpClient.layerUndici`) enter through the published layer
- Reads a service warms at build fork through `read.pipe(Effect.ignore, Effect.forkScoped)`, layer build continuing and the fork ending with the layer
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
const app = ApplicationTools.layer().pipe(Layer.provideMerge(Catalog.layer(Catalog.fromInstallations)), Layer.provideMerge(application));
```

`ApplicationTools.layer()` reads `Catalog` and `ApplicationServer`, the service `application` builds over the Swift helper process, and hides its handler and `InterfaceMap.Store` layers behind `Layer.provide`. `Catalog` reads `ApplicationServer`, and each `provideMerge` keeps its service readable to every layer above:
- Layers inside one `mergeAll` or `provide` array build concurrently and satisfy none of each other's requirements
- Swappable dependencies stay in their reading layer's requirements, and each root provides its host's implementation
- Layers memoize by reference within one build, a layer function's result binds to one `const`, `Layer.fresh` builds one per consumer
- `Effect.provide` reuses a layer an enclosing `provide` or runtime built, `{ local: true }` and separate `provide` calls each building their own
- `Layer.build(layer)` returns the built `Context`, `Context.get(built, <Class>)` handing a service to code outside the graph
- `Layer.tap(layer, (context) => ...)` reads a service as its layer finishes, completing a `Deferred` a sibling effect awaits
- Graphs a request value selects are one `LayerMap.Service` with a `lookup` mapping the key to a layer
- `Effect.provide(<Map>.get(key))` builds a key's graph on first use and releases it `idleTimeToLive` after its last user
- `LayerRef.make(layer, { idleTimeToLive })` holds one graph built on first use, `invalidate` dropping it for the next use to rebuild

## [05]-[RUNTIME]

Hosts run their composed graph at an entry point, and each host's lifecycle selects its runtime call:

| [INDEX] | [HOST]                              | [ENTRY]                                                      |
| :-----: | :---------------------------------- | :----------------------------------------------------------- |
|  [01]   | Process program owns                | `runMain({ teardown })` over `Layer.launch` or main effect   |
|  [02]   | Host calling effects on its events  | `ManagedRuntime.make(layer)`, one `runPromise` per event     |
|  [03]   | Host awaiting one typed callback    | `Effect.fnUntraced(body, Effect.provide(layer), runPromise)` |
|  [04]   | Platform calling a fetch handler    | `HttpRouter.toWebHandler(layer)` at module scope             |
|  [05]   | React tree reading effects as state | `Atom.runtime(layer)` from `effect/reactivity`               |

```ts
const teardown: Runtime.Teardown = (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit));
```

`runMain` from `@effect/platform-node/NodeRuntime` interrupts its program on `SIGINT` or `SIGTERM`, releasing every resource, and hands the `Exit` to `teardown`. Servers stopped by their supervisor exit 0 through this `teardown`, where `defaultTeardown` exits 130:
- `Fiber.interruptAs(fiber, undefined, Context.make(Runtime.InterruptExitCode, 0))` sets the code a program ending itself exits with
- `runMain` logs a failed cause unless its error holds `[Runtime.errorReported]: false`, which a root finalizer logging that cause sets
- Product entries pass app values to library operations and run them through `Effect.provide(layer)` and `runMain({ teardown })`
- Importable Node modules can own CLI startup with a top-level `if (import.meta.main)` around `runMain`
- Host events pass their `AbortSignal` as `signal`, and a canceled event interrupts its run
- Host thunks and synchronous callbacks run through `Effect.runPromiseWith` or `runSyncWith` over `yield* Effect.context<R>()`
- `ManagedRuntime` builds its layer on the first run, later runs read the same services, and a failed build stays cached
- Hosts retrying on their next event drop a runtime whose first run fails and dispose it, the next event building a new one
- Runtimes live as long as the module holding them, built at the first event handing over the host handle their layer reads
- Session ends that keep the module loaded (Claude Code `session.end` on clear or resume) end state through a service member and keep the runtime
- `dispose` releases every resource and interrupts every fiber the runtime started, and later runs die with `ManagedRuntime disposed`
- `await using` binds a runtime to a block, its `Symbol.asyncDispose` disposing it at block exit
- Hosts keeping no layer between calls build a module-scope memo, `Effect.runSync(Cache.makeWith(lookup, { requireServicesAt: 'lookup' }))`

### [05.1]-[HOST_PLATFORMS]

Hosts without Node (the Claude Code engine) supply platform services from their own calls, and libraries reading `FileSystem`, `ChildProcessSpawner`, and `Path` run unchanged:

```ts
const platform = Layer.mergeAll(
    Layer.succeed(FileSystem.FileSystem, files($)),
    Layer.succeed(ChildProcessSpawner.ChildProcessSpawner, spawner((asked) => $.process.spawn(asked))),
    Path.layer,
    Layer.succeed(Scheduler.Scheduler, new TimeSliced()),
);
```

`files` passes `FileSystem.make(FileSystem.makeNoop({...}))` the members a library reads over `$.fs`, `spawner` passes `ChildProcessSpawner.make` a `spawn` over `$.process.spawn` returning `ChildProcessSpawner.makeHandle`, and `TimeSliced` yields the engine's worker on a clock:
- `Path.make({ ...base, resolve })` anchors relative paths at a folder the host reports (an agent's working directory)
- Use `typescript:effect` for the members each platform constructor derives and the errors an engine call maps to
