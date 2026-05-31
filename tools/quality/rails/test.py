"""VSTest and optional Stryker mutation rail for the managed Rasm test project."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Generator
from typing import Final, Literal

from beartype import beartype
from expression import effect, Error, Ok, Result

from tools.quality.process import Completed, dotnet, ProcessFault
from tools.quality.settings import ArtifactScope, MUTATION_THRESHOLDS, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type TestMode = Literal["run", "list", "coverage"]
type TestPlan = tuple[tuple[str, ...], bool, bool]


# --- [CONSTANTS] -----------------------------------------------------------------------

_COVERAGE_KEYS: Final[tuple[str, ...]] = ("Threshold", "ThresholdType", "ThresholdStat")
_STRYKER_STATIC_ARGS: Final[tuple[str, ...]] = tuple(
    item
    for pair in (
        ("--test-runner", "vstest"),
        ("--mutation-level", "Standard"),
        *((flag, str(value)) for flag, value in zip(("--threshold-high", "--threshold-low", "--break-at"), MUTATION_THRESHOLDS, strict=True)),
        *(("--reporter", reporter) for reporter in ("Html", "Json", "Progress")),
    )
    for item in pair
)
_STRYKER_ZERO_DISCOVERY_MARKERS: Final[tuple[bytes, ...]] = (b"Number of tests found: 0", b"No test result reported")
_TEST_PLANS: Final[dict[TestMode, TestPlan]] = {
    "coverage": (("/p:CollectCoverage=true",), True, True),
    "list": (("--list-tests",), False, False),
    "run": ((), True, False),
}


# --- [OPERATIONS] ------------------------------------------------------------------------


@beartype
def run_test_rail(settings: QualitySettings, scope: ArtifactScope, mode: TestMode, *, filter_expr: str = "") -> Result[None, ProcessFault]:
    target = (settings.root / settings.test_target).resolve()
    plan_args, mutate, coverage = _TEST_PLANS[mode]
    coverage_props = tuple(
        f"/p:{key}={value}"
        for key, value in zip(
            _COVERAGE_KEYS, (settings.coverage_threshold, settings.coverage_threshold_type, settings.coverage_threshold_stat), strict=True
        )
        if value is not None
    )

    @effect.result[None, ProcessFault]()
    def run() -> Generator[None]:
        yield from dotnet(
            "test",
            str(target),
            "--configuration",
            settings.configuration,
            "--results-directory",
            str(settings.test_results_dir),
            *(plan_args + (coverage_props if coverage else ())),
            *(("--filter", filter_expr) if filter_expr else ()),
            scope=scope,
            check=True,
        ).map(lambda _: None)
        match mutate and settings.mutation_eligible:
            case False:
                return
            case True:
                yield from dotnet("tool", "restore", "--tool-manifest", str(settings.dotnet_tools_manifest), scope=scope).map(lambda _: None)
                yield from dotnet(
                    "stryker",
                    "--solution",
                    str(settings.solution),
                    "--project",
                    settings.mutation_project.name,
                    "--test-project",
                    str(settings.root / settings.mutation_test_project),
                    "--configuration",
                    settings.configuration,
                    "--target-framework",
                    settings.mutation_target_framework,
                    "--output",
                    str(settings.mutation_output_dir),
                    *_STRYKER_STATIC_ARGS,
                    *(item for glob in settings.mutation_mutate_globs for item in ("--mutate", glob)),
                    scope=scope,
                    check=False,
                ).bind(_mutation_result)
                return

    return run()


def _mutation_result(completed: Completed) -> Result[None, ProcessFault]:
    payload = b"\n".join(filter(None, (completed.stderr, completed.stdout)))
    match completed.returncode == 0 or all(marker in payload for marker in _STRYKER_ZERO_DISCOVERY_MARKERS):
        case True:
            return Ok(None)
        case False:
            return Error(ProcessFault.fail(*completed.argv, detail=payload, returncode=completed.returncode))
