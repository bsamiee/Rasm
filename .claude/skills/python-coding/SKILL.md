---
name: python-coding
description: "Use when writing or reviewing Python, covering functional design with result and record types"
---

# [PYTHON_CODING]

Covers Python design decisions under workspace standards (TOTALITY, PURITY, BOUNDARY), from signature shape to host boundary. Expression supplies results and options, msgspec supplies records and codecs, and every example uses their types as vocabulary.

## [01]-[FUNCTIONS]

Function signatures are contracts, parameter types describe every value a function accepts and its return type every outcome a caller must handle. Arrow notation writes `()` for no input or no returned information, `Callable` states the signature in code:

| [INDEX] | [PURPOSE]                   | [ARROW_NOTATION]            | [ANNOTATION]                                 |
| :-----: | :-------------------------- | :-------------------------- | :------------------------------------------- |
|  [01]   | Transform a value           | `int -> str`                | `Callable[[int], str]`                       |
|  [02]   | Produce a value             | `() -> str`                 | `Callable[[], str]`                          |
|  [03]   | Consume a value for effects | `int -> ()`                 | `Callable[[int], None]`                      |
|  [04]   | Combine inputs              | `(int, int) -> int`         | `Callable[[int, int], int]`                  |
|  [05]   | Accept a function           | `(Path, (bytes -> T)) -> T` | `Callable[[Path, Callable[[bytes], T]], T]`  |
|  [06]   | Keep a wrapped signature    | `(P -> T) -> (P -> T)`      | `Callable[[Callable[P, T]], Callable[P, T]]` |

- Type parameters sit on their declaration (`def first[T]`, `class Box[T]`, `type Pair[T] = tuple[T, T]`)
- `**P` carries a wrapped function's parameters, `*Ts` ties an argument tuple to them (`Callable[[*Ts], R]`), `*args: object` erases both
- `Protocol` types a callable `Callable` cannot spell (keyword parameters, `__name__`, overloads) beside the consumer reading it
- Subjects take positional-only `/` parameters, options and flags take keyword-only `*` parameters

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, they raise no exception and return no `None` as an outcome their signature omits. Correct a dishonest contract by narrowing its input to a validated type or widening its output to `Option[T]` or `Result[T, E]`:
- `int -> Tier` is incomplete when some integers fail validation, `Count -> Tier` over a constrained `Count` covers every value
- `str -> int` hides undefined parses, `str -> Option[int]` describes every outcome (`Some(int(text)) if text.isdecimal() else Nothing`)
- `None` as a return type marks an effect with no returned information, and it makes no function pure
- `T | None` appears where an external API speaks it, `Option.of_optional` converts it at the read
- `TypeIs[T]` predicates narrow both branches at their call
- Boolean parameters that switch behavior become separate functions or a union case parameter

### [01.2]-[PURITY]

Functions are pure when their return value depends on their arguments alone and evaluation causes no side effect, so a pure call's result replaces the call. Side effects are mutating state visible outside a function, mutating an argument, raising, and I/O (clock, environment, filesystem, network, randomness, another process). Mutation of a local that never escapes is no side effect:
- Clock, randomness, and configuration enter as arguments (`def expired(now: datetime, issued: datetime, lifetime: timedelta) -> bool`)
- Module state is immutable (`frozendict`, `tuple`, `frozenset`, a frozen record)
- `nonlocal` assignment is a side effect a returned value replaces
- Random state enters as a `numpy.random.Generator`, child streams come from `SeedSequence.spawn`

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or both. A higher-order function owns stable control flow and its caller supplies the varying rule:
- `partial` over a top-level function fixes leading arguments (`tuple(map(partial(scaled, 0.5), values))`)
- Parameters order for partial application: dependencies and configuration, then policies, then the subject
- Callables pass as themselves (`map(str.strip, lines)`, `catch(path.read_bytes, exception=OSError)()`)
- Key functions are lambdas or methods, typeshed types `operator.attrgetter` and `itemgetter` results as `Any`
- `pipe(value, f, g)` composes unary functions, `Result` and `Option` chains stay fluent (`.map`, `.bind`)
- Steps repeated at separate sites get one owner generic over what the step returns (`def reset[T](folder: Path, made: Callable[[Path], T]) -> T`)

Select the form by the call site:
- Function tables keyed by type or enum member (`frozendict[type[Case], Callable[[Case], R]]`) when behaviors share a signature and vary as data
- `functools.singledispatch` when other modules register implementations for their own types
- `any` or `all` over a generator when a short-circuiting boolean suffices
- Ordered rule tuple with an explicit fallback for a first-match decision over values

### [01.4]-[DECORATORS]

Decorators and `__init_subclass__` read a declaration once at import and derive behavior from it, callers state what the declaration cannot. A decorator that is a generic record keeps its function's signature through `**P` and takes its key from the function name:

```python
class Body[**P, R](Protocol):
    __name__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> R: ...


class Command[**P, R](msgspec.Struct, frozen=True):
    body: Body[P, R]

    @property
    def key(self) -> str:
        return self.body.__name__.replace("_", "-")

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> R:
        return self.body(*args, **kwargs)
```

- `@Command` over `def build_all(target: str, *, clean: bool = False) -> int` keeps that signature and derives key `build-all`
- Decorator arguments state what a function, its docstring, and its signature cannot supply
- Declaration defects raise `TypeError` at decoration or class build (`__post_init__`, `__init_subclass__`), before first use
- Parameterized decorators return `partial` over a top-level function, applied on the `def`
- `__init_subclass__` sits on a plain base, an override on a `msgspec.Struct` base conflicts with its class keywords

Subclasses state their pattern and docstring, and the base derives each code, rejects an undocumented rule, and owns the check:

```python
class Rule:
    code: ClassVar[str]
    pattern: ClassVar[re.Pattern[str]]

    @override
    def __init_subclass__(cls) -> None:
        super().__init_subclass__()
        if cls.__doc__ is None:
            raise TypeError(cls.__qualname__)
        cls.code = "-".join(word.lower() for word in re.findall(r"[A-Z][a-z\d]*", cls.__name__))

    def hit(self, line: str) -> bool:
        return self.pattern.search(line) is not None


class TrailingSpace(Rule):
    """Line ends in whitespace."""

    pattern = re.compile(r"\s$")
```

### [01.5]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                             | [CORRECT_FORM]                                    |
| :-----: | :------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Module-level `TypeVar`                                   | Type parameters on the declaration                |
|  [02]   | `typing.cast`                                            | Type at source or narrow with `TypeIs`            |
|  [03]   | Closure returning a closure                              | `partial` over a top-level function               |
|  [04]   | Free function over one record                            | Method or property of the record                  |
|  [05]   | Metaclass                                                | `__init_subclass__`, `__set_name__`, or decorator |
|  [06]   | `None` default the body replaces                         | Default in the signature                          |
|  [07]   | Name string per subclass or decorator argument naming it | `__name__`, `__qualname__`, or docstring          |
|  [08]   | `getattr(item, "name")` or `hasattr` on a literal name   | Attribute access or a table keyed by the member   |
|  [09]   | `.map` rebuilding a value a second site computes         | One owner generic over its returned value         |

## [02]-[EXPRESSIONS]

Expressions evaluate to values, and statements perform an action or control execution. Build a returned value in one expression: a comprehension, a conditional expression, or a record constructor:
- Comprehension unpacking flattens iterables (`frozenset(*order.skus for order in orders)`) and merges mappings (`{**m for m in ms}`)
- Functions return a `tuple`, `frozenset`, or `frozendict` from every arm, a generator stays where laziness bounds memory or the sequence
- Walrus binds a value a comprehension filter or guard reads (`if (hit := pattern.search(line))`)
- `None not in (row := fetched()).values()` checks a whole mapping at once
- Reductions are `sum(terms, start=zero)`, `any`, `all`, `max(key=)`, `math.fsum`, and `functools.reduce(f, items, seed)`

### [02.1]-[PATTERN_MATCHING]

Pattern matching selects a result from a value's type, shape, or fields, and keeps recognition, extraction, guards, and results together. One arm recognizes a shape with class patterns, `|` alternatives binding the same names, `as` captures, and sequence patterns, and arms with one body merge (`Opened`, `Resumed`, and `Closed` are tagged cases with a `key` field):

```python
def applied(event: Opened | Resumed | Closed, live: frozenset[str]) -> frozenset[str]:
    match event:
        case Opened(key=key) | Resumed(key=key):
            return live | {key}
        case Closed(key=key):
            return live - {key}
```

- Class patterns bind by keyword, a `kw_only` record or a pydantic model has empty `__match_args__`
- Sequence patterns (`[first, *rest]`), mapping patterns (`{"key": value, **rest}`), and dotted value patterns (`Color.RED`) read structure
- Guards test what no pattern expresses, a walrus in a guard binds a derived value its arm reads
- Matches over a closed union end at its last case, mypy and ty prove coverage and report a missing case
- Unions with behavior per case dispatch through each case's method, `match` serves transitions and boundary translation

### [02.2]-[ITERATION]

Iterators produce values lazily and once, and a consumer that stops early computes only what it read. Select the member by the operation:

| [INDEX] | [OPERATION]      | [MEMBER]                                       |
| :-----: | :--------------- | :--------------------------------------------- |
|  [01]   | Neighbor pairs   | `itertools.pairwise`                           |
|  [02]   | Fixed chunks     | `itertools.batched(items, size, strict=True)`  |
|  [03]   | Running fold     | `itertools.accumulate(items, f, initial=seed)` |
|  [04]   | Single fold      | `functools.reduce(f, items, seed)`             |
|  [05]   | Consecutive runs | `itertools.groupby` over sorted input          |
|  [06]   | Lazy slice       | `islice`, or `takewhile` over `count`          |

- Generator functions separate producing values from transforming them, each stage takes and returns an iterable
- Iterators are single-use, a value read twice is a `tuple`

### [02.3]-[RECURSION]

Recursive functions need a base case that returns a final value, a recursive case that moves closer to it, and a returned value on every path. Python performs no tail-call optimization and bounds recursion depth, so depth and successor sharing select the form:

| [INDEX] | [APPROACH]                   | [USE_WHEN]                        | [COST]                         |
| :-----: | :--------------------------- | :-------------------------------- | :----------------------------- |
|  [01]   | Direct recursion             | Maximum depth small and bounded   | Each call grows the stack      |
|  [02]   | `expression.tailrec`         | Linear recursion, unbounded depth | Each step is a trampoline call |
|  [03]   | `reduce` or `accumulate`     | Fold over a sequence              | `reduce` keeps no intermediate |
|  [04]   | `graphlib.TopologicalSorter` | Shared or cyclic successors       | Graph built before the walk    |

`@tailrec` bodies return a value or their next arguments as `TailCall` (`-> int | TailCall[int, int]`), and its trampoline runs in constant stack.

### [02.4]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                        | [CORRECT_FORM]                                  |
| :-----: | :-------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `itertools.chain` or a nested loop to flatten       | Comprehension unpacking                         |
|  [02]   | Loop appending to a list it returns                 | Comprehension or `tuple` from every arm         |
|  [03]   | `for item in items: yield item`                     | `yield from items`                              |
|  [04]   | `isinstance` chain                                  | `match` with class patterns                     |
|  [05]   | `case _: pass`, trailing `assert_never`, or default | Last case of the union, checkers prove coverage |
|  [06]   | Dispatch on enum value order or `.value` arithmetic | Dispatch on members                             |
|  [07]   | Nested conditional repeating a fallback             | Guard with a walrus                             |
|  [08]   | `reduce(operator.add, terms)`                       | `sum(terms, start=zero)`                        |
|  [09]   | `sys.setrecursionlimit` to fit a deep recursion     | `tailrec`, a fold, or a graph traversal         |

## [03]-[RECORDS]

Records are frozen `msgspec.Struct` classes: slotted, hashable, matchable, and cheap to build. A change derives a new record through `copy.replace`, and behavior on one record is its method or property:

```python
type Count = Annotated[int, msgspec.Meta(ge=1)]


class Line(msgspec.Struct, frozen=True, kw_only=True):
    sku: str
    count: Count
    price: float

    @property
    def total(self) -> float:
        return self.count * self.price

    def restocked(self, extra: Count) -> Line:
        return copy.replace(self, count=self.count + extra)
```

- Construction trusts its caller, decode and `msgspec.convert` validate types and `Meta` constraints, so outside values enter through a codec
- `type <Name> = Annotated[T, msgspec.Meta(...)]` declares a constraint once beside its concept
- `__post_init__` raising `ValueError` holds a cross-field invariant, decode reports it as a `ValidationError` with its path
- Fields hold `tuple[T, ...]`, `frozenset[T]`, `frozendict[K, V]`, or `Mapping[K, V]`
- Per-channel values take the shape their consumer takes whole (`tuple[float, float, float]`, an `ndarray` of shape `(3,)`)
- `kw_only=True` allows required fields after defaulted ones and names every argument at construction
- Code reads fields by attribute, `__struct_fields__` and `msgspec.inspect` serve a codec alone
- Tables built once are `frozendict` keyed by the object that answers a lookup (a type, an enum member, a tuple of both)

### [03.1]-[UNIONS]

Model each valid state a consumer inspects as a distinct case, and states that differ only by a value as one record of that value. Counting values a type admits (a product multiplies its fields' counts, `Option[T]` adds one) exposes unneeded states. A tagged base carries the union's policy, `tag=True` derives each tag from its class name, and each case owns its behavior:

```python
class Shape(msgspec.Struct, frozen=True, kw_only=True, tag=True): ...


class Circle(Shape, frozen=True):
    radius: float

    @property
    def area(self) -> float:
        return math.pi * self.radius**2


class Rect(Shape, frozen=True):
    width: float
    height: float

    @property
    def area(self) -> float:
        return self.width * self.height
```

- Cases restate `frozen=True` on their class line, mypy and ty reject a frozen base under a case that omits it
- Decoders over the union (`msgspec.json.decode(raw, type=Circle | Rect)`) validate and dispatch in one pass
- Unions with one common case make it the base with its own tag, each other case overrides what differs and calls `super()` where it extends
- `tag_field` names a wire key an external format fixes, `tag="<value>"` matches an external value
- `Literal` kind fields with fields meaningful under one value become one case per value
- Enum members name a closed set of values with no data beyond their value, one enum per concept

### [03.2]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                | [CORRECT_FORM]                                        |
| :-----: | :---------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `@dataclass`, `NamedTuple`, `namedtuple`                    | Frozen `msgspec.Struct`                               |
|  [02]   | Mutable `Struct`, or a `list` field in a frozen one         | `frozen=True` with `tuple`, `frozenset`, `frozendict` |
|  [03]   | `status: Literal["ok", "error"]` with fields for one status | One tagged case per status                            |
|  [04]   | Flag field read by `if` to select behavior                  | Union case with its own method                        |
|  [05]   | Case extending a sibling case                               | Sibling cases with their own data                     |
|  [06]   | JSON text held in a record field                            | Typed field, one codec per store                      |
|  [07]   | `x`, `y`, `z` fields per channel                            | `tuple[float, float, float]`                          |
|  [08]   | `MappingProxyType` over a literal, a dict of dicts          | `frozendict` with the flattest key                    |
|  [09]   | Empty string or sentinel object marking absence             | `Option[T]` field                                     |

## [04]-[RESULTS]

Every fallible function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]                  | [CONCERN]                                            |
| :-----: | :---------------------- | :--------------------------------------------------- |
|  [01]   | `Option[T]`             | Absence without a reason                             |
|  [02]   | `Result[T, Faults]`     | Expected failure with reasons, first failure stops   |
|  [03]   | `catch(f, exception=E)` | Raising call converted to `Result[T, E]` at its call |

Input boundary selects the result type, functions preserve it, and conversion happens at one named boundary through the member named for its target (`to_result`, `to_option`, `Option.of_optional`, `to_optional`). `match` on a tag, `default_value`, and `default_with` are host operations. `Ok` and `Error` build a `Result` with an `Any` side, and the enclosing function's declared return type pins it. An empty `tuple` is a result and not absence, `Option[tuple[T, ...]]` exists only where a consumer responds differently to no collection and an empty one. Producers that map an operational failure to `Nothing` hide it from their consumer.

### [04.1]-[ERRORS]

Expected failures are data in return types, and exceptions are reserved for defects: a violated precondition, a declaration defect at import, and a raising third-party call a boundary converts. Each raising package declares one fault record and one reason enum, each reason's value holds its message text, and a caught exception stays as its cause:

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
    def caught(cls, reason: Reason, subject: object, error: Exception) -> Faults:
        match error:
            case ExceptionGroup(exceptions=leaves):
                return tuple(*cls.caught(reason, subject, leaf) for leaf in leaves)
            case _:
                return (cls(reason=reason, subject=repr(subject), cause=Some(error)),)
```

`catch(f, exception=E)(arguments)` covers one raising call, names the exception its contract raises, and passes the callable itself. `map_error` over `partial(Fault.caught, <reason>, <subject>)` turns the caught error into faults at that call:
- `catch(path.read_bytes, exception=OSError)().map_error(partial(Fault.caught, Reason.UNREADABLE, path))` reads a file
- `catch` over an `async def` or a generator function returns `Ok(<coroutine>)` and catches nothing raised later
- `except*` receives every exception as an `ExceptionGroup` and holds no `return`, its handler assigns faults and the function returns after
- Consumers classify a fault by its `reason` member
- Suppressed or warned conditions become an `Error` or a `Result` holding the degraded value

### [04.2]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether steps depend on each other:

| [INDEX] | [SIGNATURE]                 | [OPERATOR]                | [BEHAVIOR]                                           |
| :-----: | :-------------------------- | :------------------------ | :--------------------------------------------------- |
|  [01]   | `T -> R`                    | `.map`                    | Transforms a present value, keeps the result type    |
|  [02]   | `T -> bool`                 | `.filter_with`, `.filter` | Keeps a passing value, else fails with a fault       |
|  [03]   | `T -> Result[R, E]`         | `.bind`                   | Continues with a dependent step, first failure stops |
|  [04]   | `E -> F`                    | `.map_error`              | Translates a failure into package faults             |
|  [05]   | `E -> Result[T, F]`         | `.or_else_with`           | Runs a fallback after a failure                      |
|  [06]   | `Option[T] -> Result[T, E]` | `.to_result(faults)`      | Promotes absence where its reason is known           |
|  [07]   | Independent checks          | Accumulating fold         | Runs every check, keeps every fault                  |

- `filter_with` keeps or fails a `Result`, `filter` keeps or empties an `Option`
- Steps a chain binds are named functions declared `-> Result[T, E]`, a lambda returning `Ok(...) if c else Error(...)` widens to `Any`
- Values stay inside a result through a pipeline, an unwrap mid-pipeline followed by a rewrap duplicates its failure path
- `traverse`, `sequence`, `map2`, and `pipeline` stop at the first error
- Effect builders turn a `RuntimeError` raised in their body into `Ok(None)`, chained `.bind` and `.map` replace them
- Truthiness carries no meaning on `Result` and `Option` (`bool(Nothing)` is `True`), `is_ok()` and `is_none()` read the case

Workflows parse, validate, and look up in order, and adding a step adds one call:

```python
def service(table: Mapping[int, str], text: str) -> Result[str, Faults]:
    return (
        catch(int, exception=ValueError)(text)
        .map_error(partial(Fault.caught, Reason.INVALID, text))
        .filter_with(lambda port: 0 < port < 65536, lambda port: (Fault(reason=Reason.INVALID, subject=repr(port)),))
        .bind(lambda port: Option.of_optional(table.get(port)).to_result((Fault(reason=Reason.ABSENT, subject=repr(port)),)))
    )
```

Independent checks over one value run in full, `swap().default_value(())` yields each check's faults, and comprehension unpacking joins them:

```python
def checked[T](value: T, *checks: Callable[[T], Result[T, Faults]]) -> Result[T, Faults]:
    faults = tuple(*check(value).swap().default_value(()) for check in checks)
    return Error(faults) if faults else Ok(value)
```

### [04.3]-[MATCHING]

Hosts read a result once through `match` on its `tag`, each narrowed literal proves its case and the payload reads as `ok` or `error`. `Ok`, `Error`, and `Some` are functions, so `case Ok(value)` raises `TypeError`, and `case Result(tag="ok", ok=value)` binds without proving coverage:

```python
def exited(outcome: Result[int, Faults]) -> int:
    match outcome.tag:
        case "ok":
            return outcome.ok
        case "error":
            sys.stderr.writelines(f"{fault.message}\n" for fault in outcome.error)
            return 1
```

- Hosts translate a result into their vocabulary in that match: exit code, HTTP status, UI state, or a host exception
- Hosts that read a command's return value read an unmapped `Error` as success

### [04.4]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                         | [CORRECT_FORM]                                    |
| :-----: | :--------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `try` answering a value from every handler           | `catch` over the raising call                     |
|  [02]   | `except Exception` around a block                    | `catch` naming the call's exception               |
|  [03]   | Exception class per layer, or a union of fault types | One fault record and reason enum per package      |
|  [04]   | Message-only fault, or a string reason               | Reason member with its message as value           |
|  [05]   | `case Ok(value)`                                     | `match result.tag`                                |
|  [06]   | `default_value` with an arbitrary value mid-pipeline | `to_result` with faults, defaults at the host     |
|  [07]   | `bind` rebuilding `Ok` or `Error` around one value   | `filter_with`, `to_result`, or `map`              |
|  [08]   | Effect builder                                       | Chained `.bind` and `.map`                        |
|  [09]   | `contextlib.suppress`, `warnings.warn`               | `Error`, or a `Result` holding the degraded value |

## [05]-[BOUNDARIES]

Boundaries own every conversion between external values and domain values, validate input once into records, and translate results into a host's representation.

### [05.1]-[CODECS]

Decode external bytes into a wire record in one typed call, then convert it into its domain record. msgspec decodes no `Option` field, so wire records hold `T | None` or `UNSET` and domain records hold `Option[T]` (`Quota` holds `name` and `limit: Option[int]`):

```python
class Wire(msgspec.Struct, frozen=True, kw_only=True):
    name: str
    limit: int | None = None


def quota(raw: bytes) -> Result[Quota, Faults]:
    return (
        catch(msgspec.json.decode, exception=msgspec.DecodeError)(raw, type=Wire)
        .map(lambda wire: Quota(name=wire.name, limit=Option.of_optional(wire.limit)))
        .map_error(partial(Fault.caught, Reason.INVALID, raw))
    )
```

- `msgspec.json.decode(raw, type=T)` or one module-level `Decoder(T)` reads a format
- `msgspec.convert(value, T)` validates parsed builtins, `from_attributes=True` reads attribute objects, `strict=False` serves string sources
- `DecodeError` covers malformed input and its subclass `ValidationError` a schema mismatch
- Input that needs every error at once, coercion, custom validators, or a published schema takes a pydantic `TypeAdapter` built at module scope
- Files a program owns are read by key path at their use site, schemas validate input a program does not own
- Text with a grammar parses through its grammar's parser (`tomllib`, `ast`, a markdown parser)

### [05.2]-[RESOURCES]

Inject the narrowest dependency that represents what consumers need: a value for a stable snapshot, a function for an operation that runs on demand, and a `Protocol` for a seam each platform implements in full. Pair acquisition and release in one scope, and release runs on the error path:
- Composition roots read configuration and environment once, build dependencies, and bind them with `partial` into handler functions
- Process-lifetime state is a resource the entry scope yields to its dependents
- Libraries return their result type with their own faults, applications compose retry, fallback order, and caching around them
- `with` enters the owning context manager, one parenthesized `with` holds every manager a block needs
- `contextlib.ExitStack` holds a count known at runtime, `pop_all()` transfers ownership once every acquisition succeeds
- `@contextmanager` generators wrap their `yield` in `try` with `finally`
- Commits are a step after the work, outside its release

### [05.3]-[CONCURRENCY]

Concurrent regions are anyio task groups, and tasks return results so an expected failure cancels no sibling and exception groups carry defects. An awaited raising call sits in `try` with `except*`, its handler assigns faults, and the function returns after:

```python
async def loaded(path: anyio.Path) -> Result[bytes, Faults]:
    try:
        content = await path.read_bytes()
    except* OSError as group:
        failed = Fault.caught(Reason.UNREADABLE, path, group)
    else:
        return Ok(content)
    return Error(failed)
```

- `anyio.gather` returns a typed tuple and `anyio.amap` an ordered list, a task handle supplies one task's value
- Blocking calls run through `anyio.to_thread.run_sync`, CPU-bound pure functions through `to_process.run_sync` or `to_interpreter.run_sync`
- Cancellation is caught as `anyio.get_cancelled_exc_class()` alone and re-raised, cleanup awaits run under `CancelScope(shield=True)`

### [05.4]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                        | [CORRECT_FORM]                                        |
| :-----: | :-------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `json.loads` then reads by key                      | `msgspec.json.decode(raw, type=T)`                    |
|  [02]   | `Decoder(T)` built inside the function that decodes | Module-level codec or `decode(raw, type=T)`           |
|  [03]   | Decode target holding an `Option` field             | Wire field defaulting to `None`, `Option.of_optional` |
|  [04]   | `os.environ` read in domain code                    | Settings record decoded once in the entry point       |
|  [05]   | Regex standing in for a grammar                     | Grammar's own parser                                  |
|  [06]   | `asyncio`, `concurrent.futures`, `time.sleep`       | Task groups, workers, and `anyio.sleep`               |
|  [07]   | `catch` around an awaited call                      | `try` with `except*` around the awaited call          |
|  [08]   | Shared list or stream collecting task results       | `gather`, `amap`, or a task handle                    |

## [06]-[NUMERICS]

Numeric facts come from the package that states them, and array code runs whole-array operations:
- Roots, quadrature, interpolation, fits, and factorizations are scipy and numpy members
- Elementwise and reduction passes are one vectorized call, a Python loop over elements stays where each step reads its neighbors' results
- One specialized operation replaces composed general ones (`np.dot(v, v)` over `np.sum(v * v)`)
- Randomness enters as a generator argument and draws its whole shape in one call (`points + rng.normal(0.0, scale, points.shape)`)
- Library code over a mix of array types reads its namespace through `array_api_compat.array_namespace` and calls its members alone
- `np.errstate(divide="raise", invalid="raise")` scopes floating-point failures a `catch` over `FloatingPointError` converts
- Integer arrays wrap on overflow, and a Python scalar in arithmetic takes its array's dtype

## [07]-[MODULES]

Each module holds one owner, and imports point down its layers:
- Imports a module body does not evaluate at load are `lazy`, names a class body, base, decorator, or record annotation reads stay eager
- `__all__` closes a module others import and lists what they compose, names one module reads alone take a `_` prefix
- Unused public capability stays exported
- Files existing to break an import cycle merge, and the cycle goes at its root
- Annotations resolve forward references unquoted, `from __future__ import annotations` stores them as strings
- Entry modules keep top-level code import-safe, process and interpreter workers re-import them

## [08]-[TESTS]

Tests compare whole values against expected values and use their framework's API for every need:
- `assert parsed(str(value)) == Some(value)` under `@given(st.integers(min_value=0))` states a round trip for every generated input
- `pytest.param(..., id=...)` derives ids from each case's subject, `pytest.raises(E, match=...)` names a defect
- Doubles come from `unittest.mock` (`Mock`, `attach_mock`) and `monkeypatch`
- Hypothesis strategies hold their constraints, profiles hold run tuning, and `@given` tests return `None`
- Tests check behavior, a check of the code base's own structure is a defect
