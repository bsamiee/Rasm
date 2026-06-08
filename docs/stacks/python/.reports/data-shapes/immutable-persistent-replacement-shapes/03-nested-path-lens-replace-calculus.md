# Nested Path Lens Replace Calculus

# Path Segment Vocabulary

- A **path** is a non-empty tuple of segments `(s₀, s₁, …, sₙ₋₁)` where each segment is one of: **field** (Python field name on frozen owner), **key** (hashable key on `frozendict` / `Map`), **index** (non-negative int on `tuple` / `Block`), **arm** (closed union discriminant or tag literal), **slice** (half-open range on ordered collections when batch replace is load-bearing).
- Paths are value evidence — `tuple[str | int | slice, ...]` or a frozen `NamedTuple` row — not string dot-paths; `"outer.inner.field"` string parsers are rejected at composition root.
- **Root segment** names a field on the canonical owner; **interior segments** traverse nested frozen owners, persistent collections, or tagged union arms until the **focus** segment where the delta applies.
- **Focus** is the terminal segment receiving the replacement value or endomorphism; **spine** is the prefix `(s₀, …, sₙ₋₂)` whose nodes must survive sharing band law on unchanged branches.
- Empty path is identity morphism — `focus_at(())` is the owner itself; zero-hop Tier S field swap on root fields uses direct replace, not lens machinery.
- Path equality is structural on segment kinds — `(field("items"), key("k"))` differs from `(field("items_by_key"),)` even when runtime lookup coincides; proof tables key on segment tuples, not resolved values.

# Lens Row Shape

- Every admitted multi-hop transition declares one frozen **lens row**: `path_id`, `owner_alias`, `path_segments`, `focus_kind`, `tier`, `kernel`, `band`, `intermediate_observation`, `negative_fixture_ids`.
- `path_id` is a stable snake slug — `queue_registry_add_key`, `envelope_detail_arm_swap` — not ordinal depth alone.
- `path_segments` is a `tuple[Segment, ...]` registered beside the owner import — runtime string dispatch tables reconstructed at handler sites fail catalog parity.
- `focus_kind` draws from `{scalar, owner, collection, union_arm}` — selects closure obligation at the terminal hop.
- `tier`, `kernel`, and `band` inherit per-hop replace vocabulary — tier (`S` | `D` | `V`), kernel (`copy_replace`, `model_copy`, `structs_replace`, `frozendict_union`, `map_combinator`, `block_combinator`, `model_validate`), band (`value_copy`, `path_copy`, `intentional_alias`); lens row documents **strictest** tier across hops when tiers differ — root guard classifies before interior path fold begins.
- `intermediate_observation` is boolean — `true` only when a rail intentionally materializes owners at spine nodes; default `false` folds to one outer expression.
- `negative_fixture_ids` index harness anti-patterns — `mutate_spine_extract`, `alias_key_string_path`, `tier_s_on_wire_path`.

# Focused Update Endomorphism

- A **path lens** is a partial endomorphism `L: Owner → Owner | Result[Owner, E]` defined only when `path_segments` is valid on the predecessor — invalid segment kind or missing key is typed failure, not silent no-op.
- Single-hop lens on root field equals admitted replace kernel — `L(o) = copy.replace(o, field=v)` — lens machinery generalizes depth, not semantics at depth zero.
- Multi-hop lens expands as nested replace from focus outward — `L(o) = copy.replace(o, s₀=l₀(o))` where `l₀` is the lens on `path_segments[1:]` applied to `getattr(o, s₀)` — one outer expression when `intermediate_observation=false`.
- Collection focus uses combinator morphisms — `L(o) = copy.replace(o, index=o.index.add(k, v))` for `Map`; `copy.replace(o, items=o.items.append(x))` for `Block`; `base | frozendict({k: v})` when the owner field is `frozendict` and path ends at key overlay.
- Tuple index focus rebuilds via slice-concat on parent replace — no `copy.replace` on bare `tuple`; parent carries `tags[:i] + (new,) + tags[i + 1:]`.
- Fallible focus endomorphisms return `Result` at the owning transition method — lens composition short-circuits on first `Error`; spine nodes remain predecessor identities on failure paths.

# Lens Composition Laws

- **Identity**: empty path lens is `id`; `compose(id, L) = L`.
- **Associativity (observation-free)**: when `intermediate_observation=false` on both operands, `compose(L₂, L₁)` equals one registered path whose segments concatenate — `(p₁ ++ p₂)` — implemented as single nested replace expression, not two sequential bare replaces on the same root owner.
- **Non-associativity (observation load-bearing)**: when a rail observes spine owner after first hop, composition registers as two lens rows — merging into one row without `intermediate_observation=true` is a catalog defect.
- **Focus uniqueness**: two lens rows with identical `path_segments` and different `tier` or `kernel` are merge blockers — tier is not inferred at the focus.
- **Spine sharing**: under Tier S and `intentional_alias` band, unchanged spine field slots alias across predecessor and successor; `path_copy` collection spines retain subtree aliasing on `Map`/`Block` combinators when each hop uses admitted kernels.
- **Version threading**: when logical version is load-bearing, lens composition increments version once at the outermost closure — not per hop — unless a hop row documents intermediate version observation.

# Tier Routing On Path Chains

- Wire-sourced or computed delta at any focus hop routes **Tier V** for the entire lens closure — shallow replace on an interior pydantic child reached through a Tier S root hop is a tier sandwich defect.
- Tier D required when any spine node holds nested mutable ingress residue — deep isolation promotes at the offending hop before outer replace closes; `model_copy(deep=True)` on subtree snapshot is hop-local, not deferred to root.
- Tier S permitted only when every hop traverses in-session validated frozen owners and the focus delta is trusted same-type swap — deep-immutable `json.loads` trees at an interior hop do not confer Tier S on descendants.
- Patch `TypedDict` deltas map to lens rows at root — `validate(patch) → lens_apply(snapshot, path_segments, delta)` → successor; patch keys never address interior mutable dicts directly.
- Replay materialize (`decode` / `model_validate` / `convert` under pinned replay policy) produces a fresh owner — path lenses apply only after replay identity exists; bare `copy.replace` on cached session subgraph without pinned replay policy remains rejected.

# Sharing Band Survival Across k Hops

- Each hop inherits collection band — `value_copy` for `frozendict` key overlay, `path_copy` for `Map`/`Block` combinators, `intentional_alias` for scalar owner fields on trusted payloads.
- k-step Tier S chains on distinct `path_copy` hops preserve spine subtree sharing when each hop uses admitted combinators — converting spine to mutable `dict`/`list` between hops breaks band law regardless of focus correctness.
- k-step chains mixing `value_copy` and `path_copy` hops document band per segment in the lens row — defaulting entire path to `intentional_alias` because root owner is a dataclass capsule is a band violation when a `Map` field sits on the spine.
- Quadratic `frozendict` union inside a loop implementing multi-key path overlay is rejected — batch key paths register as `Map` field promotion at owner replacement, not repeated lens apply in hot folds.

# Family-Specific Path Kernels

- **Frozen dataclass**: spine hops use `copy.replace` / `dataclasses.replace`; `init=False` fields require owning transition method closing derived slots at the hop where init-only state changes — bare multi-hop replace missing init-only closure fails contract tests.
- **Pydantic frozen model**: shallow child replace at interior hop skips constraint replay unless Tier V reroutes through `model_validate` on child snapshot; parent `model_copy(update={...})` with plain dict child slot is seam defect unless Tier S trust is documented for that field.
- **msgspec struct**: interior hops use `structs.replace` / `copy.replace`; `__post_init__` runs on each replaced struct hop when defined; `force_setattr` outside `__post_init__` is rejected on path repair.
- **Tagged union**: arm hop is explicit segment — `match` predecessor → arm-specific lens on member → successor member or cross-arm fold product; wire tag strings do not select replace kernel without adapter discriminant table.
- **frozendict field**: terminal key hop uses union morphism; spine is `value_copy`; embedding `copy.replace` on mapping fields is negative fixture.
- **Map / Block field**: terminal hop uses `add`, `remove`, `change`, `cons`, `append`; parent closure is `copy.replace(owner, field=combinator_result)`.

# Union Arm And Extension Path Focus

- Closed union path prefix ends with **arm** segment binding canonical discriminant — interior fields address the matched member type only.
- Cross-arm path transition registers two lens rows or one row with explicit migration morphism — hidden arm change inside a generic `field` hop is rejected.
- Semi-closed `Extension` arms carry foreign discriminants — path into extension overflow uses `frozendict` or bounded payload segments; extension payload is immutable at handoff.
- Replay materialize remaps discriminants at adapter before arm path lenses run — replay bytes do not flow into shallow arm replace on wrong member.

# Patch-To-Path Morphism At Root

- Staging patch payloads admit a root table `PATCH_TO_PATH: frozendict[PatchKey, PathSegmentSuffix]` — patch application is `validate(patch) ∘ path_lens(snapshot, mapped_segments, focus_value)`.
- Open patch bands with `extra_items` promote to `frozendict` snapshot at materialization — path lens targets canonical field paths, not raw extra key strings without mapping row.
- Partial nested patch on pydantic child routes through child `model_validate` when any constraint must replay — shallow parent lens with dict child slot is Tier leak unless documented Tier S field.
- Multi-patch batch at root composes as one lens row per independent path when paths do not share spine prefix — shared spine prefix merges to single outer closure with multi-focus replace when observation-free.

# Catalog And Harness Extension

- Root replace-routing catalog extends `TransitionRow` with optional `path_id` foreign key — every multi-hop production transition method has exactly one lens row or explicit single-hop exemption.
- Root publishes `LENS_CATALOG: tuple[LensRow, ...]` beside `TransitionRow` — catalog parity tests iterate owner transition methods, routing rows, and lens rows in one assertion; set difference in any direction fails CI before property suites.
- Hypothesis composites draw `(prior, path_segments, delta, tier, band)` from closed exemplars — not arbitrary nested dict surgery filtered by runtime validation.
- Property targets per law: spine alias under Tier S, subtree isolation under Tier D, validation replay under Tier V, band preservation across k hops, single-expression closure when observation-free.
- Mutation testing kills mutants that flatten nested replace to sequential same-owner replaces without catalog registration — same kill obligation as direct `__replace__` call-site policy.

# Snapshot And Replay Orthogonality

- Snapshot morphisms export read-only carriers readable at path addresses — snapshot is not inverse of path lens; `snapshot(lens(o, δ))` equals snapshot of terminal enriched owner after lens completes.
- Replay materialize binds fresh owner — path lenses run in consuming process on replay product; parent-process subgraph references do not shorten path segments.
- Succession version increment stays at outermost live-replace closure — path lens does not bump version per hop unless hop row documents observation.
- Receipt append on path transition uses `Block` combinator on owner field — not post-hoc list mutation after lens returns.

# Checker And Analyzer Alignment

- Static proof owns segment kind exhaustiveness — ty/mypy `match` on `Segment` union; stringly path builders fail lint when repo policy rows apply.
- Runtime proof owns hop witnesses — beartype on transition entrypoints checks owner types at root; path legality stays in contract tables.
- Import-linter flags interior modules constructing path segment tuples from wire keys — path registration belongs beside owner import at root or owner module.
- Ruff policy bans extracted-spine mutation between hops — same family as in-place nested dict assignment on frozen owners.

# Proof Obligations By Path Depth

- Depth zero: direct replace law — successor `is not` predecessor; tier and kernel match owner-selection row.
- Depth one: assert single outer expression when observation-free; spine field aliases under Tier S; child hop invokes `__post_init__` on dataclass and msgspec arms when defined.
- Depth two and above: assert segment tuple registered equals production path; intermediate spine nodes alias when band and tier permit; Tier D samples assert `successor.spine_node is not predecessor.spine_node` at the promoted hop only.
- Terminal collection hop: assert combinator product `is not` extracted mutable view; predecessor collection identity preserved on `path_copy` bands.
- Terminal union arm hop: assert discriminant segment present in row; cross-arm negative fixture fails before behavioral tests.
- Patch-to-path samples: assert patch key resolves through `PATCH_TO_PATH` — unmapped patch keys fail at root, not as silent no-op at focus.
- Shrinking on path composites rebuilds lawful segment tuples — invalid segment kind mutations reject at construction gate; tier downgrade during shrink is forbidden.

# Materialization Stage And Path Placement

- Path lenses execute in enrichment and interior domain stages on frozen owners — staging payloads and ingress carriers are pre-path mutable evidence only.
- Nested payload promotion at materialization stage six completes before first multi-hop path on that subtree — child mutable residue crossing into path apply without promotion is shallow-law breach.
- Egress projection reads terminal identity after path lens completes — encoding does not mutate spine fields discovered late during path apply.
- Trusted-replay materialize completes materialization stages one through six before path lenses run in the consuming process — replay is not a shortcut past hop-level promotion.
- Stage-skipping from path-updated owner directly to bytes without projection owner requires documented root exemption — leaf modules do not skip wire project because path replace succeeded.

# Hash, Key, And Ordered Path Defects

- Hashable `frozendict` key path segments require hashable focus values — nested mutable list in key row fails hash stability proof at path registration, not at apply time surprise.
- Order-sensitive evidence paths use `index` or `slice` segments on `tuple` / `Block` — key segment on ordered stream when sequence order is load-bearing is a segment-kind defect.
- `frozendict` equality is order-insensitive — two path rows differing only by key insertion order in evidence tables must prove same canonical focus when keys are equal as mappings.
- `Map` path with missing key on remove hop returns typed absence — `Option`-aware `map.change` or explicit remove row; silent no-op remove is rejected.

# init=False And Computed Field Path Closure

- Multi-hop path crossing `init=False` field boundary requires owning transition method — lens row names `transition` method, not bare synthesized replace across init-only gap.
- Pydantic `@computed_field` is not a path segment target — path stops at stored fields; derived values recompute on successor identity by construction.
- Manual cache slots on spine nodes invalidate across path replace when focus mutation changes cache inputs — prefer recompute-on-replace over undeclared `_cache_*` on spines path lenses traverse frequently.

# Exemplar Path Row Patterns

- **Scalar root swap**: `path_segments=(field("phase"),)`, `focus_kind=scalar`, `tier=S`, `kernel=copy_replace`, `band=intentional_alias` — single-hop exemption from multi-hop machinery when catalog still registers row for parity.
- **Nested child field**: `path_segments=(field("config"), field("timeout"))`, `focus_kind=scalar`, `tier=S`, `kernel=copy_replace`, `intermediate_observation=false` — one expression `copy.replace(outer, config=copy.replace(outer.config, timeout=v))`.
- **Map registry key**: `path_segments=(field("index"), key("handler_id"))`, `focus_kind=collection`, `tier=S`, `kernel=map_combinator`, `band=path_copy` — parent closure via `copy.replace(owner, index=owner.index.add(k, v))`.
- **Block receipt append**: `path_segments=(field("receipts"),)`, `focus_kind=collection`, `kernel=block_combinator`, `band=path_copy` — `copy.replace(owner, receipts=owner.receipts.append(r))`.
- **Union arm field**: `path_segments=(arm("success"), field("detail"))`, `focus_kind=owner`, `tier=V` when wire-sourced detail — arm segment precedes field segments; discriminant table at adapter.
- **Tier D nested promote**: `path_segments=(field("payload"), field("items"))`, `tier=D`, `kernel=model_validate` on child snapshot before parent close — isolation hop documented in row, not inferred from parent tier.

# Composition Root Path Guard

- Root ingress classifies tier before delegating to registered lens row — handlers do not construct ad hoc path tuples from wire keys at leaf sites.
- Root patch seam resolves patch keys through `PATCH_TO_PATH` then invokes lens apply on validated snapshot — interior modules receive successor owner, not raw patch dict plus path string.
- Root catalog publishes `LENS_CATALOG: tuple[LensRow, ...]` beside `TransitionRow` catalog — harness parametrization imports both; orphan path apply without row fails CI.
- Alias normalization on patch and wire inputs completes at root before segment resolution — foreign keys map to Python field names; segment tuples use canonical field names only.
