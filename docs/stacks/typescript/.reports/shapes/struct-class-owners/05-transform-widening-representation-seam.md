# Transform-Widening Representation Seam

[SCHEMA_SELECTION_SEAM]:
- The two effectful-widening verbs differ in exactly one expression — the schema each hands to `makeClass` — and that one expression is the whole representation seam: `.transformOrFail` builds `transformOrFail(parentSchema, typeSchema(Struct(merged)), options)` and `.transformOrFailFrom` builds `transformOrFail(encodedSchema(parentSchema), Struct(merged), options)`, where `parentSchema` is the parent owner's own codec, `merged` is `extendFields(fields, newFields)`, and `Struct(merged)` is the full merged codec while `typeSchema(Struct(merged))` is its transform-collapsed type face. The verb is not two control paths; it is one `makeClass` call parameterized by which schema occupies the `from` slot and whether the `to` is the merged type face or the merged codec.
- The suffix `From` is a `from`-side binding decision read off the inner transform: the unsuffixed verb binds the inner `from` to the parent's FULL codec (decode sees `Struct.Type<Fields>`, the decoded parent instance shape), the `From`-suffixed verb binds the inner `from` to `encodedSchema(parentSchema)` (decode sees `Simplify<I>`, the parent's encoded record). The `decode` signature the checker enforces is the projection of that binding — `(input: Simplify<Struct.Type<Fields>>, …)` for the type-face verb, `(input: Simplify<I>, …)` for the encoded-face verb — so the function's input type is not a separate declaration but the read-out of which AST face the inner `from` walked to.
- The `to` slot mirrors the `from` choice and is the reason the verbs round-trip differently: the type-face verb's `to` is `typeSchema(Struct(merged))` (already collapsed to the type side, so the merged fields land as a pure decoded shape the transform produces directly), the encoded-face verb's `to` is the full `Struct(merged)` codec (so after the user `decode` yields `I & Struct.Encoded<NewFields>`, the merged struct's OWN per-field transforms run, decoding each field a second admission step). The encoded-face verb therefore composes the user transform with the merged struct's field codecs; the type-face verb does not, because its `to` was pre-collapsed.

```typescript
import { Effect, Schema } from 'effect';

class Raw extends Schema.Class<Raw>('Raw')({ token: Schema.NumberFromString }) {}

// transformOrFail: decode sees the DECODED parent type — token is already `number`
class Scaled extends Raw.transformOrFail<Scaled>('Scaled')(
    { tenth: Schema.Number },
    {
        decode: (parent) => Effect.succeed({ token: parent.token, tenth: parent.token / 10 }),
        encode: (self) => Effect.succeed({ token: self.token }),
    },
) {}

// transformOrFailFrom: decode sees the ENCODED side — token is still the wire `string`
class Tagged extends Raw.transformOrFailFrom<Tagged>('Tagged')(
    { kind: Schema.String },
    {
        decode: (encoded) => Effect.succeed({ token: encoded.token, kind: encoded.token.slice(0, 1) }),
        encode: (self) => Effect.succeed({ token: self.token }),
    },
) {}
```

[DECODE_POSITION_LAW]:
- The verb fixes WHERE in the decode pipeline the new fields are derived, and the position is forced by the inner transform's `from` binding under one fixed parse order: a `Transformation` node decodes `from(i)` first, then the `FinalTransformation`'s `decode`, then `to`. The new owner's outer node is `transform(encodedSide, declaration)` with `encodedSide` being the inner widening transform, so decoding the owner decodes the inner transform first, and the inner transform's `from` decides whether the parent's full field decode has already run when the user `decode` fires.
- `.transformOrFail` runs the new-field derivation AFTER the parent decode: the inner `from` is the parent codec, so `from(i)` fully decodes the parent (every parent field's own transform bottoms out) into `Struct.Type<Fields>` before the user `decode` computes the new fields from that decoded shape — the derivation reads the post-decode representation, so a parent field typed `NumberFromString` is a `number` in the `decode` input, never the wire `string`.
- `.transformOrFailFrom` runs the new-field derivation BEFORE the merged decode: the inner `from` is `encodedSchema(parentSchema)`, so `from(i)` only re-asserts the parent's ENCODED record (transforms erased to their `from` face), the user `decode` computes the new fields from that pre-decode `I`, and only then does the inner `to` = `Struct(merged)` run every field's decode — so the same parent field is the wire `string` in the `decode` input, and the computed field is derived from bytes, not from values.
- The position is the law a computed field's source obeys: a field whose value is a function of the DECODED parent (a sum of decoded numbers, a flag off a parsed enum) is a `transformOrFail` row; a field whose value is a function of the RAW encoded form (a checksum over the wire bytes, a discriminant sliced from a not-yet-parsed string) is a `transformOrFailFrom` row — choosing the wrong suffix feeds the derivation a representation it cannot compute from, and the mismatch surfaces as a type error on the `decode` input parameter, not a runtime drift.

```typescript
import { Effect, ParseResult, Schema } from 'effect';

class Envelope extends Schema.Class<Envelope>('Envelope')({ count: Schema.NumberFromString }) {}

// derive from BYTES: the checksum is over the wire form, so the encoded-face verb feeds `decode` the
// pre-parse `count: string` — running BEFORE the merged `count` decode validates it as a number.
class Sealed extends Envelope.transformOrFailFrom<Sealed>('Sealed')(
    { digest: Schema.String },
    {
        decode: (encoded) =>
            encoded.count.length > 0
                ? Effect.succeed({ count: encoded.count, digest: `<${encoded.count}>` })
                : Effect.fail(new ParseResult.Type(Schema.String.ast, encoded.count)),
        encode: (self) => Effect.succeed({ count: self.count }),
    },
) {}
```

[ENCODED_FACE_INVARIANCE]:
- Both verbs leave the owner's encoded face `I` UNCHANGED — `Class<…, I, …>` carries the parent `I` verbatim through both, unlike `.extend` which widens it to `I & Struct.Encoded<NewFields>` — because the new fields are placed on the inner transform's `to` side, and `encodedAST` on a `Transformation` recurses the `from` exclusively, never reaching the `to`. The new fields thus have no wire representation: they accrue into the constructor type `C` and the instance type `Struct.Type<Fields & NewFields>`, yet `Schema.Schema.Encoded<typeof Owner>` is the parent's record alone.
- The invariance is the whole point of the verbs over `.extend`: a `transformOrFail` field is a computed-on-admission field that is reconstructed every decode and never read from input, so the wire contract is narrower than the in-memory shape by exactly the new-field set. The round-trip is asymmetric by construction — `encode` returns the parent type/encoded shape and the computed field is dropped, `decode` re-derives it — so a field that must NOT travel (a memoized aggregate, a parse-time-stamped derivation, a denormalized index) lands as one transform row instead of a wire field a consumer could forge or a `.extend` field a decoder would demand.
- The encoded-face invariance composes with field-level transforms differently per verb: under `transformOrFailFrom` the inner `to` is the full merged codec, so the merged struct's per-field encoders run on encode, and a NEW field whose schema is itself a transform (a `NumberFromString` computed field) would gain a wire shape were it on the encoded face — but it sits on the merged `to` whose result the user `encode` discards back to `I`, so even a transform-typed computed field stays wire-invisible. Under `transformOrFail` the `to` is pre-collapsed to the type face, so no field encoder ever runs there. Neither verb can leak a computed field to the wire; the difference is only whether the merged field codecs participate at all.

```typescript
import { Effect, Schema } from 'effect';

class Doc extends Schema.Class<Doc>('Doc')({ body: Schema.String }) {}

// the computed field is in the instance type and constructor, ABSENT from the wire — one row replaces
// a wire field a consumer could forge plus a `.extend` decoder that would demand it on input.
class Indexed extends Doc.transformOrFail<Indexed>('Indexed')(
    { wordCount: Schema.Number },
    {
        decode: (parent) => Effect.succeed({ body: parent.body, wordCount: parent.body.split(' ').length }),
        encode: (self) => Effect.succeed({ body: self.body }),
    },
) {
    get isLong(): boolean {
        return this.wordCount > 100;
    }
}
type WireDoc = Schema.Schema.Encoded<typeof Indexed>; // { readonly body: string } — wordCount never serializes
const built = new Indexed({ body: '<x>', wordCount: 1 }); // constructor accrued wordCount: it is constructible
```

[FULL_OWNER_PARITY]:
- Both transform-widened results pass through the identical `makeClass` body that `Schema.Class`/`.extend` use, so each is a FULL owner with `new`/`make`, the `Data.Class` structural `Equal`/`Hash` base, `Self.fields` re-exposing the merged `PropertySignature` record, `Self.ast` as the three-face transform node, and the JSON-Schema surrogate — the effectful transform widens the codec topology without demoting the result to a bare `SchemaClass` codec view the way a top-level `Schema.transformOrFail` over a class would. The widening verb is the only path that adds an effectful step yet keeps the instance owner intact.
- The widening verb is invisible to construction and visible only to decode: both verbs feed the SAME merged record to `makeClass`, which derives `constructorSchema` from `typeSchema(schema)`, so `new Owner({ …, computed })` and `Owner.make` demand and type-side-validate the computed field identically no matter which suffix built the owner — the suffix's entire effect is which representation the DECODE derivation reads, never what construction expects. A caller building in-process supplies the computed field as a plain decoded value and the transform `decode` never runs; the suffix is a decode-path selector that the constructor surface cannot observe.
- The prototype slot `Proto` survives both verbs unchanged — a subclass of a `TaggedError` widened by `transformOrFail` stays `yield*`-able and throwable — and class-body methods on the widened subclass attach to its concrete instance type; the decoded value answers its getters because the outer `decode: i => new this(i, true)` runs the real constructor over the transform's output, reconstructing the method-bearing instance after the new fields were derived. A computed field and a getter that reads it coexist on one widened owner, the field reconstructed at decode and the getter riding the prototype.

[CONTEXTUAL_DERIVATION_CHANNEL]:
- The two transform functions are a requirement source neither structural widening has: `.extend` carries no `decode`/`encode` and so accrues only `R | Struct.Context<NewFields>`, whereas both transform verbs union the function requirements `R2`/`R3` onto the owner's `R`, so a context-dependent derivation — a computed field whose value needs an injected service to produce — is reachable ONLY through the transform verbs and surfaces its requirement on the owner's own channel, provided once at the composition root and never threaded at each construction. The structural verb cannot host a contextual computation; the effectful verb makes it a definition-time aspect of the owner.
- The requirement enters on DECODE, not construction: the `decode: (input, options, ast) => Effect<…, ParseIssue, R2>` runs inside `Schema.decodeUnknown(Owner)` where the owner's context is already provided, so the contextual derivation fires at the admission boundary and the in-process `new Owner({ …, computed })` supplies the field directly with zero requirement — construction stays pure while admission carries the channel. A computed field that needs a service to derive is therefore wire-shaped (absent from `I`, reconstructed under context at decode) and construction-shaped (supplied as a plain value when built in-process), the same field reached two ways from one owner.
- The suffix is orthogonal to the contextual channel but composes with it: a `transformOrFailFrom` derivation requiring a service computes from the raw encoded form under that service, a `transformOrFail` derivation requiring a service computes from the decoded parent under it — so a checksum service over bytes is the encoded-face contextual row and an enrichment service over decoded values is the type-face contextual row, the two axes (representation and requirement) stacking on one widening declaration the loose spelling would split into a decoder, a service call, and a representation conversion.

[COLLAPSE_TRIGGERS]:
- A class owner plus a free `derive(instance)` function that recomputes a field every read, plus the call-site discipline that remembers to call it, is the collapse trigger: the derivation folds into one `transformOrFail` row whose `decode` computes the field once at admission and whose getter reads the stored value — the free deriver, the recompute-on-read cost, and the forgotten-call hazard are the exact spellings the transform-widened owner deletes, the field materialized at the one admission seam.
- A parent owner plus a sibling owner that adds a wire-traveling field via `.extend` plus a downstream guard stripping that field before re-serialize is the collapse trigger when the field is COMPUTED, not received: `.extend` makes the field a wire input a consumer can forge and a decoder must demand, whereas `transformOrFail` makes it wire-invisible and decode-reconstructed — the strip-before-encode discipline and the forgeable input both vanish, the suffix `From` selecting whether the computation reads decoded values or raw bytes.
- Two parallel widened owners — one deriving a field from the decoded parent, one deriving from the raw wire form — built as separate classes with hand-threaded representation conversions are the collapse trigger: they are one parent owner widened by `transformOrFail` and `transformOrFailFrom` respectively, the suffix carrying the representation choice as a verb, never a conversion step the second owner re-spells; the rejected shape is a single owner that decodes the parent, re-encodes it, and re-parses to reach the other representation, the exact double-conversion the encoded-face verb's `from` binding performs once.

[BOUNDARY_DELEGATION]:
- The decode act running `Schema.decodeUnknown(Owner)` over a transform-widened owner is the consuming boundary; the owner declares the widening verb, the new-field derivation, and the representation the derivation reads — every consumer reads the in-memory type (computed field present) and the wire type (computed field absent) from the one declaration, and the `ParseError` from a failed derivation converts at that boundary identically to any field-decode failure, the transform's `decode` reporting a `Transformation` issue under the owner's own parse policy.
