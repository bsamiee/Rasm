# [RECORDS]

Covers records from decode through transition.

## [01]-[FALLIBLE_TRANSITIONS]

Fallible transitions reach `__post_init__` through `replaced`, and its `ValueError` returns as an `INVALID` fault:

```python
def replaced[R: msgspec.Struct](record: R, /, **changes: object) -> Outcome[R]:
    return catch(partial(msgspec.structs.replace, record), exception=ValueError)(**changes).map_error(partial(Fault.of, Reason.INVALID, type(record).__name__))
```

`Stock` holds `count` and `reserved`, and its `__post_init__` raises `ValueError(f"reserved {self.reserved} outside 0 to {self.count}")`. `msgspec.json.decode(b'{"count": 3, "reserved": 4}', type=Stock)` fails with `reserved 4 outside 0 to 3`, and `replaced(stock, reserved=stock.reserved + 2)` twice from `reserved=0` fails with `Stock is invalid. reserved 4 outside 0 to 3`:
- `partial(msgspec.structs.replace, record)` fixes the record type before `catch` erases parameters

## [02]-[KEYED_DIFFS]

Keyed diffs map each changed key to its `(before, after)` pair, and a `None` side names the case. `edits` keeps first-seen key order and drops each key with equal snapshots:

```python
def edits[K, V](before: Mapping[K, V], after: Mapping[K, V], /) -> frozendict[K, tuple[V | None, V | None]]:
    return frozendict({key: (old, new) for key in dict.fromkeys((*before, *after)) if (old := before.get(key)) != (new := after.get(key))})
```

`case (None, new):` matches an insertion, `case (old, None):` a deletion, and `case (old, new):` a change:
- `msgspec.json.encode` writes `null` for a missing side, and `type=frozendict[str, tuple[V | None, V | None]]` reads it back

## [03]-[DEPENDENCY_GRAPHS]

`Plan` refuses an unknown dependency and a cycle at decode, and `TopologicalSorter.prepare` raises the `CycleError` msgspec reports as a `ValidationError`:

```python
class Plan(msgspec.Struct, frozen=True, kw_only=True):
    needs: frozendict[str, frozenset[str]]

    def __post_init__(self) -> None:
        if unknown := frozenset(*needed for needed in self.needs.values()) - self.needs.keys():
            raise ValueError(sorted(unknown))
        TopologicalSorter(self.needs).prepare()
```

`msgspec.json.decode(b'{"needs": {"a": ["b"], "b": ["a"]}}', type=Plan)` fails with `('nodes are in a cycle', ['a', 'b', 'a'])`, `{"a": ["c"]}` fails with `['c']`, and `TopologicalSorter(plan.needs).static_order()` lists each node after its needs:
- Rules a package member checks run in `__post_init__` through that member, and a `ValueError` subclass it raises fails the decode
- `TopologicalSorter` reads a node it never saw as a key as one with no needs, the unknown check states the rule it lacks

## [04]-[RECURSIVE_UNIONS]

Recursive unions take a package type for each leaf and one record for each node, and each operation over them is one function with one recursive `match`. `unmet` returns the failing parts of a condition, and a condition holds where it returns `()`:

```python
type Condition = str | Group


class Group(msgspec.Struct, frozen=True, kw_only=True):
    mode: Literal["all", "any", "none"]
    members: tuple[Condition, ...]


def unmet(condition: Condition, facts: frozenset[str], /) -> tuple[Condition, ...]:
    match condition:
        case Group(mode="all", members=members):
            return tuple(*unmet(member, facts) for member in members)
        case Group(mode="any", members=members) if any(not unmet(member, facts) for member in members):
            return ()
        case Group(mode="none", members=members) if all(unmet(member, facts) for member in members):
            return ()
        case str() if condition in facts:
            return ()
        case _:
            return (condition,)
```

`msgspec.json.decode(raw, type=Condition)` reads `{"mode": "all", "members": ["gpu", {"mode": "none", "members": ["busy"]}, {"mode": "any", "members": ["a", "b"]}]}`, and `unmet` with facts `{"gpu", "busy"}` returns the `none` and `any` groups, a disjunction failing as one part.

## [05]-[TRANSITIONS]

Transitions are methods of the state record. `reduce(History.added, entries, start)` replays a history, and `accumulate(entries, History.added, initial=start)` yields `start` and each later state. `History` holds its merge interval beside its entries, newest first:

```python
class History[T](msgspec.Struct, frozen=True, kw_only=True):
    interval: timedelta
    past: tuple[tuple[datetime, T], ...] = ()

    def added(self, entry: tuple[datetime, T], /) -> Self:
        match self.past:
            case ((moment, _), *rest) if timedelta() <= entry[0] - moment < self.interval:
                return msgspec.structs.replace(self, past=(entry, *rest))
            case _:
                return msgspec.structs.replace(self, past=(entry, *self.past))
```

Entries within `interval` of the newest entry replace it, and every other entry goes first: entries at `0s`, `0.5s`, and `2s` under a `1s` interval leave the ones at `2s` and `0.5s`:
- Generic records decode per parameterization, `type=History[str]` refuses an `int` entry at `$.past[0][1]`

## [06]-[ARRAY_FIELDS]

`Trace` checks its array's shape in `__post_init__`, and each new array derives through a whole-array call:

```python
class Trace(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    step: float
    volts: Samples

    def __post_init__(self) -> None:
        if self.step <= 0.0 or self.volts.ndim != 1 or self.volts.size < 2:
            raise ValueError(f"step {self.step} over shape {self.volts.shape}")

    @property
    def slopes(self) -> Samples:
        return np.diff(self.volts) / self.step
```

Records holding an array take `eq=False` and compare and hash by identity, a default record's `==` raises `ValueError` on the array's truth value and `hash` raises `TypeError`:
- Frozen records refuse field assignment and leave array elements writable, and `replaced(trace, volts=trace.volts[start:stop])` holds a view of its parent's buffer
- Codecs refuse an `ndarray` field (`` Expected `numpy.ndarray`, got `list` - at `$.volts` ``), and a wire record holds `tuple[float, ...]`
- Per-channel values take the shape their consumer takes whole, a `tuple[float, float, float]` for one color and one `(n, 3)` array for many
