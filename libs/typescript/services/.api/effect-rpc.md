# [API_CATALOGUE] @effect/rpc

`@effect/rpc` is the schema-driven, transport-agnostic RPC surface for Effect. The package re-exports twelve modules as namespaces through `index.d.ts`: procedure definition (`Rpc`), group aggregate (`RpcGroup`), typed client (`RpcClient`), server runner (`RpcServer`), wire-message algebra (`RpcMessage`), middleware (`RpcMiddleware`), stream schema (`RpcSchema`), serialization layers (`RpcSerialization`), client error (`RpcClientError`), test client (`RpcTest`), and worker bootstrap (`RpcWorker`).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/rpc`
- package: `@effect/rpc`
- entry: `@effect/rpc` (namespace barrel) plus per-module deep paths `@effect/rpc/Rpc`, `@effect/rpc/RpcGroup`, `@effect/rpc/RpcClient`, `@effect/rpc/RpcClientError`, `@effect/rpc/RpcMessage`, `@effect/rpc/RpcMiddleware`, `@effect/rpc/RpcSchema`, `@effect/rpc/RpcSerialization`, `@effect/rpc/RpcServer`, `@effect/rpc/RpcTest`, `@effect/rpc/RpcWorker`
- asset: procedure atom (`Rpc`), procedure aggregate (`RpcGroup`), typed client (`RpcClient`), server runner (`RpcServer`), wire-message algebra (`RpcMessage`), middleware tag family (`RpcMiddleware`), stream schema (`RpcSchema`), serialization layers (`RpcSerialization`), client error (`RpcClientError`), test client (`RpcTest`), worker bootstrap (`RpcWorker`)
- rail: internal-rpc
- peer: `effect`, `@effect/platform`, `msgpackr`

The `index.d.ts` barrel is the namespace-aggregating entry:

```ts contract
export * as Rpc from "./Rpc.js"
export * as RpcClient from "./RpcClient.js"
export * as RpcClientError from "./RpcClientError.js"
export * as RpcGroup from "./RpcGroup.js"
export * as RpcMessage from "./RpcMessage.js"
export * as RpcMiddleware from "./RpcMiddleware.js"
export * as RpcSchema from "./RpcSchema.js"
export * as RpcSerialization from "./RpcSerialization.js"
export * as RpcServer from "./RpcServer.js"
export * as RpcTest from "./RpcTest.js"
export * as RpcWorker from "./RpcWorker.js"
```

## [2]-[RPC]

`@effect/rpc/Rpc` — the procedure definition atom. `Rpc` carries the tag, payload/success/error schemas,
defect schema, annotations, and middleware set; the `make` constructor builds one, and the type-level
projection family derives handler shapes, exit schemas, and context requirements.

[PUBLIC_TYPE_SCOPE]: procedure model and constructors
- rail: internal-rpc

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [RAIL]                                                |
| :-----: | :----------------------- | :------------------- | :---------------------------------------------------- |
|   [1]   | `Rpc`                    | interface (5 params) | procedure atom; fluent `set*`/`prefix`/`middleware`   |
|   [2]   | `Handler`                | interface            | implemented rpc (`tag` + handler fn + context)        |
|   [3]   | `Any` / `AnyWithProps`   | interface            | erased procedure constraints                          |
|   [4]   | `From`                   | interface            | rpc derived from a tagged-request schema              |
|   [5]   | `AnySchema`              | interface            | structural schema bound (Type/Encoded/Context/ast)    |
|   [6]   | `AnyTaggedRequestSchema` | interface            | `AnySchema` + `_tag`/`success`/`failure`              |
|   [7]   | `make`                   | const fn             | procedure constructor                                 |
|   [8]   | `fromTaggedRequest`      | const fn             | `(schema) => From<S>`                                 |
|   [9]   | `exitSchema`             | const fn             | `(self) => Schema.Schema<Exit<R>, ExitEncoded<R>, …>` |
|  [10]   | `isRpc`                  | guard                | `(u) => u is Rpc<any, any, any>`                      |
|  [11]   | `TypeId`                 | unique symbol        | brand                                                 |

[PUBLIC_TYPE_SCOPE]: type-level projections (derive shapes from a procedure type `R`)
- rail: internal-rpc

| [INDEX] | [SYMBOL]                                                   | [RESOLVES_TO]                                          |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------- |
|   [1]   | `Tag<R>`                                                   | the procedure tag string                               |
|   [2]   | `Payload<R>` / `PayloadConstructor<R>`                     | payload `Type` / struct constructor input              |
|   [3]   | `Success<R>` / `SuccessSchema<R>` / `SuccessEncoded<R>`    | success value / schema / encoded                       |
|   [4]   | `SuccessExit<R>` / `SuccessExitEncoded<R>`                 | exit success (`void` for streams)                      |
|   [5]   | `SuccessChunk<R>` / `SuccessChunkEncoded<R>`               | per-chunk stream element                               |
|   [6]   | `Error<R>` / `ErrorSchema<R>` / `ErrorEncoded<R>`          | error value / schema (incl. middleware) / encoded      |
|   [7]   | `ErrorExit<R>` / `ErrorExitEncoded<R>`                     | exit error (incl. stream failure)                      |
|   [8]   | `Exit<R>` / `ExitEncoded<R>`                               | `Exit_<SuccessExit, ErrorExit>` / schema-encoded exit  |
|   [9]   | `Context<R>`                                               | payload+success+error schema context union             |
|  [10]   | `Middleware<R>` / `MiddlewareClient<R>`                    | middleware tag identifier / client-required identifier |
|  [11]   | `AddError<R, E>` / `AddMiddleware<R, M>`                   | widen the error / middleware param                     |
|  [12]   | `ToHandler<R>` / `ToHandlerFn<Current, R>`                 | `Handler<Tag>` / the implementor function shape        |
|  [13]   | `ResultFrom<R, Context>`                                   | `Effect`/`Stream`/`Mailbox` result a handler returns   |
|  [14]   | `IsStream<R, Tag>` / `ExtractTag<R, Tag>`                  | stream predicate / tag extractor                       |
|  [15]   | `ExtractProvides<R, Tag>` / `ExcludeProvides<Env, R, Tag>` | middleware-provided id / excl.                         |
|  [16]   | `Prefixed<Rpcs, Prefix>`                                   | re-tag with a prefix                                   |

```ts contract
// @effect/rpc/Rpc
const TypeId: unique symbol
type TypeId = typeof TypeId

const isRpc: (u: unknown) => u is Rpc<any, any, any>

interface Rpc<
  in out Tag extends string,
  out Payload extends AnySchema = typeof Schema.Void,
  out Success extends Schema.Schema.Any = typeof Schema.Void,
  out Error extends Schema.Schema.All = typeof Schema.Never,
  out Middleware extends RpcMiddleware.TagClassAny = never
> extends Pipeable {
  new (_: never): {}
  readonly [TypeId]: TypeId
  readonly _tag: Tag
  readonly key: string
  readonly payloadSchema: Payload
  readonly successSchema: Success
  readonly errorSchema: Error
  readonly defectSchema: Schema.Schema<unknown, any>
  readonly annotations: Context.Context<never>
  readonly middlewares: ReadonlySet<Middleware>
  setSuccess<S extends Schema.Schema.Any>(schema: S): Rpc<Tag, Payload, S, Error, Middleware>
  setError<E extends Schema.Schema.Any>(schema: E): Rpc<Tag, Payload, Success, E, Middleware>
  setPayload<P extends Schema.Struct<any> | Schema.Struct.Fields>(
    schema: P
  ): Rpc<Tag, P extends Schema.Struct<infer _> ? P : P extends Schema.Struct.Fields ? Schema.Struct<P> : never, Success, Error, Middleware>
  middleware<M extends RpcMiddleware.TagClassAny>(middleware: M): Rpc<Tag, Payload, Success, Error, Middleware | M>
  prefix<const Prefix extends string>(prefix: Prefix): Rpc<`${Prefix}${Tag}`, Payload, Success, Error, Middleware>
  annotate<I, S>(tag: Context.Tag<I, S>, value: S): Rpc<Tag, Payload, Success, Error, Middleware>
  annotateContext<I>(context: Context.Context<I>): Rpc<Tag, Payload, Success, Error, Middleware>
}

interface Handler<Tag extends string> {
  readonly _: unique symbol
  readonly tag: Tag
  readonly handler: (request: any, options: { readonly clientId: number; readonly headers: Headers }) => Effect<any, any> | Stream<any, any>
  readonly context: Context<never>
}

interface Any extends Pipeable {
  readonly [TypeId]: TypeId
  readonly _tag: string
  readonly key: string
}

interface AnyWithProps {
  readonly [TypeId]: TypeId
  readonly _tag: string
  readonly key: string
  readonly payloadSchema: AnySchema
  readonly successSchema: Schema.Schema.Any
  readonly errorSchema: Schema.Schema.All
  readonly defectSchema: Schema.Schema<unknown, any>
  readonly annotations: Context.Context<never>
  readonly middlewares: ReadonlySet<RpcMiddleware.TagClassAnyWithProps>
}

interface From<S extends AnyTaggedRequestSchema> extends Rpc<S["_tag"], S, S["success"], S["failure"]> {}

interface AnySchema extends Pipeable {
  readonly [Schema.TypeId]: any
  readonly Type: any
  readonly Encoded: any
  readonly Context: any
  readonly make?: (params: any, ...rest: ReadonlyArray<any>) => any
  readonly ast: AST.AST
  readonly annotations: any
}

interface AnyTaggedRequestSchema extends AnySchema {
  readonly _tag: string
  readonly success: Schema.Schema.Any
  readonly failure: Schema.Schema.All
}

const make: <
  const Tag extends string,
  Payload extends Schema.Schema.Any | Schema.Struct.Fields = typeof Schema.Void,
  Success extends Schema.Schema.Any = typeof Schema.Void,
  Error extends Schema.Schema.All = typeof Schema.Never,
  const Stream extends boolean = false
>(tag: Tag, options?: {
  readonly payload?: Payload
  readonly success?: Success
  readonly error?: Error
  readonly stream?: Stream
  readonly defect?: Schema.Schema<unknown, any>
  readonly primaryKey?: [Payload] extends [Schema.Struct.Fields] ? ((payload: Schema.Simplify<Schema.Struct.Type<NoInfer<Payload>>>) => string) : never
}) => Rpc<
  Tag,
  Payload extends Schema.Struct.Fields ? Schema.Struct<Payload> : Payload,
  Stream extends true ? RpcSchema.Stream<Success, Error> : Success,
  Stream extends true ? typeof Schema.Never : Error
>

const fromTaggedRequest: <S extends AnyTaggedRequestSchema>(schema: S) => From<S>
const exitSchema: <R extends Any>(self: R) => Schema.Schema<Exit<R>, ExitEncoded<R>, Context<R>>
```

[PUBLIC_TYPE_SCOPE]: response wrapper (fork / uninterruptible)
- rail: internal-rpc

```ts contract
// @effect/rpc/Rpc — Wrapper
const WrapperTypeId: unique symbol
type WrapperTypeId = typeof WrapperTypeId

interface Wrapper<A> {
  readonly [WrapperTypeId]: WrapperTypeId
  readonly value: A
  readonly fork: boolean
  readonly uninterruptible: boolean
}

const isWrapper: (u: object) => u is Wrapper<any>
const wrap: (options: { readonly fork?: boolean | undefined; readonly uninterruptible?: boolean | undefined }) => <A extends object>(value: A) => A extends Wrapper<infer _> ? A : Wrapper<A>
const fork: <A extends object>(value: A) => A extends Wrapper<infer _> ? A : Wrapper<A>
const uninterruptible: <A extends object>(value: A) => A extends Wrapper<infer _> ? A : Wrapper<A>
```

`fork` wraps a response `Effect`/`Stream` to force concurrent execution regardless of the `RpcServer`
concurrency setting; `uninterruptible` wraps it into an uninterruptible region.

## [3]-[RPC_GROUP]

`@effect/rpc/RpcGroup` — the procedure aggregate. A group accumulates `Rpc` definitions, applies
middleware/prefixes across them, and produces handler contexts and layers. `InternalRpc` holds one
`RpcGroup` as its `group` field.

```ts contract
// @effect/rpc/RpcGroup
const TypeId: unique symbol
type TypeId = typeof TypeId

interface RpcGroup<in out R extends Rpc.Any> extends Pipeable {
  new (_: never): {}
  readonly [TypeId]: TypeId
  readonly requests: ReadonlyMap<string, R>
  readonly annotations: Context.Context<never>
  add<const Rpcs2 extends ReadonlyArray<Rpc.Any>>(...rpcs: Rpcs2): RpcGroup<R | Rpcs2[number]>
  merge<const Groups extends ReadonlyArray<Any>>(...groups: Groups): RpcGroup<R | Rpcs<Groups[number]>>
  middleware<M extends RpcMiddleware.TagClassAny>(middleware: M): RpcGroup<Rpc.AddMiddleware<R, M>>
  prefix<const Prefix extends string>(prefix: Prefix): RpcGroup<Rpc.Prefixed<R, Prefix>>
  toHandlersContext<Handlers extends HandlersFrom<R>, EX = never, RX = never>(
    build: Handlers | Effect.Effect<Handlers, EX, RX>
  ): Effect.Effect<Context.Context<Rpc.ToHandler<R>>, EX, RX | HandlersContext<R, Handlers>>
  toLayer<Handlers extends HandlersFrom<R>, EX = never, RX = never>(
    build: Handlers | Effect.Effect<Handlers, EX, RX>
  ): Layer.Layer<Rpc.ToHandler<R>, EX, Exclude<RX, Scope> | HandlersContext<R, Handlers>>
  of<const Handlers extends HandlersFrom<R>>(handlers: Handlers): Handlers
  toLayerHandler<const Tag extends R["_tag"], Handler extends HandlerFrom<R, Tag>, EX = never, RX = never>(
    tag: Tag, build: Handler | Effect.Effect<Handler, EX, RX>
  ): Layer.Layer<Rpc.Handler<Tag>, EX, Exclude<RX, Scope> | HandlerContext<R, Tag, Handler>>
  accessHandler<const Tag extends R["_tag"]>(
    tag: Tag
  ): Effect.Effect<(payload: Rpc.Payload<Extract<R, { readonly _tag: Tag }>>, headers: Headers) => Rpc.ResultFrom<Extract<R, { readonly _tag: Tag }>, never>, never, Rpc.Handler<Tag>>
  annotate<I, S>(tag: Context.Tag<I, S>, value: S): RpcGroup<R>
  annotateRpcs<I, S>(tag: Context.Tag<I, S>, value: S): RpcGroup<R>
  annotateContext<S>(context: Context.Context<S>): RpcGroup<R>
  annotateRpcsContext<S>(context: Context.Context<S>): RpcGroup<R>
}

interface Any {
  readonly [TypeId]: TypeId
}

type HandlersFrom<Rpc extends Rpc.Any> = {
  readonly [Current in Rpc as Current["_tag"]]: Rpc.ToHandlerFn<Current>
}

type HandlerFrom<Rpc extends Rpc.Any, Tag extends Rpc["_tag"]> =
  Extract<Rpc, { readonly _tag: Tag }> extends infer Current ? Current extends Rpc.Any ? Rpc.ToHandlerFn<Current> : never : never

type HandlersContext<Rpcs extends Rpc.Any, Handlers> =
  keyof Handlers extends infer K ? K extends keyof Handlers & string ? HandlerContext<Rpcs, K, Handlers[K]> : never : never

type HandlerContext<Rpcs extends Rpc.Any, K extends Rpcs["_tag"], Handler> =
  [Rpc.IsStream<Rpcs, K>] extends [true]
    ? Handler extends (...args: any) =>
        Stream.Stream<infer _A, infer _E, infer _R>
        | Rpc.Wrapper<Stream.Stream<infer _A, infer _E, infer _R>>
        | Effect.Effect<ReadonlyMailbox<infer _A, infer _E>, infer _EX, infer _R>
        | Rpc.Wrapper<Effect.Effect<ReadonlyMailbox<infer _A, infer _E>, infer _EX, infer _R>>
      ? Exclude<Rpc.ExcludeProvides<_R, Rpcs, K>, Scope>
      : never
    : Handler extends (...args: any) => Effect.Effect<infer _A, infer _E, infer _R> | Rpc.Wrapper<Effect.Effect<infer _A, infer _E, infer _R>>
      ? Rpc.ExcludeProvides<_R, Rpcs, K>
      : never

type Rpcs<Group> = Group extends RpcGroup<infer R> ? string extends R["_tag"] ? never : R : never

const make: <const Rpcs extends ReadonlyArray<Rpc.Any>>(...rpcs: Rpcs) => RpcGroup<Rpcs[number]>
```

## [4]-[RPC_CLIENT]

`@effect/rpc/RpcClient` — the typed client. `RpcClient<Rpcs, E>` resolves each procedure to a callable
returning `Effect`/`Stream`/`Mailbox`; `make` requires a `Protocol` from context; `makeNoSerialization`
drives a raw message pair. The `Protocol` tag and its `make*`/`layer*` factories own transport selection
(http, socket, worker). `InternalRpc` projects `client: Effect<RpcClient<Procedures>, never, Protocol>`.

[PUBLIC_TYPE_SCOPE]: client type family
- rail: internal-rpc

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                             |
| :-----: | :---------------------- | :------------- | :------------------------------------------------- |
|   [1]   | `RpcClient<Rpcs, E>`    | type alias     | prefixed + non-prefixed method record              |
|   [2]   | `RpcClient.Prefixes`    | namespace type | dotted-tag prefix extraction                       |
|   [3]   | `RpcClient.NonPrefixed` | namespace type | tags without a `prefix.` segment                   |
|   [4]   | `RpcClient.Prefixed`    | namespace type | tags with a `prefix.` segment                      |
|   [5]   | `RpcClient.From`        | namespace type | per-method callable record                         |
|   [6]   | `RpcClient.Flat`        | namespace type | single `(tag, payload, options)` dispatch callable |
|   [7]   | `FromGroup<Group, E>`   | type alias     | `RpcClient<RpcGroup.Rpcs<Group>, E>`               |

```ts contract
// @effect/rpc/RpcClient
type RpcClient<Rpcs extends Rpc.Any, E = never> = Schema.Simplify<
  RpcClient.From<RpcClient.NonPrefixed<Rpcs>, E, ""> & {
    readonly [CurrentPrefix in RpcClient.Prefixes<Rpcs>]: RpcClient.From<RpcClient.Prefixed<Rpcs, CurrentPrefix>, E, CurrentPrefix>
  }
>

declare namespace RpcClient {
  type Prefixes<Rpcs extends Rpc.Any> = Rpcs["_tag"] extends infer Tag ? Tag extends `${infer Prefix}.${string}` ? Prefix : never : never
  type NonPrefixed<Rpcs extends Rpc.Any> = Exclude<Rpcs, { readonly _tag: `${string}.${string}` }>
  type Prefixed<Rpcs extends Rpc.Any, Prefix extends string> = Extract<Rpcs, { readonly _tag: `${Prefix}.${string}` }>

  type From<Rpcs extends Rpc.Any, E = never, Prefix extends string = ""> = {
    readonly [Current in Rpcs as Current["_tag"] extends `${Prefix}.${infer Method}` ? Method : Current["_tag"]]: <const AsMailbox extends boolean = false, const Discard = false>(
      input: Rpc.PayloadConstructor<Current>,
      options?: [Rpc.SuccessSchema<Current>] extends [RpcSchema.Stream<infer _A, infer _E>]
        ? { readonly asMailbox?: AsMailbox | undefined; readonly streamBufferSize?: number | undefined; readonly headers?: Headers.Input | undefined; readonly context?: Context.Context<never> | undefined }
        : { readonly headers?: Headers.Input | undefined; readonly context?: Context.Context<never> | undefined; readonly discard?: Discard | undefined }
    ) => Current extends Rpc.Rpc<infer _Tag, infer _Payload, infer _Success, infer _Error, infer _Middleware>
      ? [_Success] extends [RpcSchema.Stream<infer _A, infer _E>]
        ? AsMailbox extends true
          ? Effect.Effect<Mailbox.ReadonlyMailbox<_A["Type"], _E["Type"] | _Error["Type"] | E | _Middleware["failure"]["Type"]>, never, Scope.Scope | _Payload["Context"] | _Success["Context"] | _Error["Context"] | _Middleware["failure"]["Context"]>
          : Stream.Stream<_A["Type"], _E["Type"] | _Error["Type"] | E | _Middleware["failure"]["Type"], _Payload["Context"] | _Success["Context"] | _Error["Context"] | _Middleware["failure"]["Context"]>
        : Effect.Effect<Discard extends true ? void : _Success["Type"], Discard extends true ? E : _Error["Type"] | E | _Middleware["failure"]["Type"], _Payload["Context"] | _Success["Context"] | _Error["Context"] | _Middleware["failure"]["Context"]>
      : never
  }

  type Flat<Rpcs extends Rpc.Any, E = never> = <const Tag extends Rpcs["_tag"], const AsMailbox extends boolean = false, const Discard = false>(
    tag: Tag,
    payload: Rpc.PayloadConstructor<Rpc.ExtractTag<Rpcs, Tag>>,
    options?: Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>> extends Stream.Stream<infer _A, infer _E, infer _R>
      ? { readonly asMailbox?: AsMailbox | undefined; readonly streamBufferSize?: number | undefined; readonly headers?: Headers.Input | undefined; readonly context?: Context.Context<never> | undefined }
      : { readonly headers?: Headers.Input | undefined; readonly context?: Context.Context<never> | undefined; readonly discard?: Discard | undefined }
  ) => /* same Effect/Stream/Mailbox result shape as From */ never
}

type FromGroup<Group, E = never> = RpcClient<RpcGroup.Rpcs<Group>, E>
```

[PUBLIC_TYPE_SCOPE]: client constructors, headers, protocol
- rail: internal-rpc

```ts contract
// @effect/rpc/RpcClient — constructors
const makeNoSerialization: <Rpcs extends Rpc.Any, E, const Flatten extends boolean = false>(
  group: RpcGroup.RpcGroup<Rpcs>,
  options: {
    readonly onFromClient: (options: { readonly message: FromClient<Rpcs>; readonly context: Context.Context<never>; readonly discard: boolean }) => Effect.Effect<void, E>
    readonly supportsAck?: boolean | undefined
    readonly spanPrefix?: string | undefined
    readonly spanAttributes?: Record<string, unknown> | undefined
    readonly generateRequestId?: (() => RequestId) | undefined
    readonly disableTracing?: boolean | undefined
    readonly flatten?: Flatten | undefined
  }
) => Effect.Effect<{
  readonly client: Flatten extends true ? RpcClient.Flat<Rpcs, E> : RpcClient<Rpcs, E>
  readonly write: (message: FromServer<Rpcs>) => Effect.Effect<void>
}, never, Scope.Scope | Rpc.MiddlewareClient<Rpcs>>

const make: <Rpcs extends Rpc.Any, const Flatten extends boolean = false>(
  group: RpcGroup.RpcGroup<Rpcs>,
  options?: {
    readonly spanPrefix?: string | undefined
    readonly spanAttributes?: Record<string, unknown> | undefined
    readonly generateRequestId?: (() => RequestId) | undefined
    readonly disableTracing?: boolean | undefined
    readonly flatten?: Flatten | undefined
  } | undefined
) => Effect.Effect<Flatten extends true ? RpcClient.Flat<Rpcs, RpcClientError> : RpcClient<Rpcs, RpcClientError>, never, Protocol | Rpc.MiddlewareClient<Rpcs> | Scope.Scope>

// headers
const currentHeaders: FiberRef.FiberRef<Headers.Headers>
const withHeaders: {
  (headers: Headers.Input): <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(effect: Effect.Effect<A, E, R>, headers: Headers.Input): Effect.Effect<A, E, R>
}
const withHeadersEffect: {
  <E2, R2>(headers: Effect.Effect<Headers.Input, E2, R2>): <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E | E2, R | R2>
  <A, E, R, E2, R2>(effect: Effect.Effect<A, E, R>, headers: Effect.Effect<Headers.Input, E2, R2>): Effect.Effect<A, E | E2, R | R2>
}

// protocol service tag — @effect/rpc/RpcClient/Protocol
class Protocol extends Context.TagClass<Protocol, "@effect/rpc/RpcClient/Protocol", {
  readonly run: (f: (data: FromServerEncoded) => Effect.Effect<void>) => Effect.Effect<never>
  readonly send: (request: FromClientEncoded, transferables?: ReadonlyArray<globalThis.Transferable>) => Effect.Effect<void, RpcClientError>
  readonly supportsAck: boolean
  readonly supportsTransferables: boolean
}> {
  static make: <EX, RX>(f: (write: (data: FromServerEncoded) => Effect.Effect<void>) => Effect.Effect<Omit<Protocol["Type"], "run">, EX, RX>) => Effect.Effect<Protocol["Type"], EX, RX>
}

// protocol factories
const makeProtocolHttp: (client: HttpClient.HttpClient) => Effect.Effect<Protocol["Type"], never, RpcSerialization.RpcSerialization>
const layerProtocolHttp: (options: {
  readonly url: string
  readonly transformClient?: <E, R>(client: HttpClient.HttpClient.With<E, R>) => HttpClient.HttpClient.With<E, R>
}) => Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization | HttpClient.HttpClient>

const makeProtocolSocket: (options?: {
  readonly retryTransientErrors?: boolean | undefined
  readonly retrySchedule?: Schedule.Schedule<any, Socket.SocketError> | undefined
}) => Effect.Effect<Protocol["Type"], never, Scope.Scope | RpcSerialization.RpcSerialization | Socket.Socket>
const layerProtocolSocket: (options?: {
  readonly retryTransientErrors?: boolean | undefined
  readonly retrySchedule?: Schedule.Schedule<any, Socket.SocketError> | undefined
}) => Layer.Layer<Protocol, never, Socket.Socket | RpcSerialization.RpcSerialization>

const makeProtocolWorker: (options:
  | { readonly size: number; readonly concurrency?: number | undefined; readonly targetUtilization?: number | undefined }
  | { readonly minSize: number; readonly maxSize: number; readonly concurrency?: number | undefined; readonly targetUtilization?: number | undefined; readonly timeToLive: Duration.DurationInput }
) => Effect.Effect<Protocol["Type"], WorkerError, Scope.Scope | Worker.PlatformWorker | Worker.Spawner>
const layerProtocolWorker: (options:
  | { readonly size: number; readonly concurrency?: number | undefined; readonly targetUtilization?: number | undefined }
  | { readonly minSize: number; readonly maxSize: number; readonly concurrency?: number | undefined; readonly targetUtilization?: number | undefined; readonly timeToLive: Duration.DurationInput }
) => Layer.Layer<Protocol, WorkerError, Worker.PlatformWorker | Worker.Spawner>
```

## [5]-[RPC_SERVER]

`@effect/rpc/RpcServer` — the server runner. `make`/`layer` run a group against the server `Protocol`;
`toHttpApp`/`toHttpAppWebsocket`/`toWebHandler` produce `@effect/platform` HTTP entrypoints;
`layerHttpRouter` registers a route on an `HttpLayerRouter`. The server `Protocol` tag is distinct from
the client `Protocol` (different `TagClass` id) and carries multi-client dispatch (`clientId`, `disconnects`,
`clientIds`). `InternalRpc` projects `server: Layer<never, never, RpcServer.Protocol>`.

[PUBLIC_TYPE_SCOPE]: server model and runners
- rail: internal-rpc

```ts contract
// @effect/rpc/RpcServer
interface RpcServer<A extends Rpc.Any> {
  readonly write: (clientId: number, message: FromClient<A>) => Effect.Effect<void>
  readonly disconnect: (clientId: number) => Effect.Effect<void>
}

const makeNoSerialization: <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, options: {
  readonly onFromServer: (response: FromServer<Rpcs>) => Effect.Effect<void>
  readonly disableTracing?: boolean | undefined
  readonly disableSpanPropagation?: boolean | undefined
  readonly spanPrefix?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly disableClientAcks?: boolean | undefined
  readonly concurrency?: number | "unbounded" | undefined
  readonly disableFatalDefects?: boolean | undefined
}) => Effect.Effect<RpcServer<Rpcs>, never, Rpc.ToHandler<Rpcs> | Rpc.Middleware<Rpcs> | Scope.Scope>

const make: <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, options?: {
  readonly disableTracing?: boolean | undefined
  readonly spanPrefix?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly concurrency?: number | "unbounded" | undefined
  readonly disableFatalDefects?: boolean | undefined
} | undefined) => Effect.Effect<never, never, Protocol | Rpc.ToHandler<Rpcs> | Rpc.Middleware<Rpcs> | Rpc.Context<Rpcs>>

const layer: <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, options?: {
  readonly disableTracing?: boolean | undefined
  readonly spanPrefix?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly concurrency?: number | "unbounded" | undefined
  readonly disableFatalDefects?: boolean | undefined
}) => Layer.Layer<never, never, Protocol | Rpc.ToHandler<Rpcs> | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs>>

const layerHttpRouter: <Rpcs extends Rpc.Any>(options: {
  readonly group: RpcGroup.RpcGroup<Rpcs>
  readonly path: HttpRouter.PathInput
  readonly protocol?: "http" | "websocket" | undefined
  readonly disableTracing?: boolean | undefined
  readonly spanPrefix?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly concurrency?: number | "unbounded" | undefined
  readonly disableFatalDefects?: boolean | undefined
}) => Layer.Layer<never, never, RpcSerialization.RpcSerialization | HttpLayerRouter.HttpRouter | Rpc.ToHandler<Rpcs> | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs>>
```

[PUBLIC_TYPE_SCOPE]: server protocol tag and transport factories
- rail: internal-rpc

```ts contract
// @effect/rpc/RpcServer — Protocol service (@effect/rpc/RpcServer/Protocol)
class Protocol extends Context.TagClass<Protocol, "@effect/rpc/RpcServer/Protocol", {
  readonly run: (f: (clientId: number, data: FromClientEncoded) => Effect.Effect<void>) => Effect.Effect<never>
  readonly disconnects: Mailbox.ReadonlyMailbox<number>
  readonly send: (clientId: number, response: FromServerEncoded, transferables?: ReadonlyArray<globalThis.Transferable>) => Effect.Effect<void>
  readonly end: (clientId: number) => Effect.Effect<void>
  readonly clientIds: Effect.Effect<ReadonlySet<number>>
  readonly initialMessage: Effect.Effect<Option.Option<unknown>>
  readonly supportsAck: boolean
  readonly supportsTransferables: boolean
  readonly supportsSpanPropagation: boolean
}> {
  static make: <EX, RX>(f: (write: (clientId: number, data: FromClientEncoded) => Effect.Effect<void>) => Effect.Effect<Omit<Protocol["Type"], "run">, EX, RX>) => Effect.Effect<Protocol["Type"], EX, RX>
}

// transport factories — every make* returns the Protocol service shape; layer* returns Layer<Protocol, …>
const makeProtocolSocketServer: Effect.Effect<Protocol["Type"], never, Scope.Scope | RpcSerialization.RpcSerialization | SocketServer.SocketServer>
const layerProtocolSocketServer: Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization | SocketServer.SocketServer>

const makeProtocolWithHttpAppWebsocket: Effect.Effect<{ readonly protocol: Protocol["Type"]; readonly httpApp: HttpApp.Default<never, Scope.Scope> }, never, RpcSerialization.RpcSerialization>
const makeProtocolWithHttpApp: Effect.Effect<{ readonly protocol: Protocol["Type"]; readonly httpApp: HttpApp.Default<never, Scope.Scope> }, never, RpcSerialization.RpcSerialization>

const makeProtocolWebsocket: <I = HttpRouter.Default>(options: { readonly path: HttpRouter.PathInput; readonly routerTag?: Context.Tag<I, HttpRouter.HttpRouter.Service<any, any>> }) => Effect.Effect<Protocol["Type"], never, RpcSerialization.RpcSerialization | I>
const makeProtocolWebsocketRouter: (options: { readonly path: HttpRouter.PathInput }) => Effect.Effect<Protocol["Type"], never, RpcSerialization.RpcSerialization | HttpLayerRouter.HttpRouter>
const layerProtocolWebsocket: <I = HttpRouter.Default>(options: { readonly path: HttpRouter.PathInput; readonly routerTag?: HttpRouter.HttpRouter.TagClass<I, string, any, any> }) => Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization>
const layerProtocolWebsocketRouter: (options: { readonly path: HttpLayerRouter.PathInput }) => Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization | HttpLayerRouter.HttpRouter>

const makeProtocolHttp: <I = HttpRouter.Default>(options: { readonly path: HttpRouter.PathInput; readonly routerTag?: HttpRouter.HttpRouter.TagClass<I, string, any, any> }) => Effect.Effect<Protocol["Type"], never, RpcSerialization.RpcSerialization | I>
const makeProtocolHttpRouter: (options: { readonly path: HttpRouter.PathInput }) => Effect.Effect<Protocol["Type"], never, RpcSerialization.RpcSerialization | HttpLayerRouter.HttpRouter>
const layerProtocolHttp: <I = HttpRouter.Default>(options: { readonly path: HttpRouter.PathInput; readonly routerTag?: HttpRouter.HttpRouter.TagClass<I, string, any, any> }) => Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization>
const layerProtocolHttpRouter: (options: { readonly path: HttpRouter.PathInput }) => Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization | HttpLayerRouter.HttpRouter>

const makeProtocolWorkerRunner: Effect.Effect<Protocol["Type"], WorkerError, WorkerRunner.PlatformRunner | Scope.Scope>
const layerProtocolWorkerRunner: Layer.Layer<Protocol, WorkerError, WorkerRunner.PlatformRunner>

const makeProtocolStdio: <EIn, EOut, RIn, ROut>(options: {
  readonly stdin: Stream.Stream<Uint8Array, EIn, RIn>
  readonly stdout: Sink.Sink<void, Uint8Array | string, unknown, EOut, ROut>
}) => Effect.Effect<Protocol["Type"], never, Scope.Scope | RpcSerialization.RpcSerialization | RIn | Exclude<ROut, Scope.Scope>>
const layerProtocolStdio: <EIn, EOut, RIn, ROut>(options: {
  readonly stdin: Stream.Stream<Uint8Array, EIn, RIn>
  readonly stdout: Sink.Sink<void, Uint8Array | string, unknown, EOut, ROut>
}) => Layer.Layer<Protocol, never, RpcSerialization.RpcSerialization | RIn | ROut>

// HTTP entrypoints
const toHttpApp: <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, options?: {
  readonly disableTracing?: boolean | undefined; readonly spanPrefix?: string | undefined; readonly spanAttributes?: Record<string, unknown> | undefined; readonly disableFatalDefects?: boolean | undefined
} | undefined) => Effect.Effect<HttpApp.Default<never, Scope.Scope>, never, Scope.Scope | RpcSerialization.RpcSerialization | Rpc.ToHandler<Rpcs> | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs>>
const toHttpAppWebsocket: <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, options?: {
  readonly disableTracing?: boolean | undefined; readonly spanPrefix?: string | undefined; readonly spanAttributes?: Record<string, unknown> | undefined; readonly disableFatalDefects?: boolean | undefined
} | undefined) => Effect.Effect<HttpApp.Default<never, Scope.Scope>, never, Scope.Scope | RpcSerialization.RpcSerialization | Rpc.ToHandler<Rpcs> | Rpc.Context<Rpcs> | Rpc.Middleware<Rpcs>>
const toWebHandler: <Rpcs extends Rpc.Any, LE>(group: RpcGroup.RpcGroup<Rpcs>, options: {
  readonly layer: Layer.Layer<Rpc.ToHandler<Rpcs> | Rpc.Middleware<Rpcs> | RpcSerialization.RpcSerialization | HttpRouter.HttpRouter.DefaultServices, LE>
  readonly disableTracing?: boolean | undefined
  readonly spanPrefix?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly disableFatalDefects?: boolean | undefined
  readonly middleware?: (httpApp: HttpApp.Default) => HttpApp.Default<never, HttpRouter.HttpRouter.DefaultServices>
  readonly memoMap?: Layer.MemoMap
}) => {
  readonly handler: (request: globalThis.Request, context?: Context.Context<never> | undefined) => Promise<Response>
  readonly dispose: () => Promise<void>
}

// interruption fiber ids
const fiberIdClientInterrupt: FiberId.Runtime
const fiberIdTransientInterrupt: FiberId.Runtime
```

## [6]-[RPC_MESSAGE]

`@effect/rpc/RpcMessage` — the wire-message algebra. Two discriminated unions per direction: a decoded
form (`FromClient`/`FromServer`, generic over `Rpc.Any`) and an encoded wire form
(`FromClientEncoded`/`FromServerEncoded`). `RequestId` is a branded `bigint`; `ResponseId` a branded
`number`. These are the message shapes the serialization `Parser` encodes and decodes.

[PUBLIC_TYPE_SCOPE]: message unions and branded ids
- rail: internal-rpc

| [INDEX] | [SYMBOL]            | [DISCRIMINANTS / SHAPE]                                                                               |
| :-----: | :------------------ | :---------------------------------------------------------------------------------------------------- |
|   [1]   | `FromClient<A>`     | `Request<A> \| Ack \| Interrupt \| Eof`                                                               |
|   [2]   | `FromClientEncoded` | `RequestEncoded \| AckEncoded \| InterruptEncoded \| Ping \| Eof`                                     |
|   [3]   | `FromServer<A>`     | `ResponseChunk<A> \| ResponseExit<A> \| ResponseDefect \| ClientEnd`                                  |
|   [4]   | `FromServerEncoded` | `ResponseChunkEncoded \| ResponseExitEncoded \| ResponseDefectEncoded \| Pong \| ClientProtocolError` |
|   [5]   | `RequestId`         | `Branded<bigint, RequestIdTypeId>` + `(id: bigint \| string) => RequestId`                            |
|   [6]   | `ResponseId`        | `Branded<number, ResponseIdTypeId>`                                                                   |

```ts contract
// @effect/rpc/RpcMessage — request side
type FromClient<A extends Rpc.Any> = Request<A> | Ack | Interrupt | Eof
type FromClientEncoded = RequestEncoded | AckEncoded | InterruptEncoded | Ping | Eof

const RequestIdTypeId: unique symbol
type RequestIdTypeId = typeof RequestIdTypeId
type RequestId = Branded<bigint, RequestIdTypeId>
const RequestId: (id: bigint | string) => RequestId

interface RequestEncoded {
  readonly _tag: "Request"
  readonly id: string
  readonly tag: string
  readonly payload: unknown
  readonly headers: ReadonlyArray<[string, string]>
  readonly traceId?: string | undefined
  readonly spanId?: string | undefined
  readonly sampled?: boolean | undefined
}
interface Request<A extends Rpc.Any> {
  readonly _tag: "Request"
  readonly id: RequestId
  readonly tag: Rpc.Tag<A>
  readonly payload: Rpc.Payload<A>
  readonly headers: Headers
  readonly traceId?: string | undefined
  readonly spanId?: string | undefined
  readonly sampled?: boolean | undefined
}
interface Ack { readonly _tag: "Ack"; readonly requestId: RequestId }
interface Interrupt { readonly _tag: "Interrupt"; readonly requestId: RequestId; readonly interruptors: ReadonlyArray<FiberId.FiberId> }
interface AckEncoded { readonly _tag: "Ack"; readonly requestId: string }
interface InterruptEncoded { readonly _tag: "Interrupt"; readonly requestId: string }
interface Eof { readonly _tag: "Eof" }
interface Ping { readonly _tag: "Ping" }
const constEof: Eof
const constPing: Ping

// response side
type FromServer<A extends Rpc.Any> = ResponseChunk<A> | ResponseExit<A> | ResponseDefect | ClientEnd
type FromServerEncoded = ResponseChunkEncoded | ResponseExitEncoded | ResponseDefectEncoded | Pong | ClientProtocolError

const ResponseIdTypeId: unique symbol
type ResponseIdTypeId = typeof ResponseIdTypeId
type ResponseId = Branded<number, ResponseIdTypeId>

interface ResponseChunkEncoded { readonly _tag: "Chunk"; readonly requestId: string; readonly values: NonEmptyReadonlyArray<unknown> }
interface ResponseChunk<A extends Rpc.Any> { readonly _tag: "Chunk"; readonly clientId: number; readonly requestId: RequestId; readonly values: NonEmptyReadonlyArray<Rpc.SuccessChunk<A>> }
interface ResponseExitEncoded { readonly _tag: "Exit"; readonly requestId: string; readonly exit: Schema.ExitEncoded<unknown, unknown, unknown> }
interface ResponseExit<A extends Rpc.Any> { readonly _tag: "Exit"; readonly clientId: number; readonly requestId: RequestId; readonly exit: Rpc.Exit<A> }
interface ResponseDefectEncoded { readonly _tag: "Defect"; readonly defect: unknown }
const ResponseDefectEncoded: (input: unknown) => ResponseDefectEncoded
interface ResponseDefect { readonly _tag: "Defect"; readonly clientId: number; readonly defect: unknown }
interface ClientEnd { readonly _tag: "ClientEnd"; readonly clientId: number }
interface ClientProtocolError { readonly _tag: "ClientProtocolError"; readonly error: RpcClientError }
interface Pong { readonly _tag: "Pong" }
const constPong: Pong
```

## [7]-[RPC_MIDDLEWARE]

`@effect/rpc/RpcMiddleware` — the middleware tag family. `Tag<Self>()` builds a `TagClass` whose
behavior shape depends on the option flags (`wrap`, `provides`, `failure`, `optional`, `requiredForClient`).
`RpcGroup.middleware`/`Rpc.middleware` accept a `TagClassAny`; `layerClient` installs a client-side
middleware. Server-side middleware is `RpcMiddleware`, wrap-style is `RpcMiddlewareWrap`.

```ts contract
// @effect/rpc/RpcMiddleware
const TypeId: unique symbol
type TypeId = typeof TypeId

interface RpcMiddleware<Provides, E> {
  (options: { readonly clientId: number; readonly rpc: Rpc.AnyWithProps; readonly payload: unknown; readonly headers: Headers }): Effect.Effect<Provides, E>
}
interface RpcMiddlewareWrap<Provides, E> {
  (options: { readonly clientId: number; readonly rpc: Rpc.AnyWithProps; readonly payload: unknown; readonly headers: Headers; readonly next: Effect.Effect<SuccessValue, E, Provides> }): Effect.Effect<SuccessValue, E>
}
interface SuccessValue { readonly _: unique symbol }
interface RpcMiddlewareClient<R = never> {
  (options: { readonly rpc: Rpc.AnyWithProps; readonly request: Request<Rpc.Any> }): Effect.Effect<Request<Rpc.Any>, never, R>
}
interface ForClient<Id> { readonly _: unique symbol; readonly id: Id }
interface Any {
  (options: { readonly rpc: Rpc.AnyWithProps; readonly payload: unknown; readonly headers: Headers; readonly next?: Effect.Effect<any, any, any> }): Effect.Effect<any, any>
}

interface TagClass<Self, Name extends string, Options> extends TagClass.Base<
  Self, Name, Options,
  TagClass.Wrap<Options> extends true ? RpcMiddlewareWrap<TagClass.Provides<Options>, TagClass.Failure<Options>> : RpcMiddleware<TagClass.Service<Options>, TagClass.FailureService<Options>>
> {}

declare namespace TagClass {
  type Provides<Options> = Options extends { readonly provides: Context.Tag<any, any>; readonly optional?: false } ? Context.Tag.Identifier<Options["provides"]> : never
  type Service<Options> = Options extends { readonly provides: Context.Tag<any, any> } ? Context.Tag.Service<Options["provides"]> : void
  type FailureSchema<Options> = Options extends { readonly failure: Schema.Schema.All; readonly optional?: false } ? Options["failure"] : typeof Schema.Never
  type Failure<Options> = Options extends { readonly failure: Schema.Schema<infer _A, infer _I, infer _R>; readonly optional?: false } ? _A : never
  type FailureContext<Options> = Schema.Schema.Context<FailureSchema<Options>>
  type FailureService<Options> = Optional<Options> extends true ? unknown : Failure<Options>
  type Optional<Options> = Options extends { readonly optional: true } ? true : false
  type RequiredForClient<Options> = Options extends { readonly requiredForClient: true } ? true : false
  type Wrap<Options> = Options extends { readonly wrap: true } ? true : false
  interface Base<Self, Name extends string, Options, Service> extends Context.Tag<Self, Service> {
    new (_: never): Context.TagClassShape<Name, Service>
    readonly [TypeId]: TypeId
    readonly optional: Optional<Options>
    readonly failure: FailureSchema<Options>
    readonly provides: Options extends { readonly provides: Context.Tag<any, any> } ? Options["provides"] : undefined
    readonly requiredForClient: RequiredForClient<Options>
    readonly wrap: Wrap<Options>
  }
}

interface TagClassAny extends Context.Tag<any, any> {
  readonly [TypeId]: TypeId
  readonly optional: boolean
  readonly provides?: Context.Tag<any, any> | undefined
  readonly failure: Schema.Schema.All
  readonly requiredForClient: boolean
  readonly wrap: boolean
}
interface TagClassAnyWithProps extends Context.Tag<any, RpcMiddleware<any, any> | RpcMiddlewareWrap<any, any>> {
  readonly [TypeId]: TypeId
  readonly optional: boolean
  readonly provides?: Context.Tag<any, any>
  readonly failure: Schema.Schema.All
  readonly requiredForClient: boolean
  readonly wrap: boolean
}

const Tag: <Self>() => <const Name extends string, const Options extends {
  readonly wrap?: boolean
  readonly optional?: boolean
  readonly failure?: Schema.Schema.All
  readonly provides?: Context.Tag<any, any>
  readonly requiredForClient?: boolean
}>(id: Name, options?: Options | undefined) => TagClass<Self, Name, Options>

const layerClient: <Id, S, R, EX = never, RX = never>(
  tag: Context.Tag<Id, S>,
  service: RpcMiddlewareClient<R> | Effect.Effect<RpcMiddlewareClient<R>, EX, RX>
) => Layer.Layer<ForClient<Id>, EX, R | Exclude<RX, Scope>>
```

## [8]-[RPC_SCHEMA]

`@effect/rpc/RpcSchema` — the stream-success schema. `Stream<A, E>` is a `Schema` whose decoded type is
`Stream_.Stream<A["Type"], E["Type"]>`; `Rpc.make({ stream: true })` wraps the success schema as one.
The guards/extractors classify a schema as stream-bearing.

```ts contract
// @effect/rpc/RpcSchema
const StreamSchemaId: unique symbol
const isStreamSchema: (schema: Schema.Schema.All) => schema is Stream<any, any>
const isStreamSerializable: (schema: Schema.WithResult.Any) => boolean
const getStreamSchemas: (ast: AST.AST) => Option.Option<{ readonly success: Schema.Schema.Any; readonly failure: Schema.Schema.All }>

interface Stream<A extends Schema.Schema.Any, E extends Schema.Schema.All>
  extends Schema.Schema<Stream_.Stream<A["Type"], E["Type"]>, Stream_.Stream<A["Encoded"], E["Encoded"]>, A["Context"] | E["Context"]> {
  readonly success: A
  readonly failure: E
}
const Stream: <A extends Schema.Schema.Any, E extends Schema.Schema.All>(
  options: { readonly failure: E; readonly success: A }
) => Stream<A, E>
```

## [9]-[RPC_SERIALIZATION]

`@effect/rpc/RpcSerialization` — the serialization service. `RpcSerialization` is a `Context.TagClass`
producing a `Parser` (`decode`/`encode`); the module ships JSON, NDJSON, JSON-RPC, NDJSON-RPC, and
MessagePack codecs as both bare service values and `Layer`s. `makeMsgPack` accepts `msgpackr` `Options`.
`InternalRpc` selects one serialization layer for its protocol.

| [INDEX] | [SYMBOL]                       | [KIND]          | [CONTENT_TYPE / FRAMING]                   |
| :-----: | :----------------------------- | :-------------- | :----------------------------------------- |
|   [1]   | `json` / `layerJson`           | service / layer | JSON; supports framing                     |
|   [2]   | `ndjson` / `layerNdjson`       | service / layer | NDJSON; no framing (newline-delimited)     |
|   [3]   | `jsonRpc` / `layerJsonRpc`     | factory / layer | JSON-RPC; optional `contentType`           |
|   [4]   | `ndJsonRpc` / `layerNdJsonRpc` | factory / layer | NDJSON JSON-RPC; optional `contentType`    |
|   [5]   | `msgPack` / `layerMsgPack`     | service / layer | MessagePack binary                         |
|   [6]   | `makeMsgPack`                  | factory         | MessagePack with custom `msgpackr.Options` |

```ts contract
// @effect/rpc/RpcSerialization
class RpcSerialization extends Context.TagClass<RpcSerialization, "@effect/rpc/RpcSerialization", {
  unsafeMake(): Parser
  readonly contentType: string
  readonly includesFraming: boolean
}> {}

interface Parser {
  readonly decode: (data: Uint8Array | string) => ReadonlyArray<unknown>
  readonly encode: (response: unknown) => Uint8Array | string | undefined
}

const json: RpcSerialization["Type"]
const ndjson: RpcSerialization["Type"]
const jsonRpc: (options?: { readonly contentType?: string | undefined }) => RpcSerialization["Type"]
const ndJsonRpc: (options?: { readonly contentType?: string | undefined }) => RpcSerialization["Type"]
const makeMsgPack: (options?: Msgpackr.Options | undefined) => RpcSerialization["Type"]
const msgPack: RpcSerialization["Type"]

const layerJson: Layer.Layer<RpcSerialization>
const layerNdjson: Layer.Layer<RpcSerialization>
const layerJsonRpc: (options?: { readonly contentType?: string | undefined }) => Layer.Layer<RpcSerialization>
const layerNdJsonRpc: (options?: { readonly contentType?: string | undefined }) => Layer.Layer<RpcSerialization>
const layerMsgPack: Layer.Layer<RpcSerialization>
```

## [10]-[RPC_CLIENT_ERROR]

`@effect/rpc/RpcClientError` — the client-side error. A `Schema.TaggedError` named `"RpcClientError"`
with a `reason` literal (`"Protocol" | "Unknown"`), a `message` string, and an optional `cause` defect.
`RpcClient.make` produces a client failing in `RpcClientError`; the wire form rides `ClientProtocolError`.

```ts contract
// @effect/rpc/RpcClientError
const TypeId: unique symbol
type TypeId = typeof TypeId

class RpcClientError extends Schema.TaggedError<RpcClientError>()("RpcClientError", {
  reason: Schema.Literal("Protocol", "Unknown"),
  message: Schema.String,
  cause: Schema.optional(Schema.Defect)
}) {
  readonly [TypeId]: TypeId
}
```

## [11]-[RPC_TEST]

`@effect/rpc/RpcTest` — the in-memory test client. `makeClient` constructs a client that dispatches
directly through the handler layer (no protocol/transport), failing in nothing, requiring
`Rpc.ToHandler`/`Rpc.Middleware`/`Rpc.MiddlewareClient` from context.

```ts contract
// @effect/rpc/RpcTest
const makeClient: <Rpcs extends Rpc.Any, const Flatten extends boolean = false>(
  group: RpcGroup.RpcGroup<Rpcs>,
  options?: { flatten?: Flatten | undefined }
) => Effect.Effect<
  Flatten extends true ? RpcClient.RpcClient.Flat<Rpcs> : RpcClient.RpcClient<Rpcs>,
  never,
  Scope.Scope | Rpc.ToHandler<Rpcs> | Rpc.Middleware<Rpcs> | Rpc.MiddlewareClient<Rpcs>
>
```

## [12]-[RPC_WORKER]

`@effect/rpc/RpcWorker` — the worker-transport bootstrap. `InitialMessage` is a `Context.TagClass`
carrying the worker handshake payload; `makeInitialMessage`/`layerInitialMessage` build it from a schema
and effect, and `initialMessage` reads/decodes it against the server `Protocol`.

```ts contract
// @effect/rpc/RpcWorker
class InitialMessage extends Context.TagClass<InitialMessage, "@effect/rpc/RpcWorker/InitialMessage",
  Effect.Effect<readonly [data: unknown, transfers: readonly Transferable[]], never, never>> {}

declare namespace InitialMessage {
  interface Encoded { readonly _tag: "InitialMessage"; readonly value: unknown }
}

const makeInitialMessage: <A, I, R, E, R2>(
  schema: Schema.Schema<A, I, R>, effect: Effect.Effect<A, E, R2>
) => Effect.Effect<readonly [data: unknown, transferables: ReadonlyArray<globalThis.Transferable>], E | ParseError, R | R2>
const layerInitialMessage: <A, I, R, R2>(
  schema: Schema.Schema<A, I, R>, build: Effect.Effect<A, never, R2>
) => Layer.Layer<InitialMessage, never, R | R2>
const initialMessage: <A, I, R>(
  schema: Schema.Schema<A, I, R>
) => Effect.Effect<A, NoSuchElementException | ParseError, Protocol | R>
```
