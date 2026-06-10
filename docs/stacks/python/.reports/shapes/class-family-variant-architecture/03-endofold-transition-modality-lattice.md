# Endofold Transition And Modality Morphism Lattice

# Endofold Migration And Re-Ingress Triage Matrix

- Endofold — total or partial function `Member -> Result[Member, E]` on current vocabulary — models in-process modality change after materialization exit; table keys are `(from_kind: Kind, to_kind: Kind)` pairs on canonical enum members, not wire tag strings or version literals alone.
- Migration fold — `StoredVn -> Member` or `MemberVn -> Member` — runs once at read or ingress boundary when stored or wire vocabulary differs from current alias; migration modules own obsolete arms with `assert_never`; active domain `match` never branches on retired kinds.
- Re-ingress — `dict | bytes -> Result[Member, E]` through root `TypeAdapter` and smart constructor — applies when delta arrives as foreign carrier or ingress-shaped patch; endofold must not call `model_validate` on full ingress models for field-level edits on already-materialized members unless composition root documents explicit `RE_INGRESS_ROWS` entry with `re_ingress_cross_ref` id.
- Enrichment — derived slots attach via owner methods or `copy.replace` without changing discriminant kind when policy treats enrichment as same-arm mutation — enrichment is not endofold unless `to_kind` differs from `from_kind`; same-arm slot updates use replacement-axis `ReplaceRow` registry keyed by owner symbol, not transition table entries.
- Reject conflating endofold rows with migration hops — `(MemberV1, MemberV2)` keys belong in migration lattice; `(Kind.A, Kind.B)` keys on current `Member` belong in endofold lattice; version literal on member payload does not substitute for `Kind` pair when both arms share current union alias.

# Transition Row Shape And Sparse Table Law

- Every admitted endofold declares one frozen row: `from_kind`, `to_kind`, `owner_symbol`, `replace_policy`, `proof_required`, `cross_field_guard`, `negative_fixture_ids`, optional `re_ingress_cross_ref`, optional `concurrency_posture`.
- `from_kind` and `to_kind` are distinct or equal `Kind` enum members when same-arm enrichment is explicitly modeled — equal pair rows are rare and require `replace_policy` documenting slot-only mutation; default table omits identity rows unless identity is explicit contract behavior.
- `owner_symbol` pins arm-aware replace entry — `copy.replace`, `model_copy(update=...)`, owner method `transition_to_*`, or validated reconstruction — per-request ad hoc field assignment on materialized members is a row violation.
- `replace_policy` is one of `{dataclass_replace, pydantic_copy, owner_method, validated_rebuild}` — maps to polymorphic `copy.replace` (Python 3.13+) on dataclass and msgspec members with `__post_init__` replay when defined; pydantic frozen members use `model_copy(update=...)` forwarding from `__replace__` without re-running validators unless row routes through `validated_rebuild`; `validated_rebuild` invokes arm-specific smart-constructor helper, not root `TypeAdapter(Member)` on unrelated carrier shapes.
- `proof_required` is boolean per row — polymorphic nested sub-owners on either arm set `true`; monomorphic scalar-only arms may set `false` when replace is slot projection without nested union change.
- `cross_field_guard` names id in root `CROSS_FIELD_LAW_ROWS` when transition on one sub-owner field implicates top-level cross-field rules — hierarchical families cite guard ids from smart-constructor registry, not duplicate law inside row handler.
- `negative_fixture_ids` index defects each row must reject — `ingress_replay_on_materialized`, `silent_default_to_kind`, `missing_invariant_fault_on_forbidden_pair`, `mutate_in_place`.
- Sparse table law — absent `(from_kind, to_kind)` entry returns typed `InvariantFault` with both kinds in evidence; forbidden pairs never map to identity return, catch-all arm, or preferred variant unless identity row explicitly declares that behavior.

# Hierarchical Product-Keyed Transitions

- Top-level endofold on outer discriminant uses outer `(from_kind, to_kind)` keys only — inner sub-owner modality unchanged unless row declares nested transition delegate.
- Product-keyed rows declare `(outer_from, outer_to, inner_from, inner_to)` when transition simultaneously changes outer and inner kind — sparse product table returns `InvariantFault` for undeclared quadruples; do not flatten to concatenated string keys.
- Outer-only transition preserves inner sub-field value when cross-field law permits — row metadata documents inner preservation or mandatory inner reset to default sub-arm; reset without row is merge blocker.
- Inner-only transition on sub-owner field runs sub-owner endofold table keyed on `(sub_from, sub_to)` after outer arm narrows — top-level table does not duplicate inner pairs unless outer transition bundles inner change in one atomic row with `atomic=true`.
- Sub-owner endofold tables prove independent parity — `SubKind` iteration bijection with sub-transition rows; top-level table parity does not subsume inner transition width.
- Block endofold over `Block[Member]` maps element-wise through same transition row lookup — batch transition inherits per-element legality; partial batch applying transition only to compatible elements without `InvariantFault` on forbidden elements violates totality law.

# Replace Semantics And Construction Gate Separation

- Dataclass and `@tagged_union` members use `copy.replace` or owner arm-aware replace — `dataclasses.replace` and `copy.replace` share the same kernel and invoke `__post_init__` when defined; undifferentiated post-init keyed by raw tag string is rejected.
- msgspec struct members use `copy.replace` or `msgspec.structs.replace` — struct replacement is sole functional update path with `__post_init__` replay when load-bearing.
- Pydantic frozen members use `model_copy(update=...)` with `deep=True` when nested sub-owners require isolation — shallow copy aliasing nested mutable-adjacent caches fails nested sub-owner isolation contract tests; `copy.replace` on pydantic models accepts Python field names only, not `Field(alias=...)` keys.
- Validated rebuild row invokes arm-specific smart-constructor helper accepting narrowed arm plus delta — not root `TypeAdapter(Member)` on wire dict extracted from member dump unless `RE_INGRESS_ROWS` entry explicitly permits dump-then-validate path.
- Transition must not bypass construction validators on ingress-shaped deltas smuggled through `model_construct` — `PATCH_ADMISSION_ROWS` govern dict deltas at ingress patch boundaries; endofold rows reference patch policy ids instead of inventing parallel validation shortcuts.
- Endofold output is fresh `Member` instance — in-place field mutation on materialized owner breaks structural sharing identity law and invalidates `beartype` admission proofs on registry lookup entrypoints.

# Registry Transition Rows At Composition Root

- Endofold behavior slots live beside ingress registry rows as `TransitionRow` frozen records keyed by `(from_kind, to_kind)` or product key — same vocabulary owner supplies `Kind` tokens; transition registry width is independent of ingress handler registry width.
- Lookup accepts materialized `Member`, narrows with exhaustive `match` to source arm, reads `from_kind` from narrowed arm, resolves row by target `to_kind` requested by caller — lookup does not accept raw `Kind` pair from untrusted ingress without prior member proof.
- Row handler signature is `Callable[[Owner.ArmFrom], Result[Owner.ArmTo, InvariantFault]]` when table fixes both kinds — erasing to `Callable[[Member], Member]` inside row when keys already fix arms is merge blocker.
- Forbidden transition at lookup returns `InvariantFault` before arm handler executes — row absence is not silent no-op; registry transition lookup and ingress registry lookup share root import graph but distinct table symbols.
- Transition registry materializes once at root import from frozen `TRANSITION_ROWS: Mapping[tuple[Kind, Kind], TransitionRow]` — mutable dict populated by decorator side effects at import time is rejected; parity tests iterate `Kind × Kind` candidate pairs against declared row keys per policy closure.

# Fold Algebra And Endofunctor Witness

- Endofold composition `f ∘ g` applies only when `g.to_kind == f.from_kind` — checker or runtime witness on composed kinds; illegal compose returns `InvariantFault` at compose gate, not partial application silently dropping hop.
- Endofold is endofunctor on materialized `Member` category only after migration completes — domain modules type transitions as `Member -> Result[Member, InvariantFault]` not `Union[V1,V2] -> ...`.
- `map` over `Result[Member, E]` after transition preserves error rail — transition faults stay on `InvariantFault` family unless row declares recoverable sub-variant promoted at boundary.
- Optional target kind parameters use `Option[Kind]` or dedicated request struct — caller supplies desired `to_kind`; lookup validates pair against table; do not overload transition entry with bare `Member` destination guess from payload shape alone.
- Identity endofold exists only when table declares `(k, k)` row — default absence of row means identity is not admissible transition request; enrichment-only slot updates route through replacement axis without `(k, k)` transition lookup.

# Semi-Closed Extension And Unknown Arm Transitions

- Transitions from closed arm to `Extension` require explicit row and root-documented capture policy — unknown foreign discriminant materialization is re-ingress or extension admission, not silent endofold from closed arm.
- Transitions from `Extension` to closed arm require adapter fold promoting typed payload through sub-owner or top-level smart constructor — promotion is contravariant admission, not covariant endofold row unless row declares validated rebuild from extension payload with `re_ingress_cross_ref`.
- `Extension` to `Extension` modality change preserves outer extension shell when only payload overlay changes — row uses replace on payload slot; inner closed cores inside extension payload follow sub-owner re-ingress tables.
- Forbidden `(Extension, ClosedArm)` without promotion fold returns `InvariantFault` — extension arm does not bypass sub-owner `forbid_unknown_fields` during transition.

# Version Literal And Deprecation Interaction

- Current-vocabulary endofold tables ignore stored version literals except when row metadata binds `(from_kind, to_kind)` to minimum `schema_version` — version-scoped rows live in migration lattice until member carries only current alias at materialization exit.
- Deprecation arms remain transition sources until wire sunset — rows from deprecated `from_kind` may route to replacement `to_kind` in one hop; dual rows `(deprecated, new)` and `(deprecated, deprecated)` document coexistence policy during migration window.
- Adding transition row is non-breaking when pair was previously forbidden only by absence — adding arm to union is breaking for exhaustive consumers separately; transition table extension does not bypass fold arm updates.
- Obsolete kinds drop from endofold table only after migration fold removes them from stored vocabulary — table parity tests align with current `Kind` enum, not historical stored tags.

# Rich Owner Arm-Aware Transition

- Rich owner exposing `Member` as nested field routes endofold through owner method returning `Result[Owner, E]` or `Result[Member, E]` per graduation policy — exterior API stays one owner type; interior union replace does not leak as public per-arm mutators.
- Owner caches and registries invalidate or rebuild on transition rows declaring `cache_invalidate=true` in metadata — rows without flag assume immutable replace preserves cache keys derived from discriminant-only identity.
- Hierarchical rich owner composes outer owner transition with inner sub-owner transition in one owner method when product row declares atomic bundle — partial outer success with failed inner transition rolls back via fresh owner reconstruction, not in-place partial mutation.
- Smart constructor on rich owner after transition re-validates cross-field law when row touches sub-owner fields implicated by root guards — law runs once in owner method, not duplicated in row handler and owner constructor.

# Morphism Lift And Boundary Separation

- Boundary morphism `MemberA -> MemberB` between distinct families stays at adapter seam — endofold tables never translate parallel families; inter-family adapters are orthogonal to endofold keys on single `Member` alias.
- Container lift `(list[Member], to_kind)` maps element-wise transition with fail-fast on first `InvariantFault` unless batch policy row declares partial success accumulator — default is total batch success or single fault.
- Projection after transition composes covariantly — `project(transition(m))` equals transition arm-aware projection only when row declares projection commute metadata; otherwise project after materialized member returned.
- Wire egress after transition runs once on successor member — do not emit wire bytes from pre-transition member after row claims modality change; egress registry reads successor discriminant.

# Proof Obligations And CI Gates

- Static: exhaustiveness on transition lookup `match` arms mirrors ingress registry — impossible `(from_kind, to_kind)` after narrow hits `assert_never` in default arm of lookup fold.
- Table parity: declared row keys bijection with policy-permitted pairs subset of `Kind × Kind` — forbidden pairs absent from table must fail contract tests asserting `InvariantFault`, not accidental success.
- Transition parity tests parametrize every declared row with sample `from` arm value and assert successor `to_kind` and slot law — hierarchical product rows add samples only for cross-field guards implicated by metadata.
- Mutation: Stryker on transition lookup must kill mutants defaulting forbidden pairs to identity or preferred arm — kill depends on parity tests and `InvariantFault` assertions, not silent wrong-kind success.
- Runtime: `beartype` on transition entrypoints validates live source member arm matches `from_kind` implied by exhaustive narrow before row handler runs — tampered discriminant fields caught after static proof gaps.
- Property: compose associativity holds on lawful triples when middle `to_kind` matches next `from_kind` — generate lawful chains from sparse table graph, not dense `Kind × Kind` explosion.

# Critical Signals On Transition Drift

- Parallel string `if member.kind ==` transition chains in domain modules — collapse to root `transition(member, to_kind)` lookup after materialization.
- Ingress re-validation loop on materialized member via `TypeAdapter.validate_python(model_dump())` — collapse to replace row or `RE_INGRESS_ROWS` entry when full carrier replay is required.
- Identity-return fallback on forbidden `(from_kind, to_kind)` — collapse to `InvariantFault` naming both enum members.
- Duplicated inner and outer transition logic in one handler without product row or sub-owner table — collapse to two-stage lookup mirroring nested `match`.
- Transition row invoking ingress registry handler — behavior dispatch and modality change are separate axes; transition returns successor, then optional registry lookup runs on successor.
- Same-arm enrichment registered as `(k, k)` transition row when replacement axis already owns slot law — duplicate routing surfaces drift between enrichment and endofold tables.
- `PATCH_ADMISSION_ROWS` bypass via `model_construct` on ingress-shaped delta inside endofold handler — patch policy id must appear on row metadata.
- Post-transition `cast(NewArm, member)` after row handler — successor instance from row carries target arm type; `TypeIs` on successor narrows without cast.

# Anti-Patterns

- Calling root `TypeAdapter.validate_python(member.model_dump())` for in-process kind change — re-ingress disguised as endofold; use replace row or documented re-ingress row.
- `if member.kind == "a": return make_b(...)` string transition in domain modules — collapse to table lookup after materialization or add row to endofold registry at root.
- In-place `member.field = x` on frozen or dataclass materialized members — breaks identity and sharing law; always fresh successor instance.
- Dense `Kind × Kind` table declaring all pairs with no-op handlers — sparse law requires absence means forbidden; no-op grid hides missing `InvariantFault` law.
- Flat `"outer_from.outer_to.inner_from.inner_to"` transition keys — use product-keyed rows or nested sub-owner endofold tables.
- Transition row invoking ingress registry handler — behavior dispatch and modality change are separate axes; transition replaces member, then optional registry lookup runs on successor.
- Migration fold invoked mid-domain fold on live `Member` — migration belongs at read boundary only; endofold serves current alias transitions.
- Post-transition `cast(NewArm, member)` — use successor instance from row handler typed as target arm; `TypeIs` on successor narrows without cast.

# Collapse Tests

- Collapse string `if/elif` transition chains in handlers to root transition registry lookup with sparse `(from_kind, to_kind)` table and `InvariantFault` on absence.
- Collapse ingress re-validation loops on materialized members to replace or validated-rebuild rows with explicit `re_ingress_cross_ref` when full carrier replay is required.
- Collapse identity-return fallbacks on forbidden transitions to typed `InvariantFault` constructors naming both kinds.
- Collapse duplicated inner and outer transition logic in one handler to product row or nested sub-owner table with two-stage lookup.
- Done when every legal modality change on current `Member` maps to exactly one transition row, every forbidden pair fails closed with `InvariantFault`, hierarchical transitions use product keys or sub-owner tables, and CI parity tests cover all declared rows before behavioral suites.

# Transition Fault Routing And Boundary Envelope

- `InvariantFault` on forbidden `(from_kind, to_kind)` carries both enum members, optional field path prefix, and owning vocabulary name — fault constructors mirror ingress `UnknownDiscriminant` shape so root envelope distillation maps transition faults into same automation namespace as boundary validation faults.
- Recoverable transition faults — quota retry, deferred modality — stay on port-local `@tagged_union` until promotion policy requires shared domain error family; endofold rows default terminal `InvariantFault` unless row metadata declares recoverable sub-variant with explicit promotion id.
- Root `Envelope` distillation on transition failure preserves `from_kind` and requested `to_kind` tokens when automation consumers need modality context — truncation drops verbose slot diffs, not kind identity fields required for replay attribution.
- Cross-field guard failure during transition returns fault scoped to guard id from root smart constructor — sub-owner local validation faults nest path under sub-field prefix; outer transition row does not swallow inner `ValidationFault` into generic `InvariantFault` without evidence transfer.
- Registry lookup on successor member after successful transition follows ingress registry table independently — transition fault and registry fault are sequential rails; success on transition does not imply registry row exists for successor arm.

# Composition Root Transition Wiring

- Root imports frozen `TRANSITION_ROWS` beside ingress registry — transition lookup entrypoint `transition(member, to_kind)` publishes at root only; domain modules import `Kind` tokens, not transition tables.
- Callable transition API accepts materialized `Member` plus target `Kind` — raw wire dict plus desired tag string stops at root gate; `to_kind` from untrusted carrier without member narrow is rejected before table lookup.
- Root endofold entrypoint executes exhaustive source `match`, extracts `from_kind`, resolves row, delegates to arm-narrowed handler — same structural pattern as ingress registry lookup without accepting pre-materialization tags.
- Chained pipelines compose transition after enrichment and before wire egress when modality change is outbound policy — ordering row ids reference materialization handoff row registry; transition inserted mid-pipeline without row ordering metadata is merge blocker.
- Catalog `Bind` rows embedding transition-capable handlers pin signatures to `Member` input and document allowed target kinds in row metadata — surplus CLI tokens selecting illegal target kind fail at root guard before transition lookup.

# Option Block And Collection Transition Rows

- `Option[Member]` transition applies only on `Some(member)` — `Nothing` arm returns typed absence fault or skips transition per policy row; optional source does not coerce to default arm for transition purposes.
- `Block[Member]` homogeneous transition requires every element share same `from_kind` when batch row declares uniform modality shift — mixed-arm blocks fail with `InvariantFault` naming first offending element index unless batch row declares per-element sparse lookup.
- Filtered narrowed collections after element-wise `TypeIs` may transition under row keyed on proved arm — filter-then-transition without proof is same defect as filter-then-cast on variant collections.
- Transition on `Map[Kind, Member]` values transitions values only, not keys — key set is registry vocabulary; mutating map keys via endofold conflates enum closure with payload transition and is rejected.
- Serialization after batch transition encodes successor members per element vocabulary table — collection egress does not reuse pre-transition wire bytes.

# Hypothesis And Property Targets For Transitions

- Strategies build source arm from registry exemplar per `from_kind` row — target `to_kind` sampled only from declared row keys with matching `from_kind`; forbidden pairs are not generated as negative targets in property tests dedicated to success paths.
- `@given` transition properties assert one law per module: row totality on declared pairs, `InvariantFault` on forbidden pairs, compose associativity on lawful triples, or successor discriminant equality — do not merge transition, registry, and egress into one mega-property.
- Shrinking on transition failures rebuilds lawful source arms — illegal kind mutations during shrink fail at construction gate, not as accepted counterexamples.
- Metamorphic proof: transition then project then wire-decode-smart-construct equals project-then-transition only when row metadata declares commute flag — default tests treat order as significant unless row explicitly documents symmetry.

# Vocabulary Table And Contract Proof Integration

- Transition rows reference `Kind` members from vocabulary table — no parallel string tokens for `from_kind` or `to_kind`; adding transition row does not add vocabulary row unless new arm introduced simultaneously in same root promotion unit.
- Contract tests join transition row keys against vocabulary table `Kind` iteration — orphan transition kinds or kinds without any inbound or outbound row when policy expects connectivity fail import parity gate.
- Wire egress vocabulary after transition uses successor arm row from vocabulary table — msgspec tag on successor must match table parity for `to_kind`; transition tests include round-trip on successor arm, not only source arm.
- OpenAPI and schema publication unaffected by endofold tables directly — public schema reflects ingress union; transition lattice is operational metadata beside owner unless API documents allowed modality changes as enum on separate operation schema.
- Stryker jointly scores transition lookup and smart-constructor arms when mutants redirect transition to bypass construction gate — kill ratio depends on cross-linked parity tests imported from same root module graph.

# Transition Row Catalog Examples

- `card_to_bank`: `from_kind=CARD`, `to_kind=BANK`, `replace_policy=owner_method`, `proof_required=true` when nested sub-owners present — owner method validates cross-field guard `method_rail_compatible` before successor construction.
- `pending_to_settled`: same outer kind, slot-only mutation — equal `from_kind` and `to_kind` with `replace_policy=dataclass_replace` and metadata documenting status slot change without arm change; omitted from table when status change routes through enrichment not transition lattice.
- `payment_product_key`: `(outer_from=PAYMENT, outer_to=PAYMENT, inner_from=CARD, inner_to=WALLET)` — product row on hierarchical member; cross-field guard references root admit id `wallet_rail_allowed`.
- `extension_promote_card`: `from_kind=EXTENSION`, `to_kind=CARD`, `replace_policy=validated_rebuild`, `re_ingress_cross_ref=extension_payload_promote` — contravariant admission path, not covariant skip of sub-owner validators.
- `forbidden_cash_to_crypto`: absent row — contract test asserts `transition(cash_member, CRYPTO)` returns `InvariantFault` with both kinds; no default arm in lookup fold.

