# [PY_ARTIFACTS_CONFORMANCE]

`Conformance` is the PDF cryptographic-conformance close over the document rail at the exchange boundary — ONE closed `tagged_union` whose `sign`/`stamp`/`augment`/`reserve`/`audit` cases fold a PDF emitted by `document/emit#DOCUMENT` into a PAdES-signed, document-timestamped, LTV-archival, seed-value-reserved, and audited `ConformanceVerdict`, the union owning the dispatch, the async entry, AND every case body directly as a method. `pyhanko` applies PAdES baseline signatures (B-B/B-T/B-LT/B-LTA) through `PdfSigner`; the `stamp` arm a signer-free RFC-3161 `/DocTimeStamp` proof-of-existence via `PdfTimeStamper.timestamp_pdf`; the `augment` arm LTV maintenance via `add_validation_info` + `update_archival_timestamp_chain`; the `reserve` arm a signer-less empty field carrying a `SigSeedValueSpec` seed-value policy (the multi-party prepare stage whose paired FILL arm is `sign`'s `FieldPlacement(reserved)` modality, so a licensed counterparty signs the constrained field rather than minting a second); and the `audit` arm resiliently folds EVERY embedded `/Sig` through `validate_pdf_signature` and EVERY `/DocTimeStamp` through `validate_pdf_timestamp`, a broken signature projected to `Nothing` (counted, never fatal), into one typed `ConformanceVerdict`.

`SignerSource`, the closed signer-credential union (`PemKey`/`Pkcs12Bundle`/`ExternalSig`), reads its tagged case through the `@beartype(conf=FAULT_CONF)`-woven `cms()` projector — `cms` for the CMS/CAdES container PAdES rides, mirroring the `exchange/credential#CREDENTIAL` sibling's `cose()`. `Conformance.close` is `async` over `async_boundary`, offloading the synchronous `pyhanko`/`pikepdf` calls through the thread lane under `RetryClass.OCCT` so the RFC-3161 timestamp and OCSP/CRL network I/O never block the loop, and returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]`. `ConformanceVerdict` is the one acyclic value-object edge `core/receipt#RECEIPT`'s `Verdict` case imports — `receipt.py` spreads `verdict.facts()`, never reciprocally importing `ArtifactReceipt`, the consumer minting `ArtifactReceipt.Verdict(key, verdict)`. This PAdES close is orthogonal to the `exchange/credential#CREDENTIAL` C2PA content-authenticity bind — a PDF routes here, a raster/BMFF/audio asset to the c2pa rail.

## [01]-[INDEX]

- [01]-[CONFORMANCE]: the PDF cryptographic-conformance owner that IS the closed `Conformance` union (`sign`/`stamp`/`augment`/`reserve`/`audit`) over the `SignerSource` credential union, the `Appearance`/`FieldPlacement`/`PadesLevel`/`Digest` policy vocabularies, and the resilient multi-signature audit folding one `ConformanceVerdict` the `core/receipt#RECEIPT` `Verdict` case carries.

## [02]-[CONFORMANCE]

- Owner: `Conformance` is the one close owner AND the closed `tagged_union` over its own `(bytes, spec)` payload — `sign`/`stamp`/`augment`/`reserve`/`audit` by one total `match`, the async `close`/`_produced`/`_run` entry, the shared `_sink` byte-emit fold every producing arm routes through, the shared `_audited` self-audit epilogue every case converges on, every case-body method folded onto the union, never a one-field wrapper over a separate op union nor a free module function. `SignerSource` reads its tagged case through the `@beartype(conf=FAULT_CONF)`-woven `cms()` (a malformed credential lifts onto the fault rail past the thread-offload), never parallel `key_file`/`pfx_file` nullable fields. `Appearance`'s `plan()` folds each invisible/visible case to the `PdfSigner` `stamp_style`, the matching `SigFieldSpec` kwarg, and the text-param dict in ONE `match`, so `_signed` drives the engine once across both modalities. `FieldPlacement` (`new`/`reserved`) is the field-provisioning family `sign` discriminates on — `new` a `NewField` placed at signing, `reserved` the FILL of a `reserve`-created empty seed-value field — so `_signed` targets fresh OR reserved on one drive and the reserve→fill lifecycle closes; the field-creation fields ride the `new` case alone, never dead weight on the fill path. `PadesLevel`'s derived `needs_timestamp`/`embeds_validation`/`archival` carry the LTV chain behavior and its `classify` folds the achieved level; `SignerConstraint`, `DssPolicy`, `Digest`, `SigKind`, `KeyUsage`/`ExtKeyUsage`, `Commitment`, `CertifyPerm`/`DiffMode` project their `pyhanko` counterparts through derived name- or value-correspondence tables, never hand-enumerated parallel maps. `_Tally`/`_Primary` are the one-pass aggregate-fold and primary-identity projection the audit folds the live statuses through. Two `document/tagged`-sourced booleans thread the spec — `structural_conformant` (the `document/tagged#ACCESS` PDF/UA pass) and the SECOND consumed `archival_conformant` (the `document/tagged#ARCHIVE` VALIDATED PDF/A close) — projected beside the self-declared `pdfa_claim`, the archival boolean threaded EXACTLY as the structural one. `pyhanko` owns the CMS/CAdES/PAdES engine, seed-value reservation, DSS/LTV embedding, RFC-3161 chaining, and validation; `pikepdf` owns the page-count and PDF/A·PDF/X claim read. `ConformanceVerdict` is declared here so it is leaf-imported by `core/receipt#RECEIPT` without a reciprocal import.
- Cases: `sign(pdf, SignSpec)` drives one `PdfSigner(stamp_style=).sign_pdf` over an `IncrementalPdfFileWriter` — the `FieldPlacement` create-or-FILL modality, the `Appearance` seal axis, the optional `Commitment`→`CAdESSignedAttrSpec`, the `signer_key_usage`/`DssPolicy` policies, the `HTTPTimeStamper` over `tsa_url`, the `ValidationContext` for B-LT/B-LTA · `stamp(pdf, StampSpec)` the signer-free RFC-3161 `/DocTimeStamp` via `PdfTimeStamper.timestamp_pdf` · `augment(pdf, AugmentSpec)` LTV maintenance — `add_validation_info` over every `/Sig` index then `update_archival_timestamp_chain` when `tsa_url` is supplied · `reserve(pdf, ReserveSpec)` the signer-free `append_signature_field` placing an empty `SigSeedValueSpec`-constrained field (the `SigSeedValFlags` set derived from populated axes, the optional `SignerConstraint` `/Cert` binding fencing the field to a named future signer) · `audit(pdf, AuditSpec)` the resilient multi-signature pass folding every `/Sig`/`/DocTimeStamp`, censusing the EMPTY fields through `enumerate_sig_fields(filled_status=False)` in the SAME read, projecting one `ConformanceVerdict` — one total `match`, never `is`-probes. `sign`/`augment`/`reserve` self-audit the produced PDF, so every op yields one verdict shape — a `reserve` verdict carries `fields_awaiting=1`, the honest audit of a field awaiting its signer that a paired `sign(placement=reserved)` fill drops to `0`.
- Auto: `_produced` folds the case through one total `match` binding the produced bytes and its `AuditSpec`, and `_run` self-audits through one shared `_audited` site so every arm converges on one verdict shape. Every producing arm emits through the one `_sink` fold rather than re-spelling the append-only sink ceremony per body. `_signed` drives ONE `PdfSigner` across both `FieldPlacement` modalities — `new` building `new_field_spec` with `existing_fields_only=False`, `reserved` passing `new_field_spec=None` and `existing_fields_only=True` to FILL the reserve-created field, the seal still riding `stamp_style`. `_audited` reads `embedded_signatures` ONCE and splits it through one `Block.partition` on `SigKind.SIGNATURE`, validates each half through the `_resilient[T]` trap composing `catch(exception=SignatureValidationError)` (a broken signature/stamp → `Nothing`, counted never fatal), folds the live statuses in ONE `live.fold(_Tally.step, ...)` accumulating every aggregate in a single traversal (the weakest-link `min` coverage admitting `None` as `UNCLEAR`, the worst-case `max` modification), projects the primary through ONE `_Primary.of` map, reads the DSS behind a `"/DSS" in reader.root` guard, and censuses the EMPTY fields through `enumerate_sig_fields(filled_status=False)` in the same read — never a nine-pass scatter or per-projector `try`/`except`.
- Receipt: every op produces one `ConformanceVerdict` the `core/receipt#RECEIPT` `ArtifactReceipt.Verdict` case carries as `tuple[ContentKey, ConformanceVerdict]`; `receipt.py` spreads `verdict.facts()` and this owner never reciprocally imports `ArtifactReceipt`, so the value-object edge is the single acyclic leaf edge — the `core/plan#PLAN` planner mints `ArtifactReceipt.Verdict(key, verdict)` from the returned pair. `facts()` derives through `structs.asdict(self)` — one edit site that cannot drift — projecting NATIVE scalars onto the `EventDict` so the `observability/metrics` `MeterProvider` and the log consumer read numbers and booleans, never pre-stringified text. `pdfa_claim` (the self-declared XMP claim, what the file writes about itself) and `archival_conformant` (the validated in-process `pdf_oxide.validate_pdf_a` Rust-oracle verdict the `document/tagged#ARCHIVE` owner PROVES) are distinct evidence carried side by side, so the archival-delivery plane reads a validated close, never a self-assertion.
- Packages: `pyhanko` (`SimpleSigner.load`/`load_pkcs12`, `ExternalSigner`, `load_certs_from_pemder`, `PdfSigner(...).sign_pdf(existing_fields_only=, appearance_text_params=, output=)` the deepest signing surface driving BOTH new-field placement and the `existing_fields_only=True` FILL of a reserve-created field, `IncrementalPdfFileWriter.write`, `PdfFileReader.embedded_signatures`, `HTTPTimeStamper`, `TimestampRequestError`, `PdfTimeStamper.update_archival_timestamp_chain`/`timestamp_pdf`, `PdfSignatureMetadata`, `append_signature_field`, `enumerate_sig_fields(filled_status=False)` the EMPTY-field census, `SigFieldSpec`, `SigSeedValueSpec`, `SigCertConstraints`/`SigCertKeyUsage`/`SigCertConstraintFlags`, `SigSeedValFlags`, `SigSeedSubFilter.PADES`, `FieldMDPSpec`/`MDPPerm`, `CAdESSignedAttrSpec`/`GenericCommitment.*.asn1`, `DSSContentSettings`/`SigDSSPlacementPreference`, `TextStampStyle`/`QRStampStyle`/`BaseStampStyle`, `add_validation_info`, `validate_pdf_signature`/`validate_pdf_timestamp`, `read_certification_data`, `DocumentSecurityStore.read_dss`, `KeyUsageConstraints`, `SignatureValidationError`, `PdfSignatureStatus`/`DocumentTimestampStatus`/`SignatureCoverageLevel`/`ModificationLevel`, `DEFAULT_DIFF_POLICY`/`NO_CHANGES_DIFF_POLICY`); `pyhanko_certvalidator` (`ValidationContext`, `SimpleCertificateStore.from_certs`); `asn1crypto` (`x509.Certificate.subject.human_friendly`/`issuer.human_friendly`/`serial_number`, `x509.Name.build`/`x509.KeyUsage` building the reserve-arm cert constraints); `pikepdf` (`open` + `pdfa_status`/`pdfx_status`); `msgspec` (`Struct(frozen=True[, gc=False])`, `structs.asdict` deriving `facts()`); `beartype`; `expression` (`tagged_union`, `Block` combinators, `Option`, `extra.result.catch`); `anyio`; stdlib (`functools.reduce`/`operator.or_` folding the flag sets); runtime (`identity`, `faults`, `RetryClass.OCCT`).
- Growth: a new operation is one `Conformance` case plus one `_run` arm plus one case-body method; a new signer seam one `SignerSource` case plus one `cms()` arm; a new PAdES level one `PadesLevel` row; a new digest, commitment, certification, modification, key-usage, or signature-object kind one `Digest`/`Commitment`/`CertifyPerm`/`DiffMode`/`KeyUsage`/`ExtKeyUsage`/`SigKind` row the derived table picks up by correspondence; a new seed-value constraint one `ReserveSpec` field plus one `_SEED_FLAG` row plus one `SigSeedValueSpec` argument; a new audit aggregate one `_Tally` field plus one `step` term plus one `ConformanceVerdict` field; a new primary-identity fact one `_Primary` field plus one `of` term plus one field, all picked up through `structs.asdict`; a new field-provisioning modality one `FieldPlacement` case plus one `_signed` arm; a new appearance modality one `Appearance` case plus one `plan()` arm; a new upstream conformance verdict one consumed boolean the spec threads plus one `ConformanceVerdict` field, projected exactly as `structural_conformant`/`archival_conformant`; a new subject-DN component one `DnField` row; a new DSS-placement mode one `DssPlacement` row plus one `_DSS_PLACEMENT` entry; a transient network fault widens `_TRANSIENT`; zero new surface.
- Boundary: no PDF authoring (`document/emit#DOCUMENT`), no font engineering (`typography/font#FONT`), no glyph rendering (`typography/shape#SHAPE`) — the owner closes an already-emitted PDF and prepares its signature fields, never producing document content. Signature-field reservation is in-lane because the `SigSeedValueSpec`/`SigCertConstraints` seed-value policy is a cryptographic field contract `document/emit` cannot express, never generic page authoring. `pyhanko` does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF — so the structural verdict is authored upstream at `document/tagged#ACCESS` and the `audit` arm CONSUMES that boolean, folding it into `ConformanceVerdict.structural_conformant` rather than claiming a veraPDF-grade verdict no pure-Python validator resolves; the `pdfa_claim`/`pdfx_claim` read from the `pikepdf` XMP are the document's OWN declared claims (an evidence read), never a validated verdict. `typography/font#FONT` `EMBED_AUDIT` supplies the embed-completeness precondition the PDF/A close requires; the `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary, never built here. Archival evidence is not self-declared only — the SECOND consumed `archival_conformant` carries the upstream `pdf_oxide.validate_pdf_a` Rust-oracle VALIDATED close beside the self-asserted `pdfa_claim`, threaded exactly as `structural_conformant`, never a duplicated veraPDF subprocess.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from io import BytesIO
from operator import or_
from typing import Final, Literal, Self, assert_never

import pikepdf
from anyio import CapacityLimiter, to_thread
from asn1crypto import x509
from beartype import beartype
from expression import Option, case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import catch
from msgspec import Struct, structs
from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
from pyhanko.pdf_utils.reader import PdfFileReader
from pyhanko.sign import ExternalSigner, PdfSignatureMetadata, PdfSigner, PdfTimeStamper, Signer, SimpleSigner, load_certs_from_pemder
from pyhanko.sign.ades.api import CAdESSignedAttrSpec, GenericCommitment
from pyhanko.sign.diff_analysis import DEFAULT_DIFF_POLICY, NO_CHANGES_DIFF_POLICY, DiffPolicy, ModificationLevel
from pyhanko.sign.fields import (
    FieldMDPAction,
    FieldMDPSpec,
    InvisSigSettings,
    MDPPerm,
    SigCertConstraintFlags,
    SigCertConstraints,
    SigCertKeyUsage,
    SigFieldSpec,
    SigSeedSubFilter,
    SigSeedValFlags,
    SigSeedValueSpec,
    VisibleSigSettings,
    append_signature_field,
    enumerate_sig_fields,
)
from pyhanko.sign.signers.pdf_signer import DSSContentSettings, SigDSSPlacementPreference
from pyhanko.sign.timestamps import HTTPTimeStamper, TimestampRequestError
from pyhanko.sign.validation import (
    DocumentSecurityStore,
    EmbeddedPdfSignature,
    add_validation_info,
    read_certification_data,
    validate_pdf_signature,
    validate_pdf_timestamp,
)
from pyhanko.sign.validation.errors import SignatureValidationError
from pyhanko.sign.validation.settings import KeyUsageConstraints
from pyhanko.sign.validation.status import DocumentTimestampStatus, PdfSignatureStatus, SignatureCoverageLevel
from pyhanko.stamp import BaseStampStyle, QRPosition, QRStampStyle, TextStampStyle
from pyhanko_certvalidator import ValidationContext
from pyhanko_certvalidator.registry import SimpleCertificateStore

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary


# --- [TYPES] ----------------------------------------------------------------------------
class PadesLevel(StrEnum):
    B_B = "B-B"
    B_T = "B-T"
    B_LT = "B-LT"
    B_LTA = "B-LTA"

    @property
    def needs_timestamp(self) -> bool:
        return self is not PadesLevel.B_B

    @property
    def embeds_validation(self) -> bool:
        return self in (PadesLevel.B_LT, PadesLevel.B_LTA)

    @property
    def archival(self) -> bool:
        return self is PadesLevel.B_LTA

    @staticmethod
    def classify(ltv: bool, archival_valid: bool, timestamp_valid: bool, /) -> "PadesLevel":
        return PadesLevel.B_LTA if ltv and archival_valid else PadesLevel.B_LT if ltv else PadesLevel.B_T if timestamp_valid else PadesLevel.B_B


class Digest(StrEnum):
    SHA256 = "sha256"
    SHA384 = "sha384"
    SHA512 = "sha512"
    SHA3_256 = "sha3_256"
    SHA3_384 = "sha3_384"
    SHA3_512 = "sha3_512"


class SigKind(StrEnum):
    SIGNATURE = "/Sig"
    TIMESTAMP = "/DocTimeStamp"


class CertifyPerm(StrEnum):
    NO_CHANGES = "no_changes"
    FILL_FORMS = "fill_forms"
    ANNOTATE = "annotate"


class Commitment(StrEnum):
    ORIGIN = "proof_of_origin"
    RECEIPT = "proof_of_receipt"
    DELIVERY = "proof_of_delivery"
    SENDER = "proof_of_sender"
    APPROVAL = "proof_of_approval"
    CREATION = "proof_of_creation"


class DiffMode(StrEnum):
    DEFAULT = "default"
    STRICT = "strict"


class KeyUsage(StrEnum):
    DIGITAL_SIGNATURE = "digital_signature"
    NON_REPUDIATION = "non_repudiation"
    KEY_ENCIPHERMENT = "key_encipherment"
    DATA_ENCIPHERMENT = "data_encipherment"
    KEY_AGREEMENT = "key_agreement"
    KEY_CERT_SIGN = "key_cert_sign"
    CRL_SIGN = "crl_sign"
    ENCIPHER_ONLY = "encipher_only"
    DECIPHER_ONLY = "decipher_only"


class ExtKeyUsage(StrEnum):
    DOCUMENT_SIGNING = "1.3.6.1.5.5.7.3.36"  # id-kp-documentSigning, the PAdES EKU asn1crypto has no name for; carried as OID
    CODE_SIGNING = "code_signing"
    EMAIL_PROTECTION = "email_protection"
    TIME_STAMPING = "time_stamping"
    OCSP_SIGNING = "ocsp_signing"


# member values are the `asn1crypto` X.500 RDN attribute names `x509.Name.build` keys on, so the reserved
# field's future-signer subject constraint is a typed `frozendict[DnField, str]`, never a bare-string DN bag.
class DnField(StrEnum):
    COMMON_NAME = "common_name"
    COUNTRY = "country_name"
    ORGANIZATION = "organization_name"
    ORGANIZATIONAL_UNIT = "organizational_unit_name"
    LOCALITY = "locality_name"
    STATE = "state_or_province_name"
    SERIAL_NUMBER = "serial_number"
    EMAIL = "email_address"


# member names align with `SigDSSPlacementPreference`, so `_DSS_PLACEMENT` derives each row by name.
class DssPlacement(StrEnum):
    TOGETHER_WITH_SIGNATURE = "together_with_signature"
    SEPARATE_REVISION = "separate_revision"
    TOGETHER_WITH_NEXT_TS = "together_with_next_ts"


# --- [TABLES] ---------------------------------------------------------------------------
# each row derives from the vocabulary member, never a hand-enumerated parallel map: `_MDP`/`_DSS_PLACEMENT`
# by name, `_COMMITMENT` by value-to-`GenericCommitment`-name; `_DIFF` binds the two `diff_analysis` instances.
_MDP: Final[Map[CertifyPerm, MDPPerm]] = Map.of_seq((perm, MDPPerm[perm.name]) for perm in CertifyPerm)
_COMMITMENT: Final[Map[Commitment, GenericCommitment]] = Map.of_seq((c, GenericCommitment[c.value.upper()]) for c in Commitment)
_DIFF: Final[Map[DiffMode, DiffPolicy]] = Map.of_seq([(DiffMode.DEFAULT, DEFAULT_DIFF_POLICY), (DiffMode.STRICT, NO_CHANGES_DIFF_POLICY)])
_DSS_PLACEMENT: Final[Map[DssPlacement, SigDSSPlacementPreference]] = Map.of_seq((p, SigDSSPlacementPreference[p.name]) for p in DssPlacement)
_TRANSIENT: Final[tuple[type[Exception], ...]] = (TimestampRequestError,)


# --- [MODELS] ---------------------------------------------------------------------------
class PemKey(Struct, frozen=True):
    key_file: str
    cert_file: str
    ca_chain: tuple[str, ...] = ()
    passphrase: bytes | None = None


class Pkcs12Bundle(Struct, frozen=True):
    pfx_file: str
    passphrase: bytes | None = None
    ca_chain: tuple[str, ...] = ()


class ExternalSig(Struct, frozen=True):
    cert_file: str
    signature_value: bytes
    ca_chain: tuple[str, ...] = ()


@tagged_union(frozen=True)
class SignerSource:
    tag: Literal["pem", "pkcs12", "external"] = tag()
    pem: PemKey = case()
    pkcs12: Pkcs12Bundle = case()
    external: ExternalSig = case()

    @beartype(conf=FAULT_CONF)
    def cms(self) -> Signer:
        match self:
            case SignerSource(tag="pem", pem=key):
                return SimpleSigner.load(key.key_file, key.cert_file, ca_chain_files=list(key.ca_chain) or None, key_passphrase=key.passphrase)
            case SignerSource(tag="pkcs12", pkcs12=bundle):
                return SimpleSigner.load_pkcs12(bundle.pfx_file, ca_chain_files=list(bundle.ca_chain) or None, passphrase=bundle.passphrase)
            case SignerSource(tag="external", external=ext):
                chain = load_certs_from_pemder([ext.cert_file, *ext.ca_chain])
                return ExternalSigner(
                    signing_cert=chain[0], cert_registry=SimpleCertificateStore.from_certs(chain), signature_value=ext.signature_value
                )
            case _ as unreachable:
                assert_never(unreachable)


class AuditSpec(Struct, frozen=True):
    signer_context: ValidationContext | None = None
    ts_context: ValidationContext | None = None
    diff_mode: DiffMode = DiffMode.DEFAULT
    required_key_usage: tuple[KeyUsage, ...] = ()
    required_extd_key_usage: tuple[ExtKeyUsage, ...] = ()
    # the two upstream verdicts consumed via the spec (pyhanko treats PDF/A and PDF/UA as ordinary PDF):
    # `structural_conformant` the `document/tagged#ACCESS` PDF/UA pass, `archival_conformant` the SECOND consumed
    # boolean — the `document/tagged#ARCHIVE` VALIDATED `pdf_oxide.validate_pdf_a` close, threaded EXACTLY as it.
    structural_conformant: bool = False
    archival_conformant: bool = False

    def usage(self) -> KeyUsageConstraints | None:
        return (
            KeyUsageConstraints(key_usage=frozenset(self.required_key_usage) or None, extd_key_usage=frozenset(self.required_extd_key_usage) or None)
            if self.required_key_usage or self.required_extd_key_usage
            else None
        )


# the field-appearance axis: the `invisible` case an `InvisSigSettings` flag set, the `visible` case a
# `VisibleSeal` whose `qr_position` selects `TextStampStyle` (positioned seal) or `QRStampStyle` (QR encoding
# the `%(url)s` link). `plan()` folds each case to one stamp style + settings kwarg + text params in ONE match.
class VisibleSeal(Struct, frozen=True):
    stamp_text: str = "%(signer)s\n%(ts)s"
    text_params: frozendict[str, str] = frozendict()
    border_width: int = 3
    background_opacity: float = 0.6
    rotate_with_page: bool = True
    scale_with_page_zoom: bool = True
    print_signature: bool = True
    # a `QRPosition` selects the scan-to-verify `QRStampStyle` whose rendered QR encodes the `%(url)s`
    # `appearance_text_params` field (the machine-verifiable ISO-19650 sealed-delivery link); `None` keeps
    # the plain `TextStampStyle` positioned seal — the value carries the modality, never a `qr: bool` knob.
    qr_position: QRPosition | None = None

    def style(self) -> BaseStampStyle:
        return (
            QRStampStyle(
                stamp_text=self.stamp_text, border_width=self.border_width, background_opacity=self.background_opacity, qr_position=self.qr_position
            )
            if self.qr_position is not None
            else TextStampStyle(stamp_text=self.stamp_text, border_width=self.border_width, background_opacity=self.background_opacity)
        )

    def settings(self) -> VisibleSigSettings:
        return VisibleSigSettings(
            rotate_with_page=self.rotate_with_page, scale_with_page_zoom=self.scale_with_page_zoom, print_signature=self.print_signature
        )


class _SealPlan(Struct, frozen=True, gc=False):
    stamp: BaseStampStyle | None
    field_settings: frozendict[str, InvisSigSettings | VisibleSigSettings]
    text_params: frozendict[str, str]


@tagged_union(frozen=True)
class Appearance:
    tag: Literal["invisible", "visible"] = tag()
    invisible: InvisSigSettings = case()
    visible: VisibleSeal = case()

    def plan(self) -> _SealPlan:
        match self:
            case Appearance(tag="invisible", invisible=settings):
                return _SealPlan(stamp=None, field_settings=frozendict({"invis_sig_settings": settings}), text_params=frozendict())
            case Appearance(tag="visible", visible=seal):
                return _SealPlan(
                    stamp=seal.style(), field_settings=frozendict({"visible_sig_settings": seal.settings()}), text_params=seal.text_params
                )
            case _ as unreachable:
                assert_never(unreachable)


_INVISIBLE: Final[Appearance] = Appearance(invisible=InvisSigSettings())


# the sign-time DSS/VRI write policy: `include_vri` toggles the per-signature VRI dictionary the
# `add_validation_info` DSS write emits, `placement` selects where the DSS revision lands relative to
# the signature and next timestamp — projected to one `DSSContentSettings`, never a bare bool pair.
class DssPolicy(Struct, frozen=True):
    include_vri: bool = True
    placement: DssPlacement = DssPlacement.TOGETHER_WITH_NEXT_TS

    def settings(self) -> DSSContentSettings:
        return DSSContentSettings(include_vri=self.include_vri, placement=_DSS_PLACEMENT[self.placement])


_DSS: Final[DssPolicy] = DssPolicy()


# the field-provisioning modality `sign` discriminates on: `new` places a fresh field (`existing_fields_only=
# False`), `reserved` FILLS the empty seed-value field a prior `reserve` created (`new_field_spec=None`,
# `existing_fields_only=True`), completing the reserve->fill lifecycle; the field-creation fields ride `new` alone.
class NewField(Struct, frozen=True):
    box: tuple[int, int, int, int] | None = None
    page: int = 0
    lock_fields: tuple[str, ...] = ()


@tagged_union(frozen=True)
class FieldPlacement:
    tag: Literal["new", "reserved"] = tag()
    new: NewField = case()
    reserved: None = case()


_NEW_FIELD: Final[FieldPlacement] = FieldPlacement(new=NewField())


class SignSpec(Struct, frozen=True):
    signer: SignerSource
    pades_level: PadesLevel = PadesLevel.B_LTA
    field_name: str = "Signature1"
    placement: FieldPlacement = _NEW_FIELD
    certify: CertifyPerm | None = None
    commitment: Commitment | None = None
    reason: str | None = None
    location: str | None = None
    contact_info: str | None = None
    name: str | None = None
    md_algorithm: Digest = Digest.SHA256
    tsa_url: str | None = None
    validation_context: ValidationContext | None = None
    appearance: Appearance = _INVISIBLE
    signer_key_usage: tuple[KeyUsage, ...] = (KeyUsage.NON_REPUDIATION,)
    dss: DssPolicy = _DSS
    structural_conformant: bool = False
    archival_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(
            signer_context=self.validation_context,
            ts_context=self.validation_context,
            structural_conformant=self.structural_conformant,
            archival_conformant=self.archival_conformant,
        )


class StampSpec(Struct, frozen=True):
    tsa_url: str
    md_algorithm: Digest = Digest.SHA256
    validation_context: ValidationContext | None = None
    structural_conformant: bool = False
    archival_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(
            ts_context=self.validation_context, structural_conformant=self.structural_conformant, archival_conformant=self.archival_conformant
        )


class AugmentSpec(Struct, frozen=True):
    validation_context: ValidationContext
    tsa_url: str | None = None
    structural_conformant: bool = False
    archival_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(
            signer_context=self.validation_context,
            ts_context=self.validation_context,
            structural_conformant=self.structural_conformant,
            archival_conformant=self.archival_conformant,
        )


# the future-signer identity constraint the `reserve` arm binds into the field's seed-value `/Cert`: only a
# cert matching the subject DN and the named key-usage bits may fill the field. The subject DN a typed
# `frozendict[DnField, str]` fed to `x509.Name.build`, never a bare-string DN bag.
class SignerConstraint(Struct, frozen=True):
    subject_dn: frozendict[DnField, str] = frozendict()
    required_key_usage: tuple[KeyUsage, ...] = ()
    forbidden_key_usage: tuple[KeyUsage, ...] = ()

    def constraints(self) -> SigCertConstraints:
        usage = SigCertKeyUsage(
            must_have=x509.KeyUsage({ku.value for ku in self.required_key_usage}) if self.required_key_usage else None,
            forbidden=x509.KeyUsage({ku.value for ku in self.forbidden_key_usage}) if self.forbidden_key_usage else None,
        )
        return SigCertConstraints(
            flags=reduce(or_, (flag for flag, populated in _CERT_FLAG.items() if populated(self)), SigCertConstraintFlags(0)),
            subject_dn=x509.Name.build({field.value: value for field, value in self.subject_dn.items()}) if self.subject_dn else None,
            key_usage=[usage] if self.required_key_usage or self.forbidden_key_usage else None,
        )


# one row per `SigCertConstraintFlags` mandatory bit keyed to the `SignerConstraint` axis whose presence
# makes it binding; `reduce(or_, ...)` folds the populated bits exactly as `_SEED_FLAG` folds the seed flags.
_CERT_FLAG: Final[Map[SigCertConstraintFlags, Callable[[SignerConstraint], bool]]] = Map.of_seq([
    (SigCertConstraintFlags.SUBJECT_DN, lambda c: bool(c.subject_dn)),
    (SigCertConstraintFlags.KEY_USAGE, lambda c: bool(c.required_key_usage or c.forbidden_key_usage)),
])


class ReserveSpec(Struct, frozen=True):
    field_name: str = "Signature1"
    field_box: tuple[int, int, int, int] | None = None
    page: int = 0
    pades_only: bool = False
    digest_methods: tuple[Digest, ...] = ()
    timestamp_required: bool = False
    tsa_url: str | None = None
    add_rev_info: bool = False
    signer_constraint: SignerConstraint | None = None
    structural_conformant: bool = False
    archival_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(structural_conformant=self.structural_conformant, archival_conformant=self.archival_conformant)


# the mandatory-flag set the reserved field demands of its future signer, one row per `SigSeedValFlags`
# bit keyed to the `ReserveSpec` field whose presence makes that constraint binding; `reduce(or_)`
# folds the populated bits, never a parallel flag argument the body re-derives.
_SEED_FLAG: Final[Map[SigSeedValFlags, Callable[[ReserveSpec], bool]]] = Map.of_seq([
    (SigSeedValFlags.SUBFILTER, lambda spec: spec.pades_only),
    (SigSeedValFlags.DIGEST_METHOD, lambda spec: bool(spec.digest_methods)),
    (SigSeedValFlags.ADD_REV_INFO, lambda spec: spec.add_rev_info),
])


class _Tally(Struct, frozen=True, gc=False):
    valid: int = 0
    trusted: int = 0
    revoked: bool = False
    all_valid: bool = True
    all_trusted: bool = True
    docmdp_ok: bool = True
    seed_value_ok: bool = True
    timestamps_trusted: bool = True
    coverage: SignatureCoverageLevel = SignatureCoverageLevel.ENTIRE_FILE  # min-identity: weakest-link folds down
    modification: ModificationLevel = ModificationLevel.NONE  # max-identity: worst-case folds up

    @staticmethod
    def step(acc: "_Tally", status: PdfSignatureStatus, /) -> "_Tally":
        ts = status.timestamp_validity
        return _Tally(
            valid=acc.valid + status.bottom_line,
            trusted=acc.trusted + status.trusted,
            revoked=acc.revoked or status.revoked,
            all_valid=acc.all_valid and status.bottom_line,
            all_trusted=acc.all_trusted and status.trusted,
            docmdp_ok=acc.docmdp_ok and status.docmdp_ok is not False,
            seed_value_ok=acc.seed_value_ok and status.seed_value_ok is not False,
            timestamps_trusted=acc.timestamps_trusted and ts is not None and ts.trusted,
            coverage=min(acc.coverage, status.coverage if status.coverage is not None else SignatureCoverageLevel.UNCLEAR),
            modification=max(acc.modification, status.modification_level if status.modification_level is not None else ModificationLevel.NONE),
        )


class _Primary(Struct, frozen=True, gc=False):
    signer_subject: str = ""
    signer_issuer: str = ""
    signer_serial: str = ""
    digest_algorithm: str = ""
    signature_mechanism: str = ""
    signed_at: str = ""
    timestamp_at: str = ""
    content_timestamp_valid: bool = False
    qualified: bool = False

    @staticmethod
    def of(status: PdfSignatureStatus, /) -> "_Primary":
        cert, ts, content_ts, qual = status.signing_cert, status.timestamp_validity, status.content_timestamp_validity, status.qualification_result
        return _Primary(
            signer_subject=cert.subject.human_friendly,
            signer_issuer=cert.issuer.human_friendly,
            signer_serial=str(cert.serial_number),
            digest_algorithm=status.md_algorithm,
            signature_mechanism=status.pkcs7_signature_mechanism,
            signed_at=status.signer_reported_dt.isoformat() if status.signer_reported_dt is not None else "",
            timestamp_at=ts.timestamp.isoformat() if ts is not None else "",
            content_timestamp_valid=content_ts is not None and content_ts.trusted,
            qualified=qual is not None and qual.status.qualified,
        )


class ConformanceVerdict(Struct, frozen=True, gc=False):
    pades_level: str
    pages: int
    signatures: int
    timestamps: int
    fields_awaiting: int
    signatures_valid: int
    signatures_trusted: int
    signatures_broken: int
    signature_valid: bool
    trusted: bool
    revoked: bool
    coverage_level: str
    modification_level: str
    docmdp_ok: bool
    seed_value_ok: bool
    certification_level: str
    signer_subject: str
    signer_issuer: str
    signer_serial: str
    digest_algorithm: str
    signature_mechanism: str
    signed_at: str
    timestamp_at: str
    timestamp_valid: bool
    content_timestamp_valid: bool
    archival_timestamps_valid: bool
    qualified: bool
    ltv_complete: bool
    dss_certs: int
    dss_ocsps: int
    dss_crls: int
    dss_vri: int
    structural_conformant: bool
    archival_conformant: bool
    pdfa_claim: str
    pdfx_claim: str

    def facts(self) -> dict[str, object]:
        return structs.asdict(self)


@tagged_union(frozen=True)
class Conformance:
    tag: Literal["sign", "stamp", "augment", "reserve", "audit"] = tag()
    sign: tuple[bytes, SignSpec] = case()
    stamp: tuple[bytes, StampSpec] = case()
    augment: tuple[bytes, AugmentSpec] = case()
    reserve: tuple[bytes, ReserveSpec] = case()
    audit: tuple[bytes, AuditSpec] = case()

    @classmethod
    def Sign(cls, pdf: bytes, spec: SignSpec, /) -> Self:
        return cls(sign=(pdf, spec))

    @classmethod
    def Stamp(cls, pdf: bytes, spec: StampSpec, /) -> Self:
        return cls(stamp=(pdf, spec))

    @classmethod
    def Augment(cls, pdf: bytes, spec: AugmentSpec, /) -> Self:
        return cls(augment=(pdf, spec))

    @classmethod
    def Reserve(cls, pdf: bytes, spec: ReserveSpec, /) -> Self:
        return cls(reserve=(pdf, spec))

    @classmethod
    def Audit(cls, pdf: bytes, spec: AuditSpec, /) -> Self:
        return cls(audit=(pdf, spec))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical close request minted PRE-RUN — the signed payload's own address rides the verdict facts.
        return ContentIdentity.of(f"conformance-{self.tag}", self, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the pyhanko close crosses the thread lane.
        railed = await async_boundary(f"conformance.{self.tag}", self._closed)
        return railed.map(lambda verdict: ArtifactReceipt.Verdict(self._key, verdict))

    async def _closed(self) -> ConformanceVerdict:
        crossed = await LanePolicy.offload(self._run, modality=Modality.THREAD, retry=RetryClass.OCCT)
        _payload, verdict = crossed.default_with(_conformance_raise)
        return verdict

    def _run(self) -> tuple[bytes, ConformanceVerdict]:
        produced, audit = self._produced()
        return produced, self._audited(produced, audit)

    def _produced(self) -> tuple[bytes, AuditSpec]:
        match self:
            case Conformance(tag="sign", sign=(pdf, spec)):
                return self._signed(pdf, spec), spec.audit()
            case Conformance(tag="stamp", stamp=(pdf, spec)):
                return self._timestamped(pdf, spec), spec.audit()
            case Conformance(tag="augment", augment=(pdf, spec)):
                return self._augmented(pdf, spec), spec.audit()
            case Conformance(tag="reserve", reserve=(pdf, spec)):
                return self._reserved(pdf, spec), spec.audit()
            case Conformance(tag="audit", audit=(pdf, spec)):
                return pdf, spec
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _sink(write: Callable[[BytesIO], object], /) -> bytes:
        sink = BytesIO()
        write(sink)
        return sink.getvalue()

    def _signed(self, pdf: bytes, spec: SignSpec, /) -> bytes:
        level, plan = spec.pades_level, spec.appearance.plan()
        meta = PdfSignatureMetadata(
            field_name=spec.field_name,
            md_algorithm=spec.md_algorithm,
            subfilter=SigSeedSubFilter.PADES,
            reason=spec.reason,
            location=spec.location,
            contact_info=spec.contact_info,
            name=spec.name,
            certify=spec.certify is not None,
            docmdp_permissions=_MDP[spec.certify] if spec.certify is not None else None,
            cades_signed_attr_spec=CAdESSignedAttrSpec(commitment_type=_COMMITMENT[spec.commitment].asn1) if spec.commitment is not None else None,
            signer_key_usage=frozenset(usage.value for usage in spec.signer_key_usage),
            dss_settings=spec.dss.settings(),
            use_pades_lta=level.archival,
            embed_validation_info=level.embeds_validation,
            validation_context=spec.validation_context,
        )
        timestamper = HTTPTimeStamper(spec.tsa_url) if level.needs_timestamp and spec.tsa_url else None
        # ONE `PdfSigner` drives both modalities: `new` places a fresh field, `reserved` FILLS the reserve-created
        # seed-value field (`existing_fields_only=True`), the seal still riding `stamp_style`.
        match spec.placement:
            case FieldPlacement(tag="new", new=placed):
                new_field: SigFieldSpec | None = SigFieldSpec(
                    sig_field_name=spec.field_name,
                    on_page=placed.page,
                    box=placed.box,
                    field_mdp_spec=FieldMDPSpec(FieldMDPAction.INCLUDE, list(placed.lock_fields)) if placed.lock_fields else None,
                    **plan.field_settings,
                )
                existing_only = False
            case FieldPlacement(tag="reserved"):
                new_field, existing_only = None, True
            case _ as unreachable:
                assert_never(unreachable)
        engine = PdfSigner(meta, spec.signer.cms(), timestamper=timestamper, stamp_style=plan.stamp, new_field_spec=new_field)
        return self._sink(
            lambda sink: engine.sign_pdf(
                IncrementalPdfFileWriter(BytesIO(pdf)),
                existing_fields_only=existing_only,
                appearance_text_params=dict(plan.text_params) or None,
                output=sink,
            )
        )

    def _timestamped(self, pdf: bytes, spec: StampSpec, /) -> bytes:
        return self._sink(
            lambda sink: PdfTimeStamper(HTTPTimeStamper(spec.tsa_url)).timestamp_pdf(
                IncrementalPdfFileWriter(BytesIO(pdf)), spec.md_algorithm, spec.validation_context, output=sink
            )
        )

    def _augmented(self, pdf: bytes, spec: AugmentSpec, /) -> bytes:
        indices = Block.of_seq(
            i for i, sig in enumerate(PdfFileReader(BytesIO(pdf)).embedded_signatures) if str(sig.sig_object_type) == SigKind.SIGNATURE
        )

        def embedded(current: bytes, index: int, /) -> bytes:
            return self._sink(
                lambda sink: add_validation_info(
                    PdfFileReader(BytesIO(current)).embedded_signatures[index], spec.validation_context, in_place=False, output=sink
                )
            )

        enriched = indices.fold(embedded, pdf)
        return self._refreshed(enriched, spec.tsa_url, spec.validation_context) if spec.tsa_url else enriched

    def _refreshed(self, pdf: bytes, tsa_url: str, context: ValidationContext, /) -> bytes:
        return self._sink(
            lambda sink: PdfTimeStamper(HTTPTimeStamper(tsa_url)).update_archival_timestamp_chain(
                PdfFileReader(BytesIO(pdf)), context, in_place=False, output=sink
            )
        )

    def _reserved(self, pdf: bytes, spec: ReserveSpec, /) -> bytes:
        seed = SigSeedValueSpec(
            flags=reduce(or_, (flag for flag, populated in _SEED_FLAG.items() if populated(spec)), SigSeedValFlags(0)),
            subfilters=[SigSeedSubFilter.PADES] if spec.pades_only else None,
            digest_methods=[digest.value for digest in spec.digest_methods] or None,
            timestamp_required=spec.timestamp_required,
            timestamp_server_url=spec.tsa_url,
            add_rev_info=spec.add_rev_info or None,
            cert=spec.signer_constraint.constraints() if spec.signer_constraint is not None else None,
        )
        writer = IncrementalPdfFileWriter(BytesIO(pdf))
        append_signature_field(writer, SigFieldSpec(sig_field_name=spec.field_name, on_page=spec.page, box=spec.field_box, seed_value_dict=seed))
        return self._sink(writer.write)

    @staticmethod
    def _resilient[T](validate: Callable[[], T], /) -> Option[T]:
        # a structurally broken /Sig or /DocTimeStamp is counted, never fatal: the substrate trap mints
        # Result[T, SignatureValidationError] from the raise, narrowed to Nothing; an unlisted raise propagates.
        return catch(exception=SignatureValidationError)(validate)().to_option()

    def _validated(self, sig: EmbeddedPdfSignature, spec: AuditSpec, /) -> Option[PdfSignatureStatus]:
        return self._resilient(
            lambda: validate_pdf_signature(
                sig,
                signer_validation_context=spec.signer_context,
                ts_validation_context=spec.ts_context,
                diff_policy=_DIFF[spec.diff_mode],
                key_usage_settings=spec.usage(),
            )
        )

    def _stamped(self, ts: EmbeddedPdfSignature, spec: AuditSpec, /) -> Option[DocumentTimestampStatus]:
        return self._resilient(lambda: validate_pdf_timestamp(ts, validation_context=spec.ts_context, diff_policy=_DIFF[spec.diff_mode]))

    def _audited(self, pdf: bytes, spec: AuditSpec, /) -> ConformanceVerdict:
        reader = PdfFileReader(BytesIO(pdf))
        # census the EMPTY /Sig fields (`filled_status=False`) the filled `embedded_signatures` fold cannot see,
        # so the reserve->fill lifecycle is observable — `fields_awaiting` 1 after a reserve, 0 once filled.
        awaiting = Block.of_seq(enumerate_sig_fields(reader, filled_status=False))
        signatures, doc_timestamps = Block.of_seq(reader.embedded_signatures).partition(lambda s: str(s.sig_object_type) == SigKind.SIGNATURE)
        live = signatures.choose(lambda s: self._validated(s, spec))
        archival = doc_timestamps.choose(lambda ts: self._stamped(ts, spec))
        tally = live.fold(_Tally.step, _Tally())
        primary = live.try_head().map(_Primary.of).default_value(_Primary())
        certification = read_certification_data(reader)
        dss = DocumentSecurityStore.read_dss(reader) if "/DSS" in reader.root else None
        complete = not signatures.is_empty() and len(live) == len(signatures)
        timestamp_valid = not live.is_empty() and tally.timestamps_trusted
        archival_valid = not doc_timestamps.is_empty() and len(archival) == len(doc_timestamps) and all(stamp.trusted for stamp in archival)
        ltv_complete = dss is not None and bool(dss.certs)
        with pikepdf.open(BytesIO(pdf)) as document, document.open_metadata(set_pikepdf_as_editor=False) as xmp:
            pages, pdfa_claim, pdfx_claim = len(document.pages), xmp.pdfa_status or "", xmp.pdfx_status or ""
        return ConformanceVerdict(
            pades_level=PadesLevel.classify(ltv_complete, archival_valid, timestamp_valid).value,
            pages=pages,
            signatures=len(signatures),
            timestamps=len(doc_timestamps),
            fields_awaiting=len(awaiting),
            signatures_valid=tally.valid,
            signatures_trusted=tally.trusted,
            signatures_broken=len(signatures) - len(live),
            signature_valid=complete and tally.all_valid,
            trusted=complete and tally.all_trusted,
            revoked=tally.revoked,
            coverage_level=(tally.coverage if not live.is_empty() else SignatureCoverageLevel.UNCLEAR).name,
            modification_level=tally.modification.name,
            docmdp_ok=tally.docmdp_ok,
            seed_value_ok=tally.seed_value_ok,
            certification_level=certification.permission.name if certification is not None else "none",
            signer_subject=primary.signer_subject,
            signer_issuer=primary.signer_issuer,
            signer_serial=primary.signer_serial,
            digest_algorithm=primary.digest_algorithm,
            signature_mechanism=primary.signature_mechanism,
            signed_at=primary.signed_at,
            timestamp_at=primary.timestamp_at,
            timestamp_valid=timestamp_valid,
            content_timestamp_valid=primary.content_timestamp_valid,
            archival_timestamps_valid=archival_valid,
            qualified=primary.qualified,
            ltv_complete=ltv_complete,
            dss_certs=len(dss.certs) if dss is not None else 0,
            dss_ocsps=len(dss.ocsps) if dss is not None else 0,
            dss_crls=len(dss.crls) if dss is not None else 0,
            dss_vri=len(dss.vri_entries) if dss is not None else 0,
            structural_conformant=spec.structural_conformant,
            archival_conformant=spec.archival_conformant,
            pdfa_claim=pdfa_claim,
            pdfx_claim=pdfx_claim,
        )


# --- [COMPOSITION] ----------------------------------------------------------------------
# every TSA-signing/audit offload threads one explicit thread bound, not the per-loop default — the GIL-releasing
# pyhanko/pikepdf render and the RFC-3161/OCSP network seam run off the event loop, bounded at the boundary.
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
