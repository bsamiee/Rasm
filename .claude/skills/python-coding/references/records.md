# [RECORDS]

Covers record flows from decode to transition, with generic edit unions, declared graphs, recursive unions, histories, and array fields.

## [01]-[LIFECYCLE]

`Line` bounds `sku` and `count` through `Meta`, checks `reserved` against `count` in `__post_init__`, and books stock through a transition that refuses an expected failure before the replace. `Fault`, `Outcome`, and `Reason` are the package faults:

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

    def booked(self, extra: int, /) -> Outcome[Self]:
        return (
            Ok(self)
            .filter_with(lambda line: 0 < extra <= line.free, lambda line: Fault.of(Reason.OUTSIDE, "extra", extra, member=f"1 to {line.free}"))
            .map(lambda line: msgspec.structs.replace(line, reserved=line.reserved + extra))
        )


LINE = msgspec.json.Decoder(Line)
```

`LINE.decode(b'{"sku": "ABC-0001", "count": 3, "price": "2.50"}')` returns a `Line` with `total` `Decimal("7.50")`, and `Decimal` fields decode from a JSON string or number. Rejections name their path (`` Expected `int` >= 1 - at `$.count` ``), and a `reserved` past `count` reports the record's repr:
- Fields a transition moves keep their bound in `__post_init__`, and `Meta` bounds fields only a decode or `convert` sets
- `booked` returns a fault for an `extra` past `free`, and the `__post_init__` raise stays a defect no valid call reaches
- `__post_init__` raising `TypeError` or `ValueError` reports as a `ValidationError` with its path at decode and `convert`

## [02]-[GENERIC_UNIONS]

Generic unions keep one value type across every case, cases sharing fields extend one private base, and a smart constructor returns `Nothing` for no change. `edits` compares keyed snapshots and returns deletions, changes, and insertions in one tuple:

```python
class _Placed[V](msgspec.Struct, frozen=True, kw_only=True):
    key: str
    values: frozendict[str, V]


class Inserted[V](_Placed[V], frozen=True, kw_only=True): ...


class Deleted[V](_Placed[V], frozen=True, kw_only=True): ...


class Changed[V](msgspec.Struct, frozen=True, kw_only=True):
    key: str
    before: frozendict[str, V]
    after: frozendict[str, V]

    @classmethod
    def of(cls, key: str, before: Mapping[str, V], after: Mapping[str, V], /) -> Option[Self]:
        old = frozendict({name: value for name, value in before.items() if name not in after or after[name] != value})
        new = frozendict({name: value for name, value in after.items() if name not in before or before[name] != value})
        return Some(cls(key=key, before=old, after=new)).filter(lambda change: bool(change.before or change.after))


type Edit[V] = Inserted[V] | Deleted[V] | Changed[V]


def edits[V](before: Mapping[str, Mapping[str, V]], after: Mapping[str, Mapping[str, V]], /) -> tuple[Edit[V], ...]:
    return (
        *(Deleted(key=key, values=frozendict(values)) for key, values in before.items() if key not in after),
        *(*Changed.of(key, before[key], values).to_list() for key, values in after.items() if key in before),
        *(Inserted(key=key, values=frozendict(values)) for key, values in after.items() if key not in before),
    )
```

Keys present on both sides with equal values produce no edit, `Changed.of` keeps changed, removed, and added values alone, and `Inserted` and `Deleted` stay distinct cases a `match` tells apart.

## [03]-[DECLARED_GRAPHS]

`Plan` refuses an unknown dependency and a cycle at decode, and `TopologicalSorter.prepare` raises the `CycleError` msgspec reports as a `ValidationError`:

```python
class Plan(msgspec.Struct, frozen=True, kw_only=True):
    needs: frozendict[str, frozenset[str]]

    def __post_init__(self) -> None:
        if unknown := frozenset(*needed for needed in self.needs.values()) - self.needs.keys():
            raise ValueError(sorted(unknown))
        TopologicalSorter(self.needs).prepare()

    @property
    def order(self) -> tuple[str, ...]:
        return tuple(TopologicalSorter(self.needs).static_order())


PLAN = msgspec.json.Decoder(Plan)
```

`PLAN.decode(b'{"needs": {"a": ["b"], "b": ["a"]}}')` fails with `('nodes are in a cycle', ['a', 'b', 'a'])`, and a decoded `Plan` holds an order every consumer can read:
- `frozendict` and `frozenset` fields decode from JSON objects and arrays

## [04]-[RECURSIVE_UNIONS]

Recursive unions hold the union inside a case, and each operation over them is one function with one recursive `match`. `holds` evaluates a decoded requirement tree against held facts, and `unmet` returns the failing parts of each conjunction at any depth:

```python
type Condition = Has | Not | AllOf | AnyOf


class Rule(msgspec.Struct, frozen=True, kw_only=True, tag=str.lower): ...


class Has(Rule, frozen=True, kw_only=True):
    key: str


class Not(Rule, frozen=True, kw_only=True):
    inner: Condition


class AllOf(Rule, frozen=True, kw_only=True):
    members: tuple[Condition, ...]


class AnyOf(Rule, frozen=True, kw_only=True):
    members: tuple[Condition, ...]


def holds(condition: Condition, facts: frozenset[str], /) -> bool:
    match condition:
        case Has(key=key):
            return key in facts
        case Not(inner=inner):
            return not holds(inner, facts)
        case AllOf(members=members):
            return all(holds(member, facts) for member in members)
        case AnyOf(members=members):
            return any(holds(member, facts) for member in members)


def unmet(condition: Condition, facts: frozenset[str], /) -> tuple[Condition, ...]:
    match condition:
        case AllOf(members=members):
            return tuple(*unmet(member, facts) for member in members)
        case _:
            return () if holds(condition, facts) else (condition,)


CONDITION: msgspec.json.Decoder[Condition] = msgspec.json.Decoder(Condition)
```

`unmet` over `allof(has gpu, not has busy, anyof(has a, has b))` with facts `{"gpu", "busy"}` returns the `Not` and the whole `AnyOf`, a disjunction failing as one part:
- `type` aliases evaluate their value at first use, `Condition` above the classes it names resolves once they exist
- `CONDITION` validates every level, an inner failure names its path (`` Object missing required field `key` - at `$.members[0]` ``)
- `Rule` decodes as no case (`` Invalid value 'rule' - at `$.type` ``), the alias lists leaves alone
- `all` and `any` stop at the first deciding member, and nesting depth bounds the recursion

## [05]-[TRANSITIONS]

`reduce` replays a history into its final state, and `accumulate` yields each state. `History` holds its merge interval beside its entries, newest first:

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

Entries within `interval` of the newest entry replace it, and every other entry goes first: entries at `0.0`, `0.5`, and `2.0` under an interval of `1.0` leave the ones at `2.0` and `0.5`:
- `case ((moment, _), *rest)` binds the newest entry's moment and every older entry, and its guard reads `interval` from the record
- Configuration a record holds leaves the unbound `History.added` as the `(state, event) -> state` step `reduce` and `accumulate` take, with no `partial`
- `history` returns `start` and the state after each entry

## [06]-[ARRAY_FIELDS]

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

`eq=False` compares and hashes each `Trace` by identity, and records over one array compare unequal:
- Frozen records refuse field assignment with `AttributeError` and leave array elements writable
- Codecs neither decode nor encode an `ndarray` field (`` Expected `numpy.ndarray`, got `list` ``), a wire record holds `tuple[float, ...]` and the array record builds after the decode
- Per-channel values take the shape their consumer takes whole, a `tuple[float, float, float]` for one color and one `(n, 3)` array for many, in place of one field per channel
