# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, classification, and the inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings` with the keystore-then-secrets-file secret-resolution boundary on top. `CausalFrame` is the `clock/clock#CLOCK` two-half owner — admission consumes the host-minted `Hlc` stamp and the one `Tenant` partition from `clock`, re-minting nothing and re-spelling neither; `SecretBoundary` resolves the OS-keystore credential the outbound transport legs (`transport/roots#RESOURCE`) read, profile-gated and lazy on the outbound leg, never an eager unattended probe. The package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, caches a global mutable context, re-mints a causal stamp or tenant scheme, or probes the keystore eagerly. Feature gating and killswitch state are not boolean knobs the caller re-derives — they ride the `PROFILE_POLICY` table as data-driven rows so a profile resolves its admitted-feature set and its tripped killswitches from the same policy lookup the eager-import and lane-capacity columns already answer.

## [01]-[INDEX]

- [01]-[CONTEXT]: profile, correlation, deadline, the inbound `CausalFrame`/`Tenant` consumed from `clock`, the per-profile feature/killswitch policy rows, the one caller-owned context admission.
- [02]-[SETTINGS]: the local settings source order over `pydantic-settings` (init/env/dotenv/secret-file), and the keystore-then-secrets-file `SecretBoundary`.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` — the one caller-supplied context owner discriminating profile/correlation/deadline/classification and carrying the inbound `causal` frame; `RuntimeProfile` the closed `StrEnum` vocabulary with a policy-row table; `Correlation` and `Deadline` the value objects it carries; `Hlc`, `CausalFrame`, and `Tenant` arrive from the `clock/clock#CLOCK` owner, never re-spelled here.
- Cases: `RuntimeProfile` rows `TOOL` · `SIDECAR` · `PACKAGE` · `TEST`, each one row in the `PROFILE_POLICY` persistent `Map` carrying eager-import, scratch-writable, OTel-emit, lane-capacity, and the `FeatureGate` admitted-feature/killswitch columns; the profile is the key, the policy row is the value, and behavior travels on the row rather than a flag the caller re-derives; `FeatureGate` is itself a data-driven cell — an admitted-`Feature` set and a tripped-`Killswitch` set folded into the policy row, so `RuntimeContext.admits`/`tripped` discriminate by membership rather than a parallel boolean field per capability; `RuntimeContext.causal` is `Option[CausalFrame]` — `Nothing` for a locally-minted context, `Some(frame)` for a context admitting the host-minted inbound stamp, exactly the `Option[Deadline]` carry already present.
- Entry: `RuntimeContext.admit` receives caller-owned host facts plus an optional decoded `CausalFrame` and returns the frozen record; `RuntimeContext.policy` reads the per-profile row from the `Map`; `RuntimeContext.admits(feature)` and `RuntimeContext.tripped(killswitch)` answer the gate by membership in the policy row's `FeatureGate` sets so a guarded path dispatches on data, not a re-derived boolean; `RuntimeContext.attribute` projects `(tenant, hlc)` from the carried frame onto the span/receipt/metric attribute map so lanes, receipts, and metrics attribute to one host-minted frame; the owner never reads the environment and never resolves the host.
- Packages: `msgspec` (`Struct`/`field`), `expression` (`Map`/`Option`/`Nothing`/`Some`).
- Growth: a new context field is one column on `RuntimeContext`; a new profile is one `RuntimeProfile` row plus one `PROFILE_POLICY.add` entry; a new feature or killswitch is one `Feature`/`Killswitch` enum case plus its membership in the affected profile rows' `FeatureGate` sets, never a parallel boolean knob or a second flag owner; the causal/tenant frame is the `clock`-owned `Option[CausalFrame]` column, never a parallel context record; zero new surface.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context; the C# `HostProfile` stays AppHost-owned and is never mirrored row-for-row; a sibling context record beside `RuntimeContext`, a second `Tenant` spelling (the raw `serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` is absorbed into the one `clock`-owned `Tenant` newtype), a re-minted `Hlc` stamp, and a boolean-per-capability feature flag beside the policy table are the deleted forms — `CausalFrame`/`Hlc`/`Tenant` are the `clock/clock#CLOCK` owner's records, the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` being the sole mint (single-mint invariant). The broader multi-source structured settings schema stays the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA — this page realizes only its local cell: the feature/killswitch rows folded into `PROFILE_POLICY`.

```python signature
from datetime import timedelta
from enum import StrEnum
from typing import Final, Self
from uuid import UUID, uuid4

from expression import Nothing, Option, Some
from expression.collections import Map
from msgspec import Struct, field

from rasm.runtime.clock import CausalFrame, Tenant


class RuntimeProfile(StrEnum):
    TOOL = "tool"
    SIDECAR = "sidecar"
    PACKAGE = "package"
    TEST = "test"


class Feature(StrEnum):
    SECRET_MANAGER = "secret_manager"
    KEYSTORE_PROBE = "keystore_probe"
    OUTBOUND_TRANSPORT = "outbound_transport"


class Killswitch(StrEnum):
    DISABLE_OUTBOUND = "disable_outbound"
    DISABLE_SECRET_MANAGER = "disable_secret_manager"


class FeatureGate(Struct, frozen=True):
    admitted: frozenset[Feature]
    tripped: frozenset[Killswitch]

    def admits(self, feature: Feature) -> bool:
        return feature in self.admitted

    def is_tripped(self, killswitch: Killswitch) -> bool:
        return killswitch in self.tripped


class ProfilePolicy(Struct, frozen=True):
    profile: RuntimeProfile
    eager_import: bool
    scratch_writable: bool
    emit_otel: bool
    lane_capacity: int
    gate: FeatureGate


PROFILE_POLICY: Final[Map[RuntimeProfile, ProfilePolicy]] = Map.of_seq([
    (RuntimeProfile.TOOL, ProfilePolicy(RuntimeProfile.TOOL, eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=8, gate=FeatureGate(admitted=frozenset({Feature.SECRET_MANAGER, Feature.KEYSTORE_PROBE, Feature.OUTBOUND_TRANSPORT}), tripped=frozenset()))),
    (RuntimeProfile.SIDECAR, ProfilePolicy(RuntimeProfile.SIDECAR, eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=16, gate=FeatureGate(admitted=frozenset({Feature.SECRET_MANAGER, Feature.KEYSTORE_PROBE, Feature.OUTBOUND_TRANSPORT}), tripped=frozenset()))),
    (RuntimeProfile.PACKAGE, ProfilePolicy(RuntimeProfile.PACKAGE, eager_import=False, scratch_writable=False, emit_otel=False, lane_capacity=4, gate=FeatureGate(admitted=frozenset({Feature.OUTBOUND_TRANSPORT}), tripped=frozenset({Killswitch.DISABLE_SECRET_MANAGER})))),
    (RuntimeProfile.TEST, ProfilePolicy(RuntimeProfile.TEST, eager_import=False, scratch_writable=True, emit_otel=False, lane_capacity=2, gate=FeatureGate(admitted=frozenset(), tripped=frozenset({Killswitch.DISABLE_OUTBOUND, Killswitch.DISABLE_SECRET_MANAGER})))),
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

    def admits(self, feature: Feature) -> bool:
        return self.policy.gate.admits(feature)

    def tripped(self, killswitch: Killswitch) -> bool:
        return self.policy.gate.is_tripped(killswitch)

    def attribute(self) -> dict[str, str]:
        match self.causal:
            case Option(tag="some", some=frame):
                return {"rasm.tenant": str(frame.tenant), "rasm.hlc": f"{frame.hlc.physical_ticks}.{frame.hlc.logical}"}
            case _:
                return {}
```

## [03]-[SETTINGS]

- Owner: `SettingsAdmission` — the one local settings source order over `pydantic-settings`, admitting init mapping, environment-backed fields, dotenv, and OS secret files into a frozen record that rejects unknown fields; the source precedence is fixed at the one `settings_customise_sources` override, the `file_secret_settings` `secrets_dir` tier the lowest-priority fallback. `SecretBoundary` — one `resolve(service, username) -> RuntimeRail[Option[str]]` facade composing the OS keystore as the top-priority source over the `secrets_dir`/`file_secret_settings` fallback tier, profile-gated by the `FeatureGate` row and lazy on the outbound leg, never a flag set and never an eager unattended probe.
- Entry: `SettingsAdmission.admit` returns the frozen settings record; after admission no package reads the process environment. `SecretBoundary.resolve` reads `keyring.get_credential(service, username)` first when the profile admits `Feature.KEYSTORE_PROBE`, routing the keystore call through the one `reliability/faults#FAULT` `boundary("resource", ...)` conversion (catching `errors.KeyringError`), falls back to the `secrets_dir`-mounted secret file at `{secrets_dir}/{service}_{username}` (read through the same `boundary("resource", ...)` conversion catching `OSError`, since a caller-dynamic `{service}_{username}` name cannot be a declared field on the frozen `extra='forbid'` model) on any caught keystore failure (the headless/container/locked case the catalogue names) or on an `Ok(None)` miss, and returns the resolved password on a present credential; an absent secret — a keystore miss with no mounted file — is `Ok(Nothing)` rather than a fault — a missing credential is a wire fact the outbound leg routes, not a boundary failure.
- Auto: precedence is init over env over dotenv over secret-file; the `env_prefix`/`env_nested_delimiter` map nested keys and `secrets_dir` resolves OS-mounted secret files (the `file_secret_settings` source); the `SecretBoundary` keystore read is bounded to a profile whose `FeatureGate` admits `Feature.KEYSTORE_PROBE` (`TOOL`/`SIDECAR`) — a `PACKAGE`/`TEST` profile never probes the keystore and resolves the secrets-file tier directly, so a session that cannot answer a keychain prompt never triggers one; `keyring.get_credential` returns `None` for a missing secret and the `credentials.Credential` protocol's `.password` carries the resolved value, the keystore call lifted through `boundary` so a `KeyringError` raise crosses the one conversion rather than an inline `try`/`except`, and the keystore read is lazy at the outbound transport leg, never at admission. The `Feature.SECRET_MANAGER` gate and the `Killswitch.DISABLE_SECRET_MANAGER` row are the forward-looking policy columns the deferred cloud-secret-manager tier resolves against; the cloud read itself is the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA, not a realized source row here.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`/`PydanticBaseSettingsSource`/`file_secret_settings`/`secrets_dir`), `pydantic`, `keyring` (`get_credential` ENTRYPOINTS [04] / `get_password` ENTRYPOINTS [01] facade, `errors.KeyringError` errors [01] base / `errors.NoKeyringError` errors [06] headless fallback, `credentials.Credential` credentials [01] protocol read via `.password`).
- Growth: a new setting is one typed field on the settings model; a new source is one entry in the `settings_customise_sources` tuple; a new secret-resolution tier is one branch on the `SecretBoundary.resolve` source chain gated by its `Feature` row, never a second config owner; zero new surface.
- Boundary: no package reads `os.environ` directly after admission; environment reads outside this owner, an eager unattended keystore probe (the named hazard — a keychain prompt the session cannot answer), a secret read outside the one settings-admitted `SecretBoundary`, an inline `try`/`except` around the keystore call beside the one `boundary` conversion, and a hand-rolled credential constructed beside the boundary are the deleted forms; the suite-wide configuration taxonomy stays AppHost-owned, and the multi-source `STRUCTURED_SETTINGS_SCHEMA` — including the cloud secret-manager tier and the `gcp_project_id`-backed `GoogleSecretManagerSettingsSource` chain row — stays the deferred IDEA, not realized here while `google-cloud-secret-manager` is unadmitted in the central manifest; this page admits only the source rows and the secret tiers the outbound legs read today. The keystore reads OS credentials for the OUTBOUND transport legs only (`transport/roots#RESOURCE` `TransportResource.ssh` passphrase, `http` bearer); the companion UDS serve leg reads no keyring (peer identity is the kernel accept-time credential, `transport/serve#SERVE`).

```python signature
from pathlib import Path

import keyring
from expression import Nothing, Ok, Option, Some
from keyring import errors
from msgspec import Struct
from pydantic_settings import (
    BaseSettings,
    PydanticBaseSettingsSource,
    SettingsConfigDict,
)

from rasm.runtime.admission import Feature, ProfilePolicy
from rasm.runtime.faults import RuntimeRail, boundary


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
        if not self.policy.gate.admits(Feature.KEYSTORE_PROBE):
            return Ok(self._from_secrets_file(service, username))
        probed = boundary("resource", lambda: keyring.get_credential(service, username), catch=errors.KeyringError)
        match probed:
            case Ok(credential) if credential is not None:
                return Ok(Some(credential.password))
            case _:
                return Ok(self._from_secrets_file(service, username))

    def _from_secrets_file(self, service: str, username: str) -> Option[str]:
        mount = Path(self.settings.model_config["secrets_dir"]) / f"{service}_{username}"
        read = boundary("resource", lambda: mount.read_text(encoding="utf-8"), catch=OSError)
        match read:
            case Ok(text):
                return Some(text.strip())
            case _:
                return Nothing
```

## [04]-[RESEARCH]

- [SETTINGS_SOURCE_ORDER]: the `settings_customise_sources` precedence (init over env over dotenv over secret-file), the `file_secret_settings` source class, and the `secrets_dir` `SettingsConfigDict` key are verified against the `pydantic-settings` catalogue; the source-class parameter spellings are settled. The cloud `GoogleSecretManagerSettingsSource` chain row (`.api/pydantic-settings.md` source [12]) is NOT admitted on this realized cell — its backing distribution `google-cloud-secret-manager` is unadmitted in the central manifest, so a verbatim fence importing it would `ImportError`; the GCP chain row and the `gcp_project_id` field stay the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA until the distribution lands as a runtime-core row, when it becomes one additional `settings_customise_sources` tuple entry above the secrets-file fallback per the catalogue source law.
- [PROFILE_FEATURE_GATE]: the feature/killswitch state is folded into `PROFILE_POLICY` as the `FeatureGate` column — an admitted-`Feature` set and a tripped-`Killswitch` set per profile row — so `RuntimeContext.admits`/`tripped` and `SecretBoundary.resolve` dispatch on membership rather than a boolean knob the caller re-derives; this realizes the local cell of `STRUCTURED_SETTINGS_SCHEMA` (the per-profile gate) while the broader multi-source structured schema stays the deferred IDEA, not over-built here. The `Feature.KEYSTORE_PROBE` row gates the realized keystore tier and the `Killswitch.DISABLE_OUTBOUND` row short-circuits the outbound leg; the `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER` columns are the forward-looking gate the deferred cloud-secret-manager tier resolves against, carried as real policy pressure though no realized source reads them yet. The `PACKAGE`/`TEST` profiles drop the keystore probe to the secrets-file tier so the unattended-probe hazard stays closed.
- [SECRET_BOUNDARY]: `keyring.get_credential(service, username) -> Credential | None` (ENTRYPOINTS [04]) is the structured top-priority read returning `None` for a missing secret, `keyring.get_password` (ENTRYPOINTS [01]) the string-only variant, `errors.KeyringError` (errors [01]) the base the `boundary("resource", ...)` lift catches and `errors.NoKeyringError` (errors [06]) the headless/container subclass the secrets-file tier absorbs, and `credentials.Credential` (credentials [01]) the protocol `get_credential` returns, the resolved password read via `.password` — spellings settled against the `keyring` catalogue. The keystore read is routed through the one `reliability/faults#FAULT` `boundary` conversion (no inline `try`/`except` in domain flow) and falls back to the `secrets_dir`-mounted secret file `{secrets_dir}/{service}_{username}` — read through the same `boundary("resource", ...)` conversion (catching `OSError`), folding a present file to `Some(text.strip())` and an absent/unreadable mount to `Nothing` — on any caught keystore failure or an `Ok(None)` miss, profile-gated by the `FeatureGate` row and lazy on the outbound leg; the caller-dynamic `{service}_{username}` secret name cannot be a declared field on the frozen `extra='forbid'` `SettingsAdmission` (only `scratch_root`/`object_store_root`/`otel_endpoint` are admitted), so the tier reads the OS-mounted file at the `secrets_dir` root the `file_secret_settings` source itself targets rather than a `getattr` the forbidding model can never satisfy; the unattended-probe hazard is closed by the `PACKAGE`/`TEST` short-circuit to the secrets-file tier. `keyring` moves from orphaned-catalogue (zero design-page consumer after `transport/serve#KEYRING_CATALOGUE` dropped the serve leg) to the one `execution/admission#SETTINGS` outbound-credential consumer.
- [CLOCK_CONSUMPTION]: `Hlc`/`CausalFrame`/`Tenant` source from `clock/clock#CLOCK` (R0) — admission imports `CausalFrame`/`Tenant` from `rasm.runtime.clock` and consumes the host-minted `Hlc` stamp through the `CausalFrame.hlc` field (`frame.hlc.physical_ticks`/`frame.hlc.logical` in `attribute`), naming the `Hlc` type only in prose so the import carries no unused symbol; admission re-spells none of them; the earlier `transport/serve` `Hlc` import and the in-page `Tenant`/`CausalFrame` redeclarations are the deleted forms, the clock owner being the single decode site and the C# `AppHost/Runtime` the single mint.
