---
name: typescript-coding
description: "Use when writing or reviewing TypeScript, covering functional design with Effect, Schema, and algebraic data types"
---

# [TYPESCRIPT_CODING]

Covers functional TypeScript on Effect modules, with a native construct where no Effect module states a value.

[PRINCIPLES]:
- Facts are stated once where they belong, and everything else derives from that source
- Packages and hosts are used directly through what they provide, with no layer that restates or wraps them
- Surface stays minimal inside and outside each module, and every type, export, and helper states something nothing else does
- Values that vary are parameters, and kinds, keys, and states are typed values rather than strings
- Names come from domain vocabulary and members of packages in use, a file takes its concept's name, and no name is coined

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Functions as values and their composition
- [02]-[TYPES](references/types.md): Types derived from the values that own them
- [03]-[DATA](references/data.md): Immutable data and its transitions
- [04]-[RESULTS](references/results.md): Typed failure, absence, and recovery flows
- [05]-[SCHEMA](references/schema.md): Schemas as codecs at a boundary
- [06]-[SERVICES](references/services.md): Services, layers, and runtimes
- [07]-[CONCURRENCY](references/concurrency.md): Concurrent work and the state it shares

## [01]-[DECLARATIONS]

One declaration states each concept, and every other form derives from it:

```ts
class Line extends Schema.Class<Line, { readonly brand: unique symbol }>('Line')({
    sku: Sku,
    price: Schema.BigDecimal.check(Schema.isGreaterThanBigDecimal(BigDecimal.fromBigInt(0n))),
    quantity: Schema.Int.check(Schema.isGreaterThan(0)).pipe(Schema.withConstructorDefault(Effect.succeed(1))),
}) {
    get total() {
        return BigDecimal.multiply(this.price, BigDecimal.fromNumberUnsafe(this.quantity));
    }

    static readonly byTotal = Order.mapInput(BigDecimal.Order, (line: Line) => line.total);
}
```

- Cases differing in data are field sets of one tagged union, cases differing in values no rule decides are rows of an `as const` table
- Tables crossing varying axes are one `Array.cartesianWith` over the axes, each row computed from its pair
- Members differing in a number, flag, literal, or applied function are one member taking it as a parameter
- Parameters with a dominant value take a default, callers state what varies
- Declarations read once sit inline at their use, a value read twice is a `const` named for its fact, and a number Biome's `noMagicNumbers` refuses comes from the package member stating it, else a `const` local to its one reader
- Schemas declare at module scope, and each parser caches by its schema's AST

## [02]-[FUNCTIONS]

Signatures state every value a function accepts and every outcome its caller handles. Parameters and fields are `readonly`, arrays `readonly A[]`, or `Array.NonEmptyReadonlyArray<A>` where the rule needs an element.

### [02.1]-[TOTAL_FUNCTIONS]

Total functions return a declared output for each accepted input, narrowing the input to a decoded type or widening the output to `Option`, `Result`, or `Effect`:
- Parameters take the decoded type (`Quantity`, an `OrderId` brand)
- `Schema.decodeUnknownOption(Schema.NumberFromString)` states every outcome of a parse

### [02.2]-[PURITY]

Pure functions depend on their arguments alone and cause no side effect. Clock, randomness, environment, logging, and I/O are effects.

## [03]-[EXPRESSIONS]

Bindings are `const`, and values build in one expression stating each field at creation:

```ts
const summary = (lines: Array.NonEmptyReadonlyArray<Line>) => ({
    total: BigDecimal.sumAll(Array.map(lines, (line) => line.total)),
    largest: Array.max(lines, Line.byTotal),
});
```

Non-empty `lines` give `Array.max` an element, and `largest` is a `Line` with no absent case:
- Native array members state a direct `filter`, `every`, `join`, or `reduce`, and `Array`, `Record`, `Struct`, and `Iterable` functions every other traversal
- `Record.collect(record, (key, value) => ...)` maps entries to an array and `Record.filterMap` keeps the entries a lookup finds, key-value pairs from `Object.entries` restating both
- Named reductions (`BigDecimal.sumAll`, `Array.max`, `Array.groupBy`) state their fold
- `reduce` with a seed folds a reduction no member names
- `Array.makeBy`, `Array.range`, and `Array.unfold` generate sequences from a length, bounds, or a seed
- Relations between neighbors map the adjacent pairs `Array.window(values, 2)` returns, a gap filling from `Array.range` over its pair
- Callbacks nest one level, a deeper step becomes a `yield*` step or a function named for its result

### [03.1]-[PATTERN_MATCHING]

Pattern matching keeps recognition, extraction, guards, and results together. Unions own their fold: `Schema.TaggedUnion` through `match`, `Data.taggedEnum` through `$match` and `Option.filter(<option>, <enum>.$is(<tag>))` for a field of one case, and any other value through `Match`:

```ts
const route = Match.type<Request>().pipe(Match.when({ method: 'GET', cached: true }, fromCache), Match.when({ method: 'GET' }, fromOrigin), Match.orElse(reject));
```

`fromCache`, `fromOrigin`, and `reject` are handlers taking the narrowed `Request`:
- `Match.exhaustive` closes a decision over a closed union, `Match.orElse` closes an open one with a fallback outcome
- `Match.when` takes a partial object pattern or a refinement, `Match.not` an exclusion, `Match.whenOr` alternatives
- Arms sharing one result collapse into `matchOrElse` or `Match.orElse` with the exceptions as arms
- `Match.valueTags(value, handlers)` folds a `_tag` union with one handler per tag
- Host enums match with an arm per member once decoded
- Host unions with a discriminant match through `Match.discriminatorsExhaustive(key)`, a `'k' in x` or `type !==` branch restating it
- Refinements come from `Predicate` (`isRecord`, `hasProperty`, `isTagged`) composed through `Predicate.and`, `or`, and `not`

### [03.2]-[RECURSION]

TypeScript runs tail calls on the stack, and a recursion's depth bound selects its form:

| [INDEX] | [APPROACH]            | [USE_WHEN]                                   |
| :-----: | :-------------------- | :------------------------------------------- |
|  [01]   | Direct recursion      | Depth small and bounded (parsed tree)        |
|  [02]   | `Array.unfold`        | Each state yields one element and next state |
|  [03]   | `Effect.suspend` loop | Effectful transition at unbounded depth      |
|  [04]   | `Stream.paginate`     | Each step fetches page and cursor            |
|  [05]   | `Graph` traversal     | Shared or cyclic successors                  |

## [04]-[IMMUTABILITY]

Data crossing a boundary is `Schema.Class` and `Schema.TaggedUnion`, data inside the process `Data.Class` and `Data.taggedEnum`.

## [05]-[RESULTS]

Every function returns an explicit outcome type, and one type serves one concern:

| [INDEX] | [TYPE]            | [CONCERN]                                            |
| :-----: | :---------------- | :--------------------------------------------------- |
|  [01]   | `Option<A>`       | Absence without error                                |
|  [02]   | `Result<A, E>`    | Synchronous failure in pure computation              |
|  [03]   | `Effect<A, E, R>` | Effect with typed failure and services it reads      |
|  [04]   | `Stream<A, E, R>` | Values over time with one failure channel            |
|  [05]   | `Exit<A, E>`      | Completed outcome a host reads                       |
|  [06]   | `Cause<E>`        | Every failure, defect, and interruption of one fiber |

Input boundaries select the outcome type, functions preserve it, and one conversion point uses the function named for its target (`Effect.fromOption`, `Effect.fromResult`, `Option.fromNullishOr`, `Result.fromOption`). `null` and `undefined` appear at a host API alone. Empty arrays are values, `Option<readonly A[]>` stands where a consumer treats no collection and an empty one differently.

Each package declares one error class for the expected failures its producers report, holding a `reason` case for each response a caller gives with the data that response reads, the `subject` that failed, and a foreign `cause` where one exists, with a message derived from them:

```ts
class StoreError extends Schema.TaggedError<StoreError, { readonly brand: unique symbol }>()('StoreError', {
    reason: Schema.TaggedUnion({ Conflict: { version: Schema.Int }, Denied: {}, Unavailable: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    override get message() {
        return `${this.reason._tag}: ${this.subject}`;
    }
}
```

- Instances are effects failing with themselves, `return yield*` raises one in a generator and a handler returning one fails with it
- Errors a package member types (`SchemaError`, `HttpClientError`, `Cause.TimeoutError`) stay in `E` unchanged, and a codec run over a value the program built or encoded dies through `Effect.orDie`
- Foreign failures become a `cause` where the package adds a subject a caller acts on

### [05.1]-[COMPOSITION]

Dependent steps bind and independent ones combine through `Effect.all`:
- `Effect.gen` holds a sequence where a later step reads more than one earlier value, `pipe` holds a linear chain, one step is the effect itself
- Values stay in one type through a pipeline, an `Option` inside an effect included, and a consumer matches it once at the end

```ts
const place = Effect.fn('place')(function* (command: Command) {
    const [accounts, pricing] = yield* Effect.all([Accounts, Pricing]);
    const quote = yield* pricing.quote(command.lines);
    return yield* accounts.reserve({ account: command.account, amount: quote.total });
});
```

Dependency structure and concurrency bound select a traversal over many effects:

| [INDEX] | [SCENARIO]                   | [TRAVERSAL]                                      | [BEHAVIOR]                                |
| :-----: | :--------------------------- | :----------------------------------------------- | :---------------------------------------- |
|  [01]   | Independent checks           | `Effect.validate`                                | Runs every check, keeps every failure     |
|  [02]   | Independent effects          | `Effect.forEach` with `concurrency: 'unbounded'` | Overlaps, interrupts rest on failure      |
|  [03]   | Dependent or ordered effects | `Effect.forEach` without `concurrency`           | Serial, stops at first failure            |
|  [04]   | Bounded concurrency          | `Effect.forEach` with `concurrency: <n>`         | At most `<n>` effects at once             |
|  [05]   | Every effect, failures kept  | `Effect.partition`                               | Runs all, returns successes and failures  |
|  [06]   | Every outcome kept in order  | `Effect.all` with `mode: 'result'`               | Runs all, returns one `Result` per effect |

### [05.2]-[UNIONS]

Each valid state a consumer inspects is one case. Counting the values a type allows (a product multiplies its fields' counts, an optional field adds one) exposes states no case needs:
- Flags with fields meaningful for one flag value become one case per valid state
- States with no data and no behavior are a literal or a default value of a neighboring case
- Folds stand where behavior depends on the case, the union passes onward elsewhere
- Unions fit a growing set of operations, a service fits a growing set of implementations
- Absence and failure stay separate where a consumer responds differently

```ts
const Issuance = Schema.TaggedUnion({
    Pending: { challenge: Schema.Literals(['http-01', 'dns-01']), token: Schema.String },
    Issued: { serial: Schema.String, notAfter: Schema.DateTimeUtc },
    Revoked: { reason: Schema.Literals(['keyCompromise', 'superseded', 'cessationOfOperation']) },
});

const status = Issuance.match({
    Pending: ({ challenge, token }) => `awaiting ${challenge} ${token}`,
    Issued: ({ serial, notAfter }) => `${serial} until ${DateTime.formatIso(notAfter)}`,
    Revoked: ({ reason }) => `revoked: ${reason}`,
});
```

## [06]-[EFFECTS]

`Effect<A, E, R>` describes a program that succeeds with `A`, fails with `E`, reads services `R`, and performs nothing until a runtime runs it. Promises are eager, enter through `Effect.tryPromise`, and leave through a host runtime call:
- `Effect.sync` defers a side effect that cannot fail, `Effect.try` one that throws, its `catch` building the package error or a decode's `SchemaIssue` around the throw
- `Effect.callback` adapts a callback API, with registration receiving an `AbortSignal` and returning a cleanup effect
- Listeners completing a `Deferred` the registering function awaits restate `Effect.callback`, except where a host callback must throw to roll back
- `Effect.tryPromise` passes an `AbortSignal` that interruption aborts to a `try` declaring `(signal)`, a rest or defaulted parameter receives `undefined`
- `Effect.abortSignal` yields a signal its scope aborts on close, for a host's `abortSignal` or `{ signal }` option

```ts
const read = Effect.fn('read')((key: string) => Effect.tryPromise({ try: (signal) => bucket.get(key, { signal }), catch: (cause) => StoreError.make({ reason: { _tag: 'Unavailable' }, subject: key, cause }) }), Effect.map(Option.fromNullOr));
```

`bucket.get` is a host call returning `Promise<Uint8Array | null>`.

### [06.1]-[DEPENDENCIES]

Consumers receive the narrowest dependency they read: a value for a stable snapshot, an effect for an operation run on demand, and a service for a capability a program reads through its environment.

### [06.2]-[ERROR_HANDLING]

Failure policies are distinct operations:
- Fallback runs a lower-priority effect after the preferred one fails, `Effect.catch`, `Effect.catchTag` for one classified error, or `Effect.catchReason` for one reason of it
- Timeout bounds a wait, `Effect.timeout` fails with `Cause.TimeoutError` and `Effect.timeoutOption` returns `Option.none()`
- Recovery maps a final failure to a value at the host

Libraries return their outcome type with their own error, and the application composes retry schedule, fallback order, and cache around it.

## [07]-[VALUES]

Quantities with a unit are values of their module, a plain number holds a count or a dimensionless ratio:
- `Duration` holds every elapsed time and timeout, a millisecond number appears at an API taking one
- `DateTime` holds instants and zoned times, `Date` appears at an API taking one
- `BigDecimal` holds money and exact decimal arithmetic, `ByteSize` holds sizes
- `SecureRandom.SecureRandom` service's `uuidV7` creates identifiers, module `uuidV7Unsafe` where code reads no service (an event handler)
- `Intl` formats numbers, dates, lists, and plurals for display, each formatter built once outside the function reading it
- Plural forms are a record over `Intl.LDMLPluralRule` with a required `other`, indexed by the category `select(count)` returns in place of comparing a count or category with one value
- `Intl.Collator(locale, { usage: 'search', sensitivity: 'accent' }).compare(a, b) === 0` states name equality ignoring case alone

## [08]-[MODULES]

Modules hold one concept, and a class holds the concept's data with its operations as statics and getters:
- Packages hold their capability and the host facts it detects, app values (names, paths, timeouts, prose) enter as composition root arguments
- Package types pass whole, and consumers read a returned host handle
- Packages export the values consumers compose
- Modules export named declarations, a default export stands where a host loads one
- Types, `const` values and operations naming one concept share one name under one `export { X }`, an operation over a model its static (`Change.write`, never `{ Change, change }`), `erasableSyntaxOnly` refuses a `namespace`
- Package `exports` entries are the import paths consumers use
- Modules load without side effects, a registry, cache, or connection builds in the layer that owns it
- Use `typescript-coding-effect` for Effect module families, tests, and telemetry
