---
name: python-coding-expression
description: "Use when calling an expression member, covering result and option members, nested results and options, persistent collections, pipelines, and codec boundaries"
---

# [PYTHON_CODING_EXPRESSION]

`Block`, `Map`, and `Seq` import from `expression.collections`, and `pipeline` from `expression.extra.result`.

## [01]-[RESULTS]

`merge()` reads a `Result[T, T]` as `T`, so `outcome.map(render).map_error(report).merge()` answers both cases as one value with no `match`:
- `.ok` and `.error` raise `AttributeError` on the other case, `.value` raises `ValueError` on `Nothing`, and checkers accept each unguarded read
- `await`, iteration, and `*` unpacking read the `Ok` or `Some` value and raise `EffectError` on `Error` or `Nothing`, and checkers accept each
- Results and options define no `__bool__`, every value is truthy, and a test reads `is_some()`, `is_ok()`, or a pattern
- Results order `Ok` below `Error` and options `Nothing` below `Some`, then by value, and `max(options, default=Nothing)` reads the largest value
- Results and options hash by case and value, an `Option[K]` keys a table with `Nothing` holding the absent case, and `hash(Ok([...]))` and `copy.replace` raise `TypeError`
- `default_with` and `or_else_with` take no argument on `Option` and the error on `Result`, and `map2` takes `(mapper, other)` on `Option` and `(other, mapper)` on `Result`

## [02]-[OPTIONS]

`try_downcast` narrows an `object` value through an `isinstance` test and returns `T | None`. `held` turns a value of the wrong class into a fault naming expected and found classes:

```python
def held[T](kind: type[T], subject: str, value: object, /) -> Outcome[T]:
    return Option.of_optional(try_downcast(kind, value)).to_result_with(lambda: Fault.of(Reason.OUTSIDE, subject, type(value).__name__, member=kind.__name__))
```

`held(int, "count", "3")` fails with `count accepts int, not str`:
- `try_downcast` tests the origin class alone, and `try_downcast(list[int], ["x"])` returns `["x"]`
- Chains over an `Option` narrow through `bind(lambda found: Option.of_optional(try_downcast(kind, found)))`, an `isinstance` filter keeps the wide type
- `Option.of_optional` maps `None` alone to `Nothing`, and `Some(None)` is a `Some`
- `Option.Nothing()`, `copy`, `deepcopy`, and `pickle` build a `Nothing` copy that compares equal and fails `is Nothing`, and absence reads through `is_none()` or `Option(tag="none")`
- `starmap(f)` unpacks a tuple value into parameters of `f` with each element typed
- `Seq.of_iterable(layers).try_pick(lambda layer: layer.try_find(key))` reads the first layer holding `key` and calls no later layer

## [03]-[NESTING]

Nested results and options hold outcomes a consumer handles apart:
- `Outcome[Option[T]]` separates a failed read from a read that found nothing
- `Option[Option[T]]` holds a `T | UnsetType | None` patch field: `UNSET` as `Nothing`, `None` as `Some(Nothing)`, a value as `Some(Some(value))`

`patched` resolves a patch field through a fallible lookup and keeps absence a success:

```python
def patched[K, N](lookup: Callable[[K], Outcome[N]], current: Option[N], edit: Option[Option[K]], /) -> Outcome[Option[N]]:
    return edit.map(lambda wanted: wanted.map(lambda key: lookup(key).map(Some)).default_value(Ok(Nothing))).default_value(Ok(current))
```

`Nothing` keeps `current`, `Some(Nothing)` clears it, and `Some(Some(key))` sets the node `lookup` finds or fails with its fault. Inner `map(...).default_value(Ok(Nothing))` turns `Option[Outcome[N]]` into `Outcome[Option[N]]`.

## [04]-[COLLECTIONS]

Select the collection by when its items exist:

| [INDEX] | [TYPE]      | [HOLDS]                    | [USE]                                               |
| :-----: | :---------- | :------------------------- | :-------------------------------------------------- |
|  [01]   | `Block[T]`  | Tuple-backed items, eager  | Pipelines with `Option` or `Result` steps           |
|  [02]   | `Map[K, V]` | AVL tree ordered by key    | State a fold threads, every version kept            |
|  [03]   | `Seq[T]`    | Lazy view over an iterable | Unbounded sources, first matches through `try_pick` |

### [04.1]-[BLOCK]

`Block.unfold` grows a block from a state until a step returns `Nothing`. `ancestors` emits each parent of a key up to its root:

```python
def ancestors[K: SupportsLessThanAndHash](parents: Map[K, K], key: K, /) -> Block[K]:
    return Block.unfold(lambda node: parents.try_find(node).map(lambda parent: (parent, parent)), key)
```

`SupportsLessThanAndHash` imports from `expression.core.typing`, and `unfold` runs its step until the root's missing parent ends the block before it returns:
- `choose(Result.to_option)` keeps every success and `map(Result.swap).choose(Result.to_option)` every fault tuple, both in input order
- `head()`, `tail()`, and `reduce` raise `ValueError` on an empty block, and `sum()` returns `0`
- `skip_last(0)` returns an empty block and `take_last(0)` every item
- `Block` is a `Collection` and no `Sequence`, parameters reading one take `Collection` or `Iterable`, and `case Block([head, *rest])` matches a non-empty block where `case [head, *rest]` never does
- `Block` equals a `tuple` with the same items and defines no `__hash__`, and record fields hold a `tuple`
- `zip` truncates to the shorter block, and `indexed(start)` counts from `0` whatever `start` holds

### [04.2]-[MAP]

`change(key, f)` passes the held `Option[V]` to `f`, sets the key on `Some`, and removes it on `Nothing`. `accumulate` over `change` keeps every version, and versions share each subtree an edit leaves:

```python
def versions[K: SupportsLessThanAndHash, V](start: Map[K, V], edits: Iterable[tuple[K, Callable[[Option[V]], Option[V]]]], /) -> tuple[Map[K, V], ...]:
    return tuple(accumulate(edits, lambda table, edit: table.change(*edit), initial=start))
```

`versions(Map.of(a=1), (("a", lambda count: count.map(lambda value: value + 1)), ("a", lambda _: Nothing)))` returns the start, `a=2`, and an empty map:
- `Map ==` stops at the left map's last key (`Map.of(a=1) == Map.of(a=1, b=2)` holds) and is `False` against a `dict`, and whole maps compare through `left.items() == right.items()`
- `Map` is a `Mapping` iterating in key order whatever the insertion order, `of_seq` keeps the last value of a repeated key, and mixed key types raise `TypeError` at insertion
- `map`, `filter`, and `try_pick` take key and value as separate parameters, and `fold` takes its state and one `(key, value)` tuple

### [04.3]-[SEQ]

`Seq` members split by whether a second pass re-reads the source:
- `map`, `choose`, `collect`, `take`, `take_while`, `skip`, `pairwise`, and `append` re-read their source, and `filter` and `scan` return a `Seq` over a one-pass iterator even over a tuple
- `zip` and `unfold` return a bare `zip` or generator typed `Iterable`, and `Seq.of_iterable` wraps one to continue a chain
- `Seq` over a generator reads it once, and a second pass resumes where the first stopped
- Values a pipeline reads twice enter as `Block.of_seq(source)`, and an unbounded source reads through `Seq.of_iterable(source).take_while(predicate)`
- `scan` emits its seed first

## [05]-[COMPOSITION]

`counts` chains each text's dependent steps, each text stops at its first fault, and the batch reports every text's fault:

```python
def counts(texts: Iterable[str], /, *, limit: int = 100) -> Outcome[tuple[int, ...]]:
    return accumulated(
        catch(int, exception=ValueError)(text)
        .map_error(partial(Fault.of, Reason.INVALID, repr(text)))
        .filter_with(lambda count: count <= limit, partial(Fault.of, Reason.OUTSIDE, "count", member=f"at most {limit}"))
        for text in texts
    )
```

`counts(("3", "x", "500"))` fails with the `INVALID` fault for `"x"` and the `OUTSIDE` fault for `500`:
- `pipeline(f, g)` composes named result steps into one function value, `pipeline(f, g)(x)` is `f(x).bind(g)`, its overloads type up to six steps, and `expression.extra.option.pipeline` composes option steps
- `traverse(f, block)` and `sequence` call `f` on every item from last to first and return the first item's fault in input order
- `pipeable(f, *rest)` binds every argument after a subject-first function's subject, and `compose(str.strip, pipeable(textwrap.shorten, 40))` keeps each step typed
- `catch` returns a wrapped function's `Result` unwrapped while checkers type it `Result[Result[T, F], E]`, and a step returning a result catches its raising call inside

## [06]-[BOUNDARIES]

Codecs read expression types as dataclasses, and records crossing them hold `T | None` and `tuple` fields:
- Encoders in msgspec write `Option` and `Result` as their case's tag object (`{"tag":"none","none":null}`) and call no `enc_hook` on them
- `Decoder`, `convert`, and `inspect` over a type holding `Option[...]` raise `TypeError: 'Option' is not a generic type` at type analysis
- `Result` decodes in msgspec as a record requiring every case field, and msgspec encodes no `Block` or `Map` without an `enc_hook`
- Pydantic builds `Option` and `Result` fields as dataclasses that refuse `null`, bare values, and built instances, and validates and dumps `Block[T]` as a list
- Frozen records holding `Option` and `Result` construct, compare, `structs.replace`, and `to_builtins` in process
