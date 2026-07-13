# [dirty-equals] — declarative partial-structure matchers inside larger equality facts

`dirty-equals` supplies `Is*` matcher objects whose `__eq__` asserts a property rather than a literal — an `IsNow()` matches any near-current datetime, an `IsPartialDict` matches a subset of keys. The repo admits them for partial-structure assertions embedded in a larger fact: a matcher stands in for a nondeterministic or unbounded field so the surrounding structure asserts exactly, never as a replacement for whole-value equality where the value is known.

## [01]-[PACKAGE_SURFACE]

- package: `dirty-equals` · version `0.11` · license `MIT`
- namespace: `dirty_equals`
- asset: pure-Python wheel; no runtime dependency beyond the standard library
- rail: partial equality — a matcher embeds in an `==` fact or an `inline_snapshot` literal

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                    | [KIND]    | [CAPABILITY]                                                              |
| :-----: | :------------------------------------------ | :-------- | :------------------------------------------------------------------------ |
|  [01]   | `DirtyEquals[T]`                            | base      | the root; `__eq__` asserts a property, `&`/`\|`/`~` compose matchers      |
|  [02]   | `AnyThing`                                  | matcher   | matches any value — the total-acceptance leaf for a don't-care field      |
|  [03]   | `IsOneOf`                                   | matcher   | matches when the value equals one of the supplied alternatives            |
|  [04]   | `IsInstance`                                | matcher   | type-membership match; `only_direct_instance=` forbids subclasses         |
|  [05]   | `IsInt` / `IsFloat` / `IsApprox`            | numeric   | bound a number by `gt`/`lt`/`ge`/`le`/`exactly` or a `delta` tolerance    |
|  [06]   | `IsStr` / `IsBytes`                         | text      | bound length, case, or a `regex` pattern on a string or byte string       |
|  [07]   | `IsList` / `IsTuple`                        | sequence  | match items, `positions`, `length`, and `check_order` on a sequence       |
|  [08]   | `IsDict` / `IsPartialDict` / `IsStrictDict` | mapping   | exact, subset, or key-ordered mapping match from keyword/positional keys  |
|  [09]   | `IsDatetime` / `IsNow`                      | temporal  | bound by `delta`/`approx` with `enforce_tz`; `IsNow` alone carries `tz`   |
|  [10]   | `IsUUID` / `IsUrl` / `IsIP` / `IsHash`      | format    | assert a value conforms to a UUID, URL, IP, or hash shape                 |
|  [11]   | `IsJson`                                    | decode    | parse a JSON string, match its decoded structure against a nested matcher |
|  [12]   | `IsPositive` / `IsNegative`                 | predicate | sign predicates over a numeric or coercible value                         |
|  [13]   | `IsTrueLike` / `IsFalseLike`                | predicate | truthiness predicates over a numeric or coercible value                   |

```python signature
class DirtyEquals[T]:
    def __eq__(self, other: object) -> bool: ...
    def __and__(self, other: DirtyEquals[object]) -> DirtyEquals[object]: ...   # both matchers hold
    def __or__(self, other: DirtyEquals[object]) -> DirtyEquals[object]: ...    # either matcher holds
    def __invert__(self) -> DirtyEquals[T]: ...                                 # negate the match
class IsInt(DirtyEquals[int]):
    def __init__(self, *, exactly: int | None = None, approx: int | None = None, delta: int | None = None,
                 gt: int | None = None, lt: int | None = None, ge: int | None = None, le: int | None = None) -> None: ...
class IsPartialDict(DirtyEquals[dict]):
    def __init__(self, *expected_args: dict[object, object], **expected_kwargs: object) -> None: ...   # subset match on named keys
class IsNow(DirtyEquals[datetime]):
    def __init__(self, *, delta: timedelta | int | float = 2, unix_number: bool = False, iso_string: bool = False,
                 format_string: str | None = None, enforce_tz: bool = True, tz: str | tzinfo | None = None) -> None: ...
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                    | [KIND]           | [CAPABILITY]                                                           |
| :-----: | :------------------------------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `value == IsInt(ge=0)`                       | equality fact    | a matcher on either side of `==` asserts the field's property in place |
|  [02]   | `payload == {"n": IsInt(), "at": IsNow()}`   | embedded fact    | matchers occupy nondeterministic fields; sibling keys assert exactly   |
|  [03]   | `body == IsPartialDict(id=IsUUID())`         | subset fact      | assert a subset of keys and leave the rest unconstrained               |
|  [04]   | `field == IsInt(ge=0) & ~IsApprox(0)`        | composed matcher | `&`/`\|`/`~` build a compound predicate without a custom class         |
|  [05]   | `raw == IsJson(IsPartialDict(kind="shape"))` | decode fact      | parse and match a JSON string against a nested matcher                 |

```python signature
from dirty_equals import IsInt, IsNow, IsUUID, IsPartialDict, IsJson, IsApprox
def test_receipt(emit: Callable[[], dict[str, object]]) -> None:
    assert emit() == {"id": IsUUID(4), "count": IsInt(ge=1), "at": IsNow(tz="UTC"), "score": IsApprox(0.5, delta=0.01)}
    assert emit() == IsPartialDict(kind="shape")                        # remaining keys unconstrained
```

## [04]-[IMPLEMENTATION_LAW]

[DIRTY_EQUALS_TOPOLOGY]:
- Every matcher subclasses `DirtyEquals`; a match resolves through `__eq__`, so a matcher drops into any `==` position or a nested structure without a custom comparator.
- `&`/`|`/`~` compose matchers into a compound predicate — a new constraint is an operator combination, never a bespoke matcher class.
- A matcher constructor carries the constraint as keyword parameters (`gt`/`lt`/`ge`/`le`, `delta`, `regex`, `length`, `check_order`); parameterization lives in the arguments, not in matcher proliferation.
- `IsPartialDict` and `IsStrictDict` select subset versus key-ordered matching from the same keyword-key construction shape.

[STACKING]:
- `inline-snapshot`(`.api/inline-snapshot.md`): a matcher lives inside a `snapshot(...)` literal so a nondeterministic field (`IsNow`, `IsUUID`) stays partial while the recorded structure asserts exactly.
- `spec.py`(`../_testkit/spec.py`): a whole-value wire golden proves through `assert_roundtrip` byte identity; a partial fact over a nondeterministic field proves through a `dirty-equals` matcher — the two are orthogonal, never interchangeable.

[LOCAL_ADMISSION]:
- Admitted on the `tests/` dev plane for partial-structure assertions embedded in a larger equality fact.
- A matcher expresses the field that cannot be pinned; a fully known value asserts by literal equality or a snapshot golden, never by a blanket `AnyThing`.

[RAIL_LAW]:
- Package: `dirty-equals`
- Owns: property-based partial equality — numeric bounds, text/format shape, temporal proximity, and subset mapping match through `__eq__`.
- Accept: a matcher occupying a nondeterministic or unbounded field inside an exact structure; `&`/`|`/`~` composition; a matcher nested in an `inline_snapshot` literal.
- Reject: a matcher replacing whole-value equality for a known value; `AnyThing` where a bound is knowable; a custom matcher subclass where an operator composition suffices.
