# [PY_RUNTIME_FAULTS]

One fault family and one `Result`/`Option` rail span the whole branch: `BoundaryFault` is the one tagged union every package returns through — its ingress classes, its `domain` case carrying a sibling's own typed refusal token, and the `aggregate` case keep every member structurally addressable — and `RuntimeRail` is the one `Result[T, BoundaryFault]` carrier every fallible function returns. Domain logic returns `Result`/`Option` and never raises; exceptions convert exactly once at the owning boundary, and interior code receives only the rail. Absence rides the `expression` `Option` directly — no fault-bound alias, since `Option` carries no error slot to bind. `Posture` states the counterpart axis a foreign edge needs and `Option` cannot: whether a value was DECLARED by its producer, DEFAULTED from a named source, or is absent outright.

One fault-lift core backs every application shape — the explicit-thunk `boundary`, the awaitable `async_boundary`, and the `@trapped` decorator — so the sync/async split is one coroutine-detection branch, never a parallel rail. Classification is the ordered data-driven `CLASSIFY` table, `RAISES` is its raise-side twin binding every explicit refusal to a `Leg`-derived subject and a declared `Recovery`, and the same conversion is the trace-egress seam: each caught exception is recorded on the active OTel span inside the one fold, the owner never minting, naming, or ending a span — span lifecycle stays with the measured operation. `latched`, the branch's one-shot install latch, the shared `Depth` walk bound, and the WHOLE instrumentation-scope coordinate — `Scope`/`SCOPES` naming the emitting library beside the `scoped` stamp binding that name to its distribution version and semconv url — live here because faults is the one tier below every consumer, the sub-receipts `identity`, `clock`, `shapes`, and `wire` strata included, so no emitting page in the branch is left minting the unversioned scope a backend cannot join against its versioned siblings.

## [01]-[INDEX]

- [02]-[FAULT]: the closed fault family, the classification table beside the folder-wide raise census and its one seat door, the retriability and absence-posture carriers, the shared walk bound, the rail carriers and disposition-parameterized traversal, the three fault-lift shapes, the carried-fault span fold, the install latch, and the versioned instrumentation-scope stamp.

## [02]-[FAULT]

- Owner: `BoundaryFault` and its rail, tables, and cross-cutting tenants per the fence. Row SHAPE and the `Leg` CONTRACT seat here while each folder mints its own leg roster and `RAISES` table, since S0 imports no sibling yet every folder raises through the one door. `rostered` is the seat DOOR that makes those tables reachable: `retriability` and `facts` resolve a row by SUBJECT, so a folder pushes its rows into the one census at its own module scope and a per-module `DETAILS` fold builds a map nothing reads. Traversal splits by shape: `traversed` folds a homogeneous `Block` of already-evaluated rails under one `Disposition`, while `railed` is the bound `effect.result` builder for free-form interleaved binds whose later steps depend on earlier bound values — a variadic short-circuit collector beside them is `traversed(by=ABORT)` re-spelled and never lands. Three cross-cutting carriers seat beside it for the reason `scoped` does, every consumer plane reaching this tier and none reaching a peer's: `Recovery` states retriability, `Posture` states foreign-edge absence, and `Depth` bounds every walk.
- Cases: `config` versus `boundary` splits on who can repair the refusal — `config` carries a caller-repairable construction refusal (a policy value, roster, credential row, or precondition the same inputs deterministically refuse), while `boundary` carries the seam classification of a provider or runtime raise during work (a codec, render, parse, or engine failure a re-issue may clear), so a render-class or draw-class fault rides `boundary=` and a refused composition rides `config=` in every consumer. `wire` is reserved for explicit code-carrying construction where a numeric protocol/status code is the discriminant; a caught codec exception carries no code, so the `CLASSIFY` `msgspec` row lands it in the subject-carrying `boundary` case. `domain` carries a SIBLING's own refusal token whole — every folder fault union is a `@tagged_union` over `BaseException`, so its case and kwargs cross the funnel as typed evidence a consumer matches on. WHOLE means in-process: that same kwarg-only shape reconstructs through NO pickler, since `Exception.__reduce__` hands empty `args` and the union's one-case guard refuses the `__dict__` it is handed, so a `domain` fault returned on a rail across a worker breaks exactly as a raised token does. `execution/workers#CROSSING` owns that seam and it is the ONLY one: the token lowers to data at the worker floor gate and re-mints parent-side ahead of this funnel, so this family carries a live token and never a wire form, and no page below the crossing spells a serialization of its own. A deadline-owning fence constructs `deadline` through the ONE `expired` fold with its real budget and tripped-axis `cause`, so the subject derives from a rostered row and the case stays outside `RaisedTag`, where only the owning fence holds the bound; a fence whose `Option[float]` budget is absent and the `CLASSIFY` `TimeoutError` row alike read `BUDGET_UNKNOWN` — the declared budget-unknown floor a consumer reads as unspecified, never a true zero deadline, and never a `default_value(0.0)` re-spelled per site.
- Entry: the three lift shapes share one `_convert` and take a rostered `FaultRow`, never a free subject string, so a fence cannot spell a leg its package never declares. `catch` is REQUIRED on all three: an engine boundary narrows over its real multi-class raise surface, and the deleted `= Exception` default was the one clause making a bare-`Exception` funnel the cheapest form at every call site. `catch` never widens past `Exception` — converting the `anyio` cancellation exception into a fault is the forbidden widening, cancellation being scope-owned flow control rather than an ingress class. One catch-all survives per producer plane, at the outermost weave fence where an unclassified raise must not cross a process boundary, and every interior seam names the provider set it reaches.
- Auto: `subject`, `detail`, and `owner` are the three accessors every consumer reads a fault's coordinates through — the raise site, the caught evidence, and the emitting leg — so a re-raise under a caller's own row and a span attribute stamp both compose one accessor instead of re-matching the family. `FAULT_TAG`/`FAULT_SUBJECT`/`FAULT_OWNER` are the span and log KEYS those three publish under, rostered beside `SCOPES` for the same reason and spelling the C# kernel's own triple, so one vocabulary joins across all three branch ends; `FAULT_CODE` and `FAULT_POSTURE` complete the roster for the transport edge that decodes a peer detail. `facts` is the one structured egress projection the `observability/receipts#RECEIPT` `rejected` projection spreads whole, so every leaf case carries its own `subject` inline, the roster supplies its `leg`, and the receipts owner re-derives nothing. `retriability` is the ONE re-offer predicate, ordered over two declared sources: a rostered raise answers its own row's posture, and every other fault derives from the ingress class exactly as the standing frozenset read did. `faulted` is the span-side twin and the branch's ONE Error-arm fold for a fault the rail CARRIED into a live span: `_convert` covers the raise this fence caught, `faulted` covers the typed fault that crossed a worker, lane, or sibling boundary and would otherwise leave an `UNSET` span beside an uncorrelated line. Its `**fields` band is the growth axis a stage, subject, or crossing index rides, and the four hand-rolled copies a charter declaring universal behavior had grown collapse onto that one body.
- Packages: `expression`, `beartype`, `msgspec`, `anyio`, `structlog`, `opentelemetry-api`, and `opentelemetry-semantic-conventions` per the fence imports; the OTel dependency is `-api` with the semconv constant surface — the owner reads the active span, `scoped` stamps a caller-supplied factory, and no SDK type or provider instance enters. `msgspec` carries the frozen `Struct` under `FaultRow` and the `Meta` refinement under `Bound`. `structlog` enters as the PACKAGE alone, whose `get_logger` proxy resolves the configured chain at first bind, so this tier composes the log egress without importing the `observability/logging` module the rail seats above it.
- Growth: a new fault class is one `case()` with one `facts`, `subject`, and `detail` arm; a new deadline site is one `expired` call, never a `BoundaryFault(deadline=...)` literal; a new exception family is one ordered `CLASSIFY` row reaching every lift shape and the trace weave; a new raise is ONE `FaultRow` anchor in that folder's own `RAISES` table under a member of its own `<Folder>Leg` roster, and a folder's first raise mints that roster beside the table in the same pass; a new re-offer state is one `Recovery` case with one `widest` arm; a new absence source is a `defaulted` value, never a `Posture` case; a new traversal output shape is one `Disposition` member with one fold arm; a new span-side error dimension is one `**fields` key at a `faulted` call site, never a second fold; a new instrumentation scope is one `Scope` member with one `SCOPES` row; a semconv bump one `Schemas` member swap reaching every meter, tracer, logger, and `Resource` at once; a fourth scope-minting API one `scoped` call, the port already covering any factory sharing the positional `(name, version, provider, schema_url)` shape; a new one-shot install owner is one `@latched(...)` application.
- Boundary: no C# `Expected` clone and no exception taxonomy copied from a C# owner. Retriability keys on the fault's own roster row and `FaultTag`, and never imports `reliability/resilience#RESILIENCE` `RetryClass` — resilience depends on faults, never the reverse; the rail maps exceptions to fault classes, the policy table maps retry classes to exception sets, and the two meet through the rail outcome, the `Recovery` value, and the exported `spelled` matcher alone, which resilience reads and never re-derives. Precedence for retriability is TWO rungs with one declared source each, the row's declared posture over the tag-set derivation; a peer-STATED posture is a third rung above both and has no carrier here, since it decodes at `transport/serve#SERVE` and rides `Option[Recovery]` where a legacy frame states nothing.

```python signature
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

# `RaisedTag` carves the five arms an explicit refusal may take out of `FaultTag`, so the ONE construction door dispatches TOTALLY:
# `deadline` needs a real budget only the deadline-owning fence holds, `wire` a foreign protocol code, `domain` a raised
# sibling token conversion mints, and `aggregate` is `combine`'s own product — none of the four is ever a rostered raise.
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


# `Leg` is the CONTRACT, never a roster: `runtime` is S0 and imports no sibling, yet every folder raises through the
# ONE door, so this page owns the row SHAPE and the leg GRAMMAR while each folder mints the roster its own modules
# name. Member VALUE is the dotted module path beneath `PACKAGE` — `runtime.faults`, `data.tabular.lakehouse` — so one
# grammar spans the corpus and `FaultRow.seated` proves every member against a real module rather than the convention
# 197 free-string subjects carried unenforced. Bare `str` satisfies nothing here, which is what retires the literal.
@runtime_checkable
class Leg(Protocol):
    value: str


# `RuntimeLeg` is the ONE roster this page mints, one member per module in `runtime`'s own codemap; a sibling folder
# mints its own `<Folder>Leg` beside its `RAISES` table and never extends this one.
class RuntimeLeg(StrEnum):
    RECEIPTS = "runtime.receipts"
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


# every folder fault union is one `@tagged_union` over `BaseException` — `geometry/energy/climate#CLIMATE`'s
# `EnergyFault`, `geometry/graph/analytic#ANALYTIC`'s `GraphFault`, `geometry/mesh/repair#REPAIR`'s `RepairFault` — so `tag`
# names the ACTIVE case and the attribute it names carries that case's kwargs. Structural admission is what keeps this
# tier import-free: `runtime` is S0 and reaches no sibling, so a per-folder marker would invert the strata. `Protocol`
# refuses a `BaseException` base, so this types the tag half alone and the raise site proves the exception half.
@runtime_checkable
class Tagged(Protocol):
    tag: str


# Three API scope factories — `metrics.get_meter`, `trace.get_tracer`, `_logs.get_logger` — spell their version
# parameter two ways (`version` against `instrumenting_library_version`) yet share one positional shape, so this
# stamp binds POSITIONALLY through the port and that naming divergence never reaches a call site. Its third slot
# takes the provider override every runtime site declines: `None` resolves whichever global the install published.
class ScopeAcquire[T](Protocol):
    def __call__(self, name: str, version: str, provider: None, schema_url: str, /) -> T: ...


# `Recovery` closes the ONE re-offer vocabulary the branch reads: a bool erases the two states an operator most needs separated, since a
# peer that stated a window is a peer that ANSWERED while a transient fault is a dependency that did not. `throttled`
# carries the stated seconds, so a legitimate `0.0` — retry immediately — stays distinguishable from a terminal refusal
# that a falsy read coalesces with it. Wire absence is NOT a case here: a legacy frame stating nothing rides
# `Option[Recovery]` at the `transport/serve#SERVE` decode, so an unstated posture never masquerades as a stated one.
@tagged_union(frozen=True)
class Recovery:
    tag: Literal["terminal", "transient", "throttled"] = tag()
    terminal: None = case()
    transient: None = case()
    throttled: float = case()  # seconds the producer itself stated — a `Retry-After`, a broker throttle, a rate window

    @property
    def offered(self) -> bool:
        return self.tag != "terminal"

    @property
    def window(self) -> Option[float]:
        return Some(self.throttled) if self.tag == "throttled" else Nothing

    @staticmethod
    def widest(left: "Recovery", right: "Recovery", /) -> "Recovery":
        # aggregate recovery preserves the standing `any` read — one re-offerable member re-offers the whole — and two stated
        # windows keep the SHORTER, since the longer one re-offers a member the shorter already cleared.
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


# `Posture` states the ONE absence axis every foreign edge in the branch composes: a producer that DECLARED a value, a value taken
# from a NAMED fallback, and a fact nothing answered are three states one sentinel — `""`, `0.0`, an identity matrix, a
# `None` past the seam — fuses into one. `Option` states presence and cannot state provenance, so a receipt that omits
# an absent key still cannot tell a read fact from a fabricated one without this. `defaulted` NAMES its source, so the
# fabrication `docs/laws/scars.md` `[LAW_WITHOUT_PRODUCER]` forbids is either impossible or self-reporting.
@tagged_union(frozen=True)
class Posture[T]:
    tag: Literal["declared", "defaulted", "absent"] = tag()
    declared: T = case()
    defaulted: tuple[T, str] = case()  # (value, the named source that supplied it — a foreign document default, a carrier fallback)
    absent: None = case()

    @staticmethod
    def of_optional(value: T | None) -> "Posture[T]":
        # ONE sentinel projection, spelled at the single read site that first sees the foreign value per
        # `docs/stacks/python/boundaries.md` `[SENTINEL_SITE]`; a second admission site projects its own posture.
        return Posture(absent=None) if value is None else Posture(declared=value)

    @staticmethod
    def of_option(option: Option[T]) -> "Posture[T]":
        # sibling lift for a carrier an upstream seam already admitted: `Option` states presence and drops provenance,
        # so declaring it here restores the axis and no composing page re-spells a local `_declared` shim of its own.
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


# `Depth` bounds every closure fold, cell ring, topology walk, lineage hop, and CRDT read through ONE carrier. Walking to
# convergence is a NAMED case, never a max-int spelling a bound the process cannot survive, so a caller that genuinely
# converges says so and a caller that guesses cannot. Exhaustion is a TYPED fault off `DEPTH_SPENT`, never a truncated
# success: `CLAUDE.md` [PARAMETERIZATION] rules a success-shaped fall-through certifies unconverged as converged, and
# `docs/laws/scars.md` `[FORGED_ZERO]` rules the same for a bound that silently stops short.
@tagged_union(frozen=True)
class Depth:
    tag: Literal["bounded", "fixpoint"] = tag()
    bounded: Bound = case()
    fixpoint: None = case()  # walk to convergence — the honest case a max-int bound forges

    @staticmethod
    def of(bound: int) -> RuntimeRail["Depth"]:
        # `Depth.of` is the validated ingress: `Meta(ge=DEPTH_FLOOR)` refines a `Depth` DECODED from config or wire, and this refines
        # one built in process, both reading the one declared edge per `docs/stacks/python/shapes.md` `[SHARED_REFINEMENT]`.
        return Ok(Depth(bounded=bound)) if bound >= DEPTH_FLOOR else Error(DEPTH_BOUND.raised(str(bound)))

    def stepped(self) -> Option["Depth"]:
        # one descent: `Nothing` IS exhaustion, so a walk folds the bound through its own recursion and never compares
        # a counter it also increments. The fixpoint case answers itself forever and terminates on convergence alone.
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
        # ONE exhaustion spelling for every walk in the branch: the refusal seats at the bound's own owner and the
        # walking fence rides as a NAMED coordinate, so two walks can never report one exhaustion two ways.
        return DEPTH_SPENT.raised(walk.subject, self.spelled)

    @property
    def spelled(self) -> str:
        return "fixpoint" if self.tag == "fixpoint" else str(self.bounded)


# --- [CONSTANTS] --------------------------------------------------------------------------

# one declared edge for the walk bound, projected into the two validators that read it; `shapes.md` `[SHARED_REFINEMENT]`
# bars one validator from reading a foreign marker, so the edge is the constant and each stage spells its own slice.
DEPTH_FLOOR: Final[int] = 1

# the ONE budget floor a deadline carries when the fence holding it has no measured bound: the `CLASSIFY`
# `TimeoutError` row has none in hand, and a cancelled offload's `Option[float]` budget may hold none either. Spelled
# HERE and read by both, so "unspecified" and a true zero-second deadline can never be told apart by accident at one
# site and fused at another — every `default_value(0.0)` a lane, pool, or crossing fence used to spell reads this.
BUDGET_UNKNOWN: Final[float] = 0.0

# `PACKAGE` names the import ROOT every leg value hangs beneath, which `DISTRIBUTION` cannot answer: installed metadata
# keys on a distribution name while `find_spec` resolves an import path, and the two never coincide in this estate.
PACKAGE: Final[str] = "rasm"

# Two stateless postures bind every roster row and every derived default; `throttled` carries a measured window,
# so it mints per raise and takes no anchor.
TERMINAL: Final[Recovery] = Recovery(terminal=None)
TRANSIENT: Final[Recovery] = Recovery(transient=None)

# fault ATTRIBUTE roster — the one canonical span and log key per fault coordinate, seated at this tier for the
# reason `SCOPES` is: every plane stamps them, this owner stamps them itself, and it reaches no sibling. The three
# cross-branch keys are the C# kernel's own spelling and the TypeScript convention roster mirrors it, so a backend
# joins one vocabulary across all three ends. `owner` carries the `Leg` VALUE, which is what retires the separate
# `rasm.fault.leg` key beside it; `code` is the transported numeric identity and rides the TRANSPORT edge alone,
# where a decoded peer detail is in hand — an interior span holds no code and stamps none. These are span and log
# keys, never metric dimensions: a fault subject is unbounded cardinality on a stream a view identifies by its keys.
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
    deadline: tuple[str, float, str] = case()  # (subject, budget, cause) — cause keeps the per-signal/per-axis identity classification would erase
    api: tuple[str, str] = case()
    import_: tuple[str, str] = case()
    wire: tuple[str, int] = case()
    boundary: tuple[str, str] = case()
    domain: tuple[str, Tagged] = case()  # (subject, the sibling's own raised refusal token, carried whole)
    aggregate: tuple["BoundaryFault", ...] = case()

    @staticmethod
    def of[L: Leg](at: "FaultRow[L]", cause: BaseException) -> "BoundaryFault":
        # a folder fault union is the estate's OWN typed token and never a foreign ingress class, so it admits AHEAD of
        # every `CLASSIFY` row and crosses the funnel WHOLE. The catch-all is worse than message-collapse for these:
        # `Exception.__str__` renders EMPTY for a kwarg-only `@tagged_union`, so `str(cause) or type(cause).__name__`
        # coalesces all nine `EnergyFault` cases onto the bare string `EnergyFault` and no consumer can tell them apart.
        # catch-all keeps `str(cause)` (the message is its discriminant); CLASSIFY rows keep the type name — their tag IS the type.
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
        # `subject` answers the raise coordinate every leaf case carries in slot one; `aggregate` answers its own name, having no site.
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
        # `subject`'s twin: the second slot every leaf case carries, so a consumer re-raising under its own row reads
        # the caught evidence off ONE accessor instead of matching the family again at each site. The three cases
        # whose second slot is not a string answer their own render, and `aggregate` answers its members' tags.
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
        # the EMITTING leg, resolved off the roster rather than parsed back out of the subject — a fault whose
        # subject no row seats has no owner to claim, and `Nothing` is what a span attribute and a log key both OMIT
        # rather than filling with an empty string that names a series nobody fills.
        return _detail(self.subject).map(lambda row: row.leg.value)

    @property
    def ordinal(self) -> Option[int]:
        # the wire ordinal `FaultDetail.case` carries: the seated row's 1-based position under its leg in DECLARATION
        # order, so the producing family's closed ordinal crosses and a peer never reads a transport code back into a
        # taxonomy; an unseated subject answers `Nothing`, which the serve egress writes as the unspecified zero.
        return _SEATED.try_find(self.subject).map(lambda cell: cell.ordinal)

    @property
    def defect(self) -> Option[str]:
        # the seated row's closed defect token — the `reason` a field violation carries across the wire.
        return _detail(self.subject).map(lambda row: row.defect)

    @property
    def coordinates(self) -> Block[tuple[str, str]]:
        # the raise's NAMED slots back out of the detail the row's door folded, recovered against that row's declared
        # roster rather than split on a separator a value may carry; an unseated or slot-less fault answers empty. These
        # are the coordinates a peer repairs on, and the one shape a field violation crosses as.
        return _detail(self.subject).map(lambda row: _coordinates(row.slots, self.detail)).default_value(Block.empty())

    def retriability(self, codes: frozenset[FaultTag]) -> Recovery:
        # ONE predicate, two rungs, one DECLARED source each. An aggregate folds its members through `Recovery.widest`.
        # Rostered raises answer their OWN row's posture, because the defect — never the fence that caught it — knows
        # what a re-offer can clear. Everything else derives from the ingress class exactly as the standing frozenset
        # read did, so a fault with no row keeps today's answer. A peer-STATED posture outranks both and decodes at
        # `transport/serve#SERVE`; it has no carrier on this family, since every leaf case is a wire-shaped tuple the
        # branch's raise sites bind positionally and widening one would fork the family the `aggregate` fold spans.
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return Block.of_seq(members).fold(lambda folded, member: Recovery.widest(folded, member.retriability(codes)), TERMINAL)
            case _:
                declared = _detail(self.subject).map(lambda row: row.retriability)  # the ONE census, whichever module seated the row
                return declared.default_value(TRANSIENT if self.tag in codes else TERMINAL)

    def recoverable(self, codes: frozenset[FaultTag]) -> bool:
        # ONE collapse of the three-state read, spelled at a fence that must answer yes-or-no and nowhere else;
        # a caller that needs the stated window reads `retriability(...).window` instead of re-deriving it from a bool.
        return self.retriability(codes).offered

    def facts(self) -> dict[str, object]:
        # `budget`/`code` ride as native scalars the receipts `EventDict` renderer serializes; a pre-`str()` coerce erases comparability.
        # `leg` OMITS where the subject resolves no roster row, per `runtime/RULINGS.md` — an empty-string value names a series nobody fills.
        seat: dict[str, object] = self.owner.map(lambda leg: {"leg": leg}).default_value({})
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return {"tag": "aggregate", "subject": self.subject, "members": ",".join(m.tag for m in members)}
            case BoundaryFault(tag="deadline", deadline=(_, budget, cause)):
                return {"tag": "deadline", "subject": self.subject, "budget": budget, "cause": cause} | seat
            case BoundaryFault(tag="wire", wire=(_, code)):
                return {"tag": "wire", "subject": self.subject, "code": code} | seat
            case BoundaryFault(tag="domain", domain=(_, token)):
                # sibling union cases ARE the discriminant a consumer gates on, and the attribute that case
                # names IS its kwargs; reading both off `tag` is the union grammar's own projection, which is what
                # keeps the generic lift free of the sibling import S0 refuses.
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


# `FaultRow` is the raise-side twin of `CLASSIFY`: that table owns exception-to-case, this row owns defect-to-case. `subject` DERIVES
# from `leg`, so a raise cannot spell a leg its package never declares and the dotted grammar is unrepresentably wrong
# rather than merely conventional — the same derivation `InstrumentSpec.domain` proves for metric names. Row `defect`
# tokens close the detail vocabulary by construction, since one exists only as a row's field. `slots` NAMES this row's runtime
# coordinates, so near-identical defects collapse into ONE parameterized row and arity proves against the roster.
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
        # ONE construction door: the case this row's own `arm` names, the subject derived from its leg, the detail
        # folded from its closed defect token plus this raise's NAMED coordinates. `zip(strict=True)` refuses an arity
        # no roster row declared — a DEFECT at the raise site, since a coordinate no row names is evidence nobody reads.
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
        # `RAISES` proves at IMPORT over the WHOLE table, in the `observability/metrics#METRIC` `MEASURES` idiom. The
        # seat is typed over the `Leg` PROTOCOL rather than one folder's roster, because the census spans every folder
        # that pushes rows through `rostered` and a per-roster map is the dead mirror that fold exists to retire.
        # `find_spec` resolves each leg value under `PACKAGE`, so a member naming no module refuses HERE rather than at a
        # raise that may never run; `sys.modules` answers first, since a module building its own table is mid-import and
        # a spec probe against itself would re-enter its own package. `Map.add` SHADOWS a duplicate key silently, so two
        # rows spelling one coordinate would hand one defect the other's declared posture and the other's `facts` leg.
        # Both raises are estate invariants and kill the import, unlike the foreign-metadata miss `SOURCE_VERSION` answers.
        module = f"{PACKAGE}.{row.leg.value}"
        if module not in sys.modules and find_spec(module) is None:
            raise ModuleNotFoundError(module)
        if row.subject in seat:
            raise KeyError(row.subject)
        # the ordinal is the row's 1-based position under its OWN leg, counted in the declaration order the fold walks,
        # so an appended row shifts no earlier ordinal and the wire `case` a peer already holds stays valid.
        ordinal = 1 + sum(1 for cell in seat.values() if cell.row.leg.value == row.leg.value)
        return seat.add(row.subject, Seat(row=row, ordinal=ordinal))


class Seat(msgspec.Struct, frozen=True, gc=False):
    # ONE census cell: the row beside the ordinal its leg assigned it, so `retriability`, `facts`, `owner`, and the
    # wire `ordinal` all read one seat and no second map keys the same subject.
    row: FaultRow[Leg]
    ordinal: int


# --- [TABLES] ---------------------------------------------------------------------------

# THE census, and the one door onto it. `retriability` and `facts` resolve a fault's declared posture and emitting
# leg by SUBJECT, so a row seated anywhere but here is unreachable from both: its `Recovery` never outranks the
# tag-set derivation and its `leg` never reaches a log line. A per-module `DETAILS = RAISES.fold(...)` mirror is
# exactly that dead seat — it builds a map nothing reads — which is why the fold is a DOOR rather than a constant.
# `runtime` is S0 and imports no sibling, so this tier cannot pull a folder's table in; the folder PUSHES its rows
# through `rostered` at its own module scope, and a module is imported before any raise inside it can run, so the
# seat is landed by the time a subject resolves. Registration is idempotent per module by construction: the fold is
# spelled once, at the module's own `RAISES` binding, and `seated` kills the import on a duplicate coordinate.
# The census is the estate's one mutable registry and mutates at IMPORT alone, under a gate because two module
# imports race on a free-threading interpreter; the read stays lock-free, an immutable `Map` swap being atomic.
_SEATED: Map[str, Seat] = Map.empty()
_SEAT_GATE: Final[Lock] = Lock()


def rostered[L: Leg](rows: Block[FaultRow[L]], /) -> Block[FaultRow[L]]:
    # the ONE seat door every module's `RAISES` binds through; it answers the rows so the binding stays one
    # expression and the module keeps its own addressable table beside the shared census.
    global _SEATED  # ruff:ignore[global-statement] — the module-scope census this door owns; import-time mutation is its contract
    with _SEAT_GATE:
        _SEATED = rows.fold(FaultRow.seated, _SEATED)
    return rows


def _coordinates(slots: tuple[str, ...], detail: str) -> Block[tuple[str, str]]:
    # inverse of the `raised` fold: each declared slot heads its own `:<slot>=` cell, so the value of slot `i` runs from
    # the end of its head to the start of slot `i + 1`'s head (or the detail's end), whatever characters it carries.
    heads = tuple(detail.index(f":{slot}=") for slot in slots)
    return Block.of_seq(
        (slot, detail[head + len(slot) + 2 : heads[index + 1] if index + 1 < len(heads) else len(detail)])
        for index, (slot, head) in enumerate(zip(slots, heads, strict=True))
    )


def _detail(subject: str) -> Option[FaultRow[Leg]]:
    # the ONE census read: `subject` is the `leg.point` coordinate `FaultRow.subject` derives, so a fault carrying a
    # subject no module seated answers `Nothing` and every consumer omits rather than filling a key nobody fills.
    return _SEATED.try_find(subject).map(lambda cell: cell.row)


# row order is load-bearing: `TimeoutError` subclasses `OSError`, so the `deadline` row must precede the `resource` row
# or the first-match fold coalesces a timeout into `resource`, and the asyncssh terminal rows precede the channel row
# because `HostKeyNotVerifiable`/`PermissionDenied` SUBCLASS `DisconnectError`. A frozenset row matches by MODULE-QUALIFIED
# qualname over the MRO — a gated executor's death (loky/pebble pool markers) and a gated SSH channel's death classify with
# zero provider imports at this BASE tier, and the defining-module anchor keeps an unrelated class re-using a provider's
# bare name from classifying; a builtin would spell `builtins.<Name>`, but every builtin here rides its own class row.
# Every row here is a FOREIGN ingress class; the estate's own raised tokens admit ahead of the table inside `of`.
CLASSIFY: Final[Block[ClassifyRow]] = Block.of_seq([
    (TimeoutError, lambda subject, cause: BoundaryFault(deadline=(subject, BUDGET_UNKNOWN, str(cause) or type(cause).__name__))),
    ((msgspec.ValidationError, msgspec.DecodeError), lambda subject, cause: BoundaryFault(boundary=(subject, type(cause).__name__))),
    (BeartypeCallHintViolation, lambda subject, cause: BoundaryFault(api=(subject, type(cause).__name__))),
    (ImportError, lambda subject, cause: BoundaryFault(import_=(subject, type(cause).__name__))),
    (
        # caller-repairable refusals land `config`: a kernel nesting pools past LOKY_MAX_DEPTH, and the asyncssh trust and
        # credential rows a re-issue never clears — the fleet counterpart of a bad policy value.
        frozenset({"loky.process_executor.LokyRecursionError", "asyncssh.misc.HostKeyNotVerifiable", "asyncssh.misc.PermissionDenied"}),
        lambda subject, cause: BoundaryFault(config=(subject, type(cause).__name__)),
    ),
    (
        # pool deaths: loky mints its own subclass tree while pebble surfaces the STDLIB BrokenProcessPool, so both
        # spellings ride the row and the interpreter-pool death matches its concurrent.futures home.
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
        # asyncssh channel deaths — the remote arm's worker-death names, classified at the same bar as the local pools.
        frozenset({"asyncssh.misc.DisconnectError", "asyncssh.misc.ChannelOpenError"}),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
    (
        # every anyio resource death spells itself: `ConnectionFailed` alone subclasses `OSError`, so the trailing
        # builtin covers none of its siblings and an unlisted spelling falls to the catch-all, where `str(cause)` erases
        # the type the whole row exists to keep. `BusyResourceError` is the CUSTODY arm — `transport/roots#RESOURCE`'s
        # `ResourceGuard` rails an overlapping second puller through it — so dropping it classifies a real
        # single-consumer breach as an unclassified boundary fault that no `recoverable` membership reaches.
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

# `RAISES` is the raise-side roster: every explicit refusal in the branch resolves ONE anchor here and calls its door, so the 223
# literal `BoundaryFault(<case>=(…))` constructions and the 197 distinct free-string fence subjects collapse onto rows
# a reader can enumerate. Anchors are the addressable form and `RAISES` derives the census from them, so a row has one
# authority and consumers import a SYMBOL rather than re-spelling a subject string. Both seats below belong to this
# page's own walk bound: a bound below the declared floor is caller-repairable, and an exhausted walk is not.
DEPTH_BOUND: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.FAULTS, point="bound", arm="config", defect="depth-below-floor", retriability=TERMINAL, slots=("bound",)
)
DEPTH_SPENT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.FAULTS, point="depth", arm="boundary", defect="depth-exhausted", retriability=TERMINAL, slots=("walk", "bound")
)

# --- [RUNTIME_RAISES]

# every explicit refusal the `runtime` folder makes, one anchor per refusal LAW, ordered by leg then point so the
# census reads as the folder's own codemap. A consumer imports the SYMBOL — never a re-spelled subject string — so a
# rename moves one declaration and every raise site follows it. Near-identical defects collapse onto ONE
# parameterized row taking its coordinates as `slots`: three confinement gates share `ROOTS_TRAVERSAL`, two unbound
# ledger reads share `JOURNAL_UNBOUND` under a `verb` slot, and the lost/empty lease verdicts share `LEASE_LOST`.
# Rows the fence LIFTS through carry no slots — `boundary` builds the detail from the caught class — while their
# `arm` and `defect` still declare the case that refusal belongs to and their `retriability` is the posture a
# re-drive reads, which is the whole reason a lift site rides a row rather than a bare subject.
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
BUNDLE_EMIT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.BUNDLE, point="emitted", arm="boundary", defect="capsule-sink-refused", retriability=TERMINAL
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
RECEIPTS_DISPATCH: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECEIPTS, point="dispatch", arm="boundary", defect="dispatch-refused", retriability=TERMINAL
)
RECEIPTS_EMIT: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECEIPTS, point="emit", arm="boundary", defect="harvest-sink-refused", retriability=TERMINAL
)
RECEIPTS_SCOPE: Final[FaultRow[RuntimeLeg]] = FaultRow(
    leg=RuntimeLeg.RECEIPTS, point="scope", arm="config", defect="scope-breaches-grammar", retriability=TERMINAL, slots=("scope",)
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
    BUNDLE_EMIT, BINDING_ADMIT, BINDING_CONNECT, BINDING_DECODE, BINDING_DRAIN, BINDING_ENCODE, BINDING_SETTLE,
    BINDING_TRANSACTION, CLOCK_CARRIER, CLOCK_LAYOUT,
    CLOCK_SEALED, EVENT_DECODE,
    EVENT_DOMAIN, EVENT_ENCODE, EVENT_EXTENSION, EVENT_FORMAT,
    EVENT_LAG, EVENT_MINT, EVENT_NAIVE, EVENT_SOURCE, EVENT_TYPE, EVIDENCE_BUDGET, EVIDENCE_GRAMMAR, EVIDENCE_MATCHES,
    EVIDENCE_REFLECT, FILTER_PARSE, FILTER_SETTINGS,
    HOOKS_ISOLATED, HOOKS_PAYLOAD, HOOKS_REGISTER, HOOKS_RELEASE, HOOKS_SUBSCRIBE, HOOKS_TAP, IDENTITY_DERIVE, IDENTITY_FMT, JOURNAL_APPEND, JOURNAL_CENSUS, JOURNAL_CHARGE, JOURNAL_CRYPTO, JOURNAL_CUSTODY, JOURNAL_DERIVED,
    JOURNAL_DRAIN, JOURNAL_HEX, JOURNAL_INSTANT, JOURNAL_KEK, JOURNAL_OFFER, JOURNAL_PERIOD, JOURNAL_PORT, JOURNAL_RATE, JOURNAL_RETIRED,
    JOURNAL_UNBOUND, JOURNAL_UNDRAINED, LANES_EXPORT, LANES_FRONT, LANES_INLINE, LANES_ISOLATION, LANES_OFFLOAD, LOGGING_DOOR, LOGGING_SHIP,
    LOGGING_SHOWN, METRICS_INSTRUMENT, BENCH_DOUBLED, BENCH_EMPTY, BENCH_KERNEL, BENCH_QUIET, BENCH_ROUND, BENCH_ROUNDS, BENCH_TOOL,
    BENCH_WARMUP, PROFILES_DRAIN, PROFILES_JOB, RECEIPTS_DISPATCH, RECEIPTS_EMIT, RECEIPTS_SCOPE, RECIPE_ASSET, RECIPE_ENGINE,
    RECIPE_ROOT, RECIPE_RUN, CORPUS_DOUBLED, CORPUS_FMT, ROOTS_DRAIN, ROOTS_FETCH, ROOTS_HTTP, ROOTS_SCAN, ROOTS_SSH, ROOTS_STORE,
    ROOTS_TRAVERSAL, ROOTS_UNSUPPORTED, SERVE_ANCHOR,
    SERVE_BUNDLE, SERVE_CATALOG, SERVE_DIAL, SERVE_DIALS, SERVE_DIRECTION, SERVE_DISCOVERY, SERVE_DRAIN, SERVE_ENCODE, SERVE_HOST, SERVE_INPUTS,
    SERVE_REMOTE, SERVE_ROSTER, SERVE_SELECTOR, SERVE_SETTINGS, SHAPES_DOUBLED, SHAPES_DRIFT, SHAPES_FORMAT,
    SHAPES_SERVICES, SHAPES_WINDOW, TELEMETRY_STOP, WIRE_DECODE, WIRE_ENCODE,
    WIRE_INSERT, WIRE_MAINTAIN, WIRE_ORDERED, LEASE_DRIFT, LEASE_EVIDENCE, LEASE_LOST, LEASE_VERDICT, SUPERVISE_CYCLE, WORKERS_COMMAND,
    WORKERS_COVERED, WORKERS_CROSSING, WORKERS_DAEMONS, WORKERS_ENDPOINT, WORKERS_GUEST, WORKERS_PHASE, WORKERS_POOL, WORKERS_REMOTE,
    WORKERS_SEAL, WORKERS_SHM,
]))

# one shared domain BeartypeConf: binding it makes every contract violation raise the canonical
# BeartypeCallHintViolation the CLASSIFY `api` row folds onto the rail, so no adapter re-catches inline.
FAULT_CONF: Final[BeartypeConf] = BeartypeConf(violation_type=BeartypeCallHintViolation)

# consumers mint handles through the stamp — scoped(trace.get_tracer, SCOPES[Scope.WIRE]) — never a per-page literal and
# never a bare factory call. A scope names the instrumenting library, never the signal, so the meter and logger slots
# resolve one name for one library's two signals while four independently-emitting planes each keep their own row: a
# backend joining on scope separates the worker crossing, the profiler push, and the durable journal from the served
# host, and `SERVICE` narrows to that host and its CLI app name alone.
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

# Distribution version and estate-wide semconv pin complete the coordinate `SCOPES` starts, every runtime `Resource`
# carrying that same url. Both home beside the name because a scope is one triple, and both home at THIS tier because
# `identity`, `clock`, `shapes`, and `wire` emit spans from below every observability owner — a coordinate seated any
# higher leaves those pages no import that reaches it. `SCHEMA_URL` reads the semconv distribution's own `Schemas`
# roster rather than re-spelling the url, so a semconv bump moves the pin by moving the member and can never desync
# it silently. `DISTRIBUTION` names the ONE python distribution this estate declares — `pyproject.toml` `[project]
# name`, the per-package manifest being foreclosed by `libs/.planning/README.md` — because installed metadata keys on
# a DISTRIBUTION name, never a package path. The roster read is TOTAL where `version(DISTRIBUTION)` is not: that call
# RAISES `PackageNotFoundError` whenever no metadata answers, and the workspace root declares `[tool.uv] package =
# false`, so every source-tree run and every dev venv resolves nothing for a correctly-spelled name and a module-scope
# raise there kills every `scoped` mint in the process — the whole signal plane and the `Resource` service version
# with it. `SOURCE_VERSION` answers that miss with the local-version segment PEP 440 reserves for a build carrying no
# release identity, so an uninstalled run still mints a versioned, schema-pinned coordinate a backend joins against
# its installed siblings and the segment itself reports which was read; a plausible release number spelled as the miss
# is the deleted form, because it forges provenance the process never had.
SCHEMA_URL: Final[str] = Schemas.V1_43_0.value
DISTRIBUTION: Final[str] = "workspace-foundation-python"
SOURCE_VERSION: Final[str] = "0+source"
SCOPE_VERSION: Final[str] = next((dist.version for dist in distributions() if dist.metadata["Name"] == DISTRIBUTION), SOURCE_VERSION)


# --- [SERVICES] --------------------------------------------------------------------------

# module-scope bound logger — a dependency-backed runtime handle, never an immutable anchor: `structlog.get_logger()`
# is a lazy proxy resolving the configured chain at first bind, so this tier carries the PACKAGE and never the
# `observability/logging` module the import rail seats above it.
_LOG: Final = structlog.get_logger()


# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped[T](acquire: ScopeAcquire[T], scope: str) -> T:
    # one scope stamp every signal mints through: the `SCOPES` row names the emitting library, this pair versions it
    # and pins the semconv coordinate, so a meter, a tracer, and a logger opened for one scope carry an identical
    # instrumentation-scope triple and a semconv bump is one constant. A bare `get_*(scope)` mints an unversioned,
    # schema-free scope a backend cannot join against its versioned siblings, so no call site spells one.
    return acquire(scope, SCOPE_VERSION, None, SCHEMA_URL)


def expired[L: Leg](at: FaultRow[L], budget: Option[float], cause: str, /) -> BoundaryFault:
    # the ONE deadline construction for a fence that HOLDS a budget: the subject derives from the row's leg, the
    # tripped axis and its coordinates ride `cause`, and an unheld bound spells the DECLARED `BUDGET_UNKNOWN` floor
    # here rather than at each of the lane, pool, and crossing sites that each wrote `default_value(0.0)` of their
    # own. The `deadline` case stays outside `RaisedTag` for exactly this reason — only the owning fence knows the
    # real budget — so this fold is how that case reaches a rostered subject without widening the construction door.
    return BoundaryFault(deadline=(at.subject, budget.default_value(BUDGET_UNKNOWN), cause))


def faulted(span: trace.Span, event: str, fault: BoundaryFault, /, **fields: object) -> BoundaryFault:
    # the ONE Error-arm fold for a fault the rail CARRIED into a live span rather than raised inside it: `_convert`
    # already owns the raise-at-this-fence case, so this is its twin for a typed fault that crossed a worker, a lane,
    # or a sibling boundary and would otherwise leave the span `UNSET` and its error line uncorrelated. Status first,
    # then the structured line off `facts()` — the same projection the receipts `rejected` arm spreads, so the log and
    # the receipt cannot disagree — then the fault back, so a producer's arm reads `.map_error(lambda f: faulted(span,
    # "<event>", f, step=...))` and never a three-statement block whose middle line drifts per page. `**fields` is the
    # growth axis and merges UNDER the fault's own facts, so a caller key can never shadow the tag, subject, or detail
    # a reader gates on. `record_exception` is deliberately absent: the raise converted at its own boundary and is
    # unpicklable across a worker crossing, so the flat projection IS the evidence. Issue scope and tenant arrive
    # ambient through the chain's contextvars and baggage heads — a caller binding either re-owns a logging seam.
    span.set_status(Status(StatusCode.ERROR, fault.tag))
    _LOG.error(event, **(fields | fault.facts()))
    return fault


def spelled(cause: BaseException) -> frozenset[str]:
    # the ONE module-qualified MRO spelling set the branch matches import-free names against — isinstance semantics
    # for a provider class this tier never imports, the defining-module anchor rejecting an unrelated class that
    # re-uses a provider's bare name. Both matchers intersect against it: the `CLASSIFY` frozenset row here and
    # `reliability/resilience#RESILIENCE` `Backoff.retriability`'s target/refuse rosters, so the convention has one derivation
    # and a spelling change lands once instead of drifting between the classifier and the retry predicate.
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
        # `escaped` stays the default `False`: converted to `Error(fault)` at this fence, the exception never escapes the span scope.
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
    # `catch` carries NO default: the deleted `= Exception` made a bare-`Exception` funnel the cheapest form at every
    # call site, and `docs/stacks/python/rails-and-effects.md` `[EXCEPTION_CAPTURE]` bans that form by name. A seam
    # names the provider classes it reaches, so an unexpected raise propagates as the defect it is.
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
    # overloads carry the per-disposition output shape so a caller narrows on the `Disposition` literal it passes,
    # never on the runtime union; only `PARTITION` widens the Ok arm to the `(values, faults)` tuple.
    match by:
        case Disposition.ABORT:
            seed: RuntimeRail[Block[T]] = Ok(Block.empty())
            return rails.fold(lambda acc, rail: acc.bind(lambda done: rail.map(lambda value: done.append(Block.singleton(value)))), seed)
        case Disposition.ACCUMULATE | Disposition.PARTITION:
            values, faults = rails.choose(lambda rail: rail.to_option()), rails.choose(lambda rail: rail.swap().to_option())
            if by is Disposition.PARTITION:
                # total by construction — the split cannot fail; the uniform RuntimeRail return keeps one overloaded surface.
                return Ok((values, faults))
            return Ok(values) if faults.try_head().is_none() else Error(faults.reduce(BoundaryFault.combine))
        case _ as unreachable:
            assert_never(unreachable)


def latched[R, **P](
    read: Callable[[], R | None], write: Callable[[R], None], reentrant: Callable[[R], R]
) -> Callable[[Callable[P, R]], Callable[P, R]]:
    # mint once, restamp the prior receipt on re-entry; consumers inject a `msgspec.structs.replace`-built `reentrant` closure.
    def aspect(mint: Callable[P, R]) -> Callable[P, R]:
        @wraps(mint)
        def guarded(*args: P.args, **kwargs: P.kwargs) -> R:
            match read():
                case None:
                    write(receipt := mint(*args, **kwargs))
                    return receipt
                case prior:
                    return reentrant(prior)

        return guarded

    return aspect


# one _TSource types both the per-yield bind and the return_ payload, so the leaf element erases through Any.
railed = effect.result[Any, BoundaryFault]()
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
