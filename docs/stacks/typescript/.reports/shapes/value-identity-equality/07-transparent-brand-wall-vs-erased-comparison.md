# Transparent Brand Wall Vs Erased Comparison

[THE_WALL_AND_THE_COMPARISON_ARE_TWO_OPPOSITE_VARIANCES]:
- The nominal wall stands on a variance the comparison does not share: `Brand<in out K>` is INVARIANT in its phantom key while `Equivalence<in A>`/`Order<in A>` are CONTRAVARIANT in the compared value — so the same brand that two contravariant cells silently erase is the brand two invariant phantom records refuse to unify, and the wall is precisely the gap between these two variances, never a property either carries alone.
- The contravariance is the admission rail: a leaf typed over the wide base (`Eq.string`, `Order.number`) is assignable INTO a cell typed over the narrow branded subtype because `Code` extends `string`, and `Equivalence<in A>` flips the subtype relation so `Equivalence<string>` is assignable to `Equivalence<Code>` — the coarse instance flows in unwidened with no cast because contravariance is exactly the law "a comparator over the supertype serves the subtype". The brand wall declared in the cell's type parameter and the base leaf admitted through it are one assignment the variance licenses, never a coercion.
- The invariance is the rejection rail: `Brand<K>` is `{ readonly [BrandTypeId]: { readonly [k in K]: K } }`, a mapped record keyed by the literal `K`, so two distinct keys produce two record types neither assignable to the other, and `in out K` forbids `Brand<"A">` from flowing where `Brand<"B">` is wanted in either direction — the value-level `string & Brand<"A">` and `string & Brand<"B">` share the `string` half (contravariance would pass it) yet diverge on the invariant intersected half (invariance blocks it), so the conjunction `string & Brand<K>` is assignable iff the bases are compatible AND the keys are identical, the wall living entirely in the second conjunct.

```typescript
import { Brand, Equivalence as Eq } from 'effect';

type Code = string & Brand.Brand<'Code'>;
type Slot = string & Brand.Brand<'Slot'>;

// contravariance ADMITS the coarse base leaf into the branded cell with no cast
const byCode: Eq.Equivalence<Code> = Eq.string;
const bySlot: Eq.Equivalence<Slot> = Eq.string;

// invariance REJECTS cross-brand assignment at the same seam: the bases unify, the phantom keys do not
// @ts-expect-error two distinct brand keys are invariant — Equivalence<Slot> is not Equivalence<Code>
const crossed: Eq.Equivalence<Code> = bySlot;
```

[THE_ERASURE_IS_LOCALIZED_TO_THE_PROJECTION_RETURN_TYPE]:
- `mapInput`'s signature `<B, A>(f: (b: B) => A) => Equivalence<B>` is where the brand dies by construction: the projection's INPUT `B` is the branded narrowing the cell preserves, its OUTPUT `A` is the base the leaf compares, so the brand survives exactly up to the lambda's argument and is gone at its return — `Eq.mapInput(Eq.string, (r: { code: Code }) => r.code)` hands the leaf a value typed `string`, never `Code`, and the leaf never re-narrows. The erasure is not a runtime stripping but a type-level fact of which side of `f` each type sits on.
- The same lambda is the seam where the wall is deliberately re-imposed or deliberately dropped: typing the projection `(r) => r.code` returning `Code` keeps the brand one hop further (a downstream `combine` arm could still demand it), while widening the return to `string` erases it at `mapInput` — so the author chooses the erasure depth by the projection's annotated return type, and a coarsening that returns the base is the canonical, non-accidental erasure, distinct from the reflexive-corner accidental exactness a reference gate produces.
- Contravariance composes the erasure cleanly across a fold: a `combine` of `mapInput`-projected base leaves is an `Equivalence<Owner>` every arm of which read a base value, so the aggregate's input type is the owner (branded fields and all) while every arm's comparison ran on erased bases — the brand wall is preserved at the aggregate's INPUT boundary (the cell still rejects a wrong-branded owner) and absent from every internal comparison, the two facts non-interfering because input variance and body erasure are independent axes of the one instance.

[THE_SCHEMA_BRAND_IS_A_REFINEMENT_THE_WALK_STEPS_THROUGH]:
- `Schema.brand`/`fromBrand` build a `Refinement` AST node whose `from` is the base schema, and the derived-equivalence walk's `Refinement` arm is `go(ast.from, path)` — it descends straight to the base and the brand contributes nothing, so a branded schema's derived `Equivalence` IS the base schema's derived `Equivalence`, the brand-name array annotation and the refinement predicate both stepped over in one recursion. The schema-side erasure and the combinator-side erasure are the same fact reached by two routes: the brand never reaches a comparator on either path.
- The brand stack is an accumulating array annotation (`BrandAnnotation = NonEmptyReadonlyArray<string | symbol>`, each `Schema.brand` appending `[...existing, name]`), so stacking N brands is N entries on ONE refinement chain, and the equivalence walk steps through every layer to the same base — the runtime identity of a triple-branded schema and a bare schema over the same base is byte-identical, the three phantom tags existing only in `Schema.Type<S> & Brand<B1> & Brand<B2> & Brand<B3>` the type carries.
- The brand is the canonical hookless refinement: it carries a `BrandAnnotationId` array and a predicate, never the `EquivalenceAnnotationId` hook the walk consults before its structural arm, so the two annotations sit on one node under different keys and the walk reads only the latter — the brand's transparency is not a choice the derivation makes but a consequence of the brand occupying a different annotation key than identity does. A brand that wanted runtime-distinct identity would attach the equivalence key alongside the brand key on the same refinement, abandoning transparency deliberately; the default is transparent because the two keys never collide.

```typescript
import { Equivalence as Eq, Schema as S } from 'effect';

// three stacked brands accumulate as one Refinement chain; the derived equivalence walks
// go(ast.from) through every layer to S.String — the brand annotations never reach a comparator
const Tagged = S.String.pipe(S.brand('Code'), S.brand('Region'), S.brand('Slot'));
const derived: Eq.Equivalence<typeof Tagged.Type> = S.equivalence(Tagged);

// identical verdict to the bare base: the refinement-as-admission-gate leaves the grain untouched,
// so two distinct-brand-stack values over equal bases are Equal here and forbidden only at the cell type
const bare: Eq.Equivalence<string> = S.equivalence(S.String);
const sameGrain: boolean = derived('AB12' as typeof Tagged.Type, 'AB12' as typeof Tagged.Type) === bare('AB12', 'AB12');
```

[BRAND_ALL_NARROWS_THE_OUTPUT_TYPE_NEVER_THE_FOLD]:
- `Brand.all`'s narrowing is split across two distinct type-level surfaces that bracket the constructor on opposite sides: `EnsureCommonBase` is INPUT-side, folding every input brand down to one shared unbranded base (a mismatch collapses the tuple element to the literal-type error `"ERROR: All brands should have the same base type"`), while `UnionToIntersection<{ [B in keyof Brands]: FromConstructor<Brands[B]> }[number]>` is OUTPUT-side, conjoining every brand's invariant phantom record. The constructor therefore accepts the common base and emits a multiply-branded narrowing, and because the surfaces are disjoint the fold a consumer runs lives entirely on the base `EnsureCommonBase` extracted, never on the intersection the output carries.
- The intersection narrows what a row CELL admits, not what the fold computes: a cell typed `Eq.Equivalence<Code & Slot>` rejects a value carrying only `Brand<'Code'>` (the invariant `Brand<'Slot'>` conjunct is missing) yet the `Eq.string` leaf inside that cell never inspects either phantom — so `Brand.all(IsCode, IsSlot)`'s contribution is entirely at the assignment boundary the contravariant cell guards, and the comparison body is the same erased base comparison a single brand or no brand produces. Two values carrying DIFFERENT brand stacks over equal bases are `Equal.equals` at runtime (the intersections are erased) and non-interchangeable at the type seam (the intersections are invariant and distinct).
- `UnionToIntersection` is the mechanism that makes the wall TIGHTER as brands stack while the comparison stays flat: each added brand is one more invariant conjunct in the cell's required intersection, so the cell admits a strictly smaller set of source types, yet the derived comparator's discriminating power is unchanged because every conjunct erases on the way to the leaf — the type-level admission set and the runtime equality class move in opposite directions, the admission set shrinking with each brand and the equality class fixed at the base's grain.

[THE_WALL_GUARDS_ASSIGNMENT_NOT_MEMBERSHIP]:
- A branded value dropped into a `HashSet`/`HashMap` is located by the base's `Hash` and settled by the base's `Equal`, both blind to the phantom, so a "branded key" confers ZERO runtime membership distinction: two values of different brand stacks over the same base hash to one bucket and compare equal, dedup to one entry, and test present against each other — the brand protects the construction-site assignment that fed the structure, never the membership the structure decides. The wall is a compile-time gate on what may ENTER a branded position, not a runtime partition of what is THE SAME inside the structure.
- The brand is equally powerless over a captured-`Order` key: a `SortedSet`/`RedBlackTree` whose comparator is a base leaf admitted through the contravariant cell carries no phantom into its body, so a branded sorted key is exactly as discriminating as its unbranded base and the brand's sole effect is forbidding an unbranded value at the construction seam. The wall cannot raise a captured comparator's discriminating power one notch above the base, because the base leaf is what contravariance let into the cell in the first place — the brand and the membership grain are sourced from opposite ends of the same instance and never meet.
- This locates the precise hazard the brand creates rather than prevents: a developer reading `HashSet<Code>` expects `Code` membership to be brand-aware and receives base membership, so a value laundered out of its brand (via `Brand.unbranded` then re-branded under a different key, or admitted as the bare base and asserted) is membership-indistinguishable from a correctly-branded one. The brand is a reminder at the assignment seam, never an invariant the hashed or sorted structure can enforce — the structure has no phantom to read, and the only durable identity an identity-keyed structure carries is the one the base's `Equal`/`Order` computes.

```typescript
import { Brand, HashSet } from 'effect';

type Code = string & Brand.Brand<'Code'>;
type Slot = string & Brand.Brand<'Slot'>;
const Code = Brand.nominal<Code>();
const Slot = Brand.nominal<Slot>();

// the cell type forbids a bare string or a Slot at the assignment seam (compile-time wall)
const codes: HashSet.HashSet<Code> = HashSet.fromIterable([Code('AB12'), Code('AB12')]);

// but membership is decided by the erased base: a Slot over the equal base routes and settles equal,
// so the brand wall NEVER reaches the bucket — present is true across the brand boundary
const distinct: number = HashSet.size(codes); // 1, not 2
const crossBrandPresent: boolean = HashSet.has(codes, Slot('AB12') as unknown as Code);
```

[THE_GUARD_RE_ADMITS_BY_PREDICATE_NEVER_RESTORES_THE_PHANTOM]:
- `Brand.Constructor` exposes the only runtime brand surface — the throwing call, `.option`, `.either`, and the `.is` guard typed `(a: Unbranded<A>) => a is Unbranded<A> & A` — and every one tests the PREDICATE, never the phantom: a `nominal` brand's `.is` is unconditionally `true` (no predicate), a `refined` brand's `.is` runs only the refinement, so re-admitting a value to the branded type is a predicate decision that re-stamps the type-level tag onto a value that never lost it at runtime. There is no runtime phantom to check because the brand was never present at runtime; `.is` widens the static type back to the branded narrowing by asserting the predicate held.
- This closes the asymmetry the wall implies: a value that crosses the erased comparison (two brand-stacks comparing equal) cannot be re-separated by any runtime guard, because no guard reads the phantom — `.is` can only re-assert the predicate, which both stacks pass identically over the equal base. The wall is therefore one-directional in time: it forbids the wrong-branded value from entering a position at COMPILE time, but once two correctly-branded-but-differently-stacked values have been erased into one runtime comparison, no constructor surface recovers the distinction.
- The single-constructor-many-modalities shape is the collapse the brand owner makes inevitable: `nominal`, `refined`, `.option`, `.either`, `.is`, and `Brand.all`'s intersection are members of one `Brand.Constructor`, never a `makeCode`/`tryCode`/`isCode`/`validateCode` family — the throw call is the strict construction, `.either` the typed-rail admission carrying `BrandErrors`, `.option` the absence channel, `.is` the narrowing guard, so the brand's whole modality set discriminates by member off one constructor and the reject is a sibling-factory family restating one of them by name.

```typescript
import { Brand } from 'effect';

type Code = string & Brand.Brand<'Code'>;
// one constructor owns throw / option / either / is — never sibling makeCode/tryCode/isCode factories
const Code = Brand.refined<Code>(
    (raw) => raw.length === 4,
    (raw) => Brand.error(`expected 4 chars, got ${raw.length}`),
);

// .is re-admits by the PREDICATE, re-stamping the type-level tag on a value that never carried it at runtime
const admit = (raw: string): Code | string => (Code.is(raw) ? raw : raw);

// reject: a parallel guard family restates the constructor's own .is member by a renamed name
const isCode = (raw: string): raw is Code => raw.length === 4;
```

[THE_TWO_CHANNELS_DELIVER_ONE_BASE_IDENTITY_UNDER_TWO_DIFFERENT_WALLS]:
- The auto-derived `Schema.equivalence` (refinement-transparent) and the hand-built `Struct.getOrder`/`Struct.getEquivalence` table (leaf-erased through `mapInput`) both deliver the base's identity, but they impose the brand wall at different binding sites — the derived instance carries the brand only in its inferred `Schema.Type<S>` input type, the table carries it in each cell's annotated type parameter — so the same base identity reaches two consumers behind two independently-authored walls, and a field promoted to a new brand stack must be re-typed at BOTH the schema declaration and the table cell or the two surfaces guard different brand sets while computing identical comparisons.
- The wall thus rides on top of the ordering-derivation gap rather than closing it: ordering does not self-propagate from the schema, so a branded sort key is the one surface where the brand wall AND the missing field both surface only at the hand-composed `Order.struct` — anchoring that table with `satisfies { readonly [K in keyof Owner]: Order.Order<Owner[K]> }` over the owner's literal field record makes a renamed branded field a key error at declaration, recovering the compile-time propagation for the wall the derivation gives the equivalence for free. The brand wall is checkable exactly where the field-key vocabulary is anchored, and unchecked exactly where a comparator is spelled by hand off an un-anchored cell.
- The contravariance that admits the base leaf into the branded cell is the same law that lets one `Struct.getEquivalence` over a `string`-keyed record satisfy a target keyed by the branded subtype with no cast, so the table cells hold bare base leaves and the aggregate's inferred input type carries the brands — the wall is declared once in the table's `satisfies` anchor and inherited by every aggregate the table folds into, never re-stated per consumer, the brand existing only in the type the fold's input boundary preserves and absent from every projection the fold runs.
