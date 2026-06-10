# Federation Steady-State Assurance And Convergence Lattice

# Steady-State Invariant Catalog

- Steady-state invariants hold when no promotion unit is in flight and no quarantine posture is active — six equilibrium invariants closed catalog.
- **A1 witness parity** — every propagation and reconciliation witness row imported by adapters matches frozen table revision at vocabulary owner — parametrized contract test passes on every scheduled gate.
- **A2 convergence revision** — all contexts in process import identical vocabulary consensus revision hash — import-linter pin or monorepo single-source guarantees zero skew between sibling nodes.
- **A3 handoff substrate identity** — federated encoder singleton, warm-graph `TypeAdapter`/`Decoder` hashes, and root protocol scope map unchanged since last boot witness commit — E7 grace window never open during steady state.
- **A4 receipt fold closure** — every `StrEnum` kind on shared stream vocabulary has total fold arm on exporter and every receipt-edge importer — exhaustive fold test passes without E8 signal.
- **A5 diagnostic routing fidelity** — S4 smoke injects synthetic faults per stack layer and seam identity — attribution routes to liable owner on first attempt; conflated routing is assurance failure, not reconciliation trigger alone.
- **A6 promotion quiescence** — no partial landing witness gaps on git mainline fan-out set — E6 signal absent; obligated context adapter diffs present for latest vocabulary row.
- Assurance invariant law — each invariant carries `assurance_invariant_id`, `witness_table_ref`, `cadence_gate`, and `default_assurance_row` — undocumented invariant blocks steady-state certification.

# Assurance Decision Lattice

- Verification events classify through a finite chooser lattice — no ad hoc periodic checks without row binding.
- **Boot witness row** — A1–A3 at composition-root warm phase before context activation — failure blocks handoff acceptance; routes to reconciliation E7 or E2 row when skew detected.
- **Pre-merge witness row** — A6 plus propagation fan-out parity on diff set — failure blocks merge; routes to rollback-first.
- **Scheduled replay row** — A1, A2, A4 on re-sync cadence — failure routes to reconciliation re-sync row before quarantine unless grace policy applies.
- **Post-promotion certify row** — full A1–A6 after promotion unit R4 resume and R5 witness commit — certifies equilibrium restored before marking steady state.
- **Health tick row** — lightweight A3 and A5 on root interval during live operation — detects encoder skew or diagnostic regression without full generative replay.
- **Deep assurance row** — system S1–S5 layers on release cadence or federation topology change — executes after scheduled replay passes; static failure blocks expensive suites.
- Lattice law — each row names witness sources, proof layers, failure signal mapping, and reconciliation row escalation — undocumented assurance path is merge blocker.

# Convergence Proof Morphisms

- Convergence proves all federation nodes agree on logical revision — not wall-clock synchronization — structural witness equality across obligated contexts.
- **Vocabulary convergence** — consensus row hash at vocabulary owner equals hash recomputed from each importing adapter's imported row slice — mismatch triggers A2 failure and scheduled re-sync row.
- **Warm-graph convergence** — root registration row lists target class body hash; each context node asserts cached singleton matches — mismatch triggers A3 failure and warm-graph unit.
- **Seam corpus convergence** — foreign fixture corpus revision tag matches seam witness row — metamorphic S3 sample count and tag set parity without full generative run on health tick.
- **Receipt convergence** — fold totality witness on exporter equals importer kind coverage set — mismatch triggers A4 failure and receipt propagation unit.
- **Encoder convergence** — single module-level `Encoder` identity across all contexts; `order="deterministic"` policy row matches production singleton — per-context clone detection triggers encoder_skew and collapse to root singleton.
- Convergence composition — vocabulary convergence is prerequisite for warm-graph and seam convergence replay; receipt convergence may parallelize after vocabulary passes — same phase ordering as R1–R3.

# Assurance Witness And Certification Tables

- Certification tables materialize steady-state obligations beside liable owners — production adapters, composition root, and CI gates read the same frozen rows as propagation and reconciliation witnesses.
- **Steady-state witness row** — `assurance_invariant_id`, `witness_table_ref`, `cadence_gate`, `last_certified_revision`, `obligated_context_ids`, `proof_layers_on_deep_assurance`.
- **Convergence witness row** — `convergence_kind`, `source_revision_hash`, `target_context_id`, `parity_assertion_id`, `escalation_signal_on_failure`.
- **Certification commit row** — `certification_run_id`, `assurance_rows_passed`, `equilibrium_invariants_confirmed`, `steady_state_entry_timestamp`, `next_scheduled_gate`.
- **Health tick row** — `tick_interval_policy`, `lightweight_invariants`, `deep_assurance_deferral`, `failure_escalation_row`.
- Table law — composition root imports certification rows; pytest parametrization derives from same owner; duplicate invariant lists in test files are drift signals.
- Certification table change is federation event — obligated context witness imports update in same commit as revision bump.

# Assurance Cadence And Gate Binding

- Assurance gates bind to lifecycle hooks distinct from reconciliation enforcement — conflating hooks produces false steady-state confidence or duplicate proof spend.
- **Boot gate** — boot witness row A1–A3; runs after warm graph, before first canonical handoff — failure equivalent to boot gate E7 block.
- **Pre-merge gate** — pre-merge witness row A6 plus fan-out obligation set — runs on git diff; independent of runtime reconciliation loop.
- **Post-promotion gate** — post-promotion certify row full A1–A6 — runs once per promotion unit after R5; steady state re-enters only after certify row passes.
- **Scheduled gate** — scheduled replay row A1, A2, A4 — runs on root cadence policy; may overlap boot on cold start but never skips pre-merge on promotion diffs.
- **Health tick gate** — health tick row A3, A5 — runs during live operation; failure escalates to reconciliation quarantine row when tick budget exceeded.
- **Deep assurance gate** — deep assurance row system S1–S5 — runs on release or topology change cadence; scheduled gate failure blocks deep assurance start.
- Gate ordering — boot before handoff; pre-merge before merge; post-promotion before steady-state mark; scheduled independent of handoff volume; health tick continuous; deep assurance last on cadence.

# Change Classification Under Steady State

- Not every edit is a promotion event — steady-state change classification prevents unnecessary federation fan-out while preserving equilibrium when edges are untouched.
- **Interior-only change** — domain transform, fold logic, or port implementation within one context; no vocabulary, seam, wire, or warm-graph target change — assurance limited to context-local proof layers.
- **Witness-neutral change** — comment, diagnostic tag, or trace attribute without semantic effect — no assurance row update; arch rules still apply.
- **Edge-touching change** — any diff on vocabulary owner, seam adapter, wire struct, ingress model, migration fold, or root registration — reclassifies as promotion event; post-promotion certify row mandatory.
- **Topology change** — context node add, split, merge, or seam edge add/remove — federation event; deep assurance row and full S1–S5 mandatory before steady-state re-entry.
- Classification witness row — `change_class`, `assurance_rows_required`, `promotion_unit_required`, `fan_out_context_ids` — arch rule or git diff classifier exports row; misclassification is equilibrium violation when edge drift follows.

# Federation Health Signals

- Health signals are proactive drift precursors — not yet E1–E8 violations — structured vocabulary enables assurance loop action before reconciliation escalation.
- **H1 witness revision lag** — adapter imports witness table N while vocabulary owner publishes N+1 — triggers scheduled replay row before E2 bijection failure.
- **H2 metamorphic sample age** — S3 corpus fixture ids stale relative to seam witness row — triggers seam corpus convergence check on health tick.
- **H3 diagnostic latency regression** — trace span from ingress fault to liable-owner attribution exceeds root SLO row — triggers A5 deep check; not domain logic defect until routing conflation proven.
- **H4 promotion quiescence gap** — mainline lacks post-promotion certify row commit after merged promotion unit — blocks steady-state mark at CI gate.
- **H5 encoder policy drift** — non-deterministic encode observed in metamorphic sample but not production — triggers A3 and encoder convergence replay.
- Signal law — health signals map to assurance rows first; escalation to E-signals occurs when scheduled replay fails or health tick budget exceeded per root policy.

# Composition Root Assurance Loop

- Long-running processes admit a bounded assurance loop at composition root — distinct from reconciliation loop in ; runs during steady state and after certify row entry.
- **Loop trigger** — scheduled gate due, health tick interval, boot completion, or post-promotion certify request — not per-request domain invocation.
- **Loop body** — select assurance row → replay witness parity → run configured proof layers → commit or escalate certification row → schedule next gate.
- **Loop budget** — maximum assurance depth per interval documented at root — health tick stays lightweight; deep assurance deferred to release cadence unless H-signal escalates.
- **Loop idempotency** — repeated certification on unchanged revision produces identical certification commit row hash — no duplicate witness commits.
- **Loop exclusion** — interior domain modules never invoke assurance; boundary adapters and root scheduler own cadence; violations emit tagged faults upward to reconciliation loop when assurance row fails.

# Steady-State Degradation And Read-Only Federation

- Steady-state degradation is explicit root posture — extends degradation postures.
- **Assurance-paused degradation** — deep assurance and scheduled replay deferred; health tick and boot witness remain active — documents maintenance window and resume precondition rows.
- **Vocabulary read-only steady state** — token promotion frozen; A1 and A2 still run on scheduled gate — bijection on declared closure mandatory; extension arms capture unknown foreign tokens.
- **Single-context maintenance** — one node warm-graph refresh while others remain certified — A3 scoped to maintaining node; cross-context handoffs on unaffected edges continue.
- Posture change updates certification table and health tick escalation policy — contexts do not self-declare steady-state degradation.
- Resume from degradation requires post-promotion certify row or scheduled replay pass — same bar as promotion unit completion.

# Assurance Diagnostic Tags

- Assurance faults extend reconciliation tags — structured payloads name assurance invariant, not undifferentiated health text.
- **Tags** — `assurance`, `convergence`, `certification`, `health_signal`, `steady_state`, `witness_lag`, `cadence_missed` — align with observability federation vocabulary; ad hoc strings for enum-owned concepts are drift.
- **Fault payload fields** — `assurance_invariant_id`, `assurance_row`, `witness_revision_expected`, `witness_revision_observed`, `cadence_gate`, `certification_run_id` — spans omit canonical field payloads.
- **Certification blocked fault** — lists failed assurance invariants and escalation reconciliation row — injected in boot and pre-merge smoke before steady-state mark.
- **Health signal fault** — names H-signal id, tick count, and budget remaining before quarantine escalation — attribution targets composition root scheduler, not domain owner.
- Trace spans on assurance runs emit `certification_run_id`, assurance row label, and convergence kinds checked — interior domain spans omit assurance metadata.

# Assurance Collapse Tests

- Collapse tests verify assurance remediation — extend reconciliation collapse tests.
- **Skip certify after promotion** — merge lands promotion unit without post-promotion certify row — collapse by blocking steady-state mark and running full A1–A6.
- **Duplicate witness lists** — test file re-declares token set parallel to vocabulary witness table — collapse to single parametrized owner import.
- **Health tick in domain** — smart constructor or fold invokes periodic bijection check — collapse to root assurance loop and adapter witness import.
- **False steady state** — quarantine active but certification row marks steady — collapse by coupling certify row to quarantine witness clearance.
- **Interior-only mislabel** — edge-touching diff classified interior-only — collapse by reclassifying as promotion event and running fan-out obligation set.
- **Deep assurance before scheduled** — S1–S5 runs while A2 convergence fails — collapse by enforcing gate ordering; scheduled replay must pass first.
- Remediation order — classify change, select assurance row, replay witnesses, certify or escalate — never disable assurance gate to absorb propagation debt.

- - **Design phase** — role-map coverage, kind coherence matrix, and projection lattice binding per concept before federation edges activate — proof layers 1–2.
- **Promote phase** — propagation lattice row, atomic promotion unit, and witness fan-out — equilibrium temporarily suspended until post-promotion certify row passes.
- **Restore phase** — violation signal, reconciliation row, and choreography R0–R5 — steady-state mark revoked until certify row re-commits.
- **Sustain phase** — assurance cadence, convergence replay, and health tick per this document — default operating posture between promotion events.
- Lifecycle law — composition root owns phase transitions; contexts do not self-advance from sustain to promote without edge-touching diff classification; contexts do not skip restore when E-signals fire.

# Audit Trail And Certification History

- Certification history append-only at composition root — each `certification_run_id` records assurance rows passed, revision hashes witnessed, and next scheduled gate — not mutable domain state.
- **Audit row** — `certification_run_id`, `assurance_invariant_ids`, `witness_revision_hashes`, `cadence_gate`, `steady_state_entry`, `promotion_unit_id` when post-promotion, `reconciliation_run_id` when restore preceded certify.
- **History fold** — fold projections derive last-known-good revision per convergence kind, promotion frequency, and mean time between E-signals — read-only; never duplicate canonical state.
- **Retention policy** — audit rows carry schema_version on envelope; migration fold on read path when history format changes — canonical owners remain version-agnostic.
- Cross-process audit export — receipt edge may carry certification snapshot structs when evidence crosses seams — canonical field payloads forbidden in audit export per observability federation.
- Audit replay on cold start — boot gate compares live witness imports against last certification commit row — mismatch triggers scheduled replay before handoff even when no E-signal fired.

# Long-Horizon Sunset Under Steady State

- Schema_version sunset and deprecated seam retirement proceed under steady-state assurance — not ad hoc deletion during active promotion.
- **Sunset witness row** — `version_arm`, `migration_fold_totality_proof_id`, `store_read_path_zero_proof_id`, `sunset_date`, `assurance_rows_on_removal`.
- **Pre-sunset gate** — deep assurance row plus migration fold proves zero active read-path on retiring arm — forward-fix arbitration.
- **Post-sunset certify** — A1 and A5 replay after arm removal from consensus and enforcement catalog — orphan enforcement row without lattice update blocks merge.
- Vocabulary token retirement mirrors version sunset — bijection on declared closure shrinks only after seam corpus and S3 metamorphic prove zero foreign token map to retiring member.
- Steady state during sunset window — vocabulary read-only degradation may apply; assurance-paused degradation forbidden until sunset witness row commits.

# Assurance Versus Reconciliation Boundary

- Assurance and reconciliation loops are complementary — not interchangeable — composition root schedules both with distinct triggers and budgets per assurance-promotion temporal law.
- **Assurance triggers** — cadence due, boot complete, post-reconciliation certify request, health tick — proactive; lawful only when temporal witness `phase` is `assurance_between_promotions`.
- **Reconciliation triggers** — E1–E8 signals, propagation partial landing, quarantine expiry, grace-window breach — reactive; blocks steady-state mark until cleared.
- **Escalation edge** — assurance row failure on A1, A2, or A4 maps to H-signal first; repeated failure or tick budget exceeded maps to corresponding E-signal and reconciliation row — assurance does not patch domain to avoid reconciliation.
- **Shared witness tables** — propagation and reconciliation witnesses– are inputs to assurance rows — certification commits witness revision consumed by next scheduled replay.
- **Proof spend ordering** — health tick and scheduled replay before deep assurance before full S1–S5 on release cadence — same static-before-generative ordering as integration proof architecture.

# Federation Maturity And Readiness Levels

- Readiness levels classify federation graph completeness before production steady-state entry — not runtime quality scores on domain logic.
- **Level L0 — local proof** — single context passes layers 1–6; no federation edges; assurance limited to interior checklist.
- **Level L1 — vocabulary linked** — at least one vocabulary federation edge; A1 and A2 pass on boot witness; S2 bijection active.
- **Level L2 — seam proven** — active seam edges with S3 metamorphic corpus; seam repair row tested via injected remap gap smoke.
- **Level L3 — receipt linked** — receipt edges with A4 fold closure on exporter and importers; E8 enforcement active.
- **Level L4 — certified steady state** — post-promotion certify row commits; all A1–A6 pass; health tick and scheduled gate registered at root.
- **Level L5 — deep assurance current** — system S1–S5 pass on release cadence; audit trail fold projects zero uncleared E-signals in window policy.
- Promotion rule — context may not accept cross-context canonical handoffs above its linked edge maturity — root handoff graph declares minimum level per edge.

# Cross-Context Read Consistency During Steady State

- Steady-state handoffs assume logical convergence — consumers need not re-validate canonical kinds materialized by exporting context when A2 and certify row current.
- **Read path** — importing context receives canonical or fold-derived receipt; rematerialization from wire required only at persistence boundary or trust-posture downgrade.
- **Stale read guard** — when H1 witness revision lag detected, root quiesces handoffs on affected vocabulary edge until scheduled replay passes — not silent serve of pre-lag canonical.
- **Fan-out consistency** — one vocabulary row promotion certifies all fan-out contexts together; partial certify forbidden — same atomicity as promotion unit.
- **Observability** — handoff spans carry `certification_run_id` and `witness_revision_hash` at edge — not canonical payloads; consumers assert revision parity on receipt edge when policy requires.

# Assurance Proof Obligation Routing

- Proof routing during assurance follows propagation and reconciliation liability with assurance-first escalation on H-signals.
- **Boot witness failure on A3** — warm-graph unit at root; domain modules excluded — same liability as E7 without waiting for grace expiry.
- **Scheduled replay failure on A2** — vocabulary consolidate choreography R1 — do not run deep assurance on skewed revision.
- **Health tick failure on A5** — composition root diagnostic tag map and trace decorator placement — attribution conflation implicates root before either domain owner.
- **Deep assurance S3 failure** — seam adapter owner liable; vocabulary row stands when fault is cardinality — certifying row blocked until S3 passes.
- Proof debt from harness suppressions at assurance gates rejected — fix witness table typing on liable owner module per proof liability rule.

# Assurance Completeness Checklist

- Federation assurance is complete when steady-state invariants hold and certification row commits on every cadence gate — checklist runs beside reconciliation completeness.
- Every assurance invariant maps to exactly one assurance row and witness table ref — orphan invariants without certification entry block steady-state mark.
- Every promotion unit merge completes post-promotion certify row before mainline steady-state re-entry — H4 quiescence gap signals are zero.
- Every scheduled gate replays A1, A2, A4 on cadence without skip — cadence_missed diagnostic tags are zero unless assurance-paused degradation documented.
- Every edge-touching change carries promotion unit and fan-out — interior-only misclassification collapse tests pass.
- Every health tick within budget or escalates to reconciliation with documented policy — silent tick disable is equilibrium violation.
- Every assurance fault routes to liable module per diagnostic tags — conflated attribution fails smoke module.
- Collapse tests pass — no skip certify, duplicate witness lists, health tick in domain, false steady state, interior-only mislabel, or deep assurance before scheduled remains.
