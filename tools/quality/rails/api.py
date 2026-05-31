"""RhinoWIP API metadata introspection rail: doctor, path, xml search, types, decompile."""

# --- [IMPORTS] ------------------------------------------------------------------------

from pathlib import Path
import shutil
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import Completed, ProcessFault, run


# --- [TYPES] ---------------------------------------------------------------------------

type ApiKey = Literal["rhino-common", "rhino-ui", "rhino-code", "rhino-code-remote", "eto", "gh2", "gh2-io"]
type ApiOp = Literal["doctor", "path", "search", "types", "decompile"]
type ApiPathKind = Literal["assembly", "xml"]
type ApiSpec = tuple[str, str]


# --- [MODELS] ----------------------------------------------------------------------------


class ApiDoctor(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    rhino: dict[str, str]
    ilspy: dict[str, str]
    rhinocode: dict[str, str | int]
    references: tuple[dict[str, object], ...]


# --- [CONSTANTS] -----------------------------------------------------------------------

_API: Final[dict[ApiKey, ApiSpec]] = {
    "eto": ("Eto.dll", "Eto.xml"),
    "gh2": ("ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml"),
    "gh2-io": ("ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml"),
    "rhino-code": ("Rhino.Runtime.Code.dll", ""),
    "rhino-code-remote": ("Rhino.Runtime.Code.Remote.dll", ""),
    "rhino-common": ("RhinoCommon.dll", "RhinoCommon.xml"),
    "rhino-ui": ("Rhino.UI.dll", "Rhino.UI.xml"),
}
_API_RESOURCE_ROOT: Final[Path] = Path("Contents/Frameworks/RhCore.framework/Versions/Current/Resources")


# --- [OPERATIONS] ------------------------------------------------------------------------


def _api_xml_path(rhino_app: Path, key: ApiKey) -> tuple[str, str]:
    _, xml_name = _API[key]
    primary = str(rhino_app / _API_RESOURCE_ROOT / xml_name) if xml_name else ""
    return (primary, "primary") if primary and Path(primary).is_file() else (primary, "missing")


def _with_dotnet_apphost(argv: tuple[str, ...], *, env: dict[str, str] | None = None, check: bool = True) -> Result[Completed, ProcessFault]:
    dotnet = shutil.which("dotnet")
    candidates = ((env or {}).get("DOTNET_ROOT", ""), str(Path(dotnet).resolve().parent) if dotnet else "", "/usr/local/share/dotnet")
    overlay: dict[str, str] | None
    match next((root for root in candidates if root and (Path(root) / "shared").is_dir()), ""):
        case "":
            overlay = env
        case root:
            overlay = {**(env or {}), "DOTNET_ROOT": root, "DOTNET_MULTILEVEL_LOOKUP": "0"}
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
    env: dict[str, str] | None = None,
) -> Result[bytes | str | None, ProcessFault]:
    resources = rhino_app / _API_RESOURCE_ROOT
    assembly_name, _ = _API[key]
    assembly = str(resources / assembly_name) if assembly_name else ""
    match op:
        case "doctor":
            base_env = env or {}
            plist = rhino_app / "Contents/Info.plist"
            version = (
                run(("plutil", "-extract", "CFBundleVersion", "raw", "-o", "-", str(plist)), check=False)
                .map(lambda done: done.text.strip() or "unknown")
                .default_with(lambda _: "unknown")
            )
            ilspy_meta = (
                _with_dotnet_apphost(("ilspycmd", "--version"), env=env, check=False)
                .map(lambda done: {"status": "ok" if done.returncode == 0 else "failed", "version": done.text.strip() or "unavailable"})
                .default_with(lambda _: {"status": "failed", "version": "unavailable"})
            )
            rhino_code = rhino_app / "Contents/Resources/bin/rhinocode"
            direct, roll = (
                run((str(rhino_code), "list", "--json"), env=overlay, check=False).map(lambda done: done.returncode).default_with(lambda _: -1)
                if rhino_code.is_file()
                else -1
                for overlay in (None, {**base_env, "DOTNET_ROLL_FORWARD": "Major"})
            )

            def asset(path: str, label: str) -> dict[str, str]:
                return {"status": label if path and Path(path).is_file() else "missing", "path": path}

            return Ok(
                msgspec.json.encode(
                    ApiDoctor(
                        rhino={"app": str(rhino_app), "version": version},
                        ilspy={**ilspy_meta, "dotnet_root": base_env.get("DOTNET_ROOT", "hostfxr-probe")},
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
                                xml=asset(*_api_xml_path(rhino_app, api_key)),
                            )
                            for api_key, (asm, _) in _API.items()
                        ),
                    )
                )
            )
        case "path":
            xml_path, _ = _api_xml_path(rhino_app, key)
            path, missing = {
                "assembly": (assembly, f"Missing API assembly for {key}: {assembly}"),
                "xml": (xml_path, f"Missing API XML for {key}: {xml_path}"),
            }[kind]
            return Ok(path).filter_with(lambda value: bool(value) and Path(value).is_file(), lambda _: ProcessFault.fail("api", key, detail=missing))
        case "search":
            return api(rhino_app, "path", key, kind="xml", env=env).bind(
                lambda xml: (
                    run(("rg", "-n", "-C", "2", "--", pattern, str(xml)), check=True).map(lambda done: done.text)
                    if isinstance(xml, str)
                    else Error(ProcessFault.fail("api", key, detail=b"Missing xml"))
                )
            )
        case "types":
            return (
                _with_dotnet_apphost(("ilspycmd", "-l", "cisde", assembly), env=env, check=True)
                .filter_with(lambda done: not pattern or pattern in done.text, lambda _: ProcessFault.fail("api", key, detail=b"No types matched"))
                .map(lambda done: done.text)
            )
        case "decompile":
            return _with_dotnet_apphost(("ilspycmd", "-t", type_name, assembly), env=env, check=True).map(lambda done: done.text)
        case unreachable:
            assert_never(unreachable)
