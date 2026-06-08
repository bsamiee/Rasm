# Vocabulary Sentinel And State Encoding Doctrine

# Vocabulary Doctrine Charter

- Python `>=3.15` treats vocabulary, absence, and state as typed shape material threading through admission, projection lattice, context seams, tagged unions, catalog rows, composition roots, and proof harnesses — not comment, string convention, or parallel spellings per layer or projection.
- One canonical token owner feeds construction, validation, dispatch, serialization, exhaustive projection, seam remap, registry rows, and CI proof; primitive chooser rows for `sentinel()`, `StrEnum`, and `enum.verify()` live on the language axis — this doctrine owns how those primitives compose inside data-shape owners.
- Composition roots bind closed vocabularies at startup — roots import one canonical token owner per closed set and never declare parallel token strings or ad hoc remap dicts beside the vocabulary module.
- A bounded vocabulary must participate in construction, validation, dispatch, serialization, and exhaustive projection without parallel spellings; wire tokens, static proof tokens, absence markers, and lifecycle states each own one canonical encoding surface.
- Closed vocabularies, absence encodings, and state enums evolve under version pressure, CI proof harnesses, and failure archaeology — token promotion binds enum owners, ingress discriminators, wire tags, remap tables, registry rows, and handler maps in one unit; proof harnesses attribute defects to the vocabulary or adapter owner before domain logic is suspect.

# Vocabulary Surfaces

- **Wire vocabulary** — runtime values crossing ingress, egress, CLI, config, and registry keys; encode with `enum.StrEnum` when the token set is closed and named; runtime tokens use explicit values and round-trip law; compile-time proof uses `Literal[...]` or `T: Literal[...]` paired with `TypeIs`/`match` at ingress — pick wire or proof as primary; derive the other only at a documented boundary adapter.
- **Static vocabulary** — compile-time proof over a closed token set inside one owner or generic parameter; encode with `typing.Literal[...]` or a `Literal`-bounded type parameter; use `LiteralString` for human-authored or policy-sensitive text that must not accept arbitrary runtime strings — not a replacement for closed domain enums.
- **Bit vocabulary** — composable capability or option masks; encode with `enum.Flag` when int comparison must not leak, or `enum.IntFlag` only when integer interop is the explicit contract; composable masks use verified `Flag`/`IntFlag`; omission uses module-global `sentinel("NAME")` compared with `is` and never on wire.
- **Absence vocabulary** — omission distinct from every domain value including `None`; encode with module-global `builtins.sentinel("NAME")` bound once and carried in the union type.
- **Lifecycle vocabulary** — ordered or terminal process states; encode as closed `StrEnum`, tagged discriminants, or `Literal`-bounded tags with one discriminant per FSM owner — reject magic values, bool pairs, and free strings.
- **Symbolic constants** — module anchors that are not enum members; use `typing.final` names, `enum.nonmember()` payloads on enum classes, or owner-nested constants — never duplicate the same token in two shapes.

# Vocabulary Owner Topology

- **Canonical vocabulary owner** — one module or nested class block declares each closed `StrEnum`, verified `Flag`, and module-global sentinel; domain records, ingress models, and wire structs import members — not re-declared token strings, freestanding `constants.py` bags, duplicate `Literal` bands on ingress and canonical for the same concept, or hand-maintained OpenAPI enum lists detached from enum iteration.
- **Bare wire enum** — token identity only; lives at the vocabulary owner when two or more projections or contexts must agree on the same wire token set.
- **Payload enum** — token plus immutable member metadata; lives with the dispatch or catalog owner that consumes sidecars; export only the enum class, not parallel dict projections of the same metadata.
- **Private sub-vocabulary** — nest inside the owning class when vocabulary is private (`class Owner: class Phase(StrEnum): ...`); nested `class Owner.Phase(StrEnum)` stays unexported when only the owning context constructs and dispatches on it; public wire vocabulary never hides inside a private nested enum that ingress must reach; export through the owner concept page, not barrel re-exports; public wire enums live at the module boundary the serializer owns — reject freestanding enum modules whose only job is name collection.
- **Shared atom export** — when two bounded contexts must agree on tokens without sharing domain records, export `StrEnum`/`NewType` from a vocabulary owner both contexts import; records and smart constructors stay inside each context.
- Internal enums back single-owner dispatch tables, materialization stages, and private lifecycle steps; vocabulary density belongs with the owner that constructs, validates, and projects it.
- Vocabulary owners must not import domain records that re-import the vocabulary module; keep vocabulary modules dependency-minimal (enum, sentinel, typing only); import-cycle resolution for vocabulary owners belongs at the root.

# StrEnum Wire Vocabulary

- Declare `class Token(StrEnum):` with explicit wire values equal to the wire token — do not rely on `auto()` lowercasing or manual `class Foo(str, Enum)`; prefer lowercase wire tokens unless an external contract mandates otherwise; keep member names uppercase and stable for code references.
- Every bare wire enum member satisfies round-trip law: `isinstance(member, str)`, `str(member) == member.value`, and `Enum(str(member)) is member`.
- `StrEnum` serializes through JSON and most string sinks as its value string without custom encoders; treat that value as the canonical egress token; string operations return plain `str`, decohere membership, and compare equal across enum classes when values match — re-admit through the owning enum constructor at every ingress boundary; egress emits `member.value` through JSON and most string sinks without custom encoders.
- String operations on `StrEnum` members return plain `str`, not enum members; downstream code must re-validate through the enum constructor or a typed ingress boundary before dispatch; never use cross-enum string equality as proof of domain membership — construct or narrow through the owning enum type.
- Do not subclass `str, Enum` manually; `StrEnum` plus `ReprEnum` already preserve `str()`/`repr()` semantics expected by wire and logging surfaces.

# Literal Static Vocabulary

- Use `Literal["<token-a>", "<token-b>"]` for static proof on dispatch tables, generic bounds, and callable discriminator returns — literals alone do not validate ingress strings.
- Bind generic parameters with `T: Literal[...]` when a parameterized model's behavior is token-indexed and the checker must prove exhaustiveness at compile time.
- Pair `Literal` discriminants with `TypeIs` predicates or `match` narrowing when runtime proof must mirror static proof.
- Promote to `StrEnum` at the first wire ingress or config surface; maintain both only when a boundary adapter explicitly translates proof and wire layers; reject checker plugins, string enums implemented as `str` subclasses without `Enum`, and dict keys typed as plain `str` when the key set is closed.
- Bind behavior with `T: Literal["<a>", "<b>"]` on a generic owner (`class Projection[T: Tag]`) so checker-proven specializations eliminate runtime tag reads in specialized call sites.
- Wire aliases resolve at ingress through `enum._add_value_alias_` on `StrEnum.__new__` or explicit `BeforeValidator` maps — aliases register on the member object; duplicate alias targets raise at class definition; admission still produces the canonical member; domain code branches on member identity, not alias spelling; deprecate by admitting legacy at ingress, emitting primary at egress, receipt-tagging alias use at the boundary.
- Literal alias sets on variant discriminants (`Literal["<a>", "<a-legacy>"]`) type the accepted ingress band; egress emits the primary token unless the consumer contract requires legacy spelling preservation.

# Flag And Bit Vocabulary

- `FlagBoundary.STRICT` rejects out-of-range combined values; name combination aliases explicitly and verify with `NAMED_FLAGS`; use `enum.show_flag_values()` and `enum.bin()` at diagnostics boundaries.
- Apply `@enum.verify(NAMED_FLAGS)` on flag enums whose combined aliases must not contain unnamed constituent bits.
- Use `enum.IntFlag` only when integer compatibility is an explicit wire or FFI contract; document that `IntFlag` members compare equal to integers and break enum isolation.
- On Python 3.15, `IntFlag` coerces integers into the masked bit domain and may lose original literal values on normalization — persist named tokens or explicit decompositions when integer literal reproduction matters; storing `int(member)` and replaying through `IntFlag(x)` can change the stored integer while preserving membership; negative and out-of-range integers normalize to combined flag states.
- Prefer `enum.auto()` for unnamed single-bit flags; name combination aliases explicitly (`RW = R | W`) and verify them when `NAMED_FLAGS` applies.
- Selected subsets use `frozenset[Token]` or verified `Flag` combinations — not `list[str]` of `.value` strings; flag subsets test with `subset <= universe` where both sides are `Flag` members — never compare bare integers to `Flag` in domain code.
- Foreign integer bitfields at context seams remap through named decomposition or verified `IntFlag` construction at the adapter; persisting remapped integers without named token policy repeats the IntFlag normalization trap from admission.
- Reject magic integer masks, bare `int` parameters for known flag sets, and `IntEnum` when the domain is really a flag composition.

# Enum Verification Invariants

- Stack `@enum.verify(UNIQUE | CONTINUOUS | NAMED_FLAGS)` at class definition time — failed constraints raise `ValueError` at import and block the entire owner module; `@enum.unique` remains acceptable for uniqueness-only enums.
- `EnumCheck.UNIQUE` — one name per value; required for wire enums whose tokens must be injective on the wire.
- `EnumCheck.CONTINUOUS` — no skipped integer values; apply on contiguous numeric enums used as ordered indices or array slots.
- `EnumCheck.NAMED_FLAGS` — every multi-bit alias names only declared flags; apply on `Flag`/`IntFlag` enums with combination members.
- Stack verification decorators with the enum class before export; prefer `verify(UNIQUE)` when additional constraints may join the same class later; reject post-hoc loops and runtime-only validation scattered at call sites.
- Stacked `@enum.verify` failures block entire vocabulary module import — order constraints at definition time; isolate experimental enums in staging modules until constraints pass.

# Sentinel Absence Encoding

- `from builtins import sentinel` with one module-global binding `NAME = sentinel("NAME")` where variable name matches sentinel name; carry in union type (`T | type(MISSING)`), compare with `is`, never `==`; each semantic absence value binds exactly once per module — repeated `sentinel("NAME")` calls produce distinct objects and break identity law.
- Pickle and copy preserve sentinel identity only when the sentinel is importable by module and attribute name; module-level binding is mandatory for persistence and multiprocessing; wire replay uses token admission, not sentinel reconstruction from bytes.
- Use sentinel for parameter defaults meaning caller omission or inherit-upstream; use `None` only when `None` is a valid domain value; use `expression.Option` for computed absence in result rails (`some`/`none`) — do not overload sentinel for both omission and operation failure; do not pass sentinel into `Option` — collapse sentinel arms to `none()` at the boundary conversion site.
- Reject `object()` sentinels, bool default flags that split one option shape, string tokens such as `"__inherit__"`, and numeric sentinels standing in for domain states.
- Queue shutdown, task cancellation, and stream lifecycle use the owning stdlib lifecycle API (`Queue.shutdown()`, `TaskGroup.cancel()`); do not inject sentinel payloads into domain models for those concerns.
- Three wire postures stay distinct: key absent (omission), key present with JSON `null` (explicit null), key present with domain null token (valid vocabulary value); `NotRequired[T]` on boundary payloads types optional keys; `Field(default=MISSING)` types omitted constructor arguments — do not conflate payload optional keys with sentinel-parameter defaults on the same field name without documenting the semantic fork on the adapter; sentinel identity never serializes on wire; egress maps sentinel-arm semantics to omission, optional key deletion, or an explicit domain null token; collapse wire `null` to domain absence only when the contract explicitly equates them; union algebra rejects `T | None | type(MISSING)` on one slot without documented three-way semantics — collapse to tagged variant or split ingress/parameter shapes.
- Parameter absence: `def fn(value: T | type(MISSING) = MISSING)` — compare with `is`, branch with `match`/`case` treating the sentinel arm before domain logic; smart constructors filter sentinel before entering `Result` rails; keyword-only sentinel defaults pair with `*` boundaries; `Unpack` payload contracts document which keys may be `NotRequired` vs which parameters use sentinel omission.
- Sentinel identity stops at canonical or parameter boundary; projection adapters translate sentinel-arm semantics to ingress optional keys, wire omission, or explicit null variants — projections never store sentinel objects in wire-typed fields; cross-context handoff of `Option`/`Result` rails collapses sentinel arms at the exporting adapter before foreign ingress sees the payload.

# Rich Enum Payload Owners

- When members carry structured metadata, one `StrEnum` subclass with custom `__new__` binds `_value_` and typed sidecars (`tuple[str, ...]`, `frozenset[str]`, `Literal[...]`, `bool`) — separate bare wire enums from payload enums; use `enum.member()` and `enum.nonmember()` for helpers that must not become wire tokens.
- Keep sidecar fields typed; never store untyped `object` bags on enum members; do not mix bare and rich members in one class.
- Reject parallel dict registries keyed by the same tokens, `@property` ladders reconstructing metadata from names, and string parsing that re-derives payloads at dispatch time.

# State Encoding And FSM Tables

- Encode finite state machines as explicit tables keyed by `(source_state: State, event: Event)` pairs where `State` and `Event` are closed `StrEnum` vocabularies owned by the machine — not free strings, not bool pairs; store rows in `frozendict[tuple[State, Event], Transition]` or immutable tuple of row records.
- Project state with `match`/`case` over the discriminant; terminate with `assert_never` on unreachable arms when static exhaustiveness needs a runtime witness; use `TypeIs` predicates at ingress when state crosses an untyped boundary and must narrow into one enum-backed member type before dispatch.
- Transition lookup is total over declared pairs or fails with typed `IllegalTransition` fault — no default transition arm that swallows unknown events; illegal-event faults carry `source_state`, `event`, and the finite set of legal events from that source as enum members, not `.value` strings; faults crossing context seams map to boundary error unions at the adapter — foreign contexts do not import the FSM transition table interior.
- Terminal states form `frozenset[State]`; the transition table omits outgoing rows for terminal sources unless the machine documents explicit reopen events.
- Transition effects return replacement owners or `Result[Owner, E]` — state change is not in-place mutation of a mutable status field; entry and exit evidence belong on the `Transition` row variant (`on_enter`, `on_exit` as optional slots or dedicated event subclasses), not as side-effect hooks fired from stringly `if state == ...` ladders.
- State history is immutable `tuple[State, ...]` carried on the owner snapshot when history is domain evidence; append via replacement — never a mutable list on a frozen owner; cross-context handoff passes the snapshot or a fold-derived receipt — not a parallel string list reconstructed at the seam.
- FSM `State` and `Event` enums export to receipt and evidence structs as enum-typed fields — fold summaries and fact streams store members, not `.value` strings, unless the persistence contract explicitly requires wire form in the fact row.
- Hierarchical machines compose as separate `(State, Event)` vocabulary pairs per machine owner; superordinate machines reference submachine terminal states through enum members imported from the submachine vocabulary owner — not duplicated token strings.
- Reject tag `if`/`elif` chains, default arms that swallow unknown states, pre-projection normalization that erases the discriminant before `match`, and mutually exclusive bool fields (`is_active`, `is_closed`).

# Construction Validation Dispatch Serialization

- **Construction** — enum constructors and sentinel-aware factories are the only admission path from raw strings, ints, or mapping values; ingress validators call `Enum(value)` and map `ValueError` to typed boundary errors.
- **Validation** — `@enum.verify` proves class-level invariants once; boundary `TypeAdapter` or model validators prove instance-level membership; predicates prove narrowed static types after validation.
- **Dispatch** — table-driven dispatch keys off enum members or literal tags; `singledispatch` and `match` receive already-narrowed vocabulary values, not raw strings.
- **Serialization** — egress emits `member.value` for `StrEnum`, flag value or named decomposition policy for `Flag`, and never emits sentinel objects on wire formats; absence on wire uses explicit optional fields or domain null tokens, not Python sentinel identity.
- **Exhaustive projection** — folds, status receipts, and rail summaries iterate `for member in Enum` or `match` every declared member; partial maps over subsets require an explicit default policy encoded as its own variant or sentinel arm.

# Boundary Admission Egress And Membership Proof

- Fixed ordering: decode bytes → normalize token shape → construct enum member → narrow with `TypeIs` → enter dispatch; normalize wire aliases — case fold, legacy spellings, numeric string forms — in `BeforeValidator` or `model_validator(mode="before")` hooks outputting canonical token string, then construct once — never dispatch on pre-validation strings.
- Ingress admits vocabulary only through enum constructors, `TypeAdapter`/`model_validate` coercion, or msgspec decode with typed `type=` — never through bare `str` assignment; `Token(raw)` succeeds iff `raw` matches exactly one member `.value`; map `ValueError` to typed boundary faults carrying rejected token and owning enum name.
- Cache one `TypeAdapter[Token]` per wire vocabulary owner at module scope; Pydantic `strict=True` rejects silent string→enum coercion only when the ingress carrier is already typed — wire strings still route through value lookup; msgspec struct fields typed as `StrEnum` decode in one pass with `msgspec.ValidationError` on unknown tokens before domain import.
- `Flag`/`IntFlag` admission applies the declared bit mask; out-of-range combined values fail under `FlagBoundary.STRICT` unless the enum documents a permissive boundary for FFI ingress.
- Egress emits `member.value` through JSON, msgpack, and `model_dump` unless `serialization_alias` or wrap serializer documents deliberate override; `Flag` egress chooses one policy per owner — combined integer, named decomposition, or canonical alias member — document on wire contract and do not mix policies; discriminant egress matches ingress vocabulary byte-for-byte on the stable token set; absent fields use `omit_defaults`, `exclude_none`, or explicit dump policy — not sentinel placeholders, not `null` unless the consumer contract requires null for omission.
- OpenAPI and JSON Schema generation project closed vocabularies as `enum` arrays of wire tokens; keep schema enum lists derived from `for m in Token` or `model_json_schema`, not hand-maintained parallel arrays; enum order follows definition order — when documentation inherits iteration order as semantic, add new members at documented positions or version the schema.
- Membership proof: string predicates over `str` are pre-admission filters only — enum construction is proof; `StrEnum` membership via `value in Token._value2member_map_` is insufficient alone — prefer `isinstance` on already-constructed members or constructor try/except at ingress; post-admission narrowing uses `TypeIs` with `is` for singleton enum members; `Literal` tag narrowing must be biconditional; `Flag` narrowing proves membership with `member in Flag` or masked `(value & mask) == value` only when the masked form is a declared member; compose one admission gate per boundary function — downstream receives narrowed enum members or tagged variants, not partially filtered strings.
- `validate_python` on an already-instantiated `StrEnum` member is a no-op membership proof; re-admitting strings through `validate_python("token")` vs `validate_python(Token.X)` exercises different coercion paths — keep live ingress on raw wire forms.

# Total Dispatch Maps And Registry

- Build total handler maps with `frozendict({member: row(member) for member in HandlerKey})` — comprehension must cover every member or static checking plus `assert_never` proves exhaustiveness; partial maps rejected unless explicit `DEFAULT` enum member documents fallback; silent `dict.get` defaults on closed vocabularies are stringly routing.
- Nested dispatch composes as lookup → inner total map keyed by secondary enum — each level receives already-narrowed members from the level above.
- Catalog and registry rows key by `StrEnum` members (`frozendict[ToolId, ToolRow]`); row constructors accept enum members, not `.value` strings — string keys in registries are boundary leaks.
- Rich payload enums on catalog rows (`Runner`, `Input`, `Language`, `Claim`) export as typed fields on msgspec structs; downstream binds reference enum members in dispatch signatures (`Callable[[Token], R]`).
- Registry and catalog tables iterate keys with `for tool_id in ToolId` when emitting diagnostics or coverage reports — label rows with member identity, projecting to `.value` only at logging boundaries that require wire form.
- Sub-vocabulary partitions assign every member to exactly one disjoint `frozenset[Token]` bucket at import; partition construction raises if any member is unassigned or double-assigned; verify coverage with set algebra over `frozenset(Token)`; cross-context catalogs import the partition owner, not recomputed `.value` string sets.
- Open extension catalogs register new enum members at definition time with `@enum.verify(UNIQUE)`; foreign contexts consume new members only after vocabulary owner export updates — not through ad hoc string keys in consumer dicts; plugin hosts register new members through documented owner modules imported before root finalizes `frozendict` handler maps — runtime `Enum` mutation after import remains rejected.
- Enum-indexed folds: `def project[S: StrEnum](member: S, table: frozendict[type[S], Row]) -> Row` requires a row for every member; callable surfaces preserve enum types in signatures — erased `Callable[[str], R]` after admission is a boundary leak; ParamSpec-preserving decorators re-export enum-typed parameters, not `str` wideners; handler maps keyed by one enum invoked with members from another enum class because values collide is cross-enum dispatch — ingress must target the owning enum type; total maps keyed by `type[S]` require call sites to pass literal enum types — proof harness includes monomorphized samples per exported enum, not one erased map test.

# Projection Lattice Vocabulary Sync

- Discriminant vocabulary declared once on canonical owner; ingress `Discriminator`, Pydantic `Literal` tags, msgspec `tag_field`, OpenAPI `enum`, settings `validation_alias`, and cyclopts choices derive from that row — lattice law: projections may diverge on field cardinality and aliases, not on discriminant token closure without adapter-owned remap table.
- **Ingress sync** — `Field(discriminator="kind")` values are exactly `Token.member.value` or alias targets registered on the owning enum; `BeforeValidator` normalization runs before discriminator selection so alias spellings still resolve to canonical members.
- **Wire sync** — msgspec struct `tag_field` names the same discriminant field as ingress; tag strings equal canonical wire tokens unless a documented seam remap table owns the translation; `rename` on wire fields changes encoded keys, not tag strings — conflating field rename with discriminant vocabulary causes silent tag drift in union decode paths.
- **Schema sync** — `model_json_schema()` and msgspec schema introspection emit enum arrays from `for m in Token`; hand-maintained schema enum lists are drift surfaces; enum order follows definition order — when documentation inherits iteration order as semantic, add new members at documented positions or version the schema.
- **Settings and CLI sync** — environment and CLI choices map to vocabulary members through `Field(validation_alias=...)` on a settings slice; cyclopts `Parameter` choices reference the same enum type, not a parallel string tuple.

# Tagged Union And Generic Discriminant Threading

- `@tagged_union` families and generic owners bind the discriminant axis to one vocabulary row — `class Shape[Tag: Literal["<a>", "<b>"]]` or a closed `StrEnum` field — not to free `str` tags on each variant arm.
- Callable discriminators, msgspec union `tag_field`, and pydantic discriminated unions must draw tokens from the same vocabulary as enum-backed tags and enumerate the same variant set — a fourth parallel string set or union member added on one surface without updating vocabulary owner and sibling surfaces is a lattice defect.
- Generic specialization eliminates runtime tag reads only when the type parameter bound is the same `Literal` band or enum type used at ingress; erasing to `str` at a projection edge voids compile-time exhaustiveness; `S: StrEnum` type parameters on fold/project functions require total maps keyed by `type[S]` — specializing to a subset enum without narrowing the table type is a static proof hole until call sites pass literal enum types.
- Nested tagged unions carry independent discriminant vocabularies per nesting level; inner tags do not flatten into outer enum members — each level owns its enum or literal band.

# Cross Context Seam Remap

- Anti-corruption adapters own `frozendict[ForeignToken, CanonicalToken]` remap tables keyed by foreign wire strings and valued by canonical enum members — domain constructors never absorb foreign spellings.
- Seam remap runs before canonical enum construction: foreign bytes → foreign token string → remap lookup → `CanonicalToken(canonical_value)` → materialize — not string equality on `.name` or hidden `.lower()` in domain code.
- Unmapped foreign tokens return tagged seam errors with finite canonical vocabulary, not partial instances with default members.
- Cardinality remap at seams translates foreign optional/null postures into canonical absence variants (`Option`, sentinel arms, explicit null variant) per documented wire contract — not implicit domain defaults.
- Version seams select remap tables by envelope `schema_version: Literal[...]`; idempotent replay applies the same remap and admission path; canonical materialization remains pure given normalized foreign ingress.

# Composition Root Assembly

- Composition roots import `StrEnum`, `Flag`, sentinel, and partition symbols from vocabulary owners only — root modules do not inline `Literal` bands, legacy alias maps, or `dict[str, ...]` registries for concepts already enum-owned; cache `TypeAdapter[Token]` and `Encoder`/`Decoder` pairs beside vocabulary import — hot paths do not allocate per-request adapters; roots wire vocabulary into materialization adapters, settings slices, and seam exports.
- Registry `Bind` rows pin enum-typed `Claim`, `Verb`, `params_type`, and handler surfaces — surplus CLI tokens and arity faults surface before enum admission runs on handler params.
- Vocabulary version envelopes on persistence and wire structs select remap tables through root codec submodule (`schema_version: Literal[...]`) — domain owners store current enum members; version choice stays at root, not inside interior folds.
- Process-start settings admit enum members and vocabulary-backed inherit tokens — cyclopts `Parameter` choices and `Field(validation_alias=...)` reference the same `StrEnum` the vocabulary owner exports, not parallel string tuples; settings slices that mean inherit upstream use sentinel or dedicated inherit tokens; root `project_*_config` resolves inherit and sentinel arms through one `frozendict[InheritToken, CanonicalToken]` table before first `materialize_*`; fixed order: load settings → resolve inherit/sentinel → project per-context boot config → admit boot enums through live ingress constructors; settings egress to domain boot records emits canonical `member.value` or enum-typed fields on frozen boot models — settings modules do not hand wire alias spellings to smart constructors without root resolver; boot paths do not bypass admission with `.value` string injection.
- Cross-axis seam handoffs route through named root or adapter admission/projection entries; interior folds receive enum members and tagged variants only; raw strings stop at root guards and boundary adapters:
- Foreign bytes → canonical owner: wire token strings via root `materialize_*` → `Token(raw)` on materialization pipeline axis.
- Settings inherit → boot enum: inherit or sentinel arm via root resolver → `CanonicalToken` on composition root axis.
- Canonical → wire bytes: enum members on frozen owner via `project_wire` emits `member.value` on projection lattice axis.
- Canonical → foreign context: enum members on snapshot via seam adapter remap → foreign wire tokens on vocabulary lattice axis.
- FSM transition receipt: `State`/`Event` members via fold appends enum-typed fact rows on immutable replacement axis.
- Handler dispatch: `HandlerKey` member via total `frozendict` lookup at root rail on admission mechanics axis.
- Fault → envelope: enum labels on `Fault` variants via root `envelope` distillation on composition root axis.
- Persistence replay: stored wire token via root decoder → `Token(stored)` on admission mechanics axis.
- Test fixture → domain: strategy-generated member via same `Token(...)` constructor as live ingress on composition root axis.
- Chained root pipelines compose as typed steps: decode → normalize → construct enum → materialize canonical → interior replace fold → project wire — no step widens vocabulary types to plain `str` between enumerated handoffs.
- `rail(bind)` closes settings, layer stack, and scope opener before handler execution — correlation attrs (`run_id`, `strict`, trace keys) are root projection fields, not duplicate domain enum slots unless the domain contract explicitly owns the same lifecycle vocabulary; layer stack slots (`checked`, `logged`, `traced`) log rejected tokens at adapter edge per owner policy (`.name` on developer surfaces, `.value` on wire-audit surfaces); strict policy faults reference enum-backed status vocabulary on success folds — empty or skip statuses map to `strict:` prefixed faults at root guard without re-encoding status as ad hoc strings inside handlers; spawn, retry, and govern layers attach only on effectful root entries — pure handlers on immutable owners do not add transport vocabulary or claim/verb overrides foreign to the `Bind` row.
- Root `Fault` and `Envelope` variants carry enum-typed `claim`, `verb`, `slot`, and `kind` from catalog vocabulary — synthetic transport prefixes (`strict:`, `validation:`) format inside root guards; structured fault interiors use members, not prefixed enum `.value` strings; `parse_fault` and operational fault paths materialize the same `Envelope` struct — fault union discriminants match success envelope tags so wire consumers see one closed token namespace per transport family; illegal-transition and seam rejection faults map to boundary error unions at adapter — root `envelope` does not serialize interior FSM transition tables or seam remap keys; cap truncation and distillation on `_emit` preserve enum fields on fault payloads — truncation drops verbose evidence slots, not vocabulary identity fields required for dispatch and replay.
- FSM machines owned by domain modules export `State` and `Event` through vocabulary owner the root imports — root history persistence stores wire tokens or enum names per contract, replaying through root decoders, not interior `members[0]` selection; terminal-state queries at root (`frozenset[State]`) import from machine vocabulary module — root guards gate egress or strict policy on terminal states comparing enum members with `is`, not string tags from CLI input; cross-context handoff passes immutable snapshots with `tuple[State, ...]` history or fold-derived receipts — root wire projection converts enum fields to wire form once at encode, not per-layer string duplication.
- Registries and handler maps publish as `frozendict` at import completion before worker threads import root symbols — post-import enum registry mutation races under parallel importers and invalidates contract table assumptions; root registry build runs partition coverage checks before handlers bind, failing import when a new enum arm lacks a `Bind` or handler row.
- Trace correlation keys on root layers must not share names with domain enum `.value` strings consumed by log parsers — namespace partition between transport attrs and domain vocabulary is a root wiring concern.
- `--strict` at root promotes policy faults on empty folds independent of whether handlers exhaust enum dispatch maps — strict policy and vocabulary totality are orthogonal proof obligations.

# Receipt Fact Stream Observability

- Mutation receipts and fold summaries use closed `StrEnum` slot/kind vocabularies aligned with catalog and dispatch enums — not a third string namespace; receipt tags duplicate dispatch enum tokens only when imported as enum members on receipt variants, not under new spellings.
- Root fold helpers iterate `for member in Slot` when emitting coverage and diagnostics — not grep for magic strings.

# Vocabulary Version Ladder And Promotion Units

- Wire envelopes, persistence rows, and cross-process handoffs carry `schema_version: Literal[...]` or closed `SchemaVersion(StrEnum)` beside vocabulary owners — canonical domain enums stay version-agnostic; historical token spellings resolve in boundary migration folds before enum construction; vocabulary migration is explicit, not inferred from partial key presence.
- Version literals graduate through a closed ladder declared next to the vocabulary owner — `"v1"`, `"v2"` as `StrEnum` or `Literal` union members, not free strings or opaque integer counters without documented semantics.
- Persist stable wire tokens per contract, not opaque integer bitfields, when downstream systems display or diff the literal token.
- Adding a vocabulary version arm requires simultaneous updates to `frozendict[LegacyToken, Token]` migration tables, ingress alias maps, OpenAPI enum arrays derived from `for m in Token`, registry partition rows, total handler maps, and contract proof samples — partial token promotion is a merge blocker.
- Deprecating enum members retires wire tokens only after migration fold proves zero read-path dependency in stores and fixtures — removed members stay in migration modules with `assert_never` witnesses on current-version `match`, not in active dispatch tables.
- Forward-compatible ingress may accept unknown version keys only when composition root documents tier policy — unknown versions fail at adapter materialization with `MigrationFault`, not silent default to current canonical vocabulary.
- Sentinel bindings are not versioned on wire — evolution changes sentinel semantics only through new variant arms or optional-key policy on projections; never rename or rebind module-global sentinels without a breaking migration that proves pickle and multiprocessing identity parity.
- When documentation and schema inherit iteration order as semantic, member insertion mid-enum versions wire contracts — promotion units must declare order-sensitive versus order-independent enum owners explicitly.

# Vocabulary Migration Fold Placement

- Read-path vocabulary migration lives in boundary adapter modules as total folds: stored wire token → `LegacyToken` lookup → `Token(canonical_value)` → materialize — one fold owner per vocabulary concept per version jump, not scattered `if version` in domain transforms.
- `frozendict[LegacyToken, Token]` tables sit beside the vocabulary owner or root codec submodule — domain owners store current enum members; legacy strings never persist inside interior folds after successful migration.
- Flag and `IntFlag` migrations remap through named decomposition or verified construction at the adapter — persisting remapped integers without named token policy repeats the IntFlag normalization trap; migration folds emit canonical flag members or documented wire integers per contract, not raw replay integers; persist `member.name` or named decomposition when literal integer reproduction matters.
- Alias retirement migrates ingress-only spellings through remap tables while egress emits primary tokens — alias rows remain in migration modules until fixture corpora and store scans prove zero alias reads.
- Write-path emits only current-version wire tokens — adapters do not dual-write obsolete spellings unless composition root documents transitional dual-emit with sunset date.
- Replay and cache rehydration pin encoder identity, store key, and schema version at composition root — trusted-replay posture does not bypass vocabulary migration when stored bytes predate current enum closure; pinned `Decoder(Member)` replay requires root codec module path stability — renaming root codec submodules breaks replay even when enum tokens are unchanged.
- Migration fixtures include out-of-range integers and negative literals for `IntFlag` owners — membership-correct replay that loses integer literal is an expected negative metamorphic outcome, not a silent pass.

# Simultaneous Vocabulary Update Contract

- Enum member additions propagate in one promotion unit: `@enum.verify` constraints, ingress `Discriminator` mapping, msgspec `tag_field` strings, OpenAPI enum arms, hypothesis strategy registries, seam remap tables, partition buckets, and root `Bind` rows — CI contract suite fails on any orphan or unassigned member.
- Enum member removals or renames require migration fold rows, receipt kind updates, and FSM transition table edits in the same change — partial removal leaving dispatch `.get` fallbacks is forbidden.
- `Literal` band changes on generic owners update static `ty`/`mypy` bounds, callable discriminator return types, and cyclopts choice metadata in one generative schema source when root schema export owns both — dual literal maintenance without adapter justification is drift.
- Fact-stream and receipt `StrEnum` kinds align with dispatch vocabulary — adding a kind without fold totality proof and handler map update is a binding defect caught by exhaustive fold tests.
- Partition tables rebuild at import — new enum members must land in exactly one bucket before merge; set-algebra proof runs at vocabulary owner import completion.
- Composition-root schema export owns one generative source for callable discriminator `Literal` returns and cyclopts `Parameter` choices referencing the same `StrEnum` — until landed, dual maintenance is tracked as proof debt with paired diff tests asserting literal set parity on every vocabulary change.

# Integration Proof Harness Architecture

- Proof harness layers stack orthogonally for vocabulary: static exhaustiveness (`ty`, `mypy`, `match` + `assert_never`), `@enum.verify` import-time constraints, import architecture (vocabulary modules dependency-minimal), contract tables (bijection, remap totality, partition coverage), metamorphic round-trip (canonical member → wire → decode → construct enum), and runtime boundary (`beartype` on adapters post-admission).
- Contract tables live beside vocabulary owners as parametrized pytest modules — one table row drives production remap assertions, OpenAPI enum parity, and CI bijection proofs; adapters import the table, tests do not duplicate token lists.
- Arch import rules enforce: vocabulary owners never import domain records, domain modules never construct wire tokens without root or adapter admission, composition roots never inline parallel `dict[str, ...]` registries for enum-owned concepts.
- Harness execution order: static checkers and `@enum.verify` import before contract tables before metamorphic properties before integration smoke — failures at static layer block expensive generative runs.
- Proof debt from checker gaps tracks on the language axis — source declarations stay spec-complete; harness suppressions at vocabulary seams are rejected.
- Integration proofs bind to enum classes and migration fold modules via `register_law`, not pytest file paths — refactoring test layout must not orphan vocabulary witnesses.

# Vocabulary Contract Table Obligations

- Each exported `StrEnum` maintains a contract row: member closure, wire `.value` set, optional alias map, ingress discriminator literals, and OpenAPI enum array — production admission and test factories read the same row.
- Flag enums document egress policy per owner — combined integer, named decomposition, or canonical alias — contract row pins the policy; mixed egress paths fail the table.
- Sentinel contract rows list module path, binding name, and permitted projection postures (omission, optional key, explicit null variant) — sentinels never appear as wire-serializable field types on egress structs.
- FSM contract rows enumerate `(State, Event)` transition pairs, terminal `frozenset[State]`, and illegal-event fault vocabulary — partial transition maps without documented default member fail static totality proof.
- Rich payload enum rows list member sidecar fields and types — explicit construction tests per member until stdlib enum payload verification exists; static typing alone does not prove sidecar legality.

# Metamorphic Token Proof Chains

- Full vocabulary metamorphic law when bijection holds: generate lawful enum member on canonical owner → `project_wire` emits `member.value` → decode → `Token(stored)` → assert member identity with `is` — failures implicate adapter binding before domain owners.
- Alias metamorphic law: ingress alias string → normalize → construct canonical member → egress emits primary token unless contract preserves alias spellings — alias arms never round-trip as domain branch keys.
- Cross-projection chain when ingress and wire intentionally diverge: canonical member → wire → decode → construct must succeed; reverse ingress fixture paths succeed only on documented test-only adapters, not production read paths.
- Seam metamorphic law: foreign fixture corpus → anti-corruption remap → `CanonicalToken(raw)` → local canonical → local wire → decode → local construct when foreign-local bijection is policy — otherwise one-way foreign-to-canonical proof only.
- Absence metamorphic law: sentinel-arm parameters collapse to wire omission or explicit null variant at projection → decode → parameter or variant arm — sentinel identity never survives wire bytes.
- FSM metamorphic law: legal `(state, event)` sequence → transition fold → history append → replay stored state tokens through root decoder → member identity matches — illegal pairs fail at transition lookup before state mutation.

# Property Harness And Enum Strategies

- Hypothesis strategies draw admissible tokens from `st.sampled_from(tuple(Token))` on vocabulary owners — not `st.text()` filtered by runtime validation, not arbitrary ASCII shrink toward non-member strings.
- Rejection-path strategies use explicit non-member strings from contract table drift rows — success-path strategies never hard-code wire tokens bypassing enum constructors unless testing boundary faults.
- Discriminant-conditioned strategies mirror `match` gate structure — outer enum member or literal tag sampled first, payload fields conditioned on narrowed arm; independent random tags across hierarchy levels are rejected.
- Flag strategies sample declared members and verified combinations only — raw integer fuzzing belongs in adapter rejection fixtures, not success-path domain tests.
- Sentinel strategies use the module-imported sentinel symbol from production roots — dynamically created sentinels break identity law and invalidate shrink conclusions.
- Shrinking preserves enum membership — custom composite builders rebuild valid members after shrink; invalid token mutations fail at construction gate, not as accepted counterexamples.
- FSM sequence strategies walk legal transition tables — illegal event injection is a dedicated negative module, not a shrink endpoint.

# Mutation And Regression Gates

- Stryker or equivalent targets adapter remap tables, `Token(raw)` admission wrappers, and total `frozendict[HandlerKey, Callable[..., R]]` maps — mutants introducing `.get` defaults on closed vocabularies or catch-all transition arms must fail exhaustiveness type-check or contract tests.
- Mutation on `match` dispatch arms requires kill ratio — mutants defaulting to swallow unknown enum members fail static witnesses before behavioral suites pass silently.
- Adding an enum member without registry row, partition assignment, handler map entry, OpenAPI enum arm, and contract table update fails CI at import — root registry build is the first gate, not post-merge discovery.
- Ruff policy bans `TypeGuard` widening and `cast` erasure on vocabulary dispatch modules — tag-check escapes fail lint before mutation scoring.
- Import-linter rules flag duplicate token strings across ingress `Literal`, domain `StrEnum`, comment-maintained schema lists, and receipt tags — violations route to vocabulary owner collapse before merge.
- beartype-decorated adapter entrypoints receiving enum members catch post-admission `.value` string injection that static vocabulary proof cannot see.

# OpenAPI Enum And Schema Drift Gates

- Pydantic and msgspec schema generation emit enum arrays from `for m in Token` or `model_json_schema()` — hand-maintained OpenAPI enum lists parallel to enum iteration are merge blockers.
- Schema diff tests compare generated snapshots on vocabulary changes — new member without schema arm fails CI before runtime discovery.
- Callable discriminator return literals must match canonical enum `.value` set or shared `Literal` band — fourth parallel string sets fail discriminator parity diff.
- Settings models fanning out to boot configs generate internal schema snapshots — env key renames fail schema diff when they diverge from vocabulary-backed `validation_alias` tables.
- Wire-only tags absent from public API contracts stay documented on adapter modules — dual publication without ingress mirror is drift unless seam remap owns the divergence.

# Failure Archaeology And Diagnostic Routing

- Failure attribution follows vocabulary-first routing: `ValueError` from `Token(raw)` at ingress → boundary adapter and vocabulary owner; `msgspec.ValidationError` on enum field decode → wire projection owner; illegal FSM transition → machine transition table owner; unmapped foreign token → seam remap table owner; `MigrationFault` → vocabulary migration fold owner.
- Duplicate-validation failures when the same unknown token message appears from Pydantic and enum constructor — harness runs duplicate-validation checklist per vocabulary; attribution targets secondary owner for removal.
- Cross-enum equality accidents — handler invoked with member from wrong enum class because values collide — attribution targets ingress admission targeting wrong owner type, not dispatch table interior.
- Alias leakage failures — domain `match` compares alias spelling — attribution targets ingress normalization that failed to canonicalize before storage.
- Sentinel on wire failures — persistence emits sentinel name strings — attribution targets egress projection policy, not domain constructor.
- IntFlag replay mismatch — stored integer differs after `IntFlag(x)` reconstruction — attribution targets persistence policy choosing integer over named token; not domain flag algebra.

# CI Regression Gates For Vocabulary Drift

- Vocabulary bijection generative tests prove every exported member appears in ingress discriminator schema, wire tag set, OpenAPI enum, and total handler map without orphan tokens.
- Remap totality tests assert every foreign token in fixture corpus maps through seam table or yields documented seam error — no silent default member.
- Partition coverage import hook proves buckets cover `frozenset(Token)` exactly once — CI fails when enum gains member without bucket assignment.
- Round-trip discriminant property tests materialize canonical tagged values, project wire, decode, rematerialize — discriminant matches canonical member identity on stable token set.
- Exhaustive dispatch static witness — `match`/`assert_never` over enum handlers plus `ty` exhaustiveness on literal-bounded generics live beside root handler maps keyed by enum — test modules import the same `frozendict[HandlerKey, Callable[..., R]]` symbol the root publishes, not a duplicated subset map.
- Rejection-path tests assert typed boundary faults carry rejected token string and owning enum name; success-path tests construct through `Token(raw)` or strategy members, matching live root ingress, not `members[0]` hacks.
- Root worker boot CI gate imports production vocabulary modules in parent and worker contexts, asserts identical `for m in Token` closure, sentinel `is` identity, and `@enum.verify` import success — pickle round-trip on representative members runs in the gate; failures attribute to non-importable sentinel paths or divergent importer graphs.
- OpenAPI and schema snapshot tests diff enum arrays against `for m in Token` — drift fails before handler execution.

# Operational Diagnostics Across Vocabulary Handoffs

- Structured fault payloads tag vocabulary layer — `ingress`, `wire`, `seam`, `migration`, `admission`, or `fsm` discriminator — not undifferentiated parse messages.
- Illegal-transition faults carry `source_state`, `event`, and finite legal event set as enum members — logging projects to `.name` on developer surfaces and `.value` on wire-audit surfaces per owner policy.
- Seam rejection faults carry foreign token and finite canonical vocabulary listing — remap gaps attribute to adapter table, not domain `match`.
- Trace spans on vocabulary adapters emit schema version, vocabulary owner module, and materialization stage — interior domain spans omit foreign codec identifiers; rejected tokens log with enum `.name` for developer surfaces and `.value` for wire-audit surfaces, consistent per owner policy.
- Alias-use receipts tag `AliasUsed` variants with canonical member identity — folds branch on members, not raw alias strings seen only at boundary.
- Coverage reports over total dispatch maps mark unassigned enum members at CI — emitted before handlers bind at root.

# Reject And Collapse Surfaces

- Reject: magic strings, ints, bool pairs, and `object()` markers; manual `class Foo(str, Enum)`; thin one-field enum wrappers; duplicate token registries; stringly routing over closed sets; `Optional[T]` when absence means omission; `IntEnum`/`IntFlag` chosen for convenience; runtime-only enum validation; auto-generated lowercase `StrEnum` on wire without external contract; tag mismatch without seam remap; enum sprawl across ingress, wire, domain, and fixture dict; alias leakage into domain `match`; sentinel in wire projection fields; literal–enum twin without adapter; domain smart constructor branching on `schema_version` or legacy wire spellings; migration mapping unknown stored tokens to default enum member; harness suppressions or cast escapes at vocabulary seams; cross-enum dispatch; inherit magic strings without sentinel or enum; fourth discriminator set parallel to canonical enum; root `REGISTRY` keys or `Bind.claim` typed as `str` where `Claim` enum exists; cyclopts choices duplicating enum `.value` tuple beside `StrEnum`; smart constructor resolving `"__inherit__"` or settings strings in domain; envelope and fault payloads carrying raw claim/verb strings; fixtures hard-coding wire tokens bypassing enum constructors; replay selecting admission table by partial key presence instead of explicit `schema_version`; hand-maintained OpenAPI enum arrays diverging from `for m in Token`; generating hypothesis samples via unfiltered text filtered by `Token(raw)`; rebinding or duplicating `sentinel("NAME")` in tests or submodules; post-import enum member injection; vocabulary bump updating wire struct without migration table or contract samples; skipping metamorphic proof for enum fields because same-process; proof debt from checker gaps resolved via harness suppressions instead of owner typing fixes.
- **Lattice drift signals** — wire `tag_field` string differs from pydantic discriminator literal for the same variant without seam remap documentation; identical token sets on ingress model, msgspec struct, domain enum, and test fixture dict; catalog row keyed by `str` where `ToolId` enum exists; persistence or API model fields typed `T | type(MISSING)` on egress structs; unconstrained parallel `Literal` and `StrEnum` for the same concept without boundary adapter justification; domain `match` arms compare against alias spellings admitted only at ingress; handler map keyed by one enum invoked with members from another enum class because values collide.
- Collapse: wire null conflation (split into explicit variant arms or documented wire postures); pre-dispatch string (admission gate plus total enum-keyed map); partial handler map (total comprehension or explicit default member); FSM swallow (finite table plus typed `IllegalTransition` fault); alias dict twin (`_add_value_alias_` or ingress `BeforeValidator` on owning enum); sentinel on wire (optional key policy or domain null token); magic values to `StrEnum`/`Flag`/`Literal`/`sentinel()`; twin vocabularies to one owner; per-projection enums and orphan members to vocabulary owner derivation and simultaneous promotion units; seam string remap in domain to adapter `frozendict` plus enum construction; receipt token sprawl to imported enum members on receipt variants; partition string bucket to `frozenset[Token]` keyed by `Bucket` enum; inherit magic string to vocabulary-backed inherit token on settings slice plus root `project_*_config` resolver; callable discriminator literals not present on canonical enum to shared enum or literal band with parity diff gate; root string registry to enum-typed rows and total handler maps at root build; CLI choice twin to `Parameter` referencing vocabulary owner type; fault string claim to enum fields on root `Fault`/`Envelope` variants; test string fixture to strategy iteration over vocabulary owner members and constructor rejection fixtures; version guess ingress to explicit `schema_version` literal on root codec envelope; orphan enum member to simultaneous promotion unit across contract table, registry, and OpenAPI emission; legacy map twin to `frozendict[LegacyToken, Token]` on migration fold owner plus ingress alias registration; partial dispatch mutant to total comprehension plus explicit default enum member when fallback is policy; sentinel version fork to new variant arm or wire omission policy without rebinding module sentinel; plugin runtime enum to import-time owner module registration before root `frozendict` publish; absence bool to sentinel default or explicit optional variant; tag ladder to `match` on enum or tagged variant; flag int to verified `Flag` or documented `IntFlag`; weak enum wrapper to bare `StrEnum` or rich `__new__` payload enum; domain smart constructor branching on legacy spellings to adapter migration fold before enum construction; hand-maintained OpenAPI enum arrays to generated schema with diff gate; migration unknown-token default to fail-closed `MigrationFault` or typed extension capture.
- Done when every closed token set has one vocabulary owner, every projection edge derives or remaps from that owner, admission is the sole gate for token membership, egress vocabulary is deterministic, FSM tables are total or explicitly faulting, seam tables are total or explicitly faulting, absence semantics stay separated across wire, parameter, and result layers, every root import of a closed token set resolves to one vocabulary owner, every registry and settings surface uses enum-typed fields, cross-axis handoffs route through named root or adapter entries, every vocabulary change lands as one promotion unit, contract tables drive adapters and tests without parallel token prose, metamorphic and mutation gates pass without suppressions, open-extension and schema-export obligations have documented owner protocols or proof-debt tracking, failure archaeology routes defects to vocabulary or adapter owners before domain logic, and CI gates fail on vocabulary drift before handlers execute.
