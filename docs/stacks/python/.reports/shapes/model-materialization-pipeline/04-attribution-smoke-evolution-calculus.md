# Attribution Smoke Evolution Calculus

# Attribution Row Shape

- Every admitted failure signal declares one frozen row: `signal_id`, `stage_owner`, `plane_tag`, `owner_symbol`, `lattice_transition_ids`, `surface_ids`, `prefix_pattern`, `remediation_row_id`, `negative_fixture_ids`.
- `signal_id` is a stable snake slug — `validation_field_path_fault`, `wire_decode_type_mismatch`, `detail_proof_failure`, `beartype_handler_violation` — not undifferentiated exception class names alone.
- `stage_owner` draws from the seven-stage vocabulary in ingress-to-materialization doctrine — ingress, validation, normalization, construction, enrichment, materialization, serialization — plus composition-root guard stages (`bind`, `strict`, `validated`, `emit`, `encode`).
- `plane_tag` is one of `{static, compiled, runtime}` per annotate-wrap admission law — attribution names the plane that should have blocked the defect, not the plane where the symptom surfaced.
- `owner_symbol` pins the module-level gate responsible for remediation — `TypeAdapter[T]`, `Decoder[T]`, `_validated`, smart constructor, `_ENVELOPE_DECODER` — duplicate messages from two owners implicate secondary owner removal per remediation row.
- `lattice_transition_ids` link signals to handoff transition contract lattice row set — proof failure in `_validated` cites `detail_proof_validated`; consumer re-proof cites `consumer_detail_narrow` as negative attribution, not primary owner.
- `surface_ids` link signals to oracle conformance surface matrix rows when external consumers misread layout — OpenAPI field on wire symptom cites `openapi_ingress` alias row, not msgspec rename table.
- `prefix_pattern` indexes stage-tagged fault prefixes from ingress-to-materialization doctrine — `strict:`, `validation:`, `config:`, `parse:` — smoke injectors assert prefix and `owner_symbol` jointly, not message substring alone.
- `remediation_row_id` names the collapse or repair row — `remove_duplicate_validation`, `collapse_consumer_json_loads`, `add_migration_hop` — undocumented remediation is merge blocker when signal is production-admitted.
- `negative_fixture_ids` index harness cases that must route to this signal — `conflated_attribution`, `domain_reproof`, `seam_remap_context_leak`.

# Stage-First Routing Catalog

- `pydantic.ValidationError` with field paths → `stage_owner=validation`, `plane_tag=compiled`, `owner_symbol=TypeAdapter`, `prefix_pattern=validation:` — ingress adapter owns capture; domain never re-tags.
- `msgspec.ValidationError` / `DecodeError` on wire bytes → `stage_owner=serialization` or `validation` depending on ingress vs egress decode row; `plane_tag=compiled`; owner pinned `Decoder` from lattice row metadata.
- Smart constructor `Result` error variant → `stage_owner=construction`, `plane_tag=runtime`, `owner_symbol` smart constructor — bare exceptions past constructor gate are `stage_skip_dict_to_owner` negatives.
- `_validated` proof failure → `stage_owner=validated`, `owner_symbol=validate_detail` — enrichment or discriminator wiring at root; not consumer business logic.
- `BeartypeCallHintViolation` at handler edge → `stage_owner=materialization`, `plane_tag=runtime` — handler received owner before narrowing completed; not validation stage unless adapter skipped `beartype`.
- Duplicate invariant message from Pydantic and smart constructor → `signal_id=duplicate_validation_surface`, remediation `remove_duplicate_validation` on secondary owner — harness runs duplicate-validation checklist per concept.
- Ingress dict reaching domain owner without validation owner → `signal_id=stage_skip_without_exemption`, `remediation_row_id=add_root_exemption_or_adapter` — attributes to composition root wiring, not leaf handler.
- Foreign layout keys in domain `match` after materialization exit → `signal_id=seam_remap_context_leak`, `lattice_transition_ids` anti-corruption adapter row — domain modules after successful materialization never emit version faults.
- Migration read wrong discriminant → `stage_owner=serialization`, `surface_ids=migration_stored`, remediation ordered migration hop sequence — not domain `match` arms.
- Version default on unknown `schema_version` → `signal_id=version_oracle_default`, `surface_ids=stdout_envelope`, remediation pass-one literal failure row.

# Duplicate Validation And Secondary Owner Law

- Each concept admits at most one compiled validation owner and one runtime construction gate per invariant class — attribution rows declare `primary_owner` and optional `secondary_owner` columns in remediation metadata.
- When duplicate messages appear, harness attributes to secondary owner for removal — primary owner retains rule; lattice `revalidate=never` on interior canonical owners assumes single validation surface.
- Pydantic field validator plus smart constructor check on same scalar → collapse to construction gate or Pydantic boundary, not both — attribution row `duplicate_validation_surface` blocks merge until secondary removed.
- Normalization `ValueError` inside Pydantic validators stays at validation stage — not re-tagged in domain; attribution does not promote normalization faults to enrichment rows.

# Smoke Federation Row Shape

- Every integration smoke invariant declares one frozen row: `smoke_id`, `invariant_kind`, `owner_symbol`, `decoder_symbol`, `inject_fixture_id`, `expected_signal_id`, `consumer_fixture_ref`.
- `smoke_id` is stable snake slug — `one_write_stdout`, `deterministic_line_shape`, `envelope_decode_parity`, `cap_truncation_spill`, `injected_validation_gate`, `subprocess_child_fold`.
- `invariant_kind` draws from `{single_emit, decode_parity, deterministic_bytes, cap_truncated_artifact_spill, stage_injection, subprocess_envelope}` — closed vocabulary, not free-form test names.
- `owner_symbol` and `decoder_symbol` alias production singletons — `_emit_envelope`, `_ENVELOPE_DECODER`, `wire_encode` — shadow decoders in test helpers fail reference-identity smoke tied to `smoke_id`.
- `inject_fixture_id` indexes violation injectors at named stage owners — validation inject, decode inject, proof inject — conflated attribution fails smoke when `expected_signal_id` differs from observed routing.
- `expected_signal_id` links to `ATTRIBUTION_LATTICE` row — smoke asserts stage-first routing, not merely non-zero exit code.
- `consumer_fixture_ref` points to shared stdout bytes fixtures — consumer contract tests import smoke fixtures, not duplicated parser logic per ingress-to-materialization integration smoke law.

# Root Guard Attribution Rows

- Composition-root guards from ingress-to-materialization doctrine each admit attribution sub-rows linking smoke injectors to lattice ids — guard name is not sufficient without `owner_symbol` pin.
- `_bound` parse faults → `signal_id=params_bind_fault`, `prefix_pattern=parse:`, `stage_owner=bind`, `lattice_transition_ids=params_bind_to_bound_or_fault` — surplus tokens seed dispatch ring before validation runs.
- `_strict` promotion → `signal_id=strict_empty_fold`, `prefix_pattern=strict:`, `stage_owner=strict`, `lattice_transition_ids=strict_policy_promotion` — empty success folds when `--strict` admits only non-empty outcomes.
- `_validated` proof → `signal_id=detail_proof_failure`, `stage_owner=validated`, `owner_symbol=validate_detail` — `Error` outcomes skip inject row per explicit guard match on fault path smoke.
- `_guard` capture → `signal_id=uncaptured_promotion_fault`, `stage_owner=emit`, `owner_symbol=_guard` — handlers that do not throw still require row for beartype and promotion faults.
- `_emit` distillation → `signal_id=cap_truncation_without_spill`, `stage_owner=emit`, `surface_ids=stdout_envelope` — pairs with `cap_truncation_spill` smoke row and lattice `cap_truncation_spill`.
- `_encode` / `wire_encode` → `signal_id=surrogate_encode_fault`, `stage_owner=serialization`, `remediation_row_id=wire_safe_scrub` — residual encode faults fold to minimal fault envelope per ingress-to-materialization outbound serialization law.

# Smoke Invariant Catalog

- `one_write_stdout`: second emit during same `rail()` call suppressed with `FAULTED` stderr invariant — `inject_fixture_id=double_emit`, `expected_signal_id=one_write_violation`.
- `deterministic_line_shape`: stdout bytes stable under `order="deterministic"` — pairs with `cache_bytes` surface row and metamorphic encoder determinism laws.
- `envelope_decode_parity`: bytes from `_emit_envelope` decode through `_ENVELOPE_DECODER` without drift — history persist smoke asserts `store.write_history` bytes equal live emit bytes.
- `cap_truncation_spill`: `truncated=True` implies artifact spill paths keyed by `run_id` — inject long defect tuples; assert spill row, not elongated wire tuples.
- `injected_validation_gate`: ingress violation surfaces at validation owner with `validation:` prefix — not handler or consumer parser.
- `injected_proof_gate`: polymorphic detail corruption surfaces at `_validated` with proof attribution — not consumer re-proof module.
- `subprocess_child_fold`: parent decodes child stdout through envelope decoder when child is admitted rail — foreign tool bytes stay opaque unless envelope row adopted.

# Evolution Consumer Obligation Row Shape

- Every admitted consumer class declares one frozen row: `consumer_id`, `upstream_artifact`, `decode_owner`, `version_gate_fields`, `proof_policy`, `surface_ids`, `lattice_transition_ids`, `absence_encoding`.
- `consumer_id` draws from closed vocabulary — `cli_stdout_parser`, `subprocess_parent_fold`, `history_replay`, `delta_compare`, `probe_cache_key`, `invariant_doubler`, `settings_bootstrap`, `migration_read` — matching ingress-to-materialization doctrine evolution consumer obligations without prose-only tables.
- `upstream_artifact` is closed type expression — `bytes`, `Envelope`, `RunSnapshot`, deterministic encode output — erased dict views fail row admission.
- `decode_owner` pins module-level singleton — `_ENVELOPE_DECODER`, `_DETAIL_DECODER`, settings `TypeAdapter` — per-consumer `Decoder()` construction is negative `consumer_json_loads`.
- `version_gate_fields` lists pass-one closure fields — typically `schema_version`, `claim`, `verb` on envelope consumers — body polymorphism selects only after gate proven.
- `proof_policy` is one of `{required_on_success_detail, skipped_on_fault, optional, never}` — aligns with `_validated` guard and fault-path skip law from ingress-to-materialization doctrine.
- `absence_encoding` documents explicit `EMPTY` and notes policy for delta consumers — `None` coercion on snapshot scalars is negative fixture on `delta_compare` row.

# Evolution Obligation Catalog

- `cli_stdout_parser`: upstream one newline-terminated `bytes` line; `decode_owner=_ENVELOPE_DECODER`; gate `schema_version`, `claim`, `verb`; `proof_policy=required_on_success_detail`; `surface_ids=stdout_envelope`.
- `subprocess_parent_fold`: upstream child stdout `bytes`; same decoder; gate on version; proof when child ok; `lattice_transition_ids=subprocess_child_stdout_fold`.
- `history_replay`: upstream stored `bytes`; same decoder; gate on version; `proof_policy=optional`; trusted-replay posture pinned in row metadata.
- `delta_compare`: upstream two materialized `Envelope` structs; field access only; no decode; `proof_policy=never`; `absence_encoding=EMPTY_with_notes`.
- `probe_cache_key`: upstream deterministic encode; pin encoder identity and `schema_version`; no proof; `surface_ids=probe_fingerprint`.
- `invariant_doubler`: upstream second emit attempt; expect fault envelope on stderr; gate on version; `smoke_id=one_write_stdout` cross-link required.
- `settings_bootstrap`: upstream env slice; pydantic validate via `capture("settings")`; round-trip when settings feed cache keys; `surface_ids=settings_env`.
- `migration_read`: upstream stored struct vN; `msgspec.convert` single hop; gate per version literal; `surface_ids=migration_stored`; no proof assumed.

# Consumer Narrowing And Detail Obligations

- Post-materialization consumers narrow with `TypeIs` and exhaustive `match` on canonical owners — attribution row `typeguard_cast_consumer` routes `TypeGuard` plus `cast` chains to static plane defect on consumer module.
- Tagged `AnyDetail` arms narrow through discriminant tags shared with egress `tag_field` — consumer obligation row `consumer_detail_narrow` cites production `_DETAIL_DECODER`; foreign synonyms were mapped at ingress and egress adapters only.
- Optional detail slots use explicit `None` policy on consumer row — defaulting missing detail to diagnostic or success variant without envelope status proof is negative `optional_detail_default`.
- Exit code projection from materialized envelope fields — consumer rows document that exit status is not re-derived from stderr presence or ad hoc fault string matching per ingress-to-materialization diagnostic distillation law.
- History readers consume bytes written by `_encode` — replay posture is trusted-internal with pinned encoder identity; consumer rows align with `history_replay` evolution obligation and `history_replay` surface row from oracle conformance surface matrices.
- Success-path automation may assume `report.detail` satisfied round-trip proof before emit — consumer row `proof_policy=required_on_success_detail` documents trust boundary; proof failure never reaches stdout per `_validated` guard law.
- Fault outcomes skip detail proof — consumer rows with `proof_policy=skipped_on_fault` block `validate_detail` injectors in consumer contract modules.

# Diagnostic Distillation Attribution

- Operational faults and `FAILED` report defects distill into one `Diagnostic` wire shape at root egress only — attribution row `domain_preformatted_diagnostic` routes interior string assembly defects to composition root, not handler folds.
- `failing_step` names pipeline stage or guard that rejected value — distillation copies stage prefix into `failing_step` or `hint` without re-parsing domain errors; smoke asserts prefix preservation on distilled rows.
- `recent_events` bounded ring seeded from dispatch and parse paths — surplus positional tokens share ring shape with `parse_fault`; attribution links `parse:` and `validation:` injectors to same ring oracle on smoke fixtures.
- `dispatched=False` on bind and config faults — consumer obligation rows document admission-failure interpretation without inferring from empty `report`; smoke injectors cover pre-handler faults independently of handler injectors.

# Schema Evolution Attribution Rows

- Unknown `schema_version` at pass-one decode → `signal_id=version_literal_failure`, `surface_ids=stdout_envelope`, `remediation_row_id=add_migration_hop` — never default to current struct in consumer or oracle tables.
- Cache key drift after deploy → `signal_id=encoder_identity_skew`, `consumer_id=probe_cache_key`, `surface_ids=cache_bytes` — attributes to encoder module path or schema version metadata, not domain handler logic.
- Migration read wrong discriminant → `signal_id=migration_arm_miss`, `surface_ids=migration_stored`, ordered hop sequence in remediation metadata — domain `match` after materialization exit is not migration owner.
- Settings schema evolution independent of envelope version unless root couples in paired metadata — attribution rows for `settings_bootstrap` do not inherit envelope `schema_version` gate fields without explicit row coupling column.

# Proof Debt Ledger Row Shape

- Proof debt from checker gaps, harness suppressions, or unresolved open proofs declares rows beside attribution tables: `debt_id`, `blocking_layers`, `owner_module`, `sunset_criterion`, `linked_signal_ids`.
- `blocking_layers` draws from stage-attributed proof layers in ingress-to-materialization doctrine — static stage-map, import architecture, contract tables, metamorphic round-trip, runtime `beartype` — failures at static layer block generative suites.
- Harness suppressions and `cast` escapes at materialization seams are rejected — debt rows track language-axis gaps only, not production workarounds.
- `@spec` anonymous witnesses fail pytest policy — debt row `law_symbol_registration` blocks merge until named symbols on root codec owners.
- Callable `Discriminator` OpenAPI unification, nested patch one-expression seam, worker boot codec parity — honestly deferred debt rows cite static-runtime decode-planes doctrine/oracle conformance surface matrices open proofs with `sunset_criterion` tied to composition-root promotion record.

# Three-Table Join Law

- Harness modules join `signal_id` ↔ `smoke_id` ↔ `consumer_id` via explicit lookup tables — implicit name equality between stage tags, surface slugs, and consumer classes produces false-green coverage.
- Attribution row without matching smoke injector when stage is production-injected fails CI — every `inject_fixture_id` maps to exactly one `expected_signal_id`.
- Consumer row without `surface_ids` and `lattice_transition_ids` cross-links fails registry import — evolution obligations are not standalone prose.
- Promotion unit binds all three tuples when adding production fault surface — partial promotion fails static registry and smoke parametrization together.

# Metamorphic And Law Registry Coupling

- Attribution rows for proof failures bind metamorphic laws to `owner_symbol` on linked lattice rows — `detail_proof_failure` cites `validate_detail` singleton identity in production and conftest.
- Smoke `envelope_decode_parity` pairs with full-pipeline metamorphic row when bijection holds — failures implicate adapter binding before attribution targets consumer decode owner.
- Hypothesis strategies for smoke injectors draw from closed variant families on detail surface rows from oracle conformance surface matrices — invalid tag mutations fail at construction gate attribution, not as accepted smoke passes.
- Shrinking on attribution smoke preserves discriminant legality — shrink endpoints must not converge on conflated stage routing tuples.

# Registry CI And Composition Root Coupling

- Three tuples live beside root codec module — `ATTRIBUTION_LATTICE`, `SMOKE_FEDERATION`, `EVOLUTION_OBLIGATIONS` — adapters import ids; tests parametrize on slugs; prose in ingress-to-materialization doctrine through oracle conformance surface matrices remains commentary, not second source of truth.
- Adding production injectable stage without attribution and smoke rows fails merge — orphan injectors bypassing named `owner_symbol` block CI before handler integration smoke.
- `REGISTRY` rebuild at import proves every admitted consumer class has evolution obligation row when automation contract is published — missing row fails CI job importing composition root.
- Ruff banned-api rules flag consumer `json.loads`, domain re-proof helpers, and per-consumer `Decoder()` — lint runs before smoke parametrized suites per ingress-to-materialization CI enforcement gates.
- Mutation testing targets adapter attribution seams and consumer narrow folds when smoke logic is dense — interior domain folds remain secondary mutation surface.

# Harness Execution Order

- Static layer: attribution registry completeness, import architecture, evolution obligation exhaustiveness — missing consumer row for published automation contract blocks merge.
- Compiled layer: smoke decoder identity, oracle snapshot parity from oracle conformance surface matrices — per-request decoders fail before runtime smoke.
- Runtime layer: injected stage violations, metamorphic round-trip on success detail, subprocess fold smoke — draws from closed variant families on detail surface rows.
- Failure at static layer blocks expensive generative runs — same ordering as ingress-to-materialization doctrine stage-attributed proof layers and oracle conformance surface matrices harness binding.

# Negative Fixture Catalog

- `conflated_attribution`: validation inject surfaces at handler — fails smoke `injected_validation_gate` and attribution row joint assertion.
- `domain_reproof`: consumer runs `validate_detail` on every line — fails `cli_stdout_parser` proof policy and `consumer_detail_narrow` negative.
- `consumer_json_loads`: stdout parsed with loose dict — fails all envelope consumer rows and `serialization_exit_to_consumer_decode` lattice row.
- `dual_parser_logic`: consumer reimplements envelope parser beside smoke fixtures — fails `consumer_fixture_ref` shared-fixture law.
- `proof_on_fault_line`: consumer proof on error outcomes — fails `proof_policy=skipped_on_fault` on all consumer rows.
- `none_coercion_delta`: missing history side becomes `None` on snapshot scalar — fails `delta_compare` absence encoding row.

# Promotion Unit And Simultaneous Update Law

- Adding production fault routing or consumer contract binds in one promotion unit: attribution row, smoke row, evolution obligation row, lattice cross-links, surface cross-links, inject fixtures, and shared smoke bytes fixtures.
- New stage owner or guard symbol requires simultaneous update to attribution catalog, smoke injectors, and metamorphic law registry when polymorphic slots affected.
- Renaming `owner_symbol` without updating smoke `decoder_symbol` and attribution rows breaks reference-identity checks — codec submodule moves are version events for trusted-replay consumers.

# Failure Archaeology On Three-Table Defects

- Smoke passes but consumer fails — attribution targets consumer row decode owner or version gate fields, not handler logic.
- Consumer passes but CI attribution fails — targets missing `expected_signal_id` linkage or wrong `plane_tag` on attribution row.
- Proof debt row open while smoke green — targets blocking layer mis-tagging; static debt cannot be waived by runtime smoke alone.
- Subprocess parent fold mis-decodes child — targets `subprocess_child_stdout_fold` smoke row and envelope adoption metadata, not parent domain fold.

# Collapse Signals On Calculus Drift

- Module docstrings describing fault routing diverging from `ATTRIBUTION_LATTICE` — collapse to single table owner.
- Per-consumer envelope parsers beside `_ENVELOPE_DECODER` — collapse to evolution obligation rows and shared smoke fixtures.
- Hand-maintained evolution consumer tables in README — collapse to `EVOLUTION_OBLIGATIONS` tuple beside root codec module.
- Interior domain folds emitting transport-prefixed fault strings — collapse to root guard formatting and attribution rows on guard stages only.

# Integration Smoke Consumer Alignment

- Root integration smoke asserts one-write invariant, deterministic stdout line shape, and envelope decode parity — consumer contract tests reference the same smoke fixtures, not duplicated parser logic per ingress-to-materialization doctrine.
- Injected ingress violations surface at validation owner — injected wire decode failures at decode owner — injected proof failures at `_validated` guard — conflated attribution fails the smoke module jointly with attribution row assertion.
- Cap truncation smoke asserts `truncated=True` implies artifact spill paths exist — full evidence resolves through history keyed by `run_id`, aligning consumer obligation `cli_stdout_parser` with smoke `cap_truncation_spill`.
- Subprocess parent fold smoke decodes child stdout through root envelope decoder when child is admitted rail — evolution row `subprocess_parent_fold` and smoke row share `consumer_fixture_ref`.
