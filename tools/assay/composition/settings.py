"""Configure Assay runtime settings, artifact storage, and .NET artifact scopes.

`AssaySettings` validates runtime parameters, `ArtifactStore` owns fsspec-backed
artifact IO, and `ArtifactScope` projects per-run or build-closure .NET output flags.
"""

import contextlib
from dataclasses import dataclass
from datetime import datetime, UTC
from enum import StrEnum
import hashlib
import os
from pathlib import Path
import socket
import sys
from typing import Annotated, Final, Literal, override, Protocol, runtime_checkable, TYPE_CHECKING
from urllib.parse import urlsplit

import fsspec  # fsspec ships no py.typed marker (declared in [[tool.mypy.overrides]])
import msgspec
from pydantic import AfterValidator, AliasChoices, BaseModel, BeforeValidator, computed_field, ConfigDict, Field, model_validator
from pydantic_settings import (  # noqa: TC002  # Runtime annotations call the Pydantic source hook.
    BaseSettings,
    NoDecode,
    PydanticBaseSettingsSource,
    SettingsConfigDict,
)
from upath import UPath
import zstandard

from tools.assay.core.model import (  # noqa: TC001  # Settings/model field hooks evaluate these annotations at runtime.
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


class PullStrategy(StrEnum):
    """How a remote artifact backend's scope tree reaches the agent landing store.

    ``TRANSFER`` downloads the tree byte-for-byte (sftp); ``SHARED`` reads the same universal paths the
    remote tool wrote (cloud object stores); ``NONE`` skips the pull (no admitted backend).
    """

    TRANSFER = "transfer"
    SHARED = "shared"
    NONE = "none"


type AnchoredRoot = Annotated[UPath, BeforeValidator(_anchor)]
type ExecTargetValue = Annotated[Local | Ssh, BeforeValidator(_parse_exec_target)]
type ExpandedKnownHosts = Annotated[str | None, BeforeValidator(_expand_or_none)]
type ExpandedPath = Annotated[UPath, BeforeValidator(lambda v: UPath(v).expanduser())]
type RunId = Annotated[str, Field(min_length=1, max_length=160, pattern=_RUN_ID_PATTERN), AfterValidator(wire_safe)]
type StoreProtocol = Annotated[str, Field(min_length=1, max_length=64, pattern=r"^[A-Za-z][A-Za-z0-9+.-]*$")]
type WireSafeText = Annotated[str, AfterValidator(wire_safe)]  # surrogateescape arrives only via env input; default_factory values are always clean


@runtime_checkable  # Backends satisfy the consumed fsspec subset structurally; no adapter layer is needed.
class ArtifactFileSystem(Protocol):
    """Structural subset of fsspec used by Assay artifact storage."""

    def makedirs(self, path: str, *, exist_ok: bool = False) -> object:
        """Create a directory path if needed."""

    def glob(self, path: str) -> list[str]:
        """Expand a backend glob."""

    def ls(self, path: str, *, detail: bool = False) -> list[str] | list[dict[str, object]]:
        """List direct backend children; detail mode returns metadata rows."""

    def find(self, path: str, *, detail: bool = False) -> list[str] | dict[str, dict[str, object]]:
        """List recursive backend children; detail mode returns metadata keyed by path."""

    def info(self, path: str) -> dict[str, object]:
        """Return backend metadata for a path."""

    def exists(self, path: str) -> bool:
        """Return whether the backend path exists."""

    def cat_file(self, path: str) -> bytes:
        """Read bytes from a backend file."""

    def rm(self, path: str, *, recursive: bool = False) -> object:
        """Remove a backend path."""

    def open(self, path: str, mode: str = "rb", *, autocommit: bool = True) -> contextlib.AbstractContextManager[object]:
        """Open a backend file.

        `autocommit=False` defers writes to the active backend transaction.
        """

    @property
    def transaction(self) -> contextlib.AbstractContextManager[object]:
        """A backend write transaction, or nullcontext when unsupported."""
        return contextlib.nullcontext()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACTS: Final[str] = ".artifacts"
_ARTIFACTS_PATH_FLAG: Final[str] = "--artifacts-path"
_ASSAY: Final[str] = "assay"
_BUILD: Final[str] = "build"
# Run history accretes one encoded Envelope (+ optional full Report) per run; the JSON-shaped payloads compress an order of
# magnitude under zstd. The store frames history-kind writes and every byte read sniffs the frame magic and inflates lazily,
# so the codec is one store-owned boundary no caller re-derives. Content size rides the frame header; plain decompress inflates.
_HISTORY_COMPRESSOR: Final[zstandard.ZstdCompressor] = zstandard.ZstdCompressor(level=10)
_HISTORY_DECOMPRESSOR: Final[zstandard.ZstdDecompressor] = zstandard.ZstdDecompressor()
# A cold build-closure scope points DOTNET_CLI_HOME at an empty tree, so the first dotnet invocation pays the full
# first-run experience (NuGet warm-up, ASP.NET dev-cert primer, tool-path init) and writes three sentinels under
# `<home>/.dotnet/<sdk>.<suffix>`. The SDK is pinned in `global.json` (rollForward disabled), so the sentinel names are
# deterministic and the scope pre-seeds all three, draining the first build's first-run cost to a no-op.
_DOTNET_FIRST_RUN_SENTINELS: Final[tuple[str, ...]] = ("dotnetFirstUseSentinel", "aspNetCertificateSentinel", "toolpath.sentinel")
_MARKER: Final[str] = "Workspace.slnx"
_RUN_ID_PATTERN: Final[str] = r"^[A-Za-z0-9_.-]+$"
# Workroot tilde is resolved to the absolute remote home once per connection (sftp.realpath) so the SFTP push and the exec `cd` agree.
_REMOTE_WORKROOT_DEFAULT: Final[str] = "~/.assay-work"
# Injected remote PATH: a Linux toolchain prefix the agent exports for both the toolchain probe and the exec, so a non-login
# PATH still reaches uv (~/.local/bin) and dotnet (/usr/local/dotnet). The agent's own macOS PATH never crosses to a Linux host.
_REMOTE_PATH_PREFIX: Final[tuple[str, ...]] = (
    "~/.local/bin",
    "/usr/local/dotnet",
    "/usr/local/sbin",
    "/usr/local/bin",
    "/usr/sbin",
    "/usr/bin",
    "/sbin",
    "/bin",
)
# Backend admission is data, not a dispatch chain: one row per protocol carries reachability, admission, and the pull strategy the
# Ssh case selects. sftp downloads bytes (TRANSFER); the cloud rows are admitted shared object stores (s3fs/gcsfs) the remote tool
# writes and the agent reads at the same universal paths (SHARED, zero byte transfer), so the backend never re-derives provider shapes.
_BACKEND_CAPABILITY: Final[dict[str, tuple[bool, bool, PullStrategy]]] = {
    "file": (False, True, PullStrategy.NONE),
    "sftp": (True, True, PullStrategy.TRANSFER),
    "s3": (True, True, PullStrategy.SHARED),
    "gs": (True, True, PullStrategy.SHARED),
    "gcs": (True, True, PullStrategy.SHARED),
}
_REMOTE_ENV_NAMES: Final[frozenset[str]] = frozenset((
    "ASSAY_AGENT_TASK_ID",
    "ASSAY_ARTIFACT_BACKEND__PROTOCOL",  # remote executor writes scope artifacts to the same logical backend the agent reads
    "ASSAY_ARTIFACT_BACKEND__ROOT",  # cloud credentials ride the executor's ambient env, never this allowlist
    "ASSAY_ARTIFACT_RETENTION",
    "ASSAY_EXEC_TARGET",
    "ASSAY_RUN_ID",
    "LANG",
    "LC_ALL",
    "OTEL_EXPORTER_OTLP_ENDPOINT",
    "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT",
    "OTEL_SERVICE_NAME",
    "RHINO_WIP_APP_PATH",
))
# W3C Trace Context keys are lowercase; propagate.inject emits them verbatim.
_TRACE_CONTEXT_ENV: Final[frozenset[str]] = frozenset(("traceparent", "tracestate", "baggage"))
_PYTHON_TOOL_ENV_REL: Final[dict[str, str]] = {
    "UV_CACHE_DIR": ".cache/uv",
    "HYPOTHESIS_STORAGE_DIRECTORY": ".cache/hypothesis",
    "PYTEST_CACHE_DIR": ".cache/pytest",
    "RUFF_CACHE_DIR": ".cache/ruff",
    "MYPY_CACHE_DIR": ".cache/mypy",
    "COVERAGE_FILE": ".cache/coverage/.coverage",
}
# Single owner of every Python heavy-lane artifact-output root; catalog rows, the test rail, and runtime envs route here
# instead of re-spelling the literal. Of the three, only benchmark autosaves accumulate; coverage files and the mutmut
# work tree self-overwrite per run.
PY_ARTIFACT_ROOTS: Final[dict[str, str]] = {
    "coverage": f"{_ARTIFACTS}/python/coverage",
    "benchmarks": f"{_ARTIFACTS}/python/benchmarks",
    "mutmut": f"{_ARTIFACTS}/python/mutmut/work",
}
PY_COVERAGE_FILES: Final[dict[str, str]] = {fmt: f"{PY_ARTIFACT_ROOTS['coverage']}/coverage.{fmt}" for fmt in ("json", "xml", "lcov")}
# Stryker writes a sandbox (`.stryker-tmp`, cwd-relative) and reports; the staged work root keeps both under `.artifacts`.
CS_ARTIFACT_ROOTS: Final[dict[str, str]] = {"stryker": f"{_ARTIFACTS}/csharp/stryker/work"}
# One shared dotnet build closure for the static and test rails: per-claim or per-sha trees each hold a full
# solution build (~16GB), so any second key doubles the disk for zero isolation — the exclusive build lease
# already serializes writers, and the artifacts layout separates projects and pivots inside one tree.
DOTNET_BUILD_CLOSURE: Final[str] = "dotnet"

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


def size_from_info(info: dict[str, object], *, fallback: int = 0) -> int:
    """Coerce fsspec size metadata to an int for artifact byte counts.

    Returns:
        File size as an int, or fallback when the backend omits or non-ints the key.
    """
    match info.get("size", fallback):
        case int() as value:
            return value
        case _:
            return fallback


def _anchor(value: str | UPath) -> UPath:
    # Ingress anchoring gives consumers an absolute workspace root instead of a caller cwd.
    cursor = UPath(value).expanduser().resolve()
    return next((p for p in (cursor, *cursor.parents) if (p / _MARKER).is_file()), cursor)


def _default_cpu_count() -> int:
    # os.process_cpu_count honors cgroup/affinity limits; engine._governed reads this via AssaySettings.cpu_count.
    return min(256, max(1, os.process_cpu_count() or 8))


def _host_token() -> str:
    # Stable per-host token: the blake2b(hostname) digest embedded in every run id keeps run dirs disjoint per host on a shared root.
    return hashlib.blake2b(socket.gethostname().encode(), digest_size=4).hexdigest()


def _host_unique_run_id() -> str:
    # timestamp-token-pid: the embedded host token (above) carves a per-host run-id namespace a remote prune can sweep without cross-deleting.
    return f"{datetime.now(tz=UTC):%Y-%m-%dT%H-%M-%S.%f}-{_host_token()}-{os.getpid()}"


def run_id_host_token(run_id: str) -> str:
    """Extract the embedded host token from a ``<timestamp>-<token>-<pid>`` run id.

    The token is the blake2b(hostname) digest ``_host_unique_run_id`` embeds between the timestamp and the pid, so a
    remote prune over a shared workroot can select exactly the runs minted by one host without touching another host's runs.

    Returns:
        The host token segment, or ``""`` when the run id does not carry a ``-<token>-<pid>`` tail.
    """
    match run_id.rsplit("-", maxsplit=2):
        case [_, token, pid] if token and pid:
            return token
        case _:
            return ""


def _parse_exec_target(value: object) -> Local | Ssh:
    # One admission point folds the raw ssh:// URL or a round-tripped dump into the modal value object so the discriminant is the case, never a flag.
    match value:
        case Local() | Ssh():
            return value
        case "" | None:
            return Local()
        case str() as url:
            return Ssh.parse(url)
        case {"host": str() as host} as fields if host:
            return Ssh.model_validate(fields)
        case {}:
            # A hostless or empty round-tripped dump is the local case; bare `{}` matches every mapping, so this arm stays below the host arm.
            return Local()
        case _:
            raise ValueError(f"exec_target must be '' (local), an 'ssh://[user@]host[:port]' URL, or an ExecTarget value: {value!r}")


def _expand_or_none(value: str | UPath | None) -> str | None:
    match value:
        case None | "":
            return None
        case _:
            return str(UPath(value).expanduser())


def resolve_tilde(path: str, home: str) -> str:
    """Rewrite a leading ``~`` in a remote path against an absolute remote home, leaving an absolute path unchanged.

    Returns:
        The path with ``~``/``~/`` rebound to ``home``, or the original path when no leading tilde is present.
    """
    base = home.rstrip("/")
    match path:
        case "~":
            return base
        case _ if path.startswith("~/"):
            return f"{base}/{path[2:]}"
        case _:
            return path


def remote_path(home: str) -> str:
    """Project the injected remote ``PATH`` with the toolchain prefix's leading ``~`` resolved to the absolute remote home.

    Returns:
        A ``:``-joined Linux PATH covering ``~/.local/bin`` (uv) and ``/usr/local/dotnet`` (dotnet) for probe and exec parity.
    """
    return ":".join(resolve_tilde(part, home) for part in _REMOTE_PATH_PREFIX)


def _root_parts(root: str) -> tuple[str, ...]:
    return tuple(part for part in root.split("/") if part)


def _safe_segment(part: str | UPath) -> str:
    text = str(part).replace("\\", "/")
    pieces = tuple(p for p in text.split("/") if p)
    # Single-piece identity rejects absolute, trailing-slash, empty, dot, parent, and NUL segments together.
    match (any(p in {".", ".."} for p in pieces), len(pieces) == 1 and text == pieces[0], "\x00" in text):
        case (False, True, False):
            return text
        case _:
            raise ValueError(f"unsafe artifact path segment: {text!r}")


def backend_capability(protocol: str) -> tuple[bool, bool, PullStrategy]:
    """Project a backend protocol's (reachable, admitted, pull-strategy) capability row.

    Returns:
        The policy row for ``protocol``; an unknown protocol is unreachable, not admitted, and pulls nothing.
    """
    return _BACKEND_CAPABILITY.get(protocol, (False, False, PullStrategy.NONE))


def unframe(payload: bytes) -> bytes:
    """Inflate a zstd-framed payload, passing an unframed payload through unchanged.

    The frame-magic prefix discriminates history frames from plain artifacts, so a single store-owned read boundary
    serves both compressed history and uncompressed SARIF/coverage payloads with no per-consumer codec knowledge.

    Returns:
        The inflated bytes for a zstd frame, or the original bytes when the magic prefix is absent.
    """
    return _HISTORY_DECOMPRESSOR.decompress(payload) if payload[:4] == zstandard.FRAME_HEADER else payload


# --- [MODELS] ---------------------------------------------------------------------------


class ArtifactBackend(BaseModel):
    """Validated artifact backend selected by settings.

    ``storage_options`` is the typed owner of every fsspec credential/endpoint knob a non-file backend needs —
    cloud key/secret/token/endpoint_url for s3/gs/gcs, host/port/username/client_keys for sftp — passed verbatim to
    ``fsspec.filesystem(protocol, **storage_options)``; the file backend needs none and defaults to empty.
    """

    model_config = ConfigDict(frozen=True, extra="forbid")

    protocol: StoreProtocol = "file"
    root: str = ""
    storage_options: dict[str, str | int | bool] = Field(default_factory=dict)

    @model_validator(mode="after")
    def _bounded_non_file_root(self) -> ArtifactBackend:  # noqa: N804  # Pydantic v2 mode="after" passes the model instance, not cls.
        # Non-file backend singletons need a non-empty root to avoid cross-store collisions; file backends ignore storage_options.
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
        # _bounded_non_file_root guarantees non-file roots; protocol-first matching keeps the partition total.
        match self.protocol:
            case "file" if not self.root:
                return str(settings.store_root)
            case "file":
                candidate = UPath(self.root).expanduser()
                return str(candidate if candidate.is_absolute() else settings.root / candidate)
            case _:
                return self.root.rstrip("/")

    @property
    def capability(self) -> tuple[bool, bool, PullStrategy]:
        """Project this backend's (reachable, admitted, pull-strategy) capability row.

        Returns:
            Capability row from the policy table; an unknown protocol is unreachable, not admitted, no pull.
        """
        return backend_capability(self.protocol)


class Local(BaseModel):
    """Local execution target: the agent runs the check in-process on this host."""

    model_config = ConfigDict(frozen=True, extra="forbid")

    def __bool__(self) -> bool:
        """Return False: Local is the falsy identity of the local/remote discriminant engine sites read."""
        return False


class Ssh(BaseModel):
    """Remote SSH execution target owning connect kwargs, remote workroot, env-forwarding, and the pull strategy."""

    model_config = ConfigDict(frozen=True, extra="forbid")

    host: str = Field(min_length=1)
    port: Annotated[int, Field(ge=0, le=65535)] | None = None
    user: str | None = None
    known_hosts: ExpandedKnownHosts = Field(default=str(UPath("~/.ssh/known_hosts").expanduser()))
    workroot: str = _REMOTE_WORKROOT_DEFAULT

    @classmethod
    def parse(cls, url: str) -> Ssh:
        """Admit an ``ssh://[user@]host[:port]`` URL into the value object.

        Returns:
            Validated Ssh target with the URL's host, port, and user.

        Raises:
            ValueError: When the scheme is not ssh or a path/query/fragment is present.
        """
        parts = urlsplit(url)
        match (parts.scheme, parts.hostname, parts.path, parts.query, parts.fragment):
            case ("ssh", str() as host, "" | "/", "", "") if host:
                try:
                    port = parts.port
                except ValueError as exc:
                    raise ValueError(f"exec_target has a non-numeric ssh port: {url!r}") from exc
                return cls(host=host, port=port, user=parts.username)
            case _:
                raise ValueError(f"exec_target must be '' (local) or 'ssh://[user@]host[:port]' without path/query/fragment: {url!r}")

    def __bool__(self) -> bool:
        """Return True: Ssh is the truthy remote case of the local/remote discriminant engine sites read."""
        return True

    @property
    def url(self) -> str:
        """Render the canonical ``ssh://[user@]host[:port]`` URL for logging, OTel, and the lease owner.

        Returns:
            The ssh URL string projection of this target.
        """
        authority = f"{self.user}@{self.host}" if self.user else self.host
        return f"ssh://{authority}:{self.port}" if self.port is not None else f"ssh://{authority}"

    @property
    def connect_kwargs(self) -> dict[str, object]:
        """Project asyncssh connect kwargs the agent owns (host/port/username/known_hosts).

        ``port`` defaults to 22 when the URL omits it: asyncssh binds ``port=None`` as port 0 and raises ``EADDRNOTAVAIL``,
        so the value object resolves the default here while ``url`` keeps the canonical no-``:22`` rendering.

        Returns:
            Mapping passed to ``asyncssh.connect``; timeouts/keepalive are engine-owned constants.
        """
        return {"host": self.host, "port": self.port or 22, "username": self.user, "known_hosts": self.known_hosts}

    def resolve_home(self, home: str) -> Ssh:
        """Rebind this target's workroot against an absolute remote home, replacing a leading ``~``.

        The agent resolves the remote ``~`` once per connection (``sftp.realpath('.')``) so the SFTP push and the login-shell
        ``cd`` land in the same absolute dir; an already-absolute workroot is returned unchanged.

        Returns:
            This target with ``workroot`` rewritten to an absolute path, or unchanged when no leading ``~`` is present.
        """
        resolved = resolve_tilde(self.workroot, home)
        return self if resolved == self.workroot else self.model_copy(update={"workroot": resolved})

    def remote_workroot(self, run_id: str) -> str:
        """Derive the per-run remote working tree root ``<workroot>/<run_id>``.

        The workroot is absolute once ``resolve_home`` rebinds a leading ``~`` to the connection's resolved home.

        Returns:
            Remote run-dir path the pushed tree lands under and the remote cwd resolves to.
        """
        return f"{self.workroot.rstrip('/')}/{_safe_segment(run_id)}"

    def offload(self, run_id: str, configured: ArtifactBackend) -> Offload:
        """Derive the offload binding for this run: target plus the artifact backend co-located under the run dir.

        A SHARED-strategy cloud backend (s3/gs/gcs) is pinned at ``<configured.root>/<run_id>/.artifacts/assay`` so the
        remote tool writes and the agent reads the same universal store with zero byte transfer; every other case derives
        the per-run sftp store under the remote workroot. The backend is host-derived, never a separate knob.

        Returns:
            Offload value object whose backend is derived from this host and the configured store.
        """
        match configured.capability[2]:
            case PullStrategy.SHARED:
                root = f"{configured.root.rstrip('/')}/{_safe_segment(run_id)}/{_ARTIFACTS}/{_ASSAY}"
                backend = ArtifactBackend(protocol=configured.protocol, root=root)
            case _:
                backend = ArtifactBackend(protocol="sftp", root=f"{self.remote_workroot(run_id)}/{_ARTIFACTS}/{_ASSAY}")
        return Offload(target=self, backend=backend)


class Offload(BaseModel):
    """Per-run offload binding: an Ssh target with the artifact backend derived from its host.

    One owner makes host/backend inconsistency unrepresentable — the backend is never a separate knob, it is the sftp
    store pinned under the target's remote run dir, so a remote target always reads through a remotely-reachable store.
    """

    model_config = ConfigDict(frozen=True, extra="forbid")

    target: Ssh
    backend: ArtifactBackend

    @property
    def pull_strategy(self) -> PullStrategy:
        """Project the pull strategy from the derived backend's capability row.

        Returns:
            How the remote scope tree reaches the agent landing store.
        """
        return self.backend.capability[2]


class AssaySettings(BaseSettings):  # noqa: PLR0904  # AssaySettings is the central runtime-settings owner; its projections compose one concern.
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
    # Bounded stderr diagnostic-preview window retained on the receipt; the full stdout payload spills to the PROCESS artifact instead.
    stream_tail_bytes: Annotated[int, Field(ge=512)] = 4096
    stream_chunk_bytes: Annotated[int, Field(ge=4096)] = 65536
    # In-memory stdout-capture ceiling: at or below it the full payload rides the receipt inline; above it capture spills to the PROCESS artifact.
    capture_spill_bytes: Annotated[int, Field(ge=65536)] = 1_048_576
    lease_drift_tolerance: Annotated[float, Field(gt=0)] = 1.0
    # Machine-wide dotnet admission-slot lock root: per-user, machine-stable, local-fs (flock-hostable), aligned with the bridge ~/.rasm home.
    machine_lock_root: Path = Field(default_factory=lambda: Path.home() / ".rasm" / "locks")
    artifact_retention: Annotated[int, Field(ge=1, le=10000)] = 50
    build_scope_retention: Annotated[int, Field(ge=1, le=1000)] = 24
    # Remote push/pull budget: the floor covers the pull leg and small manifests; the per-file term scales the push ceiling so a
    # large git-tracked tree (1000+ files, latency-bound at one round-trip each) does not degrade mid-push on a real WAN link.
    transfer_budget_s: Annotated[float, Field(gt=0)] = 120.0
    transfer_per_file_s: Annotated[float, Field(gt=0)] = 2.5
    # SFTP push throttle: a throttled or low-MaxSessions server caps how many directory put coroutines run concurrently
    # over the one channel and how many open/write/close requests each put pipelines. Tunable down for restrictive hosts.
    sftp_push_concurrency: Annotated[int, Field(ge=1, le=256)] = 16
    sftp_max_requests: Annotated[int, Field(ge=1, le=1024)] = 64
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
    mutation_python: str = "3.15"
    log_format: LogFormat = Field(default_factory=lambda: LogFormat.HUMAN if sys.stderr.isatty() else LogFormat.CI)
    log_level: Literal["debug", "info", "warning", "error", "critical"] = "info"
    run_id: RunId = Field(default_factory=_host_unique_run_id)
    agent_task_id: WireSafeText = ""
    # NoDecode keeps the raw env string (e.g. `ssh://host`) unparsed: the union type would otherwise drive pydantic-settings to
    # `json.loads` the env value before the BeforeValidator, raising SettingsError on a bare `ssh://` URL.
    exec_target: Annotated[ExecTargetValue, NoDecode] = Field(
        default_factory=Local, description="execution target; '' = local, ssh://[user@]host[:port] = remote"
    )
    exec_known_hosts: ExpandedKnownHosts = Field(
        default=str(UPath("~/.ssh/known_hosts").expanduser()),
        description="asyncssh known_hosts path for ssh:// exec_target; empty disables the host-key check",
    )
    exec_workroot: str = Field(
        default=_REMOTE_WORKROOT_DEFAULT,
        description="remote working-tree root for ssh:// exec_target; a leading ~ resolves to the connection's remote home",
    )
    otel_endpoint: str = Field(
        default="", validation_alias=AliasChoices("ASSAY_OTEL_ENDPOINT", "OTEL_EXPORTER_OTLP_ENDPOINT", "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT")
    )

    @model_validator(mode="before")
    @classmethod
    def _fold_remote_connect_inputs(cls, data: object) -> object:
        # known_hosts/workroot arrive as separate env vars; fold them into the Ssh case at admission so the value object owns all connect facts.
        match data:
            case dict() as raw:
                match _parse_exec_target(raw.get("exec_target", "")):
                    case Ssh() as ssh:
                        overrides = {
                            field: value
                            for field, key in (("known_hosts", "exec_known_hosts"), ("workroot", "exec_workroot"))
                            for value in (raw.get(key),)
                            if key in raw
                        }
                        return {**raw, "exec_target": ssh.model_copy(update=overrides) if overrides else ssh}
                    case local:
                        return {**raw, "exec_target": local}
            case _:
                return data

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

    @computed_field  # type: ignore[prop-decorator]  # mypy lacks computed_field+property support.
    @property
    def solution(self) -> UPath:
        """Resolve Workspace.slnx under the anchored root."""
        return self.root / _MARKER

    @property
    def dotnet_sdk_version(self) -> str:
        """Project the pinned .NET SDK version from ``global.json`` under the anchored root.

        The version (``rollForward: disable``) names the deterministic first-run sentinel band a cold build scope pre-seeds.

        Returns:
            The pinned ``sdk.version``, or ``""`` when ``global.json`` is absent or omits it.
        """
        try:
            decoded = msgspec.json.decode((self.root / "global.json").read_bytes())
        except OSError, msgspec.MsgspecError:
            return ""
        match decoded:
            case {"sdk": {"version": str() as version}}:
                return version
            case _:
                return ""

    @computed_field  # type: ignore[prop-decorator]  # mypy lacks computed_field+property support.
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
    def host_run_token(self) -> str:
        """Project this run's host token: the per-host namespace key a remote prune sweeps without cross-deleting another host's runs.

        Returns:
            The blake2b(hostname) token embedded in this run id, or ``""`` for a custom run id that omits the host tail.
        """
        return run_id_host_token(self.run_id)

    @property
    def offload(self) -> Offload | None:
        """Derive the per-run offload binding when the target is remote.

        Returns:
            Offload value object pinning the sftp backend under this run's remote dir, or None for local execution.
        """
        match self.exec_target:
            case Ssh() as ssh:
                return ssh.offload(self.run_id, self.artifact_backend)
            case _:
                return None

    @property
    def store_root(self) -> UPath:
        """Resolve the .artifacts/assay root path."""
        return self.root / _ARTIFACTS / _ASSAY

    @property
    def local_root(self) -> UPath:
        """The local workspace root usable as a subprocess cwd.

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

        The no-override common path reuses the settings-validated ``artifact_backend`` verbatim (no per-call
        ``model_dump``/``model_validate`` round trip); a protocol/root override revalidates only the patched backend.
        The resolved backend's typed ``storage_options`` is the persistent fsspec credential/endpoint config; ``**opts``
        layers per-call overrides on top, both reaching ``fsspec.filesystem``.

        Returns:
            Artifact store bound to the requested backend.
        """
        override = {k: v for k, v in {"protocol": protocol, "root": root}.items() if v is not None}
        backend = self.artifact_backend if not override else ArtifactBackend.model_validate({**self.artifact_backend.model_dump(), **override})
        return ArtifactStore(fs=fsspec.filesystem(backend.protocol, **{**backend.storage_options, **opts}), root=backend.target(self))

    def with_configuration(self, configuration: Configuration) -> AssaySettings:
        """Return these settings rebound to a different build configuration."""
        return self.model_copy(update={"configuration": configuration})

    def artifact(self, kind: ArtifactKind, *parts: str | UPath) -> UPath:
        """Project an artifact path from a kind and path parts.

        Returns:
            Store-rooted artifact path.
        """
        return self.store_root.joinpath(kind.value, *(_safe_segment(p) for p in parts))

    def remote_env(self, env: dict[str, str], *, home: str, forward: frozenset[str] = frozenset()) -> dict[str, str]:
        """Project the environment subset safe to export through an SSH command string.

        `forward` names row-owned `Tool.env` keys that may cross SSH alongside the
        ambient allowlist. `home` is the connection-resolved absolute remote home that
        anchors the injected toolchain ``PATH``.

        Returns:
            Environment variables allowed to cross the SSH execution boundary, with an injected Linux ``PATH``.
        """
        # RUN_ID, the injected toolchain PATH, and the offload-derived backend live in validated settings, so inject them before
        # allowlist projection. The agent's macOS PATH never crosses; the remote PATH is a host-side Linux toolchain prefix.
        # The remote executor writes scope artifacts to the backend the agent pulls from: the per-run sftp root, never the local file root.
        safe_names = frozenset(("PATH", *self.python_tool_env, *_REMOTE_ENV_NAMES, *_TRACE_CONTEXT_ENV, *forward))
        store = self.offload.backend if self.offload is not None else self.artifact_backend
        backend = {"ASSAY_ARTIFACT_BACKEND__PROTOCOL": store.protocol, "ASSAY_ARTIFACT_BACKEND__ROOT": store.root}
        source = {"ASSAY_RUN_ID": self.run_id, **backend, **env, "PATH": remote_path(home)}
        return {k: v for k, v in source.items() if k in safe_names and v}


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)  # noqa: PLR0904  # ArtifactStore is the storage boundary.
class ArtifactStore:
    """Validated artifact filesystem boundary."""

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
        return size_from_info(self.info_path(path), fallback=fallback)

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
        """Read bytes from a validated store-relative path, inflating a zstd-framed history payload.

        Returns:
            File payload bytes, inflated when the file carries the zstd frame magic.
        """
        return unframe(self.fs.cat_file(self.path(*parts)))

    def read_text(self, *parts: str | UPath, encoding: str = "utf-8", errors: str = "replace") -> str:
        """Read text from a validated store-relative path.

        Returns:
            Decoded text payload.
        """
        return self.read_bytes(*parts).decode(encoding, errors=errors)

    def read_path(self, path: str | UPath) -> bytes:
        """Read bytes from a validated backend path returned by the store, inflating a zstd-framed history payload.

        Returns:
            File payload bytes, inflated when the file carries the zstd frame magic.
        """
        return unframe(self.fs.cat_file(self.backend_path(path)))

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
            # autocommit=False only materializes under a backend transaction; standalone writes commit directly.
            with self.fs.open(path, "wb", autocommit=not transaction) as fh:
                # Protocol returns object so backends can expose their native fsspec file handle.
                fh.write(payload)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return path

    def write_bytes(self, payload: bytes, *parts: str | UPath, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated store-relative path.

        ``create=True`` propagates FileExistsError from the backend when the target already exists.

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
        """Write bytes to a validated backend path emitted by this store.

        ``create=True`` propagates FileExistsError from the backend when the target already exists.

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
        """Persist one encoded Envelope in run history under a zstd frame.

        Returns:
            Backend path written.
        """
        return self.write_bytes(_HISTORY_COMPRESSOR.compress(payload), ArtifactKind.HISTORY.value, run_id, "envelope.json")

    def write_full_report(self, run_id: str, name: str, report: Report) -> tuple[str, int]:
        """Persist an unclipped Report under a zstd frame before previewing or clipping.

        Returns:
            Backend path and uncompressed payload byte count.
        """
        payload = wire_encode(report)
        return self.write_bytes(_HISTORY_COMPRESSOR.compress(payload), ArtifactKind.HISTORY.value, run_id, name), len(payload)

    def _sorted_run_ids(self, root: str) -> tuple[str, ...]:
        # Omitted mtimes fold to 0.0, leaving run_id as the chronological tiebreaker.
        detailed = self.walk(root, detail=True)
        # isinstance narrows walk's union to detail rows.
        detail_rows = tuple(
            (row[0].rstrip("/").rsplit("/", 1)[-1], row[1]) for row in detailed if isinstance(row, tuple) and isinstance(row[1], dict)
        )
        rows = detail_rows or tuple((path.rstrip("/").rsplit("/", 1)[-1], {}) for path in self.glob(f"{root}/*"))
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

        Mirrors retain_history over a claim root to bound per-rail scope accumulation.
        """
        self._prune(claim.value, keep)

    def retain_builds(self, keep: int) -> None:
        """Prune old build-closure scope dirs by backend metadata age, keeping the newest keep closures.

        Covers stable `build/<closure>/<config>` dirs outside history and per-claim scope roots.
        """
        self._prune(_BUILD, keep)

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
        # Lazy open avoids empty run directories when neither rails nor dotnet write into the scope.
        path = store.path(claim.value, settings.run_id)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path))

    @classmethod
    def build(cls, settings: AssaySettings, closure: str, configuration: Configuration | str | None = None) -> ArtifactScope:
        """Open a stable build-closure artifact scope, pre-seeding the cold ``DOTNET_CLI_HOME`` first-run sentinels.

        Returns:
            Artifact scope rooted under the build closure id.
        """
        store = settings.store()
        config = configuration.value if isinstance(configuration, Configuration) else configuration or settings.configuration.value
        path = store.ensure(_BUILD, closure, config)
        scope = cls(store, path, (_ARTIFACTS_PATH_FLAG, path))
        scope._preseed_dotnet_first_run(settings.dotnet_sdk_version)
        return scope

    def _preseed_dotnet_first_run(self, sdk_version: str) -> None:
        # A first invocation against a fresh DOTNET_CLI_HOME runs the first-run experience and writes these sentinels;
        # writing them up-front (idempotently, only when the SDK band is known) drains that cost from the first build.
        marker = f"{self.path}/dotnet-cli/.dotnet/{sdk_version}.{_DOTNET_FIRST_RUN_SENTINELS[0]}"
        if not sdk_version or self.store.exists_path(marker):
            return
        for suffix in _DOTNET_FIRST_RUN_SENTINELS:
            self.store.write_bytes_path(b"", f"{self.path}/dotnet-cli/.dotnet/{sdk_version}.{suffix}")

    def ensure(self) -> str:
        """Materialize this scope's directory through the store boundary.

        Returns:
            The created scope path.
        """
        return self.store.ensure_path(self.path)

    @property
    def dotnet_env(self) -> dict[str, str]:
        """Build .NET command environment isolation variables.

        VBCSCompiler shared compilation stays on; the per-closure ``--artifacts-path`` is the isolation boundary because
        the build-server pipe identity is per-user and per-SDK, so concurrent closures share servers without cross-talk.
        ``DOTNET_NOLOGO``/``DOTNET_CLI_TELEMETRY_OPTOUT`` pair with the pre-seeded sentinels to silence the first-run primer.
        """
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "DOTNET_NOLOGO": "1", "DOTNET_CLI_TELEMETRY_OPTOUT": "1"}

    @property
    def sarif_dir(self) -> str:
        """Scope-local SARIF drop directory consumed by the CspSarifDir-conditioned analyzer ErrorLog.

        Materialized eagerly because Roslyn `/errorlog` does not create parent directories.
        """
        return self.store.ensure_path(f"{self.path}/sarif")


# --- [OPERATIONS] -----------------------------------------------------------------------


def prune_python_artifacts(root: Path, keep: int) -> None:
    """Bound Python heavy-lane artifact accumulation under the repo root, keeping the newest ``keep`` benchmark autosaves.

    Only the benchmark autosave tree accumulates across runs; the coverage files and the mutmut work tree self-overwrite,
    so the bound applies solely to ``PY_ARTIFACT_ROOTS['benchmarks']``.

    Args:
        root: Local repository root that anchors the relative heavy-lane roots.
        keep: Maximum number of benchmark autosave files to retain, oldest pruned first.
    """
    benchmarks = root / PY_ARTIFACT_ROOTS["benchmarks"]
    if not benchmarks.is_dir():
        return
    files = sorted((p for p in benchmarks.rglob("*") if p.is_file()), key=lambda p: p.stat().st_mtime)
    for stale in files[: max(0, len(files) - keep)]:
        try:
            stale.unlink()
        except OSError:
            _ = stale.exists()  # best-effort prune; a vanished autosave is already gone


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ArtifactBackend",
    "ArtifactScope",
    "ArtifactStore",
    "AssaySettings",
    "CS_ARTIFACT_ROOTS",
    "Configuration",
    "Local",
    "LogFormat",
    "Offload",
    "PY_ARTIFACT_ROOTS",
    "PY_COVERAGE_FILES",
    "PullStrategy",
    "Ssh",
    "backend_capability",
    "mtime_from_info",
    "prune_python_artifacts",
    "remote_path",
    "resolve_tilde",
    "run_id_host_token",
    "size_from_info",
    "unframe",
]
