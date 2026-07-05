# W3 · Rasm.Persistence · Batch b3 — grounding dossier

Pages: `Store/coordination.md` [new] · `Version/egress.md` [new]. Verified primary extracts only; every member carries a `file:line` anchor. Both pages are NEW (absent on disk — `loc` MISSING); grounding is the brief concept + nearest siblings + the .api catalogs both will compose.

## [00]-[INVENTORIES] — real ls

### Doctrine root — `docs/stacks/csharp/`
`README.md · language.md · shapes.md · surfaces-and-dispatch.md · rails-and-effects.md · boundaries.md · algorithms.md · system-apis.md` + `domain/` (`README.md · compute · concurrency · data-interchange · diagnostics · durability · interaction · persistence · postgres · resilience · runtime · transport · validation · visuals`).

### Shared substrate `.api` tier — `libs/csharp/.api/` (31)
`api-csparse · api-extensions-ai · api-generator-equals · api-grpc-* (7) · api-hashing · api-highperformance · api-hybrid-cache · api-jsonpatch · api-languageext · api-mapperly · api-mathnet-numerics · api-mathnet-providers · api-nodatime(+-protobuf/-stj) · api-protobuf · api-quikgraph · api-redaction · api-system-configuration · api-tensors · api-thinktecture-json · api-thinktecture-messagepack · api-thinktecture-runtime-extensions · api-unicolour · api-unitsnet`.

### Folder `.api` tier — `libs/csharp/Rasm.Persistence/.api/` (78; egress/coordination-relevant subset)
`api-npgsql · api-marten · api-nats · api-cloudevents · api-kafka · api-rabbitmq · api-dotpulsar · api-redis · api-pg-net · api-schemaregistry(+-serdes-avro/-json/-protobuf) · api-chr-avro(+-binary/-confluent) · api-cbor · api-messagepack(+-analyzer)`. (Shared-tier stubs re-pointed: `api-hashing/-highperformance/-hybrid-cache/-jsonpatch/-nodatime*/-redaction/-thinktecture-json` are 1-line pointers in the folder tier.)

## [01]-[BRIEF SEAM/RIDER ANCHORS] — `RASM-CS-PERSISTENCE-DECISION.md`

- `:100` — **Version/egress row (NEW, V3/S2)**: "ONE `EgressPump` fold draining the `[V2]` outbox cursor past `OpLogEntry` rows into `EgressSink` delivery rows under ONE CloudEvents envelope. Exactly-once EFFECT: `id`=`OpLogEntry.ContentKey`, `Sequence`=cursor `long`, `Partitioning.partitionkey`=`EntityKey`. … Sinks: webhook/nats/kafka(+serdes)/rabbitmq/pulsar/wire-native/**redis-stream** as seed DATA … `EgressSink.RedisStream` row … `XADD` via `IDatabase` stream ops (`StreamEntry`/`StreamPosition`/`NameValueEntry`), the CloudEvent `id`=`OpLogEntry.ContentKey` carried as the dedup field, consumer-group ack (`StreamReadGroup`) advancing the per-sink cursor, `StreamTrimMode.Acknowledged` the retention stance … `EgressFault` = `DeadLetter`/`SinkRefused`/`CursorStall`/`DeliveryUnconfirmed`". Entry: `EgressPump.Drain(SyncCursor, EgressSink) → EgressReceipt` (`EgressSink` `[Union]` axis). Band Egress 8270.
- `:128` — **Store/coordination row (NEW, V2)**: "the four AppHost PORT contracts on ONE token-VALIDATING fenced-lease store. `CoordinationOp` `[Union]`: `BudgetDebit`, `StepStateCas`+`StepStateInFlight`(READ)+`StepStateLoad`, `LeaseAcquire`/`Renew`/`Release`+`ExpiredScan`(READ), `MembershipUpsert`/`MembershipScan`(READ), `OutboxAdvance`. ONE internal shape — every guarded-write case folds through ONE fenced-CAS predicate; every READ case … through ONE `Received`-style projection; `Coordinate.Run` discriminates the closed `[Union]` via the generated total `Switch`, EXACTLY `graph#STORE_RAIL`'s idiom (`graph.md:217`). ALL READ cases carry the frame's tenant RLS predicate STRUCTURALLY. Budget = fenced predicated compare-and-decrement, PER-UNIT VECTOR — `UPDATE budget_ledger SET balance_i = balance_i - debit_i WHERE tenant=@t AND lease_token >= @held AND balance_i >= debit_i` … `RETURNING` … Kleppmann fencing … `LeaseAcquire` MINTS that generation MONOTONICALLY (`++` via PG row-CAS `RETURNING generation`). `ONE_OUTBOX_EGRESS_SPINE` NAMED (the Marten event stream IS the outbox … mints the durable drain cursor PER-SINK — `outbox_cursor(SinkKey, long Sequence)` … `OutboxAdvance(SinkKey Sink, long Through)` is the cursor-advance case the pump calls). Composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — never a second event store, never a distributed-lock sidecar. `CoordinationReceipt` projects PER-OP". Entry: `Coordinate.Run(CoordinationOp, Option<LeaseToken>, ProjectionContext frame) : IO<Fin<CoordinationReceipt>>`. Band Coordination 8430. Consumer: `MembershipView.Serving` (`Wire/coordination.md:79`).
- `:142` — EgressFault 8270 (827x): `DeadLetter · SinkRefused · CursorStall(held-cursor) · DeliveryUnconfirmed` (types the `pg_net` `net.request_status`=PENDING/ERROR reconciliation).
- `:156` — CoordinationFault 8430 (8431-8436): `LeaseFenced(LeaseToken Stale, long Current) · CasConflict(WorkflowKey, StepKey, StepState Expected, StepState Found) · BudgetExhausted(string Unit, long Requested, long Available) · LeaseExpired(LeaseKey, HolderId) · MembershipLapsed(MembershipKey, MemberId) · OutboxDrain(SinkKey Sink, long Through)` (failed `OutboxAdvance` cursor-CAS — kept inside the fenced store's rail, never an `EgressFault`).
- `:74` — RECEIPT VALIDITY FLOOR: `EgressReceipt`/`CoordinationReceipt` implement kernel `IValidityEvidence`, `IsValid` = one `ValidityClaim.All` fold (`Rasm/.planning/Domain/rails.md#[06]-[VALIDITY_FOLD]`; a hand-rolled `&&` chain is deleted).
- `:191`,`:339` — intra-leg cursor edge FORWARD-ONLY: `egress(3)→coordination(3)#OUTBOX_CURSOR` — coordination lands BEFORE egress; the pump reads the cursor; coordination never reads the pump (S2).
- `:306` — leg 3 order: "…then coordination (before the egress pump), then egress". `:329` acceptance `coordination-dryrun` = `Store/coordination.md#Coordinate.Run · Version/egress.md#EgressPump.Drain`.
- `:352` V2 / `:353` V3 verdicts; `:243` Redis INTEGRATE (`EgressSink.RedisStream` + L2 backplane); `:257` Egress-sink axis (webhook·nats·kafka·rabbitmq·pulsar·wire-native·redis-stream, `EgressSink` `[Union]`); `:293` `DistributedLock.Postgres` REJECTED (no fencing tokens; V2 token-VALIDATING CAS strictly stronger); `:292` `WolverineFx` REJECTED (envelope-table outbox parallel to stream-IS-outbox).

## [02]-[SIBLING/FOLDER CONTEXT ANCHORS]

### `Version/ledger.md` (egress's CDC source; coordination's cursor partner)
- `:16` — `Replay(feed, ReplayWindow)` is THE windowed read; `ReplayWindow.DurableOps` is "the `Version/egress` durable-ops CDC drain (every `Family.Durable` lane past the outbox cursor)".
- `:21` — "the durable lanes (`Family.Durable`) are the exactly-once CDC row source the `Version/egress` pump drains past the `Store/coordination#OUTBOX_CURSOR` — `ReplayWindow.DurableOps` is that drain's parameterization, and the presence/awareness lane (`durable:false`) stays the lossy `DrainSurface` channel, NEVER the exactly-once CDC envelope."
- `:109-117` — `OpLogEntry(long Sequence, ModelId Model, string EntityKey, ColumnFamily Family, SyncOpKind Kind, ReadOnlyMemory<byte> Payload, UInt128 ContentKey, TraceSlot Trace, Seq<UInt128> Closure, string Actor, Guid OriginStoreId, Instant Physical, ulong Logical)` — `EntityKey`(=partitionkey), `ContentKey`(=CloudEvent id), `Trace`(=traceparent source).
- `:119-121` — `SyncCursor(Guid OriginStoreId, long Sequence, Instant Physical, ulong Logical)`; `Genesis`. (Egress `Drain` first arg; the per-origin cursor, DISTINCT from the per-sink `outbox_cursor(SinkKey, long)`.)
- `:130` — `DurableOps` = `toSeq(ColumnFamily.Items.Filter(static f => f.Durable))` filter.

### `Element/graph.md` (the STORE_RAIL idiom coordination mirrors)
- `:185` — `public static IO<Fin<GraphReceipt>> Run(IDocumentSession session, ElementIdentity identity, GraphStoreOp op, StoreActor actor, Guid storeId, ProjectionContext frame)`; `:186` runs through the generated total `GraphStoreOp.Switch` (compile-time exhaustive).
- `:199` `StoreActor` `[ComplexValueObject]`; `:204` `ProjectionContext(Func<long> Mark, Func<long,Duration> Elapsed, Func<Instant> Now, Guid Correlation, UInt128 Tenant)` — the injected frame ([A.1]); a `ClockPolicy`/`CorrelationId` parameter is the named strata inversion.
- `:213-221` `GraphStoreOp` `[Union]` (Open/Commit/CommitExclusive/Retire/Read/ReadAsOf) — the closed-family + total-Switch template `CoordinationOp`/`EgressSink` follow.

### `Rasm.Persistence/ARCHITECTURE.md` — FOLDER-WIDE GAP
- Codemap `:32-34` (Store/) shows `BlobStore.cs · Provisioning.cs` ONLY — **no `Coordination.cs` node**; `:16-23` (Version/) shows `Ledger…Recovery.cs` — **no `Egress.cs` node**. Both nodes land when leg 3 rebuilds the ARCHITECTURE codemap+seams (the leg tail owns index docs). Seams `:62` already carries the `StoreActor/ProjectionContext/ResolvedProfile` injection ([A.1]); the four coordination PORT seams (APPHOST:76-79) + `MembershipView.Serving` and the `egress→coordination#OUTBOX_CURSOR` edge are NOT yet in `[02]-[SEAMS]` — a routed folder-context gap for the leg-3 tail.

## [03]-[VERIFIED .api MEMBER BLOCKS]

### `api-npgsql.md` — coordination lock/CAS + egress WAKE
- `:196` `SELECT pg_advisory_xact_lock(@key)` via `NpgsqlCommand` — transaction-scoped exclusive lock, auto-released at commit.
- `:198` `pg_advisory_xact_lock_shared(@key)` — transaction-scoped SHARED lock (readers coexist). [UNDERUSED — see coordination worklist.]
- `:200` `NpgsqlBatch` of `pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` — "one round-trip lock-then-fenced-CAS". (The Budget/CAS write shape.)
- `:209` `NpgsqlConnection.Notification` (`event NotificationEventHandler`); `:211` `SELECT pg_notify(@channel, @payload)` via `NpgsqlCommand`; `:212` `NpgsqlConnection.WaitAsync(CancellationToken)`; `:213` `WaitAsync(TimeSpan/int, CancellationToken)`.
- `:260` "coordination (`Store/coordination#OUTBOX_CURSOR`): the fenced-lease store composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql `pg_advisory_…`".
- `:261` "egress (`Version/egress#EGRESS_SINK`): `NpgsqlConnection.Notification` + `WaitAsync` is the low-latency WAKE the egress pump (`EgressPump.Drain`) lifts".

### `api-marten.md` — outbox spine + changefeed source
- `:72` `IDocumentSession` — write side: `Store/Insert/Update/Delete`, `Events`(write), `SaveChangesAsync`, `SetH…`. `IDocumentOperations` (`:72` row [03]) shared by sessions + projection apply.
- `:185` `Events.FetchForWriting<T>(Guid id, [long expectedVersion], [CancellationToken])` → optimistic write handle `IEventStream<T>` (`:87`).
- `:117` `Marten.Subscriptions.SubscriptionBase` (`: JasperFxSubscriptionBase<IDocumentOperations,IQuerySession>`), `:120` `IChangeListener`/`NullChangeListener`.
- `QueueSqlCommand` — cited by brief `:128` + `api-npgsql:260` as the same-`IDocumentSession` SQL-command queue (the fenced-CAS UPDATE commits in the SAME session as the Marten event, the ONE_OUTBOX_EGRESS_SPINE same-tx guarantee). assay `api query --key marten` RESOLVED the assembly (candidates: `Marten.Events.MartenCommand`, `Marten.IQuerySession`, …) but did not surface `QueueSqlCommand` as a `--symbol` type match; it is a Marten `IDocumentOperations` instance method (not a type), catalog-and-brief-cited — treat as catalog-verified, re-confirm at leg-3 authoring via a method-scoped decompile.

### `api-redis.md` — EgressSink.RedisStream
- `:56` `StreamEntry` `(Id, NameValueEntry[])`; `:57` `StreamPosition` `(Key, Position)` static `Beginning`/`NewMessages`; `:58` `StreamIdempotentId` — "dedup id for at-most-once `StreamAdd`"; `:59` `StreamTrimMode` `KeepReferences/DeleteReferences/Acknowledged`; `:60` `NameValueEntry`.
- `:122` `StreamAdd(key, pairs, messageId?, maxLength?, …, trimMode, flags?)` — XADD with `StreamTrimMode` capped trim; `:123` `StreamAdd(key, field, value, StreamIdempotentId, …)` — "at-most-once XADD keyed on `StreamIdempotentId`". [UNDERUSED — the id=ContentKey dedup should ride StreamIdempotentId, not a manual field.]
- `:125` `StreamReadGroup(key, group, consumer, position?, …)` — XREADGROUP cursor-replay; `:126` `StreamAcknowledge(key, group, messageIds, flags?)` — XACK advancing the group cursor.
- `:39/:135` `ISubscriber`/`Subscribe`/`Publish` + `:174` `ChannelMessageQueue : IAsyncEnumerable<ChannelMessage>` — pub/sub (the L2-invalidation backplane, cache-owned; a low-latency WAKE candidate).

### `api-cloudevents.md` — the ONE envelope
- `:53` `CloudEvent` (mutable sealed v1.0); `:93` `Id/Source/Type/Time` — required `id`/`source`/`type` + optional `time`; `:60` `Partitioning` static — `partitionkey` attribute + `Set`/`GetPartitionKey`.
- `:79` `JsonEventFormatter` (`CloudEventFormatter` over STJ); `:80` `JsonEventFormatter<T>` typed binary-mode.
- `:144` `cloudEvent.ToKafkaMessage(contentMode, formatter)` → `Message<string?, byte[]>` (key = partitionkey).
- `:172` "populated with required `Id` (the content key), `Source` (a stable `rasm:persistence/oplog` URI), `Type` …"; `:174` "`Partitioning.SetPartitionKey(ce, entityKey)`".
- `:166` "distributed-trace continuity stacks through a CloudEvents extension attribute … the egress sets `traceparent` (and `redacted`) as [extension attrs]". [UNDERUSED trace/redaction seam.]
- `:164` "the changefeed wire is a single rail: `CdcEnvelope` … → `CloudEvent` via the `Version/egress` `Egress.Envel…`" — the pre-named egress envelope surface.

### `api-nats.md` — EgressSink.Nats
- `:44` `NatsHeaders : IDictionary<string,StringValues>` — "the `traceparent`/`Nats-Msg-Id` carrier".
- `:100` `INatsJSContext.PublishAsync<T>(subject, data, …)` → `ValueTask<PubAckResponse>` (at-least-once durable publish); `:101` `TryPublishAsync` → `NatsResult<PubAckResponse>` (ROP rail).
- `:109` `INatsJSMsg<T>.AckAsync/NakAsync/AckProgressAsync/AckTerminateAsync` — the Settle ack.
- `:148` "`PubAckResponse.Duplicate` is the JetStream message-dedup-window flag (set when the broker recognizes a previously-seen `Nats-Msg-Id`)". [UNDERUSED confirm-idempotency signal.]
- `:161` "the `EgressSink.Nats` sink: NATS is the `SinkBinding.Subject`/`ContentMode.Binary` row".
- `:164` "JetStream KV + Object Store as store backends: the revisioned-CAS KV (`CreateAsync`/`UpdateAsync(revision)`) is a `Store/coordination` distributed fence". [UNDERUSED — an alternate coordination backend axis.]

### `api-pg-net.md` — webhook sink advance-on-SUCCESS
- `:35` `net.http_post(url, body jsonb, params, headers, timeout_ms)` → `bigint` request-id; auto-injects `Content-Type: application/json`.
- `:50` `net._http_response` UNLOGGED `(id, status_code, content_type, headers, content, timed_out, error_msg)` keyed by request-id; `:52` `net.request_status` enum `('PENDING','SUCCESS','ERROR')`; `:53` `net.http_response_result (status, message, response)`. (The advance law: advance cursor ONLY on SUCCESS; PENDING holds, ERROR/timeout dead-letters — `EgressFault.DeliveryUnconfirmed`.)

### `api-kafka.md` / `api-rabbitmq.md` / `api-dotpulsar.md` / `api-schemaregistry-serdes-*`
- kafka `:24` `IProducer<TKey,TValue>`; `:39` `Message<TKey,TValue>`; `:45` `DeliveryResult<TKey,TValue>`; `:70` `ProducerConfig` — "idempotence, linger, compression"; `:102` `ProduceException`. [ProducerConfig idempotence UNDERUSED.]
- rabbitmq `:29` `IChannel`; `:30` `CreateChannelOptions` — publisher-confirm + dispatch-concurrency; `:35` `OutstandingPublisherConfirmationsRateLimiter` (default `ThrottlingRateLimiter(128,50)`); `:86` `IConnection.CreateChannelAsync(CreateChannelOptions?, ct)`. [publisher-confirm tracking UNDERUSED for DeliveryUnconfirmed.]
- pulsar `:32` `IProducer<TMessage>` (`ISend`/`SendChannel`); `:38` consumer base `Acknowledge`/`Unsubscribe`; `:108` `CreateProducer<TMessage>(ProducerOptions<TMessage>)`.
- schemaregistry-serdes-avro `:25` `AvroSerializer<T>` : `IAsyncSerializer<T>`; `:35` `IAsyncSerializer<T>` — the `Confluent.Kafka` `SetValueSerializer` target; `:38` `ISchemaRegistryClient`. Siblings `api-schemaregistry-serdes-json`/`-protobuf` present (JSON-Schema + Protobuf serde codec columns). [serdes codec AXIS avro·json·protobuf UNDERUSED vs avro-only.]

## [04]-[CROSS-PACKAGE SEAM ANCHORS] (consumer contracts, not local .api members)
- AppHost PORT counterparts (brief `:128`,`:190`, APPHOST:76-79): `capability.md:54-62` CostVector (string-keyed unit → Budget `HashMap<string,long>`); `orchestration.md:257-284` StepStateSeam + `:271,:284` `StepStateInFlight` for `CrashResume`; `Wire/coordination.md:79` `MembershipView.Serving` (the in-process membership consumer). Direction AppHost→Persistence READ/decode (correct HOST-BOUNDARY→APP-PLATFORM), no down edge. NOT Persistence `.api` members — verified as brief-cited consumer contracts, deferred to the AppHost campaign's own interiors.
