# [PERSISTENCE_TASKLOG]

Open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each task carries a status marker, thesis, capability, shape, unlocks, anchors, and optional tension; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0003]-[QUEUED]: Egress context carriers — NATS and CloudEvents legs continue the envelope trace the Kafka leg already carries.
- Capability: the `Nats` sink injects `traceparent` and baggage onto `NatsHeaders` beside `Nats-Msg-Id`; the webhook, Pulsar, and wire-native legs stamp the CloudEvents `traceparent`/`tracestate` extension attributes — every delivery joins the drain trace, never only the Kafka leg.
- Shape: carrier rows on `Version/egress.md#EGRESS_SINK` delegating to the AppHost `TraceContext` adapter family; the dedup-honesty column stays untouched.
- Unlocks: broker-hop trace continuity for the spine sink and CDC lanes; delivery evidence joins outbox drain spans across every sink case.
- Anchors: the `Version/egress.md` sink table, AppHost `telemetry.md#CORRELATION_SPINE` carrier growth row, `CloudNative.CloudEvents` extension attributes.
- Ripple: `Rasm.AppHost` `[WIRE_CARRIER_ADAPTERS]`.

[0004]-[QUEUED]: Flight SQL result plane — the federation producer speaks Flight SQL so ADBC-native consumers redeem without a bespoke descriptor contract.
- Capability: `FederationFlight` gains the `FlightSqlServer` arm — statement execution, prepared statements, and the catalog/schema metadata verbs over the same listener; the plain-Flight `GetFlightInfo`/`DoGet` ticket lane stands unchanged.
- Shape: one server arm on `Query/federation.md#FLIGHT_RESULT_PLANE` — `FlightSqlServer` beside the existing `FlightServer` subclass, Substrait plan bytes as the statement payload, results streaming through the held `ReplayKey` ticket registry; slots extend `store.federation.flight.*`.
- Unlocks: `python:data` `RemoteDriver.FLIGHTSQL` consumes the C# result plane directly through `adbc-driver-flightsql` — one cross-runtime analytics wire, no custom client.
- Anchors: the `api-arrow.md` Flight SQL surface (`FlightSqlClient` `ExecuteAsync`/`PrepareAsync` and the metadata verb family), the landed `FederationFlight` ticket registry, `FlowtideDotNet.Substrait` plan ingress.

[0005]-[QUEUED]: Partitioned dataset scan — `ParquetSharp.Dataset` reads multi-file hive-partitioned lake layouts as one Arrow stream.
- Capability: dataset scan with partition pruning and schema unification over directory datasets — the lake-scan counterpart to the single-file `ParquetSharp.Arrow` codec lane, batches feeding the same egress and query lanes.
- Shape: one scan row beside `Query/columnar.md#FLAT_TABLE_EGRESS` — `DatasetReader` over `HivePartitioning`/`IPartitioningFactory`, filter pushdown through `Col` and `FilterExtensions`, egress through `ToBatches()` yielding `IArrowArrayStream`; slot `store.columnar.scan` under the grammar.
- Unlocks: lake-resident history — Parquet daemon materializations and Delta-log generations queryable without a DuckDB mount in the loop.
- Anchors: the `api-parquetsharp.md` dataset surface, the async Parquet daemon materialization, the `store.columnar.*` slot roster.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `Store/observability.md` landed — slot grammar and registry, `pg_stat_statements`/`pg_stat_io` harvest, DuckDB profiling harvest, SQLite status harvest; `OpenTelemetry.Instrumentation.ConfluentKafka` admitted with csproj row, README registry row, and `.api` catalog.
- [0002]-[COMPLETE]: every emitting page carries its `Slots` roster on its primary owner and `SlotRegistry.Mounted()` spreads the census; the topology traversal slot collapsed to `store.topology.traverse` and the vector-route fact respelled `store.vector.route` under the grammar.
