# [PY_ARTIFACTS_API_PYHANKO]

`pyhanko` owns PDF digital signing and signature validation for the artifacts pdf rail: CMS/CAdES/PAdES signing across the B-B/B-T/B-LT/B-LTA ladder, document certification, DSS embedding, RFC 3161 timestamp and LTV chaining, pluggable diff analysis, and two-phase HSM/remote signing. Every signer composes a `pyhanko_certvalidator.ValidationContext` for trust and revocation; CMS is never hand-built and no parallel signing path forks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyhanko`
- package: `pyhanko` (MIT)
- module: `pyhanko`
- namespaces: `pyhanko.sign`, `pyhanko.sign.fields`, `pyhanko.sign.validation`, `pyhanko.sign.diff_analysis`, `pyhanko.sign.timestamps`, `pyhanko.sign.ades.api`, `pyhanko.sign.signers.pdf_signer`, `pyhanko.stamp`, `pyhanko.pdf_utils`, `pyhanko_certvalidator`
- rail: pdf — CMS/CAdES/PAdES signing + validation, document certification, DSS/LTV, RFC 3161 timestamp chaining, diff analysis, two-phase HSM/remote signing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signing principal family — `pyhanko.sign`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :--------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Signer`               | abstract base      | CMS signing contract; `async_sign` / `sign` protocol; `signing_cert`        |
|  [02]   | `SimpleSigner`         | key+cert signer    | load PEM key/cert or PKCS#12; in-process `sign_raw`; `prefer_pss`           |
|  [03]   | `ExternalSigner`       | detached signer    | pre-computed `signature_value: bytes` or deferred `int` slot for HSM/remote |
|  [04]   | `PdfSigner`            | PDF signing engine | field placement, CMS build, byte-range reservation, optional `stamp_style`  |
|  [05]   | `PdfTimeStamper`       | LTV stamper        | apply + chain document timestamp fields on PDF revisions                    |
|  [06]   | `PdfSignatureMetadata` | signing descriptor | field name, reason, certify, subfilter, DSS, PAdES-LTA, DocMDP, CAdES attrs |

[PUBLIC_TYPE_SCOPE]: timestamp client family — `pyhanko.sign.timestamps`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                   |
| :-----: | :---------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `TimeStamper`           | abstract base   | TSA protocol contract; `async_timestamp`       |
|  [02]   | `HTTPTimeStamper`       | HTTP TSA client | RFC 3161 over HTTP/HTTPS with `auth`/`headers` |
|  [03]   | `DummyTimeStamper`      | test stub       | deterministic timestamp for unit-test use      |
|  [04]   | `TimestampRequestError` | TSA fault       | TSA request or response parsing failure        |

[PUBLIC_TYPE_SCOPE]: field and seed-value family — `pyhanko.sign.fields`

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]      | [CAPABILITY]                                                         |
| :-----: | :------------------------------------------- | :----------------- | :------------------------------------------------------------------- |
|  [01]   | `SigFieldSpec`                               | field descriptor   | `sig_field_name`, `on_page`, `box`, seed value, MDP lock, appearance |
|  [02]   | `SigSeedValueSpec`                           | seed value dict    | allowed subfilters, digest methods, cert constraints, rev-info reqs  |
|  [03]   | `SigCertConstraints` / `SigCertKeyUsage`     | cert policy        | key usage, issuer, subject, policy OID restrictions                  |
|  [04]   | `FieldMDPSpec` / `FieldMDPAction`            | field MDP lock     | lock named fields after signing                                      |
|  [05]   | `MDPPerm`                                    | certification enum | `NO_CHANGES`, `FILL_FORMS`, `ANNOTATE`                               |
|  [06]   | `SigSeedSubFilter`                           | subfilter enum     | `ADOBE_PKCS7_DETACHED`, `PADES`                                      |
|  [07]   | `SigSeedValFlags` / `SigCertConstraintFlags` | constraint flags   | which seed-value entries are mandatory                               |
|  [08]   | `VisibleSigSettings` / `InvisSigSettings`    | appearance flags   | print/hidden/rotate-with-page/scale behavior                         |

[PUBLIC_TYPE_SCOPE]: validation, status, and diff family — `pyhanko.sign.validation`, `pyhanko.sign.diff_analysis`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]      | [CAPABILITY]                                                            |
| :-----: | :---------------------------------- | :----------------- | :---------------------------------------------------------------------- |
|  [01]   | `EmbeddedPdfSignature`              | signature handle   | `compute_digest`, `evaluate_signature_coverage`/`_modifications`        |
|  [02]   | `PdfSignatureStatus`                | signature result   | signature verdict; fields below                                         |
|  [03]   | `DocumentTimestampStatus`           | timestamp result   | timestamp trust + coverage                                              |
|  [04]   | `StandardCMSSignatureStatus`        | raw CMS result     | detached-CMS validation status (no PDF)                                 |
|  [05]   | `SignatureCoverageLevel`            | coverage enum      | `ENTIRE_FILE`/`ENTIRE_REVISION`/`CONTIGUOUS_BLOCK_FROM_START`/`UNCLEAR` |
|  [06]   | `DocMDPInfo`                        | certification info | `MDPPerm` level read from a certifying signature                        |
|  [07]   | `DocumentSecurityStore`             | DSS owner          | VRI/OCSP/CRL/cert store embedded in the PDF for LTV                     |
|  [08]   | `DiffPolicy` / `StandardDiffPolicy` | diff policy        | pluggable post-signature modification analysis                          |
|  [09]   | `ModificationLevel`                 | diff-level enum    | the modification level a diff policy reports                            |
|  [10]   | `KeyUsageConstraints`               | usage policy       | key-usage / extended-key-usage acceptance constraints                   |

- [02]-[PDFSIGNATURESTATUS] fields: `trusted`, `revoked`, `coverage`, `modification_level`, `docmdp_ok`, `seed_value_ok`, `diff_result`, `bottom_line`, `signing_cert` (`asn1crypto` `x509.Certificate`), `md_algorithm`, `pkcs7_signature_mechanism`, `signer_reported_dt`, `timestamp_validity`/`content_timestamp_validity` (`TimestampSignatureStatus | None`), `qualification_result` (`QualificationResult | None` whose `.status` is a `QualifiedStatus` carrying `qualified`/`qc_type`/`qc_key_security`).

[PUBLIC_TYPE_SCOPE]: document I/O and certvalidator context — `pyhanko.pdf_utils`, `pyhanko_certvalidator`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                             |
| :-----: | :------------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `PdfFileReader`            | document reader    | open a PDF stream; revision access; decrypt              |
|  [02]   | `IncrementalPdfFileWriter` | incremental writer | append-only update over an existing reader stream        |
|  [03]   | `BasePdfFileWriter`        | writer base        | root/info/obj graph; stream xref control                 |
|  [04]   | `ValidationContext`        | trust context      | trust roots, OCSP/CRL fetch, `revocation_mode`, `moment` |

[PUBLIC_TYPE_SCOPE]: visible appearance / stamp family — `pyhanko.stamp`

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]       | [CAPABILITY]                                                                     |
| :-----: | :--------------- | :------------------ | :------------------------------------------------------------------------------- |
|  [01]   | `BaseStampStyle` | appearance base     | abstract visible-seal style `PdfSigner(stamp_style=)` renders onto the field     |
|  [02]   | `TextStampStyle` | text seal           | `stamp_text` (`%(...)s`), `border_width`, `background_opacity`; positioned seal  |
|  [03]   | `QRStampStyle`   | scan-to-verify seal | `TextStampStyle` + a QR encoding the `%(url)s` link, positioned by `qr_position` |
|  [04]   | `QRPosition`     | QR placement enum   | `LEFT_OF_TEXT` / `RIGHT_OF_TEXT` / `ABOVE_TEXT` / `BELOW_TEXT`                   |

[PUBLIC_TYPE_SCOPE]: CAdES signed-attribute and DSS placement family — `pyhanko.sign.ades.api`, `pyhanko.sign.signers.pdf_signer`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]        | [CAPABILITY]                                                                  |
| :-----: | :-------------------------- | :------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `CAdESSignedAttrSpec`       | signed-attr spec     | `commitment_type`, signature-policy, signer-attribute CAdES signed attributes |
|  [02]   | `GenericCommitment`         | commitment-type enum | `PROOF_OF_{ORIGIN,RECEIPT,DELIVERY,SENDER,APPROVAL,CREATION}`                 |
|  [03]   | `DSSContentSettings`        | DSS write policy     | `include_vri`, `placement` controlling the DSS/VRI revision write             |
|  [04]   | `SigDSSPlacementPreference` | DSS placement enum   | `SEPARATE_REVISION` / `TOGETHER_WITH_NEXT_TS` / `TOGETHER_WITH_SIGNATURE`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: credential loading and signers — `pyhanko.sign`
- `SimpleSigner` loaders and `ExternalSigner` carry: `signature_mechanism=None, prefer_pss=False`; `load` adds `key_passphrase=None`, `load_pkcs12` a `passphrase=None`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]           | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------- | :---------------- | :------------------------------------------- |
|  [01]   | `SimpleSigner.load(key_file, cert_file, ca_chain_files=None, …)` | PEM loader        | load PEM key + cert chain                    |
|  [02]   | `SimpleSigner.load_pkcs12(pfx_file, ca_chain_files=None, …)`     | PKCS#12 loader    | load a PKCS#12 bundle                        |
|  [03]   | `ExternalSigner(signing_cert, cert_registry, …)`                 | HSM/remote signer | detached signer with injected/deferred bytes |
|  [04]   | `load_certs_from_pemder(cert_files)`                             | cert loader       | parse a PEM/DER cert list                    |

[ENTRYPOINT_SCOPE]: PDF signing — `pyhanko.sign`
- sign entrypoints carry: `existing_fields_only=False, bytes_reserved=None, in_place=False, output=None`.

| [INDEX] | [SURFACE]                                                     | [SHAPE]           | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------ | :---------------- | :------------------------------------------------- |
|  [01]   | `sign_pdf(pdf_out, signature_meta, signer, …)`                | sync sign         | sign a writer in-place or to an output stream      |
|  [02]   | `async_sign_pdf(pdf_out, signature_meta, signer, …)`          | async sign        | coroutine variant of `sign_pdf`                    |
|  [03]   | `PdfSigner(signature_meta, signer, …)`                        | engine ctor       | reusable signer with visible appearance            |
|  [04]   | `PdfSigner.sign_pdf(pdf_out, appearance_text_params=None, …)` | object sign       | sign via a `PdfSigner` instance                    |
|  [05]   | `PdfSigner.async_sign_pdf(pdf_out, …)`                        | object async sign | coroutine variant of instance sign                 |
|  [06]   | `PdfSigner.digest_doc_for_signing(pdf_out, …)`                | two-phase prepare | `-> (PreparedByteRangeDigest, PdfTBSDocument, IO)` |
|  [07]   | `PdfSigner.async_digest_doc_for_signing(pdf_out, …)`          | two-phase async   | coroutine variant of two-phase prepare             |
|  [08]   | `ExternalSigner.signed_attrs(digest, algorithm, …)`           | async attributes  | coroutine producing signed CMS attributes          |
|  [09]   | `Signer.async_sign_prescribed_attributes(algorithm, attrs)`   | CMS finalize      | coroutine CMS assembly over prescribed attrs       |
|  [10]   | `PreparedByteRangeDigest.fill_with_cms(output, cms)`          | PDF finalize      | fill the prepared byte-range reservation           |

[ENTRYPOINT_SCOPE]: signing descriptor knobs — `pyhanko.sign.PdfSignatureMetadata`
- defaults: `certify=False`, `docmdp_permissions=MDPPerm.FILL_FORMS`, `dss_settings=DSSContentSettings(...)`; remaining knobs default `None`.

| [INDEX] | [SURFACE]                                      | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `certify` + `docmdp_permissions`               | knob    | author certification signature with a DocMDP permission level  |
|  [02]   | `subfilter`                                    | knob    | `PADES` (CAdES-detached PAdES) vs `ADOBE_PKCS7_DETACHED`       |
|  [03]   | `embed_validation_info` + `validation_context` | knob    | embed OCSP/CRL at sign time (PAdES B-LT)                       |
|  [04]   | `use_pades_lta`                                | knob    | add the archival document timestamp (PAdES B-LTA)              |
|  [05]   | `cades_signed_attr_spec`                       | knob    | CAdES signed-attribute spec (commitment type, signer location) |
|  [06]   | `dss_settings`                                 | knob    | `DSSContentSettings` controlling VRI placement and DSS write   |

[ENTRYPOINT_SCOPE]: timestamp application — `pyhanko.sign.timestamps`, `PdfTimeStamper`

| [INDEX] | [SURFACE]                                                                       | [SHAPE]      | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------ | :----------- | :----------------------------------- |
|  [01]   | `HTTPTimeStamper(url, https=False, timeout=5, auth=None, headers=None)`         | TSA client   | RFC 3161 TSA over HTTP/HTTPS         |
|  [02]   | `PdfTimeStamper.timestamp_pdf(pdf_out, md_algorithm, validation_context, …)`    | sync stamp   | add a document timestamp field       |
|  [03]   | `PdfTimeStamper.async_timestamp_pdf(pdf_out, md_algorithm, …)`                  | async stamp  | coroutine variant of `timestamp_pdf` |
|  [04]   | `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, …)` | chain update | extend LTV archival timestamp chain  |
|  [05]   | `PdfTimeStamper.async_update_archival_timestamp_chain(reader, …)`               | chain async  | coroutine variant of chain update    |

[ENTRYPOINT_SCOPE]: signature field management — `pyhanko.sign.fields`

| [INDEX] | [SURFACE]                                              | [SHAPE]          | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `append_signature_field(pdf_out, sig_field_spec)`      | field writer     | add a signature field to a writer                 |
|  [02]   | `SigFieldSpec(sig_field_name, on_page=0, box=None, …)` | field descriptor | field placement + seed value; appearance + DocMDP |
|  [03]   | `enumerate_sig_fields(handler, filled_status=None, …)` | field enumerator | iterate signature fields with fill state          |
|  [04]   | `prepare_sig_field(sig_field_name, root, …)`           | field preparer   | locate or create a field for signing              |

[ENTRYPOINT_SCOPE]: visible appearance, CAdES attributes, and DSS placement — `pyhanko.stamp`, `pyhanko.sign.ades.api`, `pyhanko.sign.signers.pdf_signer`
- stamp-style ctors carry: `stamp_text=, border_width=, background_opacity=`; `DSSContentSettings` `placement=` takes a `SigDSSPlacementPreference`.

| [INDEX] | [SURFACE]                                       | [SHAPE]          | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `TextStampStyle(…)`                             | text seal ctor   | positioned seal fed to `PdfSigner(stamp_style=)`                |
|  [02]   | `QRStampStyle(…, qr_position=)`                 | QR seal ctor     | scan-to-verify seal; `appearance_text_params` carries `%(url)s` |
|  [03]   | `CAdESSignedAttrSpec(commitment_type=, …)`      | signed-attr ctor | attach a CAdES commitment-type + policy signed attribute        |
|  [04]   | `GenericCommitment.<PROOF_OF_*>.asn1`           | commitment value | ASN.1 commitment-type object for `CAdESSignedAttrSpec`          |
|  [05]   | `DSSContentSettings(include_vri=, placement=…)` | DSS policy ctor  | control the DSS/VRI revision placement at sign time             |

[ENTRYPOINT_SCOPE]: validation, DSS, and raw CMS — `pyhanko.sign.validation`
- validators take `embedded_sig` first and carry: `signer_validation_context=None, ts_validation_context=None, diff_policy=None`; signature/timestamp validators add `skip_diff=False`; `validate_pdf_signature -> PdfSignatureStatus`, `validate_pdf_timestamp -> DocumentTimestampStatus`; DSS embed/collect share `(embedded_sig, validation_context, skip_timestamp=False)`; `validate_cms_signature`/`validate_detached_cms` carry `async_` mirrors.

| [INDEX] | [SURFACE]                                                | [SHAPE]            | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `validate_pdf_signature(embedded_sig, …)`                | sync validate      | validate an embedded CMS signature               |
|  [02]   | `async_validate_pdf_signature(embedded_sig, …)`          | async validate     | coroutine variant of signature validation        |
|  [03]   | `validate_pdf_timestamp(embedded_sig, …)`                | timestamp validate | validate a document timestamp field              |
|  [04]   | `validate_cms_signature(…)` / `validate_detached_cms(…)` | raw CMS validate   | validate a CMS/detached-CMS blob outside a PDF   |
|  [05]   | `add_validation_info(…, add_vri_entry=True, …)`          | DSS embed          | embed validation info into the PDF DSS           |
|  [06]   | `async_add_validation_info(…)`                           | DSS async embed    | coroutine variant of DSS embedding               |
|  [07]   | `collect_validation_info(…)`                             | info collect       | gather OCSP/CRL material without writing         |
|  [08]   | `read_certification_data(reader) -> DocMDPInfo`          | certification read | retrieve DocMDP info from a certifying signature |
|  [09]   | `EmbeddedPdfSignature` coverage eval                     | coverage eval      | digest, coverage, diff, integrity; members below |

- [09]-[EMBEDDEDPDFSIGNATURE] members: `compute_digest`, `evaluate_signature_coverage`, `evaluate_modifications`, `compute_integrity_info`, `signer_cert`.

[ENTRYPOINT_SCOPE]: document I/O and trust context — `pyhanko.pdf_utils`, `pyhanko_certvalidator`
- `ValidationContext` gates all network OCSP/CRL fetch behind `allow_fetching=False`.

| [INDEX] | [SURFACE]                                                                | [SHAPE]       | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `PdfFileReader(stream, strict=True)`                                     | reader open   | open PDF from binary stream             |
|  [02]   | `PdfFileReader.decrypt(password)` / `get_historical_resolver(revision)`  | reader access | decrypt; reach an earlier revision      |
|  [03]   | `IncrementalPdfFileWriter(input_stream, prev=None, strict=True)`         | writer open   | append-only update writer over reader   |
|  [04]   | `ValidationContext(trust_roots=None, …, revocation_mode='soft-fail', …)` | trust context | build the cert/revocation trust context |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Signing folds `SimpleSigner.load`/`load_pkcs12` (or `ExternalSigner` for HSM/remote) -> `PdfSignatureMetadata(...)` -> `sign_pdf(pdf_out, meta, signer, timestamper)` or `PdfSigner(meta, signer, stamp_style=...).sign_pdf(pdf_out)`; the `async_` mirror is the coroutine-scope form.
- Signing runs over an `IncrementalPdfFileWriter(stream)`, mutating append-only revisions, so a prior signature keeps its `ENTIRE_REVISION` coverage; a fresh writer breaks that coverage.
- Field placement folds `SigFieldSpec(name, on_page, box, seed_value_dict=SigSeedValueSpec(...))` -> `append_signature_field`, `enumerate_sig_fields` locating an existing field; `SigSeedValueSpec`/`SigCertConstraints` enforce signer policy at field-creation time, never a post-sign rejection.
- Two-phase signing folds `digest_doc_for_signing(pdf_out, *, appearance_text_params=, output=) -> (PreparedByteRangeDigest, PdfTBSDocument, IO)`, then `ExternalSigner.signed_attrs(digest, algorithm, use_pades=True)` over `PreparedByteRangeDigest.document_digest`; the external service signs `CMSAttributes.dump()`, a sealed `ExternalSigner(signature_value=raw_sig)` finalizes through `sign_prescribed_attributes`, and `PreparedByteRangeDigest.fill_with_cms(output, cms)` fills the reserved region — the only path for a non-exportable key, an `int` `signature_value` the size-estimation placeholder.
- Timestamping passes `HTTPTimeStamper(url, https=True, …)` as the `timestamper` (PAdES B-T) or drives `PdfTimeStamper` directly; `DummyTimeStamper` is test-only.
- PAdES ladder: `subfilter=SigSeedSubFilter.PADES` is B-B, a `timestamper` adds B-T, `embed_validation_info=True` with a `validation_context` adds B-LT, `use_pades_lta=True` and `update_archival_timestamp_chain` add B-LTA archival refresh.
- Validation folds `PdfFileReader(stream)` -> `EmbeddedPdfSignature` (from `reader.embedded_signatures`) -> `validate_pdf_signature(sig, signer_validation_context=ValidationContext(...), diff_policy=DEFAULT_DIFF_POLICY)` -> `PdfSignatureStatus`; the receipt reads the status fields, never a stringified `summary`.
- `diff_analysis` supplies `DEFAULT_DIFF_POLICY`/`NO_CHANGES_DIFF_POLICY` and `StandardDiffPolicy(global_rules, form_rule, reject_object_freeing=True)`; a certify-then-fill workflow accepts form fills and rejects structural edits through the policy, never a manual revision byte-compare.
- `add_validation_info(sig, validation_context)` writes OCSP/CRL + cert material into a `DocumentSecurityStore`, keeping the signature verifiable past issuer-cert expiry; `collect_validation_info` gathers the same material without writing.
- Each sign/validate op captures field name, subfilter, certify + DocMDP level, coverage level, trust + revocation outcome, modification level, timestamp validity, and DSS presence as one pdf-signature receipt.

[STACKING]:
- `pypdf`(`.api/pypdf.md`) / `pikepdf`(`.api/pikepdf.md`): the assembled, repaired, linearized, and sanitized bytes are the `IncrementalPdfFileWriter` input pyhanko signs append-only; `pikepdf.sanitize`/`flatten_annotations` precede the sign.
- `pymupdf`(`.api/pymupdf.md`) / `pypdfium2`(`.api/pypdfium2.md`): render the page raster the `SigFieldSpec` + `TextStampStyle`/`QRStampStyle` visible seal draws over.
- `pdf-oxide`(`.api/pdf-oxide.md`): the dependency-free byte-level PAdES signer stands in where pyhanko's cryptography stack is barred.
- within-lib: `exchange/conformance` composes the PAdES ladder and the two-phase HSM/remote flow, whose synchronous CMS offload enters once through `anyio.run`(`libs/python/.api/anyio.md`).
- within-lib: `expression`(`libs/python/.api/expression.md`) maps the `PdfSignatureStatus` verdict (`trusted`/`revoked`/`coverage`/`modification_level`) and validation faults onto `Result`; `msgspec`(`libs/python/.api/msgspec.md`) + `structlog`(`libs/python/.api/structlog.md`) carry the signature receipt and its span.

[LOCAL_ADMISSION]:
- `import pyhanko` at boundary scope; sign over `IncrementalPdfFileWriter`, resolve trust/revocation through one `pyhanko_certvalidator.ValidationContext`.

[RAIL_LAW]:
- Package: `pyhanko`
- Owns: PDF digital signature (CMS/CAdES/PAdES B-B/B-T/B-LT/B-LTA), document certification (DocMDP/FieldMDP), signature-field + seed-value management, embedded + raw-CMS validation, DSS embedding, RFC 3161 timestamp + LTV archival chaining, pluggable diff analysis, two-phase HSM/remote signing
- Accept: PDF streams as binary I/O over `IncrementalPdfFileWriter`; certificates as PEM/DER files, PKCS#12, or `asn1crypto` objects; a `pyhanko_certvalidator.ValidationContext` for trust/revocation
- Reject: hand-rolled CMS construction; a fresh non-incremental writer for append-only signing; a `ValidationContext` built independently of `pyhanko_certvalidator`; a manual revision byte-compare where `diff_analysis` owns the policy; wrapper-renames of the signing/validation entrypoints
