# Module-evaluation collapse surface

[TEMPORAL_THESIS]:
- The one-statement collapse is a claim in two times at once: the type-level fusion holds at every program point because types are timeless, but the value-level fusion holds only at the program point the owner's heritage line runs and forward from it, so a collapse the checker accepts can still be a TDZ throw the runtime rejects — the lane's deepest failure surface is the gap between a type that resolves globally and a value that materializes positionally. A declaration is collapsible only where its type contract AND its evaluation contract both close in one statement, and the second is the one the checker cannot police.
- Every literal-preservation lever in this lane is fully erased and therefore evaluation-neutral — `as const`, `satisfies`, `<const T>`, rest capture, and `NoInfer` leave no value to schedule, so they never participate in the temporal surface; the owners that DO carry a value side (`Schema.Class`, `Data.TaggedClass`, `Effect.Service`, the `Data.taggedEnum` record) are the only declarations a module-init ordering can break. The temporal surface is exactly the value-bearing-owner subset of the lane, and the inference levers sit entirely outside it.
- The collapse-impossibility test is a single question asked of the owner's value side: at the program point the heritage expression evaluates, is every binding the expression reads already past its temporal dead zone. A `yes` admits the one statement; a `no` splits into a minimal pair whose second point is a deferral, never a restatement — and the count and shape of that pair derive from the structure of the `no`, not from author preference.

[EAGER_READ_ALPHABET]:
- The heritage expression `Factory(...args)` reads its argument positions eagerly and left-to-right when the declaration line runs, so the alphabet of module-init reads is the alphabet of factory argument shapes: a field-record argument (`Schema.Class`'s `{ ... }`) reads each field schema VALUE the moment the line evaluates, a dependency array (`Effect.Service`'s `dependencies: [Other.Default]`) reads each `Layer` element value, and a make-record's non-deferred slots read their values — every one of these is a position that can name a binding still in TDZ.
- The field-record read is the subtle eager edge: `Schema.Struct({ a: SchemaA, b: SchemaB })` and a class field block evaluate each property's schema value at the line, so a field whose schema is a reference to a not-yet-declared sibling owner is a TDZ throw at construction, not a decode-time error — the field schemas are eager arguments, and a forward reference among them fails at module-init exactly as a forward heritage base does.
- The make-record splits its own positions by eagerness: `Effect.Service`'s `dependencies: ReadonlyArray<Layer.Layer.Any>` is an eager value array read at the heritage line, while its `sync: LazyArg<...>`, `effect: Effect<...>`, and `scoped: Effect<...>` slots are deferred descriptions whose bodies run at Layer-build — so one owner declaration carries two opposite evaluation timings, the `dependencies` element a module-init read and the `make` body a provision-time read, and a forward reference is fatal in the first slot and harmless in the second.

[ONE_THUNK_THREE_OWNERS]:
- The deferral thunk is one polymorphic mechanism instantiated once per value-bearing owner family, each spelled as a `LazyArg`-shaped argument that converts an eager read into a first-use read: `Schema.suspend(f: () => Schema<A, I, R>)` defers a recursive codec, `Layer.suspend(evaluate: LazyArg<Layer<ROut, E, RIn>>)` defers a recursive layer build, and `Effect.suspend(effect: LazyArg<Effect<A, E, R>>)` defers a recursive effect computation — three signatures, one shape, the thunk argument the universal second point for a genuine value-level cycle in each owner's domain.
- The split across the three is the boundary split the whole lane runs on: `Schema.suspend` is the deferral for a cycle in a CODEC (a self-referential decoded shape that must admit from `unknown`), `Layer.suspend` for a cycle in a REQUIREMENT graph (two layers each needing the other's service), `Effect.suspend` for a cycle in a COMPUTATION (a recursive effect referencing its own binding) — so the owner family that carries the cycle selects which `suspend` closes it, never a choice of preference, and a recursive requirement graph reaches `Layer.suspend` where a recursive codec reaches `Schema.suspend`.
- The deferral point differs per family in WHERE the second-point thunk lives, and the difference is a structural consequence of where each owner's traversal begins: `Schema.suspend` lives inside the recursive FIELD (the traversal is a decode walk entering at that field), `Layer.suspend` wraps the whole recursive LAYER expression (the traversal is a build entering at the layer root), and `Effect.suspend` wraps the recursive EFFECT (the traversal is a run entering at the effect). The thunk is one mechanism, but its placement reads off the owner's traversal entry point, so a misplaced thunk — wrapping a layer's leaf instead of its self-reference, or a schema's whole struct instead of its recursive field — defers the wrong subtree and either leaves the cycle open or pays for deferring an acyclic region.

```typescript
import { Context, Layer } from 'effect'
class Pool extends Context.Tag('Pool')<Pool, { readonly size: number }>() {}
const grown: Layer.Layer<Pool> = Layer.suspend(() => base)
const base: Layer.Layer<Pool> = Layer.succeed(Pool, { size: 1 })
```

[CYCLE_GENUINENESS_DECIDES_THE_PAIR]:
- The minimal-pair count is a function of the dependency graph's cycle structure, computed not from how many declarations the naive form wrote but from how many genuine value-level back-edges the owner graph contains: a graph with zero back-edges collapses to ONE statement per owner after a topological sort, a graph with one genuine back-edge admits a deferral at exactly that edge raising the involved owners to a minimal pair, and a back-edge count of K introduces K deferral thunks — the pair count is the cardinality of the irreducible cycle set, never the declaration count.
- The repair for an ACYCLIC graph that fails at module-init is reordering alone, never a thunk: a forward reference among acyclic owners is a topological-sort violation, so moving the referenced owner above its referent closes the one statement with no second point — reaching for `suspend` to paper over an acyclic forward reference is the precautionary-deferral defect, paying a per-traversal allocation to avoid a free source move.
- The repair for a GENUINE cycle is the thunk and only the thunk, because no source order satisfies a back-edge: a value that references itself or a mutually-recursive sibling has no position where both bindings are already initialized, so reordering cannot close it and the deferral moves the read past module-init — the thunk is admitted precisely and only where the topological sort has no valid ordering, the one case the acyclic repair cannot reach.
- The forbidden third repair is the `var` hoist: a `var Owner` referenced before its initialization line reads `undefined` rather than throwing, so it converts a loud TDZ failure into a silent `undefined` propagated into a codec, a layer, or an effect — the hoist trades a compile-or-init error for a runtime corruption, and the lane forbids it absolutely because the minimal pair's whole purpose is to keep the failure loud, which the thunk does (it throws on a still-uninitialized read at first traversal) and the hoist defeats.

[GENUINE_VS_APPARENT_CYCLE]:
- A cycle visible to the type checker is not a value-level cycle, and the distinction is the lane's sharpest evaluation trap: the self-generic `class Self extends Factory<Self>(...)` is a type-level self-reference the checker resolves, but the heritage expression reads the NAME `Self` (a type, erased) not the BINDING `Self` (a value, in TDZ), so there is no value-level back-edge and the one statement closes — the apparent cycle is a type cycle the language resolves for free, and treating it as a value cycle by adding a `suspend` is the precautionary defect that misreads a type self-reference as a value one.
- A genuine value-level cycle is exactly the case where the heritage expression's eager arguments read a binding still in TDZ: a field schema that is the owner itself, a `dependencies` element that is the owner's own `Default`, an effect that references its own `const` — these read VALUES during eager evaluation, so they throw, and these alone require the thunk. The test is mechanical: trace whether the heritage's eager positions reference a value binding (not a type) that initializes at or after this line; a `yes` is genuine, a `no` is apparent.
- The Service owner makes both kinds visible in one declaration: a service whose `make` body does `yield* Other` references `Other`'s VALUE but in a deferred slot, so it is neither a module-init read nor a cycle that needs `suspend` — the requirement is discharged at provision through `R`, and a forward reference there is harmless; the same service's `dependencies: [Other.Default]` references `Other.Default`'s VALUE in an eager slot, so a forward reference there IS a module-init read and a genuine cycle between two services' `Default` layers is closed with `Layer.suspend`, not by reordering the class lines. The cycle's genuineness is decided per-slot, not per-owner.

```typescript
import { Effect } from 'effect'
class Source extends Effect.Service<Source>()('Source', { sync: () => ({ seed: () => 0 }) }) {}
class Derived extends Effect.Service<Derived>()('Derived', {
    effect: Effect.gen(function* () {
        const source = yield* Source
        return { next: () => source.seed() + 1 }
    }),
    dependencies: [Source.Default],
}) {}
const wired = Effect.provide(Derived.use((d) => Effect.sync(d.next)), Derived.Default)
```

[DEFERRED_BUILD_IS_NOT_A_PAIR]:
- The Layer-valued `Default` member is a build DESCRIPTION, not a built value, so an owner whose service reads another service is NOT a minimal pair even when the read crosses owners: the `make` body runs at Layer-build time, the `Default` materializes only when provided at the composition root, and the requirement flows through `R` — so the heritage line builds the Tag and a suspended layer description eagerly and the cross-owner read is deferred by construction, one statement closing what looks like a dependency that would force a pair.
- This is the reason the requirement graph and the evaluation graph are distinct surfaces: a service depending on five others through `yield*` is one statement with a five-element `R`, the dependencies discharged at provision, while the same five references written as eager `dependencies` array elements are five module-init reads that must topologically sort — so the SAME logical dependency is a deferred non-pair through the make body and an eager ordering constraint through the dependency array, and the author chooses which by where the reference lands.
- The deferred-build non-pair has one residual eager edge the author cannot defer: the `dependencies` array is read eagerly to wire the layer graph at the heritage line, so a service's `dependencies: [Other.Default]` requires `Other` declared above it OR the genuine-cycle thunk if the two depend on each other — the make body defers but the dependency wiring does not, so the only module-init ordering a Service owner imposes is among the layers named in its `dependencies`, never among the services it reads through `yield*`.

[CODEC_FIELD_CYCLE_FACES]:
- The recursive codec is the owner where the minimal pair's two points answer two DIFFERENT impossibilities at once, which is why neither can absorb the other: the value-side `Schema.suspend(() => Owner)` answers the temporal impossibility (the eager field read would throw on a TDZ binding), while the type-side opaque interface answers an inference impossibility (a `const` initialized from a self-referencing expression resolves to a circular `any`) — the pair is irreducible not because two statements happen to be needed but because the two impossibilities are orthogonal, a timing failure and a type-circularity failure that no single lever closes.
- The type half's CARDINALITY is itself derived from the cycle's shape, sharpening the pair count below the owner: `Schema.Schema.Type` and `Schema.Schema.Encoded` of a self-referential schema each name a distinct recursive shape, so a codec whose wire `I` diverges from its decoded `A` at the recursive position needs TWO opaque interfaces and a codec whose faces coincide there needs ONE — the minimal-pair count for a recursive codec is therefore one value point plus one-or-two type points, the type-point count reading off whether the transformation touches the recursive field, not a fixed "interface plus thunk".
- The two annotations on a recursive codec are read at two different times, which is the value-side analogue of the type/value impossibility split: the outer `const`'s annotation is read at name resolution and the inner thunk's `(): Schema.Schema<A, AEncoded> =>` annotation is read at first traversal when the body resolves the still-`undefined` binding — dropping the inner one collapses the recursive position to `Schema.Schema<unknown, unknown>`, so the temporal spread of WHEN each annotation is consumed is exactly why both are load-bearing rather than one being a restatement of the other.

```typescript
import { Schema } from 'effect'
interface Branch {
    readonly count: number
    readonly kids: ReadonlyArray<Branch>
}
interface BranchWire {
    readonly count: string
    readonly kids: ReadonlyArray<BranchWire>
}
const Branch: Schema.Schema<Branch, BranchWire> = Schema.Struct({
    count: Schema.NumberFromString,
    kids: Schema.Array(Schema.suspend((): Schema.Schema<Branch, BranchWire> => Branch)),
})
const admit = Schema.decodeUnknown(Branch)
```

[DESTRUCTURING_IS_AN_EAGER_NON_CYCLE]:
- The `Data.taggedEnum<E>()` destructuring is an eager synthesis that is structurally incapable of a value-level cycle, so its minimal pair is never a deferral: the call builds the constructor record and `$match` at the destructuring line reading only the type argument `E` (a type, erased), so the value side has no binding to forward-reference and the only ordering constraint is that the `Data.TaggedEnum<{ ... }>` alias be declared above — the pair is type-alias-then-value-destructuring, both eager, ordered by the type read alone.
- A recursive tagged union does NOT push the destructuring into a thunk, because the recursion lives in the type alias the destructuring reads, not in the value record it builds: a self-referential `Data.TaggedEnum` member whose payload references the union is a recursive TYPE the alias spells with no value-level back-edge, so the destructuring stays one eager statement — the codec-free family never needs `suspend` for recursion because it has no decode traversal to defer, the deferral being meaningless where there is no boundary walk.
- The split from the codec family is therefore a split in whether recursion has a runtime traversal to defer: a recursive `Schema.Union` of tagged classes that admits from `unknown` carries `Schema.suspend` at its recursive field because decode walks the tree, while a recursive `Data.taggedEnum` carries no thunk because its constructors build interior values with no walk — the same recursive shape is a deferred-thunk pair in the codec family and an eager type-alias pair in the codec-free family, the boundary deciding which.

[PAIR_COUNT_IS_A_SUM_OF_TWO_AXES]:
- The minimal-pair count an owner incurs is the SUM of two independent axes, not a single number, because `verbatimModuleSyntax` forces a second declaration point on an axis ORTHOGONAL to evaluation timing: the temporal axis counts genuine value-level back-edges (each a `suspend` thunk), and the erasure axis counts type-only derivations that must export under their own `export type` marker — neither axis can absorb the other's points, so an owner's total second-point count is `back-edge count + type-only-export count`, computed from two graphs the author reads separately.
- The two axes are orthogonal because one is decided by WHEN a value initializes and the other by WHICH namespace a name occupies, and a single owner can owe a point on each: a recursive codec owes a temporal point (the `suspend` thunk closing its value cycle) AND an erasure point (the `export type` carrying its opaque interface, which occupies only the type namespace and so cannot ride the value `export`), two second points on one owner answering a timing impossibility and an erasure impossibility that share no mechanism — the temporal pair stays a pair under any source order and the erasure pair stays a pair under any evaluation order.
- The sum is the lane's full lower bound on declaration count: where the chain-collapse thesis says a shape's first appearance is its only appearance, the bound that survives is "one statement plus its back-edge thunks plus its type-only export markers", every additional statement beyond that sum a re-grown chain link — so an owner with zero back-edges and a value-carried type is exactly one statement, an owner with one cycle and one extracted type face is exactly three points, and any count above the sum is the restatement the collapse deletes.

```typescript
import { Schema } from 'effect'
interface Cyclic {
    readonly tag: string
    readonly link: Cyclic | null
}
const Cyclic: Schema.Schema<Cyclic> = Schema.Struct({
    tag: Schema.String,
    link: Schema.NullOr(Schema.suspend((): Schema.Schema<Cyclic> => Cyclic)),
})
type CyclicWire = Schema.Schema.Encoded<typeof Cyclic>
export { Cyclic }
export type { CyclicWire }
```

[IMPOSSIBILITY_LEDGER]:
- type-level self-reference (`class Self extends Factory<Self>(...)`): ONE statement; the heritage reads the erased name, never the TDZ binding; no pair, no thunk; adding `suspend` is the precautionary defect.
- acyclic value forward-reference (field schema, `dependencies` element, or heritage base names a not-yet-declared owner): ONE statement after topological reorder; the referenced owner moves above its referent; no thunk — a `suspend` here pays a per-traversal allocation for a free source move.
- genuine codec cycle (a field schema is the owner or a mutually-recursive sibling): minimal PAIR — opaque interface(s) for the type half (two faces when the transform touches the recursive field, one otherwise) plus `Schema.suspend(() => Owner)` for the value half; the thunk defers the eager field read to first decode.
- genuine requirement cycle (two services' `Default` layers each in the other's `dependencies`): minimal PAIR closed by `Layer.suspend(() => other)` at the back-edge; the dependency array is the eager seam reordering cannot satisfy.
- genuine computation cycle (a recursive effect references its own binding): minimal PAIR closed by `Effect.suspend(() => self)`; the thunk defers the eager binding read to first run.
- cross-owner service read through `yield*`: ONE statement, NOT a pair; the `make` body is deferred to Layer-build and the requirement flows through `R`; only the `dependencies` array imposes a module-init order.
- codec-free recursive tagged union (`Data.taggedEnum` whose alias is self-referential): minimal PAIR of type-alias-then-destructuring, both eager; no thunk — recursion lives in the erased type, no decode walk to defer.
- type-only derivation under `verbatimModuleSyntax` (a name occupying only the type namespace): contributes one ERASURE-axis point independent of evaluation order, so an owner's total second-point count is `back-edge count + type-only-export count` and a recursive codec with an extracted face owes one temporal point AND one erasure point on the same declaration.

[EVALUATION_ANTIPATTERNS]:
- a `Schema.suspend`/`Layer.suspend`/`Effect.suspend` thunk wrapped around an acyclic owner: the precautionary-deferral defect — the eager read never throws for an acyclic owner, so the thunk adds a per-traversal allocation and a hop for a cycle that does not exist; the repair is deleting the thunk and reordering the source so the referenced owner precedes its referent.
- a `var Owner` hoist to defeat a forward reference: the silent-corruption defect — `var` hoists the binding to `undefined` and reads it without throwing, so the owner's codec/layer/effect carries `undefined` into a traversal that fails far from the cause; the repair is source reorder for an acyclic edge or a `suspend` thunk for a genuine cycle, both of which keep the failure loud where `var` makes it silent.
- a `suspend` reached for a TYPE-level self-reference (`Schema.suspend(() => Owner)` added beside a `Schema.Class<Owner>` that already names itself in its generic): the apparent-cycle misread — the heritage reads the erased name, so there is no value cycle to defer and the thunk is dead surface; the self-generic closes the type cycle for free and the value side was never circular.
- a `dependencies: [Other.Default]` element naming a service declared below: the eager-array module-init read — the dependency array evaluates at the heritage line, so a forward reference throws TDZ at construction even though the `make` body's `yield* Other` would have been fine; the repair is reordering when acyclic and `Layer.suspend` at the back-edge when the two services' layers are mutually dependent.
- a recursive codec annotated only on the outer `const` and not the inner thunk: the lost-recursion-type defect — the inner arrow body resolves to the still-`undefined` binding, so without the inner `(): Schema.Schema<A, AEncoded> =>` annotation the recursive position collapses to `Schema.Schema<unknown, unknown>` and the deep decode is untyped; both annotations are load-bearing because each is read at a different time.
- conflating the temporal and erasure axes when summing an owner's pair count: adding a `suspend` thunk to discharge an erasure-axis obligation (a forward reference the type-only export already handles) or an extra export point to discharge a temporal one mis-sums the pair, because the two axes never substitute — the repair is computing the count as `back-edge count + type-only-export count` from the two graphs separately, so a thunk lands only on a value back-edge and an `export type` lands only on a type-only name, and any second point not traceable to one specific obligation on one specific axis is the over-counted restatement the sum bounds away.
