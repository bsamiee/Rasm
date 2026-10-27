---
name: python-coding-expression
description: "Use when calling an expression member, covering result and option members, nested contexts, persistent collections, curried steps, and codec boundaries"
---

# [PYTHON_CODING_EXPRESSION]

Covers expression members past python-coding's result chain. Snippets build on python-coding's `Fault`, `Faults`, `Outcome`, and `Reason`.

`traverse` and `sequence` import from `expression.extra.result`, and `Block`, `Map`, and `Seq` with their curried modules from `expression.collections`.

## [01]-[RESULTS]

`Result[S, Faults].Ok(state)` types the fault side where no declared return fixes it (a fold seed, an unannotated local):
- `.ok` and `.error` raise `AttributeError` on the other case, `.value` raises `ValueError` on `Nothing`, and checkers accept each unguarded read
- `await`, iteration, and `*` unpacking read the `Ok` or `Some` value and raise `EffectError` on `Error` or `Nothing`, and checkers accept each
- `MailboxProcessor` and `expression.core.aiotools` call asyncio directly
- Results order `Ok` below `Error` and options `Nothing` below `Some`, then by payload, and `max(options, default=Nothing)` reads the largest value
- Results and options hash by case and payload, and `hash(Ok([...]))` and `copy.replace` on a result raise `TypeError`
- `filter(predicate, error)` fails with a fixed error and widens to `Result[T, E | F]`, `or_else(other)` takes an alternative built before the test
- `Try`, `Success`, and `Failure` fix the error side to `Exception`

## [02]-[OPTIONS]

`try_downcast` narrows a host value through an `isinstance` test and returns `T | None`. `held` turns a value of the wrong class into a fault naming expected and found classes:

```python
def held[T](kind: type[T], subject: str, value: object, /) -> Outcome[T]:
    return Option.of_optional(try_downcast(kind, value)).to_result_with(lambda: Fault.of(Reason.OUTSIDE, subject, type(value).__name__, member=kind.__name__))
```

`held(int, "count", "3")` fails with `count accepts int, not str`. Chains over an `Option` narrow through `bind(lambda found: Option.of_optional(try_downcast(kind, found)))`, an `isinstance` filter keeps the wide type.

`Nothing` keys the absent case (roots, a default) beside every `Some` key with no branch for it. `places` maps each node to its parent as an `Option` and its index among siblings:

```python
class Tree[K: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    roots: tuple[K, ...]
    children: frozendict[K, tuple[K, ...]]

    def under(self, parent: Option[K], /) -> tuple[K, ...]:
        return parent.map(lambda key: self.children[key]).default_value(self.roots)

    @property
    def places(self) -> frozendict[K, tuple[Option[K], int]]:
        return frozendict({key: (parent, index) for parent in (Nothing, *map(Some, self.children)) for index, key in enumerate(self.under(parent))})
```

Comparing `places` entry by entry shows a node moved to the root as a parent change from `Some(<key>)` to `Nothing`:
- `try_downcast` tests the origin class alone, and `try_downcast(list[int], ["x"])` returns `["x"]`
- `downcast` checks through an `assert`
- `Option.of_optional` maps `None` alone to `Nothing`, and `Some(None)` is a `Some`
- `Nothing` is one shared value, and `Option.Nothing()` builds an equal one that `is Nothing` misses
- `default_with` and `or_else_with` take no argument on `Option` and the error on `Result`
- `Block.of_seq(options).choose(identity).try_head()` reads the first present option of any count, `or_else_with` chains a fixed count lazily
- `starmap(f)` unpacks a tuple payload into parameters of `f` with each element typed

## [03]-[NESTING]

Nested contexts hold outcomes a consumer handles apart:
- `Outcome[Option[T]]` separates a failed read from a read that found nothing
- `Option[Option[T]]` holds a `T | UnsetType | None` patch field: `UNSET` as `Nothing`, `None` as `Some(Nothing)`, a value as `Some(Some(value))`

`Nothing` leaves a field, `Some(Nothing)` clears it, and `Some(Some(value))` sets it. `Change` holds a node's parent edit, and `applied` resolves a wanted parent through a fallible lookup before its one write:

```python
class Change[K](msgspec.Struct, frozen=True, kw_only=True):
    key: K
    parent: Option[Option[K]] = Nothing


def resolved[K, N](lookup: Callable[[K], Outcome[N]], wanted: Option[K], /) -> Outcome[Option[N]]:
    return wanted.map(lambda key: lookup(key).map(Some)).default_value(Ok(Nothing))


def applied[K, N](lookup: Callable[[K], Outcome[N]], parented: Callable[[N, Option[N]], Outcome[None]], change: Change[K], /) -> Outcome[None]:
    return lookup(change.key).bind(lambda node: change.parent.map(lambda wanted: resolved(lookup, wanted).bind(partial(parented, node))).default_value(Ok(None)))
```

`Change(key=<key>)` writes nothing, `parent=Some(Nothing)` writes `Nothing`, and a wanted parent no node holds fails with the lookup's fault before any write. `resolved` turns `Option[Outcome[N]]` into `Outcome[Option[N]]` and keeps absence a success.

## [04]-[COLLECTIONS]

Collections are immutable, and each operation returns a new collection:

| [INDEX] | [TYPE]      | [HOLDS]                    | [SERVES]                                       |
| :-----: | :---------- | :------------------------- | :--------------------------------------------- |
|  [01]   | `Block[T]`  | Tuple-backed items, eager  | Pipelines with `Option` or `Result` steps      |
|  [02]   | `Map[K, V]` | AVL tree ordered by key    | State a fold threads, every version kept       |
|  [03]   | `Seq[T]`    | Lazy view over an iterable | Deferred `map`, `filter`, `choose`, and `scan` |

### [04.1]-[BLOCK]

`Block.unfold` walks a tree with no recursion or mutation. `preorder` holds the pending frontier as state, and each step emits its head and pushes the head's children in front:

```python
def preorder[T](seed: T, children: Callable[[T], Iterable[T]], /) -> Block[T]:
    frontier: tuple[T, ...] = (seed,)
    return Block.unfold(lambda pending: Some((pending[0], (*children(pending[0]), *pending[1:]))) if pending else Nothing, frontier)
```

`tailrec` types its result as `Never` under mypy and keeps `TailCall` in its return under ty, and unbounded walks take `unfold`:
- `choose(Result.to_option)` keeps every success and `map(Result.swap).choose(Result.to_option)` every fault tuple, both in input order
- `head()` on an empty `Block` or `Seq` and `reduce` on an empty `Block` raise `ValueError`
- `Block` is a `Collection` and no `Sequence`, parameters reading one take `Collection` or `Iterable`, and `case [head, *rest]` never matches it
- `case Block([head, *rest])` matches a non-empty block under both checkers
- `Block` equals a `tuple` with the same items and defines no `__hash__`, and record fields hold a `tuple`
- `zip` truncates to the shorter block, and `take(n)` past its end returns every item
- `traverse` and `sequence` take a `Block`, and a `list` runs and fails both checkers

### [04.2]-[MAP]

`change(key, f)` passes the held `Option[V]` to `f`, sets the key on `Some`, and removes it on `Nothing`. `accumulate` over `change` keeps every version, and versions share each subtree an edit leaves:

```python
type Edit[K, V] = tuple[K, Callable[[Option[V]], Option[V]]]


def versions[K: SupportsLessThanAndHash, V](start: Map[K, V], edits: Iterable[Edit[K, V]], /) -> tuple[Map[K, V], ...]:
    return tuple(accumulate(edits, lambda table, edit: table.change(*edit), initial=start))
```

`versions(Map.of(a=1), (("a", lambda count: count.map(lambda value: value + 1)), ("a", lambda _: Nothing)))` returns the start, `a=2`, and an empty map, each still valid:
- Keys bound `SupportsLessThanAndHash` from `expression.core.typing`, and mixed key types raise `TypeError` at insertion
- `try_find(key)` returns an `Option`, `m[key]` raises `KeyError`, and `Map` has no `find`
- `add` replaces a held key, and `remove` of an absent key returns the map unchanged
- `map`, `filter`, and `try_pick` take key and value as separate parameters, and `fold` takes its state and one `(key, value)` tuple
- `Map` hashes and compares by entries, `Map == dict` is `False`, and an empty map is falsy

### [04.3]-[SEQ]

`Seq` reads its source on every pass:
- `Seq` over a generator reads it once, and a second pass resumes where the first stopped
- Sources a pipeline reads twice enter as `Block.of_seq(source)` or as `Seq.of_iterable` over a re-iterable value
- `scan` emits its seed first

## [05]-[COMPOSITION]

Select a curried or composed member by the step:

| [INDEX] | [STEP]                           | [MEMBER]                                        | [LIMIT]                       |
| :-----: | :------------------------------- | :---------------------------------------------- | :---------------------------- |
|  [01]   | Result steps as one function     | `pipeline(f, g)` from `expression.extra.result` | 6 steps                       |
|  [02]   | Option steps as one function     | `pipeline(f, g)` from `expression.extra.option` | 6 steps                       |
|  [03]   | Subject-first function in `pipe` | `pipeable(f, *rest)`                            | Binds every argument after it |
|  [04]   | Subject-last declaration         | `@curry_flip(n)`                                | `n` from 0 to 4               |

`port` composes dependent result steps into one function:

```python
def parsed(text: str, /) -> Outcome[int]:
    return catch(int, exception=ValueError)(text).map_error(partial(Fault.of, Reason.INVALID, repr(text)))


def bounded(value: int, /) -> Outcome[int]:
    return Ok(value).filter_with(lambda number: 0 < number < 65536, partial(Fault.of, Reason.OUTSIDE, "port", member="1 to 65535"))


port: Callable[[str], Outcome[int]] = pipeline(parsed, bounded)
```

`port("0")` fails with the `bounded` fault and `port("x")` with the `parsed` fault, and both checkers type `pipeline` without the annotation:
- Steps reading an earlier step's value bind inside its lambda, and steps reading no bound value chain on the outer result
- `pipe(value, pipeable(clamp, low, high))` keeps the return type of `clamp`, and `@curry_flip(1)` types `scaled(2.0)` as `(float) -> float`
- `curry(n)` and `curry_flip(n)` pass keyword arguments after the curried positionals
- Module functions in `pipe` (`result.map(f)`) lose types to `Unknown` under ty and to `_TSource` under mypy, and fluent chains keep them

Checkers type `catch` by what it wraps:
- `exception=` types one class bounded by `Exception`, and a tuple or `KeyboardInterrupt` fails both checkers
- Both checkers type `catch` over a `Result[T, F]` function as `Result[Result[T, F], E]`, and a step returning a result catches inside
- `@catch` on a `def` returning a `Result` fails mypy and passes ty

## [06]-[BOUNDARIES]

Codecs read expression types as dataclasses, and records crossing them hold builtin shapes:
- Encoders in msgspec write `Option` and `Result` as their case's tag object (`{"tag":"none","none":null}`) and call no `enc_hook` on them
- `Decoder`, `convert`, and `inspect` over a type holding `Option[...]` raise `TypeError: 'Option' is not a generic type` at type analysis
- `Result` and `@tagged_union` classes decode as a record requiring every case field, and msgspec encodes no `Block` or `Map` without an `enc_hook`
- Frozen records holding `Option` and `Result` construct, compare, `structs.replace`, and `to_builtins` in process
- `Option.dict()` returns the value or `None`, and `Result.dict()` returns the tag object
