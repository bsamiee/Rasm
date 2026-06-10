# Hierarchical Decomposition And Product-Sum Routing Lattice

# Decomposition Boolean Algebra

- Split predicate is OR over decomposition axes — any single trigger (depth >3, independent migration axis, orthogonal registry vocabulary, cross-field law spanning distinct sub-owners, exhaustiveness surface overflow) suffices for sub-owner extraction review.
- Retain predicate is AND over monolithic conditions — shared `Kind` enum, single version literal band, single smart-constructor fold, depth ≤3 with product-of-sum inner payloads, and inner arms meaningful only under outer context jointly block premature split.
- Promotion review evaluates `(split_trigger OR _) AND NOT (retain_and)` — route-local nested union inside one handler does not satisfy split until inner family appears in ≥2 bounded contexts or owns independent migration fold.

# Decomposition Trigger Matrix

- Split monolithic `Member` into named sub-owners when any axis fires: discriminator depth exceeds three levels; closed arm count exceeds practical exhaustiveness surface for one `match` block; independent version evolution on orthogonal modalities; registry row width exceeds one vocabulary table without shared `Kind` enum; cross-field law references fields owned by distinct sub-vocabularies.
- Retain monolithic union when all arms share one discriminant vocabulary, one version literal band, one registry `Kind` enum, and one smart-constructor `admit` fold — depth ≤3 with nested inner unions as payload fields is product-of-sum inside one outer owner, not automatic sub-owner split.
- Product-of-sum (`outer: Literal["payment"]`, `method: PaymentMethod`) is default when outer modality gates lifecycle and inner modality gates implementation detail — outer `match` routes transaction class; inner `match` routes method rail; both vocabularies stay explicit `Literal`/`StrEnum` in same bounded context.
- Flat concatenated tags (`"payment.card.settled"`) are rejected unless wire contract mandates single vocabulary — flat tags couple evolution axes, force synchronized version bumps, and break independent sub-owner parity tests.
- Count decomposition triggers at composition-root promotion review — route-local nested unions inside one HTTP handler do not justify sub-owner extraction until the same inner family appears in ≥2 bounded contexts or carries independent migration folds.

# Depth Budget And Discriminator Level Triage

- Level 0 — singleton `Member` with no nested closed unions; one discriminant field; one vocabulary table; one registry `Mapping[Kind, Row]`.
- Level 1 — outer union with inner union as frozen payload field; outer and inner discriminants use distinct field names on same wire object; inner `tag_field` scopes to inner decoder only.
- Level 2 — two nested closed unions plus scalar product fields; each union layer owns independent `TypeAdapter` surface and `assert_never` witness at its `match` boundary.
- Level 3 — practical ceiling before named sub-owner split — three discriminator routing tables in one admission graph is merge blocker for monolithic growth unless wire contract is frozen legacy.
- Beyond level 3 — extract deepest inner family into sub-owner module exporting own `Member` alias, vocabulary table, and registry; top-level record holds sub-owner as field type (`method: PaymentMethod`, `rail: SettlementRail`).
- Callable outer `Discriminator` reading nested dict slices is allowed only after `model_validator(mode="before")` shapes envelope without rewriting inner discriminants — computed routing does not increase depth budget; it reorders decode, not union width.

# Vocabulary Depth Metadata

- Each family vocabulary table carries `depth_level: Literal[0, 1, 2, 3]` metadata — encodes declared discriminator routing depth for promotion review and import-time CI gates.
- Declared `depth_level` must match nested union graph width — metadata claiming level 2 while admission graph carries four independent routing tables is merge blocker unless wire contract is frozen legacy.
- Sub-owner extraction decrements monolithic depth and assigns independent `depth_level` on sub-owner table — outer admission graph drops inner routing tables once inner family exports as field-typed sub-owner module.
- Contract snapshot tests assert `depth_level` against nested `oneOf` discriminator nesting from `model_json_schema()` — schema depth drift fails CI before behavioral suites execute on new inner arms.

# Sub-Owner Split Versus Nested Payload

- Split to sub-owner when inner family has ≥3 arms, own migration fold history, or registry handlers invoked without outer context — `PaymentMethod` registry lookup must not require outer `TransactionClass` discriminant to select row.
- Keep nested payload when inner arms are only meaningful under one outer arm — `CardDetails` inside `Payment` only; inner union width ≤ outer context; inner exhaustiveness proves inside outer arm helper, not at top-level registry.
- Sub-owner export policy: domain modules import sub-`Member` alias for field typing; sub-registry `Mapping` publishes at composition root only — interior code pattern-matches sub-field value, never imports sub-registry lookup tables.
- Top-level `Member` registry rows hold nested registry references or projection callables into sub-registries — not flattened string keys spanning outer and inner vocabularies.
- Re-merge sub-owners only when vocabularies collapse to one `Kind` enum, migration folds fuse, and nested `match` blocks merge into one exhaustive fold without guard disambiguation — otherwise field-reference composition preserves independent version arms.

# Cross-Field Law Placement

- Sub-owner local invariants prove at sub-owner smart constructor or sub-field `model_validator(mode="after")` — method-local exhaustiveness stays inside sub-module `match`.
- Top-level cross-sub-owner invariants (`method` compatible with `rail`, `amount` currency matches `rail` jurisdiction) prove once at root `admit` on enclosing record — not duplicated in each sub-owner unless sub-owner is also used standalone at another boundary.
- Cross-field law never lives in registry row handlers keyed only on outer discriminant — rows delegate arm behavior; compatibility law runs before registry lookup accepts materialized `Member`.
- Pydantic v2.13+ hierarchical admission composes nested `Annotated[..., Field(discriminator=inner)]` inside outer member types, then outer `Annotated[..., Field(discriminator=outer)]` on enclosing field — validation errors nest under property paths (`pet.cat.black.black_name`) with independent fault tokens per routing level; inner `TypeAdapter` failure must not coerce to catch-all outer arm.
- Inner union isolated `TypeAdapter` validates sub-payload before outer admission when outer envelope is optional or polymorphic — failure is typed inner fault, not silent downgrade to `Extension` unless root documents capture tier.

# Cross-Field Guard Catalog

- Root smart constructor publishes frozen `CrossFieldGuardRow` catalog keyed by stable `guard_id: str` — each row names participating sub-owner fields, predicate hook, and fault constructor on violation.
- Endofold and registry row metadata reference `cross_field_guard` by `guard_id` only — compatibility law never duplicates inside row handlers; guard catalog is single source beside root `admit`.
- Guards execute at construction gate before registry lookup accepts materialized `Member` — row handlers assume cross-field law already discharged.
- Sub-owner standalone admission at alternate boundary invokes guard subset only — full enclosing-record guards run at top-level root `admit` once.

# Multi-Stage Registry Lookup Publish

- Stage 1 — root lookup on top-level `Member`: exhaustive `match` on outer discriminant; row may hold nested registry reference, not inline inner handler keyed by concatenated string.
- Stage 2 — field access on sub-owner value triggers sub-owner lookup with own exhaustive `match` — mirrors nested `match` structure; no skip from outer row to inner handler without narrowing sub-field type first.
- Stage 3+ — standardized multi-stage lookup published at root when product-of-sum exceeds two delegation layers — depth beyond two outer/inner stages is sub-owner split signal, not ad hoc per-route handler chains.
- Registry lookup entrypoints accept materialized `Member` or narrowed sub-field after outer `match` arm — never raw inner `tag` string from wire dict before smart constructor; `Kind` alone from untrusted ingress is rejected at root gate.
- Row delegation after `case Card(...) as card` passes `card` to `Callable[[Owner.Card], Out]` — erasing to `Callable[[Member], Out]` inside row when `kind` slot already fixes arm is a merge blocker.

# Projection Nesting And Wire Flattening

- Hierarchical domain to flat wire reverses nesting order: outer tag in serialization policy, inner tag inside nested object — adapter tables at family owner declare both mappings; per-call-site string assembly is rejected.
- Projection fold on top-level `Member` may collapse multiple domain variants to one wire view — collapse at projection, not by erasing inner discriminants in domain owner.
- Inner egress structs are projection targets from inner `Member` arms — wire structs do not flow backward into domain folds without declared read-path adapter; top-level egress composes outer projection calling inner projection per arm.
- msgspec hierarchical families nest tagged structs as field types — inner `tag_field` values never promote to outer tag strings; OpenAPI `oneOf` at outer level references outer discriminators; inner schema nests under property nodes.

# Cartesian Contract Sample Budget

- Sub-owner parity tests prove independently — top-level vocabulary parity does not subsume inner table parity; set difference on either `Kind` iteration or row keys fails CI before behavioral suites.
- Cartesian samples across sub-owners run only when root smart constructor proves cross-field law requiring combined arms — sample budget is outer-arm × inner-arm pairs implicated by cross-field rules, not full cross product of all theoretical combinations.
- Hierarchical hypothesis strategies build outer arm first, then inner arm conditioned on outer discriminant — conditioning mirrors nested `match`; independent random tags on inner and outer waste cycles on validation failures.
- Round-trip proof on hierarchical families includes per-outer-arm inner round-trip matrix — metamorphic samples live beside owner; polymorphic nested sub-owners add Cartesian rows only when cross-arm law requires combined proof.

# Version Evolution Per Sub-Owner Axis

- Sub-owner version literals evolve independently when migration folds are sub-owner-local — outer `schema_version` does not force inner version bump unless cross-field law binds them; document coupling in vocabulary table row metadata.
- Top-level migration fold composes sub-owner migrations on nested fields — `StoredEnvelope -> Member` runs sub-migrations inside outer arm mapping; obsolete inner arms stay in sub-owner migration module with `assert_never`, not in active domain `match`.
- Deprecation on inner arm routes through sub-owner adapter fold — outer arms remain until outer producers migrate; do not delete outer arm because inner vocabulary shrunk.
- Unknown stored inner tag at read boundary fails with `MigrationFault` scoped to sub-owner enum name — never maps to outer default arm or top-level `Extension` unless root documents cross-level capture policy.

# Composition Root Wiring For Hierarchical Families

- Root imports sub-owner aliases for adapter field typing and nested registry references — sub-owner modules stay dependency-minimal; root wires sub-registries without family modules importing domain records.
- One root `TypeAdapter[TopMember]` owns outer discriminator policy — inner adapters cache beside sub-owner import at root, not per-request allocation inside handlers.
- Root smart constructor `admit` proves cross-field law once — handler `match` on `Member` delegates inner behavior through sub-field pattern match or sub-registry reference from outer row.
- HTTP, CLI, queue carriers share one hierarchical ingress model family — route differences are carrier decode only; nested discriminator mappings read same vocabulary tables root exports.
- Import-cycle resolution for hierarchical families belongs at root — top-level family module exports outer union and field type references; sub-owner modules do not import top-level smart constructor.
- Free-threaded workers and PEP 734 subinterpreters receive hierarchical members only as encoded bytes plus pinned root decoder policy — nested `TypeAdapter` caches and frozen registry rows from parent import graph are not portable object references; worker boot runs vocabulary table hash parity and nested adapter identity check before publishing lookup entrypoints.

# Fold Algebra On Product-Sum Shapes

- Outer fold `Member -> Out` may delegate to inner fold on sub-field inside arm — delegation preserves totality: every outer arm maps; inner `match` inside arm is second exhaustive layer.
- `Block[Member]` over hierarchical members maps outer fold per element — elements carrying sub-owners trigger inner fold inside step when projection requires inner discrimination; no container-level concatenated tag shortcut.
- Endofold transitions on hierarchical `Member` declare legality on `(outer_from, outer_to)` and optionally `(inner_from, inner_to)` when transition table is product-keyed — sparse tables return `InvariantFault` for undeclared pairs.
- Lifting morphisms over hierarchical families maps outer discriminant and inner discriminant through adapter tables — partial lift returns `Result` with arm tagged unmappable; identity lift on isomorphic nesting signals missing collapse.

# Anti-Patterns

- Flat `"outer.inner.state"` registry keys merging sub-owner vocabularies into one string namespace — couples evolution axes and hides independent parity defects.
- Top-level `match` fusing outer and inner discriminants into one mega-union when semantic layers are independent — loses nested version evolution and inflates OpenAPI `oneOf` width.
- Inner decode via try-each-member loops without inner `tag_field` — attach explicit inner discriminator or msgspec tag policy per inner union.
- Cross-field compatibility checked inside per-arm registry handler after lookup — law belongs at construction gate before behavior dispatch.
- Sub-owner registry imported into domain modules for routing — registry executes at root and adapter seams only.
- Cartesian property tests enumerating full outer×inner product when cross-field law implicates subset only — wastes CI time and obscures failing law rows.
- Outer `Extension` arm capturing unknown inner tags without typed inner payload slot — extension becomes unvalidated nested dict pipeline.
- Flattening three-level nesting into two by dropping intermediate discriminant — breaks independent inner migration and confuses exhaustive witnesses.

# Proof Obligations And CI Gates

- Static: mypy `exhaustive-match` and ty all-error on every closed union layer — outer handler `match` and inner sub-owner `match` are independent proof surfaces; nested pattern guards do not discharge inner exhaustiveness.
- Table parity per owner: `Kind` iteration bijection with registry rows at each sub-owner and top-level owner — import root module graph fails when any table drifts.
- Contract: vocabulary parity across outer `Literal`, inner `Literal`, msgspec outer tag, msgspec inner tag, and wire strings per hierarchical row — one parametrized table per owner.
- Runtime: `beartype` on root hierarchical `admit` and multi-stage registry entrypoints — confirms live sub-field discriminants match arm selected by outer `match` before inner handlers execute.
- Mutation: Stryker on nested smart-constructor arms must kill mutants dropping inner arms, remapping inner tags to outer catch-all, or skipping cross-field law guard — kill depends on exhaustiveness and parity, not silent wrong nesting at runtime.
- Arch: domain folders import sub-`Member` aliases only — never sub-registry modules, inner `TypeAdapter` instances, or duplicate inner discriminant string constants from wire modules.

# TypeIs Chains And Nested Narrowing

- Author outer `TypeIs[Owner.Payment]` and inner `TypeIs[PaymentMethod.Card]` at respective family owners — outer predicate input is top `Member`; inner predicate input is sub-owner `Member` field extracted after outer `match` arm.
- Outer `TypeIs` true branch narrows to one arm; inner refinement starts on sub-field with sub-owner alias type — do not chain outer and inner predicates on same value binding without intermediate sub-field extraction.
- Complement narrowing after outer `TypeIs` false branch discharges remaining top-level union width — inner predicates are not substitutes for outer exhaustiveness; each closed layer owns independent `assert_never` witness.
- `TypeIs` does not lift through optional sub-fields — `Option[PaymentMethod]` requires `match` on `Some(method)` before inner predicate applies; `Nothing` arm proves absence law separately from inner union exhaustiveness.
- Generic hierarchical rows (`Row[T: OuterTag]`) carry literal proof on outer tag only — inner sub-owner field widens to full inner `Member` unless homogeneous inner literal collection is declared; per-element inner `match` restores proof at use site.
- Reject `TypeGuard` on hierarchical modules — complement-safe narrowing is mandatory when registry delegation chains across outer and inner `match` arms.

# Semi-Closed Hierarchy And Extension Triage

- Outer semi-closed with `Extension` arm and closed inner cores: unknown outer discriminant with capture policy materializes `Extension` at root gate — unknown inner keys on closed inner arms still fail `forbid_unknown_fields` / `extra="forbid"` even when outer policy admits extension.
- `extra_items=` on ingress `TypedDict` applies to body overlay declared at outer envelope — inner closed structs do not inherit outer `extra_items`; semi-closed layering is envelope-first, body-second, inner-union-third.
- `ReadOnly` extension slots on outer ingress mark keys observable but not domain-invariant — promotion of extension key to closed inner arm requires explicit adapter fold through sub-owner vocabulary table, not automatic merge on decode.
- `Extension` carrying nested `frozendict` preserves foreign inner shapes losslessly — interior `match` on top `Member` handles `Extension` explicitly; inner re-ingress from extension payload uses sub-owner `TypeAdapter` only at documented re-materialization boundary.
- Partial admission tiers (`experimental_allow_partial`) stop at validation exit — hierarchical partial shapes do not enter registry lookup until completion proof closes required fields on declared ingress owner for every discriminator level in scope.

# Rich Owner Graduation With Sub-Owners

- Graduate top-level record to rich owner when ≥3 outer arms need shared caches, registries, or fold-derived status — interior closed union and nested sub-owners stay as frozen fields inside owner; graduation deepens one outer type, not parallel nominal trees beside sub-owners.
- Sub-owner rich graduation is independent — `PaymentMethod` may graduate while `Transaction` remains frozen discriminated union; cross-owner shared caches belong at composition root wiring, not duplicated static state on both owners.
- Rich owner `__post_init__` on hierarchical replace routes through arm-aware methods per outer and inner arm — undifferentiated post-init keyed by concatenated tag strings is rejected.
- Owner egress method taking top `Member` composes inner projection calls per arm — `@computed_field` on outer variant delegates to sub-owner projection helpers as private arm utilities, not public per-sub-owner dump APIs.
- Smart constructor on rich hierarchical owner returns `Result[Owner, E]` — ingress still validates through nested Pydantic discriminated models before owner constructor; cross-field law runs in owner constructor after sub-fields materialize.

# Fault Routing Per Hierarchy Level

- Outer validation faults carry outer enum token and field path prefix — inner validation faults carry inner enum token scoped to nested path; fault constructors name owning vocabulary (`UnknownDiscriminant` includes enum class name).
- Root `Envelope` distillation preserves hierarchy in fault evidence slots when automation consumers need nested rejection context — truncation drops verbose nested payloads, not outer or inner discriminant identity tokens.
- `MigrationFault` on stored hierarchical rows names sub-owner when inner tag fails at read boundary — outer successful decode with inner migration failure does not coerce to outer `Extension` unless root documents nested capture tier.
- Registry lookup impossible states after full outer and inner `match` hit `assert_never` — multi-stage lookup does not second-guess static proof with default row fallthrough.
- Recoverable vs terminal faults on port-local `@tagged_union` stay file-internal — promotion to shared domain error family requires identical evidence shape across bounded contexts consuming both outer and inner rejection paths.

# OpenAPI And Schema Publication For Nested Unions

- Pydantic hierarchical ingress exports nested `oneOf` with independent discriminator mappings per level — outer `model_json_schema()` owns outer mapping; inner mapping nests under property schema nodes; hand-authored parallel tables diverging from `Annotated[..., Discriminator(...)]` declarations are merge blockers.
- OpenAPI consumers see ingress discriminators, not wire-only msgspec tags, when ingress and wire intentionally diverge — wire-only inner tags document in adapter rows beside vocabulary table, not duplicated as public schema enum entries.
- Schema version bumps on hierarchical families update outer mapping, inner mapping, both migration folds, and contract samples in one promotion unit — partial inner schema drift fails snapshot diff before handlers execute on new inner arms.
- `RootModel` scalar-tagged members in nested positions appear as const or enum arms inside parent property schema — wrapper-only nodes hiding inner discriminant are rejected.
- Codegen draft rows for nested discriminators merge through human review into sub-owner and top-level vocabulary tables — generated inner tags are not authoritative until parity tests pass at each owner.

# Persistence And Replay For Hierarchical Members

- Stored polymorphic rows encode outer discriminant at envelope level and inner discriminant inside nested payload blob — storage schema matches egress projection order, not ingress Pydantic layout when policy diverges.
- Read-path migration composes outer `StoredV1 -> Member` with inner sub-migrations per outer arm — domain modules after root handoff see only current top `Member` and current sub-`Member` aliases.
- Round-trip proof before cache write runs outer encode/decode plus inner round-trip per implicated arm pair from vocabulary table — Cartesian cache samples follow cross-field law budget, not full combinatorial explosion.
- Index keys for persisted hierarchical graphs project through total `Member -> Kind` at outer level and optional `SubMember -> SubKind` when query law requires inner discrimination — raw concatenated tag strings invented at handler sites are rejected.
- Trusted-replay `Decoder(TopMember)` pins beside outer and inner vocabulary tables — replay validates through same root smart constructor as live ingress; stored bytes predating inner version invoke inner migration fold before interior folds observe sub-field.

# Collapse Tests For Hierarchical Families

- Collapse flat registry keys spanning outer and inner vocabularies to two-stage lookup with exhaustive `match` at each stage after smart constructor.
- Collapse monolithic mega-union formed by fusing independent inner and outer discriminants when product-of-sum with sub-owner fields preserves evolution independence.
- Collapse per-route inner `TypeAdapter` forks to root-cached inner adapters beside sub-owner vocabulary table — hot paths do not allocate divergent discriminator policy per carrier.
- Collapse cross-field law duplicated in sub-owner constructors and root `admit` to single root construction gate — sub-owners prove local law only when also admitted standalone at another boundary.
- Collapse handler `match` blocks that re-encode inner tags as string concatenations to nested `match` on materialized sub-field values.
- Done when every hierarchical family declares depth level in vocabulary metadata, sub-owner tables pass independent parity, root `admit` proves cross-field law once, and CI fails on inner arm drift before handlers execute.
