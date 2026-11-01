# [FUNCTIONS]

Covers functions as values, from bound dependencies and operator families to dispatch, iteration, recursion, pipelines, and records of functions.

## [01]-[SPECIALIZATION]

`hashed` takes its reader as a function, its path as the subject, and its seed as a keyword policy with a default. `partial` binds the reader once and leaves the seed open to each caller, and binds the host hash's own keyword into the step `map` applies. `Outcome` is the package result and `content` the package byte reader:

```python
def hashed(read: Callable[[Path], Outcome[bytes]], path: Path, /, *, seed: int = 0) -> Outcome[int]:
    return read(path).map(partial(xxhash.xxh3_64_intdigest, seed=seed))


identity = partial(hashed, content)
```

`identity(path)` and `identity(path, seed=7)` hash one file under each seed, and an unreadable path returns the reader's fault:
- Both checkers check the arguments a `partial` binds, and ty keeps every remaining parameter, keyword policies included, refusing a `Callable` slot they fail
- Partials type as `partial[R]` under mypy and fit any slot returning `R`
- Nested partials flatten (`partial(identity, seed=7).func is hashed`), and a call-site keyword overrides a bound one

## [02]-[OPERATOR_FAMILIES]

Members that differ by the function they apply are one method and one `partialmethod` per member. `Term` is an expression over named inputs, each operator binds the function it applies, and nothing evaluates until `at` reads an environment:

```python
class Term(msgspec.Struct, frozen=True):
    at: Callable[[Mapping[str, float]], float]

    def _joined(self, op: Callable[[float, float], float], other: Term | float, /) -> Term:
        right = other.at if isinstance(other, Term) else lambda _: other
        return Term(lambda env: op(self.at(env), right(env)))

    __add__ = __radd__ = partialmethod(_joined, operator.add)
    __mul__ = __rmul__ = partialmethod(_joined, operator.mul)
    __sub__ = partialmethod(_joined, operator.sub)
    __rsub__ = partialmethod(_joined, lambda a, b: b - a)
    __truediv__ = partialmethod(_joined, operator.truediv)
```

`x = Term(lambda env: env["x"])` reads one input, and `(1.0 - 0.5 * x * x).at({"x": 2.0})` returns `-1.0`:
- Both checkers type each operator's result as `Term` and check no operand, `_joined` decides every operand type
- Commutative operators bind the forward and reflected names to one `partialmethod`, a noncommutative reflected operator swaps its operands
- `partial` stored as a class attribute binds as a method, `partialmethod` states that binding and `staticmethod(partial(...))` keeps a call unbound

## [03]-[DISPATCH]

Decisions over values read a member that states the decision or a table built once. `graded` maps each error to a grade through sorted breaks, and `max` reads the worst grade from the `IntEnum` order:

```python
class Grade(IntEnum):
    PASS = 0
    WARN = 1
    FAIL = 2


def graded(errors: Iterable[float], /, *, breaks: tuple[float, float] = (0.5, 0.9)) -> Grade:
    return max((Grade(bisect(breaks, error)) for error in errors), default=Grade.PASS)
```

`Grade` values count the breaks below each grade, `breaks` types one break per member past the first, and an empty input grades `PASS` through `default`. Select the form by the decision:

| [INDEX] | [DECISION]              | [FORM]                                                                        |
| :-----: | :---------------------- | :---------------------------------------------------------------------------- |
|  [01]   | Lookup table built once | `frozendict` keyed by the object answering it (enum member, leaf type, tuple) |
|  [02]   | Threshold               | `bisect` over sorted breaks into an `IntEnum` or a label tuple                |
|  [03]   | Worst or best of a set  | `max` or `min` over an `IntEnum` with `default=`                              |

- Tables hold facts no rule states, a value following a rule (a scale, an ordinal, a product of factors) computes from its parameters
- Tables keyed by a tuple of members hold every combination, `TABLE[kind, tier]` reads one, and a missing pair raises `KeyError` as a defect
- Tables keyed by a type resolve a subclass through `next(TABLE[base] for base in kind.__mro__ if base in TABLE)`

## [04]-[ITERATION]

Iterators produce values lazily and once, and a consumer that stops early computes only what it read. Select the member by the operation:

| [INDEX] | [OPERATION]      | [MEMBER]                                              |
| :-----: | :--------------- | :---------------------------------------------------- |
|  [01]   | Parallel walk    | `zip(a, b, strict=True)`                              |
|  [02]   | Transpose        | `zip(*rows, strict=True)`                             |
|  [03]   | Neighbor pairs   | `pairwise`, `pairwise((None, *items, None))` for ends |
|  [04]   | Fixed chunks     | `batched(items, size, strict=True)`                   |
|  [05]   | Cross product    | `product(kinds, tiers)`                               |
|  [06]   | Running fold     | `accumulate(items, f, initial=seed)`                  |
|  [07]   | Single fold      | `reduce(f, items, seed)`                              |
|  [08]   | Consecutive runs | `groupby` over input ordered by key                   |
|  [09]   | Top n            | `heapq.nlargest(n, items, key=f)`                     |
|  [10]   | Sorted merge     | `heapq.merge(*runs, key=f)` over sorted inputs        |
|  [11]   | Chunked reads    | `iter(partial(stream.read, size), b"")`               |

`filled` carries each reading across the ticks before the next one, each adjacent pair bounding one `range`:

```python
def filled[V](readings: Sequence[tuple[int, V]], /, *, step: int = 1) -> tuple[tuple[int, V], ...]:
    return (*((tick, value) for (start, value), (end, _) in pairwise(readings) for tick in range(start, end, step)), *readings[-1:])
```

`filled(((0, 1.0), (3, 2.0)))` returns `((0, 1.0), (1, 1.0), (2, 1.0), (3, 2.0))`, and `readings[-1:]` closes with the last reading, or with nothing for empty input:
- Fold seeds are their operator's identity (`0`, `1`, `()`), and a fold returns its seed for empty input
- Generator functions separate producing values from transforming them, each stage takes an iterable and delegates through `yield from`
- Values read twice are a `tuple`, and a spent iterator yields nothing on a second pass

## [05]-[RECURSION]

Python performs no tail-call optimization and bounds recursion depth, and depth and successor sharing select the form:

| [INDEX] | [APPROACH]                   | [USE_WHEN]                            | [COST]                         |
| :-----: | :--------------------------- | :------------------------------------ | :----------------------------- |
|  [01]   | Recursive `match` with `*`   | Tree depth small and bounded          | Each level grows the stack     |
|  [02]   | `reduce` or `accumulate`     | Linear recursion over a sequence      | `reduce` keeps no intermediate |
|  [03]   | `graphlib.TopologicalSorter` | Shared successors in an acyclic graph | `prepare` raises `CycleError`  |
|  [04]   | `deque` worklist with `seen` | Cyclic successors                     | Mutation stays inside the walk |

Recursive folds return a tuple per node and flatten children through unpacking (`tuple(*leaves(child) for child in node.children)`). `generations` yields each batch of nodes with every predecessor done, one batch per longest-path layer:

```python
def generations[K: Hashable](graph: Mapping[K, Iterable[K]], /) -> Iterator[tuple[K, ...]]:
    sorter = TopologicalSorter(graph)
    sorter.prepare()
    while sorter.is_active():
        ready = sorter.get_ready()
        sorter.done(*ready)
        yield ready
```

`graph` maps each node to its predecessors, `{"c": ("a", "b"), "b": ("a",), "d": ()}` yields `a` and `d`, then `b`, then `c`, and nodes of one batch can run in parallel. Sorters are local state their generator owns.

## [06]-[PIPELINES]

Pipelines name a composition of unary steps once and apply it where each value enters. `slug` normalizes text left to right, and `tier` pipes text through `slug` into a raising enum lookup that `catch` converts:

```python
slug: Callable[[str], str] = compose(str.strip, str.casefold, partial(re.compile(r"[\s_]+").sub, "-"))


def tier(text: str, /) -> Outcome[Tier]:
    return pipe(text, slug, catch(Tier, exception=ValueError)).map_error(partial(Fault.of, Reason.INVALID, repr(text)))
```

`Tier` is a `StrEnum` with a `"high-water"` member, and `tier(" High_Water ")` returns it:
- Unbound methods (`str.strip`, `str.casefold`) are unary steps over their subject
- Steps fixing a later argument bind a method of the fixed object, `partial(pattern.sub, "-")` leaves the text parameter open
- `catch(Tier, exception=ValueError)` keeps `Tier` as its value type
- `compose` takes at most nine steps and `pipe` sixteen, and a longer chain splits into named compositions with the same behavior, composition being associative

## [07]-[STRATEGIES]

Behavior that varies by format or backend is a generic record of functions with one instance per variant, its type parameter the value its functions pass each other. `Grammar` splits text into numbered units of its own type and holds rules over that type, and `REPORTS` holds each grammar as its bound `report` method:

```python
class Grammar[U](msgspec.Struct, frozen=True, kw_only=True):
    units: Callable[[str], Iterable[tuple[int, U]]]
    rules: frozendict[str, Callable[[U], bool]]

    def report(self, text: str, /) -> tuple[str, ...]:
        return tuple(f"{line}: {message}" for line, unit in self.units(text) for message, rule in self.rules.items() if rule(unit))


REPORTS: frozendict[str, Callable[[str], tuple[str, ...]]] = frozendict({
    ".md": Grammar[str](units=lambda text: enumerate(text.splitlines(), start=1), rules=frozendict({"trailing whitespace": lambda line: line != line.rstrip()})).report,
    ".py": Grammar[tokenize.TokenInfo](
        units=lambda text: ((token.start[0], token) for token in tokenize.generate_tokens(io.StringIO(text).readline)),
        rules=frozendict({"shouted comment": lambda token: token.type == tokenize.COMMENT and token.string.isupper()}),
    ).report,
})
```

`REPORTS[path.suffix](text)` reports one file, `REPORTS[".py"]("x = 1  # LOUD\n")` returns `("1: shouted comment",)`, and a new format adds one entry:
- Type arguments type each lambda parameter under both checkers, and mypy infers `U` from a lambda as `Any`
- `U` is invariant (returned by `units`, taken by each rule), no `Grammar[object]` holds both instances, and their bound methods share one type
