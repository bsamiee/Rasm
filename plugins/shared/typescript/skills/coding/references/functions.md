# [FUNCTIONS]

## [01]-[PIPELINES]

Data-last package steps take their element type from the value before them. Pipelines over a value in hand pass it to `pipe`, and `flow` composes steps that type their own input (a caller's function, `String.trim`) into one function:

```ts
const displayed = pipe(
    query,
    Option.map((wanted) => Unmatched.rank(wanted, candidates)),
    Option.filter(Array.isReadonlyArrayNonEmpty),
    Option.getOrElse(() => candidates),
    Array.take(limit),
);
```

`Unmatched.of` lists candidates a failed lookup names: `query` is an `Option` of the name a caller asked for and `Unmatched.rank` orders `candidates` by fuzzy match against it. An absent query or an empty ranking lists every candidate, and `Array.take` keeps the first `limit`:
- Package members and applied data-last forms (`Array.take(limit)`, `Schema.decodeOption(S)`) enter as steps by reference
- Lambdas stand where a step joins a captured value with its input
- `Array.map(candidates, flow(names, Array.headNonEmpty))` passes a caller's function composed with a package step by reference
- Generic members whose type comes from their input (`Struct.get('<key>')` in an `Effect.fn` trailing step) take a lambda, a reference inferring `any`
- Steps reading no input pass the effect value to `Effect.tap` or `Effect.andThen`, a `() => effect` thunk restating it

## [02]-[DUAL_SIGNATURES]

`Function.dual` builds one function dispatching on its argument count: a call holding the subject runs the body data-first, and a call without it returns a data-last step:

```ts
const single: {
    <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<A, Unmatched>;
    <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<A, Unmatched>;
} = Function.dual(4, <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) =>
    Unmatched.matching(candidates, subject, query, names).pipe(
        Effect.filterOrFail(Predicate.isTupleOf(1), (matched) => Unmatched.of({ subject: `single ${subject}`, query, candidates: matched.map((each) => Array.headNonEmpty(names(each))) })),
        Effect.map(Array.headNonEmpty),
    ),
);
```

`Unmatched.matching` keeps candidates holding a name `Unmatched.named` equates with `query`. `single(profiles, 'preflight profile', Option.some(requested), (each) => [each.name])` runs data-first, `Effect.flatMap(single('tool', Option.some(id), (tool) => [tool.toString()]))` passes the data-last step, and both fail with an `Unmatched` naming candidates unless exactly one matches:
- Annotations on the binding state both signatures, each with its own type parameters, and the body states the data-first parameters
- Trailing optional parameters make arity ambiguous, a predicate in place of the arity (`(args) => Effect.isEffect(args[0])`) selecting the form
- Callbacks receiving an element index take the applied form or a lambda, as an index argument counts toward the arity `dual` dispatches on
- Exported functions consumers call both directly and in a pipeline are duals, a module-private step stays data-last

## [03]-[PARTIAL_APPLICATION]

Parameters order for partial application: dependencies known at the composition root, then policies selecting behavior, then the runtime value. Each stage runs once as its arguments arrive, with work reading fixed arguments in the outer stage and per-value work alone in the returned function:

```ts
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const words = (text: string) =>
    Iterator.from(WORDS.segment(text))
        .filter(({ isWordLike }) => isWordLike === true)
        .map(({ segment }) => segment)
        .toArray();
const holding = (segment: string) => {
    const [wanted, same] = [words(segment), Array.makeEquivalence(Unmatched.named)];
    return (name: string) => Array.window(words(name), wanted.length).some((run) => same(run, wanted));
};
```

`holding(segment)` splits one step of an interface path (`File > Save As`) into words once and returns a predicate testing whether a name holds that run of words under the collator `Unmatched.named` compares with. `const named = holding(segment)` before `items.filter((item) => Option.exists(item.name, named))` segments the step once however many items it tests:
- Failures fixed arguments decide return at their application as `Option` or `Result` (`Digits.of(system)`), and the returned function stays total
- Callers adjusting a built value (a host call's effect, emitted code) pass one step defaulting to `Function.identity`

## [04]-[FILTERS]

`Filter<In, Pass, Fail>` is a function returning `Result<Pass, Fail>`, narrowing and mapping its input in one call and failing with the typed remainder. `Array.filterMap`, `Array.partition`, `Record.filterMap`, `Option.filterMap`, `Stream.filterMap`, `Stream.partition`, `Effect.filterMapOrFail`, and `Effect.catchFilter` take one, and package constructors build it from the tag, refinement, or lookup stating the rule:

```ts
const [nulls, values] = Array.partition(members, Filter.tagged<SchemaRepresentation.Representation>()('Null'));
```

`members` are the union members of a column schema's `SchemaRepresentation`. `nulls` holds its `Null` members and `values` every other member typed without `Null`, so a column reads its nullability and its value types in one pass:
- `Filter.fromPredicate` builds a filter from a predicate or refinement and `fromPredicateOption` from a lookup, each failing with its input
- `Array.filterMap(urls, Filter.try(fileURLToPath))` keeps what a throwing parse returns, a lambda around `Result.try` restating it
- `Filter.tagged<A>()(tag)` fixes the input type, where schema `guards` take `unknown` and leave the remainder `unknown`
- `Filter.reason<E>()(tag, reason)` passes the reason of one tagged error case to `Effect.catchFilter`
- `Filter.compose` feeds one filter's pass to the next and fails with the second's remainder, `Filter.composePassthrough` with the original input
- `Filter.or` runs the second filter on input the first rejects, `Filter.zip` pairs both passes, and `andLeft` or `andRight` keeps one
- `Array.filterMap` reads any iterable, an `Array.getSomes` over `Array.map` or an `Array.flatMap` returning `[x]` or `[]` restating it
- `Option.filterMap(option, Filter.tagged(tag))` narrows to one case, `Match.tag` with `Match.option` inside `Option.flatMap` restating it
- `Effect.catchFilter` without a fallback replaces `E` with the remainder, and `Effect.filterMapOrFail` builds its error from the remainder
- `Effect.filterMapEffect` runs a `Filter.makeEffect` filter per item under `concurrency`, `Effect.findFirstFilter` returning the first pass

## [05]-[DISPATCH_TABLES]

Dispatch tables select behaviors sharing a signature by a key the input holds. Rows built from combinators state each behavior once, and a list of the cases the table handles derives from its keys:

```ts
const transfers: Readonly<Record<string, Transfer>> = {
    ':': end(keep, Option.none),
    false: end(Option.none, keep),
    break: leave('breaks'),
    return: leave('returns'),
    cd: end(move, keep),
    source: include,
    eval: evaluate,
    echo: spawn(echo),
};
const builtins = Record.keys(transfers);
```

`Transfer` maps a shell builtin's invocation to the analyzer states it leaves: `end(zero, nonzero)` gives the state reaching each exit status, `Option.none` marking one unreachable, `leave(channel)` exits through a loop or function channel, and `spawn(print)` records output. `builtins` names every row, so a new builtin is one row:
- Rows hold a function where behavior differs by case, rows of data one shared function reads are a lookup table
- Shared operations take the row contract's signature and rows name them (`write: exact`, `pack: zipped`), an adapter lambda per row restating it
- Rows stating many facts of one subject are one record `satisfies Record<string, Contract>`, selected by a `keyof typeof table` key
- Parallel string-keyed records and names normalized through `replace` restate that record, an alias (`python3`) joining its row
- Row contracts are an interface and tables `as const satisfies Record.ReadonlyRecord<string, Contract>`, keeping literal keys and typing row lambdas
- Tables over a host enum or value union type as `Readonly<Record<Union, Row>>` with keys `[ParExpOperator.UpperAll]`, one row per value
- Rows keyed by a host member's name as a string restate the member, a computed key holding the member itself
- Partial tables type as `Partial<Record<Union, Row>>`, each read entering through `Option.fromUndefinedOr(table[key])` or `UndefinedOr.match`
- Members some rows lack are optional and read `row.member?.(...) ?? fallback`, a stub row or an `Option` of a function restating absence
- Host unions dispatch through a table typed `{ readonly [T in U['type']]: (node: Extract<U, { readonly type: T }>) => R }`
- `Match.discriminatorsExhaustive('type')(table)` runs that table, each handler taking its narrowed node and a missing or extra key failing to compile

## [06]-[RULE_TABLES]

Rule tables apply every row to an input and fold the verdicts. Rows compile once at construction (alias `Set`, path selector, a row holding a value lifted to a constant function), and each compiled row returns one record whose fields each name a reducer:

```ts
const tally = Struct.makeReducer<Tally>({
    reasons: Array.getReadonlyReducerConcat(),
    notes: Array.getReadonlyReducerConcat(),
    again: Reducer.make(Duration.min, Duration.infinity),
    apart: Boolean.ReducerAnd,
    rename: Record.makeReducerUnion(Combiner.last()),
});
```

`Tally` is a policy verdict holding denial reasons, notes, the shortest recheck a task row asks for, whether a worker stays isolated, and flag renames. Each verdict lifts to a `Partial<Tally>` spread over `tally.initialValue`, and `tally.combineAll` folds a call's verdicts. Identities state absence (`Duration.infinity` asks no recheck, `true` keeps isolation), so no field takes an `Option`, and a new verdict kind is one field:
- Row contracts tie each action kind to its verdicts through an `interface` keyed by kind and read by a `Verdict<K>` type
- Tables type rows as `{ [K in Kind]: Row<K> }[Kind]`, each row's `decide` receiving `(typeof Action.cases)[K]['Type']`
- Compiling a row selects `Action.guards[row.action]` by the same key, a per-call `Schema.is` or `Match.value` over the row restating it
- Decorators over a whole table map its rows through `Record.map(rows, decorate)` (a `passHelp` letting every `--help` form through)
- Derivations every row reads compute once per evaluation into the call context (a script's invocations), a row re-walking its input restating it
- Tables a caller extends merge by name through `Record.makeReducerUnion(Combiner.last())`, a `null` row removing a name before compile

## [07]-[EFFECTFUL_FUNCTIONS]

`Effect.fn('<name>')` declares an effectful function and runs each call's body with its trailing steps inside one span and stack frame named for it. Trailing steps receive the body's effect and the caller's arguments without body parameter defaults, and hold each step reading an argument or wrapping a generator body:

```ts
const run = Effect.fn('run')(function* (argv: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) {
    const handle = yield* spawn(argv, { stdin: 'ignore', ...options, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([handle.stdout.pipe(Stream.decodeText, Stream.mkString), handle.stderr.pipe(Stream.decodeText, Stream.mkString), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? { stdout, stderr } : yield* ExitError.of({ command: CommandLine.display(argv), code, stdout, stderr });
}, Effect.scoped);
```

`spawn` starts a `ChildProcess` with the current trace context in its environment, and `ExitError` is the package error. `run` reads both pipes and the exit code at once and fails with `ExitError` on a non-zero code, `Effect.scoped` closing the process handle inside the span as each call returns, and `PlatformError` reaches the caller unchanged:
- Span names follow the binding, `<Class>.<member>` for a class field and `<Service>.<key>` for a service member
- Attributes every call shares sit in `Effect.fn('<name>', { attributes })`
- Steps reading an argument take it after the effect (`(effect, sql) => Effect.provideService(effect, SqlClient.SqlClient, sql)`)
- Trailing steps return an `Effect`, and a step returning any other value (a `Stream`) leaves the call with no span or stack frame
- Bodies a package function builds (an `SqlSchema` finder) join steps in one `Function.flow` body, as trailing steps after one type `E` as `never`
- `Effect.fn(body)` without a name marks a stack frame and opens no span
- `Effect.fnUntraced` declares a generator function a traced one calls, with no stack frame or span, in place of a function returning `Effect.gen`
- `Effect.fn.Return<A, E, R>` annotates the return of a generic or recursive generator body
- `Effect.fn('<name>')({ self: this }, body)` and `Effect.gen({ self: this }, body)` bind `this` inside a class member
- `Option.gen(this, body)` and `Result.gen(this, body)` take `this` as their first argument
