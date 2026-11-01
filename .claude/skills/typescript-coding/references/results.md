# [RESULTS]

Covers typed failure, absence, and recovery flows.

## [01]-[ABSENCE]

Reads return `Option` at their source. Effectful reads where each step reads the value the previous step found chain through `Effect.fromOption`, which fails an absent step with `Cause.NoSuchElementError`, and one `Effect.catchNoSuchElement` at the end returns the chain as an `Option` while every other error stays in `E`. `Customers.find` reads a `Contact` holding `manager` with optional `email` and `phone` as an `Option` in an effect failing with `StoreError`:

```ts
const escalation = Effect.fn('escalation')(function* (id: CustomerId) {
    const customer = yield* Effect.fromOption(yield* Customers.find(id));
    const manager = yield* Effect.fromOption(yield* Customers.find(customer.manager));
    return yield* Effect.fromOption(Option.firstSomeOf([manager.email, manager.phone]));
}, Effect.catchNoSuchElement);
```

- `Effect.fromNullishOr` enters a nullable host value into the same chain
- `Option.firstSomeOf` returns the first present source, and a new source is one more element of its list
- `Array.findFirst` with a function returning `Option` finds and maps in one pass
- `Effect.transposeOption` runs an effect an `Option` holds and returns an effect of `Option`

## [02]-[VALIDATION]

Checks on a schema hold rules over one value's fields. Rules relating a value to data it lacks (a stock table, a current date) are validators `Result.liftPredicate` builds from a predicate and an error for each rejected value, and `Effect.validate` runs one over every element:

```ts
const reservable = (stock: HashMap.HashMap<Sku, number>) =>
    Effect.validate(
        flow(
            Result.liftPredicate(
                (line: Line) => Option.exists(HashMap.get(stock, line.sku), Number.isGreaterThanOrEqualTo(line.quantity)),
                (line) => OrderError.make({ reason: 'rejected', subject: line.sku }),
            ),
            Effect.fromResult,
        ),
    );
```

`reservable(stock)(lines)` returns the lines or fails with a `NonEmptyArray` holding an `OrderError` for each line its stock cannot cover, for a caller fixing every line at once. Independent rules over one value accumulate as `Effect.validate(rules, (rule) => Effect.fromResult(rule(value)))`.

## [03]-[RECOVERY]

Consumers recover from failures they hold a response to, and every other failure passes on in `E`. `read` fails with `OrderError` or `HttpClientError`, `cached` reads a stored copy, and `Inventory.refresh` reloads stock:

```ts
const resilient = <A, R>(read: Effect.Effect<A, OrderError | HttpClientError.HttpClientError, R>, cached: Effect.Effect<A>) =>
    read.pipe(
        Effect.catchReason('HttpClientError', 'TransportError', () => cached),
        Effect.catchIf(
            (error) => error.reason === 'stale',
            () => Effect.andThen(Inventory.refresh, read),
        ),
    );
```

- `Effect.catchReason` recovers one tagged reason of a host error (`HttpClientError`, `PlatformError`) and keeps the error's other reasons
- `Effect.catchIf` over `reason` keeps `OrderError` in `E`, and a refinement narrows `E` past what it matches
- `Effect.catchTag` takes one tag or a list of tags and `Effect.catchTags` a handler per tag, each removing what it handles from `E`
- `Effect.catchTag` and `Effect.catchIf` take a third handler for every error the tag or predicate passes over, and `E` holds only what the handlers raise
- Typed recovery skips a cause holding a defect and passes each typed error in it on as a defect

## [04]-[HOST_TRANSLATION]

Hosts translate an outcome once, at the call running it. Programs fold expected failures into a host value, and the `Exit` that `runPromiseExit` resolves with fails on a defect or an interruption alone. `runtime` is a `ManagedRuntime` and `Orders.place` takes a `Quote` and fails with `OrderError`, its reasons `missing`, `rejected`, and `stale`:

```ts
const respond = (quote: Quote) =>
    runtime.runPromiseExit(Effect.match(Orders.place(quote), { onSuccess: Response.json, onFailure: (error) => Response.json(error, { status: { missing: 404, rejected: 422, stale: 409 }[error.reason] }) })).then(Exit.match({ onSuccess: Function.identity, onFailure: () => new Response(null, { status: 503 }) }));
```

- Per-reason host values sit in a literal indexed by `reason`, and a reason the literal lacks fails to compile

## [05]-[DEFECTS]

Defects are failures no caller handles (a broken invariant, a host call throwing what its contract excludes). Each defect ends its fiber at a boundary isolating a plugin or a job, and the boundary logs its cause and keeps the supervisor running:

```ts
const isolated = <A, E, R>(job: Effect.Effect<A, E, R>) => Effect.catchCauseIf(Effect.asVoid(job), Predicate.not(Cause.hasInterruptsOnly), Effect.logError);
```

- `Predicate.not(Cause.hasInterruptsOnly)` leaves an interrupted job's cause an interruption and logs nothing
- `Effect.logError` attaches a cause argument to its log record
- `Effect.catchDefect` stands at a boundary recovering defects alone, and typed errors stay for its caller
