# Cooperative Chain Schema Hook Compilation

# Cooperative Admission Contract

- `__get_pydantic_core_schema__(cls, source_type, handler)` is the sole compile-time admission surface for non-`BaseModel` types — return a `CoreSchema` node graph or delegate inner slots through `handler.generate_schema(inner)`; the class retains runtime identity, methods, and `PrivateAttr` while Pydantic owns validation and dump hooks for embedding sites.
- `__get_pydantic_json_schema__(core_schema, handler)` projects the compiled cooperative subgraph to JSON Schema — transport-specific overrides stay in this hook; core schema stays engine-agnostic; post-processing `model_json_schema()` dict surgery on cooperative embeddings duplicates law already owned by the hook pair.
- Cooperative admission does not make the rich class canonical Pydantic altitude — embedding a cooperatively typed field keeps canonical behavior on the rich owner; Pydantic owns slot validation at the boundary only unless the bounded context documents ingress-as-canonical for that family.
- `PlainValidator` / `WrapValidator` on `Annotated` aliases remain preferred when the transform is a function on a bare scalar — cooperative hooks own type identity, multi-step construction, `is_instance_schema` guards, and serializer pairing that `Annotated` stacks cannot express without erasing the domain class.
- `arbitrary_types_allowed=True` is an escape hatch rejected by default — cooperative hooks replace blanket arbitrary admission with inspectable core nodes; escape rows require explicit waiver in the owner module and collapse-test routing.

# Handler Discipline And Context Leakage

- `handler.generate_schema(T)` strips outer `Annotated` metadata from unrelated inner parameters — default for every inner slot compilation inside cooperative hooks; prevents outer field `Field(ge=...)` or alias metadata from leaking into nested type parameters the hook did not intend to contextualize.
- `handler(T)` preserves `Annotated` context for the parameter — reserve for intentional per-parameter contextual compilation only; misuse attaches parent-field validators to inner bare types and produces silent over-constraint or under-constraint at embedding sites.
- `handler.field_name` branches metadata when the same Python type embeds at different field names with different bounds, coercion, or JSON overrides — compile distinct core nodes per name; persistence keys include embedding path when bounds affect stored shape legality.
- `handler.resolve_ref_schema` and `handler.get_schema_from_ref` participate in `definition-ref` graphs for recursive cooperative types — forward-ref cooperative classes require quoted annotations plus enclosing owner `model_rebuild()` before first validation; hooks must not assume `source_type` is always the concrete class at deferred compile sites.
- Generic cooperative hooks read type parameters from `source_type` via `get_args(source_type)`, not from `cls` alone — `Box[int]` and `Box[str]` compile distinct subgraphs; adapter pins and persistence keys include concrete argument tuples.

# Chain Schema Composition Patterns

- `chain_schema` sequences core phases on one slot — typical ingress walk: `function-before` normalization → inner type validation via `handler.generate_schema` → `function-after` refinement → optional `function-plain` terminal replacement; order in the `steps` tuple is the runtime walk order, not Python method definition order.
- `chain_schema(..., serialization=...)` attaches an egress subgraph on the chain node in pydantic-core — reserve for tightly coupled single-slot transforms; when ingress and egress morphisms diverge, prefer explicit `chain_serializer_schema` or paired hook serializer nodes so conformance rows can diff validation and serialization modes independently.
- `union_schema` plus `is_instance_schema` guards admit cooperatively constructed instances — validate raw input through inner schema first, then assert `isinstance` against the domain class; reversing guard order rejects valid instances that inner coercion already materialized.
- `no_info_plain_validator_function` and `with_info_plain_validator_function` own trusted transforms that replace inner validation for the slot — pair with `json_schema_input_type` in the JSON hook when wire types differ; plain nodes are terminal unless an explicit outer `function-wrap` shell documents sandwich semantics.
- `with_info_wrap_validator_function` sandwiches core validation — `handler` in the wrap callable receives core-validated values; use when cooperative admission must observe inner constraint nodes before domain construction without duplicating `@field_validator` on every embedding model.
- `lax_or_strict_schema` inside chains expresses per-embedding strictness when class-level `ConfigDict` is lax — settings stringly env keys and interior strict paths share one cooperative type with `handler.field_name` selecting the lax or strict arm.
- `default_schema` and `nullable_schema` wrappers on chain terminals express optional cooperative slots — defaults must survive the full chain when `Field(validate_default=True)` applies on embedding fields.
- Serializer chains mirror ingress chains through `chain_serializer_schema` or paired dump hooks — ingress `chain_schema` and egress serializer subgraphs are independent morphisms; conformance rows document both when wire shape diverges from validated instance shape.

# Core Schema Builder Vocabulary

- `is_instance_schema(cls, schema=inner)` asserts runtime type after inner validation — use for cooperatively constructed classes that must not admit subclasses unless `SerializeAsAny` policy explicitly widens egress.
- `model_schema` on inner `BaseModel` intermediates is rejected inside cooperative hooks for durable domain classes — cooperative types materialize instances directly; nested `BaseModel` DTOs inside hooks duplicate shape owners and break altitude doctrine.
- `typed_dict_schema` and `dataclass_schema` delegate when cooperative classes wrap stdlib carriers — hook returns chain terminating in delegated schema plus `function-after` construction into the rich class; construction callable must be pure admission, not I/O.
- `str_schema`, `int_schema`, and constraint-bearing leaf nodes from `pydantic_core.core_schema` compose chain prefixes — regex `pattern`, `min_length`, and `multiple_of` remain inspectable to schema walkers; opaque construction bodies after leaves require `register_type_strategy` on embedding models.
- `custom_error_schema` wraps inner nodes with typed `PydanticCustomError` surfaces — boundary projection maps cooperative custom codes through the same `loc`+`msg` law as model validators.
- `ref` and `definition-ref` identity for cooperative hooks follows the same deduplication law as module-level `Annotated` aliases — identical hook bodies across modules collapse to shared refs when metadata tuples and compile fingerprints match; cosmetic reordering of `chain_schema` steps defeats deduplication and inflates schema diffs without semantic change.
- Recursive cooperative hooks return `definitions_schema(schema=definition-ref, definitions=[...])` with stable `ref` keys per arm — `definition-ref` back-edges require enclosing owner `model_rebuild()` before any cycle arm validates in isolation.

# JSON Schema Hook Projection

- `__get_pydantic_json_schema__` receives the cooperative `core_schema` subgraph and returns JSON Schema fragments merged by the enclosing generator — override wire types, examples, and vendor extensions here; keep Python return types on the cooperative class unchanged.
- Validation-mode JSON Schema documents accepted ingress shapes for the cooperative slot — when `function-before` widens input beyond Python annotations, hook must emit `json_schema_input_type` equivalent via `handler` overrides or `WithJsonSchema` on a wrapping `Annotated` export alias.
- Serialization-mode JSON Schema documents emitted wire shapes when cooperative hooks attach `plain_serializer_function` or `wrap_serializer_function` nodes — dual-mode snapshot pairs per cooperative family publishing contracts.
- `handler.resolve_ref_schema` in JSON hooks must not mutate shared `definition-ref` targets — field-local overrides belong in the returned fragment for the embedding site only.
- OpenAPI for cooperatively typed fields derives from model declarations plus hook projections — hand-maintained parallel type documentation beside hook implementations fails CI when root schema export is enabled.

# Field-Name Branching And Embedding Sites

- Same cooperative class at `port` versus `metrics_port` field names may compile different `ge`/`le` bounds via `handler.field_name` — oracle samples must cover each embedding site; alias-only proof misses branching defects.
- Settings embeddings use `handler.field_name` for env-specific coercion arms — fan-out reads validated field values after construction; hook branches must not depend on live `os.environ` at compile time.
- `TypeAdapter(CooperativeType)` at module scope compiles root-form cooperative schema without a parent `model_fields` entry — `field_name` may be `None`; root-level bounds belong in the hook body or wrapping `Annotated` alias, not in phantom field-name branches.
- Enclosing `Annotated` stacks on cooperative-typed fields compose after cooperative subgraph compilation — outer `BeforeValidator` runs before cooperative `chain_schema` ingress unless hook internally inlines equivalent normalization; duplicate normalization across alias and hook is a collapse defect.

# Altitude Placement Of Cooperative Types

- Ingress altitude cooperative hooks own wire-visible normalization and envelope unwrap inside `chain_schema` prefixes — canonical altitude cooperative hooks own durable construction invariants and persisted layout law; split hooks when wire normalization and interior construction diverge.
- Boundary-only ingress plus cooperatively typed canonical non-`BaseModel` owner is the default — one cooperative hook on the canonical class, one ingress model referencing it as a field type, zero parallel DTO reconstruction of the same slots.
- Root altitude pins `TypeAdapter` over cooperatively typed unions and module-level cooperative aliases — warm-up compiles hook subgraphs once; interior modules import adapter symbols, not re-run `GenerateSchema` on equivalent inline types.
- Cooperative hooks on rich classes with `PrivateAttr` and effectful methods stay canonical or interior altitude — validators inside hooks enforce admission only; no I/O, registry mutation, or settings reads inside `function-before`/`function-after` bodies.

# Generic And Parametric Cooperative Hooks

- PEP 695 `class Box[T]` cooperative hooks bind `T` from `get_args(source_type)` at compile time — distinct validators per `Box[int]` versus `Box[str]`; generic adapter pins cache per argument tuple at root `[CONSTANTS]`.
- Bounds on type parameters (`T: str`) compile into inner `handler.generate_schema(T)` nodes — erasing `T` to `object` or `Any` inside hook bodies breaks static parity and oracle registration.
- Recursive cooperative types defer nodes until enclosing owner `model_rebuild()` — hooks return `core_schema.definitions` with `definition-ref` back-edges; first-touch validation on the enclosing model is the proof gate for recursive closure.
- Default specialization on generic cooperative aliases (`Box` unparametrized versus `Box[int]`) produces distinct compiled schemas — persistence keys record explicit parametrization, not erased generic names.

# Python 3.15 Annotationlib And Cooperative Compile Parity

- Python `>=3.15` defers class annotations through compiler-generated `__annotate__`; cooperative hooks still compile at first-touch from resolved `source_type` — audit reads on hook owners and embedding models use `annotationlib.get_annotations(cls, format=annotationlib.Format.VALUE, include_extras=True)`, not eager `__annotations__` mutation or legacy `typing.get_type_hints` alone.
- PEP 749 `annotationlib.Format.FORWARDREF` admits factory and codegen reads on cooperative classes referencing not-yet-defined peers — production validation and hook parity proofs require VALUE format after enclosing owner `model_rebuild()`; FORWARDREF reads do not substitute for rebuild or first-touch compile proof.
- `annotationlib.Format.STRING` preserves unevaluated references for signature renderers — runtime hook compilation and generic `get_args(source_type)` binding require VALUE-resolved field types at warm-up, not STRING snapshots alone.
- Embedding owners recover cooperatively typed field annotations with `include_extras=True` so PEP 695 specializations (`Box[int]`) and wrapping `Annotated` export aliases remain visible to law-matrix parity — `include_extras=False` collapses cooperative embeddings to bare class references and hides alias-stack law from proof coupling.
- Generic cooperative hooks read type parameters from VALUE-resolved `source_type` via `get_args(source_type)` at compile time — STRING-format annotation reads cannot substitute for parametrization on adapter pins, oracle registration, or persistence keys.
- Pydantic `>=2.13.3` integrates deferred annotations through the same `GenerateSchema` pipeline — promotion units pin minor bumps behind embedding-owner `model_fields` ↔ cooperative subgraph agreement under VALUE reads, not compilation success alone.

# Serializer And Ingress Divergence On Rich Types

- Cooperative hooks may attach serializer nodes that emit wire scalars while validation materializes rich instances — conformance rows declare `model_dump` mode per msgspec projection seam; `mode='json'` when struct fields expect wire scalars.
- `when_used='json'` on serializer chain terminals limits transforms to JSON dumps — interior `model_dump(mode='python')` retains Python runtime types for folds and `msgspec.convert` lanes.
- `SerializeAsAny` on fields typed as cooperative base classes widens egress, not validation — metamorphic proof rows required when runtime subclasses change wire keys or serializer behavior.
- `@field_serializer` on embedding models plus cooperative hook serializer chains on the same slot duplicate law — pick hook serializers for reusable encodings, field serializers for owner-local exceptions only.

# Proof Obligations On Hook Owners

- Core-schema oracle walks cooperative `chain_schema` steps — unwrap `function-before`, `function-after`, `function-wrap`, and `function-plain` to inner constraint nodes; opaque construction bodies require `st.register_type_strategy` on embedding models or explicit waiver rows on the cooperative module.
- `resolve(T)` on cooperatively typed embeddings registers strategies from the hook subgraph when construction law exceeds core nodes — `st.from_type` alone under-generates identically to naive model resolution.
- `annotationlib.get_annotations(owner, format=annotationlib.Format.VALUE, include_extras=True)` on embedding owners must recover cooperative field types after `model_rebuild()` — parity proofs compare against `model_fields`, not `__annotations__` alone.
- Hook edits trigger promotion-unit regeneration: dual-mode JSON Schema snapshots for every publishing embedding, oracle sample loops per `handler.field_name` branch, and cross-engine metamorphic rows when hook serializers feed msgspec structs.
- Discriminated unions embedding cooperative arms sample one generated instance per arm through `resolve(Arm).example()` → `model_validate` — hook changes without per-arm oracle loops are exhaustiveness defects.

# Dual-Engine Cooperative Projection

- Cooperative hook serializer law is consumed at `project_wire` — `msgspec.convert(instance, StructType)` reads declared dump mode; manual dict surgery bypasses hook serializer chains and breaks conformance rows.
- Ingress from msgspec decode into cooperatively typed fields re-enters through `model_validate` when hooks add coercion or construction law beyond struct closure — struct decode never substitutes for hook admission on those rows.
- Tagged union arms carrying cooperative detail slots prove encode-decode-revalidate loops — polymorphic `SerializeAsAny` fields inherit the same metamorphic proof as closed unions.
- Decimal, datetime, UUID, and bytes serializer terminals on hooks declare dump mode explicitly in conformance rows — silent mode mismatch between hook output and struct field type fails merge at root cross-engine gates.

# Version Arms And Hook Evolution

- Breaking cooperative construction law — chain step order, `is_instance_schema` target, or inner constraint bounds — is a version bump when persisted stores embed the type — migration folds run at boundary altitude; obsolete hook arms remain in root migration modules with exhaustive folds.
- Breaking serializer transform on cooperative hooks is breaking for egress consumers — serialization-mode schema snapshot must diff even when Python class identity is unchanged.
- Breaking `handler.field_name` branching table is breaking for any consumer keyed on embedding site bounds — regenerate oracle samples per field name before merge.
- `pydantic` minor upgrades pin behind hook parity tests on root import graph — `model_fields` ↔ cooperative subgraph agreement and JSON hook fragment stability, not compilation success alone.

# Collapse Tests For Cooperative Hooks

- Parallel `BaseModel` DTO mirroring cooperative class slots — collapse to cooperative hook on rich class plus embedding field reference.
- `arbitrary_types_allowed=True` on owners embedding cooperatively typed fields — collapse to hook admission with inspectable core nodes.
- `handler(T)` on inner parameters without documented contextual intent — collapse to `handler.generate_schema(T)` default.
- Post-hoc `model_json_schema()` dict patches replacing `__get_pydantic_json_schema__` — collapse to hook-level JSON projection.
- `@field_serializer` plus cooperative hook serializer chain on the same slot — collapse to one encoding surface per concept.
- Inline `chain_schema` duplicate of module-level cooperative class hook — collapse to class method hook imported across embeddings.
- Domain import of ingress-only cooperative unwrap types for interior typing — collapse to canonical cooperative class plus root `materialize_*` return types.

# Trust Tiers And Cooperative Re-Admission

- Tier V re-admission applies when foreign carriers replace cooperatively typed fields — `model_validate` replays the full hook `chain_schema` including `function-before` widenings; `model_copy(update=...)` cannot substitute for hook construction law on wire-sourced deltas.
- `SkipValidation` on `Annotated` wrappers around cooperative types marks interior trusted slots upstream already closed — never pair with hook `function-before` widening on the same boundary-facing field without trusted-replay rows documenting upstream closure proof.
- Trusted same-owner cooperative field swaps use `model_copy` / `copy.replace` only when the base instance passed full validation at materialization — document tier in the owning transition method when callers cannot infer ingress versus domain context.
- Child cooperative re-admission before parent replace: nested dicts validate through child schema (`Child.model_validate(nested)`) before `model_copy(update={'child': validated})` — never assign raw nested dicts into frozen parents carrying cooperatively typed child slots.
- Pickle and `copy.deepcopy` traverse hook-validated instances but bypass explicit version gates — dump+validate re-admission remains mandatory at cross-process seams unless trusted-replay rows document encoder and schema version pins.

# Settings And Env Cooperative Specialization

- Settings fields embedding cooperatively typed env scalars compile hook chains with lax prefix arms for stringly env values — `field_validator(mode='before')` on the settings model duplicates hook `function-before` law and is a collapse defect unless altitude split documents distinct responsibilities.
- `Field(validation_alias=AliasChoices('LEGACY', 'CANONICAL'))` on settings embeddings resolves at construction — cooperative hooks must not read env key strings; alias routing stays on `Field`, normalization stays in hook `function-before` or dedicated alias stacks.
- `SecretStr` / `SecretBytes` wrappers around cooperative types are rejected — secrets redact at the scalar wrapper; cooperative construction hooks must not embed secret materialization that leaks into `repr()` or default dumps.
- Nested `BaseSettings` submodels referencing cooperatively typed child fields validate independently at parent construction — child `extra='forbid'` catches env namespace leakage before fan-out slices read canonical field values.
- Settings JSON Schema for cooperatively typed fields is operator-facing only — client-published OpenAPI omits settings cooperative families unless every constraint is secret-free by policy review.

# Settings Sources And Cooperative Ingress

- Custom `PydanticBaseSettingsSource` subclasses prepare raw env strings before cooperative hook chains execute — `prepare_field_value` returns carriers hook `function-before` steps accept; keep exotic I/O inside sources, not inside hook bodies.
- `populate_by_name=True` on settings admits alias and field name on env ingress — fan-out `project_*_config` reads Python field names from the validated instance after cooperative hook law is fully consumed at construction.
- `case_sensitive` and `env_nested_delimiter` changes alter which raw strings reach cooperative hooks — treat as breaking deployment contracts requiring regenerated collision matrices and hook ingress samples.

# Worker Boot And Cooperative Warm-Up

- Worker entrypoints import modules defining cooperative classes before first `model_validate` on embedding owners — lazy first-touch hook compilation in request handlers is a root warm-up defect, not a recoverable domain retry.
- `model_rebuild()` on owners embedding forward-ref cooperative types runs in worker boot when shared packages import after partial initialization — hook parity proofs execute after rebuild, not on cold `annotationlib.Format.STRING` reads.
- Generic cooperative adapters (`TypeAdapter(Box[int])`) cache per concrete argument tuple at root `[CONSTANTS]` — workers import pinned symbols, not parametrization expressions at call sites.
- Cross-process handoff of cooperatively typed owners prefers `model_dump` + `model_validate` when `schema_version` enforcement matters — hook serializer law replays only on full re-admission, not on pickle round-trips.
- Subinterpreter and worker spawn seams treat compiled cooperative `SchemaValidator` graphs as process-local — parent warm-up does not transfer hook subgraph identity; each interpreter imports cooperative modules and root-pinned `TypeAdapter` symbols independently before first `model_validate`.

# Opaque Construction And Walker Gaps

- Construction callables inside `function-after` or `function-plain` chain terminals encode semantic law invisible to core-schema walkers — every opaque construction hook carries explicit `register_type_strategy` on embedding models or waiver rows naming the oracle gap.
- Construction failures surface as generic validation errors unless hooks raise `PydanticCustomError` with typed codes — boundary projection maps custom codes through the same `loc`+`msg` law as model validators.
- Combining inspectable leaf constraints (`pattern`, `ge`, `le`) before opaque construction stacks rejection at core nodes — opaque bodies receive core-validated values only; regex rejection never reaches construction callables.
- Hypothesis registration for cooperatively typed embeddings belongs on the embedding model strategy — `st.register_type_strategy(EmbeddingModel, strategy)` must generate values satisfying hook construction law, not annotation-inferred samples alone.

# Cooperative Hook And Alias Stack Interaction

- Module-level `Annotated` aliases wrapping cooperatively typed classes compose outer metadata after hook subgraph compilation — `BeforeValidator` on the alias runs before hook ingress unless hook inlines equivalent steps; document composition order in the owner module when both exist.
- `WithJsonSchema` on aliases exporting cooperatively typed symbols handles alias-local documentation — hook `__get_pydantic_json_schema__` handles class-local wire projection; both may apply; post-hoc merge of their outputs is rejected.
- `TypeAdapter(CooperativeAlias)` and model fields referencing the same cooperatively typed class share `definition-ref` when hook bodies match — alias-only `Field` divergence on embedding sites splits refs and requires collapse to one admission surface.
- Discriminated union aliases embedding cooperatively typed arms compile `tagged-union` nodes referencing hook subgraphs per arm — adding an arm without regenerating hook oracle samples per arm is a partial contract publish blocked at root CI.
- Callable `Discriminator` on unions of cooperatively typed arms routes through member metadata — hook construction failures at one arm project through boundary `loc` law without coercing to catch-all outer arms.

# Hook Rename And Promotion Units

- Renaming a cooperative class without changing hook body bytes is a refactor-only event — `definition-ref` identity may remain stable; importers update import paths and conformance row owner symbols only.
- Changing `chain_schema` step order, inner constraint bounds, or `handler.field_name` branch tables is a breaking promotion unit — dual-mode JSON Schema diffs, oracle sample loops, msgspec conformance rows, and OpenAPI fragments regenerate together.
- Splitting one cooperative class into ingress-normalization hook plus canonical-construction hook siblings is an altitude demotion/promotion unit — `materialize_*` return types and root seam map rows update in the same merge-blocking change.
- Merging sibling cooperative classes with identical hook metadata tuples collapses `definition-ref` churn — verify with `TypeAdapter.json_schema()` ref counts before merge; spurious duplicate refs indicate embedding-site `Field` divergence.

# Anti-Patterns

- DTO duplication of cooperatively typed slots; `handler(T)` leaking parent `Annotated` context to inner bare types; I/O inside hook validator bodies; `arbitrary_types_allowed=True` without waiver row; post-hoc JSON schema dict surgery; per-request cooperative `TypeAdapter` construction; hook `chain_schema` order diverging from documented ingress semantics without version bump; `include_extras=False` in parity proofs on embedding owners; conditional altitude encoded inside one hook via runtime settings reads; settings `field_validator` duplicating hook `function-before` on the same slot; `SecretStr` wrapping cooperative materialization outputs.
