"""Rhino bridge client, scenario verify, and RhinoWIP API metadata rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from __future__ import annotations

from pathlib import Path
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import (
    Completed,
    decode_json,
    dotnet,
    dotnet_rail,
    fd_args,
    fold,
    ProcessFault,
    ProjectIndex,
    run,
    Workspace,
)
from tools.quality.settings import ArtifactScope, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type ApiKey = Literal["rhino-common", "rhino-ui", "rhino-code", "rhino-code-remote", "eto", "gh2", "gh2-io"]
type ApiOp = Literal["doctor", "path", "search", "types", "decompile"]
type ApiPathKind = Literal["assembly", "xml"]
type ApiSpec = tuple[str, str, str]
type BridgeStatus = Literal["ok", "skipped", "unsupported", "failed", "timeout", "busy"]
type BuildPolicy = Literal["always", "never"]


# --- [MODELS] ----------------------------------------------------------------------------


class ApiDoctor(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    rhino: dict[str, str]
    ilspy: dict[str, str]
    rhinocode: dict[str, str | int]
    references: tuple[dict[str, object], ...]


class BridgeFault(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    category: str = ""
    message: str = ""
    type: str = ""
    stack_trace: str = ""


class BridgePhase(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    phase: str
    status: BridgeStatus
    duration_ms: float = 0.0
    data: dict[str, object] | None = None
    fault: BridgeFault | None = None


class BridgeResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    schema: str = ""
    command: str = ""
    status: BridgeStatus = "failed"
    report_path: str = ""
    phases: tuple[BridgePhase, ...] = ()
    fault: BridgeFault | None = None

    @staticmethod
    def failed(fault: ProcessFault) -> BridgeResult:
        return BridgeResult(fault=BridgeFault(message=fault.message))

    @property
    def exit_code(self) -> int:
        return _BRIDGE_EXIT[self.status]


class VerifySummaryCounts(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    ok: int
    failed: int
    total: int


class VerifyReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    summary: VerifySummaryCounts
    scenarios: tuple[BridgeResult, ...] = ()

    @property
    def failed(self) -> int:
        return self.summary.failed


# --- [CONSTANTS] -----------------------------------------------------------------------

_API: Final[dict[ApiKey, ApiSpec]] = {
    "eto": ("Eto.dll", "Eto.xml", ".cache/nuget/packages/rhinocommon/*/lib/net8.0/Eto.xml"),
    "gh2": (
        "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll",
        "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml",
        ".cache/nuget/packages/grasshopper2/*/ref/net7.0/Grasshopper2.xml",
    ),
    "gh2-io": (
        "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll",
        "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml",
        ".cache/nuget/packages/grasshopper2/*/ref/net7.0/GrasshopperIO.xml",
    ),
    "rhino-code": ("Rhino.Runtime.Code.dll", "", ""),
    "rhino-code-remote": ("Rhino.Runtime.Code.Remote.dll", "", ""),
    "rhino-common": (
        "RhinoCommon.dll",
        "RhinoCommon.xml",
        ".cache/nuget/packages/rhinocommon/*/lib/net8.0/RhinoCommon.xml",
    ),
    "rhino-ui": ("Rhino.UI.dll", "Rhino.UI.xml", ""),
}
_API_RESOURCE_ROOT: Final[Path] = Path("Contents/Frameworks/RhCore.framework/Versions/A/Resources")
_BRIDGE_EXIT: Final[dict[BridgeStatus, int]] = {
    "busy": 5,
    "failed": 1,
    "ok": 0,
    "skipped": 0,
    "timeout": 5,
    "unsupported": 3,
}


# --- [OPERATIONS] ------------------------------------------------------------------------


def _api_xml_path(root: Path, rhino_app: Path, key: ApiKey) -> tuple[str, str]:
    _, xml_name, fallback_xml = _API[key]
    primary = str(rhino_app / _API_RESOURCE_ROOT / xml_name) if xml_name else ""
    match (primary, Path(primary).is_file() if primary else False):
        case (path, True):
            return path, "primary"
        case _:
            matches = sorted(root.glob(fallback_xml)) if fallback_xml else []
            match matches:
                case [*_, last]:
                    return str(last), "fallback"
                case _:
                    return primary, "missing"


def _verify_discover(workspace: Workspace, root: Path, pattern: str) -> tuple[Path, ...]:
    def direct(candidate: Path) -> tuple[Path, ...]:
        match (candidate.is_file(), candidate.is_dir(), candidate.suffix):
            case (True, _, ".csx") if candidate.name.endswith(".verify.csx"):
                return (candidate.resolve(),)
            case (_, True, _):
                return workspace.paths(
                    fd_args("csx", r"\.verify\.csx$", candidate, exclude=False), cwd=candidate, suffix=".csx"
                )
            case _:
                return ()

    match next((rows for candidate in (Path(pattern), root / pattern) for rows in (direct(candidate),) if rows), ()):
        case ():
            glob = pattern if any(ch in pattern for ch in "/*?[") else f"**/{pattern}"
            return workspace.paths(fd_args("csx", glob, root, exclude=False), suffix=".csx")
        case rows:
            return rows


def _verify_invoke(
    settings: QualitySettings, scope: ArtifactScope, report_dir: Path, project: Path, scenario: Path
) -> BridgeResult:
    result_path = report_dir / f"{scenario.stem.removesuffix('.verify')}.json"

    def once() -> BridgeResult:
        return (
            client_run(
                settings,
                scope,
                "check",
                str(project),
                str(scenario),
                "--result",
                str(result_path),
                check=False,
                build="never",
            )
            .map(
                lambda run: try_decode_bridge(
                    result_path.read_bytes() if result_path.is_file() else (run.stdout or run.stderr)
                ).default_with(BridgeResult.failed)
            )
            .default_with(BridgeResult.failed)
        )

    result = once()
    return result if result.status == "ok" else once()


def _verify_resolve(
    settings: QualitySettings, workspace: Workspace, index: ProjectIndex, scenario: Path
) -> Result[Path, ProcessFault]:
    owner = workspace.owner(index, scenario)
    parts = scenario.resolve().relative_to(settings.root).parts
    match parts:
        case ("tests", "csharp", "libs", project, *rest) if "scenarios" in rest:
            candidate = settings.csharp_lib_project(project)
            return Ok(candidate) if candidate.is_file() else owner
        case _:
            return owner


def _verify_summary(report_dir: Path, results: tuple[BridgeResult, ...]) -> Result[VerifyReport, ProcessFault]:
    ok = sum(result.status == "ok" for result in results)
    payload = VerifyReport(
        summary=VerifySummaryCounts(ok=ok, failed=len(results) - ok, total=len(results)), scenarios=results
    )
    # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=artifact-write ticket=QUALITY-R6
    # expires=2026-12-31 rationale=evidence-boundary
    try:
        (report_dir / "summary.json").write_bytes(msgspec.json.encode(payload))
    except OSError as exc:
        return Error(ProcessFault.fail("verify", "summary", detail=str(exc)))
    return Ok(payload)


def _with_dotnet_apphost(
    argv: tuple[str, ...], *, env: dict[str, str] | None = None, check: bool = True
) -> Result[Completed, ProcessFault]:
    dotnet_bin = (env or {}).get("DOTNET_ROOT", "")
    overlay: dict[str, str] | None
    match Path(dotnet_bin) if dotnet_bin else Path():
        case path if path.is_dir():
            overlay = {**(env or {}), "DOTNET_ROOT": str(path), "DOTNET_MULTILEVEL_LOOKUP": "0"}
        case _:
            overlay = env
    return run(argv, env=overlay, check=check)


@beartype
def api(
    rhino_app: Path,
    op: ApiOp,
    key: ApiKey = "rhino-common",
    *,
    kind: ApiPathKind = "assembly",
    pattern: str = "",
    type_name: str = "",
    settings_root: Path | None = None,
    env: dict[str, str] | None = None,
) -> Result[bytes | str | None, ProcessFault]:
    root = settings_root or QualitySettings.anchor(Path.cwd())
    resources = rhino_app / _API_RESOURCE_ROOT
    assembly_name, _, _ = _API[key]
    assembly = str(resources / assembly_name) if assembly_name else ""
    match op:
        case "doctor":
            plist = rhino_app / "Contents/Info.plist"
            version = (
                run(("plutil", "-extract", "CFBundleVersion", "raw", "-o", "-", str(plist)), check=False)
                .map(lambda done: done.text.strip() or "unknown")
                .default_with(lambda _: "unknown")
            )
            ilspy_meta = (
                _with_dotnet_apphost(("ilspycmd", "--version"), env=env, check=False)
                .map(
                    lambda done: {
                        "status": "ok" if done.returncode == 0 else "failed",
                        "version": done.text.strip() or "unavailable",
                    }
                )
                .default_with(lambda _: {"status": "failed", "version": "unavailable"})
            )
            rhino_code = rhino_app / "Contents/Resources/bin/rhinocode"
            direct, roll = (
                run((str(rhino_code), "list", "--json"), env=overlay, check=False)
                .map(lambda done: done.returncode)
                .default_with(lambda _: -1)
                if rhino_code.is_file()
                else -1
                for overlay in (None, {**(env or {}), "DOTNET_ROLL_FORWARD": "Major"})
            )

            def asset(path: str, label: str) -> dict[str, str]:
                return {"status": label if path and Path(path).is_file() else "missing", "path": path}

            return Ok(
                msgspec.json.encode(
                    ApiDoctor(
                        rhino={"app": str(rhino_app), "version": version},
                        ilspy={**ilspy_meta, "dotnet_root": (env or {}).get("DOTNET_ROOT", "hostfxr-probe")},
                        rhinocode={
                            "status": "ok" if rhino_code.is_file() else "missing",
                            "path": str(rhino_code),
                            "direct": direct,
                            "roll_forward": roll,
                        },
                        references=tuple(
                            dict[str, object](
                                key=api_key,
                                assembly=asset(str(resources / asm) if asm else "", "present"),
                                xml=asset(*_api_xml_path(root, rhino_app, api_key)),
                            )
                            for api_key, (asm, _, _) in _API.items()
                        ),
                    )
                )
            )
        case "path":
            match kind:
                case "assembly":
                    path = assembly
                    missing = f"Missing API assembly for {key}: {assembly}"
                case "xml":
                    path, _ = _api_xml_path(root, rhino_app, key)
                    missing = f"Missing API XML for {key}: {path}"
            return Ok(path).filter_with(
                lambda value: bool(value) and Path(value).is_file(),
                lambda _: ProcessFault.fail("api", key, detail=missing),
            )
        case "search":
            return api(rhino_app, "path", key, kind="xml", settings_root=settings_root, env=env).bind(
                lambda xml: (
                    run(("rg", "-n", "-C", "2", "--", pattern, str(xml)), check=True).map(lambda done: done.text)
                    if isinstance(xml, str)
                    else Error(ProcessFault.fail("api", key, detail=b"Missing xml"))
                )
            )
        case "types":
            return (
                _with_dotnet_apphost(("ilspycmd", "-l", "cisde", assembly), env=env, check=True)
                .filter_with(
                    lambda done: not pattern or pattern in done.text,
                    lambda _: ProcessFault.fail("api", key, detail=b"No types matched"),
                )
                .map(lambda done: done.text)
            )
        case "decompile":
            return _with_dotnet_apphost(("ilspycmd", "-t", type_name, assembly), env=env, check=True).map(
                lambda done: done.text
            )
        case unreachable:
            assert_never(unreachable)


@beartype
def client_run(
    settings: QualitySettings, scope: ArtifactScope, *args: str, check: bool = True, build: BuildPolicy = "always"
) -> Result[Completed, ProcessFault]:
    prelude = (
        dotnet_rail(settings, scope, restore=settings.bridge_client, targets=(settings.bridge_client,))
        if build == "always"
        else Ok(None)
    )
    return prelude.bind(
        lambda _: dotnet(
            "run",
            "--no-build",
            "--project",
            str(settings.bridge_client),
            "--configuration",
            settings.configuration,
            "--",
            *args,
            scope=scope,
            check=check,
        )
    )


@beartype
def run_verify(settings: QualitySettings, scope: ArtifactScope, pattern: str) -> Result[VerifyReport, ProcessFault]:
    root, report_dir, workspace = settings.root, settings.bridge_verify_dir, Workspace(settings.root)
    scenarios = _verify_discover(workspace, root, pattern)
    match scenarios:
        case ():
            return Error(ProcessFault.fail("verify", pattern, detail=b"No *.verify.csx scenarios matched"))
        case _:
            report_dir.mkdir(parents=True, exist_ok=True)
            index = workspace.index()

            def scenario(acc: tuple[BridgeResult, ...], item: Path) -> Result[tuple[BridgeResult, ...], ProcessFault]:
                return Ok(
                    _verify_resolve(settings, workspace, index, item)
                    .map(lambda project: (*acc, _verify_invoke(settings, scope, report_dir, project, item)))
                    .default_with(lambda fault: (*acc, BridgeResult.failed(fault)))
                )

            seed: tuple[BridgeResult, ...] = ()
            return dotnet_rail(settings, scope, restore=settings.bridge_client, targets=(settings.bridge_client,)).bind(
                lambda _: fold(scenarios, seed, scenario).bind(lambda rows: _verify_summary(report_dir, rows))
            )


def try_decode_bridge(payload: bytes | str) -> Result[BridgeResult, ProcessFault]:
    return decode_json(payload, BridgeResult)
