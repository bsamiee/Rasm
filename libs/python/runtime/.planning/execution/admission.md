# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, classification, and the inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings` with the keystore-then-secrets-file secret-resolution boundary on top. `CausalFrame` is the `clock/clock#CLOCK` two-half owner — admission consumes the host-minted `Hlc` stamp and the one `Tenant` partition from `clock`, re-minting nothing and re-spelling neither; `SecretBoundary` resolves the OS-keystore credential the outbound transport legs (`transport/roots#RESOURCE`) read, profile-gated and lazy on the outbound leg, never an eager unattended probe. The package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, caches a global mutable context, re-mints a causal stamp or tenant scheme, or probes the keystore eagerly. Feature gating and killswitch state are not boolean knobs the caller re-derives — they ride the `PROFILE_POLICY` table as data-driven rows so a profile resolves its admitted-feature set and its tripped killswitches from the same policy lookup the eager-import and lane-capacity columns already answer.

## [01]-[INDEX]

- [01]-[CONTEXT]: profile, correlation, deadline, the inbound `CausalFrame`/`Tenant` consumed from `clock`, the per-profile feature/killswitch policy rows, the one caller-owned context admission.
- [02]-[SETTINGS]: the local settings source order over `pydantic-settings` (init/env/dotenv/secret-file), and the keystore-then-secrets-file `SecretBoundary`.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` — the one caller-supplied context owner discriminating profile/correlation/deadline/classification and carrying the inbound `causal` frame; `RuntimeProfile` the closed `StrEnum` vocabulary keying the `PROFILE_POLICY` row table; `Correlation` the W3C-shaped trace cell and `Deadline` the `seconds`-projecting budget value object it carries; `Hlc`, `CausalFrame`, and `Tenant` arrive from the `clock/clock#CLOCK` owner, never re-spelled here.
- Cases: `RuntimeProfile` rows `TOOL` · `SIDECAR` · `PACKAGE` · `TEST`, each the KEY of one `PROFILE_POLICY` persistent `Map` row carrying eager-import, scratch-writable, OTel-emit, lane-capacity, and the `FeatureGate` admitted-feature/killswitch columns — the profile is the key and the `ProfilePolicy` value carries no redundant `profile` field that could drift from its key, behavior travelling on the row rather than a flag the caller re-derives; `FeatureGate` is itself a data-driven cell — an admitted-`Feature` set and a tripped-`Killswitch` set on the leaf `gc=False` struct, so `RuntimeContext.admits`/`tripped` discriminate by membership rather than a parallel boolean field per capability; `RuntimeContext.causal` is `Option[CausalFrame]` — `Nothing` for a locally-minted context, `Some(frame)` for a context admitting the host-minted inbound stamp, exactly the `Option[Deadline]` carry already present; `Deadline` is a behaviour-carrying value object whose `seconds` projection is the one `anyio.fail_after` float the `execution/lanes#LANE` `LanePolicy.deadline` reads, never a re-derived `total_seconds()` at the lane seam; `Correlation.trace_id` is the W3C-shaped 16-byte trace identifier and `Correlation.parent` the `Option[bytes]` inbound parent the admitting frame seeds from the host stamp.
- Entry: `RuntimeContext.admit` receives caller-owned host facts plus an optional decoded `CausalFrame`, lifts each nullable through `Option.of_optional` rather than an inline ternary, and seeds `Correlation.parent` from the inbound frame's `Hlc.packed` 16-byte rendering so a context admitting the host stamp threads the host causal position as its trace parent rather than minting an orphan root; `RuntimeContext.policy` reads the per-profile row from the `Map` and `RuntimeContext.budget` projects the `Option[Deadline]` to the `Option[float]` lane seconds in one place; `RuntimeContext.admits(feature)`/`tripped(killswitch)` answer the gate by membership in the policy row's `FeatureGate` sets so a guarded path dispatches on data, not a re-derived boolean; `RuntimeContext.attribute` projects profile/trace/classification onto the one `dict[str, str | int]` attribute map every signal reads and folds the carried-frame columns in through one `causal.map(lambda f: base | f.attributes("packed")).default_value(base)` — the `(tenant, hlc)` columns are NOT re-spelled here: they are the `clock#CLOCK` `CausalFrame.attributes("packed")` projection (the canonical owner of both the `rasm.tenant`/`rasm.hlc` slot keys off the one `SLOTS` table and the fixed-width `032x` hex rendering of the `Hlc.packed` 128-bit value that keeps the attribute inside the OTLP signed-int64 bound a raw 128-bit int overflows), so the absent-frame branch reduces to the base map with no parallel codepath and the two-page attribute layout cannot drift; the resulting map is admissible to `Span.set_attributes` directly without a hand-rolled flattener; the owner never reads the environment and never resolves the host.
- Packages: `msgspec` (`Struct`/`field`/`gc=False` on the leaf `FeatureGate`/`Deadline` cells), `expression` (`Map`/`Option`/`Option.of_optional`), `secrets` (`token_bytes` minting the local trace root).
- Growth: a new context field is one column on `RuntimeContext`; a new profile is one `RuntimeProfile` row plus one `PROFILE_POLICY` entry keyed by it (no row-internal profile field to keep in sync); a new feature or killswitch is one `Feature`/`Killswitch` enum case plus its membership in the affected profile rows' `FeatureGate` sets, never a parallel boolean knob or a second flag owner; a new attribute dimension is one entry in the `attribute` projection map; the causal/tenant frame is the `clock`-owned `Option[CausalFrame]` column, never a parallel context record; zero new surface.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context; the C# `HostProfile` stays AppHost-owned and is never mirrored row-for-row; a sibling context record beside `RuntimeContext`, a second `Tenant` spelling (the raw `serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` is absorbed into the one `clock`-owned `Tenant` newtype), a re-minted `Hlc` stamp, an inline `{"rasm.tenant": ..., "rasm.hlc": ...}` causal-attribute map or a hand-rolled `format(packed, "032x")`/dotted `physical_ticks.logical` HLC rendering beside the canonical `clock#CLOCK` `CausalFrame.attributes("packed")` projection, a redundant `profile` field on the `ProfilePolicy` value beside the `Map` key, a bare-`total_seconds()` deadline conversion at the lane seam beside `Deadline.seconds`, an inline `Nothing if x is None else Some(x)` ternary beside `Option.of_optional`, and a boolean-per-capability feature flag beside the policy table are the deleted forms — `CausalFrame`/`Hlc`/`Tenant` are the `clock/clock#CLOCK` owner's records, the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` being the sole mint (single-mint invariant). The broader multi-source structured settings schema stays the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA — this page realizes only its local cell: the feature/killswitch rows folded into `PROFILE_POLICY`.

```python signature
from datetime import timedelta
from enum import StrEnum
from secrets import token_bytes
from typing import Final, Self

from expression import Nothing, Option
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


class FeatureGate(Struct, frozen=True, gc=False):
    admitted: frozenset[Feature]
    tripped: frozenset[Killswitch]

    def admits(self, feature: Feature) -> bool:
        return feature in self.admitted

    def is_tripped(self, killswitch: Killswitch) -> bool:
        return killswitch in self.tripped


class ProfilePolicy(Struct, frozen=True):
    eager_import: bool
    scratch_writable: bool
    emit_otel: bool
    lane_capacity: int
    gate: FeatureGate


PROFILE_POLICY: Final[Map[RuntimeProfile, ProfilePolicy]] = Map.of_seq([
    (RuntimeProfile.TOOL, ProfilePolicy(eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=8, gate=FeatureGate(admitted=frozenset({Feature.SECRET_MANAGER, Feature.KEYSTORE_PROBE, Feature.OUTBOUND_TRANSPORT}), tripped=frozenset()))),
    (RuntimeProfile.SIDECAR, ProfilePolicy(eager_import=True, scratch_writable=True, emit_otel=True, lane_capacity=16, gate=FeatureGate(admitted=frozenset({Feature.SECRET_MANAGER, Feature.KEYSTORE_PROBE, Feature.OUTBOUND_TRANSPORT}), tripped=frozenset()))),
    (RuntimeProfile.PACKAGE, ProfilePolicy(eager_import=False, scratch_writable=False, emit_otel=False, lane_capacity=4, gate=FeatureGate(admitted=frozenset({Feature.OUTBOUND_TRANSPORT}), tripped=frozenset({Killswitch.DISABLE_SECRET_MANAGER})))),
    (RuntimeProfile.TEST, ProfilePolicy(eager_import=False, scratch_writable=True, emit_otel=False, lane_capacity=2, gate=FeatureGate(admitted=frozenset(), tripped=frozenset({Killswitch.DISABLE_OUTBOUND, Killswitch.DISABLE_SECRET_MANAGER})))),
])


class Correlation(Struct, frozen=True):
    trace_id: bytes
    parent: Option[bytes] = Nothing

    @classmethod
    def mint(cls, *, parent: bytes | None = None) -> Self:
        return cls(trace_id=token_bytes(16), parent=Option.of_optional(parent))


class Deadline(Struct, frozen=True, gc=False):
    budget: timedelta

    @property
    def seconds(self) -> float:
        return self.budget.total_seconds()


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
        frame = Option.of_optional(causal)
        return cls(
            profile=profile,
            correlation=Correlation.mint(parent=frame.map(lambda f: f.hlc.packed.to_bytes(16, "big")).to_optional()),
            deadline=Option.of_optional(deadline),
            classification=classification,
            causal=frame,
        )

    @property
    def policy(self) -> ProfilePolicy:
        return PROFILE_POLICY[self.profile]

    @property
    def budget(self) -> Option[float]:
        return self.deadline.map(lambda d: d.seconds)

    def admits(self, feature: Feature) -> bool:
        return self.policy.gate.admits(feature)

    def tripped(self, killswitch: Killswitch) -> bool:
        return self.policy.gate.is_tripped(killswitch)

    def attribute(self) -> dict[str, str | int]:
        base: dict[str, str | int] = {"rasm.profile": self.profile.value, "rasm.trace_id": self.correlation.trace_id.hex(), "rasm.classification": self.classification}
        return self.causal.map(lambda frame: base | frame.attributes("packed")).default_value(base)
```

## [03]-[SETTINGS]

- Owner: `SettingsAdmission` — the one local settings model over `pydantic-settings`, admitting init mapping, environment-backed fields, dotenv, and OS secret files into a frozen `extra='forbid'` record over the `pydantic-settings` DEFAULT init>env>dotenv>secret-file precedence (no `settings_customise_sources` override, since restating the default order is ceremony the catalogue source-law deletes — an override exists only to permute), the roots typed precisely (`scratch_root: DirectoryPath`, `otel_endpoint: HttpUrl`) rather than bare `str`. `SecretSource`/`SECRET_SOURCES` — the data-driven secret-resolution ladder, each row an `Option[Feature]` gate (`Nothing` = ungated fallback, `Some(feature)` = profile-gated) and a `Probe` resolver, so the keystore tier and the secrets-file fallback are rows in one ordered `Block` rather than parallel methods. `SecretBoundary` — the `resolve(service, username) -> RuntimeRail[Option[str]]` facade folding `SECRET_SOURCES` to the first present credential plus the `known_hosts() -> RuntimeRail[asyncssh.SSHKnownHosts]` loader the `transport/roots#RESOURCE` `ssh` leg verifies against, both profile-gated by the `FeatureGate` row, lazy on the outbound leg, never a flag set and never an eager unattended probe.
- Entry: `SettingsAdmission()` runs the default `pydantic-settings` source chain at instantiation and returns the frozen record; after admission no package reads the process environment. `SecretBoundary.resolve` filters `SECRET_SOURCES` to the rows the profile admits (an ungated row always passes, a `Some(feature)` row passes only when the `FeatureGate` admits it) and folds them through `rail.bind(found -> Ok(found) if found.is_some() else row.probe(...))` so the ladder short-circuits on the first present credential and a tripped/absent gate drops its row from the fold rather than branching — the keystore `Probe` routes `keyring.get_credential(service, username)` through the one `reliability/faults#FAULT` `boundary("resource", ...)` conversion (catching `errors.KeyringError`) and lifts a present `Credential.password` to `Some`, and the ungated secrets-file `Probe` reads the `secrets_dir`-mounted `{secrets_dir}/{service}_{username}` through the same `boundary("resource", ...)` conversion (catching `OSError`, since a caller-dynamic `{service}_{username}` name cannot be a declared field on the frozen `extra='forbid'` model); an absent secret — every gated row dropped or missed with no mounted file — folds to `Ok(Nothing)` rather than a fault, a missing credential being a wire fact the outbound leg routes, not a boundary failure. `SecretBoundary.known_hosts` reads the configured `known_hosts` path (defaulting to `~/.ssh/known_hosts`) through `asyncssh.read_known_hosts` lifted on the same `boundary("resource", ...)` conversion, returning the verified `SSHKnownHosts` database the `transport/roots#RESOURCE` `ssh` connection law binds into `asyncssh.connect(..., known_hosts=...)`, so host-key verification is admission-supplied and never the disabled-verification `known_hosts=None` the connection law forbids.
- Auto: precedence is the `pydantic-settings` default init>env>dotenv>secret-file; the `env_prefix`/`env_nested_delimiter` map nested keys and `secrets_dir` resolves OS-mounted secret files (the `file_secret_settings` source); the `SECRET_SOURCES` fold drops the keystore row on any profile whose `FeatureGate` lacks `Feature.KEYSTORE_PROBE` (`PACKAGE`/`TEST`), so those profiles resolve the ungated secrets-file row directly and a session that cannot answer a keychain prompt never triggers one; `keyring.get_credential` returns `None` for a missing secret (lifted through `Option.of_optional`) and the `credentials.Credential` protocol's `.password` carries the resolved value, the keystore call lifted through `boundary` so a `KeyringError` raise crosses the one conversion rather than an inline `try`/`except`, and the keystore read is lazy at the outbound transport leg, never at admission. The `Feature.SECRET_MANAGER` gate and the `Killswitch.DISABLE_SECRET_MANAGER` row are the forward-looking policy columns the deferred cloud-secret-manager tier resolves against; the cloud read itself is the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA, realized as one additional `SecretSource(Some(Feature.SECRET_MANAGER), _cloud_probe)` row above the secrets-file fallback when `google-cloud-secret-manager` lands, not a realized row here.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`/`secrets_dir`/`file_secret_settings` default chain), `pydantic` (`DirectoryPath` PUBLIC_TYPES [14] / `HttpUrl` PUBLIC_TYPES [05] precise root types), `keyring` (`get_credential` ENTRYPOINTS [04] structured read returning `Credential | None`, `errors.KeyringError` errors [01] base / `errors.NoKeyringError` errors [06] headless subclass the secrets-file row absorbs, `credentials.Credential` credentials [01] protocol read via `.password`), `asyncssh` (`read_known_hosts` ENTRYPOINTS [02] / `SSHKnownHosts` PUBLIC_TYPES [04] the outbound `ssh` trust database), `expression` (`Block.of_seq`/`Block.filter`/`Block.fold`/`Option`/`Result.bind`).
- Growth: a new setting is one typed field on the settings model; a new source-precedence permutation is the one `settings_customise_sources` override returning a reordered tuple (absent until a reorder is actually needed); a new secret-resolution tier is one `SecretSource` row on `SECRET_SOURCES` carrying its `Option[Feature]` gate and `Probe`, the deferred cloud tier exactly that one row, never a second config owner or a parallel resolve method; zero new surface.
- Boundary: no package reads `os.environ` directly after admission; environment reads outside this owner, a no-op `settings_customise_sources` override restating the default order, an eager unattended keystore probe (the named hazard — a keychain prompt the session cannot answer), a secret read outside the one settings-admitted `SecretBoundary`, a parallel `_from_secrets_file`/`_from_keystore` method pair beside the one `SECRET_SOURCES` fold, an inline `try`/`except` around the keystore call beside the one `boundary` conversion, a bare-`str` filesystem-root or endpoint field beside the precise `DirectoryPath`/`HttpUrl` types, a disabled-verification `known_hosts=None` SSH trust beside the admission-loaded `SSHKnownHosts`, and a hand-rolled credential constructed beside the boundary are the deleted forms; the suite-wide configuration taxonomy stays AppHost-owned, and the multi-source `STRUCTURED_SETTINGS_SCHEMA` — including the cloud secret-manager tier and the `gcp_project_id`-backed `GoogleSecretManagerSettingsSource` chain row — stays the deferred IDEA while `google-cloud-secret-manager` is unadmitted in the central manifest; this page admits only the source rows and the secret tiers the outbound legs read today. The keystore reads OS credentials for the OUTBOUND transport legs only (`transport/roots#RESOURCE` `TransportResource.ssh` passphrase, `http` bearer); the companion UDS serve leg reads no keyring (peer identity is the kernel accept-time credential, `transport/serve#SERVE`).

```python signature
from collections.abc import Callable
from pathlib import Path
from typing import Final

import asyncssh
import keyring
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from keyring import errors
from msgspec import Struct
from pydantic import DirectoryPath, HttpUrl
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.admission import Feature, ProfilePolicy
from rasm.runtime.faults import RuntimeRail, boundary


class SettingsAdmission(BaseSettings):
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__", secrets_dir="/run/secrets")

    scratch_root: DirectoryPath
    object_store_root: str | None = None
    otel_endpoint: HttpUrl | None = None
    known_hosts: Path | None = None


type Probe = Callable[["SecretBoundary", str, str], RuntimeRail[Option[str]]]


class SecretSource(Struct, frozen=True):
    gate: Option[Feature]
    probe: Probe


def _keystore(_: "SecretBoundary", service: str, username: str) -> RuntimeRail[Option[str]]:
    probed = boundary("resource", lambda: keyring.get_credential(service, username), catch=errors.KeyringError)
    return Ok(probed.to_option().bind(Option.of_optional).map(lambda cred: cred.password))


def _secrets_file(boundary_owner: "SecretBoundary", service: str, username: str) -> RuntimeRail[Option[str]]:
    mount = Path(boundary_owner.settings.model_config["secrets_dir"]) / f"{service}_{username}"
    read = boundary("resource", lambda: mount.read_text(encoding="utf-8"), catch=OSError)
    return Ok(read.to_option().map(lambda text: text.strip()))


SECRET_SOURCES: Final[Block[SecretSource]] = Block.of_seq([
    SecretSource(Some(Feature.KEYSTORE_PROBE), _keystore),
    SecretSource(Nothing, _secrets_file),
])


class SecretBoundary(Struct, frozen=True):
    settings: SettingsAdmission
    policy: ProfilePolicy

    def resolve(self, service: str, username: str) -> RuntimeRail[Option[str]]:
        admitted = SECRET_SOURCES.filter(lambda row: row.gate.map(self.policy.gate.admits).default_value(True))
        return admitted.fold(lambda rail, row: rail.bind(lambda found: Ok(found) if found.is_some() else row.probe(self, service, username)), Ok(Nothing))

    def known_hosts(self) -> RuntimeRail[asyncssh.SSHKnownHosts]:
        return boundary("resource", lambda: asyncssh.read_known_hosts(str(self.settings.known_hosts or Path.home() / ".ssh" / "known_hosts")), catch=OSError)
```

## [04]-[RESEARCH]

- [SETTINGS_SOURCE_ORDER]: the realized model rides the `pydantic-settings` DEFAULT source precedence — init>env>dotenv>secret-file per the catalogue IMPLEMENTATION_LAW source-law — so NO `settings_customise_sources` override is authored: the catalogue states the default order explicitly and that "reorder by returning a permuted tuple" is the override's only purpose, making an override that returns the verbatim default pure ceremony, deleted here. The `file_secret_settings` `secrets_dir` tier (the lowest-priority fallback) and the `secrets_dir`/`env_prefix`/`env_nested_delimiter` `SettingsConfigDict` keys are verified against the catalogue; the roots are typed precisely against the `pydantic` catalogue — `scratch_root: DirectoryPath` (PUBLIC_TYPES [14], existence-checked directory) and `otel_endpoint: HttpUrl` (PUBLIC_TYPES [05], parsed URL value object) replacing the bare `str` forms so the model rejects a non-directory scratch root and a malformed endpoint at admission rather than at first use. The cloud `GoogleSecretManagerSettingsSource` chain row (`.api/pydantic-settings.md` source [12]) is NOT admitted on this realized cell — its backing distribution `google-cloud-secret-manager` is unadmitted in the central manifest, so a verbatim fence importing it would `ImportError`; the GCP chain row and the `gcp_project_id` field stay the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA until the distribution lands, when the cloud read becomes one `SecretSource(Some(Feature.SECRET_MANAGER), ...)` row above the secrets-file fallback rather than a settings-source-chain entry — the secret ladder is the `SECRET_SOURCES` fold, distinct from the `pydantic-settings` model-field source chain.
- [PROFILE_FEATURE_GATE]: the feature/killswitch state is folded into `PROFILE_POLICY` as the `FeatureGate` column — an admitted-`Feature` set and a tripped-`Killswitch` set per profile row — so `RuntimeContext.admits`/`tripped` and the `SecretBoundary.resolve` `SECRET_SOURCES` filter dispatch on membership rather than a boolean knob the caller re-derives; `ProfilePolicy` carries no `profile` field because the `Map[RuntimeProfile, ProfilePolicy]` key IS the profile, eliminating the key/field drift vector a duplicated discriminant introduces. This realizes the local cell of `STRUCTURED_SETTINGS_SCHEMA` (the per-profile gate) while the broader multi-source structured schema stays the deferred IDEA, not over-built here. The `Feature.KEYSTORE_PROBE` row gates the realized keystore `SecretSource` and the `Killswitch.DISABLE_OUTBOUND` row short-circuits the outbound leg; the `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER` columns are the forward-looking gate the deferred cloud-secret-manager `SecretSource` row resolves against, carried as real policy pressure though no realized source reads them yet. The `PACKAGE`/`TEST` profiles drop the keystore row from the `SECRET_SOURCES` fold to the ungated secrets-file row so the unattended-probe hazard stays closed.
- [SECRET_BOUNDARY]: the secret ladder is the data-driven `SECRET_SOURCES: Block[SecretSource]` table, each row an `Option[Feature]` gate and a `Probe` resolver — the realized table is the gated keystore row (`Some(Feature.KEYSTORE_PROBE)`, `_keystore`) over the ungated secrets-file fallback row (`Nothing`, `_secrets_file`), and `resolve` is one `Block.filter` (admitting an ungated row always and a `Some(feature)` row only when the `FeatureGate` admits it) folded through `rail.bind(found -> Ok(found) if found.is_some() else row.probe(...))` so the ladder short-circuits on the first present credential without a per-tier `if`/`else` chain — adding the deferred cloud tier is one row, never a new private method, so the Growth claim is literally the table shape. `keyring.get_credential(service, username) -> Credential | None` (ENTRYPOINTS [04]) is the structured keystore read returning `None` for a missing secret (lifted through `Option.of_optional` so the `Ok(None)` keystore miss folds to `Nothing` and falls through to the next row), `errors.KeyringError` (errors [01]) the base the `boundary("resource", ...)` lift catches and `errors.NoKeyringError` (errors [06]) the headless/container subclass the secrets-file row absorbs, and `credentials.Credential` (credentials [01]) the protocol `get_credential` returns, the resolved password read via `.password` — spellings settled against the `keyring` catalogue. Both probes route through the one `reliability/faults#FAULT` `boundary` conversion (no inline `try`/`except` in domain flow); the secrets-file probe reads the `secrets_dir`-mounted `{secrets_dir}/{service}_{username}` (catching `OSError`), folding a present file to `Some(text.strip())` and an absent/unreadable mount to `Ok(Nothing)` — the caller-dynamic `{service}_{username}` name cannot be a declared field on the frozen `extra='forbid'` `SettingsAdmission` (only `scratch_root`/`object_store_root`/`otel_endpoint`/`known_hosts` are admitted), so the row reads the OS-mounted file at the `secrets_dir` root the `file_secret_settings` source itself targets rather than a `getattr` the forbidding model can never satisfy. `SecretBoundary.known_hosts` is the second realized leg — `asyncssh.read_known_hosts(filelist) -> SSHKnownHosts` (`.api/asyncssh.md` ENTRYPOINTS [02]) lifted on the same `boundary("resource", ...)` conversion over the configured `known_hosts` path (defaulting to `~/.ssh/known_hosts`), returning the `SSHKnownHosts` (PUBLIC_TYPES [04]) trust database the `transport/roots#RESOURCE` `ssh` connection law binds into `asyncssh.connect(..., known_hosts=...)`, closing the seam where the consumer named admission as the host-key-database owner but admission exposed only the credential resolve. `keyring` is the one `execution/admission#SETTINGS` outbound-credential consumer (orphaned-catalogue after `transport/serve#KEYRING_CATALOGUE` dropped the serve leg).
- [CLOCK_CONSUMPTION]: `Hlc`/`CausalFrame`/`Tenant` source from `clock/clock#CLOCK` (R0) — admission imports `CausalFrame`/`Tenant` from `rasm.runtime.clock` and consumes the host-minted `Hlc` stamp through the `CausalFrame.hlc` field, projecting the carried `(tenant, hlc)` onto the span/receipt/metric attribute map by composing the canonical `CausalFrame.attributes("packed")` projection (clock `[02]-[CLOCK]`) — admission re-spells NOTHING: the `rasm.tenant`/`rasm.hlc` slot keys ride the clock owner's one `SLOTS` table and the fixed-width `032x` hex of the `Hlc.packed` 128-bit value is the clock owner's `packed`-shape rendering, not a re-derived `format(packed, "032x")` here. The clock `[ATTRIBUTE_PROJECTION]` row names this admission inline map the deleted form that collapses INTO `frame.attributes("packed")`; this page is that collapse — the prior `format(...)`/`f"{physical_ticks}.{logical}"` admission spelling diverged from the clock owner's rendering and is deleted, and selecting the `"packed"` shape (over `"halves"`) keeps the single-int HLC inside the OTLP signed-int64 bound a raw 128-bit int would overflow while the two halves the metric attributer needs ride the `"halves"` shape on its own page — one projector, two output shapes, no per-page map. `Correlation.trace_id` carries a 16-byte W3C-shaped identifier and `RuntimeContext.admit` seeds `Correlation.parent` from the inbound frame's `Hlc.packed.to_bytes(16, "big")` so a context admitting the host stamp threads the host causal position as its trace parent — `Correlation` and `CausalFrame` stay distinct concepts (the `Correlation` the `observability/receipts#RECEIPT` `merge_contextvars` bound-context spine, the `CausalFrame` the host causal stamp) but the admit path now links them rather than minting an orphan parentless root. Admission re-spells none of the clock records; the earlier `transport/serve` `Hlc` import and the in-page `Tenant`/`CausalFrame` redeclarations are the deleted forms, the clock owner being the single decode site and the C# `AppHost/Runtime` the single mint.
