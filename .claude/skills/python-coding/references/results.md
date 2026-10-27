# [RESULTS]

Covers result flows past one chain, from accumulation and recovery to option combinators, host translation, and the equations results satisfy.

## [01]-[ACCUMULATION]

`accumulated` combines any count of independent results and `paired` a fixed arity. `Fault`, `Faults`, and `Outcome` are the package faults:

```python
def accumulated[T](results: Iterable[Outcome[T]], /) -> Outcome[tuple[T, ...]]:
    held = tuple(results)
    match tuple(*result.swap().default_value(()) for result in held):
        case (head, *rest):
            return Error((head, *rest))
        case _:
            return Ok(tuple(*result.to_option().to_list() for result in held))


@overload
def paired[A, B](a: Outcome[A], b: Outcome[B], /) -> Outcome[tuple[A, B]]: ...
@overload
def paired[A, B, C](a: Outcome[A], b: Outcome[B], c: Outcome[C], /) -> Outcome[tuple[A, B, C]]: ...
def paired(*results: Outcome[object]) -> Outcome[tuple[object, ...]]:
    return accumulated(results)
```

`swap().default_value(())` yields each result's faults, and the `(head, *rest)` arm builds the non-empty tuple `Faults` requires. Each `paired` overload types one arity's values as a tuple, and batches pass every item's outcome to `accumulated`, an empty batch returning `Ok(())`:
- `traverse`, `sequence`, `map2`, and `pipeline` keep one error, `traverse` calls its step on every element back to front
- Effect builders swallow a `RuntimeError` a step raises (`Ok(None)` from a builder), chains replace them

## [02]-[RECOVERY]

`journal` reads a missing file as empty content, and `ended` keeps a failure's faults when the step that marks the failure fails. `content` is the package byte reader:

```python
def _unwritten(faults: Faults, /) -> Outcome[bytes]:
    match faults:
        case (Fault(reason=Reason.UNREADABLE, cause=FileNotFoundError()),):
            return Ok(b"")
        case _:
            return Error(faults)


def journal(path: Path, /) -> Outcome[bytes]:
    return content(path).or_else_with(_unwritten)


def _ended[T](mark: Callable[[], Outcome[None]], faults: Faults, /) -> Outcome[T]:
    return Error((*faults, *mark().swap().default_value(())))


def ended[T](work: Outcome[T], mark: Callable[[], Outcome[None]], /) -> Outcome[T]:
    return work.or_else_with(partial(_ended, mark))
```

`journal` returns `Ok(b"")` for a missing file alone, a directory or a denied read keeps its fault, and `ended` returns the work faults followed by the marking fault:
- `or_else_with` runs only on failure, a success skips the marking step
- `merge()` on a `Result[T, T]` reads whichever side a fold ended on, a partial log on the error side reaching a rollback

## [03]-[OPTIONS]

Options combine applicatively where every part must be present. `joined` concatenates optional tuples and returns `Nothing` when any part is absent:

```python
def joined[T](parts: Iterable[Option[tuple[T, ...]]], /) -> Option[tuple[T, ...]]:
    empty: Option[tuple[T, ...]] = Some(())
    return reduce(lambda held, part: held.map2(lambda done, more: (*done, *more), part), parts, empty)
```

`joined` returns `Some(())` for no parts:
- `found.map(filled).default_with(lambda: built().bind(owned))` runs the costly build only on a miss

## [04]-[HOST_TRANSLATION]

`status` maps each fault's reason to an HTTP status, and `responded` turns an outcome into a status and a JSON body:

```python
def status(fault: Fault, /) -> HTTPStatus:
    match fault.reason:
        case Reason.ABSENT:
            return HTTPStatus.NOT_FOUND
        case Reason.INVALID | Reason.OUTSIDE:
            return HTTPStatus.BAD_REQUEST
        case _:
            return HTTPStatus.INTERNAL_SERVER_ERROR


JSON = msgspec.json.Encoder()


def responded(outcome: Outcome[str], /) -> tuple[HTTPStatus, bytes]:
    match outcome.tag:
        case "ok":
            return HTTPStatus.OK, JSON.encode({"endpoint": outcome.ok})
        case "error":
            return max(map(status, outcome.error)), JSON.encode([fault.message for fault in outcome.error])
```

`status` reads members of any package's enum, and `responded` returns the highest status among the faults: absent and unreadable faults together answer `500`. `max` reads `Faults` with no default, its type holds at least one fault.

## [05]-[EQUATIONS]

`map` and `bind` satisfy the functor and monad laws for every value, `Error` included, and a step hiding mutation or counting calls breaks them:

```text
Functor identity:     r.map(lambda x: x) == r
Functor composition:  r.map(g).map(f) == r.map(lambda x: f(g(x)))
Monad left identity:  Ok(x).bind(f) == f(x)
Monad right identity: r.bind(Ok) == r
Monad associativity:  r.bind(f).bind(g) == r.bind(lambda x: f(x).bind(g))
```

Both identities let `Ok` lift a value with no other effect, and associativity lets a workflow regroup its steps into named functions with the same result.
