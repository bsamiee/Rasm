# [PERSISTENCE_RULINGS]

`Rasm.Persistence` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `age` admits `ExtensionAdmission.Standalone` — `LOAD` is a per-connection concern, so `Preload` faults correct deployments on `MissingPreload`.
- `OpenTelemetry.Instrumentation.ConfluentKafka` links each record's context on consume — a `CdcIngress` fold mints a second edge for one cause.
- `Npgsql.NodaTime` admits on the DATA SOURCE beside its spatial sibling — the EF plugin places no codec on a raw connection.
- `AMQPNetLite.Core` admits as a DIRECT dependency — `Version/egress` composes its links, so arriving transitively leaves that fence unpinned.
- `Apache.Arrow.Flight.AspNetCore` moves in lockstep with `Apache.Arrow.Flight` — it alone holds the `InternalsVisibleTo` grant reaching the adapter.
- `RocksDbException` publishes no code — its message IS `Status::ToString()`, so a prefix roster re-proves against the installed library.
- `Azure.Storage.Blobs.Batch` tracks its OWN version line — it lags the blobs line by design, so pinning them equal resolves nothing.

## [02]-[SHAPE]

- `SetKey` is the ONE model-qualified membership axis — `(ModelId, NodeId)` under one byte order, so a bare-`NodeId` axis re-mints the ambiguity.
- `SetScope` arrives as caller DATA — the evaluator reads no rollup, so `ProjectGraph` supplies a scope and read-your-writes holds per model.
- Federation crosses ONLY `ModelLink` rows — in-model `Relationship` never widens, so a scoped selection and the `Federate` union never substitute.
- Rank vocabulary declares the expression and DERIVES its order — `Rank` is `"{Score} DESC"`, so no arm orders by an expression it never projects.
- `ArtifactKind` DERIVES retention from provenance — `Texture` answers rebuildable `Cache` or acquired `Blob`, so no caller sets retention.
- `LandingArm` is the DATASET shape, never the producing package — a shared arm splits its hive tree on a segment neither scan can prune on.
- `ProjectionContext` is this package's one time-and-causal frame — a `ClockPolicy` or `Principal` parameter inverts the strata the frame fixes.
- `Crdt.Apply` and `GraphDelta.Apply` are the only materializers — projection, merge, and AS-OF fold one delta, so a second forks replay.
- Usage levels carry `rasm.tenant` alone — class and tier stay `UsageReceipt` facts, so no meter dimension multiplies the capped tenant series.
- Settlement fans every column `EgressReceipt.Drained` partitions, duplicates included — an omitted column reports a rate above its own traffic.
- Residence literals render through the declared `ColumnType` — a stringified operand kills pruning, coerces on PostgreSQL, raises on ClickHouse.
- Read SCOPE parts from SHAPE at the residence entry — tenant rides the frame and window `ResidenceWindow`, so no plan expresses a cross-tenant scan.
- Rollups materialize SUMMARY STATE and readers name the accessor — a stored `avg` over an irregular series ships a mean the tile never claimed.
- Columnstore segment lists carry BOUNDED keys alone — segmenting a `KeyHex` content key mints one batch per row and deletes the compression itself.
- Residence relations resolve through one lower-cased `AnalyticsSchema.Table` DDL and read both quote — PostgreSQL case-folds, ClickHouse does not.
- `ColumnShape` GENERATES containers over `ColumnType` — `List`, `Map`, and `Dictionary` are cases, so a flat `map-string-string` row forks it.
- `ColumnType` and `ColumnShape` stay BRANCH-LOCAL — no contract manifest names one, so a peer planting a relation reaches `ResidenceDdl`.
- No metrics store enters the `Residence` roster — a TSDB's per-series cardinality ceiling is the one column this family refuses to grow.
- Residence health measures OUTCOME — `ResidenceRead.Health` compares resident extent to horizon, so one engine's catalog answers for one tier.
- Every `ResidenceFault` pairs its residence with one neutral `EngineFault` — a per-backend case carries a column only one engine fills.
- Arity proves against `ResidenceLanding.Supplied`, never `Payload` — the custodian's tenant and landing instant are cells a producer cannot send.
- `ReadRouter.Observed` and the read receipt answer DISJOINT questions — where reads spend versus how one resolved; a lifted phase twins one number.
- CRDT payload equality rides `CrdtBytes` on `ReadOnlyMemory<byte>` — an `ImmutableArray<byte>` swap re-types the `CrdtWire.Decode` zero-copy seam.
- `TimeSpine` rides the DECLARATION — `Admit` faults a category its columns contradict, since one inferred from `time` re-dates event facts.
- `KvSpace` is the ONE keyspace axis both KV engines realize — a composite-key prefix carries none of the four postures a row does.
- KV key order proves by OMISSION — no row supplies a comparer, so a declared `CompareWith` voids every prefix stop with no compile error.
- `RetryShape` carries the re-drive ROUTE and `IsTransient` narrows to `Waited` — a wait against `BUSY_SNAPSHOT` spins on the caller's own read.
- `EmbeddedFault.Busy` DERIVES its route from the extended status it keeps — a retry column copies a value the case already reads.
- `KvSeal.Ordered` names a value that is itself a POSITION — sealing a dup value voids `GetBoth`, `Unlink`, and `DuplicatesSort` together.
- `KvVault` nonces are RANDOM per value — a key-derived nonce repeats across every member `Append` accrues under one key.
- LMDB sync is ENVIRONMENT-scoped — the environment takes the roster's strictest `KvDurability`, so a per-space posture is inexpressible.
- `SpoolAccrual` length-frames every merge operand — a separator join shifts two member splits onto one boundary.
- `Lane` is the ONE lane-token owner — `StoreProfile.Lanes` and `ServerExtension.Lane` compose it, so a bare token forks one vocabulary.
- Every lane admits ONCE at its owning entry — `Admit` and `Register` read the ROW's own `Lane`, so a new lane gates with no new call site.
- `StoreProfile` and `BackendProvider` are DISJOINT — one names an engine a generation is minted FOR, the other an engine this package OPENS.
- `FlightServer` is NOT a gRPC service — no bind attribute rides its hierarchy, so `MapGrpcService<T>` fails and `AddFlightServer<T>` binds.
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
- `StoreProfile.Admits` gates the cache lane — Marten backs both residences, so a single-process store realizes neither.
- `WideColumnFault.Foreign` is the open tail — folding an unmapped throw into `Unavailable` publishes a level and replica counts nobody measured.
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
- `IStoreRetriable` is the ONE retriability spelling both process-seam bands answer — a per-band fold arm re-decides what each band already published.
- `StoreVerdict` is the currency the re-drive seam crosses on — naming a pipeline type inverts the strata `StoreRedrivePort` and its unbound row hold.
- `ReconcileAxis` owns every manifest axis token beside `RestartClass` — a bare literal forks one axis into two a deploy plane then diffs as two.
- `OutboxCursor` carries `Parked`/`Attempt`/`Status` per sink — a per-event delivery row gives one commit two owners and one sink another's retries.
- `SweepReceipt` conserves over FIVE slots — a refused key booked as `Evicted` reports reclaim the lane never released and bytes still resident.
- `LeaseGuard` is advisory DETECTION — read as a gate it re-mints the frozen-guard scar each guarded write's own `fence <= @token` predicate closes.
- `EmbeddedFault.Reoffer` dispatches all four `RetryShape` routes — `IsTransient` narrows to `Waited`, so a bool drops `Restarted` and `Rescoped`.
- Residence floor columns spell `Fits`/`Admit`/`Tenancy`/`Lifetime`/`Degrade` — `Ingest` names one entry kind and `Retain` asks no owner to end a row.
- `Lifetime` states the extent AND the owner ending it — a window stated without its scheduler promises an expiry no owner runs.
- Recovery grades ONE verdict on two proofs whose halves absorb absence oppositely — an unmeasured RPO refuses, an absent RTO passes.
- `OpLogEntry.Sequence` resumes a drain and orders NOTHING — two stores mint sequence 41, so the dot is the only portable coordinate.
- `OperationId.Counter` IS the origin's `VersionVector` slot — a second counter beside it drifts the moment either advances alone.
- One `DotSource` per store mints every dot — the changefeed range and the authoring stamp reserving apart mint one counter twice.
- `VersionVector` owns `Ordered`/`WriteTo` and every byte-deriving reader takes them — a caller enumerating `Slots` writes bucket order.
- `Subscription` VALUES over `Binding` rows are the whole delivery-target family — a per-transport case re-mints the knob set the row carries.
- `protocolsettings` admits against the binding row's OWN roster — a key accepted and ignored publishes governance no leg ever reads.
- `id` renders the dot and `subject` the content key — collapsing them makes two peers' identical deltas one event and drops the second.
- `datacontenttype` and `dataschema` are payload-arrow ROW DATA — a literal names the mint site's guess over a registry-framed body.
- `Egress.Envelope` returns `Fin` — the owner's `Validate()` IS the mint boundary, so a malformed grammar value letters before a transport takes it.
- CESQL evaluation is TOTAL — a value beside accumulated faults, so a runtime error withholds one event and never darkens a subscription.
- CESQL grammars build ONCE as static parser values — a parser constructed per evaluation rebuilds the expression graph per event.

## [03]-[COLLAPSE]

- Reads and writes ride ONE coordination fold — a read names no lock and no guarded statement, so a second read leg is the deleted twin.
- LWT posture rides the bound `CacheProfile` row — a per-call options object re-spells the retry and serial levels the roster already declares.
- Tag-line owner reads seat at the `StoreSlot` owner — the wire tap and the plan harvest read one grammar, so a second parse forks it.
- ONE presigner keyed by dialed ENDPOINT — S3 and self-hosted share `GrantSigner`, so no row mints a per-provider signer.
- `RetentionSweep.Execute` evicts SET-SHAPED through one arrow — a per-key arrow degrades every lane to one round trip and strands `EraseMany` whole.
- `BudgetCredit` IS the seed — its `ON CONFLICT` establishes an absent unit, so a seeding case beside it is the deleted twin of one write.

## [04]-[STRUCTURE]

- Per-provider variance rides ONE leg carrier — a residence axis widens `BlobHandle`, never fourteen delegate arities.
- Page and object bounds are ROW columns — `EraseBatch` and `ObjectCeiling` state limits no SDK enforces client-side.
- `StoreRedrivePort.Carry` is a generic METHOD, not a delegate column — an attempt's value crosses per pass and C# forbids a generic field.

## [05]-[PROCESS]

- `ElementSet` widening re-cuts the `elementset` parity vector in the same pass — a lone change diverges one selection's hash across runtimes.
- `Optimize` and every scaffold run ONCE PER PROFILE through `IdentityDesignFactory` — a defaulted profile emits one engine's DDL under the other's.
- `Coordinate.Verified` is this rail's `verifySucceeded` probe — an unverified re-drive mints a second lease generation and decrements a vector twice.
- `CoordinationFault.Lift` seats at the OUTER strategy edge — a strategy composed beneath it receives a `Fin` value and has nothing left to classify.
