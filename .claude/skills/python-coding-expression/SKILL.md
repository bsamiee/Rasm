---
name: python-coding-expression
description: "Use when calling an expression member, covering results, options, nesting, persistent collections, pipelines, and codec boundaries"
---

# [PYTHON_CODING_EXPRESSION]

`Block`, `Map`, and `Seq` import from `expression.collections`, and `pipeline` from `expression.extra.result`.

## [01]-[RESULTS]

`merge()` unwraps either case of a `Result[T, T]`, and `outcome.map(render).map_error(report).merge()` answers both cases with no `match`:
- `.ok` and `.error` raise `AttributeError` on the other case, `.value` raises `ValueError` on `Nothing`, and checkers accept each unguarded read
- `await`, iteration, and `*` unpacking read an `Ok` or `Some` value and raise `EffectError` on `Error` or `Nothing`, and checkers accept each
- Results and options define no `__bool__`, every value is truthy, and tests read `is_some()`, `is_ok()`, or a pattern
- Results order `Ok` below `Error` and options `Nothing` below `Some`, then by value, and `max(options, default=Nothing)` reads the largest value
- Results and options hash by case and value, `Option[K]` keys hold `Nothing` for absence, and `hash(Ok([...]))` and `copy.replace` raise `TypeError`
- On `Option`, `default_with` and `or_else_with` callbacks take nothing and `map2` takes `mapper` first, on `Result` the error and `mapper` last

## [02]-[OPTIONS]

`try_downcast` narrows an `object` through `isinstance` to `T | None`, and `held` turns a wrong-class value into a fault naming both classes:

```python
def held[T](kind: type[T], subject: str, value: object, /) -> Outcome[T]:
    return Option.of_optional(try_downcast(kind, value)).to_result_with(lambda: Fault.of(Reason.OUTSIDE, subject, type(value).__name__, member=kind.__name__))
```

`held(int, "count", "3")` fails with `count accepts int, not str`:
- `try_downcast` tests the origin class alone, and `try_downcast(list[int], ["x"])` returns `["x"]`
- `Option` chains narrow through `bind(lambda found: Option.of_optional(try_downcast(kind, found)))`, an `isinstance` filter keeps the wide type
- `Option.of_optional` maps `None` alone to `Nothing`, and `Some(None)` is a `Some`
- `Option.Nothing()`, `copy`, `deepcopy`, and `pickle` build an equal `Nothing` failing `is Nothing`, test `is_none()` or `Option(tag="none")`
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

`SupportsLessThanAndHash` imports from `expression.core.typing`, and `unfold` runs its step until the root's missing parent ends the block:
- `choose(Result.to_option)` keeps every success and `map(Result.swap).choose(Result.to_option)` every fault tuple, both in input order
- `head()`, `tail()`, and `reduce` raise `ValueError` on an empty block, and `sum()` returns `0`
- `skip_last(0)` returns an empty block and `take_last(0)` every item
- `Block` fits `Collection` or `Iterable` parameters and no `Sequence`, and `case Block([head, *rest])` matches where `case [head, *rest]` fails
- `Block` equals a `tuple` with the same items and defines no `__hash__`, and record fields hold a `tuple`
- `zip` truncates to the shorter block, and `indexed(start)` counts from `0` whatever `start` holds

### [04.2]-[MAP]

`change(key, f)` passes the held `Option[V]` to `f`, sets the key on `Some`, and removes it on `Nothing`. `accumulate` over `change` keeps every version, and versions share each subtree an edit leaves:

```python
def versions[K: SupportsLessThanAndHash, V](start: Map[K, V], edits: Iterable[tuple[K, Callable[[Option[V]], Option[V]]]], /) -> tuple[Map[K, V], ...]:
    return tuple(accumulate(edits, lambda table, edit: table.change(*edit), initial=start))
```

`versions(Map.of(a=1), (("a", lambda count: count.map(lambda value: value + 1)), ("a", lambda _: Nothing)))` returns `a=1`, `a=2`, and an empty map:
- `Map ==` reads left keys alone (`Map.of(a=1) == Map.of(a=1, b=2)` holds) and equals no `dict`, full equality is `left.items() == right.items()`
- `Map` is a `Mapping` iterating in key order, `of_seq` keeps a repeated key's last value, and mixed key types raise `TypeError` at insertion
- `map`, `filter`, and `try_pick` take key and value as separate parameters, and `fold` takes its state and one `(key, value)` tuple

### [04.3]-[SEQ]

`Seq` members split by whether a second pass re-reads the source:
- `map`, `choose`, `collect`, `take`, `take_while`, `skip`, `pairwise`, and `append` re-read their source, `filter` and `scan` read any source once
- `zip` and `unfold` return a plain `zip` or generator typed `Iterable`, and `Seq.of_iterable` wraps one to continue a chain
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
- `pipeline(f, g)(x)` is `f(x).bind(g)`, overloads type up to six steps, and `expression.extra.option.pipeline` composes option steps
- `traverse(f, block)` and `sequence` call `f` on every item from last to first and return the first item's fault in input order
- `pipeable(f, *rest)` binds `rest` after `f`'s first parameter, and `compose(str.strip, pipeable(textwrap.shorten, 40))` keeps each step typed
- `catch` keeps a wrapped function's `Result` flat while checkers type it `Result[Result[T, F], E]`, and result steps catch their raising call inside

## [06]-[BOUNDARIES]

Codecs read expression types as dataclasses, and records crossing them hold `T | None` and `tuple` fields:
- Encoders in msgspec write `Option` and `Result` as their case's tag object (`{"tag":"none","none":null}`) and call no `enc_hook` on them
- `Decoder`, `convert`, and `inspect` over a type holding `Option[...]` raise `TypeError: 'Option' is not a generic type` at type analysis
- `Result` decodes in msgspec as a record requiring every case field, and msgspec encodes no `Block` or `Map` without an `enc_hook`
- Pydantic builds `Option` and `Result` fields as dataclasses refusing `null`, raw values, and built instances, and round-trips `Block[T]` as a list
- Frozen records holding `Option` and `Result` construct, compare, `structs.replace`, and `to_builtins` in process
