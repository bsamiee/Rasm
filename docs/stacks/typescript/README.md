# [STACKS_TYPESCRIPT]

This folder is the TypeScript stack router. It routes language, shape, surface, rail, stream, boundary, system-API, domain, and quality-rail decisions to the concept page that owns the coding choice. The router builds one admission-first paradigm: raw input is decoded once into evidence-carrying owners, admitted values travel the `Effect<A, E, R>` channel, behavior lives on generated dispatch surfaces, and projections leave at explicit boundaries where the requirement set is provided exactly once.

Pages carry no outside-source blocks, release narration, provenance, process state, project anchors, tool context, source-footnote blocks, or meta commentary. Pages name exact code, package, operator, generated-surface, and command spellings in code spans; verification happens before authoring, and the page states doctrine as fact.

## [1]-[ATLAS]

This table is the lookup by reader decision.

| [INDEX] | [DECISION]             | [READ]                                                | [STATE] |
| :-----: | :--------------------- | :---------------------------------------------------- | :------ |
|   [1]   | language shape         | [language](language.md)                               | planned |
|   [2]   | domain shape           | [shapes](shapes.md)                                   | planned |
|   [3]   | surface and dispatch   | [surfaces and dispatch](surfaces-and-dispatch.md)     | planned |
|   [4]   | result and effect flow | [rails and effects](rails-and-effects.md)             | planned |
|   [5]   | fiber and stream       | [streams and concurrency](streams-and-concurrency.md) | planned |
|   [6]   | host and wire boundary | [boundaries](boundaries.md)                           | planned |
|   [7]   | system API replacement | [system APIs](system-apis.md)                         | planned |
|   [8]   | quality rail           | [testing](testing/README.md)                          | planned |
|   [9]   | domain routing         | [domain](domain/README.md)                            | planned |
|  [10]   | host runtime           | [runtime](domain/runtime.md)                          | planned |
|  [11]   | wire transport         | [transport](domain/transport.md)                      | planned |
|  [12]   | data persistence       | [persistence](domain/persistence.md)                  | planned |
|  [13]   | distributed execution  | [cluster](domain/cluster.md)                          | planned |
|  [14]   | telemetry signal       | [diagnostics](domain/diagnostics.md)                  | planned |
|  [15]   | command line           | [cli](domain/cli.md)                                  | planned |
|  [16]   | model inference        | [ai](domain/ai.md)                                    | planned |
|  [17]   | infrastructure program | [infra](domain/infra.md)                              | planned |
|  [18]   | retained interaction   | [interaction](domain/interaction.md)                  | planned |
|  [19]   | design system          | [visuals](domain/visuals.md)                          | planned |
|  [20]   | data interchange       | [data interchange](domain/data-interchange.md)        | planned |
|  [21]   | local durability       | [durability](domain/durability.md)                    | planned |
|  [22]   | identity and access    | [identity](domain/identity.md)                        | planned |
|  [23]   | service integration    | [integrations](domain/integrations.md)                | planned |

## [2]-[DOCTRINE]

Nineteen laws in five groups govern every TypeScript decision in this stack; concept pages instantiate them and no page restates them. Correctness is structural rather than disciplinary: each law converts a class of error into compile-time pressure `tsc` chases through every dispatch, or into a declaration that behavior is recoverable from. Density is the consequence, not the goal — derivation collapses artifact count multiplicatively rather than additively, so one primary declaration stands in for its whole secondary family of type, codec, equality, arbitrary, and JSON Schema, and capability grows as union members, handler rows, and policy values inside existing owners, never as surfaces beside them. The atlas is sized for large systems: total lines and public surface grow sublinearly with capability because every owner is declared with the capacity to absorb the family it anchors. Enforcement is doctrine-first: the `tsc` posture flags, Biome, and the repository's own ast-grep rules encode these laws — the doctrine authors the tool, never the reverse. Findings against these laws are architecture pressure: fix the shape, not the diagnostic.

[FLOW]:
- `EXPRESSION_SPINE` — all domain logic is expression-shaped; dependent steps compose monadically through `Effect.gen` and `pipe`, independent computations compose applicatively through `Effect.all` and its error-accumulating `validate` mode, and `Match` closes conditional logic as the expression of record — dependence licenses sequence, independence licenses accumulation, and the carrier, never a flag, selects the algebra. Statements survive only inside measured kernels and platform-forced boundaries, and any page that shows one names the exemption.
- `BOUNDARY_ADMISSION` — raw material is admitted exactly once through `Schema.decodeUnknown` into an evidence-carrying owner; interior code never re-validates and never sees `unknown`, `any`, null-as-failure, or provider shapes. `Option` carries absence, the typed failure channel carries fallibility, and `ParseError` converts at the owning boundary.
- `CHANNEL_TOTALITY` — `Effect<A, E, R>` is the complete contract: a closed `E` alphabet of tagged failures and an `R` requirement set provided exactly once at the composition root through one `Layer` graph. Exceptions, raw Promises, and ambient singletons are admitted at boundaries only; a signature that undersells its failure or requirement set is a defect, not a style choice.
- `STRUCTURED_CONCURRENCY` — every fiber is scoped: lifetimes attach to `Scope`, interruption is the only cancellation rail, and concurrency degree is declared as a policy value on the combinator, never improvised with unsupervised forks. A fiber that outlives its scope outside a named daemon root is a leak.

[SHAPE]:
- `SHAPE_BUDGET` — a concept owns exactly one declaration; variants are members of one closed family — a `Schema.TaggedClass` union, a `Data.TaggedEnum`, a `Schema.Literal` vocabulary — never sibling types. Three or more parallel shapes, sibling factories, repeated dispatch arms, or single-call helpers is the collapse trigger, not a style preference.
- `DEEP_SURFACES` — prefer one rich polymorphic surface over many shallow ones; each module exposes one entrypoint family — one service, one schema family, one dispatch surface — and keeps internals unexported. One deep owner that holds a full concern beats four fragments that scatter it.
- `MODAL_ARITY` — one entrypoint owns all call modalities; singular, plural, batch, and stream discriminate on the shape of the input value — tag, arity, `Iterable`, `Stream` — never on name suffixes or boolean knobs. A parallel parameter that re-describes the input is the knob smuggled back in; the discriminant must be recoverable from the value itself.
- `EVIDENCE_FUSION` — one declaration owns static type and runtime evidence: the schema is primary, and every secondary artifact — type, guard, codec, equivalence, arbitrary, JSON Schema — derives from it. Types are extracted through `Schema.Schema.Type` and `typeof`, never restated; nominal identity is declared through brands and tags, never simulated with parallel interfaces.
- `ANTICIPATORY_COLLAPSE` — an owner is shaped for the family it will absorb, not the instance in hand: the moment a second case, dimension, knob, or modality is conceivable, the shape generalizes so the next requirement lands as a union member `tsc` chases, a handler row, a policy value, or a layer swap with zero new surface. The proof of a correct shape is the diff of the next feature — one declaration inside the owner, every consumer untouched or broken loudly at compile time. Sizing for the instance and widening later is the rejected order: widening after call sites exist multiplies surfaces, widening before they exist is one polymorphic dispatch.

[DERIVATION]:
- `POLICY_VALUES` — configuration enters as one domain value that carries its own behavior — a `Schedule`, a `Layer`, a `Config`, a literal-keyed handler row — never as flag sets whose combinations the implementation must re-derive. Behavior rows live with the vocabulary that selects them.
- `DERIVED_LOGIC` — when cases share generative structure, the logic is derived — `Match.exhaustive` over the family, a `satisfies`-closed table, a `Schema.transform` chain — never enumerated arms. One primary correspondence is declared and every secondary map derives from it; the derivation is the executable specification.
- `DERIVED_TYPES` — types are computed where the language allows so one declaration yields the family: conditional, mapped, and template-literal types, `Schema.Schema.Type`/`Schema.Schema.Encoded`/`Context.Tag.Service` extraction, and constrained generics replace rank-specific or per-provider copies.
- `SYMBOLIC_REFERENCE` — names, discriminants, and correspondences travel as literal-typed symbols the compiler checks — `_tag` members, `keyof typeof` chains, `satisfies`-anchored keys — never as string literals that restate something the program already knows.
- `SEMANTIC_NAMING` — every file, module, type, function, parameter, field, case, test, and region name uses the bounded context's canonical term, grammatical role, and tense. One semantic word is the default; two words are allowed when owner plus action, result, axis, or boundary is load-bearing; three words are the ceiling unless a generated contract, external API, or ambiguity proof requires more. Operations use action verbs, values and receipts use result nouns, policies and vocabularies use stable noun or adjective rows, and boundary adapters preserve foreign names only at the seam. Renaming for variety, tense drift, abbreviations, prefix/suffix families, and parallel labels for one concept are rejected.

[MATERIAL]:
- `LIBRARY_DEPTH` — admitted packages are the standard library: `effect` owns rails, time, entropy, schema, state, streams, and collections; `@effect/platform` owns host services; lane owners own their domains. Use the deepest combinator the package itself reaches for; wrappers, rename adapters, and platform-reflex spellings — `Date`, `Math.random`, `JSON.parse`, `setTimeout`, raw `fetch` — are rejected. Native primitives remain owners only when they carry the invariant directly.
- `DEFINITION_TIME_ASPECTS` — cross-cutting capability attaches at definition time as composition values: `Schedule` policies, `Layer` wrapping, `HttpApiMiddleware`, schema annotations, `Effect.fn` instrumentation. Decorators are rejected; an aspect materializes policy and never hides control flow, and policy pushed across the admission seam stops being recoverable from its declaration.

[INTEGRATION]:
- `ROOT_REBUILD` — new capability is woven into the owning shape as if it had always existed; reshape the owner rather than appending beside it. No shims, compat aliases, `[Obsolete]` layers, or migration surfaces — break the API when the collapse improves the system.
- `ONE_HOP_RESOLUTION` — a name resolves to its semantics in one hop: no alias-to-constant-to-enum-to-class chains, no forwarding helpers, no helper or util shells, no convenience wrappers. A value that takes two jumps to trace marks a layer to delete.
- `COMPOSED_IMPLEMENTATION` — a feature of any complexity is implemented by composing the page owners — admitted shapes, typed rails, dispatch surfaces, boundary projections, numeric routes — before any new scaffold is named. The pages are one body, and the same concern resolves identically wherever it appears. A need with no composed spelling marks a missing case in an owning page's law: the law extends first, the feature lands second. Cards and snippets show this composition at full operator depth. Flat code — logic below the operator depth the admitted packages reach — is surface sprawl in time: it re-derives, line by line, what a deeper composed form states once.

## [3]-[COLLAPSE_SCAN]

Run this scan on every edit. Any signal triggers the move; three or more instances make it mandatory.

| [INDEX] | [SIGNAL]                                                   | [MOVE]                                             |
| :-----: | :--------------------------------------------------------- | :------------------------------------------------- |
|   [1]   | sibling `get`/`getMany`/`getById` names                    | one modality-polymorphic entrypoint on input shape |
|   [2]   | interface and schema declared in parallel for one concept  | one schema; the type is extracted, never restated  |
|   [3]   | try/catch inside domain flow                               | admission at the boundary, typed failure inward    |
|   [4]   | boolean parameter selects between two bodies               | one policy value — Schedule, Layer, or handler row |
|   [5]   | function calls exactly one other function                  | delete the hop                                     |
|   [6]   | repeated `_tag` switch or if-else chains                   | `Match.exhaustive` or a derived table              |
|   [7]   | raw Promise or async/await in a domain signature           | the Effect carrier end-to-end                      |
|   [8]   | `useState`/`useReducer` holding domain state               | Atom over the owning Effect service                |
|   [9]   | wrapper renames a package API                              | use the package surface directly                   |
|  [10]   | hand-rolled retry, timeout, or polling loop                | a Schedule value                                   |
|  [11]   | `new Date()`, `Math.random()`, `setTimeout` in domain code | Clock, DateTime, Random through `R`                |
|  [12]   | `Effect.provide` scattered mid-pipeline                    | one Layer graph at the composition root            |

## [4]-[PAGE_CRAFT]

How pages in this folder are authored. The corpus is one body; these laws keep it coherent.

- Atlas law: this README owns doctrine and routing; each concept page owns one disjoint layer; a sibling concern is neither re-shown nor pointed to — cohesion comes from shared law, not linkage. The README is the only file that links.
- Zero meta: concept pages carry no provenance, source trace, release narration, process state, project, tool, or skill context.
- Page grammar: narrow index table, then family cards, then the snippet beside the rule it proves; the page ends at its last card section. Structure is identical across stack folders; content never is.
- Card fields are earned: `Use / Accept / Reject / Law / Boundary` lines appear on a card only where each one decides something; a field line that decides nothing is deleted, not filled.
- Snippet law: every snippet compiles under the active surface; identifiers are legal neutral names; placeholder strings such as `"<value-a>"` appear only inside literals; no project, host, or domain concept anchors a snippet.
- Snippet coverage: each snippet is doctrine-exemplary at full operator depth, roughly 3-4x denser than ordinary code, and exercises a surface region no other snippet in the corpus shows — variety within the doctrine, zero duplicated demonstrations. The region is the snippet's spotlight demonstration; finalized surfaces composed as supporting material occupy no region and duplicate nothing.
- Scale fidelity: a snippet shows the form at the shape it takes in a large system — admission, dispatch, rail, and policy composed in one fence with the growth axis visible — never an isolated minimum; a statement-bearing snippet sits beside the Exemption line naming its platform-forced seam.
- Code names before prose: every member a card or snippet names — attribute, knob, operator, generated surface — is verified against the installed package implementation before it is written; an unconfirmable claim is not authored, and a nameable surface spelled as prose is a defect — the code span is the instruction.
- Card economy: cards are few, deep, and evidence-dense; near-peer cards merge until each retained card owns a decision cluster, and a card line carries exactly one decision. A thin card deciding one concept is a sibling line, not an owner.
- Snippet adherence: snippets obey the card they prove, compose finalized laws as supporting material, and reduce lines and surface through parameterization, algorithmic structure, and package depth, never by deleting capability.
- Altitude routing: when two pages touch one fact, the ledger records the split — mechanics at the owning page, consequence at the consumer; prose re-teaching an owned mechanic is repaired by routing to the owner, while composing owned surfaces inside a snippet is supporting material and owns nothing.
- Reject columns are load-bearing: every `Use` names the spelling, wrapper, or local pattern it deletes.
- Tables enumerate, cards legislate: rows stay atomic and narrow — no prose cramming, no links inside cells; nuance moves to a card.
- Route scope is README-local: lookup rows and conflict rules live in README files; concept pages carry only the law they own.
- Manifest truth: package versions, admitted packages, compiler posture, workspace graph, lock state, and tool settings live in `package.json`, `pnpm-workspace.yaml`, `pnpm-lock.yaml`, `tsconfig*`, Biome config, and workspace tool configuration; no package-named pages; a package is named only where it changes the implementation choice.

## [5]-[CORPUS_LAW]

How the corpus accretes. The folder is one cohesive body, not isolated pages; atlas order is implementation order. The atlas `[STATE]` column is the law registry: a `finalized` page is binding law for every page authored after it; a `partial` page carries no authority and awaits rebuild; a `target` page exists only as roadmap scope. Finalization is a one-way gate — a context-free cold grade of the full page and every snippet, converging to a zero-edit pass, flips the state; the producer's grade admits, the cold grade decides.

- Three-layer inheritance: every page is authored under the doctrine, under every finalized page — adhered to, never restated, never referenced, never contradicted — and from its own source reservoir.
- Prose consumes earlier layers as given: vocabulary, owners, rails, and policy values arrive settled, never re-taught, and the page spends its lines only on its own layer.
- Reservoir residue: a page's source workspace deliberately holds more than the page prints, and it outlives the page — later enrichment and snippet work mine it without source-footnoting it; depth beyond one page's budget is corpus capital, never waste.
- Snippet stacking: code fully captures the card it proves, then composes on earlier layers' law at full doctrine depth — the new surface in the spotlight, established surfaces as supporting material, every touched concern built at the standard its owning finalized page legislates.
- Region ledger: the workspace root keeps one ledger of owned regions — page concerns at card altitude, snippet demonstrations at fence altitude. Snippet rows are written before code exists, page rows at finalization, and a duplicated region is repaired by routing to its owner, never by re-teaching — the ledger is what makes ownership checkable without re-reading the corpus.
- Purpose: the corpus is loaded as the operative standard in place of weaker context; zero-meta, zero-anchor, and stated-as-fact are absolute because any provenance, hedge, or stale claim poisons every downstream generation that loads the page.
