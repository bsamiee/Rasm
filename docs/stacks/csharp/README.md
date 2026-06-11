# [STACKS_CSHARP]

This folder is the C# stack decision atlas. It routes language, shape, surface, rail, boundary, algorithm, system-API, and proof decisions to the concept page that owns the coding choice. The atlas builds one admission-first paradigm: raw input is admitted once through generated owners, admitted values travel typed rails, behavior lives on generated dispatch surfaces, and projections leave at explicit boundaries.

The atlas is also a build order: pages are layers of one cumulative body, and every page is authored from the full established law of the pages finalized before it. Pages cite no external sources — no links, no version narration, no provenance; verification precedes the page, and the page states law as fact.

## [1]-[ATLAS]

This table is a lookup by reader decision.

| [INDEX] | [DECISION]              | [READ]                                    | [STATE]   |
| :-----: | :---------------------- | :---------------------------------------- | :-------- |
|   [1]   | language shape          | [language](language.md)                   | finalized |
|   [2]   | domain shape            | [shapes](shapes.md)                       | partial   |
|   [3]   | surface and dispatch    | `surfaces-and-dispatch.md`                | planned   |
|   [4]   | result and effect flow  | [rails and effects](rails-and-effects.md) | partial   |
|   [5]   | host and wire boundary  | `boundaries.md`                           | planned   |
|   [6]   | numeric approach        | [algorithms](algorithms.md)               | partial   |
|   [7]   | system API replacement  | [system APIs](system-apis.md)             | finalized |
|   [8]   | proof rail              | [testing](testing/README.md)              | finalized |
|   [9]   | hosting and runtime     | `domain/runtime.md`                       | planned   |
|  [10]   | concurrency and streams | `domain/concurrency.md`                   | planned   |
|  [11]   | telemetry signal        | `domain/diagnostics.md`                   | planned   |
|  [12]   | boundary validation     | `domain/validation.md`                    | planned   |
|  [13]   | resilience policy       | `domain/resilience.md`                    | planned   |
|  [14]   | data persistence        | `domain/persistence.md`                   | planned   |
|  [15]   | compute lane            | `domain/compute.md`                       | planned   |

## [2]-[DOCTRINE]

Thirteen laws in five groups govern every C# decision in this stack. Concept pages instantiate them; no page restates them. The laws exist so correctness is structural rather than disciplinary: admission-once makes the interior total over valid values; closed families convert change into compile-time pressure; policy-as-values makes behavior recoverable from declarations alone; derivation makes every secondary surface provably consistent with its primary. Density is the consequence, not the goal — when one declaration carries the family, every remaining line is load-bearing. Enforcement is doctrine-first: `.editorconfig` severities, build-injected analyzers, and the repository's own analyzer encode these laws — the doctrine authors the tool, never the reverse. Analyzer findings against these laws are architecture pressure: fix the shape, not the diagnostic.

[FLOW]:
- `EXPRESSION_SPINE` — all domain logic is expression-shaped; dependent steps compose monadically and independent computations compose applicatively — dependence licenses sequence, independence licenses accumulation, and the carrier, never a flag, selects the combination algebra. Statements survive only inside measured kernels and platform-forced boundaries, and any page that shows one names the exemption. Composition runs through `Bind`, `Map`, query expressions, switch expressions, and generated `Switch` dispatch; `ref struct` fold kernels and span loops are the named kernel exemption.
- `BOUNDARY_ADMISSION` — raw material is admitted exactly once into an evidence-carrying owner; interior code never re-validates and never sees nulls, sentinels, or provider shapes. Generated factories and validation partials admit or reject; one rail bridge converts the generated outcome into `Fin<T>` or `Validation<Error,T>`; `Option<T>` carries absence; exceptions convert at the owning boundary.

[SHAPE]:
- `SHAPE_BUDGET` — a concept owns exactly one type; variants are cases inside one closed family, never sibling types. `[Union]`, `[SmartEnum<TKey>]`, `[ValueObject<T>]`, and `[ComplexValueObject]` own the family; families are closed internally and open only where foreign code must extend without editing the owner. Three or more parallel shapes, sibling factories, repeated dispatch arms, or single-call helpers is the collapse trigger, not a style preference.
- `DEEP_SURFACES` — prefer one rich polymorphic surface over many shallow ones; each module exposes one entrypoint family and keeps internals private. One deep owner that holds a full concern beats four fragments that scatter it.
- `MODAL_ARITY` — one entrypoint owns all call modalities; singular, plural, batch, and stream discriminate on the shape of the input value — type, case, pattern, or arity — never on name suffixes or boolean knobs. `params` collections, collection expressions, and one `ReadOnlySpan<T>` boundary absorb arity; a request `[Union]` plus one total dispatch absorbs verb families. Collapsing arity must not smuggle the knob back in: a parallel parameter that re-describes the input — a batch, many, or mode flag beside the value — is the rejected form, because the discriminant must be recoverable from the value itself.

[DERIVATION]:
- `POLICY_VALUES` — configuration enters as one domain value that carries its own behavior — a smart-enum row with constructor delegates, a union case, or a frozen policy table — never as flag sets whose combinations the implementation must re-derive. Behavior rows live with the vocabulary that selects them.
- `DERIVED_LOGIC` — when cases share generative structure, the logic is derived — state-threaded `Switch`, a `Fold` algebra, or a table — never enumerated arms. One primary correspondence is declared and every secondary map derives from it; a derived form is replaced only where a structurally cheaper primitive exists, and the replacement preserves the law the derivation encodes — the derivation is the executable specification.
- `DERIVED_TYPES` — types are computed where the language allows so one declaration yields the family: generic math constraints, type parameters with semantic payload, `K<F,A>` carrier-polymorphic arrows, and source-generated owners replace rank-specific or per-carrier copies.
- `SYMBOLIC_REFERENCE` — names, paths, discriminants, and correspondences travel as symbols and derived values — `nameof` including unbound generics, smart-enum keys, vocabulary tables — never as string literals that restate something the program already knows.

[MATERIAL]:
- `LIBRARY_DEPTH` — admitted packages are the standard library: LanguageExt owns rails, effects, schedules, and immutable collections; Thinktecture owns generated domain shape; MathNet and CSparse own numeric algorithms; proof packages own their rails. Use the deepest operator, combinator, or generated surface the package itself reaches for; wrappers, rename adapters, and BCL-first reflexes are rejected. BCL primitives remain owners only when they carry the invariant directly.
- `DEFINITION_TIME_ASPECTS` — cross-cutting capability — admission, identity, dispatch, serialization, grammar, logging — attaches at definition time through attribute-directed source generation, and generated partial hooks are the advice points on construction. Composition-time policy — retry, recovery, resource lifetime — attaches as effect transformers: `Schedule`-driven retry, named catch combinators, and bracket. Aspects materialize policy and never hide control flow. The two weaves meet at exactly one seam — the admission boundary where generated outcomes lift into rails; policy pushed across that seam in either direction stops being recoverable from its declaration.

[INTEGRATION]:
- `ROOT_REBUILD` — new capability is woven into the owning shape as if it had always existed; reshape the owner rather than appending beside it. No shims, compat aliases, `[Obsolete]` layers, or migration surfaces — break the API when the collapse improves the system.
- `ONE_HOP_RESOLUTION` — a name resolves to its semantics in one hop: no alias-to-constant-to-enum-to-class chains, no forwarding helpers, no helper or util shells, no convenience wrappers. A value that takes two jumps to trace marks a layer to delete.

## [3]-[COLLAPSE_SCAN]

Run this scan on every edit. Any signal triggers the move; three or more instances make it mandatory.

| [INDEX] | [SIGNAL]                                          | [MOVE]                                   |
| :-----: | :------------------------------------------------ | :--------------------------------------- |
|   [1]   | sibling names share a prefix or suffix            | one modality-polymorphic entrypoint      |
|   [2]   | same return rail, signatures differ only by arity | input-shape discrimination               |
|   [3]   | functions differ only by a literal                | parameterize; the literal becomes policy |
|   [4]   | boolean parameter selects between two bodies      | one derived body or one policy value     |
|   [5]   | function calls exactly one other function         | delete the hop                           |
|   [6]   | parallel dispatch arms repeat structure           | table or fold algebra                    |
|   [7]   | several types share fields for one concept        | one closed family                        |
|   [8]   | wrapper renames a package API                     | use the package surface directly         |
|   [9]   | the same 2-4 wrappers recur together              | one parameterized aspect                 |

## [4]-[PAGE_CRAFT]

How pages in this folder are authored. The corpus is one body; these laws keep it coherent.

- Atlas law: this README owns doctrine and routing; each concept page owns one disjoint layer; a sibling concern is neither re-shown nor pointed to — cohesion comes from shared law, not linkage. The README is the only file that links.
- Zero meta: concept pages carry no provenance, sourcing, version narration, process or planning state, project, tool, or skill context.
- Page grammar: narrow index table, then family cards, then the snippet beside the rule it proves; the page ends at its last card section. Structure is identical across stack folders; content never is.
- Card fields are earned: `Use / Accept / Reject / Law / Boundary` lines appear on a card only where each one decides something; a field line that decides nothing is deleted, not filled.
- Snippet law: every snippet compiles under the active surface; identifiers are legal neutral names; placeholder strings such as `"<value-a>"` appear only inside literals; no project, host, or domain concept anchors a snippet.
- Snippet coverage: each snippet is doctrine-exemplary at full operator depth, roughly 3-4x denser than ordinary code, and exercises a surface region no other snippet in the corpus shows — variety within the doctrine, zero duplicated demonstrations. The region is the snippet's spotlight demonstration; finalized surfaces composed as supporting material occupy no region and duplicate nothing.
- Reject columns are load-bearing: every `Use` names the spelling, wrapper, or local pattern it deletes.
- Tables enumerate, cards legislate: rows stay atomic and narrow — no prose cramming, no links inside cells; nuance moves to a card.
- Planning is quarantined: build order, target-page scopes, and conflict rules live only in README files — this roadmap tail and a planned subfolder's own README; concept pages never carry them.
- Manifest truth: package versions, references, injected globals, tools, and graph admission live in `Directory.Packages.props` and `Directory.Build.props`; no package-named pages; a package is named only where it changes the implementation choice.

## [5]-[CORPUS_LAW]

How the corpus accretes. The atlas `[STATE]` column is the law registry: a `finalized` page is binding law for every page authored after it; a `partial` page carries no authority and awaits rebuild; a `planned` page exists only as roadmap scope. Finalization is a one-way gate — the snippet-refinement stage's clean verification pass flips the state.

- Three-layer inheritance: every page is authored under the doctrine, under every finalized page — adhered to, never restated, never referenced, never contradicted — and from its own research reservoir.
- Prose consumes earlier layers as given: vocabulary, owners, rails, and policy values arrive settled, never re-taught, and the page spends its lines only on its own layer.
- Reservoir residue: a page's research workspace deliberately holds more than the page prints, and it outlives the page — later enrichment and snippet work mine it without citing it; depth beyond one page's budget is corpus capital, never waste.
- Snippet stacking: code fully captures the card it proves, then composes on earlier layers' law at full doctrine depth — the new surface in the spotlight, established surfaces as supporting material, every touched concern built at the standard its owning finalized page legislates.
- Purpose: the corpus is loaded as the operative standard in place of weaker context; zero-meta, zero-anchor, and stated-as-fact are absolute because any provenance, hedge, or stale claim poisons every downstream generation that loads the page.

## [6]-[ROADMAP]

Planned pages in build order. Each entry states what the page must decide; the scope moves into the page when it is authored and leaves this tail.

[SURFACES_AND_DISPATCH]:
- Owns: surface and aspect architecture — how one polymorphic entrypoint, its arity collapse, its parameter algebra, its dispatch form, and its aspects compose. Assumes language shape and generated owners from prior pages; never re-teaches pattern syntax, generated `Switch`, or smart-enum behavior rows.
- Entrypoint law: one entrypoint per concern; verb families collapse into a request union with one total dispatch; arity collapses into `params` collections, collection expressions, and span boundaries.
- Parameter algebra: knob sets collapse into policy values; optional context enters as `Option<T>` or a runtime record, never as nullable flag tails; defaults derive from the policy owner; a parameter that re-describes the input is arity smuggled back into a knob, and the anti-pattern is stated explicitly.
- Dispatch forms: state-threaded generated `Switch` for shared context, smart-enum delegate rows for case-owned behavior, frozen tables for bounded vocabulary keys, extension blocks for receiver-owned surfaces.
- Aspects: attribute-directed generation as definition-time weaving; generated partial hooks as construction advice; effect transformers, named catch combinators, and bracket as composition-time policy; deterministic stacking order.

[BOUNDARIES]:
- Owns: host, native, and wire boundary law — how foreign lifetime, absence, events, threads, and wire shapes become domain material. Assumes carriers and generated admission from prior pages; never re-teaches rail choice or factory shape.
- Resource law: every native or host resource has one capsule owner that acquires, projects, and disposes; borrowed and owned lifetimes are cases of one capsule surface, and projections leave as values, never as live handles.
- Absence law: host sentinels project to `Option<T>` at the boundary and never propagate inward.
- Event law: a subscription is a disposable value; attach and detach are symmetric and owned by one seam.
- Thread law: marshaling onto host threads is an explicit boundary effect with captured failure, never an ambient assumption.
- State law: boundary cells own session, memoization, and singleton lifetime; token-gated ownership prevents stale teardown.
- Wire law: serialization contracts stay protocol-shaped at the edge; domain owners never carry codec policy.

The `domain/` pages build in strict dependency order: runtime -> concurrency -> diagnostics -> validation -> resilience -> persistence -> compute — after every root page is finalized through the corpus sweep, so each later page implicitly carries the earlier law. Package admission to the central manifest happens at each page's research start, the docs lead admission, and the full build charter lives in [domain](domain/README.md).

[DOMAIN_RUNTIME]:
- Owns: hosting lifecycle, dependency composition with assembly scanning, decoration, and keyed services, options and configuration with AOT-safe validation, hybrid caching, the process cancellation spine, and time — clock abstraction plus calendar vocabulary.
- Composition law: one composition root per process owns scanning, decoration, and keyed registration; modules contribute registrations and never resolve services themselves.
- Options law: configuration binds and validates once at startup and travels as policy values; an ambient configuration read inside domain flow marks a seam violation.
- Assumes: boundary state law and effect rails from prior pages; never re-teaches state cells or carrier choice.

[DOMAIN_CONCURRENCY]:
- Owns: threading law, structured concurrency, channel-based producer-consumer flow, reactive streams where they change the design choice, atomic state cells at concurrency scope, and parallelism policy.
- Stream law: channels own producer-consumer seams; reactive streams are admitted only where operator composition changes the design, never as a second effect system beside the rails.
- Disjoint: the boundaries page owns host-thread marshaling; the rails page owns the effect rails concurrency composes.

[DOMAIN_DIAGNOSTICS]:
- Owns: structured logging, traces and metrics, sampling and enrichment governance, sensitive-data redaction, and one correlation spine across every signal.
- Signal law: one correlation identifier crosses logs, traces, and metrics unchanged; sampling and enrichment are root-governed policy, never per-call-site choices.
- Redaction law: sensitive shapes are redacted at definition time through annotated types; an unredacted sensitive value reaching any exporter is a seam violation.

[DOMAIN_VALIDATION]:
- Owns: wire-DTO, options, and input validation at boundaries, and the law of which validator owns which seam: generated partials admit value objects, functional validation rails own domain accumulation, the boundary validator owns wire shapes.
- Boundary: external input crosses one seam in one order — raw shape, boundary validator, typed rail, domain owner; folding an unvalidated external shape straight into a domain model skips the law.
- Assumes: domain shape and rail law from prior pages; never re-teaches them.

[DOMAIN_RESILIENCE]:
- Owns: transport and boundary resilience pipelines for remote and external hops.
- Law: domain-internal retry and repeat is schedule policy on effect rails; transport resilience is a pipeline at the seam — never both on one seam.
- Ownership: exactly one retry owner per outbound hop, held at the composition root; a lower layer detecting a second owner emits conflict evidence instead of stacking a loop.

[DOMAIN_PERSISTENCE]:
- Owns: relational persistence doctrine, provider-polymorphic across embedded and server engines as one case axis inside one doctrine: compiled models, document columns, complex types, interceptors, migrations, bulk movement, integrity, snapshots, and retention.
- Provider law: provider variance selects through one policy value; temporal values persist through one calendar vocabulary, and platform date sentinels never enter persisted shapes.
- Schema law: a store whose schema is newer than the compiled model is a typed rejection, never a best-effort open; partially applied migrations surface as operator-visible receipts.

[DOMAIN_COMPUTE]:
- Owns: tensor primitives at application scope, measured dispatch with receipts, remote compute lanes over typed contracts, and a model-inference lane scoped to verified surfaces.
- Receipt law: every measured dispatch emits typed evidence — route, elapsed, capability — beside its result; remote lanes carry schema-derived contracts proven at compile time, with payloads outside generator coverage projected to attributed records at the boundary.
- Disjoint: the algorithms page owns numeric route law; this page owns application-scope compute composition.
