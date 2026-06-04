"""The ``test`` rail: three thin folds over one Engine, distinguished only by ``Mode`` + ``Params``.

Owns ``Claim.TEST`` across the three orchestrated runner families (C# ``dotnet test`` MTP + Coverlet
+ ``dotnet-stryker``; Python ``pytest`` + ``coverage`` + ``mutmut`` + ``pytest-benchmark``; TypeScript
``vitest`` only). It owns no executor logic: every verb is one adapter over the shared ``thin_rail``.

Two invariants the rail does not re-implement: counts derive only in ``core/model.fold``, while
``killed``/``survived``/``coverage`` ride the ``TestRun`` detail via the catalog ``parse_tests``
Parser; and the ``mutation.lock`` global-exclusive lease is acquired inside the Engine, not here.
The mutation eligibility guard runs before any ``Check`` is built, so a row whose target was
overridden never strands the global lane on a guaranteed-reject lease acquire (busy-storm avoidance).
The TypeScript mutation gap is structural, not a TODO: ``vitest`` is excluded by ``_NO_MUTATION`` and
a ``--mutation`` request on a TS-only ``Routed`` folds to ``EMPTY`` (exit 0 — the precondition is
valid but inapplicable), recorded by a logged parity note rather than dropped silently.
"""

from dataclasses import dataclass
from typing import TYPE_CHECKING

# ``Ok``/``Error`` are factory functions, not match classes: match arms narrow on the tagged-union
# shape ``Result(tag="error", error=…)``, never ``Error(x)`` as a class pattern.
from expression import Error, Ok, Result
import structlog

from tools.assay._TMP.composition.catalog import parse_tests, select  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.engine import fan_out  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.model import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    TestRun,
)
from tools.assay._TMP.core.routing import route  # noqa: PLC2701  # intra-staging import; _TMP is the package root


if TYPE_CHECKING:
    from pathlib import Path

    from tools.assay._TMP.composition.settings import ArtifactScope, AssaySettings  # intra-staging import; _TMP is the package root
    from tools.assay._TMP.core.model import AnyDetail, Completed, Tool  # intra-staging import; _TMP is the package root
    from tools.assay._TMP.core.routing import Routed  # intra-staging import; _TMP is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """The thin ``Claim.TEST`` params leaf: ``BaseParams{paths, language}`` plus eight test fields.

    Frozen and Cyclopts ``Parameter(name="*")``-flattened. The eight are fields, never verbs: ``target``
    overrides the default test project/closure; ``all`` widens to the whole ``.slnx``/workspace;
    ``no_build`` reuses warm binaries; ``mutation`` selects Stryker (C#)/``mutmut`` (Py) and is
    unsupported on TS; ``benchmark`` selects BenchmarkDotNet/``pytest-benchmark``; ``coverage`` selects
    Coverlet/``coverage.py``; ``fixtures`` surfaces the ``pytest --dead-fixtures``/``--dup-fixtures``
    audit in ``list`` mode; ``filter`` is the MTP filter expr / ``pytest -k`` / ``vitest -t``.
    """

    target: Path | None = None
    all: bool = False
    no_build: bool = False
    mutation: bool = False
    benchmark: bool = False
    coverage: bool = False
    fixtures: bool = False
    filter: str = ""


# --- [CONSTANTS] ------------------------------------------------------------------------

_NO_MUTATION: frozenset[str] = frozenset({"vitest"})  # the TS mutation gap, logged not dropped
_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner; or target/all overrides the default)"
_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.test")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _eligible(tool: Tool, params: TestParams) -> bool:
    """The pre-fan-out eligibility gate: mutation rows survive only on the standing default target.

    A ``Mode.MUTATION`` row is dropped unless ``--mutation`` was requested AND no ``target``/``all``
    override displaced the default project AND the runner supports mutation; ``Mode.RESTORE``/
    ``Mode.BUILD`` rows drop under ``--no-build``. Running before ``Check`` construction is the point:
    a guaranteed-reject mutation row never reaches ``fan_out`` and so never acquires the global
    ``mutation.lock`` lease, which is what averts the busy-storm under concurrent agents.
    """
    match (tool.mode, params.mutation):
        case (Mode.MUTATION, False):
            return False
        case (Mode.MUTATION, True):
            return params.target is None and not params.all and tool.name not in _NO_MUTATION
        case (Mode.RESTORE, _) | (Mode.BUILD, _):
            return not params.no_build
        case _:
            return True


def _rows(claim: Claim, params: TestParams, mode: Mode) -> tuple[Tool, ...]:
    """The eligible catalog slice for one verb: verb-``mode`` rows plus the shared restore/build warm-up.

    The predicate keeps the verb's own ``mode`` plus the restore/build prelude rows, then applies the
    mutation/no-build eligibility gate, so the returned tuple is exactly the set ``_checks`` fans out.
    """
    return tuple(t for t in select(claim, params.language) if (t.mode is mode or t.mode in {Mode.RESTORE, Mode.BUILD}) and _eligible(t, params))


def _mutation_gap(params: TestParams, rows: tuple[Tool, ...]) -> bool:
    """Detect the structural TypeScript / overridden-target mutation gap: requested but no lane ran.

    True when ``--mutation`` was requested yet no surviving row carries ``Mode.MUTATION`` — the
    precondition is valid but inapplicable (TS has no mutation runner, or ``target``/``all`` displaced
    the default project), so the verb folds to ``EMPTY`` (exit 0), not ``FAULTED``. The caller logs the
    parity note; ``TestRun.mutation`` stays ``"off"`` recording that no mutation lane executed.
    """
    return params.mutation and not any(t.mode is Mode.MUTATION for t in rows)


def _checks(rows: tuple[Tool, ...], routed: Routed) -> tuple[Check, ...]:
    """Bind each eligible ``Tool`` row to the routed scope: ``routed.files`` flow as ``Check.paths``.

    A pure projection — no scope is embedded in the ``Check``; ``routed``/``scope``/``deadline`` reach
    the executor as ``fan_out`` parameters, never as ``Check`` fields.
    """
    return tuple(Check(tool=t, paths=routed.files) for t in rows)


def _detail(done: tuple[Completed, ...], params: TestParams) -> AnyDetail | None:
    """Project the mutation/coverage evidence onto a ``TestRun`` detail via the catalog ``parse_tests`` Parser.

    ``killed``/``survived``/``coverage`` are Parser-derived, never fold-derived — conflating mutant
    kill-ratio into ``Counts`` would re-introduce the per-rail count struct the design retired. On a
    non-mutation run the detail is ``None``. The subtlety on a mutation run: ``parse_tests`` decodes
    every receipt to a defaulted ``TestRun`` (an empty stdout yields ``TestRun(mutation="off")``), so
    the verb's evidence is the FIRST decode that actually carries a mutation lane (``mutation != "off"``)
    — otherwise a barren ``pytest``/``dotnet test`` receipt would mask the Stryker/``mutmut`` row that ran.
    """
    match params.mutation:
        case False:
            return None
        case True:
            return next((d for c in done if (d := parse_tests(c)) is not None and _is_mutation(d)), None)


def _is_mutation(detail: AnyDetail) -> bool:
    """Discriminate a real mutation ``TestRun`` decode from a defaulted/off one.

    Narrows the ``AnyDetail`` union to a ``TestRun`` whose ``mutation`` field names an actual lane
    (Stryker/``mutmut``), rejecting the ``"off"`` default a barren ``pytest``/``dotnet test`` receipt
    decodes to, and any non-``TestRun`` variant.
    """
    match detail:
        case TestRun(mutation=m) if m != "off":
            return True
        case _:
            return False


def _completeds(slots: tuple[Result[Completed, Fault], ...]) -> Result[tuple[Completed, ...], Fault]:
    """Collapse the per-slot ``fan_out`` results into one channel: any ``Fault`` slot dominates.

    A single error slot (``BUSY``/``TIMEOUT``/``FAULTED`` riding ``Envelope.error``) short-circuits the
    whole verb to its ``Fault``; otherwise every slot is on the success channel and the ``Completed``
    receipts flow to ``fold`` — note a non-zero process exit is still an ``Ok(Completed)``, not an error
    slot. The match narrows on ``Result(tag="error", error=…)``, never an ``Error(x)`` class pattern.
    An empty ``slots`` (the TS mutation gap routed zero checks) folds to ``Ok(())`` → ``EMPTY``.
    """
    match next((s for s in slots if s.is_error()), None):
        case Result(tag="error", error=fault):
            return Error(fault)
        case _:
            return Ok(tuple(s.ok for s in slots))


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """The one fold every ``test`` verb shares: route once, gate, fan out, collapse, fold.

    NOT a ``Handler`` — the per-verb adapters close over ``(verb, mode)`` and are the bound ``Handler``s.
    The flow: route runs exactly once (``Routed`` carries a singular ``language``, so a polyglot request
    still routes one shared file set), the eligible ``Claim.TEST`` slice is selected and mutation-gated
    before any ``Check`` is built, the gap parity note is logged when a requested mutation lane was
    structurally inapplicable, ``fan_out`` executes the checks under one event loop (acquiring
    ``mutation.lock`` for any ``Mode.MUTATION`` row), ``_completeds`` short-circuits on the first
    ``Fault`` slot, and ``fold`` derives counts and status onto one ``Report``. ``thin_rail`` returns a
    plain ``Result`` and so is never ``@effect.result``-wrapped (that decorator wraps generators only).
    """
    rows = _rows(claim, params, mode)
    match _mutation_gap(params, rows):
        case True:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language)
        case False:
            pass
    return route(_route_language(params.language), params.paths).bind(
        lambda routed: _completeds(fan_out(_checks(rows, routed), settings=settings, scope=scope, routed=routed)).map(
            lambda done: fold(claim, verb, done, detail=_detail(done, params))
        )
    )


def _route_language(language: Language | None) -> Language:
    """Resolve the single routing ``Language``: a concrete request routes verbatim; ``None`` defaults to ``PYTHON``.

    ``route`` is singular (one ``Language``, one ``Routed``), so a polyglot ``language=None`` request —
    which still ``select``s rows across every TEST language — must collapse to ONE routing language.
    ``Language.PYTHON`` carries the ``glob`` strategy, so ``routed.files`` is the suffix-projected
    change-set the fanned checks consume; per-language routing stays a routing concern, not a rail one.
    """
    match language:
        case None:
            return Language.PYTHON
        case _:
            return language


# --- [COMPOSITION] ----------------------------------------------------------------------


def run(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """``Handler``: execute the test suite (``Mode.RUN``) for the routed languages.

    The C# ``dotnet test`` MTP, Python ``pytest`` (plus the ``coverage``/``mutmut``/``pytest-benchmark``
    rows when their flags are set), and TS ``vitest`` rows fan out and fold into one ``Report``.
    """
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)


def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # noqa: A001  # the verb IS "list"; this is the registry-bound Handler name
    """``Handler``: enumerate tests (``Mode.LIST``) and surface the ``--dead-fixtures``/``--dup-fixtures`` audit.

    ``dotnet test --list-tests`` (``parse_tests`` → ``TestRun.selected``) and ``pytest --dead-fixtures``
    are the ``Mode.LIST`` rows; the fixtures audit surfaces when ``params.fixtures`` is set.
    """
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="list", mode=Mode.LIST)


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """``Handler``: run the suite under coverage (``Mode.RUN`` + the Coverlet/``coverage.py`` tail).

    Identical ``Mode.RUN`` fold as ``run`` with ``params.coverage`` flowing through the Coverlet (C#) /
    ``coverage.py`` (Py) row tail; coverage percentage rides ``TestRun.coverage`` via the Parser.
    """
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "list", "run", "thin_rail"]
