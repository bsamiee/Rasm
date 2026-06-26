# [PY_ARTIFACTS_CONFORMANCE]

The PDF cryptographic-conformance close over the document rail at the exchange boundary. `Conformance` is ONE owner — a closed `tagged_union` whose `sign`/`stamp`/`augment`/`audit` cases fold a PDF emitted by `document/emit#DOCUMENT` into a PAdES-signed, document-timestamped, LTV-archival, and audited `ConformanceVerdict` — the `tagged_union` owning the dispatch, the async entry, AND every case body directly as a method, with no one-field wrapper over a separate op union and no free case-body function beside it. `pyhanko` applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) under the `SigSeedSubFilter.PADES` subfilter with an optional `CAdESSignedAttrSpec` commitment-type signed attribute, embedded DSS validation material, an `HTTPTimeStamper` authority, a `Digest` digest-algorithm vocabulary, `MDPPerm` certification permissions, and `FieldMDPSpec` field-lock policy; the `stamp` arm applies a signer-free RFC-3161 `/DocTimeStamp` proof-of-existence timestamp through `PdfTimeStamper.timestamp_pdf`; the `augment` arm maintains long-term-archival validity through `add_validation_info` DSS embedding and `PdfTimeStamper.update_archival_timestamp_chain` archival refresh; and the `audit` arm RESILIENTLY folds EVERY embedded `/Sig` signature — a structurally broken signature counted, never fatal — through `validate_pdf_signature` and EVERY `/DocTimeStamp` archival stamp through `validate_pdf_timestamp` under a `DiffPolicy` into one typed `ConformanceVerdict` carrying the multi-signature valid/trusted/broken counts, the primary signer certificate's `subject`/`issuer`/`serial_number`, the claimed signing time and the TSA-attested timestamp time, the `md_algorithm` and `pkcs7_signature_mechanism` actually used, the bottom-line validity/trust/revocation aggregates, coverage, modification level, DocMDP certification, seed-value conformance, content-, signature-, and archival-timestamp trust, the EU-qualified status, the embedded DSS LTV-material counts (certs / OCSPs / CRLs / VRI entries), the classified PAdES level achieved, the document's declared PDF/A and PDF/X conformance claims, and the upstream structural-conformance result.

The signer credential is the closed `SignerSource` union — `PemKey` / `Pkcs12Bundle` / `ExternalSig` cases, each a frozen signer-struct read by the `SignerSource.cms()` projector method (woven with the shared `@beartype(conf=FAULT_CONF)` contract, mirroring the `exchange/credential#CREDENTIAL` sibling's format-named `SignerSpec.cose()` method — `cms` for the CMS/CAdES container PAdES rides, `cose` for c2pa) that binds `SimpleSigner.load`, `SimpleSigner.load_pkcs12`, or the `ExternalSigner` injected-signature HSM/remote seam — never the mutually-exclusive `key_file`/`pfx_file`/`external_signature` nullable-field bag a flat `params` struct smuggles, and never a free `_signer` function reconstructing the dispatch outside the owner. `Conformance.close` is `async` over the runtime `async_boundary`, offloading the synchronous `pyhanko`/`pikepdf` calls through `anyio.to_thread.run_sync` so the RFC-3161 timestamp and OCSP/CRL network I/O never block the event loop, with a `stamina.retry(on=TimestampRequestError)` definition-time weave re-attempting the transient TSA seam exactly as the `exchange/credential#CREDENTIAL` sibling re-attempts its own crypto-with-network seam, and returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]`.

`ConformanceVerdict` is the one acyclic value-object edge the `core/receipt#RECEIPT` `Verdict` case imports: `receipt.py` imports `ConformanceVerdict` and spreads `verdict.facts()`, so this owner never reciprocally imports `ArtifactReceipt` — the consumer mints `ArtifactReceipt.Verdict(key, verdict)` from the returned pair. This PAdES PDF-cryptographic close is orthogonal to the `exchange/credential#CREDENTIAL` C2PA cross-format content-authenticity bind — a PDF routes here, a raster/BMFF/audio asset routes to the c2pa rail.

## [01]-[INDEX]

- [01]-[CONFORMANCE]: `pyhanko` PAdES signing (with `CAdESSignedAttrSpec` commitment-type attributes), LTV archival maintenance, and resilient multi-signature conformance-audit owner that IS the closed `Conformance` `tagged_union` (`sign`/`stamp`/`augment`/`audit`, the signer-free `stamp` arm a `PdfTimeStamper.timestamp_pdf` proof-of-existence `/DocTimeStamp`), the dispatch, async entry, the `stamina.retry(on=TimestampRequestError)` transient-TSA weave, and case-body methods folded onto the union with no wrapper struct and no free helper; `SignerSource` the per-seam signer-credential union read by its `@beartype`-woven `cms()` projector method, `PadesLevel` the LTV-behavior policy row whose derived `needs_timestamp`/`embeds_validation`/`archival` properties carry the sign-time chain behavior and whose `classify` staticmethod folds the achieved-level derivation onto the vocabulary, `Digest` the digest-algorithm vocabulary, `SigKind` the `/Sig`/`/DocTimeStamp` signature-object discriminant, `KeyUsage` the audit key-usage constraint vocabulary, `Commitment` the CAdES commitment-type vocabulary projecting `GenericCommitment` through `_COMMITMENT`, `CertifyPerm`/`DiffMode` the vocabularies projecting `MDPPerm`/`DiffPolicy` through the `_MDP`/`_DIFF` `frozendict` tables, and `ConformanceVerdict` the rich audit value object every arm folds — carrying signer identity, signing/timestamp time, crypto-mechanism, multi-signature counts, EU-qualified status, and DSS LTV-material counts — that the `core/receipt#RECEIPT` `Verdict` case carries as the one acyclic leaf edge, the consumer minting `ArtifactReceipt.Verdict(key, verdict)` so `conformance` imports no `ArtifactReceipt`.

## [02]-[CONFORMANCE]

- Owner: `Conformance` the one PDF-cryptographic-close owner AND the closed `tagged_union` discriminating operation over its own typed `(bytes, spec)` payload — `sign`, `stamp`, `augment`, `audit` — matched by one total `match`/`case`, the `Sign`/`Stamp`/`Augment`/`Audit` factories, the async `close`/`_emit`/`_run` entry (the `_emit` thread-offload carrying the `stamina.retry(on=TimestampRequestError, attempts=4)` weave), AND the `_signed`/`_timestamped`/`_augmented`/`_refreshed`/`_resilient`/`_validated`/`_stamped`/`_audited` case-body methods all folded onto the union, never a one-field wrapper struct over a separate op union, never a `step: str` discriminant beside a shared optional-field `params` bag, and never a free module function reconstructing a case body outside the owner. `SignerSource` is the closed credential union carrying one frozen signer-struct per seam (`PemKey` PEM key+cert, `Pkcs12Bundle` PKCS#12, `ExternalSig` injected-signature HSM/remote) whose `cms()` method — woven with the shared `@beartype(conf=FAULT_CONF)` contract so a malformed credential lifts onto the runtime fault rail rather than raising past the thread-offload seam — reads the tagged case directly, never parallel `key_file`/`cert_file`/`pfx_file`/`external_signature` nullable fields the body re-derives. `PadesLevel` is the policy-as-value level row whose derived properties carry the LTV chain behavior and whose `classify(ltv, archival_valid, timestamp_valid)` staticmethod folds the achieved-level derivation onto the vocabulary; `Digest` the digest-algorithm vocabulary the signing `md_algorithm` is (the sha2/sha3 family passed straight to `PdfSignatureMetadata` as a `StrEnum` value, never a bare `str`); `Commitment` the CAdES commitment-type vocabulary projecting `GenericCommitment` (`proof-of-origin`/`-receipt`/`-delivery`/`-sender`/`-approval`/`-creation`) through `_COMMITMENT`; `CertifyPerm` the certification vocabulary projecting `MDPPerm` through `_MDP`; `DiffMode` the modification-policy vocabulary projecting the `pyhanko` `DiffPolicy` through `_DIFF`; `SigKind` the closed `/Sig`/`/DocTimeStamp` signature-object-type discriminant replacing the repeated bare-string `sig_object_type` comparisons; `KeyUsage` the closed key-usage vocabulary `AuditSpec.required_key_usage` carries into a `KeyUsageConstraints(key_usage=frozenset(...))` rather than a bare-`str` usage bag; `ConformanceVerdict` the carried audit value object every op folds, a `gc=False` scalar leaf whose `facts()` derives through `msgspec.structs.asdict`. `pyhanko` owns the CMS/CAdES/PAdES signing engine, the DSS/LTV embedding, the RFC-3161 timestamp chaining, the pluggable diff policy, and the embedded-signature plus document-timestamp validation; `pikepdf` owns the page-count and PDF/A·PDF/X conformance-claim read. `ConformanceVerdict` is declared on this owner so the value object is leaf-imported by `core/receipt#RECEIPT` without a reciprocal `ArtifactReceipt` import.
- Cases: `Conformance` cases `sign(pdf, SignSpec)` (the `SignSpec.signer` `SignerSource` credential, the `PadesLevel` whose properties derive `use_pades_lta`/`embed_validation_info`, the `SigFieldSpec` placement with optional `FieldMDPSpec` form-field lock, the optional `Commitment` projected to a `CAdESSignedAttrSpec` commitment-type signed attribute, the `reason`/`location`/`name`/`contact_info` text and the `Digest` `md_algorithm`, the optional `CertifyPerm` certification, the `HTTPTimeStamper` over `tsa_url`, and the `ValidationContext` for B-LT/B-LTA embedding — driving one `sign_pdf` over an `IncrementalPdfFileWriter`) · `stamp(pdf, StampSpec)` (the signer-free proof-of-existence pass — one `PdfTimeStamper(HTTPTimeStamper(tsa_url)).timestamp_pdf` applying an RFC-3161 `/DocTimeStamp` over the document with the `Digest` `md_algorithm` and optional `ValidationContext`, requiring no `SignerSource` credential) · `augment(pdf, AugmentSpec)` (LTV maintenance — `add_validation_info` folded over every `/Sig` signature index to embed fresh OCSP/CRL into the DSS, then `PdfTimeStamper.update_archival_timestamp_chain` to refresh the archival timestamp when `tsa_url` is supplied) · `audit(pdf, AuditSpec)` (the resilient multi-signature conformance pass folding every `/Sig` through `validate_pdf_signature` under the `AuditSpec.usage()` `KeyUsageConstraints` and every `/DocTimeStamp` archival stamp through `validate_pdf_timestamp` under the `DiffMode`-selected `DiffPolicy`, reading `read_certification_data`, the `DocumentSecurityStore.read_dss` cert/OCSP/CRL/VRI store, the primary signer's `signing_cert`, and the `pikepdf` `pdfa_status`/`pdfx_status` conformance claims, projecting one `ConformanceVerdict`) — selected by one total `match`, never a chain of `is`-probes. The `sign`/`augment` arms self-audit the produced PDF (sign-then-verify), so every op yields a fully-populated verdict in one shape.
- Entry: `Conformance.close` is `async` over the runtime `async_boundary` and returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]` — `_emit` offloads the synchronous `pyhanko`/`pikepdf` work through `anyio.to_thread.run_sync(self._run)` so the TSA timestamp and OCSP/CRL revocation fetches never block the event loop, under a `stamina.retry(on=TimestampRequestError, attempts=4)` definition-time weave on `_emit` re-attempting the transient TSA seam — a failed attempt produces no output, so the retry re-runs `_run` clean rather than emitting a duplicate signature — mints the `ContentKey` over the produced bytes, and returns the `(key, verdict)` pair. The thread offload is the backend-agnostic crossing the sibling `exchange/credential#CREDENTIAL` rail uses for the same crypto-with-network concern: `pyhanko`'s synchronous engine drives `requests`-based TSA/OCSP/CRL transport on the worker thread regardless of the `anyio` backend, so the owner never forces an `asyncio`-only async-HTTP path nor crosses the `anyio.to_process` gated band the host-native `detect`/`metadata` siblings ride (`pyhanko` is `py3-none-any` pure Python and `pikepdf` a `cp314-abi3` wheel, both clean on the cp315-core loader).
- Auto: `_run` folds the case through one total `match`. The `sign` case drives `_signed` — building the `SigFieldSpec` (with `FieldMDPSpec(FieldMDPAction.INCLUDE, lock_fields)` when fields are locked), the `PdfSignatureMetadata` whose `use_pades_lta`/`embed_validation_info` derive off the `PadesLevel` properties, whose `docmdp_permissions` resolves the typed `MDPPerm` through `_MDP`, and whose `cades_signed_attr_spec` resolves a `CAdESSignedAttrSpec(commitment_type=_COMMITMENT[spec.commitment].asn1)` when a commitment is supplied, the `HTTPTimeStamper` when the level needs one, and the `spec.signer.cms()` credential whose contract violation lifts onto the runtime fault rail — then one `sign_pdf(writer, meta, signer, timestamper=, new_field_spec=, output=)` into a fresh `BytesIO`. The `stamp` case drives `_timestamped` — one `PdfTimeStamper(HTTPTimeStamper(spec.tsa_url)).timestamp_pdf(writer, spec.md_algorithm, spec.validation_context, output=)` applying a signer-free `/DocTimeStamp` with no credential. The `augment` case drives `_augmented` — a `Block.fold` over the `SigKind.SIGNATURE` indices threading `add_validation_info(..., in_place=False, output=)` to embed fresh validation material, then the conditional `_refreshed` archival-chain extension. The `audit` case drives `_audited` — reading `reader.embedded_signatures` ONCE and splitting it through one `Block.partition` keyed on `SigKind.SIGNATURE` into the `/Sig` `EmbeddedPdfSignature` and `/DocTimeStamp` archival-timestamp halves in a single pass, resiliently validating each signature through the `_validated` projector over `validate_pdf_signature(sig, signer_validation_context=, ts_validation_context=, diff_policy=, key_usage_settings=)` and each archival stamp through the symmetric `_stamped` projector over `validate_pdf_timestamp`, both delegating to the one `_resilient[T]` boundary-capture aspect that projects a `SignatureValidationError` onto `Nothing` so a broken signature or stamp is counted not fatal, collected through `Block.choose` into the live-status sets, folding the live statuses into the `signatures_valid`/`signatures_trusted` counts, the `signatures_broken` shortfall, the `complete`-gated aggregate booleans (`all` bottom-line/trusted/docmdp/seed-value, `any` revoked, weakest-link `min` coverage, worst-case `max` modification), and the archival `DocumentTimestampStatus.trusted` flags into `archival_timestamps_valid`, projecting the primary live status's `signing_cert.subject`/`issuer`/`serial_number`, `signer_reported_dt`, `timestamp_validity.timestamp`, `content_timestamp_validity.trusted`, `md_algorithm`, `pkcs7_signature_mechanism`, and `qualification_result.status.qualified` through `Option.map` defaulted absence, reading `read_certification_data(reader).permission`, gating `DocumentSecurityStore.read_dss(reader)` behind a `"/DSS" in reader.root` membership test and reading its `certs`/`ocsps`/`crls`/`vri_entries` cardinality, reading the `pikepdf` page count and `pdfa_status`/`pdfx_status` conformance claims in one open, classifying the achieved `PadesLevel` from the LTV and archival-timestamp-trust evidence, and projecting one `ConformanceVerdict`.
- Receipt: every op produces one `ConformanceVerdict`, the audit-evidence value object the `core/receipt#RECEIPT` `ArtifactReceipt.Verdict` case carries as `tuple[ContentKey, ConformanceVerdict]`. `receipt.py` imports `ConformanceVerdict` and its `_facts` arm spreads `verdict.facts()`, and this owner never reciprocally imports `ArtifactReceipt`, so the value-object edge is the single acyclic leaf edge the receipt union admits — the `core/plan#PLAN` planner (or any coordinator holding both owners) mints `ArtifactReceipt.Verdict(key, verdict)` from the returned `(key, verdict)` pair. `ConformanceVerdict.facts()` derives the fact map through `msgspec.structs.asdict(self)` — one edit site that cannot drift from the field set — projecting NATIVE scalars (`bool`/`int`/`str`) onto the `dict[str, object]` `EventDict`, never `str(...)`-pre-stringified text, so the `observability/metrics` `MeterProvider` valid/trusted/broken/qualified signal stream and the structured-log consumer read the verdict facts as numbers and booleans. Structural conformance enters on two axes the verdict surfaces, never a new receipt field: the `document/tagged#ACCESS` PDF/UA `structural_conformant` boolean carried on the spec as interior evidence, and the document's own declared `pdfa_claim`/`pdfx_claim` read here from the `pikepdf` XMP — what the signed document ASSERTS about its PDF/A and PDF/X conformance, distinct from the upstream PDF/UA structural pass.
- Packages: `pyhanko` (`SimpleSigner.load`/`load_pkcs12`, `ExternalSigner(signing_cert=, cert_registry=, signature_value=)`, `load_certs_from_pemder`, `Signer`, `sign_pdf(..., new_field_spec=, output=)`, `IncrementalPdfFileWriter`, `PdfFileReader.embedded_signatures`, `HTTPTimeStamper`, `TimestampRequestError` (`pyhanko.sign.timestamps`), `PdfTimeStamper(timestamper).update_archival_timestamp_chain(reader, validation_context, in_place=False, output=)`, `PdfTimeStamper(timestamper).timestamp_pdf(pdf_out, md_algorithm, validation_context, output=)`, `PdfSignatureMetadata` with `field_name`/`md_algorithm`/`reason`/`location`/`contact_info`/`name`/`subfilter`/`certify`/`docmdp_permissions`/`cades_signed_attr_spec`/`use_pades_lta`/`embed_validation_info`/`validation_context`, `SigFieldSpec(sig_field_name=, on_page=, box=, field_mdp_spec=)`, `SigSeedSubFilter.PADES`, `FieldMDPSpec(action, fields)`/`FieldMDPAction`, `MDPPerm`, `CAdESSignedAttrSpec(commitment_type=)` and `GenericCommitment.*.asn1` (`pyhanko.sign.ades.api`), `add_validation_info(emb, vc, in_place=False, output=)`, `validate_pdf_signature(emb, signer_validation_context=, ts_validation_context=, diff_policy=, key_usage_settings=) -> PdfSignatureStatus`, `validate_pdf_timestamp(emb, validation_context=, diff_policy=) -> DocumentTimestampStatus` (`.trusted`), `read_certification_data(reader) -> DocMDPInfo` (`.permission`), `DocumentSecurityStore.read_dss(reader)` (`.certs`/`.ocsps`/`.crls`/`.vri_entries`), `EmbeddedPdfSignature.sig_object_type`, `KeyUsageConstraints(key_usage=)` (`pyhanko.sign.validation.settings`), `SignatureValidationError` (`pyhanko.sign.validation.errors`), `PdfSignatureStatus.bottom_line`/`trusted`/`revoked`/`coverage`/`modification_level`/`docmdp_ok`/`seed_value_ok`/`signer_reported_dt`/`md_algorithm`/`pkcs7_signature_mechanism`/`timestamp_validity`/`content_timestamp_validity`/`signing_cert`/`qualification_result`, `TimestampSignatureStatus.timestamp`/`.trusted`, `QualificationResult.status.qualified`, `SignatureCoverageLevel`, `ModificationLevel`, `DEFAULT_DIFF_POLICY`/`NO_CHANGES_DIFF_POLICY`/`DiffPolicy`); `pyhanko_certvalidator` (`ValidationContext` arriving at the boundary, `SimpleCertificateStore.from_certs` for the external-signer registry); `asn1crypto` (the `signing_cert` `x509.Certificate.subject.human_friendly`/`issuer.human_friendly`/`serial_number` identity accessors reached through the catalogued `signing_cert` type); `pikepdf` (`open` + `len(pdf.pages)` + `open_metadata` + `PdfMetadata.pdfa_status`/`pdfx_status`); `msgspec` (`Struct(frozen=True)` for the specs, `Struct(frozen=True, gc=False)` for the `ConformanceVerdict` scalar leaf, `structs.asdict` deriving `facts()`); `beartype` (`beartype(conf=FAULT_CONF)` the contract weave on `SignerSource.cms`); `expression` (`tagged_union`/`tag`/`case`, `Block.of_seq`/`fold`/`choose`/`partition`/`try_head`/`is_empty`, `Option`/`Some`/`Nothing`/`map`/`default_value`); `stamina` (`retry(on=TimestampRequestError, attempts=4)` the transient-TSA network weave on `_emit`); `anyio` (`to_thread.run_sync`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`/`FAULT_CONF`).
- Growth: a new operation is one `Conformance` case plus one `_run` arm plus one case-body method; a new signer seam is one `SignerSource` case plus one `cms()` arm, never a parallel signer owner; a new PAdES level is one `PadesLevel` row whose derived properties carry its LTV behavior; a new digest algorithm is one `Digest` row; a new commitment type is one `Commitment` row plus one `_COMMITMENT` entry; a new certification permission is one `CertifyPerm` row plus one `_MDP` entry; a new modification policy is one `DiffMode` row plus one `_DIFF` entry; a new key-usage constraint is one `KeyUsage` row; a new signature-object kind is one `SigKind` row; a new audit fact is one `ConformanceVerdict` field that `facts()` picks up through `structs.asdict` with no second edit; a signer-eligibility constraint (who may sign a prepared field) is one `SigSeedValueSpec`/`SigCertConstraints` projection on `_signed`'s `seed_value_dict`; a signature-policy identifier or signer-attribute assertion is one `signature_policy_identifier`/`signer_attributes` projection on the `CAdESSignedAttrSpec`; a visible signature appearance is one `PdfSigner(stamp_style=...)` swap for the bare `sign_pdf`; zero new surface.
- Boundary: no PDF authoring (that stays at `document/emit#DOCUMENT`), no font engineering (`typography/font#FONT`), no glyph rendering (`typography/shape#SHAPE`); the owner closes an already-emitted PDF, never producing one. `pyhanko` does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF — so the structural verdict is authored upstream at `document/tagged#ACCESS` (the `pikepdf` `StructTreeRoot`/`StructElem` marked-content tree over the `document/model#NODE` family) and the `audit` arm CONSUMES that boolean through the spec, folding it into `ConformanceVerdict.structural_conformant` rather than claiming a veraPDF-grade verdict no pure-Python validator resolves; the `pdfa_claim`/`pdfx_claim` the arm reads from the `pikepdf` XMP are the document's OWN declared `pdfaid`/`pdfxid` claims (an evidence read of what the signed file asserts), never a validated PDF/A verdict; the embed-completeness precondition the PDF/A close requires arrives from `typography/font#FONT` `EMBED_AUDIT`; the `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary, never built here. The deleted forms are the `MappingProxyType` step table where `frozendict` is the owner, the one-field `Conformance` wrapper over a separate `ConformOp` union where the `tagged_union` owns dispatch and async entry directly, the free `_signer`/`_classified` module functions where `SignerSource.cms()` and `PadesLevel.classify()` fold the dispatch onto the owner and the vocabulary, the optional-field signer bag where `SignerSource` is a closed union, the bare-`str` `md_algorithm` where the `Digest` vocabulary types it, the repeated bare-string `sig_object_type == "/Sig"`/`"/DocTimeStamp"` comparisons where the `SigKind` vocabulary and one `Block.partition` own the discriminant, the bare-`str` key-usage bag where the `KeyUsage` vocabulary types `required_key_usage`, the duplicated per-projector `try`/`except` where the one `_resilient[T]` aspect owns the `SignatureValidationError`-to-`Nothing` capture, the two-pass tuple-comprehension partition where one `Block.partition` splits the embedded signatures, the un-retried TSA network seam where the `stamina.retry(on=TimestampRequestError)` weave re-attempts the transient, the empty-string-as-absence signing metadata where `str | None` states omission, the lone `embedded_signatures[0]` audit where the verdict folds every `/Sig`, the audit that ABORTS on one broken signature where `_validated` projects the `SignatureValidationError` onto `Nothing` and the `signatures_broken` count, the identity-blind verdict where `signing_cert.subject`/`issuer`/`serial_number` and `signer_reported_dt`/`timestamp_validity.timestamp` carry WHO signed and WHEN, the semantically-empty `archival_metadata=bool(xmp)` where the DSS `certs`/`ocsps`/`crls`/`vri_entries` counts carry the real LTV-material evidence, the presence-only B-LTA classification where `validate_pdf_timestamp` proves the archival timestamp's trust, the erased `object` certification-permission and `object | None` validation-context where `MDPPerm` and `ValidationContext` type them, the bare `@beartype` raise where `beartype(conf=FAULT_CONF)` lifts a contract violation onto the fault rail, the hand-maintained `facts()` mirror where `structs.asdict` derives it, the synchronous network-blocking boundary where `anyio.to_thread.run_sync` offloads the TSA/OCSP I/O, and the discarded verdict where `close` returns `(ContentKey, ConformanceVerdict)`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Literal, Self, assert_never

import pikepdf
import stamina
from anyio import to_thread
from beartype import beartype
from builtins import frozendict
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, structs
from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
from pyhanko.pdf_utils.reader import PdfFileReader
from pyhanko.sign import (
    ExternalSigner,
    PdfSignatureMetadata,
    PdfTimeStamper,
    Signer,
    SimpleSigner,
    load_certs_from_pemder,
    sign_pdf,
)
from pyhanko.sign.ades.api import CAdESSignedAttrSpec, GenericCommitment
from pyhanko.sign.diff_analysis import DEFAULT_DIFF_POLICY, NO_CHANGES_DIFF_POLICY, DiffPolicy, ModificationLevel
from pyhanko.sign.fields import FieldMDPAction, FieldMDPSpec, MDPPerm, SigFieldSpec, SigSeedSubFilter
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
from pyhanko_certvalidator import ValidationContext
from pyhanko_certvalidator.registry import SimpleCertificateStore

from rasm.runtime.content_identity import ContentIdentity, ContentKey
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
        return (
            PadesLevel.B_LTA if ltv and archival_valid
            else PadesLevel.B_LT if ltv
            else PadesLevel.B_T if timestamp_valid
            else PadesLevel.B_B
        )


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


# --- [CONSTANTS] ------------------------------------------------------------------------
_MDP: frozendict[CertifyPerm, MDPPerm] = frozendict({
    CertifyPerm.NO_CHANGES: MDPPerm.NO_CHANGES,
    CertifyPerm.FILL_FORMS: MDPPerm.FILL_FORMS,
    CertifyPerm.ANNOTATE: MDPPerm.ANNOTATE,
})
_DIFF: frozendict[DiffMode, DiffPolicy] = frozendict({
    DiffMode.DEFAULT: DEFAULT_DIFF_POLICY,
    DiffMode.STRICT: NO_CHANGES_DIFF_POLICY,
})
_COMMITMENT: frozendict[Commitment, GenericCommitment] = frozendict({
    Commitment.ORIGIN: GenericCommitment.PROOF_OF_ORIGIN,
    Commitment.RECEIPT: GenericCommitment.PROOF_OF_RECEIPT,
    Commitment.DELIVERY: GenericCommitment.PROOF_OF_DELIVERY,
    Commitment.SENDER: GenericCommitment.PROOF_OF_SENDER,
    Commitment.APPROVAL: GenericCommitment.PROOF_OF_APPROVAL,
    Commitment.CREATION: GenericCommitment.PROOF_OF_CREATION,
})


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
                return ExternalSigner(signing_cert=chain[0], cert_registry=SimpleCertificateStore.from_certs(chain), signature_value=ext.signature_value)
            case _ as unreachable:
                assert_never(unreachable)


class AuditSpec(Struct, frozen=True):
    signer_context: ValidationContext | None = None
    ts_context: ValidationContext | None = None
    diff_mode: DiffMode = DiffMode.DEFAULT
    required_key_usage: tuple[KeyUsage, ...] = ()
    structural_conformant: bool = False

    def usage(self) -> KeyUsageConstraints | None:
        return KeyUsageConstraints(key_usage=frozenset(self.required_key_usage)) if self.required_key_usage else None


class SignSpec(Struct, frozen=True):
    signer: SignerSource
    pades_level: PadesLevel = PadesLevel.B_LTA
    field_name: str = "Signature1"
    field_box: tuple[int, int, int, int] | None = None
    page: int = 0
    certify: CertifyPerm | None = None
    commitment: Commitment | None = None
    lock_fields: tuple[str, ...] = ()
    reason: str | None = None
    location: str | None = None
    contact_info: str | None = None
    name: str | None = None
    md_algorithm: Digest = Digest.SHA256
    tsa_url: str | None = None
    validation_context: ValidationContext | None = None
    structural_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(signer_context=self.validation_context, ts_context=self.validation_context, structural_conformant=self.structural_conformant)


class StampSpec(Struct, frozen=True):
    tsa_url: str
    md_algorithm: Digest = Digest.SHA256
    validation_context: ValidationContext | None = None
    structural_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(ts_context=self.validation_context, structural_conformant=self.structural_conformant)


class AugmentSpec(Struct, frozen=True):
    validation_context: ValidationContext
    tsa_url: str | None = None
    structural_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(signer_context=self.validation_context, ts_context=self.validation_context, structural_conformant=self.structural_conformant)


class ConformanceVerdict(Struct, frozen=True, gc=False):
    pades_level: str
    pages: int
    signatures: int
    timestamps: int
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
    pdfa_claim: str
    pdfx_claim: str

    def facts(self) -> dict[str, object]:
        return structs.asdict(self)


@tagged_union(frozen=True)
class Conformance:
    tag: Literal["sign", "stamp", "augment", "audit"] = tag()
    sign: tuple[bytes, SignSpec] = case()
    stamp: tuple[bytes, StampSpec] = case()
    augment: tuple[bytes, AugmentSpec] = case()
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
    def Audit(cls, pdf: bytes, spec: AuditSpec, /) -> Self:
        return cls(audit=(pdf, spec))

    async def close(self) -> RuntimeRail[tuple[ContentKey, ConformanceVerdict]]:
        return await async_boundary(f"conformance.{self.tag}", self._emit)

    @stamina.retry(on=TimestampRequestError, attempts=4)
    async def _emit(self) -> tuple[ContentKey, ConformanceVerdict]:
        payload, verdict = await to_thread.run_sync(self._run)
        return ContentIdentity.of(f"conformance-{self.tag}", payload), verdict

    def _run(self) -> tuple[bytes, ConformanceVerdict]:
        match self:
            case Conformance(tag="sign", sign=(pdf, spec)):
                signed = self._signed(pdf, spec)
                return signed, self._audited(signed, spec.audit())
            case Conformance(tag="stamp", stamp=(pdf, spec)):
                stamped = self._timestamped(pdf, spec)
                return stamped, self._audited(stamped, spec.audit())
            case Conformance(tag="augment", augment=(pdf, spec)):
                upgraded = self._augmented(pdf, spec)
                return upgraded, self._audited(upgraded, spec.audit())
            case Conformance(tag="audit", audit=(pdf, spec)):
                return pdf, self._audited(pdf, spec)
            case _ as unreachable:
                assert_never(unreachable)

    def _signed(self, pdf: bytes, spec: SignSpec, /) -> bytes:
        level = spec.pades_level
        field = SigFieldSpec(
            sig_field_name=spec.field_name,
            on_page=spec.page,
            box=spec.field_box,
            field_mdp_spec=FieldMDPSpec(FieldMDPAction.INCLUDE, list(spec.lock_fields)) if spec.lock_fields else None,
        )
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
            use_pades_lta=level.archival,
            embed_validation_info=level.embeds_validation,
            validation_context=spec.validation_context,
        )
        timestamper = HTTPTimeStamper(spec.tsa_url) if level.needs_timestamp and spec.tsa_url else None
        sink = BytesIO()
        sign_pdf(IncrementalPdfFileWriter(BytesIO(pdf)), meta, spec.signer.cms(), timestamper=timestamper, new_field_spec=field, output=sink)
        return sink.getvalue()

    def _timestamped(self, pdf: bytes, spec: StampSpec, /) -> bytes:
        sink = BytesIO()
        PdfTimeStamper(HTTPTimeStamper(spec.tsa_url)).timestamp_pdf(IncrementalPdfFileWriter(BytesIO(pdf)), spec.md_algorithm, spec.validation_context, output=sink)
        return sink.getvalue()

    def _augmented(self, pdf: bytes, spec: AugmentSpec, /) -> bytes:
        indices = Block.of_seq(
            i for i, sig in enumerate(PdfFileReader(BytesIO(pdf)).embedded_signatures) if str(sig.sig_object_type) == SigKind.SIGNATURE
        )

        def embedded(current: bytes, index: int, /) -> bytes:
            sink = BytesIO()
            add_validation_info(PdfFileReader(BytesIO(current)).embedded_signatures[index], spec.validation_context, in_place=False, output=sink)
            return sink.getvalue()

        enriched = indices.fold(embedded, pdf)
        return self._refreshed(enriched, spec.tsa_url, spec.validation_context) if spec.tsa_url else enriched

    def _refreshed(self, pdf: bytes, tsa_url: str, context: ValidationContext, /) -> bytes:
        sink = BytesIO()
        PdfTimeStamper(HTTPTimeStamper(tsa_url)).update_archival_timestamp_chain(PdfFileReader(BytesIO(pdf)), context, in_place=False, output=sink)
        return sink.getvalue()

    @staticmethod
    def _resilient[T](validate: Callable[[], T], /) -> Option[T]:
        try:  # a structurally broken /Sig or /DocTimeStamp projects to Nothing — counted, never fatal to the audit
            return Some(validate())
        except SignatureValidationError:
            return Nothing

    def _validated(self, sig: EmbeddedPdfSignature, spec: AuditSpec, /) -> Option[PdfSignatureStatus]:
        return self._resilient(lambda: validate_pdf_signature(sig, signer_validation_context=spec.signer_context, ts_validation_context=spec.ts_context, diff_policy=_DIFF[spec.diff_mode], key_usage_settings=spec.usage()))

    def _stamped(self, ts: EmbeddedPdfSignature, spec: AuditSpec, /) -> Option[DocumentTimestampStatus]:
        return self._resilient(lambda: validate_pdf_timestamp(ts, validation_context=spec.ts_context, diff_policy=_DIFF[spec.diff_mode]))

    def _audited(self, pdf: bytes, spec: AuditSpec, /) -> ConformanceVerdict:
        reader = PdfFileReader(BytesIO(pdf))
        signatures, doc_timestamps = Block.of_seq(reader.embedded_signatures).partition(lambda s: str(s.sig_object_type) == SigKind.SIGNATURE)
        live = signatures.choose(lambda s: self._validated(s, spec))
        archival = doc_timestamps.choose(lambda ts: self._stamped(ts, spec))
        primary: Option[PdfSignatureStatus] = live.try_head()
        cert = primary.map(lambda s: s.signing_cert)
        certification = read_certification_data(reader)
        dss = DocumentSecurityStore.read_dss(reader) if "/DSS" in reader.root else None
        complete = not signatures.is_empty() and len(live) == len(signatures)
        timestamp_valid = not live.is_empty() and all(s.timestamp_validity is not None and s.timestamp_validity.trusted for s in live)
        archival_valid = not doc_timestamps.is_empty() and len(archival) == len(doc_timestamps) and all(stamp.trusted for stamp in archival)
        ltv_complete = dss is not None and bool(dss.certs)
        with pikepdf.open(BytesIO(pdf)) as document, document.open_metadata(set_pikepdf_as_editor=False) as xmp:
            pages, pdfa_claim, pdfx_claim = len(document.pages), xmp.pdfa_status or "", xmp.pdfx_status or ""
        return ConformanceVerdict(
            pades_level=PadesLevel.classify(ltv_complete, archival_valid, timestamp_valid).value,
            pages=pages,
            signatures=len(signatures),
            timestamps=len(doc_timestamps),
            signatures_valid=sum(1 for s in live if s.bottom_line),
            signatures_trusted=sum(1 for s in live if s.trusted),
            signatures_broken=len(signatures) - len(live),
            signature_valid=complete and all(s.bottom_line for s in live),
            trusted=complete and all(s.trusted for s in live),
            revoked=any(s.revoked for s in live),
            coverage_level=min((s.coverage for s in live), default=SignatureCoverageLevel.UNCLEAR).name,
            modification_level=max((s.modification_level for s in live if s.modification_level is not None), default=ModificationLevel.NONE).name,
            docmdp_ok=all(s.docmdp_ok is not False for s in live),
            seed_value_ok=all(s.seed_value_ok is not False for s in live),
            certification_level=certification.permission.name if certification is not None else "none",
            signer_subject=cert.map(lambda c: c.subject.human_friendly).default_value(""),
            signer_issuer=cert.map(lambda c: c.issuer.human_friendly).default_value(""),
            signer_serial=cert.map(lambda c: str(c.serial_number)).default_value(""),
            digest_algorithm=primary.map(lambda s: s.md_algorithm).default_value(""),
            signature_mechanism=primary.map(lambda s: s.pkcs7_signature_mechanism).default_value(""),
            signed_at=primary.map(lambda s: s.signer_reported_dt.isoformat() if s.signer_reported_dt else "").default_value(""),
            timestamp_at=primary.map(lambda s: s.timestamp_validity.timestamp.isoformat() if s.timestamp_validity is not None else "").default_value(""),
            timestamp_valid=timestamp_valid,
            content_timestamp_valid=primary.map(lambda s: s.content_timestamp_validity is not None and s.content_timestamp_validity.trusted).default_value(False),
            archival_timestamps_valid=archival_valid,
            qualified=primary.map(lambda s: s.qualification_result is not None and s.qualification_result.status.qualified).default_value(False),
            ltv_complete=ltv_complete,
            dss_certs=len(dss.certs) if dss is not None else 0,
            dss_ocsps=len(dss.ocsps) if dss is not None else 0,
            dss_crls=len(dss.crls) if dss is not None else 0,
            dss_vri=len(dss.vri_entries) if dss is not None else 0,
            structural_conformant=spec.structural_conformant,
            pdfa_claim=pdfa_claim,
            pdfx_claim=pdfx_claim,
        )
```

## [03]-[RESEARCH]

- [SIGN_AXIS] [RESOLVED]: the `pyhanko 0.35.1` signing surface verifies against the installed distribution and the folder `.api` catalogue — `SimpleSigner.load(key_file, cert_file, ca_chain_files=, key_passphrase=)`, `SimpleSigner.load_pkcs12(pfx_file, ca_chain_files=, passphrase=)`, `ExternalSigner(signing_cert, cert_registry, signature_value=)` (the injected-signature HSM/remote seam), `load_certs_from_pemder(cert_files)`, `SimpleCertificateStore.from_certs(certs)` (the external-signer cert registry), `sign_pdf(pdf_out, signature_meta, signer, timestamper=, new_field_spec=, output=)`, and `IncrementalPdfFileWriter` are settled. The credential mode is the closed `SignerSource` union whose `cms()` projector method — woven with the shared `@beartype(conf=FAULT_CONF)` contract so a malformed `SignerSource` lifts onto the runtime fault rail, exactly as the `exchange/credential#CREDENTIAL` sibling's format-named `SignerSpec.cose()` method does — folds the three load paths onto the owner rather than a free `_signer` function reconstructing the dispatch beside it; a mutually-exclusive nullable-field bag and an `ExternalSigner(signing_cert=None)` are excluded by construction. The `PdfSignatureMetadata` field set verifies against the installed constructor — including `cades_signed_attr_spec: CAdESSignedAttrSpec | None`, so the optional `Commitment` axis projects a `CAdESSignedAttrSpec(commitment_type=_COMMITMENT[spec.commitment].asn1)` CAdES commitment-type signed attribute (`GenericCommitment.PROOF_OF_ORIGIN`/`PROOF_OF_RECEIPT`/`PROOF_OF_DELIVERY`/`PROOF_OF_SENDER`/`PROOF_OF_APPROVAL`/`PROOF_OF_CREATION` from `pyhanko.sign.ades.api`, each carrying `.asn1 -> CommitmentTypeIndication`); the descriptive `reason`/`location`/`name`/`contact_info` ride as `str | None` (omission stated by `None`) and `md_algorithm` is the closed `Digest` `StrEnum` passed straight through, never a bare `str`. `SigFieldSpec(sig_field_name, on_page, box, field_mdp_spec=, seed_value_dict=, doc_mdp_update_value=)`, `FieldMDPSpec(action, fields)`, and `FieldMDPAction` (`ALL`/`INCLUDE`/`EXCLUDE`) verify, so `lock_fields` projects a `FieldMDPSpec` form-field lock and the signer-eligibility constraint grows as a `SigSeedValueSpec`/`SigCertConstraints` `seed_value_dict` projection. The PAdES level rises B-B -> B-LTA as `subfilter=PADES` plus a `timestamper` engages B-T, `embed_validation_info=True` writes the DSS for B-LT, and `use_pades_lta=True` with a timestamper writes the initial archival timestamp for B-LTA — the initial archival stamp is `use_pades_lta`'s, so the chain refresh is the `Augment` arm's concern alone.
- [STAMP_AXIS] [RESOLVED]: the signer-free document-timestamp entrypoint `PdfTimeStamper(timestamper).timestamp_pdf(pdf_out, md_algorithm, validation_context, output=)` verifies against the installed distribution and rides the same `IncrementalPdfFileWriter` plus `output=` append-only convention as `sign_pdf` and `update_archival_timestamp_chain`. The `stamp` case applies an RFC-3161 `/DocTimeStamp` proof-of-existence timestamp over the document with the `Digest` `md_algorithm` and an optional `ValidationContext`, requiring no `SignerSource` credential — the PAdES capability the owner formerly left to the unused `timestamp_pdf` entrypoint, now the realized fourth `Conformance` case absorbed under the same growth law (`StampSpec` payload, `Stamp` factory, `_run` arm, `_timestamped` body). The produced `/DocTimeStamp` self-audits through the same `validate_pdf_timestamp` archival path, `StampSpec.audit()` projecting `ts_context=self.validation_context` so the timestamp trust is verified, not assumed.
- [AUDIT_AXIS] [RESOLVED]: `PdfFileReader(stream).embedded_signatures` (a property over EVERY signature, read ONCE and split through one `Block.partition` keyed on the `SigKind.SIGNATURE` discriminant into its `/Sig` and `/DocTimeStamp` halves in a single pass), `EmbeddedPdfSignature.sig_object_type` (the `SigKind` discriminant, a closed `/Sig`/`/DocTimeStamp` vocabulary replacing repeated bare-string comparisons), `validate_pdf_signature(embedded_sig, signer_validation_context=, ts_validation_context=, diff_policy=, key_usage_settings=) -> PdfSignatureStatus`, and `validate_pdf_timestamp(embedded_sig, validation_context=, diff_policy=) -> DocumentTimestampStatus` verify against the installed distribution; the audit is RESILIENT on BOTH axes — the `_validated` projector over `validate_pdf_signature` and the symmetric `_stamped` projector over `validate_pdf_timestamp` both delegate to the one `_resilient[T]` boundary-capture aspect projecting a `SignatureValidationError` (from `pyhanko.sign.validation.errors`) onto `Nothing`, collected through `Block.choose` into the live-status sets so a structurally broken signature is counted as `signatures_broken` (the shortfall against `len(signatures)`) and a broken archival stamp drops `archival_timestamps_valid` to false rather than either aborting the whole audit through the boundary. The `PdfSignatureStatus` field set the verdict folds verifies against the installed class hierarchy (`PdfSignatureStatus -> StandardCMSSignatureStatus -> SignatureStatus`): `bottom_line`, `trusted`, `revoked`, `coverage` (`SignatureCoverageLevel`, orderable so the weakest-link `min` is direct), `modification_level` (`ModificationLevel`, orderable so the worst-case `max` is direct), `docmdp_ok`, `seed_value_ok`, `timestamp_validity` (`TimestampSignatureStatus | None` carrying `.timestamp` and `.trusted`), `content_timestamp_validity` (the PAdES content-timestamp `TimestampSignatureStatus | None`, the proof-of-existence stamp distinct from the signature- and archival-timestamps), the base `signing_cert` (`asn1crypto.x509.Certificate` whose `.subject.human_friendly`/`.issuer.human_friendly` properties and `.serial_number` int verify against the installed `asn1crypto`), `signer_reported_dt` (the signer's claimed signing time), `md_algorithm` and `pkcs7_signature_mechanism` (the digest and signature mechanism actually used), and `qualification_result` (`QualificationResult | None` whose `.status.qualified` bool reports the EU-qualified outcome when a qualified `ValidationContext` is supplied) — so the verdict reports WHO signed, WHEN, with WHAT cryptography, at WHAT trust and qualification level, never validity alone. `AuditSpec.required_key_usage` — a closed `KeyUsage` `StrEnum` tuple over the nine asn1crypto key-usage names, never a bare-`str` bag — projects a `KeyUsageConstraints(key_usage=frozenset(...))` (from `pyhanko.sign.validation.settings`) into the `key_usage_settings=` slot so a key-usage policy is a typed validation constraint rather than a post-hoc check. `read_certification_data(reader) -> DocMDPInfo | None` (`.permission` the `MDPPerm` level) reads the certifying signature with a `None` guard. `DocumentSecurityStore.read_dss(reader)` is gated behind a `"/DSS" in reader.root` membership test, and its instance `certs`/`ocsps`/`crls`/`vri_entries` cardinalities ride the verdict as the real LTV-material evidence — replacing the semantically-empty `archival_metadata=bool(xmp)` whose `bool` over a metadata context object was always truthy. The achieved PAdES level is classified through `PadesLevel.classify` (folded onto the vocabulary): B-LTA when the DSS is present AND every `/DocTimeStamp` archival stamp validates as trusted, B-LT when the DSS is present, B-T when the signature timestamps are trusted, B-B otherwise.
- [AUGMENT_AXIS] [RESOLVED]: the LTV-maintenance entrypoints verify against the installed distribution — `add_validation_info(embedded_sig, validation_context, in_place=False, output=)` embeds fresh OCSP/CRL/cert material into the DSS (upgrading B-T -> B-LT), and `PdfTimeStamper(timestamper).update_archival_timestamp_chain(reader, validation_context, in_place=False, output=)` extends the archival timestamp chain (refreshing B-LTA before the prior archival timestamp's TSA certificate expires). `_augmented` folds `add_validation_info` over the `SigKind.SIGNATURE` signature indices through `indices.fold(embedded, pdf)` (threading each output as the next input) before the conditional chain refresh, so a multi-signature PDF has every signature's revocation material embedded, never the first alone, and the fold rides the `expression.Block` substrate rather than a `functools.reduce` import. The `Augment` arm requires a `ValidationContext` (the trust/revocation source `add_validation_info` consumes), so `AugmentSpec.validation_context` is non-optional where `SignSpec`/`AuditSpec` carry it optionally.
- [RAIL_AND_EDGE] [RESOLVED]: `Conformance.close` is `async` over the runtime `async_boundary` and offloads `_run` through `anyio.to_thread.run_sync` rather than blocking the runtime boundary — `pyhanko`'s RFC-3161 timestamp and OCSP/CRL revocation transport is `requests`-based blocking network I/O on B-T/B-LT/B-LTA that would otherwise block the event loop; the thread offload is the backend-agnostic crossing the `exchange/credential#CREDENTIAL` sibling uses for the same crypto-with-network concern, never forcing `pyhanko`'s `asyncio`-only `aiohttp` async path onto a possibly-`trio` backend. `pyhanko` (`py3-none-any` pure Python) and `pikepdf` (`cp314-abi3`) both resolve clean on the cp315-core loader, so the owner needs no `anyio.to_process` gated band. A `stamina.retry(on=TimestampRequestError, attempts=4)` definition-time weave on `_emit` re-attempts the transient TSA network seam the `requests` transport raises through `TimestampRequestError` — `stamina` auto-detects the live backend through `sniffio`, so the backoff sleep is an `asyncio`/`trio` checkpoint, and a failed attempt produces no output, so the retry re-runs `_run` clean rather than emitting a duplicate signature — the same crypto-with-network resilience the `exchange/credential#CREDENTIAL` sibling applies to its own transient subset. The entry returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]`, carrying the verdict the audit produces rather than a bare key. `ConformanceVerdict` is the one acyclic value-object edge: `core/receipt#RECEIPT` imports it (`from artifacts.exchange.conformance import ConformanceVerdict`) for its `verdict: tuple[ContentKey, ConformanceVerdict]` case and spreads `verdict.facts()`, and this owner never imports `ArtifactReceipt`, so the consumer mints `ArtifactReceipt.Verdict(key, verdict)` from the returned pair — importing `ArtifactReceipt` here would close the module-scope cycle the leaf edge exists to avoid. `ConformanceVerdict.facts()` derives `dict[str, object]` through `msgspec.structs.asdict` with native `bool`/`int`/`str` scalars — one derivation that cannot drift from the `gc=False` scalar-leaf field set — so the receipt owner's `_facts` arm and the `MeterProvider` read them unstringified. The case-body methods (`_signed`/`_timestamped`/`_augmented`/`_refreshed`/`_resilient`/`_validated`/`_stamped`/`_audited`) are folded onto the `Conformance` owner rather than free module functions, and `to_thread.run_sync(self._run)` targets the bound method (the in-process thread offload, unlike `to_process`, admits a bound method), so the dispatch, the async entry, and every case body live on one owner.
