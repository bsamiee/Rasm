# [COMPOSITION_AND_EFFECT_ORCHESTRATION]

[VERIFIED_SYMBOLS]:
- Exist: `pipe(value, *fns)` (eager, value-first), `pipe2` / `pipe3` / `starpipe` (tuple variants), `PipeMixin.pipe` (method form on `Result` / `Option` / `Seq` / `Block` / `Map`), `compose(*fns)` / `starcompose` (deferred factory; zero args is identity), `curry(N)` / `curry_flip(N)` for N in 0..4, `flip` / `fst` / `snd` / `identity`; rail combinators `result.map/bind/map_error/map2/or_else/or_else_with/filter/filter_with/swap/merge` and `option.map/bind/map2/starmap/or_else` (all `@curry_flip(1)`, source-last, pipeline-native); `effect.result` / `effect.option` / `effect.try_` / `effect.seq` builders; `extra.result.pipeline` and `extra.option.pipeline` (kleisli); `extra.result.catch`; `extra.result.traverse` / `sequence`; `Block` / `Seq` / `Map`. [expression 5.6.0 installed source, 2026-06-09]
- Do not exist: `flow`, a standalone `Pipe` (only `PipeMixin`), `result.pipeline` in core (it lives in `extra.result`), `option.catch`, `result.starmap`, a top-level `starpipe`; `curry_flipped` is a deprecated alias of `curry_flip`.

[EFFECT_BUILDER_REALITY]:
- `effect.result` is the `ResultBuilder` class; usage is `@effect.result[T, E]()` where the subscript is `__class_getitem__` and the call instantiates. Inside, `yield from` (not bare `yield`) unwraps via `Result.__iter__`, returning the `.ok` value or short-circuiting via `EffectError` on the first `Error`; bare `yield` sends the `Result` back as the next value and type-errors. [expression/effect/result.py 5.6.0, 2026-06-09]
- The decorated function's runtime return annotation stays whatever the generator body was annotated, so annotate the outer return as `Result[T, E]` explicitly or rely on `ResultBuilder.__call__`'s overload.

[CHOOSER]:
- `pipe(value, f, g)` for eager 2-9 typed steps; `compose(*fns)` to build, store, or fold a reusable transform; `curry_flip(N)` to make a domain function source-last and pipeline-native; `extra.result.pipeline(*fns)` (kleisli) for `A -> Result[B, E]` steps composed before a value (its zero-arg form seeds with `Ok`); `@effect.result[T, E]()` do-notation for three or more named intermediate values or non-adjacent dependencies.
- Threshold: one or two bind steps use `pipe(value, result.bind(f), result.map(g))`; three or more named steps use `@effect.result`.

[PARAMETERIZATION]:
- Fold a config-driven sequence of `Result -> Result` partials (each `result.map(f)` / `result.bind(f)` returns `Callable[[Result], Result]`) into one function via `compose(*transforms)`, replacing a hand-wired `pipe`. Keep one register per fold: `compose` over `Result -> Result` combinators, or `pipeline` over `A -> Result` kleisli steps — mixing them type-errors.

[DISPATCH_RAIL_SEAM]:
- A dispatch surface returns `Result`; downstream composes via `dispatch(...).map(f).bind(g)` (method), `pipe(dispatch(...), result.map(f), result.bind(g))`, or `pipeline(lambda v: dispatch(v), s2, s3)(value)`. `result.map_error` at the seam normalizes domain error types before a unified downstream rail.
- `traverse(fn, Block)` and `sequence(Block[Result])` lift a fallible `A -> Result[B, E]` across a collection (right-to-left `fold_back`; it short-circuits on the first error in left-to-right order while preserving prior successes).
