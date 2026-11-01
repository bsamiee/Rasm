---
name: typescript-coding
description: "Use when writing or reviewing TypeScript, covering functional design with Effect, Schema, and algebraic data types"
---

# [TYPESCRIPT_CODING]

Covers TypeScript design decisions under workspace standards (TOTALITY, PURITY, BOUNDARY), from signature shape to the runtime that runs a program. Effect is part of the language: `Effect`, `Schema`, `Option`, `Result`, `Match`, `Layer`, `Stream`, and the data modules are the vocabulary every example uses, and a native construct stands where no Effect module states the value.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Worked flows for functions as values, from `pipe` and keyed tables to `Effect.fn` and composition roots
- [02]-[TYPES](references/types.md): Type-level derivation, from inferred declarations to keyed tables, brands, and generic constraints
- [03]-[DATA](references/data.md): Snapshot and transition model, with structural equality, persistent collections, and optics
- [04]-[RESULTS](references/results.md): Worked flows for `Option`, `Result`, the error channel, accumulation, and union folds
- [05]-[SCHEMA](references/schema.md): Codecs at a boundary, from classes and variants to grammars decoded inside the schema
- [06]-[SERVICES](references/services.md): Worked flows for services, layers, configuration, scoped resources, and the runtime
- [07]-[CONCURRENCY](references/concurrency.md): Fibers, streams, queues, observable state, and schedules over shared resources

## [01]-[DECLARATIONS]

One declaration states each concept, and every other form of it derives. A schema class states the type, constructor, guard, equality, codec, and every constraint on its fields, a keyed table states the cases and their data:

```ts
const discounts = { basic: 0n, plus: 5n, pro: 12n } as const;

class Line extends Schema.Class<Line>('Line')({
    tier: Schema.Literals(Struct.keys(discounts)),
    price: Schema.BigDecimal.check(Schema.isGreaterThanBigDecimal(BigDecimal.make(0n, 0))),
    quantity: Schema.Int.check(Schema.isGreaterThan(0)).pipe(Schema.withConstructorDefault(Effect.succeed(1))),
}) {
    get total() {
        return BigDecimal.multiply(this.price, BigDecimal.make(BigInt(this.quantity) * (100n - discounts[this.tier]), 2));
    }

    static readonly byTotal = Order.mapInput(BigDecimal.Order, (line: Line) => line.total);
}
```

`discounts` states each tier and its percent off once, decoding and `Line.make` enforce every field check, `quantity` defaults at construction, `total` derives from the fields, and `byTotal` orders lines by it. A new tier is one row:
- Cases differing in data are field sets of one tagged union, cases differing in values no rule decides are rows of an `as const` table
- Values a rule decides (a rank, a cumulative set, a step) derive from the case's index in an ordered `Schema.Literals` and a parameter
- Tables crossing varying axes are one `Array.cartesianWith` over the axes, each row computed from its pair
- Members differing in a number, flag, literal, or applied function are one member taking it as a parameter
- Fields with a dominant value take `Schema.withConstructorDefault`, parameters with one take a default, callers state what varies
- Facts a state decides (its meaning, label, message) are getters over its data, each union case rendering its own
- Declarations read once sit inline at their use, a value read twice is a `const`

Types derive at each use from the value stating the fact (`typeof X.Type`, `keyof typeof table`, indexed access), and an alias or interface stands where no schema, table, or function states the shape. Mapped, conditional, and template literal types derive from a declared table or schema, a generic parameter exists where a caller supplies the type.

## [02]-[FUNCTIONS]

Function signatures are contracts, parameter types describe every value the function accepts and the return type every outcome the caller handles:

| [INDEX] | [PURPOSE]                    | [SIGNATURE]                        |
| :-----: | :--------------------------- | :--------------------------------- |
|  [01]   | Transform a value            | `(a: A) => B`                      |
|  [02]   | Produce a value on demand    | `LazyArg<A>`                       |
|  [03]   | Answer with absence          | `(a: A) => Option<B>`              |
|  [04]   | Fail without an effect       | `(a: A) => Result<B, E>`           |
|  [05]   | Describe an effect           | `(a: A) => Effect<B, E, R>`        |
|  [06]   | Describe many values in time | `(a: A) => Stream<B, E, R>`        |
|  [07]   | Test or narrow a value       | `Predicate<A>`, `Refinement<A, B>` |

- Parameters and fields are `readonly`, arrays are `ReadonlyArray<A>`, or `NonEmptyReadonlyArray<A>` where the rule needs an element
- Return types infer from the body

### [02.1]-[HONEST_SIGNATURES]

Functions honor their signature when each accepted input produces a declared output. Narrow the input to a decoded type or widen the output to `Option`, `Result`, or `Effect`:
- Parameters take the decoded type (`Quantity`, an `OrderId` brand), so every accepted input has an answer
- `Schema.decodeUnknownOption(Schema.NumberFromString)` states every outcome of a parse
- Absence an operation answers is `Option<A>` (`Array.get`, `Record.get`), `null` and `undefined` appear at a host API alone
- Absence a caller chooses is an optional key, which holds its type or is absent under `exactOptionalPropertyTypes`

### [02.2]-[PURITY]

Functions are pure when their result depends on their arguments alone and evaluation causes no side effect. Clock, randomness, environment, logging, and I/O are effects, and a variable dependency enters as an argument or as a service the effect reads:

```ts
const stamped = (label: string) => Effect.map(DateTime.now, (now) => `${DateTime.formatIso(now)} ${label}`);
```

`Clock`, `Random`, and `Console` are references with a default implementation, and a layer replaces each one.

### [02.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions own the stable control flow and the caller supplies the varying rule. Parameters order for left-to-right application: dependencies known at the composition root, then policies that select behavior, then the runtime value. `flow` composes functions into one function, `pipe` applies a value through them. The call site selects the form:
- Partial application when a dependency or policy is fixed before the value arrives
- Keyed table of functions when behaviors share a signature and vary by a key
- `Match` with ordered arms and `Match.orElse` for a first-match decision over values

## [03]-[EXPRESSIONS]

Expressions evaluate to values, and every binding is `const`. A value builds in one expression that states each field at creation:

```ts
const summary = (lines: Array.NonEmptyReadonlyArray<Line>) => ({
    total: BigDecimal.sumAll(Array.map(lines, (line) => line.total)),
    largest: Array.max(lines, Line.byTotal),
});
```

Non-empty `lines` give `Array.max` an element, so `largest` is a `Line` with no absent case:
- `Array`, `Record`, `Struct`, and `Iterable` functions state every traversal, named reductions (`BigDecimal.sumAll`, `Array.max`, `Array.groupBy`) state their fold
- `Array.reduce` with a seed folds a reduction no member names
- `Array.makeBy`, `Array.range`, and `Array.unfold` generate sequences from a length, bounds, or a seed
- Relations between neighbors map the adjacent pairs `Array.window(values, 2)` answers, a gap filling from `Array.range` over its pair
- Inner callbacks hold no further callback, a deeper step becomes a `yield*` step or a function named for its result

### [03.1]-[PATTERN_MATCHING]

Pattern matching keeps recognition, extraction, guards, and results together. A union owns its fold: a `Schema.TaggedUnion` through `match`, a `Data.taggedEnum` through `$match`, and any other value through `Match`:

```ts
const route = Match.type<Request>().pipe(
    Match.when({ method: 'GET', cached: true }, fromCache),
    Match.when({ method: 'GET' }, fromOrigin),
    Match.orElse(reject),
);
```

`fromCache`, `fromOrigin`, and `reject` are handlers taking the narrowed `Request`:
- `Match.exhaustive` closes a decision over a closed union, `Match.orElse` closes an open one with the fallback outcome
- `Match.when` takes a partial object pattern or a refinement, `Match.not` an exclusion, `Match.whenOr` alternatives
- Arms sharing one result collapse into `matchOrElse` or `Match.orElse` with the exceptions as arms
- Host values decode at their read through a schema (`Schema.decodeUnknownEffect`, `Schema.is`, `Schema.instanceOf`) the fold then reads

### [03.2]-[RECURSION]

Recursive functions need a base case, a recursive case closer to it, and a value on every path. TypeScript runs tail calls on the stack, so the depth bound selects the form:

| [INDEX] | [APPROACH]            | [USE_WHEN]                                       |
| :-----: | :-------------------- | :----------------------------------------------- |
|  [01]   | Direct recursion      | Depth small and bounded (a parsed tree)          |
|  [02]   | `Array.unfold`        | Each state yields one element and the next state |
|  [03]   | `Effect.suspend` loop | Effectful transition at unbounded depth          |
|  [04]   | `Stream.paginate`     | Each step fetches a page and a cursor            |
|  [05]   | `Graph` traversal     | Shared or cyclic successors                      |

## [04]-[IMMUTABILITY]

Values are fixed once created, a change derives a new value, and a state transition is a function from the old state and its input to the replacement:
- Data crossing a boundary is `Schema.Class` and `Schema.TaggedUnion`, data inside the process is `Data.Class` and `Data.taggedEnum`
- Each kind has structural `Equal` and `Hash`, so values with equal fields are one value
- Plain objects change through a spread or `Struct.evolve`, class instances and nested paths through an optic
- Native `Map`, `Set`, and arrays mutate inside a scope that returns an immutable value

## [05]-[RESULTS]

Every function returns an explicit outcome type, and one type serves one concern:

| [INDEX] | [TYPE]            | [CONCERN]                                             |
| :-----: | :---------------- | :---------------------------------------------------- |
|  [01]   | `Option<A>`       | Absence without an error                              |
|  [02]   | `Result<A, E>`    | Synchronous failure in a pure computation             |
|  [03]   | `Effect<A, E, R>` | Effect with a typed failure and the services it reads |
|  [04]   | `Stream<A, E, R>` | Many values over time with one failure channel        |
|  [05]   | `Exit<A, E>`      | Settled outcome a host reads                          |
|  [06]   | `Cause<E>`        | Every failure, defect, and interruption of one fiber  |

Input boundaries select the outcome type, functions preserve it, and a conversion happens at one point through the function named for the target (`Effect.fromOption`, `Effect.fromResult`, `Option.fromNullishOr`, `Result.fromOption`). An empty array is a value, `Option<ReadonlyArray<A>>` stands where the consumer treats no collection and an empty one differently. Producers report a failure on the error channel and answer `Option.none()` for absence alone.

### [05.1]-[COMPOSITION]

Each step is a function, and its signature and its dependence on earlier steps select the operator:

| [INDEX] | [SIGNATURE]            | [OPERATOR]                   | [BEHAVIOR]                                          |
| :-----: | :--------------------- | :--------------------------- | :-------------------------------------------------- |
|  [01]   | `A => B`               | `map`                        | Transforms a present value                          |
|  [02]   | `A => boolean`         | `filter`, `filterOrFail`     | Keeps a passing value, `filterOrFail` names failure |
|  [03]   | `A => F<B>`            | `flatMap`, `yield*` in `gen` | Continues with a dependent step                     |
|  [04]   | `{ a: F<A>, b: F<B> }` | `Effect.all`                 | Combines independent effects into a struct          |
|  [05]   | `A => F<void>`         | `tap`                        | Runs a step and keeps the value                     |

- `Effect.gen` holds a sequence where a later step reads more than one earlier value, `pipe` holds a linear chain, one step is the effect itself
- Values stay in one type through a pipeline, an `Option` inside an effect included, and the consumer matches it once at the end

```ts
const place = Effect.fn('place')(function* (command: Command) {
    const [accounts, pricing] = yield* Effect.all([Accounts, Pricing]);
    const quote = yield* pricing.quote(command.lines);
    return yield* accounts.reserve({ account: command.account, amount: quote.total });
});
```

### [05.2]-[ERRORS]

Expected failures are data on the error channel, and defects are programmer errors and broken invariants that end the fiber. Each package declares one error class: a `reason` literal naming what failed, the `subject` it failed on, and the host `cause` where one exists, with the message derived from them:

```ts
class OrderError extends Schema.TaggedError<OrderError>()('OrderError', {
    reason: Schema.Literals(['missing', 'rejected', 'stale']),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    override get message() {
        return `${this.reason}: ${this.subject}`;
    }
}
```

Facts some reasons lack are `Schema.optionalKey` fields, as `cause` is.

### [05.3]-[UNIONS]

Each valid state a consumer inspects is one case. Counting the values a type allows (a product multiplies its fields' counts, an optional field adds one) exposes states no case needs:

```ts
const Shape = Schema.TaggedUnion({
    Circle: { radius: Schema.Finite },
    Rectangle: { width: Schema.Finite, height: Schema.Finite },
});

const area = Shape.match({
    Circle: ({ radius }) => Math.PI * radius ** 2,
    Rectangle: ({ width, height }) => width * height,
});
```

- Flags with fields meaningful for one flag value become one case per valid state
- Cases hold their own data side by side as alternatives
- States with no data and no behavior are a literal or a default value of a neighboring case
- Folds stand where behavior depends on the case, the union passes onward elsewhere
- Unions fit a growing set of operations, a service fits a growing set of implementations
- Absence and failure stay separate where the consumer responds differently

## [06]-[EFFECTS]

`Effect<A, E, R>` describes a program that succeeds with `A`, fails with `E`, and reads services `R`, and performs nothing until a runtime runs it. A Promise is an effect already running, it enters at the boundary through `Effect.tryPromise` and leaves through the host's runtime call:
- `Effect.fn('<name>')` declares every effectful function, with a span and stack frame named for it
- `Effect.sync` defers a side effect that cannot fail, `Effect.try` one that throws, its `catch` building the package error around the cause
- `Effect.tryPromise` hands the host call an `AbortSignal`, so interruption cancels the request
- `Effect.callback` adapts a callback API, its registration receives an `AbortSignal` and returns its cleanup effect

### [06.1]-[DEPENDENCIES]

Consumers receive the narrowest dependency they read: a value for a stable snapshot, an effect for an operation run on demand, and a service for a capability a program reads through its environment. Service members are the package members that own each operation:

```ts
class Accounts extends Context.Service<Accounts>()('@app/Accounts', {
    make: Effect.map(SqlClient.SqlClient, (sql) => ({
        find: SqlSchema.findOneOption({ Request: AccountId, Result: Account, execute: (id) => sql`SELECT * FROM accounts WHERE id = ${id}` }),
    })),
}) {
    static readonly layer = Layer.effect(this, this.make);
}
```

`SqlSchema.findOneOption` encodes the request, decodes the first row through `Account`, and answers a missing row with `Option.none()`, so the member is the package function itself.

### [06.2]-[POLICIES]

Failure policies are distinct operations:
- Fallback runs a lower-priority effect after the preferred one fails, `Effect.catch`, or `Effect.catchTag` for one classified error
- Retry reruns a transient failure on a `Schedule`
- Timeout bounds a wait, `Effect.timeout` fails with `Cause.TimeoutError` and `Effect.timeoutOption` answers `Option.none()`
- Recovery maps a final failure to a value at the host

Libraries return their outcome type with their own error, and the application composes the retry schedule, fallback order, and cache around it. The dependency structure and the concurrency bound select the traversal:

| [INDEX] | [SCENARIO]                   | [TRAVERSAL]                                      | [BEHAVIOR]                                 |
| :-----: | :--------------------------- | :----------------------------------------------- | :----------------------------------------- |
|  [01]   | Independent checks           | `Effect.validate`                                | Runs every check, keeps every failure      |
|  [02]   | Independent effects          | `Effect.forEach` with `concurrency: 'unbounded'` | Overlaps, interrupts the rest on a failure |
|  [03]   | Dependent or ordered effects | `Effect.forEach` without `concurrency`           | Serial, stops at the first failure         |
|  [04]   | Bounded concurrency          | `Effect.forEach` with `concurrency: <n>`         | At most `<n>` effects at once              |
|  [05]   | Every effect, failures kept  | `Effect.partition`                               | Runs all, returns successes and failures   |
|  [06]   | Every outcome kept in order  | `Effect.all` with `mode: 'result'`               | Runs all, returns one `Result` per effect  |

## [07]-[VALUES]

Quantities with a unit are values of their module, a bare number holds a count or a dimensionless ratio:
- `Duration` holds every elapsed time and timeout, a millisecond number appears at an API taking one
- `DateTime` holds instants and zoned times, `Date` appears at an API taking one
- `BigDecimal` holds money and exact decimal arithmetic, `ByteSize` holds sizes
- `Intl` formats numbers, dates, lists, and plurals for display

## [08]-[MODULES]

Modules hold one concept, and a class holds the concept's data with its operations as statics and getters:
- Packages hold the capability they own, app values (names, paths, timeouts, prose) enter as arguments from the composition root
- Package types pass whole: a wrapper takes the package's options type with the one field it adds, consumers read a returned host handle
- Packages export the values consumers compose, every type a consumer names derives from an exported value
- Modules export named declarations, a default export stands where a host loads one
- Package `exports` entries are the import paths consumers use, an `index.ts` holds re-exports alone
- Modules load without side effects, a registry, cache, or connection builds in the layer that owns it
