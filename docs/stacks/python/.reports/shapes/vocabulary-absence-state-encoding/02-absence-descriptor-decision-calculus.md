# Absence Descriptor Decision Calculus — Implication Lattice, Dominance, And Witness Chain

# Descriptor Implication Lattice

- Descriptors are not independent — implication edges form a finite lattice consumed by the dominance engine before domain assignment; edges are `frozendict[Descriptor, frozenset[Descriptor]]` closure rows, not comments.
- `CORRELATED_ABSENCE → TRISTATE_WIRE` — joint posture implies at least three distinguishable wire postures or variant arms; correlated pair without tri-state capability is `REJECT` at import.
- `PERSIST_HORIZON → REPLAY_STABLE` — durable absence must survive versioned replay; replay-stable without persist horizon is legal only when horizon is `SESSION`.
- `AUDIT_VISIBLE → PERSIST_HORIZON` when `ForkOutcome=VARIANT_FORK` — auditable variant arms must round-trip; audit-only sentinel without persist row is `REJECT`.
- `RAIL_POSITION → ¬AUDIT_VISIBLE` on same concept — computed absence and auditable wire token on one slot is `REJECT`; split concepts instead of stacking bits.
- Implication violation fails at vocabulary owner import — not deferred to first `evaluate` call; lattice closure is computed once and cached beside descriptor rows.

# Descriptor Basis

- Six binary descriptors form the decision basis — each concept maps to a `frozenset[Descriptor]` row beside the vocabulary owner; import partition proves every exported concept assigns exactly six bits, not five, not seven.
- **`AUDIT_VISIBLE`** — absence must appear in wire logs, receipts, or OpenAPI; when true, encoding must survive egress as named token or variant discriminant — parameter-only sentinel is illegal regardless of manual routing habit.
- **`PERSIST_HORIZON`** — absence state must round-trip through persistence; when true, encoding must not rely on pickle-only PEP 661 sentinel identity or session-local `UNSET` without root decoder policy row.
- **`RAIL_POSITION`** — absence is computed outcome of an operation, not caller omission; when true, carrier must be `Option`/`Result` terminus — sentinel and wire tri-state are illegal at the rail exit.
- **`TRISTATE_WIRE`** — consumer contract distinguishes key absent, key null, and domain value on the same field; when true, scalar `T | None` without third arm is illegal — variant fork or codec tri-state row required.
- **`CORRELATED_ABSENCE`** — field absence correlates with sibling field presence; when true, per-field collapse rows are illegal — tagged variant owner must own the joint posture.
- **`REPLAY_STABLE`** — absence evidence must survive `schema_version` ladder without silent reinterpretation; when true, encoding must pin migration fold row before enum construction — version-agnostic sentinel rebind is illegal.
- Descriptor row is data consumed by the decision engine — not prose in routing comments; promotion unit updates descriptor bits before fork-outcome or encoding-domain rebinding.

# Encoding Domain Partition

- Four encoding domains partition the solution space — domains are mutually exclusive carriers per concept at a given pipeline stage; carrying two domains on one slot without variant fork is a lattice defect.
- **`DOMAIN_VARIANT`** — absence is a `tagged_union` arm (`Present[T] | Withheld | ExplicitNull`); owns correlated and tri-state invariants jointly; interior `match` is sole dispatch.
- **`DOMAIN_CODEC_TRI`** — absence is `msgspec.UNSET` or pydantic experimental `MISSING` on ingress struct field only — `UNSET` is falsey, `MISSING` compares with `is`; owns wire key posture; collapses before domain materialization; never stored on frozen canonical owner.
- **`DOMAIN_PARAMETER`** — absence is module-global `builtins.sentinel("NAME")` on callable signature; PEP 661 sentinels are truthy — guards use `is`, never boolean coercion; owns caller omission; collapses at smart-constructor gate; never serializes.
- **`DOMAIN_RAIL`** — absence is `Option`/`Result` channel; owns computed inapplicability; never used as constructor default on frozen records.
- Domain assignment is a pure fold over descriptor bits — no manual routing table; dominance order resolves ties when multiple domains satisfy bits; output is one `EncodingDomain` bound in `DecisionSnapshot`, not a parallel vocabulary-family pass.

# Dominance Partial Order

- Encoding domains form a strict partial order under descriptor satisfaction — not a flat routing table; higher domain wins when multiple domains satisfy the same descriptor subset.
- **`DOMAIN_VARIANT` ⊐ `DOMAIN_CODEC_TRI`** when `CORRELATED_ABSENCE` or (`TRISTATE_WIRE` and `AUDIT_VISIBLE`) — variant fork dominates codec tri-state; tri-state scalar union is rejected even if msgspec supports it.
- **`DOMAIN_RAIL` ⊐ `DOMAIN_PARAMETER`** when `RAIL_POSITION` — operation outcome dominates caller omission; sentinel default on operation exit is rejected.
- **`DOMAIN_CODEC_TRI` ⊐ `DOMAIN_PARAMETER`** when `PERSIST_HORIZON` and not `RAIL_POSITION` — wire-round-trippable absence dominates session-local sentinel; sentinel on persisted ingress struct is rejected.
- **`DOMAIN_VARIANT` ⊐ `DOMAIN_RAIL`** when `AUDIT_VISIBLE` and `CORRELATED_ABSENCE` — auditable joint posture dominates silent `Option.none()` without receipt variant.
- Incomparable pairs (`DOMAIN_PARAMETER` vs `DOMAIN_CODEC_TRI` without `PERSIST_HORIZON`) require explicit `WIRE_ONLY` vs `CALL_ONLY` descriptor split on two concepts — merging into one slot is rejected; dominance does not fabricate a winner.
- Dominance engine output is one domain per concept per stage — stage legality matrix filters legal domains further; dominance without stage legality is insufficient for admission.

# Stage Legality Matrix

- Pipeline stages form a second axis orthogonal to encoding domains — `MaterializationStage(StrEnum)` values `INGRESS_DECODE`, `BOUNDARY_COLLAPSE`, `CANONICAL_MATERIALIZE`, `INTERIOR_FOLD`, `EGRESS_ENCODE`, `REPLAY_REHYDRATE`; legality is `{stage × domain → bool}` encoded as `frozendict[tuple[MaterializationStage, EncodingDomain], bool]` beside root codec submodule.
- **`INGRESS_DECODE`** — permits `DOMAIN_CODEC_TRI`, `DOMAIN_VARIANT` discriminant selection; forbids `DOMAIN_PARAMETER`, `DOMAIN_RAIL`.
- **`BOUNDARY_COLLAPSE`** — permits all domains as input; output slots typed only `DOMAIN_VARIANT` arms or collapsed `T` / `Option` — `UnsetType` and sentinel objects forbidden on collapse output type.
- **`CANONICAL_MATERIALIZE`** — permits collapsed `T`, `Option`, enum members, variant instances; forbids every absence carrier object.
- **`INTERIOR_FOLD`** — permits `Option`/`Result` rails only; forbids reintroduction of codec tri-state or sentinel via `replace` widening.
- **`EGRESS_ENCODE`** — permits `DOMAIN_CODEC_TRI` projection from canonical; `DOMAIN_PARAMETER` and `DOMAIN_RAIL` never encode — adapter row maps collapsed canonical to omit/null/token per contract.
- **`REPLAY_REHYDRATE`** — permits `DOMAIN_CODEC_TRI` and `DOMAIN_VARIANT` from stored bytes; `DOMAIN_PARAMETER` illegal — replay reconstructs caller omission only through explicit stored variant, not sentinel identity from bytes.
- Illegal stage×domain pairs fail at composition-root import when concept descriptor row is bound — not at first runtime call; matrix is total over exported concepts.

# Fork Decision Tree

- Variant fork vs scalar tri-state vs sentinel default is a decision tree on descriptors — not manual routing; tree output is `ForkOutcome(StrEnum)` with values `VARIANT_FORK`, `CODEC_TRI`, `SENTINEL_PARAM`, `RAIL_OPTION`, `REJECT`.
- `CORRELATED_ABSENCE=1` → `VARIANT_FORK` unconditionally — no scalar escape hatch.
- `TRISTATE_WIRE=1` and `AUDIT_VISIBLE=0` → `CODEC_TRI` on ingress struct; collapse to `T` or `Option` at boundary per `RAIL_POSITION`.
- `TRISTATE_WIRE=1` and `AUDIT_VISIBLE=1` → `VARIANT_FORK` with `ExplicitNull` arm — tri-state must be match-visible in interior.
- `PERSIST_HORIZON=0` and `RAIL_POSITION=0` and `AUDIT_VISIBLE=0` → `SENTINEL_PARAM` on callable signature only.
- `RAIL_POSITION=1` → `RAIL_OPTION` at operation boundary; other domains rejected on same concept.
- `REPLAY_STABLE=1` and chosen outcome is `SENTINEL_PARAM` → `REJECT` — remap to `VARIANT_FORK` or `CODEC_TRI` with migration row; sentinel-only concepts cannot gain `PERSIST_HORIZON` without fork promotion unit.
- Tree is evaluated once per concept at owner import — runtime re-evaluation is forbidden; concept splits require new owner, not dynamic tree walk.

# Witness Chain Typing

- Absence proofs flow as witness objects through collapse — witnesses are not `TypeIs` predicates alone; predicates mint witnesses; interior folds consume witness tags, not raw carriers.
- **`OmitWitness`** — certifies wire key was absent at decode; minted by `is UNSET` / `is MISSING` predicate on ingress field; consumed at collapse to `Withheld` variant or parameter default resolution.
- **`NullWitness`** — certifies explicit null on wire distinct from omission; minted by `is None` arm after tri-state decode; consumed at collapse to `ExplicitNull` variant or domain `NullToken` member.
- **`ParamWitness`** — certifies caller did not supply argument; minted by `is` on module-global PEP 661 sentinel binding; consumed at smart-constructor gate only — never stored on frozen owner.
- **`RailWitness`** — certifies operation returned no value; minted by `Option` inspector at rail exit; consumed by downstream `bind` — never coerced to `ParamWitness`.
- Witness chain law: `mint(stage_n) → consume(stage_n+1)` — witness minted at `INGRESS_DECODE` must be consumed at `BOUNDARY_COLLAPSE`; surviving witness past `CANONICAL_MATERIALIZE` is defect at interior entry.
- `frozendict[WitnessKind, ConsumerFn]` is total over `WitnessKind(StrEnum)` — witness kinds, wire `StrEnum` tokens, and encoding-domain enums stay in separate classes; bijection proof row links witness kinds to encoding domains without enum sharing.

# Multi Field Absence Algebra

- Correlated absence is a relation `Rel ⊆ FieldId × FieldId`, not per-field collapse — stored as `frozenset[tuple[FieldId, FieldId]]` beside variant owner; partition proves every correlated pair lives in one variant arm.
- Mutual exclusion law: `(a,b) ∈ Rel` implies ingress cannot admit `Present(a)` and `OmitWitness(b)` simultaneously — validator graph rejects before domain import.
- Independence default: fields absent `Rel` collapse independently via per-field witness consumers — correlation descriptor `CORRELATED_ABSENCE=0` forbids non-empty `Rel`.
- `Rel` composition is transitive closure at import — implicit correlation must be explicit in relation set; transitive edges not in closure fail partition proof.
- N-ary correlation (`a` forbids `b` and `c`) requires variant arm owning joint posture — binary `Rel` rows pair-wise encode n-ary via clique partition on variant discriminant.

# Codec Isomorphism Breach Catalog

- Absence encodings are isomorphic only within declared codec class — breaches are merge blockers cataloged per owner row, not discovered at runtime.
- **JSON omit ≡ msgspec `UNSET` omit** when `omit_defaults=True` and field specifier matches — breach when Pydantic `exclude_unset` or `exclude_none` semantics diverge from msgspec on the same struct shape.
- **JSON `null` ≢ key absent** in all codecs — protobuf optional scalar, JSON Schema `nullable`, and Python tri-state agree only when `TRISTATE_WIRE=1` row documents third arm; treating as isomorphic is defect.
- **Sentinel identity ≢ wire token string** under any codec — breach if persistence stores `sentinel.__name__` or `"__missing__"` literal; metamorphic law is one-way collapse only.
- **`Option.none` ≢ JSON omit** — rail absence is not wire omission unless `AUDIT_VISIBLE=0` and egress row explicitly maps `none()` to key deletion at adapter.
- **IntFlag normalized int ≢ named decomposition** — breach under replay when stored integer changes under `IntFlag(x)`; `PERSIST_HORIZON=1` rows must pin named token policy.

# Collapse Composition Law

- Foreign-posture remap alone cannot reach canonical owners — decision calculus mandates `foreign_posture → witness mint → domain collapse → canonical` with associativity checked on the finite `MaterializationStage` enum.
- Composition table `frozendict[tuple[MaterializationStage, MaterializationStage], ComposeLegality]` marks legal witness compositions — `(INGRESS_DECODE, BOUNDARY_COLLAPSE)` legal; `(BOUNDARY_COLLAPSE, INTERIOR_FOLD)` legal only when `RailWitness` present; `(CANONICAL_MATERIALIZE, EGRESS_ENCODE)` legal only on collapsed `T` or variant.
- Illegal composition `(BOUNDARY_COLLAPSE, BOUNDARY_COLLAPSE)` — double collapse without canonical materialize is `REJECT`.
- Idempotence law — `collapse ∘ collapse = collapse` on canonical types; second collapse on `T` is no-op typed identity; second collapse on `UnsetType` is defect.
- Seam-first law — foreign posture remap runs before witness mint; `frozendict[ForeignPosture, WitnessKind]` precedes `frozendict[WitnessKind, ConsumerFn]` — foreign token remap and posture remap compose only through witness kind, not parallel tables.

# Replay And Temporal Absence

- Absence carries a temporal kind independent of encoding domain — `AbsenceHorizon(StrEnum)` with values `CALL`, `SESSION`, `PERSIST`, `AUDIT` indexes which stages may reintroduce the absence evidence.
- `CALL` horizon — `ParamWitness` only; illegal on `REPLAY_REHYDRATE` — replay must materialize explicit variant if omission must survive.
- `SESSION` horizon — `UnsetType` on wire struct within process; illegal across worker boundary without `PERSIST` promotion.
- `PERSIST` horizon — variant arm or `NullToken` on canonical snapshot; migration fold owns horizon transitions across `schema_version`.
- `AUDIT` horizon — receipt/fact-stream variant with enum-typed discriminant; interior fold appends witness summary, not raw codec carrier.
- Horizon promotion (`CALL` → `PERSIST`) requires variant fork promotion unit — cannot add `PERSIST_HORIZON` descriptor without `ForkOutcome` change and migration row.
- Temporal law: absence at `T0` decoded under `schema_version=V` must not be reinterpreted as different horizon at `T1` without migration fold — partial-key version guess is rejected.

# Evaluate Algorithm

- `evaluate` is one polymorphic entry — input `ConceptId` enum member; output frozen decision snapshot; no sibling `evaluate_wire` / `evaluate_param` split.
- Step 1 — load `frozenset[Descriptor]` row; run implication closure; `REJECT` on closure violation.
- Step 2 — run fork decision tree; bind `ForkOutcome`; `REJECT` on `SENTINEL_PARAM` with `PERSIST_HORIZON`.
- Step 3 — map `ForkOutcome` to `EncodingDomain` via dominance order over satisfied descriptor subset.
- Step 4 — filter domain through stage legality matrix; emit `frozenset[MaterializationStage]` legal set.
- Step 5 — mint default `WitnessChain` tuple ordered by stage — `OmitWitness` before `NullWitness` before `ParamWitness` before `RailWitness` per fork class.
- Step 6 — bind `AbsenceHorizon` from descriptor fold: `PERSIST_HORIZON` → `PERSIST`, `AUDIT_VISIBLE` → `AUDIT`, `RAIL_POSITION` → `CALL`, else `SESSION`.
- Step 7 — attach `Rel` clique when `CORRELATED_ABSENCE`; empty `frozenset` otherwise.
- Step 8 — select breach catalog rows applicable to chosen domain — attach `frozenset[BreachClass]`.
- Algorithm is pure — no I/O, no settings read; composition root caches `frozendict[ConceptId, DecisionSnapshot]` at import completion.

# Decision Engine Integration

- Composition root binds `evaluate(concept) -> frozendict[DecisionKey, DecisionValue]` at import — keys include `encoding_domain`, `fork_outcome`, `legal_stages`, `witness_chain`, `horizon`, `rel_clique`; values are closed enums or frozensets, not strings.
- Decision output is a read-only `DecisionSnapshot` row for materialization adapters — vocabulary owner does not re-derive descriptors at runtime; interior modules import the cached snapshot only.
- Promotion unit for descriptor change updates: descriptor bits, dominance resolution, fork outcome, stage matrix, witness consumers, `Rel` clique, breach catalog row, migration fold — partial update fails import-time registry build.

# Metamorphic Decision Laws

- Descriptor stability law — `evaluate(c)` at import equals `evaluate(c)` after module reload in worker boot gate; decision snapshot is immutable for process lifetime.
- Dominance monotonicity law — adding `CORRELATED_ABSENCE` to descriptor row never lowers domain from `DOMAIN_VARIANT` to scalar tri-state; strict dominance direction only.
- Witness consumption law — for each `WitnessKind` in chain, exactly one consumer at `legal_stages` successor; unconsumed witness at `CANONICAL_MATERIALIZE` fails metamorphic chain.
- Horizon monotonicity law — promotion `CALL` → `PERSIST` requires new `ForkOutcome`; demotion `PERSIST` → `CALL` is `REJECT` without deprecation migration unit.
- Breach negative law — each `BreachClass` in catalog produces one falsifiable metamorphic counterexample.
- Decision parity law — change to wire `StrEnum` member triggers descriptor diff when member is `NullToken` or `InheritToken` — token promotion and descriptor promotion are coupled.
