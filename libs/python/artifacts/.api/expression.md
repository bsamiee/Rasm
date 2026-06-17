# [PY_RUNTIME_API_EXPRESSION]

`Expression` supplies the railway-oriented programming core: `Result`/`Option` monads, tagged-union construction, computation-expression builders, currying/composition combinators, and immutable functional collections. It is the single ROP rail owner the branch composes; domain logic returns `Result`/`Option`, never raises.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Expression`
- package: `expression`
- import: `expression`
- version: `5.6.0`
- owner: `runtime`
- rail: rails
- namespaces: `expression`, `expression.collections`, `expression.core`, `expression.extra`, `expression.system`
- capability: `Result`/`Option` monads, tagged unions, computation-expression builders, function combinators, immutable collections

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result and option family
- rail: rails

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]          | [RAIL]                               |
| :-----: | :------------ | :--------------------- | :----------------------------------- |
|   [1]   | `Result`      | monad                  | success/error railway value          |
|   [2]   | `Ok`          | result case            | success carrier                      |
|   [3]   | `Error`       | result case            | failure carrier                      |
|   [4]   | `Option`      | monad                  | present/absent value                 |
|   [5]   | `Some`        | option case            | present value                        |
|   [6]   | `Nothing`     | option singleton       | absent value                         |
|   [7]   | `Try`         | alias                  | exception-captured result            |
|   [8]   | `Success`     | try case               | captured success                     |
|   [9]   | `Failure`     | try case               | captured exception                   |
|  [10]   | `Builder`     | computation expression | monadic do-notation base             |
|  [11]   | `EffectError` | fault                  | computation-expression short-circuit |

[PUBLIC_TYPE_SCOPE]: tagged-union and async family
- rail: rails

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                          |
| :-----: | :------------------ | :-------------- | :------------------------------ |
|   [1]   | `tagged_union`      | class decorator | discriminated-union declaration |
|   [2]   | `case`              | union field     | union-case marker               |
|   [3]   | `tag`               | union field     | discriminant marker             |
|   [4]   | `MailboxProcessor`  | actor           | message-loop agent              |
|   [5]   | `AsyncReplyChannel` | reply           | request/reply channel           |
|   [6]   | `TailCall`          | recursion       | trampolined tail-call marker    |

[PUBLIC_TYPE_SCOPE]: immutable collections family
- rail: rails

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------ | :------------ | :------------------------ |
|   [1]   | `collections.Block`             | collection    | persistent immutable list |
|   [2]   | `collections.Map`               | collection    | persistent ordered map    |
|   [3]   | `collections.asyncseq.AsyncSeq` | collection    | async lazy sequence       |
|   [4]   | `collections.Seq`               | collection    | lazy sequence combinators |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: result/option operations
- rail: rails

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :--------------------- | :------------- | :-------------------------- |
|   [1]   | `Result.map`           | functor        | map success value           |
|   [2]   | `Result.bind`          | monad          | chain result-returning step |
|   [3]   | `Result.map_error`     | functor        | transform failure value     |
|   [4]   | `Result.map2`          | applicative    | combine two results         |
|   [5]   | `Result.or_else`       | recovery       | fallback on error           |
|   [6]   | `Result.or_else_with`  | recovery       | lazy fallback on error      |
|   [7]   | `Result.default_value` | extract        | value-or-default            |
|   [8]   | `Result.default_with`  | extract        | value-or-lazy-default       |
|   [9]   | `Result.to_option`     | convert        | drop error to option        |
|  [10]   | `Result.of_option`     | convert        | lift option with error      |
|  [11]   | `Result.swap`          | invert         | swap ok/error sides         |
|  [12]   | `Result.filter`        | guard          | predicate-to-error          |
|  [13]   | `is_ok` / `is_error`   | predicate      | result discriminant         |
|  [14]   | `is_some` / `is_none`  | predicate      | option discriminant         |

[ENTRYPOINT_SCOPE]: combinators
- rail: rails

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------- | :------------- | :----------------------------- |
|   [1]   | `pipe`                      | composition    | left-to-right value pipeline   |
|   [2]   | `pipe2` / `pipe3`           | composition    | fixed-arity pipelines          |
|   [3]   | `compose`                   | composition    | right-to-left function compose |
|   [4]   | `curry` / `curry_flip`      | currying       | partial-application transform  |
|   [5]   | `flip`                      | argument       | swap two arguments             |
|   [6]   | `identity`                  | function       | identity combinator            |
|   [7]   | `fst` / `snd`               | tuple          | pair projection                |
|   [8]   | `default_arg`               | option         | option-or-default extract      |
|   [9]   | `failwith`                  | fault          | raise as effect error          |
|  [10]   | `tailrec` / `tailrec_async` | recursion      | trampoline decorator           |

## [4]-[IMPLEMENTATION_LAW]

[RAILS_TOPOLOGY]:
- result law: every fallible domain function returns `Result[T, BoundaryFault]`; success is `Ok`, failure is `Error`, and `BoundaryFault` is the single fault owner the branch carries.
- option law: optional values are `Option[T]` with `Some`/`Nothing`; `None`-returning helpers and sentinel propagation are deleted in favor of `Option`.
- chaining law: sequential fallible steps compose through `bind`; independent results combine through `map2`/applicative joins; `pipe` threads the value, never nested calls.
- union law: closed domain variants are `@tagged_union` classes with `case()` fields matched by `match`/`case`, never stringly-typed kind fields or parallel sibling classes.
- agent law: mailbox-style coordination uses `MailboxProcessor`/`AsyncReplyChannel`, never a hand-rolled queue actor.

[LOCAL_ADMISSION]:
- The boundary-conversion surface lifts ingress exceptions into `Error(BoundaryFault(...))` once at the edge; interior code never sees a raw exception.
- Recovery is `or_else_with`; default extraction is `default_with`; no try/except inside domain logic.
- Persistent collection accumulation uses `collections.Block`/`Map`, never mutable list/dict accumulation in a fold.

[RAIL_LAW]:
- Package: `expression`
- Owns: the Result/Option ROP rail, tagged-union construction, function combinators, and immutable collections
- Accept: `Result`/`Option` returns, `bind`/`map2` composition, `@tagged_union` variants, `pipe` pipelines
- Reject: exception-based domain control flow, sentinel/`None` propagation, stringly-typed dispatch, mutable accumulation
