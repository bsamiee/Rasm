# [API_CATALOGUE] @effect/platform

Grounded from installed `node_modules` type declarations (`@effect/platform` 0.96.1, reflected from `dist/dts/*.d.ts`). Covers the platform surfaces the six-package TS planning corpus consumes — the browser `HttpClient` binding, the `KeyValueStore` persistence face, the `Worker`/`WorkerRunner` pool backing, the schema-channel encoders, the `Socket` transport, the platform error rail, the `Runtime` boundary entry, and the `PlatformConfigProvider`. It is not an exhaustive module dump: the `HttpApi*`/`HttpServer*`/`HttpRouter`/`HttpApp`/`FileSystem`/`Path`/`Command`/`CommandExecutor`/`Terminal`/`Multipart`/`Etag`/`Template`/`Url`/`OpenApi*`/`SocketServer`/`Effectify`/`HttpMiddleware` modules ship in the package but no planning owner stands up an HTTP server, a CLI, or a filesystem-rooted config tree from this package, so they are out of scope. Every spelling below is verbatim from the declaration files.

The package re-exports each module as a namespace from the root (`export * as HttpClient from "./HttpClient.js"`, etc.); the namespace name equals the module name.

---

## [1]-[FETCH_HTTP_CLIENT]

The browser `HttpClient` binding. `layer` requires nothing and produces `HttpClient`.

```ts
// @effect/platform/FetchHttpClient
declare const Fetch_base: Context.TagClass<Fetch, "@effect/platform/FetchHttpClient/Fetch", typeof fetch>
export declare class Fetch extends Fetch_base {}

declare const RequestInit_base: Context.TagClass<RequestInit, "@effect/platform/FetchHttpClient/FetchOptions", globalThis.RequestInit>
export declare class RequestInit extends RequestInit_base {}

export declare const layer: Layer.Layer<HttpClient>
```

`Fetch` and `RequestInit` are optional `Context` overrides (a custom `fetch` impl, or default `globalThis.RequestInit` fields like `credentials`/`mode`); neither is supplied day-one.

## [2]-[HTTP_CLIENT]

The polymorphic HTTP client surface. `HttpClient` is `HttpClient.With<HttpClientError>` — the error-typed face.

```ts
// @effect/platform/HttpClient
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface HttpClient extends HttpClient.With<Error.HttpClientError> {}

export declare namespace HttpClient {
  interface With<E, R = never> extends Pipeable, Inspectable {
    readonly [TypeId]: TypeId
    readonly execute: (request: ClientRequest.HttpClientRequest) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly get: (url: string | URL, options?: ClientRequest.Options.NoBody) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly head: (url: string | URL, options?: ClientRequest.Options.NoBody) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly post: (url: string | URL, options?: ClientRequest.Options.NoUrl) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly patch: (url: string | URL, options?: ClientRequest.Options.NoUrl) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly put: (url: string | URL, options?: ClientRequest.Options.NoUrl) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly del: (url: string | URL, options?: ClientRequest.Options.NoUrl) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
    readonly options: (url: string | URL, options?: ClientRequest.Options.NoUrl) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
  }
  type Preprocess<E, R> = (request: ClientRequest.HttpClientRequest) => Effect.Effect<ClientRequest.HttpClientRequest, E, R>
  type Postprocess<E = never, R = never> = (request: Effect.Effect<ClientRequest.HttpClientRequest, E, R>) => Effect.Effect<ClientResponse.HttpClientResponse, E, R>
}

export declare const HttpClient: Context.Tag<HttpClient, HttpClient>

export declare const execute: (request: ClientRequest.HttpClientRequest) => Effect.Effect<ClientResponse.HttpClientResponse, Error.HttpClientError, HttpClient>
export declare const get: (url: string | URL, options?: ClientRequest.Options.NoBody | undefined) => Effect.Effect<ClientResponse.HttpClientResponse, Error.HttpClientError, HttpClient>
export declare const post: (url: string | URL, options?: ClientRequest.Options.NoUrl | undefined) => Effect.Effect<ClientResponse.HttpClientResponse, Error.HttpClientError, HttpClient>
// head / patch / put / del / options follow the same shape
```

Transform combinators (dual `(self, …)` / `(…) => self` forms):

```ts
export declare const filterStatusOk: <E, R>(self: HttpClient.With<E, R>) => HttpClient.With<E | Error.ResponseError, R>
export declare const filterStatus: { <E, R>(self: HttpClient.With<E, R>, f: (status: number) => boolean): HttpClient.With<E | Error.ResponseError, R> /* + curried */ }

export declare const retry: {
  <E, O extends NoExcessProperties<Effect.Retry.Options<E>, O>>(options: O): <R>(self: HttpClient.With<E, R>) => Retry.Return<R, E, O>
  <B, E, R1>(policy: Schedule.Schedule<B, NoInfer<E>, R1>): <R>(self: HttpClient.With<E, R>) => HttpClient.With<E, R1 | R>
}
export declare const retryTransient: {
  <B, E, R1 = never, const Mode extends "errors-only" | "response-only" | "both" = never, Input = …>(options: {
    readonly mode?: Mode | undefined
    readonly while?: Predicate.Predicate<NoInfer<E>>
    readonly schedule?: Schedule.Schedule<B, NoInfer<Input>, R1>
    readonly times?: number
  } | Schedule.Schedule<B, NoInfer<Input>, R1>): <R>(self: HttpClient.With<E, R>) => HttpClient.With<E, R1 | R>
}

export declare const mapRequest: { <E, R>(self: HttpClient.With<E, R>, f: (a: ClientRequest.HttpClientRequest) => ClientRequest.HttpClientRequest): HttpClient.With<E, R> /* + curried */ }
export declare const mapRequestEffect: { <E, R, E2, R2>(self: HttpClient.With<E, R>, f: (a: ClientRequest.HttpClientRequest) => Effect.Effect<ClientRequest.HttpClientRequest, E2, R2>): HttpClient.With<E | E2, R | R2> /* + curried */ }
export declare const tap: { <E, R, _, E2, R2>(self: HttpClient.With<E, R>, f: (response: ClientResponse.HttpClientResponse) => Effect.Effect<_, E2, R2>): HttpClient.With<E | E2, R | R2> /* + curried */ }
export declare const followRedirects: { <E, R>(self: HttpClient.With<E, R>, maxRedirects?: number | undefined): HttpClient.With<E, R> /* + curried */ }
export declare const withCookiesRef: { <E, R>(self: HttpClient.With<E, R>, ref: Ref<Cookies>): HttpClient.With<E, R> /* + curried */ }
export declare const withScope: <E, R>(self: HttpClient.With<E, R>) => HttpClient.With<E, R | Scope>

export declare const currentTracerPropagation: FiberRef.FiberRef<boolean>
export declare const withTracerPropagation: { <E, R>(self: HttpClient.With<E, R>, enabled: boolean): HttpClient.With<E, R> /* + curried */ }
export declare const currentTracerDisabledWhen: FiberRef.FiberRef<Predicate.Predicate<ClientRequest.HttpClientRequest>>
export declare const SpanNameGenerator: Context.Reference<SpanNameGenerator, (request: ClientRequest.HttpClientRequest) => string>
export declare const withSpanNameGenerator: { <E, R>(self: HttpClient.With<E, R>, f: (request: ClientRequest.HttpClientRequest) => string): HttpClient.With<E, R> /* + curried */ }

export declare const make: (f: (request: ClientRequest.HttpClientRequest, url: URL, signal: AbortSignal, fiber: RuntimeFiber<ClientResponse.HttpClientResponse, Error.HttpClientError>) => Effect.Effect<ClientResponse.HttpClientResponse, Error.HttpClientError>) => HttpClient
export declare const makeWith: <E2, R2, E, R>(postprocess: HttpClient.Postprocess<E2, R2>, preprocess: HttpClient.Preprocess<E2, R2>) => HttpClient.With<E, R>
export declare const layerMergedContext: <E, R>(effect: Effect.Effect<HttpClient, E, R>) => Layer<HttpClient, E, R>
```

## [3]-[HTTP_CLIENT_REQUEST]

Immutable request value. The `Options.NoBody`/`Options.NoUrl` projections derive off the one `Options` interface.

```ts
// @effect/platform/HttpClientRequest
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface HttpClientRequest extends Inspectable, Pipeable {
  readonly [TypeId]: TypeId
  readonly method: HttpMethod
  readonly url: string
  readonly urlParams: UrlParams.UrlParams
  readonly hash: Option.Option<string>
  readonly headers: Headers.Headers
  readonly body: Body.HttpBody
}

export interface Options {
  readonly method?: HttpMethod | undefined
  readonly url?: string | URL | undefined
  readonly urlParams?: UrlParams.Input | undefined
  readonly hash?: string | undefined
  readonly headers?: Headers.Input | undefined
  readonly body?: Body.HttpBody | undefined
  readonly accept?: string | undefined
  readonly acceptJson?: boolean | undefined
}
export declare namespace Options {
  interface NoBody extends Omit<Options, "method" | "url" | "body"> {}
  interface NoUrl extends Omit<Options, "method" | "url"> {}
}

export declare const make: <M extends HttpMethod>(method: M) => (url: string | URL, options?: (M extends "GET" | "HEAD" ? Options.NoBody : Options.NoUrl) | undefined) => HttpClientRequest
export declare const get: (url: string | URL, options?: Options.NoBody) => HttpClientRequest
export declare const post: (url: string | URL, options?: Options.NoUrl) => HttpClientRequest
export declare const head: (url: string | URL, options?: Options.NoBody) => HttpClientRequest
export declare const options: (url: string | URL, options?: Options.NoUrl) => HttpClientRequest
// patch / put / del follow the NoUrl shape
```

## [4]-[HTTP_CLIENT_RESPONSE]

```ts
// @effect/platform/HttpClientResponse
export declare const TypeId: unique symbol
export interface HttpClientResponse extends IncomingMessage.HttpIncomingMessage<Error.ResponseError> {
  readonly [TypeId]: TypeId
  readonly request: ClientRequest.HttpClientRequest
  readonly status: number
  readonly cookies: Cookies.Cookies
  readonly formData: Effect.Effect<FormData, Error.ResponseError>
}

export declare const fromWeb: (request: ClientRequest.HttpClientRequest, source: Response) => HttpClientResponse

export declare const schemaJson: <R, I extends { readonly status?: number; readonly headers?: Readonly<Record<string, string>>; readonly body?: unknown }, A>(schema: Schema.Schema<A, I, R>, options?: ParseOptions) => (self: HttpClientResponse) => Effect.Effect<A, Error.ResponseError | ParseResult.ParseError, R>
export declare const schemaNoBody: <R, I extends { readonly status?: number; readonly headers?: Readonly<Record<string, string>> }, A>(schema: Schema.Schema<A, I, R>, options?: ParseOptions) => (self: HttpClientResponse) => Effect.Effect<A, ParseResult.ParseError, R>

export declare const stream: <E, R>(effect: Effect.Effect<HttpClientResponse, E, R>) => Stream.Stream<Uint8Array, Error.ResponseError | E, R>

export declare const matchStatus: { <const Cases extends { readonly [status: number]: (_: HttpClientResponse) => any; readonly "2xx"?: …; readonly "3xx"?: …; readonly "4xx"?: …; readonly "5xx"?: …; readonly orElse: (_: HttpClientResponse) => any }>(self: HttpClientResponse, cases: Cases): … /* + curried */ }
export declare const filterStatusOk: (self: HttpClientResponse) => Effect.Effect<HttpClientResponse, Error.ResponseError>

export { schemaBodyJson, schemaBodyUrlParams, schemaHeaders } from "./HttpIncomingMessage.js"
```

## [5]-[HTTP_CLIENT_ERROR]

The HTTP client fault rail — two `_tag`-discriminated `YieldableError` classes.

```ts
// @effect/platform/HttpClientError
export declare const TypeId: unique symbol
export declare const isHttpClientError: (u: unknown) => u is HttpClientError
export type HttpClientError = RequestError | ResponseError

export declare class RequestError extends RequestError_base<{
  readonly request: ClientRequest.HttpClientRequest
  readonly reason: "Transport" | "Encode" | "InvalidUrl"
  readonly cause?: unknown
  readonly description?: string
}> {
  get methodAndUrl(): string
  get message(): string
}

export declare class ResponseError extends ResponseError_base<{
  readonly request: ClientRequest.HttpClientRequest
  readonly response: ClientResponse.HttpClientResponse
  readonly reason: "StatusCode" | "Decode" | "EmptyBody"
  readonly cause?: unknown
  readonly description?: string
}> {
  get methodAndUrl(): string
  get message(): string
}
```

## [6]-[REQUEST_SUPPORT]

```ts
// @effect/platform/HttpMethod
export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH" | "HEAD" | "OPTIONS"
export declare namespace HttpMethod {
  type NoBody = "GET" | "HEAD" | "OPTIONS"
  type WithBody = Exclude<HttpMethod, NoBody>
}
export declare const hasBody: (method: HttpMethod) => boolean
export declare const all: ReadonlySet<HttpMethod>
export declare const isHttpMethod: (u: unknown) => u is HttpMethod
```

```ts
// @effect/platform/Headers
export declare const HeadersTypeId: unique symbol
export interface Headers extends Redactable {
  readonly [HeadersTypeId]: HeadersTypeId
  readonly [key: string]: string
}
export type Input = Record.ReadonlyRecord<string, string | ReadonlyArray<string> | undefined> | Iterable<readonly [string, string]>
export declare const schema: Schema.Schema<Headers, Record.ReadonlyRecord<string, string>>
export declare const empty: Headers
export declare const fromInput: (input?: Input) => Headers
export declare const set: { (self: Headers, key: string, value: string): Headers /* + curried */ }
export declare const setAll: { (self: Headers, headers: Input): Headers /* + curried */ }
export declare const merge: { (self: Headers, headers: Headers): Headers /* + curried */ }
export declare const get: { (self: Headers, key: string): Option.Option<string> /* + curried */ }
export declare const has: { (self: Headers, key: string): boolean /* + curried */ }
export declare const remove: { (self: Headers, key: string | RegExp | ReadonlyArray<string | RegExp>): Headers /* + curried */ }
export declare const redact: { (self: Headers, key: string | RegExp | ReadonlyArray<string | RegExp>): Record<string, string | Redacted.Redacted> /* + curried */ }
export declare const currentRedactedNames: FiberRef.FiberRef<ReadonlyArray<string | RegExp>>
```

```ts
// @effect/platform/HttpBody
export type HttpBody = Empty | Raw | Uint8Array | FormData | Stream
export type ErrorReason = { readonly _tag: "JsonError"; readonly error: unknown } | { readonly _tag: "SchemaError"; readonly error: ParseResult.ParseError }
export interface HttpBodyError { readonly [ErrorTypeId]: ErrorTypeId; readonly _tag: "HttpBodyError"; readonly reason: ErrorReason }

export declare const empty: Empty
export declare const raw: (body: unknown, options?: { readonly contentType?: string; readonly contentLength?: number }) => Raw
export declare const uint8Array: (body: globalThis.Uint8Array, contentType?: string) => Uint8Array
export declare const text: (body: string, contentType?: string) => Uint8Array
export declare const json: (body: unknown, contentType?: string) => Effect.Effect<Uint8Array, HttpBodyError>
export declare const jsonSchema: <A, I, R>(schema: Schema.Schema<A, I, R>) => (body: A, contentType?: string) => Effect.Effect<Uint8Array, HttpBodyError, R>
export declare const urlParams: (urlParams: UrlParams.UrlParams, contentType?: string) => Uint8Array
export declare const formData: (body: globalThis.FormData) => FormData
export declare const stream: (body: Stream_.Stream<globalThis.Uint8Array, unknown>, contentType?: string, contentLength?: number) => Stream
```

```ts
// @effect/platform/UrlParams
export interface UrlParams extends ReadonlyArray<readonly [string, string]> {}
export type Input = CoercibleRecord | Iterable<readonly [string, Coercible]> | URLSearchParams
export type Coercible = string | number | bigint | boolean | null | undefined
export declare const empty: UrlParams
export declare const fromInput: (input: Input) => UrlParams
export declare const set: { (self: UrlParams, key: string, value: Coercible): UrlParams /* + curried */ }
export declare const append: { (self: UrlParams, key: string, value: Coercible): UrlParams /* + curried */ }
export declare const getFirst: { (self: UrlParams, key: string): Option.Option<string> /* + curried */ }
export declare const makeUrl: (url: string, params: UrlParams, hash: Option.Option<string>) => Either.Either<URL, Error>
export declare const schemaStruct: <A, I extends Record<string, string | ReadonlyArray<string> | undefined>, R>(schema: Schema.Schema<A, I, R>, options?: ParseOptions) => (self: UrlParams) => Effect.Effect<A, ParseResult.ParseError, R>
```

```ts
// @effect/platform/Cookies
export declare const TypeId: unique symbol
export interface Cookies extends Pipeable, Inspectable.Inspectable {
  readonly [TypeId]: TypeId
  readonly cookies: Record.ReadonlyRecord<string, Cookie>
}
export interface Cookie extends Inspectable.Inspectable {
  readonly [CookieTypeId]: CookieTypeId
  readonly name: string; readonly value: string; readonly valueEncoded: string
  readonly options?: {
    readonly domain?: string; readonly expires?: Date; readonly maxAge?: Duration.DurationInput; readonly path?: string
    readonly priority?: "low" | "medium" | "high"; readonly httpOnly?: boolean; readonly secure?: boolean
    readonly partitioned?: boolean; readonly sameSite?: "lax" | "strict" | "none"
  }
}
export declare class CookiesError extends CookiesError_base<{ readonly reason: "InvalidName" | "InvalidValue" | "InvalidDomain" | "InvalidPath" | "InfinityMaxAge" }> {}
export declare const empty: Cookies
export declare const fromSetCookie: (headers: Iterable<string> | string) => Cookies
export declare function makeCookie(name: string, value: string, options?: Cookie["options"]): Either.Either<Cookie, CookiesError>
export declare const get: { (self: Cookies, name: string): Option.Option<Cookie> /* + curried */ }
export declare const merge: { (self: Cookies, that: Cookies): Cookies /* + curried */ }
```

## [7]-[PLATFORM_ERROR]

The platform-wide fault rail — two `Schema.TaggedError` classes sharing a `module` literal axis.

```ts
// @effect/platform/Error
export declare const TypeId: unique symbol
export declare const isPlatformError: (u: unknown) => u is PlatformError

export declare const Module: Schema.Literal<["Clipboard", "Command", "FileSystem", "KeyValueStore", "Path", "Stream", "Terminal"]>

export declare class BadArgument extends BadArgument_base {
  readonly [TypeId]: typeof TypeId
  get message(): string
}

export declare const SystemErrorReason: Schema.Literal<["AlreadyExists", "BadResource", "Busy", "InvalidData", "NotFound", "PermissionDenied", "TimedOut", "UnexpectedEof", "Unknown", "WouldBlock", "WriteZero"]>
export type SystemErrorReason = typeof SystemErrorReason.Type

export declare class SystemError extends SystemError_base {
  readonly [TypeId]: typeof TypeId
  get message(): string
}

export type PlatformError = BadArgument | SystemError
export declare const PlatformError: Schema.Union<[typeof BadArgument, typeof SystemError]>

export declare const TypeIdError: <const TypeId extends symbol, const Tag extends string>(typeId: TypeId, tag: Tag) => new <A extends Record<string, any>>(args: Simplify<A>) => Cause.YieldableError & Record<TypeId, TypeId> & { readonly _tag: Tag } & Readonly<A>
```

## [8]-[KEY_VALUE_STORE]

The platform key-value face. `forSchema` derives a typed `SchemaStore` projection off the one store.

```ts
// @effect/platform/KeyValueStore
export declare const TypeId: unique symbol
export interface KeyValueStore {
  readonly [TypeId]: TypeId
  readonly get: (key: string) => Effect.Effect<Option.Option<string>, PlatformError.PlatformError>
  readonly getUint8Array: (key: string) => Effect.Effect<Option.Option<Uint8Array>, PlatformError.PlatformError>
  readonly set: (key: string, value: string | Uint8Array) => Effect.Effect<void, PlatformError.PlatformError>
  readonly remove: (key: string) => Effect.Effect<void, PlatformError.PlatformError>
  readonly clear: Effect.Effect<void, PlatformError.PlatformError>
  readonly size: Effect.Effect<number, PlatformError.PlatformError>
  readonly modify: (key: string, f: (value: string) => string) => Effect.Effect<Option.Option<string>, PlatformError.PlatformError>
  readonly modifyUint8Array: (key: string, f: (value: Uint8Array) => Uint8Array) => Effect.Effect<Option.Option<Uint8Array>, PlatformError.PlatformError>
  readonly has: (key: string) => Effect.Effect<boolean, PlatformError.PlatformError>
  readonly isEmpty: Effect.Effect<boolean, PlatformError.PlatformError>
  readonly forSchema: <A, I, R>(schema: Schema.Schema<A, I, R>) => SchemaStore<A, R>
}
export declare namespace KeyValueStore { type AnyStore = KeyValueStore | SchemaStore<any, any> }
export declare const KeyValueStore: Context.Tag<KeyValueStore, KeyValueStore>

export declare const make: (impl: Omit<KeyValueStore, typeof TypeId | "has" | "modify" | "modifyUint8Array" | "isEmpty" | "forSchema"> & Partial<KeyValueStore>) => KeyValueStore
export declare const prefix: { <S extends KeyValueStore.AnyStore>(self: S, prefix: string): S /* + curried */ }
export declare const layerMemory: Layer.Layer<KeyValueStore>
export declare const layerStorage: (evaluate: LazyArg<Storage>) => Layer.Layer<KeyValueStore>
export declare const layerFileSystem: (directory: string) => Layer.Layer<KeyValueStore, PlatformError.PlatformError, FileSystem.FileSystem | Path.Path>

export declare const SchemaStoreTypeId: unique symbol
export interface SchemaStore<A, R> {
  readonly [SchemaStoreTypeId]: SchemaStoreTypeId
  readonly get: (key: string) => Effect.Effect<Option.Option<A>, PlatformError.PlatformError | ParseResult.ParseError, R>
  readonly set: (key: string, value: A) => Effect.Effect<void, PlatformError.PlatformError | ParseResult.ParseError, R>
  readonly remove: (key: string) => Effect.Effect<void, PlatformError.PlatformError>
  readonly clear: Effect.Effect<void, PlatformError.PlatformError>
  readonly size: Effect.Effect<number, PlatformError.PlatformError>
  readonly modify: (key: string, f: (value: A) => A) => Effect.Effect<Option.Option<A>, PlatformError.PlatformError | ParseResult.ParseError, R>
  readonly has: (key: string) => Effect.Effect<boolean, PlatformError.PlatformError>
  readonly isEmpty: Effect.Effect<boolean, PlatformError.PlatformError>
}
export declare const layerSchema: <A, I, R>(schema: Schema.Schema<A, I, R>, tagIdentifier: string) => { readonly tag: Context.Tag<SchemaStore<A, R>, SchemaStore<A, R>>; readonly layer: Layer.Layer<SchemaStore<A, R>, never, KeyValueStore> }
```

## [9]-[WORKER_POOL]

The `DecodeWorkerPool` backing surface: a `PlatformWorker` spawns a `BackingWorker`, a `WorkerManager` lifts it into typed `Worker`s, and `makePool`/`makePoolLayer` build the pool.

```ts
// @effect/platform/Worker
export interface BackingWorker<I, O> {
  readonly send: (message: I, transfers?: ReadonlyArray<unknown>) => Effect.Effect<void, WorkerError>
  readonly run: <A, E, R>(handler: (_: BackingWorker.Message<O>) => Effect.Effect<A, E, R>) => Effect.Effect<never, E | WorkerError, R>
}
export declare namespace BackingWorker { type Message<O> = readonly [ready: 0] | readonly [data: 1, O] }

export declare const PlatformWorkerTypeId: unique symbol
export interface PlatformWorker {
  readonly [PlatformWorkerTypeId]: PlatformWorkerTypeId
  readonly spawn: <I, O>(id: number) => Effect.Effect<BackingWorker<I, O>, WorkerError, Spawner>
}
export declare const PlatformWorker: Context.Tag<PlatformWorker, PlatformWorker>

export interface Worker<I, O, E = never> {
  readonly id: number
  readonly execute: (message: I) => Stream.Stream<O, E | WorkerError>
  readonly executeEffect: (message: I) => Effect.Effect<O, E | WorkerError>
}
export interface Spawner { readonly _: unique symbol }
export declare const Spawner: Context.Tag<Spawner, SpawnerFn<unknown>>
export interface SpawnerFn<W = unknown> { (id: number): W }

export interface WorkerPool<I, O, E = never> {
  readonly backing: Pool.Pool<Worker<I, O, E>, WorkerError>
  readonly broadcast: (message: I) => Effect.Effect<void, E | WorkerError>
  readonly execute: (message: I) => Stream.Stream<O, E | WorkerError>
  readonly executeEffect: (message: I) => Effect.Effect<O, E | WorkerError>
}

export declare const WorkerManagerTypeId: unique symbol
export interface WorkerManager {
  readonly [WorkerManagerTypeId]: WorkerManagerTypeId
  readonly spawn: <I, O, E>(options: Worker.Options<I>) => Effect.Effect<Worker<I, O, E>, WorkerError, Scope.Scope | Spawner>
}
export declare const WorkerManager: Context.Tag<WorkerManager, WorkerManager>
export declare const layerManager: Layer.Layer<WorkerManager, never, PlatformWorker>

export declare const makePool: <I, O, E>(options: WorkerPool.Options<I>) => Effect.Effect<WorkerPool<I, O, E>, WorkerError, WorkerManager | Spawner | Scope.Scope>
export declare const makePoolLayer: <Tag, I, O, E>(tag: Context.Tag<Tag, WorkerPool<I, O, E>>, options: WorkerPool.Options<I>) => Layer.Layer<Tag, WorkerError, WorkerManager | Spawner>

export interface SerializedWorkerPool<I extends Schema.TaggedRequest.All> {
  readonly backing: Pool.Pool<SerializedWorker<I>, WorkerError>
  readonly execute: <Req extends I>(message: Req) => Req extends Schema.WithResult<infer A, infer _I, infer E, infer _EI, infer R> ? Stream.Stream<A, E | WorkerError | ParseResult.ParseError, R> : never
  readonly executeEffect: <Req extends I>(message: Req) => Req extends Schema.WithResult<infer A, infer _I, infer E, infer _EI, infer R> ? Effect.Effect<A, E | WorkerError | ParseResult.ParseError, R> : never
  readonly broadcast: <Req extends I>(message: Req) => …
}
export declare const makePoolSerialized: <I extends Schema.TaggedRequest.All>(options: SerializedWorkerPool.Options<I>) => Effect.Effect<SerializedWorkerPool<I>, WorkerError, WorkerManager | Spawner | Scope.Scope>
export declare const makePoolSerializedLayer: <Tag, I extends Schema.TaggedRequest.All>(tag: Context.Tag<Tag, SerializedWorkerPool<I>>, options: SerializedWorkerPool.Options<I>) => Layer.Layer<Tag, WorkerError, WorkerManager | Spawner>
export declare const layerSpawner: <W = unknown>(spawner: SpawnerFn<W>) => Layer.Layer<Spawner, never, never>
```

`makePoolSerializedLayer` + a `Context.Tag<DecodeWorkerPool, SerializedWorkerPool<I>>` is the production form: the pool tag is what `DecodeWorkerPool` exposes, the platform-browser worker layer supplies `PlatformWorker`, and `layerSpawner` carries the `new Worker(url)` constructor.

## [10]-[WORKER_RUNNER]

The worker-side counterpart — `layerSerialized` registers schema-typed handlers per `_tag` inside the worker module; `launch` boots the runner layer.

```ts
// @effect/platform/WorkerRunner
export interface BackingRunner<I, O> {
  readonly run: <A, E, R>(handler: (portId: number, message: I) => Effect.Effect<A, E, R> | void) => Effect.Effect<void, never, Scope.Scope | R>
  readonly send: (portId: number, message: O, transfers?: ReadonlyArray<unknown>) => Effect.Effect<void>
  readonly disconnects?: Mailbox.ReadonlyMailbox<number>
}
export declare const PlatformRunnerTypeId: unique symbol
export interface PlatformRunner {
  readonly [PlatformRunnerTypeId]: PlatformRunnerTypeId
  readonly start: <I, O>(closeLatch: typeof CloseLatch.Service) => Effect.Effect<BackingRunner<I, O>, WorkerError>
}
export declare const PlatformRunner: Context.Tag<PlatformRunner, PlatformRunner>
export interface CloseLatch { readonly _: unique symbol }
export declare const CloseLatch: Context.Reference<CloseLatch, Deferred.Deferred<void, WorkerError>>
export declare const layerCloseLatch: Layer.Layer<CloseLatch>

export declare const make: <I, E, R, O>(process: (request: I) => Stream.Stream<O, E, R> | Effect.Effect<O, E, R>, options?: Runner.Options<I, O, E>) => Effect.Effect<void, WorkerError, PlatformRunner | R | Scope.Scope>
export declare const layer: <I, E, R, O>(process: (request: I) => Stream.Stream<O, E, R> | Effect.Effect<O, E, R>, options?: Runner.Options<I, O, E>) => Layer.Layer<never, WorkerError, R | PlatformRunner>
export declare const makeSerialized: <R, I, A extends Schema.TaggedRequest.All, const Handlers extends SerializedRunner.Handlers<A>>(schema: Schema.Schema<A, I, R>, handlers: Handlers) => Effect.Effect<void, WorkerError, PlatformRunner | Scope.Scope | R | SerializedRunner.HandlersContext<Handlers>>
export declare const layerSerialized: <R, I, A extends Schema.TaggedRequest.All, const Handlers extends SerializedRunner.Handlers<A>>(schema: Schema.Schema<A, I, R>, handlers: Handlers) => Layer.Layer<never, WorkerError, PlatformRunner | R | SerializedRunner.HandlersContext<Handlers>>
export declare const launch: <A, E, R>(layer: Layer.Layer<A, E, R>) => Effect.Effect<void, E | WorkerError, R>
```

## [11]-[WORKER_ERROR]

```ts
// @effect/platform/WorkerError
export declare const WorkerErrorTypeId: unique symbol
export declare const isWorkerError: (u: unknown) => u is WorkerError
export declare class WorkerError extends WorkerError_base {
  readonly [WorkerErrorTypeId]: WorkerErrorTypeId
  static readonly Cause: Schema.Schema<Cause.Cause<WorkerError>, Schema.CauseEncoded<WorkerErrorFrom, unknown>>
  static readonly encodeCause: (a: Cause.Cause<WorkerError>) => Schema.CauseEncoded<WorkerErrorFrom, unknown>
  static readonly decodeCause: (u: Schema.CauseEncoded<WorkerErrorFrom, unknown>) => Cause.Cause<WorkerError>
  get message(): string
}
export interface WorkerErrorFrom { readonly _tag: "WorkerError"; readonly reason: "spawn" | "decode" | "send" | "unknown" | "encode"; readonly cause: unknown }
```

## [12]-[TRANSFERABLE]

Zero-copy transfer collection for the worker boundary.

```ts
// @effect/platform/Transferable
export interface CollectorService {
  readonly addAll: (_: Iterable<globalThis.Transferable>) => Effect.Effect<void>
  readonly unsafeAddAll: (_: Iterable<globalThis.Transferable>) => void
  readonly read: Effect.Effect<Array<globalThis.Transferable>>
  readonly clear: Effect.Effect<Array<globalThis.Transferable>>
}
export declare class Collector extends Collector_base {}
export declare const makeCollector: Effect.Effect<CollectorService>
export declare const addAll: (tranferables: Iterable<globalThis.Transferable>) => Effect.Effect<void>
export declare const schema: { <A, I, R>(self: Schema.Schema<A, I, R>, f: (_: I) => Iterable<globalThis.Transferable>): Schema.Schema<A, I, R> /* + curried */ }
export declare const ImageData: Schema.Schema<ImageData>
export declare const MessagePort: Schema.Schema<MessagePort>
export declare const Uint8Array: Schema.Schema<Uint8Array>
```

`Transferable.Uint8Array` is the schema that marks a `Uint8Array` payload for zero-copy transfer.

## [13]-[CHANNEL_ENCODERS]

The framing-codec layer. Each is an `effect/Channel` transform between `Chunk<Uint8Array>` (or `Chunk<string>`) and `Chunk<A>`.

```ts
// @effect/platform/MsgPack
export declare class MsgPackError extends MsgPackError_base<{ readonly reason: "Pack" | "Unpack"; readonly cause: unknown }> {}
export declare const pack: <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<unknown>, IE | MsgPackError, IE, Done, Done>
export declare const unpack: <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<unknown>, Chunk.Chunk<Uint8Array>, IE | MsgPackError, IE, Done, Done>
export declare const packSchema: <A, I, R>(schema: Schema.Schema<A, I, R>) => <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<A>, IE | MsgPackError | ParseError, IE, Done, Done, R>
export declare const unpackSchema: <A, I, R>(schema: Schema.Schema<A, I, R>) => <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<A>, Chunk.Chunk<Uint8Array>, MsgPackError | ParseError | IE, IE, Done, Done, R>
export declare const duplexSchema: { <R, …, IA, II, IR, OA, OI, OR>(self: Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<Uint8Array>, OutErr, MsgPackError | ParseError | InErr, OutDone, InDone, R>, options: { readonly inputSchema: Schema.Schema<IA, II, IR>; readonly outputSchema: Schema.Schema<OA, OI, OR> }): Channel.Channel<Chunk.Chunk<OA>, Chunk.Chunk<IA>, MsgPackError | ParseError | OutErr, InErr, OutDone, InDone, R | IR | OR> /* + curried */ }
```

```ts
// @effect/platform/Ndjson
export declare class NdjsonError extends NdjsonError_base<{ readonly reason: "Pack" | "Unpack"; readonly cause: unknown }> {}
export interface NdjsonOptions { readonly ignoreEmptyLines?: boolean }
export declare const pack: <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<unknown>, IE | NdjsonError, IE, Done, Done>
export declare const packString: <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<string>, Chunk.Chunk<unknown>, IE | NdjsonError, IE, Done, Done>
export declare const unpack: <IE = never, Done = unknown>(options?: NdjsonOptions) => Channel.Channel<Chunk.Chunk<unknown>, Chunk.Chunk<Uint8Array>, IE | NdjsonError, IE, Done, Done>
export declare const packSchema: <A, I, R>(schema: Schema.Schema<A, I, R>) => <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<A>, IE | NdjsonError | ParseError, IE, Done, Done, R>
export declare const unpackSchema: <A, I, R>(schema: Schema.Schema<A, I, R>) => <IE = never, Done = unknown>(options?: NdjsonOptions) => Channel.Channel<Chunk.Chunk<A>, Chunk.Chunk<Uint8Array>, NdjsonError | ParseError | IE, IE, Done, Done, R>
// + packSchemaString / unpackSchemaString / duplex / duplexString / duplexSchema / duplexSchemaString
```

```ts
// @effect/platform/ChannelSchema
export declare const encode: <A, I, R>(schema: Schema.Schema<A, I, R>) => <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<I>, Chunk.Chunk<A>, IE | ParseError, IE, Done, Done, R>
export declare const decode: <A, I, R>(schema: Schema.Schema<A, I, R>) => <IE = never, Done = unknown>() => Channel.Channel<Chunk.Chunk<A>, Chunk.Chunk<I>, ParseError | IE, IE, Done, Done, R>
export declare const encodeUnknown: <A, I, R>(schema: Schema.Schema<A, I, R>) => …
export declare const decodeUnknown: <A, I, R>(schema: Schema.Schema<A, I, R>) => …
export declare const duplex: { <R, …, IA, II, IR, OA, OI, OR>(self: Channel.Channel<Chunk.Chunk<OI>, Chunk.Chunk<II>, OutErr, ParseError | InErr, OutDone, InDone, R>, options: { readonly inputSchema: Schema.Schema<IA, II, IR>; readonly outputSchema: Schema.Schema<OA, OI, OR> }): Channel.Channel<Chunk.Chunk<OA>, Chunk.Chunk<IA>, ParseError | OutErr, InErr, OutDone, InDone, R | IR | OR> /* + curried + duplexUnknown */ }
```

## [14]-[SOCKET]

WebSocket / transform-stream transport as an `effect/Channel`.

```ts
// @effect/platform/Socket
export declare const TypeId: unique symbol
export declare const Socket: Context.Tag<Socket, Socket>
export interface Socket {
  readonly [TypeId]: TypeId
  readonly run: <_, E = never, R = never>(handler: (_: Uint8Array) => Effect.Effect<_, E, R> | void, options?: { readonly onOpen?: Effect.Effect<void> }) => Effect.Effect<void, SocketError | E, R>
  readonly runRaw: <_, E = never, R = never>(handler: (_: string | Uint8Array) => Effect.Effect<_, E, R> | void, options?: { readonly onOpen?: Effect.Effect<void> }) => Effect.Effect<void, SocketError | E, R>
  readonly writer: Effect.Effect<(chunk: Uint8Array | string | CloseEvent) => Effect.Effect<void, SocketError>, never, Scope.Scope>
}
export declare class CloseEvent { readonly code: number; readonly reason?: string; constructor(code?: number, reason?: string); toString(): string }
export type SocketError = SocketGenericError | SocketCloseError
export declare class SocketGenericError extends SocketGenericError_base<{ readonly reason: "Write" | "Read" | "Open" | "OpenTimeout"; readonly cause: unknown }> {}
export declare class SocketCloseError extends SocketCloseError_base<{ readonly reason: "Close"; readonly code: number; readonly closeReason?: string }> {
  static is(u: unknown): u is SocketCloseError
  static isClean(isClean: (code: number) => boolean): (u: unknown) => u is SocketCloseError
}

export declare const toChannel: <IE>(self: Socket) => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<Uint8Array | string | CloseEvent>, SocketError | IE, IE, void, unknown>
export declare const makeChannel: <IE = never>() => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<Uint8Array | string | CloseEvent>, SocketError | IE, IE, void, unknown, Socket>

export interface WebSocketConstructor { readonly _: unique symbol }
export declare const WebSocketConstructor: Context.Tag<WebSocketConstructor, (url: string, protocols?: string | Array<string>) => globalThis.WebSocket>
export declare const layerWebSocketConstructorGlobal: Layer.Layer<WebSocketConstructor>
export declare const makeWebSocket: (url: string | Effect.Effect<string>, options?: { readonly closeCodeIsError?: (code: number) => boolean; readonly openTimeout?: DurationInput; readonly protocols?: string | Array<string> }) => Effect.Effect<Socket, never, WebSocketConstructor>
export declare const makeWebSocketChannel: <IE = never>(url: string, options?: { readonly closeCodeIsError?: (code: number) => boolean }) => Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<Uint8Array | string | CloseEvent>, SocketError | IE, IE, void, unknown, WebSocketConstructor>
export declare const layerWebSocket: (url: string, options?: { readonly closeCodeIsError?: (code: number) => boolean }) => Layer.Layer<Socket, never, WebSocketConstructor>
export declare const fromTransformStream: <R>(acquire: Effect.Effect<InputTransformStream, SocketError, R>, options?: { readonly closeCodeIsError?: (code: number) => boolean }) => Effect.Effect<Socket, never, Exclude<R, Scope.Scope>>
```

## [15]-[RUNTIME]

The node-entry boundary. `makeRunMain` is the factory; the platform driver (`@effect/platform-node` `NodeRuntime.runMain`) supplies the bound `RunMain`.

```ts
// @effect/platform/Runtime
export interface Teardown { <E, A>(exit: Exit.Exit<E, A>, onExit: (code: number) => void): void }
export declare const defaultTeardown: Teardown
export interface RunMain {
  (options?: { readonly disableErrorReporting?: boolean; readonly disablePrettyLogger?: boolean; readonly teardown?: Teardown }): <E, A>(effect: Effect.Effect<A, E>) => void
  <E, A>(effect: Effect.Effect<A, E>, options?: { readonly disableErrorReporting?: boolean; readonly disablePrettyLogger?: boolean; readonly teardown?: Teardown }): void
}
export declare const makeRunMain: (f: <E, A>(options: { readonly fiber: Fiber.RuntimeFiber<A, E>; readonly teardown: Teardown }) => void) => RunMain
```

## [16]-[CONFIG_PROVIDER]

Config-provider layers sourced from the filesystem.

```ts
// @effect/platform/PlatformConfigProvider
export declare const fromDotEnv: (paths: string) => Effect.Effect<ConfigProvider.ConfigProvider, PlatformError, FileSystem.FileSystem>
export declare const layerDotEnv: (path: string) => Layer.Layer<never, PlatformError, FileSystem.FileSystem>
export declare const layerDotEnvAdd: (path: string) => Layer.Layer<never, never, FileSystem.FileSystem>
export declare const fromFileTree: (options?: { readonly rootDirectory?: string }) => Effect.Effect<ConfigProvider.ConfigProvider, never, Path.Path | FileSystem.FileSystem>
export declare const layerFileTree: (options?: { readonly rootDirectory?: string }) => Layer.Layer<never, never, Path.Path | FileSystem.FileSystem>
export declare const layerFileTreeAdd: (options?: { readonly rootDirectory?: string }) => Layer.Layer<never, never, Path.Path | FileSystem.FileSystem>
```

`layerDotEnvAdd`/`layerFileTreeAdd` merge their provider onto the ambient `ConfigProvider` (additive); the non-`Add` forms replace it. Both require a `FileSystem` (and `Path` for the tree form), so they are node-tier-only.
