# [TYPESCRIPT_SHAPES]

Every concept takes exactly one runtime authority, and five discriminants select it before any field is written: admission (raw material crossing a trust boundary), identity regime (structural, tag, brand, or reference), variant arity (one shape or N alternatives), payload timing (variant data fixed at declaration or constructed per occurrence), and openness (closed vocabulary or foreign extension). One Schema is the single source of truth for a shape — decode, encode, brand, filter, and every projection derive from that one declaration, never from parallel schemas. Every misplaced shape traces to one mis-answered discriminant or to a second authority minted beside the first.

The named defect class this page refuses: loose-schema spam (a fresh `Schema.Struct` per shape), branded-type nonsense (a standalone `type Id = string & Brand` or a module-level `const _Id = Schema.brand(...)` export), const spam (N parallel one-use `Schema.Literal`/`Schema.brand` consts before a class), parallel projection structs (`TaskInsert`/`TaskUpdate` redeclaring what the model derives), and the standalone `type`/`interface` that mirrors a runtime shape.

## [1]-[OWNER_CHOOSER]

When a concept matches several signatures, the most specific row wins.

| [INDEX] | [CONCEPT_SIGNATURE]                         | [OWNER]                                   | [IDENTITY] |
| :-----: | :------------------------------------------ | :---------------------------------------- | :--------- |
|   [1]   | invariant-bearing scalar                    | `Schema.brand` field modifier             | brand      |
|   [2]   | N fields, one concept, decode authority     | `Schema.Class`                            | structural |
|   [3]   | persisted entity with SQL projections       | `Model.Class`                             | structural |
|   [4]   | bounded vocabulary, behavior rows           | `as const satisfies Record<...>`          | key        |
|   [5]   | closed alternatives, per-occurrence payload | `Data.TaggedEnum`                         | tag        |
|   [6]   | cross-cutting boundary failure              | `Data.TaggedError` / `Schema.TaggedError` | tag        |
|   [7]   | wire-to-domain bidirectional projection     | `Schema.transform`                        | structural |
|   [8]   | interior product, no decode, no boundary    | inferred `as const` plain object          | structural |
|   [9]   | foreign wire enum or ordinal at the seam    | `Schema.Literal` / `Schema.Enums` at edge | ordinal    |
|  [10]   | foreign code must add variants              | interface or class hierarchy              | declared   |

## [2]-[DECISION_LAW]

[OWNER_SELECTION]:
- `SelectOwner(concept)`: choose by decode-versus-interior, field coverage, invariant, identity regime, variant arity, payload timing, and openness; high-churn intermediate values stay inferred plain objects until the seam where decode makes them domain material.
- `UseClass(authority)`: external codecs, value objects, and domain authorities use `Schema.Class` with computed getters; persisted entities use `Model.Class` with field modifiers and the six derived projections.
- `UseTaggedEnum(family)`: closed variants with per-occurrence payload, exhaustive `$match` fold, `$is` narrowing, and stored call modality; never exported, file-internal dispatch.
- `UseVocabulary(rows)`: a bounded keyed domain whose rows carry behavior columns; `keyof typeof` is the discriminant, indexed access the output, computed getters read the row.

[COLLAPSE_FUNCTIONS]:
- `CollapseSchema(family)`: keep one Schema authority and derive every view through `Schema.pick`/`omit`/`partial`/`extend`; delete parallel structs, mirror types, standalone branded exports, and the `Schema.Struct` minted per shape.
- `CollapseVocabulary(consts)`: a third one-use module-level `const _Status = Schema.Literal(...)` before a class collapses into inline field-position `Schema.Literal(...)`; extract to a const only when two or more sites within the file consume it.
- `CollapseDispatch(arms)`: a `Match.when` chain classifying into tiers a vocabulary already maps collapses into vocabulary lookup or threshold iteration.

[BEHAVIOR_FUNCTIONS]:
- `PlaceBehavior(selector)`: use computed getters on a class for derived semantics over private vocabulary axes, `$match` fold for closed-family projection, vocabulary indexed access for keyed lookup, and `Match` only for structural or predicate dispatch on non-keyed shapes.
- `RejectExternalDispatch(key)`: collapse a parallel `Match` chain, a sibling decoder family, and repeated full-coverage folds into computed getters, one `$match`, or one vocabulary lookup.

[CHANGE_FUNCTIONS]:
- `PlaceGrowthCost(owner)`: a new `Data.TaggedEnum` variant breaks every `$match` and `tagsExhaustive` site at compile time; a new vocabulary row adds one entry and the `keyof typeof` discriminant absorbs it; a new field adds one Schema field and the projections derive.

[EXEMPTION_FUNCTIONS]:
- `UseManualHierarchy(axis)`: hand-roll an interface or class hierarchy only when foreign code must add variants without editing the owner; otherwise the closed `Data.TaggedEnum` is the family.
- `UseSeamEnum(wire)`: permit `Schema.Literal` or `Schema.Enums` over a foreign ordinal only at the boundary; re-close into the domain owner on decode.

## [3]-[CLASS_AUTHORITY]

[SINGLE_SOURCE]:
- Law: one `Schema.Class` is the sole authority for a value object — decode through `Schema.decodeUnknown`, encode through `Schema.encode`, cross-field invariants through `Schema.filter` co-located on the struct, and every projection through `Schema.pick`/`omit`/`partial`.
- Law: a branded primitive is a field-position `Schema.brand(...)` modifier inside the owning class; a standalone module-level branded export is the rejected form.
- Law: private vocabulary substrate (`_Protocol`, `_Projections`) drives the class but is never imported — consumers read computed getters and call one polymorphic static projection.
- Use: a polymorphic `static readonly as = <K extends keyof typeof _Projections>(variant: K) => ...` over a private projection map, deleting the exported decoder family.
- Reject: a parallel `Schema.Struct` plus a separate `type`, a standalone brand export, and a projection map imported by consumers.
- Boundary: a class crossing to a `TypeLiteral` through `Schema.omit()` strips nominal identity, struct-level filter, and computed getters — an irreversible projection owned at the seam.

```ts conceptual
import { Record as R, Schema as S } from "effect"

const _Protocol = {
  h2:   { secure: true,  multiplex: true,  upgrade: false },
  h2c:  { secure: false, multiplex: true,  upgrade: true  },
  http: { secure: false, multiplex: false, upgrade: false },
} as const satisfies Record<string, { secure: boolean; multiplex: boolean; upgrade: boolean }>

class Target extends S.Class<Target>("Target")(S.Struct({
  host:     S.NonEmptyString,
  port:     S.Number.pipe(S.int(), S.between(1, 65535)),
  protocol: S.Literal(...(R.keys(_Protocol) as [keyof typeof _Protocol, ...Array<keyof typeof _Protocol>])),
  weight:   S.Number.pipe(S.between(0, 1)),
  zone:     S.NonEmptyString.pipe(S.brand("Zone")),
  drain:    S.optionalWith(S.Boolean, { default: () => false }),
}).pipe(S.filter(({ host, zone }) => host !== zone || "host and zone differ"))) {
  get transport() { return _Protocol[this.protocol] }
  get secure()    { return this.transport.secure }
  get active()    { return !this.drain && this.weight > 0 }
  static readonly as = <K extends keyof typeof _Projections>(variant: K) =>
    S.decodeUnknown(_Projections[variant])
}

const _Projections = {
  route:   Target.pipe(S.omit(), S.pick("host", "port", "protocol", "weight")),
  summary: Target.pipe(S.omit(), S.pick("host", "protocol", "drain")),
} satisfies Record<string, S.Schema<any, any, never>>
```

[MODEL_PROJECTIONS]:
- Law: one `Model.Class` field-modifier set derives the six typed projections — `.fields`, `.insert`, `.update`, `.select`, `.json`, `.jsonCreate` — so a model replaces four to six parallel struct declarations.
- Law: modifiers compose conjunctively — `Model.Generated`, `Model.GeneratedByApp`, `Model.FieldExcept`, `Model.DateTimeInsertFromDate`, `Model.DateTimeUpdateFromDate` each narrow variant membership, and the intersection is the field's projection set.
- Law: a nullable column surfaces as `Option<unknown>` through `Schema.optionalWith(S.Unknown, { as: "Option" })`; `Option.none` is SQL NULL, never a domain `null`.
- Reject: a `type TaskInsert = Omit<Task, "id">` redeclaring what `Model.insert` derives; `Schema.partial` before `Schema.omit` or directly on a class throws at runtime, so `Schema.partialWith({ exact: true })` after `omit` is the projection form.
- Boundary: the repository is a `static readonly repo` generator yielding `SqlClient` and returning one polymorphic `run` surface; the persisted shape's wire codec belongs to `boundaries.md`.

```ts conceptual
import { Model } from "@effect/sql"
import { Schema as S } from "effect"

class Entity extends Model.Class<Entity>("Entity")({
  id:        Model.Generated(Model.GeneratedByApp(S.UUID)),
  tenantId:  Model.FieldExcept("update", "jsonUpdate")(S.String.pipe(S.brand("TenantId"))),
  name:      S.NonEmptyTrimmedString,
  payload:   S.optionalWith(S.Unknown, { as: "Option" }),
  status:    S.Literal("active", "archived", "purging"),
  createdAt: Model.DateTimeInsertFromDate,
  updatedAt: Model.DateTimeUpdateFromDate,
}) {
  get terminal() { return this.status === "purging" }
}

const _EntityKey   = Entity.pipe(S.pick("id", "tenantId"))
const _EntityPatch = Entity.pipe(S.omit("id", "tenantId", "createdAt", "updatedAt"), S.partialWith({ exact: true }))
```

## [4]-[VOCABULARIES]

[VOCABULARY_DECLARATION]:
- Law: a bounded keyed domain is one `as const satisfies Record<...>` whose keys fix membership and whose columns carry behavior — status, retryability, schedule, log level, weight — read by indexed access, never re-derived.
- Law: `keyof typeof V` is the discriminant union, `(typeof V)[keyof typeof V]` the row type, and `R.keys(V)` spreads into a `Schema.Literal(...)` so the wire vocabulary derives from the same anchor.
- Law: classification iterates or indexes the vocabulary when the vocabulary owns the thresholds; a `Match.when` chain re-encoding the vocabulary's tiers is the rejected form.
- Accept: a vocabulary whose row holds an `Effect`-valued column (a `log` level) directly, since an `Effect` is lazy and constructing the row constructs no work; a construction-heavy policy value — a `Schedule.exponential(...)`, a built `Layer`, a compiled `Schema` — is a thunked column (`() => Schedule.exponential(...)`) so the cost is paid per use, not for every row at module init.
- Reject: a hand-listed string-literal union beside the vocabulary, N parallel one-use literal consts, a tier classifier duplicating vocabulary knowledge, and an eagerly-constructed `Schedule`/`Layer` column built for every row whether the row is dispatched or not.

```ts conceptual
import { Duration, Effect, Number as N, Option, pipe } from "effect"

const Severity = {
  nominal:  { ceiling: 0.3, weight: 1, log: Effect.logInfo    },
  elevated: { ceiling: 0.7, weight: 3, log: Effect.logWarning },
  critical: { ceiling: 1.0, weight: 5, log: Effect.logError   },
} as const satisfies Record<string, { ceiling: number; weight: number; log: (...a: ReadonlyArray<unknown>) => Effect.Effect<void> }>

type _Level = keyof typeof Severity

const _classify = (score: number): _Level =>
  pipe(
    Object.entries(Severity) as ReadonlyArray<readonly [_Level, (typeof Severity)[_Level]]>,
    (rows) => rows.find(([, row]) => score < row.ceiling),
    (hit) => hit?.[0] ?? "critical",
  )

const _backoff = (level: _Level) =>
  pipe(N.divide(Severity[level].weight, Severity.critical.weight), Option.map(Duration.seconds), Option.getOrElse(() => Duration.zero))
```

## [5]-[TAGGED_UNIONS]

[FAMILY_SELECTION]:
- Law: a closed family is one `Data.TaggedEnum` — variants carry per-occurrence payload, `$match` is the exhaustive fold, `$is` the narrowing refinement, and the constructor (`Family.Variant({...})`) the sole ingress.
- Law: a `Data.TaggedEnum` stays file-internal and is never exported; a boundary-crossing failure embeds it as a payload inside one exported `Data.TaggedError` whose single `_tag` carries the union.
- Law: a new variant breaks every `$match` and `Match.tagsExhaustive` site at compile time; the dispatch signature owns the variant list, so growth is a binary break across consumers, free inside one build graph.
- Law: two variants carrying identical payload are distinct only when the tag is itself the load-bearing discriminant a consumer folds on — a `Queued`/`Retrying` pair whose `$match` arms diverge stays split, but two tags that every arm treats identically collapse to one variant with a payload field, since a tag that no fold reads is parallel-shape spam.
- Use: `Family.$match(value, handlers)` for immediate one-shot dispatch, curried `Family.$match(handlers)` for pipeline composition through `Array.map`.
- Reject: a success-or-failure `Data.TaggedEnum` because the Effect error channel owns outcome transport; a `switch (x._tag)` ladder because only the generated `$match` is total; two same-payload tags no `$match` arm distinguishes.
- Boundary: cross-module dispatch architecture and the error-rail shape belong to `surfaces-and-dispatch.md` and `rails-and-effects.md`; this site owns the family declaration and its fold.

```ts conceptual
import { Data, Number as N, Option, pipe } from "effect"

type Phase = Data.TaggedEnum<{
  Closed:   { readonly failures:  number; readonly ceiling: number }
  HalfOpen: { readonly successes: number; readonly window:  number }
  Open:     { readonly elapsed:   number; readonly backoff: number }
}>
const Phase = Data.taggedEnum<Phase>()

const pressure = Phase.$match({
  Closed:   ({ failures, ceiling })   => pipe(N.divide(failures, failures + ceiling), Option.getOrElse(() => 0)),
  HalfOpen: ({ successes, window })   => pipe(N.divide(successes, window), Option.map((r) => 1 - r * r), Option.getOrElse(() => 1)),
  Open:     ({ elapsed, backoff })    => pipe(N.divide(elapsed, backoff), Option.map(N.clamp({ minimum: 0, maximum: 1 })), Option.getOrElse(() => 1)),
})

const isOpen = Phase.$is("Open")
```

[FAULT_FAMILY]:
- Law: one `Data.TaggedError` per module surface carries a `reason` discriminant keyed to one `as const satisfies Record` policy table; computed getters (`status`, `retryable`, `retryAfter`, `severity`) project policy from the row, never from inline literals.
- Law: an internal `Data.TaggedEnum` sub-vocabulary embeds as a payload field, and a `$match` fold inside a getter derives the normalized scalar; `$is` short-circuits the retryability predicate.
- Law: `Schema.TaggedError` is the form when the fault is wire-carried (an HTTP API error), reusing the same policy-table getters; `HttpApiSchema.annotations` binds the default status and the getter overrides per reason.
- Reject: a separate error class per method, an inline status/retry/transport literal outside the policy table, and a parallel error type for an auth or transport failure where one `reason` row adds it.
- Boundary: cause-tree normalization (`Cause.match`), defect-versus-failure, and the rail-level accumulation combinators belong to `rails-and-effects.md`; this site owns the fault family, its policy projection, and the shape-level admission algebra below.

[ADMISSION_ACCUMULATION]:
- Law: a multi-field decode picks its combination algebra from the field dependency graph, fixed once at the shape — independent fields whose validity does not depend on each other admit through the applicative product so every fault surfaces (`Schema.decodeUnknown(schema, { errors: "all" })` over one struct, or `Effect.all({ ... }, { mode: "validate" })` over per-field admissions), and a field whose validity is contingent on a prior field's decoded value binds fail-fast through `Schema.transformOrFail` so the dependent step never runs against an invalid antecedent.
- Law: the carrier selects accumulate-versus-abort, never a flag — `{ errors: "all" }` and `mode: "validate"` are the accumulating carriers, the default first-failure carrier is the dependent-bind form; a fault family whose `Order.max` join combines many faults into a worst-case is the accumulating consumer's projection of that algebra.
- Reject: a fail-fast decode over genuinely independent fields that hides every fault after the first, an applicative product over a dependent chain that validates a step against an antecedent the program has not yet proven, and a post-decode second-pass re-validation that re-reports faults the single decode already owns.

```ts conceptual
import { Data, Duration, Effect, Order, Option } from "effect"

const _Policy = {
  unauthorized: { status: 401, retryable: false, retryAfter: Duration.zero,       ord: 0, log: Effect.logWarning },
  conflict:     { status: 409, retryable: true,  retryAfter: Duration.zero,       ord: 1, log: Effect.logWarning },
  unavailable:  { status: 503, retryable: true,  retryAfter: Duration.seconds(1), ord: 2, log: Effect.logError   },
} as const satisfies Record<string, { status: number; retryable: boolean; retryAfter: Duration.Duration; ord: number; log: (...a: ReadonlyArray<unknown>) => Effect.Effect<void> }>

class GatewayFault extends Data.TaggedError("GatewayFault")<{
  readonly reason: keyof typeof _Policy
  readonly detail: string
}> {
  static readonly ord  = Order.mapInput(Order.number, (f: GatewayFault) => _Policy[f.reason].ord)
  static readonly join = Order.max(GatewayFault.ord)
  get status()     { return _Policy[this.reason].status     }
  get retryable()  { return _Policy[this.reason].retryable  }
  get retryAfter() { return _Policy[this.reason].retryAfter }
  get body() {
    return {
      error: this.reason, detail: this.detail, status: this.status, retryable: this.retryable,
      ...(Duration.greaterThan(this.retryAfter, Duration.zero) && { retryAfterMs: Duration.toMillis(this.retryAfter) }),
    } as const
  }
}
```

## [6]-[TRANSFORM_CODECS]

[BIDIRECTIONAL_PROJECTION]:
- Law: a wire-to-domain projection is one `Schema.transform(fromSchema, toSchema, { decode, encode })` — the transform is the projection, not a post-processing step, and decode/encode preserve round-trip fidelity.
- Law: a bijection vocabulary (`_Encoding` carrying id-to-key and key-to-id) is the private substrate driving both directions; a derived field present in domain but absent from wire is intentional asymmetry the encode side drops.
- Law: layered schemas compose — `Schema.Uint8ArrayFromBase64` on the wire side and `Schema.Uint8ArrayFromSelf` on the domain side each own one concern, and the transform owns only the vocabulary-driven layer.
- Reject: a separate parser and serializer function pair, a post-decode mutation step, and a transform without an explicit target schema.
- Boundary: codec attribute placement, the converter that owns a closed wire family, and byte-identity forwarding belong to `boundaries.md`; this site owns the structural transform.

```ts conceptual
import { Record as R, Schema as S } from "effect"

const _Encoding = {
  gzip: { id: 1, streamable: true  },
  zstd: { id: 2, streamable: true  },
  none: { id: 0, streamable: false },
} as const satisfies Record<string, { id: number; streamable: boolean }>

const _ById = R.fromEntries(R.toEntries(_Encoding).map(([k, v]) => [v.id, k])) as Record<number, keyof typeof _Encoding>

const Envelope = S.transform(
  S.Struct({ payload: S.Uint8ArrayFromBase64, encodingId: S.Number.pipe(S.int(), S.between(0, 2)), version: S.Number.pipe(S.int(), S.positive()) }),
  S.Struct({ payload: S.Uint8ArrayFromSelf, encoding: S.Literal("gzip", "zstd", "none"), version: S.Number.pipe(S.int(), S.positive()), streamable: S.Boolean }),
  {
    decode: ({ payload, encodingId, version }) => ({ payload, version, encoding: _ById[encodingId] ?? "none", streamable: _Encoding[_ById[encodingId] ?? "none"].streamable }),
    encode: ({ payload, encoding, version }) => ({ payload, version, encodingId: _Encoding[encoding].id }),
  },
)
```
</content>
