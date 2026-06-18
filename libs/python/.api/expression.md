# [PY_BRANCH_API_EXPRESSION]

`Expression` supplies functional programming primitives for Python: `Result[T, E]` and `Option[T]` monadic types, `pipe`/`compose` pipelines, `curry`/`curry_flip` higher-order functions, `tagged_union` discriminated unions, tail-call-safe recursion via `tailrec`/`tailrec_async`, and computation expression builders (`Builder`) for option, result, and seq effect blocks.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Expression`
- package: `Expression`
- module: `expression`
- asset: runtime library
- rail: functional-core

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core algebraic types
- rail: functional-core

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                        |
| :-----: | :--------------- | :----------------- | :---------------------------- |
|   [1]   | `Result`         | tagged union       | ok/error discriminant         |
|   [2]   | `Ok`             | result constructor | successful value carrier      |
|   [3]   | `Error`          | result constructor | failure value carrier         |
|   [4]   | `Option`         | tagged union       | some/none discriminant        |
|   [5]   | `Some`           | option constructor | present value carrier         |
|   [6]   | `Nothing`        | option singleton   | absent value sentinel         |
|   [7]   | `Try`            | try alias          | exception-backed result       |
|   [8]   | `Success`        | try constructor    | success case of Try           |
|   [9]   | `Failure`        | try constructor    | failure case of Try           |
|  [10]   | `TailCall`       | trampoline carrier | pending tail call             |
|  [11]   | `TailCallResult` | trampoline alias   | done or pending tail call     |
|  [12]   | `EffectError`    | effect failure     | short-circuit in effect block |
|  [13]   | `Builder`        | computation expr   | effect block builder base     |

[PUBLIC_TYPE_SCOPE]: effect builders
- rail: functional-core

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                   |
| :-----: | :-------------- | :--------------- | :----------------------- |
|   [1]   | `effect.option` | builder instance | option computation block |
|   [2]   | `effect.result` | builder instance | result computation block |
|   [3]   | `effect.seq`    | builder instance | seq computation block    |
|   [4]   | `effect.try_`   | builder instance | try computation block    |

[PUBLIC_TYPE_SCOPE]: mailbox / actor
- rail: functional-core

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]               |
| :-----: | :------------------ | :------------ | :------------------- |
|   [1]   | `MailboxProcessor`  | actor class   | async message loop   |
|   [2]   | `AsyncReplyChannel` | reply handle  | actor reply delivery |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipe and compose
- rail: functional-core

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :----------------------- | :-------------- | :---------------------------------- |
|   [1]   | `pipe(value, *fns)`      | linear pipeline | thread value through functions      |
|   [2]   | `pipe2(values, /, *fns)` | tuple pipeline  | two-argument threaded pipeline      |
|   [3]   | `pipe3(values, /, *fns)` | tuple pipeline  | three-argument threaded pipeline    |
|   [4]   | `compose(*fns)`          | composition     | right-to-left function composition  |
|   [5]   | `curry(num_args)`        | curry decorator | auto-curry N-arity function         |
|   [6]   | `curry_flip(num_args)`   | curry decorator | curried with flipped argument order |

[ENTRYPOINT_SCOPE]: option operations
- rail: functional-core

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `option.map(option, mapper)`               | functor map    | transform present value       |
|   [2]   | `option.bind(option, mapper)`              | monadic bind   | flat-map over option          |
|   [3]   | `option.of_optional(value)`                | constructor    | lift nullable to option       |
|   [4]   | `option.to_optional(value)`                | projection     | lower option to nullable      |
|   [5]   | `option.default_arg(value, default_value)` | fold           | extract with fallback value   |
|   [6]   | `option.of_result(result)`                 | conversion     | strip error, keep ok as some  |
|   [7]   | `option.is_some(option)`                   | type guard     | narrow to Some branch         |
|   [8]   | `option.is_none(option)`                   | type guard     | narrow to Nothing branch      |
|   [9]   | `option.or_else(option, if_none)`          | fallback       | substitute alternative option |

[ENTRYPOINT_SCOPE]: result operations
- rail: functional-core

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :--------------------------------- | :------------- | :------------------------- |
|   [1]   | `result.map(result, mapper)`       | functor map    | transform ok value         |
|   [2]   | `result.bind(result, mapper)`      | monadic bind   | flat-map over result       |
|   [3]   | `result.map_error(result, mapper)` | error map      | transform error branch     |
|   [4]   | `result.is_ok(result)`             | type guard     | narrow to Ok branch        |
|   [5]   | `result.is_error(result)`          | type guard     | narrow to Error branch     |
|   [6]   | `result.of_option(value, error)`   | conversion     | lift option to result      |
|   [7]   | `result.or_else(result, other)`    | fallback       | substitute on error        |
|   [8]   | `result.swap(result)`              | inversion      | flip ok and error branches |

[ENTRYPOINT_SCOPE]: tail recursion and misc
- rail: functional-core

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]   | [RAIL]                          |
| :-----: | :---------------------------------- | :--------------- | :------------------------------ |
|   [1]   | `tailrec(fn)`                       | trampoline wrap  | stack-safe tail recursion       |
|   [2]   | `tailrec_async(fn)`                 | async trampoline | async stack-safe tail recursion |
|   [3]   | `tagged_union(_cls, *, frozen)`     | union decorator  | discriminated union class       |
|   [4]   | `failwith(message)`                 | raise helper     | raise EffectError with message  |
|   [5]   | `identity(value)`                   | identity fn      | passthrough function            |
|   [6]   | `default_arg(value, default_value)` | fold             | unwrap option with default      |
|   [7]   | `fst(pair)` / `snd(pair)`           | tuple projection | first / second element          |
|   [8]   | `upcast(value)` / `downcast(value)` | cast helpers     | up/down-cast expression helpers |

## [4]-[IMPLEMENTATION_LAW]

[EXPRESSION_TOPOLOGY]:
- modules: `expression.core`, `expression.collections`, `expression.effect`, `expression.extra`, `expression.system`
- `Result[T, E]` and `Option[T]` are tagged unions decorated with `@tagged_union`
- `pipe` threads a value left-to-right through an arbitrary number of unary callables
- effect blocks (`effect.option`, `effect.result`) use generator coroutine protocol; `yield` binds inner values, raising `EffectError` on nothing/error to short-circuit
- `tailrec`/`tailrec_async` use the trampoline pattern: function returns `TailCall` for continuation or a plain value for completion

[LOCAL_ADMISSION]:
- Use `pipe` as the primary composition surface; avoid intermediate variables for sequential transforms.
- Use `option.of_optional` / `option.to_optional` at nullable boundaries; keep domain code `Option`-native.
- Use `result.map` / `result.bind` chains instead of imperative try/except in domain transforms.
- Use effect blocks only when bind chains span more than three levels; prefer explicit `bind` otherwise.

[RAIL_LAW]:
- Package: `Expression`
- Owns: Result/Option monads, pipe/compose, curry, tagged unions, tail recursion, effect builders
- Accept: `pipe`, `compose`, `Result`, `Option`, `tagged_union`, `tailrec`, effect blocks
- Reject: hand-rolled monad patterns, nested if-chains for absent/error paths, recursive functions without trampoline
