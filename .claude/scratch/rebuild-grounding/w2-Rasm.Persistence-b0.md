# w2 · Rasm.Persistence · b0 — Version batch (ledger, commits, timetravel, merge)

VERIFIED PRIMARY EXTRACTS ONLY. Every claim carries a `file:line` anchor. Members marked `[assay]` confirmed via `uv run python -m tools.assay api`; `[catalog]` confirmed in a `.api` file; `[disk]` quoted from the target/sibling page.

## [00]-[INVENTORIES] — real `ls`

### doctrine root — `docs/stacks/csharp/`
```
README.md  language.md  shapes.md  surfaces-and-dispatch.md  rails-and-effects.md  boundaries.md  algorithms.md  system-apis.md  domain/
```
### doctrine domain — `docs/stacks/csharp/domain/`
```
README.md  compute.md concurrency.md data-interchange.md diagnostics.md durability.md interaction.md persistence.md postgres.md resilience.md runtime.md transport.md validation.md visuals.md
```
### shared substrate `.api` — `libs/csharp/.api/` (30 catalogs; Version-relevant)
```
api-languageext.md  api-thinktecture-runtime-extensions.md  api-thinktecture-messagepack.md  api-thinktecture-json.md
api-hashing.md  api-highperformance.md  api-mapperly.md  api-generator-equals.md  api-quikgraph.md
api-nodatime.md  api-nodatime-stj.md  api-protobuf.md  api-mathnet-numerics.md  api-csparse.md
```
### folder `.api` — `libs/csharp/Rasm.Persistence/.api/` (Version-relevant subset of 84 catalogs)
```
api-marten.md  api-messagepack.md  api-messagepack-analyzer.md  api-thinktecture-serialization.md
api-npgsql.md  api-npgsql-ef.md  api-nodatime.md  api-nodatime-stj.md  api-jsonpatch.md  api-hashing.md
api-cloudevents.md  api-nats.md  api-kafka.md  api-rabbitmq.md  api-dotpulsar.md  api-redis.md  api-pg-net.md
```
### Version folder — `libs/csharp/Rasm.Persistence/.planning/Version/` (8 pages)
```
ledger.md  commits.md  timetravel.md  merge.md  provenance.md  recovery.md  retention.md   (+ egress.md is NEW per DECISION, not on disk)
```

## [01]-[VERIFIED MEMBERS]

### System.IO.Hashing — `.artifacts/assay/scope/api/System.IO.Hashing/decompile.cs` [assay]
- `:125` `public static UInt128 HashToUInt128(ReadOnlySpan<byte> source, long seed = 0L)` — **seed defaults 0**, so kernel `ContentHash.Of(span)` (seed-zero) is value-identical to the raw call; the [B] re-anchor is pure call-path collapse, never an identity migration.
- `:160` `public override void Append(ReadOnlySpan<byte> source)`
- `:196` `public UInt128 GetCurrentHashAsUInt128()`

### kernel `ContentHash` — `libs/csharp/Rasm/.planning/Domain/identity.md` [disk]
- `:12` "Owner: `ContentHash` static class — one member, one algorithm, one seed."
- `:14` "THE federation content key, verbatim contract — every content hash in the federation composes this entry … the `Rasm.Persistence` snapshot spine and artifact index … One algorithm, one seed, no second hasher: a second hashing path anywhere in the federation forks identity and is the deleted form."
- `:92` `ContentHash` `static entry over seed-zero XxHash128` `ReadOnlySpan<byte> → UInt128`.

### seam `ContentAddress` re-anchor law — `libs/csharp/Rasm.Persistence/.planning/Element/codec.md` [disk]
- `:157` "the seam owns every minting entry — `ContentAddress.Of(ReadOnlySpan<byte>)` hashes the framing/chunk preimage, `ContentAddress.Of(UInt128)` wraps a precomputed … digest, `ContentAddress.Of(Node, tolerance)` … `ContentAddress.OfGraph(ElementGraph)` the order-independent snapshot identity".
- `:158` "a per-call-site `XxHash128.HashToUInt128` invocation is the deleted spelling (value-identical, so the re-anchor is pure call-path collapse, never an identity migration)".
- `:161` Growth: INCREMENTAL `OfGraph(prior, delta)` is `Rasm.Element/Projection/address`'s member; "until it lands the documented interim is the whole-graph `OfGraph` recompute". STREAMING identity = kernel `Append`+`GetCurrentHashAsUInt128` seed-zero, "Persistence IS that demanding consumer".
- `:166-170` policy table: `[01]` kernel `ContentHash.Of` (seed-zero), "a direct `XxHash128` call site is deleted"; `[04]` `OfGraph(prior, delta)` seam contract, "whole-graph recompute is the documented interim".

### Marten — `libs/csharp/Rasm.Persistence/.api/api-marten.md` [catalog] + `.artifacts/assay/scope/api/Marten/decompile.cs` [assay]
- decompile `:13` `public abstract class SubscriptionBase : JasperFxSubscriptionBase<IDocumentOperations, IQuerySession, ISubscription>, ISubscription`
- decompile `:28` `public abstract Task<IChangeListener> ProcessEventsAsync(EventRange page, ISubscriptionController controller, IDocumentOperations operations, CancellationToken …)`
- catalog `:118` `JasperFx.Events.Projections.EventRange` — "the delivered batch: `List<IEvent> Events`, `ShardName`, `SequenceFloor`/`SequenceCeiling`".
- catalog `:120` `IChangeListener` / `NullChangeListener`.
- catalog `:172` `Events.AggregateStreamAsync<T>(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, T? state = null, long fromVersion = 0, …)` — AS-OF fold by `version:` or `timestamp:`.
- catalog `:173` `Events.AggregateStreamAsync<T>(string streamKey, …)` / `AggregateStreamToLastKnownAsync<T>(…)` — "fold ignoring an aborted tail".
- catalog `:204` `store.BuildProjectionDaemonAsync(…)`; `:205` `daemon.StartAllAsync()` / `RebuildProjectionAsync<TView>(…)` / `WaitForNonStaleData(TimeSpan)`; `:206` `store.Advanced.RebuildSingleStreamAsync<T>(Guid id, …)`.
- catalog `:109/:150` `FetchEventStoreStatistics` → `EventStoreStatistics` (`EventSequenceNumber`).

### MessagePack — `.artifacts/assay/scope/api/MessagePack/decompile.cs` [assay] + `api-messagepack.md` [catalog]
- decompile `:397` `public static readonly MessagePackSecurity UntrustedData = new MessagePackSecurity{…}` with `:401` `MaximumDecompressedSize = 67108864`.
- decompile `:463` `public MessagePackSecurity WithMaximumObjectGraphDepth(int maximumObjectGraphDepth)`; `:475` `WithMaximumDecompressedSize` builder present.
- catalog `:47` `MessagePackCompression` cases `None`, `Lz4Block`, `Lz4BlockArray`.
- catalog `:66` `CompositeResolver.Create` overloads.
- confirms commits.md `:320-323` `CompositeResolver.Create(PersistenceResolver.Instance, StandardResolver.Instance)` + `.WithCompression(Lz4BlockArray)` + `.WithSecurity(UntrustedData.WithMaximumObjectGraphDepth(64).WithMaximumDecompressedSize(1<<20))` uses REAL members.

### Thinktecture MessagePack bridge — `api-thinktecture-serialization.md` [catalog]
- `:50` `ThinktectureMessageFormatterResolver : IFormatterResolver`; `Instance`, `GetFormatter`.
- `:124` "Accept: `ThinktectureJsonConverterFactory` on `JsonSerializerOptions`, `ThinktectureMessageFormatterResolver.Instance` in the `CompositeResolver`" — the generated-owner formatter for the `[SmartEnum]`/`[Union]`/`[ComplexValueObject]` members (`Hlc`/`VersionVector`) riding `CrdtOpWire`.
- NOTE: commits.md `:321` cites `PersistenceResolver.Instance` (a Persistence-local composite) — the `PersistenceResolver` owner is NOT defined in `commits.md`; it must compose `ThinktectureMessageFormatterResolver.Instance` for the wire `[SmartEnum]`/`[ComplexValueObject]` members to formatter-resolve. Verify PersistenceResolver's definition site in the folder on landing.

### LanguageExt — `libs/csharp/.api/api-languageext.md` [catalog]
- `:68` `Fold<S>(S, Func<S, A, S>)` instance; `:69` `Traverse<F, B>` / `TraverseM<M, B>`; `:117` `Bind`/`Filter`/`Fold`/`Distinct`/`Traverse` on `Seq`. All four target pages compose `Fold`/`Bind`/`Map`/`Choose`/`GroupBy`/`toSeq`/`toHashMap` — confirmed member family.

## [02]-[DECISION SCOPE ANCHORS] — `RASM-CS-PERSISTENCE-DECISION.md` [disk]

### Version page rows (`[02]-[PAGE_SET]`, all `KEEP · improve`, leg 2 except egress)
- `:93` **row 5 `Version/ledger.md`** — "`[V10]` `SyncOpKind.Truncate`+`WholeRelation` wire-or-delete; `OpLogEntry.Codec`→`Family`-derived accessor; `ProcessEventsAsync` batches the range into ONE fold (`:119`); `OpLogEntry.ContentKey` re-anchors `ContentHash.Of` ([B])." Seams ADD: `← Rasm.AppHost/Wire/companion` PRESENCE PeerRoster over DrainSurface; `← Rasm.AppUi/Collab/Editing` edit-intent projection + per-doc replay-window READ AND `← Rasm.AppHost/Runtime/determinism` neutral-log window-read — "ONE windowed-read case parameterized by origin/entity/window serves BOTH"; `→ typescript:core/interchange/codec`.
- `:94` **row 6 `Version/commits.md`** — "**`[V4]/H1` MINT `CommitFault`/`CrdtWireFault : Expected`** — disk `:358`/`:400`/`:411` bare `Error.New(8261/8264/8263)`, ZERO union today; NOT 'register as-is'. `[V10]` near-linear `MergeBase` (one reverse-reachability generation-mark pass + dominance filter, killing the per-candidate `Rank` re-run `:108-113`; BIM:94 the named consumer). [B] raw-hash re-anchor (`CommitNode` `:91`, `CrdtWire.ContentKey` `:354`, parity corpus `:375-376,401`). PIN `CRDT_OP_SET` … `[V10]` dead `VectorOrder`/`Order` + `ContentParityCorpus.Seed` wire-or-fold."
- `:95` **row 7 `Version/timetravel.md`** — "**`[V12]/S3` `AsOfKey`=`Checkpoint.Address` arm** (the seam `ContentAddress.OfGraph` digest; the field exists `:84`; ONE content digest serving icechunk + recovery oracle, while the prior-linked `Hash` chain (`:154-158`) stays tamper evidence per [B]; `recovery.md:65` `RecoveryPoint.AsCut()` resolves to it). [B] edge keys → `ContentAddress.Of(span)` (H3, `:203-204`). `[V9]` Scrub/Bisect re-shape to incremental `OfGraph(prior,delta)` (interim whole-graph recompute documented). `[V10]` `AggregateStreamToLastKnownAsync` drop-or-compose."
- `:96` **row 8 `Version/merge.md`** — "`[V8]b` Type-correlation key row (classification-independent Type natural key mirroring `ExternalKey`, so a re-keyed Type diffs as RENAME; COUPLED to V8a — reads the kernel seed on landing OR ships its own classification-excluded seed interim). `[V10]` thread `ElementJson.Options` STJ through the `JsonPatchDocument` insert arm (`:390`). MemberPath NodeId typing carried VERIFIED-CLEAN with a leg-2 re-verify obligation."

### `[01]-[SHARED_LAW]` anchors
- `:20` **[A.1] strata / injection** — `ClockPolicy`/`CorrelationId`/`TenantContext` "referenced by simple name across ~18 pages … `ledger.md:284`/`timetravel.md:134`/`recovery.md:134`/`retention`/`merge`/`commits`/`provenance` leg 2"; AppHost `Runtime/time.md:18` declares `ClockPolicy` NEVER crosses the strata DAG downward. RULING: the Persistence-owned port-input shapes (`StoreActor` + `ClockPolicy`/`CorrelationId`/`TenantContext`/`RecoveryObjective` carried on injected `ProjectionContext`/`ResolvedProfile`) DEFINED leg 1, EVERY referencing page re-threads IN ITS OWN LEG.
- `:58-68` **[B] HASHER_RE_ANCHOR table** — decision-complete raw-mint → re-anchor rows, all leg 2:
  - `:63` `OpLogEntry.ContentKey` | `ledger.md:153` raw `XxHash128.HashToUInt128(payload.Span)` → `ContentHash.Of(payload.Span)`.
  - `:64` `CommitNode` content key | `commits.md:91` → `ContentHash.Of(canonical.WrittenSpan)`.
  - `:65` `CrdtWire.ContentKey` | `commits.md:354` → `ContentHash.Of(EncodeCompanion(op).Span)`.
  - `:66` `ParityVector.Pin/Stamped/Contribute` | `commits.md:375-376,401` raw → route through `ContentHash.Of`; "fold the dead `ContentParityCorpus.Seed=0L` into the mint or delete".
  - `:68` timetravel EDGE keys (H3) | `timetravel.md:203-204` raw → `ContentAddress.Of(edge.ToCanonicalBytes(tolerance).Span)` — "the SEAM form, matching `merge.md:160`".
- `:70` DEFENSIBLE-LOCAL (NOT re-anchored): `provenance` internal Merkle rolling digests, `commits.md:119` `MerkleRange` peer digest, `timetravel` checkpoint `ChainHash` prior-link (`:154-158`, tamper evidence over the seam `Address`, never a content key).
- `:74` **[C] RECEIPT_VALIDITY_FLOOR** — a Persistence receipt carrying validity evidence implements kernel `IValidityEvidence` and spells `IsValid` as one `ValidityClaim.All` fold over `Of(...)` arms; "The kernel `NamingHash` the merge consumer reads already registers through this floor."

### `[02]-[SEAM_BACKBONE]` (A.2) — cross-runtime seams touching these pages
- `:28` **S3** `AsOfKey`=`Checkpoint.Address` — owner `Version/timetravel.md`: ONE content digest (seam `ContentAddress.OfGraph` at the cut) serving icechunk seam AND recovery oracle; prior-linked `Checkpoint.Hash` chain (`timetravel.md:154-158`) stays tamper evidence.
- `:29` **S4** `CrdtOpWire` bit-parity — owner `Version/commits.md`: `[MessagePack.Union]` `[Key]` sequence IS the wire; MINT `CommitFault`/`CrdtWireFault` (H1); PIN `CRDT_OP_SET` byte-identical into kernel golden corpus `Rasm/.planning/Spatial/reconciliation.md#[03]-[ONE_WIRE_FIXTURE_CORPUS]` row [04].

### `[03]-[BAND_REGISTRY]` — Version decade rows
- `:140` `8250 SyncFault Version/ledger 8251-8256` — "register as-is (the one correct typed band on disk)".
- `:141` `8260 CommitFault/CrdtWireFault Version/commits 8261,8263,8264` — "**MINT typed `[Union]:Expected`** (H1 — disk: bare `Error.New`, ZERO union; NOT 'register as-is')".
- `:142` `8270 EgressFault Version/egress` NEW (827x gap between Commit and Retention).
- `:162` No-band pages: "`Version/timetravel`, `Version/merge`" (total algebras / verdict cases).

### `[04]-[SEAM_LEDGER]` — Version rows
- `:174` ARCH:50 icechunk AsOfKey (S3) — KEEP target LIVE `Version/timetravel`; capability realized `AsOfKey`=`Checkpoint.Address`.
- `:177` ARCH:55 reconciliation GeometryHash — RETARGET `Query/topology` → `Version/merge#STRUCTURAL_DIFF` (merge's `GraphNode.GeometryHash` the consumer); "the per-node content axis carries `(EncodeForm, GeometryHash)` PAIRS — a bare digest never crosses a form boundary — and the Persistence `CanonicalWriter` reads the kernel-frozen `EncodeForm` layouts (IEEE-754-LE, `-0.0`→`+0.0`)".
- `:187` `Version/ledger#CHANGEFEED ← Rasm.AppUi/Collab/Editing` edit-intent projection + per-doc replay-window READ AND `← Rasm.AppHost/Runtime/determinism` neutral-log window-read — ONE windowed-read case; "text-container contingency rides the EXISTING `CrdtOpWire` `RgaSequence`, zero new wire row".
- `:191` `Version/egress → Store/coordination#OUTBOX_CURSOR` (intra-leg cursor edge, forward-only — S2); `[05]:128` `ONE_OUTBOX_EGRESS_SPINE` "the Marten event stream IS the outbox … mints the durable drain cursor PER-SINK — `outbox_cursor(SinkKey, long Sequence)`", the egress pump drains the `Version/ledger#CHANGEFEED` `OpLogEntry` rows.

## [03]-[TARGET-PAGE DISK ANCHORS] — the exact fences the improve motions touch

### `ledger.md` [disk]
- `:108-112` `OpLogEntry(… SnapshotCodec Codec, ReadOnlyMemory<byte> Payload, UInt128 ContentKey, …)` — `Codec` stored field; `Stamp => new(Physical, Logical)`.
- `:119-124` `ProcessEventsAsync` body: `foreach (var e in range.Events.OfType<IEvent<GraphEvent>>()) await sink(OpLog.Project(e)).RunAsync(…)` — **per-event await, NOT a batched range fold** ([V10] `:93` target).
- `:148-158` `Project` — `:153` `XxHash128.HashToUInt128(payload.Span)` (re-anchor target [B] `:63`).
- `:29-36` `SyncOpKind` `Upsert|Delete|Truncate|Presence`; `:31` `Truncate = new("truncate", tombstone: true, wholeRelation: true)` — `WholeRelation` carried but no consumer in `Project`/`SyncMerge` reads it ([V10] wire-or-delete).
- `:242-276` `SyncFault : Expected` typed `[Union]` band 8250 — the CORRECT template commits.md lacks.
- `:284-290` `SyncSession(ClockPolicy Clocks, ReceiptSinkPort Sink, CorrelationId Correlation, …)` — raw ClockPolicy/CorrelationId ([A.1] re-thread to injected frame).

### `commits.md` [disk]
- `:38-44` `VectorOrder` smart-enum; `:100-106` `CommitGraph.Order(left,right)` → Before/After/Concurrent/Equal — computed but `MergeBase` uses `Rank`, not `Order`; dead unless a consumer reads it ([V10] wire-or-fold).
- `:86-92` `Commit(...)` → `:91` `XxHash128.HashToUInt128(canonical.WrittenSpan)` (re-anchor [B] `:64`).
- `:108-114` `MergeBase` — `:112` `common.Fold(Set<UInt128>(), (acc, candidate) => acc.Union(toSet(Rank(resolve, candidate).Keys)…))` — **`Rank` re-walked PER candidate** (O(candidates × graph)); `:132-141` `Rank` is a BFS longest-path per root ([V10] near-linear one-pass target `:94`).
- `:319-358` `CrdtWire` — `:356-358` `Decode` returns `.MapFail(static error => Error.New(8261, …))` — **bare `Error.New`, no typed union** (H1 MINT target).
- `:354` `ContentKey(CrdtOp) => XxHash128.HashToUInt128(EncodeCompanion(op).Span)` (re-anchor [B] `:65`).
- `:374-378` `ParityVector` — `:375` `Pin() … XxHash128.HashToUInt128(Canonical.Span)`; `:376` `Stamped() … XxHash128.HashToUInt128(Canonical.Span)` (re-anchor [B] `:66`).
- `:380-411` `ContentParityCorpus` — `:381` `public const long Seed = 0L` (DEAD — never read); `:398-401` `Contribute` → `Error.New(8264, …)` (bare); `:408-411` `Reconcile` → `Error.New(8263, …)` (bare).
- `:296-317` `CrdtOpWire` `[MessagePack.Union(0..9, …)]` `[Key]` sequence — the S4 wire; `:374` `ParityVector.Digest` `Option<UInt128>` design-pin gap (PIN `CRDT_OP_SET` → REAL).
- `:119` `MerkleRange.Of` `XxHash128.HashToUInt128` — DEFENSIBLE-LOCAL peer digest, NOT re-anchored ([B] `:70`).

### `timetravel.md` [disk]
- `:16` Packages line cites `AggregateStreamToLastKnownAsync` — never composed in any op ([V10] drop-or-compose).
- `:84` `Checkpoint(Hlc At, long Version, ContentAddress Address, UInt128 Hash, Option<UInt128> Prior)` — `Address` = the `AsOfKey` seam digest (S3); `Hash` = the rolling chain.
- `:142-161` `Anchor`/`Seal`/`Verify`/`ChainHash` — `:154-158` `ChainHash` rolling XxHash128 over prior+Address — DEFENSIBLE-LOCAL tamper chain, NOT re-anchored ([B] `:70`).
- `:202-211` `EdgeDeltas` — `:203` `toHashMap(a.Edges.Select(e => (XxHash128.HashToUInt128(e.ToCanonicalBytes(a.Header.Tolerance).Span), e)))`; `:204` same for `b.Edges` — **raw hasher on edge keys** (re-anchor [B] `:68` → `ContentAddress.Of(...Span)` matching `merge.md:160`).
- `:227-238` `Scrub` — `:236` `ContentAddress.OfGraph(acc.Graph).Value, ContentAddress.OfGraph(next).Value` — **whole-graph recompute per frame** ([V9] incremental `OfGraph(prior,delta)` target; interim documented).
- `:240-261` `Bisect`/`Descend` — `:258-259` re-`ReconstructAt` + `OfGraph` per probe (same [V9] whole-graph recompute pressure).
- `:60-69` `TimeCut` `[ComplexValueObject]` — FROZEN vocabulary (V5d), `graph.md:229-232/:302` reads it; the case-set must not change.
- `:129-134` `TimeLog(… CorrelationId Correlation, ClockPolicy Clocks)` — raw ([A.1] re-thread).

### `merge.md` [disk]
- `:272-278` `Reconcile(persisted, ingested)` correlates ONLY via `:283-284` `ExternalKey` → `Node.Object { ExternalId }` (IFC GlobalId); a `Type` definition object carries no ExternalId → re-keyed Type reads delete+insert ([V8b] Type natural-key row target).
- `:305-315` `GeometryDigest(RepresentationContentHash)` folds raw `ByIdentifier` `UInt128` digests via `XxHash128.Append` — the reconciliation consumer (ARCH:55); content axis must carry `(EncodeForm, GeometryHash)` PAIRS per the kernel reconciliation bridge, not a bare digest.
- `:386-397` `Append` — `:390` insert arm `s.Doc.Add($"/{i.Node.Key.Value}", s.Target.Find(i.Node.Key).Match(Some: n => (object)n, …))` — the `JsonPatchDocument` serializes a real `Node` WITHOUT the codec `ElementJson.Options` STJ converter graph ([V10] thread `ElementJson.Options`).
- `:236-252` `MemberDiff`/`OwningNode` — `:249` `segments[i].Value is NodeId key` MemberPath pattern (VERIFIED-CLEAN, leg-2 re-verify obligation per `:96`).
- `:160` `ContentAddress.Of(o.ToCanonicalBytes(graph.Header.Tolerance).Span).Value` — the SEAM node-content key form the timetravel edge re-anchor ([B] `:68`) must match.
- Band NONE — `MergeConflict` is a conflict class (`:162` no-band list).

## [04]-[SIBLING/FOLDER CONTEXT] — full-file reads, anchors

- `provenance.md:170-200` `CausalDag.Derive` reads `changefeed: Seq<OpLogEntry>` + resolve `CommitNode` (consumes ledger `OpLogEntry` + commits `CommitNode`); `:291` `AgentKey` raw `XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor))` (re-anchor [B] `:67`, leg 2); `:229,:321` Merkle rolling digests DEFENSIBLE-LOCAL.
- `provenance.md:21` "the attribution reconciles with `Version/timetravel#TIME_TRAVEL` `BlameRow` (the same `(Hlc, origin)` winner)" — timetravel `Blame` (`:213-225` `OriginOf`/`ActorOf`) and provenance agree on the header slots.
- `recovery.md:65` `RecoveryPoint.AsCut()` → `TimeCut.AtVersion(v, new Hlc(At,0))` / `TimeCut.Of(At)` — consumes timetravel `TimeCut` (S3 AsOfKey oracle bridge); `:212/:326-348` `ReAttest` folds `AggregateStreamAsync(version:)` + compares `ContentAddress.OfGraph` to `TargetAddress` — the recovery consumer of the timetravel `AsOfKey` content-identity digest.
- `retention.md:174,241-242` `RetentionSweep.Mark(referencedAt, everyCut: Seq<TimeCut>)` consumes timetravel `TimeCut` (frozen vocab V5c); `:100,:121-124` reads blobstore `StorageTier` (frozen vocab V5c).
- `merge.md:15/:405-408` `MergeConflict.Project → ConflictReceipt` reuses ledger `Version/ledger#MERGE_LAW` `ConflictReceipt` (`ledger.md:207`) — merge consumes ledger's conflict vocabulary, never re-mints.
- `commits.md:3` mints `ContentParityCorpus` = Persistence leg of `ONE_WIRE_FIXTURE_CORPUS`; the `elementset` parity slot (`commits.md:368`) flows in one-directionally from `Query/lane#ELEMENT_SET_ALGEBRA` via `Contribute`.
- README `libs/csharp/Rasm.Persistence/README.md` + `ARCHITECTURE.md` present (193 / 120 LOC); band/seam authority is the DECISION, not README.

## [05]-[CROSS-CUTTING GAPS ROUTED TO THIS BATCH]
1. **[B] hasher re-anchor** — ledger(`:153`), commits(`:91/:354/:375-376/:401`), timetravel(`:203-204`, H3 seam form). Value-identical collapse; `XxHash128.HashToUInt128` seed=0 verified `[assay]` == `ContentHash.Of`.
2. **[A.1] frame re-thread** — ledger `SyncSession:284`, commits (Stamp/ClockPolicy usage), timetravel `TimeLog:129-134`, merge (`Project` CorrelationId) all carry raw `ClockPolicy`/`CorrelationId`; re-thread onto the injected `ProjectionContext`/frame each leg.
3. **egress downstream (NEW page)** — `Version/egress.md` (S2) drains the ledger `OpLogEntry` changefeed via `Store/coordination#OUTBOX_CURSOR`; ledger's changefeed is the durable row source. Route the outbox-drain seam awareness to ledger.
4. **[C] receipt-validity floor** — merge consumes kernel `NamingHash` which registers through the floor; the Version receipts (`SyncApplyReceipt`, `TimeTravelReceipt`, `MergeOutcome`) carry validity evidence — density obligation, not an acyclicity gate.
