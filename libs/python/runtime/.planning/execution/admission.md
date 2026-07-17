# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission: one immutable `RuntimeContext` carries the profile, correlation, deadline, classification, and inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local settings source order; `SecretBoundary.resolve` is the one credential reader the outbound `transport/roots#RESOURCE` legs consume — output-parameterized, profile-gated, and lazy on the outbound leg, never an eager unattended probe.

`CausalFrame`, `Hlc`, and `Tenant` arrive from the `clock/clock#CLOCK` owner — admission consumes the host-minted stamp and re-mints nothing. Each `SECRET_LADDER` tier folds through the `reliability/resilience#RESILIENCE` `guarded` envelope under the `RetryClass.SECRET` row, so a transiently-locked keystore or unreachable Secret Manager retries inside one derivation span rather than failing the resolve. Feature gating and killswitch state are data rows on `PROFILE_POLICY`, never boolean knobs the caller re-derives. This package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, or caches a global mutable context, and a resolved secret crosses as `SecretStr`, never a bare `str` a receipt or log egress serializes.

## [01]-[INDEX]

- [01]-[CONTEXT]: the caller-owned `RuntimeContext` with the clock-consumed causal carry and the `PROFILE_POLICY` feature/killswitch rows.
- [02]-[SETTINGS]: the `pydantic-settings` admission, the `SECRET_LADDER` tier table, and the output-parameterized `SecretBoundary.resolve`.

## [02]-[CONTEXT]

- Owner: `RuntimeContext` is the one caller-supplied context discriminating profile, correlation, deadline, and classification, carrying the inbound `causal` frame as `Option[CausalFrame]` — `Nothing` locally minted, `Some(frame)` the host stamp. `PROFILE_POLICY` keys behavior on the profile row, so the `ProfilePolicy` value carries no redundant `profile` field to drift and a caller never re-derives a flag.
- Entry: `Deadline.seconds` is the one `float` the `execution/lanes#LANE` `LanePolicy.deadline` reads — never a re-derived `total_seconds()` at the lane seam. `Correlation.seed(frame)` is the one parent-derivation owner: the packing semantics stay the `clock/clock#CLOCK` owner's, and `attribute` folds the carried frame through `CausalFrame.attributes("packed")` rather than re-spelling the `(rasm.tenant, rasm.hlc)` columns, so the result is admissible to `Span.set_attributes` directly.
- Growth: a new context field is one `RuntimeContext` column; a new profile one `RuntimeProfile` member plus one `PROFILE_POLICY` row; a new feature one `Feature` case plus its membership in the affected rows' `admitted` sets; a new killswitch one `Killswitch` case plus one `KILLSWITCH_FEATURE` disabling edge plus its `tripped` memberships — never a parallel boolean knob; a new attribute dimension one entry in the `attribute` projection.
- Boundary: no environment probing, profile resolution, service-root construction, or global mutable context lives here. `HostProfile` stays AppHost-owned, never mirrored row-for-row, and `CausalFrame`/`Hlc`/`Tenant` stay the `clock/clock#CLOCK` owner's records.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import timedelta
from enum import StrEnum
from secrets import token_bytes
from typing import Final, Self

from expression import Nothing, Option
from expression.collections import Map
from msgspec import Struct, field

from rasm.runtime.clock import CausalFrame

# --- [TYPES] ----------------------------------------------------------------------------


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


# --- [CONSTANTS] ------------------------------------------------------------------------

# each killswitch names the feature it revokes, so a tripped switch and an admitted feature meet in one `admits` fold rather than two
# predicates a caller must remember to AND.
KILLSWITCH_FEATURE: Final[Map[Killswitch, Feature]] = Map.of_seq([
    (Killswitch.DISABLE_OUTBOUND, Feature.OUTBOUND_TRANSPORT),
    (Killswitch.DISABLE_SECRET_MANAGER, Feature.SECRET_MANAGER),
])

# --- [MODELS] ---------------------------------------------------------------------------


class FeatureGate(Struct, frozen=True, gc=False):
    admitted: frozenset[Feature]
    tripped: frozenset[Killswitch]

    def admits(self, feature: Feature) -> bool:
        # effective availability folds BOTH axes — admitted AND not revoked — so a killswitch is never dead policy a row ignores.
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
        # un-hex the clock owner's canonical packed `rasm.hlc` rendering to the 16-byte W3C identity; an absent frame mints an orphan root.
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

- Owner: `SettingsAdmission` admits init mapping, environment, dotenv, and OS secret files over the DEFAULT `pydantic-settings` precedence — no `settings_customise_sources` override exists, because restating the default order is ceremony and an override is earned only by a permutation or a new origin. Every root is typed against the `pydantic` catalogue, never bare `str`. `BasicCredential` is deliberately not named `Credential`: the serve-side `CredentialPolicy` union is `transport/serve#SERVE`'s decode of the C#-minted wire axis, a different concept under a different name.
- Entry: `SecretBoundary.resolve` is the one credential reader, parameterized over output shape by `@overload` — admitting a new consumer shape is one `SecretShape` member plus one overload arm plus one fold-tail arm, never a parallel resolver. An absent secret folds to `Ok(Nothing)` rather than a fault: a missing credential is a wire fact the outbound leg routes. `known_hosts` returns the admission-loaded `SSHKnownHosts` the `transport/roots#RESOURCE` `ssh` leg binds — host-key verification is admission-supplied, never the disabled-verification `known_hosts=None` the connection law forbids.
- Auto: the ladder fold drops every row the carried `FeatureGate` refuses, so a session that cannot answer a keychain prompt never triggers one and a killswitched deployment never dials GCP. Its declared-field twin is the branch-catalogued `GoogleSecretManagerSettingsSource` injected with this same cached client — the settings-source chain row a deployment adds when declared model fields, not per-service credentials, live in Secret Manager.
- Growth: a new setting is one typed field on the model; a new source origin or precedence permutation is the one `settings_customise_sources` override, absent until needed; a new secret-resolution tier is one `SecretTier` case plus one `SECRET_LADDER` row carrying its `Option[Feature]` gate and `RetryClass`; a new output shape one `SecretShape` member plus one overload and one fold-tail arm.
- Boundary: no package reads `os.environ` after admission. Keystore and cloud tiers read credentials for the OUTBOUND transport legs only — the companion UDS serve leg reads no keyring, peer identity being the kernel accept-time credential (`transport/serve#SERVE`). Multi-source remote-config work rides the `STRUCTURED_SETTINGS_SCHEMA` idea card.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from functools import cache
from pathlib import Path
from typing import Final, Literal, assert_never, overload

import anyio
import asyncssh
import keyring
from expression import Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from pydantic import AnyUrl, DirectoryPath, FilePath, HttpUrl, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guarded

# cold cloud dependencies: `lazy` binds defer the gRPC client stack and the crc32c digest to the gated arm's first fire.
lazy import google_crc32c
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
    # discriminant IS the resolver — never a `Probe` callable type plus parallel free probe functions; `cloud` carries the Secret
    # Manager namespace prefix its secret ids mint under.
    tag: Literal["keystore", "cloud", "file"] = tag()
    keystore: bool = case()
    cloud: str = case()
    file: bool = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# one secret-mount anchor the `secrets_dir` source target and the `secrets_mount` field default both read; a deployment override
# threads the paired `secrets_mount=` field + `_secrets_dir=` kwarg so the two never split.
_SECRETS_MOUNT: Final[str] = "/run/secrets"

# secret-probe thread bound: this tier sits below the lanes bands, so it carries its own explicit limiter sized just past the
# ladder depth, bounding every concurrent resolve without leaning on the ambient default limiter.
_PROBE_BAND: Final[anyio.CapacityLimiter] = anyio.CapacityLimiter(4)

# --- [MODELS] ---------------------------------------------------------------------------


class BasicCredential(Struct, frozen=True):
    username: str
    secret: SecretStr


class SettingsAdmission(BaseSettings):
    # pydantic's edge carries pydantic-native types and `| None`, lifted to `Option` at the read site — never an `expression.Option`
    # field pydantic-core cannot build a core schema for.
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
        # `username=None` resolves the keystore's backend-default user; the resolved `BasicCredential.username` is read back off the store, never re-stamped.
        admitted = SECRET_LADDER.filter(lambda row: row.gate.map(self.gate.admits).default_value(True))

        async def walk(rows: Block[TierRow]) -> RuntimeRail[Option[BasicCredential]]:
            # `Block.fold` cannot await the per-tier `guarded`, so the closed-arity ladder recurses head-then-tail, the empty ladder
            # folding to `Ok(Nothing)`; the arms match the carrier tag, never a class pattern — `Ok`/`Error`/`Some` are constructor functions.
            match rows.try_head():
                case Option(tag="some", some=head):
                    match await self._probe(head, service, username):
                        case Result(tag="error") as faulted:
                            return faulted
                        case Result(tag="ok", ok=Option(tag="some")) as hit:
                            return hit
                        case _:
                            return await walk(rows.tail())
                case _:
                    return Ok(Nothing)

        resolved = await walk(admitted)
        return resolved if shape is SecretShape.CREDENTIAL else resolved.map(lambda cred: cred.map(lambda c: c.secret))

    async def _probe(self, row: TierRow, service: str, username: str | None) -> RuntimeRail[Option[BasicCredential]]:
        # each tier arm owns its complete `guarded(...)` envelope — the blocking read offloads to the anyio worker pool, a transient
        # retries under the row's policy inside one span, the terminal raise lifts once — never a bare `boundary` failing on the first transient.
        match row.tier:
            case SecretTier(tag="keystore"):

                def keystore_read() -> Option[BasicCredential]:
                    # `NoKeyringError` (no viable backend, headless) is a MISS floored to `Nothing` — never retried, never a terminal Error.
                    try:
                        found = keyring.get_credential(service, username)
                    except keyring.errors.NoKeyringError:
                        return Nothing
                    return Option.of_optional(found).map(lambda c: BasicCredential(c.username, SecretStr(c.password)))

                return await guarded(row.retry_class, anyio.to_thread.run_sync, keystore_read, subject="secret", limiter=_PROBE_BAND)
            case SecretTier(tag="cloud", cloud=namespace):
                match Option.of_optional(self.settings.gcp_project_id):
                    case Option(tag="some", some=project):

                        def cloud_read() -> Option[BasicCredential]:
                            # `NotFound` is a MISS (the keyring missing-vs-error law on the cloud store); transport faults raise into the retry.
                            client = _secret_client()
                            name = client.secret_version_path(project, f"{namespace}-{_secret_name(service, username)}", "latest")
                            try:
                                payload = client.access_secret_version(name=name).payload
                            except NotFound:
                                return Nothing
                            if google_crc32c.value(payload.data) != payload.data_crc32c:
                                # Secret Manager's client does NOT self-verify `data_crc32c`: a mismatch is corrupted transport — a retryable
                                # `OSError` transient, never a MISS and never a silently-trusted payload.
                                raise OSError(f"secret-crc32c:{name}")
                            return Some(BasicCredential(username or service, SecretStr(payload.data.decode("utf-8").strip())))

                        return await guarded(row.retry_class, anyio.to_thread.run_sync, cloud_read, subject="secret", limiter=_PROBE_BAND)
                    case _:
                        # no admitted project root: the arm folds to a miss, no client constructed.
                        return Ok(Nothing)
            case SecretTier(tag="file"):

                def file_read() -> Option[BasicCredential]:
                    path = self.settings.secrets_mount / _secret_name(service, username)
                    return (
                        Some(BasicCredential(username or service, SecretStr(path.read_text(encoding="utf-8").strip()))) if path.exists() else Nothing
                    )

                return await guarded(row.retry_class, anyio.to_thread.run_sync, file_read, subject="secret", limiter=_PROBE_BAND)
            case _ as unreachable:
                assert_never(unreachable)

    def known_hosts(self) -> RuntimeRail[asyncssh.SSHKnownHosts]:
        path = Option.of_optional(self.settings.known_hosts).map(str).default_value(str(Path.home() / ".ssh" / "known_hosts"))
        return boundary("resource", lambda: asyncssh.read_known_hosts(path), catch=OSError)


# --- [OPERATIONS] -----------------------------------------------------------------------


# one secret-naming correspondence the file mount and the cloud namespace both read.
def _secret_name(service: str, username: str | None) -> str:
    return Option.of_optional(username).map(lambda u: f"{service}_{u}").default_value(service)


# ADC-resolved once per process; a pinned key path rides `from_service_account_file`.
@cache
def _secret_client() -> SecretManagerServiceClient:
    return SecretManagerServiceClient()


# --- [TABLES] ---------------------------------------------------------------------------

# keystore over cloud over file: each row binds its profile gate and the `RetryClass.SECRET` row (`KeyringLocked`/`OSError` transients
# under one backoff); the cloud row is live only where `Feature.SECRET_MANAGER` admits AND `gcp_project_id` is set.
SECRET_LADDER: Final[Block[TierRow]] = Block.of_seq([
    TierRow(SecretTier(keystore=True), Some(Feature.KEYSTORE_PROBE), RetryClass.SECRET),
    TierRow(SecretTier(cloud="rasm"), Some(Feature.SECRET_MANAGER), RetryClass.SECRET),
    TierRow(SecretTier(file=True), Nothing, RetryClass.SECRET),
])
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
