# Absence Adjunction Galois Connection And Encode Adequacy

# Galois Connection Descriptor Posture Lattice

- Finite lattices `D` (descriptor subsets closed under implication closure) and `P` (distinguishable posture antichains under witness-chain cardinality invariant) form Galois connection `α: D → P`, `γ: P → D` — `α(S)` is minimal posture set realizing every descriptor in `S`; `γ(Q)` is maximal descriptor subset satisfied by posture set `Q`.
- Galois law `α(S) ⊆ Q iff S ⊆ γ(Q)` — single biconditional checked per concept at import; violation is `GaloisFault` with finite counterexample posture and descriptor witness.
- Monotonicity: `S₁ ⊆ S₂ ⇒ α(S₁) ≤ α(S₂)` in posture lattice — adding `CORRELATED_ABSENCE` cannot shrink distinguishable postures; dominance-order monotonicity is corollary, not independent axiom.
- `α ∘ γ ∘ α = α` and `γ ∘ α ∘ γ = γ` — closure operators on respective lattices; `evaluate` snapshot stores `galois_certificate: frozenset[tuple[frozenset[Descriptor], frozenset[PostureClass]]]` not recomputed at runtime.
- Posture classes are not wire tokens — `PostureClass(StrEnum)` meta-vocabulary indexes equivalence classes of observables; bijection row links to `WitnessTag` mint classes and encode counit tags without sharing wire `StrEnum`.
- Correlated concepts: `α` maps clique `Rel` to single variant-arm posture class, not Cartesian product — power-set posture count forbidden when `CORRELATED_ABSENCE=1`.
- `SECURITY_SENSITIVE` derived descriptor tightens `γ` — posture classes requiring redacted egress sit lower in `P`; Galois certificate includes redaction morphism obligation on encode side.
- Contract row `galois_cover` frozenset pins Hasse edges on `P` — diff gate fails when posture class count changes without Galois certificate update.

# Left Adjoint Collapse And Right Adjoint Encode

- Adjunction `L ⊣ R` over bounded absence pipeline: `L: WireCarrier → Canonical` (collapse/admission), `R: Canonical → WireCarrier` (encode/projection) — `Hom(L(w), c) ≅ Hom(w, R(c))` natural in `w` and `c` for lawful carriers only.
- `L` factors as witness mint ∘ domain collapse ∘ canonical materialize from the collapse semigroup table — `L` is not a new collapse table; adjunction certifies existing forward morphisms are left adjoint to chosen encode policy.
- `R` is contravariant on egress field ordering — `EncodePolicy` row selects omit/null/token emission per posture class; `R` never reintroduces `WitnessTag` or axis carriers; output is wire struct or bytes only.
- Unit morphism `η: id → R ∘ L` — canonical owner after collapse embeds into encode-then-decode round-trip; metamorphic law: `decode(R(c))` refines `c` up to breach tolerance encoded on snapshot.
- Counit morphism `ε: L ∘ R → id` on wire image — encoded wire after decode-collapse equals original wire up to breach catalog; `ε` failure implicates encode policy row, not interior fold.
- Adjunction pins encode policy as morphism, not `omit_defaults` / `exclude_unset` flags — `frozendict[PostureClass, EncodeFn]` total over posture classes in `α(descriptor_basis)`; missing encode row is import `REJECT`.
- `TRANSITIONAL` fork binds dual adjunction — `L_param ⊣ R_variant` before sunset, `L_variant ⊣ R_variant` after; counit path switches at `sunset_instant`; param-only `R` illegal after sunset.
- Illegal adjunction: `L` on `DOMAIN_PARAMETER` paired with `R` that emits wire token for caller omission — counit would breach sentinel isomorphism row in the breach tolerance catalog.

# Contravariant Encode Functor And Policy Naturality

- Contravariant functor `Encode*: Canonical^op → Wire^op` — `Encode*(f)(c') = f(c')` for canonical morphisms `f`; egress adapters implement `Encode*` on posture-preserving canonical updates only.
- Encode policy naturality: for canonical replace `ρ: c → c'` via struct copy/replace, square `Encode*(ρ) ∘ R(c) = R(c') ∘ ρ_wire` commutes when `ρ` preserves posture class — interior enrichment replace must not break encode naturality.
- `Encode*` rejects constant morphisms — collapsing all canonical values to one wire shape without `DEFAULT` enum arm violates total vocabulary dispatch; encode policy cannot erase distinguishable postures.
- CLI and settings egress use same `Encode*` instance as persistence — parallel encode policies per projection without seam remap row is lattice defect; projection lattice law requires contravariant image of one `R` across ingress, settings, and persistence.
- OpenAPI schema emission is `Forget ∘ R` — forgetful functor drops counit detail; schema never lists `PostureClass` or counit tags; distinguishability invariant checked on `R`, not on schema shadow.
- Seam anti-corruption: foreign encode is `R_foreign`; canonical encode is `R_canonical`; seam adapter natural transformation `ν: R_canonical ⇒ F_seam ∘ R_foreign` — not a second collapse table.

# Unit Counit Metamorphic Adequacy Laws

- Unit adequacy chain: canonical fixture `c` → `R(c)` → `L(R(c))` refines `c` — refinement via `RefinedAbsent` bridge on canonical image; failure attributes to `η` row on snapshot.
- Counit adequacy chain: lawful wire fixture `w` → `L(w)` → `R(L(w))` equals `w` modulo `frozendset[BreachClass]` tolerance — each breach class documents which counit equation weakens to one-way law.
- Triangular identity: `ε_{L(w)} ∘ L(η_w) = id_{L(w)}` — collapse after unit embedding is identity on collapsed image; enumerated per `ConceptId` at import beside collapse semigroup triple enumeration.
- Co-triangular identity: `R(ε_c) ∘ η_{R(c)} = id_{R(c)}` — encode after counit on canonical is identity on encoded image; failure means encode policy merges distinct postures.
- Adequacy is asymmetric by design — unit may be strict where counit is breach-modulo; snapshot stores `adequacy_profile: frozenset[AdequacyClass]` with `STRICT_UNIT`, `BREACH_COUNIT`, `ONE_WAY_SEAM` rows.
- `ONE_WAY_SEAM` adequacy: foreign-to-canonical only — `L_foreign` satisfies unit; counit on foreign wire intentionally undefined; seam table documents directionality.

# Comonad Extract Duplicate On Canonical Image

- Canonical carriers after erasure carry comonad `(C, ε, δ)` — `ε: C → C` is extract (identity on collapsed values); `δ: C → C` is duplicate (context copy for audit/receipt only).
- Comonad laws: `ε ∘ δ = id`, `ε ∘ ε = ε`, `δ ∘ δ = (id ⊗ δ) ∘ δ` — checked on canonical image types only; `δ` must not re-mint `WitnessTag` on interior paths.
- `δ` duplicates `DecisionSnapshot` hash into receipt context — interior folds read snapshot context without importing semigroup table; comonad context is read-only audit fiber.
- Enrichment handoff forbids `δ` that widens fields to axis carriers — `construction_exit_to_enrichment_exit` row rejects comonad duplicate reintroducing `UnsetType`.
- Comonad is not a persistence monad — `δ` does not serialize; replay uses root decoder `L`, not comonad duplicate; pickle of canonical owners strips comonad context.
- Kleisli arrows from the collapse semigroup embed into comonad-coKleisli egress planning — `R` factors through coKleisli when encode policy reads comonad context for `AUDIT_VISIBLE` receipts.

# Concept Sum Product And Field Tensor Algebra

- Multiple absence-bearing fields on one owner compose by concept tensor — `ConceptId` sum `C₁ + C₂` for tagged variant owners; product `C₁ × C₂` for independent fields sharing ingress struct.
- Sum evaluation: `evaluate(C₁ + C₂) = evaluate(C₁) ⊔ evaluate(C₂)` — coproduct in snapshot lattice merges `legal_transition_ids` and takes join of posture classes under `α`; dominance computed per arm, not on product descriptor bits.
- Product evaluation: `evaluate(C₁ × C₂) = evaluate(C₁) ⊗ evaluate(C₂)` — tensor requires independence: `Rel` cliques empty across product boundary; correlated product without variant fork is `REJECT`.
- Product Galois: `α(S₁ × S₂) = α(S₁) × α(S₂)` only when independence proven — else product forces `VARIANT_FORK` on outer owner.
- Field tensor contract row `frozendict[FieldId, ConceptId]` drives per-field snapshot slice — handler `Bind.params_type` matches tensor collapse of field concepts, not owner-level guess.
- Promotion unit on owner field add updates tensor row and re-verifies adjunction per concept slice — partial tensor update fails registry import.

# Inverse Adequacy And Breach Tolerance Algebra

- Breach tolerance is not ad hoc — `frozendict[BreachClass, AdequacyRelaxation]` maps each documented breach class to which adjunction equation weakens and to what degree.
- `JSON_NULL_NOT_OMIT` relaxes counit on omit-vs-null only — unit remains strict; metamorphic counit uses three fixtures not two.
- `SENTINEL_NOT_WIRE` relaxes counit entirely on param paths — unit strictness still required after `L`; one-way adequacy profile mandatory.
- `OPTION_NOT_OMIT` relaxes counit when `RAIL_POSITION=1` — encode may omit key while rail carries `Option.none`; relaxation row documents consumer contract.
- Tolerance composition: `τ₁ ∘ τ₂` only when breach classes commute — non-commuting relaxations on same concept is `REJECT`; tolerance semigroup is smaller than collapse semigroup.
- Inverse admission `L⁺` partial inverse of `L` on posture-class image — exists only when unit adequate; `L⁺` used in trusted-replay diagnostics, not production ingress.

# Triangular And Galois Verification Register

- `PO-ADJ-01` — Galois certificate satisfies `α ∘ γ ∘ α = α` per concept on finite descriptor power set fragment.
- `PO-ADJ-02` — Unit chain `decode ∘ R` refines identity on canonical fixtures per posture class.
- `PO-ADJ-03` — Counit chain `R ∘ L` equals identity modulo declared breach tolerance per class.
- `PO-ADJ-04` — Triangular and co-triangular identities enumerated for legal transition triples induced by snapshot.
- `PO-ADJ-05` — `Encode*` naturality square commutes on enrichment replace fixtures.
- `PO-ADJ-06` — Comonad laws on canonical image without `WitnessTag` reintroduction.
- `PO-ADJ-07` — Concept tensor coproduct/product snapshots match field tensor row.
- `PO-ADJ-08` — `TRANSITIONAL` adjunction switch at `sunset_instant` UTC verified in dated fixture corpus.

# Kan Extension Across Version Seams

- Vocabulary version seam is left Kan extension of `L_v` along migration functor `M: v → v+1` — `Lan_M L_v` must agree with `L_{v+1}` on migrated fixtures; disagreement is `MigrationAdequacyFault`.
- Right Kan extension `Ran_M R_v` governs encode on legacy stores — egress to obsolete consumers uses extended encode policy; sunset on `TRANSITIONAL` is instance of Kan restriction.
- Kan extension exists only when vocabulary migration fold is total — partial `frozendict[LegacyToken, Token]` on the migration owner blocks adjunction extension; adjunction certification consumes migration tables, never duplicates them.
- Pointwise adequacy: for each migrated fixture, `M(L_v(w)) ≅ L_{v+1}(M(w))` — metamorphic row per `schema_version` bump.

# Composition Root Adjunction Binding

- Root caches `frozendict[ConceptId, AdjunctionRow]` beside `DecisionSnapshot` — row holds `L` symbol, `R` symbol, `η` witness, `ε` witness, `adequacy_profile`, `galois_certificate`, `encode_policy` table.
- `Bind.params_type` uses `L` output type; egress handlers use `R` input type on canonical — mixed handler that encodes without snapshot `R` row is import `REJECT`.
- Root boot logs adjunction profile hash per concept — audit correlation with `DecisionRecorded` receipt variants.
- Seam adapters import `Ran_M R` when foreign consumer requires legacy encode — not a domain concern.

# Failure Archaeology Extension

- Unit failure (`η` break) routes to encode policy owner — interior fold innocent.
- Counit failure (`ε` break) routes to collapse or breach misclassification — check tolerance row before collapse table.
- Galois failure routes to descriptor or posture mis-count — dominance Hasse cover secondary to Galois certificate.
- Triangular failure routes to illegal semigroup shortcut — collapse composition table primary.
- Tensor failure routes to hidden correlation across product fields — variant owner must fork.
- `MigrationAdequacyFault` routes to version Kan extension row — not interior domain.

# Counit Tag Namespace And Erasure Partition

- Egress counit certifies emitted wire posture — `CounitTag = NewType("CounitTag", Literal["emit_omit", "emit_null", "emit_token", "emit_variant"])` partitions from ingress `WitnessTag` literals — shared string value across namespaces is `REJECT`.
- Mint table `frozendict[PostureClass, CounitTag]` pairs posture class to emit class — total over `α(descriptor_basis)`; `emit_variant` mandatory when `AUDIT_VISIBLE` and `CORRELATED_ABSENCE`.
- Counit consume at wire bytes boundary — `ε` witness compares emitted bytes against `CounitTag` expectation; mismatch before foreign seam is encode policy fault.
- Erasure partition law: ingress tags erased at `CANONICAL_MATERIALIZE`; counit tags minted only at `EGRESS_ENCODE` handoff — interior signatures mention neither; `beartype` bans both past respective boundaries.
- Snapshot `schema_version` bump when `CounitTag` literals added — four-literal `WitnessTag` ingress closure preserved under separate namespace; migration row documents dual-namespace proof harness.

# Kleisli CoKleisli Pipeline Embedding

- Forward Kleisli arrows `η_K: w → L(w)` compose collapse morphisms from the collapse semigroup — Kleisli composition `>=>` is associativity law already proven; adjunction does not replace semigroup table.
- Backward coKleisli arrows `ε_K: R(c) → c` plan encode from comonad context — `R` coKleisli morphism reads `DecisionSnapshot` hash from `δ(c)` without importing collapse table.
- Kleisli-coKleisli adjunction correspondence: `Hom_K(w, R(c)) ≅ Hom(c, L(w))` when `w` lawful — metamorphic proof reuses unit/counit chains as Kleisli/coKleisli round-trips.
- Interior handlers are coKleisli morphisms from canonical comonad — handlers never Kleisli-compose on wire carriers; root rail closes Kleisli chain at ingress adapter only.
- `singledispatch` registers on coKleisli canonical inputs post-erasure — registering on Kleisli wire inputs violates axis-collapse invariant (wire carriers never reach interior dispatch).

# Posture Class Observability And Redaction Morphism

- `PostureClass` indexes observables, not secrets — `SECURITY_SENSITIVE` derived flag binds redaction morphism `ρ: PostureClass → RedactedDigest` on encode audit surfaces.
- Developer log projection uses `ρ ∘ R` — wire-audit projection uses full `R` with `CounitTag` — owner policy row pins surface selection; mixing surfaces without row is defect.
- Observability span tags carry `posture_class` and `adequacy_profile` — not raw `WitnessTag` or `CounitTag` literals on interior spans.
- Receipt `AliasUsed` and `DecisionRecorded` variants carry `PostureClass` when `AUDIT_VISIBLE` — folds branch on posture class enum, not emit spelling.
- Redaction morphism idempotent: `ρ ∘ ρ = ρ` — double redaction on egress is no-op; metamorphic log fixtures assert token literals absent when `ρ` active.

# Simultaneous Promotion Unit For Adjunction Change

- Descriptor bit flip updates: Galois certificate, `Encode*` policy table, counit mint table, adequacy profile, tolerance row, tensor field row, Kan extension row — partial update fails import registry.
- `PostureClass` addition requires simultaneous OpenAPI cardinality check, metamorphic counit fixture, and `PO-ADJ-03` row — orphan posture class without encode policy is merge blocker.
- `sunset_instant` change on `TRANSITIONAL` requires dated fixture corpus refresh and `PO-ADJ-08` rerun — calendar-only edits rejected.
- Fifth namespace (`CounitTag`) promotion bumps snapshot `schema_version` and diagnostic replay migration — domain wire tokens unaffected when encode literals unchanged.
