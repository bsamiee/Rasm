# [DRAFT] Rasm.Persistence structural blueprint — lens: FEDERATION-EGRESS-FIRST

Complete page-set/band-registry/seam-ledger/roster/leg blueprint, worked BACKWARD from the four cross-runtime wire seams to the Query/Version owners that carry them. Every V1-V13 verdict, E1-E14 evidence row, `[03]` capability delta, and TASKLOG/IDEAS card is disposed; every ruled default is honored unless disk proof overturns it (only V1's wire form is overturned, in the owner's favor, per the probe). The federation probe verdict FEASIBLE_WITH_GAPS is disposed: REINTRODUCE, corrected wire form. All four AppHost PORT contracts are homed on Persistence-owned op-union cases.

Final shape: **24 pages** (Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3), zero one-file folders; **6 new** (authority, retrieval, federation, egress, coordination, schedule); **18 improve**; **0 rebuild**; **0 delete** (the ~12 dead pages were already deleted in the element rebuild — cards re-point, no `deletePages` row is owed this campaign).

---

## [§0] THE WIRE-SEAM BACKBONE — the lens walked backward

The lens fixes four cross-runtime seams as ground truth and derives every owner from them. Each seam is content-addressed through the ONE kernel `ContentHash.Of` seed-zero entry, so a Persistence-owned type never mints a second identity. Each walks to exactly one owner page and one op-union case.

### [S1] SUBSTRAIT PLAN INGRESS → `SetExpr` — owner `Query/federation.md` (NEW, V1)

The `python:data/tabular/query` producer emits a portable plan; the C# federated entity graph must execute it against the SoR. Walked backward:

1. **Wire form (probe-CORRECTED, ruled-default overturned in the owner's favor).** The plan arrives as **Substrait protobuf bytes** (primary) or canonical **protobuf-JSON** (bonus — `python substrait` libs emit both, giving the producer signature-lock latitude). Ingress is `Substrait.Protobuf.Plan.Parser.ParseFrom(wireBytes)` then `new SubstraitDeserializer().Deserialize(protoPlan)` → managed `FlowtideDotNet.Substrait.Plan` — ~2 lines, both **public and shipped in-assembly** (`probe-federation.md:22-36`, decompile-confirmed; falsifies `api-flowtide-substrait.md:151`'s "no public Deserialize"). The ruled default's hand-written `RelationVisitor` transcriber and its `Grpc.Tools` codegen admission are BOTH DROPPED: regenerating the substrait `.proto` mints a duplicate CLR `Plan` type `SubstraitDeserializer` rejects by identity; only `Google.Protobuf@3.35.1` (already admitted, shared-tier) is used, as the parse runtime.
2. **Lowering — one `LoweringTarget` discriminant `RelationVisitor` subclass** (~150-250 LOC; reference `SubstraitToDifferentialComputeVisitor` = 139 LOC / 9 relation kinds). Managed `Plan` → `SetExpr | columnar route` per the probe `[03]` table: `ReadRelation`+`Filter`(SetPredicate)→`Predicate`, `SetRelation`→`Union`/`Intersect`/`Difference` (exact 3-for-3), `VirtualTableReadRelation`→`Literal(Seq<NodeId>)`, bounded `IterationRelation`→`Closure(Seed,Depth)`, key-semijoin `Join`→`Intersect`; `Project`/`Aggregate`/`Sort`/`TopN`/`Fetch`/`Window`/general-`Join`→columnar/ADBC; `WriteRelation`→REJECT (read-only); `ExchangeRelation`→DROP (single-node).
3. **Pinned execution.** ONE `TimeCut` (default: the `StalenessWatermark.HeadSequence` head, `lane.md:40`). The `SetExpr` subset rides the store's existing GiST/GIN indexes (`SetResolve` in the lane algebra `lane.md:127-266`); the tabular subset rides the EXISTING in-process DuckDB engine (via the `substrait` community extension `from_substrait(blob)`, a `ColumnarExtension.Substrait` row on `columnar.md`, NOT a NuGet) plus the external ADBC warehouse's own engine. **No second query engine** — the owner is a router/lowerer over two standing lanes; the V1 retire criterion is NOT triggered.
4. **Receipt.** Returns the `ElementSet` receipt (`lane.md:129` currency) + the Arrow batch stream, recording the **(plan digest, cut, watermark) triple** where `plan digest = ContentHash.Of(wireBytes)` (the kernel entry over the received bytes, NEVER a local `XxHash128` call site) so a federated result is content-addressed and reproducible; replay lands as one `ArtifactKind` reuse row (`cache.md:20` growth law), never a second cache.
5. **`SourceKind` capability axis** (seed DATA): `Substrait-native` vs `SQL-only`. Committed tabular federation scopes to Substrait-capable sources (in-process DuckDB via the extension; Flight-SQL/DataFusion-class ADBC that advertise `AdbcStatement.SubstraitPlan`); SQL-only warehouses (the Beta BigQuery driver) stage as a growth row — the producer emits SQL text for that `SourceKind`, executed via `AdbcStatement.SqlQuery`. `SourceKind.SignedArtifact` (the `[ARTIFACT_CONTENT_KEY_FEDERATION]` card) is a source row whose content-key binding resolves through `Version/provenance` (ARCH:51).

Entry: `public static IO<FederatedResult> Execute(FederationPlan plan, TimeCut cut)`, `FederationPlan` discriminating on `SourceKind`; `FederationFault` band 8430.

### [S2] CONTENT-ADDRESSED CloudEvents EGRESS — owner `Version/egress.md` (NEW, V3)

CDC egress drains the ONE changefeed (`ledger#CHANGEFEED`) and the ONE `[V2]` outbox cursor; the parity floor RAISES to exactly-once EFFECT because at-least-once + replay without an idempotency contract is a duplicate generator. Walked backward from the envelope:

1. **One content-addressed envelope, never a minted UUID.** `EgressPump.Drain` folds the outbox cursor past `OpLogEntry` rows into `EgressSink` delivery rows under ONE CloudEvents envelope: CloudEvent **`id` = `OpLogEntry.ContentKey`** (`ledger.md:109`, the delta content key already exists), **`Sequence` extension** (String, `api-cloudevents.md:62`) = the outbox `long Sequence` (`ledger.md:108`/`SyncCursor.Sequence :114`), **`Partitioning.partitionkey` = `EntityKey`** (`ledger.md:108`) for ordered partitioned delivery.
2. **Per-sink dedup is a composition, not a promise** (seed DATA sink axis, categorical-best floor):
   - `Nats` — JetStream durable streams own the `DeliveryGuarantee.Settle` ceremony; the CloudEvent `id` maps to the `Nats-Msg-Id` header (`api-nats.md:44`) so the server-side dedup window collapses a redelivered op to a no-op, and the publish-ack (`api-nats.md:94`) is what advances the cursor.
   - `Webhook` (server-side via the admitted `pg_net` extension row) — states its own honesty: `net.http_post` returns only a request-id `bigint` and is fire-and-forget (`api-pg-net.md:35`), so the pump NEVER advances on post; it advances only after reconciling `net._http_response`/`net.request_status` to `SUCCESS` (`api-pg-net.md:50-53`), `PENDING` holds the cursor, `ERROR`/timeout routes the typed dead-letter; `net._http_response` is UNLOGGED, so a crash drops pending rows, the held cursor re-posts, and the receiver-side idempotency-key header (the same `id`) absorbs the duplicate for at-most-once effect.
   - wire-native gRPC/Arrow hop — READS the AppHost side: `HopPolicy` gains a per-hop delivery-honesty axis (AppHost `[V9]`, the demanding consumer of stated-honesty-per-sink), so the cursor-advance law composes the hop's DECLARED guarantee, never an assumed one.
   - `Kafka` (+SchemaRegistry serdes, `Chr.Avro`×3 the codec column), `RabbitMQ.Client`, `DotPulsar` — one delivery ROW each over the one pump fold; a broker row leaves only on the `[V3]` per-package redundancy proof.
3. Presence/awareness stays `ledger#PRESENCE` (the lossy `AwarenessLane` `DrainSurface`, `ledger.md:467`) — the egress owner drains durable ops, never the lossy lane.

Entry: `public static IO<EgressReceipt> Drain(SyncCursor cursor, EgressSink sink)`, `EgressSink` the delivery axis; `EgressFault` band 8420. Seam edge `Version/egress → Store/coordination [CURSOR]` (no reverse edge — coordination never reads the pump).

### [S3] AsOfKey ICECHUNK SEAM — owner `Version/timetravel.md` (IMPROVE, V12)

`ARCH:50` `Version/timetravel ← python:data/gridded/virtual [CONTENT_KEY]: icechunk as-of snapshot identity` targets a LIVE page but `AsOfKey` is a genuine phantom (zero hits; `TASKLOG:48` placed it on the deleted `Version/snapshots`). RAISED from a governance re-anchor to a load-bearing capability, ONE digest serving two consumers:

1. **`AsOfKey` = `Checkpoint.Hash`** (`timetravel.md:84`, `public readonly record struct Checkpoint(Hlc At, long Version, ContentAddress Address, UInt128 Hash, ...)` — the field exists). The checkpoint's content digest IS the cross-runtime as-of key: `python:data/gridded/virtual` reads the identical as-of snapshot identity over the ONE kernel `ContentHash.Of` seed-zero entry.
2. **Recovery content-identity oracle.** `RecoveryPoint.AsCut()` (`recovery.md:65`, real, returns `TimeCut`) resolves the restored coordinate to the checkpoint `AsOfKey`; `RecoveryFault.VerifyFailed(Route, Expected, Found)` (`recovery.md:87`, real) fires on the reconstructed-vs-checkpoint mismatch. This makes `recovery.md:3` "re-establishes CONTENT IDENTITY" an ASSERTABLE proof, not best-effort completion.
3. The incremental `OfGraph(prior, delta)` contract (`[V9]`) keeps the AS-OF reconstruction under it O(delta) across a million-event restore, never O(N·graph).

Entry addition: `AsOfKey` accessor on the checkpoint chain (`timetravel.md:142` `Anchor`); no fault band of its own (composes graph/codec digests through the seam).

### [S4] CrdtOpWire BIT-PARITY LAW — owner `Version/commits.md` (IMPROVE, V4-MINT + SEAM_AND_RAIL)

The `CrdtOpWire` `[MessagePack.Union]` `[Key]` sequence IS the wire schema, reproduced in Python/TS bit-identically; canonical bytes are seam-owned and the `[AMENDMENTS]` count-prefix fork is absorbed BY COMPOSITION. Three obligations the disk falsifies as unmet:

1. **MINT `CommitFault`/`CrdtWireFault : Rasm.Domain.Expected`** — `commits.md` is a THIRD un-banded bare-`Error.New` owner (`:358` 8261 decode-drift, `:411` 8263 parity-drift, `:400` 8264 owner-mints; NO `[Union]` fault owner on the page). V4's "Commit 826x register as-is" is WRONG — the band is MINTED, not registered (H1).
2. **Pin the `CRDT_OP_SET` parity fixture** — kernel `reconciliation.md:129` names `commits#CRDT_ALGEBRA` the producer of the `MvRegister`/`opMerge` op-set whose divergent-delivery folds converge byte-identically; the commits leg PINS it (turning the DESIGN-PIN REAL under the one kernel seed), never a fabricated byte set.
3. **Re-anchor the raw hasher mints** — `CrdtWire.ContentKey` (`commits.md:354`), `CommitNode` key (`:91`), `ParityVector` corpus (`:375-376,401`) currently raw `XxHash128.HashToUInt128`; each composes `ContentHash.Of` (value-identical, pure call-path collapse). `ContentParityCorpus.Seed=0L` folds into the mint or dies (dead const, `:381`).

Entry: `Crdt.Apply` over the `CrdtField` join-semilattice; `CommitFault` band 8260.

---

## [§1] FEDERATION HINGE DISPOSITION (V1) — probe FEASIBLE, REINTRODUCE

Probe verdict FEASIBLE_WITH_GAPS is disposed as **REINTRODUCE `Query/federation.md`** (a draft assuming FEASIBLE against an INFEASIBLE probe would lose at judge; the probe IS feasible, so REINTRODUCE stands and the retire alternative is closed). The ruled-default WIRE FORM is the ONE departure disk proves stronger (§0/S1): shipped-public `SubstraitDeserializer` collapses transcription to ~2 lines and forecloses `Grpc.Tools`. Two chain links close honestly at ingress: (a) WIRE FORM — protobuf bytes/JSON through the shipped `SubstraitDeserializer.Deserialize`, `SqlPlanBuilder.Sql`/`GetPlan` staying the self-hosted SQL front-end (never the cross-runtime wire); (b) PINNED EXECUTION — one `TimeCut`, the `(plan digest, cut, watermark)` receipt, replay as one `ArtifactKind` row. Roster: KEEP `FlowtideDotNet.Substrait`+ADBC(Apache/BigQuery)+`Apache.Arrow.Flight` (ride V1); ADD the DuckDB `substrait` community-extension ROW; DROP the `Grpc.Tools` codegen assumption. The three BLOCKED federation cards re-anchor to the owner with honest status (SetExpr subset zero-gap fences; tabular subset scoped to Substrait-capable sources; BLOCKED on the `python:data` producer).

---

## [§2] THE FOUR AppHost PORT CONTRACTS — homed on `Store/coordination.md` (NEW, V2)

Each `Rasm.AppHost/ARCHITECTURE.md:76-79` PORT row walks to one Persistence-owned `CoordinationOp` case; the op-union, `FencingToken`, membership rows, and receipts are Persistence-owned types AppHost's PORT adapters DECODE — no AppHost interface or type crosses down. The fencing is RAISED from lease-ISSUING to token-VALIDATING (Kleppmann): a fence a resource never checks is decorative.

| AppHost PORT row | Persistence owner + op-union case | Contract |
|---|---|---|
| `APPHOST:76` `Agent/capability ⇄` fenced per-tenant Budget debit `(ONE_FENCED_LEASE_STORE)` | `Store/coordination#CoordinationOp.BudgetDebit` | Fenced predicated compare-and-decrement, PER-UNIT VECTOR: the Budget row carries N unit balances keyed by the `CostUnit` string key (`capability.md:54-62`, `CostVector(HashMap<CostUnit,long>)`); ONE `UPDATE ... WHERE token >= held AND balance_i >= debit_i` per requested unit atomically. A scalar debit is falsified by the multi-unit consumer; a stale token is a typed `LeaseFenced`, never a lost-update or double-spend across handoff. |
| `APPHOST:77` `Runtime/orchestration ⇄` workflow step-state CAS `(ONE_FENCED_LEASE_STORE)` | `CoordinationOp.StepStateCas` (write) + `CoordinationOp.InFlight` (READ) + `CoordinationOp.ExpiredScan` (READ) | Fenced CAS on the step-state row under the TenantId-RLS predicate (`orchestration.md:257-271` `StepStateSeam.Commit`); the READ cases serve `CrashResume`/orphan-reclaim (`orchestration.md:271,282,284` `StepStateSeam.InFlight`) — a write-only union strands the crash-resume flagship and forces the AppHost table scan the PORT law forbids. |
| `APPHOST:78` `Wire/outbox ⇄` transactional outbox same-tx `(ONE_OUTBOX_EGRESS_SPINE)` | `Store/coordination#OUTBOX_SPINE` (names the Marten stream) | The Marten event stream IS the outbox (events commit in the same `IDocumentSession` as state — the same-tx guarantee already holds); the owner NAMES it, mints the durable drain cursor (`SyncCursor`, `ledger.md:114`) + at-least-once advance law; `[V3]`'s `EgressPump` consumes it. No second event store. |
| `APPHOST:79` `Wire/Coordination.cs ⇄` CAS + fenced-lease + membership `(ONE_FENCED_LEASE_STORE)` | `CoordinationOp.LeaseAcquire` / `LeaseRenew` / `MembershipUpsert` | Monotonic `FencingToken` over PG row-CAS + `pg_advisory_xact_lock`; membership rows with lease-expiry semantics; `LISTEN`/`NOTIFY` wake. `MembershipView.Serving` roster (`AppHost:80`, in-process consumer) reads these rows. |

Entry: `public static IO<CoordinationReceipt> Run(CoordinationOp op)`; `CoordinationFault` band 8410 (`LeaseFenced`, `CasConflict`, `BudgetExhausted`, `LeaseExpired`, `MembershipStale`). Composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql advisory locks + LISTEN/NOTIFY — never a distributed-lock sidecar. `Store/` grows to a real 3-page axis.

---

## [§3] PAGE-SET TABLE (24 rows)

Columns: **path** | **semantic action → engine lowering** (`kind` new/rebuild/improve; `absorb {into,from}`; `deletePages` none owed — pre-deleted) | **owner charter · folder home · per-page skeleton · V4 band** | **entry signature** (op-union, or NONE) | **in/out seams** (both directions, one anchor per seam) | **leg**.

### Element/ (4 — +authority)

| Path | Action → lowering | Charter · skeleton · band | Entry | Seams (in ← / out →) | Leg |
|---|---|---|---|---|---|
| `Element/graph.md` | KEEP → `improve` | The store rail + one materializer (`GraphDelta.ReplayOnto`), co-txn identity commit, generated-total `GraphStoreOp.Switch`, one-boundary `Lift`. **HOSTS the `[FAULT_TABLES]` band-registry owner block** (§4). Band `Graph 8300`. | `GraphStore.Run(GraphStoreOp) → GraphReceipt` (`[Union]` op) | in ← `Rasm.Element` SoR (ARCH:41); ← `Rasm` kernel content-key (ARCH:42); ← `AppHost/Runtime` ProjectionContext ingredients (ARCH:54, injected shape); ← `timetravel#TimeCut` (frozen vocab, V5d); out → graph is a root | 1 |
| `Element/codec.md` | KEEP → `improve` | Seam-composed `ContentAddress`/`OfGraph`, 8-rank `RejectTier`, single-pass atomic seal, CBOR trust boundary, FastCDC chunker. `HashPolicy` shrinks to `{Identity,Content}` + header forward-compat law (V10); the `:175` `ByDomainId` prose dies; raw mints (`SnapshotHeader.Seal`, `ContentChunker`) re-anchor onto `ContentHash.Of`. Band `Codec 8310-8330` (multi-decade stride). | `SnapshotCodec` `[SmartEnum]` axis + `Seal`/`Verify`/`Chunk` ops (composing `ContentAddress`) | in ← `Rasm` kernel `ContentHash.Of` (ARCH:42); out → `typescript:wire` CBOR bytes (ARCH:43); ← `Compute/CONTENT_CHUNKER` (COMPUTE:111 corrected) | 1 |
| `Element/identity.md` | SPLIT → `improve` + `absorb {into: authority, from: identity}` | Relational tier + `IdentityPolicy` key axis + KMS custody (signing AND envelope, one `KmsProvider` axis) + `SchemaVerdict`. **Hosts the V6 migration owner**: `UseValueObjectValueConverter()` + `UseSnakeCaseNamingConvention()` registered, hand `NodeId` converter + ~13 `HasColumnName` die (LanguageExt converters stay); `element_identity`/`node_cell` DDL authored, `Microsoft.EntityFrameworkCore.Design` earns admission; extension DDL rides `ServerExtension.CreateSql` through this one migration owner. Tenant-RLS + KMS keyrings stay here. Band `Identity 8340`. | `IdentityStore.Stamp` + `SchemaGate.Verify` + `IdentityPolicy`/`KmsProvider` axes (no single op-union) | in ← `AppHost/Runtime` `[PORT]` ObjectAcl store + TenantId RLS + KMS-unwrap (ARCH:53, splits — KMS/RLS stay); out → the V6 DDL | 1 |
| `Element/authority.md` | NEW → `kind: new`, `absorb {into: authority, from: identity}` (the `:287-341,465-484` authz set-algebra) | The object-ACL authz vocabulary: `Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor` — pure deny-over-allow set-algebra, zero `KmsProvider`. Consumed by `commits#Movable` ACL gate. NO fault band (access is a value verdict, not a rail fault; sub-bands under Identity 8340 only if a rail is later added). | `Authority.Admit(GrantSet, AclScope) → Effective` (set-algebra fold) — **NONE (vocabulary+ops)** | in ← `identity` (extraction source); out → `commits#Movable` (ACL gate, intra-package); the own `ARCH:53` PORT `ObjectAcl` half re-points here | 1 |

### Version/ (8 — +egress)

| Path | Action → lowering | Charter · skeleton · band | Entry | Seams (in ← / out →) | Leg |
|---|---|---|---|---|---|
| `Version/ledger.md` | KEEP → `improve` | One-materializer changefeed projection off Marten `SubscriptionBase`; `(Hlc, OriginStoreId)` LWW; exact-conservation `Apply`. V10: batch `ProcessEventsAsync` into ONE `IO` fold (`:119`); `SyncOpKind.Truncate`+`WholeRelation` wire-or-die (`:31,34`); `OpLogEntry.Codec` collapses to a `Family`-derived accessor (`:149-153`). Band `Sync 8250` (correctly typed). | `OpLog.Project` + `SyncOpKind` `[Union]` dispatch + `ChangefeedSubscription` | in ← `python:runtime/transport` CRDT delta (ARCH:48); ⇄ `AppHost/Runtime` HLC+TraceSlot (ARCH:49); ← **`AppHost/Wire/companion` PRESENCE PeerRoster** (NEW row, ledger.md:467); ← `Compute` Assessment blobs (ARCH:52); ← **`Bim` Speckle Base import** (BIM:104 corrected); out → `typescript:state` wire (ARCH:46) | 2 |
| `Version/commits.md` | KEEP → `improve` | Content-addressed commit-DAG, six-type CRDT algebra, `CrdtOpWire` `[Key]` flagship (§0/S4). **MINT `CommitFault`** (H1: 8261/8263/8264). Near-linear `MergeBase` (one reverse-reachability generation-mark pass + dominance filter, killing `:108-113` O(V²)); `VectorOrder`/`Order` wire-or-die; `ContentParityCorpus.Seed` folds into the mint; PIN the `CRDT_OP_SET` fixture; re-anchor raw mints onto `ContentHash.Of`. Band `Commit 8260` (MINTED, not register-as-is). | `Crdt.Apply` (`CrdtField` dispatch) + `MergeBase` + `CommitNode` | in ← `Rasm.Element` graph delta; out → `typescript:wire` `CrdtOpWire` msgpack union (ARCH:44); ⇄ `python:runtime/transport` None-companion parity corpus (ARCH:45); ← `Bim` `BimCommit`/`MergeBase` (BIM:92,94 — spelling aligned to `commits#MergeBase`) | 2 |
| `Version/timetravel.md` | KEEP → `improve` | ONE materializer AS-OF reconstruct/diff/blame/scrub/bisect/branch, `Snapshot<T>(Inline)` checkpoints. **Mint the `AsOfKey`=`Checkpoint.Hash` arm** (§0/S3); re-shape `Scrub`(`:236`)/`Bisect`(`:258-259`) to the incremental `OfGraph(prior,delta)` composition (interim full-recompute documented); edge keys use seam `ContentAddress.Of` not raw `XxHash128` (H3, `:203-204`); `AggregateStreamToLastKnownAsync` composes-or-leaves the package list. No own band. | `TimeLog.Reconstruct/Diff/Scrub/Bisect` + `Checkpoint.Anchor` | in ← `python:data/gridded/virtual` icechunk `AsOfKey` (ARCH:50, re-anchored onto this LIVE page); ← `graph#TimeCut` consumer (frozen vocab, V5d); out → `recovery` (`AsCut`→`AsOfKey`) | 2 |
| `Version/merge.md` | KEEP → `improve` | Re-ingest `Reconcile` before diff, two-axis three-way merge, `MergeConflict` derived-logic collapse, `always-succeeds-with-annotations`. **V8b: second correlation-key row for `ObjectKind.Type`** (classification-independent Type natural key mirroring `ExternalKey`, so a re-keyed Type diffs as RENAME not delete-all+insert-all); thread `ElementJson.Options` through the `JsonPatchDocument` insert arm (`:390`); `MemberPath` NodeId typing VERIFIED clean (`api-generator-equals:52-53`). `GraphNode.GeometryHash` is the ARCH:55 consumer. No own band (`MergeConflict` is a conflict class). | `StructuralMerge.Merge` + `Reconcile` | in ← **`Rasm/Spatial/reconciliation` adjacency `GeometryHash`** (ARCH:55 RE-TARGETED here from topology; digest-semantics resolved §5); out → `typescript:wire` RFC 6902 patch (ARCH:47) | 2 |
| `Version/provenance.md` | KEEP → `improve` | Exact W3C-PROV-O typing, independent-`digestOf` attested verify (`Unauthored` reachable), Merkle transparency log. V10: `ProvJson` reads `AgentClass` off the `Principal` not unconditional `Person` (`:257`); the mutation-boundary annotation lands; `AgentKey` composes `ContentHash.Of`. `ProvRelation` `[Union]` axis. No own band (verify emits a verdict). | `ProvNode.Of` + `AttestedLedger.Append/Verify` | in ← `python:artifacts/provenance` signed-artifact content-key (ARCH:51) — the `[ARTIFACT_CONTENT_KEY_FEDERATION]` binding home; out → the federation `SourceKind.SignedArtifact` resolution | 2 |
| `Version/retention.md` | KEEP → `improve` | Six-row `RetentionClass` axis, fail-closed `RetentionCeiling`, full-history reachability `Mark` over EVERY `TimeCut`, ONE receipted executor. V10: `StorageLane.Durable` consumes-or-dies (`:36`). Band `Retention 8280` (correctly typed). | `RetentionSweep.Run` + `Mark` over `RetentionClass` `[SmartEnum]` | in ← `blobstore#StorageTier` (frozen vocab, V5c); ← `timetravel#TimeCut` (frozen vocab); ← `Microsoft.Extensions.Compliance.Redaction DataClassification` taxonomy (shared-tier, NOT an AppHost downward ref); ← `Compute` Assessment blobs (ARCH:52) | 2 |
| `Version/recovery.md` | KEEP → `improve` | Real `(Timeline, Lsn)` `RecoveryPoint` via Npgsql `IdentifySystem`, six-step ranked restore, `ReAttest` content-identity commit-point. **V10: real RPO lag** (freshest-replicated-blob age vs newest-event instant, killing `Duration.FromMinutes(absent.Count)` `:180`); WAL-throughput lifts to a policy row (`:171`). `AsCut()`→`AsOfKey` verify (§0/S3). Band `Recovery 8290`. | `RecoveryRoutes.Run` + `PointInTimeRestore` over `RecoveryRoute` `[SmartEnum]` | in ← `blobstore#ObjectStore.Head` (frozen vocab, V5c, `:178`); ← `AppHost/Runtime` RPO/RTO objectives (ARCH:64, injected shape); ← `timetravel#AsOfKey` | 2 |
| `Version/egress.md` | NEW → `kind: new` | §0/S2: ONE `EgressPump` fold draining the outbox cursor into `EgressSink` delivery rows under ONE CloudEvents envelope, exactly-once-EFFECT (`id`=`ContentKey`, `Sequence`=cursor, `partitionkey`=`EntityKey`), per-sink dedup honesty, typed dead-letter, replay. Sinks: Webhook(pg_net)/Nats/wire-native + Kafka/RabbitMQ/DotPulsar rows. Band `Egress 8420`. Ruled site `Version/` (beside the changefeed; a `Sync/` revival needs a second sync sibling — none exists). | `EgressPump.Drain(SyncCursor, EgressSink) → EgressReceipt` (`EgressSink` axis) | in ← `ledger#CHANGEFEED` op-log; in ← `Store/coordination#OUTBOX_SPINE` cursor `[CURSOR]`; ← `AppHost` `HopPolicy` delivery-honesty (wire-native row); out → the sink wire (NATS/webhook/broker) | 4 |

### Query/ (7 — +retrieval, +federation)

| Path | Action → lowering | Charter · skeleton · band | Entry | Seams (in ← / out →) | Leg |
|---|---|---|---|---|---|
| `Query/lane.md` | SPLIT → `improve` + `absorb {into: retrieval, from: lane}` (`FUSION_AND_CACHE`+`VECTOR_CODEBOOK`, `:267-520`) | Drops to READ_ROUTING + `ELEMENT_SET_ALGEBRA`: `ReadRouter`, `StalenessWatermark` sequence-gap honesty, `ElementSet`/`SetExpr` selection algebra + length-framed `Canonical` preimage (the `elementset` parity vector). The bare `Error.New(8360-8363)` LEAVES with the codebook. `store.WaitForNonStaleProjectionDataAsync` may route through `IMartenDatabase` directly (seam-honesty). No own band after extraction. | `ReadRouter.Route(ReadDemand, QueryShape)` + `ElementSetAlgebra.Evaluate(SetExpr)` (`SetExpr` `[Union]`) | in ← `Rasm.Element` graph; ⇄ `python:data/tabular/query` ElementSet currency (ARCH:57, Substrait half moves to federation); out → `federation` (SetExpr lowering target) | 3 |
| `Query/retrieval.md` | NEW → `kind: new`, `absorb {into: retrieval, from: lane}` | The coupled ANN owner: `FusionRank.Fuse` + `VectorCodebook.Train`/`AdcScan` (PQ/ADC hot-set lane), the `HybridCache` read-through, the min-priority-queue max-heap. **MINT `RetrievalFault`** replacing lane's bare `Error.New` (4 cases matching 8360-8363). **V6: the pgvector `<->`/`<=>` LINQ server-side ANN row** lands here beside the in-process PQ/ADC row (residency is row data on the one retrieval axis). Band `Retrieval 8400`. | `Retrieval.Fuse` + `VectorCodebook.Train`/`AdcScan` | in ← **`Rasm.Compute/Model/embedding` VECTOR_CODEBOOK** (NEW own row; COMPUTE:99, targets retrieval post-split); ← `pgvector` column map; out → `cache` fusion result | 3 |
| `Query/topology.md` | KEEP → `improve` | QuikGraph view, frozen incidence, traversal/path/components/topo-sort. **NEW in-fence fix: `Lca` pre-gates `IsDirectedAcyclicGraph` + rails `TopologyFault.Cyclic`** (`:257-264`, symmetric with `Order`; `api-quikgraph.md:26` flags `OfflineLeastCommonAncestor` unsound over cyclic input, and topology models containment cycles as real). `TypedEdge.IsContainment`/`Kind` consume-or-die. Band `Topology 8370` (keeps 837x). | `TopologyView.Of` + `TopologyQuery` `[Union]` dispatch | in ← `Rasm.Bim` (soft anchors, resolve to cache); out → the snapshot `OfGraph` memo (Element seam, stays). **ARCH:55 reconciliation NO LONGER targets topology** (re-pointed to merge) | 3 |
| `Query/columnar.md` | KEEP → `improve` | DuckDB INSTALL/LOAD lane, `BimOpenSchemaProjection : FlatTableProjection` (Persistence-owned generic branch — DECISION rules this the law, aligns ARCH:56), ParquetSharp. **V10: typed `Identifier`/`StorePath`/`SecretName` trust gate** on every raw-interpolated site (`Mount :255`, `Secret :271`, `Egress {projection}/{destination}/{stamp} :356-360`, `Generation {root} :390`); the `{projection}` becomes a composed typed projection; four `"80%"`/`"90%"` posture literals collapse to one constant (`:80-83`). **ADD `ColumnarExtension.Substrait` row** (the DuckDB `substrait` community extension `from_substrait(blob)`, the V1 tabular-lane close). Phantom spellings closed: `ExecuteQueryAsync`→`ExecuteQuery`, `BimData.WriteDuckDB` receiver. Band `Columnar 8350-8356` (keeps whole decade). | `Columnar.Run(ColumnarQuery)` over `ColumnarProfile`/`ColumnarExtension` axes | in ← `Rasm.Bim/Model` BimOpenSchema (ARCH:56, ruled Persistence-generic); ⇄ `python:data/tabular` Arrow over ADBC (ARCH:62); ← columnar/provisioning verification (quality re-anchor); out → federation tabular lane | 3 |
| `Query/cypher.md` | KEEP → `improve` (rename) | Injection-safe server-side `format('%L')` composition, three non-conflated result spaces, `pgr_TSP` 4.0. **RENAME `GraphFault`→`CypherFault`** (V4, resolves the simple-name ×2 with `graph#GraphFault`). `AgtypePath.Weight` populate-or-flatten (`:198`, dead 0.0). Band `Cypher 8360` (keeps 836x, renamed). | `Cypher.Run(GraphQuery)` (`GraphQuery` `[Union]`) | in ← `provisioning#ServerExtension.CreateSql` on the identity-migration rail (frozen vocab, V5c, `:3`); out → `ElementSet`/`H3Cell`/`Severed` result spaces | 3 |
| `Query/cache.md` | KEEP → `improve` | `ArtifactIndexRow` blob index + `ModelResultIndex` recency horizon + `BenchmarkRow` claim gate; the `cache.md:20` one-row `ArtifactKind` growth law (load-bearing for Compute assessment blobs, Fabrication durable rows, federation replay). Sync-over-async `IBufferDistributedCache` boundary (accepted shape, batching note only). No own band (composes). | `Cache.Lookup`/`Register` over `ArtifactKind` axis | in ← `Rasm.Compute` INDEX (ARCH:58); ⇄ **`AppHost/Runtime` CACHE_PORT L2 partition** (NEW own row, cache.md:191-232, `APPHOST:69`); out → `IBufferDistributedCache` (Redis-swap seam survives) | 3 |
| `Query/federation.md` | NEW → `kind: new` | §0/S1 + §1: Substrait plan decode (shipped-public `SubstraitDeserializer`) lowering onto `SetExpr` + the columnar/ADBC lane over ONE `LoweringTarget` discriminant fold; `SourceKind` seed-DATA axis (durable-store/signed-artifact/ADBC-warehouse; Substrait-native vs SQL-only capability); ONE `TimeCut` pinned; `(plan digest=ContentHash.Of(bytes), cut, watermark)` receipt; replay as one `ArtifactKind` row. NO second engine. Band `Federation 8430`. | `Federation.Execute(FederationPlan, TimeCut) → FederatedResult` (`FederationPlan` on `SourceKind`) | in ← `python:data/tabular/query` Substrait portable plan (ARCH:57 Substrait half, re-homed here); ← `lane#SetExpr` (lowering target); ← `columnar` (tabular execution); ← `provenance#SignedArtifact`; out → `ElementSet`+Arrow stream | 3 |

### Ingest/ (2 — +schedule)

| Path | Action → lowering | Charter · skeleton · band | Entry | Seams (in ← / out →) | Leg |
|---|---|---|---|---|---|
| `Ingest/tabular.md` | KEEP → `improve` | ONE rectangular-data owner: MiniExcel spreadsheet + the `Sep` delimited lane MADE EXPLICIT owner law (a fenced `Sep` surface or the delimited concern is genuinely MiniExcel-covered and the Sep row drops). **V10: `TabularWire.Wire` composes into `Policy().DynamicColumns` or dies** (`:272-273`, unwired CustomFormatter; STJ `Bind<T>` is the live path). `TabularSpec` one discriminant. Band `Tabular 8390` (RE-BANDED off 837x). | `TabularSource.Run(TabularSpec)` (`[Union]`) | in ← MiniExcel/Sep readers; out → `Rasm.Element` row-shape wire (ARCH:61); ← `Compute` Extract (COMPUTE:115 corrected → graph/Ingest) | 3 |
| `Ingest/schedule.md` | NEW → `kind: new` | V11 (consumer PROVEN — BIM:102 P6/MS-Project 4D counterpart + MPXJ.Net admitted): the `MPXJ.Net` schedule-file codec (.mpp/XER/PMXML → the record rail) + durable schedule rows; the Persistence half of the relocated Bim schedule domain (the app root owns the schedule→element map, per tabular law). Band `Schedule 8440`. | `ScheduleSource.Run(ScheduleSpec)` (`[Union]`) | in ← MPXJ readers; ← **`Bim` P6/MS-Project 4D** (BIM:102 corrected here); out → the record rail / durable schedule rows | 3 |

### Store/ (3 — +coordination)

| Path | Action → lowering | Charter · skeleton · band | Entry | Seams (in ← / out →) | Leg |
|---|---|---|---|---|---|
| `Store/blobstore.md` | KEEP → `improve` | Nine-delegate `ObjectLeg`, one `bracketIO` seal, write-once 412-noop, `RemoteStoreFault` `Lift`, write-blob-first + `PendingWrite` fence, WORM defense. **V10: `MultipartTransfer.Upload`+`BlobTransferReceipt` become THE composed receipt path or both die** (`:362,323`); `BlobResidence.Correlation` populates-or-dies (`:160-161`); **GCS/Minio `ObjectChecksum` rows read honest SDK-native CRC (Crc64/None), never decorative `XxHash128`** (`:239-240`; only S3 supplies `Integrity.Wire`). Verify `CompleteMultipartUploadRequest.ChecksumXXHASH128` via assay before leg close. Band `RemoteStore 5400`. | `BlobStore.Put/Get/Seal` over `ObjectStore` axis | in ← `Compute` authored GLB content-key (ARCH:59); ← `Bim/Exchange` IFC/BREP (ARCH:60); out → **`Compute GeometrySource` `Placement`** (NEW own row, C2, currently prose); ← `retention#GC` (frozen, V5c) | 4 |
| `Store/provisioning.md` | KEEP → `improve` | Six-command `NpgsqlBatch` verification fold, `FailureRank.Absorb` absence policy, `.api`-gated `ExtensionAdmission`, embedded residency-split ritual, `EngineOps` `Handle`-bridge capsule. **V4: `ServerFault` RE-BANDS off 835x to a fresh decade** (Columnar keeps 835x); all SEVEN loose `Error.New` become typed cases (`FailureRank` 8371-8373, readiness 8374-8375, `Admit` 8379-8380, AND `EmbeddedStore.Refused` 7701/7702 → `EmbeddedFault.Refused`, the register-missed third breach). The scale-out roster re-disposes per V13 (§6). Bands `Server 8380` (re-banded) + `Embedded 7710`. | `ClusterProvision.Verify/Admit/Reload` + `EmbeddedStore.Open` over `ServerExtension`/`StoreProfile` axes | in ← `AppHost/Observability` HEALTH_PROBE (ARCH:63, Npgsql-only; APPHOST:85 corrected); out → `cypher#ServerExtension.CreateSql` (frozen vocab); `Npgsql.OpenTelemetry` observability row | 4 |
| `Store/coordination.md` | NEW → `kind: new` | §2: ONE token-VALIDATING fenced-lease store — `CoordinationOp` op-union (BudgetDebit per-unit-vector, StepStateCas + InFlight/ExpiredScan reads, LeaseAcquire/Renew, MembershipUpsert), the NAMED `OUTBOX_SPINE` (Marten stream + `SyncCursor` drain cursor + at-least-once advance). Composes Marten/Npgsql primitives, no sidecar. Band `Coordination 8410`. | `Coordinate.Run(CoordinationOp) → CoordinationReceipt` (`[Union]`) | in ⇄ `AppHost/Agent/capability` Budget (APPHOST:76); ⇄ `AppHost/Runtime/orchestration` step-state (APPHOST:77); ⇄ `AppHost/Wire/outbox` (APPHOST:78); ⇄ `AppHost/Wire/coordination` (APPHOST:79); out → `Version/egress` cursor `[CURSOR]` | 4 |

### Governance docs (re-emit with the final page-set — land in the leg that changes their facts)

| Doc | Action | Change |
|---|---|---|
| `ARCHITECTURE.md` `[01]-[DOMAIN_MAP]` | `improve` | Codemap gains `Element/Authority.cs`, `Query/Retrieval.cs`, `Query/Federation.cs`, `Version/Egress.cs`, `Store/Coordination.cs`, `Ingest/Schedule.cs`; leg 1 (structure) + per-leg (fact) |
| `ARCHITECTURE.md` `[02]-[SEAMS]` | `improve` | Full corrected ledger (§5): ARCH:50/55/57 corrected, 4 wired-undeclared rows added, 4 coordination PORT rows added, 1 egress→coordination cursor edge; legs 1-4 as facts land |
| `README.md` `[01]-[ROUTER]` + package groups | `improve` | Router gains the 6 new pages; `[STORE_BACKENDS]`/`[EMBEDDED_KV]`/`[STREAMING_EGRESS]` groups re-emit to the V13 axis map + V3 sink rows; prune rows leave |
| `TASKLOG.md` / `IDEAS.md` | `improve` | Full card disposition (§10); phantom-realization corrected, stale `-[COMPLETE]` re-pointed, in the leg that changes each anchor |

---

## [§4] BAND REGISTRY (re-partitioned, duplicate-fails-at-type-init)

ONE `[SmartEnum<int>]` `PersistenceFaultBand` sited as a `[FAULT_TABLES]` owner block on `Element/graph.md` (the store-rail root every rail composes), mirroring `.archive/RASM-COMPONENT-PARADIGM-DECISION.md:141-149`. A duplicate integer fails the generated key lookup at type initialization; every fault union's `Code => Band + n`; every loose receipt code is a typed case or a registered sub-band row. Lands COMPLETE in leg 1 (every row assigned) so a duplicate is unrepresentable from the first leg; each union's re-derivation lands with its owning page's leg.

### Persistence-OWNED bands

| Band | Value | Owner page | Disposition | Union / cases |
|---|---|---|---|---|
| `RemoteStore` | 5400 | `Store/blobstore` | register-as-is | `RemoteStoreFault` |
| `Embedded` | 7710 | `Store/provisioning` | register-as-is + ABSORB 7701/7702 | `EmbeddedFault` 7711-7714 + `Refused` (was loose `Error.New` 7701/7702) |
| `Sync` | 8250 | `Version/ledger` | register-as-is | `SyncFault` 8251-8256 |
| `Commit` | 8260 | `Version/commits` | **MINT (H1, not register-as-is)** | `CommitFault`/`CrdtWireFault` 8261 decode-drift, 8263 parity-drift, 8264 owner-mints |
| `Retention` | 8280 | `Version/retention` | register-as-is | `RetentionFault` 8281-8283 |
| `Recovery` | 8290 | `Version/recovery` | register-as-is | `RecoveryFault` 829x |
| `Graph` | 8300 | `Element/graph` | register-as-is (keeps name `GraphFault`) | `GraphFault` 8300-8302 |
| `Codec` | 8310 | `Element/codec` | register-as-is (multi-decade stride 831x-833x) | `CodecFault` 8310/8320+Rank/8330 |
| `Identity` | 8340 | `Element/identity` | register-as-is | `IdentityFault` 834x |
| `Columnar` | 8350 | `Query/columnar` | KEEPS 835x (whole decade 8350-8356) | `ColumnarFault` 8350-8356 |
| `Cypher` | 8360 | `Query/cypher` | KEEPS 836x, **RENAME `GraphFault`→`CypherFault`** | `CypherFault` 8360-8363 |
| `Topology` | 8370 | `Query/topology` | KEEPS 837x | `TopologyFault` 8370-8371 (+`Cyclic`) |
| `Server` | 8380 | `Store/provisioning` | **RE-BAND off 835x collision** | `ServerFault` (was 8350-8352) + loose receipt codes (`FailureRank`/readiness/`Admit`) as typed cases 8380-8389 |
| `Tabular` | 8390 | `Ingest/tabular` | **RE-BAND off 837x collision** | `TabularFault` (was 8370-8373) 8390-8393 |
| `Retrieval` | 8400 | `Query/retrieval` | **MINT (replaces lane bare `Error.New` 8360-8363)** | `RetrievalFault` 8400-8403 |
| `Coordination` | 8410 | `Store/coordination` | **MINT (new owner)** | `CoordinationFault` `LeaseFenced`/`CasConflict`/`BudgetExhausted`/`LeaseExpired`/`MembershipStale` 8410-8416 |
| `Egress` | 8420 | `Version/egress` | **MINT (new owner)** | `EgressFault` `DeadLetter`/`SinkRefused`/`CursorStall`/`DeliveryUnconfirmed` 8420-8424 |
| `Federation` | 8430 | `Query/federation` | **MINT (new owner)** | `FederationFault` `PlanRejected`(WriteRelation)/`UnsupportedRelation`(fail-closed)/`TranscriptionFailed`/`SourceUncapable`(SQL-only) 8430-8435 |
| `Schedule` | 8440 | `Ingest/schedule` | **MINT (new owner)** | `ScheduleFault` `CodecReject`/`UnknownDialect` 8440-8442 |

The three E4 collisions resolve: `Columnar 835x` ∩ `ServerFault` → ServerFault re-bands to 8380; `Cypher 836x` ∩ lane codebook → codebook becomes `RetrievalFault` 8400; `Topology 837x` ∩ `Tabular`+provisioning-loose → Tabular re-bands to 8390, provisioning-loose folds into ServerFault 8380. The `GraphFault` simple-name ×2 resolves (cypher→`CypherFault`).

### PINNED-MIRROR foreign bands (reserved; telemetry-doc; never minted here — cross-package disjointness as rows, not prose)

| Band | Value(s) | Owner |
|---|---|---|
| AppHost | 1xxx / 4100-4810 | `Rasm.AppHost` (V1 precedent) |
| Compute | 2200-2299 | `Rasm.Compute` |
| Remote `WireFault` | 4520-4532 | `Rasm.Compute` (wire band) |
| AppUi | 6xxx | `Rasm.AppUi` |
| Component | 2300 | `Rasm.Materials/Component` |
| Generation | 2350 | `Rasm.Generation` (reserved) |
| Geometry | 2400 | `Rasm` kernel (pinned mirror of the kernel `GeometryFault` century 2400-2449, `Rasm/Numerics/faults.md`) |
| Material | 2450 | `Rasm.Materials/Appearance` |
| Projection | 2470 | `Rasm.Materials/Projection` |
| Element (kernel-AEC) | 2500 | `Rasm.Element` |
| Bim | 2600 | `Rasm.Bim` |
| Fabrication | 2700 | `Rasm.Fabrication` (persisted receipts decode 2701-2710, never 25xx — §10 forward constraint) |
| `Fault.UnsupportedCode` | 9104 | `Rasm/Domain/rails.md` (the only coded kernel-substrate case) |

---

## [§5] CORRECTED SEAM LEDGER

### Own `[02]-[SEAMS]` block — post-campaign (in-scope, Persistence-owned)

Corrections to the three declared-unwired + four wired-undeclared additions + four coordination PORT counterparts + one egress cursor edge. Unchanged live rows (ARCH:41,42,43,44,45,46,48,49,52,56,58,59,60,61,62,63,64) HOLD.

| Row | Kind | Correction |
|---|---|---|
| `ARCH:50` `Version/timetravel ← python:data/gridded/virtual` icechunk | declared-unwired → WIRED | Mint `AsOfKey`=`Checkpoint.Hash` on the LIVE `timetravel` page (§0/S3); target unchanged, capability realized |
| `ARCH:55` reconciliation `GeometryHash` | mis-targeted → RE-TARGETED | **`Query/topology` → `Version/merge#STRUCTURAL_DIFF`** (topology has zero reconciliation refs; `merge.md GraphNode.GeometryHash` is the real consumer). Digest-semantics resolved: the geometry `[V8]` unification rules whether merge's `GeometryHash` composes the kernel adjacency `Encode` or stays a Representations digest; kernel `Rasm/ARCHITECTURE.md:79` re-points onto the same home (geometry-campaign counterpart) |
| `ARCH:57` `Query/lane ⇄ python:data/tabular/query` ElementSet + Substrait | half-phantom → SPLIT | ElementSet currency stays `lane`; **Substrait portable plan → `Query/federation`** (V1 REINTRODUCE) |
| `Query/retrieval ⇄ Rasm.Compute/Model/embedding` VECTOR_CODEBOOK | wired-undeclared → ADD | `lane.md:381-388` wires, `COMPUTE:99` declares; new own row (post-V5b split targets `retrieval`) |
| `Query/cache ⇄ Rasm.AppHost/Runtime` CACHE_PORT L2 | wired-undeclared → ADD | `cache.md:191-232` wires, `APPHOST:69` declares; distinct from ARCH:58 (Compute INDEX) |
| `Version/ledger ← Rasm.AppHost/Wire/companion` PRESENCE PeerRoster | wired-undeclared → ADD | `ledger.md:467` `AwarenessLane`; the beat PRODUCER is companion `PeerRoster`, `DrainSurface` the transport; crosses as a Persistence-owned wire row |
| `Store/blobstore#Placement ⇄ Rasm.Compute GeometrySource` | prose-only → ADD | `blobstore.md:277,292` `BlobRemote`/`Placement`, consumer the above-seam Compute runner (C2) |
| `Store/coordination ⇄ AppHost/Agent/capability` Budget | NEW PORT | §2 `CoordinationOp.BudgetDebit` (APPHOST:76) |
| `Store/coordination ⇄ AppHost/Runtime/orchestration` step-state | NEW PORT | §2 `CoordinationOp.StepStateCas`+`InFlight` (APPHOST:77) |
| `Store/coordination ⇄ AppHost/Wire/outbox` outbox | NEW PORT | §2 `OUTBOX_SPINE` (APPHOST:78) |
| `Store/coordination ⇄ AppHost/Wire/coordination` lease+membership | NEW PORT | §2 `LeaseAcquire`/`MembershipUpsert` (APPHOST:79) |
| `Version/egress → Store/coordination` cursor | NEW `[CURSOR]` | §0/S2; forward-only (no reverse edge — acyclic) |

### Twelve+ stale sibling → Persistence rows (counterpart obligations; corrected targets; sibling interiors OUT of edit scope)

| Sibling row | Dead target | Corrected target | Owning campaign |
|---|---|---|---|
| `COMPUTE:111` FastCDC content-key delta | `Rasm.Persistence/Sync` | `Element/codec#CONTENT_CHUNKER` | Compute |
| `COMPUTE:115` parse-to-canonical Extract | `Query/pipeline` | `graph#STORE_RAIL` / `Ingest` | Compute |
| `COMPUTE:116` anomaly rule source | `Store/quality` | columnar/provisioning verification surface | Compute |
| `COMPUTE:119` Protobuf Kafka topics | `Sync` | retire w/ V3/V7 streaming disposition | Compute |
| `BIM:91` federation AuditEntry log | `Query/federation` | **`Query/federation` (V1 reintroduced, now live)** | Bim |
| `BIM:95` IFC validation rules | `Store/quality` | columnar/provisioning quality surface | Bim |
| `BIM:101` durable annotation + CDE | `Sync/annotation` | `ledger#CHANGEFEED` | Bim |
| `BIM:102` P6/MS-Project 4D | `Sync/schedule` | **`Ingest/schedule.md` (V11, now minted)** | Bim |
| `BIM:104` Speckle Base import | `Sync` | `Version/ledger` (Speckle SyncTransport) | Bim |
| `BIM:81` GDAL/OGR GeoParquet ingest (un-enumerated) | `/Store` (folder) | `columnar.md` / `Ingest/` axis, or retire | Bim |
| `BIM:94` `CommitGraph.MergeBase` (spelling) | — | align to `commits#MergeBase` (no `CommitGraph` type) | Bim |
| `APPHOST:71` drain 2PC in-doubt | `Query/transaction` | `graph#STORE_RAIL` (V12 prepared-tx RETIRE — the single-session spine mints no `pg_prepared_xacts`) | AppHost |
| `APPHOST:73` keyed OutboundHop egress | `Sync/egress` | **`Version/egress.md` (V3, now minted)** | AppHost |
| `APPHOST:85` health-probe driver roster | Npgsql/Redis/Kafka | `Store/provisioning`, Npgsql-only (matches ARCH:63; V7/V3 prune re-writes the roster) | AppHost |

---

## [§6] ROSTER DELTA (with `.api` obligations)

Central `Directory.Packages.props` Persistence block (`:249-317`) + `Rasm.Persistence.csproj` + README groups. Manifest motion lands in leg 1 (every ruled prune is zero-page-consumer). PROPS anchors are the disk-corrected values (api-manifests §1 confirms the manifest shifted -2 since brief authoring).

| Package | Disposition | Ruling / proof | `.api` obligation |
|---|---|---|---|
| `ClickHouse.Driver` (265) | PRUNE | V13 redundancy: TimescaleDB OLAP owns the scale in-PG; no consumer names a scale it cannot reach | catalog leaves; re-scope none |
| `ScyllaDBCSharpDriver` (309) | PRUNE | V13 redundancy: no named owning axis; wide-column absent | catalog leaves |
| `Qdrant.Client` (306) | PRUNE | V13 redundancy: pgvector/pgvectorscale in-PG own ANN | catalog leaves |
| `DeltaLake.Net` (274) | PRUNE | V13 redundancy: DuckDB columnar owns interchange; DuckLake catalog is the recorded forward extension-row candidate (not a NuGet) | catalog leaves |
| `rocksdb` (308) | PRUNE | V13 redundancy: SQLite embedded floor owns KV | catalog leaves |
| `LightningDB` (283) | PRUNE | V13 redundancy: SQLite embedded floor | catalog leaves |
| `PollinationSDK` (305) | PRUNE | charter misplacement — domain cloud SDK, no store-class concern | `api-pollination-sdk` leaves |
| `StackExchange.Redis` (312, direct) | PRUNE | L2 swap row keeps `Microsoft.Extensions.Caching.StackExchangeRedis`, transits the driver | `api-redis` leaves |
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` | PRUNE | no persisted NTS geometry column; H3 rides `pocketken.H3` | `api-npgsql-nts` leaves |
| `Npgsql.NetTopologySuite` | PRUNE | same | `api-nts-*` leaves |
| `NetTopologySuite.IO.GeoJSON4STJ` | PRUNE | same (csproj ref) | `api-nts-io` leaves |
| `NetTopologySuite.IO.GeoPackage` | PRUNE | same (csproj ref) | catalog leaves |
| `Microsoft.EntityFrameworkCore.Sqlite` | PRUNE | embedded floor is raw ADO by design (`provisioning.md:369-434`) | `api-ef-sqlite` leaves |
| `EFCore.NamingConventions` (277) | COMMIT | V6 `UseSnakeCaseNamingConvention()` | re-anchor `Schema/*`→`Element/identity` migration owner |
| `Microsoft.EntityFrameworkCore.Design` | COMMIT | V6 — the migration owner earns it (identity DDL) | re-anchor `api-ef-design`→identity migration |
| `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` (317) | COMMIT | V6 `UseValueObjectValueConverter()` | re-anchor `api-thinktecture-ef` |
| `FlowtideDotNet.Substrait` (279) | KEEP (ride V1) | probe FEASIBLE — the federation owner composes it | **CORRECT `api-flowtide-substrait.md:151`** (public `Deserialize` — the falsified claim) + re-anchor → `Query/federation` |
| `Apache.Arrow.Adbc` + `.Drivers.Apache` + `.Drivers.BigQuery` | KEEP (ride V1) | federation source rows (Substrait-native + SQL-only capability axis) | re-anchor → `Query/federation`/`columnar` |
| `Apache.Arrow.Flight` | KEEP (ride V1) | bulk-result hop | re-anchor off `Sync/egress`→`columnar`/`federation` |
| DuckDB `substrait` community extension | **ADD (ROW, not NuGet)** | probe G3 — `ColumnarExtension.Substrait` on `columnar.md`; verify a build for the pinned DuckDB v1.5.x | stub row in `api-duckdb.md` extension table |
| `Grpc.Tools` codegen assumption | **DROP** | probe — counterproductive (mints a duplicate CLR `Plan`); only `Google.Protobuf@3.35.1` (shared-tier) is used | no admission; no `.api` |
| `NATS.Net` (295) | KEEP (V3 anchor) | JetStream Settle ceremony + `Nats-Msg-Id` dedup | re-anchor `api-nats`→`Version/egress` |
| `CloudNative.CloudEvents` (+`.SystemTextJson`) | KEEP (V3 envelope) | the ONE egress envelope | re-anchor `api-cloudevents`→`Version/egress` |
| `pg_net` (extension row) | KEEP (V3 webhook) | server-side `net.http_post` response-reconciled sink | `api-pg-net` live-anchored |
| `Confluent.Kafka` + `Confluent.SchemaRegistry`(+.Serdes.Avro/.Json/.Protobuf) | SINK-ROW (ride V3) | one delivery row; leaves only on per-package redundancy proof | re-anchor `api-kafka`/`api-schemaregistry`→`Version/egress` |
| `Chr.Avro`(+`.Binary`+`.Confluent`) | SINK-ROW (ride V3) | the Kafka row's codec column | re-anchor `api-chr-avro*`→`Version/egress` |
| `CloudNative.CloudEvents.Kafka` | SINK-ROW (ride V3) | Kafka CloudEvents binding | re-anchor → `Version/egress` |
| `RabbitMQ.Client` (307) | SINK-ROW (ride V3) | one delivery row | re-anchor `api-rabbitmq`→`Version/egress` |
| `DotPulsar` (275) | SINK-ROW (ride V3) | one delivery row | re-anchor `api-dotpulsar`→`Version/egress` |
| `MPXJ.Net` | KEEP (ride V11) | consumer proven (BIM:102); `Ingest/schedule.md` mints | re-anchor `api-mpxj`→`Ingest/schedule` |
| `Ara3D.BimOpenSchema`(+`.IO`) | KEEP + **BUMP 1.0.1→1.6.1** | live columnar consumer (`columnar.md:405-490`); feed latest 1.6.1 (2026-07-03) — re-decompile for `AssemblyConfiguration("Debug")`, retire or escalate the DEBUG-IL caveat | re-anchor `api-ara3d-bimopenschema`→`columnar`; update version/DEBUG note |
| `linq2db.EntityFrameworkCore` | LEAVE (CONDITIONAL resolved) | tabular's charter is record-rail ingress, not bulk relational egress; DuckDB `COPY` + Marten append own the bulk lanes — no page composes a linq2db fence; the prose ref (`tabular.md:5,20`) drops | `api-linq2db-ef` leaves |
| `Npgsql.OpenTelemetry` (301) | KEEP | ruled default — the provisioning observability row names its tracing+metrics builder subscriptions at the composition root | `api-npgsql-otel` gains anchor → `Store/provisioning` |

### Cross-cutting `.api` obligations (both tiers)

- **Catalog re-anchor map** (dead-page → live): `Store/profiles`→`Store/provisioning`; `Store/remote`→`Store/blobstore`; `Store/encryption`→`Element/identity`; `Store/quality`→`columnar`+`provisioning`; `Store/server`→`provisioning`; `Store/tenancy`→`identity`; `Query/rail`/`Query/lanes`→`Query/lane`; `Query/transaction`→`graph#STORE_RAIL`; `Query/federation`/`Query/pipeline`→`Query/federation` (reintroduced)/`columnar`; `Version/snapshots`→`Version/timetravel`; `Sync/egress`→`Version/egress`; `Sync/coordination`→`Store/coordination`; `Sync/schedule`→`Ingest/schedule`; `Schema/{identity,converters,migration}`→`Element/{codec,identity}` + the V6 migration owner.
- **21 anchor-less catalogs gain anchors** — the core `api-marten`→graph/provisioning, `api-npgsql`→provisioning, `api-objectstore`→blobstore are first-class (the event-store, driver, blobstore spine).
- **`api-npgsql.md` catalog gap**: ADD advisory-lock (`pg_advisory_xact_lock`) + `LISTEN`/`NOTIFY` rows (V2 fenced lease + V3 pump wake; zero members catalogued today).
- **Misfiled shared `api-messagepack.md`**: delete (byte-identical dup, `shasum 5527490f`) or re-scope `RASM_PERSISTENCE`→neutral `RASM_API` (the landed `RASM_API_LANGUAGEEXT` precedent).
- **`api-h3.md:3`**: correct the `NetTopologySuite.IO.GeoJSON4STJ` (pruned) composition claim → transitive core-NTS 2.6.0 (`pocketken.H3` survives).
- **9 double-cataloged overlays** (`hashing/highperformance/hybrid-cache/jsonpatch/messagepack/nodatime-stj/nodatime/redaction/thinktecture-json`): UNION the divergent content into the shared-tier owner, collapse the package copy to a one-line pointer (8 DIVERGE — content must union before delete, not blind-delete).
- **Uniform provenance tag** `<!-- catalog:Pkg@ver -->` + `[STACKING]` section across the kept tier (currently 4/77 tagged; 0/31 shared) — the single machine-checkable version echo.
- **`api-languageext.md`**: DISCHARGED (already landed, `RASM_API_LANGUAGEEXT`, 21,804 B) — verify-only, never re-author.
- **Verify-or-die open confirm**: `CompleteMultipartUploadRequest.ChecksumXXHASH128` via `assay api` (AWSSDK.S3 registered as source) before the blobstore leg closes; the two enum members (`XXHASH128`/`FULL_OBJECT`) are catalogued.

---

## [§7] VERDICT DISPOSITION (V1-V13)

| V | Ruling (this draft) |
|---|---|
| V1 FEDERATION | **REINTRODUCE `Query/federation.md`** — probe FEASIBLE_WITH_GAPS; wire form CORRECTED (shipped-public `SubstraitDeserializer`, no `Grpc.Tools`); SetExpr lowering + columnar/ADBC lane over `LoweringTarget`; `SourceKind` capability axis; `(plan digest, cut, watermark)` receipt; retire NOT triggered (§0/S1, §1) |
| V2 COORDINATION | **NEW `Store/coordination.md`** — token-VALIDATING fenced lease, per-unit-vector Budget CAS, StepState CAS + InFlight/ExpiredScan reads, named `OUTBOX_SPINE`; all four PORT rows homed (§2). Store→3 pages |
| V3 EGRESS | **NEW `Version/egress.md`** — one pump, exactly-once-EFFECT CloudEvents envelope, per-sink dedup honesty; sinks Webhook(pg_net)/Nats/wire-native + Kafka/RabbitMQ/DotPulsar rows (§0/S2). Version stays 8 |
| V4 FAULT REGISTRY | **§4** — one `[SmartEnum<int>]` on `graph#[FAULT_TABLES]`; three collisions re-partitioned (Server→8380, Tabular→8390, Retrieval 8400); Commit MINTED (H1); pinned-mirror foreign rows; `GraphFault`×2 → `CypherFault` |
| V5 SPLITS+CYCLES | (a) `identity`→`authority` extraction; (b) `lane`→`retrieval` extraction; (c) `retention⇄blobstore StorageTier` frozen-vocab (default, no `Store/tier` extraction — no leg-4 owner amends it mid-campaign); (d) `graph←timetravel TimeCut` frozen-vocab. `recovery←blobstore ObjectStore.Head` + `cypher←provisioning ServerExtension` also frozen-vocab. ONE ruling binds all four; leg 4 may not alter a frozen vocabulary |
| V6 EF+EMBEDDED | COMMIT `UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()` (hand converters die, LanguageExt converters stay); migration owner on `identity` schema section (DDL authored, `EF.Design` earns admission); extension DDL rides `ServerExtension.CreateSql`; embedded floor charter stated (relational identity + `EngineOps`, read-only tier, never SoR); pgvector server-side ANN row lands on `retrieval` |
| V7 SCALE-OUT PRUNE | INTEGRATION-FIRST — the 6 scale-out backends leave on named V13 redundancy proofs (§6); PollinationSDK (charter), NTS bridges (no NTS column), EF-Sqlite (raw ADO), direct Redis (swap-row transit) leave with `.api`+README+csproj in one motion; SoR spine stays singular |
| V8 TYPE-REKEY | (a) consumer contract RECORDED (Type-seed excludes `Classifications`, waterfalled to Element/Materials; coupled — `[V8]`b waits on `[V8]`a's kernel seed change or ships its own classification-excluded seed); (b) `merge` gains the `ObjectKind.Type` correlation-key row NOW (RENAME not churn); migration case named onto `identity#IDENTITY_POLICY` expand/flip/contract |
| V9 INCREMENTAL OFGRAPH | consumer contract RECORDED on `codec#CONTENT_ADDRESS`; `Scrub`/`Bisect` re-shape to `OfGraph(prior,delta)` (interim full-recompute documented); kernel hasher never re-homed; geometry `[V2]` parametric digest waterfalled (must compose INTO `ToCanonicalBytes`, not beside — else `OfGraph` blind to a parametric-body edit, a false green on `[V12]` AsOfKey + this OfGraph proof) |
| V10 DEFECT SET | all repairs land with the owning page's improve: recovery RPO lag + WAL policy row; near-linear MergeBase; ledger batch fold + Truncate/Codec dispose; merge STJ options; blobstore Upload/Correlation/checksum honesty; retention `StorageLane.Durable`; provenance Principal-derived class; columnar trust gates + posture constant; codec HashPolicy shrink; timetravel `AggregateStreamToLastKnownAsync`. Plus 2 register-undercounts (provisioning 7701/7702 third breach; GCS/Minio dishonest checksum rows) + 2 new in-fence (topology `Lca` DAG-gate, tabular `Wire` dead carrier) |
| V11 INGEST | **NEW `Ingest/schedule.md`** — consumer PROVEN (BIM:102 + MPXJ admitted); MPXJ codec + durable schedule rows; `tabular` Sep charter explicit; Ingest→2 pages (fold-up alternative NOT triggered) |
| V12 GOVERNANCE | §5 ledger + §10 cards; AsOfKey re-realized on `timetravel`; prepared-tx RETIRE (no `pg_prepared_xacts`); Fabrication durable rows decode 2701-2710; all wired-undeclared rows added |
| V13 STORE AXIS | full axis map as first-class content (§6); SoR spine SINGULAR (one event store/materializer/identity/changefeed — unchallengeable seal); `ARCH:115` re-scopes to that spine boundary (a perimeter-axis engine row carrying unreachable capability is a legal DECISION admission, never a build-leg drift); each scale-out prune argued per-package |

---

## [§8] EVIDENCE DISPOSITION (E1-E14)

| E | Disk verdict | Disposition |
|---|---|---|
| E1 Phantom realization | HOLD (+ un-enumerated IDEAS:57-61, TASKLOG:46-47) | §10 — 3 federation phantoms re-anchor to V1 owner; 6+5 stale `-[COMPLETE]` re-point to real fold owners |
| E2 Orphaned roster | DRIFT (PROPS -2; Ara3D 1.0.1→1.6.1 REFUTED) | §6 — prunes on redundancy/charter proofs; Ara3D bumped |
| E3 Catalog drift | DRIFT (21 anchor-less not 16; messagepack byte-identical) | §6 — full re-anchor map; dup deleted; 9 overlays unioned |
| E4 Band collisions | DRIFT (columnar 8350-8356 whole decade; 837x three-way; anchors off-by-one) | §4 — one registry; Server/Tabular/Retrieval re-band; CypherFault rename |
| E5 Coordination gap | HOLD (bilateral, fully-specified) | §2 — four PORT rows homed on `CoordinationOp` |
| E6 Seam-ledger drift | HOLD (ARCH:55 target DRIFT) | §5 — ARCH:50/55/57 corrected; 4 wired-undeclared added; 12 sibling counterparts |
| E7 EF stack | HOLD | V6 — commit converters; migration owner; NTS/EF-Sqlite prune; pgvector server-side row |
| E8 Verified logic bugs | HOLD (recovery anchors -1: `:180`/`:171`/`:145`) | V10 — RPO lag, WAL policy, near-linear MergeBase, ledger fold, STJ options; MemberPath VERIFIED clean |
| E9 Dead carriers | HOLD (+ commits `VectorOrder`/`Seed`) | V10 — wire-or-die each; blobstore Upload/Correlation, ledger Truncate/Codec, retention Durable, timetravel AggregateStreamToLastKnown, codec HashPolicy |
| E10 Type-rekey gap | HOLD (element.md:295 classification-dependent proof) | V8 — merge Type-correlation key; kernel seed-exclusion waterfalled |
| E11 OfGraph hot paths | HOLD (Of(Node) vs span REFUTED — intentional; H3 edges the real issue) | V9 — incremental contract recorded; timetravel edges use seam `ContentAddress.Of` |
| E12 Mandate 1+2 clean | HOLD | frozen-wire law binds; CRDT_OP_SET pinned by commits; 2701-2710 forward constraint |
| E13 Folder+page overload | DRIFT (lane 520, retrieval span :267-520) | V5/V11 — authority + retrieval extractions; Ingest→2; frozen-vocab cross-leg edges |
| E14 Parameterization | DRIFT (columnar wider — {projection} raw SQL; :271 raw not bound) | V10 — typed `Identifier`/`StorePath`/`SecretName` gate; posture constant; provenance Principal class |

Beyond-register findings disposed: H1 commits third bare-`Error.New` (§4 MINT); H2 ~10 version-engine raw hasher mints (V9/SEAM_AND_RAIL re-anchor extended to leg 2); H3 timetravel edge keys (seam `ContentAddress.Of`); provisioning 7701/7702 (EmbeddedFault.Refused); GCS/Minio dishonest checksum (honest SDK-native rows); codec `:175` ByDomainId prose (dies); topology `Lca` DAG-gate; tabular `Wire` dead carrier; columnar 2 phantom spellings; `api-flowtide-substrait.md:151` FALSIFIED (public Deserialize); csproj AppHost ProjectReference reversal (§12).

---

## [§9] CAPABILITY-ESCALATION [03] DELTA DISPOSITION

| Plane | Now→Target | Closed by |
|---|---|---|
| Element (graph/codec/identity+authority) | 9→9.5 | authority extraction; EF commit + snake-case + generated converters (V6); DDL/migration owner; band registry sited (§4); incremental-OfGraph + parametric-digest + streaming-identity consumer contracts recorded (V9); raw-hash mints → `ContentHash.Of`; HashPolicy shrink |
| Version core (ledger/commits/timetravel) | 9→9.5 | near-linear MergeBase; dead vocab wired-or-dead; batched changefeed fold; `AsOfKey`=`Checkpoint.Hash` (§0/S3); `CRDT_OP_SET` pinned; Scrub incremental |
| Version merge | 8.5→9.5 | Type-correlation key row; MemberPath VERIFIED; STJ options threaded; migration case on `IdentityPolicy` |
| Version governance (provenance/retention/recovery) | 9→9.5 | real RPO lag; WAL-throughput policy row; Principal-derived class; `StorageLane.Durable` disposed |
| Query read lanes (lane/topology/columnar/cypher/cache) | 8.5→9.5 | retrieval extracted; `CypherFault` rename; columnar trust gates + posture constant + substrait extension row + phantom spellings; pgvector ruling; topology Lca DAG-gate; watermark/receipt preserved |
| Query retrieval (split) | —→9.5 | fusion + PQ codebook + `RetrievalFault` band; Compute codebook seam declared (§5) |
| Query federation (new) | 0→9 | §0/S1 + §1 — Substrait→SetExpr lowering; public-Deserialize ingress; `SourceKind` axis; ADBC lane; cut-pinned content-addressed receipt; python:data wire signature-locked; honest cards |
| Ingest (tabular+schedule) | 6.5→9 | folder law satisfied (schedule minted); Sep charter explicit; MPXJ codec + durable rows |
| Store (blobstore/provisioning) | 9→9.5 | receipt path composed; checksum honesty; loose codes typed (incl. 7701/7702); README bridge row reconciled |
| Store coordination (new) | 0→9.5 | §2 — token-VALIDATING lease; fenced per-unit-vector Budget CAS; CAS/membership op-union; named outbox + cursor; four PORT rows homed |
| Changefeed egress (new) | 0→9 | §0/S2 — one pump; exactly-once-effect envelope; NATS dedup + Settle-ack advance, pg_net response-reconciled advance; typed dead-letter + replay |
| Governance perimeter | 3→9.5 | §5/§6/§10 — zero phantom claims; ledger both directions; catalogs live-anchored; zero orphan admissions; type-enforced registry |

---

## [§10] CARD DISPOSITION (TASKLOG + IDEAS — all 30)

### TASKLOG OPEN (1)

| Card (line) | Disposition |
|---|---|
| `[ARTIFACT_CONTENT_KEY_FEDERATION]-[BLOCKED]` (:20) | RE-ANCHOR: the phantom "`Query/federation#ENTITY_GRAPH SourceKind.SignedArtifact` is realized" (:24) → the V1-reintroduced `Query/federation.md` (`SourceKind.SignedArtifact` a real source row) with honest BLOCKED status (Python C2PA producer gated); the content-key binding homes on `Version/provenance` (ARCH:51) |

### TASKLOG CLOSED (18)

| Card (line) | Disposition |
|---|---|
| `[PERSISTENCE_BIM_ARTIFACT_INDEX]` (:34) | HOLD — `cache#ARTIFACT_BLOB_INDEX` LIVE |
| `[RECOVERY_PAGE_AUTHOR]` (:35) | HOLD — `recovery.md` LIVE |
| `[RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT]` (:36) | HOLD — `RecoveryFault` 829x LIVE |
| `[TRANSACTION_PAGE_AUTHOR]` (:37) | RE-POINT — `Query/transaction.md` DELETED → `graph#STORE_RAIL` (own fold record); 2PC → V12 prepared-tx RETIRE |
| `[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]` (:38) | HOLD — `ledger#MERGE_LAW ConflictResult` LIVE (`ledger.md:225`) |
| `[ENCRYPTION_PAGE_AUTHOR]` (:39) | HOLD — records the settled fold (KMS→`identity#AUTHORITY`, SSE→`blobstore#BLOB_GC`) |
| `[SQLCIPHER_RESEARCH_PROMOTE]-[DROPPED]` (:40) | HOLD — dropped (embedded floor rejects cipher bundle) |
| `[QUALITY_PAGE_AUTHOR]` (:41) | RE-POINT — `Store/quality.md` DELETED → columnar/provisioning verification surfaces |
| `[PIPELINE_PAGE_AUTHOR]` (:42) | RE-POINT — `Query/pipeline.md` DELETED → `columnar` (DuckDB COPY/Arrow/Parquet bulk lane) |
| `[EGRESS_PAGE_AUTHOR]` (:43) | RE-POINT — `Sync/egress.md` DELETED → **`Version/egress.md` (V3, re-minted leg 4)** |
| `[SCHEMADDL_SQL_COLLAPSE]` (:44) | RE-POINT — resolves the `:44↔:45` contradiction: → the V6 migration owner (identity schema section); extension DDL rides `ServerExtension.CreateSql` |
| `[STORE_SERVER_SPLIT]` (:45) | HOLD — `provisioning.md` re-charter + tenancy fold LIVE (correctly states "no SchemaDdl.Sql owner"; :44 re-points to the V6 owner) |
| `[ANNOTATION_RELOCATE_TO_BIM]` (:46) | RE-POINT (doubly stale) — `Sync/annotation.md` + phantom `Query/federation#ENTITY_GRAPH` join key → durable annotation to `ledger#CHANGEFEED`, join on `GlobalId` (real owner); counterpart BIM:101 |
| `[SCHEDULE_RELOCATE_TO_BIM]` (:47) | RE-POINT — `Sync/schedule.md` DELETED → **`Ingest/schedule.md` (V11)**; counterpart BIM:102 |
| `[ICECHUNK_ASOF_CONTENT_KEY]` (:48) | RE-POINT — `Version/snapshots#SNAPSHOT_PROTOCOL AsOfKey` DELETED (phantom) → **`timetravel.md` (V12, AsOfKey=Checkpoint.Hash real)**; Ripple data [TENSOR_SPLIT] |
| `[KMS_PACKAGE_ADMISSION]` (:49) | HOLD — KMS trio pinned LIVE |
| `[ARROW_FLIGHT_PACKAGE_ADMISSION]` (:50) | HOLD — Arrow.Flight admitted (now rides V1) |
| `[KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION]` (:51) | HOLD — Kafka+CloudEvents pinned (now V3 sink-row-gated) |

### IDEAS OPEN (4)

| Card (line) | Disposition |
|---|---|
| `[PERSISTENCE_LIBRARY_TABLES]-[QUEUED]` (:19) | HOLD OPEN — re-anchor the durable library-table off the ARCHITECTURE prose onto the real owners (blobstore content rows + cache artifact index + identity/coordination durable tier), not a phantom Schema; honest QUEUED (Materials content-key producer gated) |
| `[FABRICATION_PROGRAM_DURABLE_ROWS]-[QUEUED]` (:26) | RE-ANCHOR — off dead `Schema/ddl#IDENTITY` (:30) → blobstore content rows + cache artifact index + V2/Store growth axis; KIND-AGNOSTIC `ArtifactKind` rows on the one artifact index under one `ContentHash.Of` fold; forward constraint: durable receipts decode 2701-2710 (never 25xx); honest QUEUED (Fabrication wire pins gated) |
| `[REUSE_WIRE]-[BLOCKED]` (:34) | RE-ANCHOR — phantom "source-agnostic `FederatedEntity`/`EntityGraph` is realized" (:38) → V1 `Query/federation.md` (SetExpr algebra + reintroduced owner); honest BLOCKED (python:data producer gated); Ripple data [REUSE_WIRE] |
| `[SUBSTRAIT_FEDERATION_SEAM]-[BLOCKED]` (:41) | RE-ANCHOR — phantom "`ElementSet`/`SetExpr` algebra + `FederatedPlan` is realized" (:45) → V1 `Query/federation.md` (Substrait decode → SetExpr lowering, real per probe); honest BLOCKED (python:data query producer gated); Ripple data [SUBSTRAIT_PORTABILITY]/[QUERY_IR_AND_SQLGATE] |

### IDEAS CLOSED (7)

| Card (line) | Disposition |
|---|---|
| `[PERSISTENCE_BIM_SYNC_CRDT]` (:55) | HOLD — `commits#CRDT_ALGEBRA CrdtField/Crdt.Apply` + `MergeBase` LIVE; align the `CommitGraph.MergeBase` spelling → `commits#MergeBase` (no `CommitGraph` type; BIM:94 counterpart); the O(V²) MergeBase repair lands with commits |
| `[DURABILITY_RECOVERY_OBSERVATORY]` (:56) | HOLD — `recovery.md` LIVE (the RPO-lag repair lands with recovery) |
| `[TRANSACTION_CONCURRENCY_CONTROL]` (:57) | RE-POINT — `Query/transaction.md StoreOp` DELETED → `graph#STORE_RAIL` |
| `[ENVELOPE_ENCRYPTION_KMS]` (:58) | RE-POINT — `Store/encryption.md` DELETED → `identity#AUTHORITY` (KMS custody) + `blobstore#BLOB_GC` (object-SSE) |
| `[DATA_QUALITY_INTEGRITY_FRAMEWORK]` (:59) | RE-POINT — `Store/quality.md` DELETED → columnar/provisioning verification surfaces |
| `[BULK_ETL_INTERCHANGE_PIPELINE]` (:60) | RE-POINT — `Query/pipeline.md` DELETED → `columnar` (DuckDB COPY/Arrow/Parquet) + `tabular` ingress |
| `[CDC_STREAMING_EGRESS]` (:61) | RE-POINT — `Sync/egress.md` DELETED → **`Version/egress.md` (V3)**; the sink roster (webhook/NATS/Kafka/RabbitMQ/Arrow-Flight/gRPC) aligns to the V3 sink-row axis |

---

## [§11] LEG PARTITION (acyclic; the listed order IS dependency order)

1. **SPINE + PERIMETER** — band registry COMPLETE (§4, every row incl. Retrieval/Coordination/Egress/Federation/Schedule bands + re-banded Server/Tabular so a duplicate is unrepresentable from leg 1); `Element/` (authority extraction V5a, EF commit + migration owner V6, codec consumer-contracts V9 + kernel-entry hash re-anchor + HashPolicy V10); FULL roster reconciliation + catalog re-anchor/dedup/stub (V7, §6 — gates every later import); own-ledger seam corrections (§5 half).
2. **VERSION ENGINE** — ledger/commits/timetravel/merge/provenance/retention/recovery V10 repairs; `AsOfKey` arm; `CRDT_OP_SET` pin on commits + the H2 version-engine hasher re-anchors (extended from leg 1's codec-only enumeration); V8 Type-correlation + migration case; V9 Scrub/Bisect incremental re-shape.
3. **QUERY + INGEST** — `retrieval` extraction + `RetrievalFault` (V5b); columnar trust gates + posture constant + substrait extension row; **`federation` owner** (V1, lowering onto retrieval+columnar which rebuild FIRST in this leg); cache/cypher/topology cold passes (`CypherFault` rename); `Ingest/` growth (schedule V11).
4. **STORE + COORDINATION + EGRESS** — blobstore V10 repairs; provisioning loose-code typing onto the registry (Server re-band, 7701/7702); **`Store/coordination.md`** (V2) lands BEFORE **`Version/egress.md`** (V3, drains the leg-4 outbox cursor + the leg-2 changefeed).

**Acyclicity proof.** Cross-leg edges are all frozen-vocabulary contracts (V5) an earlier leg consumes as a FROZEN shape a later leg may not alter: `graph`(1)←`timetravel#TimeCut`(2); `recovery`(2)←`blobstore#ObjectStore.Head`(4); `retention`(2)←`blobstore#StorageTier`(4); `cypher`(3)←`provisioning#ServerExtension`(4). The one NEW forward edge `Version/egress`(4)→`Store/coordination#OUTBOX_SPINE`(4) is intra-leg and forward-only (coordination never reads the pump). The federation owner(3) lowers onto `retrieval`(3)+`columnar`(3) which the leg orders first. No back edge exists; a leg-4 change to a frozen vocabulary reopens the consuming leg as a hard residual (the sequencing law).

---

## [§12] STRATA ACYCLICITY — the AppHost ProjectReference reversal

`Rasm.Persistence.csproj:10` carries `<ProjectReference Include="../Rasm.AppHost/Rasm.AppHost.csproj" />` — a DOWN/PEER edge reversing the PLACEMENT_LAW ("AppHost is a PORT peer; Persistence contributes rows and never reverses the dependency"). **RULING: REMOVE it.** The four PORT contracts are Persistence-owned types (`CoordinationOp`, `FencingToken`, `MembershipRow`, receipts) AppHost's `Wire/Coordination.cs`+`Wire/Outbox.cs` adapters DECODE — the direction is AppHost→Persistence. The AppHost-authored ingredient vocabularies Persistence consumes cross the PORT boundary WITHOUT a downward type reference: `ClockPolicy`/`TenantContext`/`CorrelationId`/`RecoveryObjective` enter as injected `ProjectionContext`/`ResolvedProfile` shapes Persistence defines; the `CostUnit` key crosses as its `[SmartEnum<string>]` STRING key (the Budget row is `HashMap<string,long>`, AppHost maps its smart-enum at the boundary); `TraceSlot`/`TenantId` cross as wire values; the `DataClassification` taxonomy rides the admitted shared-tier `Microsoft.Extensions.Compliance.Redaction` owner both packages compose (NOT an AppHost reference). This clears the strata-acyclicity gate: zero upward edges, zero Persistence→AppHost build-graph edge.

---

## [COUNTS]

- **Lens**: federation-egress-first
- **Page rows**: 24 (Element 4, Version 8, Query 7, Ingest 2, Store 3) — 6 new, 18 improve, 0 rebuild, 0 delete
- **Verdicts disposed**: 13 (V1-V13)
- **Evidence disposed**: 14 (E1-E14) + 11 beyond-register findings (H1-H3, provisioning 7701/7702, GCS/Minio checksum, codec ByDomainId prose, topology Lca, tabular Wire, columnar spellings, flowtide:151, csproj reversal)
- **Cards disposed**: 30 (TASKLOG 1 open + 18 closed; IDEAS 4 open + 7 closed)
- **Packages disposed**: 42 (13 prune, 3 commit, 11 sink-row, 5 ride-V1, 3 V3-anchor keep, 1 pg_net keep, 2 keep+bump, 3 conditional-resolved, 1 add-extension-row) + Grpc.Tools assumption dropped
- **AppHost PORT contracts homed**: 4/4 (BudgetDebit, StepStateCas+InFlight, OUTBOX_SPINE, LeaseAcquire+MembershipUpsert) on `Store/coordination#CoordinationOp`
- **Band registry**: 19 Persistence-owned bands + 13 pinned-mirror foreign rows; 3 collisions re-partitioned; 1 rename; 1 MINT correction (Commit)
