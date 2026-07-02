# [PY_ARTIFACTS_CONFORMANCE]

The PDF cryptographic-conformance close over the document rail at the exchange boundary. `Conformance` is ONE owner — a closed `tagged_union` whose `sign`/`stamp`/`augment`/`reserve`/`audit` cases fold a PDF emitted by `document/emit#DOCUMENT` into a PAdES-signed, document-timestamped, LTV-archival, seed-value-reserved, and audited `ConformanceVerdict` — the `tagged_union` owning the dispatch, the async entry, AND every case body directly as a method, with no one-field wrapper over a separate op union and no free case-body function beside it. `pyhanko` applies PAdES baseline signatures (B-B / B-T / B-LT / B-LTA) through the `PdfSigner` engine under the `SigSeedSubFilter.PADES` subfilter with an optional `CAdESSignedAttrSpec` commitment-type signed attribute, an optional `TextStampStyle` visible drawing-sheet seal (the invisible byte-range signature and the positioned ISO-19650 professional seal folding onto ONE `Appearance` closed family selecting `stamp_style` and the matching `SigFieldSpec` appearance settings), a `signer_key_usage` sign-time key-usage assertion, a `DSSContentSettings` DSS/VRI write policy, embedded DSS validation material, an `HTTPTimeStamper` authority, a `Digest` digest-algorithm vocabulary, `MDPPerm` certification permissions, and `FieldMDPSpec` field-lock policy; the `stamp` arm applies a signer-free RFC-3161 `/DocTimeStamp` proof-of-existence timestamp through `PdfTimeStamper.timestamp_pdf`; the `augment` arm maintains long-term-archival validity through `add_validation_info` DSS embedding and `PdfTimeStamper.update_archival_timestamp_chain` archival refresh; the `reserve` arm prepares a signer-less empty signature field carrying a `SigSeedValueSpec` seed-value policy (allowed subfilter, digest set, revocation-info and timestamp requirement, and an optional `SigCertConstraints` future-signer identity constraint binding the subject DN and required/forbidden `SigCertKeyUsage` bits) through `append_signature_field` so a counterparty's later signature is constrained at field-creation time — the multi-party professional-seal prepare stage whose paired FILL arm is the `sign` case's `FieldPlacement(reserved)` modality (`PdfSigner.sign_pdf(existing_fields_only=True)` with no `new_field_spec`), so the licensed counterparty signs the constrained field the `reserve` arm opened rather than minting a second new field; and the `audit` arm RESILIENTLY folds EVERY embedded `/Sig` signature — a structurally broken signature projected to `Nothing` through `expression.extra.result.catch`, counted, never fatal — through `validate_pdf_signature` and EVERY `/DocTimeStamp` archival stamp through `validate_pdf_timestamp` under a `DiffPolicy`, aggregating the multi-signature evidence in ONE `_Tally` fold over the live statuses and projecting the primary signer's identity through ONE `_Primary` map into one typed `ConformanceVerdict` carrying the multi-signature valid/trusted/broken counts, the primary signer certificate's `subject`/`issuer`/`serial_number`, the claimed signing time and the TSA-attested timestamp time, the `md_algorithm` and `pkcs7_signature_mechanism` actually used, the bottom-line validity/trust/revocation aggregates, coverage, modification level, DocMDP certification, seed-value conformance, content-, signature-, and archival-timestamp trust, the EU-qualified status, the embedded DSS LTV-material counts (certs / OCSPs / CRLs / VRI entries), the classified PAdES level achieved, the document's declared PDF/A and PDF/X conformance claims, the empty-field census (the count of reserved fields awaiting their counterparty signer, folded through `enumerate_sig_fields(filled_status=False)`), and the two upstream conformance booleans the spec carries — the `document/tagged#ACCESS` PDF/UA `structural_conformant` pass AND the SECOND consumed `archival_conformant`, the `document/tagged#ARCHIVE` VALIDATED (not self-declared) PDF/A close threaded exactly as the structural one.

The signer credential is the closed `SignerSource` union — `PemKey` / `Pkcs12Bundle` / `ExternalSig` cases, each a frozen signer-struct read by the `SignerSource.cms()` projector method (woven with the shared `@beartype(conf=FAULT_CONF)` contract, mirroring the `exchange/credential#CREDENTIAL` sibling's format-named `SignerSpec.cose()` method — `cms` for the CMS/CAdES container PAdES rides, `cose` for c2pa) that binds `SimpleSigner.load`, `SimpleSigner.load_pkcs12`, or the `ExternalSigner` injected-signature HSM/remote seam — never the mutually-exclusive `key_file`/`pfx_file`/`external_signature` nullable-field bag a flat `params` struct smuggles, and never a free `_signer` function reconstructing the dispatch outside the owner. `Conformance.close` is `async` over the runtime `async_boundary`, offloading the synchronous `pyhanko`/`pikepdf` calls through `anyio.to_thread.run_sync` under the module-level `_THREAD_GATE` `CapacityLimiter` (the explicit thread bound, never the per-loop 40-token default) so the RFC-3161 timestamp and OCSP/CRL network I/O never block the event loop, with a `stamina.retry(on=_TRANSIENT, attempts=4, timeout=30.0)` definition-time weave re-attempting the transient TSA seam exactly as the `exchange/credential#CREDENTIAL` sibling re-attempts its own crypto-with-network seam, and returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]`.

`ConformanceVerdict` is the one acyclic value-object edge the `core/receipt#RECEIPT` `Verdict` case imports: `receipt.py` imports `ConformanceVerdict` and spreads `verdict.facts()`, so this owner never reciprocally imports `ArtifactReceipt` — the consumer mints `ArtifactReceipt.Verdict(key, verdict)` from the returned pair. This PAdES PDF-cryptographic close is orthogonal to the `exchange/credential#CREDENTIAL` C2PA cross-format content-authenticity bind — a PDF routes here, a raster/BMFF/audio asset routes to the c2pa rail.

## [01]-[INDEX]

- [01]-[CONFORMANCE]: `pyhanko` PAdES signing (with `CAdESSignedAttrSpec` commitment-type attributes), LTV archival maintenance, seed-value field reservation, and resilient multi-signature conformance-audit owner that IS the closed `Conformance` `tagged_union` (`sign`/`stamp`/`augment`/`reserve`/`audit`, the signer-free `stamp` arm a `PdfTimeStamper.timestamp_pdf` proof-of-existence `/DocTimeStamp`, the signer-free `reserve` arm an `append_signature_field` seed-value-constrained empty field, the `sign` arm's `FieldPlacement` create-or-FILL modality — `new` placing a fresh field, `reserved` filling the reserve-created field through `PdfSigner.sign_pdf(existing_fields_only=True)` to close the multi-party lifecycle), the dispatch, async entry, the `stamina.retry(on=_TRANSIENT)` transient-TSA weave, and case-body methods folded onto the union with no wrapper struct and no free helper; `SignerSource` the per-seam signer-credential union read by its `@beartype`-woven `cms()` projector method, `Appearance` the closed invisible/visible field-appearance family whose `visible` case carries a `VisibleSeal` `TextStampStyle` drawing-sheet professional seal and whose `plan()` folds each case to the `PdfSigner` `stamp_style` plus the matching `SigFieldSpec` appearance-settings kwarg in one `match`, `SignerConstraint` the reserve-arm future-signer identity constraint (a subject `DnField` DN plus required/forbidden `KeyUsage` bits projected to `SigCertConstraints`/`SigCertKeyUsage` and its `_CERT_FLAG`-folded mandatory `SigCertConstraintFlags`), `DssPolicy` the sign-time DSS/VRI write policy over the `DssPlacement` vocabulary projecting `DSSContentSettings`, `PadesLevel` the LTV-behavior policy row whose derived `needs_timestamp`/`embeds_validation`/`archival` properties carry the sign-time chain behavior and whose `classify` staticmethod folds the achieved-level derivation onto the vocabulary, `Digest` the digest-algorithm vocabulary, `SigKind` the `/Sig`/`/DocTimeStamp` signature-object discriminant, `KeyUsage`/`ExtKeyUsage` the audit key-usage and extended-key-usage constraint vocabularies projecting into `KeyUsageConstraints`, `Commitment` the CAdES commitment-type vocabulary projecting `GenericCommitment` through the value-derived `_COMMITMENT`, `CertifyPerm`/`DiffMode` the vocabularies projecting `MDPPerm`/`DiffPolicy` through the name-derived `_MDP` / instance-bound `_DIFF` `frozendict` tables, `_Tally` the one-pass aggregate-fold seed and `_Primary` the one primary-identity projection the audit folds the live statuses through, and `ConformanceVerdict` the rich audit value object every arm folds — carrying signer identity, signing/timestamp time, crypto-mechanism, multi-signature counts, the `fields_awaiting` empty-field census, EU-qualified status, DSS LTV-material counts, and the two `structural_conformant`/`archival_conformant` upstream verdicts the spec threads (the `document/tagged#ACCESS` PDF/UA pass and the `document/tagged#ARCHIVE` VALIDATED PDF/A close) — that the `core/receipt#RECEIPT` `Verdict` case carries as the one acyclic leaf edge, the consumer minting `ArtifactReceipt.Verdict(key, verdict)` so `conformance` imports no `ArtifactReceipt`.

## [02]-[CONFORMANCE]

- Owner: `Conformance` the one PDF-cryptographic-close owner AND the closed `tagged_union` discriminating operation over its own typed `(bytes, spec)` payload — `sign`, `stamp`, `augment`, `reserve`, `audit` — matched by one total `match`/`case`, the `Sign`/`Stamp`/`Augment`/`Reserve`/`Audit` factories, the async `close`/`_emit`/`_run`/`_produced` entry (the `_emit` thread-offload carrying the `stamina.retry(on=_TRANSIENT, attempts=4, timeout=30.0)` weave, `_produced` the one total `match` binding each case's `(bytes, AuditSpec)`, `_run` the single shared `_audited` self-audit epilogue every case converges on), AND the `_sink` byte-emit fold (the one `BytesIO`-`output=`/`writer.write`-`getvalue` sink ceremony every producing arm routes through) plus the `_signed`/`_timestamped`/`_augmented`/`_refreshed`/`_reserved`/`_resilient`/`_validated`/`_stamped`/`_audited` case-body methods all folded onto the union, never a one-field wrapper struct over a separate op union, never a `step: str` discriminant beside a shared optional-field `params` bag, and never a free module function reconstructing a case body outside the owner. `SignerSource` is the closed credential union carrying one frozen signer-struct per seam (`PemKey` PEM key+cert, `Pkcs12Bundle` PKCS#12, `ExternalSig` injected-signature HSM/remote) whose `cms()` method — woven with the shared `@beartype(conf=FAULT_CONF)` contract so a malformed credential lifts onto the runtime fault rail rather than raising past the thread-offload seam — reads the tagged case directly, never parallel `key_file`/`cert_file`/`pfx_file`/`external_signature` nullable fields the body re-derives. `Appearance` is the closed invisible/visible field-appearance family — the `invisible` case an `InvisSigSettings` print/hidden/bounds flag set, the `visible` case a `VisibleSeal` whose `qr_position`-selected `TextStampStyle` (positioned drawing-sheet professional seal) or `QRStampStyle` (scan-to-verify seal whose QR encodes the `%(url)s` verification link) renders the ISO-19650 seal (`stamp_text` interpolating the `appearance_text_params` `%(...)s` fields) — whose `plan()` folds each case to the `PdfSigner` `stamp_style`, the matching `SigFieldSpec` `invis_sig_settings`/`visible_sig_settings` kwarg, and the sign-time text-param dict in ONE `match`, so `_signed` drives the `PdfSigner` engine once across both modalities rather than branching the bare `sign_pdf`. `FieldPlacement` is the closed field-provisioning family the `sign` case discriminates on — `new` a `NewField` (box/page/`lock_fields`) placed at signing (`existing_fields_only=False`), `reserved` the FILL of a `reserve`-created empty seed-value field (`new_field_spec=None`, `existing_fields_only=True`) — so `_signed` targets a fresh OR a reserved field on ONE `PdfSigner` drive and the multi-party reserve->fill lifecycle closes rather than the create-only sign stranding the prepared field; the field-creation fields ride the `new` case alone, never a SignSpec bag whose box/page/lock are dead weight on the fill path. The spec threads two `document/tagged`-sourced conformance booleans — `structural_conformant` (the `document/tagged#ACCESS` PDF/UA pass, the seam `document/tagged` already writes) and the SECOND consumed `archival_conformant` (the `document/tagged#ARCHIVE` VALIDATED `pdf_oxide.validate_pdf_a` PDF/A close) — projected to the verdict beside the self-declared `pdfa_claim`, the archival boolean threaded EXACTLY as the structural one rather than a self-asserted claim. `SignerConstraint` is the reserve-arm future-signer identity constraint carrying a subject `DnField` DN and required/forbidden `KeyUsage` bit tuples, projecting one `SigCertConstraints(subject_dn=x509.Name.build(...), key_usage=[SigCertKeyUsage(...)])` whose mandatory `SigCertConstraintFlags` derive from the populated axes through `_CERT_FLAG` exactly as `_SEED_FLAG` derives the seed flags — never a bare-string DN bag or a free cert field. `DssPolicy` projects `DSSContentSettings(include_vri=, placement=)` over the name-derived `DssPlacement`/`_DSS_PLACEMENT` vocabulary, and the `SignSpec.signer_key_usage` `KeyUsage` tuple projects the `PdfSignatureMetadata.signer_key_usage` sign-time key-usage assertion (defaulting `non_repudiation`, pyhanko's own default). `PadesLevel` is the policy-as-value level row whose derived properties carry the LTV chain behavior and whose `classify(ltv, archival_valid, timestamp_valid)` staticmethod folds the achieved-level derivation onto the vocabulary; `Digest` the digest-algorithm vocabulary the signing `md_algorithm` is (the sha2/sha3 family passed straight to `PdfSignatureMetadata` as a `StrEnum` value, never a bare `str`); `Commitment` the CAdES commitment-type vocabulary projecting `GenericCommitment` (`proof-of-origin`/`-receipt`/`-delivery`/`-sender`/`-approval`/`-creation`) through the value-derived `_COMMITMENT`; `CertifyPerm` the certification vocabulary projecting `MDPPerm` through the name-derived `_MDP`; `DiffMode` the modification-policy vocabulary projecting the `pyhanko` `DiffPolicy` through `_DIFF`; `SigKind` the closed `/Sig`/`/DocTimeStamp` signature-object-type discriminant replacing the repeated bare-string `sig_object_type` comparisons; `KeyUsage`/`ExtKeyUsage` the closed key-usage and extended-key-usage vocabularies `AuditSpec` carries into a `KeyUsageConstraints(key_usage=, extd_key_usage=)` rather than a bare-`str` usage bag; `_Tally` the `gc=False` aggregate-fold accumulator and `_Primary` the `gc=False` primary-identity projection the audit derives the verdict scalars from in one pass each; `ConformanceVerdict` the carried audit value object every op folds, a `gc=False` scalar leaf whose `facts()` derives through `msgspec.structs.asdict`. `pyhanko` owns the CMS/CAdES/PAdES signing engine, the seed-value field reservation, the DSS/LTV embedding, the RFC-3161 timestamp chaining, the pluggable diff policy, and the embedded-signature plus document-timestamp validation; `pikepdf` owns the page-count and PDF/A·PDF/X conformance-claim read. `ConformanceVerdict` is declared on this owner so the value object is leaf-imported by `core/receipt#RECEIPT` without a reciprocal `ArtifactReceipt` import.
- Cases: `Conformance` cases `sign(pdf, SignSpec)` (the `SignSpec.signer` `SignerSource` credential, the `PadesLevel` whose properties derive `use_pades_lta`/`embed_validation_info`, the `FieldPlacement` create-or-FILL modality (`new` a fresh `SigFieldSpec` carrying `box`/`page`/optional `FieldMDPSpec` form-field lock, `reserved` FILLING the empty seed-value field a prior `reserve` created via `PdfSigner.sign_pdf(existing_fields_only=True)` with no `new_field_spec`), the `Appearance` invisible/visible drawing-sheet-seal axis, the optional `Commitment` projected to a `CAdESSignedAttrSpec` commitment-type signed attribute, the `reason`/`location`/`name`/`contact_info` text and the `Digest` `md_algorithm`, the `signer_key_usage` `KeyUsage` sign-time assertion and the `DssPolicy` DSS/VRI write policy, the optional `CertifyPerm` certification, the `HTTPTimeStamper` over `tsa_url`, and the `ValidationContext` for B-LT/B-LTA embedding — driving one `PdfSigner(stamp_style=).sign_pdf` over an `IncrementalPdfFileWriter`) · `stamp(pdf, StampSpec)` (the signer-free proof-of-existence pass — one `PdfTimeStamper(HTTPTimeStamper(tsa_url)).timestamp_pdf` applying an RFC-3161 `/DocTimeStamp` over the document with the `Digest` `md_algorithm` and optional `ValidationContext`, requiring no `SignerSource` credential) · `augment(pdf, AugmentSpec)` (LTV maintenance — `add_validation_info` folded over every `/Sig` signature index to embed fresh OCSP/CRL into the DSS, then `PdfTimeStamper.update_archival_timestamp_chain` to refresh the archival timestamp when `tsa_url` is supplied) · `reserve(pdf, ReserveSpec)` (the signer-free field-preparation pass — one `append_signature_field` over an `IncrementalPdfFileWriter` placing an empty `SigFieldSpec` whose `SigSeedValueSpec` seed-value dict constrains the future signer: the `SigSeedValFlags` mandatory-flag set derived from which constraint each spec field populates, the `pades_only` subfilter restriction, the `digest_methods` allowed-digest set, the `timestamp_required`/`tsa_url` TSA requirement, the `add_rev_info` PAdES revocation-info requirement, and the optional `SignerConstraint` cert-identity `/Cert` binding — a subject `DnField` DN plus required/forbidden `KeyUsage` bits fencing the field to a named future signer — requiring no `SignerSource` credential) · `audit(pdf, AuditSpec)` (the resilient multi-signature conformance pass folding every `/Sig` through `validate_pdf_signature` under the `AuditSpec.usage()` `KeyUsageConstraints` and every `/DocTimeStamp` archival stamp through `validate_pdf_timestamp` under the `DiffMode`-selected `DiffPolicy`, aggregating the live statuses in one `_Tally.step` fold and the primary identity in one `_Primary.of` map, censusing the EMPTY `/Sig` fields awaiting a counterparty signer through `enumerate_sig_fields(filled_status=False)` in the SAME read, reading `read_certification_data`, the `DocumentSecurityStore.read_dss` cert/OCSP/CRL/VRI store, the primary signer's `signing_cert`, and the `pikepdf` `pdfa_status`/`pdfx_status` conformance claims, projecting one `ConformanceVerdict`) — selected by one total `match`, never a chain of `is`-probes. The `sign`/`augment`/`reserve` arms self-audit the produced PDF (sign-then-verify), so every op yields a fully-populated verdict in one shape — a `reserve` verdict carries the prepared document's pages and PDF/A·PDF/X claims with zero signatures and `fields_awaiting=1`, the honest audit of a field awaiting its signer that a paired `sign(placement=reserved)` fill then drops back to `fields_awaiting=0`.
- Auto: `_produced` folds the case through one total `match` binding the produced bytes and the case's `AuditSpec`, and `_run` self-audits that produced document through one shared `_audited` epilogue so every arm converges on one verdict shape at a single audit site. Every producing arm emits its bytes through the one `_sink(write)` fold — a fresh `BytesIO`, the `output=`/`writer.write` sink call, then `getvalue()` — so the append-only sink ceremony lives at one site rather than re-spelled per case body. The `sign` case drives `_signed` — folding `spec.appearance.plan()` to the `PdfSigner` `stamp_style`, the `SigFieldSpec` `invis_sig_settings`/`visible_sig_settings` kwarg (spread from `plan.field_settings`), and the `appearance_text_params` dict; building the `SigFieldSpec` (with `FieldMDPSpec(FieldMDPAction.INCLUDE, lock_fields)` when fields are locked), the `PdfSignatureMetadata` whose `use_pades_lta`/`embed_validation_info` derive off the `PadesLevel` properties, whose `docmdp_permissions` resolves the typed `MDPPerm` through `_MDP`, whose `cades_signed_attr_spec` resolves a `CAdESSignedAttrSpec(commitment_type=_COMMITMENT[spec.commitment].asn1)` when a commitment is supplied, whose `signer_key_usage` is the `KeyUsage`-value set and whose `dss_settings` is the `DssPolicy.settings()` `DSSContentSettings`, the `HTTPTimeStamper` when the level needs one, and the `spec.signer.cms()` credential whose contract violation lifts onto the runtime fault rail — then matching `spec.placement` to drive one `PdfSigner(meta, signer, timestamper=, stamp_style=plan.stamp, new_field_spec=).sign_pdf(writer, existing_fields_only=, appearance_text_params=, output=)` into a fresh `BytesIO` — the `new` case building the `new_field_spec` from `NewField` and passing `existing_fields_only=False`, the `reserved` case passing `new_field_spec=None` and `existing_fields_only=True` to FILL the reserve-created field the seal still riding `stamp_style` — the deepest signing surface replacing the bare `sign_pdf` that neither carries a visible appearance nor fills an existing field. The `stamp` case drives `_timestamped` — one `PdfTimeStamper(HTTPTimeStamper(spec.tsa_url)).timestamp_pdf(writer, spec.md_algorithm, spec.validation_context, output=)` applying a signer-free `/DocTimeStamp` with no credential. The `augment` case drives `_augmented` — a `Block.fold` over the `SigKind.SIGNATURE` indices threading `add_validation_info(..., in_place=False, output=)` to embed fresh validation material, then the conditional `_refreshed` archival-chain extension. The `reserve` case drives `_reserved` — deriving the `SigSeedValFlags` mandatory set through `reduce(or_, ...)` over the `_SEED_FLAG` table (one row per flag whose predicate reads the populated `ReserveSpec` field), building the `SigSeedValueSpec(flags=, subfilters=, digest_methods=, timestamp_required=, timestamp_server_url=, add_rev_info=, cert=)` whose `cert` is the optional `spec.signer_constraint.constraints()` `SigCertConstraints` — its `SigCertConstraintFlags` mandatory set folded by `_CERT_FLAG` and its `subject_dn`/`key_usage` projected from the `DnField` DN and the `KeyUsage` bit tuples — and `append_signature_field(writer, SigFieldSpec(..., seed_value_dict=))` into a fresh `BytesIO` via `writer.write(sink)`. The `audit` case drives `_audited` — reading `reader.embedded_signatures` ONCE and splitting it through one `Block.partition` keyed on `SigKind.SIGNATURE` into the `/Sig` `EmbeddedPdfSignature` and `/DocTimeStamp` archival-timestamp halves in a single pass, resiliently validating each signature through the `_validated` projector over `validate_pdf_signature(sig, signer_validation_context=, ts_validation_context=, diff_policy=, key_usage_settings=)` and each archival stamp through the symmetric `_stamped` projector over `validate_pdf_timestamp`, both delegating to the one `_resilient[T]` boundary-capture aspect that composes `expression.extra.result.catch(exception=SignatureValidationError)` projecting a broken signature or stamp onto `Nothing` (counted, never fatal), collected through `Block.choose` into the live-status sets, folding the live statuses in ONE `live.fold(_Tally.step, _Tally())` pass that accumulates the `valid`/`trusted` counts, the `all_valid`/`all_trusted`/`docmdp_ok`/`seed_value_ok`/`timestamps_trusted` AND-aggregates, the `revoked` OR-aggregate, the weakest-link `min` `coverage` (a `None` coverage admitted as `UNCLEAR`), and the worst-case `max` `modification`, projecting the primary live status through ONE `_Primary.of` map (its `signing_cert.subject`/`issuer`/`serial_number`, `signer_reported_dt`, `timestamp_validity.timestamp`, `content_timestamp_validity.trusted`, `md_algorithm`, `pkcs7_signature_mechanism`, and `qualification_result.status.qualified`) defaulted to the empty `_Primary()` on absence, reading `read_certification_data(reader).permission`, gating `DocumentSecurityStore.read_dss(reader)` behind a `"/DSS" in reader.root` membership test and reading its `certs`/`ocsps`/`crls`/`vri_entries` cardinality, censusing the EMPTY `/Sig` fields awaiting a counterparty signer through `enumerate_sig_fields(reader, filled_status=False)` for the `fields_awaiting` count in the same read, reading the `pikepdf` page count and `pdfa_status`/`pdfx_status` conformance claims in one open, classifying the achieved `PadesLevel` from the LTV and archival-timestamp-trust evidence, and projecting one `ConformanceVerdict` carrying the spec's two `structural_conformant` (PDF/UA) and `archival_conformant` (validated PDF/A) upstream verdicts.
- Receipt: every op produces one `ConformanceVerdict`, the audit-evidence value object the `core/receipt#RECEIPT` `ArtifactReceipt.Verdict` case carries as `tuple[ContentKey, ConformanceVerdict]`. `receipt.py` imports `ConformanceVerdict` and its `_facts` arm spreads `verdict.facts()`, and this owner never reciprocally imports `ArtifactReceipt`, so the value-object edge is the single acyclic leaf edge the receipt union admits — the `core/plan#PLAN` planner (or any coordinator holding both owners) mints `ArtifactReceipt.Verdict(key, verdict)` from the returned `(key, verdict)` pair. `ConformanceVerdict.facts()` derives the fact map through `msgspec.structs.asdict(self)` — one edit site that cannot drift from the field set — projecting NATIVE scalars (`bool`/`int`/`str`) onto the `dict[str, object]` `EventDict`, never `str(...)`-pre-stringified text, so the `observability/metrics` `MeterProvider` valid/trusted/broken/qualified signal stream and the structured-log consumer read the verdict facts as numbers and booleans. Conformance enters on THREE axes the verdict surfaces, never a new receipt rail: the `document/tagged#ACCESS` PDF/UA `structural_conformant` boolean (the seam `document/tagged` already writes) AND the SECOND consumed `document/tagged#ARCHIVE` VALIDATED PDF/A `archival_conformant` boolean, both carried on the spec as interior evidence threaded identically; and the document's OWN declared `pdfa_claim`/`pdfx_claim` read here from the `pikepdf` XMP — what the signed document ASSERTS about its PDF/A and PDF/X conformance. The self-declared XMP claim and the validated `archival_conformant` are distinct evidence: `pdfa_claim` is the `pdfaid` the file writes about itself, `archival_conformant` is the in-process `pdf_oxide.validate_pdf_a(level)` Rust-oracle verdict the upstream `document/tagged#ARCHIVE` owner PROVES, so the archival-delivery plane reads a validated close, never a self-assertion.
- Packages: `pyhanko` (`SimpleSigner.load`/`load_pkcs12`, `ExternalSigner(signing_cert=, cert_registry=, signature_value=)`, `load_certs_from_pemder`, `Signer`, `PdfSigner(signature_meta, signer, timestamper=, stamp_style=, new_field_spec=)` (`.sign_pdf(pdf_out, existing_fields_only=, appearance_text_params=, output=)`, the deepest signing surface driving BOTH the new-field placement and the `existing_fields_only=True` FILL of a reserve-created field, replacing the bare `sign_pdf`), `IncrementalPdfFileWriter` (`.write(sink)`), `PdfFileReader.embedded_signatures`, `HTTPTimeStamper`, `TimestampRequestError` (`pyhanko.sign.timestamps`), `PdfTimeStamper(timestamper).update_archival_timestamp_chain(reader, validation_context, in_place=False, output=)`, `PdfTimeStamper(timestamper).timestamp_pdf(pdf_out, md_algorithm, validation_context, output=)`, `PdfSignatureMetadata` with `field_name`/`md_algorithm`/`reason`/`location`/`contact_info`/`name`/`subfilter`/`certify`/`docmdp_permissions`/`cades_signed_attr_spec`/`signer_key_usage`/`dss_settings`/`use_pades_lta`/`embed_validation_info`/`validation_context`, `append_signature_field(pdf_out, sig_field_spec)`, `enumerate_sig_fields(handler, filled_status=None, with_name=None)` (yielding `(name, value, ref)`, `filled_status=False` the EMPTY-field census of reserved fields awaiting a signer), `SigFieldSpec(sig_field_name=, on_page=, box=, field_mdp_spec=, seed_value_dict=, invis_sig_settings=, visible_sig_settings=)`, `VisibleSigSettings`/`InvisSigSettings`, `SigSeedValueSpec(flags=, subfilters=, digest_methods=, timestamp_required=, timestamp_server_url=, add_rev_info=, cert=)`, `SigCertConstraints(flags=, subject_dn=, issuers=, key_usage=)`, `SigCertKeyUsage(must_have=, forbidden=)`, `SigCertConstraintFlags` (`SUBJECT_DN`/`KEY_USAGE`), `SigSeedValFlags` (`SUBFILTER`/`DIGEST_METHOD`/`ADD_REV_INFO`), `SigSeedSubFilter.PADES`, `FieldMDPSpec(action, fields)`/`FieldMDPAction`, `MDPPerm`, `CAdESSignedAttrSpec(commitment_type=)` and `GenericCommitment.*.asn1` (`pyhanko.sign.ades.api`), `DSSContentSettings(include_vri=, placement=)`/`SigDSSPlacementPreference` (`pyhanko.sign.signers.pdf_signer`), `TextStampStyle(stamp_text=, border_width=, background_opacity=)`/`BaseStampStyle` (`pyhanko.stamp`), `add_validation_info(emb, vc, in_place=False, output=)`, `validate_pdf_signature(emb, signer_validation_context=, ts_validation_context=, diff_policy=, key_usage_settings=) -> PdfSignatureStatus`, `validate_pdf_timestamp(emb, validation_context=, diff_policy=) -> DocumentTimestampStatus` (`.trusted`), `read_certification_data(reader) -> DocMDPInfo` (`.permission`), `DocumentSecurityStore.read_dss(reader)` (`.certs`/`.ocsps`/`.crls`/`.vri_entries`), `EmbeddedPdfSignature.sig_object_type`, `KeyUsageConstraints(key_usage=, extd_key_usage=)` (`pyhanko.sign.validation.settings`), `SignatureValidationError` (`pyhanko.sign.validation.errors`), `PdfSignatureStatus.bottom_line`/`trusted`/`revoked`/`coverage`/`modification_level`/`docmdp_ok`/`seed_value_ok`/`signer_reported_dt`/`md_algorithm`/`pkcs7_signature_mechanism`/`timestamp_validity`/`content_timestamp_validity`/`signing_cert`/`qualification_result`, `TimestampSignatureStatus.timestamp`/`.trusted`, `QualificationResult.status.qualified`, `SignatureCoverageLevel`, `ModificationLevel`, `DEFAULT_DIFF_POLICY`/`NO_CHANGES_DIFF_POLICY`/`DiffPolicy`); `pyhanko_certvalidator` (`ValidationContext` arriving at the boundary, `SimpleCertificateStore.from_certs` for the external-signer registry); `asn1crypto` (the `signing_cert` `x509.Certificate.subject.human_friendly`/`issuer.human_friendly`/`serial_number` identity accessors reached through the catalogued `signing_cert` type, and `x509.Name.build(name_dict)`/`x509.KeyUsage(bit_set)` building the reserve-arm subject-DN and key-usage cert constraints); `pikepdf` (`open` + `len(pdf.pages)` + `open_metadata` + `PdfMetadata.pdfa_status`/`pdfx_status`); `msgspec` (`Struct(frozen=True)` for the specs, `Struct(frozen=True, gc=False)` for the `_Tally`/`_Primary` audit accumulators and the `ConformanceVerdict` scalar leaf, `structs.asdict` deriving `facts()`); `beartype` (`beartype(conf=FAULT_CONF)` the contract weave on `SignerSource.cms`); `expression` (`tagged_union`/`tag`/`case`, `Block.of_seq`/`fold`/`choose`/`partition`/`try_head`/`is_empty`, `Option`/`map`/`default_value`, `extra.result.catch` the single-exception substrate trap); `stamina` (`retry(on=_TRANSIENT, attempts=4, timeout=30.0)` the transient-TSA network weave on `_emit`); `anyio` (`to_thread.run_sync(limiter=_THREAD_GATE)` the bounded thread offload, `CapacityLimiter` the explicit thread band); stdlib (`functools.reduce`/`operator.or_` folding the `SigSeedValFlags` and `SigCertConstraintFlags` mandatory sets); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`/`FAULT_CONF`).
- Growth: a new operation is one `Conformance` case plus one `_run` arm plus one case-body method; a new signer seam is one `SignerSource` case plus one `cms()` arm, never a parallel signer owner; a new PAdES level is one `PadesLevel` row whose derived properties carry its LTV behavior; a new digest algorithm is one `Digest` row; a new commitment type is one `Commitment` row whose `_COMMITMENT` entry derives from the value-to-`GenericCommitment` name correspondence; a new certification permission is one `CertifyPerm` row whose `_MDP` entry derives from the `StrEnum`-name correspondence; a new modification policy is one `DiffMode` row plus one `_DIFF` entry; a new key-usage or extended-key-usage constraint is one `KeyUsage`/`ExtKeyUsage` row; a new signature-object kind is one `SigKind` row; a new seed-value constraint is one `ReserveSpec` field plus one `_SEED_FLAG` row plus one `SigSeedValueSpec` argument; a new audit aggregate is one `_Tally` field plus one `step` term plus one `ConformanceVerdict` field; a new primary-identity fact is one `_Primary` field plus one `of` term plus one `ConformanceVerdict` field, all picked up through `structs.asdict` with no second edit; a new field-provisioning modality is one `FieldPlacement` case plus one `_signed` `match` arm; a new upstream conformance verdict (a PDF/X preflight close, a PDF/UA-2 pass) is one consumed boolean the spec threads plus one `ConformanceVerdict` field, projected exactly as `structural_conformant`/`archival_conformant`; a new transient network fault widens the `_TRANSIENT` tuple; a new subject-DN component is one `DnField` row; an issuer-certificate restriction on the reserved field is one `SignerConstraint.issuers` field projected to `SigCertConstraints(issuers=)`; a signature-policy identifier or signer-attribute assertion is one `signature_policy_identifier`/`signer_attributes` projection on the `CAdESSignedAttrSpec`; a new DSS-placement mode is one `DssPlacement` row plus one `_DSS_PLACEMENT` entry; the scan-to-verify QR seal on a printed sheet is the `QRStampStyle` the `VisibleSeal.qr_position` value already drives (`appearance_text_params` carrying the `%(url)s` verification link), so a further visible-seal renderer is one `BaseStampStyle` branch in `VisibleSeal.style()`; a new appearance modality is one `Appearance` case plus one `plan()` arm; zero new surface.
- Boundary: no PDF authoring (that stays at `document/emit#DOCUMENT`), no font engineering (`typography/font#FONT`), no glyph rendering (`typography/shape#SHAPE`); the owner closes an already-emitted PDF and prepares its signature fields, never producing document content. Signature-field reservation is in-lane because the `SigSeedValueSpec`/`SigCertConstraints` seed-value policy is a cryptographic field contract `document/emit` cannot express, never generic page authoring. `pyhanko` does NOT enforce PDF/A or PDF/UA structural conformance — it treats them as ordinary PDF — so the structural verdict is authored upstream at `document/tagged#ACCESS` (the `pikepdf` `StructTreeRoot`/`StructElem` marked-content tree over the `document/model#NODE` family) and the `audit` arm CONSUMES that boolean through the spec, folding it into `ConformanceVerdict.structural_conformant` rather than claiming a veraPDF-grade verdict no pure-Python validator resolves; the `pdfa_claim`/`pdfx_claim` the arm reads from the `pikepdf` XMP are the document's OWN declared `pdfaid`/`pdfxid` claims (an evidence read of what the signed file asserts), never a validated PDF/A verdict; the embed-completeness precondition the PDF/A close requires arrives from `typography/font#FONT` `EMBED_AUDIT`; the `ValidationContext` arrives from `pyhanko_certvalidator` at the boundary, never built here. The deleted forms are the `MappingProxyType` step table where `frozendict` is the owner, the hand-enumerated `_MDP`/`_COMMITMENT` correspondence tables where the `StrEnum`-name and value-to-`GenericCommitment` correspondences derive each row, the one-field `Conformance` wrapper over a separate `ConformOp` union where the `tagged_union` owns dispatch and async entry directly, the free `_signer`/`_classified` module functions where `SignerSource.cms()` and `PadesLevel.classify()` fold the dispatch onto the owner and the vocabulary, the optional-field signer bag where `SignerSource` is a closed union, the bare-`str` `md_algorithm` where the `Digest` vocabulary types it, the repeated bare-string `sig_object_type == "/Sig"`/`"/DocTimeStamp"` comparisons where the `SigKind` vocabulary and one `Block.partition` own the discriminant, the bare-`str` key-usage bag where the `KeyUsage`/`ExtKeyUsage` vocabularies type `required_key_usage`/`required_extd_key_usage`, the hand-rolled per-projector `try`/`except SignatureValidationError` where the one `_resilient[T]` aspect composes the `expression.extra.result.catch` substrate trap, the two-pass tuple-comprehension partition where one `Block.partition` splits the embedded signatures, the nine-pass `sum`/`all`/`any`/`min`/`max` scatter over `live` where one `live.fold(_Tally.step, ...)` accumulates every aggregate in a single traversal, the unguarded `min((s.coverage for s in live), ...)` that faults when `s.coverage` is `None` where the `_Tally.step` admits `None` as `UNCLEAR`, the ten repeated `primary.map(lambda s: ...).default_value(...)` chains where one `primary.map(_Primary.of).default_value(_Primary())` projects every primary scalar at once, the un-retried TSA network seam where the `stamina.retry(on=_TRANSIENT)` weave re-attempts the transient, the empty-string-as-absence signing metadata where `str | None` states omission, the lone `embedded_signatures[0]` audit where the verdict folds every `/Sig`, the audit that ABORTS on one broken signature where `_validated` projects the `SignatureValidationError` onto `Nothing` and the `signatures_broken` count, the identity-blind verdict where `signing_cert.subject`/`issuer`/`serial_number` and `signer_reported_dt`/`timestamp_validity.timestamp` carry WHO signed and WHEN, the semantically-empty `archival_metadata=bool(xmp)` where the DSS `certs`/`ocsps`/`crls`/`vri_entries` counts carry the real LTV-material evidence, the presence-only B-LTA classification where `validate_pdf_timestamp` proves the archival timestamp's trust, the unprepared signing field where the `reserve` arm projects a `SigSeedValueSpec` seed-value policy through `append_signature_field`, the invisible-only sign that cannot seal a drawing sheet where the `Appearance` closed family drives `PdfSigner(stamp_style=)` for the positioned ISO-19650 professional seal, the bare `sign_pdf` convenience where the `PdfSigner` engine carries the appearance on one drive path, the free-string DN bag where `DnField`/`x509.Name.build` types the reserve-arm subject constraint, the reserved field open to any signer where `SignerConstraint` binds its future signer's subject DN and required/forbidden key-usage through `SigCertConstraints`, the implicit `signer_key_usage`/`dss_settings` defaults the body re-derives where `SignSpec` states them as the `KeyUsage`-tuple assertion and the `DssPolicy` write-policy value, the erased `object` certification-permission and `object | None` validation-context where `MDPPerm` and `ValidationContext` type them, the bare `@beartype` raise where `beartype(conf=FAULT_CONF)` lifts a contract violation onto the fault rail, the hand-maintained `facts()` mirror where `structs.asdict` derives it, the synchronous network-blocking boundary where the `_THREAD_GATE`-bounded `anyio.to_thread.run_sync` offloads the TSA/OCSP I/O off the event loop, the unbounded `to_thread.run_sync` trusting the per-loop 40-token default where the explicit `_THREAD_GATE` `CapacityLimiter` bounds the thread band, the per-arm `self._audited(...)` epilogue repeated across the five cases where `_produced` yields one `(bytes, AuditSpec)` pair and `_run` self-audits every produced document at a single shared `_audited` site, the create-ONLY sign that mints a fresh field on every call and strands the `reserve` arm's empty field where the `FieldPlacement(reserved)` modality FILLS it through `PdfSigner.sign_pdf(existing_fields_only=True)` to close the multi-party reserve->fill lifecycle, the filled-only audit blind to a reserved field awaiting its signer where `enumerate_sig_fields(filled_status=False)` censuses the empty fields into `fields_awaiting`, the self-declared-ONLY archival evidence where the SECOND consumed `archival_conformant` boolean carries the upstream `document/tagged#ARCHIVE` `pdf_oxide.validate_pdf_a` in-process Rust-oracle VALIDATED PDF/A close beside the self-asserted `pdfa_claim` XMP read (threaded exactly as the existing `structural_conformant` seam, never a veraPDF subprocess this owner would duplicate), and the discarded verdict where `close` returns `(ContentKey, ConformanceVerdict)`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from io import BytesIO
from operator import or_
from typing import Final, Literal, Self, assert_never

import pikepdf
import stamina
from anyio import CapacityLimiter, to_thread
from asn1crypto import x509
from beartype import beartype
from builtins import frozendict
from expression import Option, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import catch
from msgspec import Struct, structs
from pyhanko.pdf_utils.incremental_writer import IncrementalPdfFileWriter
from pyhanko.pdf_utils.reader import PdfFileReader
from pyhanko.sign import (
    ExternalSigner,
    PdfSignatureMetadata,
    PdfSigner,
    PdfTimeStamper,
    Signer,
    SimpleSigner,
    load_certs_from_pemder,
)
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
# each policy row derives from the vocabulary member, never a hand-enumerated parallel map: `_MDP`
# by `StrEnum`-name correspondence, `_COMMITMENT` by value-to-`GenericCommitment`-name; `_DIFF`
# binds the two module-level `diff_analysis` policy instances no name correspondence reaches.
_MDP: Final[frozendict[CertifyPerm, MDPPerm]] = frozendict({perm: MDPPerm[perm.name] for perm in CertifyPerm})
_COMMITMENT: Final[frozendict[Commitment, GenericCommitment]] = frozendict({c: GenericCommitment[c.value.upper()] for c in Commitment})
_DIFF: Final[frozendict[DiffMode, DiffPolicy]] = frozendict({DiffMode.DEFAULT: DEFAULT_DIFF_POLICY, DiffMode.STRICT: NO_CHANGES_DIFF_POLICY})
_DSS_PLACEMENT: Final[frozendict[DssPlacement, SigDSSPlacementPreference]] = frozendict({p: SigDSSPlacementPreference[p.name] for p in DssPlacement})
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
                return ExternalSigner(signing_cert=chain[0], cert_registry=SimpleCertificateStore.from_certs(chain), signature_value=ext.signature_value)
            case _ as unreachable:
                assert_never(unreachable)


class AuditSpec(Struct, frozen=True):
    signer_context: ValidationContext | None = None
    ts_context: ValidationContext | None = None
    diff_mode: DiffMode = DiffMode.DEFAULT
    required_key_usage: tuple[KeyUsage, ...] = ()
    required_extd_key_usage: tuple[ExtKeyUsage, ...] = ()
    # the two upstream conformance verdicts consumed via the spec, both authored upstream because pyhanko
    # treats PDF/A and PDF/UA as ordinary PDF: `structural_conformant` the `document/tagged#ACCESS` PDF/UA
    # StructTreeRoot pass (the existing seam `document/tagged` writes), `archival_conformant` the SECOND
    # consumed boolean threaded EXACTLY as it — the `document/tagged#ARCHIVE` in-process
    # `pdf_oxide.validate_pdf_a(level)` VALIDATED (not self-declared) PDF/A close beside the self-asserted `pdfa_claim`.
    structural_conformant: bool = False
    archival_conformant: bool = False

    def usage(self) -> KeyUsageConstraints | None:
        return (
            KeyUsageConstraints(key_usage=frozenset(self.required_key_usage) or None, extd_key_usage=frozenset(self.required_extd_key_usage) or None)
            if self.required_key_usage or self.required_extd_key_usage
            else None
        )


# the field-appearance axis the invisible byte-range signature AND the visible drawing-sheet
# professional seal both fold onto: the `invisible` case carries the `InvisSigSettings` print/hidden/
# out-of-bounds flags, the `visible` case a `VisibleSeal` whose `qr_position`-selected `TextStampStyle`
# (positioned seal) or `QRStampStyle` (scan-to-verify seal encoding the `%(url)s` link in its QR) renders
# the ISO-19650 seal (`stamp_text` interpolating the `appearance_text_params` `%(...)s` fields) and whose
# `VisibleSigSettings` sets the annotation rotate/scale/print behavior. One `plan()` folds each case to
# the `PdfSigner` stamp style, the matching `SigFieldSpec` appearance-settings kwarg, and the sign-time
# text params in ONE match, so `_signed` branches on neither the modality nor the settings field.
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
            QRStampStyle(stamp_text=self.stamp_text, border_width=self.border_width, background_opacity=self.background_opacity, qr_position=self.qr_position)
            if self.qr_position is not None
            else TextStampStyle(stamp_text=self.stamp_text, border_width=self.border_width, background_opacity=self.background_opacity)
        )

    def settings(self) -> VisibleSigSettings:
        return VisibleSigSettings(rotate_with_page=self.rotate_with_page, scale_with_page_zoom=self.scale_with_page_zoom, print_signature=self.print_signature)


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
                return _SealPlan(stamp=seal.style(), field_settings=frozendict({"visible_sig_settings": seal.settings()}), text_params=seal.text_params)
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


# the signature-field provisioning modality the `sign` case discriminates on: `new` places a fresh field at
# signing (its `box`/`page`/FieldMDP-lock its own payload, `existing_fields_only=False`), `reserved` FILLS the
# empty seed-value field a prior `reserve` created (named by `SignSpec.field_name`, `new_field_spec=None`,
# `existing_fields_only=True`) — the counterparty fill stage completing the multi-party reserve->fill lifecycle
# `reserve` opens. The value carries the modality, never a `fill_existing: bool` knob, and the field-creation
# fields ride the `new` case alone rather than a bag whose box/page/lock are dead weight on the fill path.
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
        return AuditSpec(signer_context=self.validation_context, ts_context=self.validation_context, structural_conformant=self.structural_conformant, archival_conformant=self.archival_conformant)


class StampSpec(Struct, frozen=True):
    tsa_url: str
    md_algorithm: Digest = Digest.SHA256
    validation_context: ValidationContext | None = None
    structural_conformant: bool = False
    archival_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(ts_context=self.validation_context, structural_conformant=self.structural_conformant, archival_conformant=self.archival_conformant)


class AugmentSpec(Struct, frozen=True):
    validation_context: ValidationContext
    tsa_url: str | None = None
    structural_conformant: bool = False
    archival_conformant: bool = False

    def audit(self) -> AuditSpec:
        return AuditSpec(signer_context=self.validation_context, ts_context=self.validation_context, structural_conformant=self.structural_conformant, archival_conformant=self.archival_conformant)


# the future-signer identity constraint the `reserve` arm binds into the empty field's seed-value `/Cert`
# dictionary — only a certificate matching the subject DN and carrying (and lacking) the named key-usage
# bits may fill the field, the multi-party professional-seal prepare stage. The key-usage axis reuses the
# shared `KeyUsage` vocabulary projected to `x509.KeyUsage` bit sets, the subject DN a typed
# `frozendict[DnField, str]` fed to `x509.Name.build`, never a bare-string DN bag or a free cert field.
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
_CERT_FLAG: Final[frozendict[SigCertConstraintFlags, Callable[[SignerConstraint], bool]]] = frozendict({
    SigCertConstraintFlags.SUBJECT_DN: lambda c: bool(c.subject_dn),
    SigCertConstraintFlags.KEY_USAGE: lambda c: bool(c.required_key_usage or c.forbidden_key_usage),
})


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
_SEED_FLAG: Final[frozendict[SigSeedValFlags, Callable[[ReserveSpec], bool]]] = frozendict({
    SigSeedValFlags.SUBFILTER: lambda spec: spec.pades_only,
    SigSeedValFlags.DIGEST_METHOD: lambda spec: bool(spec.digest_methods),
    SigSeedValFlags.ADD_REV_INFO: lambda spec: spec.add_rev_info,
})


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

    async def close(self) -> RuntimeRail[tuple[ContentKey, ConformanceVerdict]]:
        return await async_boundary(f"conformance.{self.tag}", self._emit)

    @stamina.retry(on=_TRANSIENT, attempts=4, timeout=30.0)
    async def _emit(self) -> tuple[ContentKey, ConformanceVerdict]:
        payload, verdict = await to_thread.run_sync(self._run, limiter=_THREAD_GATE)
        return ContentIdentity.of(f"conformance.{self.tag}", payload), verdict

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
        # the `PdfSigner` engine drives BOTH field modalities on ONE path: `new` places a fresh field
        # (`new_field_spec=`, `existing_fields_only=False`) — the invisible byte-range signature or the
        # visible drawing-sheet seal via `stamp_style` — while `reserved` FILLS the empty seed-value field a
        # prior `reserve` created (`new_field_spec=None`, `existing_fields_only=True`), the seal still riding
        # `stamp_style`, so the counterparty signature lands in the constrained field rather than a second new one.
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
        return self._sink(lambda sink: engine.sign_pdf(IncrementalPdfFileWriter(BytesIO(pdf)), existing_fields_only=existing_only, appearance_text_params=dict(plan.text_params) or None, output=sink))

    def _timestamped(self, pdf: bytes, spec: StampSpec, /) -> bytes:
        return self._sink(lambda sink: PdfTimeStamper(HTTPTimeStamper(spec.tsa_url)).timestamp_pdf(IncrementalPdfFileWriter(BytesIO(pdf)), spec.md_algorithm, spec.validation_context, output=sink))

    def _augmented(self, pdf: bytes, spec: AugmentSpec, /) -> bytes:
        indices = Block.of_seq(
            i for i, sig in enumerate(PdfFileReader(BytesIO(pdf)).embedded_signatures) if str(sig.sig_object_type) == SigKind.SIGNATURE
        )

        def embedded(current: bytes, index: int, /) -> bytes:
            return self._sink(lambda sink: add_validation_info(PdfFileReader(BytesIO(current)).embedded_signatures[index], spec.validation_context, in_place=False, output=sink))

        enriched = indices.fold(embedded, pdf)
        return self._refreshed(enriched, spec.tsa_url, spec.validation_context) if spec.tsa_url else enriched

    def _refreshed(self, pdf: bytes, tsa_url: str, context: ValidationContext, /) -> bytes:
        return self._sink(lambda sink: PdfTimeStamper(HTTPTimeStamper(tsa_url)).update_archival_timestamp_chain(PdfFileReader(BytesIO(pdf)), context, in_place=False, output=sink))

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
        return self._resilient(lambda: validate_pdf_signature(sig, signer_validation_context=spec.signer_context, ts_validation_context=spec.ts_context, diff_policy=_DIFF[spec.diff_mode], key_usage_settings=spec.usage()))

    def _stamped(self, ts: EmbeddedPdfSignature, spec: AuditSpec, /) -> Option[DocumentTimestampStatus]:
        return self._resilient(lambda: validate_pdf_timestamp(ts, validation_context=spec.ts_context, diff_policy=_DIFF[spec.diff_mode]))

    def _audited(self, pdf: bytes, spec: AuditSpec, /) -> ConformanceVerdict:
        reader = PdfFileReader(BytesIO(pdf))
        # census the EMPTY /Sig fields (a counterparty field a `reserve` prepared, awaiting its signer) in the
        # same read: `filled_status=False` yields the unsigned fields the filled `embedded_signatures` fold
        # cannot see, so the reserve->fill lifecycle is observable — `fields_awaiting` is 1 after a reserve, 0
        # once the counterparty `sign(placement=reserved)` fills it.
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
# every TSA-signing/audit offload threads this one explicit bound, so N concurrent `close` calls share
# a fixed thread band rather than the per-loop 40-token default — the GIL-releasing pyhanko/pikepdf
# native render and the RFC-3161/OCSP network seam run off the event loop, bounded at the boundary.
_THREAD_GATE: Final[CapacityLimiter] = CapacityLimiter(8)
```

## [03]-[RESEARCH]

- [SIGN_AXIS] [RESOLVED]: the `pyhanko 0.35.1` signing surface verifies against the installed distribution and the folder `.api` catalogue — `SimpleSigner.load(key_file, cert_file, ca_chain_files=, key_passphrase=)`, `SimpleSigner.load_pkcs12(pfx_file, ca_chain_files=, passphrase=)`, `ExternalSigner(signing_cert, cert_registry, signature_value=)` (the injected-signature HSM/remote seam), `load_certs_from_pemder(cert_files)`, `SimpleCertificateStore.from_certs(certs)` (the external-signer cert registry), `PdfSigner(signature_meta, signer, timestamper=, stamp_style=, new_field_spec=).sign_pdf(pdf_out, appearance_text_params=, output=)` (the engine surface driving both the invisible byte-range signature and the visible-seal path on ONE drive, verified against the installed `0.35.1` and replacing the bare `sign_pdf` convenience that carries no `stamp_style`), and `IncrementalPdfFileWriter` are settled. The credential mode is the closed `SignerSource` union whose `cms()` projector method — woven with the shared `@beartype(conf=FAULT_CONF)` contract so a malformed `SignerSource` lifts onto the runtime fault rail, exactly as the `exchange/credential#CREDENTIAL` sibling's format-named `SignerSpec.cose()` method does — folds the three load paths onto the owner rather than a free `_signer` function reconstructing the dispatch beside it; a mutually-exclusive nullable-field bag and an `ExternalSigner(signing_cert=None)` are excluded by construction. The `PdfSignatureMetadata` field set verifies against the installed constructor — including `cades_signed_attr_spec: CAdESSignedAttrSpec | None`, so the optional `Commitment` axis projects a `CAdESSignedAttrSpec(commitment_type=_COMMITMENT[spec.commitment].asn1)` CAdES commitment-type signed attribute (`GenericCommitment.PROOF_OF_ORIGIN`/`PROOF_OF_RECEIPT`/`PROOF_OF_DELIVERY`/`PROOF_OF_SENDER`/`PROOF_OF_APPROVAL`/`PROOF_OF_CREATION` from `pyhanko.sign.ades.api`, each carrying `.asn1 -> CommitmentTypeIndication`, all verified present on the installed enum); the descriptive `reason`/`location`/`name`/`contact_info` ride as `str | None` (omission stated by `None`) and `md_algorithm` is the closed `Digest` `StrEnum` passed straight through, never a bare `str`. `SigFieldSpec(sig_field_name, on_page, box, field_mdp_spec=, seed_value_dict=, doc_mdp_update_value=)`, `FieldMDPSpec(action, fields)`, and `FieldMDPAction` (`ALL`/`INCLUDE`/`EXCLUDE`) verify, so `lock_fields` projects a `FieldMDPSpec` form-field lock. The PAdES level rises B-B -> B-LTA as `subfilter=PADES` plus a `timestamper` engages B-T, `embed_validation_info=True` writes the DSS for B-LT, and `use_pades_lta=True` with a timestamper writes the initial archival timestamp for B-LTA — the initial archival stamp is `use_pades_lta`'s, so the chain refresh is the `Augment` arm's concern alone.
- [STAMP_AXIS] [RESOLVED]: the signer-free document-timestamp entrypoint `PdfTimeStamper(timestamper).timestamp_pdf(pdf_out, md_algorithm, validation_context, output=)` verifies against the installed distribution and rides the same `IncrementalPdfFileWriter` plus `output=` append-only convention as `sign_pdf` and `update_archival_timestamp_chain`. The `stamp` case applies an RFC-3161 `/DocTimeStamp` proof-of-existence timestamp over the document with the `Digest` `md_algorithm` and an optional `ValidationContext`, requiring no `SignerSource` credential — the PAdES capability the owner formerly left to the unused `timestamp_pdf` entrypoint, now the realized `Conformance` case absorbed under the same growth law (`StampSpec` payload, `Stamp` factory, `_run` arm, `_timestamped` body). The produced `/DocTimeStamp` self-audits through the same `validate_pdf_timestamp` archival path, `StampSpec.audit()` projecting `ts_context=self.validation_context` so the timestamp trust is verified, not assumed.
- [RESERVE_AXIS] [RESOLVED]: `append_signature_field(pdf_out: BasePdfFileWriter, sig_field_spec: SigFieldSpec)`, `SigSeedValueSpec(flags, reasons, timestamp_server_url, timestamp_required, cert, subfilters, digest_methods, add_rev_info, seed_signature_type, sv_dict_version, legal_attestations, lock_document, appearance)`, `SigSeedValFlags` (`FILTER`/`SUBFILTER`/`V`/`REASONS`/`LEGAL_ATTESTATION`/`ADD_REV_INFO`/`DIGEST_METHOD`/`LOCK_DOCUMENT`/`APPEARANCE_FILTER`), and `IncrementalPdfFileWriter.write(stream)` verify against the installed distribution, and `SigSeedValueSpec` constructs cleanly with `subfilters: List[SigSeedSubFilter] | None`, `digest_methods: List[str] | None`, `timestamp_required: bool`, `timestamp_server_url: str | None`, `add_rev_info: bool | None`. The `reserve` case prepares an empty signer-less signature field carrying a `SigSeedValueSpec` seed-value policy so a counterparty's later signature is constrained at field-creation time rather than rejected post-sign — the prepare stage of the multi-party signing lifecycle the owner previously omitted. The `SigSeedValFlags` mandatory set derives from which `ReserveSpec` constraint each field populates through the `_SEED_FLAG` table (`SUBFILTER` for `pades_only`, `DIGEST_METHOD` for `digest_methods`, `ADD_REV_INFO` for `add_rev_info`), `reduce(or_, ...)` folding the populated bits — never a parallel flag argument. Cert-identity seed-value constraints are BUILT as the optional `SignerConstraint` axis: `SigCertConstraints(flags=, subject_dn=x509.Name.build({field.value: value for field, value in subject_dn.items()}), key_usage=[SigCertKeyUsage(must_have=x509.KeyUsage({ku.value for ku in required_key_usage}), forbidden=x509.KeyUsage({...}))])` projected into `SigSeedValueSpec(cert=)`, the mandatory `SigCertConstraintFlags` (`SUBJECT_DN`/`KEY_USAGE`) folded by `_CERT_FLAG` exactly as `_SEED_FLAG` folds the seed flags — verified against the installed `0.35.1` (`SigCertConstraints.__init__(flags, subjects, subject_dn, issuers, info_url, url_type, key_usage)`, `SigCertKeyUsage.__init__(must_have, forbidden)`, `x509.KeyUsage(set)` and `x509.Name.build(name_dict)`) constructing cleanly and serializing the `/Cert` key into the seed-value `as_pdf_object()`. The subject DN is a typed `frozendict[DnField, str]` whose members are the `asn1crypto` RDN attribute names and the key-usage bits reuse the shared `KeyUsage` vocabulary, so only a signer whose certificate matches the named subject and key-usage may fill the reserved field — the multi-party professional-seal prepare stage the owner previously only gestured at. An issuer-certificate restriction (`SigCertConstraints(issuers=[Certificate])`, needing loaded cert objects) is the deeper growth axis. The `reserve` verdict self-audits the prepared document (zero signatures, the document's pages and PDF/A·PDF/X claims) so every op yields one verdict shape.
- [FILL_AXIS] [RESOLVED]: `PdfSigner.sign_pdf(pdf_out, existing_fields_only=False, bytes_reserved=None, *, appearance_text_params=None, in_place=False, output=None)` verifies against the installed distribution — the `existing_fields_only` parameter is the FILL-a-reserved-field path the owner formerly left BROKEN: the `sign` arm always minted a NEW field through `new_field_spec=field`, so a `reserve` arm's seed-value-constrained empty field had NO arm to fill it and the multi-party reserve->fill lifecycle dead-ended at the prepare stage. The `sign` case now discriminates a `FieldPlacement` modality — `new` builds the `SigFieldSpec` and passes `existing_fields_only=False` (the fresh-field placement), `reserved` passes `new_field_spec=None` and `existing_fields_only=True` so `PdfSigner` locates the reserved field by `meta.field_name` and fills it, the counterparty seal still riding `stamp_style`. The field-creation fields (`box`/`page`/`lock_fields`) collapse onto the `new` case's `NewField` payload rather than a SignSpec bag whose box/page/lock are dead weight on the fill path. The reserve arm's `SigCertConstraints` seed-value policy then fences the fill to the licensed counterparty at field-creation time exactly as designed, the lifecycle whole.
- [AWAITING_CENSUS] [RESOLVED]: `enumerate_sig_fields(handler, filled_status=None, with_name=None)` verifies against the installed distribution, yielding `(fq_name, field_value, field_ref)` per `/Sig` field where `filled_status=False` returns the EMPTY (unsigned) fields the filled `reader.embedded_signatures` fold cannot see. `_audited` folds `enumerate_sig_fields(reader, filled_status=False)` in the same read for the `fields_awaiting` scalar — a native `int` on the all-scalar verdict the `observability/metrics` `MeterProvider` reads — so the reserve->fill lifecycle is observable evidence: `fields_awaiting=1` after a `reserve` prepares the counterparty field, `0` once the `sign(placement=reserved)` fill lands. The filled-only audit that reported only embedded signatures was blind to a field awaiting its signer, the multi-party-workflow evidence the ISO-19650 delivery verdict wants.
- [ARCHIVAL_CONFORMANT] [RESOLVED]: the verdict formerly carried only the SELF-DECLARED XMP `pdfa_claim` (the `pdfaid` the file writes about itself). The VALIDATED PDF/A verdict is authored UPSTREAM at `document/tagged#ARCHIVE` through the in-process `pdf_oxide.PdfDocument.validate_pdf_a(level)` Rust oracle (`{valid, level, errors}`, no JVM), so this owner CONSUMES it as the SECOND `archival_conformant` boolean threaded onto each spec EXACTLY as the existing `structural_conformant` seam — `document/tagged.md` already threads its `StructureAudit.conformant` into `AuditSpec.structural_conformant`, and the archival boolean rides the identical path (never a renamed seam that would break the `document/tagged` coupling) — the verdict projecting `archival_conformant` beside `structural_conformant`, both native booleans distinct from the self-declared `pdfa_claim`. A third validated verdict (a PDF/X preflight close, a PDF/UA-2 pass) lands as one more consumed boolean threaded the same way. CRITICAL: no veraPDF/ghostscript subprocess is added here — PDF/A+PDF/UA validation is already resolved in-process by `pdf_oxide` at `document/tagged`, and PDF-X color/TAC preflight is `graphic/color/managed` lcms2 gamut-check, so a validator arm here would duplicate owned capability. The producing edge (`document/tagged#ARCHIVE` emitting the validated boolean) is the cross-file seam.
- [AUGMENT_AXIS] [RESOLVED]: the LTV-maintenance entrypoints verify against the installed distribution — `add_validation_info(embedded_sig, validation_context, in_place=False, output=)` embeds fresh OCSP/CRL/cert material into the DSS (upgrading B-T -> B-LT), and `PdfTimeStamper(timestamper).update_archival_timestamp_chain(reader, validation_context, in_place=False, output=)` extends the archival timestamp chain (refreshing B-LTA before the prior archival timestamp's TSA certificate expires). `_augmented` folds `add_validation_info` over the `SigKind.SIGNATURE` signature indices through `indices.fold(embedded, pdf)` (threading each output as the next input) before the conditional chain refresh, so a multi-signature PDF has every signature's revocation material embedded, never the first alone, and the fold rides the `expression.Block` substrate rather than a `functools.reduce` import. The `Augment` arm requires a `ValidationContext` (the trust/revocation source `add_validation_info` consumes), so `AugmentSpec.validation_context` is non-optional where `SignSpec`/`AuditSpec` carry it optionally.
- [AUDIT_AXIS] [RESOLVED]: `PdfFileReader.embedded_signatures` (a property over EVERY signature, read ONCE and split through one `Block.partition` keyed on the `SigKind.SIGNATURE` discriminant into its `/Sig` and `/DocTimeStamp` halves in a single pass), `EmbeddedPdfSignature.sig_object_type` (the `SigKind` discriminant, a closed `/Sig`/`/DocTimeStamp` vocabulary replacing repeated bare-string comparisons), `validate_pdf_signature(embedded_sig, signer_validation_context=, ts_validation_context=, diff_policy=, key_usage_settings=) -> PdfSignatureStatus`, and `validate_pdf_timestamp(embedded_sig, validation_context=, diff_policy=) -> DocumentTimestampStatus` verify against the installed distribution; the audit is RESILIENT on BOTH axes — the `_validated` projector over `validate_pdf_signature` and the symmetric `_stamped` projector over `validate_pdf_timestamp` both delegate to the one `_resilient[T]` boundary-capture aspect that composes `expression.extra.result.catch(exception=SignatureValidationError)` (the substrate single-exception trap minting `Result[T, SignatureValidationError]`, `.to_option()`-narrowed to `Nothing`, an unlisted raise propagating as a defect) rather than a hand-rolled `try`/`except`, collected through `Block.choose` into the live-status sets so a structurally broken signature is counted as `signatures_broken` (the shortfall against `len(signatures)`) and a broken archival stamp drops `archival_timestamps_valid` to false rather than either aborting the whole audit. The multi-signature aggregates fold in ONE `live.fold(_Tally.step, _Tally())` pass — the `valid`/`trusted` counts (`status.bottom_line`/`status.trusted` as `bool`-summed ints), the `all_valid`/`all_trusted`/`docmdp_ok`/`seed_value_ok`/`timestamps_trusted` AND-aggregates, the `revoked` OR-aggregate, the weakest-link `min` `coverage`, and the worst-case `max` `modification` — replacing the rejected nine-pass `sum`/`all`/`any`/`min`/`max` scatter over `live`; `coverage` verifies as `SignatureCoverageLevel | None` (an `OrderedEnum`, so `min`/`max` are direct), so `_Tally.step` admits a `None` coverage as `SignatureCoverageLevel.UNCLEAR` rather than faulting an unguarded `min` over a `None`. The primary identity folds in ONE `live.try_head().map(_Primary.of).default_value(_Primary())` projection replacing ten repeated `primary.map(...).default_value(...)` chains. The `PdfSignatureStatus` field set the verdict folds verifies against the installed class hierarchy (`PdfSignatureStatus -> StandardCMSSignatureStatus -> SignatureStatus`): `bottom_line`, `trusted`, `revoked`, `coverage` (`SignatureCoverageLevel | None`), `modification_level` (`ModificationLevel | None`, the `OrderedEnum` worst-case `max`), `docmdp_ok`, `seed_value_ok`, `timestamp_validity` (`TimestampSignatureStatus | None` carrying `.timestamp` and `.trusted`), `content_timestamp_validity` (the PAdES content-timestamp `TimestampSignatureStatus | None`), the base `signing_cert` (`asn1crypto.x509.Certificate` whose `.subject.human_friendly`/`.issuer.human_friendly` properties and `.serial_number` int verify), `signer_reported_dt` (`datetime | None`), `md_algorithm` and `pkcs7_signature_mechanism` (`str`), and `qualification_result` (`QualificationResult | None` whose `.status` is a `QualifiedStatus` whose `.qualified` bool reports the EU-qualified outcome) — so the verdict reports WHO signed, WHEN, with WHAT cryptography, at WHAT trust and qualification level, never validity alone. `AuditSpec.required_key_usage`/`required_extd_key_usage` — closed `KeyUsage`/`ExtKeyUsage` `StrEnum` tuples — project a `KeyUsageConstraints(key_usage=, extd_key_usage=)` (`extd_key_usage: Optional[Iterable[str]]` verified to accept both the asn1crypto name form and the OID dotted-string form, so `ExtKeyUsage.DOCUMENT_SIGNING` rides the `1.3.6.1.5.5.7.3.36` id-kp-documentSigning OID asn1crypto has no name for) into the `key_usage_settings=` slot so a key-usage policy is a typed validation constraint rather than a post-hoc check. `read_certification_data(reader) -> DocMDPInfo` (`.permission` the `MDPPerm` level) reads the certifying signature with a `None` guard. `DocumentSecurityStore.read_dss(reader)` is gated behind a `"/DSS" in reader.root` membership test, and its instance `certs`/`ocsps`/`crls`/`vri_entries` cardinalities (verified set in `__init__`) ride the verdict as the real LTV-material evidence — replacing the semantically-empty `archival_metadata=bool(xmp)`. The achieved PAdES level is classified through `PadesLevel.classify` (folded onto the vocabulary): B-LTA when the DSS is present AND every `/DocTimeStamp` archival stamp validates as trusted, B-LT when the DSS is present, B-T when the signature timestamps are trusted, B-B otherwise.
- [TABLE_DERIVATION] [RESOLVED]: the policy tables derive from the vocabulary member, mirroring the `exchange/credential#CREDENTIAL` sibling's `_SIGNING_ALG`/`_INTENT`/`_DIGITAL_SOURCE` name-correspondence pattern: `_MDP = frozendict({perm: MDPPerm[perm.name] for perm in CertifyPerm})` (the `CertifyPerm` member names `NO_CHANGES`/`FILL_FORMS`/`ANNOTATE` match `MDPPerm`'s, verified), and `_COMMITMENT = frozendict({c: GenericCommitment[c.value.upper()] for c in Commitment})` (the `Commitment` values `proof_of_origin`/... upper-case to the `GenericCommitment` member names `PROOF_OF_ORIGIN`/..., verified against the installed enum) — so a new certification permission or commitment is one `StrEnum` member, the pyhanko enum resolved by correspondence, never a hand-enumerated row. `_DIFF` stays explicit because `DEFAULT_DIFF_POLICY`/`NO_CHANGES_DIFF_POLICY` are module-level `diff_analysis` instances no name correspondence reaches. The `_SEED_FLAG` table keys each `SigSeedValFlags` mandatory bit to the `ReserveSpec` field whose presence makes it binding, `reduce(or_, ...)` folding the populated bits over the verified-Flag enum; `_CERT_FLAG` is the identical predicate-per-flag shape over `SigCertConstraintFlags` (`SUBJECT_DN`/`KEY_USAGE`) keyed to the populated `SignerConstraint` axis, and `_DSS_PLACEMENT = frozendict({p: SigDSSPlacementPreference[p.name] for p in DssPlacement})` is the same `StrEnum`-name correspondence as `_MDP` (the `DssPlacement` member names `TOGETHER_WITH_SIGNATURE`/`SEPARATE_REVISION`/`TOGETHER_WITH_NEXT_TS` match the `SigDSSPlacementPreference` names, verified).
- [APPEARANCE_AXIS] [RESOLVED]: the visible-signature appearance is BUILT as the closed `Appearance` invisible/visible family. `PdfSigner(signature_meta, signer, *, timestamper=, stamp_style=, new_field_spec=)` and `PdfSigner.sign_pdf(pdf_out, *, appearance_text_params=, output=)` verify against the installed `0.35.1`, as do `SigFieldSpec.invis_sig_settings`/`visible_sig_settings`, `VisibleSigSettings(rotate_with_page, scale_with_page_zoom, print_signature)`, `InvisSigSettings(set_print_flag, set_hidden_flag, box_out_of_bounds)`, and `pyhanko.stamp.TextStampStyle(stamp_text, border_width, background_opacity)` (a `BaseStampStyle` subclass). The `invisible` case carries an `InvisSigSettings` flag set and drives `stamp_style=None` (the invisible byte-range signature the owner formerly produced through the bare `sign_pdf`); the `visible` case carries a `VisibleSeal` whose `TextStampStyle` renders the positioned drawing-sheet professional seal, its `stamp_text` interpolating the `appearance_text_params` `%(...)s` fields at sign time (`%(signer)s`/`%(ts)s` etc.) — the ISO-19650 sealed-delivery capability the owner previously only named as a Growth swap. One `Appearance.plan()` folds each case to the `PdfSigner` `stamp_style`, the matching `SigFieldSpec` appearance-settings kwarg, and the text-param dict, so `_signed` drives the `PdfSigner` engine ONCE across both modalities. A visible seal requires the `SignSpec.field_box`, so a `visible` appearance with no box raises `SigningError` converted by `async_boundary`. The scan-to-verify seal is BUILT: `VisibleSeal.qr_position` (a `pyhanko.stamp.QRPosition` — `LEFT_OF_TEXT`/`RIGHT_OF_TEXT`/`ABOVE_TEXT`/`BELOW_TEXT`, verified re-exported from `pyhanko.stamp`) selects `QRStampStyle` over `TextStampStyle` in `VisibleSeal.style()`, whose default `stamp_text` interpolates `%(url)s`, so the `text_params` verification link is encoded into the sheet's QR — the machine-verifiable ISO-19650 sealed-delivery seal, the value-discriminated modality the owner previously named only as a Growth swap.
- [SIGN_POLICY_AXIS] [RESOLVED]: `PdfSignatureMetadata.signer_key_usage: Set[str]` (verified `default_factory` `{"non_repudiation"}`) and `PdfSignatureMetadata.dss_settings: DSSContentSettings` verify against the installed constructor. `SignSpec.signer_key_usage: tuple[KeyUsage, ...]` (defaulting `(KeyUsage.NON_REPUDIATION,)`, pyhanko's own default) projects `signer_key_usage=frozenset(usage.value for usage in ...)` — the sign-time key-usage assertion reusing the same `KeyUsage` vocabulary the audit's `KeyUsageConstraints` reads, asserting the expected usage at sign time rather than a post-hoc check. `DssPolicy` projects `DSSContentSettings(include_vri=, placement=_DSS_PLACEMENT[placement])` over the `DssPlacement`/`SigDSSPlacementPreference` name-derived vocabulary, tuning where the B-LT/B-LTA DSS revision lands and whether the per-signature VRI dictionary is written — the DSS/VRI write policy the sign side previously could not express.
- [ASYNC_OFFLOAD] [RESOLVED]: `pyhanko` ships native async mirrors (`async_sign_pdf`, `async_validate_pdf_signature`, `async_add_validation_info`, `async_update_archival_timestamp_chain`) that drive OCSP/CRL/TSA fetch as coroutines without a thread token when paired with an async-aware `ValidationContext`. The owner KEEPS the synchronous surface under one `anyio.to_thread.run_sync(limiter=_THREAD_GATE)` offload: the CMS/CAdES build core is GIL-bound Python plus `cryptography`/`asn1crypto` native work — CPU-bound, isolate-unsafe, and per `concurrency#OFFLOAD_LANE` never legal on the event loop — so the native async path would still block the loop on the signing kernel while only the network fetch yielded, whereas the whole-`_run` `to_thread` offload places the CPU core AND its OCSP/CRL/TSA I/O off-loop under one bounded token, the dominant form. The async mirrors are the adopted path only once the certvalidator async fetch is wired AND the CMS build is separately offloaded, a future trade the `_THREAD_GATE` bound does not yet require.
