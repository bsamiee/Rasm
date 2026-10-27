# [FUNCTIONS]

Covers functions as values, from bound dependencies and operator families to dispatch, iteration, recursion, pipelines, and records of functions.

## [01]-[SPECIALIZATION]

Handlers take each dependency as the function they call, and composition binds it once through `partial`. `sized` reads bytes through `Reader` and checks a limit, production binds `content`, and a probe binds `fixed` over chosen bytes. `Fault`, `Outcome`, and `Reason` are the package faults, and `content` is the package byte reader:

```python
type Reader = Callable[[Path], Outcome[bytes]]


def fixed(data: bytes, _path: Path, /) -> Outcome[bytes]:
    return Ok(data)


def sized(read: Reader, limit: int, path: Path, /) -> Outcome[int]:
    return read(path).map(len).filter_with(lambda size: size <= limit, partial(Fault.of, Reason.OUTSIDE, str(path), member=f"at most {limit} bytes"))


handled: Callable[[Path], Outcome[int]] = partial(sized, content, 4096)
probe: Callable[[Path], Outcome[int]] = partial(sized, partial(fixed, b"abc"), 2)
```

`probe(Path("x"))` fails with `x accepts at most 2 bytes, not 3`:
- Both checkers check arguments a `partial` binds, and mypy accepts the `partial[R]` it infers in any `Callable` slot returning `R`
- `partial` values keep their remaining parameters under ty, and a slot with other parameters fails
- Nested partials flatten (`partial(partial(f, a), b).func is f`), and a call-site keyword overrides a bound one

## [02]-[OPERATOR_FAMILIES]

Members that differ by the function they apply are one method and one `partialmethod` per member. `Vec` writes its componentwise rule once, and each operator binds the `operator` function it applies:

```python
class Vec(msgspec.Struct, frozen=True):
    x: float
    y: float

    def _zipped(self, op: Callable[[float, float], float], other: Vec | float, /) -> Vec:
        match other:
            case Vec(x=x, y=y):
                return Vec(op(self.x, x), op(self.y, y))
            case _:
                return Vec(op(self.x, other), op(self.y, other))

    __add__ = partialmethod(_zipped, operator.add)
    __sub__ = partialmethod(_zipped, operator.sub)
    __mul__ = partialmethod(_zipped, operator.mul)
    __truediv__ = partialmethod(_zipped, operator.truediv)
```

`(Vec(1.0, 2.0) + Vec(3.0, 4.0)) * 2.0 - 1.0` returns `Vec(7.0, 11.0)`, and `sum(vectors, start=Vec(0.0, 0.0))` folds with no accumulator:
- Both checkers type each operator's result as `Vec` and check no operand, the method's own `match` decides every operand type
- Reflected operators (`__radd__`) bind a swapping operator function, `partialmethod(_zipped, lambda a, b: b - a)` for `__rsub__`
- Expression trees take the same form, each operator building a node of its operation in place of one method per node kind

## [03]-[DISPATCH]

Decisions over values read a table built once or a member that states the decision. `graded` maps each error to a grade through sorted breaks, and `max` reads the worst grade from the `IntEnum` order:

```python
class Grade(IntEnum):
    PASS = 0
    WARN = 1
    FAIL = 2


BREAKS: tuple[float, ...] = (0.5, 0.9)


def graded(errors: Iterable[float], /) -> Grade:
    return max((Grade(bisect(BREAKS, error)) for error in errors), default=Grade.PASS)
```

`Grade` values count the breaks below each grade, and an empty input grades `PASS` through `default`. Select the form by the decision:

| [INDEX] | [DECISION]                             | [FORM]                                                                        |
| :-----: | :------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | Lookup table built once                | `frozendict` keyed by the object answering it (enum member, leaf type, tuple) |
|  [02]   | Threshold                              | `bisect` over sorted breaks into an `IntEnum` or a label tuple                |
|  [03]   | Worst or best of a set                 | `max` or `min` over an `IntEnum` with `default=`                              |
|  [04]   | Short-circuiting boolean               | `any` or `all` over a generator                                               |
|  [05]   | Implementations other modules register | `functools.singledispatch`                                                    |

- Tables keyed by a tuple of members hold every combination, `TABLE[kind, tier]` reads one, and a missing pair raises `KeyError` as a defect
- Tables keyed by a type resolve a subclass through `next(TABLE[base] for base in kind.__mro__ if base in TABLE)`

## [04]-[ITERATION]

Iterators produce values lazily and once, and a consumer that stops early computes only what it read. Select the member by the operation:

| [INDEX] | [OPERATION]      | [MEMBER]                                              |
| :-----: | :--------------- | :---------------------------------------------------- |
|  [01]   | Parallel walk    | `zip(a, b, strict=True)`                              |
|  [02]   | Neighbor pairs   | `pairwise`, `pairwise((None, *items, None))` for ends |
|  [03]   | Fixed chunks     | `batched(items, size, strict=True)`                   |
|  [04]   | Running fold     | `accumulate(items, f, initial=seed)`                  |
|  [05]   | Single fold      | `reduce(f, items, seed)`                              |
|  [06]   | Consecutive runs | `groupby` over input ordered by key                   |
|  [07]   | Host cursor      | `takewhile(advance, count())`                         |
|  [08]   | Chunked reads    | `iter(partial(stream.read, size), b"")`               |
|  [09]   | Transpose        | `zip(*rows)`, `for a, b in [zip(*pairs)]` to bind it  |

`numbered` holds a two-level counter as `accumulate` state, each heading at depth 1 opening a new section and every other heading counting inside it:

```python
def numbered(depths: Iterable[int], /) -> tuple[tuple[int, int], ...]:
    return tuple(accumulate(depths, lambda at, depth: (at[0] + 1, 0) if depth == 1 else (at[0], at[1] + 1), initial=(0, 0)))[1:]
```

`numbered((1, 2, 2, 1, 2))` returns `((1, 0), (1, 1), (1, 2), (2, 0), (2, 1))`, and the slice drops the seed `accumulate` yields first:
- Fold seeds are their operator's identity (`0`, `1`, `()`), and a fold returns its seed for empty input
- Generator functions separate producing values from transforming them, each stage takes an iterable and delegates through `yield from`
- Second passes over a spent iterator read nothing, and a value read twice is a `tuple`

## [05]-[RECURSION]

Recursive functions need a base case that returns a final value, a recursive case that moves closer to it, and a returned value on every path. Python performs no tail-call optimization and bounds recursion depth, and depth and successor sharing select the form:

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

`graph` maps each node to its predecessors, `{"c": ("a", "b"), "b": ("a",), "d": ()}` yields `a` and `d`, then `b`, then `c`, and nodes of one batch can run in parallel. Sorters are local state their generator owns, and `static_order()` serves a flat order.

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
- `compose` takes at most nine steps and `pipe` sixteen, a longer call matches no overload, and a long chain splits into named compositions
- Composition is associative and `compose()` is the identity, regrouping steps into named parts keeps behavior
- `Result.pipe` and `Option.pipe` apply functions to the container, `map` and `bind` apply them to its value

## [07]-[STRATEGIES]

Behavior that varies by format or backend is a generic record of functions with one instance per variant, in place of a base class and a subclass per variant. `Grammar` holds parse, unit, and rule functions over its own parse type, and `REPORTS` holds grammars of different parse types as their bound `report` methods:

```python
class Grammar[T](msgspec.Struct, frozen=True, kw_only=True):
    parse: Callable[[str], T]
    units: Callable[[T], Iterable[tuple[int, str]]]
    rules: frozendict[Callable[[str], bool], str]

    def report(self, text: str, /) -> tuple[str, ...]:
        return tuple(f"{line}: {message}" for line, unit in self.units(self.parse(text)) for rule, message in self.rules.items() if rule(unit))


PROSE = Grammar(parse=str.splitlines, units=lambda lines: enumerate(lines, start=1), rules=frozendict({str.isupper: "shouts", (lambda unit: unit != unit.rstrip()): "trails"}))
CODE = Grammar(
    parse=lambda text: tokenize.generate_tokens(io.StringIO(text).readline),
    units=lambda tokens: ((token.start[0], token.string) for token in tokens if token.type == tokenize.COMMENT),
    rules=frozendict({str.isupper: "shouts"}),
)
REPORTS: frozendict[str, Callable[[str], tuple[str, ...]]] = frozendict({".md": PROSE.report, ".py": CODE.report})
```

`REPORTS[".py"]("x = 1  # LOUD\n")` returns `("1: shouts",)`:
- Both checkers infer `T` from `parse` (`Grammar[list[str]]` for `str.splitlines`) and type each later lambda parameter from it
- `T` is invariant, read by `parse` and taken by `units`, and a `frozendict[str, Grammar[object]]` refuses both instances
