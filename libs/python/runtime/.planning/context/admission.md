# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, and classification a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings` with secret-file and dotenv resolution. The package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, or caches a global mutable context.

## [1]-[INDEX]

- `[2]-[CONTEXT]` — profile, correlation, deadline, the one caller-owned context admission.
- `[3]-[SETTINGS]` — the local settings source order over `pydantic-settings`.

## [2]-[CONTEXT]

- Owner: `RuntimeContext` — the one caller-supplied context owner discriminating profile/correlation/deadline/classification; `RuntimeProfile` the closed `StrEnum` vocabulary with a policy-row table; `Correlation` and `Deadline` the value objects it carries.
- Cases: `RuntimeProfile` rows `TOOL` · `SIDECAR` · `PACKAGE` · `TEST`, each one row in the `PROFILE_POLICY` persistent `Map` carrying eager-import, scratch-writable, OTel-emit, and lane-capacity behavior columns; the profile is the key, the policy row is the value, and behavior travels on the row rather than a flag the caller re-derives.
- Entry: `RuntimeContext.admit` receives caller-owned host facts and returns the frozen record; `RuntimeContext.policy` reads the per-profile row from the `Map`; the owner never reads the environment and never resolves the host.
- Packages: `msgspec` (`Struct`/`field`), `expression` (`Map`/`Option`/`Nothing`/`Some`).
- Growth: a new context field is one column on `RuntimeContext`; a new profile is one `RuntimeProfile` row plus one `PROFILE_POLICY.add` entry; zero new surface.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context; the C# `HostProfile` stays AppHost-owned and is never mirrored row-for-row; a sibling context record beside `RuntimeContext` is the deleted form.

```python signature
from datetime import timedelta
from enum import StrEnum
from typing import Final, Self
from uuid import UUID, uuid4

from expression import Nothing, Option, Some
from expression.collections import Map
from msgspec import Struct, field


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


PROFILE_POLICY: Final[Map[RuntimeProfile, ProfilePolicy]] = Map.of_seq([
    (RuntimeProfile.TOOL, ProfilePolicy(RuntimeProfile.TOOL, eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=8)),
    (RuntimeProfile.SIDECAR, ProfilePolicy(RuntimeProfile.SIDECAR, eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=16)),
    (RuntimeProfile.PACKAGE, ProfilePolicy(RuntimeProfile.PACKAGE, eager_import=False, scratch_writable=False, emit_otel=False, lane_capacity=4)),
    (RuntimeProfile.TEST, ProfilePolicy(RuntimeProfile.TEST, eager_import=False, scratch_writable=True, emit_otel=False, lane_capacity=2)),
])


class Correlation(Struct, frozen=True):
    trace_id: UUID
    parent: Option[UUID] = Nothing

    @classmethod
    def mint(cls) -> Self:
        return cls(trace_id=uuid4())


class Deadline(Struct, frozen=True):
    budget: timedelta


class RuntimeContext(Struct, frozen=True):
    profile: RuntimeProfile
    correlation: Correlation = field(default_factory=Correlation.mint)
    deadline: Option[Deadline] = Nothing
    classification: str = "internal"

    @classmethod
    def admit(cls, profile: RuntimeProfile, *, deadline: Deadline | None = None, classification: str = "internal") -> Self:
        return cls(
            profile=profile,
            correlation=Correlation.mint(),
            deadline=Nothing if deadline is None else Some(deadline),
            classification=classification,
        )

    @property
    def policy(self) -> ProfilePolicy:
        return PROFILE_POLICY[self.profile]
```

## [3]-[SETTINGS]

- Owner: `SettingsAdmission` — the one local settings source order over `pydantic-settings`, admitting init mapping, environment-backed fields, dotenv, and OS secret files into a frozen record that rejects unknown fields; the source precedence is fixed at the one `settings_customise_sources` override.
- Entry: `SettingsAdmission.admit` returns the frozen settings record; after admission no package reads the process environment.
- Auto: precedence is init over env over dotenv over secret-file; the `env_prefix`/`env_nested_delimiter` map nested keys, and `secrets_dir` resolves OS-mounted secret files (the `file_secret_settings` source), so a secret never lands in an environment variable.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`/`PydanticBaseSettingsSource`), `pydantic`.
- Growth: a new setting is one typed field on the settings model; a new source is one entry in the `settings_customise_sources` tuple; zero new surface.
- Boundary: no package reads `os.environ` directly after admission; environment reads outside this owner are the deleted form; the suite-wide configuration taxonomy stays AppHost-owned.

```python signature
from pydantic_settings import BaseSettings, PydanticBaseSettingsSource, SettingsConfigDict


class SettingsAdmission(BaseSettings):
    model_config = SettingsConfigDict(
        frozen=True,
        extra="forbid",
        env_prefix="RASM_PY_",
        env_nested_delimiter="__",
        secrets_dir="/run/secrets",
    )

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
        return (init_settings, env_settings, dotenv_settings, file_secret_settings)
```

## [4]-[RESEARCH]

- [SETTINGS_SOURCE_ORDER]: the `settings_customise_sources` precedence (init over env over dotenv over secret-file) and the `secrets_dir` source-class spelling are verified against `pydantic-settings`; the source-class parameter spellings confirm at fence transcription.
