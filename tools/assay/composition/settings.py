"""Configure Assay runtime settings and execution targets.

`AssaySettings` validates runtime parameters; `Local`/`Ssh` are the modal execution-target value objects and
`Offload` binds a remote target to its derived artifact backend. Artifact storage lives in `tools.assay.composition.store`.
"""

from datetime import datetime, UTC
from enum import StrEnum
import hashlib
import os
from pathlib import Path
import socket
import sys
from typing import Annotated, Final, Literal, override
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

from tools.assay.composition.store import ArtifactStore, safe_segment
from tools.assay.core.model import ArtifactKind, wire_safe  # noqa: TC001  # Settings/model field hooks evaluate these annotations at runtime.


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

# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACTS: Final[str] = ".artifacts"
_ASSAY: Final[str] = "assay"
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

# --- [BOUNDARIES] -----------------------------------------------------------------------


def _anchor(value: str | UPath) -> UPath:
    # Ingress anchoring gives consumers an absolute workspace root instead of a caller cwd.
    cursor = UPath(value).expanduser().resolve()
    return next((p for p in (cursor, *cursor.parents) if (p / _MARKER).is_file()), cursor)


def _default_cpu_count() -> int:
    # os.process_cpu_count honors cgroup/affinity limits; the governed-concurrency fold reads this via AssaySettings.cpu_count.
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


def backend_capability(protocol: str) -> tuple[bool, bool, PullStrategy]:
    """Project a backend protocol's (reachable, admitted, pull-strategy) capability row.

    Returns:
        The policy row for ``protocol``; an unknown protocol is unreachable, not admitted, and pulls nothing.
    """
    return _BACKEND_CAPABILITY.get(protocol, (False, False, PullStrategy.NONE))


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
        return f"{self.workroot.rstrip('/')}/{safe_segment(run_id)}"

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
                root = f"{configured.root.rstrip('/')}/{safe_segment(run_id)}/{_ARTIFACTS}/{_ASSAY}"
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
        return self.store_root.joinpath(kind.value, *(safe_segment(p) for p in parts))

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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ArtifactBackend",
    "AssaySettings",
    "Configuration",
    "Local",
    "LogFormat",
    "Offload",
    "PullStrategy",
    "Ssh",
    "backend_capability",
    "remote_path",
    "resolve_tilde",
    "run_id_host_token",
]
