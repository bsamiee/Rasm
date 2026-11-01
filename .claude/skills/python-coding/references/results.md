# [RESULTS]

Covers result flows past one chain, from accumulation and context checks to recovery, option combinators, host translation, and the equations results satisfy. `Fault`, `Faults`, `Outcome`, and `Reason` are the package faults, and `content` is the package byte reader.

## [01]-[ACCUMULATION]

`accumulated` combines any count of independent results and `paired` a fixed arity:

```python
def accumulated[T](results: Iterable[Outcome[T]], /) -> Outcome[tuple[T, ...]]:
    held = tuple(results)
    match tuple(*result.swap().default_value(()) for result in held):
        case (head, *rest):
            return Error((head, *rest))
        case _:
            return Ok(tuple(result.ok for result in held))


@overload
def paired[A, B](a: Outcome[A], b: Outcome[B], /) -> Outcome[tuple[A, B]]: ...
@overload
def paired[A, B, C](a: Outcome[A], b: Outcome[B], c: Outcome[C], /) -> Outcome[tuple[A, B, C]]: ...
def paired(*results: Outcome[object]) -> Outcome[tuple[object, ...]]:
    return accumulated(results)
```

`swap().default_value(())` yields each result's faults, the `(head, *rest)` arm builds the non-empty tuple `Faults` requires, and an empty batch returns `Ok(())`. `traverse`, `sequence`, `map2`, and `pipeline` keep one error.

`priced` reads each line against a catalog and reports every unknown SKU at once. `Line` is a decoded record of `sku` and a `Meta`-bounded `count`:

```python
def priced(prices: Mapping[str, Decimal], lines: Iterable[Line], /) -> Outcome[Decimal]:
    return accumulated(
        Option.of_optional(prices.get(line.sku)).map(lambda price: price * line.count).to_result_with(partial(Fault.of, Reason.ABSENT, "catalog", member=line.sku))
        for line in lines
    ).map(lambda costs: sum(costs, start=Decimal()))
```

Each unknown SKU adds one `catalog holds no <sku>` fault, and a fully priced batch returns its exact `Decimal` total:
- Checks reading one item and context run per item inside `accumulated`, a check across items reads the batch once and joins through `paired`
- Rules sharing `T -> Outcome[T]` run as `accumulated(rule(value) for rule in rules).map(lambda _: value)`

## [02]-[RECOVERY]

`journal` reads a missing file as empty content, and `ended` runs a marking step after a failure and appends its fault:

```python
def _unwritten(faults: Faults, /) -> Outcome[bytes]:
    match faults:
        case (Fault(reason=Reason.UNREADABLE, cause=FileNotFoundError()),):
            return Ok(b"")
        case _:
            return Error(faults)


def journal(path: Path, /) -> Outcome[bytes]:
    return content(path).or_else_with(_unwritten)


def ended[T](work: Outcome[T], mark: Callable[[], Outcome[None]], /) -> Outcome[T]:
    return work.or_else_with(lambda faults: Error((*faults, *mark().swap().default_value(()))))
```

`journal` returns `Ok(b"")` for a missing file alone, and a directory, a denied read, or a missing file beside a second fault keeps its faults. `ended` skips the marking step on success and returns the work faults followed by the marking fault.

## [03]-[OPTIONS]

Options combine where every part must be present. `collected` returns every value, or `Nothing` when any part is absent:

```python
def collected[T](options: Iterable[Option[T]], /) -> Option[tuple[T, ...]]:
    held = tuple(options)
    return Some(tuple(*option.to_list() for option in held)).filter(lambda values: len(values) == len(held))
```

`collected` returns `Some(())` for no parts, and `found.map(Ok).default_with(partial(loaded, key))` runs a costly fallible load only on a miss.

## [04]-[HOST_TRANSLATION]

`STATUS` maps each reason a client causes to its HTTP status, and `responded` answers an outcome with a status and a JSON body:

```python
STATUS: frozendict[StrEnum, HTTPStatus] = frozendict({Reason.ABSENT: HTTPStatus.NOT_FOUND, Reason.INVALID: HTTPStatus.BAD_REQUEST, Reason.OUTSIDE: HTTPStatus.BAD_REQUEST})


def responded(outcome: Outcome[object], /) -> tuple[HTTPStatus, bytes]:
    match outcome:
        case Result(tag="ok", ok=value):
            return HTTPStatus.OK, msgspec.json.encode(value)
        case Result(error=faults):
            return max(STATUS.get(fault.reason, HTTPStatus.INTERNAL_SERVER_ERROR) for fault in faults), msgspec.json.encode([fault.message for fault in faults])
```

Keys take members of any package's reason enum, a reason with no row answers `500`, and `responded` returns the highest status among the faults: absent and unreadable faults together answer `500`. `max` reads the faults with no default, `Faults` holds at least one.

## [05]-[EQUATIONS]

`map` and `bind` satisfy the functor and monad laws for every value, `Error` included, and a step hiding mutation or counting calls breaks them:

```text
Functor identity:     r.map(lambda x: x) == r
Functor composition:  r.map(g).map(f) == r.map(lambda x: f(g(x)))
Monad left identity:  Ok(x).bind(f) == f(x)
Monad right identity: r.bind(Ok) == r
Monad associativity:  r.bind(f).bind(g) == r.bind(lambda x: f(x).bind(g))
Accumulation:         paired(Ok(a), Ok(b)) == Ok((a, b)), paired(Error(e), Error(f)) == Error((*e, *f))
```

Both identities let `Ok` lift a value with no other effect, and associativity lets a workflow regroup its steps into named functions with the same result.
