# Absence Axis Ownership Lattice

- Four orthogonal absence axes own distinct invariants — parameter omission (`builtins.sentinel`), wire key posture (`msgspec.UNSET` / Pydantic experimental `MISSING`), computed rail absence (`expression.Option`), and vocabulary-encoded policy (`StrEnum` inherit/null members); collapsing axes without a named adapter row is a lattice defect the composition root must reject at import.
- Axis ownership invariant: only the axis that materialized the absence may interpret it — interior domain folds receive collapsed canonical values (`T`, `Option[T]`, enum members), never raw `UNSET`, `MISSING`, or sentinel objects carried past the admitting decorator.
- Payload vs invariant split: wire/posture axes (`UNSET`, `MISSING`, key omission) own ingress codec invariants on boundary structs; parameter axis (`sentinel`) owns constructor arity invariants on callables; rail axis (`Option`) owns effect outcome invariants; vocabulary axis (`InheritToken`, `NullToken`) owns policy tokens that survive audit — domain frozen owners store only collapsed payload shapes.
- Type evidence per axis: PEP 661 `NAME = sentinel('NAME')` with checker name-binding law; `T | UnsetType` with `is UNSET`; `T | type(MISSING)` with `is MISSING`; `Option[T]` with `some`/`none` tags; inherit policy with `InheritToken` enum membership — mixing evidence forms on one slot requires a tagged variant owner, not union stacking.
- Construction location: sentinel defaults bind at callable/smart-constructor signature; `UNSET`/`MISSING` defaults bind on boundary `msgspec.Struct` / Pydantic ingress slices only; `Option` construction binds at operation exit; inherit tokens construct at settings admission before root `project_*_config` — domain frozen owners store none of the wire/parameter sentinels.
- Polymorphism without branching: `frozendict[AbsenceAxis, CollapseFn]` at the composition-root adapter owns axis→canonical projection; `match` on `AbsenceAxis` member identity selects collapse row — no `if value:` truthiness tests (PEP 661 sentinels are truthy; `UNSET` is falsey; the axes disagree on boolean coercion by design).
- `typing_extensions.sentinel` backports `builtins.sentinel` below `>=3.15`; on 3.15+ import `from builtins import sentinel` — vocabulary owner binds one module-global factory; contract row pins `(module, name)` import path so pickle, copy, and PEP 661 name-binding law stay stable across interpreter upgrades.
- `AbsenceAxis(StrEnum)` is a meta-vocabulary owned by the adapter module, not the wire serializer — axis members index collapse rows; wire `StrEnum` tokens and axis tokens never share a class.
- Composition-root `Bind` rows type handler params after collapse — `params_type` references frozen canonical models without `UnsetType`/`type(MISSING)` slots; surplus CLI tokens fault before axis carriers reach handlers.

# Vocabulary Family Chooser Algorithm

- Chooser row table `frozendict[ConceptKind, VocabularyFamily]` lives beside the canonical vocabulary owner — one row per closed concept; rows are data, not comments; import-time partition verifies every exported concept maps to exactly one family.
- **Closed wire token set** → bare `StrEnum` at serializer-owned module boundary; invariant owned by enum constructor + `@enum.verify(UNIQUE)`; validation at ingress `Token(raw)` once; dispatch keys `frozendict[Token, Row]`; polymorphism via `match`/`project[S: StrEnum]` — not string routing.
- **Compile-time exhaustiveness only** → `Literal` band or `T: Literal[...]` on generic owner; invariant owned by static checker + `assert_never`; runtime admission promotes to `StrEnum` at first wire touch via boundary adapter — dual maintenance requires documented lattice remap, not parallel dicts.
- **Composable capability mask** → `Flag` with `NAMED_FLAGS`; invariant owned by `FlagBoundary.STRICT`; IntFlag only when FFI contract row documents integer normalization trap; subset algebra uses `Flag` members, never bare `int`.
- **Caller parameter omission** → module-global PEP 661 sentinel; invariant owned by `is` identity law and pickle module path; never serializes; collapse to `Option` or explicit variant at exporting adapter.
- **Wire key absent vs null vs value** → `UnsetType`/`MISSING` on ingress struct field only; invariant owned by codec (`omit` on encode, default on decode); interior canonical uses explicit `NullToken` `StrEnum` member when null is domain vocabulary — not `None` overload when `None` is also valid payload.
- **TypedDict optional key band** → `NotRequired[T]` / `Required[T]` on boundary `TypedDict` when shape is mapping-first and msgspec struct is not the carrier; invariant owned by closed `TypedDict` (`extra_items=...` / PEP 728 `closed=True`); promote to `Struct`+`UNSET` at first binary codec — not parallel optional-key semantics on canonical owner.
- **Ordered/terminal process state** → closed `State`/`Event` `StrEnum` pair on FSM owner; invariant owned by total `frozendict[(State, Event), Transition]`; illegal pairs fault before mutation — bool pairs and free strings rejected.
- **Structured member metadata** → rich `StrEnum.__new__` payload enum on dispatch/catalog owner; invariant owned by sidecar field types; bare and rich members never share one class — registry exports enum, not parallel dict projections.
- **Effect-rail computed absence** → `Option[T]` / `Result[T, E]` at operation boundary only; invariant owned by rail fold; ingress carriers never use `Option` to mean key omission — collapse sentinel/`UNSET` to `none()` at the single rail conversion decorator.
- Chooser conflicts resolve by visibility audit: wire-auditable policy → `StrEnum` token; parameter-only omission → PEP 661 sentinel; tri-state JSON field → `UNSET`/`MISSING`; computed failure → `Option` — chooser table row documents audit surface.

# PEP 661 Sentinel Advanced Evidence

- Checker law: `VAR = sentinel('VAR')` requires matching names — mismatch is a static error, not a runtime footgun; encodes the one-binding-per-semantics invariant at definition time on the vocabulary owner.
- Truthiness trap: PEP 661 sentinels evaluate truthy unlike `None` and unlike `msgspec.UNSET` (falsey) — absence guards must use `is` / `is not` exclusively; boolean coercion is a mutation-risk defect routed to sentinel contract rows, not domain logic.
- Type expression forms: PEP 661 permits `T | MISSING` without `type()` wrapper when `MISSING` is a recognized sentinel binding; narrowing via `is` yields static singleton `MISSING` arm and `T` else-arm — pair with `TypeIs` predicates at module scope for reusable admission gates.
- `__or__`/`__ror__` on sentinel objects build `typing.Union` at runtime — generic factories composing `T | MISSING` programmatically must import the module-global binding, not call `sentinel()` per composition.
- Pickle identity: `__module__` is writable for unusual binding sites; contract row pins importable `(module, name)`.
- Keyword-only sentinel defaults pair with `*` boundaries — `def fn(*, value: T | type(MISSING) = MISSING)` keeps positional wire values from colliding with omission arms; `Unpack` payload contracts document per-key `NotRequired` vs per-parameter sentinel in one adapter row.
- Rejected PEP 661 alternatives remain collapse targets: `object()` markers, single-valued enum sentinels, `Ellipsis`, `Literal` sentinel typing, and bool default flags — chooser table maps each smell to the canonical family above.
- PEP 661 rejected `typing.Literal` for sentinel typing — static proof uses `TypeIs`+`is` narrowing instead; do not add `Literal["__missing__"]` bands parallel to sentinel bindings.
- C API `PySentinel_Check` enables extension modules to participate in axis collapse — foreign C sentinels must still map through adapter `frozendict` before domain ingress; C identity check is not domain admission.

# msgspec UNSET NODEFAULT And Union Discriminants

- `msgspec.NODEFAULT` marks struct fields with no default at schema definition (required slot); `msgspec.UNSET` marks tri-state wire fields where key absence ≠ explicit `null` — conflating them breaks decode/encode law; invariant owned by struct field specifier on wire owner.
- `T | None | UnsetType = UNSET` is the canonical wire tri-state annotation; encode omits `UNSET` fields; decode sets missing keys to `UNSET`; explicit JSON `null` yields `None` — `UNSET` arm → omitted key → decode → `is UNSET`, distinct from null arm.
- `array_like=True` and `omit_defaults` interact with `UNSET` — contract row documents egress policy per owner; array-mode structs that cannot omit fields must not host `UNSET` tri-state without an explicit adapter variant.
- Tagged union `tag_field` plus `StrEnum` discriminant decodes variant and enum vocabulary in one pass — absence of variant key is omission at envelope level, not `UNSET` inside a selected arm; nested unions carry independent discriminant vocabularies per level.
- `StrEnum` fields on `msgspec.Struct` raise `ValidationError` on unknown tokens before domain import — enum admission and `UNSET` admission compose in one decode; `@admit_boundary` collapses `UNSET` before handing interior frozen owners.
- `forbid_unknown_fields=True` on ingress structs pairs with closed vocabulary — unknown keys fail before sentinel/`UNSET` arms execute; seam remap runs on known foreign tokens only.
- `copy.replace` on frozen struct preserves `UNSET` identity — replacement folds that materialize canonical owners must match on `is UNSET`, not truthiness, before copying field values into domain records.
- `msgspec.field(default=UNSET)` is the field specifier for tri-state slots — `default_factory` on `UNSET` fields is a schema defect; construction validates at struct `__init__` generation time on wire owner.
- Decoder `type=` tuple unions admit per-arm `StrEnum` discriminants — axis collapse runs on the decoded struct product before tagged-arm dispatch; union decode never hands interior folds a widened `str` discriminant.

# Pydantic MISSING Implicit Unset And Validator Graph

- Experimental `pydantic.experimental.missing_sentinel.MISSING` mirrors PEP 661 wire-field semantics for rich-domain ingress — Pydantic names it `MISSING` not `UNSET` to avoid conflation with implicit `model_fields_set` unset tracking surfaced by `exclude_unset=True`; excluded from `model_dump` / JSON Schema — chooser row picks explicit `MISSING` when tri-state must survive validator graphs, implicit unset when only egress omission matters.
- Union constraint push-down applies `Field`/`Annotated` constraints to non-`MISSING` union members automatically — invariant owned by pydantic-core schema compiler; `@field_validator(mode="before")` normalizes wire aliases before `MISSING` arm comparison.
- `MISSING` identity validates with `is` in pydantic-core — equality-based literal schemas rejected; aligns with PEP 661 identity law once experimental import collapses to `builtins.sentinel`.
- Discriminated unions with `Field(discriminator="kind")` require every variant arm to admit the same vocabulary owner — `MISSING` on optional scalar fields inside an arm is arm-local tri-state, not a substitute for a missing discriminant key.
- Settings slices combining inherit sentinel defaults and `MISSING` optional fields route through root resolver `frozendict[InheritToken | type(MISSING), CanonicalValue]` — settings module owns raw axis values; boot frozen models own collapsed enums only.
- `model_construct` preserving `MISSING` identity is the trusted-replay admission path.
- `Annotated[T | MISSING, BeforeValidator(normalize)]` admits wire strings before `MISSING` arm selection — validator graph order is normalize → construct enum → compare `is MISSING`; domain `@model_validator` runs after collapse on canonical slice types.
- `TypeAdapter` cached beside vocabulary owner validates collapsed boot models — re-admitting `MISSING` through `validate_python` on canonical owners is a defect; live ingress stays on wire/raw settings forms only.

# TypeIs Predicates Generic Threading And Classes

- Module-scope predicates `is_missing(v: T | Missing) -> TypeIs[Missing]` and `is_unset(v: T | UnsetType) -> TypeIs[UnsetType]` own biconditional narrowing — body uses `is` only; checker narrows both branches unlike `TypeGuard`; predicates live on vocabulary owner, not inside domain folds.
- Enum admission predicate `is_member(v: str | S, owner: type[S]) -> TypeIs[S]` where `S: StrEnum` — construction in predicate via `owner(v)` maps `ValueError` to `False`; dispatch receives `S`, never raw `str`.
- Generic owners `class Shape[Tag: Literal[...], A: AbsenceAxis]` thread absence axis as type parameter only when collapse table is total over `A` — specializing `Tag` without matching `A` row is a static proof hole until call sites pass literal enum types for both parameters.
- Class-level nested `class Owner.Phase(StrEnum)` stays private when absence vocabulary is single-owner — public wire absence tokens never hide inside private nested enums ingress must reach; export through owner concept page.
- Callable surfaces exporting `Callable[..., TypeIs[Token]]` as callback protocols preserve narrowing through decorator stacks when `ParamSpec` and `__wrapped__` propagate — erased `bool` callbacks are boundary leaks.
- `enum.member()` predicates on vocabulary classes expose `is_missing`/`is_unset` as non-wire callables — `@enum.nonmember()` keeps predicates off `StrEnum` iteration and OpenAPI enum emission.
- Literal-bounded `Tag` parameters specialize collapse tables monomorphically — `project[Literal["a"], AbsenceAxis.OMIT]` eliminates runtime axis read at specialized sites only when both bounds match ingress vocabulary row.

# beartype Decorator Stack And AOP Collapse

- `@beartype` applies post-admission on adapter entrypoints where static proof ends — parameters typed as `Token` (enum member), not `str`; `Annotated[Token, Is[lambda m: type(m) is owner]]` catches cross-enum value collision static proof cannot see.
- beartype does not reinterpret `UNSET`/`MISSING`/`sentinel` semantics — axis collapse runs in `@admit_boundary` inner kernel before `@beartype` outer wrap; stack order: synthesize struct → axis collapse → enum construct → `@beartype` enforce narrowed types.
- `annotationlib.get_annotations(..., include_extras=True)` on admission decorators preserves `UnsetType`, `Annotated[BeforeValidator]`, and `Field` metadata under Python 3.15 `__annotate__` deferral — factories that strip extras collapse admitted shape to bare `T` and void absence evidence.
- `@validate_call` on settings loaders admits enum members and sentinel defaults at CLI/env boundary — signature preserves `T | type(MISSING) = MISSING`; cyclopts `Parameter` choices reference `StrEnum` owner, not parallel string tuples.
- ParamSpec-preserving `@admit_vocab` decorator tables keyed by `HandlerKey` enum re-export enum-typed parameters through `__wrapped__` — widening to `str` after admission.
- `@singledispatch` registers on collapsed canonical types, not axis carriers — register `handle(token: Token)` after collapse, not `handle(raw: str | UnsetType)`; dispatch graph receives one narrowed vocabulary per level.
- `BeartypeConf` composed at module `[CONSTANTS]` applies uniform post-admission policy — conf table rows pin `is_color=False` O(1) checking on enum-typed handler params; vale `Is` predicates attach only for cross-enum collision guards.
- `@dataclass_transform` does not apply to msgspec `Struct` — absence field specifiers stay `msgspec.field`; stacking marker-only decorators on wire owners is rejected.

# StrEnum Inherit Tagged Variants And Structural Ports

- When inherit/upstream-null policy is wire-visible or audit-logged, promote to `InheritToken(StrEnum)` or explicit `NullToken` variant arm — sentinel remains for parameter-only omission invisible on wire; chooser row documents visibility requirement; root resolver table maps inherit members to canonical values in one fold.
- Tagged union families encode absence as discriminant variants (`Present[T] | Absent | ExplicitNull`) when three-way semantics must survive interior `match` — flat `T | None | type(MISSING)` on one field is rejected; variant owner owns the invariant, not the scalar slot.
- Rich payload enums carry absence metadata (`frozenset[str]` legal overrides, `Literal` bands) on catalog rows — `frozendict[Token, Row]` reads sidecars; `@enum.nonmember()` helpers stay off wire token set.
- FSM `State` enums export terminal `frozenset[State]` — absence of outgoing transitions is table omission, not sentinel state members; illegal transition faults carry enum-typed evidence for observability spans.
- Protocol `AbsenceProjector` on adapter modules declares `collapse_parameter`, `collapse_wire_field`, `collapse_option` — composition root binds concrete projector per context; interior modules depend on protocol, not msgspec/Pydantic sentinel types; structural port enables polymorphic adapter families without rat-nesting helper modules.
- Hierarchical tagged unions keep absence vocabulary per nesting level — outer `EnvelopeKind` and inner `PayloadKind` are independent `StrEnum` owners; flattening inner absence tokens into outer enum members is a lattice defect caught by discriminant parity diff.
- Catalog `frozendict[ToolId, ToolRow]` rows store enum-typed fields only after collapse — row constructors reject `UnsetType` carriers; registry build at import runs axis partition before handler `Bind` publish.

# Projection Lattice Absence Sync

- OpenAPI/JSON Schema for tri-state wire fields emits `anyOf` over value schema and explicit `null` only — `UNSET`/`MISSING` arms never appear in public schema; omission is absence of key, not a schema enum value; lattice law: projections diverge on key cardinality, not on tri-state interior semantics.
- msgspec schema introspection and Pydantic `model_json_schema()` derive optional-key policy from the same contract row that pins `UNSET`/`MISSING` — hand-maintained `required[]` arrays parallel to struct fields are drift surfaces.
- Egress `omit_defaults` on msgspec and `model_dump(exclude_none=True)` on Pydantic compose with axis collapse — domain frozen owners emit canonical `member.value` or explicit `NullToken`; adapter chooses omit-vs-null, not interior smart constructors.
- CLI/settings projection maps env strings to `InheritToken` or sentinel arms before enum construction — cyclopts choices enumerate `StrEnum` members only; inherit resolution runs in root `project_*_config`, not handler bodies.
- Persistence replay selects root decoder by `schema_version` literal — stored bytes predating tri-state policy replay through migration fold, then axis collapse, then enum construct; partial-key version guess is rejected.
- Seam remap `frozendict[ForeignPosture, AbsenceAxis]` precedes canonical collapse — foreign JSON `null` vs key omission vs sentinel string tokens map to axis members before `StrEnum` construction; unmapped postures yield tagged seam faults with finite axis vocabulary.
- Callable discriminator `Literal` returns on schema export must match `StrEnum.value` set or share one generative row — fourth parallel absence spelling in OpenAPI `enum` arrays fails discriminator parity gate.

# Frozen Replacement And Rail Boundary Conversion

- Frozen replacement law for absence carriers — append state history and receipt rows only after collapse; wire structs may carry `UNSET`/`MISSING` across one adapter hop; canonical snapshots never persist axis objects in frozen fields.
- Immutable domain owners accept only collapsed slots — smart constructors filter sentinel with `is` before `Result` rails; `Option` arms materialize at operation boundary, not as constructor defaults on frozen records.
- `dataclasses.replace` / `copy.replace` on wire structs preserves `UNSET`/`MISSING` identity — replacement into canonical owners runs through `frozendict[AbsenceAxis, CollapseFn]` per field, not field-by-field `if`.
- Sentinel-to-`Option` collapse is a single boundary conversion site — `match` on `(is_missing(v), is_unset(v))` via `TypeIs` predicates routes to `none()`; never nest sentinel inside `Option` carriers.
- `Result` error channels carry enum-typed fault vocabulary — absence of success payload is not a sentinel on the success channel; transport faults use root `Envelope` discriminants aligned with catalog `Claim`/`Verb` enums.
- Interior folds keyed by `Token` receive members after collapse — fold signatures erase absence axes; widening fold inputs to `T | UnsetType` is a composition-root leak.
- Receipt/fact-stream `StrEnum` kinds align with post-collapse dispatch enums — mutation receipts never record `UNSET`/`MISSING`/sentinel names; fold appends enum members after axis collapse projects slot vocabulary.

# Polymorphic Collapse Tables Proof And Lattice Closure

- `AbsenceAxis` itself is a closed `StrEnum` vocabulary with `@enum.verify(UNIQUE)` — axis tokens are not wire tokens; partition table assigns each concept's chooser outcome to exactly one axis; import fails on unassigned concepts.
- Total `frozendict[AbsenceAxis, Callable[[object], Collapsed]]` at adapter owner replaces axis ladders — comprehension covers every `AbsenceAxis` enum member; `assert_never` on default arm.
- Cross-library translation law: `UNSET → wire omit`, `MISSING → wire omit`, `sentinel → param omitted / inherit resolver`, `Option.none → rail terminus` — each arrow is one adapter row; interior never reverses translation without root codec `schema_version` envelope.
- Cardinality remap at seams pairs foreign optional/null postures with axis enum members before collapse — `frozendict[ForeignPosture, AbsenceAxis]` runs before `frozendict[AbsenceAxis, Collapsed]`; two-stage fold preserves pure interior folds.
- PEP 661 truthy sentinel through parameter default → collapse → must not survive wire round-trip; `UNSET`/`null`/`value` tri-state chain per field; inherit `StrEnum` → resolver → boot enum `is` identity; predicate+constructor chain rejects cross-axis contamination.
- Failure archaeology routes axis violations to adapter owner: truthiness guard on PEP 661 sentinel → sentinel contract row; `UNSET` conflated with default → wire struct owner; `exclude_unset` conflated with `MISSING` → Pydantic ingress owner; cross-axis union on domain field → variant owner collapse.
- Arch import rules: domain modules never import `UnsetType`/`MISSING`/`sentinel` except through adapter ports; vocabulary owners declare axis enums and collapse tables; composition roots bind projector implementations.
- OpenAPI enum arrays for wire `StrEnum` never list axis tokens or sentinel names; dual namespace publication is merge blocker.
- Axis change updates chooser row, collapse `frozendict`, contract columns, and `Bind.params_type` — partial axis promotion without handler signature update fails import-time registry build.
