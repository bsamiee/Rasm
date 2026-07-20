# [PY_RUNTIME_LOGGING]

`LogPipeline` owns the structlog processor pipeline — one shared chain every event crosses, native and foreign alike, and the one ship policy that decides where a rendered line lands. Structured JSON events ship to stdout where the collector tail promotes `trace_id`/`span_id` onto OTLP log records; stderr stays human diagnostic residue. One `SHARED_CHAIN` table feeds both render tails, so a new processor is one row reaching the native chain and the foreign stdlib bridge in the same edit.

`Signals.emit`/`emit_async`, the `Receipt` fold, the `Redaction` model, and the `LEVEL_METHOD` row table arrive settled from `observability/receipts#RECEIPT`; this page owns only the chain those surfaces render through. `LogShip` is the policy value `observability/telemetry#TELEMETRY` threads into its install, so one value drives both halves of the escape hatch — provider registration there, handler attach here. Chain-resident `redact` applies whatever `Redaction` the emit bound under `REDACTION_KEY`, and a foreign record with no bound policy folds through the keep-all `OPEN`.

## [01]-[INDEX]

- [01]-[PIPELINE]: the shared processor chain, the `LogShip` policy, the trace-context and redaction processors, the stdout ship, and the stdlib bridge.

## [02]-[PIPELINE]

- Owner: `LogPipeline.configure` wires the whole pipeline once — the shared chain, the filtering floor, the byte renderer, and the identity-stable stdlib bridge — and re-configure re-formats the held bridge handler in place, so an embedding host's handlers survive untouched. `SHARED_CHAIN` is the one processor table: the native structlog path appends the render tail, the foreign `ProcessorFormatter` pre-chain reads the identical rows behind one `ExtraAdder`, so a bridged grpcio/apscheduler/executor record's `extra` fields join the event before the correlation, redaction, and callsite rows every native event also crosses.
- Cases: `LogShip` is the ship vocabulary — `STDOUT_COLLECTOR` renders JSON bytes to `sys.stdout.buffer` through `BytesLoggerFactory` and leaves OTLP promotion to the collector tail; `INPROCESS_OTLP` also attaches the `LoggingHandler` bridge resolving the globally registered `LoggerProvider`. `STDOUT_COLLECTOR` is the default because the logs signal is Development: the underscore log modules stay out of the process by default, and the module-scope `lazy from` defers the `LoggingHandler` import so only a selected escape hatch ever reifies `opentelemetry.sdk._logs`.
- Entry: `configure(floor, ship)` — the floor keys `LEVEL_METHOD` for the filtering wrapper and the stdlib root level, and the ship value is the same one the caller hands `Telemetry.install`, so the provider half and the handler half of the escape hatch cannot diverge.
- Auto: `trace_context` reads the active recording span off `trace.get_current_span().get_span_context()` and writes `trace_id` as `032x`, `span_id` as `016x`, and the integer `trace_flags`, so every stdout line correlates to the C#-parented trace before any collector touches it. `redact` scrubs the fully assembled line — receipt facts, ambient contextvars fields, trace ids — after the injectors, so a `bind_contextvars`-sourced classified field cannot bypass the policy the emit bound. `CallsiteParameterAdder` skips the emit machinery's own frames, so callsite fields name the producing owner.
- Packages: `structlog` (chain, `ProcessorFormatter`, `ExtraAdder`, `BytesLoggerFactory`, filtering wrapper), `opentelemetry-api` (span-context read only), stdlib `logging` — this page is the one sanctioned stdlib-logging call site — and receipts (`LEVEL_METHOD`, `LogLevel`, `Redaction`, `OPEN`, `REDACTION_KEY`, `ENCODE`).
- Growth: a new chain concern is one `SHARED_CHAIN` row reaching both render paths; a new ship target is one `LogShip` member with its arm in `configure`; a new log level reaches this page through the receipts-owned `LEVEL_METHOD` row with no edit here.
- Boundary: this page renders and ships; it constructs no provider, exporter, or processor of the OTLP pipeline — `observability/telemetry#TELEMETRY` alone registers the `LoggerProvider` the escape hatch resolves, and every runtime module below the composition root emits through `Signals`, never a direct stdlib-logging call.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import logging  # the ONE sanctioned stdlib-logging call site: the bridge handler, the escape-hatch attach, and the root floor
import sys
from enum import StrEnum
from typing import Final

import structlog
from opentelemetry import trace

from rasm.runtime.receipts import ENCODE, LEVEL_METHOD, OPEN, REDACTION_KEY, LogLevel, Redaction

lazy from opentelemetry.sdk._logs import LoggingHandler  # reified only when the INPROCESS_OTLP escape hatch is selected

# --- [TYPES] ----------------------------------------------------------------------------

# processor signatures type against structlog's own event-dict contract; the receipts-owned projection shape stays that owner's.
type EventDict = structlog.typing.EventDict
type Processor = structlog.typing.Processor


class LogShip(StrEnum):
    STDOUT_COLLECTOR = "stdout-collector"
    INPROCESS_OTLP = "inprocess-otlp"


# --- [OPERATIONS] -------------------------------------------------------------------------


def trace_context(_: object, __: str, event: EventDict) -> EventDict:
    ctx = trace.get_current_span().get_span_context()
    if ctx.is_valid:
        event.update(trace_id=trace.format_trace_id(ctx.trace_id), span_id=trace.format_span_id(ctx.span_id), trace_flags=int(ctx.trace_flags))
    return event


def redact(_: object, __: str, event: EventDict) -> EventDict:
    # scrubs the fully-assembled line after the injectors; a line with no bound policy (a foreign bridge record) folds keep-all.
    bound = event.get(REDACTION_KEY)
    return (bound if isinstance(bound, Redaction) else OPEN).apply(event)


# one processor table both render paths read; a new concern is one row here.
SHARED_CHAIN: Final[tuple[Processor, ...]] = (
    structlog.contextvars.merge_contextvars,
    structlog.processors.add_log_level,
    trace_context,
    redact,
    structlog.processors.CallsiteParameterAdder(additional_ignores=["rasm.runtime.receipts", "rasm.runtime.logging"]),
    structlog.processors.dict_tracebacks,
    structlog.processors.TimeStamper(fmt="iso"),
)

# --- [SERVICES] -------------------------------------------------------------------------

# identity-stable bridge: repeated configure re-formats this handler in place instead of stacking siblings.
# stream=sys.stdout keeps the JSON envelope on stdout with the native path; stderr stays human diagnostics.
_BRIDGE: Final[logging.Handler] = logging.StreamHandler(stream=sys.stdout)


class LogPipeline:
    @staticmethod
    def configure(floor: LogLevel = "info", ship: LogShip = LogShip.STDOUT_COLLECTOR) -> None:
        structlog.configure(
            processors=[*SHARED_CHAIN, structlog.processors.EventRenamer(to="body"), structlog.processors.JSONRenderer(serializer=ENCODE)],
            wrapper_class=structlog.make_filtering_bound_logger(LEVEL_METHOD[floor][0]),
            logger_factory=structlog.BytesLoggerFactory(),
        )
        _BRIDGE.setFormatter(
            structlog.stdlib.ProcessorFormatter(
                # ExtraAdder rides the bridge path alone — native events never populate stdlib `extra` — and precedes the
                # shared rows, so redaction and correlation govern the extra-sourced fields it merges.
                foreign_pre_chain=[structlog.stdlib.ExtraAdder(), *SHARED_CHAIN],
                processors=[
                    structlog.stdlib.ProcessorFormatter.remove_processors_meta,
                    structlog.processors.EventRenamer(to="body"),
                    structlog.processors.JSONRenderer(serializer=lambda line, **_kw: ENCODE(line).decode()),
                ],
            )
        )
        root = logging.getLogger()
        if _BRIDGE not in root.handlers:
            root.addHandler(_BRIDGE)
        if ship is LogShip.INPROCESS_OTLP and not any(isinstance(held, LoggingHandler) for held in root.handlers):
            # escape hatch: resolves the LoggerProvider the telemetry install registered under the same ship value.
            root.addHandler(LoggingHandler(level=LEVEL_METHOD[floor][0]))
        root.setLevel(LEVEL_METHOD[floor][0])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
