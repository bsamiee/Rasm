# [API_CATALOGUE] @effect/cluster

Dependency catalogue for `@effect/cluster` (v0.59.0), the node-tier distributed sharding, entity, durable-workflow, and runner-backplane surface. Grounded from the installed `node_modules/@effect/cluster/dist/dts/*.d.ts` declarations. The package is a `node`-tagged satellite that never enters the browser bundle. Owner-symbol consumers: `ClusterEngine` (`durable-execution/engine#ENGINE` — layers `ClusterWorkflowEngine` over `Sharding`); `RunnerBackplane` + `ScheduledWork` (`runtime-backplane/backplane#RUNNER_AND_SCHEDULING` — protocol/message-storage/runner-storage/runner-health backplane, snowflake id source, cluster singletons, durable cron); `SqlBoundary` (`persistence/store-boundary#STORE_BOUNDARY` — the one `SqlClient` surface `SqlMessageStorage` + `SqlRunnerStorage` ride).

Every namespace is a barrel re-export from `index.d.ts` (`export * as <Namespace> from "./<Module>.js"`); a symbol is reached as `Cluster.<Namespace>.<symbol>` or via deep import `@effect/cluster/<Module>`. Signatures are transcription-complete.

---

## [Entity]

The entity vocabulary: a typed `RpcGroup`-backed addressable actor with a behavior-handler layer factory, client constructor, and per-message `Request`/`Replier` plumbing.

### TypeId / Entity / Any / HandlersFrom

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export interface Entity<in out Type extends string, in out Rpcs extends Rpc.Any> extends Equal.Equal {
  readonly [TypeId]: TypeId;
  readonly type: Type & Brand<"EntityType">;
  readonly protocol: RpcGroup.RpcGroup<Rpcs>;
  getShardGroup(entityId: EntityId): string;
  getShardId(entityId: EntityId): Effect.Effect<ShardId.ShardId, never, Sharding>;
  annotate<I, S>(tag: Context.Tag<I, S>, value: S): Entity<Type, Rpcs>;
  annotateRpcs<I, S>(tag: Context.Tag<I, S>, value: S): Entity<Type, Rpcs>;
  annotateContext<S>(context: Context.Context<S>): Entity<Type, Rpcs>;
  annotateRpcsContext<S>(context: Context.Context<S>): Entity<Type, Rpcs>;
  readonly client: Effect.Effect<(entityId: string) => RpcClient.RpcClient.From<Rpcs, MailboxFull | AlreadyProcessingMessage | PersistenceError>, never, Sharding>;
  toLayer<Handlers extends HandlersFrom<Rpcs>, RX = never>(build: Handlers | Effect.Effect<Handlers, never, RX>, options?: {
    readonly maxIdleTime?: DurationInput | undefined;
    readonly concurrency?: number | "unbounded" | undefined;
    readonly mailboxCapacity?: number | "unbounded" | undefined;
    readonly disableFatalDefects?: boolean | undefined;
    readonly defectRetryPolicy?: Schedule.Schedule<any, unknown> | undefined;
    readonly spanAttributes?: Record<string, string> | undefined;
  }): Layer.Layer<never, never, Exclude<RX, Scope | CurrentAddress | CurrentRunnerAddress> | RpcGroup.HandlersContext<Rpcs, Handlers> | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs> | Sharding>;
  of<Handlers extends HandlersFrom<Rpcs>>(handlers: Handlers): Handlers;
  toLayerMailbox<R, RX = never>(build: ((mailbox: Mailbox.ReadonlyMailbox<Envelope.Request<Rpcs>>, replier: Replier<Rpcs>) => Effect.Effect<never, never, R>) | Effect.Effect<(mailbox: Mailbox.ReadonlyMailbox<Envelope.Request<Rpcs>>, replier: Replier<Rpcs>) => Effect.Effect<never, never, R>, never, RX>, options?: {
    readonly maxIdleTime?: DurationInput | undefined;
    readonly mailboxCapacity?: number | "unbounded" | undefined;
    readonly disableFatalDefects?: boolean | undefined;
    readonly defectRetryPolicy?: Schedule.Schedule<any, unknown> | undefined;
    readonly spanAttributes?: Record<string, string> | undefined;
  }): Layer.Layer<never, never, Exclude<RX, Scope | CurrentAddress | CurrentRunnerAddress> | R | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs> | Sharding>;
}

export type Any = Entity<string, Rpc.Any>;

export type HandlersFrom<Rpc extends Rpc.Any> = {
  readonly [Current in Rpc as Current["_tag"]]: (envelope: Request<Current>) => Rpc.ResultFrom<Current, any> | Rpc.Wrapper<Rpc.ResultFrom<Current, any>>;
};
```

`type` is brand-tagged `EntityType`; the protocol is an `@effect/rpc` `RpcGroup`. `toLayer` is the production registration path (entity handlers as a `Layer` requiring `Sharding`); `toLayerMailbox` is the manual-mailbox variant; `client` builds an entity-keyed `RpcClient` factory.

### Constructors and refinement

```ts
export declare const isEntity: (u: unknown) => u is Any;

export declare const fromRpcGroup: <const Type extends string, Rpcs extends Rpc.Any>(
  type: Type,
  protocol: RpcGroup.RpcGroup<Rpcs>
) => Entity<Type, Rpcs>;

export declare const make: <const Type extends string, Rpcs extends ReadonlyArray<Rpc.Any>>(
  type: Type,
  protocol: Rpcs
) => Entity<Type, Rpcs[number]>;
```

`make` accepts an array of `Rpc.make(...)` definitions; `fromRpcGroup` accepts a pre-built `RpcGroup`.

### Context tags / keep-alive

```ts
export declare class CurrentAddress extends Context.TagClass<CurrentAddress, "@effect/cluster/Entity/EntityAddress", EntityAddress>() {}
export declare class CurrentRunnerAddress extends Context.TagClass<CurrentRunnerAddress, "@effect/cluster/Entity/RunnerAddress", RunnerAddress>() {}

export declare const keepAlive: (enabled: boolean) => Effect.Effect<void, never, Sharding | CurrentAddress>;
export declare const KeepAliveRpc: Rpc.Rpc<"Cluster/Entity/keepAlive", typeof Schema.Void, typeof Schema.Void, typeof Schema.Never, never>;
export declare class KeepAliveLatch extends Context.TagClass<KeepAliveLatch, "effect/cluster/Entity/KeepAliveLatch", Effect.Latch>() {}
```

`CurrentAddress` / `CurrentRunnerAddress` are accessible from within a handler effect; both are `Exclude`d from the `toLayer` requirement set.

### Replier

```ts
export interface Replier<Rpcs extends Rpc.Any> {
  readonly succeed: <R extends Rpcs>(request: Envelope.Request<R>, value: Replier.Success<R>) => Effect.Effect<void>;
  readonly fail: <R extends Rpcs>(request: Envelope.Request<R>, error: Rpc.Error<R>) => Effect.Effect<void>;
  readonly failCause: <R extends Rpcs>(request: Envelope.Request<R>, cause: Cause.Cause<Rpc.Error<R>>) => Effect.Effect<void>;
  readonly complete: <R extends Rpcs>(request: Envelope.Request<R>, exit: Exit.Exit<Replier.Success<R>, Rpc.Error<R>>) => Effect.Effect<void>;
}

export declare namespace Replier {
  type Success<R extends Rpc.Any> = Rpc.Success<R> extends Stream.Stream<infer _A, infer _E, infer _R>
    ? Stream.Stream<_A, _E | Rpc.Error<R>, _R> | Mailbox.ReadonlyMailbox<_A, _E | Rpc.Error<R>>
    : Rpc.Success<R>;
}
```

### Request (per-message carrier)

```ts
export declare class Request<Rpc extends Rpc.Any> extends Data.Class<Envelope.Request<Rpc> & {
  readonly lastSentChunk: Option.Option<Reply.Chunk<Rpc>>;
}> {
  get lastSentChunkValue(): Option.Option<Rpc.SuccessChunk<Rpc>>;
  get nextSequence(): number;
}
```

### Test client

```ts
export declare const makeTestClient: <Type extends string, Rpcs extends Rpc.Any, LA, LE, LR>(
  entity: Entity<Type, Rpcs>,
  layer: Layer.Layer<LA, LE, LR>
) => Effect.Effect<(entityId: string) => Effect.Effect<RpcClient.RpcClient<Rpcs>>, LE, Scope | ShardingConfig | Exclude<LR, Sharding> | Rpc.MiddlewareClient<Rpcs>>;
```

---

## [Sharding]

The central runtime service: shard placement, snowflake minting, client construction, entity/singleton registration, message routing, and the lifecycle layer.

### Sharding service

```ts
export declare class Sharding extends Context.TagClass<Sharding, "@effect/cluster/Sharding", {
  readonly getRegistrationEvents: Stream.Stream<ShardingRegistrationEvent>;
  readonly getShardId: (entityId: EntityId, group: string) => ShardId;
  readonly hasShardId: (shardId: ShardId) => boolean;
  readonly getSnowflake: Effect.Effect<Snowflake.Snowflake>;
  readonly isShutdown: Effect.Effect<boolean>;
  readonly makeClient: <Type extends string, Rpcs extends Rpc.Any>(entity: Entity<Type, Rpcs>) => Effect.Effect<(entityId: string) => RpcClient.RpcClient.From<Rpcs, MailboxFull | AlreadyProcessingMessage | PersistenceError>>;
  readonly registerEntity: <Type extends string, Rpcs extends Rpc.Any, Handlers extends HandlersFrom<Rpcs>, RX>(entity: Entity<Type, Rpcs>, handlers: Effect.Effect<Handlers, never, RX>, options?: {
    readonly maxIdleTime?: DurationInput | undefined;
    readonly concurrency?: number | "unbounded" | undefined;
    readonly mailboxCapacity?: number | "unbounded" | undefined;
    readonly disableFatalDefects?: boolean | undefined;
    readonly defectRetryPolicy?: Schedule.Schedule<any, unknown> | undefined;
    readonly spanAttributes?: Record<string, string> | undefined;
  }) => Effect.Effect<void, never, Scope.Scope | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs> | Exclude<RX, Scope.Scope | CurrentAddress | CurrentRunnerAddress>>;
  readonly registerSingleton: <E, R>(name: string, run: Effect.Effect<void, E, R>, options?: {
    readonly shardGroup?: string | undefined;
  }) => Effect.Effect<void, never, R | Scope.Scope>;
  readonly send: (message: Message.Incoming<any>) => Effect.Effect<void, EntityNotAssignedToRunner | MailboxFull | AlreadyProcessingMessage>;
  readonly sendOutgoing: (message: Message.Outgoing<any>, discard: boolean) => Effect.Effect<void, MailboxFull | AlreadyProcessingMessage | PersistenceError>;
  readonly notify: (message: Message.Incoming<any>, options?: {
    readonly waitUntilRead?: boolean | undefined;
  }) => Effect.Effect<void, EntityNotAssignedToRunner | AlreadyProcessingMessage>;
  readonly reset: (requestId: Snowflake.Snowflake) => Effect.Effect<boolean>;
  readonly pollStorage: Effect.Effect<void>;
  readonly activeEntityCount: Effect.Effect<number>;
}>() {}

export declare const layer: Layer.Layer<Sharding, never, ShardingConfig | Runners | MessageStorage.MessageStorage | RunnerStorage | RunnerHealth.RunnerHealth>;
```

`Sharding.layer` is the assembly point requiring the five backplane services; the `HttpRunner` / `SocketRunner` / `SingleRunner` / `TestRunner` layers wire it as a unit so the backplane is rarely composed by hand.

---

## [ShardingConfig]

The runner configuration record: addresses, shard weights/groups, lock intervals, entity timeouts, poll intervals. Defaults and env-driven layers.

### ShardingConfig service + Type fields

```ts
export declare class ShardingConfig extends Context.TagClass<ShardingConfig, "@effect/cluster/ShardingConfig", {
  readonly runnerAddress: Option.Option<RunnerAddress>;
  readonly runnerListenAddress: Option.Option<RunnerAddress>;
  readonly runnerShardWeight: number;
  readonly availableShardGroups: ReadonlyArray<string>;
  readonly assignedShardGroups: ReadonlyArray<string>;
  readonly shardsPerGroup: number;
  readonly shardLockRefreshInterval: DurationInput;
  readonly shardLockExpiration: DurationInput;
  readonly shardLockDisableAdvisory: boolean;
  readonly preemptiveShutdown: boolean;
  readonly entityMailboxCapacity: number | "unbounded";
  readonly entityMaxIdleTime: DurationInput;
  readonly entityRegistrationTimeout: DurationInput;
  readonly entityTerminationTimeout: DurationInput;
  readonly entityMessagePollInterval: DurationInput;
  readonly entityReplyPollInterval: DurationInput;
  readonly refreshAssignmentsInterval: DurationInput;
  readonly sendRetryInterval: DurationInput;
  readonly runnerHealthCheckInterval: DurationInput;
  readonly simulateRemoteSerialization: boolean;
}>() {}
```

Documented defaults from the declaration: `runnerShardWeight` = 1; `availableShardGroups` / `assignedShardGroups` = `["default"]`; `preemptiveShutdown` = true; `entityRegistrationTimeout` = 1 minute; `entityTerminationTimeout` = 15 seconds (kubernetes default). `shardsPerGroup` must be consistent across all runners.

### Layers and config

```ts
export declare const defaults: ShardingConfig["Type"];
export declare const layer: (options?: Partial<ShardingConfig["Type"]>) => Layer.Layer<ShardingConfig>;
export declare const layerDefaults: Layer.Layer<ShardingConfig>;
export declare const config: Config.Config<ShardingConfig["Type"]>;
export declare const configFromEnv: Effect.Effect<ShardingConfig["Type"], ConfigError, never>;
export declare const layerFromEnv: (options?: Partial<ShardingConfig["Type"]> | undefined) => Layer.Layer<ShardingConfig, ConfigError>;
export declare const shardGroupConfig: (config: ShardingConfig["Type"]) => {
  readonly available: ReadonlySet<string>;
  readonly assigned: ReadonlySet<string>;
};
```

`configFromEnv` resolves the full `Type` shape from `effect/Config`; `layerFromEnv` is the production layer with override options.

---

## [Envelope]

The wire-level message union sent between runners: a `Request`, an `AckChunk`, or an `Interrupt`. Carries the encoded/partial-encoded schema variants and primary-key derivation.

### Envelope union and Request

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export type Envelope<R extends Rpc.Any> = Request<R> | AckChunk | Interrupt;

export declare namespace Envelope {
  type Any = Envelope<any>;
  type Encoded = Request.Encoded | typeof AckChunk.Encoded | typeof Interrupt.Encoded;
  type PartialEncoded = Request.PartialEncoded | AckChunk | Interrupt;
}

export interface Request<in out Rpc extends Rpc.Any> {
  readonly [TypeId]: TypeId;
  readonly _tag: "Request";
  readonly requestId: Snowflake;
  readonly address: EntityAddress;
  readonly tag: Rpc.Tag<Rpc>;
  readonly payload: Rpc.Payload<Rpc>;
  readonly headers: Headers.Headers;
  readonly traceId?: string | undefined;
  readonly spanId?: string | undefined;
  readonly sampled?: boolean | undefined;
}

export declare namespace Request {
  type Any = Request<any>;
  interface Encoded {
    readonly _tag: "Request";
    readonly requestId: string;
    readonly address: typeof EntityAddress.Encoded;
    readonly tag: string;
    readonly payload: unknown;
    readonly headers: ReadonlyRecord<string, string>;
    readonly traceId?: string | undefined;
    readonly spanId?: string | undefined;
    readonly sampled?: boolean | undefined;
  }
  interface PartialEncoded {
    readonly _tag: "Request";
    readonly requestId: Snowflake;
    readonly address: EntityAddress;
    readonly tag: string;
    readonly payload: unknown;
    readonly headers: Headers.Headers;
    readonly traceId?: string | undefined;
    readonly spanId?: string | undefined;
    readonly sampled?: boolean | undefined;
  }
}
```

### AckChunk / Interrupt tagged classes

```ts
export declare class AckChunk extends Schema.TaggedClass<AckChunk>()("AckChunk", {
  id: Schema.Schema<Snowflake, string, never>;
  address: typeof EntityAddress;
  requestId: Schema.Schema<Snowflake, string, never>;
  replyId: Schema.Schema<Snowflake, string, never>;
}) {
  readonly [TypeId]: TypeId;
  withRequestId(requestId: Snowflake): AckChunk;
}

export declare class Interrupt extends Schema.TaggedClass<Interrupt>()("Interrupt", {
  id: Schema.Schema<Snowflake, string, never>;
  address: typeof EntityAddress;
  requestId: Schema.Schema<Snowflake, string, never>;
}) {
  readonly [TypeId]: TypeId;
  withRequestId(requestId: Snowflake): Interrupt;
}
```

### Refinement / constructor / schemas / primary key

```ts
export declare const isEnvelope: (u: unknown) => u is Envelope<any>;

export declare const makeRequest: <Rpc extends Rpc.Any>(options: {
  readonly requestId: Snowflake;
  readonly address: EntityAddress;
  readonly tag: Rpc.Tag<Rpc>;
  readonly payload: Rpc.Payload<Rpc>;
  readonly headers: Headers.Headers;
  readonly traceId?: string | undefined;
  readonly spanId?: string | undefined;
  readonly sampled?: boolean | undefined;
}) => Request<Rpc>;

export declare const EnvelopeFromSelf: Schema.Schema<Envelope.Any, Envelope.Any>;
export declare const RequestFromSelf: Schema.Schema<Request.Any, Request.Any>;
export declare const PartialEncodedRequest: Schema.Struct<{ /* Request fields, Snowflake<->string */ }>;
export declare const PartialEncoded: Schema.Union<[Schema.Struct<{ /* Request */ }>, typeof AckChunk, typeof Interrupt]>;
export declare const PartialEncodedArray: Schema.Schema<Array<Envelope.PartialEncoded>, Array<Envelope.Encoded>>;
export declare const PartialEncodedRequestFromSelf: Schema.Struct<{ /* Request fields, self schemas */ }>;
export declare const PartialEncodedFromSelf: Schema.Union<[Schema.Struct<{ /* Request */ }>, Schema.Schema<AckChunk>, Schema.Schema<Interrupt>]>;

export declare const primaryKey: <R extends Rpc.Any>(envelope: Envelope<R>) => string | null;
export declare const primaryKeyByAddress: (options: {
  readonly address: EntityAddress;
  readonly tag: string;
  readonly id: string;
}) => string;
```

---

## [Message]

The local-vs-remote message wrappers around an `Envelope`: incoming (handler-facing) and outgoing (sender-facing) tagged classes, plus serialization codecs.

### Incoming / Outgoing unions and classes

```ts
export type Incoming<R extends Rpc.Any> = IncomingRequest<R> | IncomingEnvelope;
export type IncomingLocal<R extends Rpc.Any> = IncomingRequestLocal<R> | IncomingEnvelope;
export type Outgoing<R extends Rpc.Any> = OutgoingRequest<R> | OutgoingEnvelope;

export declare const incomingLocalFromOutgoing: <R extends Rpc.Any>(self: Outgoing<R>) => IncomingLocal<R>;

export declare class IncomingRequest<R extends Rpc.Any> extends Data.TaggedClass("IncomingRequest")<{
  readonly envelope: Envelope.Request.PartialEncoded;
  readonly lastSentReply: Option.Option<Reply.ReplyEncoded<R>>;
  readonly respond: (reply: Reply.ReplyWithContext<R>) => Effect.Effect<void, MalformedMessage | PersistenceError>;
}> {}

export declare class IncomingRequestLocal<R extends Rpc.Any> extends Data.TaggedClass("IncomingRequestLocal")<{
  readonly envelope: Envelope.Request<R>;
  readonly lastSentReply: Option.Option<Reply.Reply<R>>;
  readonly respond: (reply: Reply.Reply<R>) => Effect.Effect<void, MalformedMessage | PersistenceError>;
}> {}

export declare class IncomingEnvelope extends Data.TaggedClass("IncomingEnvelope")<{
  readonly _tag: "IncomingEnvelope";
  readonly envelope: Envelope.AckChunk | Envelope.Interrupt;
}> {}

export declare class OutgoingRequest<R extends Rpc.Any> extends Data.TaggedClass("OutgoingRequest")<{
  readonly envelope: Envelope.Request<R>;
  readonly context: Context<Rpc.Context<R>>;
  readonly lastReceivedReply: Option.Option<Reply.Reply<R>>;
  readonly rpc: R;
  readonly respond: (reply: Reply.Reply<R>) => Effect.Effect<void>;
}> {
  encodedCache?: Envelope.Request.PartialEncoded;
}

export declare class OutgoingEnvelope extends Data.TaggedClass("OutgoingEnvelope")<{
  readonly envelope: Envelope.AckChunk | Envelope.Interrupt;
  readonly rpc: Rpc.AnyWithProps;
}> {
  static interrupt(options: {
    readonly address: EntityAddress;
    readonly id: Snowflake;
    readonly requestId: Snowflake;
  }): OutgoingEnvelope;
}
```

The `_base` constructors are the `effect/Data` `VoidIfEmpty` tagged-record form; the `extends Data.TaggedClass(...)` rendering above is the equivalent public shape.

### Serialization

```ts
export declare const serialize: <Rpc extends Rpc.Any>(message: Outgoing<Rpc>) => Effect.Effect<Envelope.Envelope.PartialEncoded, MalformedMessage>;
export declare const serializeEnvelope: <Rpc extends Rpc.Any>(message: Outgoing<Rpc>) => Effect.Effect<Envelope.Envelope.Encoded, MalformedMessage>;
export declare const serializeRequest: <Rpc extends Rpc.Any>(self: OutgoingRequest<Rpc>) => Effect.Effect<Envelope.Request.PartialEncoded, MalformedMessage>;
export declare const deserializeLocal: <Rpc extends Rpc.Any>(self: Outgoing<Rpc>, encoded: Envelope.Envelope.PartialEncoded) => Effect.Effect<IncomingLocal<Rpc>, MalformedMessage>;
```

---

## [Reply]

The entity response union: a terminal `WithExit` or an intermediate streaming `Chunk`. Carries encoded variants, context-wrapped form, and the per-rpc schema constructors.

### Reply union, encoded interfaces, ReplyWithContext

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export declare const isReply: (u: unknown) => u is Reply<Rpc.Any>;

export type Reply<R extends Rpc.Any> = WithExit<R> | Chunk<R>;
export type ReplyEncoded<R extends Rpc.Any> = WithExitEncoded<R> | ChunkEncoded<R>;

export interface WithExitEncoded<R extends Rpc.Any> {
  readonly _tag: "WithExit";
  readonly requestId: string;
  readonly id: string;
  readonly exit: Rpc.ExitEncoded<R>;
}

export interface ChunkEncoded<R extends Rpc.Any> {
  readonly _tag: "Chunk";
  readonly requestId: string;
  readonly id: string;
  readonly sequence: number;
  readonly values: NonEmptyReadonlyArray<Rpc.SuccessChunkEncoded<R>>;
}

export declare class ReplyWithContext<R extends Rpc.Any> extends Data.TaggedClass("ReplyWithContext")<{
  readonly reply: Reply<R>;
  readonly context: Context.Context<Rpc.Context<R>>;
  readonly rpc: R;
}> {
  static fromDefect(options: { readonly id: Snowflake; readonly requestId: Snowflake; readonly defect: unknown; }): ReplyWithContext<any>;
  static interrupt(options: { readonly id: Snowflake; readonly requestId: Snowflake; }): ReplyWithContext<any>;
}
```

### Chunk / WithExit classes and schemas

```ts
export declare const Reply: <R extends Rpc.Any>(rpc: R) => Schema.Schema<Reply<R>, ReplyEncoded<R>, Rpc.Context<R>>;

export declare const Encoded: Schema.Union<[
  Schema.Struct<{ _tag: Schema.Literal<["WithExit"]>; requestId: typeof Schema.String; id: typeof Schema.String; exit: typeof Schema.Unknown; }>,
  Schema.Struct<{ _tag: Schema.Literal<["Chunk"]>; requestId: typeof Schema.String; id: typeof Schema.String; sequence: typeof Schema.Number; values: Schema.Array$<typeof Schema.Unknown>; }>
]>;

export declare class Chunk<R extends Rpc.Any> extends Data.TaggedClass("Chunk")<{
  readonly requestId: Snowflake;
  readonly id: Snowflake;
  readonly sequence: number;
  readonly values: NonEmptyReadonlyArray<Rpc.SuccessChunk<R>>;
}> {
  readonly [TypeId]: symbol;
  static emptyFrom(requestId: Snowflake): Chunk<Rpc.Any>;
  static readonly schemaFromSelf: Schema.Schema<Chunk<never>>;
  static schema<R extends Rpc.Any>(rpc: R): Schema.Schema<Chunk<R>, ChunkEncoded<R>, Rpc.Context<R>>;
  withRequestId(requestId: Snowflake): Chunk<R>;
}

export declare class WithExit<R extends Rpc.Any> extends Data.TaggedClass("WithExit")<{
  readonly requestId: Snowflake;
  readonly id: Snowflake;
  readonly exit: Rpc.Exit<R>;
}> {
  readonly [TypeId]: symbol;
  static schema<R extends Rpc.Any>(rpc: R): Schema.Schema<WithExit<R>, WithExitEncoded<R>, Rpc.Context<R>>;
  withRequestId(requestId: Snowflake): WithExit<R>;
}

export declare const serialize: <R extends Rpc.Any>(self: ReplyWithContext<R>) => Effect.Effect<ReplyEncoded<R>, MalformedMessage>;
export declare const serializeLastReceived: <R extends Rpc.Any>(self: OutgoingRequest<R>) => Effect.Effect<Option.Option<ReplyEncoded<R>>, MalformedMessage>;
```

---

## [ClusterError]

The closed tagged-error rail. Every error is a `Schema.TaggedError` class with a `_tag` literal and a `static is(u)` guard; the persistence/malformed pair carries `static refail`.

### Error classes

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export declare class EntityNotAssignedToRunner extends Schema.TaggedError<EntityNotAssignedToRunner>()("EntityNotAssignedToRunner", {
  address: typeof EntityAddress;
}) {
  readonly [TypeId]: symbol;
  static is(u: unknown): u is EntityNotAssignedToRunner;
}

export declare class MalformedMessage extends Schema.TaggedError<MalformedMessage>()("MalformedMessage", {
  cause: typeof Schema.Defect;
}) {
  readonly [TypeId]: symbol;
  static is(u: unknown): u is MalformedMessage;
  static refail: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, MalformedMessage, R>;
}

export declare class PersistenceError extends Schema.TaggedError<PersistenceError>()("PersistenceError", {
  cause: typeof Schema.Defect;
}) {
  readonly [TypeId]: symbol;
  static refail<A, E, R>(effect: Effect.Effect<A, E, R>): Effect.Effect<A, PersistenceError, R>;
}

export declare class RunnerNotRegistered extends Schema.TaggedError<RunnerNotRegistered>()("RunnerNotRegistered", {
  address: typeof RunnerAddress;
}) {
  readonly [TypeId]: symbol;
}

export declare class RunnerUnavailable extends Schema.TaggedError<RunnerUnavailable>()("RunnerUnavailable", {
  address: typeof RunnerAddress;
}) {
  readonly [TypeId]: symbol;
  static is(u: unknown): u is RunnerUnavailable;
}

export declare class MailboxFull extends Schema.TaggedError<MailboxFull>()("MailboxFull", {
  address: typeof EntityAddress;
}) {
  readonly [TypeId]: symbol;
  static is(u: unknown): u is MailboxFull;
}

export declare class AlreadyProcessingMessage extends Schema.TaggedError<AlreadyProcessingMessage>()("AlreadyProcessingMessage", {
  envelopeId: Schema.Schema<Snowflake, string, never>;
  address: typeof EntityAddress;
}) {
  readonly [TypeId]: symbol;
  static is(u: unknown): u is AlreadyProcessingMessage;
}
```

The client-facing error set on an entity `RpcClient.From` is exactly `MailboxFull | AlreadyProcessingMessage | PersistenceError`; the `Sharding.send` rail adds `EntityNotAssignedToRunner`.

---

## [Identity vocabulary]

The branded identity primitives and their schemas: `EntityId`, `EntityType`, `MachineId`, `ShardId`, and the `Snowflake` 64-bit id with its generator.

### EntityId / EntityType / MachineId

```ts
export declare const EntityId: Schema.brand<typeof Schema.NonEmptyTrimmedString, "EntityId">;
export type EntityId = typeof EntityId.Type;
export declare const make: (id: string) => EntityId;

export declare const EntityType: Schema.brand<typeof Schema.NonEmptyTrimmedString, "EntityType">;
export type EntityType = typeof EntityType.Type;

export declare const MachineId: Schema.brand<typeof Schema.Int, "MachineId">;
export type MachineId = typeof MachineId.Type;
export declare const make: (shardId: number) => MachineId;
```

### ShardId

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export declare const make: (group: string, id: number) => ShardId;

export declare class ShardId extends Schema.Class<ShardId>("ShardId")({
  group: typeof Schema.String;
  id: typeof Schema.Int;
}) {
  readonly [TypeId]: TypeId;
  [Equal.symbol](that: ShardId): boolean;
  [Hash.symbol](): number;
  toString(): string;
  static toString(shardId: { readonly group: string; readonly id: number; }): string;
  static fromStringEncoded(s: string): { readonly group: string; readonly id: number; };
  static fromString(s: string): ShardId;
}
```

### Snowflake (64-bit bigint id + generator)

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export type Snowflake = Brand.Branded<bigint, TypeId>;
export declare const Snowflake: (input: string | bigint) => Snowflake;

export declare namespace Snowflake {
  interface Parts {
    readonly timestamp: number;
    readonly machineId: MachineId;
    readonly sequence: number;
  }
  interface Generator {
    readonly unsafeNext: () => Snowflake;
    readonly setMachineId: (machineId: MachineId) => Effect.Effect<void>;
  }
}

export declare const SnowflakeFromBigInt: Schema.Schema<Snowflake, bigint>;
export declare const SnowflakeFromString: Schema.Schema<Snowflake, string>;
export declare const constEpochMillis: number;

export declare const make: (options: { readonly machineId: MachineId; readonly sequence: number; readonly timestamp: number; }) => Snowflake;
export declare const timestamp: (snowflake: Snowflake) => number;
export declare const dateTime: (snowflake: Snowflake) => DateTime.Utc;
export declare const machineId: (snowflake: Snowflake) => MachineId;
export declare const sequence: (snowflake: Snowflake) => number;
export declare const toParts: (snowflake: Snowflake) => Snowflake.Parts;

export declare const makeGenerator: Effect.Effect<Snowflake.Generator>;
export declare class Generator extends Context.TagClass<Generator, "@effect/cluster/Snowflake/Generator", Snowflake.Generator>() {}
export declare const layerGenerator: Layer.Layer<Generator>;
```

`Snowflake` is a branded `bigint` (the `int64AsType: 'bigint'` decode posture on the `transport-wire` snapshot rail aligns to this 64-bit width). `Generator` is the runner-unique id source `Sharding.getSnowflake` draws from.

---

## [Addresses]

The two struct-class addresses: `EntityAddress` (shard + type + id) and `RunnerAddress` (host + port, the `PrimaryKey` carrier). `SingletonAddress` keys a cluster singleton.

### EntityAddress

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export declare class EntityAddress extends Schema.Class<EntityAddress>("EntityAddress")({
  shardId: typeof ShardId;
  entityType: Schema.brand<typeof Schema.NonEmptyTrimmedString, "EntityType">;
  entityId: Schema.brand<typeof Schema.NonEmptyTrimmedString, "EntityId">;
}) {
  readonly [TypeId]: symbol;
  [Equal.symbol](that: EntityAddress): boolean;
  [Hash.symbol](): number;
}

export declare const EntityAddressFromSelf: Schema.Schema<EntityAddress>;
export declare const make: (options: { readonly shardId: ShardId; readonly entityType: EntityType; readonly entityId: EntityId; }) => EntityAddress;
```

### RunnerAddress / SingletonAddress

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export declare class RunnerAddress extends Schema.Class<RunnerAddress>("RunnerAddress")({
  host: typeof Schema.NonEmptyString;
  port: typeof Schema.Int;
}) {
  readonly [TypeId]: symbol;
  [PrimaryKey.symbol](): string;
  [Equal.symbol](that: RunnerAddress): boolean;
  [Hash.symbol](): number;
  toString(): string;
  [NodeInspectSymbol](): string;
}
export declare const make: (host: string, port: number) => RunnerAddress;

export declare class SingletonAddress extends Schema.Class<SingletonAddress>("SingletonAddress")({
  shardId: typeof ShardId;
  name: typeof Schema.NonEmptyTrimmedString;
}) {
  readonly [TypeId]: symbol;
  [Hash.symbol](): number;
  [Equal.symbol](that: SingletonAddress): boolean;
}
```

---

## [ClusterSchema]

The `Context.Reference` annotation flags attached to rpc/entity schemas via `entity.annotateRpcs(...)` / `.annotate(...)`. These drive cluster behavior (persistence, interruptibility, shard-group routing, tracing).

```ts
export declare class Persisted extends Context.ReferenceClass<Persisted, "@effect/cluster/ClusterSchema/Persisted", boolean>() {}

export declare class Uninterruptible extends Context.ReferenceClass<Uninterruptible, "@effect/cluster/ClusterSchema/Uninterruptible", boolean | "client" | "server">() {
  static forServer(context: Context.Context<never>): boolean;
  static forClient(context: Context.Context<never>): boolean;
}

export declare class ShardGroup extends Context.ReferenceClass<ShardGroup, "@effect/cluster/ClusterSchema/ShardGroup", (entityId: EntityId) => string>() {}

export declare class ClientTracingEnabled extends Context.ReferenceClass<ClientTracingEnabled, "@effect/cluster/ClusterSchema/ClientTracingEnabled", boolean>() {}
```

`annotateRpcs(ClusterSchema.Persisted, true)` is the canonical durable-entity flag — it routes messages through `MessageStorage` for exactly-once replay.

---

## [ShardingRegistrationEvent]

The registration-event union streamed from `Sharding.getRegistrationEvents`.

```ts
export type ShardingRegistrationEvent = EntityRegistered | SingletonRegistered;

export interface EntityRegistered {
  readonly _tag: "EntityRegistered";
  readonly entity: Entity<any, any>;
}
export interface SingletonRegistered {
  readonly _tag: "SingletonRegistered";
  readonly address: SingletonAddress;
}

export declare const match: {
  <const Cases extends { readonly EntityRegistered: (args: EntityRegistered) => any; readonly SingletonRegistered: (args: SingletonRegistered) => any; }>(cases: Cases & { [K in Exclude<keyof Cases, "EntityRegistered" | "SingletonRegistered">]: never; }): (value: ShardingRegistrationEvent) => Unify<ReturnType<Cases["EntityRegistered" | "SingletonRegistered"]>>;
  <const Cases extends { readonly EntityRegistered: (args: EntityRegistered) => any; readonly SingletonRegistered: (args: SingletonRegistered) => any; }>(value: ShardingRegistrationEvent, cases: Cases & { [K in Exclude<keyof Cases, "EntityRegistered" | "SingletonRegistered">]: never; }): Unify<ReturnType<Cases["EntityRegistered" | "SingletonRegistered"]>>;
};
export declare const EntityRegistered: Data.Case.Constructor<EntityRegistered, "_tag">;
export declare const SingletonRegistered: Data.Case.Constructor<SingletonRegistered, "_tag">;
```

---

## [MessageStorage]

The mailbox-persistence service: save requests/envelopes/replies, retrieve unprocessed messages and replies, primary-key dedup, reply-handler registration. Carries the `SaveResult` dedup union, the `Encoded` driver shape, and memory/noop layers.

### MessageStorage service

```ts
export declare class MessageStorage extends Context.TagClass<MessageStorage, "@effect/cluster/MessageStorage", {
  readonly saveRequest: <R extends Rpc.Any>(envelope: Message.OutgoingRequest<R>) => Effect.Effect<SaveResult<R>, PersistenceError | MalformedMessage>;
  readonly saveEnvelope: (envelope: Message.OutgoingEnvelope) => Effect.Effect<void, PersistenceError | MalformedMessage>;
  readonly saveReply: <R extends Rpc.Any>(reply: Reply.ReplyWithContext<R>) => Effect.Effect<void, PersistenceError | MalformedMessage>;
  readonly clearReplies: (requestId: Snowflake.Snowflake) => Effect.Effect<void, PersistenceError>;
  readonly repliesFor: <R extends Rpc.Any>(requests: Iterable<Message.OutgoingRequest<R>>) => Effect.Effect<Array<Reply.Reply<R>>, PersistenceError | MalformedMessage>;
  readonly repliesForUnfiltered: <R extends Rpc.Any>(requestIds: Iterable<Snowflake.Snowflake>) => Effect.Effect<Array<Reply.ReplyEncoded<R>>, PersistenceError | MalformedMessage>;
  readonly requestIdForPrimaryKey: (options: { readonly address: EntityAddress; readonly tag: string; readonly id: string; }) => Effect.Effect<Option.Option<Snowflake.Snowflake>, PersistenceError>;
  readonly registerReplyHandler: <R extends Rpc.Any>(message: Message.OutgoingRequest<R> | Message.IncomingRequest<R>) => Effect.Effect<void, EntityNotAssignedToRunner>;
  readonly unregisterReplyHandler: (requestId: Snowflake.Snowflake) => Effect.Effect<void>;
  readonly unregisterShardReplyHandlers: (shardId: ShardId) => Effect.Effect<void>;
  readonly unprocessedMessages: (shardIds: Iterable<ShardId>) => Effect.Effect<Array<Message.Incoming<any>>, PersistenceError>;
  readonly unprocessedMessagesById: <R extends Rpc.Any>(messageIds: Iterable<Snowflake.Snowflake>) => Effect.Effect<Array<Message.Incoming<R>>, PersistenceError>;
  readonly resetShards: (shardIds: Iterable<ShardId>) => Effect.Effect<void, PersistenceError>;
  readonly resetAddress: (address: EntityAddress) => Effect.Effect<void, PersistenceError>;
  readonly clearAddress: (address: EntityAddress) => Effect.Effect<void, PersistenceError>;
}>() {}
```

### SaveResult dedup union

```ts
export type SaveResult<R extends Rpc.Any> = SaveResult.Success | SaveResult.Duplicate<R>;

export declare const SaveResult: {
  readonly Success: <A>(args: void) => SaveResult.Success;
  readonly Duplicate: <A>(args: { readonly originalId: Snowflake.Snowflake; readonly lastReceivedReply: Option.Option<Reply.Reply<A extends Rpc.Any ? A : never>>; }) => SaveResult.Duplicate<A extends Rpc.Any ? A : never>;
  readonly $is: <Tag extends "Success" | "Duplicate">(tag: Tag) => { /* refinement overloads */ };
  readonly $match: { /* match overloads */ };
};
export declare const SaveResultEncoded: {
  readonly Success: Data.Case.Constructor<SaveResult.Success, "_tag">;
  readonly Duplicate: Data.Case.Constructor<SaveResult.DuplicateEncoded, "_tag">;
  readonly $is: <Tag extends "Success" | "Duplicate">(tag: Tag) => (u: unknown) => boolean;
  readonly $match: { /* match overloads */ };
};

export declare namespace SaveResult {
  type Encoded = SaveResult.Success | SaveResult.DuplicateEncoded;
  interface Success { readonly _tag: "Success"; }
  interface Duplicate<R extends Rpc.Any> {
    readonly _tag: "Duplicate";
    readonly originalId: Snowflake.Snowflake;
    readonly lastReceivedReply: Option.Option<Reply.Reply<R>>;
  }
  interface DuplicateEncoded {
    readonly _tag: "Duplicate";
    readonly originalId: Snowflake.Snowflake;
    readonly lastReceivedReply: Option.Option<Reply.ReplyEncoded<any>>;
  }
  interface Constructor extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: SaveResult<this["A"] extends Rpc.Any ? this["A"] : never>;
  }
}
```

### Encoded driver shape, constructors, memory/noop

```ts
export type Encoded = {
  readonly saveEnvelope: (options: { readonly envelope: Envelope.Envelope.Encoded; readonly primaryKey: string | null; readonly deliverAt: number | null; }) => Effect.Effect<SaveResult.Encoded, PersistenceError>;
  readonly saveReply: (reply: Reply.ReplyEncoded<any>) => Effect.Effect<void, PersistenceError>;
  readonly clearReplies: (requestId: Snowflake.Snowflake) => Effect.Effect<void, PersistenceError>;
  readonly requestIdForPrimaryKey: (primaryKey: string) => Effect.Effect<Option.Option<Snowflake.Snowflake>, PersistenceError>;
  readonly repliesFor: (requestIds: Arr.NonEmptyArray<string>) => Effect.Effect<Array<Reply.ReplyEncoded<any>>, PersistenceError>;
  readonly repliesForUnfiltered: (requestIds: Arr.NonEmptyArray<string>) => Effect.Effect<Array<Reply.ReplyEncoded<any>>, PersistenceError>;
  readonly unprocessedMessages: (shardIds: Arr.NonEmptyArray<string>, now: number) => Effect.Effect<Array<{ readonly envelope: Envelope.Envelope.Encoded; readonly lastSentReply: Option.Option<Reply.ReplyEncoded<any>>; }>, PersistenceError>;
  readonly unprocessedMessagesById: (messageIds: Arr.NonEmptyArray<Snowflake.Snowflake>, now: number) => Effect.Effect<Array<{ readonly envelope: Envelope.Envelope.Encoded; readonly lastSentReply: Option.Option<Reply.ReplyEncoded<any>>; }>, PersistenceError>;
  readonly resetAddress: (address: EntityAddress) => Effect.Effect<void, PersistenceError>;
  readonly clearAddress: (address: EntityAddress) => Effect.Effect<void, PersistenceError>;
  readonly resetShards: (shardIds: Arr.NonEmptyArray<string>) => Effect.Effect<void, PersistenceError>;
};

export type EncodedUnprocessedOptions<A> = { readonly existingShards: Array<number>; readonly newShards: Array<number>; readonly cursor: Option.Option<A>; };
export type EncodedRepliesOptions<A> = { readonly existingRequests: Array<string>; readonly newRequests: Array<string>; readonly cursor: Option.Option<A>; };

export declare const make: (storage: Omit<MessageStorage["Type"], "registerReplyHandler" | "unregisterReplyHandler" | "unregisterShardReplyHandlers">) => Effect.Effect<MessageStorage["Type"]>;
export declare const makeEncoded: (encoded: Encoded) => Effect.Effect<MessageStorage["Type"], never, Snowflake.Generator>;
export declare const noop: MessageStorage["Type"];

export type MemoryEntry = {
  readonly envelope: Envelope.Request.Encoded;
  lastReceivedChunk: Option.Option<Reply.ChunkEncoded<any>>;
  replies: Array<Reply.ReplyEncoded<any>>;
  deliverAt: number | null;
};
export declare class MemoryDriver extends Effect.Service<MemoryDriver>()("@effect/cluster/MessageStorage/MemoryDriver", { /* dependencies: [layerGenerator]; effect resolves storage + encoded + maps */ }) {}

export declare const layerNoop: Layer.Layer<MessageStorage>;
export declare const layerMemory: Layer.Layer<MessageStorage | MemoryDriver, never, ShardingConfig>;
```

`makeEncoded` is the path a custom driver (SQL, redis) takes — implement the `Encoded` shape and the package derives the full `MessageStorage["Type"]`.

---

## [RunnerStorage]

The runner-registry + advisory-lock persistence service: register/unregister runners, list runners, acquire/refresh/release shard locks.

```ts
export declare class RunnerStorage extends Context.TagClass<RunnerStorage, "@effect/cluster/RunnerStorage", {
  readonly register: (runner: Runner, healthy: boolean) => Effect.Effect<MachineId.MachineId, PersistenceError>;
  readonly unregister: (address: RunnerAddress) => Effect.Effect<void, PersistenceError>;
  readonly getRunners: Effect.Effect<Array<readonly [runner: Runner, healthy: boolean]>, PersistenceError>;
  readonly setRunnerHealth: (address: RunnerAddress, healthy: boolean) => Effect.Effect<void, PersistenceError>;
  readonly acquire: (address: RunnerAddress, shardIds: Iterable<ShardId>) => Effect.Effect<Array<ShardId>, PersistenceError>;
  readonly refresh: (address: RunnerAddress, shardIds: Iterable<ShardId>) => Effect.Effect<Array<ShardId>, PersistenceError>;
  readonly release: (address: RunnerAddress, shardId: ShardId) => Effect.Effect<void, PersistenceError>;
  readonly releaseAll: (address: RunnerAddress) => Effect.Effect<void, PersistenceError>;
}>() {}

export interface Encoded {
  readonly getRunners: Effect.Effect<Array<readonly [runner: string, healthy: boolean]>, PersistenceError>;
  readonly register: (address: string, runner: string, healthy: boolean) => Effect.Effect<number, PersistenceError>;
  readonly unregister: (address: string) => Effect.Effect<void, PersistenceError>;
  readonly setRunnerHealth: (address: string, healthy: boolean) => Effect.Effect<void, PersistenceError>;
  readonly acquire: (address: string, shardIds: NonEmptyArray<string>) => Effect.Effect<Array<string>, PersistenceError>;
  readonly refresh: (address: string, shardIds: Array<string>) => Effect.Effect<ReadonlyArray<string>, PersistenceError>;
  readonly release: (address: string, shardId: string) => Effect.Effect<void, PersistenceError>;
  readonly releaseAll: (address: string) => Effect.Effect<void, PersistenceError>;
}

export declare const makeEncoded: (encoded: Encoded) => RunnerStorage["Type"];
export declare const makeMemory: Effect.Effect<RunnerStorage["Type"], never, never>;
export declare const layerMemory: Layer.Layer<RunnerStorage>;
```

---

## [Runner]

The physical-server model: `address` + shard `groups` + rebalancing `weight`, with sync codecs.

```ts
export declare const TypeId: unique symbol;
export type TypeId = typeof TypeId;

export declare class Runner extends Schema.Class<Runner>("Runner")({
  address: typeof RunnerAddress;
  groups: Schema.Array$<typeof Schema.String>;
  weight: typeof Schema.Number;
}) {
  static pretty: (a: Runner) => string;
  readonly [TypeId]: symbol;
  static readonly decodeSync: (i: string, overrideOptions?: ParseOptions) => Runner;
  static readonly encodeSync: (a: Runner, overrideOptions?: ParseOptions) => string;
  [NodeInspectSymbol](): string;
  [Equal.symbol](that: Runner): boolean;
  [Hash.symbol](): number;
}

export declare const make: (props: { readonly address: RunnerAddress; readonly groups: ReadonlyArray<string>; readonly weight: number; }) => Runner;
```

---

## [Runners]

The runner-to-runner transport service: ping, local/remote send, notify, availability. Carries the internal RPC group (`Ping`/`Notify`/`Effect`/`Stream`/`Envelope`), the RPC-protocol tag, and the rpc/noop layers.

### Runners service

```ts
export declare class Runners extends Context.TagClass<Runners, "@effect/cluster/Runners", {
  readonly ping: (address: RunnerAddress) => Effect.Effect<void, RunnerUnavailable>;
  readonly sendLocal: <R extends Rpc.Any>(options: {
    readonly message: Message.Outgoing<R>;
    readonly send: <Rpc extends Rpc.Any>(message: Message.IncomingLocal<Rpc>) => Effect.Effect<void, EntityNotAssignedToRunner | MailboxFull | AlreadyProcessingMessage>;
    readonly simulateRemoteSerialization: boolean;
  }) => Effect.Effect<void, EntityNotAssignedToRunner | MailboxFull | AlreadyProcessingMessage | PersistenceError>;
  readonly send: <R extends Rpc.Any>(options: { readonly address: RunnerAddress; readonly message: Message.Outgoing<R>; }) => Effect.Effect<void, EntityNotAssignedToRunner | RunnerUnavailable | MailboxFull | AlreadyProcessingMessage | PersistenceError>;
  readonly notify: <R extends Rpc.Any>(options: { readonly address: Option.Option<RunnerAddress>; readonly message: Message.Outgoing<R>; readonly discard: boolean; }) => Effect.Effect<void, PersistenceError>;
  readonly notifyLocal: <R extends Rpc.Any>(options: { readonly message: Message.Outgoing<R>; readonly notify: (options: Message.IncomingLocal<any>) => Effect.Effect<void, EntityNotAssignedToRunner>; readonly discard: boolean; readonly storageOnly?: boolean | undefined; }) => Effect.Effect<void, PersistenceError>;
  readonly onRunnerUnavailable: (address: RunnerAddress) => Effect.Effect<void>;
}>() {}
```

### Constructors, RPC group, RpcClientProtocol

```ts
export declare const make: (options: Omit<Runners["Type"], "sendLocal" | "notifyLocal">) => Effect.Effect<Runners["Type"], never, MessageStorage.MessageStorage | Snowflake.Generator | ShardingConfig | Scope>;
export declare const makeNoop: Effect.Effect<Runners["Type"], never, MessageStorage.MessageStorage | Snowflake.Generator | ShardingConfig | Scope>;
export declare const layerNoop: Layer.Layer<Runners, never, ShardingConfig | MessageStorage.MessageStorage>;

export declare class Rpcs extends RpcGroup.RpcGroup</* Ping | Notify | Effect | Stream | Envelope */> {}
export interface RpcClient extends RpcClient_.FromGroup<typeof Rpcs, RpcClientError> {}
export declare const makeRpcClient: Effect.Effect<RpcClient, never, RpcClient_.Protocol | Scope>;

export declare const makeRpc: Effect.Effect<Runners["Type"], never, Scope | RpcClientProtocol | MessageStorage.MessageStorage | Snowflake.Generator | ShardingConfig>;
export declare const layerRpc: Layer.Layer<Runners, never, MessageStorage.MessageStorage | RpcClientProtocol | ShardingConfig>;

export declare class RpcClientProtocol extends Context.TagClass<RpcClientProtocol, "@effect/cluster/Runners/RpcClientProtocol", (address: RunnerAddress) => Effect.Effect<RpcClient_.Protocol["Type"], never, Scope>>() {}
```

The `Rpcs` group is the internal runner-mesh protocol (`Ping`, `Notify`, `Effect`, `Stream`, `Envelope`); `RpcClientProtocol` is the per-address transport factory the `HttpRunner` / `SocketRunner` layers provide.

---

## [RunnerHealth]

The runner liveness probe service with noop, ping, and Kubernetes-backed variants.

```ts
export declare class RunnerHealth extends Context.TagClass<RunnerHealth, "@effect/cluster/RunnerHealth", {
  readonly isAlive: (address: RunnerAddress) => Effect.Effect<boolean>;
}>() {}

export declare const layerNoop: Layer.Layer<RunnerHealth, never, never>;
export declare const makePing: Effect.Effect<RunnerHealth["Type"], never, Runners.Runners | Scope.Scope>;
export declare const layerPing: Layer.Layer<RunnerHealth, never, Runners.Runners>;
export declare const makeK8s: (options?: { readonly namespace?: string | undefined; readonly labelSelector?: string | undefined; } | undefined) => Effect.Effect<{ readonly isAlive: (address: RunnerAddress) => Effect.Effect<boolean>; }, never, K8s.K8sHttpClient>;
export declare const layerK8s: (options?: { readonly namespace?: string | undefined; readonly labelSelector?: string | undefined; } | undefined) => Layer.Layer<RunnerHealth, never, K8s.K8sHttpClient>;
```

---

## [RunnerServer]

The server-side layers receiving runner-mesh RPC and forwarding to `Sharding`, plus the client-only embed layer.

```ts
export declare const layerHandlers: Layer.Layer<Rpc.Handler<"Ping"> | Rpc.Handler<"Notify"> | Rpc.Handler<"Effect"> | Rpc.Handler<"Stream"> | Rpc.Handler<"Envelope">, never, MessageStorage.MessageStorage | Sharding.Sharding>;
export declare const layer: Layer.Layer<never, never, RpcServer.Protocol | Sharding.Sharding | MessageStorage.MessageStorage>;
export declare const layerWithClients: Layer.Layer<Sharding.Sharding | Runners.Runners, never, RpcServer.Protocol | ShardingConfig | Runners.RpcClientProtocol | MessageStorage.MessageStorage | RunnerStorage.RunnerStorage | RunnerHealth.RunnerHealth>;
export declare const layerClientOnly: Layer.Layer<Sharding.Sharding | Runners.Runners, never, ShardingConfig | Runners.RpcClientProtocol | MessageStorage.MessageStorage | RunnerStorage.RunnerStorage>;
```

---

## [HttpRunner]

The HTTP and WebSocket transport assemblies: client-protocol layers, raw http-app effects, and the full-runner layers in registering and client-only forms.

```ts
export declare const layerClientProtocolHttp: (options: { readonly path: string; readonly https?: boolean | undefined; }) => Layer.Layer<RpcClientProtocol, never, RpcSerialization.RpcSerialization | HttpClient.HttpClient>;
export declare const layerClientProtocolHttpDefault: Layer.Layer<Runners.RpcClientProtocol, never, RpcSerialization.RpcSerialization | HttpClient.HttpClient>;
export declare const layerClientProtocolWebsocket: (options: { readonly path: string; readonly https?: boolean | undefined; }) => Layer.Layer<RpcClientProtocol, never, RpcSerialization.RpcSerialization | Socket.WebSocketConstructor>;
export declare const layerClientProtocolWebsocketDefault: Layer.Layer<Runners.RpcClientProtocol, never, RpcSerialization.RpcSerialization | Socket.WebSocketConstructor>;

export declare const toHttpEffect: Effect.Effect<Effect.Effect<HttpServerResponse, never, Scope | HttpServerRequest>, never, Scope | RpcSerialization.RpcSerialization | Sharding.Sharding | MessageStorage>;
export declare const toHttpEffectWebsocket: Effect.Effect<Effect.Effect<HttpServerResponse, never, Scope | HttpServerRequest>, never, Scope | RpcSerialization.RpcSerialization | Sharding.Sharding | MessageStorage>;

export declare const layerClient: Layer.Layer<Sharding.Sharding | Runners.Runners, never, ShardingConfig.ShardingConfig | Runners.RpcClientProtocol | MessageStorage | RunnerStorage | RunnerHealth>;
export declare const layerHttpOptions: (options: { readonly path: HttpRouter.PathInput; }) => Layer.Layer<Sharding.Sharding | Runners.Runners, never, RunnerStorage | RunnerHealth | RpcSerialization.RpcSerialization | MessageStorage | ShardingConfig.ShardingConfig | Runners.RpcClientProtocol | HttpRouter.HttpRouter>;
export declare const layerWebsocketOptions: (options: { readonly path: HttpRouter.PathInput; }) => Layer.Layer<Sharding.Sharding | Runners.Runners, never, ShardingConfig.ShardingConfig | Runners.RpcClientProtocol | MessageStorage | RunnerStorage | RunnerHealth | RpcSerialization.RpcSerialization | HttpRouter.HttpRouter>;
export declare const layerHttp: Layer.Layer<Sharding.Sharding | Runners.Runners, never, RpcSerialization.RpcSerialization | ShardingConfig.ShardingConfig | HttpClient.HttpClient | HttpServer.HttpServer | MessageStorage | RunnerStorage | RunnerHealth>;
export declare const layerHttpClientOnly: Layer.Layer<Sharding.Sharding | Runners.Runners, never, RpcSerialization.RpcSerialization | ShardingConfig.ShardingConfig | HttpClient.HttpClient | MessageStorage | RunnerStorage>;
export declare const layerWebsocket: Layer.Layer<Sharding.Sharding | Runners.Runners, never, RpcSerialization.RpcSerialization | ShardingConfig.ShardingConfig | Socket.WebSocketConstructor | HttpServer.HttpServer | MessageStorage | RunnerStorage | RunnerHealth>;
export declare const layerWebsocketClientOnly: Layer.Layer<Sharding.Sharding | Runners.Runners, never, ShardingConfig.ShardingConfig | MessageStorage | RunnerStorage | RpcSerialization.RpcSerialization | Socket.WebSocketConstructor>;
```

---

## [SocketRunner]

The raw-socket transport assembly (tcp `SocketServer`-backed) in registering and client-only forms.

```ts
export declare const layer: Layer.Layer<Sharding.Sharding | Runners.Runners, never, Runners.RpcClientProtocol | ShardingConfig | RpcSerialization.RpcSerialization | SocketServer | MessageStorage | RunnerStorage.RunnerStorage | RunnerHealth>;
export declare const layerClientOnly: Layer.Layer<Sharding.Sharding | Runners.Runners, never, Runners.RpcClientProtocol | ShardingConfig | MessageStorage | RunnerStorage.RunnerStorage>;
```

---

## [SingleRunner]

The single-node SQL-backed cluster for durable entities and workflows.

```ts
export declare const layer: (options?: {
  readonly shardingConfig?: Partial<ShardingConfig.ShardingConfig["Type"]> | undefined;
  readonly runnerStorage?: "memory" | "sql" | undefined;
}) => Layer.Layer<Sharding.Sharding | Runners.Runners | MessageStorage.MessageStorage, ConfigError.ConfigError, SqlClient.SqlClient>;
```

This is the canonical durable-cluster entry for a single node — it composes `Sharding` + `Runners` + `MessageStorage` over one `SqlClient`, optionally backing runner-storage in SQL or memory.

---

## [TestRunner]

The in-memory cluster for testing — memory-backed message and runner storage.

```ts
export declare const layer: Layer.Layer<Sharding.Sharding | Runners.Runners | MessageStorage.MessageStorage | MessageStorage.MemoryDriver>;
```

---

## [SqlMessageStorage]

The SQL-backed `MessageStorage` driver over `@effect/sql` `SqlClient`, with table-prefix option.

```ts
export declare const make: (options?: { readonly prefix?: string | undefined; } | undefined) => Effect.Effect<MessageStorage["Type"], never, Snowflake.Generator | SqlClient.SqlClient>;
export declare const layer: Layer.Layer<MessageStorage.MessageStorage, never, SqlClient.SqlClient | ShardingConfig>;
export declare const layerWith: (options: { readonly prefix?: string | undefined; }) => Layer.Layer<MessageStorage.MessageStorage, never, SqlClient.SqlClient | ShardingConfig>;
```

---

## [SqlRunnerStorage]

The SQL-backed `RunnerStorage` driver (advisory-lock shard acquisition) over `SqlClient`. Emits `SqlError`.

```ts
export declare const make: (options: { readonly prefix?: string | undefined; }) => Effect.Effect<RunnerStorage["Type"], SqlError, Scope.Scope | ShardingConfig.ShardingConfig | SqlClient.SqlClient>;
export declare const layer: Layer.Layer<RunnerStorage.RunnerStorage, SqlError, SqlClient.SqlClient | ShardingConfig.ShardingConfig>;
export declare const layerWith: (options: { readonly prefix?: string | undefined; }) => Layer.Layer<RunnerStorage.RunnerStorage, SqlError, SqlClient.SqlClient | ShardingConfig.ShardingConfig>;
```

---

## [ClusterWorkflowEngine]

The cluster-backed `WorkflowEngine` from `@effect/workflow`: register/execute/poll/interrupt/resume workflows, activity execution, durable-deferred + durable-clock plumbing. This is the surface `ClusterEngine` (`durable-execution/engine#ENGINE`) layers.

```ts
export declare const make: Effect.Effect<{
  readonly register: <Name extends string, Payload extends Workflow.AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(workflow: Workflow.Workflow<Name, Payload, Success, Error>, execute: (payload: Payload["Type"], executionId: string) => Effect.Effect<Success["Type"], Error["Type"], R>) => Effect.Effect<void, never, Scope.Scope | Exclude<R, WorkflowEngine | WorkflowInstance | Workflow.Execution<Name> | Scope.Scope> | Payload["Context"] | Success["Context"] | Error["Context"]>;
  readonly execute: <Name extends string, Payload extends Workflow.AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All, const Discard extends boolean = false>(workflow: Workflow.Workflow<Name, Payload, Success, Error>, options: { readonly executionId: string; readonly payload: Payload["Type"]; readonly discard?: Discard | undefined; readonly suspendedRetrySchedule?: Schedule.Schedule<any, unknown> | undefined; }) => Effect.Effect<Discard extends true ? string : Success["Type"], Error["Type"], Payload["Context"] | Success["Context"] | Error["Context"]>;
  readonly poll: <Name extends string, Payload extends Workflow.AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(workflow: Workflow.Workflow<Name, Payload, Success, Error>, executionId: string) => Effect.Effect<Workflow.Result<Success["Type"], Error["Type"]> | undefined, never, Success["Context"] | Error["Context"]>;
  readonly interrupt: (workflow: Workflow.Any, executionId: string) => Effect.Effect<void>;
  readonly resume: (workflow: Workflow.Any, executionId: string) => Effect.Effect<void>;
  readonly activityExecute: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(activity: Activity.Activity<Success, Error, R>, attempt: number) => Effect.Effect<Workflow.Result<Success["Type"], Error["Type"]>, never, Success["Context"] | Error["Context"] | R | WorkflowInstance>;
  readonly deferredResult: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(deferred: DurableDeferred.DurableDeferred<Success, Error>) => Effect.Effect<Exit.Exit<Success["Type"], Error["Type"]> | undefined, never, WorkflowInstance>;
  readonly deferredDone: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(deferred: DurableDeferred.DurableDeferred<Success, Error>, options: { readonly workflowName: string; readonly executionId: string; readonly deferredName: string; readonly exit: Exit.Exit<Success["Type"], Error["Type"]>; }) => Effect.Effect<void, never, Success["Context"] | Error["Context"]>;
  readonly scheduleClock: (workflow: Workflow.Any, options: { readonly executionId: string; readonly clock: DurableClock.DurableClock; }) => Effect.Effect<void>;
}, never, Scope.Scope | MessageStorage | Sharding.Sharding>;

export declare const layer: Layer.Layer<WorkflowEngine, never, Sharding.Sharding | MessageStorage>;
```

---

## [ClusterCron]

The shard-pinned durable cron layer — a singleton-like scheduled job bound to a shard group.

```ts
export declare const make: <E, R>(options: {
  readonly name: string;
  readonly cron: Cron.Cron;
  readonly execute: Effect.Effect<void, E, R>;
  readonly shardGroup?: string | undefined;
  readonly calculateNextRunFromPrevious?: boolean | undefined;
  readonly skipIfOlderThan?: Duration.DurationInput | undefined;
}) => Layer.Layer<never, never, Sharding | Exclude<R, Scope>>;
```

Documented defaults: `calculateNextRunFromPrevious` = false (next run from current time); `skipIfOlderThan` = "1 day". This is the `ScheduledWork` durable-cron surface.

---

## [Singleton]

The cluster-singleton layer — a single named effect runs on exactly one runner across the cluster.

```ts
export declare const make: <E, R>(name: string, run: Effect.Effect<void, E, R>, options?: {
  readonly shardGroup?: string | undefined;
}) => Layer.Layer<never, never, Sharding | Exclude<R, Scope>>;
```

---

## [EntityProxy / EntityProxyServer]

Derive an `RpcGroup` or `HttpApiGroup` from an `Entity` (tag-prefixed, with a `<Tag>Discard` variant per rpc), and the server layers implementing the derived handlers.

### EntityProxy (derivation)

```ts
export declare const toRpcGroup: <Type extends string, Rpcs extends Rpc.Any>(entity: Entity.Entity<Type, Rpcs>) => RpcGroup.RpcGroup<ConvertRpcs<Rpcs, Type>>;

export type ConvertRpcs<Rpcs extends Rpc.Any, Prefix extends string> = Rpcs extends Rpc.Rpc<infer _Tag, infer _Payload, infer _Success, infer _Error, infer _Middleware>
  ? Rpc.Rpc<`${Prefix}.${_Tag}`, Schema.Struct<{ entityId: typeof Schema.String; payload: _Payload; }>, _Success, Schema.Schema<_Error["Type"] | MailboxFull | AlreadyProcessingMessage | PersistenceError | _Error["Encoded"] | typeof MailboxFull["Encoded"] | typeof AlreadyProcessingMessage["Encoded"] | typeof PersistenceError["Encoded"], _Error["Context"]>>
    | Rpc.Rpc<`${Prefix}.${_Tag}Discard`, Schema.Struct<{ entityId: typeof Schema.String; payload: _Payload; }>, typeof Schema.Void, Schema.Union<[typeof MailboxFull, typeof AlreadyProcessingMessage, typeof PersistenceError]>>
  : never;

export declare const toHttpApiGroup: <const Name extends string, Type extends string, Rpcs extends Rpc.Any>(name: Name, entity: Entity.Entity<Type, Rpcs>) => HttpApiGroup.HttpApiGroup<Name, ConvertHttpApi<Rpcs>>;

export type ConvertHttpApi<Rpcs extends Rpc.Any> = Rpcs extends Rpc.Rpc<infer _Tag, infer _Payload, infer _Success, infer _Error, infer _Middleware>
  ? HttpApiEndpoint.HttpApiEndpoint<_Tag, "POST", { readonly entityId: string; }, never, _Payload["Type"], never, _Success["Type"], _Error["Type"] | MailboxFull | AlreadyProcessingMessage | PersistenceError, _Payload["Context"] | _Success["Context"], _Error["Context"]>
    | HttpApiEndpoint.HttpApiEndpoint<`${_Tag}Discard`, "POST", { readonly entityId: string; }, never, _Payload["Type"], never, void, MailboxFull | AlreadyProcessingMessage | PersistenceError>
  : never;
```

### EntityProxyServer (handlers)

```ts
export declare const layerHttpApi: <ApiId extends string, Groups extends HttpApiGroup.Any, ApiE, ApiR, Name extends HttpApiGroup.Name<Groups>, Type extends string, Rpcs extends Rpc.Any>(api: HttpApi.HttpApi<ApiId, Groups, ApiE, ApiR>, name: Name, entity: Entity.Entity<Type, Rpcs>) => Layer.Layer<ApiGroup<ApiId, Name>, never, Sharding | Rpc.Context<Rpcs>>;
export declare const layerRpcHandlers: <const Type extends string, Rpcs extends Rpc.Any>(entity: Entity.Entity<Type, Rpcs>) => Layer.Layer<RpcHandlers<Rpcs, Type>, never, Sharding | Rpc.Context<Rpcs>>;
export type RpcHandlers<Rpcs extends Rpc.Any, Prefix extends string> = Rpcs extends Rpc.Rpc<infer _Tag, infer _Payload, infer _Success, infer _Error, infer _Middleware> ? Rpc.Handler<`${Prefix}.${_Tag}`> | Rpc.Handler<`${Prefix}.${_Tag}Discard`> : never;
```

---

## [EntityResource]

A scoped resource acquired inside an entity that survives restarts until idle-ttl elapses or `close` is called. Includes the `CloseScope` tag and a Kubernetes-pod resource constructor.

```ts
export declare const TypeId: "~@effect/cluster/EntityResource";
export type TypeId = "~@effect/cluster/EntityResource";

export interface EntityResource<out A, out E = never> {
  readonly [TypeId]: TypeId;
  readonly get: Effect.Effect<A, E, Scope.Scope>;
  readonly close: Effect.Effect<void>;
}

export declare class CloseScope extends Context.TagClass<CloseScope, "@effect/cluster/EntityResource/CloseScope", Scope.Scope>() {}

export declare const make: <A, E, R>(options: { readonly acquire: Effect.Effect<A, E, R>; readonly idleTimeToLive?: Duration.DurationInput | undefined; }) => Effect.Effect<EntityResource<A, E>, E, Scope.Scope | Exclude<R, CloseScope> | Sharding | Entity.CurrentAddress>;
export declare const makeK8sPod: (spec: v1.Pod, options?: { readonly idleTimeToLive?: Duration.DurationInput | undefined; } | undefined) => Effect.Effect<EntityResource<K8sHttpClient.PodStatus>, never, Scope.Scope | Sharding | Entity.CurrentAddress | K8sHttpClient.K8sHttpClient>;
```

---

## [K8sHttpClient]

The Kubernetes API client tag and pod schemas backing `RunnerHealth.layerK8s` and `EntityResource.makeK8sPod`.

```ts
export declare class K8sHttpClient extends Context.TagClass<K8sHttpClient, "@effect/cluster/K8sHttpClient", HttpClient.HttpClient>() {}
export declare const layer: Layer.Layer<K8sHttpClient, never, HttpClient.HttpClient | FileSystem.FileSystem>;

export declare const makeGetPods: (options?: { readonly namespace?: string | undefined; readonly labelSelector?: string | undefined; } | undefined) => Effect.Effect<Effect.Effect<Map<string, Pod>, HttpClientError.HttpClientError | ParseResult.ParseError, never>, never, K8sHttpClient>;
export declare const makeCreatePod: Effect.Effect<(spec: v1.Pod) => Effect.Effect<PodStatus, never, Scope>, never, K8sHttpClient>;

export declare class PodStatus extends Schema.Class<PodStatus>("PodStatus")({
  phase: typeof Schema.String;
  conditions: Schema.Array$<Schema.Struct<{ type: typeof Schema.String; status: typeof Schema.String; lastTransitionTime: Schema.NullOr<typeof Schema.String>; }>>;
  podIP: typeof Schema.String;
  hostIP: typeof Schema.String;
}) {}

export declare class Pod extends Schema.Class<Pod>("Pod")({ status: typeof PodStatus; }) {
  get isReady(): boolean;
  get isReadyOrInitializing(): boolean;
}
```

---

## [DeliverAt]

The delayed-delivery protocol: an entity payload implementing `DeliverAt` schedules its message for a future `DateTime`.

```ts
export declare const symbol: unique symbol;

export interface DeliverAt {
  [symbol](): DateTime;
}

export declare const isDeliverAt: (self: unknown) => self is DeliverAt;
export declare const toMillis: (self: unknown) => number | null;
```

---

## [ClusterMetrics]

The five gauge metrics emitted by the runtime — the source for the node `cluster-metrics` observability signal.

```ts
export declare const entities: Metric.Metric.Gauge<bigint>;
export declare const singletons: Metric.Metric.Gauge<bigint>;
export declare const runners: Metric.Metric.Gauge<bigint>;
export declare const runnersHealthy: Metric.Metric.Gauge<bigint>;
export declare const shards: Metric.Metric.Gauge<bigint>;
```
