# [PY_ARTIFACTS_API_PYHANKO]

`pyhanko` is the PDF digital-signature and validation owner for the artifacts pdf rail. `PdfSigner`, `SimpleSigner`, `ExternalSigner`, and `PdfTimeStamper` own signing; `validate_pdf_signature`/`validate_pdf_timestamp`/`validate_cms_signature` and `add_validation_info`/`DocumentSecurityStore` own verification and DSS embedding; `PdfFileReader`/`IncrementalPdfFileWriter` own append-only document I/O; `SigFieldSpec`/`SigSeedValueSpec`/`append_signature_field` own field placement and seed-value constraints; `diff_analysis` owns the pluggable modification policy; `ades`/`qualified` own PAdES baseline and EU-qualified profiles. The signing owner composes these against a `pyhanko_certvalidator.ValidationContext` (trust roots, OCSP/CRL fetch, revocation mode); it never builds CMS by hand or forks a parallel signing path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyhanko`
- package: `pyhanko`
- import: `pyhanko`
- owner: `artifacts`
- rail: pdf
- version: `0.35.1`; pairs with `pyhanko-certvalidator 0.31.1` for the validation context
- license: `MIT` (permissive; no copyleft obligation)
- abi: `py3-none-any` pure Python (`oscrypto`/`asn1crypto`/`cryptography` carry the crypto natives); `Requires-Python >=3.10`; manifest pin `>=0.35.1`
- entry points: none as a library dependency (the distribution also ships a `pyhanko` CLI, unused by this rail)
- capability: PDF digital signature (CMS/CAdES/PAdES B-B/B-T/B-LT/B-LTA), document certification (DocMDP/FieldMDP), signature-field + seed-value management, CMS + detached-CMS validation, DSS embedding, RFC 3161 timestamp chaining, LTV archival refresh, pluggable diff analysis, two-phase HSM/remote signing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signing principal family
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                              |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------------------------- |
|  [01]   | `Signer`               | abstract base      | CMS signing contract; `async_sign` / `sign` protocol; `signing_cert`     |
|  [02]   | `SimpleSigner`         | key+cert signer    | load PEM key/cert or PKCS#12; in-process `sign_raw`; `prefer_pss`         |
|  [03]   | `ExternalSigner`       | detached signer    | pre-computed `signature_value: bytes` or deferred `int` slot for HSM/remote |
|  [04]   | `PdfSigner`            | PDF signing engine | field placement, CMS build, byte-range reservation, optional `stamp_style` |
|  [05]   | `PdfTimeStamper`       | LTV stamper        | apply + chain document timestamp fields on PDF revisions                 |
|  [06]   | `PdfSignatureMetadata` | signing descriptor | field name, reason, certify, subfilter, DSS, PAdES-LTA, DocMDP, CAdES attrs |

[PUBLIC_TYPE_SCOPE]: timestamp client family
- rail: pdf — `pyhanko.sign.timestamps`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :---------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `TimeStamper`           | abstract base   | TSA protocol contract; `async_timestamp`      |
|  [02]   | `HTTPTimeStamper`       | HTTP TSA client | RFC 3161 over HTTP/HTTPS with `auth`/`headers` |
|  [03]   | `DummyTimeStamper`      | test stub       | deterministic timestamp for unit-test use     |
|  [04]   | `TimestampRequestError` | TSA fault       | TSA request or response parsing failure       |

[PUBLIC_TYPE_SCOPE]: field and seed-value family
- rail: pdf — `pyhanko.sign.fields`

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]      | [CAPABILITY]                                                       |
| :-----: | :----------------------------------- | :----------------- | :---------------------------------------------------------------- |
|  [01]   | `SigFieldSpec`                       | field descriptor   | `sig_field_name`, `on_page`, `box`, seed value, MDP lock, appearance |
|  [02]   | `SigSeedValueSpec`                   | seed value dict    | allowed subfilters, digest methods, cert constraints, rev-info reqs |
|  [03]   | `SigCertConstraints` / `SigCertKeyUsage` | cert policy   | key usage, issuer, subject, policy OID restrictions               |
|  [04]   | `FieldMDPSpec` / `FieldMDPAction`    | field MDP lock     | lock named fields after signing                                   |
|  [05]   | `MDPPerm`                            | certification enum | `NO_CHANGES`, `FILL_FORMS`, `ANNOTATE`                            |
|  [06]   | `SigSeedSubFilter`                   | subfilter enum     | `ADOBE_PKCS7_DETACHED`, `PADES`                                  |
|  [07]   | `SigSeedValFlags` / `SigCertConstraintFlags` | constraint flags | which seed-value entries are mandatory                     |
|  [08]   | `VisibleSigSettings` / `InvisSigSettings` | appearance flags | print/hidden/rotate-with-page/scale behavior                |

[PUBLIC_TYPE_SCOPE]: validation, status, and diff family
- rail: pdf — `pyhanko.sign.validation`, `pyhanko.sign.diff_analysis`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                               |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `EmbeddedPdfSignature`    | signature handle   | `signer_cert`, `compute_digest`, `evaluate_signature_coverage`, `evaluate_modifications` |
|  [02]   | `PdfSignatureStatus`      | signature result   | `trusted`/`revoked`/`coverage`/`modification_level`/`docmdp_ok`/`seed_value_ok`/`diff_result`/`bottom_line` |
|  [03]   | `DocumentTimestampStatus` | timestamp result   | timestamp trust + coverage                                                |
|  [04]   | `StandardCMSSignatureStatus` | raw CMS result | detached-CMS validation status (no PDF)                                   |
|  [05]   | `SignatureCoverageLevel`  | coverage enum      | `ENTIRE_FILE`, `ENTIRE_REVISION`, `CONTIGUOUS_BLOCK_FROM_START`, `UNCLEAR` |
|  [06]   | `DocMDPInfo`              | certification info | `MDPPerm` level read from a certifying signature                          |
|  [07]   | `DocumentSecurityStore`   | DSS owner          | VRI/OCSP/CRL/cert store embedded in the PDF for LTV                       |
|  [08]   | `DiffPolicy` / `StandardDiffPolicy` / `ModificationLevel` | diff policy | pluggable post-signature modification analysis            |
|  [09]   | `KeyUsageConstraints`     | usage policy       | key-usage / extended-key-usage acceptance constraints                     |

[PUBLIC_TYPE_SCOPE]: document I/O and certvalidator context
- rail: pdf — `pyhanko.pdf_utils`, `pyhanko_certvalidator`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
| :-----: | :------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `PdfFileReader`            | document reader    | open a PDF stream; revision access; decrypt                  |
|  [02]   | `IncrementalPdfFileWriter` | incremental writer | append-only update over an existing reader stream            |
|  [03]   | `BasePdfFileWriter`        | writer base        | root/info/obj graph; stream xref control                     |
|  [04]   | `ValidationContext`        | trust context      | trust roots, OCSP/CRL fetch, `revocation_mode`, `moment`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: credential loading and signers
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `SimpleSigner.load(key_file, cert_file, ca_chain_files=None, key_passphrase=None, other_certs=None, signature_mechanism=None, prefer_pss=False)` | PEM loader | load PEM key + cert chain |
|  [02]   | `SimpleSigner.load_pkcs12(pfx_file, ca_chain_files=None, other_certs=None, passphrase=None, signature_mechanism=None, prefer_pss=False)` | PKCS#12 loader | load a PKCS#12 bundle |
|  [03]   | `ExternalSigner(signing_cert, cert_registry, signature_value=None, signature_mechanism=None, prefer_pss=False, embed_roots=True)` | HSM/remote signer | detached signer with injected or deferred signature bytes |
|  [04]   | `load_certs_from_pemder(cert_files)`                                                              | cert loader    | parse a PEM/DER cert list             |

[ENTRYPOINT_SCOPE]: PDF signing
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SURFACE]                                                                                                                       | [ENTRY_FAMILY]    | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :---------------- | :--------------------------------------------- |
|  [01]   | `sign_pdf(pdf_out, signature_meta, signer, timestamper=None, new_field_spec=None, existing_fields_only=False, bytes_reserved=None, in_place=False, output=None)` | sync sign | sign a writer in-place or to an output stream |
|  [02]   | `async_sign_pdf(pdf_out, signature_meta, signer, timestamper=None, new_field_spec=None, ...)`                                  | async sign        | coroutine variant of `sign_pdf`                |
|  [03]   | `PdfSigner(signature_meta, signer, *, timestamper=None, stamp_style=None, new_field_spec=None)`                                | engine ctor       | reusable signer with visible appearance        |
|  [04]   | `PdfSigner.sign_pdf(pdf_out, existing_fields_only=False, bytes_reserved=None, *, appearance_text_params=None, in_place=False, output=None)` | object sign | sign via a `PdfSigner` instance         |
|  [05]   | `PdfSigner.async_sign_pdf(pdf_out, ...)`                                                                                        | object async sign | coroutine variant of instance sign             |
|  [06]   | `PdfSigner.digest_doc_for_signing(pdf_out, ...) -> (PreparedByteRangeDigest, PdfTBSDocument, IO)`                              | two-phase prepare | byte range + digest for external HSM/remote sig |
|  [07]   | `PdfSigner.async_digest_doc_for_signing(pdf_out, ...)`                                                                          | two-phase async   | coroutine variant of two-phase prepare         |

[ENTRYPOINT_SCOPE]: signing descriptor knobs
- rail: pdf — `pyhanko.sign.PdfSignatureMetadata`

`PdfSignatureMetadata(field_name=None, md_algorithm=None, location=None, reason=None, contact_info=None, name=None, app_build_props=None, prop_auth_time=None, prop_auth_type=None, certify=False, subfilter=None, embed_validation_info=False, use_pades_lta=False, timestamp_field_name=None, validation_context=None, docmdp_permissions=MDPPerm.FILL_FORMS, signer_key_usage=..., cades_signed_attr_spec=None, dss_settings=DSSContentSettings(...), tight_size_estimates=False, ac_validation_context=None)`

| [INDEX] | [KNOB]                  | [CAPABILITY]                                                          |
| :-----: | :---------------------- | :------------------------------------------------------------------- |
|  [01]   | `certify` + `docmdp_permissions` | author certification signature with a DocMDP permission level |
|  [02]   | `subfilter`             | `PADES` (CAdES-detached PAdES) vs `ADOBE_PKCS7_DETACHED`              |
|  [03]   | `embed_validation_info` + `validation_context` | embed OCSP/CRL at sign time (PAdES B-LT)        |
|  [04]   | `use_pades_lta`         | add the archival document timestamp (PAdES B-LTA)                    |
|  [05]   | `cades_signed_attr_spec` | CAdES signed-attribute spec (commitment type, signer location)      |
|  [06]   | `dss_settings`          | `DSSContentSettings` controlling VRI placement and DSS write         |

[ENTRYPOINT_SCOPE]: timestamp application
- rail: pdf — `pyhanko.sign.PdfTimeStamper`, `pyhanko.sign.timestamps`

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `HTTPTimeStamper(url, https=False, timeout=5, auth=None, headers=None)`           | TSA client     | RFC 3161 TSA over HTTP/HTTPS         |
|  [02]   | `PdfTimeStamper.timestamp_pdf(pdf_out, md_algorithm, validation_context, ...)`    | sync stamp     | add a document timestamp field       |
|  [03]   | `PdfTimeStamper.async_timestamp_pdf(pdf_out, md_algorithm, ...)`                  | async stamp    | coroutine variant of `timestamp_pdf` |
|  [04]   | `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, ...)` | chain update   | extend LTV archival timestamp chain  |
|  [05]   | `PdfTimeStamper.async_update_archival_timestamp_chain(reader, ...)`               | chain async    | coroutine variant of chain update    |

[ENTRYPOINT_SCOPE]: signature field management
- rail: pdf — `pyhanko.sign.fields`

| [INDEX] | [SURFACE]                                                                                              | [ENTRY_FAMILY]   | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------------------------------------- | :--------------- | :--------------------------------------- |
|  [01]   | `append_signature_field(pdf_out, sig_field_spec)`                                                     | field writer     | add a signature field to a writer        |
|  [02]   | `SigFieldSpec(sig_field_name, on_page=0, box=None, seed_value_dict=None, field_mdp_spec=None, doc_mdp_update_value=None, combine_annotation=True, empty_field_appearance=False, invis_sig_settings=InvisSigSettings(...), readable_field_name=None, visible_sig_settings=VisibleSigSettings(...))` | field descriptor | declare a field placement + seed value; both `invis_sig_settings` and `visible_sig_settings` carry the appearance flags, `doc_mdp_update_value` sets the per-field DocMDP level |
|  [03]   | `enumerate_sig_fields(handler, filled_status=None, with_name=None)`                                   | field enumerator | iterate signature fields with fill state |
|  [04]   | `prepare_sig_field(sig_field_name, root, update_writer, ...)`                                         | field preparer   | locate or create a field for signing     |

[ENTRYPOINT_SCOPE]: validation, DSS, and raw CMS
- rail: pdf — `pyhanko.sign.validation`

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY]     | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `validate_pdf_signature(embedded_sig, signer_validation_context=None, ts_validation_context=None, diff_policy=None, key_usage_settings=None, skip_diff=False) -> PdfSignatureStatus` | sync validate | validate an embedded CMS signature |
|  [02]   | `async_validate_pdf_signature(embedded_sig, signer_validation_context=None, ts_validation_context=None, ac_validation_context=None, diff_policy=None, ...)` | async validate | coroutine variant of signature validation |
|  [03]   | `validate_pdf_timestamp(embedded_sig, validation_context=None, diff_policy=None, skip_diff=False) -> DocumentTimestampStatus` | timestamp validate | validate a document timestamp field |
|  [04]   | `validate_cms_signature(...)` / `validate_detached_cms(...)` (+ `async_` mirrors)                                  | raw CMS validate   | validate a CMS/detached-CMS blob outside a PDF    |
|  [05]   | `add_validation_info(embedded_sig, validation_context, skip_timestamp=False, add_vri_entry=True, in_place=False, output=None)` | DSS embed | embed validation info into the PDF DSS |
|  [06]   | `async_add_validation_info(embedded_sig, validation_context, ...)`                                                 | DSS async embed    | coroutine variant of DSS embedding                |
|  [07]   | `collect_validation_info(embedded_sig, validation_context, skip_timestamp=False)`                                  | info collect       | gather OCSP/CRL material without writing          |
|  [08]   | `read_certification_data(reader) -> DocMDPInfo`                                                                    | certification read | retrieve DocMDP info from a certifying signature  |
|  [09]   | `EmbeddedPdfSignature.compute_digest(...)` / `evaluate_signature_coverage()` / `evaluate_modifications(...)` / `compute_integrity_info(...)` / `signer_cert`        | coverage eval      | byte-range digest, `SignatureCoverageLevel`, diff, the consolidated integrity record, and the embedded signer certificate |

[ENTRYPOINT_SCOPE]: document I/O and trust context
- rail: pdf — `pyhanko.pdf_utils`, `pyhanko_certvalidator`

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `PdfFileReader(stream, strict=True)`                                                       | reader open    | open PDF from binary stream                |
|  [02]   | `PdfFileReader.decrypt(password)` / `get_historical_resolver(revision)`                    | reader access  | decrypt; reach an earlier revision         |
|  [03]   | `IncrementalPdfFileWriter(input_stream, prev=None, strict=True)`                           | writer open    | append-only update writer over reader      |
|  [04]   | `ValidationContext(trust_roots=None, extra_trust_roots=None, other_certs=None, moment=None, allow_fetching=False, crls=None, ocsps=None, revocation_mode='soft-fail', retroactive_revinfo=False)` | trust context | build the cert/revocation trust context |

## [04]-[IMPLEMENTATION_LAW]

[SIGN_TOPOLOGY]:
- signing path: `SimpleSigner.load`/`load_pkcs12` (or `ExternalSigner` for HSM/remote) -> `PdfSignatureMetadata(...)` -> `sign_pdf(pdf_out, meta, signer, timestamper)` or `PdfSigner(meta, signer, stamp_style=...).sign_pdf(pdf_out)`. The async mirror is functionally equivalent and preferred inside a coroutine.
- incremental update: always sign over an `IncrementalPdfFileWriter(stream)`; signing mutates append-only revisions, never a fresh writer, so existing signatures keep `ENTIRE_REVISION` coverage.
- field path: `SigFieldSpec(name, on_page, box, seed_value_dict=SigSeedValueSpec(...))` -> `append_signature_field` to register; `enumerate_sig_fields` to locate existing; `SigSeedValueSpec`/`SigCertConstraints` enforce signer policy at field-creation time, never a post-sign rejection.
- two-phase signing: `digest_doc_for_signing` returns a `PreparedByteRangeDigest` + `PdfTBSDocument`; the external HSM/remote service signs the digest, the bytes are injected via `ExternalSigner.signature_value`, and the TBS document is finalized — the only path for non-exportable keys.
- timestamp: `HTTPTimeStamper(url, https=True, timeout=..., auth=..., headers=...)` passes as the `timestamper` argument (PAdES B-T) or drives `PdfTimeStamper` directly; `DummyTimeStamper` is test-only.
- PAdES ladder: `subfilter=SigSeedSubFilter.PADES` is B-B; add a `timestamper` for B-T; set `embed_validation_info=True` with a `validation_context` for B-LT; set `use_pades_lta=True` and run `update_archival_timestamp_chain` for B-LTA archival refresh.
- validation path: `PdfFileReader(stream)` -> `EmbeddedPdfSignature` (from `reader.embedded_signatures`) -> `validate_pdf_signature(sig, signer_validation_context=ValidationContext(...), diff_policy=DEFAULT_DIFF_POLICY)` -> `PdfSignatureStatus`. The status carries `trusted`, `revoked`, `coverage` (`SignatureCoverageLevel`), `modification_level`, `docmdp_ok`, `seed_value_ok`, and `diff_result`; the validation receipt reads these fields, never a stringified `summary`.
- diff axis: `diff_analysis` supplies the ready `DEFAULT_DIFF_POLICY` and `NO_CHANGES_DIFF_POLICY` instances plus `StandardDiffPolicy(global_rules, form_rule, reject_object_freeing=True)` for composable whitelist rules; a certify-then-fill workflow accepts form fills while rejecting structural edits through the policy, never a manual revision byte-compare.
- DSS/LTV axis: `add_validation_info(sig, validation_context)` writes OCSP/CRL + cert material into a `DocumentSecurityStore` so the signature stays verifiable after issuer certs expire; `collect_validation_info` gathers the same material without writing for an out-of-band store.
- evidence: each sign/validate op captures field name, subfilter, certify flag + DocMDP level, coverage level, trust + revocation outcome, modification level, timestamp validity, and DSS presence as a pdf-signature receipt.
- boundary: pyhanko owns CMS/CAdES/PAdES signing + validation, DSS, and timestamp chaining; the trust/revocation context comes from `pyhanko_certvalidator.ValidationContext`; certificate/key material is parsed by `asn1crypto`/`cryptography`; PDF structure repair/linearization/encryption routes to `pikepdf`; render/extract routes to `pymupdf`; never a hand-rolled CMS encoder or a second signing path.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyhanko`
- Owns: PDF digital signature (CMS/CAdES/PAdES B-B/B-T/B-LT/B-LTA), document certification (DocMDP/FieldMDP), signature-field + seed-value management, embedded + raw-CMS validation, DSS embedding, RFC 3161 timestamp + LTV archival chaining, pluggable diff analysis, two-phase HSM/remote signing
- Accept: PDF streams as binary I/O over `IncrementalPdfFileWriter`; certificates as PEM/DER files, PKCS#12, or `asn1crypto` objects; a `pyhanko_certvalidator.ValidationContext` for trust/revocation
- Reject: hand-rolled CMS construction; a fresh (non-incremental) writer for append-only signing; building the `ValidationContext` independently of `pyhanko_certvalidator`; a manual revision byte-compare where `diff_analysis` owns the policy; wrapper-renames of the signing/validation entrypoints
