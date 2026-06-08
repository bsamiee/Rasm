# Structural Sharing, Identity Lattice, And Copy-On-Write Positioning

# Sharing Band Taxonomy

- Immutable replacement admits three sharing bands — `value_copy`, `path_copy`, and `intentional_alias` — each with distinct identity law, amortized cost, and proof obligation; band selection is architectural, not a micro-optimization after the fact.
- Value-copy band: PEP 814 `frozendict` construction (`frozendict(dict)`, `frozendict | frozendict`, `frozendict | dict`) materializes a new shell with O(n) shallow entry duplication per PEP 814 specification; unchanged value objects may alias operands (`successor['k'] is predecessor['k']`) but the mapping shell never aliases; hash and equality are order-insensitive (`hash(frozendict)` ≡ `hash(frozenset(items()))`).
- Path-copy band: `expression.collections.Map` and `Block` retain unmodified subtrees across `add`, `remove`, `change`, `cons`, and `append`; successor collection `is not` predecessor while shared interior nodes may alias; hot folds over large keyed registries and ordered evidence streams belong here — not PEP 603 `frozenmap` (O(1) mutation, O(log n) lookup) nor repeated `frozendict` union inside event loops.
- Intentional-alias band: Tier S same-process field swaps on owners whose nested payloads are already immutable child owners or value types; unchanged field slots may alias across successor and predecessor; this band is trust-gated, not a default for ingress residue.
- Band downgrade — routing a large-N hot fold through repeated value-copy union, or routing wire-sourced nested mutable residue through intentional-alias — is a composition defect independent of whether the outer owner shell is frozen.

# Copy-On-Write Positioning

- PEP 814 (Final, Python 3.15) adds `frozendict` to `builtins` — not a `dict` subclass, inherits `object`, implements `collections.abc.Mapping`, supports pickling, and guarantees shallow shell immutability after construction; thread-safe publication without locks once constructed when nested values are not mutated by other threads (PEP 814 thread-safety clause).
- PEP 814 explicitly defers O(1) `dict.freeze()`, copy-on-write promotion (`frozendict(dict)` sharing hash table until first mutation), and literal syntax — deferred family aligns with PEP 351 freeze-protocol; current construction cost is O(n) shallow copy and is not pinned as permanently O(n) in the PEP.
- Doctrine treats deferred COW as a future boundary-only acceleration for pre-admission builders, not a domain-transition kernel; domain interiors stay on admitted replace APIs regardless of stdlib COW arrival.
- `frozendict.copy()` returns the same object with a new reference in CPython (PEP 814 copy semantics) — identity duplication, not a replacement transition; field overrides and patch semantics belong on `|` union or explicit fold, not on `.copy()`; shallow copy aliases nested mutable values (PEP 814 `mutable=[]` example).
- `|=` on `frozendict` binds a new frozen mapping — left operand unchanged, same semantics as `|` without in-place mutation (PEP 814 union section).
- Mutate-then-freeze (`dict` ladder → `frozendict(...)`) and hypothetical future `dict.freeze()` occupy the same rejected seam class when a single replacement expression from the prior immutable state can state the transition.
- When stdlib COW lands, composition roots may adopt it only at ingress builder seams under documented policy rows; interior `copy.replace`, `Map` combinators, and tier routing remain unchanged.
- `MappingProxyType` over live backing storage is not COW immutability — PEP 814 cites non-hashable proxy, backing-map retrieval via `gc.get_referents()`, and live mutation reflection; proxy stays rejected for durable tables regardless of COW roadmap.

# Identity Lattice

- Replacement correctness decomposes into four independent identity axes — owner shell, collection shell, subtree node, and nested payload — passing one axis does not discharge the others.
- Owner shell: every material transition produces `successor is not predecessor` on the canonical frozen owner; `id(owner)` caches and identity-keyed memo tables are rejected — version fields and value keys own succession evidence.
- Collection shell: `frozendict`, `Map`, `Block`, `tuple`, and `frozenset` transitions always bind a new collection identity even when interior sharing aliases; treating collection replace as in-place update is a shallow-law violation.
- Subtree node: path-copy bands prove retained subtree aliasing only where the combinator documents structural sharing; value-copy bands prove entry-value aliasing for untouched keys, not shell aliasing.
- Nested payload: Tier S permits alias; Tier D requires `successor.nested is not predecessor.nested` when isolation is load-bearing; Tier V requires validation replay that may or may not preserve alias depending on constructor semantics — tier is orthogonal to sharing band.
- Hash identity: `frozendict` and hashable struct equality are value-defined; `msgspec` `cache_hash=True` memoizes on the instance and invalidates only via replacement-produced successor — mutating predecessor memo slots is impossible on frozen owners but successor hash must reflect changed fields.
- Cross-family identity: pydantic dump dict equality, `msgspec.structs.asdict` equality, and owner equality are unrelated witnesses — proof tables stay inside the owning family per axis.
- Frozen dataclass `slots=True` eliminates per-instance `__dict__` — replacement produces a new slot layout instance; manual cache slots (`_cache_*`) complicate every morphism and are avoided unless eager derivation is infeasible.
- `functools.cached_property` on `slots=True` frozen dataclasses and msgspec structs without cache-slot design is rejected — lazy derived fields belong on pydantic `computed_field`, eager `__post_init__`, or recompute-on-replace, not undeclared descriptor caches.
- Pydantic `frozen=True` manages its own layout — slot doctrine on dataclass capsules does not transfer; shallow replace on pydantic still skips constraint replay unless Tier V applies.

# Amortized Transition Routing

- Bounded N (policy tables, annotation metadata, hashable composite keys, registry rows under ~10² keys): value-copy `frozendict` union with left-associated normalized fold — `BASE | overlay_a | overlay_b`.
- Transition count T with varying keys on a growing registry: when T×N union work exceeds path-copy amortized bounds, migrate the field to `expression.Map` at the next owner replacement — band migration is an owner-field promotion, not a helper extraction.
- Unbounded or hot keyed registry inside a pydantic or msgspec model field: path-copy `Map.add` / `map.change` fold seeded from `Map.empty()` — quadratic union inside event loops is a routing defect caught at root warm-up review.
- Ordered receipt, defect, and routed-job streams: path-copy `Block.cons` / `append` / `map` / `fold` — promote from `tuple` concatenation when fold algebra owns the stream; `block.fold` with `result.bind` threads state without mutable outer lists.
- Fixed-order wire tuples and small closed sequences: `tuple` slice-concat or `+` carried through parent `copy.replace` — bare `tuple` has no replace kernel.
- Closed capability evidence: `frozenset` algebra only — mutable `set` staging before freeze is rejected at the same seam class as dict ladders.
- Multi-key same-level owner transition: one `copy.replace` / `model_copy` / `structs.replace` closing all fields — sequential single-field replaces on the same owner are reserved for intentional intermediate observation on rails.
- `@dataclass(frozen=True, slots=True, kw_only=True)` capsules default replace morphism band intentional-alias on scalar fields — collection fields on the same owner still follow collection band rules, not capsule defaults.

# Publication Under Concurrency

- PEP 779 (Final, accepted 2025-06-16) establishes Phase II criteria met in Python 3.14+: free-threaded CPython is officially supported but still optional (compile-time choice, not default); Phase III default-flip remains undecided — frozen shells published before foreign import are safe to read without locks when shallow-immutability law holds on all nested payloads exposed to parallel readers.
- PEP 779 Phase II hard targets: ≤15% single-thread performance regression, ≤20% memory overhead (pyperformance geometric mean), extension-module coverage, and internal free-threading documentation — band-finalized tables must meet publication proof under these builds, not only GIL-enabled interpreters.
- `frozendict` (PEP 814 thread-safety), finalized `Map`, `frozenset`, and frozen owner instances are publication-grade; lazy mutation behind module init hooks, `MappingProxyType`, or post-import catalog patching races under parallel importers and worker threads.
- `ContextVar` publication rebinding sets a new immutable snapshot — `ctx.set(copy.replace(snapshot, ...))` — never patches mutable accumulators captured from enclosing scopes across tasks.
- `concurrent.interpreters` and multiprocessing seams treat parent-process owner identities as non-portable; configuration slices and encoded bytes cross; successors materialize locally through boundary decode — not session `copy.replace` on cached instances.
- Extension codecs compiled for free-threaded builds (PEP 803 `abi3t`) do not relax shallow-immutability or tier law — faster decode paths still materialize new frozen owners at ingress.

# msgspec Acyclic Graph Contract

- `gc=False` on `msgspec.Struct(frozen=True, ...)` is an ownership-graph contract: struct instances in a `gc=False` graph must not participate in reference cycles — cycles containing only `gc=False` nodes leak permanently.
- Wire, report, and decode carriers with tree-shaped or DAG-shaped ownership qualify; rich cyclic domain graphs keep default GC or relocate cycle-prone handles outside the struct capsule.
- `cache_hash=True` pairs with hashable field graphs used as dict/set keys; replacement with unchanged hashable fields may reuse hash semantics on the successor via fresh construction paths — predecessor memo is not mutated.
- `msgspec.structs.force_setattr` is confined to `__post_init__` on frozen structs before the instance is handed to domain logic — domain transitions use `structs.replace` / `copy.replace`, not force setattr repair.
- `convert` / `decode` materialize first durable struct identity at boundaries; `structs.replace` begins only after that identity exists — decode/materialize axis and tier S/D/V axis are independent; collapsing them at ingress produces tier leaks.

# Boundary Deep-Immutable Ingress

- Python 3.15 `json.loads` / `json.load` admit `object_pairs_hook=frozendict` and `array_hook=tuple` (or a closed tuple factory) to produce deeply shallow-immutable parse trees at the wire seam — this is ingress evidence, not a canonical domain owner.
- Deep-immutable JSON trees still require `TypeAdapter.validate_python` or `msgspec.convert` promotion before domain replace algebra — parse-tree immutability does not downgrade Tier V on wire-sourced deltas.
- `annotationlib.get_annotations(..., format=annotationlib.Format.VALUE)` folded into `frozendict[str, tuple[Policy, ...]]` rows is durable policy evidence at decoration boundaries — not a `MappingProxyType` over a builder dict.
- ORM `Mapping` views and `frame.f_locals` captures may use `MappingProxyType` only as ephemeral introspection snapshots — promotion to domain owners routes through validation, not proxy presentation as configuration.

# Tier Orthogonality To Sharing Band

- Tier S/D/V classifies trust and validation replay on deltas; sharing band classifies cost and alias law on successors — orthogonal axes composed at every transition contract.
- Tier S with value-copy band: same-process trusted field swap on small `frozendict` policy overlay — union morphism, no `model_validate`.
- Tier S with path-copy band: trusted `Map.add` on in-session registry — combinator morphism, child payloads may alias when immutable.
- Tier D with either band: nested isolation required regardless of path-copy subtree sharing — `model_copy(deep=True)` or child `model_validate` before parent replace.
- Tier V with either band: constraint replay required — shallow replace on the same delta is a negative control even when band would permit alias.
- Band selection never downgrades tier at ingress; tier selection never excuses band violation on hot folds — root guards classify both before interior handoff.

# Construction Stage And Band Placement

- From first durable owner construction onward, band law applies on collection fields — ingress carriers and staging payloads are pre-band mutable evidence only.
- Construction and enrichment emit owners whose collection fields already sit in the correct sharing band — promoting from mutable staging to `Map`/`Block`/`frozendict` at construction, not at first domain fold.
- Last construction checkpoint before domain handoff is the final point for nested payload promotion — child mutable residue crossing into durable domain use without promotion is a shallow-law breach.
- Outbound serialization reads terminal identity without changing band — persistent collections inside model fields stay on combinator representations until boundary projection; mid-fold `dict`/`list` conversion for encoder convenience violates band ownership.
- Trusted-replay persistence read re-enters at decode materialization — replayed owners pick up band from the owner-selection table, not from the prior process layout.

# Replace Morphism And Kernel Equivalence

- `copy.replace` (3.13+) is the polymorphic morphism entry; family-local aliases (`dataclasses.replace`, `structs.replace`, `model_copy`) are equivalent only when the owner-selection row names that kernel — cross-family morphism application is a binding defect, not a convenience.
- `__replace__` synthesis on frozen dataclasses, `NamedTuple`, pydantic frozen models, and msgspec structs participates in the same morphism class; direct `__replace__` at application sites is rejected when `copy.replace` applies — variance opt-out via `__replace__ = None` removes the morphism entirely for read-only generic boxes.
- `frozendict` is outside the replace morphism class — mapping transitions are union morphisms `(|)` or explicit folds into a new `frozendict`; embedding replace-shaped call sites on mappings is rejected.
- `init=False` dataclass fields fall outside synthesized replace morphisms — owning transition methods must close init-only slots by computing derived state and returning a fully constructed successor; bare replace missing init-only fields is a contract failure, not a runtime surprise.
- Replace morphisms invoke `__post_init__` on dataclass and msgspec arms when defined — post-init is part of the morphism, not first-construction-only; pydantic shallow replace morphisms forward to `model_copy` without constraint replay unless Tier V reroutes.

# Nested Payload Promotion Ladder

- Nested mutable ingress residue promotes upward through a closed ladder — boundary snapshot, frozen child owner, persistent collection combinator, or Tier D deep isolation — not through `deepcopy` plus field assignment inside domain folds.
- Shallow shell with mutable nested `dict`/`list`/`set` is pre-promotion evidence only; durable domain owners never retain nested mutable slots updated through item assignment, `.update`, or `.append` on extracted views.
- Promotion at parent replace time routes through nested morphism on the child before outer closure: `copy.replace(outer, inner=copy.replace(outer.inner, field=value))` — partial inner mutation before outer assembly is rejected.
- msgspec `gc=False` graphs promote nested wire subtrees to frozen child structs at ingress rather than embedding mutable dict views — `deepcopy` staging inside acyclic graphs is rejected when `convert` or child `model_validate` can materialize the child owner.
- Tuple and `Block` fields promote ordered evidence by successor construction — slice-concat, `cons`, or `append` — never by mutating a list extracted from the prior owner followed by re-wrapping.

# Composition Root Sharing Posture

- Composition roots declare default sharing band per published owner family — bounded policy tables default value-copy; large handler registries default path-copy `Map`; receipt streams default path-copy `Block` — leaf modules inherit band choice from root catalog rows, not local convenience.
- Root warm-up folds materialize publication tables once — repeated single-key `frozendict` union inside root import loops is a band violation when the catalog row names `map_combinator`; finalize `Map` or `frozendict` before worker threads import root symbols.
- Root egress projection reads terminal enriched identity without mutating fields to close encoding gaps — structural sharing on the canonical owner ends at the boundary projection morphism; wire bytes never alias domain field objects.
- Encoder `order="deterministic"` at root pairs with replacement-produced version increments — logical succession changes encoded bytes; predecessor bytes are not reused across successors when persistence keys on wire shape.
- Root replace-routing catalogs document `kernel` and implicit sharing band together — a row naming `frozendict_union` implies value-copy band; `map_combinator` and `block_combinator` imply path-copy band; mismatched kernel/band pairing is a routing defect.

# Certificate Band And Kernel Pairing

- Compose-chain **HopRecord** spine carries `band: Literal["value_copy", "path_copy", "intentional_alias"]` per hop — closed vocabulary aligned with sharing band taxonomy; `PathBindingDigest` repeats `band` per binding when batch or nested-path certificates split focus across collection and scalar spines.
- Catalog row `kernel` implies default `band`: `frozendict_union` → `value_copy`; `map_combinator` / `block_combinator` → `path_copy`; `copy_replace` / `model_copy` / `structs_replace` on scalar-only deltas → `intentional_alias` unless `path_id` focuses a collection field documented as `path_copy` on the same hop.
- Verification fold rejects kernel/band mismatch against root `TransitionRow` — `map_combinator` hop recording `value_copy`, or `frozendict_union` inside a hot-fold loop recording `path_copy` without band-migration row, is a compose defect witnessed before behavioral suites.
- Golden certificate exemplars bind `golden_chain_id` to expected `band` per hop — assurance invariant R3 (sharing band publication) pairs PEP 779 worker-import timing with band-finalized `frozendict`/`Map`/`Block` tables; certificate emit aborts when child mutable residue blocks collection-band hop recording.
- OTEL live-hop budget exports `band` as low-cardinality closed literal (≤32 distinct values per key) — unbounded handler strings normalize to hash prefix before export; `tier` and `kernel` pair with `band` on the same budget row.
- Mixed-band nested path: one `HopRecord` may document `intentional_alias` on scalar spine segments and `path_copy` on collection `path_id` focus — `intermediate_observation=false`; two sequential bare replaces on the same owner without path lens closure is negative-control only, not production certificate content.

# Functional Update Algebra

- Domain transitions are endomorphisms on frozen owner types — `Owner → Owner`, `Owner → Result[Owner, E]`, or closed union member replacement — not partial mutations on erased carriers.
- Collection updates inside owners are functorial lifts: parent transition equals replace on the parent with combinator-applied child — `copy.replace(owner, index=owner.index.add(k, v))` — not mutate-extract-replace.
- Multi-step pipelines thread successors as fold seeds — `pipe(prior, result.map(lambda o: copy.replace(o, phase=next)))` — shared outer mutable closure accumulators defeat functorial threading and break publication law under concurrency.
- Normalized policy composition is a union fold morphism, not a builder dict — `BASE | sentinel | measured | overlay` with right-hand override; commutative equality, non-commutative override semantics are pinned contract, not incidental.
- Patch application is morphism composition at root — `validate(patch) ∘ merge(snapshot, delta) ∘ materialize` — staging `TypedDict` payloads never compose directly with interior replace endomorphisms.
