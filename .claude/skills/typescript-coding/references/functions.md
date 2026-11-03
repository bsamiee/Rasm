# [FUNCTIONS]

## [01]-[PIPELINES]

Data-last package steps take their element type from the value before them. Reusable pipelines over a collection type their subject and pass it to `pipe`, `flow` composes steps that type their own input (`String.trim`, a declared function):

```ts
const podium = (entries: readonly Entry[], places: number) =>
    pipe(
        entries,
        Array.groupBy((entry) => entry.player),
        Record.map(Array.max(Entry.byScore)),
        Record.values,
        Array.sort(Order.flip(Entry.byScore)),
        Array.take(places),
    );
```

`Entry.byScore` orders entries by score. `Array.groupBy` returns non-empty groups, and `Array.max` picks each player's best entry with no absent case. `podium` returns best entries of the top `places` players:
- Package members and applied data-last forms (`String.includes(term)`) enter as steps by reference, a lambda reads a field or joins arguments
- Nested data-first module calls become `pipe` steps
- Steps reading no input pass the effect value to `Effect.tap` or `Effect.andThen`, a `() => effect` thunk restating it

## [02]-[DUAL_SIGNATURES]

`Function.dual` builds one function dispatching on its data-first parameter count: a call holding the subject runs the body data-first, and a call without it returns a data-last step:

```ts
const hedged = Function.dual<(after: Duration.Input) => <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>, <A, E, R>(self: Effect.Effect<A, E, R>, after: Duration.Input) => Effect.Effect<A, E, R>>(
    2,
    (self, after) => Effect.race(self, Effect.delay(self, after)),
);
```

`hedged(request, p95)` and `request.pipe(hedged(p95))` run one body, racing the request against a copy started after `p95`, the caller's observed latency percentile, and keeping the first success:
- Type arguments state both signatures once, generic signatures included, and body parameters take their types from the data-first one
- Trailing optional parameters make argument count ambiguous, and a predicate on the arguments (`(args) => Effect.isEffect(args[0])`) selects the form
- Callbacks passing an element index take the applied form (`hedged(p95)`) or a lambda, an unapplied function receives the index as its next parameter
- Exported functions consumers call both directly and in a pipeline are duals, a module-private step stays data-last

## [03]-[PARTIAL_APPLICATION]

Parameters order for partial application: dependencies known at the composition root, then policies selecting behavior, then the runtime value. Each stage runs as its arguments arrive, with work reading fixed arguments in the outer stage and per-value work alone in the returned function:

```ts
const matching = (query: string, locale?: string) =>
    pipe(
        query,
        String.toLocaleLowerCase(locale),
        (lowered) => new Intl.Segmenter(locale, { granularity: 'word' }).segment(lowered),
        Array.filter((word) => word.isWordLike === true),
        Array.map(({ segment }) => String.includes(segment)),
        Predicate.every,
        Predicate.mapInput(String.toLocaleLowerCase(locale)),
    );
```

`matching(query)` lowercases the query once, keeps the word segments `isWordLike` marks at the locale's word boundaries, and returns a `Predicate<string>`. Each tested text lowercases once before every term checks it, and `titles.filter(matching(query))` keeps titles holding every term:
- Failures fixed arguments decide return at their application as `Option` or `Result`, and the returned function stays total
- Package builders enter as one `(builder: B) => B` parameter defaulting to `Function.identity`, an options record mirroring their setters restating it

## [04]-[FILTERS]

`Filter<In, Pass, Fail>` is a function returning `Result<Pass, Fail>`, narrowing and mapping its input in one call and failing with the typed remainder. `Array.filterMap`, `Array.partition`, `Record.filterMap`, `Option.filterMap`, `Stream.filterMap`, `Stream.partition`, `Effect.filterMapOrFail`, and `Effect.catchFilter` take one, and package constructors build it from the tag, refinement, or lookup stating the rule:

```ts
const matched = (invoices: HashMap.HashMap<string, Invoice>) =>
    Filter.compose(
        Filter.tagged<typeof Change.Type>()('Paid'),
        Filter.fromPredicateOption(({ reference }) => HashMap.get(invoices, reference)),
    );
```

`Change` is a `Schema.TaggedUnion` with a `Paid` case holding `amount` and `reference` and an empty `Voided` case. `Array.partition(changes, matched(open))` returns the invoice each payment matches beside every change matching none, typed as a `Voided` change or a `Paid` one with an unknown reference:
- `Filter.fromPredicate` builds a filter from a refinement, `Filter.fromPredicateOption` from a lookup returning `Option`, failing with its input
- `Filter.compose` feeds one filter's pass to the next, `Filter.or` runs the second on the input the first rejects, and `Filter.zip` pairs both passes
- Schema `guards` take `unknown` and leave the remainder `unknown`, `Filter.tagged<A>()` fixes the input type
- `Effect.catchFilter` without a fallback replaces `E` with the remainder, and `Effect.filterMapOrFail` builds its error from the remainder
- `Array.flatMap` callbacks returning `[x]` or `[]` restate `Array.filterMap`

## [05]-[DISPATCH_TABLES]

Dispatch tables select behaviors sharing a signature by a decoded key. Modules or objects holding the rows are the table, and the key schema lists the member names it accepts:

```ts
const Casing = Schema.Literals(['camelCase', 'constantCase', 'kebabCase', 'pascalCase', 'snakeCase'] satisfies readonly (keyof typeof String)[]);
```

`String[casing]` types as the union of the accepted members, each `(self: string) => string`, and `Array.map(rows, Record.mapKeys(String[casing]))` renames every row's keys through the member a decoded `Casing` selects. `satisfies` checks each name against the module, and a new casing is one name in the list:
- Rows hold a function or module where behavior differs by case, rows of data one shared function reads are a lookup table
- Rows no module holds are an `as const` object keyed by the encoded token, with typed handles as values
- `satisfies Record<Extract<keyof typeof Enum, Uppercase<string>>, A>` requires a row per host enum member, minus reverse-mapped numeric keys
- Open host unions key an optional mapped table (`{ readonly [M in Method]?: Handler }`), each read falling back through `UndefinedOr.match`
- Row contracts are an interface and tables `as const satisfies Record.ReadonlyRecord<string, Contract>`, keeping literal keys and typing row lambdas
- Rows spread one default row, a member some rows lack holding a neutral function there (`() => Effect.succeed(true)`) in place of an `Option<fn>`
- Rows with a fixed outcome hold the value, the reader lifting a non-function row through `Predicate.isFunction`, a constant thunk restating it
- Ordered rules over tested columns are rows of `Predicate` columns (`Function.constTrue` a wildcard), `Array.findFirst` selecting the first match

## [06]-[EFFECTFUL_FUNCTIONS]

`Effect.fn('<name>')` declares an effectful function and runs each call's body with its trailing steps inside one span and stack frame named for it. Trailing steps receive the body's effect and the caller's arguments without body parameter defaults, and hold each step reading an argument or wrapping a generator body:

```ts
const archive = Effect.fn('archive')(
    function* (source: URL, target: string) {
        const [fs, response] = yield* Effect.all([FileSystem.FileSystem, HttpClient.get(source)]);
        yield* Stream.run(response.stream, fs.sink(target));
        return yield* fs.stat(target);
    },
    (effect, source) => Effect.andThen(Effect.annotateCurrentSpan({ source: source.href }), effect),
);
```

`archive` streams `source` into `target` and returns the written file's `File.Info`, with a span recording each source. `HttpClientError` and `PlatformError` reach the caller unchanged, and the effect requires `HttpClient` and `FileSystem`:
- Span names follow the binding, `<Class>.<member>` for a class field and `<Service>.<key>` for a service member
- Attributes every call shares sit in `Effect.fn('<name>', { attributes })`
- Trailing steps return an `Effect`, and a step returning any other value (a `Stream`) leaves the call with no span or stack frame
- `Effect.fnUntraced` declares a generator function a traced one calls, with no stack frame or span, in place of a function returning `Effect.gen`
- `Effect.fn.Return<A, E, R>` annotates the return of a generic or recursive generator body
- `Effect.fn('<name>')({ self: this }, body)` and `Effect.gen({ self: this }, body)` bind `this` inside a class member
