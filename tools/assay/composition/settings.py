"""Config surface: ``AssaySettings``, the ``artifact`` path fold, the ``ArtifactStore``, and the dotnet ``ArtifactScope``.

Env/flags validate through ``pydantic`` once at ``__init__``; ``frozen=True`` keeps the instance
hashable and STM-safe so ``model_copy(update=...)`` threads a fresh config through ``fan_out``.
"""

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

    from tools.assay.core.model import ArtifactKind, Claim  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------


class Configuration(StrEnum):
    """MSBuild ``-c`` axis: the member IS the verbatim configuration value (Cyclopts token, wire value, discriminant)."""

    DEBUG = "Debug"
    RELEASE = "Release"


class LogFormat(StrEnum):
    """structlog renderer axis: console (``HUMAN``) or JSON (``CI``); default derives from ``sys.stderr.isatty()``."""

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
    """The sole ``pydantic-settings`` surface: env-only host/infra scalars, validated once.

    ``settings_customise_sources`` collapses the ingress to ``(init_settings, env_settings)`` (init
    kwargs beat ``ASSAY_*`` env; CLI/dotenv/secrets dropped — Cyclopts is the CLI boundary). ``frozen``
    makes the instance hashable and STM-safe under ``fan_out``.
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
        """Precedence ``init > ASSAY_* env``; drop CLI, dotenv, and secrets entirely (validate once)."""
        return (init_settings, env_settings)

    @computed_field  # type: ignore[prop-decorator]  # pydantic computed_field over a property
    @property
    def solution(self) -> UPath:
        """The ``Workspace.slnx`` the SOLUTION arm resolves against.

        Construction NEVER hard-fails on a missing ``Workspace.slnx`` (``_anchor`` falls back to ``cwd``)
        per the point-and-go contract; an absent solution is a per-operation C# concern that exits into a
        clean ``Completed(FAILED)``/Fault, never a settings-construction crash.
        """
        return self.root / _MARKER

    @computed_field  # type: ignore[prop-decorator]  # pydantic computed_field over a property
    @property
    def agent_context(self) -> dict[str, str]:
        """The ``{run.id, agent.task.id}`` fleet-correlation tags, folded from the canonical fields once at the boundary (never ``os.environ``)."""
        return {"run.id": self.run_id, "agent.task.id": self.agent_task_id}

    @property
    def store_root(self) -> UPath:
        """The ``.artifacts/assay`` tree root; every ``artifact`` path and local ``store`` share it."""
        return self.root / _ARTIFACTS / _ASSAY

    def store(self, *, protocol: str = "file", **opts: object) -> ArtifactStore:
        """The backend-agnostic ``.artifacts`` I/O surface: ``file`` local, ``memory`` for tests.

        ``protocol="memory"`` roots at the ``run_id``-keyed relative path so concurrent suites sharing the
        class-global ``MemoryFileSystem`` partition the dict; ``protocol="file"`` roots at ``store_root``.
        """
        match protocol:
            case "file":
                target = str(self.store_root)
            case _:
                target = f"{_ARTIFACTS}/{_ASSAY}/{self.run_id}"
        return ArtifactStore(fs=fsspec.filesystem(protocol, **opts), root=target)

    def artifact(self, kind: ArtifactKind, *parts: str | UPath) -> UPath:
        """The sole path projector: fold ``kind`` plus ``parts`` onto ``store_root``.

        The path namespace is exactly the ``ArtifactKind`` member set; a closure build tree lives in the
        ``SCOPE`` namespace (``artifact(ArtifactKind.SCOPE, "build", closure)``) — there is no ``BUILD`` kind.
        """
        return self.store_root.joinpath(kind.value, *(str(p) for p in parts))


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class ArtifactStore:
    """The ``fsspec`` ``.artifacts`` backend (the sole writer for every ``.artifacts/`` tree): a ``Store`` is *where* bytes go.

    ``ensure`` is the only directory creator — explicit ``makedirs`` is required because
    ``LocalFileSystem.auto_mkdir=False`` by default, so lazy ``open(mode="w")`` parent creation cannot
    be relied upon.
    """

    fs: AbstractFileSystem
    root: str

    def ensure(self, *parts: str) -> str:
        """Create and return the ``root``-relative directory path; the sole ``makedirs`` site."""
        path = "/".join((self.root, *parts))
        self.fs.makedirs(path, exist_ok=True)
        return path

    def glob(self, pattern: str) -> tuple[str, ...]:
        """Expand a glob under ``root`` to a tuple — the immutable shape rails fold over."""
        return tuple(self.fs.glob(f"{self.root}/{pattern}"))

    def exists(self, *parts: str) -> bool:
        """Report whether the ``root``-relative path exists; backs lease/staleness probes."""
        return bool(self.fs.exists("/".join((self.root, *parts))))


@dataclass(frozen=True, slots=True)
class ArtifactScope:
    """The dotnet-isolated tree ``run_check`` takes as ``scope``: a ``Scope`` is *which* tree (vs ``Store`` = the I/O backend).

    Pairs the scoped ``path`` with the ``--artifacts-path`` ``dotnet_flags`` ``engine._splice`` splices
    into a ``Runner.DOTNET`` argv. ``open`` yields the per-run scope; ``build`` yields the *stable*
    per-closure scope (never run-scoped) so the warm MSBuild/analyzer tree survives across runs.
    """

    store: ArtifactStore
    path: str
    dotnet_flags: tuple[str, ...]

    @classmethod
    def open(cls, settings: AssaySettings, claim: Claim) -> ArtifactScope:
        """Per-run scope ``.artifacts/assay/<claim>/<run_id>/`` — disjoint per invocation."""
        store = settings.store()
        path = store.ensure(claim.value, settings.run_id)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @classmethod
    def build(cls, settings: AssaySettings, closure: str) -> ArtifactScope:
        """Stable per-closure scope ``.artifacts/assay/build/<closure>/`` — survives across runs."""
        store = settings.store()
        path = store.ensure(_BUILD, closure)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path, _DISABLE_BUILD_SERVERS))

    @property
    def dotnet_env(self) -> dict[str, str]:
        """The MSBuild isolation overlay: per-scope CLI home + disabled node reuse."""
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "MSBUILDDISABLENODEREUSE": "1"}


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["AssaySettings", "ArtifactScope", "ArtifactStore", "Configuration", "LogFormat"]
