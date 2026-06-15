# Encoded Axis Divergence Stack: One Column's I-Slot Recurses While Its Decoded Interior Stays Fixed

[THE_ENCODED_VIEW_IS_A_RECURSIVE_I_SLOT_READ_THAT_DESCENDS_THROUGH_TRANSFORMS]:
- `Schema.Schema.Encoded<S>` is the one-step variance read `S extends Variance<_A, infer I, _R> ? I`, but the I it reads is itself the fixpoint of a structural descent: a variant struct's encoded AST is computed by `encodedAST_(ast, false)`, which on a `Transformation` node UNCONDITIONALLY returns `encodedAST_(ast.from)` and recurses, so the wire body of a column is the encoded leaf at the BOTTOM of its transform stack, not the leaf one hop down. A `Model.JsonFromString(Schema.Redacted(inner))` db slot is `transform(String, transform(inner, RedactedFromSelf))` — two stacked transformations — and the encoded read peels BOTH, yielding the `string` text leaf, never the intermediate `Redacted<Encoded<inner>>`. The encoded axis is a recursion to the deepest `from`; the decoded axis is the prototype at the top.
- The descent is structure-preserving at every non-transform node: `encodedAST_` rebuilds a `TypeLiteral` by mapping each `PropertySignature`'s `type` through itself, a `TupleType` element-wise, a `Union` member-wise, and threads a `Suspend` through a fresh thunk — so the encoded view of a struct of structs is a struct of encoded structs, computed in one pass with no per-column projection authored. A column whose encoded leaf is a nested object recurses into that object's own property signatures, so the wire body of a `JsonFromString(Struct({ inner: Struct({...}) }))` json slot is the full nested encoded record, the recursion bottoming out only at primitive keywords.
- The decoded interior is INVARIANT across the encoded divergence because it is read off the SAME node from the opposite end: `Schema.Schema.Type<typeof Self.json>` and `Schema.Schema.Type<typeof Self.insert>` of a `JsonFromString` column are the identical `Type<inner>` (the parsed object), while `Schema.Schema.Encoded<typeof Self.json>` is the structured object and `Schema.Schema.Encoded<typeof Self.insert>` is `string`. One leaf, two encoded reads, one decoded read — the divergence is entirely on the I axis, and a consumer comparing decoded interiors across variants compares one type.

[THE_RENAME_AXIS_IS_A_KEY_REMAP_IN_THE_ENCODED_MAPPED_TYPE_NOT_A_RUNTIME_STEP]:
- `Schema.Struct.Encoded<F>` remaps every encoded key through `as Key<F, K>`, and `Key<F, K>` is `F[K] extends PropertySignature.All<infer Key> ? [Key] extends [never] ? K : Key : K` — it reads the THIRD type parameter of a `PropertySignature` (the encoded key) and falls back to the property name `K` only when that parameter is `never`. A `fromKey<S, "<external>">` slot is `PropertySignature<":", Type<S>, "<external>", ":", Encoded<S>, false, Context<S>>` — its third parameter is the foreign name, so `Key<F, K>` resolves to `"<external>"` and the encoded record carries the column under the foreign key while the decoded record carries it under `K`. The rename is a fact of the encoded mapped type's `as` clause, computed at projection, never a remap the consumer writes after decode.
- `fromKey.Rename<S, Key>` is the homomorphism that proves the rename touches ONLY the encoded key: it destructures `PropertySignature<_TypeToken, _Type, _Key, _EncodedToken, _Encoded, _HasDefault, _R>` and rebuilds it with `_Key` replaced by `Key`, every other token slot — TypeToken, EncodedToken, Encoded, HasDefault, R — preserved verbatim. So renaming a slot that is also an `Overrideable` (`HasDefault=true`) keeps its elision, renaming an `optionalWith` slot keeps its `"?:"` encoded token, renaming a service-carrying slot keeps its `R` — the rename composes onto any signature kind without disturbing the membership, default, optionality, or requirement that signature already carries.
- The rename dispatches per variant because `fieldFromKey`'s mapping is `{ readonly [K in keyof S]?: string }` over a `Field<S>`'s existing slots: a mapping naming only `json` rewrites the third parameter of the `json` slot's signature alone, leaving `insert`/`update`/`select` at their canonical `Key=never` fallback. One canonical column carries different encoded keys per variant — the SQL row spells it canonically, the JSON body spells it foreign — with no second model and the decoded key fixed at `K` across all six.

[FOUR_ORTHOGONAL_AXES_COMPOSE_ON_ONE_SLOT_WITHOUT_NESTING]:
- A column's full wire behavior is the product of four independent axes, each living in a different position of the slot's `Field<Config>` row or its `PropertySignature` tokens, none recoverable from another: MEMBERSHIP (does the variant own the key — `V extends keyof Config`), REPRESENTATION (what transform wraps the decoded interior — `Schema.Redacted` versus the bare leaf), TEXT-VS-STRUCTURED (whether the encoded leaf is `string` or the object — `Model.JsonFromString`'s db-versus-json slot schema), and ENCODED-KEY (which name the I-slot's third token carries — `Model.fieldFromKey`). The axes multiply rather than coincide: one column built as `Model.JsonFromString(Schema.Redacted(inner))` then piped through `Model.fieldFromKey` and narrowed by membership sets all four, each read off its own position with no axis subsuming another, so the count of distinct wire bytes one column projects is the product of the variants its membership spans and the representations its slots carry.

```typescript
import { Model } from '@effect/sql';
import { JSONSchema, Schema } from 'effect';

const Inner = Schema.Struct({ region: Schema.NonEmptyTrimmedString });

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.UuidV4Insert(Schema.Uint8ArrayFromSelf.pipe(Schema.brand('Id'))),
    secret: Model.Sensitive(Schema.Redacted(Schema.NonEmptyTrimmedString)),
    payload: Model.JsonFromString(Inner).pipe(Model.fieldFromKey({ json: '<external>', jsonCreate: '<external>' })),
}) {}

type StoreRow = Schema.Schema.Encoded<typeof Owner.insert>;
type WireBody = Schema.Schema.Encoded<typeof Owner.json>;
type CreateWire = Schema.Schema.Encoded<typeof Owner.jsonCreate>;

const contract = JSONSchema.make(Owner.jsonCreate, { target: 'openApi3.1' });
```

- `StoreRow` carries `secret` as the unredacted `string` (the `Redacted` transform's encoded peel) under the canonical key (no `fromKey` for db slots), `payload` as `string` under the canonical key (the `JsonFromString` db leaf), and no `id` overrideable surfaces as a key the writer names — three columns, three encoded shapes off one membership-complete db variant. `WireBody` DROPS `secret` entirely (`Sensitive` omits the json family — membership, not redaction), carries `payload` as the structured object under `"<external>"` (text-vs-structured flipped AND key renamed), and keeps `id` — the same three columns project a categorically different record because three of the four axes diverged between the two variants.
- The axes do not nest because each is a per-slot rewrite of one row, never a field wrapped in a field: `Model.Sensitive` narrows `keyof Config`, `Schema.Redacted` composes a transform onto the slot's leaf, `Model.JsonFromString` writes different leaf schemas per slot family, `Model.fieldFromKey` rewrites the third PropertySignature token — four edits, one flat `Field<Config>` result, the projection reading membership in the key position and representation in the value position in one mapped type. Stacking a fifth modality on the same column is one more per-slot rewrite, never a wrapper chain the consumer must unwind.

[WIRE_ARITY_IS_A_THIRD_AXIS_THE_ENCODED_TOKEN_DISJOINT_FROM_MEMBERSHIP_AND_DEFAULT]:
- The wire body partitions its keys by `EncodedOptionalKeys<F>` — `Fields[K] extends OptionalEncodedPropertySignature ? K : never` reading the FOURTH PropertySignature parameter (the `EncodedToken`) for `"?:"` — a position disjoint from both the membership key-clause (`V extends keyof Config`, dropped-or-present) and the construct-default token (`HasDefault`, elided-or-required at `.make`). So a column has THREE independent wire-relevant arities off one signature: present-or-absent (membership), required-or-optional ON the wire (`EncodedToken`), and required-or-elided IN the constructor (`HasDefault`) — and the encoded view reads only the first two, never the third.
- `Model.FieldOption` is the column that exercises the `EncodedToken` axis across variant families: the db slots are `OptionFromNullOr<inner>` whose encoded leaf is `null | Encoded<inner>` at a `":"` encoded token (a REQUIRED wire key that admits `null`), while the json-create slot is `optionalWith<inner, { as: "Option", nullable: true }>` whose encoded is `Encoded<inner> | null | undefined` at a `"?:"` encoded token (an OPTIONAL wire key admitting `null` AND absence). One `Option<A>` decoded interior, two wire arities — the store row's column is always emitted, the create body's column may be omitted — the divergence riding the encoded token while the type stays fixed.
- The `EncodedToken` axis produces a failure surface categorically distinct from membership-erasure: a `Sensitive` json column is ABSENT from `keyof Schema.Schema.Encoded<typeof Self.json>` (no key), while a `FieldOption` json-create column is PRESENT in that keyof as an optional property — so `keyof` distinguishes "the wire never carries this" from "the wire may omit this," and a consumer building a partial-update body off `Self.jsonUpdate` reads the optional keys as the legitimately-omittable set, never conflating them with the membership-dropped columns that cannot appear at all.

[ENCODEDBOUNDSCHEMA_RETAINS_ONLY_THE_STABLE_FILTER_AND_THE_TRANSFORM_FREE_REFINEMENT]:
- `Schema.encodedSchema(Self.json)` is `make(AST.encodedAST(ast))` and `Schema.encodedBoundSchema(Self.json)` is `make(AST.encodedBoundAST(ast))` — one `encodedAST_` engine, the `isBound` flag the only difference, and that flag changes EXACTLY the `Refinement` arm. With `isBound=false` a `Refinement` always collapses to its encoded `from`; with `isBound=true` it re-wraps the refinement only when `getTransformationFrom(ast.from) === undefined && hasStableFilter(ast)` — both conditions required. The bound form is NOT "keep every refinement on the wire"; it is "keep a refinement whose subject is not a transformation AND whose filter is explicitly flagged stable."
- `hasStableFilter` is true only for filters carrying `[StableFilterAnnotationId]: true`, and that annotation is set on `minItems`, `maxItems`, and `itemsCount` ALONE — the array-length filters. The string filters `minLength`/`maxLength`/`nonEmptyString` carry NO stable annotation, so a `NonEmptyTrimmedString`'s emptiness rejection does NOT survive `encodedBoundSchema`: it is `Trimmed.pipe(nonEmptyString(...))`, an unflagged `minLength(1)` refinement, and the bound read drops it to the bare `string` encoded leaf. A publish path that must reject empty strings before decode cannot rely on the bound form to carry that bound — only an array-length minimum survives the encoded boundary.

```typescript
import { Schema } from 'effect';

const Bounded = Schema.Struct({
    tags: Schema.Array(Schema.String).pipe(Schema.minItems(1)),
    label: Schema.NonEmptyTrimmedString,
});

const lenient = Schema.encodedSchema(Bounded);
const bound = Schema.encodedBoundSchema(Bounded);

type Permissive = Schema.Schema.Type<typeof lenient>;
type StableOnly = Schema.Schema.Type<typeof bound>;
```

- `lenient` strips both bounds — `tags` decodes any array, `label` any string — the permissive wire gate that admits the raw shape and defers refinement to the full decode. `bound` retains the `minItems(1)` on `tags` (a stable-flagged array filter over a non-transform `Array`) yet DROPS the emptiness check on `label` (an unflagged `minLength` whose `nonEmptyString` annotation is absent from the stable set) — so the bound wire schema rejects an empty `tags` array but accepts an empty `label`, the asymmetry a direct consequence of which filters carry the stable annotation. Reject the assumption that `encodedBoundSchema` is "the bounded wire shape": it is the stable-array-filter wire shape, and a string emptiness invariant must be re-asserted on the decoded side or carried by an array filter.
- A refinement OVER a transformation collapses even when stable-flagged, because the first condition `getTransformationFrom(ast.from) === undefined` fails: `getTransformationFrom` recurses `Refinement`/`Suspend` down to a `Transformation` and returns its `from`, so any bound stacked above a `JsonFromString` text transform or a `Trim` is unconditionally dropped by the bound read. The bound form preserves a filter only at the top of a transform-free leaf — the encoded boundary cannot carry a constraint that lives above a representation transform, the recursion peeling the transform taking the bound with it.

[THE_DOWNSTREAM_WIRE_SURFACES_ARE_PURE_READS_OF_THE_ENCODED_AST_NOT_KEPT_MIRRORS]:
- `JSONSchema.make(Self.jsonCreate, { target })` compiles the ENCODED AST of the variant — it internally takes `encodedSchema(schema)` and walks the resulting I-side struct — so every encoded-axis divergence is already baked into the document with no per-field annotation: a `Sensitive` column never appears (absent from the json struct's property signatures), a `fromKey` rename surfaces the property under the foreign key (the `as Key<F, K>` remap is in the AST the compiler reads), a `JsonFromString` json slot emits the structured object schema, and an `optionalWith` create slot emits an optional-plus-nullable property. The `target` selects `"jsonSchema7" | "jsonSchema2019-09" | "jsonSchema2020-12" | "openApi3.1"`; the column manifest is the same encoded read regardless.
- `Schema.standardSchemaV1(Self.json)` returns `StandardSchemaV1<I, A>` whose FIRST type parameter is the encoded `I` — so the foreign validator's declared INPUT shape IS the variant's wire body, every encoded-axis divergence (the renamed key, the structured-versus-text leaf, the optional-on-wire token) surfacing as the input contract a cross-tool consumer validates against, the output `A` the decoded interior. The encoded view is the cross-tool boundary's input type by construction, not a hand-translated DTO handed to the validator. Its admissibility is gated where the requirement closure is owned; the encoded read itself is unconditional.
- `Schema.parseJson(Self.jsonCreate)` is `transform<SchemaClass<unknown, string>, typeof Self.jsonCreate>` — it composes the text seam ONTO the variant, so its OWN encoded leaf is `string` (the request body) while its decoded interior is the create payload: decode `JSON.parse`s the inbound text straight into the decoded type, encode `JSON.stringify`s the projection, and a `ParseJsonOptions` configures both passes in one value. A `JsonFromString` column already carries text at the leaf; `parseJson` carries text at the whole-body seam — the same `string`-encoded transform applied at two altitudes, and a column stored as text inside a body itself parsed from text is two stacked `string` peels the encoded recursion descends through in order.

```typescript
import { Model } from '@effect/sql';
import { Schema } from 'effect';

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    region: Model.JsonFromString(Schema.Struct({ city: Schema.NonEmptyTrimmedString })),
}) {}

const transit = Schema.parseJson(Owner.jsonCreate);
const gate = Schema.standardSchemaV1(Owner.json);

type RequestText = Schema.Schema.Encoded<typeof transit>;
type CreatePayload = Schema.Schema.Type<typeof transit>;
type ValidatorInput = Schema.Schema.Encoded<typeof gate>;
```

- `RequestText` is `string` — `parseJson`'s outer transform makes the whole create body a text seam, so the inbound HTTP body needs no separate parse step before decode; `CreatePayload` is the decoded create object, `region` a structured `{ city }` (the json-variant slot of `JsonFromString`) despite the column being stored as text in the db variants. One owner yields the text-on-the-wire request codec and the structured decoded payload from one `parseJson` of one variant, the encoded leaf `string` at the body altitude and the json object at the column altitude.
- `ValidatorInput` is `Schema.Schema.Encoded<typeof gate>` — and because `standardSchemaV1` returns `StandardSchemaV1<I, A> & SchemaClass<A, I, never>`, this read is the foreign validator's declared input shape: `region` appears as the structured `{ city }` json-variant leaf, the encoded-axis divergence of the column surfacing verbatim as the cross-tool input contract. The encoded view is the universal input type every downstream wire surface shares; the next service-free column lands as one more property `JSONSchema.make`, `parseJson`, and the validator input all absorb at compile time with no mirror respelled. A json leaf carrying an unresolved requirement bars only the lift, not the encoded read — the boundary the closure owns, not the encoded axis.

[THE_INSTANCEOF_LEAF_IS_THE_DEGENERATE_ENCODED_AXIS_WHERE_I_EQUALS_A]:
- The encoded recursion terminates trivially at a leaf whose I EQUALS its A: `Schema.instanceOf(Ctor)` is `AnnotableDeclare<instanceOf<A>, A>` — a `Declaration` node, not a `Transformation`, so `encodedAST_` recurses its type parameters but finds no `from` to descend, leaving the encoded leaf identical to the live instance. A column built from a bare `instanceOf` has NO distinct wire body: its `Schema.Schema.Encoded` re-emits the host object, so the json variant of such a column carries a non-portable reference, and `JSONSchema.make` over a variant containing it fails to produce a serializable property.
- The fix is to pair the host instance with a real `Schema.transformOrFail` whose encoded `from` is the serializable form, which re-introduces a `Transformation` node the encoded recursion DESCENDS to the portable leaf — so a column carrying a non-serializable interior crosses the wire only when its representation axis supplies a transform the encoded read can peel to a primitive. The encoded-axis-divergence stack requires at least one transform in the slot's stack for the wire body to differ from the decoded interior; an `instanceOf` leaf is the degenerate row where the four axes collapse to one shape and the wire body is the live object the encoded recursion cannot serialize.
