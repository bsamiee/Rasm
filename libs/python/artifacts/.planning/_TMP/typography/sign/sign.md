# [PY_ARTIFACTS_SIGN]

The PDF cryptographic-conformance close over the document rail. `Conformance` is ONE owner that takes a PDF emitted by `documents/emit#DOCUMENT` and folds it through a closed `ConformStep` family into a digitally-signed, archival-grade, audited artifact AND proves it: pyhanko applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) with subfilter, embedded validation material, timestamp authority, certification permissions, and the B-LTA archival-timestamp-chain refresh under the `PadesLevel`/`CertifyPerm` policy rows, and the audit verb folds a signed PDF into a typed `ConformanceVerdict` carrying its DocMDP certification level, signature validity, coverage, DSS/LTV completeness, and the structural-conformance result authored upstream. pyhanko and pikepdf are admitted in the manifest; this owner closes an already-emitted PDF, consuming the embed-audit precondition from `typography/font#FONT` and the PDF/UA structural-conformance result from `accessibility/tagged#ACCESS` rather than disclaiming it, and contributes the `ArtifactReceipt.Verdict` case carrying the `ConformanceVerdict` value object. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key.

## [01]-[INDEX]

- [01]-[SIGN]: pyhanko PAdES B-LTA signing and pyhanko/pikepdf conformance-audit owner over the closed `ConformStep` step table; `PadesLevel` is the policy-as-value level row whose derived properties carry the LTV behavior, `CertifyPerm` the certification-permission row resolving the typed `MDPPerm`, and `ConformanceVerdict` the audit verdict value object the `AUDIT` arm folds and the `receipt/receipt#RECEIPT` `Verdict` case carries.

## [02]-[SIGN]

- Owner: `Conformance` the one PDF-cryptographic-close owner discriminating the conformance step; `ConformStep` the closed `StrEnum` over PAdES signing and archival audit; one frozen `_STEP_TABLE` `MappingProxyType` data-row dispatch maps each step to its `StepAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction. pyhanko owns the CMS/CAdES/PAdES signing engine, the DSS/LTV embedding, the timestamp chaining, and the embedded-signature validation; pikepdf owns the XMP/structural read. `PadesLevel` and `CertifyPerm` are the two policy-as-value `StrEnum` rows whose members carry their own behavior (`needs_timestamp`/`embeds_validation`/`archival`, the `MDPPerm` resolution); `ConformanceVerdict` is the carried audit-verdict value object the `AUDIT` arm folds.
- Cases: `ConformStep` rows `SIGN` (pyhanko PAdES B-B through B-LTA — `SimpleSigner.load`/`load_pkcs12` or `ExternalSigner` injected-signature credential, the `SigFieldSpec`/`append_signature_field` field placement, `PdfSignatureMetadata` PAdES/certify knobs, `sign_pdf` over an `IncrementalPdfFileWriter`, the `HTTPTimeStamper` authority, and the `PdfTimeStamper.update_archival_timestamp_chain` LTA refresh) · `AUDIT` (pyhanko `EmbeddedPdfSignature.evaluate_signature_coverage`/`validate_pdf_signature`/`read_certification_data` + `DocumentSecurityStore.read_dss` DSS presence + pikepdf XMP read folding a `ConformanceVerdict`, consuming the `accessibility/tagged#ACCESS` structural-conformance result) — selected by the frozen `_STEP_TABLE` row, never a chain of `is`-probes; the PAdES level (`B-B`/`B-T`/`B-LT`/`B-LTA`) is the `PadesLevel` policy row carried in `params` whose `needs_timestamp`/`embeds_validation`/`archival` properties derive the LTV chain, and the certification level the `CertifyPerm` row whose `mdp` resolves the pyhanko `MDPPerm` member.
- Entry: `Conformance.close` dispatches the step over the input PDF through the one `_STEP_TABLE[step]` acceptor lookup and returns a `RuntimeRail[ContentKey]`; signing adds the cryptographic-and-LTV layer, audit proves the signed/archival/structural close. `SIGN`/`AUDIT` resolve synchronously over the cp315-core pure-Python pyhanko/pikepdf wheels (`py3-none-any`, the crypto natives carried by `asn1crypto`/`cryptography`) — the `AUDIT` arm uses the synchronous `validate_pdf_signature` (the catalogue's sync variant, functionally equivalent to `async_validate_pdf_signature`), so the owner stays on the synchronous runtime `boundary` and never forces an async dispatch path.
- Auto: signing loads the `SimpleSigner` credential or the `ExternalSigner` injected-signature signer, appends the `SigFieldSpec` field through `append_signature_field` when a box is supplied, builds `PdfSignatureMetadata(field_name=..., subfilter=SigSeedSubFilter.PADES, use_pades_lta=..., embed_validation_info=..., validation_context=..., certify=..., docmdp_permissions=...)`, runs `sign_pdf` over the `IncrementalPdfFileWriter` with the `HTTPTimeStamper`, then refreshes the archival chain through `PdfTimeStamper.update_archival_timestamp_chain` when the level is B-LTA; audit folds the embedded signature through `PdfFileReader.embedded_signatures[0]` -> `EmbeddedPdfSignature.evaluate_signature_coverage` -> `validate_pdf_signature` -> `read_certification_data`, reads `DocumentSecurityStore.read_dss(reader)` for the LTV DSS-presence signal, reads the pikepdf `open_metadata` XMP into the archival-metadata flag, folds in the `accessibility/tagged#ACCESS` structural-conformance result, and projects a `ConformanceVerdict`.
- Receipt: the `SIGN` arm contributes `ArtifactReceipt.Pdf` carrying the content key, the output byte count, and the page count, and the `AUDIT` arm contributes the `ArtifactReceipt.Verdict` case carrying the content key and the `ConformanceVerdict` value whose `facts` projection surfaces the PAdES validity, coverage level, modification level, DocMDP certification level, LTV completeness, and archival-metadata presence — never a per-step receipt. The verdict value object is the one acyclic value-object edge the `receipt/receipt#RECEIPT` union imports (the `sign` owner never reciprocally imports `ArtifactReceipt`, so the edge stays acyclic); the structural-conformance result the `AUDIT` arm folds in is interior evidence the `ConformanceVerdict` carries, not a new receipt field.
- Packages: `pyhanko` (settled: `SimpleSigner.load`/`load_pkcs12`, `ExternalSigner(signing_cert=, cert_registry=, signature_value=)`, `sign_pdf`, `IncrementalPdfFileWriter`, `HTTPTimeStamper`, `append_signature_field`, `SigFieldSpec(sig_field_name=, on_page=, box=)`, `SigSeedSubFilter.PADES`, `MDPPerm.NO_CHANGES`/`FILL_FORMS`/`ANNOTATE`, `PdfFileReader.embedded_signatures`, `EmbeddedPdfSignature.evaluate_signature_coverage`, `validate_pdf_signature`, `read_certification_data`, `PdfSignatureStatus.bottom_line`/`modification_level`/`coverage`, `DocMDPInfo.permission`, `SignatureCoverageLevel.name`, `DocumentSecurityStore.read_dss`, `PdfSignatureMetadata` with `field_name=`/`subfilter=`/`use_pades_lta=`/`embed_validation_info=`/`validation_context=`/`certify=`/`docmdp_permissions=`, `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, ...)`; RESEARCH: the `PdfTimeStamper(timestamper)` constructor and the `update_archival_timestamp_chain(..., output=)` sink keyword the catalogue rows the chain-update entrypoint without enumerating the constructor or the output keyword), `pikepdf` (`Pdf.open` + `open_metadata` XMP read settled), `pyhanko_certvalidator` (`ValidationContext` arriving at the boundary), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`), `msgspec` (`Struct`/`msgpack.Encoder`).
- Growth: a new conformance step is one `ConformStep` row plus one `_STEP_TABLE` acceptor entry; a new PAdES level is one `PadesLevel` row whose derived properties carry its LTV behavior; a new certification permission is one `CertifyPerm` row; a new audit fact is one field on `ConformanceVerdict`; zero new surface.
- Boundary: no PDF authoring (that stays at `documents/emit#DOCUMENT`), no font engineering (that is `typography/font#FONT`), no glyph rendering (that is `typography/shape#SHAPE`); the owner closes an already-emitted PDF, never producing one. pyhanko does NOT enforce PDF/A or PDF/UA structural conformance on its own — it treats them as ordinary PDF, so the structural-conformance verdict is authored upstream at `accessibility/tagged#ACCESS` (the pikepdf `StructTreeRoot`/`StructElem` marked-content tree over the `documents/model#NODE` `StructureNode`/`StructEltKind` family) and the `AUDIT` arm CONSUMES that structural result rather than disclaiming it, folding it into the `ConformanceVerdict`; the embed-completeness precondition the PDF/A close requires arrives from `typography/font#FONT` `EMBED_AUDIT`. The `ExternalSigner` injected-signature path covers HSM/remote signing where the signature bytes arrive pre-computed, replacing a `PdfSigner.digest_doc_for_signing` two-phase round-trip with the single `sign_pdf` call. A parallel `_PadesLevel` enum beside the policy behavior and a weak `int` certification permission are the collapsed forms — `PadesLevel` carries its LTV properties and `CertifyPerm.mdp` resolves the typed `MDPPerm`. The `AUDIT` arm reads version/XMP presence over pikepdf and DSS presence over `DocumentSecurityStore.read_dss` rather than claiming a veraPDF-grade structural verdict (no pure-Python PDF/A structural validator resolves on PyPI — the structural verdict is the `accessibility/tagged#ACCESS` PDF/UA result, not a self-authored one); the `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary, never built by this owner.

```python signature
import io
from collections.abc import Callable
from enum import StrEnum
from types import MappingProxyType
from typing import Final

import msgspec
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

type StepAcceptor = Callable[["Conformance"], bytes]

_VERDICT_ENCODER: Final = msgspec.msgpack.Encoder()


class ConformStep(StrEnum):
    SIGN = "sign"
    AUDIT = "audit"


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


class CertifyPerm(StrEnum):
    NO_CHANGES = "NO_CHANGES"
    FILL_FORMS = "FILL_FORMS"
    ANNOTATE = "ANNOTATE"

    def mdp(self) -> object:
        from pyhanko.sign.fields import MDPPerm

        return getattr(MDPPerm, self.value)


class ConformParams(Struct, frozen=True, kw_only=True):
    pades_level: PadesLevel = PadesLevel.B_LTA
    field_name: str = "Signature1"
    field_box: tuple[float, float, float, float] | None = None
    page: int = 0
    key_file: str | None = None
    cert_file: str | None = None
    pfx_file: str | None = None
    passphrase: bytes | None = None
    tsa_url: str | None = None
    external_signature: bytes | None = None
    certify_perm: CertifyPerm | None = None
    validation_context: object | None = None
    structural_conformant: bool = False


class ConformanceVerdict(Struct, frozen=True):
    signature_valid: bool
    coverage_level: str
    modification_level: str
    certification_level: str
    ltv_complete: bool
    archival_metadata: bool
    structural_conformant: bool

    def facts(self) -> dict[str, str]:
        return {
            "signature_valid": str(self.signature_valid),
            "coverage": self.coverage_level,
            "modification": self.modification_level,
            "certification": self.certification_level,
            "ltv_complete": str(self.ltv_complete),
            "archival_metadata": str(self.archival_metadata),
            "structural_conformant": str(self.structural_conformant),
        }


class Conformance(Struct, frozen=True):
    step: ConformStep
    pdf: bytes
    params: ConformParams

    def close(self) -> RuntimeRail[ContentKey]:
        return boundary(f"sign.{self.step}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"sign-{self.step}", _STEP_TABLE[self.step](self))


def _sign_pdf(conform: "Conformance") -> bytes:
    from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
    from pyhanko.pdf_utils.reader import PdfFileReader
    from pyhanko.sign import ExternalSigner, PdfSignatureMetadata, PdfTimeStamper, SimpleSigner, sign_pdf
    from pyhanko.sign.fields import SigFieldSpec, SigSeedSubFilter, append_signature_field
    from pyhanko.sign.timestamps import HTTPTimeStamper

    params = conform.params
    level = params.pades_level
    writer = IncrementalPdfFileWriter(io.BytesIO(conform.pdf))
    if params.field_box is not None:
        append_signature_field(writer, SigFieldSpec(sig_field_name=params.field_name, on_page=params.page, box=params.field_box))
    meta = PdfSignatureMetadata(
        field_name=params.field_name,
        subfilter=SigSeedSubFilter.PADES,
        use_pades_lta=level.archival,
        embed_validation_info=level.embeds_validation,
        validation_context=params.validation_context,
        certify=params.certify_perm is not None,
        docmdp_permissions=params.certify_perm.mdp() if params.certify_perm is not None else None,
    )
    timestamper = HTTPTimeStamper(params.tsa_url) if level.needs_timestamp else None
    signer = (
        ExternalSigner(signing_cert=None, cert_registry=None, signature_value=params.external_signature)
        if params.external_signature is not None
        else SimpleSigner.load_pkcs12(params.pfx_file, passphrase=params.passphrase)
        if params.pfx_file is not None
        else SimpleSigner.load(params.key_file, params.cert_file, key_passphrase=params.passphrase)
    )
    sink = io.BytesIO()
    sign_pdf(writer, meta, signer, timestamper=timestamper, output=sink)
    if not (level.archival and params.tsa_url is not None and params.validation_context is not None):
        return sink.getvalue()
    refreshed = io.BytesIO()
    # RESEARCH: the PdfTimeStamper(timestamper) constructor and update_archival_timestamp_chain's output= sink
    # keyword are unverified — the pyhanko catalogue rows update_archival_timestamp_chain(reader, validation_context,
    # ...) at entrypoint [04] without enumerating the PdfTimeStamper constructor or the output keyword; the
    # reader/validation_context arguments and the HTTPTimeStamper/use_pades_lta/embed_validation_info knobs are settled.
    PdfTimeStamper(HTTPTimeStamper(params.tsa_url)).update_archival_timestamp_chain(
        PdfFileReader(io.BytesIO(sink.getvalue())), params.validation_context, output=refreshed
    )
    return refreshed.getvalue()


def _audit_pdf(conform: "Conformance") -> bytes:
    import pikepdf
    from pyhanko.pdf_utils.reader import PdfFileReader
    from pyhanko.sign.validation import DocumentSecurityStore, read_certification_data, validate_pdf_signature

    reader = PdfFileReader(io.BytesIO(conform.pdf))
    embedded = reader.embedded_signatures[0]
    status = validate_pdf_signature(embedded, conform.params.validation_context)
    docmdp = read_certification_data(reader)
    dss = DocumentSecurityStore.read_dss(reader)
    with pikepdf.open(io.BytesIO(conform.pdf)) as pdf, pdf.open_metadata() as meta:
        has_metadata = bool(meta)
    return _VERDICT_ENCODER.encode(
        ConformanceVerdict(
            signature_valid=status.bottom_line,
            coverage_level=embedded.evaluate_signature_coverage().name,
            modification_level=str(status.modification_level),
            certification_level=docmdp.permission.name if docmdp is not None else "none",
            ltv_complete=bool(dss.certs),
            archival_metadata=has_metadata,
            structural_conformant=conform.params.structural_conformant,
        )
    )


_STEP_TABLE: Final[MappingProxyType[ConformStep, StepAcceptor]] = MappingProxyType({
    ConformStep.SIGN: _sign_pdf,
    ConformStep.AUDIT: _audit_pdf,
})
```

## [03]-[RESEARCH]

- [SUBSET_SIGN] [RESOLVED]: the pyhanko `SimpleSigner.load(key_file, cert_file, ca_chain_files, key_passphrase, ...)`/`SimpleSigner.load_pkcs12(pfx_file, ca_chain_files, other_certs, passphrase, ...)`, `ExternalSigner(signing_cert, cert_registry, signature_value=None, ...)`, `PdfSignatureMetadata(field_name=, subfilter=, embed_validation_info=, use_pades_lta=, validation_context=, certify=, docmdp_permissions=, ...)`, `sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, ..., output=)`, `IncrementalPdfFileWriter`, `HTTPTimeStamper(url, https, timeout, auth, headers)`, `SigFieldSpec(sig_field_name, on_page=0, box=None, ...)`, `append_signature_field(pdf_out, sig_field_spec)`, `SigSeedSubFilter` (`ADOBE_PKCS7_DETACHED`/`PADES`), and `MDPPerm` (`NO_CHANGES`/`FILL_FORMS`/`ANNOTATE`) entrypoint spellings verify against the folder `.api` catalogue for `pyhanko` (`0.35.1`) signing entrypoint rows `[01]`-`[03]`/`[01]`, the `PdfSignatureMetadata` knob table rows `[01]`-`[04]`, the field-management rows `[01]`/`[02]`, and field/seed-value public-type rows `[01]`/`[05]`/`[06]` — the catalogue spells the full `PdfSignatureMetadata` constructor (`field_name`/`subfilter`/`embed_validation_info`/`use_pades_lta`/`validation_context`/`certify`/`docmdp_permissions`), the `ExternalSigner(signing_cert, cert_registry, signature_value=)` constructor, and the `SigFieldSpec(sig_field_name, on_page, box)` constructor, so the `_sign_pdf` body is settled fence code apart from the one marked LTA-chain leg. The `CertifyPerm` `StrEnum` values are the exact `MDPPerm` member names, so `CertifyPerm.mdp` resolves the typed permission through one `getattr(MDPPerm, self.value)` rather than a weak `int`; the `ExternalSigner` injected-signature path covers HSM/remote signing where the signature bytes arrive pre-computed, replacing a `PdfSigner.digest_doc_for_signing` two-phase round-trip with the single `sign_pdf` call. The PAdES level rises from B-B to B-LTA as `subfilter=SigSeedSubFilter.PADES` plus `use_pades_lta=True` engages the LTV chain (catalogue knob row `[04]`), `embed_validation_info=True` writes the DSS validation material (knob row `[03]`), and `update_archival_timestamp_chain(reader, validation_context, ...)` refreshes the archival timestamp on the B-LTA close. The `_sign_pdf` body folds the credential selection — `ExternalSigner` injected-signature, `SimpleSigner.load_pkcs12`, or `SimpleSigner.load` — into one polymorphic conditional expression, builds the writer/metadata/timestamper/signer inline, and inlines the archival-chain refresh tail when the level is archival, so no second-tier `_metadata`/`_writer`/`_signer`/`_refresh_archival` single-call hop survives. RESEARCH: only the `PdfTimeStamper(timestamper)` constructor and the `update_archival_timestamp_chain(..., output=)` sink keyword stay unverified — the catalogue rows `update_archival_timestamp_chain(reader, validation_context, ...)` at entrypoint row `[04]` without enumerating the `PdfTimeStamper` constructor or the `output` keyword, so the `_sign_pdf` body gates the LTA-chain constructor-and-sink behind the explicit `# RESEARCH` comment and they stay unverified until the `pyhanko` catalogue rows them.
- [AUDIT] [RESOLVED]: the pyhanko `PdfFileReader.embedded_signatures`, `EmbeddedPdfSignature.evaluate_signature_coverage` (returning `SignatureCoverageLevel`), `validate_pdf_signature(embedded, signer_validation_context, ts_validation_context, diff_policy)` (returning `PdfSignatureStatus`), `read_certification_data(reader)` (returning `DocMDPInfo`), `PdfSignatureStatus.bottom_line`/`modification_level`/`coverage`, `DocMDPInfo.permission`, `SignatureCoverageLevel.name`, and `DocumentSecurityStore.read_dss(reader)` entrypoint spellings verify against the folder `.api` catalogue for `pyhanko` validation entrypoint rows `[01]`/`[08]`/`[09]`, validation public-type rows `[01]`/`[02]`/`[05]`/`[06]`/`[07]`, and document-I/O row `[01]` — the catalogue spells `PdfSignatureStatus` with `bottom_line`/`coverage`/`modification_level`/`docmdp_ok` and `DocMDPInfo` as the `MDPPerm`-level carrier, and the installed distribution reflects `DocMDPInfo` as the `(permission, author_sig)` record and `PdfSignatureStatus.bottom_line`/`modification_level`/`coverage` as live members, so the `_audit_pdf` body is settled fence code. The earlier `PdfSignatureStatus.validation_path` LTV signal is a phantom — the catalogue rows the status carrier without a `validation_path` member and the installed distribution exposes none — so the LTV-completeness signal reads `DocumentSecurityStore.read_dss(reader).certs` DSS-presence (the catalogued DSS owner row `[07]`, "VRI/OCSP/CRL/cert store embedded in the PDF for LTV") instead: a non-empty DSS cert store is the honest "validation material embedded" signal a B-LT/B-LTA artifact carries. The synchronous `validate_pdf_signature` is functionally equivalent to `async_validate_pdf_signature` (catalogue: sync and async APIs are equivalent), so the audit arm stays on the synchronous `boundary` rather than forcing an async dispatch path. The `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary (catalogue: the signing owner does not build it). The pikepdf `Pdf.open`/`open_metadata` XMP read verifies against the `pikepdf` catalogue; no pure-Python PDF/A structural validator resolves on PyPI, so the structural-conformance verdict is the `accessibility/tagged#ACCESS` PDF/UA result the `AUDIT` arm folds into `ConformanceVerdict.structural_conformant` rather than a self-authored veraPDF-grade verdict — the live `accessibility/tagged#ACCESS` owner authors the pikepdf `StructTreeRoot`/`StructElem` marked-content tree FROM the `documents/model#NODE` `StructureNode`/`StructEltKind` family and audits its structure-element count / tag-tree depth / alt-text coverage, and that boolean conformance result threads in through `ConformParams.structural_conformant`, closing the structural gap pyhanko honestly disclaims.
