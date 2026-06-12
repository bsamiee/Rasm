# npgsql-driver — bedrock

## data-source law

- `NpgsqlDataSourceBuilder` is the single configuration owner for everything below the store profile; `Build()` yields the one process-lifetime `NpgsqlDataSource` per database, and every later surface (`CreateCommand`, `CreateBatch`, `OpenConnectionAsync`, `CreateConnection`) hangs off it.
- Builder ownership rows: type mapping (`MapEnum`, `MapComposite`, `AddTypeInfoResolverFactory`), JSON policy (`ConfigureJsonOptions`, `EnableDynamicJson(jsonbClrTypes, jsonClrTypes)`), logging (`UseLoggerFactory`, `EnableParameterLogging`), tracing (`ConfigureTracing`), type loading (`ConfigureTypeLoading`), TLS and auth callbacks, and per-connection initialization (`UsePhysicalConnectionInitializer(sync, async)` — the one seam for per-session `SET` state).
- Constructing connections from raw strings after a data source exists is the rejected form — it forks pool identity and forgets every builder policy.
- Credential rotation is builder policy, not connection ceremony: `UsePeriodicPasswordProvider(provider, successRefreshInterval, failureRefreshInterval)` refreshes on a schedule with a distinct failure cadence; `UsePasswordProvider(sync, async)` resolves per physical open. Token-style auth becomes a declared row — no password at rest in the string.
- `EnableParameterLogging` is a deliberate two-state secret-handling decision: parameter values are absent from logs until explicitly admitted; turning it on in shared environments is a redaction-policy violation waiting to happen.
- Type-loading policy: `ConfigureTypeLoading` exposes `EnableTypeLoading(false)` (skip the catalog query entirely — fixed-vocabulary deployments), `SetTypeLoadingSchemas(...)` (bound the catalog scan), and `EnableTableCompositesLoading` (admit table-row composites).
- A trimmed builder variant exists for size-constrained binaries with per-capability opt-in; the default builder admits the full type system. Choosing slim is a deployment-topology row, not code structure.
- Store-owns-its-strategy boundary: the data source owns retry-relevant knobs (`Timeout`, `CommandTimeout`, `CancellationTimeout`, keepalives) as profile data, and the store-level execution strategy (`EnableRetryOnFailure(maxRetryCount, maxRetryDelay, errorCodesToAdd)`) is declared on the provider options beside it — transient-fault classification (`NpgsqlException.IsTransient`, extendable by SQLSTATE rows) never reaches an outbound-hop owner. The data source is the unit a strategy wraps, never the thing that retries itself.
- Per-source server settings ride the `Options` startup parameter — session floors (a statement-timeout ceiling for the whole profile) are connection-string facts applied at connection birth, distinct from the physical initializer which owns programmatic per-session state.
- `CancellationTimeout` is the cancel-escalation knob: how long a cancellation request may wait for the server's acknowledgement before the connection is hard-closed — cancellation is cooperative first, destructive second, and the window between is declared.
- Multi-host: `BuildMultiHost()` yields one source spanning hosts; per-acquisition routing passes a session-attribute value (primary, standby, prefer-standby, read-write, read-only) at open or via wrapped per-attribute views; `LoadBalanceHosts` and `HostRecheckSeconds` are policy rows.
- Role detection rides server probes (recovery state, default-transaction read-only) on the recheck cadence; read/write splitting is an acquisition-time policy value on one source — never two configured sources.
- ADO-vs-mapped seam: the EF provider accepts a `DbDataSource` directly, and its options expose `ConfigureDataSource(Action<NpgsqlDataSourceBuilder>)`. When ADO depth (COPY, LISTEN, replication) and mapped querying coexist, one shared data source serves both — two sources to one database is pool fragmentation.

## fault surface

- Two-tier exception law: `NpgsqlException` is the driver/transport tier (carries `IsTransient` and the failing `BatchCommand`); `PostgresException` is the server tier, carrying the full structured error: `SqlState`, `Severity`, `MessageText`, `Detail`, `Hint`, `Position`, `SchemaName`, `TableName`, `ColumnName`, `DataTypeName`, `ConstraintName`.
- The structured fields make boundary fault conversion lossless: a unique violation converts to a typed conflict fault carrying the constraint name; a check violation names its constraint; no message parsing exists.
- `PostgresErrorCodes` supplies SQLSTATE constants — fault-class dispatch is `switch` over constants, never string literals.
- `Detail` and `Hint` can carry row data on constraint violations; they are evidence for diagnostics, not user-facing text — classification-aware redaction applies before export.
- Error-detail admission is itself a connection-string decision: `IncludeErrorDetail` gates whether the data-bearing detail fields populate at all, and `IncludeFailedBatchedCommand` gates whether a batch fault names its failing command text — both default closed, and opening them is a redaction-policy decision, not a debugging convenience.
- Channel security is declared profile data, never negotiated implicitly: `SslMode`, `RequireAuth`, and the GSS encryption mode pin what the connection will accept — a server downgrading below the declared floor is a connection failure, not a silent fallback.

## pooling, multiplexing, pipelining

- Pool policy is connection-string data: `MaxPoolSize`/`MinPoolSize`, `ConnectionIdleLifetime`, `ConnectionPruningInterval`, `ConnectionLifetime`, `NoResetOnClose`.
- Pool reset-on-return is a session-state discard; `NoResetOnClose` trades session hygiene for hot-path latency and is admissible only where the physical initializer establishes no per-session state.
- Multiplexing (`Multiplexing=true`, requires pooling) decouples logical commands from physical connections: many in-flight commands coalesce onto few connectors, `WriteCoalescingBufferThresholdBytes` governing write batching.
- Binding law under multiplexing: transactions (which must start through the transaction factory in this mode), COPY operations, LISTEN sessions, and replication each bind a dedicated connector for their window; multiplexing serves the stateless query swarm around them.
- Multiplexing admission test: session-scoped state (temp tables, `SET` parameters) is unsafe outside a bound window; a lane relying on session state either binds or disqualifies multiplexing.
- Automatic preparation: `MaxAutoPrepare` (off at zero) with `AutoPrepareMinUsages` (five) promotes hot statements into an LRU-managed prepared set; explicitly prepared statements are exempt from eviction.
- Prepared state survives pooled close/reopen — preparation cost amortizes across the pool's life, not a connection's. Long-lived processes set a nonzero auto-prepare budget and let the LRU manage it; explicit `Prepare()` is reserved for the known-fixed hot set.
- `NpgsqlBatch` is the pipelining unit: N statements, one round trip, results read sequentially from one reader via `BatchCommands` rows — each batch command carrying its own statement and parameters, so a batch is constructible as a fold over a fact collection.
- Source-level execution composes with multiplexing: `CreateCommand`/`CreateBatch` on the data source execute without an explicitly opened connection — the connection-less shape is the multiplexed fast path, and explicitly opened connections are reserved for the binding lanes.
- `EnableErrorBarriers` inserts a protocol synchronization point after every batch command: without it the first failure poisons the remainder of the batch; with it each command fails independently and outcomes fold per-command.
- Barrier decision: all-or-nothing batches (one transaction) leave barriers off; independent-fact batches (telemetry writes, multi-tenant fanout) turn them on and fold per-command outcomes into receipts.
- Round-trip arithmetic — the three levers are orthogonal and compose: batch what is sequential (latency linear in statement count collapses to one), multiplex what is concurrent (connection count collapses under fan-in), prepare what repeats (parse/plan cost collapses under repetition).

## binary COPY

- Import surface: `BeginBinaryImport(copyFromCommand)` → `NpgsqlBinaryImporter` with `StartRow()`, `Write<T>(value)`, `Write<T>(value, NpgsqlDbType)`, `Write<T>(value, dataTypeName)`, `WriteNull()`, `WriteRow(params)`, terminal `Complete() → ulong` (rows written).
- The commit edge is inverted from ordinary writers and is the lane's defining trap: `Complete()` commits; disposing without `Complete()` cancels the COPY and discards every buffered row.
- The inversion makes the importer a natural bracket-shaped resource: the success branch ends in `Complete`, the failure branch is plain disposal — exception safety means data is discarded, never half-written.
- Type discipline inside COPY: binary COPY has no server-side coercion — each `Write<T>` must match the column wire type exactly; the `NpgsqlDbType`/`dataTypeName` overloads disambiguate jsonb-vs-json, array element types, and range species.
- A wire-type mismatch surfaces mid-stream as a copy fault, not at `Complete` — write the discriminated overload wherever the CLR type maps ambiguously.
- Export surface: `BeginBinaryExport` → `StartRow() → int` (column count; end-of-data sentinel), `Read<T>()`/`Read<T>(NpgsqlDbType)`, `IsNull` probe before read, `Skip()` for unwanted columns, `Cancel()` for early abandonment without protocol corruption.
- Raw lane: `BeginRawBinaryCopy` streams the entire COPY payload as opaque bytes — the table-to-table pipe between databases with zero materialization: export raw from one source, import raw into the other.
- Text COPY (`BeginTextImport`/`BeginTextExport`) exists for text-format tool interop only; binary is the default lane.
- Server-side filters belong in the COPY SQL itself (`COPY (SELECT ...) TO ...`); defect budgets ride the SQL-law bulk-admission options — the importer never filters.
- COPY binds a connector (no multiplexed COPY) and out-throughputs batched INSERT by an order of magnitude; routing rule: any write set large enough to batch twice is large enough to COPY.
- Importer and exporter carry their own `Timeout` independent of command timeout — bulk windows are sized by the bulk operation's policy row, not by the query default they would otherwise inherit.

## arrays, ranges, composites, enums

- Arrays are first-class wire citizens: CLR arrays and lists map to native array columns; `ArrayNullabilityMode` governs null-element admission (never, always, per-instance) as profile policy.
- Dimensional shape is part of the wire contract: multidimensional arrays map; jagged arrays do not.
- `NpgsqlRange<T>` carries bound values plus per-bound inclusivity/infinity flags; multiranges map as arrays or lists of ranges; `NpgsqlInterval` carries the wire interval triple where mapped calendar types are not in play.
- Composites: `MapComposite<T>(pgName, nameTranslator)` binds a CLR record to a composite type by name translation, not position — the default snake-case translator maps member names to attribute names, and mismatches surface at first use, not at registration.
- Composite mapping is the one lane where a domain value crosses the wire as itself — admitted once at the builder, used in queries, batches, and COPY identically.
- Enums: `MapEnum<TEnum>(pgName, nameTranslator)` binds CLR enums to native enum types with label translation; the EF options carry a mirror `MapEnum` — wire mapping and model mapping are two declarations of one fact and must agree (the dual-declaration trap: model-only maps fail at the wire, wire-only fail in migrations).
- `EnableUnmappedTypes()` admits enums, composites, and ranges without registration via runtime lookup — a declared concession for schema-fluid lanes, paying per-use resolution; `EnableRecordsAsTuples()` admits positional record reads.
- `ReloadTypes()`/`ReloadTypesAsync()` refresh the data source's type catalog after DDL introduces types; without a reload, newly created enums/composites/extension types resolve as unknown in live processes. Pair every runtime DDL application with a reload on the owning source.

## temporal store mapping

- The calendar-typed mapping plugin is one admission row (`UseNodaTime` on provider options plus its data-source configuration plugin) carrying the full temporal mapping table; store contracts use calendar types, never BCL time, per the settled temporal rail.
- The instant family maps to `timestamptz`: instant, zoned, and offset datetime mappings all land in the same store type — the store records one global timeline point, and zone/offset are presentation facts that do not survive the column.
- The consequence is the timestamptz law: a zoned value round-trips as instant-plus-lost-zone; where the zone itself is data, it is its own column (a zone mapping exists for exactly this), never an expectation of the timestamp column.
- Local datetimes map to `timestamp` — the only legitimate use of the zone-less type: wall-clock facts that must not shift with timeline context (schedules, recurrence anchors). A legacy mapping for instants-as-timestamp exists solely for pre-existing schemas; new columns never take it.
- Date and time-of-day map to `date`/`time`; offset time to `timetz`; both elapsed-time species map to `interval` — the machine-duration and the calendar-period are distinct CLR types over one store type, and the choice declares whether arithmetic is fixed-length or calendar-aware.
- Range mappings cover interval ranges and date-interval ranges with their multirange forms — the storage spine under the SQL law's temporal constraints, making overlap exclusion reachable from mapped types end-to-end.
- Temporal aggregates and measures translate (`Sum`, `Average`, `Distance`, `RangeAgg`, `RangeIntersectAgg` as function projections), and an evaluatable-expression filter keeps temporal expressions inside SQL instead of client-evaluating them — temporal predicates stay set-algebraic.

## LISTEN/NOTIFY

- Server semantics that shape the client law: NOTIFY is transactional (delivered on commit, never on rollback); duplicate notifications within one transaction collapse; payloads are size-capped near 8KB; the notification queue is large but finite, and a stalled listener eventually back-pressures notifying transactions.
- Queue health is observable server-side (`pg_notification_queue_usage()`) — a verification-law row, not a client poll.
- Client mechanics: notifications surface on the connection's `Notification` event (`NpgsqlNotificationEventArgs { PID, Channel, Payload }`) but are only processed during protocol interaction — an idle connection delivers nothing.
- The canonical listener: a dedicated bound connection runs `LISTEN`, then loops `WaitAsync(token)` (timeout overloads exist), with `KeepAlive` enabled so intermediaries do not kill the idle socket.
- The timeout overloads return a boolean — notification-arrived versus window-elapsed — so the wait loop doubles as the listener's liveness heartbeat: an elapsed window is the natural point for the watermark catch-up fetch.
- The trap: handlers attached, no `Wait` loop — notifications appear only when an unrelated query happens to run on that connection. The event without the wait loop is a latent bug, not a slow path.
- Capacity law: notifications are signals, not payload transport — the payload carries a cursor hint (channel plus key), and the listener re-reads authoritative state through the query rail. Payload-as-data loses writes on every reconnect window.
- One listener connection serves many channels (`LISTEN` per channel, demultiplex on `Channel`); listener count per process is one, not per-concern — fanout happens in-process off the single wake stream.
- The `Notice` event is the notification's sibling with opposite semantics: server notices (warnings, raise messages) are informational, non-transactional, and arrive on any connection — diagnostics material, never control flow; conflating the two events routes server chatter into the wake path.

## connection lifecycle levers

- `ClearPool(connection)` / `ClearAllPools()` invalidate pooled physical connections — the levers for credential rotation cutover and post-failover reconnection; new acquisitions re-open under current policy while in-flight work drains on the old sockets.
- `CloneWith(connectionString)` derives a connection with amended policy from an existing one — the narrow seam for one-off divergence (a longer command timeout for one maintenance window) without forking the data source.
- `UnprepareAll()` drops the connection's prepared set — paired with pool clearing after wholesale statement-shape changes; otherwise the prepared-statement persistence law keeps stale plans alive across pooled reopen.
- Connection facts surface as live state: server version, backend process id, current timezone, and the server-parameter snapshot are readable per connection — verification and diagnostics consume them without issuing queries.

## logical replication client — decode into the change contract

- Connection species: `LogicalReplicationConnection` (its own type, not the regular connection) with `Open`, `IdentifySystem` → `ReplicationSystemIdentification { SystemId, Timeline, XLogPos, DbName }`, `Show`, `DropReplicationSlot`, and LSN bookkeeping state: `LastReceivedLsn`, `LastFlushedLsn`, `LastAppliedLsn`, `WalReceiverStatusInterval` (ten-second default), `WalReceiverTimeout`.
- System identification is the stream's coordinate check: system id and timeline pin which cluster history the consumer's stored watermark belongs to — a watermark replayed against a different system id or timeline is a restored-elsewhere cluster, and the consumer's verdict is re-bootstrap, not resume.
- A physical-replication connection type and a text-decoding plugin lane exist; the change contract rides the logical pgoutput lane — the others serve inspection and standby tooling, not the contract.
- Slot creation: `CreatePgOutputReplicationSlot(slotName, temporarySlot, slotSnapshotInitMode, twoPhase)`; snapshot mode is the bootstrap law's hinge.
- Bootstrap law: snapshot mode `Use` inside a transaction reads the exact snapshot the slot begins at — snapshot-read the tables in that transaction, then stream from the slot with zero gap and zero overlap. `Export` hands the snapshot to other sessions; `NoExport` discards it.
- Stream shape: `StartReplication(slot, PgOutputReplicationOptions, token, walLocation?)` → `IAsyncEnumerable<PgOutputReplicationMessage>`.
- Options rows: publication name set; protocol version (four protocol generations); `Binary` (binary tuple data — pairs with the type-mapped vocabulary); `StreamingMode` (off | on | parallel — in-progress large transactions stream before commit); `Messages` (decoded emitted-message passthrough); `TwoPhase`. Streaming mode and protocol version travel together — in-progress and parallel streaming demand the higher protocol rows.
- Message taxonomy — the decode fold's closed input family: transaction framing (`BeginMessage`/`CommitMessage`); schema announcements (`RelationMessage`, `TypeMessage`); data (`InsertMessage`; `DefaultUpdateMessage`/`FullUpdateMessage`/`IndexUpdateMessage`; `KeyDeleteMessage`/`FullDeleteMessage`; `TruncateMessage`); streamed framing (`StreamStartMessage`/`StreamStopMessage`/`StreamCommitMessage`/`StreamAbortMessage` plus a parallel-abort variant); prepared-transaction framing (prepare family); `LogicalDecodingMessage` for application-emitted messages.
- The update/delete case split is replica-identity evidence: which before-image arrives (none | key-only | full) is decided by the table's replica identity. A consumer needing old values must see the full-image cases, and that requirement is a DDL fact (replica identity full), not a client option.
- `RelationMessage` precedes data for each touched relation and re-arrives on schema change: `RelationId`, namespace and name, column rows (name, type id, modifier, key flags), replica identity setting. The consumer's relation cache keyed by `RelationId` is mandatory state — data messages carry only the id.
- Tuple reading is single-pass streaming: `ReplicationTuple` is an async sequence of `ReplicationValue` (`Kind`: null | unchanged-toasted | text | binary; `Get<T>()`, `GetPostgresType`, `IsDBNull`, stream and text-reader access).
- Trap, boundary one: tuples must be fully consumed in order before advancing the message enumerator — the buffer is reused, and deferred reads observe the next message's bytes.
- Trap, boundary two: `UnchangedToastedValue` is not a value — unmodified TOASTed columns arrive as that marker unless replica identity is full; the decode fold carries "unchanged" as a distinct state, never as null.
- Acknowledgement protocol: the enumerator advances `LastReceivedLsn` automatically; durable consumption is only what `SetReplicationStatus(lastAppliedAndFlushedLsn)` reports, sent on the status interval, with `SendStatusUpdate()` forcing a flush.
- The slot retains WAL until acknowledged — the at-least-once contract. Apply-then-acknowledge, acknowledge only what is durably applied, treat redelivery after crash as the normal path; the applied-LSN watermark is the idempotency key.
- Slot-retention pressure from a lagging acknowledger is a server-disk liability — which is why slot lag is a verification-law observable, not a client-side concern.
- Fold into the settled change contract: messages decode into the op-log shape — transaction-framed, LSN-cursored, tag-transition facts — whose law the durability layer owns. This lane owns exactly the decode: message family → fact rows, relation cache, before-image evidence, watermark acknowledgement.
- Cursor unification: the LSN here, the notification wake above, and the SQL-law transition pair are three resolutions of one store-to-consumer change contract — wake fast, fetch deltas, replay gaps from the slot.

## telemetry composition

- Wiring is two composition-root rows: `AddNpgsql()` on the tracer builder; `AddNpgsqlInstrumentation(Action<NpgsqlMetricsOptions>)` on the meter builder. Span and meter identities are driver facts; governance (sampling, export, redaction) arrives settled and this lane only plugs into it.
- Depth lives on the data source via `ConfigureTracing`: per-command and per-batch filters (`ConfigureCommandFilter`/`ConfigureBatchFilter`) drop noise spans at the source — cheaper than sampling them away downstream.
- Enrichment callbacks (`ConfigureCommandEnrichmentCallback`/`ConfigureBatchEnrichmentCallback`) attach domain attributes to the live span; span-name providers (`ConfigureCommandSpanNameProvider`/`ConfigureBatchSpanNameProvider`) collapse per-statement cardinality into stable names.
- `EnableFirstResponseEvent` marks time-to-first-row as a span event — the latency split between server execution and result drain, otherwise invisible inside one span duration.
- `EnablePhysicalOpenTracing` makes physical opens spans — pool starvation becomes directly visible as open-span bursts rather than inferred from latency tails.
- COPY operations carry their own filter, enrichment, and span-name rows (`ConfigureCopyOperationFilter`/`...EnrichmentCallback`/`...SpanNameProvider`) — bulk lanes are first-class trace citizens.
- Composition law: filters and name providers are declared once on the builder beside the type mappings — telemetry shape is store-profile data; a per-call-site tracing decision is the rejected form.

## divergent — datasource-multiplexing

- Maximal unification: every per-database concern — mapping vocabulary, JSON policy, credential rotation, tracing shape, pool/multiplexing/prepare budgets, initializer state, retry classification — is one builder fold producing one process-lifetime source. A new enum is a row; a new telemetry attribute is a row; a topology change is multi-host policy. Scaling the store surface never adds a second configuration site, and the mapped profile inherits the same fold through its data-source seam.
- Connector-binding algebra — the non-obvious unifier: the driver has exactly one exclusivity concept. Transaction, COPY, LISTEN, and replication are all "bind a connector for a window"; multiplexing is the policy for everything unbound.
- Connection budgeting is therefore a closed static table, not an emergent runtime property: listeners + replication streams + concurrent-transaction ceiling + COPY width = bound budget; everything else shares the multiplexed remainder. Each lane classifies as bound or unbound at design time.
- Rejected forms this law forecloses, each named: per-request data sources (pool identity churn); connection-string forks for per-tenant credentials (the rotation provider owns credentials); hand-rolled statement caches (LRU auto-prepare owns repetition); semaphore-guarded connection sharing (multiplexing owns fan-in); late per-call JSON/type configuration (the builder owns admission, once).

## divergent — copy-composites

- COPY × composites × arrays at full depth: binary COPY writes composite, array, and range values through the same registered wire codecs as queries — one mapping registration serves query, batch, and bulk lanes identically.
- The non-obvious consequence: bulk admission of rich domain rows needs zero flattening — `Write<T>` with the mapped CLR type is the whole protocol. A staging-table-of-strings design is a rejected form born of text-COPY habits.
- The COPY-then-reconcile fusion (this lane × SQL law): bulk rows COPY into an unlogged staging relation (bound connector, binary, `Complete()` receipt), then one MERGE reconciles staging into the target with per-row verdicts — the highest-throughput receipted upsert architecture the engine offers.
- Failure taxonomy for the bulk lane: pre-`Complete` exception → zero rows (cancellation semantics, safe whole retry); mid-stream type mismatch → copy fault naming the offending type (fix overload discrimination, retry whole); post-`Complete` constraint violation cannot exist — COPY into constraint-bearing targets validates in-stream, violations abort pre-commit.
- The retry unit is always the whole COPY; partial-progress retry does not exist in the protocol — exactly what makes the lane composable with idempotent staging.

## divergent — notify-replication

- The two channels are one contract at two durability ranks: LISTEN/NOTIFY is ephemeral wake (lost on disconnect, collapsed on duplication, capped payload); the replication slot is durable replay (retained until acknowledged, complete, ordered, before-images on demand).
- The fused consumer law: subscribe both; treat NOTIFY as a latency optimization over the slot's polling cadence; derive correctness solely from the slot's LSN watermark. A consumer correct only when NOTIFY arrives is broken by definition; a consumer ignoring NOTIFY is merely slower.
- Slot lifecycle discipline: temporary slots (dropped on disconnect) serve inspection and tests; durable slots are provisioned-and-verified artifacts whose existence, plugin, and lag are verification rows. Creating durable slots ad hoc from application code couples process lifetime to server disk retention — the operational trap that motivates verification-only provisioning.
- Streamed-transaction staging — the deepest constraint the client protocol imposes on the change contract: under streaming mode, facts from an uncommitted transaction interleave between stream-start/stop frames and may terminate in abort. The decode fold stages streamed facts keyed by transaction id and releases them only on stream-commit, mirroring the server's own spill behavior.
- Parallel streaming adds out-of-order stream interleaving across transactions; the staging key remains transaction id, and commit order — not arrival order — is the op-log order.
- Emitted-message channel: with `Messages` enabled, application-emitted decoding messages arrive in-stream as their own message type with a transactional flag — an ordered, WAL-durable side-channel for markers (schema epochs, batch boundaries) that travels inside the same cursor as the data it annotates.
