# [PY_RUNTIME_FAULTS]

One fault family and one `Result`/`Option` rail span the whole branch: `BoundaryFault` is the one tagged union every package returns through — its ingress classes, its `domain` case carrying a sibling's own typed refusal token, and the `aggregate` case keep every member structurally addressable — and `RuntimeRail` is the one `Result[T, BoundaryFault]` carrier every fallible function returns. Domain logic returns `Result`/`Option` and never raises; exceptions convert exactly once at the owning boundary, and interior code receives only the rail. Absence rides the `expression` `Option` directly — no fault-bound alias, since `Option` carries no error slot to bind. `Posture` states the counterpart axis a foreign edge needs and `Option` cannot: whether a value was DECLARED by its producer, DEFAULTED from a named source, or is absent outright.

One fault-lift core backs every application shape — the explicit-thunk `boundary`, the awaitable `async_boundary`, and the `@trapped` decorator — so the sync/async split is one coroutine-detection branch, never a parallel rail. Classification is the ordered data-driven `CLASSIFY` table, `RAISES` is its raise-side twin binding every explicit refusal to a `Leg`-derived subject and a declared `Recovery`, and the same conversion is the trace-egress seam: each caught exception is recorded on the active OTel span inside the one fold, the owner never minting, naming, or ending a span — span lifecycle stays with the measured operation. `latched`, the branch's one-shot install latch, the shared `Depth` walk bound, and the WHOLE instrumentation-scope coordinate — `Scope`/`SCOPES` naming the emitting library beside the `scoped` stamp binding that name to its distribution version and semconv url — live here because faults is the one tier below every consumer, the `identity`, `clock`, `shapes`, and `wire` strata below the observe owner included, so no emitting page in the branch is left minting the unversioned scope a backend cannot join against its versioned siblings.

## [01]-[INDEX]

- [02]-[FAULT]: the closed fault family, the classification table beside the folder-wide raise census and its one seat door, the retriability and absence-posture carriers, the shared walk bound, the rail carriers and disposition-parameterized traversal, the three fault-lift shapes, the carried-fault span fold, the install latch, and the versioned instrumentation-scope stamp.

## [02]-[FAULT]

- Owner: `BoundaryFault` and its rail, tables, and cross-cutting tenants per the fence. Row SHAPE and the `Leg` CONTRACT seat here while each folder mints its own leg roster and `RAISES` table, since S0 imports no sibling yet every folder raises through the one door. `rostered` is the seat DOOR that makes those tables reachable: `retriability` and `facts` resolve a row by SUBJECT, so a folder pushes its rows into the one census at its own module scope and a per-module `DETAILS` fold builds a map nothing reads. Traversal splits by shape: `traversed` folds a homogeneous `Block` of already-evaluated rails under one `Disposition`, while `railed` is the bound `effect.result` builder for free-form interleaved binds whose later steps depend on earlier bound values — a variadic short-circuit collector beside them is `traversed(by=ABORT)` re-spelled and never lands. Three cross-cutting carriers seat beside it for the reason `scoped` does, every consumer plane reaching this tier and none reaching a peer's: `Recovery` states retriability, `Posture` states foreign-edge absence, and `Depth` bounds every walk.
- Cases: `config` versus `boundary` splits on who can repair the refusal — `config` carries a caller-repairable construction refusal (a policy value, roster, credential row, or precondition the same inputs deterministically refuse), while `boundary` carries the seam classification of a provider or runtime raise during work (a codec, render, parse, or engine failure a re-issue may clear), so a render-class or draw-class fault rides `boundary=` and a refused composition rides `config=` in every consumer. `wire` is reserved for explicit code-carrying construction where a numeric protocol/status code is the discriminant; a caught codec exception carries no code, so the `CLASSIFY` `msgspec` row lands it in the subject-carrying `boundary` case. `domain` carries a SIBLING's own refusal token whole — every folder fault union is a `@tagged_union` over `BaseException`, so its case and kwargs cross the funnel as typed evidence a consumer matches on. WHOLE means in-process: that same kwarg-only shape reconstructs through NO pickler, since `Exception.__reduce__` hands empty `args` and the union's one-case guard refuses the `__dict__` it is handed, so a `domain` fault returned on a rail across a worker breaks exactly as a raised token does. `execution/workers#CROSSING` owns that seam and it is the ONLY one: the token lowers to data at the worker floor gate and re-mints parent-side ahead of this funnel, so this family carries a live token and never a wire form, and no page below the crossing spells a serialization of its own. A deadline-owning fence constructs `deadline` through the ONE `expired` fold with its real budget and tripped-axis `cause`, so the subject derives from a rostered row and the case stays outside `RaisedTag`, where only the owning fence holds the bound; a fence whose `Option[float]` budget is absent and the `CLASSIFY` `TimeoutError` row alike read `BUDGET_UNKNOWN` — the declared budget-unknown floor a consumer reads as unspecified, never a true zero deadline, and never a `default_value(0.0)` re-spelled per site.
- Entry: the three lift shapes share one `_convert` and take a rostered `FaultRow`, never a free subject string, so a fence cannot spell a leg its package never declares. `catch` is REQUIRED on all three: an engine boundary narrows over its real multi-class raise surface, and the deleted `= Exception` default was the one clause making a bare-`Exception` funnel the cheapest form at every call site. `catch` never widens past `Exception` — converting the `anyio` cancellation exception into a fault is the forbidden widening, cancellation being scope-owned flow control rather than an ingress class. One catch-all survives per producer plane, at the outermost weave fence where an unclassified raise must not cross a process boundary, and every interior seam names the provider set it reaches.
- Auto: `subject`, `detail`, and `owner` are the three accessors every consumer reads a fault's coordinates through — the raise site, the caught evidence, and the emitting leg — so a re-raise under a caller's own row and a span attribute stamp both compose one accessor instead of re-matching the family. `FAULT_TAG`/`FAULT_SUBJECT`/`FAULT_OWNER` are the span and log KEYS those three publish under, rostered beside `SCOPES` for the same reason and spelling the .NET kernel's own triple, so one vocabulary joins across all three branch ends; `FAULT_CODE` and `FAULT_POSTURE` complete the roster for the transport edge that decodes a peer detail. `facts` is the one structured egress projection every fault-carrying log line spreads whole, so every leaf case carries its own `subject` inline, the roster supplies its `leg`, and no producing site re-derives a coordinate. `retriability` is the ONE re-offer predicate, ordered over two declared sources: a rostered raise answers its own row's posture, and every other fault derives from the ingress class exactly as the standing frozenset read did. `faulted` is the span-side twin and the branch's ONE Error-arm fold for a fault the rail CARRIED into a live span: `_convert` covers the raise this fence caught, `faulted` covers the typed fault that crossed a worker, lane, or sibling boundary and would otherwise leave an `UNSET` span beside an uncorrelated line. Its `**fields` band is the growth axis a stage, subject, or crossing index rides, and the four hand-rolled copies a charter declaring universal behavior had grown collapse onto that one body.
- Packages: `expression`, `beartype`, `msgspec`, `anyio`, `structlog`, `opentelemetry-api`, and `opentelemetry-semantic-conventions` per the fence imports; the OTel dependency is `-api` with the semconv constant surface — the owner reads the active span, `scoped` stamps a caller-supplied factory, and no SDK type or provider instance enters. `msgspec` carries the frozen `Struct` under `FaultRow` and the `Meta` refinement under `Bound`. `structlog` enters as the PACKAGE alone, whose `get_logger` proxy resolves the configured chain at first bind, so this tier composes the log egress without importing the `observability/logging` module the rail seats above it.
- Growth: a new fault class is one `case()` with one `facts`, `subject`, and `detail` arm; a new deadline site is one `expired` call, never a `BoundaryFault(deadline=...)` literal; a new exception family is one ordered `CLASSIFY` row reaching every lift shape and the trace weave; a new raise is ONE `FaultRow` anchor in that folder's own `RAISES` table under a member of its own `<Folder>Leg` roster, and a folder's first raise mints that roster beside the table in the same pass; a new re-offer state is one `Recovery` case with one `widest` arm; a new absence source is a `defaulted` value, never a `Posture` case; a new traversal output shape is one `Disposition` member with one fold arm; a new span-side error dimension is one `**fields` key at a `faulted` call site, never a second fold; a new instrumentation scope is one `Scope` member with one `SCOPES` row; a semconv bump one `Schemas` member swap reaching every meter, tracer, logger, and `Resource` at once; a fourth scope-minting API one `scoped` call, the port already covering any factory sharing the positional `(name, version, provider, schema_url)` shape; a new one-shot install owner is one `@latched(...)` application.
- Boundary: no C# `Expected` clone and no exception taxonomy copied from a .NET owner. Retriability keys on the fault's own roster row and `FaultTag`, and never imports `reliability/resilience#RESILIENCE` `RetryClass` — resilience depends on faults, never the reverse; the rail maps exceptions to fault classes, the policy table maps retry classes to exception sets, and the two meet through the rail outcome, the `Recovery` value, and the exported `spelled` matcher alone, which resilience reads and never re-derives. Precedence for retriability is TWO rungs with one declared source each, the row's declared posture over the tag-set derivation; a peer-STATED posture is a third rung above both and has no carrier here, since it decodes at `transport/serve#SERVE` and rides `Option[Recovery]` where a legacy frame states nothing.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import inspect
import sys
from collections.abc import Awaitable, Callable
from enum import StrEnum
from functools import wraps
from threading import Lock
from importlib.metadata import distributions
from importlib.util import find_spec
from typing import Annotated, Any, Final, Literal, Protocol, assert_never, overload, runtime_checkable

import anyio
import msgspec
import structlog
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Ok, Option, Result, Some, case, effect, tag, tagged_union
from expression.collections import Block, Map
from opentelemetry import trace
from opentelemetry.semconv.schemas import Schemas
from opentelemetry.trace import Status, StatusCode

# --- [TYPES] ----------------------------------------------------------------------------

type FaultTag = Literal["config", "resource", "deadline", "api", "import_", "wire", "boundary", "domain", "aggregate"]
type Catch = type[BaseException] | tuple[type[BaseException], ...]
type ClassifyMarker = Catch | frozenset[str]
type ClassifyRow = tuple[ClassifyMarker, Callable[[str, BaseException], "BoundaryFault"]]
type RuntimeRail[T] = Result[T, "BoundaryFault"]
type Trapped[**P, T] = Callable[P, RuntimeRail[T]] | Callable[P, Awaitable[RuntimeRail[T]]]
type Bound = Annotated[int, msgspec.Meta(ge=DEPTH_FLOOR)]

type RaisedTag = Literal["config", "resource", "api", "import_", "boundary"]


class Disposition(StrEnum):
    ABORT = "abort"
    ACCUMULATE = "accumulate"
    PARTITION = "partition"


class Scope(StrEnum):
    WIRE = "wire"
    METER = "meter"
    LOGGER = "logger"
    SERVICE = "service"
    RESILIENCE = "resilience"
    IDENTITY = "identity"
    EVIDENCE = "evidence"
    RECIPE = "recipe"
    WORKERS = "workers"
    PROFILES = "profiles"
    JOURNAL = "journal"


@runtime_checkable
class Leg(Protocol):
    value: str


class RuntimeLeg(StrEnum):
    OBSERVE = "runtime.observe"
    LOGGING = "runtime.logging"
    METRICS = "runtime.metrics"
    HOOKS = "runtime.hooks"
    PROFILES = "runtime.profiles"
    TELEMETRY = "runtime.telemetry"
    BUNDLE = "runtime.bundle"
    JOURNAL = "runtime.journal"
    FAULTS = "runtime.faults"
    RESILIENCE = "runtime.resilience"
    ROOTS = "runtime.roots"
    SERVE = "runtime.serve"
    SHAPES = "runtime.shapes"
    WIRE = "runtime.wire"
    EVENT = "runtime.event"
    BINDING = "runtime.binding"
    FILTER = "runtime.filter"
    ADMISSION = "runtime.admission"
    LANES = "runtime.lanes"
    WORKERS = "runtime.workers"
    RECIPE = "runtime.recipe"
    CLOCK = "runtime.clock"
    IDENTITY = "runtime.identity"
    REPRODUCTION = "runtime.reproduction"
    EVIDENCE = "runtime.evidence"


@runtime_checkable
class Tagged(Protocol):
    tag: str


class ScopeAcquire[T](Protocol):
    def __call__(self, name: str, version: str, provider: None, schema_url: str, /) -> T: ...


@tagged_union(frozen=True)
class Recovery:
    tag: Literal["terminal", "transient", "throttled"] = tag()
    terminal: None = case()
    transient: None = case()
    throttled: float = case()

    @property
    def offered(self) -> bool:
        return self.tag != "terminal"

    @property
    def window(self) -> Option[float]:
        return Some(self.throttled) if self.tag == "throttled" else Nothing

    @staticmethod
    def widest(left: "Recovery", right: "Recovery", /) -> "Recovery":
        match (left.tag, right.tag):
            case ("transient", _) | (_, "transient"):
                return TRANSIENT
            case ("throttled", "throttled"):
                return Recovery(throttled=min(left.throttled, right.throttled))
            case ("throttled", _):
                return left
            case (_, "throttled"):
                return right
            case ("terminal", "terminal"):
                return TERMINAL
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class Posture[T]:
    tag: Literal["declared", "defaulted", "absent"] = tag()
    declared: T = case()
    defaulted: tuple[T, str] = case()
    absent: None = case()

    @staticmethod
    def of_optional(value: T | None) -> "Posture[T]":
        return Posture(absent=None) if value is None else Posture(declared=value)

    @staticmethod
    def of_option(option: Option[T]) -> "Posture[T]":
        return option.map(lambda value: Posture(declared=value)).default_value(Posture(absent=None))

    def option(self) -> Option[T]:
        match self:
            case Posture(tag="declared", declared=value) | Posture(tag="defaulted", defaulted=(value, _)):
                return Some(value)
            case Posture(tag="absent"):
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def source(self) -> Option[str]:
        match self:
            case Posture(tag="defaulted", defaulted=(_, source)):
                return Some(source)
            case Posture(tag="declared") | Posture(tag="absent"):
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class Depth:
    tag: Literal["bounded", "fixpoint"] = tag()
    bounded: Bound = case()
    fixpoint: None = case()

    @staticmethod
    def of(bound: int) -> RuntimeRail["Depth"]:
        return Ok(Depth(bounded=bound)) if bound >= DEPTH_FLOOR else Error(DEPTH_BOUND.raised(str(bound)))

    def stepped(self) -> Option["Depth"]:
        match self:
            case Depth(tag="fixpoint"):
                return Some(self)
            case Depth(tag="bounded", bounded=1):
                return Nothing
            case Depth(tag="bounded", bounded=left):
                return Some(Depth(bounded=left - 1))
            case _ as unreachable:
                assert_never(unreachable)

    def exhausted[L: Leg](self, walk: "FaultRow[L]", /) -> "BoundaryFault":
        return DEPTH_SPENT.raised(walk.subject, self.spelled)

    @property
    def spelled(self) -> str:
        return "fixpoint" if self.tag == "fixpoint" else str(self.bounded)


# --- [CONSTANTS] ------------------------------------------------------------------------

DEPTH_FLOOR: Final[int] = 1

BUDGET_UNKNOWN: Final[float] = 0.0

PACKAGE: Final[str] = "rasm"

TERMINAL: Final[Recovery] = Recovery(terminal=None)
TRANSIENT: Final[Recovery] = Recovery(transient=None)

FAULT_CODE: Final[str] = "rasm.fault.code"
FAULT_OWNER: Final[str] = "rasm.fault.owner"
FAULT_POSTURE: Final[str] = "rasm.fault.posture"
FAULT_SUBJECT: Final[str] = "rasm.fault.subject"
FAULT_TAG: Final[str] = "rasm.fault.tag"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class BoundaryFault:
    tag: FaultTag = tag()
    config: tuple[str, str] = case()
    resource: tuple[str, str] = case()
    deadline: tuple[str, float, str] = case()
    api: tuple[str, str] = case()
    import_: tuple[str, str] = case()
    wire: tuple[str, int] = case()
    boundary: tuple[str, str] = case()
    domain: tuple[str, Tagged] = case()
    aggregate: tuple["BoundaryFault", ...] = case()

    @staticmethod
    def of[L: Leg](at: "FaultRow[L]", cause: BaseException) -> "BoundaryFault":
        match cause:
            case Tagged() as token:
                return BoundaryFault(domain=(at.subject, token))
            case _:
                matched = CLASSIFY.choose(lambda row: Some(row[1](at.subject, cause)) if _hits(row[0], cause) else Nothing)
                return matched.try_head().default_with(lambda: BoundaryFault(boundary=(at.subject, str(cause) or type(cause).__name__)))

    @staticmethod
    def combine(left: "BoundaryFault", right: "BoundaryFault") -> "BoundaryFault":
        match (left, right):
            case (BoundaryFault(tag="aggregate"), BoundaryFault(tag="aggregate")):
                return BoundaryFault(aggregate=(*left.aggregate, *right.aggregate))
            case (BoundaryFault(tag="aggregate"), _):
                return BoundaryFault(aggregate=(*left.aggregate, right))
            case (_, BoundaryFault(tag="aggregate")):
                return BoundaryFault(aggregate=(left, *right.aggregate))
            case _:
                return BoundaryFault(aggregate=(left, right))

    @property
    def subject(self) -> str:
        match self:
            case BoundaryFault(tag="aggregate"):
                return "aggregate"
            case BoundaryFault(tag="deadline", deadline=(subject, _, _)):
                return subject
            case BoundaryFault(tag="wire", wire=(subject, _)):
                return subject
            case (
                BoundaryFault(config=(subject, _))
                | BoundaryFault(resource=(subject, _))
                | BoundaryFault(api=(subject, _))
                | BoundaryFault(import_=(subject, _))
                | BoundaryFault(boundary=(subject, _))
                | BoundaryFault(domain=(subject, _))
            ):
                return subject
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def detail(self) -> str:
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return ",".join(member.tag for member in members)
            case BoundaryFault(tag="deadline", deadline=(_, budget, cause)):
                return f"{cause}:{budget}"
            case BoundaryFault(tag="wire", wire=(_, code)):
                return str(code)
            case BoundaryFault(tag="domain", domain=(_, token)):
                return token.tag
            case (
                BoundaryFault(config=(_, detail))
                | BoundaryFault(resource=(_, detail))
                | BoundaryFault(api=(_, detail))
                | BoundaryFault(import_=(_, detail))
                | BoundaryFault(boundary=(_, detail))
            ):
                return detail
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def owner(self) -> Option[str]:
        return _detail(self.subject).map(lambda row: row.leg.value)

    @property
    def ordinal(self) -> Option[int]:
        return _SEATED.try_find(self.subject).map(lambda cell: cell.ordinal)

    @property
    def defect(self) -> Option[str]:
        return _detail(self.subject).map(lambda row: row.defect)

    @property
    def coordinates(self) -> Block[tuple[str, str]]:
        return _detail(self.subject).map(lambda row: _coordinates(row.slots, self.detail)).default_value(Block.empty())

    def retriability(self, codes: frozenset[FaultTag]) -> Recovery:
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return Block.of_seq(members).fold(lambda folded, member: Recovery.widest(folded, member.retriability(codes)), TERMINAL)
            case _:
                declared = _detail(self.subject).map(lambda row: row.retriability)
                return declared.default_value(TRANSIENT if self.tag in codes else TERMINAL)

    def recoverable(self, codes: frozenset[FaultTag]) -> bool:
        return self.retriability(codes).offered

    def facts(self) -> dict[str, object]:
        seat: dict[str, object] = self.owner.map(lambda leg: {"leg": leg}).default_value({})
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return {"tag": "aggregate", "subject": self.subject, "members": ",".join(m.tag for m in members)}
            case BoundaryFault(tag="deadline", deadline=(_, budget, cause)):
                return {"tag": "deadline", "subject": self.subject, "budget": budget, "cause": cause} | seat
            case BoundaryFault(tag="wire", wire=(_, code)):
                return {"tag": "wire", "subject": self.subject, "code": code} | seat
            case BoundaryFault(tag="domain", domain=(_, token)):
                return {"tag": "domain", "subject": self.subject, "case": token.tag, "evidence": getattr(token, token.tag)} | seat
            case (
                BoundaryFault(tag=tag, config=(_, detail))
                | BoundaryFault(tag=tag, resource=(_, detail))
                | BoundaryFault(tag=tag, api=(_, detail))
                | BoundaryFault(tag=tag, import_=(_, detail))
                | BoundaryFault(tag=tag, boundary=(_, detail))
            ):
                return {"tag": tag, "subject": self.subject, "detail": detail} | seat
            case _ as unreachable:
                assert_never(unreachable)


class FaultRow[L: Leg](msgspec.Struct, frozen=True):
    leg: L
    point: str
    arm: RaisedTag
    defect: str
    retriability: Recovery
    slots: tuple[str, ...] = ()

    @property
    def subject(self) -> str:
        return f"{self.leg.value}.{self.point}"

    def raised(self, *subjects: str) -> "BoundaryFault":
        detail = ":".join((self.defect, *(f"{slot}={value}" for slot, value in zip(self.slots, subjects, strict=True))))
        match self.arm:
            case "config":
                return BoundaryFault(config=(self.subject, detail))
            case "resource":
                return BoundaryFault(resource=(self.subject, detail))
            case "api":
                return BoundaryFault(api=(self.subject, detail))
            case "import_":
                return BoundaryFault(import_=(self.subject, detail))
            case "boundary":
                return BoundaryFault(boundary=(self.subject, detail))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def seated(seat: "Map[str, Seat]", row: "FaultRow[Leg]", /) -> "Map[str, Seat]":
        module = f"{PACKAGE}.{row.leg.value}"
        if module not in sys.modules and find_spec(module) is None:
            raise ModuleNotFoundError(module)
        if row.subject in seat:
            raise KeyError(row.subject)
        ordinal = 1 + sum(1 for cell in seat.values() if cell.row.leg.value == row.leg.value)
        return seat.add(row.subject, Seat(row=row, ordinal=ordinal))


class Seat(msgspec.Struct, frozen=True, gc=False):
    row: FaultRow[Leg]
    ordinal: int


# --- [TABLES] ---------------------------------------------------------------------------

_SEATED: Map[str, Seat] = Map.empty()
_SEAT_GATE: Final[Lock] = Lock()


def rostered[L: Leg](rows: Block[FaultRow[L]], /) -> Block[FaultRow[L]]:
    global _SEATED
    with _SEAT_GATE:
        _SEATED = rows.fold(FaultRow.seated, _SEATED)
    return rows


def _coordinates(slots: tuple[str, ...], detail: str) -> Block[tuple[str, str]]:
    heads = tuple(detail.index(f":{slot}=") for slot in slots)
    return Block.of_seq(
        (slot, detail[head + len(slot) + 2 : heads[index + 1] if index + 1 < len(heads) else len(detail)])
        for index, (slot, head) in enumerate(zip(slots, heads, strict=True))
    )


def _detail(subject: str) -> Option[FaultRow[Leg]]:
    return _SEATED.try_find(subject).map(lambda cell: cell.row)


CLASSIFY: Final[Block[ClassifyRow]] = Block.of_seq([
    (TimeoutError, lambda subject, cause: BoundaryFault(deadline=(subject, BUDGET_UNKNOWN, str(cause) or type(cause).__name__))),
    ((msgspec.ValidationError, msgspec.DecodeError), lambda subject, cause: BoundaryFault(boundary=(subject, type(cause).__name__))),
    (BeartypeCallHintViolation, lambda subject, cause: BoundaryFault(api=(subject, type(cause).__name__))),
    (ImportError, lambda subject, cause: BoundaryFault(import_=(subject, type(cause).__name__))),
    (
        frozenset({"loky.process_executor.LokyRecursionError", "asyncssh.misc.HostKeyNotVerifiable", "asyncssh.misc.PermissionDenied"}),
        lambda subject, cause: BoundaryFault(config=(subject, type(cause).__name__)),
    ),
    (
        frozenset({
            "loky.process_executor.TerminatedWorkerError",
            "loky.process_executor.BrokenProcessPool",
            "loky.process_executor.ShutdownExecutorError",
            "concurrent.futures.process.BrokenProcessPool",
            "concurrent.futures.interpreter.BrokenInterpreterPool",
            "pebble.common.types.ProcessExpired",
        }),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
    (
        frozenset({"asyncssh.misc.DisconnectError", "asyncssh.misc.ChannelOpenError"}),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
    (
        (
            anyio.BrokenWorkerProcess,
            anyio.BrokenWorkerInterpreter,
            anyio.BrokenResourceError,
            anyio.BusyResourceError,
            anyio.ClosedResourceError,
            anyio.ConnectionFailed,
            OSError,
        ),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
])

DEPTH_BOUND: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.FAULTS, point="bound", arm="config", defect="depth-below-floor", retriability=TERMINAL, slots=("bound",)
)
DEPTH_SPENT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.FAULTS, point="depth", arm="boundary", defect="depth-exhausted", retriability=TERMINAL, slots=("walk", "bound")
)

# --- [RUNTIME_RAISES]

ADMISSION_HOSTS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="hosts", arm="resource", defect="known-hosts-unreadable", retriability=TERMINAL
)
BACKEND_CLAIMANT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="claimant", arm="config", defect="contract-claimant-refused", retriability=TERMINAL,
    slots=("ordinal", "refusal")
)
BACKEND_CONTRACT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="contract", arm="config", defect="contract-invariant", retriability=TERMINAL,
    slots=("reason", "subjects")
)
BACKEND_MERGE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="merge", arm="config", defect="contract-invariant", retriability=TERMINAL, slots=("reason", "subjects")
)
BACKEND_MINT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="mint", arm="config", defect="contract-invariant", retriability=TERMINAL, slots=("reason", "subjects")
)
PROFILE_GRANT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="grant", arm="config", defect="isolation-grant-absent", retriability=TERMINAL,
    slots=("axis", "isolation", "feature")
)
PROFILE_HOST: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="host", arm="config", defect="in-host-without-descriptor", retriability=TERMINAL, slots=("axis",)
)
SECRET_NAME: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="secret", arm="config", defect="name-breaches-alphabet", retriability=TERMINAL,
    slots=("name", "pattern")
)
SECRET_READ: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="read", arm="resource", defect="secret-read-refused", retriability=TRANSIENT
)
TENANCY_GRADE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="grade", arm="config", defect="grade-above-ceiling", retriability=TERMINAL, slots=("grade", "issuer")
)
TENANCY_ISSUER: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="issuer", arm="config", defect="unrostered-issuer", retriability=TERMINAL, slots=("issuer",)
)
TENANCY_SCOPE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="scope", arm="config", defect="tenant-scope-mismatch", retriability=TERMINAL,
    slots=("axis", "principal")
)
TENANCY_TENANT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ADMISSION, point="tenant", arm="config", defect="issuer-ungranted-tenant", retriability=TERMINAL,
    slots=("issuer", "tenant")
)
BUNDLE_ARCHIVE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BUNDLE, point="archive", arm="boundary", defect="archive-refused", retriability=TERMINAL
)
BUNDLE_COLLECT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BUNDLE, point="collect", arm="boundary", defect="collector-refused", retriability=TERMINAL
)
BINDING_ADMIT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING,
    point="admit",
    arm="config",
    defect="binding-content-refused",
    retriability=TERMINAL,
    slots=("binding", "content"),
)
BINDING_CONNECT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING, point="connect", arm="resource", defect="binding-connect-failed", retriability=TRANSIENT
)
BINDING_DECODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING,
    point="decode",
    arm="boundary",
    defect="binding-decode-failed",
    retriability=TERMINAL,
    slots=("binding", "cause"),
)
BINDING_ENCODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING, point="encode", arm="boundary", defect="binding-encode-failed", retriability=TERMINAL
)
BINDING_DRAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING,
    point="drain",
    arm="boundary",
    defect="binding-drain-failed",
    retriability=TERMINAL,
    slots=("binding", "cause"),
)
BINDING_SETTLE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING,
    point="settle",
    arm="boundary",
    defect="binding-settlement-failed",
    retriability=TRANSIENT,
    slots=("binding", "cause"),
)
BINDING_TRANSACTION: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BINDING,
    point="transaction",
    arm="boundary",
    defect="binding-transaction-failed",
    retriability=TRANSIENT,
    slots=("binding", "cause"),
)
CLOCK_CARRIER: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.CLOCK, point="carrier", arm="boundary", defect="carrier-decode-failed", retriability=TERMINAL
)
CLOCK_LAYOUT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.CLOCK, point="layout", arm="config", defect="cell-layout-drift", retriability=TERMINAL, slots=("drift",)
)
CLOCK_SEALED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.CLOCK, point="sealed", arm="boundary", defect="layout-probe-failed", retriability=TERMINAL
)
EVENT_DOMAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="domain", arm="config", defect="unrostered-domain", retriability=TERMINAL, slots=("domain",)
)
EVENT_EXTENSION: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="extension", arm="boundary", defect="extension-admit-failed", retriability=TERMINAL
)
EVENT_DECODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="decode", arm="boundary", defect="event-format-decode-failed", retriability=TERMINAL
)
EVENT_ENCODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="encode", arm="boundary", defect="event-format-encode-failed", retriability=TERMINAL
)
EVENT_FORMAT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT,
    point="format",
    arm="config",
    defect="event-format-unavailable",
    retriability=TERMINAL,
    slots=("format", "mode"),
)
EVENT_LAG: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="lag", arm="config", defect="recorded-precedes-occurred", retriability=TERMINAL
)
EVENT_MINT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT,
    point="mint",
    arm="boundary",
    defect="attribute-set-refused",
    retriability=TERMINAL,
    slots=("attribute", "finding"),
)
EVENT_NAIVE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="naive", arm="config", defect="naive-stamp", retriability=TERMINAL
)
EVENT_SOURCE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT,
    point="source",
    arm="config",
    defect="malformed-capability-reference",
    retriability=TERMINAL,
    slots=("reference",),
)
EVENT_TYPE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVENT, point="type", arm="config", defect="malformed-type", retriability=TERMINAL, slots=("spelling",)
)
EVIDENCE_BUDGET: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVIDENCE, point="budget", arm="boundary", defect="tree-budget-spent", retriability=TERMINAL
)
EVIDENCE_GRAMMAR: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVIDENCE, point="grammar", arm="config", defect="uncovered-grammar", retriability=TERMINAL, slots=("probe", "language")
)
EVIDENCE_MATCHES: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVIDENCE, point="matches", arm="resource", defect="match-limit-truncated", retriability=TRANSIENT,
    slots=("probe", "language")
)
EVIDENCE_REFLECT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.EVIDENCE, point="reflect", arm="import_", defect="distribution-unreachable", retriability=TERMINAL
)
HOOKS_REGISTER: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.HOOKS, point="register", arm="config", defect="roster-refused", retriability=TERMINAL
)
HOOKS_SUBSCRIBE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.HOOKS, point="subscribe", arm="config", defect="attach-refused", retriability=TERMINAL
)
HOOKS_RELEASE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.HOOKS, point="release", arm="config", defect="custody-unclaimed", retriability=TERMINAL
)
HOOKS_ISOLATED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.HOOKS, point="isolated", arm="boundary", defect="isolation-sink-refused", retriability=TERMINAL
)
HOOKS_PAYLOAD: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.HOOKS, point="payload", arm="config", defect="payload-type-mismatch", retriability=TERMINAL
)
HOOKS_TAP: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.HOOKS, point="tap", arm="boundary", defect="subscriber-refused", retriability=TERMINAL
)
FILTER_PARSE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.FILTER, point="parse", arm="boundary", defect="expression-unparseable", retriability=TERMINAL
)
FILTER_SETTINGS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.FILTER, point="settings", arm="config", defect="unadmitted-protocol-settings", retriability=TERMINAL,
    slots=("protocol", "keys")
)
IDENTITY_DERIVE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.IDENTITY, point="derive", arm="boundary", defect="canonical-encode-failed", retriability=TERMINAL
)
IDENTITY_FMT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.IDENTITY, point="fmt", arm="config", defect="fmt-breaches-grammar", retriability=TERMINAL, slots=("fmt", "pattern")
)
JOURNAL_APPEND: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="append", arm="boundary", defect="ledger-append-refused", retriability=TRANSIENT
)
JOURNAL_CENSUS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="census", arm="config", defect="measures-unrostered", retriability=TERMINAL, slots=("measures",)
)
JOURNAL_CHARGE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="charge", arm="boundary", defect="rate-arithmetic-refused", retriability=TERMINAL
)
JOURNAL_CRYPTO: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="crypto", arm="boundary", defect="aead-refused", retriability=TERMINAL, slots=("axis", "detail")
)
JOURNAL_CUSTODY: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="custody", arm="config", defect="key-material-unbound", retriability=TERMINAL, slots=("service",)
)
JOURNAL_DERIVED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="derived", arm="boundary", defect="derived-write-refused", retriability=TERMINAL
)
JOURNAL_DRAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="shutdown", arm="boundary", defect="journal-close-refused", retriability=TERMINAL
)
JOURNAL_HEX: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="hex", arm="boundary", defect="kek-hex-malformed", retriability=TERMINAL
)
JOURNAL_INSTANT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="instant", arm="config", defect="naive-datetime", retriability=TERMINAL
)
JOURNAL_KEK: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="kek", arm="config", defect="kek-width-mismatch", retriability=TERMINAL, slots=("width", "expected")
)
JOURNAL_OFFER: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="offer", arm="resource", defect="intake-closed", retriability=TERMINAL
)
JOURNAL_PERIOD: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="period", arm="config", defect="window-bounds-inverted", retriability=TERMINAL
)
JOURNAL_PORT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="port", arm="config", defect="port-members-unmet", retriability=TERMINAL, slots=("members",)
)
JOURNAL_RATE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="rate", arm="config", defect="unrated-resource", retriability=TERMINAL, slots=("resource",)
)
JOURNAL_RETIRED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="record", arm="config", defect="custody-retired", retriability=TERMINAL
)
JOURNAL_UNBOUND: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="ledger", arm="config", defect="ledger-unbound", retriability=TERMINAL, slots=("verb",)
)
JOURNAL_UNDRAINED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.JOURNAL, point="drain", arm="config", defect="drain-unbound-or-owned", retriability=TERMINAL
)
LANES_EXPORT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LANES, point="export", arm="boundary", defect="span-export-failed", retriability=TERMINAL
)
LANES_FRONT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LANES, point="front", arm="boundary", defect="front-cancelled", retriability=TERMINAL
)
LANES_INLINE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LANES, point="inline", arm="boundary", defect="inline-body-refused", retriability=TERMINAL
)
LANES_ISOLATION: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LANES, point="isolation", arm="config", defect="no-isolation-arm", retriability=TERMINAL, slots=("kernel", "kind")
)
LANES_OFFLOAD: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LANES, point="offload", arm="boundary", defect="crossing-refused", retriability=TRANSIENT
)
LOGGING_DOOR: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LOGGING, point="door", arm="boundary", defect="door-report-refused", retriability=TERMINAL
)
LOGGING_SHIP: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LOGGING, point="ship", arm="boundary", defect="log-egress-refused", retriability=TERMINAL
)
LOGGING_SHOWN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.LOGGING, point="shown", arm="boundary", defect="repr-refused", retriability=TERMINAL
)
METRICS_INSTRUMENT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.METRICS, point="instrument", arm="boundary", defect="instrumentor-conflict", retriability=TERMINAL
)
BENCH_DOUBLED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="doubled", arm="config", defect="roster-doubles-subject", retriability=TERMINAL, slots=("subjects",)
)
BENCH_EMPTY: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="empty", arm="boundary", defect="no-round-measured", retriability=TERMINAL, slots=("subject",)
)
BENCH_KERNEL: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="kernel", arm="config", defect="no-kernel-covers", retriability=TERMINAL, slots=("subjects",)
)
BENCH_QUIET: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="quiet", arm="config", defect="no-subject-provisioned", retriability=TERMINAL, slots=("subjects",)
)
BENCH_ROUND: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="round", arm="boundary", defect="round-refused", retriability=TERMINAL
)
BENCH_ROUNDS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="rounds", arm="config", defect="window-geometry-invalid", retriability=TERMINAL,
    slots=("subject", "rounds", "warmup")
)
BENCH_TOOL: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="tool", arm="config", defect="no-tool-row-keys", retriability=TERMINAL, slots=("subjects",)
)
BENCH_WARMUP: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="warmup", arm="boundary", defect="warmup-refused", retriability=TERMINAL
)
PROFILES_DRAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="drain", arm="boundary", defect="profiler-shutdown-refused", retriability=TERMINAL
)
PROFILES_JOB: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.PROFILES, point="job", arm="boundary", defect="job-body-refused", retriability=TERMINAL
)
OBSERVE_DISPATCH: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.OBSERVE, point="dispatch", arm="boundary", defect="dispatch-refused", retriability=TERMINAL
)
OBSERVE_SCOPE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.OBSERVE, point="scope", arm="config", defect="scope-breaches-grammar", retriability=TERMINAL, slots=("scope",)
)
RECIPE_ASSET: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECIPE, point="asset", arm="config", defect="destination-escapes-project", retriability=TERMINAL, slots=("relative",)
)
RECIPE_ENGINE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECIPE, point="engine", arm="boundary", defect="engine-check-refused", retriability=TRANSIENT
)
RECIPE_ROOT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECIPE, point="root", arm="resource", defect="asset-fetch-without-root", retriability=TERMINAL
)
RECIPE_RUN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECIPE, point="run", arm="resource", defect="recipe-run-failed", retriability=TERMINAL, slots=("detail",)
)
CORPUS_DOUBLED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.REPRODUCTION, point="census", arm="config", defect="duplicate-producing-tag", retriability=TERMINAL
)
CORPUS_FMT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.REPRODUCTION, point="fmt", arm="config", defect="fmt-breaches-grammar", retriability=TERMINAL,
    slots=("fixture", "fmt", "pattern")
)
ROOTS_DRAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="drain", arm="boundary", defect="store-drain-refused", retriability=TERMINAL
)
ROOTS_FETCH: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="fetch", arm="boundary", defect="object-fetch-refused", retriability=TRANSIENT
)
ROOTS_HTTP: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="http", arm="boundary", defect="http-acquisition-refused", retriability=TRANSIENT
)
ROOTS_SCAN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="scan", arm="resource", defect="filesystem-read-refused", retriability=TRANSIENT
)
ROOTS_SSH: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="ssh", arm="resource", defect="sftp-acquisition-refused", retriability=TRANSIENT
)
ROOTS_STORE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="store", arm="boundary", defect="store-op-refused", retriability=TRANSIENT
)
ROOTS_TRAVERSAL: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="traversal", arm="resource", defect="path-escapes-root", retriability=TERMINAL, slots=("root", "relative")
)
ROOTS_UNSUPPORTED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.ROOTS, point="unsupported", arm="config", defect="backend-refuses-op", retriability=TERMINAL, slots=("op", "reason")
)
SERVE_ANCHOR: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="anchor", arm="config", defect="channel-anchor-arity", retriability=TERMINAL, slots=("count",)
)
SERVE_BUNDLE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="bundle", arm="config", defect="unsupported-credential-kinds", retriability=TERMINAL, slots=("kinds",)
)
SERVE_CATALOG: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="catalog", arm="config", defect="unknown-descriptor", retriability=TERMINAL, slots=("descriptor",)
)
SERVE_DIAL: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="dial", arm="boundary", defect="dial-refused", retriability=TRANSIENT
)
SERVE_DIALS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="dials", arm="boundary", defect="dial-drain-refused", retriability=TERMINAL
)
SERVE_DIRECTION: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="direction", arm="config", defect="credential-direction-mismatch", retriability=TERMINAL,
    slots=("credential", "direction")
)
SERVE_DISCOVERY: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="discovery", arm="config", defect="descriptor-pin-diverged", retriability=TERMINAL,
    slots=("expected", "actual")
)
SERVE_DRAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="drain", arm="boundary", defect="host-drain-refused", retriability=TERMINAL
)
SERVE_ENCODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="encode", arm="config", defect="request-encode-refused", retriability=TERMINAL
)
SERVE_HOST: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="host", arm="boundary", defect="host-serve-refused", retriability=TERMINAL
)
SERVE_INPUTS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="inputs", arm="boundary", defect="assignment-decode-failed", retriability=TERMINAL
)
SERVE_REMOTE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="remote", arm="boundary", defect="peer-conflict", retriability=TERMINAL
)
SERVE_ROSTER: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="roster", arm="config", defect="empty-service-roster", retriability=TERMINAL
)
SERVE_SELECTOR: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="selector", arm="config", defect="selector-outside-roster", retriability=TERMINAL
)
SERVE_SETTINGS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SERVE, point="settings", arm="config", defect="settings-admission-refused", retriability=TERMINAL
)
SHAPES_DOUBLED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SHAPES, point="doubled", arm="config", defect="duplicate-row-name", retriability=TERMINAL
)
SHAPES_DRIFT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SHAPES, point="drift", arm="config", defect="registry-drift", retriability=TERMINAL, slots=("rows",)
)
SHAPES_FORMAT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SHAPES, point="format", arm="boundary", defect="format-key-unrostered", retriability=TERMINAL, slots=("key",)
)
SHAPES_SERVICES: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SHAPES, point="services", arm="boundary", defect="service-name-unresolvable", retriability=TERMINAL
)
SHAPES_WINDOW: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.SHAPES, point="window", arm="boundary", defect="retry-window-unspellable", retriability=TERMINAL, slots=("window",)
)
TELEMETRY_STOP: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.TELEMETRY, point="stop", arm="boundary", defect="provider-shutdown-refused", retriability=TERMINAL
)
WIRE_DECODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WIRE, point="decode", arm="boundary", defect="decode-failed", retriability=TERMINAL
)
WIRE_ENCODE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WIRE, point="encode", arm="boundary", defect="encode-failed", retriability=TERMINAL
)
WIRE_INSERT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WIRE, point="insert", arm="boundary", defect="unknown-predecessor", retriability=TERMINAL, slots=("origin", "logical")
)
WIRE_MAINTAIN: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WIRE, point="maintain", arm="boundary", defect="unobserved-horizon", retriability=TERMINAL, slots=("origin",)
)
WIRE_ORDERED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WIRE, point="ordered", arm="boundary", defect="depth-exhausted", retriability=TERMINAL
)
LEASE_DRIFT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="drift", arm="config", defect="generation-drift", retriability=TERMINAL
)
LEASE_EVIDENCE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="evidence", arm="boundary", defect="settlement-projection-refused", retriability=TERMINAL
)
LEASE_LOST: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="lost", arm="resource", defect="lease-not-held", retriability=TERMINAL, slots=("reason",)
)
LEASE_VERDICT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="verdict", arm="config", defect="unexpected-verdict", retriability=TERMINAL, slots=("verb", "verdict")
)
SUPERVISE_CYCLE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="cycle", arm="boundary", defect="probe-cycle-refused", retriability=TERMINAL
)
WORKERS_COMMAND: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="command", arm="config", defect="daemon-charge-without-command", retriability=TERMINAL, slots=("charge",)
)
WORKERS_COVERED: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="covered", arm="import_", defect="roster-name-unresolved", retriability=TERMINAL
)
WORKERS_CROSSING: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="crossing", arm="boundary", defect="crossing-refused", retriability=TRANSIENT
)
WORKERS_DAEMONS: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="daemons", arm="boundary", defect="supervisor-stop-refused", retriability=TERMINAL
)
WORKERS_ENDPOINT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="endpoint", arm="config", defect="remote-arm-without-endpoint", retriability=TERMINAL, slots=("kernel",)
)
WORKERS_GUEST: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="guest", arm="config", defect="guest-validation-refused", retriability=TERMINAL, slots=("kernel",)
)
WORKERS_PHASE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="phase", arm="config", defect="pool-closed", retriability=TERMINAL, slots=("kind", "kernel", "phase")
)
WORKERS_POOL: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="pool", arm="boundary", defect="pool-drain-refused", retriability=TERMINAL
)
WORKERS_REMOTE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="remote", arm="boundary", defect="remote-crossing-refused", retriability=TRANSIENT
)
WORKERS_SEAL: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="seal", arm="boundary", defect="kernel-seal-refused", retriability=TERMINAL
)
WORKERS_SHM: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.WORKERS, point="shm", arm="config", defect="shared-memory-wire-is-host-local", retriability=TERMINAL, slots=("kernel",)
)

RAISES: Final[Block[FaultRow[RuntimeLeg]]] = rostered(Block.of_seq([
    DEPTH_BOUND, DEPTH_SPENT,
    ADMISSION_HOSTS, BACKEND_CLAIMANT, BACKEND_CONTRACT, BACKEND_MERGE, BACKEND_MINT, PROFILE_GRANT, PROFILE_HOST, SECRET_NAME,
    SECRET_READ, TENANCY_GRADE, TENANCY_ISSUER, TENANCY_SCOPE, TENANCY_TENANT, BUNDLE_ARCHIVE, BUNDLE_COLLECT,
    BINDING_ADMIT, BINDING_CONNECT, BINDING_DECODE, BINDING_DRAIN, BINDING_ENCODE, BINDING_SETTLE,
    BINDING_TRANSACTION, CLOCK_CARRIER, CLOCK_LAYOUT,
    CLOCK_SEALED, EVENT_DECODE,
    EVENT_DOMAIN, EVENT_ENCODE, EVENT_EXTENSION, EVENT_FORMAT,
    EVENT_LAG, EVENT_MINT, EVENT_NAIVE, EVENT_SOURCE, EVENT_TYPE, EVIDENCE_BUDGET, EVIDENCE_GRAMMAR, EVIDENCE_MATCHES,
    EVIDENCE_REFLECT, FILTER_PARSE, FILTER_SETTINGS,
    HOOKS_ISOLATED, HOOKS_PAYLOAD, HOOKS_REGISTER, HOOKS_RELEASE, HOOKS_SUBSCRIBE, HOOKS_TAP, IDENTITY_DERIVE, IDENTITY_FMT, JOURNAL_APPEND, JOURNAL_CENSUS, JOURNAL_CHARGE, JOURNAL_CRYPTO, JOURNAL_CUSTODY, JOURNAL_DERIVED,
    JOURNAL_DRAIN, JOURNAL_HEX, JOURNAL_INSTANT, JOURNAL_KEK, JOURNAL_OFFER, JOURNAL_PERIOD, JOURNAL_PORT, JOURNAL_RATE, JOURNAL_RETIRED,
    JOURNAL_UNBOUND, JOURNAL_UNDRAINED, LANES_EXPORT, LANES_FRONT, LANES_INLINE, LANES_ISOLATION, LANES_OFFLOAD, LOGGING_DOOR, LOGGING_SHIP,
    LOGGING_SHOWN, METRICS_INSTRUMENT, BENCH_DOUBLED, BENCH_EMPTY, BENCH_KERNEL, BENCH_QUIET, BENCH_ROUND, BENCH_ROUNDS, BENCH_TOOL,
    BENCH_WARMUP, PROFILES_DRAIN, PROFILES_JOB, OBSERVE_DISPATCH, OBSERVE_SCOPE, RECIPE_ASSET, RECIPE_ENGINE,
    RECIPE_ROOT, RECIPE_RUN, CORPUS_DOUBLED, CORPUS_FMT, ROOTS_DRAIN, ROOTS_FETCH, ROOTS_HTTP, ROOTS_SCAN, ROOTS_SSH, ROOTS_STORE,
    ROOTS_TRAVERSAL, ROOTS_UNSUPPORTED, SERVE_ANCHOR,
    SERVE_BUNDLE, SERVE_CATALOG, SERVE_DIAL, SERVE_DIALS, SERVE_DIRECTION, SERVE_DISCOVERY, SERVE_DRAIN, SERVE_ENCODE, SERVE_HOST, SERVE_INPUTS,
    SERVE_REMOTE, SERVE_ROSTER, SERVE_SELECTOR, SERVE_SETTINGS, SHAPES_DOUBLED, SHAPES_DRIFT, SHAPES_FORMAT,
    SHAPES_SERVICES, SHAPES_WINDOW, TELEMETRY_STOP, WIRE_DECODE, WIRE_ENCODE,
    WIRE_INSERT, WIRE_MAINTAIN, WIRE_ORDERED, LEASE_DRIFT, LEASE_EVIDENCE, LEASE_LOST, LEASE_VERDICT, SUPERVISE_CYCLE, WORKERS_COMMAND,
    WORKERS_COVERED, WORKERS_CROSSING, WORKERS_DAEMONS, WORKERS_ENDPOINT, WORKERS_GUEST, WORKERS_PHASE, WORKERS_POOL, WORKERS_REMOTE,
    WORKERS_SEAL, WORKERS_SHM,
]))

FAULT_CONF: Final[BeartypeConf] = BeartypeConf(violation_type=BeartypeCallHintViolation)

SCOPES: Final[Map[Scope, str]] = Map.of_seq([
    (Scope.WIRE, "rasm.wire"),
    (Scope.METER, "rasm.runtime"),
    (Scope.LOGGER, "rasm.runtime"),
    (Scope.SERVICE, "rasm.companion"),
    (Scope.RESILIENCE, "rasm.runtime.resilience"),
    (Scope.IDENTITY, "rasm.runtime.identity"),
    (Scope.EVIDENCE, "rasm.runtime.evidence"),
    (Scope.RECIPE, "rasm.runtime.recipe"),
    (Scope.WORKERS, "rasm.runtime.workers"),
    (Scope.PROFILES, "rasm.runtime.profiles"),
    (Scope.JOURNAL, "rasm.runtime.journal"),
])

SCHEMA_URL: Final[str] = Schemas.V1_43_0.value
DISTRIBUTION: Final[str] = "rasm-runtime"
SOURCE_VERSION: Final[str] = "0+source"
SCOPE_VERSION: Final[str] = next((dist.version for dist in distributions() if dist.metadata["Name"] == DISTRIBUTION), SOURCE_VERSION)


# --- [SERVICES] -------------------------------------------------------------------------

_LOG: Final = structlog.get_logger()


# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped[T](acquire: ScopeAcquire[T], scope: str) -> T:
    return acquire(scope, SCOPE_VERSION, None, SCHEMA_URL)


def expired[L: Leg](at: FaultRow[L], budget: Option[float], cause: str, /) -> BoundaryFault:
    return BoundaryFault(deadline=(at.subject, budget.default_value(BUDGET_UNKNOWN), cause))


def faulted(span: trace.Span, event: str, fault: BoundaryFault, /, **fields: object) -> BoundaryFault:
    span.set_status(Status(StatusCode.ERROR, fault.tag))
    _LOG.error(event, **(fields | fault.facts()))
    return fault


def spelled(cause: BaseException) -> frozenset[str]:
    return frozenset(f"{klass.__module__}.{klass.__qualname__}" for klass in type(cause).__mro__)


def _hits(marker: ClassifyMarker, cause: BaseException) -> bool:
    match marker:
        case frozenset() as names:
            return bool(spelled(cause) & names)
        case classes:
            return isinstance(cause, classes)


def _convert[L: Leg](at: FaultRow[L], cause: BaseException) -> BoundaryFault:
    fault = BoundaryFault.of(at, cause)
    span = trace.get_current_span()
    if span.is_recording():
        span.record_exception(cause, attributes={FAULT_TAG: fault.tag, FAULT_SUBJECT: at.subject, FAULT_OWNER: at.leg.value})
        span.set_status(Status(StatusCode.ERROR, fault.tag))
    return fault


def _guard[T, L: Leg](at: FaultRow[L], thunk: Callable[[], T], catch: Catch) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except catch as cause:
        return Error(_convert(at, cause))


@beartype(conf=FAULT_CONF)
def boundary[T, L: Leg](at: FaultRow[L], thunk: Callable[[], T], *, catch: Catch) -> RuntimeRail[T]:
    return _guard(at, thunk, catch)


@beartype(conf=FAULT_CONF)
async def async_boundary[T, L: Leg](at: FaultRow[L], thunk: Callable[[], Awaitable[T]], *, catch: Catch) -> RuntimeRail[T]:
    try:
        return Ok(await thunk())
    except catch as cause:
        return Error(_convert(at, cause))


def trapped[**P, T, L: Leg](at: FaultRow[L], *, catch: Catch) -> Callable[[Callable[P, T] | Callable[P, Awaitable[T]]], Trapped[P, T]]:
    def decorate(fn: Callable[P, T] | Callable[P, Awaitable[T]]) -> Trapped[P, T]:
        if inspect.iscoroutinefunction(fn):

            @wraps(fn)
            async def awaited(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
                return await async_boundary(at, lambda: fn(*args, **kwargs), catch=catch)

            return awaited

        @wraps(fn)
        def called(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
            return _guard(at, lambda: fn(*args, **kwargs), catch)

        return called

    return decorate


@overload
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ...) -> RuntimeRail[Block[T]]: ...
@overload
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Literal[Disposition.PARTITION]) -> RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]: ...
def traversed[T](
    rails: Block[RuntimeRail[T]], *, by: Disposition = Disposition.ABORT
) -> RuntimeRail[Block[T]] | RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]:
    match by:
        case Disposition.ABORT:
            seed: RuntimeRail[Block[T]] = Ok(Block.empty())
            return rails.fold(lambda acc, rail: acc.bind(lambda done: rail.map(lambda value: done.append(Block.singleton(value)))), seed)
        case Disposition.ACCUMULATE | Disposition.PARTITION:
            values, faults = rails.choose(lambda rail: rail.to_option()), rails.choose(lambda rail: rail.swap().to_option())
            if by is Disposition.PARTITION:
                return Ok((values, faults))
            return Ok(values) if faults.try_head().is_none() else Error(faults.reduce(BoundaryFault.combine))
        case _ as unreachable:
            assert_never(unreachable)


def latched[R, **P](
    read: Callable[[], R | None], write: Callable[[R], None], reentrant: Callable[[R], R]
) -> Callable[[Callable[P, R]], Callable[P, R]]:
    def aspect(mint: Callable[P, R]) -> Callable[P, R]:
        @wraps(mint)
        def guarded(*args: P.args, **kwargs: P.kwargs) -> R:
            match read():
                case None:
                    write(minted := mint(*args, **kwargs))
                    return minted
                case prior:
                    return reentrant(prior)

        return guarded

    return aspect


railed = effect.result[Any, BoundaryFault]()
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
