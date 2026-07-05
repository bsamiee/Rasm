# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, classification, and the inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings` with the data-driven `SECRET_LADDER` secret-resolution table on top. `CausalFrame`, `Hlc`, and `Tenant` arrive from the `clock/clock#CLOCK` two-half owner: admission consumes the host-minted stamp and the one partition, re-minting nothing and re-spelling neither. `SecretBoundary.resolve` is the one credential reader the outbound transport legs (`transport/roots#RESOURCE`) consume, parameterized over output shape (`SecretShape.TOKEN` -> `Option[SecretStr]`, `SecretShape.CREDENTIAL` -> `Option[BasicCredential]`) so the SSH-password and the HTTP-`BasicAuth` consumers read one entrypoint rather than two methods, profile-gated and lazy on the outbound leg, never an eager unattended probe. Each ladder tier — the OS keystore, the gated GCP Secret Manager read over the admitted `google-cloud-secret-manager` client, and the mounted-file fallback — folds through the `reliability/resilience#RESILIENCE` `guarded` retried-traced-railed envelope so a transiently-locked keystore or a transiently-unreachable Secret Manager retries under one `stamina` policy row inside one derivation span rather than failing the resolve on the first `KeyringLocked`.

Feature gating and killswitch state are data-driven rows on `PROFILE_POLICY`, never boolean knobs the caller re-derives: a profile resolves its admitted-feature set and its tripped killswitches from the same lookup the eager-import and lane-capacity columns answer. The package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, caches a global mutable context, re-mints a causal stamp or tenant scheme, or probes the keystore eagerly. Resolved secrets cross as `pydantic` `SecretStr`, never a bare `str` a receipt or log egress could serialize.

## [01]-[INDEX]

- [01]-[CONTEXT]: profile, correlation, deadline, the inbound `CausalFrame`/`Tenant` consumed from `clock`, the per-profile feature/killswitch policy rows, the one caller-owned context admission.
- [02]-[SETTINGS]: the local settings source order over `pydantic-settings` (init/env/dotenv/secret-file), the parameterized `secrets_mount`/`gcp_project_id` roots, the `SecretTier` ADT (`keystore`/`cloud`/`file`, all three realized) and its `SECRET_LADDER` resolution table, and the output-parameterized `SecretBoundary.resolve` fused with the `reliability/resilience#RESILIENCE` `guarded` envelope.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` — the one caller-supplied context owner discriminating profile/correlation/deadline/classification and carrying the inbound `causal` frame; `RuntimeProfile` the closed `StrEnum` vocabulary keying the `PROFILE_POLICY` row table; `Correlation` the W3C-shaped trace cell and `Deadline` the `seconds`-projecting budget value object it carries; `Hlc`, `CausalFrame`, and `Tenant` arrive from the `clock/clock#CLOCK` owner, never re-spelled here.
- Cases:
  - `RuntimeProfile` rows `TOOL` · `SIDECAR` · `PACKAGE` · `TEST` each KEY one `PROFILE_POLICY` persistent `Map` row carrying the eager-import, scratch-writable, OTel-emit, lane-capacity, and `FeatureGate` columns. The profile is the key, so the `ProfilePolicy` value carries no redundant `profile` field that could drift; behavior travels on the row, not a flag the caller re-derives.
  - `FeatureGate` is a data-driven cell — an admitted-`Feature` set and a tripped-`Killswitch` set on the leaf `gc=False` struct — and `admits` folds BOTH axes through the one `KILLSWITCH_FEATURE` disabling-edge table: a feature is live iff it is in `admitted` and no tripped killswitch revokes it. A killswitch can never be dead policy a row ignores, and `RuntimeContext.admits` answers effective availability by membership rather than a parallel boolean field per capability the caller ANDs by hand.
  - `RuntimeContext.causal` is `Option[CausalFrame]` — `Nothing` for a locally-minted context, `Some(frame)` for a context admitting the host-minted inbound stamp — exactly the `Option[Deadline]` carry already present.
  - `Deadline` is a behavior-carrying value object whose `seconds` projection is the one `float` the `execution/lanes#LANE` `LanePolicy.deadline: Option[float]` reads into its `move_on_after(self.deadline.default_value(float("inf")))` scope, never a re-derived `total_seconds()` at the lane seam.
  - `Correlation.trace_id` is the W3C-shaped 16-byte trace identifier; `Correlation.parent` is the `Option[bytes]` inbound parent. `Correlation.seed(frame)` is the one parent-derivation classmethod folding the `Option[CausalFrame]` to a minted `Correlation` whose parent un-hexes the host stamp's canonical `clock#CLOCK` `CausalFrame.attributes("packed")` `rasm.hlc` `032x` rendering — `Nothing` mints an orphan root, `Some(frame)` threads the host causal position — so the frame-to-parent projection is one named owner, never an inline `frame.map(lambda f: bytes.fromhex(...)).to_optional()` lambda repeated at the `admit` call site beside a `Hlc.packed.to_bytes(16, ...)` re-packing the clock owner forbids.
- Entry:
  - `RuntimeContext.admit` receives caller-owned host facts plus an optional decoded `CausalFrame`, lifts each nullable through `Option.of_optional` rather than an inline ternary, and delegates the trace-parent seed to `Correlation.seed(frame)`. A context admitting the host stamp threads the host causal position as its trace parent rather than minting an orphan root; the packing semantics stay the `clock#CLOCK` owner's, admission consuming the one rendering through the one `seed` classmethod, never re-deriving a `to_bytes(16, ...)` pack or re-spelling the un-hex at the call site.
  - `RuntimeContext.policy` reads the per-profile row from the `Map`, and `RuntimeContext.budget` projects the `Option[Deadline]` to the `Option[float]` lane seconds in one place.
  - `RuntimeContext.admits(feature)` answers EFFECTIVE availability (admitted-and-not-killswitched, folded through `KILLSWITCH_FEATURE`) and `tripped(killswitch)` the raw trip state, both by membership in the row's `FeatureGate` sets, so a guarded path dispatches on data, never a re-derived boolean it must AND.
  - `RuntimeContext.attribute` projects profile/trace/classification onto the one `dict[str, str | int]` attribute map every signal reads, folding the carried-frame columns through `causal.map(lambda f: base | f.attributes("packed")).default_value(base)`. The `(rasm.tenant, rasm.hlc)` columns are NOT re-spelled here — they are the `clock#CLOCK` `CausalFrame.attributes("packed")` projection, owner of both the `SLOTS`-table slot keys and the fixed-width `032x` hex of the `Hlc.packed` 128-bit value that keeps the attribute inside the OTLP signed-int64 bound a raw 128-bit int overflows. The absent-frame branch reduces to `base` with no parallel codepath, and the result is admissible to `Span.set_attributes` directly without a hand-rolled flattener.
- Packages: `msgspec` (`Struct`/`field`/`gc=False` on the leaf `FeatureGate`/`Deadline` cells), `expression` (`Map`/`Option`/`Option.of_optional`), `secrets` (`token_bytes` minting the local trace root).
- Growth: a new context field is one column on `RuntimeContext`; a new profile is one `RuntimeProfile` row plus one `PROFILE_POLICY` entry keyed by it (no row-internal profile field to keep in sync); a new feature is one `Feature` case plus its membership in the affected profile rows' `admitted` set, and a new killswitch is one `Killswitch` case plus one `KILLSWITCH_FEATURE` disabling-edge row plus its membership in the affected `tripped` sets, never a parallel boolean knob or a second flag owner; a new attribute dimension is one entry in the `attribute` projection map; the causal/tenant frame is the `clock`-owned `Option[CausalFrame]` column, never a parallel context record; zero new surface.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context. The C# `HostProfile` stays AppHost-owned, never mirrored row-for-row; `CausalFrame`/`Hlc`/`Tenant` are the `clock/clock#CLOCK` owner's records, the C# `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` the sole mint (single-mint invariant). The deleted forms are:
  - a sibling context record beside `RuntimeContext`, or a second `Tenant` spelling — the raw `serve#CAPABILITY_INVOKE` `CommandArguments.tenant: str` is absorbed into the one `clock`-owned `Tenant` newtype.
  - a re-minted `Hlc` stamp, an inline `{"rasm.tenant": ..., "rasm.hlc": ...}` causal-attribute map, or a hand-rolled `format(packed, "032x")`/dotted `physical_ticks.logical` HLC rendering beside the canonical `clock#CLOCK` `CausalFrame.attributes("packed")` projection.
  - a `Hlc.packed.to_bytes(16, ...)` re-packing of the parent-id beside the un-hexed canonical rendering (the packing semantics are the clock owner's, not admission's to duplicate); an inline `frame.map(lambda f: bytes.fromhex(...)).to_optional()` parent-seed lambda at the `admit` call site beside the one `Correlation.seed(frame)` classmethod.
  - a redundant `profile` field on the `ProfilePolicy` value beside the `Map` key; a bare-`total_seconds()` deadline conversion at the lane seam beside `Deadline.seconds`; an inline `Nothing if x is None else Some(x)` ternary beside `Option.of_optional`.
  - a boolean-per-capability feature flag beside the policy table, or a killswitch left as dead policy a row never folds — a tripped switch must revoke its `KILLSWITCH_FEATURE`-paired feature in the one `admits` fold, never a separate predicate the caller forgets to AND.
- The broader multi-source structured settings schema stays the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA; this page realizes only its local cell, the feature/killswitch rows folded into `PROFILE_POLICY`.

```python signature
from datetime import timedelta
from enum import StrEnum
from secrets import token_bytes
from typing import Final, Self

from expression import Nothing, Option
from expression.collections import Map
from msgspec import Struct, field

from rasm.runtime.clock import CausalFrame


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


# the one disabling-edge table: each killswitch names the feature it revokes, so a tripped
# switch and an admitted feature meet in one `admits` fold rather than two predicates a
# caller must remember to AND — a new killswitch is one row, never a parallel boolean knob.
KILLSWITCH_FEATURE: Final[Map[Killswitch, Feature]] = Map.of_seq([
    (Killswitch.DISABLE_OUTBOUND, Feature.OUTBOUND_TRANSPORT),
    (Killswitch.DISABLE_SECRET_MANAGER, Feature.SECRET_MANAGER),
])


class FeatureGate(Struct, frozen=True, gc=False):
    admitted: frozenset[Feature]
    tripped: frozenset[Killswitch]

    def admits(self, feature: Feature) -> bool:
        # effective availability folds BOTH axes: the feature is in the admitted set AND no
        # tripped killswitch revokes it, so a killswitch is never dead policy a row ignores.
        return feature in self.admitted and not any(KILLSWITCH_FEATURE[k] is feature for k in self.tripped)

    def is_tripped(self, killswitch: Killswitch) -> bool:
        return killswitch in self.tripped


class ProfilePolicy(Struct, frozen=True):
    eager_import: bool
    scratch_writable: bool
    emit_otel: bool
    lane_capacity: int
    gate: FeatureGate


PROFILE_POLICY: Final[Map[RuntimeProfile, ProfilePolicy]] = Map.of_seq([
    (
        RuntimeProfile.TOOL,
        ProfilePolicy(
            eager_import=True,
            scratch_writable=True,
            emit_otel=True,
            lane_capacity=8,
            gate=FeatureGate(admitted=frozenset({Feature.SECRET_MANAGER, Feature.KEYSTORE_PROBE, Feature.OUTBOUND_TRANSPORT}), tripped=frozenset()),
        ),
    ),
    (
        RuntimeProfile.SIDECAR,
        ProfilePolicy(
            eager_import=True,
            scratch_writable=True,
            emit_otel=True,
            lane_capacity=16,
            gate=FeatureGate(admitted=frozenset({Feature.SECRET_MANAGER, Feature.KEYSTORE_PROBE, Feature.OUTBOUND_TRANSPORT}), tripped=frozenset()),
        ),
    ),
    (
        RuntimeProfile.PACKAGE,
        ProfilePolicy(
            eager_import=False,
            scratch_writable=False,
            emit_otel=False,
            lane_capacity=4,
            gate=FeatureGate(admitted=frozenset({Feature.OUTBOUND_TRANSPORT}), tripped=frozenset({Killswitch.DISABLE_SECRET_MANAGER})),
        ),
    ),
    (
        RuntimeProfile.TEST,
        ProfilePolicy(
            eager_import=False,
            scratch_writable=True,
            emit_otel=False,
            lane_capacity=2,
            gate=FeatureGate(admitted=frozenset(), tripped=frozenset({Killswitch.DISABLE_OUTBOUND, Killswitch.DISABLE_SECRET_MANAGER})),
        ),
    ),
])


class Correlation(Struct, frozen=True):
    trace_id: bytes
    parent: Option[bytes] = Nothing

    @classmethod
    def mint(cls, *, parent: bytes | None = None) -> Self:
        return cls(trace_id=token_bytes(16), parent=Option.of_optional(parent))

    @classmethod
    def seed(cls, frame: Option[CausalFrame]) -> Self:
        # the one parent-derivation: un-hex the clock owner's canonical packed `rasm.hlc`
        # `032x` rendering to the 16-byte W3C identity; an absent frame mints an orphan root.
        return cls.mint(parent=frame.map(lambda f: bytes.fromhex(f.attributes("packed")["rasm.hlc"])).to_optional())


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
            profile=profile, correlation=Correlation.seed(frame), deadline=Option.of_optional(deadline), classification=classification, causal=frame
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
        base: dict[str, str | int] = {
            "rasm.profile": self.profile.value,
            "rasm.trace_id": self.correlation.trace_id.hex(),
            "rasm.classification": self.classification,
        }
        return self.causal.map(lambda frame: base | frame.attributes("packed")).default_value(base)
```

## [03]-[SETTINGS]

- Owner: `SettingsAdmission` is the one local settings model over `pydantic-settings`, admitting init mapping, environment-backed fields, dotenv, and OS secret files into a frozen `extra='forbid'` record over the DEFAULT `init>env>dotenv>secret-file` precedence. No `settings_customise_sources` override exists: restating the default order is ceremony the catalogue source-law deletes, and an override exists only to permute or to admit a new origin. Every root is typed against the `pydantic` catalogue, never bare `str`: `scratch_root: DirectoryPath`, `object_store_root: AnyUrl | None` (scheme-checked URL), `otel_endpoint: HttpUrl | None`, `known_hosts: FilePath | None` (existence-checked file), the parameterized `secrets_mount: Path` (defaulting to the one `_SECRETS_MOUNT` anchor the `secrets_dir` source target shares — a deployment override threads the paired `secrets_mount=`/`_secrets_dir=` constructor pair so the field and the `file_secret_settings` source never split), and `gcp_project_id: str | None` (the cloud-tier admission root) — the pydantic-edge `| None` lifted to `Option` at the read site per the boundary law, never an `expression.Option` field pydantic-core cannot schema. `SecretShape` is the closed output vocabulary the resolve parameterizes over, `SecretTier` the closed resolution ADT, and `BasicCredential` the `(username, SecretStr)` value object the `CREDENTIAL` shape yields — `BasicCredential` because the wire-axis `CredentialPolicy` union is `transport/serve#SERVE`'s decode of the C#-minted axis, a different concept under a different name.
- `SecretTier`/`SECRET_LADDER` is the data-driven secret-resolution ladder: `SecretTier` is the `@tagged_union` over `keystore`/`cloud`/`file` — all three cases REALIZED — and `SECRET_LADDER: Block[TierRow]` is the one ordered table of `(SecretTier, Option[Feature] gate, RetryClass)` rows, keystore over cloud over file, each carrying its profile gate (`Nothing` ungated fallback, `Some(feature)` profile-gated) and the landed `reliability/resilience#RESILIENCE` `RetryClass.SECRET` policy row its `guarded` envelope retries under. The probe behavior rides one `match row.tier` fold inside `_probe` — the tier IS the discriminant, a new tier is one case plus one row, and the cloud tier is that growth law's realized proof: one `cloud` case whose payload names the Secret Manager namespace, one `TierRow` gated by `Some(Feature.SECRET_MANAGER)`, zero new resolver functions.
- `SecretBoundary` carries the admitted `SettingsAdmission` and the resolved `FeatureGate`, exposing `resolve(service, username=None, shape=SecretShape.TOKEN)` parameterized over BOTH input AND output through `@overload` on the `SecretShape` literal: `TOKEN` types `RuntimeRail[Option[SecretStr]]` — the shape the realized `transport/roots#RESOURCE` legs read, the `ssh` passphrase and the `http` `_BearerAuth` bearer — and `CREDENTIAL` types `RuntimeRail[Option[BasicCredential]]`, the `httpx.BasicAuth(username, password)` two-part leg. One entrypoint discriminates both shapes; admitting the BASIC consumer is one `@overload` arm, never a parallel resolver. The `username: str | None` admits the service-scoped bearer leg: `None` resolves the keystore's backend-default user, and the returned `credentials.Credential.username` is the authoritative identity read back off the store, never the request value re-stamped. `known_hosts() -> RuntimeRail[asyncssh.SSHKnownHosts]` is the loader the `transport/roots#RESOURCE` `ssh` leg verifies against. All gated by the carried `gate`, lazy on the outbound leg, never an eager unattended probe.
- Entry: `SettingsAdmission()` runs the default `pydantic-settings` source chain at instantiation and returns the frozen record; after admission no package reads the process environment. `SecretBoundary.resolve` filters `SECRET_LADDER` to the rows the carried `gate` admits — an ungated `Nothing` row always passes, a `Some(feature)` row only when `FeatureGate.admits` holds — then threads the depth-bounded async `walk` over the closed-tier ladder (the synchronous `Block.fold` cannot await the per-tier `guarded` envelope, and the row count is the closed `SecretTier` arity, so the head-then-tail recursion is bounded without a trampoline): short-circuit on the first probe `Error`, return the first present `BasicCredential`, fold the empty ladder to `Ok(Nothing)`. `_probe` is the one resilience-fused tier resolver — each `match row.tier` arm owns its complete `guarded(row.retry_class, anyio.to_thread.run_sync, <tier-read>, subject="secret")` so the blocking read offloads to the `anyio` worker pool AND a transient retries under the tier's `stamina` row inside one derivation span, the terminal raise lifting once through the resilience owner's `async_boundary`. The `keystore` arm routes `keyring.get_credential(service, username)` and lifts a present `credentials.Credential` to `BasicCredential(c.username, SecretStr(c.password))`. The `cloud` arm is REAL over the admitted `google-cloud-secret-manager` client: it reads `_secret_client().access_secret_version(name=secret_version_path(project, f"{namespace}-{name}", "latest")).payload.data` where `project` is the `gcp_project_id` root (an absent project folds the arm to `Ok(Nothing)` with no client construction) and `name` is the same `_secret_name(service, username)` fold the file tier reads — one naming correspondence, two mounts; a `NotFound` version is a MISS swallowed to `Nothing` (the keyring `MISSING_VS_ERROR` law applied to the cloud store) so the fold falls through to `file`, while a transport fault raises into `guarded`'s `RetryClass.SECRET` retry. The `file` arm reads `settings.secrets_mount / _secret_name(...)`, folding a present file to `Some(BasicCredential(...))` and an absent mount to `Nothing`. The shape projection is the fold tail: `CREDENTIAL` returns the `Option[BasicCredential]` directly, `TOKEN` projects `cred.map(lambda c: c.secret)`. An absent secret — every gated row dropped or missed — folds to `Ok(Nothing)` rather than a fault, a missing credential being a wire fact the outbound leg routes.
- `SecretBoundary.known_hosts` reads the configured `known_hosts` path (defaulting to `~/.ssh/known_hosts`) through `asyncssh.read_known_hosts` lifted on `reliability/faults#FAULT` `boundary("resource", ...)` — a one-shot startup read, not a transient-retry leg — returning the verified `SSHKnownHosts` database the `transport/roots#RESOURCE` `ssh` connection law binds into `asyncssh.connect(..., known_hosts=...)`. Host-key verification is admission-supplied, never the disabled-verification `known_hosts=None` the connection law forbids.
- Auto: precedence is the `pydantic-settings` default `init>env>dotenv>secret-file`; `env_prefix`/`env_nested_delimiter` map nested keys and `secrets_dir` resolves OS-mounted secret files through the `file_secret_settings` source. The ladder fold drops the keystore row on any profile whose `FeatureGate` lacks `Feature.KEYSTORE_PROBE` and the cloud row wherever `Feature.SECRET_MANAGER` is unadmitted or `Killswitch.DISABLE_SECRET_MANAGER` trips (`PACKAGE`/`TEST` resolve the ungated file row directly), so a session that cannot answer a keychain prompt never triggers one and a killswitched deployment never dials GCP. The cloud client mints once per process through the `functools.cache`-held `_secret_client` — ADC-resolved (workload identity, metadata server, or `GOOGLE_APPLICATION_CREDENTIALS`), `from_service_account_file` when a deployment pins a key path — and both google imports are module-scope `lazy from` bindings, so the cold gRPC client stack costs nothing until the gated arm first fires; the response's `SecretPayload.data_crc32c` transport-integrity field rides the message the client verifies at its own boundary. The declared-field twin of this caller-dynamic ladder is the branch-catalogued `GoogleSecretManagerSettingsSource` (`secret_client=`-injected with this same client) — the settings-source chain row a deployment adds when declared model fields, not per-service credentials, live in Secret Manager; this page's cloud slice of the structured-settings concern lands here, the remote-config remainder stays deferred.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`/`secrets_dir`/`file_secret_settings` default chain; `GoogleSecretManagerSettingsSource` PUBLIC_TYPES [13] the declared-field cloud chain row this client backs), `pydantic` (`DirectoryPath`/`FilePath`/`AnyUrl`/`HttpUrl` precise roots, `SecretStr` the repr-hiding carrier read via `.get_secret_value()` only at the transport seam), `google-cloud-secret-manager` (`SecretManagerServiceClient` construct [01] ADC / `from_service_account_file` [02] / `secret_version_path(project, secret, version)` [03] / `access_secret_version(name=)` read [01] -> `AccessSecretVersionResponse.payload` [02] `SecretPayload.data`+`data_crc32c` [03]; `google.api_core.exceptions.NotFound` the miss seam), `keyring` (`get_credential` ENTRYPOINTS [04] returning `Credential | None`, `errors.KeyringLocked` errors [02] the retried transient, `errors.NoKeyringError` errors [06] the headless miss the keystore arm floors to `Nothing`, `credentials.Credential` credentials [01]), `asyncssh` (`read_known_hosts` ENTRYPOINTS [02] / `SSHKnownHosts` PUBLIC_TYPES [04]), `anyio` (`to_thread.run_sync` the worker-pool offload every blocking tier read rides), `expression` (`Block.of_seq`/`Block.filter`/`Block.try_head`/`Block.tail`/`Option.of_optional`/`Option.map`/`Option.default_value`/`Result.map`/`tagged_union`/`case`/`tag`), `reliability/resilience#RESILIENCE` (`guarded(cls, fn, *args, subject=...)` the retried-traced-railed tier envelope, the landed `RetryClass.SECRET` row every `TierRow` binds).
- Growth: a new setting is one typed field on the settings model; a new source-precedence permutation or source origin is the one `settings_customise_sources` override (absent until actually needed — the `GoogleSecretManagerSettingsSource` chain row is that override's first real tenant when declared-field cloud settings land); a new secret-resolution tier is one `SecretTier` case plus one `TierRow` carrying its `Option[Feature]` gate and `RetryClass` — the cloud tier is exactly that diff, landed; a new resolved output shape is one `SecretShape` member plus one `@overload` and one fold-tail arm; zero new surface.
- Boundary: no package reads `os.environ` directly after admission. The keystore and cloud tiers read credentials for the OUTBOUND transport legs only; the companion UDS serve leg reads no keyring, peer identity being the kernel accept-time credential (`transport/serve#SERVE`). The deleted forms are:
  - environment reads outside this owner; a no-op `settings_customise_sources` override restating the default order.
  - an eager unattended keystore probe, an eager cloud dial at admission, or a secret read outside the one settings-admitted `SecretBoundary`; a module-scope eager google import beside the `lazy from` boundary; a per-call client construction beside the `functools.cache`-held `_secret_client`.
  - a `Probe` callable type plus parallel free resolver functions beside the one `SecretTier`-discriminated `_probe` fold; a rail-replacing `try`/`except` or a bare `boundary("resource", ...)` around a tier read beside the one `guarded` envelope — distinct from the two narrow miss seams (`except NoKeyringError` in `keystore_read`, `except NotFound` in `cloud_read`) that reclassify the no-backend/no-version signal as the `MISSING_VS_ERROR` `Nothing` floor BEFORE `guarded` sees it, never a general trap that swallows a transient the row must retry.
  - a bare-`str` filesystem-root, store-root, or endpoint field beside the precise `DirectoryPath`/`AnyUrl`/`HttpUrl`/`FilePath` types; a bare-`str` resolved secret beside the repr-hiding `SecretStr`; a disabled-verification `known_hosts=None` SSH trust beside the admission-loaded `SSHKnownHosts`; a second `/run/secrets` literal beside the one `_SECRETS_MOUNT` anchor the field default and the `secrets_dir` config both read; a `model_config.get("secrets_dir")` reach where the `secrets_mount` field IS the read surface.
  - two parallel `resolve_token`/`resolve_credential` methods beside the one `@overload`-parameterized `resolve(service, username, shape)`; a second `Credential`-named type beside `BasicCredential` (the serve-side `CredentialPolicy` union is the wire axis, not this value object); a direct `access_secret_version` call outside the gated `cloud` arm.
- The suite-wide configuration taxonomy stays AppHost-owned. The cloud-secret slice of the structured-settings concern is REALIZED here — the gated cloud tier, the `gcp_project_id` root, and the parameterized mount; only the multi-source remote-config remainder of `STRUCTURED_SETTINGS_SCHEMA` stays the deferred IDEA.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from functools import cache
from pathlib import Path
from typing import Final, Literal, assert_never, overload

import anyio
import asyncssh
import keyring
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from pydantic import AnyUrl, DirectoryPath, FilePath, HttpUrl, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guarded

# cold cloud dependency: `lazy` binds defer the gRPC client stack to the gated arm's first fire.
lazy from google.api_core.exceptions import NotFound
lazy from google.cloud.secretmanager import SecretManagerServiceClient

# `Feature`/`FeatureGate` are the [02]-[CONTEXT] owners of this same `rasm.runtime.admission`
# module — no cross-module import, the two fences are one module's two declaration regions.

# --- [TYPES] ----------------------------------------------------------------------------


class SecretShape(StrEnum):
    TOKEN = "token"  # the bare passphrase/bearer the SSH/HTTP legs read off `.get_secret_value()`
    CREDENTIAL = "credential"  # the (username, secret) pair the `httpx.BasicAuth` leg reads


@tagged_union(frozen=True)
class SecretTier:
    # the discriminant IS the resolver: a new tier is one case plus one `SECRET_LADDER` row,
    # never a `Probe` callable type plus a parallel free probe function. `cloud` carries the
    # Secret Manager namespace prefix its secret ids mint under.
    tag: Literal["keystore", "cloud", "file"] = tag()
    keystore: bool = case()
    cloud: str = case()
    file: bool = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# the one default OS secret-mount anchor: the `secrets_dir` source target and the
# `secrets_mount` field default both read it; a deployment override threads the paired
# `secrets_mount=` field + `_secrets_dir=` constructor kwarg so the two never split.
_SECRETS_MOUNT: Final[str] = "/run/secrets"

# --- [MODELS] ---------------------------------------------------------------------------


class BasicCredential(Struct, frozen=True):
    username: str
    secret: SecretStr


class SettingsAdmission(BaseSettings):
    # the pydantic edge: fields carry pydantic-native types and `| None`, lifted to `Option`
    # at the read site per the boundary law — never an `expression.Option` field
    # pydantic-core cannot build a core schema for.
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__", secrets_dir=_SECRETS_MOUNT)

    scratch_root: DirectoryPath
    object_store_root: AnyUrl | None = None
    otel_endpoint: HttpUrl | None = None
    known_hosts: FilePath | None = None
    secrets_mount: Path = Path(_SECRETS_MOUNT)
    gcp_project_id: str | None = None


class TierRow(Struct, frozen=True):
    tier: SecretTier
    gate: Option[Feature]
    retry_class: RetryClass


class SecretBoundary(Struct, frozen=True):
    settings: SettingsAdmission
    gate: FeatureGate

    @overload
    async def resolve(self, service: str, username: str | None = ..., shape: Literal[SecretShape.TOKEN] = ...) -> RuntimeRail[Option[SecretStr]]: ...
    @overload
    async def resolve(
        self, service: str, username: str | None = ..., *, shape: Literal[SecretShape.CREDENTIAL]
    ) -> RuntimeRail[Option[BasicCredential]]: ...
    async def resolve(
        self, service: str, username: str | None = None, shape: SecretShape = SecretShape.TOKEN
    ) -> RuntimeRail[Option[SecretStr]] | RuntimeRail[Option[BasicCredential]]:
        # `username=None` is the service-scoped bearer leg the keystore resolves to its backend-default
        # user; the resolved `BasicCredential.username` is read back off the store, never re-stamped.
        admitted = SECRET_LADDER.filter(lambda row: row.gate.map(self.gate.admits).default_value(True))

        async def walk(rows: Block[TierRow]) -> RuntimeRail[Option[BasicCredential]]:
            # the synchronous `Block.fold` cannot thread the per-tier `guarded` await, so the closed
            # `SecretTier`-arity-bounded ladder recurses head-then-tail; the empty ladder folds to `Ok(Nothing)`.
            match rows.try_head():
                case Some(head):
                    match await self._probe(head, service, username):
                        case Error(_) as faulted:
                            return faulted
                        case Ok(Some(_)) as hit:
                            return hit
                        case _:
                            return await walk(rows.tail())
                case _:
                    return Ok(Nothing)

        resolved = await walk(admitted)
        return resolved if shape is SecretShape.CREDENTIAL else resolved.map(lambda cred: cred.map(lambda c: c.secret))

    async def _probe(self, row: TierRow, service: str, username: str | None) -> RuntimeRail[Option[BasicCredential]]:
        # each tier rides ONE resilience envelope: the blocking read offloads to the anyio worker
        # pool and a transient retries under the row's `stamina` policy inside one span, the
        # terminal raise lifting once — never a bare `boundary` failing the resolve on the first
        # transient. Each arm owns its complete `await guarded(...)`.
        match row.tier:
            case SecretTier(tag="keystore"):

                def keystore_read() -> Option[BasicCredential]:
                    # `NoKeyringError` (no viable backend, headless/container) is a MISS floored to
                    # `Nothing` so the fold falls through — never retried, never a terminal Error.
                    try:
                        found = keyring.get_credential(service, username)
                    except keyring.errors.NoKeyringError:
                        return Nothing
                    return Option.of_optional(found).map(lambda c: BasicCredential(c.username, SecretStr(c.password)))

                return await guarded(row.retry_class, anyio.to_thread.run_sync, keystore_read, subject="secret")
            case SecretTier(tag="cloud", cloud=namespace):
                match Option.of_optional(self.settings.gcp_project_id):
                    case Some(project):

                        def cloud_read() -> Option[BasicCredential]:
                            # a missing secret version is a MISS (`NotFound` -> `Nothing`, the keyring
                            # missing-vs-error law on the cloud store); transport faults raise into the
                            # `RetryClass.SECRET` retry.
                            client = _secret_client()
                            name = client.secret_version_path(project, f"{namespace}-{_secret_name(service, username)}", "latest")
                            try:
                                payload = client.access_secret_version(name=name).payload
                            except NotFound:
                                return Nothing
                            return Some(BasicCredential(username or service, SecretStr(payload.data.decode("utf-8").strip())))

                        return await guarded(row.retry_class, anyio.to_thread.run_sync, cloud_read, subject="secret")
                    case _:
                        # no admitted project root: the arm folds to a miss, no client constructed.
                        return Ok(Nothing)
            case SecretTier(tag="file"):

                def file_read() -> Option[BasicCredential]:
                    path = self.settings.secrets_mount / _secret_name(service, username)
                    return (
                        Some(BasicCredential(username or service, SecretStr(path.read_text(encoding="utf-8").strip()))) if path.exists() else Nothing
                    )

                return await guarded(row.retry_class, anyio.to_thread.run_sync, file_read, subject="secret")
            case _ as unreachable:
                assert_never(unreachable)

    def known_hosts(self) -> RuntimeRail[asyncssh.SSHKnownHosts]:
        path = Option.of_optional(self.settings.known_hosts).map(str).default_value(str(Path.home() / ".ssh" / "known_hosts"))
        return boundary("resource", lambda: asyncssh.read_known_hosts(path), catch=OSError)


# --- [OPERATIONS] -----------------------------------------------------------------------


# the one secret-naming correspondence the file mount and the cloud namespace both read.
def _secret_name(service: str, username: str | None) -> str:
    return Option.of_optional(username).map(lambda u: f"{service}_{u}").default_value(service)


# ADC-resolved once per process (workload identity, metadata server, or
# GOOGLE_APPLICATION_CREDENTIALS); a pinned key path rides `from_service_account_file`.
@cache
def _secret_client() -> SecretManagerServiceClient:
    return SecretManagerServiceClient()


# --- [TABLES] ---------------------------------------------------------------------------

# keystore over cloud over file: each row binds its profile gate and the landed
# `RetryClass.SECRET` resilience row (`KeyringLocked`/`OSError` transients under one backoff);
# the cloud row is live only where `Feature.SECRET_MANAGER` admits AND `gcp_project_id` is set.
SECRET_LADDER: Final[Block[TierRow]] = Block.of_seq([
    TierRow(SecretTier(keystore=True), Some(Feature.KEYSTORE_PROBE), RetryClass.SECRET),
    TierRow(SecretTier(cloud="rasm"), Some(Feature.SECRET_MANAGER), RetryClass.SECRET),
    TierRow(SecretTier(file=True), Nothing, RetryClass.SECRET),
])
```
