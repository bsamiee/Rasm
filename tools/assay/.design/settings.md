# [H1][ASSAY_SETTINGS_DESIGN]
>**Dictum:** *One pydantic surface holds env config; one `artifact(kind, *parts)` folds every path; nothing here models a wire shape.*

`composition/settings.py` exposes `AssaySettings` and imports `ArtifactKind` from `core/model.py`. Sole `pydantic-settings` consumer; every other shape is `msgspec`.

**Canonical:** [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) §5 · [`model.md`](model.md) (`ArtifactKind`) · [`snippets/cli.py.md`](snippets/cli.py.md) (registry passes `init_settings` overrides).

---
## [1][FIELD_SET]

`model_config = SettingsConfigDict(env_prefix="ASSAY_", env_nested_delimiter="__", frozen=True, extra="forbid")`.

```python
class Configuration(StrEnum):
    DEBUG = "Debug"
    RELEASE = "Release"
```

| [FIELD] | [TYPE / BOUND] | [DEFAULT] | [SOURCE] |
| --- | --- | --- | --- |
| `root` | `Path` (anchored to `Workspace.slnx`) | `Path.cwd` (factory) | env |
| `configuration` | `Configuration` | `Configuration.RELEASE` | env |
| `static_configuration` | `Configuration` | `Configuration.DEBUG` | env |
| `configurations` | `frozenset[Configuration] \| None` | `None` | env |
| `dotnet_max_cpu` | `Annotated[int, Field(ge=1, le=64)]` | `4` | env |
| `mutation_max_cpu` | `Annotated[int, Field(ge=1, le=64)]` | `2` | env |
| `test_target` | `Path` (expanded) | `tests/csharp/libs/Rasm/Rasm.Tests.csproj` | env |
| `mutation_project` | `Path` (expanded) | `libs/csharp/Rasm/Rasm.csproj` | env |
| `mutation_test_project` | `Path` (expanded) | `=test_target default` | env |
| `mutation_target_framework` | `str` | `"net10.0"` | env |
| `rhino_app` | `Path \| None` | discovery factory | env |
| `bridge_endpoint` | `Path` | `~/.rasm/rhino-bridge.json` | env |
| `test_timeout_s` | `Annotated[float, Field(gt=0.0)]` | `300.0` | env |
| `mutation_timeout_s` | `Annotated[float, Field(gt=0.0)]` | `1200.0` | env |
| `scenario_timeout_s` | `Annotated[float, Field(gt=0.0)]` | `180.0` | env |
| `verify_retention_s` | `Annotated[float, Field(gt=0.0)]` | `300.0` | env |
| `run_id` | `str` | `"%Y-%m-%dT%H-%M-%S.%f-{pid}"` factory | init only (registry/Cyclopts) |
| `log_format` | `Literal["human", "ci"]` *(P2: `LogFormat` StrEnum)* | derived from `stderr.isatty()` | env |

Bounded vocabularies (`CS_SUFFIXES`, trigger files, mutation thresholds) stay immutable `Final` catalog data in the owning rail, never env-driven.

**Env-only discipline:** host/infra scalars (`mutation_target_framework`, `rhino_app`, `bridge_endpoint`, `verify_retention_s`, CPU limits, targets) are env-only — no per-field Cyclopts flags on `AssaySettings`. Verb-specific overrides flow through frozen `@dataclass` `Params` and `settings.model_copy(update=...)` inside handlers.

---
## [2][ARTIFACT_DERIVATION]

```python
@property
def artifact_root(self) -> Path:
    return self.root / ".artifacts" / "assay"

def artifact(self, kind: ArtifactKind, *parts: str | Path) -> Path:
    return self.artifact_root.joinpath(kind.value, *(str(p) for p in parts))
```

| [RETIRED PROPERTY] | [DERIVATION] |
| --- | --- |
| `mutation_lock` / `bridge_lock` | `artifact(LOCKS, "mutation.lock")` / `artifact(LOCKS, "bridge.lock")` |
| `build_lock(closure)` | `artifact(LOCKS, f"build-{closure}.lock")` |
| `test_results(all)` | `artifact(TEST, "all" if all else slice, run_id)` |
| `mutation_output_dir` | `artifact(MUTATION, slice, run_id)` |
| `bridge_verify_root` | `artifact(RHINO, "verify", run_id)` |
| `ArtifactScope.open` | `artifact(PROCESS, claim.value, run_id)` |

---
## [3][VALIDATORS_AND_COMPUTED]

- `@field_validator("root", mode="before")` → anchor to nearest `Workspace.slnx`.
- `@field_validator("test_target", ...)` → `expanduser`.
- `@field_validator("rhino_app", mode="before")` → coerce existing `*.app` or `None`; `AliasChoices("ASSAY_RHINO_APP", "RHINO_WIP_APP_PATH")`.
- `@field_validator("configurations", mode="before")` → split env string into `frozenset[Configuration]`; pydantic validates tokens against the enum.
- `@model_validator(mode="after")` → `mutation_test_project` under `root` when mutation runs.
- `@computed_field` only for `solution = root / "Workspace.slnx"` when a dump needs it; parameterized paths use `artifact()` only.

---
## [4][SOURCE_PRECEDENCE]

```python
@classmethod
def settings_customise_sources(cls, settings_cls, init_settings, env_settings, dotenv_settings, file_secret_settings):
    return (init_settings, env_settings)  # Cyclopts/registry init > ASSAY_* env; no CliSettingsSource, no dotenv
```

Cyclopts owns argv; registry constructs `AssaySettings(**overrides)` as `init_settings` at top precedence. **Do not** instantiate `CliSettingsSource` (`research-pydantic-settings.md` §3).

---
## [5][PYDANTIC_BOUNDARY]

`AssaySettings` models only validated config scalars and `Path`s; never embeds or encodes `msgspec` shapes. `ArtifactKind` is defined in `core/model.py` and referenced unchanged into `Artifact.kind` — vocabulary reuse, not a duplicated shape.

---
## [6][OPEN_DECISIONS]

1. **`ASSAY_RUN_ID`** for CI replay vs init-only `run_id`.
2. **`.env` discovery** — disabled by default for hermetic parallel agents.
3. **`SecretStr`** when OTLP header or yak token enters config.
