# Federation Equilibrium Reconciliation Lattice

# Assurance-Promotion Temporal Law

- Federation admits three non-overlapping temporal phases — **promotion in-flight**, **reconciliation in-flight**, and **assurance-between-promotions** — composition root owns phase transitions; contexts do not self-advance or self-certify across phase boundaries.
- **Promotion in-flight wins** — when a propagation promotion unit is open (`promotion_unit_id` pinned by change classifier, partial landing witness gap, or edge quarantine on promotion diff), promotion obligations : vocabulary fan-out, seam corpus append, warm-graph rebind, and receipt fold updates must complete before steady-state invariants bind as certification truth; assurance must not widen ingress posture, hand-edit consensus rows, or patch seam adapters to absorb partial evolution — E6 partial landing is merge blocker, not a cue to relax scheduled replay.
- **Reconciliation in-flight wins** — when an equilibrium violation signal E1–E8 fires, quarantine posture is active, or choreography phases R0–R5 are incomplete, reconciliation obligations from this document take precedence over assurance certify and steady-state mark: restoration morphisms, arbitration rows, and proof replay R3 must complete before post-promotion certify row; assurance must not patch domain interiors or bypass DTO at consuming context to clear E-signals.
- **Assurance between promotions** — when no promotion unit is in flight, no reconciliation run is active, and no quarantine posture blocks affected edges, sustain phase holds: A1–A6; scheduled replay, health tick, and deep assurance run on cadence; witness revision parity and convergence morphisms assert logical revision stability across obligated contexts — not wall-clock synchronization.
- **Pre-merge is promotion-adjacent, not sustain** — pre-merge enforcement E2, E6, and witness table parity on git diff runs via change classifier before merge lands; it classifies edge-touching diffs and blocks partial landing — it does not substitute for post-promotion certify and does not discharge sustain-phase scheduled replay when no promotion diff is present.
- **Post-reconciliation certify bridges phases** — post-promotion certify row full A1–A6 runs once per committed promotion unit after R5 witness commit when promotion triggered reconciliation, or after standalone reconciliation run when violation fired during sustain; steady-state mark re-enters only after certify passes — equilibrium suspended from first edge-touching edit or E-signal through certify completion.
- **Assurance deferral under in-flight promotion or reconciliation** — scheduled replay, deep assurance, and convergence replay on `obligated_context_ids` defer when promotion unit open or reconciliation run active on those contexts; boot witness A1–A3 and health tick A3, A5 on unaffected routes may continue when root policy documents scoped degradation — assurance-paused degradation never defers pre-merge on promotion diffs, R3 proof replay on liable edge, or post-reconciliation certify on the landing unit.
- **Substrate freeze law** — vocabulary consensus rows, seam remap tables, warm-graph registration targets, receipt fold arms, and migration fold owners specialize once per logical revision; promotion in-flight and reconciliation R1–R2 are the only lawful windows for row, adapter, and warm-graph mutation on federation edges — assurance between promotions treats those artifacts as read-only witnesses except via classified edge-touching promotion per evolution coupling.
- **Proof spend ordering** — static S1–S2 before contract S2 before metamorphic S3 before runtime S4 before mutation S6 per integration proof architecture; reconciliation R3 replays from failed layer only after R1 vocabulary consolidate and R2 edge repair complete when signals require — deep assurance during open promotion or reconciliation on affected contexts is collapse defect.
- Temporal witness row — `phase` (`promotion_in_flight` | `reconciliation_in_flight` | `assurance_between_promotions`), `promotion_unit_id`, `reconciliation_run_id`, `quarantine_posture`, `deferred_assurance_rows`, `certify_pending` — composition root exports row; false steady state when phase is not `assurance_between_promotions` but certification row marks steady is equilibrium violation.

# Violation Signal Taxonomy

- **Static reconciliation** — import architecture, kind coherence matrix, and role-map coverage failures before runtime handoff — blocks context activation at composition root.
- **Dynamic reconciliation** — seam remap gap, warm-graph skew, fold lag, or encoder identity mismatch during live handoff — routes through liable edge module without domain interior mutation until edge proof passes.
- **Temporal reconciliation** — trusted-replay pin expiry, schema_version sunset, or witness table revision drift — composition root schedules re-sync sweep before accepting new canonical input on affected edges.
- Reconciliation graph must remain acyclic — bidirectional repair edges between contexts indicate missing vocabulary federation or duplicate seam ownership unresolved by propagation rollback.

- Violation signals classify through a closed vocabulary — not free-form error strings — each signal maps to one equilibrium invariant and one default reconciliation row.
- **E1 single-owner breach** — duplicate validation surfaces on same invariant across federation edges; diagnostic names both contexts and both enforcing modules.
- **E2 vocabulary skew** — orphan token, collision row, or import revision mismatch on consensus table; bijection gate or import-linter hash pin fails.
- **E3 kind degradation** — interior transform accepts ingress or wire kind without adapter rematerialization; kind coherence matrix violation.
- **E4 rail double-collapse** — `Result`/`Option` collapsed twice on same concept across seam and interior; rail collapse once invariant breached.
- **E5 version non-monotonicity** — write-path emits obsolete layout or read-path default arm on unknown stored tag; migration fold totality failure.
- **E6 propagation partial landing** — fan-out context lacks witness row update after promotion unit merge; obligated context list in fault payload.
- **E7 warm-graph stale** — `TypeAdapter` or `Decoder` singleton predates promoted target class body; encode/decode identity mismatch at metamorphic gate.
- **E8 receipt fold lag** — exporter added kind without importer fold arm on shared stream vocabulary; exhaustive fold test fails on receipt edge only.
- Signal law — each signal carries `equilibrium_invariant_id`, `liable_owner_module`, `default_reconciliation_row`, and `proof_layers_to_replay` — undocumented signal kind blocks merge.

# Reconciliation Decision Lattice

- Restoration events classify through a finite chooser lattice — no ad hoc multi-context patches without row binding.
- **Rollback-first row** — E2, E6, E7 on active promotion unit — full atomic unit revert; no forward-fix on consuming context until vocabulary or warm-graph unit completes.
- **Seam repair row** — E3 or E5 at foreign pair only — remap table, migration fold, or cardinality rule patch on seam adapter; vocabulary row stands when fault is layout not token.
- **Thin projection row** — E1 duplicate validation — collapse secondary owner to projection mirror in same promotion unit; canonical owner retains rule.
- **Quarantine row** — E4 or repeated E3 on same edge within grace window — composition root isolates context node; handoffs route to tagged `EquilibriumFault` without partial canonical emission.
- **Re-sync row** — E8 or temporal witness drift — receipt propagation unit or consensus revision sweep across receipt-edge importers; no bypass DTO at consumer.
- **Forward-fix row** — permitted only when rollback unit would orphan production stores — composition root documents forward-fix exemption with migration fold proof and sunset date; default remains rollback-first.
- Lattice law — each row names liable owner, affected context ids, restoration morphism sequence, and equilibrium invariant restored — undocumented restoration path is merge blocker.

# Restoration Choreography Graph

- Multi-context restoration executes as a typed choreography — not parallel domain edits across contexts.
- **Phase R0 — signal freeze** — composition root pins promotion unit id, violating signal, and obligated context set; no new handoffs on affected edges until choreography completes or quarantine posture declared.
- **Phase R1 — vocabulary consolidate** — when E2 or E6 — vocabulary owner lands consensus row and fan-out adapter diffs before any seam or warm-graph repair; bijection gate re-runs at S2.
- **Phase R2 — edge repair** — seam adapter, migration fold, or warm-graph registration patch on liable module only; domain interiors in all contexts excluded from R2 edit set.
- **Phase R3 — proof replay** — system S1–S5 layers re-run in order from failed layer; static failure blocks generative replay; attribution stays on edge until R3 passes.
- **Phase R4 — handoff resume** — composition root clears quarantine; canonical handoffs restart at local canonical per chain restart rule; ingress carriers forbidden at resume boundary.
- **Phase R5 — witness commit** — rollback witness row or reconciliation witness row updates beside liable owner; fan-out contexts import revised row in same commit as R4 resume.
- Choreography parallelism — R1 and R2 serialize per liability assignment; R3 may parallelize per context node only after shared vocabulary and warm-graph nodes pass.

# Forward-Fix Versus Rollback Arbitration

- Arbitration table resolves when rollback and forward-fix both appear viable — default favors rollback-first per propagation atomicity.
- **Store dependency test** — forward-fix permitted when migration fold proves active read-path on obsolete version in production stores and rollback would orphan bytes — liable owner documents store key set and sunset date.
- **Handoff graph test** — forward-fix permitted when removing context node would break acyclic federation proof and retirement migration fold is total — deprecated seam archives with `assert_never` witnesses.
- **Bijection integrity test** — forward-fix forbidden when E2 collision or orphan token — vocabulary consolidate is sole legal path; no seam remap workaround.
- **Dual-owner test** — forward-fix forbidden on E1 — thin projection row mandatory; adding third parallel type at consumer is equilibrium violation.
- Arbitration witness row — `arbitration_id`, signals considered, chosen row, exemption document ref, proof layers waived — composition root owns row; contexts do not self-arbitrate.

# Quarantine And Isolation Postures

- Quarantine is explicit composition-root posture — not silent request dropping at seam without diagnostic tag.
- **Edge quarantine** — single seam adapter or vocabulary edge isolated; other federation edges remain full federation posture; handoffs on quarantined edge return `EquilibriumFault` with signal and liable owner.
- **Context quarantine** — entire context node suspended; upstream contexts may still emit canonical handoffs; downstream consumers route to tagged fault until R3 completes.
- **Vocabulary freeze quarantine** — token promotion halted; existing consensus rows remain bijective; extension arms capture unknown foreign tokens per semi-closed policy.
- **Grace window** — documented interval where transient E7 warm-graph skew self-heals via root re-registration without quarantine — grace expiry without repair escalates to edge quarantine.
- Posture change updates root configuration record, S4 diagnostic routing, and trace span attributes — contexts do not self-declare quarantine; observability tags `quarantine`, `edge_id`, `signal`, `equilibrium_invariant_id`.

# Cross-Context Re-Sync Protocol

- Re-sync restores logical consensus without requiring distributed clocks — structural witness replay across obligated contexts.
- **Consensus replay** — vocabulary owner exports frozen witness table revision; each importing adapter asserts row parity via parametrized contract test — mismatch triggers E2 and R1 choreography.
- **Warm-graph replay** — composition root re-registers singleton targets; each context node asserts `TypeAdapter`/`Decoder` identity hash against warm-graph witness row — mismatch triggers E7 and R2 warm-graph unit.
- **Receipt replay** — exporter publishes fold totality witness; importers on receipt edge assert kind coverage — mismatch triggers E8 and re-sync row.
- **Encoder replay** — federated singleton `order="deterministic"` identity compared across contexts in process — per-context encoder clone detected triggers encoder_skew signal and collapse to root singleton.
- Re-sync cadence — on boot, after promotion unit merge, on grace-window expiry, and on injected partial-landing smoke — not on every handoff; handoff hot path assumes equilibrium unless signal fires.

# Equilibrium Witness Replay Tables

- Witness replay tables materialize reconciliation obligations beside liable owners — production adapters and CI gates read the same frozen rows as propagation witnesses.
- **Reconciliation witness row** — `signal_id`, `equilibrium_invariant_id`, `default_reconciliation_row`, `choreography_phases`, `liable_owner_module`, `obligated_context_ids`, `proof_layers_to_replay`.
- **Arbitration witness row** — `arbitration_id`, `rollback_eligible`, `forward_fix_eligible`, `exemption_document_ref`, `store_dependency_proof_id`.
- **Quarantine witness row** — `posture_id`, `isolated_edge_or_context`, `grace_expiry_policy`, `resume_precondition_layers`.
- **Resume witness row** — `promotion_unit_id` or `reconciliation_run_id`, `signals_cleared`, `equilibrium_invariants_restored`, `handoff_resume_boundary`.
- **Temporal witness row** — `phase`, `promotion_unit_id`, `reconciliation_run_id`, `quarantine_posture`, `deferred_assurance_rows`, `certify_pending` — composition root exports row per assurance-promotion temporal law; adapters read phase before handoff acceptance.
- Table law — adapters import witness rows; pytest parametrization derives from same owner; duplicate signal lists in test files are drift signals.
- Witness replay change is federation event — obligated context adapter diffs land in same commit as reconciliation witness revision.

# Enforcement Row Catalog

- Enforcement rows bind runtime and CI checks to equilibrium invariants — chooser lattice from reconciliation decision lattice drives parametrized enforcement suite.
- **Import enforcement** — S1 acyclic federation graph; cross-context domain interior import forbidden; vocabulary federation edges typed — failure signal `federation_import`, row rollback-first or quarantine by severity.
- **Bijection enforcement** — S2 consensus row collision and orphan tests at vocabulary owner — failure signal E2, row R1 vocabulary consolidate.
- **Kind enforcement** — static assignability and import-linter kind coherence matrix — failure signal E3, row seam repair or quarantine.
- **Propagation enforcement** — partial landing detector on git diff fan-out obligation set — failure signal E6, row rollback-first.
- **Warm-graph enforcement** — singleton identity hash versus promoted class body — failure signal E7, row R2 warm-graph unit.
- **Receipt enforcement** — fold totality on exporter and importers — failure signal E8, row re-sync.
- **Diagnostic enforcement** — S4 smoke injects synthetic violations per signal — conflated attribution fails before merge; each signal must route to liable owner in fault payload.
- Enforcement catalog change requires simultaneous reconciliation witness row update — orphan enforcement without lattice row blocks merge.

# Reconciliation Diagnostic Tags

- Reconciliation faults extend propagation diagnostic tags — structured payloads name equilibrium invariant, not undifferentiated validation text.
- **Tags** — `reconciliation`, `equilibrium`, `quarantine`, `choreography`, `arbitration`, `resume_blocked`, `grace_expired`, `temporal_phase` — align with observability federation vocabulary; ad hoc strings for enum-owned concepts are drift.
- **Fault payload fields** — `signal_id`, `equilibrium_invariant_id`, `reconciliation_row`, `choreography_phase`, `liable_owner_module`, `obligated_context_ids`, `promotion_unit_id`, `reconciliation_run_id`, `temporal_phase` — seam spans omit canonical field payloads.
- **Quarantine fault** — names isolated edge or context id, grace policy id, and resume precondition layers — attribution targets composition root posture record, not domain owner.
- **Resume blocked fault** — lists uncleared signals and failed proof layers — injected in S4 smoke to assert routing before handoff resume commits.
- Trace spans on choreography phases emit `reconciliation_run_id`, phase label, and equilibrium invariant at risk — interior domain spans omit reconciliation metadata.

# Reconciliation Proof Obligation Routing

- Proof replay after restoration follows propagation-aware routing.
- **R3 static failure** — remain in R2 edge repair; domain modules excluded; fix liable owner typing or adapter binding.
- **R3 S2 failure after R1** — vocabulary consolidate incomplete; expand fan-out adapter diffs; do not proceed to seam repair.
- **R3 S3 failure after seam repair** — remap table or corpus incomplete; vocabulary row may stand; rollback seam unit if corpus append missing.
- **R3 S4 failure after resume** — composition root handoff wiring or diagnostic tags; attribution conflation implicates root span attributes.
- **R3 mutation failure on edge** — dense seam logic requires kill ratio on remap `match` arms — interior domain mutation surfaces secondary.
- Proof debt from harness suppressions at reconciliation seams rejected — fix owner typing on liable edge module.

# Runtime Reconciliation Loop

- Long-running processes admit a bounded reconciliation loop at composition root — not per-request ad hoc repair in domain transforms; distinct from assurance loop in per assurance-promotion temporal law.
- **Loop trigger** — signal accumulator on boundary faults tagged `equilibrium`, `propagation`, or `staleness`; grace-window expiry; scheduled re-sync cadence; temporal witness `phase` transition to `reconciliation_in_flight`.
- **Loop body** — read temporal witness row → set `phase` to `reconciliation_in_flight` → classify signal → select reconciliation row → execute choreography phases R0–R5 → assert proof replay → commit or rollback witness → set `certify_pending` when R5 completes.
- **Loop budget** — maximum reconciliation attempts per edge per interval documented at root — exceeded budget escalates to context quarantine and operator alert.
- **Loop idempotency** — repeated identical signals on same edge without intervening promotion apply same row; no duplicate thin projection or seam patch layers.
- **Loop exclusion** — interior domain `@effect.result` generators never invoke reconciliation; boundary adapters emit tagged faults upward to root loop.

# Degraded Equilibrium Grace Windows

- Grace windows permit temporary posture mismatch while restoration choreographs — not indefinite validation downgrade.
- **Warm-graph grace** — brief window after promotion where root re-registration in flight; E7 signals within window queue for R2 without quarantine.
- **Receipt grace** — exporter lands kind before importers when receipt unit commit is atomic but importer activation lags one boot phase — importers quiesce handoffs until re-sync row completes.
- **Vocabulary grace** — read-only degradation — unknown foreign tokens route to extension arm; bijection on declared closure remains mandatory.
- Grace expiry without R3 pass escalates to quarantine row — silent extended grace is equilibrium violation.
- Grace policy rows live in composition root configuration record beside degradation postures.

# Reconciliation Collapse Tests

- Collapse tests verify reconciliation remediation — extend propagation collapse tests.
- **Hotfix bypass** — consuming context patches domain constructor to absorb seam skew — collapse to seam repair row and R2 edge repair.
- **Cherry-pick promotion** — single projection edge lands without atomic unit — collapse to rollback-first row and full unit revert.
- **Dual reconciliation** — two contexts independently repair same vocabulary token — collapse to vocabulary owner sole liability and R1 consolidate.
- **Resume at ingress** — handoff resumes with foreign carrier after quarantine clear — collapse to chain restart at local canonical.
- **Proof skip** — R4 resume without R3 replay — collapse by blocking resume witness commit.
- **Grace permanent** — warm-graph skew tolerated beyond documented window — collapse to edge quarantine and E7 enforcement.
- **Assurance during reconciliation** — scheduled replay or deep assurance mutates federation witness while `reconciliation_run_id` open on affected contexts — collapse by deferring sustain gates until R5 and certify or enforcing R2 edge repair only on liable module.
- **False steady state** — quarantine active or reconciliation run open but certification row marks steady — collapse by coupling certify row to temporal witness `phase` and quarantine clearance from quarantine witness row.
- Remediation order — signal classify, lattice row select, choreography phases, proof replay, witness commit — never add third parallel type at consumer.

# Enforcement Timing And CI Gate Binding

- Reconciliation enforcement executes at distinct lifecycle hooks — conflating hooks produces false equilibrium confidence; temporal phase from assurance-promotion temporal law determines which gates are lawful at each hook.
- **Pre-merge gate** — promotion-adjacent; E2, E6, propagation partial landing, and witness table parity run on git diff fan-out set — failure blocks merge before runtime activation; does not discharge sustain-phase scheduled replay when diff set empty.
- **Boot gate** — E7 warm-graph replay, encoder identity parity, and consensus revision check run at composition-root warm phase before context activation — failure blocks handoff acceptance.
- **Post-promotion gate** — full R0–R5 choreography witness commit required before promotion unit marks complete — R4 resume without R3 replay fails boot gate on next process start; `certify_pending` on temporal witness row blocks steady-state mark until post-reconciliation certify.
- **Runtime gate** — signal accumulator triggers bounded reconciliation loop — grace-window signals queue; expiry without R3 pass escalates to quarantine at runtime gate.
- **Scheduled gate** — re-sync cadence replays consensus, warm-graph, and receipt witnesses — independent of handoff volume; skipped scheduled gate is staleness drift.
- Gate ordering — pre-merge before boot before runtime; scheduled gate may overlap boot but never skips pre-merge enforcement on promotion diffs.

# Reconciliation Versus Assurance Temporal Boundary

- Reconciliation and assurance loops are complementary — composition root schedules both with distinct triggers, phases, and proof budgets per assurance-promotion temporal law.
- **Reconciliation triggers** — E1–E8 signals, propagation partial landing, quarantine expiry, grace-window breach, temporal reconciliation cadence — reactive; sets `phase` to `reconciliation_in_flight`; blocks steady-state mark until R5 and certify complete.
- **Assurance triggers** — cadence due, boot complete, post-reconciliation certify request, health tick — proactive; lawful only in `assurance_between_promotions` phase on non-quarantined edges; may run when zero E-signals active and no promotion unit in flight on obligated contexts.
- **Escalation edge** — H-signals; repeated failure, grace expiry, or tick budget exceeded maps to corresponding E-signal and reconciliation row — assurance does not patch seam or vocabulary to avoid reconciliation.
- **Shared witness tables** — propagation witnesses are inputs to assurance rows in — certification commits logical revision consumed by next scheduled replay; promotion unit lands witness mutations atomically before reconcile or certify witnesses them.
- **Phase transition law** — promote opens `promotion_in_flight`; E-signal or partial landing during promote extends through reconcile to certify; sustain holds only when temporal witness `phase` is `assurance_between_promotions` and quarantine witness cleared.

# Reconciliation Completeness Checklist

- Federation reconciliation is complete when equilibrium invariants run — checklist runs at merge gate beside propagation completeness.
- Every violation signal maps to exactly one reconciliation row and enforcement catalog entry — orphan signals without witness row block merge.
- Every restoration run completes R0–R5 or documents quarantine posture with resume preconditions — incomplete choreography blocks handoff resume.
- Every forward-fix exemption carries arbitration witness row, store dependency proof, and sunset date — default rollback-first preserved in catalog.
- Every quarantine posture updates root configuration and S4 diagnostic routing — contexts do not self-declare isolation.
- Every reconciliation fault routes to liable edge module per diagnostic tags — conflated attribution fails smoke module.
- Temporal witness row `phase` is `assurance_between_promotions` when certification row marks steady — false steady state with open `promotion_unit_id`, `reconciliation_run_id`, or uncleared quarantine blocks merge.
- Collapse tests pass — no hotfix bypass, cherry-pick promotion, dual reconciliation, resume at ingress, proof skip, permanent grace, or assurance during `reconciliation_in_flight` on affected contexts remains.
