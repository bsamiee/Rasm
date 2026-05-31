"""Cyclopts entrypoint routing static, test, bridge, and RhinoWIP API quality rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Callable
from functools import partial
from pathlib import Path
import shutil
import sys
import time
from typing import Annotated, Final, Literal

from cyclopts import App, Parameter
from expression import Result
import msgspec
import structlog

from tools.quality.process import Completed, dotnet_build, ProcessFault
from tools.quality.rails import api as api_rail, bridge as bridge_rail, package as bridge_package, static as static_rail, test as test_rail
from tools.quality.settings import ArtifactScope, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type ApiCliOp = Literal["doctor", "path", "xml", "types", "decompile"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_BRIDGE_VERBS: Final[tuple[str, ...]] = ("doctor", "launch", "quit")
_PACKAGE: Final[tuple[bridge_package.PackageMode, ...]] = ("deploy", "package", "publish")
_PARAMETER = Parameter(show_default=False)
_SELF_CLI: Final[tuple[str, ...]] = ("dotnet", "fd", "git")
_STATIC: Final[dict[str, tuple[str, static_rail.StaticScope]]] = {"check": ("check", "changed"), "full": ("full", "full")}
api = App(name="api", help="RhinoWIP API metadata.", default_parameter=_PARAMETER)
app = App(name="quality", help="Rasm typed quality operator.", default_parameter=_PARAMETER, result_action="return_value")
bridge = App(name="bridge", help="Rhino bridge gate.", default_parameter=_PARAMETER)
log = structlog.get_logger("quality")
static = App(name="static", help="Static C# gate.", default_parameter=_PARAMETER)
test_app = App(name="test", help="Unit and mutation gate.", default_parameter=_PARAMETER)


# --- [OPERATIONS] ------------------------------------------------------------------------


def _bridge(*args: str) -> int:
    return rail(
        "bridge",
        "client",
        lambda s, sc: bridge_rail.build_client(s, sc).bind(lambda _: bridge_rail.client_run(s, sc, *args, check=False)),
        ok=_bridge_on_ok,
    )


def _bridge_on_ok(completed: Completed) -> int:
    sys.stdout.buffer.write(completed.stdout)
    sys.stderr.buffer.write(completed.stderr)
    match completed.stdout:
        case b"":
            return completed.returncode
        case payload:
            return bridge_rail.try_decode_bridge(payload).map(lambda result: result.exit_code).default_with(lambda _: completed.returncode)


def _emit(payload: str | bytes | None, code: int = 0, *, newline: bool = False) -> int:
    ending = b"\n" if newline else b""
    match payload:
        case bytes() as data:
            sys.stdout.buffer.write(data + ending)
        case str() as text:
            sys.stdout.write(text + ending.decode())
        case None:
            pass
    return code


def _package(mode: bridge_package.PackageMode, slug: str, version: str) -> int:
    return rail(
        "bridge",
        mode,
        lambda s, sc: bridge_package.run_package_rail(s, sc, mode, slug, version),
        ok=lambda artifact: _emit(str(artifact.stage), newline=True) if artifact is not None else 0,
    )


_API: Final[dict[ApiCliOp, tuple[api_rail.ApiOp, str | None, Callable[[bytes | str | None], int]]]] = {
    "decompile": ("decompile", None, _emit),
    "doctor": ("doctor", None, lambda payload: _emit(payload if isinstance(payload, bytes) else b"", newline=True)),
    "path": ("path", None, lambda value: _emit(value.decode() if isinstance(value, bytes) else value, newline=True)),
    "types": ("types", None, _emit),
    "xml": ("search", "xml", _emit),
}
_STATIC_OK: Final[dict[static_rail.StaticOutcome, Callable[[], int]]] = {
    "done": lambda: 0,
    "skip": lambda: (log.info("phase", status="skipped", message="no C#-relevant changes"), 0)[1],
}


@api.default
def api_gate(
    op: ApiCliOp, key: api_rail.ApiKey = "rhino-common", kind: api_rail.ApiPathKind = "assembly", pattern: str = "", type_name: str = ""
) -> int:
    rail_op, phase, ok = _API[op]
    return rail(
        "api",
        phase or op,
        lambda s, sc: api_rail.api(s.rhino_app, rail_op, key, kind=kind, pattern=pattern, type_name=type_name, env=sc.dotnet_env),
        ok=ok,
    )


@bridge.command(name="build-bridge")
def build_bridge_cmd() -> int:
    return rail("bridge", "build-bridge", lambda s, sc: dotnet_build(s, sc, restore=s.solution, targets=s.bridge_projects, serial=True))


@bridge.command
def check(target: str, scenario: str = "") -> int:
    return _bridge("check", target, *((scenario,) if scenario else ()))


@bridge.command
def clean(target: str = "") -> int:
    return _bridge("clean", *((target,) if target else ()))


def rail[T](
    rail_name: str,
    phase: str,
    execute: Callable[[QualitySettings, ArtifactScope], Result[T, ProcessFault]],
    *,
    settings: QualitySettings | None = None,
    ok: Callable[[T], int] | None = None,
) -> int:
    cfg = settings or QualitySettings()
    started = time.perf_counter()
    with structlog.contextvars.bound_contextvars(rail=rail_name, phase=phase):
        with ArtifactScope.open(cfg, rail_name) as scope:
            on_ok = ok or (lambda _: 0)
            code = execute(cfg, scope).map(on_ok).default_with(lambda fault: (sys.stderr.write(f"{fault.message}\n"), fault.returncode or 1)[1])
        log.info("phase", status="ok" if code == 0 else "failed", exit_code=code, duration_ms=(time.perf_counter() - started) * 1000)
        return code


@app.command(name="self-test")
def self_test_cmd() -> int:
    settings = QualitySettings()
    missing = tuple(cmd for cmd in _SELF_CLI if shutil.which(cmd) is None)
    paths = (settings.solution, settings.root / settings.test_target, settings.dotnet_tools_manifest)
    match (missing, all(path.is_file() for path in paths)):
        case ([], True):
            sys.stdout.write("quality: self-test passed\n")
            return 0
        case _:
            sys.stderr.write(f"quality: self-test failed missing={missing}\n")
            return 2


@static.default
def static_gate(mode: Literal["check", "full"]) -> int:
    phase, scope = _STATIC[mode]
    return rail("static", phase, lambda s, sc: static_rail.run_static_rail(s, sc, scope), ok=lambda outcome: _STATIC_OK[outcome]())


@test_app.default
def unit_gate(
    mode: Literal["run", "list", "coverage"],
    filter_expr: str = "",
    target: Path | None = None,
    all_targets: Annotated[bool | None, Parameter(name="--all")] = None,
    mutation: test_rail.MutationMode = "off",
) -> int:
    cfg = QualitySettings()
    settings = cfg if target is None else cfg.model_copy(update={"test_target": target})
    all_runnable = all_targets is True
    return rail(
        "test",
        mode,
        lambda s, sc: test_rail.run_test_rail(s, sc, mode, all_targets=all_runnable, filter_expr=filter_expr, mutation=mutation),
        settings=settings,
        ok=lambda _: _test_render(settings, mode, all_targets=all_runnable, mutation=mutation),
    )


def _verify_render(summary: bridge_rail.VerifyReport) -> int:
    for scenario in summary.scenarios:
        log.info(
            "scenario",
            name=scenario.name,
            status=scenario.status,
            report=scenario.report_path,
            captures=tuple(capture.get("path") for capture in scenario.captures),
            facts={key: value for block in scenario.facts for key, value in block.items()},
            fault=scenario.fault.message if scenario.fault is not None else None,
            exception_reports=len(scenario.exception_reports),
            stdout_truncated=scenario.stdout_truncated or None,
        )
    log.info(
        "verify",
        ok=summary.summary.ok,
        failed=summary.summary.failed,
        total=summary.summary.total,
        report_dir=summary.report_dir,
        expires_in_seconds=summary.expires_in_seconds,
    )
    return _emit(msgspec.json.encode(summary), 0 if summary.failed == 0 else 1, newline=True)


def _test_render(settings: QualitySettings, mode: test_rail.TestMode, *, all_targets: bool, mutation: test_rail.MutationMode) -> int:
    log.info(
        "test",
        results=str(settings.test_results_dir),
        all=all_targets or None,
        mutation=mutation if mode == "run" else None,
        mutation_output=str(settings.mutation_output_dir) if mode == "run" and mutation != "off" and settings.mutation_eligible else None,
    )
    return 0


@bridge.command
def verify(pattern: str) -> int:
    return rail("bridge", "verify", lambda s, sc: bridge_rail.run_verify(s, sc, pattern), ok=_verify_render)


# --- [COMPOSITION] -----------------------------------------------------------------------


for _verb in _BRIDGE_VERBS:
    bridge.command(name=_verb)(partial(_bridge, _verb))

for _package_mode in _PACKAGE:
    bridge.command(name=_package_mode)(partial(_package, _package_mode))

for _rail_app in (static, test_app, bridge, api):
    app.command(_rail_app)


def main(argv: list[str] | None = None) -> int:
    structlog.configure(
        processors=(structlog.contextvars.merge_contextvars, structlog.processors.TimeStamper(fmt="iso"), structlog.dev.ConsoleRenderer()),
        # Logs/diagnostics to stderr; machine payloads (rail JSON, _emit) stay alone on stdout.
        logger_factory=structlog.PrintLoggerFactory(file=sys.stderr),
        cache_logger_on_first_use=True,
    )
    return (app() if argv is None else app(argv)) or 0


if __name__ == "__main__":
    raise SystemExit(main())
