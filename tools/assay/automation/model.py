"""Define automation trigger and action wire models."""

from typing import Annotated

from msgspec import json, Meta, Raw

from tools.assay.core.model import Base, Claim  # noqa: TC001  # tools.assay package; Claim is a runtime msgspec field type


# --- [MODELS] ---------------------------------------------------------------------------


class Watch(Base, frozen=True, tag_field="kind", tag="watch", forbid_unknown_fields=True):
    """Filesystem trigger interpreted by `watchfiles`.

    Attributes:
        paths: Files or directories to watch.
        filter: Engine-resolved watch filter tag.
        ignore_patterns: Glob patterns excluded from watch events.
        debounce: Watch debounce window in milliseconds.
        cpu_threshold: CPU ceiling that skips a fire when reached.
    """

    paths: tuple[str, ...]
    filter: str = "default"
    ignore_patterns: tuple[str, ...] = ()
    debounce: Annotated[int, Meta(ge=1)] = 1600
    step: Annotated[int, Meta(ge=1)] = 50
    force_polling: bool | None = None
    poll_delay_ms: Annotated[int, Meta(ge=1)] = 300
    recursive: bool = True
    ignore_permission_denied: bool | None = None
    cpu_threshold: Annotated[float, Meta(ge=0, le=1)] | None = None


class Schedule(Base, frozen=True, tag_field="kind", tag="schedule", forbid_unknown_fields=True):
    """Cron trigger interpreted by `aiocron`.

    Attributes:
        cron: Cron expression passed to the scheduler.
        cpu_threshold: CPU ceiling that skips a fire when reached.
    """

    cron: str
    timezone: str = "UTC"
    cpu_threshold: Annotated[float, Meta(ge=0, le=1)] | None = None


class Manual(Base, frozen=True, tag_field="kind", tag="manual", forbid_unknown_fields=True):
    """Immediate trigger that fires once."""


class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    """Rail action decoded against the registry when it fires.

    Attributes:
        claim: Claim that owns the rail.
        verb: Verb within the claim.
        params: Raw JSON payload decoded against the bound params type.
    """

    claim: Claim
    verb: str
    params: Raw = Raw()


class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    """Direct executable and arguments to run verbatim."""

    argv: Annotated[tuple[str, ...], Meta(min_length=1)]


class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    """Ordered actions evaluated by the engine."""

    actions: tuple["Action", ...]  # noqa: UP037  # forward-ref string is required: the Action alias is declared below, __future__ annotations is forbidden here, and msgspec evals this annotation at class creation


class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    """Action wrapper that coalesces trigger storms.

    Attributes:
        action: Wrapped action to fire.
        window_ms: Quiescence window in milliseconds.
        collapse: Whether to keep only the trailing fire.
    """

    action: "Action"  # noqa: UP037  # forward-ref string is required: the Action alias is declared below, __future__ annotations is forbidden here, and msgspec evals this annotation at class creation
    window_ms: int = 500
    collapse: bool = True


type Trigger = Watch | Schedule | Manual
type Action = Rail | Program | Sequence | Debounce


# --- [TABLES] ---------------------------------------------------------------------------


_DECODE_TRIGGER: json.Decoder[Trigger] = json.Decoder(Trigger)
_DECODE_ACTION: json.Decoder[Action] = json.Decoder(Action)  # built after Action so msgspec sees the final union
_ENCODE = json.Encoder(order="deterministic")

# --- [OPERATIONS] -----------------------------------------------------------------------


def describe(node: Trigger | Action) -> str:  # noqa: PLR0911, PLR0912  # one return/arm per union case is the canonical total projection; counts scale with the union, not with branching complexity
    """Render a trigger or action as a one-line label.

    Returns:
        Human-readable label for the trigger or action.
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


def decode(blob: bytes, *, trigger: bool) -> Trigger | Action:
    """Decode one trigger or action JSON blob.

    Returns:
        Decoded trigger or action union member.
    """
    match trigger:
        case True:
            return _DECODE_TRIGGER.decode(blob)
        case False:
            return _DECODE_ACTION.decode(blob)


def encode(node: Trigger | Action) -> bytes:
    """Encode a trigger or action to deterministic JSON.

    Returns:
        Deterministic JSON bytes for the wire model.
    """
    return _ENCODE.encode(node)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Action", "Debounce", "Manual", "Program", "Rail", "Schedule", "Sequence", "Trigger", "Watch", "decode", "describe", "encode"]
