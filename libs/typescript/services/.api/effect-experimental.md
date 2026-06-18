# [API_CATALOGUE] @effect/experimental

`@effect/experimental` re-exports each module under its own namespace; the barrel covers durable-substrate persistence (`Persistence`, `PersistedCache`, `PersistedQueue`), rate-limiting, key-scoped reactivity, request-batching, event journal/log/sync, actor state machines, and SSE encoding.

---

## [1]-[INDEX_BARREL]

```ts
// @effect/experimental — namespace re-exports (index.d.ts)
export * as DevTools from "./DevTools.js"
export * as Event from "./Event.js"
export * as EventGroup from "./EventGroup.js"
export * as EventJournal from "./EventJournal.js"
export * as EventLog from "./EventLog.js"
export * as EventLogEncryption from "./EventLogEncryption.js"
export * as EventLogRemote from "./EventLogRemote.js"
export * as EventLogServer from "./EventLogServer.js"
export * as Machine from "./Machine.js"
export * as PersistedCache from "./PersistedCache.js"
export * as PersistedQueue from "./PersistedQueue.js"
export * as Persistence from "./Persistence.js"
export * as RateLimiter from "./RateLimiter.js"
export * as Reactivity from "./Reactivity.js"
export * as RequestResolver from "./RequestResolver.js"
export * as Sse from "./Sse.js"
export * as VariantSchema from "./VariantSchema.js"
```

| [INDEX] | [MODULE]                                                   | [SECTION] | [PRIMARY ROLE]                                        |
| :-----: | :--------------------------------------------------------- | :-------: | :---------------------------------------------------- |
|   [1]   | `Persistence`                                              |    [2]    | backing/result persistence stores + KVS/memory layers |
|   [2]   | `PersistedCache`                                           |    [3]    | result-persisted memoized lookup cache                |
|   [3]   | `PersistedQueue`                                           |    [4]    | durable retrying work queue                           |
|   [4]   | `RequestResolver`                                          |    [5]    | dataLoader batching + persisted request resolvers     |
|   [5]   | `RateLimiter`                                              |    [6]    | fixed-window / token-bucket limiter + store           |
|   [6]   | `Reactivity`                                               |    [7]    | key-scoped mutation/query/stream invalidation         |
|   [7]   | `Event` / `EventGroup`                                     |    [8]    | event definition + domain grouping vocabulary         |
|   [8]   | `EventJournal`                                             |    [9]    | local append-only journal + entry/remote model        |
|   [9]   | `EventLog`                                                 |   [10]    | handler registry, compaction, reactivity, client      |
|  [10]   | `EventLogRemote` / `EventLogServer` / `EventLogEncryption` |   [11]    | sync protocol, server storage, subtle-crypto          |
|  [11]   | `Machine`                                                  |   [12]    | actor state machine + serializable snapshot/restore   |
|  [12]   | `Sse`                                                      |   [13]    | server-sent-events parser/encoder channel             |
|  [13]   | `DevTools`                                                 |   [14]    | tracing client socket layers                          |
|  [14]   | `VariantSchema`                                            |   [15]    | multi-variant schema struct/field/class builder       |

---

## [2]-[PERSISTENCE]

Backing stores hold raw `unknown` values keyed by string; result stores hold `Exit` values keyed by
a `Schema.WithResult` + `PrimaryKey` request. `PersistenceError` is the union error rail.

```ts
// @effect/experimental/Persistence
export const ErrorTypeId: unique symbol
export type ErrorTypeId = typeof ErrorTypeId

export type PersistenceError = PersistenceParseError | PersistenceBackingError

export declare class PersistenceParseError extends ... {
  // _tag: "PersistenceError"; reason: "ParseError"; method: string; error: ParseResult.ParseError["issue"]
  static make(method: string, error: ParseResult.ParseError["issue"]): PersistenceParseError
  get message(): string
}
export declare class PersistenceBackingError extends ... {
  // _tag: "PersistenceError"; reason: "BackingError"; method: string; cause: unknown
  static make(method: string, cause: unknown): PersistenceBackingError
  get message(): "BackingError"
}

export const BackingPersistenceTypeId: unique symbol
export type BackingPersistenceTypeId = typeof BackingPersistenceTypeId
export interface BackingPersistence {
  readonly [BackingPersistenceTypeId]: BackingPersistenceTypeId
  readonly make: (storeId: string) => Effect.Effect<BackingPersistenceStore, never, Scope.Scope>
}
export interface BackingPersistenceStore {
  readonly get: (key: string) => Effect.Effect<Option.Option<unknown>, PersistenceError>
  readonly getMany: (key: Array<string>) => Effect.Effect<Array<Option.Option<unknown>>, PersistenceError>
  readonly set: (key: string, value: unknown, ttl: Option.Option<Duration.Duration>) => Effect.Effect<void, PersistenceError>
  readonly setMany: (entries: ReadonlyArray<readonly [key: string, value: unknown, ttl: Option.Option<Duration.Duration>]>) => Effect.Effect<void, PersistenceError>
  readonly remove: (key: string) => Effect.Effect<void, PersistenceError>
  readonly clear: Effect.Effect<void, PersistenceError>
}
export const BackingPersistence: Context.Tag<BackingPersistence, BackingPersistence>

export const ResultPersistenceTypeId: unique symbol
export type ResultPersistenceTypeId = typeof ResultPersistenceTypeId
export interface ResultPersistence {
  readonly [ResultPersistenceTypeId]: ResultPersistenceTypeId
  readonly make: (options: {
    readonly storeId: string
    readonly timeToLive?: (key: ResultPersistence.KeyAny, exit: Exit.Exit<unknown, unknown>) => Duration.DurationInput
  }) => Effect.Effect<ResultPersistenceStore, never, Scope.Scope>
}
export interface ResultPersistenceStore {
  readonly get: <R, IE, E, IA, A>(key: ResultPersistence.Key<R, IE, E, IA, A>) => Effect.Effect<Option.Option<Exit.Exit<A, E>>, PersistenceError, R>
  readonly getMany: <R, IE, E, IA, A>(key: ReadonlyArray<ResultPersistence.Key<R, IE, E, IA, A>>) => Effect.Effect<Array<Option.Option<Exit.Exit<A, E>>>, PersistenceError, R>
  readonly set: <R, IE, E, IA, A>(key: ResultPersistence.Key<R, IE, E, IA, A>, value: Exit.Exit<A, E>) => Effect.Effect<void, PersistenceError, R>
  readonly setMany: <R, IE, E, IA, A>(entries: Iterable<readonly [ResultPersistence.Key<R, IE, E, IA, A>, Exit.Exit<A, E>]>) => Effect.Effect<void, PersistenceError, R>
  readonly remove: <R, IE, E, IA, A>(key: ResultPersistence.Key<R, IE, E, IA, A>) => Effect.Effect<void, PersistenceError>
  readonly clear: Effect.Effect<void, PersistenceError>
}
export const ResultPersistence: Context.Tag<ResultPersistence, ResultPersistence>

export interface Persistable<A extends Schema.Schema.Any, E extends Schema.Schema.All>
  extends Schema.WithResult<A["Type"], A["Encoded"], E["Type"], E["Encoded"], A["Context"] | E["Context"]>, PrimaryKey.PrimaryKey {}

export namespace ResultPersistence {
  interface Key<R, IE, E, IA, A> extends Schema.WithResult<A, IA, E, IE, R>, PrimaryKey.PrimaryKey {}
  type KeyAny = Persistable<any, any>
  type TimeToLiveArgs<A> = A extends infer K
    ? K extends Persistable<infer _A, infer _E> ? [request: K, exit: Exit.Exit<_A["Type"], _E["Type"]>] : never
    : never
}

// layers
export const layerResult: Layer.Layer<ResultPersistence, never, BackingPersistence>
export const layerMemory: Layer.Layer<BackingPersistence>
export const layerKeyValueStore: Layer.Layer<BackingPersistence, never, KeyValueStore.KeyValueStore>
export const layerResultMemory: Layer.Layer<ResultPersistence>
export const layerResultKeyValueStore: Layer.Layer<ResultPersistence, never, KeyValueStore.KeyValueStore>

export const unsafeTtlToExpires: (clock: Clock.Clock, ttl: Option.Option<Duration.Duration>) => number | null
```

- `layerKeyValueStore` / `layerResultKeyValueStore` are the seam to `@effect/platform/KeyValueStore`; the browser `host` snapshot store composes these over a `KeyValueStore` binding rather than reading `localStorage` directly.
- `Persistable` requires a request schema that is both `Schema.WithResult` and `PrimaryKey.PrimaryKey`; the primary key is the persistence key.

---

## [3]-[PERSISTED_CACHE]

```ts
// @effect/experimental/PersistedCache
export interface PersistedCache<K extends Persistence.ResultPersistence.KeyAny> {
  readonly get: (key: K) => Effect.Effect<Schema.WithResult.Success<K>, Schema.WithResult.Failure<K> | Persistence.PersistenceError>
  readonly invalidate: (key: K) => Effect.Effect<void, Persistence.PersistenceError>
}

export const make: <K extends Persistence.ResultPersistence.KeyAny, R>(options: {
  readonly storeId: string
  readonly lookup: (key: K) => Effect.Effect<Schema.WithResult.Success<K>, Schema.WithResult.Failure<K>, R>
  readonly timeToLive: (...args: Persistence.ResultPersistence.TimeToLiveArgs<K>) => Duration.DurationInput
  readonly inMemoryCapacity?: number | undefined
  readonly inMemoryTTL?: Duration.DurationInput | undefined
}) => Effect.Effect<PersistedCache<K>, never, Schema.SerializableWithResult.Context<K> | R | Persistence.ResultPersistence | Scope.Scope>
```

- Requires a `Persistence.ResultPersistence` layer in context; `inMemoryCapacity`/`inMemoryTTL` add an L1 cache above the result store.

---

## [4]-[PERSISTED_QUEUE]

```ts
// @effect/experimental/PersistedQueue
export const TypeId: TypeId
export type TypeId = "~@effect/experimental/PersistedQueue"

export interface PersistedQueue<in out A, out R = never> {
  readonly [TypeId]: TypeId
  // Returns the id of the enqueued element; duplicate ids are not re-added.
  readonly offer: (value: A, options?: { readonly id: string | undefined }) =>
    Effect.Effect<string, PersistedQueueError | ParseResult.ParseError, R>
  // Element marked processed on success, retried otherwise (default maxAttempts 10).
  readonly take: <XA, XE, XR>(
    f: (value: A, metadata: { readonly id: string; readonly attempts: number }) => Effect.Effect<XA, XE, XR>,
    options?: { readonly maxAttempts?: number | undefined }
  ) => Effect.Effect<XA, XE | PersistedQueueError | ParseResult.ParseError, R | XR>
}

export declare class PersistedQueueFactory extends Context.TagClass<
  PersistedQueueFactory, "@effect/experimental/PersistedQueue/PersistedQueueFactory", {
    readonly make: <A, I, R>(options: { readonly name: string; readonly schema: Schema.Schema<A, I, R> }) => Effect.Effect<PersistedQueue<A, R>>
  }> {}

export const make: <A, I, R>(options: { readonly name: string; readonly schema: Schema.Schema<A, I, R> })
  => Effect.Effect<PersistedQueue<A, R>, never, PersistedQueueFactory>
export const makeFactory: Effect.Effect<{ readonly make: <A, I, R>(options: { readonly name: string; readonly schema: Schema.Schema<A, I, R> }) => Effect.Effect<PersistedQueue<A, R>> }, never, PersistedQueueStore>
export const layer: Layer.Layer<PersistedQueueFactory, never, PersistedQueueStore>

export const ErrorTypeId: ErrorTypeId
export type ErrorTypeId = "~@effect/experimental/PersistedQueue/PersistedQueueError"
export declare class PersistedQueueError extends Schema.TaggedErrorClass<...> {
  // _tag: "PersistedQueueError"; message: string; cause?: Schema.Defect
  readonly [ErrorTypeId]: ErrorTypeId
}

export declare class PersistedQueueStore extends Context.TagClass<
  PersistedQueueStore, "@effect/experimental/PersistedQueue/PersistedQueueStore", {
    readonly offer: (options: { readonly name: string; readonly id: string; readonly element: unknown; readonly isCustomId: boolean }) => Effect.Effect<void, PersistedQueueError>
    readonly take: (options: { readonly name: string; readonly maxAttempts: number }) => Effect.Effect<{ readonly id: string; readonly attempts: number; readonly element: unknown }, PersistedQueueError, Scope.Scope>
  }> {}
export const layerStoreMemory: Layer.Layer<PersistedQueueStore>
```

- The `host` offline command queue composes `layer` over a `PersistedQueueStore`; ordered drain on redial uses the FIFO `take` retry loop with capped `maxAttempts`.

---

## [5]-[REQUEST_RESOLVER]

```ts
// @effect/experimental/RequestResolver
export const dataLoader:
  ((options: { readonly window: Duration.DurationInput; readonly maxBatchSize?: number })
    => <A extends Request.Request<any, any>>(self: RequestResolver.RequestResolver<A, never>)
      => Effect.Effect<RequestResolver.RequestResolver<A, never>, never, Scope.Scope>)
  & (<A extends Request.Request<any, any>>(self: RequestResolver.RequestResolver<A, never>, options: { readonly window: Duration.DurationInput; readonly maxBatchSize?: number })
      => Effect.Effect<RequestResolver.RequestResolver<A, never>, never, Scope.Scope>)

export interface PersistedRequest<R, IE, E, IA, A>
  extends Request.Request<A, E>, Schema.WithResult<A, IA, E, IE, R> {}
export namespace PersistedRequest {
  type Any = PersistedRequest<any, any, any, any, any> | PersistedRequest<any, never, never, any, any>
}

export const persisted: {
  <Req extends PersistedRequest.Any>(options: { readonly storeId: string; readonly timeToLive: (...args: Persistence.ResultPersistence.TimeToLiveArgs<Req>) => Duration.DurationInput }):
    (self: RequestResolver.RequestResolver<Req, never>) => Effect.Effect<RequestResolver.RequestResolver<Req, Schema.WithResult.Context<Req>>, never, Persistence.ResultPersistence | Scope.Scope>
  <Req extends PersistedRequest.Any>(self: RequestResolver.RequestResolver<Req, never>, options: { readonly storeId: string; readonly timeToLive: (...args: Persistence.ResultPersistence.TimeToLiveArgs<Req>) => Duration.DurationInput }):
    Effect.Effect<RequestResolver.RequestResolver<Req, Schema.WithResult.Context<Req>>, never, Persistence.ResultPersistence | Scope.Scope>
}
```

- `dataLoader` and `persisted` are data-last/data-first dual combinators over the core `effect/RequestResolver`; `persisted` requires a `ResultPersistence` layer and a `PersistedRequest` schema.

---

## [6]-[RATE_LIMITER]

```ts
// @effect/experimental/RateLimiter
export const TypeId: TypeId
export type TypeId = "~@effect/experimental/RateLimiter"

export interface RateLimiter {
  readonly [TypeId]: TypeId
  readonly consume: (options: {
    readonly algorithm?: "fixed-window" | "token-bucket" | undefined
    readonly onExceeded?: "delay" | "fail" | undefined
    readonly window: Duration.DurationInput
    readonly limit: number
    readonly key: string
    readonly tokens?: number | undefined
  }) => Effect.Effect<ConsumeResult, RateLimiterError>
}
export const RateLimiter: Context.Tag<RateLimiter, RateLimiter>

export const make: Effect.Effect<RateLimiter, never, RateLimiterStore>
export const layer: Layer.Layer<RateLimiter, never, RateLimiterStore>
export const makeWithRateLimiter: Effect.Effect<((options: {
  readonly algorithm?: "fixed-window" | "token-bucket" | undefined
  readonly onExceeded?: "delay" | "fail" | undefined
  readonly window: Duration.DurationInput; readonly limit: number; readonly key: string; readonly tokens?: number | undefined
}) => <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E | RateLimiterError, R>), never, RateLimiter>
export const makeSleep: Effect.Effect<((options: {
  readonly algorithm?: "fixed-window" | "token-bucket" | undefined
  readonly window: Duration.DurationInput; readonly limit: number; readonly key: string; readonly tokens?: number | undefined
}) => Effect.Effect<ConsumeResult, RateLimitStoreError>), never, RateLimiter>

export const ErrorTypeId: ErrorTypeId
export type ErrorTypeId = "~@effect/experimental/RateLimiter/RateLimiterError"
export declare class RateLimitExceeded extends Schema.TaggedErrorClass<...> {
  // _tag: "RateLimiterError"; retryAfter: Schema.DurationFromMillis; key: string; limit: number; remaining: number
  readonly [ErrorTypeId]: ErrorTypeId
  readonly reason: "Exceeded"
  get message(): string
}
export declare class RateLimitStoreError extends Schema.TaggedErrorClass<...> {
  // _tag: "RateLimiterError"; message: string; cause?: Schema.Defect
  readonly [ErrorTypeId]: ErrorTypeId
  readonly reason: "StoreError"
}
export const RateLimiterError: Schema.Union<[typeof RateLimitExceeded, typeof RateLimitStoreError]>
export type RateLimiterError = RateLimitExceeded | RateLimitStoreError

export interface ConsumeResult {
  readonly delay: Duration.Duration       // Duration.zero when allowed immediately
  readonly limit: number
  readonly remaining: number
  readonly resetAfter: Duration.Duration
}

export declare class RateLimiterStore extends Context.TagClass<
  RateLimiterStore, "@effect/experimental/RateLimiter/RateLimiterStore", {
    readonly fixedWindow: (options: { readonly key: string; readonly tokens: number; readonly refillRate: Duration.Duration; readonly limit: number | undefined }) => Effect.Effect<readonly [count: number, ttl: number], RateLimiterError>
    readonly tokenBucket: (options: { readonly key: string; readonly tokens: number; readonly limit: number; readonly refillRate: Duration.Duration; readonly allowOverflow: boolean }) => Effect.Effect<number, RateLimiterError>
  }> {}
export const layerStoreMemory: Layer.Layer<RateLimiterStore>
```

- Both errors carry `_tag: "RateLimiterError"` discriminated by the `reason` field; `RateLimitExceeded.retryAfter` drives the `"delay"` strategy.

---

## [7]-[REACTIVITY]

```ts
// @effect/experimental/Reactivity
export declare class Reactivity extends Context.TagClass<Reactivity, "@effect/experimental/Reactivity", Reactivity.Service> {}
export const make: Effect.Effect<Reactivity.Service, never, never>
export const layer: Layer.Layer<Reactivity>

// data-last + data-first dual accessors; keys = array of arbitrary tokens or a record of token arrays
export const mutation: {
  (keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>): <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R | Reactivity>
  <A, E, R>(effect: Effect.Effect<A, E, R>, keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>): Effect.Effect<A, E, R | Reactivity>
}
export const query: {
  (keys: ...): <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<Mailbox.ReadonlyMailbox<A, E>, never, R | Scope.Scope | Reactivity>
  <A, E, R>(effect: Effect.Effect<A, E, R>, keys: ...): Effect.Effect<Mailbox.ReadonlyMailbox<A, E>, never, R | Scope.Scope | Reactivity>
}
export const stream: {
  (keys: ...): <A, E, R>(effect: Effect.Effect<A, E, R>) => Stream.Stream<A, E, Exclude<R, Scope.Scope> | Reactivity>
  <A, E, R>(effect: Effect.Effect<A, E, R>, keys: ...): Stream.Stream<A, E, Exclude<R, Scope.Scope> | Reactivity>
}
export const invalidate: (keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>) => Effect.Effect<void, never, Reactivity>

export namespace Reactivity {
  interface Service {
    readonly unsafeInvalidate: (keys: ...) => void
    readonly unsafeRegister: (keys: ..., handler: () => void) => () => void
    readonly invalidate: (keys: ...) => Effect.Effect<void>
    readonly mutation: <A, E, R>(keys: ..., effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
    readonly query: <A, E, R>(keys: ..., effect: Effect.Effect<A, E, R>) => Effect.Effect<Mailbox.ReadonlyMailbox<A, E>, never, R | Scope.Scope>
    readonly stream: <A, E, R>(keys: ..., effect: Effect.Effect<A, E, R>) => Stream.Stream<A, E, Exclude<R, Scope.Scope>>
  }
}
```

- `query` returns a `Mailbox.ReadonlyMailbox` that re-emits when any keyed `mutation`/`invalidate` fires on overlapping keys; `stream` is the same as an `effect/Stream`.

---

## [8]-[EVENT_AND_EVENT_GROUP]

```ts
// @effect/experimental/Event
export const TypeId: unique symbol
export type TypeId = typeof TypeId
export const isEvent: (u: unknown) => u is Event<any, any, any, any>

export interface Event<
  out Tag extends string,
  in out Payload extends Schema.Schema.Any = typeof Schema.Void,
  in out Success extends Schema.Schema.Any = typeof Schema.Void,
  in out Error extends Schema.Schema.All = typeof Schema.Never
> {
  readonly [TypeId]: TypeId
  readonly tag: Tag
  readonly primaryKey: (payload: Schema.Schema.Type<Payload>) => string
  readonly payload: Payload
  readonly payloadMsgPack: MsgPack.schema<Payload>
  readonly success: Success
  readonly error: Error
}
export interface EventHandler<in out Tag extends string> { readonly _: unique symbol; readonly tag: Tag }

export const make: <Tag extends string, Payload extends Schema.Schema.Any = typeof Schema.Void, Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never>(options: {
  readonly tag: Tag
  readonly primaryKey: (payload: Schema.Schema.Type<Payload>) => string
  readonly payload?: Payload
  readonly success?: Success
  readonly error?: Error
}) => Event<Tag, Payload, Success, Error>

// Event namespace type helpers (selected): Any, AnyWithProps, ToService, Tag, ErrorSchema, Error,
// AddError, PayloadSchema, Payload, TaggedPayload, SuccessSchema, Success, Context, WithTag,
// ExcludeTag, PayloadWithTag, SuccessWithTag, ErrorWithTag, ContextWithTag
```

```ts
// @effect/experimental/EventGroup
export const TypeId: unique symbol
export const isEventGroup: (u: unknown) => u is EventGroup.Any
export interface EventGroup<out Events extends Event.Any = never> extends Pipeable {
  new (_: never): {}
  readonly [TypeId]: TypeId
  readonly events: Record.ReadonlyRecord<string, Events>
  add<Tag extends string, Payload extends Schema.Schema.Any = typeof Schema.Void, Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never>(options: {
    readonly tag: Tag
    readonly primaryKey: (payload: Schema.Schema.Type<Payload>) => string
    readonly payload?: Payload; readonly success?: Success; readonly error?: Error
  }): EventGroup<Events | Event<Tag, Payload, Success, Error>>
  addError<Error extends Schema.Schema.Any>(error: Error): EventGroup<Event.AddError<Events, Error>>
}
export const empty: EventGroup<never>
// EventGroup namespace: Any, AnyWithProps, ToService, Events, Context
```

---

## [9]-[EVENT_JOURNAL]

```ts
// @effect/experimental/EventJournal
export declare class EventJournal extends Context.TagClass<EventJournal, "@effect/experimental/EventJournal", {
  readonly entries: Effect.Effect<ReadonlyArray<Entry>, EventJournalError>
  readonly write: <A, E, R>(options: { readonly event: string; readonly primaryKey: string; readonly payload: Uint8Array; readonly effect: (entry: Entry) => Effect.Effect<A, E, R> }) => Effect.Effect<A, EventJournalError | E, R>
  readonly writeFromRemote: (options: {
    readonly remoteId: RemoteId
    readonly entries: ReadonlyArray<RemoteEntry>
    readonly compact?: ((uncommitted: ReadonlyArray<RemoteEntry>) => Effect.Effect<ReadonlyArray<[compacted: ReadonlyArray<Entry>, remoteEntries: ReadonlyArray<RemoteEntry>]>, EventJournalError>) | undefined
    readonly effect: (options: { readonly entry: Entry; readonly conflicts: ReadonlyArray<Entry> }) => Effect.Effect<void, EventJournalError>
  }) => Effect.Effect<void, EventJournalError>
  readonly withRemoteUncommited: <A, E, R>(remoteId: RemoteId, f: (entries: ReadonlyArray<Entry>) => Effect.Effect<A, E, R>) => Effect.Effect<A, EventJournalError | E, R>
  readonly nextRemoteSequence: (remoteId: RemoteId) => Effect.Effect<number, EventJournalError>
  readonly changes: Effect.Effect<Queue.Dequeue<Entry>, never, Scope>
  readonly destroy: Effect.Effect<void, EventJournalError>
}> {}

export const ErrorTypeId: unique symbol
export declare class EventJournalError extends Schema.TaggedClass<...> {
  // _tag: "EventJournalError"; method: string; cause: Schema.Defect
  readonly [ErrorTypeId]: ErrorTypeId
}

// remote + entry id branded schemas
export const RemoteIdTypeId: unique symbol
export const RemoteId: Schema.brand<typeof Schema.Uint8ArrayFromSelf, typeof RemoteIdTypeId>
export type RemoteId = typeof RemoteId.Type
export const makeRemoteId: () => RemoteId
export const EntryIdTypeId: unique symbol
export const EntryId: Schema.brand<typeof Schema.Uint8ArrayFromSelf, typeof EntryIdTypeId>
export type EntryId = typeof EntryId.Type
export const makeEntryId: (options?: { msecs?: number }) => EntryId
export const entryIdMillis: (entryId: EntryId) => number

export declare class Entry extends Schema.Class<Entry, { id: EntryId; event: string; primaryKey: string; payload: Uint8Array }> {
  static arrayMsgPack: Schema.Array$<MsgPack.schema<typeof Entry>>
  static encodeArray: (a: readonly Entry[], overrideOptions?) => Effect.Effect<readonly Uint8Array[], ParseError, never>
  static decodeArray: (i: readonly Uint8Array[], overrideOptions?) => Effect.Effect<readonly Entry[], ParseError, never>
  get idString(): string
  get createdAtMillis(): number
  get createdAt(): DateTime.Utc
}
export declare class RemoteEntry extends Schema.Class<RemoteEntry, { remoteSequence: number; entry: Entry }> {}

// stores
export const makeMemory: Effect.Effect<typeof EventJournal.Service>
export const layerMemory: Layer.Layer<EventJournal>
export const makeIndexedDb: (options?: { readonly database?: string }) => Effect.Effect<typeof EventJournal.Service, EventJournalError, Scope>
export const layerIndexedDb: (options?: { readonly database?: string }) => Layer.Layer<EventJournal, EventJournalError>
```

- `layerIndexedDb` is the browser-tier durable journal; `layerMemory` is the test/ephemeral journal. Entry `id` is a ULID-style `Uint8Array` brand whose embedded timestamp is read via `entryIdMillis` / `createdAtMillis`.

---

## [10]-[EVENT_LOG]

```ts
// @effect/experimental/EventLog
export const SchemaTypeId: unique symbol
export const isEventLogSchema: (u: unknown) => u is EventLogSchema<any>
export interface EventLogSchema<Groups extends EventGroup.Any> {
  new (_: never): {}
  readonly [SchemaTypeId]: SchemaTypeId
  readonly groups: ReadonlyArray<Groups>
}
export const schema: <Groups extends ReadonlyArray<EventGroup.Any>>(...groups: Groups) => EventLogSchema<Groups[number]>

export const HandlersTypeId: unique symbol
export interface Handlers<R, Events extends Event.Any = never> extends Pipeable {
  readonly [HandlersTypeId]: { _Endpoints: Covariant<Events> }
  readonly group: EventGroup.AnyWithProps
  readonly handlers: Record.ReadonlyRecord<string, Handlers.Item<R>>
  readonly context: Context.Context<any>
  handle<Tag extends Events["tag"], R1>(name: Tag, handler: (options: {
    readonly payload: Event.PayloadWithTag<Events, Tag>
    readonly entry: Entry
    readonly conflicts: Array<{ readonly entry: Entry; readonly payload: Event.PayloadWithTag<Events, Tag> }>
  }) => Effect.Effect<Event.SuccessWithTag<Events, Tag>, Event.ErrorWithTag<Events, Tag>, R1>): Handlers<R | R1, Event.ExcludeTag<Events, Tag>>
}
// Handlers namespace: Any, Item<R>, ValidateReturn<A>, Error<A>, Context<A>

export const group: <Events extends Event.Any, Return>(group: EventGroup<Events>, f: (handlers: Handlers<never, Events>) => Handlers.ValidateReturn<Return>) => Layer.Layer<Event.ToService<Events>, Handlers.Error<Return>, Exclude<Handlers.Context<Return>, Scope>>
export const groupCompaction: <Events extends Event.Any, R>(group: EventGroup<Events>, effect: (options: {
  readonly primaryKey: string; readonly entries: Array<Entry>; readonly events: Array<Event.TaggedPayload<Events>>
  readonly write: <Tag extends Event.Tag<Events>>(tag: Tag, payload: Event.PayloadWithTag<Events, Tag>) => Effect.Effect<void>
}) => Effect.Effect<void, never, R>) => Layer.Layer<never, never, Identity | EventJournal | R | Event.Context<Events>>
export const groupReactivity: <Events extends Event.Any>(group: EventGroup<Events>, keys: { readonly [Tag in Event.Tag<Events>]?: ReadonlyArray<string> } | ReadonlyArray<string>) => Layer.Layer<never, never, Identity | EventJournal>

export declare class Registry extends Context.TagClass<Registry, "@effect/experimental/EventLog/Registry", {
  readonly add: (handlers: Handlers.Any) => Effect.Effect<void>
  readonly handlers: Effect.Effect<Record.ReadonlyRecord<string, Handlers.Item<any>>>
}> {
  static layer: Layer.Layer<Registry, never, never>
}

export declare class Identity extends Context.TagClass<Identity, "@effect/experimental/EventLog/Identity", {
  readonly publicKey: string; readonly privateKey: Redacted.Redacted<Uint8Array>
}> {
  static makeRandom(): { readonly publicKey: string; readonly privateKey: Redacted.Redacted<Uint8Array> }
  static readonly Schema: Schema.Struct<{ publicKey: typeof Schema.String; privateKey: Schema.Redacted<...> }>
  static readonly SchemaFromString: Schema.transform<...>
  static decodeString: (s: string) => Identity["Type"]
  static encodeString: (identity: Identity["Type"]) => string
}
export const layerIdentityKvs: (options: { readonly key: string }) => Layer.Layer<Identity, ParseResult.ParseError | Error.PlatformError, KeyValueStore.KeyValueStore>

export declare class EventLog extends Context.TagClass<EventLog, "@effect/experimental/EventLog/EventLog", {
  readonly write: <Groups extends EventGroup.Any, Tag extends Event.Tag<EventGroup.Events<Groups>>>(options: {
    readonly schema: EventLogSchema<Groups>; readonly event: Tag; readonly payload: Event.PayloadWithTag<EventGroup.Events<Groups>, Tag>
  }) => Effect.Effect<Event.SuccessWithTag<EventGroup.Events<Groups>, Tag>, Event.ErrorWithTag<EventGroup.Events<Groups>, Tag> | EventJournalError>
  readonly registerRemote: (remote: EventLogRemote) => Effect.Effect<void, never, Scope>
  readonly registerCompaction: (options: { readonly events: ReadonlyArray<string>; readonly effect: (options: { readonly entries: ReadonlyArray<Entry>; readonly write: (entry: Entry) => Effect.Effect<void> }) => Effect.Effect<void> }) => Effect.Effect<void, never, Scope>
  readonly registerReactivity: (keys: Record<string, ReadonlyArray<string>>) => Effect.Effect<void, never, Scope>
  readonly entries: Effect.Effect<ReadonlyArray<Entry>, EventJournalError>
  readonly destroy: Effect.Effect<void, EventJournalError>
}> {}

export const layerEventLog: Layer.Layer<EventLog, never, EventJournal | Identity>
export const layer: <Groups extends EventGroup.Any>(_schema: EventLogSchema<Groups>) => Layer.Layer<EventLog, never, EventGroup.ToService<Groups> | EventJournal | Identity>
export const makeClient: <Groups extends EventGroup.Any>(schema: EventLogSchema<Groups>) => Effect.Effect<(<Tag extends Event.Tag<EventGroup.Events<Groups>>>(event: Tag, payload: Event.PayloadWithTag<EventGroup.Events<Groups>, Tag>) => Effect.Effect<Event.SuccessWithTag<EventGroup.Events<Groups>, Tag>, Event.ErrorWithTag<EventGroup.Events<Groups>, Tag> | EventJournalError>), never, EventLog>
```

- `EventLog.layer(schema)` is the full composition; it requires `EventJournal` (storage) + `Identity` (signing key) + the per-group handler services produced by `group(...)`.

---

## [11]-[EVENT_LOG_REMOTE_SERVER_ENCRYPTION]

```ts
// @effect/experimental/EventLogRemote — client-side sync
export interface EventLogRemote {
  readonly id: RemoteId
  readonly changes: (identity: typeof Identity.Service, startSequence: number) => Effect.Effect<Mailbox.ReadonlyMailbox<RemoteEntry>, never, Scope.Scope>
  readonly write: (identity: typeof Identity.Service, entries: ReadonlyArray<Entry>) => Effect.Effect<void>
}
// protocol tagged classes: Hello, ChunkedMessage(.split/.join), WriteEntries, Ack, RequestChanges,
//   Changes, StopChanges, Ping, Pong, RemoteAdditions (note _tag literal "RemoveAdditions")
export const ProtocolRequest: Schema.Union<[WriteEntries, RequestChanges, StopChanges, ChunkedMessage, Ping]>
export const ProtocolRequestMsgPack: MsgPack.schema<typeof ProtocolRequest>
export const ProtocolResponse: Schema.Union<[Hello, Ack, Changes, ChunkedMessage, Pong]>
export const ProtocolResponseMsgPack: MsgPack.schema<typeof ProtocolResponse>
export const decodeRequest / encodeRequest / decodeResponse / encodeResponse: (i/a: Uint8Array, overrideOptions?) => ...
export const fromSocket: (options?: { readonly disablePing?: boolean }) => Effect.Effect<void, never, Scope.Scope | EventLog | EventLogEncryption | Socket.Socket>
export const fromWebSocket: (url: string, options?: { readonly disablePing?: boolean }) => Effect.Effect<void, never, Scope.Scope | EventLogEncryption | EventLog | Socket.WebSocketConstructor>
export const layerWebSocket: (url: string, options?: { readonly disablePing?: boolean }) => Layer.Layer<never, never, Socket.WebSocketConstructor | EventLog | EventLogEncryption>
export const layerWebSocketBrowser: (url: string, options?: { readonly disablePing?: boolean }) => Layer.Layer<never, never, EventLog>
```

```ts
// @effect/experimental/EventLogServer — server-side storage + handlers
export const makeHandler: Effect.Effect<(socket: Socket.Socket) => Effect.Effect<void, Socket.SocketError, Scope.Scope>, never, Storage>
export const makeHandlerHttp: Effect.Effect<Effect.Effect<HttpServerResponse.HttpServerResponse, HttpServerError.RequestError | Socket.SocketError, HttpServerRequest.HttpServerRequest | Scope.Scope>, never, Storage>
export declare class PersistedEntry extends Schema.Class<PersistedEntry, { entryId: EntryId; iv: Uint8Array; encryptedEntry: Uint8Array }> {
  static fromMsgPack: MsgPack.schema<typeof PersistedEntry>
  static encode: (a: PersistedEntry, overrideOptions?) => Uint8Array
  get entryIdString(): string
}
export declare class Storage extends Context.TagClass<Storage, "@effect/experimental/EventLogServer/Storage", {
  readonly getId: Effect.Effect<RemoteId>
  readonly write: (publicKey: string, entries: ReadonlyArray<PersistedEntry>) => Effect.Effect<ReadonlyArray<EncryptedRemoteEntry>>
  readonly entries: (publicKey: string, startSequence: number) => Effect.Effect<ReadonlyArray<EncryptedRemoteEntry>>
  readonly changes: (publicKey: string, startSequence: number) => Effect.Effect<Mailbox.ReadonlyMailbox<EncryptedRemoteEntry>, never, Scope.Scope>
}> {}
export const makeStorageMemory: Effect.Effect<typeof Storage.Service, never, Scope.Scope>
export const layerStorageMemory: Layer.Layer<Storage>
```

```ts
// @effect/experimental/EventLogEncryption — subtle-crypto rail
export const EncryptedEntry: Schema.Struct<{ entryId: EntryId; encryptedEntry: Uint8Array }>
export const EncryptedRemoteEntry: Schema.Struct<{ sequence: number; iv: Uint8Array; entryId: EntryId; encryptedEntry: Uint8Array }>
export interface EncryptedRemoteEntry extends Schema.Schema.Type<typeof EncryptedRemoteEntry> {}
export declare class EventLogEncryption extends Context.TagClass<EventLogEncryption, "@effect/experimental/EventLogEncryption", {
  readonly encrypt: (identity: typeof Identity.Service, entries: ReadonlyArray<Entry>) => Effect.Effect<{ readonly iv: Uint8Array; readonly encryptedEntries: ReadonlyArray<Uint8Array> }>
  readonly decrypt: (identity: typeof Identity.Service, entries: ReadonlyArray<EncryptedRemoteEntry>) => Effect.Effect<Array<RemoteEntry>>
  readonly sha256String: (data: Uint8Array) => Effect.Effect<string>
  readonly sha256: (data: Uint8Array) => Effect.Effect<Uint8Array>
}> {}
export const makeEncryptionSubtle: (crypto: Crypto) => Effect.Effect<typeof EventLogEncryption.Service>
export const layerSubtle: Layer.Layer<EventLogEncryption>
```

- `fromWebSocket` / `layerWebSocket` need `EventLog` + `EventLogEncryption` + a `Socket.WebSocketConstructor`; `layerWebSocketBrowser` packs the browser socket + subtle-crypto so its only requirement is `EventLog`. Note the typo'd `_tag: "RemoveAdditions"` literal on `RemoteAdditions` (verbatim in the published types).

---

## [12]-[MACHINE]

```ts
// @effect/experimental/Machine
export const TypeId: unique symbol
export interface Machine<State, Public extends Procedure.TaggedRequest.Any, Private extends Procedure.TaggedRequest.Any, Input, InitErr, R> extends Pipeable {
  readonly [TypeId]: TypeId
  readonly initialize: Machine.Initialize<Input, State, Public, Private, R, InitErr, R>
  readonly retryPolicy: Schedule.Schedule<unknown, InitErr | MachineDefect, R> | undefined
}
export const SerializableTypeId: unique symbol
export interface SerializableMachine<State, Public extends Schema.TaggedRequest.All, Private extends Schema.TaggedRequest.All, Input, InitErr, R, SR> extends Machine<State, Public, Private, Input, InitErr, R> {
  readonly [SerializableTypeId]: SerializableTypeId
  readonly schemaInput: Schema.Schema<Input, unknown, SR>
  readonly schemaState: Schema.Schema<State, unknown, SR>
}
export const ActorTypeId: unique symbol
export declare class MachineDefect extends Schema.TaggedErrorClass<...> {
  // _tag: "MachineDefect"; cause: Schema.Defect
  static wrap<A, E, R>(effect: Effect.Effect<A, E, R>): Effect.Effect<A, MachineDefect, R>
}
export declare class MachineContext extends Context.TagClass<MachineContext, "@effect/experimental/Machine/Context", Procedure.Procedure.BaseContext> {}

export interface Actor<M extends Machine.Any> extends Subscribable.Subscribable<Machine.State<M>> {
  readonly [ActorTypeId]: ActorTypeId
  readonly machine: M
  readonly input: Machine.Input<M>
  readonly send: <Req extends Machine.Public<M>>(request: Req) => Effect.Effect<Request.Success<Req>, Request.Error<Req>>
  readonly join: Effect.Effect<never, Machine.InitError<M> | MachineDefect>
}
export interface SerializableActor<M extends Machine.Any> extends Actor<M> {
  readonly sendUnknown: (request: unknown) => Effect.Effect<Schema.ExitEncoded<unknown, unknown, unknown>, ParseResult.ParseError>
}

export const make: {
  <State, Public extends Procedure.TaggedRequest.Any, Private extends Procedure.TaggedRequest.Any, InitErr, R>(initialize: Effect.Effect<ProcedureList<State, Public, Private, R>, InitErr, R>): Machine<State, Public, Private, void, InitErr, Exclude<R, Scope.Scope | MachineContext>>
  <State, Public extends Procedure.TaggedRequest.Any, Private extends Procedure.TaggedRequest.Any, Input, InitErr, R>(initialize: Machine.Initialize<Input, State, Public, Private, R, InitErr, R>): Machine<State, Public, Private, Input, InitErr, Exclude<R, Scope.Scope | MachineContext>>
}
export const makeWith: <State, Input = void>() => { /* two overloads mirroring make */ }
export const makeSerializable: { /* state-only + state+input overloads */ }
export const retry: {
  <M extends Machine.Any, Out, In extends Machine.InitError<M> | MachineDefect, R>(policy: Schedule.Schedule<Out, In, R>): (self: M) => Machine.AddContext<M, R>
  <M extends Machine.Any, Out, In extends Machine.InitError<M> | MachineDefect, R>(self: M, policy: Schedule.Schedule<Out, In, R>): Machine.AddContext<M, R>
}
export const currentTracingEnabled: FiberRef.FiberRef<boolean>
export const withTracingEnabled: { (enabled: boolean): <A,E,R>(effect) => Effect; <A,E,R>(effect, enabled: boolean): Effect }
export const boot: <M extends Machine.Any>(self: M, ...[input, options]: ...) => Effect.Effect<M extends { readonly [SerializableTypeId]: SerializableTypeId } ? SerializableActor<M> : Actor<M>, never, Machine.Context<M> | Scope.Scope>
export const snapshot: <State, Public, Private, Input, InitErr, R, SR>(self: Actor<SerializableMachine<...>>) => Effect.Effect<[input: unknown, state: unknown], ParseResult.ParseError, SR>
export const restore: <State, Public, Private, Input, InitErr, R, SR>(self: SerializableMachine<...>, snapshot: readonly [input: unknown, state: unknown]) => Effect.Effect<Actor<SerializableMachine<...>>, ParseResult.ParseError, R | SR>

// Machine namespace: Any, Initialize, InitializeSerializable, Public, Private, State, InitError, Context, Input, AddContext
// re-exports: export * as procedures from "./Machine/ProcedureList.js"
//             export * as serializable from "./Machine/SerializableProcedureList.js"
//             export { NoReply } from "./Machine/Procedure.js"
```

```ts
// @effect/experimental/Machine/Procedure
export interface TaggedRequest<Tag extends string, A, E> extends Request<A, E>
export interface Procedure<Request extends TaggedRequest.Any, State, R> extends Pipeable
export interface SerializableProcedure<Request extends Schema.TaggedRequest.All, State, R> extends Procedure<Request, State, R>
export const NoReply: unique symbol; export type NoReply = typeof NoReply
export const isSerializable: (u: unknown) => u is SerializableProcedure<any, any, any>
export type Handler<Request extends TaggedRequest.Any, State, Requests extends TaggedRequest.Any, R> =
  (context: Procedure.Context<Requests | Request, Request, State>) => Effect.Effect<readonly [response: Request.Success<Request> | NoReply, state: State], Request.Error<Request>, R>
export const make: <Requests extends TaggedRequest.Any, State>() => <Req extends TaggedRequest.Any>() => <R>(tag: Req["_tag"], handler: Handler<Req, State, Requests, R>) => Procedure<Req, State, R>
export const makeSerializable: <Requests extends TaggedRequest.Any, State>() => <Req extends Schema.TaggedRequest.All, IS, R, RS>(schema, handler) => SerializableProcedure<...>

// @effect/experimental/Machine/ProcedureList — make, add, addPrivate, addProcedure, addProcedurePrivate, withInitialState
// @effect/experimental/Machine/SerializableProcedureList — make, add, addPrivate, withInitialState
```

- `boot` discriminates the actor type on the presence of `SerializableTypeId`; a serializable machine boots a `SerializableActor` whose state survives `snapshot`/`restore`.

---

## [13]-[SSE]

```ts
// @effect/experimental/Sse
export const makeChannel: <IE, Done>(options?: { readonly bufferSize?: number }) => Channel.Channel<Chunk.Chunk<Event>, Chunk.Chunk<string>, IE, IE, void, Done>
export function makeParser(onParse: (event: AnyEvent) => void): Parser
export interface Parser { feed(chunk: string): void; reset(): void }
export interface Encoder { write(event: AnyEvent): string }
export interface Event { readonly _tag: "Event"; readonly event: string; readonly id: string | undefined; readonly data: string }
export interface EventEncoded { readonly event: string; readonly id: string | undefined; readonly data: string }
export const RetryTypeId: unique symbol
export declare class Retry { readonly [RetryTypeId]: RetryTypeId; readonly _tag: "Retry"; readonly duration: Duration.Duration; readonly lastEventId: string | undefined; static is(u: unknown): u is Retry }
export type AnyEvent = Event | Retry
export const encoder: Encoder
```

---

## [14]-[DEV_TOOLS]

```ts
// @effect/experimental/DevTools — tracing client layers
export const layerSocket: Layer.Layer<never, never, Socket.Socket>
export const layerWebSocket: (url?: string) => Layer.Layer<never, never, Socket.WebSocketConstructor>
export const layer: (url?: string) => Layer.Layer<never>
```

---

## [15]-[VARIANT_SCHEMA]

`VariantSchema.make` returns a builder whose `Struct` / `Field` / `Class` / `Union` produce schemas
that project to a different `Schema.Struct` per declared variant (the same fields encode/decode
differently per variant). This is the engine behind `@effect/sql`'s `Model` variants (insert /
select / update / json).

```ts
// @effect/experimental/VariantSchema
export const TypeId: unique symbol
export interface Struct<in out A extends Field.Fields> extends Pipeable { readonly [TypeId]: A }
export const isStruct: (u: unknown) => u is Struct<any>
export const FieldTypeId: unique symbol
export interface Field<in out A extends Field.Config> extends Pipeable { readonly [FieldTypeId]: FieldTypeId; readonly schemas: A }
export const isField: (u: unknown) => u is Field<any>
export const fields: <A extends Struct<any>>(self: A) => A[TypeId]

export type ExtractFields<V extends string, Fields extends Struct.Fields, IsDefault = false> = ...
export type Extract<V extends string, A extends Struct<any>, IsDefault = false> = ...

export interface Class<Self, Fields extends Struct.Fields, SchemaFields extends Schema.Struct.Fields, A, I, R, C>
  extends Schema.Schema<Self, Schema.Simplify<I>, R>, Struct<Schema.Simplify<Fields>> {
  new (props: ..., options?: { readonly disableValidation?: boolean }): A
  readonly ast: AST.Transformation
  make<Args extends Array<any>, X>(this: { new (...args: Args): X }, ...args: Args): X
  annotations(annotations: Schema.Annotations.Schema<Self>): Schema.SchemaClass<Self, I, R>
  readonly identifier: string
  readonly fields: Schema.Simplify<SchemaFields>
}
export interface Union<Members extends ReadonlyArray<Struct<any>>> extends Schema.Union<...> {}
export namespace Union { type Variants<Members, Variants extends string> = ... }
export interface fromKey<S extends Schema.Schema.All, Key extends string> extends Schema.PropertySignature<...> {}
export namespace fromKey { type Rename<S, Key extends string> = ... }

export const make: <const Variants extends ReadonlyArray<string>, const Default extends Variants[number]>(options: {
  readonly variants: Variants
  readonly defaultVariant: Default
}) => {
  readonly Struct: <const A extends Struct.Fields>(fields: A & Struct.Validate<A, Variants[number]>) => Struct<A>
  readonly Field: <const A extends Field.ConfigWithKeys<Variants[number]>>(config: A & { readonly [K in Exclude<keyof A, Variants[number]>]: never }) => Field<A>
  readonly FieldOnly: <const Keys extends ReadonlyArray<Variants[number]>>(...keys: Keys) => <S extends ...>(schema: S) => Field<{ readonly [K in Keys[number]]: S }>
  readonly FieldExcept: <const Keys extends ReadonlyArray<Variants[number]>>(...keys: Keys) => <S extends ...>(schema: S) => Field<{ readonly [K in Exclude<Variants[number], Keys[number]>]: S }>
  readonly fieldEvolve: { /* data-last + data-first */ }
  readonly fieldFromKey: { /* data-last + data-first */ }
  readonly Class: <Self = never>(identifier: string) => <const Fields extends Struct.Fields>(fields: Fields & Struct.Validate<Fields, Variants[number]>, annotations?: Schema.Annotations.Schema<Self>) => [Self] extends [never] ? MissingSelfGeneric : ClassFromFields<...> & { readonly [V in Variants[number]]: Extract<V, Struct<Fields>> }
  readonly Union: <const Members extends ReadonlyArray<Struct<any>>>(...members: Members) => Union<Members> & Union.Variants<Members, Variants[number]>
  readonly extract: { <V extends Variants[number]>(variant: V): <A extends Struct<any>>(self: A) => Extract<...>; <V, A>(self: A, variant: V): Extract<...> }
}

// overrideable property signatures
export const Override: <A>(value: A) => A & Brand<"Override">
export interface Overrideable<To, From, R = never> extends Schema.PropertySignature<":", (To & Brand<"Override">) | undefined, never, ":", From, true, R> {}
export const Overrideable: <From, IFrom, RFrom, To, ITo, R>(from: Schema.Schema<From, IFrom, RFrom>, to: Schema.Schema<To, ITo>, options: {
  readonly generate: (_: Option.Option<ITo>) => Effect.Effect<From, ParseResult.ParseIssue, R>
  readonly decode?: Schema.Schema<ITo, From>
  readonly constructorDefault?: () => To
}) => Overrideable<To, IFrom, RFrom | R>
```

---

## [16]-[IMPLEMENTATION_LAW]

[DURABLE_SUBSTRATE]:
- `node-durable` builds the persistence substrate on `Persistence.ResultPersistence` (`layerResult` over a `BackingPersistence`); `PersistedCache.make` and `RequestResolver.persisted` are the two memoization rails over it, both keyed by a `Persistable`/`PersistedRequest` schema (`Schema.WithResult` + `PrimaryKey`).
- The backing layer choice is the only swap point: `layerMemory` (test), `layerKeyValueStore` (KVS-backed), with `layerResultMemory` / `layerResultKeyValueStore` as the pre-composed result-store variants. A hand-rolled key-value cache outside `BackingPersistenceStore` is the deleted form.
- `PersistedQueue` (`layer` over `PersistedQueueStore`, `layerStoreMemory` for tests) is the durable work queue; ordered FIFO `take` with `maxAttempts` retry replaces any bespoke queue table.

[OFFLINE_HOST]:
- The browser `host` snapshot store composes `Persistence.layerKeyValueStore` over `@effect/platform-browser`'s `KeyValueStore` binding so a snapshot persists as a Schema-encoded value; the offline command queue drains via `PersistedQueue`. A direct `localStorage` read or a JSON blob outside the encoded store is the deleted form.
- The local event log uses `EventJournal.layerIndexedDb` + `EventLog.layer(schema)` + `Identity` (`layerIdentityKvs` to persist the signing key in KVS); remote sync attaches via `EventLogRemote.layerWebSocketBrowser` which folds the browser socket and subtle-crypto so only `EventLog` remains as a requirement.

[RATE_AND_REACTIVITY]:
- `RateLimiter.layer` over `RateLimiterStore` (`layerStoreMemory` for single-node) gates outbound calls; `makeWithRateLimiter` wraps an effect, `makeSleep` only delays. Both errors carry `_tag: "RateLimiterError"` discriminated by `reason`.
- `Reactivity.layer` backs key-scoped cache invalidation; `query` returns a `Mailbox.ReadonlyMailbox` that re-emits on overlapping-key `mutation`/`invalidate`, and `EventLog.groupReactivity` wires journal writes into the same key space.

[RAIL_LAW]:
- `Persistence`, `PersistedCache`, `PersistedQueue`, `RequestResolver`, `RateLimiter`, `Machine`, `EventLogServer` are node-tier or shared; `EventJournal.layerIndexedDb`, `EventLogRemote.layerWebSocketBrowser`, `EventLogEncryption.layerSubtle` are browser-tier offline surfaces.
- `DevTools` layers are dev-only tracing clients; not composed in production runtime.
- Every error rail is a `Schema.TaggedError`/`TaggedClass` (`PersistenceError`, `PersistedQueueError`, `EventJournalError`, `RateLimiterError`, `MachineDefect`) — domain code matches on `_tag`/`reason`, never on string inspection.
