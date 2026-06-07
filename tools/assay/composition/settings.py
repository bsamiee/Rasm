"""Define Assay settings, artifact stores, and .NET artifact scopes."""

import contextlib
from dataclasses import dataclass
from datetime import datetime, UTC
from enum import StrEnum
import os
import sys
from typing import Annotated, Final, override, Protocol, runtime_checkable, TYPE_CHECKING
from urllib.parse import urlsplit

import fsspec  # type: ignore[import-untyped]  # fsspec ships no py.typed marker
from pydantic import AliasChoices, BaseModel, BeforeValidator, computed_field, ConfigDict, Field, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict
from upath import UPath  # pathlib-compatible path for local and fsspec-backed artifact stores


if TYPE_CHECKING:
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

_MARKER: Final[str] = "Workspace.slnx"
_ARTIFACTS: Final[str] = ".artifacts"
_ASSAY: Final[str] = "assay"
_BUILD: Final[str] = "build"
_DISABLE_BUILD_SERVERS: Final[str] = "--disable-build-servers"
_ARTIFACTS_PATH_FLAG: Final[str] = "--artifacts-path"
_RUN_ID_PATTERN: Final[str] = r"^[A-Za-z0-9_.-]+$"
_REMOTE_ENV_NAMES: Final[frozenset[str]] = frozenset((
    "ASSAY_AGENT_TASK_ID",
    "ASSAY_ARTIFACT_RETENTION",
    "ASSAY_EXEC_TARGET",
    "ASSAY_RUN_ID",
    "HOME",
    "LANG",
    "LC_ALL",
    "OTEL_EXPORTER_OTLP_ENDPOINT",
    "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT",
    "OTEL_SERVICE_NAME",
    "PATH",
))
_TRACE_CONTEXT_ENV: Final[frozenset[str]] = frozenset(("traceparent", "tracestate", "baggage"))
_PYTHON_TOOL_ENV: Final[dict[str, str]] = {
    "UV_CACHE_DIR": ".cache/uv",
    "HYPOTHESIS_STORAGE_DIRECTORY": ".cache/hypothesis",
    "RUFF_CACHE_DIR": ".cache/ruff",
    "MYPY_CACHE_DIR": ".cache/mypy",
    "COVERAGE_FILE": ".cache/coverage/.coverage",
}


# --- [BOUNDARIES] -----------------------------------------------------------------------


def _anchor(value: str | UPath) -> UPath:
    # Normalize once at ingress so all readers see an absolute UPath anchored to the nearest workspace marker.
    cursor = UPath(value).expanduser().resolve()
    return next((p for p in (cursor, *cursor.parents) if (p / _MARKER).is_file()), cursor)


def _expand_or_none(value: str | UPath | None) -> str | None:
    match value:
        case None | "":
            return None
        case _:
            return str(UPath(value).expanduser())


def _safe_segment(part: str | UPath) -> str:
    text = str(part).replace("\\", "/")
    pieces = tuple(p for p in text.split("/") if p)
    match (
        text.startswith("/"),
        text in {"", ".", ".."},
        any(p in {".", ".."} for p in pieces),
        len(pieces) == 1 and text == pieces[0],
        "\x00" in text,
    ):
        case (False, False, False, True, False):
            return text
        case _:
            raise ValueError(f"unsafe artifact path segment: {text!r}")


# --- [TYPES] ----------------------------------------------------------------------------

type AnchoredRoot = Annotated[UPath, BeforeValidator(_anchor)]
type ExpandedPath = Annotated[UPath, BeforeValidator(lambda v: UPath(v).expanduser())]
type ExpandedKnownHosts = Annotated[str | None, BeforeValidator(_expand_or_none)]
type RunId = Annotated[str, Field(min_length=1, max_length=160, pattern=_RUN_ID_PATTERN)]
type StoreProtocol = Annotated[str, Field(min_length=1, max_length=64, pattern=r"^[A-Za-z][A-Za-z0-9+.-]*$")]


@runtime_checkable
class ArtifactFileSystem(Protocol):
    """Structural subset of fsspec used by Assay artifact storage."""

    def makedirs(self, path: str, *, exist_ok: bool = False) -> object:
        """Create a directory path if needed.

        Returns:
            Backend-specific creation result.
        """

    def glob(self, path: str) -> list[str]:
        """Expand a backend glob.

        Returns:
            Matching backend paths.
        """

    def ls(self, path: str, *, detail: bool = False) -> list[str] | list[dict[str, object]]:
        """List a backend path.

        Returns:
            Backend path rows or detail dictionaries.
        """

    def find(self, path: str, *, detail: bool = False) -> list[str] | dict[str, dict[str, object]]:
        """Recursively list backend paths.

        Returns:
            Backend paths or detail dictionaries keyed by path.
        """

    def info(self, path: str) -> dict[str, object]:
        """Return backend metadata for a path."""

    def exists(self, path: str) -> bool:
        """Return whether the backend path exists."""

    def pipe_file(self, path: str, value: bytes) -> object:
        """Write bytes to a backend file.

        Returns:
            Backend-specific write result.
        """

    def cat_file(self, path: str) -> bytes:
        """Read bytes from a backend file.

        Returns:
            File payload bytes.
        """

    def rm(self, path: str, *, recursive: bool = False) -> object:
        """Remove a backend path.

        Returns:
            Backend-specific removal result.
        """


# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactBackend(BaseModel):
    """Validated artifact backend selected by settings."""

    model_config = ConfigDict(frozen=True, extra="forbid")

    protocol: StoreProtocol = "file"
    root: str = ""

    def target(self, settings: AssaySettings) -> str:
        """Project the backend root without smuggling run scope into storage ownership.

        Returns:
            Backend root path used by `ArtifactStore`.
        """
        match (self.protocol, self.root):
            case ("file", ""):
                return str(settings.store_root)
            case ("file", root):
                candidate = UPath(root).expanduser()
                return str(candidate if candidate.is_absolute() else settings.root / candidate)
            case (_, root) if root:
                return root.rstrip("/")
            case _:
                return f"{_ARTIFACTS}/{_ASSAY}"


class AssaySettings(BaseSettings):
    """Validated Assay runtime settings."""

    model_config = SettingsConfigDict(
        env_prefix="ASSAY_",
        env_nested_delimiter="__",
        nested_model_default_partial_update=True,
        case_sensitive=False,
        frozen=True,
        extra="forbid",
        populate_by_name=True,
        env_ignore_empty=True,
        env_parse_enums=True,
    )

    root: AnchoredRoot = Field(default_factory=UPath.cwd)
    configuration: Configuration = Configuration.RELEASE
    dotnet_max_cpu: Annotated[int, Field(ge=1, le=64)] = 4
    mutation_max_cpu: Annotated[int, Field(ge=1, le=64)] = 2
    max_checks: Annotated[int, Field(ge=1, le=64)] = 8
    stream_tail_bytes: Annotated[int, Field(ge=512)] = 4096
    stream_chunk_bytes: Annotated[int, Field(ge=4096)] = 65536
    lease_drift_tolerance: Annotated[float, Field(gt=0)] = 1.0
    artifact_retention: Annotated[int, Field(ge=1, le=10000)] = 50
    artifact_backend: ArtifactBackend = Field(default_factory=ArtifactBackend)
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
    trigger_prefixes: tuple[str, ...] = ("tools/cs-analyzer/",)
    test_target: ExpandedPath = UPath("tests/csharp/libs/Rasm/Rasm.Tests.csproj")
    log_format: LogFormat = Field(default_factory=lambda: LogFormat.HUMAN if sys.stderr.isatty() else LogFormat.CI)
    run_id: RunId = Field(default_factory=lambda: f"{datetime.now(tz=UTC):%Y-%m-%dT%H-%M-%S.%f}-{os.getpid()}")
    agent_task_id: str = ""
    exec_target: str = Field(default="", description="execution target; '' = local, ssh://[user@]host[:port] = remote")
    exec_known_hosts: ExpandedKnownHosts = Field(
        default=str(UPath("~/.ssh/known_hosts").expanduser()),
        description="asyncssh known_hosts path for ssh:// exec_target; empty disables the host-key check",
    )
    otel_endpoint: str = Field(
        default="", validation_alias=AliasChoices("ASSAY_OTEL_ENDPOINT", "OTEL_EXPORTER_OTLP_ENDPOINT", "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT")
    )

    @field_validator("run_id", "agent_task_id")
    @classmethod
    def _wire_safe(cls, value: str) -> str:  # pydantic field validators are class-bound
        # Scrub lone surrogates (surrogateescape from an invalid-UTF-8 ASSAY_RUN_ID/ASSAY_AGENT_TASK_ID env): these
        # tags flow into agent_context, the structlog CI JSONRenderer, and the Envelope wire, all of which choke on
        # un-encodable surrogates. The default_factory value is already clean, so this only ever scrubs env input.
        return value.encode("utf-8", "replace").decode("utf-8")

    @field_validator("exec_target")
    @classmethod
    def _exec_target(cls, value: str) -> str:  # pydantic field validators are class-bound
        # Validate scheme, host, and port before engine._run_remote can raise mid-spawn.
        match value:
            case "":
                return value
            case _:
                parts = urlsplit(value)
                match (parts.scheme, parts.hostname, parts.path, parts.query, parts.fragment):
                    case ("ssh", str() as host, "" | "/", "", "") if host:
                        try:
                            _ = parts.port
                        except ValueError as exc:
                            raise ValueError(f"exec_target has a non-numeric ssh port: {value!r}") from exc
                        return value
                    case _:
                        raise ValueError(f"exec_target must be '' (local) or 'ssh://[user@]host[:port]' without path/query/fragment: {value!r}")

    @override
    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> tuple[PydanticBaseSettingsSource, ...]:
        """Use init values and `ASSAY_*` environment variables as the only settings sources."""
        return (init_settings, env_settings)

    @computed_field  # type: ignore[prop-decorator]  # pydantic computed_field over a property
    @property
    def solution(self) -> UPath:
        """Resolve `Workspace.slnx` under the anchored root."""
        return self.root / _MARKER

    @computed_field  # type: ignore[prop-decorator]  # pydantic computed_field over a property
    @property
    def agent_context(self) -> dict[str, str]:
        """Build log and trace correlation tags."""
        return {"run.id": self.run_id, "agent.task.id": self.agent_task_id}

    @property
    def python_tool_env(self) -> dict[str, str]:
        """Project repo-local Python tool storage variables for subprocess launches."""
        return dict(_PYTHON_TOOL_ENV)

    @property
    def store_root(self) -> UPath:
        """Resolve the `.artifacts/assay` root path."""
        return self.root / _ARTIFACTS / _ASSAY

    @property
    def local_root(self) -> UPath:
        """Return the local workspace root usable as a subprocess cwd.

        Raises:
            ValueError: When `root` is not a local file path.
        """
        match self.root.protocol:
            case "" | "file":
                return self.root
            case protocol:
                raise ValueError(f"root must use file protocol for process execution, got {protocol!r}")

    def store(self, *, protocol: str | None = None, root: str | None = None, **opts: object) -> ArtifactStore:
        """Create an artifact store backend.

        Returns:
            Artifact store bound to the requested backend.
        """
        backend = self.artifact_backend.model_copy(update={k: v for k, v in {"protocol": protocol, "root": root}.items() if v is not None})
        return ArtifactStore(fs=fsspec.filesystem(backend.protocol, **opts), root=backend.target(self))

    def artifact(self, kind: ArtifactKind, *parts: str | UPath) -> UPath:
        """Project an artifact path from a kind and path parts.

        Returns:
            Store-rooted artifact path.
        """
        return self.store_root.joinpath(kind.value, *(_safe_segment(p) for p in parts))

    def remote_env(self, env: dict[str, str]) -> dict[str, str]:
        """Project the environment subset safe to export through an SSH command string.

        Returns:
            Environment variables allowed to cross the SSH execution boundary.
        """
        safe_names = frozenset((*self.python_tool_env, *_REMOTE_ENV_NAMES, *_TRACE_CONTEXT_ENV))
        return {key: value for key, value in env.items() if key in safe_names and key.replace("_", "").isalnum()}


# ArtifactBackend references AssaySettings in an annotation; rebuild after AssaySettings exists.
ArtifactBackend.model_rebuild()


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)  # noqa: PLR0904  # ArtifactStore is the single public storage owner; splitting operations would weaken the boundary.
class ArtifactStore:
    """Artifact filesystem backend."""

    fs: ArtifactFileSystem
    root: str

    def path(self, *parts: str | UPath) -> str:
        """Build a validated store-relative backend path.

        Returns:
            Backend path under the store root.
        """
        return "/".join((self.root, *(_safe_segment(p) for p in parts)))

    def ensure(self, *parts: str) -> str:
        """Create and return a directory under the store root.

        Returns:
            Store-relative directory path.
        """
        path = self.path(*parts)
        self.fs.makedirs(path, exist_ok=True)
        return path

    def glob(self, pattern: str) -> tuple[str, ...]:
        """Expand a glob under the store root.

        Returns:
            Matching store paths.
        """
        return tuple(self._normalize_backend_path(path) for path in sorted(self.fs.glob(f"{self.root}/{pattern}")))

    def list(self, *parts: str | UPath) -> tuple[str, ...]:
        """List direct children under a store-relative path.

        Returns:
            Matching backend paths.
        """
        rows = self.fs.ls(self.path(*parts), detail=False)
        paths = tuple(str(path) for path in rows)
        return tuple(self._normalize_backend_path(path) for path in sorted(paths))

    def find(self, *parts: str | UPath) -> tuple[str, ...]:
        """Recursively list paths under a store-relative path.

        Returns:
            Matching backend paths.
        """
        rows = self.fs.find(self.path(*parts), detail=False)
        paths = tuple(str(path) for path in rows)
        return tuple(self._normalize_backend_path(path) for path in sorted(paths))

    def info(self, *parts: str | UPath) -> dict[str, object]:
        """Return backend metadata for a store-relative path."""
        return self.fs.info(self.path(*parts))

    def info_path(self, path: str | UPath) -> dict[str, object]:
        """Return backend metadata for a previously emitted backend path."""
        return self.fs.info(self.backend_path(path))

    def exists_path(self, path: str | UPath) -> bool:
        """Return whether a previously emitted backend path exists."""
        return bool(self.fs.exists(self.backend_path(path)))

    def size_path(self, path: str | UPath, *, fallback: int = 0) -> int:
        """Return a backend path size as an int.

        Returns:
            File size, or `fallback` when the backend omits size metadata.
        """
        value = self.info_path(path).get("size", fallback)
        return value if isinstance(value, int) else fallback

    def mtime_path(self, path: str | UPath, *, fallback: float = 0.0) -> float:
        """Return a backend path modification time as a float.

        Returns:
            Modification time, or `fallback` when the backend omits mtime metadata.
        """
        value = self.info_path(path).get("mtime", fallback)
        return value if isinstance(value, int | float) else fallback

    def backend_path(self, path: str | UPath) -> str:
        """Validate and normalize a backend path previously emitted by this store.

        Returns:
            Backend path under this store root.

        Raises:
            ValueError: When the path is outside this store root.
        """
        text = str(path).replace("\\", "/")
        normalized = self._normalize_backend_path(text)
        match normalized == self.root or normalized.startswith(f"{self.root}/"):
            case True:
                return normalized
            case False:
                raise ValueError(f"artifact path escaped store root: {text!r}")

    def _normalize_backend_path(self, path: str) -> str:
        return path.removeprefix("/") if not self.root.startswith("/") else path

    def exists(self, *parts: str) -> bool:
        """Return whether a store-relative path exists."""
        return bool(self.fs.exists(self.path(*parts)))

    def read_bytes(self, *parts: str | UPath) -> bytes:
        """Read bytes from a validated store-relative path.

        Returns:
            File payload bytes.
        """
        return self.fs.cat_file(self.path(*parts))

    def read_text(self, *parts: str | UPath, encoding: str = "utf-8", errors: str = "replace") -> str:
        """Read text from a validated store-relative path.

        Returns:
            Decoded text payload.
        """
        return self.read_bytes(*parts).decode(encoding, errors=errors)

    def read_path(self, path: str | UPath) -> bytes:
        """Read bytes from a validated backend path returned by the store.

        Returns:
            File payload bytes.
        """
        return self.fs.cat_file(self.backend_path(path))

    def read_text_path(self, path: str | UPath, encoding: str = "utf-8", errors: str = "replace") -> str:
        """Read text from a validated backend path returned by the store.

        Returns:
            Decoded text payload.
        """
        return self.read_path(path).decode(encoding, errors=errors)

    def write_bytes(self, payload: bytes, *parts: str | UPath, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated store-relative path and return the backend path.

        Returns:
            Backend path that received the payload.

        Raises:
            FileExistsError: When `create` is true and the target already exists.
        """
        path = self.path(*parts)
        parent = path.rsplit("/", 1)[0] if "/" in path else self.root
        with getattr(self.fs, "transaction", contextlib.nullcontext()) if transaction else contextlib.nullcontext():
            self.fs.makedirs(parent, exist_ok=True)
            if create and self.fs.exists(path):
                raise FileExistsError(path)
            self.fs.pipe_file(path, payload)
        return path

    def write_text(self, payload: str, *parts: str | UPath, create: bool = False, transaction: bool = False, encoding: str = "utf-8") -> str:
        """Write text to a validated store-relative path and return the backend path.

        Returns:
            Backend path that received the payload.
        """
        return self.write_bytes(payload.encode(encoding), *parts, create=create, transaction=transaction)

    def write_bytes_path(self, payload: bytes, path: str | UPath, *, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated backend path emitted by this store.

        Returns:
            Backend path that received the payload.

        Raises:
            FileExistsError: When `create` is true and the target already exists.
        """
        target = self.backend_path(path)
        parent = target.rsplit("/", 1)[0] if "/" in target else self.root
        with getattr(self.fs, "transaction", contextlib.nullcontext()) if transaction else contextlib.nullcontext():
            self.fs.makedirs(parent, exist_ok=True)
            if create and self.fs.exists(target):
                raise FileExistsError(target)
            self.fs.pipe_file(target, payload)
        return target

    def write_text_path(self, payload: str, path: str | UPath, *, create: bool = False, transaction: bool = False, encoding: str = "utf-8") -> str:
        """Write text to a validated backend path emitted by this store.

        Returns:
            Backend path that received the payload.
        """
        return self.write_bytes_path(payload.encode(encoding), path, create=create, transaction=transaction)

    def remove(self, *parts: str | UPath, recursive: bool = False) -> object:
        """Remove a validated store-relative path.

        Returns:
            Backend-specific removal result.
        """
        return self.fs.rm(self.path(*parts), recursive=recursive)

    def remove_path(self, path: str | UPath, *, recursive: bool = False) -> object:
        """Remove a validated backend path returned by the store.

        Returns:
            Backend-specific removal result.
        """
        return self.fs.rm(self.backend_path(path), recursive=recursive)


@dataclass(frozen=True, slots=True)
class ArtifactScope:
    """Scoped .NET artifact directory and command flags."""

    store: ArtifactStore
    path: str
    dotnet_flags: tuple[str, ...]

    @classmethod
    def open(cls, settings: AssaySettings, claim: Claim) -> ArtifactScope:
        """Open a per-run artifact scope for a claim.

        Returns:
            Artifact scope rooted under the claim and run id.
        """
        store = settings.store()
        path = store.ensure(claim.value, settings.run_id)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @classmethod
    def build(cls, settings: AssaySettings, closure: str) -> ArtifactScope:
        """Open a stable build-closure artifact scope.

        Returns:
            Artifact scope rooted under the build closure id.
        """
        store = settings.store()
        path = store.ensure(_BUILD, closure)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @property
    def dotnet_env(self) -> dict[str, str]:
        """Build `.NET` command environment isolation variables."""
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "MSBUILDDISABLENODEREUSE": "1"}


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["ArtifactBackend", "AssaySettings", "ArtifactScope", "ArtifactStore", "Configuration", "LogFormat"]
