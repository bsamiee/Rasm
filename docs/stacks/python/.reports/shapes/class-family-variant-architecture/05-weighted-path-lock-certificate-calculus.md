# Weighted Path, Lock-Order DAG, And Transition Certificate Calculus

# Weighted Shortest-Path Cost Tables

- Lexicographic `Kind.name` tie-break from reachability calculus is syntactic default only — when multiple shortest paths share hop count, `PATH_COST_ROWS: Mapping[tuple[Kind, Kind], int]` assigns semantic cost per edge `(from_kind, to_kind)` derived from cross-field guard severity, audit tier, or recoverable-fault probability documented on transition row metadata.
- Materialize `WEIGHTED_REACHABLE: Mapping[Kind, Mapping[Kind, int]]` at root import alongside `REACHABLE` — Dijkstra or Bellman-Ford on `TRANSITION_GRAPH` with edge weights from `PATH_COST_ROWS`; absent weight defaults to `1` per hop; unreachable pairs remain absent from inner map.
- Materialize `CANONICAL_COST_PATHS: Mapping[tuple[Kind, Kind], tuple[Kind, ...]]` storing minimum-cost path per reachable pair — when cost ties, secondary tie-break remains lexicographic `Kind.name` on path suffix; tertiary tie-break uses stable row `priority: int` on competing edges.
- `transition_to(member, target_kind, cost_policy=semantic)` selects path from `CANONICAL_COST_PATHS` when semantic policy active — default `cost_policy=lexicographic` preserves prior reachability behavior for owners without cost rows; policy column on family vocabulary metadata declares active selector.
- Cost row shape: `from_kind`, `to_kind`, `base_cost: int`, `guard_severity_weight`, `audit_tier_weight`, `recoverable_fault_weight` — composite cost is sum of declared columns; negative weights rejected at import parity; zero-cost edges require explicit row documenting no-op modality semantics.
- Hierarchical product-path edges carry cost on quadruple row — `(outer_from, outer_to, inner_from, inner_to)` cost independent of outer-only segment costs; bundled atomic rows from assigned doctrine owner declare bundled cost overriding sum of decomposed hops when `atomic_cost_override` column set.
- Semi-closed `Extension` promotion hops carry elevated cost by default — promotion rows without explicit cost inherit `base_cost=10` sentinel until owner contract row pins business cost; graph reachability unchanged, path selection prefers closed-to-closed routes when targets tie on hop count.
- Contract tests assert `PO-WP-01`: every edge in `TRANSITION_GRAPH` has cost row or default weight; `PO-WP-02`: canonical cost path total equals minimum over all paths enumerated for `Kind` width ≤ practical CI budget; `PO-WP-03`: cost tie-break produces deterministic path id across imports.
- Anti-pattern: hand-maintained preferred path tuples in handlers bypassing `CANONICAL_COST_PATHS` — explicit path override requires `PATH_OVERRIDE_ROWS` keyed by `(source, target, override_id)` referencing business policy document id at owner.

# Cross-Owner Lock-Order DAG For Product Chains

- `concurrency_posture` columns govern single-owner memo behavior — multi-owner product-path chains require `LOCK_ORDER_DAG: Mapping[OwnerSymbol, int]` publishing finite rank per rich-owner symbol participating in any transition row touching shared class memo under policy (3).
- Lock symbol `L(owner_symbol, memo_site)` acquires in ascending `LOCK_ORDER_DAG` rank — release in reverse; never hold `L_b` while waiting on `L_a` when `rank(a) > rank(b)`; ranks are integers with gaps allowed for future owner insertion.
- Product-path chain touching outer rich owner `TransactionOwner` and inner sub-owner `PaymentMethodOwner` declares cross-owner rank row — default order: sub-owner memo locks (rank 10) precede outer owner memo locks (rank 20) when inner transition completes before outer shell replace; inverted order requires explicit `lock_inversion_justification_id` on row metadata.
- `transition_chain` on hierarchical member acquires locks in DAG order before first hop mutates shared memo — hop handlers assume locks held per row `lock_scope: {source_owner, target_owner, both}`; scope `both` requires both symbols' locks held through atomic product row only.
- Deadlock harness: parallel tasks running overlapping product-path chains must acquire locks in identical DAG order — CI fixture inverts acquisition on flagged pairs and asserts hang or typed `LockInversionFault`, not memo corruption.
- `ContextVar` lane (posture `contextvar_lane`) and lock policy (3) on disjoint memo sites of same owner remain lawful when row documents disjoint sites — DAG rank applies only when single task touches both sites in one chain; cross-owner DAG is separate from intra-owner site lattice from rich-owner doctrine.
- `LockInversionFault` carries `held_symbol`, `waiting_symbol`, `chain_id`, `failed_hop_index` — root envelope distillation maps to same automation namespace as `InvariantFault`; recoverable retry on lock inversion is rejected unless row declares queue policy at composition root.
- Import parity `PO-LO-01`: every rich-owner transition row with `concurrency_posture=lock_per_owner` or `epoch_bump` touching class memo has `OwnerSymbol` present in `LOCK_ORDER_DAG`; `PO-LO-02`: every pair of symbols co-occurring on product-path rows has comparable rank; `PO-LO-03`: no cycles in rank assignment.

# Transition Chain Certificate Schema

- `TransitionChainWitness` tuple from reachability calculus is audit spine only — `TransitionChainCertificate` is immutable msgspec struct or frozen dataclass with versioned schema: `certificate_schema_version: Literal["1"]`, `chain_id`, `source_from_kind`, `requested_target_kind`, `path: tuple[Kind, ...]`, `path_cost: int`, `hop_records: tuple[HopRecord, ...]`, `successor_member_hash`, `vocabulary_table_hash`, `graph_snapshot_hash`.
- `HopRecord` per hop binds: `hop_index`, `row_id`, `from_kind`, `to_kind`, `replace_policy`, `concurrency_posture`, `cross_field_guard_id`, `lock_symbols_held: frozenset[str]`, `predecessor_object_id`, `successor_object_id`, `predecessor_is_successor: bool`, `guard_severity_cost`, `commute_metadata_flag`.
- Certificate emits at root on successful `transition_to` or `transition_chain` completion — not on forbidden-pair faults; failed mid-chain emits `ChainFaultCertificate` with `failed_edge`, `completed_prefix`, `partial_hop_records`, same hash fields for replay attribution.
- `chain_id` is content hash of `(source_from_kind, requested_target_kind, path, vocabulary_table_hash, graph_snapshot_hash)` — stable across reserialization; worker handoff pins parent `chain_id` when successor member crosses process boundary.
- `successor_member_hash` is hash of canonical wire projection of final arm discriminant plus slot fingerprint declared on vocabulary row — not full payload dump; truncation-safe for automation envelope detail slots.
- `graph_snapshot_hash` pins `TRANSITION_GRAPH`, `REACHABLE`, and active cost table at import — vocabulary owner update without graph rebuild changes hash and invalidates stale certificates at verification gate.
- Verification fold is pure on certificate plus optional successor snapshot — checks path equals `CANONICAL_COST_PATHS` or documented override row, hop count matches path length, lock order compliance when policy (3), predecessor inequality at each hop, `Kind` tokens match vocabulary table iteration.
- Replay law: trusted re-invocation walks `hop_records` in order calling row `owner_symbol` methods named in certificate — shortcut `model_copy` where row declares `replace_policy=validated_rebuild` fails replay module; alias-only enum edit must leave certificate spine unchanged except hash slices.
- Automation envelope embeds certificate under `modality_evidence` slot with discriminant shared with egress fault namespace — consumers match on `chain_id` and `requested_target_kind`; truncated envelopes retain hash fields, drop verbose hop payloads.

# Certificate And Weighted Path Interleave

- Cost-aware `transition_to` appends `path_cost` and per-hop `guard_severity_cost` to certificate — OTEL export optional row lists `exported_path_cost` when observability kind permits medium cardinality per rich-owner doctrine.
- Path override row pins non-minimum-cost path when business policy mandates audit-heavy route — certificate records `override_id` and `path_cost` actual; verification compares against override row not `CANONICAL_COST_PATHS`.
- Multiple minimum-cost paths with identical total cost — secondary lexicographic tie-break on `Kind.name` sequence; certificate stores chosen path; mutation tests fail mutants selecting non-lexicographic equal-cost path without override row.

# Hierarchical And Generic Certificate Extensions

- Product-path certificates nest `inner_chain_certificate: TransitionChainCertificate | None` on hops where outer kinds stable and inner segment nontrivial — flat concatenation of outer and inner paths in one tuple rejected when sub-owner graph independent.
- PEP 695 generic owners add `type_args_hash: str` to certificate spine — specialization cache key `(from_kind, to_kind, type_args_hash)` from assigned doctrine owner must match certificate field; generic replay fails when type argument tuple drifted without vocabulary promotion.
- Sub-owner certificate emits independently when sub-field transition invoked standalone — top-level certificate references sub-certificate by `chain_id` foreign key only; merging sub-certificates into outer without explicit nest column is merge blocker.

# Composition Root Certificate Wiring

- Root publishes `transition_to`, `transition_chain`, `verify_certificate`, and `replay_from_certificate` beside graph symbols from reachability calculus — domain modules import `Kind` tokens only; certificate emitters and verification folds stay in root import graph.
- `CANONICAL_COST_PATHS`, `PATH_COST_ROWS`, and `LOCK_ORDER_DAG` materialize immediately after `LAWFUL_CHAINS` parity check — cost and lock tables are read-only frozensets and mappings, not mutable caches updated by handlers.
- Certificate emit hooks attach to successful chain completion before ingress registry lookup on successor — ordering metadata on pipeline rows references materialization handoff lattice; certificate inserted mid-pipeline without ordering id is merge blocker.
- Catalog `Bind` rows embedding chain-capable handlers pin `cost_policy` and allowed target kinds to subset of `WEIGHTED_REACHABLE[from_kind]` — surplus CLI tokens selecting unreachable or non-policy target fail at root guard before cost path lookup.
- HTTP, CLI, queue, and replay carriers share one certificate schema — route differences are carrier decode only; certificate `chain_id` namespace is per root invocation, not per transport.
- Import-cycle resolution for certificate modules belongs at root — family owner exports row metadata and `Kind`; certificate builder imports rows read-only without family importing certificate symbols.

# Block Option And Batch Certified Chains

- `Block[Member]` homogeneous `transition_to` requires uniform `from_kind`, reachable target path for each element, and uniform `concurrency_posture` across batch row — mixed posture within one batch rejected at API gate before lock acquisition.
- Batch certificate emits `BatchTransitionCertificate` wrapping per-element `TransitionChainCertificate` tuple plus `batch_id` content hash — partial batch success forbidden unless batch row declares sparse per-element policy from transition row catalog.
- `Option[Member]` certified chain applies on `Some(member)` only — `Nothing` returns absence fault or skips per policy row; certificate omits hop records on `Nothing` arm.
- Filtered narrowed block after element-wise `TypeIs` may chain under cost path keyed on proved arm — filter-then-chain without proof is same defect as filter-then-cast without element proof.
- Batch lock acquisition orders symbols by `LOCK_ORDER_DAG` once before element loop — per-element lock reordering inside loop violates DAG law and fails deadlock harness.

# Registry Ingress Egress After Certified Chain

- Successful certified chain returns fresh successor `Member` — ingress registry lookup on successor runs independently with exhaustive `match` on final arm; certificate success does not bypass construction gate on successor fields mutated by last hop.
- Wire egress after certified chain encodes successor once — pre-chain wire bytes invalid after modality change; egress registry reads successor discriminant from vocabulary table final arm row.
- Endofold chain followed by enrichment uses replacement axis on successor only — enrichment on pre-chain member after successful partial chain is defect unless undo row restored source binding per transition row catalog.
- Round-trip proof after certified chain constructs source arm, runs `transition_to`, verifies certificate, projects successor to wire, decodes, smart-constructs, asserts equality on terminal discriminant — metamorphic samples use vocabulary table terminal arm exemplars.
- Chained transition registry rows remain distinct from ingress handler rows — certificate `hop_records` reference transition row ids only; graph edges and cost tables derive from transition table, not ingress registry width.

# Chain Fault Routing And Boundary Envelope

- Mid-chain fault certificate preserves `failed_edge`, `completed_prefix`, `requested_target`, and `path_cost_accumulated` — fault shape extends single-hop `InvariantFault` with path evidence for automation replay attribution.
- Root `Envelope` distillation on chain fault preserves terminal requested target and completed prefix kinds — truncation drops verbose slot diffs per hop, not kind identity tokens or `chain_id` hash fields.
- `LockInversionFault` certificate variant carries `held_symbol`, `waiting_symbol`, and partial `lock_symbols_held` — distillation maps to same automation namespace as transition faults; recoverable retry rejected unless queue policy row at root.
- Cross-field guard failure on hop `i` returns fault certificate scoped to row `cross_field_guard_id` — prior successful hops appear in `partial_hop_records`; default leaves source member unchanged when hop `i` fails before mutation.
- Registry lookup on partial chain success forbidden — chain either completes all hops returning final successor plus success certificate or fails before exposing intermediate successor to ingress registry unless observable intermediate policy row declares otherwise.

# Hypothesis And Property Targets

- Strategies sample source arm from registry exemplar per reachable `from_kind` — target kind from `WEIGHTED_REACHABLE[from_kind]` via `st.sampled_from`; unreachable targets appear only in negative-path modules.
- Certificate properties assert one law per module: cost-path totality, verification fold acceptance on golden fixtures, lock-order compliance on concurrent fixtures, or nested certificate foreign-key compose — do not merge certificate, registry, and egress metamorphic proofs.
- Shrinking on chain failures rebuilds lawful source arms — illegal kind insertion during shrink fails at construction gate, not as accepted counterexamples.
- Metamorphic: verified certificate replay equals direct `transition_to` on sample arms when `cost_policy` matches — order significant when override row pins non-minimum-cost path.

# Transition Certificate Catalog Examples

- `pending_to_settled_semantic`: `(PENDING, PROCESSING, SETTLED)` path selected by minimum guard severity cost when `(PENDING, SETTLED)` direct edge exists but carries higher audit cost — certificate records `path_cost` and per-hop `guard_severity_cost`.
- `card_to_bank_multi_owner`: product-path chain acquires `PaymentMethodOwner` lock rank 10 before `TransactionOwner` rank 20 — certificate `lock_symbols_held` on inner hop documents both symbols when atomic row scope `both`.
- `extension_promote_certified`: single-hop `(EXTENSION, CARD)` with elevated default cost — certificate `path` length one; `re_ingress_cross_ref` on hop record; promotion fold runs inside row handler.
- `forbidden_crypto_fault_cert`: unreachable `(CASH, CRYPTO)` emits `ChainFaultCertificate` with empty `partial_hop_records` — contract test asserts no mutation on source member before fault emit.
- `worker_handoff_chain`: parent `chain_id` pinned in `WorkerHandoffRecord` extension on certificate — child process verification compares parent hash before local transition replay.

# Proof Obligations And CI Gates

- `PO-TC-08` — `CANONICAL_COST_PATHS` path cost equals `WEIGHTED_REACHABLE` minimum for sampled reachable pairs stratified by hop count.
- `PO-TC-09` — certificate verification fold accepts golden fixtures per declared row and rejects tampered `path`, `hop_records`, or hash fields.
- `PO-TC-10` — lock-order DAG complete for all multi-owner product rows; deadlock harness passes on flagged symbol pairs.
- `PO-TC-11` — hierarchical nested certificates verify independently and compose by doctrine reference.
- `PO-TC-12` — worker handoff certificate parent `chain_id` matches snapshot taken before cross-process encode.
- Static: certificate schema hash pins hop field set — transition row signature change without schema bump fails evolution hook.
- Mutation: Stryker on cost path selector must kill mutants routing through non-minimum-cost edge without override row.
- Property: certificate reserialization round-trip preserves `chain_id`; replay re-invocation equals direct `transition_to` on sample arms modulo object id fields.
- Arch: domain modules must not import certificate types or `LOCK_ORDER_DAG` — root-published verification entrypoints only.

# Anti-Patterns

- Runtime Dijkstra per request when `CANONICAL_COST_PATHS` materialized at import — merge blocker.
- Certificate as unstructured log string without versioned schema — automation consumers cannot verify replay.
- Multi-owner chain without `LOCK_ORDER_DAG` ranks on co-occurring symbols — deadlock under parallel CI.
- Flat `"owner_a.owner_b.hop"` lock keys — use `OwnerSymbol` ranks and scoped `lock_symbols_held` on hop records.
- Certificate omitting `graph_snapshot_hash` — stale graph replay after row addition passes verification incorrectly.
- Weighted path table duplicating dense `Kind × Kind` no-op grid — costs attach only to declared sparse edges.
- Inner certificate merged into outer path tuple as string concatenation — use nested certificate reference column.
- Mid-chain certificate emit on partial success exposing intermediate successor to registry — forbidden unless row declares observable intermediate policy from transition row catalog.

# Collapse Tests

- Collapse handler-preferred path tuples to `transition_to` with `cost_policy=semantic` and `CANONICAL_COST_PATHS` lookup.
- Collapse ad hoc lock acquisition order in product-path handlers to `LOCK_ORDER_DAG` ranks with deadlock harness.
- Collapse unstructured transition audit logs to `TransitionChainCertificate` emit at root on chain completion.
- Collapse lexicographic-only routing when cost rows present to semantic policy with deterministic tie-break documentation.
- Collapse generic transition replay without `type_args_hash` to specialization-aware certificate spine.
- Done when every reachable pair resolves minimum-cost canonical path at import, every multi-owner chain declares lock ranks, every successful multi-hop transition emits verifiable certificate, and CI gates `PO-TC-08` through `PO-TC-12` pass before behavioral suites.

# Automation Consumer Certificate Decode Law

- Automation consumers decode stdout through same module-level envelope decoder root uses for emit — pass-one logic validates `schema_version`, fault discriminant, and `chain_id` hash before interpreting modality evidence slots.
- Detail slots carrying `TransitionChainCertificate` projections narrow through discriminant tags shared with egress `tag_field` — consumer `match` uses same vocabulary encoder emitted; foreign synonyms mapped at root ingress and egress adapters only.
- When `truncated=True` on envelope, consumers resolve full certificate through history artifacts keyed by `chain_id` — wire line is index; truncated hop records are not complete modality audit trails.
- Consumer replay modules call root-published `verify_certificate` before trusting modality evidence — interior modules do not implement partial verification folds on certificate hash slices alone.
- Certificate schema version mismatch fails closed at consumer gate with `MigrationFault` on envelope — unknown certificate versions do not coerce to latest schema or default empty hop records.

# Semi-Closed Extension Cost Path And Graph Nodes

- `Extension` arm participates in weighted graph only when explicit rows declare extension as source or target node — unknown extension payload does not auto-create zero-cost edges to all closed arms.
- Reachability from closed arm to `Extension` requires declared row plus root-documented capture policy from transition row catalog — cost row alone does not authorize capture; row plus policy row both present.
- Reachability from `Extension` to closed arm routes through promotion fold referenced by hop `re_ingress_cross_ref` — certificate lists promotion hop with elevated cost; handler invokes contravariant admission, not covariant skip.
- Extension-to-extension hops preserve extension shell when row metadata `extension_shell=preserve` — inner closed cores inside extension payload follow sub-owner weighted graphs independently.
- Semi-closed unknown outer tag captured as `Extension` does not inherit minimum cost from closed subgraph — extension node costs computed only from rows explicitly listing `Kind.EXTENSION` endpoints.

# Rich Owner Certificate Graduation And Owner Method Delegation

- Rich owner exposing `Member` as nested field publishes `transition_to(target_kind)` returning `Result[Owner, E]` plus optional `TransitionChainCertificate` — interior union chain runs through owner method; exterior API stays one owner type without public per-arm chain helpers.
- Owner method resolves cost path on nested `Member`, acquires locks per `LOCK_ORDER_DAG`, then applies arm-aware replace on owner shell — owner caches invalidate per row `concurrency_posture` before successor owner and certificate emit return to caller.
- Hierarchical rich owner with sub-owner fields composes certified chain on sub-field when outer kinds stable — owner method delegates to sub-owner `transition_to` then re-runs cross-field law once on recomposed owner; nested certificate foreign key required on hop record.
- Smart constructor on owner after certified chain re-validates cross-field law when any hop row touches sub-owner fields implicated by root guards — law runs once in owner method, not per hop inside row handlers.
- Graduation from frozen `Member` to rich owner preserves cost tables and lock DAG keyed on `Kind` — owner symbol change does not remap graph edges; row `owner_symbol` and lock rank rows update in same promotion unit as graduation.

# Import Arch Mutation And Final CI Closure

- Import-linter flags domain modules importing `CANONICAL_COST_PATHS`, `PATH_COST_ROWS`, `LOCK_ORDER_DAG`, or certificate struct types — cost, lock, and certificate symbols are root-published only.
- Arch rule: certificate builder module imports transition rows and cost rows read-only — builder must not define competing transition handlers or duplicate row keys.
- Arch rule: interior handlers never assemble cost path tuples from wire tag strings — target kind and optional override id come from typed request structs at root boundary.
- Stryker jointly scores cost path selector and certificate verification when mutants drop cost row without removing edge — kill depends on `PO-WP-02` parity and certificate golden fixtures.
- Ruff policy bans runtime `networkx` or ad hoc Dijkstra in handler modules for cost path discovery — weighted closure uses stdlib-only enum-width materialization at import.
- Mutation on cost tie-break branch must fail canonical path contract tests when alternate equal-cost path exists — mutants picking non-lexicographic path without override row fail CI.
- `beartype` on `verify_certificate` and `replay_from_certificate` entrypoints validates live certificate kind tokens against vocabulary enum — tampered path kinds caught after static proof gaps.

# Vocabulary Table And Contract Proof Integration

- Cost rows reference `Kind` members from vocabulary table — adding cost edge without vocabulary row occurs only in same promotion unit as new arm from vocabulary promotion unit.
- Contract tests join `PATH_COST_ROWS` keys against `TRANSITION_GRAPH` edges — orphan costs or edges without cost/default weight fail import gate.
- Certificate golden fixtures live beside family owner — one fixture per declared transition row with sample source arm, expected path, and verification acceptance.
- Wire egress after certified chain uses final successor arm vocabulary row — certificate tests include round-trip on terminal arm, not only source arm.
- OpenAPI publication unaffected by cost tables directly — operational metadata beside owner unless API documents allowed modality sequences as separate operation schema enumerating lawful minimum-cost targets from `WEIGHTED_REACHABLE`.
- Stryker jointly scores certificate emit hook and smart-constructor arms when mutants skip certificate emit on successful chain — kill ratio depends on golden fixture absence detection, not silent chain completion without audit spine.
- Table-driven hypothesis strategies import materialized exemplars from registry row fixtures — strategies do not scrape cost tables or certificate JSON for kind tokens independent of owner vocabulary table.
