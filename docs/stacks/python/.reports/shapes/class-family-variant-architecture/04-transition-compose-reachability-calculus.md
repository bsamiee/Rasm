# Transition Compose Graph Reachability And Concurrency Calculus

# Sparse Graph Materialization At Root Import

- Materialize `TRANSITION_GRAPH: Mapping[Kind, frozenset[Kind]]` as adjacency projection of declared `TRANSITION_ROWS` keys — edge `(a, b)` exists iff row `(a, b)` present; absent edge means forbidden hop, not implicit identity.
- Materialize `REACHABLE: Mapping[Kind, frozenset[Kind]]` as transitive closure over `TRANSITION_GRAPH` via Floyd-Warshall or repeated squaring on `Kind` enum width — closure computed once at root import, not per request; width equals closed union enum cardinality, not Cartesian handler count.
- Materialize `LAWFUL_CHAINS: Mapping[tuple[Kind, Kind], tuple[Kind, ...]]` storing one canonical path per reachable `(source, target)` pair — canonical path is shortest hop count; tie-break by lexicographic `Kind.name` sequence when multiple shortest paths exist.
- `UNREACHABLE_PAIRS: frozenset[tuple[Kind, Kind]]` is complement of `REACHABLE` key set over `Kind × Kind` — contract tests assert `transition(member, target)` on unreachable pair returns `InvariantFault` without attempting partial hops.
- Graph materialization imports `Kind` from vocabulary owner only — parallel string adjacency built from wire tags or comment-maintained lists is merge blocker; graph rebuild runs on every root import when transition table or enum membership changes.
- Adding transition row triggers graph rebuild in same root promotion unit as row parity tests — partial row without graph refresh leaves stale `REACHABLE` until import, caught by import-time parity gate comparing edge count to row count.

# Multi-Hop Compose Gate And Chain Witness

- Single-hop `transition(member, to_kind)` resolves row `(from_kind, to_kind)` directly — multi-hop `transition_chain(member, path: tuple[Kind, ...])` requires `path[0]` matches source arm kind and each consecutive pair is edge in `TRANSITION_GRAPH`.
- `transition_to(member, target_kind)` resolves canonical path from `LAWFUL_CHAINS[(from_kind, target_kind)]` when pair reachable — caller does not supply intermediate kinds unless explicit path override row documents non-canonical route policy.
- Compose associativity on lawful triple `(a, b, c)` holds when edges `(a,b)` and `(b,c)` exist — `transition(transition(m, b), c)` equals `transition_chain(m, (a, b, c))` up to successor slot law declared per row metadata; default tests treat order significant unless row declares commute flag from transition row metadata.
- Failed hop mid-chain returns `InvariantFault` naming failed edge, completed prefix path, and source member arm — no silent rollback to source member unless row metadata declares compensating undo row on same path prefix.
- Chain witness record `TransitionChainWitness` carries `path`, `row_ids`, `successor_ids` — audit and automation replay attribute multi-hop modality changes without re-deriving path from ad hoc logs.
- Identity hop `(k, k)` participates only when explicit same-arm row exists — `transition_chain` with length-1 path `(k,)` routes through enrichment replacement axis when no `(k,k)` row, not through transition lookup.

# Hierarchical Product-Path Composition

- Outer-only multi-hop chains use outer `(from_kind, to_kind)` edges only — inner sub-owner kinds preserved across hops when each row metadata declares `inner_preservation=full` or documents mandatory reset per hop.
- Product-path edges keyed `(outer_from, outer_to, inner_from, inner_to)` compose when middle outer and inner kinds align — `(PAY, PAY, CARD, WALLET)` followed by `(PAY, PAY, WALLET, BANK)` requires first hop `inner_to` equals second hop `inner_from`; mismatch returns `InvariantFault` at compose gate before second row handler runs.
- Sub-owner reachability graphs materialize independently beside sub-owner `TRANSITION_ROWS` — top-level `REACHABLE` does not embed inner kinds; cross-level chain resolves outer segment on top graph then inner segment on sub-field graph after outer narrow.
- Bundled atomic product rows from transition catalog supersede sequential outer-only hops when metadata `atomic=true` — chain API rejects decomposed two-hop substitute when atomic row exists for same `(source, target)` product key quadruple.
- Hierarchical `transition_to` on top `Member` may delegate to sub-owner `transition_to` on sub-field when outer kinds stable and inner pair reachable — delegation publishes at root as two-stage API mirroring registry lookup from .

# PEP 695 Specialization And Adapter Cache Keys

- Generic `Member` owners with literal-indexed arms require transition row handlers specialized per `(from_literal, to_literal)` tuple when replace policy depends on type argument — root warm-up caches `TransitionHandler[from_lit, to_lit]` beside `TypeAdapter` specialization keys noted in assigned doctrine owner
- Cache key tuple `(owner_module, from_kind, to_kind, type_args_hash)` materializes at import — hot paths do not re-resolve generic replace handlers via unchecked `getattr` on tag strings.
- Specialization width equals declared literal band on vocabulary table — orphan specialization without matching `Kind` member fails import parity; adding literal alias updates specialization cache in same promotion unit as vocabulary row.
- Non-generic owners use `(owner_module, from_kind, to_kind)` cache key only — do not allocate generic cache slots on monomorphic families.

# Concurrency Epoch Rows And Rich-Owner Cache Law

- Every transition row admits optional `concurrency_posture` column: `{immutable_successor, lock_per_owner, epoch_bump, contextvar_lane}` — default `immutable_successor` when row omits column; rich-owner rows with shared class memo set non-default posture explicitly.
- `immutable_successor` — parallel tasks may hold distinct successor bindings; shared binding mutation without snapshot is compose defect; row assumes replace returns fresh instance with no shared mutable cache slots aliasing source.
- `lock_per_owner` — rich owner serializes transition entry on owner instance lock; parallel transition requests on same owner queue; distinct owner instances proceed concurrently when cross-field law permits.
- `epoch_bump` — row increments owner epoch counter on success; readers of class-scoped memo consult epoch before cache hit — aligns with rich-owner graduation doctrine; epoch mismatch returns typed stale-read fault at owner accessor, not silent old cache value.
- `contextvar_lane` — request-scoped transition side effects publish through `ContextVar` token; parallel tasks isolate successor visibility without global lock when row metadata declares lane id registered at root.
- Free-threaded PEP 779 interleave on import-published `frozendict` registry tables forbids lazy dict mutation during transition — transition handlers must not populate module-level handler maps on first chain hop under parallel workers.
- Block batch multi-hop uses same posture as single-hop row on each element — mixed posture across elements in one batch is rejected; batch row metadata declares uniform posture or batch decomposes to sequential single-element chains.

# Result Rail Accumulation And Fault Composition

- `transition_chain` returns `Result[Member, InvariantFault]` — first failed hop short-circuits; fault evidence includes `failed_edge`, `completed_prefix`, `requested_target`.
- Recoverable sub-variant rows from assigned doctrine owner propagate only on final hop unless row metadata declares recoverable intermediate — default terminal fault on any forbidden edge.
- `Result` bind over chain steps requires uniform error type — heterogeneous per-hop fault shapes force port-local fault union promotion at boundary, not widened `Exception` chain.
- Successor member after full chain passes through ingress registry lookup independently — chain success does not imply registry row exists for final arm.

# Block Option And Batch Reachability

- `Block[Member]` homogeneous chain requires all elements share same `from_kind` and target path reachable for each — first element failing reachability fails entire batch with index in fault evidence unless batch row declares sparse per-element policy.
- `Option[Member]` chain applies on `Some(member)` only — `Nothing` returns absence fault or skips per policy row; no default arm coercion to source kind for reachability test.
- Filtered narrowed block after element-wise `TypeIs` may chain under path keyed on proved arm — filter-then-chain without proof is same defect as filter-then-cast without element proof.
- Batch canonical path selection uses same `LAWFUL_CHAINS` as singleton — mixed target kinds in one batch rejected at API gate before graph lookup.

# Proof Obligations And CI Gates

- `PO-TC-01` — edge count in `TRANSITION_GRAPH` bijects with `TRANSITION_ROWS` key count.
- `PO-TC-02` — every `REACHABLE` pair has entry in `LAWFUL_CHAINS` with path length equal shortest-hop metric.
- `PO-TC-03` — associativity enumeration on all lawful `(a,b,c)` triples from graph passes — precomputed triple list per concept, not global dense enumeration when enum width exceeds practical CI budget; sample stratified by hop count.
- `PO-TC-04` — unreachable pair contract tests assert `InvariantFault` without partial mutation on source member.
- `PO-TC-05` — hierarchical product-path compose tests cover implicated quadruples from cross-field guard metadata only — Cartesian budget from decomposition lattice, not full outer×inner product.
- `PO-TC-06` — concurrency posture column present on every rich-owner transition row touching shared class memo — import fails when posture omitted on flagged rows.
- `PO-TC-07` — specialization cache key count matches generic literal band width when owner declares type-parameterized transition handlers.
- Static: transition chain lookup `match` mirrors single-hop exhaustiveness — impossible kinds after narrow hit `assert_never`.
- Mutation: Stryker on `transition_to` must kill mutants routing through forbidden intermediate edge or skipping epoch bump on flagged rows.
- Property: lawful chain length equals shortest-path length; successor `to_kind` equals requested target; compose of two-hop canonical segments equals four-hop canonical only when graph confirms no shorter shortcut exists.

# Hypothesis And Metamorphic Targets

- Strategies sample source arm from registry exemplar per reachable `from_kind` — target kind from `REACHABLE[from_kind]` via `st.sampled_from`; unreachable targets appear only in negative-path modules.
- Chain properties assert one law per module: shortest-path totality, mid-hop fault, associativity on sampled triples, or epoch bump on concurrent rich-owner fixtures — do not merge chain, registry, and egress metamorphic proofs.
- Shrinking on chain failures rebuilds lawful source arms — illegal kind insertion during shrink fails at construction gate.
- Metamorphic: `transition_to` then project then wire-decode equals project on final successor when row commute metadata absent — order significant by default per transition row catalog.

# Composition Root Chain Wiring

- Root publishes `transition(member, to_kind)`, `transition_to(member, target_kind)`, and `transition_chain(member, path)` beside single-hop transition lookup — domain modules import `Kind` tokens only; graph tables and chain APIs stay at root.
- `TRANSITION_GRAPH`, `REACHABLE`, and `LAWFUL_CHAINS` materialize in root import block immediately after `TRANSITION_ROWS` parity check — graph symbols are read-only frozensets and tuples, not mutable caches updated by handlers.
- Chained pipelines insert `transition_to` after enrichment and before wire egress when outbound policy requires modality shift — ordering row ids reference materialization handoff lattice; chain inserted mid-pipeline without ordering metadata is merge blocker.
- Catalog `Bind` rows embedding chain-capable handlers pin allowed target kinds to subset of `REACHABLE[from_kind]` for each bound arm — surplus CLI tokens selecting unreachable target fail at root guard before graph lookup.
- HTTP and queue carriers do not fork graph policy per route — all carriers call same root chain entrypoints after smart constructor materializes source `Member`.
- Import-cycle resolution for graph modules belongs at root — family owner modules export `Kind` and row metadata; graph builder imports rows from owner without family importing graph symbols.

# Chain Fault Routing And Boundary Envelope

- Mid-chain `InvariantFault` carries `failed_edge: tuple[Kind, Kind]`, `completed_prefix: tuple[Kind, ...]`, `requested_target: Kind`, and `source_from_kind: Kind` — fault shape mirrors single-hop transition fault with path evidence for automation replay.
- Root `Envelope` distillation on chain failure preserves terminal requested target and completed prefix kinds — truncation drops verbose slot diffs per hop, not kind identity tokens required for replay attribution.
- Cross-field guard failure on hop `i` returns fault scoped to row `cross_field_guard` id — prior successful hops do not roll back successor bindings unless undo row metadata declares compensating path; default leaves source member unchanged when hop `i` fails before any mutation.
- Recoverable chain faults — deferred modality on intermediate hop — stay port-local until promotion policy requires shared domain error family; default terminal fault on first forbidden edge.
- Registry lookup on partial chain success is forbidden — chain either completes all hops returning final successor or fails before exposing intermediate successor to ingress registry handlers unless row declares observable intermediate state policy.

# Semi-Closed Extension And Graph Nodes

- `Extension` arm participates in `TRANSITION_GRAPH` only when explicit rows declare extension as source or target node — unknown extension payload does not auto-create edges to all closed arms.
- Reachability from closed arm to `Extension` requires declared row and root-documented capture policy from transition row catalog — graph edge alone does not authorize capture; row plus policy row both present.
- Reachability from `Extension` to closed arm routes through promotion fold referenced by row `re_ingress_cross_ref` — graph path may list edge but handler invokes contravariant admission, not covariant skip.
- Extension-to-extension hops preserve extension shell when row metadata `extension_shell=preserve` — inner closed cores inside extension payload follow sub-owner graphs independently.
- Semi-closed unknown outer tag captured as `Extension` does not inherit reachability from closed subgraph — extension node reachability computed only from rows explicitly listing `Kind.EXTENSION` endpoints.

# Registry Ingress And Egress After Chain

- Successful `transition_to` returns fresh `Member` successor — ingress registry lookup on successor runs independently with exhaustive `match` on final arm; chain completion does not bypass construction gate on successor fields mutated by last row.
- Wire egress after chain encodes successor member once — pre-chain wire bytes are invalid after modality change; egress registry reads successor discriminant from vocabulary table final arm row.
- Endofold chain followed by enrichment uses replacement axis on successor only — enrichment on pre-chain member after successful partial chain is defect unless undo row restored source binding.
- Chained transition registry rows are distinct from ingress handler rows — same `(from_kind, to_kind)` may appear in both tables with different handler symbols; graph edges derive from transition table only, not ingress registry width.
- Round-trip proof after chain constructs source arm, runs `transition_to`, projects successor to wire, decodes, smart-constructs, asserts equality on terminal discriminant — metamorphic samples use vocabulary table terminal arm exemplars.

# Graph Cycle Strong Component And Idempotence Law

- Directed cycles in `TRANSITION_GRAPH` are permitted when business law admits revisiting modality — cycle detection does not reject graph build; `transition_to` shortest-path selection avoids cycle unless no acyclic path exists.
- Strong components with mutual reachability expose multiple canonical paths — tie-break by lexicographic `Kind.name` on shortest acyclic path when acyclic path exists; pure cycle-only reachability requires explicit policy row naming allowed cycle traversal count.
- Idempotent row `(k, k)` creates self-loop edge — reachability includes `k` in `REACHABLE[k]` via zero-hop identity only when policy declares reflexive reachability; default graph reachability uses path length ≥1 unless reflexive row metadata set.
- Chain API rejects paths that repeat same kind with no declared self-loop edge — `transition_chain(m, (A, B, A))` requires edges `(A,B)` and `(B,A)` both present; accidental infinite handler loops without graph edge are negative fixtures.
- Deprecation subgraphs during migration window may form temporary two-node cycles between deprecated and replacement kinds — sunset metadata on rows documents cycle teardown date; graph rebuild after arm deletion removes cycle edges in same promotion unit.

# Transition Chain Catalog Examples

- `pending_to_settled_via_processing`: canonical path `(PENDING, PROCESSING, SETTLED)` when edges `(PENDING, PROCESSING)` and `(PROCESSING, SETTLED)` declared — `transition_to(pending_member, SETTLED)` resolves three-node witness without caller supplying `PROCESSING`.
- `card_to_wallet_to_bank`: hierarchical product path when atomic row absent — outer `(PAY, PAY)` stable, inner `(CARD, WALLET)` then `(WALLET, BANK)`; compose gate verifies inner alignment at middle hop.
- `extension_promote_single_hop`: edge `(EXTENSION, CARD)` with `validated_rebuild` policy — graph lists direct edge; chain length one; promotion fold runs inside row handler not multi-hop decomposition.
- `forbidden_crypto_from_cash`: pair `(CASH, CRYPTO)` absent from `REACHABLE` — contract test asserts `transition_to(cash_member, CRYPTO)` returns `InvariantFault` before any row handler executes.
- `rich_owner_epoch_row`: `(ACTIVE, SUSPENDED)` with `concurrency_posture=epoch_bump` — concurrent transition tests assert memo readers observe bumped epoch after success.

# Vocabulary And Contract Integration

- Graph edges reference `Kind` members from vocabulary table — adding edge without vocabulary row occurs only in same promotion unit as new arm from vocabulary promotion unit.
- Contract tests join `TRANSITION_GRAPH` keys against `Kind` iteration — orphan kinds in graph or enum members unreachable in any chain when policy expects connectivity fail import gate.
- Wire egress after full chain uses final successor arm vocabulary row — chain tests include round-trip on terminal arm, not only source arm.
- OpenAPI publication unaffected by reachability tables — operational metadata beside owner unless API documents allowed modality sequences as separate operation schema enumerating lawful target kinds from `REACHABLE`.

# Anti-Patterns

- Dense `Kind × Kind` transition table with no-op rows to simulate graph — sparse law requires absence means forbidden; reachability precomputes from sparse rows only.
- Per-request Floyd-Warshall on hot path — closure materializes at import; runtime path search without cached `LAWFUL_CHAINS` is merge blocker.
- Caller-supplied arbitrary kind paths bypassing `TRANSITION_GRAPH` edge check — explicit path override requires documented policy row.
- Flat concatenated string path keys `"a.b.c"` spanning outer and inner vocabularies — use product-path composition or sub-owner graphs from assigned doctrine owner.
- Parallel transition on shared rich-owner instance without posture column when class memo exists — stale cache reads until epoch or lock row declared.
- Generic transition handler resolved by raw tag string at runtime — specialization cache keys required for literal-indexed owners.
- Mid-chain silent identity fallback when edge missing — fail closed with `InvariantFault` naming failed edge.
- `transition_chain` invoking migration fold on intermediate hop — migration belongs at read boundary; graph edges operate on current `Member` only.

# Collapse Tests

- Collapse handler loops calling single-hop transition until target reached without graph witness to `transition_to` with cached canonical path and chain witness record.
- Collapse runtime BFS path discovery duplicated in routes to root-import `LAWFUL_CHAINS` materialization.
- Collapse parallel transition on shared owner without epoch or lock row to explicit `concurrency_posture` on rich-owner transition rows.
- Collapse string-built multi-hop kind lists in domain modules to root-published reachability API accepting materialized `Member` and target `Kind` only.
- Collapse generic handler dict keyed by wire tag to specialization cache keyed by `(from_kind, to_kind, type_args_hash)`.
- Done when every reachable `(from_kind, to_kind)` pair resolves canonical path at import, every unreachable pair fails closed before mutation, hierarchical chains compose through product-path or two-stage delegation, concurrency posture declared on all rich-owner transition rows, and CI graph parity gates pass before behavioral suites.

# Rich Owner Chain Graduation And Owner Method Delegation

- Rich owner exposing `Member` as nested field publishes `transition_to(target_kind)` method returning `Result[Owner, E]` — interior union chain runs through owner method; exterior API stays one owner type without public per-arm chain helpers.
- Owner method resolves graph path on nested `Member` then applies arm-aware replace on owner shell — owner caches invalidate per row `concurrency_posture` before successor owner returns to caller.
- Hierarchical rich owner with sub-owner fields composes chain on sub-field when outer kinds stable — owner method delegates to sub-owner `transition_to` then re-runs cross-field law once on recomposed owner.
- Smart constructor on owner after chain re-validates cross-field law when any hop row touches sub-owner fields implicated by root guards — law runs once in owner method, not per hop inside row handlers.
- Graduation from frozen `Member` to rich owner preserves graph tables keyed on `Kind` — owner symbol change does not remap graph edges; row `owner_symbol` updates in same promotion unit as graduation.

# Import Arch Mutation And Doctrine Closure

- Import-linter flags domain modules importing `TRANSITION_GRAPH`, `REACHABLE`, or `LAWFUL_CHAINS` — graph symbols are root-published only; family modules export row declarations and `Kind`, not materialized closure.
- Arch rule: graph builder module imports transition rows read-only — graph module must not define competing transition handlers or duplicate row keys.
- Arch rule: interior handlers never call `transition_chain` with path tuples assembled from wire tag strings — target kind and optional override path id come from typed request structs at root boundary.
- Stryker jointly scores graph builder and `transition_to` when mutants drop edges from adjacency without removing rows — kill depends on `PO-TC-01` parity and unreachable-pair tests, not silent wrong-path success.
- Ruff policy bans runtime `networkx` or ad hoc BFS in handler modules for path discovery — graph materialization uses stdlib-only enum-width closure at import.
- Mutation on `LAWFUL_CHAINS` tie-break branch must fail canonical path contract tests when alternate equal-length path exists — mutants picking non-lexicographic path without override row fail CI.
- `beartype` on `transition_to` and `transition_chain` entrypoints validates live source `Member` and tuple path kinds against vocabulary enum — tampered path kinds caught after static proof gaps.
- Doctrine closure: sparse rows expressible as import-time graph with bijective edge-row mapping.
- Doctrine closure: every reachable pair resolves canonical shortest path without runtime search.
- Doctrine closure: hierarchical and generic owners share chain API surface at root with independent subgraph or specialization cache where required.
- Doctrine closure: concurrency posture declared on all rich-owner transition rows touching shared memo before parallel CI fixtures run.
- Doctrine closure: proof obligation register `PO-TC-01` through `PO-TC-07` passes in CI order without suppressions.
- Doctrine closure: duplicate Floyd-Warshall, product-path compose, and concurrency posture prose at the composition root is a merge blocker.

