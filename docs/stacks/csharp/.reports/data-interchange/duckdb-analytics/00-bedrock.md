# duckdb-analytics — bedrock

## connection and handle law

- One process holds one native database handle per data-source path: `DuckDBConnection` instances over the same path share a refcounted native handle keyed case-insensitively.
- The shared handle is disposed when the last connection over its path closes — buffer pool, temp state, and attached-catalog state evict with it; reopening pays full engine boot.
- An analytical session therefore keeps one anchor connection open for its lifetime; open/close-per-query patterns silently rebuild the engine each time and are the first thing to delete in a slow analytical lane.
- In-memory modality is a two-row axis: `DuckDBConnectionStringBuilder.InMemoryConnectionString` (`DataSource=:memory:`) yields a private engine per connection — two such connections share nothing.
- `InMemorySharedConnectionString` (`DataSource=:memory:?cache=shared`) routes through the shared-handle cache, so multiple connections see one in-memory catalog; the shared form is the only in-memory shape that supports one-writer/N-reader.
- The `?key=value` tail after the data source carries engine configuration into the native open call — boot configuration becomes part of the connection value rather than post-open `SET` ceremony, and the configuration dictionary travels with the connection-string identity.
- `DuckDBConnection.Duplicate()` clones an open connection onto the same native database: the sanctioned shape for concurrent work inside one process, since a streaming read and a concurrent write need separate connections over one handle.
- The connection-string builder's keyword indexer accepts arbitrary engine settings beside `DataSource` — the builder is the typed assembly point for the configuration tail, so profiles construct connection strings from policy values instead of concatenating key-value text.
- `BeginTransaction()` returns the typed transaction scope; the engine permits one writing transaction context per database, so write lanes serialize at the transaction boundary, not in managed code.
- `Cancel()` on the command routes to the native connection's `Interrupt()` — callable from any thread, it aborts the running query on the connection; there is no per-command granularity, so one connection carries at most one cancellable unit of work.
- `GetQueryProgress()` returns a struct of `Percentage`, `RowsProcessed`, `TotalRowsToProcess`; a negative percentage means no progress is available and projects to absence at the rail bridge, never to a fabricated zero.
- Progress rides the engine's progress machinery, which is enabled by default; a profile that disables it also forfeits progress reads — the two are one policy decision.
- `DuckDBException` carries a typed `ErrorType` discriminant beside the message; the boundary conversion maps error classes to fault cases, never message substrings.
- `GetSchema()` exposes metadata collections by name — `Tables`, `Columns`, `ForeignKeys`, `Indexes` — with restriction-value filtering; catalog introspection is a metadata read, not an information-schema string query in caller code.
- The collection names are published constants on the provider, not magic strings — schema reads reference the constant, and a typo'd collection name becomes a compile-visible symbol error instead of a runtime metadata miss.
- The engine's thread default claims every core — an engine embedded beside a host workload declares `threads` from the host's parallelism budget as a consumed budget row, or the two workloads oversubscribe the machine against each other.
- Engine posture is four knobs with engine-derived defaults: `memory_limit` (80 percent of RAM), `threads` (CPU-core count), `temp_directory` (`<database>.tmp`, the spill target for larger-than-memory operators), `max_temp_directory_size` (90 percent of free disk).
- An analytical lane embedded in a host process declares all four explicitly — the defaults are sized for a dedicated machine, and an undeclared `memory_limit` is a latent host-memory incident.
- Spill is silent by design: operators exceeding the memory limit spill to the temp directory and complete slowly instead of failing; a lane that prefers loud failure caps `max_temp_directory_size` low and treats spill as a signal.
- Settings travel three routes — connection-tail configuration for boot-scoped keys, `SET` for session-scoped, attach options for per-catalog — and the profile sorts keys by mutability, so a boot-only key never masquerades as a post-open `SET` that silently fails to apply.
- `preserve_insertion_order` defaults true and is the hidden memory multiplier: large export and ETL pipelines that do not need row order set it false, letting the engine stream reorder-free and cutting peak memory on wide scans and `COPY` jobs.
- Multi-process posture is binary: one process may hold a database file read-write, or any number may hold it read-only — the access mode is a configuration row in the connection-string tail, and a reader fleet over a published analytical file declares read-only access rather than discovering the writer lock at open.

## parameter and transaction law

- `DuckDBParameter` is the typed binding unit — constructed bare, from value, from `(DbType, value)`, or named `(name, [DbType,] value)` — and the collection indexes by both position and name, so positional and named SQL placeholders are one parameter surface, not two binding styles.
- An explicit `DbType` pins the bound logical type when CLR inference would widen or mis-map; value-only binding is acceptable for exact-width CLR types and a defect for `object`-typed pipeline values.
- The transaction object carries `Commit`/`Rollback` and its declared isolation level; transactions group multi-statement mutations, while pure analytical reads run unwrapped — wrapping reads buys nothing and serializes against the write lane.

## appender law

- `CreateAppender` is a three-arity family — `(table)`, `(schema, table)`, `(catalog, schema, table)` — and the catalog arm is load-bearing: it reaches tables in attached stores, so one appender surface serves every catalog the connection sees.
- The appender buffers exactly one native data chunk whose row capacity is the engine's vector size, read from the native library at startup rather than hardcoded; managed code never assumes the quantum.
- `CreateRow` auto-flushes the chunk into the native appender every vector-size rows; `Close()` flushes the tail chunk before sealing — flush points are structural, not caller-visible, and per-row flush calls do not exist.
- The standard engine quantum is on the order of two thousand rows per chunk — small enough that per-chunk failure localization is fine-grained, large enough that managed-to-native transition cost vanishes per row.
- Each row carries the qualified table name into its failure evidence — appender errors cite catalog, schema, and table without caller-side context threading.
- Error timing is the defining trap: `AppendValue` writes into managed vector writers and throws only on managed type mismatch; constraint violations and engine-side failures surface at chunk-append boundaries and at `Close()`.
- A primary-key collision in row three may not throw until thousands of rows later; the rail treats `Close()` as the commit point and wraps it explicitly in the exception-capture grammar.
- `Dispose()` calls `Close()` when not already closed — relying on disposal alone puts a throwing flush inside `using` unwind; the law is explicit `Close()` inside the rail with disposal as backstop.
- `Clear()` is the abort lane: it discards all pending rows native-side and resets the managed chunk without closing the appender — the only way to back out of a partially appended batch and continue on the same appender.
- `IDuckDBAppenderRow` is one fluent surface returning itself per append; `EndRow()` seals the row, and a row with the wrong append count fails at the seal, localizing arity errors to the row.
- Typed `AppendValue` overloads cover the full primitive lattice: all integer widths signed and unsigned, `float`/`double`/`decimal`, `bool`, `string`, `Guid`, `BigInteger`, enums via a generic arm, `Span<byte>`/`byte[]` blobs.
- Temporal appends accept `DateOnly`/`TimeOnly`, `DateTime`/`DateTimeOffset`/`TimeSpan`, and the provider's date/time structs — temporal admission happens here once; interior code never formats temporals for the store.
- `AppendValue(IEnumerable<T>)` writes LIST columns directly — nested collection ingest without row explosion or string encoding.
- `AppendNullValue()` writes SQL NULL; `AppendDefault()` writes the column's declared DEFAULT and is the only way to exercise column defaults through the bulk lane — omitting a column is not an option on the appender.
- The mapped appender lifts column order out of call sites: `CreateAppender<T, TMap>` with a `DuckDBAppenderMap<T>` subclass declaring ordered `Map(getter)`, `DefaultValue()`, and `NullValue()` rows per column.
- The map is validated at appender construction — zero mappings or a mapping/column count mismatch throws before any row moves, making shape drift between the record and the table a construction-time failure.
- Vector writers are typed per column from the table's logical types at chunk initialization — numeric, string, decimal, guid, interval, enum, and list columns each get a dedicated writer — so append cost is a typed vector store, never boxing through a generic object lane.

## streaming-result law

- `DuckDBCommand.UseStreamingMode` is the one switch between two execution regimes: off executes to a fully materialized result the engine holds whole; on executes through the streaming pipeline and the result yields chunks lazily.
- The reader self-discriminates: it asks the native result whether it streams and pulls chunks through the streaming fetch or by materialized chunk index accordingly — consuming code is identical across regimes, so the flag is pure policy.
- Memory posture: a materialized result holds the entire result set in the native result handle for its lifetime; a streaming result holds one vector-size chunk at a time, with the plan executing incrementally as the reader advances.
- Streaming is the default posture for any result not known to be small; materialized is correct only when the result is re-walked or small enough to be cheaper than incremental execution setup.
- Streaming shifts error timing forward: a failure deep in the plan surfaces during `Read()` mid-iteration, not at `ExecuteReader()` — the consuming fold carries the failure rail through enumeration, not just around the execute call.
- Multi-statement command text prepares and executes each statement in sequence, each yielding its own result under the same streaming flag; `NextResult()` walks them.
- The multi-statement pipeline is itself lazy — later statements execute as their results are reached, so an error in statement three of a batch surfaces at the `NextResult()` that reaches it, extending the error-late discipline across the batch.
- `RecordsAffected` is always `-1` on the reader; mutation counts come from `ExecuteNonQuery`, never from reader metadata.
- `Prepare()` materializes the prepared statement for re-execution with rebound parameters — repeated parameterized reads amortize planning through the prepared lane, not through SQL string caching.
- Column access projects through typed vector readers at chunk-relative offsets: `GetFieldValue<T>` is the typed lane, `GetValue` boxes, and provider-specific values (intervals, out-of-CLR-range decimals) surface through `GetProviderSpecificValue`.
- Null checks ride the vector validity mask via `IsDBNull`; reading a value without the mask check observes vector garbage semantics, not a managed null.
- Nested logical types project on read through dedicated vector readers — list, struct, and map columns arrive as typed managed values, so document-shaped query results cross the boundary without string re-parsing.
- `GetStream(ordinal)` exposes blob columns as streams — large binary values drain incrementally instead of materializing as arrays.
- The streaming flag defaults off — an unconfigured command is materialized, which means the default posture is wrong for large results and the analytical profile sets the flag as policy, not per query.
- A streaming result occupies its connection until drained or disposed; concurrent work during a long drain runs on a `Duplicate()` connection, never by interleaving commands on the streaming one.

## parquet movement

- Export is one `COPY (SELECT ...) TO '<path>' (FORMAT parquet, ...)` statement whose option rows form the whole policy surface; there is no second export API to keep consistent.
- Compression is a policy row: `zstd` (with `COMPRESSION_LEVEL`), `snappy` (default), `gzip`, `brotli`, `lz4`, `lz4_raw`, `uncompressed` — codec choice composes orthogonally with row-group sizing and partitioning.
- Row-group geometry is two rows: `ROW_GROUP_SIZE` (122880 rows default) and `ROW_GROUP_SIZE_BYTES`; multi-file splitting adds `ROW_GROUPS_PER_FILE` and `FILE_SIZE_BYTES`.
- Row-group size is the unit of both scan parallelism and filter skipping: groups near the default row count parallelize and zonemap-prune well; tiny groups are the signature defect of append-per-batch exporters — batch through a staging table and export once.
- `PARTITION_BY` writes hive-layout trees with `FILENAME_PATTERN` naming the leaves; partition keys move into the directory structure and out of the file payload.
- Destination collision is a closed policy row: `OVERWRITE`, `OVERWRITE_OR_IGNORE`, or `APPEND` — append-to-directory creates new files beside existing generations rather than mutating them.
- `KV_METADATA` writes caller key-value pairs into the footer; `FIELD_IDS` pins stable field identity across renames; `PARQUET_VERSION` selects the newer encoding generation for consumers that read it.
- Ingest is the `read_parquet` table function over one path, a glob, or a list of paths; bare `FROM '<path>.parquet'` sugar resolves to the same function.
- File enumeration is orthogonal to every read option — a glob, a list, and a single path compose identically with name alignment, partition lifting, and provenance columns — so growing from one file to a generation directory changes the path argument and nothing else.
- `union_by_name` aligns heterogeneous file generations by column name instead of position; `hive_partitioning` lifts directory keys into columns; `filename` and `file_row_number` provide per-row provenance.
- `binary_as_string` repairs foreign writers that drop the UTF8 flag; `encryption_config` covers encrypted footers; a `schema` map keyed by field id renames and retypes at scan time without rewriting files.
- Projection pushdown and zonemap filter pushdown are automatic — only referenced columns decode, and row groups failing predicates skip; selective queries over parquet approach index-scan economics with zero index maintenance.
- The footer is queryable without decoding data: `parquet_schema` (declared shape), `parquet_metadata` (per-row-group statistics), `parquet_file_metadata` (file level), `parquet_kv_metadata` (caller stamps) — the engine-side descriptor-read surface artifact-identity law composes.
- `FORMAT` is one axis on one statement: the same `COPY ... TO` emits parquet, delimited, or JSON by format row — JSON output adds an `ARRAY` row selecting array-of-records versus newline-delimited — so every egress format shares the destination, collision, and compression vocabulary instead of owning a private export path.

## attach and the analytical-read-only boundary

- `ATTACH '<path>' AS <alias> (READ_ONLY)` mounts a second catalog under the alias, defaulting to the file stem; names resolve fully qualified as `catalog.schema.table`.
- `IF NOT EXISTS` and `OR REPLACE` make mounting idempotent; `USE` moves the default catalog; `DETACH` closes the catalog and releases its file lock.
- Remote HTTP and object-store attachments are read-only by construction; local files take the `READ_ONLY` flag explicitly, and the analytical profile always supplies it for stores it does not own.
- One transaction writes to at most one attached database — cross-catalog work is read-from-many, write-to-one by engine law, which already enforces the analytical topology without managed coordination.
- Foreign-engine stores attach through `TYPE` rows (`TYPE sqlite` for embedded operational stores; server engines via their scanner extensions), giving the analytical lane direct SQL over operational data with no export hop.
- Extension reality gates offline hosts: `autoinstall_known_extensions` and `autoload_known_extensions` both default true, so first use of a scanner or format extension downloads from the extension repository at runtime.
- A sealed or air-gapped process pre-installs extensions into the extension directory and declares the autoinstall pair as policy rows — an ambient-network dependency inside an analytical query is the failure that only fires in production.
- The read-only boundary is one law: the analytical engine is a projection surface — operational stores enter read-only via attach or admission via file ingest, results leave as artifacts or typed reads, and the engine's own database file is a rebuildable derived artifact, never a system of record.
- Attach is also the schema-inspection lane: attached catalogs answer the same metadata-collection reads as the primary, so a pre-flight gate over a foreign store — expected tables, expected columns — is a metadata query against the alias before any data moves.
- The ephemeral-compute topology composes the two modalities: an in-memory primary catalog with durable stores attached read-only computes entirely in scratch and writes only artifacts — process exit leaves no engine state to clean, and the durable stores were never writable from the analytical lane at all.

## user-defined function surface

- Scalar UDFs register on the connection: `RegisterScalarFunction<T..., TResult>` up to four typed parameters, plus a variadic flag on the single-parameter arm for params-shaped functions.
- `ScalarFunctionOptions` declares `IsPureFunction` (planner may fold and cache) and `HandlesNulls` (the function sees nulls instead of null-propagation skipping it) — purity and null posture are declared, not inferred.
- Table UDFs register via `RegisterTableFunction<T...>` up to eight typed parameters; the bind callback returns a `TableFunction(columns, data, cardinality)` record, and `CardinalityHint(Value, IsExact)` feeds the join planner — an exact hint changes plans, a missing hint degrades them.
- Beyond eight generic parameters, a params-array arm takes explicit provider types per parameter — the typed-generic family and the type-array arm are one registration surface with two declaration spellings, not two capabilities.
- A `TableFunction`'s data is a managed `IEnumerable` pulled by the engine — managed enumeration becomes a relation, so any in-process sequence joins, filters, and aggregates under full SQL without staging.
- Both directions speak the vector protocol: `IDuckDBDataReader.GetValue`/`IsValid` reads arguments per row against the validity mask; `IDuckDBDataWriter.WriteValue`/`WriteNull` emits results.
- UDF bodies are vector kernels invoked with a row count, not per-row delegates — the managed-transition cost amortizes across the chunk, and per-row managed callbacks are the rejected shape.
- Exceptions thrown inside UDF callbacks are captured across the native boundary and resurface as query errors on the executing statement — a UDF fault is a query fault, not a process fault.

## divergent — appender-streaming

- Ingress and egress are one symmetric chunk algebra: bulk in moves vector-size chunks through the appender, bulk out moves vector-size chunks through the streaming reader — the same quantum both directions.
- A unified movement law budgets in chunks, not rows: peak managed memory is one chunk wide regardless of batch size, and throughput tuning is chunk-boundary work (column width, list nesting) rather than row-loop work.
- Backpressure is pull-shaped on both lanes: the streaming reader executes the plan only as fast as `Read()` advances, and the appender flushes only as `CreateRow` crosses chunk boundaries — neither lane buffers ahead.
- A slow consumer or producer therefore throttles the engine for free; explicit drop or queue policy is needed only when bridging to push-shaped sources, and that policy lives in the lane vocabulary, not here.
- The cancellation-and-abort taxonomy has three verbs with distinct scopes: `Interrupt` (connection-scoped, aborts the running query, surfaces on the blocked call), `Clear` (appender-scoped, discards buffered rows, lane stays usable), `Close` (commit-or-throw).
- A bulk-lane rail maps each verb to its own receipt — interrupted reads and cleared batches are evidence-bearing outcomes, never silent unwinds.
- Error-late is the unifying failure mode across both lanes: appender constraint failures defer to flush and close; streaming plan failures defer to mid-drain reads.
- The composed law is commit-point explicitness: every bulk movement names its commit point — the `Close`, the final `Read` — and converts there, so deferred failures land in the typed rail instead of escaping from disposal or enumeration glue.
- The provider has four asynchrony seams — negative progress, error-late streaming reads, close-time constraint surfacing, interrupt exceptions — and each maps to its own typed outcome; one catch-all over the lane collapses evidence the receipts need.
- Rejected: row-at-a-time parameterized `INSERT` loops — forecloses chunk amortization; orders of magnitude slower than the appender for bulk.
- Rejected: `DataTable`/adapter materialization — forecloses streaming and double-buffers every value boxed.
- Rejected: materialized `ExecuteReader` for large exports — forecloses incremental execution and spill-free drains.
- Rejected: hand-batched multi-row `INSERT ... VALUES` text — forecloses typed binding and re-parses SQL per batch.
- The table-UDF arm closes the loop from the other side: where the appender pushes managed rows into a table, a registered table function makes a managed sequence queryable in place — push when the data must persist, expose when it must only participate in one query; staging tables for single-query joins are the foreclosed middle.
- Quantitative thresholds for lane choice: vector-size-order row counts and below fit a single chunk and any lane is fine; thousands to millions of rows demand the appender or the streaming reader; results that must be re-walked demand materialization or a local table — the decision is row-count order of magnitude versus re-walk need, two questions, no benchmarks required.

## divergent — parquet-attach-shred

- The whole lane is one movement algebra with three policy axes: source (any attached catalog or readable file), transform (one SQL projection), destination (one `COPY ... TO` with format, compression, and partition rows).
- Every flow — operational-store snapshot to parquet, parquet generations to a merged view, JSON feed to typed columns — is one instance of attach/read, project, copy; a new flow is a row in each axis, never a new pipeline shape.
- JSON shredding is admission, not manipulation: `read_json` auto-detects layout (array, newline-delimited, unstructured) and record-ness, and samples for schema inference under a `sample_size` budget (20480 objects default) and a `maximum_object_size` cap (16 MiB default — oversized documents reject, they do not truncate).
- Inference is a development-time tool whose output is frozen: the inferred shape is pinned by passing an explicit `columns` struct, because sampling drift on sparse fields silently retypes columns between runs — an unpinned `read_json` in a contract path is the defect.
- `ignore_errors` applies only to newline-delimited input, making NDJSON the only fault-tolerant JSON ingest layout; array-layout feeds fail whole on one bad element.
- Document-to-relational collapse happens inside the engine vocabulary: `->`/`->>` extraction operators, `json_structure` for shape inference, `json_transform` for typed projection against a declared structure, `unnest` for exploding arrays into rows.
- Shredding is a SQL projection over admitted JSON — managed-side DOM walking followed by re-insert re-implements the engine's own lane at a fraction of its speed and is the rejected spelling.
- Shred-at-read versus store-shredded is a declared choice: a JSON-typed column keeps documents queryable in place for evolving shapes, while shredding to typed columns at admission buys columnar scan economics — the deciding question is whether the document shape is still moving.
- Field-id discipline closes the rename loop: stamping stable field identities at export and mapping by field id at scan makes column renames non-breaking across artifact generations — name-keyed reading absorbs additions, id-keyed mapping absorbs renames, and together they cover the whole additive-evolution surface.
- `COPY ... TO` output is a filesystem effect outside transaction rollback — an aborted transaction does not unwrite the file — so artifact publication composes the atomic-write protocol (temp path, rename) rather than trusting transactional cleanup.
- Artifact evolution across generations is absorbed at read time: `union_by_name` makes additive columns compatible by construction (absent columns read NULL), hive partition keys arrive as columns, and `filename` provenance pins every row to its generation.
- A directory of artifact generations is therefore one queryable relation; schema growth never invalidates old files, and compaction is a new artifact written beside the old, never an in-place merge.
- The embedded-attach pattern composes the durability-owned store without copying: attach the operational file read-only, project hot rows into the analytical catalog or directly to parquet, detach.
- The foreclosed form is the export-import shuffle — dumping the operational store to an intermediate format and re-ingesting — which doubles I/O and loses type fidelity at the intermediate hop.
- The analytical catalog itself participates in the artifact economy: its database file is a single-file, attachable, read-only-mountable artifact — a computed mart is built once, fenced, and shipped to consumers who attach it read-only.
- The same one-transaction-one-writer law guarantees mart producers and consumers never contend; the mart file plus its stamp and hash is a complete, self-describing deliverable.
- Partition design has a working band: partition keys with cardinality in the tens to low thousands prune effectively; higher-cardinality keys explode file counts and metadata cost, and the corrective is bucketing the key or demoting it from path to column — partitioning is a pruning instrument, never a uniqueness scheme.
- Footer-stamp reads compose the attach law for verification-only flows: a consumer can gate a parquet delivery — descriptor present, stamp matches expectation — entirely through footer functions before granting the artifact a read path, making artifact admission a metadata-cost operation.
