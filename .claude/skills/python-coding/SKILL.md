---
name: python-coding
description: "Use when writing or reviewing Python, covering functional design with result and record types"
---

# [PYTHON_CODING]

Covers Python design decisions from signature shape to host boundary in the newest Python forms. Expression supplies results and options, msgspec records and codecs, and anyio concurrency.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Worked flows for specialization, operator families, dispatch, iteration, pipelines, and strategies
- [02]-[DECORATORS](references/decorators.md): Worked flows for wrappers, factories, derived callables, class hooks, registries, and decorator order
- [03]-[RECORDS](references/records.md): Worked flows for records from decode to transition, with generic and recursive unions, graphs, and arrays
- [04]-[RESULTS](references/results.md): Worked flows for accumulation, recovery, options, host translation, and result equations
- [05]-[BOUNDARIES](references/boundaries.md): Worked flows for one entry from argv through codecs, templates, resources, and plans to an exit code
- [06]-[CONCURRENCY](references/concurrency.md): Worked flows for anyio tasks returning results, with deadlines, cancellation, and workers
- [07]-[NUMERICS](references/numerics.md): Worked flows for whole-array passes, buffer ownership, floating-point scopes, random streams, and tables

## [01]-[FUNCTIONS]

Function signatures are contracts, parameter types describe every value a function accepts and its return type every outcome a caller must handle:
- Type parameters sit on their declaration with inferred variance (`def first[T]`, `class Box[T]`, `type Pair[T] = tuple[T, T]`)
- Bounds name a `Protocol` of the operations a body reads and keep each caller's type, constraints (`[S: (str, bytes)]`) replace `AnyStr`
- `**P` captures a wrapped function's parameters, `*Ts` ties an argument tuple to them (`Callable[[*Ts], R]`), `*args: object` erases both
- Parameters take the narrowest ABC a body reads (`Iterable`, `Collection`, `Sequence`, `Mapping`), return types stay concrete
- `Protocol` types a callable `Callable` cannot spell (keyword parameters, `__name__`, overloads) beside the consumer reading it
- Modules pass as a `Protocol` of the members a body reads, both checkers match it member by member, and ty matches a module constant to a `@property` alone
- Subjects take positional-only `/` parameters, options and flags take keyword-only `*` parameters
- Members differing only in a number, flag, type, or applied function are one generic member taking it as a parameter

`Field` states the operators a formula reads, and `smoothstep` returns its argument's own type, a numpy scalar or an array of any shape, in place of one overload per numeric kind:

```python
class Field(Protocol):
    def __add__(self, other: Self | float, /) -> Self: ...
    def __mul__(self, other: Self | float, /) -> Self: ...
    def __rsub__(self, other: float, /) -> Self: ...


def smoothstep[V: Field](values: V, /) -> V:
    return values * values * (3.0 - values * 2.0)
```

`ty check` reads a `float` argument as `float | int` and refuses it against an operator protocol.

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, with no exception or `None` their signature omits. Correct a dishonest contract by narrowing its input to a validated type or widening its output to `Option[T]` or `Outcome[T]`:
- `int -> Tier` is incomplete when some integers fail validation, `Quantity -> Tier` over a record validating at construction covers every value
- `Annotated` constraints narrow nothing for a checker, a parameter typed `Annotated[int, Meta(ge=1)]` accepts every `int`
- `min`, `max`, and `next` raise on empty input, `max(items, default=...)` and `Block.of_seq(items).try_head()` make the read total, and `Option.of_optional(next(..., None))` restates the second
- `T | None` appears where an external API speaks it, `Option.of_optional` converts it at the read
- Types narrow through `match`, `isinstance`, or a `TypeIs` predicate, `TypeGuard` narrows the true branch alone
- Boolean parameters that switch behavior become separate functions or a union case parameter

### [01.2]-[PURITY]

Functions are pure when their return value depends on their arguments alone and evaluation causes no side effect, and a pure call's result replaces the call. Side effects are mutating state visible outside a function, mutating an argument, raising, and I/O (clock, environment, filesystem, network, randomness, another process). Mutation of a local that never escapes is no side effect:
- Clock, randomness, and configuration enter as arguments (`def expired(now: datetime, issued: datetime, lifetime: timedelta) -> bool`)
- Module state and parameter defaults are immutable values (`()`, `frozenset()`, `frozendict()`, `Nothing`, a frozen record)
- State a step changes returns as its next value (`(State, Event) -> State`), a `nonlocal` cell or a list closures append to is shared state
- `functools.cache` memoizes a pure top-level function or a zero-argument builder, a cached method keeps every `self` it receives alive

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or both, and own stable control flow while their caller supplies the varying rule. Bind each varying part through `partial` over a top-level function:
- Parameters order for partial application: dependencies and configuration, then policies, then the subject
- Closures returning a closure become a top-level function bound through `partial`
- Callables built per loop item bind it through `partial(f, item)`, a lambda reads the loop variable's last value
- Callables pass as themselves (`map(str.strip, lines)`, `reduce(Box.merged, boxes)`), a lambda that forwards its argument restates them
- Key functions are lambdas or methods, typeshed types `operator.attrgetter` and `itemgetter` results as `Any`
- `pipe(value, f, g)` runs unary steps, `compose(f, g)` names a reusable pipeline, `Result` and `Option` chains stay fluent

`_read` owns the catch and its fault once, and each reader is a `partial` over the unbound method it applies. `Fault`, `Outcome`, and `Reason` are the package faults:

```python
def _read[T](reader: Callable[[Path], T], path: Path, /) -> Outcome[T]:
    return catch(reader, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))


content: Callable[[Path], Outcome[bytes]] = partial(_read, Path.read_bytes)
text: Callable[[Path], Outcome[str]] = partial(_read, Path.read_text)
listing: Callable[[Path], Outcome[tuple[Path, ...]]] = partial(_read, lambda folder: tuple(folder.iterdir()))
```

## [02]-[EXPRESSIONS]

Expressions evaluate to values, and statements perform an action or control execution. Build a returned value in one expression, a comprehension, conditional expression, or record constructor:
- Functions return a `tuple`, `frozenset`, or `frozendict` from every arm, a generator stays where laziness bounds memory
- `Option` values flatten through `to_list()` (`tuple(*o.to_list() for o in options)`), `bind(identity)` flattens a nested `Option`
- `dict.fromkeys(items)` keeps first occurrences in order, a `seen` set beside a loop restates it
- Reductions take the operator's own member: `sum(terms, start=zero)` over numbers and arrays, `math.prod`, `math.fsum`, `"".join`

Comprehension unpacking flattens each display kind, a later mapping key wins a merge, and a walrus binds what a filter tests and the element reads:

```python
def merged(layers: Iterable[Mapping[str, str]], /) -> frozendict[str, str]:
    return frozendict({**layer for layer in layers})


def flags(table: Mapping[str, str], /) -> tuple[str, ...]:
    return tuple(*(f"--{key}", value) for key, value in table.items())


def keys(pattern: re.Pattern[str], lines: Iterable[str], /) -> frozenset[str]:
    return frozenset(*hit.groups() for line in lines if (hit := pattern.fullmatch(line.strip())))
```

Calls without the `for` (`frozenset(*groups)`) pass each group as its own argument.

### [02.1]-[PATTERN_MATCHING]

Pattern matching selects a result from a value's type, shape, or fields, and keeps recognition, extraction, guards, and results together. `advanced` is one transition over a pair, and `|` alternatives share an arm when they bind the same names:

```python
type Phase = Idle | Running | Done
type Event = Started | Stopped


def advanced(phase: Phase, event: Event, /) -> Phase:
    match phase, event:
        case Idle() | Done(), Started(at=at):
            return Running(since=at)
        case Running(since=since), Stopped(at=at) if at >= since:
            return Done(took=at - since)
        case _:
            return phase
```

Each phase and event is a frozen record, and patterns read them by form:
- Class patterns bind by keyword, a `kw_only` record or a pydantic model has empty `__match_args__`
- Sequence patterns (`[first, *rest]`) bind `rest` as a `list` and never match `str` or `bytes`, `case (only,):` proves exactly one
- `case (first, *rest): return sum(rest, first)` folds a non-empty sequence with no zero sentinel
- Mapping patterns (`{"key": float(value), **rest}`) decode each key once, dotted value patterns (`Color.RED`) compare members
- Guards test what no pattern expresses, a walrus in a guard binds a derived value its arm reads
- Matches returning from each arm over a closed union prove coverage, mypy and ty report a missing case as an implicit `None` return
- Matches over a tuple subject end in `case _:`, mypy proves no coverage of a tuple and reports a missing return
- Matches over decoded or host input end in `case _:` returning an `Error`

## [03]-[RECORDS]

Records are frozen `msgspec.Struct` classes: slotted, hashable, matchable, and cheap to build. Constraint aliases state a domain once, and behavior on a record is its method or property. `Ramp` bounds each stop and its stop count through `Meta`, checks stop order in `__post_init__`, and derives a value no field stores:

```python
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
type Positive = Annotated[float, msgspec.Meta(gt=0.0)]


class Stop(msgspec.Struct, frozen=True, array_like=True):
    at: Unit
    value: Positive


class Ramp(msgspec.Struct, frozen=True, kw_only=True):
    stops: Annotated[tuple[Stop, ...], msgspec.Meta(min_length=2)]

    def __post_init__(self) -> None:
        if any(left.at >= right.at for left, right in pairwise(self.stops)):
            raise ValueError(self)

    @property
    def slopes(self) -> tuple[float, ...]:
        return tuple((right.value - left.value) / (right.at - left.at) for left, right in pairwise(self.stops))

    def mirrored(self) -> Self:
        return msgspec.structs.replace(self, stops=tuple(Stop(at=1.0 - stop.at, value=stop.value) for stop in reversed(self.stops)))


RAMP = msgspec.json.Decoder(Ramp)
```

`RAMP.decode(b'{"stops": [[0.0, 1.0], [1.5, 2.0]]}')` fails with `` Expected `float` <= 1.0 - at `$.stops[1][0]` ``, one stop fails with `` Expected `array` of length >= 2 - at `$.stops` ``, and `array_like=True` reads each stop as a pair:
- `Meta` bounds hold at decode and `convert` alone, values from outside a decode enter through `convert`
- `__post_init__` raises `ValueError` for an invariant across fields and runs at construction, decode, `convert`, and `structs.replace`
- Transitions keep every invariant by construction (`mirrored` maps a unit interval onto itself), or return `Outcome[Self]`
- `structs.replace` types as the record under mypy and ty, `copy.replace` fails mypy over `Self` or a union
- Fields hold frozen typed values (`tuple[T, ...]`, `frozenset[T]`, `frozendict[K, V]`, a frozen record), hashing a record hashes every field
- Length bounds on a collection field are `Meta(min_length=n)`, msgspec decodes `tuple[T, *tuple[T, ...]]` as a pair of `T` and a tuple
- Absence is an `Option[T]` property over a decoded `T | None` field
- Code reads fields by attribute, `msgspec.structs.fields` and `msgspec.inspect` serve a codec alone
- `functools.cached_property` on a record needs `dict=True`, a slotted record raises `TypeError` at its first read
- `hash()` salts `str` and `bytes` per process, and keys that outlive a process hash a deterministic encoding
- Use `python-coding-msgspec` for msgspec records and codecs

### [03.1]-[UNIONS]

Model each valid state a consumer inspects as a distinct case, and states that differ only by a value as one record of that value. Counting values a type admits (a product multiplies its fields' counts, `Option[T]` adds one) exposes unneeded states. One tagged base serves every union of a package, a callable `tag` derives each wire tag from its class name, and each case owns its behavior:

```python
class Case(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower): ...


class Circle(Case, frozen=True, kw_only=True):
    radius: Positive

    @property
    def area(self) -> float:
        return math.pi * self.radius**2


class Rect(Case, frozen=True, kw_only=True):
    width: Positive
    height: Positive

    @property
    def area(self) -> float:
        return self.width * self.height


type Shape = Circle | Rect

SHAPES = msgspec.json.Decoder(tuple[Shape, ...])


def covered(shapes: Iterable[Shape], /) -> float:
    return math.fsum(shape.area for shape in shapes)
```

`SHAPES` dispatches each element by its `kind` key in one pass, and `covered` reads `area` from every case without a `match`:
- Consumers take the `Shape` alias, the base declares no `area`, and a `match` over the alias proves coverage
- Cases restate `frozen=True` and `kw_only=True`, checkers reject a frozen base under an unfrozen case
- Cases extend the tagged base directly, and a base listing `ABC` beside `msgspec.Struct` fails with a metaclass conflict
- `Literal` status fields with fields that hold meaning under one value become one case per value
- `tag_field` names a wire key an external format fixes, `tag="<value>"` matches one external value
- Enum members name a closed set of values with no data beyond their value, `StrEnum` for wire values, `IntEnum` for an order

### [03.2]-[DERIVATION]

Decorators, `__init_subclass__`, and `__set_name__` read a declaration once at import and derive behavior from it in place of a metaclass, callers state what the declaration cannot. Decorators that are generic records hold their function as `__wrapped__`, `**P` keeps its signature for checkers, and `inspect.signature` follows it at runtime. Registries are one frozen table over the namespace, each value narrowed through `isinstance`:

```python
class Body[**P, R](Protocol):
    __name__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> R: ...


class Command[**P, R](msgspec.Struct, frozen=True):
    __wrapped__: Body[P, R]

    @property
    def key(self) -> str:
        return self.__wrapped__.__name__.replace("_", "-")

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> R:
        return self.__wrapped__(*args, **kwargs)


@Command
def build_all(target: str, /, *, clean: bool = False) -> int:
    return 0 if clean else len(target)


COMMANDS: frozendict[str, Command[[str], int]] = frozendict({entry.key: entry for entry in globals().values() if isinstance(entry, Command)})
```

Calls through a record check against `P`, and `build_all(1)` or a second positional argument fails mypy and ty:
- `Body` gives ty the `__name__` no `Callable` declares, and the table type names the signature every entry shares
- Declaration defects raise `TypeError` at decoration or class build, before first use

## [04]-[RESULTS]

Every fallible function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]       | [CONCERN]                                          |
| :-----: | :----------- | :------------------------------------------------- |
|  [01]   | `Option[T]`  | Absence without a reason                           |
|  [02]   | `Outcome[T]` | Expected failure with reasons, first failure stops |

Input boundaries select the result type, functions preserve it, and conversion happens at one named boundary through the member named for its target (`to_result_with`, `to_option`, `Option.of_optional`, `to_optional`):
- `default_value` and `default_with` supply a default the domain defines for absence or failure
- Empty `tuple` is a result and not absence, `Option[tuple[T, ...]]` exists only where a consumer treats no collection and an empty one apart
- Producers that map an operational failure to `Nothing` hide it from their consumer
- Truthiness means nothing on `Result` and `Option` (`bool(Nothing)` is `True`), `is_ok()` and `is_none()` read the case
- Use `python-coding-expression` for expression members

### [04.1]-[ERRORS]

Exceptions mark a violated precondition, a declaration defect at import, or a third-party raise a boundary converts. `Reason` values are message templates, and `Fault.of` returns the non-empty tuple every fault builder needs:

```python
type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class Reason(StrEnum):
    ABSENT = "{subject} holds no {member}"
    INVALID = "{subject} is invalid. {cause}"
    OUTSIDE = "{subject} accepts {member}, not {cause}"
    UNREADABLE = "{subject} is unreadable. {cause}"
    FAILED = "{subject} failed. {cause}"


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    reason: StrEnum
    subject: str
    member: str = ""
    cause: object = ""

    @property
    def message(self) -> str:
        return self.reason.format(subject=self.subject, member=self.member, cause=self.cause)

    @classmethod
    def of(cls, reason: StrEnum, subject: str, cause: object = "", /, *, member: str = "") -> Faults:
        return (cls(reason=reason, subject=subject, member=member, cause=cause),)
```

`reason` takes a member of any package's reason enum, and a new failure kind adds one member with its template:
- `subject` names the refused value, `member` the member or bound it misses, and a caught exception stays as `cause`
- `partial(Fault.of, <reason>, <subject>)` serves `map_error`, `filter_with`, and `to_result_with` alike, the refused value becoming `cause`
- Consumers classify a fault by its `reason` member or by a class pattern on its `cause`
- Suppressed or warned conditions become an `Error` or a `Result` holding the degraded value
- Preconditions raise explicitly, `python -O` strips every `assert`

`catch(f, exception=E)(arguments)` from `expression.extra.result` covers one raising call and passes the callable itself. `ran` binds its fault builder once and keeps the `CalledProcessError` holding a child's status as cause:

```python
@catch(exception=OSError)
def _launched(argv: tuple[str, ...], /) -> subprocess.CompletedProcess[str]:
    return subprocess.run(argv, capture_output=True, text=True, check=False)


def ran(argv: tuple[str, ...], /) -> Outcome[str]:
    failed = partial(Fault.of, Reason.FAILED, argv[0])
    return (
        _launched(argv)
        .map_error(failed)
        .bind(lambda done: catch(done.check_returncode, exception=subprocess.CalledProcessError)().map(lambda _: done.stdout).map_error(failed))
    )
```

Each `catch` names the exception its call's contract raises:
- `catch` erases parameters to `...` and an overloaded return to mypy's first overload or ty's union, a typed `def` under `@catch` keeps both
- `catch` over an `async def` or a generator function returns `Ok(<coroutine>)` and catches nothing raised later
- Lambdas building fault tuples type them as `tuple[Fault]`, and a later `bind` to a step returning `Outcome` fails both checkers

### [04.2]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether steps depend on each other:

| [INDEX] | [SIGNATURE]                 | [OPERATOR]              | [BEHAVIOR]                                           |
| :-----: | :-------------------------- | :---------------------- | :--------------------------------------------------- |
|  [01]   | `T -> R`                    | `.map`                  | Transforms a present value, keeps the result type    |
|  [02]   | `T -> bool` on `Result`     | `.filter_with`          | Keeps a passing value, else fails with built faults  |
|  [03]   | `T -> bool` on `Option`     | `.filter`               | Keeps a passing value, else empties it               |
|  [04]   | `T -> Result[R, E]`         | `.bind`                 | Continues with a dependent step, first failure stops |
|  [05]   | `E -> F`                    | `.map_error`            | Translates a failure into package faults             |
|  [06]   | `E -> Result[T, E]`         | `.or_else_with`         | Recovers one failure shape, else keeps the faults    |
|  [07]   | `Option[T] -> Result[T, E]` | `.to_result_with(f)`    | Promotes absence where its reason is known           |
|  [08]   | Independent results         | `paired`, `accumulated` | Keeps every value, or every fault in input order     |

Independent checks run as separate results and combine through `paired`, a step needing every check passed chains after the combination, and dependent steps over items fold through `reduce` over `.bind`:

```python
def named(entry: Entry, /) -> Outcome[str]:
    return Ok(entry.name).filter_with(str.isidentifier, partial(Fault.of, Reason.OUTSIDE, "name", member="an identifier"))


def sized(limit: int, entry: Entry, /) -> Outcome[int]:
    return Ok(entry.size).filter_with(lambda size: 0 < size <= limit, partial(Fault.of, Reason.OUTSIDE, "size", member=f"1 to {limit}"))


def accepted(limit: int, taken: frozenset[str], entry: Entry, /) -> Outcome[Entry]:
    return (
        paired(named(entry), sized(limit, entry))
        .map(lambda _: entry)
        .filter_with(lambda held: held.name not in taken, lambda held: Fault.of(Reason.OUTSIDE, "name", held.name, member="an unused name"))
    )


def folded[S, T](step: Callable[[S, T], Outcome[S]], seed: S, items: Iterable[T], /) -> Outcome[S]:
    return reduce(lambda held, item: held.bind(lambda state: step(state, item)), items, Ok(seed))
```

`Entry` is a record of `name` and `size`. `Entry(name="9x", size=0)` fails with both faults and never reads `taken`, and `folded` stops at the first refused item and returns `seed` for none:
- Steps a chain binds are named functions declared `-> Outcome[T]`, a lambda returning `Ok(...) if c else Error(...)` widens to `Any`
- Steps one member states (`filter_with`, `to_result_with`, `Some(x).filter`) replace a ternary or `bind` rebuilding `Ok` or `Error`
- Values stay inside a result through a pipeline, an unwrap mid-pipeline followed by a rewrap duplicates its failure path

### [04.3]-[MATCHING]

Hosts read a result once through `match` on its `tag`, each narrowed literal proves its case and the value reads as `ok` or `error`:

```python
def exited(outcome: Outcome[int], /) -> int:
    match outcome.tag:
        case "ok":
            return outcome.ok
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 1
```

`Ok`, `Error`, and `Some` are functions, `case Ok(value)` raises `TypeError`, and `case Result(tag="ok", ok=value)` binds without proving coverage. Hosts that read a command's return value read an unmapped `Error` as success.

## [05]-[BOUNDARIES]

### [05.1]-[CODECS]

Decode external bytes or builtins into a record in one typed call through a module-level codec or `msgspec.convert`. Generic records holding a `TypeForm[T]` decode any type form, a `type` alias over `Annotated` included, and both checkers read the result as `T`:

```python
class Stored[T](msgspec.Struct, frozen=True, kw_only=True):
    path: Path
    form: TypeForm[T]

    def read(self) -> Outcome[T]:
        return content(self.path).bind(lambda raw: catch(msgspec.json.decode, exception=msgspec.DecodeError)(raw, type=self.form).map_error(partial(Fault.of, Reason.UNREADABLE, str(self.path))))
```

`Stored(path=<path>, form=Ports)` over `type Ports = tuple[Annotated[int, msgspec.Meta(ge=1, le=65535)], ...]` reads as `Outcome[tuple[int, ...]]`, and a file holding `[0]` fails with `` Expected `int` >= 1 - at `$[0]` ``:
- `Decoder` keeps the type of a class or `tuple[...]` argument, an alias argument types it `Decoder[Any]` until an annotation on the codec names the type
- `DecodeError` covers malformed input and its subclass `ValidationError` a schema mismatch
- Wire records apart from domain records exist only where the wire shape differs (names, units, nesting)
- Files a program owns are read by key path at their use site, schemas validate input a program does not own
- Text with a grammar parses through its grammar's parser (`tomllib`, `ast`, `string.Formatter().parse`, a markdown parser)
- Use `python-coding-pydantic` for validation that reports every error at once or publishes a schema

### [05.2]-[RESOURCES]

Inject the narrowest dependency that represents what consumers need: a value for a stable snapshot, a function for an operation that runs on demand, and a `Protocol` for an interface each platform implements in full. Scopes yield their acquisition's result to their body:

```python
@catch(exception=OSError)
def _entered(stack: ExitStack, paths: Sequence[Path], /) -> tuple[BufferedReader, ...]:
    return tuple(stack.enter_context(path.open("rb")) for path in paths)


@contextmanager
def opened(paths: Sequence[Path], /) -> Generator[Outcome[tuple[BufferedReader, ...]]]:
    with ExitStack() as stack:
        yield _entered(stack, paths).map_error(partial(Fault.of, Reason.UNREADABLE, ", ".join(map(str, paths))))
```

Failed opens release every stream opened before them at scope exit, and the body receives one fault in place of an exception:
- `@contextmanager` and `@asynccontextmanager` return `Generator[T]` or `AsyncGenerator[T]`, and `ExitStack` replaces `try` with `finally`
- Commits are a step after the work, outside its release
- Use `python-coding-telemetry` for logs, traces, and measurements

### [05.3]-[CONCURRENCY]

Concurrent regions are anyio task groups, tasks return results that cancel no sibling on an expected failure, and exception groups hold defects. `catch` and `bind` span no `await`, and an `async def` holds an awaited raising call in `try` and returns `Error` from its handler:

```python
async def loaded(path: anyio.Path, /) -> Outcome[bytes]:
    try:
        data = await path.read_bytes()
    except OSError as error:
        return Error(Fault.of(Reason.UNREADABLE, str(path), error))
    return Ok(data)
```

`await anyio.amap(loaded, paths)` returns each path's outcome in input order, a missing file's fault beside its siblings' bytes:
- `anyio.gather` returns a typed tuple, and a task handle one task's value
- Blocking standard library calls take the anyio member ruff's `ASYNC` rules name, other blocking native calls run through `to_thread.run_sync`
- CPU-bound pure Python runs through `to_process` or `to_interpreter`

### [05.4]-[TIME]

Instants are aware `datetime` values and durations are `timedelta` values, in fields, parameters, and arithmetic alike. `Instant` refuses a naive value at decode, and `Lease` computes from a `now` its caller reads once:

```python
type Instant = Annotated[datetime, msgspec.Meta(tz=True)]


class Lease(msgspec.Struct, frozen=True, kw_only=True):
    issued: Instant
    lifetime: timedelta

    def left(self, now: datetime, /) -> timedelta:
        return max(self.issued + self.lifetime - now, timedelta())

    def renewals(self, step: timedelta, now: datetime, /) -> int:
        return self.left(now) // step
```

`{"issued": "2026-03-07T12:00:00", "lifetime": "PT1H"}` fails with `` Expected `datetime` with a timezone component - at `$.issued` ``:
- Clock reads take `datetime.now(UTC)`, and comparing a naive with an aware value raises `TypeError`
- Numbers suffixed `_ms` or `_seconds` restate a unit `timedelta` holds, `delta / timedelta(seconds=1)` hands a numeric API its `float`
- `delta // step` counts whole steps and `delta % step` returns the rest, exact to the microsecond
- Datetimes sharing one `ZoneInfo` subtract as wall clocks, across a DST change `later - start` reads `1 day` for 23 elapsed hours, and elapsed spans subtract in UTC
- Elapsed intervals read a monotonic clock (`time.monotonic_ns()`, `anyio.current_time()`) no wall clock adjustment moves

## [06]-[NUMERICS]

Numeric facts come from the package that states them, and array code runs whole-array operations. Floating-point scopes that raise are `np.errstate` on one named `def`, and `@catch` above it converts `FloatingPointError` into a `Result` with no `with` block or flag:

```python
@catch(exception=FloatingPointError)
@np.errstate(divide="raise", invalid="raise")
def _ratios(numerator: Samples, denominator: Samples, /) -> Samples:
    return numerator / denominator


def efficiency(output: Samples, supply: Samples, /) -> Outcome[Samples]:
    return _ratios(output, supply).map_error(partial(Fault.of, Reason.INVALID, "supply"))
```

`Samples` is `npt.NDArray[np.float64]`, and each call runs inside a fresh scope that leaves `np.geterr()` unchanged:
- Roots, quadrature, interpolation, fits, and factorizations are scipy and numpy members, a hand loop that converges is a defect
- One specialized operation replaces composed general ones (`np.dot(v, v)` over `np.sum(v * v)`, `np.add.at` over a scatter loop)
- Arrays build once (`np.fromiter(items, dtype, count=n)`, one `np.concatenate`), each `np.append` copies the whole array
- Module-level arrays call `setflags(write=False)` once built

## [07]-[MODULES]

Each module holds one owner, and imports point down its layers:
- Imports a module body does not evaluate at load are `lazy`, names a class body, base, decorator, or record annotation reads stay eager
- `__all__` lists what importers compose, and names one module reads alone take a `_` prefix
- Unused public capability stays exported
- Files existing to break an import cycle merge, and the cycle goes at its root
- Annotations stay unquoted with no `from __future__ import annotations`, runtime readers call `annotationlib.get_annotations`
- Entry modules hold host translation alone with import-safe top-level code, process and interpreter workers re-import them (`__mp_main__` under `to_process`)
- Use `python-coding-cyclopts` for command-line applications
- Code an older interpreter imports keeps its forms: `MappingProxyType` for `frozendict`, eager imports, nested comprehensions
- Use `python-document` for docstrings
