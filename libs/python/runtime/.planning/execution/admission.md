# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, classification, and the inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings` with the keystore-then-secrets-file secret-resolution boundary on top. `CausalFrame` composes the host-minted `Hlc` two-half stamp (`transport/serve#CRDT_DECODE`) and the one `Tenant` partition decoded inbound (`transport/serve#PROTO_TRANSCODE`), re-minting nothing; `SecretBoundary` resolves the OS-keystore credential the outbound transport legs (`transport/roots#RESOURCE`) read, profile-gated and lazy on the outbound leg, never an eager unattended probe. The package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, caches a global mutable context, re-mints a causal stamp or tenant scheme, or probes the keystore eagerly.

## [01]-[INDEX]

- [01]-[CONTEXT]: profile, correlation, deadline, the inbound `CausalFrame`/`Tenant`, the one caller-owned context admission.
- [02]-[SETTINGS]: the local settings source order over `pydantic-settings` and the keystore-then-secrets-file `SecretBoundary`.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` — the one caller-supplied context owner discriminating profile/correlation/deadline/classification and carrying the inbound `causal` frame; `RuntimeProfile` the closed `StrEnum` vocabulary with a policy-row table; `Correlation`, `Deadline`, and `CausalFrame` the value objects it carries; `Tenant` the one wire-partition scheme.
- Cases: `RuntimeProfile` rows `TOOL` · `SIDECAR` · `PACKAGE` · `TEST`, each one row in the `PROFILE_POLICY` persistent `Map` carrying eager-import, scratch-writable, OTel-emit, and lane-capacity behavior columns; the profile is the key, the policy row is the value, and behavior travels on the row rather than a flag the caller re-derives; `RuntimeContext.causal` is `Option[CausalFrame]` — `Nothing` for a locally-minted context, `Some(frame)` for a context admitting the host-minted inbound stamp, exactly the `Option[Deadline]` carry already present.
- Entry: `RuntimeContext.admit` receives caller-owned host facts plus an optional decoded `CausalFrame` and returns the frozen record; `RuntimeContext.policy` reads the per-profile row from the `Map`; `RuntimeContext.attribute` projects `(tenant, hlc)` from the carried frame onto the span/receipt/metric attribute map so lanes, receipts, and metrics attribute to one host-minted frame; the owner never reads the environment and never resolves the host.
- Packages: `msgspec` (`Struct`/`field`), `expression` (`Map`/`Option`/`Nothing`/`Some`).
- Growth: a new context field is one column on `RuntimeContext`; a new profile is one `RuntimeProfile` row plus one `PROFILE_POLICY.add` entry; the causal/tenant frame is one `Option[CausalFrame]` column, never a parallel context record; zero new surface.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context; the C# `HostProfile` stays AppHost-owned and is never mirrored row-for-row; a sibling context record beside `RuntimeContext`, a second `Tenant` spelling (the raw `serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` is absorbed into the one `Tenant` newtype), and a re-minted `Hlc` stamp are the deleted forms — `CausalFrame` composes the existing `transport/serve#CRDT_DECODE` `Hlc`, the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` being the sole mint (single-mint invariant).

```python signature
from datetime import timedelta
from enum import StrEnum
from typing import Final, NewType, Self
from uuid import UUID, uuid4

from expression import Nothing, Option, Some
from expression.collections import Map
from msgspec import Struct, field

from rasm.runtime.serve import Hlc


type Tenant = NewType("Tenant", str)


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


class CausalFrame(Struct, frozen=True):
    hlc: Hlc
    tenant: Tenant

    @classmethod
    def of(cls, hlc_physical: int, hlc_logical: int, tenant: str) -> Self:
        return cls(hlc=Hlc(hlc_physical, hlc_logical), tenant=Tenant(tenant))


class RuntimeContext(Struct, frozen=True):
    profile: RuntimeProfile
    correlation: Correlation = field(default_factory=Correlation.mint)
    deadline: Option[Deadline] = Nothing
    classification: str = "internal"
    causal: Option[CausalFrame] = Nothing

    @classmethod
    def admit(
        cls, profile: RuntimeProfile, *, deadline: Deadline | None = None, classification: str = "internal", causal: CausalFrame | None = None
    ) -> Self:
        return cls(
            profile=profile,
            correlation=Correlation.mint(),
            deadline=Nothing if deadline is None else Some(deadline),
            classification=classification,
            causal=Nothing if causal is None else Some(causal),
        )

    @property
    def policy(self) -> ProfilePolicy:
        return PROFILE_POLICY[self.profile]

    def attribute(self) -> dict[str, str]:
        match self.causal:
            case Option(tag="some", some=frame):
                return {"rasm.tenant": str(frame.tenant), "rasm.hlc": f"{frame.hlc.physical_ticks}.{frame.hlc.logical}"}
            case _:
                return {}
```

## [03]-[SETTINGS]

- Owner: `SettingsAdmission` — the one local settings source order over `pydantic-settings`, admitting init mapping, environment-backed fields, dotenv, and OS secret files into a frozen record that rejects unknown fields; the source precedence is fixed at the one `settings_customise_sources` override. `SecretBoundary` — one `resolve(service, username) -> RuntimeRail[Option[str]]` facade composing the OS keystore as the top-priority source over the existing `secrets_dir`/`file_secret_settings` fallback tier, profile-gated and lazy on the outbound leg, never a flag set and never an eager unattended probe.
- Entry: `SettingsAdmission.admit` returns the frozen settings record; after admission no package reads the process environment. `SecretBoundary.resolve` reads `keyring.get_credential(service, username)` first, falls back to the settings-admitted secrets-file value on `errors.NoKeyringError` (the headless/container case the catalogue names), and returns `Ok(Nothing)` for an absent secret rather than a fault — a missing credential is a wire fact the outbound leg routes, not a boundary failure.
- Auto: precedence is init over env over dotenv over secret-file; the `env_prefix`/`env_nested_delimiter` map nested keys, and `secrets_dir` resolves OS-mounted secret files (the `file_secret_settings` source), so a secret never lands in an environment variable; the `SecretBoundary` keystore read is bounded to a `RuntimeProfile` admitting `emit_otel`/attended operation (`TOOL`/`SIDECAR`) — a `PACKAGE`/`TEST` profile never probes the keystore and resolves the secrets-file tier directly, so a session that cannot answer a keychain prompt never triggers one; `keyring.get_credential` returns `None` for a missing secret and never raises, the `credentials.SimpleCredential` structured `(username, password)` read carrying the resolved pair, and the keystore read is lazy at the outbound transport leg, never at admission.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`/`PydanticBaseSettingsSource`/`file_secret_settings`/`secrets_dir`), `pydantic`, `keyring` (`get_credential` ENTRYPOINTS [4] / `get_password` ENTRYPOINTS [1] facade, `errors.NoKeyringError` errors [6] headless fallback, `credentials.SimpleCredential` credentials [2] structured read).
- Growth: a new setting is one typed field on the settings model; a new source is one entry in the `settings_customise_sources` tuple; a new secret-resolution tier is one branch on the `SecretBoundary.resolve` source chain, never a second config owner; zero new surface.
- Boundary: no package reads `os.environ` directly after admission; environment reads outside this owner, an eager unattended keystore probe (the named hazard — a keychain prompt the session cannot answer), a secret read outside the one settings-admitted `SecretBoundary`, and a hand-rolled credential constructed beside the boundary are the deleted forms; the suite-wide configuration taxonomy stays AppHost-owned. The keystore reads OS credentials for the OUTBOUND transport legs only (`transport/roots#RESOURCE` `TransportResource.ssh` passphrase, `http` bearer); the companion UDS serve leg reads no keyring (peer identity is the kernel accept-time credential, `transport/serve#SERVE`).

```python signature
import keyring
from expression import Error, Nothing, Ok, Option, Some
from keyring import errors
from msgspec import Struct
from pydantic_settings import BaseSettings, PydanticBaseSettingsSource, SettingsConfigDict

from rasm.runtime.admission import ProfilePolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail


class SettingsAdmission(BaseSettings):
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__", secrets_dir="/run/secrets")

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


class SecretBoundary(Struct, frozen=True):
    settings: SettingsAdmission
    policy: ProfilePolicy

    def resolve(self, service: str, username: str) -> RuntimeRail[Option[str]]:
        if not self.policy.eager_import:
            return Ok(self._from_secrets_file(service, username))
        try:
            credential = keyring.get_credential(service, username)
        except errors.NoKeyringError:
            return Ok(self._from_secrets_file(service, username))
        except errors.KeyringError as cause:
            return Error(BoundaryFault(resource=(service, type(cause).__name__)))
        if credential is None:
            return Ok(self._from_secrets_file(service, username))
        return Ok(Some(credential.password))

    def _from_secrets_file(self, service: str, username: str) -> Option[str]:
        secret = getattr(self.settings, f"{service}_{username}", None)
        return Nothing if secret is None else Some(secret)
```

## [04]-[RESEARCH]

- [SETTINGS_SOURCE_ORDER]: the `settings_customise_sources` precedence (init over env over dotenv over secret-file), the `file_secret_settings` source class, and the `secrets_dir` `SettingsConfigDict` key are verified against the `pydantic-settings` catalogue; the source-class parameter spellings are settled.
- [SECRET_BOUNDARY]: `keyring.get_credential(service, username) -> Credential | None` (ENTRYPOINTS [4]) is the structured top-priority read returning `None` for a missing secret (never raising), `keyring.get_password` (ENTRYPOINTS [1]) the string-only variant, `errors.NoKeyringError` (errors [6]) the headless/container fallback the `pydantic-settings` `secrets_dir`/`file_secret_settings` tier catches, and `credentials.SimpleCredential` (credentials [2]) the resolved `(username, password)` pair — spellings settled against the `keyring` catalogue. The keystore read is profile-gated (`eager_import` rows only) and lazy on the outbound leg; the unattended-probe hazard is closed by the `PACKAGE`/`TEST` short-circuit to the secrets-file tier. `keyring` moves from orphaned-catalogue (zero design-page consumer after `transport/serve#KEYRING_CATALOGUE` dropped the serve leg) to the one `execution/admission#SETTINGS` outbound-credential consumer.
