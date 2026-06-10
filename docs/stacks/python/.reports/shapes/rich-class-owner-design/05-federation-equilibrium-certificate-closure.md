# Federation Equilibrium And Certificate Closure

# Certificate Wire Encoding And Bytes Hash Law

- Canonical certificate wire form is msgspec `Struct(frozen=True)` with `forbid_unknown_fields`, `omit_defaults`, and `order="deterministic"` encoder singleton at composition root — JSON canonical projection is egress adapter view only, not authoritative bytes for hash or worker handoff.
- `certificate_bytes_hash` algorithm row names encoder identity (`msgspec_encode_deterministic`, never ad hoc `json.dumps`) — `WorkerHandoffRecord.snapshot_bytes_hash` and certificate `chain_id` content hash use same encoder family as federation propagation rows from shape-system integration doctrine.
- Schema evolution on certificate struct bumps `certificate_schema_version` literal and `schema_hash` — hop field additions without version bump fail evolution hook; alias-only owner transition edits preserve schema version when hop spine unchanged.
- Cross-process transmit carries certificate bytes plus hash — receiver verifies hash before verification fold; pickle of certificate Python object is negative fixture; subinterpreter and multiprocessing handoff use byte slice only.
- Reader-emitted certificate schema from typed-payload contract emission generates struct fields from live transition signatures — parallel JSON certificate templates remain dual-source defect; wire struct is sole generator.
- Redaction for external audit exports operates on logical spine then re-encodes deterministically — redacted bytes get distinct `redaction_profile` literal; hash compares within profile, not across profiles.
- CI metamorphic corpus stores authoritative msgspec bytes beside golden field snapshots — PR diff on certificate bytes without routing change passes alias perturbation modules; spine field change without `schema_hash` bump fails bijection gate.

# Federation Equilibrium Classification Via Certificate Spine

- Equilibrium violation signals `E3` (kind degradation) and `E5` (version non-monotonicity) classify primarily from certificate spine diff against golden fixture, not field snapshot equality alone — snapshot match with hop tier drift or missing ingress rematerialization hop is `E3`; `version_hash` regression without authorized `delta_authorizes_bump` is `E5`.
- Certificate spine diff fold is pure — compares ordered `hops` tuple: `method_qualified_name`, `tier`, `epoch`, `morphism_row_id`, `validator_replay`, `seam_kind` on adapter hops; field snapshot diff is secondary witness.
- `E3` at seam adapter attributes liable module to egress or ingress hop with `live_projection_invoked: true` or missing `F_ingress` rematerialization — enrichment-only chains must not contain Construction ingress hops unless re-ingress event class declared in row.
- `E5` attributes to hop where `version_hash` slice changes without `delta_authorizes_bump: true` or worker handoff rejects stale hash — federation propagation row invalidates identity caches omitting version axis per composition epoch succession-key law.
- Equilibrium reconciliation row selection maps signal to choreography phase from shape-system integration doctrine — certificate provides `proof_layers_to_replay` column listing hop indices requiring metamorphic replay; R3 proof replay re-invokes methods named in spine, not shortcut constructors.
- Quarantine posture on repeated `E3` at same seam edge within grace window isolates adapter hop symbol — certificate records `quarantine_posture_id` foreign key; owner transition methods do not self-quarantine.
- Merge gate on federated packages: when `version_hash` matches across contexts but certificate spine differs, equilibrium module classifies before field-only snapshot diff — prevents silent compose-law drift across bounded contexts sharing vocabulary owner.

# Certificate Spine Non-Sufficiency

- Certificate proof is bidirectionally non-sufficient — neither terminal field snapshots nor ordered hop spine alone discharge federation equilibrium, cross-process replay, global observability budget, or trace-linkage obligations; contract rows declare `equivalence_axes_witnessed` as the proved subset and metamorphic modules stack complementary layers beside spine diff.
- **Field snapshot non-sufficiency** — field snapshot equality alone does not prove equivalent compose law when hop spine changed; golden PR merge gate runs structural spine diff before field snapshot diff when certificate module is in affected owner closure; snapshot match with hop tier drift, missing `F_ingress` rematerialization, `live_projection_invoked: true` on egress, or registry μ→match reorder classifies as `E3` from spine diff against golden — not from snapshot diff alone; `version_hash` match across contexts with differing spine triggers equilibrium module before field-only comparison; trusted replay that reaches field-equal final owner via shortcut constructors fails replay-stability when certificate declares Tier V full replay on named hops; band downgrade, illegal `(S^k, V, S)` sandwich, and `compose_illegal` patterns classify from bytes spine before field snapshot comparison — final field equality does not repair downgrade or reorder defects.
- **Spine non-sufficiency** — ordered `hops` tuple alone does not witness axes omitted from `equivalence_axes_witnessed` — object identity across process seams, `encoded_bytes` stability, logical version monotonicity, and alias equivalence require explicit axis columns or hop-level witnesses; spine without `certificate_bytes_hash` on `WorkerHandoffRecord` is insufficient for cross-process replay — receiver verifies hash before verification fold; pickle of certificate Python object or parent `chain_id` without byte attestation is negative fixture; JSON certificate or `model_dump` diff as authoritative hash source triggers `encoder_skew` — `msgspec_encode_deterministic` encoder singleton at root is sole hash family; per-owner OTEL budget pass in isolation is insufficient when composition root batches multi-owner export — `global_attribute_budget` aggregation runs before exporter invocation; verifier reads certificate egress hop `attribute_key_count` and `max_distinct_per_key`, not live exporter state; post-export certificate amendment is negative fixture; `traceparent` from live `target(settings)` or `agent_context` property at adapter is epoch leak — spine records `live_projection_invoked: false` on egress hops; equilibrium classification from `fault.message` string is insufficient — `E3`/`E5`/`E6` bind to shape-system integration signal vocabulary and spine diff fold only; `sys.monitoring` overlay is non-load-bearing — monitoring presence does not substitute for bytes hash or hop witnesses; redacted audit bytes hash within `redaction_profile` only — cross-profile hash compare is defect; `ContextVar` override without `restored: true` at chain terminus fails parallel-task replay even when stored fields match per context.
- **Classifier inputs (pure spine diff fold)** — compares `method_qualified_name`, `tier`, `epoch`, `morphism_row_id`, `validator_replay`, `seam_kind`, `federation_edge_id`, adapter symbols, and `envelope_arm` ordering — field snapshot is optional secondary witness attached after classification; `proof_layers_to_replay` on certificate names hop indices for R3 metamorphic replay — replay re-invokes spine methods in order, not dump-diff reconstruction.
- Negative controls encode non-sufficiency explicitly — `field_snapshot_only_handoff`, `json_authoritative_hash`, `per_owner_budget_isolation`, `live_projection_traceparent`, `fault_message_classifier`, `spine_without_bytes_hash`, `monitoring_substitutes_spine` — each required failure in steady-state assurance modules beside positive `equilibrium_spine_e3_e5_classifier` and `federation_handoff_certificate` rows.

# Global Cardinality Aggregation At Composition Root

- Per-owner `otel_budget_per_key` rows from lock-order telemetry certificate lattice sum at composition root into `global_attribute_budget` and `global_correlation_index_budget` — export batch guard runs before OTEL exporter invocation; owner methods do not self-limit.
- Aggregation fold groups keys by `observability_kind` — stored-field tags sum low-cardinality ceilings; correlation envelope tags sum index-key ceilings separately from detail-key high-cardinality allowance.
- Federation overflow attribution publishes `overflow_owner_symbol` and `overflow_key` in `EquilibriumFault` at root guard — not silent attribute drop; clamp policy (truncate, hash-prefix remap, reject egress) named in federation propagation row.
- Path export modes (`hash_prefix`, `bucket`, `forbidden`) normalize before aggregation — raw segment counts never enter sum-check; adapter hop in certificate records `path_export_mode` per exported path key.
- Subprocess-only keys partition outside OTEL global sum — `subprocess_key_count` on egress hop certificate field; dual-tier keys count OTEL slice only toward global budget per lock-order telemetry partition law.
- Cardinality metamorphic modules simulate multi-owner export batch from extended row exemplars — single-owner pass does not discharge global aggregation module when owner participates in federated composition root.
- Budget breach replay uses certificate egress hop `attribute_key_count` and `max_distinct_per_key` — verifier reads spine, not live exporter state; post-export certificate amendment is negative fixture.

# Trace Correlation And W3C Traceparent Binding

- Correlation envelope hops (`run_id`, `agent_context`, scrubbed surrogates) export `correlation_index_key` at low cardinality — certificate egress hop records `trace_linkage_mode: w3c_traceparent | baggage_only | forbidden`.
- W3C `traceparent` injection is egress adapter responsibility at composition root — owner `@computed_field` supplies correlation ids; adapter binds `traceparent` from index key plus optional `baggage` entries from authorized computed-export tags.
- Certificate records `traceparent_emitted: bool` and `correlation_ids_exported: frozenset[str]` — interior-only kinds must not appear; promotion to computed-export requires trace linkage review row when `trace_linkage_mode` is not `forbidden`.
- Rail modules consume settings dumps for correlation — live `agent_context` property invocation at trace injection is epoch leak; certificate verifier asserts `live_projection_invoked` false on egress hops.
- Distributed handoff across worker boundary carries `parent_trace_context` in `WorkerHandoffRecord` extension when row declares cross-process trace continuation — worker adapter starts child span linked to parent `traceparent`; snapshot hash does not include trace context bytes.
- Federation propagation aligns trace tags with observability vocabulary from shape-system integration — ad hoc span attribute strings for enum-owned concepts are drift; certificate `exported_kinds` must map to federation tag rows.

# Async Lock Lattice Under Kleisli Port Shells

- Asyncio mutex policy extends lock-order telemetry certificate lattice when enrichment handlers `await` Kleisli port operations — `async_lock_rank` column beside `lock_order_rank`; sync and async ranks share epoch table indices but separate total orders within rank band.
- Acquisition rule for async locks: sort by `(epoch_rank, owner_qualified_name, memo_site, async_before_sync: bool)` — awaitable port shell acquires async lock before entering sync enrichment memo critical section when row documents mixed handler; reverse ordering is `async_lock_inversion` certificate fault.
- `asyncio.Lock` forbidden across await points that call port enumeration — Worker-epoch port IO stays sync-isolated in worker process; Enrichment async handlers may await only read-only probes row-authorized without listing side effects.
- Certificate `HopRecord` gains optional `async_lock_held_during_hop` and `await_boundary_crossed` — metamorphic async interleave fixture proves identical lattice order across tasks; inverted async acquisition fails harness.
- Kleisli retry shells reference `certificate_chain_ref` and `kleisli_chain_ref` foreign keys — scheduling and backoff stay in port pipeline doctrine; certificate proves epoch placement at Worker boundary, not retry policy interior.
- Composition root audit for coroutine handlers documents `async_sync_bridge_symbol` — bridge must release sync locks before await and re-acquire in lattice order after; holding `threading.Lock` across await is critical-section violation from lock-order telemetry certificate lattice, amplified under async scheduling.

# Certificate-Gated Merge And Replay Closure

- Replay closure checklist: trusted materialization re-invokes hop methods in spine order; worker handoff verifies `forbidden_fields_absent`; alias perturbation preserves `morphism_row_id` and spine arm qualnames; Tier V predecessor inequality at each link.
- Promotion unit merging rich owner changes updates certificate golden, morphism table, enum `assert_never`, global budget row, and federation witness in one atomic changeset — partial certificate update without owner transition change is evolution defect when spine should change.
- Production sampling draws chain ids from row policy — sampled certificates still use full schema; sampling affects storage retention only, not hop field omission.

# Worker Handoff And Cross-Package Certificate Propagation

- `WorkerHandoffRecord` extends with `certificate_bytes`, `certificate_bytes_hash`, `parent_trace_context`, `federation_context_ids` — worker entry verifies hash, runs verification fold, materializes settings via Construction-equivalent ingress, emits child chain continuing from snapshot hop.
- Cross-package handoff cites parent `chain_id` and equilibrium invariant ids restored at last parent hop — shape-system federation propagation row aligns `version_hash` and snapshot hash; certificate is liability witness, not pickle id.
- Stale parent alias on live object after transmit does not invalidate worker certificate when handoff hash matches — compose-stability worker succession law preserved; equilibrium module classifies parent drift separately from worker legality.
- Multiprocessing and subinterpreter workers share handoff schema — `worker_kind` literal records process model; forbidden port fields attestation remains mandatory.
- Parent k-step compose after worker spawn appends new parent certificate — in-flight worker child chain references handoff snapshot hash independent of parent subsequent Tier S alias band.

# Seam Functor Certificate At Federation Boundary

- Multi-context egress stacks nest adapter hops with `seam_kind` and `federation_edge_id` — equilibrium `E3` classifies missing rematerialization on `eta` hops across pydantic-msgspec family boundary.
- Ingress replay at Construction re-ingress event adds hop with `re_ingress_reason` literal — enrichment chains lacking declared re-ingress but containing `F_ingress` hop fail spine classifier.
- Cyclopts `F_cli` hop at federation boundary records `param_owner_symbol` and `seam_validate_row_id` — skipped `model_validate` promotion from owner absorption decision lattice surfaces as `E3` at CLI-to-settings seam when certificate expects validation hop.
- Certificate spine diff across federated contexts uses `federation_edge_id` alignment — same owner symbol in different contexts must produce spine-equivalent hop sequences modulo adapter symbols; divergence triggers propagation partial-landing signal `E6` when witness row missing.

# Contract Row Extensions

- Rich-owner rows gain: `certificate_bytes_hash_alg`, `certificate_persistence_tier`, `global_budget_participation`, `trace_linkage_mode`, `async_lock_rank`, `federation_edge_ids`, `equilibrium_invariant_ids`.
- Composition-root rows gain: `global_attribute_budget`, `global_correlation_index_budget`, `overflow_clamp_policy`, `certificate_encoder_singleton_id`.
- Metamorphic modules: `certificate_bytes_roundtrip`, `equilibrium_spine_e3_e5_classifier`, `global_otel_budget_batch`, `traceparent_egress_binding`, `async_lock_interleave`, `federation_handoff_certificate`, `doctrine_closure_checklist`.
- Property targets draw `(chain, bytes_hash, federation_context, async_policy, budget_batch)` from extended rows — shrinking preserves spine legality and hash stability.

# Stability Anti-Patterns And Negative Controls

- Field snapshot equality without certificate bytes hash on worker handoff — insufficient for cross-process replay proof.
- JSON certificate as authoritative hash source — encoder skew versus msgspec deterministic bytes; `encoder_skew` equilibrium signal.
- Global budget check per owner in isolation when composition root batches exports — overflow undetected until production guard.
- `traceparent` derived from live `target(settings)` at adapter — epoch leak; certificate `live_projection_invoked` must be false.
- Async handler holding sync lock across await — critical-section violation; async_lock_interleave module required failure.
- Certificate persistence optional in CI for audited owner — merge gate defect regardless of production toggle.
- Equilibrium classification from `fault.message` string — signal vocabulary from shape-system integration only; spine diff is primary for `E3`/`E5`.

# Refinement Stack And Enrichment Closure At Federation

- `Annotated[..., BeforeValidator]` and `msgspec.Meta(...)` refinements stack on field types — federation ingress replays same validators at each context boundary; single-scalar coercion never promotes rich owner sibling when absorption from owner absorption decision lattice discharges axis.
- Nested frozen sub-owners refine parent via child snapshot merge only — cross-package handoff carries child snapshots inside parent settings bytes, not parallel `{Parent}{Child}` DTO pairs at federation seam.
- `NewType` opaque scalars remain transparent at seam functors per ingress row — enrichment k-chain on parent does not unwrap/re-wrap at each federated hop unless Tier V replay row declares unwrap policy.
- Enrichment closure from composition epoch and compose-stability doctrines unchanged at federation scale — reachable operations remain `{with_configuration, nested child override, context projection, param bound, fabrication}`; new operation names still require promotion quorum plus compose-row and certificate-schema extension in all obligated contexts.
- Concept-density collapse before federation promotion — three single-caller validators on one axis absorb into one ladder before engineers model cross-package owner siblings for the same invariant.

# Concurrency Compose Certificates Across Federated Tasks

- Free-threaded CI interleave across federated contexts requires import-published registry before parallel enrichment — certificate bytes record `memo_publication=import_frozendict` on hops when policy (1) applies; lazy fill during interleave is required failure in all contexts sharing registry owner.
- `ContextVar` override tokens document restoration in bytes spine — override without `restored: true` at chain terminus fails parallel-task metamorphic replay even when stored fields match authoritative owner in each context.
- Lock-order lattice under policy (3) applies uniformly when multiple contexts share composition root process — `lock_order_rank` on hop records must be consistent with epoch table; cross-context deadlock harness uses same lexicographic sort as lock-order telemetry certificate lattice.
- Post-materialization frozen instance reads across federated parallel tasks require distinct Tier V snapshots per binding — shared binding mutation without snapshot is compose defect visible in certificate predecessor inequality witnesses.
- `sys.monitoring` overlay on transition entry appends to bytes certificate as non-load-bearing — monitoring absence does not discharge stability or equilibrium obligations; monitoring presence does not substitute for bytes hash or spine witnesses.

# Rail Interior Morphism Stability Across Federation

- Morphism μ totality rows reference param `Self | Fault` product kind by sealed type — federation contexts consume sole root μ export; string routing tables outside morphism table are absorption defect in every importing context.
- `TypeIs` narrowing witness hop between μ and rail match records `narrow_predicate_symbol` — bool-guard hops are negative-only fixtures; federation rail modules share narrowing gate with origin context.
- Import graph certificate attestation names `morphism_export_site: qualified_module` — duplicate export sites fail static scan before bytes encoding; cyclic envelope imports fail collapse tests at federation boundary.
- Enum promotion changeset updates certificate golden bytes alongside morphism table in all obligated contexts — new `assert_never` arm appears as new hop `envelope_arm` value; alias-only edits must not add or remove arms in spine across contexts.

# Observability Subprocess Versus Export At Federation Root

- Subprocess env keys declared `subprocess_only` bypass OTEL global sum — certificate egress hop partitions `subprocess_key_count` from `attribute_key_count`; dual-tier keys explicit in `dual_tier_keys` set for verifier.
- `remote_env` projection certificate hop records `filtered_key_count` and `injected_key_set` — federation batch aggregation runs on OTEL slice only unless row promotes dual-tier export.
- Interior-only kinds carry zero OTEL attributes in all contexts — promotion to computed-export in one context without federation fan-out witness update triggers `E6` partial-landing signal.
- Correlation envelope exports index key at low cardinality globally — duplicate high-cardinality values on index key across contexts fail global budget module before per-owner checks pass in isolation.

# Nominal Versus Structural Grades At Federation Boundary

- Structural port slice hops at federation edge record `protocol_symbol` and `get_protocol_members_digest` — cross-context projection law runs on doubles; integration law runs on production settings type in each bounded context; certificate does not embed full settings class when slice grade suffices.
- Nominal `@disjoint_base` hops record `sealed_member_qualname` — federation routing keyed on sealed class survives alias perturbation across contexts; arm shift without qualname change is `E3` routing defect in equilibrium classifier.
- Tagged generic `Shape[T: Tag]` hops record `type_param_binding` and `stage_literal` — phantom stage regression across federation handoff is static defect when stage parameters bound on owner generic in both contexts.
- Grade migration at federation edge requires promotion unit spanning all obligated contexts — mid-flight grade drift on one context alone triggers `E6` partial-landing signal; certificate spine diff detects missing witness row update.
- Certificate does not downgrade nominal to structural at seam — hop records reference grade row from protocol runtime-evidence lattice; bool-guard hops remain negative-only fixtures.

# Delta Algebra And Multi-Hop Certificate At Federation Scale

- Closed `TypedDict` delta keys on `with_configuration` hops appear as `delta_key_set: frozenset[str]` in bytes-encoded certificate — wire alias keys absent; cross-context replay verifies Construction-normalized vocabulary at every federated context importing the settings owner.
- `None` clearing versus key omission encoded per hop as disjoint `cleared_keys` and `omitted_keys` — federation handoff replays sequential hop certificates; merged offline delta without row authorization must not hash-equate to sequential bytes.
- `Unpack[ConfigurationDelta]` transitions record `co_varying_key_set` as atomic unit — splitting authorized delta across synthetic Tier S hops without row permission fails spine diff against golden across federation edge.
- `frozendict` allowlist rebuild hops record `allowlist_shell_id` — global budget module treats allowlist key explosion as cardinality input when keys export as stored-field tags; shell alias without row authorization fails isolation certificate at federation verifier.

# Param Bound, Morphism, And Registry Segments At Root

- Param `bound` audited invocations emit single-hop certificate segments — `morphism_row_id` and `golden_hint_hash` stable across federation contexts sharing vocabulary owner; alias perturbation PR proves bytes hash unchanged when routing unchanged.
- Registry `_bound` handler match hop follows μ hop in certificate order — materialize settings → bind → μ → match reflected as epoch-ordered subsequence in bytes spine; reordering detected by equilibrium classifier as `E3` compose defect.
- Cyclopts `F_cli` materialization hop included when param chain crosses federation CLI boundary — surplus never routes through settings `with_configuration`; CLI hop epoch is Construction-to-Enrichment gate crossing visible in spine diff.
- Rail `@tagged_union` receipt after μ carries `rail_fault_arm` sealed name — federation contexts import root morphism export only; duplicate μ export sites fail static acyclic handoff certificate scan before equilibrium classification runs.

# Subinterpreter, Multiprocessing, And Trusted Replay

- Multiprocessing worker entry verifies `WorkerHandoffRecord` bytes hash before local `store` — stale `version_hash` rejects without Construction re-ingress; equilibrium module emits `E5` with `liable_owner_module` from handoff row.
- Parent certificate `chain_id` referenced in handoff — worker child chain does not mutate parent binding; parent k-step compose after transmit appends new parent certificate bytes unrelated to in-flight worker child.
- `concurrent.interpreters` workers inherit identical handoff schema — subinterpreter id recorded as `worker_kind` literal; forbidden port fields attestation mandatory before child chain emission.
- Trusted replay materialization uses same transition methods as live enrichment — shortcut constructors failing k-step witness columns fail replay-stability even when final field equality holds; replay certificate bytes must hash-match golden modulo `monitoring_events` overlay.
- Shared-memory transmit records `transmit_medium` and `byte_length` — hash covers bytes not Python object id; federation propagation row names medium algorithm beside msgspec deterministic encode.

# Schema Evolution And Encoder Federation Hooks

- Certificate `schema_hash` and `certificate_schema_version` bump triggers golden bytes corpus regeneration — owner transition signature change without schema bump fails evolution hook across all federation contexts importing owner.
- Enum alias-only changesets run alias perturbation on morphism table and certificate bytes — routing unchanged implies bytes hash unchanged; new enum member triggers full multi-surface promotion plus equilibrium witness update in one unit.
- Encoder singleton `order="deterministic"` identity compared across federated contexts at boot — per-context encoder clone triggers `encoder_skew` signal from shape-system integration propagation doctrine; reconciliation R2 warm-graph unit before handoff resume.
- Schema-hash field on settings owners gates worker snapshot acceptance — alias-only enum edits do not bump hash; validator replay change bumps hash and invalidates identity caches omitting version axis per composition epoch succession-key law.

# Fabrication, Service, And Scope Certificate Families

- Terminal `φ` hop records `fresh_service_id`, `settings_snapshot_id`, and `construction_proof_hop_id` foreign key — Enrichment `store` certificates without construction proof reference fail worker and federation replay modules.
- Repeated `φ` from identical settings emits distinct `fresh_service_id` per certificate bytes — service ids must never alias across repeated fabrication certificates in same federation batch.
- Service scope operations in Worker epoch append to child certificate linked via `parent_chain_id` — settings owner certificate chain terminates at `φ`; scope chain bytes stored separately without embedding port handles.
- `FabricationOpts` validation sub-hops ordered validate → project → construct under terminal `φ` — skipping validate-before-project appears only in negative-control bytes fixtures.

# k-Step Band Witnesses In Bytes-Encoded Spine

- Tier S hops at alias witness interval record `nested_slot_alias_witness` when band row declares spine check — verifier replays witness from bytes without recomputing band taxonomy from immutable-replacement doctrine.
- Band downgrade mid-chain appears as hop where `sharing_band` changes — `band_downgrade_fault` in equilibrium verifier; final field equality does not repair downgrade; federation contexts share band row foreign keys.
- `max_k_value_copy` exceeded records `value_copy_steps` count — verifier compares to row ceiling; enrichment loop certificates aggregate count across handler batch when row names batch scope.
- Illegal `(S^k, V, S)` sandwiches marked `compose_illegal: true` in negative bytes fixtures only — production spine must never contain pattern; classifier rejects before field snapshot comparison.

# Audit Storage And Retention

- CI metamorphic corpus stores full certificate bytes beside field snapshots — retention keyed by `chain_id` and `certificate_schema_version`.
- Certificate redaction profiles strip `predecessor_id`/`successor_id` for external audit — redacted bytes maintain hop spine and tier witnesses; identity witnesses replaced with content-hash surrogates per row.
