# Snapshot, Replay, And Succession Morphisms

# Morphism Class Extension

- Live replace morphisms (`copy.replace`, union, `Map`/`Block` combinators) are live-session endomorphisms on frozen owners — snapshot, replay, and succession morphisms are a second class operating across time, process, and persistence seams; conflating the classes produces tier leaks and false identity witnesses.
- Live replace: same process, post-validation predecessor, successor via admitted replace kernel from immutable replacement owner-selection table — band and tier axes apply unchanged per immutable replacement tier law and structural sharing-band taxonomy.
- Snapshot isolate: cross-boundary or cross-call freeze of observable state into a Tier D or egress projection carrier — produces evidence readable without mutating the live predecessor; not a domain transition on the session owner.
- Replay materialize: bytes, store row, or trusted snapshot re-enters through `decode` / `model_validate` / `convert` and binds a **new** owner identity — never `copy.replace` on a cached session instance unless root replay guard pins encoder, schema version, and store key.
- Succession witness: explicit version, epoch, or monotonic sequence field on the canonical owner documents logical progression when external stores key on business identity rather than `id(owner)` — increment is part of the live-replace expression, not a side channel on the predecessor.

# Succession Evidence Lattice

- Succession decomposes into four independent evidence axes — object identity, logical version, encoded-bytes identity, and business key — passing one axis does not discharge the others; proof tables name the axis under test.
- Object identity: every live replace asserts `successor is not predecessor`; replay always asserts `replayed is not session_cached` even when logical version and field payloads match — object axis is process-local.
- Logical version: monotonic integer, epoch token, or closed enum generation on the owner — two successors with the same version and different field deltas is a transition defect; version regression without documented rollback morphism is rejected.
- Encoded-bytes identity: persistence or cache keys on wire shape require root `order="deterministic"` encoder pinning — replacement-produced successors change bytes when any serialized field changes; reusing predecessor bytes for a successor with bumped version is a positive failure in root contract tests.
- Business key: external stores address rows by stable identifiers independent of Python `id` — succession evidence on the owner must align with store upsert semantics; bare replace on an in-memory instance does not imply store row update until egress or persistence morphism completes.
- Cross-axis composition: Tier S live replace may preserve encoded bytes when version is not serialized; Tier V replay from wire always re-materializes and may change object identity while preserving business key — axis witnesses stay orthogonal to sharing band.

# Snapshot Morphism Semantics

- Snapshot morphisms project terminal enriched identity to an immutable carrier — `model_dump(mode="python")` staging, `project_wire`, `structs.asdict` egress slices, or Tier D `model_copy(deep=True)` — without repairing domain fields to close encoding gaps.
- Snapshot is read-only evidence export, not a staging dict for interior `.update` — mutate-then-validate on dump output before re-presenting as owner is the same rejected seam class as mutate-then-freeze.
- Tier D snapshots for handoff across async tasks, subprocess boundaries, or handler seams require nested isolation when policy rows demand it — shallow dump with aliased nested mutable residue is pre-promotion evidence only.
- Deep-immutable `json.loads` product with `object_pairs_hook=frozendict` is a wire snapshot, not a canonical owner — promotion morphism still runs before domain replace algebra.
- Snapshot timing: capture after the last live replace in the bounded context fold completes — mid-pipeline snapshots of partially enriched owners are introspection-only, not durable configuration evidence.

# Replay Materialization Pipeline

- Persistence read, probe-store fetch, and cross-process config handoff share one root pipeline: `bytes | store_row → decode → validate → frozen owner` — no step widens to mutable staging between frozen handoffs.
- Replay product identity is always fresh construction — `model_construct` and trusted `convert` are admitted only under root pinned replay policy when store key, schema version, and encoder identity are fixed at the composition root.
- Wire-sourced replay deltas route Tier V through `model_validate` / `TypeAdapter.validate_python` before any interior handoff — deep-immutable parse trees do not downgrade tier.
- Same logical payload replayed twice yields two distinct object identities unless an explicit interning policy row documents value-keyed memoization — default doctrine rejects `id`-keyed session caches.
- msgspec `decode` / `convert` materialize first durable struct at the boundary; live `structs.replace` begins only after replay identity exists — axis separation from tier law remains load-bearing on replay paths.

# Version Monotonicity And Rollback

- Forward transitions express version increment inside the replace morphism — `copy.replace(prior, version=prior.version + 1, ...)` or validated reconstruction with explicit generation field — cache-slot mutation on the predecessor is rejected.
- Rollback is a named morphism, not silent version regression — closed rollback rows in the root transition catalog map `(current_version, rollback_token) → prior_generation_owner` via `model_validate` over pinned snapshot evidence, not arbitrary field assignment on the live session owner.
- Concurrent writers on the same business key compose through store semantics — in-memory replace algebra does not substitute for optimistic concurrency tokens when the store owns conflict resolution; owner version fields mirror store generation when load-bearing.
- Union arm migration on replay: ingress discriminants map through adapter tables before domain replace — wire tag strings on replayed bytes do not flow into shallow replace on the wrong arm.

# Cross-Process And Cross-Session Seams

- Parent-process owner identities are non-portable across `multiprocessing` and `concurrent.interpreters` seams per structural sharing publication law — configuration slices and encoded bytes cross; successors materialize locally through root decode paths.
- Worker entrypoints rebind `ContextVar` from root-published frozen snapshots — parent session owners do not cross as mutable state; replay morphism runs in the child process composition root.
- Shared-memory or file-backed probe stores hand off bytes and pinned decoder policy — not live Python references to canonical owners.
- Free-threaded publication law applies to replayed registry imports — tables must finalize before worker threads import root symbols; replay does not relax publication timing.
- Async task handoff across `asyncio` boundaries passes snapshot carriers or bytes plus replay policy — awaiting coroutines do not close over mutable session owners mutated after task creation.
- Subinterpreter config isolation (PEP 734) treats each interpreter composition root as independent replay consumer — shared bytecode does not imply shared canonical owner identity across interpreters.

# Temporal Succession Without Time Travel

- Doctrine rejects implicit time-travel on canonical owners — replay materializes current store truth or pinned bytes, not arbitrary historical field subsets selected without rollback row.
- Event-sourced folds replay ordered evidence through `Block` or store log materialize steps — each log entry is replay morphism or live replace on the prior successor, not in-place patch of an ancestor instance kept in memory.
- Point-in-time snapshot queries export read-only carriers at boundary — interior domain modules receive materialized owner for that timestamp via replay pipeline, not `copy.replace` rewinding fields on the live session owner.
- Audit trails store wire tokens or receipt blocks — reconstructing prior generations walks rollback or event replay morphisms, not mutating version fields downward on the current owner.

# Band And Tier On Replay Paths

- Replay materialization picks sharing band from the owner-selection table at construction — not from the prior process layout or ad hoc dump shape; collection fields enter at the correct band during materialization stage six per structural sharing band placement law.
- Band migration during replay is a construction-time promotion — migrating a registry field from `value_copy` `frozendict` to `path_copy` `Map` happens on the replayed owner at materialization, not mid-interior fold on a session cache.
- Tier orthogonality holds on replay per immutable replacement tier law: Tier V for wire-sourced replay; Tier D when snapshot holds nested mutable residue; Tier S only for same-process trusted snapshot merge documented beside the transition method — replay never auto-downgrades tier because the predecessor was once validated in another process.

# Receipt And Provenance On Succession

- Material live transitions may append algorithm receipts to path-copy `Block` streams — receipt export is evidence-only projection crossing seams; receipts do not duplicate canonical domain state.
- Succession receipts carry route, status, sampling, or solver evidence when load-bearing — they materialize as frozen struct append products at the exporting boundary, not mutable lists patched after the transition.
- Replay folds consume receipt blocks as read-only evidence — foreign contexts import frozen streams or fold summaries, not interior accumulators that built them.
- Receipt append on live replace uses owner replace or `Block.cons`/`append` — never in-place mutation on a shared outer list captured from enclosing scope.

# Root Replay Guard Obligations

- Composition root declares pinned replay policy beside encoder singletons — `ReplayPolicyRow` fields from replay policy row shape section are one frozen configuration row per persistence-dependent owner family.
- Round-trip proof at root witnesses wire legality for persistence-dependent owners per immutable replacement egress guard — `decoder.decode(encoder.encode(project_wire(canonical)))` failure maps to boundary `Fault`; it does not authorize domain replace repair on the failed value.
- Trusted-replay negative control: bare `copy.replace` on cached session owner without pinned policy is rejected at root — indexed in immutable replacement binding defect and rejection catalogs.
- Replay catalog rows name `kernel: replay_materialize` distinct from live replace kernels — every persistence read path binds exactly one `TransitionRow` plus one `ReplayPolicyRow`; catalog parity extends immutable replacement transition catalog width law.

# Composition With Live Replace Folds

- Chained pipelines compose as typed steps — live replace fold → terminal snapshot → encode → store write → later replay materialize → interior replace-only transforms — no step substitutes snapshot for replay or replay for live replace without root guard classification.
- Patch application at root remains `validate(patch) ∘ merge(snapshot, delta) ∘ materialize` — replayed owners accept patches only through Tier S trusted merge or Tier V validate paths documented on the method contract.
- Enrichment before snapshot: live replace morphisms complete before snapshot capture — egress reads terminal enriched identity without mutating fields discovered late during encoding.
- Interior modules between replay materialize and the next snapshot/export remain replacement-only — replay does not grant a mutation window on fields that live replace law already forbids.
- Union tagged families replay through adapter-owned discriminant tables before arm-specific replace — cross-arm replay without explicit migration row is a seam break, not an interior fold concern.

# Materialization Stage Placement

- Stages one through six hold frozen owners; replay materialize completes stage one through six in the consuming process — replay is not a shortcut that skips validation or nested promotion at stage six.
- Stage seven egress reads terminal identity after live replace or replay materialize — encoded bytes exit only from enriched canonical owners, never from mid-pipeline snapshot carriers mistaken for terminal identity.
- Trusted-replay persistence read re-enters at decode materialization — replayed owners inherit band from owner-selection table at construction, not from store layout or dump key ordering in the persisted blob.
- Stage-skipping from replayed struct directly to bytes without projection owner requires documented bounded-context exemption at root — leaf modules do not skip enrichment because bytes arrived from a trusted store.
- Immutable materialization stage six is the last promotion point for nested payload isolation on replay — child mutable residue crossing into interior folds without promotion is a shallow-law breach regardless of store trust.

# Optimistic Concurrency And Store Tokens

- When external stores own conflict resolution, owner version or generation fields mirror store tokens — live replace increments in-memory version; persistence morphism carries store-conditional upsert semantics at the root seam.
- Replay after failed upsert re-materializes from store truth, not from the stale in-memory session owner — session cache invalidation is explicit root obligation, not lazy overwrite via `copy.replace`.
- Compare-and-swap rows at root map `(business_key, expected_generation) → Result[PersistedOwner, Conflict]` — conflict arms return typed evidence; interior modules do not retry upsert by mutating the failed owner in place.
- Idempotent replay of identical bytes produces fresh object identity by default — value-keyed interning requires explicit policy row beside the replay guard when deduplication is load-bearing for memory, not for correctness.
- Store schema migration replays through versioned decode rows — pinned schema version in replay policy must bump when wire layout changes; bare replace on instances decoded under prior schema is rejected.

# Morphism Composition Laws

- Identity: first materialize from ingress or replay yields `Owner₀`; no successor equals predecessor on live replace — replay always binds new identity even when payload is logically equal to a prior session owner.
- Associativity of snapshot after replace: `snapshot(replace(o, δ))` equals snapshot of terminal enriched `o'` — snapshot does not commute with partial replace; capture only after the fold completes.
- Non-commutativity of replay after snapshot: `materialize(snapshot(o))` may differ from `o` when tier V replay replays constraints — snapshot carriers are not inverse of materialize unless round-trip proof row documents bijection for that owner family.
- Functorial threading: `replace(replace(o, δ₁), δ₂)` folds to one replace when intermediate observation is not load-bearing — same multi-field closure law; replay morphisms do not participate in interior double-replace shortcuts across process seams.
- Receipt append distributes over live replace: `append(receipt, replace(o, δ))` uses `copy.replace(o, receipts=o.receipts.append(r))` or `Block` combinator — not post-hoc list mutation after replace returns.

# Family-Specific Replay Routing

- Pydantic canonical owners replay through root `TypeAdapter.validate_python` or `model_validate` on decoded mapping — shallow `model_copy` on replayed dict slices without validate is Tier leak when constraints must replay.
- msgspec wire structs replay through module `Decoder.decode` then optional `convert` to domain struct — `structs.replace` on wire struct before domain promotion is rejected when promotion row exists.
- `frozendict` policy tables replay as mapping-shaped JSON through boundary codecs — union recomposition on replayed tables uses same `value_copy` band law from structural sharing identity lattice; order-insensitive equality pins regardless of store key order.
- `expression.Map` and `Block` fields inside replayed pydantic or msgspec owners materialize at `path_copy` band during construction — do not rebuild via mutable dict/list staging at replay warm-up.
- Cross-family replay promotion follows conversion-versus-replacement rows from immutable replacement doctrine — `model_dump` surgery before struct construction at replay seam is rejected when `msgspec.convert` or explicit `project_wire` states the mapping.

# Replay Policy Row Shape

- Every persistence read, probe-store fetch, and cross-process config handoff declares one frozen **replay policy row** beside root encoder singletons: `replay_policy_id`, `owner_alias`, `store_key`, `schema_version`, `encoder_order: Literal["deterministic"]`, `decode_kernel: Literal["decode", "type_adapter", "convert"]`, `admitted_construct: Literal["model_validate", "model_construct", "convert"]`, `tier_default`, `interning: Literal["none", "value_keyed"]`, `negative_fixture_ids`.
- `replay_policy_id` is a stable snake slug — `settings_probe_v3`, `envelope_cache_row` — not ordinal store version alone.
- `admitted_construct` documents when `model_construct` or trusted `convert` is permitted — only when store key, schema version, and encoder identity are pinned at composition root; default is `model_validate` materialize.
- `interning=none` is default — same logical payload replayed twice yields two distinct object identities; `value_keyed` requires explicit policy row when deduplication is load-bearing for memory, not correctness.
- Root `REPLAY_CATALOG: tuple[ReplayPolicyRow, ...]` extends immutable replacement doctrine `TransitionRow` catalog — every persistence read path binds exactly one `kernel: replay_materialize` transition row plus one replay policy row; orphan persistence paths without row witnesses fail catalog parity.
- Replay policy rows name `equivalence_axes` default witness set for replay hops in compose-chain certificates — typically `{object_identity, logical_version, encoded_bytes}`; business key axis added when store upsert semantics are load-bearing.

# Session Cache And Invalidation Law

- In-memory session caches key on `(business_key, generation, schema_version, replay_policy_id)` — not `id(owner)` alone.
- Stale session owner after failed optimistic upsert must not receive further `copy.replace` — root invalidates cache entry and re-materializes from store truth via replay morphism.
- Replay after store conflict re-materializes from authoritative store row — lazy overwrite via `copy.replace` on stale session owner is root seam defect.
- Idempotent replay of identical bytes produces fresh object identity when `interning=none` — certificate replay hops assert `predecessor_is_successor: false` even when field payloads match prior session owner.
- Cache hit on value-keyed interning must not skip Tier V when wire layout or schema version changed — interning keys include `schema_version` and `bytes_hash` slices, not field equality alone.

# FSM And Event-Sourced Replay

- FSM `State` and `Event` enum fields on frozen owners transition through live replace methods in immutable replacement doctrine — history persistence stores wire tokens or enum names per contract.
- Replay through root decoders materializes successors for historical generations — not in-place enum field assignment on session owners.
- Event-sourced folds replay ordered evidence through `Block` or store log materialize steps — each log entry is replay morphism or live replace on prior successor, not in-place patch of an ancestor instance kept in memory.
- Point-in-time snapshot queries export read-only carriers at boundary — interior modules receive materialized owner for that timestamp via replay pipeline, not `copy.replace` rewinding fields on live session owner.
- Audit trails store wire tokens or receipt blocks — reconstructing prior generations walks rollback or event replay morphisms, not mutating version fields downward on current owner.

# Proof Obligations By Morphism Class

- **Live replace** (immutable replacement doctrine): `successor is not predecessor`; tier and kernel match `TransitionRow`; optional alias witness under Tier S and `intentional_alias` band from structural sharing identity lattice.
- **Snapshot isolate**: carrier is read-only evidence — no `.update` on dump output; Tier D nested isolation when policy requires; snapshot timing after terminal enriched identity only.
- **Replay materialize**: `replayed is not session_cached` even when logical version and field payloads match; Tier V for wire-sourced bytes; fresh construction through pinned decode row; `structs.replace` forbidden before replay identity exists.
- **Succession witness**: version monotonicity on forward transitions; encoded bytes change when serialized version or fields change under deterministic encoder; business key alignment with store upsert semantics.
- **Cross-axis**: passing object identity does not discharge logical version, encoded bytes, or business key — proof tables parametrize `equivalence_axis` explicitly per delta batch equivalence fold vocabulary.
- **Certificate replay hop**: production paths record `replay_policy_id`, `schema_version`, `store_key`, `bytes_hash` — bare session `copy.replace` appears only in negative fixtures indexed by `negative_fixture_ids`.

# Hypothesis Replay Strategy Law

- Strategies draw from registered replay policy rows and closed `(bytes_fixture, tier, schema_version)` exemplars — not arbitrary `json.loads` output filtered by runtime validation alone.
- Replay draws extend live-replace tuples to `(prior_session, bytes_or_store_row, replay_policy_id, expected_axis)` — shrink must preserve tier legality and schema version pin.
- Snapshot draws pair `(terminal_owner, snapshot_carrier_kind, fields_present)` — missing load-bearing fields in `fields_present` are negative-only strategies, not shrink endpoints.
- Succession draws generate version-forward and named-rollback rows only — silent version regression without rollback token is construction-gate failure.
- Cross-process draws include `forbidden_fields_absent` probes — mutable session caches and `ContextVar` accumulators must not appear in handoff fixtures.
- `@given` targets register one law per morphism class — live replace, snapshot, replay, and succession remain separate targets; conflating replay with Tier S live replace in one property is rejected strategy design.

# Rejection Signals

- Bare `copy.replace` on cached session owner after persistence read — collapse to root `decode` → materialize under pinned replay policy row.
- Snapshot carrier presented as canonical domain owner — collapse to promotion morphism before interior replace algebra.
- `model_dump` diff used as replay delta — collapse to delta batch equivalence fold calculus structured delta row or Tier V `model_validate` on full snapshot.
- Replay tier downgrade because predecessor was once validated in another process — collapse to root tier guard before interior handoff.
- Mid-pipeline snapshot mistaken for terminal identity at stage seven egress — collapse to capture after last live replace completes.
- Downward version assignment on live owner without named rollback row — collapse to snapshot-replay succession law rollback catalog entry via `model_validate` over pinned snapshot.
- Parent-process owner reference crossed multiprocessing seam — collapse to bytes plus replay policy; child materializes locally.
- Event log replay mutating ancestor instance in memory — collapse to successor chain via replay materialize per log entry.
- `model_construct` on wire-sourced replay without documented `admitted_construct` row — collapse to Tier V `model_validate`.
- Reusing predecessor encoded bytes for successor with bumped version — positive failure in root contract tests on `encoded_bytes` axis.
