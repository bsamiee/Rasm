# [PY_RUNTIME_LOGGING]

`LogPipeline` owns the structlog processor chain and the one log egress every event crosses — native, foreign, and the interpreter's own stderr doors alike. One `shared_chain` table feeds both render paths, so a new concern is one row reaching native chain and foreign stdlib bridge together, and one bound row caps every value's depth, width, and length before either render reads it. Its armed egress pair resolves the raised exception, then tees onto a log record carrying event name as body, remaining fields as attributes, and the SDK-derived exception triple — handing the dict on so the console renders what the wire took.

`Signals.emit`/`emit_async`, the `Receipt` fold, the `Redaction` model, and `LEVEL_METHOD` arrive settled from `observability/receipts#RECEIPT`; the `scoped` instrumentation stamp and the `Scope`/`SCOPES` row it binds arrive from `reliability/faults#FAULT`, the one tier below every emitting owner. `LogShip` is the policy value `observability/telemetry#TELEMETRY` threads into its install, so one value drives both halves of the egress — provider registration there, chain arm here. Chain-resident `redact` applies whatever `Redaction` the emit bound under `REDACTION_KEY`, and a foreign record with no bound policy folds through the keep-all `OPEN`.

## [01]-[INDEX]

- [02]-[PIPELINE]: one shared processor chain, `LogShip` policy over its `SHIP_OTLP` row, logger-name, trace-context, payload-bound, and redaction processors, fault-resolution and wire-projection paired under `LogLimits`, scope-keyed configure custody, one console handler hosting the foreign leg, and `DOORS` chaining every interpreter hook that writes past the handler roster.

## [02]-[PIPELINE]

- Owner: `LogPipeline.configure` wires the whole pipeline once — the shared chain, the folded filtering floor, the console handler's `ProcessorFormatter`, the stdlib root floor, and the process doors — and re-configure re-formats the held handler in place, so an embedding host's handlers survive untouched. `shared_chain` builds the one processor table each configure, so its limits-parameterized rows carry the folded caps as policy rather than a literal: the native structlog path appends `ProcessorFormatter.wrap_for_formatter`, the foreign pre-chain reads the identical rows behind one reseated `ExtraAdder`, so a bridged hypercorn/apscheduler/executor record's `extra` fields join the event before the correlation, callsite, traceback, bound, redaction, and wire rows every native event also crosses, and the three keys the formatter seeded ahead of that merge survive it. One writer owns stdout: the native path routes through `stdlib.LoggerFactory` into the same handler the foreign path lands on, so no binary-layer render races the text layer for the stream.
- Cases: `LogShip` is the egress vocabulary and `SHIP_OTLP` its one dispatch row — `CONSOLE` renders the JSON line and seats no wire row at all, `OTLP_CONSOLE` renders that same line and seats the `faulted`/`shipped` pair projecting every event onto the registered `LoggerProvider`, standing as the exporting process-root default, the deployed collector admitting an OTLP receiver alone. `ProcessorFormatter` on the console handler is the one seam a foreign stdlib record crosses the shared chain through. Arming decides table MEMBERSHIP rather than a per-event branch, so an unarmed process pays nothing per line, and an armed one reaching the chain before the telemetry install resolves the API no-op logger. That projection is the one row leaving the process, so it runs fenced and its attribute cap truncates a producer's tail rather than the diagnosis: a wedged queue, a retired provider, or a cleaner refusal costs the wire line alone, never the console residue beside it or the caller's next statement, and a flooded line still carries its traceback, stack, and callsite.
- Entry: `configure(floor, ship, limits=, scope=)` returns a `LogReceipt` — the floor keys `LEVEL_METHOD` for the filtering wrapper and the stdlib root level, the ship value is the same one the caller hands `Telemetry.install`, and `limits` is the per-composition cap policy. Chain rows resolve their logger through the registered provider alone and name no provider instance, so the spec rail is a `SimpleLogRecordProcessor(InMemoryLogRecordExporter())` added to that one provider: every native and foreign line lands as a `ReadableLogRecord` a spec reads back off `get_finished_logs()` and asserts body, severity, timestamp, and attribute shape against, with no collector and no second registration against the set-once global. Custody keys by `ScopeKey` like every sibling owner: a same-scope re-configure with identical columns restamps `REENTRANT`, and a changed column re-folds. `_posture` is that fold — the strictest reading of every live composition's request, the finest floor and the tightest caps, with the wire arming as a union because a sink is a destination a composition adds and never one it takes from a host — so an embedded composition can neither silence a host's debug floor nor loosen its payload bound. Every admitted change restamps the whole registry with the fold's result, so a stored row is never a posture the process has moved past and `REENTRANT` answers the live floor, egress arm, and caps rather than the ones standing when that scope first configured.
- Auto: `trace_context` is the one writer of the three correlation keys — a valid span context off `trace.get_current_span().get_span_context()` stamps `trace_id` as `032x`, `span_id` as `016x`, and the integer `trace_flags`, an invalid one strips whatever a foreign record's `extra` injected — so a console line correlates to the C#-parented trace or to nothing, never to a caller-supplied string; the wire record resolves the identical correlation from the ambient context at construction, so those three keys ride `RECORD_SLOTS` rather than duplicating as attributes. `bounded` is the one value coercion both renders read: every non-opaque value narrows to the OTLP-admissible shape under the folded depth, width, and length caps, degrades past them through the receipt encoder's own conversion, and leaves a mixed-type collection in a shape the record cleaner keeps whole — so console serializer and wire attribute carry one identical fact and a cyclic or input-scaled producer container raises no `RecursionError` out of the log call. `redact` runs last among the mutators and scrubs the fully assembled line — receipt facts, ambient contextvars fields, callsite rows, the structured traceback, and the timestamp — so no injector lands a classified value downstream of the policy the emit bound, and its `hash` class digests a value the bound already closed. `CallsiteParameterAdder`, `StackInfoRenderer`, and `stdlib.LoggerFactory` share one `IGNORED_FRAMES` roster naming all three emit machineries — receipts' fold, this page's doors, and the faults tier's carried-fault line — so callsite fields, the rendered stack, and the deduced logger name each resolve the producing owner, and `add_logger_name` lands that name on the line off the same `_record` discriminant the severity and clock rows read, answering a bridged record with its producer's own logger where every signal of this process shares one instrumentation scope. Traceback rendering spells its transformer rather than taking the preconfigured `dict_tracebacks`, whose frame-locals default carries every local of every frame under names no `Redaction` table ever rowed and whose rich default reaches for an unadmitted package. `_severity` derives the OTel band arithmetically — the bands are four wide and decade-aligned to stdlib numbering — reading a foreign record's exact `levelno` when one is present and the receipts-owned row otherwise, so a library registering its own level name reaches the wire in-band instead of killing the formatter on a name lookup, and it carries that registered name out beside the band because `severity_text` is the source's own spelling and the band is only what a backend orders on.
- Packages: `structlog` (chain, `ProcessorFormatter`, `ExtraAdder`, `LoggerFactory`, filtering wrapper, logger-name, callsite and stack rows, the traceback transformer), `opentelemetry-api` (span-context read and the logs emit seam), `msgspec` (the policy and receipt `Struct` pair, `structs.replace`/`asdict`, the `to_builtins` value conversion), `expression` (`Option`/`Block`/`Map`), stdlib `logging`/`sys`/`threading`/`warnings` — this page is the one sanctioned stdlib-logging call site and the one owner of the interpreter's stderr doors — faults (`SCOPES`, `Scope`, `boundary`, `scoped`), and receipts (`DEFAULT_SCOPE`, `ENCODE`, `EventDict`, `LEVEL_METHOD`, `LogLevel`, `OPEN`, `REDACTION_KEY`, `Redaction`, `ScopeKey`).
- Law: all three fences resolve a `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.LOGGING` and each keeps the plane's one catch-all with its reason stated at the site — an egress row reaching outside the process, a producer `__repr__` the interpreter has already given up on, and a door whose own failure is the surface that would report it. The door NAME still rides the emitted line, so no coordinate leaves with the free subject string.
- Growth: a new chain concern is one `shared_chain` row reaching both render paths; a new egress target is one `LogShip` member with its `SHIP_OTLP` row; a new cap is one `LogLimits` column reaching the bound and the meet together; a new callsite field is one `CALLSITE` member; a new interpreter door is one `DOORS` row spelling its own severity and payload projection; a new record slot is one entry on the roster `RECORD_SLOTS` derives from; a new log level reaches this page through the receipts-owned `LEVEL_METHOD` row with no edit here.
- Boundary: this page renders, projects, and ships; it constructs no provider, exporter, or processor of the OTLP pipeline — `observability/telemetry#TELEMETRY` alone registers the `LoggerProvider` this chain resolves, and every runtime module below the composition root emits through `Signals`, never a direct stdlib-logging call. No SDK import enters: the logs API carries the whole emit seam, so the cold `sdk._logs` tier reifies at telemetry's install alone and `LogLimits` is a page-owned policy shape rather than the SDK type whose import reifies that tier in every composition root. Exception semantics stay the SDK's — handing `emit` the raised object is what lands the module-qualified type, the message, and the stack under the specification's own attribute names, so this page spells none of the three and admits no constant for them. SDK env-resolved record limits stand behind the wire as the deployment ceiling; the chain-resident bound is the policy floor both renders cross, applied before the record exists so a hostile payload reaches neither the console serializer nor the batch queue.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import logging
import sys
import threading
import warnings
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

from rasm.runtime.faults import LOGGING_DOOR, LOGGING_SHIP, LOGGING_SHOWN, SCOPES, Scope, boundary, scoped
from rasm.runtime.receipts import DEFAULT_SCOPE, ENCODE, LEVEL_METHOD, OPEN, REDACTION_KEY, EventDict, LogLevel, Redaction, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------

type ChainEvent = structlog.typing.EventDict

type Uncaught = Callable[..., tuple[BaseException | None, EventDict]]


class LogShip(StrEnum):
    CONSOLE = "console"
    OTLP_CONSOLE = "otlp-console"


class LogOutcome(StrEnum):
    CONFIGURED = "configured"
    REENTRANT = "reentrant"


# --- [CONSTANTS] ------------------------------------------------------------------------

LOG_SCOPE: Final[str] = SCOPES[Scope.LOGGER]

IGNORED_FRAMES: Final[list[str]] = ["rasm.runtime.faults", "rasm.runtime.logging", "rasm.runtime.receipts"]

CALLSITE: Final[frozenset[structlog.processors.CallsiteParameter]] = frozenset({
    structlog.processors.CallsiteParameter.PATHNAME,
    structlog.processors.CallsiteParameter.LINENO,
    structlog.processors.CallsiteParameter.FUNC_NAME,
    structlog.processors.CallsiteParameter.THREAD_NAME,
})

DIAGNOSTIC: Final[tuple[str, ...]] = ("logger", "exception", "stack", *sorted(parameter.value for parameter in CALLSITE))

RAISED: Final[str] = "_raised"

OPAQUE: Final[frozenset[str]] = frozenset({"_record", "_from_structlog", RAISED, REDACTION_KEY})

SEEDED: Final[frozenset[str]] = frozenset({"event", "_record", "_from_structlog"})

CORRELATION: Final[frozenset[str]] = frozenset({"trace_id", "span_id", "trace_flags"})

RECORD_SLOTS: Final[frozenset[str]] = OPAQUE | CORRELATION | frozenset({"event", "level", "timestamp"})

NANOS: Final[int] = 1_000_000_000

# --- [MODELS] ---------------------------------------------------------------------------


class LogLimits(Struct, frozen=True):
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

# --- [OPERATIONS] -----------------------------------------------------------------------


def _merged(adder: structlog.stdlib.ExtraAdder, logger: logging.Logger, name: str, event: ChainEvent) -> ChainEvent:
    seeded = {key: event[key] for key in SEEDED if key in event}
    return dict(adder(logger, name, event)) | seeded


def trace_context(_: object, __: str, event: ChainEvent) -> ChainEvent:
    ctx = trace.get_current_span().get_span_context()
    kept = {key: value for key, value in event.items() if key not in CORRELATION}
    correlated = (
        {"trace_id": trace.format_trace_id(ctx.trace_id), "span_id": trace.format_span_id(ctx.span_id), "trace_flags": int(ctx.trace_flags)}
        if ctx.is_valid
        else {}
    )
    return kept | correlated


def faulted(_: object, __: str, event: ChainEvent) -> ChainEvent:
    match event.get("exc_info"):
        case BaseException() as raised:
            pass
        case (_, BaseException() as raised, _):
            pass
        case flag:
            raised = sys.exception() if flag else None
    return event if raised is None else event | {RAISED: raised, "exc_info": raised}


def redact(_: object, __: str, event: ChainEvent) -> ChainEvent:
    bound = event.get(REDACTION_KEY)
    return (bound if isinstance(bound, Redaction) else OPEN).apply(dict(event))


def _homogeneous(members: list[object]) -> list[object] | dict[str, object]:
    kinds = {type(member) for member in members if member is not None}
    return members if len(kinds) < 2 else {str(index): member for index, member in enumerate(members)}


def _wire(value: object, length: int, items: int, depth: int) -> object:
    match value:
        case str() as text:
            return text[:length]
        case bytes() | bytearray() | memoryview() as raw:
            return bytes(raw[:length])
        case None | bool() | int() | float():
            return value
        case Struct() as owner if depth > 0:
            return _wire(asdict(owner), length, items, depth)
        case Mapping() as mapping if depth > 0:
            return {str(key): _wire(item, length, items, depth - 1) for key, item in islice(mapping.items(), items)}
        case Set() as members if depth > 0:
            return _homogeneous([_wire(item, length, items, depth - 1) for item in islice(sorted(members, key=repr), items)])
        case Sequence() as sequence if depth > 0:
            return _homogeneous([_wire(item, length, items, depth - 1) for item in islice(sequence, items)])
        case _ if depth > 0:
            return _wire(to_builtins(value, builtin_types=(bytes,), str_keys=True, order="deterministic", enc_hook=repr), length, items, depth - 1)
        case _:
            return repr(value)[:length]


def bounded(limits: LogLimits, _: object, __: str, event: ChainEvent) -> ChainEvent:
    _, length = limits.bounds()
    return {
        key: value if key in OPAQUE else _wire(value, length, limits.max_value_items, limits.max_value_depth) for key, value in event.items()
    }


def _attributed(event: ChainEvent, count: int) -> dict[str, object]:
    kept = {key: value for key, value in event.items() if key not in RECORD_SLOTS}
    return dict(islice(({key: kept[key] for key in DIAGNOSTIC if key in kept} | kept).items(), count))


def _raised(event: ChainEvent) -> BaseException | None:
    match event.get(RAISED):
        case BaseException() as fault:
            return fault
        case _:
            return None


def _severity(event: ChainEvent) -> tuple[SeverityNumber, str]:
    match event.get("_record"):
        case logging.LogRecord() as record:
            numeric, text = record.levelno, record.levelname
        case _:
            numeric, text = LEVEL_METHOD[(level := event["level"])][0], level.upper()
    return (SeverityNumber(max(numeric, 0) // 10 * 4 + 1) if numeric < 50 else SeverityNumber.FATAL), text


def _stamped(event: ChainEvent) -> int:
    match event.get("_record"):
        case logging.LogRecord() as record:
            return int(record.created * NANOS)
        case _:
            return time_ns()


@cache
def _logger(registered: LoggerProvider) -> Logger:
    return scoped(get_logger, LOG_SCOPE)


def shipped(count: int, _: object, __: str, event: ChainEvent) -> ChainEvent:
    number, text = _severity(event)
    boundary(
        LOGGING_SHIP,
        lambda: _logger(get_logger_provider()).emit(
            timestamp=_stamped(event),
            body=event.get("event"),
            event_name=None if "_record" in event else event.get("event"),
            severity_number=number,
            severity_text=text,
            attributes=_attributed(event, count),
            exception=_raised(event),
        ),
        catch=Exception,
    )
    return {key: value for key, value in event.items() if key != RAISED}


def _shown(value: object) -> str:
    return boundary(LOGGING_SHOWN, lambda: repr(value), catch=Exception).default_with(lambda _refused: f"<{type(value).__name__}>")


def _reported(door: str, level: LogLevel, fold: tuple[BaseException | None, EventDict]) -> None:
    LEVEL_METHOD[level][1](structlog.get_logger())(door, exc_info=fold[0], **fold[1])


def _uncaught(door: str, level: LogLevel, project: Uncaught, prior: Callable[..., object], *payload: object) -> None:
    try:
        boundary(LOGGING_DOOR, lambda: _reported(door, level, project(*payload)), catch=Exception)
    finally:
        prior(*payload)


def _posture(receipts: Map[ScopeKey, LogReceipt]) -> tuple[LogLevel, bool, LogLimits]:
    rows = Block.of_seq(receipts.values())
    return (
        rows.fold(lambda held, row: row.floor if LEVEL_METHOD[row.floor][0] < LEVEL_METHOD[held][0] else held, rows[0].floor),
        rows.fold(lambda held, row: held or SHIP_OTLP[row.ship], False),
        rows.fold(lambda held, row: LogLimits.met(held, row.limits), rows[0].limits),
    )


# --- [TABLES] ---------------------------------------------------------------------------

SHIP_OTLP: Final[Map[LogShip, bool]] = Map.of_seq([(LogShip.CONSOLE, False), (LogShip.OTLP_CONSOLE, True)])

DOORS: Final[Block[tuple[ModuleType, str, LogLevel, Uncaught]]] = Block.of_seq([
    (sys, "excepthook", "critical", lambda _kind, raised, _traceback: (raised, {})),
    (
        sys,
        "unraisablehook",
        "critical",
        lambda event: (event.exc_value, {"origin": _shown(event.object)} | ({} if event.err_msg is None else {"detail": event.err_msg})),
    ),
    (threading, "excepthook", "critical", lambda args: (args.exc_value, {})),
    (
        warnings,
        "showwarning",
        "warning",
        lambda message, category, filename, lineno, *_rest: (
            None,
            {"origin": f"{filename}:{lineno}", "kind": category.__name__, "detail": _shown(message)},
        ),
    ),
])


def shared_chain(limits: LogLimits, otlp: bool) -> tuple[structlog.typing.Processor, ...]:
    count, _ = limits.bounds()
    resolve: tuple[structlog.typing.Processor, ...] = (faulted,) if otlp else ()
    project: tuple[structlog.typing.Processor, ...] = (partial(shipped, count),) if otlp else ()
    return (
        structlog.contextvars.merge_contextvars,
        structlog.processors.add_log_level,
        structlog.stdlib.add_logger_name,
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
            stamped = seeded.map(lambda _key, row: replace(row, effective=effective, otlp=otlp, bounds=bounds, compositions=compositions))
            receipt = stamped[scope]

            chain = shared_chain(bounds, otlp)
            structlog.configure(
                processors=[*chain, structlog.stdlib.ProcessorFormatter.wrap_for_formatter],
                wrapper_class=structlog.make_filtering_bound_logger(LEVEL_METHOD[effective][0]),
                logger_factory=structlog.stdlib.LoggerFactory(ignore_frame_names=IGNORED_FRAMES),
                cache_logger_on_first_use=False,
            )
            handler = cls._console if cls._console is not None else logging.StreamHandler(stream=sys.stdout)
            handler.setFormatter(
                structlog.stdlib.ProcessorFormatter(
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
                for host, slot, level, project in DOORS:
                    setattr(host, slot, partial(_uncaught, f"{host.__name__}.{slot}", level, project, getattr(host, slot)))

            cls._console = handler
            cls._receipts = stamped
            cls._process = receipt
            return receipt

    @classmethod
    def receipt(cls) -> Option[LogReceipt]:
        return Option.of_optional(cls._process)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
