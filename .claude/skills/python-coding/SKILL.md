---
name: python-coding
description: "Use when writing or reviewing Python, covering functional design with result and record types"
---

# [PYTHON_CODING]

Examples use Expression, msgspec, anyio, and numpy types as standard library.

[PRINCIPLES]:
- Facts are stated once where they belong, and everything else derives from that source
- Packages and hosts are used directly through what they provide, with no layer that restates or wraps them
- Surface stays minimal inside and outside each module, and every type, export, and helper states something nothing else does
- Values that vary are parameters, and kinds, keys, and states are typed values rather than strings

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Functions as values, from partial application and dispatch to iteration, topological order, and pipelines
- [02]-[DECORATORS](references/decorators.md): Decorator factories, stacks, and dispatch registries
- [03]-[RECORDS](references/records.md): Record transitions, keyed diffs, dependency graphs, recursive unions, and array fields
- [04]-[RESULTS](references/results.md): Result accumulation, recovery, option combinators, and status mapping
- [05]-[BOUNDARIES](references/boundaries.md): Command flow from process input through streams, templates, file writes, and plans to one outcome
- [06]-[CONCURRENCY](references/concurrency.md): Async Python under anyio with tasks, deadlines, cancellation, and workers
- [07]-[NUMERICS](references/numerics.md): Numeric passes through package members, array ownership, floating-point scopes, random streams, and tables

## [01]-[FUNCTIONS]

Parameter types state every value a function accepts and return types every outcome its caller handles:
- Bounds name a `Protocol` of the operations a body reads and keep each caller's type, constraints (`[S: (str, bytes)]`) replace `AnyStr`
- `**P` captures a wrapped function's parameters, `*Ts` ties an argument tuple to them (`Callable[[*Ts], R]`), `*args: object` erases both
- `Protocol` types a callable `Callable` cannot spell (keyword parameters, `__name__`, overloads) beside the consumer reading it
- Modules pass as a `Protocol` of the members a body reads, a module constant as a `@property` member
- Subjects take positional-only `/` parameters, a value a caller varies is a keyword-only `*` parameter, required unless an owner states its default
- Choices a composition passes down are one record its caller builds once
- Members differing only in a number, flag, type, or applied function are one generic member taking it as a parameter

`Field` states the operators a formula reads, and `smoothstep` returns its argument's type (numpy scalar or array of any shape) from one body:

```python
class Field(Protocol):
    def __add__(self, other: Self | float, /) -> Self: ...
    def __mul__(self, other: Self | float, /) -> Self: ...
    def __rsub__(self, other: float, /) -> Self: ...


def smoothstep[V: Field](values: V, /) -> V:
    return values * values * (3.0 - values * 2.0)
```

### [01.1]-[TOTAL_FUNCTIONS]

Functions are total when each declared input produces a declared output, with no exception or `None` their signature omits. Functions failing on part of their input narrow it to a validated type or widen output to `Option[T]` or `Outcome[T]`:
- `int -> Tier` is incomplete when some integers fail validation, `Quantity -> Tier` over a record validating at construction covers every value
- `min`, `max`, and `next` raise on empty input, `max(items, default=...)` and `Block.of_seq(items).try_head()` make the read total
- `T | None` appears where an external API speaks it, `Option.of_optional` converts it at the read
- Types narrow through `match`, `isinstance`, or a `TypeIs` predicate
- Behavior a caller selects enters as the function it applies, a `bool` parameter holds data

### [01.2]-[PURITY]

Pure functions compute their result from arguments alone with no side effect, and the result can replace the call. Mutating a local that stays in its function is no side effect:
- Clock, randomness, and configuration enter as arguments (`def expired(now: datetime, issued: datetime, lifetime: timedelta) -> bool`)
- Module state and parameter defaults are immutable values built without I/O (`()`, `frozenset()`, `frozendict()`, `Nothing`, a frozen record)
- State a step changes returns as its next value (`(State, Event) -> State`)
- `functools.cache` memoizes a pure top-level function or a zero-argument builder, a cached method keeps every `self` it receives alive

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions take or return a function and own fixed control flow, their caller supplies the varying rule. Bind each varying part through `partial` over a top-level function:
- Parameters order for partial application: dependencies and configuration, then policies, then the subject
- Callables pass as themselves (`map(str.strip, lines)`, `reduce(Box.merged, boxes)`)
- Key functions are lambdas or unbound methods (`str.casefold`)

`read` owns the catch and its fault once, and each reader is a `partial` over the function it applies, `partial(read, Path.read_text)` for text:

```python
def read[T](reader: Callable[[Path], T], path: Path, /) -> Outcome[T]:
    return catch(reader, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))


content: Callable[[Path], Outcome[bytes]] = partial(read, Path.read_bytes)
```

## [02]-[EXPRESSIONS]

Build a returned value in one expression (a comprehension, conditional expression, or record constructor):
- Functions return a `tuple`, `frozenset`, or `frozendict` from every arm, a generator stays where laziness bounds memory
- `Option` values flatten through `to_list()` (`tuple(*o.to_list() for o in options)`), `bind(identity)` flattens a nested `Option`
- Reductions take the operator's own member: `sum(terms, start=zero)` over numbers and arrays, `math.prod`, `math.fsum`, `"".join`

Comprehension unpacking flattens each element's parts into one collection, and a walrus binds what a filter tests and the element reads:

```python
def keys(pattern: re.Pattern[str], lines: Iterable[str], /) -> frozenset[str]:
    return frozenset(*hit.groups() for line in lines if (hit := pattern.fullmatch(line.strip())))
```

### [02.1]-[PATTERN_MATCHING]

Pattern matching selects a result by a value's type, shape, or fields, and keeps extraction, guards, and results together. `toggled` derives each transition from the case it receives, with no signal naming it:

```python
type Watch = Running | Paused


def toggled(watch: Watch, at: datetime, /) -> Watch:
    match watch:
        case Running(since=since, total=total) if at > since:
            return Paused(total=total + (at - since))
        case Paused(total=total):
            return Running(since=at, total=total)
        case _:
            return watch
```

`Running` holds `since` and `total`, `Paused` holds `total`, and a toggle at or before `since` returns `watch` unchanged:
- Class patterns bind by keyword, a `kw_only` record or a pydantic model has empty `__match_args__`
- `|` alternatives share an arm when each binds the same names (`case Running(total=total) | Paused(total=total):`)
- Sequence patterns (`[first, *rest]`) bind `rest` as a `list` and match no `str` or `bytes`, `case (only,):` proves exactly one
- `case (first, *rest): return sum(rest, first)` folds a non-empty sequence with no zero sentinel
- Mapping patterns (`{"key": float(value), **rest}`) decode each key once, dotted value patterns (`Color.RED`) compare members
- Guards test what no pattern expresses, a walrus in a guard binds a derived value its arm reads
- Matches over a closed union return from each arm with no `case _:`, a guarded arm or a tuple subject ends in `case _:`
- Matches over decoded or external input end in `case _:` returning an `Error`

## [03]-[RECORDS]

Values no language or package type (`datetime`, `Path`, `ndarray`) states are frozen `msgspec.Struct` records: slotted, hashable, matchable. Fields state constraints as `Meta` bounds every decode enforces, a `type` alias names each bounded domain, and behavior is a method or property. `Ramp` bounds each stop and the stop count, checks stop order in `__post_init__`, and derives a value no field stores:

```python
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]
type Positive = Annotated[float, msgspec.Meta(gt=0.0)]


class Ramp(msgspec.Struct, frozen=True, kw_only=True):
    stops: Annotated[tuple[tuple[Unit, Positive], ...], msgspec.Meta(min_length=2)]

    def __post_init__(self) -> None:
        if any(start >= end for (start, _), (end, _) in pairwise(self.stops)):
            raise ValueError(self)

    @property
    def slopes(self) -> tuple[float, ...]:
        return tuple((high - low) / (end - start) for (start, low), (end, high) in pairwise(self.stops))

    def mirrored(self) -> Self:
        return msgspec.structs.replace(self, stops=tuple((1.0 - at, value) for at, value in reversed(self.stops)))
```

`msgspec.json.decode(raw, type=Ramp)` refuses a stop outside its bound or a lone stop with the refused key path (`$.stops[1][0]`):
- Parts a projection reads together by position are a typed tuple (`tuple[Unit, Positive]`), parts read by name are record fields
- `Meta` bounds hold at decode and `convert` alone, values built outside a decode enter through `convert`, `Annotated` parameters accept any value
- `__post_init__` raises `ValueError` for an invariant across fields and runs at construction, decode, `convert`, and `structs.replace`
- Transitions keep every invariant by construction (`mirrored` maps a unit interval onto itself), or return `Outcome[Self]`
- Fields hold frozen typed values (`tuple[T, ...]`, `frozenset[T]`, `frozendict[K, V]`, a frozen record), hashing a record hashes every field
- Absence is an `Option[T]` property over a decoded `T | None` field
- Code reads fields by attribute, `msgspec.structs.fields` and `msgspec.inspect` serve a codec alone
- `functools.cached_property` on a record needs `dict=True`
- `hash()` salts `str` and `bytes` per process, and keys that outlive a process hash a deterministic encoding
- Use `python-coding-msgspec` for msgspec records and codecs

### [03.1]-[UNIONS]

Model each state a consumer inspects as its own case, and states differing only by a value as one record of that value. Counting a type's values (a product multiplies its fields' counts, `Option[T]` adds one) exposes unneeded states. One tagged base serves every record union in a package, and a callable `tag` derives each wire tag from its class name. Operations over every case are one function with one `match`:

```python
class Case(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower): ...


class Circle(Case, frozen=True, kw_only=True):
    radius: Positive


class Rect(Case, frozen=True, kw_only=True):
    width: Positive
    height: Positive


type Shape = Circle | Rect


def area(shape: Shape, /) -> float:
    match shape:
        case Circle(radius=radius):
            return math.pi * radius**2
        case Rect(width=width, height=height):
            return width * height
```

`msgspec.json.decode(raw, type=tuple[Shape, ...])` dispatches each element by `kind` in one pass, and `math.fsum(map(area, shapes))` sums them:
- Consumers take the `Shape` alias, and a new case adds its arm to every `match` over it
- Cases extend the tagged base directly and restate `frozen=True` and `kw_only=True`
- Fields that hold meaning under one value of a status field make one case per value, each holding its own fields

### [03.2]-[DERIVATION]

Behavior derives from a declaration read once at import, callers state what the declaration cannot. Closed value sets are one enum, and each rule-derived fact is a property over a member's value:

```python
class Tier(IntEnum):
    FREE = auto()
    TEAM = auto()
    SCALE = auto()

    @property
    def quota(self) -> int:
        return 1 << (10 * self.value)
```

`Tier.TEAM.quota` is `1 << 20`, and a new member takes every fact from its rank:
- `StrEnum` with `auto()` takes each wire token from its member name, an explicit value holds a datum consumers read (a template)
- Tables take each key from the entry's own declaration: a member, a class, or a function's `__name__`
- Class facts derive at class build through `__init_subclass__` and `__set_name__`

## [04]-[RESULTS]

Fallible functions return a result type, one type per concern:

| [INDEX] | [TYPE]       | [CONCERN]                                          |
| :-----: | :----------- | :------------------------------------------------- |
|  [01]   | `Option[T]`  | Absence without a reason                           |
|  [02]   | `Outcome[T]` | Expected failure with reasons, first failure stops |

Input boundaries select the result type and functions keep it. One boundary converts it through the member named for its target (`to_result_with`, `to_option`, `Option.of_optional`, `to_optional`):
- `default_value` and `default_with` supply a default the domain defines for absence or failure
- Empty `tuple` is a result, `Option[tuple[T, ...]]` exists only where a consumer treats no collection and an empty one apart
- Operational failures stay `Error` through every producer, `Nothing` holds absence alone
- Use `python-coding-expression` for expression members

### [04.1]-[ERRORS]

Exceptions mark a violated precondition or a third-party raise a boundary converts. One `Fault` record serves every package, its `reason` an enum member holding the message template. `Fault.of` returns the non-empty tuple every fault builder needs:

```python
type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


class Reason(StrEnum):
    ABSENT = "{subject} holds no {member}"
    INVALID = "{subject} is invalid. {cause}"
    OUTSIDE = "{subject} accepts {member}, not {cause}"
    UNREADABLE = "{subject} is unreadable. {cause}"
    UNWRITABLE = "{subject} is unwritable. {cause}"
    EXPIRED = "{subject} exceeded {cause}"
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
- `partial(Fault.of, <reason>, <subject>)` serves `map_error` and `filter_with` with the refused value as `cause`, and `to_result_with` with none
- Consumers classify a fault by its `reason` member or by a class pattern on its `cause`
- Suppressed or warned conditions become an `Error` or a `Result` holding the degraded value

`catch(f, exception=E)(arguments)` from `expression.extra.result` covers one raising call and passes the callable itself. `ran` applies one fault builder to a failed launch and a failed exit, and a failed child's `CompletedProcess` stays as cause:

```python
@catch(exception=OSError)
def _launched(argv: tuple[str, ...], /) -> subprocess.CompletedProcess[str]:
    return subprocess.run(argv, capture_output=True, text=True, check=False)


def ran(argv: tuple[str, ...], /) -> Outcome[str]:
    failed = partial(Fault.of, Reason.FAILED, argv[0])
    return _launched(argv).map_error(failed).filter_with(lambda done: done.returncode == 0, failed).map(lambda done: done.stdout)
```

`catch` names the exception its call's contract raises and erases parameters to `...`. Overloaded calls take a typed `def` under `@catch` that fixes the return type.

### [04.2]-[COMPOSITION]

Give each step a function, and select its operator by signature and by whether steps depend on each other:

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

Checks reading a value alone are `Meta` bounds or `__post_init__` on its record, and chains hold checks reading a runtime value (a limit, a held set). Independent checks combine through `paired`:

```python
def accepted(limit: int, taken: frozenset[str], entry: Entry, /) -> Outcome[Entry]:
    return paired(
        Ok(entry.size).filter_with(lambda size: size <= limit, partial(Fault.of, Reason.OUTSIDE, "size", member=f"at most {limit}")),
        Ok(entry.name).filter_with(lambda name: name not in taken, partial(Fault.of, Reason.OUTSIDE, "name", member="an unused name")),
    ).map(lambda _: entry)
```

`Entry` is a record of `name` and `size`, and an entry over the limit with a taken name fails with both faults:
- `reduce(lambda held, item: held.bind(lambda state: step(state, item)), items, Ok(seed))` folds dependent steps until the first refused item
- Steps a chain binds are named functions declared `-> Outcome[T]`
- Checks one member states run through it (`filter_with`, `to_result_with`, `Some(x).filter`)

### [04.3]-[MATCHING]

Values stay inside a result through a pipeline, and the entry point reads it once through `match`. `Result(tag="ok", ok=<name>)` binds the value and `Result(error=<name>)` the faults, an exhaustive pair:

```python
EXITS: frozendict[StrEnum, int] = frozendict({
    Reason.ABSENT: os.EX_DATAERR,
    Reason.INVALID: os.EX_DATAERR,
    Reason.OUTSIDE: os.EX_DATAERR,
    Reason.UNREADABLE: os.EX_NOINPUT,
    Reason.UNWRITABLE: os.EX_CANTCREAT,
    Reason.EXPIRED: os.EX_TEMPFAIL,
    Reason.FAILED: os.EX_UNAVAILABLE,
})


def exited(outcome: Outcome[int], /) -> int:
    match outcome:
        case Result(tag="ok", ok=code):
            return code
        case Result(error=faults):
            sys.stderr.writelines(f"{fault.message}\n" for fault in faults)
            return EXITS[faults[0].reason]
```

`EXITS` maps every reason the command's packages raise to its `sysexits` status, the first fault exits with its reason's status, and a reason with no row raises `KeyError` as a defect.

## [05]-[BOUNDARIES]

### [05.1]-[CODECS]

Decode external bytes or builtins in one typed call through `msgspec.<format>.decode(raw, type=T)` or `msgspec.convert`. Module-level codecs hold options those functions lack (`dec_hook`, `strict=False`). `TypeForm[T]` parameters accept any type form, a `type` alias over `Annotated` included, and type the result as `T`:

```python
def stored[T](form: TypeForm[T], path: Path, /) -> Outcome[T]:
    return content(path).bind(lambda raw: catch(msgspec.json.decode, exception=msgspec.DecodeError)(raw, type=form).map_error(partial(Fault.of, Reason.INVALID, str(path))))
```

`stored(Ports, path)` over `type Ports = tuple[Annotated[int, msgspec.Meta(ge=1, le=65535)], ...]` reads as `Outcome[tuple[int, ...]]`:
- `partial(stored, Ports)` is the reader of one file kind
- `DecodeError` covers malformed input and its subclass `ValidationError` a schema mismatch
- Wire records apart from domain records exist only where the wire shape differs (names, units, nesting)
- Files a program owns are read by key path at their use site, schemas validate input a program does not own
- Text with a grammar parses through its grammar's parser (`tomllib`, `ast`, `string.Formatter().parse`, a markdown parser)
- Use `python-coding-pydantic` for validation that reports every error at once or publishes a schema

### [05.2]-[RESOURCES]

Inject the narrowest dependency: a value for a snapshot, a function for on-demand work, a `Protocol` for an interface each platform implements in full. Scopes yield their acquisition's result to their body:

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
- `@contextmanager` and `@asynccontextmanager` return `Generator[T]` or `AsyncGenerator[T]`, and `ExitStack` owns every release
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
- Blocking calls inside a coroutine take their anyio member (`anyio.Path`, `anyio.open_file`, `anyio.run_process`, `anyio.sleep`)

### [05.4]-[TIME]

Instants are aware `datetime` values and durations are `timedelta` values, in fields, parameters, and arithmetic alike. `Instant` refuses a naive value at decode, and `Lease` computes from a `now` its caller reads once:

```python
type Instant = Annotated[datetime, msgspec.Meta(tz=True)]


class Lease(msgspec.Struct, frozen=True, kw_only=True):
    issued: Instant
    lifetime: timedelta

    def left(self, now: datetime, /) -> timedelta:
        return max(self.issued + self.lifetime - now, timedelta())
```

`Lease` decodes `lifetime` from an ISO 8601 duration (`PT1H`):
- Fields and parameters hold a `timedelta` in place of a number with a unit suffix, `delta / timedelta(seconds=1)` hands a numeric API its `float`
- `delta // step` counts whole steps and `delta % step` returns the rest, exact to the microsecond
- Datetimes sharing one `ZoneInfo` subtract as wall clocks (`1 day` across a 23-hour DST day), elapsed spans subtract in UTC
- Elapsed intervals read a monotonic clock (`time.monotonic_ns()`, `anyio.current_time()`) no wall clock adjustment moves

## [06]-[NUMERICS]

Numeric facts come from the package that states them, and array code runs whole-array operations:
- Specialized members state a composed operation once (`np.vecdot(rows, rows)` for each row's squared norm)
- Arrays build once (`np.fromiter(items, dtype, count=n)`, one `np.concatenate`)

## [07]-[MODULES]

Each module holds one owner, and imports point down its layers:
- Imports a module body does not evaluate at load are `lazy`, names a class body, base, decorator, or record annotation reads stay eager
- `__all__` lists what importers compose, and names one module reads alone take a `_` prefix
- Unused public capability stays exported
- Files existing to break an import cycle merge, and the cycle goes at its root
- Annotations stay unquoted, runtime readers call `annotationlib.get_annotations`
- Entry modules hold boundary code alone in import-safe top-level code, `to_process` and `to_interpreter` workers re-import them
- Use `python-coding-cyclopts` for command-line applications
- Use `python-document` for docstrings
