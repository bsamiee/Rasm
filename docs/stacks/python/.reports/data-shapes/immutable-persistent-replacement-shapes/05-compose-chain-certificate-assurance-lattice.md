# Compose-Chain Certificate, Assurance, And Federation Lattice

# Compose-Chain Certificate Schema

- **Certificate** is a frozen msgspec struct or `@dataclass(frozen=True, slots=True, kw_only=True)` row emitted at composition root on each audited transition chain — not a log string, pytest stdout capture, or `model_dump` diff artifact.
- Required spine fields: `certificate_schema_version: Literal[...]`, `chain_id` (content hash of ordered hop list or stable uuid), `owner_alias`, `base_family` (`dataclass` | `pydantic` | `msgspec`), `morphism_classes: frozenset[Literal["live", "snapshot", "replay", "batch"]]`, `hop_count`, `hops: tuple[HopRecord, ...]`.
- `chain_id` binds to golden fixtures in CI — orphan or duplicate `chain_id` in corpus fails before behavioral suites; alias-only field rename PRs must show hop spine unchanged except optional monitoring overlay fields.
- Certificate `schema_hash` pins reader-emitted hop field set — owner transition signature change without schema hash bump fails evolution hook even when method names unchanged.
- Optional overlay `monitoring_events: tuple[str, ...]` is non-load-bearing — absence does not invalidate certificate; presence does not discharge hop witnesses.

# HopRecord Row Shape

- Each **HopRecord** is the minimal auditable spine atom binding one link in the compose chain — not a collapsed log line, dump diff hunk, or inferred replace step; production emit produces one row per catalog-registered transition step unless observation-free nested path row documents single-hop closure.
- Required hop columns: `hop_index`, `transition: str` (qualified method name), `morphism_class`, `tier` (`S` | `D` | `V`), `kernel`, `band`, `path_id`, `delta_id`, `predecessor_id`, `successor_id`, `predecessor_is_successor: bool` (must be false on material live and batch hops).
- **Path evidence**: `path_segments: tuple[Segment, ...]` copied from registered lens row — not reconstructed from wire keys at certificate emit time; `focus_hash: str` is deterministic digest of canonical focus value or endomorphism id at apply boundary.
- **Batch evidence**: when `morphism_class=batch`, `path_bindings: tuple[PathBindingDigest, ...]` where each digest pairs `path_id`, `focus_hash`, and binding index — order matches `DeltaRow.path_bindings` registration order unless row documents commutative key set.
- **Replay evidence**: when `morphism_class=replay`, `replay_policy_id`, `schema_version`, `store_key`, `bytes_hash` — fresh materialize witness; bare session `copy.replace` without pinned policy is negative-control only, never production certificate content.
- **Snapshot evidence**: when `morphism_class=snapshot`, `snapshot_carrier_kind`, `fields_present: frozenset[str]` — documents fields included for round-trip re-entry; omitted load-bearing fields fail replay validate module when policy requires full snapshot.
- Tier V hops add `validator_replay: full | none` — production paths must record `full`; shallow replace paths appear only in negative fixtures indexed by `negative_fixture_ids`.
- Version load-bearing chains add `version_before`, `version_after`, `delta_authorizes_bump: bool` — version unchanged across material forward hop when version is load-bearing is certificate defect, not silent pass.

# PathBinding Digest And Focus Hash Law

- **Focus hash** is computed on canonical serialized focus evidence at root apply boundary — msgspec msgpack deterministic encode, pydantic `model_dump(mode="json")` on focus child, or scalar `repr` for closed value types per family row; hash algorithm pinned beside owner import as `focus_hash_kernel`.
- `PathBindingDigest` fields: `binding_index`, `path_id`, `focus_hash`, `tier`, `band` — digest tuple is the compose-chain certificate segment delta batch equivalence fold segment named without expansion; audit replay diffs bindings by hash, not raw focus object equality across releases.
- Wire-sourced focus values hash after alias normalization and Tier V validate product — hashing pre-validate staging dicts is rejected; certificate emit runs after root tier guard completes.
- Collection focus on `Map`/`Block`/`frozendict` binds combinator morphism id plus focus payload hash — `map.add(key, value)` records key hash and value hash separately when row documents split witness for ordered batch overlays.
- Cross-arm union hops include `arm_segment` literal matching nested path lens discriminant segment — certificate without arm segment on cross-arm transition is compose defect witness.

# OTEL Cardinality Budgets Per Morphism Class

- Cardinality budget is contract-row ceiling on distinct attribute value count per process lifetime per attribute key — budgets pair with `equivalence_axis` and `delta_id` from delta batch equivalence fold calculus, not sampling rate alone.
- **Live replace hop** default budget: low cardinality (≤ 32 distinct values per key) on `tier`, `kernel`, `band`, `path_id` — enums and closed literals only; unbounded handler strings normalize to hash prefix or bucket index before export.
- **Batch hop** default budget: medium cardinality (≤ 10² per key) on `delta_id` and `binding_count` — `path_id` per binding exports as indexed keys `path_id_0`, `path_id_1` capped by row `max_bindings_exported`; overflow routes to composition-root guard fault, not silent attribute drop at leaf handlers.
- **Replay hop** default budget: low on `schema_version`, `store_key`, `replay_policy_id`; high-cardinality permitted only on dedicated `bytes_hash` detail key with separate low-cardinality `replay_policy_id` index — duplicate surrogate ids across keys fail budget module.
- **Snapshot hop** default budget: zero OTEL attributes by default — snapshot carriers are interior or persistence evidence; promotion to exported tags requires budget row amendment and certificate schema bump.
- **Observation chain** (`composition_mode=observation_chain`) exports `chain_segment_index` and `delta_id` per segment at root boundary only — interior spine observation between bindings does not multiply exported `path_id` cardinality beyond row ceiling.
- Budget breach at runtime routes to root guard — owner transition methods do not silently drop attributes; guard clamps, aggregates, or rejects egress with typed fault; interior folds never self-limit OTEL.
- Certificate egress hop lists `exported_attribute_keys: frozenset[str]`, `attribute_key_count`, `max_distinct_per_key` post-encode — budget verification reads certificate egress hop, not live exporter state alone.

# Equivalence Axis Witness Columns

- Each certificate carries top-level `equivalence_axes_witnessed: frozenset[Literal["object_identity", "logical_version", "field_equality", "alias_equivalence", "encoded_bytes"]]` — subset of axes proved on chain; undeclared axis is not implied by certificate presence.
- Per-hop axis columns optional when hop row documents partial witness — batch atomic closure typically witnesses `object_identity` and `alias_equivalence` under Tier S; store CAS success adds `logical_version` and `encoded_bytes` on terminal hop only.
- **Object identity**: every live and batch hop asserts `predecessor_is_successor is False`; replay hop asserts fresh materialize even when field snapshots match prior session — object axis is process-local, not serializable across process seams in certificate alone.
- **Logical version**: terminal hop asserts monotonic version when load-bearing — intermediate observation-chain hops may record `version_observed` without increment; per-binding version bumps require documented intermediate witness rows.
- **Alias equivalence**: Tier S hops record `alias_witness_fields: frozenset[str]` — unchanged slots expected to alias; Tier D/V hops record `isolation_hops: frozenset[int]` as hop indices where alias must break.
- **Encoded-bytes**: terminal egress hop records `wire_bytes_hash` under deterministic encoder — successor bytes differ from predecessor when serialized version or payload changes; reusing predecessor bytes hash with bumped version is positive failure in verification fold.
- Field snapshot attached to certificate is optional evidence — snapshot equality without matching hop spine and focus hashes does not prove equivalent compose law across releases.

# Certificate Spine Non-Sufficiency

- Certificate proof is bidirectionally non-sufficient — neither terminal field snapshots nor ordered `hops: tuple[HopRecord, ...]` alone discharge replacement equivalence, path-binding replay, cross-process handoff, global OTEL budget, or federation equilibrium obligations; contract rows declare `equivalence_axes_witnessed` as the proved subset and metamorphic modules stack complementary layers beside spine diff.
- **Field snapshot non-sufficiency** — field snapshot equality alone does not prove equivalent compose law when hop spine changed; golden PR merge gate runs structural spine diff before field snapshot diff when certificate module is in affected owner closure; snapshot match with hop tier drift, missing `morphism_class=replay` materialize hop, wire-sourced batch without Tier V validate hop, `kernel`/`band` mismatch against catalog row, or permuted `path_bindings` classifies from spine diff against golden — not from snapshot diff alone; trusted replay reaching field-equal final owner via shortcut `model_copy`, bare `copy.replace` where certificate declares Tier V full replay, or dump-diff-derived deltas fails replay-stability module even when field snapshot matches; illegal morphism sequence (`snapshot → live` without validate, `batch partial → publish`), batch binding order permutation, and focus hash drift classify from spine before field snapshot comparison — final field equality does not repair reorder, downgrade, or hash defects.
- **Spine non-sufficiency** — ordered `HopRecord` spine alone does not witness axes omitted from `equivalence_axes_witnessed` — object identity across process seams, `encoded_bytes` stability, logical version monotonicity, and alias equivalence require explicit hop-level columns or top-level axis declaration; spine without matching `focus_hash` on every live and batch hop is insufficient for path-binding replay — verifier recomputes focus hash from stored focus value via pinned `focus_hash_kernel` row, not raw focus object equality or `model_dump` dict equality across releases; hashing pre-validate staging dicts, wire keys reconstructed into `path_segments` at emit time, or focus evidence before root tier guard completes is rejected — `focus_hash` runs at apply boundary after tier classification; spine without `certificate_bytes_hash` on `WorkerHandoffRecord` is insufficient for cross-process replay — receiver verifies hash before verification fold; pickle of certificate Python object or parent `chain_id` without byte attestation is negative fixture; JSON certificate or `model_dump` diff as authoritative hash source triggers `encoder_skew` — `msgspec_encode_deterministic` encoder singleton at root is sole hash family; per-owner OTEL budget pass in isolation is insufficient when composition root batches multi-owner export — verifier reads certificate egress hop `attribute_key_count` and `max_distinct_per_key`, not live exporter state; post-export certificate amendment is negative fixture; `sys.monitoring` overlay is non-load-bearing — monitoring presence does not substitute for `focus_hash`, bytes hash, or hop witnesses; optional field snapshot does not discharge spine or focus hash obligations when axes undeclared.
- **HopRecord as spine atom** — each hop is the minimal auditable unit binding morphism class, tier, kernel, band, path/delta identity, and focus evidence; collapsing multiple production transitions into one hop without `intermediate_observation` row documentation is fraud witness; splitting one atomic batch apply into multiple hops without observation load-bearing row is compose defect; `HopRecord` field set changes bump `certificate_schema_version` — alias-only enum edits preserve spine when `transition`, `path_id`, `delta_id`, and `focus_hash_kernel` unchanged.
- **Focus hash as binding witness** — `PathBindingDigest.focus_hash` and per-hop `focus_hash` are the cross-release stable identity for path focus — not Python object identity, not pydantic model equality, not wire key strings; collection focus on `Map`/`Block`/`frozendict` binds combinator morphism id plus split key/value hashes when row documents ordered batch overlays; cross-arm union hops require `arm_segment` on `HopRecord` matching nested path lens discriminant — certificate without arm segment on cross-arm transition is compose defect; audit replay diffs `path_bindings` tuple by digest order and hash set, not by re-executing interior replace folds from field snapshot alone.
- **Classifier inputs (pure spine diff fold)** — compares ordered hops on `transition`, `morphism_class`, `tier`, `kernel`, `band`, `path_id`, `delta_id`, `focus_hash`, `validator_replay`, and `path_bindings` digest tuple — field snapshot is optional secondary witness attached after classification; `proof_layers_to_replay` on certificate names hop indices requiring metamorphic replay — R3 re-invokes spine transition methods in order, not dump-diff reconstruction.
- Negative controls encode non-sufficiency explicitly — `field_snapshot_only_audit`, `focus_object_equality_without_hash`, `pre_validate_focus_hash`, `wire_key_reconstructed_path_segments`, `spine_without_focus_hashes`, `json_authoritative_hash`, `per_owner_budget_isolation`, `replay_without_materialize_hop`, `binding_order_permutation`, `monitoring_substitutes_spine` — each required failure in steady-state assurance modules beside positive `compose_chain_bijection` and certificate exemplar rows.

# Verification And Replay Morphisms

- **Verification fold** is pure on certificate plus optional field snapshot — no live port IO, no env re-read, no `getmembers` introspection; checks predecessor inequality, focus hash set equality against registered exemplars, tier monotonicity, band/kernel pairing against catalog rows, OTEL budget headroom when egress hop present.
- **Replay law**: trusted materialization reconstructs chain by re-invoking transition methods named in certificate hops — shortcut `model_copy`, bare `copy.replace` where certificate declares Tier V full replay, or dump-diff-derived deltas fail replay-stability module even when final owner matches field snapshot.
- Certificate diff across releases compares hop method names, `path_id`, `delta_id`, focus hashes, tier, kernel, and `schema_hash` — field snapshot equality alone does not prove equivalent compose law when hop spine changed.
- Bijection module maps `chain_id` to golden certificate fixtures beside owner import — set difference between production emit paths and golden corpus fails CI before property suites.
- Worker handoff extension `WorkerHandoffRecord`: `parent_chain_id`, `snapshot_bytes_hash`, `schema_version`, `forbidden_fields_absent` (mutable session caches, `ContextVar` accumulators) — child process emits new `chain_id` continuing from replay materialize hop per snapshot-replay succession law.

# Catalog Federation And Parity Extension

- Root publishes `CERTIFICATE_CATALOG: tuple[CertificateExemplarRow, ...]` beside `TransitionRow`, `LensRow`, and `DeltaRow` — each production audited transition has exactly one exemplar row naming expected hop count, morphism classes, and witnessed equivalence axes.
- **CertificateExemplarRow** binds `owner_alias`, `transition`, `expected_hop_count`, `required_path_ids`, `required_delta_id`, `equivalence_axes_witnessed`, `otel_budget_row_id`, `golden_chain_id`.
- Catalog parity joins four catalogs and owner transition methods in one CI assertion — set difference in any direction fails before behavioral suites; orphan certificate emit without exemplar row is merge blocker.
- Harness parametrization imports all four catalogs plus certificate exemplars — tier matrix, band composites, path depth, batch binding count, and certificate spine derive from registered rows, not caller inference at test sites.
- `PATCH_TO_PATH`, `DELTA_TO_BINDINGS`, and `LENS_CATALOG` resolution tables must match focus hashes in golden certificates — drift in mapping tables fails certificate parity before path or delta suites run.

# Steady-State Assurance Invariants

- Steady-state holds when no promotion unit is in flight and composition root has completed boot witness — six replacement-specific invariants plus federation rows from shape-system integration doctrine form closed assurance catalog for this domain.
- **R1 catalog parity** — every `TransitionRow`, `LensRow`, `DeltaRow`, and `CertificateExemplarRow` matches production handler imports — parametrized contract test passes on boot and pre-merge gate.
- **R2 tier orthogonality** — root tier guard samples pass on scheduled cadence — no production path routes Tier V delta through shallow replace.
- **R3 sharing band publication** — `frozendict`, `Map`, `Block`, and frozen owner tables fully materialized before worker import — lazy mutation behind `MappingProxyType` fails publication proof under PEP 779.
- **R4 replay pin integrity** — encoder identity, schema version, and store key pins match replay policy row — round-trip proof samples pass for persistence-dependent owners; stale session cache without generation invalidation fails health tick.
- **R5 compose-chain bijection** — golden `chain_id` corpus bijects to audited production transitions — certificate schema hash matches reader emission; hop spine stable across alias-only enum edits except documented overlay fields.
- **R6 equivalence axis coverage** — every `DeltaRow.equivalence_axis` appears in at least one golden certificate `equivalence_axes_witnessed` — orphan axis declaration without certificate witness fails assurance gate.
- Assurance decision lattice rows: **boot witness** (R1–R3), **pre-merge witness** (R1, R5, catalog parity), **scheduled replay** (R4, R2), **post-promotion certify** (R1–R6 full), **health tick** (R3, R4 lightweight) — undocumented assurance path is merge blocker.

# Exemplar Certificate Row Patterns

- **Single-hop Tier S live**: one `HopRecord` with `morphism_class=live`, `path_id` to scalar root lens, `focus_hash` of closed value, `equivalence_axes_witnessed={object_identity, alias_equivalence}`, `otel_budget_row_id=live_low` — equivalent to single-hop lens row bundled for audit parity.
- **Observation-free nested path**: `hop_count=1`, `path_segments` depth two in one record, `intermediate_observation=false`, `kernel=copy_replace`, `band=intentional_alias` on scalar spine and path-copy on collection focus when row documents mixed band — one certificate hop, not two sequential bare replaces.
- **Atomic multi-binding batch**: `morphism_class=batch`, `delta_id` registered, `path_bindings` digest tuple length equals binding count, `composition_mode=atomic`, `equivalence_axes_witnessed` includes `object_identity` — CAS absent; store overlay uses separate exemplar.
- **Store CAS success chain**: `replay` or `live` predecessor hop → `batch` with `store_guard` → terminal `encoded_bytes` witness — conflict arm certificate carries `fault_kind=stale_generation` exemplar in negative corpus only.
- **Replay materialize opening hop**: always first in consuming-process certificate — `bytes_hash`, `replay_policy_id`, `schema_version`; subsequent hops use live or batch only on replay product identity.
- **Audit receipt bundled batch**: `delta_kind=audit_receipt` binding digest in same certificate as field batch when receipt attests same event — separate `chain_id` when receipt is independent lifecycle evidence.

# Cross-Morphism Compose Chain Laws

- Compose chains concatenate morphism classes under root guard — legal sequences include `live → snapshot → encode`, `replay → batch → live`, `live → batch → snapshot`; illegal sequences include `snapshot → live` without validate, `replay → copy.replace` without materialize hop, `batch partial → publish`.
- **Associativity (certificate level)**: two observation-free batch closures on same predecessor merge to one certificate with concatenated `path_bindings` digests when `DeltaRow` documents merge — separate `chain_id` values when intermediate observation load-bearing.
- **Non-commutativity**: snapshot after batch differs from batch after snapshot — certificate hop order must match production execution order; reordering hops without matching execution is fraud witness, not optimization.
- Receipt append distributes per snapshot-replay succession law and delta batch equivalence fold calculus — audit hop with `delta_kind=audit_receipt` appears in same certificate as field batch hops when receipt attests same transition event; post-hoc list mutation never appears in hop spine.
- Store CAS failure emits certificate with `fault_kind=stale_generation` and `successor_published: false` — prior owner field values unchanged; partial binding apply on conflict path is positive failure in verification fold.
- Endofold union arm migration registers explicit hop with `arm_segment` — hidden arm change inside generic field hop without discriminant segment fails R5 bijection module.

# Materialization Stage And Certificate Placement

- Certificate emit runs at root handler edge after tier classification, path/delta resolution, and atomic apply complete — staging payloads and ingress carriers never appear as canonical hop owners in production certificates.
- Stage six promotion on all binding spines completes before batch or multi-hop path certificates record collection-band hops — child mutable residue without promotion is shallow-law breach witnessed as certificate emit abort, not silent hop omission.
- Stage seven egress hop is terminal certificate segment when persistence or OTEL load-bearing — `wire_bytes_hash` recorded after `project_wire` plus deterministic encode; domain field repair before encode does not appear in lawful hop spine.
- Trusted-replay chains begin with `morphism_class=replay` hop in consuming process — parent-process hop records must not appear in child certificate without `WorkerHandoffRecord` extension.
- Stage-skipping exemption requires documented root waiver row beside affected owner — waiver certificates carry `stage_skip_exempt: true` and `waiver_row_id`; leaf modules never emit exempt certificates.

# Certificate Evolution And Schema Migration

- `certificate_schema_version` literal bumps when `HopRecord` field set changes — addition of optional overlay fields may not require bump when reader treats absence as empty; removal or rename of required hop fields always bumps version.
- Owner family promotion (pydantic ingress → msgspec domain) emits new exemplar row — golden `chain_id` may change when hop kernels change even if field snapshot matches; migration row documents `prior_schema_hash → successor_schema_hash` mapping for audit diff tools.
- Alias-only enum or vocabulary edit preserves hop spine when `transition`, `path_id`, `delta_id`, and focus hash kernels unchanged — CI alias perturbation module asserts golden certificate reserializes identically except optional monitoring overlay.
- Breaking replace-kernel change (pydantic shallow replace semantics shift) requires exemplar refresh and R2 tier orthogonality re-run — trust-boundary samples update beside owner module, certificate exemplar updates beside root import.
- Deprecated transition methods remain in golden corpus until producers migrate — egress and certificate emit route to replacement transition in one adapter fold; deleting hop from corpus while production still emits it is seam break witnessed as R5 failure.

# Scheduled Assurance Gates And Health Ticks

- **Boot witness gate** runs R1–R3 immediately after root warm phase — catalog federation parity, tier downgrade negative samples, and publication immutability before foreign context import activation.
- **Pre-merge gate** runs R1, R5, and full five-catalog set difference — blocks merge when certificate exemplar orphan or hop spine drift detected on changed owner modules.
- **Scheduled replay gate** runs R4 and R2 on cadence from root policy — round-trip proof corpus and tier matrix for persistence-dependent owners; failures escalate to replay invalidate and session cache flush rows before quarantine.
- **Post-promotion certify gate** runs R1–R6 after vocabulary or schema promotion unit completes — steady-state certification commit row records `next_scheduled_gate` timestamp only when all invariants green.
- **Health tick gate** runs lightweight R3 and R4 between deep gates — detects registry lazy mutation regression and encoder pin drift without full generative property replay; tick interval foreign-keyed to root policy record, not hardcoded in doctrine.

# Rejection Signals

- Certificate without registered `path_id` or `delta_id` when production transition uses path or batch machinery — collapse to catalog federation row before emit enabled.
- Focus hash computed on pre-validate wire dict — collapse to Tier V validate then hash at apply boundary.
- Field snapshot attached without hop spine — collapse to full certificate or remove snapshot-only audit false confidence.
- OTEL export of unbounded `path_segments` tuple literals — collapse to bounded `path_id` and indexed binding keys per budget row.
- Replay certificate hop with session owner `predecessor_id` matching `successor_id` — collapse to fresh materialize proof from snapshot-replay succession law.
- Certificate chain claiming Tier S on wire-sourced batch binding — collapse to Tier V full validate hop in spine.
- Shared mutable accumulator building `path_bindings` before single atomic apply — collapse to delta batch atomic batch expression.
- Interior module caching certificate half-state across requests — collapse to root emit with session cache key on generation and schema version.
- Golden `chain_id` corpus drift without schema hash bump — collapse to evolution hook and exemplar row update.
- Verification fold that discharges compose law from field equality alone — collapse to hop spine plus focus hash replay module.

# Proof Obligations By Certificate Segment

- **Emit obligation**: production audited transition produces exactly one certificate per invocation — zero emit on unaudited Tier S interior replace is lawful when exemplar row documents audit exemption; duplicate emit per single transition is defect.
- **Focus hash obligation**: recomputing focus hash from stored focus value at verification time equals recorded `focus_hash` — algorithm pin in `focus_hash_kernel` row; drift without kernel bump fails golden corpus.
- **Spine obligation**: ordered `transition` qualified names match production call stack modulo root handler wrappers — inlined helper names absent from catalog fail spine match even when field snapshot correct.
- **Binding order obligation**: `PathBindingDigest.binding_index` monotonic and matches `DeltaRow.path_bindings` registration order unless commutative key set row documents order independence — permutation negative fixture proves order-sensitive batches fail when misordered.
- **Replay pin obligation**: `replay_policy_id`, `schema_version`, and `store_key` in replay hop match root frozen replay policy row byte-for-byte — pin drift fails R4 without requiring live store access in verification fold.
- **OTEL obligation**: egress hop `max_distinct_per_key` less than or equal to budget row ceiling — certificate records actuals for post-hoc audit; exceedance is positive failure even when encode succeeded at runtime before guard deployment.

# Succession Axis Embedding In Certificates

- Four-axis succession lattice embeds as certificate columns — each axis maps to explicit hop fields or top-level `equivalence_axes_witnessed`.
- **Business key** column on store CAS hops — `business_key` literal matches optimistic-concurrency row; CAS conflict certificates omit successor publish fields, retain business key for fault attribution.
- **Encoded-bytes identity** terminal hop pairs `wire_bytes_hash` with `encoder_policy_id` referencing root deterministic encoder row — successor bytes hash change without field delta when version serialized is positive failure.
- **Logical version** on forward chains — `version_after == version_before + 1` when load-bearing unless exempt row documents non-serialized version; rollback chains use named rollback hop with pinned snapshot hash, not downward version without rollback row.
- Cross-axis independence witnessed in certificate — field-equal replay product with distinct `predecessor_id` and `successor_id` proves object axis; same certificate must not claim encoded-bytes unchanged when version serialized and bumped.

# Free-Threaded Certificate Emit And Publication

- Certificate emit is root-serialized per chain when memo policy (3) or shared emit buffer participates — parallel tasks emit distinct `chain_id` values; shared emit accumulator without lock lattice is seam defect foreign-keyed to rich-class-owner when applicable.
- Golden corpus and `CERTIFICATE_CATALOG` finalize before worker threads import — post-import exemplar mutation races under PEP 779 parallel importers and fails R3 identically to registry publication law from structural sharing identity lattice.
- `ContextVar` certificate context stores last emitted `chain_id` and terminal successor snapshot immutably — rebinding after emit completes; per-hop partial publish to shared `ContextVar` dict is root seam defect.
- Worker child emits continuation certificate only after `WorkerHandoffRecord` validates parent snapshot — parent hop records do not appear in child spine without handoff extension segment.

# Round-Trip Certificate And Wire Proof Coupling

- Root round-trip proof from immutable replacement doctrine (`decoder.decode(encoder.encode(project_wire(canonical)))`) pairs with terminal certificate egress hop — `wire_bytes_hash` in certificate must equal hash of round-trip proof sample bytes when same owner and encoder policy row apply.
- Round-trip proof witnesses wire legality, not domain replace correctness — certificate verification fold treats round-trip failure as boundary fault; it does not authorize domain `copy.replace` repair on failed value.
- Certificate without terminal egress hop when owner persistence keys on encoded bytes is audit exemption defect unless documented waiver — persistence-dependent owners require `wire_bytes_hash` witness on steady-state path.
- Tagged union round-trip samples attach `union_arm` to egress hop — arm tag in encoded bytes must match certificate discriminant segment from final live or batch hop.
- Re-entry after round-trip uses Tier V on wire-sourced bytes — certificate for re-entry chain begins with replay or validate hop, not Tier S live replace on decoded session cache.
