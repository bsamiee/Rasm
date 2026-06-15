# Variant Alphabet Sizing Threshold

[THE_THRESHOLD_IS_A_FIELD_ARITY_CHANGE_NOT_A_CHANNEL_COUNT]:
- The `C ≥ 3` promotion is not a count of channels reached by counting — it is the arity flip of the field itself, and the count is the symptom. A field at one or two channels is a single `PropertySignature` whose `Type`/`Encoded` is one pair; a field at three is a *function from variant to schema*, and the moment a field must answer "which schema on this channel" the field stops being a value and becomes a per-variant lookup. The threshold is read off whether the field's representation is a point or a map, never off the integer of channels, because a two-channel field whose two channels are the *same* schema is still a point and stays in-owner.
- The number three is derived, not chosen: `Schema.optionalWith` already carries *two* representations in one declaration — its `Type` is `Option<A>` (decoded) and its `Encoded` is `A | null | undefined` (wire), so a single field modifier spans the read↔json divergence with no alphabet. The third distinct representation is the first one `optionalWith` structurally cannot hold, because its `Options` record (`as`, `default`, `exact`, `nullable`, `onNoneEncoding`) parameterizes exactly one decode/encode bracket, not a third independently-shaped channel. The threshold is the cardinality at which the in-owner field modifier's fixed two-representation budget is exceeded — `C = 3` is the first `C` where no single `PropertySignature` covers the demand.
- The discriminant under the count is per-call versus per-channel divergence, and the two never sum: a field present-or-absent *per construction call* is `Schema.optionalWith({ as: "Option" })` regardless of how many channels exist, while a field whose admitted *shape* differs *per channel* is the alphabet member. A concept with five channels but one shape across all of them is a flat owner with `C = 1`; a concept with three channels each admitting a structurally distinct schema is the alphabet at `C = 3`. The misread is counting channels the value passes through rather than channels whose *schema* diverges — only the latter increments the sizing `C`.

```typescript
// sub-threshold: one PropertySignature spans decoded Option<A> and wire A | null | undefined — no alphabet
const note = Schema.optionalWith(Schema.NonEmptyTrimmedString, { as: 'Option', nullable: true })
// reject at C >= 3: three sibling structs hand-derived for three channels, each a pick/omit that drifts on field change
const Public = Schema.Struct({ id: Schema.UUID, body: Schema.String })
const Internal = Schema.Struct({ id: Schema.UUID, body: Schema.String, owner: Schema.String })
const Audit = Schema.Struct({ id: Schema.UUID, body: Schema.String, owner: Schema.String, revisedBy: Schema.String })
```

[PROMOTION_LIFTS_THE_SUB_THRESHOLD_CONSTRUCT_INTO_ONE_CHANNEL]:
- Crossing the threshold does not discard the in-owner modifier — it *embeds* it as one channel's value, so the promotion is representation-preserving and the sub-threshold spelling survives intact inside the alphabet. A field whose absence-on-some-channels and `Option`-on-others both hold is one variant-keyed config whose individual channel values are the very `optionalWith`/`OptionFromNullOr` constructs the flat owner used: the database channels carry `Schema.OptionFromNullOr<S>` (a `transform<NullOr<S>, OptionFromSelf<…>>`) and the json channels carry `Schema.optionalWith(S, { as: "Option" })` and `Schema.optionalWith(S, { as: "Option", nullable: true })` — the same point-constructs, now indexed by channel rather than standing alone.
- This is the collapse that fuses the two tiers into one continuum: the alphabet member is not a *new* construct replacing the field modifier, it is a *function over* field modifiers, so the promotion's diff is the field's value changing from `optionalWith(...)` to `{ db: OptionFromNullOr(...), json: optionalWith(...) }` and nothing else. The reject is re-deriving the absence semantics from scratch at the alphabet tier — the channel values are exactly the sub-threshold constructs the modifier already named, and the promotion threads them through the variant axis without re-spelling either.
- The pre-composed column-policy modifiers are the proof the lift is mechanical: a modifier admitting a field on a strict channel subset is a fixed `Field` config — a generated-column policy is `{ select, update, json }` (insert omitted), a sensitive-value policy is `{ select, insert, update }` (every json channel omitted) — each a closed map from a channel subset to the *same* schema. The open `Field({ … })` form is the same mechanism with per-channel *distinct* schemas; the canned modifier is its sugar when the per-channel schema is uniform and only presence varies. Selecting between the open form and a canned modifier is selecting whether the divergence is shape-and-presence or presence-alone.

```typescript
const Cell = VariantSchema.make({ variants: ['select', 'insert', 'update', 'json'], defaultVariant: 'select' })
// the sub-threshold optionalWith is preserved, now one channel's value inside a variant-keyed function
const optional = <S extends Schema.Schema.Any>(s: S) =>
    Cell.Field({
        select: Schema.OptionFromNullOr(s),
        insert: Schema.OptionFromNullOr(s),
        update: Schema.OptionFromNullOr(s),
        json: Schema.optionalWith(s, { as: 'Option' }),
    })
class Row extends Cell.Class<Row>('Row')({
    id: Schema.UUID,
    label: Cell.FieldExcept('json')(Schema.NonEmptyTrimmedString), // presence-only policy: db channels, json omitted
    note: optional(Schema.String), // shape-and-presence policy: distinct schema per channel
}) {}
```

[DEFAULT_VARIANT_IS_A_SUFFIX_KEYSTROKE_MINIMIZATION]:
- The default variant is the solution to an objective function, not a label: the corpus suffix surface is the sum over non-default channels of each channel's read-count, the bare owner *is* `ExtractFields<Default, Fields, true>`, and the optimal default is the `argmax` channel of the read distribution — the one whose suffix the bare-owner type absorbs to zero. A default chosen by declaration order or alphabetical accident maximizes rather than minimizes that surface, taxing every common read with an explicit `Owner.<channel>` suffix the default could have erased. The minimization is computed against the read distribution, never the write distribution, because only reads traverse the bare owner.
- The decision is write-once and irreversible without re-routing every suffix-free read: a default flip after consumers bind changes the bare-owner instance type, so every unsuffixed `decodeUnknown(Owner)` and every field access through the bare class breaks at compile time — the loud-break the wrong default earns, and the reason the default is fixed at the alphabet declaration with the same conservatism as the irrecoverable shape axes.
- The default's `IsDefault = true` flag is the mechanism, not cosmetics: the extraction branches `IsDefault extends true ? [A] extends [Schema.Schema.Any] ? A : Schema.Struct<…>`, so a nested-struct field that is itself already a schema passes through *unprojected* on the default channel while every named member re-wraps it as a fresh `Schema.Struct`. The default projection alone can therefore carry a sub-owner at its full schema form — its identifier, its annotations, its instance methods intact — whereas the named-member projections shed them by re-structuring. A composite field whose embedded owner must keep its behavior on the read channel and collapse to a plain struct on the write channels places its rich form on the default, because only the default's `IsDefault` path tolerates the un-re-wrapped sub-schema — the suffix-free channel is also the only sub-owner-preserving one, fusing the keystroke minimization with a representation-fidelity asymmetry no named member shares.

```typescript
const View = VariantSchema.make({ variants: ['summary', 'detail', 'export'], defaultVariant: 'detail' })
class Item extends View.Class<Item>('Item')({
    id: Schema.UUID,
    title: Schema.NonEmptyTrimmedString,
    body: View.FieldOnly('detail', 'export')(Schema.String),
}) {}
// detail is the hot read: the bare owner IS the detail projection, suffix-free
const read: (u: unknown) => Effect.Effect<Item, ParseResult.ParseError> = Schema.decodeUnknown(Item)
// the rare channels carry the suffix cost the minimization pushed onto them
const exportWire: (u: unknown) => Effect.Effect<Schema.Schema.Type<typeof Item.export>, ParseResult.ParseError> =
    Schema.decodeUnknown(Item.export)
```

[ALPHABET_TOTALITY_IS_A_SUBSET_GATE_NOT_AN_EQUALITY]:
- The totality is one-directional and the directionality is the whole law: the field-config gate is `[keyof Config] extends [Variant]` — a *subset* check, so a field may name *fewer* channels than the alphabet but never *more*. Under-naming is legal and load-bearing (it is the mechanism of per-channel presence — a generated-column policy under-names by omitting `insert`), while over-naming poisons the field's value with the literal type `"field must have valid variants"`. The alphabet bounds the field's channels from above; the field bounds its own presence from below by which subset it names — the gate is not "the field's channels equal the alphabet," it is "the field's channels are within it."
- The two failure modes are non-substitutable diagnostics resolved on disjoint surfaces, and neither is a runtime check: over-naming surfaces a string-literal type at the *field declaration site inside the struct* (before any projection is taken), while under-naming succeeds and the field is *dropped* from the unnamed channel's projection by `as … ? K : never` key-remapping — the extracted struct simply lacks the field rather than carrying it as `never` or `undefined`. The declaration-side gate and the projection-side drop compose into a complete totality: a foreign channel fails loudly at declaration, a missing channel vanishes silently from one projection, and the alphabet is the closed vocabulary both directions are checked against.
- The string-literal failure type is the mechanism that makes the break loud rather than a cryptic structural mismatch: the field's value resolving to a sentence rather than a `Schema` is what surfaces the misroute as a readable type error at the offending field, so the totality reports *which* field named *which* unrouted channel with no inspection of the projection chain. The reject is deferring the channel check to projection time — a field validated only when extracted would admit an unrouted channel name into the owner and fail at the consumer, not the declaration, splitting the diagnostic from its cause.

```typescript
const Doc = VariantSchema.make({ variants: ['public', 'internal'], defaultVariant: 'public' })
// legal under-naming: the field names a subset, omitted from the public projection by key-remapping
class Record_ extends Doc.Class<Record_>('Record_')({
    slug: Schema.NonEmptyTrimmedString,
    owner: Doc.FieldOnly('internal')(Schema.String), // present on internal, dropped from public
}) {}
// reject: a field config naming a channel outside the alphabet resolves to the literal error type at declaration
// const Broken = Doc.Class<...>('Broken')({ x: Doc.Field({ audit: Schema.String }) })
//   -> x's value is `"field must have valid variants"`, surfaced here, never at projection
const project: (v: 'public' | 'internal') => Schema.Schema.Any = Match.type<'public' | 'internal'>().pipe(
    Match.when('public', () => Doc.extract(Record_, 'public')),
    Match.when('internal', () => Doc.extract(Record_, 'internal')),
    Match.exhaustive,
)
```

[ANTICIPATORY_ALPHABET_IS_DECLARED_OPEN_BEFORE_THE_THIRD_CHANNEL]:
- The alphabet is sized at the *second* divergent channel, not the third, because the promotion from `Schema.optionalWith` to a variant alphabet is a cross-owner re-route that breaks every consumer reading the flat field, while widening an already-open alphabet by one variant is one literal appended to the `variants` array. The asymmetry is the whole sizing rule: a flat field that becomes a two-element alphabet on declaration absorbs the third channel as one literal with zero consumer churn, whereas the same field left flat until the third channel arrives forces the cross-owner break at exactly the moment call sites exist. The conservatism reads the field-arity axis the way identity regime is read on shape — pessimistically, one channel before the count demands it.
- A channel added to an open alphabet is one declaration that re-flows the whole cross-product: appending a variant literal widens every field's legal config domain, every projection member, and every derived surface simultaneously, and a field that does not name the new channel is silently absent there (the legal under-naming), so the growth touches only the fields that genuinely diverge on it. The proof of a correctly-sized alphabet is the new-channel diff — one literal in `variants`, plus one config key on each diverging field, and zero edit to any field that does not — against the flat-owner alternative's `C` sibling schemas each re-derived per channel.
- The pre-threshold flat field and the alphabet member are the same construct at two arities, so the anticipatory declaration costs nothing the flat owner saved: a field uniform across every channel stays a plain schema *inside* the alphabet owner (no per-variant config), and only a field that diverges carries a config — declaring the alphabet does not force every field to become a per-variant function, it only admits the ones that must. The over-shaping cost is therefore one unused-but-uniform owner whose fields are plain schemas until one diverges, against the under-shaping cost of re-spelling every projection when the third channel breaks the flat field — the loud/silent asymmetry that licenses sizing the alphabet open early.

```typescript
// anticipatory: the alphabet is declared open, so a fourth channel is one literal, every projection re-flows
const Channels = VariantSchema.make({
    variants: ['read', 'write', 'wire', 'audit'], // the next channel appends one literal here, breaking no consumer
    defaultVariant: 'read',
})
const Slug = Schema.String.pipe(Schema.minLength(1), Schema.brand('Slug'))
class Entity extends Channels.Class<Entity>('Entity')({
    slug: Slug, // uniform across every channel: a plain schema, no per-variant config
    secret: Channels.FieldExcept('wire', 'audit')(Schema.Redacted(Schema.String)), // diverges: named subset only
    revised: Channels.fieldFromKey({ audit: 'revised_at' })(Channels.FieldOnly('audit')(Schema.DateTimeUtc)),
}) {
    get ref(): string {
        return `${this.slug}`
    }
}
```

[THE_ALPHABET_IS_PARTITIONABLE_AND_THE_PARTITION_IS_A_SIZING_SUB_DECISION]:
- A single alphabet has internal *families* — disjoint sub-sets of variant literals that share a representation regime — and the sizing decision is not only the alphabet's cardinality but its partition into families a consumer addresses as a unit. A database-and-json alphabet partitions into a database family (`select | insert | update`) and a json family (`json | jsonCreate | jsonUpdate`), exported as named literal-union sub-alphabets, so a consumer reads the whole family rather than enumerating its members — the partition is the second axis of the alphabet's shape, and an alphabet declared without recognizing its families forces every consumer to spell each member where a family-name would resolve them together.
- The threshold arithmetic is alphabet-agnostic and rides the partition: the in-owner field modifier covers the divergence one decode/encode bracket spans, the alphabet covers every divergence beyond it, and a field's config keys *cluster by family* — a sensitive-value policy names the entire database family and omits the entire json family, a generated-column policy names a sub-set of the database family. The clustering is the signal the alphabet partitions correctly: a field config whose named channels do not respect any family boundary is the smell that the alphabet's families are mis-drawn, because a genuine representation regime is named or omitted as a whole, not per-member.
- The partition is the unit the cross-product collapse operates on, so the per-family modifier is denser than the per-member config: a field admitted on one family and stripped from another is one family-keyed policy, not three member-keyed entries, and the canned column-policy modifiers are exactly family-respecting configs — they name `{ select, update, json }` or `{ select, insert, update }` as families-minus-an-exception rather than enumerating channels. Reaching for the open `Field({ … })` per-member form when a family-keyed modifier exists is the un-collapsed spelling the partition deletes; the open form is reserved for divergence that crosses a family boundary in shape.

```typescript
const Cell = VariantSchema.make({
    variants: ['select', 'insert', 'update', 'json', 'jsonCreate', 'jsonUpdate'],
    defaultVariant: 'select',
})
type Database = 'select' | 'insert' | 'update' // the database family, addressed by the consumer as one unit
class Note extends Cell.Class<Note>('Note')({
    id: Schema.UUID,
    secret: Cell.FieldExcept('json', 'jsonCreate', 'jsonUpdate')(Schema.Redacted(Schema.String)), // omits the whole json family
    body: Schema.String,
}) {}
// a consumer addresses a family, not its members: the partition resolves the sub-set in one name
const dbWire: (v: Database) => Schema.Schema.Any = Match.type<Database>().pipe(
    Match.when('select', () => Cell.extract(Note, 'select')),
    Match.when('insert', () => Cell.extract(Note, 'insert')),
    Match.when('update', () => Cell.extract(Note, 'update')),
    Match.exhaustive,
)
```
