# Derived Descriptor Implication Closure

- Basis stays six binary descriptors (`AUDIT_VISIBLE`, `PERSIST_HORIZON`, `RAIL_POSITION`, `TRISTATE_WIRE`, `CORRELATED_ABSENCE`, `REPLAY_STABLE`) — `SECURITY_SENSITIVE` and `LAZY_DEFERRED` are derived predicates, not eighth and ninth basis bits; partition proof remains six assignments per concept.
- `SECURITY_SENSITIVE(c) := AUDIT_VISIBLE(c) ∧ ¬RAIL_POSITION(c)` — auditable wire or variant absence that must not echo raw tokens in developer logs; egress projection row selects redacted `.name` or hashed digest policy.
- `LAZY_DEFERRED(c) := RAIL_POSITION(c) ∧ ¬PERSIST_HORIZON(c)` — computed absence resolved only after I/O boundary; interior folds may not branch on lazy absence before rail exit consumer runs.
- `SECURITY_SENSITIVE → VARIANT_FORK` when `CORRELATED_ABSENCE` — joint auditable posture cannot use codec tri-state that leaks sibling field names in omit-only schema.
- `LAZY_DEFERRED → ¬REPLAY_STABLE` on same concept — lazy rail absence cannot gain `PERSIST_HORIZON` without fork promotion that materializes explicit variant arm before store.
- Derived closure table `frozendict[DerivedDescriptor, Callable[[frozenset[Descriptor]], bool]]` evaluates at import beside basis row — implication edges extend with derived nodes; closure failure is `REJECT`.
- Promotion unit updates derived closure when basis bits flip.

# Collapse Semigroup And Canonical Homomorphism

- Collapse operators form a semigroup `(Σ, ∘)` over stage-indexed morphisms — elements are `CollapseFn[Upstream, Downstream]` keyed by `AbsenceStage` pairs legal per `SEMIGROUP_TABLE: frozendict[tuple[AbsenceStage, AbsenceStage], CollapseFn | REJECT]`.
- Identity morphism `id_T: T → T` exists at `CANONICAL_MATERIALIZE` — `collapse ∘ id = collapse` and `id ∘ collapse = collapse` on collapsed types only; applying collapse to `UnsetType`, `type(MISSING)`, or PEP 661 sentinel after first collapse is illegal composition row.
- Associativity law: `(f ∘ g) ∘ h = f ∘ (g ∘ h)` for legal triples — import-time `prove_associativity(SEMIGROUP_TABLE, legal_transition_ids)` enumerates every `(s1,s2,s3)` where each adjacent pair is legal; mismatch between composed fn and table lookup is `SemigroupFault` at import, not a runtime property test.
- Canonical homomorphism `π: Σ → CanonicalPayload` maps collapse morphisms to payload algebra — `π(f ∘ g) = π(f) · π(g)` where `·` is variant selection or field merge on canonical owners; violation implicates adapter row, not domain fold.
- `seam_posture` morphism `σ: ForeignPosture → WitnessKind` is prefix, not parallel to foreign token remap — `τ_token ∘ σ = τ_composed` where `τ_token` is seam table; token and posture remaps compose only through shared witness kind.
- Double-collapse defect `(BOUNDARY_COLLAPSE, BOUNDARY_COLLAPSE)` is zero morphism — semigroup rejects zero-length canonical materialize hop; shortcut interior collapse is explicit `REJECT` composition row.
- Idempotence on canonical image: `collapse(t) = t` implies `collapse(collapse(x)) = collapse(x)` — second application on `T`, `Option[T]`, or variant arm is typed no-op; second application on axis carrier is breach.
- Semigroup table `frozendict[tuple[Stage, Stage], CollapseFn | REJECT]` is total over legal pairs from composition matrix — comprehension covers every legal pair; `assert_never` on illegal pair arms.

# Dominance Hasse Diagram Verification

- Dominance partial order is finite — `EncodingDomain` has four nodes; Hasse edges are cover relations, not full reachability prose.
- Cover edges pinned as data: `VARIANT ⊐ CODEC_TRI`, `VARIANT ⊐ RAIL` under auditable correlation, `RAIL ⊐ PARAMETER` under `RAIL_POSITION`, `CODEC_TRI ⊐ PARAMETER` under `PERSIST_HORIZON` — stored `frozenset[tuple[EncodingDomain, EncodingDomain]]`.
- `verify_dominance_closure(descriptors, order) -> EncodingDomain | REJECT` runs at import — algorithm: filter domains satisfying descriptor subset, take maximal elements under order, reject if antichain size > 1 without `WIRE_ONLY`/`CALL_ONLY` split.
- Antichain detection uses topological sort on induced subgraph — incomparable surviving domains emit `DominanceFault` with finite legal split recipes, not silent chooser default.
- Strictness: adding descriptor bit may only move dominance upward — `CORRELATED_ABSENCE` addition cannot demote `DOMAIN_VARIANT` to `DOMAIN_CODEC_TRI`; import asserts monotonicity over descriptor power set lattice.
- Hasse diagram stored beside `evaluate` — contract row includes `hasse_cover` frozenset.
- Machine proof obligation: for every concept, `evaluate` output domain equals `verify_dominance_closure` output.
- Dominance verification is O(1) per concept — four-domain finite set; no runtime graph library dependency.

# Witness Erasure And Chain Consumption Protocol

- `WitnessTag = NewType("WitnessTag", Literal["omit", "null", "param", "rail"])` — zero-cost carrier; full witness classes rejected unless owner row proves chain law cannot hold with tags alone.
- Mint table `frozendict[WitnessKind, MintFn]` produces `WitnessTag` at legal stage only — mint at `INGRESS_DECODE` for `OmitWitness`/`NullWitness` (`msgspec.UNSET` is falsey; mint predicate uses `is UNSET`, never truthiness); mint at callable boundary for `ParamWitness` (PEP 661 sentinel is truthy; mint predicate uses `is MISSING` only); mint at rail exit for `RailWitness` (`Option.none()` tag).
- Consume table `frozendict[WitnessTag, ConsumerFn]` is total — each tag consumed exactly once at successor stage per witness consumption law; unconsumed tag at `CANONICAL_MATERIALIZE` is import-time `REJECT` on consume table comprehension.
- Erasure law: `erase(tag: WitnessTag) -> None` at `CANONICAL_MATERIALIZE` — interior function signatures never mention `WitnessTag`; `beartype` entrypoints after erasure accept only collapsed types; `PySentinel_Check` and `msgspec` unset probes belong in mint predicates only, not interior signatures.
- Tag bijection row links `WitnessKind` enum members to `WitnessTag` literals without sharing enum class with wire `StrEnum` — three namespaces remain: wire tokens, witness kinds, erased tags; OpenAPI and schema emission never list witness tags.
- Chain tuple `tuple[WitnessTag, ...]` ordered by `AbsenceStage` index — duplicate tag in chain is `REJECT` at `evaluate` unless `TRANSITIONAL` fork outcome documents dual-mint window with paired consumers.
- Consumption failure routes to adapter owner — surviving tag past `enrichment_exit_to_materialization_exit` handoff fails metamorphic chain with `WitnessSurvivorFault` carrying finite legal consume rows from snapshot.

# Transitional Fork And Horizon Sunset Contract

- `ForkOutcome` gains `TRANSITIONAL` member — documents migration window where `SENTINEL_PARAM` ingress and `VARIANT_FORK` egress coexist under explicit sunset policy.
- `TRANSITIONAL` legal only when `sunset: datetime.date` and `successor_fork: ForkOutcome` bind on decision snapshot — import fails when `TRANSITIONAL` lacks both fields; production `evaluate` remains pure — sunset comparison runs in root replay guard, not inside `evaluate` import path.
- Dual-mint chain law under `TRANSITIONAL`: `ParamWitness` mint at callable boundary, `OmitWitness` or variant arm mint at `EGRESS_ENCODE` — both consumed before `REPLAY_REHYDRATE`; replay after sunset must reject param-only path.
- Horizon promotion `CALL → PERSIST` under `TRANSITIONAL` requires `successor_fork=VARIANT_FORK` — sunset date is latest day param-only admission accepted; day after sunset `evaluate` must return non-transitional snapshot.
- `TRANSITIONAL` row in promotion unit updates: sunset, successor fork, dual witness consumers, migration fold, breach catalog — partial transitional row fails registry build.
- Demotion `PERSIST → CALL` remains `REJECT` — only upward horizon promotion permitted through transitional window.
- Metamorphic transitional law: param omission before sunset round-trips through variant arm after sunset on replay — one negative test per owner with dated fixture corpus.

# Absence Handoff Row Federation

- `ABSENCE_HANDOFF_ROWS: tuple[HandoffRow, ...]` lives beside `DecisionSnapshot` at composition root — each row binds `transition_id`, `absence_stage`, `encoding_domain_legality`, `owner_symbol`, `witness_erasure`, `negative_fixture_ids`; absence modules import slugs from this tuple, never invent parallel `MaterializationStage` enums.
- `transition_id` slugs are stable snake identifiers shared with pipeline handoff tables — `ingress_carrier_to_validation_exit`, `validation_exit_to_normalization_exit`, `normalization_exit_to_construction_exit`, `construction_exit_to_enrichment_exit`, `enrichment_exit_to_materialization_exit`, `materialization_exit_to_wire_projection`, `cache_read_trusted_replay`, `history_envelope_rehydrate`, `enum_token_to_materialized_field` — row presence is import gate, not commentary.
- Stage legality matrix is `frozendict[tuple[AbsenceStage, EncodingDomain], bool]` derived from `ABSENCE_HANDOFF_ROWS` — each `absence_stage` maps to exactly one primary `transition_id`; duplicate stage keys without documented split recipe fail registry build.
- `BOUNDARY_COLLAPSE` rows pin `validation_exit_to_construction_exit` and `normalization_exit_to_construction_exit` — collapse executes at construction gate `owner_symbol` (`TypeAdapter`, smart constructor, `Decoder.decode` completion); interior modules never host second collapse without canonical materialize hop.
- `REPLAY_REHYDRATE` rows pin `cache_read_trusted_replay` and `history_envelope_rehydrate` with `encoding_domain_legality` forbidding `DOMAIN_PARAMETER` — replay omission requires variant bytes or codec tri-state, not PEP 661 sentinel identity reconstructed from stored names.
- `enum_token_to_materialized_field` row pins vocabulary admission at construction gate — wire token string upstream, `StrEnum` member downstream; absence collapse consumes member identity after admission, never substitutes raw string dispatch.
- Partial and async rows inherit domain legality from sync parent row — `partial_validation_exit` forbids `DOMAIN_PARAMETER`; `async_capture_carrier_read` completes carrier before witness mint; axis carriers do not re-enter past sync collapse kernel.

# Distinguishability And Posture Cardinality Invariant

- Absence encoding must preserve distinguishable postures — cardinality invariant `|postures| = 2^|basis_bits_active|` only when postures are independent; correlated concepts use variant arm count, not power set.
- `TRISTATE_WIRE` active implies `|postures| ≥ 3` on wire projection — omit, null, value are three distinguishable observables; JSON Schema `anyOf` cardinality checked against invariant.
- `CORRELATED_ABSENCE` active implies `|postures| = |variant_arms|` not `3^k` per field — mutual exclusion reduces Cartesian product; partition proof on `Rel` clique enforces arm count.
- `AUDIT_VISIBLE` active implies every posture maps to distinct wire token or variant discriminant — hash collision on redacted digest policy is breach row when `SECURITY_SENSITIVE` derived.
- Information defect: collapsing distinguishable postures without documented breach row is `REJECT` — chooser cannot merge omit and null when `TRISTATE_WIRE=1`.
- Metamorphic distinguishability law: generate one fixture per posture, project wire, decode — decoded posture class must match source class; conflation fails before domain import.
- Breach catalog links to cardinality rows — each breach class lowers achievable distinguishable count; tests assert invariant under declared breach, not universal isomorphism.

# Decision Snapshot Receipt And Audit Trail

- `evaluate` output materializes as frozen `DecisionSnapshot` record — fields: `concept_id`, `descriptor_basis`, `derived_flags`, `fork_outcome`, `encoding_domain`, `legal_transition_ids`, `witness_chain`, `horizon`, `rel_clique`, `breach_classes`, `hasse_cover`, `transitional_sunset | None`.
- Snapshot is immutable for process lifetime — composition root caches `frozendict[ConceptId, DecisionSnapshot]`; interior modules read snapshot, never re-run `evaluate`.
- Audit receipt variant `DecisionRecorded` appends snapshot hash to fact stream when `AUDIT_VISIBLE` — hash covers descriptor basis and fork outcome only, not full collapse source.
- Cross-worker proof: parent and child import identical snapshot `frozendict` keys and `fork_outcome` per concept — worker boot gate compares snapshot equality, not only enum member closure.
- Snapshot serialization for tooling uses msgspec struct with `schema_version` — tooling replay does not feed interior domain; diagnostic-only path.
- Promotion unit bumps snapshot `schema_version` when any snapshot field added — migration row for diagnostic corpora only; domain wire unaffected.
- `DecisionSnapshot` never stored inside canonical domain owner — receipt points to concept id and snapshot hash, not embedded collapse tables.

# Absence Refinement Type Bridge

- Static refinement bridges witness tags to collapsed types — `RefinedAbsent[T] = Annotated[T, Is[absence_refinement_predicate]]` lives on adapter module only.
- Refinement predicate table `frozendict[WitnessTag, Callable[[object], bool]]` pairs tag to runtime test — body uses `is` for PEP 661 sentinel and `msgspec.UNSET`, never truthiness; PEP 661 `__copy__` returning self preserves refinement identity through adapter hops.
- `TypeIs` mint predicates return `WitnessTag` on true arm — connects predicate doctrine to tag erasure without `TypeGuard` widening.
- Generic owners specialize refinements monomorphically — `Shape[Tag, Snapshot: DecisionSnapshot]` binds snapshot literal per concept; erasing `Snapshot` to `dict` voids refinement proof.
- Interior modules import collapsed `T` only — refinement annotations stripped at erasure boundary via adapter return type normalization.
- Static exhaustiveness pairs refinement with `assert_never` on impossible tag arms after total consume table — unreachable tag is compile error when snapshot literal bound.

# Evaluate Extension Algorithm

- Steps 9–12 extend the eight-step `evaluate` kernel on the same polymorphic entry; no parallel evaluators per axis or stage.
- Step 9 — compute derived descriptor closure; merge into snapshot `derived_flags` frozenset.
- Step 10 — run `verify_dominance_closure`; bind `encoding_domain`; attach `hasse_cover` witness.
- Step 11 — verify semigroup associativity for legal stage triples induced by snapshot `legal_transition_ids` — `REJECT` on illegal triple.
- Step 12 — when `fork_outcome=TRANSITIONAL`, validate sunset and successor fork; bind dual witness chain; else erase transitional fields to `None`.
- `TRANSITIONAL` with `REJECT` from step 2 horizon conflict overrides step 12 — param fork cannot persist without variant successor.
- Algorithm output unchanged shape — extended snapshot fields are additive; consumers pattern-match on `fork_outcome` first.
- Pure function law preserved — no I/O, no settings, no clock in production import.

# Metamorphic Semigroup Laws

- Associativity metamorphic: random legal triple `(s1,s2,s3)` of transition ids — composed collapse on fixture equals single-path collapse through intermediate canonical — failure implicates semigroup table row.
- Homomorphism metamorphic: `π(f∘g)(x) = π(f)(π(g)(x))` on lawful fixture x per concept — variant and field merge must commute with adapter collapse.
- Erasure metamorphic: witness chain minted at ingress — no `WitnessTag` survives `enrichment_exit_to_materialization_exit` handoff row — interior entry `beartype` rejects tag carriers and axis objects (`UNSET`, `MISSING`, module sentinel).
- Transitional metamorphic: fixture corpus dated before and after sunset — before admits param path; after requires variant bytes on replay.
- Distinguishability metamorphic: one fixture per posture class — round-trip preserves posture class count; merged postures fail cardinality invariant.
- Derived descriptor metamorphic: `SECURITY_SENSITIVE` concept — log capture fixtures contain no raw withheld wire tokens.
- Snapshot stability metamorphic: reload vocabulary module in worker — `DecisionSnapshot` per concept bit-identical.

# Functor Laws On Collapse Pipelines

- Collapse morphisms optionally lift to functor `F` over bounded payload categories — `F(collapse_fn)` maps variant arms to variant arms; object mapping sends axis carriers to initial object rejected by type system.
- Functor identity: `F(id_T) = id_{F(T)}` on canonical image — enrichment and materialization handoffs preserve identity collapse as no-op `copy.replace` when fields unchanged.
- Functor composition: `F(f ∘ g) = F(f) ∘ F(g)` for legal pairs — violation at `enrichment_exit_to_materialization_exit` implicates adapter conflating variant arms during enrichment.
- Constant morphism rejection: collapse sending all inputs to one canonical value without descriptor `DEFAULT` member is illegal — total dispatch doctrine forbids silent constant collapse except explicit default enum arm.
- Naturality against wire projection: square commutes when `π_wire ∘ collapse_ingress = collapse_egress ∘ π_canonical` — metamorphic proof draws commutative diagram per concept with lawful fixture.
- Enrichment handoff must not widen witness tags — `construction_exit_to_enrichment_exit` row forbids `WitnessTag` in enrichment input signature; naturality breaks if enrichment re-mints omit tags.

# Composition Root Snapshot Binding

- Root imports `frozendict[ConceptId, DecisionSnapshot]` before handler `Bind` publish — `params_type` on each `Bind` row must match snapshot `encoding_domain` collapse output, not pre-snapshot guess.
- `Bind.params_type` construction reads snapshot only — handler modules import collapsed types; snapshot hash logged at root boot for audit correlation.
- Registry partition runs after snapshot cache — unassigned concept in snapshot table fails import before handler map comprehension.
- Root `project_*_config` resolves inherit through snapshot `legal_transition_ids` — settings slice concepts without snapshot row cannot enter boot path.
- Seam adapters receive snapshot slice for foreign concept mapping — `frozendict[ForeignConceptId, ConceptId]` links foreign names to evaluated snapshot keys.
- Fault envelope carries `concept_id` and snapshot hash on absence violations — `WitnessSurvivorFault` and `DominanceFault` include finite legal fork outcomes from snapshot, not free text.
- Strict policy at root remains orthogonal — `--strict` faults do not substitute for missing snapshot row on new enum concept.

# Proof Obligation Register

- `PO-DOM-01` — Hasse cover matches `verify_dominance_closure` for all concepts.
- `PO-DOM-02` — `prove_associativity` passes for all legal transition triples induced per concept snapshot.
- `PO-DOM-03` — Witness consume table total; no survivor past `enrichment_exit_to_materialization_exit` erasure handoff.
- `PO-DOM-04` — `ABSENCE_HANDOFF_ROWS` covers every `AbsenceStage` key referenced in semigroup and stage legality matrices.
- `PO-DOM-05` — Distinguishability count matches posture fixtures per concept.
- `PO-DOM-06` — `TRANSITIONAL` rows include sunset and successor fork or import fails.
- `PO-DOM-07` — Derived closure predicates consistent with basis bits on all concepts.
- `PO-REG` — `register_law` binds `evaluate`, `SEMIGROUP_TABLE`, `ABSENCE_HANDOFF_ROWS`, and `DecisionSnapshot` symbols.
