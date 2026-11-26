---
name: coding
description: "Use when writing or reviewing TypeScript, covering functional design with Effect, Schema, and algebraic data types"
---

# [TYPESCRIPT_CODING]

Covers functional TypeScript on Effect modules.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Functions as values and their composition
- [02]-[TYPES](references/types.md): Types derived from the values that own them
- [03]-[DATA](references/data.md): Immutable data and its transitions
- [04]-[RESULTS](references/results.md): Typed failure, absence, recovery, and defect boundaries
- [05]-[SCHEMA](references/schema.md): Schemas as codecs at a boundary
- [06]-[SERVICES](references/services.md): Services, layers, and runtimes
- [07]-[CONCURRENCY](references/concurrency.md): Concurrent work and the state it shares
- [08]-[ANALYSIS](references/analysis.md): Programs parsed, evaluated, and rewritten as data

## [01]-[DECLARATIONS]

One declaration states each concept, its set of cases included, and every other form derives from it:

```ts
class Digits extends Schema.Class<Digits, { readonly brand: unique symbol }>('Digits')({
    system: Schema.String,
    table: Schema.Array(Schema.String.check(Schema.isPattern(/^\p{Nd}$/u))).check(Schema.isBetweenLength(10, 10)),
}) {
    static readonly #bySystem = pipe(
        Intl.supportedValuesOf('numberingSystem'),
        Record.fromIterableWith((system) => [system, Array.makeBy(10, new Intl.NumberFormat(undefined, { numberingSystem: system }).format)]),
        Record.filterMap((table, system) => Schema.decodeResult(Digits)({ system, table })),
    );
    static readonly of = (system: string): Option.Option<Digits> => Record.get(Digits.#bySystem, system);
}
```

`Intl.supportedValuesOf` lists every numbering system the runtime holds, the class check keeps the decimal ones, and `Digits.of` reads one by key, so a runtime adding a system adds a row with no edit:
- Field lists derive from the schema owning them (`Struct.pick(value, Struct.keys(Reference.fields))`, `Control.mapFields`), a key list restating them
- Values in a role are a record keyed by role (`{ begin, end }`), a tuple zipped against role values misaligning when either order changes
- Cases differing in data are field sets of one tagged union, cases differing in values no rule decides are rows of one `as const` record
- Rows hold every column of their key (`{ awk: { idioms, effects } }`) read by `keyof typeof table`, parallel tables per column restating them
- Tables crossing axes are nested `Record.map` over each axis's record where a nested domain value reads them, else one `Array.cartesianWith`
- Products over external input (brace expansion) bound their size before `Array.cartesianWith` builds them, returning `Option.none()` past the limit
- Members differing in a number, flag, literal, or applied function are one member taking that value alone
- Parameters default to a value an owner states (a host's resolved option, a standard), and every other parameter is required
- Parameters forwarded to a host argument the host defaults stay optional and pass `undefined`, a literal test choosing that default restating it
- Parameters every caller passes one value for fold into their owner, and fields no caller acts on go
- Parameters another parameter determines (a config path from a setting's name) derive from it inside the function
- Values read once are a local in their reader named for their fact, inline where the value is another name, and a value read twice is a `const`
- Constants one relation joins (a width and height, a minimum and maximum, names and their order) are one value
- Shapes a client and a server both spell (wire records, header keys, RPC payloads) are one contract schema both import, a consumer's copy drifting
- Numbers come from the package member, standard, or measured property stating them, else from a parameter
- Schemas declare at module scope and parsers cache by AST, a schema over a caller value building once in `Function.memoize` or a field

## [02]-[FUNCTIONS]

Signatures state every value a function accepts and every outcome its caller handles. Parameters and fields are `readonly`, arrays `readonly A[]`, or `Array.NonEmptyReadonlyArray<A>` where the rule needs an element, and `Effect.forEach` over a non-empty input (`Array.range`, `Array.makeBy`) returns one.

Total functions return a declared output for each accepted input, narrowing the input to a decoded type or widening the output to `Option`, `Result`, or `Effect`:
- Parameters take the decoded type (`Digits`, `Control['kind']`)
- `Schema.decodeUnknownOption(Schema.NumberFromString)` states every outcome of a parse

Pure functions depend on their arguments alone and cause no side effect. Clock, randomness, environment, logging, and I/O are effects.

## [03]-[EXPRESSIONS]

Bindings are `const`, and values build in one expression stating each field at creation:

```ts
const kashidaPlaces = (text: string, rows: Parameters<typeof candidates>[2]) =>
    words(text)
        .map((word) => joins(text, word).flatMap((join) => candidates(text, join, rows)))
        .filter(Array.isArrayNonEmpty)
        .map(Array.min(BY_RANK))
        .toSorted(BY_RANK);
```

`words`, `joins`, and `candidates` read the joining positions of Arabic text, and `BY_RANK` orders a place by its rank. `Array.isArrayNonEmpty` narrows each word's places, so `Array.min` returns the best one with no absent case:
- Native methods state direct `filter`, `every`, `join`, `reduce`, or `Set` relations (`union`, `difference`, `isSupersetOf`), Effect modules others
- Iterator helpers (`map`, `toArray`, `join`) traverse a `Map`, `Set`, or `matchAll` result, a spread or `Array.fromIterable` restating them
- `Record.collect` maps entries to an array and `Record.keys` lists keys, after any `Record.filter`, `Object.entries` pairs restating them
- `Record.filterMap` maps through a `Result`: `Result.failVoid` drops an entry, `Result.fromOption` enters a lookup, `Schema.decodeResult` validates
- Dictionaries use `Record.getSomes`, which widens keys; closed optional fields spread `Option.map` field objects through `Option.getOrUndefined`
- Resources shared across cases build once as an `Option` over the non-empty set of cases needing them, each case joining through `Option.toArray`
- `Struct.renameKeys` maps keys to the spelling a host reads, `Struct.evolveKeys` computes them, a destructure rebuilding the object restating both
- Named reductions (`Number.sumAll`, `BigDecimal.sumAll`, `Array.max`, `Array.groupBy`) state their fold, `Math.sumPrecise` a Node float sum
- Sequences zip through `Array.zip` in pairs and `Iterator.zip` in larger sets, browser and Adobe host code keeping `Array.zip`
- `reduce` with a seed folds a reduction no member names, `Array.mapAccum(xs, seed, (state, x) => [next, y])` maps under a running state
- `Array.makeBy` and `Array.range` generate sequences from a length or bounds
- Relations between neighbors map the adjacent pairs `Array.window(values, 2)` returns, a gap filling from `Array.range` over its pair
- `RegExp.escape(text)` builds a pattern from literal text, a `replaceAll` escape restating it

### [03.1]-[PATTERN_MATCHING]

Unions own their fold: `Schema.TaggedUnion` through `match`, `Data.taggedEnum` through `$match` and `Option.filter(<option>, <enum>.$is(<tag>))` for a field of one case, and any other value through `Match`:

```ts
const verdict = Match.value({ command: Option.getOrUndefined(command), inPlace, json: jsonParse.test(written) }).pipe(
    Match.when({ command: Match.defined }, ({ command: words }) => rewrite(invocation, words)),
    Match.when({ inPlace: true }, () => [{ deny: `${name} -i edits files in place.`, advice: 'Edit them with sd.' }]),
    Match.when({ json: true }, () => [{ deny: `${name} inline code parses JSON.`, advice: 'Read it with jq.' }]),
    Match.orElse(() => []),
);
```

`command` is an `Option` of the words an idiom restates the invocation as, `rewrite` builds that edit, and `inPlace` and `json` are facts of the invocation and its code (`jsonParse` matching a JSON parse). Arms read facts by name, and `Option.getOrUndefined` lets `Match.defined` narrow the command:
- `Match.exhaustive` closes a decision over a closed union, `Match.orElse` closes an open one with a fallback outcome
- `Match.option` closes a match where unmatched input is absence and `Match.result` one for `filterMap`, `Match.orElse` to none restating them
- `Match.when` takes a refinement or a partial pattern with a value or refinement per field (`Schema.is(X.fields.f)`), `Match.whenOr` alternatives
- `Match.not` takes an exclusion, `Match.orElse` closing the arms it leaves
- `Match.valueTags(value, handlers)` folds a `_tag` union with one handler per tag
- Paths dispatch on role and arity (`{ role, below: Predicate.isTupleOf(2) }`), a path no arm names falling to the refusal arm
- Fields a rule derives from the input join once outside the fold as defaults, an arm overriding one only where its host departs from the rule
- Arms return what their case adds, one value or many through `Array.ensure`
- Values compare against a constructed case (`Equal.equals(stdin, Stdin.Piped({ pipeline, index }))`), a tag test and a `===` per field restating it
- Host enums match with an arm per member once decoded
- Host unions with a discriminant match through `Match.discriminatorsExhaustive(key)(handlers)`, a `'k' in x` or `type !==` branch restating it
- Refinements come from `Predicate` (`isObject`, `hasProperty`, `isTagged`) composed through `Predicate.and`, `or`, and `not`
- `Predicate.isTagged` inside a `filter` narrows nothing after it, and `filterMap(Filter.tagged(tag))` narrows to the case

### [03.2]-[RECURSION]

TypeScript runs tail calls on the stack, and a recursion's depth bound selects its form:

| [INDEX] | [APPROACH]            | [USE_WHEN]                                                                              |
| :-----: | :-------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | Direct recursion      | Depth small and bounded (parsed tree), or consumed input bounding it                    |
|  [02]   | `Array.unfold`        | Each state yields one element and an `Option` of next state (a folder's ancestors)      |
|  [03]   | `Array.mapAccum`      | Evaluation threading state through a sequence (an assignment reaching next word)        |
|  [04]   | `Effect.suspend` loop | Effectful transition at unbounded depth                                                 |
|  [05]   | `Stream.paginate`     | Each round yields outputs and an `Option` of next state (cursor, frontier, fixed point) |
|  [06]   | `Graph` traversal     | Shared or cyclic successors                                                             |

### [03.3]-[NESTING]

oxfmt prints a call with more than one argument one argument per line at any width when more than one argument is an inline function or one argument is a call taking an inline function:
- Calls holding one inline function beside plain arguments, calls of one argument, and object literals of inline functions print on one line
- Generators print each statement at their body's level
- Callbacks a host types `void` keep a block body around their statement, `typescript/strict-void-return` refusing a value-returning expression body

Data-first calls nested through their first argument become one pipe from the innermost subject, each call a data-last step at one level:

```ts
const displayed = (query: Option.Option<string>, candidates: readonly string[], limit: number) =>
    pipe(
        query,
        Option.map((wanted) => Unmatched.rank(wanted, candidates)),
        Option.filter(Array.isReadonlyArrayNonEmpty),
        Option.getOrElse(() => candidates),
        Array.take(limit),
    );
```

`Unmatched.rank` orders candidates by their match to the query, and `displayed` prints one level where `Array.take` around `Option.getOrElse` around `Option.filter` around `Option.map` printed a level per call.

Forbidden forms take the rebuild in their row, each module imported from `effect`:

| [INDEX] | [FORBIDDEN]                                                     | [REBUILD]                                                        |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `acquireUseRelease`, `acquireRelease` over a disposable handle  | `acquireDisposable` piped through each use, `Effect.scoped` last |
|  [02]   | Callback reading an `Effect.all` tuple                          | `yield* Effect.all([...])` in a generator, fold as its `return`  |
|  [03]   | `flatMap` into `Option.match`, `.value` after an early `return` | `Option.match(yield* effect, {...})` in a generator              |
|  [04]   | Array pattern holding an object pattern                         | `Array.headNonEmpty(items).<field>` destructured flat            |
|  [05]   | `Option.liftPredicate`, then `Option.flatMap` to a field        | `Option.filter(Option.fromUndefinedOr(field), predicate)`        |
|  [06]   | `Result.fromOption(Option.map(option, f), g)`                   | `Option.match` arms returning `Result.fail`, `Result.succeed`    |
|  [07]   | Lookup pairs built, then walked by `forEach`                    | One walk, each lookup through `Option.flatMapNullishOr`          |
|  [08]   | `Effect.forEach` yielding `Option`, then `Array.getSomes`       | `Effect.filterMapEffect` with a filter yielding a `Result`       |
|  [09]   | `pipe(<call>, Function.constVoid)` standing for a statement     | Block-bodied callback                                            |
|  [10]   | `Effect.succeed(value)` mapped or joined in `Effect.all`        | Plain value read inside `Effect.map` over the effect             |
|  [11]   | `match` arms repeating one result across tags                   | `matchOrElse` with the differing tags as arms                    |
|  [12]   | `Array.match` into `Option`, then `Array.getSomes`              | `filter(Array.isArrayNonEmpty)`, then the non-empty member       |
|  [13]   | Set grown until `next.size === held.size`                       | `Graph.dfs` over `Graph.fromSnapshot`                            |
|  [14]   | `relative(a, b).startsWith('..')`, `startsWith(folder + '/')`   | `path.resolveWithin(folder, relative)` returning `Some`          |
|  [15]   | `Stream.runFold(init, (_, last) => last)`                       | `Stream.runLast`                                                 |

`Effect.filterMapEffect` returns successes in completion order under `concurrency`, and an `Option` held before the effect keeps `Effect.transposeOption`. `Effect.when` takes its condition as an `Effect<boolean>`, so a flag computed before the call enters as `Effect.succeed(flag)`.

## [04]-[IMMUTABILITY]

Records a signature names are a class over their schema, `Schema.Opaque` over a struct, array, or tuple for plain data and `Schema.Class` for instances with getters and methods. Data inside the process is `Data.Class` and `Data.taggedEnum` where it carries a name or operations, and a module-private record stays a readonly object `Equal` compares by its keys.

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

Input boundaries select the outcome type, functions preserve it, and one conversion point uses the function named for its target (`Effect.fromOption`, `Effect.fromResult`, `Option.fromNullishOr`, `Result.fromOption`). `null` and `undefined` appear at a host API alone, a host field whose absence means a default reads `field ?? default` at the read, an optional value passes directly to a host prop accepting `undefined` and through a conditional spread to one refusing it, and `Option` stands where a consumer acts on absence. Absent text stays `undefined` or `Option`, never `''`, and text with an optional part is one expression choosing between complete values (``Option.match(tag, { onNone: () => name, onSome: (suffix) => `${name}-${suffix}` })``); `''` stands only where a host input spells unset as `''` (a JSX `value`, a `<select>` placeholder), decoded back to absence at that input. Cases sharing an absent spelling are a tagged enum compared through `Equal`, as a session and an agent both keyed `''` collide. Casts and `!` never stand in for a presence proof, a decode or `Effect.fromNullishOr` supplying it. Empty arrays are values, `Option<readonly A[]>` stands where a consumer treats no collection and an empty one differently.

Each package declares one error class for the expected failures its producers report, holding a `reason` case for each response a caller gives with the data that response reads, the `subject` that failed, and a foreign `cause` where one exists, with a message derived from them:

```ts
class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', {
    reason: Schema.TaggedUnion({ Refused: {}, Unavailable: {}, Cancelled: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: HostError['reason'], subject: string) => (cause?: unknown) =>
        HostError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [`${subject} ${String.toLowerCase(reason._tag)}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
    static readonly refused = (subject: string) => HostError.of({ _tag: 'Refused' }, subject);
    override readonly [ErrorReporter.severity]: LogLevel.Severity = this.reason._tag === 'Unavailable' ? 'Warn' : 'Info';
}
```

`HostError.refused('panels and menu areas')` is the `catch` of each host call reading them. `of` omits an absent `cause`, a `cause: undefined` key decoding as `null` across a JSON wire, and `Formatter.format` renders a foreign cause with its nested chain:
- `message` is a field the constructing static computes, an own property `structuredClone` and a worker's serializer copy where a getter is lost
- `[ErrorReporter.severity]` gives a reporter the level of each reason, `'Info'` where the class sets none
- Instances are effects failing with themselves, `return yield*` raises one in a generator and a handler returning one fails with it
- `Effect.when(error, condition)` takes an instance as its effect, an `Effect.fail` or `flow(make, Effect.fail)` around one restating it
- Errors a package member types (`SchemaError`, `HttpClientError`, `Cause.TimeoutError`) stay in `E` unchanged
- Codecs over a value the program built die through `Effect.orDie`
- Absences and refused arguments fail with the `Cause` error naming them (`NoSuchElementError`, `IllegalArgumentError`)
- Refusal messages name received and accepted kinds, class names from the constructor, and a bound's range solved from the operation's relation
- Values no package error owns (an entry script's refusal) fail with `new Cause.UnknownError(value, message)`, the message naming the refusal
- Messages render a schema-typed field through `Schema.encodeResult(Schema.fromJsonString(Schema.toCodecJson(S)))`, never `String`
- Pure checks return finding lists joined into one error over `Schema.NonEmptyArray(Finding)`, its `raise` static failing on a non-empty list
- Foreign failures become a `cause` under a subject a caller acts on, a `String(cause)` or `error.message` detail flattening its chain

### [05.1]-[COMPOSITION]

Dependent steps bind and independent ones combine through `Effect.all`:
- `Effect.gen` holds a sequence where a later step reads more than one earlier value, `pipe` holds a linear chain, one step is the effect itself
- Values stay in one type through a pipeline, an `Option` inside an effect included, and a consumer matches it once at the end

```ts
const run = Effect.fn('run')(function* (argv: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
    const handle = yield* spawn(argv, { stdin: 'ignore', ...options, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? { stdout, stderr } : yield* ExitError.of({ command: CommandLine.display(argv), code, stdout, stderr });
}, Effect.scoped);
```

`spawn` starts a `ChildProcess` with trace context in its environment, both pipes drain beside the exit code, and the code selects between the output and an `ExitError` the generator raises. Dependency structure and concurrency bound select a traversal over many effects:

| [INDEX] | [SCENARIO]                   | [TRAVERSAL]                                                | [BEHAVIOR]                               |
| :-----: | :--------------------------- | :--------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Independent checks           | `Effect.validate`                                          | Runs every check, keeps every failure    |
|  [02]   | Independent effects          | `Effect.forEach` with `concurrency: 'unbounded'`           | Overlaps, interrupts rest on failure     |
|  [03]   | Dependent or ordered effects | `Effect.forEach` without `concurrency`                     | Serial, stops at first failure           |
|  [04]   | Bounded concurrency          | `Effect.forEach` with `concurrency: <n>`                   | At most `<n>` effects at once            |
|  [05]   | Every effect, failures kept  | `Effect.partition`                                         | Runs all, returns successes and failures |
|  [06]   | Every outcome kept in order  | `Effect.all` with `mode: 'result'`                         | Runs all, one `Result` per effect        |
|  [07]   | Every effect, causes joined  | `Exit.asVoidAll` over `Effect.forEach(items, Effect.exit)` | Runs all, fails with every cause in `E`  |
|  [08]   | Outcomes up to first failure | `Stream.takeUntil(Result.isFailure)` over `Effect.result`  | Serial, keeps outcomes to the failure    |
|  [09]   | First success, misses kept   | `Effect.flip(Effect.forEach(items, flow(f, Effect.flip)))` | Serial, fails with every miss in order   |
|  [10]   | Effectful predicate          | `Effect.filter(items, (item) => Effect<boolean>)`          | Keeps items yielding `true`              |
|  [11]   | First effectful match        | `Effect.findFirst(items, (item) => Effect<boolean>)`       | Serial, runs no item after first match   |
|  [12]   | First match mapped           | `Effect.findFirstFilter(items, (item) => Effect<Result>)`  | Serial, returns the first success value  |

`Effect.firstSuccessOf` fails with its last error alone, so alternatives whose misses each list candidates take row 09 and merge the misses into one error.

### [05.2]-[UNIONS]

Each valid state a consumer inspects is one case. Counting the values a type allows (a product multiplies its fields' counts, an optional field adds one) exposes states no case needs:
- Flags with fields meaningful for one flag value become one case per valid state
- States with no data and no behavior are a literal or a default value of a neighboring case
- Absence meaning a default is a case of its own (`region` beside `windows` and `element`), an optional union describing it in prose
- Folds stand where behavior depends on the case, the union passes onward elsewhere
- Unions fit a growing set of operations, a service fits a growing set of implementations
- Absence and failure stay separate where a consumer responds differently
- Optional fields set together are one optional struct

```ts
const Condition = Schema.TaggedUnion({
    Value: { at: Locator, value: Value },
    Documents: { held: Schema.Boolean },
    Modal: { at: Schema.NullOr(Locator) },
});

const when = Condition.match({
    Value: ({ at, value }) => `when ${at} is ${value}`,
    Documents: ({ held }) => (held ? 'when a document is open' : 'when no document is open'),
    Modal: ({ at }) => (at === null ? 'when no dialog is open' : `when ${at} is open`),
});
```

`Condition` is one fact a control's presence depends on, `Locator` the encoded path of a control or dialog, and `Value` a read value.

## [06]-[EFFECTS]

`Effect<A, E, R>` describes a program that succeeds with `A`, fails with `E`, reads services `R`, and performs nothing until a runtime runs it. Promises are eager, enter through `Effect.tryPromise`, and leave through a host runtime call:
- `Effect.sync` defers an infallible side effect, `Effect.try` a throwing one, its `catch` wrapping the throw in the package error or a `SchemaIssue`
- `Effect.promise` turns a rejection into a defect, so a host promise enters through `Effect.tryPromise` with the package error
- Promise steps take one `Effect.tryPromise` each with their own reason, a `.then` chain inside one giving every step one failure
- `Effect.callback` adapts a callback API, with registration receiving an `AbortSignal` and returning a cleanup effect
- Listeners completing a `Deferred` the registering function awaits restate `Effect.callback`, except where a host callback must throw to roll back
- `Effect.tryPromise` passes a `try` declaring `(signal)` an `AbortSignal` interruption aborts, a rest or defaulted parameter receiving `undefined`
- `Effect.abortSignal` yields a signal its scope aborts on close, for a host's `abortSignal` or `{ signal }` option

```ts
const page = (assets: Fetcher) =>
    Effect.tryPromise({ try: (signal) => assets.fetch(new Request('http://localhost/', { signal })), catch: HostingError.of('Template', 'fetch') }).pipe(
        Effect.filterOrFail(({ ok }) => ok, ({ status }) => HostingError.of('Template', `answer ${status}`)()),
        Effect.flatMap((response) => Effect.tryPromise({ try: () => response.text(), catch: HostingError.of('Template', 'read') })),
    );
```

`Fetcher` is the Workers binding type of static assets and `HostingError.of(subject, step)` the package error's constructor, interruption aborts its fetch, and the fetch, the status, and the body read each fail with their own reason.

### [06.1]-[DEPENDENCIES]

Consumers receive the narrowest dependency they read: a value for a stable snapshot, an effect for an operation run on demand, and a service for a capability a program reads through its environment. Contracts mark members some implementers lack optional, read as `engine.exclusive?.(client, plan) ?? plan`, and functions take `Pick<Contract, '<key>'>`. A stub returning `Effect.void` or `Option.none()`, or an `Option<Option<A>>` whose outer level means the implementer lacks the member, restates absence.

### [06.2]-[ERROR_HANDLING]

Failure policies are distinct operations:
- Fallback runs a lower-priority effect after the preferred one fails, `Effect.catch`
- Timeout bounds a wait: `timeout` fails with `TimeoutError`, `timeoutOrElse` with a reason naming what never arrived, `timeoutOption` with none
- Recovery maps a final failure to a value at the host
- Compensation runs a failed step's inverse, `Effect.onError(step, () => Effect.orDie(inverse))`, keeping the step's failure in `E`
- Inverses a later failure in scope triggers run as `Effect.acquireRelease(step, (_, exit) => Exit.isFailure(exit) ? inverse : Effect.void)`
- Retries repeat while each attempt shrinks the failed set (`left.size < failed.size && left.isSubsetOf(failed)`), a fixed count retrying a stuck step
- Failure causes stay in `E`, and a fallback, default, or retry hiding one is a defect

Libraries return their outcome type with their own error, and the application composes retry schedule, fallback order, and cache around it.

## [07]-[VALUES]

Quantities with a unit are values of their module, a plain number holds a count or a dimensionless ratio:
- `SemVer` holds versions, decoded through `Schema.SemVerFromString` and written through `SemVer.format`, a `\d+\.\d+\.\d+` pattern restating them
- `Duration` holds elapsed times, a millisecond number at an API taking one, and text typed `` `${number} ${Duration.Unit}` `` is an undecoded input
- `DateTime` holds instants and zoned times, `Date` appears at an API taking one
- `BigDecimal` holds money and exact decimal arithmetic and `ByteSize` sizes, `Duration.format` and `ByteSize.format` rendering fixed English for logs
- `SecureRandom.SecureRandom` service's `uuidV7` creates identifiers, module `uuidV7Unsafe` where code reads no service (an event handler)
- `Intl` instances built once outside their reader segment words and format numbers, lists, plurals, and times in a caller locale, else the host's
- `Intl` `format` and `compare` are bound getters a step takes by reference (`Array.makeBy(n, formatter.format)`)
- Sizes render through a `style: 'unit'` `NumberFormat` per byte unit over `ByteSize.toUnit`, at the largest unit the rounded value reaches
- Instants render through `DateTime.formatIntl`, offsets through `RelativeTimeFormat` at the largest `Duration.to<Unit>` reaching 1
- Durations render through `DurationFormat` over `Duration.parts`, its sub-millisecond `nanos` split into `microseconds` and `nanoseconds`
- Plural forms are an `Intl.LDMLPluralRule` record of functions over the formatted count, `other` required, a `PluralRules` per rule type
- Non-negative counts pad to their formatted total's length with formatter's `format(0)`, a fixed `minimumIntegerDigits` restating the width
- Host names compare through one `Equivalence` over `Intl.Collator(locale, { usage: 'search', sensitivity: 'base', ignorePunctuation: true })`
- `sensitivity: 'base'` with `ignorePunctuation` equates `&File` with `File` and `Save As…` with `Save As`, which `'accent'` keeps apart
- Search folds query and candidates through one normal form (`Digits.fold(text.normalize('NFKD').replaceAll(/\p{M}/gu, ''))`) before ranking
- Patterns under `u` match digits through `\p{Nd}`, `\d` reading ASCII alone

## [08]-[MODULES]

Classes hold their data with operations reading one instance as methods and getters, statics building or relating instances. Concepts caller data configures are a `Schema.Class` of the data with each use a method:
- Packages hold their capability and the host facts it detects, app values (names, paths, timeouts, prose) enter as composition root arguments
- Seams a product fills are optional parameters with error and service type parameters defaulting to `never`, filled where no derivation reaches
- Exported names read correctly beside other packages' names: nouns for owners, verb-noun for functions
- Union schemas, components, and primary functions hold their variants as members `Object.assign` or an expando sets
- Schemas declare where a boundary decodes or encodes through them, a member schema reached through its owner
- Libraries return a layer or effect their entry point runs, require platform services, and leave the platform layer and runtime to that entry point
- Package types pass whole, and consumers read returned host handles and the context a host passes its callback, a re-read from source restating them
- Tool facts come from the tool's own resolution decoded through a schema (`calculateConfigForFile`, `rustc --print`, `jscpd --debug`)
- Models of a tool's config search, regexes over its human output, and tests of its error wording restate that resolution
- Host process-global defaults (`GlobalWorkerOptions.workerSrc`) stay unset, each instance taking the value through its constructor option
- Resources a caller passes to a host handle release with the caller
- Values a renderer escapes for its context pass unescaped, a hand `<` escape in React `<style>` text restating it
- JSON fixed at build (a repository or dependency manifest) imports `with { type: 'json' }`, a read and decode standing for files changing after build
- Modules loaded by name are a table of `() => import('<path>')` thunks typed by the import, a schema over the loaded module restating it
- Programs started per call import only what their verdict reads, heavy subsystems behind `import()`
- Members only their owner reads stay module-private, a published one letting a consumer change the owner's state
- Types, schemas, and operations of one concept share one name under one `export { X }`, a `thing`, `ThingView`, or `thingFor` restating them
- Modules load without side effects, a registry, cache, or connection building in its layer, a memo doing no I/O at load excepted
- Use `typescript:effect` for Effect module families and telemetry
- Use `typescript:testing` for suites
