# [FUNCTIONS]

Covers functions bound, passed, and returned as values.

## [01]-[OPERATOR_OVERLOADING]

Members that differ by the function they apply are one method and one `partialmethod` per member. `Term` is an expression over named inputs, each operator binds the function it applies, and nothing evaluates until `at` reads an environment:

```python
class Term(msgspec.Struct, frozen=True):
    at: Callable[[Mapping[str, float]], float]

    def _joined(self, op: Callable[[float, float], float], other: Term | float, /) -> Term:
        return Term(lambda env: op(self.at(env), other.at(env) if isinstance(other, Term) else other))

    __add__ = __radd__ = partialmethod(_joined, operator.add)
    __mul__ = __rmul__ = partialmethod(_joined, operator.mul)
    __sub__ = partialmethod(_joined, operator.sub)
    __rsub__ = partialmethod(_joined, flip(operator.sub))
    __truediv__ = partialmethod(_joined, operator.truediv)
```

`x = Term(lambda env: env["x"])` reads one input, and `(1.0 - 0.5 * x * x).at({"x": 2.0})` returns `-1.0`:
- Commutative operators bind the forward and reflected names to one `partialmethod`, a noncommutative reflected operator binds `flip(op)`
- `partialmethod` passes the instance before its bound arguments, and a `partial` class attribute passes it after them

## [02]-[DISPATCH]

Decisions over values read a member that states the decision or a table built once. `graded` maps each error to a grade through sorted breaks, and `max` reads the worst grade from the `IntEnum` order:

```python
class Grade(IntEnum):
    PASS = 0
    WARN = 1
    FAIL = 2


def graded(errors: Iterable[float], /, *, breaks: tuple[float, float] = (0.5, 0.9)) -> Grade:
    return max((Grade(bisect(breaks, error)) for error in errors), default=Grade.PASS)
```

`Grade` values count the breaks below each grade, `breaks` types one break per member past the first, and an empty input grades `PASS` through `default`. Tables keyed by a tuple of members hold every combination, `TABLE[kind, tier]` reads one, and a missing pair raises `KeyError` as a defect.

## [03]-[ITERATION]

Select the member by the operation:

| [INDEX] | [OPERATION]              | [MEMBER]                                       |
| :-----: | :----------------------- | :--------------------------------------------- |
|  [01]   | Parallel apply           | `map(f, a, b, strict=True)`                    |
|  [02]   | Neighbor pairs with ends | `pairwise((None, *items, None))`               |
|  [03]   | Fixed chunks             | `batched(items, size, strict=True)`            |
|  [04]   | Sorted merge             | `heapq.merge(*runs, key=f)` over sorted inputs |
|  [05]   | Chunked reads            | `iter(partial(stream.read, size), b"")`        |

`filled` repeats each reading across the ticks before the next one, each adjacent pair bounding one `range`:

```python
def filled[V](readings: Sequence[tuple[int, V]], /, *, step: int = 1) -> tuple[tuple[int, V], ...]:
    return (*((tick, value) for (start, value), (end, _) in pairwise(readings) for tick in range(start, end, step)), *readings[-1:])
```

`filled(((0, 1.0), (3, 2.0)))` returns `((0, 1.0), (1, 1.0), (2, 1.0), (3, 2.0))`, and `readings[-1:]` closes with the last reading, or with nothing for empty input.

## [04]-[RECURSION]

Walks over an acyclic graph whose nodes share successors run through `graphlib.TopologicalSorter`, with no recursion depth and no node visited twice. `generations` yields each batch of nodes with every predecessor done, one batch per longest-path layer, from a sorter it owns:

```python
def generations[K: Hashable](graph: Mapping[K, Iterable[K]], /) -> Iterator[tuple[K, ...]]:
    sorter = TopologicalSorter(graph)
    sorter.prepare()
    while sorter.is_active():
        ready = sorter.get_ready()
        yield ready
        sorter.done(*ready)
```

`graph` maps each node to its predecessors, `{"c": ("a", "b"), "b": ("a",), "d": ()}` yields `a` and `d`, then `b`, then `c`, and nodes of one batch can run in parallel. A scheduler calling `done` per finished node in place of per batch starts each node once its own predecessors finish.

## [05]-[PIPELINES]

`compose(f, g)` names a pipeline of unary steps once and applies it where each value enters, and `pipe(value, f, g)` runs steps on one value. `slug` normalizes text left to right, and `tide` applies it before a raising enum lookup that `catch` converts:

```python
slug: Callable[[str], str] = compose(str.strip, str.casefold, partial(re.compile(r"[\s-]+").sub, "_"))


def tide(text: str, /) -> Outcome[Tide]:
    return catch(Tide, exception=ValueError)(slug(text)).map_error(partial(Fault.of, Reason.INVALID, repr(text)))
```

`Tide` is a `StrEnum` with an `auto()` member `HIGH_WATER`, and `tide(" High-Water ")` returns it:
- Steps fixing a later argument bind a method of the fixed object, `partial(pattern.sub, "_")` leaves the text parameter open
- `compose` types at most nine steps and `pipe` sixteen, and a longer chain splits into named compositions
- `compose` returns a closure no worker pickles, and a step crossing `to_process` or `to_interpreter` is a top-level `def` or a `partial` over one
- `Result` and `Option` chains stay fluent method calls

## [06]-[STRATEGIES]

Behavior that varies by format or backend is one generic function taking each varying function as a parameter, its type parameter the value those functions pass each other. `report` splits text into numbered units of its own type and applies rules over that type:

```python
def report[U](units: Callable[[str], Iterable[tuple[int, U]]], rules: Mapping[str, Callable[[U], bool]], text: str, /) -> tuple[str, ...]:
    return tuple(f"{line}: {message}" for line, unit in units(text) for message, rule in rules.items() if rule(unit))
```

`partial(report, <units>, <rules>)` is one format's reporter, and `report(lambda text: ((token.start[0], token) for token in tokenize.generate_tokens(io.StringIO(text).readline)), {"shouted comment": lambda token: token.type == tokenize.COMMENT and token.string.isupper()}, "x = 1  # LOUD\n")` returns `("1: shouted comment",)`:
- `U` is the unit type `units` yields, a `TokenInfo` above, and each rule takes that type
- Reporters over different unit types share `Callable[[str], tuple[str, ...]]`, and a table keyed by suffix holds one `partial` per format
