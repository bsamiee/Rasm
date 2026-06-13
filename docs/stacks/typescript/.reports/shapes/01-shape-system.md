# shapes — the unified shape ownership system

# THE UNIFIED SHAPE OWNERSHIP SYSTEM — TypeScript + Effect 3.21.2

All API claims below verified against `node_modules/effect/dist/dts/` (Match terminals, `Data.TaggedEnum.WithGenerics`, `Brand.nominal/refined/all`, `Schema.Config(name, schema)` requiring `I extends string`, `Schema.TaggedRequest`, `disableValidation` on Class `new`, `optionalWith`, `fromBrand`, `suspend`, `Effect.Service` + `DefaultWithoutDependencies`, `Context.Reference`, `@effect/experimental/Machine` installed, `effect/Types` shipping `Simplify/Equals/EqualsWith/UnionToIntersection/Tags/ExtractTag/MergeRight/Mutable/DeepMutable/NoInfer` + variance witnesses).

## Governing principle

EVIDENCE_FUSION instantiated: every domain shape has exactly ONE owning declaration fusing static type, runtime evidence (validation/codec/equality), and nominal identity. All secondary artifacts — types, guards, codecs, arbitraries, JSON schemas, equivalences — derive from the owner. A hand-written type restating an owner's fields is a defect.

Three axes pick the owner, in order:
1. **Wire participation** (crosses any serialization boundary: HTTP, DB row, file, IPC, snapshot) → Schema family.
2. **In-process only** → Data family or Brand.
3. **Does-things vs is-things** → service ladder vs data rows. A "service" with no Effect-returning member is not a service.

---

## 1. THE SHAPE CHOOSER

| # | Shape class | Owning construct | Derives free | Construction law | Equality / identity | Wire | Deletes (rejected forms) |
|---|---|---|---|---|---|---|---|
| 1 | Branded primitive (tag only, no check) | `type UserId = string & Brand.Brand<"UserId">` + `Brand.nominal<UserId>()` | `.is` guard, `.option`, `.either`; type from the brand carrier | Constructor call (zero-cost) at the birth site | underlying `===`; nominal at type level | Never alone — upgrade to row 2 the moment it crosses a wire | type-fest `Tagged`, ts-essentials `Opaque`, hand-rolled `& {__brand}` intersections |
| 2 | Refined value (validated primitive) | Wire-visible: `Schema.String.pipe(Schema.filter(p), Schema.brand("Sku"))`. Born in-process first: `Brand.refined<A>(pred, onFail)` then lifted into decode via `Schema.fromBrand` — predicate exists ONCE | Codec, filter-respecting `Arbitrary`, JSONSchema, `.option/.either/.is` | Boundary: decode. Internal birth: `Constructor.option/.either` (never throwing call in domain) | Primitive equality; nominal brand | Yes, as branded primitive | Wrapper class around one primitive; `asserts` functions; parse-then-revalidate |
| 3 | Value object (multi-field, identity = value) | Wire-visible: `Schema.Class<Self>(id)(fields)`; in-process-only: `Data.Class` / `Data.case<T>()`. Validated VO adds `Schema.filter` on fields or the class; unit-less VO is the same construct without filters — NOT a different construct | Equal/Hash by value, opaque nominal type, codec + Arbitrary + Pretty + JSONSchema + `Schema.equivalence`; instance methods preserved; `.extend` | `make`/`new` for internal birth (both validate); decode at boundary; `disableValidation` exists only on `new`, only in measured hot loops, with why-comment | Structural value equality (free) | Schema form yes; Data form no | `interface` + factory fn + separate validate fn triples; Equivalence instances declared away from the shape |
| 4 | Vocabulary (closed key set + policy rows — the enum replacement AND the policy carrier) | Keys declared ONCE: `const Severity = ["low","med","high"] as const`; `type Severity = typeof Severity[number]`; wire guard DERIVED: `Schema.Literal(...Severity)`; policy rows: `as const satisfies Record<Severity, Row>` | Union type, runtime key guard, totality proof via `satisfies`, undefined-free indexing (finite-union-keyed Record is NOT widened under noUncheckedIndexedAccess) | Literal use; table lookup by `Severity`-typed key only | `===` on literals | `Schema.Literal` derived from the tuple | `enum`/`const enum` (banned by erasableSyntaxOnly anyway), `Map` lookups, `switch` over raw strings, class-per-variant. Payload-carrying "vocabulary" is not vocabulary — it is row 6/7 |
| 5 | Domain entity / aggregate (wire-visible, identity = id) | `Schema.Class<Self>(id)(fields)` with a row-2 branded id field | Codec, Equal/Hash, JSONSchema, Arbitrary, Pretty, equivalence, instance methods | Decode at boundary; `make` for internal birth; update = one `with`-family method producing `new Self({...})` | Equal.equals is the free VALUE-equality witness (snapshots, tests); aggregate IDENTITY is the branded id; HashMap/HashSet key entities by id, never by instance | Yes | The quadruple: `interface User` + `UserSchema` + `UserModel` + `toUser` mapper. This row is the reason the system exists |
| 6 | In-process closed ADT | `Data.taggedEnum<Definition>()`; generic families via `TaggedEnum.WithGenerics<N>` | Case constructors, `$is(tag)`, `$match` (exhaustive, curried), structural Equal/Hash per case | Generated case constructors ONLY | Structural per case | No — promote to row 7 by REPLACEMENT, never mirror | Discriminated-union interfaces + hand factories; class-per-case hierarchies; `instanceof` checks |
| 7 | Serializable closed ADT | Cases = `Schema.TaggedClass`; family = `Schema.Union(CaseA, CaseB, ...)` declared once beside the cases; union type extracted | Per-case + union codec, Equal/Hash, Arbitrary, JSONSchema; `_tag` injected | Case `make`/`new` internal; union decode at boundary | Structural per case | Yes | Parallel `Data.taggedEnum` mirror of the same family; ad hoc per-case codec assembly |
| 8a | State family — fold machine (DEFAULT) | State = row 6 (row 7 when snapshots persist); events = row 6 (row 10 when external); transition = ONE total fold: `Match.tagsExhaustive` over state nesting event dispatch, or a `satisfies Record<StateTag, Record<EventTag, ...>>` table | Compile-time exhaustiveness over states × events | Pure `(state, event) => state`; lifecycle owned by enclosing fiber/Ref | Structural | Snapshot via row 7 | Mutable `this.state` classes; Ref + ad hoc methods masquerading as machines; XState |
| 8b | State family — Machine actor | `@effect/experimental` `Machine.make/makeWith`; public procedures = `Schema.TaggedRequest`, private = lighter procedures; `Actor extends Subscribable<State>` | Mailbox serialization, request/reply typing, snapshot/restore, subscribability | `Machine.boot` inside a scoped layer | Actor identity; state snapshots structural | Procedures + snapshots serializable | Hand-rolled actor loops; Queue + match spaghetti |
| 9 | Error family — ONE system, two ADMISSION TIERS | Tier 1 (in-process): `Data.TaggedError("X")<fields>`. Tier 2 (boundary rail): `Schema.TaggedError<Self>()(tag, fields)`. Promotion = REPLACE the declaration (same name, same tag) the first time the error crosses a serialization boundary — both are YieldableError so zero call sites change | `_tag`, yield*-ability in `Effect.gen`, E-channel membership; tier 2 adds codec + HttpApiSchema status annotation hook | `new` at raise site; raised via `yield*` / `Effect.fail`; module error union extracted: `type RepoError = NotFound \| Conflict` | Structural | Tier 2 only | `extends Error` classes; thrown strings; error-code enums + lookup tables; BOTH tiers alive for one tag |
| 10 | Request / command | `Schema.TaggedRequest<Self>()(tag, { payload, success, failure })` — single substrate for RequestResolver batching, RPC, Machine procedures | Payload/success/failure codecs in one declaration; structural Equal/Hash drives request-cache deduplication for free | `new` at call site; decode at transport | Structural (load-bearing: it IS the dedup key) | Yes | Command interface + handler-arg type + response type triples; bespoke RPC envelopes |
| 11 | Service | Three-tier ladder. `Context.Reference<Self>()(id, {defaultValue})` when a universal default EXISTS and override is the exception (R never accumulates). `Context.Tag`/`TagClass` when the contract must be implementation-free at declaration: multi-impl ports, cross-package seams. `Effect.Service<Self>()(key, {effect\|scoped\|sync\|succeed, dependencies, accessors})` — the DEFAULT for one-canonical-impl services: tag + impl + `.Default` (+ `.DefaultWithoutDependencies` only when `dependencies` present) + `.use` + accessors in one declaration. One-line law: default exists → Reference; multiple/deferred impls → Tag; single canonical impl → Service | Tag identity, layers, accessors per tier | Layer composition only; never `new` | Reference identity by tag key | No | Constructor-injected classes; manual `Tag` + `Layer.succeed` pairs where `Effect.Service` suffices; pure-data records behind a Tag (that is config — row 12) |
| 12 | Config | `Config` primitives composed in the owning module; schema-validated values via `Schema.Config(name, schema)` (encoded side must be string — providers are stringly); secrets via `Config.redacted`; surfaced as `Context.Reference` default or consumed inside `Effect.Service` `effect:` | Validation, redaction, provider mapping, defaulting | Resolved ONLY at the composition root via ConfigProvider | n/a | env/file/args via provider | `process.env` reads in domain; dotenv objects passed as parameters; zod-parsed env bags |
| 13 | Collection / keyed state | Position law: wire/transfer + domain signatures = `ReadonlyArray` (`Schema.Array`/`Schema.NonEmptyArray`); keyed DOMAIN state = `HashMap`/`HashSet` (keys are Equal-bearing: branded ids, Data values); Stream/accumulation hot paths = `Chunk` (internal only, never in signatures); vocabulary-keyed total tables = `Record` + satisfies (row 4) | HashMap gives Equal-based lookup; Array gives codecs | n/a | Element-wise / Equal-based | Array forms only | JS `Map`/`Set` keyed by domain values (reference equality lies); `Chunk` in public signatures; arrays-as-sets with deep-compare `.find` |
| 14 | Recursive shape | `Schema.suspend` with explicit annotation. The required companion `interface Category { readonly children: ReadonlyArray<Category> }` is the SINGLE sanctioned interface-beside-schema in the doctrine: same name, declared adjacent, and it IS the extraction target — fields exist nowhere third. In-process recursion: `Data.taggedEnum` definitions recurse naturally | Codec etc. per row 5/7 | Per rows 5/7 | Structural | Yes | Any other interface duplication "justified like the recursive case" |
| 15 | Generic / parameterized family | In-process: `TaggedEnum.WithGenerics<N>` definition interface + `Data.taggedEnum<Def>()`. Wire: schema FACTORY function `const Paginated = <A,I,R>(item: Schema.Schema<A,I,R>) => ...`; every instantiation named once (`const PaginatedUser = Paginated(User)`) with extracted type | Instantiated codecs; generic `$match` via `TaggedEnum.Kind` | Per instantiation | Structural | Factory output yes | `any`-typed envelopes; per-instantiation hand-written shapes |
| 16 | Phantom / compile-time-only | Brands for primitives (units of measure are brands); unique-symbol TypeId + `effect/Types` variance witnesses (`Covariant<A>`, `Contravariant<A>`, `Invariant<A>`) for carriers | Nominal disambiguation; variance actually checked (phantom slots participate) | None — erased | n/a | No | Unused type parameters (TS silently drops them — no variance enforcement); comment-claimed variance |

`Schema.Struct` has no row because it owns nothing: it has no construction law, no Equal/Hash, no nominal identity. It is admitted only in encoded/wire POSITION — transform sources, HTTP payload schemas, `Schema.Class` field groups — and its `.Type` is never exported as a named domain type.

---

## 2. ADMISSION TOPOLOGY

**Single-decode law.** Untrusted data is decoded EXACTLY ONCE, at the outermost line where bytes/JSON/env/rows enter, via `Schema.decodeUnknown(Owner)`. Inside the perimeter: no re-validation, no defensive checks, no instanceof guards, no "double parse for safety". Possession of the type IS the evidence. `decodeUnknown` imports are physically confined to boundary modules (adapters); domain modules import owner classes, not decoders.

**Constructor evidence vs boundary decode.** `make`/`new` is for values BORN in-process from already-admitted parts — it runs filters (invariant preservation) but no codec transforms. `decode` is for values CROSSING from encoded space — transforms + filters. `encode` only at exit. The escape hatch `disableValidation` exists only on `new` (not `.make`), is admitted only in measured hot loops over already-proven data, and requires an adjacent why-comment.

**Option/absence law.** Domain shapes NEVER use `?:`. Absence is `Option<A>` in an always-present field; `Schema.optionalWith(s, { as: "Option" })` performs the wire↔domain conversion at decode, so the decoded artifact physically has no optional properties. `?:` is reserved for encoded/wire shapes and foreign API types. Under noUncheckedIndexedAccess, index access yields `T | undefined` exactly once at the boundary line and is immediately discharged by `Option.fromNullable`; defaults resolve at decode via `optionalWith({ default })` — domain never sees missing.

**Brand split.** Brand-at-decode (`Schema.brand`/`Schema.fromBrand`) when the value enters via wire: the decode IS the branding event. Brand-at-construction (`Brand.refined`/`Brand.nominal` call) when the value is computed in-process (derived ids, computed quantities). Both produce the SAME brand type — one declaration owns the brand symbol and predicate; `Schema.fromBrand` lifts the in-process constructor into decode so the refinement exists once.

---

## 3. TYPE DISCIPLINE (designed against the chooser)

**Extraction law.** Types come FROM owners, never beside them: `Schema.Schema.Type<typeof T>` / `Schema.Schema.Encoded<typeof T>` (same-name const+type pairing, namespace-free), the class type itself for Class owners, `Context.Tag.Service<typeof T>`, `Effect.Effect.Success/Error/Context`. `Parameters`/`ReturnType` extraction is rejected for domain signatures (write the type) and admitted only for foreign-library glue.

**Exhaustiveness ranking law** — densest admissible form wins, in order:
1. `as const satisfies Record<Tag, Handler>` table + direct index — when handlers are context-free rows (data or same-signature functions).
2. `$match` — owner-attached fold for `Data.taggedEnum` families the module owns.
3. `Match.typeTags<Union>()` / `Match.valueTags` — one-call exhaustive dispatch over foreign tagged unions.
4. `Match.type/value` pipelines ending in `exhaustive` — when guards/`whenOr`/partial discrimination are needed; `withReturnType<Ret>()` mandatory when arms must agree on a non-inferred type; `orElse` admitted only at true boundaries (logging unknowns), never to dodge a missing case.
5. `switch` + `never` assertion: forbidden in domain; admissible only inside perf-critical kernels with a why-comment.

**Variance law.** Every phantom/generic carrier declares `in`/`out` per parameter, matching Effect's own (`Effect<out A, out E, out R>`, `Schema<in out A, in out I, out R>`). Phantom slots use `effect/Types` variance witnesses so they participate in checking. An unannotated phantom parameter is a defect.

**Generic-constraint laws.** `const T` parameters on literal-preserving APIs (table builders, vocabulary functions). Native `NoInfer<A>` on parameters that must not drive inference (defaults, handlers keyed by an earlier parameter). Conditional checks are non-distributive by default — `[T] extends [U]`; bare distribution only when distribution is the point, stated. Constraints are minimal: constrain to the capability used.

**Single-brand law.** Effect `Brand` is the only branding mechanism in the codebase. type-fest `Tagged`, ts-essentials' encodings, ts-toolbelt `A.Type`, and hand-rolled tag intersections are forbidden — they create incompatible nominal universes.

**Helper admission tiers.** Tier 1: `effect/Types` (`Simplify`, `Equals`, `EqualsWith`, `UnionToIntersection`, `Tags`, `ExtractTag`, `MergeRight`, `Mutable`, `DeepMutable`, `NoInfer`, variance witnesses) — covers near-all residual need. Tier 2 (boundary only, never re-exported into domain): type-fest solely for delimiter-case string transforms and `Paths`/`Get` lenses when a foreign API demands them. Not admitted: ts-essentials, ts-toolbelt.

**Dispatch-table law.** Tables are `as const satisfies Record<K, V>` where `K` is a vocabulary union; indexing only by `K`-typed keys (undefined-free under noUncheckedIndexedAccess — string-typed keys ARE widened, so any stringly access must pass `Option.fromNullable` first). Adding a vocabulary member breaks every table at compile time; that is the feature, not a cost.

**Modal-arity law.** One function owns all modalities of an operation. Objective selection:
- Input modes form a closed tagged family → single signature over the family union, internal dispatch via the ranking law.
- Modes differ only by cardinality/optionality → single generic signature over the more general type (`Iterable<A>`); `getOne`/`getMany` proliferation forbidden.
- Return type genuinely depends on the input's literal type AND a union return would force narrowing at every call site → overloads on ONE implementation, 2–3 clauses max; data-first/data-last via Effect's `dual` pattern.
- Never sibling functions differing only in input shape.

---

## 4. FOUNDATION FILES — DECISION: 2 FILES

Two files, not three. Rationale: EVIDENCE_FUSION makes shape and type inseparable — every chooser row's "derives" column IS the extraction law instantiated, every vocabulary row IS the dispatch-table law instantiated. A standalone `types.md` would either restate chooser rows or be a stub, and with three files the authoring order is unsolvable (type discipline is designed AGAINST the chooser, so neither can be authored "under" the other). SHAPE_BUDGET applied to the doctrine itself: one concept — ownership — one file. The split that works is mechanics vs ownership:

**`language.md`** — "The compiler contract: posture, syntax admissibility, module law, and shape-agnostic type mechanics."
- `[POSTURE]` — flag table as law: strict, exactOptionalPropertyTypes, noUncheckedIndexedAccess, erasableSyntaxOnly, verbatimModuleSyntax, pure ESM; TS7-faithful (write to TS6 semantics).
- `[SYNTAX_ADMISSIBILITY]` — banned: enum/const enum, runtime namespace, parameter properties, `import =`/`export =`; ambient `declare` permitted. Card: the as-const-tuple/keyof-typeof MECHANICS only — ownership decision deferred to the chooser.
- `[MODULES]` — verbatimModuleSyntax `import type` law, same-name type+const pairing, export-surface law.
- `[TYPE_MECHANICS]` — card families: variance annotations + witnesses; `const` type params; `NoInfer`; `[T] extends [U]`; `satisfies` + `as const` semantics; index-access widening facts (finite-union-keyed Record vs string keys under noUncheckedIndexedAccess).
- `[HELPER_ADMISSION]` — effect/Types tier table; type-fest boundary residue; dead list.

**`shapes.md`** — "Every domain shape's single owning construct, its admission path, and the type discipline derived from ownership." Authored under `language.md`; uses its mechanics vocabulary without restating it.
- `[CHOOSER]` — the master table (Section 1 above), the file's index.
- `[ADMISSION]` — single-decode, make-vs-decode, Option/absence, brand split.
- `[VALUES]` — cards: branded primitive, refined value, value object, vocabulary.
- `[ENTITIES_AND_FAMILIES]` — entity/aggregate, in-process ADT, serializable ADT, recursive, generic families, phantom.
- `[STATE]` — fold machine vs Machine actor boundary card.
- `[ERRORS]` — one system, two tiers, promotion-by-replacement.
- `[REQUESTS]` — TaggedRequest substrate card.
- `[SERVICES]` — three-tier ladder + config shapes.
- `[COLLECTIONS]` — position law.
- `[TYPE_DISCIPLINE]` — extraction, exhaustiveness ranking, dispatch-table totality, single-brand, modal arity (all reference chooser rows; mechanics cited from language.md by name only).
- `[COLLISION_WALL]` — Section 5 below as enforcement cards.

Placement rule preventing restatement: language.md owns HOW constructs behave (compiler/type mechanics); shapes.md owns WHICH construct owns WHAT and WHEN (ownership, admission, construction, equality, wire). No law appears twice; shapes cites language mechanics by card name.

---

## 5. COLLISION AUDIT — structural preventions

1. **`Schema.Struct` as domain owner.** Prevention: Struct has no chooser row — no construction law, no equality regime, so it cannot satisfy any card's requirements. Enforcement rule: `Schema.Struct` may not be exported from domain modules and its `.Type` may never become a named exported type; it appears only in encoded position inside transforms, payload schemas, and Class field groups.
2. **Parallel interface beside a class/schema.** Prevention: extraction law makes any `interface` structurally overlapping an owner's fields a defect, with exactly one sanctioned exception (the recursive-schema companion, same name, adjacent). verbatimModuleSyntax makes type-only imports explicit, so violations are grep-detectable.
3. **Second brand system.** Prevention: single-brand law + dependency admission (type-fest's `Tagged` not importable into domain tiers). Effect brands hang off unique symbols, so a foreign brand fails to unify at first cross-use — the collision is a compile error, not a convention.
4. **`$match` beside `Match` for one family.** Prevention: ownership rule — owned `Data.taggedEnum` families dispatch via their generated `$match`/`$is`; `Match.*` is reserved for families the module does not own and non-tagged inputs. A family needing Match combinators signals a missing fold on the owner — add it there.
5. **Service shape used as data / data behind a Tag.** Prevention: tier-selection law requires every Tag/Service member to be effectful or a capability; a Tag whose service type has no Effect-returning member is a defect routed to `Context.Reference` (config) or a data row.
6. **Both error tiers alive for one tag.** Prevention: promotion-by-replacement — `_tag` strings are codebase-unique (one tag = one declaration), and because both tiers are YieldableError, promotion changes zero call sites, removing the only incentive to keep duplicates.
7. **decode/make misuse.** Prevention: `decodeUnknown` imports confined to boundary modules; domain imports owner classes only. Decoding trusted internal data or `new`-ing untrusted data is structurally visible as an import-location violation, not a judgment call.
8. **`?:` creep into domain.** Prevention: exactOptionalPropertyTypes + the Option/absence law; `optionalWith({ as: "Option" })` means the decoded artifact physically has no optional fields — domain code cannot even express wire-style absence without bypassing the owner.

---

Further considerations:

- **Promotion paths are the system's hidden API.** Three rows have explicit in-process→wire promotions (row 1→2, 6→7, error tier 1→2), all by REPLACEMENT with unchanged call sites. When authoring the cards, state the call-site-invariance guarantee explicitly — it is what makes "start cheap, promote later" safe and deletes the temptation to start everything as Schema "just in case" (ANTICIPATORY_COLLAPSE applied correctly: anticipate the family, not the codec).
- **`Schema.TaggedRequest` structural equality is load-bearing, not incidental** — it is the request-cache dedup key. The card must forbid non-deterministic payload fields (timestamps, random ids) in requests, or dedup silently breaks; such fields belong in the resolver's context, not the request.
- **The vocabulary row's single-source chain (`as const` tuple → extracted union → `Schema.Literal(...keys)` → satisfies-table) is the doctrine's smallest complete EVIDENCE_FUSION demonstration** — author it as the opening worked example in shapes.md, since every later card (dispatch tables, state-machine event tags, error tag unions) is the same chain at larger scale.
