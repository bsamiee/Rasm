# System Coherence And Shape Federation

# Shape Kind Ontology

- Python `>=3.15` admits nine systemic shape kinds — vocabulary, absence, record, variant, collection, port, rail, wire, ingress — orthogonal to stack altitude; a **kind** names what the value *is*; a **layer** names where invariant proof completes; conflating kind and layer is a taxonomy defect.
- Kind registry is closed — no tenth kind without charter amendment; each kind carries `kind_id`, `proof_locus`, `interior_admissible`, and `federation_carry` columns on the ontology row beside the vocabulary owner.
- **Vocabulary kind** — `Literal`, `StrEnum`, `NewType`, `Annotated` scalar constraints; proof locus is closure and bijection; federates across contexts via vocabulary federation edge only.
- **Absence kind** — `Option[T]`, `sentinel()`, explicit variant absence arms; proof locus is collapse semantics at boundary; federates like scalar vocabulary — foreign null semantics remap at seam, never default to Python `None`.
- **Record kind** — frozen `dataclass` or `BaseModel` domain owner; proof locus is smart constructor and cross-field law; never federates as importable interior — only seam adapter outputs cross contexts.
- **Variant kind** — `@tagged_union`, discriminated union, msgspec `tag_field` family; proof locus is exhaustive `match` with `assert_never`; `@disjoint_base` (PEP 800) seals sibling arms; graduation from record kind is the sole lawful record→variant morphism.
- **Collection kind** — immutable `tuple`, `frozenset`, `frozendict`, `Block`, `Map`; proof locus is shallow immutability and nested kind closure; embeds only record, variant, vocabulary, or absence kinds — never port, rail, wire, or ingress as durable field types.
- **Port kind** — `Protocol` capability slice; proof locus is structural conformance at injection boundary; outputs canonical kinds or `Result` thereof — port return types are never stored as model fields.
- **Rail kind** — `Result[T, E]`, `Option[T]`, `@effect.result` carriers; proof locus is channel discipline and single collapse per crossing; interior binds only on record or variant payloads already at materialization exit.
- **Wire kind** — msgspec `Struct` egress and persistence envelope; proof locus is transport policy; boundary-external — domain modules reference only through adapter signatures.
- **Ingress kind** — Pydantic strict model or `TypeAdapter` target; proof locus is foreign-byte admission; boundary-external — never substitutes for record kind in interior transforms.
- Each kind occupies exactly one proof locus per concept per bounded context; kind duplication across contexts requires vocabulary federation, not kind redefinition or runtime `isinstance` repair.
- Kind membership is static after materialization exit; projected views inherit kind through explicit derivation morphisms, never through runtime tagging, `TypeGuard`, or `cast` on integration modules.
- Record and variant kinds never co-own the same concept without graduation morphism; optional-field collapse pretending to be variant kind is rejected at kind coherence matrix.

# Cross-Kind Morphism Laws

- Legal morphisms form a directed acyclic graph — not every kind pair admits direct transform; illegal edges require rematerialization through canonical record or variant kind.
- **Vocabulary → record** — field assignment after closure proof; no morphism from raw `str` without vocabulary kind admission first.
- **Record → variant** — graduation only; morphism is smart-constructor promotion or `@tagged_union` arm selection, not optional-field collapse.
- **Record → wire** — `project_*_wire` only; inverse requires decode plus `materialize_*`, never trusted `convert` on foreign bytes without trust-posture pin.
- **Ingress → record** — `materialize_*` adapter only; ingress kind never morphs directly to wire kind without canonical intermediate unless ingress and wire are policy-identical under documented lattice collapse.
- **Port → record** — forbidden; ports return canonical kinds or `Result` thereof — port outputs are not stored as model fields.
- **Rail → record** — `bind`/`map` on canonical interiors only; rail carriers wrapping ingress or wire kinds must rematerialize before interior bind chains.
- Morphism composition is associative only when intermediate kinds match — `ingress → wire → canonical` without record intermediate is a system defect unless documented one-hop collapse with metamorphic proof.
- Adding a morphism edge requires simultaneous contract-row, metamorphic round-trip, and role-map update — partial morphism promotion is a merge blocker.

# Kind Federation Registry

- Federated kind rows materialize beside the vocabulary owner as frozen `frozendict[KindId, KindOntologyRow]` — each row binds `kind_id`, `proof_locus`, `interior_admissible`, `federation_carry`, and `morphism_sources`.
- **Federation carry column** — `none` for record, variant, collection, port, rail kinds; `vocabulary_edge` for vocabulary and absence kinds; `receipt_edge` for fold-derived evidence slices; `boundary_only` for wire and ingress kinds.
- **Morphism sources column** — closed set of lawful predecessor kinds per row; illegal source kinds fail static assignability before behavioral tests.
- Context nodes import kind registry read-only at warm time — no context redeclares kind proof locus or carry column locally.
- Kind promotion is a federation event — adding a morphism source or changing `federation_carry` updates consensus row, seam remap tables where wire-visible, and kind coherence matrix in one promotion unit.
- Cross-context handoff preserves kind through the chain — vocabulary kind remains vocabulary kind after federation import; record kind crossing a seam exits as record kind in the receiving context only after `materialize_*` completes; wire and ingress kinds never appear on interior handoff signatures post-seam.
- Kind skew detection — same concept typed as record kind in exporting context and ingress kind in importing context interior without seam adapter is `kind_coherence` fault; remediation targets seam placement, not canonical owner thinning.

# Federated Context Topology

- A shape system is a directed acyclic graph of bounded contexts — nodes are contexts, edges are seam adapters or vocabulary federation links; cycles indicate import-law violation.
- Each context node owns one vertical stack, one projection lattice, and one fact-stream bus — federation never merges stacks into a shared interior module.
- **Vocabulary federation edge** — shared `StrEnum`, `NewType`, or `Literal` owner imported by two contexts; no domain record crosses this edge.
- **Seam edge** — anti-corruption `materialize_*` per foreign pair; carries `Result[Canonical, SeamError]`; restarts materialization pipeline at canonical handoff in the receiving context.
- **Receipt edge** — fold-derived immutable structs cross contexts when carrying evidence only; canonical state never duplicates inside receipt payloads.
- Context depth from composition root determines settings fan-out order — upstream contexts materialize before downstream contexts consume canonical handoffs.
- Parallel contexts at the same depth share no canonical types unless vocabulary federation explicitly unifies tokens — shared concepts get one owning context and adapter exports for consumers.

# Global Vocabulary Consensus

- Federated vocabulary lives in a vocabulary owner module per token family — not replicated in ingress models, wire structs, or domain enums across contexts.
- Consensus row declares canonical member, ingress discriminator literal, wire `tag_field` string, seam remap foreign tokens, and OpenAPI enum arm — one row drives all projection edges system-wide.
- Token promotion is a federation event — every context importing the vocabulary owner updates contract tables in the same change unit; orphan token in any projection edge fails CI bijection gate.
- Semi-closed extension variants (`Extension`, `Unknown` arms) register at vocabulary owner when foreign contexts require lossless foreign discriminant preservation — not ad hoc per-seam string buckets.
- Absence sentinel vocabulary federates like scalar tokens — `Option`, sentinel union members, and explicit variant cases share one closure declaration; seam remap maps foreign null semantics to federated absence kind, not to Python `None` by default.

# System Schema Warm Graph

- Composition root warms one module-level `TypeAdapter` singleton and one msgspec `Encoder`/`Decoder` pair per ingress/wire target before domain transforms execute — cold instantiation per request is a system defect.
- Warm graph nodes are ingress projections, wire structs, settings slices, and vocabulary `TypeAdapter` targets — edges are documented adapter functions, not import dependencies between projection modules.
- `pydantic.mypy` with `init_typed` and `init_forbid_extra` validates warm graph completeness at static layer before generative suites run.
- Settings materialization completes once at process boot — frozen settings instance fans to per-context boot configs through root `project_*_config` before any context stack activates.
- Schema rebuild after forward-ref resolution occurs at warm time — domain modules assume compiled schemas; runtime `model_rebuild()` outside boundary packages indicates warm-graph gap.
- Deferred annotations (PEP 649/749) resolve through `annotationlib.get_annotations(..., format=annotationlib.Format.VALUE)` in metadata-driven warm paths — direct `__annotations__` access misses deferred materialization at system bind time.

# Kind Coherence Matrix

- Coherence matrix declares which kind pairs may compose in one expression without rematerialization — matrix rows are enforced by import architecture and static assignability, not runtime checks.
- **Coherent** — record embeds record, variant, vocabulary, absence, collection kinds; variant arms embed record kinds flatly; collection kinds nest immutable aggregates.
- **Incoherent without adapter** — record with ingress field type; variant with wire arm type; rail bind on wire struct; port method accepting raw `dict[str, object]`.
- **Conditionally coherent** — phantom-staged record `Record[Stage: Literal["canonical"]]` accepts enrichment only from same stage or promoted stage literal — cross-stage composition without transition method is incoherent.
- **Federation-coherent** — vocabulary kind imported across contexts; record kinds never imported across context interiors — only seam adapter outputs cross context boundaries.
- Matrix violations surface as import-linter rules and static assignability failures before behavioral tests — attribution targets the composing module, not the canonical owner.

# Meta-Lattice Federation

- Each bounded context owns a projection lattice — federation links lattices at seam edges and vocabulary federation nodes, never by merging ingress or wire projections across contexts.
- Lattice sibling projections within one context derive from one canonical owner — federated lattices do not share ingress models; they share vocabulary rows and seam remap tables only.
- Cross-lattice derivation chain — context A canonical → context A wire → decode → context B seam adapter → context B canonical — each hop has one typed owner; skipping hops is federation defect.
- Receipt or evidence projections federate as read-only lattice siblings — fold-derived slices export across receipt edges; they never become canonical owners in consuming contexts.
- Meta-lattice drift signal — same semantic field name appears on ingress in context A and canonical in context B without seam remap documentation — collapse test targets federation edge, not local projection.

# System Equilibrium Invariants

- Six global invariants hold across all contexts simultaneously — violation in any context is a system defect even when local bounded-context proof passes.
- **Single owner** — every invariant has exactly one enforcing kind owner system-wide; duplicate validation across federation edges fails duplicate-validation checklist.
- **Acyclic federation** — context graph, morphism graph, and warm-graph import closure are DAGs — cycle detection runs at import architecture layer.
- **Vocabulary bijection** — federated token rows are bijective across declared closure — orphan or collision in any projection edge fails contract gate.
- **Kind preservation** — canonical kind never degrades to ingress or wire kind in interior transforms — wire-as-domain collapse test applies system-wide.
- **Rail collapse once** — `Result`/`Option` collapse occurs at most once per boundary crossing per concept — interior federation handoffs pass canonical kinds on rails already collapsed.
- **Version monotonicity** — write-path emits current version only; read-path migration folds are total — unknown stored tags fail closed system-wide.
- Equilibrium restoration order — consolidate federated vocabulary, collapse duplicate owner, repair seam adapter, thin secondary projection — never add federation bypass type.

# Cross-Context Morphism Chains

- System-level handoff is a typed morphism chain — not a shared mutable store, not a re-parsed dict bus, not a foreign domain import.
- Standard chain — `foreign_ingress → Result[ForeignCanonical] → seam_materialize → Result[LocalCanonical] → interior_transform → LocalCanonical → project_local_wire`.
- Chain restart rule — receiving context restarts at local canonical or boot-config handoff after seam crossing — never at ingress payload carriers even when byte layouts match.
- Parallel chain fan-out — one local canonical may project to multiple wire egress targets or multiple seam exports — each edge owns separate adapter; fan-out does not fork canonical identity.
- Chain correlation — composition root pins run id, schema version, and encoder identity at chain entry — trusted-replay applies only when all three match stored metadata.
- Async chains use `@effect.async_result` at seam and boundary adapters only — interior async transforms consume canonical kinds, not foreign projections.

# Python 3.15 Type Infrastructure Binder

- System-wide type infrastructure binds through PEP bundle — not per-module ad hoc generic syntax or checker escapes.
- **PEP 695** — type parameters, `**P` ParamSpec, and default type parameters on generic owners, protocols, and payload surfaces; legacy `TypeVar`/`Optional` imports are policy-banned system-wide.
- **PEP 728** — `TypedDict` openness declared explicitly with `closed=True` or `extra_items=T` on payload kind surfaces only — never on canonical record or variant kinds.
- **PEP 749/649** — deferred annotations via `__annotate__`; introspection at warm-graph and registry walk sites uses `annotationlib`, not raw `__annotations__`.
- **PEP 742** — `TypeIs` for complement-safe narrowing at structural admission and variant refinement — `TypeGuard` and `cast` banned on family and integration modules.
- **PEP 800** — `@disjoint_base` on variant family seals; slot-disjoint dataclass inference supplements but does not replace explicit seals when exhaustiveness is load-bearing.
- **PEP 814** — `frozendict` from builtins for federated policy tables, remap rows, and hashable registry keys — not third-party immutable dict shims.
- **PEP 692** — `Unpack[TypedDict]` on keyword ingress at adapter signatures; unpack into validation immediately inside adapter body — payload kind never crosses into domain modules.

# Observability Federation

- Trace context propagates across seam edges with exporting context, importing context, schema version, and materialization stage — not canonical field payloads in span attributes.
- Diagnostic vocabulary reuses federated `StrEnum` rows — ad hoc log strings for concepts already vocabulary-owned are system drift signals.
- Fact-stream append events in one context export slot/kind enum members on receipt edges — consuming contexts fold imported streams without re-parsing canonical interiors.
- System smoke asserts conflated attribution fails — ingress defect in context A must not surface at context B domain owner when seam adapter is the correct attribution target.
- Encoder determinism for cross-context cache keys pins `order="deterministic"` on module-level encoder singleton shared by all contexts in the process — metamorphic proof uses production encoder instance.

# Federation Evolution And Breaking Change

- Breaking federation change requires simultaneous update to vocabulary consensus row, all seam remap tables, all ingress discriminators, all wire tags, and seam metamorphic fixture corpus — partial federation promotion is merge blocker.
- Adding a context node requires declared edges to composition root, warm-graph registration, and federation acyclic import proof — orphan context without seam or vocabulary link is rejected.
- Removing a context node retires seam adapters with migration fold proving zero dependency in production handoff graphs — deprecated seams stay in migration modules with `assert_never` witnesses.
- Context split — one canonical owner becomes two contexts — requires new seam between split contexts and vocabulary row partition; no duplicate canonical owners post-split.
- Context merge — two canonical owners collapse to one — requires anti-corruption retirement and vocabulary row consolidation in one promotion unit.
- Phantom stage literal changes on federated generic owners require stage-map updates on every cross-context projection edge — static stage bounds and adapter rejection tests update system-wide.

# System Boot And Lifecycle Ordering

- Process boot executes in fixed order — settings warm → vocabulary consensus load → per-context warm graph → protocol scope bind → domain-ready signal — no context transforms before warm graph completes.
- Settings slice validates once; root `project_*_config` fans immutable boot configs to each context node before that node's stack activates — contexts never read sibling settings or raw environment.
- Protocol implementations bind at root into `Map[type[Port], object]` or explicit service records — domain `@effect.result` generators resolve ports after bind, never during warm phase.
- Lazy context activation is permitted only when composition root documents deferral policy — deferred context still completes warm graph before first handoff accepts canonical input.
- Shutdown and flush — fact-stream snapshots export on context teardown when receipt edges require durability — append-only streams never mutate retroactively during export.

# Trust Posture Federation

- Trust labels are per-edge, not per-process — semi-trusted service ingress in context A does not downgrade validation in context B when handoff passes canonical kinds.
- Re-entry of `json.loads` output, ORM dict views, or partial buffers anywhere in the federation restarts at validation — upstream trust label does not propagate across seam or receipt edges.
- Cross-context trusted-replay requires composition root to pin store key, schema version, and encoder identity for both exporting and importing contexts in the same pin set.
- msgspec `Decoder` on trusted-internal bytes is permitted only when encoder module identity matches warm-graph registration — foreign context bytes always restart at ingress validation.

# Federation Failure Attribution

- System fault payloads extend bounded-context attribution with federation discriminators — `seam`, `vocabulary`, `federation_import`, `warm_graph`, `kind_coherence` tag structured boundary faults.
- Seam remap failure carries foreign token and consensus row key — fault attributes to vocabulary owner when token is unmapped, to seam adapter when row exists but cardinality remap fails.
- Kind coherence violation faults name expected kind, received kind, and composing module — not the canonical owner unless owner emitted wrong kind at materialization exit.
- Cross-context duplicate-validation faults name both contexts and both enforcing surfaces — remediation collapses to single owner per duplicate-validation test, federation-wide.
- Warm-graph failure at deferred annotation resolution attributes to boundary package binding `annotationlib` path — domain modules never appear in warm-graph fault stack.
