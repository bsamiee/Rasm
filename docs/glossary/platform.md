# [PLATFORM_GLOSSARY]

Substrate vocabulary spans telemetry, storage engines, and execution: each term names what a runtime surface does, and the branch doctrine owns how that surface is spelled.

## [01]-[TELEMETRY_MODEL]

- `signal`: names one of the telemetry data kinds a pipeline carries end to end — metrics, logs, traces, profiles.
    - [NOT]: signal processing and POSIX signals; only a telemetry data kind carries this word.
- `metric`: aggregates measurements over time under one instrument name, unit, and attribute set.
- `log`: records one timestamped event body with severity and attributes, correlated by the active trace context.
- `trace`: joins the spans of one distributed operation under one trace id.
- `span`: bounds one timed operation inside a trace, carrying its parent, attributes, status, and events.
- `profile`: samples a process's own execution — stacks, allocations, locks — against wall or CPU time.
- `semantic convention`: fixes the attribute and metric names one domain uses, so peers query one vocabulary across runtimes.
- `resource`: identifies the entity emitting telemetry through immutable attributes — service namespace, service name, instance id.
    - [NOT]: cloud resources and disposable handles; only emitter identity carries this word.
- `scope`: identifies the instrumenting library emitting a signal, version-stamped on tracer, meter, and logger alike.
    - [NOT]: lexical scope and dependency scope; only emitting-library identity carries this word.
- `exemplar`: attaches one sampled measurement's trace and span ids to an aggregated metric point, wiring metric to trace.
- `temporality`: declares whether a metric point reports the change since the last export or the running total.
- `delta temporality`: reports each export's own increment, so a collector or backend performs the summation.
- `cumulative temporality`: reports the running total since one start time, so a reader differentiates to recover rates.
- `cardinality`: counts distinct attribute-value combinations one instrument produces, and unbounded cardinality is the metric-store failure mode.

## [02]-[TELEMETRY_TRANSPORT]

- `OTLP`: frames every telemetry signal as one protobuf or JSON payload family one ingest door accepts.
- `propagation`: injects and extracts trace context and baggage across a process boundary through registered propagators.
- `baggage`: carries key-value context alongside trace context, readable by every downstream process.
- `head sampling`: decides at span start whether a trace records, from the parent decision and a ratio.
- `parent-based sampling`: adopts an inbound parent's recording decision so no trace fractures across a process boundary.
- `tail sampling`: decides after a trace assembles whether it ships, from policy over the completed span set.
- `collector`: receives, processes, and exports telemetry out of process, decoupling every emitter from every backend.
- `receiver`: admits telemetry into a collector over one protocol and shape.
- `processor`: transforms telemetry inside a collector pipeline — batching, filtering, attribute promotion, redaction.
    - [NOT]: SDK span processors alone; a collector pipeline stage carries the same word.
- `exporter`: ships telemetry out of a collector or SDK to one destination protocol.
- `pipeline`: chains one signal's receivers, processors, and exporters into one ordered path.
- `agent`: runs a collector beside each workload, owning local buffering and host enrichment.
    - [NOT]: autonomous software agents; only a per-node collector deployment carries this word.
- `gateway`: runs a collector as a shared service every workload ships to, owning fleet-wide policy and tail decisions.

## [03]-[TELEMETRY_BACKENDS]

- `observability backend`: stores and queries one or more telemetry signals behind a query surface a dashboard reads.
- `TSDB`: stores numeric series keyed by name and label set, indexed for range queries over time.
- `log aggregation`: indexes log bodies and labels across every emitter so one query spans the fleet.
- `tracing backend`: stores assembled traces and serves span-graph queries over them.
- `continuous profiling`: samples production profiles continuously and stores them as queryable series against code identity.

## [04]-[ENGINE_CLASSES]

- `OLTP`: serves many small transactional reads and writes at low latency under row-oriented storage.
- `OLAP`: serves few large analytical scans at high throughput under column-oriented storage.
- `HTAP`: serves transactional and analytical loads over one system without an export hop between them.
- `RDBMS`: stores relations under a declared schema and answers set-algebraic queries with transactional guarantees.
- `embedded database`: runs inside the calling process with no server, so the database is a library and its file is the store.
- `extension`: adds types, functions, index methods, or hooks into an engine's own runtime rather than beside it.
    - [NOT]: file extensions and class extension methods; only engine plugins carry this word.

## [05]-[SCHEMA_AND_CHANGE]

- `schema migration`: advances a database from one declared shape to the next as an ordered change unit.
- `declarative schema management`: computes the change plan by diffing a desired schema against the live one, so no change script is hand-written.
- `desired state`: declares the shape a reconciler drives toward, and the reconciler owns every step reaching it.
- `generation`: identifies one immutable schema-artifact set by digest, and a digest change replaces that set whole.
    - [NOT]: garbage-collector generations and code generation; only the schema deployment unit carries this word.
- `DDL`: declares and alters database structure — tables, types, indexes, policies.
- `DML`: reads and mutates rows inside an existing structure.
- `RLS`: filters every row access through a policy predicate the engine evaluates, so a query cannot reach past its tenant.
- `GUC`: holds one engine session or transaction setting a policy predicate reads under one namespace spelling.
- `partition`: splits one logical table into physical children by a key so pruning and retention operate per child.
    - [NOT]: broker partitions and set partitions; only a table's physical child carries this word.

## [06]-[REPLICATION_AND_FLOW]

- `WAL`: appends every change record before its page write, making crash recovery and streaming replication one mechanism.
- `logical replication`: streams decoded row changes rather than physical pages, so a subscriber applies them into its own shape.
- `CDC`: publishes committed row changes as an ordered stream downstream consumers read.
- `dead-letter`: routes a message no consumer can process onto a separate destination, keeping the primary path draining.

## [07]-[COLUMNAR_AND_LAKE]

- `columnar`: stores each column contiguously, so a scan reads only the columns a query names and compresses per type.
- `Parquet`: encodes columnar data with per-column compression, statistics, and row groups a reader prunes on.
- `Arrow`: fixes an in-memory columnar layout every process reads without copy or deserialization.
- `Flight SQL`: serves SQL over the Arrow wire protocol, so results cross as Arrow batches rather than row encodings.
- `data lake`: stores raw files on object storage under no engine's exclusive control.
- `lakehouse`: adds table transactions, schema evolution, and as-of reads over lake files, so one store answers analytical queries directly.
- `object store`: addresses immutable blobs by key over HTTP with no filesystem semantics.

## [08]-[EXECUTION_SUBSTRATE]

- `worker`: executes admitted work units off a queue, owning nothing about which units arrive.
- `pool`: bounds a reusable set of expensive handles, and every borrow returns to the same set.
- `executor`: owns the threads or processes work bodies run on, and a caller selects an executor rather than spawning.
- `scheduler`: orders admitted work against capacity, dependency, and priority.
- `supervisor`: owns child lifecycles, restarting or failing a subtree under a declared policy.
- `dispatcher`: routes one request to its owning handler through data, never through a call-site branch.
- `isolation`: fixes how far a work body runs from its caller — in-process, thread, process, wasm, remote.
- `crossing`: names one boundary a value traverses, where shape, ownership, or trust changes.
- `marshalling`: converts a value into the representation a boundary admits, and back on return.
- `structured concurrency`: scopes every spawned task to a lexical block that cannot exit before its children settle.
- `backpressure`: propagates a slow consumer's limit upstream so a producer slows rather than buffering unboundedly.
- `capacity limiter`: caps concurrent entries into one subsystem at the boundary, making that bound explicit rather than ambient.
- `deadline`: fixes the absolute time an operation must complete by, propagating to every nested call.
- `cancellation`: signals that an operation's result is no longer wanted, and every cooperating body unwinds.
- `work descriptor`: carries a work unit's identity, inputs, cost, and admission case as data a scheduler reads without invoking it.
- `sandbox`: confines a body's reachable filesystem, network, and syscall surface for the duration of one run.
