# [RUNTIME_MECHANICS_AND_COMPOSITION_SEAMS]

[TAGGED_UNION_RUNTIME_IDENTITY_HAZARDS]:
- `frozen=True` guards only declared case-field names in `__setattr__`; arbitrary non-case attributes (`obj.metadata = 42`) still pass to `object.__setattr__`, so a frozen `@tagged_union` is a value-object only when no extra attribute is ever set and the active case value is itself immutable.
- The generated `__hash__` over `(cls.__name__, tag, value)` does not constrain the case value: a frozen union holding a `list`/`dict` constructs cleanly but raises `TypeError` the moment it is used as a `dict` key, `set` member, `heapq` element, or `frozenset` entry — failure is deferred to first hash, not construction.
- `copy.copy` reconstructs via `cls(**{tag: value})`, sharing a mutable case value between original and copy; `copy.deepcopy` deep-copies the active value before reconstruction. Because `frozen=True` blocks reassignment but not in-place mutation, `original.leaf.append(x)` mutates the shared shallow copy and the original together.

[SERIALIZATION_GAPS]:
- `dataclasses.asdict()` emits only the active case (`{'circle': 5.0}`), never the tag: the per-instance `__dataclass_fields__` override holds the active case descriptor and `tag` is set outside the dataclass field machinery. Tag-carrying serialization must be explicit — `{'tag': obj.tag, obj.tag: getattr(obj, obj.tag)}`, the shape `Result.dict()` produces.
- `Result`'s Pydantic schema is broken on JSON ingress in expression 5.6.0: the before-validator calls `cls(data)` positionally against a keyword-only `__init__`, so `model_validate_json` on any `Result`-typed field raises `TypeError: __init__() takes 1 positional argument but 2 were given`. Python-object validation and `model_dump_json` both work; the boundary codec must decode JSON to a dict and call `model_validate(dict)` instead of `model_validate_json`.

[EFFECT_BUILDER_COMPOSITION_MECHANICS]:
- `combine(xs, ys)` is `xs.bind(lambda _: ys)`, which is how `@effect.result()` chains successive `yield from` statements while discarding intermediate Ok values and short-circuiting on the first `Error`; side-effecting dispatch arms (`yield from dispatch(Req(persist=...))`) compose inside the do-block without binding a return.
- `@effect.option()` short-circuits on `Nothing` via `EffectError` raised in `Option.__iter__`, carrying no error payload, whereas `@effect.result()` short-circuits on `Error` carrying the error value; the two builders are not cross-rail composable — `yield from Some(...)` and `yield from Ok(...)` cannot share one do-block.

[KLEISLI_PIPELINE_VS_DO_NOTATION]:
- `option.pipeline(*fns)` seeds with `Some` and `result.pipeline(*fns)` seeds with `Ok`, so an empty pipeline is the lift, not the identity — the first function receives a bare `T` (not a rail) and the zero-arg form equals `Some`/`Ok`. Both are point-free Kleisli `>=>` chains of unary rail-returning functions; reach for `@effect.result()` do-notation instead only when an intermediate binding must be named.

[TRAVERSE_AND_SEQUENCE_AS_FOLD]:
- `traverse(fn, lst)` right-folds `fn: T -> Result[R, E]` over a `Block[T]` into `Result[Block[R], E]` (via an `@effect.result()` folder), short-circuiting on first `Error` — the rail-preserving map that validates and transforms a collection without leaving `Result`.
- `sequence(lst)` is `traverse(identity, lst)`: it collapses a `Block[Result[T, E]]` into `Result[Block[T], E]`, the fan-in aggregator for N independent dispatch calls that each return a rail.

[TAILREC_STACK_SAFE_ADT_FOLD]:
- `tailrec` trampolines a function returning `T | TailCall[P]`: a `TailCall(*args)` re-enters without growing the Python stack, making it the recursion-limit-safe driver for ADT folds (`@tagged_union` trees, `Block` chains); `tailrec_async` is the `async def` form for recursive handlers.
- It drives linear recursion only; tree/DAG traversal threads an explicit pending-node `list` as a `tailrec` argument — pop one node per bounce, append children — for stack-safe DFS, with `@tagged_union` nodes serving directly as stack elements.

[ACTOR_MODEL_DISPATCH_SURFACE]:
- `MailboxProcessor[Msg]` over a `@tagged_union` `Msg` is a single-writer dispatch surface whose handler is one `while True: match await mbox.receive()` total fold: `post(Msg(put=...))` is fire-and-forget, `await post_and_async_reply(lambda ch: Msg(get=(k, ch)))` is request-reply, and a `AsyncReplyChannel[Reply]` embedded in a case field is the typed reply port.
- `post_and_async_reply` returns an `Awaitable[Reply]` resolved only when the matched arm calls `channel.reply(value)`; a reply-tagged message whose arm falls through leaves the caller's awaitable permanently unresolved, so every reply-bearing tag needs an unconditional arm.

[PARSER_COMBINATOR_DISPATCH]:
- `Parser.bind(f)` is context-sensitive dispatch: the recognized token selects the next parser, so `tag_p.bind(lambda t: payload_for(t).map(lambda p: (t, p)))` routes each tag to a distinct payload grammar and yields a typed `(tag, payload)` inside `Result[A, str]`.
- `or_else(p2)` backtracks — on `p1` failure it retries `p2` at the same input position, not the post-`p1` remainder — so `choice(Block([...]))` (a `reduce` of `or_else`) is first-match-wins alternation over competing parsers at one position.

[COMBINATOR_SEED_AND_FLIP_CONTRACTS]:
- `@curry_flip(N)` curries the N trailing parameters as the partial config and leaves the leading source parameter for last, giving `pipe(data, f(config))` directly; `@curry(N)` curries the leading N parameters, so its source must be threaded as the final nested call instead — `curry_flip` is the pipe-native form, `curry` the application-order form.
- `starcompose(*fns)` requires a tuple seed: it unpacks each stage's tuple output into the next via `f(*fields)` and raises `TypeError: Value after * must be an iterable` on a scalar seed, unlike `compose`, which threads any value — `starcompose` is the multi-arity fold for tuple-returning stages.

[RAIL_SEAM_CONVERTERS]:
- `Result.of_option(opt, err)` / `of_option_with(opt, err_fn)` lift `Option` into `Result` (`Nothing -> Error(err)`, lazily for the `_with` form), and `to_option()` drops back, discarding the error — the seam between absence-only and fallible arms in one dispatch graph.
- `swap()` inverts the rails (`Ok(v) <-> Error(v)`), turning an error case into the success signal for idempotent arms; `merge()` (on `Result[T, T]`) returns the inner value regardless of tag when both rails carry a uniform type.
- `map2(other, mapper)` is the applicative combine: `mapper(a, b)` only when both are `Ok`, left-biased on `Error`, joining two already-computed dispatch results without the sequential dependency a `bind` imposes.
