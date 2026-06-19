"""Wire models decoded by the automation engine.

Trigger and Action are closed msgspec unions with tagged decoding, frozen structs, and
unknown-field rejection at the wire boundary.
"""

from enum import StrEnum
from typing import Annotated, assert_never

from msgspec import json, Meta, Raw

from tools.assay.core.model import Base, Claim  # noqa: TC001  # runtime msgspec field; deferring breaks struct resolution


# --- [TYPES] ----------------------------------------------------------------------------


class WatchFilter(StrEnum):
    """Wire tag resolved to a watchfiles filter by the engine."""

    DEFAULT = "default"
    PYTHON = "python"


class Edge(StrEnum):
    """Debounce firing edge: leading fires the first event then drains; trailing fires the last after quiescence."""

    LEADING = "leading"
    TRAILING = "trailing"


# --- [MODELS] ---------------------------------------------------------------------------


class Watch(Base, frozen=True, tag_field="kind", tag="watch", forbid_unknown_fields=True):
    """Filesystem trigger interpreted by watchfiles.

    cpu_threshold suppresses a fire before the action pipeline when sampled CPU utilization
    meets or exceeds the fractional ceiling.
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
    """Cron trigger interpreted by APScheduler.

    cpu_threshold suppresses a scheduled fire before the action pipeline when sampled CPU
    utilization meets or exceeds the fractional ceiling.
    """

    cron: str
    timezone: str = "UTC"
    cpu_threshold: Annotated[float, Meta(ge=0, le=1)] | None = None


class Manual(Base, frozen=True, tag_field="kind", tag="manual", forbid_unknown_fields=True):
    """Immediate trigger that fires once."""


class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    """Rail action resolved against the registry at fire time.

    params remains raw until registry dispatch provides the bound params type, so unknown
    claim or verb pairs fail during fire execution rather than wire decode.
    """

    claim: Claim
    verb: str
    params: Raw = Raw()


class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    """Direct executable and arguments to run verbatim."""

    argv: Annotated[tuple[str, ...], Meta(min_length=1)]


class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    """Ordered actions evaluated by the engine."""

    actions: tuple["Action", ...]  # noqa: UP037  # forward ref; Action alias defined after Sequence


class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    """Action wrapper that coalesces trigger storms within a quiescence window.

    edge selects which event in the window fires (trailing keeps only the last); window_ms is
    the quiescence duration in milliseconds.
    """

    action: "Action"  # noqa: UP037  # forward ref; Action alias defined after Debounce
    window_ms: int = 500
    edge: Edge = Edge.TRAILING


type Trigger = Watch | Schedule | Manual
type Action = Rail | Program | Sequence | Debounce

# --- [TABLES] ---------------------------------------------------------------------------

TRIGGER_DECODER: json.Decoder[Trigger] = json.Decoder(Trigger)  # msgspec resolves union members eagerly at Decoder.__init__
ACTION_DECODER: json.Decoder[Action] = json.Decoder(Action)
_NODE_DECODER: json.Decoder[Trigger | Action] = json.Decoder(Trigger | Action)  # disjoint tags discriminate the combined union
_ENCODE = json.Encoder(order="deterministic")

# --- [OPERATIONS] -----------------------------------------------------------------------


def describe(node: Trigger | Action) -> str:  # noqa: PLR0911, PLR0912, PLR0914
    """Render a compact label for automation telemetry.

    Returns:
        Kind plus discriminating fields such as path count, cron spec, rail key, or
        debounce window.
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
        case never:
            assert_never(never)


def encode(node: Trigger | Action) -> bytes:
    """Encode a trigger or action as deterministic JSON.

    Returns:
        JSON bytes with stable key ordering for hashing or comparison.
    """
    return _ENCODE.encode(node)


def decode(blob: bytes) -> Trigger | Action:
    """Decode JSON bytes through the combined closed union, discriminating by the wire ``kind`` tag.

    Args:
        blob: Raw wire bytes.

    Returns:
        Decoded Trigger or Action instance selected by the encoded tag.
    """
    return _NODE_DECODER.decode(blob)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ACTION_DECODER",
    "Action",
    "Debounce",
    "Edge",
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
