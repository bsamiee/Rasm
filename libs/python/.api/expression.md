# [PY_BRANCH_API_EXPRESSION]

`Expression` supplies the functional core for Python: `Result[T, E]` and `Option[T]` monads with full functor/applicative/monad surfaces, `pipe`/`compose` pipelines, `curry`/`curry_flip`/`flip` higher-order combinators, `@tagged_union` discriminated unions with `case`/`tag` discriminants, trampolined tail recursion (`tailrec`/`tailrec_async`), generator-driven computation-expression effect builders (option/result/seq/try), persistent collections (`Block`/`Map`/`Seq`/`TypedArray`), and an actor `MailboxProcessor`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Expression`
- package: `Expression`
- module: `expression`
- version: `5.6.0`
- license: MIT
- asset: runtime library
- rail: functional-core
- namespaces: `expression` (re-exports `core`), `expression.core`, `expression.collections`, `expression.effect`, `expression.extra`, `expression.system`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core algebraic types
- rail: functional-core

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                                                  |
| ------- | ---------------- | ------------------ | ------------------------------------------------------- |
| [01]    | `Result`         | tagged union       | ok/error discriminant (`Result.Ok`/`Result.Error`)      |
| [02]    | `Ok`             | result constructor | successful value carrier                                |
| [03]    | `Error`          | result constructor | failure value carrier                                   |
| [04]    | `Option`         | tagged union       | some/none discriminant (`Option.Some`/`Option.Nothing`) |
| [05]    | `Some`           | option constructor | present value carrier                                   |
| [06]    | `Nothing`        | option singleton   | absent value sentinel                                   |
| [07]    | `Try`            | try alias          | `Result[T, Exception]` exception-backed result          |
| [08]    | `Success`        | try constructor    | success case of `Try`                                   |
| [09]    | `Failure`        | try constructor    | failure case of `Try`                                   |
| [10]    | `TailCall`       | trampoline carrier | pending tail call                                       |
| [11]    | `TailCallResult` | trampoline alias   | `T TailCall` (done or pending)                          |
| [12]    | `EffectError`    | effect failure     | short-circuit signal inside an effect block             |
| [13]    | `Builder`        | computation expr   | base for generator-driven effect builders               |

[PUBLIC_TYPE_SCOPE]: typeclass / capability protocols
- rail: functional-core
- structural protocols used as bounds across the combinators; satisfy them to compose with `pipe`, `sort`, and `sum`.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [RAIL]                                   |
| ------- | ------------------------------------------ | ------------- | ---------------------------------------- |
| [01]    | `PipeMixin`                                | mixin         | endows `.pipe(*fns)` on any carrier      |
| [02]    | `SupportsMatch`                            | protocol      | structural-pattern-match capability      |
| [03]    | `SupportsLessThan` / `SupportsGreaterThan` | protocol      | total-order bound for `Block.sort`       |
| [04]    | `SupportsSum`                              | protocol      | additive bound for `Block.sum`/`Seq.sum` |

[PUBLIC_TYPE_SCOPE]: tagged-union discriminants
- rail: functional-core

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY]   | [RAIL]                                       |
| ------- | --------------------------------------------------------------------------- | --------------- | -------------------------------------------- |
| [01]    | `tagged_union(_cls=None, *, frozen=False, repr=True, eq=True, order=False)` | union decorator | discriminated-union class decorator          |
| [02]    | `tag()`                                                                     | tag field       | declare the union's discriminant tag field   |
| [03]    | `case()`                                                                    | case field      | declare one union case (payload-typed field) |

[PUBLIC_TYPE_SCOPE]: effect builders (`expression.effect`)
- rail: functional-core

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                                      |
| ------- | --------------- | ---------------- | ------------------------------------------- |
| [01]    | `effect.option` | builder instance | option computation block (`@effect.option`) |
| [02]    | `effect.result` | builder instance | result computation block (`@effect.result`) |
| [03]    | `effect.seq`    | builder instance | lazy seq computation block                  |
| [04]    | `effect.try_`   | builder instance | exception-trapping computation block        |

[PUBLIC_TYPE_SCOPE]: persistent collections (`expression.collections`)
- rail: functional-core
- immutable, structurally-shared; every update returns a new structure, the original unchanged.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [RAIL]                                  |
| ------- | ------------ | ----------------- | --------------------------------------- |
| [01]    | `Block`      | persistent vector | immutable ordered sequence (F# `list`)  |
| [02]    | `Map`        | persistent map    | immutable ordered key→value AVL map     |
| [03]    | `Seq`        | lazy sequence     | composable lazy iterator pipeline       |
| [04]    | `TypedArray` | typed array       | homogeneous fixed-dtype immutable array |

[PUBLIC_TYPE_SCOPE]: mailbox / actor (`expression.core`)
- rail: functional-core

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                     |
| ------- | ------------------- | ------------- | ------------------------------------------ |
| [01]    | `MailboxProcessor`  | actor class   | async message loop with a serialized inbox |
| [02]    | `AsyncReplyChannel` | reply handle  | typed actor reply delivery                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipe, compose, and currying
- rail: functional-core

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]   | [RAIL]                                                    |
| ------- | ---------------------------------------------------------------------- | ---------------- | --------------------------------------------------------- |
| [01]    | `pipe(value, *fns)`                                                    | linear pipeline  | thread a value left-to-right through unary fns            |
| [02]    | `pipe2(values, /, *fns)` / `pipe3(values, /, *fns)`                    | tuple pipeline   | two/three-argument threaded pipeline                      |
| [03]    | `compose(*fns)`                                                        | composition      | left-to-right function composition into one callable      |
| [04]    | `curry(num_args)` / `curry_flip(num_args)` / `curry_flipped(num_args)` | curry decorator  | auto-curry N-arity fn, optionally flipping argument order |
| [05]    | `flip(fn)`                                                             | combinator       | swap the first two arguments of a binary fn               |
| [06]    | `identity(value)`                                                      | identity fn      | passthrough                                               |
| [07]    | `fst(pair)` / `snd(pair)`                                              | tuple projection | first / second element                                    |
| [08]    | `upcast(value)` / `downcast(value)` / `try_downcast(type_, value)`     | cast helpers     | up/down-cast; `try_downcast` returns `T \| None`          |

[ENTRYPOINT_SCOPE]: option operations (`expression.option` + `Option` methods)
- rail: functional-core

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                           |
| ------- | ------------------------------------------------------------------------------------------ | -------------- | ------------------------------------------------ |
| [01]    | `option.map(mapper)` / `Option.map(mapper)`                                                | functor map    | transform present value                          |
| [02]    | `option.bind(mapper)` / `Option.bind(mapper)`                                              | monadic bind   | flat-map over option                             |
| [03]    | `Option.map2(other, mapper)` / `Option.starmap(mapper)`                                    | applicative    | combine two options / spread a tuple value       |
| [04]    | `option.of_optional(value)` / `Option.of_obj(value)`                                       | constructor    | lift nullable / any object to option             |
| [05]    | `Option.to_optional()` / `Option.to_list()` / `Option.to_seq()`                            | projection     | lower option to nullable/list/seq                |
| [06]    | `Option.of_result(result)` / `Option.to_result(error)` / `Option.to_result_with(error_fn)` | conversion     | bridge `Option`↔`Result`                         |
| [07]    | `Option.default_value(value)` / `Option.default_with(fn)`                                  | fold           | extract with eager / lazy fallback               |
| [08]    | `Option.filter(predicate)`                                                                 | refinement     | demote `Some` to `Nothing` on a failed predicate |
| [09]    | `is_some(option)` / `is_none(option)` / `Option.is_some()` / `Option.is_none()`            | type guard     | narrow Some / Nothing branch                     |
| [10]    | `Option.or_else(if_none)` / `Option.or_else_with(fn)`                                      | fallback       | substitute an alternative option                 |

[ENTRYPOINT_SCOPE]: result operations (`expression.result` + `Result` methods)
- rail: functional-core

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]  | [RAIL]                                                                                                                                                                                                                  |
| ------- | ------------------------------------------------------------------------------------ | --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [01]    | `result.map(mapper)` / `Result.map(mapper)`                                          | functor map     | transform ok value                                                                                                                                                                                                      |
| [02]    | `result.bind(mapper)` / `Result.bind(mapper)`                                        | monadic bind    | flat-map over result                                                                                                                                                                                                    |
| [03]    | `Result.map2(other, mapper)`                                                         | applicative     | combine two oks, short-circuit on first error                                                                                                                                                                           |
| [04]    | `result.map_error(mapper)` / `Result.map_error(mapper)`                              | error map       | transform the error branch                                                                                                                                                                                              |
| [05]    | `Result.default_value(value)` / `Result.default_with(fn)`                            | fold            | extract ok with eager / lazy fallback                                                                                                                                                                                   |
| [06]    | `Result.of_option(error)` / `Result.of_option_with(error_fn)` / `Result.to_option()` | conversion      | bridge `Result`↔`Option`                                                                                                                                                                                                |
| [07]    | `Result.filter(predicate, error)` / `Result.filter_with(predicate, error_fn)`        | refinement      | demote `Ok` to `Error` on a failed predicate                                                                                                                                                                            |
| [08]    | `Result.swap()` / `Result.merge()`                                                   | inversion       | flip ok/error branches / collapse to one type                                                                                                                                                                           |
| [09]    | `is_ok(result)` / `is_error(result)` / `Result.is_ok()` / `Result.is_error()`        | type guard      | narrow Ok / Error branch                                                                                                                                                                                                |
| [10]    | `Result.or_else(other)` / `Result.or_else_with(fn)`                                  | fallback        | substitute on error                                                                                                                                                                                                     |
| [11]    | `extra.result.traverse(fn, lst)`                                                     | ROP sequencing  | `(T -> Result[R, E]) × Block[T] -> Result[Block[R], E]`; short-circuits on the first `Error` — the admitted gate fold over a row set (runtime `execution/recipe#RECIPE` `_staged` engine gate the live consuming fence) |
| [12]    | `extra.result.sequence(lst)`                                                         | ROP sequencing  | `Block[Result[T, E]] -> Result[Block[T], E]`; the identity-`fn` `traverse`                                                                                                                                              |
| [13]    | `extra.result.pipeline(*fns)` / `extra.result.catch(exception)`                      | ROP composition | Kleisli composition of `T -> Result` fns / decorator trapping one exception class into `Error`                                                                                                                          |

[ENTRYPOINT_SCOPE]: persistent collection operations
- rail: functional-core
- `Block`/`Map`/`Seq` from `expression.collections`; module-level callables (`block.map`, `seq.filter`, `map.add`) compose under `pipe`; methods mirror them fluently.

| [INDEX] | [SURFACE]                                                                                                                            | [ENTRY_FAMILY]      | [RAIL]                                         |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------ | ------------------- | ---------------------------------------------- |
| [01]    | `Block.of_seq(xs)` / `Block.empty()` / `Block.singleton(x)` / `Block.range(...)`                                                     | constructor         | build a `Block`                                |
| [02]    | `Block.cons(x)` / `Block.append(other)` / `Block.tail()` / `Block.head()` / `Block.try_head()`                                       | structural          | prepend / concat / decompose                   |
| [03]    | `Block.map(f)` / `Block.mapi(f)` / `Block.choose(f)` / `Block.collect(f)` / `Block.filter(p)` / `Block.partition(p)`                 | transform           | map/filter-map/flat-map/split                  |
| [04]    | `Block.fold(folder, state)` / `Block.reduce(reducer)` / `Block.sum()` / `Block.sum_by(f)` / `Block.unfold(gen, seed)`                | reduce              | left fold / reduce / generate                  |
| [05]    | `Block.sort()` / `Block.sort_with(key)` / `Block.zip(other)` / `Block.take(n)` / `Block.skip(n)`                                     | sequence ops        | ordering / pairing / slicing                   |
| [06]    | `Map.of_seq(pairs)` / `Map.of_list(pairs)` / `Map.of_block(block)` / `Map.empty()`                                                   | constructor         | build a `Map`                                  |
| [07]    | `Map.add(k, v)` / `Map.remove(k)` / `Map.change(k, fn)` / `key in map` / `map[key]`                                                  | mutate (persistent) | insert / delete / update / membership / lookup |
| [08]    | `Map.try_find(k)` / `Map.try_get_value(k)` / `Map.get(k)` / `Map.contains_key(k)`                                                    | lookup              | `Option`-returning and raising lookups         |
| [09]    | `Map.map(proj)` / `Map.filter(p)` / `Map.fold(f, s)` / `Map.partition(p)` / `Map.keys()` / `Map.values()` / `Map.items()`            | transform           | project / filter / fold / iterate              |
| [10]    | `Seq.of_iterable(xs)` / `Seq.delay(thunk)` / `Seq.map(f)` / `Seq.filter(p)` / `Seq.collect(f)` / `Seq.scan(f, s)` / `Seq.fold(f, s)` | lazy ops            | lazily-evaluated streaming pipeline            |

[ENTRYPOINT_SCOPE]: tail recursion and effect short-circuit
- rail: functional-core

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]   | [RAIL]                                               |
| ------- | ----------------------------------- | ---------------- | ---------------------------------------------------- |
| [01]    | `tailrec(fn)`                       | trampoline wrap  | stack-safe tail recursion                            |
| [02]    | `tailrec_async(fn)`                 | async trampoline | async stack-safe tail recursion                      |
| [03]    | `failwith(message)`                 | raise helper     | raise `EffectError` to short-circuit an effect block |
| [04]    | `default_arg(value, default_value)` | fold             | unwrap an `Option` with a default                    |

## [04]-[IMPLEMENTATION_LAW]

[EXPRESSION_TOPOLOGY]:
- modules: `expression.core` (monads, combinators, builders, mailbox — re-exported at top level), `expression.collections` (`Block`/`Map`/`Seq`/`TypedArray` + matching module-level callables), `expression.effect`, `expression.extra` (`extra.result`: the `traverse`/`sequence`/`pipeline`/`catch` Result-sequencing combinators — never hand-roll a short-circuiting fold over a `Block` of `Result`s), `expression.system`.
- `Result[T, E]` and `Option[T]` are `@tagged_union` classes; build with `Ok(v)`/`Error(e)`/`Some(v)`/`Nothing`, decompose with structural `match` on the case, and chain with the `map`/`bind`/`map2`/`filter` instance methods or the curried `expression.result`/`expression.option` module callables (designed for `pipe`).
- `@tagged_union` plus `tag()`/`case()` declares an exhaustively-matchable discriminated union; `frozen=True` makes instances hashable, `order=True` derives comparison. This is the canonical owner for any bounded variant set — never parallel boolean flags.
- `pipe(value, *fns)` threads a value through unary callables; the module-level collection/monad callables are curried so `pipe(xs, block.map(f), block.filter(p), block.fold(g, 0))` reads as a point-free pipeline. `compose(*fns)` builds the reusable callable; `flip`/`curry_flip` reorder arguments to fit pipeline position.
- effect builders (`@effect.option`/`@effect.result`/`@effect.try_`) use the generator-coroutine protocol: `yield from m` binds the inner value, an absent/error case raises `EffectError` and short-circuits the whole block to `Nothing`/`Error`. Use them when bind chains exceed ~three levels; prefer explicit `bind`/`map2` otherwise.
- `tailrec`/`tailrec_async` trampoline: the function returns `TailCall(*args)` to recurse without growing the stack, or a plain value to complete.
- `MailboxProcessor.start(body)` runs an actor over a serialized inbox; `post`/`post_and_async_reply` enqueue, and `AsyncReplyChannel` delivers a typed reply.

[STACKS_WITH]:
- `beartype` (`.api/beartype.md`): at a boundary adapter, catch `BeartypeCallHintViolation` (or use `BeartypeConf(violation_type=...)`) and lift onto `Result.Error`; `door.is_bearable(value, hint)` returns a `TypeIs` guard that narrows before an `Ok(value)`. `door.is_subhint` validates a `@tagged_union` registry at startup.
- `msgspec`/`pydantic` (`.api/msgspec.md`, `.api/pydantic.md`): a decode is the boundary that mints a `Result` — wrap `msgspec.json.decode(buf, type=T)` / a pydantic `model_validate` in `effect.try_` so `msgspec.ValidationError`/`pydantic.ValidationError` becomes `Result.Error`. A `@tagged_union` discriminant maps onto a `msgspec.Struct`-tagged union or a pydantic discriminated union at the wire edge while domain code stays `Result`/`Option`-native.
- `anyio` (`.api/anyio.md`): an `anyio.to_thread.run_sync`/`run_process` offload returns a value the caller lifts into `Result`; a `TimeoutError` from `fail_after` or a `BrokenWorkerProcess` is caught at the boundary and mapped to a typed `Error`, never escaping as a raw exception.
- `structlog` (`.api/structlog.md`): fold a `Result.Error` payload into a single structured log event at the egress boundary (`log.error(event, error=err)`); the happy `Ok` path stays silent. Never `raise`-and-log inside a `bind` chain.

[LOCAL_ADMISSION]:
- Use `pipe`/`compose` as the primary composition surface; avoid intermediate variables for sequential transforms.
- Use `option.of_optional`/`Option.to_optional` at nullable boundaries; keep domain code `Option`-native.
- Use `Result.map`/`bind`/`map2`/`filter` chains instead of imperative try/except in domain transforms; let `effect.try_` be the single try-trapping boundary.
- Use `@tagged_union` for every bounded variant set; never model variants with parallel classes, string flags, or `Optional` fields.
- Use `Block`/`Map`/`Seq` for persistent domain state; never mutate a `list`/`dict` accumulator in a domain transform.

[RAIL_LAW]:
- Package: `Expression`
- Owns: `Result`/`Option`/`Try` monads, `pipe`/`compose`, currying + `flip`, `@tagged_union` discriminated unions, trampolined tail recursion, effect builders, persistent `Block`/`Map`/`Seq`/`TypedArray`, the actor `MailboxProcessor`
- Accept: `pipe`/`compose`, `Result`/`Option`/`Try`, `@tagged_union`+`tag`/`case`, `tailrec`/`tailrec_async`, `effect.*` builders, persistent collections, the typeclass protocols as bounds
- Reject: hand-rolled monad patterns, `returns`-library types, nested if-chains for absent/error paths, recursive functions without a trampoline, mutable list/dict accumulation in domain transforms
