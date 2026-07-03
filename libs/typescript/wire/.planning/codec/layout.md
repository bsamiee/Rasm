# [WIRE_LAYOUT]

`codec/layout.ts` decodes `LayoutConstraintWire` from `Rasm.AppUi/Shell` — an ORDERED Cassowary constraint program the C# solver authored. Order is load-bearing: constraints install into the tableau in program order, strengths resolve ties deterministically only when insertion order is preserved, so the decode lands a `Schema.NonEmptyArray` whose positions are the program. The decoded program is data for the `@lume/kiwi` tableau in `ui/viewer` `panel/layout` — this rail decodes and never solves.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]            | [OWNS]                                                          |
| :-----: | :------------------- | :-------------------------------------------------------------------- |
|   [1]   | `CONSTRAINT_PROGRAM` | the ordered constraint rows, strength/relation vocabularies, decode    |

## [2]-[CONSTRAINT_PROGRAM]

- Owner: `LayoutProgram` — one `Schema.Class`: the ordered constraint rows over two closed vocabularies (`relation`: `le`/`ge`/`eq`; `strength`: `required`/`strong`/`medium`/`weak`) with linear terms as `{ variable, coefficient }` pairs plus a constant per row; edit-variable declarations ride the program head.
- Entry: `LayoutProgram.FromBytes` composing the proto engine; `LayoutProgram.decode(octets)` the one-shot rail; `ui/viewer` `panel/layout` reads the program through `#vocab` and feeds its tableau.
- Receipt: the program is the solver's complete input — variables, edits, ordered constraints; the viewer's solve produces the layout, and a solve receipt flowing back rides `codec/envelope.ts`'s receipt plane, not this rail.
- Growth: a new constraint axis (a priority band beyond the four strengths, a stay-variable row) is a C# program change mirrored as one field or vocabulary row here; the viewer's tableau adapter reads the same program shape.
- Law: order is the program — `Schema.NonEmptyArray` preserves wire order verbatim and the class exposes no sorted view; a consumer that re-orders constraints has changed the program's semantics and owns that decision visibly in its own code.
- Law: the vocabularies are closed at the seam — `relation` and `strength` decode through `Schema.Literal` rows matching the Cassowary contract; a numeric strength passthrough (raw solver weights) is rejected because the four-band vocabulary is the cross-language agreement.
- Law: decode-only — no tableau, no solving, no variable resolution here; TS solving lives in the viewer where `@lume/kiwi` is admitted (`scope:viewer`), and this module's census fence keeps solver imports out of `wire`.
- Boundary: the tableau adapter and solve loop are `ui/viewer` `panel/layout`; control gestures that perturb layout are `codec/control.ts`'s family.

```typescript
import { Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _relations = ["le", "ge", "eq"] as const
const _strengths = ["required", "strong", "medium", "weak"] as const

const _Term = Schema.Struct({
  variable: Schema.NonEmptyString,
  coefficient: Schema.Number,
})

const _Constraint = Schema.Struct({
  relation: Schema.Literal(..._relations),
  strength: Schema.Literal(..._strengths),
  terms: Schema.NonEmptyArray(_Term),
  constant: Schema.Number,
})

class LayoutProgram extends Schema.Class<LayoutProgram>("LayoutProgram")({
  surface: Schema.NonEmptyString,
  edits: Schema.Array(Schema.NonEmptyString),
  constraints: Schema.NonEmptyArray(_Constraint),
}) {
  static readonly FromBytes: Schema.Schema<LayoutProgram, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.LayoutConstraintWire, LayoutProgram)
  static readonly decode: (octets: Uint8Array) => Effect.Effect<LayoutProgram, ParseResult.ParseError> = Schema.decodeUnknown(LayoutProgram.FromBytes)
}

declare namespace LayoutProgram {
  type Relation = (typeof _relations)[number]
  type Strength = (typeof _strengths)[number]
  type Constraint = Schema.Schema.Type<typeof _Constraint>
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { LayoutProgram }
```
