# Typed Payload Contract Doctrine

# Critical Signals

- Typed payloads are pre-materialization static dictionary contracts only — boundary, staging, patch, event, and egress assignability slots; canonical owners, variant families, and wire structs occupy durable layers above the promotion gate.
- One payload name owns one contract; openness (`closed`, `extra_items`, default open), requiredness, and `ReadOnly` evidence are declared, not inferred from comments, runtime repair, or checker convenience.
- Promotion at the composition root is the sole lawful transition from payload evidence to materialized owners; interior modules accept owners or frozen extension snapshots, never assignable payload types or forwarded `**kwargs`.
- Contract reader emission is the single field-authority surface — `annotationlib` VALUE-grade folds emit rows that adapters, OpenAPI fragments, hypothesis strategies, and metamorphic chains consume identically; oracle skew against `TypeAdapter(Payload).json_schema()` blocks merge at the emission layer; parallel prose field lists and hand-maintained JSON schemas are merge blockers.
- Triage tie-break anchors at promotion gate side — pre-gate payload law, post-gate owner law, adapter-only correspondence; assurance certify rows run only after promotion unit commit and never substitute gate admission or re-admit payload-shaped interior parameters.

# Contract Surface Law

- Python `>=3.15` typed payloads occupy pre-materialization lattice slots only — never canonical, variant-family, or wire-struct layers.
- Payload law belongs in shape declarations; prose, comments, runtime `dict` repair, and boundary validators must not substitute for erased or implicit payload structure.
- A typed payload is a static dictionary contract: declared keys, per-key requiredness, per-key mutability evidence, and an explicit openness posture — not a domain owner, validation engine, or serialization policy.
- One payload name owns one contract; parallel mirror shapes for requiredness, openness, or read-only posture split one concept and are rejected.
- `TypedDict` at runtime is a plain `dict`; static contracts are checker-enforced. Runtime immutability, ingress validation, and wire codec policy belong to materialized owners and boundary adapters.
- Stdlib `typing` ships `closed=`, `extra_items=`, `Required`, `NotRequired`, and `ReadOnly` on `TypedDict`; author payload law from the typing specification — checker gaps are proof debt, not license to omit declarations.

# Openness, Extension, And Inheritance

- Payload exactness is a deliberate choice among three postures: `closed=True`, `extra_items=T`, or default open — never an accident of default openness.
- `closed=True` forbids keys beyond the declared set; use for wire envelopes, event bodies, provider response slots, and any contract where unknown keys are a defect.
- `extra_items=T` permits a typed extension band beyond a known core — metadata bags, provider overlays, plugin annotations, keyword tails not fixed at declaration time.
- Default open: structural assignability permits extra keys typed as `ReadOnly[object]`; construction still rejects undeclared keys on literals and constructors.
- `closed=True` and `extra_items=` are mutually exclusive; combining them is a runtime error; prefer `closed=True` over `extra_items=Never`.
- `closed` and `extra_items` inherit through subclassing without widening; a subclass cannot widen a closed parent, cannot pass `closed=False` under a parent with `closed=True` or `extra_items`, and cannot add keys to a `closed=True` parent.
- Narrowing `extra_items` is permitted only when the parent declares `extra_items=ReadOnly[T]`; widening `T`, removing `closed=True`, or passing `closed=False` under a closed or `extra_items` parent is rejected.
- A closed extension slot is `extra_items=T` with an intentional narrow value type — not default open behavior and not `dict[str, Any]`; extension slots must be typed narrowly: `str`, `Atom`, `JsonValue`, `ReadOnly[Metadata]`, or another bounded owner.
- Subclass keys under `extra_items` respect inheritance law: non-read-only `extra_items` forces subclass additions to be non-required; read-only `extra_items` allows required subclass keys assignable to `T`.
- Adding keys to a parent with `closed=True` is a static error — use sibling closed payloads per variant or discriminant-selected body payloads.
- Multiple inheritance merges `__required_keys__` and `__optional_keys__` as set unions with per-key conflict resolution: a key required in any base remains required unless explicitly marked `NotRequired` in the merging child and inheritance law permits the override.
- Mixing `total=False` defaults with `Required` on individual keys in a subclass is the preferred way to stage optional overlays on a `total=True` base without splitting mirror types.
- Pattern consumption uses `match` with `**extensions` capture only after known-key shape and `extra_items` type are declared; fold extensions into tuple or frozen map owners at promotion.

# Requiredness, Read-Only, And Absence

- Per-key requiredness uses `Required[T]` and `NotRequired[T]` in one owner instead of split total/non-total sibling types; `total=False` makes declared keys optional by default; `total=True` makes them required by default.
- `__required_keys__` and `__optional_keys__` are introspection truth; `__total__` alone is insufficient when `Required`/`NotRequired` or mixed inheritance is present.
- `extra_items` keys are always non-required; `Required` and `NotRequired` must not annotate `extra_items`; deleting an `extra_items` key is permitted; deleting a required declared key is not.
- Requiredness answers whether a key must be present; it does not answer whether the value may be updated — that is `ReadOnly` evidence.
- `ReadOnly[T]` marks static non-mutability on ingress evidence consumers must not mutate — identifiers, discriminants, envelope fields, snapshot inputs, callback parameters typed as read-only payloads.
- Mutable-item payloads are assignable to read-only-item payloads; the reverse is rejected.
- Frozen domain owners own durable immutability after promotion; do not claim immutability in comments when only `ReadOnly` is declared.
- `extra_items=ReadOnly[T]` enables lawful narrowing in subclasses when extension metadata is evidence, not a mutable scratch map.
- `NotRequired[T]` types optional key presence; `T | None` types optional null values — do not conflate; do not use `NotRequired[T | None]` when only absence or only null is intended.
- `None` as a value and key absence are different contracts; optional value types use `T | None` on the key; optional presence uses `NotRequired[T]`.
- The same field name may pair payload `NotRequired` with ingress-model `Field(default=MISSING)` only when the adapter documents the semantic fork: wire-optional versus parameter-omitted.
- Sentinel-parameter defaults on keyword callables pair with `*` boundaries; `Unpack` payload contracts document which keys are `NotRequired` — keep sentinel and payload omission lists synchronized in the adapter module.
- Vocabulary absence for domain optional slots belongs on materialized owners after promotion unless wire contract is source of truth.

# Construction Versus Assignability

- Assignability answers whether an existing mapping may be treated as compatible after it exists; construction answers whether a literal or constructor call may introduce keys at creation time.
- Open and default-open payloads permit extra keys on assignability — unknown keys type as `ReadOnly[object]`; construction still rejects undeclared keys on literals and constructors.
- `closed=True` removes extras from both postures; a value typed as a closed payload cannot be proven to carry keys outside the declared set at any use site.
- `extra_items=T` types extension keys on assignability while keeping them non-required; extension keys appear only on values that already satisfy assignability or arrive from untrusted ingress decoded upstream.
- Adapters decode untrusted material to `dict[str, object]`, validate into boundary payloads via `TypeAdapter`, then treat the product as assignable evidence — not as proof that arbitrary construction sites may omit openness declarations.
- Law must distinguish may-exist-after-assignability from may-be-passed-at-construction.

# Closed-Key Narrowing

- `key in payload` narrowing on closed payloads composes left-to-right; test discriminant keys before body keys.
- `in` narrowing is static evidence only — boundary validation must still gate untrusted ingress; negative `not in` tests do not prove absence for required keys on closed payloads.
- Union narrowing uses `match payload:` with one arm per closed variant or per `(kind, body)` pair; use `in` guards inside arms when optional declared keys partition behavior without warranting separate payload types.

# Keyword-Callable And Decorator Seams

- Implementation signatures use `def op(**kwargs: Unpack[Payload]) -> R`; keyword-callable hooks use `Callable[[Unpack[Payload]], R]` — not homogeneous `**kwargs`, untyped `**kwargs`, or one-method `Protocol` shells that hide parameter names.
- Structural ports that are not independent multi-method surfaces stay on `Callable[[Unpack[Payload]], R]`.
- Callable and payload contracts compose with `ParamSpec`, `Concatenate`, `/` positional-only boundaries, and `@wraps` on decorators so aspect stacks preserve the payload shape end to end.
- The signature owns the call contract at the declaration site; erased `Callable[..., R]`, `*args` parsing for positional keyword bundles, and wrapper signatures that drop `Unpack` are rejected.
- Decorators preserve shape with PEP 695 `**P` and `@wraps`: inner `(*args: P.args, **kwargs: P.kwargs)` forwards without erasing `Unpack`; `Concatenate[Context, P]` prepends bound context without redeclaring payload keys.
- Decorators must not replace static contracts with runtime `dict` repair, `get` defaults for missing keys, or `extra="allow"` on closed surfaces.
- Decorator altitude and phase ordering belong on decorator admission surfaces — not payload field declarations.
- `Unpack[Payload, extra_items=T]` at a callable boundary is equivalent to required known keywords plus `**tail: T`; unknown keywords at call sites are checker errors only when the payload is closed; otherwise they must satisfy `extra_items`.
- Keyword-tail call sites collect `**tail` into a frozen snapshot at the adapter exit when the tail must survive past the call boundary.

# Boundary Payloads And Anti-Corruption

- Boundary payloads type wire, API, CLI, config, and provider surfaces before domain materialization — static compatibility shapes, not validated domain objects.
- Ingress decodes untrusted mapping material into a boundary payload, then promotes to a domain owner; egress projects owners into boundary payloads, then encodes; the payload type names the static field set; the codec owns bytes, text, and schema version.
- Closed boundary envelopes use `closed=True` when the wire format forbids undeclared keys; open provider surfaces use `extra_items` when the provider may return a typed extension band beyond the stable core.
- Reject `Mapping[str, object]`, `dict[str, Any]`, and untyped `**data` when a `TypedDict` can name the shape; runtime validation may gate untrusted input but must not replace a missing static payload declaration.
- Provider and wire field renaming stays once in the boundary adapter as typed correspondence rows — not scattered `model_dump` key pops or inline literals duplicated across ingress and egress.
- Anti-corruption adapters decode foreign layout into boundary payloads or ingress models, then promote to canonical owners; canonical owners never absorb foreign field names, cardinality, or discriminant encoding.
- When ingress field count differs from canonical field count, the adapter documents each omission as wire-optional, domain-default, or computed-on-materialize; payload `NotRequired` keys mirror wire optionality; canonical defaults apply at promotion, not by mutating the payload dict.
- Provider response slots mixing a stable core with a typed overlay declare `extra_items` on the boundary payload and map overlay keys once before variant promotion.

# Staging, Patch, And Replacement Handoffs

- Partial construction payloads stage data before a domain owner is complete — `total=False` with `Required` only on keys required for the current stage.
- One staged payload per construction phase, not parallel full and partial types for the same concept; staging payloads remain read-only evidence where later steps must not rewrite earlier evidence.
- Smart constructors accept partial construction payloads and return `Result[Owner, E]` or `Option[Owner]`; failed or abandoned staging payloads discard at the seam — they do not persist in registries, caches, or context variables.
- Patch payloads express intentional partial updates: updatable fields are `NotRequired`; identity and version fields that must not change are `ReadOnly` and block update slots at the static seam — omitted `NotRequired` keys mean leave unchanged.
- Patch payloads hand off to replacement expressions — `model_copy(update=...)`, `copy.replace(...)`, or `model_validate(snapshot | delta)` — not in-place dict mutation; closed patch payloads pair with closed replacement folds.
- Patch payloads never apply themselves; replacement owners must not reinterpret read-only patch keys as mutable defaults.
- Patch payloads are closed unless typed extension metadata is explicitly admitted via `extra_items`; do not reuse full creation payloads as patch shapes.
- Adapters routing both route on payload type or explicit stage discriminant, never on keys-present heuristics; creation and patch payloads are different seam artifacts even when key names overlap.
- Nested partial updates assemble nested validate inputs explicitly until polymorphic nested replace admits one-expression validated replacement; shallow `model_copy(update={"nested": {...}})` without typed nested validation is rejected when nested slots are materialized models.
- Patch payloads name `ReadOnly` on identity and version keys — replace proof asserts omitted `NotRequired` keys leave nested subgraphs untouched and read-only keys reject update attempts at validation exit.
- Extension metadata on updates uses `extra_items` on the patch payload only when the replacement owner admits typed extension bands.

# Events, Discriminants, And Variant Envelopes

- Event and message payloads are closed or `extra_items`-bounded with a discriminant key typed as `Literal` or closed vocabulary enum — not tag strings checked at runtime only.
- Envelope-plus-body uses two owners: a closed envelope with read-only identity fields and a body payload selected by `kind` through `match` exhaustiveness with `assert_never`.
- Semi-closed and closed variant families pair a discriminant envelope payload with a body payload selected by `kind` or tag literal; the envelope carries read-only identity and occurrence fields; the body carries variant-specific required keys.
- When subclassing would violate `closed=True`, use sibling closed body payloads per variant and a closed union at the match gate — match envelope first, bind body type from discriminant, then match body keys; never mutate the envelope dict to attach body fields.
- `match payload:` on the discriminant binds the body payload type or promoting constructor per arm; `assert_never` witnesses arms the checker cannot close when the vocabulary row is finite.
- Class-family variant dispatch runs on materialized tagged unions; payload seams supply static discriminant evidence and promotion constructs the tagged owner before family dispatch runs.
- Open extension arms in variant families use `extra_items=T` on ingress or staging payload, then fold captured extensions into `frozendict` or tuple snapshots at promotion; extension dicts do not flow through family dispatch as scratch maps.
- Vocabulary modules export token rows; payloads import literals or enum members; bare wire enums and payload enums stay separate shapes — discriminant keys carry bare tokens; rich member metadata lives on payload enums or canonical owners.
- `TypeIs` narrowing on materialized owners complements payload discriminant matching after promotion with a single vocabulary source of truth.
- Illegal FSM transitions at ingress emit typed fault owners with enum-member labels; vocabulary modules own the legal-event set; payload seams carry evidence fields only.
- Event consumption treats the payload as evidence with `ReadOnly` on fields handlers must not rewrite; promotion to domain facts happens once in the handler entry, not by mutating the event dict in place.
- `StrEnum` members carrying non-member payloads prove through explicit seam tests — static typing relies on typing-spec nonmember rules; runtime tests assert member construction preserves payload fields and rejects illegal member definitions.
- Proof table asserts parity among payload literal, enum member value, and wire string for every arm without reconstructing tokens from member names at runtime.
- Adapter modules document each member payload row in the contract table and test member construction explicitly — do not infer payload legality from enum membership alone.
- Fault payloads at illegal FSM transitions carry enum-member labels in typed fault owners — seam tests feed out-of-vocabulary discriminants and assert fault discriminant vocabulary, not bare exceptions.

# Promotion, Extension Capture, And Egress

- Promotion is the lawful transition from static payload compatibility to a materialized domain owner at the composition-root gate; payloads stop at the promotion gate; domain logic runs on owners.
- Promote when values cross from transport or staging into invariant-bearing domain space — after ingress decode, after event admission, after partial construction completes, or before dispatch on a closed variant family.
- Pattern matching with `**extensions` captures the extension band when `extra_items` names `T`; fold captured extensions into `frozendict[str, V]` or immutable tuple evidence once at the gate — not lazily inside transforms, through long pipelines, or as scratch maps in family dispatch.
- Tuple or `frozendict` snapshots are promotion products, not alternate permanent models; snapshot at promotion and thread through domain logic as part of the owner or as a dedicated immutable field.
- Do not promote prematurely inside boundary adapters that only forward opaque mapping material; do not defer promotion so deeply that domain code operates on raw `dict` with comment contracts.
- Closed payloads project to `Mapping[str, VT]` only when every declared value type is assignable to `VT` — an egress typing convenience, not ingress license.
- Wire egress projects from canonical through `msgspec.convert` or field-explicit struct construction; egress payloads, when used, are assignability targets for dict literals built in the adapter.
- Field renames for wire format happen in adapter projection, not via duplicate payload names; partial egress uses dedicated closed subsets with fewer keys when requiredness matches the wire contract.

# Materialization Gate And Pipeline

- Payloads are pre-materialization evidence; domain modules accept materialized owners past the promotion gate, not boundary payloads, validated dict views, or `Mapping[str, object]` carriers.
- Payloads do not appear as parameters to stage-skipping interior transforms.
- `TypeAdapter(Payload)` owns validation when the durable handoff remains a static payload through adapter exit — config snapshots, provider compatibility probes, staging tables, and wire previews.
- `validate_python` accepts mapping material after ingress decode; pair with `ConfigDict(extra="forbid")` on closed wire envelopes; `validate_json` owns the bytes path; do not `json.loads` then validate when the adapter can own the bytes path.
- Promotion to `BaseModel`, `Struct`, or a rich class owner happens in the next adapter expression, not inside domain folds; validation failures map to typed fault owners inside boundary `capture`.
- `bytes` and `str` ingress decode inside the adapter to `dict[str, object]`, validate, then promote — payloads name field compatibility after parse, not the wire carrier.
- Keyword ingress typed as `Unpack[Payload]` unpacks inside the adapter in the same expression that validates or promotes — do not thread `**kwargs` through interior modules.
- Pipeline order: wire decode → boundary validate into payload or ingress model → discriminant `match` → owner construction → domain fold; partial construction aligns with staging carriers; patch payloads align with replacement stage entry; extension bands fold at promotion into immutable owner evidence.
- When staging completes, the durable handoff is always a materialized owner, never a validated dict typed as assignable payload evidence.
- Extension bands from `extra_items` fold at the promotion gate into `frozendict` or tuple snapshots on the owner — downstream materialization stages receive immutable extension evidence, not mutable capture dicts.
- Untrusted ingress always passes `TypeAdapter` validation before payload assignability is assumed; trusted-replay paths pin adapter module identity, schema version, and store key beside the payload owner at the composition root — replay validates into the same boundary payload type as live ingress, then promotes through the identical gate expression.

# Projection Lattice And Stage Roles

- Boundary ingress — closed or `extra_items`-bounded payloads decode foreign mapping material after bytes/text parse; below Pydantic ingress models when the model wraps the payload; above raw `dict[str, object]` at the adapter exit.
- Staging partial — `total=False` with stage-scoped `Required` keys between ingress validation and `Result[Owner, E]` promotion; never persist in root registries after promotion completes or fails.
- Patch delta — `NotRequired`-only update contracts hand off to replacement expressions at the composition root; not interior parameters and not wire egress owners.
- Event evidence — closed envelope plus discriminant-selected body payloads supply static handler admission evidence; promotion to domain facts once at the root gate, not inside dispatch folds.
- Egress assignability — closed egress subsets or full boundary payloads type dict literals built by projection adapters; interior modules emit canonical or wire struct owners, not egress payload types.
- Keyword-callable surface — `Unpack[Payload]` on root adapter signatures only; interior modules receive materialized owners or frozen extension snapshots; keyword payload contracts do not thread past the first validate/promote expression.
- Canonical, wire egress, settings, CLI, persistence, and receipt slices use frozen models or structs — not parallel `TypedDict` definitions for the same concept.
- Lattice law: one payload owner module per concept stage per bounded context; no projection imports another projection as its definition source; boundary payloads import from the payload owner module; materialized projections import canonical or wire owners — never each other's field lists.
- Ingress models and boundary payloads may diverge on aliases and `NotRequired` keys; they must not diverge on discriminant vocabulary without adapter-owned mapping tables; vocabulary tokens export once from the canonical or vocabulary owner.
- Settings and CLI slices do not redeclare payload key sets — cyclopts `Parameter` and `pydantic-settings` `Field(validation_alias=...)` project from canonical or settings models; static payload contracts type adapter kwargs at the CLI boundary only.
- Dual-surface owners without decorator-admitted composition use a dedicated staging payload per surface until a single owner can project both without lattice collapse; proof asserts CLI invocations materialize the same canonical owner as HTTP ingress for equivalent field sets.
- Settings `validation_alias` tables map once in the adapter; proof samples include alias-key ingress dicts validated into boundary payloads before promotion.
- Sentinel-parameter defaults on CLI entrypoints pair with `*` boundaries — proof verifies positional callers cannot skip into sentinel arms and that `NotRequired` payload keys align with cyclopts `Parameter` omission semantics documented in the adapter module.

# Programmatic Reading And TypeAdapter

- Read payload law through `annotationlib.get_annotations(owner, format=annotationlib.Format.FORWARDREF)` — not raw `__annotations__`; forward references and deferred annotation thunks resolve through the annotationlib entry point.
- Fold requiredness from `__required_keys__` and `__optional_keys__`; read openness from `__closed__` and `extra_items`; missing `__closed__` is default-open assignability posture at introspection, not permission to skip declaration at authoring sites.
- Strip `ReadOnly` wrappers for codec dispatch; preserve them in static signature emission.
- Contract readers emit one boundary table per payload owner — required keys, value types, read-only flags, openness posture, and `extra_items` type — at build or test collection time; adapters consume tables, domain modules import payload types directly.
- Codegen and fixture generators consume emitted tables — OpenAPI fragments, test factories, and provider compatibility matrices — without maintaining parallel field lists in prose or JSON Schema documents.
- Type-only payload modules are valid integration sources: readers, signatures, and codegen import the contract without runtime payload classes.
- `json_schema()` exports the contract for OpenAPI and fixtures — reader-emitted tables and adapter JSON Schema must agree; divergence is a duplicate-contract defect.
- Adapter-derived JSON Schema is sole wire-schema authority for payload names; parallel OpenAPI field lists and JSON contract tables are merge blockers.
- Generic payload owners with `T` in body slots and `extra_items=T` require paired static proof: annotationlib resolves `T` and `ReadOnly` wrappers; `TypeAdapter` with concrete specializations validates runtime behavior per argument.
- When Pydantic ingress models embed generic payload fields, `model_rebuild()` runs at root first-touch before contract reader emission; reader output and `TypeAdapter.json_schema()` must agree on specialized field sets for each concrete argument row.
- Reader emission tests compare frozenset outputs to `TypeAdapter` specialized schemas per concrete argument; `model_rebuild` ordering is documented at root first-touch.
- Cross-checker matrix modules pin `from __future__ import annotations` posture explicitly; reader emission tests compare frozenset outputs across backends for the same owner until parity is proven.
- Do not maintain parallel non-generic payload siblings to dodge forward-ref proof — specialize at adapter and reader sites; introduce sibling closed payloads only when required-key sets are incompatible.

# Generic Payloads

- Generic payloads declare type parameters on the `TypedDict` when extension value types or body slots vary: `class SlotPayload[T](TypedDict): ...` with bounded `extra_items=T`.
- Bound type parameters on extension bands prevent unbounded `object` erosion in generic clients.
- Default type parameters stabilize call sites with fixed extension atoms: `class Envelope[T: JsonValue = JsonValue](TypedDict, closed=True): ...`.
- Generic payloads remain static contracts; runtime validation uses `TypeAdapter` with concrete or bounded-union arguments.
- Separate closed payloads when variants carry incompatible required-key sets that generic optional fields would erase; do not introduce parallel non-generic payloads when a single generic owner can express the family.

# Composition Root Wiring

- The composition root owns every boundary payload import, `TypeAdapter` instantiation, promotion expression, and egress projection for a bounded context; interior modules import canonical owners and vocabulary only.
- Interior modules do not construct adapters or payload tables at call sites.
- Root assembly binds ingress adapters to promotion gates in one expression per entry route: decode → validate → `match` discriminant → construct owner → hand off to domain fold.
- Staging payloads never persist in root registries after promotion succeeds or fails.
- Keyword entry routes declare `Unpack[Payload]` on root signatures only — inner modules receive materialized owners or frozen extension snapshots, not forwarded `**kwargs`.
- Root-level `ParamSpec` stacks on decorators applied at the integration edge preserve `Unpack` through the full callable graph before domain handlers bind; decorator proof runs against root-wrapped callables, not raw inner targets.
- Multiple ingress routes for one concept share one payload owner module and one vocabulary export — root wiring differs by carrier; payload law does not fork per route.

# Proof, Checker Matrix, And Runtime Gates

- Proof consumes reader-emitted contract table rows as the single field-authority surface — adapters, factories, OpenAPI fragments, and hypothesis strategies read the same tables; prose field lists and parallel JSON schemas are rejected.
- Payload owners participate in the integration proof harness as an orthogonal static layer — type-only payload modules run in the cross-checker matrix even when runtime tests target promoted owners only.
- Each closed payload owner maintains a parametrized contract table row: declared keys, requiredness frozensets, `ReadOnly` flags, `__closed__`, and `extra_items` type when present.
- Contract tables link to metamorphic chains at the composition root: lawful payload dict → validate → promote → project egress → validate assignability — failures implicate adapter binding before domain owners.
- Per-payload wire samples live beside the owner module: lawful closed literals, lawful `extra_items` extension bands, and drift samples with forbidden keys or wrong discriminant literals — `TypeAdapter.validate_python` and `validate_json` assert accept/reject parity with the table.
- Static proof runs pyright, mypy, and ty on payload-law modules in a fixed cross-checker matrix covering `closed=True`, `extra_items`, mixed `Required`/`NotRequired`, generic `TypedDict[T]`, and `ReadOnly` variance.
- Cross-checker matrix is a merge gate for stack integration — bounded contexts cannot ship payload law on a single backend green.
- One passing backend does not relax declarations required by others — suppressions, `cast`, and `# type: ignore` at payload sites fail lint policy before checker matrix execution.
- Open proof debt does not waive spec-complete declarations or proof obligations while tracking remains open.
- Inheritance edges that one backend tracks as open — subclass under `extra_items=ReadOnly[T]`, narrowing `extra_items`, closed parent sibling variants — each edge owns a minimal repro module in the matrix until tracking closes.
- Assignability versus construction splits get distinct matrix modules: open assignability with construction rejection, closed assignability, and `extra_items` extension assignability; checker errors at the wrong site indicate mis-tagged proof modules.
- Callable seam modules exercise `Callable[[Unpack[Payload]], R]`, `Concatenate[Context, P]`, and decorator-wrapped forwards in one file per payload family.
- Static checks include decorator-wrapped callables, generic payload specializations at adapter sites, and `Concatenate` stacks — one representative module per payload family exercises the full seam signature graph.
- Discriminant exhaustiveness proof pairs static checker witnesses with runtime promotion tests: every `Literal` or enum arm in the vocabulary row constructs the expected owner kind through the root gate expression; missing arms fail CI before behavioral suites run.
- `TypeAdapter(Payload).json_schema()` output is snapshotted or diff-tested against reader-emitted OpenAPI fragments — divergence between adapter schema, reader tables, and hand-maintained fixtures is a merge blocker.
- Callable payload seams prove signature preservation: collected root handlers and decorated hooks expose `Unpack[Payload]` in `inspect.signature` after `inspect.unwrap` exhaustion; ParamSpec erasure fails the contract suite before integration tests execute.
- `assert_never` witness lines in promotion `match` blocks stay excluded from branch-coverage targets — exhaustiveness is proven by type checkers and discriminant table parity.
- Runtime proof at the composition root validates adapter accept/reject parity, promotion `match` exhaustiveness, extension-band frozen snapshots, and owner round-trip when polymorphic interiors cross boundaries — payload validation alone does not substitute for promoted-owner proof.
- Root round-trip proof encodes promoted owners and decodes through boundary adapters when polymorphic interiors cross process boundaries.
- Promotion tests use explicit payload literals or adapter-validated dicts — not `isinstance(x, Payload)`; construction rejection tests prove call sites cannot inject extra keys into closed literals.
- Extension capture tests assert promotion folds `**extensions` into `frozendict` or tuple snapshots on the owner — mutable extension dicts must not survive into domain transform parameters.
- Egress proof builds dict literals from canonical projection, asserts assignability to egress boundary payloads, then validates through `TypeAdapter` when wire compliance is closed.
- Arch import rules extend to payload boundaries: domain modules must not import boundary payload types; composition roots own adapter and payload owner imports; codegen consumes reader tables without duplicating field lists in test modules.
- Proof harness layer ordering places static checker matrix before contract tables before hypothesis properties before root round-trip — payload-law failures at static layer block expensive generative runs.
- Failures at callable seams — `Unpack` erasure, closed-key assignability, `ReadOnly` variance — block merge before runtime promotion tests run.
- beartype-decorated root entrypoints taking materialized owners receive mutation probes on live instances — runtime admission catches post-promotion field tampering that static payload proof cannot see; payload law and runtime admission are complementary, not substitutable.
- Mutation and regression gates integrate with stack evolution triggers — adding a payload key without updating table, schema snapshot, and hypothesis registry blocks merge at the contract layer before integration smoke.

# Property Testing And Mutation Gates

- Hypothesis strategies for closed payloads draw from contract table rows — `st.builds` or `st.fixed_dictionaries` keyed by `__required_keys__` and optional keys from `__optional_keys__`; drift strategies add forbidden keys for negative validation tests only.
- Extension-band strategies respect `extra_items` bounds — `st.dictionaries` with keys disjoint from declared sets and values drawn from `T` or `ReadOnly[T]` exemplars; captured `**extensions` folds prove frozen snapshot shapes at promotion.
- Discriminant-conditioned strategies mirror `match` gate structure — outer kind sampled first, body payload fields conditioned on kind literal; independent random tags filtered by runtime validation are rejected as strategy design.
- Keyword-callable property tests preserve `Unpack` through generated call patterns; strategies invoke root-wrapped handlers with lawful keyword sets only; missing required keys are negative cases, not shrink targets.
- Shrinking rebuilds lawful payload dicts after shrink steps — invalid extra keys on closed payloads and illegal read-only mutations are rejected at construction gate, not used as shrink endpoints.
- Stryker or equivalent mutation on promotion `match` arms requires kill ratio on discriminant routing — mutants that default to catch-all or drop arms must fail exhaustiveness type-check or contract tests, not pass with silent wrong-owner construction.
- Adding a payload key, discriminant arm, or openness flag without updating contract table, adapter field maps, json_schema snapshot, and hypothesis registry is a merge blocker — static proof and runtime suite encode the same closed membership as source.
- Mutations introducing `cast`, `# type: ignore`, or ParamSpec erasure on root callables fail lint before mutation scoring — payload law regressions surface in the checker matrix first.

# Collapse Tests

- Domain payload import — interior module imports boundary `TypedDict` owner; collapse to canonical owner plus root adapter promotion expression.
- Dual schema — OpenAPI or fixture dict maintained beside `TypeAdapter.json_schema()`; collapse to adapter-derived schema and reader-emitted table.
- Parallel create and patch names — overlapping key sets with different requiredness declared as duplicate concepts; collapse to distinct stage-specific payload names tied to one vocabulary owner.
- kwargs threading — `**kwargs` forwarded through three modules after root ingress; collapse to single root validate/promote expression and owner-only interior APIs.
- Wire dict domain — domain fold accepts `dict[str, Any]` with comment contract; collapse to promotion gate and frozen owner parameters.
- Default-open wire — boundary envelope omits `closed=True` where wire forbids extras; collapse to explicit `closed=True` or bounded `extra_items` plus drift negative samples.
- Payload as lattice owner — boundary `TypedDict` imported where canonical or wire struct should occupy the stack layer; collapse to role map reassignment and owner promotion at the correct stage slot.
- Cross-context payload fork — two bounded contexts declare identical closed payloads without shared vocabulary module import; collapse to one boundary owner module and cross-context adapter import.
- Lattice projection source inversion — egress payload or ingress model imports field lists from a sibling projection; collapse to canonical or payload owner module as sole definition source.
- Stage-skipping payload carrier — interior transform accepts validated payload assignability after staging should have promoted; collapse to materialization pipeline stage map and owner-only interior API.
- Stack graduation residue — concept graduated to variant family or wire struct while interior modules retain payload imports for the same field set; collapse graduation and retire payload owner in one promotion unit.
- Harness layer inversion — integration tests run hypothesis payload properties before static checker matrix; collapse to proof harness layer ordering.
- Done when stack role map, materialization alignment, and root collapse tests pass jointly — every bounded context has one payload owner module per concept stage, one root wiring graph per ingress route, contract tables drive adapters and tests without parallel field prose, no bounded context treats payloads as durable lattice owners above the promotion gate, and checker matrix plus runtime suite pass without suppressions.

# Evolution And Diagnostics

- Payload key additions, discriminant arm extensions, and openness flag changes are version events when wire-visible — update boundary payload owner, adapter field table, `TypeAdapter.json_schema()` snapshot, contract reader row, hypothesis registry entry, and drift negative samples in one promotion unit.
- Closed key removal or requiredness tightening requires simultaneous egress projection updates and migration fold rows when stored bytes carry the retired key; partial payload promotion is a merge blocker.
- `extra_items` type narrowing is permitted under inheritance law; widening `T`, removing `closed=True`, or splitting one payload into parallel mirror types for requiredness is rejected — evolution uses sibling closed variants or stage-specific payload names tied to one vocabulary owner.
- Generic payload specialization changes require paired updates to reader frozenset emission and `TypeAdapter` concrete argument rows; generic forward-ref proof modules in the checker matrix must stay green across backends.
- Deprecation retires payload arms only after migration proves zero ingress dependency — retired discriminants remain in migration modules with `assert_never` witnesses, not in active promotion `match` blocks.
- Settings and CLI staging payloads evolve independently of wire boundary payloads unless the composition root explicitly couples surfaces; dual-surface changes bind canonical equality proof across CLI and HTTP paths in the same unit.
- Vocabulary changes on payload discriminant keys propagate in one unit: payload `Literal` or enum import, envelope `match` arms, promotion owner constructors, OpenAPI enum fragments from adapter schema, and contract table discriminant row — orphan arms fail CI before behavioral suites.
- Provider remap table changes bind ingress adapter, boundary payload canonical key set, and anti-corruption field correspondence — external key renames do not reach interior modules without adapter-owned mapping.
- Callable payload seam changes bind root handler signatures, decorator-wrapped `inspect.signature` proof, and ParamSpec preservation tests — signature erasure is a contract defect, not a runtime workaround.
- Payload validation failures at the boundary map to typed fault owners with adapter-owned step names — domain modules do not format wire-key error strings from raw `ValidationError` dumps.
- Assignability failures after closed adapter decode indicate wire drift or missing `closed=True` on the boundary declaration — diagnostics name the payload owner and forbidden key, not a cue to widen the contract.
- Construction failures on typed literals indicate call sites building dicts by hand — route through validation or typed constructor paths; do not relax openness to absorb the defect.
- ParamSpec erasure after decorator application names the wrapper site in static proof output — fix the decorator stack before widening payload requiredness.
- Parallel JSON Schema diverging from `TypeAdapter.json_schema()` or reader-emitted tables indicates duplicate contract surfaces.
- Domain modules importing boundary payload types indicate a missed promotion gate.
- `model_dump` dict surgery at interior sites indicates projection collapse — route through egress adapter and closed egress payload or struct projection.
- Patch payloads reaching domain folds without a replacement owner target indicate a seam skip — patch application belongs on the replacement expression, not inside transforms.
- Discriminant exhaustiveness gaps are binding defects surfaced in checker matrix and contract table parity before runtime promotion tests — missing `match` arms are not integration test gaps.

# Rejection Catalog

- `dict[str, Any]`, homogeneous or untyped `**kwargs`, split total/non-total mirror inheritance, and comments claiming read-only or closed behavior without declarations.
- Runtime `TypedDict` enforcement, `isinstance(x, Payload)`, `TypedDict` subclassing at runtime for behavior, `Mapping[str, object]` bags where payloads can name the shape, and callback `Protocol` shells erasing keyword names.
- Parallel payload types for create, patch, and wire when stage-specific names tied to one concept suffice; default-open posture when exactness or typed extension is required.
- Interior modules constructing `TypeAdapter(Payload)` for convenience — boundary leak bypassing root wiring and proof harness attribution.
- `**kwargs` threading past root ingress, dual JSON Schema beside `TypeAdapter.json_schema()` or reader-emitted tables, and domain modules importing boundary payload types.
- `model_dump` dict surgery routed to interior `TypedDict` assignability; patch payloads applied inside domain folds without replacement expression; mutable staging dicts retained after failed promotion or in context variables.
- `st.from_type(Payload)` or table-less Hypothesis builders on closed payloads; in-place mutation of admitted event or staging dicts after promotion admission.
- Checker-matrix green on one backend used to omit declarations required by others — checker gaps are not merge criteria for omitting declarations.
- Integration tests importing `TypeAdapter` in domain modules for convenience; contract tables maintained in JSON while Python payload owners drift — dual sources; collapse to reader-emitted tables from live annotations.
- Bounded contexts without a payload stage role map per concept — boundary, staging, patch, and egress slots undocumented at the composition root.
- Trusted-replay paths validating into a different boundary payload type than live ingress for the same concept.
- Stack graduation that promotes a concept to canonical or wire struct while retaining parallel payload owners imported by interior modules for the same field set.
- Cross-context payload owner duplication — identical closed payloads declared in two bounded contexts without shared boundary vocabulary module import.
- Evolution changes that update payload keys without simultaneous adapter table, schema snapshot, and hypothesis registry rows.
- Skipping extension-band fold proof because payloads assign cleanly; shallow nested `model_copy(update=...)` without typed nested validation on materialized nested slots.
