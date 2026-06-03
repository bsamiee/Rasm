"""Cyclopts entrypoint routing static, test, bridge, and RhinoWIP API quality rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Callable
from functools import partial
import os
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

type ApiCliOp = Literal["doctor", "resolve", "query", "show"]
type ApiShowPolicy = Literal["current", "latest"]
type StaticCliOp = Literal["fix", "report", "build", "full", "plan"]
type TestListFormat = Literal["text", "json"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_BRIDGE_VERBS: Final[tuple[str, ...]] = ("doctor", "launch", "quit")
_PACKAGE: Final[tuple[bridge_package.PackageMode, ...]] = ("deploy", "publish")
_PARAMETER = Parameter(show_default=False)
_SELF_CLI: Final[tuple[str, ...]] = ("dotnet", "fd", "git", "rg")
_STATIC: Final[dict[StaticCliOp, tuple[str, static_rail.StaticMode]]] = {
    "fix": ("fix", "fix"),
    "report": ("report", "report"),
    "build": ("build", "build"),
    "full": ("full", "full"),
    "plan": ("plan", "plan"),
}
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
        lambda s, sc: bridge_rail.with_bridge_lease(
            s, lambda: bridge_rail.build_client(s, sc).bind(lambda _: bridge_rail.client_run(s, sc, *args, check=False))
        ),
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


@bridge.command(name="package")
def package_cmd(slug: str, version: str = "", extra: str = "") -> int:
    match slug:
        case "list":
            return rail(
                "bridge", "package-list", lambda s, sc: bridge_package.package_list_payload(s, sc), ok=lambda payload: _emit(payload, newline=True)
            )
        case "plan":
            return rail(
                "bridge",
                "package-plan",
                lambda s, sc: bridge_package.package_plan_payload(s, sc, version, extra),
                ok=lambda payload: _emit(payload, newline=True),
            )
        case _:
            return _package("package", slug, version)


def _emit_api(payload: bytes | str | None, *, strict: bool = False, newline: bool = True) -> int:
    code = 0
    match payload:
        case bytes() as raw:
            pass
        case str() as text:
            raw = text.encode()
        case None:
            raw = b"{}"
    match strict:
        case True:
            try:
                status = msgspec.json.decode(raw).get("status", "")
                code = 2 if status in {"failed", "missing"} else 0
            except msgspec.DecodeError, AttributeError:
                code = 0
        case _:
            pass
    return _emit(payload, code=code, newline=newline)


_API_PATH_KIND: Final[dict[str, api_rail.ApiPathKind]] = {
    "all": "all",
    "assembly": "assembly",
    "deps": "deps",
    "nuspec": "nuspec",
    "package-root": "package-root",
    "xml": "xml",
}
_STATIC_OK: Final[dict[static_rail.StaticOutcome, Callable[[], int]]] = {
    "done": lambda: 0,
    "full-trigger-skip": lambda: (log.info("phase", status="skipped", message="full-trigger input requires static full"), 0)[1],
    "skip": lambda: (log.info("phase", status="skipped", message="no C#-relevant changes"), 0)[1],
}


@api.default
def api_gate(
    op: ApiCliOp,
    key: str = "rhino-common",
    value: str = "",
    *,
    strict: Annotated[bool | None, Parameter(name="--strict")] = None,
    restore: Annotated[api_rail.ApiRestoreMode, Parameter(name="--restore")] = "missing",
    max_lines: Annotated[int, Parameter(name="--max-lines")] = 120,
    full: Annotated[bool | None, Parameter(name="--full")] = None,
    lines: Annotated[str, Parameter(name="--lines")] = "",
    grep: Annotated[str, Parameter(name="--grep")] = "",
    latest: Annotated[bool | None, Parameter(name="--latest")] = None,
) -> int:
    # `value` discriminates by verb: resolve kind, query symbol, otherwise unused; `key` is the source key (or show token).
    kind = _API_PATH_KIND.get(value, "all") if op == "resolve" else "all"
    symbol = value if op == "query" else ""
    show_policy: ApiShowPolicy = "latest" if latest is True else "current"
    return rail(
        "api",
        op,
        lambda s, sc: api_rail.api(
            s,
            sc,
            op,
            key,
            symbol=symbol,
            kind=kind,
            max_lines=max_lines,
            full=full is True,
            lines=lines,
            grep=grep,
            show_policy=show_policy,
            restore=restore,
            env=sc.dotnet_env,
        ),
        ok=lambda payload: _emit_api(payload, strict=strict is True, newline=not (op == "show" and full is True)),
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
def self_test_cmd(rhino: Annotated[bool | None, Parameter(name="--rhino")] = None) -> int:
    settings = QualitySettings()
    missing = tuple(cmd for cmd in _SELF_CLI if shutil.which(cmd) is None)
    missing_paths = tuple(
        str(path) for path in (settings.solution, settings.root / settings.test_target, settings.dotnet_tools_manifest) if not path.is_file()
    )
    yak = settings.rhino_app / "Contents/Resources/bin/yak" if settings.rhino_app is not None else Path()
    yak_ready = settings.rhino_app is not None and yak.is_file() and os.access(yak, os.X_OK)
    match (missing, missing_paths, rhino is True, yak_ready):
        case ((), (), False, _):
            sys.stdout.write("quality: self-test passed\n")
            return 0
        case ((), (), True, True):
            sys.stdout.write("quality: self-test passed\n")
            return 0
        case _:
            sys.stderr.write(f"quality: self-test failed missing={missing} paths={missing_paths} yak={yak}\n")
            return 2


@static.default
def static_gate(mode: StaticCliOp, paths: tuple[Path, ...] = ()) -> int:
    phase, rail_mode = _STATIC[mode]
    match rail_mode:
        case "plan":
            return rail("static", phase, lambda s, sc: static_rail.plan_payload(s, sc, paths), ok=lambda payload: _emit(payload, newline=True))
        case _:
            return rail("static", phase, lambda s, sc: static_rail.run_static_rail(s, sc, rail_mode, paths), ok=lambda outcome: _STATIC_OK[outcome]())


@test_app.default
def unit_gate(
    mode: Literal["run", "list", "coverage"],
    filter_expr: str = "",
    target: Path | None = None,
    *,
    all_targets: Annotated[bool | None, Parameter(name="--all")] = None,
    mutation: test_rail.MutationMode = "off",
    no_build: Annotated[bool | None, Parameter(name="--no-build")] = None,
    test_modules: str = "",
    format: Annotated[TestListFormat, Parameter(name="--format")] = "text",
    limit: Annotated[int, Parameter(name="--limit")] = 0,
    grep: Annotated[str, Parameter(name="--grep")] = "",
) -> int:
    cfg = QualitySettings()
    settings = cfg if target is None else cfg.model_copy(update={"test_target": target})
    all_runnable = all_targets is True
    explicit_target = target is not None
    if mode == "list" and format == "json":
        return rail(
            "test",
            mode,
            lambda s, sc: test_rail.list_tests_payload(
                s,
                sc,
                all_targets=all_runnable,
                filter_expr=filter_expr,
                no_build=no_build is True,
                test_modules=test_modules,
                limit=limit,
                grep=grep,
                explicit_target=explicit_target,
            ),
            settings=settings,
            ok=lambda payload: _emit(payload, newline=True),
        )
    return rail(
        "test",
        mode,
        lambda s, sc: test_rail.run_test_rail(
            s,
            sc,
            mode,
            all_targets=all_runnable,
            filter_expr=filter_expr,
            mutation=mutation,
            no_build=no_build is True,
            test_modules=test_modules,
            explicit_target=explicit_target,
        ),
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
        first_failure=summary.first_failure.name if summary.first_failure is not None else None,
        first_failure_status=summary.first_failure.status if summary.first_failure is not None else None,
    )
    return _emit(msgspec.json.encode(summary), 0 if summary.failed == 0 else 1, newline=True)


def _test_render(settings: QualitySettings, mode: test_rail.TestMode, *, all_targets: bool, mutation: test_rail.MutationMode) -> int:
    log.info(
        "test",
        results=str(settings.test_results(all_targets=all_targets)),
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
