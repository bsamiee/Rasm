# [PERSISTENCE_RULINGS]

`Rasm.Persistence` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `age` admits `ExtensionAdmission.Standalone` — `LOAD` is a per-connection concern, so `Preload` faults correct deployments on `MissingPreload`.
- `OpenTelemetry.Instrumentation.ConfluentKafka` links each record's context on consume — a `CdcIngress` fold mints a second edge for one cause.
- `Npgsql.NodaTime` admits on the DATA SOURCE beside its spatial sibling — the EF plugin places no codec on a raw connection.
- `AMQPNetLite.Core` admits as a DIRECT dependency — `Version/egress` composes its links, so arriving transitively leaves that fence unpinned.
- `Apache.Arrow.Flight.AspNetCore` moves in lockstep with `Apache.Arrow.Flight` — it alone holds the `InternalsVisibleTo` grant reaching the adapter.
- `RocksDbException` exposes no stable typed status — preserve its exact exceptional `Error`; message text cannot govern recovery.
- `Azure.Storage.Blobs.Batch` tracks its OWN version line — it lags the blobs line by design, so pinning them equal resolves nothing.
- `MPXJ.Net` compiles its Maven closure per artifact scope and configuration; packed-POM warnings stay carved from cold-lane promotion.

## [02]-[SHAPE]

- `SetKey` is the ONE model-qualified membership axis — `(ModelId, NodeId)` under one byte order, so a bare-`NodeId` axis re-mints the ambiguity.
- `SetScope` arrives as caller DATA — the evaluator reads no rollup, so `ProjectGraph` supplies a scope and read-your-writes holds per model.
- Federation crosses ONLY `ModelLink` rows — in-model `Relationship` never widens, so a scoped selection and the `Federate` union never substitute.
- Rank vocabulary declares the expression and DERIVES its order — `Rank` is `"{Score} DESC"`, so no arm orders by an expression it never projects.
- `ArtifactKind` DERIVES retention from provenance — `Texture` answers rebuildable `Cache` or acquired `Blob`, so no caller sets retention.
- `LandingArm` is the DATASET shape, never the producing package — a shared arm splits its hive tree on a segment neither scan can prune on.
- `ProjectionContext` is this package's one time-and-causal frame — a `ClockPolicy` or `Principal` parameter inverts the strata the frame fixes.
- `Crdt.Apply` and `GraphDelta.Apply` are the only materializers — projection, merge, and AS-OF fold one delta, so a second forks replay.
- Member patches are binary — `FieldMask` diff over `NodeWire`, `IsValid` gate, `Merge` apply; ProtoJSON renders `PatchOp` leaves, never a target.
- `EntityEdit` lowers onto `Element.EntityEditWire` through `EditWire` — key and base cross as 16-byte addresses, one `PatchOp` per mask path.
- `SnapshotHeader` is the native 88-byte artifact trust frame — fixed offsets, pre-parser CRC, and digests through `ContentHash.Wire`/`Admit`.
- Backend literals render through the declared `ColumnType` — a stringified operand kills pruning, coerces on PostgreSQL, raises on ClickHouse.
- Read SCOPE parts from SHAPE at the backend entry — tenant rides the frame and window `BackendWindow`, so no plan expresses a cross-tenant scan.
- Rollups materialize SUMMARY STATE and readers name the accessor — a stored `avg` over an irregular series ships a mean the tile never claimed.
- Columnstore segment lists carry BOUNDED keys alone — segmenting a `KeyHex` content key mints one batch per row and deletes the compression itself.
- Backend relations resolve through one lower-cased `AnalyticsSchema.Table` DDL and read both quote — PostgreSQL case-folds, ClickHouse does not.
- `ColumnShape` GENERATES containers over `ColumnType` — `List`, `Map`, and `Dictionary` are cases, so a flat `map-string-string` row forks it.
- `ColumnType` and `ColumnShape` stay BRANCH-LOCAL — no contract manifest names one, so a peer planting a relation reaches `BackendDdl`.
- No metrics store enters the `Backend` roster — a TSDB's per-series cardinality ceiling is the one column this family refuses to grow.
- Backend health measures OUTCOME — `BackendRead.Health` compares resident extent to horizon, so one engine's catalog answers for one tier.
- Every `BackendFault` pairs its backend with one neutral `EngineFault` — a per-backend case carries a column only one engine fills.
- Arity proves against `BackendLanding.Supplied`, never `Payload` — the custodian's tenant and landing instant are cells a producer cannot send.
- `ArrowLanding.Build` derives batches from the DECLARED schema, reading `ColumnType`'s own Arrow type and builder — a positional list forks it.
- Producer admission at a backend boundary ACCUMULATES every offending column — a producer refused on the first never sees the second.
- Compute's `AssessmentRow` crosses IN as producer-handed row data — a Persistence-declared result-row twin re-mints vocabulary its owner closes.
- CRDT payload equality rides `CrdtBytes` on `ReadOnlyMemory<byte>` — an `ImmutableArray<byte>` swap re-types the `CrdtWire.Decode` zero-copy boundary.
- `Crdt.Apply` takes the entry `OperationId` beside the generated op — MV-register context never substitutes for the outer dot.
- OR tombstones key by element; RGA routes retain predecessor and value identity without retired bytes.
- Presence retains stamped live/left cells and the monotone liveness horizon; family mismatch or unseated maintenance refuses.
- `TimeSpine` rides the DECLARATION — `Admit` faults a category its columns contradict, since one inferred from `time` re-dates event facts.
- `KvSpace` is the ONE keyspace axis both KV engines realize — a composite-key prefix carries none of the postures a row does.
- KV rows supply no comparer — key order is byte order, and a declared `CompareWith` voids every prefix stop with no compile error.
- `EmbeddedFault.Reoffer` totals every `RetryShape` route — `BUSY_SNAPSHOT` RESTARTS, so a wait against it spins on the caller's own read.
- Every store band DERIVES the kernel `Retriability` from its `Route` — a posture column beside the route publishes one answer twice.
- `EmbeddedFault.Busy` DERIVES its route from the extended status it keeps — a retry column copies a value the case already reads.
- `KvSeal.Ordered` names a value that is itself a POSITION — sealing a dup value voids `GetBoth`, `Unlink`, and `DuplicatesSort` together.
- `KvVault` nonces are RANDOM per value — a key-derived nonce repeats across every member `Append` accrues under one key.
- LMDB sync is ENVIRONMENT-scoped — the environment takes the roster's strictest `KvDurability`, so a per-space posture is inexpressible.
- `SpoolAccrual` length-frames every merge operand — a separator join shifts two member splits onto one boundary.
- `Lane` is the ONE lane-token owner — `StoreProfile.Lanes` and `ServerExtension.Lane` compose it, so a bare token forks one vocabulary.
- Every `Lane` row admits ONCE at its owning entry — `Admit` and `Register` read the ROW's own `Lane`, so a new row gates with no new call site.
- `StoreProfile` and `BackendProvider` are DISJOINT — one names an engine a generation is minted FOR, the other an engine this package OPENS.
- `AutoCreate.None` is the STORE posture — the runtime asserts configuration and materializes nothing, so every DDL byte lands from the deploy plane.
- Read models REBUILD from the event log through the projection daemon — replay is the whole upgrade path, so no projection preserves state.
- `FlightServer` is NOT a gRPC service — no bind attribute rides its hierarchy, so `MapGrpcService<T>` fails and `AddFlightServer<T>` binds.
- Flight refusals raise through AppHost `FaultWire.Raise` — one producer table packs `FaultDetail`; a page-local `StatusCode` switch is the twin.
- `WireLimits.Plan` is the ONE foreign-plan ceiling — both Substrait doors read it, so a size or depth past the row refuses before allocation.
- Substrait-JSON parses through the page-declared `PlanJson` alone — a FOREIGN descriptor outside `WireAdmission.Registry`, unknown fields tolerated.
- Rebalance resolves derive their timeout from the pinned poll interval — a dead leader burns it whole, so chunking multiplies the bound it guards.
- Rebalance handlers contain their own refusals — `Consume` re-raises whatever one throws, so a resolve fault arrives as a consume fault instead.
- `EnablePartitionEof` stays ARMED and counts on its own column — that edge is the lane's one idle signal, and suppressing it claims unmeasured lag.
- Batched egress bodies admit only a per-envelope contract — one response settling N envelopes fills `Duplicates` with an unmeasured number.
- Transports publishing no producer-side flow bound take the fence's OWN window — `SenderLink` takes peer credit and queues past it unbounded.
- `Watch` cells admit a polled probe where the driver publishes no event — `ClickHouse.Driver` raises `StateChange` nowhere, so state says nothing.
- `Coordinate.Run` takes the op SEQUENCE — acquisition orders on `LockRank`, execution on the caller, so a single-op sibling re-opens the deadlock.
- Advisory keys acquire in `LockRank` depth order and the rank IS the key prefix — a scope minted outside the ladder carries no order to take.
- Budget grain is the ledger ROW under the engine's own `WHERE` re-check — a snapshot-computed guard names a balance the same statement overdrew.
- Vector all-or-nothing takes the batch `SAVEPOINT`, never the caller's transaction — a refusal undoes the unit and leaves the caller's work its own.
- `CacheProfile` closes the execution-profile roster — an unrostered name throws at FIRST execute rather than falling back to the cluster default.
- Consistency evidence publishes the bound profile row's DECLARED level — `AppliedInfo` and `IPage` discard the `RowSet` the achieved level rode.
- `StoreProfile.Admits` gates the cache lane — Marten backs both backends, so a single-process store realizes neither.
- `CacheFault.Foreign` retains the provider `Error` — mapping it to `Unavailable` fabricates a consistency level and replica counts.
- Dead-letter rows carry generated `FaultObservation`; `EgressWire` alone projects generated host dead-letter and replay messages.
- Model identity is the PROFILE row — `UseModel` bypasses the model cache, so a cache-key factory forecloses the compiled model for nothing.
- `IdentityShapeRow` keys on `StoreProfile`'s own vocabulary — a shared key joins two axes, and a missing row fails the generated lookup loud.
- Compiled-model trust is a MEASURED digest of model metadata — a hand-written version column diverges the first time the model moves.
- `StoreBinding` names the PROVIDER row alone — the composed acquisition value is `IdentityLease`, so one concept never wears two spellings.
- Read predicates are `IdentityFilter` SHAPES, never public statics — an `IQueryable` over a pooled context enumerates reclaimed state.
- Page ordering tuple and its covering index are ONE declaration — a tuple with no index prefix seeks by scan at every page depth.
- Distance ranking rides the unpaged stream — a `<->` KNN order has no total index prefix a keyset cursor resumes from.
- Content keys cover PLAINTEXT — a codec or seal frames STORED bytes, so keying either forks one address per row.
- Stored form travels as object METADATA — the writer declares it and `Head` observes it, so no read names its own codec.
- Transport and identity digests are TWO columns — `ProvesIdentity` names the one form where they coincide.
- `Transition` rewrites a storage-class HEADER and IS the whole ladder — a payload re-PUT moves bytes nothing changed.
- Cold-rung state is a closed `ThawState` — `Option` cannot separate readable-now from must-ask from already-asked.
- `ObjectVerb` rides every `Bound` crossing — `Transport` names the op a re-drive repeats, `ColdRefuses` reads one code per verb, no per-leg catch.
- `Retriability` names WHETHER a re-offer is admitted, a band overriding it, and `RetryShape` WHERE — a bool spanning both drops two routes.
- `StoreVerdict` is the currency the re-drive boundary crosses on — naming a pipeline type inverts the strata `StoreRedrivePort` holds.
- `ReconcileAxis` owns every manifest axis token beside `RestartClass` — a bare literal forks one axis into two a deploy plane then diffs as two.
- `OutboxCursor` owns one optional `OutboxDeferred` per sink; the committed op-log never stores delivery state.
- First-terminal handling is one atomic `QuarantineAndAdvance`; separate letter and cursor writes are forbidden.
- Cursor sequence and CloudEvents `D20` ordinal are store-local drain positions, never HLC or portable order.
- `LeaseGuard` is advisory DETECTION — read as a gate it re-mints the frozen-guard scar each guarded write's own `fence <= @token` predicate closes.
- Backend floors spell `Fits`/`Admit`/`Tenancy`/`Lifetime`/`Degrade` — `Ingest` names one entry kind and `Retain` asks no owner to end a row.
- `Lifetime` states the extent AND the owner ending it — a window stated without its scheduler promises an expiry no owner runs.
- Recovery grades ONE verdict on two proofs whose halves absorb absence oppositely — an unmeasured RPO refuses, an absent RTO passes.
- `OpLogEntry.Sequence` resumes a drain and orders NOTHING — two stores mint sequence 41, so the dot is the only portable coordinate.
- `OperationId.Counter` IS the origin's `VersionVector` slot — a second counter beside it drifts the moment either advances alone.
- One `DotSource` per store mints every dot — the changefeed range and the authoring stamp reserving apart mint one counter twice.
- `VersionVector` owns `Ordered`/`CanonicalBytes` and every byte-deriving reader takes them — a caller enumerating `Slots` writes bucket order.
- `OperationId.Counter` streams as ONE parity-pinned `CanonicalWriter.I64` word — `U64` never substitutes, a sixteen-byte twin forks the key.
- `ColumnFamily.Crdt.Codec` is generated `crdt.CrdtOpWire`; `OpLogEntryWire` retains those bytes at `[Key(6)] Payload`.
- `OpLogEntryWire`'s thirteen-slot MessagePack envelope leaves every non-CRDT lane opaque.
- `OpLogWire.Encode` and `Decode` share one admission fold; sync ports exchange frames, never typed entries.
- Ordinary dots are nonzero and gap-free; only the exact empty-origin, zero-counter, empty-context value is genesis.
- New entries apply only where the current frontier dominates their pre-mint context, since a later dot otherwise hides a missing predecessor.
- Closure is unique and strictly content-key ordered, and excludes the entry payload key that transfer adds separately.
- `Subscription` VALUES over `Binding` rows are the whole delivery-target family — a per-transport case re-mints the knob set the row carries.
- `protocolsettings` admits against the binding row's OWN roster — a key accepted and ignored publishes governance no leg ever reads.
- `id` renders the dot and `subject` the content key — collapsing them makes two peers' identical deltas one event and drops the second.
- `datacontenttype` is payload-arrow ROW DATA naming the body codec's media type.
- `dataschema` is an optional absolute URI for event data; protobuf `proto_data` uses its `Any` type URL, never a registry subject.
- Registry subject/version remains typed codec configuration outside the envelope; the `Any` type URL remains payload schema identity.
- Completed generated `Extensions` validates through one descriptor-rooted Celly evaluator before projection or ingress admission.
- `Egress.Envelope` returns `Fin` — the owner's `Validate()` IS the mint boundary, so a malformed grammar value letters before a transport takes it.
- CESQL evaluation is TOTAL — a value beside accumulated faults, so a runtime error withholds one event and never darkens a subscription.
- CESQL grammars build ONCE as static parser values — a parser constructed per evaluation rebuilds the expression graph per event.
- `StoreRedrivePort.Carry` is a generic METHOD, not a delegate column — an attempt's value crosses per pass and C# forbids a generic field.
- `Coordinate.Verified` is this port's `verifySucceeded` probe — an unverified re-drive mints a second lease generation and double-decrements.
- `CoordinationFault.Lift` seats at the OUTER strategy edge — a strategy composed beneath it receives a `Fin` value and has nothing left to classify.
- Generated `SyncService` owns cross-store sync: `SyncEndpoint` serves, `SyncWire` dials through AppHost, and `bytes` carries op-log frames.
- `assessment_rows` carries facet arity in one `List(Utf8)` column; its `value` column owns the fact projected by scalar columns.

## [03]-[COLLAPSE]

- `HandleBridge` is the ONE raw-SQLite-handle bridge over every `sqlite3_*` crossing — a second `SqliteConnection.Handle` reach re-opens disposal.
- Reads and writes ride ONE coordination fold — a read names no lock and no guarded statement, so a second read leg is the deleted twin.
- LWT posture rides the bound `CacheProfile` row — a per-call options object re-spells the retry and serial levels the roster already declares.
- ONE presigner keyed by dialed ENDPOINT — S3 and self-hosted share `GrantSigner`, so no row mints a per-provider signer.
- `RetentionSweep.Execute` evicts SET-SHAPED through one arrow — a per-key arrow degrades retention lanes to round trips and strands `EraseMany`.
- `BudgetCredit` IS the seed — its `ON CONFLICT` establishes an absent unit, so a seeding case beside it is the deleted twin of one write.
- Merkle, chain, and subtree digests fold on `ContentHash.Of` — no raw `XxHash128` append site survives in this folder.
- Analytics backend, lakehouse landing, serving, and dataset rosters stay SEPARATE owners — one page holding all hides what an ordinal binds.

## [04]-[STRUCTURE]

- Per-provider variance rides ONE leg carrier — a placement axis widens `BlobHandle`, never fourteen delegate arities.
- Page and object bounds are ROW columns — `EraseBatch` and `ObjectCeiling` state limits no SDK enforces client-side.

## [05]-[PROCESS]

- `KeySelection` membership or framing widening re-cuts the `elementset` parity vector in the same pass — its slot label stays the corpus name.
- `Optimize` and every scaffold run ONCE PER PROFILE through `IdentityDesignFactory` — a defaulted profile emits one engine's DDL under the other's.
