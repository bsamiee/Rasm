"""Define settings, artifact storage, and dotnet artifact scopes."""

from dataclasses import dataclass
from datetime import datetime, UTC
from enum import StrEnum
import os
import sys
from typing import Annotated, Final, override, TYPE_CHECKING
from urllib.parse import urlsplit

import fsspec  # type: ignore[import-untyped]  # fsspec ships no py.typed marker
from pydantic import AliasChoices, BeforeValidator, computed_field, Field, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict
from upath import UPath  # pathlib.Path drop-in: file:// local stays identical, memory://+remote unlock


if TYPE_CHECKING:
    from fsspec.spec import AbstractFileSystem  # type: ignore[import-untyped]  # fsspec ships no py.typed marker
    from pydantic_settings import PydanticBaseSettingsSource

    from tools.assay.core.model import ArtifactKind, Claim


# --- [TYPES] ----------------------------------------------------------------------------


class Configuration(StrEnum):
    """MSBuild configuration value."""

    DEBUG = "Debug"
    RELEASE = "Release"


class LogFormat(StrEnum):
    """Structured-log renderer format."""

    HUMAN = "human"
    CI = "ci"


# --- [CONSTANTS] ------------------------------------------------------------------------

_MARKER: Final[str] = "Workspace.slnx"  # root anchor: the nearest ancestor holding it is the repo root
_ARTIFACTS: Final[str] = ".artifacts"
_ASSAY: Final[str] = "assay"
_BUILD: Final[str] = "build"  # SCOPE-namespace closure-tree segment; there is no BUILD ArtifactKind
_DISABLE_BUILD_SERVERS: Final[str] = "--disable-build-servers"
_ARTIFACTS_PATH_FLAG: Final[str] = "--artifacts-path"


def _anchor(value: str | UPath) -> UPath:
    # Climb to the nearest ancestor holding Workspace.slnx, normalizing once at the pydantic boundary so
    # every post-ingress reader sees an absolute repo-anchored UPath (a pathlib drop-in) never a raw string.
    cursor = UPath(value).expanduser().resolve()
    return next((p for p in (cursor, *cursor.parents) if (p / _MARKER).is_file()), cursor)


type AnchoredRoot = Annotated[UPath, BeforeValidator(_anchor)]
type ExpandedPath = Annotated[UPath, BeforeValidator(lambda v: UPath(v).expanduser())]


# --- [MODELS] ---------------------------------------------------------------------------


class AssaySettings(BaseSettings):
    """Validated assay runtime settings.

    Attributes:
        root: Repository root anchored to `Workspace.slnx` when present.
        configuration: Release-style configuration for runtime and package operations.
        static_configuration: Debug-style configuration for static compile operations.
        dotnet_max_cpu: Maximum dotnet build parallelism.
        mutation_max_cpu: Maximum mutation-test parallelism.
        max_checks: Maximum concurrent checks in fan-out.
        stream_tail_bytes: Retained byte tail for child process streams.
        stream_chunk_bytes: Byte chunk size for stream reads.
        lease_drift_tolerance: Process start-time tolerance for stale lease detection.
        artifact_retention: Number of persisted run-history entries to keep.
        scoped_verbs: Dotnet verbs that receive scoped artifact flags.
        trigger_files: Files that escalate C# routing to full scope.
        trigger_prefixes: Path prefixes that escalate C# routing to full scope.
        test_target: Default C# test project.
        mutation_project: Default C# mutation target project.
        log_format: Renderer format for structlog.
        run_id: Correlation identifier for the current invocation.
        agent_task_id: Optional fleet or agent task identifier.
        exec_target: Local or SSH execution target.
        exec_known_hosts: Optional AsyncSSH known-hosts path.

    """

    model_config = SettingsConfigDict(env_prefix="ASSAY_", case_sensitive=False, frozen=True, extra="forbid", populate_by_name=True)

    root: AnchoredRoot = Field(default_factory=UPath.cwd)
    configuration: Configuration = Configuration.RELEASE
    static_configuration: Configuration = Configuration.DEBUG
    dotnet_max_cpu: Annotated[int, Field(ge=1, le=64)] = 4
    mutation_max_cpu: Annotated[int, Field(ge=1, le=64)] = 2
    max_checks: Annotated[int, Field(ge=1, le=64)] = 8  # CapacityLimiter fan-out cap
    stream_tail_bytes: Annotated[int, Field(ge=512)] = 4096  # engine bounded-tail deque cap (stdout/stderr retention)
    stream_chunk_bytes: Annotated[int, Field(ge=4096)] = 65536  # engine ByteReceiveStream.receive max per read
    lease_drift_tolerance: Annotated[float, Field(gt=0)] = 1.0  # psutil create_time NTP-drift band (seconds)
    artifact_retention: Annotated[int, Field(ge=1, le=10000, validation_alias=AliasChoices("ASSAY_ARTIFACT_RETENTION"))] = 50  # newest-N run history
    scoped_verbs: frozenset[str] = frozenset(("build", "clean", "msbuild", "pack", "publish", "restore", "run", "test"))
    trigger_files: frozenset[str] = frozenset((
        ".config/dotnet-tools.json",
        "Directory.Build.props",
        "Directory.Build.targets",
        "Directory.Packages.props",
        "Workspace.slnx",
        ".editorconfig",
        "global.json",
    ))
    trigger_prefixes: tuple[str, ...] = ("tools/cs-analyzer/",)  # any descendant escalates the route to FULL
    test_target: ExpandedPath = UPath("tests/csharp/libs/Rasm/Rasm.Tests.csproj")
    mutation_project: ExpandedPath = UPath("libs/csharp/Rasm/Rasm.csproj")
    log_format: LogFormat = Field(default_factory=lambda: LogFormat.HUMAN if sys.stderr.isatty() else LogFormat.CI)
    run_id: str = Field(
        default_factory=lambda: f"{datetime.now(tz=UTC):%Y-%m-%dT%H-%M-%S.%f}-{os.getpid()}",
        validation_alias=AliasChoices("ASSAY_RUN_ID"),  # prefixed-only; populate_by_name keeps init-by-canonical
    )
    agent_task_id: str = Field(
        default="",
        validation_alias=AliasChoices("ASSAY_AGENT_TASK_ID"),  # prefixed-only; no bare-env leak
    )
    exec_target: str = Field(
        default="",
        validation_alias=AliasChoices("ASSAY_EXEC_TARGET"),  # prefixed-only; '' = local, ssh:// = remote
        description="execution target; '' = local, ssh://[user@]host[:port] = remote",
    )
    exec_known_hosts: str | None = Field(
        default=None,
        validation_alias=AliasChoices("ASSAY_EXEC_KNOWN_HOSTS"),  # prefixed-only; no bare-env host-key leak
        description="asyncssh known_hosts path for ssh:// exec_target; None disables the host-key check",
    )

    @field_validator("exec_target")
    @classmethod
    def _exec_target(cls, value: str) -> str:  # pydantic field_validator is cls-bound (classmethod)
        # Admit "" (local) or a well-formed ssh://[user@]host[:port] URI. Validate scheme+host+numeric port
        # HERE so engine._run_remote's urlsplit(...).port can never raise mid-spawn past the engine's catch.
        match value:
            case "":
                return value
            case _:
                parts = urlsplit(value)
                match (parts.scheme, parts.hostname):
                    case ("ssh", str() as host) if host:
                        try:
                            _ = parts.port  # numeric-port probe: a non-numeric authority raises ValueError HERE, at the boundary
                        except ValueError as exc:
                            raise ValueError(f"exec_target has a non-numeric ssh port: {value!r}") from exc
                        return value
                    case _:
                        raise ValueError(f"exec_target must be '' (local) or 'ssh://[user@]host[:port]' (remote): {value!r}")

    @override
    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: type[BaseSettings],  # pydantic override contract: signature fixed, only init/env returned
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,  # dotenv source dropped at the boundary
        file_secret_settings: PydanticBaseSettingsSource,  # secrets source dropped at the boundary
    ) -> tuple[PydanticBaseSettingsSource, ...]:
        """Use init values and `ASSAY_*` environment variables as the only settings sources.

        Args:
            settings_cls: Settings class supplied by pydantic-settings.
            init_settings: Values passed to the constructor.
            env_settings: Values loaded from the environment.
            dotenv_settings: Dotenv source, intentionally ignored.
            file_secret_settings: Secret-file source, intentionally ignored.

        Returns:
            Ordered settings sources.

        """
        return (init_settings, env_settings)

    @computed_field  # type: ignore[prop-decorator]  # pydantic computed_field over a property
    @property
    def solution(self) -> UPath:
        """Resolve the workspace solution path.

        Returns:
            The `Workspace.slnx` path under `root`.

        """
        return self.root / _MARKER

    @computed_field  # type: ignore[prop-decorator]  # pydantic computed_field over a property
    @property
    def agent_context(self) -> dict[str, str]:
        """Build log and trace correlation tags.

        Returns:
            Mapping of run and agent task identifiers.

        """
        return {"run.id": self.run_id, "agent.task.id": self.agent_task_id}

    @property
    def store_root(self) -> UPath:
        """Resolve the assay artifact root path.

        Returns:
            The `.artifacts/assay` path under `root`.

        """
        return self.root / _ARTIFACTS / _ASSAY

    def store(self, *, protocol: str = "file", **opts: object) -> ArtifactStore:
        """Create an artifact store backend.

        Args:
            protocol: fsspec protocol, such as `file` or `memory`.
            **opts: Backend options forwarded to fsspec.

        Returns:
            Artifact store rooted for this run.

        """
        match protocol:
            case "file":
                target = str(self.store_root)
            case _:
                target = f"{_ARTIFACTS}/{_ASSAY}/{self.run_id}"
        return ArtifactStore(fs=fsspec.filesystem(protocol, **opts), root=target)

    def artifact(self, kind: ArtifactKind, *parts: str | UPath) -> UPath:
        """Project an artifact path from kind and path parts.

        Args:
            kind: Artifact namespace.
            *parts: Additional path components.

        Returns:
            Path under the assay artifact root.

        """
        return self.store_root.joinpath(kind.value, *(str(p) for p in parts))


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class ArtifactStore:
    """Artifact filesystem backend.

    Attributes:
        fs: fsspec filesystem used for artifact I/O.
        root: Backend-relative artifact root.

    """

    fs: AbstractFileSystem
    root: str

    def ensure(self, *parts: str) -> str:
        """Create and return a directory under the store root.

        Args:
            *parts: Path components under the store root.

        Returns:
            Backend path to the ensured directory.

        """
        path = "/".join((self.root, *parts))
        self.fs.makedirs(path, exist_ok=True)
        return path

    def glob(self, pattern: str) -> tuple[str, ...]:
        """Expand a glob under the store root.

        Args:
            pattern: Glob pattern relative to the store root.

        Returns:
            Matching backend paths.

        """
        return tuple(self.fs.glob(f"{self.root}/{pattern}"))

    def exists(self, *parts: str) -> bool:
        """Check whether a store-relative path exists.

        Args:
            *parts: Path components under the store root.

        Returns:
            Whether the path exists.

        """
        return bool(self.fs.exists("/".join((self.root, *parts))))


@dataclass(frozen=True, slots=True)
class ArtifactScope:
    """Dotnet artifact scope for one run or build closure.

    Attributes:
        store: Store that owns the scope directory.
        path: Backend path of the scope directory.
        dotnet_flags: Dotnet flags that route artifacts into this scope.

    """

    store: ArtifactStore
    path: str
    dotnet_flags: tuple[str, ...]

    @classmethod
    def open(cls, settings: AssaySettings, claim: Claim) -> ArtifactScope:
        """Open a per-run artifact scope.

        Args:
            settings: Runtime settings.
            claim: Claim namespace for the run.

        Returns:
            Artifact scope under `.artifacts/assay/<claim>/<run_id>`.

        """
        store = settings.store()
        path = store.ensure(claim.value, settings.run_id)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @classmethod
    def build(cls, settings: AssaySettings, closure: str) -> ArtifactScope:
        """Open a stable build-closure artifact scope.

        Args:
            settings: Runtime settings.
            closure: Closure identifier.

        Returns:
            Artifact scope under `.artifacts/assay/build/<closure>`.

        """
        store = settings.store()
        path = store.ensure(_BUILD, closure)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @property
    def dotnet_env(self) -> dict[str, str]:
        """Build dotnet environment isolation variables.

        Returns:
            Environment overlay for dotnet commands in this scope.

        """
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "MSBUILDDISABLENODEREUSE": "1"}


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["AssaySettings", "ArtifactScope", "ArtifactStore", "Configuration", "LogFormat"]
