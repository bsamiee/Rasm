# Lock Order, Telemetry Cardinality, And Audit Certificate Lattice

# Lock-Order Lattice Under Memo Policy (3)

- Memo policy (3) (`threading.Lock`) activates only when import-published `frozendict`/`frozenset` and `ContextVar` lanes are infeasible — contract row must document infeasibility reason before lock column appears; default absorb back to (1) or (2) when cold-path fill can publish immutable snapshot after first fill.
- Lock-order lattice is a finite partial order on lock symbols `L(owner_symbol, memo_site)` — acquisition rule: sort symbols by `(epoch_rank, owner_qualified_name, memo_site)` lexicographically ascending; release in reverse; never acquire `L_b` while holding `L_a` when `L_a > L_b` in sort order.
- Epoch rank table fixes cross-owner order: Construction publish locks (0) precede registry compose locks (1) precede enrichment class-memo locks (2) precede Worker fabrication locks (3) — enrichment compose must not hold epoch-2 lock while waiting on epoch-1 registry lock filled by another task holding epoch-2.
- Nested lock on same owner symbol is forbidden — one `L(AssaySettings, registry_cache)` per symbol; multiple memo sites on one owner collapse to single lock or promote to import-published table split by memo key immutability.
- `threading.RLock` is rejected on rich owners — memo fill must be idempotent cold-path once under lock; re-entrant memo recursion signals compose defect or promotion to import-published structure.
- Free-threaded builds (PEP 779): `functools.cache`/`lru_cache` preserve internal coherence but do not guarantee single-fill under parallel first-touch — policy (3) cold-path fill must be idempotent once; prefer import-published `frozendict`/`frozenset` over lock when publication can complete at import.
- Lock hold time budget: fill under lock computes immutable `frozendict`/`frozenset` snapshot, assigns to class attribute, releases — no IO, no port enumeration, no `model_validate` replay inside critical section; violation fails concurrency metamorphic modules.
- Compose associativity under lock policy (3): k-step Tier S chains on distinct bindings remain lawful when each task acquires locks in lattice order before reading shared class memo — shared binding without Tier V snapshot plus lock inversion is required failure.
- Free-threaded CI interleave fixture: thread A runs enrichment k-chain, thread B triggers lazy registry fill — positive pass requires A and B acquire locks in identical lattice order; inverted acquisition must deadlock or fail assert in test harness, not corrupt memo table.
- `ContextVar` lane (policy 2) and lock policy (3) on different memo sites of same owner is lawful when row documents disjoint sites — CV holds request-scoped override, lock guards class-scoped cold fill; lattice order still applies when one task touches both sites in one handler.
- Lock-order row column `lock_order_rank: int | forbidden` beside `memo_publication` — `forbidden` when policy is (1) or (2); rank is epoch table index when policy is (3).
- Deadlock harness requirement: any two owner symbols that share policy (3) memo sites must declare cross-symbol rank in compose row or share epoch rank table — undefined cross-symbol order fails 3.20-readiness concurrency certificate module.

# OTEL Cardinality Budgets Per Observability Kind

- Cardinality budget is a contract-row ceiling on distinct attribute value count per process lifetime per attribute key — not a sampling rate alone; budgets pair with `observability_kind` on the owner contract row (stored-field, computed-export, correlation envelope, interior-only).
- **Stored-field tag** default budget: low cardinality (≤ 32 distinct values per key) — enums, protocol tokens, backend kind; path roots and hostnames require normalization to bounded vocabulary (hash prefix, bucket index) before export when distinct count exceeds ceiling.
- **Computed-export tag** default budget: medium cardinality (≤ 10² per key) — `agent_context` and solution tags must derive from bounded stored axes; unbounded run-scoped strings promote to correlation envelope kind with separate budget row.
- **Correlation envelope** default budget: high cardinality permitted on dedicated low-cardinality index key plus high-cardinality detail key — `run_id` exports as single correlation id attribute; scrubbed surrogate ids must not duplicate across keys; budget row names `index_key` and `detail_key` separately.
- **Interior-only signal** budget: zero OTEL attributes — Enrichment may read; Egress emits nothing; promotion to computed-export requires budget row amendment and cardinality impact review, not silent `@computed_field` add.
- Path-segment and artifact basename attributes absorb into hashed or truncated projection at adapter — raw `artifact(kind, *parts)` segments never export unbounded; adapter column `path_export_mode: hash_prefix | bucket | forbidden`.
- Subprocess env assembly may carry higher cardinality than OTEL when row declares `subprocess_tier=extended` — OTEL budget still applies to exported tags; env-only keys skip OTEL ceiling but remain in interior-only or correlation kinds for rail dumps.
- Cardinality metamorphic modules sample lawful settings snapshots from row exemplars — assert attribute key count and distinct value count after simulated k-step enrichment and single egress encode; overflow fails CI before production export.
- Budget breach at runtime routes to composition-root guard — owner methods do not silently drop attributes; guard clamps, aggregates, or rejects egress with typed fault on the propagation row; interior enrichment never self-limits OTEL.
- SDK guardrails (`OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT`, `OTEL_METRIC_EXPORT_ATTRIBUTE_COUNT_LIMIT`) complement but do not replace per-key distinct-value ceilings — contract-row budgets name allowed keys and max distinct counts; exporter key-count limits are backstop only.
- Enum and vocabulary promotions that add wire tokens do not automatically raise cardinality — new enum member updates budget row only when distinct export value count crosses ceiling; alias-only edits skip budget bump.

# Compose-Chain Certificate Schema

- Certificate is an immutable msgspec struct or frozen dataclass row emitted at composition root on each audited transition chain — not a log string; canonical fields are versioned schema with `certificate_schema_version` literal.
- Required certificate spines: `chain_id` (stable uuid or content hash of ordered hop list), `owner_symbol`, `base_family` (dataclass | pydantic | msgspec), `hop_count`, ordered `hops: tuple[HopRecord, ...]`.
- `HopRecord` binds per link: `hop_index`, `method_qualified_name`, `tier` (S | D | V), `epoch` (construction | enrichment | egress | worker), `predecessor_id` and `successor_id` (object id at hop boundary for inequality witness, not equality claim), `predecessor_is_successor: bool` (must be false at Tier V), `sharing_band` when tier S, `alias_witness_index` when row declares interval, `memo_publication` mode, `version_hash` slice unchanged unless `delta_authorizes_bump: bool`.
- Tier V hops add `validator_replay: full | none` — must be `full` on settings transitions; shallow `model_copy` paths emit `validator_replay: none` only in negative-control fixtures, never production certificates.
- Fabrication terminal hop `φ` adds `service_symbol`, `fresh_service_id`, `settings_snapshot_id` — certificate proves terminal morphism; no further settings hops on same binding without new chain.
- Morphism hop after param `bound` adds `morphism_row_id`, `envelope_arm` (sealed type name), `mu_output_kind` — row id derives from `(owner_qualified_name, verb_or_kind, sealed_arm)` not wire token; alias perturbation replay asserts identical `morphism_row_id` across certificate reserialization.
- Worker handoff certificate extension `WorkerHandoffRecord`: `parent_chain_id`, `snapshot_bytes_hash`, `version_hash`, `transmit_epoch`, `forbidden_fields_absent` (port handles, ContextVar tokens) — worker entry emits child certificate with new `chain_id` continuing from snapshot materialization.
- ContextVar override segment adds `contextvar_token_id`, `restored: bool` — parallel-task metamorphic replay fails when `restored` is false at chain terminus even if stored fields match authoritative owner.
- `sys.monitoring` overlay optional field `monitoring_events: tuple[str, ...]` — non-load-bearing; absence does not invalidate certificate; presence does not discharge hop witnesses.
- Certificate `schema_hash` field pins reader-emitted hop field set — owner transition signature change without schema hash bump fails evolution hook even when hop method names unchanged.

# Certificate Verification And Replay Law

- Verification fold is pure on certificate plus optional field snapshot — no live `getmembers`, no port IO, no env re-read; checks predecessor inequality at Tier V, alias witness indices at declared intervals, epoch monotonicity along chain, lock-order compliance when memo policy (3), OTEL budget headroom when egress hop present.
- Certificate spine non-sufficiency: field snapshot equality is insufficient proof — two chains may terminate snapshot-equal while hop spine diverges on tier, morphism row id, lock-order rank, or egress budget columns; merge and replay gates diff spine before snapshots.
- Replay law: trusted materialization reconstructs chain by re-invoking transition methods named in certificate hops — shortcut constructors or bare `model_copy` where certificate declares Tier V full replay fails replay-stability module even when final snapshot matches.
- Certificate diff across releases compares hop method names, tier, epoch, morphism row ids, lock-order ranks, OTEL egress columns, and version_hash — field snapshot equality alone does not prove equivalent compose law when hop spine changed.
- Bijection module maps `chain_id` to golden certificate fixtures — orphan or duplicate `chain_id` in CI corpus fails; alias-only enum PRs must show certificate spine unchanged except monitoring overlay.
- Worker replay verifies parent certificate `WorkerHandoffRecord` before local `store` — stale `version_hash` rejects without Construction re-ingress; lawful stale parent alias after transmit remains valid on worker when snapshot hash matches handoff record.

# Lock Lattice And Certificate Interleave

- Certificate emitted after all locks acquired for chain released — hop records never claim memo_publication=import_frozendict while lock policy (3) fill is in progress; `lock_held_during_hop: bool` on HopRecord when policy (3) documents critical section boundary.
- Lock-order violation detected at runtime publishes certificate with `fault_kind=lock_inversion` at composition root — owner transition methods do not catch; enrichment chain aborts per compose-chain failure short-circuit — no subsequent owner transitions on the fault binding.
- Import-published registry (policy 1) chains omit lock fields — certificate `memo_publication=import_frozendict` implies `lock_order_rank=forbidden` on all hops; CI asserts consistency.

# Telemetry Egress And Certificate Coupling

- Egress hop in certificate lists `exported_kinds: frozenset[ObservabilityKind]` and `attribute_key_count`, `max_distinct_per_key` post-encode — OTEL budget verification reads certificate egress hop, not live exporter state alone.
- Correlation envelope hops require `correlation_index_key` in certificate matching row — duplicate high-cardinality values on index key fail budget module.
- Promotion of interior `@property` to computed-export requires certificate schema bump on affected owner row — new export hop must appear in metamorphic OTEL round-trip with budget columns filled.

# Param Bound And Registry Certificate Segments

- Param `bound` invocations emit single-hop certificate segments when audited — not composable across verbs; `hop_count` is 1 per `bound` call; registry morphism `μ` is separate hop appended at composition root with distinct `chain_id` suffix or continuation record per row policy.
- Surplus fault arms bind `envelope_arm` to sealed `Fault` type name and `golden_hint_hash` — diagnostic bytes hashed at construction, not stored verbatim in certificate; replay compares hash to golden fixture from param fault cap row.
- Success `Self` arms bind `param_type_name` and `verb` — arity table row id from vocabulary contract, not token count alone; k-step param enrichment outside `bound` remains undefined and must not appear in certificate spine.
- Registry `_bound` handler match hop follows μ hop in certificate order — materialize settings → bind → μ → match is reflected as epoch-ordered subsequence; reordering in certificate spine is compose defect witness.
- Cyclopts boundary certificates include `F_cli` materialization hop when param chain audited — surplus never routes through settings `with_configuration`; CLI slice hop epoch is Construction-to-Enrichment gate crossing.

# Delta Algebra And Certificate Coupling

- Closed `TypedDict` delta keys on `with_configuration` hops appear in certificate as `delta_key_set: frozenset[str]` — wire alias keys must be absent; Construction-normalized domain vocabulary only; negative fixture certificates containing alias keys fail static delta-law modules.
- `None` clearing versus key omission encoded per hop as `cleared_keys` and `omitted_keys` disjoint sets — certificate replay verifies non-commutative semantics across multi-hop chains; merged offline delta without row authorization must not produce equivalent certificate to sequential hops.
- `Unpack[ConfigurationDelta]` transitions record atomic delta unit in certificate — `co_varying_key_set` must appear or shrink as one block; splitting authorized delta across synthetic Tier S hops without row permission fails certificate spine diff against golden.
- `frozendict` allowlist rebuild hops record `allowlist_shell_id` (object id of published shell) — value-copy band chains assert shell id changes per step when union overlay applies; alias on shell across steps without row authorization fails isolation certificate module.

# Fabrication And Service Certificate Hops

- `FabricationOpts` validation hop precedes projection hop precedes construct hop in certificate — three sub-records under terminal `φ` or single `φ` hop with ordered `sub_hops` tuple; skipping validate-before-project appears only in negative-control fixtures.
- Exclusive-resource sub-owner legality cites Construction-epoch validator hop id in certificate — Enrichment `store` certificates reference `construction_proof_hop_id` foreign key; fabrication without construction proof reference fails worker and enrichment replay modules.
- Repeated `φ` from identical settings emits distinct `fresh_service_id` per certificate — predecessor settings `successor_id` may alias when Tier S band declares intentional-alias; service ids must never alias across repeated fabrication certificates.
- Service scope operations (`open`, `write_bytes`) in Worker epoch append to child certificate after worker handoff — settings owner certificate chain terminates at `φ`; scope chain `chain_id` links via `parent_chain_id` without embedding port handles; worker and egress hops may carry `kleisli_chain_ref` when port discovery chain attaches — retry and backoff policy stays outside certificate interior.

# Rail Interior And Morphism Certificate Stability

- Rail `@tagged_union` fault receipt after μ carries `rail_fault_arm` sealed name — distinct from param `Fault` arm; certificate proves total morphism at root, not per-handler re-wrap; handler string keys absent from spine.
- `TypeIs` narrowing witness hop optional between μ and rail match — when present, records `narrow_predicate_symbol` and `narrowed_arm`; bool-guard hops are negative-only fixtures.
- Import graph certificate attestation: `morphism_export_site: qualified_module` — sole μ export per process composition root; duplicate export sites fail static acyclic handoff certificate scan.
- Enum promotion changeset updates certificate golden fixtures alongside morphism table — new `assert_never` arm appears as new hop `envelope_arm` value; alias-only edits must not add or remove arms in spine.

# Subinterpreter And Multiprocessing Certificate Discipline

- Multiprocessing worker entry certificate must include `WorkerHandoffRecord` with `forbidden_fields_absent=True` attestation — pickle of port-typed fields produces certificate fault at root before worker chain starts.
- Parent certificate `chain_id` referenced in handoff — worker child chain does not mutate parent binding; parent k-step compose after transmit appends new parent certificate unrelated to in-flight worker child until reload event.
- `concurrent.interpreters` workers inherit identical handoff schema — subinterpreter id recorded as `worker_kind` literal; snapshot bytes hash algorithm named in row (`msgspec_encode`, `json_canonical`, etc.).
- Shared-memory transmit of settings snapshot records `transmit_medium` and `byte_length` — hash covers bytes not Python object id; parent alias law on live object after transmit does not invalidate worker certificate when handoff hash matches.

# Seam Functor Certificate At Epoch Crossings

- Ingress `F_ingress` hop at Construction epoch records `ingress_adapter_symbol` and `forbid_unknown_fields` posture — enrichment certificates must not contain ingress hops unless chain explicitly replays Construction re-ingress event class.
- Egress `F_egress` hop records `exclude_computed_fields` policy and `exported_field_set` — live `target(settings)` invocation bit must be false; true bit is epoch-leak negative fixture in certificate verifier.
- Natural transformation η hop records `source_family` and `target_family` — `(η, S^k)` illegal sandwiches produce certificate verifier fault before field snapshot comparison.
- Multi-hop egress stack certificates nest adapter hops in associate order — domain module publishes one canonical owner; per-hop DTO siblings appear as adapter symbols in spine, not parallel owner symbols.

# Audit Storage, Retention, And CI Corpus

- CI metamorphic corpus stores full certificate bytes beside field snapshots — retention keyed by `chain_id` and `certificate_schema_version`; production retention duration is row policy, schema mandatory in CI.
- Certificate redaction for external audit exports strips `predecessor_id`/`successor_id` when privacy row requires — redacted corpus maintains hop spine and tier witnesses; identity witnesses replaced with content-hash surrogates documented in row.
- Golden certificate diff on PR uses structural diff on hop spine — field snapshot diff alone insufficient for merge gate when certificate module is in affected owner closure.
- Orphan `chain_id` in corpus without owner contract row blocks merge — reader-emitted certificate schema must reference live owner symbol.

# Contract Row Extensions

- Rich-owner rows gain: `lock_order_rank`, `lock_infeasibility_reason`, `otel_budget_per_key`, `path_export_mode`, `correlation_index_key`, `certificate_schema_version`, `subprocess_tier`.
- Service and worker rows gain: `worker_handoff_required: bool`, `certificate_chain_family` linking parent and child chain ids.
- Metamorphic modules: `lock_order_deadlock_or_order`, `otel_budget_egress`, `certificate_spine_replay`, `alias_perturbation_certificate`, `worker_handoff_certificate`.
- Property targets draw lawful chains with certificates from extended rows — shrinking preserves hop legality and budget headroom; illegal lock inversion certificates fail at construction gate.

# Cross-Package Certificate Propagation

- Cross-package handoff rows align `version_hash` and `snapshot_bytes_hash` with worker handoff certificate — owner handoff liability cites certificate `chain_id`, not ad hoc pickle id.
- Seam crossings append adapter hop to certificate with `seam_kind: ingress | egress | eta` — multi-hop egress associates at root; each adapter hop is certificate-visible for audit diff.
- Equilibrium reconciliation uses certificate spine diff to classify seam versus interior faults — cardinality remap failures attribute to adapter hop in certificate, not vocabulary owner hop.

# Stability Anti-Patterns And Negative Controls

- Proving audit replay from field equality without certificate spine — insufficient when tier or morphism row id drifted.
- Holding lock policy (3) across await or port IO — critical section violation; concurrency and epoch modules fail together.
- Exporting raw path segments to OTEL without `path_export_mode` — cardinality breach; budget module rejects even when functional egress succeeds.
- Certificate `morphism_row_id` keyed on enum `.value` or wire string — alias edit shifts id without routing change; bijection drift undetected.
- Worker certificate missing `forbidden_fields_absent` affirmation while port handle present — negative fixture in cross-process modules.
- Lock-order ranks assigned per developer convention — only epoch rank table plus lexicographic owner/site sort is lawful; ad hoc ordering fails deadlock harness.

# Checker, Evolution, And Mutation Hooks

- Certificate schema version bump triggers replay corpus regeneration — new hop field requires golden certificate diff in same changeset as owner transition change.
- Mutation testing adds lock-inversion mutants (acquire registry lock before settings memo lock), OTEL budget bypass mutants (export interior-only kind), certificate spine mutants (drop Tier V predecessor inequality) — kill required alongside `_bound` and band-downgrade mutants.
- Enum alias-only changesets run alias perturbation on both morphism table and certificate golden fixtures — routing unchanged implies certificate spine unchanged.
- Ruff and beartype on certificate struct modules treat hop field types as runtime-evaluated — `TYPE_CHECKING`-only imports on certificate schema owners reject merge until unconditional field symbols restore.
- Cross-checker modules verify certificate-typed transition wrappers preserve `Unpack[ConfigurationDelta]` and `Self` narrowing on the callable seam signature matrix — certificate emission does not relax signature proof debt.
- Certificate schema fields emit from live owner transition signatures via contract reader — parallel JSON certificate templates remain dual-source defect.

# k-Step Band Witnesses In Certificate Spine

- Tier S hops at alias witness interval `i` record `nested_slot_alias_witness: bool` when compose row declares spine check — floor(k/2) alias witness links when k ≥ 3 on path-copy `Map` bands; certificate `sharing_band` and `alias_witness_index` cite compose-row ids, not re-derived band prose; verifier replays witness without recomputing band taxonomy.
- Band downgrade mid-chain appears as hop where `sharing_band` changes — value-copy union after path-copy `Map` without row migration emits `band_downgrade_fault` in verifier; final field equality does not repair downgrade.
- Intentional-alias band breaks at Tier D/V child isolation hop — subsequent Tier S hops inherit post-isolation band in certificate; illegal `(S^k, V, S)` sandwiches marked `compose_illegal: true` in negative fixtures only.
- `max_k_value_copy` exceeded in chain records `value_copy_steps` count — verifier compares to row ceiling; enrichment loop certificates aggregate count across handler invocation batch when row names batch scope.

# Nominal Versus Structural Certificate Grades

- Structural port slice hops record `protocol_symbol` and `get_protocol_members_digest` — doubles used in CI carry same digest as production slice; certificate does not embed full settings type when slice grade suffices; hop records reference sealed arm names consistent with `TypeIs` grade on port slices — certificate does not downgrade structural to nominal.
- Nominal `@disjoint_base` hops record `sealed_member_qualname` — wire token changes do not alter qualname on alias perturbation; arm shift without qualname change is routing defect caught by alias fixture.
- Tagged generic `Shape[T: Tag]` hops record `type_param_binding` and `stage_literal` when phantom stage bound — stage regression appears as certificate static defect when checker binds stage on owner generic.
- Grade migration mid-chain requires certificate schema version bump — verifier rejects mixed-grade spine without row migration in same changeset.

# Lock Lattice Composition With Registry Publish

- Registry import-published tables (epoch rank 1) must complete before any enrichment lock (epoch rank 2) acquisition — certificate on registry publish chain ends before enrichment chain `chain_id` begins when chains split; combined chain shows epoch-0/1 hops before epoch-2 hops with monotonic rank.
- Lazy fill attempt during parallel import triggers `memo_fill_deferred` hop flag — positive CI requires deferred fill after import completes; certificate records deferral, not silent corruption.
- Composition root publish event is single Construction-epoch certificate per process boot — worker processes emit separate boot certificate; parent boot chain id referenced in worker handoff when worker spawns after parent publish.

# OTEL Subprocess Versus Export Budget Interaction

- Subprocess env keys declared `subprocess_only` in row bypass OTEL `attribute_key_count` on egress hop but appear on `subprocess_key_count` field — metamorphic modules assert partition: no key in both sets unless row promotes dual-tier export.
- `remote_env` projection certificate hop records `filtered_key_count` and `injected_key_set` — allowlist rebuild from new `frozendict` per settings step appears as separate hop with distinct `allowlist_shell_id`.
- Dual-tier keys (`stored-field` OTEL plus `subprocess_tier=extended`) require budget on OTEL slice only — certificate lists `dual_tier_keys` explicitly for verifier partition logic.

# Property-Test And Hypothesis Certificate Targets

- `@given` certificate targets remain family-scoped — settings chain certificate, worker handoff, OTEL budget, lock-order, and band-witness targets never merge into one mega-property.
- Strategies build lawful hop sequences from extended contract rows then materialize certificates — arbitrary method name sequences filtered only by runtime success are rejected strategy design.
- Composite draws tuple `(prior, delta_sequence, k, band, epoch, lock_policy, certificate_schema_version)` from registered exemplars — independent draw of k or lock rank without row coupling is negative strategy design.
- Shrinking on certificate failure truncates hop list while preserving epoch monotonicity, lock-order legality, and band witness intervals — illegal Tier V equality witness mid-shrink fails at construction gate.
- Negative-only strategies include lock inversion orderings, budget-exceeding path exports, band downgrade without migration row, certificates omitting worker `forbidden_fields_absent`, and ingress hops on enrichment-only chains — shrink endpoints must not converge on illegal certificates.
- Regression fixtures pin full certificate bytes beside golden field snapshots — CI diff on certificate without snapshot change passes on alias-only routing stability PRs; spine change without owner method change fails bijection module.
