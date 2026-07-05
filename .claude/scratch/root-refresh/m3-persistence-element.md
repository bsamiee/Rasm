# [M3_PERSISTENCE_LAUNCH_READINESS_CENSUS]

Adversarial census of `RASM-CS-PERSISTENCE-DECISION.md` (381 lines) + `RASM-CS-PERSISTENCE-BRIEF.md` (216 lines) against the landed rebuild engine (`.claude/workflows/rebuild.js`, 583 LOC), the landed kernel (`libs/csharp/Rasm/.planning/`), `Rasm.Element/.planning/`, and `Rasm.Persistence/.planning/`. Verdict: the plan is ~90% launch-ready; a small set of engine-invocation and kernel-anchor fixes stand between it and a zero-fix next-session run. Counts: item1 = 3 blockers + 1 pass · item2 = 5 drifts · item3 = 3 (2 genuine, 1 interim-only) · item4 = keep-3-legs (0 change) · item5 = 4 chaff.

---

## [1]-[ENGINE_EXECUTABILITY] — 3 blockers, 1 pass

Engine contract (rebuild.js): `args = {targets, brief, leg, waves, riders, acceptance}`. Two brief modalities — (A) `{brief, leg}` targetless: the leg-partition rows ARE the page set; (B) `{targets, brief, leg}`: targets bound admission, leg activates `M.legRead`. Rider schema `RIDER` (:50-57): `{motion∈[manifest-drop,manifest-add,catalog-delete,counterpart-edit,verify], target, anchor(SYMBOL,"never a line number"), detail?, page?, guardPage?, wave:int}`. Trace schema `TRACE` (:58-59): `{name, needs:['<page>#<entry>'|seam-anchor]}`. Plan schema kinds ∈ {new,rebuild,improve} + `absorb` + `deletePages` + `upstreamMissing`.

### [1.1]-[BLOCKER] `leg` field OMITTED from all 6 invocation rows
DECISION `[06]:299-301` and BRIEF `[EXECUTION]:14-16` invoke `Workflow(rebuild.js, {targets:[...], brief:"…DECISION.md"})` — grep `leg:` = ZERO hits across both docs. Effect: `LEG=null` (rebuild.js:35) ⇒ `M.legRead=''` (:238-246) is never injected. The leg-selector's three services are lost: (a) leg-partition-as-authoritative-page-set, (b) reinforced rider-cell transcription, (c) **acceptance-trace emission** (the only place the plan is told to emit `acceptance` traces is `legRead`). The run still executes (targets are explicit) but degrades to the no-leg path.
FIX: append `, leg: 1` / `leg: 2` / `leg: 3` to each invocation (targets present + leg present = engine's "rows bound the admission" path, legRead:241). One-token edit ×6 rows.

### [1.2]-[BLOCKER] acceptance dry-runs are campaign-wide PROSE, not per-leg typed `{name, needs}` traces
DECISION `[06]:313-315` "Campaign acceptance" (a)-(e) is prose, all POST-leg-3; legs 1-2 have ZERO acceptance definition. Even for leg 3 the plan agent must convert prose→traces and cross-reference entry symbols from the `[02]` page-set table (the prose names no `<page>#<entry>` needs). Lossy — the fix-empowered fable acceptance agent (rebuild.js:502-516) has thin/no traces to run.
FIX: author explicit `acceptance` TRACE rows per leg using the entry signatures already in `[02]`. Leg-3 (a) VERSION ENGINE → `{name:'version-engine-dryrun', needs:['Version/commits.md#Crdt.Apply','Version/merge.md#StructuralMerge.Reconcile','Version/timetravel.md#TimeLog.Reconstruct','Version/provenance.md#AttestedLedger.Verify','Version/retention.md#RetentionSweep.Run','Version/recovery.md#RecoveryRoutes.Run']}`; (b) COORDINATION → needs `['Store/coordination.md#Coordinate.Run','Version/egress.md#EgressPump.Drain']`; (c) FEDERATION → `['Query/federation.md#Federation.Execute','Query/lane.md#ElementSet.Evaluate']`; (e) SPATIAL+CLOUD → `['Ingest/geospatial.md#GeoSource.Run','Store/blobstore.md#ObjectStore.Presigned','Query/cache.md#ArtifactKind.CloudRun']`. Legs 1-2: either state "generic sweep only" or add a band-registry trace `{name:'band-registry-complete', needs:['Element/graph.md#FAULT_TABLES']}`.

### [1.3]-[BLOCKER-SOFT] riders never tabulated as typed rows
The engine forces riders as `{motion,target,anchor(SYMBOL),page?,guardPage?,wave}`; the DECISION scatters them across `[05]-[ROSTER_DELTA]`, `.api catalog obligations` (:280-282), and `[10]-[OPEN_ITEMS]` (:373-379) as prose. The plan agent must synthesize them, and the DECISION is line-anchor-saturated while riders require SYMBOL anchors. The concrete rider set the plan must recover:
- `catalog-delete` · `libs/csharp/.api/api-messagepack.md` · anchor `RASM_PERSISTENCE`-tag · guardPage `libs/csharp/Rasm.Persistence/.api/api-messagepack.md` · wave 1 (byte-identical dup, shasum 9a7ba81f; the Materials copy cb59c0f1 DIVERGES, stays).
- `manifest-drop` · `Directory.Packages.props` · anchor `Grpc.Tools` · wave 1 (V1 DROP; regen mints a duplicate CLR `Plan`).
- manifest version edits (no version motion in the enum → express as counterpart-edit on `Directory.Packages.props`): `Ara3D.BimOpenSchema` 1.0.1→1.6.1 (wave 1), `MPXJ.Net` catalog 16.4.1→16.5.0 re-verify (wave 3).
- `verify` riders from `[10]`: `AWSSDK.S3`#`CompleteMultipartUploadRequest.ChecksumXXHASH128` (page blobstore, wave 3); DuckDB `substrait` extension build for `DuckDB.NET.Data.Full` 1.5.x (page federation, wave 3); Ara3D `AssemblyConfiguration("Debug")` re-decompile (wave 1); `merge`#`MemberPathSegment.Value is NodeId` under `[UnorderedEquality]` (page merge, wave 2).
FIX: add a typed `[RIDERS]` table to the DECISION keyed by leg, symbol anchors only. Note: there are essentially ZERO `manifest-add` NuGet riders — the roster is INTEGRATE-ONLY (`[05]`), so this is small.

### [1.4]-[PASS] targets · kinds · SPLIT/absorb all consume cleanly
- Targets resolvable: all 25 `[06]` paths are exact repo-relative; the 18 existing pages confirmed on disk (`Element/{graph,codec,identity}`, `Version/*7`, `Query/{lane,topology,columnar,cypher,cache}`, `Ingest/tabular`, `Store/{blobstore,provisioning}`); the 7 `new` pages (authority, egress, retrieval, federation, schedule, geospatial, coordination) confirmed ABSENT — so no `new`→silent-`rebuild` reclassification (rebuild.js legRead: "brief-new already on disk → rebuild").
- Leg counts 4/7/14 = 25 ✓; each leg fits IMPL_BATCH=4 chunking with the split pairs co-batched: leg-1 all 4 Element pages = 1 batch (identity `rebuild` + authority `new` intra-batch); leg-3 batch0=[lane,retrieval,topology,columnar] (lane/retrieval split co-batched), batch1 federation chained AFTER batch0 (federation→retrieval/columnar/lane satisfied via prevImpl chain), batch3=[coordination,egress] (egress→coordination#OUTBOX_CURSOR intra-batch, coordination listed first). Dependency order holds under the chained pipeline (rebuild.js:574-592).
- SPLIT via paired `new`+`rebuild`, engine `absorb` UNSET corpus-wide (both split sources survive) — matches the engine's absorb=whole-page-then-DELETE semantics. Correct.

---

## [2]-[KERNEL_TRUTH_DRIFT] — 5 drifts (namespaces + core digest grammar CLEAN)

CLEAN (EXISTS-AS-CITED): `ContentHash.Of(ReadOnlySpan<byte>)` seed-zero `XxHash128.HashToUInt128` (`Rasm/.planning/Domain/identity.md:31`, ns `Rasm.Domain`); the `Append`+`GetCurrentHashAsUInt128` streaming Growth member (identity.md:17); `Expected`/`IsExpected`/`Category`/`#[04]-[FAULT_BAND]` + `Fault.UnsupportedCode=9104` (`Domain/rails.md:104-127,:86`); `GeometryFault` 2400-2449 at ns `Rasm.Numerics` root (`Numerics/faults.md:3,:34`) — the DECISION 2400 pinned-mirror row is correct; `ONE_WIRE_FIXTURE_CORPUS` §`:326` + `CRDT_OP_SET|Version/commits#CRDT_ALGEBRA|DESIGN-PIN` `:341` + golden 52-byte `0x9462A71A5DD13DCFA3B1D6D225FCBE70` `:338/:364`; **namespaces folder-true** — all 9 (`Rasm.{Domain,Analysis,Processing,Meshing,Parametric,Spatial,Numerics,Solving,Drawing}`), **ZERO `Rasm.Vectors`/`Rasm.Geometry.*`** anywhere in kernel OR persistence docs; `EncodeForm` correct.

### [2.1]-[DRIFT] `NamingHashOps.Encode`/`Reconcile` is a PHANTOM type (3 occurrences)
`NamingHashOps` appears 3× (e.g. BRIEF `[SEAM_AND_RAIL_LAW]:55` "`NamingHashOps.Encode`/`Reconcile`"). No `NamingHashOps` type exists on disk. Landed surface: `Reconciliation.Apply(ReconcileOp)` with `Encode(EncodeForm Form)` (`Spatial/reconciliation.md:45`) and `Reconcile(NameTable Prior, CanonicalTopology Rebuilt)` (`:46`) as request CASES through the ONE `Reconciliation.Apply` entry (`:217`); the standalone statics are explicitly "the deleted sibling form" (`:16`). FIX: re-spell every `NamingHashOps.Encode`/`Reconcile` → `Reconciliation.Apply(ReconcileOp.Encode|Reconcile)`; `TopoName→GeometryHash` via the `NamingHash` receipt / `NameAddress(TopoName, EntityKind, GeometryHash)` (`:185`).

### [2.2]-[DRIFT] `Rasm/ARCHITECTURE.md:79` STALE anchor (5 occurrences)
Cited 5× (BRIEF:137, BRIEF:211, DECISION `[V12]`/`[06]` OUT_OF_SCOPE). On disk `:79` is a source-tree line (`Inspect.cs ...`). The `[CONTENT_KEY]` seam family is `:87-92` (not the cited `:76-81`); the reconciliation→Persistence counterpart row is `:90`, currently `Spatial/Reconciliation.cs → Rasm.Persistence/Query/topology`. FIX: re-anchor `:79`→`:90` and `:76-81`→`:87-92` everywhere. NOTE the compounding: the DECISION `[04]` RETARGETS its own ARCH:55 topology→`Version/merge`, so the kernel `:90` row (still →`Query/topology`) is the counterpart-obligation that must re-point to `Version/merge` — correctly OUT_OF_SCOPE (kernel campaign) but the anchor handed to that campaign is wrong.

### [2.3]-[DRIFT] geometry BRIEF/DECISION citations dangle on archive (6 occ / 5 lines)
`RASM-CS-GEOMETRY-BRIEF` ×5 (BRIEF:5,:108,:137,:211; DECISION:3) + `RASM-CS-GEOMETRY-DECISION` ×1 (DECISION:3). The load-bearing ones are the `[V9]` parametric-digest WATERFALL (BRIEF:108,:211 "waterfalled into `RASM-CS-GEOMETRY-BRIEF:[V2]`"). On archive these become dangling pointers. FIX: re-point the `[V9]` waterfall demand onto the LANDED kernel owner of parametric canonical bytes (`Rasm/.planning/Parametric/` + the `ToCanonicalBytes` composer at `Projection/address` / `Spatial/reconciliation`), and demote the DECISION:3 / BRIEF:5 "upstream honored: RASM-CS-GEOMETRY-*" header refs to "the landed kernel corpus" (the geometry campaign's rulings are already absorbed into the landed pages).

### [2.4]-[DRIFT] `Of(Node, Tolerance)` cosmetic member drift
DECISION E11 / BRIEF `[SEAM_AND_RAIL_LAW]` spell `ContentAddress.Of(Node, Tolerance)`; disk is `Of(Node node, double tolerance)` (`Element/Projection/address.md:58`) — param is `double`, not a `Tolerance` type. The overload set (`Of(span)`, `Of(Node,double)`, `Of(UInt128)`, `OfGraph(ElementGraph)`) is all real and the DECISION correctly REFUTES the "overload split is a defect" claim (E11) — so this is spelling-only. FIX: drop the pseudo-`Tolerance` type spelling.

### [2.5]-[DRIFT] "eight sub-domain folders" (BRIEF:5) — disk has NINE
Kernel has 9 folders (Analysis, Domain, Drawing, Meshing, Numerics, Parametric, Processing, Solving, Spatial). FIX: `eight`→`nine`.

---

## [3]-[HANDROLL_COLLAPSE] — 2 genuine gaps, 1 interim-only

The [B] hasher re-anchor is the PRIMARY collapse and is well-built (all 9 leg-1/leg-2 cited sites CONFIRMED on disk: ledger:153, commits:91/:354/:375/:376/:401, provenance:291, timetravel:203/:204; the DEFENSIBLE-LOCAL carve-outs commits:119 MerkleRange, provenance Merkle rolling digests, timetravel ChainHash are correctly excluded). Canonical bytes correctly NOT re-derived — codec composes the seam `ContentAddress`, the "Persistence-local NodeHash/GraphHash forwarding owner is the deleted form" law confirmed (`codec.md:151,:157`).

### [3.1]-[GAP] `[B]` census MISSES 3 content-key-shaped raw sites
Raw `XxHash128.HashToUInt128` content-key re-derivations NOT in the [B] table nor carved out as defensible-local:
- `Query/lane.md:201` — `ElementSet(XxHash128.HashToUInt128(preimage.Span), …)` (element-set receipt key, leg 3).
- `Query/lane.md:212` — `Receipt(Seq<NodeId>)` element-set content key (leg 3).
- `Element/identity.md:246` — `StoreKey.Content(XxHash128.HashToUInt128(s.Material.Span))` (content StoreKey, leg 1).
These are content addresses that must compose `ContentHash.Of`/seam `ContentAddress` by the same one-hasher law as the [B] rows. FIX: extend the DECISION `[B]` table with these 3 (leg-1 identity, leg-3 lane), OR justify them defensible-local. (Codec's 6 framing/chunk sites :217/:318/:356/:437/:439/:448 are covered by the tier-level "codec-tier mints compose here, leg 1" [B] row + the :156 opaque-hash carve-out — acceptable.)

### [3.2]-[INTERIM] `ContentAddress.OfGraph(prior, delta)` two-arg hook is a PHANTOM
`[V9]` and the acceptance dry-run (a) "AS-OF scrub incremental-shaped" assume a delta-composable `OfGraph(prior, delta)`. On disk `OfGraph` is SINGLE-ARG `OfGraph(ElementGraph)` (`Element/Projection/address.md:69`); `delta.md:99` says the header key "composes, never re-spelled here" — no incremental hook exists. The DECISION frames this HONESTLY as a consumer contract ("the owner's to provide … Persistence composes on landing; whole-graph recompute the documented interim") and OUT_OF_SCOPE (Element campaign). No DECISION defect — but the leg-2 timetravel/scrub rebuild MUST author the INTERIM single-arg recompute, never the phantom two-arg member, or the fence cites a nonexistent owner. Worth a one-line guard in the DECISION `[02]` timetravel row.

### [3.3]-[CLEAN] V8 type-rekey premise verified
`element.md:50-51` `RootedType(ReadOnlySpan<byte>)` seed is Representations-EXCLUDED (`WriteObject(..., includeRepresentations:false)` :298-299) BUT includes `Classifications` (`:225-228,:298`) — so the Type seed is classification-DEPENDENT today, exactly the `[V8]a` premise. The `merge.Reconcile` `ObjectKind.Type` correlation-key row (`[V8]b`) is a real, needed collapse. Sound.

---

## [4]-[LEG_OPTIMIZATION] — keep 3 legs (no change)

All 25 pages are ONE package (`Rasm.Persistence`) ⇒ the engine's package-lane parallelism yields NOTHING (1 lane), and within a leg the build batches CHAIN by dependency (rebuild.js:574-592, no intra-leg build parallelism; only Discover pools). The 3 legs are therefore inherently SERIAL by the real dependency waterfall: Element spine (band-registry HOST on graph.md, `ContentHash` re-anchor root, `StoreActor` frame shapes defined leg 1) → Version engine (consumes registry + hashers + frame) → Query/Ingest/Store/Egress (consumes Element codec/identity + Version changefeed/outbox). The frozen-vocab contracts (`TimeCut`, `StorageTier`, `ObjectStore.Head`, `ServerExtension.CreateSql`) make cross-leg order safe by decision-time freeze. VERDICT: partition is dependency-sound, 4/7/14 is coarse (NOT mini-leg spam). No merge/split. The one unused affordance is multi-leg-one-run (`leg:[1,2,3]`) — optional session-overhead saving at a large context cost; the per-session cadence is the safe default. If leg-3 context is ever tight, the natural Query|Store split is frozen-vocab-safe, but not required.

---

## [5]-[CHAFF_CENSUS] — 4 items

- **BRIEF `[01]-[PHASE_ARCHITECTURE]` (:62-72) + `[EXECUTION]` STEP 1 (:13) are SPENT.** The design workflow (survey/draft/judge/salvage/emit) already ran and emitted the DECISION; STEP 1 `Workflow(design-cs-persistence.js…)` is done. For a launch-ready next session (Phase 2 legs only) this is historical scaffolding. Not deletable (durable input) but should be marked LANDED so the next session doesn't re-read it as pending.
- **BRIEF `[V1]-[V13]` verdict SHAPES are superseded by DECISION `[07]-[VERDICT_DISPOSITION]`.** The BRIEF's "recommended shapes, may exceed never dilute" verdicts are now RULED in the DECISION; keeping both is duplication (the BRIEF verdicts remain useful only as rationale trail).
- **Capability-escalation table duplicated:** BRIEF `[03]-[CAPABILITY_ESCALATION]` ≈ DECISION `[09]-[CAPABILITY_ESCALATION]` (near-verbatim). Each doc self-stands, so tolerable, but it is redundant surface.
- **Line-anchor saturation vs. rider SYMBOL law.** Both docs are dominated by `page.md:NNN` anchors; the engine forbids line-number rider anchors. Not chaff per se, but it forces the plan agent to reverse-map every rider to a symbol — the `[1.3]` typed-rider table resolves it. Minor style note: several `[02]` skeleton cells run 200+ words of nested parentheticals (e.g. rows 14/18/25); dense-agent house style, but the controlling contract could lead each cell.

---

## [TOP_BLOCKERS_TO_CLEAN_LAUNCH]
1. [1.1] Add `leg: N` to all 6 engine invocations (DECISION:299-301, BRIEF:14-16) — without it the acceptance-close machinery never arms.
2. [1.2] Author per-leg `acceptance` TRACE rows (`{name, needs:['<page>#<entry>']}`) — the (a)-(e) prose is not engine-consumable.
3. [1.3] Add a typed `[RIDERS]` table (catalog-delete api-messagepack, manifest-drop Grpc.Tools, 2 version edits, 4 verify) with SYMBOL anchors.
4. [2.1] Re-spell `NamingHashOps.Encode`/`Reconcile` (×3) → `Reconciliation.Apply(ReconcileOp.Encode|Reconcile)` — phantom kernel type.
5. [2.2] Re-anchor `Rasm/ARCHITECTURE.md:79`→`:90` and `:76-81`→`:87-92` (×5) — stale kernel seam anchors.
6. [2.3] Re-point the geometry-doc citations (6 occ) onto landed kernel anchors before the geometry BRIEF/DECISION archive.
7. [3.1] Extend `[B]` with the 3 missed content-key raw sites (lane:201/:212, identity:246).
