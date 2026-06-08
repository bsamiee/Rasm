# Disjoint-Generic Collapse And Proof Stack

# Proof Triangle On Python 3.15

- Python `>=3.15` admits `@typing.disjoint_base` (PEP 800 Accepted) as the nominal seal for closed variant families; pair it with PEP 742 `TypeIs` predicates (Final, Python 3.13+ in `typing`) and `match` + `assert_never` witnesses so static narrowing, runtime `isinstance` proof, and exhaustiveness obligations form one triangle — no arm may omit any leg.
- PEP 800 defines two class properties: a class may *be* a disjoint base, and every nominal class must *have* a valid disjoint base. A class is a disjoint base when decorated with `@disjoint_base`, when it declares non-empty `__slots__`, or when it is `object`. Candidate resolution walks base classes; incompatible unrelated candidates make the class definition invalid — checkers emit diagnostics and treat `isinstance()` / `match` arms across unrelated seals as unreachable.
- Declare exactly one internal `_Seal` per family owner, decorate with `@disjoint_base`, and inherit every leaf variant only from `_Seal` — never from sibling variants or foreign nominal bases that would admit a common runtime subclass. `@disjoint_base` is disallowed on `Protocol` and `TypedDict`; permitted on nominal classes including `NamedTuple`.
- Author one `TypeIs[Owner.VariantX]` per arm at the family owner; predicate input type is the exported `Member` alias; return `True` iff value is exactly that arm and `False` for every other member — biconditional membership, not validity filtering. Narrowing applies to the first positional argument only; success narrows to the intersection of the prior type and `T`, failure to the complement within the prior type — same practical behavior as `isinstance()`.
- `TypeIs` complement narrowing in the `else` branch discharges remaining union width without `cast()` — repo policy bans `TypeGuard` on family modules because complement-safe narrowing is mandatory for fold algebra and registry delegation.
- `@disjoint_base` makes `isinstance(x, A)` and `case A():` branches unreachable when `x` is already narrowed to `B` whose disjoint base is unrelated to `A`'s — checker reachability aligns with CPython multiple-inheritance layout law; do not suppress unreachable warnings on sealed families.
- Non-empty `__slots__` on `_Seal` implies disjoint-base status even without explicit decorator — prefer explicit `@disjoint_base` on family seals so stubtest and cross-checker parity do not depend on heuristic slot inference alone.
- `assert_never` witnesses accept `Never`-typed remainders only after `match` arms cover every closed arm — witness lines are proof obligations excluded from coverage metrics; they are not behavioral branches to exercise in tests.

# PEP 695 Generic Tagged Families

- Collapse N tag-differing dataclass siblings into one generic owner with PEP 695 syntax (`class Row[T: Tag]:`) where `Tag` is a type parameter bounded by `Literal` union or verified `StrEnum` — shared frozen fields and `model_config` hoist once on the generic shell; arm-specific slots bind through `T` phantom or through discriminant `Literal` specialization at construction.
- Export closed membership as PEP 695 alias beside owner: `type Member = Row[TagA] | Row[TagB] | Row[TagC]` — consumers narrow against `Member`, not repeated inline unions or unconstrained `Row[Any]`. Runtime exposes declared type parameters on generic objects via `__type_params__`.
- PEP 695 constrained parameters use literal tuple syntax `def fold[T: (A, B, C)](value: T) -> Out` — union aliases cannot substitute for constraint tuples; when algorithm is parametric over whole `Member`, bound `T: Member` and `match` inside body instead of parallel constraint tuples that drift from vocabulary table.
- Generic type aliases `type Bag[T: Kind] = ...` scope type parameters to the alias right-hand side only — do not re-export bare `Bag` without type arguments; implied `Any` on alias use is a merge blocker for domain families. Type alias right-hand sides must not reference legacy module-level `TypeVar` bindings.
- Homogeneous tagged collections carry literal proof: `tuple[Row[Literal["card"]], ...]` or `Block[Member]` — heterogeneous bags require full `Member` union; `TypeIs` on elements does not lift to `TypeIs[list[A]]`; map narrowing with element-wise fold.
- Tagged generics own one discriminant axis — reject mixing generic tag parameter with unrelated nominal subclasses on the same field; if wire and domain tags diverge, vocabulary remap rows live at adapter seam, not second class hierarchy inside owner.
- Pydantic generic variant cases parametrize `BaseModel` once per family; `GenerateSchema` emits distinct compiled validators per concrete tag tuple — cache `TypeAdapter[Member]` at composition root, not per arm string.

# Collapse Decision Algebra And Polymorphic Owner

- Collapse triggers when ≥3 sibling shapes, factories, or dispatch arms share one bounded-context concept — count includes private helpers and route-local unions; pre-materialization `HANDLERS[kind_str]` counts as arms even when handlers live in one dict.
- Run collapse morphism test before keeping parallel types: if adapter would be identity up to field rename or enum tag alias, delete duplicate owner and merge vocabulary table rows — `Member` and `MemberDto` coexisting signals missing collapse.
- Polymorphic owner exports exactly one public admission surface `admit: Ingress -> Result[Member, E]` — per-arm `make_*` factories become private arm helpers invoked only inside smart-constructor `match`; registry lookup accepts `Member` after construction gate, never raw `Kind` from wire slices.
- Total projections `project: Member -> View` and contravariant smart constructors share one exhaustive `match` per direction — covariant egress and contravariant ingress are not interchangeable; missing projection arm is compile error under `exhaustive-match`, not runtime `None`.
- Endofold transitions `(from_kind, to_kind) -> Member` declare legal modality changes in frozen transition-row tables — absent row returns `InvariantFault`; version migration folds (`MemberV1 -> Member`) run once at ingress and are not endofold rows.
- `singledispatch` and `dict[str, Handler]` are boundary-only open extension — closed in-repo vocabularies collapse to discriminated owner before any registry materializes; finite version sets use `Map[Ver, Callable]` with literal-version keys, not plugin growth.
- Rich-owner graduation deepens one outer type when ≥3 arms need shared caches or fold-derived status — interior closed union stays nested inside owner; graduation does not spawn parallel nominal hierarchy beside `@tagged_union` or Pydantic discriminated union.

# Semi-Closed PEP 728 Extension Pipeline

- Python 3.15 `TypedDict` admits `closed=True` for ingress cores and `extra_items=T` for typed extension overlays (PEP 728) — never combine `closed` and `extra_items` on one payload; semi-closed families pair closed discriminant envelope with `extra_items` body capture when extension wire keys must survive losslessly.
- Ingress promotion flow: closed envelope `match` on `kind` → body payload selected by discriminant → unknown `kind` with policy `capture` materializes `Extension` variant carrying `discriminant: str` and `payload: frozendict[str, object]` or bounded overlay typed by `extra_items` — not open `dict[str, Any]` flowing into interior `match`.
- `ReadOnly` on extension slots (PEP 705) marks keys downstream may observe but not treat as domain-invariant — promotion to closed arm requires explicit adapter fold; extension arms participate in exhaustiveness with dedicated `case Extension(...)`.
- Closed variant ingress uses `forbid_unknown_fields` / Pydantic `extra="forbid"` — unknown keys on closed arms fail validation even when semi-closed policy admits `Extension`; extension is not partial-validation escape hatch.
- `NotRequired` ingress keys materialize to `Option` or absent sub-variants at construction gate — do not use `Member | None` when `None` conflates missing, null wire, and inapplicable modality; sentinel union members (`Absent`, `Withheld`) preserve law.

# Single Polymorphic Owner At Composition Root

- Composition root binds one `Member` alias, one vocabulary table, one frozen `Mapping[Kind, Row]`, and one cached `TypeAdapter[Member]` — interior modules import alias and enum tokens only; ingress models, msgspec wire structs, and adapters stay in boundary modules reachable from root import graph.
- Registry width equals union width at import — `Kind` iteration minus row keys must be empty in both directions; orphan rows and missing rows are import-time defects caught by table parity tests before behavioral suites run.
- Root admission pipeline is linear and typed: carrier decode → ingress validate → smart-constructor `match` → `Result[Member, BoundaryFault]` — no step widens `Member` to `object`, `dict[str, Any]`, or bare discriminant strings between handoffs.
- Callable `Discriminator` hooks execute inside root-cached adapters — cyclopts choices, OpenAPI enum export, and CLI surplus-token policy read the same vocabulary table; dual-maintained string tuples on route modules are merge blockers.
- Sub-owner families export field typing aliases to domain modules but publish registry lookup only at root — two-stage dispatch mirrors nested `match`; flat `"payment.card.settled"` tags couple evolution axes and are rejected unless wire contract mandates single flat vocabulary.
- Adding one arm is one root promotion unit: vocabulary row, registry row, smart-constructor arm, migration arm, OpenAPI mapping, hypothesis exemplar, and metamorphic round-trip sample — partial promotion leaves routes unrouted while interior static proof may still pass.

# Checker Matrix Static Proof And Proof Debt

- Primary static gates: mypy strict + `exhaustive-match`, ty all-error, pydantic mypy plugin — Ruff bans `TypeGuard`, `cast`, and legacy `Optional` on family modules; mutations introducing tag-check casts fail lint before mutation scoring.
- PEP 728 inheritance edges (`extra_items` narrowing, closed parent siblings) may lag on mypy — author law from Python 3.15 typing spec; track checker gaps as proof debt with minimal repro modules until mypy closes tracking; pyright may lead on `extra_items` assignability.
- PEP 800 `@disjoint_base` ships in `typing` on 3.15; backport in `typing_extensions>=4.15.0` — runtime sets `__disjoint_base__ = True` without enforcing layout; stubtest validates decorator placement on seal classes heuristic inference would miss.
- PEP 742 `TypeIs` ships in `typing` on 3.13; backport in `typing_extensions>=4.10.0` — input type must be assignable to narrowed type `T`; incompatible predicate contracts are static errors.
- `TypeIs` over generic containers remains checker-fragile — do not rely on `TypeIs[Sequence[A]]` lifting from element proofs; element-wise `map` fold with per-element `match` or `TypeIs` preserves totality without intersection-type stalls (mypy #20359 class of bugs).
- Pydantic `Discriminator` callable return must be `Literal | None` per arm — `None` means validation failure; impossible inputs inside callable end with `assert_never`, not silent catch-all routing to preferred variant.
- msgspec decode and Pydantic `TypeAdapter(Member)` must agree on tag strings per vocabulary table row — contract tests parametrize parity across `Literal`, `StrEnum` member, msgspec `tag`, and wire string for every arm.
- Hypothesis strategies draw from registry exemplars via `st.sampled_from` over materialized arms — `st.one_of` cardinality equals union width; `st.from_type(Member)` without registered exemplars wastes cycles on validation failures.
- Stryker on root smart-constructor `match` and registry lookup must kill mutants that drop arms, remap keys, or default unknown tags — kill ratio depends on exhaustiveness type-check and table parity, not silent wrong-arm behavior at runtime.

# PEP 800 Reachability Overloads And Final Seals

- PEP 800 reachability extends to `match` arms: `case B():` on value narrowed to `A` is unreachable when `A` and `B` carry unrelated `@disjoint_base` seals — enable checker unreachable warnings on family modules; do not model sealed variants as open nominal siblings.
- Disjoint-base knowledge supports overload overlap analysis: overlapping `@overload` parameter types are unsound when a runtime value could satisfy both unless return types align — variant arms used in overload banks must share no common subclass across unrelated seals.
- `@final` on leaf variants when family is fully closed prevents accidental subclass drift that would break biconditional `TypeIs` proofs — omit `@final` only when downstream specialization is real, checker-proven, and vocabulary table documents extension axis.
- Intersection narrowing after `TypeIs` success is `Member & VariantX` in checker semantics — predicate false branch narrows to complement; chaining two arm predicates in sequence must not leave impossible intersections without `assert_never` witness.
- Stub files for extension modules should mark C-extension layout classes with `@disjoint_base` when runtime forbids multiple disjoint inheritance — family seals in application code mirror stub posture for cross-checker parity between mypy, pyright, and ty.
- `NamedTuple` variant rows may carry `@disjoint_base` on an internal seal tuple base when PEP 800 nominal rules apply — prefer frozen dataclass or `@tagged_union` for domain families; `NamedTuple` seals are wire-projection targets, not rich-owner interiors.

# Literal Propagation And Enum Closure At Owner

- Every closed arm binds discriminant as single-value `Literal` on uniform field name — pattern match uses typed patterns on materialized members, not pre-extracted string locals that erase literal proof.
- `StrEnum` with `@enum.verify(EnumCheck.CONTINUOUS | EnumCheck.NAMED_FLAGS)` when vocabulary needs iteration, registry keys, or subset algebra — variant rows reference verified enum members; never route domain code on raw `.value` strings outside boundary adapters.
- Collapse isomorphic variants differing only by enum tag into one frozen row carrying `reason: Reason` — alias literal sets (`Literal["reptile", "lizard"]`) stay explicit on one arm when wire vocabulary intentionally aliases.
- Vocabulary table at owner declares one row per arm: canonical wire token, Pydantic `Literal`, msgspec `tag`, `StrEnum` member, persistence key — registry and adapters read same frozen table; domain modules import enum and `Literal` types only.
- Literal-indexed generic owners propagate tag proof through homogeneous collections — `tuple[Row[Literal["card"]], ...]` preserves arm proof; widening to `Block[Member]` drops literal index and requires per-element `match` or `TypeIs` again at use site.

# Proof-Stack Anti-Patterns

- Flat `if/elif` on `kind` strings inside domain modules — replace with `match` on materialized `Member` or collapse to registry lookup after construction gate.
- `typing.cast` after tag checks or post-filter element assumptions — replace with `TypeIs` predicates authored at family owner plus `@disjoint_base` seals.
- Smart unions without explicit `Discriminator` on Pydantic ingress — always attach field or callable discriminator; default union modes are not domain policy.
- Runtime-checkable `Protocol` unions simulating closed variant sets — `isinstance` across unrelated implementers cannot discharge exhaustiveness.
- msgspec YOLO union decode without `tag_field` — always attach explicit tags per arm; try-each-member loops on tagged wire are rejected.
- Registry lookup by string tag extracted from payload before smart constructor — bypasses construction gate and erases `TypeIs` proof chain.
- Single registry row handling multiple arms via internal `if` on kind — recreates switchboard inside row; one row per arm with arm-narrowed handler callable.
- Persisting ingress Pydantic models when wire and ingress shapes diverge — storage inherits validation-only fields and wrong discriminator policy on read.
- Hypothesis `st.from_type(Member)` without registered exemplars — generates invalid tags; use `st.sampled_from` over vocabulary-table materialized arms.
- OpenAPI discriminator tables maintained separately from `Annotated[..., Discriminator(...)]` — dual vocabulary sources diverge from runtime `TypeAdapter` routing.

# Contract Proof Gates And CI Closure

- Vocabulary-table contract tests assert parity across Pydantic `Literal`, msgspec `tag`, `StrEnum` member, and wire string for every arm — one parametrized table drives production adapters and test assertions.
- Per-arm round-trip samples construct canonical `Member`, project to wire, decode, assert equality on discriminant fields — nested sub-owners add Cartesian samples only when cross-field law requires combined proof.
- Discriminator callable tests feed synthetic dict slices per arm and assert routed member type — `None` return and impossible inputs exercise `assert_never` paths in fixtures, not production catch-alls.
- Import-linter rules flag domain modules importing root registry modules, interior handlers importing `TypeAdapter` from adapter files, and duplicate discriminant strings across ingress `Literal`, msgspec `tag`, and comment-maintained schema lists.
- OpenAPI schema snapshot tests diff polymorphic `oneOf` discriminator mappings from `model_json_schema()` against vocabulary table — drift fails CI before handlers execute on new arms.
- `beartype` on registry lookup and smart-constructor entrypoints validates live `Member` instances at boundary calls — runtime admission catches discriminant tampering static proof cannot see after morphism or container extraction.
- Arch rules enforce domain folders never import sibling domain folders — composition roots never import foreign registries from other bounded contexts; family modules stay dependency-minimal with wiring at root only.
