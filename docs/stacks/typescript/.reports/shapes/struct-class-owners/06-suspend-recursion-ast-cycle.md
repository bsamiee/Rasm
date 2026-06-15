# Suspend-Recursion AST Cycle

[INITIALIZATION_ORDER_TRAP]:
- An owner's field record is evaluated as the argument to `Schema.Class<Self>("Id")(...)` BEFORE the `class Self extends …` binding exists, so a bare `Self` inside that record reads a temporal-dead-zone binding and fails twice over: the value reference throws `ReferenceError` at module evaluation, and the type reference reports `TS2506 'Self' is referenced directly or indirectly in its own base expression` plus, on the thunk, `TS7024 Function implicitly has return type 'any' because it does not have a return type annotation and is referenced … in one of its return expressions` — the two diagnostics are one cause, the checker chasing `Self`'s type through the base expression that produces it.
- `Schema.suspend(() => Self)` is the only legal forward reference because the thunk body is not evaluated at record-construction time: `suspend(f) = make(new AST.Suspend(() => f().ast))` builds an `AST.Suspend` whose `f` is unevaluated, so the field record completes and the `class Self` binding lands before any code reads `f`; the cut is deferring `Self` past class initialization, not a different reference to it.
- The explicit `Schema<A, I, R>` return annotation at the suspend site is mandatory exactly when the recursion makes the return type self-referential — the checker cannot infer the type of a thunk whose body returns the very binding whose type it is computing, so `Schema.suspend((): Schema.Schema<Self> => Self)` supplies the type the inference loop cannot close; the annotation is the type-level analogue of the runtime deferral, breaking the static cycle the same node breaks at runtime.
- The single-parameter `Schema.Schema<Self>` form holds only while the encoded face equals the instance type, and a class method or getter breaks that equality with a hard boundary: the `Proto` slot puts the method on `Self` but never on the encoded field record, so `Schema.Schema<Self>` (whose `Encoded` defaults to `Self`) rejects with `TS2322 'Encoded' … 'leaf' is missing in type '{ … }' but required in type 'Self'`, and trying to repair it with `Schema.Schema<Self, Schema.Schema.Encoded<typeof Self>>` deepens the loop into `TS2577 Return type annotation circularly references itself` — a recursive owner that carries behavior must therefore supply a hand-declared encoded interface as the `I` argument, the same encoded-face cut a mutual recursion forces, here triggered by the `Proto`-versus-codec divergence rather than by cross-reference.

```typescript
import { Schema } from 'effect';

// reject: a bare `Node` in the record reads the unbound binding — TS2506 on the class, TS7024 on the thunk:
//   children: Schema.Array(Node)                     // value: ReferenceError at module load
//   children: Schema.Array(Schema.suspend(() => Node)) // type: implicit-any return, recursive base
// the suspend thunk defers past initialization; the explicit Schema<Node> annotation closes the type cycle
class Node extends Schema.Class<Node>('Node')({
    value: Schema.Number,
    children: Schema.Array(Schema.suspend((): Schema.Schema<Node> => Node)),
}) {}

const admit = Schema.decodeUnknown(Node);
```

[DOUBLE_MEMOIZATION_LAW]:
- The cycle is broken at two distinct layers, each a `memoizeThunk` that fires its inner function exactly once and caches the result: the `AST.Suspend` constructor wraps its own `f` as `this.f = memoizeThunk(f)`, so the first `ast.f()` resolves the inner AST node once and freezes that node's identity for the lifetime of the schema; and the parser's `Suspend` arm builds `const get = memoizeThunk(() => goMemo(ast.f(), isDecoding))` returning `(a, options) => get()(a, options)`, so the inner parser is compiled once on first decode and the closure defers compilation past the cyclic build — neither layer re-resolves on subsequent traversals.
- Stable AST-node identity from the first memo layer is what makes the second layer's cache sound: the parser's `goMemo` is keyed on the AST node by `WeakMap` (`decodeMemoMap`/`encodeMemoMap`), and `AST.equals` for a `Suspend` returns `self === that` (reference identity, shared with `Refinement`/`TupleType`/`TypeLiteral`/`Transformation`/`Declaration`), never a structural walk — a structural comparison would itself recurse forever through the cycle, so the memoized thunk yielding one frozen node per schema is the precondition for every identity-keyed cache downstream.

[PROJECTION_FOLD_REWRAP_LAW]:
- A static-face fold over a `Suspend` re-wraps rather than resolves: `typeAST` returns `new Suspend(() => typeAST(ast.f()), ast.annotations)` and `encodedAST_` returns `new Suspend(() => encodedAST_(ast.f(), isBound), borrowedAnnotations)` — the fold is pushed inside a fresh lazy thunk, so projecting a recursive owner to its type face or encoded face yields another recursive schema whose cycle is intact, never an eagerly-unrolled infinite tree; `pick`, `partial`, `required`, `mutable`, and `rename` apply the same re-wrap, every structural operation preserving the suspension.
- The encoded-face fold mints a DERIVED runtime identifier for the recursion's wire form: when the suspended node carries a JSON identifier, `encodedAST_` borrows `{ [JSONIdentifierAnnotationId]: \`${identifier}Encoded${isBound ? "Bound" : ""}\` }`, so the type-side cycle and the encoded-side cycle resolve to distinct `$ref` identities in the projected document — a recursive owner's instance shape and its wire shape are two separate self-referential definitions at the AST level, the encoded one auto-named off the type one, never a second runtime schema declared beside the owner (orthogonal to the compile-time encoded `interface` an uninferable mutual cycle hand-declares for the checker).
- Every codec-derived compiler closes the cycle with one shared idiom, which is the collapse: the parser, `Schema.equivalence`, and `Pretty` each compute the `Suspend` arm as `const get = memoizeThunk(() => go(ast.f(), …)); return (…) => get()(…)` — a recursive owner therefore yields a terminating decoder, a terminating structural comparator, and a terminating renderer from the one declaration, the loose spelling of a hand-written depth-bounded walker per consumer being the surface this one node deletes across all derivations at once.

[JSON_SCHEMA_REF_CYCLE]:
- JSON-Schema generation breaks the cycle by pre-registering the `$ref` BEFORE recursing into the body: `go` writes `$defs[id] = { $ref: getRef(id) }` first, then overwrites `$defs[id] = go(ast, …, "ignore-identifier", …)` — so a node that re-encounters its own identifier during the body walk finds the `$ref` already seated in `$defs` and returns it, the placeholder self-reference being the termination, never an unrolled schema.
- A `Suspend` ALWAYS demands an identifier for JSON Schema: `go` forces identifier handling under `AST.isSuspend(ast)` even when `topLevelReferenceStrategy === "skip"`, and a suspended node whose resolved AST carries no identifier anywhere throws `Generating a JSON Schema for this schema requires an "identifier" annotation` at the suspended path — a recursive owner is JSON-Schema-projectable only when the recursion's reference target is named, the identifier being the `$ref` key the cyclic definition needs.
- The identifier requirement is satisfied automatically when the suspend thunk returns a named owner (a `Schema.Class` carries its constructor identifier as the JSON identifier, so `Schema.suspend(() => Self)` over a class needs no extra annotation), but an anonymous recursive struct must carry `.annotations({ identifier })` on the suspended schema or the struct it resolves to — the named class is the collapsed form, the bare struct the spelling that forces a manual identifier the class already supplies.

```typescript
import { JSONSchema, Schema } from 'effect';

// the recursion's reference target must be named: a `Schema.Class` supplies its own JSON identifier,
// so the cyclic `$ref` resolves with no extra annotation — `$defs` carries one self-referential definition.
class Folder extends Schema.Class<Folder>('Folder')({
    name: Schema.String,
    entries: Schema.Array(Schema.suspend((): Schema.Schema<Folder> => Folder)),
}) {}

const document = JSONSchema.make(Folder); // $defs: { Folder: { …, entries: { items: { $ref: "#/$defs/Folder" } } } }

// reject: an anonymous recursive struct throws "requires an identifier annotation" at the suspended path —
// the named class above is the spelling that already carries the $ref key the cyclic definition demands.
```

[GENERATION_DEPTH_DECAY]:
- A recursive owner's derived `Arbitrary` terminates by a shared depth budget, not by chance: the `Suspend` arm seeds `ctx.depthIdentifier = description.id` on first encounter and threads it through the recursion, and a suspended array field generates via `fc.oneof({ maxDepth: 2, depthIdentifier }, fc.constant([]), fc.array(item, constraints))` — the shared `depthIdentifier` makes the generator bias toward the empty arm as depth accrues and `maxDepth` caps the recursion, so sampling a self-referential schema yields finite trees with no stack overflow.
- The suspended-array generator clamps `maxLength` to `Math.max(2, minLength ?? 0)` inside the recursive context regardless of a wider `Schema.maxItems` annotation — a recursive collection field samples at most two children per level even when its non-recursive constraint admits more, the depth-decay overriding the breadth constraint precisely because an unclamped recursive breadth would multiply into unbounded trees; the annotation governs the leaf shape, the recursive context governs the fan-out.
- The sampler's cycle-break is a two-pass reference graph, not the parser's single deferral: a first `getDescription` pass converts a re-encountered `Suspend` into a `{ _tag: "Ref", id }` node via an `idMemoMap` keyed on the AST plus a monotonic counter, so only the first occurrence becomes a `{ _tag: "Suspend", id, description }` descriptor and every back-edge is a named `Ref` — the recursion is materialized as an explicit `letrec`-style graph whose `id` is exactly the `depthIdentifier` fast-check decays against, the back-edge naming preceding generation rather than being discovered during it.

[MUTUAL_RECURSION_CLOSURE]:
- Two owner families close a mutual cycle when each one's suspend thunk names the OTHER's eventual binding: `Folder.entries` holds `Schema.Union(File, Schema.suspend(() => Folder))` while a sibling owner referencing `Folder` does the same in reverse — each suspend defers past the point where both class bindings exist, so the AST cycle (Folder → Suspend → Folder, or A → Suspend → B → Suspend → A) closes through the lazy thunks rather than an initialization-order deadlock that no declaration order could satisfy.
- The mutual cycle forces an explicit encoded-side interface on at least one node: where a self-recursion can annotate `Schema.suspend((): Schema.Schema<Self> => Self)` (encoded type inferred), a mutual recursion whose encoded shape is itself recursive must declare the encoded record as an `interface` and supply it as the second type argument — `Schema.suspend((): Schema.Schema<Folder, FolderEncoded> => Folder)` — because the checker cannot infer an encoded type that names the encoded type it is computing; the hand-written `interface FolderEncoded` is the encoded-face cut, the type-level cost the mutual cycle imposes that a self-cycle over a primitive-encoded field avoids.
- Each member of a mutual cycle remains a full owner — `new`/`make`, structural `Equal`/`Hash`, instance methods, and per-family `_tag` survive, and the cycle rides only the field records — so a recursive union member that is a `TaggedClass` keeps its discriminant and its body behavior while participating in the cycle, the suspension wrapping the reference to the family member, never demoting it to a bare codec view.

```typescript
import { Schema } from 'effect';

// mutual recursion: the union member suspends the family owner, the family suspends through the member set.
// the explicit FolderEncoded interface is the encoded-face cut the mutual cycle forces — inference cannot
// close an encoded type that names itself, so one node declares it by hand and supplies it as the I argument.
class File extends Schema.Class<File>('File')({ name: Schema.String, size: Schema.Number }) {}

interface FolderEncoded {
    readonly name: string;
    readonly entries: ReadonlyArray<{ readonly name: string; readonly size: number } | FolderEncoded>;
}
class Folder extends Schema.Class<Folder>('Folder')({
    name: Schema.String,
    entries: Schema.Array(Schema.Union(File, Schema.suspend((): Schema.Schema<Folder, FolderEncoded> => Folder))),
}) {}

const admit = Schema.decodeUnknown(Folder); // walks the cycle through both lazy thunks, terminates by data depth
```

[COLLAPSE_TRIGGERS]:
- A self-referential entity modeled as a flat owner plus a hand-written recursive decoder plus a depth-counter that re-walks children is the collapse trigger: the recursion folds into one `Schema.suspend(() => Self)` field whose decoder, structural comparator, renderer, and depth-decaying sampler all derive from the one node — the manual recursive walker, the explicit visited-set, and the per-consumer depth guard are the exact spellings the suspended field deletes, every derivation closing the cycle through the same memoized thunk.
- Two owners cross-referencing each other through stringified ids or a sidecar lookup table — a parent holding child ids and a resolver that rehydrates them — collapse to a mutual `Schema.suspend` cycle when the relation is true containment: the embedded recursion makes `Schema.decodeUnknown` admit the whole nested shape in one pass, deleting the id-indirection table and the rehydration step the loose spelling threads at every read.
- A recursive owner paired with a hand-authored JSON Schema carrying a manually-spelled `$ref` and `$defs` entry is the collapse trigger: `JSONSchema.make` over the suspended owner pre-registers the `$ref` and overwrites it with the resolved body, so the cyclic schema document derives from the one declaration whose suspended node already names its reference target — the hand-written `$defs`/`$ref` pair and the discipline keeping it synchronized with the owner are the surface the identifier-bearing suspended node deletes.
