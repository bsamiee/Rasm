# Composition Epoch And Proof Calculus

# Transition Composition Monoid On One Owner Family

- Within one admitted base family on one owner symbol, enrichment transitions form a partial monoid: identity is the materialized canonical instance; composition is `g ∘ f` meaning apply `f` then `g`, each step returning `Self` or halting on `Fault`/`ValidationError` without silent coercion — tier labels (S/D/V) and replace kernels inherit immutable replacement algebra and contract-row columns.
- Tier S (`copy.replace`, `structs.replace`) composition preserves shallow sharing on unchanged nested slots — `h ∘ g ∘ f` must not deep-copy unchanged children unless an intermediate step documents `deep=True` or child snapshot `model_validate`.
- Tier V (`model_validate` on full snapshot) composition is not associative with Tier S steps on the same axis — inserting `model_copy(update=...)` between two Tier V transitions breaks validator replay law; composition tables mark illegal `(S, V, S)` sandwiches on pydantic settings owners.
- Nested sub-owner override composition requires child snapshot merge before parent fabrication at every link — `(parent ∘ child_delta_a) ∘ child_delta_b` re-merges explicit child snapshots; unfiltered spreads do not associate because omission versus `None` clearing is non-commutative.
- Param `bound` composition is not defined across verbs — surplus law is per invocation; registry morphism is the sole compose site for `bound` outputs, not chained `bound` on the same tokens.
- Fabrication `store` is a terminal morphism `Settings → Service` — not composable with further settings transitions on the same binding without rebinding local settings name; `store ∘ with_configuration` requires fresh settings reference before fabrication, not reuse of pre-transition alias — validate → project → construct ordering inherits immutable replacement fabrication law.
- Failure short-circuit: any `Fault` or `ValidationError` in a compose chain aborts subsequent owner transitions on that binding — no auto-recovery wrapper on the owner; composition root owns retry policy, not the rich class.
- Metamorphic proof for compose chains uses named links with predecessor `is not` successor at each Tier V link and shared-nested `is` only where contract row declares intentional-alias band — chains that mutate intermediate owners in place fail compose modules regardless of final field equality; certificate spine bytes and hop witnesses cite compose-chain certificate struct — equilibrium classification and federation extensions cite lock-order telemetry and federation equilibrium doctrines.

# Epoch Boundary Calculus

- Four enrichment epochs partition owner operations: **Construction** (env sources, `settings_customise_sources`, pydantic validators, msgspec decode/`__post_init__`), **Enrichment** (`with_configuration`, param `bound`, context projections on materialized settings), **Egress** (wire encode, CLI cyclopts slice, OTEL/subprocess env export), **Worker** (cross-process snapshot transmit, local fabrication rebuild) — Construction begins after model-materialization pipeline canonical exit; phantom stage literals on owner generics make Construction→Egress static defect when `ty`/`mypy` bind stage parameters per ingress-to-materialization enrichment lifecycle encoding.
- Construction → Enrichment gate: no env re-read, no `settings_customise_sources`, no nested `nested_model_default_partial_update` semantics inside transition methods — violation is epoch leak, not a new owner class.
- Enrichment → Egress gate: only canonical stored fields and contract-authorized `@computed_field` exports cross — method products (`ArtifactStore`, port handles), interior `@property` folds, and stage-mismatched generics are seam-local; egress adapters project from snapshots, not live method invocation.
- Enrichment → Worker gate: transmit frozen settings snapshots or msgspec-encoded slices — never port-typed fields, never parent `ArtifactStore` handles, never `ContextVar` tokens without explicit restoration proof in worker entry.
- Worker → Enrichment gate: worker rebuilds through fabrication methods on received snapshot — worker-local stores do not flow backward into parent enrichment bindings without a new validated ingress event.
- Construction → Egress gate: direct wire egress from pre-canonical builders is rejected — cyclopts and ingress pydantic materialize before egress structs; phantom stage parameters make Construction→Egress static defect when `ty`/`mypy` bind stage literals.
- `ContextVar` override lanes live inside Enrichment epoch only — parallel tasks may diverge inputs; overrides never substitute for Tier V transitions on the authoritative settings owner and never publish to Egress without snapshot freeze.
- Epoch leak tests are negative fixtures in contract rows — positive law matrices stay per-epoch; cross-epoch chain tests belong in metamorphic compose modules named by epoch pair (`construction_to_enrichment`, `enrichment_to_worker`).

# Seam Functor Laws At Family Boundaries

- Ingress functor `F_ingress: Wire → Canonical` maps foreign dict/bytes through pydantic `model_validate` or msgspec `convert` with explicit forbid/alias policy — `F_ingress` must not call enrichment method families.
- Egress functor `F_egress: Canonical → Wire` maps through `msgspec.convert`, `model_dump` slices with `exclude_computed_fields` policy, or adapter-projected structs — `F_egress` must not mutate canonical identity.
- Identity law: `F_egress ∘ F_ingress ≈ id` on lawful snapshots only where contract row declares round-trip tier — interior `@property` folds and non-exported computed fields are intentionally non-id; round-trip proof is scoped, not universal.
- Composition law: multi-hop egress (`Canonical → ReportStruct → JSON bytes`) associates at the adapter composition root — domain modules publish one canonical owner, not per-hop DTO siblings.
- Natural transformation across families: pydantic `model_validate` at seam followed by msgspec `convert` is explicit `η: Pydantic → Msgspec` — no implicit `.model_dump()` without re-validation policy row; η failures surface as `ValidationError`/`msgspec.ValidationError` at the seam, not inside owner interiors.
- Seam failures do not auto-morph into param `Fault` or rail tagged unions — boundary kind stays boundary kind; composition root morphism tables map seam failures to rail faults when policy requires, not owner methods.
- Cyclopts slice functor `F_cli: Canonical → ParamOwner` materializes frozen dataclass fields only — surplus routes to param `bound`, not to settings `with_configuration`; `F_cli` ∘ `F_ingress` rejected when param promotion skipped seam `model_validate` row.

# Morphism Composition And Envelope Accumulation

- Param `bound` output morphism `μ: Self | Fault → RailFault` is total at composition root — `μ` composes with registry `_bound` handlers; param owners do not nest `μ` inside `bound`.
- `μ` composition associativity: `μ₂ ∘ μ₁` only when two envelope families intentionally collapse at root — default is single `μ` per parse boundary; double morphism without row authorization is envelope duplication defect.
- `TypeIs` witnesses compose: `narrow ∘ μ` before rail `match` — bool guards and `cast` break associativity with static proof and are rejected.
- Settings `ValidationError` at construction does not compose into `Result` rails — epoch separation forbids mixing construction and enrichment envelopes on the same transition binding.
- Fault accumulation on surplus parse: first fault wins at `bound` — no accumulation list on param owner; multi-error aggregation belongs in pydantic `ValidationError` construction epoch only.
- Service fabrication morphism `φ: Settings → Service` is zero-inverse — no `φ⁻¹` on owner; settings transitions after `φ` require new local binding, not mutation of service interior from settings methods.

# Proof Obligation Bundles By Owner Archetype

- **Necessary bundle** (blocks merge if absent): one contract row per owner symbol; static introspection twin (`getmembers_static`) before live walks; Tier V negative control versus `model_copy`; param `bound` zero-exception cyclopts sample; enum `assert_never` witness on closed `match`.
- **Sufficient bundle** (discharges rich-owner promotion quorum): necessary bundle plus metamorphic chain covering epoch pairs the owner participates in; morphism bijection table with stable ids; mutation kill on `_bound` and `_arity` hooks when param polymorphism is load-bearing.
- Settings archetype adds: nested child-snapshot override samples, `Unpack[ConfigurationDelta]` callable-seam matrix, computed-export round-trip only when contract row declares export tier.
- Param archetype adds: verb × token-count arity table from vocabulary, surplus cap golden fixtures, registry morphism totality rows.
- Msgspec catalog archetype adds: `cache_hash` stability post-`structs.replace`, decode round-trip on typed enum axes, ban `deepcopy` when `__post_init__` load-bearing.
- Service archetype adds: port symbol conformance rows, negative `isinstance` on partial doubles, fabrication ordering validate → project → construct.
- Enum archetype adds: alias registration rows, `EnumCheck` verify flags, factory totality over declared scalar bands.
- Promotion without necessary bundle is absorption defect even when algorithm family exists — behavior density does not waive proof minima; promotion quorum and absorption targets cite owner absorption decision lattice rows.

# Behavioral Law Matrices And Metamorphic Compose Chains

- Behavioral law registers in parametrized matrices covering roundtrip snapshots and projection outputs (`artifact_backend_target_dispatch`, `assay_settings_with_configuration_roundtrip`) — names reference the transition or fabrication method, not the module.
- Tier V settings transitions prove successor `is not` predecessor, field-delta equality on changed axes only, and full validator replay via `model_validate` — shallow `model_copy(update=...)` on the same delta is a negative control; nested sub-owner override samples assemble explicit child snapshots before parent fabrication.
- Param `bound` samples cover unbounded base arity, subclass caps, and surplus fault arms — each row asserts product kind (`Self` versus `Fault`), clipped diagnostic length, and zero raised exceptions at the cyclopts boundary.
- Fabrication samples assert validate → project → construct ordering — skipping validate-before-project or passing unvalidated `**opts` when `FabricationOpts` is declared fails the sample table.
- Projection samples inject minimal protocol doubles from `get_protocol_members(Port)` — projection law runs on doubles, integration law runs on production context types; erased mocks and full settings fixtures are separate rows.
- Enum factory samples prove total mapping over declared scalar bands, alias resolution via `_add_value_alias_`, and reject out-of-band inputs with typed fault or default-arm policy documented in the row — silent coercion without row authorization fails CI.
- msgspec struct replace samples prove `__post_init__` replay on `copy.replace` and ban `copy.deepcopy` when post-init is load-bearing — deepcopy-as-clone fixtures are rejected, not proof paths.
- Metamorphic chains prove materialized settings → `with_configuration` delta → `store` fabrication → `ArtifactStore.open` — each link asserts immutable predecessor identity while successor bindings update and intermediate owners are not mutated in place.
- Param → `bound` → registry morphism → rail fault or success with total envelope map at the composition root; enum algebra fold totality — `None` collapse from fold is harness failure; worker snapshot → fabrication rebuild without parent port handles.
- Computed-export → subprocess env → OTEL only when contract row declares export tier — interior `@property` folds excluded unless promoted to `@computed_field` in the row; replaying chain segments out of order must fail metamorphic modules.
- Morphism tables are bijection-checked with stable row ids — CI diff fails on orphan ids; `TypeIs` narrowing witnesses before rail `match` omit bool-return guards so lint and behavioral suites share one gate.
- Settings snapshots and param `Self` occupy separate local bindings after morphism — combined product types or cross-field embedding fail metamorphic replay; golden hint fixtures store clipped bytes at param construction — morphism output asserts `fault.hint == golden` without secondary truncation unless a downstream cap row declares a stricter fixture column.
- Mutation testing targets registry `_bound` polymorphism — mutants removing `Fault` arms, param `Self` branches, or morphism defaults must fail contract tests or exhaustiveness type-check, not pass with silent wrong routing.
- Mutation on param `_arity` hooks must kill tests distinguishing subclass caps — default-hook mutants that always return `None` when a subclass expects bounded arity are required failures; Tier V mutants replacing `model_validate` with bare `model_copy` must fail negative controls.
- Mutants introducing `# type: ignore`, `cast`, or `TypeGuard` on rich owner transition signatures fail Ruff policy before mutation scoring — proof debt stays on language axis; owner source remains spec-complete.
- Adding enum members without updating every `match` consumer, contract row, and `assert_never` witness fails static and behavioral gates together — enum promotion is a multi-surface change set, not a single-file edit.
- `sys.monitoring` may witness transition method entry — enrichment law remains value-based; monitoring is diagnostic, not a substitute for typed transition products or certificate spine witnesses per rich class owner mechanics.
- Trusted replay pins settings module identity and schema hash beside owner symbols — replay materialization uses the same transition methods as live enrichment, not shortcut constructors.

# Hypothesis Compose Strategy Law

- Strategies draw from contract table rows — `st.sampled_from` over lawful `(prior, delta, tier)` exemplars per owner; compose draws extend to `(prior, delta_sequence, k, band, epoch)` tuples from registered exemplars beside owner import.
- Param surplus strategies generate verb tokens and path tuples bounded by subclass `_arity` rows — surplus lengths beyond cap must produce `Fault`, not arbitrary exceptions.
- Settings delta strategies build `TypedDict` literals from closed key sets — forbidden keys and wire-alias keys are negative-only strategies, not shrink endpoints.
- Enum strategies condition member payloads on declared arms — independent random strings validated into enum membership waste cycles and miss lawful shrink paths.
- Composite strategies mirror owner `match` structure — outer discriminant (fault versus success, protocol branch, enum kind) conditions inner field generation; outer epoch conditions inner operation family.
- `@given` targets register one law per property — successor identity, nested isolation, validator replay, morphism totality, and fold totality remain separate targets per owner family.
- Negative-only strategies include forbidden `(S, V, S)` sandwiches, port IO from Construction validators, and wire-alias delta keys — shrink endpoints must not converge on illegal tuples.
- Band-unaware `st.from_type` on settings owners remains rejected — k-step compose properties require table-driven builders matching extended compose columns.

# Nominal Versus Structural Proof Grade Selection

- **Structural grade** (`Protocol` slice + `TypeIs` + `get_protocol_members` doubles): context-bound projections consuming strict subsets of settings axes; cross-module imports that must not depend on concrete settings class; law matrices on doubles, integration on production context type.
- **Nominal grade** (`@disjoint_base` + sealed member classes + `assert_never`): closed variant sets with distinct per-arm method families; wire tagged unions decoded to distinct runtime classes; registry `match` arms keyed on routed param products, not strings alone.
- **Tagged generic grade** (`Shape[T: Tag]` PEP 695): field-only discriminant differences without distinct method sets per arm — exhaustiveness via type parameter binding, not `isinstance` ladders.
- Grade downgrade (nominal → string routing table) and grade upgrade (structural slice → full settings class dependency) both require contract-row migration and consumer `match` updates — grade is not a local file decision.
- When structural and nominal both apply (context slice of a sealed family), structural gates projection parameters, nominal gates variant arms — mixing grades on one method signature without row documentation fails static readiness modules.

# Generic Variance, Stage Narrowing, And Checker Debt Scoring

- Phantom stage literals on owner generics narrow return types on enrichment transitions — `with_configuration: Self@canonical → Self@enriched` is the canonical pattern; stage regression in compose chains is static defect when stage parameters are bound.
- PEP 695 type parameters on rich owners use declaration-site variance defaults — covariant read-only carriers set `__replace__ = None` when synthesized replace defeats read-only boxes; transition methods pin `Self` or closed unions at public seam.
- `Unpack[ConfigurationDelta]` on transition signatures requires dedicated callable-seam matrix — ParamSpec erasure through decorator stacks on root-wrapped handlers scores as **blocking debt** until matrix passes, not as advisory lint.
- `Self` narrowing disagreement between `mypy` and `ty` scores **tracked debt** when source is spec-complete and both run in CI — suppressions on transition signatures score **blocking debt**.
- `@runtime_checkable` re-decoration on every protocol subclass scores **blocking debt** on 3.20-readiness rows when missing — deprecation warning in 3.15 is early signal, not waiver.
- `annotationlib.get_annotations(..., Format.VALUE)` versus `__annotations__` mismatch on cyclopts/pydantic joint consumers scores **blocking debt** on owners exported to decorators.
- Debt score gates merge: any blocking debt fails; tracked debt requires linked issue id in contract row; waived debt is forbidden on Tier V and morphism surfaces.

# Schema Evolution And Replay Succession Keys

- Materialized settings owners carry explicit version or schema-hash field when validators add/remove/rename fields across releases — replay pins module identity plus hash beside owner symbol in trusted materialization fixtures.
- Successor-key law: enrichment transitions that do not alter version axis preserve version field; ingress events that bump schema hash invalidate identity caches keyed on value equality without version — contract rows document version participation in equality.
- Enum and catalog promotions are multi-surface evolution events — every new member updates `match` consumers, morphism tables, contract rows, and `assert_never` witnesses in one changeset; partial enum edits are evolution defects.
- Worker snapshots include version/hash slice — worker fabrication rejects stale snapshots when hash row mismatches without re-ingress path.

# Import Graph, Acyclic Handoff, And Registry Compose Order

- Rich owner modules remain import-acyclic — registry rows reference owner types as forward declarations resolved at composition root; owner modules do not import rail arms that import those owners back.
- Handoff exports at seams are immutable snapshots or fold-derived receipts — live mutable buffers, shared service handles, and `ContextVar` tokens are not re-published as canonical state from owner modules.
- Registry compose order: materialize settings → bind param `bound` → morphism `μ` → `_bound` handler `match` — reordering morphism before settings materialization is compose defect, not a registry feature.
- Lazy import on consuming rail modules is permitted only when importing already-materialized owner types — runtime-evaluated base modules must export field types unconditionally; import-gated symbols break decorator admission.
- Cross-owner enrichment never closes over mutable class attributes on another owner — `classmethod` memo dicts and registry caches publish at root as `frozendict` or guarded locks before parallel tasks consume them.

# Diagnostic Egress And Observability Tiering

- Observability exports partition into three tiers aligned with projection doctrine but scoped to telemetry law: **stored-field tags** (persisted axes dumped to OTEL/subprocess), **computed-export tags** (`@computed_field` authorized in contract row), **interior-only signals** (`@property` folds never exported without row promotion).
- Enrichment epoch may read interior tiers for subprocess assembly; Egress epoch may emit only stored-field and computed-export tiers — promoting interior fold to exported tag requires contract-row amendment and metamorphic OTEL round-trip sample.
- `agent_context`, `run_id` scrubbing, and correlation envelopes flow settings → logging/wire through adapter projection — rail modules consume dumps, not re-derive tags by re-invoking context projections.
- `sys.monitoring` witnesses on transition entry are diagnostic overlays — they do not substitute for typed transition products, epoch gates, or proof bundles; monitoring hooks attach at composition root on named transition symbols only.
- Wire round-trip proof for computed-export tags runs at root guards when subprocess or OTEL depends on them — interior `@property` folds skip round-trip obligations by row declaration.

# Compose Anti-Patterns And Negative Controls

- Chaining `model_copy(update=...)` steps to simulate `with_configuration` — associativity violation; negative control must fail Tier V compose modules.
- Calling `store` on stale settings alias after `with_configuration` rebound elsewhere — terminal morphism composed with wrong predecessor binding; metamorphic chain must assert fresh reference.
- Nesting `μ` inside param `bound` or handler arms — envelope duplication; static import graph must show single root morphism export.
- Egress adapter invoking `target(settings)` live during encode — epoch leak from Enrichment into Egress; encode paths use pre-projected snapshots or stored fields only.
- Worker inheriting parent `ArtifactStore` or port handle — Worker epoch leak; negative fixture in cross-process modules.
- `getmembers` catalog walk before static twin exists — introspection defect; CI blocks on live property execution during collection.
- Proving compose associativity by final field equality alone — insufficient when intermediate in-place mutation occurred; predecessor `is not` successor required at each link.
- JSON contract tables parallel to Python owners — dual proof source; reader-emitted tables from live annotations remain the only authoritative row generator.
- Proof green on shallow replace alone while Tier V transition methods lack negative controls — tier separation becomes unenforceable.
- Skipping morphism totality because handlers manually wrap faults today — envelope families diverge silently as arms accrete.
- Mutation testing only pydantic validators while registry `_bound` routing stays unmutated — param polymorphism remains unproven load-bearing infrastructure.

# Checker Orchestration And Multi-Backend Convergence

- Pydantic plugin synthesis (`__replace__`, `@computed_field`, `@override` on hooks), `ty` all-error posture, and `mypy` `explicit-override`/`exhaustive-match` run on the same owner family — one green backend does not relax declarations required by others.
- Ruff `runtime-evaluated-base-classes` and `runtime-evaluated-decorators` gate unconditional field imports and PEP 695 `**P` preservation under decorator stacks — compose proofs assume these gates pass before mutation testing.
- Beartype on root handlers accepting materialized owners catches post-promotion field tampering on frozen instances — compose chains that rebind names must not reuse instances that underwent illegal attribute writes in tests.
- Hypothesis strategies draw from contract rows, not `st.from_type` on settings owners — compose property tests build lawful epoch transitions per Hypothesis Compose Strategy Law section; arbitrary dict patches are negative strategy design only.
- Cross-checker modules maintain parallel law matrices until `Self`/`Unpack`/stage narrowing converges — tracked debt rows link issue ids; blocking debt on Tier V surfaces forbids merge.

# Delta Algebra And Enrichment Closure

- Closed `TypedDict` deltas with `closed=True` and `NotRequired` keys form a partial commutative monoid only when `None` clearing and key omission are row-defined — `merge(delta_a, delta_b)` in tests mirrors `with_configuration(**(a | b))` legality, not arbitrary dict union.
- Wire alias keys never appear in delta spreads — alias normalization stays in Construction epoch; Enrichment epoch consumes domain vocabulary keys exclusively.
- Param-owner deltas stay `copy.replace` with field-name keys — cyclopts surplus never enters delta TypedDicts; filtering `None` at method boundary preserves omission semantics across compose.
- Multi-axis co-varying transitions promote `Unpack[ConfigurationDelta]` — loose `**overrides: object` rejects compose typing evidence; surplus kwargs must fail static checking, not silently drop.
- Enrichment closure: the set of operations reachable from canonical materialization without epoch leak is exactly `{with_configuration, nested child override, context projection, param bound, fabrication}` — any new operation name requires promotion quorum plus compose-row extension.

# Refinement Stack Without Shape Proliferation

- `Annotated[..., BeforeValidator]` and `msgspec.Meta(...)` stack on field types, not parallel sibling classes — single-scalar coercion never promotes a rich owner when validator absorption suffices.
- `NewType` plus parent validator owns opaque scalar gates — compose law treats `NewType` as transparent at seam functors when ingress row declares unwrap policy.
- Nested frozen sub-owners refine parent axes without `{Parent}{Child}` DTO pairs — child snapshot merge in parent transition is the sole compose path for nested refinement.
- `frozendict` allowlist fields refine policy tables at materialization — enrichment rebuild publishes new `frozendict` instances; compose chains that alias old allowlist shells fail isolation samples.
- Refinement promotion triggers concept-density collapse before new owners — three single-caller validators on one axis absorb into one `field_validator` ladder or `model_validator(mode="after")` per owner absorption decision lattice.

# Fabrication Compose And Service Lifecycle

- Fabrication sub-owners (`FabricationOpts`) are ephemeral validation capsules — they do not compose with settings transitions; `validate(opts) → project → construct` ordering is strict at every `store` invocation.
- Exclusive-resource law proves at sub-owner `model_validator(mode="after")` in Construction epoch — fabrication in Enrichment epoch assumes legality; compose must not skip construction proof because fabrication succeeded once before.
- Repeated `store()` from identical frozen settings yields new service identities with equivalent logical roots — service caches belong on service or scope owner, not settings; compose tests assert distinct `ArtifactStore` ids across repeated fabrication.
- Service operations (`open`, `write_bytes`, `retain_history`) live in Worker and Enrichment epochs on service owner — settings owner does not proxy service methods; compose chains terminate at `φ(settings)`.
