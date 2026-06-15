# Union Codec Wire Projection

[STRIP_IS_STRUCTURE_PRESERVING_OVER_THE_MEMBER_ARRAY]:
- `typeSchema` and `encodedSchema` are not flatteners — both recurse the union node by mapping the stripper over `ast.types` (`changeMap(ast.types, typeAST)`, `changeMap(ast.types, encodedAST_)`) and reassembling through `Union.make(types)`, so a tagged family's type-projection and wire-projection are each *themselves a union* whose arm count equals the source's, the `_tag` discriminant surviving every strip — the wire view of an N-arm variant is an N-arm variant, never a collapsed record.
- The recursion preserves arm *identity* by referential short-circuit: `changeMap` returns the original array when no arm changed (`changed ? out : as`), so stripping a union whose arms carry no transformation yields the *same* `ast.types` reference and the projection is the owner unchanged — the strip allocates a new union only at arms that actually carried encoding, the partition's shape recomputing precisely at the seam that moved it.
- The collapse this licenses: the wire-contract emitter, the in-memory-shape validator, and the round-trip codec are three reads of one owner's stripped projections, never three hand-authored union mirrors — `JSONSchema.make(encodedSchema(Owner))` is the wire schema, `Schema.is(typeSchema(Owner))` the post-decode interior guard, and both keep the arm partition the decoder reads, so a new arm appended to the owner widens all three with one declaration; the rejected spelling is a parallel `WireUnion` restating each arm's encoded shape beside the domain union, two member lists the next arm edits twice.

```typescript
import { Schema } from 'effect'

class Emitted extends Schema.TaggedClass<Emitted>()('Emitted', { at: Schema.DateTimeUtc, count: Schema.Int.pipe(Schema.positive()) }) {}
class Idle extends Schema.TaggedClass<Idle>()('Idle', { since: Schema.DateTimeUtc }) {}
const Owner = Schema.Union(Emitted, Idle)
const Wire = Schema.encodedSchema(Owner)
const Domain = Schema.typeSchema(Owner)
const wireGuard = Schema.is(Wire)
const domainGuard = Schema.is(Domain)
```

- `Wire` is `Schema.Union` of `{ _tag: "Emitted"; at: string; count: number }` and `{ _tag: "Idle"; since: string }` — `DateTimeUtc` stripped to its ISO-string encoded side, `positive()` dropped — while `Domain` is the union of the decoded shapes (`at` a `DateTime.Utc`, `count` still a number); both retain two arms keyed on `_tag`, so `wireGuard` admits a raw payload and `domainGuard` admits a decoded value, one owner yielding both partition-preserving guards, never a flattened `{ _tag: string }` either projection would lose.

[BOUND_STRIP_KEEPS_CARDINALITY_FILTERS_DROPS_VALUE_FILTERS]:
- The third stripper, `encodedBoundSchema`, is `encodedSchema` that *preserves refinements up to the first transformation point* — its internal `encodedAST_(ast, true)` keeps a `Refinement` arm only when two conditions hold at once: `getTransformationFrom(ast.from)` is `undefined` (no transform sits below the filter) and `hasStableFilter(ast)` is true (the filter carries `StableFilterAnnotationId`), otherwise it descends to the filter's `from` and the constraint vanishes.
- The stable-filter set is narrow and deliberate: only the array-cardinality filters `minItems`, `maxItems`, and `itemsCount` annotate `StableFilterAnnotationId: true`; value-domain filters (`positive`, `between`, `nonEmptyString`, `int`) do *not*, so `encodedBoundSchema` of a `Schema.Array(member).pipe(Schema.minItems(1))` arm keeps the `minItems` on the wire contract while `encodedBoundSchema` of a `Schema.Int.pipe(Schema.positive())` arm drops `positive` exactly as `encodedSchema` would — the "bound" distinction is observable only on cardinality-constrained collection arms.
- The law for choosing the stripper over a variant family: `encodedSchema` for the *raw transport contract* (a JSON body validator that must accept any string a `DateTimeUtc` arm decodes), `encodedBoundSchema` for a *wire contract that still asserts collection cardinality* (a batch arm whose `minItems` is a wire-level invariant the consumer must honor), `typeSchema` for the *interior re-entry guard* — three projections off one union, the stripper selecting which refinements ride the wire, never a hand-pruned schema per surface.

[TRANSFORM_ARM_DECIDES_WHICH_SIDE_THE_STRIP_KEEPS]:
- A transformation arm is the only node where `typeAST` and `encodedAST_` keep *different sides*: `typeAST` of a `Transformation` recurses `ast.to` (re-attaching the transform's preserved annotations onto the domain side), `encodedAST_` recurses `ast.from` (the wire side, annotations dropped) — so the type-projection of a transform-bearing union is the union over each arm's decoded shape and the wire-projection is the union over each arm's encoded shape, two materialized schemas off one owner where a refinement-only union returned the *same* reference for both strips.
- The strip is therefore the only direction-correct source for a generated contract: an inbound wire validator must come from `encodedSchema` (the projection that kept every arm's `from`), an outbound domain assertion from `typeSchema` — reading the wrong projection's literals for a contract inverts every transform arm, since the projection that kept `to` carries names the wire never sends and the projection that kept `from` carries codes the interior never holds.
- The annotation asymmetry compounds it: `typeAST` runs `preserveTransformationAnnotations`, re-attaching exactly the *projection-derivation* annotations — `examples`, `default`, the `jsonSchema` override, `arbitrary`, `pretty`, `equivalence` — onto the domain side, while `encodedAST_` keeps none, so a `jsonSchema`-override or custom `arbitrary` declared on a transform arm survives into `typeSchema` and `Arbitrary.make(typeSchema(Owner))` but vanishes from `encodedSchema` — the override rides the side the strip kept, the wire projection falling back to the structural default the arm's `from` describes.

[ENCODE_SHAPE_COLLISION_IS_RESOLVED_BY_THE_DOMAIN_TAG]:
- The decisive asymmetry of the codec: encode receives the *decoded* `Type` and keys its dispatch on the type-side discriminant the value still carries, so an arm's wire shape is the *output* of selection, never its key — which means arms that *collide on their encoded shape* (two arms decoding to distinct domain types yet encoding to the identical wire object) stay deterministically encodable, because the domain `_tag` selects the arm before any wire object exists; a wire-shape-keyed encoder could not tell the colliding arms apart, the exact ambiguity the domain-side key forecloses.
- The collision is invisible to decode and surfaces only as a *non-round-tripping* family: two arms that encode to one wire shape mean the wire payload decodes to whichever arm wins the decode search tree, so `decode ∘ encode` is identity (the domain tag chose the arm, and the wire object re-decodes to a sibling) only when the colliding arms also share their decoded type — a genuine encode-shape collision between *distinct* domain types is a lossy projection the type system admits and the round-trip law breaks, the boundary being that an encode-collision is safe exactly when it is also a decode-merge.
- The codec pipeline carries the direction in its error frame, not a flag: a transform arm runs `from → transform → to` on decode and `to → transform → from` on encode, the located issue tagged `"Encoded"`/`"Type"` swapped by direction — so an encode failure on a domain-discriminated arm reports against that arm's *encoded* expectation, the diagnostic naming the wire shape the domain value failed to produce, the disambiguation and the error locality both keyed on the side the direction owns.

```typescript
import { Schema } from 'effect'

const Compact = Schema.transform(
    Schema.Struct({ _tag: Schema.Literal('row'), a: Schema.Int, b: Schema.Int }),
    Schema.Struct({ _tag: Schema.Literal('pair'), sum: Schema.Int }),
    { strict: true, decode: ({ a, b }) => ({ _tag: 'pair' as const, sum: a + b }), encode: ({ sum }) => ({ _tag: 'row' as const, a: sum, b: 0 }) },
)
const Single = Schema.Struct({ _tag: Schema.Literal('single'), sum: Schema.Int })
const Owner = Schema.Union(Compact, Single)
const toWire = Schema.encodeSync(Owner)
const emitted = toWire({ _tag: 'pair', sum: 7 })
```

- `Compact` encodes `pair → row` while `Single` is identity, so both can emit a `{ _tag, sum }`-shaped neighborhood, yet `toWire({ _tag: 'pair', sum: 7 })` selects `Compact` by the *domain* tag `"pair"` and emits `{ _tag: 'row', a: 7, b: 0 }` — the encoder partitioned on the type-side literal the value carries, never on the wire object it has not yet built; a hand-rolled encoder switching on the encoded shape could not tell `pair` from `single` before encoding, the exact ambiguity the domain-side encode tree forecloses.

[TRANSFORMLITERALS_IS_ONE_PAIR_TABLE_READ_BOTH_WAYS]:
- `transformLiteral(from, to)` is `transform(Literal(from), Literal(to), { decode: () => to, encode: () => from })` — both directions *ignore their input* and return the opposite cell of the one pair, so the inverse encode map is not authored, derived, or stored: it is the identical pair tuple read right-to-left, the decode reading left-to-right; one row carries both directions, and a code-name correspondence cannot drift between a decode table and an encode table because there is only one tuple.
- `transformLiterals(...pairs)` is `Union(...pairs.map(([from, to]) => transformLiteral(from, to)))` — a union of single-cell transform arms — so the wire-code-to-domain-name remap inherits the whole union codec machinery: decoding partitions the encode/decode search trees per direction (decode keys the `from` codes, encode keys the `to` names), `Schema.Schema.Type` is `pairs[number][1]` (the names) and `Schema.Schema.Encoded` is `pairs[number][0]` (the codes), and a new correspondence is one `[code, name]` row widening both projections, every `Match.exhaustive` over the decoded names breaking until the new name is handled.
- The collapse trigger and its boundary: a decode lookup `Record<Code, Name>` paired with a hand-inverted encode lookup `Record<Name, Code>` is two tables the next pair edits twice and a transposition typo silently desyncs — `transformLiterals` deletes the second table by making encode the same tuple reversed; the boundary is the discriminant-type homogeneity the wire layer demands — mixing numeric and string codes in one pair table builds arms whose encoded literals share a stringified bucket, the remap correct at the type level but the wire disambiguator's index collapsing the code axis it kept distinct.

```typescript
import { Schema } from 'effect'

const Tier = Schema.transformLiterals([0, 'guest'], [1, 'member'], [2, 'admin'])
type TierName = Schema.Schema.Type<typeof Tier>
type TierCode = Schema.Schema.Encoded<typeof Tier>
const fromWire = Schema.decodeUnknownSync(Tier)
const toWire = Schema.encodeSync(Tier)
const name: TierName = fromWire(2)
const code: TierCode = toWire('member')
```

- `fromWire(2)` reads the `[2, 'admin']` row left-to-right to `'admin'`, `toWire('member')` reads the `[1, 'member']` row right-to-left to `1` — one pair table, both directions, `TierName` the `'guest' | 'member' | 'admin'` union and `TierCode` the `0 | 1 | 2` union both projected off `pairs[number]`; adding `[3, 'owner']` widens the name union, the code union, and every exhaustive dispatch over names in one row, the rejected spelling a `decode` object literal beside an `encode` object literal whose key-value transposition the checker never inspects.

[JSON_DISCRIMINANT_IS_A_SINGLETON_ENUM_NOT_A_CONST]:
- The `_tag` materializes on the JSON contract as `{ type: "string", enum: [literal] }` — a single-element `enum`, never a JSON `const` — across every target (`jsonSchema7`, `jsonSchema2019-09`, `jsonSchema2020-12`, `openApi3.1`): the `Literal` emitter branches on the literal's runtime type and emits `{ type, enum: [literal] }` for string, number, and boolean, with no `const` path anywhere in the emitter, so a consumer validates the discriminant by single-element-enum membership and a contract test asserting `const` mis-specifies the emitted shape.
- A bigint or symbol discriminant has *no* JSON projection: the emitter throws a missing-annotation error for any literal that is not string, number, or boolean (and for `null` emits `{ type: "null" }`), so a numeric-tagged variant family JSON-projects but a `Schema.tag(0n)` arm forces an explicit `jsonSchema` annotation or the whole union fails to emit — the discriminant's wire-projectability is narrower than its decode-admissibility, the bigint axis the decoder accepts the emitter rejects.
- The union itself emits `anyOf` over its arm schemas (arity 0 to `constNever`, arity 1 to the bare member, arity `>= 2` to `{ anyOf }`), each `TaggedClass` arm an object schema carrying its singleton-`enum` `_tag` property — so the wire contract is a discriminated `anyOf` structurally mirroring the runtime decoder's bucket partition, the discriminant a per-arm constant the consumer branches on exactly as the search tree does.

[COMPACTION_MERGES_LITERAL_ARMS_NEVER_OBJECT_ARMS]:
- The `anyOf` is post-processed by `compactUnion`, which fuses *adjacent* members that are each a bare single-element-enum of the *same* `type` into one multi-value `enum` (`{ type, enum: [...last.enum, ...m.enum] }`) — so a union of bare `Schema.Literal` arms collapses its per-arm `enum` cells into one consolidated `{ type: "string", enum: [...] }`, the discriminant vocabulary materializing as a single closed enum rather than N singleton `anyOf` entries.
- The compaction is structurally inert on a `TaggedClass`/`TaggedStruct` union: those arms emit *object* schemas, not bare literals, so `isCompactableLiteral` is false for each and every tagged arm survives as its own `anyOf` entry carrying its own `_tag` singleton-enum — the merge applies to a discriminant-*vocabulary* owner (`Schema.Literal('a', 'b', 'c')`, `transformLiterals` decoded names), never to the variant-*family* owner whose arms carry fields, so the two owners JSON-project to two different shapes from the same `make` call.
- The adjacency requirement is load-bearing and order-sensitive: compaction fuses only when the previous accumulated member is also a compactable literal of the same type, so a literal arm separated from its sibling by an object arm does not merge with it — a mixed union interleaving bare literals and tagged objects emits a partially-compacted `anyOf`, the literal runs fused and the object arms standing alone, the wire shape a function of declaration order the emitter walks once.

[IDENTIFIER_ANNOTATION_IS_THE_DEFS_CUT_POINT]:
- A member's `identifier`/`jsonSchema.identifier` annotation is the cut where an arm is hoisted into `$defs` and referenced by `$ref`: the emitter, on a `handle-identifier` pass, replaces the arm inline with `{ $ref: definitionPath + id }` (default path `#/$defs/`) and materializes `$defs[id]` by a *two-phase write* — it assigns the `$ref` placeholder to `$defs[id]` *first*, then recurses the full arm schema into `$defs[id]` — so a self-referential arm whose recursion reaches its own `$ref` finds the slot already occupied and terminates, the cycle broken by the placeholder-before-recurse order.
- The cut point is what converts a recursive variant from an infinite inline expansion into a finite cyclic contract: a `Schema.suspend` arm annotated with an `identifier` emits a `$ref` back to its own `$defs` slot, and `getIdentifierAnnotation` reaches *through* the suspend (`isSuspend(ast) → getIdentifierAnnotation(ast.f())`) and through a transform-to-declaration surrogate (`isTransformation && isTypeLiteral(from) && isDeclaration(to)` with a surrogate annotation) to find the identifier the recursion needs — so the annotation placed once on the recursive arm is the load-bearing bit that makes the wire contract emittable at all.
- The strip composes with the cut for direction-correct `$defs` names: the encoded-side suspend re-threads its borrowed identifier with an `Encoded`/`Bound` suffix (`${identifier.value}Encoded`, `${...}EncodedBound`), so `JSONSchema.make(encodedSchema(RecursiveOwner))` emits `$defs` keyed by the suffixed names distinct from the type-projection's keys — one recursive owner yielding a domain-named `$defs` graph and a wire-named `$defs` graph, the cut point reused per direction, never two hand-authored definition maps; `topLevelReferenceStrategy: "skip"` is the one knob that inlines the top-level union instead of hoisting it, reserved for an envelope that must not self-reference.

```typescript
import { JSONSchema, Schema } from 'effect'

type Node = Branch | Leaf
class Leaf extends Schema.TaggedClass<Leaf>()('Leaf', { value: Schema.Int }) {}
class Branch extends Schema.TaggedClass<Branch>()('Branch', {
    children: Schema.Array(Schema.suspend((): Schema.Schema<Node> => Tree)),
}) {}
const Tree = Schema.Union(Leaf, Branch).annotations({ identifier: 'Tree' })
const wireContract = JSONSchema.make(Schema.encodedSchema(Tree))
```

- `Tree`'s `identifier: "Tree"` annotation hoists the whole union into `$defs.Tree` and the recursive `Branch.children` arm emits `{ $ref: "#/$defs/Tree" }`, the two-phase write seeding the placeholder before recursing so the self-reference resolves to a finite cyclic `anyOf` of two object arms each carrying its `_tag` singleton enum; `encodedSchema` strips `Int` to its number wire shape while keeping both arms and the recursion, so the wire contract is the partition-preserving cyclic schema, the rejected spelling an inline expansion that never terminates or a hand-cut `$defs` map the next arm must re-thread by hand.
