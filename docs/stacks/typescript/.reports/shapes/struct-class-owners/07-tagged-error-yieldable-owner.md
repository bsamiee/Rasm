# Tagged-Error Yieldable Owner

[FOUR_CHANNEL_MEMBERSHIP_FUSION]:
- The owner's `Proto` slot is `cause_.YieldableError`, and that interface is not a single capability but a four-channel polymorphic membership pinned to the instance itself: it carries `[EffectTypeId]: Effect.VarianceStruct<never, this, never>`, `[StreamTypeId]: Stream.VarianceStruct<never, this, never>`, `[SinkTypeId]: Sink.VarianceStruct<never, unknown, never, this, never>`, and `[ChannelTypeId]: Channel.VarianceStruct<never, unknown, this, unknown, never, unknown, never>` — every channel binds the error TYPE to the same `this`, so one declaration makes the instance a member of the `E` alphabet of an `Effect`, a `Stream`, a `Sink`, AND a `Channel` simultaneously, the failure type recovered as `this` in each variance position.
- The `this`-binding in the variance struct is the structural reason the instance type and the failure-channel membership are one value, not two: a `Schema.Class` carrying a `_tag` is a data record; a `TaggedError` carrying the identical `_tag` is that record plus the four variance slots, so the SAME field set is data in one owner and a typed failure in the other, the difference being exactly the `Proto = YieldableError` intersection the seventh `Class` type parameter supplies — `Class<Self, Fields, Encoded, Context, Constructor<Omit<Fields, "_tag">>, {}, cause_.YieldableError>`.
- Because the variance is keyed by unique symbols, an error owner answers `Effect.EffectTypeId` membership AND its own `_tag` discriminant without collision: the `_tag` is a real field on the encoded record while the channel membership rides symbol-keyed prototype slots never serialized — the wire form is the field record, the failure-channel identity is prototype-side, and the two never contend for a key.

[PROTOTYPE_MERGE_COLLAPSE]:
- The yieldable owner's behavior is assembled by ONE prototype spread whose order is load-bearing: `StructuralCommitPrototype = { ...CommitPrototype, ...StructuralPrototype }`, where `CommitPrototype = { ...EffectPrototype, _op: OP_COMMIT }` supplies the four variance slots, the single-shot iterator, `pipe`, and the commit opcode, and `StructuralPrototype` supplies the keys-length-gated structural `[Equal.symbol]` and the `Hash.structure`-seeded `[Hash.symbol]` — the second spread OVERRIDES the first's `[Equal.symbol]`/`[Hash.symbol]`, replacing `EffectPrototype`'s reference-identity comparison (`this === that`, `Hash.random(this)`) with value-structural comparison.
- This override is the collapse that fuses a domain error value with structural identity: a raw `Effect` is compared by reference (two distinct `Effect.fail` values are unequal even with the same error), but a yieldable error owner is compared by its field set because `StructuralPrototype` won the spread — so two `TaggedError` instances with equal fields satisfy `Equal.equals` and key one `HashSet` bucket, the membership semantics of a data record surviving INTO a value that is also a failure-channel member. The spread order, not a separate mixin, is why the error is both yieldable and value-keyed.
- The override is total over the two symbols only: every other `EffectPrototype` slot (`_op`, the iterator, the four type-ids, `pipe`) survives the second spread untouched because `StructuralPrototype` declares only the two equality symbols — so the error keeps its commit opcode and iterator while gaining structural equality, the merge being surgical rather than wholesale replacement.

```typescript
import { Equal, Schema } from 'effect';

// the error owner is BOTH a yieldable failure AND a value-keyed record — StructuralPrototype's
// [Equal.symbol] overrode EffectPrototype's reference identity, so equal fields compare equal.
class Rejected extends Schema.TaggedError<Rejected>()('Rejected', {
    code: Schema.Number,
    detail: Schema.NonEmptyTrimmedString,
}) {
    get retriable(): boolean {
        return this.code >= 500;
    }
}

const sameValue: boolean = Equal.equals(
    new Rejected({ code: 503, detail: '<upstream-down>' }),
    new Rejected({ code: 503, detail: '<upstream-down>' }),
); // true — structural, not reference: the second prototype spread won the equality symbols

// reject: a hand-rolled `class E extends Error` + a parallel `eqError(a, b)` by-field helper +
// a `failE(e): Effect<never, E>` wrapper re-spell, across three surfaces, the structural equality,
// the value identity, and the failure membership the one StructuralCommitPrototype merge already fuses.
```

[COMMIT_BRIDGE_LAW]:
- The `yield*`-ability of the owner is one prototype property and one fiber arm, not a conversion step: the instance bears `_op: OP_COMMIT` (from `CommitPrototype`), and the fiber runtime's evaluation table has the single arm `[OP_COMMIT](op) => internalCall(() => op.commit())`, where the `YieldableError` base defines `commit() { return fail(this); }` — so when a fiber steps onto an error instance it calls the instance's own `commit`, which lifts it into a `Cause.Fail` carrying itself; the error IS an `Effect` node whose evaluation is its own failure.
- The iterator half is `[Symbol.iterator]() => new SingleShotGen(new YieldWrap(this))`, a generator that yields the wrapped instance exactly once then completes — `Effect.gen`'s `fromIterator` drives that generator, receives the `YieldWrap(error)`, and the runtime resolves the wrapped node through the same `OP_COMMIT` arm; the single-shot contract means `yield* error` produces the failure on its first and only step, never a re-entrant yield. The expression `yield* new Rejected({...})` is therefore not sugar over `yield* Effect.fail(e)` — the error value is itself the yielded effect node, the `fail` lift happening inside `commit` at fiber-step time.
- The bridge is why the owner needs no `.toEffect()`/`.fail()` method and no `Effect.fail(e)` wrap at the raise site: the instance flows directly into a generator position because it carries the iterator and the commit opcode, so an `Effect.gen` body raises a domain error by `yield* error` and the typed `E` channel accrues the error's type by the variance struct's `this` binding — one declaration makes the value raisable, typed, and structurally identified.

```typescript
import { Effect, Schema } from 'effect';

class Denied extends Schema.TaggedError<Denied>()('Denied', {
    principal: Schema.NonEmptyTrimmedString,
    scope: Schema.String,
}) {}
class Expired extends Schema.TaggedError<Expired>()('Expired', {
    at: Schema.Number,
}) {}

// the error VALUE is yielded directly — it carries _op: OP_COMMIT and the single-shot iterator,
// so the fiber runs its own `commit()` = fail(this); the E channel accrues `Denied | Expired`.
const authorize = (
    ok: boolean,
    fresh: boolean,
): Effect.Effect<string, Denied | Expired> =>
    Effect.gen(function* () {
        if (!ok) {
            return yield* new Denied({ principal: '<actor>', scope: '<resource>' });
        }
        if (!fresh) {
            return yield* new Expired({ at: 0 });
        }
        return '<granted>';
    });

// reject: `throw new Denied(...)` inside the gen escapes the typed channel into a defect, and
// `yield* Effect.fail(new Denied(...))` re-wraps a value that is ALREADY an effect node — the
// bare `yield* error` is the spelling the commit bridge licenses, the Effect.fail hop deleted.
```

[CAUSE_MESSAGE_OUTSIDE_FIELDS_LAW]:
- The instance type is `Struct.Type<Fields> & {} & YieldableError`, and `YieldableError extends Error`, so `message: string`, `cause?: unknown`, `name: string`, and `stack?: string` are part of the instance type yet absent from `Fields`, from the encoded record `Struct.Encoded<Fields>`, and from the constructor parameter `Struct.Constructor<Omit<Fields, "_tag">>` — the error carries four `Error`-prototype members the codec never serializes, the throwable face living entirely on the inherited `Error` prototype rather than the field record.
- These members are POPULATED at construction by the `Data.Error` base the owner extends: the generated constructor's final `super(props, true)` hands the validated props to `Data.Error`, whose own constructor runs `super(args?.message, args?.cause ? { cause: args.cause } : undefined)` then `Object.assign(this, args)` — so when `message` or `cause` IS declared as a `Schema` field, that field value flows through the native `Error(message, { cause })` options bag into the `Error` slot AND onto the instance as an own property; when it is NOT declared, the native `Error` defaults apply and the field record stays clean of error metadata.
- The dual-population is the seam a declared `message` field straddles, and the declaration decision is a representation choice with two consequences at once: declaring `message: Schema.String` makes the human string a member of `Encoded` and the constructor parameter (it serializes and is supplied at `new`) while ALSO setting the native `Error.message` slot via the super-bag, so the wire form and the throwable face share one source; omitting it leaves the string a prototype-side render that never serializes and never appears in the constructor required-set — the same field name is a wire-carried datum or a derived view depending solely on whether it is declared, never both.
- `cause` declared as a `Schema` field is the controlled chaining seam: its value passes through the `Error(message, { cause })` options bag, so the owner's `cause` becomes the native `Error.cause` the platform's error chain walks AND a serialized field — a declared `cause: Schema.Defect` (or a nested error schema) makes the wrapped origin both inspectable on the throwable face and recoverable from the encoded record, where an undeclared cause leaves `Error.cause` at its native `undefined`.

```typescript
import { Schema } from 'effect';

// `cause` and `message` are DECLARED fields: each flows through the Data.Error super-bag into the
// native Error slot AND serializes — the throwable face and the wire record share one source.
class Wrapped extends Schema.TaggedError<Wrapped>()('Wrapped', {
    operation: Schema.NonEmptyTrimmedString,
    message: Schema.String,
    cause: Schema.Defect,
}) {}

const e = new Wrapped({ operation: '<commit>', message: '<store unavailable>', cause: { code: 'ECONN' } });
const nativeMessage: string = e.message; // set via super(message, { cause }) — real Error.message
const nativeCause: unknown = e.cause; // set via the Error options bag — walked by the platform error chain
const onWire = Schema.encodedSchema(Wrapped); // { _tag, operation, message, cause } — all four serialize

// reject: an undeclared-message owner whose consumer reads `e.message` gets a field-record DUMP from the
// synthesized getter, not a domain string; declaring `message` is the spelling that makes the string a
// first-class wire field instead of a render — the loose alternative is a parallel `describe(e)` helper.
```

[RENDER_FACE_DIVERGENCE]:
- The error owner passes `disableToString: true` to its class builder, the lone kind that does — a plain `Schema.Class` installs a `toString` returning `Identifier({ field: value, ... })`, but the error suppresses that install so the inherited `Error.prototype.toString` (`name: message`) survives, the error rendering as an error rather than a struct dump; the `name` is `tag` (set by `Base.prototype.name = tag` before the class is built), so an unhandled error prints `Tag: <message>` the way every platform error does.
- The synthesized auto-message renders `Reflect.ownKeys(fields)` — the ORIGINAL pre-tag field record, NOT the `_tag`-extended `taggedFields` — through `Inspectable.formatUnknown`, so it deliberately omits `_tag`: the discriminant already rides `name` (`Tag: <fields>` would double-print it), and the render is the field payload alone, the tag and the body partitioned across the two faces rather than concatenated into one string.
- The two render faces are orthogonal aspects of the one owner: `toString`/`name` come from the `Error` prototype the throwable face needs, while `Pretty` and the JSON-Schema surrogate come from the codec face — a yieldable error pretty-prints as `Identifier(...)` through the schema `pretty` annotation yet `toString`s as `Tag: message` through the `Error` prototype, the same instance answering a structural renderer and a throwable renderer from two independent slots, never one conflated string.

[OWNER_SHAPE_VERSUS_CHANNEL_ROLE]:
- The error's OWNER SHAPE is struct-class-owned in full: it is born from a field record (or a `Schema.filter`-refined struct via `HasFields`), it has a validating `new`/`make`, a three-AST codec, structural `Equal`/`Hash`, `Self.fields`, `.extend`/`transformOrFail` widening, and `Schema.suspend` recursion — every owner mechanism the lane legislates applies unchanged because the error IS a `Class`, the `Proto = YieldableError` slot being the only difference from a plain tagged class; a recursive, refined, or widened error is a routine owner declaration, not a special case.
- A subclass of a yieldable error STAYS yieldable because `.extend` threads `Proto` into the subclass return `Class<…, Self, Proto>` and the real prototype chain carries the `Error` base, the four variance slots, the commit opcode, and the iterator — so widening an error owner by a field never strips its `E`-channel membership, and the subclass accrues the new field into the constructor while remaining `yield*`-able, the widening law and the yieldable law composing on one declaration.
- The role of the error VALUE as a member of an `Effect<A, E, R>` `E` alphabet — what `catchTag`/`catchTags` key off the `_tag` to recover, how the closed `E` union is shaped at a function boundary, how the requirement and failure channels compose — is the consuming rail's concern, not the owner's: the owner declares the value, its codec, its identity, and its yieldability; the rail declares which alphabet the value joins and how it is recovered, so the same owner declaration serves every signature that admits its `_tag` into `E` without the owner naming a single one.
- The decode boundary that admits a serialized error (the `causeParse` codec walking a `Cause` tree, the `Schema.decodeUnknown(ErrorUnion)` keying on `_tag`) and the recovery boundary that catches it (`Effect.catchTag` on the `_tag` literal) are the consuming seams; the error owner declares the field record whose `_tag` IS the discriminant both seams read, so a serialized error round-trips and a live error recovers from the one tag the owner injects — the boundary translation and the channel recovery never re-derive the discriminant the owner already carries.

[COLLAPSE_TRIGGERS]:
- A `class E extends Error` plus a sibling `Schema.Struct` mirroring its fields plus a `failE(e): Effect<never, E>` lift plus an `eqError` by-field comparator is the four-surface collapse trigger: they fold into one `Schema.TaggedError` whose `YieldableError` proto makes the value directly `yield*`-able, whose codec serializes the field record, and whose `StructuralCommitPrototype` compares by field — the hand-rolled Error subclass, the parallel codec struct, the fail-lift wrapper, and the equality helper are the exact spellings the one owner deletes.
- Three or more sibling error classes raised and recovered together — caught by one `catchTags` map, decoded from one wire `_tag` field, stored in one failure column — fuse into a `Schema.Union` of `TaggedError` members: the union owns the family codec, the `_tag`-keyed decode dispatch, and the bare-union `E` type, while each member stays independently `yield*`-able; the per-member `is`-guard zoo and the `A | B | C` failure-type restatement are the spellings the union deletes, the next error landing as one member that breaks every total `catchTags` site at compile time.
- An error modeled as a tagged class plus a free `describe(e): string` renderer and a separate `wrap(origin): E` chaining factory is the collapse trigger when the render and the origin belong on the value: declaring `message` and `cause` as fields routes both through the `Data.Error` super-bag onto the native `Error` faces AND the wire record, so `e.message`, `e.cause`, and the serialized form all read from the field declarations — the standalone describer and the chaining factory are the spellings the declared `message`/`cause` fields delete.
- A `TaggedError` whose validity spans two fields paired with a free `assertValidError(e)` guard collapses into a `Schema.filter`-refined struct passed as the error's `fieldsOr`: the refinement gates `new`/`make`/decode of the error identically to any refined owner, so an error that is only constructible when its fields cohere (a range error whose `actual` must fall outside `[min, max]`) carries its own invariant; the free guard and the remember-to-call discipline are the spellings the refined error owner deletes, the cross-field gate riding the same `HasFields` arm a non-error owner uses.
