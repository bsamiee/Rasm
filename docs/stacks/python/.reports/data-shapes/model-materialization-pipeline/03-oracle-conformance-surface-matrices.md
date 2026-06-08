# Oracle Conformance Surface Matrices

# Critical Signals

- Every production-published external layout admits one frozen `ORACLE_CONFORMANCE` row binding `surface_id`, primary oracle, `oracle_symbol`, field-policy projection, lattice foreign keys, and CI gates — metamorphic and consumer proof consumes row columns as layout authority; hand-maintained schema prose beside row refs fails admission.
- Oracle singularity is per-surface invariant — Pydantic `json_schema`/`core_schema` owns ingress OpenAPI; `msgspec.inspect` owns stdout, cache, probe, and migration stored layouts; mixing oracles on one surface is indexed negative `mixed_oracle_surface` before runtime suites execute.
- `FIELD_POLICY` is the sole rename, alias, cap, omit, and discriminant vocabulary owner — surface rows project column subsets; adapter magic numbers and undocumented alias drift fail `cap_constant_duplication` and `alias_drift_without_row` at compiled layer.
- Matrix is proof and CI policy only — interior domain modules must not import `ORACLE_CONFORMANCE` or field-policy tables as runtime routing authority; composition root registers rows; boundary law modules and parametrized harnesses consume them.
- Oracle conformance surface matrices own surface row shape, oracle taxonomy, field-policy join, lattice cross-links, harness layer map, and promotion atomicity — static–runtime decode-planes doctrine, handoff transition contract lattice, attribution smoke evolution calculus, and pipeline equilibrium closure calculus cite surface tables by machine-table foreign key without restating surface bodies.

# Surface Taxonomy

- Admitted consumer surfaces are closed vocabulary — `openapi_ingress`, `stdout_envelope`, `cache_bytes`, `settings_env`, `migration_stored`, `probe_fingerprint`, `history_replay`, `subprocess_child_stdout` — not free-form module names.
- Each surface binds exactly one primary compiled oracle — Pydantic `TypeAdapter.json_schema()` / `core_schema` for ingress OpenAPI; `msgspec.inspect.type_info` for stdout wire, cache bytes, and migration stored structs — mixed oracles on one surface fail matrix admission.
- Static plane supplies checker contracts (`TypedDict`, `Unpack`, PEP 695 aliases) that oracles must reflect — static assignability without compiled-oracle row is incomplete coverage.
- Runtime plane supplies metamorphic witnesses on decoded instances — oracles prove layout and constraint law; metamorphic laws prove bijection on polymorphic slots — neither substitutes for the other on the same surface.
- Surfaces that never cross process boundaries still require rows when external tooling consumes them — CLI stdout and probe cache keys are first-class surfaces even when no HTTP client exists.

# Conformance Row Shape

- Every admitted surface declares one frozen row: `surface_id`, `primary_oracle`, `oracle_symbol`, `concept_owner`, `projection_family`, `field_policy_refs`, `mode`, `lattice_transition_ids`, `negative_fixture_ids`.
- `surface_id` is a stable snake slug matching surface taxonomy — not ordinal numbers or stage names alone.
- `primary_oracle` is one of `{pydantic_json_schema, pydantic_core_schema, msgspec_type_info, msgspec_field_table, annotationlib_value}` — secondary oracles attach via sub-rows, not overloaded primary fields.
- `oracle_symbol` pins the module-level product — `TypeAdapter[Params].json_schema()`, `_ENVELOPE_STRUCT`, `msgspec.inspect.type_info(WireRow)` — per-request schema builds are row violations.
- `concept_owner` names the canonical runtime or compiled type the surface exposes — one concept per row; parallel `FooOpenAPI` and `FooWire` rows link through `concept_owner=Foo` sub-lattice, not duplicate concept slugs.
- `projection_family` draws from `{ingress, domain, wire}` per ingress-to-materialization doctrine — OpenAPI rows tag `ingress`; stdout and cache rows tag `wire`; domain rows exist only for interior snapshot exports explicitly published.
- `field_policy_refs` is a frozenset of `concept_slug` keys into `FIELD_POLICY` — alias, `serialization_alias`, rename, omit, `Meta(max_length=...)`, `tag_field` columns project per surface; duplicated cap constants in adapters fail conformance smoke.
- `mode` is one of `{validation, serialization, dual}` — dual-mode aliases require paired sub-rows `_validation` and `_serialization` when serializer stacks diverge wire from Python-runtime dump shapes.
- `lattice_transition_ids` links matrix rows to handoff transition contract lattice handoffs — cross-family projection rows must cite `pydantic_validated_to_msgspec_wire` and `materialization_exit_to_wire_projection` when both surfaces publish one concept.
- `negative_fixture_ids` index harness anti-patterns — `mixed_oracle_surface`, `dump_schema_surgery`, `alias_drift_without_row`, `cap_constant_duplication`.
- Optional columns: `proof_required: bool`, `encoder_symbol`, `decoder_symbol`, `schema_version_literal`, `discriminant_owner`, `alignment_sub_row_ids: frozenset[str]`, `promotion_unit_id` when row is mid-flight.

# Field Policy Row Shape

- Central `FIELD_POLICY: tuple[FieldPolicyRow, ...]` declares per-concept field law once beside root codec module — surfaces project subsets; duplicated cap constants in adapters without policy linkage fail conformance smoke.
- `FieldPolicyRow` frozen record columns: `concept_slug`, `canonical_name`, `ingress_alias`, `wire_rename`, `openapi_doc`, `cap_max_length`, `omit_on_wire`, `tag_string`, `discriminant_literal`, optional `serialization_alias`, optional `validation_alias_choices`.
- `concept_slug` links all surface rows publishing one logical field — OpenAPI ingress row and wire stdout row cite same slug; orphan field on one surface without policy row fails `field_policy_surface_join` in pipeline equilibrium closure calculus.
- `ingress_alias` and `validation_alias_choices` appear on OpenAPI and `settings_env` projections only — wire rows read `wire_rename`, `cap_max_length`, `omit_on_wire`, `tag_string`.
- `discriminant_literal` and `tag_string` share `discriminant_owner` slug — adapters map external synonyms only at ingress and egress lattice rows; interior `match` reads canonical tokens from policy owner.
- Intentional cross-surface divergence documents explicit column presence — when Pydantic `json_schema()` and msgspec inspect disagree on shared logical field, promotion unit adds `projection_conformance` diff row naming winning surface per column.

# Projection Conformance Row Shape

- Cross-family concepts admit `projection_conformance` surface row when ingress and wire surfaces both publish one `concept_slug` — primary oracle is diff table, not third engine.
- `ProjectionConformanceRow` extends base surface row with `ingress_surface_id`, `wire_surface_id`, `predicted_renames: frozenset[str]`, `predicted_omits: frozenset[str]`, `positive_fixture_id`, `negative_fixture_ids` including `dump_schema_surgery`.
- Positive fixture: one-expression `msgspec.convert` or adapter projection from validated ingress model to wire struct — field-policy table predicts rename and omit outcomes; metamorphic law asserts bytes after encode.
- Negative fixture: `model_dump` → key surgery → `Struct(**d)` — fails projection conformance and `pydantic_validated_to_msgspec_wire` lattice row together.
- JSON Schema export never substitutes for wire oracle on cache or stdout surfaces — CI fails `mixed_oracle_surface` when handlers document wire layout via OpenAPI types alone.

# OpenAPI Ingress Surface Rows

- `openapi_ingress` primary oracle: Pydantic `TypeAdapter.json_schema()` or model `model_json_schema()` on ingress family types — OpenAPI consumers align with compiled Pydantic graph, not msgspec wire layout.
- Field names on OpenAPI reflect `validation_alias` / `AliasChoices` admission path — `serialization_alias` and alias serializers belong on egress sub-rows, not silently merged into ingress schema export.
- Discriminated unions export via Pydantic `oneOf` / `discriminator` nodes — msgspec `tag_field` strings appear on wire surface rows only; ingress OpenAPI maps external synonyms through adapter alias policy declared once in field-policy table.
- Settings ingress sub-row `settings_env` shares Pydantic oracle but adds `validation_alias` collision matrix for env keys — settings schema version is independent of envelope `schema_version` unless composition root couples them in paired rows.
- Callable `Discriminator` and cyclopts choice metadata not yet unified — dual vocabulary maintenance persists until root publishes single generative export; matrix row still pins `oracle_symbol` on `TypeAdapter` until unification lands.
- OpenAPI diff tests run on pydantic-owned ingress changes in the same promotion unit as field-policy table updates — wire-only tag changes do not satisfy OpenAPI row obligations alone.

# Stdout And Envelope Wire Surface Rows

- `stdout_envelope` primary oracle: `msgspec.inspect.type_info` on closed envelope struct — field order, `Meta(max_length=...)`, and `schema_version` literal closure are compiled-oracle evidence.
- Cap metadata on envelope and nested `Diagnostic` rows reads from inspect `Meta` — `_HINT_CAP`, `_MESSAGE_CAP`, and sibling caps are derived constants in composition root, not adapter magic numbers duplicated without row linkage.
- Pass-one consumer gate fields (`schema_version`, `claim`, `verb`) appear on envelope oracle before polymorphic body slots — body `Decoder` arms have separate detail surface rows keyed by discriminant vocabulary shared with domain owner.
- `truncated=True` is a wire-surface field on envelope oracle — consumers treat truncated tuples as summaries; full evidence resolves through history artifacts keyed by `run_id`, not elongated wire rows.
- Deterministic encode (`order="deterministic"`) is part of stdout surface contract when automation parses bytes — oracle row metadata pins encoder singleton identity beside layout oracles.
- Subprocess child stdout reuses `stdout_envelope` row when child is admitted rail — foreign tool bytes stay opaque unless envelope row is explicitly adopted for that integration.

# Cache And Probe Surface Rows

- `cache_bytes` and `probe_fingerprint` primary oracle: same msgspec struct as stdout egress when store keys derive from deterministic encode — dual-encoding fixtures (history JSON vs msgspec bytes) fail `cache_bytes` negative `dual_dump_persist`.
- Probe cache keys pin encoder module path and `schema_version` beside layout oracle — changing either invalidates prior keys without documented re-key row in promotion unit.
- Write path row pairs with handoff transition contract lattice `cache_write_after_proof` — polymorphic detail slots set metamorphic proof flag on matrix row metadata.
- Read path row pairs with `cache_read_trusted_replay` — oracle documents stored struct version literal; migration sub-row handles `msgspec.convert` hop without re-deriving layout from OpenAPI ingress schema.
- Merged cache staging dicts exist only inside boundary persistence helpers — matrix does not admit durable dict surfaces; immediate re-encode row closes the seam.

# Detail And Polymorphic Body Surface Rows

- Polymorphic `detail` slots admit one surface row per closed variant arm plus umbrella `detail_union` row — umbrella row references discriminant owner and lists admitted tag literals; arm rows pin struct layout oracles independently.
- Success-path detail assumes round-trip proof at root egress — detail surface metadata cites `_validated` lattice row; metamorphic laws draw from closed families admitted by `_DETAIL_DECODER` oracle symbol.
- Fault-path detail uses `Diagnostic` wire shape oracle — same cap policy as envelope row; distillation fields (`failing_step`, `recent_events`, `elapsed_ms`) appear on diagnostic surface, not duplicated on OpenAPI ingress rows.
- Mutual exclusivity of `report` and `error` envelope slots is runtime invariant on envelope oracle — static payloads do not express exclusivity without promotion to compiled owners; matrix documents slot policy, not domain branching.
- Optional detail slots declare explicit `None` policy on detail surface row — consumers do not default missing detail to success variant unless envelope status and row metadata declare optional polymorphism.

# History Replay And Delta Surface Rows

- `history_replay` primary oracle: same msgspec envelope struct as `stdout_envelope` — bytes written by `_encode` must decode through `_ENVELOPE_DECODER` with layout parity proved in history persist smoke.
- `RunSnapshot` and delta compare surfaces consume materialized envelope structs — field access derives from stdout oracle; key sets for added/removed counts come from struct fields, not dict projections of wire JSON.
- Missing history sides fold to explicit `EMPTY` status in delta notes — surface row documents absence encoding; `None` coercion on snapshot scalars is negative fixture on delta consumer row.
- History retention failures are best-effort at root — surface row does not admit alternate history wire shapes when persistence fails after stdout emit.

# Migration And Stored Egress Rows

- `migration_stored` primary oracle: msgspec inspect on each pinned version struct in migration table — one row per stored version literal, plus current egress row linked by ordered hop sequence at composition root.
- Breaking wire changes require new version literal row and migration fold — field renames and discriminant moves are version events, not silent `BeforeValidator` patches reflected only in runtime behavior without oracle update.
- Obsolete variant arms survive in migration oracle tables with exhaustive `match` — current egress oracle excludes retired tags; domain modules after migration see only current vocabulary.
- Multi-hop history replays declare ordered row sequence — chained hand-rolled copies in leaf modules are negative `dump_schema_surgery` on migration surface.

# Cross-Surface Divergence Law

- Surfaces project subsets from `FIELD_POLICY` — OpenAPI row includes ingress aliases and validation constraints; wire row includes rename, cap, tag, and omit flags; intentional divergence is explicit column presence, not accidental dump drift.
- `serialization_alias` on Pydantic aliases affects egress sub-rows only when alias serializers transform wire keys — conformance sub-row `_serialization` documents dump mode (`python` vs `json`) per projection seam from pydantic-domain doctrine.
- Discriminant vocabulary sub-row declares one owner for literals, `StrEnum` values, and msgspec `tag` strings — matrix rows reference `discriminant_owner` slug; adapters map external synonyms only at ingress and egress lattice rows.
- When Pydantic `json_schema()` and msgspec inspect disagree on a shared logical field, promotion unit adds explicit `projection_conformance` divergence row documenting which surface wins — silent preference for either oracle is rejected.
- Domain interior types do not receive surface rows — rich owners expose snapshots through adapter projection; only published surfaces appear in matrix.

# Alignment Sub-Row Binding

- Every surface row expands to three alignment sub-rows per core-schema phase morphism law: `_alignment_static`, `_alignment_compiled`, `_alignment_runtime` — static sub-row names payload or alias; compiled sub-row names `oracle_symbol`; runtime sub-row names decoded handoff artifact.
- Static sub-row proves checker assignability and `TypeIs` narrowing targets — compiled sub-row snapshots oracle output; runtime sub-row names metamorphic decode product.
- Promotion path sub-rows enforce unidirectionality — reverse projection sub-rows list omit, rename, and cap fields explicitly; accidental dump drift is negative `alias_drift_without_row`.
- Annotationlib `Format.VALUE` with `include_extras=True` on alias owners must agree with compiled oracle field constraint nodes — static-only proof without compiled snapshot is incomplete row coverage.

# Partial Stream And Settings Surface Rows

- Partial ingress sub-row documents `experimental_allow_partial` admission on Pydantic validation oracle only — downstream construction surface rows annotate intentional partial closure; mixing partial oracle with full domain invariant surfaces without row split is negative `partial_full_invariant_mix`.
- Streaming byte feeds complete to `bytes` before msgspec oracle applies — partial buffer carriers do not receive wire layout oracle until frame completion row closes at adapter boundary.
- `settings_env` row adds env-key collision matrix and frozen settings round-trip when settings feed cache keys — proof sub-row cites `settings_snapshot_roundtrip` lattice id; settings schema version independent of envelope unless root couples in paired metadata.

# Metamorphic Law Registry Coupling

- Surface rows with `proof_required=true` metadata bind metamorphic laws to `oracle_symbol` and production encoder singleton — shadow encoders in test helpers fail reference-identity checks tied to `surface_id`.
- `@spec` and `register_law` associate witnesses with root codec functions named in lattice rows — surface parametrized tests import production oracle symbols, not parallel schema builders.
- Subset bijection on wire surfaces declares excluded fields — computed-on-materialize, wire-omitted, and cap-truncated slots document rationale beside surface row; full structural equality is not implied by row admission alone.
- Encoder determinism laws on `cache_bytes` and `probe_fingerprint` pin `order="deterministic"` — metamorphic bytes equality uses production encoder instance named in surface row metadata.

# Harness Binding And Proof Order

- Static layer: import architecture, field-policy exhaustiveness, surface registry completeness — every production-published surface has matrix row before merge.
- Compiled layer: oracle snapshot diff (`json_schema` hash, `msgspec.inspect` field-order table), singleton identity smoke on `oracle_symbol` — per-request schema builds fail here.
- Runtime layer: metamorphic round-trip on surfaces with `proof_required` metadata, consumer smoke decoding stdout bytes through pinned decoder — draws from closed variant families on detail surface rows.
- Surface diff tests parametrize on `surface_id` — missing row when adapter publishes new external contract is merge blocker.
- Lattice join table maps `surface_id` ↔ `transition_id` for cross-check — harness modules must join via explicit lookup, not implicit name equality between stage tags and surface slugs.

# Surface Registry Shape

- Root `ORACLE_CONFORMANCE: tuple[SurfaceRow, ...]` extends ingress-to-materialization projection family law — every production-published layout binds exactly one `SurfaceRow` plus optional alignment and projection-conformance sub-rows; orphan surfaces without rows fail merge before parametrized oracle suites execute.
- `SurfaceRow` is a frozen record beside root codec module — `surface_id`, `primary_oracle`, `oracle_symbol`, `concept_owner`, `projection_family`, `field_policy_refs: frozenset[str]`, `mode`, `lattice_transition_ids`, `negative_fixture_ids`, optional `proof_required`, `encoder_symbol`, `decoder_symbol`, `schema_version_literal`, `discriminant_owner`, `alignment_sub_row_ids`, `promotion_unit_id`.
- `AlignmentSubRow` attaches per surface when cross-family or stage-skipping edges publish — `sub_row_id` suffixes `_alignment_static`, `_alignment_compiled`, `_alignment_runtime`; static names payload or alias; compiled names `oracle_symbol`; runtime names decoded handoff artifact type expression.
- `OracleSnapshotFixture` pairs each `surface_id` with compiled-oracle hash or field-order table — `json_schema` hash for Pydantic surfaces; `msgspec.inspect` field-order tuple for wire surfaces; snapshot rotation without source change attributes to field-policy drift, not runtime decode faults.
- Registry build is import gate — composition root import proves `FIELD_POLICY` exhaustiveness for every capped or aliased field referenced by surface rows; missing policy row blocks CI before metamorphic replay.
- Duplicate surface declarations — test module re-declares oracle slugs parallel to owner registry — collapse to single parametrized import from composition-root `ORACLE_CONFORMANCE` per steady-state policy in pipeline equilibrium closure calculus.

# Surface-Lattice Join Catalog

- `SURFACE_LATTICE_JOIN: tuple[SurfaceLatticeJoinRow, ...]` maps `surface_id` ↔ `transition_id` for cross-check — harness modules join via explicit lookup; implicit name equality between stage tags and surface slugs fails closure in pipeline equilibrium closure calculus.
- `stdout_envelope` ↔ `serialization_exit_to_consumer_decode`, `encode_to_stdout_bytes`, `envelope_egress_wrap` — decoder and encoder symbols on surface row must alias lattice `owner_symbol` metadata.
- `cache_bytes` ↔ `cache_write_after_proof`, `cache_read_trusted_replay`, `merged_cache_row_reencode` — `encoder_symbol` and `schema_version_literal` on surface row must match lattice trusted-replay pins.
- `openapi_ingress` ↔ `ingress_carrier_to_validation_exit`, `params_bind_to_bound_or_fault` when `Bind.params_type` exposes public schema — each admitted params type cites ingress surface row or documented exemption registry entry.
- `migration_stored` ↔ ordered `version_migration_single_hop` sequence — surface rows declare version literal per hop; multi-hop chains declare ordered row list at composition root, not chained copies in leaf modules.
- `detail_union` and arm rows ↔ `detail_proof_validated`, `consumer_detail_narrow` — polymorphic slots with `proof_required=true` on lattice must cite matching detail surface rows with metamorphic law binding.
- `projection_conformance` ↔ `pydantic_validated_to_msgspec_wire`, `materialization_exit_to_wire_projection` — triple row set required when one `concept_owner` publishes ingress and wire surfaces; missing leg fails E3 closure in pipeline equilibrium closure calculus.
- Join negative: wire layout documented only on lattice row without `stdout_envelope` or `cache_bytes` surface row — fails `mixed_oracle_surface` and `transition_surface_join` jointly.

# Stage-Map Versus Surface Role Map

- Materialization stage names from ingress-to-materialization doctrine and surface taxonomy slugs align but are not interchangeable identifiers — surface rows carry `stage_tags: frozenset[str]` and `projection_family` when layout crosses family boundaries.
- Harness tables naming only stages or only `surface_id` without join row produce false-green coverage — CI contract modules must join stage-map exhaustiveness to surface rows via `SURFACE_LATTICE_JOIN`, not prose proximity.
- Ingress projection surfaces tag `projection_family=ingress` on `openapi_ingress` and `settings_env`; wire projection surfaces tag `wire` on `stdout_envelope`, `cache_bytes`, `probe_fingerprint`, `history_replay`; domain snapshot exports tag `domain` only when interior layout is explicitly published to external tooling.
- Envelope pass-one fields appear on `stdout_envelope` oracle before polymorphic body slots — body detail surfaces cite discriminant vocabulary shared with domain owner; stage tag `emit` annotates envelope wrap rows without replacing `surface_id`.
- Interior rich owners do not receive surface rows when snapshots flow through adapter projection only — `projection_family=domain` rows exist solely for published exports; stage tag `materialization` does not auto-admit domain surface rows.

# CI Enforcement And Registry Coupling

- `ORACLE_CONFORMANCE` lives beside root codec module as frozen tuple — adapters and OpenAPI publishers import `surface_id` rows; tests parametrize on surface slug; prose in ingress-to-materialization doctrine through handoff transition contract lattice remains commentary, not second source of truth.
- Adding production-published layout without matrix row and oracle snapshot fixture fails merge — orphan OpenAPI or wire documentation bypassing field-policy table blocks CI.
- Ruff banned-api and import-linter rules flag domain imports of schema export helpers — surface ownership stays at composition root and adapter boundary per ingress-to-materialization projection family law.
- Registry rebuild at import proves every admitted `Bind.params_type` has ingress OpenAPI or validation surface row when verb exposes public schema — missing row fails CI job importing composition root.
- Mutation testing targets adapter projection folds and field-policy remap tables when surface seam logic is dense — interior domain folds remain secondary mutation surface.

# Negative Fixture Catalog

- `mixed_oracle_surface`: stdout or cache documented via Pydantic OpenAPI types — fails wire surface rows.
- `dump_schema_surgery`: post-hoc dict edits to oracle output before publish — fails compiled-layer snapshot law.
- `alias_drift_without_row`: adapter uses alias not reflected in field-policy table — fails OpenAPI and projection conformance rows.
- `cap_constant_duplication`: adapter hardcodes max length diverging from struct `Meta` — fails stdout envelope row and cap spill lattice row.
- `openapi_for_wire_layout`: external consumers instructed to parse msgspec layout from OpenAPI export — fails dual-engine triple row set.
- `version_oracle_default`: unknown `schema_version` maps to current struct in oracle table — fails pass-one literal closure on envelope surface row.
- `dual_dump_persist`: history stores pydantic JSON while cache uses msgspec bytes without paired rows — fails cache and history replay surfaces together.
- `partial_full_invariant_mix`: partial validation oracle paired with full domain invariant surfaces without row split — fails partial ingress sub-row and construction surface pairing.

# Promotion Unit And Simultaneous Update Law

- Adding or mutating a surface row binds in one promotion unit: `ORACLE_CONFORMANCE` tuple, `FIELD_POLICY` rows, oracle snapshot fixtures, lattice cross-links, metamorphic laws when polymorphic slots publish on that surface.
- New wire tag or discriminant requires simultaneous update to field-policy discriminant owner, msgspec struct oracle, detail decoder arm, exhaustive `match`, hypothesis strategy registry, and stdout/cache surface rows.
- Renaming `oracle_symbol` without snapshot fixture update breaks compiled-layer diff CI — codec submodule moves are version events for trusted-replay even when struct layouts unchanged.
- OpenAPI and wire surface changes touching one concept update `projection_conformance` diff row in the same promotion unit — partial promotion fails registry import and snapshot diff together.

# Consumer Contract Rows

- `consumer_pass_one_gate` mirrors envelope oracle fields required before body polymorphism — matrix row lists minimal field set; consumer tests assert gate without reimplementing decode logic.
- `consumer_detail_narrow` links detail surface rows to shared discriminant vocabulary — consumers match canonical tags; foreign synonyms were mapped at ingress and egress adapters only.
- `delta_two_envelope_compare` consumes materialized envelope structs — field access derives from stdout oracle row; no proof assumed; missing history sides use explicit `EMPTY` notes per ingress-to-materialization doctrine.
- Automation assumes success-path detail satisfied round-trip proof before emit — matrix metadata on detail surface rows cites `_validated` lattice row; consumers do not re-run proof on every line.

# Collapse Signals On Matrix Drift

- Module docstrings describing wire layout diverging from `msgspec.inspect` snapshots — collapse to single matrix owner and refreshed oracle fixtures.
- OpenAPI examples hand-maintained beside generated schema — collapse to alias-level `WithJsonSchema` and field-policy `openapi_doc` column.
- Cap constants in adapters not derived from inspect — collapse to field-policy and envelope oracle linkage.
- Parallel OpenAPI and wire type names for one concept without `projection_conformance` row — collapse to one canonical owner with triple row set.
- Consumer parsers using `json.loads` dict views — collapse to pinned decoder row and stdout surface oracle field access.

# Failure Archaeology On Surface Defects

- OpenAPI consumer sees wire field names — attribution targets ingress alias policy row and field-policy `ingress_alias` column, not msgspec rename table.
- Cache key drift after deploy — attribution targets encoder identity or `schema_version` metadata on `cache_bytes` row, not domain handler logic.
- Cap truncation missing on wire — attribution targets envelope oracle `Meta` linkage and cap spill lattice row, not interior fold string assembly.
- Consumer proof failure on success line — attribution targets root `_validated` row and detail surface metamorphic binding, not consumer re-proof logic.
- Migration read produces wrong discriminant — attribution targets ordered migration surface row sequence, not domain `match` arms after materialization exit.
- Oracle snapshot diff fails without source change — attribution targets field-policy table drift or alias serializer sub-row mode mismatch, not runtime decode faults.

# Harness Execution Order

- Layer 1 `static` executes before Layer 2 `compiled` before Layer 3 `runtime` — surface registry completeness, import architecture, and field-policy exhaustiveness block expensive metamorphic suites when cheaper layers fail.
- Layer 1 `static` — `FIELD_POLICY` column coverage per `concept_slug`, surface registry totality, domain import bans on schema export helpers, alignment static sub-row assignability on published payloads.
- Layer 2 `compiled` — oracle snapshot diff per `surface_id`, singleton identity smoke on `oracle_symbol`, `encoder_symbol`, `decoder_symbol` — per-request schema builds and shadow encoders fail here.
- Layer 3 `runtime` — metamorphic round-trip on surfaces with `proof_required=true`, consumer smoke decoding stdout bytes through pinned decoder, cap truncation spill parity — Hypothesis strategies draw from closed variant families on detail surface rows.
- Surface parametrized modules accept `surface_id` — one test function per oracle obligation per surface; mega-tests covering all surfaces on one axis are rejected for failure localization.
- Failure at Layer 1 blocks Layer 3 generative replay — same ordering as ingress-to-materialization doctrine stage-attributed proof layers, handoff transition contract lattice harness binding, and attribution smoke evolution calculus smoke federation execution order.

# Rejection Shortcuts

- Conformance proof without frozen `ORACLE_CONFORMANCE` row — register `surface_id` beside root codec module before parametrized oracle modules.
- Wire layout documented via Pydantic OpenAPI types on stdout or cache surfaces — indexed negative `mixed_oracle_surface` and `openapi_for_wire_layout`.
- Per-request `TypeAdapter.json_schema()` or fresh `msgspec.inspect.type_info` builds in handlers — fails singleton identity smoke and `per_request_codec` lattice negative.
- Hand-maintained OpenAPI examples beside generated schema — collapse to `WithJsonSchema` on alias owners and field-policy `openapi_doc` column.
- Cap constants in adapters not derived from struct `Meta` or field-policy `cap_max_length` — fails `cap_constant_duplication` on envelope and diagnostic surfaces.
- Consumer parsers using `json.loads` dict views — fails `consumer_json_loads` on all envelope consumer rows; collapse to pinned `decoder_symbol` from surface row.
- Dual-encoding persist without paired surface rows — history JSON plus msgspec cache bytes without `history_replay` and `cache_bytes` row linkage fails `dual_dump_persist`.
- Surface diff tests keyed to module paths instead of `surface_id` — harness discovery fails before metamorphic replay.
- Interior domain module importing `ORACLE_CONFORMANCE` or `FIELD_POLICY` for runtime routing — bypasses matrix-as-proof-only law and import-linter gates.

# Matrix Completeness Checklist

- Every production-published external layout has one `SurfaceRow` with closed `primary_oracle`, pinned `oracle_symbol`, and `projection_family` tag matching surface taxonomy.
- Every capped or aliased field on admitted surfaces cites `FIELD_POLICY` row via `field_policy_refs` — orphan adapter constants fail `field_policy_surface_join`.
- Every cross-family concept publishing ingress and wire layouts has minimum triple row set — `openapi_ingress`, wire surface, `projection_conformance` — plus `pydantic_validated_to_msgspec_wire` lattice foreign key.
- Every `surface_id` with `proof_required=true` binds metamorphic law to production `encoder_symbol` and `decoder_symbol` — shadow codecs in test helpers fail reference-identity checks.
- Every polymorphic detail slot has umbrella `detail_union` row plus per-arm rows — arm oracles pin struct layout independently; discriminant literals match `discriminant_owner`.
- Every `lattice_transition_ids` entry resolves to live handoff transition contract lattice row — `SURFACE_LATTICE_JOIN` documents bidirectional lookup for harness parametrization.
- Every negative fixture id in surface rows has one fixture factory and one assertion family in table-driven negative modules.
- Every oracle snapshot fixture pairs `surface_id` with compiled-layer hash or field-order table — rotation emits promotion diff with `pre_hash`, `post_hash`, and affected `field_policy_refs`.
- Every `Bind.params_type` exposing public schema has ingress surface row or documented root exemption — registry import gate passes before handler integration smoke.
- Done when surface registry, field-policy table, lattice join catalog, harness execution order, and rejection shortcuts are closed — interior modules never import matrix tables as runtime authority.

