# No-instantiation owner law

[OWNER_IS_A_SCHEMA]:
- The class-as-both-type-and-value owner satisfies `Class<Self, Fields, I, R, C, Inherited, Proto> extends Schema<Self, Simplify<I>, R>`, so the class value IS a `Schema` and the constructor is the ONE extra surface layered atop it — every producer keyed off a `Schema<A, I, R>` (`decodeUnknown`, `decode`, `validate`, `encode`, `is`, `asserts`, `typeSchema`, `Schema.equivalence`, `Arbitrary.make`) consumes the class through its `Schema` face with no `new` in reach. The instantiation link is not a peer of these producers but a sibling the `Schema` face never needs: the owner names the shape once, and the shape's whole producer family reads the same value.
- The decisive consequence: a class consumed purely as a type-and-codec source is referenced by exactly two spellings the owner already carries — `Schema.Schema.Type<typeof Owner>` for the erased decoded type and `Schema.decodeUnknown(Owner)` for the boundary producer — and a `new Owner(...)` that follows either is a third reference to a value the two already fixed. The no-instantiation law is the observation that the constructor is the only owner surface a boundary-fed concept never touches, so its appearance after a decode is positive evidence of a re-grown chain link.
- The reach gap that makes the law decidable: the constructor surface lives only on the `Class` interface (the `new (props, options?)` and the static `make`), never on the bare `Schema` the producers accept, so a function typed `(s: Schema<A, I, R>) => ...` structurally cannot instantiate its argument — passing the class where a `Schema` is expected erases the constructor from view, and the only way `new` re-enters is a direct reference to the class value by its own name. The instantiation link is reachable solely through the owner's identifier, never through any producer it is handed to.

```typescript
import { Schema } from 'effect'
class Owner extends Schema.Class<Owner>('Owner')({
    code: Schema.String,
    weight: Schema.Number.pipe(Schema.positive()),
}) {
    get doubled(): number {
        return this.weight * 2
    }
}
const admit = Schema.decodeUnknown(Owner)
type Decoded = Schema.Schema.Type<typeof Owner>
```

[PRODUCER_LATTICE]:
- The owner's value-producing surface is a lattice with two axes, not a flat menu: the INPUT FACE the producer consumes (`unknown` at the wire, `I` already in wire shape, `A` already in memory, or constructor parts `C`) crossed with the OUTPUT CHANNEL it routes failure through (the `Effect` rail, an `Either`, an `Option`, a throw, a `u is A` refinement). `decodeUnknown` is `unknown → Effect<A, ParseError, R>`, `decode` is `I → Effect<A, ParseError, R>`, `validate` is `unknown → Effect<A, ParseError, R>` reading only the Type face, and `new`/`make` is `C → A` total — one cell each in the same grid, all yielding `A` from the one owner.
- The input-face axis is the seam selector: `decodeUnknown` and `decode` cross the boundary (they run the `I → A` transformation), `validate`/`is`/`asserts` read the Type face WITHOUT the transform (they re-admit a value already in `A` shape), and `new`/`make` constructs from parts that never were either wire or `A`. A value's face decides which column it can enter — a wire blob enters only through `decodeUnknown`, an already-decoded `A` re-validated enters through `validate`, and known-valid parts enter through `new` — so the producer is read off the value's provenance, never chosen by preference.
- The output-channel axis is the requirement gate: the sync and `Option`/`Either` cells constrain `R = never` (`decodeUnknownSync: <A, I>(schema: Schema<A, I, never>)`, `decodeUnknownOption`, `decodeEither`), because a synchronous or value-returning producer has no fiber to discharge a service through, while the `Effect` cells keep `R` general. An owner whose fields carry effectful transformations is therefore reachable from `unknown` only through the `Effect`-channel cell — the sync cell does not type-check against it — so the channel a producer offers is not interchangeable with another; it is fixed by whether the owner's `R` is `never`.
- The lattice subsumes the bare `is`/`asserts` row as the zero-output cell: `is: <A, I, R>(schema) => (u: unknown) => u is A` produces no value, only a refinement, and `asserts` produces `asserts u is A`, so the owner yields a type guard from the same declaration the codec comes from — the boundary check and the type narrowing are two cells of one grid, never a hand-written `typeof`-chain predicate beside the owner.

```typescript
import { Schema } from 'effect'
class Packet extends Schema.Class<Packet>('Packet')({
    id: Schema.String,
    size: Schema.Number.pipe(Schema.nonNegative()),
}) {}
const admit = Schema.decodeUnknown(Packet)
const isPacket = Schema.is(Packet)
const guard = (u: unknown): number => (isPacket(u) ? u.size : 0)
```

[REQUIREMENT_THREADING_SPLIT]:
- The sharpest divider between the decode producer and the construct producer is the `R` channel: `decodeUnknown(Owner)` returns `Effect<A, ParseError, R>` where `R = Schema.Struct.Context<Fields>` accumulates every requirement a transformation field declares, while `Owner.make`/`new Owner` returns `A` synchronously and discharges NOTHING — the constructor type is `new (props: ...) => Struct.Type<Fields> & Inherited & Proto`, a total function with no `R` in its result. A field decoded through a service-requiring transform forces its concept through the decode cell, because the construct cell cannot run the transform and the type proves it: the value the constructor produces is the decoded shape minus the transformation the boundary owns.
- This makes the no-instantiation law mandatory rather than stylistic for any owner with effectful fields: where the wire `I` differs from the decoded `A` by a transformation requiring `R`, `new Owner({ ...A-shaped... })` cannot be reached from a wire value at all — there is no `A` to pass until the decode has run — so the only producer that turns the wire blob into `A` is `decodeUnknown`, and a `new` following it re-runs no transform and re-discharges no service; it copies an instance the decode already minted. The construct cell is structurally unreachable from the wire when `R ≠ never`.
- The inheritance verbs widen the requirement monotonically: `.extend<Next>('Next')(NewFields)` returns `Class<Next, Fields & NewFields, I & Struct.Encoded<NewFields>, R | Struct.Context<NewFields>, ...>`, so a child adding a service-requiring field unions its `R` into the owner's, and the child's `decodeUnknown` carries the merged requirement while the child's `new` still discharges none. The requirement-threading split is preserved through inheritance: the decode face of an extended owner is its full transformation pipeline, the construct face is the merged field record minus every transform — the same two faces, accreted.

[SEAM_DECIDES_WHETHER_NEW_IS_DEAD]:
- The law that governs when `new` is a dead chain-link reduces to one question about the value's seam: did it cross the boundary. A value arriving from `unknown` crosses the boundary, so `decodeUnknown` is its sole producer and the decoded instance is the construction — a `new Owner(decoded)` after it re-runs the field refinements on a value the decode already proved valid, the instantiation-after-decode link. A value assembled in interior code from parts that never left the program crosses no boundary, so it has no decode to produce it and the `new`/`make` IS the construction, not a restatement.
- The corollary that catches the subtle case: a value already in `A` shape but of untrusted PROVENANCE (an `A` that was deserialized elsewhere, a cached instance whose invariants may have drifted) is re-admitted through `validate(Owner)`, NOT `new Owner(...)` — `validate` reads the Type face and re-checks the refinements against the in-memory `A` without re-running the `I → A` transform, exactly the producer for "I hold an `A` but do not trust it". Routing such a value through `new` discards the re-check (the constructor validates the constructor-argument fields, but the value is already a full `A`, so passing it as parts is both a restatement and the wrong face); routing it through `decodeUnknown` re-runs a transform the `A` already passed. The three producers partition the provenance space with no overlap.
- The selection therefore has exactly three terminal answers, one per provenance: from-the-wire → `decodeUnknown` (the only cell that transforms), in-memory-untrusted-`A` → `validate` (re-check, no transform), interior-known-parts → `new`/`make` (the genuine construction). A `new` is the dead link precisely in the first two cases and the live construction only in the third — so the law is not "avoid `new`" but "reach for `new` only when the value's provenance is interior parts," and its appearance after a decode or beside a `validate` is the chain link the seam analysis deletes.

[INTERIOR_IDENTITY_IS_THE_LIVE_LINK]:
- The construction link is alive — not a restatement — exactly when the owner's reach is interior structural identity rather than boundary admission, and the owner shape that names this is the codec-free producer: a `Data.TaggedClass` value built by `new` carries `Equal`/`Hash` but no decode, so there is no `decodeUnknown` cell that could have produced it and the `new` is the one and only producer. The no-instantiation law and the codec-free-owner choice are the same decision read from two directions: a concept whose values only ever originate in interior code chooses the codec-free owner AND keeps its `new`, while a concept whose values originate at a boundary chooses the codec owner AND deletes its `new`.
- The mixed owner is where the law earns its edge: a `Schema.Class` (codec included) whose values sometimes arrive from the wire and sometimes are built interior carries BOTH live producers — `decodeUnknown` for the wire arrivals and `new`/`make` for the interior ones — and the chain link is dead per-call-site, not per-owner. The same owner's `new` is the live construction at an interior assembly site and the dead restatement at a site that already decoded; the law is applied to each producer call against the value it produces, never declared once for the whole owner.
- The interior `new` retains the owner's full instance surface that a re-decode would also yield: the static `make<C extends new (...args) => any>(this: C, ...args): InstanceType<C>` threads the concrete subclass through `this`, so a class extended with body getters returns the extended instance from `make`, not a bare struct — the live interior construction carries the same methods the decoded instance does, which is why an interior value built by `new` and a wire value produced by `decodeUnknown` are interchangeable downstream and the choice between them is purely provenance, never capability.

[TYPE_ONLY_CONSUMPTION_FACES]:
- The owner consumed as a pure type source — no value produced at all — is the limit of the no-instantiation law: `Schema.Schema.Type<typeof Owner>` and `Schema.Schema.Encoded<typeof Owner>` lift the decoded and wire types with zero runtime presence, so a function annotated `(x: Schema.Schema.Type<typeof Owner>) => ...` names the shape the owner fixes without referencing the owner value, and neither a `new` nor a parallel `interface` follows. The erased extraction is the apex consumption: the owner is touched only at the type level, and the value side of the owner is never reached at the consumption site.
- The serialization protocols consume the owner type-only through a parallel extractor family: `Serializable.Type<T>`, `Serializable.Encoded<T>`, and `Serializable.Context<T>` lift the wire-protocol faces from an owner used as an RPC or persistence shape, so a transport that round-trips the owner reads `Serializable.Encoded<typeof Owner>` for the wire contract and never instantiates — the owner names the message shape, the protocol's own codec produces the values at the seam. The decode-side producer and the serialize-side extractor are the boundary's two type-only reads off one owner.
- The guard face derives a refinement type from the owner with no construction: `Schema.Schema.ToAsserts<S> = (input: unknown, options?) => asserts input is Schema.Schema.Type<S>` is the type of `Schema.asserts(Owner)`, so an assertion function's signature is computed from the owner's Type face — annotating a boundary guard `const assertOwner: Schema.Schema.ToAsserts<typeof Owner> = Schema.asserts(Owner)` names the owner once as a type and once as the producer of the guard, never a third `new` and never a hand-spelled `asserts u is Decoded` restating the Type face the extractor already yields.

```typescript
import { Schema } from 'effect'
class Message extends Schema.Class<Message>('Message')({
    channel: Schema.String,
    payload: Schema.Unknown,
}) {}
const assertMessage: Schema.Schema.ToAsserts<typeof Message> = Schema.asserts(Message)
type Wire = Schema.Schema.Encoded<typeof Message>
```

[ENCODE_HAS_NO_CONSTRUCTOR_ANALOGUE]:
- The construct cell occupies only one quadrant of the lattice — it produces `A` from parts — so every cell that produces `I` is a producer the constructor can never stand in for: `encode(Owner): (a: A) => Effect<I, ParseError, R>` runs the `A → I` transformation to put a value on the wire, and `new`/`make` has no return that is `I`. A concept that must serialize reaches the encode cell, and a hand-built wire object minted beside the owner to feed a transport is the parallel value the encode producer derives — the encode-then-no-construct asymmetry is the proof the lattice is two-directional while the constructor is single-directional.
- The decode and encode cells are the owner's bidirectional boundary pair and neither has a constructor twin: `decodeUnknown` is `unknown → A`, `encode` is `A → I`, and the round-trip a transport needs is the two cells composed, with `new` appearing at neither end — a value arriving from the wire is decoded, a value leaving for the wire is encoded, and any `new Owner(...)` between them produces an `A` that the decode already produced or the encode is about to consume, the dead link on either side of the round-trip.
- The construct cell also carries no requirement to discharge on the wire-bound side: because `new`/`make` produces `A` synchronously and `encode` carries the same `R` the decode does, an owner with effectful transformation fields can be encoded only through the effectful cell, so a serialization path is forced into the `Effect` channel exactly as the admission path is — the constructor's totality is precisely what makes it unable to participate in either boundary direction when the transformation needs a service.

```typescript
import { Schema, Effect } from 'effect'
class Record extends Schema.Class<Record>('Record')({
    key: Schema.String,
    at: Schema.DateTimeUtc,
}) {}
const serialize = (r: Record): Effect.Effect<Schema.Schema.Encoded<typeof Record>, never> =>
    Schema.encode(Record)(r).pipe(Effect.orDie)
```

[FAILURE_SURFACE_OF_THE_DEAD_LINK]:
- The dead instantiation link does not error — it silently doubles work, which is its hazard: `Schema.decodeUnknown(Owner)(wire).pipe(Effect.map((a) => new Owner(a)))` type-checks because the decoded `a` is a valid constructor argument, but the `new` re-runs every field refinement the decode already ran, so the cost is two validation passes for one value and the second pass can even fail differently if a construction-only default fired on the first — the link is invisible to the checker and visible only as redundant runtime validation. The repair is deleting the `Effect.map`: the decode already produced the instance.
- The inverse hazard is reaching for `validate` or `decodeUnknown` where the value is interior known parts: routing constructor parts through `validate(Owner)` forces them through the Type-face checker as an `unknown`, losing the constructor's defaulting (the constructor supplies `withConstructorDefault` fields the validate path cannot) and paying a parse traversal where a direct `new` would validate the same fields once and inject the defaults — so the over-reach toward the boundary producer for an interior value is the symmetric defect to the dead `new`, and both are read from the value's provenance, not the producer's name.
- The requirement-channel hazard surfaces only at the composition root: a `new Owner(...)` that bypasses a decode also bypasses the `R` the decode would have demanded, so an owner whose fields require a service constructs cleanly in interior code while its `decodeUnknown` would have forced the service into the requirement set — an interior construction that SHOULD have gone through decode (the value actually came from the wire) hides a skipped boundary admission, and the missing `R` in the surrounding `Effect`'s requirement set is the only trace. The dead link's deepest cost is not the double validation but the silently un-discharged transformation when `new` is reached for a value that crossed a boundary it pretends it did not.
