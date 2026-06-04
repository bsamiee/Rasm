"""Automation arm shapes: two ``msgspec``-tagged unions — ``Trigger`` (what re-fires) and ``Action`` (what each fire runs).

Automation is a first-class arm, **not** a ``Claim``: these shapes sit outside the six
quality claims and the ``Detail`` tagged base, yet share the canonical ``Base`` policy and
the status algebra from ``core/model.py`` — automation never redefines either. They are
inert data decoded once at the loop boundary; ``automation/engine.py`` interprets them under
one ``anyio`` task group, emitting one ``Envelope`` per fire (the documented exception to the
one-Envelope invariant).

The model imports no trigger backend (``watchfiles``/``aiocron``), no governor (``psutil``),
and no per-verb ``Params`` dataclass: each backend choice stays in the interpreter and only
the inert wire shape lives here. ``Watch.filter`` is a vocabulary string the interpreter
resolves to a ``BaseFilter``, extended by ``Watch.ignore_patterns`` (inert rejection globs the
interpreter folds into that filter); ``Schedule.cron`` is a parser-agnostic spec string;
``cpu_threshold`` is a bare float; ``Rail.params`` is zero-copy ``msgspec.Raw`` so the
registry decodes it late against ``Bind.params``; ``Debounce`` wraps a single inner ``Action``
so a trigger storm coalesces to one delayed fire without a parallel execution engine.
"""

from msgspec import json, Raw

from tools.assay._TMP.core.model import Base, Claim  # noqa: PLC2701, TC001  # _TMP staging root; Claim is a runtime msgspec field type


# --- [MODELS] ---------------------------------------------------------------------------


class Watch(Base, frozen=True, tag_field="kind", tag="watch", forbid_unknown_fields=True):
    """Filesystem-driven re-fire over the ``watchfiles.awatch`` seam.

    ``filter`` is a vocabulary tag (``"default"`` -> ``DefaultFilter()``, ``"python"`` ->
    ``PythonFilter()``) the interpreter resolves; the wire stays a string so no ``watchfiles``
    type leaks onto it. ``ignore_patterns`` extends that base vocabulary tag with domain-specific
    rejection globs (vendor dirs, build artifacts) the interpreter folds into the resolved
    ``BaseFilter`` — inert data on the wire, no ``watchfiles`` subclass leaks. ``debounce`` is
    the Rust-layer batching window in ms. ``cpu_threshold`` is the optional fleet-governor gate:
    when set and ``psutil.cpu_percent`` >= it at the fire boundary, ``engine`` emits
    ``Completed{SKIP}`` and elides the fire; ``None`` disables it.
    """

    paths: tuple[str, ...]
    filter: str = "default"
    ignore_patterns: tuple[str, ...] = ()
    debounce: int = 1600
    cpu_threshold: float | None = None


class Schedule(Base, frozen=True, tag_field="kind", tag="schedule", forbid_unknown_fields=True):
    """Cron-driven re-fire over the ``aiocron.crontab`` seam, spec parsed by the interpreter.

    ``cron`` is a parser-agnostic spec string (extended 6-field accepted). ``cpu_threshold``
    mirrors ``Watch``: the same inert governor gate, never stored as an engine knob.
    """

    cron: str
    cpu_threshold: float | None = None


class Manual(Base, frozen=True, tag_field="kind", tag="manual", forbid_unknown_fields=True):
    """Immediate single fire: no re-fire machinery, no governor — the degenerate trigger."""


class Rail(Base, frozen=True, tag_field="kind", tag="rail", forbid_unknown_fields=True):
    """A quality-rail fire over the ``composition/registry.rail`` seam.

    ``params`` is zero-copy ``msgspec.Raw`` so the action carries an opaque per-verb payload
    without this module importing every rail's frozen ``Params`` dataclass; the registry
    decodes it at dispatch against ``Bind.params``. The asymmetry is deliberate: an invalid
    ``params`` payload surfaces as a ``FAULTED`` ``Fault`` at the fire boundary, not at
    trigger-config decode.
    """

    claim: Claim
    verb: str
    params: Raw = Raw()


class Program(Base, frozen=True, tag_field="kind", tag="program", forbid_unknown_fields=True):
    """A direct program fire: ``argv`` binds to a DIRECT-runner ``Check`` via ``run_check``."""

    argv: tuple[str, ...]


class Sequence(Base, frozen=True, tag_field="kind", tag="sequence", forbid_unknown_fields=True):
    """A recursive action fold: the third case as one row, nesting ``tuple[Action, ...]``.

    The short-circuit policy is **fixed and owned by ``engine``**, never the data — the
    recursive ``match`` over the tuple is the fold vehicle and the data stays inert. The
    annotation is a string forward-reference because the ``Action`` alias is declared below
    this class; msgspec resolves it lazily at ``Decoder`` construction, after the alias exists.
    """

    actions: tuple["Action", ...]  # noqa: UP037  # forward-ref string is load-bearing: the Action alias is declared below, __future__ annotations is forbidden here, and msgspec evals this annotation at class creation


class Debounce(Base, frozen=True, tag_field="kind", tag="debounce", forbid_unknown_fields=True):
    """A coalescing wrapper that throttles a fast-trigger storm down to one delayed fire.

    ``action`` is the WRAPPED ``Action`` the engine runs once the storm settles — ``Debounce``
    adds no execution engine of its own, it only schedules the inner action's existing seam.
    ``window_ms`` is the quiescence window: the engine resets a per-action timer on each trigger
    and fires ``action`` only after ``window_ms`` elapses with no new trigger. ``collapse=True``
    drops every coalesced trigger to a single trailing fire (storm -> one run); ``collapse=False``
    keeps the leading fire and suppresses only the trailing tail. The recursion is a string
    forward-ref for the same reason as ``Sequence.actions``: the ``Action`` alias is declared below.
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
    """Render a trigger or action as a one-line human label.

    One polymorphic entry point: a single ``match`` folds every ``Trigger`` and ``Action``
    case and recurses over ``Sequence.actions``, so nesting is one ``match``, not a hierarchy.
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
    """Recover a ``Trigger`` (``trigger=True``) or ``Action`` from one JSON blob in a single pass.

    The explicit short ``kind`` tag recovers the case and ``forbid_unknown_fields`` makes a
    drifting emitter fail loud at the decode boundary rather than silently dropping fields.
    """
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
