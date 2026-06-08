# Immutable, Persistent, And Replacement Shapes

# Immutable Owner Law

- Domain values carrying invariants, lifecycle state, or cross-call evidence are frozen after materialization on Python `>=3.15`; every state change is replacement returning a new owner or typed rail — mutation between boundaries is rejected.
- One frozen owner holds fields, derived evidence, and transition methods; scattered mutation repair, post-hoc freeze, and attribute-normalization loops are collapse targets.
- Shallow immutability is explicit law: `frozen=True`, `frozendict`, and `tuple`/`frozenset` freeze the container shell, not nested mutable payloads. Nested dicts, lists, or buffers require deep replacement, boundary snapshots, or promotion to immutable child owners before the parent is durable. A `frozendict` value slot holding a mutable `dict` or `list` violates shallow law even when the outer shell is frozen.
- Parallel mutable and frozen variants of the same concept are rejected. One canonical owner per concept; transitions use replacement algebra, not dual-class repair, post-hoc freeze, attribute-normalization loops, or `{Concept}Draft` types that batch writes before a final `replace`.

# Frozen Material Owners And Kernel Routing

- `@dataclass(frozen=True, slots=True, kw_only=True)` owns algorithmic capsules, decorator-bound parameters, and concern witnesses needing `copy.replace` without pydantic or msgspec overhead. `slots=True` eliminates per-instance `__dict__` and pairs with `frozen=True` so assignment failures surface at the owner.
- `msgspec.Struct(frozen=True, gc=False, ...)` owns wire-shaped, decode-shaped, and report-shaped carriers where C-accelerated codecs, `omit_defaults`, `forbid_unknown_fields`, and `tag_field` unions are load-bearing. `gc=False` is a performance contract for acyclic struct graphs; cyclic rich owners keep default GC or relocate cycle-prone state outside the struct. `cache_hash=True` memoizes `__hash__` when frozen owners serve as dict or set keys; invalidation is by replacement identity, not cache-slot mutation.
- `pydantic.BaseModel` with `model_config = ConfigDict(frozen=True)` owns ingress-shaped, settings-shaped, and boundary-validated carriers where env parsing, alias routing, validators, and `computed_field` close the trust surface. Pydantic manages its own layout; do not add `__slots__` to pydantic models.
- Algorithmic capsule, CLI param, concern witness: `@dataclass(frozen=True, slots=True, kw_only=True)` — replacement via `copy.replace` / `dataclasses.replace` — re-validation via `__post_init__` only.
- Wire, report, decode carrier: `msgspec.Struct(frozen=True, gc=False, ...)` — replacement via `msgspec.structs.replace` / `copy.replace` — re-validation via `__post_init__` only; decode validators on ingress only.
- Ingress, settings, rich boundary model: `pydantic.BaseModel` + `frozen=True` — replacement via `model_copy(update=...)` / `copy.replace` — re-validation via `model_validate` for untrusted or computed deltas.
- Policy table, hashable map key: `frozendict[K, V]` — replacement via `|` union / explicit fold — values must enter hashable.
- Large map inside pydantic field: `expression.Map[K, V]` — replacement via `Map` combinators / `model_copy` on parent — parent pydantic validators on `model_copy` / `model_validate`.
- Ordered immutable sequence: `tuple[T, ...]`, `expression.Block[T]` — replacement via `model_copy`, `Block`/`Seq` combinators — parent owner validation.
- Closed capability or set evidence: `frozenset[T]` — replacement via set algebra producing new `frozenset`.
- Surface chooser: bounded hashable policy row → `frozendict`; large keyed registry inside a model field → `expression.collections.Map`; ordered receipt or defect stream → `expression.collections.Block`; closed capability evidence → `frozenset`; fixed-order wire tuple → `tuple`. Route cross-family transitions through the owning family's replacement entry — do not call `dataclasses.replace` on msgspec structs or `structs.replace` on pydantic models.

# Immutable Mapping Surfaces

- Built-in `frozendict` (PEP 814, Python 3.15) is the canonical immutable mapping for policy tables, nested annotation metadata, registry rows, and hashable mapping keys — import from `builtins`; it is not a `dict` subclass. Equality and `hash()` are order-insensitive over items; insertion order is preserved for iteration only. When sequence order is semantic, use `tuple` pairs or an owning value object.
- Persistent map update uses `|` / `|=` where `|=` binds a new frozen mapping; prefer `base | frozendict({key: value})` over mutate-then-freeze dict ladders. `frozendict.copy()` duplicates the frozen shell shallowly; it does not accept field overrides and is not a replacement API. Keys and values must be hashable when the mapping itself is hashed or used as an outer key.
- `expression.collections.Map[K, V]` owns HAMT-backed immutable maps inside pydantic fields and hot update folds where O(log32 N) structural sharing beats repeated `frozendict` union for large maps.
- `types.MappingProxyType` is a read-only view over a live backing mapping — rejected for immutable tables, module-level policy rows, durable configuration snapshots, and model fields presented as configuration evidence. Accept only for ephemeral introspection snapshots (`frame.f_locals`, `locals()` captures) at evaluation boundaries.
- `immutables` and `pyrsistent` are not admitted dependencies.

# Replacement Protocol

- `copy.replace(obj, /, **changes)` (Python 3.13+) is the canonical polymorphic replacement entry across admitted owner families; callers route through `copy.replace` or the family-local alias, not direct `__replace__` calls. Frozen dataclasses synthesize `__replace__` from field metadata; `dataclasses.replace()` and `copy.replace()` share the same kernel and invoke `__post_init__` when defined.
- `msgspec.structs.replace` and `copy.replace` on msgspec structs invoke `__post_init__` when defined — struct replacement is the sole functional update path. Pydantic models expose `__replace__` forwarding to `model_copy(update=...)` without re-running validators unless the transition routes through `model_validate`. `copy.replace` on pydantic models accepts Python field names only, not `Field(alias=...)` keys.
- `NamedTuple` and synthesized `__replace__` stdlib types participate in `copy.replace` directly; they do not participate in pydantic or msgspec replace routing tables. `frozendict` does not support `copy.replace`; mapping transitions use union (`|`) or explicit fold into a new `frozendict`. `init=False` dataclass fields are not carried by `copy.replace` — owning transition methods must close the gap by computing derived init-only state and returning a fully constructed successor.
- Disable synthesized `__replace__` with `__replace__ = None` on a dataclass when generic covariance must not be defeated by contravariant replace parameters — variance and checker contract, not a general immutability escape.

# Structural Sharing And Collection Algebra

- `frozendict` union (`base | overlay`) materializes a new mapping sharing unchanged entries with operands; cost is linear in combined entry count — use for small policy tables, annotation metadata rows, and hashable composite keys where N stays bounded. Normalized policy tables compose as a left-associated union fold: `BASE | sentinel_overlay | measured | overlay`; right-hand keys override left. Lookup on hashable row keys uses order-insensitive `frozendict` equality. When key field order is semantic evidence, use a `NamedTuple`, dataclass row, or `tuple` of pairs.
- `expression.collections.Map` uses HAMT structural sharing: `map.add`, `map.remove`, and `map.change` retain unmodified subtrees — O(log32 N) per transition for hot folds inside pydantic model fields. Parent-owner transitions route through `copy.replace(owner, index=owner.index.add(key, value))`, not mutation of an extracted `dict` view. `Map.empty()` seeds registry tables; multi-key transitions compose as nested `add` folds or `map.change(key, fn)` when the new value depends on the prior `Option` entry.
- `expression.collections.Block` and `Seq` append and map with persistent vector sharing; repeated element-wise updates on ordered evidence streams belong on `Block`, not `list` mutation followed by `tuple(...)`. `Block.empty()` seeds receipt and defect streams; `block.cons`, `block.append`, `block.map`, `block.filter`, and `block.fold` produce successor blocks. `block.fold` with `result.bind` threads state transitions across ordered stages while preserving immutable intermediates. Promote from `tuple` concatenation to `Block` when folds, filters, or parallel accumulators own the sequence algebra.
- `frozenset` union, intersection, and difference produce new sets sharing elements with operands; capability checks at boundaries use subset tests on `frozenset` evidence, not mutable `set` fields. Ordered versus unordered identity: `tuple` and `Block` preserve sequence semantics; `frozendict` and `Map` treat order as non-semantic for equality and hash.
- Annotation metadata on callables folds through `annotationlib.get_annotations(..., format=annotationlib.Format.VALUE)` into `frozendict[str, tuple[Policy, ...]]` rows — durable policy evidence, not a `MappingProxyType` over a builder dict. `Map` and `Block` values inside msgspec or pydantic fields serialize through the parent owner's egress path; do not convert persistent collections to mutable `dict`/`list` at domain boundaries.

# Nested Replacement Grammar

- Single-expression nested transition: `copy.replace(outer, inner=copy.replace(outer.inner, field=value))` — the outer replace closes only after the inner replace completes; partial inner mutation before outer closure is rejected. Multi-field same-level transition: one `copy.replace(owner, field_a=v_a, field_b=v_b)` or one `model_copy(update={...})` with all Python field names — sequential replaces on the same owner only when intermediate owners are intentionally observed on a rail.
- Pydantic nested model fields accept shallow child replacement without re-running constraints unless the transition routes through `model_validate` on the child or full parent snapshot. msgspec nested struct replacement chains through `structs.replace` or `copy.replace` at each level; `__post_init__` on the child runs when the child replace completes, then the parent replace assembles the new parent.
- Collection-field nested replace never mutates the collection in place: `copy.replace(owner, items=owner.items.cons(x))` or `copy.replace(owner, items=owner.items.append(block.of(x)))` on `Block`. Tuple field update rebuilds via slice-and-concat: `owner.tags[:i] + (new,) + owner.tags[i + 1:]` — there is no `copy.replace` on `tuple` itself.

# Tier S, D, And V Routing

- Tier S (shallow, trusted): same-process, same-type field swaps on an already-validated in-session owner via `copy.replace`, `model_copy(update=...)`, or `structs.replace` — nested mutable payloads may alias across successor and predecessor identities.
- Tier D (deep, isolated): untrusted deltas, cross-boundary snapshots, or nested mutable ingress residue via `model_copy(deep=True)`, `model_validate(snapshot | delta)`, or child `model_validate` before parent replace — nested object identity must differ when isolation is load-bearing.
- Tier V (re-validated): wire-sourced, computed, or constraint-replay changes always through `model_validate` / `TypeAdapter.validate_python`, never shallow pydantic replace, even when field types match.
- `frozen=True` on the parent documents shell immutability only; tier selection is explicit per transition family and documented in the owning method contract when callers cannot infer it from ingress versus domain context. msgspec struct graphs marked `gc=False` assume acyclic ownership; deep isolation for nested mutable ingress is promotion to frozen child structs or `convert` at the boundary, not `deepcopy` plus field assignment.
- Tier downgrade at ingress or handler edges is a fault, not a convenience — wire-sourced deltas must not reach shallow replace before tier classification completes.

# Lifecycle And Transition Algebra

- A lifecycle transition is replacement returning a new identity: construction completes into an immutable owner; each subsequent phase emits a successor rather than patching fields in place. Transition methods return `typing.Self`, `Result[Self, E]`, or a narrowed member of a closed family — they do not mutate `self`.
- Queue, task, session, and envelope lifecycles use owner replacement or family dispatch (`match` + `assert_never`), not sentinel mutation of shared mutable containers. Language-owned lifecycle APIs (`asyncio.Queue.shutdown`, `queue.Queue.shutdown`) replace ad hoc sentinel-item repair.
- Version or epoch fields on canonical owners document logical succession when external stores key on business identity rather than object `id`. Replacement produces a new instance; version increment is part of the transition expression, not cache-slot mutation on the predecessor.
- Context-managed transitions return a new owner carrying handles immutably. Resources are not patched on the prior instance after materialization; seam teardown reads the terminal owner snapshot.
- FSM `State` and `Event` enum fields on frozen owners transition through owner replace methods — history persistence stores wire tokens or enum names per contract; replay through boundary decoders materializes successors, not in-place enum field assignment on session owners.

# Normalized Copies And Patch Handoff

- Normalization produces a replacement owner, not in-place repair — wire-key collapse, unit rescaling, default injection, and envelope unwrap happen in boundary `model_validator(mode="before")`, ingress adapters, or one-shot `model_validate` over a snapshot, then the result is frozen for domain use.
- Patch and staging payloads (`TypedDict`, closed payload unions) stop at the materialization boundary; domain modules receive replacement owners, not raw patch mappings. Patch application fold: validate patch via `TypeAdapter(Payload)` → merge evidence into `model_validate(snapshot | delta)` or `copy.replace` on trusted snapshots → emit successor frozen owner. Patch payloads never call `.update` on materialized owners or on nested mutable dict fields.
- `ReadOnly` on payload keys is static assignability evidence only; promotion to durable immutability happens on the replacement product (`frozen=True` owner, `frozendict` row), not on the staging payload type. Open payloads with `extra_items` promote extension bands to `frozendict` snapshots or child frozen models at materialization — the promotion product is the replacement owner, not a permanent parallel type for the same concept.
- Partial nested deltas on pydantic rich owners route through full nested `model_validate` on the child snapshot when any child constraint must replay; shallow parent `model_copy` with a plain dict child slot is a seam defect unless Tier S trust is documented for that field.
- Mutate-then-freeze copies (`dict` update → `frozendict(...)`, field assignment → `dataclasses.replace`) are rejected when a single replacement expression from the prior immutable state can state the transition.

# Cached Derived Values

- Derived values belong on the owner as `@pydantic.computed_field`, read-only `@property`, or an explicit projection method — not module-level caches keyed by owner identity. `functools.cached_property` fails on `@dataclass(frozen=True, slots=True)` and `msgspec.Struct` owners without a dedicated cache slot and manual descriptor policy.
- Slot-backed manual cache pattern: declare a private slot (`_cache_y`), populate lazily inside a `@property` on frozen dataclass or hand-rolled slot class. Prefer recomputation or eager post-init derivation when cache slots would complicate every replacement path.
- Pydantic `computed_field` participates in schema and serialization when the derived value crosses ingress, egress, or settings export; stdlib `@property` suffices for pre-serialization internals only. Replacement invalidates derived state by construction — a new owner identity recomputes or re-enters post-init/computed_field paths.
- `@computed_field` dump policy (`exclude_computed_fields`, `mode`, alias routing) is owned by boundary adapters. Interior transitions must not assume dumped snapshots omit computed evidence that `model_validate` re-entry requires on the same owner family.

# Conversion Versus Replacement

- `msgspec.convert(obj, Type)` and `decode` own ingress materialization from loosely typed carriers; `structs.replace` and `copy.replace` own domain transitions after the first durable struct exists. Do not use `convert` as a shorthand for same-type field swap inside domain logic.
- `pydantic.TypeAdapter.validate_python` and `model_validate` own untrusted or computed deltas (Tier V); `model_copy` and `copy.replace` own trusted same-owner transitions (Tier S). The materialization axis completes validation; the replacement axis assumes prior validation unless Tier V applies.
- `msgspec.structs.asdict` and field-extraction dict spreads are decode/projections for egress or boundary handoff, not replacement kernels. Reconstruction via `Owner(**{**asdict(x), "field": v})` is rejected when replace APIs apply. Cross-family promotion at boundaries follows explicit policy rows: pydantic ingress → `model_validate` → msgspec egress via `msgspec.convert(model.model_dump(mode="python"), StructType)`; domain interiors stay in one family per concept. Alias normalization completes before replacement on re-entry paths: wire keys map to Python field names at the adapter; `copy.replace` with alias keys silently skips fields.

# Result-Bound Transition Pipelines

- Fallible transitions return `Result[Successor, E]`; success carries a new owner identity, error carries typed evidence — `raise` inside domain transition methods is rejected when a rail can express the failure. `pipe(prior, result.map(lambda o: copy.replace(o, phase=next_phase)))` is the canonical success-path step; `result.bind` chains steps returning `Result`.
- `Option.map` + `default_value` collapses absence at the adapter boundary only; domain folds inside `block.fold` / `result.bind` retain `Result` evidence until the owning operation terminates. Parallel validation accumulates defects in `Block[Defect]` without mutating a shared error list; sequential validation short-circuits on first `Error` while prior owner identities remain unchanged. Seam publication sends the defect block or error rail product, not a partially mutated owner.
- Evidence streams that cross context seams materialize as `Block` or tagged struct append products at the exporting boundary — foreign contexts import the frozen stream or fold summary, not the interior `list` or mutable accumulator that built it.
- Algorithm receipts and fold summaries cross context seams as read-only frozen structs when they carry route, status, or sampling evidence. They do not duplicate canonical domain state; receipt export is evidence-only projection.

# Boundary Egress, Seam Routing, And Re-Entry

- Immutable owners exit domain logic only through boundary-owned projection expressions. Domain modules read frozen snapshots; egress adapters own codec choice, alias policy, and deterministic encoding — never in-place field repair before encode.
- Pydantic canonical owners egress through `model_dump(mode="python", by_alias=...)` or `model_dump_json` at boundary modules only; interior folds do not call dump — dump output is wire-staging evidence, not a domain owner and not a replacement kernel. msgspec wire structs egress through module-level `Encoder.encode` on the projected struct. Rich domain owners that never import msgspec expose `to_wire() -> WireStruct` or adapter-owned `project_<concept>_wire(domain) -> WireStruct` field-explicitly — not `msgspec.convert(model.model_dump())` without a documented cross-family policy row.
- `frozendict` policy rows egress as mapping-shaped JSON through boundary codecs accepting `Mapping[str, object]` or a dedicated wire struct row. Do not convert durable `frozendict` tables to mutable `dict` at domain interiors for serializer convenience. `expression.Map` and `expression.Block` inside model fields serialize through the parent owner's pydantic or msgspec egress path — persistent collection combinators stay inside the parent field until the boundary projection materializes a wire tuple, struct list, or tagged stream, not a copied Python `list`/`dict` mid-fold. `NamedTuple` and frozen dataclass witnesses cross subprocess or logging seams as tuple/dict snapshots built at the boundary; they are not ingress owners on the return path unless the seam re-enters through validation.
- Egress preserves shallow-immutability law: encoded snapshots must not embed live mutable object references that alias domain state unless Tier D isolation already promoted nested payloads to immutable child owners. Materialization stage seven is the only stage emitting `bytes` or adapter-owned text from a canonical owner; stages one through six hold frozen instances. Stage-skipping from enriched owner to bytes without a projection owner is a bounded-context adapter exemption, not a leaf-module shortcut. Enrichment-stage successors (`model_copy`, `structs.replace`, `copy.replace`) are still pre-egress owners; egress reads the terminal enriched identity and does not mutate fields to fix encoding gaps discovered late. Trusted-replay rehydration from probe stores may use `model_construct` or trusted `convert` only when store key, schema version, and encoder identity are pinned at the composition root — replay product is a new immutable owner identity, not in-place patch of a cached instance.
- Cross-family promotion at materialization exit follows Conversion Versus Replacement policy rows — `model_dump` → dict surgery → `Struct(**d)` at the seam is rejected when `msgspec.convert` or explicit construction can state the mapping.
- Seam routing is one-shot and typed: each context edge declares source owner, target projection, and whether the handoff is read-only evidence or a re-materialization trigger. Bidirectional owner sync across seams is rejected. Chained seams compose as typed pipelines: ingress validate → canonical replace fold → wire project → encode — each step emits one artifact type; erased `object` handoffs between steps are rejected.
- Ingress bytes → first frozen `BaseModel`/`Struct` via `model_validate`/`decode`.
- Staging payload → domain owner via `model_validate(snapshot | delta)`.
- Domain transition → `copy.replace`/`model_copy`/`structs.replace`/`Map`/`Block` combinators.
- Canonical → wire bytes → `project_wire` + `encode`.
- Union arm change → arm fold + `model_validate` or `copy.replace`.
- Cross-context import → read-only frozen struct or protocol.
- Decorator invocation → frozen config closure and replacement owners on rail.
- Persistence read → `decode` → materialize, not bare replace.
- Closed `@tagged_union` families transition by replacement on the matched arm: `match owner` → arm-specific `copy.replace` or `model_validate` → successor member. Cross-arm discriminant change is explicit in the fold, not hidden in mutating helpers on a shared base. Ingress discriminants map to canonical union arms through adapter-owned tables before domain replace algebra runs; wire tag strings do not flow into `copy.replace` on the wrong arm. Deprecated union arms remain until producers migrate; egress projections route to replacement arms in one adapter fold — deleting an arm while wire still emits it is a seam break. Semi-closed `Extension` arms carry foreign discriminants in frozen overflow (`frozendict`, bounded `TypedDict` with `extra_items`) — extension payload is immutable at handoff. Endofold transitions return a new union member; fallible transitions return `Result[Member, E]`; seam adapters collapse file-internal error unions to boundary errors before the replacement fold publishes across contexts.
- Bytes, `json.loads` output, ORM views, and `MappingProxyType` captures always re-enter at ingress validation regardless of prior session trust; re-entry product is a new materialized owner, not session `copy.replace`, except trusted-replay bytes under pinned encoder policy. `model_validate` / `TypeAdapter.validate_python` owns untrusted and computed re-entry deltas; `copy.replace` and `model_copy` own same-process, same-type, post-validation transitions only. Tier selection at re-entry: Tier V for wire-sourced or computed changes; Tier D for snapshots holding nested mutable ingress residue; Tier S only when the predecessor owner was already validated in-session and the delta is a trusted same-type swap. msgspec `decode`/`convert` materialize the first durable struct at the boundary; `structs.replace` begins only after that identity exists.
- Round-trip proof (`decoder.decode(encoder.encode(project_wire(canonical)))`) runs at boundary egress guards for tagged unions and polymorphic slots — witnesses wire shape legality before persistence, not domain replacement correctness; proof failure maps to boundary `Fault` or `Result` error; it does not authorize domain `copy.replace` repair on the failed value.

# Decorator, Settings, And Context Snapshots

- Decorator factories close over frozen configuration owners (`Field`, `ConfigDict`, `BeartypeConf`) declared in module constants — not literals rebuilt per decoration. Per-invocation state threads through parameters and returned replacement owners on the rail, not mutable closures or builder dicts filled across calls.
- Decorator-bound owners that admit post-init invariants must replay those invariants on replace paths when admission policy requires it — replace paths that skip decorator-bound replay are seam defects, not Tier S shortcuts.
- Settings materialize once at process start as frozen pydantic-settings models; domain boot receives an immutable settings instance or a canonical config record projected once — not live env rereads patched into a shared settings object.
- `ContextVar` stores immutable snapshots set by rebinding to `copy.replace(snapshot, ...)` or a new owner. Mutable accumulators in `ContextVar` patched across tasks are seam defects under free-threaded execution.
- cyclopts `Parameter` metadata projects from frozen dataclass or settings slices; CLI modules do not declare independent mutable field sets. CLI parse product validates into ingress or canonical owners before domain replace algebra.
- Cross-context registry exports freeze at module init or explicit publish time as `frozendict`, `Map`, or `frozenset`. Foreign modules import the published snapshot symbol — not a live builder dict, lazy fill hook, or `Mapping` re-export over mutable storage.
- Worker and multiprocessing entrypoints rebind `ContextVar` from published frozen snapshots — parent-process owner identities do not cross process seams as mutable session state; configuration slices cross, successors materialize locally through boundary decode paths.

# Composition Root Binding

- Python `>=3.15` binds immutable replacement algebra at composition roots so tier routing, family-local replace kernels, lifecycle transition registries, and frozen publication tables import one canonical routing owner — roots never declare parallel `dict` staging, mutable handler accumulators, or ad hoc `model_copy` paths beside the owner transition contract.

# Root Replace Dispatch Catalog

- Composition roots materialize a frozen replace-routing catalog as `frozendict[OwnerAlias, TransitionSpec]` or `tuple[TransitionRow, ...]` beside the canonical owner import — interior modules call owner transition methods; roots publish the catalog for harness parametrization and registry parity tests, not for runtime string lookup in domain folds.
- Each `TransitionRow` is a frozen record carrying `owner: type[Owner]`, `tier: Literal["S", "D", "V"]`, `kernel: Literal["copy_replace", "model_copy", "structs_replace", "frozendict_union", "map_combinator", "block_combinator", "model_validate"]`, and optional `transition: str` naming the owning method — row keys are owner types or closed transition tokens, not free strings reconstructed at handler sites.
- Registry width equals closed transition surface width for each owner family — every documented transition method has exactly one row; orphan rows and undocumented production replace paths are merge blockers caught by catalog parity tests before behavioral suites run.
- Roots import owner modules and transition catalogs only — catalog modules stay dependency-minimal (frozen rows, `Literal`, typing); roots wire catalogs into rail handlers, settings fan-out, and seam exports without catalog modules importing domain operation kernels.
- Module-level `TypeAdapter` and codec singletons cache at root beside owner imports — hot paths do not allocate per-request adapters or re-declare tier defaults on leaf replace call sites.

# Tier Guards At Root

- Root ingress and handler edges classify deltas before replace algebra runs: same-process post-validation field swaps route Tier S; cross-boundary snapshots with nested mutable residue route Tier D; wire-sourced, computed, or constraint-replay deltas route Tier V through `model_validate` / `TypeAdapter.validate_python` at the root guard, not inside interior folds.
- Tier downgrade at root is a fault, not a convenience — handlers that receive `json.loads` output, ORM rows, or foreign `Mapping` views must not call `copy.replace` or shallow `model_copy` before the root tier guard completes.
- Trusted-replay read paths pin encoder identity, store key, and schema version at root — replay product materializes a new owner through `decode` → validate; bare `copy.replace` on a cached session instance without pinned replay policy is rejected at the root replay guard.
- Patch payloads from typed-payload seams stop at root materialization — fold `TypeAdapter(Payload)` → `model_validate(snapshot | delta)` or Tier S `copy.replace` on trusted snapshots → hand successor frozen owner to interior modules; patch mappings never reach domain replace algebra directly.
- Alias normalization completes at root before any replace call — wire keys map to Python field names at the adapter; `copy.replace` and `model_copy(update=...)` with alias keys are negative fixtures in root contract tests.

# Lifecycle Registry At Root

- Queue, task, session, and envelope lifecycles declare transition rows at root as total folds over frozen owners — each legal phase change maps to one replace expression or `Result[Successor, E]` handler registered beside the owner import; language-owned lifecycle APIs replace ad hoc sentinel-item repair in root-bound transports.
- Endofold and variant-arm transitions at root return replacement union members via arm-specific `copy.replace`, `model_validate`, or registry transition rows — cross-arm discriminant change is explicit in the root fold, not hidden in mutable helpers on a shared base imported by handlers.
- Version or epoch increments on canonical owners are part of the root transition expression — `copy.replace(prior, version=prior.version + 1, ...)` or validated reconstruction; cache-slot mutation on the predecessor is rejected at root publication proof.
- Context-managed transitions at root return a new owner carrying handles immutably — `rail(bind)` `finally` blocks read terminal owner snapshots for teardown; resources are not patched on the prior instance after materialization.
- FSM `State` and `Event` enum fields on frozen owners transition through root-bound replace methods — history persistence stores wire tokens or enum names per contract; replay through root decoders materializes successors, not in-place enum field assignment on session owners.

# Seam Map

- Egress cap replace, FSM state successor handoff, and frozen registry publication are root obligations absent from domain-only seam tables — composition roots extend seam law with rows that domain interiors do not own.
- Ingress bytes → first owner: `model_validate` / `decode` materializes initial frozen `BaseModel` / `Struct` at materialization pipeline edge.
- Staging payload → domain owner: `model_validate(snapshot | delta)` validates patch `TypedDict` at typed-payload plus materialization edge.
- Root handler → interior fold: owner transition method or Tier S replace hands successor frozen owner from root handler to interior modules.
- Interior domain transition: `copy.replace` / `model_copy` / `structs.replace` / `Map`/`Block` combinators on prior frozen owner inside domain modules.
- Canonical → wire bytes: `project_wire` + `encode` projects `WireStruct` snapshot at projection lattice edge.
- Union arm change: root arm fold plus `model_validate` or `copy.replace` on prior union member at class-family variant edge.
- Cross-context import: read-only frozen struct export only at shape-system integration edge.
- Decorator invocation: frozen config closure returns replacement owners on rail at decorator admission edge.
- Persistence read: root `decode` → materialize from trusted-replay bytes at materialization pipeline edge.
- Egress cap / truncate: `structs.replace` on envelope fields of canonical owner at composition-root rail edge.
- FSM receipt append: `copy.replace` on state field of frozen owner plus enum state at vocabulary edge.
- Registry publication: `frozendict` / `Map` / `frozenset` frozen table snapshot at module init at composition root.
- Route each edge through its kernel or projection entry — interior domain modules own interior transition rows; foreign-axis rows execute at root adapters and boundary modules, not inside leaf replace folds.
- Chained root pipelines compose as typed steps: ingress validate → canonical replace fold at root handler edge → interior replace-only transforms → wire project → encode — no step widens owners to mutable staging dicts between frozen handoffs.

# Rail Integration

- Root egress guards apply `structs.replace` and `model_copy` only on envelope and report carriers at the cap-truncation boundary — canonical domain owners inside handler success paths transition through owner methods before `_emit` sees them; egress does not repair domain fields to fix encoding gaps.
- `_emit` folds `Result[Report, Fault]` into one `Envelope`, applies cap policies via `structs.replace`, and routes `FAILED` reports through diagnostic distillation — truncation sets `truncated=True`; full payloads spill to history artifacts, not partial wire lines patched on mutable staging structs.
- Per-invocation `ContextVar` rings reset in `finally` around every `rail(bind)` call — rebinding uses `ctx.set(copy.replace(snapshot, ...))` or a new frozen owner; mutable accumulators patched across invocations are root seam defects under free-threaded execution.
- Success-path `_guard` chains bind replace steps on `Result` without nested `try` inside `bind` bodies — `pipe(prior, result.map(lambda o: copy.replace(o, phase=next)))` at root orchestration only when the bounded context documents root-owned phase transitions; interior domain modules own their transition methods.
- Round-trip proof at root (`decoder.decode(encoder.encode(project_wire(canonical)))`) witnesses wire legality for persistence-dependent owners — proof failure maps to boundary `Fault`; it does not authorize domain `copy.replace` repair on the failed value.
- Composition-root egress guards prove `decoder.decode(encoder.encode(project_wire(canonical)))` for owners whose persistence or cache keys depend on bytes — proof witnesses wire legality, not domain replace correctness; failures map to boundary `Fault`, not domain repair.
- Canonical frozen owner to wire projection proves shallow-immutability on encoded snapshots — dump output must not embed live mutable aliases unless Tier D child promotion samples already isolated nested payloads.
- Cross-family promotion samples follow declared conformance rows: pydantic canonical → explicit `project_wire` → msgspec encode — `model_dump` mutation before struct construction is a negative fixture.
- Trusted-replay read paths prove `decode → materialize` produces new owner identity — bare `copy.replace` on cached session instance without pinned encoder policy is a negative control.
- Settings materialize once at process start as frozen pydantic-settings models — root `project_*_config` emits canonical boot records projected once; domain boot receives immutable settings or frozen config slices, not live env rereads patched into a shared settings object.
- Decorator factories at root close over frozen configuration owners (`Field`, `ConfigDict`, `BeartypeConf`) declared in module constants — per-invocation state threads through parameters and returned replacement owners on the rail, not mutable closures filled across calls.
- cyclopts `Parameter` metadata projects from frozen dataclass or settings slices at root — CLI parse product validates into ingress or canonical owners before domain replace algebra; CLI modules do not declare independent mutable field sets.
- Cross-context registry exports freeze at module init or explicit publish time as `frozendict`, `Map`, or `frozenset` — foreign modules import the published snapshot symbol from the root `__all__`, not a live builder dict, lazy fill hook, or `Mapping` re-export over mutable storage.
- Worker and multiprocessing entrypoints rebind `ContextVar` from root-published frozen snapshots — parent-process owner identities do not cross process seams as mutable session state; configuration slices cross, successors materialize locally through root decode paths.
- Root-published policy tables, handler maps, and remap rows use `frozendict` for bounded hashable rows and `expression.Map` for large keyed registries materialized once at import — repeated single-key updates in root warm-up folds use `Map.add`, not `frozendict` union inside loops.
- Receipt, defect, and routed-job streams appended at root use `Block` combinators — `block.cons`, `block.append`, and `block.fold` with `result.bind` thread state without mutating shared outer lists captured from enclosing scopes.
- Root warm-up must not convert `Map`, `Block`, or `frozendict` fields to mutable `dict`/`list` for encoder or registry convenience — persistent collection combinators stay inside parent fields until boundary projection at egress.
- Capability and policy evidence at root publication uses `frozenset` — subset checks at root guards compare enum or capability members with frozen set evidence, not mutable `set` fields on shared module state.

# Proof Harness Alignment

- Root contract modules import the same frozen replace-routing catalog production handlers use — parametrized tier matrix tests derive rows from `TransitionRow.tier`, not caller inference at the test site.
- Hypothesis strategies at root draw replace transitions from closed catalog exemplars per owner — `st.sampled_from` over `(prior, delta, tier)` tuples registered beside the root import graph, not arbitrary dict patches filtered by runtime validation.
- Catalog parity tests iterate owner transition methods and routing rows in one assertion — set difference in either direction fails CI before property suites run.
- Root egress samples pair `structs.replace` cap truncation with canonical owner replace transitions — interior replace must not assume dump omits fields that re-entry validation requires.
- Free-threaded root publication samples assert `frozendict`, `Map`, and `frozenset` tables are fully materialized before worker threads import root symbols — lazy mutation behind `MappingProxyType` fails publication proof.
- Mutation testing on root tier-routing branches requires kill ratio on downgrade paths — mutants that route Tier V deltas through shallow replace at the root guard must fail contract or property tests.
- Import-linter and custom analyzer rules flag in-place domain mutation on frozen owners inside root handler modules — `model.field =`, nested dict item assignment, and mutate-then-freeze ladders fail lint when repo policy rows apply.
- Python minor upgrades pin behind root replace-kernel smoke samples: frozen dataclass, msgspec struct, pydantic frozen model, `NamedTuple`, and `frozendict` union — `copy.replace` availability and `frozendict` builtins import are version gates executed from root test seams.
- Pydantic upgrades require root shallow-replace non-revalidation samples refreshed when `model_copy` semantics shift — trust-boundary documentation beside root-owned owners updates with sample expectations.
- Composition-root changes to default tier posture trigger full tier matrix re-run for affected owners in the routing catalog — stage-skipping exemptions require documented proof waiver rows beside the root module.
- Direct `__replace__` call sites in root and handler modules fail grep or custom policy when `copy.replace` or family alias applies — application modules use polymorphic entry only.
- New mutable `dict`/`list`/`set` fields on `frozen=True` owners bound at root are merge blockers — static review plus root replace proof must show collection transitions through `Map`/`Block`/tuple/`frozenset`/`frozendict`.
- Root mutable staging dict: handlers build `dict` patches then `model_validate` or `replace` at the end — collapse to single-expression `model_validate(snapshot | delta)` or nested `copy.replace` from the prior frozen owner.
- Tier leak at handler: wire-sourced delta reaches `model_copy(update=...)` without root Tier V guard — collapse to root `TypeAdapter.validate_python` before interior handoff.
- Shared closure accumulator: decorator or `rail` closure mutates outer `dict`/`list` across calls — collapse to frozen config capture plus returned replacement owners on the rail.
- Registry lazy fill: module-level handler `dict` populated by side-effect registration — collapse to frozen `frozendict` or `tuple[TransitionRow, ...]` materialized at import with parity tests.
- Egress domain repair: `_emit` or encode path mutates canonical owner fields before `project_wire` — collapse to boundary projection from terminal enriched identity without field assignment.
- Replay bare replace: cached session owner updated via `copy.replace` on decoded bytes without pinned encoder policy — collapse to root `decode` → materialize producing new identity.
- Collection mid-root mutation: root warm-up converts `Block`/`Map` to `list`/`dict` for registry build — collapse to persistent combinators through parent owner fields until egress projection.
- Done when every root-bound owner transition routes through a catalog row with explicit tier, every cross-axis handoff in the seam map names a root or adapter entry, frozen publication tables materialize before foreign import, and CI gates fail on tier downgrade before handlers execute.

# Binding Defect Signals

- Root handler imports `dataclasses.replace` for msgspec structs or `structs.replace` for pydantic models — family-local kernel violation from owner selection table.
- Interior module imports root replace-routing catalog to choose kernels at runtime — catalog is harness and parity evidence, not leaf dispatch.
- `MappingProxyType` over module-level mutable dict presented as durable configuration in root `__all__` exports.
- Parallel `{Concept}Draft` types in root handlers batching writes before final freeze when single nested replace or `model_validate` over staging payload states the transition.
- Root property tests using `st.from_type(FrozenModel)` without registered exemplars — exercises validation failures instead of replace law at the composition root.
- Skipping catalog parity when adding owner transition methods — production paths exist without `TransitionRow` witnesses.
- Root import graph depth: owner and catalog modules imported transitively through domain modules reintroduce cycles — keep owner modules leaf-ward and bind catalogs at root only through adapter and handler import edges named in `__all__`.
- Tier orthogonality vs strict policy: root `--strict` promotes policy faults on empty folds independent of tier classification — strict posture and Tier V replay are separate proof obligations on the same handler edge.
- Encoder determinism and replace identity: persistence keys on encoded bytes require root encoder `order="deterministic"` pinned at module level; replacement-produced successors change logical version fields without reusing encoded bytes from the predecessor.
- Hypothesis shrink on tier composites: shrinking must preserve tier legality; custom composites rebuild valid predecessors after shrink rather than downgrading Tier V fixtures to Tier S mutations.
- Free-threaded catalog publish: replace-routing catalogs and registry tables must finalize as immutable structures before worker threads import root symbols; post-import mutation of transition rows races under parallel importers.
- Block fold error short-circuit: root validation that accumulates defects in `Block[Defect]` must not mutate partially validated owners on short-circuit `Error` — prior identities remain unchanged until the root fold publishes a single successor or fault product.

# Proof Contract Routing

- Immutable replacement owners carry four independent proof surfaces — successor identity, tier routing, shallow-shell law, and family-local replace kernel dispatch; passing one surface does not discharge the others.
- Proof samples live beside the owning family module — dataclass capsules, msgspec structs, pydantic frozen models, and `frozendict` table rows each maintain a minimal contract table keyed by transition family; adapters read production replace paths, tests assert the same paths.
- Static proof (mypy `exhaustive-match`, ty all-error, dataclass replace variance) and runtime proof (beartype on transition entrypoints, Hypothesis replace-law draws) are separate rails — both must pass for owners that cross context seams or publish registry evidence.
- Contract suites parametrize tier rows from owning transition method contracts, not caller inference at the test site; registry strategies draw from production owner alias types, not wire struct modules or dump dicts.
- Successor identity: every material transition asserts `successor is not prior` and field-delta equality on changed slots only — unchanged field objects may alias under Tier S; Tier D and Tier V samples assert nested isolation when policy requires it.
- Single-expression closure: nested replace proofs use one outer expression `copy.replace(outer, inner=copy.replace(outer.inner, field=v))` — sequential bare replaces on the same owner without intermediate observation are folded into one sample to match production transition methods.
- Multi-field same-level replace proves one `copy.replace(owner, a=v_a, b=v_b)` or one `model_copy(update={...})` with Python field names — alias keys in update dicts must fail the sample (silent skip proof), matching re-entry law from seam routing.
- `init=False` dataclass fields: proof samples supply owned transition methods that close init-only slots — bare `copy.replace` missing init-only fields must fail in contract tests, not in production.
- `__replace__ = None` variance opt-out: static proof documents read-only generic boxes without synthesized replace — contract suite skips replace-law draws on those owners and proves covariance intent through checker fixtures instead.
- `frozendict` replace kernel uses union only: proof asserts `copy.replace(frozendict({}), key=v)` raises `TypeError` and `base | frozendict({key: value})` produces order-insensitive equal successor — union commutativity for equality holds regardless of operand order in the sample table.
- Each owning transition method documents its tier in the method contract — proof suite parametrizes tier rows from that contract, not from caller inference at the test site.
- Tier S proof obligation: successor identity; optional nested alias witness when nested payloads are immutable child owners — entry `copy.replace`, `model_copy(update=...)`, `structs.replace`.
- Tier D proof obligation: nested object identity differs from predecessor; no shared mutable dict/list aliases — entry `model_copy(deep=True)`, child `model_validate` before parent replace.
- Tier V proof obligation: constraint violation replay on invalid delta; shallow replace on same delta must fail or diverge — entry `model_validate`, `TypeAdapter.validate_python`.
- Tier downgrade defects are explicit proof failures: applying Tier S shallow replace to a wire-sourced delta sample must not pass when Tier V policy owns the method.
- msgspec acyclic `gc=False` graphs: Tier D proof promotes nested ingress to frozen child structs or boundary `convert` — `deepcopy` plus assignment samples are rejected fixtures, not proof paths.
- Frozen dataclass and msgspec struct replace samples invoke `__post_init__` on the replaced instance — post-init invariant violations surface on replace, not only on first construction.
- Pydantic frozen model shallow replace samples prove constraints do not re-run: a field value violating `Field(ge=...)` applied through `model_copy(update=...)` may succeed unless Tier V reroutes through `model_validate` — trust-boundary fact, not validation evidence. Tier V pydantic samples apply invalid computed deltas through `model_validate` and assert `ValidationError` — matching shallow replace on the same delta is a negative control proving tier separation.
- Rich owner full-snapshot transitions (`model_validate` over `{name: getattr(self, name) for name in fields}` with delta) replay all validators — selective override samples prove nested child snapshots validate before parent assembly.
- Decorator-admitted owners prove post-init replay asymmetry: replace paths that skip decorator-bound invariants must fail contract tests when admission policy requires replay on transition.
- `frozendict` union samples assert unchanged value objects are identical (`is`) across predecessor and successor for untouched keys — changed keys bind new objects; outer shell is always a new `frozendict` identity.
- `expression.Map` add/remove samples assert O(log32 N) path sharing informally via unchanged subtree identity where test harness exposes prior map reference — at minimum, successor map `is not` predecessor and lookup equality holds. `Block.cons`/`append` samples assert predecessor block identity is preserved and successor carries extended sequence — in-place list mutation on extracted views is a negative fixture.
- Tuple field replace samples use slice-concat or `+` — proof asserts no `copy.replace` on bare `tuple`; parent owner replace carries the new tuple value.
- `frozenset` algebra samples prove new set identity on every transition — in-place `set.update` on a stored mutable copy is a defect fixture excluded from proof tables.
- Hashable `frozendict` key rows prove `hash(row_a) == hash(row_b)` when mappings are equal regardless of insertion order — order-sensitive key evidence uses `NamedTuple` or tuple pairs with distinct equality samples.
- Nested hash defects are positive proof failures: a key row holding a mutable list value must fail hash or break stability across a mutation probe on the nested object.
- msgspec `cache_hash=True` samples prove hash memoization survives across replace when field values unchanged — replace with changed hashable field produces new hash without mutating predecessor memo slots.
- Owner identity caches keyed by `id(owner)` are rejected — proof uses value keys or explicit version fields; samples assert two replacements produce distinct ids and correct version monotonicity when version is load-bearing.
- Hypothesis strategies draw replace transitions from closed tier and family registry rows — `st.sampled_from` over `(prior, delta, tier)` exemplars per owner, not arbitrary dict patches filtered by runtime validation.
- Replace-law properties: `successor is not prior`; changed fields match delta; unchanged fields alias under Tier S; nested isolation under Tier D; validation replay under Tier V — each law registers as a separate `@given` target per owner family.
- `frozendict` union properties assert equality commutativity and override semantics (rightmost key wins) — generate random key sets and union orders, assert canonical normalized result.
- `Map` fold properties generate sequences of `add`/`remove` operations and assert equivalent fold from empty seed matches batch union reference implementation for small N only — large-N performance belongs in bench rails, not property proof.
- Shrinking preserves tier legality — custom composites rebuild valid predecessors after shrink; invalid tier downgrade mutations reject at construction gate.
- Registry-driven catalog rows embedding immutable owners register strategies through the same owner alias types production uses — not wire struct modules or dump dicts.
- Free-threaded samples publish frozen successors across threads — shared mutable closure or `ContextVar` accumulator mutation probes are negative fixtures; `ContextVar.set(copy.replace(snapshot, ...))` rebinding samples pass.
- Module-level registry publication samples assert `frozendict`, `Map`, or `frozenset` materialized at init — lazy mutation behind `MappingProxyType` fails publication proof.
- Decorator closure samples assert frozen configuration capture — per-invocation state threads through returned replacement owners, not mutable outer dicts filled across calls.
- `@computed_field` egress samples pair serialization-mode schema snapshots with replace transitions — interior replace must not assume dump omits fields that re-entry validation requires.
- Mypy and ty prove exhaustive `match` on replace-routing dispatch tables — new owner family row without fold update fails CI before behavioral tests.
- Ruff policy bans in-place domain mutation patterns on frozen owners — `model.field =`, nested dict item assignment, and mutate-then-freeze ladders fail lint when flagged by repo policy rows.
- Mutation testing on tier-routing branches requires kill ratio on downgrade paths — mutants that route Tier V deltas through shallow replace must fail contract or property tests.
- Adding a mutable `dict`/`list`/`set` field to a `frozen=True` owner is a merge blocker — static review plus sample replace proof must show collection transitions through `Map`/`Block`/tuple/`frozenset`/`frozendict`, not in-place methods.
- Direct `__replace__` call sites fail grep or custom policy when `copy.replace` or family alias applies — application modules use polymorphic entry only.
- Python minor upgrades pin behind replace-kernel smoke samples: frozen dataclass, msgspec struct, pydantic frozen model, `NamedTuple`, and `frozendict` union — `copy.replace` availability and `frozendict` builtins import are version gates.
- Pydantic upgrades require shallow-replace non-revalidation samples refreshed when `model_copy` semantics shift — trust-boundary documentation in owner modules updates with sample expectations.
- Composition-root changes to tier defaults trigger full tier matrix re-run for affected owners — stage-skipping exemptions require documented proof waiver rows.
- New `frozendict` or `expression.Map` dependency versions run hash and union equality regression rows — order-insensitivity is a pinned contract, not an incidental behavior.
- `functools.cached_property` on `slots=True` frozen dataclass or msgspec struct without cache-slot design fails import-time or dedicated negative sample — documents PEP 779 adjacent failure mode.

# Composite Identity And Hashing

- Hashable `frozendict` keys require hashable values at every nesting level used in equality or `hash()`. Row-shaped keys prefer small `frozendict` literals, frozen dataclass rows, or `NamedTuple` rows depending on whether mapping semantics, derived fields, or fixed column order dominate.
- Owner identity for evidence: successor `is not` predecessor after every material transition; caches keyed by `id(owner)` are rejected — use value keys (`frozendict`, hashable structs with `cache_hash=True`) or explicit version fields. Equality across families is not assumed: pydantic `model_dump` dict equality is not owner equality; `frozendict` order-insensitive equality differs from `tuple` pair sequence equality.

# Concurrency And Publication

- Under free-threaded execution (PEP 779), shared mutable owner state is a data race; frozen owners and replacement-produced successors are safe to publish when no nested mutable payload aliases untrusted shared state. Registry and catalog tables exposed to domain logic are immutable — `MappingProxyType` over module-level mutable dicts is not an acceptable publish surface.
- Registry and catalog tables must finalize as immutable structures before worker threads import published symbols; post-import mutation of publication rows races under parallel importers.

# Mutation Boundaries

- Mutation is permitted only at explicit boundaries: ingress parsers materializing first owners, egress encoders reading immutable snapshots, framework-owned buffers, and analyzer-governed boundary exemptions. Domain modules between boundaries are replacement-only.
- `ReadOnly[T]` on `TypedDict` payload keys is static evidence only, not runtime immutability. Durable immutability after promotion belongs to frozen owners and `frozendict`, not to payload comments.
- `beartype` and pydantic validators gate boundary adapters after typed materialization; they do not authorize in-place domain mutation of frozen owners.

# Variance Opt-Out

- Synthesized `__replace__` on generic frozen dataclasses can force invariant type parameters on read-only boxes. Set `__replace__ = None` when replace is unused and covariance on the type parameter is load-bearing — a variance and checker contract, not a general immutability escape. Owners that perform transitions must keep synthesized replace or expose an owned method calling `dataclasses.replace` internally.
- Static checkers may still treat fields as invariant after opt-out; document read-only intent through `typing.ReadOnly` on dataclass `TypedDict` staging shapes and frozen material owners at promotion.

# Rejection And Defect Signals

- In-place field assignment on materialized domain owners; mutate-then-freeze ladders; `MappingProxyType` over module-level mutable dicts presented as immutable policy; tuple-of-pairs pseudo-maps; parallel `{Concept}Draft` mutable types; parent `frozen=True` with child field typed as mutable `dict`, `list`, or `set` updated through `.update`, `.append`, or item assignment on the nested object.
- `immutables`, `pyrsistent`, and other non-manifest persistent libraries; `functools.cached_property` on `slots=True` frozen dataclasses or msgspec structs without explicit cache-slot design; direct `__replace__` calls at application sites when `copy.replace` applies; field-extraction reconstruction instead of replace APIs; `deepcopy` followed by mutation; lifecycle sentinels when replacement or language sentinels own the concern; using `convert` as a same-type field-swap shorthand inside domain logic; assuming shallow pydantic replace re-validates constraints.
- Quadratic `frozendict` union inside a loop over keys or events where `Map.add` would be logarithmic per step; `block.fold` or `result.bind` steps mutating a shared outer `dict`, `list`, or `set` captured from an enclosing scope instead of threading replacements through the fold seed.
- Shallow pydantic `model_copy` or `copy.replace` on snapshots holding nested mutable ingress residue without Tier D or Tier V routing; re-entry `copy.replace` on wire-sourced deltas without `model_validate` replay; egress path that converts `Map`/`Block`/`frozendict` fields to mutable collections for encoder convenience inside domain modules.
- Shared mutable closure state in decorators when frozen configuration plus returned replacement owners can thread the same evidence; union transition that mutates a field on the matched arm instead of returning a replacement member or explicit cross-arm fold product; parallel `{Concept}Wire` domain class that duplicates canonical invariants instead of a boundary `project_wire` from one frozen canonical owner.
- `model_dump` or `asdict` output mutated before re-validation and presented as a domain owner; publishing `MappingProxyType` over module-level mutable dicts as durable configuration to foreign contexts; skipping wire round-trip proof for tagged union egress when persistence or cache keys depend on encoded bytes.
- Contract tests that reconstruct owners via `Owner(**{**asdict(x), "field": v})` instead of replace APIs; Tier S samples applied to wire fixtures or `json.loads` output without re-entry validation.
- Property tests using `st.from_type(FrozenModel)` without registered exemplars — generates invalid deltas that exercise validation failures instead of replace law.
- Proof tables keyed by `id(owner)` or assuming mutable registry publication; parallel `{Concept}Draft` types tested only through final freeze.
- Skipping hash stability samples for nested `frozendict` key rows because "small table" — composition defects surface at scale in catalog folds.
- Egress round-trip proof omitted for tagged owners whose cache keys are encoded bytes — enrichment and manual assembly still drift without composition-root guard samples.
- Skipping wire round-trip proof for tagged union egress when persistence or cache keys depend on encoded bytes — proof belongs at the composition-root guard, not as an optional test-only concern.

# Runtime And Checker Truth

- Python 3.15 ships `frozendict` in `builtins` (PEP 814). `copy.replace` ships in `copy` (3.13+). Verified on CPython 3.15.0b1: `copy.replace` works on frozen dataclasses, `NamedTuple`, pydantic frozen models, and msgspec frozen structs; raises `TypeError` on `frozendict`. `frozendict` union is order-insensitive for equality and hash. `MappingProxyType` reflects backing-map mutation. `functools.cached_property` raises on frozen `slots=True` dataclasses and msgspec structs.
- Pydantic `model_copy(update=...)` and `copy.replace` on frozen models accept field values without re-running `Field` constraints in the default configuration — treat that as a trust-boundary fact, not validation evidence. `__replace__` synthesis can make generic type parameters appear invariant to static checkers.
