"""Microsoft Testing Platform and optional Stryker mutation rail for Rasm tests."""

# --- [IMPORTS] ------------------------------------------------------------------------

from dataclasses import dataclass
from pathlib import Path
from typing import Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec
import structlog

from tools.quality.process import Completed, dotnet, leased, ProcessFault, Workspace
from tools.quality.settings import ArtifactScope, MUTATION_THRESHOLDS, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type MutationMode = Literal["off", "changed", "full"]
type TestMode = Literal["run", "list", "coverage"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_COVERLET_ARGS: Final[tuple[str, ...]] = ("--coverlet", "--coverlet-output-format", "json", "--coverlet-output-format", "cobertura")
_STRYKER_STATIC_ARGS: Final[tuple[str, ...]] = tuple(
    item
    for pair in (
        ("--test-runner", "mtp"),
        ("--mutation-level", "Standard"),
        *((flag, str(value)) for flag, value in zip(("--threshold-high", "--threshold-low", "--break-at"), MUTATION_THRESHOLDS, strict=True)),
        *(("--reporter", reporter) for reporter in ("Html", "Json", "Progress")),
    )
    for item in pair
)
_STRYKER_ZERO_DISCOVERY_MARKERS: Final[tuple[bytes, ...]] = (b"Number of tests found: 0", b"No test result reported")
_STRYKER_ZERO_DISCOVERY_DETAIL: Final[bytes] = (
    b"Stryker discovered zero tests after MTP unit execution. Mutation is not a valid quality signal until Stryker discovers tests."
)
log = structlog.get_logger("quality.test")


@dataclass(frozen=True, slots=True)
class TestRailPlan:
    args: tuple[str, ...]
    can_mutate: bool


_TEST_PLANS: Final[dict[TestMode, TestRailPlan]] = {
    "coverage": TestRailPlan(args=_COVERLET_ARGS, can_mutate=False),
    "list": TestRailPlan(args=("--list-tests",), can_mutate=False),
    "run": TestRailPlan(args=(), can_mutate=True),
}


class TestRunReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    query: dict[str, str | bool]
    status: str
    artifact_paths: dict[str, str]
    mutation: MutationMode = "off"


class TestListReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    query: dict[str, object]
    status: str
    count: int
    returned: int
    tests: tuple[str, ...]
    artifact_paths: dict[str, str]


# --- [OPERATIONS] ------------------------------------------------------------------------


@beartype
def run_test_rail(
    settings: QualitySettings,
    scope: ArtifactScope,
    mode: TestMode,
    *,
    all_targets: bool = False,
    filter_expr: str = "",
    mutation: MutationMode = "off",
    no_build: bool = False,
    test_modules: str = "",
    explicit_target: bool = False,
) -> Result[TestRunReport, ProcessFault]:
    plan = _TEST_PLANS[mode]
    target_fault = _target_fault(all_targets=all_targets, test_modules=test_modules, explicit_target=explicit_target)
    if target_fault is not None:
        return Error(target_fault)
    mutation_fault = _mutation_preflight(
        settings, mode, all_targets=all_targets, test_modules=test_modules, mutation=mutation, can_mutate=plan.can_mutate
    )
    if mutation_fault is not None:
        return Error(mutation_fault)

    def report(_: None) -> TestRunReport:
        return TestRunReport(
            query={"op": mode, "all": all_targets, "filter": filter_expr, "test_modules": test_modules, "no_build": no_build},
            status="ok",
            artifact_paths={"results": str(settings.test_results(all_targets=all_targets)), "run": str(scope.path)},
            mutation=mutation if mode == "run" else "off",
        )

    return (
        dotnet(
            *_test_args(settings, plan.args, filter_expr, all_targets=all_targets, no_build=no_build, test_modules=test_modules),
            scope=scope,
            scoped=False,
            check=True,
            timeout=settings.test_timeout_s,
            mode="stream",
        )
        .map(lambda _: None)
        .bind(lambda _: _run_mutation(settings, scope, mutation).map(lambda _: None) if mutation != "off" else Ok(None))
        .map(report)
    )


def list_tests_payload(
    settings: QualitySettings,
    scope: ArtifactScope,
    *,
    all_targets: bool = False,
    filter_expr: str = "",
    no_build: bool = False,
    test_modules: str = "",
    limit: int = 0,
    grep: str = "",
    explicit_target: bool = False,
) -> Result[bytes, ProcessFault]:
    target_fault = _target_fault(all_targets=all_targets, test_modules=test_modules, explicit_target=explicit_target)
    if target_fault is not None:
        return Error(target_fault)

    def report(completed: Completed) -> bytes:
        pattern = grep.strip()
        discovered = tuple(line.strip() for line in completed.lines() if line.strip() and " " not in line.strip())
        matched = tuple(row for row in discovered if not pattern or pattern.lower() in row.lower())
        selected = matched[:limit] if limit > 0 else matched
        return msgspec.json.encode(
            TestListReport(
                query={"op": "list", "all": all_targets, "filter": filter_expr, "test_modules": test_modules, "limit": limit, "grep": pattern},
                status="ok",
                count=len(matched),
                returned=len(selected),
                tests=selected,
                artifact_paths={"results": str(settings.test_results(all_targets=all_targets)), "run": str(scope.path)},
            )
        )

    return dotnet(
        *_test_args(settings, _TEST_PLANS["list"].args, filter_expr, all_targets=all_targets, no_build=no_build, test_modules=test_modules),
        scope=scope,
        scoped=False,
        check=True,
        timeout=settings.test_timeout_s,
        mode="capture",
    ).map(report)


def _target_fault(*, all_targets: bool, test_modules: str, explicit_target: bool) -> ProcessFault | None:
    active = sum((all_targets, bool(test_modules.strip()), explicit_target))
    return ProcessFault.fail("test", "target", detail=b"Choose exactly one of --all, --test-modules, or an explicit target.") if active > 1 else None


def _mutation_preflight(
    settings: QualitySettings, mode: TestMode, *, all_targets: bool, test_modules: str, mutation: MutationMode, can_mutate: bool
) -> ProcessFault | None:
    match mutation:
        case "off":
            return None
        case _ if not can_mutate or mode != "run":
            return ProcessFault.fail("stryker", mutation, detail=b"Mutation is only valid for test run.")
        case _ if all_targets or test_modules.strip():
            return ProcessFault.fail("stryker", mutation, detail=b"Mutation requires the default managed test target.")
        case _ if not settings.mutation_eligible:
            return ProcessFault.fail("stryker", mutation, detail=b"Mutation is owned by the default Rasm project/test pair.")
        case _:
            return None


def _test_args(
    settings: QualitySettings, plan_args: tuple[str, ...], filter_expr: str, *, all_targets: bool, no_build: bool = False, test_modules: str = ""
) -> tuple[str, ...]:
    target_args: tuple[str, ...]
    build_args: tuple[str, ...]
    configuration_args: tuple[str, ...]
    match test_modules.strip():
        case str(expr) if expr:
            target_args = ("--test-modules", expr, "--root-directory", str(settings.root))
            build_args = ()
            configuration_args = ()
        case _:
            target_args = (
                ("--solution", str(settings.solution)) if all_targets else ("--project", str((settings.root / settings.test_target).resolve()))
            )
            build_args = ("--no-build",) if no_build else ()
            configuration_args = ("--configuration", settings.configuration)
    return (
        "test",
        *target_args,
        *configuration_args,
        *build_args,
        "--results-directory",
        str(settings.test_results(all_targets=all_targets)),
        "--minimum-expected-tests",
        "1",
        *plan_args,
        *_filter_args(filter_expr),
    )


def _filter_args(filter_expr: str) -> tuple[str, ...]:
    text = filter_expr.strip()
    match text:
        case "":
            return ()
        case query if query.startswith("/"):
            return ("--filter-query", query)
        case trait if "=" in trait:
            return ("--filter-trait", trait)
        case class_name if class_name.endswith(("Tests", "Laws", "Spec")) or "+" in class_name:
            return ("--filter-class", f"*{class_name}*")
        case method if "." in method:
            return ("--filter-method", f"*{method}*")
        case method:
            return ("--filter-method", f"*{method}*")


def _run_mutation(settings: QualitySettings, scope: ArtifactScope, mode: MutationMode) -> Result[None, ProcessFault]:
    def run_globs(globs: tuple[str, ...]) -> Result[None, ProcessFault]:
        match globs:
            case ():
                log.info("mutation", status="skipped", mode=mode, reason="no eligible managed source changes")
                return Ok(None)
            case _:
                return leased(
                    settings,
                    settings.mutation_lock,
                    "mutation",
                    lambda: dotnet("tool", "restore", "--tool-manifest", str(settings.dotnet_tools_manifest), scope=scope).bind(
                        lambda _: dotnet(
                            "tool",
                            "run",
                            "dotnet-stryker",
                            "--",
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
                            "--concurrency",
                            str(settings.mutation_max_cpu),
                            *_STRYKER_STATIC_ARGS,
                            *(item for glob in globs for item in ("--mutate", glob)),
                            scope=scope,
                            cwd=settings.root / settings.mutation_project.parent,
                            check=False,
                            timeout=settings.mutation_timeout_s,
                            mode="stream",
                        ).bind(_mutation_result)
                    ),
                    mode=mode,
                )

    return _mutation_globs(settings, mode).bind(run_globs)


def _mutation_globs(settings: QualitySettings, mode: MutationMode) -> Result[tuple[str, ...], ProcessFault]:
    match mode:
        case "off":
            return Ok(())
        case "full":
            return Ok(settings.mutation_mutate_globs)
        case "changed":
            project_dir = settings.mutation_project.parent.as_posix().rstrip("/") + "/"
            return (
                Workspace(settings.root)
                .changed()
                .map(
                    lambda rows: tuple(
                        path.removeprefix(project_dir)
                        for raw in rows
                        for path in (Path(raw).as_posix(),)
                        if path.startswith(project_dir)
                        and path.endswith(".cs")
                        and "/bin/" not in path
                        and "/obj/" not in path
                        and (settings.root / path).is_file()
                    )
                )
                .map(lambda changed: (*changed, "!**/bin/**/*.cs", "!**/obj/**/*.cs") if changed else ())
            )


def _mutation_result(completed: Completed) -> Result[None, ProcessFault]:
    payload = b"\n".join(filter(None, (completed.stderr, completed.stdout)))
    match all(marker in payload for marker in _STRYKER_ZERO_DISCOVERY_MARKERS):
        case True:
            return Error(
                ProcessFault.fail(
                    "stryker", "test-discovery", detail=b"\n".join((_STRYKER_ZERO_DISCOVERY_DETAIL, payload)), returncode=completed.returncode or 2
                )
            )
        case False if completed.returncode == 0:
            return Ok(None)
        case False:
            return Error(ProcessFault.fail(*completed.argv, detail=payload, returncode=completed.returncode))
