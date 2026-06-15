# Annotation Hook Node Locality And Totality

[THE_HOOK_IS_READ_BEFORE_THE_STRUCTURAL_SWITCH_AND_OWN_PROPERTY_ONLY]:
- The walk consults the node's `equivalence` hook FIRST and the structural fallback only on its absence: each visited node runs `getEquivalenceAnnotation(ast)`, and on `Some` it dispatches and returns immediately, never reaching the `_tag` switch that supplies the default — so a hook is not a tweak layered over the structural derivation, it is a total replacement of the node's comparator with the structural arm as the else-branch.
- `getAnnotation(key)` is `hasOwnProperty.call(ast.annotations, key)` — an OWN-property read on exactly this node's annotation record, never a prototype-chain or parent walk — so the override is strictly node-local: a hook on an owner node decides that owner's comparator and contributes zero to any child, and a hook on a child decides the child's comparator and is invisible to the parent's structural walk except as the leaf instance the parent's fold reads. Identity injection is one subtree overridden, every sibling and ancestor derived structurally, and the two never bleed.
- The own-property read makes cross-node conflict-freedom STRUCTURAL, not coincidental: because no node's hook lookup ever sees another node's annotations, there is no precedence rule to author when several nodes in one owner each carry a hook — the AST topology resolves it, an outer hook winning its whole subtree (it returns before recursing) and an inner hook winning only what its outer parent delegates down. The only way two hooks "conflict" is nesting, and nesting is total dominance: the nearer-the-root hook short-circuits before the walk can reach the deeper one, so a hook is overridden only by a hook strictly above it on the same path, never beside it.

[THE_HOOK_ARITY_IS_A_THREE_WAY_TAG_SWITCH_INSIDE_THE_HOOK_BRANCH]:
- The hook's call shape is decided by a SECOND `_tag` switch nested inside the `isSome` branch, disjoint from the fallback switch: a `Declaration` hook is invoked `hook(...ast.typeParameters.map((tp) => go(tp, path)))` — N-ary over the node's recursively-derived type-parameter equivalences; a `Refinement` hook is invoked `hook(go(ast.from, path))` — unary over the base's derived equivalence; every other node's hook is invoked `hook()` — nullary. The arity is not a convention the hook author observes, it is the literal call the walk emits keyed on the node it sits on.
- A generic owner's derived identity is therefore itself parameterized by its members' identities: the `Declaration` hook receives the FULLY-DERIVED equivalence of each type parameter (the walk recurses `go(tp)` before calling the hook), so a container declaration's comparator is a function of its element comparator and composes without the author ever naming the element type — the same shape `Array.getEquivalence`/`Option.getEquivalence` expose as a runtime lift, here as the declaration's build-time hook.
- The path is threaded through STRUCTURAL descent but not through type-parameter derivation: the `Declaration` hook derives each parameter with `go(tp, path)` — the SAME path, never `path.concat`ed — while the structural `TupleType` arm advances `path.concat(i)` per position and `TypeLiteral` advances `path.concat(ps.name)` per property. A failure inside a type-parameter subtree therefore reports the OWNER declaration's path, not a synthetic parameter index, while a failure inside a structural field reports the precise field route — the path granularity tracks whether the descent crossed a declaration boundary or a structural one.

```typescript
import { Equivalence as Eq, ParseResult, Schema as S } from 'effect';

// the N-ary Declaration hook receives the FULLY-DERIVED element equivalence and returns the
// container equivalence parameterized over it — the typed EquivalenceAnnotation<A, [Elem]> arity
// is checked because this is the parameterized declare channel, not the universal .annotations()
const Boxed = <A, I>(item: S.Schema<A, I>): S.declare<ReadonlyArray<A>, ReadonlyArray<I>, readonly [S.Schema<A, I>]> =>
    S.declare(
        [item],
        {
            decode: (it) => (input, opts) => ParseResult.decodeUnknown(S.Array(it))(input, opts),
            encode: (it) => (input, opts) => ParseResult.encodeUnknown(S.Array(it))(input, opts),
        },
        {
            // unary call here, but typed N-ary: the hook's arity is bound to typeParameters length,
            // and the walk supplies each `elem` by recursively deriving go(tp) before invoking it
            equivalence: (elem: Eq.Equivalence<A>) =>
                Eq.make((x, y) => x.length === y.length && x.every((v, i) => elem(v, y[i]))),
        },
    );

const eq: Eq.Equivalence<ReadonlyArray<string>> = S.equivalence(Boxed(S.String));
```

[THE_TOTALITY_BOUNDARY_IS_A_SINGLE_THROW_AT_NEVERKEYWORD_AND_IT_IS_PER_DERIVATION]:
- `NeverKeyword` is the SOLE node whose structural arm throws across the entire equivalence walk — every other leaf folds to `Equal.equals`, every composite recurses, and the throw fires at `Schema.equivalence(schema)` BUILD time, not at the first `(a, b)` comparison, so a degenerate field is caught when the comparator is constructed and never when two values meet. Derivation safety is thereby a property of the type's inhabitedness checked once: a `Never`-decoded field is uninhabited, has no value to compare, and is the only shape that cannot yield a total comparator; every well-formed owner short of one produces a terminating equivalence regardless of nesting or cycles.
- The throwing-node SET is itself derivation-relative, and the contrast pins what "inhabited enough to compare" means: the Arbitrary derivation treats a bare `Declaration` AND `NeverKeyword` identically — both `absurd(...)` because neither can GENERATE a value without a hook — while the equivalence walk throws only on `NeverKeyword` and degrades a hookless `Declaration` to `Equal.equals`. An opaque carrier is reference-comparable (you can ask "are these the same handle") but not generatable (you cannot synthesize one), so equivalence's totality boundary is strictly LOOSER than arbitrary's: comparing needs only a value in hand, generating needs a recipe.
- The build-time throw self-locates by both axes: the error embeds `at path: ${formatPath(path)}` — the bracketed field route the structural descent accumulated — AND `schema (${ast._tag})` — the node tag — so the single fatal point names the precise subtree AND the kind of node that defeated derivation. The path is non-empty only when the `Never` sits under a structural descent that `path.concat`ed; a top-level `Schema.Never` throws with an empty path, the node tag alone identifying it.

```typescript
import { Equivalence as Eq, Schema as S } from 'effect';

// every node here folds or recurses: the build SUCCEEDS and yields a total comparator —
// inhabitedness, not nesting depth, is the safety property
class Node extends S.Class<Node>('Node')({
    label: S.String,
    weight: S.Number.pipe(S.int()), // refinement is invisible to identity: compares as plain number
    payload: S.Unknown, // folds to Equal.equals — the latent reference-identity trap on this field alone
}) {}
const total: Eq.Equivalence<Node> = S.equivalence(Node);

// reject: a Never-decoded field is the ONE shape that throws at BUILD time, naming `at path: [slot]`
// and `schema (NeverKeyword)` — the throw fires here, never on a later compare of two values
const Degenerate = S.Struct({ slot: S.Never });
const build = (): Eq.Equivalence<{ readonly slot: never }> => S.equivalence(Degenerate);
```

[THE_ARITY_TYPE_CHECK_LIVES_ONLY_ON_THE_PARAMETERIZED_DECLARE_CHANNEL]:
- The hook's typed arity is enforced through exactly one surface: the parameterized `Schema.declare(typeParameters, options, annotations)` accepts `Annotations.Schema<A, { readonly [K in keyof P]: Schema.Type<P[K]> }>`, whose `equivalence?: EquivalenceAnnotation<A, TypeParameters>` is `(...equivalences: { readonly [K in keyof TypeParameters]: Equivalence<TypeParameters[K]> }) => Equivalence<A>` — the hook's parameter tuple is type-coupled to the declaration's `typeParameters` array, so a wrong-arity hook is a tuple-length mismatch the checker reports at the declaration site.
- The universal `.annotations()` method ERASES that check: it accepts `Annotations.GenericSchema<A>` whose `equivalence?: (..._: any) => Equivalence.Equivalence<A>` is a variadic `any` — so attaching an `equivalence` hook through `schema.annotations({ equivalence: ... })` carries NO arity contract, and a hook authored with the wrong parameter count type-checks at attach time and only the runtime call shape (which the walk emits by `_tag`) decides what it actually receives. The typed-arity guarantee is a property of the `declare` constructor's annotation slot, not of the annotation key itself.
- The consequence is a channel discipline: a generic owner whose identity depends on its parameters' identities MUST be declared through parameterized `declare` to get the arity wall, because a nullary node (`StringKeyword`, `TypeLiteral`, a monomorphic `declare`) whose hook is mistakenly authored N-ary is called `hook()` by the walk and silently ignores its declared parameters, comparing by whatever the zero-argument body returns. The node `_tag` the walk switches on, not the hook's own signature, is the authority on how many derived equivalences arrive.

[SUSPEND_IS_DOUBLY_MEMOIZED_AND_KEYED_BY_NODE_INSTANCE_NOT_BY_DEPTH]:
- A cyclic owner terminates because `Suspend` is memoized at TWO layers that compound: the AST node's own thunk is `this.f = memoizeThunk(f)` in its constructor, so `ast.f()` resolves the suspended child AST exactly once per node instance for its lifetime; the equivalence walk then wraps that in a SECOND `memoizeThunk(() => go(ast.f(), path))` and returns `(a, b) => get()(a, b)`, so the built comparator for the suspended subtree is constructed once on first comparison and reused thereafter. The self-reference never re-walks the AST and never re-builds the comparator.
- The memoization is keyed by the `Suspend` NODE instance, never by value depth: a recursive owner reaching itself through one shared `Suspend` node yields ONE built equivalence that every level of an arbitrarily deep structure reuses, so the comparator is built at constant cost and applied to unbounded depth — depth is a runtime property of the values compared, the comparator a build-time property of the cycle's single suspension point. A tree-shaped owner with N distinct recursive positions has N `Suspend` nodes and N memo cells, each independent.
- The deferred build inverts WHERE the totality boundary fires for a suspended subtree: because `go(ast.f())` runs inside the thunk and the thunk fires on first INVOCATION, a `Suspend` wrapping an uninhabited node does NOT throw at `Schema.equivalence` build time — it throws on the first comparison that forces the thunk, the one place the otherwise-build-time `NeverKeyword` guard slips past construction. So the inhabitedness check is build-time everywhere EXCEPT behind a suspension, where it is deferred to first use exactly as the comparator itself is, and a recursive owner with a degenerate suspended branch reports total at build and fatal at the first compare that reaches that branch.

```typescript
import { Equivalence as Eq, Schema as S } from 'effect';

interface Tree {
    readonly tag: string;
    readonly kids: ReadonlyArray<Tree>;
}
// the single Suspend node anchors the cycle; its derived equivalence is built once and reused at
// every depth — the comparator is a build-time property of the suspension, depth a runtime one
const Tree: S.Schema<Tree> = S.suspend(() =>
    S.Struct({ tag: S.String, kids: S.Array(Tree) }),
);

const sameTree: Eq.Equivalence<Tree> = S.equivalence(Tree);
const cyclic: boolean = sameTree(
    { tag: '<root>', kids: [{ tag: '<leaf>', kids: [] }] },
    { tag: '<root>', kids: [{ tag: '<leaf>', kids: [] }] },
);
```

[THE_HOOK_VERSUS_FALLBACK_DIVERGE_ON_THE_REFINEMENT_NODE_ALONE]:
- `Refinement` is the one node where the hooked and hookless paths read the SAME child but call it differently: with a hook the walk runs `hook(go(ast.from, path))` — unary over the base equivalence, so the hook can REFINE the base comparator (tighten or replace it) while still receiving it; without a hook the walk runs `go(ast.from, path)` directly — the base equivalence passed through verbatim, the predicate invisible. So a refinement's identity is either "the base, optionally narrowed by a supplied hook" or "exactly the base", never "the base intersected with the predicate" — the runtime check is a decode-time admission gate, never an identity lever, unless a hook explicitly makes it one.
- This is why two refined values are equal iff their bases are equal absent a hook: `Schema.Number.pipe(Schema.int())` derives `go(NumberKeyword)` = `Equal.equals`, the `int` predicate contributing nothing, so the refinement is a decode-time admission gate the identity walk steps straight through — and a hook on the refinement node is the ONLY lever that makes a refinement carry distinct identity, by supplying a comparator that consults the narrowed shape the base ignores. The unary hook arity exists precisely so a refinement override can build on the base rather than discard it.
- The refinement node is therefore the canonical injection seam, and its hook composes WITH the base rather than replacing the subtree: because the hook receives `go(ast.from)`, an author who narrows identity (a tolerance-banded number, a case-folded string) writes the override as a wrapper around the base comparator, never a from-scratch one — the unary call shape hands the base in so the refinement's custom identity is a strictly-finer or strictly-coarser transform of it, the one node where the hook is a modifier on its child rather than a total replacement of it.

[THE_DEGRADE_NODES_ARE_THE_LATENT_REFERENCE_TRAP_THE_THROW_DOES_NOT_CATCH]:
- The totality boundary catches uninhabited fields but is SILENT on the broadly-inhabited ones that degrade to reference identity: `UnknownKeyword`, `AnyKeyword`, and a hookless `Declaration` each fold to `Equal.equals` and BUILD without error, so an `unknown`-typed or opaque-declared field admitted past the boundary deduplicates and tests membership by REFERENCE while the build reports success — the comparator is total (it returns an answer for every pair) yet coarsened to reference at that node, and no build-time signal distinguishes "correctly structural" from "silently reference". The throw guards inhabitedness, never identity grain.
- The trap is located precisely at the degrade nodes and is closed precisely by the hook at the same node: a `Schema.declare`d opaque carrier given an `equivalence` hook compares by the hook's body, and the same carrier without one compares by reference — so the node that the totality walk treats as safe-because-inhabited is the node whose identity the author must decide, and leaving it hookless is choosing reference identity, not omitting a decision. The build's silence on a degrade node is the signal to supply the hook the parameterized `declare` channel arity-checks.
- An empty `TypeLiteral` (no property signatures, no index signatures) also degrades to `Equal.equals` rather than building a key-walking comparator, so an owner that decodes to `{}` compares by reference — the structural arm only earns its `Object.keys` walk when at least one signature exists, and a record that loses all its fields (a projection, a transform to empty) silently reverts to reference identity at exactly the node a non-empty record would compare structurally. The degrade-to-`Equal.equals` is the uniform fallback for "nothing structural to compare here", whether the node is opaque, unknown, or vacuously empty.
