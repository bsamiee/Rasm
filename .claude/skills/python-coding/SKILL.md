---
name: python-coding
description: "Use when writing or reviewing Python, covering functional design with result and record types"
---

# [PYTHON_CODING]

Covers Python design decisions from signature shape to host boundary. Expression supplies results and options, msgspec supplies records and codecs, and every example uses their types and the newest Python forms as vocabulary.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Worked flows for decorators, registries, partial application, and pipelines over functions as values
- [02]-[RECORDS](references/records.md): Worked flows for records from decode to transition, with tagged unions, transitions, and array fields
- [03]-[RESULTS](references/results.md): Worked flows for faults, accumulation, validators, workflows, host translation, and result equations
- [04]-[BOUNDARIES](references/boundaries.md): Worked flows for one entry from argv and environment through codecs and resources to an exit code
- [05]-[CONCURRENCY](references/concurrency.md): Worked flows for anyio tasks returning results, with fan-out, deadlines, cancellation, and workers
- [06]-[NUMERICS](references/numerics.md): Worked flows for whole-array passes, buffer ownership, floating-point scopes, random streams, and tables

## [01]-[FUNCTIONS]

Function signatures are contracts, parameter types describe every value a function accepts and its return type every outcome a caller must handle. `Callable[[int], str]` states `int -> str`, and `Callable[[Callable[P, T]], Callable[P, T]]` keeps a wrapped signature:
- Type parameters sit on their declaration with inferred variance (`def first[T]`, `class Box[T]`, `type Pair[T] = tuple[T, T]`)
- Bounds keep a caller's type (`def unique[T: Hashable](items: Iterable[T]) -> tuple[T, ...]`), constraints (`[S: (str, bytes)]`) replace `AnyStr`
- `**P` captures a wrapped function's parameters, `*Ts` ties an argument tuple to them (`Callable[[*Ts], R]`), `*args: object` erases both
- Parameters take the narrowest ABC a body reads (`Iterable`, `Collection`, `Sequence`), return types stay concrete
- `Protocol` types a callable `Callable` cannot spell (keyword parameters, `__name__`, overloads) beside the consumer reading it
- Subjects take positional-only `/` parameters, options and flags take keyword-only `*` parameters
- Types narrow through `match`, `isinstance`, or a `TypeIs` predicate, a value typed wrong is retyped at its source

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, they raise no exception and return no `None` as an outcome their signature omits. Correct a dishonest contract by narrowing its input to a validated type or widening its output to `Option[T]` or `Result[T, E]`:
- `int -> Tier` is incomplete when some integers fail validation, `Quantity -> Tier` over a record validating in `__post_init__` covers every value
- `Annotated` constraints narrow nothing for a checker, a parameter typed `Annotated[int, Meta(ge=1)]` accepts every `int`
- `str -> int` hides undefined parses, `str -> Option[int]` describes every outcome (`catch(int, exception=ValueError)(text).to_option()`)
- `min`, `max`, and `next` raise on empty input, `Option.of_optional(max(items, key=len, default=None))` makes the read total
- `T | None` appears where an external API speaks it, `Option.of_optional` converts it at the read, `None not in mapping.values()` tests every value at once
- Boolean parameters that switch behavior become separate functions or a union case parameter

### [01.2]-[PURITY]

Functions are pure when their return value depends on their arguments alone and evaluation causes no side effect, and a pure call's result replaces the call. Side effects are mutating state visible outside a function, mutating an argument, raising, and I/O (clock, environment, filesystem, network, randomness, another process). Mutation of a local that never escapes is no side effect:
- Clock, randomness, and configuration enter as arguments (`def expired(now: datetime, issued: datetime, lifetime: timedelta) -> bool`)
- Module state and parameter defaults are the immutable value a body reads (`()`, `frozenset()`, `frozendict()`, `Nothing`, a frozen record)
- State a step changes returns as its next value (`(State, Event) -> State`) in place of a `nonlocal` assignment
- `functools.cache` memoizes a pure top-level function, a cached method keeps every `self` it receives alive

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or both. A higher-order function owns stable control flow and its caller supplies the varying rule:
- `partial` over a top-level function fixes leading arguments (`tuple(map(partial(scaled, 0.5), values))`) in place of a returned closure
- Callables built per loop item bind it through `partial(f, item)`, a lambda reads the loop variable's last value
- Parameters order for partial application: dependencies and configuration, then policies, then the subject
- Callables pass as themselves (`map(str.strip, lines)`, `catch(path.read_bytes, exception=OSError)()`)
- Key functions are lambdas or methods, typeshed types `operator.attrgetter` and `itemgetter` results as `Any`
- `pipe(value, f, g)` runs unary functions, `compose(f, g)` names a reusable pipeline, `Result` and `Option` chains stay fluent
- Steps repeated at separate sites get one owner generic over what the step returns (`def reset[T](folder: Path, made: Callable[[Path], T]) -> T`)

## [02]-[RECORDS]

Records are frozen `msgspec.Struct` classes: slotted, hashable, matchable, and cheap to build. A change derives a new record through `msgspec.structs.replace`, and behavior on one record is its method or property:
- `__post_init__` raises `ValueError` for an invariant across fields, and runs at construction, decode, `convert`, and `structs.replace`
- `structs.replace` types as the record under mypy and ty, `copy.replace` fails mypy over `Self` or a union
- `Meta` states a single-field bound once and holds it at decode and `convert` alone, values from outside a decode enter through `convert`
- Decode and `convert` report a `__post_init__` `ValueError` as a `ValidationError` with its path
- `type <Name> = Annotated[T, msgspec.Meta(...)]` declares a constraint once beside its concept
- Fields hold frozen typed values (`tuple[T, ...]`, `frozenset[T]`, `frozendict[K, V]`, a frozen record), hashing a record hashes every field
- Absence is an `Option[T]` field, msgspec decodes no `Option`, and a decoded record holds `T | None` and a property returns its `Option`
- Per-channel values take the shape their consumer takes whole (`tuple[float, float, float]` for one value, one `(n, 3)` array for many)
- Records holding an array declare `eq=False` and compare by identity, an array field fails `hash` and `==`
- `kw_only=True` allows required fields after defaulted ones and names every argument at construction
- Code reads fields and members by attribute, `msgspec.structs.fields` and `msgspec.inspect` serve a codec alone
- `functools.cached_property` on a record needs `dict=True`, a slotted record raises `TypeError` at its first read

### [02.1]-[UNIONS]

Model each valid state a consumer inspects as a distinct case, and states that differ only by a value as one record of that value. Counting values a type admits (a product multiplies its fields' counts, `Option[T]` adds one) exposes unneeded states. A tagged base holds the union's policy, `tag=True` derives each tag from its class name, and each case owns its behavior:
- Cases restate `frozen=True` and `kw_only=True`, checkers reject a frozen base under an unfrozen case and `kw_only` covers its own class
- Cases extend the tagged base directly, a plain `Struct` that `ABC` beside it fails with a metaclass conflict
- `Literal` status fields with fields that hold meaning under one value become one case per value
- Union decoders validate and dispatch each case by its tag in one pass
- `tag_field` names a wire key an external format fixes, `tag="<value>"` matches an external value
- Enum members name a closed set of values with no data beyond their value, one enum per concept, `StrEnum` for wire values

### [02.2]-[DERIVATION]

Decorators, `__init_subclass__`, and `__set_name__` read a declaration once at import and derive behavior from it in place of a metaclass, callers state what the declaration cannot. A decorator that is a generic record holds its function as `__wrapped__`, which keeps the signature for checkers through `**P` and for `inspect.signature` at runtime:
- Wrappers returning a function apply `functools.wraps`, which copies `__name__`, `__doc__`, `__wrapped__`, and `__type_params__`
- Declaration defects raise `TypeError` at decoration or class build (`__post_init__`, `__init_subclass__`), before first use
- Parameterized decorators return `partial` over a top-level function, applied on the `def`
- `__init_subclass__` overrides on a `msgspec.Struct` base hide the class keywords its stub declares

## [03]-[EXPRESSIONS]

Expressions evaluate to values, and statements perform an action or control execution. Build a returned value in one expression: a comprehension, a conditional expression, or a record constructor:
- Comprehension unpacking flattens iterables (`frozenset(*order.skus for order in orders)`) and merges mappings (`{**m for m in ms}`)
- `Option` values flatten through `to_list()` (`tuple(*o.to_list() for o in options)`), unpacking `Nothing` raises `EffectError`
- Functions return a `tuple`, `frozenset`, or `frozendict` from every arm, a generator stays where laziness bounds memory or the sequence
- Walrus binds a value a comprehension filter or guard reads (`if (hit := pattern.search(line))`)
- Reductions take the operator's own member: `sum(terms, start=zero)` over numbers and arrays, `math.prod`, `math.fsum`, `"".join`, `any`, `all`
- `x or fallback` replaces every falsy value (`0`, `""`, `()`), absence is an `Option`

### [03.1]-[PATTERN_MATCHING]

Pattern matching selects a result from a value's type, shape, or fields, and keeps recognition, extraction, guards, and results together. One arm recognizes a shape with class patterns, `|` alternatives binding the same names, `as` captures, and sequence patterns, and arms with one body merge:
- Class patterns bind by keyword, a `kw_only` record or a pydantic model has empty `__match_args__`
- Sequence patterns (`[first, *rest]`) bind `rest` as a `list` and never match `str` or `bytes`, `case [row]:` reads exactly one row
- Mapping patterns (`{"key": value, **rest}`) ignore keys they omit, dotted value patterns (`Color.RED`) compare members
- Guards test what no pattern expresses, a walrus in a guard binds a derived value its arm reads
- Matches returning from each arm over a closed union prove coverage, mypy and ty report a missing case as an implicit `None` return
- Matches over a `(state, event)` pair end in `case _:`, mypy proves no coverage of a tuple subject and reports a missing return
- Matches over decoded or host input end in `case _:` returning an `Error`
- Unions with behavior per case dispatch through each case's method, `match` serves transitions and boundary translation

### [03.2]-[ITERATION]

Iterators produce values lazily and once, and a consumer that stops early computes only what it read. Select the member by the operation:

| [INDEX] | [OPERATION]      | [MEMBER]                                       |
| :-----: | :--------------- | :--------------------------------------------- |
|  [01]   | Parallel walk    | `zip(a, b, strict=True)`                       |
|  [02]   | Neighbor pairs   | `itertools.pairwise`                           |
|  [03]   | Fixed chunks     | `itertools.batched(items, size, strict=True)`  |
|  [04]   | Running fold     | `itertools.accumulate(items, f, initial=seed)` |
|  [05]   | Single fold      | `functools.reduce(f, items, seed)`             |
|  [06]   | Consecutive runs | `itertools.groupby` over input ordered by key  |
|  [07]   | Lazy slice       | `islice`, or `takewhile` over `count`          |
|  [08]   | Chunked reads    | `iter(partial(stream.read, size), b"")`        |

- Fold seeds are their operator's identity (`0`, `1`, `()`), which a fold returns for empty input
- Generator functions separate producing values from transforming them, each stage takes an iterable and delegates through `yield from`
- Iterators are single-use, a second pass over a spent one reads nothing, and a value read twice is a `tuple`

### [03.3]-[RECURSION]

Recursive functions need a base case that returns a final value, a recursive case that moves closer to it, and a returned value on every path. Python performs no tail-call optimization and bounds recursion depth, and depth and successor sharing select the form:

| [INDEX] | [APPROACH]                   | [USE_WHEN]                            | [COST]                         |
| :-----: | :--------------------------- | :------------------------------------ | :----------------------------- |
|  [01]   | Direct recursion             | Maximum depth small and bounded       | Each call grows the stack      |
|  [02]   | `reduce` or `accumulate`     | Linear recursion over a sequence      | `reduce` keeps no intermediate |
|  [03]   | `while` over local state     | Linear recursion over state alone     | Rebinding stays inside the def |
|  [04]   | `graphlib.TopologicalSorter` | Shared successors in an acyclic graph | Raises `CycleError` on a cycle |
|  [05]   | `deque` worklist with `seen` | Cyclic successors                     | Mutation stays inside the walk |

`expression.tailrec` types its call as `Never` under mypy and as the undecorated `T | TailCall` union under ty.

## [04]-[RESULTS]

Every fallible function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]       | [CONCERN]                                          |
| :-----: | :----------- | :------------------------------------------------- |
|  [01]   | `Option[T]`  | Absence without a reason                           |
|  [02]   | `Outcome[T]` | Expected failure with reasons, first failure stops |

Input boundary selects the result type, functions preserve it, and conversion happens at one named boundary through the member named for its target (`to_result`, `to_option`, `Option.of_optional`, `to_optional`):
- `match` on a tag is a host operation, `default_value` and `default_with` supply a default the domain defines for absence or failure
- `Ok` and `Error` build a `Result` with an `Any` side, the enclosing function's declared return type accepts it
- Empty `tuple` is a result and not absence, `Option[tuple[T, ...]]` exists only where a consumer treats no collection and an empty one apart
- Producers that map an operational failure to `Nothing` hide it from their consumer

### [04.1]-[ERRORS]

Expected failures are data in return types, and exceptions are reserved for defects: a violated precondition, a declaration defect at import, and a raising third-party call a boundary converts. Raisers import one `Fault` record, `Faults = tuple[Fault, *tuple[Fault, ...]]`, and `Outcome[T] = Result[T, Faults]` from one root package:
- Each raising package declares one reason enum, each value a message template over `subject`, `member`, and `cause`
- `subject` names the refused value, `member` the member or bound it misses, and a caught exception stays as `cause`
- Non-empty fault tuples come from a `match` on `(head, *rest)`

`catch(f, exception=E)(arguments)` from `expression.extra.result` covers one raising call, names the exception its contract raises, and passes the callable itself. `map_error` builds faults from the caught error at that call (`catch(path.read_bytes, exception=OSError)().map_error(lambda error: (Fault(reason=Reason.UNREADABLE, subject=str(path), cause=error),))`):
- `@catch(exception=E)` over a `def` converts its body's raise, a `Result` the body returns passes through unwrapped
- `catch` erases parameters to `...` and an overloaded return to mypy's first overload or ty's union, one typed `def` or codec method keeps both
- `catch` over an `async def` or a generator function returns `Ok(<coroutine>)` and catches nothing raised later
- Lambdas passed to `map_error` type their tuple as `tuple[Fault]`, and a later `bind` to a step returning `Outcome` fails both checkers
- Fault builders a chain binds past are named functions declared `-> Faults`, bound through `partial` over their subject
- Consumers classify a fault by its `reason` member
- Suppressed or warned conditions become an `Error` or a `Result` holding the degraded value
- Preconditions raise explicitly, `python -O` strips every `assert`

### [04.2]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether steps depend on each other:

| [INDEX] | [SIGNATURE]                 | [OPERATOR]           | [BEHAVIOR]                                           |
| :-----: | :-------------------------- | :------------------- | :--------------------------------------------------- |
|  [01]   | `T -> R`                    | `.map`               | Transforms a present value, keeps the result type    |
|  [02]   | `T -> bool` on `Result`     | `.filter_with`       | Keeps a passing value, else fails with built faults  |
|  [03]   | `T -> bool` on `Option`     | `.filter`            | Keeps a passing value, else empties it               |
|  [04]   | `T -> Result[R, E]`         | `.bind`              | Continues with a dependent step, first failure stops |
|  [05]   | `E -> F`                    | `.map_error`         | Translates a failure into package faults             |
|  [06]   | `E -> Result[T, E]`         | `.or_else_with`      | Runs a fallback after a failure                      |
|  [07]   | `Option[T] -> Result[T, E]` | `.to_result(faults)` | Promotes absence where its reason is known           |
|  [08]   | `Iterable[Outcome[T]]`      | `accumulated`        | Keeps every value, or every fault in input order     |

- Steps a chain binds are named functions declared `-> Result[T, E]`, a lambda returning `Ok(...) if c else Error(...)` widens to `Any`
- Steps one member states (`filter_with`, `to_result`, `map`) replace a `bind` rebuilding `Ok` or `Error` around one value
- Values stay inside a result through a pipeline, an unwrap mid-pipeline followed by a rewrap duplicates its failure path
- `traverse`, `sequence`, `map2`, and `pipeline` keep one error, `traverse` calls its step on every element back to front and returns the first error
- Dependent steps over items fold in order through `reduce` over `.bind`, and no step runs after the first error
- Effect builders swallow a `RuntimeError` a step raises (`Ok(None)` from a builder, an `AttributeError` from `traverse`), chains replace them
- Truthiness means nothing on `Result` and `Option` (`bool(Nothing)` is `True`), `is_ok()` and `is_none()` read the case

### [04.3]-[MATCHING]

Hosts read a result once through `match` on its `tag`, each narrowed literal proves its case and the value reads as `ok` or `error`. `Ok`, `Error`, and `Some` are functions, `case Ok(value)` raises `TypeError`, and `case Result(tag="ok", ok=value)` binds without proving coverage:
- Hosts that read a command's return value read an unmapped `Error` as success

## [05]-[BOUNDARIES]

Boundaries own every conversion between external values and domain values, validate input once into records, and translate results into a host's representation.

### [05.1]-[CODECS]

Decode external bytes into a record in one typed call through a module-level codec. A wire record apart from its domain record exists only where the wire shape differs (names, units, nesting):
- `Decoder(T)` and `Encoder()` sit at module scope and serve every call of their format
- `Decoder` keeps the type of a class or `tuple[...]` argument, a `type` alias argument types the codec as `Decoder[Any]`
- `Decoder.decode_lines` and `Encoder.encode_lines` read and write JSON Lines, one bad line fails the batch, per-line faults decode each line
- `msgspec.convert(value, T)` validates parsed builtins, `from_attributes=True` reads attribute objects, `strict=False` serves string sources
- `DecodeError` covers malformed input and its subclass `ValidationError` a schema mismatch
- Input that needs every error at once, coercion, custom validators, or a published schema takes a pydantic `TypeAdapter` built at module scope
- Use `python-coding-pydantic` for pydantic models, adapters, validators, and settings
- Persisted state encodes through a msgspec JSON or MessagePack codec, `pickle` and `shelve` run code from the store on load
- Bytes decode to text once at entry with a named `errors=` policy, text encodes once at exit
- Files a program owns are read by key path at their use site, schemas validate input a program does not own
- Text with a grammar parses through its grammar's parser (`tomllib`, `ast`, `csv`, a markdown parser)

### [05.2]-[RESOURCES]

Inject the narrowest dependency that represents what consumers need: a value for a stable snapshot, a function for an operation that runs on demand, and a `Protocol` for a seam each platform implements in full. Pair acquisition and release in one scope, and release runs on the error path:
- Composition roots read configuration and environment once, build dependencies, and bind them with `partial` into handler functions
- Process-lifetime state is a resource the entry scope yields to its dependents
- Libraries return their result type with their own faults, applications compose retry, fallback order, and caching around them
- `with` enters the owning context manager, one parenthesized `with` holds every manager a block needs
- `contextlib.ExitStack` holds a count known at runtime, `pop_all()` transfers ownership once every acquisition succeeds
- `@contextmanager` and `@asynccontextmanager` return `Generator[T]` or `AsyncGenerator[T]` and wrap their `yield` in `try` with `finally`
- Commits are a step after the work, outside its release

### [05.3]-[CONCURRENCY]

Concurrent regions are anyio task groups, tasks return results that cancel no sibling on an expected failure, and exception groups hold defects. No `catch` or `bind` spans an `await`, and one `async def` holds the awaited raising call in `try` and returns its `Error` from the handler:
- `except*` serves the `ExceptionGroup` a task group raises
- `anyio.gather` returns a typed tuple, `anyio.amap(step, items)` an ordered list, and a task handle one task's value
- Blocking and GIL-releasing native calls run through `to_thread.run_sync`, CPU-bound pure Python through `to_process` or `to_interpreter`
- Blocking raising calls run their `catch` inside the worker (`await to_thread.run_sync(catch(path.read_bytes, exception=OSError))`)
- `*Ts` runners (`anyio.run`, `start_soon`, `run_sync`) take the target and its arguments, and both checkers reject `partial(anyio.run, <target>)`
- Synchronous code offloads a blocking call to a `concurrent.futures.ThreadPoolExecutor` in `with`, and its `map` keeps input order
- `CapacityLimiter(n)` bounds a fan-out over a limited resource
- Deadlines are the caller's scope, an `async def` takes no `timeout` parameter
- `fail_after` raises `TimeoutError` where expiry is a failure, `move_on_after` sets `cancelled_caught` where expiry is a branch
- Cancellation is caught as `anyio.get_cancelled_exc_class()` alone and re-raised, cleanup awaits run under `CancelScope(shield=True)`

## [06]-[NUMERICS]

Numeric facts come from the package that states them, and array code runs whole-array operations:
- Roots, quadrature, interpolation, fits, and factorizations are scipy and numpy members
- One specialized operation replaces composed general ones (`np.dot(v, v)` over `np.sum(v * v)`)
- Hot numbers sit in one array of a fixed numeric dtype, a list of Python numbers or an `object` array boxes each value
- Module-level arrays call `setflags(write=False)` once built
- `==` on arrays is elementwise, `np.array_equal` and `np.allclose` compare whole arrays
- Arrays build once (`np.fromiter(items, dtype, count=n)`, one `np.concatenate`), each `np.append` copies the whole array
- Integer arrays wrap on overflow, a Python scalar in arithmetic takes its array's dtype
- Money is `Decimal`, a binary `float` rounds decimal fractions

## [07]-[MODULES]

Each module holds one owner, and imports point down its layers:
- Imports a module body does not evaluate at load are `lazy`, names a class body, base, decorator, or record annotation reads stay eager
- `__all__` closes a module others import and lists what they compose, names one module reads alone take a `_` prefix
- Unused public capability stays exported
- Files existing to break an import cycle merge, and the cycle goes at its root
- Annotations stay unquoted with no `from __future__ import annotations`, runtime readers call `annotationlib.get_annotations`
- Entry modules hold host translation alone with import-safe top-level code, process and interpreter workers re-import them
- Code an older interpreter imports keeps its forms: `MappingProxyType` for `frozendict`, eager imports, nested comprehensions
- Use `python-document` for docstrings
