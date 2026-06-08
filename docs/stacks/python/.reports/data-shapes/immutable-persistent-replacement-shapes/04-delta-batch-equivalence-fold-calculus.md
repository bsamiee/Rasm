# Delta, Batch Fold, And Observational Equivalence Calculus

# Delta Morphism Class Extension

- Four morphism evidence classes precede batch calculus — live replace, snapshot isolate, replay materialize, and path lens endomorphisms — **delta morphisms** are a fifth class: structured descriptions of field and collection changes intended for merge, audit, or store-conditional apply without re-deriving deltas from dump diffs at handler sites.
- **Live delta**: trusted same-process change set on a validated predecessor — pairs with Tier S path lens or multi-field `copy.replace` when delta row documents trusted origin; not wire-sourced `json.loads` output presented as delta.
- **Patch delta**: staging `TypedDict` or closed payload union validated at root — `TypeAdapter(Payload).validate_python(material)` product is delta evidence until promotion fold emits successor owner; patch is pre-owner evidence, not interior domain state.
- **Store delta**: external generation token plus bounded field overlay from optimistic-concurrency seam — `(expected_generation, business_key, overlay)` row; apply is fallible `Result[Successor, Conflict]` at root, not bare `copy.replace` on stale session owner.
- **Audit delta**: receipt-only append or version bump without canonical field mutation — evidence crosses seams as `Block` append or frozen struct; audit deltas do not substitute for live replace on domain fields unless transition row documents receipt-only morphism.
- Conflating delta evidence with canonical owner identity — treating validated patch dict as durable domain owner — is the same seam class as snapshot-as-owner on replay paths.

# Delta Row Shape

- Every admitted batch or store-conditional transition declares one frozen **delta row**: `delta_id`, `owner_alias`, `delta_kind`, `path_bindings`, `tier`, `store_guard`, `equivalence_axis`, `composition_mode`, `negative_fixture_ids`.
- `delta_id` is a stable snake slug — `settings_timeout_overlay`, `registry_multi_key_batch`, `session_version_cas` — not ordinal patch depth alone.
- `delta_kind` draws from `{live_field_set, patch_payload, store_overlay, audit_receipt}` — selects merge kernel at root before interior handoff.
- `path_bindings` is `tuple[PathBinding, ...]` where each `PathBinding` pairs `path_id` from `LENS_CATALOG` with `focus_value` or focus endomorphism — empty bindings with non-audit `delta_kind` fail catalog parity.
- `tier` documents strictest tier across bindings — wire-sourced focus in any binding routes Tier V for entire batch closure; tier sandwich across bindings is a merge blocker.
- `store_guard` is optional `CompareAndSwapRow(expected_generation, business_key)` — present only for `store_overlay` and persistence-coupled live batches; absent on pure Tier S in-session batches.
- `equivalence_axis` names the proof witness under test — `{object_identity, logical_version, field_equality, alias_equivalence, encoded_bytes}` from succession evidence lattice — batch proofs declare axis explicitly, not caller inference.
- `composition_mode` is `atomic` or `observation_chain` — `atomic` folds all bindings in one outer replace expression; `observation_chain` registers intentional spine observation between bindings with separate lens rows.
- `negative_fixture_ids` index harness anti-patterns — `dump_diff_delta`, `tier_s_wire_batch`, `stale_session_cas`, `mutable_patch_accumulator`.

# PathBinding And Resolution Tables

- `PathBinding` is a frozen row: `path_id`, `focus_value | focus_endomorphism_id`, `binding_index`, `tier`, `band` — `path_id` foreign-keys `LENS_CATALOG`; interior handlers do not construct bindings from wire keys.
- `DELTA_TO_BINDINGS: frozendict[DeltaKey, tuple[PathBinding, ...]]` resolves staging patch keys to registered `path_id` and focus slots — unmapped keys fail at root, not silent no-op at interior.
- `PATCH_TO_PATH: frozendict[PatchKey, tuple[Segment, ...]]` supplies segment suffixes when patch keys address multi-hop spines — resolution composes `validate(patch) ∘ map(PATCH_TO_PATH) ∘ bind(DELTA_TO_BINDINGS)`.
- `Segment` kinds are `{field, key, index, arm, slice}` — string dot-path parsers at handler sites are rejected; segment tuples use canonical Python field names after alias normalization at root.
- Open patch `extra_items` promote to `frozendict` at materialization — delta resolution targets canonical paths via mapping row, not raw extra key strings.
- Dump-diff at handler site — computing delta by comparing `model_dump` outputs — is rejected when `DELTA_TO_BINDINGS` or validated patch payload can state the change set.

# Observational Equivalence Lattice

- Replacement correctness decomposes into five observational axes — object identity, logical version, field-wise equality, intentional alias, and encoded-bytes identity — passing one axis does not discharge batch or delta obligations; proof tables name the axis under test per succession evidence.
- **Object identity**: every live replace and atomic batch closure asserts `successor is not predecessor`; replay and store apply assert fresh materialize even when field equality holds — object axis is process-local.
- **Logical version**: forward batch increments version once at outermost closure — not per binding unless `observation_chain` row documents intermediate version witness; version unchanged across material batch when version is load-bearing is a transition defect.
- **Field equality**: two owners may be field-equal on all stored slots while differing on object axis — equality is not successor identity; pydantic `model_dump` dict equality is not owner equality.
- **Alias equivalence**: Tier S batches permit unchanged slot `is` aliasing across predecessor and successor; Tier D and Tier V batches break alias at bindings where isolation or replay requires it — alias axis is orthogonal to field equality when nested child owners re-materialize.
- **Encoded-bytes identity**: persistence keys on wire shape require deterministic encoder pinning at root — batch successors change bytes when any serialized field in the projection changes; reusing predecessor bytes for bumped version is positive failure.
- Cross-axis composition: store apply may preserve business key while changing object identity and encoded bytes — witnesses stay independent; assuming field equality implies store upsert success is rejected.

# Equivalence Axis Non-Sufficiency

- Batch and store proof is bidirectionally non-sufficient — neither field snapshots alone nor a single declared `equivalence_axis` discharges full batch, CAS, or certificate obligations; contract rows declare witnessed axis subset; metamorphic modules stack complementary layers beside atomic closure proof.
- **Field equality non-sufficiency** — field-equal successors after batch apply do not prove CAS success, tier legality, or atomic closure — stale-generation overlay on field-equal session owner still emits `Conflict`; field-equal replay product does not authorize further `copy.replace` without store truth reload; merge gates compare `path_bindings` digest order and `delta_id` before field snapshot diff when batch certificate is in affected closure.
- **Object identity non-sufficiency** — `successor is not predecessor` alone does not witness encoded-bytes stability, logical version monotonicity, or alias law on non-focus slots — object axis without `alias_equivalence` declaration does not discharge Tier D isolation at promoted binding; certificate batch hop with `predecessor_is_successor: false` does not imply `focus_hash` stability across releases without pinned `focus_hash_kernel`.
- **Encoded-bytes non-sufficiency** — deterministic encode equality on terminal projection does not prove interior batch used atomic closure — sequential bare replaces can yield identical bytes when intermediate owners were not observed; bytes match with divergent `path_bindings` registration order classifies as batch routing defect before field diff.
- **Axis declaration non-sufficiency** — `equivalence_axis` on `DeltaRow` names proof target for parametrized suites, not implied witnesses on undeclared axes — store success row witnessing `logical_version` does not discharge `alias_equivalence` unless row or certificate `equivalence_axes_witnessed` includes both; compose-chain certificate cites `focus_hash` per binding — axis proof without hash digest is insufficient for cross-release audit replay.
- **Classifier order** — root batch guard classifies `delta_kind` and strictest tier → atomic apply or `Conflict` → axis witnesses per declared subset → field snapshot optional secondary witness; assuming dump-diff reconstruction satisfies any axis without `DELTA_CATALOG` row is positive failure in contract tables only.

# Batch Fold Composition Laws

- **Empty batch**: identity delta on predecessor — no `path_bindings`; successor equals prior only for audit-only no-op rows explicitly registered; default empty binding set is catalog defect for live batches.
- **Atomic composition**: when `composition_mode=atomic` and all bindings are observation-free, `apply_batch(o, δ)` equals one nested replace closing all focuses — `copy.replace(o, **{field: focused_value, ...})` or nested lens composition per lens associativity law; sequential bare replaces on same root without atomic row are rejected.
- **Binding independence**: bindings with disjoint spine prefixes compose in one atomic closure — shared spine prefix merges to single outer expression with multi-focus replace; independent `Map` key bindings on same field use `Map` batch `add` fold or nested `add` chain per path-copy band law, not repeated full-owner replace per key in hot loops.
- **Non-commutative override**: later bindings in observation chain do not override earlier applied focuses on same path unless row documents overlay semantics — `PATCH_TO_PATH` resolves key collisions at root; rightmost patch key wins on `frozendict` overlay batches only when `delta_kind=patch_payload` and mapping semantics apply.
- **Fallible batch**: `apply_batch` returning `Result[Owner, E]` short-circuits on first binding failure — prior owner identity unchanged on error path; partial mutation via extracted spine views between bindings is seam defect.
- **Receipt distribution**: audit bindings append to `Block` fields via combinator on atomic closure — `copy.replace(o, receipts=o.receipts.append(r))` bundled with field bindings in same expression when receipt documents same transition.

# Store-Conditional Apply Morphism

- Store overlay deltas require root `CompareAndSwapRow` — `(business_key, expected_generation, path_bindings)` — apply pipeline: load store truth → compare generation → on match, atomic batch on in-memory owner → persistence morphism at egress; on mismatch, `Conflict` evidence without mutating session owner.
- Stale session owner after failed CAS must not receive further `copy.replace` — root invalidates session cache key `(business_key, generation, schema_version)` and re-materializes from store truth via replay materialize pipeline.
- Idempotent store apply of identical overlay on unchanged generation produces new object identity by default — value-keyed interning requires explicit policy row beside replay guard.
- Generation regression without named rollback row is rejected — store truth replay, not downward version assignment on live owner.
- Schema version in store guard must align with pinned decode row — batch apply on instance decoded under prior schema without migration morphism is merge blocker.

# Concurrent Publication On Batch Folds

- PEP 779 free-threaded publication law applies to batch publication — finalized `frozendict`, `Map`, `Block`, and frozen owners are safe for parallel readers when shallow-immutability holds on all nested payloads exposed; batch fold must complete before successor publishes to shared readers.
- Atomic batch closure produces one publication event — partial bindings visible to other threads via shared mutable spine extract is data race and shallow-law breach.
- `ContextVar` batch context rebinding sets terminal successor once — `ctx.set(successor)` after atomic apply completes; per-binding `ContextVar` patch accumulators across tasks are root seam defects.
- Worker and multiprocessing seams do not pass in-flight batch partial state — encoded bytes and configuration slices cross; successor materializes locally through replay; parent-process batch half-state is non-portable.
- Root warm-up batch folds materialize publication tables once before worker import — post-import mutation of delta catalog rows races under parallel importers.

# Tier And Band On Batch Paths

- Tier classification runs once at root on entire `path_bindings` tuple — strictest tier wins; interior module cannot downgrade per-binding tier after root handoff.
- Tier V batch: any wire-sourced or computed focus value routes full batch through `model_validate` / `TypeAdapter.validate_python` on snapshot merge — shallow multi-field `model_copy` on batch with one wire binding is tier leak.
- Tier D batch: any binding traversing nested mutable ingress residue requires promotion at that binding before atomic closure — deep isolation is hop-local per path lens row; deferred isolation to final binding is rejected.
- Band survival: atomic batch preserving multiple path-copy `Map`/`Block` bindings must use combinators per binding — mid-batch conversion of spine collections to mutable `dict`/`list` breaks band law regardless of tier.
- Quadratic `frozendict` union batching many keys in loop is rejected — multi-key overlay batches on large N register `Map` field promotion at owner replacement per amortized transition routing.

# Family-Specific Batch Kernels

- **Frozen dataclass**: atomic batch uses one `copy.replace` with all Python field names — `init=False` gaps closed by owning transition method when any binding crosses init-only boundary; per-field sequential replace without atomic row fails contract tests.
- **Pydantic frozen model**: batch `model_copy(update={...})` accepts Python field names only — alias keys silent-skip is negative fixture; Tier V batch routes through `model_validate(snapshot | merged_delta)` when any constraint must replay.
- **msgspec struct**: batch `structs.replace` / `copy.replace` invokes `__post_init__` once on terminal struct when defined — not per binding.
- **frozendict field overlay batch**: terminal bindings on distinct keys compose as left-associated union fold `base | k1 | k2 | ...` in one expression — not `copy.replace` on mapping field.
- **Map field batch**: `index=fold(Map.add, bindings)` or nested add chain in one parent replace — keys added in catalog registration order unless row documents commutative key set.
- **Tagged union batch**: cross-arm bindings require explicit migration row — batch cannot mix arm focuses without discriminant segment per path lens calculus.

# Catalog And Harness Extension

- Root catalog extends `LENS_CATALOG` with `DELTA_CATALOG: tuple[DeltaRow, ...]` — every production batch transition, store overlay, and patch-driven multi-path update has exactly one delta row.
- Catalog parity joins `TransitionRow`, `LensRow`, and `DeltaRow` in one CI assertion — set difference in any direction fails before property suites.
- Hypothesis composites draw `(prior, path_bindings, delta_kind, tier, equivalence_axis)` from closed exemplars — not arbitrary dict diffs filtered by runtime validation.
- Property targets: atomic closure identity, axis witnesses per `equivalence_axis`, CAS conflict path preserves prior owner, band preservation across multi-key `Map` batch, generation monotonicity on store success path.
- Mutation testing kills mutants that flatten atomic batch to sequential same-owner replaces or route store overlay through bare `copy.replace` without generation guard.

# Interaction With Snapshot, Replay, And Path Lenses

- Snapshot morphisms export read-only carriers — delta rows are not derived from snapshot diff for domain apply; patch and store overlays materialize at root from validated evidence, not snapshot subtract.
- Replay materialize produces fresh owner — batch apply runs on replay product in consuming process; CAS expected_generation aligns with store truth after replay, not parent session cache.
- Path lens associativity embeds into atomic batch — `apply_batch` with single binding equals `lens_apply`; multi-binding atomic batch equals composed lens when `composition_mode=atomic` and observation-free.
- Succession version increment stays at outermost batch closure — per-binding version bumps require `observation_chain` rows with documented intermediate witness.

# Proof Obligations By Equivalence Axis

- **Object identity**: every atomic batch and store success path asserts `successor is not predecessor`; conflict path asserts `session_owner is unchanged`.
- **Logical version**: forward batch asserts `successor.version == predecessor.version + 1` when version load-bearing — or documents exempt row when version not serialized.
- **Field equality**: negative control only — two replayed owners field-equal with distinct object ids prove replay identity law, not batch success.
- **Alias equivalence**: Tier S batch asserts unchanged non-focus slots `is` predecessor slots; Tier D batch asserts promoted hop `is not` at isolation binding.
- **Encoded-bytes**: store success batch with changed serialized fields asserts `encode(successor) != encode(predecessor)` under deterministic encoder — unchanged bytes with bumped version is positive failure.
- CAS negative: stale generation asserts `Conflict` and prior owner field values unchanged — no partial binding apply on failure path.
- Shrinking on batch composites preserves tier legality and binding path registration — invalid binding key mutations reject at construction gate.

# Materialization Stage And Batch Placement

- Delta validation and path resolution execute at root materialization and handler edges — staging payloads remain pre-batch mutable evidence only.
- Atomic batch apply on canonical owners runs in enrichment and interior stages after stage-six promotion completes on all binding spines — child mutable residue in any binding without promotion is shallow-law breach.
- Egress follows terminal batch successor — encoding does not repair fields discovered late during batch apply; projection reads enriched identity.
- Trusted-replay materialize completes stages one through six before batch apply in consuming process — replay shortcut past promotion at any binding is rejected.

# Checker And Analyzer Alignment

- Static proof owns `delta_kind` and `composition_mode` exhaustiveness — ty/mypy `match` on closed delta row unions; dump-diff builders fail lint when `DELTA_TO_BINDINGS` applies.
- Runtime proof owns axis witnesses — beartype on `apply_batch` entrypoints checks owner type at root; equivalence axis stays in contract tables.
- Import-linter flags interior modules constructing `PathBinding` tuples from wire keys — delta and path registration belong beside owner import at root.
- Ruff policy bans shared mutable batch accumulators in closures — same family as replacement doctrine shared-closure defect signals.

# Exemplar Delta Row Patterns

- **Single-field atomic live**: `delta_kind=live_field_set`, one `PathBinding` to scalar root lens, `composition_mode=atomic`, `tier=S`, `equivalence_axis=object_identity` — equivalent to single-hop lens row bundled as delta for batch catalog parity.
- **Multi-field same-level atomic**: `path_bindings` tuple of root field lenses with no shared spine — `composition_mode=atomic`, one `copy.replace(o, field_a=v_a, field_b=v_b)` closure; `equivalence_axis=alias_equivalence` under Tier S.
- **Patch payload batch**: `delta_kind=patch_payload`, bindings resolved through `DELTA_TO_BINDINGS` after `TypeAdapter` validate — `tier=V` when any patch value is wire-sourced; merge via `model_validate(snapshot | merged)` not sequential shallow copies.
- **Map multi-key registry batch**: bindings share `field("index")` spine with distinct `key(...)` segments — `Map.add` fold in one parent replace; `equivalence_axis=alias_equivalence` on path-copy band; `binding_count` documents key cardinality for amortization review.
- **Store CAS overlay**: `delta_kind=store_overlay`, `store_guard` present, `tier` inherits from overlay bindings — success path increments version once; conflict path emits `Conflict` without successor publish.
- **Audit receipt append**: `delta_kind=audit_receipt`, empty or receipt-only `path_bindings` — `Block` combinator bundled with concurrent field bindings in same atomic row when receipt attests same transition event.
- **Observation chain dependency**: `composition_mode=observation_chain`, two delta rows registered — second row `delta_id` documents dependency on first successor; merging without intermediate observation flag is catalog defect.

# Composition Root Delta Guard

- Root ingress classifies `delta_kind` and strictest tier before `apply_batch` — handlers do not construct ad hoc `path_bindings` from wire keys at leaf sites.
- Root patch seam validates patch then resolves `DELTA_TO_BINDINGS` then invokes atomic apply on trusted or validated snapshot — interior modules receive successor owner, not raw patch plus binding list.
- Root store seam loads generation from store truth before CAS apply — expected_generation in row matches store read, not session cache assumption.
- Root catalog publishes `DELTA_CATALOG` beside `LENS_CATALOG` and `TransitionRow` tuple — harness parametrization imports all three; orphan batch apply without delta row fails CI.
- Alias normalization on patch, wire, and store overlay inputs completes at root before binding resolution — segment tuples and field names use canonical Python names only.

# Hash, Key, And Ordered Batch Defects

- Hashable `frozendict` overlay batches require hashable focus values at every key binding — nested mutable list in overlay value fails hash stability proof at delta registration.
- Order-sensitive batch bindings on `tuple` / `Block` use `index` or `slice` path segments — batch key overlay on ordered stream when sequence order is load-bearing is segment-kind defect.
- `frozendict` batch overlay equality is order-insensitive — two delta rows with permuted binding order on same key set must prove identical canonical overlay when keys are equal as mappings.
- `Map` batch remove bindings return typed absence on missing keys — `Option`-aware remove or explicit negative fixture; silent no-op remove in batch is rejected.
- Multi-key `frozendict` union batch on large N inside event loop is routing defect — register `Map` promotion at owner replacement per amortized transition routing before hot batch path goes to production.

# Patch-To-Delta And Merge Morphisms

- Staging patch merge morphism at root: `validate(patch) ∘ resolve(DELTA_TO_BINDINGS) ∘ atomic_apply(snapshot, bindings)` — interior fold begins on successor only.
- Closed patch unions map each arm to distinct `delta_id` — discriminant match precedes binding resolution; bare dict arm selection without adapter table is seam break.
- Partial nested patch on pydantic child in batch routes through child `model_validate` when any constraint must replay — shallow parent batch with dict child slot in one binding is Tier leak unless documented Tier S child field.
- Open extension bands fold into `frozendict` before binding resolution — extension keys resolve through mapping row to interior canonical paths, not direct interior dict `.update`.
- Merge associativity with replay: `apply_batch(materialize(replay(bytes)), δ)` runs only after replay stages complete — delta apply does not shortcut replay promotion at any binding spine.

# Functional Batch Update Algebra

- Batch apply is an endomorphism on frozen owner types — `Owner → Owner`, `Owner → Result[Owner, E]`, or `Result[Owner, E]` when store guard present — not partial mutation on erased carriers or validated patch dicts.
- Collection batch updates are functorial lifts on shared spine — parent closure equals single replace with combinator-folded child across all key or index bindings — not per-key extract-mutate-reinstall loops.
- Multi-step batch pipelines thread successors as fold seeds — `pipe(prior, result.bind(lambda o: apply_batch(o, δ)))` — shared outer mutable binding accumulators defeat functorial threading and break publication law under concurrency.
- Normalized overlay composition is union fold morphism for `frozendict` batches — left-associated `base | o₁ | o₂` with right-hand override; commutative equality, non-commutative override semantics are pinned contract.
- Store-conditional batch is morphism composition at root — `load_truth ∘ compare_generation ∘ atomic_apply ∘ persist` — session owner never persists before CAS success row completes.

# Harness And Property Targets

- Root contract modules import `DELTA_CATALOG` beside `LENS_CATALOG` and encoder singletons — parametrized batch matrix tests derive from registered `(prior, path_bindings, delta_kind, tier)` exemplars.
- Hypothesis composites draw batch tuples from closed catalog — shrink rebuilds lawful bindings after mutation; tier downgrade during shrink is forbidden.
- Round-trip samples pair atomic batch succession with store reload — assert business key and generation align on success, `Conflict` on stale generation, object identity differs on success even when overlay is logically noop on fields.
- Free-threaded batch publication samples assert successor is fully materialized before worker threads observe shared registry — partial atomic apply visible across threads is positive failure.
- Mutation testing on batch tier-routing kills mutants routing any wire binding through shallow `model_copy` — same kill obligation as tier downgrade and path-lens tier-sandwich paths.

# Proof Obligation Register

- `PO-BAT-01` — `DELTA_CATALOG` parity with production batch apply paths and `TransitionRow` registry.
- `PO-BAT-02` — Atomic closure law holds for every `composition_mode=atomic` row in catalog fixtures.
- `PO-BAT-03` — Axis witnesses match declared `equivalence_axis` per row — undeclared axis negative controls fail.
- `PO-BAT-04` — CAS conflict path preserves session owner identity and field values — no partial binding apply.
- `PO-BAT-05` — `DELTA_TO_BINDINGS` total over closed patch key set for each `patch_payload` row.
- `PO-BAT-06` — `focus_hash_kernel` pinned beside owner import when certificate batch hops consume digests.
- `PO-REG` — `register_law` binds `apply_batch`, `DELTA_CATALOG`, `DELTA_TO_BINDINGS`, and axis witness modules.

# Rejection Signals

- `model_dump` before/after diff as delta source at handler — collapse to `DELTA_TO_BINDINGS` or validated patch payload.
- Sequential bare `copy.replace` per binding without atomic delta row — collapse to single nested closure or register `observation_chain`.
- Store overlay via `copy.replace` on stale session owner — collapse to CAS row plus replay invalidate on conflict.
- Wire-sourced batch with any Tier S binding — collapse to Tier V full snapshot validate.
- Mutable dict batching patch keys then `model_validate` at end — collapse to atomic `model_validate(snapshot | merged_delta)`.
- Partial `Map` key apply leaving mutable extracted dict on spine — collapse to combinator batch on parent field.
- Assuming field-equal replay product implies CAS success — collapse to generation guard and store truth reload.
- Audit receipt appended via shared outer `list` mutation — collapse to `Block` combinator in atomic batch expression.
- Cross-arm batch bindings without migration row — collapse to per-arm delta rows with discriminant segments.
- Interior handler caching batch half-state across requests — collapse to root delta catalog with explicit session cache key on generation.
