"""Microsoft Testing Platform and optional Stryker mutation rail for Rasm tests."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Generator
from contextlib import contextmanager
from dataclasses import dataclass
from importlib import import_module
import os
from pathlib import Path
from types import ModuleType
from typing import Final, Literal

from beartype import beartype
from expression import effect, Error, Ok, Result
import structlog

from tools.quality.process import Completed, dotnet, ProcessFault, Workspace
from tools.quality.settings import ArtifactScope, MUTATION_THRESHOLDS, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type MutationMode = Literal["off", "changed", "full"]
type TestMode = Literal["run", "list", "coverage"]
type TestPlan = tuple[tuple[str, ...], bool]


@dataclass(frozen=True, slots=True)
class _PosixLock:
    module: ModuleType
    exclusive: int
    nonblock: int
    unlock: int

    def flock(self, fd: int, operation: int, /) -> None:
        match getattr(self.module, "flock", None):
            case action if callable(action):
                action(fd, operation)
            case _:
                raise RuntimeError("fcntl.flock is unavailable")


def _posix_flag(module: ModuleType, name: str) -> int:
    match getattr(module, name, None):
        case int() as value:
            return value
        case _:
            raise RuntimeError(f"fcntl.{name} is unavailable")


# --- [CONSTANTS] -----------------------------------------------------------------------

_COVERLET_ARGS: Final[tuple[str, ...]] = (
    "--coverlet",
    "--coverlet-output-format",
    "json",
    "--coverlet-output-format",
    "cobertura",
    "--coverlet-include",
    "[Rasm*]*,[Foundation.CSharp.Analyzers]*,[Radyab]*",
    "--coverlet-exclude",
    "[*Tests]*,[*TestKit]*",
    "--coverlet-exclude-assemblies-without-sources",
    "None",
    "--coverlet-exclude-by-file",
    "**/obj/**;**/*.Generated.cs;**/*.g.cs",
    "--coverlet-exclude-by-attribute",
    "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute,System.CodeDom.Compiler.GeneratedCodeAttribute,System.Runtime.CompilerServices.CompilerGeneratedAttribute",
    "--coverlet-skip-auto-props",
)
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
_TEST_PLANS: Final[dict[TestMode, TestPlan]] = {"coverage": (_COVERLET_ARGS, False), "list": (("--list-tests",), False), "run": ((), True)}
_POSIX_MODULE: Final = import_module("fcntl")
_POSIX_LOCK: Final = _PosixLock(
    module=_POSIX_MODULE,
    exclusive=_posix_flag(_POSIX_MODULE, "LOCK_EX"),
    nonblock=_posix_flag(_POSIX_MODULE, "LOCK_NB"),
    unlock=_posix_flag(_POSIX_MODULE, "LOCK_UN"),
)
log = structlog.get_logger("quality.test")


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
) -> Result[None, ProcessFault]:
    plan_args, can_mutate = _TEST_PLANS[mode]

    @effect.result[None, ProcessFault]()
    def run() -> Generator[None]:
        yield from dotnet(
            *_test_args(settings, plan_args, filter_expr, all_targets=all_targets),
            scope=scope,
            scoped=False,
            check=True,
            timeout=settings.test_timeout_s,
            mode="stream",
        ).map(lambda _: None)
        match can_mutate and mutation != "off":
            case True:
                yield from _run_mutation(settings, scope, mutation).map(lambda _: None)
            case False:
                return

    return run()


def _test_args(settings: QualitySettings, plan_args: tuple[str, ...], filter_expr: str, *, all_targets: bool) -> tuple[str, ...]:
    match all_targets:
        case True:
            target_args = ("--solution", str(settings.solution))
        case False:
            target_args = ("--project", str((settings.root / settings.test_target).resolve()))
    return (
        "test",
        *target_args,
        "--configuration",
        settings.configuration,
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
    match settings.mutation_eligible:
        case False:
            return Error(ProcessFault.fail("stryker", mode, detail="Mutation is owned by the default Rasm project/test pair."))
        case True:
            pass
    globs = _mutation_globs(settings, mode)
    match globs:
        case ():
            log.info("mutation", status="skipped", mode=mode, reason="no eligible managed source changes")
            return Ok(None)
        case _:
            pass
    try:
        with _mutation_lock(settings):
            return dotnet("tool", "restore", "--tool-manifest", str(settings.dotnet_tools_manifest), scope=scope).bind(
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
            )
    except BlockingIOError:
        return Error(ProcessFault.fail("stryker", "lock", detail=f"mutation lock is already held: {settings.mutation_lock}"))
    except OSError as exc:
        return Error(ProcessFault.fail("stryker", "lock", detail=str(exc)))


@contextmanager
def _mutation_lock(settings: QualitySettings) -> Generator[None]:
    settings.mutation_lock.parent.mkdir(parents=True, exist_ok=True)
    with settings.mutation_lock.open(mode="a+", encoding="utf-8") as guard:
        _POSIX_LOCK.flock(guard.fileno(), _POSIX_LOCK.exclusive | _POSIX_LOCK.nonblock)
        guard.seek(0)
        owner = guard.read()
        if owner.strip():
            log.info("mutation", status="recovered-stale-lock", lock=str(settings.mutation_lock), owner=owner.strip())
        guard.seek(0)
        guard.truncate()
        guard.write(f"run_id={settings.run_id}\npid={os.getpid()}\n")
        guard.flush()
        try:
            yield
        finally:
            _POSIX_LOCK.flock(guard.fileno(), _POSIX_LOCK.unlock)
            settings.mutation_lock.unlink(missing_ok=True)


def _mutation_globs(settings: QualitySettings, mode: MutationMode) -> tuple[str, ...]:
    match mode:
        case "off":
            return ()
        case "full":
            return settings.mutation_mutate_globs
        case "changed":
            project_dir = settings.mutation_project.parent.as_posix().rstrip("/") + "/"
            changed = tuple(
                path.removeprefix(project_dir)
                for raw in Workspace(settings.root).changed()
                for path in (Path(raw).as_posix(),)
                if path.startswith(project_dir) and path.endswith(".cs") and "/bin/" not in path and "/obj/" not in path
            )
            return (*changed, "!**/bin/**/*.cs", "!**/obj/**/*.cs") if changed else ()


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
