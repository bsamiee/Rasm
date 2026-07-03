# [TYPESCRIPT_DERIVATION]

This page is the derivation law: every type, vocabulary, and secondary surface computes from one anchor, and a hand-written parallel is a second, unverified source of truth. The type plane is the value plane's derived shadow — one declaration anchors each correspondence, `typeof`, `keyof typeof`, indexed access, mapped generation, template literals, and conditional decomposition compute everything downstream, and inference is solved once at the owner so no consumer ever restates what the anchor already carries. Everything around the algebra is shed by kind: the stated-annotation export gate and emit seam are `language.md`'s, collection and scalar selection is `values.md`'s, Schema owners and their derived surfaces are `shapes.md`'s, and overload sets, `Match` terminals, and `Function.dual` operators are `surfaces-and-dispatch.md`'s.

## [01]-[ANCHOR_ALGEBRA]

[ANCHOR_ALGEBRA]:
- Anchor law: one declaration anchors each correspondence — a value anchor where runtime rows, iteration, or order exist (the `as const` table, the `as const` key tuple), a type anchor where the correspondence is purely type-plane, a `const` type parameter where the caller supplies the shape, a state parameter where the fact is protocol position
- Direction law: the algebra runs three directions and closes — types derive from values, secondary types compute from primary types, and generated types govern values back through contract checks — so an anchor edit propagates as compile pressure around the whole loop; a hand parallel in any direction is the break that lets the loop drift silently
- Gate law: every export-reachable binding carries a stated annotation, and `as const satisfies` is not a stated form — an anchor that any export reaches through a `typeof` query, a namespace projection, or a public signature is plain `as const` with its contract homed as constrained-default guard aliases in the merged namespace; the expression-seam `satisfies` spelling survives only where no export reaches the anchor
- Placement law: a contract check rides the anchor declaration, an annotation states a public operation surface, and `as` is kernel material — each declaration fixes its form once, and a widening annotation on an anchor destroys every literal downstream
- Pre-solve law: inference is the owner's obligation — `const` type parameters, `NoInfer` check-only positions, and instantiation expressions fix literals at the declaration; a consumer writing a type argument, an `as const`, or a re-assertion marks the owner's signature as the defect
- Posture law: distribution and variance are declared decisions — a naked conditional parameter maps a union member-wise, `[T] extends [U]` asks one question of the whole, and `in`/`out` annotations make variance checked intent
- Fence law: `HKT.TypeLambda` quantifies over containers only and is proven by plural instances; a phantom state parameter carries compile-time protocol facts only and is witnessed or it enforces nothing

Treat the type level as computation over anchors, never as a place to author facts. Replace a hand union, a parallel constant, a mirrored interface, a re-asserted literal, or an assertion-repaired spread the moment an anchor can derive or govern it.

## [02]-[DERIVATION_CHOOSER]

Each table routes a correspondence to the form that owns it, and every `[USE]` names the spelling it deletes. Each row routes to the `[03]` contract owning its mechanism for the rule the row cannot state.

[ANCHOR_FORMS]: which declaration anchors a correspondence.

| [INDEX] | [CONCERN]                          | [USE]                                                    | [REPLACE]                                      |
| :-----: | :--------------------------------- | :------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | keyed rows any export reaches      | `as const` table + merged-namespace guard aliases        | hand union, annotation restating every row     |
|  [02]   | keyed rows no export reaches       | `as const satisfies Record<string, Row>` at the anchor   | guard ceremony where `satisfies` still rides   |
|  [03]   | key set for wire, order, iteration | `as const` key tuple; table guarded against its elements | keys restated at the literal, `as`-cast spread |
|  [04]   | governed value, literals inert     | stated `Record<GeneratedUnion, V>` annotation            | hand-synced parallel table that drifts         |
|  [05]   | pure type-plane correspondence     | `type` literal-union or template-pattern anchor          | a dead runtime table no value reads            |
|  [06]   | caller-supplied literal shape      | `const` type parameter at the owner                      | caller `as const` discipline                   |
|  [07]   | protocol call order                | accumulating state parameter, structurally witnessed     | runtime flag re-checked per call               |
|  [08]   | boundary shape                     | Schema owner                                             | interface plus parallel decoder pair           |

[PROJECTION_FORMS]: how a surface derives from its anchor.

| [INDEX] | [CONCERN]                    | [USE]                                                                                       | [REPLACE]                                      |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | discriminant union           | `keyof typeof Table`                                                                        | hand-written union                             |
|  [02]   | row and axis types           | indexed access `(typeof Table)[Kind]["axis"]`                                               | per-row aliases                                |
|  [03]   | tag union of a tagged family | `Family["_tag"]` — indexed access distributes over the union                                | hand-listed tag union                          |
|  [04]   | literal-keyed return         | indexed-access return on the generic key                                                    | conditional return needing a cast to implement |
|  [05]   | package-carrier channel      | `Effect.Effect.Success`, `Schema.Schema.Type`, `Context.Tag.Service`, `Layer.Layer.Success` | hand `infer` over package types                |
|  [06]   | local-carrier channels       | one bracketed multi-clause `infer` conditional                                              | chained single-`infer` conditionals            |
|  [07]   | union subset by pattern      | `Extract`/`Exclude` with a template pattern                                                 | re-listed members                              |
|  [08]   | derived-surface flatten      | `Types.Simplify` at the public alias                                                        | hand-rolled `{ [K in keyof T]: T[K] }` alias   |

[GENERATION_FORMS]: how a member family generates from anchors.

| [INDEX] | [CONCERN]                  | [USE]                                                 | [REPLACE]                     |
| :-----: | :------------------------- | :---------------------------------------------------- | :---------------------------- |
|  [01]   | renamed member family      | mapped type, `as` key remap, intrinsic case operators | sibling interfaces per member |
|  [02]   | capability cross product   | template literal over union anchors                   | enumerated string constants   |
|  [03]   | row-predicate filtering    | key remap to `never` on the row's column              | a parallel filtered table     |
|  [04]   | seam-recast view           | `-readonly`/`+?`/`-?` on the same mapped clause       | a second hand-written family  |
|  [05]   | generated-union governance | contract-checked value closing the loop               | hand-synced tables that drift |

[INFERENCE_FORMS]: how the owner pre-solves what consumers would otherwise repair.

| [INDEX] | [CONCERN]              | [USE]                                                         | [REPLACE]                             |
| :-----: | :--------------------- | :------------------------------------------------------------ | :------------------------------------ |
|  [01]   | literal retention      | `const` type parameter                                        | `as const` at every call site         |
|  [02]   | check-only position    | `NoInfer<T>`                                                  | mirror generic, argument-order repair |
|  [03]   | pre-solved generic     | instantiation expression under a semantic name                | wrapper arrow restating the signature |
|  [04]   | distribution posture   | naked parameter to map; `[T] extends [U]` for one answer      | accidental union distribution         |
|  [05]   | variance intent        | `in`/`out` on public generic carriers                         | inference drift, phantom bivariance   |
|  [06]   | container polymorphism | `HKT.TypeLambda` plus `HKT.Kind` under a typeclass constraint | per-container operator copies         |

## [03]-[DERIVATION_CONTRACTS]

Each contract fixes the placement rule its chooser rows cannot state. Snippets compose settled surfaces as supporting material; the spotlight is the derivation mechanism itself, and each contract closes on the boundary that hands the derived surface to its owning page.

[VOCABULARY_TABLE_SITE]:
- Use when: a bounded keyed domain carries behavior rows and its secondary surfaces — discriminant, projections, wire literal, returns — must stay provably consistent with it.
- Accept: the `as const` table merged with a `declare namespace` type hub under one exported name, so the discriminant, row, axis, tuple, and contract types all qualify off the single import; the contract as a constrained-default guard pair inside the hub — a row guard whose default `typeof` query proves completeness and row shape, a key guard whose default discriminant proves no excess row — drift failing loudly at the alias declaration with zero widening of the anchor; the key tuple joining the owner where order, iteration, or a non-empty spread is load-bearing; `keyof typeof` discriminants; indexed row and axis projections; a conditional return derived as indexed access over the table keyed by the generic literal parameter; the tuple spread into `Schema.Literal`.
- Reject: a hand union or parallel constant beside the table; an annotation restating every row to buy an export — the guard pair carries the contract and the anchor keeps its literals; a row-tuple anchor (`[{ kind: ... }] as const`) self-carrying its keys — lookup degrades to a linear scan and the discriminant re-derives as `(typeof rows)[number]["kind"]`, so the record owns lookup and the tuple owns order; `Object.keys` enumeration widened to `Array<string>` and asserted back when the tuple already carries the ordered key set; an `extends ?` conditional return — the checker cannot prove its body, and the cast it demands marks dispatch that belongs to the overload owner; table keys restated inside a wire literal.
- Law: the guard pair is the satisfies algebra relocated to the declaration seam — a constrained default validates without widening exactly as `satisfies` does, completeness and shape ride the row guard, excess rides the key guard, and both erase; an anchor no export reaches keeps the shorter `as const satisfies` spelling at the expression seam.
- Law: the tuple spread holds `Schema.Literal`'s non-empty overload and preserves the exact literal tuple; spreading derived keys lands in the widened `ReadonlyArray` overload and demotes the schema to `SchemaClass<Kind>` — non-emptiness and order are tuple facts stated once at the anchor, never assertion repairs at the seam.
- Boundary: the stated-annotation export gate is `language.md`'s; the Schema owner that admits the wire value, and the class families vocabularies embed into, are `shapes.md`'s; dispatch composed over row lookups is `surfaces-and-dispatch.md`'s.

```ts conceptual
import { Schema } from "effect"

const tiers = ["narrow", "level", "broad"] as const          // key anchor: order, iteration, and non-emptiness are tuple facts stated once

const Tier = {                                               // plain as const: the anchor keeps every literal; its contract lives in the guards below
  narrow: { ceiling: 2, weight: 5, wire: 429 },
  level: { ceiling: 24, weight: 3, wire: 425 },
  broad: { ceiling: 96, weight: 1, wire: 503 },
} as const

declare namespace Tier {                                     // merged type hub: one exported name carries the table and every derived type
  type Kinds = typeof tiers
  type Kind = keyof typeof Tier                              // discriminant derives; a hand union is a second, unverified source of truth
  type Row = (typeof Tier)[Kind]
  type Ceiling<K extends Kind = Kind> = (typeof Tier)[K]["ceiling"]
  type Contract = Record<Kinds[number], { readonly ceiling: number; readonly weight: number; readonly wire: number }>
  type _Rows<T extends Contract = typeof Tier> = T           // row guard: a missing key or wrong axis shape fails this default at declaration
  type _Keys<K extends keyof Contract = Kind> = K            // key guard: an excess table row fails here — closure in both directions, zero widening
}

const TierWire: Schema.Literal<Tier.Kinds> = Schema.Literal(...tiers) // the tuple spread holds the non-empty overload; derived keys would demote to SchemaClass<Kind>

const ceiling = <K extends Tier.Kind>(kind: K): Tier.Ceiling<K> => Tier[kind].ceiling // conditional return by indexed access: the checker proves the body

const _cap: 2 = ceiling("narrow")                            // checked witness: the row's literal projects, never the axis union

// --- [EXPORTS] ---------------------------------------------------------------------

export { ceiling, Tier, tiers, TierWire }                    // the tuple a public signature speaks goes public under its own name, never leaked as a _-type
```

[GENERATED_SURFACE_SITE]:
- Use when: a member family corresponds to an anchor generatively — renamed handler names, capability matrices, filtered projections — so the family's size tracks the anchor's, never a hand count.
- Accept: mapped types with `as` key remapping and the intrinsic case operators `Capitalize`, `Uncapitalize`, `Uppercase`, `Lowercase`; template-literal cross products multiplying union anchors; row-predicate filtering by remapping the excluded key to `never`; modifier algebra on the same mapped clause — `-readonly` strips and `+?`/`-?` recast presence, so a type-seam view derives with zero second family; `Extract` and `Exclude` over template patterns for subsets; the governed value closing the loop under a stated `Record<GeneratedUnion, V>` annotation — the mapped domain demands completeness, the fresh-literal check rejects excess, the binding exports, and a new anchor row breaks it loudly at compile time.
- Reject: sibling interfaces restating a family member by member; enumerated string constants a template cross product derives; a parallel filtered table beside its anchor; a generated family over members that merely rhyme — generation encodes a real correspondence, never a coincidence of spelling.
- Law: the mapped parameter correlates name and payload — `(typeof Anchor)[K]` keeps each generated member row-precise where a hand-written family blurs every payload to the union.
- Law: a governed value whose row literals are inert rides the stated annotation and exports; a governed value whose literals feed derived types is itself an anchor and takes the vocabulary owner's guard form.
- Law: generation is type-plane — a key the runtime must compute crosses at a value, because runtime case conversion widens to `string` and cannot index a generated record; a family the runtime addresses by key anchors as table rows instead.
- Boundary: domain shape variants, partial views, and wire projections derive on the Schema owner in `shapes.md`; these forms own type-seam families only.

```ts conceptual
import type { Effect } from "effect"

type Verb = "emit" | "drain"                                 // type anchor: only the type plane reads it, so a value tuple here would be a dead runtime table

const Signal = {
  open: { grade: "state", retain: true },
  frame: { grade: "burst", retain: false },
  close: { grade: "state", retain: true },
} as const

declare namespace Signal {
  type Kind = keyof typeof Signal
  type Row = { readonly grade: "state" | "burst"; readonly retain: boolean }
  type _Rows<T extends Record<Kind, Row> = typeof Signal> = T // self-keyed row guard: the open table proves shape against its own key set
  type Wake = { readonly [K in Kind as `on${Capitalize<K>}`]: (row: (typeof Signal)[K]) => Effect.Effect<void> } // remapped family: K correlates each generated name to its row-precise payload
  type Staged = { -readonly [K in keyof Wake]+?: Wake[K] }   // modifier algebra: one clause recasts the family as a mutable partial staging view
  type Retained = { readonly [K in Kind as (typeof Signal)[K]["retain"] extends true ? K : never]: (typeof Signal)[K] } // filtering is remap-to-never on the row's own column
  type Grant = `${Kind}:${Verb}`                             // cross product: |Kind| x |Verb| literals from two anchors, zero listed
  type Drain = Extract<Grant, `${string}:drain`>             // subset by template pattern, never a re-listed union
}

const quota: Record<Signal.Grant, number> = {                // governed value: the stated annotation demands all six grants, rejects any excess, and exports
  "open:emit": 8,
  "open:drain": 2,
  "frame:emit": 64,
  "frame:drain": 16,
  "close:emit": 8,
  "close:drain": 2,
}

const _drain: Signal.Drain = "frame:drain"                   // checked witness over the generated subset

// --- [EXPORTS] ---------------------------------------------------------------------

export { quota, Signal }
export type { Verb }
```

[INFERENCE_PRESOLVE_SITE]:
- Use when: an owner accepts literal-bearing arguments whose precision downstream types consume — plans, routes, step tuples, keyed selections.
- Accept: a `const` type parameter so caller literals and tuples arrive narrow with zero call-site ceremony; `NoInfer` on every position that checks against another position's inference instead of driving it; an instantiation expression pre-solving a package generic once at the owner under a semantic name — on an exported binding the applied generic rides the annotation as `typeof` over the instantiation, never a restated signature; derived return fields projecting the inferred literal shape.
- Reject: caller `as const` discipline — unenforceable and lost at the first refactor; a mirror type parameter added to repair inference order; a wrapper arrow restating a whole signature to pin one type argument — the instantiation expression pins it with no body; a call site writing a type argument the owner's signature should have fixed; an exported call-result binding — its type exists only by inference, so it fails the stated-annotation gate, and a proof or staging binding stays interior instead.
- Law: one position drives inference and every other position checks — `NoInfer` moves the error to the wrong argument instead of silently widening the inference the right argument produced.
- Law: pre-solving is not renaming — the instantiation expression changes the type surface by monomorphizing the generic to the domain shape, and the binding carries the domain role's semantic name; a binding that changes neither type nor name is the deleted hop.
- Boundary: data-first/data-last pairing on a pre-solved operator is `surfaces-and-dispatch.md`'s.

```ts conceptual
import { Array } from "effect"

type Lane = "bulk" | "live"
type Step = { readonly lane: Lane; readonly take: number }

const plan = <const Steps extends ReadonlyArray<Step>>(     // const parameter: caller literals arrive narrow with zero as const at any call site
  steps: Steps,
  fuse: NoInfer<Steps[number]["lane"]>,                     // check-only position: steps alone drives inference; fuse validates against the plan's own lanes
): { readonly steps: Steps; readonly fuse: Steps[number]["lane"] } => ({ steps, fuse })

const nextStep: typeof Array.head<Step> = Array.head        // instantiation expression rides the annotation: the package generic pre-solved once under a semantic name

const _run = plan([{ lane: "live", take: 2 }, { lane: "bulk", take: 5 }], "live")

const _fuse: "bulk" | "live" = _run.fuse                    // the owner solved inference; consumers never re-instantiate or re-assert

// @ts-expect-error "bulk" is a lane, but not one of this plan's lanes
const _drift = plan([{ lane: "live", take: 2 }], "bulk")

// --- [EXPORTS] ---------------------------------------------------------------------

export { nextStep, plan }
export type { Lane, Step }
```

[CONDITIONAL_DECOMPOSITION_SITE]:
- Use when: a type computes from another type's structure — channel extraction, literal-pattern parsing, whole-shape questions over unions.
- Accept: shipped extractors first — `Effect.Effect.Success`, `Effect.Effect.Error`, `Effect.Effect.Context`, `Schema.Schema.Type`, `Schema.Schema.Encoded`, `Context.Tag.Service`, `Layer.Layer.Success` — each itself a bracketed multi-clause conditional, so a hand `infer` over a package carrier re-derives what the package already exports; one bracketed multi-clause conditional for local carriers, every channel extracted in a single match with `infer ... extends` constraints inline; naked-parameter distribution only as the deliberate member-wise map; explicit `in`/`out` on public generic carriers; tail-position recursion with an accumulator parameter for bounded literal-pattern decomposition.
- Reject: chained single-`infer` conditionals where one clause extracts every channel; a naked conditional that must answer once for the whole — `boolean` splits to `true | false` and `never` distributes to nothing, so an undeclared posture is a latent bug, not a style choice; variance left to inference on a public carrier when the annotation makes intent compiler-checked; recursion that wraps its own result — pending work stacks toward the depth gate that the tail form elides.
- Law: `infer M extends Realm` narrows inside the clause — the constraint participates in the match itself, and a failed constraint selects the false branch; a post-filter conditional after an unconstrained `infer` is the rejected two-step spelling.
- Law: recursion is budgeted — the accumulator carries every partial so each step's result is final and tail-recursion elimination applies; type-level parsing or arithmetic over domain data re-derives what the value level owns, and the repair is one value computation whose result the type level derives from the anchor.
- Boundary: exhaustive dispatch over an extracted union is `surfaces-and-dispatch.md`'s.

```ts conceptual
import type { Effect } from "effect"

type Realm = "doc" | "layer"                                 // type anchor: no runtime row exists, so the union itself anchors the correspondence
type Verb = "pull" | "push"

type Feed<in Cmd, out Row> = {                               // declared variance is checked intent: a wrong in/out is a compile error
  readonly push: (cmd: Cmd) => Effect.Effect<void, RangeError>
  readonly tail: Effect.Effect<Row>
}

type Parse<R> = R extends `${infer M extends Realm}/${infer V extends Verb}` // multi-clause infer: every channel extracts in one conditional, constraints inside the match
  ? { readonly realm: M; readonly verb: V }
  : never

type Split = Parse<"doc/pull" | "layer/push">                // naked parameter distributes: a union maps per member, two records out

type Channel = Split["verb"]                                 // indexed access distributes over the union: every member's column collects into "pull" | "push"

type Facet<H> = [H] extends [(cmd: infer C) => Effect.Effect<infer _A, infer E>] // bracketed posture: one answer over the whole shape, never a per-member split
  ? { readonly cmd: C; readonly fault: E }
  : never

type Probe = Facet<Feed<`${Realm}/${Verb}`, number>["push"]> // { cmd: "doc/pull" | "doc/push" | "layer/pull" | "layer/push"; fault: RangeError }

type Trail<S extends string, Acc extends ReadonlyArray<string> = []> = S extends `${infer Head}/${infer Rest}` // accumulator in tail position: each step is final, so elimination budgets the recursion
  ? Trail<Rest, [...Acc, Head]>
  : readonly [...Acc, S]

const _deep: Trail<"doc/pull/live/burst"> = ["doc", "pull", "live", "burst"] // checked witness over the decomposed tuple

// --- [EXPORTS] ---------------------------------------------------------------------

export type { Channel, Facet, Feed, Parse, Probe, Realm, Split, Trail, Verb }
```

[CONTAINER_KIND_SITE]:
- Use when: one combinator must serve many containers — the same enrichment over `Option`, `ReadonlyArray`, and an own container, written once.
- Accept: an own `HKT.TypeLambda` implementor per own container, whose single `readonly type` line states how the container consumes `this["Target"]` — interface-declared by necessity, since a `this` type resolves only there, and the sole interface export the derivation layer sanctions; combinator signatures over `HKT.Kind<F, In, Out2, Out1, Target>` constrained by the `@effect/typeclass` vocabulary — `Covariant` and its siblings — with `In`, `Out2`, and `Out1` threading ambient channels and `Target` as the mapped slot; shipped instances from `@effect/typeclass/data/Option` and `@effect/typeclass/data/Array`; one own instance per own container, its dual `map` paired with the `covariant.imap` derivation.
- Reject: an HKT combinator with one instance — an ordinary generic wearing a lambda; domain variation carried on `F` — vocabulary rows and tagged families own domain axes, the lambda owns containers only; a local functor-shaped interface shadowing the typeclass vocabulary; container abstraction no call site swaps.
- Law: the fence is plural instantiation — the lambda earns existence when combinator call sites apply distinct instances; when every call site names one container, the combinator collapses into that container's module and the lambda dies.
- Boundary: which container a domain value rides, and the instance merge algebra over `struct` and `tuple` composition, are `values.md`'s.

```ts conceptual
import * as covariant from "@effect/typeclass/Covariant"
import * as ArrayInstances from "@effect/typeclass/data/Array"
import * as OptionInstances from "@effect/typeclass/data/Option"
import { Function, type HKT, Option } from "effect"

type Ranked<out A> = {                                       // an own container earns an own lambda; the variance annotation travels with it
  readonly rank: number
  readonly value: A
}

interface RankedTypeLambda extends HKT.TypeLambda {
  readonly type: Ranked<this["Target"]>                      // the one line a lambda owns: how the container consumes its Target
}

const _map = Function.dual<
  <A, B>(f: (a: A) => B) => (self: Ranked<A>) => Ranked<B>,
  <A, B>(self: Ranked<A>, f: (a: A) => B) => Ranked<B>
>(2, (self, f) => ({ rank: self.rank, value: f(self.value) }))

const RankedCovariant: covariant.Covariant<RankedTypeLambda> = { map: _map, imap: covariant.imap<RankedTypeLambda>(_map) }

const brace = <F extends HKT.TypeLambda>(F: covariant.Covariant<F>): (<R, O, E>(self: HKT.Kind<F, R, O, E, number>) => HKT.Kind<F, R, O, E, { readonly value: number; readonly even: boolean }>) =>
  (self) => F.map(self, (value) => ({ value, even: value % 2 === 0 })) // one combinator, every container: the instance is the only per-container cost

const _braced = brace(OptionInstances.Covariant)(Option.some(3))       // Option.Option<{ value: number; even: boolean }>
const _rows = brace(ArrayInstances.Covariant)([1, 2, 3])               // ReadonlyArray<{ value: number; even: boolean }>
const _ranked = brace(RankedCovariant)({ rank: 1, value: 3 })          // Ranked<{ value: number; even: boolean }>

// --- [EXPORTS] ---------------------------------------------------------------------

export { brace, RankedCovariant }
export type { Ranked, RankedTypeLambda }
```

[TYPESTATE_SITE]:
- Use when: the legality of a call depends on protocol position — a terminal operation forbidden until every slot is set, a transition forbidden after it fires — and the runtime holds no state that could check it.
- Accept: one state parameter accumulating filled keys through `Exclude`-constrained transitions, so a refill fails at the key constraint; the terminal operation demanding completion structurally — its parameter typed at the full record, never a conditional return; `Record.singleton` composing the accumulated record — a generic computed key in an object literal widens to a string index, so the package construction carries what the checker cannot type; `@ts-expect-error` proof tokens pinning each forbidden call as rejected.
- Reject: a phantom parameter carrying ordinary domain data — a value the program reads at runtime is a field on its owner, never a type argument; an unreferenced state parameter — an unused type parameter does not participate in assignability, so every instantiation unifies and the protocol it claims is unenforced; a runtime flag re-checking what the state parameter proves; a conditional-return terminal whose implementation needs the cast.
- Law: the witness law — a protocol parameter is witnessed structurally in a real position or through a `Types.Invariant` variance slot; unwitnessed state erases from comparison and enforces nothing.
- Law: the subtraction law — each transition subtracts its key from the open set and intersects it into the filled record, so the protocol is order-free, refills are compile errors, and completion is exactly the spent key set.
- Boundary: a builder whose product is a domain shape terminates at the Schema owner in `shapes.md`; runtime acquire/release ordering is `rails-and-effects.md` material.

```ts conceptual
import { Record } from "effect"

type Slots = { readonly route: string; readonly limit: number; readonly lane: "bulk" | "live" }

type Draft<Filled extends Partial<Slots>> = {                // Filled is witnessed structurally by state; an unreferenced parameter would enforce nothing
  readonly state: Filled
  readonly slot: <K extends Exclude<keyof Slots, keyof Filled>>(key: K, value: Slots[K]) => Draft<Filled & Record.ReadonlyRecord<K, Slots[K]>>
}

const draft = <Filled extends Partial<Slots>>(state: Filled): Draft<Filled> => ({
  state,
  slot: (key, value) => draft({ ...state, ...Record.singleton(key, value) }), // the package construction carries the generic single-key record the checker cannot type
})

const seal = (complete: Draft<Slots>): Slots => complete.state // completion is demanded by the parameter type, never by a conditional return needing a cast

const staged = draft({}).slot("route", "<value-a>").slot("limit", 8)

// @ts-expect-error the protocol forbids refilling a slot
const twice = staged.slot("route", "<value-b>")

// @ts-expect-error seal demands every slot; "lane" is missing
const early = seal(staged)

const sealed: Slots = seal(staged.slot("lane", "live"))      // the order-free protocol closes only when the key set is spent

// --- [EXPORTS] ---------------------------------------------------------------------

export { draft, seal }
export type { Draft, Slots }
```

## [04]-[ABSTRACTION_COLLAPSE_TESTS]

Use these tests before keeping a form the derivation layer already owns.

[PARALLEL_RESTATEMENT]:
- Smell: a hand union tracks a table's keys, an interface mirrors a derivable type, a second constant restates rows another declaration carries, an exported anchor's annotation restates every row, or a value the type plane could govern drifts unchecked beside its generated union.
- Collapse: derive — `keyof typeof`, indexed access, mapped generation, template cross product, or the shipped extractor — home the contract as the namespace guard pair, and govern the value under the generated union's stated annotation.
- Done when: every secondary surface computes from its anchor, every governed value breaks on anchor growth, and deleting the parallel changes no meaning anywhere.

[CALL_SITE_RESIDUE]:
- Smell: consumers write `as const`, type arguments, or literal re-assertions, re-derive a union the owner already carries, aim `Parameters`/`ReturnType` at an owner's export, or a call-result binding sits in the exports block.
- Collapse: `const` type parameters, `NoInfer` check-only positions, and instantiation expressions at the owner; the owner's merged namespace carries the type a consumer would re-derive; proofs and staging bindings stay interior.
- Done when: call sites carry values only, and the owner's signature is the only place inference is described.

[TYPE_PLANE_OVERREACH]:
- Smell: type-level parsing or arithmetic re-derives a value fact, a recursive conditional wraps its own result toward the depth gate, or a conditional return needs a cast to implement.
- Collapse: compute at the value level and derive the type from the anchor; rewrite the recursion with a tail-position accumulator; move provable returns to indexed access and dispatch to the overload owner.
- Done when: every conditional body is checker-proven, and no directive or cast spans a derivation.

[UNWITNESSED_ABSTRACTION]:
- Smell: an HKT combinator every call site applies to one container, or a state parameter that is unused or carries domain data.
- Collapse: the ordinary generic or the container's own module; a structural or `Types.Invariant` witness, or a field on the owning shape.
- Done when: every lambda has plural instances at real call sites, and every state parameter is witnessed and protocol-only.
