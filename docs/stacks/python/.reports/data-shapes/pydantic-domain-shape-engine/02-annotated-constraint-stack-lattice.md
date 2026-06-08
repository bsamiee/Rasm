# Annotated Constraint Stack Lattice

# Stack Evaluation Order

- `Annotated` metadata evaluates right-to-left for functional validators — outer `BeforeValidator` runs before inner `AfterValidator` on the same bare type parameter; misordering normalization and refinement inverts admission semantics silently.
- `Field(...)` constraints compile into inspectable core nodes (`ge`, `le`, `min_length`, `pattern`, `discriminator`) — they are not decorators and do not participate in the functional-validator call chain.
- `annotated-types` constraints (`Gt`, `Len`, `Predicate`, `doc`) attach as metadata layers Pydantic and JSON Schema generators read — custom `Predicate` bodies are opaque to schema walkers unless paired with explicit `WithJsonSchema` or `register_type_strategy` rows.
- `PlainValidator` and `PlainSerializer` replace inner validation or default serialization for the annotated slot — reserve for trusted transforms; stacking `PlainValidator` beneath `Field` constraints still applies constraint nodes after the plain function returns.
- `WrapValidator` and `WrapSerializer` sandwich core logic — `handler` receives core-validated values; wrap layers own egress or ingress projection without duplicating `field_validator` on every embedding model.
- `SkipValidation` marks interior trusted slots on `Annotated` — never stacks with `BeforeValidator` widening on the same boundary-facing alias; trusted-replay rows document upstream closure proof.
- Pydantic 2.4+ passes `ValidationInfo` into `Annotated` validator callables — `field_name`, `config`, and `context` enable per-embedding-site behavior on reusable alias stacks without duplicating alias symbols per parent field name.

# Type Alias Versus Per-Field Embedding

- Reusable law belongs on PEP 695 `type` aliases (`type Port = Annotated[int, Field(ge=1, le=65535), ...]`) admitted once at module scope — per-field repetition of identical `Annotated` stacks drifts constraint metadata across parents.
- Altitude-specific divergence uses distinct alias symbols (`IngressPort`, `CanonicalPort`) rather than conditional validator branches inside one alias — wire versus interior law is not a runtime parameter on shared `Annotated` stacks.
- `TypeAdapter(Alias)` and model fields referencing the same alias share one compiled subgraph via `definition-ref` — changing the alias is a promotion-unit event across every embedding site and JSON Schema snapshot.
- Embedding a bare `T` with inline `Field` on one model and `Annotated[T, Field(...)]` alias on a sibling model duplicates core nodes under different refs — collapse to one alias before OpenAPI export.
- Settings fields, ingress envelopes, and canonical slots import the same alias only when coercion and alias routing law is identical — env stringly coercion belongs on settings-specific aliases even when numeric bounds match.

# Field Constraint And Validator Interaction

- `Field(strict=True|False)` on an `Annotated` alias overrides class-level `ConfigDict` strictness for that slot only — per-field strict exceptions pair with stringly env keys on settings aliases without mutating the compiled class schema.
- `Field(validate_default=True)` runs the full `Annotated` stack against defaults at class definition — defaults must survive plain and wrap layers, not only core type nodes.
- `Field(discriminator=...)` on union aliases must sit on the closed union alias, not on individual arms — moving discriminator metadata to a child arm breaks tagged-union compilation and OpenAPI `oneOf` mapping.
- `json_schema_input_type` on `field_validator` documents wire types when Python annotations differ from accepted ingress — the override applies to model fields, not to reusable `Annotated` aliases unless the alias carries equivalent `WithJsonSchema` validation-mode metadata.
- Cross-field invariants never belong inside `Annotated` functional validators — `model_validator(mode='after')` on the owning model is the only cross-slot admission surface.

# Serializer Stack And Wire Divergence

- `PlainSerializer` and `WrapSerializer` on aliases define egress law independent of ingress validators — validation-type and wire-type divergence is intentional; snapshot rows declare dump mode (`python` vs `json`) per projection seam.
- `when_used='json'` on alias serializers limits transformation to JSON dumps — Python-runtime `model_dump(mode='python')` retains pre-serializer types for interior folds and downstream struct conversion lanes.
- `SerializeAsAny[T]` on field annotations widens serialization, not validation — alias-level serializers on `SerializeAsAny` fields require explicit polymorphic proof rows when runtime subclasses change wire keys.
- `@computed_field` cannot carry `Annotated` serializer stacks — computed egress uses property return types; wire divergence for computed slots uses wrapping `model_serializer` on the owner.
- Stacking alias serializers with `@field_serializer` on the same logical slot duplicates law — pick alias serializers for reusable encodings, field serializers for owner-local exceptions only.

# WithJsonSchema And Core Hook Placement

- `WithJsonSchema({...})` on aliases attaches vendor extensions and mode-specific wire shapes without mutating Python types — overrides belong on the alias, not post-processed `model_json_schema()` dict surgery.
- `__get_pydantic_json_schema__` on custom types adapts compiled core nodes — JSON overrides stay in the hook; core schema stays transport-agnostic; alias `WithJsonSchema` handles alias-local documentation, not multi-step core logic.
- Validation-mode and serialization-mode JSON Schema may differ for the same alias when serializer stacks transform egress — dual-mode snapshot pairs per alias family, not single-mode alias exports.
- `Field(json_schema_extra=...)` on embedding fields adds field-local OpenAPI metadata atop alias-generated schema — prefer alias-level extras for concept-wide examples; field extras for embedding-site documentation only.

# Compilation And definition-ref Identity

- `GenerateSchema` collapses identical `Annotated` stacks to shared `definition-ref` nodes when metadata tuples match — cosmetic reordering of metadata objects can defeat deduplication and inflate schema diffs without semantic change.
- `handler.generate_schema(inner)` inside `__get_pydantic_core_schema__` must not leak outer `Annotated` context into unrelated inner parameters — `handler(T)` preserves context and is reserved for intentional contextual compilation only.
- `handler.field_name` branches per-embedding-site metadata on cooperatively typed classes — same Python type, different bounds at different field names, compiles distinct core nodes; persistence keys must include embedding path when bounds affect stored shape legality.
- Module-level `TypeAdapter(Alias)` and model field embeddings of `Alias` warm the same `definition-ref` when imported from one module — split alias definitions across modules duplicate refs until schema generator unifies them.
- Worker entrypoints import modules defining alias symbols before first `model_validate` — lazy first-touch alias compilation in request handlers is a warm-up defect; generic alias adapters (`TypeAdapter(Box[int])`) cache per concrete argument tuple at composition root.

# PEP 749 Annotationlib On Alias Embeddings

- Python `>=3.15` defers annotations through `__annotate__` (PEP 749); alias parity proofs read resolved field types through `annotationlib.get_annotations`, not eager `__annotations__` mutation or legacy `typing.get_type_hints` alone.
- `annotationlib.get_annotations(owner, format=annotationlib.Format.VALUE, include_extras=True)` must recover the full metadata tuple on fields referencing alias symbols — `include_extras=False` collapses `Annotated` to bare `T` and hides stack law from static parity proofs.
- `annotationlib.Format.FORWARDREF` admits factory reads that must not import unresolved peers during collection — forward-ref owners still require quoted annotations plus `model_rebuild()` before production validation; FORWARDREF reads do not substitute for rebuild.
- `annotationlib.Format.STRING` preserves unevaluated references for signature renderers — runtime validation and schema compilation require VALUE format at first-touch warm-up, not STRING snapshots alone.
- Law-matrix emission comparing `model_fields` types against alias declarations runs after `model_rebuild()` on owners embedding forward-ref aliases — FORWARDREF reads prove factory codegen only, not runtime parity.
- Mypy `pydantic.mypy` plugin reads alias-expanded field types on owners — alias definitions must typecheck independently before embedding; orphan aliases outside root import smoke are static-proof defects.
- PEP 695 `type Alias[T] = Annotated[T, ...]` parametrizes alias families — `get_args` on parametrized aliases includes constraint metadata per argument tuple; generic adapter pins must use concrete specializations.

# Proof Obligations On Stack Owners

- Core-schema oracle walks unwrap `function-before`, `function-after`, `function-wrap`, and `function-plain` nodes to inner constraint nodes — validator bodies opaque to walkers require `st.register_type_strategy` on the embedding model or explicit waiver rows on the alias module.
- Alias edits trigger promotion-unit regeneration: dual-mode JSON Schema snapshots for every publishing embedding, core-schema oracle sample loops, and cross-engine snapshot rows when alias serializers feed struct wire layouts.
- `resolve(T)` on alias targets registers strategies from the alias's compiled subgraph when predicates exceed core nodes — `st.from_type` alone under-generates alias law identically to naive model resolution.
- Snapshot rows map alias `serialization_alias` and serializer transforms to wire rename policy — alias-only proof without embedding-site samples misses `handler.field_name` branching defects.
- Discriminated union alias families sample one generated instance per arm through `resolve(Arm).example()` → `model_validate` — alias-level union edits without per-arm oracle loops are exhaustiveness defects.
- Settings aliases prove single-pass construction with `Field(validation_alias=AliasChoices(...))` collision matrices — alias widenings that change env admission require regenerated boot fixtures, not runtime-only validator edits.

# Discriminated Union Alias Patterns

- Closed union aliases (`type Pet = Annotated[Cat | Dog, Field(discriminator='pet_type')]`) are the single publication surface — duplicate union spellings on parent models drift `tagged-union` metadata in generated schema.
- Callable `Discriminator(fn)` plus `Tag('case')` on aliases routes non-uniform keys — nested unions wrap inner `Annotated[..., Field(discriminator=inner)]` before outer alias routing; failure at any level projects through boundary `loc` law.
- `RootModel[Literal[...]]` discriminators on 2.13+ compile root-type schema from alias arms — OpenAPI must not wrap object envelopes around root-form alias exports.
- Extension arms on alias unions require explicit policy rows — `extra='allow'` or untagged fallback unions break client codegen when alias publishes closed `oneOf`.
- Adding an alias union arm without updating OpenAPI discriminator mapping, snapshot samples, and wire tags is a partial contract publish blocked at root CI.

# Settings And Env Alias Specialization

- Settings aliases carry `Field(validation_alias=AliasChoices('LEGACY', 'CANONICAL'))` for env name multiplicity — normalization stays in `field_validator(mode='before')` on the alias or embedding settings model, not in custom source classes.
- `SecretStr` and `SecretBytes` on aliases redact `repr()` and default dumps — settings aliases never pair with `PlainSerializer` that leaks raw secret material into operator diagnostics.
- Nested `BaseSettings` submodels reference child settings aliases — parent construction validates child shapes independently; child `extra='forbid'` catches env namespace leakage before configuration slices read canonical field values.
- `populate_by_name=True` on settings owners admits alias and field name on env ingress — configuration projection reads Python field names from the validated instance after alias law is fully consumed at construction.
- Settings JSON Schema for alias-typed fields is operator-facing only — client-published OpenAPI omits settings alias families unless every constraint is secret-free by policy review.

# SkipValidation And Trusted Alias Slots

- `SkipValidation` on alias interiors marks slots upstream already closed on trusted same-process artifacts — boundary-facing model fields referencing `SkipValidation` aliases without trusted-replay rows violate admission tier law.
- Wire-sourced field replacement on alias-typed slots requires full `model_validate` replay of the alias stack including `BeforeValidator` widenings — `model_copy(update=...)` cannot substitute for alias law on foreign-carrier deltas.
- Trusted same-owner alias field swaps use `model_copy` / `copy.replace` only when the base instance passed full validation at materialization — document trust context in the owning transition method when callers cannot infer ingress versus interior provenance.
- Child model re-admission before parent replace: nested dicts validate through child schema before `model_copy(update={'child': validated})` — never assign raw nested dicts into frozen parents carrying alias-typed child slots.
- Pickle and `copy.deepcopy` traverse alias-validated models but bypass explicit version gates — dump+validate re-admission remains mandatory at cross-process seams unless trusted-replay rows document encoder and schema version pins.

# Predicate And Opaque Constraint Law

- `Predicate(fn)` from `annotated-types` encodes semantic law invisible to core-schema walkers — every predicate alias carries paired `WithJsonSchema` validation-mode override or explicit waiver row naming the oracle gap.
- Predicate failures surface as generic validation errors unless validators raise `PydanticCustomError` with typed codes — boundary projection maps custom codes through the same `loc`+`msg` law as core nodes.
- Combining `Predicate` with `Field(pattern=...)` stacks regex core nodes before predicate calls — predicate receives core-validated values; regex rejection never reaches opaque predicate bodies.
- Hypothesis registration for predicate aliases belongs on the embedding model strategy, not on bare `T` — `st.register_type_strategy(EmbeddingModel, strategy)` must generate values satisfying predicate law, not annotation-inferred samples alone.

# Alias Rename And Promotion Units

- Renaming a module-level alias symbol without changing metadata tuple is a refactor-only event — `definition-ref` identity and compiled subgraph bytes may remain stable; importers update import paths only.
- Changing metadata tuple order or constraint values on an alias is a breaking promotion unit — dual-mode JSON Schema diffs, oracle sample loops, cross-engine snapshot rows, and OpenAPI discriminator tables regenerate together.
- Splitting one overloaded alias into ingress-normalization and canonical-constraint siblings is a promotion unit — handoff constructor return types and adapter map rows update in the same merge-blocking change.
- Merging sibling aliases with identical metadata tuples collapses `definition-ref` churn — verify with `TypeAdapter.json_schema()` ref counts before merge; spurious duplicate refs indicate embedding-site `Field` divergence.
- `schema_version: Literal[...]` on owners embedding alias unions is durable shape law — alias constraint changes that break stored layout require version bump, migration fold, and dual-mode snapshot regeneration in one promotion unit.
- Breaking alias routing (`validation_alias`, `serialization_alias`, `AliasChoices`) is breaking for any consumer keyed on wire paths — run round-trip proof under old and new alias samples before merge.
- Breaking serializer transform on an alias (`PlainSerializer`, `when_used='json'`) is breaking for egress consumers — serialization-mode schema snapshot must diff even when Python annotations are unchanged.
- Obsolete alias union arms remain in root migration modules with exhaustive folds — active ingress and canonical models expose only current alias symbols; `assert_never` witnesses retired tag vocabularies.

# Collapse Tests For Alias Stacks

- Per-field inline `Annotated` duplicate of a module alias — collapse to single alias symbol imported across all embeddings.
- `TypeAdapter` constructed over inline `Annotated` spelling equivalent to a pinned alias constant — collapse to root `Final[TypeAdapter[Alias]]` warm-up.
- Post-hoc `model_json_schema()` dict patches replacing alias `WithJsonSchema` — collapse to alias-level metadata and `__get_pydantic_json_schema__` hooks.
- Alias serializer plus owner `@field_serializer` on the same logical slot — collapse to one encoding surface per concept.
- `include_extras=False` in `annotationlib` parity proofs on alias-embedding owners — collapse to `include_extras=True` law-matrix rows.
- Domain import of ingress-only unwrap aliases for interior typing — collapse to canonical types plus handoff constructor return types.
- `SkipValidation` stacked with `BeforeValidator` widening on the same boundary alias — collapse to one trust posture per alias family.

# Anti-Patterns

- Inline duplicate `Annotated` stacks across models instead of module aliases; `PlainValidator` on boundary-facing keys; `SkipValidation` stacked with ingress widening validators; post-hoc `model_json_schema()` dict patches replacing `WithJsonSchema`; per-request `TypeAdapter` over alias symbols; alias serializers plus duplicate `@field_serializer` on the same slot; `handler(T)` leaking contextual metadata in cooperative schema hooks; `include_extras=False` in parity proofs; conditional validator branches encoding wire versus interior law inside one alias; cosmetic metadata reorder defeating `definition-ref` deduplication without semantic change.
