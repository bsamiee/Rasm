"""Automation arm shapes: two ``msgspec``-tagged unions — ``Trigger`` (what re-fires) and ``Action`` (what each fire runs).

Inert data only — every backend choice (``watchfiles``/``aiocron``/``psutil``, the per-verb
``Params``) stays in ``engine.py``, which interprets these shapes under one ``anyio`` task group and
emits one ``Envelope`` per fire (the documented exception to the one-Envelope invariant).
"""

from msgspec import json, Raw

from tools.assay.core.model import Base, Claim  # noqa: TC001  # tools.assay package; Claim is a runtime msgspec field type


# --- [MODELS] ---------------------------------------------------------------------------


class Watch(Base, frozen=True, tag_field="kind", tag="watch", forbid_unknown_fields=True):
    """Filesystem-driven re-fire over the ``watchfiles.awatch`` seam.

    ``filter`` is a vocabulary tag the interpreter resolves to a ``BaseFilter`` (the wire stays a string
    so no ``watchfiles`` type leaks); ``ignore_patterns`` adds rejection globs folded into that filter.
    ``cpu_threshold`` is the optional governor gate — when ``psutil.cpu_percent`` meets it at the fire
    boundary the engine emits ``SKIP`` and elides the fire; ``None`` disables it.
    """

    paths: tuple[str, ...]
    filter: str = "default"
    ignore_patterns: tuple[str, ...] = ()
    debounce: int = 1600
    cpu_threshold: float | None = None


class Schedule(Base, frozen=True, tag_field="kind", tag="schedule", forbid_unknown_fields=True):
    """Cron-driven re-fire over the ``aiocron.crontab`` seam; ``cron`` is a parser-agnostic spec, ``cpu_threshold`` mirrors ``Watch``."""

    cron: str
    cpu_threshold: float | None = None


class Manual(Base, frozen=True, tag_field="kind", tag="manual", forbid_unknown_fields=True):
    """Immediate single fire: no re-fire machinery, no governor — the degenerate trigger."""


class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    """A quality-rail fire over the ``composition/registry.rail`` seam.

    ``params`` is zero-copy ``msgspec.Raw``: the registry decodes it late at dispatch against
    ``Bind.params``, so an invalid payload surfaces as a ``FAULTED`` ``Fault`` at the fire boundary,
    not at trigger-config decode.
    """

    claim: Claim
    verb: str
    params: Raw = Raw()


class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    """A direct program fire: ``argv`` binds to a DIRECT-runner ``Check`` via ``run_check``."""

    argv: tuple[str, ...]


class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    """A recursive action fold nesting ``tuple[Action, ...]``; the short-circuit policy is owned by ``engine``, never the data."""

    actions: tuple["Action", ...]  # noqa: UP037  # forward-ref string is load-bearing: the Action alias is declared below, __future__ annotations is forbidden here, and msgspec evals this annotation at class creation


class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    """A coalescing wrapper that throttles a trigger storm to one delayed ``action`` fire per ``window_ms`` quiescence window.

    ``collapse=True`` drops every coalesced trigger to a single trailing fire (storm -> one run);
    ``collapse=False`` keeps the leading fire and suppresses only the trailing tail.
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
    """Render a trigger or action as a one-line human label, recursing over ``Sequence.actions``."""
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
    """Recover a ``Trigger`` (``trigger=True``) or ``Action`` from one JSON blob, the ``kind`` tag selecting the case."""
    match trigger:
        case True:
            return _DECODE_TRIGGER.decode(blob)
        case False:
            return _DECODE_ACTION.decode(blob)


def encode(node: Trigger | Action) -> bytes:
    """Encode a trigger or action to deterministic-order JSON via the sole cached ``Encoder``."""
    return _ENCODE.encode(node)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Action", "Debounce", "Manual", "Program", "Rail", "Schedule", "Sequence", "Trigger", "Watch", "decode", "describe", "encode"]
