# [PY_ARTIFACTS_API_PYHANKO]

`pyhanko` supplies a PDF digital signature and validation pipeline for the artifacts pdf rail: `PdfSigner`, `SimpleSigner`, `ExternalSigner`, and `PdfTimeStamper` own the signing surface; `validate_pdf_signature`, `validate_pdf_timestamp`, and `add_validation_info` own the verification surface; `PdfFileReader` and `IncrementalPdfFileWriter` own the document I/O layer; `SigFieldSpec` and `append_signature_field` own field placement.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyhanko`
- package: `pyhanko`
- import: `pyhanko`
- owner: `artifacts`
- rail: pdf
- entry points: none (library only)
- capability: PDF digital signature (CMS/CAdES/PAdES), document certification, signature field management, CMS validation, DSS embedding, and document timestamp chaining

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signing principal family
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :----------------- | :-------------------------------------------------------------- |
|   [1]   | `Signer`               | abstract base      | CMS signing contract; `async_sign` / `sign` protocol            |
|   [2]   | `SimpleSigner`         | key+cert signer    | load PEM key/cert or PKCS#12; delegate `sign_raw`               |
|   [3]   | `ExternalSigner`       | detached signer    | pre-computed signature bytes or deferred integer slot           |
|   [4]   | `PdfSigner`            | PDF signing engine | coordinate field placement, CMS build, byte-range reservation   |
|   [5]   | `PdfTimeStamper`       | LTV stamper        | apply and chain document timestamp fields on PDF revisions      |
|   [6]   | `PdfSignatureMetadata` | signing descriptor | field name, reason, certify flag, DSS settings, PAdES/LTA knobs |

[PUBLIC_TYPE_SCOPE]: timestamp client family
- rail: pdf — `pyhanko.sign.timestamps`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                              |
| :-----: | :---------------------- | :-------------- | :---------------------------------------- |
|   [1]   | `TimeStamper`           | abstract base   | TSA protocol contract                     |
|   [2]   | `HTTPTimeStamper`       | HTTP TSA client | RFC 3161 requests over HTTP/HTTPS         |
|   [3]   | `DummyTimeStamper`      | test stub       | deterministic timestamp for unit test use |
|   [4]   | `TimestampRequestError` | TSA fault       | TSA request or response parsing failure   |

[PUBLIC_TYPE_SCOPE]: field and seed-value family
- rail: pdf — `pyhanko.sign.fields`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [CAPABILITY]                                                   |
| :-----: | :------------------- | :----------------- | :------------------------------------------------------------- |
|   [1]   | `SigFieldSpec`       | field descriptor   | name, page, box, seed values, MDP lock, appearance             |
|   [2]   | `SigSeedValueSpec`   | seed value dict    | allowed subfilters, digest methods, cert constraints, rev info |
|   [3]   | `SigCertConstraints` | cert policy        | key usage, issuer, subject, policy OID restrictions            |
|   [4]   | `FieldMDPSpec`       | field MDP lock     | lock named fields after signing                                |
|   [5]   | `MDPPerm`            | certification enum | `NO_CHANGES`, `FILL_FORMS`, `ANNOTATE`                         |
|   [6]   | `SigSeedSubFilter`   | subfilter enum     | `ADOBE_PKCS7_DETACHED`, `PADES`                                |
|   [7]   | `SigAuthType`        | auth type enum     | signature authentication type seed value                       |

[PUBLIC_TYPE_SCOPE]: validation and coverage family
- rail: pdf — `pyhanko.sign.validation`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                               |
| :-----: | :------------------------ | :----------------- | :------------------------------------------------------------------------- |
|   [1]   | `EmbeddedPdfSignature`    | signature handle   | access CMS data, compute digest, evaluate coverage                         |
|   [2]   | `PdfSignatureStatus`      | signature result   | trust, coverage, modification level, seed validity                         |
|   [3]   | `DocumentTimestampStatus` | timestamp result   | timestamp trust and coverage                                               |
|   [4]   | `SignatureCoverageLevel`  | coverage enum      | `ENTIRE_FILE`, `ENTIRE_REVISION`, `CONTIGUOUS_BLOCK_FROM_START`, `UNCLEAR` |
|   [5]   | `DocMDPInfo`              | certification info | `MDPPerm` level read from a certifying signature                           |
|   [6]   | `DiffPolicy`              | diff policy base   | pluggable modification analysis policy                                     |

[PUBLIC_TYPE_SCOPE]: document I/O family
- rail: pdf — `pyhanko.pdf_utils`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                      |
| :-----: | :------------------------- | :----------------- | :------------------------------------------------ |
|   [1]   | `PdfFileReader`            | document reader    | open a PDF stream; revision access; decrypt       |
|   [2]   | `IncrementalPdfFileWriter` | incremental writer | append-only update over an existing reader stream |
|   [3]   | `BasePdfFileWriter`        | writer base        | root/info/obj graph; stream xref control          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: credential loading
- rail: pdf — `pyhanko.sign.SimpleSigner`

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :------------------------ |
|   [1]   | `SimpleSigner.load(key_file, cert_file, ca_chain_files, key_passphrase, ...)` | PEM loader     | load PEM key + cert chain |
|   [2]   | `SimpleSigner.load_pkcs12(pfx_file, ca_chain_files, passphrase, ...)`         | PKCS#12 loader | load PKCS#12 bundle       |
|   [3]   | `load_certs_from_pemder(cert_files)`                                          | cert loader    | parse PEM/DER cert list   |

[ENTRYPOINT_SCOPE]: PDF signing
- rail: pdf — `pyhanko.sign`

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY]    | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------------------------------------------- | :---------------- | :--------------------------------------------- |
|   [1]   | `sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, existing_fields_only, ...)` | sync sign         | sign a PDF writer in-place or to output stream |
|   [2]   | `async_sign_pdf(pdf_out, signature_meta, signer, timestamper, new_field_spec, ...)`                 | async sign        | coroutine variant of `sign_pdf`                |
|   [3]   | `PdfSigner.sign_pdf(pdf_out, existing_fields_only, bytes_reserved, ...)`                            | object sign       | sign via `PdfSigner` instance                  |
|   [4]   | `PdfSigner.async_sign_pdf(pdf_out, ...)`                                                            | object async sign | coroutine variant of instance sign             |
|   [5]   | `PdfSigner.digest_doc_for_signing(pdf_out, ...)`                                                    | two-phase sign    | prepare byte range and digest for external sig |
|   [6]   | `PdfSigner.async_digest_doc_for_signing(pdf_out, ...)`                                              | two-phase async   | coroutine variant of two-phase prepare         |

[ENTRYPOINT_SCOPE]: timestamp application
- rail: pdf — `pyhanko.sign.PdfTimeStamper`

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `PdfTimeStamper.timestamp_pdf(pdf_out, md_algorithm, validation_context, ...)`    | sync stamp     | add a document timestamp field       |
|   [2]   | `PdfTimeStamper.async_timestamp_pdf(pdf_out, md_algorithm, ...)`                  | async stamp    | coroutine variant of `timestamp_pdf` |
|   [3]   | `PdfTimeStamper.update_archival_timestamp_chain(reader, validation_context, ...)` | chain update   | extend LTV archival timestamp chain  |
|   [4]   | `PdfTimeStamper.async_update_archival_timestamp_chain(reader, ...)`               | chain async    | coroutine variant of chain update    |

[ENTRYPOINT_SCOPE]: signature field management
- rail: pdf — `pyhanko.sign.fields`

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]   | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------ | :--------------- | :--------------------------------------- |
|   [1]   | `append_signature_field(pdf_out, sig_field_spec)`             | field writer     | add a signature field to a writer        |
|   [2]   | `enumerate_sig_fields(handler, filled_status, with_name)`     | field enumerator | iterate signature fields with fill state |
|   [3]   | `prepare_sig_field(sig_field_name, root, update_writer, ...)` | field preparer   | locate or create a field for signing     |

[ENTRYPOINT_SCOPE]: validation
- rail: pdf — `pyhanko.sign.validation`

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY]     | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :----------------- | :------------------------------------------------ |
|   [1]   | `validate_pdf_signature(embedded_sig, signer_validation_context, ts_validation_context, diff_policy, ...)` | sync validate      | validate an embedded CMS signature                |
|   [2]   | `async_validate_pdf_signature(embedded_sig, signer_validation_context, ts_validation_context, ...)`        | async validate     | coroutine variant of signature validation         |
|   [3]   | `validate_pdf_timestamp(embedded_sig, validation_context, diff_policy, skip_diff)`                         | timestamp validate | validate a document timestamp field               |
|   [4]   | `add_validation_info(embedded_sig, validation_context, skip_timestamp, add_vri_entry, ...)`                | DSS embed          | embed validation info into the PDF DSS            |
|   [5]   | `async_add_validation_info(embedded_sig, validation_context, ...)`                                         | DSS async embed    | coroutine variant of DSS embedding                |
|   [6]   | `collect_validation_info(embedded_sig, validation_context, skip_timestamp)`                                | info collect       | gather OCSP/CRL material without writing          |
|   [7]   | `read_certification_data(reader)`                                                                          | certification read | retrieve `DocMDPInfo` from a certifying signature |
|   [8]   | `EmbeddedPdfSignature.compute_digest(hash_algo)`                                                           | digest compute     | compute byte-range hash of the embedded signature |
|   [9]   | `EmbeddedPdfSignature.evaluate_signature_coverage()`                                                       | coverage eval      | return `SignatureCoverageLevel`                   |

[ENTRYPOINT_SCOPE]: document I/O
- rail: pdf — `pyhanko.pdf_utils`

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `PdfFileReader(stream, strict=True)`                        | reader open    | open PDF from binary stream                |
|   [2]   | `PdfFileReader.decrypt(password)`                           | reader decrypt | decrypt password-protected PDF             |
|   [3]   | `PdfFileReader.get_historical_resolver(revision)`           | history access | access an earlier document revision        |
|   [4]   | `IncrementalPdfFileWriter(input_stream, prev, strict=True)` | writer open    | open incremental update writer over reader |

## [4]-[IMPLEMENTATION_LAW]

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
