"""VSTest and optional Stryker mutation rail for the managed Rasm test project."""

# --- [IMPORTS] ------------------------------------------------------------------------

from __future__ import annotations

from typing import Final, Literal

from beartype import beartype
from expression import Ok, Result

from tools.quality.process import dotnet, ProcessFault
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
        *(
            (flag, str(value))
            for flag, value in zip(
                ("--threshold-high", "--threshold-low", "--break-at"), MUTATION_THRESHOLDS, strict=True
            )
        ),
        *(("--reporter", reporter) for reporter in ("Html", "Json", "Progress")),
    )
    for item in pair
)
_TEST_PLANS: Final[dict[TestMode, TestPlan]] = {
    "coverage": (("/p:CollectCoverage=true",), True, True),
    "list": (("--list-tests",), False, False),
    "run": ((), True, False),
}


# --- [OPERATIONS] ------------------------------------------------------------------------


@beartype
def run_test_rail(
    settings: QualitySettings, scope: ArtifactScope, mode: TestMode, *, filter_expr: str = ""
) -> Result[None, ProcessFault]:
    target = (settings.root / settings.test_target).resolve()
    plan_args, mutate, coverage = _TEST_PLANS[mode]
    coverage_props = tuple(
        f"/p:{key}={value}"
        for key, value in zip(
            _COVERAGE_KEYS,
            (settings.coverage_threshold, settings.coverage_threshold_type, settings.coverage_threshold_stat),
            strict=True,
        )
        if value is not None
    )
    return (
        dotnet(
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
        )
        .map(lambda _: None)
        .bind(
            lambda _: (
                Ok(None)
                if not mutate or not settings.mutation_eligible
                else dotnet("tool", "restore", "--tool-manifest", str(settings.dotnet_tools_manifest), scope=scope)
                .map(lambda _: None)
                .bind(
                    lambda _: dotnet(
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
                        check=True,
                    ).map(lambda _: None)
                )
            )
        )
    )
