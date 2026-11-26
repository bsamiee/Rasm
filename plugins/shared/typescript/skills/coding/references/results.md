# [RESULTS]

## [01]-[ABSENCE]

Reads return `Option` at their source. Effectful reads where each step reads the value the previous step found chain through `Effect.fromOption` and `Effect.fromNullishOr`, each failing an absent step with `Cause.NoSuchElementError`, and one `Effect.catchNoSuchElement` at the end returns the chain as an `Option` while every other error stays in `E`:

```ts
const chain = Effect.fnUntraced(function* ({ shell, script }: Call, lines: readonly (readonly string[])[]) {
    const [first, ...others] = yield* Effect.forEach(lines, (line) => shell.script(script.dialect)`${line}`);
    const start = yield* Effect.fromNullishOr(first);
    const stmt = yield* Effect.fromOption(Array.head(start.syntax.stmts));
    return yield* shell.edit(start, (editor) => others.map((next) => editor.follow(stmt, '&&', next)));
}, Effect.catchNoSuchElement);
```

`Call` holds a hook call's `shell` and parsed `script`, `shell.script` parses one command line, and `shell.edit` rewrites a parsed script through an editor. `chain` returns the script running each later line after the first line's first statement through `&&`, none for no lines or an empty first line, and a `ShellError` from parsing or editing stays in `E`:
- `Effect.fromOption(option, onNone)` fails with the error its thunk builds, an `Unmatched` with candidates or a named `NoSuchElementError`
- Steps a caller must see fail inside the chain fail with a package error, the closing catch reading each `NoSuchElementError` as none
- `Effect.filterOrFail(effect, predicate)` gates a step into the chain, a `fromOption` after a hand check restating it
- `Effect.filterOrFail(lookup, rule, onFail)` refuses a count (`Predicate.isTupleOf(1)`, `Array.isReadonlyArrayEmpty`)
- `Result.filterOrFail(Function.constant(flag), onFail)` refuses on a fact computed before the chain as one step
- `Array.headNonEmpty` reads the element `Predicate.isTupleOf(1)` passed and the first part of `String.split`, a `[0] ?? text` restating it
- `UndefinedOr.match` and `UndefinedOr.map(x, f) ?? fallback` fold a host `T | undefined`, an `Option` round trip restating them
- `Option.firstSomeOf` returns the first present source of a list
- `Option.all` combines independent options over a tuple or struct for one `Option.match`, and over an array holds a value when every element does
- `Option.zipWith(a, b, f)` joins independent options into one value, an `Option.map` inside an `Option.flatMap` restating it
- `Option.bind('key', (fields) => lookup)` adds a looked-up field to a struct `Option.bindTo` started, consumers destructuring both
- `Option.gen` and `Result.gen` sequence pure steps where a later one reads more than one earlier value, each guard a `yield*` of `liftPredicate`
- `Option.liftThrowable` and `UndefinedOr.liftThrowable` lift a call whose throw means absence, an `Effect.try` discarding its error restating them
- `Array.findFirst` and `Array.findLast` with a function returning `Option` find and map in one pass, the callback's index reading a neighbor
- `Array.fromNullishOr(x)` spreads a nullable value into an array literal
- `Effect.transposeOption` turns an `Option` of an effect into an effect of `Option`, an `Option.match` with an `Effect.void` arm restating it
- `Effect.when(effect, condition)` runs `effect` into `Option`, and `.pipe(Effect.orElseSucceed(Option.none))` reads its failure as none too

## [02]-[ACCUMULATION]

Independent checks run to the end and fail with every failure. `Effect.validate` fails with a `NonEmptyArray` of failures, `Effect.sandbox` on each element puts its defects in that list as causes, and one catch merges the causes:

```ts
const failAll = <E>(causes: readonly Cause.Cause<E>[]) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))));
const write = (target: InDesignDOMObject<unknown>, values: Record.ReadonlyRecord<string, unknown>) =>
    Effect.map(decodeWrites(target, values), (writes) => Effect.validate(writes, ([name, write]) => Effect.sandbox(Effect.as(write, name))).pipe(Effect.catch(failAll)));
```

`decodeWrites` checks every field of `values` against the InDesign object's declaration and returns one host write per field, failing with every refused field before any write runs. The returned effect runs every write and fails with one cause holding each refusal, a throwing setter's defect included:
- Checks whose successes a later step reads run `Effect.validate` over `flow(check, Effect.sandbox)`, `Exit.asVoidAll` where none is read
- Rules relating a value to data it lacks are validators `Result.liftPredicate(predicate, onFail)` builds, entering through `Effect.fromResult`
- Independent rules over one value accumulate as `Effect.validate(rules, (rule) => Effect.fromResult(rule(value)))`
- Misses of one kind merge into one error, an `Unmatched` naming every missing query or an `UnknownError` naming each unreadable file
- Caller names resolve in one `Array.partition` against the host's list, continuing with host spelling and failing once with every missing name
- Listings tag each failure of an `Effect.partition` with its subject (`[file, error]`) and return it beside the items read
- Hosts with a partial-result type receive both, `new AggregateCreateNodesError(failures, results)` keeping each decoded Nx project
- `Schema.Result(Schema.Array(X), Schema.ErrorInstance())` fails a listing whole on one unreadable item
- Consumers of a repeated listing log each unreadable subject once, `Stream.mapAccum` carrying a `HashSet` of the subjects logged

## [03]-[RECOVERY]

Consumers recover from failures they hold a response to, and every other failure passes on in `E`. A portfolio asset read answers store failures as unavailable and an absent row as not found:

```ts
const unavailable = Effect.fn('unavailable')((error: Cause.YieldableError) => Effect.andThen(Effect.logError(Cause.fail(error)), HttpApiError.ServiceUnavailable.make()));
const readAsset = Effect.fn('readAsset')(Function.flow(SqlSchema.findOne({ Request: AssetRead, Result: AssetObject, execute: selectAsset }), Effect.catchTag(['SqlError', 'SchemaError'], unavailable, () => HttpApiError.NotFound.make())));
```

`SqlSchema.findOne` fails with `NoSuchElementError` when `selectAsset` matches no row. `unavailable` logs the store failure as its cause and fails with `ServiceUnavailable`, the third handler takes every tag the list leaves, and `E` holds `ServiceUnavailable` and `NotFound` alone:
- `Effect.catchTag` takes one tag or a non-empty list and removes what it handles from `E`, its `orElse` taking every other tag
- `Effect.catchIf` with a refinement narrows `E`, and a predicate (`({ cause }) => cause instanceof NotFound`) keeps `E` whole until an `orElse`
- `Effect.catchTag('NoSuchElementError', Effect.die)` turns an absence the query excludes (a `LEFT JOIN` from one literal row) into a defect

## [04]-[HOST_VALUES]

Hosts convert an outcome once, at the call running it. An `Exit` passes through a host callback whole, where a rejected promise holds one error:

```ts
const modal = Effect.fnUntraced(function* <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>) {
    const services = yield* Effect.context<R>();
    const settled = yield* Effect.tryPromise({
        try: (signal): Promise<unknown> => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(body(context), { signal }), { commandName }),
        catch: HostError.refused(commandName),
    });
    return yield* yield* Effect.orDie(Schema.decodeUnknownEffect(Schema.declare((value: unknown): value is Exit.Exit<A, E> => Exit.isExit(value)))(settled));
});
```

`core.executeAsModal` is Photoshop's modal scope, resolving with what its callback resolves, and `HostError.refused` builds the package error naming a refused host call. The body runs inside the modal with the caller's services, an interrupted `modal` aborts it through `signal`, and `yield* yield*` raises the body's typed error, defect, or interruption in the calling fiber as the body raised it:
- Programs fold expected failures into the host value, and the `Exit` of `runPromiseExit` then fails on a defect or interruption alone
- Callbacks a host reads no value from run `Effect.runFork(Effect.matchCause(effect, handlers))`, each outcome one host call (`next`, `end`)
- `runPromise` rejects with `Cause.squash`: the first typed error, else the first defect, else an `Error` naming the interruption
- Hosts reading their own error type from a rejection receive it from one `Effect.mapError` before the run
- Steps mapping every error to a protocol error (`Server.orInternalError`) follow `Effect.catchNoSuchElement`, keeping absence out of `E`
- `Cause.prettyErrors(cause)` renders a finished call's failures and defects as one `Error` each, an interruption alone as one `InterruptError`
- Per-reason host values (a badge, a status) are one `as const` table, `reason` reading its keys through `Schema.Literals(Struct.keys(table))`

## [05]-[DEFECTS]

Defects are failures no caller handles (a broken invariant, a host call throwing what its contract excludes, a throw in caller-supplied code). A boundary around one unit (a table row, a job, a plugin, an RPC call) ends the unit, records its cause, and keeps every other unit running. Each row of a compiled policy table becomes a fault value when it throws, dies, or fails:

```ts
const evaluate = (rule: string) => (verdicts: () => Verdicts<Verdict>): Effect.Effect<Tally, never, Requirements> =>
    Effect.suspend(() => {
        const given = verdicts();
        return Effect.isEffect(given) ? given : Effect.succeed(given);
    }).pipe(
        Effect.map((given) => tally.combineAll(given.map(lift))),
        Effect.catchCause((cause) => Effect.succeed({ ...tally.initialValue, faults: [`${rule}: failed and judged nothing: ${message(cause)}`] })),
    );
```

`verdicts` returns a row's verdicts or an effect of them, `lift` turns a verdict into a partial `Tally`, `tally` is the `Struct.makeReducer` folding them, and `message` joins the messages of `Cause.prettyErrors`. A synchronous throw dies inside `Effect.suspend`, and every other row still decides:
- Interruption of the boundary's own fiber skips every catch, while one read from joined work (`Fiber.join`) reaches `catchCause` as any cause
- `Cause.hasInterruptsOnly(cause) ? Effect.interrupt : fault` passes a joined interruption on, `E` typed `never` where `catchCauseIf` keeps `E`
- `Effect.catchDefect(MediaError.of('Unreadable', name))` turns a defect of caller-supplied code into its item's package error
- `Effect.logError(message, cause)` records each `Cause` argument as the log record's cause in place of printing it in the message
- Steps no later step reads (a finalizer, a cleanup) discard through `Effect.ignore({ log, message })`, a cause holding a defect still failing
- `Effect.ignoreCause({ log, message })` also discards defects and joined interruptions, for a handler a loop runs again under `Effect.forever`
