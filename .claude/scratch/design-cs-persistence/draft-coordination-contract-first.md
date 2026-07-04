# [DRAFT — COORDINATION-CONTRACT-FIRST] RASM-CS-PERSISTENCE structural blueprint

LENS: work BACKWARD from the four `Rasm.AppHost/ARCHITECTURE.md:76-79` PORT contracts — the token-VALIDATING fenced-lease store, the fenced Budget debit + step-state CAS as op-union cases, membership rows, the NAMED outbox and its egress pump — to the exact `Store/`+`Version/` structure that serves them. The PORT law is the design pressure that mints `Store/coordination.md` and `Version/egress.md`, re-partitions the band space to give them typed faults, and forces the strata-acyclicity resolution the coordination contract implies. Every other page, verdict, evidence row, card, and package is disposed to completeness — a blueprint leaving any disposition open loses at judge.

THESIS: the 18 fence interiors are world-class; the PORT law is the largest true capability gap. Four AppHost contracts dangle ownerless, and homing them is not one new page but a coordination SPINE — one `CoordinationOp` op-union owning fenced-lease/CAS/Budget/membership as cases, the Marten stream NAMED as the outbox with a durable cursor, and the egress pump draining that cursor under an exactly-once-effect envelope. Working backward from the contracts also forces the one structural correction the perimeter dossiers surface but no per-page pass fixes: the `Rasm.Persistence.csproj:10` `Rasm.AppHost` ProjectReference is a strata reversal — the PORT contract types are Persistence-owned and flow UP to AppHost, so the reference inverts and the shared runtime frame re-homes to the kernel.

FINAL PAGE-SET: 24 pages (Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3), zero one-file folders. Ruled-default map honored exactly — no departure carried disk proof of a stronger shape.

---

## [00]-[COORDINATION_DERIVATION] — the four PORT contracts walked BACKWARD to their homes

The AppHost PORT law is CONCRETE, not vague (sibling-ledgers E5): `Rasm.AppHost/.planning/Runtime/orchestration.md:257-282` fully specifies the `StepStateSeam` ripple; `Agent/capability.md:54-69` meters a real multi-unit `CostVector`. Each contract is HOMED on a Persistence-owned type, walked to its owning page and op-union case.

### [PORT_1] `APPHOST:76` fenced per-tenant Budget debit (`ONE_FENCED_LEASE_STORE`)

- HOME: `Store/coordination.md#BUDGET_LEDGER`.
- Op-union case: `CoordinationOp.Debit(LeaseToken Token, TenantId Tenant, MeterVector Request)`.
- Derivation: the debit is a fenced compare-and-decrement, PER-UNIT VECTOR (capability.md:62 `CostVector(HashMap<CostUnit,long> Units)` — a scalar debit is falsified by the consumer). The `budget_ledger` row carries N unit balances; ONE predicated `UPDATE budget_ledger SET balance_i = balance_i - debit_i WHERE tenant = @t AND lease_token >= @held AND balance_i >= debit_i FOR EVERY requested unit RETURNING balance` — the token check AND the non-negative guard land in one atomic predicate, so per-tenant accounting cannot double-spend across a fenced handoff, and a scalar-only debit forcing a client-side multi-statement span the fence cannot cover is the deleted form.
- Fault: `CoordinationFault.LeaseFenced(LeaseToken Stale, long Current)` (stale-holder resumed after handoff — Kleppmann fencing) or `CoordinationFault.BudgetExhausted(MeterUnit Unit, long Requested, long Available)`.
- Receipt: `CoordinationReceipt` slot `store.coord.debit` carrying the post-debit `MeterVector`.

### [PORT_2] `APPHOST:77` workflow step-state CAS (`ONE_FENCED_LEASE_STORE`)

- HOME: `Store/coordination.md#STEP_STATE`.
- Op-union cases: `CoordinationOp.StepCommit(LeaseToken Token, WorkflowKey Instance, StepKey Step, StepState State)` (fenced CAS — the AppHost `StepStateSeam.Commit(WorkflowInstance, WorkflowStep)→Fin<Unit>` counterpart, orchestration.md:271); `CoordinationOp.StepLoad(WorkflowKey Instance)` (`StepStateSeam.Load`); `CoordinationOp.InFlight(TenantContext Tenant)` — the READ case (`StepStateSeam.InFlight: Func<TenantContext, Fin<Seq<string>>>`, orchestration.md:271; the `CrashResume` flagship at :282 reads the durable in-flight set).
- Derivation: `[V2]`'s "READ cases beside guarded writes" is a HARD demand — a write-only union strands the crash-resume flagship and forces the AppHost-side table scan the PORT law forbids. The step-state CAS + fenced-token column land under the `TenantId` RLS predicate (orchestration.md:267), so the read cases are RLS-scoped by construction.
- Fault: `CoordinationFault.CasConflict(WorkflowKey, StepKey, StepState Expected, StepState Found)` / `CoordinationFault.LeaseFenced`.
- Receipt: `store.coord.step` / `store.coord.inflight`.

### [PORT_4] `APPHOST:79` CAS + fenced-lease + membership backing store (`ONE_FENCED_LEASE_STORE`)

- HOME: `Store/coordination.md#LEASE_STORE` + `#MEMBERSHIP`.
- Op-union cases: `Acquire(LeaseKey Key, HolderId Holder, Duration Ttl)` mints a MONOTONIC `LeaseToken` (fencing generation `++` via PG row-CAS `RETURNING generation`); `Renew(LeaseToken Token)`; `Release(LeaseToken Token)`; `ExpiredScan(TenantContext Tenant)` — the READ case (orphan-reclaim ingress); `Join(MembershipKey Group, MemberId Member, Duration Ttl)`; `Members(MembershipKey Group)` — the READ case (membership rows with lease-expiry semantics).
- Derivation — the fencing is RAISED from lease-ISSUING to token-VALIDATING: a fence a resource never checks is decorative. Every guarded write (`Debit`, `StepCommit`) carries the holder's token, and the guarded `UPDATE` REJECTS a token older than the row's current lease generation via the SAME PG row-CAS predicate that debits — a stale holder that resumed after a handoff is a typed `LeaseFenced` fault, never a lost update. Composes Npgsql `pg_advisory_xact_lock` (`api-npgsql.md` CATALOG GAP — leg 1 adds the advisory-lock rows) + PG row-CAS. Never a distributed-lock sidecar; `DistributedLock.Postgres` is the recorded rejection (no fencing tokens — the token-VALIDATING CAS is strictly stronger).
- Fault: `CoordinationFault.LeaseFenced` / `CoordinationFault.LeaseExpired(LeaseKey, HolderId)` / `CoordinationFault.MembershipLapsed(MembershipKey, MemberId)`.
- Receipt: `store.coord.lease` / `store.coord.membership`.

### [PORT_3] `APPHOST:78` transactional outbox same-tx (`ONE_OUTBOX_EGRESS_SPINE`)

- HOME (spine): `Store/coordination.md#OUTBOX` NAMES the Marten event stream as the outbox — events commit in the SAME `IDocumentSession` as state (the same-tx guarantee already holds through `graph#STORE_RAIL`'s one-session law), so the owner does not mint a second event store; it NAMES the existing stream, mints the durable drain cursor (`outbox_cursor(sink, long Sequence)`) and the at-least-once advance law.
- HOME (pump): `Version/egress.md#EGRESS_PUMP` (V3) drains the cursor past `OpLogEntry` rows into `EgressSink` delivery rows.
- Op-union case: `CoordinationOp.OutboxAdvance(SinkKey Sink, long Through)` — the cursor advance the pump calls; ONE pump fold, never a second changefeed. Composes Marten `FetchForWriting` (optimistic/exclusive stream locking) + `QueueSqlCommand` (same-session SQL) + `LISTEN`/`NOTIFY` pump wake (api-npgsql gap — leg 1 adds).
- Fault: `EgressFault` (842x, owned by `Version/egress.md`) on delivery; `CoordinationFault` on cursor-CAS.

### [BACKWARD_STRUCTURE] the exact Store/Version shape the contracts force

- ONE entry: `CoordinationStore.Run(IDocumentSession session, CoordinationOp op, Option<LeaseToken> token, TenantContext tenant, ClockPolicy clocks, CorrelationId correlation) : IO<Fin<CoordinationReceipt>>` — discriminates the closed `[Union]` `CoordinationOp` through the generated total `Switch`, EXACTLY `graph.md#STORE_RAIL` `GraphStore.Run`'s idiom (`libs/csharp/Rasm.Persistence/.planning/Element/graph.md:256`). One rail owns the bracket; the guarded-write legs share the fenced-CAS predicate fold; the read legs (`InFlight`/`ExpiredScan`/`Members`/`StepLoad`) share one `Received`-style projection.
- `Store/` grows to a real 3-page axis: `blobstore` · `provisioning` · `coordination` (NEW). No one-file-folder pressure — Store was already 2.
- `Version/` grows to 8: the seven engine pages + `egress` (NEW). The pump sits beside the changefeed it drains (`ledger#CHANGEFEED`), so a `Sync/` folder revival is unjustified (no second sync-shaped sibling — the folder law forbids the one-file `Sync/`).
- The ONE seam edge the siting adds: `Version/egress → Store/coordination#OUTBOX` (cursor read). No reverse edge — coordination NEVER reads the pump — so it is acyclic; both land in leg 4.
- Singletons preserved: the new owners JOIN the standing singletons (`graph#STORE_RAIL`'s one `IDocumentSession`, `timetravel`'s one materializer, `ledger`'s one changefeed), never mint parallels. The outbox IS the stream; the cursor is the only new durable-state row.

### [STRATA_RESOLUTION] the reversal the coordination contract forces (strata-acyclicity judge gate)

Working backward from the contract exposes the one build-graph reversal no per-page pass fixes (api-manifests §4): `Rasm.Persistence.csproj:10` carries a `Rasm.AppHost` ProjectReference — Persistence→AppHost, a strata reversal (`PLACEMENT_LAW`: AppHost is a PORT peer Persistence contributes rows to, never reverses). RULING:

- The coordination CONTRACT is Persistence-owned: `CoordinationOp`, `LeaseToken`, `MeterUnit`/`MeterVector` (the Persistence-owned metering vocabulary AppHost's `CostVector` maps onto at the boundary), `WorkflowKey`/`StepKey`/`StepState`, `MembershipKey`/`MemberId`, `CoordinationReceipt` — AppHost's `Wire/Coordination.cs`, `Agent/capability.cs`, `Runtime/orchestration.cs` DECODE them upward. `[V2]` law: no AppHost interface or type crosses down.
- The `Rasm.Persistence.csproj:10` `Rasm.AppHost` ProjectReference DELETES; the correct edge is AppHost→Persistence (the PORT-consumer direction), landed on the AppHost campaign.
- The shared runtime FRAME currently consumed downward (`ARCH:54` `ClockPolicy`/`CorrelationId`/`TenantContext`; `graph.md:222` `Principal` from `Rasm.AppHost/Agent/identity#PRINCIPAL`) re-homes to the kernel `Rasm` so BOTH strata consume it UPWARD — a KERNEL/APPHOST counterpart obligation (out of scope for Persistence interiors; recorded in the seam ledger). Until it lands, the interim is the frame passed as VALUES through the PORT adapter, never as AppHost types in Persistence signatures.
- Effect: zero upward edges — the strata-acyclicity gate closes. This ruling is the coordination lens's distinctive structural contribution; a federation-first or perimeter-first draft that leaves csproj:10 standing fails the gate.

---

## [01]-[PAGE_SET] — the final 24-page table (engine-native actions)

Schema per row: `path` | `semantic action` + `engine lowering` (kind `new|rebuild|improve`, absorb `{into,from}`) | `owner charter` (folder home · per-page skeleton · V4 band) | `entry signature` (op-union, or NONE for vocabulary) | `in/out seam edges` | `leg`. Deleted-page vocabulary is NOT a page row (those .md files are already gone from the element rebuild); it is re-anchored in the card + catalog tables. No `deletePages` rows: the corpus deletes no live page — the four SPLITs shed a section to a new sibling, the source page survives and improves.

| # | Path | Action + engine lowering | Owner charter (skeleton · band) | Entry signature | In/out seams (anchor, both directions) | Leg |
|---|---|---|---|---|---|---|
| 1 | `Element/graph.md` | KEEP → improve | store-rail root; +`[FAULT_TABLES]` band-registry owner block (V4); STORE_RAIL absorbs the `Query/transaction` fold (TASKLOG:37) · **GraphFault 830x** + the `FaultBand [SmartEnum<int>]` registry | `GraphStore.Run(session, identity, GraphStoreOp, actor, storeId, clocks, correlation) : IO<Fin<GraphReceipt>>` | IN `←Rasm.Element` (ElementGraph/GraphDelta, ARCH:41); `←Rasm` (kernel hash, ARCH:42); `←Rasm/frame` (ClockPolicy/CorrelationId/Principal — re-homed kernel, ARCH:54 corrected); consumes `timetravel.TimeCut` (FROZEN, V5d). OUT `→ledger#CHANGEFEED` (OpLog.Project input) | 1 |
| 2 | `Element/codec.md` | KEEP → improve | codec axis; ContentAddress seam-composed; V10 HashPolicy ruling (shrink to `{Identity,Content}` + header forward-compat, delete `:175` ByDomainId prose); V9 consumer contracts recorded · **CodecFault 831x-833x** (multi-decade stride, register as-is) | `Codec.Seal/Verify` static surfaces (NONE — vocabulary + static ops) | IN `←Rasm` kernel `ContentHash.Of` (ARCH:42, the ONE hasher entry). OUT `→typescript:wire` (SnapshotHeader+CBOR, ARCH:43) | 1 |
| 3 | `Element/identity.md` | SPLIT → improve (sheds authz) | relational tier + IdentityPolicy + KMS custody (signing+envelope on one KmsProvider axis, settled) + SchemaVerdict + the V6 DDL/migration owner; EF commit `UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()` · **IdentityFault 834x** | `IdentityStore.Stamp` + `SchemaGate` (NONE — relational surface, no single op-union) | IN `⇄Rasm.AppHost/Runtime` [PORT] TenantId-RLS + KMS-unwrap (ARCH:53 split: KMS/RLS stays; APPHOST:72 unchanged). OUT identity row co-committed via `graph#STORE_RAIL` | 1 |
| 4 | `Element/authority.md` | NEW → new; absorb `{into: Element/authority.md, from: Element/identity.md#[04]-[AUTHORITY] authz-subset (:287-341,465-484)}` | object-ACL authz set-algebra (`Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor`, zero KmsProvider) · **no new band** (denials are DATA; composes IdentityFault 834x if a rail is needed) | `Authority.Effective/Admit/LapsedFor` (NONE — pure set-algebra) | IN consumed by `commits.Movable` ACL gate (commits.md:70). OUT `ObjectAcl` store moves here (ARCH:53 split half) | 1 |
| 5 | `Version/ledger.md` | KEEP → improve | changefeed projection; V10 `Truncate`/`WholeRelation` wire-or-delete, `Codec`→Family-derived accessor, batched `ProcessEventsAsync` fold; AsOfKey-adjacent presence drain · **SyncFault 825x** (register as-is — the ONLY correct typed band adjacency in the lane) | `OpLog.Project` + `ChangefeedSubscription` (NONE — projection surface) | IN `⇄python:runtime/transport` (ARCH:48); `⇄Rasm.AppHost/Runtime` HLC+TraceSlot (ARCH:49); `←Rasm.AppHost/Wire/companion` PRESENCE PeerRoster (ADD, ledger.md:466-472); `←Rasm.AppUi/Collab/Editing` CHANGEFEED edit-intent window-read (ADD); `←Rasm.AppHost/Runtime/determinism` neutral-log window-read (ADD, ONE windowed-read case). OUT `→typescript:wire` | 2 |
| 6 | `Version/commits.md` | KEEP → improve | commit-DAG + CRDT algebra + CrdtOpWire flagship; **MINT `CommitFault`/`CrdtWireFault [Union]:Expected`** (H1 — was bare `Error.New` 8261/8263/8264 at :358,:400,:411, NO fault owner today); near-linear `MergeBase` (V10, protects BIM:94); dead carriers `VectorOrder`/`Order`+`ContentParityCorpus.Seed` wire-or-delete; pin `CRDT_OP_SET` MvRegister/opMerge fixture · **CommitFault 826x** (MINT, not "register as-is") | `CommitGraph`/`Crdt.Apply` static surfaces (NONE) | IN raw mints re-anchor through `ContentHash.Of` (H2). OUT `→typescript:wire` CrdtOpWire (ARCH:44); `⇄python:runtime/transport` parity corpus (ARCH:45); `→typescript:state` (ARCH:46); produces `CRDT_OP_SET` (kernel reconciliation.md:129 DESIGN-PIN) | 2 |
| 7 | `Version/timetravel.md` | KEEP → improve | AS-OF reconstruct/diff/scrub/bisect; **`AsOfKey`=`Checkpoint.Hash` arm** (V12 — re-realizes TASKLOG:48 on a live page; the ONE cross-runtime + recovery content-identity digest); Scrub/Bisect re-shape to incremental `OfGraph(prior,delta)` (V9, interim documented); H3 edge keys → seam `ContentAddress.Of(span)`; drop dead `AggregateStreamToLastKnownAsync` · **shares GraphFault/registry** (no own fault union today; TimeTravelReceipt typed) | `TimeLog.Reconstruct/ReconstructAt` + `TimeCut` vocabulary (NONE) | IN `←python:data/gridded/virtual` icechunk AsOfKey (ARCH:50 CORRECTED — member now minted). OUT `TimeCut` consumed by `graph` (FROZEN, V5d) | 2 |
| 8 | `Version/merge.md` | KEEP → improve | three-way structural merge; **V8 Type-correlation key row** (2nd correlation key for `ObjectKind.Type` — classification-independent seed identity mirroring `ExternalKey`, so a re-key diffs as RENAME); thread `ElementJson.Options` STJ (V10, :390); MemberPath NodeId typing VERIFIED clean (E8) · **shares registry** (MergeConflict typed) | `StructuralMerge.Reconcile` (NONE — diff surface) | IN `←Rasm/Spatial/reconciliation` GeometryHash (ARCH:55 RE-TARGETED here from topology; resolve adjacency-vs-Representations digest, C1). OUT `→typescript:wire` JsonPatch (ARCH:47) | 2 |
| 9 | `Version/provenance.md` | KEEP → improve | W3C-PROV + attested ledger; `ProvJson` reads agent class off `Principal` (V10, :257, kill unconditional `Person`); mutation-boundary annotation; `AgentKey` raw mint → `ContentHash.Of` (H2) · **shares registry** (attested-verify is the recovery template) | `ProvNode.Of` + `AttestedLedger` (NONE) | IN `←python:artifacts/provenance` signed-artifact binding (ARCH:51). OUT independent-`digestOf` verify reused by `recovery` | 2 |
| 10 | `Version/retention.md` | KEEP → improve | classification/retention + full-history GC; `StorageLane.Durable` wire-or-delete (V10, :36) · **RetentionFault 828x** (register as-is) | `RetentionSweep` executor (NONE — one receipted fold) | IN consumes `blobstore.StorageTier` (FROZEN, V5c) + `timetravel.TimeCut` (FROZEN); `←Rasm.Compute` Assessment blobs (ARCH:52). OUT retention class on `cache`/`blobstore` | 2 |
| 11 | `Version/recovery.md` | KEEP → improve | verified PITR; **real RPO lag** (V10 — freshest-blob-lag not `Duration.FromMinutes(absent.Count)` :180); WAL-throughput policy row (:171); `AsOfKey`=`Checkpoint.Hash` as the restore content-identity oracle (`RecoveryPoint.AsCut()`→AsOfKey, `RecoveryFault.VerifyFailed` fires on mismatch) · **RecoveryFault 829x** (register as-is) | `PointInTimeRestore`/`RecoveryRoutes` (NONE — static choreography) | IN consumes `blobstore.ObjectStore.Head` (FROZEN, V5c); `←Rasm.AppHost/Runtime` RPO/RTO objective (ARCH:64) | 2 |
| 12 | `Version/egress.md` | NEW → new | V3 CDC egress pump; ONE pump fold draining the V2 outbox cursor into `EgressSink` rows under ONE CloudEvents envelope; **exactly-once EFFECT** (`id`=`OpLogEntry.ContentKey`, `Sequence`=cursor `long`, `partitionkey`=`EntityKey`); NATS `Nats-Msg-Id` dedup + Settle-ack advance; pg_net response-reconciled advance (never fire-and-forget); typed dead-letter + replay · **EgressFault 842x** (NEW) | `EgressPump.Run(EgressOp, cursor, clocks, correlation) : IO<Fin<EgressReceipt>>` (op-union over sink rows) | IN `→Store/coordination#OUTBOX` cursor read (internal, acyclic); `←Rasm.AppHost/Wire` OutboundHop delivery-honesty axis (APPHOST:73 CORRECTED here). OUT `EgressSink` rows (NATS/Webhook/wire-native + Kafka/RabbitMQ/Pulsar sink-DATA rows) | 4 |
| 13 | `Query/lane.md` | SPLIT → improve (sheds fusion+codebook) | routing + `ElementSet`/`SetExpr` set-algebra; `StalenessWatermark`; `WaitForNonStale` production-entry smell noted · **shares registry** (RetrievalFault extracts) | `ReadRouter.Route` + `ElementSetAlgebra.Evaluate` (op-union over SetExpr) | IN `⇄python:data/tabular/query` ElementSet currency (ARCH:57 — Substrait half → federation). OUT feeds `commits.ContentParityCorpus.Contribute` (elementset parity) | 3 |
| 14 | `Query/retrieval.md` | NEW → new; absorb `{into: Query/retrieval.md, from: Query/lane.md#[04]-[FUSION_AND_CACHE]+#[05]-[VECTOR_CODEBOOK] (:267-520)}` | one coupled ANN owner (fusion + PQ codebook); **`RetrievalFault [Union]`** kills lane's bare `Error.New` 8360-8363 (the hardest E4 breach); pgvector `<->`/`<=>` LINQ as the server-side ANN row beside the in-process PQ/ADC (V6 MINE) · **RetrievalFault 840x** (NEW) | `Retrieval.Fuse`/`VectorCodebook.Train` (op-union) | IN `←Rasm.Compute/Model/embedding` VECTOR_CODEBOOK (ADD — was lane, post-split targets retrieval; COMPUTE:99). OUT ranked ElementSet | 3 |
| 15 | `Query/topology.md` | KEEP → improve | QuikGraph view + frozen incidence; **`Lca` DAG pre-gate** (NEW MEDIUM — `IsDirectedAcyclicGraph()` before `OfflineLeastCommonAncestor`, rail `TopologyFault.Cyclic`, symmetric with `Order`); `TypedEdge.IsContainment`/`Kind` dead-accessor prune; ARCH:55 re-targeted OFF topology · **TopologyFault 837x** (keep) | `TopologyQuery` (op-union) | IN inline projection. OUT `TopologyView.Of`→`ContentAddress.OfGraph` per-snapshot memo (E11 — stays Element-seam) | 3 |
| 16 | `Query/columnar.md` | KEEP → improve | DuckDB analytical lane; **E14 typed trust gates** (`Identifier`/`StorePath`/`SecretName` VOs on `Mount`/`Secret`/`Egress {projection}`/`Generation`; `{projection}`→composed SetExpr); posture constant (kill 4× `"80%"`/`"90%"`); **ADD `ColumnarExtension.Substrait` row** (DuckDB `from_substrait(blob)`, probe G3); rule the BimOpenSchema branch Persistence-owned generic (align ARCH:56); phantom-spellings closed (`ExecuteQueryAsync`, `BimData.WriteDuckDB`) · **ColumnarFault 835x** (keep, 8350-8356) | `ColumnarLane` (op-union) | IN `←Rasm.Bim/Model` BimOpenSchema (ARCH:56 — ruled Persistence-generic). OUT `⇄python:data/tabular` Arrow/ADBC (ARCH:62); executes federation tabular subtree | 3 |
| 17 | `Query/cypher.md` | KEEP → improve | self-hosted AGE + pgrouting; **`GraphFault`→`CypherFault` rename** (E4 simple-name collision resolves; band prose dies); `AgtypePath.Weight` dead carrier wire-or-collapse · **CypherFault 836x** (rename, keep 836x) | `GraphQuery` (op-union) | IN consumes `provisioning.ServerExtension.CreateSql` on the identity-migration rail (FROZEN, V5c). OUT AGE→ElementSet / pgrouting→H3Cell | 3 |
| 18 | `Query/cache.md` | KEEP → improve | compute-result reuse index; one-row `ArtifactKind` growth law PRESERVED (load-bearing for Compute V1 + Fabrication rows); L2 swap-row charter · **shares registry** (BenchmarkRow typed) | `CacheLane` + `CacheL2Store:IBufferDistributedCache` (op-union + port) | IN `←Rasm.Compute` INDEX (ARCH:58). OUT `⇄Rasm.AppHost/Runtime` CACHE_PORT L2 partition (ADD — cache.md:191-232, APPHOST:69) | 3 |
| 19 | `Query/federation.md` | NEW → new | **V1 REINTRODUCE** (probe FEASIBLE_WITH_GAPS); Substrait plan → `SetExpr` lowering fold (one `RelationVisitor` subclass, ~150-250 LOC, `LoweringTarget` discriminant per the probe [03] routing table) + the columnar/ADBC execution lane; cut-pinned content-addressed result (`plan-digest·cut·watermark` triple, plan-digest = `ContentHash.Of` over wire bytes); `SourceKind` capability axis (Substrait-native vs SQL-only) · **FederationFault 843x** (NEW — SubstraitParse/UnsupportedRelation fail-closed/WriteRelation-reject) | `Federation.Lower/Execute(FederationOp) : IO<Fin<(ElementSet, ArrowStream)>>` (op-union; SourceKind seed DATA) | IN `⇄python:data/tabular/query` Substrait wire (ARCH:57 Substrait half realized). OUT lowers onto `lane.SetExpr` + `columnar` execution; `←Rasm.Bim` federation AuditEntry (BIM:91 counterpart) | 3 |
| 20 | `Ingest/tabular.md` | KEEP → improve | rectangular-data owner; **Sep charter EXPLICIT** (fenced `Sep` delimited lane or the concern is MiniExcel-covered and the Sep roster row drops); `TabularWire.Wire` dead carrier wire-or-delete (NEW MEDIUM); `linq2db` CONDITIONAL (composed fence or leaves) · **TabularFault 839x** (RE-BAND off 837x) | `TabularSpec` (op-union — the one modality discriminant) | IN spreadsheet/CSV. OUT `→Rasm.Element` row-shape wire (ARCH:61) | 3 |
| 21 | `Ingest/schedule.md` | NEW → new | V11 `MPXJ.Net` schedule-file codec (.mpp/XER/PMXML → record rail) + durable schedule rows; the Persistence half of the relocated Bim CPM/4D domain · **ScheduleFault 844x** (NEW) | `ScheduleSource` (op-union over the record rail) | IN `←Rasm.Bim/schedule` P6/MS-Project (BIM:102 counterpart, the named consumer that clears V11's deciding criterion). OUT durable schedule rows | 3 |
| 22 | `Store/blobstore.md` | KEEP → improve | four-provider object store; V10 `Upload`/`BlobTransferReceipt` become THE composed receipt path (or both die); GCS/Minio checksum rows read honest SDK-native `Crc64`/`None` (not decorative `XxHash128`); `Correlation` threaded-or-dropped; confirm `CompleteMultipartUploadRequest.ChecksumXXHASH128` via assay (C1) · **RemoteStoreFault 540x** (register as-is); `StorageTier` FROZEN vocab | `ObjectStore` nine-delegate `ObjectLeg` (op-union) | IN `←Rasm.Compute` GLB (ARCH:59); `←Rasm.Bim/Exchange` IFC/BREP (ARCH:60); `←Rasm.AppUi/Collab/sync` snapshot-accelerator rows (ADD, V12). OUT `StorageTier`/`ObjectStore.Head` consumed FROZEN by retention/recovery | 4 |
| 23 | `Store/provisioning.md` | KEEP → improve | verification-first server tier + embedded floor; **`ServerFault` RE-BAND 838x** (was 8350-8352, collided Columnar); **7 loose `Error.New` → typed cases** (8371-8375/8379/8380 + `EmbeddedStore.Refused` 7701/7702 → `EmbeddedFault.Refused`); embedded-floor charter stated (relational floor + EngineOps, never SoR — Marten is PG-only); health roster Npgsql-only · **ServerFault 838x** (RE-BAND) + **EmbeddedFault 771x** (absorb loose) | `ClusterProvision.Verify/Admit/Reload` + `EmbeddedStore` (op-union) | IN self-provisioned PG18. OUT `←Rasm.AppHost/Observability` HEALTH_PROBE Npgsql-only (ARCH:63; APPHOST:85 corrected); `ServerExtension.CreateSql` consumed FROZEN by cypher | 4 |
| 24 | `Store/coordination.md` | NEW → new | **V2 the four PORT contracts**; `CoordinationOp` op-union (Acquire/Renew/Release/ExpiredScan/Debit/StepCommit/StepLoad/InFlight/Join/Members/OutboxAdvance); token-VALIDATING fenced lease; per-unit-vector fenced compare-and-decrement Budget; step-state CAS + InFlight read; membership lease-expiry rows; the NAMED outbox spine + durable cursor · **CoordinationFault 841x** (NEW) | `CoordinationStore.Run(session, CoordinationOp, Option<LeaseToken>, tenant, clocks, correlation) : IO<Fin<CoordinationReceipt>>` | IN `⇄Rasm.AppHost/Agent/capability` [PORT] Budget (APPHOST:76 counterpart); `⇄Rasm.AppHost/Runtime/orchestration` [PORT] step-state (APPHOST:77); `⇄Rasm.AppHost/Wire/outbox` [PORT] outbox (APPHOST:78); `⇄Rasm.AppHost/Wire/Coordination` [PORT] lease+membership (APPHOST:79). OUT `#OUTBOX`→`Version/egress` cursor (internal) | 4 |

Folder totals: Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3 = **24**, zero one-file folders. SPLITs (identity→+authority, lane→+retrieval) shed a section to a NEW sibling; the source survives and improves. NEW pages: authority, egress, retrieval, federation, schedule, coordination (6). No page is deleted.

---

## [02]-[BAND_REGISTRY] — the re-partitioned 83xx map (one `[SmartEnum<int>]`, duplicate fails at type-init)

Sited as the `[FAULT_TABLES]` owner block on `Element/graph.md` (the store-rail root every rail composes). `sealed partial class FaultBand : [SmartEnum<int>]` with `new(<band>, owner: "<Page#Owner>")` rows — the generated key lookup makes a duplicate integer fail at type initialization, mirroring `.archive/RASM-COMPONENT-PARADIGM-DECISION.md:141-149`. Every fault union derives `Code => Band + n`; every receipt-carried code resolves through the registry. Per-page decade prose (`topology.md:166`, `columnar.md:128`, `cypher.md:49`) DIES for one registry pointer.

### [OWN_BANDS] Persistence-owned (13 disjoint decades)

| Band | Owner union | Page | Codes | Registry action |
|---|---|---|---|---|
| 540x | `RemoteStoreFault` | Store/blobstore | 540x | register as-is |
| 771x | `EmbeddedFault` | Store/provisioning | 7711-7714 + fold 7701/7702 `Refused` | register + ABSORB loose (E4 undercount) |
| 825x | `SyncFault` | Version/ledger | 8251-8256 | register as-is |
| 826x | `CommitFault`/`CrdtWireFault` | Version/commits | 8261 decode-drift, 8263 parity-drift, 8264 owner-mints | **MINT typed `[Union]:Expected`** (H1 — NOT "register as-is"; was bare `Error.New`) |
| 828x | `RetentionFault` | Version/retention | 8281-8283 | register as-is |
| 829x | `RecoveryFault` | Version/recovery | 829x | register as-is |
| 830x | `GraphFault` | Element/graph | 8300-8302 | register as-is; registry HOST |
| 831x-833x | `CodecFault` | Element/codec | 8310/8320+Rank/8330 | register as-is (legal multi-decade stride) |
| 834x | `IdentityFault` | Element/identity | 834x | register as-is (authority composes it, no new band) |
| 835x | `ColumnarFault` | Query/columnar | 8350-8356 | keep 835x (whole decade, E4 span-widened) |
| 836x | `CypherFault` | Query/cypher | 8360-8363 | **RENAME** from `GraphFault` (simple-name ×2 resolves), keep 836x |
| 837x | `TopologyFault` | Query/topology | 8370-8371 | keep 837x |

### [REBANDED_AND_NEW] (7 fresh decades — the collision + new-owner resolutions)

| Band | Owner union | Page | Registry action |
|---|---|---|---|
| 838x | `ServerFault` | Store/provisioning | **RE-BAND** (was 8350-8352 ≡ Columnar) + absorb the 7 loose receipt codes 8371-8375/8379/8380 as typed cases |
| 839x | `TabularFault` | Ingest/tabular | **RE-BAND** off 837x (was 8370-8373 ≡ Topology + provisioning-loose) |
| 840x | `RetrievalFault` | Query/retrieval | **NEW** — kills lane's bare `Error.New` 8360-8363 (≡ Cypher), the hardest E4 breach |
| 841x | `CoordinationFault` | Store/coordination | **NEW** — LeaseFenced/BudgetExhausted/CasConflict/LeaseExpired/MembershipLapsed |
| 842x | `EgressFault` | Version/egress | **NEW** — dead-letter/delivery-honesty |
| 843x | `FederationFault` | Query/federation | **NEW** — SubstraitParse/UnsupportedRelation(fail-closed)/WriteRelation-reject/SqlOnlySource |
| 844x | `ScheduleFault` | Ingest/schedule | **NEW** — MPXJ codec faults |

The 8xxx space absorbs all owners at 540x/771x/825x-844x — the federation-wide renumber ([06]) is NOT triggered; the re-partition fits.

### [PINNED_MIRRORS] foreign neighborhoods (cross-package disjointness as ROWS, never prose)

| Mirror band | Foreign owner |
|---|---|
| 1xxx / 4100-4810 | AppHost |
| 2200-2299 | Compute |
| 4520-4532 | Compute Remote `WireFault` wire band |
| 6xxx | AppUi |
| 2300 / 2350 / 2400 / 2450 / 2470 / 2500 / 2600 / 2700 | AEC registry (Component/Generation/Geometry[mirror of kernel `GeometryFault` 2400-2449, `Rasm/.planning/Numerics/faults.md`]/Material/Projection/Element/Bim/Fabrication) |
| 9104 | kernel-substrate `Fault.UnsupportedCode` (`Rasm/.planning/Domain/rails.md`) |

Precedent: AppHost/AppUi registries pin theirs reciprocally (`RASM-CS-APPHOST-BRIEF.md [V1]`).

---

## [03]-[SEAM_LEDGER] — own rows corrected + 12 sibling counterpart obligations

### [OWN_CORRECTIONS] (Persistence-owned, in scope)

| ARCH row | Current | Corrected |
|---|---|---|
| ARCH:50 icechunk AsOfKey | declared-unwired (member absent) | MINT `AsOfKey`=`Checkpoint.Hash` on `timetravel`; target stays live `Version/timetravel`; kill TASKLOG:48 phantom |
| ARCH:53 identity KMS PORT | `ObjectAcl` + TenantId-RLS + KMS on one row | SPLIT: `ObjectAcl`→`Element/authority`; TenantId-RLS + KMS keyrings stay `Element/identity` (APPHOST:72 unchanged) |
| ARCH:55 reconciliation GeometryHash | targets `Query/topology` (zero refs — mis-targeted) | RE-TARGET → `Version/merge#STRUCTURAL_DIFF` (real consumer `GraphNode.GeometryHash`); resolve adjacency-`Encode` vs Representations-digest semantics (C1); kernel counterpart re-points `Rasm/ARCHITECTURE.md:79` (geometry V8) |
| ARCH:57 lane Substrait half | half-phantom | V1 REINTRODUCE: ElementSet currency stays `lane`; Substrait plan → `Query/federation` |
| ARCH:54 frame ingredients | `←Rasm.AppHost/Runtime` (downward) | re-home `ClockPolicy`/`CorrelationId`/`TenantContext`/`Principal` to kernel `Rasm` (strata resolution); csproj:10 AppHost ProjectReference DELETES |

ADD (wired-undeclared, own ledger):
- `Query/retrieval VECTOR_CODEBOOK ⇄ Rasm.Compute/Model/embedding` (post-V5b split; COMPUTE:99 declares).
- `Query/cache#L2_CONTRIBUTION ⇄ Rasm.AppHost/Runtime CACHE_PORT` (cache.md:191-232; APPHOST:69).
- `Version/ledger#PRESENCE ← Rasm.AppHost/Wire/companion PeerRoster` beats (ledger.md:466-472; driver corrected — companion roster produces, DrainSurface transports).

ADD (V2 coordination — four PORT counterparts on `Store/coordination`):
- `Store/coordination#BUDGET_LEDGER ⇄ Rasm.AppHost/Agent/capability [PORT]` (APPHOST:76).
- `Store/coordination#STEP_STATE ⇄ Rasm.AppHost/Runtime/orchestration [PORT]` (APPHOST:77).
- `Store/coordination#OUTBOX ⇄ Rasm.AppHost/Wire/outbox [PORT]` (APPHOST:78) + internal `→Version/egress`.
- `Store/coordination#LEASE_STORE ⇄ Rasm.AppHost/Wire/Coordination [PORT]` (APPHOST:79).

ADD (V3 egress + V12 changefeed reads):
- `Version/egress ← Rasm.AppHost/Wire OutboundHop` delivery-honesty axis (APPHOST:73 corrected).
- `Version/ledger#CHANGEFEED ← Rasm.AppUi/Collab/Editing` edit-intent window-read; `← Rasm.AppHost/Runtime/determinism` neutral-log window-read (ONE windowed-read case parameterized by origin/entity/window serves BOTH).
- `Store/blobstore ← Rasm.AppUi/Collab/sync` snapshot-accelerator rows (content-keyed, derivable-class retention, never SoR).

Prepared-transaction disposition: RETIRE — the single-`IDocumentSession` spine mints no `pg_prepared_xacts` in-doubt set, so the AppHost 2PC drain-row criterion resolves RETIRE (`RASM-CS-APPHOST-BRIEF.md [V11]`) unless a draft proves a prepared-tx owner on `graph#STORE_RAIL`. Ruled RETIRE.

### [SIBLING_COUNTERPART_OBLIGATIONS] (12 stale rows; sibling interiors OUT of edit scope, corrected targets listed for their campaigns)

| Sibling row | Dead target | Corrected target | Campaign |
|---|---|---|---|
| COMPUTE:111 FastCDC content-key delta | `Rasm.Persistence/Sync` | `Element/codec#CONTENT_CHUNKER` | Compute |
| COMPUTE:115 parse-to-canonical Extract | `Query/pipeline` | `graph#STORE_RAIL` (TASKLOG:37 fold) / `Ingest` | Compute |
| COMPUTE:116 anomaly rule source | `Store/quality` | `columnar`/`provisioning` verification surface | Compute |
| COMPUTE:119 Protobuf Kafka topics | `Sync` | RETIRE w/ V3/V7 streaming disposition | Compute |
| BIM:91 federation AuditEntry log | `Query/federation` (deleted) | `Query/federation.md` (V1 REINTRODUCED — the target re-lives) | Bim |
| BIM:95 IFC validation rules | `Store/quality` | `columnar`/`provisioning` quality surface | Bim |
| BIM:101 durable annotation + CDE | `Sync/annotation` | `ledger#CHANGEFEED` | Bim |
| BIM:102 P6/MS-Project 4D | `Sync/schedule` | `Ingest/schedule.md` (V11 — the named consumer clearing the deciding criterion) | Bim |
| BIM:104 Speckle Base import | `Sync` | `Version/ledger` (Speckle SyncTransport) | Bim |
| APPHOST:71 drain 2PC in-doubt | `Query/transaction` | `graph#STORE_RAIL` (prepared-tx RETIRE) | AppHost |
| APPHOST:73 keyed OutboundHop egress | `Sync/egress` | `Version/egress.md` (V3) | AppHost |
| APPHOST:85 health-probe driver roster | Npgsql/Redis/Kafka | `Store/provisioning`, Npgsql-only (matches ARCH:63; V7/V3 prune) | AppHost |

Additional (un-enumerated, recorded for completeness): BIM:81 GDAL/OGR GeoParquet ingest (`/Store` folder) → `columnar.md`/`Ingest/` axis or retire; BIM:94 `CommitGraph.MergeBase` spelling → align to `commits#MergeBase`.

---

## [04]-[ROSTER_DELTA] — package dispositions with `.api` obligations

Central owner `Directory.Packages.props` Persistence block (`:251`, label-grouped, hand-edited) + `Rasm.Persistence.csproj`. Manifest motion lands in LEG 1 (every ruled prune is zero-page-consumer, safe). `.api` obligations per `[ROSTER_RECONCILIATION]`: every kept catalog re-anchors to the live page set, the 21 anchor-less gain anchors, uniform `<!-- catalog:Pkg@ver -->` tag + `[STACKING]` section lands, the misfiled shared `api-messagepack.md` deletes/re-scopes, the 8 divergent overlays union into shared owners. Every PROPS anchor `-2` from the brief (manifest shifted since authoring; corrected below). License gate enforced.

| # | Package | PROPS | Disposition | `.api` obligation |
|---|---|---|---|---|
| 1 | `ClickHouse.Driver` | 265 | V13 re-dispose: PRUNE on redundancy proof vs in-PG TimescaleDB OLAP (no owning axis row) | catalog + README `[STORE_BACKENDS]` row leave |
| 2 | `ScyllaDBCSharpDriver` | 309 | V13: PRUNE (wide-column, no named owning axis → redundancy rules) | catalog leaves |
| 3 | `Qdrant.Client` | 306 | V13: PRUNE vs pgvector/pgvectorscale in-PG (no proven ceiling) | catalog leaves |
| 4 | `DeltaLake.Net` | 274 | V13: PRUNE (no Delta-wire columnar demand); DuckLake catalog row is the recorded forward candidate | catalog leaves |
| 5 | `rocksdb` | 308 | V13: PRUNE vs SQLite embedded floor (no proven ceiling) | catalog + README `[EMBEDDED_KV]` leave |
| 6 | `LightningDB` | 283 | V13: PRUNE vs SQLite floor | catalog leaves |
| 7 | `PollinationSDK` | 305 | PRUNE (charter misplacement — domain cloud SDK, no store concern) | `api-pollination-sdk`→dead `Store/remote`, deletes |
| 8 | `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` | 300 | PRUNE (no persisted NTS column) | catalog leaves |
| 9 | `Npgsql.NetTopologySuite` | 302 | PRUNE (NTS bridge) | catalog leaves |
| 10 | `NetTopologySuite.IO.GeoJSON4STJ` | 137 | PRUNE (NTS bridge) | catalog leaves; `api-h3.md:3` corrects to transitive core-NTS 2.6.0 |
| 11 | `NetTopologySuite.IO.GeoPackage` | 138 | PRUNE (NTS bridge) | catalog leaves |
| 12 | `Microsoft.EntityFrameworkCore.Sqlite` | 292 | PRUNE (embedded floor is raw ADO by design) | `api-ef-sqlite`→dead `Store/profiles`, deletes |
| 13 | `StackExchange.Redis` (direct) | 312 | PRUNE (transits via `Microsoft.Extensions.Caching.StackExchangeRedis`; swap-row keeps the wrapper) | catalog leaves |
| 14 | `EFCore.NamingConventions` | 279 | COMMIT (V6 `UseSnakeCaseNamingConvention()`) | re-anchor `api-ef-naming` → V6 migration owner (Element/identity) |
| 15 | `Microsoft.EntityFrameworkCore.Design` | 291 | COMMIT (V6 — earns admission at the DDL/migration owner) | re-anchor `api-ef-design` → Element/identity schema section |
| 16 | `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` | 316 | COMMIT (V6 `UseValueObjectValueConverter()`) | re-anchor `api-thinktecture-ef` → Element/identity |
| 17 | `NATS.Net` | 297 | KEEP-COMMIT (V3 anchor — JetStream Settle + `Nats-Msg-Id` dedup); KV/ObjectStore recorded-unconsumed | re-anchor `api-nats` → Version/egress |
| 18 | `CloudNative.CloudEvents` | 268 | KEEP-COMMIT (V3 ONE envelope) | re-anchor `api-cloudevents` → Version/egress |
| 19 | `CloudNative.CloudEvents.SystemTextJson` | 270 | KEEP-COMMIT (V3 envelope formatter) | catalog → Version/egress |
| 20 | `CloudNative.CloudEvents.Kafka` | 269 | SINK-ROW (V3 default) | catalog → Version/egress sink row |
| 21 | `Confluent.Kafka` | 271 | SINK-ROW (V3 default; leaves only on redundancy proof) | re-anchor `api-kafka` → Version/egress |
| 22 | `Confluent.SchemaRegistry` | 272 | SINK-ROW codec column | catalog → Version/egress |
| 23 | `Confluent.SchemaRegistry.Serdes.Avro` | 273 | SINK-ROW codec | catalog → Version/egress |
| 24 | `Confluent.SchemaRegistry.Serdes.Json` | 274 | SINK-ROW codec | catalog → Version/egress |
| 25 | `Confluent.SchemaRegistry.Serdes.Protobuf` | 275 | SINK-ROW codec (COMPUTE:119 counterpart) | catalog → Version/egress |
| 26 | `Chr.Avro`(+`.Binary`+`.Confluent`) | 264-266 | SINK-ROW codec column (3 pkgs ride the Kafka row) | catalogs → Version/egress |
| 27 | `RabbitMQ.Client` | 309 | SINK-ROW (V3 default) | re-anchor `api-rabbitmq` → Version/egress |
| 28 | `DotPulsar` | 277 | SINK-ROW (V3 default) | re-anchor `api-dotpulsar` → Version/egress |
| 29 | `FlowtideDotNet.Substrait` | 279 | KEEP (V1 REINTRODUCE — probe FEASIBLE; `SubstraitDeserializer.Deserialize` public) | re-anchor `api-flowtide-substrait` → Query/federation; FLIP `:151` (public round-trip) |
| 30 | `Apache.Arrow.Adbc` | 253 | KEEP (V1 execution lane) | re-anchor `api-adbc-apache` → Query/federation+columnar |
| 31 | `Apache.Arrow.Adbc.Drivers.Apache` | 254 | KEEP (V1 source row) | catalog → Query/federation |
| 32 | `Apache.Arrow.Adbc.Drivers.BigQuery` | 255 | KEEP (V1 source row, `SourceKind` SQL-only scoped, G3) | catalog → Query/federation |
| 33 | `Apache.Arrow.Flight` | 257 | KEEP (V1 bulk-result hop) | re-anchor `api-arrow` Flight → Query/federation |
| 34 | `MPXJ.Net` | 296 | KEEP (V11 — `Ingest/schedule.md` mints; BIM:102 consumer) | re-anchor `api-mpxj` → Ingest/schedule |
| 35 | `linq2db.EntityFrameworkCore` | 286 | CONDITIONAL — composed fence in V11 tabular BulkCopy or LEAVES (currently prose-only) | catalog → Ingest/tabular or leaves |
| 36 | `Npgsql.OpenTelemetry` | 303 | KEEP (provisioning observability row — tracing+metrics at AppHost root) | anchor `api-npgsql-otel` → Store/provisioning |
| 37 | `Ara3D.BimOpenSchema` | 258 | KEEP + **BUMP 1.0.1→1.6.1** (feed latest 2026-07-03; re-verify DEBUG-IL) | re-anchor `api-ara3d-bimopenschema` → Query/columnar |
| 38 | `Ara3D.BimOpenSchema.IO` | 259 | KEEP + BUMP 1.0.1→1.6.1 | catalog → Query/columnar |
| 39 | `Grpc.Tools` (shared 55) | — | DROP the V1 codegen ASSUMPTION (probe: unnecessary + counterproductive — regenerating substrait `.proto` mints a duplicate CLR `Plan` the shipped deserializer rejects by identity); `Google.Protobuf@3.35.1` (shared) is the sole runtime dep, already admitted | no admission; note in Query/federation |
| 40 | DuckDB `substrait` community extension | — | **ADD** as `ColumnarExtension.Substrait` ROW (probe G3; MIT; not a NuGet — the DuckLake-precedent extension-row admission); verify a build vs the pinned `DuckDB.NET.Data.Full` v1.5.x runtime | `ColumnarExtension` seed-DATA row on Query/columnar |
| 41 | `api-messagepack.md` (shared, misfiled dup) | — | DELETE the byte-identical package copy; re-scope shared `RASM_PERSISTENCE`→neutral `RASM_API` (per landed `RASM_API_LANGUAGEEXT`) | catalog motion, leg 1 |

Recorded rejections (never re-proposed): `WolverineFx` (parallel envelope-table outbox vs stream-IS-outbox law), `DistributedLock.Postgres` (no fencing tokens — token-VALIDATING CAS strictly stronger), `MemoryPack` (C#-only wire vs frozen msgpack bit-parity), `LoroCs` (Rust-binding CRDT wire vs `CrdtOpWire [Key]` bit-parity). CONDITIONAL-FORWARD: `Apache.Arrow.Flight.Sql` (only if V1 charters a standards Flight-SQL endpoint). R14 `api-languageext.md` DISCHARGED (already landed, substantive — verify-only). ADD demanded: NONE (verified verdict — the domain-gap sweep found every scoped concern owned by an admitted package).

---

## [05]-[VERDICT_DISPOSITION] (V1-V13)

| V | Ruling |
|---|---|
| V1 FEDERATION | **REINTRODUCE** `Query/federation.md` (probe FEASIBLE_WITH_GAPS overturns the wire-form premise in the owner's favor). Substrait plan bytes → `SubstraitDeserializer.Deserialize` (PUBLIC, ~2 lines) → `RelationVisitor` lowering onto `SetExpr` + columnar/ADBC. DROP `Grpc.Tools`; `SourceKind` capability axis; ADD DuckDB substrait extension row. Retire criterion NOT triggered (no second engine). Cards re-anchor honest. |
| V2 COORDINATION | NEW `Store/coordination.md` — the four PORT contracts as `CoordinationOp` cases; token-VALIDATING fenced lease; per-unit-vector fenced compare-and-decrement Budget; step-state CAS + READ cases (InFlight/ExpiredScan/Members/StepLoad); membership lease-expiry; NAMED outbox spine + cursor. `Store/`→3. csproj strata reversal resolved ([00]). |
| V3 EGRESS | NEW `Version/egress.md` (beside the changefeed — no `Sync/` revival). ONE pump over the outbox cursor; exactly-once-EFFECT envelope (`id`=`ContentKey`, `Sequence`=cursor, `partitionkey`=`EntityKey`); NATS dedup + Settle-ack advance, pg_net response-reconciled advance; sink rows as seed DATA. `Version/egress→Store/coordination` edge acyclic. |
| V4 BAND REGISTRY | `[SmartEnum<int>]` on `graph.md#[FAULT_TABLES]`; 13 own bands + 7 rebanded/new + pinned mirrors ([02]). **Commit 826x MINTED** (H1 correction — not "register as-is"). Cypher renames `CypherFault`. Loose codes → typed cases. |
| V5 SPLITS+CYCLES | (a) `identity`→+`authority` (authz set-algebra). (b) `lane`→+`retrieval` (fusion+codebook, RetrievalFault). (c) `StorageTier` stays blobstore-owned FROZEN vocab (no `Store/tier` extraction). (d) `TimeCut` stays timetravel-owned FROZEN. All four inversions bind as frozen contracts consumed across legs; a leg-4 change reopens the consuming leg as a hard residual. |
| V6 EF+EMBEDDED | COMMIT `UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()`; hand `NodeId` converter + ~13 `HasColumnName` die; LanguageExt-type converters stay. DDL/migration owner sites on `Element/identity` schema section (`Microsoft.EntityFrameworkCore.Design` earns admission). Embedded floor charter stated (relational + EngineOps, never SoR — Marten PG-only). pgvector `<->`/`<=>` LINQ = server-side ANN row on `retrieval`. |
| V7 SCALE-OUT+ORPHAN | INTEGRATION-FIRST: 6 scale-out backends PRUNE on redundancy proofs (V13); PollinationSDK/NTS bridges/EF-Sqlite/direct-Redis leave manifest+csproj+README+`.api` in one leg-1 motion. SoR-spine seal stands. |
| V8 TYPE-REKEY | (a) consumer contract RECORDED for the Materials/Element campaign (exclude `Classifications` from `NodeId.RootedType` seed — coupled to `[V8]`a landing kernel-side per element.md:295). (b) `merge.Reconcile` gains the 2nd correlation key row for `ObjectKind.Type` NOW (classification-independent Type natural key → re-key diffs as RENAME); executing wave stays `identity#IDENTITY_POLICY` expand/flip/contract. |
| V9 INCREMENTAL OFGRAPH | consumer contract, not a re-home. `codec#CONTENT_ADDRESS` records the delta-composable `OfGraph(prior,delta)` (Element seam owner's to provide, `delta.md:55` hook); Scrub/Bisect re-shape to fold the incremental form; whole-graph recompute documented interim. Geometry `[V2]` parametric-digest waterfall SHARPENED (digest is a COMPONENT of `ToCanonicalBytes`, not a sibling key) — Persistence the demanding consumer. |
| V10 DEFECT SET | recovery real RPO lag + WAL-throughput policy row; commits near-linear MergeBase + dead vocab; ledger Truncate/Codec/batched fold; merge STJ options + MemberPath verified; blobstore Upload receipt path + checksum honesty + Correlation; retention StorageLane.Durable; provenance Principal-agent-class; columnar trust gates + posture constant; codec HashPolicy shrink. PLUS H1/H2/H3 (commits fault MINT, version-engine hash re-anchor, timetravel edge-key seam) — brief-adjacent, disposed. |
| V11 INGEST GROWTH | `tabular` KEEPS (Sep charter explicit); NEW `Ingest/schedule.md` (MPXJ codec + durable rows) — BIM:102 is the named durable-schedule consumer clearing the deciding criterion, so `MPXJ.Net` stays and Ingest→2. Fold-up alternative NOT triggered. |
| V12 GOVERNANCE | every phantom-realization clause corrects; six stale `-[COMPLETE]` cards re-point; `[FABRICATION_PROGRAM_DURABLE_ROWS]` re-anchors off dead `Schema/ddl#IDENTITY` onto blobstore rows + cache artifact index (2701-2710 decode constraint); ledger both directions complete ([03]); AsOfKey/reconciliation/Substrait own rows corrected; prepared-tx RETIRE. |
| V13 STORE AXIS | store perimeter PARAMETERIZED — every store-class concern names axis/provider-rows/selection-policy as DATA. TWO boundaries hold: SoR spine SINGULAR (one event store/materializer/identity/changefeed — unchallengeable seal); `ARCHITECTURE.md:115` re-scopes to that spine boundary (a perimeter-axis engine row reaching capability the in-PG owner cannot is a legal DECISION-level admission, never "a new relational engine row"). |

---

## [06]-[EVIDENCE_DISPOSITION] (E1-E14)

| E | Status | Disposition |
|---|---|---|
| E1 Phantom realization | HOLD (+ un-enumerated IDEAS:57-61, TASKLOG:46-47) | every phantom clause corrects (V1/V12); six stale COMPLETE re-point; card table [09] disposes all |
| E2 Orphaned roster | DRIFT (PROPS -2; Ara3D 1.0.1→1.6.1 REFUTED-latest) | roster delta [04] executes; scale-out V13; Ara3D BUMP + DEBUG-IL re-verify |
| E3 Catalog drift | DRIFT (21 anchor-less not 16; messagepack byte-identical dup) | re-anchor map [04]; core `api-marten`/`api-npgsql`/`api-objectstore` gain anchors (graph/provisioning, provisioning, blobstore); dup deletes |
| E4 Band collisions | DRIFT (columnar 8350-**8356**; 837x THREE-way) | V4 registry [02] — one `[SmartEnum<int>]` resolves all three decades + bare `Error.New` + `GraphFault`×2 |
| E5 Coordination gap | HOLD (bilateral; AppHost fully-specified) | V2 `Store/coordination` [00] homes all four PORT rows + counterpart ledger rows |
| E6 Seam-ledger drift | HOLD (ARCH:55 target DRIFT) | own corrections + 12 sibling obligations [03] |
| E7 EF stack | HOLD | V6 commit; hand-mapping dies; DDL owner sites on identity; NTS bridges prune |
| E8 Verified logic bugs | HOLD (recovery anchors -1: :180/:171/:145) | V10 repairs; MemberPath VERIFIED clean (resolves the "unverified" concern) |
| E9 Dead carriers | HOLD (+ commits `VectorOrder`/`Order`, `ContentParityCorpus.Seed`) | V10 wire-or-delete each; the two commits carriers added |
| E10 Type-rekey gap | HOLD (element.md:295 load-bearing proof) | V8 merge Type-correlation key; `[V8]`a-`[V8]`b coupling recorded |
| E11 OfGraph hot paths | HOLD (Of(Node,tol) split REFUTED as defect — intentional; H3 EDGE keys are the real issue) | V9 incremental contract; timetravel edge keys → seam `ContentAddress.Of(span)` |
| E12 Mandate 1+2 | HOLD | count-prefix ZERO-structural; re-band gate passes (no 25xx crosses a persisted boundary); Fabrication rows decode 2701-2710 |
| E13 Folder+page overload | DRIFT (lane 520; retrieval :267-520; Sep 5,18,20,297) | V5 splits; V11 Ingest growth; four frozen-contract cross-leg edges bound |
| E14 Parameterization | DRIFT (wider — `{projection}` raw SQL; `:271` Secret raw) | V10 columnar trust gates (VO family) + posture constant |

---

## [07]-[FEDERATION_PROBE_DISPOSITION]

Probe verdict FEASIBLE_WITH_GAPS is DISPOSED as the V1 ruling (a draft assuming INFEASIBLE loses at judge):

- WIRE FORM overturned in the owner's favor: `api-flowtide-substrait.md:151` is FALSIFIED — `SubstraitDeserializer.Deserialize(Substrait.Protobuf.Plan)`/`Deserialize(string json)`/`DeserializeFromJson` are PUBLIC in-assembly; `Substrait.Protobuf.Plan.Parser.ParseFrom(bytes)` is public. Transcription ≈ 2 lines (shipped). The ONLY substantive new code is the lowering visitor (~150-250 LOC, reference `SubstraitToDifferentialComputeVisitor` 139 LOC/9 kinds). Transcription-heavier-than-lowering retire trigger runs the OPPOSITE way.
- G1 catalog `:151` FLIPS + re-anchors to `Query/federation`; `SubstraitSerializer` internal (retain wire bytes, never re-serialize).
- G2 `LoweringTarget` discriminant with the [03] routing table (SetExpr 3-for-3 on SetRelation→Union/Intersect/Difference; ProjectRelation/AggregateRelation/Sort/TopN/Fetch/Window/general-Join → columnar; ExchangeRelation drops; WriteRelation rejects fail-closed).
- G3 tabular execution: ADD `ColumnarExtension.Substrait` row (DuckDB `from_substrait(blob)`, verify vs pinned v1.5.x); `SourceKind` capability axis (Substrait-native vs SQL-only); scope committed federation to Substrait-capable sources, stage SQL-only warehouses via the existing `AdbcStatement.SqlQuery` path (no second engine).
- G4 retain wire bytes for the tabular lane + plan digest; `SubstraitDeserializer` only for SetExpr subtrees.
- G5 external-source consistency honesty: the `(plan-digest, cut, watermark)` triple content-addresses the plan + LOCAL coordinate; external read is snapshot-at-execution, recorded as such.
- Roster: DROP `Grpc.Tools` assumption; `Google.Protobuf@3.35.1` sole runtime dep. Three BLOCKED cards re-anchor honest (SetExpr subset zero-gap; tabular scoped; BLOCKED on `python:data` producer).

---

## [08]-[CAPABILITY_ESCALATION_DISPOSITION] ([03] delta table)

| Plane | Now→Target | Closing delta | Leg |
|---|---|---|---|
| Element (graph/codec/identity+authority) | 9→9.5 | authority extraction; EF commit+snake-case+generated converters; DDL/migration owner; band registry sited; V9/parametric/streaming-identity consumer contracts; raw-hash re-anchor; HashPolicy ruling | 1 |
| Version core (ledger/commits/timetravel) | 9→9.5 | near-linear MergeBase; dead vocab wired-or-dead; batched fold; AsOfKey=Checkpoint.Hash; CRDT_OP_SET pin; Scrub incremental | 2 |
| Version merge | 8.5→9.5 | Type-correlation key; MemberPath verified; STJ options; re-key migration case | 2 |
| Version governance (provenance/retention/recovery) | 9→9.5 | real RPO lag; WAL-throughput policy row; Principal-agent-class; StorageLane.Durable disposed | 2 |
| Query read lanes (lane/topology/columnar/cypher/cache) | 8.5→9.5 | retrieval extracted; CypherFault rename; columnar trust gates+posture; pgvector LINQ; watermark/receipt preserved | 3 |
| Query retrieval (split) | —→9.5 | fusion+PQ codebook+RetrievalFault as one ANN owner; Compute codebook seam declared | 3 |
| Query federation (new) | 0→9 | Substrait→SetExpr lowering; public-deserialize ingress; SourceKind; ADBC lane; plan-digest·cut·watermark receipt; python:data wire; honest cards | 3 |
| Ingest (tabular+schedule) | 6.5→9 | folder law satisfied; Sep charter explicit; MPXJ schedule codec + durable rows | 3 |
| Store (blobstore/provisioning) | 9→9.5 | receipt path composed; checksum honesty; loose codes typed; roster-vs-README bridge reconciled | 4 |
| Store coordination (new) | 0→9.5 | token-VALIDATING fenced-lease; Budget = fenced predicated compare-and-decrement; CAS/membership op-union; named outbox + cursor; all four PORT rows homed | 4 |
| Changefeed egress (new) | 0→9 | one pump over outbox cursor; exactly-once-effect envelope; sink rows (NATS dedup+Settle, pg_net response-reconciled) | 4 |
| Governance perimeter | 3→9.5 | zero phantom claims; ledger both directions; 77 catalogs live-anchored; zero orphan admissions; band registry type-enforced | 1 |

---

## [09]-[CARD_DISPOSITION] — full TASKLOG/IDEAS table (30 cards)

### IDEAS.md (11)

| Card | Status now | Disposition |
|---|---|---|
| `[PERSISTENCE_LIBRARY_TABLES]` | QUEUED | KEEP OPEN, honest — Materials content-key producer-gated; anchors resolve (blobstore content rows + identity content-hash) | 
| `[FABRICATION_PROGRAM_DURABLE_ROWS]` | QUEUED | RE-ANCHOR off dead `Schema/ddl#IDENTITY and Store` (:28,30) → blobstore content rows + cache artifact index + `[V2]`/`Store` growth axis; carry the KIND-AGNOSTIC `ArtifactKind`-row constraint (programs/`.cli`/3MF/NC1/travelers on ONE artifact index under `ContentHash.Of`) + the 2701-2710 decode constraint (never 25xx); Fabrication-wire-pins BLOCKED |
| `[REUSE_WIRE]` | BLOCKED | CORRECT phantom `IDEAS:38` "FederatedEntity/EntityGraph ... is realized" → honest: lowering target realized as `Query/federation` fences (SetExpr subset zero-gap); BLOCKED on `python:data [REUSE_WIRE]` producer |
| `[SUBSTRAIT_FEDERATION_SEAM]` | BLOCKED | CORRECT phantom `IDEAS:45` "ElementSet/SetExpr algebra + FederatedPlan is realized" → honest: `Query/federation` lowers Substrait onto SetExpr (V1); BLOCKED on `python:data [SUBSTRAIT_PORTABILITY]`/`[QUERY_IR_AND_SQLGATE]` producer |
| `[PERSISTENCE_BIM_SYNC_CRDT]` | COMPLETE | HOLD legit (`commits#CRDT_ALGEBRA` `CrdtField`/`Crdt.Apply` real :189,:212) |
| `[DURABILITY_RECOVERY_OBSERVATORY]` | COMPLETE | HOLD legit (`recovery.md` real) — but the "re-establishes CONTENT IDENTITY" claim is RAISED to assertable by the AsOfKey oracle (V12) |
| `[TRANSACTION_CONCURRENCY_CONTROL]` | COMPLETE (cites deleted `Query/transaction.md`) | RE-POINT → `graph#STORE_RAIL` fold (TASKLOG:37); prepared-tx 2PC RETIRE |
| `[ENVELOPE_ENCRYPTION_KMS]` | COMPLETE (cites deleted `Store/encryption.md`) | RE-POINT → `Element/identity#AUTHORITY` (KMS) + `Store/blobstore#BLOB_GC` (SSE) folds (TASKLOG:39) |
| `[DATA_QUALITY_INTEGRITY_FRAMEWORK]` | COMPLETE (cites deleted `Store/quality.md`) | RE-POINT → columnar/provisioning verification surfaces |
| `[BULK_ETL_INTERCHANGE_PIPELINE]` | COMPLETE (cites deleted `Query/pipeline.md`) | RE-POINT → `Query/columnar` |
| `[CDC_STREAMING_EGRESS]` | COMPLETE (cites deleted `Sync/egress.md`) | RE-POINT → `Version/egress.md` (V3 NEW owner) |

### TASKLOG.md (19)

| Card | Status now | Disposition |
|---|---|---|
| `[ARTIFACT_CONTENT_KEY_FEDERATION]` | BLOCKED | CORRECT phantom `TASKLOG:24` "`Query/federation#ENTITY_GRAPH SourceKind.SignedArtifact` is realized" → honest: `Version/provenance` owns the signed-artifact binding (ARCH:51) / re-anchor to `Query/federation` (V1); BLOCKED on `artifacts [PROVENANCE]` producer |
| `[PERSISTENCE_BIM_ARTIFACT_INDEX]` | COMPLETE | HOLD legit (`cache#ARTIFACT_BLOB_INDEX` real) |
| `[RECOVERY_PAGE_AUTHOR]` | COMPLETE | HOLD legit (`recovery.md` real) |
| `[RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT]` | COMPLETE | HOLD legit (RecoveryFault 829x real) |
| `[TRANSACTION_PAGE_AUTHOR]` | COMPLETE (cites deleted `Query/transaction.md`/`.cs`) | RE-POINT → `graph#STORE_RAIL` (TASKLOG:37 fold record); 2PC RETIRE |
| `[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]` | COMPLETE | HOLD legit (`ledger#MERGE_LAW ConflictResult` real :225) |
| `[ENCRYPTION_PAGE_AUTHOR]` | COMPLETE | HOLD legit (encryption decomposed into identity + blobstore) |
| `[SQLCIPHER_RESEARCH_PROMOTE]` | DROPPED | HOLD (embedded floor rejects cipher bundle — settled) |
| `[QUALITY_PAGE_AUTHOR]` | COMPLETE (cites deleted `Store/quality.md`/`.cs`) | RE-POINT → columnar/provisioning verification surfaces |
| `[PIPELINE_PAGE_AUTHOR]` | COMPLETE (cites deleted `Query/pipeline.md`/`.cs`) | RE-POINT → `Query/columnar` |
| `[EGRESS_PAGE_AUTHOR]` | COMPLETE (cites deleted `Sync/egress.md`/`.cs`) | RE-POINT → `Version/egress.md` (V3) |
| `[SCHEMADDL_SQL_COLLAPSE]` | COMPLETE (cites deleted `Schema/ddl#EXTENSION_DDL`, contradicts `:45`) | RE-POINT → V6 migration owner on `Element/identity`; the `:44`↔`:45` self-contradiction resolves (no `SchemaDdl.Sql`; extension DDL is `ServerExtension.CreateSql`) |
| `[STORE_SERVER_SPLIT]` | COMPLETE | HOLD legit (`Store/provisioning.md` real; tenancy folded into identity) |
| `[ANNOTATION_RELOCATE_TO_BIM]` | COMPLETE (cites deleted `Sync/annotation.md` + phantom `Query/federation#ENTITY_GRAPH`) | RE-POINT → `ledger#CHANGEFEED` (BIM:101 counterpart); drop phantom join key |
| `[SCHEDULE_RELOCATE_TO_BIM]` | COMPLETE (cites deleted `Sync/schedule.md`) | RE-POINT → `Ingest/schedule.md` (V11; BIM:102 counterpart) |
| `[ICECHUNK_ASOF_CONTENT_KEY]` | COMPLETE (cites deleted `Version/snapshots#SNAPSHOT_PROTOCOL AsOfKey`) | RE-POINT → `Version/timetravel` (`AsOfKey`=`Checkpoint.Hash`, V12 re-realizes on a live page) |
| `[KMS_PACKAGE_ADMISSION]` | COMPLETE | HOLD legit (KMS trio pinned) |
| `[ARROW_FLIGHT_PACKAGE_ADMISSION]` | COMPLETE | HOLD legit (rides V1 now, not the retired pipeline) |
| `[KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION]` | COMPLETE | HOLD legit; `.api` re-anchor Kafka/CloudEvents → `Version/egress` (V3) |

Summary: 3 phantom-realization corrections (IDEAS:38/45, TASKLOG:24) + 1 provenance re-anchor; 11 stale-`COMPLETE`/`QUEUED` re-points to real fold owners; 15 HOLD-legit; 1 DROPPED-hold. Every card claim resolves against disk post-DECISION.

---

## [10]-[LEG_PARTITION] — proven acyclic (within a leg, listed order IS dependency order)

1. **SPINE + PERIMETER** — band registry COMPLETE (all 20 own bands incl. new-owner bands — coordination/egress/federation/schedule/retrieval — and rebanded provisioning decades assigned in leg 1, so a duplicate integer is unrepresentable from the first leg); `Element/` (authority extraction V5a, EF commit + DDL/migration owner V6, codec consumer contracts V9 + kernel-entry hash re-anchor + HashPolicy V10); FULL roster reconciliation + catalog re-anchor/dedup/stub (V7, `[ROSTER_RECONCILIATION]`) — gates every later leg's imports; own-ledger seam corrections (V12 ledger half); the csproj strata reversal resolution. Pages: `graph`, `codec`, `identity`, `authority`.
2. **VERSION ENGINE** — `ledger`/`commits`/`timetravel`/`merge`/`provenance`/`retention`/`recovery` V10 repairs; the `AsOfKey` arm; the `CRDT_OP_SET` parity pin on commits; V8 Type-correlation + named migration case; V9 Scrub/Bisect incremental re-shape (interim documented); H1 Commit fault MINT; H2 version-engine hash re-anchor; H3 edge-key seam.
3. **QUERY + INGEST** — `Query/retrieval` extraction + RetrievalFault (V5b); `columnar` trust gates + posture + DuckDB substrait extension row; the V1 `Query/federation` owner; `cache`/`cypher` (CypherFault rename executes here)/`topology` (Lca gate); `lane` cold pass; `Ingest/` growth (tabular Sep charter + schedule V11).
4. **STORE + COORDINATION + EGRESS** — `blobstore` V10 repairs; `provisioning` loose-code typing onto the registry (ServerFault re-band); `Store/coordination` (V2); the `Version/egress` owner (V3) over the leg-2 changefeed AND the leg-4 outbox spine.

### [ACYCLICITY_PROOF]

Zero upward strata edges (Persistence depends UP on `Rasm` + `Rasm.Element` ONLY; the csproj:10 AppHost reversal DELETES — [00]). The four on-disk intra-package inversions resolve as FROZEN vocabulary contracts (V5), so an earlier-leg page consuming a later-leg vocabulary is acyclic BY CONTRACT:

| Inversion | Direction | Frozen vocabulary | Rule |
|---|---|---|---|
| `graph` ← `timetravel.TimeCut` | leg 1 ← leg 2 | `TimeCut` cases | leg 2 may not alter; change reopens leg 1 as hard residual |
| `recovery` ← `blobstore.ObjectStore.Head` | leg 2 ← leg 4 | `ObjectLeg.Head` delegate shape | leg 4 may not alter; change reopens leg 2 |
| `retention` ← `blobstore.StorageTier` | leg 2 ← leg 4 | `StorageTier` rows | leg 4 may not alter; change reopens leg 2 |
| `cypher` ← `provisioning.ServerExtension.CreateSql` | leg 3 ← leg 4 | `ServerExtension` row shape | leg 4 may not alter; change reopens leg 3 |

The one NEW edge (V3): `Version/egress → Store/coordination#OUTBOX` cursor read — BOTH leg 4, no reverse edge (coordination never reads the pump), so intra-leg acyclic. Within leg 4 the order is `blobstore` → `provisioning` → `coordination` → `egress` (egress drains coordination's cursor, so coordination lands first).

### [ACCEPTANCE_DRY_RUNS] (compose from rebuilt fences after leg 4)

- (a) VERSION-ENGINE: commit → three-way merge across a simulated Type re-key reading as RENAME → AS-OF scrub shaped for the incremental contract → attested provenance verify → retention sweep → verified restore whose terminal proof is a content-identity assertion (restored `OfGraph` == checkpoint `AsOfKey`, `RecoveryFault.VerifyFailed` unreachable) beside a real RPO lag.
- (b) COORDINATION: a fenced Budget debit whose stale-token replay is REJECTED (a `LeaseFenced` fault — the fence is validated not merely issued), a step-state CAS, an `InFlight` read serving CrashResume, and an outbox drain through the egress pump to a CloudEvents sink where a redelivered event dedups by `id`=`ContentKey`.
- (c) FEDERATION: a Substrait plan lowered to `SetExpr` executing on the columnar lane (probe-confirmed feasible).
- (d) PERIMETER AUDIT: every fault code resolves to exactly one registry row; every digest mint resolves through `ContentHash.Of`/seam `ContentAddress`; every catalog anchor + card anchor resolves to a live page/disk; `dotnet restore` clean after the manifest motion.

A dry-run reaching for a missing owner reopens V1/V2/V3 as residuals.
