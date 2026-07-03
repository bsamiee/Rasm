# [API_CATALOGUE] @effect/workflow

`@effect/workflow` is the durable-workflow engine for Effect: a `Workflow` is a named, payload/success/error-schema'd unit whose `execute` body survives process restarts, replays deterministically, and resumes from durable checkpoints. The page owns the full module surface — the `Workflow` definition and its result algebra, run-once `Activity` units with retry/compensation, the `WorkflowEngine`/`WorkflowInstance` durable kernel services, the durable primitives (`DurableClock`, `DurableDeferred`, `DurableQueue`, `DurableRateLimiter`), and the RPC/HTTP proxy derivation (`WorkflowProxy`/`WorkflowProxyServer`). It is the single durable-execution owner; no hand-rolled state machine, saga, or retry loop survives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/workflow`
- package: `@effect/workflow` (0.18.2, MIT, © Effectful Technologies)
- import: `@effect/workflow` (namespace barrel) plus per-module deep paths `@effect/workflow/Workflow`, `/Activity`, `/WorkflowEngine`, `/DurableClock`, `/DurableDeferred`, `/DurableQueue`, `/DurableRateLimiter`, `/WorkflowProxy`, `/WorkflowProxyServer`
- rail: durable-work
- tier: `node` (`scope:node`; the durable engine is `ClusterWorkflowEngine` over `@effect/cluster` sharding + `@effect/sql` message storage)
- peer: `effect` `^3.21.2`, `@effect/experimental` `^0.60.0`, `@effect/platform` `^0.96.1`, `@effect/rpc` `^0.75.1`
- modules: `Workflow`, `Activity`, `WorkflowEngine`, `DurableClock`, `DurableDeferred`, `DurableQueue`, `DurableRateLimiter`, `WorkflowProxy`, `WorkflowProxyServer`
- capability: durable workflow definition/execution/poll/interrupt/resume, run-once activities with retry and compensation, durable clocks/deferreds/queues/rate-limiters, RPC/HTTP proxy derivation over a workflow set
- engine backend: the package ships only `WorkflowEngine.layerMemory` (in-memory, testing); the production `WorkflowEngine` is `@effect/cluster`'s `ClusterWorkflowEngine.layer` (`Layer<WorkflowEngine, never, Sharding | MessageStorage>`), so a durable workflow requires the cluster + SQL-storage substrate, never a bare in-memory layer in production

The package root re-exports every module under its namespace:

```ts contract
export * as Activity from "./Activity.js"
export * as DurableClock from "./DurableClock.js"
export * as DurableDeferred from "./DurableDeferred.js"
export * as DurableQueue from "./DurableQueue.js"
export * as DurableRateLimiter from "./DurableRateLimiter.js"
export * as Workflow from "./Workflow.js"
export * as WorkflowEngine from "./WorkflowEngine.js"
export * as WorkflowProxy from "./WorkflowProxy.js"
export * as WorkflowProxyServer from "./WorkflowProxyServer.js"
```

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Workflow module
- rail: durable-work

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]   | [RAIL]                          |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `Workflow.Workflow<Name, Payload, Success, Error>` | interface       | workflow definition             |
|  [02]   | `Workflow.Any`                                     | interface       | erased workflow upper bound     |
|  [03]   | `Workflow.AnyStructSchema`                         | interface       | payload schema upper bound      |
|  [04]   | `Workflow.AnyTaggedRequestSchema`                  | interface       | tagged-request payload bound    |
|  [05]   | `Workflow.Execution<Name>`                         | interface       | per-name execution marker       |
|  [06]   | `Workflow.Requirements<Workflows>`                 | type            | context-union extractor         |
|  [07]   | `Workflow.Payload<W>` / `Success<W>` / `Error<W>`  | type            | namespace type-level extractors |
|  [08]   | `Workflow.Result<A, E>`                            | union           | `Complete<A,E> \| Suspended`    |
|  [09]   | `Workflow.ResultEncoded<A, E>`                     | union           | encoded result shape            |
|  [10]   | `Workflow.Complete<A, E>`                          | class           | terminal-exit result case       |
|  [11]   | `Workflow.Suspended`                               | class           | suspended result case           |
|  [12]   | `Workflow.CompleteEncoded<A, E>`                   | interface       | encoded complete case           |
|  [13]   | `Workflow.CaptureDefects`                          | reference class | defect-capture annotation       |
|  [14]   | `Workflow.SuspendOnFailure`                        | reference class | suspend-on-error annotation     |
|  [15]   | `Workflow.TypeId` / `Workflow.ResultTypeId`        | symbol          | brand symbols                   |

[PUBLIC_TYPE_SCOPE]: Activity / durable primitives
- rail: durable-work

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]   | [RAIL]                           |
| :-----: | :--------------------------------------------------- | :-------------- | :------------------------------- |
|  [01]   | `Activity.Activity<Success, Error, R>`               | interface       | run-once unit (extends `Effect`) |
|  [02]   | `Activity.Any`                                       | interface       | erased activity bound            |
|  [03]   | `Activity.CurrentAttempt`                            | reference class | attempt-counter annotation       |
|  [04]   | `DurableClock.DurableClock`                          | interface       | durable timer                    |
|  [05]   | `DurableDeferred.DurableDeferred<Success, Error>`    | interface       | durable one-shot result          |
|  [06]   | `DurableDeferred.Token`                              | branded string  | deferred completion token        |
|  [07]   | `DurableDeferred.TokenParsed`                        | schema class    | parsed token struct              |
|  [08]   | `DurableQueue.DurableQueue<Payload, Success, Error>` | interface       | durable work queue               |

[PUBLIC_TYPE_SCOPE]: Engine / proxy
- rail: durable-work

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]    | [RAIL]                           |
| :-----: | :--------------------------------------------------- | :--------------- | :------------------------------- |
|  [01]   | `WorkflowEngine.WorkflowEngine`                      | TagClass service | durable-execution kernel         |
|  [02]   | `WorkflowEngine.WorkflowInstance`                    | TagClass service | per-execution runtime state      |
|  [03]   | `WorkflowEngine.Encoded`                             | interface        | erased engine impl shape         |
|  [04]   | `WorkflowProxy.ConvertRpcs<Workflows, Prefix>`       | type             | workflow→Rpc mapping             |
|  [05]   | `WorkflowProxy.ConvertHttpApi<Workflows>`            | type             | workflow→HttpApiEndpoint mapping |
|  [06]   | `WorkflowProxyServer.RpcHandlers<Workflows, Prefix>` | type             | derived rpc handler union        |

## [03]-[WORKFLOW]

The `Workflow` interface carries the four schema axes (`Name`, `Payload`, `Success`, `Error`) and the full execution/lifecycle method surface. All execution methods require `WorkflowEngine` plus the schemas' `Context`.

```ts contract
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface Workflow<Name extends string, Payload extends AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All> {
  readonly [TypeId]: TypeId
  readonly name: Name
  readonly payloadSchema: Payload
  readonly successSchema: Success
  readonly errorSchema: Error
  readonly annotations: Context.Context<never>
  annotate<I, S>(tag: Context.Tag<I, S>, value: S): Workflow<Name, Payload, Success, Error>
  annotateContext<I>(context: Context.Context<I>): Workflow<Name, Payload, Success, Error>
  readonly execute: <const Discard extends boolean = false>(
    payload: [keyof Payload["fields"]] extends [never] ? void : Schema.Simplify<Schema.Struct.Constructor<Payload["fields"]>>,
    options?: { readonly discard?: Discard }
  ) => Effect.Effect<
    Discard extends true ? string : Success["Type"],
    Discard extends true ? never : Error["Type"],
    WorkflowEngine | Payload["Context"] | Success["Context"] | Error["Context"]
  >
  readonly poll: (executionId: string) => Effect.Effect<
    Result<Success["Type"], Error["Type"]> | undefined,
    never,
    WorkflowEngine | Success["Context"] | Error["Context"]
  >
  readonly interrupt: (executionId: string) => Effect.Effect<void, never, WorkflowEngine>
  readonly resume: (executionId: string) => Effect.Effect<void, never, WorkflowEngine>
  readonly toLayer: <R>(
    execute: (payload: Payload["Type"], executionId: string) => Effect.Effect<Success["Type"], Error["Type"], R>
  ) => Layer.Layer<
    never,
    never,
    WorkflowEngine | Exclude<R, WorkflowEngine | WorkflowInstance | Execution<Name> | Scope.Scope> | Payload["Context"] | Success["Context"] | Error["Context"]
  >
  readonly executionId: (payload: Schema.Simplify<Schema.Struct.Constructor<Payload["fields"]>>) => Effect.Effect<string>
  readonly withCompensation: {
    <A, R2>(compensation: (value: A, cause: Cause.Cause<Error["Type"]>) => Effect.Effect<void, never, R2>): <E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R | R2 | WorkflowInstance | Execution<Name> | Scope.Scope>
    <A, E, R, R2>(effect: Effect.Effect<A, E, R>, compensation: (value: A, cause: Cause.Cause<Error["Type"]>) => Effect.Effect<void, never, R2>): Effect.Effect<A, E, R | R2 | WorkflowInstance | Execution<Name> | Scope.Scope>
  }
}

export interface AnyStructSchema extends Pipeable {
  readonly [Schema.TypeId]: any
  readonly make: any
  readonly Type: any
  readonly Encoded: any
  readonly Context: any
  readonly ast: AST.AST
  readonly fields: Schema.Struct.Fields
  readonly annotations: any
}

export interface AnyTaggedRequestSchema extends AnyStructSchema {
  readonly _tag: string
  readonly Type: PrimaryKey.PrimaryKey
  readonly success: Schema.Schema.Any
  readonly failure: Schema.Schema.All
}

export interface Execution<Name extends string> {
  readonly _: unique symbol
  readonly name: Name
}

export interface Any {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly payloadSchema: AnyStructSchema
  readonly successSchema: Schema.Schema.Any
  readonly errorSchema: Schema.Schema.All
  readonly annotations: Context.Context<never>
  readonly executionId: (payload: any) => Effect.Effect<string>
}

export type Requirements<Workflows extends Any> = Workflows extends Workflow<infer _Name, infer _Payload, infer _Success, infer _Error>
  ? _Payload["Context"] | _Success["Context"] | _Error["Context"]
  : never

export declare namespace Workflow {
  type Payload<W extends Workflow<any, any, any, any>> = W extends Workflow<any, infer Payload, any, any> ? Payload["Type"] : never
  type Success<W extends Workflow<any, any, any, any>> = W extends Workflow<any, any, infer Success, any> ? Success["Type"] : never
  type Error<W extends Workflow<any, any, any, any>> = W extends Workflow<any, any, any, infer Error> ? Error["Type"] : never
}
```

[ENTRYPOINT_SCOPE]: Workflow constructors
- rail: durable-work

```ts contract
export declare const make: <
  const Name extends string,
  Payload extends Schema.Struct.Fields | AnyStructSchema,
  Success extends Schema.Schema.Any = typeof Schema.Void,
  Error extends Schema.Schema.All = typeof Schema.Never
>(options: {
  readonly name: Name
  readonly payload: Payload
  readonly idempotencyKey: (payload: Payload extends Schema.Struct.Fields ? Schema.Struct.Type<Payload> : Payload["Type"]) => string
  readonly success?: Success
  readonly error?: Error
  readonly suspendedRetrySchedule?: Schedule.Schedule<any, unknown> | undefined
  readonly annotations?: Context.Context<never>
}) => Workflow<Name, Payload extends Schema.Struct.Fields ? Schema.Struct<Payload> : Payload, Success, Error>

export declare const fromTaggedRequest: <S extends AnyTaggedRequestSchema>(
  schema: S,
  options?: { readonly suspendedRetrySchedule?: Schedule.Schedule<any, unknown> | undefined }
) => Workflow<S["_tag"], S, S["success"], S["failure"]>
```

[ENTRYPOINT_SCOPE]: Result algebra + scope/compensation operators
- rail: durable-work

```ts contract
export declare const ResultTypeId: unique symbol
export type ResultTypeId = typeof ResultTypeId

export type Result<A, E> = Complete<A, E> | Suspended
export type ResultEncoded<A, E> = CompleteEncoded<A, E> | typeof Suspended.Encoded
export declare const isResult: <A = unknown, E = unknown>(u: unknown) => u is Result<A, E>

export declare class Complete<A, E> extends Complete_base<{ readonly exit: Exit.Exit<A, E> }> {
  readonly [ResultTypeId]: ResultTypeId
  static SchemaFromSelf<Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(_options: { readonly success: Success; readonly error: Error }): Schema.Schema<Complete<Success["Type"], Error["Type"]>>
  static SchemaEncoded<Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: { readonly success: Success; readonly error: Error }): Schema.Struct<{ _tag: Schema.tag<"Complete">; exit: Schema.Exit<Success, Error, typeof Schema.Defect> }>
  static Schema<Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: { readonly success: Success; readonly error: Error }): Schema.Schema<Complete<Success["Type"], Error["Type"]>, CompleteEncoded<Success["Encoded"], Error["Encoded"]>>
}

export interface CompleteEncoded<A, E> {
  readonly _tag: "Complete"
  readonly exit: Schema.ExitEncoded<A, E, unknown>
}

export declare class Suspended extends Suspended_base {
  readonly [ResultTypeId]: ResultTypeId
}

export declare const Result: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: {
  readonly success: Success
  readonly error: Error
}) => Schema.Schema<Result<Success["Type"], Error["Type"]>, ResultEncoded<Success["Encoded"], Error["Encoded"]>, Success["Context"] | Error["Context"]>

export declare const intoResult: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<Result<A, E>, never, Exclude<R, Scope.Scope> | WorkflowInstance>
export declare const wrapActivityResult: <A, E, R>(effect: Effect.Effect<A, E, R>, isSuspend: (value: A) => boolean) => Effect.Effect<A, E, R | WorkflowInstance>

export declare const scope: Effect.Effect<Scope.Scope, never, WorkflowInstance>
export declare const provideScope: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, Exclude<R, Scope.Scope> | WorkflowInstance>
export declare const addFinalizer: <R>(f: (exit: Exit.Exit<unknown, unknown>) => Effect.Effect<void, never, R>) => Effect.Effect<void, never, WorkflowInstance | R>
export declare const suspend: (instance: WorkflowInstance["Type"]) => Effect.Effect<never>

export declare const withCompensation: {
  <A, R2>(compensation: (value: A, cause: Cause.Cause<unknown>) => Effect.Effect<void, never, R2>): <E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R | R2 | WorkflowInstance | Scope.Scope>
  <A, E, R, R2>(effect: Effect.Effect<A, E, R>, compensation: (value: A, cause: Cause.Cause<unknown>) => Effect.Effect<void, never, R2>): Effect.Effect<A, E, R | R2 | WorkflowInstance | Scope.Scope>
}

export declare class CaptureDefects extends Context.ReferenceClass<CaptureDefects, "@effect/workflow/Workflow/CaptureDefects", boolean> {}
export declare class SuspendOnFailure extends Context.ReferenceClass<SuspendOnFailure, "@effect/workflow/Workflow/SuspendOnFailure", boolean> {}
```

`CaptureDefects` defaults to `true` (defects captured into the result). `SuspendOnFailure` set to `true` suspends the workflow on any error for later `Workflow.resume(executionId)`.

## [04]-[ACTIVITY]

`Activity` is the run-once unit inside a workflow body. The interface extends `Effect.Effect`, so an activity is directly yieldable; `execute`/`executeEncoded` carry the `Scope` requirement for explicit-scope use.

```ts contract
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface Activity<Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never, R = never>
  extends Effect.Effect<Success["Type"], Error["Type"], Success["Context"] | Error["Context"] | R | WorkflowEngine | WorkflowInstance> {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly successSchema: Success
  readonly errorSchema: Error
  readonly exitSchema: Schema.Schema<Exit.Exit<Success["Type"], Error["Type"]>, Exit.Exit<Success["Encoded"], Error["Encoded"]>, Success["Context"] | Error["Context"]>
  readonly execute: Effect.Effect<Success["Type"], Error["Type"], Success["Context"] | Error["Context"] | R | Scope | WorkflowEngine | WorkflowInstance>
  readonly executeEncoded: Effect.Effect<Success["Encoded"], Error["Encoded"], Success["Context"] | Error["Context"] | R | Scope | WorkflowEngine | WorkflowInstance>
}

export interface Any {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly successSchema: Schema.Schema.Any
  readonly errorSchema: Schema.Schema.All
  readonly execute: Effect.Effect<any, any, any>
  readonly executeEncoded: Effect.Effect<any, any, any>
}

export declare const make: <R, Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never>(options: {
  readonly name: string
  readonly success?: Success | undefined
  readonly error?: Error | undefined
  readonly execute: Effect.Effect<Success["Type"], Error["Type"], R>
  readonly interruptRetryPolicy?: Schedule.Schedule<any, Cause.Cause<unknown>> | undefined
}) => Activity<Success, Error, Exclude<R, WorkflowInstance | WorkflowEngine | Scope>>

export declare const retry: {
  <E, O extends Types.NoExcessProperties<Omit<Effect.Retry.Options<E>, "schedule">, O>>(options: O): <A, R>(self: Effect.Effect<A, E, R>) => Effect.Retry.Return<R, E, A, O>
  <A, E, R, O extends Types.NoExcessProperties<Omit<Effect.Retry.Options<E>, "schedule">, O>>(self: Effect.Effect<A, E, R>, options: O): Effect.Retry.Return<R, E, A, O>
}

export declare class CurrentAttempt extends Context.ReferenceClass<CurrentAttempt, "@effect/workflow/Activity/CurrentAttempt", number> {}

export declare const idempotencyKey: (name: string, options?: { readonly includeAttempt?: boolean | undefined } | undefined) => Effect.Effect<string, never, WorkflowInstance>

export declare const raceAll: <const Activities extends NonEmptyReadonlyArray<Any>>(name: string, activities: Activities) => Effect.Effect<
  (Activities[number] extends Activity<infer _A, infer _E, infer _R> ? _A["Type"] : never),
  (Activities[number] extends Activity<infer _A, infer _E_1, infer _R_1> ? _E_1["Type"] : never),
  (Activities[number] extends Activity<infer Success, infer Error, infer R> ? Success["Context"] | Error["Context"] | R : never) | WorkflowEngine | WorkflowInstance
>
```

## [05]-[ENGINE]

`WorkflowEngine` is the durable-execution kernel (a `Context.TagClass`); `WorkflowInstance` is the per-execution runtime state tag. `layerMemory` provides an in-memory engine for testing; `makeUnsafe` builds an engine from an `Encoded` impl.

```ts contract
export declare class WorkflowEngine extends Context.TagClass<WorkflowEngine, "@effect/workflow/WorkflowEngine", {
  readonly register: <Name extends string, Payload extends Workflow.AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(
    workflow: Workflow.Workflow<Name, Payload, Success, Error>,
    execute: (payload: Payload["Type"], executionId: string) => Effect.Effect<Success["Type"], Error["Type"], R>
  ) => Effect.Effect<void, never, Scope.Scope | Exclude<R, WorkflowEngine | WorkflowInstance | Workflow.Execution<Name> | Scope.Scope> | Payload["Context"] | Success["Context"] | Error["Context"]>
  readonly execute: <Name extends string, Payload extends Workflow.AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All, const Discard extends boolean = false>(
    workflow: Workflow.Workflow<Name, Payload, Success, Error>,
    options: { readonly executionId: string; readonly payload: Payload["Type"]; readonly discard?: Discard | undefined; readonly suspendedRetrySchedule?: Schedule.Schedule<any, unknown> | undefined }
  ) => Effect.Effect<Discard extends true ? string : Success["Type"], Error["Type"], Payload["Context"] | Success["Context"] | Error["Context"]>
  readonly poll: <Name extends string, Payload extends Workflow.AnyStructSchema, Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(
    workflow: Workflow.Workflow<Name, Payload, Success, Error>, executionId: string
  ) => Effect.Effect<Workflow.Result<Success["Type"], Error["Type"]> | undefined, never, Success["Context"] | Error["Context"]>
  readonly interrupt: (workflow: Workflow.Any, executionId: string) => Effect.Effect<void>
  readonly resume: (workflow: Workflow.Any, executionId: string) => Effect.Effect<void>
  readonly activityExecute: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(activity: Activity.Activity<Success, Error, R>, attempt: number) => Effect.Effect<Workflow.Result<Success["Type"], Error["Type"]>, never, Success["Context"] | Error["Context"] | R | WorkflowInstance>
  readonly deferredResult: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(deferred: DurableDeferred.DurableDeferred<Success, Error>) => Effect.Effect<Exit.Exit<Success["Type"], Error["Type"]> | undefined, never, WorkflowInstance>
  readonly deferredDone: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(deferred: DurableDeferred.DurableDeferred<Success, Error>, options: { readonly workflowName: string; readonly executionId: string; readonly deferredName: string; readonly exit: Exit.Exit<Success["Type"], Error["Type"]> }) => Effect.Effect<void, never, Success["Context"] | Error["Context"]>
  readonly scheduleClock: (workflow: Workflow.Any, options: { readonly executionId: string; readonly clock: DurableClock }) => Effect.Effect<void>
}> {}

export declare class WorkflowInstance extends Context.TagClass<WorkflowInstance, "@effect/workflow/WorkflowEngine/WorkflowInstance", {
  readonly executionId: string
  readonly workflow: Workflow.Any
  readonly scope: Scope.CloseableScope
  suspended: boolean
  interrupted: boolean
  cause: Cause.Cause<never> | undefined
  readonly activityState: { count: number; readonly latch: Effect.Latch }
}> {
  static initial(workflow: Workflow.Any, executionId: string): WorkflowInstance["Type"]
}

export interface Encoded {
  readonly register: (workflow: Workflow.Any, execute: (payload: object, executionId: string) => Effect.Effect<unknown, unknown, WorkflowInstance | WorkflowEngine>) => Effect.Effect<void, never, Scope.Scope>
  readonly execute: <const Discard extends boolean>(workflow: Workflow.Any, options: { readonly executionId: string; readonly payload: object; readonly discard: Discard; readonly parent?: WorkflowInstance["Type"] | undefined }) => Effect.Effect<Discard extends true ? void : Workflow.Result<unknown, unknown>>
  readonly poll: (workflow: Workflow.Any, executionId: string) => Effect.Effect<Workflow.Result<unknown, unknown> | undefined>
  readonly interrupt: (workflow: Workflow.Any, executionId: string) => Effect.Effect<void>
  readonly resume: (workflow: Workflow.Any, executionId: string) => Effect.Effect<void>
  readonly activityExecute: (activity: Activity.Any, attempt: number) => Effect.Effect<Workflow.Result<unknown, unknown>, never, WorkflowInstance>
  readonly deferredResult: (deferred: DurableDeferred.Any) => Effect.Effect<Exit.Exit<unknown, unknown> | undefined, never, WorkflowInstance>
  readonly deferredDone: (options: { readonly workflowName: string; readonly executionId: string; readonly deferredName: string; readonly exit: Exit.Exit<unknown, unknown> }) => Effect.Effect<void>
  readonly scheduleClock: (workflow: Workflow.Any, options: { readonly executionId: string; readonly clock: DurableClock }) => Effect.Effect<void>
}

export declare const makeUnsafe: (options: Encoded) => WorkflowEngine["Type"]
export declare const layerMemory: Layer.Layer<WorkflowEngine>
```

## [06]-[DURABLE_PRIMITIVES]

[ENTRYPOINT_SCOPE]: DurableClock
- rail: durable-work

```ts contract
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface DurableClock {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly duration: Duration.Duration
  readonly deferred: DurableDeferred.DurableDeferred<typeof Schema.Void>
}

export declare const make: (options: { readonly name: string; readonly duration: Duration.DurationInput }) => DurableClock
export declare const sleep: (options: {
  readonly name: string
  readonly duration: Duration.DurationInput
  readonly inMemoryThreshold?: Duration.DurationInput | undefined
}) => Effect.Effect<void, never, WorkflowEngine | WorkflowInstance>
```

`sleep.inMemoryThreshold` defaults to 60 seconds; durations at or below it run in-memory instead of scheduling a durable wake.

[ENTRYPOINT_SCOPE]: DurableDeferred
- rail: durable-work

```ts contract
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface DurableDeferred<Success extends Schema.Schema.Any, Error extends Schema.Schema.All = typeof Schema.Never> {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly successSchema: Success
  readonly errorSchema: Error
  readonly exitSchema: Schema.ExitFromSelf<Success, Error, typeof Schema.Defect>
  readonly withActivityAttempt: Effect.Effect<DurableDeferred<Success, Error>>
}

export interface Any {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly successSchema: Schema.Schema.Any
  readonly errorSchema: Schema.Schema.All
  readonly exitSchema: Schema.ExitFromSelf<any, any, any>
}

export declare const make: <Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never>(
  name: string,
  options?: { readonly success?: Success | undefined; readonly error?: Error | undefined }
) => DurableDeferred<Success, Error>

export { await_ as await }
declare const await_: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>) => Effect.Effect<Success["Type"], Error["Type"], WorkflowEngine | WorkflowInstance | Success["Context"] | Error["Context"]>

export declare const into: {
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>): <R>(effect: Effect.Effect<Success["Type"], Error["Type"], R>) => Effect.Effect<Success["Type"], Error["Type"], R | WorkflowEngine | WorkflowInstance | Success["Context"] | Error["Context"]>
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(effect: Effect.Effect<Success["Type"], Error["Type"], R>, self: DurableDeferred<Success, Error>): Effect.Effect<Success["Type"], Error["Type"], R | WorkflowEngine | WorkflowInstance | Success["Context"] | Error["Context"]>
}

export declare const raceAll: <const Effects extends NonEmptyReadonlyArray<Effect.Effect<any, any, any>>, SI, SR, EI, ER>(options: {
  name: string
  success: Schema.Schema<Effects[number] extends Effect.Effect<infer S, infer _E_2, infer _R_3> ? S : never, SI, SR>
  error: Schema.Schema<Effects[number] extends Effect.Effect<infer _S, infer E, infer _R_4> ? E : never, EI, ER>
  effects: Effects
}) => Effect.Effect<
  (Effects[number] extends Effect.Effect<infer _A, infer _E, infer _R> ? _A : never),
  (Effects[number] extends Effect.Effect<infer _A, infer _E_1, infer _R_1> ? _E_1 : never),
  (Effects[number] extends Effect.Effect<infer _A, infer _R_2, infer R> ? R : never) | SR | ER | WorkflowEngine | WorkflowInstance
>

export declare const TokenTypeId: unique symbol
export type TokenTypeId = typeof TokenTypeId
export type Token = Brand.Branded<string, TokenTypeId>
export declare const Token: Schema.brand<typeof Schema.String, typeof TokenTypeId>

export declare class TokenParsed extends TokenParsed_base {
  get asToken(): Token
  static readonly FromString: Schema.Schema<TokenParsed, string>
  static readonly fromString: (i: string, overrideOptions?: import("effect/SchemaAST").ParseOptions) => TokenParsed
  static readonly encode: (a: TokenParsed, overrideOptions?: import("effect/SchemaAST").ParseOptions) => string
}

export declare const token: <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>) => Effect.Effect<Token, never, WorkflowInstance>

export declare const tokenFromExecutionId: {
  (options: { readonly workflow: Workflow.Any; readonly executionId: string }): <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>) => Token
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>, options: { readonly workflow: Workflow.Any; readonly executionId: string }): Token
}

export declare const tokenFromPayload: {
  <W extends Workflow.Any>(options: { readonly workflow: W; readonly payload: Schema.Simplify<Schema.Struct.Constructor<W["payloadSchema"]["fields"]>> }): <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>) => Effect.Effect<Token>
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All, W extends Workflow.Any>(self: DurableDeferred<Success, Error>, options: { readonly workflow: W; readonly payload: Schema.Simplify<Schema.Struct.Constructor<W["payloadSchema"]["fields"]>> }): Effect.Effect<Token>
}

export declare const done: {
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: { readonly token: Token; readonly exit: Exit.Exit<Success["Type"], Error["Type"]> }): (self: DurableDeferred<Success, Error>) => Effect.Effect<void, never, WorkflowEngine | Success["Context"] | Error["Context"]>
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>, options: { readonly token: Token; readonly exit: Exit.Exit<Success["Type"], Error["Type"]> }): Effect.Effect<void, never, WorkflowEngine | Success["Context"] | Error["Context"]>
}

export declare const succeed: {
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: { readonly token: Token; readonly value: Success["Type"] }): (self: DurableDeferred<Success, Error>) => Effect.Effect<void, never, WorkflowEngine | Success["Context"]>
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>, options: { readonly token: Token; readonly value: Success["Type"] }): Effect.Effect<void, never, WorkflowEngine | Success["Context"]>
}

export declare const fail: {
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: { readonly token: Token; readonly error: Error["Type"] }): (self: DurableDeferred<Success, Error>) => Effect.Effect<void, never, WorkflowEngine | Error["Context"]>
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>, options: { readonly token: Token; readonly error: Error["Type"] }): Effect.Effect<void, never, WorkflowEngine | Error["Context"]>
}

export declare const failCause: {
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(options: { readonly token: Token; readonly cause: Cause.Cause<Error["Type"]> }): (self: DurableDeferred<Success, Error>) => Effect.Effect<void, never, WorkflowEngine | Error["Context"]>
  <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(self: DurableDeferred<Success, Error>, options: { readonly token: Token; readonly cause: Cause.Cause<Error["Type"]> }): Effect.Effect<void, never, WorkflowEngine | Error["Context"]>
}
```

[ENTRYPOINT_SCOPE]: DurableQueue (depends on `@effect/experimental/PersistedQueue`)
- rail: durable-work

```ts contract
export type TypeId = "~@effect/workflow/DurableQueue"
export declare const TypeId: TypeId

export interface DurableQueue<Payload extends Schema.Schema.Any, Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never> {
  readonly [TypeId]: TypeId
  readonly name: string
  readonly payloadSchema: Payload
  readonly idempotencyKey: (payload: Payload["Type"]) => string
  readonly deferred: DurableDeferred.DurableDeferred<Success, Error>
}

export declare const make: <Payload extends Schema.Schema.Any | Schema.Struct.Fields, Success extends Schema.Schema.Any = typeof Schema.Void, Error extends Schema.Schema.All = typeof Schema.Never>(options: {
  readonly name: string
  readonly payload: Payload
  readonly idempotencyKey: (payload: Payload extends Schema.Struct.Fields ? Schema.Struct<Payload>["Type"] : Payload["Type"]) => string
  readonly success?: Success | undefined
  readonly error?: Error | undefined
}) => DurableQueue<Payload extends Schema.Struct.Fields ? Schema.Struct<Payload> : Payload, Success, Error>

export declare const process: <Payload extends Schema.Schema.Any, Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(
  self: DurableQueue<Payload, Success, Error>,
  payload: Payload["Type"],
  options?: { readonly retrySchedule?: Schedule.Schedule<any, PersistedQueue.PersistedQueueError> | undefined }
) => Effect.Effect<Success["Type"], Error["Type"], WorkflowEngine.WorkflowEngine | WorkflowEngine.WorkflowInstance | PersistedQueue.PersistedQueueFactory | Success["Context"] | Error["Context"] | Payload["Context"]>

export declare const makeWorker: <Payload extends Schema.Schema.Any, Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(
  self: DurableQueue<Payload, Success, Error>,
  f: (payload: Payload["Type"]) => Effect.Effect<Success["Type"], Error["Type"], R>,
  options?: { readonly concurrency?: number | undefined } | undefined
) => Effect.Effect<never, never, WorkflowEngine.WorkflowEngine | PersistedQueue.PersistedQueueFactory | R | Payload["Context"] | Success["Context"] | Error["Context"]>

export declare const worker: <Payload extends Schema.Schema.Any, Success extends Schema.Schema.Any, Error extends Schema.Schema.All, R>(
  self: DurableQueue<Payload, Success, Error>,
  f: (payload: Payload["Type"]) => Effect.Effect<Success["Type"], Error["Type"], R>,
  options?: { readonly concurrency?: number | undefined } | undefined
) => Layer.Layer<never, never, WorkflowEngine.WorkflowEngine | PersistedQueue.PersistedQueueFactory | R | Payload["Context"] | Success["Context"] | Error["Context"]>
```

[ENTRYPOINT_SCOPE]: DurableRateLimiter (depends on `@effect/experimental/RateLimiter`)
- rail: durable-work

```ts contract
export declare const rateLimit: (options: {
  readonly name: string
  readonly algorithm?: "fixed-window" | "token-bucket" | undefined
  readonly window: Duration.DurationInput
  readonly limit: number
  readonly key: string
  readonly tokens?: number | undefined
}) => Activity.Activity<typeof Schema.Void, typeof RateLimiter.RateLimitStoreError, RateLimiter.RateLimiter>
```

## [07]-[PROXY]

`WorkflowProxy` derives an `RpcGroup` or `HttpApiGroup` from a workflow set; `WorkflowProxyServer` provides the matching handler layers. Each workflow yields three operations: the base execution, a `${Name}Discard` fire-and-forget, and a `${Name}Resume` resume-by-execution-id.

```ts contract
// WorkflowProxy
export declare const toRpcGroup: <const Workflows extends NonEmptyReadonlyArray<Workflow.Any>, const Prefix extends string = "">(
  workflows: Workflows,
  options?: { readonly prefix?: Prefix | undefined }
) => RpcGroup.RpcGroup<ConvertRpcs<Workflows[number], Prefix>>

export type ConvertRpcs<Workflows extends Workflow.Any, Prefix extends string> = Workflows extends Workflow.Workflow<infer _Name, infer _Payload, infer _Success, infer _Error>
  ? Rpc.Rpc<`${Prefix}${_Name}`, _Payload, _Success, _Error> | Rpc.Rpc<`${Prefix}${_Name}Discard`, _Payload> | Rpc.Rpc<`${Prefix}${_Name}Resume`, typeof ResumePayload>
  : never

export declare const toHttpApiGroup: <const Name extends string, const Workflows extends NonEmptyReadonlyArray<Workflow.Any>>(
  name: Name,
  workflows: Workflows
) => HttpApiGroup.HttpApiGroup<Name, ConvertHttpApi<Workflows[number]>>

export type ConvertHttpApi<Workflows extends Workflow.Any> = Workflows extends Workflow.Workflow<infer _Name, infer _Payload, infer _Success, infer _Error>
  ? HttpApiEndpoint.HttpApiEndpoint<_Name, "POST", never, never, _Payload["Type"], never, _Success["Type"], _Error["Type"], _Payload["Context"] | _Success["Context"], _Error["Context"]>
    | HttpApiEndpoint.HttpApiEndpoint<`${_Name}Discard`, "POST", never, never, _Payload["Type"], never, void, never, _Payload["Context"]>
    | HttpApiEndpoint.HttpApiEndpoint<`${_Name}Resume`, "POST", never, never, typeof ResumePayload.Type, never, void, never, typeof ResumePayload.Context>
  : never

// WorkflowProxyServer
export declare const layerHttpApi: <ApiId extends string, Groups extends HttpApiGroup.Any, ApiE, ApiR, Name extends HttpApiGroup.Name<Groups>, const Workflows extends NonEmptyReadonlyArray<Workflow.Any>>(
  api: HttpApi.HttpApi<ApiId, Groups, ApiE, ApiR>,
  name: Name,
  workflows: Workflows
) => Layer.Layer<ApiGroup<ApiId, Name>, never, WorkflowEngine | Workflow.Requirements<Workflows[number]>>

export declare const layerRpcHandlers: <const Workflows extends NonEmptyReadonlyArray<Workflow.Any>, const Prefix extends string = "">(
  workflows: Workflows,
  options?: { readonly prefix?: Prefix }
) => Layer.Layer<RpcHandlers<Workflows[number], Prefix>, never, WorkflowEngine | Workflow.Requirements<Workflows[number]>>

export type RpcHandlers<Workflows extends Workflow.Any, Prefix extends string> = Workflows extends Workflow.Workflow<infer _Name, infer _Payload, infer _Success, infer _Error>
  ? Rpc.Handler<`${Prefix}${_Name}`> | Rpc.Handler<`${Prefix}${_Name}Discard`> | Rpc.Handler<`${Prefix}${_Name}Resume`>
  : never
```

## [08]-[IMPLEMENTATION_LAW]

[WORKFLOW_TOPOLOGY]:
- a `Workflow` is four schema axes (`Name`/`Payload`/`Success`/`Error`) plus one durable `execute` body; the body is deterministic and replay-safe — every non-deterministic or side-effecting step is an `Activity` (run-once, memoized by `executionId` + activity name), so a replay after a crash re-derives pure control flow and reads completed activity results from storage rather than re-running them.
- `Workflow.make({ idempotencyKey })` makes an execution idempotent on the derived id: two `execute` calls with the same key join one execution. `execute({ discard: true })` is fire-and-forget (returns the `executionId` string); `poll`/`interrupt`/`resume` address a running execution by id.
- the result algebra is `Result = Complete<A,E> | Suspended`: `Suspended` is a first-class outcome (awaiting a `DurableDeferred`, a `DurableClock`, or `SuspendOnFailure`), resumed later by `Workflow.resume(executionId)` or a deferred completion token — never a blocked fiber holding a process.
- `withCompensation` registers a per-activity undo `(value, cause) => Effect<void>` run on downstream failure; `CaptureDefects` (default true) folds defects into the result; `SuspendOnFailure` suspends on any error for operator-driven resume.

[STACKS_WITH]:
- `@effect/cluster` `ClusterWorkflowEngine` (`effect-cluster.md`, `execution/engine#ENGINE`): the production `WorkflowEngine` is `ClusterWorkflowEngine.layer : Layer<WorkflowEngine, never, Sharding.Sharding | MessageStorage>`. A workflow becomes durable only when this layer is provided over the cluster substrate — `Sharding` places each execution on a single runner (single-writer replay), `MessageStorage` (SQL-backed via `execution/backplane`) persists the activity journal and deferred state. `WorkflowEngine.layerMemory` is the `@effect/vitest` test double, never the deployed engine.
- `@effect/sql` (`effect-sql.md`, `persistence/store#STORE`): the durable substrate is one `SqlClient` — the cluster `MessageStorage`/`RunnerStorage` layers ride the same `PgClient` the whole node tier shares, so workflow durability, entity mailboxes, and the outbox are one transactional store, not three.
- `@effect/experimental` `PersistedQueue` + `RateLimiter` (`effect-experimental.md`): `DurableQueue.process`/`worker` require `PersistedQueue.PersistedQueueFactory` — the durable, retrying work-queue substrate under a workflow's fan-out; `DurableRateLimiter.rateLimit` yields an `Activity` requiring `RateLimiter.RateLimiter`, so a per-key durable rate limit is one activity in the workflow body, composed above the per-tenant `messaging/quota#QUOTA` governor.
- `@effect/rpc` `RpcGroup` + `@effect/platform` `HttpApiGroup` (`effect-rpc.md`, `messaging/rpc#RPC`): `WorkflowProxy.toRpcGroup(workflows)` derives an `RpcGroup` (three procedures per workflow — `{Name}`/`{Name}Discard`/`{Name}Resume`) and `toHttpApiGroup` an `HttpApiGroup`; `WorkflowProxyServer.layerRpcHandlers`/`layerHttpApi` provide the matching handler layers requiring only `WorkflowEngine | Workflow.Requirements<Workflows>`. Exposing a workflow set is a projection, never a hand-written controller.
- `effect` `Schema`/`Schedule`/`Cause` (`../../.api/effect.md`): the four axes are `Schema.Schema`s (payload/success/error decoded at the durable boundary); `Activity.make({ interruptRetryPolicy })` and `Activity.retry({ ...Effect.Retry.Options })` take `effect/Schedule` policies; `withCompensation` folds over `Cause.Cause<Error>`. `Activity.CurrentAttempt` (a `Context.Reference`) reads the retry attempt inside the body.
- `DurableDeferred` + `DurableClock` as signal/timer rails: external completion (human approval, webhook, `DurableQueue` reply) awaits a `DurableDeferred` addressed by `token`/`tokenFromExecutionId`/`tokenFromPayload` and completed out-of-band via `done`/`succeed`/`fail`; a durable delay is `DurableClock.sleep` (`inMemoryThreshold` 60s — shorter sleeps run in-memory, longer schedule a durable wake), so a month-long "wait for X" survives every restart.
- `execution/saga#SAGA` + `agent/runtime#RUNTIME`: the saga is a `SagaStep` chain over `Activity` + `withCompensation` (engine-driven rollback on `StepOutcome` failure); the `DurableAgent` is `Activity` (each tool call) + `@effect/ai` composed on `ClusterWorkflowEngine`, journaling `AgentJournal` — both are workflow bodies, not bespoke state machines.

[LOCAL_ADMISSION]:
- Provide `ClusterWorkflowEngine.layer` (not `layerMemory`) in every non-test composition root; a `layerMemory` engine in production loses durability silently on restart.
- Model every non-deterministic step (clock read, random, network, DB write) as an `Activity` so replay is deterministic; a bare `Effect` in the workflow body re-runs on every replay and breaks exactly-once.
- Address long waits with `DurableDeferred`/`DurableClock`, never `Effect.sleep`/an in-memory `Deferred` — those do not survive a restart and pin a fiber.
- Derive the RPC/HTTP surface with `WorkflowProxy`; never hand-write an execute/resume controller per workflow.
- Register undo logic with `withCompensation` at the activity that needs rollback; never a manual try/compensate branch in the body.

[RAIL_LAW]:
- Package: `@effect/workflow`
- Owns: the durable-execution concern — workflow definition/execution/poll/interrupt/resume, run-once activities with retry + compensation, durable clocks/deferreds/queues/rate-limiters, and the RPC/HTTP proxy derivation — as the single durable-work owner over the `ClusterWorkflowEngine` substrate
- Accept: `Schema`-typed payload/success/error axes, `idempotencyKey` for execution identity, `Activity` for every side effect, `effect/Schedule` retry policies, `WorkflowProxy` for the wire surface, `DurableDeferred`/`DurableClock` for signals/timers
- Reject: a hand-rolled saga/state-machine/retry loop, `layerMemory` in production, a side effect run directly in the workflow body, `Effect.sleep`/in-memory `Deferred` for durable waits, and a bespoke per-workflow controller where `WorkflowProxy` projects the group
