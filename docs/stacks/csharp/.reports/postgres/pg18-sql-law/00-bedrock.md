# pg18-sql-law — bedrock

## engine feature gate

- The provider carries one declared engine-version gate — `SetPostgresVersion(int major, int minor)` on the provider options builder — and every current-engine translation below switches on it.
- Undeclared, the provider assumes a trailing default and silently withholds newer translations; the gate is mandatory store-profile policy, not an optimization knob. The whole feature law of this page hangs on one declaration.
- Adoption boundary law: a feature is admitted into the SQL law only when (a) the engine floor carries it, (b) the declared gate exposes it, and (c) a typed spelling exists — LINQ translation, model annotation, or interpolated typed SQL.
- A feature failing (c) is not "unusable"; it routes to the typed-SQL rail (divergent section). The gate decides translation availability, never capability existence.

## identity law — uuidv7

- `uuidv7()` is the canonical surrogate-key generator: timestamp-prefixed v7 UUIDs keep B-tree insertion append-mostly, deleting the random-page-write amplification v4 keys impose on every index that carries them.
- `uuidv4()` exists as an explicit alias for the random form; choosing it is a declared decision, never a default.
- Signature edge: `uuidv7([shift interval])` — the optional interval shifts the embedded timestamp. This is the backfill lever: synthetic historical rows carry truthful temporal ordering by shifting, instead of fabricating ordering columns.
- Extraction pair: `uuid_extract_timestamp(uuid) → timestamptz` and `uuid_extract_version(uuid) → smallint`.
- Consequence of extraction: a v7 key carries a free coarse creation-time axis — range scans on creation time can predicate on the key itself, collapsing a dedicated `created_at` index for coarse windows.
- Client/server duality: `Guid.CreateVersion7()` translates to `uuidv7()` under the engine gate — one identity vocabulary serves client-generated (offline, batch-assembled) and server-generated (default-valued) keys.
- Key-generation-site law: site is a policy value (client | server), never two key shapes; both sides produce the same ordered species, so merge across writers needs no key reconciliation.
- Integer-key counterpoint: `UseIdentityByDefaultColumn` (provider default) / `UseIdentityAlwaysColumn` / `UseSequence` / `UseHiLo` remain the integer lanes, with `HasIdentityOptions(startValue, incrementBy, minValue, maxValue, cyclic, numbersToCache)` as the single tuning row.
- The identity chooser: integers where the key never leaves the store boundary; v7 UUIDs where keys travel across processes, merge from multiple writers, or pre-exist their row.
- `UseSerialColumn` is the legacy spelling kept for scaffolded compatibility; identity columns own the integer lane in new schemas.

## generated-column law

- Virtual is the engine default: `GENERATED ALWAYS AS (expr)` without `STORED` computes at read, occupies zero heap, and makes adding a derived column a metadata-only migration.
- The provider mirrors the flip: omitting `stored: true` on a computed-column declaration now emits a virtual column under the engine gate — a silent semantics change for migration diffs authored against the older stored default. Diff review must treat absent `stored:` as semantic, not noise.
- The stored-only boundary (the law's edge): a virtual column cannot be indexed, and it does not travel in logical replication by default.
- `STORED` is therefore a forced consequence, not a preference: `indexed(col) ∨ replicated(col) → STORED`; everything else stays virtual. That single implication is the entire decision table.
- Replication nuance inside the rule: generated columns replicate when the publication's column list names them, or via the publication's `publish_generated_columns` option — and only stored generation qualifies.
- A virtual column referenced by a downstream change-contract consumer is a publication design error; it surfaces at subscription, not at DDL — catch it in publication review.
- Write-amplification calculus: STORED pays expression cost plus heap bytes per write and a table rewrite on expression change; virtual pays expression cost per read. Hot-read derived predicates needing an index pay stored; display projections and rarely-read computed flags stay virtual and free.
- Virtual columns ride star expansion like any column — wide virtual expressions are a per-read CPU tax on every unprojected read path; narrow projections are the remedy, and a virtual column priced for one consumer should not bill every reader.
- Composition: a stored generated `tsvector` column is the canonical full-text spine (mechanics in the extension lane); this law decides only the stored/virtual axis it rides on.

## DML law — RETURNING old/new, MERGE, conflict routing

- `RETURNING` exposes `old.*` and `new.*` on `INSERT`, `UPDATE`, `DELETE`, and `MERGE`; the aliases are renameable to dodge column-name collisions.
- The transition pair deletes the read-before-write round trip: one statement yields `(before, after)` — the natural shape for change receipts, optimistic-concurrency evidence, and delta emission.
- Rejected form: any pattern that SELECTs prior state, mutates, then re-reads is now structurally obsolete — it pays two extra round trips for a weaker consistency story.
- `MERGE ... RETURNING merge_action(), old.*, new.*` is the one upsert-with-evidence statement: per-row verdict (insert | update | delete) plus the transition pair in one scan of the target.
- `WHEN NOT MATCHED BY SOURCE` arms make MERGE a full reconciliation primitive — set the target equal to the source, with receipts per row.
- MERGE's source is any row source — a staging relation for bulk, a `VALUES` list for small reconciles — so the same statement shape serves ten rows inline and ten million staged, with no intermediate table for the small case.
- Data-modifying CTEs chain receipts into further folds in one statement: `WITH changed AS (UPDATE ... RETURNING new.*) SELECT aggregate FROM changed` — the mutation and its receipt aggregation are one round trip, and the CTE boundary is the only place a mutation's output can feed further relational algebra.
- Conflict routing chooser: `INSERT ... ON CONFLICT DO UPDATE` is the high-concurrency single-row upsert — it takes speculative insertion locks and cannot raise unique violations.
- ON CONFLICT requires an arbiter: the conflict target must infer a unique index or named constraint, and a conflict target with a `WHERE` clause arbitrates against a partial unique index — partial-uniqueness upserts are first-class, not a workaround.
- The `EXCLUDED` pseudo-row inside `DO UPDATE` carries the proposed values; `DO UPDATE ... WHERE` arms make the update conditional on the existing row — conflict-skip-if-unchanged is one predicate, not a read.
- jsonb subscripting assigns in place: `UPDATE t SET doc['k'] = value` is the typed-SQL spelling for single-path document writes outside the mapped partial-update lane — same effect class as the function form, terser under composition.
- MERGE can race under concurrency: a unique violation inside MERGE is a retryable rail error, not a logic bug. Retry policy belongs on the MERGE rail only — ON CONFLICT needs none.
- The routing law: row-at-a-time idempotent writes → ON CONFLICT; batch reconciliation against a staging or source set → MERGE. Neither has a LINQ spelling; both are typed-SQL residents.
- `COPY ... ON_ERROR ignore` with `REJECT_LIMIT n` converts bulk admission from all-or-nothing to bounded-defect admission — the reject limit is a typed budget, breach aborts, `LOG_VERBOSITY` governs defect-row noise.
- The bulk-defect budget is the SQL-side mirror of boundary admission: defects are counted and bounded, never silently absorbed.

## JSON law — jsonb-canonical, index-asymmetric

- jsonb is canonical-by-construction: duplicate keys collapse last-wins, key order is rewritten, whitespace is gone. Documents are stored canonical, full stop.
- `json` is a text-preserving relic admitted only when byte-identity of the original document is itself the contract (signed payloads); admitting it is a declared exception with its reason attached.
- Index asymmetry is the document law's core, row one: GIN `jsonb_ops` (default) indexes every key and value — serves existence (`?`), containment (`@>`), and path predicates, at full index weight.
- Row two: GIN `jsonb_path_ops` indexes only value-path hashes — substantially smaller and faster for containment, structurally blind to key-existence predicates.
- Row three: an expression GIN on one hot interior path — smallest of all, serving exactly one path and nothing else.
- The index choice is therefore a query-vocabulary declaration: containment-only document lanes take `jsonb_path_ops`; mixed-predicate lanes pay for `jsonb_ops`; single-path lanes take the expression row.
- Canonicality has an index dividend: because jsonb stores one canonical byte form, a plain unique B-tree over the document enforces canonical-document uniqueness — semantically-equal documents collide regardless of source formatting, a dedup constraint no text store can express.
- `JSON_TABLE(doc, path COLUMNS (...))` shreds documents into relational rows inside the query: typed columns, nested paths, ordinality, per-column error handling. It replaces client-side document iteration and lateral element-expansion chains with one declarative projection.
- JSON_TABLE has no LINQ spelling — flagship typed-SQL resident.
- Its output is a relation like any other: it joins, aggregates, and feeds MERGE as a source — document shredding composes with the reconciliation law in one statement, documents in, per-row verdicts out.
- SQL/JSON path (`jsonpath`) is the predicate language under `@?`/`@@` and JSON_TABLE; GIN serves `@?`/`@@` where the path is index-compatible.
- The two path operators answer different questions: `@?` tests path existence (any match), `@@` evaluates the path as a boolean predicate (first result must be true) — a predicate written with the wrong operator silently changes semantics on empty matches, the subtle one of the pair.
- Hot-scalar promotion: a jsonb interior scalar that is equality-filtered or sorted hot graduates to a stored generated column with a B-tree — the generated-column law composing into the document law; the document stays canonical, the scalar gets relational index economics, and no write path changes.
- Large-document storage composes with the column-compression declaration: document columns whose values routinely exceed inline size take the cheaper compression codec as a per-column model fact — document law and storage law meet in one declaration, not in server defaults.
- Trap with boundary: extracting-then-comparing (`->>'k' = v`) defeats `jsonb_path_ops` containment indexes; the same predicate as a containment or path expression stays indexed. Predicate spelling is index policy.
- Typed document mapping: complex-type JSON mapping (`ComplexProperty(...).ToJson()`) lands documents in jsonb with full LINQ traversal translation, replacing owned-entity JSON mapping.
- `ExecuteUpdate` against members inside the document translates to `jsonb_set()` — a partial document write with no read-modify-write cycle and no full-document churn.
- Scalar collections map to native arrays at top level and JSON arrays when nested inside documents; nested-collection membership translates to `@>` containment — which the GIN rows above serve. A queried nested collection implies a containment-capable index row: mapping law and index law meet here.

## constraint law — temporal and deferred validation

- Temporal keys: `PRIMARY KEY (id, valid_period WITHOUT OVERLAPS)` and `UNIQUE ... WITHOUT OVERLAPS` enforce range non-overlap per key inside the engine.
- Temporal references: `FOREIGN KEY (id, period PERIOD) REFERENCES parent (id, period)` enforces interval containment across tables.
- The application-side overlap check — the previously unavoidable read-check-write race — is deleted; concurrent writers serialize on the constraint itself.
- Range and multirange column mappings make these constraints directly reachable from mapped temporal types; the constraint is model-visible, and its violation class (exclusion violation, SQLSTATE 23P01) joins the typed conflict-fault vocabulary at the boundary.
- Key-shape edge: the WITHOUT OVERLAPS column is the last key column and the underlying index is GiST-shaped — equality columns lead, the range trails; key design follows that order or the constraint cannot form.
- The GiST shape pulls in an extension dependency: scalar key parts inside a GiST index require the btree-emulation operator extension — a temporal constraint is therefore also an extension capability row, declared and verified like any lane, not a pure-core feature.
- Virtual columns and predicates: a virtual generated column may be filtered freely, but the predicate cannot be index-served (virtual is unindexable) — a hot predicate on a virtual column is the promotion signal to stored-plus-index, the same move as hot-scalar promotion below.
- WITHOUT OVERLAPS is the declarative special case of the general exclusion constraint (`EXCLUDE USING gist (key WITH =, period WITH &&)`); bespoke exclusion shapes — inequality keys, custom overlap operators, buffered geometry exclusion — still take the general form, which remains a typed-SQL/migration resident.
- The chooser between them: temporal key semantics → WITHOUT OVERLAPS (participates in PRIMARY KEY/UNIQUE, referencable by PERIOD foreign keys); any other mutual-exclusion predicate → EXCLUDE, which cannot be referenced by a foreign key — referencability is the dividing line.
- `NOT NULL ... NOT VALID` two-phases null-tightening on large relations: declare (instant; new writes enforced) then `VALIDATE CONSTRAINT` (concurrent-friendly full check).
- NOT NULL constraints now materialize as catalog constraint rows at all — introspectable and nameable rather than column flags; verification tooling can read them like any constraint.
- Law: tightening a hot table is always declare-then-validate; single-step `SET NOT NULL` on a large relation is the rejected lock-holding form.
- Cluster integrity default moved: data checksums are on by default at cluster initialization — page corruption surfaces as typed read errors instead of silent drift; checksum state is a compatibility token between clusters that the ops verification law reads.

## fault-class disposition

- Statement shapes map to SQLSTATE classes deterministically, and the disposition taxonomy is therefore declarable beside the statement vocabulary: unique violation (23505) and exclusion violation (23P01) are conflict facts — typed domain faults, never retried; foreign-key (23503) and check (23514) violations are admission defects — the write was wrong, retry cannot fix it.
- Serialization failure (40001) and deadlock detection (40P01) are the retryable class — artifacts of concurrent scheduling, not of the statement; they are the only classes a store-level retry strategy may consume silently.
- Data-exception class faults (the 22-prefixed family — bad casts, range overflow, malformed input) are admission defects that escaped the boundary: their disposition is upstream repair at the validation seam, and their appearance in production is evidence the admission gate has a hole.
- The taxonomy's load-bearing consequence: fault handling is a total dispatch over SQLSTATE class — conflict → domain fault with constraint identity; admission defect → typed rejection with column/constraint evidence; scheduling artifact → strategy-absorbed retry; everything else → boundary escalation. No string inspection, no catch-all rethrow.
- MERGE's unique-violation possibility (above) lands in the conflict row of this same dispatch — the taxonomy is one table for every statement shape, which is what makes per-statement error handling a rejected form.

## index vocabulary law

- The provider's index grammar covers the engine's index features as model declarations: `HasMethod` (access method), `HasOperators` (per-column opclass), `IncludeProperties` (covering payload), `AreNullsDistinct` (null-uniqueness semantics), `HasNullSortOrder` (per-column null placement), `UseCollation` (per-column collation), `HasStorageParameter` (any storage knob) — index design is schema data, never deploy-time hand DDL.
- Covering indexes (`IncludeProperties`) carry non-key payload in the leaf — index-only scans for the projection without widening the key; the chooser: columns filtered or sorted go in the key, columns merely projected go in the include list, and a projected column in the key position is wasted key width.
- `AreNullsDistinct(false)` flips unique semantics to treat nulls as equal — single-null uniqueness without the partial-index workaround; the default (nulls distinct) permits unlimited null rows under a unique constraint, the classic silent surprise this declaration exists to delete.
- Per-column null sort order matters exactly where an index serves an `ORDER BY ... NULLS FIRST/LAST` — mismatch silently forfeits the index for the sort; the declaration and the query spell the same policy once.
- Per-column TOAST compression is a property declaration (`UseCompressionMethod`) — the modern compression method for large text/document/vector payloads is a per-column decision recorded in the model, and changing it affects only newly written values (old rows keep their codec until rewritten).
- Element-converted arrays stay native arrays (`HasPostgresArrayConversion` with element converter): a value-converted element type does not demote the column to bytea or jsonb — array operators and membership translation survive element conversion, which no generic converter mechanism would preserve.

## planner and execution law

- Skip scan: a multicolumn B-tree now serves predicates that omit leading columns — the planner skips across distinct leading values.
- Adoption consequence: composite indexes ordered `(low-cardinality discriminant, selective column)` become broadly reusable; a single-column index whose only justification was "leading column unfiltered" is now deletable redundancy.
- Skip-scan boundary: the win scales inversely with leading-column cardinality — high-cardinality leaders still warrant their own index. Index-set review is due at engine adoption, not deferred.
- Skip scan is planner-transparent — no query change, no hint surface; adoption work is entirely an index-set review, and a query suddenly using a composite index it previously ignored is the feature working, not a plan regression.
- Asynchronous I/O: `io_method = worker` (default; `io_workers` pool, default 3) or `io_uring` (platform-conditional) issues reads concurrently for sequential scans, bitmap heap scans, and vacuum.
- Concurrency defaults rose: `effective_io_concurrency` and `maintenance_io_concurrency` default to 16 — server facts that explain bulk-read throughput envelopes; the process reads them, never sets them.
- `EXPLAIN ANALYZE` now includes buffer counts by default and reports fractional row estimates, per-node index lookup counts, and explicit disabled-node marks — plan evidence is richer without flag ceremony; plan-parsing diagnostics must accept the buffers block unconditionally.
- Prepared repetition interacts with plan genericity: repeated executions graduate to generic plans, and the array-parameter membership spelling (one parameter for any collection size) is what keeps those generic plans valid — statement-text stability and plan stability are the same property seen from two layers.
- Upgrades preserve planner statistics, extended statistics excepted — the post-upgrade law shrinks to: re-ANALYZE only relations carrying extended statistics, not the cluster.

## divergent — pg-features-depth

- v7 keys × skip scan: composite indexes `(tenant_discriminant, v7_key)` stay append-local per discriminant AND serve key-only lookups via skip scan — the previously mandatory second index on the bare key is deletable when discriminant cardinality is low. Two features jointly delete an index class.
- RETURNING old/new × jsonb partial update: `UPDATE ... SET doc = jsonb_set(doc, path, val) RETURNING old.doc #> path, new.doc #> path` yields a path-scoped before/after delta in one statement — the minimal change-evidence quantum for document stores, composable into the change-contract shape the driver lane decodes.
- Temporal WITHOUT OVERLAPS × range mappings: the overlap rejection arrives as a constraint-violation SQLSTATE and converts at the boundary into a typed conflict fault, not a generic store error — calendar correctness becomes a fault-vocabulary row instead of an application algorithm.
- MERGE × COPY defect budgets: staged bulk admission (COPY with a reject budget) followed by MERGE reconciliation composes two receipted primitives into a fully-evidenced ingestion pipeline — rows admitted, rows rejected, per-row reconciliation verdicts — with zero application-side row iteration.
- Trap: virtual-by-default flips computed-column semantics in migration diffs authored before the gate; absent `stored:` is a semantic change. Boundary: only computed columns; plain columns unaffected.
- Trap: MERGE raises unique violations where ON CONFLICT cannot; retry policy is MERGE-rail-only. Boundary: single-row idempotent upserts never need it.
- Trap: bulk CSV end-of-data marker handling changed — embedded terminator lines no longer end file-sourced CSV, while older stdin-streaming client tooling may still honor them; bulk lanes pin client tooling to the engine generation.
- Trap: checksum-state mismatch between clusters blocks in-place upgrade; the verification law reads the checksum token before any upgrade plan exists.
- v7 keys × COPY: bulk admission has no key-returning channel (COPY carries no RETURNING), so bulk lanes use client-generated v7 keys — rows arrive key-complete, and the server-default key strategy is structurally incompatible with the highest-throughput write path.

## divergent — sql-vs-linq routing law

- The routing law in one sentence: a construct lives in LINQ exactly when the provider owns its translation; everything else is typed interpolated SQL through the same composable query rail — there is no third lane, and string concatenation does not exist.
- LINQ-resident value families (provider-translated): array operators and `= ANY` membership; range/multirange algebra with range aggregates (`RangeAgg`, `RangeIntersectAgg`); temporal aggregates over mapped calendar types (`Sum`, `Average`, `Distance` as function projections); network and cube algebra; KNN distance ordering.
- LINQ-resident text families: full-text (`ToTsVector`, `WebSearchToTsQuery`, `Matches`, `Rank` with weight vectors and normalization); the trigram similarity family; the phonetic/edit-distance family (`FuzzyStringMatchLevenshtein`, bounded `...LessEqual`, `...Soundex`, `...Metaphone`); `ILike`.
- LINQ-resident document and hierarchy families: JSON traversal, containment, and `jsonb_set` partial updates via `ExecuteUpdate`; ltree ancestry and pattern matching; set-based `ExecuteUpdate`/`ExecuteDelete` as the mutation tail.
- Aggregate projections are LINQ-resident too: `ArrayAgg`, `JsonAgg`/`JsonbAgg`, `JsonObjectAgg`/`JsonbObjectAgg` (tuple-pair input), and the statistical family fold grouped rows into arrays and documents server-side — the canonical replacement for fetch-then-group materialization.
- The aggregate consequence: a parent-with-children projection is one grouped query ending in `ArrayAgg`/`JsonbAgg`, not a join multiplication or a second round trip — document-shaped reads come from relational storage on demand.
- Parameterized-collection edge: membership over a parameter collection translates to `= ANY(@array)` — one array parameter regardless of element count, so plans are cacheable and auto-preparation-friendly where expanded IN-lists fragment the plan cache; collection size changes never change the statement text.
- Routing any translated construct to raw SQL forfeits compile-time shape checking for zero capability gain — rejected form.
- Typed-SQL residents, statement-shaped (no translation; the construct is a whole-statement form): `MERGE` with `merge_action()` and old/new; `INSERT ... ON CONFLICT`; `RETURNING old/new`; every `COPY` form (driver-lane mechanics).
- Typed-SQL residents, query-shaped (relational-algebra constructs without translations): `JSON_TABLE`; `LATERAL` correlation; `DISTINCT ON`; recursive CTEs; `TABLESAMPLE`; window frames beyond the translated core; set-returning crosstab-species functions.
- Typed-SQL residents route through interpolated typed SQL (`SqlQuery<T>` / `ExecuteSql` species): interpolation holes become parameters mechanically, never text.
- Composability hinge: an interpolated typed-SQL query returning a mapped type re-enters LINQ — `Where`/`OrderBy`/`Select` compose over it as a subquery. The routing decision is per-construct, not per-query: a MERGE-fed CTE can terminate in a LINQ tail.
- Routing falsifier: if a typed-SQL construct must sit in a non-leaf position under LINQ operators that need its columns, the whole query routes typed-SQL — the composition hinge opens only where the SQL fragment is the root source.
- Parameterization edge: identifiers (table, column, index names) are not parameterizable in either lane; dynamic identifiers ride quoted-identifier interpolation at one declared seam over a closed vocabulary — free-text identifiers are an injection surface and a rejected form.
- DDL routes through neither door: schema statements live in migration artifacts exclusively; a DDL string in either query lane is a deployment-law violation surfacing as a privilege failure under the runtime credential — the privilege split enforces the routing.
- The routing table is self-enforcing at query compilation: an untranslatable LINQ construct fails loudly when the query compiles, never by silent client evaluation — misrouting toward LINQ is caught structurally, and only misrouting toward raw SQL (forfeiting an existing translation) needs review discipline.
- The table also drifts in one direction only: provider generations move constructs from resident to translated, never back — a routing decision is re-checkable but never invalidated toward more SQL.
- The two-door consequence: because both doors feed one rail, query review reduces to one question per construct — "does a translation exist?" — and the answer is static, cacheable, and enforceable by inspection of the translated-family list above.
