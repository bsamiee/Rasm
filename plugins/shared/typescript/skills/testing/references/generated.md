# [GENERATED]

`effect/Arbitrary` derives generators from schemas and runs checks that shrink and replay their failures, `@effect/vitest` registers each check as a test, and `TestSchema.Asserts` runs codec round trips over schema-derived values.

## [01]-[CHECKS]

`it.prop` and `it.effect.prop` take a title, inputs, a body, and Vitest options holding `arbitrary: CheckOptions`:

```ts
const Truth = {
    conditions: Schema.Array(Condition).check(Schema.isMinLength(1), Schema.isMaxLength(10)),
    reads: Schema.NonEmptyArray(Read).check(Schema.isMinLength(14), Schema.isMaxLength(24)),
    owner: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 999 })),
    draft: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 999 })),
};

it.effect.prop(
    'stores no document name or user name',
    Truth,
    Effect.fnUntraced(function* (truth) {
        const world = app(truth);
        const reads = yield* Effect.forEach(world.readings, (read) => Schema.decodeEffect(Observation)(world.observation(read)));
        const last = reads.reduce((map, read) => map.fold(read, world.user)[1], InterfaceMap.empty);
        const written = yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown))(last);
        assert.notInclude(written, world.user.username);
    }),
    { arbitrary: { seed: 2026, runs: 300 } },
);
```

`interface-map.test.ts` declares `Condition`, `Read`, and `app` beside the test, and each input key reaches the body as one field:
- Inputs are a tuple or record of schemas and arbitraries, joined by `Arbitrary.all` at registration, so an underivable schema fails its file
- `it.prop` bodies run synchronously and return `boolean` or `void`, a `false` or a throw falsifying, and any other value throws a `TypeError` naming `it.effect.prop`
- `it.effect.prop` bodies falsify on `false`, a typed failure, a throw, or a defect such as a failed `assert`, each kept as its `Cause`, and an interruption interrupts the test
- Bodies asserting through `assert` return `void`, as both testers pass every value except `false`
- Evaluations each run in their own scope, and plain `it.effect.prop` provides test services to each evaluation
- Evaluations inside a `layer(L)` block share one build of `L`, and `{ buildPerTest: true }` builds `L` per evaluation in a fork of its parent's memo map
- Failures reach Vitest as a defect holding the formatted result, and a timeout interrupts every phase except a synchronous body that never returns

ClickHouse and Postgres suites start one container per block and create one database per evaluation through `outer.layer(Server.database, { buildPerTest: true })`, so evaluations hold no shared rows.

`CheckOptions` settles each run, every option read once per check:

| [INDEX] | [OPTION]      | [DEFAULT]             | [EFFECT]                                              |
| :-----: | :------------ | :-------------------- | :---------------------------------------------------- |
|  [01]   | `runs`        | 100                   | Passing evaluations a check requires                  |
|  [02]   | `size`        | 10                    | Complexity scale the last run reaches                 |
|  [03]   | `maxDiscards` | `max(100, 10 × runs)` | Rejected draws before `Exhausted`                     |
|  [04]   | `maxShrinks`  | 100                   | Shrink candidates inspected after a first failure     |
|  [05]   | `seed`        | Drawn from `Random`   | Master seed of every attempt                          |
|  [06]   | `concurrency` | 1                     | Generated cases pending evaluation at once            |
|  [07]   | `replay`      | None                  | One recorded falsification, every other option unread |

- `Arbitrary.configureGlobal({ check, sample })` replaces these defaults for each later check in its process, `replay` excepted
- `concurrency` evaluates later cases ahead and interrupts them before a sequential shrink, so deterministic properties keep one verdict and token
- Database suites pass `concurrency: 4` through `sampled(store, runs)`, as each evaluation owns its database
- Properties return one outcome per input and mutate no input, as shrinks and replays evaluate it again with no clone or service reset
- `Arbitrary.checkEffect` outside a test returns a `CheckResult`, passes on `true` alone, so a body returning `void` falsifies, and lets defects and interruptions through

## [02]-[GENERATORS]

Members of `effect/Arbitrary` compose generators, each carrying its shrinker:

| [INDEX] | [MEMBER]                                | [DRAWS]                                    | [SHRINKS]                              |
| :-----: | :-------------------------------------- | :----------------------------------------- | :------------------------------------- |
|  [01]   | `schema(S)`, `schema(S, { shrink })`    | Decoded `Type` of `S`, every check applied | Schema-derived, or `shrink` tree whole |
|  [02]   | `Constant(value)`                       | One value, never cloned                    | None                                   |
|  [03]   | `array(item, { minLength, maxLength })` | Length growing with size, inside bounds    | Removed blocks, then each element      |
|  [04]   | `all(tuple)`, `all(record)`             | Value of its input's shape                 | One member at a time                   |
|  [05]   | `map(f)`                                | Transformed value                          | Source candidates, mapped              |
|  [06]   | `filter(p)`, `filterMap(f)`             | Accepted value, rejects discarded          | Accepted candidates alone              |
|  [07]   | `flatMap(f)`                            | Generator a drawn value selects            | Source value, then dependent value     |
|  [08]   | `sampleEffect(a, options)`              | `count` values, 10 by default              | None, no property runs                 |

Schemas state a domain once for the code under test and its generator:
- Checks carrying a generation constraint (`isBetween`, `isInt`, `isMinLength`, `isMaxLength`, `isPattern`) build values inside their range, and every other check (`Schema.makeFilter`, `isMultipleOf`) filters draws
- ClickHouse `Bounds` refines a struct of literal bounds through `makeFilter`, so draws with unordered bounds or no common integer type discard
- `Arbitrary.filter` holds a condition no schema check states, `filterMap` transforms and rejects in one step, and each rejected root spends `maxDiscards`
- Narrowed ranges state their reason in `annotate({ description })`, as code files hold no comments
- Union members take equal shares of draws, uniform inside a nested `Literals`, so a `Schema.Literals(HOSTILE)` member holds a stated share (`Step` in `object-model/host.test.ts`)
- `Arbitrary.flatMap` callbacks are synchronous and deterministic, run again on shrink and replay, so a schema derived inside one derives again per call
- `Arbitrary.schema` throws when it cannot compile a schema or find a finite route through a recursive one, and `array` throws `RangeError` on invalid bounds

Draws favor limits, early attempts most:
- Lengths land on a bound and integers near zero or a bound in 1 draw of `2 + floor(log10(attempt + 1))`
- Strings draw at that rate from a hostile set (`''`, `'\0'`, `'__proto__'`, `'constructor'`, lone surrogates, `'😀'`) fitting their length bounds
- Records take a null prototype every `4 × (2 + floor(log10(attempt + 1)))` attempts, and `structuredClone` mapped over a draw gives each record `Object.prototype` (`Plain` in `host.test.ts`)
- Domains refusing a hostile value filter it at its generator, as `construction.test.ts` drops `'\0'` and ill-formed strings from `unit`
- `size` climbs from 0 to its setting across runs, each unconstrained string, array, and record reading it apart, and stated minimums hold at 0

## [03]-[PATTERNS]

`Schema.isPattern` and checks built on it (`isAscii`, `isUUID`, `isULID`, `isStartingWith`, `isEndingWith`, `isIncluding`) give a string its pattern, which the generator compiles into a constructive generator, then tests each value against the `RegExp`:

| [INDEX] | [SYNTAX]                                                                        | [GENERATION] |
| :-----: | :------------------------------------------------------------------------------ | :----------- |
|  [01]   | Literals, `.`, classes and ranges, `\d`, `\w`, `\s` and their negations         | Constructive |
|  [02]   | `\n`, `\t`, `\xHH`, `\uHHHH`, `\u{H}`, `\cX`                                    | Constructive |
|  [03]   | `*`, `+`, `?`, `{m}`, `{m,}`, `{m,n}`, lazy forms                               | Constructive |
|  [04]   | `(...)`, `(?:...)`, `(?<name>...)`, `\|`, `^`, `$`                              | Constructive |
|  [05]   | Flag `i`, `m`, or `v`                                                           | Filter alone |
|  [06]   | `\p{...}`, `\P{...}`, `\k<name>`, `\1`, lookaround, `\b` and `\B` outside class | Filter alone |

- Patterns without `^` or flag `y` take printable ASCII before their match, and patterns without `$` after it
- Patterns without flag `u` draw code points below `0x10000` alone
- Strings holding more than one pattern build from a random one and filter through the rest (`key` of `Step` in `object-model/host.test.ts`)
- String lengths reach at most `max(minLength, shortest match, size)` under `maxLength`, so a match longer than `size` never draws until `size` covers it
- Filter-only patterns discard until `maxDiscards`, and `Exhausted` names each as `UnsupportedPattern` with its path from the schema root

## [04]-[SHRINKING]

Shrinking starts at a first failure and inspects up to `maxShrinks` candidates, keeping each smaller input that fails with the same class, `false` or a failed effect:
- Candidates a check, `filter`, or dependent generator rejects spend budget unevaluated, as do candidates failing with the other class
- Searches return the best input found inside their budget, never a proven minimum
- Candidates pass every schema check and filter, so each shrunk input is one the generator can draw
- Arrays drop prefixes and interior blocks inside their length bounds, `all` shrinks one member at a time, and `flatMap` its source first
- Union values from a costlier member try a draw of the cheapest member first
- Typed errors can change while shrinking, as the runner compares failure classes alone
- `Arbitrary.schema(S, { shrink })` replaces derived shrinking with a synchronous, deterministic callback whose candidates the schema validates

## [05]-[REPLAY]

Failures print the shrunk input, its failure, and a replay token:

```text
Property falsified after <runs> run(s) and <shrinks> shrink(s)
Shrunk input: <value>
Failure: <returned false | Cause>
Replay: [<0 or 1>,"<seed>",<attempt>,<size>,[<shrink path>],"<ReturnedFalse | PropertyError>"]
```

`{ arbitrary: { replay: '<token>' } }` regenerates the failing attempt at its recorded size, then walks its shrink path, reading no other option and reporting one run:
- Replays report `Property replay failed: <reason>`, reason `PropertyPassed`, `ShrinkPassed`, `AttemptDiscarded`, or `ShrinkPathUnavailable`, when the failure no longer recurs
- Tests holding a token fail before their fix as falsified and after it as `PropertyPassed`, so a token stays out of every commit
- Tokens address attempts and shrink steps of one arbitrary, so a changed generator or Effect release reads another input
- Checks with no `seed` draw a master seed from `Random`, which `Math.random` backs, so each run draws new cases and its token records the seed
- Checks with `seed` draw the same cases on every run (`interface-map.test.ts` holds `seed: 2026`)
- Exhausted checks print `Seed: <seed>` and each unsupported pattern, and that `seed` reproduces the exhausted run

Counterexamples become named example rows:
1. Add `replay: '<token>'` to the failing test's `arbitrary` options, run it alone (`nx test <project> -- -t '<title>'`), and read its shrunk input
2. Write a named `it.effect` or an `it.effect.each` row holding the shrunk input as literals, and run it red before any fix
3. Remove `replay`, and raise the counterexample's class in draws through a `Literals` union member, a bound, or a pattern
4. Fix code until named test and property check both pass

## [06]-[CODECS]

`new TestSchema.Asserts(S)` of `effect/testing` checks a schema's codec over values `Arbitrary.schema(S)` draws, each method taking `CheckOptions`:

| [INDEX] | [METHOD]                                | [RETURNS]              | [CHECKS]                                  | [RUNS] |
| :-----: | :-------------------------------------- | :--------------------- | :---------------------------------------- | :----: |
|  [01]   | `verifyRoundTrip(options)`              | `Promise<void>`        | Decoded encoding equals input by `S`      |  100   |
|  [02]   | `verifyRoundTripEffect(options)`        | Effect, codec services | As `verifyRoundTrip`, in its fiber        |  100   |
|  [03]   | `arbitrary().verifyGeneration(options)` | `void`, synchronous    | `Schema.is(S)` holds for each drawn value |   20   |

- Failing encodes and decodes falsify a round trip as `returned false` and shrink, while schema defects and interruptions pass through unshrunk
- Round trips compare through `Schema.toEquivalence(S)`, so a schema declaring an equivalence annotation compares by it
- Failures throw from `node:assert` with the formatted result, its shrunk input and replay token included
