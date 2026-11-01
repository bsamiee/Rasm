# [RESULTS]

Covers outcome flows, from absence and validation to recovery, host translation, defects, and union folds.

## [01]-[ABSENCE]

`Option<A>` holds a value that can be absent with no error to report, and every reader maps over it (`Contact` decodes `email` and `phone` as `Option<string>`):

```ts
const reachable = (contacts: ReadonlyArray<Contact>) => Array.findFirst(contacts, (contact) => Option.orElse(contact.email, () => contact.phone));
```

- `Array.findFirst` with a function returning `Option` finds and maps in one pass, `Option.orElse` picks the first present source
- `Array.get`, `Record.get`, and `Option.flatMapNullishOr` read an index, a key, or a nullable host field as an `Option`
- `Array.filterMap` and `Stream.filterMap` keep what a step answers with `Result.succeed`, `Result.fromOption` adapts a step answering `Option`

## [02]-[VALIDATION]

Schema checks hold rules over one value's fields. Rules relating a value to data it lacks (a stock table, the current date) are validators returning `Result`, with the package error class as `E`:

```ts
const remaining = (stock: HashMap.HashMap<Sku, number>) => (line: Line) =>
    pipe(
        HashMap.get(stock, line.sku),
        Result.fromOption(() => OrderError.make({ reason: 'missing', subject: line.sku })),
        Result.filterOrFail((held) => held >= line.quantity, () => OrderError.make({ reason: 'rejected', subject: line.sku })),
        Result.map((held) => held - line.quantity),
    );

const reservable = (stock: HashMap.HashMap<Sku, number>) => (lines: ReadonlyArray<Line>) =>
    pipe(Array.partition(lines, remaining(stock)), ([held, failed]) => Array.match(failed, { onEmpty: () => Result.succeed(held), onNonEmpty: Result.fail }));
```

`remaining` stops at its first failed step, and `reservable` runs `remaining` over every line and fails with a `NonEmptyArray` of errors, the failure `Effect.validate` gives an effectful validator, so a caller repairs every line at once:
- `Result.liftPredicate` turns a predicate and an error constructor into a validator that returns its input
- `Option.all` and `Result.all` combine independent values into a tuple or struct and stop at the first absent or failed one, `Option.gen` and `Result.gen` bind steps that read earlier values
- `SchemaIssue.makeFormatterStandardSchemaV1()` flattens a `SchemaError` issue into `{ message, path }` entries a form shows per field

## [03]-[RECOVERY]

Consumers recover where they hold a response to a failure, and every other failure passes on unchanged (`Inventory.reserve` fails with `OrderError` and answers a hold with `complete`, `Inventory.refresh` is an effect):

```ts
const reserved = (order: Order) =>
    Inventory.reserve(order.lines).pipe(
        Effect.catchIf((error) => error.reason === 'stale', () => Effect.andThen(Inventory.refresh, Inventory.reserve(order.lines))),
        Effect.filterOrFail((held) => held.complete, () => OrderError.make({ reason: 'rejected', subject: order.id })),
    );
```

- Error instances are effects failing with themselves, `yield* OrderError.make(...)` and a handler returning one raise it
- `Effect.catchTag` takes one tag or a non-empty list of tags, `Effect.catchTags` takes a handler per tag
- `Effect.catchTag`, `Effect.catchTags`, and `Effect.catchIf` take a final handler for every other error, one step covers both outcomes
- `Effect.catchIf` with a refinement removes the matched errors from `E`, a predicate over `reason` keeps the class in `E`
- `Effect.mapError` builds the package error around each failure of a foreign step (a decode, a client call) as its `cause`
- `Effect.catchReason` recovers one tagged `reason` of a host error (`HttpClientError`, `PlatformError`) and keeps the parent for the rest
- Typed recovery runs on a cause free of defects, a cause holding a defect passes each typed error on as a defect

## [04]-[HOST_TRANSLATION]

Hosts that render an outcome fold the expected failures inside the program and run it to an `Exit`, which then fails on a defect or an interruption alone and renders the host's unavailable state (`runtime` is a `ManagedRuntime`, `Orders.place` fails with `OrderError`):

```ts
export const onSubmit = (order: Order) =>
    runtime
        .runPromiseExit(Effect.match(Orders.place(order), { onSuccess: (receipt) => receipt.summary, onFailure: (error) => error.message }))
        .then(Exit.getSuccess)
        .then(Option.getOrElse(() => 'unavailable'));
```

`runPromiseExit` resolves with every outcome, `runPromise` rejects with one squashed `unknown`.

## [05]-[DEFECTS]

Defects are failures no caller handles (a broken invariant, a programmer error, a host call throwing what its contract excludes). Each ends its fiber and reaches the boundary isolating a plugin or a job, which logs the cause and keeps its supervisor running:

```ts
const isolated = <A, E, R>(job: Effect.Effect<A, E, R>) =>
    job.pipe(
        Effect.asSome,
        Effect.catchCause((cause) => (Cause.hasInterruptsOnly(cause) ? Effect.interrupt : Effect.as(Effect.logError(cause), Option.none()))),
    );
```

- `Cause.hasInterruptsOnly` separates cancellation from failure, an interrupted job stays interrupted and logs nothing
- `Effect.catchDefect` stands at a boundary that recovers defects alone and keeps typed errors for its caller
- `runMain` exits with 130 on interruption alone, otherwise with the squashed error's `[Runtime.errorExitCode]` member or 1
- Error classes derive `[Runtime.errorExitCode]` from `reason` in a getter, `[Runtime.errorReported]` as `false` skips the `runMain` log

## [06]-[UNION_FOLDS]

Recursive unions fold with one function per case, and each case function folds its children through the same function. Cases that differ only in the operation they apply are one case keyed by a table of functions:

```ts
const operators = { add: Number.sum, multiply: Number.multiply } as const;

type Expr = Data.TaggedEnum<{
    Num: { readonly value: number };
    Apply: { readonly operator: keyof typeof operators; readonly left: Expr; readonly right: Expr };
}>;

const Expr = Data.taggedEnum<Expr>();

const evaluate: (expr: Expr) => number = Expr.$match({
    Num: ({ value }) => value,
    Apply: ({ operator, left, right }) => operators[operator](evaluate(left), evaluate(right)),
});
```

- New operators are one row of `operators`, and every fold and constructor reads the row through `keyof typeof operators`
- Each further operation over the union is one more `$match`, and the union declaration stays unchanged
