# Static–Runtime Decode Planes

# Three Evidence Planes

- Materialization admits three non-interchangeable evidence planes: **static** (checker-visible contracts), **compiled** (engine graphs warmed once per process), and **runtime** (immutable instances domain logic consumes).
- Plane identifiers are closed vocabulary — `{static, compiled, runtime}` — used on lattice rows, alignment sub-rows, closure joins, and harness attribution; stage names from ingress-to-materialization doctrine and projection families annotate rows but never substitute for plane tags.
- Static plane owns `TypedDict` openness, `Required`/`NotRequired`/`ReadOnly`, `Unpack` keyword bundles, `TypeIs` narrowing, and `match` exhaustiveness — it proves shape legality before execution, not after decode.
- Compiled plane owns `__pydantic_core_schema__`, module-level `TypeAdapter`/`SchemaValidator`, and `msgspec.inspect.type_info` — shape law materializes here at import or first compile, not on each ingress call.
- Runtime plane owns frozen `BaseModel`, `Struct`, rich class owners, and `frozendict` snapshots — the sole durable artifacts past materialization exit; static and compiled planes do not substitute for them in domain modules.
- A concept may exist on one plane only until promotion: `TypedDict` is static-only; wire `Struct` is compiled+runtime; rich owners are runtime-only with static ports — never collapse planes by erasing to `object` or `dict[str, Any]`.
- Promotion path is unidirectional across planes — static payload → compiled validation → runtime owner — reverse projection for egress is lossy only at declared omit/rename/cap fields; accidental dump drift is not a fourth plane.
- Proof harnesses must name which plane each obligation targets — static checker, compiled oracle, or runtime metamorphic — mixing plane identifiers in one law module produces false-green coverage.
- Plane non-interchangeability is equilibrium invariant — static assignability does not discharge compiled decode; compiled oracle snapshot passing does not discharge runtime round-trip; runtime instance equality does not discharge static exhaustiveness on variant arms.

# Plane Evidence Obligations

- Each plane admits one primary proof modality — static: checker and import architecture; compiled: oracle snapshot and singleton identity; runtime: metamorphic law, smoke injection, and `beartype` after materialization exit.
- Static obligations prove ingress contracts and handler narrowing before execution — `TypedDict` closedness, `Unpack` boundary placement, `TypeIs` complement safety, `match` exhaustiveness on canonical owners.
- Compiled obligations prove graph identity and layout law — `TypeAdapter.json_schema()` / `core_schema`, `msgspec.inspect.type_info`, module-level singleton reference identity, `annotationlib.Format.VALUE` field-key parity against `model_fields`.
- Runtime obligations prove durable artifact law on materialized values — round-trip bijection, envelope one-write invariant, cap truncation spill, consumer decode parity through pinned decoders.
- Three-row alignment sub-lattice (`_alignment_static`, `_alignment_compiled`, `_alignment_runtime`) binds every cross-family seam — static contract symbol, compiled owner symbol, runtime handoff type; prose seam lists without sub-rows fail `plane_stage_consistency_join`.
- Harness terminal order stacks plane layers — static import and payload law before compiled oracle diff before runtime metamorphic and smoke — static failure blocks generative suites regardless of runtime smoke green.

# Static Ingress Contracts

- Ingress static contracts are pre-materialization shapes: closed `TypedDict` (PEP 728), `Unpack[Payload]` on adapter signatures, and PEP 695 `type` aliases for union admission — they type what may arrive, not what domain already proved.
- PEP 728 `closed=True` removes extension keys from static proof; `extra_items=T` types a bounded extension band; default openness still rejects construction-time undeclared keys — static assignability and construction postures diverge by design.
- `ReadOnly[T]` on payload keys is static non-mutation evidence for ingress consumers; durable immutability still requires frozen materialized owners — `ReadOnly` does not replace `ConfigDict(frozen=True)` or `Struct(frozen=True)`.
- PEP 692 `Unpack[TypedDict]` types keyword ingress at the adapter callable edge; unpack into validation inside the adapter body immediately — `Unpack` must not appear on domain module signatures past validation exit.
- PEP 742 `TypeIs[T]` narrows materialized owners after validation and `beartype` — complement-safe narrowing for consumer and handler interiors; `TypeGuard` plus `cast` chains at materialization seams are rejected.
- Static exhaustiveness (`match` + `assert_never`) and runtime discriminator routing are separate obligations — schema-complete unions and domain-complete `match` arms both must pass; neither substitutes for the other.
- Checker gaps on PEP 728 inheritance edges are proof debt on the language axis — payload law is authored from the 3.15 typing spec; integration CI runs pyright on representative payload modules when mypy lags.

# Annotation Introspection Bridge

- PEP 649 defers annotation evaluation through `__annotate__`; PEP 749 makes `annotationlib` the canonical read path — raw `__annotations__` misses deferred thunks and strips `Annotated` metadata.
- Composition-root codec factories and decorator admission stacks read `annotationlib.get_annotations(owner, format=annotationlib.Format.VALUE)` when constraints, `Field`, or `msgspec.Meta` layers are load-bearing at definition time.
- `format=annotationlib.Format.FORWARDREF` resolves quoted and forward names for schema-build tooling — use at compile-time graph construction, not at hot decode paths.
- `include_extras=True` is mandatory when `Annotated` stacks carry `BeforeValidator`, `Field`, or `msgspec.Meta` — default reads collapse to bare `T` and silently drop admission metadata.
- PEP 747 `TypeForm[T]` types polymorphic ingress targets in adapter registries — `TypeAdapter`/`Decoder` selection keys on `TypeForm`, not `type[Any]` or erased `object` carriers.
- Pydantic `GenerateSchema` and msgspec struct layout both consume resolved annotation values — static declarations, annotationlib reads, and compiled graphs must agree on field order, optionality, and constraint nodes.

# Compiled Graph Owners

- Pydantic compiled graph: `GenerateSchema` → `CoreSchema` on `__pydantic_core_schema__` → immutable `SchemaValidator` behind models and module-level `TypeAdapter` singletons — per-request `TypeAdapter()` recompiles and breaks cache/proof identity.
- msgspec compiled graph: struct layout, `encode_name`, `tag_field`, `Meta` constraints, and `forbid_unknown_fields` policy materialize at class body execution — `msgspec.inspect.type_info` is the oracle for layout proofs without Pydantic dependency.
- Dual compiled graphs for one concept are forbidden at domain interior — ingress Pydantic graph and egress msgspec graph may coexist only across a documented projection row at the boundary.
- `model_rebuild()` resolves forward-ref-deferred Pydantic nodes only — field-set mutations require a new class definition, not repeated rebuild in production; msgspec struct field changes require re-import, not runtime patch.
- Warm compiled graphs at composition-root startup when cold-start latency matters — lazy first-touch compilation is not an optimization path for hot ingress or egress lanes.

# Stage-To-Plane Mapping

- Pipeline stages from ingress-to-materialization doctrine map onto planes — conflating stage names with plane names produces harness tables that pass while seams drift.
- Ingress payload and outbound serialization stages sit at transport boundaries — carriers (`bytes`, `str`, staging `Mapping`) are pre-runtime; encoded `bytes` exit is post-runtime projection.
- Validation and normalization stages execute on the **compiled** plane — Pydantic `SchemaValidator` and msgspec decode hooks are graph operations producing **runtime** instances.
- Construction, enrichment, and immutable materialization stages produce and transform **runtime** owners only — static payloads and compiled graphs do not enter domain folds.
- Normalization that reshapes wire keys runs inside the validation owner's compiled graph (`BeforeValidator`, `model_validator(before)`, `dec_hook`) — a separate normalization stage owner is logical, not a second decode pass.
- Enrichment and smart constructors may consult **static** ports (`Protocol`) and return `Result[RuntimeOwner, E]` — they do not re-parse **static** payloads or recompile graphs.
- Round-trip proof and egress encode read **runtime** owners through **compiled** encoders — proof witnesses bijection between runtime identity and wire bytes, not between static payloads and JSON Schema.

# Ingress Decode Matrix

- Decode owner selection is a function of **carrier** × **trust posture** × **target compiled graph** — not of caller convenience or module proximity.
- `bytes`/`str` wire: single-pass `TypeAdapter.validate_json` (Pydantic targets) or tagged `Decoder.decode` / `msgspec.json.decode` (Struct targets) — `json.loads` then `validate_python` is a static and runtime defect.
- `dict[str, object]` / ORM views: `TypeAdapter.validate_python` (Pydantic) or `msgspec.convert(..., type=Struct)` (trusted-internal semi-shaped carriers) — never route untrusted dicts to bare `Struct(...)` construction.
- `TypedDict` views: static contract only until `TypeAdapter[Payload].validate_python` promotes to validated staging; promotion to `BaseModel`/`Struct` is the construction gate, not a TypedDict method call.
- Untrusted external wire always lands on Pydantic compiled graph or msgspec decode with explicit `forbid_unknown_fields` and boundary `capture` — semi-trusted internal service payloads still validate once; trusted-replay may use pinned `Decoder` without Pydantic when store key, schema version, and encoder identity are composition-root pinned.
- Partial stream ingress (`experimental_allow_partial`) admits incomplete documents at Pydantic validation exit only — downstream construction and domain invariants assume intentional partial closure; msgspec decoders require complete frames.
- `Buffer` and `__buffer__` carriers (PEP 688) still complete to `bytes` at the adapter before msgspec decode — buffer views are not a parallel decode plane.

# Trust Posture Across Planes

- Trust posture labels which plane may skip work — it never removes the compiled graph for untrusted carriers or the runtime owner requirement past materialization exit.
- Untrusted external wire: static payload contracts guide adapter signatures; compiled Pydantic or constrained msgspec decode is mandatory; runtime owners emerge only after validation exit.
- Semi-trusted internal payloads: compiled decode once at ingress; `forbid_unknown_fields` / `extra="forbid"` stays on; static openness on staging payloads does not downgrade runtime strictness.
- Trusted-internal same-process struct graphs: runtime instances may short-circuit re-validation when session provenance is pinned — static contracts still type handler edges via `beartype`.
- Trusted-replay from store bytes: compiled `Decoder` identity and `schema_version` literal are pinned at composition root — static migration tables type each version hop; runtime `model_construct` is permitted only under trusted-replay rows.
- Never-trusted carriers (`json.loads` output, live ORM `__dict__`, partial LLM buffers without partial admission) re-enter at compiled validation regardless of upstream labels — static narrowing on closed payloads does not apply to untrusted dict material.

# Envelope Decode Planes

- Versioned envelopes use two compiled passes when body type depends on envelope metadata — static contracts type each pass target separately; runtime body selection follows pass-one discriminant closure.
- Pass one: closed envelope struct on msgspec compiled graph — `schema_version` is a static+runtime closed `Literal`; unknown versions fail before body decode allocates runtime slots.
- Pass two: body `Decoder` selected from version + discriminant — `msgspec.Raw` holds unparsed body bytes between passes on the wire plane without eager polymorphic runtime materialization.
- Mutual exclusivity of success and fault body slots is a runtime invariant enforced in `model_validator(mode="after")` or `__post_init__` — static payloads do not express mutual exclusivity without promotion to compiled owners.
- Cap metadata (`Meta(max_length=...)`) is compiled-oracle evidence read via `msgspec.inspect` — static int literals in adapters duplicate cap law and drift from wire structs.

# Pydantic–msgspec Seam Law

- Engine split is invariant-class split: Pydantic owns JSON Schema-rich ingress, settings, discriminated unions, env coercion, and `ValidationError` surfaces; msgspec owns zero-copy encode/decode, deterministic wire rows, `gc=False` hot structs, and volume egress.
- Cross-family handoff executes once per concept in one boundary expression: `msgspec.convert(validated, WireStruct)`, owner `to_wire()`, or adapter-owned field table — never `model_dump` → dict surgery → `Struct(**d)`.
- Ingress model and egress struct may diverge by version arm — OpenAPI reflects Pydantic ingress; storage and stdout reflect msgspec egress; `msgspec.convert` migration folds bridge stored egress to current egress, not ingress replay.
- Domain modules import one engine per concern — interior owners do not import `msgspec` encoders or Pydantic `TypeAdapter`; projection adapters own both graphs at the package boundary.
- `beartype` decorates boundary callables after validation materializes typed values — it is a runtime gate on compiled-graph products, not a second ingress validation pipeline duplicating Pydantic or msgspec.
- Normalization hooks (`BeforeValidator`, `model_validator(before)`, `dec_hook`/`enc_hook` tables) stay on the owning engine — do not split wire key collapse across Pydantic and msgspec without a documented adapter row.

# Static–Runtime Alignment Obligations

- Every admitted materialization seam declares a three-row alignment table: static contract (payload or annotation), compiled owner (`TypeAdapter` symbol or `Struct` class), runtime handoff artifact type — prose seam lists without table rows are rejected.
- `TypeAdapter.json_schema()` / `core_schema` and `msgspec.inspect.type_info` supply compiled-oracle snapshots for contract tests — static payload keys must map to compiled field names via alias policy declared once in the adapter module.
- Promotion path is always unidirectional: static payload → compiled validation → runtime owner — reverse projection for egress is lossy only at declared omit/rename/cap fields, not accidental dump drift.
- `validate_call` and `validate_strings` signatures are schema sources — erased parameters (`Any`, bare `dict`) break static-runtime alignment for public API surfaces.
- After runtime materialization exit, interior code treats instances as trusted for shape legality — re-validation of canonical owners is a boundary defect unless trusted-replay policy explicitly pins snapshot bytes and encoder identity.
- Discriminant vocabulary is declared once on the runtime owner — static literals, `StrEnum` values, and msgspec `tag` strings align through compiled graphs; adapters map external synonyms only at ingress and egress, not in interior `match` arms.

# Consumer And Handler Planes

- Automation consumers operate on **runtime** envelope structs decoded through pinned **compiled** decoders — static stdout parser types document field access, not `json.loads` dict views.
- Pass-one consumer logic proves `schema_version`, `claim`, and `verb` on runtime envelope instances before interpreting polymorphic body slots — static narrowing on closed envelope payloads mirrors but does not replace decode.
- Handler interiors narrow with `TypeIs` and exhaustive `match` on **runtime** canonical owners — consumer and handler planes reject `TypeGuard` followed by `cast` after materialization exit.
- Success-path `detail` slots may assume round-trip proof ran at root egress — consumers do not re-run `validate_detail` on every line; proof is a runtime–compiled obligation at emit, not consumer business logic.
- `ReadOnly` on ingress staging payloads does not propagate to consumer mutation policy — consumers treat decoded runtime structs as the authoritative immutability contract.

# Python 3.15 Materialization Gates

- `requires-python >=3.15` baseline: PEP 728 explicit TypedDict openness, PEP 649/749 deferred annotations, PEP 695 `type` aliases, PEP 742 `TypeIs`, PEP 814 `frozendict` for immutable mapping materialization, PEP 686 UTF-8 default text.
- `frozendict` materializes immutable mapping snapshots at enrichment and persistence boundaries — tuple-pair encodings and thin mapping wrappers are banned replacements.
- Ruff `target-version = "py315"` and `runtime-evaluated-base-classes` admit msgspec and pydantic constructors as immutable-call surfaces — materialization code uses `A | B`, `type` aliases, and unquoted annotations throughout.
- Free-threaded builds (PEP 779): module-level `Encoder`/`Decoder`/`TypeAdapter` singletons must finalize before parallel importers bind root symbols — post-import reassignment of codecs races under parallel test workers.
- `annotationlib` replaces introspection hacks that mutated `__annotations__` on decorator stacks — admission factories read, never write, annotation maps.

# Projection Family Plane Assignment

- Ingress projection family (Pydantic frozen models, settings, `TypeAdapter` targets) spans **static** payload staging plus **compiled** Pydantic graphs — runtime product is a validated model or scalar container, not a wire struct.
- Domain projection family (rich class owners, frozen dataclasses, variant families) is **runtime**-primary — **static** `Protocol` ports type capabilities; **compiled** graphs do not re-encode domain law.
- Wire projection family (msgspec `Struct`, `tag_field`, deterministic encode) is **compiled**+**runtime** — no domain methods; **static** contracts appear only on adapter-owned staging payloads mirroring wire keys.
- Cross-family projection is a boundary-only **compiled** operation (`msgspec.convert`, `to_wire()`) from **runtime** domain to **runtime** wire struct — interior modules hold one family per concept.
- JSON Schema export reads **compiled** Pydantic graphs — OpenAPI consumers align with ingress family; stdout and cache consumers align with wire family oracles (`msgspec.inspect`), not mixed oracles on one surface.
- Settings bootstrap materializes **runtime** frozen settings once via **compiled** Pydantic graph at composition root — env slices are untrusted carriers until validation produces runtime settings owners.

# Subinterpreter And Process Plane Closure

- PEP 734 subinterpreters (`concurrent.interpreters`) and multiprocessing workers treat each interpreter or process as an independent composition root — parent-process compiled graphs, `SchemaValidator` handles, `BaseSettings` singletons, and admission certificates are not portable object references across seams.
- Subinterpreter closure unit is the smallest boot-atomic change set restoring compiled-plane identity in the child root — warm every owner on the worker path, replay `annotationlib.Format.VALUE` field-key parity against `model_fields`, finalize module-level `TypeAdapter`/`Encoder`/`Decoder` singletons, complete `BaseSettings` construction before parallel importers bind handler entrypoints.
- Cross-interpreter and cross-process handoff transmits `bytes` plus `schema_version` and encoder-module identity metadata — never pickle compiled validators, settings singletons, or parent `TypeAdapter` instances unless a `trusted_replay` lattice row documents encoder pins and version preconditions.
- Worker boot admission certificate replays per owner on the worker path: VALUE keys match `model_fields`, forward-ref families call `model_rebuild()` before first validation, pinned adapter symbols resolve without per-call `TypeAdapter()` — certificate failure blocks subinterpreter publish, not domain retry.
- Free-threaded builds (PEP 779) and parallel test workers share compiled-plane finalize law — module-level codec singletons must complete assignment before importers bind root symbols; post-import reassignment of `_ENCODER`, `_ENVELOPE_DECODER`, or `_DETAIL_DECODER` races and fails `warm_graph_finalize_before_import` closure.
- Subinterpreter publish row (`subinterpreter_publish_closure`) joins `warm_graph_finalize_before_import`, `codec_singleton_identity`, and admission certificate replay — child interpreter must not expose `rail(bind)` until compiled-plane witness and singleton identity smoke pass in that interpreter's root import graph.
- Process spawn replay prefers `model_dump` + `model_validate` or deterministic `wire_encode` bytes when schema version enforcement matters — deepcopy and pickle bypass version gates unless trusted-replay rows document encoder identity pins.
- Vocabulary and enum closure parity across forked importers remains proof debt until worker-boot CI gate lands — subinterpreter closure rows cite `sunset_criterion` on composition-root promotion record; interim replay depends on root module path stability and pinned decoder construction per composition-root boundary-limits open proof.

# Pipeline Closure Cross-Cutting Join

- `PIPELINE_CLOSURE` is the federated join table witnessing equilibrium across six primary machine tables — `TRANSITION_LATTICE`, `ORACLE_CONFORMANCE`, `FIELD_POLICY`, `ATTRIBUTION_LATTICE`, `SMOKE_FEDERATION`, `EVOLUTION_OBLIGATIONS` — prose in ingress-to-materialization doctrine through attribution smoke evolution calculus is normative commentary; closure rows are the enforceable foreign-key graph.
- Closure row shape is frozen: `closure_id`, `invariant_kind`, `primary_table`, `primary_row_id`, `foreign_table_ids`, `foreign_row_ids`, `proof_layers`, `collapse_row_id`, `blocking_merge` — closure rows never duplicate primary table bodies; they witness join integrity only.
- `invariant_kind` draws from closed vocabulary — `{handoff_surface_parity, attribution_transition_parity, smoke_signal_parity, consumer_evolution_parity, singleton_identity, promotion_atomicity, plane_stage_consistency}` — not free-form checklist items or ordinal wave labels.
- `proof_layers` stacks orthogonal gates named by plane — static import architecture, compiled oracle snapshot, lattice contract parametrization, metamorphic round-trip, runtime smoke injection — earliest failing layer blocks later layers per harness terminal order.
- Plane-stage closure rows in this document anchor the join catalog — full equilibrium calculus and reconciliation choreography live in pipeline equilibrium closure calculus; readers need not leave static-runtime decode-planes doctrine to understand how planes bind to closure.
- `plane_stage_consistency_join`: every handoff transition contract lattice row `upstream_plane` and `downstream_plane` must align with stage-to-plane mapping in this document — using `validation` stage name as plane tag without explicit `{static, compiled, runtime}` column fails harness attribution.
- `warm_graph_finalize_before_import`: primary compiled-plane law from this document; foreign keys to free-threaded boot rows and `subinterpreter_publish_closure` — post-import singleton reassignment fails singleton identity and metamorphic byte laws together.
- `codec_singleton_identity`: joins lattice rows naming `wire_encode`, `_ENVELOPE_DECODER`, `_DETAIL_DECODER` with smoke decoder symbol and oracle symbol rows — production and conftest must alias the same module object; shadow encoders in test helpers fail closure at static layer.
- `transition_surface_join`: every lattice row with `proof_required=true` on polymorphic slot cites matching oracle conformance surface matrices row on `concept_owner` and `lattice_transition_ids` — plane tags on lattice row must match surface `projection_family` plane assignment.
- Promotion closure unit is merge-atomic across primary tables and affected `PIPELINE_CLOSURE` rows — partial promotion leaves dangling foreign keys; `blocking_merge=true` defaults on production-admitted seams unless composition-root exemption registry cites `closure_id`.
- Proof debt from checker gaps declares rows beside `PIPELINE_CLOSURE` with `sunset_criterion` — harness suppressions and `cast` escapes at materialization seams are rejected, not debt; worker boot singleton parity and nested patch one-expression seam are admitted debt rows until sunset.

# Anti-Patterns Across Planes

- Using static `TypedDict` openness to justify skipping runtime validation on untrusted wire — static proof is not ingress admission.
- Reading `__annotations__` without `annotationlib` in codec or decorator factories — drops deferred and `Annotated` metadata silently.
- Holding Pydantic ingress model and msgspec wire struct as co-equal domain types for one concept — collapse to one runtime owner with projection folds.
- Running `msgspec.Struct(...)` on untrusted dicts without `convert`/`decode` constraints — bypasses compiled graph enforcement.
- Applying `experimental_allow_partial` then enforcing full domain closure invariants in the same pipeline without documenting intentional partial shape.
- Proving only static assignability or only runtime round-trip — dual-plane concepts require both compiled-oracle and metamorphic witnesses.
- Pickling parent `TypeAdapter` or `SchemaValidator` across subinterpreter seams — bypasses admission certificate and `subinterpreter_publish_closure` rows.
- Hand-maintained join tables or prose checklists beside `PIPELINE_CLOSURE` — collapse to closure tuple foreign keys at composition root.
- Publishing `rail(bind)` in child interpreter before compiled-plane warm-up and singleton identity smoke — violates `subinterpreter_publish_closure` and `warm_graph_finalize_before_import`.
