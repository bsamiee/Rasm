# Variant Family Architecture

# Critical Signals

- `@disjoint_base` (PEP 800) + PEP 742 `TypeIs` + `match`/`assert_never` form the proof triangle per closed union layer — hierarchical families require independent witnesses at each routing level; nested pattern guards do not discharge inner exhaustiveness.
- Product-of-sum depth budget is three discriminator routing levels inside one owner; beyond level three extract inner closed union into named sub-owner field with independent vocabulary table, registry parity, and migration axis — flat `"outer.inner"` tags couple evolution axes unless wire contract mandates single flat vocabulary.
- Cross-sub-owner invariants prove once at root smart constructor `admit` before registry lookup; multi-stage registry lookup mirrors nested `match` (outer narrow → sub-field → inner narrow); each sub-owner table parity proves independently — top-level parity does not subsume inner tables.
- Worker and subinterpreter handoffs re-materialize hierarchical `Member` through root-pinned decoder bytes — import-time frozen registries and cached nested `TypeAdapter` pairs do not cross process or interpreter seams without vocabulary table hash parity at receiving root boot.
- Sparse `TRANSITION_ROWS` materialize at root import into read-only `TRANSITION_GRAPH`, transitive `REACHABLE`, and shortest-hop `LAWFUL_CHAINS` — absent `(from_kind, to_kind)` row means forbidden hop, not implicit identity; per-request path search without cached canonical chains is merge blocker.
- `TypeAdapter.validate_python(member.model_dump())` on live `Member` to change discriminant kind — collapse to endofold transition row or documented re-ingress row, not ingress replay disguised as in-process transition.
- Migration fold invoked mid-domain on materialized current-vocabulary `Member` — collapse to read-boundary migration module; endofold tables operate only after migration completes.
- Vocabulary table row count must equal closed union width — registry parity tests are import-time defects; orphan rows and missing arms block merge before behavioral suites run.

# Family Ownership And Closure

- One closed domain owns exactly one namespace object — outer class, `@tagged_union` owner, or single module-local family alias — never scatter variants as unrelated module-level siblings.
- Export one union type alias (`type Member = A | B | C`) beside the owner; all consumers narrow against that alias, not repeated inline unions.
- Closed membership is fixed at declaration; adding a case is a type error until every projection, adapter, morphism, migration fold, and materialization arm updates.
- Semi-closed families declare a closed core plus typed extension surface — `extra_items=` on ingress `TypedDict`, `NotRequired`/`ReadOnly` slots, or dedicated `Extension` variant — never open `dict[str, object]` payload pretending to be semi-closed.
- Reject parallel type families for the same concept; ≥3 sibling shapes, factories, or dispatch arms triggers collapse into one polymorphic owner before helpers, registries, or services.

# Disjoint Seals Nested Namespaces And Tagged Generics

- Mark the internal seal with `@disjoint_base` (PEP 800; `typing.disjoint_base` on Python ≥3.15) on nested `_Seal`; every variant inherits only from that seal, not from each other.
- Two variants with distinct disjoint seals cannot share a common runtime subclass — checker errors on disjoint-base violations are hard failures, not suppressions.
- Pair `@disjoint_base` with `@final` on leaf variants when fully closed; omit `@final` only when downstream specialization is real and checker-proven.
- Prefer `isinstance(value, Owner.VariantA)` plus `TypeIs[Owner.VariantA]` over `Protocol`/`runtime_checkable` shells invented solely to satisfy narrowing.
- Nest variants under owning outer class or `@tagged_union` nested dataclasses; hoist shared frozen fields, `model_config`, and invariants onto one internal base inside the owner.
- Keep variant declarations adjacent and ordered by semantic rank (discriminant value, lifecycle, severity) when order is load-bearing for generated tables or wire compatibility.
- Never flatten nested variants to module scope to reduce indentation.
- Parameterize one dataclass or frozen model on `Literal`-bounded type parameter (`class Shape[T: Tag]`); provide one `TypeIs` predicate when runtime refinement is required — predicate must be biconditional membership proof, not validity filter.
- Tagged generics replace N nearly identical classes differing only by phantom tag; do not mix tagged generics with unrelated nominal subclasses for the same discriminant axis.

# Literal Discriminants And Enum Vocabularies

- Every closed union member carries shared discriminant field typed as single-value `Literal` or bounded literal union on the same field name; pattern match on typed patterns, not pre-extracted string variables.
- Discriminant field names are uniform across the family unless callable `Discriminator` plus `Tag()` is explicitly justified for heterogeneous wire shapes.
- Multiple literal values on one variant (`pet_type: Literal["reptile", "lizard"]`) are allowed when wire vocabulary intentionally aliases; keep alias sets explicit in `Literal`.
- Use `StrEnum` when vocabulary requires iteration, registry keys, subset algebra, runtime identity, or method dispatch; use `Literal` on variant rows when no enum machinery is needed.
- Declare enum invariants with `@enum.verify(EnumCheck.CONTINUOUS | EnumCheck.NAMED_FLAGS | ...)` at class body — variant arms reference verified enums, not raw ints or strings reconstructed in validators.
- Collapse isomorphic variants differing only by enum tag into one frozen row carrying `reason: Reason` or `kind: Kind`.
- Never persist or route on raw `.value` strings outside boundary adapters — pass enum members through domain code; serialize at the edge.

# Stack Admission Surfaces

- Pydantic (`pydantic>=2.13.3`): frozen `BaseModel`, `strict=True`, explicit `Discriminator` on every domain family; `TypeAdapter(FamilyUnion)` owns boundary validation.
- Prefer field `Discriminator("method")` when members share uniform discriminant field with single-value `Literal` types; reserve `Tag()` plus callable for computed or heterogeneous discriminants.
- Nest discriminated unions via nested `Annotated` layers when wire schema is hierarchical; hoist shared ingress config onto single-inheritance base — avoid diamond `model_config` merges.
- Never rely on default `union_mode="smart"` or `left_to_right` for domain families — always attach explicit discriminator.
- Pydantic v2.13+ allows `RootModel` with `Literal` root in unions for scalar-tagged members; `model_validator(mode="before")` is wire normalization only — discriminant must survive unchanged.
- msgspec (`msgspec>=0.21.1`): `Struct(frozen=True, forbid_unknown_fields=True)` for wire egress; shared `tag_field` with unique tags per variant; tag values unique within union; prefer explicit string or int tags over class-name tags when JSON contract is stable across refactors.
- Wire structs are projection targets from canonical domain owner, not parallel domain trees.
- expression (`expression>=5.6.0`): `@tagged_union(frozen=True)` for in-process closed variants and file-internal error hierarchies — never export `@tagged_union` error variants across module boundaries.
- When `@property` discriminant exists on cases, it IS the routing predicate — reuse in folds and gates instead of re-switching on field tuples.
- Shared package errors use few exported frozen carriers with enum/`Literal` discriminants.

# TypeIs Exhaustive Dispatch And Fold Algebra

- PEP 742 `TypeIs[T]` is mandatory for reusable narrowing; `TypeGuard` is banned except true non-subtype narrowing — repo Ruff policy enforces complement-safe narrowing.
- Predicates must be true exactly for `T` and false for all other union members; author once at family owner; `TypeIs` plus `@disjoint_base` eliminates `cast()` after isinstance.
- Predicates consume narrowed values in expression pipelines — do not widen again into untyped locals after proof.
- `match/case` is sole variant dispatch mechanism in domain code; enable mypy `exhaustive-match` and ty all-error.
- End every closed `match` with `case _ as unreachable: assert_never(unreachable)` when checker requires witness — at every closed union layer in hierarchical families.
- `assert_never` accepts values typed `Never` — after full narrowing remaining binding must be unreachable.
- Pattern guards do not discharge exhaustiveness unless guard is proven mutually exclusive and complete — prefer direct variant patterns.
- Fold over `Member` is total function `Member -> Out` as one `match/case` with one arm per variant; partial folds keyed by string tags are switchboards, not algebra.
- `Block[Member].fold(initial, step)` aggregates collections when `step` is total per-member projection — step inherits exhaustiveness from single-member fold it delegates to.
- `list[Member]`, `tuple[Member, ...]`, and `Block[Member]` require element-wise narrowing — no `TypeIs[list[A]]` from `TypeIs[A]` on elements; map predicates across containers in one expression fold.
- `result.bind` and `option.bind` over variant transforms require each arm return same success type; heterogeneous per-arm success types force new discriminated result family instead of widening to `object`.
- Functorial `map` over `Member` is syntactic sugar for `match value: case A(a): return f(a)` with same exhaustiveness witness — do not implement `map` as `getattr` dispatch on `kind` field.
- Endofolds (`Member -> Member`) model domain transitions via `copy.replace` on materialized members — sparse `TransitionRow` registries keyed by `(from_kind: Kind, to_kind: Kind)` declare legal modality changes; absent entries return typed `InvariantFault`; transition parity tests cover every declared `(from_kind, to_kind)` pair with sample values — forbidden pairs assert fault constructors, not silent no-op or identity return unless identity is explicit row behavior.
- Version migration folds are not endofold transition rows — migration runs once at read boundary; endofolds operate on current `Member` alias only after migration completes.
- Reducers over variant streams (`Block[Member] -> Summary`) compose per-member fold with commutative accumulator only when accumulator semigroup is proven independent of variant order; otherwise fold left-to-right with ordered evidence.
- Nested `match` exhaustiveness is independent per level — pattern guards on hierarchical patterns do not discharge inner exhaustiveness; prove guard mutual exclusion separately or prefer unguarded variant patterns.
- Generic functions over `Member` must pattern-match inside body — abstract methods on non-variant base do not satisfy exhaustiveness.
- Fold helpers delegating to caller-supplied functions require `Callable[[Member], Out]` — wrapping in `object` erases proof obligation.
- Bounded `TypeVar` members (`T: Member`) appear in generic algorithms parametric over whole union, not single arm — constrain `T` to union alias, then `match` on `T` inside body.
- `Literal`-indexed generic owners propagate literal proof through homogeneous tag bags; heterogeneous collections need full `Member` union.

# Collapse Morphisms And Extension Boundaries

- When collapse triggers per family-ownership rules, one polymorphic entrypoint owns all modalities with total projections, not per-variant public functions.
- Three or more sibling dataclasses/models differing only by tag, optional field presence, or enum value collapse into one discriminated owner before registry, facade, or `dict[str, Handler]` switchboard.
- Wrapper models that rename or forward another variant without adding invariants are deleted; move boundary renaming to adapters.
- Duplicate factory functions collapse into one smart constructor returning `Result[Member, E]` with discriminant-driven construction inside.
- Closed families use `@tagged_union`, discriminated unions, nested disjoint variants, and `match` dispatch.
- Open extension uses `functools.singledispatch` only at boundaries for externally supplied types — never for in-repo closed vocabularies.
- Closed version sets use `Map[Ver, Callable]` or literal-version overloads when vocabulary is finite but implementations vary — not unbounded registries.
- Morphism between closed families `MemberA -> MemberB` is total adapter when both vocabularies are finite; partial morphisms return `Result[MemberB, E]` with explicit unmappable arms.
- Morphisms live at boundary adapters only — interior domain code does not translate between parallel variant families for the same concept.
- Discriminant vocabulary alignment is explicit at adapter (`Literal["card"]` → `Literal["card_payment"]`), not string convention in domain modules; inverse morphisms are separate total functions when round-trip is required.
- Lifting morphisms over containers maps element-wise through base morphism fold — container mapping inherits totality from element morphism.
- Reject isomorphic family duplication where morphism would be identity up to renaming — collapse to one owner and one projection instead of maintaining `Member` and `MemberDto` plus translator.

# Ingress Egress Materialization And Proof Stack

- Pydantic frozen models admit at boundaries; msgspec structs emit performance-critical wire shapes; discriminant values agree across ingress and egress — literal tags, enum values, and msgspec `tag` strings are one vocabulary declared at owner, mapped at adapters only.
- Materialize once at construction gate: wire → discriminated ingress → canonical `Member`; domain → projection → tagged struct → bytes — no mid-pipeline dict switchboards.
- Construction exit is first frozen `Member` or `Result[Member, E]`; validation exit may still be Pydantic discriminated ingress model until smart constructor maps it.
- Normalization may reshape outer envelopes and wire keys on ingress union members but must preserve discriminant literals byte-for-byte — normalization is not second discriminator pass; unknown tags still fail at validation exit.
- Enrichment operates on materialized `Member` values — derived slots attach via `copy.replace` or owner methods per arm; enrichment must not re-decode wire tags or re-run `TypeAdapter` on already-materialized members except at explicit re-ingress boundaries.
- Egress projection runs once per outbound event: `Member` → wire tagged struct → bytes — wire structs never flow backward into domain folds without declared read-path adapter.
- Declaration stack: PEP 695 `type` aliases, inline generics, `@disjoint_base`, `Literal`/`StrEnum` discriminants, `Annotated[..., Discriminator(...)]`, frozen slots models.
- Static proof: mypy strict + `exhaustive-match`, ty all-error, pydantic mypy plugin, Ruff bans on `TypeGuard`, `cast`, `Optional`, legacy generics.
- Runtime admission: Pydantic/`msgspec` discriminators decode exactly one variant; `enum.verify` guards enum laws; `beartype` on validated entrypoints confirms live instances after static proof — especially after container extraction or morphism translation.
- `match` + `assert_never` connects static exhaustiveness to runtime witness; `TypeIs` connects isinstance/tag proof to checker narrowing.
- Pydantic `Discriminator` callable return type must be `Literal | None` per arm mapping — `None` routes to validation failure, not silent skip; wildcard `case _` inside callable uses `assert_never` for impossible inputs.
- msgspec decode of `Member` and Pydantic `TypeAdapter(Member)` must agree on tag strings for each arm per vocabulary table.
- Fail closed on unknown discriminants, missing fields, and impossible combinations.
- Stage-skipping (`dict` → `Member` without validation owner, or `Member` → bytes without wire projection) requires composition-root exemption.
- Trusted-replay rehydration uses pinned `Decoder(Member)` only when store key, schema version, and encoder identity are declared beside vocabulary table — replay is not substitute for first ingress validation.
- `assert_never` witness lines stay excluded from coverage metrics — exhaustiveness witnesses are proof obligations, not behavioral branches to cover.

# Hierarchical Sum Composition And Inner Discriminants

- Variant family may carry another closed union as payload field — product-of-sum — when outer discriminant selects modality and inner selects sub-modality; both must be explicit `Literal`/`StrEnum` vocabularies in same bounded context.
- Outer routing uses top-level discriminant only; inner routing defers to nested `match` — never flatten nested unions into concatenated string tags (`"payment.card"`) unless wire contract mandates single flat vocabulary.
- Depth budget: three discriminator levels is practical ceiling before family splits into named sub-owners composed by field reference rather than one monolithic union alias.
- Pydantic hierarchical families compose with nested `Annotated[..., Discriminator(inner)]` inside outer union members, then outer `Annotated[..., Discriminator(outer)]` on enclosing field — each level is independent routing table with own validation error path.
- msgspec hierarchical families nest tagged structs as field types; inner `tag_field` values scope to inner union decoder, not promoted to outer tag strings.
- `@tagged_union` domain families may embed another `@tagged_union` or discriminated Pydantic member as frozen field; prove inner exhaustiveness in inner `match`, outer exhaustiveness in outer `match` — do not fuse into one mega-union when discriminants are semantically layered.
- Inner and outer discriminants must use different field names when both appear in same wire object unless callable `Discriminator` explicitly maps composite key.
- Callable outer discriminators may read nested dict slices only when `model_validator(mode="before")` has already shaped nested envelope without rewriting inner discriminants.
- Inner union `TypeAdapter` surfaces may validate sub-payloads in isolation before outer admission when outer envelope is optional or polymorphic — sub-adapter failure must not coerce to catch-all outer arm.
- Projection from hierarchical domain to flat wire reverses nesting order: outer tag first in serialization policy, inner tag inside nested object — adapter tables declare both mappings at family owner, not per call site.
- Exhaustiveness on hierarchical families requires `assert_never` at every union layer that is closed.

# Total Projections Construction Rails And Smart Constructors

- Projections `project: Member -> View` are total — every variant maps to `View` subtype or dedicated view union; missing arms are compile errors under `exhaustive-match`, not runtime `None` returns.
- View unions are smaller than domain unions when multiple domain variants collapse to one wire or report shape — collapse at projection fold, not by erasing discriminants in domain owner.
- Contravariant ingress adapters (`View -> Result[Member, E]`) are smart constructors; covariant egress adapters (`Member -> Wire`) are projections — never interchange exhaustiveness obligations.
- `computed_field` and `@property` on Pydantic variant members may project local views, but cross-variant summaries (`total_amount: Member -> Decimal`) belong on family owner as method taking `Member`, not duplicated per-variant properties.
- One smart constructor `admit: Ingress -> Result[Member, E]` owns all modalities — discriminant on ingress payload selects arm inside single `match`, not chain of per-variant constructors exported to callers.
- Per-variant `classmethod` factories are allowed only as private arm helpers called from smart constructor — public surface exposes `admit` or `from_ingress`, never `make_card` and `make_bank` siblings.
- Construction failures fold into one boundary error family (`ValidationFault`, `InvariantFault`, `UnknownDiscriminant`, `MigrationFault`) — not bare exceptions per variant.
- `Result[Member, E]` smart constructors must not silently default unknown discriminants to preferred variant.
- Per-arm validation messages map to shared fault constructors inside smart-constructor `match`; interior domain errors use separate rail families from boundary faults.
- Re-materialization after domain edit uses `model_validate` on Pydantic members or `copy.replace` on dataclass/`@tagged_union` members — construction rails and transition rails are distinct; do not bypass validators on ingress-shaped deltas.
- Error projection to wire encodes fault discriminant through msgspec tagged struct or stable string code — fault egress is total on boundary error union with own exhaustiveness witness.
- Recoverable vs terminal faults on ports use port-local `@tagged_union` — promoted to shared domain errors only when multiple bounded contexts consume identical evidence shapes.

# Version Evolution Semi-Closed Extension And Optional Arms

- Finite version sets encode as `Literal` version discriminant on each member or dedicated `Version` `StrEnum` field paired with `@enum.verify(EnumCheck.CONTINUOUS)` when versions are dense integers.
- Adding variant arm is breaking change for exhaustive consumers until every `match`, morphism, and materialization fold gains new arm — version discriminants scope which union shape applies at ingress, they do not bypass exhaustiveness.
- Deprecation arms remain in closed union until all producers migrate — mark deprecated variants in schema metadata and route projections to replacement arms in one adapter fold.
- `Map[Ver, Callable]` version dispatch is for implementation variation under fixed vocabulary, not unbounded plugin growth.
- Migration folds (`MemberV1 -> MemberV2`) are explicit total functions with `assert_never` on obsolete arms — run migration at ingress gate once, then domain sees only current `Member` alias.
- Semi-closed families declare typed `Extension` or `Unknown` variant when foreign discriminants must be preserved losslessly — carrying `discriminant: str`, `payload: frozendict[str, object]`, or bounded `TypedDict` with `extra_items=type` (Python 3.15).
- `extra_items=` on ingress `TypedDict` admits extension keys without polluting closed core; materialize to `Extension` when discriminant is not in closed vocabulary and policy allows capture; otherwise fail validation.
- `ReadOnly` slots (PEP 705) mark extension fields downstream may read but not treat as domain-invariant — promotion to closed variant requires explicit adapter fold.
- Extension arms participate in exhaustiveness with explicit `case Extension(...)` handling — semi-closed is not open `dict[str, object]`; unknown keys on closed variants still fail `forbid_unknown_fields` / `extra="forbid"`.
- Optional variant presence uses `Option[Member]` or dedicated sentinel union members (`Absent`, `Missing`, `Withheld`) — not `Member | None` when `None` conflates missing, null wire, and inapplicable modality.
- Per-arm optional fields use `Option[T]` or narrowed sub-variants (`EmptyCard`, `FilledCard`) when absence changes dispatch law.
- `NotRequired` on ingress `TypedDict` fields materializes to `Option` or absent sub-variants at construction gate.
- Exhaustive `match` over `Option[Member]` separates `Some(member)` with inner member fold from `Nothing`.
- Narrowing through `filter`/`map` combinators on variant collections returns `list[Member]` unless `TypeIs` predicate proves single arm.
- Serialization of `list[Member]` uses outer array with per-element discriminants — Pydantic and msgspec decode each element through same family discriminator policy as singleton members.

# Rich Owner Graduation Protocol Exclusion And Persistence

- Graduate to rich class owner when ≥3 dispatch arms need shared mutable-adjacent state (caches, registries, lifecycle hooks), algorithm-specific evidence slots, or fold-derived status semantics beyond frozen payload — graduation adds behavior, not parallel type.
- Graduation preserves closed union inside rich owner as nested variants or single discriminated field — exterior API exposes one owner type; `Member` alias becomes `Owner.Member` or internal union field, not sibling module-level export.
- Do not graduate to abstract base plus concrete leaves when `@tagged_union` or discriminated unions already own modality — graduation deepens one owner; it does not spawn nominal hierarchy parallel to sealed family.
- Smart constructors on rich owner subsume per-arm `admit` folds — construction rail returns `Result[Owner, E]`; ingress still validates through Pydantic discriminated models before owner constructor runs.
- `__post_init__` on msgspec or dataclass rich owners replays on `copy.replace` transitions per arm — variant-specific post-init logic routes through arm-aware replace methods, not tag-string conditionals inside one undifferentiated hook.
- Rich owner serialization tiers (`@computed_field` vs `@property`) apply per-arm projections exported through one owner method taking `Member` — per-variant dump helpers are private arm helpers, not public API.
- `Protocol` ports implement capabilities (`Decode`, `Store`, `Render`), never variant identity, discriminant tags, or sealed family membership; closed modality stays on `Member` with `match` and `TypeIs`.
- Reject `Protocol` per variant (`CardHandler`, `BankHandler`) when arms share one bounded context — collapse to one port parameterized on `Member` with exhaustive interior `match`, or one port method accepting `Member` and returning `Result`.
- Structural ports may be generic over `T: Member` when algorithms are parametric — bound is family alias, not single arm; implementation body still pattern-matches exhaustively.
- `runtime_checkable` protocol unions simulating closed variant sets are banned — runtime `isinstance` across unrelated implementers cannot discharge exhaustiveness.
- Plugin extension at system boundary uses `singledispatch` or registry rows keyed by foreign types — not `Protocol` branches for in-repo `Member` arms.
- Port-local fault unions (`@tagged_union` with recoverable vs terminal arms) stay file-internal to port module.
- Persisted polymorphic rows encode `Member` through wire tagged structs or versioned envelope structs (`schema_version`, `payload`) — storage schema matches egress projection, not ingress Pydantic shapes when policy diverges.
- Each arm's persistence row carries same outer discriminant vocabulary as live egress — drift between stored tags and current `Member` arms is repaired at read-path migration folds, not silent default arm selection on decode.
- Round-trip proof (`decoder.decode(encoder.encode(member))`) runs before cache write or cross-process handoff — polymorphic detail slots and nested sub-owners inherit same proof gate; proof failure maps to boundary `Fault` or `Result`, never coerces to default variant, `Extension` arm, or `None` unless slot policy explicitly declares optional polymorphism.
- Versioned storage uses migration fold at read boundary once (`StoredV1 -> Member`) — domain modules after migration see only current `Member`; obsolete arms remain in migration fold with `assert_never`, not in active domain `match`.

# Vocabulary Table Registry Materialization And Contract Proof

- Vocabulary table at family owner declares one row per closed arm: canonical wire token, Pydantic `Literal`, msgspec `tag`, `StrEnum` member, OpenAPI enum entry, and optional persistence key — adapters and registries read same frozen table; domain modules import enum and `Literal` types only.
- Registry materialization projects each table row into typed handler slot — callable, nested registry reference, or lazy factory — keyed by row's `StrEnum` member; row keys are enum members, not raw strings reconstructed at lookup sites.
- Materialize registries once at module import as `Mapping[Kind, Row]` or `frozenset[Row]` with `Kind: StrEnum` — not mutable dicts populated by side-effect registration loops that admit duplicate or orphan keys without static notice.
- Row count must equal closed union width — every arm has exactly one registry row and every row maps to exactly one arm; orphan rows and missing rows are merge blockers caught by table parity tests before behavioral suites run.
- Semi-closed `Extension` arms get registry rows only when policy assigns explicit handler behavior — otherwise extension routes to boundary fault constructors, not silent default row fallthrough.
- Registry row is frozen record (`msgspec.Struct`, frozen dataclass, or Pydantic frozen model) carrying `kind: Kind` plus behavior slots — never bare `Callable` stored under string key without discriminant field on row itself.
- Rows store behavior references (`handler`, `project`, `encode`) as typed callables with signatures parameterized on narrowed arm type where possible — `Callable[[Owner.Card], Out]` per row, not `Callable[[Member], Out]` when row already fixes `kind=Kind.CARD`.
- Catalog rows (`Tool`, `Check`, `Bind`) embedding `Member` fields inherit family vocabulary table for strategy registration — catalog registry width is independent of `Member` width but `Member`-typed fields in row still narrow through family `match` fold, not row-local tag strings.
- Reject `dict[str, Handler]`, `@lru_cache` keyed by raw discriminant strings, and module-level handler lists indexed by integer position — all bypass enum closure and exhaustiveness coupling.
- Private arm helpers referenced from row slots stay unexported — public surface exposes registry lookup returning `Result[Out, E]` or dispatches through one owner method taking `Member`.
- Table parity tests iterate `Kind` enum members and registry row keys in one parametrized assertion — set difference in either direction fails CI; test imports same frozen table production code uses.
- Typed row collections use `tuple[Row, ...]` or `frozenset[Row]` with literal length when union width is small and fixed — mypy and ty flag row tuple length drift against `Member` alias width when rows carry phantom `Kind` literals per slot.
- Adding arm updates vocabulary table, registry rows, `match` folds, morphisms, migration folds, hypothesis registry, and parity tests in one change — registry-only updates without fold updates leave live behavior unrouted while static proof still passes.
- `assert_never` inside registry lookup defaults witnesses impossible `Kind` values after enum exhaust — lookup functions take `Member`, narrow with `match`, then delegate to row slot; they do not accept raw `Kind` from untrusted ingress without prior materialization.
- Registry rows keyed by `StrEnum` tags are projections from `Member` to row lookup keys — key fold is total and lives beside owner; registry stores behavior, not variant identity.
- When ingress alias spellings differ from canonical registry keys, remap tables live at adapter seam on vocabulary axis — remap output is canonical `Kind`; registry rows never register alias strings as secondary keys.
- msgspec wire-only tags differing from ingress discriminators map through egress remap rows declared beside vocabulary table — domain registry keys stay on canonical enum members throughout.
- Remap tables are total on declared alias closure — unknown alias at ingress fails validation before registry lookup; remap is not catch-all extension path.
- Cross-context shared atoms import from vocabulary owner; each context's `Member` registry binds local rows to imported enum members — shared tokens do not imply shared handler registries across bounded contexts.
- OpenAPI `oneOf` discriminator mappings may generate vocabulary table draft rows for review — generated rows are not authoritative until merged into owner table and parity tests pass; schema is evidence, not source of truth over model declarations.
- Codegen emits registry row skeletons with `kind` slots and placeholder handler types — implementers fill behavior slots; codegen does not emit string-keyed handler dicts.
- Reverse flow from msgspec schema introspection to vocabulary rows requires explicit human merge — automatic promotion of discovered tags into closed union membership without fold updates is rejected.
- Pydantic discriminated unions export JSON Schema `oneOf` plus `discriminator` mapping when ingress models generate OpenAPI — `Literal` discriminant fields on each member drive mapping; do not hand-author parallel discriminator tables diverging from model declarations.
- Callable `Discriminator` plus `Tag()` arms document computed routing in schema via member `title` and tag metadata — OpenAPI consumers must see same tag vocabulary as runtime `TypeAdapter` routing.
- msgspec wire tags used only on egress still appear in published API contracts when ingress and wire are intentionally identical — when ingress aliases differ, OpenAPI reflects ingress discriminators; wire-only tags stay in adapter documentation, not duplicated in public schema.
- `RootModel` with `Literal` root in union members appears as const/schema enum arms — scalar-tagged members do not use wrapper-only schema nodes hiding discriminant.
- Schema version bumps require simultaneous updates to discriminator mapping, migration fold, and contract proof samples — OpenAPI drift is static defect caught by schema diff tests against `model_json_schema()`, not runtime discovery.
- Vocabulary-table contract tests assert parity across Pydantic `Literal`, msgspec `tag`, `StrEnum` member, and wire string for every arm — one parametrized table drives assertions; adapters read table in production.
- Per-arm round-trip samples live beside family owner — each variant constructs canonical `Member`, projects to wire, decodes, and asserts equality; nested sub-owners add Cartesian samples only when cross-arm law requires combined proof.
- Exhaustiveness regression runs mypy with `exhaustive-match` and ty all-error on modules owning `match` folds — adding arm without updating folds fails CI before behavioral tests execute.
- Morphism contract tests prove total mapping for declared boundary adapters and explicit `Result` failure for unmappable ingress when partial morphisms are policy — inverse morphisms are separate test modules when round-trip is required.
- Discriminator callable tests feed synthetic dict slices per arm and assert routed member type — `None` return and impossible inputs hit `assert_never` paths in test fixtures, not production catch-alls.
- Hypothesis strategies draw from closed arm registry rows keyed by `StrEnum` or `Literal` tags via `st.sampled_from` over materialized exemplars per arm, not `st.text()` filtered by runtime validation.
- `st.one_of` arms must map one-to-one to union members — strategy cardinality matches static union width; semi-closed `Extension` arms get dedicated strategies with typed payload bounds when policy admits them.
- Composite strategies for hierarchical families build outer arm first, then inner arm conditioned on outer discriminant — conditioning mirrors nested `match` structure, not independent random tags.
- Registry-driven catalog rows embedding `Member` fields register strategies through same vocabulary table — hypothesis registration imports family alias types, not wire struct modules.
- Table-driven hypothesis strategies import materialized exemplars from registry row fixtures — strategies do not scrape codegen output or OpenAPI JSON for tag strings independent of owner table.
- Shrinking preserves discriminant legality — custom `st.composite` builders rebuild valid `Member` values after shrink steps; invalid tag mutations are rejected at construction gate, not used as shrink targets.
- Law tests over folds assert totality: `project(member)` never returns sentinel `None` for closed families; `admit(ingress)` failures are always typed boundary errors for out-of-vocabulary discriminants.

# Sub-Owner Family Topology Registry Composition And Cross-Field Law

- When hierarchical depth exceeds three discriminator levels or variant count exceeds practical exhaustiveness surface, split into named sub-owners (`PaymentMethod`, `SettlementRail`, `FailureReason`) referenced by field type — each sub-owner exports own `Member` alias and vocabulary table.
- Top-level `Member` carries sub-owners as product fields (`method: PaymentMethod`, `rail: SettlementRail`) — outer discriminant selects transaction class; inner families evolve on independent version arms without rewriting outer union.
- Each sub-owner materializes own registry beside vocabulary table — top-level `Member` registry rows hold nested registry references or projection callables into sub-registries, not flattened string keys spanning both vocabularies.
- Top-level lookup on `Member` routes outer discriminant first; nested field access triggers sub-owner registry lookup on sub-field value — two-stage dispatch mirrors nested `match` structure; deeply nested product-of-sum shapes beyond two-stage outer/inner delegation require standardized multi-stage lookup publish at root — split into sub-owners when depth exceeds three discriminator levels.
- Sub-owner registry width proves independently — top-level parity tests do not subsume inner table parity; Cartesian contract samples cover cross-sub-owner combinations only when top-level smart constructor proves cross-field law.
- Sub-owner re-export for adapter field typing does not re-export sub-registries to domain modules — interior code pattern-matches on `Member` and sub-fields; registry lookup stays at boundary adapters and composition roots.
- Sub-owner registries live beside owner module — top-level family imports sub-union aliases for field typing only; sub-owner folds and morphisms do not leak into top-level `match` arms except through explicit projection methods on sub-field.
- Cross-sub-owner invariants (method compatible with rail) belong on top-level smart constructor `admit` — sub-owners prove local exhaustiveness; enclosing record proves cross-field law once at construction gate.
- Decomposition is preferred over monolithic string discriminants (`"payment.card.settled"`) — flat tags couple evolution axes and force synchronized version bumps across unrelated modalities.
- Re-merge sub-owners only when discriminants collapse to one vocabulary and folds fuse without nested `match` — otherwise keep field-reference composition.

# Collection Dispatch Rail Binding And Stack Axis Seams

- `Block[Member]`, `tuple[Member, ...]`, and `list[Member]` dispatch by mapping each element through same member-level fold or registry-backed projection — collection dispatch inherits per-element totality; no container-level tag shortcut.
- Batch registry lookup folds `Block[Member]` to `Block[Out]` with `block.map(step)` where `step` is total single-member projection — partial batch handlers that skip unknown arms violate same law as partial member folds.
- Grouped aggregation keys project each element through `Member -> Kind` before counting — `count_by_kind` is total key fold composed with `Counter` or `Map` algebra, not registry side effect mutating global handler state.
- Filtered collections narrowed to one arm require element-wise `TypeIs` proof or dedicated narrowed container alias — filtering then indexing registry rows by assumed arm without proof is same defect as post-filter `cast`.
- Serialization registries for `list[Member]` reuse singleton-member wire policy per element — collection encode tables reference element vocabulary table, not parallel per-container tag namespace.
- Registry handlers return `Result[Out, E]` or `Option[Out]` uniformly when exposed at boundary seams — mixing bare exceptions per row breaks single boundary error family.
- Heterogeneous per-arm success types require new discriminated result family — registry rows do not widen success to `object`; each row's success type must unify under declared result union or row projects into shared covariant view type first.
- `result.bind` chains over registry dispatch compose only when inner handler returns same error rail — cross-row error translation happens inside lookup fold, not in callers via ad hoc `except` per arm.
- Interior domain transforms on `Member` stay on rail-free canonical layer — registry rows at boundaries collapse `Result` once; projected views do not re-enter registry lookup without rematerializing to `Member`.
- Port implementations parameterized on `Member` may hold registry references as constructor dependencies — port method still pattern-matches exhaustively; registry supplies arm-specific behavior slots match delegates to, not replaces.
- Vocabulary axis: registry row keys import from vocabulary owner; rows never declare parallel token strings; seam remap tables live on vocabulary axis and feed adapter ingress before registry lookup sees canonical `Kind`.
- Materialization axis: registry lookup runs on materialized `Member` after construction gate exit; ingress `TypeAdapter` validation completes before any row handler executes domain behavior.
- Replacement axis: endofold and enrichment transitions invoke registry transition rows or owner methods, then apply `copy.replace` / `model_copy` on returned member — registry handlers do not mutate materialized owners.
- Collection axis: `Block`/`Map` folds over registry projections use same total step function as singleton dispatch; registry rows do not special-case container carriers with different behavior tables.
- Rail axis: boundary registry surfaces return `Result`; interior folds on `Member` remain rail-free unless bounded context explicitly routes domain faults through typed error family unrelated to ingress vocabulary.
- Wire egress axis: encode rows may differ from ingress handler rows when wire projection is not symmetric — egress registry or projection fold lives beside ingress registry; both read vocabulary table for tag strings.
- Registry lookup entrypoints accept `Member`, execute exhaustive `match`, and delegate to row narrowed for that arm — lookup does not accept `Kind` alone from wire carriers or dict slices without prior smart constructor proof.
- Row delegation uses arm-narrowed types inside each match arm — handler invocation after `case Card(...) as card` passes `card` to row callable typed for `Card`, preserving checker narrowing without `cast`.
- `beartype` on registry lookup entrypoints validates live `Member` instances at boundary calls — runtime admission confirms discriminant fields match arm selected by `match` before row handlers run.
- Failed lookup on impossible materialized states hits `assert_never` in match default — registry rows are not second routing path that accepts values static proof already excluded.
- Endofold registry rows return new `Member` instances via `copy.replace` or validated reconstruction — transition tables do not mutate fields in place or bypass construction validators on ingress-shaped deltas.

# Composition Root Assembly Seams And Consumer Closure

- Python `>=3.15` binds closed variant families at composition roots so ingress routes, registry lookup, migration folds, wire egress, and automation consumers share one `Member` alias, one vocabulary table, and one smart-constructor gate.
- Composition roots import `Member`, vocabulary `StrEnum`/`Literal` owners, frozen registry rows, and vocabulary parity tables from family owner module only — no parallel inline unions, duplicate `Discriminator` mappings, or per-route vocabularies.
- Module-level `TypeAdapter[Member]` and pinned `Encoder`/`Decoder` pairs cache at root beside family import — hot paths do not allocate per-request adapters or fork discriminator policy per ingress carrier.
- Registry `Mapping[Kind, Row]` materializes once at root import from frozen vocabulary table — roots publish lookup entrypoints accepting `Member`, not mutable `@register` dicts populated by decorator side effects after handler bind.
- Open extension after import for plugin hosts requires vocabulary owner update and root registry rebuild at import — runtime union mutation and post-hoc `Enum` extension are rejected.
- Sub-owner registries (`PaymentMethod`, `SettlementRail`) import from sub-owner modules at root only for adapter wiring and nested lookup delegation — interior domain modules import sub-union aliases for field typing, not sub-registry lookup tables.
- Import-cycle resolution belongs at root: family modules stay dependency-minimal; roots wire families into materialization adapters, settings slices, catalog `Bind` rows, and seam exports without family modules importing domain records or port implementations.
- Trusted-replay paths pin adapter module identity, `schema_version`, store key, and encoder module path beside vocabulary table — replay validates through same root `TypeAdapter` and smart constructor as live ingress, then hands off `Member` to interior folds without bypassing migration when stored bytes predate current version.
- Cross-process `Member` replay for multiprocessing workers depends on encoder module path stability and enum pickle by value — root worker boot must prove vocabulary table parity and registry width across forked importers in one CI gate.
- HTTP, CLI, queue, webhook, and replay carriers share one ingress discriminated model family and one root smart constructor; route wiring differs by carrier decode only.
- Root admission is one polymorphic gate: carrier decode → ingress validate → exhaustive `match` → `Result[Member, BoundaryFault]` — staging dicts and partial ingress models never persist in root registries after promotion succeeds or fails.
- Callable `Discriminator` hooks declared on ingress models execute inside root-cached `TypeAdapter` instances — cyclopts choices, OpenAPI enum metadata, and CLI surplus-token policy reference same `StrEnum` or `Literal` vocabulary family owner exports, not parallel string tuples on route modules.
- Callable discriminator literals from ingress hooks must unify with cyclopts choice metadata and OpenAPI enum export in one generative schema source — dual maintenance at root schema export is a merge blocker until both surfaces read the same vocabulary table.
- Interior modules never admit foreign discriminants from raw carrier slices without root `TypeAdapter` proof.
- Foreign bytes reach `Member` through root `admit_*` / `TypeAdapter` and smart constructor.
- `Member` flows to interior folds via handler exhaustive `match` on materialized union members, to wire bytes via root `project_*_wire` and module `_ENCODER`, to registry behavior via root dispatch entrypoints accepting `Member` with exhaustive `match` then arm-narrowed row handler, to foreign context via seam adapter morphism at boundary only, and to persistence via root encode with read-path migration fold.
- Settings boot → construction context: root `project_*_config` → constructor inputs.
- Stored bytes → `Member` replay: root pinned `Decoder` → migration → smart constructor.
- `Block[Member]` batch projection uses root `block.map(member_project)` with per-element totality.
- Fault → automation envelope: root `envelope` distillation with `BoundaryFault` discriminant.
- Chained root pipelines compose as typed steps: decode → ingress validate → construct `Member` → registry lookup or interior fold → enrich via owner method → project wire → encode — no step widens `Member` to `object`, `dict[str, Any]`, or bare `str` discriminants between enumerated handoffs.
- Interior domain folds receive `Member`, `Option[Member]`, or `Block[Member]` only — raw dict carriers, wire tag strings, and unmaterialized `Kind` values stop at root guards and named adapter modules in root import graph.
- Root `Bind` rows pin handler signatures to `Member` or `Result[Member, E]` — surplus CLI tokens and arity faults surface before registry lookup runs on handler params.
- Catalog rows embedding `Member` fields register hypothesis strategies through vocabulary table at root build — strategy cardinality matches union width; root import of catalog modules fails when new arm lacks registry row and strategy exemplar.
- Endofold transition tables keyed by `(from_kind, to_kind)` live at root beside ingress registry — forbidden transitions return typed `InvariantFault` at root guards before interior folds observe illegal modality changes.
- `rail(bind)` closes settings, layer stack, and scope opener before handler execution on `Member`-typed routes — correlation attrs (`run_id`, `strict`, trace keys) are root projection fields, not duplicate discriminant slots on variant arms unless domain contract explicitly owns same lifecycle vocabulary.
- Layer stack slots (`checked`, `logged`, `traced`) on root-bound handlers log rejected discriminants at adapter edge using owner policy: arm `.name` on developer surfaces, wire `tag` on audit surfaces — one policy per vocabulary table row, applied consistently across layers bound at root.
- Strict policy faults on empty or skip folds reference enum-backed status vocabulary — empty handler results map to `strict:` prefixed faults at root guard without re-encoding status as ad hoc strings inside per-arm handler bodies.
- Spawn, retry, and govern layers attach only on effectful root entries carrying `Member` — pure folds on immutable members do not add transport vocabulary or claim/verb overrides foreign to `Bind` row.
- Trace spans on root materialization adapters emit discriminant arm, `schema_version`, and materialization stage at handoff — interior domain spans omit msgspec module paths and Pydantic model names foreign to bounded context vocabulary.
- Root `Fault` and `Envelope` variants carry enum- or `Literal`-typed fault discriminants (`UnknownDiscriminant`, `ValidationFault`, `InvariantFault`, `MigrationFault`) imported from boundary error family — synthetic transport prefixes format inside root guards; structured fault interiors use enum members, not per-arm exception type names on the wire.
- Smart constructor `match` arms map per-variant validation messages to shared fault constructors.
- Root `envelope` distillation emits one closed fault namespace per transport family; callers receive `Result[Member, BoundaryFault]`, not variant-specific exceptions crossing stdout line.
- `parse_fault` and operational fault paths materialize same `Envelope` struct — fault union discriminants align with success envelope tags so automation consumers see one token namespace per root invocation.
- Cap truncation and distillation on `_emit` preserve fault discriminant identity fields required for dispatch and replay — truncation drops verbose evidence slots, not `UnknownDiscriminant` tokens or `schema_version` closure fields.
- Interior domain error families stay off root envelope unless explicitly promoted — domain `match` on transform faults does not reuse ingress discriminant vocabulary on exported automation lines.
- Rich owners graduating from `Member` families expose one root factory returning `Result[Owner, E]` — ingress still validates through Pydantic discriminated models before owner constructor; root wiring does not expose per-arm undecorated constructors to handlers.
- Sub-owner fields on top-level records resolve through root smart constructor cross-field law — method compatible with rail, version alignment across nested families; sub-owner local exhaustiveness proves at sub-module; top-level `admit` proves cross-field law once at root.
- Owner egress through root uses one projection method taking `Member` or canonical payload — `@computed_field` tiers and wire dumps are owner methods, not per-route dump helpers in root modules.
- `__post_init__` replay on rich owner transitions routes through arm-aware replace at root enrichment boundaries — root does not call undifferentiated post-init hooks keyed by raw tag strings extracted from carriers.
- Root encode emits current-version wire structs or versioned envelopes — dual-write of obsolete layouts requires composition-root documented transitional policy with sunset date.
- Read-path migration folds live in root adapter submodules as total `StoredEnvelope -> Result[Member, MigrationFault]` — domain modules after root handoff see only current `Member`; obsolete arms stay in migration modules with `assert_never`, not in active handler `match`; unknown stored tags fail closed with `MigrationFault` or typed `Extension` when policy requires capture, never map to default arm.
- Index and query keys for persisted `Member` graphs project through total key folds at root materialization — databases store projected scalars from `Member -> Kind`, not raw discriminant strings invented at handler sites.
- Round-trip proof before cache write or cross-process handoff runs at root egress guard: `decoder.decode(encoder.encode(member))` per arm sample from vocabulary table — polymorphic nested sub-owners add Cartesian samples only when top-level smart constructor proves cross-arm law.
- Published OpenAPI for polymorphic ingress reflects Pydantic `Annotated[..., Discriminator(...)]` declarations — root schema export does not maintain hand-authored `oneOf` tables parallel to model definitions.
- Automation consumers decode stdout through same module-level envelope decoder root uses for emit — pass-one logic validates `schema_version`, `claim`, and `verb` before interpreting `Member`-backed `report` or `error` slots.
- Detail slots carrying polymorphic `Member` projections or arm-specific evidence narrow through discriminant tags shared with egress `tag_field` — consumer `match` uses same vocabulary encoder emitted; foreign synonyms were mapped at root ingress and egress adapters only.
- When `truncated=True` on envelope, consumers resolve full evidence through history artifacts keyed by `run_id` — wire line is the index; truncated polymorphic tuples are not complete arm populations.
- `experimental_allow_partial` and tiered Pydantic validation apply only at declared ingress gates for streaming or incremental carriers — partial shapes do not enter root registry lookup or interior `Member` folds until completion proof closes all required fields on the declared ingress owner.
- Partial admission stops at validation exit — construction and enrichment assume intentional partial shape only when composition root documents tier policy; frozen domain invariants requiring full field closure reject partial materialization.
- OpenAPI and client SDK generation mark polymorphic fields with same discriminator required for complete admission — partial tiers are not silently exposed as optional domain arms.
- Semi-closed `Extension` capture routes through root gate only when composition root documents tier policy — extension arms materialize with typed payload slots at gate; partial dicts with unknown keys on closed arms still fail `forbid_unknown_fields`; extension is not partial-validation escape hatch.
- Domain modules import `Member` alias and vocabulary enums only — ingress Pydantic models, msgspec wire structs, and `TypeAdapter` instances stay in boundary adapter modules.
- Sub-owner aliases export from sub-owner modules; top-level family re-exports for field typing only when bounded context treats sub-family as stable public vocabulary — otherwise import sub-alias from sub-module at adapter sites.
- `match` folds and `TypeIs` predicates import from family owner module — handlers do not re-declare inline unions or duplicate tag string constants from wire modules.
- Interior functions annotate parameters as `Member`, `Option[Member]`, or `Block[Member]` — erased `object` or `dict[str, Any]` carriers re-enter only at boundary adapters with explicit admission.
- Test factories building `Member` for domain suites use canonical constructors or `model_validate` on ingress fixtures in test boundary modules — domain tests do not import wire decoders for convenience; test fixtures build `Member` through same smart constructor as live ingress.

# Test Harness CI Gates And Evolution

- Importing composition root module graph fails when `Kind` iteration gains member without vocabulary table row, registry row, `match` fold arm, migration fold update, and hypothesis exemplar — enum closure and union width parity are import-time defects, not runtime surprises.
- Rejection-path tests assert typed `UnknownDiscriminant` and `ValidationFault` carry rejected token and owning enum name.
- Success-path tests construct through root smart constructor matching live ingress, not enum member indexing hacks.
- Round-trip property tests at root exercise `Member` → wire → decode → smart constructor → equality on discriminant fields — metamorphic proof runs against root codecs and published registry lookup, not interior modules without egress guards.
- Static exhaustiveness witnesses (`match` + `assert_never`, ty / mypy `exhaustive-match`) live beside root handler maps and registry rows — test modules import same frozen registry symbol root publishes, not duplicated subset map.
- `assert_never` lines in root promotion and registry lookup remain proof obligations excluded from branch-coverage targets — exhaustiveness is proven by type checkers and table parity, not by executing unreachable defaults.
- Import-linter rules flag domain modules importing root registry modules, interior handlers importing `TypeAdapter` from adapter implementation files, and duplicate discriminant strings across ingress `Literal`, msgspec `tag`, and comment-maintained schema lists.
- OpenAPI and schema snapshot tests diff polymorphic `oneOf` discriminator mappings from `model_json_schema()` against vocabulary table rows — drift fails CI before handlers execute on new arms.
- Stryker or equivalent mutation on root registry lookup and smart constructor `match` arms requires kill ratio on discriminant routing — mutants that remove arms, drop registry rows, or default to catch-all must fail exhaustiveness type-check, table parity tests, or contract tests, not pass with silent wrong-arm behavior.
- Mutation scoring on registry lookup must kill mutants that drop rows or remap keys — parity tests and exhaustiveness checks catch routing mutants before mutation gates accept change.
- Ruff policy bans `TypeGuard` and `cast` on family modules — mutations introducing tag-check casts should fail lint before mutation scoring.
- Adding variant arm without updating vocabulary table, morphisms, migration folds, OpenAPI mapping, registry rows, and hypothesis registry is merge blocker — static proof and contract suite encode same closed membership as source.
- `beartype`-decorated entrypoints taking `Member` receive mutation probes on live instances — runtime admission catches discriminant field tampering static proof cannot see after morphism or container extraction.
- Adding variant arm is one root promotion unit: vocabulary table row, registry row, smart constructor arm, migration fold arm, OpenAPI mapping, hypothesis exemplar, root `Bind` handler correlation, and metamorphic sample — partial promotion leaves routes unrouted while static interior proof may still pass.
- Version literal bumps on polymorphic envelopes update root codec submodule, migration fold, and contract samples simultaneously — forward-compatible ingress of unknown versions fails at root materialization with `MigrationFault`, not silent default arm selection.
- Deprecation arms remain in closed union and root registry until all producers migrate — root projections route deprecated arms to replacement behavior in one adapter fold; arm deletion precedes wire sunset, not reverse.
- Codegen draft registry rows and OpenAPI discriminator hints merge through human review into owner vocabulary table — generated rows are not authoritative until root import parity tests pass.
- Arch rules enforce: domain folders never import sibling domain folders; domain modules never import msgspec decoders or Pydantic `TypeAdapter` except through type-only boundary stubs when unavoidable; composition roots never import foreign registries or file-local fold helpers from other bounded contexts.

# Collapse Tests

- Collapse root inline unions, per-route discriminator forks, string registry dispatch before materialization, interior `TypeAdapter` calls, default arms on unknown tags, domain test wire decoder imports, and dual OpenAPI tables beside Pydantic models.
- Collapse handler signatures or `Bind` params declaring `A | B | C` beside imported `Member` to family owner alias and root-published `TypeAdapter`.
- Collapse HTTP and CLI modules declaring different `Literal` sets for same concept to one vocabulary table and root smart constructor.
- Collapse `HANDLERS[kind_str]` at root before materialization to `Member`-accepting lookup with exhaustive `match` after smart constructor.
- Collapse root migration or ingress mapping unknown discriminants to preferred variant to `UnknownDiscriminant` or typed `Extension` with root-documented policy.
- Done when every root import of closed variant family resolves to one owner module, every ingress route shares one smart constructor and vocabulary table, cross-axis handoffs route through named root entries, registry width equals union width at import, and CI gates fail on arm drift before handlers execute.

# Anti-Patterns

- Flat `dict[str, Callable]` or `if/elif` on `kind` strings; smart unions without discriminators; parallel class hierarchies when `@tagged_union` or discriminated unions suffice; `typing.cast` after tag checks; exported `@tagged_union` error types; thin wrapper variants; runtime-checkable `Protocol` unions as closed sets; bare `str` discriminants; msgspec YOLO union decode; multiple discriminant field names without callable justification.
- Mutable module-level dict populated by `@register` decorators at import time — orphan and duplicate keys without parity proof.
- Registry lookup by string tag extracted from payload before materialization — bypasses construction gate and smart constructor narrowing.
- Single registry row handling multiple arms via internal `if` on kind — recreates switchboard inside row instead of one row per arm.
- Sharing one handler callable across rows without arm-specific narrowing — erases per-arm type proof and hides arm-specific fault semantics.
- Top-level flat registry merging sub-owner keys into one string namespace — couples evolution axes and breaks independent sub-owner parity tests.
- Interior domain modules importing registry modules for routing — registry executes at composition-root and adapter seams; domain law stays on exhaustive `match` folds owned by family module.
- Transition table entries calling ingress validators on already-materialized members — confuses migration, endofold, and re-ingress boundaries.
- OpenAPI-generated handler maps checked into source without vocabulary table merge — dual vocabulary sources diverge from model-declared discriminators.
- Flattening hierarchical discriminants into one string namespace to avoid nested `match` blocks — loses independent evolution of inner and outer vocabularies.
- `dict[str, Handler]` registries keyed by discriminant strings inside domain modules — bypasses closed union exhaustiveness.
- Partial folds returning `None` for unhandled variants — totality violation; use `Result` or dedicated `Unhandled` error arm.
- Identity morphisms between duplicate family types (`Dto` ↔ `Model`) signal missing collapse into one canonical owner.
- Open `Extension` arms without typed payload — extension becomes second unvalidated dict pipeline.
- Filtering `list[Member]` then casting elements to single arm — replace with `TypeIs` element proof or dedicated narrowed collection type.
- Version-specific parallel union types (`MemberV1`, `MemberV2`) without migration fold — permanent family duplication.
- Inner union decode via try-each-member loops on tagged wire — always attach explicit inner `tag_field` or discriminator.
- Domain modules importing decoders or `TypeAdapter` for interior routing — boundary leak bypassing materialization gates.
- Persisting ingress Pydantic models directly when wire and ingress shapes diverge — storage inherits validation-only fields and wrong discriminator policy on read.
- `Protocol` handler per variant arm in one bounded context — exhaustiveness evasion via runtime plugin tables.
- Skipping round-trip proof for polymorphic cache slots because "same process" — enrichment bugs and manual assembly still drift discriminants.
- Graduating to rich owner by duplicating `Member` as parallel optional fields on bag class — loses closed union proof and reintroduces impossible field combinations.
- Migration at read time mapping unknown stored tags to default arm — fail closed with `MigrationFault` or typed `Extension` when policy requires capture.
- Hypothesis `st.from_type(Member)` on closed unions without registered exemplars — generates invalid tags that waste cycles on validation failures instead of lawful samples.
- OpenAPI discriminator tables maintained separately from `Annotated[..., Discriminator(...)]` declarations — dual vocabulary sources.
