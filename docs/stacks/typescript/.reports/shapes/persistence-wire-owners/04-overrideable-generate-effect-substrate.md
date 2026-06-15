# Overrideable Generate Effect Substrate: The Auto-Stamp Is The Encode Arm Of One Transform

[THE_STAMP_IS_THE_ENCODE_ARM_NOT_A_FIELD_DEFAULT]:
- The auto-stamp slot is `transformOrFail(from, Union(Undefined, to), { decode, encode })` piped through `propertySignature` then `withConstructorDefault` — and `generate` is bound to the `encode` callback, never the `decode`. `transformOrFail`'s `encode` runs when a value crosses FROM the decoded `to` side TO the persisted `from` side, so the mint fires on the WRITE pass that serializes the row, not on construction and not on read; the persisted column is the effect's output, computed against the live world at the moment the row is encoded.
- The `decode` arm is `_ => options.decode ? ParseResult.decode(options.decode)(_) : ParseResult.succeed(undefined)` — reading a stored row through an overrideable slot yields `undefined` unless a `decode: Schema<ITo, From>` is supplied to reconstitute it. This is the structural reason the timestamp and uuid markers place the overrideable ONLY in the write slots (`insert`/`update`) and a plain non-overrideable schema (`Schema.DateTimeUtc`, the branded leaf) in the read slots: the overrideable cannot itself carry the read shape, because its decode collapses to `undefined`. A marker that placed the overrideable in `select` would decode every read of that column to `undefined`, the slot being a write-only mint, not a round-tripping codec.
- The `encode` callback `dt => options.generate(dt === undefined ? Option.none() : Option.some(dt))` is the entire branch logic: the slot's decoded value (`undefined` when the constructor default fired, the forced value otherwise) selects which `Option` case `generate` receives, and `generate` returns the persisted `From`. There is no second dispatch, no flag, no per-marker override of the mint — every auto-stamped column in the system is this one `encode`-arm closure differing only in the `generate` body, so a current-time stamp and a v4-uuid mint are one substrate with two effect bodies.

```typescript
import { VariantSchema } from '@effect/experimental';
import { Effect, Option, Schema } from 'effect';

const StampSlot = VariantSchema.Overrideable(Schema.String, Schema.String, {
    generate: (forced: Option.Option<string>) =>
        Option.match(forced, {
            onNone: () => Effect.succeed(`${Date.now()}`),
            onSome: (value) => Effect.succeed(value),
        }),
});
```

- `generate`'s two arms ARE the branch the `encode` closure selects: `onNone` is the auto-mint (constructor default fired, `dt === undefined`), `onSome` is the forced path (a value reached the slot). Reject a separate `decode`/`encode` pair authored on a hand-rolled `Schema.transformOrFail` beside the field: the substrate already owns both directions, the `encode` arm pre-wired to dispatch `generate` on the `undefined` sentinel, so the only authored surface is the effect body, never the transform plumbing.

[THE_OPTION_IS_AN_ENCODE_TIME_DISCRIMINANT_NOT_A_VALUE_CHANNEL]:
- `Option.some(dt)` versus `Option.none()` is computed by the runtime test `dt === undefined`, NOT by reading the `Override` brand — the phantom `& Brand<"Override">` is erased before encode runs, so `generate` discriminates purely on the presence of a non-`undefined` decoded slot value. A forced `Model.Override(x)` and a bare `x` (were the type system to admit it) both reach `generate` as `Option.some`; the brand's only job is to make the bare `x` a compile error at the elidable key, never to change the runtime branch.
- The `Option` is the substrate's sole input to the mint: `generate` receives no row context, no sibling-field values, no parent instance — it sees only `Option<ITo>`, the forced-or-absent decoded value of its OWN slot. A mint that must read another column's value cannot be an `Overrideable`; the substrate's input arity is exactly one optional self-value, so cross-field derivation routes through a post-encode transform on the variant struct, never through the slot's `generate`.
- The forced value arrives as `ITo` (the ENCODED type of the `to` schema), not `To` (the decoded type): `generate: (_: Option.Option<ITo>) => Effect<From, ParseIssue, R>`, and the `encode` closure passes `dt` — the already-encoded slot value — into the `Option`. So a mint that inspects the forced value sees its wire-encoded form, and a `generate` body comparing the forced value against a decoded shape must re-decode it; the substrate hands the effect the encoded leaf, the symmetric point to its returning the persisted `From`.

[THE_REQUIREMENT_IS_A_THREE_SOURCE_UNION_NOT_THE_GENERATE_R_ALONE]:
- The overrideable's `R` is `RFrom | R` where `RFrom` is the `from` schema's OWN context and `R` is `generate`'s effect requirement — `Overrideable<From, IFrom, RFrom, To, ITo, R>(from: Schema<From, IFrom, RFrom>, to: Schema<To, ITo>, { generate: (_) => Effect<From, ParseIssue, R> }) => Overrideable<To, IFrom, RFrom | R>`. Three independent surfaces can each inject a service: the `from` codec, the optional `decode: Schema<ITo, From>`, and the `generate` effect, their requirements unioned by the underlying `transformOrFail<From, To, RD | RE>` and the `propertySignature` lift. A write variant acquires a non-`never` requirement from ANY of the three, so a `from` schema reading a service contaminates the slot's `R` even when `generate` is pure.
- `propertySignature(transformSchema)` sets the slot's `R` to `Schema.Context<transformSchema>` and `HasDefault` to `false`; `withConstructorDefault` then flips `HasDefault` `false → true` while leaving `R` UNTOUCHED — its signature is `<TypeToken, …, R>(self: PropertySignature<…, boolean, R>) => PropertySignature<…, true, R>`. The constructor-default combinator is requirement-transparent: the elision the slot gains and the service the slot carries are set by two different combinators in the pipe, so no amount of defaulting can discharge or add an `R`. The requirement is a property of the transform, the elision a property of the terminal pipe step.
- The built-in stampers are declared `Overrideable<…, …, never>` because their `from` is a context-free leaf (`Schema.String`, `globalThis.Date`, `number`, `Uint8Array`), their `generate` mints from the ambient clock or entropy the platform already provides, and they pass no `decode` — all three R-sources resolve `never`, so the library's auto-stamp adds nothing to any variant's requirement. A bespoke slot whose `from` is `Schema<From, IFrom, SomeService>`, or whose `generate` reads a tagged service, is the only construction that lifts a write variant off `never`, and the union means closing ONE source does not close the slot if another remains open.

[OVERRIDE_BYPASSES_THE_KEY_BUT_THE_EFFECT_STILL_FIRES]:
- Forcing a value through the elidable key sets the encode arm's `dt` to that value, so `generate(Option.some(x))` STILL RUNS on the write pass — `Override` reaches the constructor key, never the `encode` arm. The forced value is the effect's INPUT, not a replacement for the effect: `generate`'s `onSome` arm validates, transforms, or re-mints `x`, so the substrate's encode behavior on a forced row is "run the mint with `some(x)`," not "store `x` raw." `Override` is a "feed the mint a value" escape, not a "skip the mint" escape, and a slot whose `onSome` ignores its argument forces every write to a fresh mint regardless of the value passed.
- Because the effect fires whether the default or the forced path is taken, `Override` cannot satisfy a non-`never` requirement: the slot's schema type is unchanged by the value passed, so the variant codec still declares `RFrom | R` in its `Context` and the encode still demands the service be provided. A write whose stamper reads `SomeService` requires `SomeService` at the composition root regardless of whether every row forces its value — the requirement is on the codec, never on the absence-of-a-forced-value, the sharpest separation between the construct-time optionality and the encode-time requirement.
- The effect can FAIL: `generate` returns `Effect<From, ParseResult.ParseIssue, R>`, so a forced value that the `onSome` arm rejects, or an ambient read that errors, surfaces as a `ParseIssue` (a `Transformation` node in the issue tree) at the variant's identifier — `Model.Override(x)` does not make the write infallible, it makes the write feed `x` into a fallible mint. The auto-stamp path and the forced path share one failure channel, the issue rendered under the same `"${owner}.insert"` title either way.

```typescript
import { VariantSchema } from '@effect/experimental';
import { Context, Effect, Option, ParseResult, Schema } from 'effect';

class Clock extends Context.Tag('Clock')<Clock, { readonly now: Effect.Effect<string> }>() {}

const SeqSlot: VariantSchema.Overrideable<string, string, Clock> = VariantSchema.Overrideable(
    Schema.String,
    Schema.String,
    {
        generate: (forced: Option.Option<string>) =>
            Option.match(forced, {
                onNone: () => Effect.flatMap(Clock, (c) => c.now),
                onSome: (value) =>
                    value.length > 0
                        ? Effect.succeed(value)
                        : Effect.fail(new ParseResult.Forbidden(Schema.String.ast, value, 'empty stamp')),
            }),
    },
);
```

- `SeqSlot` proves the three claims at once: `Clock` rides the slot's `R` purely through `generate` (the `from` schema `Schema.String` is `never`), so the write variant holding `SeqSlot` carries `Clock` in its `Context` and demands provision at the root; `onSome` runs on a forced `Model.Override(value)`, so forcing feeds the effect rather than skipping it; and `onSome`'s `Effect.fail(new ParseResult.Forbidden(...))` shows the mint can reject a forced value with a `ParseIssue`, the auto-stamp's failure rail identical to the forced path's. Reject a sibling validator beside the field that pre-checks `value.length`: the substrate's `generate` already owns the encode-time guard, the rejection a `ParseIssue` the variant codec raises, not a second pass.

[CONSTRUCTORDEFAULT_AND_GENERATE_ARE_TWO_DEFAULTS_AT_TWO_PHASES]:
- The substrate carries two distinct default mechanisms that must not be conflated: `constructorDefault` (typed `() => To`, default `constUndefined`) fills the CONSTRUCTOR key when the slot is omitted at `.make`, while `generate(Option.none())` computes the PERSISTED value at encode. The chain `withConstructorDefault(options.constructorDefault ?? constUndefined)` wires the constructor default, and the encode arm wires the mint — two phases, two functions, one slot.
- `constUndefined` is the load-bearing constructor default precisely because it threads `undefined` through to the encode arm's `dt === undefined` test, firing `generate(none)`: omitting the key produces `undefined` at construct time, which the encode pass reads as "no forced value, mint it." A `constructorDefault` returning a concrete `To` would instead make the constructor supply that value, so `dt` would be non-`undefined` and `generate(some)` would fire on EVERY write — the default value becoming a perpetual forced override. The two defaults compose: the constructor default decides what reaches the encode arm, the encode arm decides what reaches the store, and `constUndefined` is the identity that keeps the mint, not the constructor, authoritative.
- A non-`constUndefined` `constructorDefault` is the construct for a column whose constructor should pre-supply a stable seed yet still pass through the `generate` mint — a `constructorDefault` returning a sentinel that `generate`'s `onSome` arm rewrites — but it loses the elision's auto-mint semantics: the key is still elidable (still `HasDefault=true`), the constructor still fills it, and `generate` always sees `some`. The default is `constUndefined` for every shipped stamper because the intended semantics are "omit at construct, mint at encode," and any other `constructorDefault` converts the slot to "always forced," a different column entirely.
