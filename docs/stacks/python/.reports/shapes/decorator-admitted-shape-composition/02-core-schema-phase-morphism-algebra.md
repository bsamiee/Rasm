# Core-Schema Phase Morphism Algebra — Compile Graph And Parametric Admission

# Compile Graph As Admission Artifact

- Pydantic class-body decorators admit executable law by compiling into a directed acyclic graph of pydantic-core schema nodes; Python decorator application order on methods is evidence input, not the runtime walk order.
- `GenerateSchema` executes during class-body and first-touch phases; each admitted `@field_validator`, `Annotated` validator, `@model_validator`, and `@computed_field` maps to tagged nodes (`function-before`, `function-after`, `function-wrap`, `computed-field`, `model`) with stable discriminator metadata.
- Compiled graph is the canonical admission receipt for pydantic owners; `cls.model_fields` is the public projection, `cls.__pydantic_fields__` is internal `FieldInfo`, `cls.__pydantic_core_schema__` is the phase morphism target—source decorators are hypotheses until the graph agrees.
- First-touch `TypeAdapter(T)` and `model_rebuild()` materialize deferred forward references and generic bindings into the same graph family; decorator authors treating import-time `model_fields` as final without first-touch proof admit stale phase morphisms.

# Decorator-To-Node Morphism Table

- `@field_validator(mode="before")` admits `function-before` nodes ahead of core type validation for the named field; multiple before validators on one field compile to an ordered chain keyed by method definition order within the same mode.
- `@field_validator(mode="after")` admits `function-after` nodes after core validation; after nodes on one field run before cross-field `model_validator(mode="after")` on the whole model.
- `@field_validator(mode="wrap")` admits sandwich nodes receiving a `handler` that already includes inner plain and before phases; outer wrap decorators in source compile to outer runtime layers—reversing wrap order in source reverses sandwich semantics even when signatures are identical.
- `@field_validator(mode="plain")` replaces core validation for the field; admission requires `json_schema_input_type` when wire types diverge from Python types—plain nodes are terminal for the field slot unless an explicit wrap outer shell is declared.
- `@model_validator(mode="before")` admits model-level preprocessing before per-field chains begin; `@model_validator(mode="after")` admits terminal gates only after all fields materialize—after model validators never interleave with per-field after nodes on sibling fields.
- `Annotated[T, BeforeValidator(f), WrapValidator(g), Field(...)]` composes right-to-left at compile time with explicit phase tags; method validators on the same field merge into the graph by field name and mode, not by proximity in the class body.
- `@computed_field` admits derived slots into the model schema and JSON schema; computed nodes execute only after field materialization and never accept constructor input—duplicate undecorated `@property` projections outside the graph are parallel registries, not compile evidence.
- `@field_serializer` and `@model_serializer` admit egress-only nodes that do not participate in ingress `model_validate` unless a single method illegally combines both concerns—which is rejected.

# Annotated Stack Reduction Laws

- `Annotated` metadata reduces with right-to-left composition: `Annotated[T, A, B, C]` applies `C` then `B` then `A` on the ingress path toward core `T` validation—decorator factories must not assume left-to-right Python source order for `Annotated` chains.
- `BeforeValidator` and `AfterValidator` on the same `Annotated` slot compile to distinct phase tags; swapping metadata order swaps runtime phase order without changing bare `T`.
- `WrapValidator` outermost in `Annotated` metadata (leftmost in conventional spelling) compiles outside plain and before inner metadata—factories merging `Annotated` stacks from multiple mixins must preserve reduction order, not dict insertion order from namespace merges.
- `Field(...)` inside `Annotated` admits specifier law at the field slot; `include_extras=True` on every factory read that compiles validators—stripping extras collapses admitted shapes to bare `T` and drops phase tags silently.
- `PlainValidator` inside `Annotated` is a terminal replacement for core validation on that slot; pairing plain with wrap on the same scalar requires explicit conformance rows documenting wire versus Python divergence.
- Repo policy rejects post-class `model_fields` mutation to simulate `Annotated` stacks—specifier and `Annotated` admission happen at class-body compile time only.

# Per-Field Phase Pipeline Morphism

- Runtime ingress on one field walks `before` → core type validation → `after` → `wrap` terminal handlers for that field; the morphism is per-field first, then model-level after validators across the constructed instance.
- Cross-field invariants that require multiple materialized fields belong in `model_validator(mode="after")`, not in `mode="before"` on a single field—before field validators see partial inputs and cannot close multi-field law without leaking ordering assumptions.
- `mode="wrap"` handlers receive a `handler` callable representing the inner compiled subgraph; decorator tests that mock only the outer body without invoking `handler` under-test miss sandwich regressions.
- Serializer phases are egress-only morphisms from validated instances to wire shapes; admitting serializers does not relax `frozen=True` or reopen constructor fields—serializer nodes are not reverse edges on the ingress DAG.
- Failure attribution follows phase tags: `ValidationError.loc` prefixes name field slots and validator indices derived from graph positions, not Python method `__qualname__` alone—harnesses diff `loc` paths against compiled node tags after dependency bumps.

# Parametric Specialization And Rebuild Morphism

- PEP 695 `class Owner[T](BaseModel)` binds type parameters at definition; `Owner[int]` and `Owner[str]` compile distinct `__pydantic_core_schema__` graphs after `model_rebuild()`—decorator caches key on `(cls, tuple(get_args(cls)))`, not erased generic names.
- Forward references in deferred modules resolve at rebuild; owners referencing not-yet-defined peers use string annotations or consistent `from __future__ import annotations` policy with `annotationlib.Format.FORWARDREF` reads during factory execution—VALUE reads before rebuild on unresolved peers are factory defects.
- First `model_validate`, `model_validate_json`, or `TypeAdapter.validate_python` on a parametrized specialization is the proof gate for compile-time decorator admission; CI must exercise first-touch on every concrete argument tuple referenced in production, not only monomorphic import fixtures.
- Bounds and defaults on type parameters (`T: str = str`) are admission evidence for `TypeAdapter` and field defaults; decorator factories that erase `T` to `Any` at wrapper boundaries break rebuild morphisms and static parity with `model_fields`.
- `Generic` inheritance order `class Box[T](BaseModel, Generic[T])` or PEP 695 compact form must preserve parameters through MRO; intermediate bases that drop `Generic` erase parameters and break validator bodies closing over `T`.
- Subclass decoration re-executes admission; subclass validators append to parent compiled pipelines in C3 MRO order—silent override requires `@typing.override` on intentional replacement methods.
- Module-level `TypeAdapter(T)` compiles a graph distinct from class-body `__pydantic_core_schema__` when `T` is a union, `TypedDict`, `Annotated` alias, or bare primitive—root-form adapters are first-class compile owners, not mirrors of a decorated class graph.
- Generic `TypeAdapter(Box[int])` caches per concrete argument tuple; hot paths import pinned adapter symbols from module constants, not per-call `TypeAdapter` construction—adapter drift without constant symbol update is a compile fingerprint defect.

# Discriminated Union Compile Nodes

- `Field(discriminator=...)` on closed union fields compiles `tagged-union` nodes with one subgraph per arm; callable `Discriminator(fn)` routes on `Literal` tags or explicit `None`—never widened `str` routing that absorbs unknown tags into a default arm.
- Each union arm carries independent `@field_validator` and `Field` specifier pipelines; shared validators on a mixin base append per arm in C3 MRO order unless `@override` marks replacement—validator graph appends per arm, not replaces silently.
- Nested unions compile nested discriminator nodes; inner `TypeAdapter` validation failure must not coerce to catch-all outer arm—each discriminator level owns independent error paths in the compiled graph.
- `@computed_field` on union arms compiles computed nodes only after arm materialization; ingress union members must not host enrichment decorators whose semantics never appear as compile nodes on the arm subgraph.

# Cross-Phase Interference And Node Collapse

- Import-phase catalog decorators must not depend on call-time validation results; call-time policy wrappers must not mutate `model_fields` or `__pydantic_core_schema__`—phase boundaries are not reorderable.
- Marker-only decorators without executable semantics do not emit compile nodes—lint rejects marker shims masquerading as admission.
- Post-class `model_config.update()` hides policy from checkers and may desynchronize compile graph from class-body `ConfigDict`—subclass config uses class-body assignment only.
- Identical `Annotated` metadata tuples collapse to shared `definition-ref` nodes when cosmetic reordering does not change semantics—metadata reorder without semantic change inflates schema diffs and defeats deduplication.

# Compile Oracle Fingerprints

- Class fingerprints after first-touch: `model_fields` keys versus `annotationlib.get_annotations(cls, format=Format.VALUE, include_extras=True)`; `__pydantic_core_schema__` tag multiset versus admitted decorator modes on each field.
- `TypeAdapter(cls).core_schema` oracle matches class schema after rebuild on generic specializations—diff tag sets on promotion without fingerprint row update fails owner harness.
- Root adapter fingerprints store `(adapter_id, tuple(get_args(T)), core_schema_tag_multiset)` beside pinned module constants—generic adapter drift without constant symbol update is a merge blocker.
- Oracle walks unwrap `function-before`, `function-after`, `function-wrap`, and `function-plain` nodes to inner constraint nodes—validator bodies opaque to walkers require explicit strategy registration or waiver rows on the owning module.
- Phase-attributed failure injection targets one compile phase per test: import-time duplicate registration, class-body conflicting transform, first-touch stale generic graph, egress serializer node regression—conflated attribution fails harness modules.

# Evolution On Compile Graph

- Pin `pydantic` minor upgrades behind dual-mode JSON Schema snapshot diffs and `__pydantic_core_schema__` oracle walks on every owner carrying validators, serializers, or computed fields—compilation success alone does not discharge decorator contract review.
- Adding or removing a validator decorator is a stack version event when wire acceptance width, cross-field law, or egress shape changes—regenerate core-schema oracle samples and JSON Schema snapshots in the same promotion unit.
- `mode="wrap"` to `mode="before"` refactors are version events when sandwich semantics change—regenerate core-schema oracle samples and conformance rows.
- Persisted owners carry `schema_version: Literal[...]`; read-path migration replays stored payloads through historical `TypeAdapter` compiled at stored version, then folds to current owner graph—validator removals on current owner do not rewrite stored bytes without version bump.
- Obsolete validator nodes remain in migration modules with exhaustive witnesses—live classes expose only the current decorator stack compile tags, not historical graph shapes on production owners.
- Fold totality on compile surfaces asserts every pydantic owner in a promotion unit has oracle walk coverage—owners carrying validators without tag multiset proof are exhaustiveness defects.
