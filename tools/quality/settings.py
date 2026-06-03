"""Frozen gate settings and per-agent MSBuild artifact scope for the quality operator."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Iterator
from contextlib import contextmanager
from dataclasses import dataclass
from datetime import datetime, UTC
import os
from pathlib import Path
import plistlib
from typing import Annotated, Final, Literal

from pydantic import Field, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


# --- [TYPES] ---------------------------------------------------------------------------

type Configuration = Literal["Debug", "Release"]


# --- [CONSTANTS] -----------------------------------------------------------------------

_QUALITY: Final[str] = "quality"
_ARTIFACTS: Final[str] = ".artifacts"
_DEFAULT_RASM_TESTS: Final[Path] = Path("tests/csharp/libs/Rasm/Rasm.Tests.csproj")
_DOTNET_CLI: Final[str] = "dotnet-cli"
_MARKER: Final[str] = "Workspace.slnx"
CS_SUFFIXES: Final[frozenset[str]] = frozenset((".cs", ".csproj", ".props", ".targets", ".json", ".resx", ".ico", ".ghicon", ".yml", ".yaml"))
FULL_CONFIGURATIONS: Final[tuple[Configuration, Configuration]] = ("Debug", "Release")
FULL_TRIGGER_FILES: Final[frozenset[str]] = frozenset((
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    _MARKER,
    ".editorconfig",
    "global.json",
))
IGNORE_FIXTURE_PREFIXES: Final[tuple[str, ...]] = ("tests/tools/ast-grep/", "tests/tools/py_analyzer/")
MUTATION_THRESHOLDS: Final[tuple[int, int, int]] = (95, 90, 85)
PROJECT_EXCLUDE_DIRS: Final[tuple[str, ...]] = (
    ".archive",
    _ARTIFACTS,
    ".cache",
    ".git",
    ".nx",
    "bin",
    "coverage",
    "node_modules",
    "obj",
    "test-results",
    "tmp",
)
RASM_BRIDGE_SLUG: Final[str] = "rasm-bridge"
STATIC_FULL_TRIGGER_PREFIXES: Final[tuple[str, ...]] = ("tools/cs-analyzer/",)
YAK_PLATFORM: Final[str] = "mac"
YAK_DISTRIBUTION_GLOB: Final[str] = "*-rh9_*-mac.yak"


# --- [MODELS] ----------------------------------------------------------------------------


class QualitySettings(BaseSettings):
    """Gate configuration loaded once at startup via ``QUALITY_<FIELD>`` env vars."""

    model_config = SettingsConfigDict(env_prefix="QUALITY_", env_nested_delimiter="__", frozen=True, extra="forbid")

    root: Path = Field(default_factory=Path.cwd)
    configuration: Configuration = "Release"
    static_configuration: Configuration = "Debug"
    dotnet_max_cpu: Annotated[int, Field(ge=1, le=64)] = 4
    mutation_max_cpu: Annotated[int, Field(ge=1, le=64)] = 2
    test_target: Path = Field(default=_DEFAULT_RASM_TESTS)
    mutation_project: Path = Field(default=Path("libs/csharp/Rasm/Rasm.csproj"))
    mutation_test_project: Path = Field(default=_DEFAULT_RASM_TESTS)
    mutation_target_framework: str = "net10.0"
    rhino_app: Path = Field(default_factory=lambda: QualitySettings._resolve_rhino_app())
    bridge_endpoint: Path = Field(default=Path.home() / ".rasm" / "rhino-bridge.json")
    test_timeout_s: Annotated[float, Field(gt=0.0)] = 300.0
    mutation_timeout_s: Annotated[float, Field(gt=0.0)] = 1200.0
    scenario_timeout_s: Annotated[float, Field(gt=0.0)] = 180.0
    verify_retention_seconds: Annotated[float, Field(gt=0.0)] = 300.0
    configurations: str | None = None
    run_id: str = Field(default_factory=lambda: f"{datetime.now(tz=UTC).strftime('%Y-%m-%dT%H-%M-%S.%f')}-{os.getpid()}")
    coverage_threshold: float | None = None
    coverage_threshold_type: str | None = None
    coverage_threshold_stat: str | None = None

    @field_validator("test_target", "mutation_project", "mutation_test_project", mode="before")
    @classmethod
    def _expand(cls, value: str | Path) -> Path:
        return Path(value).expanduser()

    @field_validator("root", mode="before")
    @classmethod
    def _anchor(cls, value: str | Path) -> Path:
        return cls.anchor(Path(value).expanduser())

    @field_validator("rhino_app", mode="before")
    @classmethod
    def _require_rhino_app(cls, value: str | Path | None) -> Path:
        return cls._coerce_rhino_bundle(Path(value).expanduser() if value else None, label=value)

    @classmethod
    def anchor(cls, start: Path) -> Path:
        cursor = start.expanduser().resolve()
        return next((parent for parent in (cursor, *cursor.parents) if (parent / _MARKER).is_file()), cursor)

    @staticmethod
    def _rhino_listing() -> str:
        candidates = sorted(app.name for app in Path("/Applications").glob("Rhino*.app") if app.is_dir())
        return ", ".join(candidates) if candidates else "none found in /Applications"

    @staticmethod
    def _coerce_rhino_bundle(path: Path | None, *, label: object) -> Path:
        match path:
            case Path() as bundle if bundle.is_dir():
                return bundle
            case _:
                raise ValueError(
                    f"Rhino app bundle not found or not a directory: {label!r}. "
                    f"Set RHINO_WIP_APP_PATH (or QUALITY_RHINO_APP) to a Rhino*.app bundle. "
                    f"Candidates: {QualitySettings._rhino_listing()}"
                )

    @staticmethod
    def _newest_rhino_app() -> Path | None:
        def bundle_version(app: Path) -> tuple[int, ...]:
            # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=plist-read ticket=QUALITY-R8 expires=2026-12-31 rationale=fs-ingress
            try:
                plist = plistlib.loads((app / "Contents" / "Info.plist").read_bytes())
            except OSError, plistlib.InvalidFileException:
                return ()
            return tuple(int(part) for part in str(plist.get("CFBundleVersion", "")).split(".") if part.isdigit())

        return max(
            ((bundle_version(app), "WIP" in app.name, app) for app in Path("/Applications").glob("Rhino*.app") if app.is_dir()),
            default=((), False, None),
        )[2]

    @staticmethod
    def _resolve_rhino_app() -> Path:
        override = os.environ.get("RHINO_WIP_APP_PATH", "")  # noqa: TID251
        candidate = Path(override).expanduser() if override else QualitySettings._newest_rhino_app()
        return QualitySettings._coerce_rhino_bundle(candidate, label=override or "auto-discover")

    @property
    def solution(self) -> Path:
        return self.root / _MARKER

    @property
    def mutation_eligible(self) -> bool:
        return (self.root / self.test_target).resolve() == (self.root / self.mutation_test_project).resolve()

    @property
    def test_slice(self) -> str:
        return self.test_target.stem

    @property
    def artifact_root(self) -> Path:
        return self.root / _ARTIFACTS

    @property
    def test_results_dir(self) -> Path:
        return self.test_results(all_targets=False)

    def test_results(self, *, all_targets: bool) -> Path:
        return self._artifact_dir(kind="test", slice_name="all" if all_targets else self.test_slice)

    @property
    def mutation_output_dir(self) -> Path:
        return self._artifact_dir("mutation")

    @property
    def mutation_lock(self) -> Path:
        return self.artifact_root / "locks" / "mutation.lock"

    @property
    def bridge_lock(self) -> Path:
        return self.artifact_root / "locks" / "bridge.lock"

    @property
    def bridge_client(self) -> Path:
        return self.root / "tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"

    @property
    def bridge_client_ready(self) -> bool:
        bin_dir = self.bridge_client.parent / "bin" / self.configuration
        return any(bin_dir.glob("*/Rasm.RhinoBridge.Client.dll"))

    @property
    def scenario_kit_project(self) -> Path:
        return self.root / "tests/csharp/_testkit/Rasm.TestKit.csproj"

    @property
    def bridge_projects(self) -> tuple[Path, ...]:
        base = self.root / "tools/rhino-bridge"
        return (base / "protocol/Rasm.RhinoBridge.Protocol.csproj", base / "plugin/Rasm.RhinoBridge.Plugin.csproj", self.bridge_client)

    @property
    def bridge_verify_root(self) -> Path:
        return self.artifact_root / "rhino" / "verify"

    @property
    def bridge_verify_dir(self) -> Path:
        return self.bridge_verify_root / self.run_id

    def static_configurations(self, scope: Literal["changed", "full"]) -> tuple[Configuration, ...]:
        raw = self.configurations
        match (raw, scope):
            case (str(text), _) if text.strip():
                selected = frozenset(text.split())
                return tuple(configuration for configuration in FULL_CONFIGURATIONS if configuration in selected)
            case (_, "full"):
                return FULL_CONFIGURATIONS
            case _:
                return (self.static_configuration,)

    @property
    def dotnet_tools_manifest(self) -> Path:
        return self.root / ".config/dotnet-tools.json"

    def csharp_lib_project(self, project: str) -> Path:
        return self.root / "libs/csharp" / project / f"{project}.csproj"

    @property
    def mutation_mutate_globs(self) -> tuple[str, ...]:
        return ("**/*.cs", "!**/bin/**/*.cs", "!**/obj/**/*.cs")

    @staticmethod
    def version_props(version: str = "") -> tuple[str, ...]:
        return (f"/p:Version={version}", f"/p:InformationalVersion={version}") if version else ()

    def dotnet_env(self, scope_path: Path, *, rail: str = "") -> dict[str, str]:
        overlay = {"DOTNET_CLI_HOME": str(scope_path / _DOTNET_CLI)} | ({} if rail == "static" else {"MSBUILDDISABLENODEREUSE": "1"})
        return {**os.environ, **overlay, "RHINO_WIP_APP_PATH": str(self.rhino_app)}  # noqa: TID251

    def _artifact_dir(self, kind: Literal["test", "mutation"], slice_name: str | None = None) -> Path:
        return self.artifact_root / kind / (slice_name or self.test_slice) / self.run_id


@dataclass(frozen=True, slots=True)
class ArtifactScope:
    root: Path
    rail: str
    scope_path: Path
    dotnet_env: dict[str, str]

    @property
    def path(self) -> Path:
        return self.scope_path

    @property
    def dotnet_flags(self) -> tuple[str, ...]:
        base = ("--artifacts-path", str(self.path))
        return base if self.rail == "static" else (*base, "--disable-build-servers")

    @property
    def artifacts_property(self) -> tuple[str, ...]:
        return ("-p:ArtifactsPath=" + str(self.path),)

    @classmethod
    @contextmanager
    def open(cls, settings: QualitySettings, rail: str) -> Iterator[ArtifactScope]:
        scope_path = settings.artifact_root / _QUALITY / rail / settings.run_id
        (scope_path / _DOTNET_CLI).mkdir(parents=True, exist_ok=True)
        yield ArtifactScope(root=settings.root, rail=rail, scope_path=scope_path, dotnet_env=settings.dotnet_env(scope_path, rail=rail))
