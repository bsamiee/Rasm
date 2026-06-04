"""The ``test`` rail: three thin folds over one Engine, distinguished only by ``Mode`` + ``Params``.

Polyglot over ``Claim.TEST`` (C#/Python/TS runner families): ``language=None`` fans each language through
its OWN ``route``, so eligible rows bind to that language's routed files. The mutation eligibility guard
runs before any ``Check`` is built so an overridden-target row never strands the global lane on a
guaranteed-reject lease acquire (busy-storm avoidance). The TS mutation gap is structural — no ``vitest``
row carries ``Mode.MUTATION``, so ``--mutation`` on a TS-only request folds to ``EMPTY`` with a parity note.
"""

from dataclasses import dataclass
from pathlib import Path  # noqa: TC003  # unconditional: cyclopts resolves TestParams.target's `Path | None` at CLI-build (PEP 649)
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
    from expression.collections import Block

    from tools.assay.core.model import AnyDetail, Completed, Language, Tool  # intra-package import; tools.assay is the package root
    from tools.assay.core.routing import Routed  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """The ``Claim.TEST`` params leaf: ``BaseParams{paths, language}`` plus eight test fields.

    ``target`` overrides the default test project; ``all`` widens to the whole workspace; ``no_build``
    reuses warm binaries; ``mutation`` selects Stryker/``mutmut`` (unsupported on TS); ``benchmark``,
    ``coverage``, and ``fixtures`` gate their respective runner tails; ``filter`` is the runner filter expr.
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
    # Data-driven off select(Claim.TEST): the polyglot fan never routes a language carrying no test row.
    match selected:
        case None:
            return tuple(sorted({t.language for t in select(Claim.TEST)}, key=lambda member: member.value))
        case language:
            return (language,)


def _eligible(tool: Tool, params: TestParams) -> bool:
    # Gating before Check construction is load-bearing: a guaranteed-reject mutation row never reaches fan_out and so
    # never acquires the mutation.lock lease, averting the busy-storm under concurrent agents.
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
    return tuple(
        t for t in select(Claim.TEST, language) if (t.mode is mode or t.mode in {Mode.RESTORE, Mode.BUILD, Mode.MUTATION}) and _eligible(t, params)
    )


def _mutation_gap(params: TestParams, rows: tuple[Tool, ...]) -> bool:
    # TS-no-runner / overridden-target gap: a valid-but-inapplicable precondition folds to EMPTY (exit 0), not FAULTED.
    return params.mutation and not any(t.mode is Mode.MUTATION for t in rows)


def _checks(routed: Routed, params: TestParams, mode: Mode) -> tuple[Check, ...]:
    # Pure projection: scope/deadline reach the executor as fan_out parameters, never as Check fields.
    return tuple(Check(tool=t, paths=routed.files) for t in _rows(routed.language, params, mode))


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    # route resolves one Language per call; the first routing Fault short-circuits the whole polyglot fan.
    return sequence(block.of_seq(route(language, paths) for language in languages))


def _dispatch(
    routed: Routed, params: TestParams, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode
) -> tuple[Result[Completed, Fault], ...]:
    # One fan_out per language so each argv tail is projected from its own Routed; an empty fan contributes nothing, not a phantom EMPTY.
    checks = _checks(routed, params, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


def _detail(done: tuple[Completed, ...], params: TestParams) -> AnyDetail | None:
    # killed/survived/coverage are Parser-derived, never fold-derived (conflating kill-ratio into Counts would re-introduce
    # the retired per-rail count struct). parse_tests decodes every receipt to a defaulted TestRun, so the verb's evidence is
    # the FIRST decode carrying a real mutation lane — else a barren pytest receipt would mask the Stryker/mutmut row that ran.
    match params.mutation:
        case False:
            return None
        case True:
            return next((d for c in done if (d := parse_tests(c)) is not None and _is_mutation(d)), None)


def _is_mutation(detail: AnyDetail) -> bool:
    # Narrow AnyDetail to a TestRun whose mutation lane names a real runner, rejecting the OFF default and non-TestRun variants.
    match detail:
        case TestRun(mutation=lane) if lane is not MutationLane.OFF:
            return True
        case _:
            return False


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """The one fold every ``test`` verb shares: gate, route per language, fan out per language, fold.

    The eligible slice is mutation-gated before any ``Check`` is built (the parity note is logged when a
    requested mutation lane was structurally inapplicable). When any eligible row carries ``Mode.MUTATION``
    the whole multi-language weave runs under the single global-exclusive ``mutation.lock`` lease: a busy
    lock short-circuits to ``Fault(BUSY)`` (exit 5) without spawning. ``fold`` carries the ``_detail`` evidence.
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
    """``Handler``: execute the test suite (``Mode.RUN``) for the routed languages."""
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)


def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # noqa: A001  # the verb IS "list"; this is the registry-bound Handler name
    """``Handler``: enumerate tests (``Mode.LIST``) and surface the ``--dead-fixtures``/``--dup-fixtures`` audit when ``params.fixtures`` is set."""
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="list", mode=Mode.LIST)


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """``Handler``: run the suite under coverage (``Mode.RUN`` + the Coverlet/``coverage.py`` tail); percentage rides ``TestRun.coverage``."""
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "list", "run", "thin_rail"]
