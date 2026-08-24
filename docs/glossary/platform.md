# [PLATFORM_GLOSSARY]

Substrate vocabulary spans telemetry, storage engines, and execution: each term names what a runtime surface does, and the branch doctrine owns how that surface is spelled.

## [01]-[TELEMETRY_MODEL]

- `signal`: Names one of the telemetry data kinds a pipeline carries end to end — metrics, logs, traces, profiles.
    - [NOT]: Signal processing and POSIX signals; only a telemetry data kind carries this word.
- `metric`: Aggregates measurements over time under one instrument name, unit, and attribute set.
- `log`: Records one timestamped event body with severity and attributes, correlated by the active trace context.
- `trace`: Joins the spans of one distributed operation under one trace id.
- `span`: Bounds one timed operation inside a trace, carrying its parent, attributes, status, and events.
- `profile`: Samples a process's own execution — stacks, allocations, locks — against wall or CPU time.
- `semantic convention`: Fixes the attribute and metric names one domain uses, so peers query one vocabulary across runtimes.
- `resource`: Identifies the entity emitting telemetry through immutable attributes — service namespace, service name, instance id.
    - [NOT]: Cloud resources and disposable handles; only emitter identity carries this word.
- `scope`: Identifies the instrumenting library emitting a signal, version-stamped on tracer, meter, and logger alike.
    - [NOT]: Lexical scope and dependency scope; only emitting-library identity carries this word.
- `exemplar`: Attaches one sampled measurement's trace and span ids to an aggregated metric point, wiring metric to trace.
- `temporality`: Declares whether a metric point reports the change since the last export or the running total.
- `delta temporality`: Reports each export's own increment, so a collector or backend performs the summation.
- `cumulative temporality`: Reports the running total since one start time, so a reader differentiates to recover rates.
- `cardinality`: Counts distinct attribute-value combinations one instrument produces, and unbounded cardinality is the metric-store failure mode.

## [02]-[TELEMETRY_TRANSPORT]

- `OTLP`: Frames every telemetry signal as one protobuf or JSON payload family one ingest door accepts.
- `propagation`: Injects and extracts trace context and baggage across a process boundary through registered propagators.
- `baggage`: Carries key-value context alongside trace context, readable by every downstream process.
- `head sampling`: Decides at span start whether a trace records, from the parent decision and a ratio.
- `parent-based sampling`: Adopts an inbound parent's recording decision so no trace fractures across a process boundary.
- `tail sampling`: Decides after a trace assembles whether it ships, from policy over the completed span set.
- `collector`: Receives, processes, and exports telemetry out of process, decoupling every emitter from every backend.
- `receiver`: Admits telemetry into a collector over one protocol and shape.
- `processor`: Transforms telemetry inside a collector pipeline — batching, filtering, attribute promotion, redaction.
    - [NOT]: SDK span processors alone; a collector pipeline stage carries the same word.
- `exporter`: Ships telemetry out of a collector or SDK to one destination protocol.
- `pipeline`: Chains one signal's receivers, processors, and exporters into one ordered path.
- `agent`: Runs a collector beside each workload, owning local buffering and host enrichment.
    - [NOT]: Autonomous software agents; only a per-node collector deployment carries this word.
- `gateway`: Runs a collector as a shared service every workload ships to, owning fleet-wide policy and tail decisions.

## [03]-[TELEMETRY_BACKENDS]

- `observability backend`: Stores and queries one or more telemetry signals behind a query surface a dashboard reads.
- `TSDB`: Stores numeric series keyed by name and label set, indexed for range queries over time.
- `log aggregation`: Indexes log bodies and labels across every emitter so one query spans the fleet.
- `tracing backend`: Stores assembled traces and serves span-graph queries over them.
- `continuous profiling`: Samples production profiles continuously and stores them as queryable series against code identity.

## [04]-[ENGINE_CLASSES]

- `OLTP`: Serves many small transactional reads and writes at low latency under row-oriented storage.
- `OLAP`: Serves few large analytical scans at high throughput under column-oriented storage.
- `HTAP`: Serves transactional and analytical loads over one system without an export hop between them.
- `RDBMS`: Stores relations under a declared schema and answers set-algebraic queries with transactional guarantees.
- `embedded database`: Runs inside the calling process with no server, so the database is a library and its file is the store.
- `extension`: Adds types, functions, index methods, or hooks into an engine's own runtime rather than beside it.
    - [NOT]: File extensions and class extension methods; only engine plugins carry this word.

## [05]-[SCHEMA_AND_CHANGE]

- `schema migration`: Advances a database from one declared shape to the next as an ordered change unit.
- `declarative schema management`: Computes the change plan by diffing a desired schema against the live one, so no change script is hand-written.
- `desired state`: Declares the shape a reconciler drives toward, and the reconciler owns every step reaching it.
- `generation`: Identifies one immutable schema-artifact set by digest, and a digest change replaces that set whole.
    - [NOT]: Garbage-collector generations and code generation; only the schema deployment unit carries this word.
- `DDL`: Declares and alters database structure — tables, types, indexes, policies.
- `DML`: Reads and mutates rows inside an existing structure.
- `RLS`: Filters every row access through a policy predicate the engine evaluates, so a query cannot reach past its tenant.
- `GUC`: Holds one engine session or transaction setting a policy predicate reads under one namespace spelling.
- `partition`: Splits one logical table into physical children by a key so pruning and retention operate per child.
    - [NOT]: Broker partitions and set partitions; only a table's physical child carries this word.

## [06]-[REPLICATION_AND_FLOW]

- `WAL`: Appends every change record before its page write, making crash recovery and streaming replication one mechanism.
- `logical replication`: Streams decoded row changes rather than physical pages, so a subscriber applies them into its own shape.
- `CDC`: Publishes committed row changes as an ordered stream downstream consumers read.
- `dead-letter`: Routes a message no consumer can process onto a separate destination, keeping the primary path draining.
- `topic`: Fans events to subscribed sinks under one named row that also budgets the waiting room it sheds at.
    - [NOT]: Broker topics naming durable partitioned logs; this estate's `Topic` rows fan in process and durability rides the outbox.
- `subscription`: Seats one bounded consumer sink on a topic fan, so one hop owns one waiting room and a gap fold accounts every declined offer.
    - [NOT]: Changefeed subscriptions draining durable store ranges for replay; a bus subscription holds nothing and re-reads nothing after a drop.
- `drop receipt`: Accounts one in-process delivery loss by conservation — a declined-offer span or a shed dispatch — never by interception.
    - [NOT]: Interaction's `DropReceipt` settling a pointer drag-drop effect; a loss row and a drop gesture share a spelling and nothing else.

## [07]-[COLUMNAR_AND_LAKE]

- `columnar`: Stores each column contiguously, so a scan reads only the columns a query names and compresses per type.
- `Parquet`: Encodes columnar data with per-column compression, statistics, and row groups a reader prunes on.
- `Arrow`: Fixes an in-memory columnar layout every process reads without copy or deserialization.
- `Flight SQL`: Serves SQL over the Arrow wire protocol, so results cross as Arrow batches rather than row encodings.
- `data lake`: Stores raw files on object storage under no engine's exclusive control.
- `lakehouse`: Adds table transactions, schema evolution, and as-of reads over lake files, so one store answers analytical queries directly.
- `object store`: Addresses immutable blobs by key over HTTP with no filesystem semantics.

## [08]-[EXECUTION_SUBSTRATE]

- `worker`: Executes admitted work units off a queue, owning nothing about which units arrive.
- `pool`: Bounds a reusable set of expensive handles, and every borrow returns to the same set.
- `executor`: Owns the threads or processes work bodies run on, and a caller selects an executor rather than spawning.
- `scheduler`: Orders admitted work against capacity, dependency, and priority.
- `supervisor`: Owns child lifecycles, restarting or failing a subtree under a declared policy.
- `dispatcher`: Routes one request to its owning handler through data, never through a call-site branch.
- `isolation`: Fixes how far a work body runs from its caller — in-process, thread, process, wasm, remote.
- `crossing`: Names one boundary a value traverses, where shape, ownership, or trust changes.
- `marshalling`: Converts a value into the representation a boundary admits, and back on return.
- `structured concurrency`: Scopes every spawned task to a lexical block that cannot exit before its children settle.
- `backpressure`: Propagates a slow consumer's limit upstream so a producer slows rather than buffering unboundedly.
- `capacity limiter`: Caps concurrent entries into one subsystem at the boundary, making that bound explicit rather than ambient.
- `budget governor`: Throttles one budgeted resource against its own declared ceiling, admitting work while the measured rate stays under it.
- `deadline`: Fixes the absolute time an operation must complete by, propagating to every nested call.
- `cancellation`: Signals that an operation's result is no longer wanted, and every cooperating body unwinds.
- `work descriptor`: Carries a work unit's identity, inputs, cost, and admission case as data a scheduler reads without invoking it.
- `sandbox`: Confines a body's reachable filesystem, network, and syscall surface for the duration of one run.
