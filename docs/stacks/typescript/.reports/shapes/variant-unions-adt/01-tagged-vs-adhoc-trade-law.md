# Tagged-versus-Adhoc Trade Law

[SEAM_COST_LAW]:
- One owner is selected by a seam-cost function, never by case count: `cost(owner) = codec_weight + scan_penalty + discriminant_coupling`, evaluated per variant family, and the owner minimizing it wins — a bare inline union, a decode-free `Data.taggedEnum`, or a `Schema.Union` of tagged members each occupy a distinct minimum over the same three axes.
- `codec_weight` is the only axis the seam forces: a family that never crosses `Schema.decodeUnknown` pays zero codec weight and the tagged-class owner's codec is dead surface, so an inline structural union or `Data.taggedEnum` dominates; a family admitted through a wire pays the codec weight regardless, so the question collapses to which tagged owner, never whether to tag.
- `scan_penalty` is the per-decode arm-trial count, fixed by whether every member carries a single-literal discriminant property: a fully discriminated family decodes by one bucket lookup (penalty `1`), a family with one undiscriminated member decodes every input against the discriminated bucket *plus* that member (penalty `bucket + otherwise`), so the penalty is not the member count but the undiscriminated-member count.
- `discriminant_coupling` is whether the discriminant is forced onto the wire: a stored `_tag` couples the serialized shape to the dispatch axis, `attachPropertySignature` decouples them by computing the tag on decode and stripping it on encode, and a `symbol` discriminant removes the tag from the JSON keyspace entirely — the axis is paid only where the wire must carry the tag.
- The crossover is anticipatory, not reactive: the moment a second case, a wire crossing, or a reusable narrowing guard is conceivable, the seam-cost minimum has already moved to a tagged owner, so the inline union is the correct shape only for a closed, single-site, never-serialized two-arm choice — widening an inline union after consumers exist re-derives the `_tag === "<lit>"` predicate at every site the tagged owner would have deleted.

[INLINE_UNION_FLOOR]:
- A bare inline `type T = A | B` is the zero-owner floor: no codec, no constructors, no `$is`/`$match`, no `Equal`/`Hash` — narrowing is hand-written `x.k === "<lit>"`, equality is structural `===` or a per-call comparator, and each is the loose spelling a tagged owner deletes the instant a third consumer appears.
- The floor holds correctness only while the discriminant is read at exactly one site: a single `s.kind === "circle" ? ... : ...` over a two-arm union pays nothing the tagged owner saves, so promoting it is surface sprawl until a second reader or a wire crossing moves the seam-cost minimum.
- The inline union forfeits the derivation family wholesale: `Schema.equivalence`, `Arbitrary.make`, `Pretty.make`, `JSONSchema.make`, and the `_tag`-keyed decode search tree are all unreachable, so every projection it would need is re-authored by hand — the floor is a floor precisely because each saved artifact is a future re-derivation, not a present cost.

```typescript
type Shape = { readonly kind: 'circle'; readonly radius: number } | { readonly kind: 'square'; readonly side: number }
const isCircle = (s: Shape): s is Extract<Shape, { kind: 'circle' }> => s.kind === 'circle'
const area = (s: Shape): number => (isCircle(s) ? Math.PI * s.radius ** 2 : s.side ** 2)
```

- `isCircle` restates the discriminant the union already carries, and at a second reader a third hand-written guard appears — the inline floor admits exactly one read before the seam-cost minimum has moved to `Data.taggedEnum`, whose `$is("circle")` is the same guard derived once.

[UNTAGGED_REJECTION_GATE]:
- `Data.taggedEnum<A>()` type-rejects any member already carrying a `_tag`: `ChildrenAreTagged<A>` maps each member to `"_tag" extends keyof A[K]`, and `UntaggedChildren<A>` resolves the owner's constraint to the string-literal error `"It looks like you're trying to create a tagged enum, but one or more of its members already has a `_tag` property."` — so feeding a pre-tagged shape fails to type rather than double-tagging.
- The gate is structural, not a runtime check: the owner's tag is woven by the `TaggedEnum<A>` mapped type (`{ readonly _tag: Tag } & A[Tag]`), so a member that already spells `_tag` would collide with the synthesized field, and the constraint catches it at the constructor's type position — the decode-free owner refuses an already-discriminated input as a category error, not a runtime conflict.
- The gate enforces the single-source direction: a shape carrying its own `_tag` is already an inline tagged union, and re-wrapping it through `Data.taggedEnum` would produce a parallel owner for one concept — the rejection forces the choice to be *either* the hand-spelled inline tagged union *or* the generated owner, never both layered.

[DISCRIMINANT_COUPLING_TRICHOTOMY]:
- The three discriminant spellings partition on the `discriminant_coupling` axis alone, every other axis held equal: a bare `_tag: Schema.Literal` field and a `Schema.tag` defaulted signature both *store* the tag (wire-coupled, differing only in construction cost — the defaulted form elides the tag at `.make`, the bare literal demands it), while `attachPropertySignature` *injects* the tag (wire-decoupled). The trade is not stored-versus-defaulted — that is a construction-ergonomics choice within the coupled half — it is coupled-versus-injected.
- `attachPropertySignature` is the only admitted untagged-to-tagged promotion: its `value` is constrained to `AST.LiteralValue | symbol`, so the attached field is a `Literal` schema participating in the decode search tree exactly like a native `_tag` — a union of injected-tag structs earns the bucket lookup the discriminant-admission gate demands — yet the encoded shape never carries it. A shape-discriminated payload is admitted once and the interior gains a uniform discriminant the whole `Match`/`$is` family keys on, the alternative — re-deriving the tag from structural presence at every interior read — the deleted spelling.
- The `symbol` form is the maximal decoupling: the injected discriminant is unreachable by JSON yet live for interior `Match.discriminator` dispatch, so where the wire already owns a string key that would collide, the dispatch axis rides a key no serializer can reach — the only spelling that keeps the wire contract and the dispatch contract independently authored over one owner.

```typescript
import { Schema } from 'effect'

const A = Schema.Struct({ value: Schema.Int }).pipe(Schema.attachPropertySignature('kind', 'a'))
const B = Schema.Struct({ label: Schema.String }).pipe(Schema.attachPropertySignature('kind', 'b'))
const Owner = Schema.Union(A, B)
const decode = Schema.decodeUnknownSync(Owner)
const encode = Schema.encodeSync(Owner)
const tagged = decode({ value: 1 })
const dispatched: number = tagged.kind === 'a' ? tagged.value : tagged.label.length
const wire: Schema.Schema.Encoded<typeof Owner> = encode(tagged)
```

- `decode({ value: 1 })` yields `{ kind: 'a', value: 1 }` — the interior `tagged.kind` drives one-bucket decode and direct narrowing — while `wire` types at the owner's `Encoded`, which carries no `kind` at all: the promotion pays the admission codec weight while paying zero wire coupling, so the discriminant exists for the whole interior `Match`/`$is` family yet is unreachable on the byte projection, the seam-cost minimum for a legacy payload whose tag must not serialize.
- The decoupling is asymmetric by the type, not by a flag: `attachPropertySignature` returns `SchemaClass<A & { readonly [k in K]: V }, I, R>`, so decode-add and encode-strip are the one combinator's two directions — never a paired add-on-read plus delete-on-write a `Schema.transform` would have to author to fake a stored tag's removal — and one declaration owns both directions of the coupling flip.

[DISCRIMINANT_ADMISSION_GATE]:
- The `scan_penalty` axis is governed by a single gate the bucket partition never advertises: `getLiterals` admits a property as a discriminant only when `AST.isLiteral(type) && !propertySignature.isOptional` — a single `Literal` AST node, required. A `_tag` typed `Schema.Literal("a", "b")` is a *union* of literals, not one `Literal` node, and an *optional* discriminant is `isOptional`, so both silently fall to `otherwise` and forfeit the bucket lookup entirely.
- The gate is the trade-law's sharpest hidden edge: a family that looks tagged at the type level (`_tag` present on every arm) pays the full undiscriminated penalty whenever a single arm widens its tag to a literal union or makes it optional — the field must be one required literal per member, so a discriminant axis modeled as a multi-valued field is `discriminant_coupling` and `scan_penalty` both regressed at once with no type-level signal.
- The penalty is set by member kind, not member count: one bare-`Struct` (`otherwise`) arm is appended to *every* discriminated bucket's candidate list at decode, so it taxes every input in the family with a trailing full-structural trial — `scan_penalty = bucket_size + otherwise_count`, minimized to `1` only when every member earns the gate, and a wide or optional tag on a single arm pushes it to the undiscriminated cost for the whole family.
- The gate co-determines error locality, not only decode cost: a strict-subset bucket localizes the failure diagnostic to that bucket's expected shape, so an `otherwise`-heavy or wide-tag union regresses decode cost and error precision with the same single shape decision — the gate is one knob over both the runtime trial count and the diagnostic blast radius.

[SCAN_PENALTY_SPANS_BOTH_SEAMS]:
- The discriminant-admission gate is paid at two seams by the same shape decision: the decode search tree keys on a member's single-literal property, and `Match.discriminator(field)` keys on `Tags<D, R>` — the identical single-literal-field condition. A family that fails the gate at decode (a wide or optional tag, a bare `Struct` arm) fails it at dispatch too, forcing a structural `Match.when` whose residual totality is `Refinement`-fragile where the gated family takes a key lookup.
- The two seams share one discriminant key: `Match.discriminator(field)`/`discriminators(field)` take the field as the first argument, so an injected non-`_tag` discriminant routes the dispatch family through the `discriminator` form keying on the exact literal the decode search tree partitioned on — the injected tag is one axis serving admission and dispatch, never two parallel partitions.
- The fusion is the trade-law's load: choosing the gated discriminant once pays down `scan_penalty` at both the admission boundary and every interior dispatch site, so the cost function is not a decode-only metric — an undiscriminated owner re-derives the structural trial at both seams, the gated owner amortizes one key lookup across the whole consumer surface.

[CROSSOVER_THRESHOLD]:
- The seam-cost minimum moves owner across exactly three thresholds, each a single axis crossing zero: a wire crossing flips `codec_weight` from zero (`Data.taggedEnum` dominates) to forced (`Schema.Union` of tagged members dominates); a second discriminant reader flips the inline floor's saved-artifact cost positive (a derived `$is`/`$match`/`equivalence` now deletes more than it costs); a wire-field collision or a tag-must-not-serialize constraint flips `discriminant_coupling`, selecting `attachPropertySignature` over a stored tag.
- No threshold is the case count: three near-identical arms is the collapse trigger for *factoring a shared field block* into a base extended across the union, not for choosing tagged-versus-adhoc — the owner selection is set by the three seam axes alone, and a two-arm family across a wire still takes `Schema.Union` of tagged members while a ten-arm interior-only family still takes `Data.taggedEnum`.
- The proof of a correct owner is the next-feature diff: under the seam-cost minimum, a new case lands as one constructor or one `TaggedClass` appended to the union, every `$match`/`Match.exhaustive`/derived projection breaking loudly until the arm is handled — under the inline floor past its single-reader budget, the new case is a new hand-written guard at every reader and a new comparator at every equality site, the multiplicative re-derivation the tagged owner collapses to one declaration.
