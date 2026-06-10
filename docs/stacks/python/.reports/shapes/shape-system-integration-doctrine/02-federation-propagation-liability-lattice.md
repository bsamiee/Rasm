# Federation Propagation And Liability Lattice

# Propagation Graph Over Federation Topology

- Python `>=3.15` admits a propagation graph layered on the federation DAG — nodes are vocabulary owners, seam adapters, warm-graph registrations, and receipt fold owners; edges are promotion obligations, not import dependencies.
- Propagation originates only at canonical promotion events — vocabulary row add, variant arm add, phantom stage literal add, schema_version arm add, or context split/merge — never at interior refactors that do not touch federation edges.
- **Vocabulary propagation** — consensus row change fans to every context importing the vocabulary owner; fan-out width equals federation vocabulary-edge count; partial fan-out is merge blocker.
- **Seam propagation** — remap table or cardinality rule change fans only to the foreign pair owning the seam adapter; does not require interior domain module edits unless canonical materialization contract changes.
- **Warm-graph propagation** — ingress or wire target change fans to composition-root warm registration and every context node caching `TypeAdapter` or `Decoder` for that target — cold recompile after promotion is defect.
- **Receipt propagation** — new `StrEnum` kind on fact stream fans to every fold reducer importing the kind vocabulary and every receipt edge consumer — fold totality updates in same promotion unit.
- Propagation graph must remain acyclic — bidirectional obligation edges between contexts indicate missing vocabulary federation or duplicate seam ownership.

# Liability Assignment On Federation Edges

- Every propagation edge carries exactly one liability owner — the module that must land first in a promotion unit; downstream contexts are consumers, not co-owners.
- **Vocabulary liability** — vocabulary owner module owns consensus row, bijection contract table, and OpenAPI enum arm; seam adapters import rows; contexts never co-edit token strings.
- **Seam liability** — anti-corruption adapter module owns remap totality, cardinality remap, and seam metamorphic corpus; vocabulary owner is liable only when fault is unmapped token.
- **Migration liability** — boundary migration fold owner per version jump; canonical owners in all contexts remain version-agnostic; cross-context migration chains compose inside-out at seam exit before local fold.
- **Warm-graph liability** — composition root owns singleton registration; boundary packages own `annotationlib` resolution paths; domain modules never appear in warm-graph fault stacks.
- **Proof liability** — system S2–S5 layers attribute to federation edge first; context-local canonical record is liable only after edge proof passes — reverses common instinct to patch domain when seam remap fails.
- Dual liability on one invariant — duplicate-validation across federation edges — collapses to single owner per equilibrium invariant; secondary surface thins to projection mirror in same promotion unit.

# Propagation Decision Lattice

- Promotion events classify through a finite chooser lattice — no free-form federation edits without lattice row binding.
- **Token-only change** — vocabulary propagation only; seam remap foreign column updates when foreign contexts exist; ingress discriminator and wire tag update via consensus import; domain interiors unchanged.
- **Arm add on closed union** — vocabulary row plus registry row plus migration arm plus fold arm plus system S3 corpus append — all contexts sharing owner import vocabulary; seam remap gains row only when foreign union splits differently.
- **Cardinality axis change** — seam liability primary; ingress optionality mirror secondary; canonical smart constructor tertiary only when absence semantics change — never invert order.
- **Context split** — new seam between split contexts plus vocabulary row partition plus warm-graph node registration plus S1 acyclic re-proof — no duplicate canonical owners post-split.
- **Context merge** — vocabulary consolidation plus seam retirement plus migration fold proving zero handoff dependency — anti-corruption modules archive with `assert_never` witnesses.
- **Stage literal add on federated generic** — phantom stage map updates on every cross-context projection edge accepting staged owners — static stage bounds update system-wide in one unit.
- Lattice law: each row names liability owner, propagation fan-out set, and proof layers that must re-run — undocumented promotion path is merge blocker.

# Staleness And Consensus Drift Detection

- Federation consensus is logical, not temporal — drift is detectable structurally without distributed clocks.
- **Orphan token drift** — token appears on ingress or wire in any context without consensus row — CI bijection gate fails at vocabulary owner.
- **Collision drift** — two consensus rows map to same wire tag or discriminator literal — contract table parametrized collision test fails before metamorphic suite.
- **Import skew drift** — context A imports vocabulary owner revision N while context B imports revision N−1 in same process — import-linter pins vocabulary module version hash or monorepo single-source guarantees revision unity.
- **Warm-graph staleness** — `TypeAdapter` or `Decoder` instance predates promoted ingress or wire class body — metamorphic round-trip fails at encode/decode identity mismatch; attribution targets root warm re-registration, not domain logic.
- **Fold skew drift** — receipt kind added without fold arm in one context but not another on shared stream vocabulary — exhaustive fold test on importing context fails while exporter passes — receipt edge propagation incomplete.
- **Seam corpus staleness** — foreign fixture corpus lacks row for new remap entry — system S3 seam metamorphic fails one-way proof; seam adapter owner liable.
- Drift remediation order — vocabulary consolidate, warm-graph rebind, seam corpus append, fold arm add, context-local thin projection — never add bypass DTO at consuming context.

# Handoff Pressure And Concurrent Morphism Chains

- Federation handoffs are typed morphism chains — concurrency adds ordering constraints, not shared mutable canonical stores.
- **Single-writer canonical** — each concept has one owning context for canonical mutation; consumers receive immutable snapshots or fold-derived receipts — parallel writers to same canonical kind across contexts forbidden.
- **Seam serialization** — `materialize_*` at seam is pure given normalized ingress; concurrent calls do not share adapter-local mutable caches — memo tables keyed by immutable ingress hash only when composition root documents idempotent replay policy.
- **Fan-out pressure** — one local canonical may project to multiple wire targets or seam exports; each edge owns separate adapter; fan-out does not fork canonical identity or duplicate mutation receipts.
- **Fan-in pressure** — multiple foreign ingress sources map through distinct seam pairs into one local canonical owner — fan-in merges only at `materialize_*` exit via smart constructor, not via shared partial dict accumulation.
- **Async federation** — `@effect.async_result` permitted at seam and boundary adapters only; parallel async handoffs await canonical materialization before interior `bind` — rail carriers never hold foreign ingress kinds across `await` boundaries without rematerialization.
- **Backpressure signal** — receipt edge consumers that cannot fold fast enough receive stream snapshots, not live mutable buffers — cross-context backpressure is snapshot cadence policy at composition root, not interior queue mutation.

# Federation Degradation Postures

- Degradation is explicit posture change at composition root — never silent validation downgrade at seam or vocabulary edge.
- **Full federation** — all system proof layers active; semi-trusted and untrusted edges validate at ingress; default production posture.
- **Seam-isolated degradation** — one foreign context unavailable; composition root routes to tagged `SeamUnavailable` fault at anti-corruption adapter — no partial canonical materialization from stale foreign cache.
- **Vocabulary read-only degradation** — token promotion frozen; existing consensus rows remain bijective; extension arms capture unknown foreign tokens per semi-closed policy — not ad hoc string buckets per seam.
- **Warm-graph reduced mode** — documented deferral policy activates lazy context; deferred context still completes warm graph before first canonical handoff — lazy is not cold-per-request instantiation.
- **Trusted-replay-only degradation** — cross-context handoff accepts only pinned store bytes when live foreign ingress unavailable — replay pins store key, schema version, and encoder identity; migration fold still runs on stored version.
- Posture change updates composition root configuration record and system smoke diagnostic routing — contexts do not self-declare degradation.

# System Proof Layer Stack

- System S1–S5 are federation-scoped compositions of integration proof layers — not parallel proof taxonomies; propagation lattice rows name which S-layer must re-run after each promotion event.
- **S1 — federation static and import closure** — composes Layer 1 static exhaustiveness with Layer 2 import architecture over the context DAG — acyclic federation graph, cross-context domain interior import ban, wire-never-imports-ingress, and role-map coverage; failure signal `federation_import` or kind-coherence static defect; liable owner is composition root wiring or boundary package placement.
- **S2 — vocabulary bijection** — composes Layer 3 contract tables at federated vocabulary owner — consensus row collision, orphan token, OpenAPI enum arm parity, and import revision hash pin; liable owner is vocabulary owner module; seam adapters are consumers only.
- **S3 — seam and metamorphic corpus** — composes Layer 4 metamorphic and round-trip with seam-specific foreign fixture corpus — one-way foreign-to-canonical when bijection is not policy; cross-context round-trip when foreign-local bijection is documented; liable owner is anti-corruption adapter for seam faults, vocabulary owner when fault is unmapped token.
- **S4 — cross-context runtime smoke** — composes Layer 5 runtime boundary with federation diagnostic routing — injected ingress, wire, seam, migration, and propagation defects must attribute to liable edge module on first attempt; liable owner is composition root span attributes or named adapter before either domain owner.
- **S5 — federation mutation and drift** — composes Layer 6 mutation and drift on adapter `materialize_*`, remap tables, and warm-graph registration rows — Stryker kill ratio on seam logic; import-linter federation rules; integration drift signals on propagation witness gaps; domain interiors excluded from S5 edit and rollback sets.
- Stack law — S-layer failure during promotion atomicity test blocks merge at the failed layer; static S1 failure blocks S3–S5 generative spend; proof liability — patch domain only after edge S-layer passes.

# Cross-Context Proof Obligation Routing

- System proof — failure site determines promotion rollback scope.
- **S2 bijection failure** — rollback vocabulary promotion unit across all federation vocabulary edges; seam and domain edits revert with row.
- **S3 seam metamorphic failure** — rollback seam adapter and remap table; vocabulary row stands when fault is cardinality not token.
- **S4 cross-context smoke failure** — rollback composition root handoff wiring or seam diagnostic tags; attribution conflation implicates root span attributes before either domain owner.
- **Warm-graph static failure** — rollback boundary package `annotationlib` binding or root registration row — domain modules excluded from rollback set.
- **Propagation partial landing** — CI gate rejects merge when any fan-out context lacks contract row update — git diff on vocabulary owner must include simultaneous changes on all importing adapter modules or explicit exemption document.
- Proof debt from checker gaps on federation modules tracks on language axis — harness suppressions at propagation seams rejected; fix owner typing on liable edge module.

# Propagation And Evolution Coupling

- Evolution gates — version bump is federation event when wire crosses context boundaries.
- **Version arm add** — migration fold plus consensus row plus warm-graph decoder registration plus system S3 corpus plus write-path current-version-only emit — nested sub-owner migrations compose inside-out across seam before local fold.
- **Vocabulary promotion unit** — canonical enum owner, ingress discriminator, wire tag, seam foreign column, OpenAPI arm, hypothesis registry row, fold kind — fan-out contexts import same row in one commit.
- **Stack graduation across contexts** — record-to-variant graduation in owning context triggers wire egress and dispatch key propagation; consuming contexts update seam materialization only when foreign layout exposed variant axis — not when change is interior-only.
- **Receipt kind add** — fold arm totality in exporting context plus importing context fold on receipt edge — kind vocabulary federates like scalar tokens.
- Breaking federation change requires simultaneous S2, S3, S4, and warm-graph rebind — partial evolution on one context node with stale consumers is equilibrium violation.

# Promotion Atomicity And Rollback Units

- A promotion unit is the smallest merge-atomic change set that restores federation equilibrium — partial units leave propagation graph edges dangling.
- **Vocabulary unit** — consensus row, contract table row, and every importing adapter diff in one commit — rollback reverts entire unit; no cherry-pick of single projection edge.
- **Seam unit** — remap table, metamorphic corpus append, and diagnostic tag map for the foreign pair — vocabulary row included only when token set changed.
- **Warm-graph unit** — root registration row, boundary `annotationlib` path, and compiled graph smoke sample — rollback does not touch canonical interiors.
- **Receipt unit** — kind `StrEnum` member, fold arm, and receipt-edge consumer fold update — exporter and importer contexts land together or merge blocks.
- Atomicity test — after promotion, run system S1–S4 in order; any layer failure triggers full unit rollback, not localized hotfix on consuming context.
- Exemption rows document composition-root-only promotions that skip fan-out — exemptions name liable owner and proof layers still required; default is full atomic unit.

# Propagation Witness Tables

- Witness tables materialize propagation obligations as parametrized rows beside liable owners — production adapters and CI gates read the same frozen table.
- **Vocabulary witness row** — canonical member, ingress literal, wire tag, seam foreign tokens, fan-out context ids, S2 bijection key — one row drives remap assertions and metamorphic tag parity.
- **Seam witness row** — foreign pair ids, remap key, cardinality rule id, corpus fixture id, S3 chain label — seam adapter imports row; tests do not duplicate foreign token lists.
- **Propagation fan-out row** — promotion event kind, liability owner module, obligated context ids, proof layers to re-run — lattice chooser exports rows; undocumented events fail arch rule.
- **Rollback witness row** — promotion unit id, revert file set, equilibrium invariant at risk — documents which S-layer failure maps to which rollback scope.
- Table law — adapters import witness rows; pytest parametrization derives from same owner; duplicate token lists in test files are drift signals.
- Witness table change is federation event — fan-out row update triggers simultaneous adapter import diff across all obligated contexts.

# Encoder Federation And Cross-Context Cache Keys

- Cross-context cache and fingerprint keys federate through module-level encoder singletons registered at warm graph — not per-context ad hoc `json.dumps` settings.
- **Deterministic encode** — `order="deterministic"` on shared msgspec `Encoder` — metamorphic proof and production hot paths use same instance; per-context encoder clones are federation defect.
- **Key material** — cache keys hash canonical wire bytes plus schema_version plus encoder module identity — foreign context bytes restart at ingress validation before entering key material.
- **Invalidation propagation** — vocabulary row or wire struct promotion invalidates cache entries keyed on affected tags — composition root documents invalidation sweep policy; silent stale cache serve is equilibrium violation.
- **Cross-context replay keys** — trusted-replay pin set includes exporting and importing encoder identities — mismatch fails at decode with `warm_graph` fault tag, not domain owner.
- Encoder liability — composition root owns singleton identity; boundary packages own struct targets; domain modules never import encoder modules.

# Propagation Diagnostic Tags

- Propagation faults extend federation failure archaeology with obligation discriminators — structured payloads name propagation edge, not undifferentiated validation text.
- **Tags** — `propagation`, `liability`, `staleness`, `atomicity`, `witness_gap`, `encoder_skew` — align with observability federation vocabulary; ad hoc strings for enum-owned concepts are drift.
- **Fault payload fields** — promotion event kind, liable owner module, fan-out context id, proof layer failed, rollback unit id — seam spans omit canonical field payloads.
- **Staleness fault** — names expected consensus revision, observed revision, and drift signal kind — attribution targets vocabulary owner or warm-graph registration, not interior transform.
- **Partial landing fault** — lists obligated contexts missing witness row update — CI smoke injects synthetic partial landing to assert routing before merge.
- Trace spans on propagation rollback emit exporting context, promotion unit id, and equilibrium invariant at risk — interior domain spans omit promotion metadata.

# Propagation Collapse Tests

- Collapse tests verify propagation remediation — same posture as lattice drift in , scoped to federation edges.
- **Bypass DTO** — consuming context defines parallel ingress type to avoid seam update after vocabulary promotion — collapse to seam adapter plus consensus import.
- **Split liability** — two contexts edit same token string — collapse to vocabulary owner sole liability; secondary projections thin to mirror.
- **Stale warm singleton** — root caches `TypeAdapter` predating promoted model — collapse to warm-graph unit rebind at root, not domain constructor patch.
- **Fan-out orphan** — vocabulary row lands without adapter diff in one importing context — collapse by completing atomic promotion unit rollback.
- **Async seam cache** — adapter stores mutable partial canonical across `await` — collapse to pure `materialize_*` with immutable ingress hash memo only under documented replay policy.
- **Encoder per context** — duplicate encoder instances with divergent `order` policy — collapse to federated singleton at composition root.
- **Receipt fold lag** — exporter adds kind without importer fold arm — collapse by completing receipt propagation unit across receipt edge.
- Remediation order matches drift section — never add third parallel type at consuming context to absorb propagation debt.

# Propagation Completeness Checklist

- Federation propagation is complete when equilibrium invariants — checklist runs at merge gate beside role-map coverage.
- Every promotion event maps to exactly one lattice row with liable owner and fan-out set — orphan events without witness row block merge.
- Every vocabulary row fan-out completes across all federation vocabulary edges — bijection and import skew drift signals are zero.
- Every seam remap change carries S3 corpus append when foreign pair is active — seam corpus staleness signals are zero.
- Every warm-graph target change rebinds root singleton before context activation — warm-graph staleness signals are zero.
- Every receipt kind change updates fold totality on exporter and all receipt-edge importers — fold skew signals are zero.
- Every degradation posture change updates root configuration and S4 diagnostic routing — contexts do not self-declare posture.
- Every propagation fault routes to liable edge module per diagnostic tags — conflated attribution fails smoke module.
- Collapse tests pass — no bypass DTO, split liability, stale warm singleton, fan-out orphan, async seam cache, encoder per context, or receipt fold lag remains.

