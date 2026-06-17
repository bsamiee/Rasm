# [API_CATALOGUE] effect

Grounded from installed `node_modules/effect` type declarations (`dist/dts/*.d.ts`, 176 module files). Covers the `effect` core runtime library only — the held-v3 toolchain row. The combinator surfaces are scoped to those the six TS package charters consume; this is not an exhaustive dump of every re-export from `effect/index.d.ts`. Sibling packages (`@effect/platform`, `@effect/sql`, `@effect/sql-pg`, `@effect/rpc`, `@effect/cluster`, `@effect/workflow`) carry their own catalogue pages.

- package: `effect`
- version: `3.21.3` (v3 held past the async-local-storage patched line; v4 is a designed-only growth row)
- module shape: deep-import per concept (`effect/Effect`, `effect/Layer`, …); barrel re-export at `effect`
- dual signatures: most combinators are data-first/data-last overloaded (`{ (self, …); (…): (self) => … }`); the data-last curried arm is shown — it is the `pipe`-composable form

---

## [1]-[CORE_COMPUTATION]

### Effect

```ts
// effect/Effect
export interface Effect<out A, out E = never, out R = never>
  extends Effect.Variance<A, E, R>, Pipeable { /* + Symbol.iterator for gen do-notation */ }

export declare const EffectTypeId: unique symbol

export declare namespace Effect {
  type Success<T>  // A
  type Error<T>    // E
  type Context<T>  // R
}
```

`A` = success, `E` = typed error channel, `R` = required context (services). Three-channel discipline: every domain operation declares its failures in `E` and its dependencies in `R`. Generator do-notation via `Effect.gen(function*(){ … })`.

Constructors and lifters:

```ts
export declare const succeed: <A>(value: A) => Effect<A>
export declare const fail: <E>(error: E) => Effect<never, E>
export declare const sync: <A>(thunk: LazyArg<A>) => Effect<A>
export declare const die: (defect: unknown) => Effect<never>
export declare const gen: { /* (f: () => Generator<…>) => Effect<…>; (self, f) => … */ }
```

Transformation (data-last arm shown):

```ts
export declare const map:     <A, B>(f: (a: A) => B) => <E, R>(self: Effect<A, E, R>) => Effect<B, E, R>
export declare const flatMap: <A, B, E2, R2>(f: (a: A) => Effect<B, E2, R2>) => <E, R>(self: Effect<A, E, R>) => Effect<B, E | E2, R | R2>
export declare const tap:     <A, X, E2, R2>(f: (a: A) => Effect<X, E2, R2>) => <E, R>(self: Effect<A, E, R>) => Effect<A, E | E2, R | R2>
export declare const mapError:<E, E2>(f: (e: E) => E2) => <A, R>(self: Effect<A, E, R>) => Effect<A, E2, R>
```

Error rails (the typed-recovery surface; never `try/catch` in domain logic):

```ts
export declare const catchAll:  <E, A2, E2, R2>(f: (e: E) => Effect<A2, E2, R2>) => <A, R>(self: Effect<A, E, R>) => Effect<A | A2, E2, R | R2>
export declare const catchTag:  <K extends E["_tag"] & string, E, A1, E1, R1>(k: K, f: (e: …) => Effect<A1, E1, R1>) => <A, R>(self: Effect<A, E, R>) => Effect<…>
export declare const catchTags: <Cases>(cases: Cases) => <A, E, R>(self: Effect<A, E, R>) => Effect<…>
```

Concurrency:

```ts
export declare const all: <const Arg extends Iterable<Effect<any, any, any>> | Record<string, Effect<any, any, any>>, O extends Options>(arg: Arg, options?: O) => Effect<…>
export declare const forEach: <A, B, E, R>(self: Iterable<A>, f: (a: A, i: number) => Effect<B, E, R>, options?: { concurrency?: Concurrency }) => Effect<Array<B>, E, R>
export declare const fork: <A, E, R>(self: Effect<A, E, R>) => Effect<Fiber.RuntimeFiber<A, E>, never, R>
```

Resource and scope:

```ts
export declare const scoped:         <A, E, R>(effect: Effect<A, E, R>) => Effect<A, E, Exclude<R, Scope.Scope>>
export declare const acquireRelease: <A, E, R, X, R2>(acquire: Effect<A, E, R>, release: (a: A, exit: Exit.Exit<unknown, unknown>) => Effect<X, never, R2>) => Effect<A, E, R | R2 | Scope.Scope>
```

Resilience and observability:

```ts
export declare const retry:        <E, R2>(policy: Schedule.Schedule<any, E, R2>) => <A, R>(self: Effect<A, E, R>) => Effect<A, E, R | R2>
export declare const timeout:      <A, E, R>(duration: Duration.DurationInput) => (self: Effect<A, E, R>) => Effect<A, E | Cause.TimeoutException, R>
export declare const withSpan:     (name: string, options?: …) => <A, E, R>(self: Effect<A, E, R>) => Effect<A, E, Exclude<R, Tracer.ParentSpan>>
export declare const annotateLogs: (key: string, value: unknown) => <A, E, R>(self: Effect<A, E, R>) => Effect<A, E, R>
```

Context access and injection:

```ts
// context access is the Tag itself: `const s = yield* MyTag` inside Effect.gen. The optional/optional-effect accessors below are the named entry points (there is NO bare `Effect.service`).
export declare const serviceOption:   <I, S>(tag: Context.Tag<I, S>) => Effect<Option.Option<S>>
export declare const serviceOptional: <I, S>(tag: Context.Tag<I, S>) => Effect<S, Cause.NoSuchElementException>
export declare const provide:         <RIn, E, ROut>(layer: Layer<ROut, E, RIn>) => <A, E1, R>(self: Effect<A, E1, R>) => Effect<A, E | E1, RIn | Exclude<R, ROut>>
export declare const provideService:  <I, S>(tag: Context.Tag<I, S>, service: S) => <A, E, R>(self: Effect<A, E, R>) => Effect<A, E, Exclude<R, I>>
```

App-boundary execution (the only place an `Effect` is discharged):

```ts
export declare const runFork:        <A, E>(effect: Effect<A, E>, options?: Runtime.RunForkOptions) => Fiber.RuntimeFiber<A, E>
export declare const runPromise:     <A, E>(effect: Effect<A, E, never>, options?: { readonly signal?: AbortSignal }) => Promise<A>
export declare const runPromiseExit: <A, E>(effect: Effect<A, E, never>, options?: { readonly signal?: AbortSignal }) => Promise<Exit.Exit<A, E>>
export declare const runSync:        <A, E>(effect: Effect<A, E>) => A
```

### Effect.Service

```ts
// the canonical closed-service owner pattern; yields a TagClass + static Default Layer
export declare const Service: <Self = never>() => [Self] extends [never] ? MissingSelfGeneric : {
  <const Key extends string, const Make extends {
    readonly effect: Effect<Service.AllowedType<Key, Make>, any, any>
                   | ((...args: any) => Effect<Service.AllowedType<Key, Make>, any, any>)
    readonly dependencies?: ReadonlyArray<Layer.Layer.Any>
    readonly accessors?: boolean
  } /* | { scoped } | { sync } | { succeed } */>(key: Key, make: Make): Service.Class<Self, Key, Make>
}

// usage:
// class MyService extends Effect.Service<MyService>()("MyService", {
//   effect: Effect.gen(function*() { … }),
//   dependencies: [OtherService.Default],
// }) {}
//   -> MyService is a Context.Tag; MyService.Default is its Layer.
```

`Effect.Service` is the one app-service owner shape across the closed five-service budget — one `Self` brand, one `Key` string, one of `{ effect | scoped | sync | succeed }` plus optional `dependencies`/`accessors`. The generated class is simultaneously the `Tag` and carries a static `Default` Layer.

---

## [2]-[CONTEXT_AND_LAYER]

### Context / Tag

```ts
// effect/Context
export interface Context<in Services> extends Equal, Pipeable, Inspectable

export interface Tag<in out Id, in out Value> extends Pipeable, Inspectable
export interface TagClass<Self, Id extends string, Type> extends Tag<Self, Type>

export declare namespace Tag {
  type Service<T>     // Value
  type Identifier<T>  // Id
}

// constructors
export declare const make:       <I, S>(tag: Tag<I, S>, service: Types.NoInfer<S>) => Context<I>
export declare const add:        <I, S, I2, S2>(self: Context<I>, tag: Tag<I2, S2>, service: S2) => Context<I | I2>
export declare const get:        <Services, T extends ValidTagsById<Services>>(self: Context<Services>, tag: T) => Tag.Service<T>
export declare const GenericTag: <Identifier, Service = Identifier>(key: string) => Tag<Identifier, Service>
export declare const empty:      () => Context<never>
```

Class-style tag: `class Foo extends Context.Tag("Foo")<Foo, FooShape>() {}` produces the canonical identifier-keyed service handle accessed via `yield* Foo` inside `Effect.gen`.

### Layer

```ts
// effect/Layer
export interface Layer<in ROut, out E = never, out RIn = never>
  extends Layer.Variance<ROut, E, RIn>, Pipeable

export declare namespace Layer {
  type Context<T>  // RIn
  type Error<T>    // E
  type Success<T>  // ROut
  type Any
}

// composition (data-last arms)
export declare const provide:        <RIn2, E2, ROut2>(that: Layer<RIn2, E2, ROut2>) => <RIn, E, ROut>(self: Layer<ROut, E, RIn>) => Layer<ROut | ROut2, E | E2, Exclude<RIn, ROut2> | RIn2>
export declare const merge:          <RIn2, E2, ROut2>(that: Layer<RIn2, E2, ROut2>) => <RIn, E, ROut>(self: Layer<ROut, E, RIn>) => Layer<ROut | ROut2, E | E2, RIn | RIn2>
export declare const effect:         <I, S, E, R>(tag: Context.Tag<I, S>, effect: Effect<Types.NoInfer<S>, E, R>) => Layer<I, E, R>
export declare const scoped:         <I, S, E, R>(tag: Context.Tag<I, S>, effect: Effect<S, E, R>) => Layer<I, E, Exclude<R, Scope>>
export declare const scopedDiscard:  <E, R>(effect: Effect<unknown, E, R>) => Layer<never, E, Exclude<R, Scope>>
export declare const succeed:        <I, S>(tag: Context.Tag<I, S>, resource: Types.NoInfer<S>) => Layer<I>
export declare const empty:          Layer<never>
```

`Layer<ROut, E, RIn>` is the dependency-graph node: builds `ROut` services from `RIn` dependencies, possibly failing with `E`. The composition root assembles one DAG; layers are memoized so a shared dependency builds once.

---

## [3]-[RUNTIME]

### Runtime

```ts
// effect/Runtime
export interface Runtime<in R> extends Pipeable {
  readonly context: Context.Context<R>
  readonly runtimeFlags: RuntimeFlags.RuntimeFlags
  readonly fiberRefs: FiberRefs.FiberRefs
}

export declare const runFork:    <R>(runtime: Runtime<R>) => <A, E>(effect: Effect<A, E, R>, options?: RunForkOptions) => Fiber.RuntimeFiber<A, E>
export declare const runPromise: <R>(runtime: Runtime<R>) => <A, E>(effect: Effect<A, E, R>, options?: { readonly signal?: AbortSignal }) => Promise<A>
export declare const runSync:    <R>(runtime: Runtime<R>) => <A, E>(effect: Effect<A, E, R>) => A
```

### ManagedRuntime

```ts
// effect/ManagedRuntime
export interface ManagedRuntime<in R, out ER>
  extends Effect.Effect<Runtime.Runtime<R>, ER> {
  readonly memoMap: Layer.MemoMap
  readonly runtimeEffect: Effect.Effect<Runtime.Runtime<R>, ER>
  readonly runtime: () => Promise<Runtime.Runtime<R>>
  readonly runFork:    <A, E>(self: Effect<A, E, R>, options?: RunForkOptions) => Fiber.RuntimeFiber<A, E | ER>
  readonly runPromise: <A, E>(self: Effect<A, E, R>, options?: { readonly signal?: AbortSignal }) => Promise<A>
  readonly runSync:    <A, E>(self: Effect<A, E, R>) => A
  readonly dispose:    () => Promise<void>
  readonly disposeEffect: Effect<void, ER>
}

export declare const make: <R, E>(layer: Layer.Layer<R, E, never>, memoMap?: Layer.MemoMap) => ManagedRuntime<R, E>
```

`ManagedRuntime.make(appLayer)` is the SPA root handle the composition root constructs once from the assembled Layer graph; `dispose()` runs all scoped finalizers.

### Scope

```ts
// effect/Scope
export interface Scope extends Pipeable {
  readonly [ScopeTypeId]: ScopeTypeId
  readonly strategy: ExecutionStrategy.ExecutionStrategy
}
export interface CloseableScope extends Scope, Pipeable

export declare const Scope: Context.Tag<Scope, Scope>
export declare const addFinalizer: (self: Scope, finalizer: Effect<void>) => Effect<void>
export declare const close:        (self: CloseableScope, exit: Exit.Exit<unknown, unknown>) => Effect<void>
export declare const make:         (executionStrategy?: ExecutionStrategy.ExecutionStrategy) => Effect<CloseableScope>
```

---

## [4]-[CONCURRENCY_STATE]

### Fiber

```ts
// effect/Fiber
export interface Fiber<out A, out E = never>
  extends Effect.Effect<A, E>, Fiber.Variance<A, E> {
  readonly id: () => FiberId.FiberId
  readonly status: Effect<FiberStatus.FiberStatus>
  readonly await: Effect<Exit.Exit<A, E>>
  readonly interruptAsFork: (fiberId: FiberId.FiberId) => Effect<void>
}
export interface RuntimeFiber<out A, out E = never> extends Fiber<A, E>
```

### Ref / SubscriptionRef

```ts
// effect/Ref
export interface Ref<in out A>
  extends Ref.Variance<A>, Effect.Effect<A>, Readable.Readable<A> {
  modify<B>(f: (a: A) => readonly [B, A]): Effect<B>
}
export declare const make: <A>(value: A) => Effect<Ref<A>>

// effect/SubscriptionRef
export interface SubscriptionRef<in out A>
  extends SubscriptionRef.Variance<A>, Synchronized.SynchronizedRef<A>, Subscribable<A> {
  readonly changes: Stream.Stream<A>
}
export declare const make: <A>(value: A) => Effect<SubscriptionRef<A>>
```

`SubscriptionRef.changes` is the subscribable cell bridge each store fold exposes to the atom layer — the canonical state-to-view stream seam.

### Schedule

```ts
// effect/Schedule
export interface Schedule<out Out, in In = unknown, out R = never>
  extends Schedule.Variance<Out, In, R>, Pipeable

// retry-policy constructors
export declare const exponential: (base: DurationInput, factor?: number) => Schedule<Duration>
export declare const recurs:      (n: number) => Schedule<number>
export declare const union:       <Out2, In2, R2>(that: Schedule<Out2, In2, R2>) => <Out, In, R>(self: Schedule<Out, In, R>) => Schedule<Out | Out2, In & In2, R | R2>
export declare const addDelay:    <Out>(f: (out: Out) => DurationInput) => <In, R>(self: Schedule<Out, In, R>) => Schedule<Out, In, R>
export declare const jittered:    <Out, In, R>(self: Schedule<Out, In, R>) => Schedule<Out, In, R | Random>
export declare const whileOutput: <Out>(f: Predicate<NoInfer<Out>>) => <In, R>(self: Schedule<Out, In, R>) => Schedule<Out, In, R>
```

`Schedule` is the typed retry-policy value for the staleness-forward posture — stream interruption folds to a `Schedule`, never an improvised loop.

---

## [5]-[STREAM]

```ts
// effect/Stream
export interface Stream<out A, out E = never, out R = never>
  extends Stream.Variance<A, E, R>, Pipeable

// construction (no bare `async` in v3.21.3 — the push/scoped/effect arms are canonical)
export declare const fromEffect:  <A, E, R>(effect: Effect<A, E, R>) => Stream<A, E, R>
export declare const asyncPush:   <A, E = never, R = never>(register: (emit: Emit.EmitOpsPush<E, A>) => Effect<unknown, E, R | Scope.Scope>, options?: { readonly bufferSize?: number | "unbounded" }) => Stream<A, E, Exclude<R, Scope.Scope>>
export declare const asyncScoped: <A, E = never, R = never>(register: (emit: Emit.Emit<R, E, A, void>) => Effect<unknown, E, R | Scope.Scope>, bufferSize?: number | "unbounded") => Stream<A, E, Exclude<R, Scope.Scope>>
export declare const asyncEffect: <A, E = never, R = never>(register: (emit: Emit.Emit<R, E, A, void>) => Effect<unknown, E, R>, bufferSize?: number | "unbounded") => Stream<A, E, R>
export declare const never:       Stream<never>
export declare const empty:       Stream<never>
export declare const fail:        <E>(error: E) => Stream<never, E>

// transform (data-last)
export declare const tap:       <A, X, E2, R2>(f: (a: A) => Effect<X, E2, R2>) => <E, R>(self: Stream<A, E, R>) => Stream<A, E | E2, R | R2>
export declare const mapEffect:  <A, B, E2, R2>(f: (a: A) => Effect<B, E2, R2>, options?: { concurrency?: number | "unbounded" }) => <E, R>(self: Stream<A, E, R>) => Stream<B, E | E2, R | R2>
export declare const retry:      <E, R2>(schedule: Schedule.Schedule<any, E, R2>) => <A, R>(self: Stream<A, E, R>) => Stream<A, E, R | R2>

// fold / consume
export declare const runFold:    <S, A>(s: S, f: (s: S, a: A) => S) => <E, R>(self: Stream<A, E, R>) => Effect<S, E, R>
export declare const runForEach: <A, X, E2, R2>(f: (a: A) => Effect<X, E2, R2>) => <E, R>(self: Stream<A, E, R>) => Effect<void, E | E2, R | R2>
```

`Stream<A, E, R>` carries connect-es server-streams; the canonical store pattern is `Stream.runFold` into an immutable keyed map per store owner, with `Stream.retry(schedule)` for the staleness-forward reconnect.

---

## [6]-[SCHEMA]

```ts
// effect/Schema
export interface Schema<in out A, in out I = A, out R = never>
  extends Schema.Variance<A, I, R>, Pipeable {
  readonly Type: A       // decoded type
  readonly Encoded: I    // wire type
  readonly Context: R    // service requirements
  readonly ast: AST.AST
}

// decode / encode at the wire edge
export declare const decodeUnknown:        <A, I, R>(schema: Schema<A, I, R>, options?: ParseOptions) => (u: unknown, overrideOptions?: ParseOptions) => Effect<A, ParseResult.ParseError, R>
export declare const decodeUnknownEither:  <A, I>(schema: Schema<A, I, never>, options?: ParseOptions) => (u: unknown) => Either<A, ParseResult.ParseError>
export declare const decodeUnknownPromise: <A, I>(schema: Schema<A, I, never>, options?: ParseOptions) => (u: unknown) => Promise<A>
export declare const encodeUnknown:        <A, I, R>(schema: Schema<A, I, R>, options?: ParseOptions) => (a: unknown, overrideOptions?: ParseOptions) => Effect<I, ParseResult.ParseError, R>

// domain-shape constructors (the one domain-shape / one error-rail patterns)
export declare const Class:       <Self = never>(identifier: string) => <Fields extends Struct.Fields>(fieldsOr: Fields | HasFields<Fields>, annotations?: …) => Class<Self, Fields, …>
export declare const TaggedClass: <Self = never>(identifier?: string) => <Tag extends string, Fields extends Struct.Fields>(tag: Tag, fieldsOr: Fields | HasFields<Fields>, …) => TaggedClass<Self, Tag, …>
export declare const TaggedError: <Self = never>(identifier?: string) => <Tag extends string, Fields extends Struct.Fields>(tag: Tag, fieldsOr: Fields | HasFields<Fields>, …) => TaggedErrorClass<Self, Tag, Fields>

// vocabulary primitives
export declare function Literal<Literals extends NonEmptyReadonlyArray<AST.LiteralValue>>(...literals: Literals): Literal<Literals>
export declare const Struct:   <Fields extends Struct.Fields>(fields: Fields) => Struct<Fields>
export declare const Union:    <Members extends readonly Schema.All[]>(...members: Members) => Union<Members>
export declare const Array:    <Value extends Schema.Any>(value: Value) => Array$<Value>  // exported `Array$ as Array`
export declare const Record:   <K extends Schema.All, V extends Schema.All>(options: { readonly key: K; readonly value: V }) => Record$<K, V>
export declare const optional: <S extends Schema.All>(self: S) => optional<S>  // optionalWith(self, options) is the option-carrying sibling
```

`Schema.Class` is the one domain-shape pattern (projections derive from it, never parallel structs); `Schema.TaggedError` is the one error-rail pattern (one family per rail); decode entry is always `Schema.decodeUnknown*` at the wire edge.

---

## [7]-[OBSERVABILITY]

### Metric

```ts
// effect/Metric
export interface Metric<in out Type, in In, out Out>
  extends Metric.Variance<Type, In, Out>, Pipeable

export declare namespace Metric {
  interface Counter<In extends number | bigint>   extends Metric<MetricKeyType.Counter<In>, In, MetricState.Counter<In>>
  interface Gauge<In extends number | bigint>      extends Metric<MetricKeyType.Gauge<In>, In, MetricState.Gauge<In>>
  interface Histogram<In extends number | bigint>  extends Metric<MetricKeyType.Histogram<In>, In, MetricState.Histogram<In>>
  interface Summary<In extends number | bigint>    extends Metric<MetricKeyType.Summary<In>, In, MetricState.Summary<In>>
  interface Frequency<In extends string>           extends Metric<MetricKeyType.Frequency<In>, In, MetricState.Frequency<In>>
}

export declare const counter:   <In extends number | bigint = number>(name: string, options?: …) => Metric.Counter<In>
export declare const gauge:     <In extends number | bigint = number>(name: string, options?: …) => Metric.Gauge<In>
export declare const histogram: (name: string, boundaries: MetricBoundaries.MetricBoundaries, options?: …) => Metric.Histogram<number>
export declare const summary:   (options: …) => Metric.Summary<number>
export declare const frequency: (name: string, options?: …) => Metric.Frequency<string>
// a Metric is callable as an Effect wrapper that records the input/output pair
```

`SelfTelemetry` consumes the five metric primitives covering the host span and metric emission surface.

### Logger / Tracer

```ts
// effect/Logger
export interface Logger<in Message, out Output>
  extends Logger.Variance<Message, Output>, Pipeable {
  readonly log: (options: Logger.Options<Message>) => Output
}

// effect/Tracer
export interface Tracer {
  readonly span: (name: string, parent: Option<AnySpan>, context: Context<never>, links: ReadonlyArray<Link>, startTime: bigint, kind: SpanKind) => Span
  readonly context: <X>(f: () => X, fiber: Fiber.RuntimeFiber<any, any>) => X
}
export declare const Tracer: Context.Tag<Tracer, Tracer>
```

---

## [8]-[VALUE_ALGEBRA]

### Data (structural equality + tagged value objects)

```ts
// effect/Data
export declare const struct:       <A extends Record<string, any>>(a: A) => { readonly [P in keyof A]: A[P] }
export declare const tuple:        <As extends ReadonlyArray<any>>(...as: As) => Readonly<As>
export declare const array:        <As extends ReadonlyArray<any>>(as: As) => Readonly<As>
export declare const tagged:       <A extends { readonly _tag: string }>(tag: A["_tag"]) => Case.Constructor<A, "_tag">
export declare const Class:        new <A extends Record<string, any> = {}>(args: Types.VoidIfEmpty<{ readonly [P in keyof A]: A[P] }>) => Readonly<A>
export declare const TaggedClass:  <Tag extends string>(tag: Tag) => new <A extends Record<string, any> = {}>(args: …) => Readonly<A> & { readonly _tag: Tag }
export declare const Error:        new <A extends Record<string, any> = {}>(args: …) => Cause.YieldableError & Readonly<A>
export declare const TaggedError:  <Tag extends string>(tag: Tag) => new <A extends Record<string, any> = {}>(args: …) => Cause.YieldableError & { readonly _tag: Tag } & Readonly<A>
```

`Data.TaggedClass`/`Data.TaggedError` give typed-equal value objects and yieldable tagged errors used inside store folds and command-gateway payloads.

### Option / Either / Exit / Cause

```ts
// effect/Option
export type Option<A> = None<A> | Some<A>
export interface None<out A> extends Pipeable, Inspectable { readonly _tag: "None" }
export interface Some<out A> extends Pipeable, Inspectable { readonly _tag: "Some"; readonly value: A }

// effect/Either
export type Either<A, E = never> = Left<E, A> | Right<E, A>
export interface Left<out E, out A>  extends Pipeable, Inspectable { readonly _tag: "Left";  readonly left: E }
export interface Right<out E, out A> extends Pipeable, Inspectable { readonly _tag: "Right"; readonly right: A }

// effect/Exit
export type Exit<A, E = never> = Success<A, E> | Failure<A, E>
export interface Success<out A, out E> extends Effect.Effect<A, E>, Pipeable, Inspectable { readonly _tag: "Success"; readonly value: A }
export interface Failure<out A, out E> extends Effect.Effect<A, E>, Pipeable, Inspectable { readonly _tag: "Failure"; readonly cause: Cause.Cause<E> }
export declare const isSuccess: <A, E>(self: Exit<A, E>) => self is Success<A, E>
export declare const isFailure: <A, E>(self: Exit<A, E>) => self is Failure<A, E>

// effect/Cause — the full failure tree behind E: Empty | Fail | Die | Interrupt | Sequential | Parallel
```

`Exit` is the discharge result of `runPromiseExit`/`Fiber.await` — `Success` carries the value, `Failure` carries the full `Cause` tree (typed `Fail`, defect `Die`, `Interrupt`).

### Config / Duration

```ts
// effect/Config
export interface Config<out A>
  extends Config.Variance<A>, Effect.Effect<A, ConfigError.ConfigError>
export declare const string:      (name?: string) => Config<string>
export declare const number:      (name?: string) => Config<number>
export declare const boolean:     (name?: string) => Config<boolean>
export declare const redacted:    { (name?: string): Config<Redacted.Redacted>; <A>(config: Config<A>): Config<Redacted.Redacted<A>> }
export declare const nested:      (name: string) => <A>(self: Config<A>) => Config<A>
export declare const withDefault: <const A2>(def: A2) => <A>(self: Config<A>) => Config<A2 | A>

// effect/Duration
export interface Duration extends Equal.Equal, Pipeable, Inspectable
export type DurationInput = Duration | number /* millis */ | bigint /* nanos */ | `${number} ${Unit}` | readonly [seconds: number, nanos: number]
```

`Config` is the domain-value pattern for `SecretResolver` — one domain value at the composition root, never scattered flag reads; `Config.redacted` keeps secrets out of logs.
