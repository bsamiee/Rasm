# [SYNTHESIS] RASM-CS-PERSISTENCE — the blueprint of record

BASE = `draft-folder-architecture-first` (the consensus winner). It is the capability-primary leader on every judge lens (9.5 law-gates · 8.5 contract-reach · 9.3 partition-elegance — tied-or-highest in all three), the ONLY draft whose roster disposition satisfies the binding INTEGRATION-FIRST/HARDENING mandate on the single most-contested axis (the six scale-out backends), and the only draft producing `[V13]`'s demanded first-class AXIS MAP. The two dissenting lenses (contract-reach→federation, partition-elegance→coordination) penalized folder for KEEP-on-asserted-ceilings and over-churn labels; both are absorbed as hardening grafts (per-package ceiling proof; churn-honest kinds), never as a base change — the dissent sharpens the roster and labels, it never drops the capability the mandate forbids dropping.

Every claim below re-verified on disk (hostile pass): `csproj:10` AppHost reference REAL; `commits.md` H1 bare `Error.New` 8261/8263/8264 with ZERO `CommitFault` union REAL; the H2 version-engine raw `XxHash128` mints REAL; every band collision + the 7701/7702 fourth breach REAL; the six scale-out packages present at pinned versions; `MembershipView.Serving`/`StepStateSeam.InFlight`/`CostUnit [SmartEnum<string>]` REAL; Ara3D 1.6.1 feed-CONFIRMED via the nuget MCP (the brief's "1.0.1 latest" refuted). The 30 cards (11 IDEAS + 19 TASKLOG) reconcile on disk.

FINAL PAGE-SET: 24 pages (Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3), zero one-file folders. Lowering: 6 `new` · 2 `rebuild` (identity, lane — the split-remainders) · 16 `improve` · 2 absorb pairs · 0 deletePages.

GRAFTS APPLIED (each survives disk check): [G1] the H2 hasher re-anchor enumeration table (§B, 3 judges); [G2] per-package `[V13]` ceiling proofs on the five kept scale-out axis rows (§D, 2 judges); [G3] churn-honest kinds — commits/recovery/provisioning `rebuild`→`improve`, identity/lane stay `rebuild` (2 judges); [G4] the four-wire-seam backbone S1-S4 as the seam-ledger organizing spine (§C, 2 judges); [G5] the `MembershipView.Serving` (AppHost:80) in-process consumer + the `CostUnit` string-key boundary precision (1 judge, disk-confirmed); [G6] injection as the PRIMARY strata resolution with the frame-to-kernel re-home demoted to a contingent KERNEL/APPHOST counterpart obligation (overturns law-gates' kernel-rehome-primary; 2 lens-observations + disk favor injection); [G7] the executable SQL fenced compare-and-decrement Budget predicate skeleton (1 judge, brief-grounded); [G8] Ara3D 1.0.1→1.6.1 feed-confirmed. Already-in-base and reinforced (no re-graft owed): Egress at 827x (tight Version-decade packing), 7701/7702 absorb, BIM:81 GeoParquet homed at `Query/columnar`, H1 Commit 826x MINT, the complete AppUi both-directions ledger, E9 GCS/Minio checksum honesty.

---

## [A]-[SHARED_RESOLUTION] — the cross-leg law every page inherits

### [A.1] STRATA — the `csproj:10` reversal, injection-primary

`Rasm.Persistence.csproj:10` carries `<ProjectReference Include="../Rasm.AppHost/Rasm.AppHost.csproj" />` (CONFIRMED on disk) — a DOWN/PEER edge reversing the strata (AppHost is a PORT peer Persistence contributes rows to). **RULING: REMOVE it** (leg-1 structural fix; the `dotnet restore` acyclicity gate confirms). The `[V2]` coordination op-union, fencing tokens, membership rows, Budget vector, and receipts are Persistence-OWNED types AppHost's `Wire/Coordination`/`Wire/Outbox` adapters DECODE — no AppHost type crosses down.

The frame ingredients currently flowing DOWNWARD (`graph.md:253` `actor` = `Rasm.AppHost/Agent/identity#PRINCIPAL`; `graph.md:219,256` `Run(... Principal actor, ClockPolicy clocks, CorrelationId correlation ...)`) resolve by INJECTION, not kernel re-home: `ClockPolicy`/`CorrelationId`/`TenantContext`/`RecoveryObjective` enter as injected `ProjectionContext`/`ResolvedProfile` shapes Persistence DEFINES and AppHost's composition root SUPPLIES; `CostUnit` crosses as its `[SmartEnum<string>]` STRING key ([G5] — `capability.md:54` confirms `CostUnit` is string-keyed, so the Budget row is `HashMap<string,long>` and AppHost maps its smart-enum at the boundary); `TraceSlot`/`TenantId` cross as wire values; `Principal` crosses as an injected value through the PORT adapter. Disk proof: `graph.md` already takes these as constructor VALUES, so injection is the natural, smallest-blast-radius fix. The stronger frame-re-home-to-kernel `Rasm` framing (coordination-first `[00]`; law-gates' G-LG-1) is DEMOTED to a contingent KERNEL/APPHOST counterpart obligation recorded in the ledger, NOT the Persistence primary — re-homing `Principal` (an AppHost identity type) into the geometry/identity kernel carries a larger cross-campaign blast radius than the injection form, and two partition-elegance observations plus disk favor injection. Effect: zero upward strata edges; the acyclicity gate closes.

### [A.2] SEAM BACKBONE — the four cross-runtime wire seams ([G4] organizing spine)

The own seam ledger is organized around the four cross-runtime seams every consumer resolves through the ONE kernel `ContentHash.Of` seed-zero entry, so a Persistence-owned type never mints a second identity:
- **S1 Substrait plan ingress → `SetExpr`** — owner `Query/federation.md` (NEW, V1). Wire = shipped-public `SubstraitDeserializer.Deserialize` (~2 lines), plan digest = `ContentHash.Of(wireBytes)`.
- **S2 content-addressed CloudEvents egress** — owner `Version/egress.md` (NEW, V3). `id`=`OpLogEntry.ContentKey`, `Sequence`=outbox cursor, `partitionkey`=`EntityKey`; per-sink dedup as composition.
- **S3 `AsOfKey`=`Checkpoint.Hash` icechunk seam** — owner `Version/timetravel.md` (IMPROVE, V12). ONE digest serving the icechunk cross-runtime seam AND the recovery content-identity oracle.
- **S4 `CrdtOpWire` bit-parity law** — owner `Version/commits.md` (IMPROVE, V4-MINT). `[MessagePack.Union]` `[Key]` sequence IS the wire; MINT `CommitFault` (H1); PIN `CRDT_OP_SET`; re-anchor raw mints.

### [A.3] FROZEN-VOCABULARY inversions (V5c/d) — the four cross-leg contracts

The DECISION freezes four vocabulary shapes at decision time so an earlier-leg page consumes the frozen CONTRACT, never the later leg's rebuilt page (acyclic by contract; a leg-4 change reopens the consuming leg as a hard residual): `TimeCut` (timetravel-owned; `graph` consumes), `StorageTier` (blobstore-owned; `retention` consumes), `ObjectStore.Head` (blobstore-owned; `recovery` consumes), `ServerExtension.CreateSql` (provisioning-owned; `cypher` consumes). The `Store/tier` neutral-vocabulary extraction is NOT taken (no leg-4 owner amends a frozen vocabulary mid-campaign — the deciding criterion fails). All four CONFIRMED on disk.

---

## [B]-[HASHER_RE_ANCHOR] — [G1] the H2 enumeration (leg-2 extension of the codec-only mandate)

`[SEAM_AND_RAIL_LAW]` enumerates codec-only re-anchor targets (leg 1). Disk proves the VERSION ENGINE mints raw `XxHash128` keys leg 2 has no mandate to touch. Value-identical (`ContentHash.Of(span) => XxHash128.HashToUInt128(span)` seed-zero), so pure call-path collapse. The DECISION EXTENDS the re-anchor to leg 2 as a decision-complete table:

| Mint | Anchor (disk-verified) | Re-anchor |
|---|---|---|
| `OpLogEntry.ContentKey` | `ledger.md:153` `XxHash128.HashToUInt128(payload.Span)` | `ContentHash.Of(payload.Span)` |
| `CommitNode` content key | `commits.md:91` `new CommitNode(XxHash128.HashToUInt128(canonical.WrittenSpan)...)` | `ContentHash.Of(canonical.WrittenSpan)` |
| `CrdtWire.ContentKey` | `commits.md:354` `=> XxHash128.HashToUInt128(EncodeCompanion(op).Span)` | `ContentHash.Of(EncodeCompanion(op).Span)` |
| `ParityVector.Pin/Stamped/Contribute` | `commits.md:375-376,401` raw `XxHash128.HashToUInt128(Canonical.Span)` | route through `ContentHash.Of`; fold the dead `ContentParityCorpus.Seed=0L` into the mint or delete |
| `AgentKey` (durable PROV node identity) | `provenance.md:291` `=> XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor))` | `ContentHash.Of(Encoding.UTF8.GetBytes(actor))` |
| edge content keys | `timetravel.md:203-204` | `ContentAddress.Of(edge.ToCanonicalBytes(tolerance).Span)` — the SEAM form (H3), matching `merge.md:160` |

Codec-tier targets stay leg 1 (`SnapshotHeader.Seal`, `ContentChunker`, cache identity, the V1 plan digest). DEFENSIBLE-LOCAL (not re-anchored): the `provenance` internal Merkle digests (`Bundle`/`Append`/`Pair` rolling `XxHash128` at `:229,:321` — a transparency-log construction, not a content-key mint). The blobstore multipart/chunk folds whose payloads outgrow a one-shot span compose the `identity.md` Growth-row streaming member (`XxHash128.Append` + `GetCurrentHashAsUInt128`, seed zero) on landing; the `ContentAddress.Of(UInt128)` wrap of a precomputed digest is the documented interim.

---

## [C]-[PAGE_SET] — 24 rows (path · action+lowering · owner charter/home/skeleton/band · entry · seams in⇄out · leg)

`kind` is the rebuild-engine lowering; `absorb {into,from}` carries the extraction; `deletePages`=0. Band = the `[F]` registry row. Seams list one anchor per crossing, both ledger directions. [G3] churn-honest kinds applied: identity/lane `rebuild` (split-remainders shed ~37%); commits/recovery/provisioning `improve` (the sound-surface pages the brief grades preserve-never-re-derive).

### `Element/` — 4 pages (the ElementGraph store-load roundtrip + identity + authz)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 1 | `Element/graph.md` | KEEP · `improve` | Store-rail root: one-materializer event sourcing (`GraphDelta.ReplayOnto`), co-txn identity commit, one-boundary fault conversion. **HOSTS the `[F]` `[FAULT_TABLES]` `FaultBand` `[SmartEnum<int>]` registry owner block.** Band **Graph 8300**. `Element/` | `GraphStore.Run(session, identity, GraphStoreOp, Principal actor, storeId, ClockPolicy, CorrelationId) : IO<Fin<GraphReceipt>>` + `FaultBand` registry | `← Rasm.Element` (ElementGraph SoR, ARCH:41); `← Rasm` (kernel seed-zero, ARCH:42); `← Rasm.AppHost/Runtime` (ClockPolicy/CorrelationId/TenantContext/Principal as INJECTED port-input VALUES, ARCH:54 — [G6]); `← timetravel#TimeCut` (frozen-vocab, V5d); `← APPHOST:71` (prepared-tx RETIRE counterpart — the single-`IDocumentSession` spine mints no `pg_prepared_xacts`); `→ Compute:115` (parse-to-canonical Extract counterpart) | 1 |
| 2 | `Element/codec.md` | KEEP · `improve` | Seam-composed `ContentAddress` (kernel `ContentHash.Of` seed-zero, no second hasher), CBOR trust boundary, single-pass atomic seal, FastCDC chunker. `[V10]` HashPolicy shrink to `{Identity,Content}` + `HashDomain` forward-compat byte law; kill `:175` `ByDomainId` prose drift. **THE kernel-hash re-anchor root** (all codec-tier mints compose here; §B). Band **Codec 8310** (831x/832x/833x stride). `Element/` | `SnapshotCodec` `[SmartEnum]` axis + `ContentAddress.Of/OfGraph/Of(UInt128)` seam-compose (NONE — vocabulary + static ops) | `← Rasm` (kernel seed-zero `XxHash128`, ARCH:42 — the ONE hasher entry); `→ typescript:wire` (SnapshotHeader canonical-CBOR, ARCH:43); `← Compute:111` (FastCDC content-key delta → `#CONTENT_CHUNKER`) | 1 |
| 3 | `Element/identity.md` | SPLIT · `rebuild` (absorb-out) | Relational identity tier (Marten doc), `IdentityPolicy` key axis, KMS custody (`SigningKeyring`+`EnvelopeKeyring` on the one `KmsProvider` axis, V5a — authz-vs-crypto split), `SchemaVerdict` boot fold. `[V6]` EF commit (`UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()`; hand `NodeId` converter + ~13 `HasColumnName` die; LanguageExt converters stay) + the DDL/migration owner (`element_identity`/`node_cell` DDL; `ServerExtension.CreateSql` commits through this rail; `EF.Design` admitted). Slims 653→~410 LOC. Band **Identity 8340**. `Element/` | `IdentityStore.Stamp` + `SchemaGate` + `IdentityPolicy`/`KmsProvider` axes (NONE — relational surface) | `⇄ Rasm.AppHost/Runtime` (TenantId RLS + KMS SigningKeyring/EnvelopeKeyring unwrap PORT — ARCH:53 SPLIT, KMS/RLS stay; APPHOST:72 unchanged) | 1 |
| 4 | `Element/authority.md` | NEW · `new` · `absorb {into: Element/authority.md, from: Element/identity.md#[04]-[AUTHORITY] (:287-341,465-484)}` | Object-ACL authz set-algebra: `Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor` — pure deny-over-allow set-algebra, zero `KmsProvider`, total. Band **NONE** (sub-bands under Identity 8340 only if a rail is later added). `Element/` | `Authority.Admit(GrantSet, AclScope) → Effective` (NONE — pure set-algebra) | `⇄ Rasm.AppHost/Runtime` (ObjectAcl identity-store PORT — the ARCH:53 `ObjectAcl` half moves HERE); `→ Version/commits#Movable` (ACL gate on GrantSet) | 1 |

### `Version/` — 8 pages (the engine projecting from Marten events; stays flat at 8)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 5 | `Version/ledger.md` | KEEP · `improve` | Changefeed projection off Marten `SubscriptionBase`, `(Hlc,OriginStoreId)` LWW, presence/awareness drain. `[V10]` `SyncOpKind.Truncate`+`WholeRelation` wire-or-delete; `OpLogEntry.Codec`→`Family`-derived accessor; `ProcessEventsAsync` batches the range into ONE fold (`:119`); `OpLogEntry.ContentKey` re-anchors `ContentHash.Of` (§B). Band **Sync 8250**. `Version/` | `OpLog.Project` + `SyncOpKind` `[Union]` + `ChangefeedSubscription` (NONE — projection surface) | `⇄ python:runtime/transport` (OpLogEntry CRDT delta, ARCH:48); `⇄ Rasm.AppHost/Runtime` (HLC+TraceSlot PORT, ARCH:49); `← Rasm.AppHost/Wire/companion` (PRESENCE PeerRoster beats over DrainSurface, ADD, ledger.md:468); `← Rasm.AppUi/Collab/Editing` (edit-intent projection + per-doc replay-window READ) + `← Rasm.AppHost/Runtime/determinism` (neutral-log window-read — ONE windowed-read case parameterized by origin/entity/window serves BOTH); `← BIM:101` (durable-annotation CDE); `← BIM:104` (Speckle Base import); `→ typescript:wire` | 2 |
| 6 | `Version/commits.md` | KEEP · `improve` ([G3]: was `rebuild`) | Content-addressed commit-DAG, six-type CRDT algebra, `CrdtOpWire` `[MessagePack.Union]` flagship (S4). **`[V4]/H1` MINT `CommitFault`/`CrdtWireFault : Expected`** — disk: `:358`/`:400`/`:411` bare `Error.New(8261/8264/8263)`, ZERO union today; NOT "register as-is". `[V10]` near-linear `MergeBase` (one reverse-reachability generation-mark pass + dominance filter, killing the per-candidate `Rank` re-run `:108-113`; BIM:94 the named consumer). §B raw-hash re-anchor (`CommitNode` `:91`, `CrdtWire.ContentKey` `:354`, parity corpus `:375-376,401`). PIN `CRDT_OP_SET` (`MvRegister`/`opMerge` converging byte-identically — `reconciliation.md:129` DESIGN-PIN → REAL). `[V10]` dead `VectorOrder`/`Order` + `ContentParityCorpus.Seed` wire-or-fold. Band **Commit 8260** (MINT). `Version/` | `Crdt.Apply` (`CrdtField` dispatch) + `CommitGraph.MergeBase` + `ContentParityCorpus` (NONE — static surfaces) | `→ typescript:wire` (CrdtOpWire msgpack union, ARCH:44); `⇄ python:runtime/transport` (None-companion + parity corpus, ARCH:45); `→ typescript:state` (ARCH:46); `← BIM:92/94` (align spelling to `commits#MergeBase`) | 2 |
| 7 | `Version/timetravel.md` | KEEP · `improve` | ONE materializer AS-OF (`GraphDelta.ReplayOnto` via `AggregateStreamAsync`), two-axis RangeDiff, monotone bisect, non-crypto checkpoint chain. **`[V12]/S3` `AsOfKey`=`Checkpoint.Hash` arm** (the field exists `:84`; ONE digest serving icechunk + recovery oracle; `recovery.md:65` `RecoveryPoint.AsCut()` resolves to it). §B edge keys → `ContentAddress.Of(span)` (H3, `:203-204`). `[V9]` Scrub/Bisect re-shape to incremental `OfGraph(prior,delta)` (interim whole-graph recompute documented). `[V10]` `AggregateStreamToLastKnownAsync` drop-or-compose. Band NONE (composes graph/codec digests). `Version/` | `TimeLog.Reconstruct/Diff/Scrub/Bisect` + `Checkpoint.Anchor` + `TimeCut` vocabulary (NONE) | `← python:data/gridded/virtual` (icechunk AsOfKey over the kernel seed, ARCH:50 — target LIVE, capability realized); `→ graph` (TimeCut FROZEN, V5d); `→ recovery#ReAttest` (AsOfKey oracle) | 2 |
| 8 | `Version/merge.md` | KEEP · `improve` | Re-ingest `Reconcile` GlobalId alignment before diff, two-axis three-way merge, `always-succeeds-with-annotations` `MergeOutcome`. `[V8]b` Type-correlation key row (classification-independent Type natural key mirroring `ExternalKey`, so a re-keyed Type diffs as RENAME; COUPLED to V8a — waits on the kernel seed OR ships its own classification-excluded seed). `[V10]` thread `ElementJson.Options` STJ through the `JsonPatchDocument` insert arm (`:390`). MemberPath NodeId typing VERIFIED CLEAN (`api-generator-equals:52-53`, E8). Band NONE (`MergeConflict` is a conflict class). `Version/` | `StructuralMerge.Reconcile` + `EditOp`/`EntityEdit` + `ExternalKey`(+Type key) (NONE — diff surface) | `← Rasm/Spatial/reconciliation` (GeometryHash RE-TARGETED OFF topology ONTO `Version/merge#STRUCTURAL_DIFF`; `GraphNode.GeometryHash` the named consumer, ARCH:55; adjacency-vs-Representations digest gap surfaced to the geometry `[V8]` counterpart, `Rasm/ARCHITECTURE.md:79` re-points); `→ typescript:wire` (JsonPatch RFC-6902, ARCH:47) | 2 |
| 9 | `Version/provenance.md` | KEEP · `improve` | Exact W3C-PROV-O typing, independent-`digestOf` attested verify (makes `Unauthored` reachable), Merkle transparency-log proofs — the federation-wide verify template. `[V10]` Principal-derived agent class (read the class off `Principal`, not unconditional `Person` `:257`). §B `AgentKey` re-anchor (`:291` → `ContentHash.Of`); the internal Merkle rolling digests (`:229,:321`) stay defensible-local. Band NONE (`AttestVerdict.Unauthored` is a verdict). `Version/` | `ProvNode.Of` + `AttestedLedger.Append/Verify` (NONE) | `← python:artifacts/provenance` (signed-artifact content-key binding + attested-ledger authority, ARCH:51 — the `[ARTIFACT_CONTENT_KEY_FEDERATION]` home); `→ Query/federation#SourceKind.SignedArtifact` (binding resolution) | 2 |
| 10 | `Version/retention.md` | KEEP · `improve` | Six-row `RetentionClass`, fail-closed `RetentionCeiling`, full-history reachability `Mark` over EVERY `TimeCut`, ONE receipted executor. `[V10]` `StorageLane.Durable` consume-in-a-guard-or-delete (`:36`). `[V5]c` FROZEN imports: `StorageTier` (blobstore-owned) + `TimeCut` (timetravel-owned). Band **Retention 8280**. `Version/` | `RetentionSweep.Run` + `RetentionClass` `[SmartEnum]` (NONE — one receipted fold) | `← Rasm.Compute` (content-keyed Assessment blobs, ARCH:52); `← Store/blobstore#StorageTier` (frozen-vocab, V5c); `← Version/timetravel#TimeCut` (frozen-vocab) | 2 |
| 11 | `Version/recovery.md` | KEEP · `improve` ([G3]: was `rebuild`) | Real `(Timeline,Lsn)` `RecoveryPoint` via Npgsql `IdentifySystem`, six-step ranked restore, `ReAttest` content-identity commit-point. `[V10]` the two verified RPO bugs: `ObjectReplica` real freshest-replicated-blob lag (killing `Duration.FromMinutes(absent.Count)` `:180`); `PgPitr` WAL-throughput → explicit policy row (killing 16-MiB-segment-as-rate `:171`). `[V12]/S3` terminal proof: reconstructed `OfGraph` == checkpoint `AsOfKey`, `RecoveryFault.VerifyFailed` unreachable on a clean restore. `[V5]c` FROZEN import `ObjectStore.Head`. Band **Recovery 8290**. `Version/` | `RecoveryRoutes.Run` + `RecoveryRoute` `[SmartEnum]` + `RestoreStep` (NONE — static choreography) | `← Rasm.AppHost/Runtime` (RPO/RTO objective INJECTED inputs, ARCH:64); `← Store/blobstore#ObjectStore.Head` (frozen-vocab, V5c); `← Version/timetravel#AsOfKey` (content-identity oracle) | 2 |
| 12 | `Version/egress.md` | NEW · `new` | **`[V3]/S2` CDC egress owner** (ruled site: beside the changefeed; `Sync/` revival refused for lack of a second sync sibling). ONE `EgressPump` fold draining the `[V2]` outbox cursor past `OpLogEntry` rows into `EgressSink` delivery rows under ONE CloudEvents envelope. Exactly-once EFFECT: `id`=`OpLogEntry.ContentKey`, `Sequence`=cursor `long`, `Partitioning.partitionkey`=`EntityKey`. Per-sink dedup as composition: `Nats` maps `id`→`Nats-Msg-Id` + Settle-ack advances; `Webhook`(`pg_net`) advances ONLY on `net.request_status`=SUCCESS reconciliation (PENDING holds, ERROR/timeout dead-letters, UNLOGGED crash → held cursor re-posts); wire-native gRPC/Arrow READS AppHost `HopPolicy` delivery-honesty. Sinks: webhook/nats/kafka(+serdes)/rabbitmq/pulsar/wire-native as seed DATA. Typed dead-letter + replay. Band **Egress 8270** (827x — tight Version-decade packing). `Version/` | `EgressPump.Drain(SyncCursor, EgressSink) → EgressReceipt` (`EgressSink` `[Union]` axis) | `← Store/coordination#OUTBOX_CURSOR` (drains the cursor — internal, no reverse edge); `← Version/ledger#CHANGEFEED` (OpLogEntry rows); `→ Rasm.AppHost/Wire/outbox` (keyed OutboundHop egress counterpart, APPHOST:73); reads AppHost `HopPolicy` (RASM-CS-APPHOST-BRIEF `[V9]`) | 4 |

### `Query/` — 7 pages (read lanes split by consistency demand + the reuse index)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 13 | `Query/lane.md` | SPLIT · `rebuild` (absorb-out) | Drops to routing + set-algebra: `ReadRouter` (sync authoritative vs async analytical), `StalenessWatermark` sequence-gap, `ElementSet`/`SetExpr`/`ElementSetAlgebra.Evaluate` + length-framed `Canonical` parity contribution. The `FUSION_AND_CACHE`+`VECTOR_CODEBOOK` ANN subsystem leaves to `retrieval`. Route `WaitForNonStaleProjectionDataAsync` through `IProjectionDaemon`/`IMartenDatabase`, not `TestingExtensions`. Band NONE (set-algebra total post-split). `Query/` | `ReadRouter.Route` + `ElementSet.Evaluate(SetExpr, SetResolve)` (`SetExpr` `[Union]`) | `⇄ python:data/tabular/query` (ElementSet receipt currency STAYS here, ARCH:57a); `→ Query/federation#SetExpr` (lowering target); `→ retrieval#VECTOR_CODEBOOK` (absorb-out) | 3 |
| 14 | `Query/retrieval.md` | NEW · `new` · `absorb {into: Query/retrieval.md, from: Query/lane.md#[04]-[FUSION_AND_CACHE]+#[05]-[VECTOR_CODEBOOK] (:267-520)}` | The coupled ANN subsystem: `FusionRank.Fuse` (weighted n-ary reciprocal-rank fusion, pgvector SQL binding) + `VectorCodebook.Train`/`AdcScan` (PQ k-means, coarse→fine ADC scan). **`[V4]/V5b` MINT `RetrievalFault : Expected`** (kills the lane codebook's bare `Error.New(8360..8363)` — disk-confirmed, the only un-banded lane owner). `[V6]` pgvector `<->`/`<=>` LINQ server-side ANN row beside the in-process PQ/ADC hot-set lane (residency is row data on the one retrieval axis). Band **Retrieval 8410**. `Query/` | `Retrieval.Fuse` + `VectorCodebook.Train`/`AdcScan` (`RetrievalOp` `[Union]`) | `⇄ Rasm.Compute/Model/embedding` (VECTOR_CODEBOOK — `VectorRow.ContentKey` ↔ `EmbeddingVector.ContentKey`; COMPUTE:99 declares, post-split targets `retrieval`); `← pgvector` column map; `→ cache` fusion result | 3 |
| 15 | `Query/topology.md` | KEEP · `improve` | In-process QuikGraph view, frozen incidence, the DEFAULT synchronous topology owner. **`[V10]/NEW` `Lca` DAG-gate** (`Lca`/`Ancestor` pre-gate `tree.IsDirectedAcyclicGraph()` and rail `TopologyFault.Cyclic`, symmetric with the already-gated `Order`; `api-quikgraph:26` flags `OfflineLeastCommonAncestor` unsound over cyclic input; topology models containment cycles as real). `[V10]` `TypedEdge.IsContainment`/`Kind` dead-accessor prune. Band **Topology 8370** (8370-8371, keeps 837x). `Query/` | `TopologyView.Of` + `TopologyQuery` `[Union]` | `← Rasm.Bim` (soft anchors → cache); `→ Element seam` (per-snapshot `ContentAddress.OfGraph` memo stays). **ARCH:55 NO LONGER targets topology** (re-pointed to merge) | 3 |
| 16 | `Query/columnar.md` | KEEP · `improve` | ONE in-process DuckDB engine, co-transactional `BimOpenSchemaProjection : FlatTableProjection` (Persistence-owned generic branch — DECISION rules this, aligns ARCH:56; Bim supplies the typed schema seed only), ParquetSharp, ADBC. `[V10]/E14` typed trust gate (`Identifier`/`StorePath`/`SecretName` value-object family gates `Mount`/`Secret`/`Egress {projection}`/`Generation`; `{projection}`→composed typed projection, never raw SQL). `[V10]` posture constant (the four `"80%"`/`"90%"` literals `:80-83` collapse to one). `[V10]` phantom-spellings closed (`ExecuteQueryAsync`→`ExecuteQuery`; `BimData.WriteDuckDB`→`frames.ToDataSet().WriteToDuckDB`). **ADD `ColumnarExtension.Substrait` row** (DuckDB `from_substrait(blob)`, V1 lane). Band **Columnar 8350** (8350-8356, the WHOLE decade — collision winner). `Query/` | `Columnar.Run(ColumnarQuery)` over `ColumnarProfile`/`ColumnarExtension` axes | `← Rasm.Bim/Model` (BimOpenSchema seed, ARCH:56); `⇄ python:data/tabular` (Arrow over ADBC, ARCH:62); `← BIM:81` (GDAL/OGR GeoParquet columnar ingest — HOMED, the 13th sibling row [G-partition]); `← Query/federation` (tabular-subtree execution) | 3 |
| 17 | `Query/cypher.md` | KEEP · `improve` | Injection-safe server-side `format('%L')` composition, three-space result discipline (AGE→ElementSet, pgrouting→H3Cell, pgr_bridges→raw long). **`[V4]` `GraphFault`→`CypherFault` RENAME** (disk-confirmed simple-name ×2: `graph.md:154` `GraphFault` ns `Rasm.Persistence` vs cypher ns `Rasm.Persistence.Query`; graph keeps `GraphFault`). `[V10]` `AgtypePath.Weight`-always-0.0 dead carrier (populate or collapse to `Seq<NodeId>`). Band **Cypher 8360** (8360-8363, keeps 836x). `Query/` | `Cypher.Run(GraphQuery)` (`GraphQuery` `[Union]`) | `← Store/provisioning#ServerExtension.CreateSql` (frozen-vocab install rail, V5c); optional/demoted beneath QuikGraph | 3 |
| 18 | `Query/cache.md` | KEEP · `improve` | The compute-result reuse index: `ArtifactIndexRow` blob index + `ModelResultIndex` recency horizon + `BenchmarkRow` claim gate, sync-over-async `IBufferDistributedCache` L2. The `cache.md:20` one-`ArtifactKind`-row growth law is load-bearing for Compute/Fabrication/Materials durable rows (accepted `IDistributedCache` boundary — batching note, never a rebuild). Band NONE (storage leg). `Query/` | `Cache.Lookup/Register` over `ArtifactKind` axis + `CacheL2Store:IBufferDistributedCache` | `← Rasm.Compute` (ArtifactIndexRow/ModelResultIndex/BenchmarkRow INDEX, ARCH:58); `⇄ Rasm.AppHost/Runtime` (L2 partition CACHE_PORT — TenantId RLS + IBufferDistributedCache, ADD, APPHOST:69) | 3 |
| 19 | `Query/federation.md` | NEW · `new` | **`[V1]/S1` REINTRODUCE** (probe FEASIBLE_WITH_GAPS — retire NOT triggered; a router/lowerer over standing lanes, no second engine). Substrait protobuf bytes → `Substrait.Protobuf.Plan.Parser.ParseFrom` + `new SubstraitDeserializer().Deserialize` (shipped PUBLIC, ~2 lines — falsifies `api-flowtide-substrait.md:151`) → a `LoweringTarget` `RelationVisitor` (~150-250 LOC) onto `SetExpr` (SetRelation→Union/Intersect/Difference 3-for-3, ReadRelation+Filter→Predicate, VirtualTableRead→Literal, bounded Iteration→Closure, key-semijoin Join→Intersect) OR the columnar/ADBC lane (Project/Aggregate/Sort/TopN/Window→columnar; Write→REJECT fail-closed; Exchange→DROP). Plan digest = `ContentHash.Of(wireBytes)` (never a local `XxHash128`). Executes against ONE `TimeCut`; returns the `ElementSet` receipt + Arrow batch; the `(plan-digest·cut·watermark)` triple content-addresses the result; replay is one `ArtifactKind` reuse row. `SourceKind` capability axis (Substrait-native durable-store/signed-artifact/ADBC vs SQL-only staged). DROP `Grpc.Tools`; ADD DuckDB substrait extension row; `Google.Protobuf@3.35.1` (shared, admitted) the sole runtime dep. Band **Federation 8420**. `Query/` | `Federation.Execute(FederationPlan, TimeCut) → FederatedResult` (`FederationPlan` on `SourceKind`) | `← python:data/tabular/query` (Substrait portable-plan half, ARCH:57b SPLIT, signature-locked; ElementSet currency stays lane); `→ Query/lane#SetExpr` + `→ Query/columnar` (internal lowering/execution); `← provenance#SignedArtifact`; `← BIM:91` (federation AuditEntry log counterpart) | 3 |

### `Ingest/` — 2 pages (the one-file-folder violation ends; a real codec-ingress axis)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 20 | `Ingest/tabular.md` | KEEP · `improve` | One rectangular-data owner: MiniExcel spreadsheet + **`Sep` as EXPLICIT owner law** (a fenced SIMD delimited-lane surface, the `[V11]` charter made real). `[V10]/NEW` `TabularWire.Wire` disposition (compose the `DynamicExcelColumn` `CustomFormatter` into `Policy().DynamicColumns`, OR delete `Wire` — the STJ `Bind<T>` is the single wired path). `linq2db.EntityFrameworkCore` CONDITIONAL: composes a real BulkCopy fence → KEEP, else LEAVES (deciding evidence: DuckDB `COPY` + Npgsql binary import already own the bulk lane, so absent a composed fence it leaves). `[V4]` `TabularFault` RE-BAND off 837x. Band **Tabular 8390**. `Ingest/` | `TabularSource.Run(TabularSpec)` (`[Union]`) | `→ Rasm.Element` (row-shape only, ARCH:61); `← Compute:115` (Extract counterpart → graph/Ingest) | 3 |
| 21 | `Ingest/schedule.md` | NEW · `new` | **`[V11]` schedule-file codec** (`MPXJ.Net` .mpp/XER/PMXML → the record rail) + durable schedule rows (the Persistence half of the relocated Bim schedule domain — TASKLOG:47 moved the CPM/4D DOMAIN to `Rasm.Bim`; the file codec + durable store is ingest-shaped). Mints because BIM:102 supplies the durable-schedule consumer (fold-up alternative NOT taken). Band **Schedule 8400**. `Ingest/` | `ScheduleSource.Run(ScheduleSpec)` (`[Union]`) | `← Rasm.Bim/schedule` (P6/MS-Project 4D → HOME, BIM:102 counterpart); `→ Rasm.Element` (schedule→element row-shape) | 3 |

### `Store/` — 3 pages (durable-home + coordination substrate)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 22 | `Store/blobstore.md` | KEEP · `improve` | Nine-delegate `ObjectLeg` (four providers fill once), write-once 412-noop seal, `RemoteStoreFault` structural `Lift`, write-blob-first + full-history GC. `[V10]` `MultipartTransfer.Upload`+`BlobTransferReceipt` become THE composed receipt path (or both die). **[G8]/E9 checksum honesty** (`ObjectChecksum` axis has `XxHash128`/`Crc64`/`None` rows `:32-37`; GCS/Minio rows read the SDK-native `Crc64`/`None` stance, NOT the decorative `XxHash128` only `S3Leg.Seal` supplies `Integrity.Wire`). `[V10]` `BlobResidence.Correlation` thread-from-write-op-or-drop. `StorageTier` FROZEN vocabulary (V5c — blobstore-owned; `Store/tier` extraction NOT taken). Band **RemoteStore 5400**. `Store/` | `BlobStore.Put/Get/Seal` over `ObjectStore` `[SmartEnum]` (4 providers) | `← Rasm.Compute` (GLB by RepresentationContentHash, content-keyed write-first, ARCH:59); `← Rasm.Bim/Exchange` (IFC/BREP IfcRepHash GLB, ARCH:60); `→ Rasm.Compute GeometrySource` (`Placement` egress row, C2, currently prose — rowed explicit); `← Rasm.AppUi/Collab/sync` (snapshot-accelerator rows, content-keyed derivable-class retention, never SoR) | 4 |
| 23 | `Store/provisioning.md` | KEEP · `improve` ([G3]: was `rebuild`) | Verification-first six-command `NpgsqlBatch` fold, `FailureRank.Absorb` absence policy, `ExtensionAdmission` install preconditions, `EngineOps` `Handle`-bridge capsule, embedded residency-split ritual. **`[V4]` `ServerFault` RE-BAND off 835x** (Columnar keeps 835x; ServerFault→838x, absorbs the loose `Error.New` 8371-8375/8379/8380 as typed cases — disk-confirmed). **`EmbeddedStore.Refused` 7701/7702 → `EmbeddedFault.Refused`** (disk-confirmed at `:433/:440`, the fourth breach the E4 census omitted, in-banded to Embedded 771x). `[V6]` embedded charter HONEST (Marten PG-only → `StoreProfile.Embedded` (SQLite) carries NO ElementGraph event-sourcing path — the real charter is relational identity floor + `EngineOps` checkpoint/snapshot/backup + read-only/offline tier, never SoR). `ServerExtension.CreateSql` FROZEN install rail (V5c). `Npgsql.OpenTelemetry` observability row. Bands **Server 8380** + **Embedded 7710**. `Store/` | `ClusterProvision.Verify/Admit/Reload` + `EmbeddedStore.Open` over `ServerExtension`/`StoreProfile` axes | `← Rasm.AppHost/Observability` (Npgsql-ONLY HEALTH_PROBE, ARCH:63; APPHOST:85 drops Redis/Kafka); hosts the `[V13]` embedded/KV-floor + SoR-spine axes | 4 |
| 24 | `Store/coordination.md` | NEW · `new` | **`[V2]` the four AppHost PORT contracts** on ONE token-VALIDATING fenced-lease store. `CoordinationOp` `[Union]`: `BudgetDebit`, `StepStateCas`+`StepStateInFlight`(READ)+`StepStateLoad`, `LeaseAcquire`/`Renew`/`Release`+`ExpiredScan`(READ), `MembershipUpsert`/`MembershipScan`(READ), `OutboxAdvance`. **[G7] Budget = fenced predicated compare-and-decrement, PER-UNIT VECTOR** — `UPDATE budget_ledger SET balance_i = balance_i - debit_i WHERE tenant = @t AND lease_token >= @held AND balance_i >= debit_i` FOR EVERY requested unit atomically `RETURNING` (N unit balances mirroring `CostVector`, `capability.md:54-62`; a scalar debit is falsified by the multi-unit consumer; a stale token is a typed `LeaseFenced`, never a lost update). Kleppmann fencing: the guarded write REJECTS a token older than the row's lease generation via the SAME row-CAS predicate. READ cases beside guarded writes (`StepStateInFlight` for `CrashResume` `orchestration.md:271,282`; `ExpiredScan` orphan-reclaim). **`ONE_OUTBOX_EGRESS_SPINE` NAMED** (the Marten event stream IS the outbox — same-`IDocumentSession` guarantee; mints the durable drain cursor + at-least-once advance; `[V3]` egress consumes it). Composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — never a second event store, never a distributed-lock sidecar. Band **Coordination 8430**. `Store/` | `Coordinate.Run(CoordinationOp, Option<LeaseToken>, TenantContext, ClockPolicy, CorrelationId) : IO<Fin<CoordinationReceipt>>` | `⇄ Rasm.AppHost/Agent/capability` (fenced per-tenant Budget debit, APPHOST:76); `⇄ Rasm.AppHost/Runtime/orchestration` (step-state CAS + InFlight, APPHOST:77); `⇄ Rasm.AppHost/Wire/outbox` (transactional outbox same-tx, APPHOST:78); `⇄ Rasm.AppHost/Wire/Coordination` (CAS+lease+membership, APPHOST:79) — **`MembershipView.Serving` (AppHost:80, `Wire/coordination.md:79`) is the in-process consumer of the membership rows [G5]**; `→ Version/egress#OUTBOX_CURSOR` (the pump drains it, no reverse edge) | 4 |

---

## [D]-[BAND_REGISTRY] — `[V4]` the re-partitioned 83xx map (duplicate fails at type-init)

ONE `FaultBand` `[SmartEnum<int>]` sited on `Element/graph.md#[FAULT_TABLES]` (the store-rail root every rail composes), mirroring `.archive/…DECISION.md:141-149`. Each fault union derives `Code => Band + n`; a duplicate band integer FAILS at type initialization. Every loose receipt code is a typed case or registered sub-band. Per-page decade prose (`topology:166`, `columnar:128`, `cypher:49`) dies for one registry pointer. Lands COMPLETE in leg 1; each union re-derives in its owning leg.

### Own bands (19 unique own decades; all three collisions + Commit re-partitioned)

| Band | Union | Owning page | Codes | Disposition |
|---|---|---|---|---|
| 5400 | `RemoteStoreFault` | `Store/blobstore` | 540x | register as-is |
| 7710 | `EmbeddedFault` | `Store/provisioning` | 7711-7714 **+ Refused (7701/7702 absorbed)** | register + absorb the census-missed loose 770x cluster (disk-confirmed `:433/:440`) |
| 8250 | `SyncFault` | `Version/ledger` | 8251-8256 | register as-is (the one correct typed band on disk) |
| 8260 | `CommitFault`/`CrdtWireFault` | `Version/commits` | 8261,8263,8264 | **MINT typed `[Union]:Expected`** (H1 — disk: bare `Error.New`, ZERO union; NOT "register as-is") |
| 8270 | `EgressFault` | `Version/egress` | 8271-8273 | **NEW** (827x — the free Version-decade gap between Commit and Retention) |
| 8280 | `RetentionFault` | `Version/retention` | 8281-8283 | register as-is |
| 8290 | `RecoveryFault` | `Version/recovery` | 829x | register as-is |
| 8300 | `GraphFault` | `Element/graph` | 8300-8302 | register as-is (keeps the simple name; cypher renames) + registry HOST |
| 8310 | `CodecFault` | `Element/codec` | 8310/8320/8330 | register as-is (legal 831x-833x multi-decade stride) |
| 8340 | `IdentityFault` | `Element/identity` | 834x | register as-is (authority composes it, no new band) |
| 8350 | `ColumnarFault` | `Query/columnar` | 8350-8356 | KEEP 835x — the WHOLE decade (7 cases, disk-confirmed), collision winner |
| 8360 | `CypherFault` | `Query/cypher` | 8360-8363 | KEEP 836x + **RENAME from `GraphFault`** — collision winner + simple-name ×2 resolved |
| 8370 | `TopologyFault` | `Query/topology` | 8370-8371 | KEEP 837x — collision winner |
| 8380 | `ServerFault` | `Store/provisioning` | re-banded 838x | **RE-BAND off 835x** + the loose `FailureRank`/readiness/`Admit` 8371-8375/8379/8380 become typed cases |
| 8390 | `TabularFault` | `Ingest/tabular` | re-banded 839x | **RE-BAND off 837x** (was 8370-8373, three-way collision) |
| 8400 | `ScheduleFault` | `Ingest/schedule` | 840x | **NEW** |
| 8410 | `RetrievalFault` | `Query/retrieval` | 841x | **MINT** (kills the lane codebook bare `Error.New(8360..8363)` — disk-confirmed the only un-banded lane owner) |
| 8420 | `FederationFault` | `Query/federation` | 842x | **NEW** (SubstraitParse + unsupported-relation fail-closed + WriteRejected) |
| 8430 | `CoordinationFault` | `Store/coordination` | 843x | **NEW** (incl. `LeaseFenced`, `CasConflict`, `BudgetExhausted`, `MembershipLapsed`) |

No-band pages (total algebras / verdict cases / storage legs): `Element/authority`, `Version/timetravel`, `Version/merge`, `Version/provenance`, `Query/lane`, `Query/cache`.

### Pinned-mirror foreign rows (cross-package disjointness is a row, never prose)

AppHost 1xxx/4100-4810 · Compute 2200-2299 + Remote `WireFault` 4520-4532 · AppUi 6xxx · AEC registry 2300/2350/**2400 (mirror of kernel `GeometryFault` 2400-2449)**/2450/2470/2500/2600/**2700→2701-2710** · kernel-substrate `Fault.UnsupportedCode` 9104. Precedent: RASM-CS-APPHOST-BRIEF `[V1]` reciprocal pin.

---

## [E]-[SEAM_LEDGER] — [G4] organized by the four wire seams + own corrections + ADD + 12 sibling counterparts

### Own-row corrections (ARCH:41-64)

| Row | Correction |
|---|---|
| ARCH:50 icechunk AsOfKey (**S3**) | KEEP target (LIVE `Version/timetravel`); capability realized — `AsOfKey`=`Checkpoint.Hash` (V12); the phantom was the member, not the page |
| ARCH:53 identity KMS PORT | SPLIT — `ObjectAcl` store → `Element/authority`; TenantId RLS + KMS keyrings → `Element/identity` (APPHOST:72 unchanged) |
| ARCH:54 frame ingredients | **[G6]** re-express as INJECTED port-input shapes Persistence defines (ClockPolicy/CorrelationId/TenantContext/Principal as VALUES, not a reference); csproj:10 AppHost ProjectReference DELETES; frame-to-kernel re-home recorded as a contingent KERNEL/APPHOST counterpart obligation, not the primary fix |
| ARCH:55 reconciliation GeometryHash | RETARGET `Query/topology` → `Version/merge#STRUCTURAL_DIFF` (topology zero reconciliation refs; merge's `GraphNode.GeometryHash` the consumer); adjacency-vs-Representations digest gap → geometry `[V8]` counterpart (`Rasm/ARCHITECTURE.md:79` re-points) |
| ARCH:56 columnar BimOpenSchema | RE-SCOPE "Bim-implemented" → Persistence-implemented generic `BimOpenSchemaProjection : FlatTableProjection`; Bim supplies the typed schema seed only |
| ARCH:57 lane ElementSet + Substrait (**S1**) | SPLIT — ElementSet currency stays `Query/lane`; the Substrait portable-plan half → `Query/federation` (V1 reintroduce) |
| ARCH:63 provisioning HEALTH_PROBE | KEEP Npgsql-only; the stale side is APPHOST:85 (drops Redis/Kafka) |

### ADD (wired-undeclared own rows + minted-owner seams)

- `Query/retrieval ⇄ Rasm.Compute/Model/embedding` VECTOR_CODEBOOK (post-V5b; COMPUTE:99 declares)
- `Query/cache ⇄ Rasm.AppHost/Runtime` CACHE_PORT L2 partition + TenantId RLS (APPHOST:69 declares; distinct from ARCH:58 Compute INDEX)
- `Version/ledger ← Rasm.AppHost/Wire/companion` PRESENCE PeerRoster beats over DrainSurface (ledger.md:468 — the roster produces, DrainSurface transports; Persistence-owned wire row, no AppHost type down)
- `Version/ledger#CHANGEFEED ← Rasm.AppUi/Collab/Editing` edit-intent projection + per-doc replay-window READ **AND** `← Rasm.AppHost/Runtime/determinism` neutral-log window-read — ONE windowed-read case parameterized by origin/entity/window serves BOTH (RASM-CS-APPUI `[V2]` / RASM-CS-APPHOST `[V10]`; text-container contingency rides the EXISTING `CrdtOpWire` `RgaSequence`, zero new wire row)
- `Store/blobstore ← Rasm.AppUi/Collab/sync` snapshot-accelerator rows (content-keyed, derivable-class retention, never SoR)
- `Store/blobstore#Placement ⇄ Rasm.Compute GeometrySource` (C2 — prose-only today, rowed explicit)
- The four `Store/coordination` PORT counterparts (APPHOST:76-79) + **`MembershipView.Serving` AppHost:80 in-process consumer [G5]**
- `Version/egress → Store/coordination#OUTBOX_CURSOR` (internal cursor edge, forward-only — coordination never reads the pump — **S2**)
- `Query/federation → Query/lane#SetExpr` + `→ Query/columnar` (internal lowering/execution edges — **S1**)

### The twelve stale sibling → Persistence rows (counterpart obligations; sibling interiors OUT of edit scope)

| Sibling row | Dead target | Corrected target | Campaign |
|---|---|---|---|
| COMPUTE:111 FastCDC content-key delta | `Sync` | `Element/codec#CONTENT_CHUNKER` | Compute |
| COMPUTE:115 parse-to-canonical Extract | `Query/pipeline` | `Element/graph#STORE_RAIL` / `Ingest` | Compute |
| COMPUTE:116 anomaly rule source | `Store/quality` | `Query/columnar` + `Store/provisioning` verification surfaces | Compute |
| COMPUTE:119 Protobuf Kafka topics | `Sync` | retire with the `[V3]`/`[V7]` streaming disposition | Compute |
| BIM:91 federation AuditEntry log | `Query/federation` | `Query/federation` (V1 REINTRODUCED — owner re-lives) | Bim |
| BIM:95 IFC validation rules | `Store/quality` | `Query/columnar` + `Store/provisioning` quality surfaces | Bim |
| BIM:101 durable annotation + CDE | `Sync/annotation` | `Version/ledger#CHANGEFEED` | Bim |
| BIM:102 P6/MS-Project 4D | `Sync/schedule` | `Ingest/schedule.md` (V11 NEW) | Bim |
| BIM:104 Speckle Base import | `Sync` | `Version/ledger` (Speckle SyncTransport) | Bim |
| APPHOST:71 drain 2PC in-doubt | `Query/transaction` | `Element/graph#STORE_RAIL` — prepared-tx RETIRE (the single-`IDocumentSession` spine mints no `pg_prepared_xacts`) | AppHost |
| APPHOST:73 keyed OutboundHop egress | `Sync/egress` | `Version/egress.md` (V3 NEW) | AppHost |
| APPHOST:85 health-probe driver roster | Npgsql/Redis/Kafka | `Store/provisioning`, Npgsql-only (matches ARCH:63) | AppHost |

Additional (un-enumerated, recorded): BIM:81 GDAL/OGR GeoParquet → `Query/columnar` (HOMED, the 13th sibling row [G-partition]); BIM:94 `CommitGraph.MergeBase` spelling → align to `commits#MergeBase` (static, no `CommitGraph` type).

---

## [F]-[ROSTER_DELTA] — INTEGRATION-FIRST; [G2] per-package `[V13]` ceiling proofs; `.api` obligations in the same motion

Central owner `Directory.Packages.props` Persistence block (`:251` label-grouped, hand-edited) + `Rasm.Persistence.csproj`. Manifest motion lands in leg 1 (every ruled prune is zero-page-consumer). License gate: OSS/free-for-OSS-commercial admissible; pay-tiered REJECTED. A leaving row takes its `.api` catalog + README group row + csproj `PackageReference` + AppHost probe row in ONE motion.

### PRUNE (each on a per-package charter/redundancy proof — never a bare zero-consumer default)

| Package | Proof |
|---|---|
| `PollinationSDK` (`:307`) | charter misplacement — a domain cloud SDK with no store-class concern |
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`, `Npgsql.NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage` | no persisted NTS geometry column; H3 rides `pocketken.H3` on the transitive core-NTS crossing (`api-h3.md:3` corrects) |
| `Microsoft.EntityFrameworkCore.Sqlite` | the embedded floor is raw ADO by design (`provisioning.md:369-434`) |
| `StackExchange.Redis` (direct) | the L2 swap row keeps `Microsoft.Extensions.Caching.StackExchangeRedis` (an `IBufferDistributedCache`), which transits the driver |
| `ScyllaDBCSharpDriver` (`:311`) | **the single scale-out prune** — `[V13]` wide-column "only with a named owning axis"; the store perimeter's closed axes carry NO wide-column owner and no consumer names one (SoR spine + columnar + vector + embedded/KV + object + egress cover every store class) |

### KEEP as `[V13]` axis rows — [G2] per-package ceiling PROOF (INTEGRATION-FIRST, not asserted)

The `[V13]` STORE_AXIS_MAP is first-class content; every store-class concern names its axis, provider rows (deployment/policy DATA), and selection policy. Each kept scale-out row carries the PROVEN ceiling the in-PG/in-process owner cannot reach ([G2] hardening the base's one-line assertions per the contract-reach + partition-elegance demand; the brief `[V13]` requires the ceiling proved, not defaulted). Every provider row carries its provisioning/health/recovery posture through the verification-first fold.

| Axis | Owning page | Provider rows (seed DATA) | Selection | Scale-out ceiling proof |
|---|---|---|---|---|
| Object store | `Store/blobstore` | s3 · azure-blob · gcs · minio | `ObjectStore` `[SmartEnum]` | (landed) |
| Egress sink | `Version/egress` | webhook · nats · kafka · rabbitmq · pulsar · wire-native | `EgressSink` `[Union]` | `[V3]` |
| Read-lane/analytic engine | `Query/columnar` | duckdb-in-process · pg_duckdb-in-PG · **clickhouse-scaleout** | `ColumnarEngine` axis | `ClickHouse.Driver` KEEP (`:267`) — the horizontally-sharded columnar MPP row for billion-row cross-model OLAP the single-node in-process DuckDB engine and in-PG TimescaleDB provably cannot own at cluster scale (distributed merge-tree, not a second SoR) |
| Lakehouse interchange | `Query/columnar` | ducklake(extension, forward) · **delta** | format row | `DeltaLake.Net` KEEP (`:276`) — the Delta transaction-log wire for external-warehouse interop (Spark/Databricks lakehouse) DuckDB's in-process catalog cannot emit; a format row, not an engine |
| Vector search | `Query/retrieval` | pgvector-in-PG · pgvectorscale-diskann · pq-adc-in-process · **qdrant-scaleout** | `VectorBackend` axis | `Qdrant.Client` KEEP (`:308`) — the billion-scale sharded ANN row (quantization + horizontal shard routing) over the in-PG pgvector/pgvectorscale ceiling at corpus scale |
| Embedded/KV floor | `Store/provisioning` | sqlite(raw-ADO) · **rocksdb-lsm** · **lmdb** | `EngineOps`-tier row | `rocksdb`+`LightningDB` KEEP (`:310`,`:285`) — write-optimized LSM (`rocksdb`) + read-optimized memory-mapped B+tree MVCC (`LightningDB`) `EngineOps`-tier rows over the transactional-SQLite floor the single-writer WAL engine cannot match on high-write-fan-out / zero-copy-read workloads |
| Relational SoR spine | `Store/provisioning` + `Element/graph` | postgres-18 (SINGULAR) | SEALED | the ONE event store · ONE materializer · ONE identity · ONE changefeed — unchallengeable; `ARCHITECTURE.md:115` re-scopes to exactly this spine boundary |

### `[V3]` streaming/messaging sink rows (INTEGRATION-FIRST — each leaves ONLY on a per-package redundancy proof)

KEEP-COMMIT the spine anchors: `NATS.Net`, `CloudNative.CloudEvents`(+`.SystemTextJson`), `pg_net` (extension row — server-side webhook). KEEP as sink ROWS (default): `Confluent.Kafka` + `Confluent.SchemaRegistry`(+`.Serdes.Avro`/`.Json`/`.Protobuf`) with `Chr.Avro`(+`.Binary`+`.Confluent`) as the codec column, `CloudNative.CloudEvents.Kafka`, `RabbitMQ.Client`, `DotPulsar`. A broker row leaves only on proven redundancy against an admitted sink of the same delivery semantics or feed-verified abandonment.

### `[V1]` federation rows (KEEP — probe FEASIBLE)

`FlowtideDotNet.Substrait@0.15.0` (`:281` — the shipped-public `SubstraitDeserializer.Deserialize` is the transcription; `api-flowtide-substrait.md:151` corrected), `Apache.Arrow.Adbc` + `.Drivers.{Apache,BigQuery}` (source rows), `Apache.Arrow.Flight` (bulk-result hop). **ADD** a DuckDB `substrait` community-extension row (`ColumnarExtension.Substrait` — extension row, NOT a NuGet; verify a build for the pinned DuckDB 1.5.x runtime, fail-closed on unsupported relations). **DROP** the `Grpc.Tools` codegen assumption (`:55` shared) — unnecessary AND counterproductive (regenerating substrait `.proto` mints a duplicate CLR `Plan` the shipped deserializer rejects by identity); `Google.Protobuf@3.35.1` (`:47`, already admitted) is the sole runtime dep.

### `[V6]`/`[V11]`/observability/Ara3D

- COMMIT (`[V6]`): `EFCore.NamingConventions`, `Microsoft.EntityFrameworkCore.Design` (earns admission at the identity DDL/migration owner), `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`, `Pgvector.EntityFrameworkCore` (the `<->`/`<=>` LINQ server-side ANN row on `Query/retrieval`).
- `MPXJ.Net@16.4.1` (`:296`, `[V11]`): KEEP — `Ingest/schedule.md` mints (BIM:102 consumer).
- `linq2db.EntityFrameworkCore` (CONDITIONAL): composes a real BulkCopy fence in `Ingest/tabular` → KEEP, else LEAVES (DuckDB `COPY` + Npgsql binary import own the bulk lane).
- `Npgsql.OpenTelemetry` (ruled default KEEP): the provisioning observability row (tracing+metrics builder subscriptions at the AppHost composition root).
- **[G8] `Ara3D.BimOpenSchema`(+`.IO`): KEEP on the live columnar consumer + BUMP 1.0.1 → 1.6.1** — nuget MCP CONFIRMED 1.6.1 published 2026-07-03 (the brief's "1.0.1 latest" parenthetical is feed-REFUTED; verified-local wins). Re-decompile at 1.6.1 on the bump: retire the DEBUG-IL caveat if `AssemblyConfiguration("Debug")` is gone, else escalate to in-corpus absorption.

### `.api` catalog obligations (leg-1 motion)

Re-anchor all 77 package catalogs to LIVE pages (the dead-page re-anchor map: `Store/{profiles,remote,encryption,quality,server,tenancy}`, `Query/{federation,pipeline,transaction,rail,lanes}`, `Version/snapshots`, `Sync/*`, `Schema/*` → live). The 21 anchor-less (not 16 — disk-corrected) gain anchors, core spine `api-marten`→graph/provisioning, `api-npgsql`→provisioning, `api-objectstore`→blobstore first-class. `api-npgsql.md` catalog GAP: add advisory-lock (`pg_advisory_xact_lock`) + LISTEN/NOTIFY rows (V2 lease + V3 pump wake); `Npgsql.Replication` pgoutput stays recorded-unconsumed. `CompleteMultipartUploadRequest.ChecksumXXHASH128`: confirm via `assay api` (AWSSDK.S3) before the blobstore leg closes (the two enum members already verified). `api-flowtide-substrait.md:151` FLIP + re-anchor to `Query/federation`; `SubstraitSerializer` internal (retain wire bytes). Delete the misfiled shared `api-messagepack.md` (byte-identical dup `shasum 5527490f`) or re-scope `RASM_PERSISTENCE`→neutral `RASM_API`. UNION the 9 double-cataloged overlays into shared owners FIRST, then collapse to one-line pointers (8 DIVERGE — never blind-delete). `api-h3.md:3` correct the NTS-IO claim → transitive core-`NetTopologySuite` 2.6.0. Uniform `<!-- catalog:Pkg@ver -->` + `[STACKING]` across the kept tier (currently 4/77 + 0/31). `api-languageext.md` DISCHARGED (verify-only). Admissions carry `.api` stubs in the same motion (DuckDB substrait extension row overlays `api-duckdb`).

### csproj dependency-reversal (PLACEMENT_LAW / strata gate)

`Rasm.Persistence.csproj:10` `<ProjectReference Include="../Rasm.AppHost/Rasm.AppHost.csproj" />` (CONFIRMED) REMOVES — see [A.1].

---

## [G]-[VERDICT_DISPOSITION] — V1-V13

| V | Ruling |
|---|---|
| V1 FEDERATION | REINTRODUCE `Query/federation.md` (probe FEASIBLE_WITH_GAPS overturns the wire-form premise in the owner's favor). Shipped-public `SubstraitDeserializer.Deserialize` (~2 lines) → `RelationVisitor` lowering onto `SetExpr` + columnar/ADBC; plan digest = `ContentHash.Of(bytes)`; `SourceKind` axis; DROP `Grpc.Tools`; ADD DuckDB substrait extension row. Retire NOT triggered (no second engine). Cards honest-BLOCKED. |
| V2 COORDINATION | NEW `Store/coordination.md` — token-VALIDATING fenced lease; [G7] per-unit-vector fenced compare-and-decrement Budget; step-state CAS + READ cases (InFlight/ExpiredScan/Members/StepLoad); membership lease-expiry ([G5] MembershipView.Serving consumer); NAMED outbox spine + cursor. csproj strata reversal resolved ([A.1]). Store→3. |
| V3 EGRESS | NEW `Version/egress.md` (beside the changefeed — no `Sync/` revival). ONE pump over the outbox cursor; exactly-once-EFFECT envelope (`id`=`ContentKey`, `Sequence`=cursor, `partitionkey`=`EntityKey`); NATS dedup + Settle-ack advance, pg_net response-reconciled advance; sink rows as seed DATA. Egress→coordination edge acyclic. Band 827x. |
| V4 BAND REGISTRY | `[SmartEnum<int>]` on `graph.md#[FAULT_TABLES]`; 19 own bands + pinned mirrors ([D]). **Commit 826x MINTED** (H1 — disk-confirmed, NOT "register as-is"). Cypher renames `CypherFault`. Loose codes (incl. 7701/7702) → typed cases. Three collisions re-partitioned (Server→838x, Tabular→839x, Retrieval 841x). |
| V5 SPLITS+CYCLES | (a) `identity`→+`authority` (authz set-algebra). (b) `lane`→+`retrieval` (fusion+codebook, RetrievalFault). (c) `StorageTier`/`ObjectStore.Head`/`ServerExtension` stay owner-frozen. (d) `TimeCut` timetravel-frozen. All four inversions bind as frozen contracts consumed across legs; a leg-4 change reopens the consuming leg as a hard residual. |
| V6 EF+EMBEDDED | COMMIT `UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()`; hand `NodeId` converter + ~13 `HasColumnName` die; LanguageExt converters stay. DDL/migration owner on `Element/identity` schema section (`EF.Design` admitted). Embedded charter HONEST (relational + `EngineOps`, never SoR — Marten PG-only). pgvector `<->`/`<=>` LINQ = server-side ANN row on `retrieval`. |
| V7 SCALE-OUT+ORPHAN | INTEGRATION-FIRST: 5 scale-out KEEP as `[V13]` axis rows with per-package ceiling PROOFS ([G2]); ScyllaDB PRUNE (no wide-column axis owner). PollinationSDK/4 NTS bridges/EF-Sqlite/direct-Redis PRUNE in one leg-1 motion. SoR-spine seal stands. |
| V8 TYPE-REKEY | (a) consumer contract RECORDED (exclude `Classifications` from the Type seed — Element/Materials owns; Persistence the demanding consumer; `element.md:295` writes Classifications unconditionally today). (b) `merge.Reconcile` gains the `ObjectKind.Type` correlation-key row NOW (classification-independent natural key → RENAME); executing wave stays `identity#IDENTITY_POLICY` expand/flip/contract. |
| V9 INCREMENTAL OFGRAPH | consumer contract on `codec#CONTENT_ADDRESS` (`OfGraph(prior,delta)` is `Rasm.Element/Projection/address`'s; Scrub/Bisect re-shape; whole-graph recompute interim). Geometry `[V2]` parametric digest waterfalled SHARPENED (a COMPONENT of `ToCanonicalBytes`, not a sibling key — else `OfGraph` blind to a parametric-body edit, a false green on the AsOfKey/OfGraph proofs). Kernel hasher never re-homed. |
| V10 DEFECT SET | all repairs land with the owning page's rebuild ([C]): recovery real RPO lag + WAL policy row; near-linear MergeBase; ledger Truncate/Codec/batch fold; merge STJ options + MemberPath verified clean; blobstore receipt-path + [G8] checksum honesty + Correlation; retention StorageLane.Durable; provenance Principal-agent-class; columnar trust gates + posture constant + phantom-spellings; codec HashPolicy shrink; timetravel AggregateStreamToLastKnown. PLUS H1/H2/H3 + provisioning 7701/7702 + GCS/Minio checksum + topology Lca DAG-gate + tabular Wire. |
| V11 INGEST GROWTH | `tabular` KEEPS (Sep explicit owner law; linq2db conditional). NEW `Ingest/schedule.md` (MPXJ codec + durable rows; BIM:102 the named consumer clearing the deciding criterion). Folder law satisfied (2 pages); fold-up NOT triggered. |
| V12 GOVERNANCE | every phantom-realization clause corrects; the stale `-[COMPLETE]` cards re-point ([I]); `[FABRICATION_PROGRAM_DURABLE_ROWS]` re-anchors off dead `Schema/ddl#IDENTITY` onto blobstore rows + cache artifact index (2701-2710 decode constraint, KIND-AGNOSTIC ArtifactKind rows); ledger both directions complete ([E], incl. both AppUi crossings); AsOfKey/reconciliation/Substrait own rows corrected; prepared-tx RETIRE. |
| V13 STORE AXIS | store perimeter PARAMETERIZED — the full AXIS MAP first-class ([F]) with per-package ceiling proofs [G2]. SoR spine SINGULAR (one event store/materializer/identity/changefeed — unchallengeable seal); `ARCHITECTURE.md:115` re-scopes to that spine boundary (a perimeter-axis engine row carrying unreachable capability is a legal DECISION-level admission, never "a new relational engine row"). |

---

## [H]-[EVIDENCE_DISPOSITION] — E1-E14 (re-verified on disk; drifts carried)

| E | Status | Disposition (disk correction carried) |
|---|---|---|
| E1 phantom realization | V1/V12 | HOLD; +un-enumerated IDEAS:57-61, TASKLOG:46-47; `:44`↔`:45` contradiction resolves (SchemaDdl.Sql → V6 migration owner) |
| E2 orphaned roster | V7/V13 | DRIFT — PROPS anchors -2; **Ara3D "1.0.1 latest" REFUTED (1.6.1 feed-confirmed [G8])**; scale-out present at pinned versions |
| E3 catalog drift | V7/ROSTER | DRIFT — anchor-less is 21 not 16; messagepack byte-identical dup confirmed |
| E4 band collisions | V4 | DRIFT — Columnar 8350-8356 (whole decade); 837x THREE-way; **+commits H1 (826x bare `Error.New`, disk-confirmed)**; +provisioning 7701/7702 (fourth cluster, disk-confirmed `:433/:440`) |
| E5 coordination gap | V2 | HOLD — bilateral; AppHost fully-specified (`orchestration.md:257-282` StepStateSeam, `capability.md:54-62` CostVector, `Wire/coordination.md:79` MembershipView.Serving) |
| E6 seam-ledger drift | V12 | HOLD — ARCH:55 target DRIFT → Version/merge; all 12 sibling rows HOLD; +C1 GeometryHash digest-semantics gap |
| E7 EF stack | V6 | HOLD — identity `:82`/`:120` mapping conflict; MigrationBuilder=0 (no DDL owner); embedded no ElementGraph path |
| E8 verified logic bugs | V10 | DRIFT anchors — recovery `:180`/`:171`; MergeBase `:108-113` HOLD; MemberPath VERIFIED CLEAN |
| E9 dead carriers | V10 | HOLD — +commits `VectorOrder`/`Order` + `ContentParityCorpus.Seed`; +AgtypePath.Weight, TypedEdge.Kind, TabularWire.Wire; [G8] GCS/Minio dishonest checksum |
| E10 type-rekey | V8 | HOLD — sharper at `element.md:295` (WriteObject writes `Classifications` UNCONDITIONALLY → Type seed classification-dependent today; V8a↔V8b coupling) |
| E11 OfGraph hot paths | V9 | HOLD — Of(Node,Tol)-vs-span overload REFUTED as a defect (both real, distinct id-inclusive/exclusive semantics); the genuine issue is H3 (timetravel EDGE keys raw) |
| E12 mandate clean | V12 | HOLD — count-prefix ZERO-structural; re-band gate; Fabrication 2701-2710 forward constraint on IDEAS:26-32 |
| E13 folder+page overload | V5/V11 | DRIFT — Ingest one-file CONFIRMED; lane 520 LOC (retrieval span `:267-520`); Sep prose `:5,18,20,297` |
| E14 parameterization | V10 | DRIFT — columnar surface WIDER (`{projection}` raw SQL, `{stamp}`); posture literals `:80-83` confirmed |

Beyond-register findings disposed: H1 (commits MINT, §D), H2 (version-engine hasher enumeration, §B), H3 (timetravel edge keys, §B), provisioning 7701/7702 (EmbeddedFault.Refused), GCS/Minio checksum honesty [G8], codec `:175` ByDomainId prose (dies), topology Lca DAG-gate, tabular Wire dead carrier, columnar 2 phantom spellings, `api-flowtide-substrait.md:151` FALSIFIED, csproj:10 reversal ([A.1]).

---

## [I]-[CAPABILITY_ESCALATION] — the [03] plane targets (Phase-2 acceptance bar)

| Plane | Now→Target | Carried by |
|---|---|---|
| Element (graph/codec/identity+authority) | 9→9.5 | authority extraction; EF commit + snake-case + generated converters; DDL/migration owner; band registry sited; V9/parametric/streaming-identity consumer contracts; §B hash re-anchor; HashPolicy ruling |
| Version core (ledger/commits/timetravel) | 9→9.5 | near-linear MergeBase; dead vocab wired-or-dead; batched fold; AsOfKey=Checkpoint.Hash; CRDT_OP_SET pin; Scrub incremental; §B version-engine re-anchor |
| Version merge | 8.5→9.5 | Type-correlation key; MemberPath verified; STJ options; re-key migration case |
| Version governance (provenance/retention/recovery) | 9→9.5 | real RPO lag; WAL policy row; Principal-agent-class; StorageLane.Durable |
| Query read lanes (lane/topology/columnar/cypher/cache) | 8.5→9.5 | retrieval extracted; CypherFault rename; columnar trust gates + posture + substrait row + phantom-spellings; pgvector; topology Lca gate |
| Query retrieval (split) | —→9.5 | fusion + PQ codebook + RetrievalFault; Compute codebook seam declared |
| Query federation (new) | 0→9 | Substrait→SetExpr lowering; public-Deserialize ingress; SourceKind; ADBC lane; cut-pinned receipt; python:data wire; honest cards |
| Ingest (tabular+schedule) | 6.5→9 | folder law; Sep explicit; MPXJ codec + durable rows |
| Store (blobstore/provisioning) | 9→9.5 | receipt path; [G8] checksum honesty; loose codes typed (incl. 7701/7702); README bridge row |
| Store coordination (new) | 0→9.5 | token-VALIDATING lease; [G7] fenced per-unit-vector Budget CAS; CAS/membership op-union ([G5] MembershipView consumer); named outbox + cursor; four PORT rows homed |
| Changefeed egress (new) | 0→9 | one pump; exactly-once envelope; sink dedup honesty; dead-letter; replay |
| Governance perimeter | 3→9.5 | zero phantoms; ledger both directions (both AppUi crossings); 77 catalogs live; zero orphans; type-enforced registry |

---

## [J]-[FEDERATION_PROBE] — honored (FEASIBLE_WITH_GAPS → REINTRODUCE)

Wire transcription is shipped-public (~2 lines: `Substrait.Protobuf.Plan.Parser.ParseFrom` + `new SubstraitDeserializer().Deserialize`) — `api-flowtide-substrait.md:151` FALSIFIED. `Grpc.Tools` DROPPED (a regenerated `Plan` is rejected by identity); `Google.Protobuf@3.35.1` the sole runtime dep. Transcription-heavier-than-lowering retire trigger runs OPPOSITE (transcription ≈2 lines, lowering ≈150-250 LOC / reference 139 LOC). One-engine test PASSES (SetExpr for key-selection; existing DuckDB + ADBC for tabular — no new engine). G3 tabular: ADD `ColumnarExtension.Substrait` row (verify vs pinned DuckDB 1.5.x, fail-closed) + `SourceKind` axis (Substrait-native committed; SQL-only warehouses staged via `AdbcStatement.SqlQuery`). Roster: KEEP FlowtideDotNet.Substrait + ADBC(Apache/BigQuery) + Arrow.Flight; ADD DuckDB substrait extension row. Three BLOCKED cards re-anchor honest.

---

## [K]-[CARD_DISPOSITION] — all 30 (11 IDEAS + 19 TASKLOG; disk-verified counts)

### IDEAS (11 — 4 open + 7 closed)

| Card | Status | Disposition |
|---|---|---|
| :19 `[PERSISTENCE_LIBRARY_TABLES]` | QUEUED | KEEP QUEUED honest; re-anchor onto real durable owners (blobstore content rows + cache `ArtifactKind` index + identity/coordination tier); lands when Materials catalogue owners pin content keys |
| :26 `[FABRICATION_PROGRAM_DURABLE_ROWS]` | QUEUED | KEEP QUEUED; re-anchor off dead `Schema/ddl#IDENTITY` onto blobstore content rows + cache artifact index + `[V2]`/Store growth axis; KIND-AGNOSTIC `ArtifactKind` rows (programs/.cli/3MF/NC1/stock-state/travelers under one `ContentHash.Of` fold); forward constraint: durable receipts decode 2701-2710, never 25xx |
| :34 `[REUSE_WIRE]` | BLOCKED | KEEP BLOCKED (python:data-gated); correct the `:38` phantom → the SetExpr/ElementSet substrate realized as `Query/federation` fences; re-anchor V1 |
| :41 `[SUBSTRAIT_FEDERATION_SEAM]` | BLOCKED | KEEP BLOCKED; correct the `:45` phantom → `Query/federation` lowers Substrait onto SetExpr (V1, real per probe); re-anchor V1 |
| :55 `[PERSISTENCE_BIM_SYNC_CRDT]` | COMPLETE | LEGIT KEEP CLOSED (`commits#CRDT_ALGEBRA` real); align BIM:94 `CommitGraph.MergeBase` spelling → `commits#MergeBase` |
| :56 `[DURABILITY_RECOVERY_OBSERVATORY]` | COMPLETE | LEGIT KEEP CLOSED; the V10 RPO repairs + V12 AsOfKey verify strengthen the "re-establishes CONTENT IDENTITY" claim into an assertable proof |
| :57 `[TRANSACTION_CONCURRENCY_CONTROL]` | COMPLETE (STALE) | RE-POINT off deleted `Query/transaction.md` → `Element/graph#STORE_RAIL` (fold); prepared-tx RETIRE |
| :58 `[ENVELOPE_ENCRYPTION_KMS]` | COMPLETE (STALE) | RE-POINT off deleted `Store/encryption.md` → `Element/identity` (KMS custody, post-V5a) + `Store/blobstore#BLOB_GC` (object-SSE) |
| :59 `[DATA_QUALITY_INTEGRITY_FRAMEWORK]` | COMPLETE (STALE) | RE-POINT off deleted `Store/quality.md` → `Query/columnar` + `Store/provisioning` verification surfaces |
| :60 `[BULK_ETL_INTERCHANGE_PIPELINE]` | COMPLETE (STALE) | RE-POINT off deleted `Query/pipeline.md` → `Query/columnar` (bulk ingest/egress over ArrowChunk) |
| :61 `[CDC_STREAMING_EGRESS]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/egress.md` → `Version/egress.md` (V3 NEW owner; re-owned + re-realized leg 4) |

### TASKLOG (19 — 1 open + 18 closed)

| Card | Status | Disposition |
|---|---|---|
| :20 `[ARTIFACT_CONTENT_KEY_FEDERATION]` | BLOCKED | KEEP BLOCKED (python:artifacts-gated); correct the `:24` phantom → `Version/provenance#ATTESTED_LEDGER` signed-artifact binding (ARCH:51) + the V1 `Query/federation` `SourceKind.SignedArtifact` source row |
| :34 `[PERSISTENCE_BIM_ARTIFACT_INDEX]` | COMPLETE | LEGIT KEEP CLOSED (`cache#ARTIFACT_BLOB_INDEX` real) |
| :35 `[RECOVERY_PAGE_AUTHOR]` | COMPLETE | LEGIT KEEP CLOSED (recovery real; V10 RPO repairs land with the improve) |
| :36 `[RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT]` | COMPLETE | LEGIT KEEP CLOSED (RecoveryFault 829x real) |
| :37 `[TRANSACTION_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Query/transaction.md`/`.cs` → the fold into `graph#STORE_RAIL` (TxnScope/IsolationPolicy/2PC through `GraphStoreOp`); prepared-tx RETIRE |
| :38 `[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]` | COMPLETE | LEGIT KEEP CLOSED (`ledger#MERGE_LAW ConflictResult` real `:225`) |
| :39 `[ENCRYPTION_PAGE_AUTHOR]` | COMPLETE | LEGIT KEEP CLOSED (records the settled fold — KMS→`identity`, SSE→`blobstore#BLOB_GC`) |
| :40 `[SQLCIPHER_RESEARCH_PROMOTE]` | DROPPED | LEGIT DROPPED-hold (embedded floor rejects the cipher bundle) |
| :41 `[QUALITY_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Store/quality.md`/`.cs` → `Query/columnar` + `Store/provisioning` verification surfaces |
| :42 `[PIPELINE_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Query/pipeline.md`/`.cs` → `Query/columnar` |
| :43 `[EGRESS_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/egress.md`/`.cs` → `Version/egress.md` (V3; the COMPLETE was FALSE — owner deleted — flip to the V3 re-authoring pointer, re-realized leg 4) |
| :44 `[SCHEMADDL_SQL_COLLAPSE]` | COMPLETE (STALE + self-contradicting) | RE-POINT off `SchemaDdl.Sql` (absent, contradicts :45) → the V6 identity migration owner; extension DDL is `ServerExtension.CreateSql`; NO `SchemaDdl.Sql` owner |
| :45 `[STORE_SERVER_SPLIT]` | COMPLETE | LEGIT KEEP CLOSED (the truth-source contradicting :44; provisioning + tenancy-into-identity) |
| :46 `[ANNOTATION_RELOCATE_TO_BIM]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/annotation.md` + phantom `Query/federation#ENTITY_GRAPH` join → `Version/ledger#CHANGEFEED` (BIM:101); join on real GlobalId |
| :47 `[SCHEDULE_RELOCATE_TO_BIM]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/schedule.md` → `Ingest/schedule.md` (V11 NEW; MPXJ codec + durable rows); 4D/CPM to Rasm.Bim/schedule (BIM:102) |
| :48 `[ICECHUNK_ASOF_CONTENT_KEY]` | COMPLETE (STALE + phantom) | RE-POINT off deleted `Version/snapshots#SNAPSHOT_PROTOCOL AsOfKey` → `Version/timetravel` (`AsOfKey`=`Checkpoint.Hash`, V12; re-realized leg 2) |
| :49 `[KMS_PACKAGE_ADMISSION]` | COMPLETE | LEGIT KEEP CLOSED (KMS trio pinned) |
| :50 `[ARROW_FLIGHT_PACKAGE_ADMISSION]` | COMPLETE | LEGIT KEEP CLOSED; correct the ripple ("rides BULK_ETL_INTERCHANGE_PIPELINE", deleted) → rides V3 `Version/egress` + V1 `Query/federation` |
| :51 `[KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION]` | COMPLETE | LEGIT KEEP CLOSED; the Kafka/CloudEvents ride the V3 sink rows |

Totals: 30 · 5 open (3 BLOCKED + 2 QUEUED, corrected + held) · 25 closed (10 LEGIT KEEP + 1 LEGIT DROP + 14 stale re-pointed: IDEAS:57-61, TASKLOG:37,41,42,43,44,46,47,48).

---

## [L]-[LEG_PARTITION] — proven acyclic on the frozen-contract graph

Within a leg the listed order IS dependency order. The four on-disk intra-package inversions resolve per V5 (frozen vocabulary contracts consumed across legs); the DECISION freezes `StorageTier`/`ObjectStore.Head`/`ServerExtension`/`TimeCut` shapes at decision time, so an earlier leg consumes the frozen CONTRACT, never the later leg's rebuilt page.

1. **SPINE + PERIMETER** — the `[V4]` band registry COMPLETE (all 19 own + new-owner + re-banded decades in leg 1, so a duplicate integer is unrepresentable from leg 1; each union re-derives in its owning leg, the `CypherFault` rename executing in leg 3); `Element/` (authority extraction V5a, EF commit + migration owner V6, codec consumer-contracts V9 + kernel-hash re-anchor + HashPolicy V10); the FULL roster reconciliation + catalog re-anchor/dedup/stub motion (V7, ROSTER — gates every later import); the own-ledger seam corrections (V12 half); the csproj:10 AppHost-reference removal. Pages: graph, codec, identity, authority.
2. **VERSION ENGINE** — ledger/commits/timetravel/merge/provenance/retention/recovery V10 repairs; the `AsOfKey` arm; the `CRDT_OP_SET` parity pin (commits) + **§B version-engine hasher re-anchor (extended from leg-1's codec-only enumeration)**; H1 CommitFault MINT; the V8 Type-correlation path + named migration case; the V9 Scrub/Bisect incremental re-shape (interim documented). Pages: ledger, commits, timetravel, merge, provenance, retention, recovery.
3. **QUERY + INGEST** — retrieval extraction + RetrievalFault (V5b, before the federation owner that lowers onto it); columnar trust gates + posture + DuckDB substrait extension row; the V1 federation owner; cache/cypher (CypherFault rename)/topology (Lca gate) cold passes; Ingest growth (tabular Sep + schedule V11). Pages: lane, retrieval, topology, columnar, cypher, cache, federation, tabular, schedule.
4. **STORE + COORDINATION + EGRESS** — blobstore V10 repairs; provisioning loose-code typing (ServerFault re-band + 7701/7702); `Store/coordination.md` (V2, before the egress pump); the egress owner (V3) over the leg-2 changefeed + the leg-4 outbox spine. Pages: blobstore, provisioning, coordination, egress.

**Acyclicity proof.** Zero upward strata edges (Persistence up-only on `Rasm` + `Rasm.Element`; csproj:10 AppHost reversal DELETES — [A.1]; the `MembershipView.Serving` AppHost:80 consumer [G5] is an AppHost→Persistence READ, no down edge). Frozen-vocab inversions: `graph(1)←timetravel(2)` TimeCut, `retention(2)←blobstore(4)` StorageTier, `recovery(2)←blobstore(4)` ObjectStore.Head, `cypher(3)←provisioning(4)` ServerExtension — each an earlier-leg page reading the DECISION-frozen shape (a change reopens the consuming leg as hard residual). `egress(4)→coordination(4)` OUTBOX_CURSOR — intra-leg, forward-only (coordination never reads the pump), coordination lands first. `federation(3)→lane(3)`+`→columnar(3)`+`→retrieval(3)` — within-leg, ordered (retrieval/columnar before federation). Acyclic. ∎

Campaign acceptance (post-leg-4 composed dry-runs): (a) version-engine — commit → three-way merge across a simulated Type re-key reading as RENAME → AS-OF scrub incremental-shaped → attested verify → retention sweep → verified restore whose terminal proof is `reconstructed OfGraph == checkpoint AsOfKey` (`RecoveryFault.VerifyFailed` unreachable) beside a real RPO lag; (b) coordination — a fenced Budget debit whose stale-token replay is REJECTED (`LeaseFenced`), a step-state CAS, an `InFlight` read serving CrashResume, an outbox drain to a CloudEvents sink dedup'd by `id`=`ContentKey`; (c) federation — a Substrait plan lowered to `SetExpr` executing on the columnar lane; (d) perimeter audit — every fault code → exactly one registry row, every digest mint → `ContentHash.Of`/seam `ContentAddress`, every catalog + card anchor → disk, `dotnet restore` clean.

---

## [M]-[SELF_REPORT] + open items

- **Page rows**: 24 (Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3). Lowering: 6 `new` (authority, egress, retrieval, federation, schedule, coordination) · 2 `rebuild` (identity, lane — the split-remainders, [G3]) · 16 `improve` · 2 absorb pairs · 0 deletePages.
- **Verdicts**: 13 (V1-V13). **Evidence**: 14 (E1-E14) + 11 beyond-register (H1-H3, 7701/7702, GCS/Minio checksum, ByDomainId prose, Lca gate, tabular Wire, columnar spellings, flowtide:151, csproj reversal). **Cards**: 30 (5 open corrected-held, 14 stale re-pointed, 11 legit KEEP/DROP). **Packages**: 5 charter/redundancy prunes + ScyllaDB scale-out prune + 5 scale-out KEEP-as-axis-rows (per-package ceiling proofs [G2]) + streaming sink rows + federation trio + EF commit + MPXJ + Ara3D BUMP + DuckDB substrait extension ADD + Grpc.Tools DROP.
- **Bands**: 19 unique own + pinned-mirror foreign; Egress 827x; Commit 826x MINT; duplicate-fails-at-type-init.
- **AppHost PORT contracts homed**: 4/4 on `Store/coordination#CoordinationOp` + `#OUTBOX_SPINE` (APPHOST:76 BudgetDebit · :77 StepStateCas+InFlight · :78 OutboxCursor · :79 LeaseAcquire/Renew/Release+MembershipUpsert) + the :80 MembershipView.Serving consumer [G5].
- **Merge re-verification**: acyclic (§L) · disposition-complete (V/E/card/package/[03]-delta all disposed) · band-unique (19 own, no duplicate) · PORT-complete (4/4 + :80 consumer).

OPEN ITEMS (verification obligations carried into the build legs, none blocking the DECISION):
1. `CompleteMultipartUploadRequest.ChecksumXXHASH128` — confirm via `assay api` (AWSSDK.S3 restored assembly) before the blobstore leg closes; the two enum members `XXHASH128`/`FULL_OBJECT` already verified.
2. DuckDB `substrait` community-extension — verify a build exists for the pinned `DuckDB.NET.Data.Full` 1.5.x runtime; fail-closed (`FederationFault.UnsupportedRelation`) on an unsupported relation.
3. Ara3D DEBUG-IL re-verify at the feed-confirmed 1.6.1 — re-decompile for `AssemblyConfiguration("Debug")`; retire the DEBUG-IL caveat if gone, escalate to in-corpus absorption if it persists.
4. The frame-to-kernel re-home (ClockPolicy/CorrelationId/TenantContext/Principal) is a contingent KERNEL/APPHOST counterpart obligation ([A.1]/[G6]) — injection is the interim/primary Persistence resolution; the re-home lands (if ever) in the AppHost/kernel campaigns, never a Persistence edit.
5. BIM:81 GDAL/OGR GeoParquet exact anchor not resolved in the Bim governance grep — homed at `Query/columnar` per three drafts as a sibling counterpart obligation (Bim campaign owns the interior).
