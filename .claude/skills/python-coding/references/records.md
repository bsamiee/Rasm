# [RECORDS]

Covers record flows from decode to transition, with tagged unions, recursive unions, transitions, and array fields.

## [01]-[LIFECYCLE]

`Line` bounds `sku` and `count` through `Meta`, checks `reserved` against `count` in `__post_init__`, and books stock through a transition. `Fault`, `Outcome`, and `Reason` are the package faults:

```python
type Count = Annotated[int, msgspec.Meta(ge=1)]
type Sku = Annotated[str, msgspec.Meta(pattern=r"^[A-Z]{3}-\d{4}$")]


class Line(msgspec.Struct, frozen=True, kw_only=True):
    sku: Sku
    count: Count
    reserved: int = 0
    price: Decimal

    def __post_init__(self) -> None:
        if not 0 <= self.reserved <= self.count:
            raise ValueError(self)

    @property
    def free(self) -> int:
        return self.count - self.reserved

    @property
    def total(self) -> Decimal:
        return self.count * self.price

    def booked(self, extra: int, /) -> Outcome[Line]:
        return (
            Ok(self)
            .filter_with(lambda line: 0 < extra <= line.free, lambda line: (Fault(reason=Reason.OUTSIDE, subject="extra", member=f"1 to {line.free}", cause=str(extra)),))
            .map(lambda line: msgspec.structs.replace(line, reserved=line.reserved + extra))
        )


LINE = msgspec.json.Decoder(Line)
```

`LINE.decode(b'{"sku": "ABC-0001", "count": 3, "price": "2.50"}')` returns a `Line` with `total` `Decimal("7.50")`. `Decimal` fields decode from a JSON string or number and encode as a string. Rejections read `` Expected `int` >= 1 - at `$.count` `` for a zero count, `` Expected `str` matching regex ... - at `$.sku` `` for a malformed code, and the record's repr from `ValueError(self)` for a `reserved` past `count`, followed by its path inside a container (`` - at `$[0]` ``). `msgspec.convert({"sku": sku, "count": count, "price": price}, Line)` applies the same checks and messages to computed values.

Fields a transition moves keep their bound in `__post_init__`, and `Meta` holds bounds of fields only a decode or `convert` sets: `reserved` takes no `Meta`. Transitions refuse an expected failure before the replace: `booked` returns a fault for an `extra` past `free`, and the `__post_init__` raise stays a defect no valid call reaches. `line.booked(2)` returns `Ok` of a new line and leaves `line` unchanged.

## [02]-[UNIONS]

Tagged cases own their behavior, a `type` alias names the union consumers take, and one codec decodes a collection of cases:

```python
class Figure(msgspec.Struct, frozen=True, kw_only=True, tag=True): ...


class Circle(Figure, frozen=True, kw_only=True):
    radius: float

    @property
    def area(self) -> float:
        return math.pi * self.radius**2

    def scaled(self, factor: float, /) -> Circle:
        return msgspec.structs.replace(self, radius=self.radius * factor)


class Rect(Figure, frozen=True, kw_only=True):
    width: float
    height: float

    @property
    def area(self) -> float:
        return self.width * self.height

    def scaled(self, factor: float, /) -> Rect:
        return msgspec.structs.replace(self, width=self.width * factor, height=self.height * factor)


type Shape = Circle | Rect

SHAPES = msgspec.json.Decoder(tuple[Shape, ...])


def covered(shapes: Iterable[Shape], /) -> float:
    return math.fsum(shape.area for shape in shapes)
```

Encoded cases hold their tag under `type` (`{"type":"Circle","radius":1.0}`), and `SHAPES` dispatches each element by tag. Unknown tags fail as `` Invalid value 'Hex' - at `$[0].type` ``, and a missing tag fails as `` Object missing required field `type` - at `$[0]` ``. Consumers take `Shape` in place of `Figure`: the base declares no `area`, and a `match` over the alias proves coverage. `covered` reads `area` from every case without a `match`. Each `scaled` returns its own case type, and `tuple(shape.scaled(2.0) for shape in shapes)` keeps every case.

## [03]-[RECURSIVE_UNIONS]

Recursive unions hold the union inside a case, and each case folds its parts through their own properties (series and parallel groups of cells):

```python
type Node = Cell | Series | Parallel


class Part(msgspec.Struct, frozen=True, kw_only=True, tag=True): ...


class Cell(Part, frozen=True, kw_only=True):
    capacity: float
    voltage: float
    resistance: float

    def __post_init__(self) -> None:
        if min(self.capacity, self.voltage, self.resistance) <= 0.0:
            raise ValueError(self)

    @property
    def cells(self) -> tuple[Cell, ...]:
        return (self,)

    def aged(self, fade: float, /) -> Cell:
        return msgspec.structs.replace(self, capacity=self.capacity * (1.0 - fade), resistance=self.resistance * (1.0 + fade))


class Group(Part, frozen=True, kw_only=True):
    parts: tuple[Node, ...]

    def __post_init__(self) -> None:
        if not self.parts:
            raise ValueError(self)

    @property
    def cells(self) -> tuple[Cell, ...]:
        return tuple(*part.cells for part in self.parts)

    def aged(self, fade: float, /) -> Self:
        return msgspec.structs.replace(self, parts=tuple(part.aged(fade) for part in self.parts))


class Series(Group, frozen=True, kw_only=True):
    @property
    def capacity(self) -> float:
        return min(part.capacity for part in self.parts)

    @property
    def voltage(self) -> float:
        return math.fsum(part.voltage for part in self.parts)

    @property
    def resistance(self) -> float:
        return math.fsum(part.resistance for part in self.parts)


class Parallel(Group, frozen=True, kw_only=True):
    @property
    def capacity(self) -> float:
        return math.fsum(part.capacity for part in self.parts)

    @property
    def voltage(self) -> float:
        return min(part.voltage for part in self.parts)

    @property
    def resistance(self) -> float:
        return 1.0 / math.fsum(1.0 / part.resistance for part in self.parts)


NODE: msgspec.json.Decoder[Node] = msgspec.json.Decoder(Node)
```

`type Node` precedes the classes it names, an alias value evaluates on first use. `Group` holds what both groups share (`parts`, `cells`, `aged`), and each group adds the ratings that differ: capacity takes the weakest part in series and adds in parallel, voltage adds in series, and resistance adds in series and adds as conductance in parallel. Recursion sits in the properties: a group computes `capacity` from each part's `capacity`, a cell holds `capacity` as a field, and nesting depth bounds the recursion. `cells` flattens leaves in order through comprehension unpacking, and `aged` rebuilds every level and returns `Self`: an aged `Series` stays a `Series`. `Series(parts=(Parallel(parts=(cell, cell)),) * 4)` over `Cell(capacity=3.0, voltage=3.6, resistance=0.03)` rates `6.0` capacity, `14.4` voltage, and `0.06` resistance.

`NODE` validates every level of a nested pack, and an empty group at any depth fails with its path (`` Parallel(parts=()) - at `$.parts[0]` ``). `Node` lists the cases alone, and `Group` decodes as no case (`` Invalid value 'Group' - at `$.type` ``). The annotation on `NODE` restores the `Node` type its alias argument erases.

## [04]-[TRANSITIONS]

Transitions return the next state as a value, and `reduce` replays a history into its final state while `accumulate` yields each state. `History` holds its merge interval beside its entries, newest first:

```python
class History[T](msgspec.Struct, frozen=True, kw_only=True):
    interval: float
    past: tuple[tuple[float, T], ...] = ()

    def added(self, entry: tuple[float, T], /) -> Self:
        match self.past:
            case ((moment, _), *rest) if 0.0 <= entry[0] - moment < self.interval:
                return msgspec.structs.replace(self, past=(entry, *rest))
            case _:
                return msgspec.structs.replace(self, past=(entry, *self.past))


def replayed[T](start: History[T], entries: Iterable[tuple[float, T]], /) -> History[T]:
    return reduce(History.added, entries, start)


def history[T](start: History[T], entries: Iterable[tuple[float, T]], /) -> tuple[History[T], ...]:
    return tuple(accumulate(entries, History.added, initial=start))
```

Entries within `interval` of the newest entry replace it, and every other entry goes first: entries at `0.0`, `0.5`, and `2.0` under an interval of `1.0` leave the ones at `2.0` and `0.5`. The sequence pattern binds the newest entry's moment and every older entry, and its guard reads the record's own setting. Configuration a record holds leaves the unbound `History.added` as the `(state, event) -> state` step `reduce` and `accumulate` take, with no `partial`. `history` returns `start` and the state after each entry, and an empty history replays to `start`.

## [05]-[ARRAY_FIELDS]

`Trace` checks its array's shape in `__post_init__` and derives each new array through a whole-array call:

```python
type Samples = npt.NDArray[np.float64]


class Trace(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    step: float
    volts: Samples

    def __post_init__(self) -> None:
        if self.step <= 0.0 or self.volts.ndim != 1 or self.volts.size < 2:
            raise ValueError(self)

    @property
    def slopes(self) -> Samples:
        return np.diff(self.volts) / self.step

    def clipped(self, floor: float, /) -> Trace:
        return msgspec.structs.replace(self, volts=np.maximum(self.volts, floor))
```

`Trace` records over one array compare unequal and share one `set`. Frozen records refuse field assignment with `AttributeError` and leave array elements writable. `clipped` builds a new array through `np.maximum` and leaves `trace.volts` unchanged. Codecs neither decode nor encode an `ndarray` field (`` Expected `numpy.ndarray`, got `list` ``): a wire record holds `tuple[float, ...]`, and the array record builds after the decode.
