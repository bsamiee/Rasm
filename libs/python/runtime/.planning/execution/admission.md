# [PY_RUNTIME_ADMISSION]

Caller-owned context and settings admission. One immutable `RuntimeContext` carries the profile, correlation, deadline, classification, and the inbound `CausalFrame` a caller supplies; one `SettingsAdmission` owns the local source order over `pydantic-settings` with the data-driven `SECRET_LADDER` secret-resolution table on top. `CausalFrame`, `Hlc`, and `Tenant` arrive from the `clock/clock#CLOCK` two-half owner: admission consumes the host-minted stamp and the one partition, re-minting nothing and re-spelling neither. `SecretBoundary.resolve` is the one credential reader the outbound transport legs (`transport/roots#RESOURCE`) consume, parameterized over output shape (`SecretShape.TOKEN` -> `Option[SecretStr]`, `SecretShape.CREDENTIAL` -> `Option[Credential]`) so the SSH-password and the HTTP-`BasicAuth` consumers read one entrypoint rather than two methods, profile-gated and lazy on the outbound leg, never an eager unattended probe. Each ladder tier folds through the `reliability/resilience#RESILIENCE` `guarded` retried-traced-railed envelope so a transiently-locked keystore retries under one `stamina` policy row inside one derivation span rather than failing the resolve on the first `KeyringLocked`.

Feature gating and killswitch state are data-driven rows on `PROFILE_POLICY`, never boolean knobs the caller re-derives: a profile resolves its admitted-feature set and its tripped killswitches from the same lookup the eager-import and lane-capacity columns answer. The package never discovers the host, starts services, owns lifecycle, derives product roots, reads the environment after admission, caches a global mutable context, re-mints a causal stamp or tenant scheme, or probes the keystore eagerly. Resolved secrets cross as `pydantic` `SecretStr`, never a bare `str` a receipt or log egress could serialize.

## [01]-[INDEX]

- [01]-[CONTEXT]: profile, correlation, deadline, the inbound `CausalFrame`/`Tenant` consumed from `clock`, the per-profile feature/killswitch policy rows, the one caller-owned context admission.
- [02]-[SETTINGS]: the local settings source order over `pydantic-settings` (init/env/dotenv/secret-file), the `SecretTier` ADT and its `SECRET_LADDER` resolution table, and the output-parameterized `SecretBoundary.resolve` fused with the `reliability/resilience#RESILIENCE` `guarded` envelope.

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
        return cls(profile=profile, correlation=Correlation.seed(frame), deadline=Option.of_optional(deadline), classification=classification, causal=frame)

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

- Owner: `SettingsAdmission` is the one local settings model over `pydantic-settings`, admitting init mapping, environment-backed fields, dotenv, and OS secret files into a frozen `extra='forbid'` record over the DEFAULT `init>env>dotenv>secret-file` precedence. No `settings_customise_sources` override exists: restating the default order is ceremony the catalogue source-law deletes, and an override exists only to permute. Every root is typed against the `pydantic` catalogue, never bare `str`: `scratch_root: DirectoryPath`, `object_store_root: AnyUrl | None` (scheme-checked URL, not a stringly store path), `otel_endpoint: HttpUrl | None`, `known_hosts: FilePath | None` (existence-checked file) — the pydantic-edge `| None` lifted to `Option` at the read site per the boundary law, never an `expression.Option` field pydantic-core cannot schema. `SecretShape` is the closed output vocabulary the resolve parameterizes over, `SecretTier` the closed resolution ADT, and `Credential` the `(username, SecretStr)` value object the `CREDENTIAL` shape yields.
- `SecretTier`/`SECRET_LADDER` is the data-driven secret-resolution ladder. `SecretTier` is the `@tagged_union` over `keystore`/`file`/`cloud` cases (the `cloud` case the forward-looking deferred tier), and `SECRET_LADDER: Block[TierRow]` is the one ordered table of `(SecretTier, Option[Feature] gate, RetryClass)` rows — `keystore` over `file`, each carrying its profile gate (`Nothing` ungated fallback, `Some(feature)` profile-gated) and the `reliability/resilience#RESILIENCE` `RetryClass.SECRET` policy row its `guarded` envelope retries under. `RetryClass.SECRET` is the one cross-file addition this page REQUIRES on the resilience owner: a new `RetryClass.SECRET = "secret"` member plus one `POLICY.add("secret", Policy(attempts=3, timeout=10.0, target=(keyring.errors.KeyringLocked, OSError)))` row whose transient `target` tuple is the keystore/file transients a reused `SSH`/`OBJECT_STORE` row would never catch — not a knob threaded through `guarded`. The probe behavior rides one `match self.tier` fold inside `_probe`, never a `Probe` callable type plus two free `_keystore`/`_secrets_file` functions: the tier IS the discriminant and a new tier is one case plus one row, the lanes `ADMIT_TABLE`/`AdmitRow` collapse shape applied to the secret ladder.
- `SecretBoundary` carries the admitted `SettingsAdmission` and the resolved `FeatureGate`, exposing `resolve(service, username=None, shape=SecretShape.TOKEN) -> RuntimeRail[Option[SecretStr]] | RuntimeRail[Option[Credential]]` parameterized over BOTH input AND output through `@overload` on the `SecretShape` literal. `TOKEN` types `RuntimeRail[Option[SecretStr]]` — the shape the realized `transport/roots#RESOURCE` legs read today, the `ssh` `password=` passphrase and the `http` `_BearerAuth` bearer (`httpx` ships no bearer `Auth`, only `BasicAuth`/`DigestAuth`/`NetRCAuth`, so the bearer is a custom `auth_flow` over the bare `SecretStr`). `CREDENTIAL` types `RuntimeRail[Option[Credential]]` — the forward output the `httpx.BasicAuth(username, password)` two-part leg reads when an `http` BASIC artifact lands, the (username, secret) pair the bearer/passphrase legs do not need. One entrypoint discriminates both shapes rather than two methods, so admitting the BASIC consumer is one `@overload` arm, not a parallel resolver. The `username: str | None` admits the service-scoped bearer leg (no user): `None` resolves the keystore's backend-default user, and the returned `credentials.Credential.username` is the authoritative identity read back off the store, never the request value re-stamped. `known_hosts() -> RuntimeRail[asyncssh.SSHKnownHosts]` is the loader the `transport/roots#RESOURCE` `ssh` leg verifies against. All gated by the carried `gate`, lazy on the outbound leg, never a flag set and never an eager unattended probe.
- Entry: `SettingsAdmission()` runs the default `pydantic-settings` source chain at instantiation and returns the frozen record; after admission no package reads the process environment. `SecretBoundary.resolve` filters `SECRET_LADDER.filter` to the rows the carried `gate` admits — an ungated `Nothing` row always passes, a `Some(feature)` row only when `FeatureGate.admits` holds, so a tripped or absent gate drops its row from the ladder rather than branching at the probe. The filtered ladder threads through the depth-bounded async `walk` over the closed-tier ladder (the synchronous `Block.fold` cannot await the per-tier `guarded` envelope, and the ladder's row count is the closed `SecretTier` arity, so the recursion is bounded without a `tailrec_async` trampoline): `walk` short-circuits on the first probe `Error`, returns the first present `Credential`, recurses on `rows.tail()` otherwise, and folds the empty ladder to `Ok(Nothing)`. `_probe` is the one resilience-fused tier resolver: `match row.tier` dispatches each arm to its own complete `reliability/resilience#RESILIENCE` `guarded(row.retry_class, anyio.to_thread.run_sync, <tier-read>, subject="secret")`, so the blocking keystore/file read offloads to the `anyio` worker pool (never stalling the loop) AND a transiently-locked keystore (`KeyringLocked`) retries under the tier's `stamina` row inside one derivation span, the terminal raise lifting once through the resilience owner's `async_boundary` — never a bare `boundary("resource", ...)` that fails the resolve on the first transient. The `keystore` tier routes `keyring.get_credential(service, username)` and lifts a present `credentials.Credential` to the `Credential(username, SecretStr(cred.password))` value object; the `file` tier reads the mounted `{root}/{service}_{username}` where `root` is the one `SECRETS_MOUNT` anchor (the `secrets_dir` the `SettingsConfigDict` fixes), the `Path(root) / ...` construction inside the thunk so the caller-dynamic `{service}_{username}` name cannot be a declared field on the frozen `extra='forbid'` model, the read folding to `Credential(username, SecretStr(text.strip()))`. The shape projection is the fold tail: `CREDENTIAL` returns the `Option[Credential]` directly, `TOKEN` projects `cred.map(lambda c: c.secret)` to `Option[SecretStr]`. An absent secret — every gated row dropped or missed with no mounted file — folds to `Ok(Nothing)` rather than a fault, a missing credential being a wire fact the outbound leg routes, not a boundary failure.
- `SecretBoundary.known_hosts` reads the configured `known_hosts` path (defaulting to `~/.ssh/known_hosts`) through `asyncssh.read_known_hosts` lifted on `reliability/faults#FAULT` `boundary("resource", ...)` (the host-key DB load is a one-shot startup read, not a transient-retry leg), returning the verified `SSHKnownHosts` database the `transport/roots#RESOURCE` `ssh` connection law binds into `asyncssh.connect(..., known_hosts=...)`. Host-key verification is admission-supplied, never the disabled-verification `known_hosts=None` the connection law forbids.
- Auto: precedence is the `pydantic-settings` default `init>env>dotenv>secret-file`; `env_prefix`/`env_nested_delimiter` map nested keys and `secrets_dir` resolves OS-mounted secret files through the `file_secret_settings` source. The `SECRET_LADDER` fold drops the keystore row on any profile whose `FeatureGate` lacks `Feature.KEYSTORE_PROBE` (`PACKAGE`/`TEST`), so those profiles resolve the ungated file row directly and a session that cannot answer a keychain prompt never triggers one. `keyring.get_credential` returns `None` for a missing secret (lifted through `Option.of_optional`) and the `credentials.Credential` protocol's `.username`/`.password` carry the resolved identity and value, the call riding the `guarded` envelope so a `KeyringError` raise crosses the resilience owner's one `async_boundary` rather than an inline `try`/`except`; the keystore read is lazy at the outbound transport leg, never at admission. The `Feature.SECRET_MANAGER` gate and the `Killswitch.DISABLE_SECRET_MANAGER` row are the forward-looking columns the deferred cloud-secret-manager tier resolves against — the `SecretTier.cloud` case and one `TierRow(SecretTier(cloud=...), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` row above the file fallback when `google-cloud-secret-manager` lands, the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA, not a realized row here.
- Packages: `pydantic-settings` (`BaseSettings`/`SettingsConfigDict`/`secrets_dir`/`file_secret_settings` default chain), `pydantic` (`DirectoryPath` PUBLIC_TYPES [14] / `FilePath` PUBLIC_TYPES [14] / `AnyUrl` PUBLIC_TYPES [05] / `HttpUrl` PUBLIC_TYPES [05] precise root types, `SecretStr` PUBLIC_TYPES [09] the repr/serialization-hiding secret carrier read via `.get_secret_value()` only at the transport seam), `keyring` (`get_credential` ENTRYPOINTS [04] structured read returning `Credential | None`, `errors.KeyringError` errors [01] base / `errors.KeyringLocked` errors [02] the transient the `guarded` retry catches / `errors.NoKeyringError` errors [06] headless subclass the file row absorbs, `credentials.Credential` credentials [01] protocol read via `.username`/`.password`), `asyncssh` (`read_known_hosts` ENTRYPOINTS [02] / `SSHKnownHosts` PUBLIC_TYPES [04] the outbound `ssh` trust database), `anyio` (`to_thread.run_sync` ENTRYPOINTS [01] the worker-pool offload the blocking keystore/file read rides so the probe never stalls the loop), `expression` (`Block.of_seq`/`Block.filter`/`Block.try_head`/`Block.tail`/`Option.of_optional`/`Option.map`/`Option.default_value`/`Result.map`/`tagged_union`/`case`/`tag`), `reliability/resilience#RESILIENCE` (`guarded(cls, fn, *args, subject=...)` the retried-traced-railed tier envelope, `RetryClass`).
- Growth: a new setting is one typed field on the settings model; a new source-precedence permutation is the one `settings_customise_sources` override returning a reordered tuple (absent until a reorder is actually needed); a new secret-resolution tier is one `SecretTier` case plus one `TierRow` on `SECRET_LADDER` carrying its `Option[Feature]` gate and `RetryClass`, the deferred cloud tier exactly that one case plus one row, never a second config owner, a parallel resolve method, or a free probe function; a new resolved output shape is one `SecretShape` member plus one `@overload` and one fold-tail arm; zero new surface.
- Boundary: no package reads `os.environ` directly after admission. The keystore reads OS credentials for the OUTBOUND transport legs only (`transport/roots#RESOURCE` `TransportResource.ssh` passphrase, `http` bearer); the companion UDS serve leg reads no keyring, peer identity being the kernel accept-time credential (`transport/serve#SERVE`). The deleted forms are:
  - environment reads outside this owner, or a no-op `settings_customise_sources` override restating the default order.
  - an eager unattended keystore probe — the named hazard, a keychain prompt the session cannot answer — or a secret read outside the one settings-admitted `SecretBoundary`.
  - a `Probe` callable type plus parallel free `_keystore`/`_secrets_file` resolver functions beside the one `SecretTier`-discriminated `_probe` fold; a rail-replacing `try`/`except` or a bare `boundary("resource", ...)` around the keystore call beside the one `guarded` resilience envelope that retries the transient and lifts the terminal raise once — distinct from the narrow `except keyring.errors.NoKeyringError` miss seam inside `keystore_read`, which reclassifies the no-backend signal as the keyring `MISSING_VS_ERROR` `Nothing` floor before `guarded` sees it so the fold falls through to `file`, never a general fault trap that swallows a transient lock the `guarded` row must retry.
  - a bare-`str` filesystem-root, store-root, or endpoint field beside the precise `DirectoryPath`/`AnyUrl`/`HttpUrl`/`FilePath` types; a bare-`str` resolved secret beside the repr-hiding `SecretStr`; a disabled-verification `known_hosts=None` SSH trust beside the admission-loaded `SSHKnownHosts`; a hand-rolled credential constructed beside the boundary.
  - two parallel `resolve_token`/`resolve_credential` methods beside the one `@overload`-parameterized `resolve(service, username, shape)`; a `model_config.get("secrets_dir")` reach into the model config where the `SECRETS_MOUNT` anchor IS the fixed secret root.
- The suite-wide configuration taxonomy stays AppHost-owned, and the multi-source `STRUCTURED_SETTINGS_SCHEMA` — including the cloud secret-manager tier and the `gcp_project_id`-backed `GoogleSecretManagerSettingsSource` chain row — stays the deferred IDEA while `google-cloud-secret-manager` is unadmitted in the central manifest. This page admits only the source rows and the secret tiers the outbound legs read today.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
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

# `Feature`/`FeatureGate` are the [02]-[CONTEXT] owners of this same `rasm.runtime.admission`
# module — no cross-module import, the two fences are one module's two declaration regions.

# --- [TYPES] ----------------------------------------------------------------------------


class SecretShape(StrEnum):
    TOKEN = "token"           # the bare passphrase/bearer the SSH/HTTP legs read off `.get_secret_value()`
    CREDENTIAL = "credential" # the (username, secret) pair the `httpx.BasicAuth` leg reads


@tagged_union(frozen=True)
class SecretTier:
    # the discriminant IS the resolver: a new tier is one case plus one `SECRET_LADDER` row,
    # never a `Probe` callable type plus a parallel free probe function. `cloud` is the
    # deferred `google-cloud-secret-manager` arm, unrealized until the distribution lands.
    tag: Literal["keystore", "file", "cloud"] = tag()
    keystore: bool = case()
    file: bool = case()
    cloud: str = case()

# --- [CONSTANTS] ------------------------------------------------------------------------

# the one OS secret-mount root — the `file_secret_settings` source target and the `file`
# tier read this single anchor, never two `/run/secrets` literals and never a reach into
# `model_config["secrets_dir"]` (the config sets it FROM this anchor, not the other way).
SECRETS_MOUNT: Final[str] = "/run/secrets"

# --- [MODELS] ---------------------------------------------------------------------------


class Credential(Struct, frozen=True):
    username: str
    secret: SecretStr


class SettingsAdmission(BaseSettings):
    # the pydantic edge: fields carry pydantic-native types and `| None`, lifted to `Option`
    # at the read site (`known_hosts`/the resource legs) per the boundary law — never an
    # `expression.Option` field pydantic-core cannot build a core schema for.
    model_config = SettingsConfigDict(frozen=True, extra="forbid", env_prefix="RASM_PY_", env_nested_delimiter="__", secrets_dir=SECRETS_MOUNT)

    scratch_root: DirectoryPath
    object_store_root: AnyUrl | None = None
    otel_endpoint: HttpUrl | None = None
    known_hosts: FilePath | None = None


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
    async def resolve(self, service: str, username: str | None = ..., *, shape: Literal[SecretShape.CREDENTIAL]) -> RuntimeRail[Option[Credential]]: ...
    async def resolve(self, service: str, username: str | None = None, shape: SecretShape = SecretShape.TOKEN) -> RuntimeRail[Option[SecretStr]] | RuntimeRail[Option[Credential]]:
        # `username=None` is the service-scoped bearer leg the keystore resolves to its backend-default
        # user; the resolved `Credential.username` is read back off the store, never re-stamped from the request.
        admitted = SECRET_LADDER.filter(lambda row: row.gate.map(self.gate.admits).default_value(True))

        async def walk(rows: Block[TierRow]) -> RuntimeRail[Option[Credential]]:
            # the synchronous `Block.fold` cannot thread the per-tier `guarded` await, so the closed
            # `SecretTier`-arity-bounded ladder recurses head-then-tail with no trampoline; the empty
            # ladder folds to `Ok(Nothing)`. `match rows.try_head()` destructures the head through the
            # `Option`/`Result` cases rather than a phantom `.value` read or an imperative `is_none` ladder.
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

    async def _probe(self, row: TierRow, service: str, username: str | None) -> RuntimeRail[Option[Credential]]:
        # each tier rides ONE resilience envelope: a transiently-locked keystore retries under
        # the row's `stamina` policy inside one span and the terminal raise lifts once — never a
        # bare `boundary` that fails the resolve on the first `KeyringLocked`. The blocking
        # keystore/file read offloads to the anyio worker pool so the probe never stalls the loop.
        # Each arm owns its complete `await guarded(...)`, so no `read` name leaks out of a `case`
        # body for the shared tail to depend on; the deferred `cloud` arm folds to `Ok(Nothing)`.
        match row.tier:
            case SecretTier(tag="keystore"):
                def keystore_read() -> Option[Credential]:
                    # the store resolves a `None` `username` to its backend-default user; the returned
                    # credential's own `username` is authoritative, never the request value. `NoKeyringError`
                    # (no viable backend, the headless/container case) is a MISS not a fault, swallowed to
                    # `Nothing` so the fold falls through to the ungated `file` tier rather than the
                    # `guarded` lifting it as a terminal `Error` that `walk` would short-circuit on — it
                    # is not in the `RetryClass.SECRET` `target`, so it is never retried, only floored.
                    try:
                        found = keyring.get_credential(service, username)
                    except keyring.errors.NoKeyringError:
                        return Nothing
                    return Option.of_optional(found).map(lambda c: Credential(c.username, SecretStr(c.password)))
                return await guarded(row.retry_class, anyio.to_thread.run_sync, keystore_read, subject="secret")
            case SecretTier(tag="file"):
                def file_read() -> Option[Credential]:
                    name = Option.of_optional(username).map(lambda u: f"{service}_{u}").default_value(service)
                    path = Path(SECRETS_MOUNT) / name
                    return Some(Credential(username or service, SecretStr(path.read_text(encoding="utf-8").strip()))) if path.exists() else Nothing
                return await guarded(row.retry_class, anyio.to_thread.run_sync, file_read, subject="secret")
            case SecretTier(tag="cloud"):
                # the deferred `google-cloud-secret-manager` arm: no `SECRET_LADDER` row admits it
                # yet, so it folds to `Ok(Nothing)` rather than a catch-all that would hide a fourth
                # tag — the explicit arm keeps the `SecretTier` match total over the closed union.
                return Ok(Nothing)
            case _ as unreachable:
                assert_never(unreachable)

    def known_hosts(self) -> RuntimeRail[asyncssh.SSHKnownHosts]:
        path = Option.of_optional(self.settings.known_hosts).map(str).default_value(str(Path.home() / ".ssh" / "known_hosts"))
        return boundary("resource", lambda: asyncssh.read_known_hosts(path), catch=OSError)

# --- [TABLES] ---------------------------------------------------------------------------

# the keystore tier over the file fallback; each row binds its profile gate and the
# `RetryClass.SECRET` row whose transient set is the keystore/file transients
# (`keyring.errors.KeyringLocked`, `OSError`) the resilience owner catches under one backoff.
# The deferred cloud tier is one row: `TierRow(SecretTier(cloud="rasm"), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)`.
SECRET_LADDER: Final[Block[TierRow]] = Block.of_seq([
    TierRow(SecretTier(keystore=True), Some(Feature.KEYSTORE_PROBE), RetryClass.SECRET),
    TierRow(SecretTier(file=True), Nothing, RetryClass.SECRET),
])
```

## [04]-[RESEARCH]

- [SETTINGS_SOURCE_ORDER]: the realized model rides the `pydantic-settings` DEFAULT source precedence — init>env>dotenv>secret-file per the catalogue IMPLEMENTATION_LAW source-law — so NO `settings_customise_sources` override is authored: the catalogue states the default order explicitly and that "reorder by returning a permuted tuple" is the override's only purpose, making an override that returns the verbatim default pure ceremony, deleted here. The `file_secret_settings` `secrets_dir` tier (the lowest-priority fallback) and the `secrets_dir`/`env_prefix`/`env_nested_delimiter` `SettingsConfigDict` keys are verified against the catalogue; every root is typed precisely against the `pydantic` catalogue — `scratch_root: DirectoryPath` (PUBLIC_TYPES [14], existence-checked directory), `object_store_root: AnyUrl | None` (PUBLIC_TYPES [05], scheme-checked URL, never a stringly store path), `otel_endpoint: HttpUrl | None` (PUBLIC_TYPES [05], parsed URL value object), and `known_hosts: FilePath | None` (PUBLIC_TYPES [14], existence-checked file) — so the model rejects a non-directory scratch root, a malformed endpoint, a non-URL store root, and a missing known-hosts file at admission rather than at first use. The nullable roots carry the pydantic-edge `| None` (the type pydantic-core builds a core schema for) and lift to `Option` at the read site through `Option.of_optional` per the boundary law (internal code uses `Option`, the settings model is the edge), never an `expression.Option` field pydantic-core cannot schema. The cloud `GoogleSecretManagerSettingsSource` chain row (`.api/pydantic-settings.md` source [13]) is NOT admitted on this realized cell — its backing distribution `google-cloud-secret-manager` is unadmitted in the central manifest, so a verbatim fence importing it would `ImportError`; the GCP chain row and the `gcp_project_id` field stay the deferred `STRUCTURED_SETTINGS_SCHEMA` IDEA until the distribution lands, when the cloud read becomes one `SecretTier.cloud` case plus one `TierRow(SecretTier(cloud=...), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` row above the file fallback rather than a settings-source-chain entry — the secret ladder is the `SECRET_LADDER` fold, distinct from the `pydantic-settings` model-field source chain.
- [PROFILE_FEATURE_GATE]: the feature/killswitch state is folded into `PROFILE_POLICY` as the `FeatureGate` column — an admitted-`Feature` set and a tripped-`Killswitch` set per profile row — and `FeatureGate.admits` folds BOTH axes through the one `KILLSWITCH_FEATURE` disabling-edge `Map[Killswitch, Feature]` (a feature is live iff admitted and not revoked by any tripped killswitch), so `RuntimeContext.admits` and the `SecretBoundary.resolve` `SECRET_LADDER` filter dispatch on EFFECTIVE availability rather than a boolean knob the caller re-derives and a killswitch is never dead policy the resolve path silently ignores — the moment the deferred cloud row admits `Feature.SECRET_MANAGER`, a profile tripping `Killswitch.DISABLE_SECRET_MANAGER` drops that row through the same `admits` fold with no new branch. `ProfilePolicy` carries no `profile` field because the `Map[RuntimeProfile, ProfilePolicy]` key IS the profile, eliminating the key/field drift vector a duplicated discriminant introduces. This realizes the local cell of `STRUCTURED_SETTINGS_SCHEMA` (the per-profile gate) while the broader multi-source structured schema stays the deferred IDEA, not over-built here. The `Feature.KEYSTORE_PROBE` row gates the realized `keystore` `SecretTier` and the `Killswitch.DISABLE_OUTBOUND` row revokes `Feature.OUTBOUND_TRANSPORT` on the outbound leg through its `KILLSWITCH_FEATURE` edge; the `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER` pair is the forward-looking gate the deferred `SecretTier.cloud` row resolves against, live in the `admits` fold rather than carried as inert columns. The `PACKAGE`/`TEST` profiles drop the keystore row from the `SECRET_LADDER` fold to the ungated `file` tier so the unattended-probe hazard stays closed.
- [SECRET_BOUNDARY]: the secret ladder is the `SecretTier` `@tagged_union` (`keystore`/`file`/`cloud`) folded over the data-driven `SECRET_LADDER: Block[TierRow]` table, each `TierRow` an `Option[Feature]` gate plus the `reliability/resilience#RESILIENCE` `RetryClass.SECRET` row its envelope retries under — the gated keystore row (`SecretTier(keystore=True)`, `Some(Feature.KEYSTORE_PROBE)`, `RetryClass.SECRET`) over the ungated file fallback (`SecretTier(file=True)`, `Nothing`, `RetryClass.SECRET`). `RetryClass.SECRET` does not yet exist on the resilience owner (members `OBJECT_STORE`/`HTTP`/`SSH`/`WIRE`/`SCAN`); this page DECLARES the cross-file addition — one `RetryClass.SECRET` member plus one `POLICY.add` row binding `target=(keyring.errors.KeyringLocked, OSError)` — because a reused `SSH`/`OBJECT_STORE` `target` tuple never catches a keystore lock. The dependency direction holds: resilience maps retry classes to exception sets, admission consumes the class, neither imports the other's domain types beyond the `RetryClass` enum and `guarded`. This collapses the prior 1 type-alias (`Probe`) + 1 struct (`SecretSource`) + 2 free functions (`_keystore`/`_secrets_file`) into 1 ADT + 1 row struct + 1 `match`-dispatched `_probe` — the tier IS the discriminant, the lanes `Admit`/`ADMIT_TABLE` collapse shape applied to the secret ladder, so a new tier is one case plus one row and never a new private resolver function. `resolve` is parameterized over input AND output through `@overload` on the `SecretShape` literal (`TOKEN` -> `RuntimeRail[Option[SecretStr]]` the realized `transport/roots#RESOURCE` `ssh` passphrase and `http` `_BearerAuth` bearer legs read today, `CREDENTIAL` -> `RuntimeRail[Option[Credential]]` the forward `httpx.BasicAuth(username, password)` two-part leg reads when a BASIC artifact lands), so admitting the BASIC consumer is one `@overload` arm on the one entrypoint rather than a second method — the same input-and-output projection axis the `transport/roots#RESOURCE` `survey` `@overload` and the `evidence/identity#IDENTITY` `ContentIdentity.of` ladder realize. The fold is a depth-bounded async `walk` over the `FeatureGate.admits`-filtered rows (an ungated row always passes, a `Some(feature)` row only when the gate holds), short-circuiting on the first probe `Error` or first present `Credential` and folding the empty ladder to `Ok(Nothing)`; the synchronous `Block.fold` cannot thread the await the per-tier `guarded` envelope returns, so `walk` recurses on `Block.try_head`/`Block.tail` (depth bounded by the closed `SecretTier` arity, no `tailrec_async` trampoline) rather than a mutable accumulation.
  - `_probe` rides ONE `reliability/resilience#RESILIENCE` `guarded(row.retry_class, anyio.to_thread.run_sync, <tier-read>, subject="secret")` envelope per tier, composing five admitted surfaces into one rail: the `match row.tier` discriminant owns one complete `await guarded(...)` per arm over its `keystore_read`/`file_read` blocking thunk (no `read` name leaking out of a `case` body), `anyio.to_thread.run_sync` (ENTRYPOINTS [01]) offloads it off the event loop, `guarded` retries the transient (`keyring.errors.KeyringLocked` errors [02]) under the tier's `stamina` policy inside one derivation span, `keyring.get_credential` (ENTRYPOINTS [04]) does the structured read, and `pydantic` `SecretStr` carries the result — never a bare `boundary("resource", ...)` that fails the resolve on the first transient lock and never a bare-`str` secret a receipt could leak. The `keystore_read` thunk lifts `keyring.get_credential(service, username) -> Credential | None` (returning `None` for a miss, and resolving a `None` `username` to the backend-default user per ENTRYPOINTS [04]) through `Option.of_optional` and maps a present `credentials.Credential` (credentials [01]) to `Credential(c.username, SecretStr(c.password))` — reading the store-resolved `c.username` back rather than re-stamping the request value, so the service-scoped bearer leg carries the backend's own identity. `errors.NoKeyringError` (errors [06]), the no-viable-backend headless/container case, is NOT in the `RetryClass.SECRET` `target` tuple — it is terminal, never retried — so the thunk swallows it to `Nothing` at the `keyring` missing-vs-error seam (the catalogue `MISSING_VS_ERROR` law: `get_credential` returns `None` for a missing secret, only no-backend raises) and the fold falls through to the ungated `file` tier; were the thunk to let it escape, `guarded` would lift it as a terminal `Error` and `walk` would short-circuit the whole resolve on `case Error(_)` rather than reaching the file fallback the gated-keystore-headless profile depends on.
  - The `file` tier reads the mounted secret with the `Path(SECRETS_MOUNT) / name` build inside the `read` thunk, where `name` folds the optional `username` into `{service}_{username}` (the user-scoped mount) or bare `service` (the service-scoped bearer mount) through `Option.of_optional(username).map(...).default_value(service)`, folding a present file to `Some(Credential(username or service, SecretStr(text.strip())))` and an absent mount to `Nothing` through the `path.exists()` gate. `root` is the one `SECRETS_MOUNT` anchor the `SettingsConfigDict` `secrets_dir` is itself set FROM — never a `model_config.get("secrets_dir")` reach into the model config (the config derives from the anchor, not the reverse, so reading it back is circular ceremony). The caller-dynamic `{service}_{username}` name cannot be a declared field on the frozen `extra='forbid'` `SettingsAdmission` (only `scratch_root`/`object_store_root`/`otel_endpoint`/`known_hosts` are admitted), so the tier reads the OS-mounted file at the same root the `file_secret_settings` source targets, never a `getattr` the forbidding model cannot satisfy.
  - `SecretBoundary.known_hosts` is the host-key-database leg: `asyncssh.read_known_hosts(filelist) -> SSHKnownHosts` (ENTRYPOINTS [02]) lifted on `reliability/faults#FAULT` `boundary("resource", ...)` (catching `OSError`) over the `FilePath | None` `known_hosts` path lifted through `Option.of_optional(...).map(str).default_value(~/.ssh/known_hosts)`, returning the `SSHKnownHosts` (PUBLIC_TYPES [04]) trust database the `transport/roots#RESOURCE` `ssh` connection law binds into `asyncssh.connect(..., known_hosts=...)`. This is the one-shot startup DB load, not a transient-retry leg, so it rides the bare `boundary` rather than `guarded`. `keyring` is the one `execution/admission#SETTINGS` outbound-credential consumer (orphaned-catalogue after `transport/serve#KEYRING_CATALOGUE` dropped the serve leg).
- [CLOCK_CONSUMPTION]: `Hlc`/`CausalFrame`/`Tenant` source from `clock/clock#CLOCK` (R0) — admission imports `CausalFrame`/`Tenant` from `rasm.runtime.clock` and consumes the host-minted `Hlc` stamp through the `CausalFrame.hlc` field, projecting the carried `(tenant, hlc)` onto the span/receipt/metric attribute map by composing the canonical `CausalFrame.attributes("packed")` projection (clock `[02]-[CLOCK]`) — admission re-spells NOTHING: the `rasm.tenant`/`rasm.hlc` slot keys ride the clock owner's one `SLOTS` table and the fixed-width `032x` hex of the `Hlc.packed` 128-bit value is the clock owner's `packed`-shape rendering, not a re-derived `format(packed, "032x")` here. The clock `[ATTRIBUTE_PROJECTION]` row names this admission inline map the deleted form that collapses INTO `frame.attributes("packed")`; this page is that collapse — the prior `format(...)`/`f"{physical_ticks}.{logical}"` admission spelling diverged from the clock owner's rendering and is deleted, and selecting the `"packed"` shape (over `"halves"`) keeps the single-int HLC inside the OTLP signed-int64 bound a raw 128-bit int would overflow while the two halves the metric attributer needs ride the `"halves"` shape on its own page — one projector, two output shapes, no per-page map. `Correlation.trace_id` carries a 16-byte W3C-shaped identifier and `RuntimeContext.admit` seeds `Correlation.parent` by un-hexing the inbound frame's canonical `CausalFrame.attributes("packed")` `rasm.hlc` 032x rendering to that 16-byte identity (`bytes.fromhex(frame.attributes("packed")["rasm.hlc"])`), so the parent-id derives from the clock owner's one packed rendering rather than a `to_bytes(16, "big")` re-pack admission would otherwise duplicate — a context admitting the host stamp threads the host causal position as its trace parent through the same projection `attribute` composes. `Correlation` and `CausalFrame` stay distinct concepts (the `Correlation` the `observability/receipts#RECEIPT` `merge_contextvars` bound-context spine, the `CausalFrame` the host causal stamp) but the admit path now links them rather than minting an orphan parentless root. Admission re-spells none of the clock records; the earlier `transport/serve` `Hlc` import and the in-page `Tenant`/`CausalFrame` redeclarations are the deleted forms, the clock owner being the single decode site and the C# `AppHost/Runtime` the single mint.
