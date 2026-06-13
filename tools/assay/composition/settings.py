"""Assay runtime configuration, artifact backend, and .NET artifact scope surfaces.

Three public surfaces compose the settings layer: AssaySettings owns all validated
runtime parameters sourced from ASSAY_* environment variables; ArtifactStore is the
single filesystem owner that routes reads and writes through a pluggable fsspec backend;
ArtifactScope projects per-run or per-closure artifact directories together with the
MSBuild flags that redirect .NET build outputs into them.

ArtifactFileSystem defines the structural fsspec subset required by ArtifactStore so
any fsspec-compatible backend (local, memory, GCS, S3) satisfies the protocol without
an adapter layer.
"""

import contextlib
from dataclasses import dataclass
from datetime import datetime, UTC
from enum import StrEnum
import os
from pathlib import Path
import sys
from typing import Annotated, Final, Literal, override, Protocol, runtime_checkable, TYPE_CHECKING
from urllib.parse import urlsplit

import fsspec  # fsspec ships no py.typed marker (declared in [[tool.mypy.overrides]])
import msgspec
from pydantic import AfterValidator, AliasChoices, BaseModel, BeforeValidator, computed_field, ConfigDict, Field, model_validator
from pydantic_settings import (  # noqa: TC002  # Pydantic calls the hook by runtime annotations.
    BaseSettings,
    PydanticBaseSettingsSource,
    SettingsConfigDict,
)
from upath import UPath

from tools.assay.core.model import (  # noqa: TC001  # pydantic_settings and ArtifactBackend field hooks evaluate these annotations at runtime.
    ArtifactKind,
    Claim,
    Envelope,
    Report,
    wire_encode,
    wire_safe,
)


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [TYPES] ----------------------------------------------------------------------------


class Configuration(StrEnum):
    """MSBuild configuration value."""

    DEBUG = "Debug"
    RELEASE = "Release"


class LogFormat(StrEnum):
    """Structured-log renderer format."""

    HUMAN = "human"
    CI = "ci"


type AnchoredRoot = Annotated[UPath, BeforeValidator(_anchor)]
type ExecTarget = Annotated[str, AfterValidator(_exec_target)]
type ExpandedKnownHosts = Annotated[str | None, BeforeValidator(_expand_or_none)]
type ExpandedPath = Annotated[UPath, BeforeValidator(lambda v: UPath(v).expanduser())]
type RunId = Annotated[str, Field(min_length=1, max_length=160, pattern=_RUN_ID_PATTERN), AfterValidator(wire_safe)]
type StoreProtocol = Annotated[str, Field(min_length=1, max_length=64, pattern=r"^[A-Za-z][A-Za-z0-9+.-]*$")]
type WireSafeText = Annotated[str, AfterValidator(wire_safe)]  # surrogateescape arrives only via env input; default_factory values are always clean


@runtime_checkable  # Protocol mirrors the fsspec structural subset ArtifactStore consumes; backends satisfy it without an adapter.
class ArtifactFileSystem(Protocol):
    """Structural subset of fsspec used by Assay artifact storage."""

    def makedirs(self, path: str, *, exist_ok: bool = False) -> object:
        """Create a directory path if needed."""

    def glob(self, path: str) -> list[str]:
        """Expand a backend glob."""

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

    def cat_file(self, path: str) -> bytes:
        """Read bytes from a backend file."""

    def rm(self, path: str, *, recursive: bool = False) -> object:
        """Remove a backend path."""

    def open(self, path: str, mode: str = "rb", *, autocommit: bool = True) -> contextlib.AbstractContextManager[object]:
        """Open a backend file; autocommit=False defers the write to transaction commit (tempfile + move).

        Returns:
            A context manager yielding a writable/readable file handle.
        """

    @property
    def transaction(self) -> contextlib.AbstractContextManager[object]:
        """Return a backend write-transaction context; backends without one fold to nullcontext."""
        return contextlib.nullcontext()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACTS: Final[str] = ".artifacts"
_ARTIFACTS_PATH_FLAG: Final[str] = "--artifacts-path"
_ASSAY: Final[str] = "assay"
_BUILD: Final[str] = "build"
_DISABLE_BUILD_SERVERS: Final[str] = "--disable-build-servers"
_MARKER: Final[str] = "Workspace.slnx"
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
    "RHINO_WIP_APP_PATH",
))
# Lowercase names are load-bearing: W3C Trace Context defines these keys lowercase and propagate.inject emits them verbatim.
_TRACE_CONTEXT_ENV: Final[frozenset[str]] = frozenset(("traceparent", "tracestate", "baggage"))
_PYTHON_TOOL_ENV_REL: Final[dict[str, str]] = {
    "UV_CACHE_DIR": ".cache/uv",
    "HYPOTHESIS_STORAGE_DIRECTORY": ".cache/hypothesis",
    "PYTEST_CACHE_DIR": ".cache/pytest",
    "RUFF_CACHE_DIR": ".cache/ruff",
    "MYPY_CACHE_DIR": ".cache/mypy",
    "COVERAGE_FILE": ".cache/coverage/.coverage",
}

# --- [BOUNDARIES] -----------------------------------------------------------------------


def mtime_from_info(info: dict[str, object]) -> float:
    """Coerce fsspec mtime/created metadata to a POSIX float for history ranking.

    Returns:
        Modification time as a POSIX float, or 0.0 when the backend omits both keys.
    """
    match info.get("mtime", info.get("created", 0.0)):
        case int() | float() as value:
            return float(value)
        case datetime() as value:  # fsspec memory/GCS backends return `created` as a tz-aware datetime instead of a float
            return value.timestamp()
        case _:
            return 0.0


def _anchor(value: str | UPath) -> UPath:
    # Walk ancestors at ingress so every consumer receives an absolute UPath rooted at the workspace, not the cwd.
    cursor = UPath(value).expanduser().resolve()
    return next((p for p in (cursor, *cursor.parents) if (p / _MARKER).is_file()), cursor)


def _default_cpu_count() -> int:
    # os.process_cpu_count honors cgroup/affinity limits; engine._governed reads this via AssaySettings.cpu_count.
    return min(256, max(1, os.process_cpu_count() or 8))


def _exec_target(value: str) -> str:
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


def _expand_or_none(value: str | UPath | None) -> str | None:
    match value:
        case None | "":
            return None
        case _:
            return str(UPath(value).expanduser())


def _root_parts(root: str) -> tuple[str, ...]:
    return tuple(part for part in root.split("/") if part)


def _safe_segment(part: str | UPath) -> str:
    text = str(part).replace("\\", "/")
    pieces = tuple(p for p in text.split("/") if p)
    # `len(pieces) == 1 and text == pieces[0]` subsumes both the leading-slash and "/"/"."/".." rejects:
    # an absolute "/abs", a trailing "a/", and the empty/dot segments all break the single-piece identity.
    match (any(p in {".", ".."} for p in pieces), len(pieces) == 1 and text == pieces[0], "\x00" in text):
        case (False, True, False):
            return text
        case _:
            raise ValueError(f"unsafe artifact path segment: {text!r}")


# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactBackend(BaseModel):
    """Validated artifact backend selected by settings."""

    model_config = ConfigDict(frozen=True, extra="forbid")

    protocol: StoreProtocol = "file"
    root: str = ""

    @model_validator(mode="after")
    def _bounded_non_file_root(self) -> ArtifactBackend:  # noqa: N804  # Pydantic v2 mode="after" passes the model instance, not cls.
        # Non-file backends (e.g., fsspec MemoryFileSystem) are process-global singletons; an empty root collides across parallel stores.
        match (self.protocol, self.root.strip(), ".." in _root_parts(self.root)):
            case ("file", _, _):
                return self
            case (_, "", _):
                raise ValueError(f"non-file backend {self.protocol!r} requires a non-empty root")
            case (_, _, True):
                raise ValueError(f"non-file backend root must not traverse with '..': {self.root!r}")
            case _:
                return self

    def target(self, settings: AssaySettings) -> str:
        """Project the backend root without smuggling run scope into storage ownership.

        Returns:
            Backend root path used by ArtifactStore.
        """
        # _bounded_non_file_root guarantees every non-file backend carries a non-empty root, so the
        # non-file branch needs no empty-root fallback. Discriminating on protocol first keeps the match
        # total for the type checker (file / non-file partition the str) with no unreachable arm.
        match self.protocol:
            case "file" if not self.root:
                return str(settings.store_root)
            case "file":
                candidate = UPath(self.root).expanduser()
                return str(candidate if candidate.is_absolute() else settings.root / candidate)
            case _:
                return self.root.rstrip("/")


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
    cpu_count: Annotated[int, Field(ge=1, le=256)] = Field(default_factory=_default_cpu_count)
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
    probe_fixture_prefixes: tuple[str, ...] = ("tests/ast-grep/", "tests/python/tools/py_analyzer/")
    test_target: ExpandedPath = UPath("tests/csharp/libs/Rasm/Rasm.Tests.csproj")
    mutation_python: str = "3.14.5"
    log_format: LogFormat = Field(default_factory=lambda: LogFormat.HUMAN if sys.stderr.isatty() else LogFormat.CI)
    log_level: Literal["debug", "info", "warning", "error", "critical"] = "info"
    run_id: RunId = Field(default_factory=lambda: f"{datetime.now(tz=UTC):%Y-%m-%dT%H-%M-%S.%f}-{os.getpid()}")
    agent_task_id: WireSafeText = ""
    exec_target: ExecTarget = Field(default="", description="execution target; '' = local, ssh://[user@]host[:port] = remote")
    exec_known_hosts: ExpandedKnownHosts = Field(
        default=str(UPath("~/.ssh/known_hosts").expanduser()),
        description="asyncssh known_hosts path for ssh:// exec_target; empty disables the host-key check",
    )
    otel_endpoint: str = Field(
        default="", validation_alias=AliasChoices("ASSAY_OTEL_ENDPOINT", "OTEL_EXPORTER_OTLP_ENDPOINT", "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT")
    )

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
        """Use init values and ASSAY_* environment variables as the only settings sources."""
        _ = (settings_cls, dotenv_settings, file_secret_settings)
        return (init_settings, env_settings)

    @computed_field  # type: ignore[prop-decorator]  # mypy Pydantic plugin does not model computed_field stacked on property
    @property
    def solution(self) -> UPath:
        """Resolve Workspace.slnx under the anchored root."""
        return self.root / _MARKER

    @computed_field  # type: ignore[prop-decorator]  # mypy Pydantic plugin does not model computed_field stacked on property
    @property
    def agent_context(self) -> dict[str, str]:
        """Build log and trace correlation tags."""
        return {"run.id": self.run_id, "agent.task.id": self.agent_task_id}

    @property
    def python_tool_env(self) -> dict[str, str]:
        """Project repo-local Python tool storage variables for subprocess launches."""
        local = self.local_root
        return {key: str(local / rel) for key, rel in _PYTHON_TOOL_ENV_REL.items()}

    @property
    def store_root(self) -> UPath:
        """Resolve the .artifacts/assay root path."""
        return self.root / _ARTIFACTS / _ASSAY

    @property
    def local_root(self) -> UPath:
        """Return the local workspace root usable as a subprocess cwd.

        Raises:
            ValueError: When root is not a local file path.
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
        backend = ArtifactBackend.model_validate({
            **self.artifact_backend.model_dump(),
            **{k: v for k, v in {"protocol": protocol, "root": root}.items() if v is not None},
        })
        return ArtifactStore(fs=fsspec.filesystem(backend.protocol, **opts), root=backend.target(self))

    def with_configuration(self, configuration: Configuration) -> AssaySettings:
        """Return these settings rebound to a different build configuration."""
        return self.model_copy(update={"configuration": configuration})

    def artifact(self, kind: ArtifactKind, *parts: str | UPath) -> UPath:
        """Project an artifact path from a kind and path parts.

        Returns:
            Store-rooted artifact path.
        """
        return self.store_root.joinpath(kind.value, *(_safe_segment(p) for p in parts))

    def remote_env(self, env: dict[str, str], *, forward: frozenset[str] = frozenset()) -> dict[str, str]:
        """Project the environment subset safe to export through an SSH command string.

        ``forward`` names keys declared by the row Tool's own ``env`` — a deliberate per-spawn
        declaration that crosses the SSH boundary explicitly alongside the ambient allowlist; the
        allowlist continues to gate ambient (host-inherited) environment that was never declared.

        Returns:
            Environment variables allowed to cross the SSH execution boundary.
        """
        # ASSAY_RUN_ID lives only in default_factory (never os.environ), so inject it explicitly for remote process run-id sharing.
        safe_names = frozenset((*self.python_tool_env, *_REMOTE_ENV_NAMES, *_TRACE_CONTEXT_ENV, *forward))
        source = {"ASSAY_RUN_ID": self.run_id, **env}
        return {k: v for k, v in source.items() if k in safe_names}


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)  # noqa: PLR0904  # ArtifactStore is the single public storage owner; splitting would weaken the boundary.
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

    def ensure_path(self, path: str | UPath) -> str:
        """Create a directory at a backend path previously emitted by this store.

        Routes the makedirs through the store boundary so rails never reach the fsspec
        handle directly; validates the path against the store root before creating it.

        Returns:
            The validated, created backend path.
        """
        validated = self.backend_path(path)
        self.fs.makedirs(validated, exist_ok=True)
        return validated

    def glob(self, pattern: str) -> tuple[str, ...]:
        """Expand a glob under the store root.

        Returns:
            Matching store paths.
        """
        return tuple(self._normalize_backend_path(path) for path in sorted(self.fs.glob(f"{self.root}/{pattern}")))

    def walk(self, *parts: str | UPath, recursive: bool = False, detail: bool = False) -> tuple[str, ...] | tuple[tuple[str, dict[str, object]], ...]:
        """List children under a store-relative path, optionally recursive and with metadata.

        recursive chooses find over ls; detail projects (path, metadata) rows over plain paths.

        Returns:
            Matching backend paths, or (path, metadata) rows when detail is set; empty tuple when the path is absent.
        """
        base = self.path(*parts)
        try:
            rows = self.fs.find(base, detail=detail) if recursive else self.fs.ls(base, detail=detail)
        except FileNotFoundError:
            return ()
        match (detail, rows):
            case (False, list() as paths):
                return tuple(self._normalize_backend_path(str(path)) for path in sorted(str(p) for p in paths))
            case (True, dict() as keyed):
                return tuple(sorted((self._normalize_backend_path(str(path)), dict(info)) for path, info in keyed.items()))
            case (True, list() as details):
                return tuple(
                    sorted((self._normalize_backend_path(str(row.get("name", row.get("path", "")))), row) for row in details if isinstance(row, dict))
                )
            case _:
                return ()

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
            File size, or fallback when the backend omits size metadata.
        """
        value = self.info_path(path).get("size", fallback)
        return value if isinstance(value, int) else fallback

    def mtime_path(self, path: str | UPath, *, fallback: float = 0.0) -> float:
        """Return a backend path modification time as a float.

        Returns:
            Modification time, or fallback when the backend omits mtime metadata.
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

    def _write_at(self, path: str, payload: bytes, *, create: bool, transaction: bool) -> str:
        parent = path.rsplit("/", 1)[0] if "/" in path else self.root
        with self.fs.transaction if transaction else contextlib.nullcontext():
            self.fs.makedirs(parent, exist_ok=True)
            if create and self.fs.exists(path):
                raise FileExistsError(path)
            # Deferred commit (LocalFileOpener: tempfile + atomic move) only under the backend transaction; a
            # standalone write commits directly — autocommit=False outside a transaction never materializes.
            with self.fs.open(path, "wb", autocommit=not transaction) as fh:
                # fsspec file handles expose write; the Protocol yields the structural minimum (object) across backends.
                fh.write(payload)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return path

    def write_bytes(self, payload: bytes, *parts: str | UPath, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated store-relative path; raises FileExistsError when create is true and the target exists.

        Returns:
            Backend path that received the payload.
        """
        return self._write_at(self.path(*parts), payload, create=create, transaction=transaction)

    def write_text(self, payload: str, *parts: str | UPath, create: bool = False, transaction: bool = False, encoding: str = "utf-8") -> str:
        """Write text to a validated store-relative path and return the backend path.

        Returns:
            Backend path that received the payload.
        """
        return self.write_bytes(payload.encode(encoding), *parts, create=create, transaction=transaction)

    def write_bytes_path(self, payload: bytes, path: str | UPath, *, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated backend path emitted by this store; raises FileExistsError when create is true and the target exists.

        Returns:
            Backend path that received the payload.
        """
        return self._write_at(self.backend_path(path), payload, create=create, transaction=transaction)

    def open_write(self, *parts: str | UPath) -> tuple[str, object]:
        """Open a validated store-relative backend file for incremental byte writes.

        Returns:
            Backend path and writable file object.
        """
        path = self.path(*parts)
        parent = path.rsplit("/", 1)[0] if "/" in path else self.root
        self.fs.makedirs(parent, exist_ok=True)
        return path, self.fs.open(path, "wb")

    def write_text_path(self, payload: str, path: str | UPath, *, create: bool = False, transaction: bool = False, encoding: str = "utf-8") -> str:
        """Write text to a validated backend path emitted by this store.

        Returns:
            Backend path that received the payload.
        """
        return self.write_bytes_path(payload.encode(encoding), path, create=create, transaction=transaction)

    def write_many(self, rows: tuple[tuple[bytes, tuple[str | UPath, ...]], ...], *, transaction: bool = True) -> tuple[str, ...]:
        """Write multiple payloads, each committed atomically when the backend supports transactions.

        Segment validation runs over every part-tuple before the first write, so an unsafe segment cannot leave a
        half-written batch behind.

        Returns:
            Backend paths that received the payloads.
        """
        resolved = tuple((self.path(*parts), payload) for payload, parts in rows)
        return tuple(self._write_at(path, payload, create=False, transaction=transaction) for path, payload in resolved)

    def adopt_file(self, source: str | UPath | Path, *parts: str | UPath) -> str:
        """Copy a local tool-produced file into the configured artifact backend.

        Returns:
            Backend path that received the copied payload.
        """
        return self.write_bytes(Path(str(source)).read_bytes(), *parts)

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

    def write_history(self, run_id: str, payload: bytes) -> str:
        """Persist one encoded Envelope in run history.

        Returns:
            Backend path written.
        """
        return self.write_bytes(payload, ArtifactKind.HISTORY.value, run_id, "envelope.json")

    def write_full_report(self, run_id: str, name: str, report: Report) -> tuple[str, int]:
        """Persist an unclipped Report before previewing or clipping.

        Returns:
            Backend path and payload byte count.
        """
        payload = wire_encode(report)
        return self.write_bytes(payload, ArtifactKind.HISTORY.value, run_id, name), len(payload)

    def _sorted_run_ids(self, root: str) -> tuple[str, ...]:
        # Rank run-dir ids oldest-first by (mtime, run_id) within one root; an omitted-mtime backend folds to 0.0,
        # leaving the lexicographic run id as the chronological tiebreaker.
        detailed = self.walk(root, detail=True)
        # walk return is a union; the isinstance guard narrows to detail rows
        rows = tuple((path.rstrip("/").rsplit("/", 1)[-1], info) for path, info in detailed if isinstance(info, dict)) or tuple(  # type: ignore[str-unpack]
            (path.rstrip("/").rsplit("/", 1)[-1], {}) for path in self.glob(f"{root}/*")
        )
        return tuple(run_id for run_id, _ in sorted(rows, key=lambda row: (mtime_from_info(row[1]), row[0])))

    def _prune(self, root: str, keep: int) -> None:
        runs = self._sorted_run_ids(root)
        for run_id in runs[: max(0, len(runs) - keep)]:
            try:
                self.remove(root, run_id, recursive=True)
            except FileNotFoundError:
                _ = self.exists(root, run_id)

    def sorted_history_ids(self) -> tuple[str, ...]:
        """Return history run ids ranked oldest-first by (mtime, run_id) within the history root.

        Backends that omit mtime fold to 0.0, leaving the lexicographic run id as the chronological tiebreaker.

        Returns:
            Run ids ordered oldest-first.
        """
        return self._sorted_run_ids(ArtifactKind.HISTORY.value)

    def retain_history(self, keep: int) -> None:
        """Prune old run history by backend metadata age, keeping the newest keep runs."""
        self._prune(ArtifactKind.HISTORY.value, keep)

    def retain_scopes(self, claim: Claim, keep: int) -> None:
        """Prune old per-claim scope run-dirs by backend metadata age, keeping the newest keep runs.

        Mirrors retain_history over a claim root (`<claim>/<run_id>/`), bounding the artifact scope pile-up that
        ArtifactScope.open creates per rail run; prune order is oldest-first by (mtime, run_id).
        """
        self._prune(claim.value, keep)

    def load_history(self, run_id: str) -> Envelope | None:
        """Load one run Envelope and restore its full report artifact when available.

        Returns:
            Restored Envelope, or None when the run cannot be read.
        """
        match run_id:
            case "":
                return None
            case _:
                try:
                    env = msgspec.json.decode(self.read_bytes(ArtifactKind.HISTORY.value, run_id, "envelope.json"), type=Envelope)
                    return self.restore_full_report(env)
                except OSError, msgspec.MsgspecError:
                    return None

    def restore_full_report(self, env: Envelope) -> Envelope:
        """Replace a clipped report with its full-report artifact when present.

        Returns:
            Envelope with the full report restored when possible.
        """
        match env.report:
            case None:
                return env
            case report:
                artifact = next((a for a in report.artifacts if a.id == "full-report"), None)
                if artifact is None:
                    return env
                try:
                    restored = msgspec.json.decode(self.read_path(artifact.path), type=Report)
                except OSError, msgspec.MsgspecError:
                    return env
                return msgspec.structs.replace(env, report=restored)

    def resolve_artifacts(self, token: str, *roots: str, latest: bool = False) -> tuple[str, ...]:
        """Resolve a token through direct paths, basenames, substring matches, or explicit latest lookup.

        Returns:
            Matching backend paths ordered newest-first.
        """
        try:
            direct = self.backend_path(token)
        except ValueError:
            direct = ""
        if direct and self.exists_path(direct):
            return (direct,)
        if latest:
            return next((ranked for root in roots if (ranked := self._ranked_files(root))), ())
        if not token.strip():
            return ()
        stem = token.rsplit("/", 1)[-1]
        return self._ranked_files(*roots, accept=lambda path: token in {path, stem} or token in path)

    def _ranked_files(self, *roots: str, accept: Callable[[str], bool] = lambda _path: True) -> tuple[str, ...]:
        # fs.find walks files only (withdirs defaults False), replacing the former walk-union + directory filter.
        def rows(root: str) -> tuple[tuple[str, dict[str, object]], ...]:
            try:
                found = self.fs.find(self.path(*_root_parts(root)), detail=True)
            except FileNotFoundError:
                return ()
            return tuple((self._normalize_backend_path(str(path)), dict(info)) for path, info in (found.items() if isinstance(found, dict) else ()))

        matches = tuple((path, info) for root in roots for path, info in rows(root) if accept(path))
        return tuple(path for path, _ in sorted(matches, key=lambda row: (-mtime_from_info(row[1]), row[0])))


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
        # Lazy: compute the path without makedirs — writers makedirs(parent) on first write, and dotnet creates
        # its own --artifacts-path, so an opened-but-unused scope leaves no empty directory behind.
        path = store.path(claim.value, settings.run_id)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @classmethod
    def build(cls, settings: AssaySettings, closure: str, configuration: Configuration | str | None = None) -> ArtifactScope:
        """Open a stable build-closure artifact scope.

        Returns:
            Artifact scope rooted under the build closure id.
        """
        store = settings.store()
        config = configuration.value if isinstance(configuration, Configuration) else configuration or settings.configuration.value
        path = store.ensure(_BUILD, closure, config)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    def ensure(self) -> str:
        """Materialize this scope's directory through the store boundary.

        ArtifactScope.open is lazy (no eager makedirs); a rail that writes its own files into
        the scope path calls this once to create the directory without reaching the fsspec handle.

        Returns:
            The created scope path.
        """
        return self.store.ensure_path(self.path)

    @property
    def dotnet_env(self) -> dict[str, str]:
        """Build .NET command environment isolation variables."""
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "MSBUILDDISABLENODEREUSE": "1"}

    @property
    def sarif_dir(self) -> str:
        """Scope-local SARIF drop directory consumed by the CspSarifDir-conditioned analyzer ErrorLog.

        Materialized eagerly: the Roslyn ``/errorlog`` writer does not create parent directories,
        so an absent drop directory fails every build row with a path error instead of emitting SARIF.
        """
        return self.store.ensure_path(f"{self.path}/sarif")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ArtifactBackend", "ArtifactScope", "ArtifactStore", "AssaySettings", "Configuration", "LogFormat", "mtime_from_info"]
