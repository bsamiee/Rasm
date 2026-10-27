# [RESULTS]

Covers result flows from faults through validation, batches, and workflows to host translation and the equations results satisfy.

## [01]-[FAULTS]

`Faults` holds at least one `Fault`, and each reason value is a message template over `subject`, `member`, and `cause`:

```python
type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class Reason(StrEnum):
    ABSENT = "{subject} holds no {member}"
    INVALID = "{subject} is invalid. {cause}"
    OUTSIDE = "{subject} accepts {member}, not {cause}"
    UNREADABLE = "{subject} is unreadable. {cause}"


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    reason: StrEnum
    subject: str
    member: str = ""
    cause: BaseException | str = ""

    @property
    def message(self) -> str:
        return self.reason.format(subject=self.subject, member=self.member, cause=self.cause)
```

`reason` takes a member of any package's reason enum, and a new failure kind adds one member with its template. `Fault(reason=Reason.OUTSIDE, subject="size", member="1 to 4096", cause="0")` reads `size accepts 1 to 4096, not 0`, and a caught exception kept as `cause` reads as its own message. `Error` and `map_error` take a one-fault tuple `(Fault(...),)` as `Faults`, and an empty tuple fails both checkers.

## [02]-[VALIDATION]

Independent results combine through `accumulated` over any count and `paired` over a fixed arity, both keeping every fault in input order:

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

`swap().default_value(())` yields each result's faults, and the `(head, *rest)` arm builds the non-empty tuple `Faults` requires. Each `paired` overload types one arity's values as a tuple, and batches pass every item's outcome to `accumulated` (an empty batch returns `Ok(())`). Checks over one value run as independent results:

```python
class Wire(msgspec.Struct, frozen=True, kw_only=True):
    name: str
    size: int


def named(wire: Wire, /) -> Outcome[str]:
    return Ok(wire.name).filter_with(str.isidentifier, lambda name: (Fault(reason=Reason.OUTSIDE, subject="name", member="an identifier", cause=repr(name)),))


def sized(limit: int, wire: Wire, /) -> Outcome[int]:
    return Ok(wire.size).filter_with(lambda size: 0 < size <= limit, lambda size: (Fault(reason=Reason.OUTSIDE, subject="size", member=f"1 to {limit}", cause=str(size)),))


def accepted(taken: frozenset[str], wire: Wire, /) -> Outcome[Wire]:
    return (
        paired(named(wire), sized(4096, wire))
        .map(lambda _: wire)
        .filter_with(lambda live: live.name not in taken, lambda live: (Fault(reason=Reason.OUTSIDE, subject="name", member="an unused name", cause=repr(live.name)),))
    )
```

Checks reading a setting take it before the subject. Steps that need every check passed chain after the combination: for `Wire(name="9x", size=0)`, `accepted` returns both faults (`name accepts an identifier, not '9x'`, `size accepts 1 to 4096, not 0`) and never reads `taken`.

## [03]-[WORKFLOWS]

Workflows parse, validate, and look up in order, and adding a step adds one call:

```python
def port(text: str, /) -> Outcome[int]:
    return (
        catch(int, exception=ValueError)(text)
        .map_error(lambda error: (Fault(reason=Reason.INVALID, subject=repr(text), cause=error),))
        .filter_with(lambda number: 0 < number < 65536, lambda number: (Fault(reason=Reason.OUTSIDE, subject="port", member="1 to 65535", cause=str(number)),))
    )


def service(table: Mapping[int, str], number: int, /) -> Outcome[str]:
    return Option.of_optional(table.get(number)).to_result_with(lambda: (Fault(reason=Reason.ABSENT, subject="table", member=f"port {number}"),))


def endpoint(table: Mapping[int, str], text: str, /) -> Outcome[str]:
    return port(text).bind(lambda number: service(table, number).map(lambda name: f"{name}:{number}"))
```

Failed steps skip every later step and return one fault: `"x"` fails at `catch` with its `ValueError` as cause, `"70000"` fails `filter_with` with the parsed number as cause, and `"22"` fails `to_result_with` against a table without port 22. `filter_with` builds faults from a refused value, and `to_result_with` builds faults on absence alone. Steps reading an earlier value nest inside a lambda that binds the value: `endpoint` maps the found name with `number` in scope and returns `Ok("https:443")` for `"443"`.

## [04]-[HOST_TRANSLATION]

Hosts translate a result in one `match` on `tag` into their vocabulary (exit code, HTTP status, UI state, host exception), and classify each fault by its `reason` member:

```python
def exited(outcome: Outcome[int], /) -> int:
    match outcome.tag:
        case "ok":
            return outcome.ok
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 1


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

`exited` returns a success's value as exit code, and a failure writes each fault message to standard error and returns `1`. `status` reads members of any package's enum, and `responded` answers with the highest status among the faults: absent and unreadable faults together answer `500`. `max` reads `Faults` with no default, its type holds at least one fault.

## [05]-[EQUATIONS]

`map` and `bind` satisfy these equations for every value, `Error` included, and a step hiding mutation or counting calls breaks them:

```text
Functor identity:     r.map(lambda x: x) == r
Functor composition:  r.map(g).map(f) == r.map(lambda x: f(g(x)))
Monad left identity:  Ok(x).bind(f) == f(x)
Monad right identity: r.bind(Ok) == r
Monad associativity:  r.bind(f).bind(g) == r.bind(lambda x: f(x).bind(g))
```

Both identities let `Ok` lift a value with no other effect, and associativity lets a workflow regroup its steps into named functions with the same result.
