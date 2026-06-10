# Mutual Embedding Graph And Owner Egress Calculus

# Mutual Cooperative Definition-Ref Cycles

- Two or more cooperative classes referencing each other as field types compile a shared `definitions` table on the enclosing owner — each cooperative hook returns `core_schema.definitions` entries with `definition-ref` back-edges; the enclosing `BaseModel` or `TypeAdapter` root is the sole rebuild and first-touch validation gate for the whole graph.
- Cycle closure requires quoted forward annotations on every back-edge field — `model_rebuild()` on the enclosing owner resolves cooperative `source_type` nodes before any arm of the cycle validates in isolation; validating cooperative types through standalone `TypeAdapter` without enclosing rebuild produces `PydanticUserError` on unresolved refs.
- Mutual cycles reject parallel `BaseModel` DTO intermediates per arm — each cooperative class owns one hook subgraph; inserting DTO shells between mutual references duplicates shape owners and breaks altitude doctrine from altitude admission doctrine.
- `definition-ref` identity deduplicates identical hook bodies across the cycle — cosmetic reordering of `chain_schema` steps on one arm without matching the paired arm defeats deduplication and inflates JSON Schema diffs without semantic change.
- Persistence keys for stored mutual graphs include enclosing owner `schema_version`, concrete generic argument tuples on every cooperative arm, and embedding field paths when `handler.field_name` branches bounds — cycle storage keyed on a single arm type alone loses embedding-site legality.
- Oracle registration for mutual graphs belongs on the enclosing owner strategy — `st.register_type_strategy(EnclosingOwner, strategy)` must generate mutually consistent arm instances satisfying every hook construction law in the cycle; per-arm standalone strategies without enclosing coherence under-generate cross-arm invariants.

# Rebuild Ordering And Graph Warm-Up

- Rebuild order is enclosing owner first, then root `TypeAdapter` pins that reference the enclosing family — interior modules import rebuilt owners and pinned adapters; cooperative modules do not call `model_rebuild()` in isolation unless they are also `TypeAdapter` roots documented in the seam map.
- Factory-defined owners embedding mutual cooperatives require `model_rebuild(_types_namespace=...)` with explicit namespace carrying every cooperative symbol — `globals()` from factory `exec` contexts is insufficient; missing namespace entries surface as forward-ref defects at first production validation, not at import.
- Worker boot imports the module defining the enclosing owner before worker threads validate any arm — lazy first-touch compilation on one arm of an unresolved cycle is a root warm-up defect; parent-process validator identity does not cross spawn seams.
- Plugin hosts registering new cooperative arms after root import lack automatic graph rebuild — dynamic cycle extension requires explicit registration hook that re-runs enclosing `model_rebuild()` and re-warms root `TypeAdapter` constants; post-import class surgery on one arm races under free-threaded importers.
- Generic parametrization of enclosing owners (`Graph[NodeKind]`) produces distinct definition tables per argument tuple — persistence and adapter pins record concrete tuples; erasure-friendly generic names alone fail cross-process replay rows.

# Generic And Parametric Graph Families

- PEP 695 generic enclosing owners (`class GraphEnvelope[T](BaseModel)`) and generic cooperative arms compile distinct `definitions` tables per concrete `get_args` tuple — matrix rows, adapter pins, and persistence keys record explicit specializations; default and explicit PEP 696 specializations are separate rows, not one erased name.
- Type-parameter bounds on graph slots compile into inner `handler.generate_schema(T)` nodes on every cooperative arm referencing the parameter — erasing `T` to `object` or `Any` inside cycle hooks breaks static parity, oracle registration, and cross-process replay keyed on argument tuples.
- Recursive generic graphs defer cooperative nodes until enclosing owner `model_rebuild()` with namespace carrying every parametrized symbol — first-touch validation on `GraphEnvelope[int]` does not authorize `GraphEnvelope[str]` without its own warm-up pin and matrix row.
- Subclasses of generic graph owners must preserve parametrization through the hierarchy — intermediate bases that drop `Generic` erase argument tuples at compile sites and split `definition-ref` identity across siblings incorrectly.
- `TypeAdapter(GraphEnvelope[int])` and `TypeAdapter(GraphEnvelope[str])` are distinct root pins — workers import both symbols when production admits multiple specializations; parametrization expressions at call sites are rejected.
- Generic root unions (`RootModel[Annotated[Union[ArmA[T], ArmB[T]], Field(discriminator='tag')]]`) compile per-argument discriminator tables — OpenAPI mapping, msgspec wire tags, and matrix `discriminant_literal` columns include concrete tuple suffixes when arms depend on `T`.

# RootModel Literal-Root Discriminator Matrices

- `RootModel[Annotated[Union[ArmA, ArmB, ...], Field(discriminator='tag')]]` and `RootModel[Union[CooperativeA, CooperativeB, ...]]` with `Literal` roots on 2.13+ compile root-type schema without object envelopes — JSON Schema and OpenAPI emit the root value directly; proof matrices must not wrap root-form samples in spurious property shells.
- Callable `Discriminator` on root unions routes through member metadata and `Tag('case')` — cooperative arms contribute hook subgraphs per tag; failure at one arm projects through boundary `loc` law without coercing to catch-all outer arms.
- Nested root unions wrap inner `Annotated[..., Field(discriminator=inner)]` before outer root routing — failure at any level is a typed `ValidationError` on the discriminated path; conformance samples must exercise failures at every level, not only the outer discriminant.
- `RootModel` root-form embeddings of cooperatively typed arms require hook `__get_pydantic_json_schema__` to emit root-type fragments — hand-maintained parallel root documentation beside hook implementations fails CI when root schema export is enabled.
- Extension or catch-all arms on root unions require explicit policy rows — `extra='allow'` or untagged fallback unions break client codegen when root publishes closed `oneOf`.
- Adding a root arm without updating OpenAPI discriminator mapping, per-arm oracle samples, msgspec wire tags, and hook promotion rows is a partial contract publish blocked at root CI.
- Round-trip proof for root unions preserves discriminant wire keys under alias policy — proof failures often indicate `serialization_alias` / `validation_alias` drift between root ingress and egress struct `rename` policy.

# Owner-Level Model Serializer Composition

- `@model_serializer(mode='wrap')` on enclosing owners post-processes default dump output after all field-level and cooperative hook serializer subgraphs execute — wrap callables receive handler-produced dict or scalar; owner wrap owns envelope keys, redaction, and aggregate wire layout, not per-slot coercion duplicated from field serializers.
- `@model_serializer(mode='plain')` replaces entire owner egress — reserve for owners whose wire form intentionally bears no structural relation to field layout; cooperative hook serializers on individual slots are bypassed unless plain callable explicitly delegates through `handler` and re-applies slot law.
- Owner wrap serializers must not re-encode cooperative slots by manual dict surgery — slot egress law stays in cooperative hook serializer chains or alias `PlainSerializer` / `WrapSerializer` stacks; owner wrap adjusts container envelope only.
- `when_used='json'` on owner serializers limits transforms to JSON dumps — interior `model_dump(mode='python')` and msgspec `convert` lanes read pre-owner-serializer Python runtime types from cooperative instances unless conformance rows declare owner wrap participation in `python` mode.
- `@computed_field` values are included in handler input to owner wrap serializers — wrap must not strip computed evidence validators assume on full snapshot re-validate unless `exclude_computed_fields` is documented in the conformance row.
- Stacking owner `@model_serializer` with per-field `@field_serializer` on the same logical aggregate slot duplicates law — pick owner wrap for envelope layout, field or hook serializers for slot encodings; collapse tests route duplicates to one surface per concept.

# Cooperative Slot And Owner Serializer Precedence

- Compilation order: cooperative hook serializer subgraphs compile per field; alias serializers on `Annotated` field types compose after hook subgraphs; `@field_serializer` on embedding fields overrides alias serializers for owner-local exceptions only; `@model_serializer` runs last on the assembled dump product.
- Duplicate normalization across cooperative hook `function-before`, alias `BeforeValidator`, and owner wrap pre-processing is a collapse defect — document single ownership of each normalization phase in the owner module when hook and alias both exist on one slot.
- `SerializeAsAny` on cooperative-typed fields widens validation-narrow egress — owner wrap must not assume static slot types when polymorphic detail slots participate; metamorphic proof rows cover runtime subclass wire keys through owner wrap when wrap reads dumped values.
- Owner wrap that redacts or renames cooperative slot keys must declare mapping in conformance rows — msgspec struct `rename` policy and owner wrap renames compose; silent double-rename fails root cross-engine gates.
- Ingress `chain_schema` and egress cooperative serializer chains remain independent morphisms — owner wrap does not replay ingress construction; Tier V re-admission still flows through `model_validate`, not through owner serializer invertibility.

# Polymorphic Serialization On Graph Owners

- `ConfigDict(polymorphic_serialization=True)` on enclosing graph owners addresses `SerializeAsAny` edge cases in nested cooperative slots and root union arms on 2.13+ — prefer explicit config over owner wrap pre-dump dict surgery when runtime subclasses change wire keys under polymorphic detail fields.
- Polymorphic cooperative slots require metamorphic proof through the full egress stack — hook serializers, alias serializers, owner wrap when load-bearing, then `project_wire` — validation-only oracle loops do not discharge subclass wire-key drift.
- Owner wrap reading dumped values from `SerializeAsAny` slots must declare polymorphic participation in conformance rows — wrap assumes static key sets only when rows document closed runtime subclass vocabulary.
- Root union arms with runtime-varying cooperative detail inherit the same encode-decode-revalidate loop as closed unions — per-arm metamorphic samples include at least one runtime subclass instance when policy permits widening beyond declared arms.
- Nested graph owners mixing closed and polymorphic slots document precedence in owner modules — owner wrap must not flatten polymorphic detail into closed envelope keys without explicit projection row.

# JSON Schema On Recursive And Root Graphs

- Enclosing owner `model_json_schema()` merges cooperative hook fragments and root-arm subgraphs through shared `definitions` — validation-mode documents accepted ingress including widened `function-before` inputs; serialization-mode documents emitted shapes after hook serializers, field serializers, and owner wrap transforms.
- Dual-mode snapshot pairs per enclosing family publishing contracts — single-mode snapshots under-approximate truth when owner wrap or computed fields define public egress.
- Recursive `definition-ref` targets appear once in `definitions` with stable ref names — pydantic 2.13 deterministic set ordering stabilizes diffs; ref churn without constraint churn requires separate review from semantic churn.
- Root-form JSON Schema must not emit object envelopes around cooperative or alias root values — hook and alias JSON projections own root-arm fragments; post-hoc `model_json_schema()` dict patches are rejected.
- `handler.resolve_ref_schema` in cooperative JSON hooks must not mutate shared definition targets — field-local and owner-local overrides return embedding-site fragments only.

# Proof Obligations On Embedding Graphs

- Core-schema oracle walks enclosing `__pydantic_core_schema__` including all cooperative `definition-ref` targets — unwrap every `chain_schema` step on every arm; opaque construction on any arm requires enclosing-owner strategy registration or explicit waiver rows naming the oracle gap.
- Mutual-cycle proof samples at least one valid graph instance per discriminant arm through `resolve(EnclosingOwner).example()` → `model_validate` — missing arm coverage is an exhaustiveness defect; mutual invariants that forbid certain arm combinations require explicit negative fixtures, not silent oracle omission.
- Root union families sample per-arm through `resolve(RootArm).example()` → `RootModel.model_validate` — hook changes on one cooperative root arm without per-arm loops are partial contract publishes.
- Owner serializer proof rows declare expected dump shape after wrap — metamorphic samples compare `model_dump(mode='json')` against serialization-mode JSON Schema fragments when owner wrap is load-bearing.
- Cross-engine metamorphic rows for graphs with owner wrap: `model_dump` mode follows struct field annotations; owner wrap output feeds `msgspec.convert` only when conformance row documents wrap participation — manual dict surgery between dump and convert bypasses owner and hook serializer law.
- `annotationlib.get_annotations(enclosing_owner, format=VALUE, include_extras=True)` parity runs after `model_rebuild()` on graphs with forward-ref cooperative arms — compare against `model_fields`, not cold `STRING` reads.

# Proof Obligation Register

- `PO-MG-01` — every mutual-cycle enclosing owner completes `model_rebuild()` in root import smoke before any matrix oracle loop executes.
- `PO-MG-02` — matrix row count bijects with cooperative arms and root union literals declared in owner modules — prose-only cycle descriptions without rows fail merge.
- `PO-MG-03` — each `arm_id` row samples `resolve(rebuild_owner).example()` → `model_validate` with `mutual_coherence` witness on enclosing strategy.
- `PO-MG-04` — every indexed `negative_fixture_ids` entry rejects admission with distinct fault evidence — orphan back-edges and cardinality violations each carry at least one negative sample.
- `PO-MG-05` — dual-mode JSON Schema snapshot pairs exist per publishing enclosing family when owner wrap or computed fields define egress.
- `PO-MG-06` — owner wrap metamorphic rows compare `model_dump(mode='json')` against serialization-mode schema fragments when wrap is load-bearing.
- `PO-MG-07` — cross-engine graph rows prove `encode → decode → model_validate` per matrix arm when graphs project to msgspec structs with documented wrap participation.
- `PO-MG-08` — `annotationlib.Format.VALUE` parity with `include_extras=True` on graph owners after rebuild matches `model_fields` on every alias-wrapped cooperative slot.
- `PO-MG-09` — standalone `TypeAdapter(CooperativeArm)` on cycle arms appears only in indexed negative fixtures unless seam map documents arm as root adapter.
- `PO-MG-10` — generic graph specializations warm every production `get_args` tuple — matrix rows and adapter pins align per argument tuple under forked workers.
- Static: import architecture forbids domain modules importing cycle arms for typing without canonical graph owner return types from altitude admission doctrine.
- Mutation: Stryker on owner wrap and hook serializer chains must kill mutants bypassing documented precedence or re-encoding cooperative slot keys at wrap layer.

# Hypothesis And Metamorphic Targets

- Enclosing-owner strategies generate mutually consistent arm instances — `mutual_coherence` witness asserts every back-edge field validates when sibling arms materialize; per-arm `st.from_type` alone under-generates cross-arm invariants.
- Matrix parametrized tests bind one property module law per graph family — rebuild closure, per-arm admission, owner wrap egress, or cross-engine round-trip; do not merge graph oracle, wrap metamorphic, and federation witness replay in one module.
- Root union families sample `st.sampled_from` over declared `discriminant_literal` values — unreachable literals appear only in negative-path modules indexed by matrix rows.
- Shrinking on graph validation failures rebuilds lawful enclosing instances — illegal arm insertion during shrink fails at construction gate before parent validate.
- Metamorphic: `model_dump` through documented wrap chain then `msgspec.convert` then decode then `model_validate` equals direct validate on source instance when conformance row documents full participation — order significant when wrap is load-bearing.

# Altitude Placement Of Graph Owners

- Enclosing owners carrying mutual cooperatives at ingress altitude own wire-visible envelope and discriminant law — canonical altitude owns durable graphs whose interior methods traverse cooperative arms after materialization; split into ingress envelope plus canonical graph owner when wire unwrap and interior traversal diverge.
- `RootModel` at ingress altitude substitutes for named envelope models when wire form is the root value — does not graduate to canonical altitude without field growth demanding a named owner family.
- Owner `@model_serializer` wrap at boundary altitude owns egress envelope projection — interior canonical transitions use `model_copy` / `copy.replace` without re-running owner serializers; wire handoff uses dump at boundary only.
- Settings embeddings of cooperative graph fragments stay root boot altitude — nested settings submodels validate independently; graph owners do not construct settings piecemeal in domain modules.

# Dual-Engine Graph Projection

- `project_wire` on enclosing owners reads cooperative hook serializer law per slot then applies owner wrap per conformance row — `msgspec.convert(instance, StructType)` mode selection follows struct annotations after documented dump and wrap chain.
- Tagged union graphs with cooperative arms prove encode-decode-revalidate per arm through root codecs — owner wrap participates in proof when egress struct expects wrapped layout.
- Ingress from msgspec decode into graph owners re-enters through `model_validate` when enclosing owners add alias, coercion, or cross-field law beyond struct closure — struct decode never substitutes for graph admission on those rows.
- Decimal, datetime, UUID, and bytes slots declare dump mode explicitly across hook serializers, owner wrap, and struct fields — silent mode mismatch at any layer fails merge at root cross-engine gates.

# Version Arms On Graph Families

- Breaking cooperative construction on any cycle arm is a version bump when persisted stores embed the graph — migration folds run at boundary altitude; obsolete arms remain in root migration modules with exhaustive folds.
- Breaking owner wrap layout — envelope keys, redaction policy, or computed-field inclusion — is breaking for egress consumers even when field annotations are unchanged — serialization-mode schema snapshot must diff.
- Breaking root discriminant vocabulary — new `Literal` value or renamed tag — bumps version arm; migration fold maps legacy tags; OpenAPI discriminator mapping and msgspec wire tags update in the same promotion unit.
- Breaking `handler.field_name` branching on any cooperative arm is breaking for consumers keyed on embedding-site bounds — regenerate oracle samples per field name before merge.
- `pydantic` minor upgrades pin behind graph parity tests on root import graph — enclosing `model_fields` ↔ cooperative subgraph agreement, owner wrap dump samples, and JSON definition stability, not compilation success alone.

# Collapse Tests For Graph And Owner Egress

- Standalone `TypeAdapter(CooperativeArm)` validation of mutual-cycle arms without enclosing rebuild — collapse to enclosing owner `model_rebuild()` plus root adapter warm-up.
- Parallel `BaseModel` DTO per cooperative arm in a mutual graph — collapse to cooperative hooks on rich classes referenced by enclosing owner fields only.
- Owner `@model_serializer` plus manual cooperative slot dict surgery in the same egress path — collapse to hook serializers plus documented owner wrap envelope-only law.
- `@field_serializer` on cooperative slots plus owner wrap re-encoding the same keys — collapse to one encoding surface per slot; owner wrap adjusts envelope only.
- Post-hoc `model_json_schema()` dict patches on recursive `definition-ref` graphs — collapse to cooperative JSON hooks and owner-level `GenerateJsonSchema` subclass hooks.
- Root union OpenAPI maintained parallel to `RootModel` declarations — collapse to model-driven schema export from root.
- Per-request `TypeAdapter` over graph owners or root unions — collapse to root `Final[TypeAdapter[...]]` warm-up.
- Domain import of ingress-only root envelope types for interior typing — collapse to canonical graph owner plus root `materialize_*` return types.

# Trust Tiers On Graph Re-Admission

- Tier V re-admission for foreign replacements of graph arms or root union values flows through enclosing `model_validate` — replays every cooperative `chain_schema` and root discriminant law; `model_copy(update=...)` cannot substitute on wire-sourced deltas.
- Trusted same-owner graph transitions use `model_copy` / `copy.replace` only when the base instance passed full validation at materialization — document tier in owning transition methods when callers cannot infer ingress versus domain context.
- Child arm re-admission before parent graph replace: nested dicts validate through child schema (`Child.model_validate(nested)`) before `model_copy(update={'arm': validated})` — never assign raw nested dicts into frozen parents carrying cooperative or root union slots.
- Pickle and `copy.deepcopy` traverse graph-validated owners but bypass version gates — dump+validate re-admission remains mandatory at cross-process seams unless trusted-replay rows document encoder and schema version pins.

# Settings And Root Graph Fragments

- Settings fields embedding single cooperative scalars from a larger graph compile isolated hook chains — full mutual graph settings embeddings are rejected unless nested `BaseSettings` submodels own each fragment with independent `extra='forbid'`.
- `Field(validation_alias=AliasChoices(...))` on settings slots referencing cooperative types resolves at construction — hooks must not read env key strings; alias routing stays on `Field`.
- Settings JSON Schema for graph-related cooperative fields is operator-facing only — client-published OpenAPI omits settings graph fragments unless every constraint is secret-free by policy review.

# Settings Sources And Graph Fragment Ingress

- Custom `PydanticBaseSettingsSource` subclasses prepare raw env strings before cooperative hook chains on graph scalar fragments execute — `prepare_field_value` returns carriers hook `function-before` steps accept; keep exotic I/O inside sources, not inside hook bodies on graph arms.
- `populate_by_name=True` on settings admitting graph-related cooperative scalars resolves alias and field name at construction — fan-out reads validated field values after full hook law consumes env ingress.
- `case_sensitive` and `env_nested_delimiter` changes alter which raw strings reach cooperative hooks on settings fragments — treat as breaking deployment contracts requiring regenerated matrix rows and hook ingress samples for affected `embedding_field_paths`.
- Nested `BaseSettings` submodels owning graph fragments validate independently — parent construction fails closed when child env namespaces leak cooperative types from sibling graph arms.

# Exhaustiveness Matrix Contract

- Every mutual-cycle enclosing owner declares a frozen matrix row per cooperative arm: `arm_id`, `cooperative_class`, `embedding_field_paths`, `rebuild_owner`, `oracle_strategy_id`, `negative_fixture_ids` — prose cycle descriptions without matrix rows fail root CI graph gates.
- `arm_id` is a stable snake slug — `node_tail`, `edge_head`, `graph_root` — not ordinal indices alone; matrix ids index promotion units and conformance samples across releases.
- `embedding_field_paths` lists every enclosing field name where the arm appears — `handler.field_name` branches require distinct path rows per bound variant; alias-only proof on one path misses branching defects on sibling embeddings.
- `rebuild_owner` names the enclosing `BaseModel` or `TypeAdapter` root that must complete `model_rebuild()` before the arm validates — standalone arm adapters are indexed negatives unless documented as `TypeAdapter` roots in the seam map.
- `oracle_strategy_id` references the enclosing-owner Hypothesis strategy or explicit waiver row — per-arm strategies without `mutual_coherence` witness fail matrix admission.
- `negative_fixture_ids` index inadmissible arm combinations — cardinality violations, topology contradictions, and orphan back-edges each carry at least one negative sample proving admission rejects the combination.
- Root union matrices extend the row shape with `discriminant_literal`, `tag_metadata`, `root_wire_form`, and `root_union_kind` — root arms omit `embedding_field_paths` when the union is the `TypeAdapter` or `RootModel` root itself.
- `root_union_kind` draws from `{field_discriminated, callable_discriminated, literal_root, nested_discriminated}` — federation graph rows index this column; mixed kinds on one owner require distinct matrix tables per publishing surface.
- Materialize `GRAPH_MATRIX: Mapping[EnclosingOwnerKey, tuple[GraphArmRow, ...]]` at root import beside owner modules — row tuples are frozen artifacts; matrix edits rebuild the table in the same promotion unit as schema or hook body changes.
- `GraphArmRow` references cooperative hook fingerprints from cooperative chain schema hooks when arms use cooperative admission — hook edits without matrix regeneration are partial promotion units blocked at root CI graph gates.
- Matrix edits trigger promotion-unit regeneration: per-arm oracle loops, dual-mode JSON Schema snapshots, msgspec conformance rows, and OpenAPI discriminator tables update in one merge-blocking change.

# Alias And Hook Composition On Graph Slots

- `Annotated` aliases on graph field slots compose after cooperative hook subgraph compilation — outer `BeforeValidator` on the alias runs before hook ingress unless the hook inlines equivalent normalization; mutual-cycle owners document composition order per slot when alias and hook coexist.
- Discriminated union fields embedding cooperative arms reference closed union aliases from annotated constraint stack — duplicate union spellings on graph owners drift `tagged-union` metadata across `definition-ref` targets in the same enclosing schema.
- `WithJsonSchema` on aliases exporting graph slot types handles alias-local documentation — cooperative `__get_pydantic_json_schema__` handles class-local wire projection; post-hoc merge of their outputs is rejected.
- `SkipValidation` on alias-wrapped cooperative slots inside trusted graphs still requires trusted-replay rows — mutual cycles do not weaken tier law on boundary-facing graph ingress fields.
- Generic alias parametrization on graph slots (`type GraphEdge[T] = Annotated[T, ...]`) compiles distinct inner nodes per argument tuple — matrix rows include concrete alias specializations, not erased generic names.

# Graph Promotion Units

- Renaming a cooperative arm class without hook body changes is refactor-only — matrix `arm_id` and conformance owner symbols update; `definition-ref` bytes may remain stable.
- Changing `chain_schema` step order, mutual back-edge bounds, or root discriminant literals is a breaking promotion unit — version bump when persisted stores embed the graph; migration fold, matrix regeneration, and dual-mode snapshots land together.
- Splitting one mutual-cycle owner into ingress envelope plus canonical graph owner is an altitude demotion/promotion unit — `materialize_*` return types, matrix `rebuild_owner` column, and root seam map rows update in the same merge-blocking change.
- Adding owner `@model_serializer` wrap to a graph that previously exported raw field dumps is breaking for egress consumers — serialization-mode schema snapshot diffs even when ingress annotations are unchanged.
- Merging sibling graph owners with identical cooperative definitions collapses `definition-ref` churn — verify with `TypeAdapter.json_schema()` ref counts; spurious duplicate refs indicate embedding-site `Field` divergence across owners.

# Composition Root Graph Harness

- Root import smoke imports every enclosing graph owner and pinned `TypeAdapter` over root unions — unresolved forward refs on any cooperative arm block merge before generative suites run.
- Session fixtures call `model_rebuild()` on graph owners mirroring production boot before oracle loops — cold first-touch on cyclic arms in property tests hides missing namespace defects.
- Matrix parametrized tests sample `resolve(EnclosingOwner).example()` per `arm_id` row through `model_validate` — missing row coverage is an exhaustiveness defect distinct from undiscriminated union sampling.
- Owner wrap metamorphic tests compare `model_dump(mode='json')` against serialization-mode schema when wrap is load-bearing — validation-mode snapshots alone under-approximate graph egress after owner serializers.
- Cross-engine graph proof uses root singleton codecs — `encode → decode → model_validate` per matrix arm when graphs project to msgspec egress structs with owner wrap participation documented in conformance rows.
- Free-threaded CI importers warm graph owners once in session scope — post-collection `model_rebuild()` on shared graph classes races with parallel test workers without root-ordered warm-up.

# Root CI Gates On Embedding Graphs

- Dual-mode JSON Schema snapshot tests diff on every graph owner the root exports to OpenAPI — validation mode and serialization mode pairs per enclosing family; owner wrap and computed fields require serialization-mode review.
- OpenAPI discriminator tables for root unions derive from `RootModel` and closed union declarations — hand-maintained parallel mappings beside graph owners fail CI when root schema export is enabled.
- Registry rebuild jobs importing the root graph fail when new cooperative arms lack matrix rows, migration entries, or handler map keys tied to graph ingress families.
- `pydantic` minor upgrades pin behind graph parity tests: matrix oracle loops, owner wrap dump samples, `model_fields` ↔ `annotationlib` agreement, and JSON `definitions` stability — compilation success alone does not discharge merge.
- Pin `annotationlib.Format.VALUE` parity on graph owners after rebuild in CI — `include_extras=False` collapses `Annotated` slot law and passes false-green on alias-wrapped cooperative embeddings.

# Validation Failure Projection On Graph Paths

- Union and discriminated-graph failures concatenate branch errors with distinct `loc` prefixes tracing through cooperative arms, nested lists, and root union branches — root capture maps every violation; consumers must not assume single-root `loc` on graph ingress.
- Cooperative hook `PydanticCustomError` types on any cycle arm map through the same `loc`+`msg` projection as model validators — domain-coded admission failures on graph paths survive root distillation without arm-specific special cases.
- Root union failures at inner discriminants prefix `loc` with branch indices before outer discriminant messages — batch ingress surfaces every violation; operator logs may truncate after mapping without dropping discriminant identity fields.
- `validate_call` handlers admitting graph fragments prefix `loc` with parameter names — root uses identical projection functions for model and call-boundary graph validation.
- Settings construction failures on graph-related cooperative env scalars map to startup faults at root boot — lazy raises inside domain rails past the `Result` boundary are rejected.

# Cross-Axis Seam Rows For Graph Handoffs

- `foreign_bytes_to_graph_owner` — raw JSON or bytes; `model_validate_json` on enclosing owner or `TypeAdapter.validate_json` on root union; compile oracle `first_touch` after `model_rebuild()`.
- `graph_owner_to_canonical_interior` — validated enclosing or root instance; root `materialize_*` smart constructor; upstream oracle `class_body` on graph schema; downstream `none` on interior rich owner unless canonical synthesis decorators apply.
- `canonical_graph_to_wire_projection` — materialized graph owner; `project_wire` with owner wrap row; downstream msgspec struct; negatives include `dump_surgery_projection` and owner wrap bypass.
- `wire_to_graph_re_ingress` — stored or cross-process bytes; `model_validate` on enclosing owner; `trust_posture` pinned per matrix row; full cooperative `chain_schema` replays unless trusted-replay documents field-level skip.
- `root_union_arm_migration` — stored layout with legacy discriminant; boundary migration fold → current root union → `model_validate`; obsolete arms remain in root migration modules with exhaustive folds.
- Chained graph pipelines compose as typed steps: foreign ingress validate → `materialize_*` → interior fold on graph arms → owner wrap dump → `project_wire` → encode — no step skips the single validation gate per boundary crossing.

# Worker Boot And Graph Parity

- Worker entrypoints re-import modules defining enclosing graph owners and cooperative arms before worker threads validate graph carriers — compiled `SchemaValidator` identity is process-local.
- `model_rebuild()` on graph owners runs in worker boot when shared packages import cooperative arms after partial initialization — matrix parity proofs execute after rebuild, not on cold `annotationlib.Format.STRING` reads.
- Generic graph adapters (`TypeAdapter(Graph[int])`) cache per concrete argument tuple at root `[CONSTANTS]` — workers import pinned symbols, not parametrization expressions at call sites.
- Cross-process handoff of graph-validated owners prefers `model_dump` + `model_validate` when `schema_version` enforcement matters — owner wrap and hook serializer law replay only on full re-admission.
- `BaseSettings` construction completes before worker threads read settings carrying cooperative graph scalars — lazy settings accessors on hot paths race under parallel importers.
- Cross-process schema parity for forked workers does not yet prove generic graph parametrization closure in one CI gate — matrix rows and adapter pins must align per argument tuple.

# Opaque Construction And Walker Gaps On Graph Arms

- Construction callables inside cooperative `function-after` or `function-plain` terminals on cycle arms encode semantic law invisible to core-schema walkers — every opaque arm carries explicit `register_type_strategy` on the enclosing owner or waiver rows naming the oracle gap.
- Opaque construction on one arm of a mutual cycle still requires enclosing-owner strategy coherence — registration on the arm class alone under-generates sibling back-edge invariants the cycle demands.
- Construction failures on graph arms surface as generic validation errors unless hooks raise `PydanticCustomError` with typed codes — boundary projection maps custom codes through the same `loc`+`msg` law as model validators on graph paths.
- Combining inspectable leaf constraints before opaque construction on cycle arms stacks rejection at core nodes — opaque bodies receive core-validated values only; regex or bound rejection never reaches construction callables.
- Hypothesis registration for graph families belongs on the enclosing owner strategy indexed by matrix `oracle_strategy_id` — strategies must satisfy construction law on every arm referenced in `embedding_field_paths`.

# Free-Threaded Graph Compile Races

- Parallel first-touch on generic graph owners (`GraphEnvelope[int]` versus `GraphEnvelope[str]`) before boot freeze completes requires root lock-order on generic adapter compile memo — without guard, `definition-ref` tag multisets race and produce undefined matrix fingerprints.
- Post-collection `model_rebuild()` on shared graph classes from pytest workers races with session-scoped warm-up — root harness declares single warm-up owner; workers import pre-rebuilt symbols only.
- Plugin hosts registering cooperative arms after root import under free-threaded importers require explicit registration hook with lock — dynamic cycle extension without serialized rebuild is race defect, not benign duplicate compile.
- Subinterpreter isolation multiplies compile domains per interpreter — cross-interpreter graph handoff requires bytes plus pinned enclosing owner adapter at receiving root, not shared module rebinding of graph constants across interpreter boundaries.

# Anti-Patterns

- Mutual cooperative cycles validated per arm without enclosing rebuild; DTO arms inside cooperative cycles; owner wrap re-encoding cooperative slots manually; plain owner serializer bypassing hook law without documented intent; root union OpenAPI parallel tables; standalone cooperative `TypeAdapter` on cycle arms in production paths; `include_extras=False` in graph parity proofs; owner serializer inverting ingress construction; settings piecemeal construction of graph fragments; per-request graph `TypeAdapter` construction; post-hoc recursive JSON schema surgery; `SerializeAsAny` polymorphic slots without metamorphic proof through owner wrap.

# Diagnostic Binding On Graph Faults

- Graph admission failures at root distill into envelope fault slots with `validation:` stage prefixes on `failing_step` — raw `ValidationError` strings do not cross into domain `Result` interiors.
- Matrix `arm_id` labels appear in operator diagnostics when capture modules annotate discriminated failures — truncation preserves discriminant and `arm_id` identity fields required for replay and dispatch.
- HTTP 422 bodies for graph ingress use `ValidationError.json()`-shaped output from root adapters — interior modules consume mapped violation tuples only.
- Settings boot failures on graph-related cooperative env fields are configuration defects — map at root startup before handlers bind; not lazy domain exceptions past the `Result` boundary.
