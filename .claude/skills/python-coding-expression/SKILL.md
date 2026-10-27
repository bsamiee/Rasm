---
name: python-coding-expression
description: "Use when calling an expression member, covering result and option members, persistent collections, curried steps, and codec boundaries"
---

# [PYTHON_CODING_EXPRESSION]

Covers expression members past python-coding's result chain, from result and option members by step shape to persistent collections, curried steps, `catch` typing, and codec boundaries. Snippets build on python-coding's `Fault`, `Faults`, `Outcome`, and `Reason`.

`catch`, `pipeline`, `traverse`, and `sequence` import from `expression.extra.result`, the option `pipeline` from `expression.extra.option`, and `Block`, `Map`, and `Seq` with their curried modules from `expression.collections`.

## [01]-[RESULTS]

Members past the core chain select by what a step holds:

| [INDEX] | [MEMBER]           | [SIGNATURE]                                   | [BEHAVIOR]                                 |
| :-----: | :----------------- | :-------------------------------------------- | :----------------------------------------- |
|  [01]   | `filter(p, error)` | `(T -> bool, F) -> Result[T, E \| F]`         | Fails with a fixed error, widens its type  |
|  [02]   | `or_else(other)`   | `Result[T, E] -> Result[T, E]`                | Takes an alternative built before the test |
|  [03]   | `default_with(f)`  | `(E -> T) -> T`                               | Computes a host value from the error       |
|  [04]   | `merge()`          | `Result[T, T] -> T`                           | Returns the value either case holds        |
|  [05]   | `map2(other, f)`   | `(Result[U, E], (T, U) -> R) -> Result[R, E]` | Pairs results, left error first            |
|  [06]   | `apply(held)`      | `Result[T -> R, E] -> Result[R, E]`           | Applies a held function, its error first   |

Folds over results seed through a typed static constructor: `Ok(state)` types as `Result[S, Any]`, and `Result[S, Faults].Ok(state)` keeps the fault side. `applied` runs dependent steps in order, each reading the state its predecessor returned, and stops at the first fault:

```python
def applied[S](state: S, steps: Iterable[Callable[[S], Outcome[S]]], /) -> Outcome[S]:
    return reduce(lambda held, step: held.bind(step), steps, Result[S, Faults].Ok(state))
```

`applied(state, ())` returns `Ok(state)`. Results hold by these rules:
- `.ok` and `.error` type as `T` and `E` on every result and raise `AttributeError` on the inactive case, and checkers accept an unguarded read
- Results hash their payload, a frozen record holding `Ok([...])` fails `hash`, and `copy.replace` on a result raises `TypeError`
- `Try`, `Success`, and `Failure` fix the error side to `Exception`, and `catch` names the class a contract raises

## [02]-[OPTIONS]

Options narrow host values at a boundary: `try_downcast` returns `T | None` from an `isinstance` test, and `Option.of_optional` lifts it into the chain. `held` turns a value of the wrong class into a fault naming the expected and found classes:

```python
def held[T](kind: type[T], subject: str, value: object, /) -> Outcome[T]:
    return Option.of_optional(try_downcast(kind, value)).to_result_with(
        lambda: (Fault(reason=Reason.OUTSIDE, subject=subject, member=kind.__name__, cause=type(value).__name__),)
    )
```

`held(int, "count", "3")` reads `count accepts int, not str`. Options hold by these rules:
- `try_downcast` tests the origin class alone, `try_downcast(list[int], ["x"])` returns `["x"]`, and element types stay unchecked
- `downcast` checks through an `assert` that `python -O` strips
- `Option.of_optional` maps `None` alone to `Nothing`, `0`, `""`, and `()` stay `Some`, and `Some(None)` is a `Some`
- `Nothing` is one shared value and `Option.Nothing()` builds an equal one, `is Nothing` misses it and `is_none()` reads both
- `map2` takes the mapper first on `Option` and second on `Result` (`low.map2(range, high)`)
- `default_with` and `or_else_with` take no argument on `Option` and the error on `Result`
- `starmap(f)` unpacks a tuple payload into `f`'s parameters with each element typed
- `.value` raises `ValueError` on `Nothing`, and `to_list()` returns a mutable `list`

## [03]-[COLLECTIONS]

Collections are persistent values, and each operation returns a new one sharing structure with its source:

| [INDEX] | [TYPE]      | [HOLDS]                    | [SERVES]                                          |
| :-----: | :---------- | :------------------------- | :------------------------------------------------ |
|  [01]   | `Block[T]`  | Tuple-backed items, eager  | Pipelines whose steps return `Option` or `Result` |
|  [02]   | `Map[K, V]` | AVL tree ordered by key    | State a fold threads, every version kept          |
|  [03]   | `Seq[T]`    | Lazy view over an iterable | Deferred `map`, `filter`, `choose`, and `scan`    |

### [03.1]-[BLOCK]

`Block.unfold` walks a tree with no recursion and no mutation: its state is the pending frontier, and each step emits the head and pushes the head's children in front. `preorder` returns every node depth first in child order:

```python
def preorder[T](seed: T, children: Callable[[T], Iterable[T]], /) -> Block[T]:
    frontier: tuple[T, ...] = (seed,)
    return Block.unfold(lambda pending: Some((pending[0], (*children(pending[0]), *pending[1:]))) if pending else Nothing, frontier)
```

Blocks hold by these rules:
- `choose(f)` maps and keeps each `Some` in one pass, `collect(f)` concatenates produced blocks, and `partition(p)` splits by a predicate
- `fold(f, seed)` returns its seed for no items, `head()` and `reduce` raise `ValueError`, and `try_head()` returns an `Option`
- `Block` is a `Collection` and no `Sequence`, parameters reading one take `Collection` or `Iterable`, and `case [head, *rest]` never matches it
- `case Block([head, *rest])` binds `rest` as a `list` under both checkers
- `Block` equals a `tuple` with the same items, defines no `__hash__`, and msgspec encodes none, so record fields hold a `tuple`
- `zip` truncates to the shorter block, and `take(n)` past its end returns every item
- `traverse` and `sequence` take a `Block`, and a `list` runs and fails both checkers

### [03.2]-[MAP]

`change(key, f)` passes the held `Option[V]` to `f`, sets the key on `Some`, and removes it on `Nothing`. `accumulate` over `change` keeps every version, and versions share each subtree an edit leaves:

```python
def versions[K: SupportsLessThanAndHash, V](start: Map[K, V], edits: Iterable[tuple[K, Option[V]]], /) -> tuple[Map[K, V], ...]:
    return tuple(accumulate(edits, lambda table, edit: table.change(edit[0], lambda _: edit[1]), initial=start))
```

`versions(Map.of(a=1), (("b", Some(2)), ("a", Nothing)))` returns the start and one map per edit, each still valid. Maps hold by these rules:
- Keys bound `SupportsLessThanAndHash` from `expression.core.typing`, mixed key types raise `TypeError` at insertion, and iteration sorts keys
- `try_find(key)` returns an `Option`, `m[key]` raises `KeyError`, and `Map` has no `find`
- `add` replaces a held key, and `remove` of an absent key returns the map unchanged
- `map`, `filter`, and `try_pick` take key and value as separate parameters, and `fold` takes its state and one `(key, value)` tuple
- `Map` hashes and compares by entries, `Map == dict` is `False`, and an empty map is falsy

### [03.3]-[SEQ]

`Seq` defers each step until iteration and reads its source on every pass:
- `Seq` over a generator reads it once, and a second pass resumes where the first stopped
- Sources a pipeline reads twice enter as `Block.of_seq(source)` or as `Seq.of_iterable` over a re-iterable value
- `scan` emits its seed first, and `head()` raises `ValueError` on no items

## [04]-[COMPOSITION]

Curried and composed forms each fit one step shape:

| [INDEX] | [SHAPE]                          | [MEMBER]                                        | [LIMIT]                       |
| :-----: | :------------------------------- | :---------------------------------------------- | :---------------------------- |
|  [01]   | Result steps as one function     | `pipeline(f, g)` from `expression.extra.result` | 6 steps                       |
|  [02]   | Option steps as one function     | `pipeline(f, g)` from `expression.extra.option` | 6 steps                       |
|  [03]   | Subject-first function in `pipe` | `pipeable(f, *rest)`                            | Binds every argument after it |
|  [04]   | Subject-last declaration         | `@curry_flip(n)`                                | `n` from 0 to 4               |

- `pipeline(f, g)(x)` equals `f(x).bind(g)`, and `pipeline()` returns `Ok` or `Some` of its argument
- `pipe(value, pipeable(clamp, low, high))` keeps the return type of `clamp`, and `@curry_flip(1)` types `scaled(2.0)` as `(float) -> float`
- `curry(n)` and `curry_flip(n)` pass keyword arguments after the curried positionals
- Module functions in `pipe` (`result.map(f)`) lose types to `Unknown` under ty and to `_TSource` under mypy, and fluent chains keep them
- `fst`, `snd`, `flip`, and `identity` cover pair reads, swapped parameters, and the identity step

`catch` typing decides what it wraps:
- `exception=` types one class bounded by `Exception`, a tuple fails both checkers, and `KeyboardInterrupt` fails both while catching at runtime
- Stacked `@catch` decorators fail mypy and pass ty
- Functions returning a `Result` type as `Result[Result[T, F], E]` through `catch`, and mypy refuses its decorator form over them
- `@catch` over a method converts each call and keeps `__name__` and `__wrapped__`

## [05]-[BOUNDARIES]

Codecs and validators read expression types as dataclasses, and records crossing them hold builtin shapes:
- Encoders in msgspec write `Option` and `Result` as their active case (`{"tag":"none","none":null}`) and calls no `enc_hook` on them
- `Decoder`, `convert`, and `inspect` over a type holding `Option[...]` raise `TypeError: 'Option' is not a generic type` at type analysis
- `Result[T, E]` decodes as a record requiring both `ok` and `error`, and msgspec encodes no `Block` or `Map` without an `enc_hook`
- Frozen records holding `Option` and `Result` construct, hash, compare, `structs.replace`, and `to_builtins` in process
- Pydantic fields typed `Option[T]` or `Result[T, E]` fail validation (`dataclass_type`) and dumps (`AttributeError`), model fields hold `T | None`
- `Option.dict()` returns the value or `None`, and `Result.dict()` returns the tag object
