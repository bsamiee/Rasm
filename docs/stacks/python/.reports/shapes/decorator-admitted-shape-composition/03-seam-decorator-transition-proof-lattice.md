# Seam Decorator Transition Proof Lattice

# Critical Signals

- Every admitted seam crossing declares one frozen lattice row binding altitude endpoints, owner type expressions, compile artifacts, unwrap proof, failure attribution, trust posture, and negative fixtures — metamorphic and arch proof consumes row columns as field authority; hand-maintained transition lists beside row refs fail admission.
- `transition_id` slugs name reusable seam morphisms — ingress materialize, wire projection, trusted replay, policy stack — registered per adapter edge in `SEAM_TRANSITION_LATTICE`, not inferred from pytest file paths or ordinal stage numbers alone.
- Harness execution order pins static checker and Ruff registration before compile oracle walks before unwrap fingerprint before call-time beartype before altitude import-graph before metamorphic decorator chains — lattice `compile_oracle` and `unwrap_proof` columns declare subset; expensive layers block when cheaper layers fail.
- Lattice is proof and CI policy only — interior modules must not import transition tables as runtime routing authority; composition roots and boundary adapters register rows; parametrized harnesses and arch gates consume them.
- Seam decorator transition proof lattice owns row shape, altitude band, compile oracle band, negative fixture band, harness layer map, and CI gate bindings — admission doctrine, wrap and AOP algebra, compile phase morphisms, promotion certificates, and equilibrium reconciliation cite transition catalogs by machine-table foreign key without restating transition bodies.

# Lattice Row Shape

- Every admitted seam crossing declares one frozen row: `transition_id`, `upstream_altitude`, `downstream_altitude`, `upstream_owner`, `downstream_owner`, `upstream_compile_artifact`, `downstream_compile_artifact`, `unwrap_proof`, `compile_oracle`, `failure_attribution`, `trust_posture`, `negative_fixture_ids`, optional `proof_required`, optional `stack_version`.
- `transition_id` is a stable snake slug — `ingress_validate_to_materialize`, `canonical_to_wire_projection`, `trusted_replay_re_ingress` — not ordinal stage numbers alone; stage names annotate rows but do not replace ids.
- `upstream_altitude` and `downstream_altitude` draw from the closed stack altitude band below — altitude downgrade without documented adapter row is a lattice defect.
- `upstream_owner` and `downstream_owner` are closed type expressions — `BaseModel`, `Struct`, `Owner`, `Result[Owner, E]`, `Callable[P, R]` — erased `dict[str, Any]` or `object` rows fail admission.
- `upstream_compile_artifact` and `downstream_compile_artifact` name durable evidence — `__pydantic_core_schema__`, `TypeAdapter.core_schema`, `msgspec.inspect.type_info`, `inspect.signature` after unwrap — not Python source decorator lines.
- `unwrap_proof` is boolean on callables crossing the transition — handler rows require `inspect.unwrap` depth parity with production seam presentation and `__type_params__` preservation on PEP 695 owners.
- `compile_oracle` is one of the closed compile oracle band values — interior canonical handoffs default `none`; ingress and generic specializations default `first_touch`; egress rows carrying serializers default `dual_mode_schema`.
- `failure_attribution` names the admitting exception family — `ValidationError.loc`, `BeartypeCallHintViolation`, `msgspec.ValidationError`, `Result` error channel — policy wrappers that erase attribution invalidate the row.
- `trust_posture` labels one of `{untrusted, semi_trusted, trusted_replay, trusted_internal}` — selects whether `.raw_function`, `model_construct`, or full ingress re-validation owns the downstream path.
- `negative_fixture_ids` index harness anti-patterns each row must reject — closed negative fixture band below; missing fixture for a declared id fails catalog completeness review.
- `proof_required` is boolean — `true` on polymorphic arms with egress serializers, trusted replay chains, and metamorphic decorator rows; `false` on interior scalar folds and import-phase registry rows unless promotion policy row overrides.

# Stack Altitude Band

- **Boundary altitude** — ingress adapters, wire projectors, and seam `materialize_*` functions admit `@validate_call`, `@beartype`, and policy wrappers (`trace`, `authorize`); synthesis and validator decorators on domain records do not run here except through `TypeAdapter` or explicit `model_validate` on declared ingress types.
- **Ingress-projection altitude** — Pydantic discriminated ingress models carry `@field_validator`, `@model_validator`, and `Field` specifiers for wire-visible law; validates foreign layout before canonical construction and never substitutes as the durable domain owner.
- **Canonical altitude** — frozen domain records, variant `Member` arms, and rich class owners carry synthesis, replacement, and enrichment decorators; `@computed_field` and `@model_validator(mode="after")` enforce invariants that must survive interior transforms without re-ingress.
- **Composition-root altitude** — roots wire adapters, settings fan-out, protocol scope maps, and module-owned `TypeAdapter` constants; decorator factories close over frozen `ConfigDict` and `BeartypeConf` declared in root `[CONSTANTS]`, not in leaf domain modules.
- **Interior altitude** — rail transforms, folds, and `copy.replace` chains operate on already-admitted canonical values; call-time `@beartype` on interior helpers is permitted; `@validate_call` and ingress validators are rejected unless an explicit `re_ingress_boundary` row is declared.
- Altitude law — domain modules importing boundary ingress models, serializer decorators on interior methods, or registry decorators mutating catalogs from call-time wrappers are seam defects; lattice rows document permitted altitude transitions only — undocumented downgrade or skip is merge blocker.

# Compile Oracle Band

- `none` — compile graph is fixed at class creation or transition is value-only; oracle walk verifies field registries and layout flags only — `msgspec.inspect.type_info`, `dataclasses.fields`, interior canonical folds.
- `class_body` — synthesis and validator decorators compile during class-body phase; oracle compares `model_fields` against `annotationlib.get_annotations(..., include_extras=True)` before first-touch.
- `first_touch` — pydantic `model_rebuild()`, `TypeAdapter` schema compile, or generic specialization materializes deferred graph; oracle compares `__pydantic_core_schema__` tag multiset after first reference with cache key `tuple(get_args(cls))`.
- `egress_only` — serializer nodes execute only on dump paths; oracle confirms ingress `model_validate` does not invoke `@field_serializer` or `@model_serializer` nodes — plain and wrap serializer phases are egress morphisms only.
- `dual_mode_schema` — paired validation-mode and serialization-mode JSON Schema snapshots required; `@computed_field` and JSON-only serializers under-approximate public egress when validation-mode snapshot alone is diffed.
- Oracle law — `compilation success alone` does not discharge lattice review; pin upgrades on rows carrying `first_touch` or `dual_mode_schema` require oracle rerun before merge.

# Policy Stack And Unwrap Band

- Callable policy stack order outermost-to-innermost: `trace > authorize > validate > cache > govern > retry > operation` — `@validate_call` sits inside observability wrappers so `ValidationError.loc` surfaces before trace span closure when possible.
- Stack reorder changes observability and failure archaeology even when static types are unchanged — reorder without stack version bump is indexed negative `policy_stack_reorder`.
- `inspect.unwrap(fn, stop=lambda f: hasattr(f, 'registry'))` halts at singledispatch base regardless of intermediate policy layers — catalog rows key dispatch base `qualname`, not outermost trace shell.
- `@beartype` outside `@validate_call` when both guard the same seam handler — inner validate_call compiles pydantic-core schema, outer beartype enforces Python annotation objects; duplicate failure surfaces with incompatible exception types are merge blockers.
- Law-matrix hypothesis stacks apply `@given` before `@settings` before pytest marks; `pytest.mark.parametrize` outermost above hypothesis when both apply — re-applying `wraps` on hypothesis inner closures resurrects strategy parameters on the collected item and is rejected.
- `TypeAdapter(T)` at module constants and `@validate_call` on handlers are alternate boundary owners — stacking both on one callable without documented admission split creates duplicate failure surfaces; split rows declare distinct `upstream_compile_artifact` columns on different symbols.

# Negative Fixture Band

- `duplicate_ingress_canonical_validator` — identical wire scalar normalization on ingress `@field_validator(mode="before")` and canonical `@model_validator` without divergence policy row.
- `interior_model_validate` — interior module calls `model_validate` on `model_dump` output without declared `re_ingress_boundary` row.
- `post_touch_registry_mutation` — call-time policy wrapper mutates class registries or catalog rows after first-touch compile.
- `marker_only_admission` — decorator carries static evidence without synthesis, validation, projection, or signature-preserving wrap semantics.
- `json_loads_before_adapter` — seam handler parses raw JSON with `json.loads` before `TypeAdapter.validate_json` on ingress model.
- `dump_surgery_projection` — wire projection mutates canonical `model_dump` dict with manual key surgery instead of admitted serializer or `msgspec.convert`.
- `parallel_compile_graph` — pydantic synthesis and msgspec struct layout on the same leaf class for one concept.
- `policy_stack_reorder` — trace, authorize, validate, cache layers reordered without stack version bump and seam failure-injection replay.
- `context_leak` — `ContextVar.set(old)` without `Token.reset()` under concurrency on `Concatenate[Ctx, P]` stacks.
- `catch_all_discriminator` — callable discriminator returns widened `str` routing absorbing unknown tags into default arm.
- `per_request_adapter` — hot path constructs fresh `TypeAdapter(T)` per invocation instead of importing pinned root constant.
- `closure_string_fixture_id` — negative or parametrized tests keyed to decorator closure string ids instead of `transition_id` and law record indices.

# Altitude-Native Edge Catalog

- `boundary_to_ingress_projection`: upstream seam handler with `@validate_call` or `TypeAdapter`; downstream ingress `BaseModel`; altitudes `boundary → ingress_projection`; compile oracle `first_touch` on ingress owner; negatives include `json_loads_before_adapter`.
- `ingress_projection_to_materialize`: upstream validated ingress model; downstream canonical `Owner` or `Result[Owner, E]`; altitudes `ingress_projection → canonical`; upstream oracle `class_body` on ingress graph; downstream `none` or `class_body` on canonical synthesis decorators; `failure_attribution` splits `ValidationError` upstream versus constructor `Result` downstream.
- `canonical_to_interior_fold`: upstream materialized canonical instance; downstream successor via `model_copy`, `copy.replace`, or owner factory; altitudes `canonical → interior`; compile oracle `none`; `@validate_call` and ingress validators rejected unless `re_ingress_boundary` row exists.
- `canonical_to_wire_projection`: upstream canonical pydantic or dataclass owner; downstream msgspec `Struct` or adapter wire row; altitudes `canonical → boundary`; downstream compile `type_info`; owner single `msgspec.convert` or admitted serializer — negatives include `dump_surgery_projection`.
- `wire_projection_to_serialization`: upstream wire struct; downstream deterministic `bytes`; compile oracle `egress_only` when `@field_serializer` nodes are load-bearing; `proof_required=true` on polymorphic arms requiring per-arm encode-decode-revalidate-metamaterialize receipts before cache write or cross-process handoff.
- `serialization_to_re_ingress`: upstream stored or cross-process `bytes`; downstream ingress-validated model; altitudes `boundary → ingress_projection`; `trust_posture=trusted_replay` or `semi_trusted` with pinned decoder identity; full ingress compile graph replays unless trust row documents field-level skip.

# Ingress And Canonical Dedup Rows

- `ingress_field_before_to_canonical_after`: upstream `@field_validator(mode="before")` on ingress; downstream `@model_validator(mode="after")` on canonical only when wire law and domain law intentionally diverge — duplicate scalar normalization on both altitudes is indexed negative `duplicate_ingress_canonical_validator`.
- `annotated_before_to_model_after`: upstream `Annotated[..., BeforeValidator(f)]` on ingress field; downstream cross-field `model_validator(mode="after")` on canonical — compile oracle diffs ingress `function-before` tags against canonical after-node multiset.
- `discriminated_ingress_to_variant_materialize`: upstream union ingress with `Discriminator`; downstream per-arm canonical constructor; unwrap proof `false`; metamorphic proof parametrizes every `Literal` arm before merge.
- `computed_field_egress_only`: upstream canonical `@computed_field`; downstream OpenAPI or wire schema fragment; compile oracle `dual_mode_schema` — validation-mode snapshots alone under-approximate egress after computed admission.
- `private_attr_interior_only`: upstream `PrivateAttr` on canonical; downstream interior cache slot; compile oracle `none`; serializers and ingress specifiers must not mirror the same logical name.

# Cross-Family Decorator Projection Rows

- `pydantic_ingress_to_msgspec_wire`: upstream frozen ingress `BaseModel` with validator graph; downstream wire `Struct` without pydantic synthesis on the same leaf; owner boundary `msgspec.convert` or adapter projection — dual synthesis on one class is indexed negative `parallel_compile_graph`.
- `msgspec_manifest_to_pydantic_settings`: upstream internal `Struct` manifest; downstream `BaseSettings` at composition root only; altitudes `interior → composition_root`; settings compile graphs evolve independently from domain canonical graphs.
- `dataclass_record_to_pydantic_canonical`: upstream `@dataclass_transform` record; downstream promoted `BaseModel` canonical — materialize row pins smart constructor, not `model_validate` on dataclass `asdict` output.
- `cooperative_type_embedded_slot`: upstream non-`BaseModel` with `__get_pydantic_core_schema__`; downstream enclosing field on ingress or canonical owner — cooperative compile subgraph oracle walks enclosing `GenerateSchema` output, not standalone class decorators.
- `type_adapter_boundary_split`: upstream `TypeAdapter(T)` at root constant; downstream handler with `@validate_call` on different callable — rows declare distinct `upstream_compile_artifact` and forbid stacking both on one symbol without documented admission split row.

# Wrap Chain And Policy Stack Rows

- `policy_stack_validate_call_handler`: upstream operational body; mid `@validate_call`; outer `trace`, `authorize`, `cache` per policy stack band; `unwrap_proof=true`; validation failures must attribute before trace span closure — reorder without stack version bump is indexed negative `policy_stack_reorder`.
- `beartype_outside_validate_call`: upstream `@validate_call` inner; downstream `@beartype` outer on same seam handler; `failure_attribution` discriminates `ValidationError` versus `BeartypeCallHintViolation` — catching either as undifferentiated `TypeError` invalidates row.
- `singledispatch_registry_fold`: upstream dispatch base with `register`; downstream policy wrappers; unwrap stops at `registry` per policy stack band; catalog rows key dispatch base `qualname`, not outermost trace shell.
- `async_coroutine_preservation`: upstream `async def` law subject; downstream policy stack; `inspect.iscoroutinefunction` true on collected callable after unwrap exhaustion — sync wrapper shells are seam defects.
- `concatenate_context_discharge`: upstream `Concatenate[Ctx, P]` outer shell; downstream inner full `P` for beartype; `ContextVar` discharge uses `Token.reset()` law — bare `ctx.set(old)` under concurrency is indexed negative `context_leak`.

# Discriminated Union And Variant Transition Rows

- `literal_discriminant_to_arm_constructor`: upstream `Literal` discriminant on ingress union field; downstream per-arm `materialize_*` targeting canonical variant; callable discriminators return `Literal` tags or `None` only — widened `str` routing is indexed negative `catch_all_discriminator`.
- `nested_union_discriminator_stack`: upstream outer union with inner tagged arm; downstream independent error paths per discriminator level — inner `TypeAdapter` failure must not coerce to outer default arm.
- `tag_metadata_msgspec_pydantic_parity`: upstream msgspec `tag_field` wire arm; downstream pydantic ingress `Discriminator` arm; vocabulary table row at composition root binds tag strings — parallel hand-maintained tag tuples on route modules fail lattice parity gate.
- `variant_computed_field_canonical_only`: upstream canonical arm `@computed_field`; downstream egress schema member; ingress union members must not host enrichment decorators that never durably own the concept.

# Dataclass And Record Transition Rows

- `dataclass_synthesis_to_init_signature`: upstream `@dataclass` or `@dataclass_transform` factory; downstream `inspect.signature(__init__)` parity with `dataclasses.fields` ordering; compile oracle `class_body`; `init=False` and `kw_only` specifiers must agree across row proof samples.
- `dataclass_post_init_to_canonical_gate`: upstream dataclass-only `__post_init__`; downstream pydantic canonical validation — dataclass validators never substitute for pydantic `model_validator` on promoted canonical owners; promotion row pins explicit constructor path.
- `record_factory_to_frozen_base_model`: upstream repo `@record` factory; downstream frozen `BaseModel` canonical; `ConfigDict(frozen=True, extra="forbid")` on downstream must not contradict upstream immutability decorators.

# Parametric And Rebuild Transition Rows

- `generic_first_touch_rebuild`: upstream `Owner[T]` at import; downstream `Owner[int]` or concrete specialization at first reference; compile oracle `first_touch`; cache keys include `tuple(get_args(cls))` — monomorphic import fixtures do not discharge row.
- `forward_ref_resolution_at_rebuild`: upstream deferred module peers; downstream resolved `model_fields` after `model_rebuild()`; oracle compares `annotationlib.Format.FORWARDREF` reads at class-body against `Format.VALUE` after first-touch.
- `schema_version_migration_hop`: upstream stored payload at version N compile graph; downstream current owner graph; owner historical `TypeAdapter` then total fold — validator decorator removal on current owner does not rewrite stored bytes without version bump.
- `stack_fingerprint_promotion`: upstream decorator stack fingerprint row; downstream updated fingerprint after add/remove/reorder event; promotion unit updates ingress mirror, metamorphic codec samples, and law-matrix `@spec` rows together — partial fingerprint update is merge blocker.

# Property Harness And Law Matrix Rows

- `spec_collected_signature_parity`: upstream production adapter handler; downstream law-matrix collected item; `unwrap_proof=true`; fingerprints must agree on `__type_params__` and signature after promotion — pytest file paths are not row keys.
- `hypothesis_compiled_strategy`: upstream pydantic owner with opaque `@field_validator` bodies; downstream `st.register_type_strategy` keyed on compiled inner schema nodes — bare annotation re-derivation under-approximates wrap and plain phases.
- `phase_attributed_failure_injection`: one defect per decoration phase per row family — import duplicate registration, class-body transform conflict, first-touch stale generic graph, call-time beartype violation; conflated injection fails harness module.
- `metamorphic_decorator_chain`: lawful instance → egress serializers → encode → decode → ingress validators → `materialize_*` → canonical equality — row `proof_required=true` on polymorphic arms; failure implicates projection decorators before validator body edits.

# Serializer Mode And Egress-Only Rows

- `field_serializer_json_only`: upstream `@field_serializer(when_used='json')`; downstream JSON egress path only; Python-mode dumps retain stored field types — row `compile_oracle=egress_only`; ingress `model_validate` must not invoke serializer nodes.
- `model_serializer_wrap_delegate`: upstream `@model_serializer(mode='wrap')`; downstream custom dump handler chain; wrap serializers delegate to default handlers — dual-purpose methods sharing ingress validator names are indexed negatives.
- `plain_serializer_wire_divergence`: upstream `PlainSerializer` with `when_used='json'`; downstream wire type diverging from Python stored type; row requires `json_schema_input_type` evidence on plain validator nodes in compiled graph — plain nodes are terminal replacements and wire contract must be schema-visible.
- `computed_field_serializer_collision`: upstream `@computed_field` plus egress serializer on same logical slot — row forbids constructor input on derived slots; OpenAPI diff gate runs under `dual_mode_schema` oracle.

# Trusted Replay And Raw Function Rows

- `validate_call_raw_function_gate`: upstream `@validate_call` wrapper; downstream `.raw_function` fast path; `trust_posture=trusted_replay` with schema version and encoder identity in row metadata — default foreign ingress uses validated wrapper only.
- `model_construct_trusted_bypass`: upstream canonical owner; downstream `model_construct` or `model_validate` with `from_attributes`; permitted only when trust row pins compile fingerprint and storage envelope — graph evolution without trust re-pin is security defect.
- `semi_trusted_type_adapter_replay`: upstream `TypeAdapter(T)` at root; downstream interior canonical type without re-validation; `trust_posture=semi_trusted`; field-level skip columns explicit per trust table — interior modules do not construct local bypasses.

# Registry And Catalog Transition Rows

- `import_phase_register_to_dispatch`: upstream handler with complete PEP 695 signature; downstream `singledispatch.register` row append; altitudes `composition_root → composition_root`; compile oracle `none`; registration does not repair erased signatures post hoc.
- `spec_law_matrix_to_production_stack`: upstream `@spec` collected item; downstream production adapter handler; `unwrap_proof=true`; registry stores `qualname` and `module` fingerprints, not closure state or pytest node ids.
- `catalog_row_to_cross_process_handoff`: upstream registry metadata; downstream canonical value envelope; lattice row forbids serializing catalog rows as durable payloads — handoff uses admitted owner bytes and version literals only.
- `pytest_parametrize_over_hypothesis`: upstream `pytest.mark.parametrize` outermost; downstream hypothesis `@given` inner per policy stack band; parametrize indices align with law records — decorator closure string ids are indexed negative `closure_string_fixture_id`.

# Settings And Boot Transition Rows

- `env_to_settings_compile`: upstream raw environment slice; downstream frozen `BaseSettings` at composition root; compile oracle `first_touch` on settings graph only; domain canonical rows must not re-encode settings `field_validator` law unless explicit mirror policy row exists.
- `settings_fanout_to_boot_record`: upstream settings owner; downstream projected boot config record; altitudes `composition_root → canonical`; `failure_attribution` on settings `ValidationError` only at process start, not per interior transform.
- `settings_customise_sources_order`: upstream source precedence declaration; downstream compiled settings instance; row documents `@override` on `settings_customise_sources` as static evidence — reorder is deployment contract, not runtime patch.
- `cyclopts_staging_to_single_owner`: upstream CLI keyword staging surface; downstream single decorator-admitted owner when promotion completes — until collapse, dedicated staging rows stay at `boundary` altitude with distinct `transition_id` from settings rows.

# OpenAPI And Schema Snapshot Row Binding

- Rows with `compile_oracle=dual_mode_schema` store paired validation-mode and serialization-mode JSON Schema snapshot ids beside owner modules — CI diffs both on decorator or `Field` specifier edits and on `pydantic` pin changes.
- Ingress and canonical owners under different decorator stacks diff snapshots independently — merging ingress wire schema into canonical snapshot rows masks altitude violations.
- `$ref` deduplication churn without constraint change is refactor-only; new validator nodes or computed-field schema members require row version bump and metamorphic replay on egress rows.
- `model_json_schema()` authority rows reject hand-maintained parallel OpenAPI field tables diverging from decorated owner output after `@computed_field` admission.

# Composition Root Assembly Rows

- `root_adapter_graph_wiring`: upstream foreign ingress type; downstream `Result[Canonical, E]` per context; each edge in the handoff graph declares one lattice row — root seam maps without row coverage fail arch gate.
- `root_type_adapter_constant_pin`: upstream generic `TypeAdapter(Box[int])`; downstream hot-path import of pinned symbol from `[CONSTANTS]`; row metadata includes `tuple(get_args(T))` — per-request `TypeAdapter` construction is indexed negative `per_request_adapter`.
- `root_beartype_conf_anchor`: upstream module `BeartypeConf` constant; downstream factories closing over the same frozen instance; per-call-site `BeartypeConf(...)` inside decorator factories invalidates root assembly rows.
- `root_import_acyclic_domain_policy`: upstream domain modules without boundary ingress imports; downstream root-owned adapters; altitude arch proof complements lattice — domain folder importing ingress decorator owners fails even when row exists elsewhere.

# Lattice Drift And Evolution Morphism

- Adding a decorator that alters compile graph, serializer egress, or callable codomain bumps `stack_fingerprint` and every lattice row referencing the owner — marker-only additions without executable semantics do not bump row version.
- `pydantic` pin upgrades require dual-mode schema diff on rows with `compile_oracle=dual_mode_schema` and `__pydantic_core_schema__` tag multiset diff on `first_touch` rows — compilation success alone does not discharge lattice review.
- Removing ingress mirror when canonical gains equivalent validator is a breaking row event — duplicate-law checklist and negative `duplicate_ingress_canonical_validator` fixtures must flip in one promotion unit.
- Arch tests grep domain modules for ingress decorator owners — lattice altitude proof complements import-graph altitude law; row absence for an admitted seam in production adapters is harness exhaustiveness defect.

# Failure Archaeology By Transition Id

- `ingress_projection_to_materialize` failure on upstream path — attribute to ingress `ValidationError.loc` and ingress compile graph, not canonical constructor.
- `ingress_projection_to_materialize` failure on downstream `Result` error — attribute to smart constructor or canonical `model_validator`, not ingress re-parse.
- `canonical_to_interior_fold` `ValidationError` — misclassified re-ingress without `re_ingress_boundary` row; interior should see canonical types only.
- `beartype_outside_validate_call` with `BeartypeCallHintViolation` after successful pydantic validation — attribute to outer beartype or policy stack, not ingress graph.
- `trusted_replay_re_ingress` `ValidationError` on trusted path — trust row stale after validator graph evolution; fix trust re-pin, not domain interior.
- `spec_collected_signature_parity` mismatch — hypothesis `wraps` ordering or `@given` parameter resurrection on collected item per policy stack band, not production stack unless test imports production incorrectly.
- `pydantic_ingress_to_msgspec_wire` encode-decode asymmetry — attribute to egress serializer or wire projection row before ingress validator edits.
- `schema_version_migration_hop` partial field acceptance — attribute to migration fold owner and historical `TypeAdapter` row, not current canonical `model_validator` alone.
- `registry_and_catalog` duplicate registration at import — attribute to import-phase catalog decorator, not call-time handler body.
- `settings_fanout_to_boot_record` env read inside domain transform — altitude violation; settings transition row never satisfied by interior `os.environ` access.

# Harness Execution Order On Lattice Proof

- Static checker and Ruff decorator registration rows execute before lattice row presence arch gate — orphan `runtime-evaluated-decorators` entries without exercised owners fail before parametrized seam suites.
- Compile oracle walks on rows with `compile_oracle=first_touch` or `dual_mode_schema` execute before metamorphic decorator chains — stale generic graphs fail cheaply before generative codec replay.
- `unwrap_proof` signature parity runs before call-time beartype samples on handler rows — fingerprint mismatch blocks expensive policy-stack failure injection.
- Seam altitude import-graph rules and lattice row coverage run before interior domain behavioral suites — misattributed graph drift is caught at harness layer, not as false domain regressions.
- Mutation kill gates on adapter modules run after lattice negative fixtures pass — mutants surviving only on happy paths indicate missing `negative_fixture_ids` coverage.

# CI Regression Gates On Lattice Rows

- Every production `materialize_*` adapter declares at least one lattice row in the owning boundary module or root seam map — adapter without row fails arch gate before behavioral suites.
- Parametrized contract tests index rows by `transition_id` and `negative_fixture_ids` — missing negative coverage for a declared anti-pattern fails decorator coverage gate.
- Mutation testing on seam adapters kills mutants erasing `__wrapped__`, widening signatures to `Any`, or moving `@validate_call` outside policy stack — kill ratio depends on lattice `unwrap_proof` and compile oracle gates.
- ast-grep flags interior `model_validate` on `model_dump` output, post-class `model_config.update()`, and domain imports of ingress-only decorator owners — violations fail before metamorphic chains misattribute drift.

# Transition Registry Shape

- Root `SEAM_TRANSITION_LATTICE: tuple[SeamTransitionRow, ...]` extends admission obligations from the decorator federation — every admitted seam edge binds exactly one lattice row; orphan adapters without row coverage fail arch gate before behavioral suites.
- `SeamTransitionRow` is a frozen record beside the owning boundary module or composition-root seam map — `transition_id`, `upstream_altitude`, `downstream_altitude`, `upstream_owner`, `downstream_owner`, `upstream_compile_artifact`, `downstream_compile_artifact`, `unwrap_proof`, `compile_oracle`, `failure_attribution`, `trust_posture`, `negative_fixture_ids`, optional `proof_required`, optional `stack_version`, optional `promotion_unit_id` when row is mid-flight.
- Seam map modules declare `foreign_ingress -> Result[Canonical, E]` edges with one row per edge — handoff graphs without total row coverage fail `root_adapter_graph_wiring` CI gate.
- Trust sub-rows attach when `trust_posture` is not `untrusted` — schema version literal, encoder module, storage envelope, field-level skip columns explicit per trust table.
- Schema snapshot sub-rows attach when `compile_oracle=dual_mode_schema` — validation-mode and serialization-mode snapshot ids beside owner modules with paired diff jobs.
- Registry build is import gate — bounded contexts without `SEAM_TRANSITION_LATTICE` row per admitted adapter edge fail arch review before parametrized seam suites execute.
- Duplicate transition declarations — test file re-declares edge catalog parallel to owner seam map — collapse to single parametrized import from boundary registry per steady-state assurance policy.

# Parametrized Harness Consumption

- Policy runners discover lattice rows by `transition_id` — parametrized contract tests must index the same slug declared in `SEAM_TRANSITION_LATTICE`; mismatch fails harness registration smoke before metamorphic suites start.
- Parametrized pytest modules accept `transition_id` and `negative_fixture_ids` — one negative test function per fixture id per row; mega-tests covering all negatives on one parametrized axis are rejected for failure localization.
- `negative_fixture_ids` drive table-driven negative modules — each id maps to one fixture factory and one assertion family; fixture id without row reference or row id without fixture fails catalog completeness review.
- Compile oracle parametrization runs on rows declaring `first_touch` or `dual_mode_schema` before rows declaring `none` only when both appear in the same promotion unit — cheaper oracle failures block expensive metamorphic replay.
- Integration matrix parametrizes altitude import-graph negatives independently per bounded context — combined multi-context mega-import test is discouraged.
- `stack_version` rotation on promotion emits changelog entry with affected `transition_id` list — consumers read changelog before upgrading seam adapters or trust rows.

# Rejection Shortcuts

- Seam proof without frozen lattice row — register edge in `SEAM_TRANSITION_LATTICE` before parametrized contract modules.
- Parametrized tests keyed to pytest module path instead of `transition_id` — harness discovery fails at registration smoke.
- Mega-test parametrized on all `negative_fixture_ids` for one row on a single axis — one test function per fixture id per row for failure localization.
- Interior `model_validate` on `model_dump` output without `re_ingress_boundary` row — indexed negative `interior_model_validate`.
- Stacking `TypeAdapter(T)` and `@validate_call` on one callable without `type_adapter_boundary_split` row — duplicate compile failure surfaces.
- Policy stack reorder without stack version bump — indexed negative `policy_stack_reorder`.
- Metamorphic proof on polymorphic arms without per-arm encode-decode receipt when `proof_required=true` — equilibrium defect on wire handoff edges.
- Hand-maintained parallel OpenAPI field tables diverging from `model_json_schema()` after decorator edits — merge blocker regardless of behavioral suite green.
- Catalog row serialized as cross-process durable payload — handoff uses admitted owner bytes and version literals only.
- Duplicate lattice row declarations in test file parallel to owner seam map — collapse to single parametrized registry import.

# Lattice Completeness Checklist

- Every admitted adapter edge has one `SeamTransitionRow` with closed altitude endpoints, owner type expressions, and `negative_fixture_ids` subset drawn from the negative fixture band.
- Every `negative_fixture_ids` entry has one fixture factory and one assertion family in table-driven negative modules.
- Every row with `compile_oracle=first_touch` or `dual_mode_schema` has oracle walk modules and snapshot sub-rows when required.
- Every row with `unwrap_proof=true` has signature parity modules comparing production adapter and law-matrix collected item.
- Every row with `trust_posture` not `untrusted` has trust sub-row with schema version, encoder identity, and field-level skip columns.
- Every `materialize_*` adapter appears in root seam map with total edge coverage — no orphan handlers.
- Every promotion touching decorator stacks emits lattice diff with affected `transition_id` list synchronized with certificate `lattice_row_ids`.
- Done when transition registry, parametrized harness consumption, CI regression gates, and rejection shortcuts are closed — interior modules never import lattice tables as runtime authority.

