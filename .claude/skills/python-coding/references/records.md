# [RECORDS]

Covers record flows from decode to transition, with tagged unions, recursive unions, state machines, and array fields.

## [01]-[LIFECYCLE]

Records enter through a decode or `msgspec.convert` checking `Meta` bounds and `__post_init__`, and change through transitions returning a `msgspec.structs.replace` copy. `Fault`, `Faults`, and `Reason` are the package faults:

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

    def booked(self, extra: int, /) -> Result[Line, Faults]:
        return (
            Ok(self)
            .filter_with(lambda line: 0 < extra <= line.free, lambda _: Fault.of(Reason.INVALID, extra))
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

`NODE` validates every level of a nested pack, and an empty group at any depth fails with its path (`` Parallel(parts=()) - at `$.parts[0]` ``). `Node` lists the cases alone, and `Group` decodes as no case (`` Invalid value 'Group' - at `$.type` ``). `Decoder(tuple[Shape, ...])` types its result from its argument, a bare `type` alias argument types the codec as `Decoder[Any]`, and the annotation on `NODE` restores `Node`.

## [04]-[STATE_MACHINES]

Transitions take configuration, then state, then event, and match on the `(state, event)` pair. `reduce` replays a history into its final state, and `accumulate` yields each state:

```python
class Trip(StrEnum):
    OVERHEAT = "overheat"
    UNDERVOLTAGE = "undervoltage"


class Limits(msgspec.Struct, frozen=True, kw_only=True):
    temperature: float = 60.0
    floor: float = 0.05
    full: float = 0.95


class Mode(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    soc: float


class Idle(Mode, frozen=True, kw_only=True): ...


class Charging(Mode, frozen=True, kw_only=True): ...


class Discharging(Mode, frozen=True, kw_only=True):
    current: float


class Tripped(Mode, frozen=True, kw_only=True):
    trip: Trip


class Signal(msgspec.Struct, frozen=True, kw_only=True, tag=True): ...


class Plugged(Signal, frozen=True, kw_only=True): ...


class Unplugged(Signal, frozen=True, kw_only=True): ...


class Drawn(Signal, frozen=True, kw_only=True):
    current: float


class Sampled(Signal, frozen=True, kw_only=True):
    soc: float
    temperature: float


class Reset(Signal, frozen=True, kw_only=True): ...


type State = Idle | Charging | Discharging | Tripped
type Event = Plugged | Unplugged | Drawn | Sampled | Reset


def transition(limits: Limits, state: State, event: Event, /) -> State:
    match state, event:
        case Tripped(soc=soc), Reset():
            return Idle(soc=soc)
        case Tripped(), _:
            return state
        case _, Sampled(soc=soc, temperature=temperature) if temperature >= limits.temperature:
            return Tripped(soc=soc, trip=Trip.OVERHEAT)
        case Discharging(), Sampled(soc=soc) if soc <= limits.floor:
            return Tripped(soc=soc, trip=Trip.UNDERVOLTAGE)
        case Charging(), Sampled(soc=soc) if soc >= limits.full:
            return Idle(soc=soc)
        case _, Sampled(soc=soc):
            return msgspec.structs.replace(state, soc=soc)
        case Idle(soc=soc) | Discharging(soc=soc), Plugged():
            return Charging(soc=soc)
        case Charging(soc=soc), Unplugged():
            return Idle(soc=soc)
        case Idle(soc=soc) | Discharging(soc=soc), Drawn(current=current) if current > 0.0:
            return Discharging(soc=soc, current=current)
        case Discharging(soc=soc), Drawn():
            return Idle(soc=soc)
        case _:
            return state


def replayed(limits: Limits, start: State, events: Iterable[Event], /) -> State:
    return reduce(partial(transition, limits), events, start)


def history(limits: Limits, start: State, events: Iterable[Event], /) -> tuple[State, ...]:
    return tuple(accumulate(events, partial(transition, limits), initial=start))
```

Arm order states precedence: `Tripped` absorbs every event except `Reset`, an overheat sample trips from any other state, and guards read `limits`. Alternatives (`Idle(soc=soc) | Discharging(soc=soc)`) bind one name in each alternative and share one body. `msgspec.structs.replace(state, soc=soc)` keeps the case and its other fields and moves the `soc` every case shares through `Mode`. Pairs no arm names keep their state through `case _:`, and mypy and ty report a missing return without it.

`partial(transition, limits)` leaves the `(State, Event) -> State` step `reduce` and `accumulate` take. `history` returns `start` and the state after each event, and an empty history replays to `start`. Events are tagged records, and a codec annotated `msgspec.json.Decoder[Event]` reads an event log through `decode_lines`.

## [05]-[ARRAY_FIELDS]

Records holding an array declare `eq=False`, check shape in `__post_init__`, and derive each new array through a whole-array call:

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

`eq=False` makes `==` and `hash` read identity: a record equals itself, a second record over the same array compares unequal, and both enter one `set`. Frozen records refuse field assignment with `AttributeError` and leave array elements writable. `clipped` builds a new array through `np.maximum` and leaves `trace.volts` unchanged. Codecs neither decode nor encode an `ndarray` field (`` Expected `numpy.ndarray`, got `list` ``): a wire record holds `tuple[float, ...]`, and the array record builds after the decode.
