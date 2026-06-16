# [PY_RUNTIME_CONTEXT_SETTINGS]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, and classification a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings`. The package never discovers the host, starts services, owns lifecycle, derives product roots, or caches a global mutable context.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                       |
| :-----: | :-------- | :---------------------------------------------------------- |
|   [1]   | CONTEXT   | profile, correlation, deadline, the one context admission   |
|   [2]   | SETTINGS  | local settings source order over pydantic-settings          |

## [2]-[CONTEXT]

- Owner: `RuntimeContext` — the one caller-supplied context owner discriminating profile/correlation/deadline/classification; `RuntimeProfile` the closed `StrEnum` vocabulary with a policy-row table; `Correlation` and `Deadline` the value objects it carries.
- Cases: `RuntimeProfile` rows `TOOL` · `SIDECAR` · `PACKAGE` · `TEST`, each a row in the frozen policy table carrying import, filesystem, observability, and concurrency columns.
- Entry: `RuntimeContext.admit` receives caller-owned host facts and returns the frozen record; never reads the environment, never resolves the host.
- Packages: `pydantic` (`BaseModel`/`Field`), `pydantic-settings`, `msgspec`.
- Growth: a new context field is one column on `RuntimeContext`; a new profile is one `RuntimeProfile` row plus one policy entry; zero new surface.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context; the C# `HostProfile` stays AppHost-owned and is never mirrored row-for-row; a sibling context record beside `RuntimeContext` is the deleted form.

```python signature
from datetime import timedelta
from enum import StrEnum
from typing import Final, Self
from uuid import UUID, uuid4

from expression import Nothing, Option
from msgspec import Struct
from pydantic import BaseModel, Field


class RuntimeProfile(StrEnum):
    TOOL = "tool"
    SIDECAR = "sidecar"
    PACKAGE = "package"
    TEST = "test"


class ProfilePolicy(Struct, frozen=True):
    profile: RuntimeProfile
    eager_import: bool
    scratch_writable: bool
    emit_otel: bool
    lane_capacity: int


PROFILE_POLICY: Final[dict[RuntimeProfile, ProfilePolicy]] = {
    RuntimeProfile.TOOL: ProfilePolicy(RuntimeProfile.TOOL, eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=8),
    RuntimeProfile.SIDECAR: ProfilePolicy(RuntimeProfile.SIDECAR, eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=16),
    RuntimeProfile.PACKAGE: ProfilePolicy(RuntimeProfile.PACKAGE, eager_import=False, scratch_writable=False, emit_otel=False, lane_capacity=4),
    RuntimeProfile.TEST: ProfilePolicy(RuntimeProfile.TEST, eager_import=False, scratch_writable=True, emit_otel=False, lane_capacity=2),
}


class Correlation(Struct, frozen=True):
    trace_id: UUID
    parent: Option[UUID] = Nothing

    @classmethod
    def mint(cls) -> Self:
        return cls(trace_id=uuid4())


class Deadline(Struct, frozen=True):
    budget: timedelta


class RuntimeContext(BaseModel, frozen=True):
    profile: RuntimeProfile
    correlation: Correlation
    deadline: Option[Deadline] = Nothing
    classification: str = Field(default="internal")

    @property
    def policy(self) -> ProfilePolicy:
        return PROFILE_POLICY[self.profile]
```

## [3]-[SETTINGS]

- Owner: `SettingsAdmission` — the one local settings source order over `pydantic-settings`, admitting an explicit mapping, an environment-backed object, or a caller-supplied payload into a frozen record that rejects unknown fields.
- Entry: `SettingsAdmission.admit` returns the frozen settings record; after admission no package reads the process environment.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`), `pydantic`.
- Growth: a new setting is one typed field on the settings model; a new source is one entry in `settings_customise_sources`; zero new surface.
- Boundary: no package reads `os.environ` directly after admission; environment reads outside this owner are the deleted form; the suite-wide configuration taxonomy stays AppHost-owned.

```python signature
from pydantic_settings import BaseSettings, PydanticBaseSettingsSource, SettingsConfigDict


class SettingsAdmission(BaseSettings):
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__")

    scratch_root: str
    object_store_root: str | None = None
    otel_endpoint: str | None = None

    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> tuple[PydanticBaseSettingsSource, ...]:
        return (init_settings, env_settings, file_secret_settings)
```

## [4]-[RESEARCH]

- [SETTINGS_SOURCE_ORDER]: the `settings_customise_sources` precedence (init over env over secret) is verified against `pydantic-settings>=2.14.0`; the source-class parameter spellings confirm at fence transcription.
