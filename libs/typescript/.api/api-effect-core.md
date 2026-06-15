# [API_CATALOGUE] effect-core

Grounded from installed `node_modules` type declarations. Covers the surfaces the TS planning pages consume — not an exhaustive module dump. Package set: `effect` (core), `@effect/platform`, `@effect/sql`, `@effect/sql-pg`, `@effect/rpc`, `@effect/cluster`, `@effect/workflow`.

---

## [1] — effect (core)

### Effect

```ts
// effect/Effect
interface Effect<out A, out E = never, out R = never>
  extends Effect.Variance<A, E, R>, Pipeable

declare const EffectTypeId: unique symbol
```

The primary computation carrier. `A` = success type, `E` = error type, `R` = required context (services). Implements `Symbol.iterator` for generator-style do-notation.

Key combinators (selected for planning pages):

```ts
// execution at the app boundary
declare const runFork: <A, E>(self: Effect<A, E, never>, options?: RunForkOptions) => RuntimeFiber<A, E>
declare const runPromise: <A, E>(self: Effect<A, E, never>, options?: RunCallbackOptions<A, E>) => Promise<A>

// context
declare const succeed: <A>(value: A) => Effect<A>
declare const fail: <E>(error: E) => Effect<never, E>
declare const service: <T extends Tag<any, any>>(tag: T) => Effect<Tag.Service<T>, never, Tag.Identifier<T>>
declare const provide: <RIn, E, ROut>(layer: Layer<ROut, E, RIn>) => <A, E1, R>(self: Effect<A, E1, R>) => Effect<A, E | E1, RIn | Exclude<R, ROut>>

// concurrency
declare const all: <const Arg extends ..., O extends Options>(arg: Arg, options?: O) => Effect<...>
declare const forEach: <A, B, E, R>(iterable: Iterable<A>, f: (a: A, i: number) => Effect<B, E, R>, options?: {concurrency?: Concurrency}) => Effect<Array<B>, E, R>

// stream conversion
declare const stream: // not exported directly; Stream.fromEffect / Stream.fromPubSub / Stream.fromQueue are in Stream module
```

### Layer

```ts
// effect/Layer
interface Layer<in ROut, out E = never, out RIn = never>
  extends Layer.Variance<ROut, E, RIn>, Pipeable

// type-level extractors
namespace Layer {
  type Context<T extends Any>  // RIn
  type Error<T extends Any>    // E
  type Success<T extends Any>  // ROut
}

// combinators used in CompositionRoot
declare const provide: <RIn2, E2, ROut2>(self: Layer<RIn2, E2, ROut2>) => <RIn, E, ROut>(layer: Layer<ROut, E, RIn>) => Layer<ROut | ROut2, E | E2, Exclude<RIn, ROut2> | RIn2>
declare const merge: <RIn2, E2, ROut2>(self: Layer<RIn2, E2, ROut2>) => <RIn, E, ROut>(layer: Layer<ROut, E, RIn>) => Layer<ROut | ROut2, E | E2, RIn | RIn2>
declare const scopedDiscard: <E, R>(effect: Effect<unknown, E, R>) => Layer<never, E, Exclude<R, Scope>>
declare const fromEffect: <T, E, R>(tag: Context.Tag<T, T>, effect: Effect<T, E, R>) => Layer<T, E, R>
```

`Layer<ROut, E, RIn>` describes how to build services of type `ROut` given dependencies `RIn`. Layers compose as a DAG; `CompositionRoot` holds the single assembled graph.

### Context / Tag

```ts
// effect/Context
interface Tag<in out Id, in out Value>
  extends Pipeable, Inspectable, ReadonlyTag<Id, Value>

// class-style Tag (used by Effect.Service pattern)
interface TagClass<Self, Id extends string, Type>
  extends Tag<Self, Type>

// bag of services keyed by Tag
interface Context<in Services>

// constructors
declare const make: <I, S>(tag: Tag<I, S>, service: Types.NoInfer<S>) => Context<I>
declare const add: <I, S>(context: Context<I>, tag: Tag<I2, S2>, service: S2) => Context<I | I2>
declare const get: <Services, T extends ValidTagsById<Services>>(context: Context<Services>, tag: T) => Tag.Service<T>
declare const GenericTag: <Identifier, Service = Identifier>(key: string) => Tag<Identifier, Service>
```

`Effect.Service` helper (used as the pattern for all five closed app-service owners):

```ts
// declared inline in Effect module: yields a TagClass with a Layer constructor
// Usage:
class MyService extends Effect.Service<MyService>()("MyService", {
  effect: Effect.gen(function*() { ... })
}) {}
// produces: class MyService extends Context.TagClass with a static Layer property
```

### Runtime

```ts
// effect/Runtime
interface Runtime<in R> extends Pipeable {
  readonly context: Context.Context<R>
  readonly runtimeFlags: RuntimeFlags.RuntimeFlags
  readonly fiberRefs: FiberRefs.FiberRefs
}

// execution entry points
declare const runFork: <R>(runtime: Runtime<R>) => <A, E>(effect: Effect<A, E, R>, options?: RunForkOptions) => RuntimeFiber<A, E>
declare const runPromise: <R>(runtime: Runtime<R>) => <A, E>(effect: Effect<A, E, R>) => Promise<A>
declare const runSync: <R>(runtime: Runtime<R>) => <A>(effect: Effect<A, never, R>) => A
```

### ManagedRuntime

```ts
// effect/ManagedRuntime
interface ManagedRuntime<in R, out ER> extends Effect.Effect<Runtime.Runtime<R>, ER> {
  readonly memoMap: Layer.MemoMap
  readonly runtimeEffect: Effect.Effect<Runtime.Runtime<R>, ER>
  readonly runtime: () => Promise<Runtime.Runtime<R>>
  readonly runFork: <A, E>(self: Effect<A, E, R>, options?: RunForkOptions) => RuntimeFiber<A, E | ER>
  readonly runPromise: <A, E>(self: Effect<A, E, R>) => Promise<A>
  readonly runSync: <A>(self: Effect<A, never, R>) => A
  readonly dispose: () => Promise<void>
  readonly disposeEffect: Effect<void, ER>
}

declare const make: <R, E>(layer: Layer<R, E, never>, memoMap?: Layer.MemoMap) => ManagedRuntime<R, E>
```

`ManagedRuntime` is the SPA root handle — `CompositionRoot` constructs one from the assembled Layer graph.

### Schema

```ts
// effect/Schema
interface Schema<in out A, in out I = A, out R = never>
  extends Schema.Variance<A, I, R>, Pipeable {
  readonly Type: A      // decoded type
  readonly Encoded: I   // wire type
  readonly Context: R   // service requirements
  readonly ast: AST.AST
}

// decode / encode (effectful)
declare const decodeUnknown: <A, I, R>(schema: Schema<A, I, R>, options?: ParseOptions)
  => (u: unknown, overrideOptions?: ParseOptions) => Effect<A, ParseResult.ParseError, R>

declare const decodeUnknownEither: <A, I>(schema: Schema<A, I, never>, options?: ParseOptions)
  => (u: unknown) => Either<A, ParseResult.ParseError>

// class-based domain shapes
declare const Class: <Self = never>(identifier: string)
  => <Fields extends Struct.Fields>(fieldsOr: Fields | HasFields<Fields>, annotations?: ...)
  => Class<Self, Fields, Struct.Encoded<Fields>, Struct.Context<Fields>, ...>

declare const TaggedClass: <Self = never>(identifier?: string)
  => <Tag extends string, Fields extends Struct.Fields>(tag: Tag, fieldsOr: Fields | HasFields<Fields>, ...)
  => TaggedClass<Self, Tag, ...>

declare const TaggedError: <Self = never>(identifier?: string)
  => <Tag extends string, Fields extends Struct.Fields>(tag: Tag, fieldsOr: Fields | HasFields<Fields>, ...)
  => TaggedErrorClass<Self, Tag, Fields>

// vocabulary primitives used in wire rails and stores
declare function Literal<Literals extends NonEmptyReadonlyArray<AST.LiteralValue>>(...literals: Literals): Literal<Literals>
declare const Struct: <Fields extends Struct.Fields>(fields: Fields) => Struct<Fields>
declare const Union: <Members extends readonly Schema.All[]>(...members: Members) => Union<Members>
declare const Array: <Value extends Schema.Any>(value: Value) => Array<Value>
declare const Record: <Key extends Schema.AnyNoContext & (string | number), Value extends Schema.Any>(key: Key, value: Value) => Record<Key, Value>
declare const Optional: <S extends Schema.All>(self: S) => optionalWith<S, {exact: true}>

// parse utilities
declare const decodeUnknownPromise: <A, I>(schema: Schema<A, I, never>, options?: ParseOptions)
  => (u: unknown) => Promise<A>
```

`Schema.Class` is the one domain-shape pattern; projections derive from it, never parallel structs. `Schema.TaggedError` is the one error-rail pattern, one family per rail. Decode entry is always `Schema.decodeUnknown*` at the wire edge.

### Stream

```ts
// effect/Stream
interface Stream<out A, out E = never, out R = never>
  extends Stream.Variance<A, E, R>, Pipeable

// construction (used by server-stream clients and fold owners)
declare const fromEffect: <A, E, R>(effect: Effect<A, E, R>) => Stream<A, E, R>
declare const async: <A, E = never, R = never>(register: (emit: Emit<A, E, R, void>) => Effect<void, E, R> | void) => Stream<A, E, R>
declare const never: Stream<never>
declare const empty: Stream<never>
declare const fail: <E>(error: E) => Stream<never, E>

// fold / consume (used in key-discriminated store folds)
declare const runFold: <S, A>(s: S, f: (s: S, a: A) => S) => <E, R>(self: Stream<A, E, R>) => Effect<S, E, R>
declare const runForEach: <A, X, E2, R2>(f: (a: A) => Effect<X, E2, R2>) => <E, R>(self: Stream<A, E, R>) => Effect<void, E | E2, R | R2>
declare const tap: <A, X, E2, R2>(f: (a: A) => Effect<X, E2, R2>) => <E, R>(self: Stream<A, E, R>) => Stream<A, E | E2, R | R2>
declare const mapEffect: <A, B, E2, R2>(f: (a: A) => Effect<B, E2, R2>, options?: ...) => <E, R>(self: Stream<A, E, R>) => Stream<B, E | E2, R | R2>

// retry / interruption (used in staleness-forward posture)
declare const retry: <E, R2>(schedule: Schedule.Schedule<any, E, R2>) => <A, R>(self: Stream<A, E, R>) => Stream<A, E, R | R2>
```

`Stream<A, E, R>` carries connect-es server-streams; the fold pattern is `Stream.runFold` into an immutable keyed map per store owner.

### Config

```ts
// effect/Config
interface Config<out A> extends Config.Variance<A>, Effect.Effect<A, ConfigError.ConfigError>

// primitives
declare const string: (name?: string) => Config<string>
declare const number: (name?: string) => Config<number>
declare const boolean: (name?: string) => Config<boolean>
declare const secret: (name?: string) => Config<Secret.Secret>
declare const redacted: (config?: Config<string>) => Config<Redacted.Redacted>
declare const nested: <A>(config: Config<A>, name: string) => Config<A>
declare const withDefault: <A>(config: Config<A>, def: Types.NoInfer<A>) => Config<A>
declare const map: <A, B>(config: Config<A>, f: (a: A) => B) => Config<B>
declare const all: <const Arg extends ..., O extends Options>(arg: Arg, options?: O) => Config<...>
```

Config is the domain-value pattern for `SecretResolver` — one domain value at the composition root, never scattered flag reads.

### Metrics

```ts
// effect/Metric
interface Metric<in out Type, in In, out Out>
  extends Metric.Variance<Type, In, Out>, Pipeable

namespace Metric {
  interface Counter<In extends number | bigint>
    extends Metric<MetricKeyType.Counter<In>, In, MetricState.Counter<In>>
  interface Gauge<In extends number | bigint>
    extends Metric<MetricKeyType.Gauge<In>, In, MetricState.Gauge<In>>
  interface Histogram<In extends number | bigint>
    extends Metric<MetricKeyType.Histogram<In>, In, MetricState.Histogram<In>>
  interface Summary<In extends number | bigint>
    extends Metric<MetricKeyType.Summary<In>, In, MetricState.Summary<In>>
  interface Frequency<In extends string>
    extends Metric<MetricKeyType.Frequency<In>, In, MetricState.Frequency<In>>
}

// constructors
declare const counter: <In extends number | bigint = number>(name: string, options?: ...) => Metric.Counter<In>
declare const gauge: <In extends number | bigint = number>(name: string, options?: ...) => Metric.Gauge<In>
declare const histogram: (name: string, boundaries: MetricBoundaries.MetricBoundaries, options?: ...) => Metric.Histogram<number>
declare const summary: (options: ...) => Metric.Summary<number>
declare const frequency: (name: string, options?: ...) => Metric.Frequency<string>

// application (callable as Effect wrapper)
// metric(effect) => Effect that records input/output pair
```

Consumed by `SelfTelemetry` — the five core metric primitives cover the host span and metric emission surface.

### Scope

```ts
// effect/Scope
interface Scope extends Pipeable {
  readonly [ScopeTypeId]: ScopeTypeId
  readonly strategy: ExecutionStrategy.ExecutionStrategy
}

interface CloseableScope extends Scope, Pipeable {
  readonly [CloseableScopeTypeId]: CloseableScopeTypeId
}

declare const Scope: Context.Tag<Scope, Scope>

// finalizer management
declare const addFinalizer: (self: Scope, finalizer: Effect<void>) => Effect<void>
declare const close: (self: CloseableScope, exit: Exit.Exit<unknown, unknown>) => Effect<void>
declare const make: (executionStrategy?: ExecutionStrategy.ExecutionStrategy) => Effect<CloseableScope>
```

### Schedule

```ts
// effect/Schedule
interface Schedule<out Out, in In = unknown, out R = never>
  extends Schedule.Variance<Out, In, R>, Pipeable

// retry-policy constructors (used in staleness-forward posture)
declare const exponential: (base: DurationInput, factor?: number) => Schedule<Duration>
declare const recurs: (n: number) => Schedule<number>
declare const union: <Out2, In2, R2>(other: Schedule<Out2, In2, R2>) => <Out, In, R>(self: Schedule<Out, In, R>) => Schedule<Out | Out2, In & In2, R | R2>
declare const addDelay: <Out, In, R>(f: (out: Out) => DurationInput) => (self: Schedule<Out, In, R>) => Schedule<Out, In, R>
declare const jittered: <Out, In, R>(self: Schedule<Out, In, R>) => Schedule<Out, In, R | Random>
declare const whileOutput: <Out>(f: Predicate<NoInfer<Out>>) => <In, R>(self: Schedule<Out, In, R>) => Schedule<Out, In, R>
```

`Schedule` is the typed retry-policy value in `STALENESS_AND_AVAILABILITY` — stream interruption folds to a `Schedule` value, not an improvised loop.

### Ref / SubscriptionRef

```ts
// effect/Ref
interface Ref<in out A> extends Ref.Variance<A>, Effect.Effect<A>, Readable.Readable<A> {
  modify<B>(f: (a: A) => readonly [B, A]): Effect<B>
}

// effect/SubscriptionRef
interface SubscriptionRef<in out A>
  extends SubscriptionRef.Variance<A>, SynchronizedRef.SynchronizedRef<A>, Subscribable<A> {
  readonly changes: Stream.Stream<A>  // the cell-bridge used by atom layer
}

declare const make: <A>(value: A) => Effect<SubscriptionRef<A>>
```

`SubscriptionRef.changes` is the bridge the atom layer exposes as a subscribable cell from each store fold.

### Data

```ts
// effect/Data
// structural equality constructors
declare const struct: <A extends Record<string, any>>(a: A) => Readonly<A>
declare const tagged: <A extends {readonly _tag: string}>(tag: A["_tag"]) => Case.Constructor<A, "_tag">

declare const Class: new <A extends Record<string, any> = {}>(
  args: Types.VoidIfEmpty<{readonly [P in keyof A]: A[P]}>
) => Readonly<A>

declare const TaggedClass: (tag: string) => <A extends {}>(args: ...) => ...
```

Used for typed-equal value objects inside store folds and command-gateway payloads.

---

## [2] — @effect/platform

### HttpClient

```ts
// @effect/platform/HttpClient
interface HttpClient extends HttpClient.With<HttpClientError>

namespace HttpClient {
  interface With<E, R = never> extends Pipeable, Inspectable {
    readonly [TypeId]: TypeId
    readonly execute: (request: HttpClientRequest) => Effect<HttpClientResponse, E, R>
    readonly get: (url: string | URL, options?: Options.NoBody) => Effect<HttpClientResponse, E, R>
    readonly post: (url: string | URL, options?: Options.NoUrl) => Effect<HttpClientResponse, E, R>
    readonly put: (url: string | URL, options?: Options.NoUrl) => Effect<HttpClientResponse, E, R>
    readonly del: (url: string | URL, options?: Options.NoUrl) => Effect<HttpClientResponse, E, R>
    readonly patch: (url: string | URL, options?: Options.NoUrl) => Effect<HttpClientResponse, E, R>
  }
}

// Context.Tag — used as the platform dependency for HttpRunner (cluster) and InternalRpc
declare const HttpClient: Context.Tag<HttpClient, HttpClient>

// FetchHttpClient layer — the browser binding (BrowserPlatform)
// @effect/platform/FetchHttpClient
declare const layer: Layer<HttpClient>
```

### HttpClientRequest

```ts
// @effect/platform/HttpClientRequest
interface HttpClientRequest extends Inspectable, Pipeable {
  readonly [TypeId]: TypeId
  readonly method: HttpMethod
  readonly url: string
  readonly urlParams: UrlParams.UrlParams
  readonly hash: Option<string>
  readonly headers: Headers.Headers
  readonly body: HttpBody.HttpBody
}
```

### KeyValueStore

```ts
// @effect/platform/KeyValueStore
interface KeyValueStore {
  readonly [TypeId]: TypeId
  readonly get: (key: string) => Effect<Option<string>, PlatformError>
  readonly getUint8Array: (key: string) => Effect<Option<Uint8Array>, PlatformError>
  readonly set: (key: string, value: string | Uint8Array) => Effect<void, PlatformError>
  readonly remove: (key: string) => Effect<void, PlatformError>
  readonly clear: Effect<void, PlatformError>
  readonly size: Effect<number, PlatformError>
  readonly modify: (key: string, f: (value: string) => string) => Effect<Option<string>, PlatformError>
}

declare const KeyValueStore: Context.Tag<KeyValueStore, KeyValueStore>
```

`KeyValueStore` is the platform key-value surface `BrowserPlatform` binds.

### Worker / WorkerPool

```ts
// @effect/platform/Worker
interface PlatformWorker {
  readonly [PlatformWorkerTypeId]: PlatformWorkerTypeId
  readonly spawn: <I, O>(id: number) => Effect<BackingWorker<I, O>, WorkerError, Spawner>
}

interface Worker<I, O, E = never> {
  readonly id: number
  // schema-typed bidirectional messaging over the backing worker
}

interface WorkerPool<I, O, E = never> {
  // pool of Workers, used by DecodeWorkerPool
}

// BackingWorker
interface BackingWorker<I, O> {
  readonly send: (message: I, transfers?: ReadonlyArray<unknown>) => Effect<void, WorkerError>
  readonly run: <A, E, R>(handler: (_: BackingWorker.Message<O>) => Effect<A, E, R>) => Effect<never, E | WorkerError, R>
}

declare const PlatformWorker: Context.Tag<PlatformWorker, PlatformWorker>
```

`PlatformWorker` + `WorkerPool` is the `DecodeWorkerPool` backing surface from `runtime-host.md`.

---

## [3] — @effect/sql

### SqlClient

```ts
// @effect/sql/SqlClient
interface SqlClient extends Constructor {
  readonly [TypeId]: TypeId
  readonly safe: this
  readonly withoutTransforms: () => this
  readonly reserve: Effect<Connection, SqlError, Scope>
  readonly withTransaction: <R, E, A>(self: Effect<A, E, R>) => Effect<A, E | SqlError, R>
  readonly reactive: <A, E, R>(
    keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>,
    effect: Effect<A, E, R>
  ) => Stream<A, E, R>
}

declare const SqlClient: Context.Tag<SqlClient, SqlClient>
```

`SqlClient` is the abstract SQL surface; `@effect/sql-pg/PgClient` implements it for Postgres.

### SqlError

```ts
// @effect/sql/SqlError
interface SqlError extends Data.TaggedError<"SqlError"> {
  readonly message: string
}
```

---

## [4] — @effect/sql-pg

### PgClient

```ts
// @effect/sql-pg/PgClient
interface PgClient extends SqlClient.SqlClient {
  readonly [TypeId]: TypeId
  readonly config: PgClientConfig
  readonly json: (_: unknown) => Fragment
  readonly listen: (channel: string) => Stream<string, SqlError>
  readonly notify: (channel: string, payload: string) => Effect<void, SqlError>
}

declare const PgClient: Context.Tag<PgClient, PgClient>

interface PgClientConfig {
  readonly url?: Redacted.Redacted
  readonly host?: string
  readonly port?: number
  readonly database?: string
  readonly username?: string
  readonly password?: Redacted.Redacted
  readonly ssl?: boolean | ConnectionOptions
  readonly maxConnections?: number
  readonly minConnections?: number
  readonly idleTimeout?: Duration.DurationInput
  readonly connectTimeout?: Duration.DurationInput
  readonly applicationName?: string
  readonly transformResultNames?: (str: string) => string
  readonly transformQueryNames?: (str: string) => string
  readonly transformJson?: boolean
}

declare const make: (options: PgClientConfig) => Effect<PgClient, SqlError, Scope | Reactivity.Reactivity>
declare const layer: (config: Config.Config<PgClientConfig>) => Layer<PgClient, ConfigError | SqlError>
declare const layerConfig: (config: Config.Config<PgClientConfig>) => Layer<PgClient, ConfigError | SqlError>
```

Consumed by `ClusterEngine` / `DURABLE_WORK_AND_RPC` for the SQL persistence adapter backing the workflow engine.

---

## [5] — @effect/rpc

### Rpc

```ts
// @effect/rpc/Rpc
interface Rpc<
  in out Tag extends string,
  out Payload extends AnySchema = typeof Schema.Void,
  out Success extends Schema.Schema.Any = typeof Schema.Void,
  out Error extends Schema.Schema.All = typeof Schema.Never,
  out Middleware extends RpcMiddleware.TagClassAny = never
> extends Pipeable {
  readonly [TypeId]: TypeId
  readonly _tag: Tag
  readonly key: string
  readonly payloadSchema: Payload
  readonly successSchema: Success
  readonly errorSchema: Error
  setSuccess<S extends Schema.Schema.Any>(schema: S): Rpc<Tag, Payload, S, Error, Middleware>
  setError<E extends Schema.Schema.Any>(schema: E): Rpc<Tag, Payload, Success, E, Middleware>
  setPayload<P extends Schema.Struct<any> | Schema.Struct.Fields>(schema: P): Rpc<...>
  middleware<M extends RpcMiddleware.TagClassAny>(middleware: M): Rpc<Tag, Payload, Success, Error, Middleware | M>
  prefix<const Prefix extends string>(prefix: Prefix): Rpc<`${Prefix}${Tag}`, Payload, Success, Error, Middleware>
}

// constructor
declare const make: <const Tag extends string>(tag: Tag, options?: {
  readonly payload?: Schema.Struct.Fields | Schema.Struct<Schema.Struct.Fields>
  readonly primaryKey?: (payload: ...) => string
  readonly success?: Schema.Schema.Any
  readonly error?: Schema.Schema.Any
}) => Rpc<Tag, ...>
```

`Rpc` is the procedure definition atom; `RpcGroup` aggregates them. Used in `DURABLE_WORK_AND_RPC` for `InternalRpc`.

### RpcGroup

```ts
// @effect/rpc/RpcGroup
interface RpcGroup<in out R extends Rpc.Any> extends Pipeable {
  readonly [TypeId]: TypeId
  readonly requests: ReadonlyMap<string, R>
  add<const Rpcs2 extends ReadonlyArray<Rpc.Any>>(...rpcs: Rpcs2): RpcGroup<R | Rpcs2[number]>
  merge<const Groups extends ReadonlyArray<Any>>(...groups: Groups): RpcGroup<R | Rpcs<Groups[number]>>
  middleware<M extends RpcMiddleware.TagClassAny>(middleware: M): RpcGroup<Rpc.AddMiddleware<R, M>>
  prefix<const Prefix extends string>(prefix: Prefix): RpcGroup<Rpc.Prefixed<R, Prefix>>
  toHandlersContext<Handlers extends HandlersFrom<R>, EX, RX>(
    build: Handlers | Effect<Handlers, EX, RX>
  ): Effect<Context<Rpc.ToHandler<R>>, EX, RX | HandlersContext<R, Handlers>>
  toLayer<Handlers extends HandlersFrom<R>, EX, RX>(
    build: Handlers | Effect<Handlers, EX, RX>
  ): Layer<Rpc.ToHandler<R>, EX, Exclude<RX, Scope> | HandlersContext<R, Handlers>>
}
```

### RpcClient

```ts
// @effect/rpc/RpcClient
type RpcClient<Rpcs extends Rpc.Any, E = never> =
  RpcClient.From<RpcClient.NonPrefixed<Rpcs>, E, ""> &
  { readonly [Prefix in RpcClient.Prefixes<Rpcs>]: RpcClient.From<RpcClient.Prefixed<Rpcs, Prefix>, E, Prefix> }

// constructor
declare const make: <Rpcs extends Rpc.Any, const Group extends RpcGroup.RpcGroup<Rpcs>>(
  group: Group,
  options: { readonly transport: ... }
) => Effect<RpcClient<Rpcs>, never, ...>

// http layer constructor (used by InternalRpc over HttpClient)
declare const makeHttp: <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, options: {
  readonly url: string
  readonly headers?: Headers.Input
}) => Effect<RpcClient<Rpcs>, never, HttpClient>
```

---

## [6] — @effect/cluster

### Entity

```ts
// @effect/cluster/Entity
interface Entity<in out Type extends string, in out Rpcs extends Rpc.Any>
  extends Equal.Equal {
  readonly [TypeId]: TypeId
  readonly type: Type & Brand<"EntityType">
  readonly protocol: RpcGroup.RpcGroup<Rpcs>
  getShardGroup(entityId: EntityId): string
  getShardId(entityId: EntityId): Effect<ShardId, never, Sharding>
  annotateRpcs<I, S>(tag: Context.Tag<I, S>, value: S): Entity<Type, Rpcs>
  // ...
}

// constructor
declare const make: <const Type extends string, const Rpcs extends ReadonlyArray<Rpc.Any>>(
  entityType: Type,
  rpcs: Rpcs
) => Entity<Type, Rpcs[number]>
```

`Entity` is the durable-actor definition — `EntityType` string + `RpcGroup` protocol. Consumed in `DURABLE_WORK_AND_RPC` as `ActivityOwner`'s entity backing.

### Sharding

```ts
// @effect/cluster/Sharding  (Context.TagClass)
class Sharding extends Context.TagClass<Sharding, "@effect/cluster/Sharding", {
  readonly getRegistrationEvents: Stream<ShardingRegistrationEvent>
  readonly getShardId: (entityId: EntityId, group: string) => ShardId
  readonly hasShardId: (shardId: ShardId) => boolean
  readonly getSnowflake: Effect<Snowflake>
  readonly isShutdown: Effect<boolean>
  readonly makeClient: <Type extends string, Rpcs extends Rpc.Any>(
    entity: Entity<Type, Rpcs>
  ) => Effect<(entityId: string) => RpcClient.RpcClient.From<Rpcs, MailboxFull | AlreadyProcessingMessage | PersistenceError>>
  readonly registerEntity: <Type extends string, Rpcs extends Rpc.Any, Handlers, RX>(
    entity: Entity<Type, Rpcs>,
    handlers: Effect<Handlers, never, RX>,
    options?: { maxIdleTime?: DurationInput; concurrency?: number | "unbounded"; mailboxCapacity?: number | "unbounded" }
  ) => Effect<void, never, RX | ...>
}> {}
```

`Sharding` is the `ClusterEngine` / `RunnerBackplane` access point — `makeClient` returns the entity-keyed RpcClient; `registerEntity` installs handlers.

### EntityProxy / ClusterWorkflowEngine

```ts
// @effect/cluster/EntityProxy
declare const toRpcGroup: <Type extends string, Rpcs extends Rpc.Any>(
  entity: Entity<Type, Rpcs>
) => RpcGroup.RpcGroup<ConvertRpcs<Rpcs, Type>>

// @effect/cluster/ClusterWorkflowEngine
declare const layer: Layer<WorkflowEngine, never, Sharding | MessageStorage>
```

`ClusterWorkflowEngine.layer` wires `@effect/workflow/WorkflowEngine` onto the cluster's `Sharding + MessageStorage` — the entry point for `ClusterEngine` in `DURABLE_WORK_AND_RPC`.

---

## [7] — @effect/workflow

### Workflow

```ts
// @effect/workflow/Workflow
interface Workflow<
  Name extends string,
  Payload extends AnyStructSchema,
  Success extends Schema.Schema.Any,
  Error extends Schema.Schema.All
> {
  readonly [TypeId]: TypeId
  readonly name: Name
  readonly payloadSchema: Payload
  readonly successSchema: Success
  readonly errorSchema: Error
  readonly execute: <const Discard extends boolean = false>(
    payload: Schema.Simplify<Schema.Struct.Constructor<Payload["fields"]>>,
    options?: { readonly discard?: Discard }
  ) => Effect<Discard extends true ? string : Success["Type"], Error["Type"], WorkflowEngine | ...>
  annotate<I, S>(tag: Context.Tag<I, S>, value: S): Workflow<Name, Payload, Success, Error>
}

// constructor
declare const make: <const Name extends string, Payload extends Schema.Struct.Fields, Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never>(
  options: {
    readonly name: Name
    readonly payload: Payload
    readonly success?: Success
    readonly error?: Error
  }
) => Workflow<Name, Schema.Struct<Payload>, Success, Error>
```

`Workflow` is the resumable-unit definition: name + payload schema + success/error schemas. Consumed in `WorkflowOwner`.

### Activity

```ts
// @effect/workflow/Activity
interface Activity<
  Success extends Schema.Schema.Any = typeof Schema.Void,
  Error extends Schema.Schema.All = typeof Schema.Never,
  R = never
> extends Effect<Success["Type"], Error["Type"], Success["Context"] | Error["Context"] | R | WorkflowEngine | WorkflowInstance> {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly successSchema: Success
  readonly errorSchema: Error
  readonly exitSchema: Schema.Schema<Exit<Success["Type"], Error["Type"]>, ...>
  readonly execute: Effect<Success["Type"], Error["Type"], ... | WorkflowEngine | WorkflowInstance>
}

declare const make: <R, Success, Error>(options: {
  readonly name: string
  readonly success?: Success
  readonly error?: Error
  readonly execute: Effect<Success["Type"], Error["Type"], R>
  readonly interruptRetryPolicy?: Schedule<any, Cause<unknown>>
}) => Activity<Success, Error, Exclude<R, WorkflowInstance | WorkflowEngine | Scope>>
```

`Activity` is the run-once unit with retry and compensation finalizers. Consumed in `ActivityOwner`.

### WorkflowEngine

```ts
// @effect/workflow/WorkflowEngine  (Context.TagClass)
class WorkflowEngine extends Context.TagClass<WorkflowEngine, "@effect/workflow/WorkflowEngine", {
  readonly register: <Name, Payload, Success, Error, R>(
    workflow: Workflow<Name, Payload, Success, Error>,
    execute: (payload: Payload["Type"], executionId: string) => Effect<Success["Type"], Error["Type"], R>
  ) => Effect<void, never, Scope | Exclude<R, WorkflowEngine | WorkflowInstance | Workflow.Execution<Name> | Scope> | ...>
  readonly execute: <Name, Payload, Success, Error, const Discard extends boolean = false>(
    workflow: Workflow<Name, Payload, Success, Error>,
    options: { readonly executionId: string; readonly payload: Payload["Type"]; readonly discard?: Discard; readonly suspendedRetrySchedule?: Schedule<any, unknown> }
  ) => Effect<Discard extends true ? string : Success["Type"], Error["Type"], ...>
  readonly poll: <Name, Payload, Success, Error>(
    workflow: Workflow<Name, Payload, Success, Error>,
    executionId: string
  ) => Effect<Workflow.Result<Success["Type"], Error["Type"]> | undefined, never, ...>
  readonly interrupt: (workflow: Workflow.Any, executionId: string) => Effect<void>
  readonly resume: (workflow: Workflow.Any, executionId: string) => Effect<void>
  readonly activityExecute: <Success, Error, R>(activity: Activity<Success, Error, R>, attempt: number) => Effect<Workflow.Result<...>, never, ...>
  readonly deferredResult: <Success, Error>(deferred: DurableDeferred<Success, Error>) => Effect<Exit<Success["Type"], Error["Type"]> | undefined, never, WorkflowInstance>
  readonly deferredDone: <Success, Error>(deferred: DurableDeferred<Success, Error>, options: { workflowName: string; executionId: string; deferredName: string; exit: Exit<...> }) => Effect<void, never, ...>
}> {}
```

`WorkflowEngine` is the durable-execution kernel accessed by `Workflow.execute` and `Activity.make`. Provided by `ClusterWorkflowEngine.layer` in the cluster-backed case.

### DurableClock / DurableDeferred

```ts
// @effect/workflow/DurableClock
interface DurableClock {
  // a durable timer that pauses without resources and resumes on external signal
  readonly name: string
}

// @effect/workflow/DurableDeferred
interface DurableDeferred<Success extends Schema.Schema.Any, Error extends Schema.Schema.All> {
  readonly name: string
  readonly successSchema: Success
  readonly errorSchema: Error
}
```

Used inside workflow `execute` bodies for externally-signalled awaits without holding resources.
