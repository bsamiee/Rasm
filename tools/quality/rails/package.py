"""Yak package staging, install, deploy, and publish rail for Rasm app and tool projects."""

# --- [IMPORTS] ------------------------------------------------------------------------

from __future__ import annotations

from collections.abc import Callable
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

from tools.quality.process import decode_json, dotnet_args, fd_args, fold, ProcessFault, run, run_fold, Workspace
from tools.quality.rails.bridge import build_client, client_run, try_decode_bridge
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
_CLIENT_STEPS: Final[frozenset[PackageStepKind]] = frozenset({"quit", "refresh"})
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
_PACKAGE_STEPS: Final[dict[tuple[PackageMode, bool], tuple[PackageStepKind, ...]]] = {
    ("deploy", False): ("install",),
    ("deploy", True): ("quit", "install", "refresh"),
    ("publish", False): ("install", "push"),
    ("publish", True): ("quit", "install", "refresh", "push"),
}


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
    def from_props(
        cls, project: Path, props: dict[str, str], settings: QualitySettings, slug: str
    ) -> Result[PackageMeta, ProcessFault]:
        missing = tuple(name for name in _META_PROPS if name != "YakPushSource" and not props.get(name))
        match missing:
            case ():
                meta = cls(
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
                )
                return meta.validate(settings, slug, props["YakPackageSlug"])
            case names:
                return Error(ProcessFault.fail("package", detail=f"Missing MSBuild properties: {', '.join(names)}"))

    def validate(self, settings: QualitySettings, slug: str, evaluated_slug: str) -> Result[PackageMeta, ProcessFault]:
        root = settings.root.resolve()
        expected_target = (self.project_dir / "bin" / settings.configuration / self.target_framework).resolve()
        checks = (
            (
                evaluated_slug == slug,
                f"Package slug mismatch for {self.project}: expected {slug}, evaluated {evaluated_slug}",
            ),
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
        return next(
            (Error(ProcessFault.fail("package", slug, detail=detail)) for ok, detail in checks if not ok), Ok(self)
        )


class PackageArtifact(msgspec.Struct, frozen=True):
    stage: Path
    meta: PackageMeta


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

            def yak(op: YakOp) -> Result[None, ProcessFault]:
                return run(
                    _yak_argv(artifact.meta, op, package_file=package_file), cwd=artifact.meta.package_dir, check=True
                ).map(lambda _: None)

            step: dict[PackageStepKind, Callable[[], Result[None, ProcessFault]]] = {
                "install": lambda: yak("install"),
                "push": lambda: yak("push"),
                "quit": lambda: client_run(settings, scope, "quit", check=False).bind(
                    lambda run: (
                        try_decode_bridge(run.stdout)
                        .filter_with(
                            lambda decoded: decoded.status == "ok",
                            lambda decoded: ProcessFault.fail(
                                "quit",
                                returncode=1,
                                detail=run.stderr
                                or (decoded.fault.message.encode() if decoded.fault else b"quit refused"),
                            ),
                        )
                        .map(lambda _: None)
                    )
                ),
                "refresh": lambda: client_run(settings, scope, "launch").bind(
                    lambda _: client_run(settings, scope, "doctor").map(lambda _: None)
                ),
            }

            steps = _PACKAGE_STEPS.get((mode, slug == RASM_BRIDGE_SLUG), ())
            prelude = build_client(settings, scope) if _CLIENT_STEPS.intersection(steps) else Ok(None)
            return prelude.bind(lambda _: fold(steps, None, lambda _, kind: step[kind]()))
        case unreachable:
            assert_never(unreachable)


def _stage(
    settings: QualitySettings, scope: ArtifactScope, project: Path, slug: str, version: str
) -> Result[PackageArtifact, ProcessFault]:
    def build(meta: PackageMeta) -> Result[PackageArtifact, ProcessFault]:
        shutil.rmtree(meta.target_dir, ignore_errors=True)
        meta.package_dir.parent.mkdir(parents=True, exist_ok=True)
        stage = Path(tempfile.mkdtemp(prefix=f"{meta.package_dir.name}.", dir=meta.package_dir.parent))
        previous = meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}")
        primary = meta.target_dir / f"{meta.assembly_name}{meta.target_ext}"
        manifest = meta.manifest_dir / "manifest.yml"

        def commit() -> Result[PackageArtifact, ProcessFault]:
            # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=atomic-package-stage ticket=QUALITY-R7
            # expires=2026-12-31 rationale=file-system-transaction-boundary
            try:
                shutil.rmtree(previous, ignore_errors=True)
                match meta.package_dir.exists():
                    case True:
                        shutil.move(str(meta.package_dir), str(previous))
                    case False:
                        pass
                shutil.move(str(stage), str(meta.package_dir))
                shutil.rmtree(previous, ignore_errors=True)
                return Ok(PackageArtifact(meta.package_dir, meta))
            except OSError as exc:
                match previous.exists() and not meta.package_dir.exists():
                    case True:
                        shutil.move(str(previous), str(meta.package_dir))
                    case False:
                        pass
                shutil.rmtree(stage, ignore_errors=True)
                return Error(ProcessFault.fail("package", "stage", detail=str(exc)))

        return run_fold(
            scope,
            (
                ("dotnet", *dotnet_args("restore", meta.project, disable_parallel=True)),
                (
                    "dotnet",
                    *dotnet_args(
                        "build",
                        meta.project,
                        configuration=settings.configuration,
                        version=settings.version_props(version),
                        serial=True,
                    ),
                ),
            ),
        ).bind(
            lambda _: (
                Ok(primary)
                .filter_with(
                    lambda _: manifest.is_file(),
                    lambda _: ProcessFault.fail("package", detail=f"Missing Yak manifest: {manifest}"),
                )
                .filter_with(
                    lambda path: path.is_file(),
                    lambda _: ProcessFault.fail("package", detail=b"Missing primary artifact"),
                )
                .bind(
                    lambda _: (
                        tuple(
                            shutil.copy2(path, stage / path.name)
                            for path in chain(
                                (
                                    meta.manifest_dir / name
                                    for name in _MANIFEST_FILES
                                    if (meta.manifest_dir / name).is_file()
                                ),
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
        .bind(build)
    )


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
    root = settings.root
    workspace = Workspace(root)
    roots = tuple(str(root / name) for name in _PACKAGE_ROOTS if (root / name).is_dir())
    found = () if not roots else workspace.paths(fd_args("csproj", ".", *roots, exclude=False), suffix=".csproj")

    def resolve(found: Path | None, project: Path) -> Result[Path | None, ProcessFault]:
        match (workspace.csproj(project, "YakPackageSlug") == slug, found):
            case (True, None):
                return Ok(project)
            case (True, _):
                return Error(
                    ProcessFault.fail(
                        "package", slug, detail=f"Expected one package project for {slug}, found duplicate"
                    )
                )
            case _:
                return Ok(found)

    return (
        fold(found, None, resolve)
        .bind(
            lambda selected: (
                Error(ProcessFault.fail("package", slug, detail=f"Expected one package project for {slug}, found 0"))
                if selected is None
                else Ok(selected)
            )
        )
        .bind(
            lambda project: _stage(settings, scope, project, slug, version).bind(
                lambda artifact: _finish(settings, scope, mode, slug, artifact)
            )
        )
    )
