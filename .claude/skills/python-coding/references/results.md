# [RESULTS]

Covers result flows from one package's faults through validation, workflows, and batches to host translation and the laws.

## [01]-[FAULTS]

Packages declare `Faults`, `Reason`, and `Fault` in one module every raiser imports. `of` builds faults for a refused value, and `caught` builds faults for a caught exception with each leaf exception kept as cause:

```python
type Faults = tuple[Fault, ...]


class Reason(StrEnum):
    ABSENT = "is absent"
    INVALID = "is invalid"
    UNREADABLE = "cannot be read"


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    reason: Reason
    subject: str
    cause: Option[Exception] = Nothing

    @property
    def message(self) -> str:
        return f"{self.subject} {self.reason}"

    @classmethod
    def of(cls, reason: Reason, subject: object, /) -> Faults:
        return (cls(reason=reason, subject=repr(subject)),)

    @classmethod
    def caught(cls, reason: Reason, subject: object, error: Exception, /) -> Faults:
        match error:
            case ExceptionGroup(exceptions=leaves):
                return tuple(*cls.caught(reason, subject, leaf) for leaf in leaves)
            case _:
                return (cls(reason=reason, subject=repr(subject), cause=Some(error)),)
```

`Error`, `filter_with`, `to_result`, and `map_error` take the `Faults` tuple both factories return with no wrapping. `StrEnum` members format as their value, the fault from `Fault.of(Reason.ABSENT, 80)` reads `80 is absent`, and a new failure kind adds one `Reason` member with its text. `repr` quotes a text subject (`'x' is invalid`) and leaves a number bare. `caught` recurses through nested `ExceptionGroup` values from a task group or `except*` and returns one fault per leaf, each leaf held as `Some(error)` in `cause`.

## [02]-[VALIDATION]

Independent checks over one value run in full, `swap().default_value(())` yields each check's faults, and comprehension unpacking joins them:

```python
class Wire(msgspec.Struct, frozen=True, kw_only=True):
    name: str
    size: int


def named(wire: Wire, /) -> Result[str, Faults]:
    return Ok(wire.name).filter_with(str.isidentifier, partial(Fault.of, Reason.INVALID))


def sized(limit: int, wire: Wire, /) -> Result[int, Faults]:
    return Ok(wire.size).filter_with(lambda size: 0 < size <= limit, partial(Fault.of, Reason.INVALID))


def checked[T](value: T, /, *checks: Callable[[T], Result[object, Faults]]) -> Result[T, Faults]:
    faults = tuple(*check(value).swap().default_value(()) for check in checks)
    return Error(faults) if faults else Ok(value)


def accepted(taken: frozenset[str], wire: Wire, /) -> Result[Wire, Faults]:
    return (
        checked(wire, named, partial(sized, 4096))
        .filter_with(lambda live: live.name not in taken, lambda live: Fault.of(Reason.INVALID, live.name))
    )
```

Checks returning any value type join one call: `named` returns `str`, `sized` returns `int`, and covariance of `Result` in its value type fits each check to `Callable[[T], Result[object, Faults]]`. `swap()` turns a failed check into `Ok(faults)` and a passing check into an `Error` that `default_value(())` reads as no fault, and the joined faults keep check order. Checks reading a setting take it before the subject, and `partial(sized, 4096)` binds it. Steps that need every check passed chain after the fold: for `Wire(name="9x", size=0)`, `accepted` returns both faults (`'9x' is invalid`, `0 is invalid`) and never reads `taken`.

## [03]-[WORKFLOWS]

Workflows parse, validate, and look up in order, and adding a step adds one call:

```python
def port(text: str, /) -> Result[int, Faults]:
    return (
        catch(int, exception=ValueError)(text)
        .map_error(partial(Fault.caught, Reason.INVALID, text))
        .filter_with(lambda number: 0 < number < 65536, partial(Fault.of, Reason.INVALID))
    )


def service(table: Mapping[int, str], number: int, /) -> Result[str, Faults]:
    return Option.of_optional(table.get(number)).to_result(Fault.of(Reason.ABSENT, number))


def endpoint(table: Mapping[int, str], text: str, /) -> Result[str, Faults]:
    return port(text).bind(lambda number: service(table, number).map(lambda name: f"{name}:{number}"))
```

Failed steps skip every later step and return one fault: `"x"` fails at `catch` with its `ValueError` as cause, `"70000"` fails `filter_with` with the parsed number as subject, and `"22"` fails `to_result` against a table without port 22. `filter_with` builds faults from a refused value, and `to_result` takes faults built at a call with its subject in scope. Steps reading an earlier value nest inside a lambda that binds the value: `endpoint` maps the found name with `number` in scope and returns `Ok("https:443")` for `"443"`.

## [04]-[BATCHES]

Batches keep each item's `Result`, `reduce` folds them into a summary record with a step matching each `tag`, and the host maps the summary:

```python
class Summary(msgspec.Struct, frozen=True, kw_only=True):
    served: tuple[str, ...] = ()
    faults: Faults = ()

    @classmethod
    def of(cls, outcomes: Iterable[Result[str, Faults]], /) -> Summary:
        return reduce(Summary.added, outcomes, cls())

    def added(self, outcome: Result[str, Faults], /) -> Summary:
        match outcome.tag:
            case "ok":
                return msgspec.structs.replace(self, served=(*self.served, outcome.ok))
            case "error":
                return msgspec.structs.replace(self, faults=(*self.faults, *outcome.error))
```

Failed items keep their faults beside every other item's value in input order. `Summary.of(endpoint(table, text) for text in texts)` folds a generator as it yields, an empty batch returns the empty seed, and each `added` step returns a new record and leaves its predecessor unchanged.

## [05]-[HOST_TRANSLATION]

Hosts translate a result in one `match` on `tag` into their vocabulary (exit code, HTTP status, UI state, host exception):

```python
def exited(outcome: Result[int, Faults], /) -> int:
    match outcome.tag:
        case "ok":
            return outcome.ok
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 1


STATUS = frozendict({
    Reason.ABSENT: HTTPStatus.NOT_FOUND,
    Reason.INVALID: HTTPStatus.BAD_REQUEST,
    Reason.UNREADABLE: HTTPStatus.INTERNAL_SERVER_ERROR,
})
JSON = msgspec.json.Encoder()


def responded(outcome: Result[str, Faults], /) -> tuple[HTTPStatus, bytes]:
    match outcome.tag:
        case "ok":
            return HTTPStatus.OK, JSON.encode({"endpoint": outcome.ok})
        case "error":
            return max(STATUS[fault.reason] for fault in outcome.error), JSON.encode([fault.message for fault in outcome.error])


def reported(summary: Summary, /) -> int:
    sys.stdout.writelines(f"{line}\n" for line in summary.served)
    sys.stderr.writelines(f"{fault.message}\n" for fault in summary.faults)
    return 1 if summary.faults else 0
```

`exited` returns a success's value as exit code, and a failure writes each fault message to standard error and returns `1`. `STATUS` keys each reason to its status, and `responded` answers with the highest status among the faults: absent and unreadable faults together answer `500`. Every fault producer builds at least one fault, and `max` reads a non-empty tuple. `reported` maps a summary in place of one result: served lines go to standard output, faults go to standard error, and any failed item exits `1`.

## [06]-[LAWS]

Laws are equations `map` and `bind` satisfy for every value, `Error` included, and a step hiding mutation or counting calls breaks them:

```text
Functor identity:     r.map(lambda x: x) == r
Functor composition:  r.map(g).map(f) == r.map(lambda x: f(g(x)))
Monad left identity:  Ok(x).bind(f) == f(x)
Monad right identity: r.bind(Ok) == r
Monad associativity:  r.bind(f).bind(g) == r.bind(lambda x: f(x).bind(g))
```

Identity laws let `Ok` lift a value with no other effect, and associativity lets a workflow regroup its steps into named functions with the same result. Hypothesis checks the laws over generated results and steps, and a decoder's totality over generated bytes:

```python
type Step = Callable[[int], Result[int, Faults]]

outcomes: st.SearchStrategy[Result[int, Faults]] = st.one_of(
    st.integers().map(Ok),
    st.builds(Fault.of, st.sampled_from(Reason), st.text()).map(Error),
)
steps: st.SearchStrategy[Step] = st.functions(like=lambda value: value, returns=outcomes, pure=True)
wires = st.builds(Wire, name=st.text(), size=st.integers())
documents = st.recursive(st.none() | st.booleans() | st.integers() | st.text(), lambda inner: st.lists(inner) | st.dictionaries(st.text(), inner))
WIRE = msgspec.json.Decoder(Wire)


def decoded(raw: bytes, /) -> Result[Wire, Faults]:
    return catch(WIRE.decode, exception=msgspec.DecodeError)(raw).map_error(partial(Fault.caught, Reason.INVALID, Wire.__name__))


@given(outcomes)
def identity(outcome: Result[int, Faults]) -> None:
    assert outcome.map(lambda value: value) == outcome
    assert outcome.bind(Ok) == outcome


@given(outcomes)
def composition(outcome: Result[int, Faults]) -> None:
    assert outcome.map(abs).map(str) == outcome.map(lambda value: str(abs(value)))


@given(st.integers(), steps)
def left_identity(value: int, step: Step) -> None:
    assert Ok(value).bind(step) == step(value)


@given(outcomes, steps, steps)
def associativity(outcome: Result[int, Faults], first: Step, then: Step) -> None:
    assert outcome.bind(first).bind(then) == outcome.bind(lambda value: first(value).bind(then))


@given(wires)
def round_trip(wire: Wire) -> None:
    assert decoded(msgspec.json.encode(wire)) == Ok(wire)


@given(st.binary() | documents.map(msgspec.json.encode))
def total(raw: bytes) -> None:
    outcome: Result[Wire, Faults] = decoded(raw)
    assert outcome.is_ok() or {fault.reason for fault in outcome.error} == {Reason.INVALID}
```

`outcomes` yields both cases, and a strategy of `Ok` values checks the success path alone. `st.functions` with `pure=True` returns one result per argument, and `left_identity` calls `step` twice on one value. `round_trip` decodes each encoded `Wire` back to `Ok(wire)`. `total` feeds random bytes and encoded JSON documents to `decoded`: a raise fails the property, and every failure holds `Reason.INVALID`. Calling a `@given` function runs its cases and raises on a counterexample with the shrunk input.
