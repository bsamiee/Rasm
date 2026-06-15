# Transform Invertibility Law

[THE_DECODE_LEG_RETURNS_THE_ENCODED_OF_THE_FAR_SIDE]:
- The strict `decode` is typed `(fromA: Type<From>, fromI: Encoded<From>) => Encoded<To>` and the strict `encode` `(toI: Encoded<To>, toA: Type<To>) => Type<From>` — the user function does NOT land on `Type<To>`, it lands on `Encoded<To>`, so the composite codec is `to.decode ∘ userDecode ∘ from.decode` and `from.encode ∘ userEncode ∘ to.encode`: the seam is a RE-DECODE point, the user leg producing the wire shape of the far side which `to` then validates and decodes again, never the decoded value itself.
- The consequence is that a transform cannot smuggle an invalid far-side value past `to`'s validation — a `decode` returning a malformed `Encoded<To>` fails inside `to`, not at the seam, so the seam's correctness obligation is reduced to producing a structurally-correct wire shape and the value invariants are re-checked by the owner the transform targets; the transform body is total over wire SHAPE, the refinement total over wire VALUE, the two obligations split across the two schemas rather than conflated in one leg.
- `decode` receiving BOTH `fromA` (decoded source) and `fromI` (the raw encoded input) is the asymmetry that makes a transform branch on the wire form while computing from the decoded form — a checksum read off `fromI` while the payload is read off `fromA` — and `encode` receiving both `toI` and `toA` symmetrically; the four-argument seam carries the full phase context so no leg re-decodes to recover a value the pipeline already holds.

```typescript
import { ParseResult, Schema } from 'effect'
const Stamped = Schema.transformOrFail(
    Schema.Struct({ raw: Schema.String }),                                  // From: wire payload
    Schema.Struct({ raw: Schema.String, len: Schema.NonNegativeInt }),      // To: re-validates `len`
    {
        decode: ({ raw }, _opts, _ast, fromI) =>                            // fromI is the original encoded input
            ParseResult.succeed({ raw, len: fromI.raw.length }),            // returns Encoded<To>, `to` re-decodes + checks NonNegativeInt
        encode: ({ raw }) => ParseResult.succeed({ raw }),                  // total: drops the derived field, rebuilds From
        strict: true,
    },
)
```

[THE_INVERTIBILITY_LATTICE_HAS_THREE_RUNGS]:
- Every transform sits on one of three rungs of an invertibility lattice the type system cannot name but the codec enforces: ISO (`decode` and `encode` are mutual inverses both ways — a key rename, a literal-pair table, a units conversion), SECTION (`decode ∘ encode = id` on `Type<To>` but `encode ∘ decode ≠ id` on `Encoded<From>` — a normalizing projection whose decoded values are a strict subset of representable wire values), and the rejected RETRACTION-WITHOUT-WITNESS (a `decode` that genuinely discards information with no total `encode` to rebuild a representative) — the codec admits the first two and refuses the third at the type level by demanding a total `encode`.
- The rung is recoverable from the SHAPE of `To`, not from inspecting the leg bodies: a `To` equal to a plain leaf schema is an iso candidate, a `To` that is a `filter`/`refine`/`OptionFromSelf`/`Trimmed`-style canonical subset is a section (the refinement IS the canonical-form witness the encode targets), so reading whether the far side is refined is reading whether the round-trip is exact or merely a retraction onto the refined image — the invertibility class is data on the `To` node, never a comment on the functions.
- An iso transform's two legs each have the OTHER as inverse, so swapping `from`/`to` and `decode`/`encode` yields the dual codec for free; a section transform has no dual — flipping it would demand `decode` reconstruct the discarded wire variation, exactly the one-way map the codec refuses — so a transform that flips cleanly is provably an iso and a transform that does not is a section, the symmetry of the declaration the proof of the round-trip class.

[THE_CANONICAL_SUBSET_IS_THE_TOTAL_ENCODE_THAT_REPLACES_A_ONE_WAY_MAP]:
- A normalizing projection (trim, case-fold, clamp, round) is genuinely lossy on the wire — many inputs map to one output — yet every shipped form types its `To` as the REFINED canonical subset rather than the bare codomain: `Trim` is `transform<SchemaClass<string>, Trimmed>`, `clamp(min, max)` is `transform<S, filter<SchemaClass<A>>>`, `Lowercase` is `transform<SchemaClass<string>, Lowercased>` — the `to` schema is the predicate-narrowed image (`Trimmed`, the in-range filter, `Lowercased`), so the `encode` is the identity onto that subset and lands in it BY CONSTRUCTION, `decode ∘ encode = id` on the canonical subset holding while `encode ∘ decode` collapses the equivalence class.
- This is how the codec FORCES a lossy projection into a section instead of permitting a one-way map: the total `encode` requirement cannot be satisfied by reconstructing the original (that information is gone), so it is satisfied by re-presenting the already-canonical value, and the refinement on `To` is what makes that re-presentation total — encode of a `Trimmed` value is the value itself, valid because `Trimmed` already holds, the canonical-form witness the refinement supplies for free rather than a hand-written `string -> string` that would have to re-prove trimmedness.
- The reject is the same projection written as a `filter` that mutates plus a separate one-way reader: a `decode` that lowercases targeting a bare `SchemaClass<string>` (not `Lowercased`) compiles, but its `encode` then has the same `string` type on both sides with no witness that the output is canonical, so a downstream `decode ∘ encode` re-lowercases redundantly and an equality check against a freshly-decoded value can diverge on a non-canonical re-encode — the refinement on `To` is the invariant that makes the round-trip idempotent, dropping it the defect that makes the section silently leak.

```typescript
import { Schema } from 'effect'
const Slug = Schema.transform(
    Schema.String,
    Schema.Lowercased.pipe(Schema.pattern(/^[a-z0-9-]+$/)),                 // To IS the canonical subset, the encode-target witness
    {
        decode: (s) => s.toLowerCase().replace(/[^a-z0-9]+/g, '-'),         // lossy: many inputs collapse to one slug
        encode: (slug) => slug,                                            // total + canonical: output already in the refined image
        strict: true,
    },
)
type Wire = Schema.Schema.Encoded<typeof Slug>                             // string — every representable string
type Norm = Schema.Schema.Type<typeof Slug>                                // the lowercased-pattern subset, the encode codomain
```

[OPTION_AND_SINGLETON_ARE_THE_WITNESS_FOR_ARITY_LOSS]:
- When the loss is ARITY rather than form — projecting one element out of a collection, collapsing an absent value — the canonical witness is `Option` or a singleton rebuild, never a thrown failure: `head` is `transform<S, OptionFromSelf<SchemaClass<A[number]>>>` (decode takes the first element to `Option`, encode rebuilds a singleton array or empty array from the `Option`), `headOrElse(fallback?)` is `transform<S, SchemaClass<A[number]>>` where the `fallback` lazy arg supplies the witness for the empty case so the decode is total without an `Option`, and `pluck(key)` is `SchemaClass<A[K], Pick<I, K>, R>` whose encode rebuilds the single-key record from the extracted column.
- The witness is the smallest structure whose decode is the projection's left-inverse: a `head` encode produces `[a]`, not the original n-element array, so it is a SECTION onto single-element arrays — feeding a multi-element array through `decode` then `encode` returns a one-element array, the arity collapse explicit in the witness shape rather than hidden in a `decode` with no encode; the `OptionFromSelf` on `To` is precisely the marker that the empty case is handled in-band rather than as a parse failure.
- This deletes the loose `rows.map((r) => r.column)` plus a separate re-wrap: `Schema.Array(Owner.pipe(Schema.pluck('id')))` is the round-trippable column extraction where the post-decode map and the encode-side re-construction descend from the one transform, so a decoded `ReadonlyArray<Id>` re-encodes to the array of single-key records the wire carried — the projection and its inverse declared once, the hand-written extractor and its absent counterpart both deleted.

[COMPOSE_FIDELITY_IS_THE_MEET_OF_THE_LEG_RUNGS]:
- Composite invertibility is the lattice MEET of the legs' rungs, not a property the join can improve: because `ComposeTransformation` is a pure marker carrying no `decode`/`encode` and the codec is the two leg ASTs run in series, iso ∘ iso stays iso, iso ∘ section drops to section, section ∘ section stays section — the composite is bounded above by its least-invertible leg, so round-trip fidelity degrades monotonically down a chain and never recovers, and a pipeline's rung is read by scanning its legs for the first refined `To`, not by inspecting the composite.
- The SEAM between two legs carries its own rung independent of the legs', and which `compose` overload type-checks is the witness: exact-equality alignment (`Type<From>` = `Encoded<To>`) is an iso seam, a subtype-narrowing overload (`Encoded<To>` assignable to `Type<From>`, or its mirror) is a section seam that down-casts the wider side at the join, and `{ strict: false }` joining on `unknown` is a seam with no fidelity proof at all — so the seam can be the lossy link even when both legs are iso, and reaching for the narrowing or `strict: false` overload is the declaration admitting the meet has dropped a rung at the JOIN rather than inside a leg.
- The codec collapses the chain to two visible slots but the failure tree preserves every seam, so the diagnostic and the round-trip see opposite structures: a decode error in a three-leg pipeline surfaces nested `Transformation` issues at each underlying leg's node, naming WHICH join rejected, while `Encoded<From>` and `Type<To>` are all the public triple exposes — the seam is invisible to a consumer reading the type and fully visible to one reading the issue, the asymmetry the reason a composite's `identifier` is reattached on the whole rather than recovered from the erased intermediate.

```typescript
import { Schema } from 'effect'
const PathOfInts = Schema.compose(
    Schema.split('/'),                                                     // string ⇄ ReadonlyArray<string>  (iso join)
    Schema.Array(Schema.NumberFromString),                                 // each segment ⇄ number           (section: "1.0" re-encodes "1")
)                                                                          // composite: section, degraded to the weaker leg
type Wire = Schema.Schema.Encoded<typeof PathOfInts>                       // string — the intermediate array erased from the triple
type Vals = Schema.Schema.Type<typeof PathOfInts>                          // readonly number[]
// Schema.compose(a, b, { strict: false })                                 // REJECT as default: joins on unknown, no seam fidelity proof
```

[THE_LITERAL_TABLE_IS_THE_ONE_PROVABLE_ISO_OVER_A_FINITE_SET]:
- `transformLiterals(...pairs)` derives a `Union` of `transformLiteral<Type, Encoded>` from a tuple of `[from, to]` literal pairs, and because each pair is a closed bijection on a finite literal set it is the one transform family that is a TOTAL ISO by construction — the decode maps `from -> to` and the encode `to -> from` with both legs exhaustive over the enumerated set, so adding an enum code is one tuple entry the union absorbs and the inverse derives from the same pair, never a parse switch plus a serialize switch maintained in lockstep.
- The arity-one form `transformLiteral(from, to)` returns the single member and the multi-pair form returns the `Union`, so a programmatically-built code table degrades cleanly to a lone iso at one pair and distributes the triple over the members at many — the bidirectional enum-code correspondence is the canonical iso the lane offers, the rung against which every richer transform is measured: if a transform is NOT a literal-pair table, ask whether its loss forces it down to a section before declaring it.
- The iso guarantee holds only while the literal sets are disjoint on each side; a pair list mapping two distinct `from` literals to the SAME `to` literal breaks the encode's left-inverse (the `to -> from` direction is ambiguous), so the union silently becomes a section the moment the `to` set has a collision — the proof of iso is the injectivity of BOTH projected literal columns, a duplicate on either column the signal the table has dropped a rung.

[THE_TOTAL_ENCODE_IS_THE_DECLARATION_THAT_SURFACES_NON_INVERTIBILITY_EARLY]:
- The mandatory `encode` leg is the mechanism that moves the non-invertibility discovery from a SILENT lost round-trip at runtime to a VISIBLE obligation at declaration: a `decode` that adds a field forces the `encode` to discard it explicitly (`({ raw }) => ({ raw })`), a `decode` that normalizes forces the `encode` to commit to the canonical representative, a `decode` that collapses arity forces the `encode` to pick a witness — so the author confronts the inverse at the moment of writing, and a projection with no honest inverse cannot be expressed as a `transform` at all, only as a one-directional decode the codec has no constructor for.
- The same rung lattice governs the OWNER granularity, where the unit is a whole entity rather than a leaf: a derived class adding fields synthesized on decode is a SECTION at the class level — `BaseClass.transformOrFail<Next>(id)` types `encode: Type<Fields & NewFields> -> Effect<Type<Fields>>` total, so the synthesized fields are discarded or recomputed on encode and the base instance is the canonical witness the extended class retracts onto, the added columns the equivalence class the round-trip collapses; only a class transform whose added fields are themselves derivable BACK from the base on encode is an iso, every other an honest section, the seam position (`transformOrFail` post-decode vs `transformOrFailFrom` pre-decode) orthogonal to the rung.
- The reject the whole lane converges on: a genuinely information-destroying extraction is NOT a `Schema.transform` and NEVER a bare one-way map function read after decode — it is either pushed onto the `To` as a refinement so the encode re-presents the canonical witness (the section), or it is not a codec concern at all and lives as a post-decode projection on the value with no claim to round-trip; the codec's refusal to admit a one-way transform is the design forcing the author to NAME the witness, and a transform whose `encode` throws, returns a constant placeholder unrelated to the value, or widens to `strict: false` to dodge the inverse is the lossy-projection temptation the total-encode law was built to refuse.
