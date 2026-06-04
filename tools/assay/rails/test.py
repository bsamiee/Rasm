"""The ``test`` rail: three thin folds over one Engine, distinguished only by ``Mode`` + ``Params``.

Owns ``Claim.TEST`` across three runner families (C# ``dotnet test`` MTP + Coverlet + ``dotnet-stryker``;
Python ``pytest`` + ``coverage`` + ``mutmut`` + ``pytest-benchmark``; TypeScript ``vitest`` only) as
adapters over the shared ``thin_rail`` — no executor logic lives here. Polyglot like ``rails/static.py``:
``language=None`` fans every ``Claim.TEST`` language through its OWN ``route`` (``_routed``), so each
language's eligible rows bind to that language's routed files — never a single shared route that would
strand the C#/TS rows on an empty Python file set.

Invariants the rail does not re-implement: counts derive only in ``core/model.fold``, while
``killed``/``survived``/``coverage`` ride the ``TestRun`` detail via the catalog ``parse_tests`` Parser;
the ``mutation.lock`` lease is acquired inside the Engine. The mutation eligibility guard runs before
any ``Check`` is built so an overridden-target row never strands the global lane on a guaranteed-reject
lease acquire (busy-storm avoidance). The TypeScript mutation gap is structural: no ``vitest`` row
carries ``Mode.MUTATION``, so ``--mutation`` on a TS-only request folds to ``EMPTY`` (exit 0, valid
precondition but inapplicable) recorded by a logged parity note, not dropped silently.
"""

from dataclasses import dataclass
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import structlog

from tools.assay.composition.catalog import parse_tests, select  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out, leased  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    fold,
    Mode,
    MutationLane,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    TestRun,
)
from tools.assay.core.routing import route  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from pathlib import Path

    from expression.collections import Block

    from tools.assay.core.model import AnyDetail, Completed, Language, Tool  # intra-package import; tools.assay is the package root
    from tools.assay.core.routing import Routed  # intra-package import; tools.assay is the package root


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


def _languages(selected: Language | None) -> tuple[Language, ...]:
    """The languages to fan: one explicit member, or every language the ``Claim.TEST`` catalog rows back.

    Derived from ``select(Claim.TEST)`` (mirrors ``code._languages``) so the supported set is data-driven —
    adding a test runner's rows auto-extends the fan, and the polyglot request never routes a language
    that carries no ``test`` row and would only waste a route.
    """
    match selected:
        case None:
            return tuple(sorted({t.language for t in select(Claim.TEST)}, key=lambda member: member.value))
        case language:
            return (language,)


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


def _rows(language: Language, params: TestParams, mode: Mode) -> tuple[Tool, ...]:
    """The eligible ``Claim.TEST`` slice for one language+verb: verb-``mode`` rows plus the restore/build/mutation prelude, ``_eligible``-gated."""
    return tuple(
        t for t in select(Claim.TEST, language) if (t.mode is mode or t.mode in {Mode.RESTORE, Mode.BUILD, Mode.MUTATION}) and _eligible(t, params)
    )


def _mutation_gap(params: TestParams, rows: tuple[Tool, ...]) -> bool:
    """True when ``--mutation`` was requested yet no surviving row carries ``Mode.MUTATION``.

    The TS-no-runner / overridden-target gap: the precondition is valid but inapplicable, so the verb
    folds to ``EMPTY`` (exit 0), not ``FAULTED``, and the caller logs a parity note.
    """
    return params.mutation and not any(t.mode is Mode.MUTATION for t in rows)


def _checks(routed: Routed, params: TestParams, mode: Mode) -> tuple[Check, ...]:
    """Bind each eligible ``Tool`` row of the routed language to the routed scope: ``routed.files`` flow as ``Check.paths``.

    A pure projection — no scope is embedded in the ``Check``; ``routed``/``scope``/``deadline`` reach
    the executor as ``fan_out`` parameters, never as ``Check`` fields.
    """
    return tuple(Check(tool=t, paths=routed.files) for t in _rows(routed.language, params, mode))


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    """Route every requested language through one ``route`` each, sequencing the per-language rail (mirrors ``static._routed``).

    ``route`` resolves one ``Language`` per call, so the polyglot request fans each member through its own
    ``route`` and ``sequence`` collapses the ``Block[Result[Routed, Fault]]`` into one ``Result[Block[Routed],
    Fault]``: the first routing ``Fault`` (a git/``fd`` spawn failure at the ``LOCAL`` boundary) short-circuits the fan.
    """
    return sequence(block.of_seq(route(language, paths) for language in languages))


def _dispatch(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    """Fan one routed language's eligible rows through the Engine under its OWN ``Routed`` context (mirrors ``static._dispatch``).

    Each language's argv tail is projected from its own ``Routed`` — the per-slot ``Result``s are returned
    flat so the caller concatenates every language's outcomes into one fold; a language with no eligible
    ``mode``-matching rows produces an empty fan and contributes nothing rather than a phantom ``EMPTY`` slot.
    """
    checks = _checks(routed, params, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


def _detail(done: tuple[Completed, ...], params: TestParams) -> AnyDetail | None:
    """Project the mutation/coverage evidence onto a ``TestRun`` detail via the catalog ``parse_tests`` Parser.

    ``killed``/``survived``/``coverage`` are Parser-derived, never fold-derived — conflating mutant
    kill-ratio into ``Counts`` would re-introduce the per-rail count struct the design retired. On a
    non-mutation run the detail is ``None``. The subtlety on a mutation run: ``parse_tests`` decodes
    every receipt to a defaulted ``TestRun`` (an empty stdout yields ``TestRun(mutation=off)``), so the
    verb's evidence is the FIRST decode that actually carries a mutation lane (``mutation != off``) —
    otherwise a barren ``pytest``/``dotnet test`` receipt would mask the Stryker/``mutmut`` row that ran.
    """
    match params.mutation:
        case False:
            return None
        case True:
            return next((d for c in done if (d := parse_tests(c)) is not None and _is_mutation(d)), None)


def _is_mutation(detail: AnyDetail) -> bool:
    """Discriminate a real mutation ``TestRun`` decode from a defaulted/off one.

    Narrows the ``AnyDetail`` union to a ``TestRun`` whose ``mutation`` lane names an actual runner
    (``MutationLane.STRYKER``/``MUTMUT``), rejecting the ``OFF`` default a barren ``pytest``/``dotnet
    test`` receipt decodes to, and any non-``TestRun`` variant.
    """
    match detail:
        case TestRun(mutation=lane) if lane is not MutationLane.OFF:
            return True
        case _:
            return False


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """The one fold every ``test`` verb shares: gate, route per language, fan out per language, fold.

    NOT a ``Handler`` — the per-verb adapters close over ``(verb, mode)`` and are the bound ``Handler``s.
    The flow: the eligible ``Claim.TEST`` slice is computed per language and mutation-gated before any
    ``Check`` is built (the gap parity note is logged when a requested mutation lane was structurally
    inapplicable); then each language routes through its own ``route`` (``_routed``) and its eligible rows
    fan out under that language's ``Routed`` (``_dispatch``), so a polyglot request binds the C#/TS rows to
    the C#/TS file sets — never a single shared Python route. ``sequence`` collapses the per-language route
    rail and per-slot ``fan_out`` rail into one ``Result``; a routing/spawn/timeout ``Fault`` short-circuits
    to the registry seam while a non-zero exit already rode the success channel as a ``Completed``. When any
    eligible row carries ``Mode.MUTATION`` the whole multi-language weave runs under the single
    global-exclusive ``mutation.lock`` lease (``leased('mutation', …, mode='exclusive')``): a busy lock
    short-circuits to ``Fault(BUSY)`` (exit 5) without spawning. ``fold`` derives counts/status onto one
    ``Report`` carrying the ``_detail`` evidence. ``thin_rail`` returns a plain ``Result`` (never yields),
    so it is never ``@effect.result``-wrapped.
    """
    languages = _languages(params.language)
    eligible = tuple(t for language in languages for t in _rows(language, params, mode))
    match _mutation_gap(params, eligible):
        case True:
            _LOG.warning(_GAP_NOTE, claim=claim.value, verb=verb, language=params.language)
        case False:
            pass

    def _work(_held: object = None) -> Result[Report, Fault]:
        def _settle(done: Block[Completed]) -> Report:
            outcomes = tuple(done)
            return fold(claim, verb, outcomes, detail=_detail(outcomes, params))

        return _routed(languages, params.paths).bind(
            lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, params, settings=settings, scope=scope, mode=mode)))).map(
                _settle
            )
        )

    match any(t.mode is Mode.MUTATION for t in eligible):
        case True:
            return leased("mutation", _work, settings=settings, run_id=settings.run_id, project="mutation", mode="exclusive")
        case False:
            return _work()


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
