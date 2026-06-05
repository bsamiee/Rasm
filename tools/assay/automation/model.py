"""Define automation trigger and action wire shapes."""

from msgspec import json, Raw

from tools.assay.core.model import Base, Claim  # noqa: TC001  # tools.assay package; Claim is a runtime msgspec field type


# --- [MODELS] ---------------------------------------------------------------------------


class Watch(Base, frozen=True, tag_field="kind", tag="watch", forbid_unknown_fields=True):
    """Filesystem trigger interpreted by the watch backend.

    Attributes:
        paths: Files or directories to watch.
        filter: Watch filter name resolved by the engine.
        ignore_patterns: Glob patterns excluded from watch events.
        debounce: Watch debounce window in milliseconds.
        cpu_threshold: Optional CPU ceiling that skips a fire when reached.
    """

    paths: tuple[str, ...]
    filter: str = "default"
    ignore_patterns: tuple[str, ...] = ()
    debounce: int = 1600
    cpu_threshold: float | None = None


class Schedule(Base, frozen=True, tag_field="kind", tag="schedule", forbid_unknown_fields=True):
    """Cron trigger interpreted by the scheduler backend.

    Attributes:
        cron: Cron expression passed to the scheduler.
        cpu_threshold: Optional CPU ceiling that skips a fire when reached.
    """

    cron: str
    cpu_threshold: float | None = None


class Manual(Base, frozen=True, tag_field="kind", tag="manual", forbid_unknown_fields=True):
    """Immediate trigger that fires once."""


class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    """Rail action decoded against the registry at fire time.

    Attributes:
        claim: Claim that owns the rail.
        verb: Verb within the claim.
        params: Raw JSON payload decoded against the bound params type.
    """

    claim: Claim
    verb: str
    params: Raw = Raw()


class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    """Direct program action.

    Attributes:
        argv: Executable and arguments to run verbatim.
    """

    argv: tuple[str, ...]


class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    """Ordered action sequence.

    Attributes:
        actions: Actions evaluated in order by the engine.
    """

    actions: tuple["Action", ...]  # noqa: UP037  # forward-ref string is load-bearing: the Action alias is declared below, __future__ annotations is forbidden here, and msgspec evals this annotation at class creation


class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    """Action wrapper that coalesces trigger storms.

    Attributes:
        action: Wrapped action to fire.
        window_ms: Quiescence window in milliseconds.
        collapse: Whether to keep only the trailing fire.
    """

    action: "Action"  # noqa: UP037  # forward-ref string is load-bearing: the Action alias is declared below, __future__ annotations is forbidden here, and msgspec evals this annotation at class creation
    window_ms: int = 500
    collapse: bool = True


# --- [TYPES] ----------------------------------------------------------------------------


type Trigger = Watch | Schedule | Manual
type Action = Rail | Program | Sequence | Debounce


# --- [CONSTANTS] ------------------------------------------------------------------------


_DECODE_TRIGGER: json.Decoder[Trigger] = json.Decoder(Trigger)
_DECODE_ACTION: json.Decoder[Action] = json.Decoder(Action)  # instantiated after the Action alias so no forward-ref string reaches msgspec early
_ENCODE = json.Encoder(order="deterministic")  # deterministic order keeps the wire content-addressable

# exhaustiveness discipline: each frozenset holds every case's short tag, in lockstep with its union so a drifting case is a static set/union mismatch
_TRIGGER_TAGS: frozenset[str] = frozenset({"watch", "schedule", "manual"})
_ACTION_TAGS: frozenset[str] = frozenset({"rail", "program", "sequence", "debounce"})


# --- [OPERATIONS] -----------------------------------------------------------------------


def describe(node: Trigger | Action) -> str:  # noqa: PLR0911, PLR0912  # one return/arm per union case is the canonical total projection; counts scale with the union, not with branching complexity
    """Render a trigger or action as a one-line label.

    Args:
        node: Trigger or action to describe.

    Returns:
        Human-readable label.
    """
    match node:
        case Watch(paths=p, debounce=d):
            return f"watch[{len(p)} paths @ {d}ms]"
        case Schedule(cron=spec):
            return f"schedule[{spec}]"
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

    Args:
        blob: JSON payload to decode.
        trigger: Whether to decode the payload as a trigger instead of an action.

    Returns:
        Decoded trigger or action.

    """
    match trigger:
        case True:
            return _DECODE_TRIGGER.decode(blob)
        case False:
            return _DECODE_ACTION.decode(blob)


def encode(node: Trigger | Action) -> bytes:
    """Encode a trigger or action to deterministic JSON.

    Args:
        node: Trigger or action to encode.

    Returns:
        Encoded JSON bytes.
    """
    return _ENCODE.encode(node)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Action", "Debounce", "Manual", "Program", "Rail", "Schedule", "Sequence", "Trigger", "Watch", "decode", "describe", "encode"]
