# [H1][ASSAY_ENUM_AND_TYPING_RESEARCH]
>**Dictum:** *One `StrEnum.__new__` member is Cyclopts token, msgspec value, and `match` key at once; a missing case is a type error, not a runtime bug.*

Verified against CPython 3.14.5 `enum`/`howto`, msgspec changelog (PR #211/#352/#606), and `ty` issues #1889/#1454/#1060. Baseline to beat: `coding-python/references/types.md`, which over-reaches `Literal` and *demotes* `StrEnum` to "iteration/identity/dispatch only" — assay's axes need all three plus `__new__` method payloads, so `Literal` is the wrong tool and `model.md`'s `Literal["graph","glob"]` is the exact spam to collapse.

---
## [1][ENUM_MASTERY]
>**Dictum:** *Multi-payload `__new__` + `_add_value_alias_` make one member carry value, behavior, and wire aliases with no parallel `Literal`.*

The `RailStatus` / `Runner` shape is the official `MultiValueEnum` recipe (howto §MultiValueEnum), not a trick: a tuple member assignment passes its elements as positional args to `__new__`/`__init__`; `__new__` sets `_value_` and registers aliases.

```python
from enum import StrEnum, verify, UNIQUE
from typing import Self

@verify(UNIQUE)                                  # @unique bans value-dupes; @verify(UNIQUE) is the decorator form
class RailStatus(StrEnum):
    exit_code: int                               # annotation-only: in __annotations__, NOT the namespace -> not a member
    severity: int                                # both become per-member attrs set by __new__
    SKIP        = "skip", 0, 0, "skipped"         # value, exit_code, severity, *aliases
    EMPTY       = "empty", 0, 1
    OK          = "ok", 0, 2
    UNSUPPORTED = "unsupported", 3, 3
    BUSY        = "busy", 5, 4
    TIMEOUT     = "timeout", 5, 5
    FAILED      = "failed", 1, 6
    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        m = str.__new__(cls, value)              # data type directly, NEVER super().__new__ (lookup-only after creation)
        m._value_ = value; m.exit_code = exit_code; m.severity = severity
        for a in aliases: m._add_value_alias_(a) # 3.13+: maps alias->member in _value2member_map_, ValueError on collision
        return m
    def join(self, other: "RailStatus") -> "RailStatus":   # member method: the monoid lives ON the type
        return max((self, other), key=lambda s: s.severity)
```

| [FACET] | [VERIFIED FACT] | [ASSAY USE] |
| --- | --- | --- |
| Multi-payload `__new__` | tuple member -> positional args; set `_value_` explicitly | `RailStatus`, `Runner.prefix`, `Language.{strategy,suffixes}` |
| `_add_value_alias_` (3.13) | alias->member in `_value2member_map_`; `ValueError` on reuse | `SKIP` accepts `"skipped"` on decode; encode normalizes to `"skip"` |
| `_value2member_map_` | msgspec decode builds its lookup from it, so aliases resolve to the singleton | bridge token round-trip, no `_missing_` |
| annotation-only attr | bare `name: T` is not a member (not in namespace dict), only types the attr | `exit_code`/`severity`/`prefix` payload typing |
| member method/property | `@enum.property` only when a member would collide with `value`/`name` | `RailStatus.join`; no separate projector |
| functional API / `defstruct` | `Enum("X",[...])` exists but loses `__new__` payloads; prefer class form | catalog stays class form; `msgspec.defstruct` for one-off detail |
| `@verify`/`@unique` | `UNIQUE` bans accidental aliases; `CONTINUOUS`/`NAMED_FLAGS` are int/flag-only | guard `RailStatus`, all axes |
| `@nonmember`/`@member` | escape hatches for class-attr-that-isn't / callable-that-is | reserve; not needed if payloads stay in `__new__` |
| `Flag`/`IntFlag` | bitwise composable membership | **no axis is a flag** — `RailStatus` is a *total order* folded by `max(severity)`, not an OR-set; do not reach for `IntFlag` |

One instance, three subsystems: Cyclopts derives choices from members and yields the member; msgspec encodes `_value_`; `match` dispatches the singleton. `RailStatus.OK == "ok"` holds (str subclass) but domain code dispatches on members — see §2 caveat.

---
## [2][ADT_AND_EXHAUSTIVENESS]
>**Dictum:** *`match` over a closed enum/tagged-union ending in `assert_never(x: Never)` turns an unhandled member into a compile-time error.*

`ty` proves exhaustiveness two ways (verified, issue #1889 `[x]` for if/match): the `assert_never` arm becomes reachable (error) when a member is added, and a non-`-> None` function with a missing arm infers an implicit `None` return (error) even *without* `assert_never`.

```python
from typing import assert_never, Never
def exit_for(s: RailStatus) -> int:              # add a RailStatus member without a case -> ty errors HERE
    match s:
        case RailStatus.SKIP | RailStatus.EMPTY | RailStatus.OK: return 0
        case RailStatus.FAILED: return 1
        case RailStatus.UNSUPPORTED: return 3
        case RailStatus.BUSY | RailStatus.TIMEOUT: return 5
        case _ as never: assert_never(never)     # never: Never iff every member is covered

# msgspec tagged-union detail dispatch is structural over the same idea:
def summarize(d: ApiSurface | VerifySummary | TestRun | PackageRun) -> str:
    match d:
        case ApiSurface(shape=k): return k
        case VerifySummary(failed=f): return f"{f} failed"
        case TestRun(killed=k): return f"{k} killed"
        case PackageRun(stage=s): return s
        case _ as never: assert_never(never)
```

**[HARSH] StrEnum `match` narrowing is currently unsound in `ty` (issue #1454, OPEN).** Because `StrEnum` inherits `str.__eq__`, value-pattern dispatch compares by equality, and `ty` mis-narrows when a `Literal[str]` and the enum mix in one subject (false-positive `assert_never`, over-precise per-arm types). Mitigations, all enforced: (a) type every `match` subject as the enum, **never** `str`; (b) never mix `case "ok":` literal arms with `case RailStatus.OK:` arms in the same `match`; (c) the additive-exhaustiveness guarantee (new member -> error) still holds, which is the property assay actually depends on. `Self`/`Never` are 3.11; `assert_never`/`assert_type` are first-class in `ty`.

---
## [3][MODERN_TYPING_INVENTORY]
>**Dictum:** *Adopt the typing levers msgspec does not already own; refuse the ones it makes redundant.*

| [CONSTRUCT] | [PEP / SINCE] | [VERDICT] | [WHERE / WHY] |
| --- | --- | --- | --- |
| `type X = ...` aliases | 695 / 3.12 | **USE** | `Hom`, `Layer`, `RoutePaths`; lazy eval kills forward-ref quoting (model.md self-refs) |
| generic class/func `[T]` | 695 / 3.12 | **USE** | `select`, `compose`, `Result[T, Fault]` wrappers |
| `**P` / `Concatenate` | 612 / 695 | **USE** | `core/aspect.py` (§4) — sole way `ty` keeps the stack transparent |
| TypeVar bounds + defaults | 696 / 3.13 | **USE** sparingly | `[**P, T, E = Never]` identity-of-union default in aspect homs |
| `Self` | 673 / 3.11 | **USE** | every axis `__new__ -> Self` |
| `Never` / `assert_never` | 3.11 | **USE** | exhaustiveness arms (§2) |
| `assert_type` | 3.11 | **USE (tests)** | pin `decode(b, Report).detail` is the variant; pin match narrowing |
| `Annotated[..., msgspec.Meta]` | 593 | **USE** | `gt/ge/le` field constraints visible to `ty` (model.md) |
| `Protocol` | 544 | **USE** | `Source`, `Parser`, engine seam — structural, no ABC |
| `@runtime_checkable` | 544 | **AVOID** | no `isinstance(x, Protocol)` path; beartype + static cover it |
| `dataclass_transform` | 681 / 3.11 | **CONSUME, don't author** | msgspec `Struct` is already decorated (PR #352) — `ty` infers `__init__` for free |
| `override` | 698 / 3.12 | **AVOID** | assay has no method-override inheritance to guard |
| `TypedDict` (+`ReadOnly`) | 589/705 | **AVOID** | msgspec `Struct` owns every shape; `counts: dict[str,int]` is an open map, not a schema |
| `LiteralString` | 675 / 3.11 | **AVOID** | argv is `tuple[str,...]` data, not SQL/shell string interpolation |
| `Literal` (free aliases) | 586 | **REFUSE as axis** | the spam this design exists to kill; keep only as pydantic config scalar (§5) |

---
## [4][DECORATOR_TYPING]
>**Dictum:** *`**P`-correct `Hom -> Hom` factories keep `ty` seeing `P` and `Result[T, Fault]` through all four aspect layers; widen `E`, never erase it.*

```python
from collections.abc import Callable
from functools import wraps
from typing import Concatenate
from expression import Result

type Hom[**P, T, E = Never] = Callable[P, Result[T, E]]

def traced[**P, T, E](*, span: str) -> Callable[[Hom[P, T, E]], Hom[P, T, E]]:
    def deco(fn: Hom[P, T, E]) -> Hom[P, T, E]:
        @wraps(fn)                                          # propagates __wrapped__/__type_params__ for inspect.unwrap + PEP 695
        def w(*a: P.args, **k: P.kwargs) -> Result[T, E]:   # P preserved -> ty checks call sites through the stack
            with _tracer.start_as_current_span(span):
                return fn(*a, **k)                          # returns the SAME Result; no Ok<->Error flip
        return w
    return deco

def needs_ctx[Ctx, **P, T, E](fn: Callable[Concatenate[Ctx, P], Result[T, E]]) -> Hom[P, T, E]:
    @wraps(fn)
    def w(*a: P.args, **k: P.kwargs) -> Result[T, E]:
        return fn(_ctx.get(), *a, **k)                       # Concatenate discharges the injected leading param
    return w
```

`@retried` is the lone exception: it wraps `Spawn = Callable[P, Coroutine[None,None,T]]` on the *exception* channel (stamina), so a domain `Error(Fault)` value flows past untouched. Compose via slot-sorted fold (aspect.md §2). No `Callable[..., Any]`, no `cast` — that erasure is precisely the `ty`-opacity the skill's decorators.md forbids.

---
## [5][HARSH_COLLAPSE_HUNT]
>**Dictum:** *Every `Literal`/string-key/duplicate below is an axis member, a `match`, or a generic waiting to happen.*

| [SITE] | [DEFECT] | [COLLAPSE] |
| --- | --- | --- |
| `model.md` `Language.route: Literal["graph","glob"]` | free `Literal`, contradicts `routing.md`'s `Strategy(StrEnum)` (`closure`/`glob`); vocab even drifts (`graph`≠`closure`) | delete the `Literal`; `Language.strategy: Strategy` carries the `__new__` payload; one canonical name |
| `model.md` `Tool.parser: str = ""` registry-key → impl | stringly-typed dispatch (banned); `catalog.md` already passes a callable `parser=parse_findings` | `parser: Parser | None`, `Parser` a `Protocol[(Completed) -> Detail]`; kill the key→impl table |
| `Mode` overloaded | model/status define capture axis `CAPTURE/STREAM`; catalog uses `CHECK/WRITE/RESTORE/BUILD/RUN/LIST/MUTATION/CLIENT/...` as a *phase* axis — two meanings, conflicting defaults (`CAPTURE` vs `CHECK`) | split: `Mode(StrEnum){CAPTURE,STREAM}` (engine tee) vs a `Phase` axis OR derive phase from `command[0]` against `_SCOPED_VERBS`; never one enum, two meanings |
| `routing.md` `RouteScope = Literal["changed","full"]` | a 2-member `Literal` that is a msgspec wire field AND drives escalation + `place` SOLUTION/FILES (identity + behavior) | promote to `Scope(StrEnum){CHANGED,FULL}` — wire+match+iteration is the StrEnum test, which it meets |
| `TestRun.mutation: str="off"`, `PackageRun.stage: str` | bounded vocabularies as bare `str`; `stage` duplicates yak `Mode` members (`stage/deploy/publish`) | `mutation: MutationMode` StrEnum; reuse the yak phase member as `stage`, not a parallel string |
| `settings.md` `configurations: str \| None` (space-set ⊆ {Debug,Release}) | stringly-typed set behind a validator | `frozenset[Configuration]`; `Configuration=Literal[...]` is the *one* legitimate `Literal` (pydantic JSON-schema config scalar, never a `match` key) |
| per-arm `place`/`_splice` if-style routing | candidate for branch creep | already `match input:` + `assert_never` — keep; do not regress to `if input ==` |

---
## [SUMMARY]

Assay's axes correctly weaponize the official multi-payload `StrEnum.__new__` + `_add_value_alias_` recipe so one member is simultaneously the Cyclopts token, the msgspec-encoded value (PR #211 encodes by `_value_`, decodes value+aliases via `_value2member_map_`), and the `match` key — a posture the `coding-python` baseline actively discourages and therefore must be overridden. **Exhaustiveness pattern:** type each `match` subject as the enum/tagged-union and end with `case _ as never: assert_never(never)`; `ty` (#1889) flags an added-but-unhandled member as a type error there, and even infers an implicit `None` return without the guard — but StrEnum value-pattern *narrowing* is currently unsound in `ty` (#1454, OPEN), so never mix `str` literal arms with member arms in one subject. **Top collapses:** (1) `Language.route: Literal["graph","glob"]` → `Strategy` StrEnum payload; (2) `Tool.parser: str` registry key → `Parser` Protocol callable; (3) the overloaded `Mode` (capture vs phase) must split, with phase ideally derived from `command[0]`. Modern typing to adopt: PEP 695 aliases/generics, `**P`/`Concatenate`, PEP 696 `E=Never` defaults, `Self`, `Never`/`assert_never`, `assert_type` in tests; to refuse as msgspec-redundant: `TypedDict`, `override`, `runtime_checkable`, `LiteralString`, and every free `Literal` axis.

## [FURTHER_CONSIDERATION]

- **`ty` may reject multi-payload member assignment.** `OK = "ok", 0, 2` is a `tuple[str,int,int]` while members type as `RailStatus`; pyright special-cases this but `ty`'s enum support lists "List form not fully supported" (#1889). Build-time verify the axes type-clean before relying on the payload form; if not, the value annotations + `__new__` signature are the disambiguator.
- **Coverage excludes `assert_never` arms.** They are provably unreachable, so `coverage.py` cannot hit them — add `exclude_also = ["assert_never\\(", "case _ as never"]` to keep mutation/coverage rails honest rather than chasing dead lines.
- **`gc=False` + member-as-value reuse.** `RailStatus` shared by reference into many `Struct`s is safe (singletons, no cycle), but if a future detail variant ever holds a back-reference to its owning `Report`, the `gc=False` leak caveat reactivates — keep evidence structs acyclic so the optimization stays free.
