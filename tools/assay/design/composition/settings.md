# [H1][ASSAY_SETTINGS]
>**Dictum:** *One `pydantic-settings` surface holds env config; `artifact(kind,*parts)` folds every path; `ArtifactStore` makes `.artifacts` backend-agnostic; `ArtifactScope` carries the per-run/closure path + dotnet overlay the engine consumes — and nothing here models a wire shape.*

## [1][PURPOSE]

`composition/settings.py` owns the sole `pydantic-settings` consumer in `assay`: `AssaySettings(BaseSettings)` (D20/D21), the `artifact(kind,*parts)` path fold, the `fsspec`-backed `ArtifactStore` (D39), and `ArtifactScope` — the run/closure scope (`open`/`build(closure)`, D32) carrying the `--artifacts-path` `dotnet_flags` + `DOTNET_CLI_HOME` overlay that `run_check` receives as `scope` and `engine._splice` pattern-matches. It imports `ArtifactKind` from `core/model.py` (§5 type table; D18 — *vocabulary reuse, not a duplicated shape*) and never embeds, encodes, or re-models a `msgspec` struct. Per §5 ingress rule env/flag is validated by `pydantic` exactly **once**; post-`__init__` attributes are plain primitives the engine, routing, and rails consume unchecked. Build stage 4 (`status → model → settings`); consumed downstream by `engine` (leases, scope), `routing` (`place(...,*,settings)` SOLUTION arm, D23), and the bespoke rails.

The path type is `upath.UPath`, not `pathlib.Path`: a `pathlib`-API drop-in whose local (`file://`) protocol keeps **identical** local-FS semantics while the same surface unlocks `memory://` test isolation and remote backends with no call-site change. The settings surface is the single owner of this canonical path type; the `msgspec`-owned `Check.cwd` (which a `UPath` cannot encode) and the rail-local subprocess/yak/scenario helpers stay `pathlib.Path` and coerce a settings path to a local `Path(str(settings.root))` at their seam — a subprocess `cwd`, a yak output-dir guard, an ilspy decompile cache, and a `.csproj` read are inherently local, never `memory://`. The de-hardcoded engine/routing constants (stream tail/chunk bytes, lease NTP-drift band, the dotnet scoped-verb set, and the full/`FULL`-escalation trigger-file + prefix vocabularies) now live as bounded settings fields so a host with a different NTP discipline, a larger stream window, or an extended trigger set is one env var or one `model_copy(update=...)`, never an edit to `core/engine.py`/`core/routing.py`.

## [2][CANONICAL_SHAPES]

Two behavior-carrying `StrEnum`s replace the `.design` `Literal["human","ci"]` (D21 — `LogFormat` is an enum, **never** a `Literal`; `Configuration` is its sibling):

```python
class Configuration(StrEnum):
    DEBUG = "Debug"; RELEASE = "Release"          # MSBuild -c value, verbatim

class LogFormat(StrEnum):
    HUMAN = "human"; CI = "ci"                     # structlog renderer selector
```

`AssaySettings(BaseSettings)` field table — env-only host/infra scalars (no per-field Cyclopts flag; verb overrides arrive via frozen `@dataclass` `Params` + `settings.model_copy(update=...)` D19):

| [FIELD] | [TYPE / BOUND] | [DEFAULT] | [SOURCE] |
| --- | --- | --- | --- |
| `root` | `UPath` (anchored to nearest `Workspace.slnx`) | `UPath.cwd` factory | env |
| `configuration` | `Configuration` | `RELEASE` | env |
| `static_configuration` | `Configuration` | `DEBUG` | env |
| `dotnet_max_cpu` | `Annotated[int, Field(ge=1, le=64)]` | `4` | env |
| `mutation_max_cpu` | `Annotated[int, Field(ge=1, le=64)]` | `2` | env |
| `max_checks` | `Annotated[int, Field(ge=1, le=64)]` | `8` | env — `CapacityLimiter` fan-out cap (§7, D33) |
| `stream_tail_bytes` | `Annotated[int, Field(ge=512)]` | `4096` | env — engine bounded-tail deque cap (stdout/stderr retention) |
| `stream_chunk_bytes` | `Annotated[int, Field(ge=4096)]` | `65536` | env — engine `ByteReceiveStream.receive` max per read |
| `lease_drift_tolerance` | `Annotated[float, Field(gt=0)]` | `1.0` | env — psutil `create_time` NTP-drift band, seconds (§7, D34) |
| `scoped_verbs` | `frozenset[str]` | `{build,clean,msbuild,pack,publish,restore,run,test}` | env — dotnet build-graph verbs `engine._splice` scopes |
| `trigger_files` | `frozenset[str]` | 7 C# anchors (`Directory.*.props`, `.slnx`, `.editorconfig`, …) | env — a changed member escalates the route to `FULL` (D6) |
| `trigger_prefixes` | `tuple[str, ...]` | `("tools/cs-analyzer/",)` | env — any descendant escalates the route to `FULL` |
| `test_target` | `UPath` (expanded) | `tests/csharp/libs/Rasm/Rasm.Tests.csproj` | env |
| `mutation_project` | `UPath` (expanded) | `libs/csharp/Rasm/Rasm.csproj` | env |
| `log_format` | `LogFormat` | derived from `sys.stderr.isatty()` (validator) | env |
| `run_id` | `str` | `"%Y-%m-%dT%H-%M-%S.%f-{pid}"` factory | init only (registry/Cyclopts); `ASSAY_RUN_ID` overrides (§7) |
| `agent_task_id` | `str` | `""` | init/`ASSAY_AGENT_TASK_ID` — fleet correlation id |
| `exec_target` | `str` (`""` or `ssh://[user@]host[:port]`) | `""` | init/`ASSAY_EXEC_TARGET` — `""` = local, `ssh://…` = remote process-exec (§7) |

`model_config = SettingsConfigDict(env_prefix="ASSAY_", case_sensitive=False, frozen=True, extra="forbid", populate_by_name=True)` — `frozen=True` makes the instance hashable and STM-safe under fan-out; `populate_by_name=True` lets the registry inject by canonical field name through the validation alias. `Meta`-style bounds (`ge`/`le`/`gt`) fire at pydantic ingress, mirroring §5's decode-time invariant for `msgspec`. `AliasChoices` supplies the init/registry override chain at the boundary — `run_id` (`AliasChoices("ASSAY_RUN_ID", "run_id")`) and `agent_task_id` (`AliasChoices("ASSAY_AGENT_TASK_ID", "agent_task_id")`) each carry one, so the registry/Cyclopts injection and the `ASSAY_*` env both bind the canonical field; canonical names stay internal.

`exec_target` is the sole knob that makes execution transparent local-vs-remote: `""` selects local process-exec; an `ssh://[user@]host[:port]` URI selects a remote host. A light `@field_validator("exec_target")` admits only those two forms (else `ValueError`) — no parsing ceremony beyond the `ssh://` prefix gate. The crucial invariant: **process-exec is the one thing that varies by target** — the subprocess spawn (local vs. SSH-tunnelled) is the only seam that reads `exec_target`; all artifact I/O continues to ride the `UPath`/`fsspec` `ArtifactStore` surface unchanged, since a `memory://`/`s3://`/remote path is already protocol-addressed and needs no per-target branch. A remote target therefore re-targets where commands *run*, not where bytes *land*.

Two `@computed_field` projections ride alongside the scalars: `solution` (`root / Workspace.slnx`, the SOLUTION arm + `_reconcile` anchor) and `agent_context: dict[str, str]` — the `{run.id, agent.task.id}` correlation map folded from `run_id` + `agent_task_id` (read once at the pydantic boundary through the field aliases, **never** `os.environ`). The engine/automation seams stamp `agent_context` onto OTel spans and structlog context so every Envelope in a fleet correlates to its driving agent (extends the D50 `run_id`-as-span-attribute observability lock with the agent-task axis).

`ArtifactStore` (D39) is the `fsspec` I/O backend (`file://` local default, `memory://` for zero-IO test isolation), keyed on a `str` root (not a `UPath`) because `fsspec` addresses bytes by protocol+string while the `UPath` settings surface owns the typed-path projection; `ArtifactScope` is the scope value `run_check` takes as `scope` — it pairs the scoped `path` with the `dotnet_flags` (`--artifacts-path …`) that `engine._splice` (D32) splices into a `Runner.DOTNET` argv, and exposes the `DOTNET_CLI_HOME`/`MSBUILDDISABLENODEREUSE` overlay. `open` yields the per-run scope (`.artifacts/assay/<claim>/<run_id>/`); `build(closure)` yields the **stable** per-closure scope (`.artifacts/assay/build/<closure>/`, not run-scoped) so the warm MSBuild/analyzer tree survives across runs. The two are distinct: a `Store` is *where* bytes go; a `Scope` is *which* dotnet-isolated tree this invocation targets.

## [3][VALIDATED_SNIPPET]

```python
# --- [TYPES] ----------------------------------------------------------------------------
class Configuration(StrEnum):
    DEBUG = "Debug"
    RELEASE = "Release"

class LogFormat(StrEnum):
    HUMAN = "human"
    CI = "ci"

# --- [MODELS] ---------------------------------------------------------------------------
class AssaySettings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="ASSAY_", case_sensitive=False, frozen=True, extra="forbid", populate_by_name=True)

    root: AnchoredRoot = Field(default_factory=UPath.cwd)                    # AnchoredRoot/ExpandedPath are Annotated[UPath, BeforeValidator]
    configuration: Configuration = Configuration.RELEASE
    static_configuration: Configuration = Configuration.DEBUG
    dotnet_max_cpu: Annotated[int, Field(ge=1, le=64)] = 4
    mutation_max_cpu: Annotated[int, Field(ge=1, le=64)] = 2
    max_checks: Annotated[int, Field(ge=1, le=64)] = 8                       # CapacityLimiter fan-out cap (§7)
    stream_tail_bytes: Annotated[int, Field(ge=512)] = 4096                  # engine bounded-tail deque cap
    stream_chunk_bytes: Annotated[int, Field(ge=4096)] = 65536              # engine ByteReceiveStream.receive max per read
    lease_drift_tolerance: Annotated[float, Field(gt=0)] = 1.0              # psutil create_time NTP-drift band (s)
    scoped_verbs: frozenset[str] = frozenset(("build", "clean", "msbuild", "pack", "publish", "restore", "run", "test"))
    trigger_files: frozenset[str] = frozenset((".config/dotnet-tools.json", "Directory.Build.props", "Directory.Build.targets",
                                               "Directory.Packages.props", "Workspace.slnx", ".editorconfig", "global.json"))
    trigger_prefixes: tuple[str, ...] = ("tools/cs-analyzer/",)             # any descendant escalates the route to FULL
    test_target: ExpandedPath = UPath("tests/csharp/libs/Rasm/Rasm.Tests.csproj")
    mutation_project: ExpandedPath = UPath("libs/csharp/Rasm/Rasm.csproj")
    log_format: LogFormat = Field(default_factory=lambda: LogFormat.HUMAN if sys.stderr.isatty() else LogFormat.CI)
    run_id: str = Field(default_factory=lambda: f"{datetime.now(tz=UTC):%Y-%m-%dT%H-%M-%S.%f}-{os.getpid()}",
                        validation_alias=AliasChoices("ASSAY_RUN_ID", "run_id"))  # init/registry override binds canonical field
    agent_task_id: str = Field(default="", validation_alias=AliasChoices("ASSAY_AGENT_TASK_ID", "agent_task_id"))  # fleet correlation
    exec_target: str = Field(default="", validation_alias=AliasChoices("ASSAY_EXEC_TARGET", "exec_target"),
                             description="execution target; '' = local, ssh://[user@]host[:port] = remote")

    @field_validator("exec_target")
    @classmethod
    def _exec_target(cls, value: str) -> str:                                # '' (local) or ssh:// URI (remote); else ValueError
        match value:
            case "" | str() if value.startswith("ssh://"):
                return value
            case _:
                raise ValueError(f"exec_target must be '' or 'ssh://[user@]host[:port]': {value!r}")

    @classmethod
    def settings_customise_sources(cls, settings_cls, init_settings, env_settings,
                                   dotenv_settings, file_secret_settings):
        return (init_settings, env_settings)                                # init > ASSAY_* env; no CLI, no dotenv, no secrets

    @model_validator(mode="after")
    def _reconcile(self) -> Self:                                           # root must anchor a real Workspace.slnx, not bare cwd
        match self.solution.is_file():
            case False:
                raise ValueError(f"ASSAY_ROOT does not anchor a Workspace.slnx tree: {self.root}")
            case True:
                return self

    @computed_field
    @property
    def solution(self) -> UPath:
        return self.root / "Workspace.slnx"

    @computed_field
    @property
    def agent_context(self) -> dict[str, str]:                              # {run.id, agent.task.id} correlation map; pydantic-read, never os.environ
        return {"run.id": self.run_id, "agent.task.id": self.agent_task_id}

    @property
    def store_root(self) -> UPath:
        return self.root / ".artifacts" / "assay"                           # every artifact path + local store share it

    def store(self, *, protocol: str = "file", **opts: object) -> "ArtifactStore":
        target = str(self.store_root) if protocol == "file" else f".artifacts/assay/{self.run_id}"  # memory roots run_id-keyed
        return ArtifactStore(fs=fsspec.filesystem(protocol, **opts), root=target)

    def artifact(self, kind: ArtifactKind, *parts: str | UPath) -> UPath:   # one path fold; retires every *_lock property
        return self.store_root.joinpath(kind.value, *(str(p) for p in parts))

# --- [SERVICES] -------------------------------------------------------------------------
@dataclass(frozen=True, slots=True)
class ArtifactStore:                                                        # fsspec backend-agnostic .artifacts I/O
    fs: AbstractFileSystem
    root: str                                                               # store_root (file) or ".artifacts/assay/<run_id>" (memory)
    def ensure(self, *parts: str) -> str:                                   # sole dir creator; LocalFileSystem.auto_mkdir=False
        path = "/".join((self.root, *parts)); self.fs.makedirs(path, exist_ok=True); return path
    def glob(self, pattern: str) -> tuple[str, ...]:
        return tuple(self.fs.glob(f"{self.root}/{pattern}"))
    def exists(self, *parts: str) -> bool:                                  # lease/staleness probes
        return bool(self.fs.exists("/".join((self.root, *parts))))

@dataclass(frozen=True, slots=True)
class ArtifactScope:                                                        # the `scope` run_check takes; engine._splice matches it
    store: ArtifactStore
    path: str
    dotnet_flags: tuple[str, ...]                                           # ("--artifacts-path", path, "--disable-build-servers")
    @classmethod
    def open(cls, settings: "AssaySettings", claim: Claim) -> "ArtifactScope":   # per-run: .artifacts/assay/<claim>/<run_id>/
        store = settings.store(); path = store.ensure(claim.value, settings.run_id)
        return cls(store, path, ("--artifacts-path", path, "--disable-build-servers"))
    @classmethod
    def build(cls, settings: "AssaySettings", closure: str) -> "ArtifactScope":  # STABLE per-closure, never run-scoped
        store = settings.store(); path = store.ensure("build", closure)
        return cls(store, path, ("--artifacts-path", path, "--disable-build-servers"))
    @property
    def dotnet_env(self) -> dict[str, str]:
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "MSBUILDDISABLENODEREUSE": "1"}
```

`artifact(kind,*parts)` is the sole path projector over the canonical `ArtifactKind` set `{LOCKS, PROCESS, TEST, MUTATION, RHINO, SCOPE}` (model.md §5): `artifact(LOCKS,"mutation.lock")`, `artifact(LOCKS,f"build-{closure}.lock")`, `artifact(RHINO,"verify",run_id)`, `artifact(TEST,slice,run_id)`, `artifact(PROCESS,run_id)`, `artifact(MUTATION,run_id)`. There is **no** `BUILD` kind — a closure build tree lives in the `SCOPE` namespace: `artifact(ArtifactKind.SCOPE, "build", closure)` is the path-fold equivalent of `ArtifactScope.build(settings, closure)` (D32, above). The `*_lock`/`*_dir` properties of `tools/quality` are retired — the lock-path namespace is exactly the `ArtifactKind` member set in `core/model.py` (§5), no member added or omitted.

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| --- | --- | --- |
| `core/model.py` | imports | `ArtifactKind` enum (D18); `kind.value` is the lock/path namespace segment. |
| `core/engine.py` | consumes | `settings.max_checks` → `CapacityLimiter`; `settings.stream_tail_bytes`/`stream_chunk_bytes` bound the streaming deque + `ByteReceiveStream.receive`; `settings.scoped_verbs` gates the `_splice` build-graph match; `settings.lease_drift_tolerance` is the psutil `(pid, create_time)` staleness band (D34); `artifact(LOCKS,...)` lease paths; `ArtifactScope.open`/`.build(closure)` → `scope` param; `settings.agent_context` (incl. `run_id`) stamps the `traced` child-span attributes correlating each `run_check` span with its parent rail span (engine seam, not `logged`); `psutil` liveness reads `run_id`/pid into the owner block (§7). A subprocess `cwd` is a local `Path(str(settings.root))` — never a `memory://` `UPath`. |
| `core/routing.py` | consumes | `settings.solution` for the `place(routed, tool, *, settings)` SOLUTION arm; `settings.trigger_files`/`trigger_prefixes` drive the `FULL`-escalation predicate (D6) over the changed set. |
| `composition/registry.py` | constructs | `AssaySettings(**overrides)` injected as `init_settings` at top precedence; each bind's handler is woven via `compose(*_RAIL_LAYERS)(_narrow(handler))` (`_narrow` retypes the bare callable to `Handler[object]`); `log_format` selects the structlog renderer at `__init__.py` configure. |
| `core/engine.py`, rails | write/read | `ArtifactStore.fs` (the `fsspec.AbstractFileSystem`) is the **sole** writer for every `.artifacts/` tree; engine and rails route all artifact I/O through `store.glob`/`exists`/`ensure`, **never** bare `pathlib`; `protocol="file"` is the default, `"memory"` under test. |
| `automation/engine.py` | consumes | `AssaySettings` threaded through the `_watch`/`_schedule` tasklets, both hosted over **one** shared `anyio.Event` stop so a single `stop.set()` (SIGTERM or task-group cancel) drains watch + cron together. |
| tests | swap | `settings.store(protocol="memory")` → `memory://` zero-IO `ArtifactStore`; no disk, global-per-class FS isolated per suite (root is `run_id`-keyed). |

## [5][EXTENSIBILITY]

A new host scalar is one typed `Field` (with `AliasChoices` if a legacy env name exists); a de-hardcoded engine/routing constant (a stream window, a drift band, a scoped verb, a trigger anchor) is one bounded `Field` here, never a literal in `core/engine.py`/`core/routing.py`; a new correlation axis is one field folded into `agent_context`; remote execution is the single `exec_target` knob (`""`=local, `ssh://…`=remote) read **only** at the process-exec seam, leaving the `UPath`/`fsspec` I/O surface untouched; a new artifact bucket is one `ArtifactKind` member in `core/model.py` (never a new property or `settings` field); a remote backend is one `protocol=` argument to `store(...)` (`filecache::s3`) and a `memory://`/`s3://` `UPath` root — the `artifact()` fold and every call site are untouched. Because `UPath` is the canonical path type, only the inherently-local seams (subprocess `cwd`, yak output-dir guard, ilspy decompile cache, `.csproj` read) coerce to `Path(str(settings.root))`; an fsspec-routed artifact path stays a `UPath` end-to-end.

## [6][CONSIDERATIONS]

- `MemoryFileSystem` is **global per class**, not per instance (dossier gotcha): parallel test workers sharing `memory://` will cross-contaminate `.artifacts`. Key `ArtifactStore.root` on `run_id` (already disjoint per §7) so concurrent suites partition the shared dict rather than collide; do not assume construction isolates state.
- `LocalFileSystem.auto_mkdir=False` by default — `build()` explicitly `makedirs(...,exist_ok=True)` rather than relying on lazy `open(mode="w")` parent creation; the same discipline must hold for any future writer that bypasses `build()`.
- The `frozen=True` config makes `AssaySettings` hashable, so a `settings.model_copy(update=...)` per verb yields a *new* immutable instance safe to thread through `fan_out` without a lock; resist adding a mutable field for run-time state — push transient state onto `Params` or the `ArtifactScope`, never onto the config surface.
- `upath.UPath` is a `pathlib`-API drop-in but **not** statically a `pathlib.Path` subclass (the class dynamically dispatches to a protocol-specific subtype at `__init__`), so `ty`/`mypy` reject a `UPath` where a `Path | PathLike[str]` is annotated and `Path(upath)` itself fails to type-check. The local protocol instance *is* a `Path` at runtime (`isinstance(UPath('x'), Path)` holds), but `msgspec` **cannot encode** a `UPath` — hence `Check.cwd` (a `msgspec.Struct` field) stays `pathlib.Path` and the inherently-local rail seams coerce `Path(str(settings.root))` (statically valid; `str(local_upath)` yields the bare local path). Never coerce a `memory://`/remote `UPath` with `Path(...)` — `Path(memory_upath)` raises; those paths must stay `UPath` and flow through `ArtifactStore.fs` (fsspec), never bare `pathlib`.
- `agent_context` is a `@computed_field`, so it serializes with the model and re-derives on every read — keep it a pure projection of `run_id`/`agent_task_id`; do not push a third correlation source through `os.environ` at the read site (that would re-introduce the double-ingress the §5 once-only rule forbids).
