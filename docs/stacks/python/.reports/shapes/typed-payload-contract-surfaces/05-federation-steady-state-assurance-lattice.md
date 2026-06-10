# Federation Steady-State Assurance Lattice

# Assurance-Promotion Temporal Law

- Payload federation admits two non-overlapping temporal phases — **promotion in-flight** and **assurance-between-promotions** — composition root owns phase transitions; contexts do not self-advance or self-certify across phase boundaries.
- **Promotion in-flight wins** — when a payload promotion unit is open (`promotion_unit_id` pinned by change classifier, restoration S0–S5, or edge quarantine), promotion obligations take precedence over sustain-phase assurance: partial row landing, gate expression edits, chain slug retargets, and fan-out witness commits must complete before steady-state invariants bind as certification truth; assurance must not widen `closed`/`extra_items` posture, hand-edit emitted rows, or patch adapter field maps to absorb partial evolution — A6 partial landing is merge blocker, not a cue to relax scheduled replay.
- **Assurance between promotions** — when no promotion unit is in flight and no quarantine posture is active on affected ingress routes, sustain phase holds: P1–P6 and A1–A6 are binding steady-state claims; scheduled replay, health tick, and deep assurance run on cadence; P5 interior payload quiescence and P2 row-oracle parity assert logical revision stability across obligated contexts — not wall-clock synchronization.
- **Pre-merge is promotion-adjacent, not sustain** — pre-merge witness row P6 and A6 runs on git diff via change classifier before merge lands; it classifies edge-touching diffs and blocks partial landing — it does not substitute for post-promotion certify and does not discharge sustain-phase scheduled replay when no promotion diff is present.
- **Post-promotion certify bridges phases** — post-promotion certify row full P1–P6 and A1–A6 runs once per committed promotion unit after restoration S5 witness commit; steady-state mark re-enters only after certify passes — equilibrium suspended from first edge-touching edit through certify completion.
- **Assurance deferral under in-flight promotion** — scheduled replay, deep assurance, and convergence replay on `obligated_context_ids` defer when promotion unit open on those contexts; boot witness P1, P3, A3 and health tick A3, A5 on unaffected routes may continue when root policy documents scoped degradation — assurance-paused degradation never defers pre-merge on promotion diffs or post-promotion certify on the landing unit.
- **Substrate freeze law** — PEP 728 openness and PEP 655 requiredness on boundary payloads, `annotationlib` reader fold output, and root-level `TypeAdapter` warm-up from Pydantic CoreSchema specialize once per logical revision; promotion in-flight is the only lawful window for row, schema snapshot, gate `match`, and adapter specialization mutation — assurance between promotions treats those artifacts as read-only witnesses except via classified edge-touching promotion.
- **Package posture** — `typing` 3.15 spec (PEP 728 `closed`/`extra_items`, PEP 655 `Required`/`NotRequired`, PEP 705 `ReadOnly`, PEP 692 `Unpack`) defines static contract revision; `annotationlib.Format.FORWARDREF` reader emission and `TypeAdapter(Payload).json_schema()` oracle are runtime/schema witnesses pinned at certify commit; pyright, mypy, and ty matrix modules share frozen `ci_gate_ids` revision per P6 — checker drift during open promotion unit attributes to incomplete landing, not to sustain-phase oracle skew alone.
- Temporal witness row — `phase` (`promotion_in_flight` | `assurance_between_promotions`), `promotion_unit_id`, `quarantine_posture`, `deferred_assurance_rows`, `certify_pending` — composition root exports row; false steady state when phase is `promotion_in_flight` but certification row marks steady is equilibrium violation.

# Steady-State Invariant Catalog

- Steady-state invariants hold only in **assurance-between-promotions** phase — when no payload promotion unit is in flight and no quarantine posture is active on an affected ingress route — six doctrine invariants plus six assurance-only invariants form the closed catalog.
- **P1 triage row parity** — every concept-stage slot at each composition root binds `{concept, stage, artifact_class, owner_module}` — undocumented triage rows fail arch import and steady-state certification.
- **P2 row authority parity** — every active `owner_id` and concrete specialization in emission registry matches reader fold output and `TypeAdapter.json_schema()` oracle — divergence blocks assurance before chain runtime.
- **P3 gate expression pin** — every ingress route references one root admission binding — split validate-then-promote without documented sub-row refs is incomplete integration surface.
- **P4 chain catalog completeness** — every active ingress and egress carrier admits frozen `chain_id` row with `emitted_row_ref`, `gate_expression_ref`, and `chain_slugs` — routes without catalog rows fail harness discovery.
- **P5 interior payload quiescence** — domain modules import canonical owners and vocabulary only — zero boundary payload type imports on interior parameters after last promotion unit commit.
- **P6 checker matrix revision lock** — pyright, mypy, and ty matrix modules referenced by `ci_gate_ids` share frozen revision hash with last certify commit — single-backend green does not satisfy P6.
- **A1 witness table parity** — adapters and metamorphic fixtures consume identical emitted row columns — hand-maintained field lists beside row authority fail A1 on scheduled gate.
- **A2 vocabulary convergence** — payload `Literal` and enum imports, envelope `match` arms, and chain `vocabulary_ref` columns agree on finite discriminant set — orphan arms fail before behavioral suites.
- **A3 adapter substrate identity** — root-level `TypeAdapter` warm-up, specialization registry, and chain catalog module identity unchanged since last boot witness — post-import reassignment is assurance failure.
- **A4 extension fold closure** — every chain declaring `extension-fold-snapshot` slug asserts frozen shape named by `extension_fold_target` — mutable extension dict survival into domain parameters fails A4.
- **A5 metamorphic attribution fidelity** — synthetic fault injection per chain slug routes to adapter binding, gate arm, or row column on first attempt — conflated attribution to domain owner is assurance failure.
- **A6 promotion quiescence** — mainline lacks open partial landing on payload key, discriminant arm, openness flag, or chain slug without simultaneous table, schema snapshot, and registry update — partial evolution is merge blocker; when violated, temporal phase stays `promotion_in_flight` and promotion in-flight wins over sustain assurance on obligated contexts.
- Invariant law — each invariant carries `assurance_invariant_id`, `witness_table_ref`, `cadence_gate`, and `default_assurance_row` — undocumented invariant blocks steady-state certification.

# Assurance Decision Lattice

- Verification events classify through a finite chooser lattice — no ad hoc periodic payload checks without row binding.
- **Boot witness row** — P1, P3, A3 at composition-root warm phase before first ingress handoff — failure blocks context activation; routes to warm-graph re-registration when adapter substrate skew detected.
- **Pre-merge witness row** — P6, A6 plus triage and chain row obligation set on diff classifier output — failure blocks merge; routes to rollback-first when partial landing detected on wire-visible payload change.
- **Scheduled replay row** — A1, A2, P2 on root cadence policy — failure routes to reader re-emission and oracle diff before metamorphic chain re-run.
- **Post-promotion certify row** — full P1–P6 and A1–A6 after payload promotion unit commit — certifies steady state restored before marking context ready for deep assurance.
- **Health tick row** — lightweight A3 and A5 on root interval during live operation — detects adapter identity drift or metamorphic attribution regression without full generative replay.
- **Deep assurance row** — static checker matrix, contract tables, and metamorphic chains per harness layer ordering — executes on release cadence or federation topology change; scheduled gate failure blocks deep assurance start.
- Lattice law — each row names witness sources, proof layers, failure signal mapping, and reconciliation escalation row — undocumented assurance path is merge blocker.

# Convergence Proof Morphisms

- Convergence proves all obligated composition roots agree on logical payload revision — not wall-clock synchronization — structural witness equality across bounded contexts sharing vocabulary export.
- **Row convergence** — `required_keys`, `optional_keys`, `read_only_keys`, `closed`, and `extra_items_type` frozensets at vocabulary owner equal hash recomputed from each importing adapter's emitted row slice — mismatch triggers A1 failure and scheduled replay row.
- **Gate convergence** — `gate_expression_ref` symbols at each root match parametrized promotion tests and chain catalog bindings — mismatch triggers P3 failure and gate expression audit row.
- **Chain convergence** — `chain_id` registry revision tag matches metamorphic parametrized module fixture ids — stale slug references trigger P4 failure before integration smoke.
- **Triage convergence** — artifact class per concept-stage equals interior import graph witness — payload import on domain parameter after certify commit triggers P5 failure and promotion gate relocation row.
- **Schema convergence** — `TypeAdapter(Payload).json_schema()` snapshot hash equals reader-emitted OpenAPI fragment hash per specialization — mismatch triggers P2 failure at emission oracle, not at domain fold.
- Convergence composition — row convergence is prerequisite for gate and chain convergence replay; triage convergence may parallelize after row passes — same harness layer ordering as metamorphic chains.

# Certification And Witness Tables

- Certification tables materialize steady-state obligations beside liable owners — production adapters and composition roots read the same frozen rows as emission, admission, and chain witnesses.
- **Steady-state witness row** — `assurance_invariant_id`, `witness_table_ref`, `cadence_gate`, `last_certified_revision`, `obligated_context_ids`, `proof_layers_on_deep_assurance`.
- **Payload federation row** — `concept`, `shared_owner_id`, `vocabulary_module_ref`, `context_ids`, `ingress_chain_ids`, `egress_chain_ids`, `dual_surface_staged` flag when CLI and HTTP paths bind canonical equality.
- **Convergence witness row** — `convergence_kind`, `source_revision_hash`, `target_context_id`, `parity_assertion_id`, `escalation_signal_on_failure`.
- **Certification commit row** — `certification_run_id`, `assurance_rows_passed`, `doctrine_invariants_confirmed`, `steady_state_entry_timestamp`, `next_scheduled_gate`.
- **Stack graduation row** — `concept`, `graduated_artifact_class`, `retired_payload_owner_id`, `promotion_unit_id`, `interior_import_scan_passed` — documents payload retirement when concept moves to variant family or wire struct.
- Table law — composition root imports certification rows; duplicate invariant lists in test files are drift signals.
- Certification table change is federation event — obligated context witness imports update in same commit as revision bump.

# Change Classification Under Steady State

- Not every edit is a payload promotion event — steady-state change classification prevents unnecessary federation fan-out while preserving doctrine equilibrium when payload edges are untouched.
- **Interior-only change** — domain fold, variant dispatch, or port implementation within one context; no payload key, discriminant, openness, triage row, emitted row, gate expression, or chain slug change — assurance limited to context-local owner proof layers.
- **Witness-neutral change** — diagnostic tag, trace attribute, or comment without semantic effect on row columns — no assurance row update; arch rules still apply.
- **Edge-touching change** — any diff on boundary payload owner, adapter correspondence table, `TypeAdapter` specialization, promotion `match` arm, chain slug, or `json_schema` snapshot — reclassifies as payload promotion event; post-promotion certify row mandatory.
- **Topology change** — bounded context add, split, merge, or new carrier posture on existing concept — federation event; deep assurance row and full harness S1–S5 mandatory before steady-state re-entry.
- **Graduation change** — concept stack-graduates to canonical owner, variant family, or wire struct while interior modules retain payload imports — collapse and stack graduation row mandatory in one promotion unit.
- Classification witness row — `change_class`, `assurance_rows_required`, `promotion_unit_required`, `fan_out_context_ids` — git diff classifier or arch rule exports row; misclassification is equilibrium violation when edge drift follows.

# Dual-Surface And Cross-Context Federation Rows

- Dual CLI and HTTP ingress for one concept registers federation row with shared `emitted_row_ref`, distinct `chain_id` per carrier, and `dual_surface_staged=true` when canonical equality proof is active merge gate — either surface move binds both chain rows and federation row in one promotion unit.
- Settings `validation_alias` and cyclopts `Parameter` omission tables map once in adapter — federation row references correspondence ref; assurance verifies alias-key ingress dicts validate into same boundary row before promotion as HTTP dict paths.
- Cross-context payload federation shares boundary owner module and vocabulary export — distinct `chain_id` per bounded context with shared `owner_id` on emitted row; interior modules in neither context import sibling context payload types.
- Federation row `obligated_context_ids` lists every context that must import revised certification row on topology change — undocumented context omitted from fan-out fails pre-merge gate A6.
- Canonical equality proof parametrizes all carriers in federation row — proof failure blocks steady-state mark even when individual carrier chains pass in isolation.
- Sentinel-parameter CLI entrypoints pair with `*` boundaries — federation row metadata flags `sentinel_boundary` when staging row documents cyclopts omission alignment; health tick verifies positional callers cannot skip into sentinel arms.

# Evolution Binding Under Steady State

- Wire-visible payload evolution is federation event — ingress chain row, egress chain row, correspondence refs, `json_schema` snapshot, emitted row, gate `match` arms, and registry update in one promotion unit; partial evolution fails A6 and blocks certify row.
- Closed key removal or requiredness tightening emits migration row metadata and stack graduation check when interior modules still import retired owner — graduation and evolution bind atomically when concept stack-moves.
- Deprecation retires promotion arms and chain slugs only after migration proves zero ingress dependency — retired rows remain in migration snapshots; active catalog and certify scope exclude retired arms.
- Provider remap table changes bind correspondence ref, boundary row canonical keys, and anti-corruption field map — external key renames never reach interior modules; federation row `correspondence_row_ref` revision bumps trigger scheduled replay.
- Callable payload seam changes bind root handler signatures, `callable-signature-preserve` chain proof, and ParamSpec preservation tests — signature erasure fails pre-merge P6 static layer before certify.
- Generic specialization evolution updates every chain row referencing `owner_id`, reader frozenset emission per concrete argument, and `generic_matrix=true` metadata when backends disagree — partial retarget fails P2 before P4 chain replay.

# Failure Attribution At Assurance Layer

- Certify row failure at P2 — reader fold bug, missing `model_rebuild()` before emission, or adapter specialization mismatch; fix emission oracle before gate or chain repair.
- Certify row failure at P3 — gate expression refactor without chain `gate_expression_ref` update; collapse to single root admission.
- Certify row failure at P5 — promotion gate misplaced; domain parameter still typed as payload; re-triage to owner.
- Scheduled gate failure at A1 — adapter field map hand-edited beside emitted row; collapse to Stage 5 fan-out from reader emission.
- Health tick failure at A3 — `TypeAdapter` warm-up race or specialization registry reassignment after import; pin boot gate order before blaming wire drift.
- Health tick failure at A5 — metamorphic fault injected at wrong layer; conflated attribution between adapter, gate, and domain owner; rebinding chain `negative_fixture_ids`.
- Quarantine resume failure at S3 — static checker matrix still red; generative replay must not start until P6 revision lock restored.
- Graduation scan failure at H6 — interior import of retired payload owner; complete stack graduation row before clearing quarantine.

# Federation Health Signals

- Health signals are proactive drift precursors — not yet binding invariant violations — structured vocabulary enables assurance loop action before metamorphic escalation.
- **H1 row revision lag** — adapter consumes emitted row revision N while payload owner publishes N+1 — triggers scheduled replay row before oracle parity failure at chain runtime.
- **H2 chain fixture age** — metamorphic parametrized module fixture ids stale relative to catalog revision tag — triggers chain convergence check on health tick.
- **H3 gate expression drift** — root handler refactors without updating `gate_expression_ref` on chain row — triggers P3 audit before promotion test false-green.
- **H4 certify quiescence gap** — mainline lacks post-promotion certify row commit after merged payload promotion unit — blocks steady-state mark; temporal witness `phase` remains `promotion_in_flight` until certify commits.
- **H5 dual-surface skew** — CLI and HTTP chain rows share `emitted_row_ref` but canonical equality proof not parametrized in same merge gate — triggers dual-surface federation row escalation.
- **H6 graduation residue** — interior scan detects payload import for field set already graduated to wire struct or variant family — triggers stack graduation row before deep assurance.
- Signal routing — each health signal maps to default assurance row and optional reconciliation row on shape-system integration axis when multi-context handoff implicated.

# Stack Graduation Equilibrium

- Stack graduation retires payload owners when durable lattice slots graduate to canonical owners, variant families, or wire structs — graduation is explicit equilibrium event, not silent import decay.
- Graduation witness row binds retired `owner_id`, active materialized owner module, egress struct or family module, and interior import scan obligation — partial graduation leaving interior payload imports is P5 and H6 failure.
- One promotion unit per graduation — payload owner retirement, triage row update, chain row retirement or retarget, emitted row archival to migration snapshot, and registry slice removal land atomically.
- Retired discriminants and payload arms remain in migration modules with `assert_never` witnesses — active promotion `match` blocks and chain slugs do not reference retired rows.
- Graduation does not demote owners to durable payload parameters — egress projection produces assignability targets only; interior API shrink via payload reintroduction is equilibrium violation.

# Violation Signal Taxonomy

- Payload violation signals classify through closed vocabulary — each signal maps to one steady-state invariant and one default restoration row.
- **V1 triage gap** — concept-stage slot undocumented or interior import contradicts triage artifact class — maps to P1; default row completes triage binding and relocates promotion gate.
- **V2 row oracle skew** — reader frozenset diverges from `TypeAdapter.json_schema()` or adapter field map — maps to P2; default row re-emits from reader emission and blocks chains until oracle green.
- **V3 gate split** — validate and promote separated across modules without documented sub-rows — maps to P3; default row collapses to single root admission expression.
- **V4 chain orphan** — active ingress route lacks `chain_id` or references prose field list — maps to P4; default row registers catalog row.
- **V5 interior payload leak** — domain module imports or accepts boundary payload type — maps to P5; default row promotes parameter site to owner and updates triage row.
- **V6 matrix revision skew** — checker matrix modules drift from certify commit hash — maps to P6; default row freezes matrix revision and replays static layer.
- **V7 extension survival** — mutable extension dict reaches domain transform after chain declaring fold slug — maps to A4; default row adds gate fold proof and metamorphic negative.
- Signal law — each signal carries `assurance_invariant_id`, `liable_owner_module`, `default_restoration_row`, and `proof_layers_to_replay` — undocumented signal blocks merge.

# Restoration And Quarantine Postures

- Multi-context payload restoration executes as typed choreography — not parallel domain edits widening contracts to absorb drift.
- **Phase S0 — signal freeze** — composition root pins promotion unit id, violating signal, and obligated context set; no new ingress on affected routes until choreography completes or quarantine declared.
- **Phase S1 — row consolidate** — when V2 or V6 — reader re-emission and oracle diff before gate or chain repair; static layer re-runs before generative replay.
- **Phase S2 — edge repair** — adapter correspondence, gate `match` arm, or chain slug patch on liable module only; domain interiors excluded from S2 edit set.
- **Phase S3 — proof replay** — harness layers re-run from failed layer per ordering; static failure blocks generative and metamorphic suites.
- **Phase S4 — handoff resume** — composition root clears quarantine; ingress restarts at pinned gate expression; raw dict carriers forbidden at resume boundary.
- **Phase S5 — witness commit** — certification and convergence rows update beside liable owner; fan-out contexts import revised rows in same commit as S4 resume.
- **Edge quarantine** — single ingress route isolated; other routes remain full posture; quarantined route returns typed fault with signal and payload owner id.
- **Context quarantine** — entire bounded context suspended on repeated V5 or V7 within grace window; upstream may still promote owners; downstream receives tagged fault until S3 passes.

# Assurance Cadence And Gate Binding

- Assurance gates bind to lifecycle hooks distinct from metamorphic failure attribution — conflating hooks produces false steady-state confidence or duplicate proof spend.
- **Boot gate** — boot witness row P1, P3, A3; runs after adapter warm-up, before first validate-promote handoff.
- **Pre-merge gate** — pre-merge witness row P6, A6; runs on git diff via change classifier; independent of runtime reconciliation loop.
- **Post-promotion gate** — post-promotion certify row full P1–P6 and A1–A6; runs once per payload promotion unit; steady state re-enters only after certify passes.
- **Scheduled gate** — scheduled replay row A1, A2, P2; runs on root cadence; may overlap boot on cold start but never skips pre-merge on promotion diffs.
- **Health tick gate** — health tick row A3, A5; runs during live operation; failure escalates to quarantine when tick budget exceeded.
- **Deep assurance gate** — deep assurance row full harness ordering; runs on release or topology change; scheduled gate failure blocks deep assurance start.
- Gate ordering — boot before handoff; pre-merge before merge; post-promotion before steady-state mark; scheduled independent of ingress volume; health tick continuous; deep assurance last on cadence.

# Assurance Versus Promotion Boundary

- Assurance and promotion choreography are complementary — composition root schedules both with distinct triggers, phases, and proof budgets per assurance-promotion temporal law.
- **Promotion triggers** — edge-touching diff on boundary payload owner, adapter correspondence table, `TypeAdapter` specialization, promotion `match` arm, chain slug, `json_schema` snapshot, triage row, or topology change — reactive; opens `promotion_unit_id`, suspends steady-state mark, mandates restoration S0–S5 when violation signals fire, and requires post-promotion certify before sustain re-entry.
- **Assurance triggers** — cadence due, boot complete, post-promotion certify request, health tick — proactive; lawful only in assurance-between-promotions phase on non-quarantined routes; may run when zero promotion units are in flight on obligated contexts.
- **Escalation edge** — assurance row failure on P2 or A1 maps to H1 or scheduled replay first; repeated failure, H4 certify quiescence gap, or tick budget exceeded maps to quarantine and restoration phases — assurance does not patch adapter field maps or widen openness to avoid promotion.
- **Shared witness tables** — emitted rows from contract reader emission lattice, triage rows from payload owner adapter triage, gate bindings from promotion gate admission calculus, and chain catalog rows are inputs to assurance rows — certification commits logical revision consumed by next scheduled replay; promotion unit lands row mutations atomically before certify witnesses them.
- **Proof spend ordering** — health tick and scheduled replay before deep assurance before full metamorphic harness on release cadence — same static-before-generative ordering as promotion gate admission calculus; deep assurance during open promotion unit on affected contexts is collapse defect.

# Composition Root Assurance Loop

- Long-running processes admit a bounded assurance loop at composition root — distinct from promotion restoration S0–S5; runs during assurance-between-promotions and after post-promotion certify entry.
- **Loop trigger** — scheduled gate due, health tick interval, boot completion, or post-promotion certify request — not per-request domain invocation and not during open promotion unit on affected routes unless root policy documents scoped degradation.
- **Loop body** — read temporal witness row phase → select assurance row → replay witness parity → run configured proof layers → commit or escalate certification row → schedule next gate.
- **Loop budget** — maximum assurance depth per interval documented at root — health tick stays lightweight; deep assurance deferred to release cadence unless H-signal escalates.
- **Loop idempotency** — repeated certification on unchanged logical revision produces identical certification commit row hash — no duplicate witness commits.
- **Loop exclusion** — interior domain modules never invoke assurance; boundary adapters and root scheduler own cadence; promotion in-flight on a route blocks loop depth beyond health tick on that route until certify or quarantine clearance.

# Payload Federation Lifecycle Synthesis

- Contract payload doctrine through federation steady-state assurance lattice form one closed payload federation lifecycle — contract doctrine, triage, reader emission, gate admission, metamorphic witness catalog, and steady-state assurance; no sixth report redefines taxonomy without doctrine amendment.
- **Design phase** — projection lattice role map, openness posture, and vocabulary export per concept before federation edges activate — static checker matrix modules gate entry.
- **Promote phase** — edge-touching promotion unit, fan-out witness commits, restoration S0–S5 when signals fire — steady-state invariants suspended; promotion in-flight wins until post-promotion certify passes.
- **Sustain phase** — assurance cadence, convergence replay, and health tick per this document — default operating posture **between** promotion events; assurance-between-promotions phase only.
- Lifecycle law — composition root owns phase transitions; contexts do not self-advance from sustain to promote without edge-touching diff classification; contexts do not skip restoration when violation signals fire during promotion.

# Assurance Collapse Tests

- Collapse tests verify assurance remediation — extend gate and evolution collapse signals with steady-state payload anti-patterns.
- **Skip certify after promotion** — merge lands promotion unit without post-promotion certify row — collapse by blocking steady-state mark and running full P1–P6 and A1–A6.
- **Assurance during in-flight promotion** — scheduled replay or deep assurance mutates emitted row while `promotion_unit_id` open — collapse by deferring sustain gates until certify or enforcing S2 edge repair only on liable module.
- **False steady state** — quarantine active or promotion unit open but certification row marks steady — collapse by coupling certify row to temporal witness `phase` and quarantine clearance.
- **Interior-only mislabel** — edge-touching payload diff classified interior-only — collapse by reclassifying as promotion event and running fan-out obligation set.
- **Deep assurance before scheduled** — metamorphic harness runs while P2 row convergence fails — collapse by enforcing gate ordering; scheduled replay must pass first.
- **Assurance widens contract** — scheduled gate failure triggers `extra_items` widening or `closed=False` under closed parent — collapse to restoration S2 edge repair on adapter only; never absorb drift by relaxing payload law.
- Remediation order — classify change, read temporal phase, select assurance or promotion row, replay witnesses, certify or restore — never disable assurance gate to absorb promotion debt.
