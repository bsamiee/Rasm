"""Yak package staging, install, deploy, and publish rail for Rasm app and tool projects."""

# --- [IMPORTS] ------------------------------------------------------------------------

import fnmatch
from itertools import chain
import os
from pathlib import Path
import shutil
import tempfile
from typing import assert_never, Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import decode_json, dotnet_build, fd_args, fold, leased, ProcessFault, run, Workspace
from tools.quality.rails.bridge import build_client, client_quit, client_refresh, with_bridge_lease
from tools.quality.settings import ArtifactScope, QualitySettings, RASM_BRIDGE_SLUG, YAK_DISTRIBUTION_GLOB, YAK_PLATFORM


# --- [TYPES] ---------------------------------------------------------------------------

type PackageMode = Literal["package", "deploy", "publish"]
type PackageStepKind = Literal["quit", "install", "refresh", "push"]
type YakOp = Literal["build", "install", "push"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_ARTIFACT_SUFFIXES: Final[frozenset[str]] = frozenset({".dll", ".json", ".rhp"})
_HOST_EXCLUDES: Final[tuple[str, ...]] = (
    "Eto.*",
    "Eto.macOS.*",
    "Grasshopper.*",
    "Grasshopper2.*",
    "GrasshopperIO.*",
    "Microsoft.macOS.*",
    "Rhino.Runtime.Code.*",
    "Rhino.UI.*",
    "RhinoCodePlatform.Rhino3D.*",
    "RhinoCommon.*",
    "System.Drawing.Common.*",
)
_MANIFEST_FILES: Final[tuple[str, ...]] = ("icon.png", "manifest.yml")
_META_PROPS: Final[tuple[str, ...]] = (
    "AssemblyName",
    "MSBuildProjectDirectory",
    "TargetDir",
    "TargetExt",
    "TargetFramework",
    "YakManifestDirectory",
    "YakPackageDirectory",
    "YakPackagePattern",
    "YakPackageSlug",
    "YakPath",
    "YakPlatform",
    "YakPushSource",
)
_PACKAGE_ROOTS: Final[tuple[str, ...]] = ("apps", "tools")

# --- [MODELS] ----------------------------------------------------------------------------


class PackageMeta(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    project: Path
    manifest_dir: Path
    target_dir: Path
    assembly_name: str
    target_ext: str
    yak_path: Path
    yak_platform: str
    yak_push_source: str
    package_dir: Path
    package_pattern: str
    target_framework: str = ""
    project_dir: Path = Path()

    @classmethod
    def from_props(cls, project: Path, props: dict[str, str], settings: QualitySettings, slug: str) -> Result[PackageMeta, ProcessFault]:
        missing = tuple(name for name in _META_PROPS if name != "YakPushSource" and not props.get(name))
        match missing:
            case ():
                return cls(
                    project=project,
                    manifest_dir=Path(props["YakManifestDirectory"]),
                    target_dir=Path(props["TargetDir"]),
                    assembly_name=props["AssemblyName"],
                    target_ext=props["TargetExt"],
                    yak_path=Path(props["YakPath"]),
                    yak_platform=props["YakPlatform"],
                    yak_push_source=props.get("YakPushSource", ""),
                    package_dir=Path(props["YakPackageDirectory"]),
                    package_pattern=props["YakPackagePattern"],
                    target_framework=props["TargetFramework"],
                    project_dir=Path(props["MSBuildProjectDirectory"]),
                ).validate(settings, slug, props["YakPackageSlug"])
            case names:
                return Error(ProcessFault.fail("package", detail=f"Missing MSBuild properties: {', '.join(names)}"))

    def validate(self, settings: QualitySettings, slug: str, evaluated_slug: str) -> Result[PackageMeta, ProcessFault]:
        root = settings.root.resolve()
        expected_target = (self.project_dir / "bin" / settings.configuration / self.target_framework).resolve()
        checks = (
            (evaluated_slug == slug, f"Package slug mismatch for {self.project}: expected {slug}, evaluated {evaluated_slug}"),
            (self.target_ext == ".rhp", f"Package project must emit .rhp for {slug}: {self.project}"),
            (
                self.target_dir.resolve().is_relative_to(root) and self.target_dir.resolve() == expected_target,
                f"Refusing to clean unexpected output directory: {self.target_dir}",
            ),
            (
                self.yak_platform == YAK_PLATFORM and fnmatch.fnmatch(self.package_pattern, YAK_DISTRIBUTION_GLOB),
                f"Package distribution must match {YAK_DISTRIBUTION_GLOB} for {slug}: {self.package_pattern}",
            ),
            (self.yak_path.is_file() and os.access(self.yak_path, os.X_OK), f"Yak not executable at {self.yak_path}"),
        )
        return next((Error(ProcessFault.fail("package", slug, detail=detail)) for ok, detail in checks if not ok), Ok(self))


class PackageArtifact(msgspec.Struct, frozen=True):
    stage: Path
    meta: PackageMeta


class PackagePolicy(msgspec.Struct, frozen=True, gc=False):
    steps: tuple[PackageStepKind, ...] = ()

    @property
    def needs_bridge(self) -> bool:
        return any(step in {"quit", "refresh"} for step in self.steps)


_PACKAGE_POLICY: Final[dict[tuple[PackageMode, bool], PackagePolicy]] = {
    ("deploy", False): PackagePolicy(steps=("install",)),
    ("deploy", True): PackagePolicy(steps=("quit", "install", "refresh")),
    ("publish", False): PackagePolicy(steps=("install", "push")),
    ("publish", True): PackagePolicy(steps=("quit", "install", "refresh", "push")),
}


class PackageRunReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    query: dict[str, str]
    status: str
    stage: str = ""
    project: str = ""
    package_pattern: str = ""
    artifact_paths: dict[str, str] = msgspec.field(default_factory=dict)


class PackageProject(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    slug: str
    project: str


class PackagePlanReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    query: dict[str, str]
    status: str
    project: str
    meta: dict[str, str]
    artifact_paths: dict[str, str]


class PackageListReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    query: dict[str, str]
    status: str
    count: int
    packages: tuple[PackageProject, ...]
    artifact_paths: dict[str, str]


class _MsbuildProps(msgspec.Struct, frozen=True):
    Properties: dict[str, str]


# --- [OPERATIONS] ------------------------------------------------------------------------


def _finish(
    settings: QualitySettings, scope: ArtifactScope, mode: PackageMode, slug: str, artifact: PackageArtifact
) -> Result[PackageArtifact | None, ProcessFault]:
    match mode:
        case "package":
            return Ok(artifact)
        case "deploy" | "publish":
            match sorted(artifact.stage.glob(artifact.meta.package_pattern)):
                case [package_file]:
                    pass
                case matches:
                    detail = f"Expected one package for pattern {artifact.meta.package_pattern}, found {len(matches)}"
                    return Error(ProcessFault.fail("package", artifact.meta.package_pattern, detail=detail))

            policy = _PACKAGE_POLICY.get((mode, slug == RASM_BRIDGE_SLUG), PackagePolicy())

            def run_step(kind: PackageStepKind) -> Result[None, ProcessFault]:
                match kind:
                    case "install":
                        return run(_yak_argv(artifact.meta, "install", package_file=package_file), cwd=artifact.meta.package_dir, check=True).map(
                            lambda _: None
                        )
                    case "push":
                        return run(_yak_argv(artifact.meta, "push", package_file=package_file), cwd=artifact.meta.package_dir, check=True).map(
                            lambda _: None
                        )
                    case "quit":
                        return client_quit(settings, scope)
                    case "refresh":
                        return client_refresh(settings, scope)
                    case unreachable:
                        assert_never(unreachable)

            def run_steps() -> Result[None, ProcessFault]:
                prelude = build_client(settings, scope) if policy.needs_bridge else Ok(None)
                return prelude.bind(lambda _: fold(policy.steps, None, lambda _, kind: run_step(kind)))

            return with_bridge_lease(settings, run_steps) if policy.needs_bridge else run_steps()
        case unreachable:
            assert_never(unreachable)


def _stage(settings: QualitySettings, scope: ArtifactScope, project: Path, slug: str, version: str) -> Result[PackageArtifact, ProcessFault]:
    def build(meta: PackageMeta) -> Result[PackageArtifact, ProcessFault]:
        lock = meta.package_dir.with_name(f".{meta.package_dir.name}.lock")
        return leased(settings, lock, "package-stage", lambda: stage_locked(meta), project=slug)

    def stage_locked(meta: PackageMeta) -> Result[PackageArtifact, ProcessFault]:
        shutil.rmtree(meta.target_dir, ignore_errors=True)
        meta.package_dir.parent.mkdir(parents=True, exist_ok=True)
        stage, previous, primary, manifest = (
            Path(tempfile.mkdtemp(prefix=f"{meta.package_dir.name}.", dir=meta.package_dir.parent)),
            meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}"),
            meta.target_dir / f"{meta.assembly_name}{meta.target_ext}",
            meta.manifest_dir / "manifest.yml",
        )

        def _restore_previous_on_failure() -> None:
            match previous.exists() and not meta.package_dir.exists():
                case True:
                    previous.replace(meta.package_dir)
                case False:
                    pass
            shutil.rmtree(stage, ignore_errors=True)

        def _promote_staged_dir() -> None:
            shutil.rmtree(previous, ignore_errors=True)
            match meta.package_dir.exists():
                case True:
                    meta.package_dir.replace(previous)
                case False:
                    pass
            stage.replace(meta.package_dir)
            shutil.rmtree(previous, ignore_errors=True)

        def commit() -> Result[PackageArtifact, ProcessFault]:
            # Atomic commit: rotate package_dir -> previous before replacing with the staged dir, restoring previous on OSError.
            try:
                _promote_staged_dir()
                return Ok(PackageArtifact(meta.package_dir, meta))
            except OSError as exc:
                _restore_previous_on_failure()
                return Error(ProcessFault.fail("package", "stage", detail=str(exc)))

        return dotnet_build(
            settings, scope, restore=meta.project, targets=(meta.project,), version=version, disable_parallel=True, serial=True, scoped=False
        ).bind(
            lambda _: (
                Ok(primary)
                .filter_with(lambda _: manifest.is_file(), lambda _: ProcessFault.fail("package", detail=f"Missing Yak manifest: {manifest}"))
                .filter_with(lambda path: path.is_file(), lambda _: ProcessFault.fail("package", detail=b"Missing primary artifact"))
                .bind(
                    lambda _: (
                        tuple(
                            shutil.copy2(path, stage / path.name)
                            for path in chain(
                                (meta.manifest_dir / name for name in _MANIFEST_FILES if (meta.manifest_dir / name).is_file()),
                                (
                                    path
                                    for path in meta.target_dir.iterdir()
                                    if path.suffix in _ARTIFACT_SUFFIXES
                                    and not any(fnmatch.fnmatch(path.name, pattern) for pattern in _HOST_EXCLUDES)
                                ),
                            )
                        ),
                        run(_yak_argv(meta, "build", version=version), cwd=stage, check=True).bind(lambda _: commit()),
                    )[1]
                )
            )
        )

    return _evaluate_meta(settings, scope, project, slug, version).bind(build)


def _package_projects(settings: QualitySettings) -> Result[tuple[Path, ...], ProcessFault]:
    root = settings.root
    workspace = Workspace(root)
    roots = tuple(str(root / name) for name in _PACKAGE_ROOTS if (root / name).is_dir())
    return Ok(()) if not roots else workspace.paths(fd_args("csproj", ".", *roots, exclude=False), suffix=".csproj")


def _resolve_project(settings: QualitySettings, slug: str) -> Result[Path, ProcessFault]:
    workspace = Workspace(settings.root)

    def resolve(found: Path | None, project: Path) -> Result[Path | None, ProcessFault]:
        match (workspace.csproj(project, "YakPackageSlug") == slug, found):
            case (True, None):
                return Ok(project)
            case (True, _):
                return Error(ProcessFault.fail("package", slug, detail=f"Expected one package project for {slug}, found duplicate"))
            case _:
                return Ok(found)

    return (
        _package_projects(settings)
        .bind(lambda found: fold(found, None, resolve))
        .bind(
            lambda selected: (
                Error(ProcessFault.fail("package", slug, detail=f"Expected one package project for {slug}, found 0"))
                if selected is None
                else Ok(selected)
            )
        )
    )


def _evaluate_meta(settings: QualitySettings, scope: ArtifactScope, project: Path, slug: str, version: str) -> Result[PackageMeta, ProcessFault]:
    return (
        run(
            (
                "dotnet",
                "msbuild",
                str(project),
                f"-p:Configuration={settings.configuration}",
                *settings.version_props(version),
                *(f"-getProperty:{name}" for name in _META_PROPS),
                "-nologo",
            ),
            env=scope.dotnet_env,
            check=True,
        )
        .bind(lambda done: decode_json(done.stdout, _MsbuildProps).map(lambda data: data.Properties))
        .bind(lambda props: PackageMeta.from_props(project, props, settings, slug))
    )


def package_plan_payload(settings: QualitySettings, scope: ArtifactScope, slug: str, version: str) -> Result[bytes, ProcessFault]:
    def payload(meta: PackageMeta) -> bytes:
        return msgspec.json.encode(
            PackagePlanReport(
                query={"op": "package-plan", "slug": slug, "version": version},
                status="ok",
                project=str(meta.project),
                meta={
                    "manifest_dir": str(meta.manifest_dir),
                    "target_dir": str(meta.target_dir),
                    "package_dir": str(meta.package_dir),
                    "package_pattern": meta.package_pattern,
                    "target_framework": meta.target_framework,
                    "yak_path": str(meta.yak_path),
                    "yak_platform": meta.yak_platform,
                    "yak_push_source": meta.yak_push_source,
                },
                artifact_paths={"run": str(scope.path)},
            )
        )

    return _resolve_project(settings, slug).bind(lambda project: _evaluate_meta(settings, scope, project, slug, version).map(payload))


def package_list_payload(settings: QualitySettings, scope: ArtifactScope) -> Result[bytes, ProcessFault]:
    workspace = Workspace(settings.root)

    def payload(projects: tuple[Path, ...]) -> bytes:
        packages = tuple(
            PackageProject(slug=slug, project=str(project.relative_to(settings.root)))
            for project in projects
            for slug in (workspace.csproj(project, "YakPackageSlug"),)
            if isinstance(slug, str) and slug
        )
        return msgspec.json.encode(
            PackageListReport(
                query={"op": "package-list"}, status="ok", count=len(packages), packages=packages, artifact_paths={"run": str(scope.path)}
            )
        )

    return _package_projects(settings).map(payload)


def _yak_argv(meta: PackageMeta, op: YakOp, *, version: str = "", package_file: Path | None = None) -> tuple[str, ...]:
    template: dict[YakOp, tuple[str, ...]] = {
        "build": ("--platform", meta.yak_platform, "--version", version),
        "install": (str(package_file),),
        "push": (*(("--source", meta.yak_push_source) if meta.yak_push_source else ()), str(package_file)),
    }
    return (str(meta.yak_path), op, *template[op])


@beartype
def run_package_rail(
    settings: QualitySettings, scope: ArtifactScope, mode: PackageMode, slug: str, version: str
) -> Result[PackageArtifact | None, ProcessFault]:
    return _resolve_project(settings, slug).bind(
        lambda project: _stage(settings, scope, project, slug, version).bind(lambda artifact: _finish(settings, scope, mode, slug, artifact))
    )


def package_run_report(mode: PackageMode, slug: str, version: str, artifact: PackageArtifact | None) -> PackageRunReport:
    return PackageRunReport(
        query={"op": mode, "slug": slug, "version": version},
        status="ok",
        stage=str(artifact.stage) if artifact is not None else "",
        project=str(artifact.meta.project) if artifact is not None else "",
        package_pattern=artifact.meta.package_pattern if artifact is not None else "",
        artifact_paths={"stage": str(artifact.stage)} if artifact is not None else {},
    )
