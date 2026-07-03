# [WIRE_CONTROL]

`codec/control.ts` decodes `ControlIntentWire` from `Rasm.AppUi/Shell` — the kind-discriminated control vocabulary the host shell emits toward viewer panels. The wire ships its cases under a foreign `kind` field; the decode attaches the interior `_tag` at the declaration so the interior dispatches one closed tagged family while the wire stays exactly as the provider ships it, and `ui/viewer` `panel/control` consumes the decoded intents through `#vocab` with compile-checked exhaustive dispatch.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                            |
| :-----: | :-------------- | :---------------------------------------------------------------------- |
|   [1]   | `INTENT_FAMILY` | the kind-discriminated intent union and its decode surface               |

## [2]-[INTENT_FAMILY]

- Owner: `ControlIntent` — one `Schema.Union` over the closed intent cases, each member a struct piping `Schema.attachPropertySignature("_tag", "<case>")` so the discriminant exists only on the decoded side and encode drops it; bound to one same-name `const`-plus-`type` pair with the decode statics assembled beside it.
- Entry: `ControlIntent.FromBytes` the composed byte schema; `ControlIntent.decode(octets)` the one-shot rail; consumers dispatch `Match.tagsExhaustive` over the decoded family and break loudly on a new case.
- Growth: a new shell intent is a C# case plus one union member here — the attached tag names it, and every exhaustive panel dispatch breaks until its arm exists.
- Law: the family is closed and kind-keyed — `orbit`, `pan`, `select`, `section`, `measure`, `focus` are the shipped cases; the wire's `kind` field is the foreign discriminant, the attached `_tag` is the interior one, and exactly one spelling of case identity exists on each side of the seam.
- Law: intents are commands-in-flight, not state — a decoded intent dispatches once to its panel arm; holding intents as a state cell re-derives the shell's own state machine and is rejected.
- Law: payload precision is per-case — each case carries exactly its axes (an orbit carries angular deltas, a section carries a plane) and a shared wide payload record is the erased-family defect.
- Boundary: panel behavior per intent is `ui/viewer` `panel/control`; the layout solver program a `section` interacts with is `codec/layout.ts`'s decode; gateway-dispatched COMMANDS (verbs with receipts) are `gateway/command.ts` — intents are fire-and-forget shell gestures, commands are receipted operations.

```typescript
import { Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "./proto.ts"

const _Vec3 = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)

const _orbit = Schema.Struct({ kind: Schema.Literal("orbit"), yaw: Schema.Number, pitch: Schema.Number }).pipe(
  Schema.attachPropertySignature("_tag", "Orbit"),
)
const _pan = Schema.Struct({ kind: Schema.Literal("pan"), dx: Schema.Number, dy: Schema.Number }).pipe(
  Schema.attachPropertySignature("_tag", "Pan"),
)
const _select = Schema.Struct({ kind: Schema.Literal("select"), targets: Schema.Array(Schema.NonEmptyString), additive: Schema.Boolean }).pipe(
  Schema.attachPropertySignature("_tag", "Select"),
)
const _section = Schema.Struct({ kind: Schema.Literal("section"), origin: _Vec3, normal: _Vec3 }).pipe(
  Schema.attachPropertySignature("_tag", "Section"),
)
const _measure = Schema.Struct({ kind: Schema.Literal("measure"), from: _Vec3, to: _Vec3 }).pipe(
  Schema.attachPropertySignature("_tag", "Measure"),
)
const _focus = Schema.Struct({ kind: Schema.Literal("focus"), target: Schema.NonEmptyString }).pipe(
  Schema.attachPropertySignature("_tag", "Focus"),
)

const _intent = Schema.Union(_orbit, _pan, _select, _section, _measure, _focus)

declare namespace ControlIntent {
  type Intent = Schema.Schema.Type<typeof _intent>
  type Tag = Intent["_tag"]
}

const ControlIntent: typeof _intent & {
  readonly FromBytes: Schema.Schema<ControlIntent.Intent, Uint8Array>
  readonly decode: (octets: Uint8Array) => Effect.Effect<ControlIntent.Intent, ParseResult.ParseError>
} = Object.assign(_intent, {
  FromBytes: ProtoCodec.family(ProtoCodec.suite.ControlIntentWire, _intent),
  decode: Schema.decodeUnknown(ProtoCodec.family(ProtoCodec.suite.ControlIntentWire, _intent)),
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { ControlIntent }
```
