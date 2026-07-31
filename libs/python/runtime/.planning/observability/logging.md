# [PY_RUNTIME_LOGGING]

`LogPipeline` owns the structlog processor chain and the one log egress every event crosses — native, foreign, and the interpreter's own terminal doors alike. One `shared_chain` table feeds both render paths, so a new concern is one row reaching native chain and foreign stdlib bridge together, and one bound row caps every value's depth, width, and length before either render reads it. Its armed egress pair resolves the raised exception, then tees onto a log record carrying event name as body, remaining fields as attributes, and the SDK-derived exception triple — handing the dict on so the console renders what the wire took.

`Signals.emit`/`emit_async`, the `Receipt` fold, the `Redaction` model, and `LEVEL_METHOD` arrive settled from `observability/receipts#RECEIPT`; the `scoped` instrumentation stamp and the `Scope`/`SCOPES` row it binds arrive from `reliability/faults#FAULT`, the one tier below every emitting owner. `LogShip` is the policy value `observability/telemetry#TELEMETRY` threads into its install, so one value drives both halves of the egress — provider registration there, chain arm here. Chain-resident `redact` applies whatever `Redaction` the emit bound under `REDACTION_KEY`, and a foreign record with no bound policy folds through the keep-all `OPEN`.

## [01]-[INDEX]

- [02]-[PIPELINE]: one shared processor chain, `LogShip` policy over its `SHIP_OTLP` row, trace-context, payload-bound, and redaction processors, fault-resolution and wire-projection paired under `LogLimits`, scope-keyed configure custody, one console handler hosting the foreign leg, and `DOORS` arming the interpreter's terminal hooks.

## [02]-[PIPELINE]

- Owner: `LogPipeline.configure` wires the whole pipeline once — the shared chain, the folded filtering floor, the console handler's `ProcessorFormatter`, the stdlib root floor, and the process doors — and re-configure re-formats the held handler in place, so an embedding host's handlers survive untouched. `shared_chain` builds the one processor table each configure, so its limits-parameterized rows carry the folded caps as policy rather than a literal: the native structlog path appends `ProcessorFormatter.wrap_for_formatter`, the foreign pre-chain reads the identical rows behind one reseated `ExtraAdder`, so a bridged grpcio/apscheduler/executor record's `extra` fields join the event before the correlation, callsite, traceback, bound, redaction, and wire rows every native event also crosses, and the three keys the formatter seeded ahead of that merge survive it. One writer owns stdout: the native path routes through `stdlib.LoggerFactory` into the same handler the foreign path lands on, so no binary-layer render races the text layer for the stream.
- Cases: `LogShip` is the egress vocabulary and `SHIP_OTLP` its one dispatch row — `CONSOLE` renders the JSON line and seats no wire row at all, `OTLP_CONSOLE` renders that same line and seats the `faulted`/`shipped` pair projecting every event onto the registered `LoggerProvider`, standing as the exporting process-root default, the deployed collector admitting an OTLP receiver alone. Console-silent rows stay refused: the console handler's `ProcessorFormatter` is the one seam a foreign stdlib record crosses the shared chain through, so silencing it silences the foreign leg's wire projection with it. Arming decides table MEMBERSHIP rather than a per-event branch, so an unarmed process pays nothing per line, and an armed one reaching the chain before the telemetry install resolves the API no-op logger. That projection is the one row leaving the process, so it runs fenced and its attribute cap truncates a producer's tail rather than the diagnosis: a wedged queue, a retired provider, or a cleaner refusal costs the wire line alone, never the console residue beside it or the caller's next statement, and a flooded line still carries its traceback, stack, and callsite.
- Entry: `configure(floor, ship, limits=, scope=)` returns a `LogReceipt` — the floor keys `LEVEL_METHOD` for the filtering wrapper and the stdlib root level, the ship value is the same one the caller hands `Telemetry.install`, and `limits` is the per-composition cap policy. Chain rows resolve their logger through the registered provider alone and name no provider instance, so the spec rail is a `SimpleLogRecordProcessor(InMemoryLogRecordExporter())` added to that one provider: every native and foreign line lands as a `ReadableLogRecord` a spec reads back off `get_finished_logs()` and asserts body, severity, timestamp, and attribute shape against, with no collector and no second registration against the set-once global. Custody keys by `ScopeKey` like every sibling owner: a same-scope re-configure with identical columns restamps `REENTRANT`, and a changed column re-folds. `_posture` is that fold — the strictest reading of every live composition's request, the finest floor and the tightest caps, with the wire arming as a union because a sink is a destination a composition adds and never one it takes from a host — so an embedded composition can neither silence a host's debug floor nor loosen its payload bound. Every admitted change restamps the whole registry with the fold's result, so a stored row is never a posture the process has moved past and `REENTRANT` answers the live floor, egress arm, and caps rather than the ones standing when that scope first configured.
- Auto: `trace_context` is the one writer of the three correlation keys — a valid span context off `trace.get_current_span().get_span_context()` stamps `trace_id` as `032x`, `span_id` as `016x`, and the integer `trace_flags`, an invalid one strips whatever a foreign record's `extra` injected — so a console line correlates to the C#-parented trace or to nothing, never to a caller-supplied string; the wire record resolves the identical correlation from the ambient context at construction, so those three keys ride `RECORD_SLOTS` rather than duplicating as attributes. `bounded` is the one value coercion both renders read: every non-opaque value narrows to the OTLP-admissible shape under the folded depth, width, and length caps, degrades past them through the receipt encoder's own conversion, and leaves a mixed-type collection in a shape the record cleaner keeps whole — so console serializer and wire attribute carry one identical fact and a cyclic or input-scaled producer container raises no `RecursionError` out of the log call. `redact` runs last among the mutators and scrubs the fully assembled line — receipt facts, ambient contextvars fields, callsite rows, the structured traceback, and the timestamp — so no injector lands a classified value downstream of the policy the emit bound, and its `hash` class digests a value the bound already closed. `CallsiteParameterAdder`, `StackInfoRenderer`, and `stdlib.LoggerFactory` share one `IGNORED_FRAMES` roster naming all three emit machineries — receipts' fold, this page's doors, and the faults tier's carried-fault line — so callsite fields, the rendered stack, and the stdlib logger name each resolve the producing owner. Traceback rendering spells its transformer rather than taking the preconfigured `dict_tracebacks`, whose frame-locals default carries every local of every frame under names no `Redaction` table ever rowed and whose rich default reaches for an unadmitted package. `_severity` derives the OTel band arithmetically — the bands are four wide and decade-aligned to stdlib numbering — reading a foreign record's exact `levelno` when one is present and the receipts-owned row otherwise, so a library registering its own level name reaches the wire in-band instead of killing the formatter on a name lookup, and it carries that registered name out beside the band because `severity_text` is the source's own spelling and the band is only what a backend orders on.
- Packages: `structlog` (chain, `ProcessorFormatter`, `ExtraAdder`, `LoggerFactory`, filtering wrapper, callsite and stack rows, the traceback transformer), `opentelemetry-api` (span-context read and the logs emit seam), `msgspec` (the policy and receipt `Struct` pair, `structs.replace`/`asdict`, the `to_builtins` value conversion), `expression` (`Option`/`Block`/`Map`), stdlib `logging`/`sys`/`threading` — this page is the one sanctioned stdlib-logging call site and the one owner of the interpreter's terminal hooks — faults (`SCOPES`, `Scope`, `boundary`, `scoped`), and receipts (`DEFAULT_SCOPE`, `ENCODE`, `EventDict`, `LEVEL_METHOD`, `LogLevel`, `OPEN`, `REDACTION_KEY`, `Redaction`, `ScopeKey`).
- Growth: a new chain concern is one `shared_chain` row reaching both render paths; a new egress target is one `LogShip` member with its `SHIP_OTLP` row; a new cap is one `LogLimits` column reaching the bound and the meet together; a new callsite field is one `CALLSITE` member; a new terminal-hook door is one `DOORS` row spelling its own payload projection; a new record slot is one entry on the roster `RECORD_SLOTS` derives from; a new log level reaches this page through the receipts-owned `LEVEL_METHOD` row with no edit here.
- Boundary: this page renders, projects, and ships; it constructs no provider, exporter, or processor of the OTLP pipeline — `observability/telemetry#TELEMETRY` alone registers the `LoggerProvider` this chain resolves, and every runtime module below the composition root emits through `Signals`, never a direct stdlib-logging call. No SDK import enters: the logs API carries the whole emit seam, so the cold `sdk._logs` tier reifies at telemetry's install alone and `LogLimits` is a page-owned policy shape rather than the SDK type whose import reifies that tier in every composition root. Exception semantics stay the SDK's — handing `emit` the raised object is what lands the module-qualified type, the message, and the stack under the specification's own attribute names, so this page spells none of the three and admits no constant for them. SDK env-resolved record limits stand behind the wire as the deployment ceiling; the chain-resident bound is the policy floor both renders cross, applied before the record exists so a hostile payload reaches neither the console serializer nor the batch queue.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import logging  # the ONE sanctioned stdlib-logging call site: the console handler, the root floor, and the foreign bridge
import sys
import threading
from collections.abc import Callable, Mapping, Sequence, Set
from enum import StrEnum
from functools import cache, partial
from itertools import islice
from threading import RLock
from time import time_ns
from types import ModuleType
from typing import ClassVar, Final

import structlog
from expression import Option
from expression.collections import Block, Map
from msgspec import Struct, to_builtins
from msgspec.structs import asdict, replace
from opentelemetry import trace
from opentelemetry._logs import Logger, LoggerProvider, SeverityNumber, get_logger, get_logger_provider

from rasm.runtime.faults import SCOPES, Scope, boundary, scoped
from rasm.runtime.receipts import DEFAULT_SCOPE, ENCODE, LEVEL_METHOD, OPEN, REDACTION_KEY, EventDict, LogLevel, Redaction, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------

# structlog's own mutable event dict, distinct from the receipts-owned `EventDict` projection shape: a processor
# parameter narrowed to that owner's `dict[str, object]` refuses the `MutableMapping` structlog hands every row.
type ChainEvent = structlog.typing.EventDict

# per-door payload projection: each interpreter hook spells its own arity — three positionals, one thread-args record,
# one unraisable record — and folds onto the pair one shared emitter takes, so that emitter never grows a hook branch.
type Uncaught = Callable[..., tuple[BaseException | None, EventDict]]


class LogShip(StrEnum):
    CONSOLE = "console"
    OTLP_CONSOLE = "otlp-console"


# CONFIGURED folded a new or changed request into the posture; REENTRANT restamps an identical same-scope request.
class LogOutcome(StrEnum):
    CONFIGURED = "configured"
    REENTRANT = "reentrant"


# --- [CONSTANTS] ------------------------------------------------------------------------

# log-signal instrumentation scope: the faults-owned row names the emitting library, and the receipts-owned `scoped`
# stamp versions it and pins the semconv url, so the logger coordinate bumps with the meter's and the tracer's.
LOG_SCOPE: Final[str] = SCOPES[Scope.LOGGER]

# one frame roster the callsite adder, the rendered stack, and the stdlib logger-name deduction all skip, so a line
# names its producing owner instead of the emit machinery that carried it. `faults` earns its row on one member:
# `faulted` writes the carried-fault line through that tier's own module-scope logger, so without the row every fault
# a producer's rail arm reports resolves the fence's own file as both its callsite and its logger name.
IGNORED_FRAMES: Final[list[str]] = ["rasm.runtime.faults", "rasm.runtime.logging", "rasm.runtime.receipts"]

# callsite roster narrowed off the adder's eleven-parameter default, which spends four keys on one source location
# (`pathname` beside `filename`, `module`, `qual_module`), two on process identity the resource plane already carries,
# and one integer `thread` colliding with a producer's own thread fact. These four localize a line and repeat nothing;
# every default row past them is width charged against the attribute cap on every record the process ever writes.
CALLSITE: Final[frozenset[structlog.processors.CallsiteParameter]] = frozenset({
    structlog.processors.CallsiteParameter.PATHNAME,
    structlog.processors.CallsiteParameter.LINENO,
    structlog.processors.CallsiteParameter.FUNC_NAME,
    structlog.processors.CallsiteParameter.THREAD_NAME,
})

# rows the CHAIN writes downstream of a producer's own fields — the structured traceback, the rendered stack, and the
# four callsite keys. Chain insertion order alone drops exactly those first under a flooded line, a producer's own
# fields and the ambient contextvars both merging ahead of them, so the projection ranks this roster first and the
# count cap truncates a producer's tail rather than the diagnosis every incident is reconstructed from.
DIAGNOSTIC: Final[tuple[str, ...]] = ("exception", "stack", *sorted(parameter.value for parameter in CALLSITE))

# slot the fault row resolves and the wire row consumes: `emit` derives the whole semantic exception triple from the
# live object, so no constant here spells an exception attribute name and no package admission gates one.
RAISED: Final[str] = "_raised"

# values the chain reads by TYPE rather than by content — structlog's stdlib-bridge pair, the resolved exception, and
# the emit-bound policy — so the bound passes them through uncoerced; a converted copy would leave the severity row
# without a record, the wire row without an exception, and the redaction row without a policy. `remove_processors_meta`
# strips the bridge pair before the console render, `redact` strips its own key, `shipped` strips the exception slot.
OPAQUE: Final[frozenset[str]] = frozenset({"_record", "_from_structlog", RAISED, REDACTION_KEY})

# the three keys `ProcessorFormatter` seeds onto a foreign event BEFORE its pre-chain runs — the message body and the
# origin pair the severity, clock, and event-name rows each discriminate on — and the only chain-owned keys no later
# row rewrites unconditionally. `ExtraAdder` merges a record's whole `extra` mapping over them, so the merge hands
# these three back and a library binding `event` in its own `extra` cannot replace the message the wire ships.
SEEDED: Final[frozenset[str]] = frozenset({"event", "_record", "_from_structlog"})

# correlation roster `trace_context` writes whole or strips whole; the record resolves the identical trace, span, and
# flags from the ambient context at construction, so an attribute copy renders one fact twice on the wire.
CORRELATION: Final[frozenset[str]] = frozenset({"trace_id", "span_id", "trace_flags"})

# keys the log record carries in its own slots — correlation, the body, the severity token, and the record's own stamp —
# derived from the two rosters above so a new opaque or correlation key reaches both the bound and the wire from one edit;
# every remaining key becomes an attribute.
RECORD_SLOTS: Final[frozenset[str]] = OPAQUE | CORRELATION | frozenset({"event", "level", "timestamp"})

NANOS: Final[int] = 1_000_000_000

# --- [MODELS] ---------------------------------------------------------------------------


class LogLimits(Struct, frozen=True):
    # payload caps applied in the chain, mirroring the four-column `LogRecordLimits` shape and its record-specific-over-
    # global precedence. Reifying that SDK type here would import the cold `sdk._logs` tier into every composition root,
    # so this owner spells the shape and the SDK's env-resolved instance stands behind the wire. The last three columns
    # have no SDK twin: the record cleaner truncates strings alone, so nesting depth, collection width, and byte length
    # are bounds only this floor carries — and a producer structure is caller-shaped, so each is a real hostile axis.
    max_attributes: int = 64
    max_attribute_length: int = 4096
    max_log_record_attributes: int | None = None
    max_log_record_attribute_length: int | None = None
    max_exception_frames: int = 16
    max_value_depth: int = 8
    max_value_items: int = 128

    def bounds(self) -> tuple[int, int]:
        return (
            self.max_log_record_attributes if self.max_log_record_attributes is not None else self.max_attributes,
            self.max_log_record_attribute_length if self.max_log_record_attribute_length is not None else self.max_attribute_length,
        )

    @staticmethod
    def met(left: "LogLimits", right: "LogLimits") -> "LogLimits":
        # limits-meet instance: two compositions in one process render under the tighter of every resolved cap, and the
        # meet lands on the record columns because that resolved pair is what the projection reads.
        (left_count, left_length), (right_count, right_length) = left.bounds(), right.bounds()
        return LogLimits(
            max_attributes=min(left.max_attributes, right.max_attributes),
            max_attribute_length=min(left.max_attribute_length, right.max_attribute_length),
            max_log_record_attributes=min(left_count, right_count),
            max_log_record_attribute_length=min(left_length, right_length),
            max_exception_frames=min(left.max_exception_frames, right.max_exception_frames),
            max_value_depth=min(left.max_value_depth, right.max_value_depth),
            max_value_items=min(left.max_value_items, right.max_value_items),
        )


class LogReceipt(Struct, frozen=True):
    # the requested triple is this composition's ask; the folded triple is the posture every event in the process
    # actually renders under, so a support bundle answers what floor, what egress, and what caps stood at capture.
    outcome: LogOutcome
    scope: ScopeKey
    floor: LogLevel
    ship: LogShip
    limits: LogLimits
    effective: LogLevel
    otlp: bool
    bounds: LogLimits
    compositions: tuple[ScopeKey, ...] = ()


LOG_LIMITS: Final[LogLimits] = LogLimits()

# --- [OPERATIONS] -------------------------------------------------------------------------


def _merged(adder: structlog.stdlib.ExtraAdder, logger: logging.Logger, name: str, event: ChainEvent) -> ChainEvent:
    # foreign leg's head: the untrusted merge runs WHOLE — the adder's capability stays composed rather than
    # re-implemented as a deny-listed copy, its `allow` collection serving a known producer set this chain never has —
    # and the `SEEDED` trio wins it back afterwards. Every other chain-owned key is written unconditionally by a row
    # downstream of the merge; these three are written by the formatter UPSTREAM of it and by nothing after, so
    # without the reseat a producer's `extra` decides the body the wire ships and the origin every record-keyed row
    # reads. `dict()` narrows structlog's mapping at the one crossing that merges over it.
    seeded = {key: event[key] for key in SEEDED if key in event}
    return dict(adder(logger, name, event)) | seeded


def trace_context(_: object, __: str, event: ChainEvent) -> ChainEvent:
    # sole writer AND sole eraser of the correlation roster: the foreign pre-chain's `ExtraAdder` merges a stdlib record's
    # whole `extra` mapping, so a library binding its own `trace_id` would otherwise render a correlation this process is
    # not inside — dropping the roster whenever the ambient span context is invalid makes the field mean one thing.
    ctx = trace.get_current_span().get_span_context()
    kept = {key: value for key, value in event.items() if key not in CORRELATION}
    correlated = (
        {"trace_id": trace.format_trace_id(ctx.trace_id), "span_id": trace.format_span_id(ctx.span_id), "trace_flags": int(ctx.trace_flags)}
        if ctx.is_valid
        else {}
    )
    return kept | correlated


def faulted(_: object, __: str, event: ChainEvent) -> ChainEvent:
    # egress-armed row seated ahead of the renderer that POPS the slot: it resolves structlog's three documented
    # `exc_info` spellings — a bare exception, an `(type, value, traceback)` triple, and ANY truthy flag falling back
    # to the handled exception — down to the one live object, so the console's structured frames and the wire's
    # semantic triple report one exception rather than two independent reads of a consumed slot. The flag arm is
    # truthiness rather than the `True` literal because the renderer's own resolution is, and a `case True` pattern
    # compares by IDENTITY: the ubiquitous `exc_info=1` would otherwise render frames on the console while the wire
    # shipped an error carrying no exception at all. Writing the resolved object back leaves the renderer its own
    # input shape, and `sys.exception()` answers `None` off an empty handler exactly as that resolution does.
    match event.get("exc_info"):
        case BaseException() as raised:
            pass
        case (_, BaseException() as raised, _):
            pass
        case flag:
            raised = sys.exception() if flag else None
    return event if raised is None else event | {RAISED: raised, "exc_info": raised}


def redact(_: object, __: str, event: ChainEvent) -> ChainEvent:
    # last mutator before the wire row: it scrubs the fully-assembled line — receipt facts, contextvars fields, callsite
    # rows, the rendered traceback, the timestamp — so no injector lands a classified value downstream of the policy.
    # A line with no bound policy (a foreign bridge record) folds keep-all. `dict()` narrows structlog's mapping onto
    # the receipts-owned event shape at the one crossing, and `apply` returns its own dict either way.
    bound = event.get(REDACTION_KEY)
    return (bound if isinstance(bound, Redaction) else OPEN).apply(dict(event))


def _homogeneous(members: list[object]) -> list[object] | dict[str, object]:
    # the record cleaner types a collection off its first non-null element and DROPS THE WHOLE VALUE on the first element
    # of another type — `bool` beside `int` clears that bar — so a mixed collection would reach the wire as null while
    # the console rendered it whole. Projecting onto an index-keyed mapping keeps every element's own type and its
    # position, mappings carrying no homogeneity rule, and both renders read this one already-projected shape.
    kinds = {type(member) for member in members if member is not None}
    return members if len(kinds) < 2 else {str(index): member for index, member in enumerate(members)}


def _wire(value: object, length: int, items: int, depth: int) -> object:
    # Log-record attributes take the SDK's EXTENDED shape — null, scalars, byte strings, and nested collection and
    # mapping values all survive the record cleaner, unlike a span's flat primitive rule — so this coercion preserves
    # structure rather than flattening it. Every unbounded axis a producer controls closes here: depth ends the descent
    # before the interpreter's frame limit does (a self-referential mapping is one caller mistake away), width bounds a
    # long collection, and length bounds text and byte strings alike — the record cleaner truncates neither bytes nor
    # nesting. Text and every buffer shape are themselves Sequences, so their arms precede the collection folds; the depth guard
    # rides the descending arms alone, so a leaf keeps its own type at every level and only a container past the bound
    # degrades. A `Struct` opens SHALLOW and spends no level of its own — the mapping its projection lands on charges the
    # descent, and each field re-enters this guard rather than riding a whole-tree conversion. Every remaining value
    # rides `to_builtins` under the receipt encoder's own deterministic order and `repr` hook, so an enum, timestamp,
    # UUID, decimal, or dataclass lands as the value that encoder already writes instead of as repr text both renders
    # then carry; that call owns leaf conversion alone, every self-referential CONTAINER having degraded above it.
    match value:
        case str() as text:
            return text[:length]
        case bytes() | bytearray() | memoryview() as raw:
            # every buffer shape rides ONE arm and lands as the `bytes` the record cleaner keeps whole: a `bytearray`
            # and a `memoryview` are both Sequences, so the collection fold below would otherwise expand a caller's
            # buffer into a list of integers — width-charged against the item cap, type-shifted, and unreadable as
            # the bytes it is — on the console line and the wire attribute alike.
            return bytes(raw[:length])
        case None | bool() | int() | float():
            return value
        case Struct() as owner if depth > 0:
            return _wire(asdict(owner), length, items, depth)
        case Mapping() as mapping if depth > 0:
            return {str(key): _wire(item, length, items, depth - 1) for key, item in islice(mapping.items(), items)}
        case Set() as members if depth > 0:
            # an unordered collection sorts before it narrows, so one membership renders under one order on both sides.
            return _homogeneous([_wire(item, length, items, depth - 1) for item in islice(sorted(members, key=repr), items)])
        case Sequence() as sequence if depth > 0:
            return _homogeneous([_wire(item, length, items, depth - 1) for item in islice(sequence, items)])
        case _ if depth > 0:
            return _wire(to_builtins(value, builtin_types=(bytes,), str_keys=True, order="deterministic", enc_hook=repr), length, items, depth - 1)
        case _:
            return repr(value)[:length]


def bounded(limits: LogLimits, _: object, __: str, event: ChainEvent) -> ChainEvent:
    # one value coercion the whole chain shares, bound to its limits row through `partial` and seated upstream of BOTH
    # renders: console serializer and wire projection read identical already-narrowed values, so neither re-derives the
    # degrade and neither walks a structure the other survived. Seating it upstream of `redact` is what lets that row's
    # `hash` class digest a closed value, and `OPAQUE` rides through untouched because those keys are read by type.
    _, length = limits.bounds()
    return {
        key: value if key in OPAQUE else _wire(value, length, limits.max_value_items, limits.max_value_depth) for key, value in event.items()
    }


def _attributed(event: ChainEvent, count: int) -> dict[str, object]:
    # the count cap truncates a TAIL, so what ranks decides what an operator still reads off a flooded line. Chain
    # insertion order alone ranks the diagnosis LAST — the traceback, the stack, and the callsite are written after a
    # producer's own fields and after the ambient contextvars merged ahead of them — so the `DIAGNOSTIC` roster is
    # projected first and everything else follows in its own insertion order, the merge keeping each key's bounded
    # value and the left operand's position. Values arrive already narrowed from the chain row.
    kept = {key: value for key, value in event.items() if key not in RECORD_SLOTS}
    return dict(islice(({key: kept[key] for key in DIAGNOSTIC if key in kept} | kept).items(), count))


def _raised(event: ChainEvent) -> BaseException | None:
    # typed read of the fault slot: the wire row hands `emit` an exception or nothing, never the untyped slot value.
    match event.get(RAISED):
        case BaseException() as fault:
            return fault
        case _:
            return None


def _severity(event: ChainEvent) -> tuple[SeverityNumber, str]:
    # OTel severity bands are four wide and decade-aligned to stdlib numbering, so the band derives and no second level
    # table stands beside LEVEL_METHOD. A foreign record carries its exact numeric level — a library-registered level
    # name resolves through no map here — and a native event keys the receipts-owned row, every structlog method name
    # folding onto that closed literal before the row is read. Number and TEXT resolve together because the two answer
    # different questions: the band is what a backend filters and orders on, while `severity_text` is the source's own
    # spelling by specification, so a library's `NOTICE`, `TRACE`, or `AUDIT` reaches the wire as itself rather than
    # collapsing onto whichever band it landed in and leaving the operator no name to search the producer's docs for.
    match event.get("_record"):
        case logging.LogRecord() as record:
            numeric, text = record.levelno, record.levelname
        case _:
            numeric, text = LEVEL_METHOD[(level := event["level"])][0], level.upper()
    return (SeverityNumber(max(numeric, 0) // 10 * 4 + 1) if numeric < 50 else SeverityNumber.FATAL), text


def _stamped(event: ChainEvent) -> int:
    # event time, never observation time. A bridged record's own `created` is the moment its producer logged, while this
    # row runs at the handler's format call — a queue handler, a memory handler, or a slow sink between them is real
    # drift — and `emit` leaves the record's `timestamp` unset when a producer passes none, so the wire would otherwise
    # carry the SDK's observation as the record's only clock and every foreign line would date from its render.
    match event.get("_record"):
        case logging.LogRecord() as record:
            return int(record.created * NANOS)
        case _:
            return time_ns()


@cache
def _logger(registered: LoggerProvider) -> Logger:
    # the mint is the receipts-owned coordinate — never a hand-built `get_logger` whose unversioned, schema-free scope
    # a backend cannot join against its siblings. The globally registered provider is the memo KEY, not an argument:
    # the SDK caches no Logger, so a per-emit mint allocates on the hot path, while the pre-install no-op provider is a
    # distinct object from the installed one, so the handle upgrades at install with no invalidation.
    return scoped(get_logger, LOG_SCOPE)


def shipped(count: int, _: object, __: str, event: ChainEvent) -> ChainEvent:
    # terminal row of an armed chain and the one wire projection: body carries the event name, every remaining key
    # becomes an attribute under the count cap the builder already resolved, and the raised object hands the SDK the
    # whole semantic exception triple — module-qualified type, message, and stack under the specification's own names —
    # so this page spells none of them and the structured frame list stays queryable beside them. `event_name` takes the
    # bounded producer slot native events carry and stays unset for a foreign record whose message is free-form. The
    # dict passes on with the fault slot dropped, so the console renders exactly the scrubbed line the wire took.
    # The projection is FENCED because it is the one row that reaches outside the process: a wedged exporter queue, a
    # provider the composition root already shut down, or a record-cleaner refusal would otherwise raise straight out
    # of the caller's own `log.info` on the native leg, where structlog fences no processor — taking the console
    # residue and the producer's next statement with a line neither was asked to carry. The rail is discarded by the
    # law the terminal doors state: the one sink that would report it is the plane this fence just caught refusing.
    number, text = _severity(event)
    boundary(
        "logging.ship",
        lambda: _logger(get_logger_provider()).emit(
            timestamp=_stamped(event),
            body=event.get("event"),
            event_name=None if "_record" in event else event.get("event"),
            severity_number=number,
            severity_text=text,
            attributes=_attributed(event, count),
            exception=_raised(event),
        ),
    )
    return {key: value for key, value in event.items() if key != RAISED}


def _shown(value: object) -> str:
    # every door projection renders a producer-owned object, and `__repr__` is producer code: a hostile or
    # half-constructed object raises inside the one hook whose whole job is reporting a failure the interpreter has
    # already given up on. The fence renders the type alone when the repr refuses, so the door still emits its line
    # rather than losing the crash to the render of one field.
    return boundary("logging.door", lambda: repr(value)).default_with(lambda _refused: f"<{type(value).__name__}>")


def _reported(door: str, fold: tuple[BaseException | None, EventDict]) -> None:
    # the door's own line: projection and render reach the chain as ONE call, so the fence above wraps both and
    # neither can strand the predecessor hook behind it.
    structlog.get_logger().critical(door, exc_info=fold[0], **fold[1])


def _uncaught(door: str, project: Uncaught, prior: Callable[..., object], *payload: object) -> None:
    # one emitter every door shares: its row projects that hook's own payload, the line crosses the whole chain so the
    # crash reaches the wire carrying its semantic exception triple, and the captured predecessor STILL runs — leaving
    # an embedding host's own hook and the interpreter's stderr traceback exactly as they stood. Projection and chain
    # are both producer-reachable code, so a raise escaping this door replaces the interpreter's own report with
    # itself, and this door is the surface that just failed. The fence folds every ingress class the branch converts
    # and never widens past `Exception` by the faults owner's law, so the predecessor rides `finally` rather than the
    # fence: a `KeyboardInterrupt` landing on the render — precisely what a human does while a crash report writes —
    # would otherwise skip a host's own hook on its way out. The rail is discarded by law: the one sink that would
    # carry it is the plane this fence caught refusing.
    try:
        boundary(door, lambda: _reported(door, project(*payload)))
    finally:
        prior(*payload)


def _posture(receipts: Map[ScopeKey, LogReceipt]) -> tuple[LogLevel, bool, LogLimits]:
    # strictest live reading per column: the finest floor and the tightest caps, with the wire arming as a union — a
    # sink is a destination a composition adds, never one it takes from a host. The caller seeds its own row first,
    # so the fold never reads an empty registry.
    rows = Block.of_seq(receipts.values())
    return (
        rows.fold(lambda held, row: row.floor if LEVEL_METHOD[row.floor][0] < LEVEL_METHOD[held][0] else held, rows[0].floor),
        rows.fold(lambda held, row: held or SHIP_OTLP[row.ship], False),
        rows.fold(lambda held, row: LogLimits.met(held, row.limits), rows[0].limits),
    )


# --- [TABLES] ---------------------------------------------------------------------------

# one egress dispatch row per ship member — telemetry's LOG provider gate reads this same row, so the provider half and
# the chain half of the egress cannot diverge on an identity comparison neither side owns.
SHIP_OTLP: Final[Map[LogShip, bool]] = Map.of_seq([(LogShip.CONSOLE, False), (LogShip.OTLP_CONSOLE, True)])

# every terminal interpreter door whose default writes an unstructured traceback straight to stderr, below the handler
# roster and outside every chain: a dead thread, an uncaught main-thread raise, and a finalizer's unraisable are among
# the lines an operator most needs on the wire and the ones this owner would otherwise never see. A row names the module
# slot it wraps and projects that hook's own payload shape onto the shared emitter; a new door is one row. A row
# rendering a producer-owned object reaches it through `_shown` rather than a bare `repr`: the object whose finalizer
# just died is precisely the one whose `__repr__` refuses, and its raise would take the whole crash report with it.
# A row projects only what its hook knows that the chain does not: `origin` therefore names the dead finalizer's own
# object and nothing else, the dying thread already reaching every line as the callsite row's `thread_name` because
# each hook runs ON the thread it reports — a second key for that one fact would fork the name across two rows.
DOORS: Final[Block[tuple[ModuleType, str, Uncaught]]] = Block.of_seq([
    (sys, "excepthook", lambda _kind, raised, _traceback: (raised, {})),
    (sys, "unraisablehook", lambda event: (event.exc_value, {"origin": _shown(event.object), "detail": event.err_msg or ""})),
    (threading, "excepthook", lambda args: (args.exc_value, {})),
])


# one processor table both render paths read, built once per configure so its limits-parameterized rows carry the folded
# caps as policy rather than a literal; a new concern is one row here. Order is law: every injector runs before `bounded`
# so nothing lands unnarrowed, `bounded` before `redact` so the policy classifies closed values, `redact` last among the
# mutators so it scrubs everything the injectors added, and `shipped` closes the table so the wire reads exactly the
# scrubbed line the console then renders. Egress arming decides MEMBERSHIP rather than a per-event branch: `faulted`
# seats ahead of the renderer that consumes the fault slot and `shipped` at the tail, so an unarmed chain carries
# neither row, spends nothing per line, and leaves no private slot for the console render to find. The traceback row is
# spelled explicitly rather than through `dict_tracebacks`: that preconfigured row carries `show_locals=True`, dumping
# every frame local into the line and onto the wire, and a `Redaction` classifies the key NAMES its table holds at
# whatever depth they sit — a frame local named by the producer's own binding is a name no policy ever rowed, so the
# locals reach the wire unscrubbed however deep the fold descends. The same row also carries `use_rich=True`, which
# reaches for a package this branch never admitted.
def shared_chain(limits: LogLimits, otlp: bool) -> tuple[structlog.typing.Processor, ...]:
    count, _ = limits.bounds()
    resolve: tuple[structlog.typing.Processor, ...] = (faulted,) if otlp else ()
    project: tuple[structlog.typing.Processor, ...] = (partial(shipped, count),) if otlp else ()
    return (
        structlog.contextvars.merge_contextvars,
        structlog.processors.add_log_level,
        trace_context,
        structlog.processors.CallsiteParameterAdder(parameters=CALLSITE, additional_ignores=IGNORED_FRAMES),
        structlog.processors.StackInfoRenderer(additional_ignores=IGNORED_FRAMES),
        *resolve,
        structlog.processors.ExceptionRenderer(
            structlog.tracebacks.ExceptionDictTransformer(show_locals=False, use_rich=False, max_frames=limits.max_exception_frames)
        ),
        structlog.processors.TimeStamper(fmt="iso"),
        partial(bounded, limits),
        redact,
        *project,
    )


# --- [SERVICES] -------------------------------------------------------------------------


class LogPipeline:
    # scope-keyed custody over one process pipeline: `_receipts` holds each composition's evidence, `_process` the
    # folded posture the bundle capsule reads, and `_console` the identity-stable handler a re-configure re-formats in
    # place — doubling as the first-configure witness the process doors arm behind. The stdlib root handler roster is
    # process-global, so the posture is folded rather than partitioned — an embedded composition self-identifies
    # through the `composition` field its bound logger carries.
    _receipts: ClassVar[Map[ScopeKey, LogReceipt]] = Map.empty()
    _process: ClassVar[LogReceipt | None] = None
    _console: ClassVar[logging.Handler | None] = None
    _gate: ClassVar[RLock] = RLock()

    @classmethod
    def configure(
        cls,
        floor: LogLevel = "info",
        ship: LogShip = LogShip.OTLP_CONSOLE,
        *,
        limits: LogLimits = LOG_LIMITS,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> LogReceipt:
        with cls._gate:
            match cls._receipts.try_find(scope):
                # every stored row carries the LIVE folded columns, restamped below on each admitted change, so an
                # unchanged request restamps its own row and never hands back a posture a sibling composition moved.
                case Option(tag="some", some=prior) if (prior.floor, prior.ship, prior.limits) == (floor, ship, limits):
                    return replace(prior, outcome=LogOutcome.REENTRANT)
                case _:
                    pass

            seeded = cls._receipts.add(
                scope,
                LogReceipt(
                    outcome=LogOutcome.CONFIGURED, scope=scope, floor=floor, ship=ship, limits=limits,
                    effective=floor, otlp=SHIP_OTLP[ship], bounds=limits,
                ),
            )
            effective, otlp, bounds = _posture(seeded)
            compositions = tuple(sorted(seeded.keys()))
            # the fold reads only the requested columns, so restamping every row is total and idempotent — the registry
            # holds one posture, and `receipt()` answers the same floor, egress arm, and caps whichever row a reader takes.
            stamped = seeded.map(lambda _key, row: replace(row, effective=effective, otlp=otlp, bounds=bounds, compositions=compositions))
            receipt = stamped[scope]

            # the native tail hands the assembled dict to the console handler's formatter, so one writer owns stdout and
            # the foreign leg's pre-chain is the same table — never a second renderer racing the stream at byte grain.
            chain = shared_chain(bounds, otlp)
            structlog.configure(
                processors=[*chain, structlog.stdlib.ProcessorFormatter.wrap_for_formatter],
                wrapper_class=structlog.make_filtering_bound_logger(LEVEL_METHOD[effective][0]),
                logger_factory=structlog.stdlib.LoggerFactory(ignore_frame_names=IGNORED_FRAMES),
                # spelled rather than defaulted: `configure` is a PARTIAL update leaving every omitted slot at
                # whatever stands, so a host that cached loggers before this composition root ran would freeze every
                # held handle at its first posture and the whole re-configure fold would answer a floor no line uses.
                cache_logger_on_first_use=False,
            )
            # constructed on the first configure, never at import: a host that reopens or captures stdout before the
            # composition root runs binds its live stream rather than the one this module saw at load.
            handler = cls._console if cls._console is not None else logging.StreamHandler(stream=sys.stdout)
            handler.setFormatter(
                structlog.stdlib.ProcessorFormatter(
                    # the reseated ExtraAdder rides the foreign path alone — native events never populate stdlib
                    # `extra` — and precedes the shared rows, so correlation, the payload bound, redaction, and the
                    # wire projection each govern the fields it merges rather than admitting a foreign record's whole
                    # `extra` mapping unread, while `SEEDED` holds back the three keys no shared row rewrites.
                    foreign_pre_chain=[partial(_merged, structlog.stdlib.ExtraAdder()), *chain],
                    processors=[
                        structlog.stdlib.ProcessorFormatter.remove_processors_meta,
                        structlog.processors.EventRenamer(to="body"),
                        structlog.processors.JSONRenderer(serializer=lambda line, **_kw: ENCODE(line).decode()),
                    ],
                )
            )
            root = logging.getLogger()
            if handler not in root.handlers:
                root.addHandler(handler)
            root.setLevel(LEVEL_METHOD[effective][0])

            if cls._console is None:
                # process doors arm once with the first composition and never disarm — arming is additive exactly as the
                # egress union is, each door wrapping whatever hook stood before it, so a second composition finds them
                # standing and an embedding host keeps its own. The warnings bridge is the fourth door and stdlib-owned:
                # it routes `showwarning` onto a logger, so every deprecation a dependency raises crosses this same chain
                # instead of writing past the whole handler roster to stderr.
                logging.captureWarnings(capture=True)
                for host, slot, project in DOORS:
                    setattr(host, slot, partial(_uncaught, f"{host.__name__}.{slot}", project, getattr(host, slot)))

            cls._console = handler
            cls._receipts = stamped
            cls._process = receipt
            return receipt

    @classmethod
    def receipt(cls) -> Option[LogReceipt]:
        # process-custody read matching every sibling install owner: Some once a composition has configured, carrying
        # the folded floor, egress arm, caps, and composition roster the bundle capsule reads as data. The read takes
        # no gate — a single reference load needs none.
        return Option.of_optional(cls._process)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
