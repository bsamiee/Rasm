# Three-AST Codec Topology

[TRANSFORMATION_SPINE]:
- `Self.ast` is materialized lazily as `transform(encodedSide, declaration, { decode: i => new this(i, true), encode: identity })`, so the node is an `AST.Transformation` whose `from` is the encoded-side struct AST and whose `to` is an `AST.Declaration` — the class is a codec FROM its field-record encoding INTO a nominal instance, and the instance face is a `Declaration` (an opaque nominal node), never the field-record `TypeLiteral` itself.
- The `to` `Declaration` carries an `instanceof`-keyed decode (`input instanceof this || fallbackInstanceOf(input)`) and an `encode` that runs `encodeUnknown(typeSide)(input)` then `new this(props, true)` — the instance face validates nominal identity, not field shape, which is why decoding-into-the-class is a real `Declaration` parse step distinct from parsing the encoded struct, and why `decode: i => new this(i, true)` reconstructs the method-bearing instance rather than returning the raw record.
- The transform's own `decode` runs the constructor with validation OFF (`new this(i, true)`) because the `from` side — the refined encoded struct — has already parsed one node earlier; the instance face never re-validates field invariants, it only re-wraps the validated record into the nominal type, so the three faces partition the work: `from` validates representation+refinements, the transform reconstructs, `to` asserts nominal identity.
- `astCache` is a process-global `WeakMap` keyed on the class constructor; the `static get ast()` computes the Transformation once and memoizes it, so the three-AST node is built on first projection and every `Schema.decodeUnknown(Self)`, `Schema.encodedSchema(Self)`, JSON-Schema walk, and union-membership read shares one frozen node — the codec topology is built once per owner, never per call.

```typescript
import { Schema } from 'effect';

class Capsule extends Schema.Class<Capsule>('Capsule')({
    rate: Schema.NumberFromString,
    span: Schema.Number.pipe(Schema.between(0, 1)),
}) {
    get scaled(): number {
        return this.rate * this.span;
    }
}

// the SAME owner yields the encoded face and the type face from one declaration — never restated
const onTheWire = Schema.encodedSchema(Capsule); // { rate: string; span: number }, the `from` struct
const inMemory = Schema.typeSchema(Capsule); // { rate: number; span: number }, the `to` declaration face
const admit = Schema.decodeUnknown(Capsule); // walks from → transform → to, returns a `scaled`-bearing instance

// reject: a parallel `WireCapsule` struct + a hand `decodeWire` + a `class CapsuleImpl` re-spell, by hand,
// the three faces the one Transformation already partitions — encoded struct, reconstruction, nominal type.
```

[FACE_DERIVATION_LAW]:
- `typeAST(Self.ast)` collapses the `Transformation` to `typeAST(ast.to)` — the instance `Declaration` — discarding the encode/decode functions, so the type face of a class is its nominal declaration, not the decoded field record; `encodedAST(Self.ast)` walks `ast.from` into the encoded struct, recursing every field's own transform to its encoded side. The two faces are reached by two distinct AST folds over one node, so a single owner answers `Schema.Schema.Type` and `Schema.Schema.Encoded` from one declaration without a parallel encoded interface.
- The `Transformation`-collapse in `typeAST` preserves EXACTLY six annotation ids onto the surviving `to` node — examples, default, jsonSchema, arbitrary, pretty, equivalence — and drops every other annotation that sat on the transformation node; an annotation placed on the transformation slot that is not one of those six is invisible to every type-face projection, which is the silent trap behind misrouting a `title` or `message` to the wrong ClassAnnotations slot.
- A field whose own schema is itself a transform (`NumberFromString`, `OptionFromNullOr`) contributes its OWN `from` to the class's `encodedAST` and its own `to` to the class's `typeAST`, so the three-AST topology nests: the class Transformation's `from` is a struct of per-field transforms, and `encodedAST` recursing the whole node yields the fully-encoded record where every field has bottomed out at its primitive wire form.

[ANNOTATION_TUPLE_TARGETING]:
- The `ClassAnnotations` tuple is destructured `[typeAnnotations, transformationAnnotations, encodedAnnotations]`, and each slot fans to the faces it governs: `typeAnnotations` is spread onto FOUR derived schemas — the `declarationSurrogate`, the `typeSide`, the `constructorSchema`, and the `transformationSurrogate` — `encodedAnnotations` onto the `encodedSide` (the `from`) and the `transformationSurrogate`, and `transformationAnnotations` onto the outer `.annotations()` applied to the assembled Transformation node plus the `transformationSurrogate`. The tuple is positional precisely because each face is a separately-annotatable AST, never one annotation map for the whole codec.
- The object form `ClassAnnotations = Annotations.Schema<Self>` is sugar for the one-element tuple `[typeAnnotations]`, so an object at the class header annotates the type face alone and leaves the encoded and transformation faces bare — the most common form targets the instance, and reaching the encoded or transformation face REQUIRES the explicit tuple, the array brackets being the only spelling that addresses the non-type faces.
- The type-face slot is not one annotation map but the seed for four divergent derivations, each carrying a distinct `AutoTitle`: the `declarationSurrogate` (titled by the bare identifier) is the projection-recoverable struct, the `typeSide` (titled `(Type side)`) drives the `fallbackInstanceOf` structural admission, the `constructorSchema` (titled `(Constructor)`) is the validator `new`/`make` run, and the merged surrogate feeds external readers — so a `parseOptions` placed in this slot governs the constructor's own validation pass AND every type-face decode at once, one policy value reaching four schemas the loose spelling would thread through each call site.
- The encoded-face slot annotates only the `from` struct and the merged transformation surrogate, so a `jsonSchema` override there shapes the wire-shape projection without touching the instance validator — the asymmetry is load-bearing: a constraint that must describe the serialized form lands in the encoded slot, a constraint that must gate construction lands in the type slot, and conflating them puts the annotation on a face that never runs at the seam that needs it.

```typescript
import { Schema } from 'effect';

const Source = Schema.Struct({
    code: Schema.NumberFromString,
    tier: Schema.String,
});

// the three-slot tuple targets the type face, the transformation face, and the encoded face independently
class Routed extends Schema.Class<Routed>('Routed')(Source, [
    { title: '<type-face>', parseOptions: { errors: 'all' } }, // type face: validator + instance projections
    { identifier: '<edge-codec>' }, // transformation face: the encoded↔type node itself
    { description: '<wire-shape>' }, // encoded face: the `from` struct downstream tools read
]) {}

// reject: a single `Routed.annotations({ ... })` AFTER declaration returns a SchemaClass stripped of
// `new`/`make` — the post-hoc spelling splits the aspect from the owner; the tuple keeps all three faces
// on the one declaration that also yields the constructor.
```

[SURROGATE_PROJECTION_REDIRECT]:
- The `to` `Declaration` is opaque to every structural walk — a `Declaration` exposes no property signatures — so each derived projection would see a nameless nominal node; the class plants the type-side struct as `SurrogateAnnotationId` on the declaration (`declarationSurrogate.ast`, the `typeSchema` struct annotated with the identifier) and a second surrogate on the Transformation node, so a downstream walker recovers the field record the declaration hides.
- JSON-Schema generation checks `getSurrogateAnnotation(ast)` first and, when present, redirects the whole walk through the surrogate struct — `go(surrogate.value, ...)` — so a class owner projects to the JSON Schema of its FIELD RECORD, not the empty object an opaque `Declaration` would yield; the surrogate is the sole reason a class has a structural JSON Schema at all.
- Union decode dispatch keys off the surrogate: `getLiterals` over a member `Declaration` reads `getSurrogateAnnotation` and recurses into the surrogate struct to harvest the `_tag` literal from a non-optional literal field, so `getSearchTree` builds the discriminant bucket map across class members from their surrogates — a `Schema.Union` of class owners dispatches `decodeUnknown` by `_tag` because each member surrogate exposes the tag literal the opaque declaration would hide, never a manual switch at the seam.
- The transformation-face surrogate merges `encodedAnnotations ∪ typeAnnotations ∪ transformationAnnotations` onto the encoded `schema`, so a tool reading the surrogate off the Transformation node sees the encoded struct enriched with every face's annotations — the surrogate is the single recoverable view that lets an external registry treat the class as its struct without holding a parallel struct declaration beside it.

[PROTO_INSTANCE_DISJUNCTION]:
- The instance type is `Struct.Type<Fields> & Inherited & Proto`, but the codec is over `encodedSide` whose type is `Struct.Encoded<Fields>` alone — so the `Proto` slot (class-body methods and getters) and the `Inherited` slot (the parent class on `.extend`) are part of the instance type yet outside the encoded face entirely; a getter changes the dispatch surface with zero wire-shape change because the encoded struct never includes it and `encodedAST` never reaches the prototype.
- The round-trip reconstructs `Proto` for free: the transform's `decode: i => new this(i, true)` runs the real constructor, so the decoded value is a genuine instance with its prototype chain — methods are not serialized and not re-attached by hand, they ride the prototype the `new this` recreates, which is why a decoded class value answers its getters while its wire form carried only the field record.
- `Proto` is `{}` for a plain class and the throwable/yieldable capability for an error owner, supplied through the `Base` the class extends; because the codec face is the encoded struct, the error's throwable surface is prototype-side and never enters `from`, `to`-as-record, or the constructor parameter — the instance type and the codec type diverge by exactly the `Inherited & Proto` intersection, which is the type-level statement that behavior is not data.
- The `fallbackInstanceOf` arm in the instance-face decode (`hasProperty(u, classSymbol) && is(typeSide)(u)`) admits a value bearing the per-class symbol AND matching the type-side struct even when it is not `instanceof this`, so cross-realm or re-deserialized instances re-admit through the structural type face rather than the brittle prototype check — the nominal `to` face has a structural escape that keeps the codec sound across realm boundaries the raw `instanceof` would reject.
