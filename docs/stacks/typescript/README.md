# [STACKS_TYPESCRIPT]

Pages carry no outside-source blocks, release narration, provenance, process state, project anchors, tool context, source-footnote blocks, or meta commentary. Pages name exact code, package, operator, generated-surface, and command spellings in code spans; verification happens before authoring, and the page states doctrine as fact. 

All pages MUST be made in full compliance with `[5]-[PAGE_CRAFT]` section rules, markdown tables, if used must adhere to project standard creation, headers are load bearing and contribute to cell understanding to alleviate pressure; cells must be high signal, not explanatory prose.

All prose, structure, and style to follow standards as defined in:
- docs/standards/style-guide.md
- docs/standards/formatting.md
- docs/standards/information-structure.md

All docs within this folder are implicitly made for agent only reading and serve as source of truth; all content is declarative instructions stated as fact, never explanatory prose, no hedging, coupling, or references used outside of the README. All files are made to integrate the full corpus implicitly, all external libs/extensions are 1st class and integrated within files without qualifying, treat as stdlib.

## [1]-[ACTIVE_SURFACE]

[ACTIVE_SURFACE]:
- Target framework: `net10.0`
- Language version: `Typescript 7` native preview (`@typescript/native-preview`, `@effect/tsgo`)
- `verbatimModuleSyntax` — a type-only import or export omitting `import type`/`export type` errors.
- `erasableSyntaxOnly` — a value-bearing `namespace` or `enum` errors (`TS1294`); an owner fuses its derived statics onto the class body (`static readonly order = …` dotted `X.order`), never a companion value namespace.
- Export baseline: explicit end-of-file `[EXPORT]`; no wildcard imports, barrel files, or facade exports
- Format and lint: Biome (4-space, 150 width, single quotes, semicolons, trailing commas)

## [2]-[ATLAS]

This table is the lookup by reader decision.

| [INDEX] | [DECISION]             | [READ]                  | [STATE]   |
| :-----: | :--------------------- | :---------------------- | :-------- |
|   [1]   | owner and domain shape | shapes.md               | finalized |
|   [2]   | surface and dispatch   | surfaces-and-dispatch   | planned   |
|   [3]   | result and effect flow | rails-and-effects.md    | planned   |
|   [4]   | fiber and stream       | streams-and-concurrency | planned   |
|   [5]   | host and wire boundary | boundaries.md           | planned   |

## [3]-[DOCTRINE]

Twenty-one laws in five groups govern every TypeScript decision in this stack; concept pages instantiate them and no page restates them. Correctness is structural rather than disciplinary: each law converts a class of error into compile-time pressure the checker chases through every dispatch, or into a declaration that behavior is recoverable from. Density is the consequence, not the goal — derivation collapses artifact count multiplicatively rather than additively, so one primary declaration stands in for its whole secondary family of type, codec, equality, dispatch, and projection, and capability grows as union members, handler rows, and policy values inside existing owners, never as surfaces beside them. Advancement is measured as depth of code collapse — one owner deleting many consumer sites — never as depth of type-system arcana: a construct that demonstrates the type system without shrinking real code is surface sprawl wearing sophistication. The atlas is sized for large systems: total lines and public surface grow sublinearly with capability because every owner is declared with the capacity to absorb the family it anchors. Enforcement is doctrine-first: the checker posture, Biome, and the repository's own ast-grep rules encode these laws — the doctrine authors the tool, never the reverse. Findings against these laws are architecture pressure: fix the shape, not the diagnostic.

[FLOW]:
- `EXPRESSION_SPINE` — all domain logic is expression-shaped; dependent steps compose monadically through `Effect.gen` and `pipe`, independent computations compose applicatively through `Effect.all` and its error-accumulating `validate` mode, and `Match` closes conditional logic as the expression of record — dependence licenses sequence, independence licenses accumulation, and the carrier, never a flag, selects the algebra. Statements survive only inside measured kernels and platform-forced boundaries, and any page that shows one names the exemption.
- `BOUNDARY_ADMISSION` — raw material is admitted exactly once through `Schema.decodeUnknown` into an evidence-carrying owner; interior code never re-validates and never sees `unknown`, `any`, null-as-failure, or provider shapes. `Option` carries absence, the typed failure channel carries fallibility, and `ParseError` converts at the owning boundary. The producer of a typed value is selected by the value's provenance, never the consumer's preference: foreign matter crosses the transforming decode gate exactly once, an in-memory untrusted value of the type re-checks without transform, and interior known parts construct directly; the same selector governs interior dispatch, and a construction or re-validation reached for a value an earlier gate already admitted is a dead link that doubles work and can bypass the requirement set the decode would have demanded.
- `CHANNEL_TOTALITY` — `Effect<A, E, R>` is the complete contract: a closed `E` alphabet of tagged failures and an `R` requirement set provided exactly once at the composition root through one `Layer` graph. Exceptions, raw Promises, and ambient singletons are admitted at boundaries only; a signature that undersells its failure or requirement set is a defect, not a style choice.
- `STRUCTURED_CONCURRENCY` — every fiber is scoped: lifetimes attach to `Scope`, interruption is the only cancellation rail, and concurrency degree is declared as a policy value on the combinator, never improvised with unsupervised forks. A fiber that outlives its scope outside a named daemon root is a leak.

[SHAPE]:
- `SINGLE_SOURCE` — a concept has one source: a single rich owner fusing its runtime value with its static type, from which its variant union, codec, equality, keys, and every projection derive. The owner is shaped to absorb the whole family it anchors, so new cases, fields, and modalities land inside it rather than as new artifacts, and changing the owner reshapes every dependent site at compile time. A nested owner standing in for one of its parts, or a parallel owner for one concept, is the defect the collapse deletes.
- `SHAPE_BUDGET` — variants are cases inside the one owner `SINGLE_SOURCE` names — a `Schema.TaggedClass` union, a `Data.taggedEnum`, a behavior-column `as const` table — never sibling types beside it. Three or more parallel shapes, sibling factories, repeated dispatch arms, or single-call helpers is the collapse trigger, not a style preference.
- `DEEP_SURFACES` — prefer one rich polymorphic surface over many shallow ones; each module exposes one entrypoint family — one service, one schema family, one dispatch surface — and keeps internals unexported. The deep owner is the behavior-column table, the `Schema.Class`/`Model.Class`, or the `taggedEnum`, parameterized by its modifier set; one owner that holds a full concern beats four fragments that scatter it.
- `MODAL_ARITY` — one entrypoint owns all call modalities; singular, plural, batch, and stream discriminate on the shape of the input value — tag, arity, `Iterable`, `Stream` — never on name suffixes or boolean knobs, and through the discriminant the owner already declares (`_tag`, key, arity). A parallel parameter that re-describes the input is the knob smuggled back in; the discriminant must be recoverable from the value itself.
- `EVIDENCE_FUSION` — when the static type is needed apart from the value, it is extracted from the one owner, never restated: `typeof`, `keyof`, indexed access, and `Schema.Schema.Type`/`Schema.Schema.Encoded` lift it from the runtime declaration, and nominal identity, optionality, and defaults are inline field modifiers on the owner — never standalone interfaces, mirror types, or parallel brands.
- `DECLARATION_COLLAPSE` — a shape's first appearance is its only appearance: value, static type, brand, codec, equality, and keys fuse in one statement so the `object → type → const → typeof → instantiation` chain never forms. `as const`, `satisfies`, `const` type parameters, and the class-as-both-type-and-value owner (`class X extends Schema.Class<X>(...)`) carry the fusion; a follow-on `type X = typeof x`, a second `const` restating the value, or a separate instantiation after the owner is declared is the chain link the collapse deletes. Each lever's reach is bounded — `as const` carries no validation, `satisfies` no decode, a `const` type parameter no persistence past erasure — so a declaration stacks the levers whose reaches union to its need.
- `ANTICIPATORY_COLLAPSE` — an owner is shaped for the family it will absorb, not the instance in hand: the moment a second case, dimension, knob, or modality is conceivable, the shape generalizes so the next requirement lands as a union member the checker chases, a handler row, a policy value, or a layer swap with zero new surface. The proof of a correct shape is the diff of the next feature — one declaration inside the owner, every consumer untouched or broken loudly at compile time. That diff sorts into three classes, not two: within-family re-route and cross-family loud break both prove a correct shape, but a silent shift — runtime semantics moving while the static type holds — has no compile-time witness and is forbidden as an in-place migration; an axis with no loud break (identity regime, comparator coarseness, fold-law class, leaf-literal width) is sized pessimistically at declaration while a loud-breaking axis is sized to the instance, so the conservatism gradient is per-axis, never uniform. Sizing for the instance and widening later is the rejected order: widening after call sites exist multiplies surfaces, widening before they exist is one polymorphic dispatch.

[DERIVATION]:
- `POLICY_VALUES` — configuration and source enter as one domain value that carries its own behavior — a `Schedule`, a `Layer`, a `Config`, a literal-keyed handler row — never as flag sets whose combinations the implementation must re-derive. The concrete store, engine, or provider is selected by which `Layer` is provided at the composition root, never a branched pipeline; caps and thresholds ride the owner table as data rows derived by indexed access, never inlined at use sites. Behavior rows live with the vocabulary that selects them.
- `DERIVED_LOGIC` — when cases share generative structure, the logic is derived — `Match.exhaustive` over the family, a `satisfies`-closed table read by indexed access, a `Schema.transform` chain — never enumerated arms. One primary correspondence is declared and every secondary map derives from it; the derivation is the executable specification.
- `DERIVED_TYPES` — types are computed where the language allows so the one owner yields the family: conditional, mapped, and template-literal types, `Schema.Schema.Type`/`Schema.Schema.Encoded`/`Context.Tag.Service` extraction, and constrained generics replace rank-specific or per-provider copies. A conditional, mapped, or template type earns its place only by deriving a family from one owner and deleting the members it would otherwise spell by hand; a standalone predicate, transform, union-arithmetic utility, or `is-*` zoo is rejected regardless of correctness, density, or cleverness, and a derived type is never declared standalone until three consumers reference it.
- `SYMBOLIC_REFERENCE` — names, discriminants, and correspondences travel as literal-typed symbols the compiler checks — `_tag` members, `keyof typeof` chains, `satisfies`-anchored keys — never as string literals that restate something the program already knows.
- `SEMANTIC_NAMING` — every file, module, type, function, parameter, field, case, test, and region name uses the bounded context's canonical term, grammatical role, and tense. One semantic word is the default; two words are allowed when owner plus action, result, axis, or boundary is load-bearing; three words are the ceiling unless a generated contract, external API, or ambiguity proof requires more. Operations use action verbs, values and receipts use result nouns, policies and vocabularies use stable noun or adjective rows, and boundary adapters preserve foreign names only at the seam. Renaming for variety, tense drift, abbreviations, prefix/suffix families, and parallel labels for one concept are rejected.

[MATERIAL]:
- `LIBRARY_DEPTH` — admitted packages are the standard library, and `effect` Schema and Data are the generated owners: one `Schema.Class`, `Model.Class`, `Data.taggedEnum`, or `Schema.Tagged*` declaration the library expands into the codec, the static type, structural `Equal`/`Hash`, the validating constructor, and every persistence and wire projection — the source-generator economy other ecosystems buy with a tool. `effect` owns rails, time, entropy, schema, state, streams, and collections; `@effect/platform` owns host services; lane owners own their domains. A raw `as const` table is admitted only as the decode-free, dependency-free vocabulary owner, and even there it is a behavior-column dispatch surface read by `keyof typeof` and indexed access, never a launchpad for hand-rolled type derivation. Reach for the deepest combinator the package reaches for; wrappers, rename adapters, and platform-reflex spellings — `Date`, `Math.random`, `JSON.parse`, `setTimeout`, raw `fetch`, raw `try/catch` — are rejected. Native primitives remain owners only when they carry the invariant directly.
- `DEFINITION_TIME_ASPECTS` — cross-cutting capability attaches at definition time as composition values: `Schedule` policies, `Layer` wrapping, `HttpApiMiddleware`, schema annotations, `Effect.fn` instrumentation. Decorators are rejected; an aspect materializes policy and never hides control flow, and policy pushed across the admission seam stops being recoverable from its declaration.

[INTEGRATION]:
- `ROOT_REBUILD` — new capability is woven into the owning shape as if it had always existed; reshape the owner rather than appending beside it. No shims, compat aliases, `[Obsolete]` layers, or migration surfaces — break the API when the collapse improves the system.
- `ONE_HOP_RESOLUTION` — a name resolves to its semantics in one hop: no alias-to-constant-to-enum-to-class chains, no forwarding helpers, no helper or util shells, no convenience wrappers. A value that takes two jumps to trace marks a layer to delete.
- `COMPOSED_IMPLEMENTATION` — a feature of any complexity is implemented by composing the page owners — admitted shapes, typed rails, dispatch surfaces, boundary projections — before any new scaffold is named. The pages are one body, and the same concern resolves identically wherever it appears. A need with no composed spelling marks a missing case in an owning page's law: the law extends first, the feature lands second. Cards and snippets show this composition at full operator depth. Flat code — logic below the operator depth the admitted packages reach — is surface sprawl in time: it re-derives, line by line, what a deeper composed form states once.

## [4]-[COLLAPSE_SCAN]

Run this scan on every edit. Any signal triggers the move; three or more instances make it mandatory.

| [INDEX] | [SIGNAL]                                                   | [MOVE]                                             |
| :-----: | :--------------------------------------------------------- | :------------------------------------------------- |
|   [1]   | sibling `get`/`getMany`/`getById` names                    | one modality-polymorphic entrypoint on input shape |
|   [2]   | interface and schema declared in parallel for one concept  | one schema; the type is extracted, never restated  |
|   [3]   | try/catch inside domain flow                               | admission at the boundary, typed failure inward    |
|   [4]   | boolean parameter selects between two bodies               | one policy value — Schedule, Layer, or handler row |
|   [5]   | function calls exactly one other function                  | delete the hop                                     |
|   [6]   | repeated `_tag` switch or if-else chains                   | `Match.exhaustive` or a behavior-column table      |
|   [7]   | raw Promise or async/await in a domain signature           | the Effect carrier end-to-end                      |
|   [8]   | `useState`/`useReducer` holding domain state               | Atom over the owning Effect service                |
|   [9]   | wrapper renames a package API                              | use the package surface directly                   |
|  [10]   | hand-rolled retry, timeout, or polling loop                | a Schedule value                                   |
|  [11]   | `new Date()`, `Math.random()`, `setTimeout` in domain code | Clock, DateTime, Random through `R`                |
|  [12]   | `Effect.provide` scattered mid-pipeline                    | one Layer graph at the composition root            |
|  [13]   | standalone type predicate or union-arithmetic utility      | fold it into the owner whose family it derives     |
|  [14]   | brand with no runtime predicate or real nominal collision  | delete it; the primitive stands                    |

## [5]-[PAGE_CRAFT]

How pages in this folder are authored. The corpus is one body; these laws keep it coherent.

- Atlas law: this README owns doctrine and routing; each concept page owns one disjoint layer; a sibling concern is neither re-shown nor pointed to — cohesion comes from shared law, not linkage. The README is the only file that links.
- Grounding law: pages are agnostic in naming only — no project name, path, symbol, or host SDK — but grounded in the shape of real systems code: an owner carries the family a real axis carries (a bounded operation vocabulary, a fault family, a persisted entity's variant set), never a floating type-system demonstration. A snippet whose only subject is the type system, not a concept the owner models, is the rejected register.
- Zero meta: concept pages carry no provenance, source trace, release narration, process state, project, tool, or skill context.
- Page grammar: narrow index table, then family cards, then the snippet beside the rule it proves; the page ends at its last card section. Structure is identical across stack folders; content never is.
- Card fields are earned: `Use / Accept / Reject / Law / Boundary` lines appear on a card only where each one decides something; a field line that decides nothing is deleted, not filled.
- Snippet law: every snippet compiles under the active surface; identifiers are legal neutral names; placeholder strings such as `"<value-a>"` appear only inside literals; no project, host, or domain concept anchors a snippet.
- Snippet coverage: each snippet is doctrine-exemplary at full operator depth, roughly 3-4x denser than ordinary code, and shows consumer code shrinking because of the owner — the owner declared once, the loose spelling it deletes named in the reject, the next case landing as one row. Each snippet exercises a surface region no other snippet in the corpus shows; variety within the doctrine, zero duplicated demonstrations.
- Scale fidelity: a snippet shows the form at the shape it takes in a large system — the owner loaded with the modifiers, columns, getters, and filters that carry its whole concern in one fence with the growth axis visible — never an isolated minimum; a statement-bearing snippet sits beside the Exemption line naming its platform-forced seam.
- Code names before prose: every member a card or snippet names — combinator, option key, modifier, generated surface — is verified against the installed package implementation before it is written; an unconfirmable claim is not authored, and a nameable surface spelled as prose is a defect — the code span is the instruction.
- Card economy: cards are few, deep, and evidence-dense; near-peer cards merge until each retained card owns a decision cluster, and a card line carries exactly one decision. A thin card deciding one concept is a sibling line, not an owner.
- Snippet adherence: snippets obey the card they prove, compose finalized laws as supporting material, and reduce lines and surface through parameterization, behavior-column dispatch, and package depth, never by deleting capability.
- Altitude routing: when two pages touch one fact, the ledger records the split — mechanics at the owning page, consequence at the consumer; prose re-teaching an owned mechanic is repaired by routing to the owner, while composing owned surfaces inside a snippet is supporting material and owns nothing.
- Reject columns are load-bearing: every `Use` names the spelling, wrapper, or local pattern it deletes.
- Tables enumerate, cards legislate: rows stay atomic and narrow — no prose cramming, no links inside cells; nuance moves to a card.
- Route scope is README-local: lookup rows and conflict rules live in README files; concept pages carry only the law they own.
- Manifest truth: package versions, admitted packages, compiler posture, workspace graph, lock state, and tool settings live in `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `tsconfig*`, Biome config, and workspace tool configuration; no package-named pages; a package is named only where it changes the implementation choice.

## [6]-[CORPUS_LAW]

How the corpus accretes. The folder is one cohesive body, not isolated pages; atlas order is implementation order. The atlas `[STATE]` column is the law registry: a `finalized` page is binding law for every page authored after it; a `target` page exists only as roadmap scope. Finalization is a one-way gate — a context-free cold grade of the full page and every snippet, converging to a zero-edit pass, flips the state; the producer's grade admits, the cold grade decides.

- Three-layer inheritance: every page is authored under the doctrine, under every finalized page — adhered to, never restated, never referenced, never contradicted — and from its own source reservoir.
- Prose consumes earlier layers as given: vocabulary, owners, rails, and policy values arrive settled, never re-taught, and the page spends its lines only on its own layer.
- Reservoir residue: a page's source workspace deliberately holds more than the page prints, and it outlives the page — later enrichment and snippet work mine it without source-footnoting it; depth beyond one page's budget is corpus capital, never waste.
- Snippet stacking: code fully captures the card it proves, then composes on earlier layers' law at full doctrine depth — the new surface in the spotlight, established surfaces as supporting material, every touched concern built at the standard its owning finalized page legislates.
- Region ledger: the workspace root keeps one ledger of owned regions — page concerns at card altitude, snippet demonstrations at fence altitude. Snippet rows are written before code exists, page rows at finalization, and a duplicated region is repaired by routing to its owner, never by re-teaching — the ledger is what makes ownership checkable without re-reading the corpus.
- Purpose: the corpus is loaded as the operative standard in place of weaker context; zero-meta, zero-anchor, and stated-as-fact are absolute because any provenance, hedge, or stale claim poisons every downstream generation that loads the page.

## [7]-[SHAPES_CHARTER]

shapes.md owns the single-source owner — one rich declaration fusing a concept's runtime value, static type, variant union, codec, equality, keys, and every projection — together with the type-derivation toolkit (`typeof`, `keyof`, indexed, conditional, mapped, and template-literal types) that lifts the static family from it. Its spine is declaration-chain collapse, `DECLARATION_COLLAPSE` made concrete per owner family: a shape's first appearance is its only appearance, and every card proves how its owner deletes the `object → type → const → typeof → instantiation` chain. The bar is one rich owner over many weak ones — no parallel schema beside the generated owner, no loose model, no hand-rolled derivation restating what the owner already carries — and every snippet is doctrine-exemplary at full operator depth, names in its reject the loose spelling it deletes, and shows consumer code shrinking. The page defers the surface exposing an owner as an entrypoint to surfaces-and-dispatch.md, the `Effect<A, E, R>` failure and requirement channels to rails-and-effects.md, the `Schema.decodeUnknown`/`ParseError` admission act and byte translation to boundaries.md, and scope-bound concurrency to streams-and-concurrency.md.
