# Handoff Transition Contract Lattice

# Lattice Row Shape

- Every admitted handoff declares one frozen row: `transition_id`, `upstream_artifact`, `downstream_artifact`, `upstream_plane`, `downstream_plane`, `owner_symbol`, `revalidate`, `proof_required`, `trust_posture`, `negative_fixture_ids`, optional `stage_tags`, optional `projection_family`, optional `static_port`.
- Optional columns bind when edges cross families or publish static evidence — `stage_tags: frozenset[str]` annotates operational stage owners; `projection_family: ingress | domain | wire | None` tags cross-family seams; `static_port: str | None` names checker-only `Protocol` evidence on enrichment hops without reclassifying runtime planes — omit all three only on monomorphic intra-runtime hops.
- `transition_id` is a stable snake slug — `ingress_bytes_to_validated_struct`, `validated_to_wire_projection`, `trusted_replay_decode` — not ordinal stage numbers alone; stage names annotate rows but do not replace ids.
- `upstream_artifact` and `downstream_artifact` are closed type expressions — `bytes`, `BaseModel`, `Struct`, `Owner`, `Envelope`, `Result[Owner, E]` — erased `object` or `dict[str, Any]` rows fail lattice admission.
- `upstream_plane` and `downstream_plane` draw from closed vocabulary `{static, compiled, runtime}` — carrier-only transitions (`bytes` pre-runtime, encoded `bytes` post-runtime) tag planes explicitly; conflating stage name with plane name fails harness attribution.
- `owner_symbol` pins the module-level gate — `TypeAdapter[T]`, `Decoder[T]`, `wire_encode`, `_validated`, smart constructor — per-request construction in production adapters is a row violation caught at compiled-layer smoke.
- `revalidate` is one of `{never, once_at_ingress, on_patch_delta, trusted_replay_only}` — interior canonical owners default `never`; patch dict handoffs default `on_patch_delta`; store bytes default `trusted_replay_only` with pinned encoder identity.
- `proof_required` is boolean on polymorphic interior slots — monomorphic closed struct rows may set `false` when construction path is decode-only; tagged union detail slots set `true` before persist and stdout emit.
- `trust_posture` labels `{untrusted, semi_trusted, trusted_internal, trusted_replay, never_trusted}` — posture selects decode owner in the ingress matrix but never removes compiled graph for untrusted carriers or runtime owner past materialization exit.
- `negative_fixture_ids` index harness anti-patterns each row must reject — `json_loads_intermediate`, `dump_surgery_projection`, `per_request_codec`, `stage_skip_dict_to_owner`.

# Stage-Native Edge Catalog

- `ingress_carrier_to_validation_exit`: upstream `bytes | str | Mapping[str, object]` (pre-runtime carrier); downstream frozen `BaseModel | Struct | TypeAdapter-validated scalar` (runtime); planes `static/compiled → compiled/runtime`; owner `TypeAdapter.validate_json` or `Decoder.decode`; `revalidate=once_at_ingress`; negatives include `json_loads_intermediate`.
- `validation_exit_to_normalization_exit`: upstream validated model or struct; downstream same runtime type with wire keys collapsed; plane `compiled/runtime → compiled/runtime`; owner codec-internal `BeforeValidator`, `model_validator(before)`, `dec_hook` — logical normalization stage, not second decode pass; `revalidate=never` on success path.
- `normalization_exit_to_construction_exit`: upstream wire-normalized instance; downstream first immutable closure with full field set; plane `runtime → runtime`; owner `BaseModel` pipeline end, `Struct` decode completion, or smart constructor `Result` gate; `proof_required=false` unless interior union slot opens at construction.
- `construction_exit_to_enrichment_exit`: upstream immutable instance; downstream successor via `model_copy`, `structs.replace`, or owner factory; plane `runtime → runtime`; `revalidate=never`; enrichment faults are `Result` or typed family errors — no validator re-entry.
- `enrichment_exit_to_materialization_exit`: upstream enriched instance; downstream canonical domain owner or terminal frozen model; plane `runtime → runtime`; interior modules accept only materialization-exit types without adapter re-entry.
- `materialization_exit_to_wire_projection`: upstream canonical owner; downstream msgspec `Struct` or adapter-owned wire row; plane `runtime → runtime`; owner one-expression `msgspec.convert`, `to_wire()`, or adapter field table — negatives include `dump_surgery_projection`.
- `wire_projection_to_serialization_exit`: upstream wire struct; downstream deterministic `bytes`; plane `runtime/compiled → post-runtime bytes`; owner module-level `Encoder` with `order="deterministic"` when cache keys or hashes depend on bytes; `proof_required` follows polymorphic slot policy on enclosed detail.
- `serialization_exit_to_consumer_decode`: upstream stored or stdout `bytes`; downstream runtime `Envelope` or wire struct; plane post-runtime bytes → `compiled/runtime`; owner pinned `_ENVELOPE_DECODER` or detail decoder — negatives include consumer `json_loads` and per-consumer `Decoder()`.

# Cross-Family And Projection Rows

- `pydantic_validated_to_msgspec_wire`: upstream frozen ingress model; downstream wire `Struct`; planes `runtime → runtime`; owner single `msgspec.convert(validated, WireStruct)` or adapter-owned field table — `msgspec.convert` accepts validated pydantic instances when field layouts align; chained `model_dump` → dict surgery → `Struct(**d)` is indexed negative `dump_surgery_projection`.
- `domain_owner_to_wire_struct`: upstream rich class owner or frozen dataclass; downstream `Struct` with `tag_field`; owner `to_wire()` or adapter-owned convert — domain modules do not import wire struct modules for behavior.
- `wire_struct_to_domain_owner`: upstream decoded wire row; downstream canonical owner via smart constructor `Result[Owner, E]` — never bare `Owner(**struct_asdict)` past construction gate without constructor law.
- `patch_dict_to_successor_owner`: upstream boundary payload `Mapping` plus prior snapshot owner; downstream successor owner; `revalidate=on_patch_delta`; owner `model_validate(snapshot | delta)` or typed patch admission — polymorphic nested slots require nested `model_validate` until replacement axis admits `replace_validated`.
- `enum_token_to_materialized_field`: upstream wire token string; downstream `StrEnum` member on materialized owner; plane `compiled → runtime`; closed vocabulary owner admits token at construction gate — wire tokens, pydantic literals, and msgspec `tag` strings share one owner; adapters map external synonyms only at this row and egress projection rows.
- `variant_discriminant_to_union_arm`: upstream discriminant literal; downstream tagged union arm at construction → enrichment `match`; static exhaustiveness and runtime discriminator routing are separate proof obligations on the same row family.

# Trusted-Replay, Cache, And Migration Rows

- `cache_write_after_proof`: upstream materialized value with optional polymorphic detail; downstream `bytes` in store; `proof_required=true` when detail is tagged union; owner `wire_encode` sharing encoder with stdout path — dual-encoding fixtures fail CI.
- `cache_read_trusted_replay`: upstream store `bytes`; downstream materialized struct; `trust_posture=trusted_replay`; owner pinned `Decoder` with `schema_version` and encoder module identity in row metadata — `revalidate=trusted_replay_only`; `model_construct` permitted only when snapshot bytes match trusted-replay table preconditions.
- `history_envelope_rehydrate`: upstream `bytes` from `load_history`; downstream runtime `Envelope`; owner `_ENVELOPE_DECODER`; delta consumers derive key sets from struct fields — merged staging dicts exist only inside boundary persistence helpers immediately before re-encode.
- `version_migration_single_hop`: upstream stored struct vN; downstream current egress struct; owner `msgspec.convert(old, new)` at read adapter — one hop per row; multi-hop chains declare ordered row sequence at composition root, not chained hand copies in leaf modules.
- `settings_snapshot_roundtrip`: upstream env slice; downstream frozen `BaseSettings`; `proof_required` when settings feed cache keys; owner `capture("settings")` at root — settings schema version independent of envelope `schema_version` unless root explicitly couples them.
- `merged_cache_row_reencode`: upstream `{**retained, **fresh}` staging dict inside boundary helper only; downstream `bytes` in same function; domain modules never appear in merge proof modules — row enforces immediate re-encode, not durable dict handoff.

# Partial, Stream, And Async Rows

- `partial_validation_exit`: upstream incomplete JSON or token buffer; downstream partially closed model; owner `TypeAdapter.validate_*` with `experimental_allow_partial`; `trust_posture=never_trusted` on carrier; downstream construction rows document intentional partial closure — mixing with full domain invariant laws without row annotation is negative `partial_full_invariant_mix`.
- `streaming_bytes_completion`: upstream incremental buffer; downstream complete `bytes` frame; plane carrier pre-runtime; msgspec decoders require complete frames — adapter buffers to completion unless codec documents incremental decode.
- `async_capture_carrier_read`: upstream awaitable transport returning raw carrier; downstream same carrier after `async_capture`; validation and decode remain sync on completed buffer inside async shell — plane tags unchanged; owner symbols mirror sync row with `async_capture` wrapper id.

# Envelope Two-Pass Rows

- `envelope_pass_one_metadata`: upstream wire `bytes`; downstream closed envelope struct with `schema_version`, `claim`, `verb`; unknown `schema_version` fails here without body decode; owner pass-one `Decoder[EnvelopeMeta]`.
- `envelope_pass_two_body`: upstream `msgspec.Raw` body slice plus pass-one discriminant; downstream polymorphic detail struct; owner version-selected body `Decoder` — mutual exclusivity of `report` and `error` enforced in `model_validator(after)` or `__post_init__`, not domain `if` ladders.
- `envelope_egress_wrap`: upstream `Result[Report, Fault]`; downstream single `Envelope` with caps applied; owner `_emit` + `structs.replace` for truncation — success and fault share one struct closure before `_encode`.
- `cap_truncation_spill`: upstream long defect or artifact tuples; downstream capped wire tuples plus `truncated=True`; overflow routes to full-report artifacts keyed by `run_id` — cap metadata read from `msgspec.inspect` field `Meta(max_length=...)`, not adapter magic constants.

# Harness Binding And Attribution

- Each `transition_id` maps to at least one parametrized contract test — missing row coverage is merge blocker when production adapters admit the transition.
- Static layer runs first: import architecture, `TypeForm` registry exhaustiveness, payload law — failures block generative suites.
- Compiled layer: `__pydantic_core_schema__` snapshots, `msgspec.inspect` field-order tables, module-level singleton identity smoke — identity checks use `is` on production `owner_symbol` objects, not structural equality of encode output; per-request codec rows fail here.
- Module-level `TypeAdapter`, `Encoder`, and `Decoder` finalize before parallel test workers or free-threaded importers bind root symbols — post-import reassignment races under `pytest-xdist` and `--disable-gil` builds break `trusted_replay` and cache-key rows silently.
- Runtime layer: metamorphic round-trip, `beartype` on adapter exit, envelope one-write invariant — Hypothesis strategies draw from closed variant families admitted by root decoders.
- Stage-tagged fault prefixes (`strict:`, `validation:`, `config:`, `parse:`) are harness attribution keys — injected violations at each gate assert expected prefix and `owner_symbol`, not undifferentiated messages.
- Failure archaeology routes lattice violations: duplicate validation message from Pydantic and smart constructor → secondary owner removal; stage skip without root exemption → composition root wiring defect; seam remap with foreign keys in domain `match` → anti-corruption adapter row.

# Negative Fixture Catalog

- `json_loads_intermediate`: `json.loads` then `validate_python` — fails ingress_bytes row and compiled-layer single-pass law.
- `dump_surgery_projection`: `model_dump` → key rename dict → `Struct(**d)` — fails cross-family rows; positive fixture is one-expression `msgspec.convert`.
- `per_request_codec`: fresh `TypeAdapter()` or `Encoder()` per call — fails `owner_symbol` identity smoke and cache key laws.
- `stage_skip_dict_to_owner`: raw dict reaches domain owner without validation owner — fails unless documented root exemption registry entry exists.
- `consumer_json_loads`: stdout parsed with loose dict — fails `serialization_exit_to_consumer_decode` row.
- `dual_dump_persist`: history stores `model_dump` JSON while stdout emits msgspec bytes — fails `cache_write_after_proof` and history parity proofs.
- `partial_full_invariant_mix`: partial validation admission with full closure domain invariants on same pipeline without row split — fails partial and construction row pairing.
- `typeguard_cast_narrowing`: `TypeGuard` plus `cast` after materialization exit — fails consumer/handler plane law; positive path uses `TypeIs` and exhaustive `match`.

# Root Guard Seam Rows

- `params_bind_to_bound_or_fault`: upstream raw CLI/host params; downstream bound params or parse `Fault`; owner `_bound` + `BaseParams.bound(verb)`; planes carrier → runtime/`Result`; `revalidate=once_at_ingress`; surplus tokens seed dispatch ring before validation runs.
- `handler_exec_to_result_report`: upstream materialized params and scope; downstream `Result[Report, Fault]`; owner composed layers + `_narrow(handler)`; materialization-exit owners only between enrichment closure and serialization projection.
- `strict_policy_promotion`: upstream success `Report`; downstream promoted or passthrough `Result`; owner `_strict`; empty/skip statuses become `strict:` faults without validator re-entry when `--strict` admits only non-empty folds.
- `detail_proof_validated`: upstream ok `Report` with polymorphic `detail`; downstream unchanged `Result` or proof fault; owner `_validated` + `validate_detail`; `proof_required=true` on tagged detail; `Error` outcomes skip row per explicit guard match.
- `guard_capture_thunk`: upstream fallible thunk; downstream uniform `Result`; owner `_guard` + `capture`/`async_capture` — sole `try`/`except` sites; handlers that "do not throw" still require row for promotion and beartype faults.
- `emit_fold_to_envelope`: upstream `Result[Report, Fault]`; downstream capped `Envelope`; owner `_emit` + diagnostic distillation; `FAILED` reports and hard faults share `Diagnostic` wire shape and cap policy.
- `encode_to_stdout_bytes`: upstream `Envelope`; downstream persisted + printed deterministic `bytes`; owner `_encode`/`wire_encode` + `_emit_envelope`; one-write invariant row suppresses second emit with `FAULTED` stderr fault.
- `registry_dispatch_terminal`: upstream raw params via `rail(bind)`; downstream terminal `Envelope`; owner closes settings, layer stack, scope opener — sole executable entry per `Bind` row; lattice links each `Bind.params_type` to ingress validation row.

# Metamorphic And Law Registry Coupling

- Round-trip laws bind to `owner_symbol` on lattice rows — `wire_encode`, `validate_detail`, `_ENVELOPE_DECODER` alias same singletons in production and conftest; shadow encoders in test helpers fail reference-identity checks tied to row ids.
- `@spec` and `register_law` associate metamorphic witnesses with root codec functions named in rows — laws register on integration owner at decoration time; proof tests import root symbols under test, not parallel encoders.
- Full-pipeline metamorphic row when bijection holds: materialized canonical → wire projection → encode → decode → rematerialize → assert equality — row id `materialization_exit_to_wire_projection` chains with `cache_write_after_proof` in generative suites.
- Subset bijection rows declare excluded fields — computed-on-materialize, wire-omitted, cap-truncated slots document rationale beside law module; lattice `proof_required` does not imply full structural equality on every field.
- Shrinking preserves discriminant legality — hypothesis composites rebuild valid materialized values after shrink; invalid tag mutations fail at construction gate row, not as accepted counterexamples.
- Encoder determinism laws pin `order="deterministic"` on rows where `proof_required` or probe cache keys apply — metamorphic bytes equality uses production `owner_symbol` instance only.

# Diagnostic, Consumer, And Evolution Rows

- `fault_to_diagnostic_distill`: upstream structured `Fault` or `FAILED` report defects; downstream `Diagnostic` wire detail; owner root `_diagnostic` at egress only — domain emits structured payloads, not pre-formatted strings; row pairs with `cap_truncation_spill`.
- `consumer_pass_one_gate`: upstream decoded `Envelope`; downstream interpreted `report` or `error` slot; consumer proves `schema_version`, `claim`, `verb` before body polymorphism — row mirrors static envelope contracts without replacing decode.
- `consumer_detail_narrow`: upstream lawful `detail` on success line; downstream arm selected via shared discriminant vocabulary; owner production `_DETAIL_DECODER`; consumers do not re-run `validate_detail` — trust `_detail_proof_validated` row on success path.
- `subprocess_child_stdout_fold`: upstream child stdout `bytes`; downstream `Completed` receipt in parent fold; decode via same `_ENVELOPE_DECODER` when child is admitted rail — foreign tool bytes stay opaque unless envelope row adopted.
- `delta_two_envelope_compare`: upstream two materialized envelope structs; downstream `RunDelta` notes and counts; no proof assumed — missing history sides fold to `EMPTY` with explicit notes, not `None` coercion on snapshot scalars.
- `schema_version_bump_migration`: upstream egress struct vN in store; downstream current egress struct; breaking wire changes require new literal and ordered migration row sequence — field renames and discriminant moves are version events, not silent `BeforeValidator` patches on live structs.

# Three-Row Alignment Sub-Lattice

- Every cross-family or stage-skipping edge admits alignment sub-rows beside the transition row: static contract (`TypedDict`/`Unpack`/PEP 695 alias), compiled owner (`TypeAdapter` symbol or `Struct` class), runtime handoff artifact — sub-lattice ids suffix `_alignment_static`, `_alignment_compiled`, `_alignment_runtime`.
- `TypeAdapter.json_schema()` and `msgspec.inspect.type_info` supply compiled-oracle snapshots referenced by alignment sub-rows — static payload keys map to compiled field names via alias policy declared once in adapter module.
- Promotion path sub-rows enforce unidirectionality: static payload → compiled validation → runtime owner — reverse projection sub-rows list omit/rename/cap fields explicitly; accidental dump drift is negative `dump_surgery_projection`.
- Discriminant vocabulary sub-row declares one owner for literals, `StrEnum` values, and msgspec `tag` strings — adapters map external synonyms only at ingress and egress rows; interior `match` arms read canonical tokens only.

# Registry, CI, And Composition Root Coupling

- Lattice table lives beside root codec module as `TRANSITION_LATTICE: tuple[TransitionRow, ...]` — adapters import row ids; tests parametrize on `transition_id`; parallel transition prose in docstrings or adapter comments without matching rows is a drift surface — collapse to single table owner.
- Adding a production handoff without lattice row and proof parametrization fails merge — orphan adapters bypassing named `owner_symbol` block CI.
- `REGISTRY` `Bind` rows reference `params_type` promotion through `_bound` — lattice includes `ingress_carrier_to_validation_exit` for each admitted `params_type` with distinct carrier posture when verbs differ.
- Registry rebuild at import proves handler map coverage — new ingress type without adapter route or lattice row fails CI job importing composition root.
- OpenAPI diff on pydantic ingress changes and `msgspec.inspect` diff on wire egress change in the same promotion unit when cross-family row touches both graphs.
- Mutation testing targets adapter `materialize_*`, projection folds, and remap tables referenced by dense seam rows — interior domain folds are secondary mutation surface.

# Stage-Map Versus Lattice Role Map

- Materialization stage names, evidence plane tags, and projection families (`ingress`, `domain`, `wire`) align but are not interchangeable identifiers — lattice rows carry `stage_tags` and `projection_family` when an edge crosses family boundaries.
- Harness tables that name only stages, only planes, or only projection roles without `transition_id` produce false-green coverage — CI contract modules join stage-map exhaustiveness to lattice ids via explicit lookup table, not implicit name equality.
- Ingress projection family edges tag `projection_family=ingress` on validation and normalization rows; domain family edges tag `domain` on construction through materialization; wire family edges tag `wire` on projection and serialization — interior modules hold one projection family per concept.
- Enrichment rows may reference structural `Protocol` ports as static evidence without reclassifying runtime owners into ingress family — `upstream_plane` and `downstream_plane` stay `runtime` while optional `static_port` column names the checker-only contract.

# Promotion Unit And Simultaneous Update Law

- Adding or mutating a lattice row binds in one promotion unit: `TRANSITION_LATTICE` tuple, parametrized contract test module, adapter `owner_symbol` wiring, negative fixture registry, and metamorphic law registration when `proof_required=true`.
- New polymorphic wire slot requires simultaneous update to detail decoder arm, exhaustive `match` on owner, hypothesis strategy registry row, and lattice rows `construction_exit_to_enrichment_exit` through `detail_proof_validated` — partial promotion fails registry import and static arm exhaustiveness together.
- Renaming `owner_symbol` target without updating law registry and singleton identity smoke breaks reference-identity checks — codec submodule moves are version events for `trusted_replay` and `cache_read` rows even when struct layouts are unchanged.
- Lattice row deletion is a breaking change when production adapters still reference `transition_id` — import-time orphan-handoff gate must pass after deletion proves no production adapter or closure row still cites the slug.

# Critical Signals On Handoff Drift

- `transition_id` referenced in production adapter without matching `TRANSITION_LATTICE` row — add row in one promotion unit or collapse adapter to documented root exemption registry entry.
- Harness parametrization listing stage names without `transition_id` join — false-green coverage; collapse to explicit stage-map → lattice lookup table.
- `model_dump` → dict rename → `Struct(**d)` on cross-family row — collapse to one-expression `msgspec.convert`; dict surgery never substitutes for projection row even when field names match.
- Dual stdout/history encoders (`model_dump` JSON in store, msgspec bytes on stdout) — collapse to shared `wire_encode` row with `cache_write_after_proof` and `serialization_exit_to_consumer_decode` foreign keys.
- Interior `validate_detail` after materialization exit — collapse proof to root `_validated` egress row; consumers trust success-path materialization via `consumer_detail_narrow`, not re-proof.

# Collapse Signals On Lattice Drift

- Parallel transition prose in module docstrings diverging from `TRANSITION_LATTICE` — collapse to single table owner.
- Handler-local `Encoder()` matching a root row owner — collapse to module-level singleton pinned at composition root.
- Interior `validate_detail` duplicating root `_validated` row — collapse proof to egress guard only.
- Three parallel types (`FooDTO`, `FooModel`, `FooStruct`) with one cross-family row missing — collapse to one canonical owner with projection row per concept.
- Consumer parser reimplementing envelope decode — collapse to `_ENVELOPE_DECODER` row and shared smoke fixtures.
- Version defaulting on unknown `schema_version` — collapse to pass-one literal failure row and migration hop sequence.