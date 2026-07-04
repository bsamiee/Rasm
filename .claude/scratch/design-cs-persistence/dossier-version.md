# DOSSIER — Persistence lane = VERSION

Scope: `libs/csharp/Rasm.Persistence/.planning/Version/` — `ledger.md` (439), `commits.md` (365/423 raw), `timetravel.md` (281), `merge.md` (387), `provenance.md` (388), `recovery.md` (312), `retention.md` (270). Register rows re-verified on disk: E8, E9 (ledger/timetravel/retention halves), E10 (merge side), E11, plus every register anchor landing in the seven pages (E1 TASKLOG:48, E4 commits band, E6 version seams, E13 recovery/retention cross-leg). All seam/verify-or-die members in-lane proved on disk against the landed `Rasm.Element` fences, the kernel `Rasm/.planning` fences, and the `.api` catalogs.

Fence interiors are genuinely 9-9.5 grade as the brief asserts. The perimeter breaks are real and, in two places, WORSE than the register states.

---

## [0] HEADLINE FINDINGS (highest value first)

**H1 — The register's "two live breaches" undercounts bare `Error.New`; `commits.md` is a THIRD un-banded owner.** SEAM_AND_RAIL_LAW and VERDICT-proof-3 assert `lane.md:432-441` is "the only un-banded owner" and name exactly "two live breaches (lane + provisioning)". FALSE. `commits.md` rails three bare `Error.New` in the 826x Commit band, on typed rails, with NO `[Union]` fault owner anywhere on the page:
- `commits.md:358` `.MapFail(static error => Error.New(8261, "<crdt-decode-drift…>"))` — on the `Fin<CrdtOp>` `Decode` rail.
- `commits.md:400` `Fin.Fail<ParityVector>(Error.New(8264, "<parity-owner-mints…>"))` — on the `Fin<ParityVector>` `Contribute` rail.
- `commits.md:411` `Validation<Error,Unit>.Fail(Error.New(8263, "<parity-drift…>"))` — on the `Reconcile` `Validation` rail.
The V4 band registry (`Commit 826x`, "register as-is") assumes Commit is already a typed band; it is NOT — the 826x codes are loose `Error.New` literals. A `CommitFault`/`CrdtWireFault` `[Union] : Rasm.Domain.Expected` must be MINTED (8261 decode-drift, 8263 parity-drift, 8264 owner-mints), not merely "registered as-is". This is the deleted form the corpus forbids, undetected by the census.

**H2 — The version engine mints ~10 raw `XxHash128` content keys that bypass the kernel `ContentHash.Of` entry, and the brief's re-anchor scope does not name any of them.** SEAM_AND_RAIL_LAW is categorical: "every Persistence digest mint composes the landed kernel `ContentHash.Of` seed-zero entry … a Persistence-local raw call site is the deleted form." The enumerated re-anchor targets are codec-only (`SnapshotHeader.Seal`, `ContentChunker`, `[V1]` plan, cache identity) — leg 1 / Element. The VERSION-ENGINE content-key mints are un-named and thus un-re-anchored by leg 2:
- `ledger.md:153` `XxHash128.HashToUInt128(payload.Span)` — `OpLogEntry.ContentKey` over the encoded `GraphDelta`.
- `commits.md:91` `XxHash128.HashToUInt128(canonical.WrittenSpan)` — `CommitNode` content key over the `Preimage`.
- `commits.md:354` `XxHash128.HashToUInt128(EncodeCompanion(op).Span)` — `CrdtWire.ContentKey` (the SANCTIONED delta hasher per `codec.md:152`, yet raw).
- `commits.md:375-376,401` — `ParityVector.Pin`/`Stamped`/`Contribute` raw hash the corpus the page's own prose (`:268,:272`) claims rides "the one kernel seed-zero `XxHash128` discipline."
- `provenance.md:291` `XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor))` — `AgentKey`.
- `timetravel.md:203-204` `XxHash128.HashToUInt128(e.ToCanonicalBytes(...).Span)` — edge content keys (see H3).
Verified value-identical: `Rasm.Element/Projection/address.md:48` `ContentAddress.Of(span) => Create(ContentHash.Of(span))` and kernel `Rasm/.planning/Domain/identity.md:31` `ContentHash.Of(span) => XxHash128.HashToUInt128(span)` seed-zero. So the re-anchor is pure call-path collapse ("digests are value-unchanged"), but leg 2 has no mandate to perform it. The DECISION must extend the SEAM_AND_RAIL_LAW re-anchor enumeration to the version-engine content-key mints, or the "one hasher entry" law is cosmetic across half the package's durable identities.

**H3 — `timetravel.md` edge keys use raw `XxHash128` where `merge.md` uses the seam `ContentAddress.Of` for the identical concept.** `merge.md:160` PropertyHash = `ContentAddress.Of(o.ToCanonicalBytes(tolerance).Span)` (seam, composes `ContentHash.Of`). `timetravel.md:203-204` EdgeDeltas key edges with bare `XxHash128.HashToUInt128(e.ToCanonicalBytes(tolerance).Span)`. Same preimage class (`ToCanonicalBytes`), two spellings — one seam-composed, one raw. `Rasm.Element/Projection/address.md:82` proves the seam already sorts edges by `ToCanonicalBytes` bytes; the edge key should be `ContentAddress.Of(edge.ToCanonicalBytes(tolerance).Span)`, not a raw instance. A concrete in-lane seam-bypass, not merely a scope gap.

---

## [1] PER-PAGE VERDICTS

### ledger.md — HOLD (9), perimeter-clean, two dead carriers

Interior sound: one-materializer changefeed projection off Marten `SubscriptionBase`; `SyncFault` (825x, 8251-8256) is a proper `[Union] : Rasm.Domain.Expected` (`:242-276`) — the ONLY correct typed band adjacency in the lane; the `(Hlc, OriginStoreId)` LWW tie-break, the `FirstWriter` inverse-comparison flip (`:303-311`), the exact-conservation `Apply` fold (`:323-337`), and the `TraceSlot` total-hex admission (`:81-89`) are all real.

- **E8 `:119` per-event sequential `RunAsync` → HOLD.** `ChangefeedSubscription.ProcessEventsAsync` at `:119`; the defect body is `:120-121` — `foreach … await sink(OpLog.Project(e)).RunAsync(EnvIO.New(...))` runs one IO per event instead of folding the range. Naivety axis (non-batched), not a correctness bug. Charter: fold the `range.Events.OfType<IEvent<GraphEvent>>()` projection into ONE `IO` and `RunAsync` once.
- **E9 `:31` `Truncate` minter-less → HOLD.** `SyncOpKind.Truncate` (`:31`, `wholeRelation:true`) has ZERO minters (`Project` at `:148-158` emits only `Delete`/`Upsert`; `Stamp` closes over a caller build) and ZERO `WholeRelation` (`:34`) readers (corpus grep clean). Dead pair. Charter: wire a whole-relation truncate op through `Apply`, or delete both `Truncate` and the `WholeRelation` column.
- **E9 `:149-153` `Codec` Family-derivable → HOLD.** `OpLogEntry.Codec` field (`:108`) is hardcoded `SnapshotCodec.JsonStj` in `Project` (`:149,:153`); the `crdt` lane implies `MessagePack`. Codec is a pure function of `Family`. Charter: collapse `Codec` to a `Family`-derived accessor unless a genuine bi-codec lane is named — which it is not.
- **E6 `:468` presence drain → HOLD.** `Awareness.AwarenessLane` (`:467-468`) opens `DrainSurface.Open<AwarenessBeat>` with the `onDrop` receipt. `ARCH:49` declares the `Version/ledger ⇄ Rasm.AppHost/Runtime [PORT]` (HLC + `TraceSlot`); the presence-beat producer (`Rasm.AppHost/Wire/companion` `PeerRoster` per V12) is the counterpart-obligation half, unstated own-side but the transport (`DrainSurface`) is composed correctly.

### commits.md — DRIFT (perimeter): un-banded faults (H1) + O(V²) MergeBase + two dead carriers

Interior sound: content-addressed commit-DAG, `Movable` polymorphic ACL gate on the `Element/identity#AUTHORITY` `GrantSet` (`:70-71`), the six-type CRDT algebra with total diagonal `switch` and the `_ => left/state` unreachable floor, the `CrdtOpWire` `[MessagePack.Union]` `[Key]` sequence flagship. `E12` confirmed on disk: no node-canonical golden pinned; the 4 parity slots at `:365-368` are `hlc-cell`/`commit-key`/`crdt-op`/`elementset` exactly.

- **H1 bare `Error.New` 826x → new defect (see [0]).** `:358,:400,:411`. No `[Union]` fault owner on the page.
- **E8 `:108-113` MergeBase O(common·rank) → HOLD (anchor exact).** `MergeBase` (`:108-114`) re-runs `Rank(resolve, candidate)` inside `common.Fold` (`:112`) — a full reverse-reachability walk PER common ancestor, O(|common|·V) on criss-cross histories, under a "true merge-base set" claim. `Rank` itself (`:132-141`) is the sanctioned mutable-worklist BFS kernel. Charter: one reverse-reachability generation-mark pass + a single dominance filter over `common` (near-linear); do not re-invoke `Rank` per candidate.
- **Dead carrier `VectorOrder`/`CommitGraph.Order` (`:37-43,:100-106`) → confirmed dead.** Corpus grep: `Order` is defined, never called; `VectorOrder` cases are produced only by `Order`. `VersionVector.Dominates`/`Join`/`Advance`/`At` carry all live concurrency logic. Charter: wire `Order` into a real concurrency consumer (the three-way merge base-selection is the natural site) or delete `VectorOrder` + `Order`.
- **Dead carrier `ContentParityCorpus.Seed = 0L` (`:381`) → confirmed dead.** Declared, prose-cited (`:268,:272`), never passed to any hash call — `Pin`/`Stamped`/`Op`/`Contribute` all use `XxHash128.HashToUInt128` (implicit seed-zero). The "one `Seed` convention" is prose over a raw call site (H2). Charter: fold `Seed` into the actual mint by routing through `ContentHash.Of`, or delete the const.
- **SEAM_AND_RAIL CRDT_OP_SET → design-pin, consistent.** Kernel `Rasm/.planning/Spatial/reconciliation.md:129` carries `CRDT_OP_SET → Rasm.Persistence/Version/commits#CRDT_ALGEBRA | DESIGN-PIN | the MvRegister/opMerge …`. On disk `ContentParityCorpus.Op(CrdtOp)` mints any op generically; no specifically-named `MvRegister`/`opMerge` divergent-delivery fixture is pinned. Correct for planning phase (matches DESIGN-PIN + `Option<UInt128>` digest gap). Leg-2 obligation: pin the concrete `MvRegister` op-set whose divergent-delivery folds converge byte-identically.

### timetravel.md — HOLD (9), one dead carrier, hot-path OfGraph confirmed, overload-split REFUTED

Interior sound: ONE materializer (`GraphDelta.ReplayOnto` via Marten `AggregateStreamAsync`), the two-axis RangeDiff (node `Inequalities` + edge content set-diff so a topology rewire never drops), monotone-predicate bisect, the non-cryptographic checkpoint chain that explicitly defers authenticity to `provenance#ATTESTED_LEDGER`.

- **E11 `:236` Scrub 2×/frame → HOLD (exact).** `ContentAddress.OfGraph(acc.Graph).Value` AND `ContentAddress.OfGraph(next).Value` per `ScrubFrame` (`:236`) — O(N·|graph|) per window. Charter: fold the incremental `OfGraph(prior, delta)` contract (frames already carry deltas) once the Element seam lands it; the whole-graph recompute is documented interim.
- **E11 `:258-259` Bisect per probe → HOLD.** Each `Search` probe fully reconstructs (`:253` `log.ReconstructAt(v)`); the flip locus reconstructs before+after with 2× `OfGraph` (`:258-259`). Charter: same incremental contract.
- **E11 `:187` `Of(Node,Tolerance)` vs merge `:160` span form → REFUTED as a defect.** BOTH overloads are real seam entries: `Rasm.Element/Projection/address.md:53` `Of(UInt128)`, `:58` `Of(Node,double)` id-INCLUSIVE, `:48` `Of(ReadOnlySpan<byte>)`, `:69` `OfGraph`. The split is INTENTIONAL: timetravel wants the id-inclusive node address (`:176` "the node's id-INCLUSIVE `ContentAddress`"), merge wants the id-exclusive content signature for re-key-tolerant change detection. No unification needed; not a defect. (The genuine issue is H3 — the EDGE keys, not the node keys.)
- **E9 `:16` `AggregateStreamToLastKnownAsync` composed nowhere → HOLD.** Listed in Packages (`:16`); the member IS real (`api-marten.md:171`) but never called in any operation (all reconstruction routes through `TimeLog.Reconstruct`/`ReconstructAt`/`Events`/`VersionAt`). Dead package-list entry. Charter: drop from the package list, or compose it as the head-state fold where `SnapshotHit` is read.
- **E6 AsOfKey absent → confirms E1 phantom.** No `AsOfKey` member anywhere in timetravel (or any Version/Element page — grep empty). `ARCH:50` `Version/timetravel ← python:data/gridded/virtual [CONTENT_KEY]: icechunk` targets this LIVE page but the member is unwired. V12 realizes it: `Checkpoint.Hash` (`:84`) IS the `AsOfKey`.

### merge.md — HOLD (8.5), the Type-rekey gap (E10) + STJ options (E8) both real

Interior sound: the re-ingest `Reconcile` GlobalId alignment BEFORE diff, the two-axis (Object-forest + content-node) three-way merge, the `MergeConflict.Evidence`/`Family` derived-logic collapse (`:111-125`), `always-succeeds-with-annotations` `MergeOutcome`, containment-cycle detection both sides.

- **E10 `:283` `ExternalKey` Object-`ExternalId`-only → HOLD (anchor exact).** `ExternalKey` (`:283-284`) matches ONLY `Node.Object { ExternalId: … }`; `Reconcile` (`:272-278`) correlates solely on it. A derived-`ObjectKind.Type` re-key (Type-seed amendment or foreign re-ingest of type definitions) carries no `ExternalId`, so it reads as delete-all + insert-all — the exact defect `Reconcile` exists to prevent. Charter: add a second correlation-key row for `ObjectKind.Type` nodes — a stable classification-independent Type natural key (the representation-excluded content seed identity mirroring `ExternalKey`'s shape) so a re-keyed Type diffs as RENAME.
- **E8 `:390` unthreaded STJ options vs `provenance.md:266` → HOLD.** `Patch` builds `new JsonPatchDocument()` (`:229`) with no options; the `insert` arm (`:390`) adds a real `Node` (a `[Union]`) as the patch value. Egress serialization needs the union-aware `ElementJson.Options` (Thinktecture converter set) that provenance threads at `:266` (`JsonSerializer.SerializeToElement(document, ElementJson.Options)`). Without it, the `Node` union + `NodeId`/`UInt128`/`Option` values do not round-trip. Charter: thread `ElementJson.Options` through the `JsonPatchDocument` construction/egress.
- **E8 MemberPath NodeId typing (`:246-249,:402-403`) → HOLD, member VERIFIED CLEAN.** `OwningNode` (`:249`) and `Pointer` (`:403`) pattern-match `seg.Value is NodeId`. `api-generator-equals.md:52-53` proves `MemberPathSegment` = `readonly struct` with `Kind` + `Value (object?)`, `MemberPathSegmentKind.Key` for a dictionary key; the `[UnorderedEquality]` `Nodes` `FrozenDictionary<NodeId,Node>` (`Rasm.Element Graph/element.md:404`) emits a `Key` segment whose `Value` IS the `NodeId` object (`:25` "dictionary-aware"). The `is NodeId` match succeeds; NO silent degradation to whole-node replace. The register's "unverified" concern resolves clean.
- **`ContentNodes` uses seam `ContentAddress.Of(span)` correctly (`:173`)** — the id-exclusive form, right for the content axis. Contrast H3 (timetravel edges) which does NOT.

### provenance.md — HOLD (9.5), the strongest fence in the lane

Interior sound: exact W3C-PROV-O typing (attribution off entity, association off activity, `WasRevisionOf` off parent-commit `OpKeys` never the geometry `Closure`), the independent-`digestOf` attested verify that makes `Unauthored` reachable (`:364,:372`), the Merkle transparency-log inclusion/consistency proofs, `ProvNode.Of` discriminating `kind.IsActivity` so a reached commit is never a mistyped Entity.

- **E14 `:257` unconditional `AgentClass.Person` → HOLD (out-of-lane register row, present in-page).** `ProvJson` stamps the bundle authority agent as `AgentClass.Person.ClassIri` unconditionally (`:257`), ignoring the `Principal`'s real agent class. Charter: read the authority's class off the `Principal` rather than hardcoding `Person`.
- **H2 raw mints:** `Bundle` (`:229-236`), `AgentKey` (`:291`), `AttestedLedger.Append`/`Pair` (`:348-354,:417-423`) are all raw `XxHash128`. `AgentKey` is a durable content key (durable PROV node identity) that must compose `ContentHash.Of`; `Bundle.Id`/`Append.Chain`/`Pair` are internal rolling/Merkle digests (defensible as local, but still raw call sites the categorical law names).
- **Card IDEAS:56 `DURABILITY_RECOVERY_OBSERVATORY` legit; provenance carries no phantom.**

### recovery.md — DRIFT (perimeter): the two verified RPO bugs, anchors moved

Interior sound: the real `(Timeline, Lsn)` `RecoveryPoint` via `Npgsql IdentifySystem` (verified `api-npgsql.md:89,96,194`), the six-step ranked restore choreography with generated `RestoreStep.Switch`, the `ReAttest` content-identity commit-point (`AggregateStreamAsync` `ContentAddress.OfGraph` compare + independent `DigestOf`). All Marten/Npgsql members verify against the catalogs (`FetchEventStoreStatistics`/`EventSequenceNumber` `api-marten.md:107`, `RebuildSingleStreamAsync` `:204`, `BuildProjectionDaemonAsync`/`StartAllAsync`/`WaitForNonStaleData` `:202-203`).

- **E8 count-as-minutes RPO → DRIFT (anchor `:181` → real `:180`).** `ObjectReplica` (`:177-180`) returns `Duration.FromMinutes(absent.Count)` — the COUNT of missing content keys expressed as MINUTES — feeding `objective.MeetsRpo(leg.Rpo)` at `:145` (fact) and `:150` (gauge), NOT `:151`. Contradicts the page's own freshest-blob-lag law (`:15`) and the `:174-176` comment restates the defect ("the count of content keys the replica still lacks as age"). Charter: compute real lag = freshest-replicated-blob age vs newest-event instant.
- **E8 segment-size-as-throughput → DRIFT (anchor `:172` → real `:171`).** `PgPitr` (`:171`) returns `Duration.FromSeconds(lagBytes / (16d*1024*1024))` — the 16-MiB WAL SEGMENT SIZE reused as a bytes-per-second THROUGHPUT rate. Charter: lift the WAL-throughput assumption into an explicit policy row (a configurable segment-rate), not a magic literal inside the RPO gauge.
- **E13 recovery ← blobstore `ObjectStore.Head` → DRIFT (anchor `:175,:179,:204` → real `:178` call + `:203` table).** `ctx.BlobStore.Head(ctx.BlobClient, key)` (`:178`); `RecoveryContext` imports `ObjectStore`/`ObjectClient` types (`:122`). Leg-2 recovery consuming leg-4 blobstore vocabulary — the V5c frozen-contract cross-leg edge; `ObjectStore.Head` delegate shape must be treated frozen or a leg-4 change reopens recovery as a hard residual.
- **Card TASKLOG:35 `RECOVERY_PAGE_AUTHOR` legit; IDEAS:56 legit.**

### retention.md — HOLD (9.5), one dead carrier, two frozen-contract imports

Interior sound: the six-row closed `RetentionClass` axis with five decisions, the fail-closed `RetentionCeiling` seam-local rank (`Unstamped` distinct from `CeilingBreach`), the full-history reachability `Mark` over EVERY `TimeCut` (never head), the ONE receipted executor (`evict` + `demote`) with the `Cool` cold-tier verdict, the exact `Conserves` partition. `RetentionFault` (828x, 8281-8283) is a proper `[Union] : Rasm.Domain.Expected` (`:70-92`).

- **E9 `:36` `StorageLane.Durable` unread → HOLD.** `StorageLane.Durable` property (`:36`) has ZERO readers — the only `.Durable` in the corpus (`merge.md:276`) is an unrelated tuple field (`move.Durable` in `Reconcile`'s remap). The lane routes by row identity, never `.Durable`. Charter: consume `.Durable` in a sweep/executor guard (e.g. reject a durable-lane eviction of a `Transient`-only artifact) or delete the column.
- **E13 retention ⇄ blobstore `StorageTier` + retention ← timetravel `TimeCut` → HOLD.** `RetentionFact.Tier` (`:100`), the `Colder` ladder (`:121-124`), `Demote` consumption (`:267`) all import blobstore `StorageTier`; `Mark(Func<TimeCut,…>, Seq<TimeCut>)` (`:241`) imports timetravel `TimeCut`. Two V5c frozen-contract edges — `StorageTier` rows and `TimeCut` cases must be frozen vocabulary the earlier legs consume, per the ruled default.
- **`RetentionCeiling.Rank` (`:103-106`) consumes AppHost `DataClassification` rows** (`None`/`Operational`/…/`Secret`) — the taxonomy-order-here law, correct.

---

## [2] REGISTER RE-VERIFICATION TABLE (in-lane rows)

| Row | Disk verdict | Anchor |
|---|---|---|
| E8 recovery count-as-minutes RPO | DRIFT | code `:180` (not `:181`); feeds `MeetsRpo` `:145`/`:150` (not `:151`); law `:15`; comment `:174-176` |
| E8 recovery segment-as-throughput | DRIFT | `:171` (not `:172`) |
| E8 commits MergeBase O(common·rank) | HOLD | `:108-114`, re-run at `:112` |
| E8 ledger per-event RunAsync | HOLD | method `:119`, body `:120-121` |
| E8 merge unthreaded STJ vs provenance:266 | HOLD | insert arm `:390`, construction `:229`; provenance `:266` |
| E8 merge/timetravel MemberPath NodeId typing | HOLD (verified clean) | merge `:249,:403`, timetravel `:284`; member real per `api-generator-equals:52-53` |
| E9 ledger Truncate minter-less | HOLD | `:31`, `WholeRelation :34` unread |
| E9 ledger Codec Family-derivable | HOLD | field `:108`, hardcode `:149,:153` |
| E9 retention StorageLane.Durable unread | HOLD | `:36` (merge:276 `.Durable` is unrelated) |
| E9 timetravel AggregateStreamToLastKnownAsync dead | HOLD | `:16` package list only; member real `api-marten:171` |
| E10 merge ExternalKey Object-only | HOLD | `:283-284`; `Reconcile :272-278` |
| E11 timetravel Scrub 2×/frame | HOLD | `:236` |
| E11 timetravel Bisect per probe | HOLD | per-probe `:253`, flip 2× `:258-259` |
| E11 Of(Node,Tolerance) vs span split | REFUTED (not a defect) | both real `address.md:53,:58`; distinct id-inclusive/exclusive semantics |
| E11 codec no-second-hasher law | HOLD (law), but breached by H2 | `codec.md:157-158` |
| E6 ARCH:50 icechunk AsOfKey | DRIFT | row targets LIVE `Version/timetravel`; member absent = unwired; deleted-page half is TASKLOG:48 `Version/snapshots` |
| E6 ledger presence drain | HOLD | `ledger.md:467-468`; PORT declared `ARCH:49` |
| E6 ARCH:55 reconciliation GeometryHash | DRIFT | row targets `Query/topology`; merge `GraphNode.GeometryHash` is a semantically-distinct representation-digest second consumer the row omits |
| E13 recovery ← blobstore ObjectStore.Head | DRIFT | call `:178`, table `:203`, types `:122` (not `:175/:179/:204`) |
| E13 retention ⇄ blobstore StorageTier / ← timetravel TimeCut | HOLD | `:100,:121-124,:241,:267` |
| E1 TASKLOG:48 ICECHUNK_ASOF phantom | REFUTED (phantom confirmed) | `Version/snapshots` + `SNAPSHOT_PROTOCOL` + `AsOfKey` all absent (grep empty) |
| E4 commits bare Error.New 826x (NEW) | new defect | `commits.md:358,:400,:411` — un-banded; undercounts "two live breaches" |

---

## [3] CROSS-CUTTING FINDINGS

**C1 — GeometryHash semantic collision (ARCH:55 ↔ merge).** `ARCH:55` declares the reconciliation adjacency-signature `GeometryHash` (kernel `Rasm/Spatial/reconciliation`) targeting `Query/topology`. But `merge.md` `GraphNode.GeometryHash` (`:73,:160`) is `GeometryDigest(Representations.ByIdentifier)` (`:305-315`) — a digest of the geometry-representation MAP, NOT the adjacency signature. Two distinct concepts share the name `GeometryHash`; the kernel `reconciliation.md` intends its adjacency bytes to BE the merge consumer's key, but on disk merge computes a representation-map rollup. The V8 geometry-brief unification must reconcile which `GeometryHash` `StructuralMerge` actually keys on, or the seam ledger points at the wrong producer. Surface to the DECISION seam table.

**C2 — Cross-leg inversions in-lane (all V5, all confirmed on disk).** (a) `Element/graph.md:228,240,267,270,302` consumes timetravel `TimeCut` (leg-1 graph ← leg-2 timetravel — V5d). (b) `recovery.md:178,122` consumes blobstore `ObjectStore.Head`/`ObjectStore`/`ObjectClient` (leg-2 ← leg-4 — V5c). (c) `retention.md:100,121-124,241,267` consumes blobstore `StorageTier` + timetravel `TimeCut` (V5c + intra-leg). Each is an earlier-leg page consuming a later-leg vocabulary; the DECISION's frozen-contract ruling must bind them or the leg partition is not acyclic.

**C3 — The un-banded/raw-hash cluster is systemic, not incidental.** H1 (commits `Error.New`) and H2 (10 raw `XxHash128` mints) together mean the two load-bearing perimeter laws — one type-enforced fault registry, one kernel hasher entry — are BOTH breached inside the version engine, and the brief's re-anchor/registry scope catches neither (`SyncFault`/`RecoveryFault`/`RetentionFault` are correctly banded; `commits` is not, and no version page routes hashing through `ContentHash.Of`). The DECISION's V4 registry map must MINT a Commit/CrdtWire band (not "register as-is"), and the SEAM_AND_RAIL_LAW hasher-re-anchor enumeration must add the version-engine content keys (`OpLogEntry.ContentKey`, `CommitNode` key, `CrdtWire.ContentKey`, `AgentKey`, timetravel edge keys, the parity corpus).

**C4 — Dead-carrier census (in-lane, beyond the register's E9 list).** Confirmed dead on disk: `SyncOpKind.Truncate`+`WholeRelation` (ledger `:31,:34`), `OpLogEntry.Codec` collapsibility (ledger `:108,:149,:153`), `StorageLane.Durable` (retention `:36`), `AggregateStreamToLastKnownAsync` (timetravel `:16`) — all in register. NOT in register: `VectorOrder`+`CommitGraph.Order` (commits `:37-43,:100-106`) and `ContentParityCorpus.Seed=0L` (commits `:381`). Both are genuine unwired carriers the census missed.

**C5 — Card verdicts (in-lane).** REFUTED/phantom: TASKLOG:48 `ICECHUNK_ASOF` (page+member absent). HOLD/legit: TASKLOG:35 `RECOVERY_PAGE_AUTHOR`, TASKLOG:38 `CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE` (`ledger#MERGE_LAW` `ConflictResult` real `:225`), IDEAS:55 `PERSISTENCE_BIM_SYNC_CRDT` (`commits#CRDT_ALGEBRA` `CrdtField`/`Crdt.Apply` real `:189,:212`), IDEAS:56 `DURABILITY_RECOVERY_OBSERVATORY`.

---

## [4] CHARTER-AS-IT-SHOULD-BE (DECISION obligations, version lane)

1. MINT a `CommitFault`/`CrdtWireFault` `[Union] : Rasm.Domain.Expected` on `commits.md` (826x: 8261 decode-drift, 8263 parity-drift, 8264 owner-mints); the V4 registry row for Commit is "MINT typed band", not "register as-is". Correct VERDICT-proof-3 / SEAM_AND_RAIL_LAW to "THREE live bare-`Error.New` owners (lane, provisioning, commits)".
2. EXTEND the SEAM_AND_RAIL_LAW hasher re-anchor enumeration to the version-engine content keys (H2 list) and schedule it in leg 2; route the parity corpus and `AgentKey` through `ContentHash.Of`, timetravel edge keys through `ContentAddress.Of(span)` (H3).
3. RULE the `merge` Type-correlation key row (E10/V8) and the `RangeDiff`/`Scrub`/`Bisect` incremental-`OfGraph` contract (E11/V9) as recorded consumer contracts; the whole-graph recompute is documented interim.
4. REPAIR the two recovery RPO gauges (E8): freshest-blob-lag for `ObjectReplica`, a WAL-throughput policy row for `PgPitr`; near-linear `MergeBase` (E8/V10).
5. DISPOSE the dead carriers: `Truncate`/`WholeRelation`, `Codec` collapse, `StorageLane.Durable`, `AggregateStreamToLastKnownAsync`, `VectorOrder`/`Order`, `ContentParityCorpus.Seed` — wire-or-delete each.
6. ADD the `AsOfKey` arm to `timetravel.md` (`Checkpoint.Hash` = `AsOfKey`), re-anchor `ARCH:50` onto the live page, kill TASKLOG:48 phantom.
7. RESOLVE the GeometryHash collision (C1) in the seam ledger; bind the C2 cross-leg frozen contracts.
