# [PY_TESTS_API_DIRTY_EQUALS]

`dirty-equals` supplies `Is*` matcher objects whose `__eq__` asserts a property rather than a literal — an `IsNow()` matches any near-current datetime, an `IsPartialDict` matches a subset of keys. Rasm admits them for partial-structure assertions embedded in a larger fact: a matcher stands in for a nondeterministic or unbounded field so the surrounding structure asserts exactly, never as a replacement for whole-value equality where the value is known.

## [01]-[PUBLIC_TYPES]

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

```python
class DirtyEquals[T]:
    def __eq__(self, other: object) -> bool: ...
    def __and__(self, other: DirtyEquals[object]) -> DirtyEquals[object]: ...
    def __or__(self, other: DirtyEquals[object]) -> DirtyEquals[object]: ...
    def __invert__(self) -> DirtyEquals[T]: ...
class IsInt(DirtyEquals[int]):
    def __init__(self, *, exactly: int | None = None, approx: int | None = None, delta: int | None = None,
                 gt: int | None = None, lt: int | None = None, ge: int | None = None, le: int | None = None) -> None: ...
class IsPartialDict(DirtyEquals[dict]):
    def __init__(self, *expected_args: dict[object, object], **expected_kwargs: object) -> None: ...
class IsNow(DirtyEquals[datetime]):
    def __init__(self, *, delta: timedelta | int | float = 2, unix_number: bool = False, iso_string: bool = False,
                 format_string: str | None = None, enforce_tz: bool = True, tz: str | tzinfo | None = None) -> None: ...
```

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                    | [KIND]           | [CAPABILITY]                                                           |
| :-----: | :------------------------------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `value == IsInt(ge=0)`                       | equality fact    | a matcher on either side of `==` asserts the field's property in place |
|  [02]   | `payload == {"n": IsInt(), "at": IsNow()}`   | embedded fact    | matchers occupy nondeterministic fields; sibling keys assert exactly   |
|  [03]   | `body == IsPartialDict(id=IsUUID())`         | subset fact      | assert a subset of keys and leave the rest unconstrained               |
|  [04]   | `field == IsInt(ge=0) & ~IsApprox(0)`        | composed matcher | `&`/`\|`/`~` build a compound predicate without a custom class         |
|  [05]   | `raw == IsJson(IsPartialDict(kind="shape"))` | decode fact      | parse and match a JSON string against a nested matcher                 |

```python
from dirty_equals import IsInt, IsNow, IsUUID, IsPartialDict, IsJson, IsApprox
def test_emitted(emit: Callable[[], dict[str, object]]) -> None:
    assert emit() == {"id": IsUUID(4), "count": IsInt(ge=1), "at": IsNow(tz="UTC"), "score": IsApprox(0.5, delta=0.01)}
    assert emit() == IsPartialDict(kind="shape")
```

## [03]-[IMPLEMENTATION_LAW]

[DIRTY_EQUALS_TOPOLOGY]:
- Every matcher subclasses `DirtyEquals`; a match resolves through `__eq__`, so a matcher drops into any `==` position or a nested structure without a custom comparator.
- `&`/`|`/`~` compose matchers into a compound predicate — a new constraint is an operator combination, never a bespoke matcher class.
- Matcher constructors carry the constraint as keyword parameters (`gt`/`lt`/`ge`/`le`, `delta`, `regex`, `length`, `check_order`); parameterization lives in the arguments, not in matcher proliferation.
- `IsPartialDict` and `IsStrictDict` select subset versus key-ordered matching from the same keyword-key construction shape.

[STACKING]:
- `spec.py`(`../testkit/spec.py`): a whole-value wire fact proves through `assert_roundtrip` byte identity; a partial fact over a nondeterministic field proves through a `dirty-equals` matcher — the two are orthogonal, never interchangeable.

[LOCAL_ADMISSION]:
- Admitted on the `tests/` dev plane for partial-structure assertions embedded in a larger equality fact.
- Matchers express the field that cannot be pinned; a fully known value asserts by literal equality, never by a blanket `AnyThing`.
