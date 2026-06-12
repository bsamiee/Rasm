# extension-lanes — bedrock

## extension admission and the provisioning axis

- Every lane below is one declared capability row built from at most three parts: a model-level extension declaration (`HasPostgresExtension`), a wire admission where the lane carries its own codecs, and a query vocabulary the provider translates.
- That triple is the closed shape of "an extension lane"; lanes differ only in which parts they need, and a new lane is three rows, zero new architecture.
- The self-vs-operator axis is decided by one server fact: whether the capability functions entirely through SQL in the database, or requires server-process configuration (shared-library preload, background worker).
- The btree-emulation operator extension joins the self-provisionable set as a constraint dependency, not a query lane: temporal keys and mixed scalar-plus-range exclusion constraints require it — a constraint-bearing migration carries the extension declaration the constraint needs.
- Self-provisionable (a migration's `CREATE EXTENSION` declaration suffices): vector, geo, trigram, ltree, hstore, citext, intarray, cube, tablefunc, and the partition manager's SQL surface.
- Operator-provisioned: anything demanding preload or a worker — in-database schedulers, audit hooks, the partition manager's background-worker variant.
- The axis is per-capability, not per-extension: the partition manager sits on both sides — SQL surface self-side, worker variant operator-side.
- Self-provisioned does not mean process-provisioned: the migration artifact declares `CREATE EXTENSION`; the running process only verifies (ops-lane law). The axis decides which artifact owns creation — migration versus operator runbook — never whether the process may create.
- Dual-admission law for codec-bearing lanes: vector and geo need both the model plugin (`UseVector` / `UseNetTopologySuite` on provider options) and the wire admission on the data-source builder (the geo lane's builder-level `UseNetTopologySuite`; the vector resolver riding the plugin's data-source configuration plugin).
- Database-first scaffolding preserves lane admission: codec-bearing lanes ship code-generator plugins that emit their own admission calls into scaffolded configuration — reverse-engineered models arrive lane-complete instead of silently lane-blind.
- Neither half stands alone — model-only fails at materialization, wire-only fails at translation. One composed admission row per lane, declared once.
- Capability rows carry extension-generation floors as data: a lane's advanced settings exist only above a given extension generation (iterative vector scans are the canonical case) — presence below the floor is absence for that sub-capability, and the verification fold reads the floor from the row, not from code.
- The floor mechanism is what lets one lane definition span heterogeneous environments: the lane folds to its supported sub-capability set per environment, and queries dispatch on the folded set rather than probing.

## vector lane

- Type ladder with index ceilings — the capacity law: `vector` indexes to 2000 dimensions; `halfvec` to 4000; `bit` to 64000; `sparsevec` to 1000 nonzero elements.
- The CLR value surface carries the dense form (`Vector` over `ReadOnlyMemory<float>`) and the sparse form; half-precision and bit forms have no mapped CLR value.
- That absence is by design, not gap: quantized forms are reached through expression indexes and typed-SQL casts — storage stays `vector`, cheaper representations exist inside index expressions only.
- Distance vocabulary as translated members: `L2Distance` (`<->`), `MaxInnerProduct` (`<#>` — negated inner product, so ascending order means most-similar-first only because the operator negates), `CosineDistance` (`<=>`), `L1Distance` (`<+>`), `HammingDistance` and `JaccardDistance` over bit.
- Opclass-operator agreement is the first sequential-scan diagnostic: the operator in `ORDER BY` must match the index's operator class or the index is silently unused.
- HNSW build knobs: `m` (16) and `ef_construction` (64) trade build time for graph quality; query knob `hnsw.ef_search` (40) trades latency for recall.
- `ef_search` floors the result count: a `LIMIT` above it cannot be satisfied — the silent-truncation edge every vector query inherits.
- Index DDL is model data: `HasMethod("hnsw")`, `HasOperators("vector_cosine_ops")`, `HasStorageParameter("m", ...)` / `("ef_construction", ...)` — versioned with the schema, never hand-applied.
- Dimension is column metadata declared by the profile model: the vector mapping carries store type and size (sized `vector(n)` column type), and dimension mismatch at write is a server fault — the model declaration is the only place the embedding width lives.
- IVFFlat law: `lists` sized rows/1000 up to ~1M rows then √rows; probes starting near √lists; build requires data present — clustering an empty table produces degenerate lists, so create after load or reindex after bulk admission.
- Species chooser: IVFFlat is the smaller, faster-build, lower-recall, churn-fragile row (static centroids); HNSW the default for query-dominated, churn-tolerant lanes.
- Iterative scans are the filtered-search law: `hnsw.iterative_scan = strict_order | relaxed_order` (off default) re-enters the graph when a filtered result underfills, until satisfied or budget-capped.
- Iterative budgets: `hnsw.max_scan_tuples` (20000) and `hnsw.scan_mem_multiplier` (work-memory multiple); the ivfflat mirror rides `ivfflat.iterative_scan` and `ivfflat.max_probes`.
- Without iterative scan, a selective predicate over an ANN index silently truncates results — the overfiltering failure is the lane's primary correctness trap, and iterative scan is its fix-with-budget.
- Relaxed order returns better-cost results mildly out of order; re-ordering happens in an outer fold over the returned window.
- Strict-versus-relaxed chooser: strict order where result identity must be stable across identical queries (pagination, cached result sets); relaxed where throughput dominates and the consumer re-ranks anyway — the two modes are one policy row, not two query shapes.
- Scan-policy delivery is per-transaction setting scope: the lane's declared knob values apply as transaction-local settings around the query — policy rows materialize as scoped settings, so two lanes with different scan policies share one connection pool without cross-contamination.
- Exact-scan baseline is a first-class row, not a fallback: no index = perfect recall; small relations and recall harnesses use it deliberately.
- Recall is only definable against that baseline: every ANN tuning claim is (recall@k versus exact, latency) on the same data — an unmeasured `ef_search` bump is not tuning.
- Build mechanics: graph-in-memory builds are dramatically faster — maintenance memory sized to the graph, parallel maintenance workers raised; both are server facts the verification law reads, so index builds are migration-time or maintenance-window events, never steady-state.
- Load-then-index beats index-then-load for both species: per-row graph insertion is the slowest write path HNSW has, and bulk admission into an indexed vector column pays it per row — bulk lanes drop or defer the index, load, then build once.
- Vacuum over HNSW is slow (graph repair); churn-heavy vector tables pair bulk deletes with concurrent reindex before vacuum — a maintenance-schedule row.
- Quantization route: `binary_quantize(col)::bit(d)` expression index plus Hamming ordering as the coarse pass, re-ranked by the true operator on the stored `vector` in an outer query — two-stage retrieval in one statement.
- The same expression-index shape serves half-precision (`(col::halfvec(d))` with the matching opclass) — storage precision and index precision are decoupled by expression indexes, the lane's central density move.
- Null/zero edges: NULL vectors are unindexed; zero vectors are degenerate under cosine. Cosine lanes admit only normalized nonzero vectors; `l2_normalize` at admission makes inner-product and cosine orderings coincide — then prefer the cheaper negated-inner-product operator.
- The bit lane stands alone for near-duplicate detection: Hamming ordering over locality-style binary signatures at 64000-dimension index capacity — dedup and similarity-clustering workloads that never need float distance skip the float type entirely.
- The sparse lane serves learned sparse encoders: the nonzero-element index bound is an encoder-selection constraint, not a tuning knob — an encoder routinely exceeding it disqualifies the lane at design time.
- Filter-shape law: equality filters ride B-tree columns beside the vector index; low-cardinality filters with per-value volume justify partial vector indexes; tenant-scale isolation justifies partitioning with per-partition indexes. The planner picks ANN-before-or-after-filter from these structures — the design lever is which structures exist; the budget lever is iterative scan.

## geo lane

- Geometry/geography axis: one CLR geometry model serves both; the column type string decides semantics — `geometry(Point, srid)` planar and fast, `geography(Point, 4326)` geodesic with meter-true distances.
- Per-property SRID and dimensionality ride the column type; geodesic operations ride the mapped function rows: `Distance`/`IsWithinDistance` with spheroid behavior, `Transform` to reproject, `Force2D` to strip Z/M before 2D-only consumers.
- Predicate translation is the lane's collapse: the geometry model's own algebra (`Intersects`, `Contains`, `Distance`, `Buffer`, set operations) translates to indexed spatial SQL — one spatial vocabulary executes in-process (construction, interchange) or in-store (filtering, joins) depending on where the expression sits. No separate "query geometry" dialect exists.
- Index law: `HasMethod("gist")` on geometry columns; bounding-box prefilters are implicit in GiST predicate translation — hand-written envelope pre-checks are a rejected re-derivation.
- KNN ordering rides the dedicated nearest-neighbor member (`DistanceKnn` → `<->`), index-ordered only as the outermost `ORDER BY ... LIMIT`; wrapped in arithmetic or post-filtered it degrades to exact evaluation.
- SRID discipline: mixed-SRID operations fault server-side; one factory per reference system declared at admission makes SRID a type-level invariant in practice, and `Transform` is the only sanctioned reprojection seam.
- Geography cost edge: geodesic predicates cost multiples of planar — the standard composition is geography for storage truth plus a stored generated planar projection column for hot planar predicates (the generated-column law composing into this lane: stored, because indexed).
- Spatial aggregates translate through the lane's aggregate plugin — collect/union folds over grouped geometries run server-side, so dissolve-style operations never materialize member geometries client-side.
- The lane's convention plugin contributes the spatial extension declaration to the model automatically when the plugin is admitted — one of the three admission parts arrives by convention, and removing the plugin removes the declaration: admission and schema declaration cannot drift apart.
- Wire projections (`AsText`/`AsBinary`) exist on the geometry surface for interchange seams; inside the store profile geometry travels as geometry — text/binary projections at query boundaries are interchange concerns, not query vocabulary.

## text lanes — full-text, trigram, citext

- The full-text spine is three declarations: a stored generated `tsvector` column (`IsGeneratedTsVectorColumn(config, props)`), a GIN index (`HasMethod("gin")`), and match predicates (`Matches(...)`).
- The helper declares the unweighted multi-column vector; per-field weighting takes an explicit computed-column expression composing weight assignment per field — the weighted spine is one expression in the model, and `Rank` with a weight vector consumes it at query time.
- The expression-index alternative (`IsTsVectorExpressionIndex(config)`) indexes the vector computation without materializing a column — zero heap cost, but every matching predicate must repeat the exact expression to hit the index.
- The spine chooser: generated column where the vector is queried from more than one shape or ranked (rank needs the vector value); expression index for single-shape match-only lanes on write-heavy tables.
- Write-heavy FTS tunes the GIN pending list: the fast-update mechanism batches index insertions into a pending list merged later — write throughput up, read variance up — and the pending-list limit is a per-index storage parameter (`HasStorageParameter`) with cleanup riding the engine's vacuum daemons; read-latency-critical lanes turn fast-update off and pay per-write index cost.
- The text-search config name is part of the column's identity — changing it is a rewrite migration; config choice is schema policy, never query policy.
- Query-side parser ladder: `WebSearchToTsQuery` for raw user input (never throws, supports quoted phrases and negation); `PlainToTsQuery` for bare conjunction; `PhraseToTsQuery` for ordered phrase; `ToTsQuery` full syntax only for machine-built expressions — the full parser throws on malformed input, so user text is admitted through the web-search row exclusively.
- Relevance is translated end-to-end: `Rank` with optional weight vectors and normalization policy, weighted vectors via `SetWeight` (weight classes are the per-field relevance lever, set inside the generated column expression), headline extraction via `GetResultHeadline`, and vector surgery (`Concat`, `Delete`) for composed-document vectors.
- Machine-built query composition is also translated (`And`, `Or`, `ToNegative`, `ToPhrase` with distance, `Contains`/`IsContainedIn` over query values) — programmatic query assembly happens in the typed vocabulary, never by string-concatenating query syntax.
- Trigram lane: the similarity family translates fully — boolean gates (`TrigramsAreSimilar`, word and strict-word variants), scores (`TrigramsSimilarity`, word forms), and distances for ordering.
- The non-obvious trigram capability: a trigram GIN opclass (`HasOperators("gin_trgm_ops")`) accelerates arbitrary infix `LIKE`/`ILike` — which no B-tree can serve; it is the only index answer to leading-wildcard search.
- Trigram thresholds are server settings, not per-query knobs in the translated surface — where threshold control matters, order by similarity distance instead of filtering on the boolean.
- The phonetic/edit-distance family is a fourth translated text row (`FuzzyStringMatchLevenshtein` with per-operation costs, bounded `FuzzyStringMatchLevenshteinLessEqual`, `FuzzyStringMatchSoundex`/`Difference`, `FuzzyStringMatchMetaphone`/`DoubleMetaphone`): exact edit-distance and phonetic equivalence for short identifiers — name deduplication, code matching — where trigram similarity is too coarse.
- The bounded form is the only scalable spelling: the cost-capped variant short-circuits beyond the maximum distance, turning an O(mn) per-row computation into a cheap reject for most rows — unbounded edit distance in a hot predicate is the rejected form.
- Edit-distance functions have no index; the scaling pattern is trigram-prefilter-then-exact: a trigram gate narrows candidates index-served, the bounded edit distance ranks survivors — two text rows composing in one query.
- Chooser inside the text family: full-text for language-aware relevance over documents; trigram for identifiers, names, typo tolerance, and infix matching; phonetic/edit-distance for short-string equivalence ranking after a narrowing gate; bare `ILike` only over already-narrowed sets.
- citext makes case-insensitivity a column property — correct for keys and case-insensitive uniqueness; redundant for search (both search lanes are already case-insensitive).
- citext columns index as ordinary B-trees — the case folding lives in the type's comparison, so unique constraints, joins, and equality predicates need no expression wrapping anywhere.
- The rejected form across all text rows: `lower()`-normalized shadow columns and client-side filtering.
- Hybrid retrieval — the text × vector fusion: lexical top-k (rank-ordered) and semantic top-k (distance-ordered) as two CTE arms over their respective indexes, fused by reciprocal-rank in one statement.
- Rank fusion is a fold over two ordered sets and belongs in SQL where both orderings are index-served; merging two client-fetched lists is the rejected spelling, paying two round trips to lose index order.

## hierarchy, kv, and array lanes

- ltree is a mapped path value type with translated ancestry (`IsAncestorOf`/`IsDescendantOf`), pattern matching (`MatchesLQuery`, `MatchesLTxtQuery`), and path algebra (`Subtree`, `Subpath`, `Index`, `LongestCommonAncestor`); lquery wildcards cover depth-bounded subtree queries.
- GiST-indexed ancestry tests replace recursive CTEs for read-heavy hierarchies; the trade is write-side path maintenance — subtree moves rewrite descendant paths.
- ltree indexes split by predicate: GiST serves ancestry and lquery pattern matching; plain B-tree serves path equality and ordering — a hierarchy column used both ways carries both, and the GiST-only reflex forfeits cheap equality.
- Hierarchy chooser: read-dominant taxonomies take ltree; write-heavy graphs keep recursive CTEs over adjacency (a typed-SQL resident).
- hstore is superseded vocabulary: flat string-to-string maps with existence/containment operators, admitted only when an upstream contract emits it — jsonb owns the document and kv concern otherwise.
- intarray adds dense int-array set algebra with its own GIN opclass — a niche accelerator for tag-id sets where jsonb containment is heavier.
- intarray's opclass is a choice against the core array opclass on the same column type — declaring it changes which operators the index serves; mixing intarray operators with a core-opclass index (or the reverse) silently forfeits the index, the lane's local opclass-agreement instance.
- cube provides n-dimensional boxes with translated algebra (`NpgsqlCube` mapping) — low-dimensional bounding volumes and parameter-space ranges; distinct from the vector lane (similarity) and the geo lane (georeferenced space).
- tablefunc (crosstab pivots) has no translated or mappable surface — a pure typed-SQL resident whose presence matters only to verification.
- Lane-overlap law — the family's unifying rule: arrays, jsonb, hstore, intarray, ltree, and trigram all answer membership/containment over irregular data; the chooser is the dominant predicate vocabulary: ordered scalars → array; documents → jsonb; paths → ltree; string fuzz → trigram; legacy flat string kv → hstore; int-set algebra at scale → intarray.
- Declaring two lanes for one column family is the rejected form; minority predicates ride expression indexes on the same column rather than a second column in a second vocabulary.

## partition lane

- Native partitioning is the only substrate (range | list | hash); trigger-routed partitioning is dead weight the native router replaced.
- The partition manager automates child lifecycle for time/serial ranges: registration (`create_parent(p_parent_table, p_control, p_interval, ...)` writing a config row), pre-creation (`premake` children ahead of now), retention (detach or drop beyond a horizon — `retention_keep_table` decides which), and a template-table mechanism propagating properties declarative partitioning does not inherit onto new children.
- The maintenance call (`run_maintenance` / its procedure form) is THE scheduler row: one idempotent call advances every registered config — creates due children, applies retention, detaches or drops.
- The call is safe early, late, or doubled; cadence at or above the smallest partition interval. Cadence ownership is settled elsewhere; this lane owns the row's content and its idempotence proof.
- The two automation variants map onto the provisioning axis: the SQL function (self side — callable by any session, including the suite scheduler) and the background-worker variant (operator side — preloaded, server-owned cadence).
- The in-database cron extension is the explicitly deleted third option: a preloaded singleton scheduler in one privileged database duplicating cadence ownership the suite scheduler already holds — operator-side burden, zero capability gain, and unportable across managed environments with closed preload lists.
- Partition-key law: the key must be inside the root's primary-key/unique constraints; global secondary uniqueness across partitions does not exist — uniqueness is per-partition plus key prefix.
- Pruning fires only on plannable key predicates: wrapping the key column in functions defeats pruning — the key column appears bare in hot predicates.
- Default-partition trap: a default partition catches out-of-range rows but taxes every later attach — attaching a new child must scan the default for rows belonging to the incoming range, under lock. Managed time-series lanes run without a default (the manager pre-creates ahead of now); a default partition is an unmanaged-ingress concession, declared as such.
- Hash partitioning is the no-lifecycle row: fixed modulus, no rotation, no retention semantics — write-spreading and per-partition maintenance parallelism only; it never enters the manager's config and answers a different question than range/list.
- Retention detach is concurrent-capable: the concurrent detach form avoids blocking queries on the parent during rotation — the retention row stays online-safe even on hot parents.
- Composition rows: per-partition vector/GiST/GIN indexes keep per-child index sizes inside build-memory budgets; detached retention children remain queryable archives feeding the export path; bulk COPY into the current child plus periodic maintenance is the steady-state time-series ingestion shape.
- Migration-coexistence edge: manager-created children are out-of-model relations — the mapped model knows the root only, and schema diffs must not reconcile children (runtime artifacts of the config, not model drift).
- The manager's config rows are schema-adjacent state traveling with environment provisioning — a verification-law observable, not application state.

## divergent — vector-lane

- The one vector law absorbing the family: storage is always full-precision `vector`; every cheaper representation is an expression index; every approximate result is budget-receipted.
- Three orthogonal policy axes fall out: index species (hnsw | ivfflat | none), representation (full | half | binary), scan policy (plain | iterative-strict | iterative-relaxed) — eighteen combinations from three rows, and the recall-cost frontier is walked by changing rows, never schema.
- The lane's typed degradation facts are exactly three — fact one: exact-baseline recall@k, the measurement every tuning claim rides.
- Fact two: scan-budget exhaustion — a max-tuples hit is truncation evidence, distinguishing "few matches exist" from "search gave up".
- Fact three: `ef_search`-vs-LIMIT floor violation — a configuration defect detectable statically from the lane's declared rows.
- Recall-cost trade row one: exact scan — recall 1.0, latency linear in rows, zero build cost.
- Row two: ivfflat — moderate recall at √-scaled probe cost, minutes-scale build, churn-degraded (static centroids).
- Row three: hnsw — high recall at log-scaled search, hours-scale build, churn-tolerant, slow vacuum.
- Row four: binary-prefilter+rerank — near-hnsw recall at a fraction of memory when dimensions are large.
- Workload chooser: read-heavy high-recall → hnsw; build-cheap static corpus → ivfflat; memory-bound high-dimension → quantized two-stage; correctness harness → exact.
- Foreclosed: client-side similarity over fetched embeddings — forfeits every index.
- Foreclosed: a second vector store beside the relational store — forfeits transactional consistency between facts and embeddings; the single-store law is the lane's reason to exist.
- Foreclosed: per-query scan-knob literals scattered in SQL — scan policy is a declared row per lane.
- Foreclosed: cosine over unnormalized vectors — normalize at admission, use inner product.
- Foreclosed: LIMIT above the search floor — silent truncation by construction.

## divergent — geo-text-lanes

- Capability-row unification: geo, full-text, and trigram are one shape at different vocabularies — admitted value type (geometry | tsvector | text), generated stored projection where derivation is needed (planar shadow | generated tsvector | none), one index-method row (gist | gin | gin+trgm-ops), one translated predicate family, and one ordering member that is index-served only at the outermost ORDER BY (KNN distance | rank | similarity distance).
- The table is the law — closed over cell content, open over rows: a future lane fills four cells. Temporal ranges already fit (range column, GiST, overlap predicates, no special ordering), proving the row-shape generalizes.
- Earned cross-lane interactions: geography storage × generated planar projection (geo × generated-column law); tsvector and trigram coexisting on one text column via two indexes chosen per predicate — not a duplicate-lane violation because the predicates differ; hybrid lexical-semantic fusion (text × vector, one statement, two index-served arms); range × partition — partition by time with WITHOUT-OVERLAPS constraints inside partitions, noting global overlap exclusion across partitions does not exist (the per-partition uniqueness law's range twin).
- The ordering-member law generalizes across all three lanes and the vector lane: every distance/rank/similarity member is index-served only as the bare outermost sort key — one rule, four lanes, and every "index ignored" diagnosis starts there.

## divergent — partition-provisioning

- The provisioning axis at full depth is a four-rung ladder, not a binary: (1) migration-declared — `CREATE EXTENSION` in the migration artifact, default for SQL-only lanes; (2) seed-declared — extension-adjacent state (manager config rows) created by idempotent migration-adjacent seeding; (3) operator-declared — preload lists, background workers, server settings: runbook artifacts the process never touches; (4) environment-given — managed-service allowlists where even operators only toggle.
- Every capability row above maps to exactly one rung, and the verification law reads the ladder top-down: absence at rungs 1–2 is a deployment defect (the artifact should have created it); absence at rungs 3–4 is a degradation row (fold the capability out, receipt the fold).
- Foreclosed: process-runtime `CREATE EXTENSION` (couples runtime privilege to DDL rights — the verification-only law's reason); trigger-maintained partition routing (native routing made it dead); in-database cron (cadence duplication); hand-rolled child-table creation in application code (the manager's config is the single source of child policy — bespoke creation forks it).
- The deepest cross-cut: partitioning is the only lane that changes other lanes' index economics — per-child index sizes, per-child build memory, prune-then-ANN ordering.
- Sizing law: partition when rows × per-row index cost exceeds maintenance memory for any single lane's index, or when retention is bulk-structural (drop a child) rather than row-wise.
- Retention-by-detach is O(1) and vacuum-free — the strongest single argument for the lane, and the reason time-series stores partition before they shard.
