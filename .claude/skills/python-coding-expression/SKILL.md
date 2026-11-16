---
name: python-coding-expression
description: "Use when calling an expression member, covering cases, results, options, unions, persistent collections, composition, effects, and codec boundaries"
---

# [PYTHON_CODING_EXPRESSION]

`Some`, `Nothing`, `NOTHING`, `Option`, `Ok`, `Error`, `Result`, `option`, `result`, `case`, `tag`, `TaggedUnion`, `effect`, `pipe`, `compose`, `pipeable`, and `apipe` import from `expression`, `Block`, `Map`, and `Seq` from `expression.collections`, `catch`, `pipeline`, and `traverse` from `expression.extra.result`, and `async_result` from `expression.extra`.

## [01]-[CASES]

`Some`, `Nothing`, `Ok`, and `Error` are frozen case classes, `Option[T]` and `Result[T, E]` alias their unions, and `match` reads each case by its class:

```python
def width(found: Option[int], /) -> int:
    match found:
        case Some(value):
            return value
        case Nothing():
            return 0
```

`case Ok(value)` and `case Error(error)` read a result the same way:
- `NOTHING` is the empty instance, `Nothing()`, copy, pickle, and decoding return it, and `Nothing` alone names the class
- `case Nothing():` matches the empty case, and bare `case Nothing:` captures every subject
- `isinstance(value, Some | Nothing)` and `isinstance(value, Ok | Error)` test a case, `isinstance` over `Option` or `Result` raises `TypeError`
- `option.is_some`, `option.is_none`, `result.is_ok`, and `result.is_error` narrow to a case, methods `is_some()` and `is_ok()` narrow nothing
- `.some`, `.ok`, and `.error` read a narrowed case and fail ty on a union, `Ok` has no `.value`, and `NOTHING.value` raises `ValueError`
- `await`, iteration, and `*` unpacking read an `Ok` or `Some` value and raise `EffectError` on `Error` or `Nothing`
- Cases define no `__bool__` and no ordering: every value is truthy, and `<`, `max`, and `sorted` without `key` raise `TypeError`
- Cases hash by value, `Option[K]` keys hold `NOTHING` for absence, `hash(Ok([...]))` raises `TypeError`, and `copy.replace` builds a case

## [02]-[RESULTS]

`merge()` unwraps either case of a `Result[T, T]`, and `outcome.map(render).map_error(report).merge()` answers both cases with no `match`:
- `default_with` and `or_else_with` callbacks take the error on `Result` and nothing on `Option`
- `map2` takes `other` first on `Result` and `mapper` first on `Option`, and a `Result` pair keeps the first error
- `catch(f, exception=E)(arguments)` returns `Ok` or `Error[E]` for one raising call with `f`'s parameters, and `exception` takes one class
- Steps returning `Result` catch their raising call inside, as ty types `catch` over such a step `Ok[Ok[T] | Error[F] | T]`
- Generic callables take the caught form in a local typed `Callable[[<parameters>], Result[T, E]]`, as ty types `catch` over a type parameter `Ok[Unknown]`, and a `partial` takes a `try` returning `Ok` or `Error`
- Decoders catch a typed `msgspec.json.Decoder(<type>).decode`, as `catch` over `msgspec.json.decode(raw, type=...)` types `Ok[Unknown]`

## [03]-[OPTIONS]

`held` narrows an `object` to `kind`, a matched `None` reading as `NOTHING`:

```python
def held[T](kind: type[T], value: object, /) -> Option[T]:
    return option.of_optional(try_downcast(kind, value))
```

Options narrow and read through module functions:
- `try_downcast` returns `T | None`, and a parameterized `kind` raises `TypeError`
- `.bind(lambda found: held(kind, found))` narrows an `Option[object]`, and an `isinstance` filter keeps the wide type
- `option.of_optional` maps `None` alone to `NOTHING`, and `Some(None)` is a `Some`
- `starmap(f)` on `Some` and `Block` unpacks a tuple value into parameters of `f` with each element typed
- `Seq.of_iterable(layers).try_pick(lambda layer: layer.try_find(key))` reads the first layer holding `key` and calls no later layer

## [04]-[NESTING]

Nested results and options hold outcomes a consumer handles apart:
- `Result[Option[T], E]` separates a failed read from a read that found nothing
- `Option[Option[T]]` holds a `T | UnsetType | None` patch field: `UNSET` as `NOTHING`, `None` as `Some(NOTHING)`, a value as `Some(Some(value))`

`patched` resolves a patch field through a fallible lookup and keeps absence a success:

```python
def patched[K, N, E](lookup: Callable[[K], Result[N, E]], current: Option[N], edit: Option[Option[K]], /) -> Result[Option[N], E]:
    match edit:
        case Some(wanted):
            return wanted.map(lambda key: lookup(key).map(Some)).default_value(Ok(NOTHING))
        case Nothing():
            return Ok(current)
```

`NOTHING` keeps `current`, `Some(NOTHING)` clears it, and `Some(Some(key))` sets the node `lookup` finds or fails with its error.

## [05]-[UNIONS]

`@case` classes are frozen cases declaring `tag: Literal["<name>"] = tag()`, and `TaggedUnion` closes their union:

```python
@final
@case
class Circle:
    radius: float
    tag: Literal["circle"] = tag()


@final
@case
class Square:
    side: float
    tag: Literal["square"] = tag()


type Shape = TaggedUnion[Circle | Square]


def area(shape: Shape, /) -> float:
    match shape:
        case Circle(radius):
            return math.pi * radius**2
        case Square(side):
            return side**2
```

`Circle(1.0)` builds a case, and patterns bind payloads by position with `tag` left out:
- `tag()` takes no value, and the single-string `Literal` names the discriminator
- `@final` refuses subclasses for checkers, and `@case` refuses subclasses, `InitVar`, non-tag `init=False` fields, `__init__`, and dataclass bases
- `__post_init__` checks payloads, and `@case(kw_only=True)` takes keyword payloads
- Unit cases compare equal and build a new instance per call
- Payloads naming a later case or the alias spell it as a string (`Block["Tree"]`), as `@case` evaluates annotations at class creation

## [06]-[COLLECTIONS]

Select the collection by when its items exist:

| [INDEX] | [TYPE]      | [HOLDS]                    | [USE]                                               |
| :-----: | :---------- | :------------------------- | :-------------------------------------------------- |
|  [01]   | `Block[T]`  | Tuple-backed items, eager  | Pipelines with `Option` or `Result` steps           |
|  [02]   | `Map[K, V]` | AVL tree ordered by key    | State a fold threads, every version kept            |
|  [03]   | `Seq[T]`    | Lazy view over an iterable | Unbounded sources, first matches through `try_pick` |

### [06.1]-[BLOCK]

`Block.unfold` grows a block from a state until a step returns `NOTHING`. `ancestors` emits each parent of a key up to its root:

```python
def ancestors[K: SupportsLessThanAndHash](parents: Map[K, K], key: K, /) -> Block[K]:
    return Block.unfold(lambda node: parents.try_find(node).map(lambda parent: (parent, parent)), key)
```

`SupportsLessThanAndHash` imports from `expression.core.typing`, and the root's missing parent ends the block:
- `head()`, `tail()`, and `reduce` raise `ValueError` on an empty block, and `sum()` returns `0`
- `skip_last(0)` returns every item and `take_last(0)` an empty block
- `Block` fits `Collection` or `Iterable` parameters and no `Sequence`, and `case Block([head, *rest])` matches where `case [head, *rest]` fails
- `Block` equals a `tuple` with the same items and defines no `__hash__`, and record fields hold a `tuple`
- `zip` truncates to the shorter block, and `indexed(start)` counts from `0` whatever `start` holds

### [06.2]-[MAP]

`change(key, f)` passes the held `Option[V]` to `f`, sets the key on `Some`, and removes it on `NOTHING`. `accumulate` over `change` keeps every version, and versions share each subtree an edit leaves:

```python
def versions[K: SupportsLessThanAndHash, V](start: Map[K, V], edits: Iterable[tuple[K, Callable[[Option[V]], Option[V]]]], /) -> tuple[Map[K, V], ...]:
    return tuple(accumulate(edits, lambda table, edit: table.change(*edit), initial=start))
```

`versions(Map.of(a=1), (("a", lambda count: count.map(lambda value: value + 1)), ("a", lambda _: NOTHING)))` returns `a=1`, `a=2`, and an empty map:
- `Map` equality compares every key and value, another type compares unequal, and maps hash by their items
- `Map` is a `Mapping` iterating in key order, `of_seq` keeps a repeated key's last value, and incomparable keys raise `TypeError` at insertion
- `map`, `filter`, and `try_pick` take key and value as separate parameters, and `fold` takes its state and one `(key, value)` tuple

### [06.3]-[SEQ]

`Seq` members split by whether a second pass re-reads the source:
- `map`, `choose`, `collect`, `take`, `take_while`, `skip`, `pairwise`, and `append` re-read their source, `filter` and `scan` read any source once
- `zip` and `unfold` return a plain `zip` or generator typed `Iterable`, and `Seq.of_iterable` wraps one to continue a chain
- `Seq` over a generator reads it once, and a second pass resumes where the first stopped
- Values a pipeline reads twice enter as `Block(source)`, and an unbounded source reads through `Seq.of_iterable(source).take_while(predicate)`
- `scan` emits its seed first

## [07]-[COMPOSITION]

Chains call fluent methods, and `pipe` and `compose` take non-generic one-argument functions, as ty infers `Unknown` for curried collection and option functions in `pipe`. `counts` chains each text's dependent steps, each text stops at its first error, and the batch reports every text's error:

```python
def counts(texts: Block[str], /, *, limit: int) -> Result[Block[int], Block[str]]:
    parsed = texts.map(
        lambda text: catch(int, exception=ValueError)(text).map_error(lambda _: f"{text!r} is no count").filter_with(lambda count: count <= limit, lambda count: f"{count} exceeds {limit}")
    )
    errors = parsed.choose(lambda outcome: outcome.swap().to_option())
    return Ok(parsed.choose(lambda outcome: outcome.to_option())) if errors.is_empty() else Error(errors)
```

`counts(Block.of("3", "x", "500"), limit=100)` fails with the errors for `"x"` and `500`:
- `traverse(f, block)` calls `f` on every item from last to first and returns the first error in input order
- `pipeline(f, g)(x)` is `f(x).bind(g)` with overloads up to six steps, and `expression.extra.option.pipeline` composes option steps
- `pipeable(f, *rest)` binds `rest` after the subject, and `compose(str.strip, pipeable(textwrap.shorten, width=width))` types each step

## [08]-[EFFECTS]

`effect.result[T, E]()` and `effect.option[T]()` turn a generator into a function returning `Result[T, E]` or `Option[T]`, and a generator comprehension binds steps of differing types:

```python
@effect.result[str, str]()
def labeled(raw: str, names: Map[int, str], /) -> Generator[str]:
    yield from (f"{name}={count}" for count in catch(int, exception=ValueError)(raw).map_error(str) for name in names.try_find(count).to_result(f"no name for {count}"))
```

`labeled("2", Map.of_seq([(2, "pair")]))` returns `Ok("pair=2")`:
- Direct style (`value = yield from step`) binds one type, the type every yield shares
- Bodies propagate exceptions, a broad `except Exception` intercepts the `EffectError` that ends them, and `effect.try_` converts no exception
- `apipe(value, *stages)` awaits each stage returning a coroutine and hands each `Result` whole to the next stage
- `async_result.bind(step)` awaits an async step on `Ok` and passes `Error` through: `await apipe(outcome, async_result.bind(fetched))`

## [09]-[BOUNDARIES]

Records crossing codec boundaries hold `T | None`, `tuple`, and tagged `msgspec.Struct` unions, and consumers read `option.of_optional(record.field)`:
- Encoders in msgspec write `Some(1)` as `{"some":1}`, `NOTHING` as `{}`, and `Block` as `{"_value":[...]}`, and call no `enc_hook`
- Decoders in msgspec (`Decoder`, `convert`, `decode(type=...)`) raise `TypeError` over a type holding `Option`, `Result`, or a case union
- Frozen msgspec records holding `Option` and `Result` construct, compare, and `structs.replace` in process
- Pydantic validates `Option`, `Result`, and case union fields from tagged objects and instances, refuses `None` and raw values, and dumps tagged
- Pydantic validates `Block` fields from an instance or `{"_value":[...]}` and refuses a list
- `TypeAdapter` over the alias (`Option[int]`, `Shape`) validates the tag, and `TypeAdapter(Circle)` skips it
