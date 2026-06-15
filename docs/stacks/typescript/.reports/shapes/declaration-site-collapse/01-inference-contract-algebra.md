# Inference-contract algebra

[ONE_CONTRACT_LAW]:
- A lever is a position, not a fuser: each literal-preservation operator is defined entirely by WHICH syntactic position it pins against the one widening default the language applies there, so the levers are not a menu of independent capabilities but one law — keep the inferred type at its narrowest — instantiated once per position. The consequence is an algebra rather than a checklist: two levers pinning the same position are redundant by definition, two pinning different positions of one declaration compose by definition, and the question "which levers does this declaration need" reduces to "which positions here face a real pressure".
- The widening pressures are a fixed alphabet, one per position: a value literal widens to its base primitive, an array literal widens to `Array<element>` losing tuple shape and order, an object literal widens to a structural record losing per-property literals and `readonly`, an inferred type parameter widens to the constraint, an annotated binding `: T` widens to exactly `T`. A lever is chosen by naming the pressure at its position; choosing a lever that pins a position no pressure threatens is the redundant-operator defect.
- The contract composes left-to-right along the declaration: a head lever fixes the narrow source type, intermediate positions derive from it by `Extract`/`Pick`/indexed access, and a tail position consumes the accumulated narrowness as a proof obligation. The choice at the head decides whether the tail can close — a widened head propagates a widened union the whole length of the pipeline, so a single missing lever at position zero is a failure surfaced only at the terminal clause, far from its cause.

[PRESSURE_HANDOFF]:
- The scrutinee-side and pattern-side levers are not parallel but sequential entry points feeding one union: `Match.value<const I>(i)` pins the scrutinee's literal union into the matcher's `R` slot, and each `Match.when<R, const P extends Types.PatternPrimitive<R>>(pattern, f)` narrows by `Types.WhenMatch<R, P>` whose terminal arm is `ExtractMatch<R, P>` — the pattern is intersected against the `R` the head lever fixed, never inferred in isolation. The handoff is the mechanism: position one supplies the closeable union, position two supplies the selector, and narrowing is `Extract<R, selector>`.
- `<const I>` on the scrutinee is load-bearing precisely because the handoff multiplies it: a widened `R` makes every downstream `Extract<R, P>` extract from a base primitive, so each arm's payload widens and the exhaustiveness proof can never reach `never`. The head lever's reach is therefore the whole pipeline's reach, not one arm's — the collision "value widened by `: T` before the lever sees it" is fatal here not at the head but at the tail, where the proof obligation it broke is finally checked.

```typescript
import { Match } from 'effect'
type Token = { readonly kind: 'num'; readonly value: number } | { readonly kind: 'sym'; readonly name: string }
const fold = (t: Token) =>
    Match.value(t).pipe(
        Match.when({ kind: 'num' }, (n) => n.value),
        Match.when({ kind: 'sym' }, (s) => s.name.length),
        Match.exhaustive,
    )
```

[REDUNDANCY_PROVEN_IN_THE_TYPE]:
- Two pattern selectors expose the redundancy law as a signature difference: `Match.when<R, const P extends Types.PatternPrimitive<R>>` carries `const P` because its argument is an arbitrary inline pattern object that widens without it, while `Match.tag<R, P extends Types.Tags<'_tag', R> & string>` and `Match.discriminator(field)<R, P extends Types.Tags<D, R> & string>` carry NO `const` — `Types.Tags<'_tag', R>` reads the already-narrow `_tag` literal union out of the `R` the scrutinee-side lever pinned, so `P` is constrained to a subset of a literal union and widening is structurally impossible. The pattern position needs a literal-preservation lever exactly when its type is inferred fresh, and needs none when its type is derived from an upstream-pinned union.
- This is the redundancy collision generalized: a lever at a position whose narrowness is already guaranteed by an upstream lever is deletable noise, and the deletion is not a style choice but the absence of `const` in the verified signature. `Match.tag` and `Match.discriminator` spell their selectors as rest spreads `[first: P, ...values: Array<P>, f]`, so they stack two redundancy-eliminating mechanisms — derived-from-`R` constraint AND rest-position capture — and an `as const` on a tag literal passed to either buys nothing the constraint already enforces.

```typescript
import { Match } from 'effect'
type Event = { readonly _tag: 'open'; readonly at: number } | { readonly _tag: 'shut'; readonly code: number }
const react = (e: Event) =>
    Match.value(e).pipe(
        Match.tag('open', (o) => o.at),
        Match.discriminator('_tag')('shut', (s) => s.code),
        Match.exhaustive,
    )
```

[TERMINAL_PROOF_OBLIGATION]:
- The contract's tail is a `never`-typed proof slot, not a runtime check: `Match.exhaustive` accepts only `Matcher<I, F, never, A, Pr, Ret>` — its scrutinee-remainder type parameter must be `never`, and each preceding arm drives that slot down by `Types.AddWithout<F, Extract<R, P>>` subtracting the matched member. The arms are the proof steps and `exhaustive` is the QED that the union was fully consumed; a leftover member leaves the slot a non-`never` type and `exhaustive` fails to type-check on the matcher, never on the missing arm.
- The head lever and the terminal proof are therefore one obligation split across the declaration: `<const I>` makes `R` a closed literal union, the arms subtract members, and `exhaustive` proves the subtraction reached zero — drop the head lever and `R` is a base primitive that subtraction can never empty, so the failure manifests at the terminal clause as "argument of type `Matcher<..., string, ...>` is not assignable to parameter expecting `Matcher<..., never, ...>`". The diagnostic names the tail; the defect is the head.
- `Match.tagsExhaustive(fields)` fuses the arm-subtraction and the terminal proof into one record: its `P extends { readonly [Tag in Types.Tags<'_tag', R> & string]: (...) => Ret } & { readonly [Tag in Exclude<keyof P, Types.Tags<'_tag', R>>]: never }` requires one handler per tag (exhaustiveness) and forbids any handler beyond the tags (exactness) on the single argument, so the whole proof is the handler record's type — no trailing `exhaustive` hop and no `R` slot to thread down arm by arm. The split-arm form and the fused-record form are the two spellings of the same `never`-proof, the record collapsing the pipeline to one position.

[ORDER_AS_A_PRESSURE_PIPE]:
- The non-commutativity of stacked levers is the data dependency between positions made syntactic, and it generalizes to a three-stage pipe: a NARROWING lever (`as const`, `<const T>`, rest capture) produces a narrow type, a CHECKING lever (`satisfies`) consumes that type to prove conformance, and a WITHDRAWING lever (`NoInfer`) consumes the inference graph to elect a source — so each stage reads what the prior stage wrote and inverting any adjacent pair feeds a stage the wrong input. Checking before narrowing proves conformance for a coarser type than the source carries; withdrawing before narrowing removes the position the narrowing lever needed as its source.
- The pipe identifies the only commutative pairs: levers acting on disjoint positions of one declaration reorder freely (an `as const` on one field and a `NoInfer` on another field's default share no input), while every pair touching one position is ordered narrow→check→withdraw. Commutativity is therefore a position-disjointness test, not a per-operator fact, and the lattice's row order is exactly this pipe order.
- `NoInfer<A>` sits at the pipe's far end by construction: it does not produce a type, it withdraws a position from candidacy so an upstream position is the sole inference source — placing it before the position that defines the type leaves nothing to infer from. The verified `NoInfer<A> = [A][A extends any ? 0 : never]` is a no-op on the type and a barrier on the inference graph, so it is ordering-only and always last among the levers touching one field.

```typescript
const grades = {
    bronze: { rate: 1, label: 'entry' },
    gold: { rate: 3, label: 'apex' },
} as const satisfies Record<string, { readonly rate: number; readonly label: 'entry' | 'apex' }>
const weight = <const G extends keyof typeof grades>(g: G): (typeof grades)[G]['rate'] => grades[g].rate
```

[CONST_PARAMETER_VS_HAS_PROBE]:
- The `<const T>` lever is needed only when a downstream type reads the VALUE of an option, not its key presence: `partialWith<const Options extends { readonly exact: true }>` carries `const Options` because the projection branches on the literal `true`, so widening `exact` to `boolean` would dissolve the branch — while `optionalWith`'s `Options extends OptionalOptions<...>` carries NO `const` because its faces resolve through `Types.Has<Options, 'exact'>`, a key-presence probe that reads `keyof Options` and never the value at that key. The lever's necessity is decided by whether the consuming conditional matches on the literal value or merely tests key membership.
- This refines the redundant-operator rule into a probe-shape rule: a const-modified options record is mandatory under a value-matching conditional (`Has<Options, 'k'> extends true ? ...` where the `true` is the option's own value) and noise under a presence-matching conditional (`'k' extends keyof Options ? ...`). Adding `const` to an options parameter whose faces only probe key presence is the precautionary-lever defect — it pins a literal no downstream type reads.

[VOCABULARY_ENTRY_POINTS]:
- A vocabulary's literals enter the contract through rest-tuple capture, never `<const T>` or `as const`: `Schema.Literal<Literals extends NonEmptyReadonlyArray<AST.LiteralValue>>(...literals)` infers the union `Literals[number]` because each spread position is inferred independently against the tuple element, and `Match.is`, `Match.tag`, `Schema.pickLiteral`, and `Schema.transformLiterals` all spell their vocabulary the same way. The rest position is the entry point whose pressure — array-literal collapse to element type — is defeated by capturing each element in its own tuple slot rather than freezing the whole array.
- Rest capture and `<const T>` compose without redundancy only because they pin different nesting depths, and `Schema.transformLiterals<const A extends AST.Members<readonly [from, to]>>(...pairs)` is the surface that proves it: the rest position pins the OUTER tuple — each `[from, to]` pair in its own argument slot — while `<const A>` pins the INNER pair, the two literals a single rest slot would still let widen because a pair is itself a two-element array. Stack the levers at the same depth and one is redundant; stack them at different depths and each defeats a pressure the other cannot reach. The depth a lever pins, not the lever's name, decides whether a second lever beside it composes or collides.

[COMPOSITION_LATTICE]:
- The lever-reach ledger and the collision ledger are one lattice keyed by the position each lever pins and the pressure that position faces; reading the lattice answers both "which lever does this declaration need" and "which lever here is redundant" from one axis. The positions, ordered by where they sit in the pressure pipe — value-source, parameter-binding, spread-argument, conformance-clause, inference-withdrawal — each admit exactly one narrowing lever, and stacking a second lever at an already-pinned position is the collision the lattice flags.

| [INDEX] | [POSITION]          | [PRESSURE]                          | [LEVER]            | [REDUNDANT WHEN]                                  |
| :-----: | :------------------ | :---------------------------------- | :----------------- | :------------------------------------------------ |
|   [1]   | value source        | primitive/array/object widening     | `as const`         | the consuming parameter is `<const T>`            |
|   [2]   | parameter binding   | inference widens to constraint      | `<const T>`        | the conditional probes key presence, not value    |
|   [3]   | spread argument     | array collapses to element type     | rest-tuple capture | the literals are already a derived sub-union       |
|   [4]   | inner-pair literal  | nested array widens past outer slot | `<const T>` (deep) | the pair carries no inner literals                |
|   [5]   | conformance clause  | `: T` annotation widens to `T`      | `satisfies T`      | the value crosses a runtime boundary (decode owns)|
|   [6]   | inference withdrawal| secondary value defines the shape   | `NoInfer<A>`       | placed on the shape-defining position             |

- The lattice subsumes the order rule: positions are listed in pipe order, so a declaration reads top-to-bottom and a lever at row N consumes the narrowness produced at rows below it — `as const` (row 1) before `satisfies` (row 5) before `NoInfer` (row 6), with rows 2-4 chosen by argument shape rather than sequenced. A multi-lever declaration is correct iff each lever's row matches a real pressure at its position and no two levers share a row; the redundancy column is the collision ledger and the position column is the reach ledger, fused.
- The derived-narrowness escape (row 3's redundant clause, the `Match.tag` case) is the lattice's deepest collapse: when a position's type is computed from an upstream-pinned union — `Tags<D, R>`, a `pickLiteral` sub-union, a `keyof` over an `as const` table — that position carries NO lever at all, because its narrowness is inherited, not produced. The presence of a lever at a derived position is always the redundant-operator defect, and its absence in the verified signature is the proof the narrowness was inherited.

[BOUNDARY_OF_THE_CONTRACT]:
- The per-lever erasure the ledger records is one fact, not six: because every lever is a position-pinning rule with no runtime artifact, the whole contract shares ONE reach edge — the source seam, the program point where a value is written in code — and stops there uniformly. The contract's narrowness is a statement ABOUT the literal as authored, so the single seam it cannot cross is the input seam where a value arrives from `unknown` and the authored literal says nothing about the dynamic shape; decode owns that seam, and no lever's narrowness — `satisfies`-asserted, `as const`-frozen, `<const T>`-inferred — survives a value that did not enter through the source.
- The narrowness is the inferred type's, never the value's, so the contract has a mutation blind spot the ledger's erasure note implies but does not name: a deep-`readonly` type sits over a structurally mutable object, and a `satisfies`-checked table re-bound through a widened alias loses both its `readonly` proof and its literals with no error at the mutation site. The contract is the authoring-time gate on the source seam alone; a vocabulary that must reject mutation or unknown input at runtime pairs the contract with a runtime owner past that seam, the two seams guarded by two mechanisms that never substitute for each other.
