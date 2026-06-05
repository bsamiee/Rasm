"""Run test, list, coverage, and mutation rails."""

from dataclasses import dataclass
from pathlib import Path  # noqa: TC003  # unconditional: cyclopts resolves TestParams.target's `Path | None` at CLI-build (PEP 649)
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec
import structlog

from tools.assay.composition.catalog import parse_tests, select
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out, leased
from tools.assay.core.model import (
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,  # noqa: TC001  # unconditional: _roster_matches uses Completed as a runtime type in the tuple annotation
    Fault,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    fold,
    Match,
    Mode,
    MutationLane,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the rail's forward-ref (PEP 649)
    TestRun,
)
from tools.assay.core.routing import route


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import AnyDetail, Language, Tool
    from tools.assay.core.routing import Routed


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class TestParams(BaseParams):
    """Parameters shared by test verbs.

    Attributes:
        target: Optional test project override.
        all: Whether to widen to the whole workspace.
        no_build: Whether to skip restore and build rows.
        mutation: Whether to select mutation rows.
        benchmark: Whether to select benchmark rows.
        coverage: Whether to select coverage rows.
        fixtures: Whether to include fixture checks.
        filter: Runner-specific test filter expression.

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


def _roster_matches(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    # Parse discovered test names from OK LIST-mode Completed stdout into typed Match rows.
    # dotnet test --list-tests: indented names after "The following Tests are available:" header.
    # pytest --collect-only -q: "module::class::test" lines.
    # Skip blank lines, the dotnet header ("The following…"), and count summaries ("N test(s) found").
    def _skip(name: str) -> bool:
        return name.startswith("The ") or (name[:1].isdigit() and name.endswith("found"))

    rows = []
    for c in outcomes:
        if c.returncode == 0 and c.stdout:
            for raw in c.stdout.decode(errors="replace").splitlines():
                name = raw.strip()
                if name and not _skip(name):
                    rows.append(Match(id=name, kind=ArtifactKind.PROCESS, text=name))
    return tuple(rows)


def _detail(done: tuple[Completed, ...], params: TestParams) -> AnyDetail | None:
    # killed/survived/coverage are Parser-derived, never fold-derived (conflating kill-ratio into Counts would re-introduce
    # the retired per-rail count struct). parse_tests decodes every receipt to a defaulted TestRun, so the verb's evidence is
    # the FIRST decode carrying a real mutation lane — else a barren pytest receipt would mask the Stryker/mutmut row that ran.
    match params.mutation:
        case False:
            return None
        case True:
            return next(
                (d for c in done if (d := parse_tests(c)) is not None and isinstance(d, TestRun) and d.mutation is not MutationLane.OFF), None
            )


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: TestParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """Run the shared test route, eligibility, fan-out, and fold body.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Test params.
        claim: Claim to fold under.
        verb: Verb to fold under.
        mode: Tool mode to select.

    Returns:
        Result containing the folded test report or routing fault.

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
    """Run eligible test suites.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Test params.

    Returns:
        Result containing the test report or routing fault.

    """
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN)


def list(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:  # noqa: A001  # the verb IS "list"; this is the registry-bound Handler name
    """List discovered tests.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Test params.

    Returns:
        Result containing discovered test rows or routing fault.

    """
    languages = _languages(params.language)

    def _settle(done: block.Block[Completed]) -> Report:
        outcomes = tuple(done)
        base = fold(Claim.TEST, "list", outcomes)
        roster = _roster_matches(outcomes)
        return msgspec.structs.replace(base, results=(*base.results, *roster)) if roster else base

    return _routed(languages, params.paths).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, params, settings=settings, scope=scope, mode=Mode.LIST)))).map(
            _settle
        )
    )


def coverage(settings: AssaySettings, scope: ArtifactScope, params: TestParams) -> Result[Report, Fault]:
    """Run eligible suites under coverage.

    Args:
        settings: Runtime settings.
        scope: Artifact scope.
        params: Test params.

    Returns:
        Result containing the coverage report or routing fault.

    """
    return thin_rail(settings, scope, params, claim=Claim.TEST, verb="coverage", mode=Mode.RUN)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TestParams", "coverage", "list", "run", "thin_rail"]
