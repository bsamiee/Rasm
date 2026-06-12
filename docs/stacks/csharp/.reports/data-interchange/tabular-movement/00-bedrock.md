# tabular-movement — bedrock

## separator and options law

- The separator is a value, not a parse loop: `Sep.New(char)` pins it; `Sep.Auto` (a null `Sep?`) defers to detection.
- The default separator is the semicolon, not the comma — an options record built without an explicit separator or auto-detection writes and expects semicolons, so comma exchange always states `Sep.New(',')` or rides detection; assuming comma-by-default is the first interop defect.
- The separator is one char by construction — multi-character delimiters are outside the lane's vocabulary, and a source using one is a foreign format admitted by other means, not a separator row.
- Detection is a quote-aware count of candidate separators (`;`, `\t`, `|`, `,`) over the first row only, stopping at the first newline — a separator first appearing past row one is invisible to it, and a single-column file with no candidate hits parses as one column.
- Contract-bearing readers pin the separator and reserve `Auto` for exploratory ingest; auto-detection inside a declared exchange profile is the drift vector.
- Reader policy is one immutable options record (`SepReaderOptions`, a readonly record struct built by `Sep.Reader(o => o with { ... })`) — every profile is a declarable policy value, and per-call-site option drift is the rejected form.
- The reader option rows: separator, culture (invariant default), `HasHeader` (true default), `ColNameComparer` (ordinal default), the string-materialization factory, `DisableFastFloat`, `DisableColCountCheck`, `DisableQuotesParsing`, `Unescape`, `Trim`, `InitialBufferLength`, `AsyncContinueOnCapturedContext`.
- The invariant-culture default is the round-trip guarantee: values written by `Format` under the writer's invariant default re-parse bit-equal under the reader's — introducing a culture row on one side without the other breaks numeric and temporal round-trips silently.
- Ordinal name comparison makes header lookup case-sensitive by default; case-insensitive exchange declares the comparer row explicitly rather than normalizing header strings in caller code.
- The comparer row governs every name lookup — header index resolution, named indexers, try-lookups — so comparer expectations cannot diverge across access paths within one reader.
- `Strict()` is the matched hardening pair: on the reader it forces quotes parsing on and `Unescape` true; on the writer it forces `Escape` true.
- Reader-strict without writer-strict (or the reverse) is the asymmetry that produces files one side cannot round-trip — the two are declared together as one exchange profile.
- Column-count checking defaults on: a row whose column count deviates from the first row throws, making ragged input a loud failure; `DisableColCountCheck` is the explicit ragged-source opt-in and shifts missing-column handling onto access-time `TryGet`.
- `InitialBufferLength` rounds up to a power of two and is performance-tuned at its default; enlarging it speculatively wastes memory without measurable gain — it moves only on measured evidence.
- Source admission is one factory family: `FromText`/`FromFile`/`From(buffer | stream | reader[, leaveOpen])`, each with an `...Async` mirror taking `CancellationToken` and returning a `ValueTask`-wrapped reader.
- The named-source arms (`From(name, nameToStream | nameToReader)`) carry a logical name through the reader for evidence — failure receipts cite the source name without the caller threading it separately.
- Source and sink ownership is explicit: every stream and writer arm has a `leaveOpen` form, and the default takes ownership and closes on dispose — a shared stream passed without `leaveOpen` is closed out from under its other consumers.
- `AsyncContinueOnCapturedContext` forwards to `ConfigureAwait` across all async members — context posture is an options row, not a call-site suffix repeated per await.
- `SepSpec` (separator, culture, context posture) is exposed by both reader and writer via `Spec` — a writer opened from a reader's spec inherits the exchange profile structurally, which is how round-trip flows guarantee profile symmetry.
- A captured spec opens new lanes directly — `spec.Reader()` and `spec.Writer()` — so profile reuse is a value pass between flows, never a re-declaration of the option rows.
- In-memory emission sizes its sink up front via the capacity arm — repeated small text artifacts avoid growth reallocation by declaring expected size from the artifact class.

## ref-struct row law

- `SepReader` is its own enumerator; `Row`, `Col`, and `Cols` are `readonly ref struct` projections over the reader's internal char buffer — they cannot escape the enumeration scope, cannot be captured or stored, and every escape attempt is a compile-time break.
- Projection delegates (`RowFunc<T>`, `RowTryFunc<T>`, `ColFunc<T>`) declare `allows ref struct`, so even the projected value may stay stack-confined through the projection.
- Reader state is queryable before iteration: `IsEmpty`, `HasRows`, `HasHeader` — empty-input handling is a property read, not a try-iterate-catch.
- `Row` carries position evidence beyond the data: `RowIndex`, plus `LineNumberFrom`/`LineNumberToExcl` — a half-open physical-line range, because a quoted column may span newlines and one logical row can occupy many lines.
- Error receipts cite the line range, never the row index alone; the row index diverges from physical lines exactly when quoted multi-line values appear.
- The row indexer is one polymorphic surface: `row[int]`, `row[Index]`, `row[string]`, `row[Range]`, `row[int[]]`/`row[ReadOnlySpan<int>]`, `row[string[]]`/`row[ReadOnlySpan<string>]` — single column, named column, contiguous slice, and arbitrary projection all discriminate on the indexer argument shape.
- `TryGet(name, out col)` is the absence-tolerant arm for columns that may not exist under a ragged or evolving header.
- `Row.Span` exposes the whole raw row as chars — the pass-through and raw-evidence lane — with the in-place mutation caveat owned below.
- `Col.Span` exposes the raw column chars; `Col.Parse<T>()` parses any `ISpanParsable<T>` directly from the span — one generic lane covers every numeric, temporal, and custom span-parsable type with zero string allocation.
- `TryParse<T>()` (nullable-struct return) and `TryParse<T>(out T)` are the rail-friendly arms; parse failure is typed absence at the column, not an exception in the loop.
- Float and double parsing routes through a specialized fast-float path unless `DisableFastFloat` — disabling exists only for bit-exact compatibility with legacy culture-specific parses.
- `Cols` is the column-set algebra: `Parse<T>()` returning a reader-pooled `Span<T>`, `Parse<T>(Span<T>)` into caller memory, `ParseToArray<T>()`, `TryParse<T>()` span-of-nullable, and `TryParse<T>(Span<T?>)`.
- Reader-pooled parse output is valid until the next row advance — a pooled `Span<T>` held across rows reads the next row's values; results that outlive the row go through the caller-memory or array arms.
- `Cols.Select(ColFunc<T>)` projects arbitrary per-column transforms, with an unmanaged function-pointer overload for hot columns where delegate indirection shows in profiles.
- `Cols.Join(separator)`/`JoinToString` re-join column sets — sub-row re-emission without writer machinery.
- `Cols.ToStrings()`/`ToStringsArray()` materialize a column set through the same pooled policy as single columns — bulk string egress stays inside the one span-to-string seam.
- A whole numeric block lifts in one `Parse<T>()` call over a range indexer; per-column loops over wide rows are the rejected spelling.
- Header resolution is index-first: `Header.IndexOf`/`TryIndexOf` accept string or span keys; `IndicesOf` (array, params-span, or into a caller span) resolves a name set to indices once, outside the row loop; `NamesStartingWith` handles prefix families.
- Per-row name lookup inside the loop re-pays hashing per row — resolve indices once, index by position thereafter; `SepReaderHeader.Empty` and `IsEmpty` make headerless sources explicit rather than null-bearing.
- `Row.UnsafeToStringDelegate` exposes a `Func<int, string>` over the current row for foreign APIs demanding delegate-shaped column access — an escape hatch valid only within the row's scope, named unsafe because the delegate must not outlive the row it views.

## unescape, trim, and the in-place mutation edge

- Unescape semantics are exact: when a column starts with a quote, the two outermost quotes are removed and every second inner quote collapses (`""` to `"`).
- Unescaping mutates the buffer in place — after accessing an unescaped column, `Row.Span` contains garbage between columns; individual `Col.Span` values remain correct.
- The ordering law: row-level span reads (raw-row logging, raw re-emission) happen before any column access when unescape is on, or the row is materialized first — violating it produces corrupted evidence, not a crash.
- With quotes parsed but `Unescape` off, quoted column spans retain their quotes verbatim — raw-value comparison against unquoted expectations fails on exactly the quoted subset, which is why strict profiles pair parsing with unescaping instead of leaving the default.
- `Trim` is a two-phase flags axis, not a boolean: `Outer` trims ASCII spaces outside quotes before unescaping, `AfterUnescape` trims inside after unescaping, `All` is both.
- Trimming removes ASCII space only — tabs and exotic whitespace survive every trim phase, so tab-padded values pass through verbatim and a tab separator never collides with its own trim policy.
- Unescaping triggers only on a leading quote — a quote appearing mid-value passes through verbatim even with unescape on, so partially quoted foreign data normalizes at the value gate, not in the parser.
- The phase choice decides whether `" padded "` survives quoting — outer-only preserves quoted padding, after-unescape strips it; the exchange profile states which.
- Trimming may also mutate in place when combined with unescape, with the same row-span caveat — the two options share one buffer discipline.
- `DisableQuotesParsing` turns quote handling off entirely for sources that never quote — measurably faster, but any stray quote silently joins columns; it composes with pinned separators only, since detection itself respects quotes.

## string materialization policy

- Strings are the only allocation in the read pipeline and are policy-routed through `SepCreateToString`: `SepToString.Direct` (allocate per access), `PoolPerCol(maxStringLength, initialCapacity, maximumCapacity)`, `OnePool`, and thread-safe variants including a fixed-capacity arm for bounded memory.
- Pool-hit economics ride column cardinality: low-cardinality categorical columns approach zero steady-state allocation; high-cardinality columns waste pool probes — the policy is chosen per profile, per data shape.
- `maxStringLength` caps which values are pool-eligible — long values bypass the pool by design, so the cap is a memory bound, not a correctness knob.
- Parallel enumeration requires the thread-safe pool variants; the default per-column pool is not thread-safe, and the mismatch surfaces only under parallel load — the parallel profile pins `PoolPerColThreadSafe` or the fixed-capacity arm explicitly.
- `reader.ToString(colIndex)` materializes through the same policy — there is exactly one span-to-string seam, so changing the pool policy changes every materialization at once.

## parallel law

- `ParallelEnumerate(reader, rowFunc | rowTryFunc[, degreeOfParallelism])` is the only parallel lane: it batches row state into pooled buffers and runs the projection as an ordered parallel query.
- Output order matches input order by construction — parallelism never reorders an exchange, so downstream order-dependent folds stay valid without sorting.
- An empty source short-circuits to an empty result without spinning up the parallel machinery.
- The try-shape (`RowTryFunc<T>` returning bool with an `out` value) fuses parse-validate-filter into one pass: rejected rows simply do not emit — admission becomes absent rows plus caller-side evidence, not exceptions inside worker threads.
- `Enumerate` is a lazy iterator over the live reader — the reader must outlive the enumeration, so the projection materializes inside the reader's scope or the reader's lifetime is owned by the consuming fold; returning the lazy enumerable past the reader's disposal is the use-after-free shape the laziness hides.
- Async consumption is first-class on the reader itself: `GetAsyncEnumerator(CancellationToken)` threads cancellation into row advance, so a drain participates in cooperative shutdown without wrapping the loop.
- Parsing stays single-threaded by design — the reader advances and batches row state, and only the projection fans out — so parallel gains cap at the projection's share of total cost, and a parse-dominated load sees no speedup from any degree setting.
- Parallelism pays only when per-row projection cost dominates I/O — wide rows, heavy parsing, per-row admission logic; for thin pass-throughs, sequential `Enumerate` beats the coordination overhead.
- The degree-of-parallelism overload is the budget hook: under a host with a declared parallelism budget, the lane consumes a budget row instead of defaulting to all cores.
- The unstated degree defers to the scheduler's choice — acceptable for a dedicated process, a defect inside a host that owns its parallelism, which is why embedded profiles always state the degree.
- Failure receipts inside parallel projection compose the row's own evidence — row index, line range, and the failing column index identify a cell; nothing thread-local participates, so receipts are identical between sequential and parallel runs.

## writer law

- Writer policy is the mirror record (`SepWriterOptions`): separator (no auto on write), culture, `WriteHeader` (true default), `Escape` (false default), `ColNotSetOption` (`Throw` default | `Empty` | `Skip`), `DisableColCountCheck`, `AsyncContinueOnCapturedContext`.
- `WriteHeader` true requires every column named; index-only emission requires it off — names and indexes are two emission modes, not mixable mid-file.
- Escape-off is the loaded default: with `Escape` false, a value containing the separator, a quote, or a newline writes raw and corrupts the file silently — any writer whose values are not provably separator-free declares `Escape` true.
- Escape semantics are exact: wrap in quotes when the value contains the separator, `\r`, `\n`, or a quote, and double inner quotes; escaping applies to written header names as well as values.
- `ColNotSetOption.Skip` plus `DisableColCountCheck` permits variable-width rows but desynchronizes data columns from header names — lawful only for headerless emission; with a header, `Empty` is the absence spelling.
- Write is span-first and format-driven: `Col.Set(ReadOnlySpan<char>)`, `Set(ReadOnlySpan<byte>)` for UTF-8 sources, and interpolated-string-handler `Set($"...")` overloads (with an optional `IFormatProvider`) that format directly into the sink buffer.
- `Col.Format<T>(value[, format])` covers any `ISpanFormattable` with an explicit format span — culture and format resolve at the write site or from options; `ToString()` on intermediate values is the rejected spelling.
- `Cols.Set`/`Cols.Format` lift whole column sets — list, array, span, and `params ReadOnlySpan<T>` arms, plus a per-element `ColAction<T>` arm for mixed formatting — so a record writes as one or two set-calls, not a column loop.
- The writer's `Cols.Set` also accepts a reader's column set directly — a projected subset of an incoming row transfers to the outgoing row in one call, which is the column-subsetting spelling of the reader-to-writer fusion.
- Rows are scoped units: `NewRow()` returns a disposable ref-struct row whose `Dispose` commits it; a `CancellationToken` arm exists for cooperative abort mid-stream — and the row also disposes asynchronously, so async sinks commit rows without sync-over-async at the flush boundary.
- Sinks mirror sources: `ToText([capacity])`, `ToFile`, `To(stream | writer | StringBuilder[, leaveOpen])`; `Flush`/`FlushAsync` drain explicitly, and the writer's `ToString()` dumps the text sink for in-memory flows.
- The reader-to-writer fusion is first-class: `writer.NewRow(readerRow)` and `readerRow.CopyTo(writerRow)` copy a parsed row into an emission under the writer's own escape policy — re-separating, re-escaping, and column-reordering flows are one copy expression, not parse-materialize-rebuild.
- Writer columns materialize on first touch: the writer row's indexers (`row[int]`, `row[string]`, plus multi-name and multi-index `Cols` arms) get-or-add the column, so the first data row defines the column set and later rows are checked against it — column-set drift between rows is the loud failure, not silent widening.
- `SepWriterHeader` pre-declares columns ahead of data: `Add`/`TryAdd`/`Contains` build the name set, and explicit `Write`/`WriteAsync` emits the header row before any data row exists — the empty-but-valid-artifact shape (header, zero rows) is producible deliberately, which downstream gates distinguish from a missing artifact.
- Row-bearing flows never call the explicit header write — the header emits with the first committed row under the header option — so the explicit call is the zero-row arm only, not a setup step to remember.

## schema-stamped artifact exchange

- An exchange artifact is payload plus identity: a schema descriptor stamp and a content hash; an artifact without both is not admissible to a contract path.
- Placement follows the format's own metadata channel: columnar artifacts carry the stamp inside the file (footer key-value metadata, written at export, read back without decoding data).
- Delimited artifacts have no metadata channel — identity travels beside the file as a sidecar or in the delivery envelope, never smuggled into fake header rows that corrupt the column contract.
- The stamp is a descriptor, not prose: column names, declared types, nullability, and a contract revision — enough that a consumer diffs the incoming descriptor against its compiled expectation before reading a single row.
- Hash identity composes the durability-owned hashing lane over artifact bytes; stamp plus hash makes artifacts content-addressable and replay-safe — same descriptor, same hash, same artifact, idempotent re-delivery.
- Identity is byte-level while equality is descriptor-level, and the two never conflate: a newline-convention change or column reorder changes the hash without changing the data — re-keying an artifact store by semantic equality requires canonical emission, not hash comparison of foreign bytes.
- The header gate is order-insensitive by name-set: column order differences are not drift under name-keyed access, but duplicate header names cannot index uniquely and reject at the gate before any row is read.
- Evolution is descriptor-diff law: an additive column is compatible by construction because consumers read by name and tolerate extras; a removal, rename, or retype changes the contract revision and is a new artifact class, never an in-place mutation.
- Column order is never load-bearing — name-keyed access on both rails (header index resolution on delimited, named columns on columnar) is what makes additive growth free.
- Format selection is fidelity-routed: columnar for typed exchange between systems that both speak it (types, nulls, nesting survive); delimited only where a foreign consumer demands text.
- Every delimited hop degrades types to culture-formatted strings — the invariant-culture, ISO-8601-temporal, explicit-escape profile is the only delimited profile that round-trips at all, and temporal spelling composes the transport-owned converter law rather than re-deciding it.
- Delivery is a closed destination union — file path, blob lane, bundle member — where each arm carries its own provenance (path plus stamp, object key plus hash, bundle manifest row); adding a destination is a new union case that breaks dispatch at compile time.
- A string-typed destination parameter is the foreclosed form — it erases the per-arm provenance obligation and reopens dispatch at runtime.
- Signature gating precedes typed construction: columnar artifacts self-identify by magic bytes at fixed offsets; delimited artifacts have no signature, so their gate is structural — separator detection plus header-shape match against the expected descriptor.
- An artifact failing its gate is rejected before any typed payload exists, with the gate verdict as the evidence row.

## divergent — sep-depth

- The maximal unification: one options value, one reader, and the consumption modality — sequential, async, parallel, try-filtered — discriminates purely on the enumeration verb over the same `Row` projection.
- The profile collapse goes one level higher: an artifact class declares one record from which reader options, writer options, and the schema descriptor all derive — three consumers, one source — so profile symmetry between read and write sides is structural rather than maintained.
- The quarantine flow composes strict structure with tolerant values: strict quote handling plus try-shaped projection routes structurally valid but value-invalid rows into an evidence lane while the stream continues — the shape for ingesting partner data that is almost clean.
- Code under this law has exactly one row-projection function per exchange profile, reused across all four modalities; modality-specific row handlers are the sprawl signal the law deletes.
- The zero-allocation pipeline at full depth: span in (source decode into the internal buffer), span-parse per column (`ISpanParsable` lattice plus fast-float), pooled-string egress only for columns that must become strings, span-formatted write out (`ISpanFormattable` plus interpolation handlers into the sink).
- String allocation appears exactly where a string-typed domain value is admitted — everywhere else the pipeline is buffer-to-buffer, and an allocation profile showing strings elsewhere marks a policy violation, not a tuning opportunity.
- Failure taxonomy with boundaries: structural failures (column-count deviation, quote imbalance under strict parsing) throw at row advance and carry the physical-line range; value failures route through `TryParse`/`RowTryFunc` as typed absence; policy failures (unset column under `Throw`) surface at row commit on the writer.
- Three failure classes, three surfacing points, three receipt shapes — collapsing them into one catch-all loses the class discriminant the rail needs.
- Rejected: `string.Split` pipelines — foreclose quoting, escaping, and span economics in one stroke.
- Rejected: per-row DTO materialization with reflection mapping — forecloses ref-struct confinement and allocates per cell.
- Rejected: regex-based delimited parsing — forecloses streaming and degrades quadratically on quoted edges.
- Rejected: caching `Row`/`Col` across row advances — does not compile, which is the ref-struct law working as designed.

## divergent — schema-stamped-exchange

- The exchange law unifies producer and consumer around one descriptor value: the producer derives the stamp from its typed projection at export; the consumer diffs the stamp at admission; the same descriptor type drives both.
- Descriptor-as-contract eliminates the parallel schema documents that drift — the descriptor lives with the artifact-class declaration, and emission, stamping, and the expected-shape gate all derive from that one declaration.
- A column added to the projection therefore updates stamp, writer, and admission gate in one diff — the absorbed-growth proof for the exchange surface.
- Generation directories are the evolution substrate: artifacts accumulate as immutable generations, never rewritten in place; the consumer's name-keyed union read absorbs additive drift across generations.
- Provenance columns (source path, generation key) keep every row traceable to its stamp; compaction — rewriting many generations into one — is a new artifact with a new hash, never an in-place merge.
- Cross-rail composition: the columnar stamp channel and the delimited sidecar converge at the destination union — the delivery arm, not the payload format, owns where identity travels.
- A consumer therefore resolves identity uniformly from the arm's provenance regardless of payload format — the collapse that lets one admission gate serve every tabular artifact class: gate on signature, resolve identity from the arm, diff the descriptor, then construct the typed reader.
- Quantitative posture: stamp reads never decode payload — footer metadata and sidecars are O(1) against artifact size, so admission verdicts are cheap enough to run on every delivery.
- Descriptor diff is name-keyed set algebra — added, removed, retyped — whose output is the admission verdict plus its evidence row.
- The diff algebra carries a compatibility lattice, not a boolean: a widening retype (integer to wider integer, non-null to nullable) may be declared compatible as a policy row, while a narrowing retype never is — compatibility is data, reviewed where the artifact class is declared.
- Revision monotonicity is part of the contract: revisions only increase, so a consumer seeing an older revision than its compiled expectation distinguishes a stale producer from a corrupt artifact — two different operational responses from one comparison.
- Identity nests through the bundle arm: a bundle member's provenance row carries the member path plus the bundle's own hash, so verifying the bundle verifies every member's container while each member still carries its own stamp — one verification walk, two identity levels.
- The sidecar binds by content hash, never by filename convention alone — a renamed artifact keeps its identity, and a sidecar pointing at bytes that no longer hash to its claim is the corruption signal, not a tolerable drift.
