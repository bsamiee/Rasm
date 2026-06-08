# Promotion Gate Admission Calculus

# Calculus Charter

- Promotion gate admission is the sole lawful morphism from static payload compatibility to materialized domain owners — one composed expression per ingress route at the composition root; interior modules receive owners or frozen extension snapshots, never assignable payload evidence.
- Admission calculus owns the gate expression algebra: validate-prefix grade, discriminant `match`, owner constructor selection, extension fold morphism, fault rail, and egress demotion boundary — not payload openness law (contract payload doctrine), triage artifact choice (payload owner adapter triage), reader emission rows (contract reader emission lattice), or staging persistence mechanics.
- Every admission path binds one frozen `GateAdmissionRow`, one emitted contract row from contract reader emission lattice, and one triage outcome from payload owner adapter triage — gate expressions without row authority, triage row, or `gate_expression_ref` are incomplete integration surfaces.
- Payload law stops at gate exit; domain folds, variant family dispatch, and replacement semantics begin on the promoted owner — re-entry to dict-shaped evidence after admission is projection-only at egress demotion boundary.

# Admission Morphism Algebra

- Canonical admission morphism chain: `promote = fold ∘ construct ∘ match ∘ validate ∘ decode` — each factor is a total or fault-short-circuiting morphism on one ingress route; interior modules never host factors after `construct` without documented `re_ingress_boundary` row from metamorphic chain witness catalog lattice.
- Composition is left-to-right on carriers: wire bytes or mapping material enters `decode`; validated payload enters `match`; matched arm product enters `construct`; constructed owner enters optional `fold` when `extra_items` or keyword tail survives — `fold ∘ construct` without declared `extension_fold_target` column from contract reader emission lattice is illegal composition.
- Identity at gate exit: `id_Owner` on materialized owners — domain transforms accept `Owner` parameters only; threading validated payload assignability past `construct` violates homomorphism `π(promote(p)) = Owner` where `π` projects to interior API types.
- Associativity on legal triples: `(fold ∘ construct) ∘ match = fold ∘ (construct ∘ match)` when extension capture binds in the same expression as construction — splitting `construct` and `fold` across modules without explicit adapter sub-row breaks associativity and fails metamorphic chain attribution.
- Fault short-circuit: any `ValidationError`, smart-constructor `Result` failure, or discriminant exhaustiveness gap aborts subsequent factors on that binding — failed staging payloads discard at seam; they do not persist in registries, caches, or context variables.
- Patch admission morphisms compose on replacement axis: `replace = construct_patch ∘ validate_patch` then `replace(owner, Δ)` — patch payloads never apply inside domain folds without replacement owner target; `replace ∘ promote` on creation routes is rejected composition.
- Trusted replay morphisms pin `validate` identity: `promote_replay = fold ∘ construct ∘ match ∘ validate_trusted` where `validate_trusted` references the same `TypeAdapter` module and concrete specialization as live ingress beside pinned `schema_version` — shortcut `construct ∘ decode` without `validate` is trust defect, not performance optimization.

# Gate Admission Row Shape

- Every admitted ingress route declares one frozen row: `gate_id`, `ingress_route`, `validate_grade`, `payload_owner`, `contract_row_ref`, `triage_row_ref`, `discriminant_key`, `match_arms`, `constructor_ref`, `extension_fold_target`, `fault_rail`, `trust_posture`, `negative_fixture_ids`.
- `gate_id` is a stable snake slug — `http_json_promote`, `cli_kwargs_promote`, `event_envelope_promote`, `staging_result_promote`, `patch_replace_promote` — not ordinal stage numbers alone; stage role tags annotate rows but do not replace ids.
- `validate_grade` draws from `{UNTRUSTED, TRUSTED_REPLAY, LITERAL, KEYWORD}` — grade mismatch against ingress carrier is attributed before owner constructor defects; closed payload validated with open posture fails at row bind time.
- `payload_owner` and `constructor_ref` are closed type expressions from triage payload owner adapter triage — erased `dict[str, Any]` or comment-contract carriers fail row admission.
- `match_arms` is a frozenset of `(discriminant_literal, body_payload_ref, owner_kind)` triples — cardinality must equal vocabulary row discriminant count from contract reader emission lattice; orphan arms and missing arms both fail checker exhaustiveness before runtime suites.
- `extension_fold_target` names `frozendict[str, V]`, tuple snapshot, or dedicated frozen field on owner — `None` when payload is `closed=True` without keyword tail; mutable extension dict survival into domain parameters is row breach.
- `fault_rail` names typed fault owner family and adapter-owned step labels — bare `ValidationError` dumps reaching interior modules invalidate row.
- `trust_posture` labels `{untrusted, trusted_replay, literal_fixture}` — selects whether full `validate` runs before `match`; trusted replay rows pin adapter identity beside `schema_version`.
- `negative_fixture_ids` index harness anti-patterns each row must reject — `isinstance_promotion_proof`, `catch_all_match_arm`, `kwargs_past_gate`, `extension_dict_in_domain_param`, `interior_type_adapter_payload`.

# Admission Composition Table

- `ADMISSION_COMPOSE_TABLE: frozendict[tuple[GateFactor, GateFactor], ComposeFn | REJECT]` is total over legal adjacent pairs on one `gate_id` — illegal pairs surface at composition-root import, not as integration test gaps.
- Legal pairs: `(decode, validate)`, `(validate, match)`, `(match, construct)`, `(construct, fold)`, `(validate_patch, construct_patch)` on patch rows only — `(construct, validate)` and `(match, decode)` are `REJECT`.
- Interior second `validate` on promoted owner requires `re_ingress_boundary` row in metamorphic chain witness catalog lattice — default interior API forbids `(construct, validate)` on same route without boundary row.
- `(fold, match)` and `(construct, match)` are `REJECT` — extension bands fold only after owner kind is fixed by discriminant arm.
- Double promotion `(promote, promote)` is zero morphism — event dict promoted at handler entry must not re-promote in dispatch folds; shortcut is explicit `REJECT` row.
- Row presence is import gate — bounded contexts without `GateAdmissionRow` per ingress route fail registry build before hypothesis properties execute.

# Gate Expression Shape

- Canonical admission form: `owner = promote(validated)` where `validated` is `TypeAdapter(Payload).validate_python(material)` or an ingress-model product immediately upstream — not raw `dict[str, object]` threaded from interior modules.
- Single-expression law: decode, validate, discriminant match, construct, and optional extension fold compose in one root binding — split validate-then-promote across modules violates admission unless each split is an explicit adapter sub-row documented at root.
- Promotion never runs inside boundary adapters that only forward opaque mapping material — adapters terminate on validated payload or ingress model when promotion is deferred; adapters that stop on raw dict when static contract exists fail triage from payload owner adapter triage.
- Staging completion admission always produces a materialized owner — `Result[Owner, E]` or tagged union construction; validated dict assignable to staging payload is not a durable handoff.
- Patch admission hands off to the replacement axis in the same expression family — `replace(owner, patch_payload)` or `model_validate(snapshot | delta)` — patch payloads never apply inside domain folds without replacement owner target.

# Validate Prefix Grade Lattice

- Validate prefix grades form a finite assignment lattice on `GateAdmissionRow.validate_grade` — each ingress route binds exactly one grade; grade selects which `validate` morphism may precede `match` on that route.
- **UNTRUSTED** — wire, CLI, provider, and live ingress paths require `TypeAdapter(Payload).validate_python` or ingress-model validate with `ConfigDict(extra="forbid")` on closed envelopes before any `match` arm executes; default grade when `trust_posture=untrusted`.
- **TRUSTED_REPLAY** — same boundary artifact class and specialization as live ingress pinned beside `schema_version`; validate expression identity matches live row from contract reader emission lattice; `validate_trusted` morphism references pinned adapter module — shortcut to `construct` without `validate` is trust defect.
- **LITERAL** — promotion tests and metamorphic chains use explicit payload literals or adapter-validated dicts — not `isinstance(x, Payload)`; construction rejection tests prove call sites cannot inject extra keys into closed literals; `trust_posture=literal_fixture`.
- **KEYWORD** — root entrypoints typed `def op(**kwargs: Unpack[Payload])` unpack inside the adapter in the same expression that validates or promotes — `**kwargs` does not thread past the first validate/promote binding; `**tail` fold binds to `extension_fold_target` when `extra_items` present.
- Grade dominance: `UNTRUSTED` strictest on foreign ingress — replay rows may downgrade only through explicit `TRUSTED_REPLAY` pin, never through interior bypass; `LITERAL` and `KEYWORD` are proof harness grades, not production ingress substitutes for `UNTRUSTED`.
- Grade mismatch defects — closed payload validated with open posture, replay into different specialization, keyword tail forwarded without `extra_items` fold — attribute to adapter binding and `validate_grade` row before owner constructor defects.

# Discriminant Match Algebra

- Semi-closed variant families admit through two-stage match: envelope discriminant first, body payload type bound per arm, owner constructor per `(kind, body)` pair — never mutate envelope dict to attach body fields.
- `match payload:` on discriminant key typed as `Literal` or closed vocabulary enum — bare string runtime checks without literal evidence fail static admission before runtime suites execute.
- Each finite vocabulary row owns one `match` arm constructing the expected owner kind — missing arms fail checker exhaustiveness and contract table discriminant parity from contract reader emission lattice before behavioral tests run.
- `assert_never` witnesses bind at promotion gate only — excluded from branch-coverage targets; exhaustiveness is proven by type checkers and discriminant table parity, not runtime default arms.
- Open extension arms on ingress or staging payloads use `match` with `**extensions` capture only after known-key shape and `extra_items` type are declared — fold captured extensions into `frozendict` or tuple snapshots in the same admission expression.
- Illegal FSM transitions at ingress emit typed fault owners with enum-member labels — vocabulary axis owns legal-event set; gate carries evidence fields only; bare exceptions at discriminant mismatch are rejected.

# Extension Fold Morphisms

- Extension bands from `extra_items=T` fold once at promotion into immutable owner evidence — `frozendict[str, V]`, tuple snapshots, or dedicated frozen fields — not scratch dicts flowing through family dispatch.
- Fold target metadata from emitted row `extension_fold_target` column names the snapshot shape hypothesis and promotion tests assert — mutable extension dicts surviving into domain transform parameters fail admission proof.
- Tuple or `frozendict` snapshots are promotion products, not alternate permanent models — snapshot at gate and thread through domain logic as part of owner or dedicated immutable field.
- Keyword-tail call sites collect `**tail` into frozen snapshot at adapter exit when tail must survive past call boundary — same fold law as `**extensions` capture on mapping payloads.
- Narrowing fold: extension values must satisfy `T` or `ReadOnly[T]` bounds from row `extra_items_type` — unbounded `object` erosion at fold site is a generic specialization defect caught at emission from contract reader emission lattice.

# Owner Constructor Selection

- Constructor choice follows triage Step 4 from payload owner adapter triage: smart constructors return `Result[Owner, E]`; tagged unions require interior `match` after promotion; Pydantic `model_validate`, msgspec struct construction, and dataclass factories bind per triage row — not ad hoc `**dict` splats on untrusted material.
- Cross-field invariants, computed slots, and enrichment belong on the constructed owner — admission expression does not re-validate fields the owner constructor already owns unless ingress model and canonical owner diverge by documented adapter row.
- Variant family owners materialize before family dispatch runs — payload seams supply static discriminant evidence only; family logic never reads raw staging dicts post-gate.
- Absence semantics on promoted owners use vocabulary sentinels or `Option[T]` — payload `NotRequired` alone does not survive past gate as domain optional semantics.
- Nested partial updates assemble nested validate inputs explicitly until polymorphic nested replace is admitted on replacement axis — shallow `model_copy(update={"nested": {...}})` without typed nested validation is rejected when nested slots are materialized models.

# Fault And Result Rails At Gate

- Validation failures at boundary map to typed fault owners with adapter-owned step names — domain modules do not format wire-key error strings from raw `ValidationError` dumps.
- Smart constructor `Result[Owner, E]` rails bind at gate exit — failed staging payloads discard at seam; they do not persist in registries, caches, or context variables.
- Assignability failures after closed adapter decode indicate wire drift or missing `closed=True` on boundary declaration — diagnostics name payload owner and forbidden key, not cue to widen contract.
- Construction failures on typed literals indicate call sites building dicts by hand — route through validation or typed constructor paths; do not relax openness to absorb defect.
- Discriminant exhaustiveness gaps are binding defects surfaced in checker matrix and contract table parity before runtime promotion tests — missing `match` arms are not integration test gaps.

# Egress Demotion Boundary

- Demotion for wire egress is projection from canonical owner — not mutation of domain instance into dict seam; egress TypedDict types assignability targets for dict literals built in adapter only.
- Interior modules emit canonical or wire struct owners — not egress payload types; partial egress uses dedicated closed subsets when requiredness matches wire contract.
- Field renames for wire format happen in adapter projection — not via duplicate payload names; `msgspec.convert` or field-explicit struct construction owns bytes path after owner projection.
- Egress assignability proof builds dict literals from canonical projection, asserts compatibility against egress row from contract reader emission lattice, then validates through `TypeAdapter` when wire compliance is closed.

# Metamorphic Chain Binding

- Root metamorphic chain: lawful payload dict → validate → promote → project egress → validate assignability — each hop cites `gate_id` and `gate_expression_ref` from bound `GateAdmissionRow`; chain rows without gate reference collapse to admission calculus sections above.
- Chain failures attribute to adapter binding when contract reader emission lattice row columns are current — promotion hop failures implicate `match_arms`, `constructor_ref`, or `extension_fold_target` on the gate row before domain owner defects.
- Chain consumes emitted rows as field authority — drift negatives reference row `closed` and `discriminant_key`; forbidden keys generated from row metadata, not hand-authored prose lists.
- Harness layer ordering: static checker matrix before contract tables before hypothesis properties before root round-trip — admission failures at static layer block expensive generative runs.
- Round-trip proof encodes promoted owners and decodes through boundary adapters when polymorphic interiors cross process boundaries — payload validation alone does not substitute for promoted-owner proof.
- beartype-decorated root entrypoints taking materialized owners receive mutation probes on live instances — runtime admission catches post-promotion field tampering static payload proof cannot see.

# Callable Seam Admission

- Keyword-callable hooks typed `Callable[[Unpack[Payload]], R]` must survive decorator stacks to gate — wrappers use PEP 695 `ParamSpec` preservation and `@wraps` so inner forwards without erasing `Unpack`.
- `Concatenate[Context, P]` prepends aspect context at seam — tracing, auth, admission metadata — without redeclaring payload keys on each wrapper layer.
- Decorators must not replace static contracts with runtime dict repair, `get` defaults for missing keys, or `extra="allow"` on closed surfaces — decorator altitude belongs on decorator-admitted-shape axis.
- Callable payload seams prove signature preservation: collected root handlers expose `Unpack[Payload]` in `inspect.signature` after `inspect.unwrap` exhaustion — ParamSpec erasure fails contract suite before integration tests execute.
- `Unpack[Payload, extra_items=T]` at callable boundary equivalent to required known keywords plus `**tail: T` — unknown keywords at call sites are checker errors when payload is closed; otherwise must satisfy `extra_items`.

# Dual-Surface And Settings Admission

- Dual CLI and HTTP ingress for one concept converge through one admission graph per triage row — canonical equality proof binds in same change unit when either surface moves.
- Settings `validation_alias` tables map once in adapter — alias-key ingress dicts validate into boundary payloads before promotion in same expression as HTTP dict ingress.
- Sentinel-parameter defaults on CLI entrypoints pair with `*` boundaries — `NotRequired` payload keys align with cyclopts `Parameter` omission semantics documented in adapter module.
- Staging payloads for settings and CLI remain at root until decorator-admitted single-owner projection lands — cross-path canonical equality is merge gate.

# Proof Obligations Per Admission Path

- Each `GateAdmissionRow` owns a proof obligation bundle — static `match_arms` parity, runtime promotion test, and optional metamorphic chain hop; missing bundle on any active `gate_id` fails registry build.
- Direct promote: promotion test from explicit payload literal or adapter-validated dict; owner kind matches triage row; extension fold snapshot shape when `extra_items` present.
- Discriminant promote: every vocabulary arm constructs expected owner through root gate expression; `assert_never` witnesses green; envelope-body row link from contract reader emission lattice satisfied.
- Staging promote: smart constructor `Result` rail tests; failed staging discarded; no payload import in interior after success.
- Patch promote: replace proof asserts omitted `NotRequired` keys leave nested subgraphs untouched; read-only keys reject update attempts at validation exit.
- Keyword promote: root signature preserves `Unpack`; lawful keyword sets only in property tests; missing required keys are negative cases.
- Replay promote: same validate expression and specialization as live ingress beside pinned `schema_version`.
- Egress demote: assignability to egress row then optional `TypeAdapter` closed compliance.

# Collapse Signals At Gate

- Domain fold parameter typed as boundary payload — missed promotion; collapse to owner admission per payload owner adapter triage.
- Validated dict threaded into interior after staging should have promoted — collapse to single root expression.
- Extension dict flowing through family dispatch — collapse to gate fold into frozen snapshot on owner.
- Event or staging dict mutated in place after admission — collapse to read-only evidence and one-shot promotion at handler entry.
- Patch payload reaching domain fold without replacement owner — collapse to replacement-axis expression.
- Interior `TypeAdapter(Payload)` — boundary leak; collapse adapter ownership to root.
- Promotion inside adapter that only forwards opaque maps — collapse validate termination or full promote in root binding.

# Evolution Binding At Gate

- Adding discriminant arm binds vocabulary row, envelope `match` arms, promotion constructors, contract table discriminant row, and hypothesis registry slice in one promotion unit — orphan arms fail CI before behavioral suites.
- Payload key addition at gate-visible boundary updates owner declaration, emitted row, adapter field map, gate `match` arms, and promotion tests in one unit — partial gate evolution is merge blocker.
- Deprecation retires promotion arms only after migration proves zero ingress dependency — retired discriminants remain in migration modules with `assert_never` witnesses, not in active `match` blocks.
- Generic specialization changes at gate require paired updates to reader frozenset emission, `TypeAdapter` concrete argument rows, and promotion tests per concrete argument — partial retarget fails matrix before runtime suites.

# Rejection Shortcuts

- `isinstance(x, Payload)` as promotion proof — use explicit literals or adapter-validated dicts.
- Default catch-all `match` arm on finite discriminant vocabulary — exhaustiveness defect.
- Promotion deferred so deeply domain code operates on raw dict with comment contracts — gate misplaced.
- Premature promotion inside adapter forwarding opaque mapping material — validate or defer per triage.
- Mutable staging dict retained after failed promotion or in context variables — seam discard law violated.
- ParamSpec erasure after decorator application — fix wrapper stack before widening payload requiredness.
- Skipping extension-band fold because payloads assign cleanly — fold proof mandatory at gate.
- Trusted replay validating into different boundary artifact than live ingress — replay row pin violated.

# Event And Handler Admission

- Event consumption treats payload as evidence with `ReadOnly` on fields handlers must not rewrite — promotion to domain facts happens once at handler entry, not by mutating event dict in place.
- Handler entry binds one gate expression per event route: validate envelope → match discriminant → construct domain fact owner → dispatch fold — event dict does not pass into interior dispatch parameters.
- Envelope-plus-body admission uses linked rows from contract reader emission lattice: envelope row supplies identity and occurrence fields; body row per arm supplies variant-specific required keys — gate never merges envelope and body into one dict mutation step.
- `StrEnum` members carrying non-member payloads prove through explicit seam tests at gate — static typing relies on typing-spec nonmember rules; runtime tests assert member construction preserves payload fields before promotion arm executes.
- Fault payloads at illegal FSM transitions carry enum-member labels in typed fault owners — seam tests feed out-of-vocabulary discriminants and assert fault discriminant vocabulary at gate, not bare exceptions.

# Staging Phase Transitions

- One staged payload per construction phase — staging payloads remain read-only evidence where later steps must not rewrite earlier evidence; phase advance re-validates or promotes, never mutates prior staging dict.
- Smart constructors accept partial construction payloads and return `Result[Owner, E]` or `Option[Owner]` at phase boundary — abandoned staging payloads discard at seam without registry persistence.
- Phase routers discriminate on payload type or explicit stage discriminant — never on keys-present heuristics; creation and patch payloads are different seam artifacts even when key names overlap.
- When staging completes, durable handoff is always materialized owner — never validated dict typed as assignable payload evidence threading into interior modules.
- Adapters routing both creation and patch bind separate gate expressions per triage row — shared vocabulary export, distinct admission paths, no unified dict carrier for both semantics.

# Nested And Polymorphic Gate Admission

- Nested partial updates assemble nested validate inputs explicitly at gate — parent owner construction receives validated nested owners, not shallow dict blobs merged by `model_copy(update=...)`.
- Polymorphic nested replace admits only through documented seam expressions with explicit validate assembly until `replace_validated` lands on replacement axis — gate tests include drift keys on nested patch literals rejected before replace reaches domain folds.
- Generic payload owners with `T` in body slots require concrete specialization at gate — promotion tests run per registered concrete argument tuple from composition root, not against unbound generic owners.
- When Pydantic ingress models embed generic payload fields, `model_rebuild()` completes at root first-touch before gate admission on specialized routes — gate expression references rebuilt model, not forward-ref stale graph.
- Closed-key narrowing via `key in payload` composes left-to-right at static seam only — gate validation still gates untrusted ingress; negative `not in` tests do not prove absence for required keys on closed payloads.

# Property Testing And Mutation At Gate

- Hypothesis strategies for promotion paths draw lawful payload dicts from contract row columns — discriminant-conditioned strategies mirror gate `match` structure: outer kind sampled first, body fields conditioned on kind literal.
- Extension-band strategies respect `extra_items_type` bounds — captured `**extensions` folds prove frozen snapshot shapes at promotion, not mutable dict survival into shrink endpoints.
- Shrinking rebuilds lawful payload dicts after shrink steps — invalid extra keys on closed payloads and illegal read-only mutations reject at construction gate, not used as shrink endpoints.
- Stryker mutation on promotion `match` arms requires kill ratio on discriminant routing — mutants defaulting to catch-all or dropping arms must fail exhaustiveness type-check or contract tests before behavioral suites pass.
- Keyword-callable property tests preserve `Unpack` through generated call patterns — strategies invoke root-wrapped handlers with lawful keyword sets only; missing required keys are negative cases, not shrink targets.

# Failure Attribution At Gate

- Owner kind mismatch after promote — discriminant `match` arm or constructor binding defect; check vocabulary row parity with contract reader emission lattice envelope-body links.
- Extension dict in domain transform parameter — fold morphism skipped or `extension_fold_target` metadata ignored at gate.
- Wire drift after closed validate — missing `closed=True` on boundary declaration or adapter correspondence row stale versus emitted row.
- ParamSpec erasure after decorator on keyword route — wrapper dropped `Unpack` before gate; fix decorator stack per callable seam admission section.
- Interior domain import of boundary payload type — promotion gate misplaced upstream; re-triage parameter site to owner per payload owner adapter triage.
- Promotion test passing with `isinstance` — proof method defect; replace with literal or adapter-validated dict admission.
- Replay owner differs from live promote for equivalent material — trusted replay validate prefix grade or specialization pin defect beside `schema_version`.
