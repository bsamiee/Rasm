# [PY_ARTIFACTS_API_PYHANKO]

`pyhanko` supplies a PDF digital signature and validation pipeline for the artifacts pdf rail: `PdfSigner`, `SimpleSigner`, `ExternalSigner`, and `PdfTimeStamper` own the signing surface; `validate_pdf_signature`, `validate_pdf_timestamp`, and `add_validation_info` own the verification surface; `PdfFileReader` and `IncrementalPdfFileWriter` own the document I/O layer; `SigFieldSpec` and `append_signature_field` own field placement.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyhanko`
- package: `pyhanko`
- import: `pyhanko`
- owner: `artifacts`
- rail: pdf
- entry points: none (library only)
- capability: PDF digital signature (CMS/CAdES/PAdES), document certification, signature field management, CMS validation, DSS embedding, and document timestamp chaining

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signing principal family
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `Signer`               | abstract base      | CMS signing contract; `async_sign` / `sign` protocol            |
|  [02]   | `SimpleSigner`         | key+cert signer    | load PEM key/cert or PKCS#12; delegate `sign_raw`               |
|  [03]   | `ExternalSigner`       | detached signer    | pre-computed signature bytes or deferred integer slot           |
|  [04]   | `PdfSigner`            | PDF signing engine | coordinate field placement, CMS build, byte-range reservation   |
|  [05]   | `PdfTimeStamper`       | LTV stamper        | apply and chain document timestamp fields on PDF revisions      |
|  [06]   | `PdfSignatureMetadata` | signing descriptor | field name, reason, certify flag, DSS settings, PAdES/LTA knobs |

[PUBLIC_TYPE_SCOPE]: timestamp client family
- rail: pdf — `pyhanko.sign.timestamps`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                              |
| :-----: | :---------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `TimeStamper`           | abstract base   | TSA protocol contract                     |
|  [02]   | `HTTPTimeStamper`       | HTTP TSA client | RFC 3161 requests over HTTP/HTTPS         |
|  [03]   | `DummyTimeStamper`      | test stub       | deterministic timestamp for unit test use |
|  [04]   | `TimestampRequestError` | TSA fault       | TSA request or response parsing failure   |

[PUBLIC_TYPE_SCOPE]: field and seed-value family
- rail: pdf — `pyhanko.sign.fields`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [CAPABILITY]                                                   |
| :-----: | :------------------- | :----------------- | :------------------------------------------------------------- |
|  [01]   | `SigFieldSpec`       | field descriptor   | name, page, box, seed values, MDP lock, appearance             |
|  [02]   | `SigSeedValueSpec`   | seed value dict    | allowed subfilters, digest methods, cert constraints, rev info |
|  [03]   | `SigCertConstraints` | cert policy        | key usage, issuer, subject, policy OID restrictions            |
|  [04]   | `FieldMDPSpec`       | field MDP lock     | lock named fields after signing                                |
|  [05]   | `MDPPerm`            | certification enum | `NO_CHANGES`, `FILL_FORMS`, `ANNOTATE`                         |
|  [06]   | `SigSeedSubFilter`   | subfilter enum     | `ADOBE_PKCS7_DETACHED`, `PADES`                                |
|  [07]   | `SigAuthType`        | auth type enum     | signature authentication type seed value                       |

[PUBLIC_TYPE_SCOPE]: validation and coverage family
- rail: pdf — `pyhanko.sign.validation`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                               |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `EmbeddedPdfSignature`    | signature handle   | access CMS data, compute digest, evaluate coverage                         |
|  [02]   | `PdfSignatureStatus`      | signature result   | trust, coverage, modification level, seed validity                         |
|  [03]   | `DocumentTimestampStatus` | timestamp result   | timestamp trust and coverage                                               |
|  [04]   | `SignatureCoverageLevel`  | coverage enum      | `ENTIRE_FILE`, `ENTIRE_REVISION`, `CONTIGUOUS_BLOCK_FROM_START`, `UNCLEAR` |
|  [05]   | `DocMDPInfo`              | certification info | `MDPPerm` level read from a certifying signature                           |
|  [06]   | `DiffPolicy`              | diff policy base   | pluggable modification analysis policy                                     |

[PUBLIC_TYPE_SCOPE]: document I/O family
- rail: pdf — `pyhanko.pdf_utils`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                      |
| :-----: | :------------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `PdfFileReader`            | document reader    | open a PDF stream; revision access; decrypt       |
|  [02]   | `IncrementalPdfFileWriter` | incremental writer | append-only update over an existing reader stream |
|  [03]   | `BasePdfFileWriter`        | writer base        | root/info/obj graph; stream xref control          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: credential loading
- rail: pdf — `pyhanko.sign.SimpleSigner`

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :------------------------ |
|  [01]   | `SimpleSigner.load(key_file, cert_file, ca_chain_files, key_passphrase, ...)` | PEM loader     | load PEM key + cert chain |
|  [02]   | `SimpleSigner.load_pkcs12(pfx_file, ca_chain_files, passphrase, ...)`         | PKCS#12 loader | load PKCS#12 bundle       |
|  [03]   | `load_certs_from_pemder(cert_files)`                                          | cert loader    | parse PEM/DER cert list   |

[ENTRYPOINT_SCOPE]: PDF signing
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY]    | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------------------------------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, existing_fields_only, ...)` | sync sign         | sign a PDF writer in-place or to output stream |
|  [02]   | `async_sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, ...)`                 | async sign        | coroutine variant of `sign_pdf`                |
|  [03]   | `PdfSigner.sign_pdf(pdf_out, existing_fields_only, bytes_reserved, ...)`                            | object sign       | sign via `PdfSigner` instance                  |
|  [04]   | `PdfSigner.async_sign_pdf(pdf_out, ...)`                                                            | object async sign | coroutine variant of instance sign             |
|  [05]   | `PdfSigner.digest_doc_for_signing(pdf_out, ...)`                                                    | two-phase sign    | prepare byte range and digest for external sig |
|  [06]   | `PdfSigner.async_digest_doc_for_signing(pdf_out, ...)`                                              | two-phase async   | coroutine variant of two-phase prepare         |

[ENTRYPOINT_SCOPE]: timestamp application
- rail: pdf — `pyhanko.sign.PdfTimeStamper`

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `PdfTimeStamper.timestamp_pdf(pdf_out, md_algorithm, validation_context, ...)`    | sync stamp     | add a document timestamp field       |
|  [02]   | `PdfTimeStamper.async_timestamp_pdf(pdf_out, md_algorithm, ...)`                  | async stamp    | coroutine variant of `timestamp_pdf` |
|  [03]   | `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, ...)` | chain update   | extend LTV archival timestamp chain  |
|  [04]   | `PdfTimeStamper.async_update_archival_timestamp_chain(reader, ...)`               | chain async    | coroutine variant of chain update    |

[ENTRYPOINT_SCOPE]: signature field management
- rail: pdf — `pyhanko.sign.fields`

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]   | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------ | :--------------- | :--------------------------------------- |
|  [01]   | `append_signature_field(pdf_out, sig_field_spec)`             | field writer     | add a signature field to a writer        |
|  [02]   | `enumerate_sig_fields(handler, filled_status, with_name)`     | field enumerator | iterate signature fields with fill state |
|  [03]   | `prepare_sig_field(sig_field_name, root, update_writer, ...)` | field preparer   | locate or create a field for signing     |

[ENTRYPOINT_SCOPE]: validation
- rail: pdf — `pyhanko.sign.validation`

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY]     | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `validate_pdf_signature(embedded_sig, signer_validation_context, ts_validation_context, diff_policy, ...)` | sync validate      | validate an embedded CMS signature                |
|  [02]   | `async_validate_pdf_signature(embedded_sig, signer_validation_context, ts_validation_context, ...)`        | async validate     | coroutine variant of signature validation         |
|  [03]   | `validate_pdf_timestamp(embedded_sig, validation_context, diff_policy, skip_diff)`                         | timestamp validate | validate a document timestamp field               |
|  [04]   | `add_validation_info(embedded_sig, validation_context, skip_timestamp, add_vri_entry, ...)`                | DSS embed          | embed validation info into the PDF DSS            |
|  [05]   | `async_add_validation_info(embedded_sig, validation_context, ...)`                                         | DSS async embed    | coroutine variant of DSS embedding                |
|  [06]   | `collect_validation_info(embedded_sig, validation_context, skip_timestamp)`                                | info collect       | gather OCSP/CRL material without writing          |
|  [07]   | `read_certification_data(reader)`                                                                          | certification read | retrieve `DocMDPInfo` from a certifying signature |
|  [08]   | `EmbeddedPdfSignature.compute_digest(hash_algo)`                                                           | digest compute     | compute byte-range hash of the embedded signature |
|  [09]   | `EmbeddedPdfSignature.evaluate_signature_coverage()`                                                       | coverage eval      | return `SignatureCoverageLevel`                   |

[ENTRYPOINT_SCOPE]: document I/O
- rail: pdf — `pyhanko.pdf_utils`

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `PdfFileReader(stream, strict=True)`                        | reader open    | open PDF from binary stream                |
|  [02]   | `PdfFileReader.decrypt(password)`                           | reader decrypt | decrypt password-protected PDF             |
|  [03]   | `PdfFileReader.get_historical_resolver(revision)`           | history access | access an earlier document revision        |
|  [04]   | `IncrementalPdfFileWriter(input_stream, prev, strict=True)` | writer open    | open incremental update writer over reader |

## [04]-[IMPLEMENTATION_LAW]

[SIGN_TOPOLOGY]:
- signing path: `SimpleSigner.load` or `load_pkcs12` -> `PdfSignatureMetadata` -> `sign_pdf` or `PdfSigner.sign_pdf`
- field path: `SigFieldSpec` -> `append_signature_field` to register; `enumerate_sig_fields` to locate existing
- incremental update: always pass `IncrementalPdfFileWriter(stream)` to signing; never use a fresh writer for append-only signing
- two-phase signing: `digest_doc_for_signing` returns `PreparedByteRangeDigest` for external HSM/remote signing
- timestamp: `HTTPTimeStamper(url, https=False, timeout=5)` -> pass as `timestamper` argument or to `PdfTimeStamper`
- LTV/PAdES: set `use_pades_lta=True` in `PdfSignatureMetadata`; call `update_archival_timestamp_chain` for archival refresh

[LOCAL_ADMISSION]:
- All signing and validation I/O accepts binary file-like objects; the owner manages open/close at the boundary.
- `ValidationContext` comes from `pyhanko_certvalidator`; the signing owner does not build it independently.
- Sync and async APIs are functionally equivalent; async variants are preferred when the execution context is a coroutine.
- `SimpleSigner` covers in-process key material; `ExternalSigner` covers HSM/remote signing where signature bytes are injected after digest.

[RAIL_LAW]:
- Package: `pyhanko`
- Owns: PDF digital signature (CMS/CAdES/PAdES), document certification, signature field management, validation, DSS embedding, and document timestamp chaining
- Accept: PDF streams as binary I/O; certificates as PEM/DER files or `asn1crypto` objects
- Reject: hand-rolled CMS construction; parallel PDF signing paths outside `pyhanko.sign`; wrapper-renames of the signing entrypoints
