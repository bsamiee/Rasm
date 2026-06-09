"""Wire models for automation triggers and actions decoded by the engine.

Triggers (Watch, Schedule, Manual) are the event sources; actions (Rail, Program,
Sequence, Debounce) are the responses. The Trigger and Action type aliases form closed
unions over these cases and are the sole surfaces passed to TRIGGER_DECODER and
ACTION_DECODER. All structs are frozen msgspec.Struct subclasses with tag-based
discriminated-union decoding; unknown fields are rejected at decode time.
"""

from enum import StrEnum
from typing import Annotated

from msgspec import json, Meta, Raw

from tools.assay.core.model import Base, Claim  # noqa: TC001  # runtime msgspec field — deferring breaks struct resolution


# --- [TYPES] ----------------------------------------------------------------------------


class WatchFilter(StrEnum):
    """Wire tag resolved to a watchfiles filter by the engine; unknown tags fail at msgspec decode."""

    DEFAULT = "default"
    PYTHON = "python"


# --- [MODELS] ---------------------------------------------------------------------------


class Watch(Base, frozen=True, tag_field="kind", tag="watch", forbid_unknown_fields=True):
    """Filesystem trigger interpreted by watchfiles.

    cpu_threshold, when set, suppresses a fire when the current CPU utilization meets or
    exceeds the fractional ceiling; the engine evaluates this before handing the event to
    the action pipeline.
    """

    paths: tuple[str, ...]
    filter: WatchFilter = WatchFilter.DEFAULT
    ignore_patterns: tuple[str, ...] = ()
    debounce: Annotated[int, Meta(ge=1)] = 1600
    step: Annotated[int, Meta(ge=1)] = 50
    force_polling: bool | None = None
    poll_delay_ms: Annotated[int, Meta(ge=1)] = 300
    recursive: bool = True
    ignore_permission_denied: bool | None = None
    cpu_threshold: Annotated[float, Meta(ge=0, le=1)] | None = None


class Schedule(Base, frozen=True, tag_field="kind", tag="schedule", forbid_unknown_fields=True):
    """Cron trigger interpreted by aiocron.

    cpu_threshold suppresses a fire when CPU utilization meets or exceeds the fractional
    ceiling at the scheduled instant; the check runs before the action pipeline executes.
    """

    cron: str
    timezone: str = "UTC"
    cpu_threshold: Annotated[float, Meta(ge=0, le=1)] | None = None


class Manual(Base, frozen=True, tag_field="kind", tag="manual", forbid_unknown_fields=True):
    """Immediate trigger that fires once."""


class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    """Rail action resolved against the registry at fire time.

    params carries the raw JSON bytes; the engine defers their decode until the bound
    params type is retrieved from the registry, so an unknown claim or verb fails at
    dispatch rather than at wire decode.
    """

    claim: Claim
    verb: str
    params: Raw = Raw()


class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    """Direct executable and arguments to run verbatim."""

    argv: Annotated[tuple[str, ...], Meta(min_length=1)]


class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    """Ordered actions evaluated by the engine."""

    actions: tuple["Action", ...]  # noqa: UP037  # forward ref — Action alias defined after Sequence


class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    """Action wrapper that coalesces trigger storms within a quiescence window.

    When collapse is True, only the trailing event in the window fires; intermediate
    events are discarded. window_ms is the quiescence duration in milliseconds.
    """

    action: "Action"  # noqa: UP037  # forward ref — Action alias defined after Debounce
    window_ms: int = 500
    collapse: bool = True


type Trigger = Watch | Schedule | Manual
type Action = Rail | Program | Sequence | Debounce


# --- [TABLES] ---------------------------------------------------------------------------


TRIGGER_DECODER: json.Decoder[Trigger] = json.Decoder(Trigger)  # msgspec resolves union members eagerly at Decoder.__init__
ACTION_DECODER: json.Decoder[Action] = json.Decoder(Action)
_ENCODE = json.Encoder(order="deterministic")

# --- [OPERATIONS] -----------------------------------------------------------------------


# total match over a closed union — branch count is structural, not complexity
def describe(node: Trigger | Action) -> str:  # noqa: PLR0911, PLR0912
    """Render a trigger or action as a compact one-line label suitable for logging.

    Returns:
        Label encoding the kind and key discriminating fields, for example
        ``watch[3 paths @ 1600ms]`` or ``debounce[rail[core:run] @ 500ms]``.
    """
    match node:
        case Watch(paths=p, debounce=d):
            return f"watch[{len(p)} paths @ {d}ms]"
        case Schedule(cron=spec, timezone=tz):
            return f"schedule[{spec}{' @ ' + tz if tz else ''}]"
        case Manual():
            return "manual"
        case Rail(claim=c, verb=v):
            return f"rail[{c.value}:{v}]"
        case Program(argv=a):
            return f"program[{a[0] if a else ''}]"
        case Sequence(actions=acts):
            return "seq[" + " > ".join(describe(a) for a in acts) + "]"
        case Debounce(action=inner, window_ms=w):
            return f"debounce[{describe(inner)} @ {w}ms]"


def encode(node: Trigger | Action) -> bytes:
    """Encode a trigger or action to JSON bytes.

    Returns:
        JSON bytes with deterministic key ordering, suitable for hashing or comparison.
    """
    return _ENCODE.encode(node)


def decode(blob: bytes, *, trigger: bool) -> Trigger | Action:
    """Decode JSON bytes to a trigger or action.

    Args:
        blob: Raw JSON bytes from the wire.
        trigger: When True, routes to TRIGGER_DECODER; when False, routes to ACTION_DECODER.

    Returns:
        Decoded instance from the appropriate closed union.
    """
    return TRIGGER_DECODER.decode(blob) if trigger else ACTION_DECODER.decode(blob)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "ACTION_DECODER",
    "Action",
    "Debounce",
    "Manual",
    "Program",
    "Rail",
    "Schedule",
    "Sequence",
    "TRIGGER_DECODER",
    "Trigger",
    "Watch",
    "WatchFilter",
    "decode",
    "describe",
    "encode",
]
