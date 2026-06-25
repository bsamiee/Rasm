# [PY_ARTIFACTS_CREDENTIAL]

The C2PA content-credential owner at the exchange boundary. `Provenance` is ONE owner over `c2pa-python` binding a signed tamper-evident manifest into an emitted artifact and reading it back, discriminating `ProvenanceOp` (`Sign`/`Read`/`Verify`/`Ingredient`) by the operation's payload shape — never a `from_file`/`from_stream`/`per-format` reader family and never a per-signing-algorithm signer type. `SignerSpec` is the ONE policy union carrying one typed signer case — a `CertKey` (`alg`, `sign_cert`, `private_key`, `ta_url`) feeding `Signer.from_info` and a `Callback` (`alg`, `certs`, `sign`, `ta_url`) whose campaign-supplied `sign` digest-callback feeds `Signer.from_callback` — so the `cryptography`/HSM seam is a tagged case carrying the keyring/HSM-bound signer as a typed field and the signing algorithm is a `C2paSigningAlg` row across ES/PS/ED25519, never a parallel signer owner or a free-text alg field. The signed-artifact buffer keys by the runtime content key over the embedded-manifest bytes, so the binding decodes the `XxHash128` seed the `csharp:Rasm.Persistence` store re-derives. This is cross-format content-authenticity provenance over the image/BMFF/audio signable set, orthogonal to the `exchange/conformance#CONFORMANCE` PAdES cryptographic close (PDF is read-only here — `Builder.sign` never touches a PDF, the `pyhanko` rail owns PDF signing); it owns no artifact production, only the credential close over already-emitted bytes.

## [01]-[INDEX]

- [01]-[CREDENTIAL]: C2PA manifest authoring + single-use signing, manifest-store extraction + `validation_state` reporting, ingredient-chain attachment, the per-case `SignerSpec` cert/callback policy union, and the content-keyed credential owner threading the typed `CredentialEvidence` receipt into `core/receipt#RECEIPT` `ArtifactReceipt.Credential` and the `csharp:Rasm.Persistence` content-key binding.

## [02]-[CREDENTIAL]

- Owner: `Provenance` the one credential owner; `ProvenanceOp` the closed `tagged_union` discriminating operation over its own typed payload — `Sign` (manifest + asset bytes + signer), `Read` (asset bytes, the projection-only extraction), `Verify` (asset bytes, the validation-state assertion), `Ingredient` (parent manifest + asset, the chain-attach pre-pass) — matched by one total `match`/`case`, never a per-operation method family; `Manifest` the frozen authoring value carrying the JSON definition, the `c2pa.actions` assertion sequence, the ingredient list, and the `C2paBuilderIntent`/`C2paDigitalSourceType` intent row; `SignerSpec` the one policy union carrying one typed signer case per signing seam; `CredentialEvidence` the typed provenance receipt; the C2PA core binds in-process — the `c2pa` wheel bundles the native `libc2pa_c` per platform loaded via `ctypes` and resolves on the cp315-core process (the arm64 `py3-none` wheel imports clean), so no gated subprocess band and no hand-rolled JUMBF/COSE codec.
- Cases: `ProvenanceOp` rows `Sign` (`Builder.from_json` author → `Signer` → `Builder.sign` into an in-memory `BytesIO`, single-use, the builder closes after `sign`) · `Read` (`Reader.try_create` → `json`/`get_active_manifest`, the optional-returning extraction that maps `ManifestNotFound` to `None`) · `Verify` (`Reader.try_create` → `get_validation_state`/`get_validation_results`, the store-field read, never a recomputed local verdict) · `Ingredient` (`Builder.add_ingredient_from_stream` parent/component attach folded onto the authoring builder before `Sign`) — each binding the one `Builder`/`Reader` surface keyed by argument shape rather than a parallel type; `SignerSpec` cases `cert_key` (`CertKeySigner`) · `callback` (`CallbackSigner`) — one frozen signer-struct per seam whose fields are the named crypto axes the `match` arm reads directly, the `alg` axis a `C2paSigningAlg` row, so a per-algorithm signer type and a positional cert tuple are the deleted forms and no private-key material is minted here. The intent axis is a `Manifest` row not a parallel field: `C2paBuilderIntent` (`CREATE`/`EDIT`/`UPDATE`) and `C2paDigitalSourceType` (the full IPTC digital-source vocabulary — `DIGITAL_CAPTURE`, `TRAINED_ALGORITHMIC_MEDIA`, `COMPOSITE_SYNTHETIC`, …) ride `Builder.set_intent`, so AI-provenance origin is the table-driven source-type token the verifier reads, never a free-text manifest field.
- Entry: `Provenance.bind` is the one modal entrypoint dispatching `ProvenanceOp` through the one `async_boundary` rail returning `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` — the `Sign` arm signs and keys the signed buffer through `ContentIdentity.of`, the `Read`/`Verify`/`Ingredient` arms key the extracted/verified store, never a per-operation entrypoint or a `sign`/`read`/`verify` method triple; `_emit` folds the `ContentIdentity.of` projection over the signed/read bytes once and spreads the `CredentialEvidence` named fields onto the flat-scalar `ArtifactReceipt.Credential` case. The asset bytes arrive as an in-memory `BytesIO` from the imaging/document owner and leave as a signed `BytesIO`, so the codec hands a decoded buffer and receives a signed buffer with no intermediate file — `sign_file` is the path convenience only when a file handle already exists, never the default. The native core signs on a worker thread through `anyio.to_thread.run_sync`, so the `async_boundary` rail never blocks the event loop; the in-process `c2pa` wheel needs no `anyio.to_process` gated band (distinct from the host-native `exchange/detect#DETECT`/`exchange/metadata#METADATA` siblings that cross the subprocess seam).
- Auto: the build is one ordered fold over `Manifest` then signer — `Manifest.author` seeds the `Builder.from_json` definition, folds `set_intent` from the intent row, folds the `add_action` assertion sequence, and folds the `Ingredient` attachments through `add_ingredient_from_stream`, and the module-level `_signer` projects the `Signer.from_info`/`from_callback` row from the `SignerSpec` case so the COSE signer is one construction off the named crypto axes; the `Sign` arm then drives `Builder.sign(signer, format, source_stream, dest_stream)` into a fresh `BytesIO`, reads the signed bytes back through `_read` for the validation evidence, and the builder closes single-use. The `Read`/`Verify` arms open one `Reader.try_create(format, stream)` context whose `get_validation_state` reads the overall `validation_state` string and `get_validation_results` reads the `validation_results` object straight off the parsed store — the validation status is a field read, not a recomputed verdict — and `json()` parses the active-manifest dict for the assertion/ingredient counts. `CredentialEvidence.measure` is the one constructor folding the active-manifest label, the matched `SignerSpec.alg` token, the `validation_state`, and the parsed `assertions`/`ingredients` array lengths into one row in a single store walk, never a per-arm re-projection or a two-pass count; the `try_create` `None` arm projects the `unsigned` evidence when an asset carries no Content Credentials. The `Context`/`Settings` per-instance configuration threads through `Context.from_dict` or `ContextBuilder.with_settings(...).with_signer(...).build()` so trust-list and verify policy is one settled `Context` value bound into both `Builder.from_json(context=...)` and `Reader.try_create(context=...)`, never the deprecated thread-local `load_settings` path.
- Receipt: each `bind` contributes `core/receipt#RECEIPT` `ArtifactReceipt.Credential` carrying the content key and the settled credential facts — c2pa manifest id (the active-manifest label), signer identity (the matched `alg` token), assertion count, and the `validation_state` string — projected through the one `CredentialEvidence.measure` constructor and spread onto the flat-scalar `ArtifactReceipt.Credential` case `tuple[ContentKey, str, str, int, str]`; `CredentialEvidence` additionally measures the ingredient-chain count as the producer-side store walk, but the settled receipt contribution is the four-scalar fact set the shared `Credential` case fixes — the manifest-store walk reads both array lengths once and the receipt threads the contract fields. `_emit` spreads the named evidence fields onto the case and the receipt owner's own `_facts` arm is the single string-map projector, so `CredentialEvidence` carries no second `facts` projection and the receipt owner imports no `CredentialEvidence` value object (the `credential.py` import of `ArtifactReceipt` would close a module-scope cycle on any reciprocal import, mirroring the flat-scalar `Egress`/`Bundle` cases). The `validation_state` is the one observable provenance value the runtime `observability/metrics` `MeterProvider` valid/invalid-credential signal stream reads off the receipt fold.
- Packages: `c2pa` (`Builder`; `from_json`/`sign`/`sign_file`/`set_intent`/`add_action`/`add_ingredient_from_stream`/`add_ingredient`/`add_resource`/`set_remote_url`/`set_no_embed`/`get_supported_mime_types`; `Reader`; `try_create`/`json`/`detailed_json`/`get_validation_state`/`get_validation_results`/`get_active_manifest`/`get_manifest`/`is_embedded`/`get_remote_url`; `Signer.from_info`/`Signer.from_callback`/`Signer.reserve_size`; `C2paSignerInfo`; `C2paSigningAlg` ES256/ES384/ES512/PS256/PS384/PS512/ED25519; `C2paBuilderIntent` CREATE/EDIT/UPDATE; `C2paDigitalSourceType`; `Context.from_dict`/`ContextBuilder` `with_settings`/`with_signer`/`build`; `Settings.from_dict`; `C2paError` + the `ManifestNotFound`/`Signature`/`Verify` subclass arms; `sdk_version`) on the cp315 core; `msgspec` (`Struct(frozen=True)` for the `Manifest`/`SignerSpec`/`CredentialEvidence` value owners); `expression` (`tagged_union`/`tag`/`case`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`).
- Growth: a new operation is one `ProvenanceOp` case with its typed payload and one `match` arm; a new signer seam is one `SignerSpec` case with its typed signer-struct and one `_signer` arm; a new signing algorithm is one `C2paSigningAlg` row threaded through the existing `alg` field, never a new signer type; a new manifest assertion is one `add_action` row in `Manifest.author`; a new intent or digital-source origin is one `C2paBuilderIntent`/`C2paDigitalSourceType` token on the existing intent row; a new evidence fact is one field on `CredentialEvidence` and one `(label, value)` row in the receipt `_facts`; a new readable projection is one `Reader` row on the `Read` arm; zero new surface.
- Boundary: a per-format reader (`PngReader`/`JpegReader`), a per-algorithm signer class, a hand-rolled JUMBF/COSE manifest codec, a local re-computation of `validation_state` standing in for the store field, a positional cert/key tuple decoded by index, a free-text `alg`/`intent` manifest field, a rename-only `sign`/`verify` forwarder, a per-arm `ContentIdentity.of` key-mint, and a second `CredentialEvidence.facts` projection are the deleted forms; this owner is content-credential binding over already-emitted artifact bytes and owns no artifact production, mints no certificate or key (the campaign signer config supplies cert/key/TSA, the `Callback` case keeps private-key material in the `cryptography` keyring or HSM and only `C2paSigningAlg` plus the PEM cert chain cross the seam), and routes a PDF or raw-camera (`arw`/`nef`) asset to the `exchange/conformance#CONFORMANCE` `pyhanko` PAdES rail rather than `Builder.sign` (read-only here). The `ArtifactReceipt.Credential` case-tuple carrying the flat `(key, manifest_id, signer, assertions, validation_state)` evidence scalars lands on the same-domain `core/receipt#RECEIPT` owner (the `Credential` case the receipt union grows beside `Media`); this page composes the settled `CredentialEvidence` value and spreads its receipt-bound fields onto the receipt case, mirroring the flat-scalar `Egress`/`Bundle` cases so the receipt owner imports no producer value object. The signed-artifact `ContentKey` is the wire to the `csharp:Rasm.Persistence` store — the binding decodes the `XxHash128` seed the persistence owner re-derives over the same signed bytes, so the C2PA manifest and the content key co-identify one durable artifact across the language boundary, the credential never re-minting the content key the runtime owns.

```python signature
from collections.abc import Callable, Mapping
from io import BytesIO
from typing import Any, Final, Literal, assert_never

from anyio import to_thread
from c2pa import (
    Builder,
    C2paBuilderIntent,
    C2paDigitalSourceType,
    C2paSignerInfo,
    C2paSigningAlg,
    Context,
    Reader,
    Signer,
)
from expression import case, tag, tagged_union
from msgspec import Struct, json

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

type SigningAlg = Literal["es256", "es384", "es512", "ps256", "ps384", "ps512", "ed25519"]
type Intent = Literal["create", "edit", "update"]


_SIGNING_ALG: Final[dict[SigningAlg, C2paSigningAlg]] = {
    "es256": C2paSigningAlg.ES256,
    "es384": C2paSigningAlg.ES384,
    "es512": C2paSigningAlg.ES512,
    "ps256": C2paSigningAlg.PS256,
    "ps384": C2paSigningAlg.PS384,
    "ps512": C2paSigningAlg.PS512,
    "ed25519": C2paSigningAlg.ED25519,
}

_INTENT: Final[dict[Intent, C2paBuilderIntent]] = {
    "create": C2paBuilderIntent.CREATE,
    "edit": C2paBuilderIntent.EDIT,
    "update": C2paBuilderIntent.UPDATE,
}


class Ingredient(Struct, frozen=True):
    definition: dict[str, Any]
    fmt: str
    source: bytes


class Manifest(Struct, frozen=True):
    definition: dict[str, Any]
    actions: tuple[dict[str, Any], ...] = ()
    ingredients: tuple[Ingredient, ...] = ()
    intent: Intent | None = None
    digital_source: str = ""

    def author(self, context: Context | None = None) -> Builder:
        builder = Builder.from_json(self.definition, context=context)
        if self.intent is not None:
            builder.set_intent(_INTENT[self.intent], C2paDigitalSourceType[self.digital_source] if self.digital_source else C2paDigitalSourceType.EMPTY)
        for action in self.actions:
            builder.add_action(action)
        for ingredient in self.ingredients:
            builder.add_ingredient_from_stream(ingredient.definition, ingredient.fmt, BytesIO(ingredient.source))
        return builder


class CertKeySigner(Struct, frozen=True):
    alg: SigningAlg
    sign_cert: bytes
    private_key: bytes
    ta_url: str | None = None


class CallbackSigner(Struct, frozen=True):
    alg: SigningAlg
    certs: str
    sign: Callable[[bytes], bytes]
    ta_url: str | None = None


@tagged_union(frozen=True)
class SignerSpec:
    tag: Literal["cert_key", "callback"] = tag()
    cert_key: CertKeySigner = case()
    callback: CallbackSigner = case()

    @property
    def alg(self) -> SigningAlg:
        match self:
            case SignerSpec(tag="cert_key", cert_key=CertKeySigner() as s):
                return s.alg
            case SignerSpec(tag="callback", callback=CallbackSigner() as s):
                return s.alg
            case _:
                assert_never(self)


class CredentialEvidence(Struct, frozen=True):
    manifest_id: str
    signer: str
    assertions: int
    ingredients: int
    validation_state: str

    @staticmethod
    def measure(store: Mapping[str, Any] | None, signer: str, state: str | None) -> "CredentialEvidence":
        active = store.get("active_manifest", {}) if store is not None else {}
        return CredentialEvidence(active.get("label", ""), signer, len(active.get("assertions", ())), len(active.get("ingredients", ())), state or "unsigned")


@tagged_union(frozen=True)
class ProvenanceOp:
    tag: Literal["sign", "read", "verify", "ingredient"] = tag()
    sign: tuple[Manifest, str, bytes, SignerSpec] = case()
    read: tuple[str, bytes] = case()
    verify: tuple[str, bytes] = case()
    ingredient: tuple[Manifest, str, bytes, SignerSpec, Ingredient] = case()


class Provenance(Struct, frozen=True):
    op: ProvenanceOp
    context: Context | None = None

    @staticmethod
    def Sign(manifest: Manifest, fmt: str, source: bytes, signer: SignerSpec, context: Context | None = None) -> "Provenance":
        return Provenance(ProvenanceOp(sign=(manifest, fmt, source, signer)), context)

    @staticmethod
    def Verify(fmt: str, source: bytes, context: Context | None = None) -> "Provenance":
        return Provenance(ProvenanceOp(verify=(fmt, source)), context)

    @staticmethod
    def Read(fmt: str, source: bytes, context: Context | None = None) -> "Provenance":
        return Provenance(ProvenanceOp(read=(fmt, source)), context)

    @staticmethod
    def WithIngredient(manifest: Manifest, fmt: str, source: bytes, signer: SignerSpec, parent: Ingredient, context: Context | None = None) -> "Provenance":
        return Provenance(ProvenanceOp(ingredient=(manifest, fmt, source, signer, parent)), context)

    async def bind(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"credential.{self.op.tag}", self._emit)

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        signed, evidence = await to_thread.run_sync(self._run)
        key = ContentIdentity.of(f"credential-{self.op.tag}", signed)
        return key, ArtifactReceipt.Credential(key, evidence.manifest_id, evidence.signer, evidence.assertions, evidence.validation_state)

    def _run(self) -> tuple[bytes, CredentialEvidence]:
        match self.op:
            case ProvenanceOp(tag="sign", sign=(manifest, fmt, source, signer)):
                return self._sign(manifest, fmt, source, signer)
            case ProvenanceOp(tag="ingredient", ingredient=(manifest, fmt, source, signer, parent)):
                return self._sign(Manifest(manifest.definition, manifest.actions, (*manifest.ingredients, parent), manifest.intent, manifest.digital_source), fmt, source, signer)
            case ProvenanceOp(tag="read", read=(fmt, source)) | ProvenanceOp(tag="verify", verify=(fmt, source)):
                return self._read(fmt, source)
            case _:
                assert_never(self.op)

    def _sign(self, manifest: Manifest, fmt: str, source: bytes, signer: SignerSpec) -> tuple[bytes, CredentialEvidence]:
        sink = BytesIO()
        builder = manifest.author(self.context)
        builder.sign(_signer(signer), fmt, BytesIO(source), sink)
        signed = sink.getvalue()
        return signed, self._read(fmt, signed, signer.alg)[1]

    def _read(self, fmt: str, source: bytes, signer: str = "") -> tuple[bytes, CredentialEvidence]:
        reader = Reader.try_create(fmt, BytesIO(source), context=self.context)
        if reader is None:
            return source, CredentialEvidence.measure(None, signer, None)
        with reader:
            return source, CredentialEvidence.measure(json.decode(reader.json()), signer, reader.get_validation_state())


def _signer(spec: SignerSpec) -> Signer:
    match spec:
        case SignerSpec(tag="cert_key", cert_key=CertKeySigner() as s):
            return Signer.from_info(C2paSignerInfo(alg=_SIGNING_ALG[s.alg], sign_cert=s.sign_cert, private_key=s.private_key, ta_url=s.ta_url))
        case SignerSpec(tag="callback", callback=CallbackSigner() as s):
            return Signer.from_callback(s.sign, _SIGNING_ALG[s.alg], s.certs, s.ta_url)
        case _:
            assert_never(spec)
```

## [03]-[RESEARCH]

- [C2PA_SIGN_AND_READ] [RESOLVED]: `Builder.from_json(manifest_json, context=None)` (the named factory over a JSON definition string-or-dict, ENTRYPOINTS row `[02]`), `Builder.sign(signer_or_format, format_or_source=None, source_or_dest=None, dest=None) -> bytes` (the single signing surface keyed by first-argument shape, single-use and closing after the call, row `[04]`; omitting `dest` buffers the signed asset into an in-memory `BytesIO`), `Builder.sign_file(source_path, dest_path, signer=None) -> bytes` (the path-to-path row `[05]`), and `Reader.try_create(format_or_path, stream=None, manifest_data=None, context=None) -> Optional[Reader]` (the optional-returning factory mapping `ManifestNotFound` to `None`, row `[02]`) verify against the folder `c2pa-python` `.api` `[03]-[ENTRYPOINTS]` `Builder` author-and-sign and `Reader` extract-and-validate tables; the in-memory `BytesIO` source/dest pair is the `[INTEGRATION_STACK]` asset-stream seam (`Builder.sign(signer, format, source_stream, dest_stream)` reads and writes `BytesIO`, no intermediate file). The single signing surface keyed by argument shape is the `[04]-[IMPLEMENTATION_LAW]` sign-axis row, so the `Sign`/`Ingredient` arms drive one `Builder.sign`, never a per-shape signer.
- [C2PA_VALIDATION_STATE] [RESOLVED]: `Reader.json() -> str` (the manifest store as a JSON string, cached, row `[03]`), `Reader.get_validation_state() -> Optional[str]` (the overall `validation_state` field read, row `[06]`), `Reader.get_validation_results() -> Optional[dict]` (the detailed `validation_results` object, row `[07]`), `Reader.get_active_manifest() -> Optional[dict]` (the active manifest dict from the store, row `[08]`), `Reader.get_manifest(label) -> Optional[dict]` (a manifest dict by label, row `[09]`), and `Reader.is_embedded() -> bool` (the embedded-vs-remote flag, row `[10]`) verify against the folder `c2pa-python` `.api` `[03]` `Reader` extract-and-validate table; the validation-axis law mandates that `validation_state` is a field read off the parsed store, never a recomputed local verdict (`[04]-[IMPLEMENTATION_LAW]` validation-axis), so `_read` reads `get_validation_state()` and parses `reader.json()` for the active-manifest label, assertion count, and ingredient count rather than re-deriving validity. The `CredentialEvidence.measure` walk over the parsed-store `active_manifest.assertions`/`active_manifest.ingredients` arrays is the manifest-store JSON shape the SDK returns.
- [C2PA_SIGNER_AXIS] [RESOLVED]: `Signer.from_info(signer_info: C2paSignerInfo) -> Signer` (the COSE signer from cert/key/TSA configuration, row `[01]`), `Signer.from_callback(callback, alg: C2paSigningAlg, certs: str, tsa_url=None) -> Signer` (the signer from an external signing callback, row `[02]`), `C2paSignerInfo(alg, sign_cert, private_key, ta_url)` (the signer-configuration value whose `alg`/`sign_cert`/`private_key`/`ta_url` fields verify against the installed `C2paSignerInfo` attribute set), and the `C2paSigningAlg` `ES256`/`ES384`/`ES512`/`PS256`/`PS384`/`PS512`/`ED25519` `IntEnum` rows verify against the folder `c2pa-python` `.api` `[03]` signer table and `[02]-[PUBLIC_TYPES]` enum vocabulary; the algorithm is one enum row across ES/PS/ED25519 (`[04]-[IMPLEMENTATION_LAW]` signer-axis), so the two `SignerSpec` cases bind one `Signer.from_info`/`Signer.from_callback` row each rather than a per-algorithm signer type, and `Signer.from_callback` is the `[INTEGRATION_STACK]` `cryptography`/HSM seam keeping private-key material out of `c2pa` (only `C2paSigningAlg` and the PEM `certs` chain cross). The `_SIGNING_ALG` `Literal`-to-`C2paSigningAlg` dispatch row resolves the enum ordinal at boundary scope so no native `IntEnum` value is hardcoded beside the signer call.
- [C2PA_INTENT_SOURCE] [RESOLVED]: `Builder.set_intent(intent: C2paBuilderIntent, digital_source_type: C2paDigitalSourceType = C2paDigitalSourceType.EMPTY)` (the manifest intent and digital-source-type row `[06]`), `Builder.add_action(action_json) -> None` (the `c2pa.actions` assertion append, row `[09]`), and `Builder.add_ingredient_from_stream(ingredient_json, format, source)` (the ingredient-attach row keyed to an open source stream, row `[07a]`) verify against the folder `c2pa-python` `.api` `[03]` `Builder` table; `C2paBuilderIntent` (`CREATE`/`EDIT`/`UPDATE`) and `C2paDigitalSourceType` (the full IPTC digital-source vocabulary including `TRAINED_ALGORITHMIC_MEDIA`/`COMPOSITE_SYNTHETIC`/`ALGORITHMICALLY_ENHANCED`) verify against the `.api` `[02]-[PUBLIC_TYPES]` enum rows. The AI-provenance origin is the table-driven `C2paDigitalSourceType` source-type token the verifier reads (`[INTEGRATION_STACK]` AI-provenance seam), so the `Manifest` intent row threads one `set_intent` rather than a free-text manifest field; the `digital_source` axis resolves through `C2paDigitalSourceType[name]` member access over the installed `IntEnum`.
- [C2PA_CONTEXT_CONFIG] [RESOLVED]: `Context.from_dict(config, signer=None) -> Context` (context from a config dict plus optional signer, row `[08a]`), `Context.builder() -> ContextBuilder` (the fluent `with_settings`/`with_signer`/`build` carrier, row `[09]`), `Context.has_signer() -> bool` (the bound-signer flag, row `[08b]`), and `Settings.from_dict(config) -> Settings` (settings from a config dict, row `[06]`) verify against the folder `c2pa-python` `.api` `[03]` signer/settings/module table; the per-instance `Context`/`Settings` configuration is the SDK trust-list and verify-policy carrier (`[04]-[IMPLEMENTATION_LAW]` boundary), so the `Context` value binds into both `Builder.from_json(context=...)` and `Reader.try_create(context=...)` rather than the deprecated thread-local `load_settings` module path (`.api` `[03]` row `[10]`, the deprecated thread-local path).
- [C2PA_CP315_INPROCESS] [RESOLVED]: the `c2pa` wheel is `py3-none-<platform>` with the native `libc2pa_c` core bundled per platform and loaded via `ctypes` (`.api` `[01]-[PACKAGE_SURFACE]` wheel line — `py3` ABI tag but NOT pure-Python, `macosx_11_0_arm64` the osx-arm64 wheel), `python_requires >=3.10`; the installed `c2pa.__version__` `0.35.1` over `c2pa-rs` core `0.88.0` (`sdk_version()`) imports clean on the cp315-core process, so this owner runs in-process with no `python_version<'3.15'` gated band and no `anyio.to_process.run_sync` subprocess seam — the synchronous SDK calls fold onto the structured-concurrency thread offload (`anyio.to_thread.run_sync`) so the `async_boundary` rail never blocks the event loop while the native core signs, and the `RuntimeRail`/`async_boundary` rail traps the `C2paError` family (the `ManifestNotFound`/`Signature`/`Verify` subclass arms discriminate codec/signature/verify faults). The pyproject `c2pa-python` admission (cross-format C2PA content credentials distinct from PAdES) carries no gate marker, matching the un-gated in-process posture.
- [PERSISTENCE_CONTENT_KEY_BINDING] [RESOLVED]: the signed-artifact `ContentKey` is the wire to the `csharp:Rasm.Persistence` store (ARCHITECTURE `[02]-[SEAMS]` `exchange/credential → csharp:Rasm.Persistence` — signed-artifact content-key binding decodes the `XxHash128` seed), so `_emit` mints `ContentIdentity.of(f"credential-{op.tag}", signed)` over the signed bytes and the persistence owner re-derives the same `XxHash128` seed over the identical signed bytes, the C2PA manifest and the content key co-identifying one durable artifact across the language boundary; the credential owner re-mints no canonical content key (the runtime `content_identity` owns it), so the binding is a key thread, never a parallel persistence identity.
- [RECEIPT_CREDENTIAL_CASE] [RESOLVED]: the `ArtifactReceipt.Credential` case carrying `tuple[ContentKey, str, str, int, str]` (key, c2pa manifest id, signer identity, assertion count, validation-state) is the settled case on the same-domain `core/receipt#RECEIPT` owner (ARCHITECTURE `[02]-[SEAMS]` `exchange/credential → python:runtime` content-credential `ArtifactReceipt` contribution), and `_emit` spreads `evidence.manifest_id`/`evidence.signer`/`evidence.assertions`/`evidence.validation_state` onto the `ArtifactReceipt.Credential(key, manifest_id, signer, assertions, validation_state)` constructor; the four-scalar contribution is flat scalars, not `tuple[ContentKey, CredentialEvidence]`, because `credential.py` imports `ArtifactReceipt` so a reciprocal `receipt.py` import of `CredentialEvidence` would close a module-scope cycle, mirroring the flat-scalar `Egress`/`Bundle` cases. The `Credential` case, its constructor, and its `_facts` arm are present on the settled `core/receipt#RECEIPT` owner (the receipt page's `Literal["...", "credential", "media"]` tag and the `Credential`/`_facts` arms), so this producer edge binds the landed five-field contract; the `CredentialEvidence.ingredients` count stays a producer-side evidence field the receipt fold does not carry.
- [CALLBACK_SIGNER_SEAM] [RESOLVED]: the `Signer.from_callback(callback, alg, certs, tsa_url=None)` external-callback signature is the `cryptography`/HSM seam (`.api` `[03]` signer row `[02]`, `[INTEGRATION_STACK]` callback-signer seam), and the `CallbackSigner.sign: Callable[[bytes], bytes]` field the `Callback` `SignerSpec` case carries returns the raw COSE signature bytes for a digest so private-key material lives in the `cryptography` keyring or HSM and `c2pa` never holds it; the campaign signer-configuration owner constructs the `CallbackSigner` with the concrete keyring/HSM-bound digest-signer as that field at composition, the same campaign-supplied-credential deferral the `exchange/conformance#CONFORMANCE` `ExternalSigner` injected-signature path uses, so the seam is one typed field on the case rather than an undefined module global. The `Callback` case threads only the alg token and the PEM cert chain across the SDK seam alongside the campaign callable; the `CertKey` case is the direct `C2paSignerInfo` cert/key path that needs no external callback. `Signer.from_callback(callback, alg: C2paSigningAlg, certs: str, tsa_url=None) -> Signer` verifies against the folder `c2pa-python` `.api` `[03]` signer row `[02]`.
