"""Agent-safe API metadata rail for RhinoWIP host assemblies and NuGet packages."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Callable, Iterable
from dataclasses import dataclass
import hashlib
from pathlib import Path
import re
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import Completed, dotnet_apphost_env, dotnet_tool, dotnet_tool_restore, ProcessFault, run, xml_iter, xml_root
from tools.quality.settings import ArtifactScope, PROJECT_EXCLUDE_DIRS, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type ApiOp = Literal["doctor", "resolve", "query", "show"]
type ApiPathKind = Literal["all", "assembly", "xml", "nuspec", "deps", "package-root"]
type ApiRestoreMode = Literal["never", "missing", "always"]
type ApiSourceKind = Literal["host", "package"]
type ApiStatus = Literal["ok", "empty", "missing", "failed"]
type ApiShowPolicy = Literal["current", "latest"]
type ApiShape = Literal["index", "namespace", "type", "member", "search"]
type ApiSpec = tuple[str, str]


# --- [MODELS] ----------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class _ApiSource:
    key: str
    kind: ApiSourceKind
    assembly: Path | None
    xml: Path | None
    assemblies: tuple[Path, ...] = ()
    xmls: tuple[Path, ...] = ()
    version: str = ""
    package: str = ""
    package_root: Path | None = None
    nuspec: Path | None = None
    asset_paths: tuple[Path, ...] = ()
    frameworks: tuple[str, ...] = ()
    owners: tuple[str, ...] = ()


@dataclass(frozen=True, slots=True)
class _Surface:
    # XML-free type/namespace index parsed from `ilspycmd -l cisde`; cached per (key, assembly fingerprint) so Index,
    # Namespace, and Search shapes stay cheap and warm across runs while only Type/Member pay the per-type decompile cost.
    types: tuple[str, ...]
    namespaces: tuple[str, ...]
    by_namespace: dict[str, tuple[str, ...]]
    artifact: Path


@dataclass(frozen=True, slots=True)
class _Shape:
    shape: ApiShape
    symbol: str = ""
    member: str = ""


class ApiArtifact(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    id: str
    kind: str
    path: str
    bytes: int = 0
    lines: int = 0


class ApiMatch(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    id: str
    kind: str
    text: str
    line: int = 0
    score: int = 0


class ApiQueryReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, tag="query"):
    query: dict[str, str]
    source: dict[str, str]
    status: ApiStatus
    counts: dict[str, int]
    artifact_paths: dict[str, str]
    shape: ApiShape = "search"
    signature: str = ""
    doc: str = ""
    results: tuple[ApiMatch, ...] = ()
    artifacts: tuple[ApiArtifact, ...] = ()
    notes: tuple[str, ...] = ()
    preview: str = ""
    truncated: bool = False


class ApiDoctorReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, tag="doctor"):
    query: dict[str, str]
    status: ApiStatus
    rhino: dict[str, str]
    ilspy: dict[str, str]
    rhinocode: dict[str, str | int]
    counts: dict[str, int]
    artifact_paths: dict[str, str]
    sources: tuple[dict[str, str], ...]
    artifacts: tuple[ApiArtifact, ...] = ()
    notes: tuple[str, ...] = ()


class ApiShowReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    query: dict[str, str]
    status: ApiStatus
    artifact: ApiArtifact
    preview: str
    counts: dict[str, int]
    content: str = ""
    truncated: bool = False


type ApiStoredReport = ApiQueryReport | ApiDoctorReport


# --- [CONSTANTS] -----------------------------------------------------------------------

_API: Final[dict[str, ApiSpec]] = {
    "eto": ("Eto.dll", "Eto.xml"),
    "gh2": ("ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml"),
    "gh2-io": ("ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml"),
    "rhino-code": ("Rhino.Runtime.Code.dll", ""),
    "rhino-code-remote": ("Rhino.Runtime.Code.Remote.dll", ""),
    "rhino-common": ("RhinoCommon.dll", "RhinoCommon.xml"),
    "rhino-ui": ("Rhino.UI.dll", "Rhino.UI.xml"),
}
_API_RESOURCE_ROOT: Final[Path] = Path("Contents/Frameworks/RhCore.framework/Versions/Current/Resources")
_API_ARTIFACT_ROOT: Final[str] = ".artifacts/quality/api"
_DIRECTORY_PACKAGES: Final[str] = "Directory.Packages.props"
_FRAMEWORK_RANK: Final[tuple[str, ...]] = ("net10.0", "net9.0", "net8.0", "net7.0", "net6.0", "netstandard2.1", "netstandard2.0", "netcoreapp3.1")
_PREVIEW_LIMIT: Final[int] = 12
_SHOW_LINES: Final[int] = 120
_ASSET_DIRS: Final[tuple[str, ...]] = ("lib", "ref", "runtimes", "build", "buildTransitive", "analyzers", "tools")
_SURFACE_KINDS: Final[frozenset[str]] = frozenset(("Class", "Struct", "Interface", "Delegate", "Enum"))
_ILSPY_LIST: Final[str] = "cisde"


# --- [OPERATIONS] ------------------------------------------------------------------------


def _artifact_id(*parts: str) -> str:
    return hashlib.sha256("|".join(parts).encode()).hexdigest()[:12]


def _artifact(root: Path, kind: str, content: bytes | str) -> ApiArtifact:
    match content:
        case bytes() as raw:
            pass
        case str() as text:
            raw = text.encode()
    path = root / kind
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(raw)
    return ApiArtifact(id=_artifact_id(str(path), kind), kind=kind, path=str(path), bytes=len(raw), lines=raw.count(b"\n"))


def _artifact_of(path: Path, kind: str) -> ApiArtifact:
    raw = path.read_bytes() if path.is_file() else b""
    return ApiArtifact(id=_artifact_id(str(path), kind), kind=kind, path=str(path), bytes=len(raw), lines=raw.count(b"\n"))


def _artifact_root(scope: ArtifactScope, op: ApiOp, key: str, query: str = "") -> Path:
    name = re.sub(r"[^A-Za-z0-9_.-]+", "-", f"{op}-{key}-{query}" if query else f"{op}-{key}").strip("-")
    return scope.path / name


def _packages(settings: QualitySettings) -> dict[str, str]:
    match xml_root(settings.root / _DIRECTORY_PACKAGES):
        case None:
            return {}
        case root:
            return {
                include: version
                for node in xml_iter(root, "PackageVersion")
                for include, version in ((node.attrib.get("Include", ""), node.attrib.get("Version", "")),)
                if include and version
            }


def _package_references(settings: QualitySettings) -> dict[str, tuple[str, ...]]:
    projects = tuple(path for path in settings.root.glob("**/*.csproj") if not any(part in PROJECT_EXCLUDE_DIRS for part in path.parts))

    def refs(project: Path) -> tuple[str, ...]:
        match xml_root(project):
            case None:
                return ()
            case root:
                return tuple(include for node in xml_iter(root, "PackageReference") for include in (node.attrib.get("Include", ""),) if include)

    pairs = tuple((package, str(project.relative_to(settings.root))) for project in projects for package in refs(project))
    return {
        package: tuple(sorted(project for item, project in pairs if item.casefold() == package.casefold()))
        for package in sorted({package for package, _ in pairs}, key=str.casefold)
    }


def _host_source(settings: QualitySettings, key: str) -> Result[_ApiSource | None, ProcessFault]:
    spec = _API.get(key)
    if spec is None:
        return Ok(None)
    assembly_name, xml_name = spec
    try:
        resources = settings.require_rhino_app() / _API_RESOURCE_ROOT
    except ValueError as exc:
        return Error(ProcessFault.fail("api", key, detail=str(exc)))
    assembly = resources / assembly_name if assembly_name else None
    xml = resources / xml_name if xml_name else None
    assemblies = (assembly,) if assembly is not None else ()
    xmls = (xml,) if xml is not None else ()
    return Ok(_ApiSource(key=key, kind="host", assembly=assembly, xml=xml, assemblies=assemblies, xmls=xmls))


def _restore_package(settings: QualitySettings, scope: ArtifactScope, package: str, version: str) -> Result[None, ProcessFault]:
    probe = scope.path / "nuget-probe" / package / "probe.csproj"
    probe.parent.mkdir(parents=True, exist_ok=True)
    probe.write_text(
        "\n".join((
            '<Project Sdk="Microsoft.NET.Sdk">',
            "  <PropertyGroup>",
            "    <TargetFramework>net10.0</TargetFramework>",
            "    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>",
            "    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>",
            "    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>",
            "  </PropertyGroup>",
            "  <ItemGroup>",
            f'    <PackageReference Include="{package}" Version="{version}" />',
            "  </ItemGroup>",
            "</Project>",
            "",
        )),
        encoding="utf-8",
    )
    return run(
        (
            "dotnet",
            "restore",
            str(probe),
            "--packages",
            str(settings.root / ".cache/nuget/packages"),
            "-p:ImportDirectoryBuildProps=false",
            "-p:ImportDirectoryBuildTargets=false",
            "-p:ManagePackageVersionsCentrally=false",
        ),
        env=scope.dotnet_env,
        check=True,
    ).map(lambda _: None)


def _framework_dir(root: Path, asset_kind: Literal["lib", "ref"] = "lib") -> Path | None:
    asset_root = root / asset_kind
    frameworks = tuple(path for path in asset_root.iterdir() if path.is_dir()) if asset_root.is_dir() else ()
    ranked = (
        *(path for name in _FRAMEWORK_RANK for path in frameworks if path.name == name),
        *sorted(path for path in frameworks if path.name not in _FRAMEWORK_RANK),
    )
    return next(iter(ranked), None)


def _package_root(settings: QualitySettings, package: str, version: str) -> Path:
    candidates = (
        settings.root / ".cache/nuget/packages" / package.casefold() / version,
        Path.home() / ".nuget/packages" / package.casefold() / version,
    )
    return next((candidate for candidate in candidates if candidate.is_dir()), candidates[0])


def _package_asset_paths(root: Path) -> tuple[Path, ...]:
    return tuple(
        sorted(path for directory in _ASSET_DIRS for base in (root / directory,) if base.exists() for path in base.rglob("*") if path.is_file())
    )


def _frameworks(root: Path) -> tuple[str, ...]:
    libs = root / "lib"
    refs = root / "ref"
    return tuple(sorted({path.name for base in (libs, refs) if base.is_dir() for path in base.iterdir() if path.is_dir()}))


def _nuget_source(
    settings: QualitySettings, scope: ArtifactScope, key: str, *, restore: ApiRestoreMode = "missing"
) -> Result[_ApiSource, ProcessFault]:
    # Categorical fuzzy resolution: exact casefold id, else unique casefold prefix, else unique substring; ambiguity is a
    # typed Error listing candidates so `languageext`, `avalonia.datagrid`, and every future central pin stay addressable.
    packages = _packages(settings)
    casefold = key.casefold()
    tokens = tuple(token for token in re.split(r"[^a-z0-9]+", casefold) if token)
    exact = tuple(name for name in packages if name.casefold() == casefold)
    fuzzy = (
        tuple(name for name in packages if name.casefold().startswith(casefold))
        or tuple(name for name in packages if casefold in name.casefold())
        or tuple(name for name in packages if tokens and all(token in name.casefold() for token in tokens))
    )
    candidates = exact or fuzzy
    resolved: Result[str, ProcessFault] = (
        Ok(candidates[0])
        if exact or len(candidates) == 1
        else Error(ProcessFault.fail("api", key, detail=f"Unknown API source: {key}"))
        if not candidates
        else Error(ProcessFault.fail("api", key, detail=f"Ambiguous key '{key}': {', '.join(sorted(candidates))}"))
    )

    def build(package: str) -> Result[_ApiSource, ProcessFault]:
        version = packages[package]
        root = _package_root(settings, package, version)
        ready = (
            Error(ProcessFault.fail("api", package, detail=f"Package is not restored: {root}"))
            if restore == "never" and not root.is_dir()
            else _restore_package(settings, scope, package, version)
            if restore == "always" or not root.is_dir()
            else Ok(None)
        )

        def source(_: None) -> Result[_ApiSource, ProcessFault]:
            selected_ref = _framework_dir(root, "ref")
            selected_lib = _framework_dir(root, "lib")
            selected = selected_ref or selected_lib
            assets = _package_asset_paths(root) if root.is_dir() else ()
            nuspec = next(iter(sorted(root.glob("*.nuspec"))), None) if root.is_dir() else None
            owners = _package_references(settings).get(package, ())
            match selected:
                case Path() as framework:
                    compile_assemblies = tuple(sorted(framework.glob("*.dll")))
                    runtime_assemblies = tuple(sorted(selected_lib.glob("*.dll"))) if selected_lib is not None else ()
                    assemblies = tuple(dict.fromkeys((*compile_assemblies, *runtime_assemblies)))
                    assembly = next((path for path in assemblies if path.stem.casefold() == package.casefold()), None) or next(iter(assemblies), None)
                    xmls = tuple(path.with_suffix(".xml") for path in assemblies if path.with_suffix(".xml").is_file())
                    xml = next((path for path in xmls if assembly is not None and path.stem == assembly.stem), None) or next(iter(xmls), None)
                    return Ok(
                        _ApiSource(
                            key=package,
                            kind="package",
                            assembly=assembly,
                            xml=xml,
                            assemblies=assemblies,
                            xmls=xmls,
                            version=version,
                            package=package,
                            package_root=root,
                            nuspec=nuspec,
                            asset_paths=assets,
                            frameworks=_frameworks(root),
                            owners=owners,
                        )
                    )
                case None:
                    return Ok(
                        _ApiSource(
                            key=package,
                            kind="package",
                            assembly=None,
                            xml=None,
                            version=version,
                            package=package,
                            package_root=root,
                            nuspec=nuspec,
                            asset_paths=assets,
                            frameworks=_frameworks(root),
                            owners=owners,
                        )
                    )

        return ready.bind(source)

    return resolved.bind(build)


def _source(settings: QualitySettings, scope: ArtifactScope, key: str, *, restore: ApiRestoreMode = "missing") -> Result[_ApiSource, ProcessFault]:
    return _host_source(settings, key).bind(lambda host: Ok(host) if host is not None else _nuget_source(settings, scope, key, restore=restore))


def _present(path: Path | None) -> str:
    return "present" if path is not None and path.is_file() else "missing"


def _source_payload(source: _ApiSource) -> dict[str, str]:
    return {
        "key": source.key,
        "kind": source.kind,
        "package": source.package,
        "version": source.version,
        "assembly": str(source.assembly) if source.assembly is not None else "",
        "xml": str(source.xml) if source.xml is not None else "",
        "assemblies": str(len(source.assemblies)),
        "xmls": str(len(source.xmls)),
        "package_root": str(source.package_root) if source.package_root is not None else "",
        "nuspec": str(source.nuspec) if source.nuspec is not None else "",
        "assets": str(len(source.asset_paths)),
        "frameworks": ",".join(source.frameworks),
        "owners": ",".join(source.owners),
    }


def _report_artifacts(root: Path, report: ApiStoredReport, *artifacts: ApiArtifact) -> bytes:
    # One generic finalizer: splice the report.json path + accumulated artifacts onto any report struct via structs.replace.
    report_artifact = _artifact(root, "report.json", msgspec.json.encode(report))
    final = msgspec.structs.replace(
        report, artifact_paths={**report.artifact_paths, "report": report_artifact.path}, artifacts=(*report.artifacts, *artifacts, report_artifact)
    )
    final_payload = msgspec.json.encode(final)
    Path(report_artifact.path).write_bytes(final_payload)
    return final_payload


def _line_number(line: str) -> int:
    match line.split(":", maxsplit=2):
        case [raw, *_] if raw.isdigit():
            return int(raw)
        case [_, raw, *_] if raw.isdigit():
            return int(raw)
        case _:
            return 0


def _matches(lines: Iterable[str], *, kind: str, pattern: str) -> tuple[ApiMatch, ...]:
    query = pattern.casefold()

    def score(text: str) -> int:
        return (100 if query and query in text.casefold() else 0) + max(0, 40 - len(text) // 4)

    ranked = tuple(sorted((line for line in lines if not pattern or query in line.casefold()), key=lambda item: (-score(item), item)))
    return tuple(
        ApiMatch(id=f"{kind}-{index:03d}", kind=kind, text=line[:320], line=_line_number(line), score=score(line))
        for index, line in enumerate(ranked[:_PREVIEW_LIMIT], start=1)
    )


def _xml_members(path: Path, pattern: str) -> tuple[str, ...]:
    match xml_root(path):
        case None:
            return ()
        case root:
            return tuple(
                f"{member.attrib.get('name', '')} {''.join(member.itertext()).strip()[:480]}"
                for member in xml_iter(root, "member")
                if not pattern
                or pattern.casefold() in member.attrib.get("name", "").casefold()
                or pattern.casefold() in "".join(member.itertext()).casefold()
            )


def _xml_search(source: _ApiSource, pattern: str) -> tuple[str, ...]:
    return tuple(line for path in source.xmls if path.is_file() for line in _xml_members(path, pattern))


def _xml_doc(source: _ApiSource, shape: _Shape) -> str:
    match shape.shape:
        case "type" | "member" | "search":
            needle = f"{shape.symbol}.{shape.member}" if shape.shape == "member" else shape.symbol
            return next(iter(_xml_search(source, needle)), "")
        case _:
            return ""


def _surface_cache_path(scope: ArtifactScope, source: _ApiSource, assemblies: tuple[Path, ...]) -> Path:
    seed = "|".join(f"{path}:{path.stat().st_mtime_ns}:{path.stat().st_size}" for path in assemblies)
    digest = hashlib.sha256(seed.encode()).hexdigest()[:16]
    safe = re.sub(r"[^A-Za-z0-9_.-]+", "-", source.key).strip("-") or "source"
    return scope.root / _API_ARTIFACT_ROOT / "surface" / f"{safe}.{digest}.txt"


def _namespace_of(fqn: str, types: frozenset[str]) -> str:
    parts = fqn.split(".")
    return next((".".join(parts[:index]) for index in range(len(parts)) if ".".join(parts[: index + 1]) in types), ".".join(parts[:-1]))


def _parse_surface(text: str, cache: Path) -> _Surface:
    types = tuple(
        dict.fromkeys(
            parts[1]
            for line in text.splitlines()
            if line.strip() and not line.startswith("# ")
            for parts in (line.split(maxsplit=1),)
            if len(parts) == 2 and parts[0] in _SURFACE_KINDS
        )
    )
    namespace_of = {fqn: _namespace_of(fqn, frozenset(types)) for fqn in types}
    namespaces = tuple(sorted({namespace for namespace in namespace_of.values() if namespace}))
    return _Surface(
        types=types,
        namespaces=namespaces,
        by_namespace={namespace: tuple(fqn for fqn in types if namespace_of[fqn] == namespace) for namespace in namespaces},
        artifact=cache,
    )


def _surface(scope: ArtifactScope, source: _ApiSource) -> Result[_Surface, ProcessFault]:
    assemblies = tuple(path for path in source.assemblies if path.is_file())
    match assemblies:
        case ():
            return Error(ProcessFault.fail("api", "surface", source.key, detail=b"assembly artifact is missing for source"))
        case _:
            pass
    cache = _surface_cache_path(scope, source, assemblies)
    match cache.is_file():
        case True:
            return Ok(_parse_surface(cache.read_text(encoding="utf-8", errors="replace"), cache))
        case False:
            pass
    attempts = tuple(
        dotnet_tool(scope, "ilspycmd", "-l", _ILSPY_LIST, str(assembly), check=False).default_value(
            Completed(argv=("ilspycmd", "-l", _ILSPY_LIST, str(assembly)), returncode=1, stdout=b"", stderr=b"ilspy failed")
        )
        for assembly in assemblies
    )
    match any(done.returncode == 0 for done in attempts):
        case False:
            stderr = "\n".join(done.stderr.decode(errors="replace") for done in attempts if done.stderr)
            return Error(ProcessFault.fail("api", "surface", source.key, detail=stderr or "ilspy type listing failed"))
        case True:
            pass
    text = "\n".join(f"# {assemblies[index]}\n{done.stdout.decode(errors='replace')}" for index, done in enumerate(attempts) if done.stdout)
    cache.parent.mkdir(parents=True, exist_ok=True)
    cache.write_text(text, encoding="utf-8")
    return Ok(_parse_surface(text, cache))


def _rank_type(types: tuple[str, ...], needle: str) -> str:
    casefold = needle.casefold()
    exact = tuple(fqn for fqn in types if fqn.casefold() == casefold)
    suffix = tuple(fqn for fqn in types if fqn.casefold().endswith("." + casefold))
    return next(iter((*exact, *sorted(suffix, key=lambda fqn: len(fqn)))), "")


def _resolve_symbol(surface: _Surface, symbol: str) -> _Shape:
    match symbol.strip():
        case "":
            return _Shape("index")
        case needle:
            pass
    casefold = needle.casefold()
    namespace = next((name for name in surface.namespaces if name.casefold() == casefold), "")
    match (namespace, _rank_type(surface.types, needle)):
        case (str(name), _) if name:
            return _Shape("namespace", name)
        case (_, str(fqn)) if fqn:
            return _Shape("type", fqn)
        case _:
            pass
    head, _, tail = needle.rpartition(".")
    owner = _rank_type(surface.types, head) if head and tail else ""
    return _Shape("member", owner, tail) if owner else _Shape("search", needle)


def _query_report(
    root: Path,
    *,
    source: _ApiSource,
    shape: _Shape,
    status: ApiStatus,
    counts: dict[str, int],
    artifacts: tuple[ApiArtifact, ...],
    signature: str = "",
    doc: str = "",
    results: tuple[ApiMatch, ...] = (),
    notes: tuple[str, ...] = (),
    preview: str = "",
    truncated: bool = False,
) -> bytes:
    return _report_artifacts(
        root,
        ApiQueryReport(
            query={"op": "query", "key": source.key, "shape": shape.shape, "symbol": shape.symbol, "member": shape.member},
            source=_source_payload(source),
            status=status,
            counts=counts,
            artifact_paths={artifact.kind: artifact.path for artifact in artifacts},
            shape=shape.shape,
            signature=signature,
            doc=doc[:480],
            results=results,
            artifacts=artifacts,
            notes=notes,
            preview=preview,
            truncated=truncated,
        ),
    )


def _surface_report(
    root: Path,
    source: _ApiSource,
    surface: _Surface,
    surface_artifact: ApiArtifact,
    shape: _Shape,
    rows: tuple[str, ...],
    *,
    kind: str,
    doc: str,
    counts: dict[str, int],
    pattern: str,
) -> bytes:
    listing = _artifact(root, f"{shape.shape}.txt", "\n".join(rows))
    return _query_report(
        root,
        source=source,
        shape=shape,
        status="ok" if rows else "empty",
        counts=counts,
        signature=shape.symbol,
        doc=doc,
        preview="\n".join(rows[:_PREVIEW_LIMIT]),
        truncated=len(rows) > _PREVIEW_LIMIT,
        artifacts=(surface_artifact, listing),
        results=_matches(rows, kind=kind, pattern=pattern),
        notes=(f"full {shape.shape} listing stored in {shape.shape}.txt", f"{len(surface.types)} types across {len(surface.namespaces)} namespaces"),
    )


def _decompile(scope: ArtifactScope, source: _ApiSource, type_name: str) -> Result[tuple[Path, Completed], ProcessFault]:
    assemblies = tuple(
        sorted((path for path in source.assemblies if path.is_file()), key=lambda path: ("/ref/" in path.as_posix(), path.as_posix().casefold()))
    )
    match assemblies:
        case ():
            return Error(ProcessFault.fail("api", "decompile", source.key, detail=b"assembly artifact is missing for source"))
        case _:
            pass
    # --no-dead-code/--no-dead-stores strip compiler-artifact bodies so the signature anchor lands on the real declaration.
    attempts = tuple(
        (
            assembly,
            dotnet_tool(scope, "ilspycmd", "-t", type_name, "--no-dead-code", "--no-dead-stores", str(assembly), check=False).default_value(
                Completed(argv=("ilspycmd", "-t", type_name, str(assembly)), returncode=1, stdout=b"", stderr=b"ilspy failed")
            ),
        )
        for assembly in assemblies
    )
    return Ok(next(((assembly, done) for assembly, done in attempts if done.returncode == 0 and done.stdout), attempts[0]))


def _decompile_report(
    root: Path,
    source: _ApiSource,
    surface_artifact: ApiArtifact,
    shape: _Shape,
    decompiled: tuple[Path, Completed],
    *,
    doc: str,
    max_lines: int,
    full: bool,
    grep: str,
) -> bytes:
    assembly, completed = decompiled
    text = completed.stdout.decode(errors="replace")
    lines = tuple(line for line in text.splitlines() if not grep or grep.casefold() in line.casefold())
    simple = shape.member or shape.symbol.rsplit(".", maxsplit=1)[-1]
    # Anchor on the declaration of `simple` as a whole identifier, skipping XML doc comments: `\bWeld\b` hits `public void Weld(`,
    # never `Unweld` or a `/// ...welded...` summary; type shapes anchor on the `class <Name>` line rather than the leading usings.
    boundary = re.compile(rf"\b{re.escape(simple)}\b", re.IGNORECASE)
    match full:
        case True:
            window = lines
        case False:
            anchor = next((offset for offset, line in enumerate(lines) if boundary.search(line) and not line.lstrip().startswith("///")), 0)
            window = lines[anchor : anchor + max_lines]
    signature = next(
        (line.strip() for line in window if boundary.search(line) and not line.lstrip().startswith("///")), next(iter(window), "").strip()
    )
    decompile_artifact = _artifact(root, "decompile.cs", text)
    preview_artifact = _artifact(root, "source.preview.cs", "\n".join(window))
    label = f"{shape.symbol}.{shape.member}" if shape.shape == "member" else shape.symbol
    return _query_report(
        root,
        source=source,
        shape=shape,
        status="ok" if completed.returncode == 0 and text else "failed",
        counts={"lines": text.count("\n"), "preview_lines": len(window), "selected_lines": len(lines), "assemblies": len(source.assemblies)},
        signature=signature,
        doc=doc,
        preview="\n".join(window),
        truncated=not full and len(lines) > len(window),
        artifacts=(surface_artifact, decompile_artifact, preview_artifact),
        results=(ApiMatch(id="decompile-001", kind=shape.shape, text=label, score=100),),
        notes=(
            f"selected assembly: {assembly}",
            "full decompiled source is stored in decompile.cs",
            "bounded source preview is stored in source.preview.cs",
        ),
    )


def _render_query(
    scope: ArtifactScope, source: _ApiSource, surface: _Surface, shape: _Shape, *, max_lines: int, full: bool, grep: str
) -> Result[bytes, ProcessFault]:
    root = _artifact_root(scope, "query", source.key, shape.symbol or shape.shape)
    surface_artifact = _artifact_of(surface.artifact, "surface.txt")
    doc = _xml_doc(source, shape)
    match shape.shape:
        case "type" | "member":
            return _decompile(scope, source, shape.symbol).map(
                lambda decompiled: _decompile_report(
                    root, source, surface_artifact, shape, decompiled, doc=doc, max_lines=max_lines, full=full, grep=grep
                )
            )
        case "index":
            rows, kind, pattern, counts = surface.namespaces, "namespace", grep, {"namespaces": len(surface.namespaces), "types": len(surface.types)}
        case "namespace":
            owned = surface.by_namespace.get(shape.symbol, ())
            rows, kind, pattern, counts = owned, "type", grep, {"types": len(owned)}
        case "search":
            hits = (*tuple(fqn for fqn in surface.types if shape.symbol.casefold() in fqn.casefold()), *_xml_search(source, shape.symbol))
            rows, kind, pattern, counts = hits, "type", shape.symbol, {"matches": len(hits), "types": len(surface.types)}
        case _ as unreachable:
            assert_never(unreachable)
    return Ok(_surface_report(root, source, surface, surface_artifact, shape, rows, kind=kind, doc=doc, counts=counts, pattern=pattern))


def _query(
    settings: QualitySettings, scope: ArtifactScope, key: str, symbol: str, *, max_lines: int, full: bool, grep: str, restore: ApiRestoreMode
) -> Result[bytes, ProcessFault]:
    return _source(settings, scope, key, restore=restore).bind(
        lambda source: dotnet_tool_restore(scope).bind(
            lambda _: _surface(scope, source).bind(
                lambda surface: _render_query(scope, source, surface, _resolve_symbol(surface, symbol), max_lines=max_lines, full=full, grep=grep)
            )
        )
    )


def _resolve_targets(source: _ApiSource, kind: ApiPathKind) -> tuple[Path, ...]:
    catalog: dict[ApiPathKind, tuple[Path, ...]] = {
        "all": (*source.assemblies, *source.xmls, *((source.nuspec,) if source.nuspec is not None else ()), *source.asset_paths),
        "assembly": source.assemblies,
        "xml": source.xmls,
        "nuspec": (source.nuspec,) if source.nuspec is not None else (),
        "deps": tuple(
            path for path in source.asset_paths if any(part in {"build", "buildTransitive", "analyzers", "tools", "runtimes"} for part in path.parts)
        ),
        "package-root": (source.package_root,) if source.package_root is not None else (),
    }
    return tuple(dict.fromkeys(catalog[kind]))


def _resolve_report(root: Path, source: _ApiSource, kind: ApiPathKind) -> bytes:
    targets = _resolve_targets(source, kind)
    existing = tuple(path for path in targets if path.exists())
    artifact = _artifact(root, f"{kind}.paths.txt", "\n".join(str(path) for path in targets))
    return _report_artifacts(
        root,
        ApiQueryReport(
            query={"op": "resolve", "key": source.key, "kind": kind},
            source=_source_payload(source),
            status="ok" if existing else "missing",
            counts={
                "paths": len(existing),
                "declared": len(targets),
                "assemblies": len(source.assemblies),
                "xmls": len(source.xmls),
                "assets": len(source.asset_paths),
                "frameworks": len(source.frameworks),
            },
            artifact_paths={artifact.kind: artifact.path},
            results=tuple(
                ApiMatch(id=_artifact_id(source.key, kind, str(path)), kind=kind, text=str(path), score=100 if path.exists() else 0)
                for path in targets[:_PREVIEW_LIMIT]
            ),
            artifacts=(artifact,),
            notes=("full resolved path list is stored in the paths artifact",),
        ),
    )


def _inventory_sources(settings: QualitySettings) -> tuple[_ApiSource, ...]:
    rhino_app = settings.rhino_app
    host_sources = tuple(
        _ApiSource(key=key, kind="host", assembly=None, xml=None) if rhino_app is None else source
        for key in _API
        for source in (_host_source(settings, key).default_value(_ApiSource(key=key, kind="host", assembly=None, xml=None)),)
        if source is not None
    )
    owners = _package_references(settings)
    nuget_sources = tuple(
        _ApiSource(key=package, kind="package", assembly=None, xml=None, version=version, package=package, owners=owners.get(package, ()))
        for package, version in _packages(settings).items()
    )
    return (*host_sources, *nuget_sources)


def _doctor(settings: QualitySettings, scope: ArtifactScope, root: Path, env: dict[str, str] | None) -> Result[bytes, ProcessFault]:
    base_env = env or {}
    apphost_env = dotnet_apphost_env(env)
    rhino_app = settings.rhino_app
    plist = rhino_app / "Contents/Info.plist" if rhino_app is not None else Path()
    version = (
        run(("plutil", "-extract", "CFBundleVersion", "raw", "-o", "-", str(plist)), check=False)
        .map(lambda done: done.text.strip() or "unknown")
        .default_with(lambda _: "missing")
        if rhino_app is not None
        else "missing"
    )
    ilspy_done = (
        dotnet_tool_restore(scope)
        .bind(lambda _: dotnet_tool(scope, "ilspycmd", "--version"))
        .default_value(Completed(argv=("ilspycmd", "--version"), returncode=1, stdout=b"", stderr=b"unavailable"))
    )
    rhino_code = rhino_app / "Contents/Resources/bin/rhinocode" if rhino_app is not None else Path()
    direct, roll = (
        run((str(rhino_code), "list", "--json"), env=overlay, check=False).map(lambda done: done.returncode).default_with(lambda _: -1)
        if rhino_code.is_file()
        else -1
        for overlay in (apphost_env, dotnet_apphost_env({**base_env, "DOTNET_ROLL_FORWARD": "Major"}))
    )
    sources = _inventory_sources(settings)
    host_sources = tuple(source for source in sources if source.kind == "host")
    nuget_sources = tuple(source for source in sources if source.kind == "package")
    stdout = ilspy_done.stdout.decode(errors="replace")
    stderr = ilspy_done.stderr.decode(errors="replace")
    artifacts = (_artifact(root, "raw.stdout.txt", stdout), _artifact(root, "raw.stderr.txt", stderr))
    status: ApiStatus = "failed" if rhino_app is None or ilspy_done.returncode != 0 else "ok"
    report = ApiDoctorReport(
        query={"op": "doctor"},
        status=status,
        rhino={"app": str(rhino_app) if rhino_app is not None else "", "version": version, "status": "ok" if rhino_app is not None else "missing"},
        ilspy={
            "status": "ok" if ilspy_done.returncode == 0 else "failed",
            "version": stdout.strip() or stderr.strip() or "unavailable",
            "dotnet_root": apphost_env.get("DOTNET_ROOT", "muxer-resolved"),
            "invocation": "dotnet tool run ilspycmd",
        },
        rhinocode={"status": "ok" if rhino_code.is_file() else "missing", "path": str(rhino_code), "direct": direct, "roll_forward": roll},
        counts={"host_sources": len(host_sources), "nuget_sources": len(nuget_sources), "sources": len(sources)},
        artifact_paths={artifact.kind: artifact.path for artifact in artifacts},
        sources=tuple(
            {**_source_payload(source), "assembly_status": _present(source.assembly), "xml_status": _present(source.xml)} for source in sources
        ),
        artifacts=artifacts,
        notes=(
            "Package sources resolve lazily from Directory.Packages.props on first query.",
            "Missing optional XML sidecars are inventory notes, not doctor failures.",
            "ilspycmd resolves via `dotnet tool run` against .config/dotnet-tools.json; the query surface engine is XML-free.",
        ),
    )
    return Ok(_report_artifacts(root, report))


def _show_direct(api_artifacts: Path, token: str) -> Path | None:
    direct = Path(token).expanduser()
    candidates = (
        *((direct,) if direct.is_file() else ()),
        *(sorted(api_artifacts.glob(f"**/{token}"), key=lambda path: path.stat().st_mtime, reverse=True) if api_artifacts.is_dir() else ()),
    )
    return next((path for path in candidates if path.is_file()), None)


def _show_artifact_path(report: ApiStoredReport, kind: str) -> str:
    return report.artifact_paths.get(kind, "")


def _show_field_match(field: dict[str, str] | ApiMatch, needle: str) -> bool:
    match field:
        case dict() as values:
            return any(value.casefold() == needle for value in values.values())
        case ApiMatch() as result:
            return needle in {result.id.casefold(), result.kind.casefold(), result.text.casefold()}


def _show_report_match(report: ApiStoredReport, token: str) -> str:
    needle = token.casefold()
    exact_artifact = next(
        (
            artifact.path
            for artifact in report.artifacts
            if needle in {artifact.id.casefold(), artifact.kind.casefold(), Path(artifact.kind).stem.casefold(), artifact.path.casefold()}
        ),
        "",
    )
    match exact_artifact:
        case str(path) if path:
            return path
        case _:
            pass
    fields: tuple[dict[str, str] | ApiMatch, ...] = (
        (report.query, report.source, *report.results) if isinstance(report, ApiQueryReport) else (report.query,)
    )
    return next(
        (
            _show_artifact_path(report, kind)
            for field in fields
            if _show_field_match(field, needle)
            for kind in ("surface.txt", "decompile.cs", "source.preview.cs", "report")
            if _show_artifact_path(report, kind)
        ),
        "",
    )


def _slice_text(text: str, *, lines: str, grep: str, max_lines: int) -> tuple[str, int, bool]:
    selected = tuple(line for line in text.splitlines() if not grep or grep.casefold() in line.casefold())
    match lines.split(":", maxsplit=1):
        case [start, end] if start.isdigit() and end.isdigit():
            window = selected[int(start) - 1 : int(end)]
        case _:
            window = selected[:max_lines]
    return ("\n".join(window), len(selected), len(selected) > len(window))


def _show_report(path: Path) -> ApiStoredReport:
    # ApiStoredReport is a msgspec tagged union ("type" discriminant): one decode dispatches to the stored variant.
    report: ApiStoredReport = msgspec.json.decode(path.read_bytes(), type=ApiStoredReport)
    return report


def _show(
    settings: QualitySettings,
    token: str,
    *,
    policy: ApiShowPolicy = "current",
    full: bool = False,
    lines: str = "",
    grep: str = "",
    max_lines: int = _SHOW_LINES,
) -> Result[bytes, ProcessFault]:
    api_artifacts = settings.root / _API_ARTIFACT_ROOT
    search_root = api_artifacts if policy == "latest" else api_artifacts / settings.run_id
    direct = _show_direct(search_root, token)
    artifact_path: Path | None
    match direct:
        case Path() as path:
            artifact_path = path
        case None:
            reports = sorted(search_root.glob("**/report.json"), key=lambda path: path.stat().st_mtime, reverse=True) if search_root.is_dir() else ()
            artifact = next(
                (path for report_path in reports for report in (_show_report(report_path),) for path in (_show_report_match(report, token),) if path),
                "",
            )
            artifact_path = Path(artifact) if artifact else None
    match artifact_path:
        case Path() as path if path.is_file():
            text = path.read_text(encoding="utf-8", errors="replace")
            preview, total, truncated = _slice_text(text, lines=lines, grep=grep, max_lines=max_lines)
            selected = text if full else preview
            payload = ApiShowReport(
                query={"op": "show", "token": token, "lines": lines, "grep": grep, "full": str(full).lower()},
                status="ok",
                artifact=ApiArtifact(
                    id=_artifact_id(str(path), path.name), kind=path.name, path=str(path), bytes=path.stat().st_size, lines=text.count("\n")
                ),
                preview=selected,
                counts={"selected_lines": total, "preview_lines": selected.count("\n") + int(bool(selected))},
                content=text if full else "",
                truncated=False if full else truncated,
            )
            return Ok(msgspec.json.encode(payload))
        case _:
            return Error(ProcessFault.fail("api", "show", token, detail=f"Artifact not found: {token}"))


@beartype
def api(
    settings: QualitySettings,
    scope: ArtifactScope,
    op: ApiOp,
    key: str = "rhino-common",
    *,
    symbol: str = "",
    kind: ApiPathKind = "all",
    max_lines: int = _SHOW_LINES,
    full: bool = False,
    lines: str = "",
    grep: str = "",
    show_policy: ApiShowPolicy = "current",
    restore: ApiRestoreMode = "missing",
    env: dict[str, str] | None = None,
) -> Result[bytes, ProcessFault]:
    handlers: dict[ApiOp, Callable[[], Result[bytes, ProcessFault]]] = {
        "doctor": lambda: _doctor(settings, scope, _artifact_root(scope, op, "all"), env),
        "resolve": lambda: _source(settings, scope, key, restore=restore).map(
            lambda source: _resolve_report(_artifact_root(scope, op, source.key, kind), source, kind)
        ),
        "query": lambda: _query(settings, scope, key, symbol, max_lines=max_lines, full=full, grep=grep, restore=restore),
        "show": lambda: _show(settings, key, policy=show_policy, full=full, lines=lines, grep=grep, max_lines=max_lines),
    }
    return handlers[op]()
