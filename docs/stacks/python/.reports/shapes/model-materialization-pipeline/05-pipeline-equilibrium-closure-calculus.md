# Pipeline Equilibrium Closure Calculus

# Closure Row Shape

- Every pipeline equilibrium obligation declares one frozen row: `closure_id`, `invariant_kind`, `primary_table`, `primary_row_id`, `foreign_table_ids`, `foreign_row_ids`, `proof_layers`, `collapse_row_id`, `blocking_merge`.
- `closure_id` is a stable snake slug — `transition_surface_join`, `signal_lattice_join`, `smoke_attribution_join`, `consumer_surface_join`, `codec_singleton_identity` — not ordinal report numbers alone.
- `invariant_kind` draws from closed vocabulary — `{handoff_surface_parity, attribution_transition_parity, smoke_signal_parity, consumer_evolution_parity, singleton_identity, promotion_atomicity, plane_stage_consistency}` — not free-form checklist items.
- `primary_table` names one owner tuple — `TRANSITION_LATTICE`, `ORACLE_CONFORMANCE`, `ATTRIBUTION_LATTICE`, `SMOKE_FEDERATION`, `EVOLUTION_OBLIGATIONS`, `FIELD_POLICY` — closure rows never duplicate row bodies from primary tables.
- `primary_row_id` and `foreign_row_ids` are closed foreign-key slugs — erased references or implicit name equality between stage tags and `transition_id` fail closure admission.
- `proof_layers` stacks orthogonal gates from ingress-to-materialization doctrine — static import architecture, compiled oracle snapshot, lattice contract parametrization, metamorphic round-trip, runtime smoke injection — failures at earliest layer block later layers.
- `collapse_row_id` names repair morphism when closure drifts — undocumented collapse is merge blocker when `blocking_merge=true`.
- `blocking_merge` defaults `true` for production-admitted seams — exemptions require composition-root registry entry citing `closure_id`.

# Federated Registry Topology

- Materialization doctrine admits six machine tables plus one closure join table — `TRANSITION_LATTICE`, `ORACLE_CONFORMANCE`, `FIELD_POLICY`, `ATTRIBUTION_LATTICE`, `SMOKE_FEDERATION`, `EVOLUTION_OBLIGATIONS`, `PIPELINE_CLOSURE` — prose in ingress-to-materialization doctrine through attribution smoke evolution calculus is normative commentary only.
- Registry topology is directed acyclic — edges flow promotion unit → primary table row → foreign table rows → closure row witness; bidirectional repair without `collapse_row_id` indicates duplicate table ownership.
- Composition root owns all tuple symbols — leaf domain modules import canonical owners only; handler-local table fragments or per-verb registry clones are topology violations.
- Table modules colocate beside root codec submodule — moving codec path is version event for `trusted_replay`, `cache_read`, `probe_cache_key`, and `singleton_identity` closure rows even when struct layouts unchanged.
- Import-linter rules enforce registry topology — domain imports of any tuple symbol or closure join helper fail static layer before generative suites.

# Cross-Table Join Catalog

- `transition_surface_join`: every handoff transition contract lattice row with `proof_required=true` on polymorphic slot must cite oracle conformance surface matrices row with matching `concept_owner` and `lattice_transition_ids` — orphan transition without surface oracle is closure failure.
- `transition_surface_join` negative: wire layout documented only on lattice row without `stdout_envelope` or `cache_bytes` surface row — fails `mixed_oracle_surface` and closure row jointly.
- `signal_lattice_join`: every attribution smoke evolution calculus `signal_id` cites handoff transition contract lattice `transition_id` on `lattice_transition_ids` — signals without lattice foreign keys fail attribution closure.
- `signal_lattice_join` negative: proof failure attributed to consumer module — fails `domain_reproof` and `consumer_detail_narrow` closure foreign keys.
- `smoke_signal_join`: every attribution smoke evolution calculus `smoke_id` cites `expected_signal_id` on `ATTRIBUTION_LATTICE` — smoke without attribution linkage fails conflated-injection closure.
- `smoke_signal_join` negative: validation inject surfaces at handler — fails `injected_validation_gate` and `conflated_attribution` jointly.
- `consumer_evolution_join`: every attribution smoke evolution calculus `consumer_id` cites oracle conformance surface matrices `surface_ids` and handoff transition contract lattice `lattice_transition_ids` — standalone consumer prose fails evolution closure.
- `consumer_evolution_join` negative: per-consumer `Decoder()` — fails `consumer_json_loads` on all envelope consumer closure rows.
- `field_policy_surface_join`: every oracle conformance surface matrices row cites `FIELD_POLICY` rows for capped or aliased fields — cap constants in adapters without policy linkage fail `cap_constant_duplication` closure.
- `plane_stage_consistency_join`: every handoff transition contract lattice row plane tags align with static-runtime decode-planes doctrine stage-to-plane mapping — stage name used as plane identifier without `upstream_plane` column fails harness attribution closure.

# Pipeline Equilibrium Invariants

- Seven materialization equilibrium invariants hold simultaneously when pipeline is merge-closed — local bounded-context proof passing while any invariant fails is a system defect.
- **E1 single validation surface** — one compiled validation owner per invariant class per concept — duplicate Pydantic and smart-constructor messages fail `duplicate_validation_surface` closure row.
- **E2 handoff type closure** — every inter-stage artifact is a closed type expression — erased `object` or durable `dict[str, Any]` handoffs fail `stage_skip_dict_to_owner` closure.
- **E3 oracle singularity** — each published external surface binds exactly one primary oracle — mixed OpenAPI-on-wire documentation fails `mixed_oracle_surface` closure.
- **E4 attribution determinism** — every injectable fault routes to one `signal_id` with pinned `owner_symbol` — conflated stage routing fails `conflated_attribution` closure.
- **E5 singleton identity** — module-level `TypeAdapter`, `Encoder`, `Decoder` symbols finalize before parallel importers bind — per-request construction fails `per_request_codec` and `singleton_identity` closure rows.
- **E6 promotion atomicity** — adding handoff, surface, signal, smoke, or consumer row lands in one promotion unit with all foreign keys — partial promotion leaves dangling closure edges.
- **E7 version monotonicity** — unknown `schema_version` fails pass-one without body decode — defaulting to current struct fails `version_oracle_default` and `version_literal_failure` closure rows.
- Restoration order when invariants breach — consolidate field policy, collapse secondary validation owner, repair seam adapter row, refresh oracle snapshot, replay smoke injectors — never add bypass type at interior.

# Promotion Closure Unit

- Promotion closure unit is the smallest merge-atomic change set restoring all six table foreign keys and affected `PIPELINE_CLOSURE` rows — partial units leave equilibrium edges dangling.
- Minimum promotion closure checklist binds simultaneously: `TRANSITION_LATTICE` row, alignment sub-rows, `ORACLE_CONFORMANCE` and `FIELD_POLICY` rows when surface publishes, `ATTRIBUTION_LATTICE` row when stage is injectable, `SMOKE_FEDERATION` row when invariant is smoke-covered, `EVOLUTION_OBLIGATIONS` row when consumer contract publishes, closure join rows, negative fixtures, metamorphic law registration when `proof_required=true`.
- New polymorphic wire slot closure requires detail decoder arm, exhaustive `match`, hypothesis strategy row, surface rows, lattice rows from construction through `detail_proof_validated`, attribution `detail_proof_failure`, smoke `injected_proof_gate`, and closure `transition_surface_join` — partial closure fails registry import and static arm exhaustiveness together.
- Renaming `owner_symbol` without updating all foreign tables and closure rows breaks reference-identity checks — codec submodule moves are version events documented on `singleton_identity` and `trusted_replay` closure rows.
- Promotion unit id pins on composition-root witness row — reconciliation after failed promotion replays closure checklist from failed `proof_layers` gate, not ad hoc leaf fixes.

# Singleton Identity Closure Rows

- `codec_singleton_identity`: primary table `TRANSITION_LATTICE` rows naming `wire_encode`, `_ENVELOPE_DECODER`, `_DETAIL_DECODER`; foreign `SMOKE_FEDERATION` `decoder_symbol`; foreign `ORACLE_CONFORMANCE` `oracle_symbol` — production and conftest alias same module object.
- `codec_singleton_identity` proof: reference-identity smoke on import; metamorphic bytes laws use production encoder only; shadow encoders in test helpers fail closure row.
- `warm_graph_finalize_before_import`: primary static-runtime decode-planes doctrine compiled-plane law; foreign free-threaded and subinterpreter rows from handoff transition contract lattice/oracle conformance surface matrices open proofs — post-import reassignment of singleton targets races under parallel workers.
- `encoder_determinism_closure`: links `cache_bytes` surface, `probe_cache_key` consumer, `deterministic_line_shape` smoke, and `order="deterministic"` lattice metadata — non-deterministic encode breaks cache key and stdout parity closures together.

# Plane-Stage-Registry Consistency

- Stage names from ingress-to-materialization doctrine, plane tags from static-runtime decode-planes doctrine, lattice `transition_id` slugs from handoff transition contract lattice, and surface `projection_family` from oracle conformance surface matrices align through closure rows — not interchangeable identifiers.
- Harness modules join stage-map exhaustiveness to `transition_id` via explicit lookup table — implicit equality between `validation` stage tag and `ingress_carrier_to_validation_exit` fails `plane_stage_consistency_join`.
- Compiled-plane obligations target `TypeAdapter`/`Decoder` singletons and oracle snapshots — static-only proof without compiled row fails closure at compiled layer.
- Runtime-plane obligations target metamorphic laws and smoke injectors — compiled oracle passing without runtime smoke on injectable stages fails closure at runtime layer.

# Metamorphic Closure Federation

- Metamorphic laws register only on root codec symbols named in lattice `owner_symbol`, surface `oracle_symbol`, and closure `codec_singleton_identity` — anonymous `@spec` witnesses fail `law_symbol_registration` closure row.
- Full-pipeline metamorphic closure chains `materialization_exit_to_wire_projection` → `cache_write_after_proof` → `envelope_decode_parity` smoke → `cli_stdout_parser` consumer when bijection holds — subset bijection declares excluded fields on closure row metadata.
- Shrinking preserves discriminant legality across closure tables — invalid tag mutations must fail at construction gate attribution, not converge as accepted counterexamples on any closure proof.
- Fault-path metamorphic closure asserts proof skip explicitly — `_validated` guard matches `Error` and returns unchanged; success and fault laws are separate closure foreign keys.

# Diagnostic And Consumer Closure

- Consumer closure requires `cli_stdout_parser` and siblings decode via `_ENVELOPE_DECODER`, gate `schema_version`/`claim`/`verb`, and honor `proof_policy` — consumer re-proof on success lines fails `domain_reproof` closure.
- Diagnostic distillation closure routes pre-formatted strings to `domain_preformatted_diagnostic` — interior folds emitting transport-prefixed faults fail root-guard closure rows.
- `truncated=True` closure joins envelope surface oracle, `cap_truncation_spill` smoke, and artifact spill keyed by `run_id` — elongated wire tuples without spill paths fail consumer and smoke closures jointly.
- Exit code projection closure documents envelope-field derivation — stderr string matching for exit status fails consumer closure on all admitted CLI hosts.

# Schema Evolution Closure

- Version bump closure requires ordered `migration_stored` surface rows, `version_migration_single_hop` lattice sequence, `migration_read` consumer row, and `version_literal_failure` attribution row — silent `BeforeValidator` patches without oracle update fail evolution closure.
- Cache key drift closure joins `encoder_identity_skew` signal, `probe_cache_key` consumer, and `cache_bytes` surface encoder metadata — deploy without re-key row breaks probe closure independently of handler logic.
- Settings schema evolution closure stays independent of envelope `schema_version` unless root documents explicit coupling column on `settings_bootstrap` and paired surface rows.

# Harness Terminal Order

- Terminal harness executes closure layers in fixed order — registry completeness and import architecture before field-policy exhaustiveness before oracle snapshot diff before lattice parametrization before metamorphic round-trip before smoke injection before consumer contract replay.
- Static closure failure blocks generative suites — same ordering as ingress-to-materialization doctrine stage-attributed proof layers, oracle conformance surface matrices harness binding, and attribution smoke evolution calculus harness execution order.
- CI merge gate requires all `blocking_merge=true` closure rows green before handler integration smoke — closure static layer failure waives no runtime smoke pass.
- Mutation closure targets dense adapter seams referenced by multiple foreign tables — interior domain folds remain secondary mutation surface per ingress-to-materialization CI enforcement gates.

# Closure Witness Replay Tables

- Witness replay tables materialize closure obligations beside root codec modules — production adapters, CI gates, and reconciliation loops read the same frozen rows as primary table tuples.
- **Closure witness row** — `closure_id`, `invariant_kind`, `primary_row_id`, `foreign_row_ids`, `last_verified_promotion_unit`, `proof_layers_cleared`, `equilibrium_invariant_id`.
- **Promotion witness row** — `promotion_unit_id`, `tables_touched`, `closure_rows_added`, `negative_fixtures_registered`, `metamorphic_laws_registered`, `merge_gate_timestamp`.
- **Singleton witness row** — `owner_symbol`, `module_path`, `object_id_hash`, `encoder_order_policy`, `schema_version_pins` — compared on boot and after promotion merge.
- **Resume witness row** — `reconciliation_run_id`, `signals_cleared`, `closure_rows_restored`, `handoff_resume_boundary` — composition root owns row; leaf modules do not self-resume after quarantine.
- Witness replay change is closure event — obligated foreign table diffs land in same commit as witness revision; duplicate closure lists in test files are drift signals.

# Violation Signal To Closure Routing

- Pipeline violation signals from attribution smoke evolution calculus map to equilibrium invariants and default closure repair rows — not undifferentiated exception classes.
- **P1 orphan foreign key** — primary table row lacks required `foreign_row_ids` entry — invariant E6 promotion atomicity; repair `add_closure_join_row` in same promotion unit.
- **P2 mixed oracle drift** — stdout documented via Pydantic schema — invariant E3 oracle singularity; repair `collapse_to_msgspec_inspect_surface`.
- **P3 conflated injection** — smoke inject surfaces at wrong `stage_owner` — invariant E4 attribution determinism; repair `smoke_signal_join` foreign key on `expected_signal_id`.
- **P4 shadow codec** — test helper encoder diverges from production singleton — invariant E5 singleton identity; repair `codec_singleton_identity` witness refresh.
- **P5 partial promotion landing** — lattice row merged without surface row when `proof_required=true` — invariant E6; repair rollback-first per promotion witness row.
- **P6 version default ingress** — unknown `schema_version` maps to current struct — invariant E7 version monotonicity; repair `version_literal_failure` attribution and pass-one oracle row.
- **P7 interior proof duplication** — domain module calls `validate_detail` — invariant E1 single validation surface; repair collapse to root `_validated` closure chain only.
- Signal law — each P-signal carries `equilibrium_invariant_id`, `default_collapse_row_id`, and `proof_layers_to_replay` — undocumented signal blocks merge when `blocking_merge=true`.

# Reconciliation Choreography For Pipeline

- Post-violation restoration executes as typed choreography at composition root — not parallel domain edits across leaf modules.
- **Phase C0 — signal freeze** — pin `promotion_unit_id`, violating P-signal, and obligated closure row set; no new handoffs on affected edges until choreography completes.
- **Phase C1 — table consolidate** — field-policy owner lands rows and fan-out surface diffs before seam or singleton repair; oracle snapshot diff re-runs at compiled layer.
- **Phase C2 — edge repair** — seam adapter, migration fold, or singleton re-registration on liable `owner_symbol` only; domain interiors excluded from C2 edit set.
- **Phase C3 — closure replay** — replay `proof_layers` from failed gate on affected `closure_id` rows; static failure remains in C2; generative suites blocked until C3 static passes.
- **Phase C4 — handoff resume** — composition root clears quarantine; canonical handoffs restart at materialization exit per composition-root handoff resume at materialization exit rule; ingress carriers forbidden at resume boundary.
- **Phase C5 — witness commit** — promotion or reconciliation witness row updates beside root codec module; foreign table importers consume revised rows in same commit as C4 resume.
- Choreography parallelism — C1 and C2 serialize per liability assignment from attribution smoke evolution calculus remediation rows; C3 may parallelize per `closure_id` only after shared vocabulary and singleton nodes pass.

# Enforcement Row Catalog

- Enforcement rows bind CI checks to equilibrium invariants — closure calculus drives parametrized enforcement suite at series terminal.
- **Registry enforcement** — all six primary tuples plus `PIPELINE_CLOSURE` importable from composition root only — failure P1, collapse `collapse_registry_fragments`.
- **Join enforcement** — every `blocking_merge=true` closure row has green foreign-key parametrization — failure P1 or P5, collapse `add_closure_join_row`.
- **Oracle enforcement** — surface snapshot diff hash matches `FIELD_POLICY` projection — failure P2, collapse `field_policy_surface_join`.
- **Attribution enforcement** — injected violations route to `expected_signal_id` on smoke rows — failure P3, collapse `smoke_signal_join`.
- **Singleton enforcement** — `object_id_hash` on witness row matches production import — failure P4, collapse `codec_singleton_identity`.
- **Consumer enforcement** — evolution rows decode shared smoke fixtures without dual parser — failure `dual_parser_logic`, collapse `consumer_evolution_join`.
- **Version enforcement** — pass-one literal closure on envelope oracle — failure P6, collapse `version_literal_failure` remediation chain.
- Enforcement catalog change requires simultaneous closure witness row update — orphan enforcement without `closure_id` blocks merge.

# PIPELINE_CLOSURE Terminal Join

- `PIPELINE_CLOSURE` is the seventh federated table — terminal join owner binding six primary machine tables into one merge-closed equilibrium graph; rows are foreign-key witnesses only, never duplicate bodies from `TRANSITION_LATTICE`, `ORACLE_CONFORMANCE`, `FIELD_POLICY`, `ATTRIBUTION_LATTICE`, `SMOKE_FEDERATION`, or `EVOLUTION_OBLIGATIONS`.
- Terminal join admission requires closed row shape — `closure_id`, `invariant_kind`, `primary_table`, `primary_row_id`, `foreign_table_ids`, `foreign_row_ids`, `proof_layers`, `collapse_row_id`, `blocking_merge` — implicit stage-tag equality without `transition_id` or `signal_id` slugs fails static import.
- Directed acyclic join topology — promotion unit → primary table row → obligated foreign rows → `PIPELINE_CLOSURE` witness; bidirectional repair without `collapse_row_id` indicates duplicate table ownership and blocks merge.
- Composition root is sole importer of `PIPELINE_CLOSURE` and all six primary tuples — leaf registry fragments, README join tables, and parallel prose checklists collapse to this terminal owner.
- Harness terminal order on closure layers — registry completeness and import architecture → field-policy exhaustiveness → oracle snapshot diff → lattice parametrization → metamorphic round-trip → smoke injection → consumer contract replay — static closure failure blocks generative suites at the same gate ordering as ingress-to-materialization doctrine stage-attributed proof layers.
- Cross-table join catalog rows (`transition_surface_join`, `signal_lattice_join`, `smoke_signal_join`, `consumer_evolution_join`, `field_policy_surface_join`, `plane_stage_consistency_join`, `codec_singleton_identity`, `encoder_determinism_closure`) are mandatory when `blocking_merge=true` on the admitted seam — partial join landing is P5 partial-promotion signal under invariant E6.
- Dual-engine triple closure on cross-family concepts — `transition_surface_join`, `field_policy_surface_join`, and `pydantic_validated_to_msgspec_wire` lattice foreign key — missing leg fails E3 oracle singularity and E6 promotion atomicity together.
- Terminal join change is equilibrium event — witness replay tables (`closure`, `promotion`, `singleton`, `resume`) update beside root codec module in the same commit as obligated primary and foreign table diffs.

# Root Guard Closure Chain

- Composition-root guard sequence from ingress-to-materialization doctrine admits terminal closure chain linking lattice, surface, attribution, smoke, and consumer foreign keys in execution order — each hop materializes one `PIPELINE_CLOSURE` row when `blocking_merge=true`.
- `registry_dispatch_terminal` → `params_bind_to_bound_or_fault` → `ingress_carrier_to_validation_exit` → `handler_exec_to_result_report` → `strict_policy_promotion` → `detail_proof_validated` → `emit_fold_to_envelope` → `encode_to_stdout_bytes` → `serialization_exit_to_consumer_decode` → `cli_stdout_parser` — each hop has one `closure_id` join when `blocking_merge=true`.
- `_guard` closure wraps entire chain — uncaptured faults fail `guard_capture_thunk` and `smoke_signal_join` on promotion or beartype injectors.
- One-write invariant closure links `encode_to_stdout_bytes`, `invariant_doubler` consumer, and `one_write_stdout` smoke — second emit fails chain before consumer replay.
- History parity closure links `cache_write_after_proof`, `envelope_decode_parity` smoke, and `history_replay` consumer — dual-encoding breaks chain at `encoder_determinism_closure`.
- Fold terminal closure links `fold` materialization from ingress-to-materialization doctrine, `emit_fold_to_envelope`, and cap spill smoke — truncation without artifact spill fails `truncated=True` closure join.

# Runtime Closure Loop

- Long-running processes admit bounded closure reconciliation loop at composition root — not per-request repair in domain transforms.
- **Loop trigger** — P-signal accumulator on boundary faults tagged `closure`, `equilibrium`, or `staleness`; promotion witness drift; scheduled singleton witness replay on boot.
- **Loop body** — classify P-signal → select `collapse_row_id` → execute choreography C0–C5 → assert C3 proof replay → commit or rollback witness row.
- **Loop budget** — maximum reconciliation attempts per `closure_id` per interval documented at root — exceeded budget escalates to edge quarantine and operator alert.
- **Loop exclusion** — hot ingress and egress paths assume equilibrium unless signal fires — closure loop is not inline validation substitute.
- Free-threaded and parallel test workers require singleton witness compare before loop body mutates registry tuples — post-import reassignment races fail P4 before C2 begins.

# Proof Debt Closure Ledger

- Proof debt from checker gaps declares rows beside `PIPELINE_CLOSURE` — harness suppressions and `cast` escapes at materialization seams are rejected, not debt.
- **Debt row shape** — `debt_id`, `blocking_closure_ids`, `owner_module`, `sunset_criterion`, `linked_p_signals`.
- **Callable discriminator unification** — blocks `openapi_ingress` closure join until single generative export; sunset tied to root OpenAPI promotion record.
- **Nested patch one-expression seam** — blocks `patch_dict_to_successor_owner` full closure until replacement axis admits `replace_validated`.
- **Worker boot singleton parity** — blocks `subprocess_child_fold` and `codec_singleton_identity` full closure until forked-importer CI gate lands.
- **Law symbol registration** — blocks metamorphic closure federation until named symbols replace anonymous `@spec` witnesses.
- Debt row open while closure import green — targets mis-tagged `blocking_closure_ids`; static debt cannot be waived by runtime smoke alone.

# Negative Fixture Federation

- `orphan_closure_foreign_key`: primary table row without required foreign row — fails registry import at static layer.
- `dual_source_prose_table`: module docstring transition list diverging from `TRANSITION_LATTICE` while closure row absent — fails promotion atomicity closure.
- `shadow_codec_closure`: test helper encoder disagrees with production singleton — fails `codec_singleton_identity` and metamorphic closure jointly.
- `partial_promotion_closure`: lattice row lands without matching surface row when `proof_required=true` — fails `transition_surface_join` at merge gate.
- `conflated_closure_attribution`: smoke green while attribution foreign key missing — fails `smoke_signal_join` before consumer replay.

# Failure Archaeology On Closure Defects

- Closure import fails at static layer — attribution targets missing foreign key on primary row, not runtime decode fault.
- Oracle snapshot diff passes but closure fails — targets `field_policy_surface_join` or dual-mode serializer sub-row drift.
- Smoke passes, consumer fails, closure green — targets wrong `consumer_fixture_ref` or version gate field omission on evolution row.
- Metamorphic green, closure fails singleton row — targets shadow encoder or post-import singleton reassignment.
- All tables green, equilibrium still drifts in production — targets undocumented handoff bypassing closure row — add `closure_id` or root exemption registry entry.

# Collapse Signals On Closure Drift

- Parallel prose completion checklists beside `PIPELINE_CLOSURE` — collapse to closure tuple owner.
- Per-package registry fragments — collapse to composition-root federated topology.
- Hand-maintained join tables in README — collapse to `PIPELINE_CLOSURE` foreign keys.
- Interior proof or encode duplicated from root guards — collapse to single egress closure chain per ingress-to-materialization collapse tests.
- Consumer parsers beside shared smoke fixtures — collapse to `consumer_evolution_join` and `_ENVELOPE_DECODER` closure rows.

# Dual-Engine Triple Closure

- Cross-family concepts require minimum closure triple — `transition_surface_join` on lattice and surface rows, `field_policy_surface_join` on divergence columns, `pydantic_validated_to_msgspec_wire` lattice foreign key on projection row — missing leg fails E3 and E6 together.
- Positive closure fixture: one-expression `msgspec.convert` with field-policy predicted rename and omit outcomes; metamorphic law asserts bytes after encode through `codec_singleton_identity` encoder.
- Negative closure fixture: `model_dump` → dict surgery → `Struct(**d)` — fails cross-family lattice row, projection conformance surface row, and dual-engine triple closure jointly.
- OpenAPI-only documentation for wire layout fails triple closure even when lattice handoff row exists — `mixed_oracle_surface` and P2 signal fire at static layer.
- Domain interior types receive no closure rows — rich owners expose snapshots through adapter projection; closure applies only to published surfaces and admitted handoffs.
