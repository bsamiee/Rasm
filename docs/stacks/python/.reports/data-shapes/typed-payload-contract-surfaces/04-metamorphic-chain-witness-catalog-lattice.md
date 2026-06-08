# Metamorphic Chain Witness Catalog Lattice

# Witness Row Shape

- Every admitted ingress or egress route declares one frozen catalog row: `chain_id`, `concept`, `stage_role`, `triage_row_ref`, `emitted_row_ref`, `gate_expression_ref`, `chain_slugs`, `carrier_posture`, `validate_grade`, `demission_grade`, `round_trip_policy`, `correspondence_row_ref`, `negative_fixture_ids`, `ci_gate_ids`.
- `chain_id` is a stable snake slug — `http_ingress_promote`, `cli_keyword_admit`, `event_envelope_body_promote`, `canonical_wire_egress`, `trusted_replay_re_ingress` — not ordinal stage numbers alone; stage names annotate rows but do not replace ids.
- `concept` names the bounded-context vocabulary owner — one `chain_id` per concept per carrier per stage role; duplicate chains for the same concept-stage-carrier tuple without documented fork are lattice defects.
- `stage_role` draws from `{boundary, staging, patch, event, egress, keyword}` — interior transforms do not admit chain rows; owner-only parameters terminate chain registration.
- `triage_row_ref` binds `{concept, stage, artifact_class, owner_module}` from triage outcome — chain rows without triage authority are incomplete integration surfaces.
- `emitted_row_ref` binds `{owner_id, specialization_args?, schema_version?}` from contract reader emission — chain proof consumes row columns as field authority; hand-maintained field lists beside row refs fail admission.
- `gate_expression_ref` names the root admission binding symbol or documented sub-expression id from the promotion gate — split validate-then-promote across modules requires explicit sub-row refs per gate expression shape law.
- `chain_slugs` is a closed tuple drawn from the slug band below — orphan routes without at least one slug fail harness discovery; pytest file paths are not valid slug substitutes.
- `carrier_posture` labels `{bytes, str, dict, keyword, in_process_owner}` — selects decode prefix, `TypeAdapter` path, and whether demission or re-ingress applies at chain tail.
- `validate_grade` copies `{UNTRUSTED, TRUSTED_REPLAY, LITERAL, KEYWORD}` from the promotion gate — mismatch between row grade and adapter binding attributes to adapter before owner constructor defects.
- `demission_grade` is `{none, assignability, closed_validate, struct_bytes}` — `none` when chain terminates on promoted owner; egress chains declare demission grade explicitly.
- `round_trip_policy` is `{none, process_boundary, persistence_bytes, trusted_replay}` — selects whether tail re-ingress chain row must exist and pin same `emitted_row_ref` as live ingress.
- `correspondence_row_ref` indexes anti-corruption field map when foreign layout differs from canonical — empty when wire and canonical keys align; provider remap changes bind correspondence and chain row in one version unit.
- `negative_fixture_ids` index harness anti-patterns each chain must reject — `kwargs_threading`, `interior_type_adapter`, `payload_interior_param`, `mutable_extension_survival`, `dual_json_schema`.
- `ci_gate_ids` bind merge blockers — `checker_matrix`, `row_oracle_parity`, `promotion_exhaustiveness`, `extension_fold_snapshot`, `egress_assignability`, `replay_row_pin` — to representative matrix modules named in row metadata.

# Chain Slug Band

- `lawful-dict-validate` — untrusted mapping material decodes to `dict[str, object]`, validates through `TypeAdapter(Payload)` or ingress model with closed posture when row `closed` column is true.
- `discriminant-promote` — finite vocabulary `match` at gate constructs expected owner kind per arm; `assert_never` witnesses green; envelope-body linked rows satisfied when `stage_role=event`.
- `extension-fold-snapshot` — `**extensions` or keyword tail folds into `frozendict` or tuple snapshot named by row `extension_fold_target` — mutable extension dict survival into domain parameters is indexed negative `mutable_extension_survival`.
- `staging-result-rail` — smart constructor `Result[Owner, E]` at staging boundary; failed staging discarded; no payload import in interior after success.
- `patch-replace-handoff` — patch payload hands to replacement expression in same root family — omitted `NotRequired` keys mean leave unchanged; read-only keys reject update at validation exit.
- `owner-interior-terminate` — chain ends on materialized owner; interior modules receive owner only — continuation as assignable payload is indexed negative `payload_interior_param`.
- `canonical-project-egress` — owner projects to wire struct or closed egress payload in adapter — interior modules do not return egress payload types.
- `egress-assignability` — dict literal built from projection asserts compatibility against egress row `required_keys` frozenset before optional `TypeAdapter` closed compliance.
- `struct-bytes-encode` — `msgspec.convert` or explicit struct construction owns bytes path after owner projection — `json.dumps` surgery at interior sites is indexed negative `dump_surgery_projection`.
- `round-trip-re-ingress` — promoted owner encodes, bytes cross boundary, receiving root decodes through pinned adapter into same boundary artifact class as live ingress beside `schema_version`.
- `keyword-unpack-admit` — root `Unpack[Payload]` unpacks in same expression as validate or promote — `**kwargs` threading past first binding is indexed negative `kwargs_threading`.
- `callable-signature-preserve` — decorated root handlers expose `Unpack[Payload]` in `inspect.signature` after `inspect.unwrap` exhaustion — ParamSpec erasure fails before integration smoke.
- `replay-row-pin` — trusted replay uses identical `emitted_row_ref` and `validate_grade=TRUSTED_REPLAY` as live chain — shortcut to owner without validate is indexed negative `replay_shortcut`.
- `row-oracle-parity` — reader-emitted frozensets agree with `TypeAdapter(Payload).json_schema()` per specialization before metamorphic chain executes — divergence blocks at reader emission, not at chain runtime.
- `hypothesis-row-draw` — generative strategies key off emitted row columns — `st.from_type(Payload)` on closed owners is indexed negative `tableless_hypothesis`.

# Ingress Chain Edge Catalog

- `wire_bytes_to_owner`: carrier `bytes`; slugs `lawful-dict-validate`, `discriminant-promote`, `extension-fold-snapshot`, `owner-interior-terminate`; validate grade `UNTRUSTED`; demission `none`; negatives `interior_type_adapter`, `json_loads_before_adapter`.
- `http_dict_to_owner`: carrier `dict`; slugs `lawful-dict-validate`, `discriminant-promote`, `owner-interior-terminate`; correspondence ref when anti-corruption maps wire aliases; validate grade `UNTRUSTED`.
- `cli_keyword_to_owner`: carrier `keyword`; slugs `keyword-unpack-admit`, `lawful-dict-validate`, `owner-interior-terminate`; validate grade `KEYWORD`; sentinel boundary metadata from row flags `sentinel_boundary` when cyclopts omission aligns.
- `settings_alias_to_owner`: carrier `dict`; slugs `lawful-dict-validate`, `owner-interior-terminate`; correspondence ref for `validation_alias` table; dual-surface canonical equality proof binds with HTTP chain row in same change unit.
- `event_envelope_body_to_fact`: carrier `dict`; slugs `lawful-dict-validate`, `discriminant-promote`, `owner-interior-terminate`; two linked `emitted_row_ref` values for envelope and body arms; handler entry single gate expression — event dict does not pass into interior dispatch.
- `staging_partial_to_owner`: carrier `dict`; slugs `lawful-dict-validate`, `staging-result-rail`, `owner-interior-terminate`; stage role `staging`; abandoned staging discarded at seam without registry persistence.
- `patch_delta_to_replaced_owner`: carrier `dict`; slugs `lawful-dict-validate`, `patch-replace-handoff`; stage role `patch`; chain terminates on replacement owner, not raw dict carrier threading into interior folds.
- `trusted_replay_to_owner`: carrier `bytes` or `dict`; slugs `replay-row-pin`, `lawful-dict-validate`, `discriminant-promote`, `owner-interior-terminate`; validate grade `TRUSTED_REPLAY`; `round_trip_policy=trusted_replay`; `schema_version` column pinned beside chain row.

# Egress Chain Edge Catalog

- `owner_to_closed_egress_dict`: carrier `dict`; slugs `canonical-project-egress`, `egress-assignability`; demission `assignability`; interior emits canonical owner only; egress row `stage_role=egress` types adapter-built literal.
- `owner_to_closed_egress_validate`: carrier `dict`; slugs `canonical-project-egress`, `egress-assignability`, `lawful-dict-validate`; demission `closed_validate`; wire compliance requires `TypeAdapter` on closed egress subset after assignability proof.
- `owner_to_wire_struct_bytes`: carrier `bytes`; slugs `canonical-project-egress`, `struct-bytes-encode`; demission `struct_bytes`; field renames happen in adapter projection — not duplicate payload names.
- `owner_to_partial_wire_subset`: carrier `dict`; slugs `canonical-project-egress`, `egress-assignability`; egress row carries fewer keys than canonical owner — adapter documents omission as wire-optional or computed-on-egress.
- `polymorphic_owner_to_tagged_wire`: carrier `bytes`; slugs `canonical-project-egress`, `struct-bytes-encode`, `discriminant-promote` on egress projection arm selection — interior `match` on owner supplies tag; wire struct carries `tag_field` per msgspec policy from triage.

# Correspondence Row Binding

- Anti-corruption correspondence rows declare `{wire_key, canonical_key, direction, omission_class}` — `direction` is `{ingress, egress, bidirectional}`; `omission_class` is `{wire_optional, domain_default, computed_on_materialize, computed_on_egress}`.
- Correspondence rows live once in adapter module — chain catalog references `correspondence_row_ref` by stable id; scattered `model_dump` key pops duplicated across ingress and egress chains are indexed negative `scattered_rename`.
- Provider overlay keys map through correspondence into `extra_items` extension fold targets — chain slug `extension-fold-snapshot` mandatory when overlay survives past gate.
- Discriminant wire tokens map through vocabulary module — correspondence does not invent parallel enum strings; payload literal, enum member value, and wire string parity proves in discriminant chain rows.
- Ingress field count differing from canonical field count documents each omission in correspondence — payload `NotRequired` mirrors wire optionality; canonical defaults apply at promotion, not by mutating payload dict.

# Round-Trip Witness Rows

- Process-boundary round-trip pairs ingress chain row with tail `round-trip-re-ingress` row — `emitted_row_ref`, `validate_grade`, and `schema_version` must match live ingress row; owner kind equality proves after full decode-promote cycle.
- Persistence-bytes round-trip adds `store_key` and `adapter_module_id` columns on replay rows — identical frozensets in two bounded contexts still require distinct `chain_id` unless shared vocabulary module documents federation.
- In-process handoff between bounded contexts terminates ingress chain on owner or frozen snapshot — payload assignability does not thread through interior packages; separate bounded context admits new ingress chain row at its root.
- Subinterpreter handoff requires bytes plus receiving-root pinned decoder — in-process owner graphs do not cross interpreter boundaries without egress demission chain and re-ingress row at target root.
- Round-trip negative fixtures include drift keys on closed replay material, wrong discriminant literals, and specialization mismatch — generated from row `closed` and `discriminant_key` columns, not hand-authored prose lists.

# Envelope-Body Chain Linking

- Semi-closed variant families register parent ingress chain row plus one child chain row per body arm — parent `emitted_row_ref` names envelope row; child refs name body rows linked by shared `vocabulary_ref` and `discriminant_key` from emitted contract rows.
- Parent chain slugs include `lawful-dict-validate` and `discriminant-promote` only on envelope material — body fields validate in arm-specific gate sub-expression after discriminant binds body payload type.
- `assert_never` witnesses attach to parent chain parametrized module — missing body arm chain rows fail catalog completeness before envelope-only metamorphic tests execute.
- Open extension arms on ingress chains add `extension-fold-snapshot` slug on parent or staging child row — `extension_fold_target` metadata propagates to both envelope and body chain rows when overlay spans both layers.
- Illegal FSM transition chains register separate fault ingress row — out-of-vocabulary discriminants assert fault owner kind through dedicated `chain_id` suffix `_fault_ingress`; fault rows do not share promote slugs with lawful envelope chains.
- Envelope dict mutation to attach body fields mid-chain is indexed negative `envelope_body_mutation` — body admission always constructs fresh owner from bound body payload type at the promotion gate.

# Property Testing Chain Binding

- Hypothesis strategies parametrized per `chain_id` draw lawful dicts from `emitted_row_ref` columns — `required_keys`, `optional_keys`, `read_only_keys`, `closed`, `extra_items_type` drive `st.fixed_dictionaries` composition.
- Discriminant-conditioned chain strategies mirror slug `discriminant-promote` structure — outer kind sampled from `vocabulary_ref` first; body field dicts conditioned on arm-specific body row `required_keys`.
- Extension-band chain strategies read `extra_items_type` for values disjoint from declared key sets — captured tail proves `extension-fold-snapshot` slug produces frozen shape named by `extension_fold_target`.
- Shrinking on chain fixtures rebuilds lawful dicts from row columns after shrink steps — invalid extra keys on `closed=true` rows and illegal read-only mutations reject at construction gate, not as shrink endpoints.
- Negative chain fixtures indexed by `negative_fixture_ids` run as separate parametrized cases — lawful chain tests and drift chain tests never share a single undifferentiated strategy blob.
- Keyword chain strategies invoke root-wrapped handlers with lawful keyword sets only — missing required keys are negative cases under `keyword-unpack-admit` slug, not shrink targets.
- Stryker mutation on chains declaring `discriminant-promote` requires kill ratio on routing mutants — catch-all defaults and dropped arms must fail exhaustiveness or contract table parity before chain runtime suite passes.

# Harness Layer And Gate Binding

- Harness execution order pins static checker matrix before emitted row consumption before hypothesis properties before metamorphic chain runtime — chain catalog rows reference `ci_gate_ids` that block generative runs when static layer fails.
- Each `chain_id` registers one parametrized metamorphic test module — lawful sample drawn from row columns, gate expression executed, owner kind asserted, optional demission assignability or bytes round-trip when `round_trip_policy` demands.
- beartype mutation probes attach to chains terminating on `owner-interior-terminate` — post-promotion field tampering caught at runtime complements static payload proof; chain row metadata flags `mutation_probe=true` when root entrypoint is beartype-decorated.
- Stryker kill ratio on `discriminant-promote` slugs requires mutants dropping `match` arms fail exhaustiveness or contract table parity — chain row `ci_gate_ids` includes `promotion_exhaustiveness` when finite vocabulary applies.
- Callable chains with `callable-signature-preserve` slug run `inspect.signature` proof before metamorphic runtime — ParamSpec erasure fails contract suite before chain parametrized tests execute.

# Staging And Phase Transition Chains

- Multi-phase construction registers one chain row per phase with `stage_role=staging` — phase routers discriminate on payload type or explicit stage discriminant, never on keys-present heuristics.
- Phase advance chains reuse `lawful-dict-validate` and `staging-result-rail` slugs — prior phase staging dict is read-only evidence; phase transition never mutates earlier staging material in place.
- Terminal staging chain adds `owner-interior-terminate` slug when phase completes — durable handoff is always materialized owner; validated dict assignable to staging payload does not terminate chain on dict carrier.
- Creation and patch concepts register separate chain rows even when key names overlap — shared `concept` and `vocabulary_ref` with distinct `stage_role` and slug sets; unified dict carrier for both semantics is indexed negative `create_patch_carrier_merge`.
- Abandoned staging chains assert discard at seam — no registry persistence slug; failed promotion chains do not retain staging payload in context variables or caches.

# Dual-Surface And Federation Rows

- Dual CLI and HTTP ingress for one concept register distinct `chain_id` values sharing one `emitted_row_ref` per stage — canonical equality proof parametrizes both chains in one merge gate; either surface move binds both chain rows.
- Cross-context payload federation shares boundary owner module and vocabulary export — distinct `chain_id` per bounded context with shared `owner_id` on emitted row; interior modules in neither context import sibling context payload types.
- Settings and CLI staging chains remain at root until decorator-admitted single-owner projection lands — federation row metadata flags `dual_surface_staged=true` when cross-path canonical equality is active merge gate.
- Wire-visible evolution on one concept updates ingress chain row, egress chain row, correspondence refs, schema snapshot, and hypothesis registry in one promotion unit — partial chain evolution is merge blocker.

# Collapse Signals At Catalog

- Chain row without `emitted_row_ref` — collapse to contract reader emission before registering metamorphic witness.
- Chain row without `gate_expression_ref` — collapse to promotion gate admission; chain describes proof only after gate exists.
- Hand-maintained metamorphic samples beside row-driven generators — collapse strategies to `hypothesis-row-draw` slug fixtures keyed on frozensets.
- Interior module imports chain catalog as runtime routing authority — category error; catalog is proof and CI policy only; runtime routing lives at composition root expressions.
- Duplicate `chain_id` with divergent `emitted_row_ref` frozensets — collapse duplicate registration passes to single reader emission bind.
- Egress chain returning TypedDict from interior transform — collapse to `canonical-project-egress` adapter ownership at root.

# Failure Attribution At Chain

- Metamorphic failure at validate step — adapter binding or row `closed` posture drift; check correspondence ref freshness versus provider remap.
- Owner kind mismatch after promote step — gate `discriminant-promote` arm or constructor binding defect; verify envelope-body emitted row link.
- Extension dict in domain parameter after chain — `extension-fold-snapshot` slug omitted or `extension_fold_target` ignored at gate.
- Egress assignability failure — demission grade mis-tagged or egress row `required_keys` stale versus projection adapter.
- Round-trip owner divergence — `replay-row-pin` or `schema_version` pin defect; trusted replay validate grade mismatch.
- Hypothesis accepts drift keys on closed chain — registry not keyed on emitted `closed` column; collapse to `hypothesis-row-draw` slug binding.
- Checker matrix green on one backend while chain row declares multi-backend `ci_gate_ids` — suppressions at payload sites fail lint before chain execution.

# Evolution Binding At Catalog

- Adding chain slug to active route updates gate expression, emitted row, adapter map, hypothesis registry slice, and parametrized metamorphic module in one version unit — orphan slug references fail harness discovery.
- Retiring discriminant arm removes slug from ingress chains only after migration proves zero dependency — retired arms remain in migration chain snapshots with `assert_never` witnesses, not in active catalog rows.
- New carrier posture on existing concept registers new `chain_id` — do not overload carrier column on existing row; triage revisits artifact choice when carrier changes wire struct versus dict path.
- Generic specialization change updates `emitted_row_ref` specialization args on every chain row referencing the owner — partial retarget fails matrix before metamorphic parametrized tests run.

# Composition Root Catalog Ownership

- The composition root registers all `chain_id` rows for its bounded context — interior modules do not append chain rows, construct metamorphic fixtures, or import catalog factories at call sites.
- Root binds each ingress route to exactly one primary ingress chain row and optional tail round-trip row — multiple carriers for one concept share `emitted_row_ref` authority; `chain_id` differs by `carrier_posture` only.
- Arch import rules extend to catalog consumers: domain modules must not import chain catalog tables or witness factories; adapters and test harnesses import rows; CI policy modules import slug band for gate discovery without importing composition roots.
- Catalog warm-up runs at test collection or build alongside contract reader emission — hot ingress paths consume pre-registered chain metadata only; runtime discovery of chain rows at request time is rejected.
- One catalog module per bounded context exports frozen row tuples — cross-context federation imports shared vocabulary and boundary owner modules, not sibling context catalog tables, unless federation row explicitly documents shared witness authority.

# CI Gate Discovery Rows

- `ci_gate_ids` on each chain row map to representative matrix modules — `checker_matrix` blocks when pyright, mypy, or ty fail on payload-law modules referenced by `emitted_row_ref`.
- `row_oracle_parity` gate executes before any chain declaring `row-oracle-parity` slug — OpenAPI fragment diff against `TypeAdapter.json_schema()` must be green from reader emission.
- `promotion_exhaustiveness` gate binds to chains with `discriminant-promote` slug — missing `match` arms fail static proof before parametrized runtime chain executes.
- `extension_fold_snapshot` gate asserts promotion products match `extension_fold_target` column — hypothesis and metamorphic modules share frozen shape oracles.
- `egress_assignability` gate runs on chains with demission grade `assignability` or `closed_validate` — projection literals must satisfy egress row frozensets before bytes encode slug runs.
- `replay_row_pin` gate compares live and replay chain rows field-for-field on `emitted_row_ref`, `validate_grade`, and `schema_version` — drift in any column blocks trusted-replay merge.
- Gate discovery modules import slug band and `ci_gate_ids` only — they do not import composition roots or domain owners; false coupling hides orphan chains without parametrized tests.

# Rejection Shortcuts

- Metamorphic proof without frozen catalog row — register `chain_id` before parametrized tests.
- `isinstance(x, Payload)` as chain validate step — use `lawful-dict-validate` with literal or adapter-validated dict at the promotion gate.
- Chain row referencing prose field list instead of `emitted_row_ref` — duplicate contract surfaces.
- Integration tests importing `TypeAdapter` in domain modules for chain convenience — boundary leak; root owns adapter warm-up.
- Skipping egress chain row because assignability seems obvious — demission grade `none` requires explicit justification when wire visibility exists.
- Trusted replay chain with different boundary artifact class than live ingress — `replay-row-pin` violated.
- Hypothesis `st.from_type(Payload)` on chains declaring `hypothesis-row-draw` — table-less builders rejected.

# Generic Specialization Chain Rows

- Generic payload owners register one chain row per concrete specialization tuple admitted at root — unbound generic `emitted_row_ref` on active chain rows is invalid integration source.
- `extra_items=T` specialization chains include `extension-fold-snapshot` slug for every concrete `T` row — unbounded `object` erosion fails catalog lint before metamorphic parametrization.
- Default specialization chains emit first in catalog registration order — additional specialization chain rows append only when composition root admits distinct concrete extension bands.
- Promotion tests on generic chains parametrized per concrete argument compare owner kinds across checker backends until parity proven — chain metadata flags `generic_matrix=true` when backends disagree on deferred thunks.
- `model_rebuild()` on ingress models embedding generic payload fields completes before chain row references gate expression — stale forward-ref graphs produce false-red metamorphic steps attributed incorrectly to gate algebra.
