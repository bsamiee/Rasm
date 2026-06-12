# [BEDROCK] retention-classes

## artifact classes

- The artifact class is the one vocabulary every stored thing belongs to; each class row carries five decisions: storage lane (process-retained registry | durable blob | relational rows), retention record, classification ceiling, loss policy (receipted-evict | declared-expiry), and identity scheme (content key | name-plus-epoch).
- A new artifact type is one class row — admission, sweep, export, and caching all read the row; none of them grow code.
- The class row also declares manifest participation: which discovery and export manifests list the class — an artifact invisible to every manifest is reachable only by identity, a deliberate property for internal-only classes.
- Canonical class set: sealed snapshot, log segment, evidence bundle, export, cache blob, ephemeral. The set is closed; extension is a row.
- An artifact fitting no class is an admission rejection, not a default class — defaulting is how unclassified data acquires accidental retention and accidental ceilings.
- Class membership is immutable after admission: reclassifying an artifact is export-then-readmit under the new class, so every lifecycle the artifact has lived under is receipted — in-place class mutation erases the history that holds and ceilings depend on.
- The class row is where other layers' eligibility predicates plug in: log-segment and tombstone-bearing classes carry a fence predicate supplied by the sync owner, and the sweep consults it as an additional keep condition.
- Retention executes lifecycles, it never legislates sync safety — the predicate's owner answers for its correctness, the sweep answers for having consulted it, and the receipt proves both.
- The inventory is one catalog: (class, identity, stamp, byte count, classification stamp, lane reference) per artifact.
- Sweep, export, and admission all fold over the same catalog rows — a lane holding bytes the catalog does not know is by definition orphaned, and a second inventory anywhere is the fork that makes orphans undetectable.

## retention policy rows

- One retention record shape per class: { maxAge, maxCount, maxBytes, hold-eligibility } — each axis optional, absent meaning unbounded on that axis.
- Unbounded is a declaration, not a default: an axis left absent is a reviewed decision visible in the row, distinct from an axis nobody considered — the record shape forces the consideration.
- The record is a frozen policy value; a sweep run snapshots the policy once and every receipt in that run cites the snapshot stamp.
- A mid-sweep policy change can never produce receipts citing a policy no one declared — the snapshot stamp is what makes every historical receipt re-checkable against a concrete policy version.
- Age cuts by artifact stamp against the sweep instant; count keeps the N newest; size evicts oldest-first until under budget.
- One instant per run: the sweep reads its clock once and every age comparison in the run uses that instant — per-artifact clock reads make the verdict sequence irreproducible across the run's own duration.
- Ordering ties break by identity after stamp — newest-first ordering is total, so count and size verdicts are deterministic even among same-stamp artifacts.
- The axes compose as any-evict-wins after holds; the verdict names the FIRST deciding rule in the declared order age → count → size, making receipt streams comparable across runs and across stores.
- Legal hold is a first-class row (class, selector, since, reason key), never a flag mutated onto artifacts: an artifact is held if ANY hold row selects it.
- Releasing a hold deletes the row and the next sweep re-evaluates naturally — release has no eviction side effect of its own, so releasing a hold can never itself destroy data.
- Hold selectors take three forms — whole class, explicit identity set, stamp range — and selectors compose by union; hold application and release both emit receipts, so preservation state changes are themselves facts.
- Hold selectors bind late: evaluation happens at sweep time against the current inventory, so a hold placed today protects artifacts admitted tomorrow if the selector matches them — early binding to an identity snapshot is the rejected form that silently under-protects.
- Each sweep run emits an active-hold inventory receipt (hold row, match count, hold age) — forgotten holds are the dominant retention failure in practice, and hold age is the signal that finds them.
- Policy changes are epoch-stamped declarations: tightening applies from the next sweep; loosening never resurrects — an evicted artifact is gone, and a policy relaxation that expects deleted data back has confused retention with archival.
- Receipts are artifacts too: the receipt stream is a class row with its own retention record (count- and age-bounded) — meta-retention terminates because the receipt class's own sweep emits only count receipts, closing the regress at depth one.

## hold-first sweep fold

- The sweep is one fold over a class's inventory ordered newest to oldest, in three fixed stages.
- Stage 1 — the hold partition exits first: holds short-circuit every other rule, so no rule evaluation ever runs against a held artifact.
- Stage 2 — each remaining artifact receives the first deciding verdict in rule order.
- Stage 3 — within the size rule, eviction walks oldest-first until the budget clears.
- The verdict union is closed: Kept | Held | HeldOverBudget | EvictAge | EvictCount | EvictSize.
- HeldOverBudget is the load-bearing verdict most designs miss: held bytes count against the size budget but cannot be evicted, so the fold reports the breach as its own receipt instead of evicting around the hold — pressure caused by preservation is surfaced, never silently displaced onto unheld artifacts beyond their own rules.
- Determinism law: same inventory snapshot + same policy snapshot + same hold rows → identical verdict sequence and receipt stream.
- The fold computes verdicts as a pure pass first, then applies effects — decision and execution split, so the verdict list is a testable value.
- A partially failed sweep resumes by re-folding: already-deleted artifacts are simply absent from the next inventory; the sweep is idempotent by re-derivation and requires no journal.
- The sweep is one schedule row per store; cadence and per-run deletion budget are policy values — a bounded run that did not finish is a normal outcome carried in the summary receipt, never an error.
- The deletion budget bounds lock-hold per run in a multi-process store: sweeping is a writer like any other, and an unbounded sweep starves sibling writers exactly the way any long write transaction does.
- The sweep transaction holds inventory read, verdict application, row deletion, and receipt emission together.
- Blob bytes are deleted AFTER the row commit — the crash window produces orphan blobs, never dangling rows pointing at deleted bytes; the asymmetry is chosen because orphans are collectible and dangling rows are lies.
- The orphan pass closes the loop: lane enumeration minus catalog rows yields orphan candidates, age-gated past the longest plausible in-flight admission so the pass can never race a write that has bytes down but no row yet.
- Orphan deletions are receipted like evictions with their own deciding rule — orphan volume is the health signal for the files-after-rows crash window, and a rising orphan rate localizes a crash-loop before any other symptom does.
- One sweeper per store at a time — sweep concurrency rides the store's own write-serialization discipline rather than a second lock vocabulary.

## eviction receipts

- Every removed artifact emits (class, identity, deciding rule, policy snapshot stamp, byte count).
- Unreceipted deletion anywhere in the system is a rail rejection — there is no "just clean it up" path; manual operator deletion routes through an administrative verdict kind so even human action lands in the ledger.
- The per-run summary receipt carries per-class counts and reclaimed bytes with the conservation check inventory = kept + held + evicted; a run whose partition does not sum is itself a typed sweep fault — bookkeeping bugs become detected facts.
- Declared-expiry classes (the ephemeral lane) are the one shape exempt from per-row receipts: their loss policy is receipt-in-advance and the sweep emits count receipts — the two loss accountings are distinct row values, never an implicit downgrade.
- Eviction receipts feed capacity planning as data: the deciding-rule distribution per class is the signal that a budget is mis-sized (everything evicting by size and nothing by age means the byte budget binds first), so retention tuning is receipt analysis, not guesswork.

## classification admission guard

- Every artifact arrives stamped with its data classification; the taxonomy and the redaction that produced the stamp are settled upstream — this layer consumes stamps, full stop.
- The class row carries a ceiling; admission compares stamp against ceiling and rejects above it with a typed receipt naming (artifact identity, offered classification, ceiling).
- The rejection receipt is routing evidence for the upstream seam that owns the fix — the guard's job is to make taxonomy drift loud and attributable, never to repair it locally.
- An unstamped artifact is indistinguishable from an over-ceiling one: missing classification rejects identically — absence of evidence is not clearance.
- The store never re-redacts and never downgrades a stamp: a second masking pass at the store is the rejected form because it hides taxonomy drift behind silent repair and creates a second enforcement seam whose behavior can diverge from the first.
- Ceilings are build-invariant: debug builds may widen budgets and admit reload lanes, but a classification ceiling that varies by build configuration leaks restricted data through development artifacts — the ceiling column has no debug override by construction.
- Export/import round-trips cannot launder classifications: an imported bundle re-enters through the same admission fold, and its payload stamps re-verify against the receiving store's ceilings.

## budget records and truncation receipts

- Bounded-capture entries (fault dumps, transcripts, payload samples) carry per-entry budgets from their class row; budgets come in three kinds — bytes, item count, and nesting depth — and a record may carry all three.
- The depth budget is the one most designs omit: recursive structures truncate by depth with the receipt naming the pruned subtree count — byte budgets alone let a deep narrow structure evade the bound that a wide shallow one trips.
- Truncation is deterministic: the same oversize input truncates to the same retained prefix under the same budget — receipts from two replicas capturing one incident stay comparable.
- Overflow stores the truncated entry WITH an embedded truncation receipt (original length, retained length, truncating rule).
- A truncated entry is self-describing — downstream consumers need no side channel to know what is missing, which is what keeps truncation from poisoning analysis built on the captured data.
- Silent truncation and silent drop are both rail rejections — the budget exists to bound bytes, never to bound accountability.
- Drop-oldest ring lanes are admissible only when constructed with an on-drop receipt delegate — loss accounting is enforced at construction, so an unreceipted-loss lane fails to build rather than failing to account at runtime.
- The on-drop delegate receives the dropped entry's identity and stamp, not the entry itself — receipting loss must not retain what was dropped, or the ring's bound is fiction.
- The truncation-versus-rejection chooser is decided by the row kind breached: oversize against a BUDGET row truncates with receipt — capture must succeed degraded; oversize against a classification CEILING rejects outright — security never degrades. The two overflow responses are not interchangeable, and conflating them is the named anti-form in both directions.

## two-tier cache

- Exactly two tiers: tier one is the process-static retained registry (content-keyed, in-memory, alive for the process); tier two is the durable blob lane the artifact class already owns.
- Lookup falls tier one → tier two → produce; produce admits into both — the fall-through order is the whole cache protocol, and any path that skips a tier has invented a third one.
- A third cache — per-call memo, per-component cache, secondary disk cache — is the rejected form: it re-derives invalidation a third time and forks identity from the content-key scheme the class declares.
- Tier one is itself bounded by a class-row budget (count and bytes) with the same eviction receipts as any lane — memory-pressure eviction from the registry is a receipted fact, not a GC mystery.
- Tier-one warm policy is a class row value: boot may pre-admit the boot-identity manifest's assets so first use never pays the tier-two read — warm-at-boot versus warm-on-demand is declared, never emergent.
- Concurrent admission resolves by compare-exchange on the content key: the racing loser's bytes are identical by content-key construction, but its handles and native resources are not — the loser disposes its candidate and adopts the winner.
- The compare-exchange admits exactly once even under N-way races — the winner is whichever candidate's exchange lands first, and correctness needs no fairness because all candidates are byte-identical.
- Race-loser disposal is a receipted fact, not an error.
- The race-loss receipt rate is itself a contention signal — sustained losses mean producers are duplicating work that a produce-once gate upstream should absorb, and the receipt stream is what makes the duplication measurable instead of invisible CPU.
- Invalidation is epoch-shaped, not item-shaped: the registry keys on (epoch, content key), so a store epoch bump implicitly orphans every pre-bump entry without enumeration — tier-one invalidation rides the same fence restore already provides, and no per-item invalidation protocol exists to get wrong.

## frozen evidence windows

- An incident verdict freezes a window [t − Δ, t] whose bounds derive from the triggering fact's stamp — never from wall-clock-now — so re-entrant triggers reuse identical bounds and capture is idempotent per incident identity (trigger stamp + trigger kind).
- Δ is a per-trigger-kind policy row, not a global constant — a crash-class trigger and a performance-class trigger justify different windows, and the row keeps the decision declared.
- The capture choreography is fixed at five steps, each receipted:
- Step 1 — buffered recent faults replay into the window store; the replay buffer's own bound is a budget row, so the window's fault history is bounded by declaration.
- Step 2 — contributors fan in as declared rows (name, order, budget, deadline) in DECLARED order; deterministic bundle layout makes incident artifacts diffable against each other.
- Step 3 — a contributor exceeding its budget lands truncated with receipt; a contributor missing its deadline lands absent-with-receipt — the bundle always seals, never hangs on a slow contributor.
- Step 4 — every payload passes the classification admission guard; an over-ceiling contributor payload is refused-with-receipt while the bundle proceeds.
- Step 5 — the window lands as ONE sealed artifact plus a wire manifest enumerating contents and content keys.
- The manifest travels cheaply; the artifact follows on demand — incident triage reads manifests, not bundles, and pulls bytes only for the incidents it opens.
- Manifest-first transfer is also the privacy posture: classification-restricted bundle content never moves to a surface that only needed to know the incident happened.
- Evidence bundles are an artifact class row like any other: hold-eligible, ceiling-restricted, count-retained — incident capture invents no storage machinery.
- An incident-driven hold is just a hold row selecting the bundle — investigation lifecycle and retention lifecycle compose through the existing instrument, with no investigation flag on the artifact.
- A partially captured window (process death mid-fan-in) is a normal sealed artifact whose manifest shows absent-with-receipt contributors — partial evidence is evidence; an all-or-nothing capture is the rejected form because incidents are precisely when processes die.

## ordered-bounded-fold primitive

- Contributor fan-in and shutdown drain-band walks are one algebra: a fold over declared rows where each row owns (order, budget, deadline, receipt shape) and the fold's value is the receipt sequence.
- Legislated once, incident capture inherits drain semantics — cancellation linkage, per-row forced deadlines, band-ledger receipts — with zero new machinery.
- Shutdown inherits evidence semantics symmetrically: the drain walk's receipts are an evidence bundle of the shutdown, so an abnormal shutdown is automatically an analyzable incident.
- The law: any "gather from N parties under deadlines" requirement instantiates this primitive with new rows; a bespoke gatherer is the rejected form, because every bespoke gatherer re-invents exactly the budget, deadline, ordering, and receipt decisions the rows already carry.

## export proofs

- Export copies artifacts out of the store's jurisdiction accompanied by a proof manifest: per artifact (identity, content key, classification stamp, retention verdict at the export instant, policy snapshot stamp).
- The export fold reuses the sweep's inventory and verdict machinery read-only — export verdicts and sweep verdicts can never disagree about the same artifact at the same instant, because they are the same function.
- The proof is what makes an export auditable without the exporting store: verdicts and stamps travel with the copy, so a recipient can verify both content integrity and lifecycle posture from the manifest alone.
- Export does NOT create a hold — exporting-to-preserve is the rejected form; the hold row is the preservation instrument, and conflating the two makes retention state unauditable because copies imply nothing about lifecycle.
- The destination's clearance is an export-fold parameter: each artifact's classification stamp compares against the destination clearance the same way admission compares against ceilings — one comparison law serving both directions of the boundary.
- An export is all-or-receipted: an artifact that cannot be exported (ceiling above the destination's clearance, missing bytes) lands in the manifest as refused-with-receipt.
- Partial exports are explicit, never silently smaller — the manifest's refused rows are as load-bearing as its included rows, because an auditor reasons from both.
- Re-import is ordinary admission: identity re-derives, classification re-verifies, class rules re-apply — an export bundle has no privileged re-entry path.

## divergent

### retention-sweep-fold — the sweep as a pure fold with a receipt monoid

- Verdict computation is a pure function of (inventory snapshot, policy snapshot, hold rows); receipts compose associatively into the run summary; effects replay idempotently — together making the sweep crash-safe, resumable, and property-testable as a value-level function with no I/O in the specification.
- The generator-driven law: for any inventory and any policy, verdicts partition the inventory exactly and the partition is stable under re-fold — one property subsuming determinism, idempotency, and conservation.
- Cross-class pressure stays inside the same fold: a global byte budget is one allocation pass distributing per-class shares by declared weight rows before the per-class folds run — global pressure produces adjusted policy snapshots, never a second sweeper.
- Classes otherwise sweep independently and in any order — verdicts are class-local by construction, so cross-class ordering is meaningful only in the allocation pass, and a design where one class's sweep outcome feeds another's verdict has smuggled a dependency the catalog should carry instead.
- The deepest composition is eligibility injection: sync fences (tombstone safety), live-cursor floors (segments still being served), export-in-flight pins, and the orphan pass's age gate all enter as keep-predicates supplied by their owning layers — the sweep remains the single deletion executor in the system while owning zero domain-safety rules.
- Every refusal receipt names which predicate held the artifact, so "why does this still exist" is always answerable from receipts — the sweep's receipt stream is the system's complete deletion ledger and its complete non-deletion ledger at once.

### artifact-classes-admission — the class row as the meeting point of five laws

- Admission is one fold: classify-check → identity-derive → race-admit → budget-check → lane-write, every arrow emitting typed evidence.
- The entire artifact subsystem reduces to four owners — class table, admission fold, sweep fold, export proof — and everything else is rows; a fifth owner appearing anywhere is the collapse trigger.
- Truncation-versus-rejection derives from the row kind breached, so overflow behavior is never a per-call-site decision.
- The two-tier cache is not a cache feature but a lane value in the class row — "cacheable" means tier-one participation, and cache coherence collapses into the epoch fence the store already maintains.
- Budget widening is debug-permissible while ceilings are build-invariant — a precise statement of what development builds may relax and what they may never relax.
- Because identity scheme is a class decision, content-addressed classes get dedup and race-loser disposal for free while name-plus-epoch classes get versioned replacement for free — the same admission fold, two identity rows, two complete behavioral families, zero conditional code.
- The catalog-as-single-inventory means every byte in every lane is reachable from one fold — which is what makes the orphan pass, the export proof, and the capacity receipts possible without per-lane scanners.
- Admission records the byte count from the artifact's own sealed length fields at the moment of write — catalog size truth is never a later filesystem stat, so capacity accounting cannot drift from what was admitted.
