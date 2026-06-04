"""The ``test`` rail: three thin folds over one Engine, distinguished only by ``Mode`` + ``Params``.

Owns ``Claim.TEST`` across three runner families (C# ``dotnet test`` MTP + Coverlet + ``dotnet-stryker``;
Python ``pytest`` + ``coverage`` + ``mutmut`` + ``pytest-benchmark``; TypeScript ``vitest`` only) as
adapters over the shared ``thin_rail`` — no executor logic lives here.

Invariants the rail does not re-implement: counts derive only in ``core/model.fold``, while
``killed``/``survived``/``coverage`` ride the ``TestRun`` detail via the catalog ``parse_tests`` Parser;
the ``mutation.lock`` lease is acquired inside the Engine. The mutation eligibility guard runs before
any ``Check`` is built so an overridden-target row never strands the global lane on a guaranteed-reject
lease acquire (busy-storm avoidance). The TypeScript mutation gap is structural: no ``vitest`` row
carries ``Mode.MUTATION``, so ``--mutation`` on a TS-only ``Routed`` folds to ``EMPTY`` (exit 0, valid
precondition but inapplicable) recorded by a logged parity note, not dropped silently.
"""

from dataclasses import dataclass
from typing import TYPE_CHECKING

# ``Ok``/``Error`` are factory functions, not match classes: match arms narrow on the tagged-union
# shape ``Result(tag="error", error=…)``, never ``Error(x)`` as a class pattern.
from expression import Error, Ok, Result
import structlog

from tools.assay._TMP.composition.catalog import parse_tests, select  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay._TMP.core.engine import fan_out, leased  # noqa: PLC2701  # intra-staging import; _TMP is the package root
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
from tools.assay._TMP.core.routing import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    route,
    Routed,  # noqa: TC001  # unconditional so beartype @checked resolves _weave's ``routed: Routed`` annotation under PEP 649
)


if TYPE_CHECKING:
    from pathlib import Path

    from tools.assay._TMP.core.model import AnyDetail, Completed, Tool  # intra-staging import; _TMP is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """The ``Claim.TEST`` params leaf: ``BaseParams{paths, language}`` plus eight test fields.

    Frozen and Cyclopts ``Parameter(name="*")``-flattened. ``target`` overrides the default test
    project/closure; ``all`` widens to the whole ``.slnx``/workspace; ``no_build`` reuses warm binaries;
    ``mutation`` selects Stryker (C#)/``mutmut`` (Py) and is unsupported on TS; ``benchmark`` selects
    BenchmarkDotNet/``pytest-benchmark``; ``coverage`` selects Coverlet/``coverage.py``; ``fixtures``
    surfaces the ``pytest --dead-fixtures``/``--dup-fixtures`` audit in ``list`` mode; ``filter`` is the
    MTP filter expr / ``pytest -k`` / ``vitest -t``.
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

_GAP_NOTE: str = "mutation requested but no eligible lane (typescript has no mutation runner; or target/all overrides the default)"
_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.test")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _eligible(tool: Tool, params: TestParams) -> bool:
    """Pre-fan-out eligibility gate: keep ``tool`` for one verb's fan-out.

    ``Mode.MUTATION`` survives only when ``--mutation`` was requested AND no ``target``/``all`` override
    displaced the default project. ``Mode.RESTORE``/``Mode.BUILD`` drop under ``--no-build``. Optional
    ``Mode.RUN`` tails (``coverage``/``pytest-benchmark``) drop unless their flag is set, so plain ``run``
    fans out only the base ``pytest``/``dotnet test``/``vitest`` row.

    Gating before ``Check`` construction is load-bearing: a guaranteed-reject mutation row never reaches
    ``fan_out`` and so never acquires the ``mutation.lock`` lease, averting the busy-storm under
    concurrent agents. ``params.filter`` argv-tail routing is a deferred ``place``/catalog change, not a
    rail gate, so it is intentionally unwired here.
    """
    match (tool.mode, params.mutation):
        case (Mode.MUTATION, False):
            return False
        case (Mode.MUTATION, True):
            return params.target is None and not params.all
        case (Mode.RESTORE, _) | (Mode.BUILD, _):
            return not params.no_build
        case _:
            return (tool.name != "coverage" or params.coverage) and (tool.name != "pytest-benchmark" or params.benchmark)


def _rows(claim: Claim, params: TestParams, mode: Mode) -> tuple[Tool, ...]:
    """The eligible catalog slice for one verb: verb-``mode`` rows plus the restore/build/mutation prelude, ``_eligible``-gated."""
    return tuple(
        t for t in select(claim, params.language) if (t.mode is mode or t.mode in {Mode.RESTORE, Mode.BUILD, Mode.MUTATION}) and _eligible(t, params)
    )


def _mutation_gap(params: TestParams, rows: tuple[Tool, ...]) -> bool:
    """True when ``--mutation`` was requested yet no surviving row carries ``Mode.MUTATION``.

    The TS-no-runner / overridden-target gap: the precondition is valid but inapplicable, so the verb
    folds to ``EMPTY`` (exit 0), not ``FAULTED``, and the caller logs a parity note.
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
    structurally inapplicable, and the ``fan_out`` + collapse + fold weave runs under one event loop.
    When the eligible slice carries any ``Mode.MUTATION`` row the whole weave is held under the
    global-exclusive ``mutation.lock`` lease (``leased('mutation', …, mode='exclusive')``): a busy lock
    short-circuits to ``Fault(BUSY)`` (exit 5) without spawning, otherwise the work runs once under the
    lease and releases on exit. ``_completeds`` short-circuits on the first ``Fault`` slot, and ``fold``
    derives counts and status onto one ``Report``. ``thin_rail`` returns a plain ``Result`` and so is
    never ``@effect.result``-wrapped (that decorator wraps generators only).
    """
    rows = _rows(claim, params, mode)
    match _mutation_gap(params, rows):
        case True:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language)
        case False:
            pass

    def _weave(routed: Routed) -> Result[Report, Fault]:
        def _work(_held: object = None) -> Result[Report, Fault]:
            return _completeds(fan_out(_checks(rows, routed), settings=settings, scope=scope, routed=routed)).map(
                lambda done: fold(claim, verb, done, detail=_detail(done, params))
            )

        match any(t.mode is Mode.MUTATION for t in rows):
            case True:
                return leased("mutation", _work, settings=settings, run_id=settings.run_id, project="mutation", mode="exclusive")
            case False:
                return _work()

    return route(_route_language(params.language), params.paths).bind(_weave)


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
