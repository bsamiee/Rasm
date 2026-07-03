# [TYPESCRIPT_SHAPES]

A domain concept takes exactly one runtime authority — a Schema owner whose single declaration is simultaneously the value, the static type, the validator, the constructor, the codec anchor, and the derivation root. Owner-form selection is mechanical over three discriminants read in order: fallibility (the concept is itself a fault travelling the error channel), payload arity (one product or a closed case family), and wire crossing (the concept carries an encoded twin or lives and dies in-process). The selected form owns everything downstream: the static type derives from the declaration, the wire twin derives from the encoded side, variants and projections derive from the field record, and every free surface — arbitrary, JSON contract, equivalence, pretty-printer, foreign-validator view — derives from the same AST, so one edit site moves the entire family.

The budget is one fifth of naive TypeScript: one rich class family replaces the interface, the DTO, the validator function, the factory, and the scattered branded scalars a naive module mints for one concept. A second shape is admitted only when it changes implementation law — a new trust boundary, a new identity regime, a new wire dialect; a parallel interface beside an owner, a hand-written wire twin, a free-floating brand export, and a shape minted to name an intermediate step are rejected on sight.

## [01]-[OWNER_FORMS]

Choose the owner form before writing any field. The most specific matching row wins.

[FORM_INDEX]:

| [INDEX] | [CONCEPT_SIGNATURE]                                  | [OWNER_FORM]                         | [WIRE]          | [IDENTITY] |
| :-----: | :--------------------------------------------------- | :----------------------------------- | :-------------- | :--------- |
|  [01]   | named product; invariants, behavior, or 2+ consumers | `Schema.Class`                       | encoded derives | structural |
|  [02]   | field block embedded in one owner                    | inline `Schema.Struct` record        | inherited       | structural |
|  [03]   | closed case family crossing a wire                   | `Schema.Union` of tagged case owners | per-case        | `_tag`     |
|  [04]   | closed case family, process-local                    | `Data.taggedEnum`                    | none            | `_tag`     |
|  [05]   | fault carried on the error channel or a wire         | `Schema.TaggedError`                 | encoded derives | `_tag`     |
|  [06]   | scalar invariant                                     | brand-in-field refinement            | inherited       | value      |
|  [07]   | one concept, N systematic storage or wire views      | `VariantSchema.make` field family    | per-variant     | structural |
|  [08]   | request, patch, or view of an owner                  | derived projection, never declared   | derives         | inherited  |

[OWNER_SELECTION]:
- Law: named products take `Schema.Class` regardless of wire exposure — the class costs one identifier string over the `Data.Class` form and gains decode, encode, `make` validation, and the whole derived-surface family the moment any consumer needs them; `Data.Class` and `Data.Case` are rejected product forms because a second product paradigm buys nothing the Schema form lacks.
- Law: the `Data`-versus-`Schema` split is earned only for case families, where the Schema form costs a class per case — a family that never serializes, never logs structurally, and never joins a decoded union is `Data.taggedEnum`; any family that might is declared wire-carried from the start, because a `Data.taggedEnum` cannot reach a wire and promotion rewrites every case construction site.
- Law: inside a wire-carried union, a case with behavior is `Schema.TaggedClass` and a pure-data case is `Schema.TaggedStruct` — the union mixes both freely, and behavior arriving later converts a struct case to a class case with zero consumer edits because tag and fields are unchanged.
- Law: `Schema.Struct` survives only as row [02] — an anonymous field block embedded in one owner or a single-consumer contract; a passed-around `Struct` promoted to a `Class` later re-anchors every consumer, so a concept with a name is declared as a class first, never migrated to one.
- Reject: a one-field class wrapping a scalar row [06] already refines; an `interface` or `type` alias declared beside any owner; a shape triple minted per architectural layer; a boolean-discriminated pair standing where row [03] or [04] owns the family.

[SHAPE_ECONOMY]:
- Law: growth lands inside the owner as a row — a field, a getter, a case, a variant column, a refinement — and the diff of the next requirement is the proof: one declaration inside the owner, consumers untouched or broken loudly at the missing arm.
- Law: a refinement shared by two owners is one `_`-prefixed interior field schema composed into both field records and exported by neither; the brand reaches consumers only through owner fields — `Shape["key"]` indexed access, instance reads — so the refinement has one edit site and zero standalone dependents.
- Law: identity is carried, not written — Schema class instances and `Data.taggedEnum` cases compare structurally under `Equal.equals` and hash by value from the declaration alone; a hand-written `equals` method or an identity field added for comparison is dead surface.

## [02]-[RICH_OWNER]

The rich owner is one `Schema.Class<Self>(identifier)(fields)` declaration: the class is the value, the type, the decode anchor, and the constructor under a single name, so a consumer takes one import and never aliases, re-derives, or writes `typeof` at a call site.

[RICH_OWNER_LAW]:
- Law: brands ride fields — `Schema.NonEmptyString.pipe(Schema.pattern(/.../), Schema.brand("<key>"))` declared at the field or as the shared interior schema; the branded type exists only through the owner, and a standalone exported brand is the named defect.
- Law: every derived reading is a class getter composing the algebra the fields admit — an `Order` policy value folding a collection field, an `Option.match` collapsing an absence field; a free function re-deriving what a getter states, or a consumer computing it at the call site, marks the missing getter.
- Law: an embedded concept with its own invariants is its own class composed as a field at full depth — `Schema.NonEmptyArray(Anchor)` — never flattened into prefixed sibling fields; the inline `Struct` block is the sub-form only while the block has no behavior and no second consumer.
- Law: lineage derives, siblings never re-declare — `Owner.extend<Wider>(identifier)(fields)` widens into a subtype carrying every base field, getter, and refinement; `Owner.transformOrFail<Enriched>(identifier)(fields, { decode, encode })` derives an owner whose added fields compute from the base at decode; a second class restating base fields is the defect these two forms exist to kill.
- Law: `new Owner(...)` and `Owner.make(...)` run the filter set, so trusted interior construction proves the same invariants decode proves; raw material enters through decode at the admission seam — placement is the boundary page's — and `{ disableValidation: true }` survives only inside a kernel that already proved the invariant it skips.
- Reject: an `interface`-plus-implementation pair; a DTO type beside the class; constructor parameter properties; a `with<Field>` copy-method family restating what a successor constructor or derived projection owns.

```typescript
import { Array as Arr, Option, Order, Schema } from "effect"

const _Key = Schema.NonEmptyString.pipe(
  Schema.pattern(/^[a-z][a-z0-9-]*$/),
  Schema.brand("<key>"),
)

class Anchor extends Schema.Class<Anchor>("Anchor")({
  axis: Schema.Literal("<axis-a>", "<axis-b>"),
  weight: Schema.Number.pipe(Schema.between(0, 1)),
}) {
  get dominant(): boolean {
    return this.weight >= 0.5
  }
}

const _byWeight: Order.Order<Anchor> = Order.mapInput(Order.number, (anchor: Anchor) => anchor.weight)

class Shape extends Schema.Class<Shape>("Shape")({
  key: _Key,
  rank: Schema.Int.pipe(Schema.between(0, 9)),
  anchors: Schema.NonEmptyArray(Anchor),
  note: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {
  get strongest(): Anchor {
    return Arr.max(this.anchors, _byWeight)
  }
  get caption(): string {
    return Option.match(this.note, {
      onNone: () => this.key,
      onSome: (note) => `${this.key}: ${note}`,
    })
  }
}

class Sealed extends Shape.extend<Sealed>("Sealed")({
  seal: Schema.UUID,
}) {
  get receipt(): string {
    return `${this.caption}#${this.seal}`
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Anchor, Sealed, Shape }
```

The `Shape` family replaces six loose shapes — the interface, the wire DTO, the key brand alias, the validator, the factory, and the anchor-summary helper — and `Sealed` lands as lineage, not a sibling: every base refinement, getter, and field arrives by derivation. The non-empty collection type makes `Arr.max` total, so the getter needs no fallback arm.

## [03]-[TAGGED_FAMILIES]

A closed family is one owner under one name; the `_tag` is simultaneously the runtime discriminant, the wire discriminant, and the dispatch key, so exactly one spelling of case identity exists.

[CASE_FAMILIES]:
- Use: `type Variant = Data.TaggedEnum<{...}>` plus `const Variant: Data.TaggedEnum.Constructor<Variant> = Data.taggedEnum<Variant>()` — the same name serves the union type, the case constructors, `$is`, and `$match`, so declaration, construction, guarding, dispatch, and typing are one import.
- Law: case payloads are `readonly` fields and a payload-free case is `{}`; constructors carry structural equality, so family members compare by value with no identity ceremony.
- Law: a generic family rides `Data.TaggedEnum.WithGenerics` with a definition interface — never a hand-parameterized union restating the case algebra per type argument.
- Law: a wire-carried family is `Schema.Union` over tagged case owners bound to one same-name `const`-plus-`type` pair, and the member set is the single growth site — a new case is one class plus the union row, with every exhaustive consumer breaking loudly until its arm exists.
- Law: vocabulary literals spread from the value anchor — `Schema.Literal(..._Stage)` derives the schema arm from the same `as const` table the type level derives from, so the vocabulary has one row set and zero parallel spellings.
- Boundary: `$match` is the family's carried dispatch surface; terminal selection between exhaustive, option, and either forms is the dispatch page's.
- Reject: sibling schemas per case with no union owner; a string-typed `status` field where a case family owns the states; a `kind` field beside `_tag`; a class hierarchy dispatched by `instanceof`.

[FAULT_DECLARATION]:
- Use: `Schema.TaggedError<Self>()(tag, fields)` for a fault that crosses a wire, logs structurally, or joins a decoded union — one class is the fault value, the fault schema, and the `_tag` catch key, and the instance is yieldable on the rail.
- Law: fields carry evidence as data — the refused key, the stage, retryability, severity — typed so policy reads them; `message` is a derived `override get` computed from fields and never a stored field, because a message field is denormalized evidence that drifts from what the fields prove.
- Boundary: fault-family architecture — the family-per-surface partition, reason discriminants, policy folds, catch routing — is the rails page's; this page owns the declaration form.
- Reject: `class Fault extends Error`; evidence baked into message strings; a fault union assembled from untagged shapes.

```typescript
import { Data, Schema } from "effect"

const _Stage = ["<stage-a>", "<stage-b>", "<stage-c>"] as const

type Signal = Data.TaggedEnum<{
  Idle: {}
  Busy: { readonly since: number }
  Halted: { readonly fault: string; readonly fatal: boolean }
}>
const Signal: Data.TaggedEnum.Constructor<Signal> = Data.taggedEnum<Signal>()

class Opened extends Schema.TaggedClass<Opened>()("Opened", {
  key: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
}) {}

class Closed extends Schema.TaggedClass<Closed>()("Closed", {
  key: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  total: Schema.NonNegative,
}) {
  get summary(): string {
    return `${this.key}=${this.total}`
  }
}

const Journal: Schema.Union<[typeof Opened, typeof Closed]> = Schema.Union(Opened, Closed)
type Journal = typeof Journal.Type

class ShapeFault extends Schema.TaggedError<ShapeFault>()("ShapeFault", {
  key: Schema.NonEmptyString,
  stage: Schema.Literal(..._Stage),
  retryable: Schema.Boolean,
}) {
  override get message(): string {
    return `<${this.stage}> refused ${this.key}`
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Journal, ShapeFault, Signal }
```

The stage vocabulary stays interior: consumers read it as `ShapeFault["stage"]`, so the row set has one edit site and the export surface holds at the three owners.

## [04]-[ABSENCE]

The interior never meets `?:`, `undefined`, or `null`: `Schema.optionalWith(S, { as: "Option" })` is the sole absence spelling on a decoded field — total `Option<A>` on the type side, optional on the encoded side — and every wire dialect folds into that one interior shape at the declaration.

[ABSENCE_LAW]:
- Law: the options row selects the wire dialect, never the interior type — `{ as: "Option", nullable: true }` folds absent-or-null to none, with `onNoneEncoding: () => Option.some(null)` when the wire demands an explicit null on write; `{ as: "Option", exact: true }` when key absence alone spells absence; `Schema.OptionFromNullOr(S)` when the key is always present and null is the absence value. Four dialects, one `Option<A>` field.
- Law: `{ default: () => value }` versus `as: "Option"` is ownership of the fallback — default when the owner fixes the value and no consumer distinguishes absent from defaulted; `Option` when the consumer decides; a defaulted field is total and plain in the interior, and `Option.getOrElse` repeating one fallback across consumers marks a default that belonged in the declaration.
- Law: absence with a cause is a tagged family — `Option.none()` carries zero evidence, so the moment two causes of absence exist the field is a case family whose cases carry their cause, and `Option` survives only as the cause-free projection a consumer derives from it; wrapping the family in `Option`, or nesting `Option`, is the rejected form.
- Reject: bare `Schema.optional` (leaks `| undefined` into the decoded type); `Schema.NullOr` on a domain field; sentinel values standing for absence; a boolean `has`-twin beside a nullable field.

```typescript
import { Data, Option, Schema } from "effect"

type Presence = Data.TaggedEnum<{
  Held: { readonly value: string }
  Redacted: { readonly until: number }
  Withdrawn: { readonly reason: string }
}>
const Presence: Data.TaggedEnum.Constructor<Presence> = Data.taggedEnum<Presence>()

class Slot extends Schema.Class<Slot>("Slot")({
  label: Schema.optionalWith(Schema.NonEmptyString, {
    as: "Option",
    nullable: true,
    onNoneEncoding: () => Option.some(null),
  }),
  alias: Schema.optionalWith(Schema.NonEmptyString, { as: "Option", exact: true }),
  weight: Schema.optionalWith(Schema.Number, { default: () => 1 }),
  tint: Schema.OptionFromNullOr(Schema.NonEmptyString),
}) {
  get title(): string {
    return Option.getOrElse(
      Option.orElse(this.label, () => this.alias),
      () => "<untitled>",
    )
  }
}

const revealed = (presence: Presence): Option.Option<string> =>
  Presence.$match(presence, {
    Held: ({ value }) => Option.some(value),
    Redacted: () => Option.none(),
    Withdrawn: () => Option.none(),
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export { Presence, revealed, Slot }
```

`revealed` fixes the collapse direction: the cause-bearing family projects down to `Option` at the consumer that does not care why — never the reverse, because a shed cause cannot be recovered.

## [05]-[WIRE_TWINS]

The wire twin derives: `typeof Owner.Encoded` is the wire type, `Schema.encodedSchema(Owner)` materializes it as a schema, and a hand-declared wire interface beside an owner is the named defect. Divergence between wire and domain lands inside the owner declaration, graded by severity.

[WIRE_TWIN_LAW]:
- Law: spelling divergence is a field-level rename — `Schema.propertySignature(S).pipe(Schema.fromKey("<foreign-name>"))` inside the field record, so the decoded side carries canonical names, the encoded side carries the foreign spelling, and no parallel rename map or mapping layer exists.
- Law: structural divergence is one bidirectional declaration — `Schema.transform(From, To, { strict: true, decode, encode })` when total, `Schema.transformOrFail` returning `ParseResult.succeed` or `ParseResult.fail(new ParseResult.Type(ast, actual, "<why>"))` when partial; both directions declare at one site, and the `To` side's own refinements re-prove after the mapping runs, so a transform moves spelling and structure but never restates validation.
- Law: `Schema.parseJson(Owner)` fuses `JSON.parse` and `JSON.stringify` into the codec — raw JSON string to owner is one decode and owner to string is one encode; a `JSON.parse` call beside a decode is the named defect, and the fused schema composes anywhere a schema does.
- Boundary: decode and encode execution — `Schema.decodeUnknown` placement, `ParseError` lifting — is the boundary page's; this page owns the twin's declaration.
- Reject: hand-written `toWire`/`fromWire` method pairs; a codec class beside an owner; two one-directional transforms for one twin; a version branch inside the owner where a read-boundary migration owns it.

```typescript
import { ParseResult, Schema } from "effect"

class Grade extends Schema.Class<Grade>("Grade")({
  key: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("grade_key")),
  score: Schema.propertySignature(Schema.Int.pipe(Schema.between(0, 100))).pipe(Schema.fromKey("grade_score")),
}) {
  get passed(): boolean {
    return this.score >= 60
  }
}

type GradeWire = typeof Grade.Encoded

const GradeFromPacked: Schema.transformOrFail<typeof Schema.NonEmptyString, typeof Grade> = Schema.transformOrFail(Schema.NonEmptyString, Grade, {
  strict: true,
  decode: (packed, _, ast) => {
    const at = packed.lastIndexOf("@")
    return at < 0
      ? ParseResult.fail(new ParseResult.Type(ast, packed, "<missing-separator>"))
      : ParseResult.succeed({
          grade_key: packed.slice(0, at),
          grade_score: Number(packed.slice(at + 1)),
        })
  },
  encode: (wire) => ParseResult.succeed(`${wire.grade_key}@${wire.grade_score}`),
})

const GradeFromJson: Schema.SchemaClass<Grade, string> = Schema.parseJson(Grade)

// --- [EXPORTS] --------------------------------------------------------------------------

export { Grade, GradeFromJson, GradeFromPacked }
export type { GradeWire }
```

The packed decode maps a legacy spelling onto the owner's encoded side and stops: a non-numeric score is not the transform's problem, because `Schema.Int.pipe(Schema.between(0, 100))` re-proves on the decoded side — the twin composes the owner's law instead of duplicating it.

## [06]-[VARIANT_DERIVATION]

When one concept systematically projects into N storage or wire views, the variant axis is data: `VariantSchema.make({ variants, defaultVariant })` yields a field-family kit — `Class`, `Field`, `FieldOnly`, `FieldExcept`, `extract` — and one declaration emits every variant schema.

[VARIANT_FAMILY]:
- Law: per-field modality declares where a field exists — a plain schema field rides every variant, `Field({ "<variant>": S })` enumerates, `FieldOnly("<variant>")(S)` restricts, `FieldExcept("<variant>")(S)` subtracts; a generated column is `FieldOnly` on read variants, a secret is `FieldExcept` on egress variants, and the field record is the single site stating the whole matrix.
- Law: the class is the default variant and carries every other variant as a static — `Row.insert`, `Row.json` — with `extract(Row, "<variant>")` as the computed access; a new variant is one tuple entry plus the per-field rows it changes, never a new shape.
- Law: a hand-declared per-variant struct family is the named defect — N parallel shapes restating one concept, each a drift site the field family makes structurally impossible.
- Boundary: the systematic axis belongs here; a one-off view derives through the projection forms of the derived-surfaces section, and a `pick`/`omit` chain rebuilding what a variant matrix states is the inverted choice.
- Reject: variant divergence handled by optional fields on one wide shape; a secret scrubbed at egress call sites instead of subtracted at the field; a write-side default re-implemented as a nullable column where `VariantSchema.Overrideable` owns the generate-unless-overridden semantics.

```typescript
import { VariantSchema } from "@effect/experimental"
import { Schema } from "effect"

const { Class, Field, FieldExcept, FieldOnly, extract } = VariantSchema.make({
  variants: ["select", "insert", "json"],
  defaultVariant: "select",
})

class Row extends Class<Row>("Row")({
  id: FieldOnly("select", "json")(Schema.Int),
  key: Schema.NonEmptyString,
  secret: FieldExcept("json")(Schema.Redacted(Schema.String)),
  revised: Field({
    select: Schema.DateTimeUtc,
    json: Schema.DateTimeUtc,
  }),
}) {
  get slug(): string {
    return `${this.id}:${this.key}`
  }
}

const RowInsert = extract(Row, "insert")
const RowJson = Row.json

// --- [EXPORTS] --------------------------------------------------------------------------

export { Row, RowInsert, RowJson }
```

Three schemas from one declaration: the insert variant carries no `id` and no `revised`, the json variant never carries `secret`, and the redaction composes at the field so no egress site can forget it.

## [07]-[DERIVED_SURFACES]

Every free surface derives from the owner's AST, and every ad-hoc view derives from the owner's field record; a hand-maintained parallel of either is the named defect.

[DERIVED_INSTANCES]:
- Law: the derivation set is `Arbitrary.make(Owner)` yielding a `FastCheck.Arbitrary<Owner>`, `JSONSchema.make(Owner)` yielding a `JSONSchema.JsonSchema7Root`, `Pretty.make(Owner)`, `Schema.equivalence(Owner)` yielding an `Equivalence.Equivalence<Owner>`, `Schema.is(Owner)` as the derived guard, and `Schema.standardSchemaV1(Owner)` as the one view handed to foreign validation consumers — a hand-written generator, printer, comparator, guard, JSON contract, or parallel validator beside an owner restates what the AST already proves.
- Law: field refinements are what keep derived surfaces truthful — `between`, `maxLength`, `pattern`, and brands flow into generation and contract emission, so a refinement skipped at the field surfaces downstream as a lying arbitrary and an under-constrained contract; the `arbitrary` annotation at the declaration overrides generation only where domain bounds exceed what filters express.
- Law: derived instances are `_`-prefixed interior consts at the owner module, exported only on cross-module demand — the owner is the export; its derivations are its interior.

[PROJECTIONS]:
- Law: a projection re-anchors on the field record — `Schema.Struct(Owner.fields)` then `Schema.pick(...)`, `Schema.omit(...)`, `Schema.partialWith({ exact: true })`, `Schema.extend(...)` — because the class node is a transformation and its projections derive from its fields, not from the class itself.
- Law: a patch is `omit` of identity then `partialWith({ exact: true })` — absent means unchanged, the exact form emits `?:` without `| undefined`, and a patch that can address a different aggregate or spell set-to-undefined is a wrong shape, not a caller error.
- Law: a projection never gains authority — it carries no field the owner lacks, and a value reaching a wire outside any owner field marks a missing owner field, not a projection to widen.
- Reject: a hand-declared patch or request interface; `Schema.partial` where the exact form is meant; a stored projection type restating what a one-line derivation states at the use site.

```typescript
import { Arbitrary, FastCheck, JSONSchema, Pretty, Schema } from "effect"
import type { Equivalence } from "effect"

class Member extends Schema.Class<Member>("Member")({
  key: Schema.UUID,
  label: Schema.NonEmptyString.pipe(Schema.maxLength(64)),
  score: Schema.Number.pipe(Schema.between(0, 1)),
}) {}

const MemberPatch = Schema.Struct(Member.fields).pipe(
  Schema.omit("key"),
  Schema.partialWith({ exact: true }),
)
const MemberBadge = Schema.Struct(Member.fields).pipe(Schema.pick("key", "label"))

const _cohort: FastCheck.Arbitrary<Member> = Arbitrary.make(Member)
const _same: Equivalence.Equivalence<Member> = Schema.equivalence(Member)
const _shown: (member: Member) => string = Pretty.make(Member)
const _contract: JSONSchema.JsonSchema7Root = JSONSchema.make(Member)
const _isMember = Schema.is(Member)
const MemberStandard = Schema.standardSchemaV1(Member)

// --- [EXPORTS] --------------------------------------------------------------------------

export { Member, MemberBadge, MemberPatch, MemberStandard }
```

One declaration feeds the generator, the equivalence, the printer, the contract, the guard, the foreign-validator view, the patch, and the badge view — eight surfaces, zero hand-maintained parallels, and every one moves when a field moves.
