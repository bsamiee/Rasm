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
from expression import Error, Ok, Result
import msgspec
import structlog

from tools.assay._logging import configure_logging  # noqa: PLC2701  # shared live-stderr structlog owner; one config across both packages
from tools.quality.process import Completed, dotnet_build, ProcessFault, RailStatus
from tools.quality.rails import api as api_rail, bridge as bridge_rail, package as bridge_package, static as static_rail, test as test_rail
from tools.quality.settings import ArtifactScope, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type JsonPrimitive = str | int | float | bool | None
type JsonValue = JsonPrimitive | tuple[JsonValue, ...] | dict[str, JsonValue]
type QualityPayload = bytes | str | msgspec.Struct | Completed | None
type TestListFormat = Literal["text", "json"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_BRIDGE_VERBS: Final[tuple[str, ...]] = ("doctor", "launch", "quit")
_PACKAGE: Final[tuple[bridge_package.PackageMode, ...]] = ("deploy", "publish")
_PARAMETER = Parameter(show_default=False)
_SELF_CLI: Final[tuple[str, ...]] = ("dotnet", "fd", "git", "rg")
api = App(name="api", help="RhinoWIP API metadata.", default_parameter=_PARAMETER)
app = App(name="quality", help="Rasm typed quality operator.", default_parameter=_PARAMETER, result_action="return_value")
bridge = App(name="bridge", help="Rhino bridge gate.", default_parameter=_PARAMETER)
log = structlog.get_logger("quality")
static = App(name="static", help="Static C# gate.", default_parameter=_PARAMETER)
test_app = App(name="test", help="Unit and mutation gate.", default_parameter=_PARAMETER)


# --- [OPERATIONS] ------------------------------------------------------------------------


class CompletedPayload(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    argv: tuple[str, ...]
    returncode: int
    stdout: str = ""
    stderr: str = ""


class FaultPayload(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    argv: tuple[str, ...]
    status: RailStatus
    returncode: int
    message: str
    stderr: str = ""


class SelfTestReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    missing_cli: tuple[str, ...]
    missing_paths: tuple[str, ...]
    yak: str
    yak_ready: bool
    rhino_required: bool


class Envelope(msgspec.Struct, frozen=True, gc=False, kw_only=True):
    schema_version: int = 1
    rail: str
    verb: str
    command_path: tuple[str, ...] = ()
    status: RailStatus = RailStatus.OK
    exit_code: int = 0
    run_id: str = ""
    duration_ms: float = 0.0
    data: JsonValue = None
    error: FaultPayload | None = None
    evidence: dict[str, JsonValue] = msgspec.field(default_factory=dict)
    truncated: bool = False
    notes: tuple[str, ...] = ()


def _bridge(*args: str) -> int:
    return rail(
        "bridge",
        "client",
        lambda s, sc: bridge_rail.with_bridge_lease(
            s, lambda: bridge_rail.build_client(s, sc).bind(lambda _: bridge_rail.client_run(s, sc, *args, check=False))
        ),
        command_path=("bridge", *args),
        status=bridge_status,
        exit_code=bridge_exit_code,
    )


def _package(mode: bridge_package.PackageMode, slug: str, version: str) -> int:
    return rail(
        "package",
        mode,
        lambda s, sc: bridge_package.run_package_rail(s, sc, mode, slug, version),
        command_path=("bridge", mode),
        data=lambda artifact: bridge_package.package_run_report(mode, slug, version, artifact),
    )


@bridge.command(name="package")
def package_cmd(slug: str, version: str = "", extra: str = "") -> int:
    match slug:
        case "list":
            return rail(
                "package", "package-list", lambda s, sc: bridge_package.package_list_payload(s, sc), command_path=("bridge", "package", "list")
            )
        case "plan":
            return rail(
                "package",
                "package-plan",
                lambda s, sc: bridge_package.package_plan_payload(s, sc, version, extra),
                command_path=("bridge", "package", "plan"),
            )
        case _:
            return _package("package", slug, version)


_API_PATH_KIND: Final[dict[str, api_rail.ApiPathKind]] = {
    "all": "all",
    "assembly": "assembly",
    "deps": "deps",
    "nuspec": "nuspec",
    "package-root": "package-root",
    "xml": "xml",
}


@api.default
def api_gate(
    op: api_rail.ApiOp,
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
    show_policy: api_rail.ApiShowPolicy = "latest" if latest is True else "current"
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
        status=api_status,
        exit_code=lambda payload: api_exit_code(payload, strict=strict is True),
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
    command_path: tuple[str, ...] = (),
    data: Callable[[T], QualityPayload] | None = None,
    status: Callable[[T], RailStatus] | None = None,
    exit_code: Callable[[T], int] | None = None,
    notes: Callable[[T], tuple[str, ...]] | None = None,
) -> int:
    cfg = settings or QualitySettings()
    started = time.perf_counter()
    with structlog.contextvars.bound_contextvars(rail=rail_name, phase=phase):
        with ArtifactScope.open(cfg, rail_name) as scope:
            result = execute(cfg, scope)
            code = result.map(
                lambda value: emit_success(
                    rail_name, phase, cfg, started, value, command_path=command_path, data=data, status=status, exit_code=exit_code, notes=notes
                )
            ).default_with(lambda fault: emit_fault(rail_name, phase, cfg, started, fault, command_path=command_path))
        log.info("phase", status="ok" if code == 0 else "failed", exit_code=code, duration_ms=(time.perf_counter() - started) * 1000)
        return code


@app.command(name="self-test")
def self_test_cmd(rhino: Annotated[bool | None, Parameter(name="--rhino")] = None) -> int:
    return rail("self", "self-test", lambda s, _: self_test(s, require_rhino=rhino is True), command_path=("self-test",))


@static.default
def static_gate(mode: static_rail.StaticMode, paths: tuple[Path, ...] = ()) -> int:
    match mode:
        case "plan":
            return rail("static", mode, lambda s, sc: static_rail.plan_payload(s, sc, paths), command_path=("static", mode))
        case _:
            return rail(
                "static",
                mode,
                lambda s, sc: static_rail.run_static_rail(s, sc, mode, paths),
                command_path=("static", mode),
                status=static_status,
                notes=static_notes,
            )


@test_app.default
def unit_gate(
    mode: test_rail.TestMode,
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
    return rail(
        "test",
        mode,
        lambda s, sc: (
            test_rail.list_tests_payload(
                s,
                sc,
                all_targets=all_runnable,
                filter_expr=filter_expr,
                no_build=no_build is True,
                test_modules=test_modules,
                limit=limit,
                grep=grep,
                explicit_target=explicit_target,
            )
            if mode == "list" and format == "json"
            else test_rail.run_test_rail(
                s,
                sc,
                mode,
                all_targets=all_runnable,
                filter_expr=filter_expr,
                mutation=mutation,
                no_build=no_build is True,
                test_modules=test_modules,
                explicit_target=explicit_target,
            )
        ),
        settings=settings,
        command_path=("test", mode),
    )


@bridge.command
def verify(pattern: str) -> int:
    return rail(
        "bridge",
        "verify",
        lambda s, sc: bridge_rail.run_verify(s, sc, pattern),
        command_path=("bridge", "verify"),
        status=lambda summary: RailStatus.OK if summary.failed == 0 else RailStatus.FAILED,
        exit_code=lambda summary: 0 if summary.failed == 0 else 1,
    )


def self_test(settings: QualitySettings, *, require_rhino: bool) -> Result[SelfTestReport, ProcessFault]:
    missing = tuple(cmd for cmd in _SELF_CLI if shutil.which(cmd) is None)
    missing_paths = tuple(
        str(path) for path in (settings.solution, settings.root / settings.test_target, settings.dotnet_tools_manifest) if not path.is_file()
    )
    yak = settings.rhino_app / "Contents/Resources/bin/yak" if settings.rhino_app is not None else Path()
    yak_ready = settings.rhino_app is not None and yak.is_file() and os.access(yak, os.X_OK)
    data = SelfTestReport(missing_cli=missing, missing_paths=missing_paths, yak=str(yak), yak_ready=yak_ready, rhino_required=require_rhino)
    match (missing, missing_paths, require_rhino, yak_ready):
        case ((), (), False, _) | ((), (), True, True):
            return Ok(data)
        case _:
            return Error(ProcessFault.fail("self-test", detail=msgspec.json.encode(data), returncode=2))


def emit_success[T](
    rail_name: str,
    phase: str,
    settings: QualitySettings,
    started: float,
    value: T,
    *,
    command_path: tuple[str, ...],
    data: Callable[[T], QualityPayload] | None,
    status: Callable[[T], RailStatus] | None,
    exit_code: Callable[[T], int] | None,
    notes: Callable[[T], tuple[str, ...]] | None,
) -> int:
    resolved_status = status(value) if status is not None else RailStatus.OK
    code = exit_code(value) if exit_code is not None else resolved_status.exit_code
    payload = data(value) if data is not None else quality_payload(value)
    return emit_envelope(
        Envelope(
            rail=rail_name,
            verb=phase,
            command_path=command_path,
            status=resolved_status,
            exit_code=code,
            run_id=settings.run_id,
            duration_ms=(time.perf_counter() - started) * 1000,
            data=payload_json(payload),
            notes=notes(value) if notes is not None else (),
        )
    )


def emit_fault(rail_name: str, phase: str, settings: QualitySettings, started: float, fault: ProcessFault, *, command_path: tuple[str, ...]) -> int:
    code = fault.returncode or fault.status.exit_code
    sys.stderr.write(f"{fault.message}\n")
    return emit_envelope(
        Envelope(
            rail=rail_name,
            verb=phase,
            command_path=command_path,
            status=fault.status,
            exit_code=code,
            run_id=settings.run_id,
            duration_ms=(time.perf_counter() - started) * 1000,
            error=FaultPayload(
                argv=fault.argv, status=fault.status, returncode=fault.returncode, message=fault.message, stderr=fault.stderr.decode(errors="replace")
            ),
        )
    )


def emit_envelope(envelope: Envelope) -> int:
    sys.stdout.buffer.write(msgspec.json.encode(envelope) + b"\n")
    return envelope.exit_code


def quality_payload(value: object) -> QualityPayload:
    match value:
        case None | bytes() | str() | msgspec.Struct() | Completed():
            return value
        case _:
            return str(value)


def payload_json(payload: QualityPayload) -> JsonValue:
    match payload:
        case None:
            return None
        case Completed(argv=argv, returncode=returncode, stdout=stdout, stderr=stderr):
            return payload_json(
                CompletedPayload(argv=argv, returncode=returncode, stdout=stdout.decode(errors="replace"), stderr=stderr.decode(errors="replace"))
            )
        case bytes() as raw:
            return decode_json_value(raw)
        case str() as text:
            return decode_json_value(text.encode())
        case msgspec.Struct():
            return decode_json_value(msgspec.json.encode(payload))


def decode_json_value(raw: bytes) -> JsonValue:
    try:
        return json_value(msgspec.json.decode(raw))
    except msgspec.DecodeError:
        return raw.decode(errors="replace")


def json_value(value: object) -> JsonValue:
    match value:
        case None | str() | bool() | int() | float():
            return value
        case list() | tuple():
            return tuple(json_value(item) for item in value)
        case dict() as rows:
            return {str(key): json_value(item) for key, item in rows.items()}
        case _:
            return str(value)


def bridge_status(completed: Completed) -> RailStatus:
    return (
        bridge_rail
        .try_decode_bridge(completed.stdout)
        .map(lambda result: result.status)
        .default_value(RailStatus.OK if completed.returncode == 0 else RailStatus.FAILED)
    )


def bridge_exit_code(completed: Completed) -> int:
    return bridge_rail.try_decode_bridge(completed.stdout).map(lambda result: result.exit_code).default_value(completed.returncode)


def api_status(payload: bytes | str) -> RailStatus:
    match payload_json(payload):
        case {"status": "ok"}:
            return RailStatus.OK
        case {"status": "empty"}:
            return RailStatus.EMPTY
        case {"status": "missing" | "failed"}:
            return RailStatus.FAILED
        case _:
            return RailStatus.OK


def api_exit_code(payload: bytes | str, *, strict: bool) -> int:
    status = api_status(payload)
    return 2 if strict and status == RailStatus.FAILED else 0


def static_status(outcome: static_rail.StaticOutcome) -> RailStatus:
    match outcome:
        case "done":
            return RailStatus.OK
        case "skip" | "full-trigger-skip":
            return RailStatus.SKIP


def static_notes(outcome: static_rail.StaticOutcome) -> tuple[str, ...]:
    match outcome:
        case "full-trigger-skip":
            return ("full-trigger input requires static full",)
        case "skip":
            return ("no C#-relevant changes",)
        case "done":
            return ()


# --- [COMPOSITION] -----------------------------------------------------------------------


for _verb in _BRIDGE_VERBS:
    bridge.command(name=_verb)(partial(_bridge, _verb))

for _package_mode in _PACKAGE:
    bridge.command(name=_package_mode)(partial(_package, _package_mode))

for _rail_app in (static, test_app, bridge, api):
    app.command(_rail_app)


def main(argv: list[str] | None = None) -> int:
    configure_logging()  # canonical live-stderr config: diagnostics to stderr, the Envelope wire keeps stdout alone
    return (app() if argv is None else app(argv)) or 0


if __name__ == "__main__":
    raise SystemExit(main())
