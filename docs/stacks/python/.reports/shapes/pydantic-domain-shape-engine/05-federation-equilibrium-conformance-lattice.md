# Federation Equilibrium Conformance Lattice

# Multi-Layer Owner Wrap Seam Catalog

- Deeply nested owner graphs stack three or more enclosing `BaseModel` layers — envelope, aggregate, and leaf projection owners — each may declare `@model_serializer(mode='wrap')` before boundary `project_wire`; federation requires explicit seam rows naming wrap order, not implicit nesting convention.
- Wrap execution order is outermost-last: leaf field and cooperative hook serializers compile per slot on the innermost owner; each enclosing `@model_serializer` wrap post-processes its child's dump product; root boundary projection consumes only the outermost wrap output documented in the conformance row.
- **Seam row `nested_wrap_chain`** — fields: `chain_id`, `innermost_owner`, `wrap_owners_ordered` (leaf→root), `per_layer_envelope_keys`, `dump_mode`, `msgspec_target_struct`, `negatives` — prose nesting without row binding fails root federation gates.
- Inner wraps must not re-encode slots already finalized by cooperative hook serializers on leaf owners — inner wrap adjusts container envelope; leaf hook law remains authoritative for slot scalars; outer wrap adjusts transport envelope only.
- `when_used='json'` on mid-layer wraps limits transforms to JSON dumps — conformance rows declare which layers participate in `python` mode for msgspec `convert` lanes; silent mid-layer `json` participation breaks interior folds.
- `@computed_field` on mid-layer owners enters that layer's handler input before its wrap executes — outer wraps must not strip computed evidence inner validators assume unless `exclude_computed_fields` is documented per layer in the row.
- Rebuild gate for multi-layer graphs is the outermost enclosing owner — `model_rebuild()` on the transport envelope owner resolves forward refs for every inner layer; inner owners do not rebuild in isolation unless documented as standalone `TypeAdapter` roots in the seam map.
- Persistence keys for multi-layer stored layouts include outermost `schema_version`, every wrap owner symbol in `wrap_owners_ordered`, and concrete generic argument tuples on cooperative arms at leaf layers — erasure-friendly generic names alone fail cross-process replay.
- Oracle registration for multi-layer graphs belongs on the outermost owner strategy — `st.register_type_strategy(OutermostOwner, strategy)` generates instances satisfying every inner wrap envelope constraint and every leaf hook construction law; per-layer strategies without `wrap_coherence` witness fail federation admission.
- Collapse test: manual dict surgery between wrap layers — collapse to documented `nested_wrap_chain` row with hook serializers at leaf and envelope-only wraps at mid and outer layers.

# Cyclopts And Root Discriminator Unification

- Callable `Discriminator` on `RootModel` unions, alias unions, and graph root arms currently maintain parallel cyclopts choice metadata — federation row `cli_http_discriminator_source` binds one generative OpenAPI and CLI vocabulary to Pydantic model declarations until cyclopts publishes native discriminator import from compiled `TypeAdapter` graphs.
- **Unification row fields** — `source_owner` (`RootModel` or closed union alias), `discriminator_kind` (`field`, `callable`, `literal_root`), `openapi_export_path`, `cyclopts_param_owner`, `tag_vocabulary`, `tag_extraction_oracle` (`json_schema_mapping` | `core_schema_choices` | `enum_members`), `generated_artifact_path`, `proof_samples_per_arm` — dual-maintained choice tables without row binding are merge blockers when root schema export is enabled.

# Cyclopts 4.16 Capability Surface And Ingress Split

- Cyclopts `>=4.16.1` (pinned in root manifest) recognizes `Annotated[..., Field(discriminator=...)]` and `Annotated[..., Discriminator(callable)]` through `get_annotated_discriminator` — effect today is `token_count` forcing a single JSON-string token for discriminated union parameters, not discriminator-aware routing or help-page choice synthesis.
- `get_choices_from_hint` extracts completion and help choices only from bare `Literal`, `Enum`/`StrEnum`, and shallow `Union` members — discriminated unions of `BaseModel` arms return empty choice tuples even when `Field(discriminator=...)` is present; cyclopts does not walk arm discriminant `const` fields or `TypeAdapter.json_schema()` mappings for help text.
- Union coercion in cyclopts remains left-to-right try-each-member per published coercion rules — discriminated union annotations change token arity, not selection algorithm; HTTP ingress through root `TypeAdapter.validate_json` / `model_validate` uses compiled `tagged-union` routing from Pydantic core — CLI and HTTP are dual ingress morphisms on the same `source_owner` until unification lands or witness artifacts enforce bijection.
- Pydantic `BaseModel` parameters delegate field metadata through `_pydantic_field_infos`, preserving `pydantic_field.discriminator` on embedded union slots — nested `Holder.pet: Pet` inherits single-token JSON consumption but still emits empty cyclopts choices on the union hint; federation does not assume help-page `[choices: ...]` appears without `generated_artifact_path` or a direct `StrEnum`/`Literal` parameter surface.
- Dataclass param owners (`@Parameter` on `BaseParams` and rail `*Params`) remain outside pydantic discriminator compilation — assay and quality CLIs bind frozen dataclass fields, not closed union aliases; `cli_http_discriminator_source` applies only where cyclopts parameters or `@validate_call` surfaces admit pydantic union hints from `source_owner`.
- Cyclopts 4.16 ships zero `json_schema` / `oneOf` import hooks — no native read of `TypeAdapter.json_schema()` discriminator tables; cyclopts `5.0.0a*` pre-releases do not yet discharge merge without generated witness artifacts and paired diff tests documented in the row.

# Discriminator Kind Projection Table

- **Field `discriminator_kind`** — `Literal` or `StrEnum` discriminant field on every arm; OpenAPI and ingress JSON Schema publish `discriminator.propertyName` plus `discriminator.mapping`; `tag_extraction_oracle` is `json_schema_mapping` — witness generator reads `frozenset(TypeAdapter(source_owner).json_schema(mode='validation')['discriminator']['mapping'])` after rebuild; cyclopts help choices and shell completion derive from the same frozenset via generated artifact or elevated `StrEnum` parameter, not hand tuples.
- **Callable `discriminator_kind`** — `Discriminator(fn)` plus `Tag('case')` arms; `TypeAdapter.json_schema()` emits `oneOf` with `$ref` arms but omits top-level `discriminator.mapping` — tag vocabulary lives in `TypeAdapter.core_schema` `tagged-union` `choices` keys (`pydantic_internal_union_tag_key` metadata), not in public JSON Schema alone; `tag_extraction_oracle` is `core_schema_choices`; witness generator walks compiled core graph, not `json_schema()` discriminator nodes.
- **`literal_root` `discriminator_kind`** — `RootModel[Annotated[Union[...], Field(discriminator=...)]]` or `RootModel[Literal[...]]` per root-form mutual-embedding law; OpenAPI may emit root-level `oneOf` without object envelope; cyclopts consumes one JSON token; proof samples use root-value carriers — CLI parsers accept root-form JSON, not spurious property shells; HTTP 422 `loc` paths prefix `root` or empty outer segment per root capture policy.
- **Nested discriminated unions** — inner `Annotated[..., Field(discriminator=inner)]` before outer routing per alias-stack and mutual-embedding law; OpenAPI nests `oneOf` under outer discriminator properties; cyclopts parameter groups mirror outer-then-inner dot-key expansion when pydantic models embed nested union fields — conformance samples exercise CLI and HTTP failures at every level with distinct `loc` prefixes, not only outer discriminant negatives.
- **Enum elevation row** — when cyclopts must show native `[choices: ...]` without generated artifacts, expose the discriminant as a direct `StrEnum` parameter or `Literal` union parameter referencing the vocabulary owner — embedding the same enum only inside discriminated `BaseModel` arms does not populate cyclopts choices under 4.16; elevation is an explicit seam decision documented beside `cyclopts_param_owner`, not an accidental duplicate surface.

# Generated Witness Artifact Protocol

- Root CI emits a frozen choice artifact beside each `cli_http_discriminator_source` row when cyclopts unification applies — artifact is a deterministic JSON or TOML document listing `tag_vocabulary` in stable sorted order plus `discriminator_kind`, `property_name` when field-kind, and `source_owner` symbol; hand-maintained parallel CLI enums fail S3 gate on diff.
- Generation morphism is single-source — `TypeAdapter(source_owner)` after `model_rebuild()` on enclosing owners from mutual embedding owner egress calculus; field-kind rows project `discriminator.mapping` keys from validation-mode schema; callable-kind rows project `tagged-union` choice keys from `core_schema` via root adapter pin, not from OpenAPI fragments missing mapping; `literal_root` rows project root-schema `oneOf` const arms or mapping keys when present.
- `Parameter(show_choices=True)` on cyclopts surfaces may reference generated vocabulary through `Parameter(choices=...)` only when the row documents explicit override — default remains type-hint-driven extraction; overrides require `generated_artifact_path` parity proof, not independent string tuples on route modules.
- Shell completion backends (`bash`, `zsh`, `fish`) call `get_choices(force=True)` — generated artifacts also feed completion scripts when union hints return empty tuples; completion drift without artifact update is **V7** skew alongside help-page drift.
- HTTP OpenAPI export and CLI artifact generation share one promotion unit script at root `[OPERATIONS]` — script accepts `source_owner`, `discriminator_kind`, and dual-mode schema mode flags; partial regeneration of OpenAPI without artifact, or artifact without OpenAPI mapping, is **V8** partial landing.

# CLI-HTTP Error Vocabulary Federation

- HTTP ingress failures at root emit `ValidationError.json()`-shaped bodies with pydantic `loc` paths and discriminant-aware branch messages — CLI ingress failures emit cyclopts `CoercionError` / `ValidationError` with parameter-name prefixes when `@validate_call` or pydantic validators participate — `cli_http_discriminator_source` rows document the root projection function that maps both into one violation tuple vocabulary for operators.
- Callable-discriminant CLI negatives that cyclopts resolves through try-each-arm `CoercionError` must still pair with HTTP negatives proving pydantic `tagged-union` rejection on the same payload — metamorphic gate requires per-arm invalid discriminant fixtures on both carriers; CLI-only negative coverage under-approximates HTTP contract when row applies to dual ingress.
- Surplus positional tokens on discriminated union parameters fault before union admission — `BaseParams.bound` surplus law and cyclopts single-token JSON law compose independently; negative fixtures include surplus-token CLI cases beside HTTP malformed-body cases.
- `validate_call` failures prefix `loc` with parameter names — when `cyclopts_param_owner` and HTTP handler share `source_owner`, root uses identical distillation for model and call-boundary failures — domain interiors never import cyclopts or pydantic exception types.

# Settings And Env Exclusion From Cyclopts Unification

- `BaseSettings` and env-backed models remain outside cyclopts unification unless explicitly published as CLI subcommands — settings fields do not generate choice tables from pydantic union discriminators; operator CLI uses settings fan-out slices and `StrEnum`/`Literal` vocabulary owners from owning doctrine layers, not parallel env vocabularies beside model declarations.
- `Field(validation_alias=...)` on settings slices and cyclopts `Parameter` choices reference the same `StrEnum` export per vocabulary-sentinel law — settings discriminant tokens and CLI choice labels biject to enum members; pydantic ingress union tags on domain payloads are a separate projection row when wire and settings intentionally diverge.
- Inherit and sentinel arms on settings resolve through root `project_*_config` before cyclopts or HTTP handlers read canonical enum members — cyclopts never sees raw inherit magic strings when vocabulary owner documents sentinel resolution.

# Promotion And Proof Obligations On Unification Rows

- Promotion unit touching discriminant vocabulary updates `cli_http_discriminator_source` row, OpenAPI mapping, generated cyclopts artifact, msgspec wire tags, matrix `discriminant_literal` columns, and `tag_extraction_oracle` when callable routing changes — one merge-blocking change; partial tag promotion is **V8**.
- S3 contract layer diffs generated artifact beside dual-mode JSON Schema snapshots — field-kind rows diff `discriminator.mapping`; callable-kind rows diff sorted `tag_vocabulary` from core-schema extraction; artifact-only or schema-only diff is under-approximation defect.
- S4 oracle samples one `resolve(source_owner).example()` per `tag_vocabulary` arm through the same admission path HTTP uses — CLI samples additionally exercise cyclopts `convert` on single JSON tokens per arm when `cyclopts_param_owner` is in scope.
- S5 metamorphic rows prove HTTP bytes → `validate_json` → dump → msgspec when cross-engine seam applies — CLI JSON tokens must validate to the same materialized arm type HTTP admits for each positive fixture.
- CLI-HTTP discriminant rename bumps `schema_version` when persisted CLI scripts or stored HTTP bodies key on legacy tags — row updates migration fold mapping legacy tags; cyclopts artifact and OpenAPI mapping regenerate together.
- Proof debt row when cyclopts gains native schema import — `sunset_criterion` names cyclopts release and deleted `generated_artifact_path`; until then, generated tables remain witness artifacts beside the row, not hand-maintained parallel enums.

# Conformance Witness Row Lattice

- Every production Pydantic seam exports a frozen witness row — federation lattice indexes rows by `row_id`, `wave_owner`, `liable_module`, and `proof_layers` without duplicating law already compiled into schemas.
- **Altitude row** — `altitude`, `owner_symbol`, `materialize_entry`, `collapse_negatives` — indexes altitude placement; interior modules reference canonical return types only.
- **Alias row** — `alias_symbol`, `metadata_tuple_hash`, `embedding_owners`, `dual_mode_schema_paths` — indexes alias constraint lattice; alias edits regenerate all indexed embeddings.
- **Cooperative row** — `cooperative_class`, `chain_schema_fingerprint`, `handler_field_name_branches`, `json_hook_fragment_id` — indexes cooperative schema hooks; hook edits regenerate oracle per branch.
- **Graph row** — `enclosing_owner`, `matrix_arm_ids`, `rebuild_owner`, `root_union_kind` — indexes mutual embedding calculus; matrix edits regenerate per-arm samples.
- **Wrap chain row** — `nested_wrap_chain` fields from multi-layer catalog — indexes wrap federation; layer additions are breaking for egress consumers.
- **Cross-engine row** — `source_owner`, `project_wire_expr`, `target_struct`, `dump_mode`, `alias_rename_policy` — indexes dual-engine handoff; unmapped dump keys fail review.
- **CLI-HTTP row** — `cli_http_discriminator_source` fields — indexes cyclopts unification; tag vocabulary drift fails metamorphic gates.
- Row law — undocumented seams are equilibrium violations; promotion units append or revise rows in the same commit as schema or hook body changes; orphan production paths without rows block merge.

# Proof Layer Federation Across Waves

- Shape owners carry six federated proof layers — static, import architecture, contract snapshots, core-schema oracle, cross-engine metamorphic, and runtime — layers are orthogonal; passing one does not discharge others; federation schedules replay order on promotion.
- **S1 static** — `mypy` plus `pydantic.mypy` on root import graph; `annotationlib.get_annotations(..., include_extras=True)` parity against `model_fields` after rebuild on every altitude owner from altitude admission doctrine.
- **S2 import architecture** — domain modules avoid direct ingress `BaseModel` imports; cooperative and alias symbols route through root adapter imports named in `__all__`; altitude violations are S2 failures.
- **S3 contract** — dual-mode JSON Schema snapshot pairs per publishing family from shape ownership doctrine; `cli_http_discriminator_source` generated choice artifacts when cyclopts unification applies; serialization mode mandatory when computed fields, owner wrap, or hook serializers define egress.
- **S4 oracle** — `resolve(T).example()` loops through `model_validate` per alias-stack walker law, cooperative hook chains, and mutual-embedding matrix `arm_id` rows; multi-layer graphs sample through outermost owner strategy only.
- **S5 metamorphic** — cross-engine `encode → decode → model_validate` per shape-ownership conformance rows; multi-layer graphs include outermost wrap in dump chain before convert; discriminated arms sample per tag.
- **S6 runtime** — `beartype` on adapters post-materialization; `validate_call` admission independent of body effects on root handlers.
- Federation replay order on promotion: S1 → S2 → S3 → S4 → S5 → S6 — S4 failure with passing S3 signals validator opacity or wrap/hook drift; S5 failure with passing S4 signals serializer width mismatch.
- Proof debt from waived layers documents `waiver_row_id` beside owner — suppressions at root adapter seams are rejected; waivers name oracle gap and sunset promotion unit.

# Equilibrium Invariants On Shape Owners

- Federation equilibrium is a closed invariant set — violation signals map to default reconciliation rows; undocumented signals block merge.
- **I1 single validation gate** — one `model_validate` / `TypeAdapter` admission per boundary crossing; double validation on same carrier is violation **V1**; reconciliation collapses to root capture only.
- **I2 altitude uniqueness** — each owner occupies exactly one altitude from altitude admission doctrine; duplicate altitude enforcement on same concept is **V2**; reconciliation collapses secondary owner to projection or ingress demotion.
- **I3 alias identity** — one `Annotated` metadata tuple per concept at module scope from annotated constraint stack; inline duplicate stacks are **V3**; reconciliation merges to single alias symbol.
- **I4 cooperative closure** — mutual cycles rebuild through enclosing owner from mutual embedding owner egress calculus; standalone arm validation without rebuild is **V4**; reconciliation routes to outermost `model_rebuild()` warm-up.
- **I5 egress morphism stack** — hook serializers, alias serializers, field serializers, owner wrap per mutual-embedding precedence; duplicate encoding surfaces are **V5**; reconciliation collapses to one layer per slot.
- **I6 witness row coverage** — every production seam has lattice row; orphan path is **V6**; reconciliation appends row before resume.
- **I7 CLI-HTTP vocabulary bijection** — cyclopts choices biject to OpenAPI discriminant mapping when `cli_http_discriminator_source` applies; skew is **V7**; reconciliation regenerates from `TypeAdapter.json_schema()`.
- **I8 promotion unit atomicity** — version bumps, altitude moves, alias edits, hook changes, matrix edits, wrap chain edits, and discriminator vocabulary changes land as one unit; partial landing is **V8**; reconciliation rollbacks entire unit.

# Cross-Wave Seam Composition Rows

- Chained pipelines compose witness rows without skipping gates — federation documents typed step sequences beside root module graph.
- `settings_boot_to_scope` — `BaseSettings` singleton from owning doctrine layers; downstream `none` on domain; S4 single-pass construction with source-order matrix.
- `foreign_bytes_to_ingress` — `model_validate_json` or `TypeAdapter.validate_json`; altitude row on ingress owner; S3 validation-mode snapshot when publishing.
- `ingress_to_canonical` — `materialize_*` from altitude admission doctrine; altitude handoff row; S2 forbids ingress type in domain signatures.
- `canonical_interior_transition` — `model_copy` / `copy.replace` per tier law from shape ownership doctrine; trust tier documented on transition method.
- `alias_stack_admission` — `Annotated` alias from annotated constraint stack on field or adapter; S4 oracle unwraps functional validator chain order.
- `cooperative_slot_admission` — hook `chain_schema` from cooperative chain schema hooks; S4 per `handler.field_name` branch; S3 dual-mode when hook widens ingress.
- `graph_cycle_admission` — enclosing rebuild from mutual embedding owner egress calculus; matrix row per arm; S4 mutual-coherence strategy on outermost owner.
- `multi_layer_wrap_egress` — `nested_wrap_chain` row; S5 metamorphic includes outermost wrap; S3 serialization-mode diff mandatory.
- `project_wire_to_msgspec` — cross-engine row from shape ownership doctrine; S5 per arm when polymorphic; dump mode from struct annotations.
- `cli_and_http_ingress` — `cli_http_discriminator_source` row; S3 OpenAPI plus generated cyclopts artifact; per-arm negative fixtures.
- `store_replay_to_current` — version arm from shape ownership doctrine; migration fold at boundary; S4 replays full validator law on stored bytes.
- No step permits raw dict promotion, dump surgery projection, or unmapped `ValidationError` past root capture — chain restart after violation always re-enters at `foreign_bytes_to_ingress` or `store_replay_to_current`.

# Promotion Unit Federation Law

- Federated promotion unit is the sole atomic merge vehicle across waves — partial field edits without row regeneration are equilibrium violations.
- Unit bundles: dependency pin when `pydantic` minor changes; dual-mode schema snapshots for every affected publishing owner; core-schema oracle loops through `resolve`; cross-engine metamorphic rows when seams move; matrix regeneration when graph arms change; `nested_wrap_chain` revision when wrap layers change; `cli_http_discriminator_source` regeneration when discriminant vocabulary changes; mypy plugin pass on root import graph; source-order matrices when settings move.
- Altitude promotion or demotion from altitude admission doctrine updates `materialize_*` return types, seam map, and altitude witness rows in the same unit — not isolated owner moves.
- Alias metadata tuple changes from annotated constraint stack regenerate every indexed embedding owner and S3 snapshots — `definition-ref` stability does not excuse skipping snapshot diff.
- Cooperative hook fingerprint changes from cooperative chain schema hooks regenerate per `handler.field_name` oracle branches and cooperative witness rows.
- Graph matrix edits from mutual embedding owner egress calculus regenerate per `arm_id` samples, negative fixtures, and OpenAPI discriminator tables.
- Breaking wrap chain layer addition is breaking for egress even when field annotations unchanged — serialization-mode S3 diff mandatory.
- Obsolete version arms remain in root migration modules with exhaustive folds — active owners expose current arm only; `assert_never` witnesses retired layouts.
- Rollback-first policy — consuming contexts do not forward-fix when vocabulary or warm-graph unit fails; full unit revert per **V8** reconciliation.

# Dual-Engine Federation At Root

- Root declares federated conformance map — one cross-engine row per Pydantic→msgspec seam the bounded context exports; multi-layer wrap output feeds `project_wire` only when row documents outermost wrap participation.
- Module-level `Encoder` / `Decoder` and `TypeAdapter` constants share root `[CONSTANTS]` — S5 uses production singletons; per-request adapter construction is federation defect **V4** analog on warm graph.
- Ingress stays Pydantic where settings, discriminated unions, computed wire fields, or JSON Schema-rich contracts apply; egress stays msgspec for volume lanes — duplicate engine owners without row are **V6** violations.
- `CodecFault` projection unifies Pydantic and msgspec failures at root capture — domain interiors never import either validation exception type.
- Tagged union and `SerializeAsAny` seams prove at root before cache write — S5 encodes polymorphic width mismatches validation alone misses.
- Trusted-replay rows document encoder identity, schema version, and pinned decoder module path — federation rejects ambient pickle replay without version gates.

# Version Arm Federation And Migration Totality

- `schema_version: Literal[...]` on canonical, envelope, or graph owners is federated version law — version appears on every witness row indexing persisted layouts.
- Read-path migration folds run once at boundary altitude — `StoredVn` → `model_validate` → migration expression → current owner; domain modules after fold see only current shape.
- Breaking changes across waves aggregate into one version bump when stores embed affected layouts — discriminant vocabulary, alias routing, hook construction, matrix arm law, wrap envelope keys, and settings source order each trigger bump independently or jointly per impact analysis.
- Migration folds prefer `model_validate` over `model_construct` unless trusted-replay row proves legality — version skew requires full validator replay.
- Multi-layer wrap envelope key changes are breaking for egress consumers and stored bytes keyed on wire layout — migration maps legacy envelope keys when wrap chain row documents rename.
- CLI-HTTP discriminant rename bumps version when persisted CLI scripts or stored HTTP bodies key on legacy tags — `cli_http_discriminator_source` row updates with migration fold mapping legacy tags.

# Matrix And Witness Table Governance

- Graph families from mutual embedding owner egress calculus maintain matrix tables — federation extends governance to all witness row kinds in the lattice; tables are frozen artifacts beside root module.
- Table edit protocol: propose row change → classify breaking versus refactor → attach promotion unit id → regenerate indexed proof layers → update fan-out importer parity tests → commit table and code atomically.
- `arm_id` and `row_id` slugs are stable across releases — ordinal indices alone fail governance; slugs index conformance samples and operator diagnostics.
- Negative fixture ids index inadmissible combinations — mutual cycles, multi-layer wrap cardinality violations, and orphan CLI choices each carry negative samples.
- Witness table revision drift triggers temporal reconciliation — composition root schedules re-sync before accepting new canonical input on affected edges.
- Importing adapters assert row parity via parametrized contract tests — mismatch triggers **V6** or **V7** and rollback-first reconciliation.

# CI Federation Gates

- Root import smoke warms every owner, wrap chain, graph matrix, and pinned adapter — unresolved forward refs block merge before generative suites.
- Federated replay runs S1–S6 per promotion unit scope — skipped layer without waiver row blocks merge.
- Dual-mode schema diff on every publishing family — single-mode diff is under-approximation defect.
- Generated cyclopts choice artifacts diff beside `cli_http_discriminator_source` when unification applies — hand-maintained CLI enums fail gate.
- `nested_wrap_chain` metamorphic tests compare outermost `model_dump(mode='json')` against serialization-mode schema — mid-layer wrap participation documented per row.
- Matrix parametrized tests cover every `arm_id` and every `row_id` with positive and indexed negative fixtures — missing coverage is exhaustiveness defect distinct from happy-path sampling.
- `pydantic` minor upgrades pin behind full federated promotion unit — compilation success alone does not discharge merge.
- Free-threaded CI warms graph and wrap owners once in session scope — post-collection rebuild races without root-ordered warm-up.

# Reconciliation Signals And Rollback Posture

- Violation signals **V1**–**V8** map to default reconciliation rows — federation at single bounded-context scope uses rollback-first without multi-context quarantine choreography.
- **Rollback-first row** — **V3**, **V6**, **V7**, **V8** on active promotion unit — full atomic unit revert; no forward-fix on consuming module until witness table and schema snapshots regenerate together.
- **Seam repair row** — **V5** duplicate encoding at one foreign pair — collapse to precedence law from mutual embedding owner egress calculus; patch liable adapter module only; domain interiors excluded from repair edit set.
- **Thin projection row** — **V2** duplicate altitude enforcement — demote secondary owner to ingress-only or projection mirror in same promotion unit; canonical owner retains durable law.
- **Warm-graph repair row** — **V4** analog when root `TypeAdapter` predates promoted owner body — re-register pinned adapter at root `[CONSTANTS]` and replay S4–S5 before resume.
- Forward-fix permitted only when rollback orphans production store bytes and migration fold proves active read-path on obsolete version — liable owner documents store key set, sunset date, and waived proof layers in arbitration witness row.
- Signal payloads carry `invariant_id`, `liable_owner_module`, `default_reconciliation_row`, and `proof_layers_to_replay` — undocumented signal kind blocks merge.

# Validation Failure Federation At Root

- Graph, multi-layer wrap, alias, cooperative, and discriminated-union failures concatenate branch errors with distinct `loc` prefixes — root capture maps every violation through one projection function; domain interiors never import `ValidationError`.
- Union failures must not assume single-root `loc` — batch ingress surfaces all violations; operator logs truncate after mapping without dropping discriminant, `arm_id`, or `row_id` identity fields.
- `PydanticCustomError` types from validators and hooks map through identical `loc`+`msg` law — domain-coded admission failures survive root distillation without wave-specific special cases.
- `validate_call` failures prefix `loc` with parameter names — CLI and HTTP consumers see one violation vocabulary when `cli_http_discriminator_source` applies.
- Settings construction failures map to startup faults at root boot — lazy raises inside domain rails past the `Result` boundary are federation defects.
- HTTP 422 and automation bodies use `ValidationError.json()`-shaped output from root adapters when machine-readable paths are required — interior modules consume mapped violation tuples only.

# Worker Boot And Cross-Process Federation

- Worker entrypoints re-import root module graph or documented subset warming every federated owner, wrap chain, matrix table, and pinned adapter — compiled validator identity is process-local.
- `model_rebuild()` on outermost graph and multi-layer envelope owners runs in worker boot when shared packages import cooperative arms after partial initialization — federation parity proofs execute after rebuild, not on cold `annotationlib.Format.STRING` reads.
- Generic federated adapters cache per concrete argument tuple at root `[CONSTANTS]` — workers import pinned symbols, not parametrization expressions at call sites.
- Cross-process handoff prefers `model_dump` + `model_validate` when `schema_version` enforcement matters — wrap, hook, and alias serializer law replays only on full re-admission.
- `BaseSettings` construction completes before worker threads read frozen settings — lazy accessors on hot paths race under parallel importers.
- Cross-process schema parity for forked workers does not yet prove generic parametrization closure in one CI gate — witness rows and adapter pins must align per argument tuple.

# Diagnostic Binding On Federated Faults

- Admission failures at root distill into envelope fault slots with `validation:` or `config:` stage prefixes on `failing_step` — raw `ValidationError` strings do not cross into domain `Result` interiors.
- Matrix `arm_id` and lattice `row_id` labels appear in operator diagnostics when capture modules annotate discriminated or seam failures — truncation preserves identity fields required for replay and dispatch.
- Multi-layer wrap faults annotate `chain_id` and failing layer symbol when wrap coherence fails — mid-layer envelope violations do not collapse to generic dump errors.
- Cap truncation on fault hints preserves `loc` prefixes and discriminant identity — verbose context drops, not routing fields.

# Settings Federation At Root Boot

- `BaseSettings` is composition-root altitude exclusively — federated settings rows index fan-out `project_*_config` slices, source-order precedence, and env collision matrices.
- Settings owners skip msgspec wire round-trip unless they project to volume egress — proof is single-pass root boot construction with frozen `extra='forbid'`.
- `SecretStr` / `SecretBytes` never cross logging, public OpenAPI, or root diagnostic dumps — settings JSON Schema is operator-facing only.
- Nested settings submodels validate independently at parent construction — piecemeal nested construction in domain modules bypasses namespace closure and is **V2** altitude violation.
- `settings_customise_sources` ordering is pinned deployment contract — breaking reorder requires federated promotion unit with collision matrices, not isolated source edits.

# Annotationlib Federation Parity

- `annotationlib.get_annotations(owner, format=annotationlib.Format.VALUE, include_extras=True)` is mandatory parity oracle on every federated owner after `model_rebuild()` — mismatches signal deferred-evaluation drift, missing rebuild, or post-hoc field injection.
- `include_extras=False` in parity proofs collapses `Annotated` stacks to bare `T` — false-green on alias-wrapped cooperative embeddings is federation defect.
- PEP 695 owners and aliases bind type parameters at definition — federated witness rows record concrete argument tuples on adapters and persistence keys.
- `pydantic.mypy` plugin alignment runs in S1 on the same module set root imports — models outside root graph are orphan shape owners.

# Trust Tier Federation Across Waves

- Tier law from shape ownership doctrine federates across alias, cooperative, graph, and multi-layer owners — no doctrine layer weakens tier posture on boundary-facing fields.
- Tier V re-admission applies when foreign carriers replace any federated slot — full `model_validate` replays alias stacks, hook `chain_schema`, graph discriminant law, and outermost wrap does not substitute for admission.
- `SkipValidation` on trusted interior slots requires trusted-replay rows — mutual cycles and multi-layer graphs do not exempt boundary-facing fields from tier law.
- Trusted same-owner transitions use `model_copy` / `copy.replace` only when base instance passed full validation at materialization — document tier on owning transition methods across waves.
- Child re-admission before parent replace validates nested dicts through child schema before `model_copy(update=...)` — applies uniformly to alias, cooperative, and graph child slots.
- Pickle and `copy.deepcopy` bypass version gates — dump+validate re-admission remains mandatory at cross-process seams unless trusted-replay rows document encoder and schema version pins.

# JSON Schema Federation Across Projections

- Dual-mode snapshots are federated contract artifacts — validation mode documents ingress; serialization mode documents egress after hook serializers, field serializers, owner wrap layers, and computed fields.
- `GenerateJsonSchema` subclass hooks customize ref naming and union representation at compile time — federation prefers per-family hooks over post-processed dict surgery on recursive `definition-ref` graphs.
- `$ref` deduplication and deterministic set ordering stabilize diffs — ref churn without constraint churn requires separate review from semantic churn during promotion units.
- `WithJsonSchema` and `json_schema_input_type` overrides stay on alias or cooperative export symbols — federation rejects post-hoc `model_json_schema()` patches replacing hook projections.
- OpenAPI for federated graphs derives from enclosing owner declarations plus cooperative JSON hooks — hand-maintained parallel documentation fails CI when root schema export is enabled.
- When ingress Pydantic shapes differ from msgspec egress tags, OpenAPI reflects ingress discriminators — wire-only tags live in cross-engine and `cli_http_discriminator_source` rows, not conflicting public vocabularies.

# Root Composition Harness Federation

- Harness layers S1–S6 stack orthogonally at root — federation schedules full replay per promotion unit; partial harness pass is not merge evidence.
- Discriminated union and graph matrix families sample at least one generated instance per arm and per `row_id` through root CI — missing arm or row coverage is exhaustiveness defect.
- `TypeAdapter`, settings singletons, wrap-chain owners, and matrix tables warm in test session fixtures mirroring production boot — cold-start compilation failures surface in CI, not first request.
- JSON Schema snapshots are not ingress fixture generators when validators widen shapes beyond annotations — S4 oracle loops and explicit admission fixtures own generative truth; S3 snapshots own published contract truth.
- Plugin hosts registering new model classes after root import require explicit federation registration hook — dynamic class bodies do not refresh witness tables without documented warm-up protocol.

# Collapse Tests For Federation Defects

- Production seam without witness row — collapse to lattice row append in same promotion unit as code change.
- Multi-layer egress with undocumented wrap order — collapse to `nested_wrap_chain` row with leaf hook serializers and envelope-only wraps.
- Cyclopts choice enum parallel to Pydantic discriminator — collapse to `cli_http_discriminator_source` with generated artifact from `TypeAdapter` export; field-kind from `json_schema` mapping, callable-kind from `core_schema` tagged-union choices, never hand tuples on route modules.
- Callable discriminator vocabulary inferred from OpenAPI alone — collapse to `tag_extraction_oracle: core_schema_choices` because validation-mode JSON Schema omits `discriminator.mapping` on callable unions.
- Empty cyclopts help choices on embedded discriminated union parameters — collapse to generated artifact or `StrEnum` elevation row; do not assume `get_choices_from_hint` walks `BaseModel` arm literals under cyclopts 4.16.
- Per-wave proof passing while sibling layer fails — collapse to federated S1–S6 replay order; fix failing layer before resume.
- Partial promotion landing on alias without embedding regeneration — collapse to full promotion unit rollback per **V8**.
- Interior `ValidationError` catch — collapse to root capture and violation tuple mapping.
- Three-plus owner wrap layers with `@field_serializer` re-encoding same keys — collapse to precedence law from mutual embedding owner egress calculus and wrap chain row.
- Domain import of ingress, alias, or graph envelope types — collapse to canonical owner plus `materialize_*` return type.

# Anti-Patterns

- Undocumented production seams; ad hoc wrap order across nested owners; hand-maintained cyclopts enums beside model discriminators; per-wave proof without federated replay; partial promotion landing; orphan cooperative arm validation; dump surgery between wrap layers; `include_extras=False` in federation parity proofs; forward-fix without migration fold when rollback viable; settings construction outside root boot; per-request `TypeAdapter` over federated adapters.
