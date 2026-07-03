export const meta = {
  name: 'stack-cs',
  whenToUse: 'Harden the docs/stacks/csharp code doctrine in place to the dense per-file bar.',
  description: 'Focused full HARDENING of the docs/stacks/csharp code doctrine — every core page AND every domain/ shard, improved in place to the same 13/10, ultra-dense, page-craft-conformant bar the python doctrine now holds. The csharp set is the historical FLOOR/reference; this pass pulls it UP to the rigor the python rebuild established: page-craft grammar (narrow index table -> deep family cards -> one agnostic snippet per region, zero duplicated demonstrations), the ~450 soft LOC density signal, extreme ADT collapse ([Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject] + source-generated case families), two-weave AOP (definition-time source-gen aspects + composition-time effect transformers), LanguageExt Fin/Validation/Option/Eff rails, full parameterization/polymorphism, C# 14 on net10 to the metal. Now also a bounded interface/graph/mapping LAW extension (cross-stratum seam, graph-as-closed-family, generated mapping/equality aspects, plus QuikGraph/Riok.Mapperly/Generator.Equals elevated to admitted core substrate) hardened into existing owners, plus an optional default-off gated new-core-page valve; still restructure-free at heart — a hostile per-file harden. Phases: Inventory (atlas order: 7 core + the domain/ router + shards) -> Gate (default-off justified-new-page valve) -> Harden (1 agent/file, 3-step ADVERSARIAL rebuild(max) -> critique(xhigh) -> redteam(max), CAP=10) -> Sweep (sequential atlas-order pass, implicit upward stacking, no duplicated snippets) -> Reconcile (union-find cross-file residuals -> fix(max) -> adversarial WRITING verify(max)). Snippets agnostic (neutral names, no project anchor); every host/NuGet member verified via assay api; every edit scoped to docs/stacks/csharp (NEVER edit a python/typescript file). Takes no args.',
  phases: [
    { title: 'Inventory', detail: 'parse the README atlas + the domain/ router for the ordered core + domain file set + per-file state, emit the region ledger seed' },
    { title: 'Gate', detail: 'justification gate (default harden-in-place): only on an explicit cited justification + target atlas position author ONE new core page, edit the README atlas/STATE, seed the region ledger, and splice it into the ordered set so Harden and Sweep treat it as a corpus member' },
    { title: 'Harden', detail: 'per file (1 agent/file): rebuild(max) -> critique(xhigh) -> redteam(max), every stage ADVERSARIAL (naive/illusory-by-default), pooled at CAP=10' },
    { title: 'Sweep', detail: 'sequential atlas-order pass: each file reads finalized priors via the region ledger, routes altitude (no re-teach), removes duplicated snippet demonstrations' },
    { title: 'Reconcile', detail: 'TERMINAL no-defer loop: union-find cluster every cross-file residual -> fix(max) -> ADVERSARIAL WRITING verify(max: re-derives necessity, proves the fix on disk, repairs weak or token fixes to the root form itself, then classifies) -> re-cluster still-open + newly-surfaced, until dry (bounded). Nothing handed off or dropped.' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'docs/stacks/csharp'

// --- [MODELS] ----------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order'], properties: { path: { type: 'string' }, order: { type: 'integer' }, folder: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } }, call: { type: 'string' } } } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'owned_regions'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['routed', 'clean'] }, rerouted: { type: 'array', items: { type: 'string' } }, owned_regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } }, repaired_files: { type: 'array', items: { type: 'string' } } } }
const GATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'reason'], properties: { verdict: { type: 'string', enum: ['harden_in_place', 'new_page'] }, reason: { type: 'string' }, page: { type: 'object', additionalProperties: false, required: ['path', 'atlas_index', 'decision', 'justification'], properties: { path: { type: 'string' }, atlas_index: { type: 'integer' }, decision: { type: 'string' }, folder: { type: 'string' }, justification: { type: 'string' }, seed_regions: { type: 'array', items: { type: 'string' } } } } } }
const SEED_SCHEMA = { type: 'object', additionalProperties: false, required: ['path', 'verdict'], properties: { path: { type: 'string' }, atlas_index: { type: 'integer' }, regions: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['seeded', 'aborted'] } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'TARGET: docs/stacks/csharp/ is the route-owned C# CODE DOCTRINE — a doc set of AGNOSTIC teaching pages (core + a domain/ shard set) that ' +
    'legislate how all project C# is written. A page teaches a coding LAW with one exemplary agnostic snippet, never a concrete module. The README ' +
    'owns routing + the 17 named laws + the COLLAPSE_SCAN + page-craft; the domain/ README is a one-table router whose shards compose the core ' +
    'laws and never re-open them; each concept page owns ONE disjoint layer and states doctrine as fact. READ docs/stacks/csharp/README.md (its ' +
    '[DOCTRINE]/[COLLAPSE_SCAN]/[PAGE_CRAFT]/[CORPUS_LAW] sections) and the domain/README.md router and hold them as law.',
  'PARITY BAR: the PYTHON doctrine docs/stacks/python/ is the peer-rigor reference — this csharp set is pulled UP to match its page-craft, ~450 ' +
    'soft LOC density signal, extreme ADT/AOP/parameterization, and zero-duplicated-demonstration corpus law. READ docs/stacks/python/ read-only ' +
    'as the rigor benchmark; carry the SHARED density laws into C# idiom, never a Python spelling. The csharp doctrine is its own language: C# 14 ' +
    'on net10, Thinktecture generated owners + LanguageExt rails.',
  'WRITE-FULLY MANDATE, scoped to docs/stacks/csharp/** ONLY: every defect you identify you FIX NOW via Edit/Write directly in the file; the ' +
    'structured fix-log you return is a REPORT of edits ALREADY MADE, never a to-do list or a would/should hedge. Edit ONLY files under ' +
    'docs/stacks/csharp/ (NEVER a python/typescript/standards file; reading them is allowed). Leave nothing behind except genuine cross-FILE items ' +
    '(report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the page is NAIVE, SHALLOW, JUNIOR, or ILLUSORY until it ' +
    'survives an aggressive attack; the burden of proof is ON THE PAGE. `finalized`, "mature", "already strong", "good enough", and a prior ' +
    '`clean` grade are REJECTED self-assessments. Default to "this page must be hardened to the strongest form the doctrine admits" and MAKE that ' +
    'harden; a no-edit verdict is earned ONLY after a genuinely aggressive attack finds nothing.',
  'ILLUSORY / FAKE content is the PRIMARY target — a snippet that READS dense (uses `[Union]`/`[SmartEnum]`/the rails, cites packages) yet ' +
    'demonstrates a THIN slice; prose that ASSERTS richness the fence lacks; a card field that decides nothing; a structurally-correct collapse ' +
    'that is semantically empty; a host/NuGet member cited but unverifiable (a PHANTOM — delete it). Treat dense, confident-looking fences with ' +
    'MORE suspicion, and DISBELIEVE every claim the page makes about itself until verified.',
  'NAIVETY is a defect on TWO orthogonal axes, both intolerable: COVERAGE — the owner models a THIN SLICE of its concept (the obvious three ' +
    'fields where the domain carries fifteen; a two-case family for a twenty-case space); APPROACH — enumerated hardcoded instances where a ' +
    'parameterized, algorithmic owner should GENERATE the space (a fixed roster of styles, patterns, or variants is seed DATA feeding ONE ' +
    'generator over named parameters, never the mechanism itself). Attack both axes in every fence and repair on sight.',
].join('\n')
const CSDOCTRINE = [
  'HOLD the README [DOCTRINE] 17 laws as fact, never restated on a concept page: [FLOW] EXPRESSION_SPINE (domain logic expression-shaped; ' +
    'dependent steps `Bind`, independent ones accumulate applicatively; the carrier selects the algebra; statements only in measured ' +
    'ref-struct/span kernels that name the exemption) + BOUNDARY_ADMISSION (raw admitted EXACTLY ONCE into an evidence-carrying owner; interior ' +
    'never re-validates or sees null/sentinel/provider shape). [SHAPE] SHAPE_BUDGET + DEEP_SURFACES + MODAL_ARITY + ANTICIPATORY_COLLAPSE + ' +
    'INTERFACE_SEAM. ' +
    '[DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + ' +
    'DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION. Run the COLLAPSE_SCAN on every fence: any ' +
    'signal triggers the move, 3+ make it mandatory; the scan list is a FLOOR, never the complete set — any repeated structure, parallel spelling, ' +
    'or enumerable family an algebra, table, fold, or generator can own is a collapse target you find yourself.',
  'A page that demonstrates a coding law must itself obey every law it can reach; a domain shard COMPOSES the finalized core laws as settled ' +
    'material and never re-opens admission/shape/rail/dispatch/boundary decisions.',
].join('\n')
const CS_SHAPE = [
  'EXTREME SHAPE/TYPE DENSITY: one concept owns exactly ONE type as a dense closed family chosen by OWNER_CHOOSER — `[ValueObject<TKey>]` ' +
    '(invariant-bearing scalar), `[ComplexValueObject]` (N-field product), `[SmartEnum<TKey>]` (wire-keyed vocabulary) / `[SmartEnum]` keyless ' +
    '(process-local behavior), `[Union]` (closed alternatives with per-occurrence payload) / `[Union<T1,...>]` ad-hoc, `record`/`readonly record ' +
    'struct` (interior product), a frozen set/table, or a language `enum` at the seam only. KILL parallel DTOs, one-field wrappers, field-rename ' +
    'shapes, nullable-as-failure, struct-`default` ghosts, and 3+ sibling types/factories/switch-arms for one concept (collapse to ONE generated ' +
    'owner or a `Fold` algebra / frozen data table); this kill list is a floor, never the full set — hunt collapse targets beyond it.',
  'ANTICIPATORY_COLLAPSE: shape the owner for the family it WILL absorb so the next case/dimension/modality lands as ONE generated case/row/policy ' +
    'value with every consumer untouched or broken LOUDLY at compile time (total generated `Switch`, NO runtime-silent `_` arm). The exemplary ' +
    'snippet shows one owner ready to replace 10+ loose things with the growth axis visible.',
].join('\n')
const CS_INTERFACE = [
  'INTERFACE DOCTRINE (lib contracts are load-bearing seams, never loose markers): an interface is EITHER the COMPOSE/inject face — a member-bearing ' +
    'strategy/store/codec/provider contract or a BCL `IAsyncEnumerable<T>`/`IDisposable`/`IAsyncDisposable` stream/lifetime contract — paired with an ' +
    'abstract-base IMPLEMENT seam that owns the shared state + template virtuals, OR a static-abstract self-constrained contract a generic dispatches ' +
    'on (`where TSelf : IObjectFactory<TSelf,TKey,TError>`, generic-math `where T : INumber<T>`/`IFloatingPointIeee754<T>`). OWNER-VEHICLE chooser, ' +
    'decided ONCE: WE close the value-shape family + in-build consumer -> `[Union]`/`[SmartEnum]`/`[ValueObject]` + total generated `Switch`; ' +
    'concretes share state/template or a foreign base/native-handle/self-typed `new()` recursion -> abstract class at the seam; foreign/downstream ' +
    'code must implement a strategy OR a generated/catalogued family publishes across an assembly/wire boundary -> an interface FLOOR + minting ' +
    'factory + ONE polymorphic operation over the floor (consumers hold the floor; the concrete is `internal` + swappable).',
  'CROSS-STRATUM SEAM ALIGNMENT: when N sibling owners on the SAME stratum must align WITHOUT referencing one another, the floor is hosted by ONE ' +
    'type on the LOWEST shared stratum (the seam owner every sibling already depends upon), exposing a single INSTANCE-interface FLOOR (an ' +
    '`IProjection<TIn,TOut>`-shaped strategy each sibling family IMPLEMENTS, never a marker) plus the minting/dispatch operation over the floor. ' +
    'Siblings align by CONTRACT — each implements the floor against its own shapes — so the seam type is the ONLY shared symbol and peers stay ' +
    'swappable with zero peer references. This is the INTERFACE_SEAM publishes-across-a-boundary vehicle turned HORIZONTAL: the floor lives below, ' +
    'the conformances live in each sibling, alignment is structural, never coupling.',
  'FOREIGN CONSTRAINT INTERFACE vs INTERNAL SWITCH (distinct seams on ONE owner): a constraint/validation contract FOREIGN or downstream code ' +
    'supplies — an `IConstraint`-shaped interface with one `Check`/`Validate` member returning `Validation<Error,T>` so independent violations ' +
    'ACCUMULATE applicatively — is a legitimate instance-interface floor, NOT the owner`s internal total generated `Switch`, which the owner CLOSES ' +
    'and never publishes. Discriminant: an OPEN extension point others plug into (constraints, projections, strategies) is an interface floor ' +
    'returning a rail; a CLOSED family the owner exhaustively dispatches is a `[Union]`/`[SmartEnum]` + `Switch`. They CO-EXIST (closed shape ' +
    'switched internally, open constraint set folded over `Validation` at the boundary); conflating them — an interface over the closed family, or a ' +
    '`Switch` over the open extension set — is the rejected form.',
  'REJECT: a member-less marker read by `is`/reflection (admissible ONLY as a generic bound `where T : IMarker`; a capability switch is an attribute ' +
    'or a generated-owner conformance); an interface over a family WE close (forfeits `Switch` totality); a `[Union]` foreign code must extend; an ' +
    'INSTANCE default-interface-method (defaults derive via `static virtual` from a minimal core — zero admitted package ships an instance DIM); a ' +
    'generic-math constraint over a carrier stopping at `IComparable`/`IEquatable`/`IFormattable` (decorative — widen to the concrete carrier); a ' +
    'heap delegate on a measured hot path where a `ref struct` visitor (`allows ref struct`, caller-owned stack) fits; an invariant generic where ' +
    '`out`/`in` removes a cast; parallel `IFooForBar` names where the type parameter or keyed service carries the modality. This law lands in README ' +
    '[SHAPE] INTERFACE_SEAM, shapes.md OWNER_CHOOSER, surfaces-and-dispatch.md (static-abstract + visitor dispatch forms), boundaries.md ' +
    '(hold-the-floor + lifetime + the cross-stratum seam floor), and rails-and-effects.md (the accumulating-`Validation` constraint fold).',
].join('\n')
const CS_AOP = [
  'TWO-WEAVE AOP: definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) attach via attribute-directed SOURCE ' +
    'GENERATION in the fixed generator-owned order; composition-time concerns attach as effect transformers in author order — retry as ' +
    '`Schedule`-driven `IO<T>.Retry`/`Prelude.retry`, recovery as named catch combinators (`@catch`/`catchOf`/`CatchM` composed via `|`), resource ' +
    'lifetime as `Bracket`/`BracketIO`/`Finally`; the two weaves meet at EXACTLY ONE seam, the admission rail bridge. 2-4 co-occurring wrappers ' +
    'collapse into ONE aspect; an aspect NEVER raises into domain flow; inline-repeated concerns and sibling helper methods are defects.',
  'MAPPING + EQUALITY are DEFINITION-TIME generated aspects, never hand-written: an owner<->DTO/proto/wire projection is emitted by Riok.Mapperly ' +
    '(a `[Mapper]` partial-producing source-generator; members verified via assay), and structural equality + the content-key ride EITHER the ' +
    'Thinktecture generated owner`s value semantics (`[ValueObject]`/`[ComplexValueObject]`) OR Generator.Equals (`[Equatable]`) for shapes ' +
    'Thinktecture does not own — a class-root `[Union]` node/edge type that otherwise SURRENDERS generated equality is the canonical case. A ' +
    'hand-rolled `Equals`/`GetHashCode`, a field-by-field hand mapper, or a runtime-reflection projector is the rejected form. These join ' +
    'admission/identity/dispatch/serialization in the fixed generator-owned order; the content-key aspect is the SAME canonical byte-codec the ' +
    'graph law addresses content by, never a second hashing path.',
].join('\n')
const CS_RAILS = [
  'RAILS (RAIL_CHOOSER, narrowest carrier chosen ONCE at admission): `Option<T>` absence, `Fin<T>` synchronous fallibility, `Validation<Error,T>` ' +
    'independent accumulated faults, `Eff<RT,T>` runtime capability, `IO<T>` deferred boundary work, `Schedule` retry, ' +
    '`Seq<T>`/`Arr<T>`/`HashMap<K,V>` immutable traversal. The fault type is a CLOSED `[Union]` family deriving from `Expected` (a bare exception ' +
    'or a generic untyped `Error` for a multi-cause domain is a defect; recovery identity via `Is`/`HasCode`/`IsType<E>`, never `==`); ' +
    'accumulate-vs-abort correct (`Apply`/`&`/`.Traverse` for independents, `Bind`/`.TraverseM`/query expressions for dependents); total generated ' +
    '`Switch` with compile-time exhaustiveness; `.Fold`/`.Traverse`/`.Choose` with the mandatory `.As()` re-anchor; NO exception control flow in ' +
    'domain logic, NO mutable accumulation.',
].join('\n')
const CS_CORE_LOGIC = [
  'WORLD-CLASS ALGORITHMIC BODIES: every body that does real work is expression-shaped at full operator depth — a naive `for`/`foreach` with ' +
    'mutable accumulation, a hand-rolled index counter, or an intermediate materialized list where a deferred pipeline or fold expresses it is a ' +
    'DEFECT. Compose deferred LINQ at depth (`Aggregate`/`SelectMany`/`Zip`/`Chunk`/`GroupBy`/`Scan`) or a LanguageExt ' +
    '`.Fold`/`.FoldBack`/`.Traverse`/`.Choose` over the carrier; a measured hot path is a `ref struct`/`Span<T>` kernel named at the ' +
    'EXPRESSION_SPINE exemption (vectorized via `TensorPrimitives`/`Vector<T>` where the shape admits); collection expressions + spread, ' +
    'list/slice/relational/logical patterns, and switch-expression dispatch over a closed family replace imperative branching. NO mutable ' +
    'accumulation in domain flow, NO intermediate sequence a fold would fuse, NO LINQ over a measured hot loop where a span kernel is the faster owner.',
].join('\n')
const CS_GRAPH = [
  'GRAPH-AS-CLOSED-FAMILY (the domain-graph law, DISTRIBUTED across shapes.md / algorithms.md / boundaries.md, never a standalone page, never ' +
    're-taught): a domain graph is a PROPERTY GRAPH whose every edge kind is ONE neutral edge-algebra `[Union]` over a small closed verb set ' +
    '(compose / assign / associate / connect / void-shaped, each carrying a typed payload) PLUS ONE `Generic(wireName, relating, related, attrs)` ' +
    'passthrough case for the open tail, so no foreign relation is dropped and the foreign relationship TAXONOMY never leaks into the neutral ' +
    'owner; the node is ONE `[Union]` over the entity family keyed in a map, and the consumer-facing aggregate is a DERIVED FOLD over the reachable ' +
    'subgraph, NEVER a second stored record. N typed per-relation classes mirroring a foreign schema is the rejected form (a `[03]-[COLLAPSE_SCAN]` ' +
    'trigger: cases mirror a foreign taxonomy -> neutral algebra + `Generic` passthrough). shapes.md OWNS the node/edge union via OWNER_CHOOSER.',
  'PHASE SPLIT + INCIDENCE (algorithms.md / system-apis.md COLLECTIONS_AND_IDENTITY): the graph has TWO phases behind one surface — a persistent ' +
    '`HashMap`/`ImmutableDictionary` WORKING graph for O(log n) structural-sharing mutation, baked ONCE via `ToFrozenDictionary()` into a `Frozen*` ' +
    'READ SNAPSHOT carrying a memoized INCIDENCE INDEX (node -> incident edges) so traversal is O(degree) not O(E); QuikGraph owns the graph view + ' +
    'traversal/topology algorithms over the snapshot. The bake is the one-way working->snapshot transition; mutating a snapshot or rebuilding the ' +
    'index per query is the rejected form, and the two collection cards JOIN as two PHASES of one structure rather than spawning a parallel card. ' +
    'CONTENT ADDRESSING is ALREADY OWNED: graph identity + structural diff COMPOSE boundaries.md BYTE_IDENTITY (one canonical byte-codec per ' +
    'identity domain, the memo key in MEMO_KEY) and system-apis.md INTEGRITY/IDENTITY_POLICY — reuse that ONE codec VERBATIM for both key-mint and ' +
    'diff; a second hashing/serialization path or a fresh content-addressing card is the rejected form. The rooted graph id is a NEUTRAL kernel ' +
    'value; any foreign/wire id is a boundary attribute, never the kernel key.',
].join('\n')
const PARAM_POLY = [
  'HEAVY PARAMETERIZATION, ZERO HARDCODING/FRAGILE LOGIC, FULL POLYMORPHISM. ONE entrypoint owns every modality, discriminating on input SHAPE ' +
    '(`params ReadOnlySpan<T>`, request union, case), never a name suffix or a `bool`/`mode`/`batch` knob (KNOB_TEST: delete each parameter; if ' +
    'the value reconstructs what it carried, it was a knob — collapse a flag into a policy value or input-shape discriminant; move ' +
    '`timeout`/`retry`/`deadline`/`CancellationToken` off the signature onto the carrier or a composition-time aspect). Configuration enters as ' +
    'ONE behavior-carrying value (smart-enum row, union case, frozen table — POLICY_VALUES), never flag sets the body re-derives; cases sharing ' +
    'structure are DERIVED (one primary correspondence, secondaries derived) — never enumerated arms.',
].join('\n')
const CS14 = [
  'LATEST STABLE C# 14 on net10 to the metal (`Nullable enable`, NRT enforced): primary constructors, collection expressions with spread, `params` ' +
    'collections (incl. `params ReadOnlySpan<T>`), list/slice/relational/logical patterns, switch expressions, `required` members, `file`-scoped ' +
    'types, `field` accessors, extension blocks (`extension(Receiver)`) and extension operators, generic math / static abstract+virtual interface ' +
    'members, `with` expressions, `System.Threading.Lock`, raw string + `u8` literals where they fit. Apply the docs/stacks/csharp ' +
    'file-organization + section-order law: `[Union]`/`[SmartEnum]`/`[ValueObject]` + generated case families stay inside the declaring owner ' +
    'block; canonical order TYPES -> CONSTANTS -> MODELS -> ERRORS -> SERVICES -> OPERATIONS -> COMPOSITION -> EXPORTS; one generated case / ' +
    'static entry per physical line; preserve generated-case + smart-enum semantic order.',
].join('\n')
const CS_SUBSTRATE = [
  'STACK CAPABILITY, ULTRA-STACKED: enumerate BOTH `.api` tiers with a REAL ls/fd listing from disk, never memory — the central `libs/csharp/.api/` ' +
    'substrate catalogs (api-thinktecture-runtime-extensions, api-quikgraph, api-mapperly, api-generator-equals, api-mathnet-numerics, api-csparse, ' +
    'plus every sibling catalog the page concern touches) and, for a domain shard, the per-folder `libs/csharp/<Package>/.api/` host/NuGet catalogs ' +
    'its concern composes — and MINE them to OPERATOR DEPTH (read-only material; edits stay scoped to docs/stacks/csharp). The universals are ' +
    'Thinktecture.Runtime.Extensions (generated domain shape) + LanguageExt.Core ' +
    '(rails, effects, schedules, immutable collections; catalog-less — assay api is its evidence), with QuikGraph (graph traversal/topology + ' +
    'graph algorithms), Riok.Mapperly (generated ' +
    'owner<->DTO/proto/wire mapping), and Generator.Equals (generated structural equality + content-key for shapes Thinktecture does not own, e.g. ' +
    'class-root `[Union]` node/edge types) as ADMITTED CORE substrate integrated ground-up the SAME way and NAMED in the README [02] LIBRARY_DEPTH ' +
    'law beside MathNet/CSparse (hand-rolled graph traversal, field-by-field mapping, or `Equals`/`GetHashCode` is the rejected form), layered ' +
    'onto the BCL; MathNet/CSparse own numeric algorithms where relevant. Compose EVERY relevant member into single dense owners woven as ONE rail ' +
    '(source-generated owners, `Fold` algebra, data tables), ALWAYS layering ' +
    'Thinktecture/LanguageExt/QuikGraph/Riok.Mapperly/Generator.Equals onto the domain surface, NOT flat one-shot ' +
    'per-API uses. Use the DEEPEST operator/combinator/generated surface each library reaches (LIBRARY_DEPTH); an admitted capability the concept ' +
    'admits but NO owner exploits is a DEFECT this pass closes; reject surface-level subsets, BCL-first reflexes, and thin rename wrappers. Cite ' +
    'ONLY host/NuGet members confirmed via `uv run python -m tools.assay api` (verified-local beats any catalog line on conflict) — a member you ' +
    'cannot verify is a phantom to delete.',
].join('\n')
const PAGECRAFT = [
  'PAGE-CRAFT LAW (README [PAGE_CRAFT]): page grammar is a NARROW index table, then deep FAMILY CARDS, then ONE agnostic snippet beside the rule ' +
    'it proves; the page ends at its last card. CARD ECONOMY: cards are few, deep, evidence-dense; near-peer cards MERGE until each owns a ' +
    'decision cluster; a card line carries exactly ONE decision; a `Use`/`Accept`/`Reject`/`Law`/`Boundary` field appears only where it decides ' +
    'something. REJECT columns are LOAD-BEARING: every `Use` names the spelling/wrapper/local pattern it DELETES. CODE NAMES BEFORE PROSE: every ' +
    'member named is verified against the package before written; a nameable surface spelled as prose is a defect. ZERO META: no provenance, ' +
    'source trace, release narration, process state, or tool context — any such block POISONS every downstream generation. The README is the only ' +
    'file that carries a Markdown link; the domain/ README is a one-table router.',
].join('\n')
const AGNOSTIC_SNIPPETS = [
  'AGNOSTIC SNIPPET LAW (style-guide [PLACEHOLDER_LAW]): every snippet compiles under C# 14 with legal NEUTRAL identifiers — ' +
    '`Shape`/`RefinedShape`/`Variant`/`PRIMARY`/`Field`/`KEY`/`Row`/`ROW_A`/`TABLE`/`SELECTED` — and placeholder strings (`"<value-a>"`) appear ' +
    'ONLY inside literals. NO project, host, repo, customer, pricing, or business-domain noun anchors a snippet; a domain noun is context poison.',
  'CORPUS-WIDE ZERO duplicated snippet demonstrations: each snippet exercises a surface region NO OTHER snippet in the corpus (core + domain/) ' +
    'shows — the region is its spotlight; finalized surfaces composed as supporting material occupy no region and duplicate nothing. A duplicated ' +
    'region is repaired by ROUTING to its owner, never by re-teaching. Snippets are doctrine-exemplary at full operator depth, ~3-4x denser than ' +
    'ordinary code, at the scale a large system takes (admission + dispatch + rail + policy in one fence with the growth axis visible).',
].join('\n')
const OPINIONATED = [
  'HEAVILY-OPINIONATED PROJECT DOCTRINE, NOT a language survey. ZERO table-stakes is tolerated, ever: a card or snippet teaching something a ' +
    'competent C# developer already knows — rather than an opinionated, dense, project-specific CHOICE — is a DEFECT to delete or densify. No ' +
    'net-casting to "cover the language"; cover only the opinionated decisions the projects need, each at 13/10.',
  'LOC budget ~450 is a SOFT pressure signal toward DENSIFICATION, NOT a hard gate. The real metric is per-card and per-snippet density: every ' +
    'card and every snippet world-class, zero filler. NEVER strip snippet whitespace, remove design content, or fragment a coherent concept to hit ' +
    'a number; a coherent dense concept (e.g. the algorithms monolith) may exceed 450 when every card and snippet earns its place. A split is ' +
    'justified ONLY by concept disjointness, never by line count.',
  'ANTI-PROLIFERATION (default = harden what exists): this pass DENSIFIES and CORRECTS existing cards/snippets in place; it does NOT grow the card ' +
    'count to look thorough. A NEW card is added ONLY to close a genuinely-OMITTED opinionated capability NO existing card owns, and every addition ' +
    'cites its ONE source (a verified package member, a doctrine law, or a consumer contract). A card that restates a neighbor, splits one decision ' +
    'across two cards, or projects a speculative future case with no cite is FILLER to delete, not author. A new core PAGE is a RARE gated ' +
    'exception, never a default move: a law delta lands as in-place hardening of its owning page unless the justification gate returned an explicit ' +
    'new-page verdict with a target atlas position. Rising card/snippet/page COUNT is a defect signal absent a cited omitted capability — the bar is ' +
    'density per card, not cards per page.',
  'ALREADY-OWNED, DO NOT RE-AUTHOR (the top spam traps): CONTENT-ADDRESSING / byte-identity is TRIPLE-OWNED by boundaries.md BYTE_IDENTITY + ' +
    'system-apis.md INTEGRITY + system-apis.md IDENTITY_POLICY (plus the durability/data-interchange domain shards for persisted artifacts) — the ' +
    'graph/content-key law COMPOSES these owners and ROUTES to them, NEVER re-teaches a canonical-byte/hash/quantization card. The INTERFACE-SEAM ' +
    'form is fully owned by README INTERFACE_SEAM + shapes.md OWNER_CHOOSER + surfaces-and-dispatch.md type-level dispatch + boundaries.md the ' +
    'floor; the cross-stratum delta is at most ONE appended consequence clause, not a restated seam card. The accumulating-`Validation` constraint ' +
    'rail is owned by rails-and-effects.md VALIDATION_MONOID + domain/validation.md. The mutate->freeze collection owners exist in system-apis.md ' +
    'COLLECTIONS_AND_IDENTITY and the memo key in boundaries.md MEMO_KEY — the phase-split harden JOINS the existing cards. Re-authoring any of ' +
    'these as fresh cards is the defect this pass most often commits — route to the owner instead.',
].join('\n')
const STYLE_PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md: lead each section with the controlling rule/contract; one idea per paragraph; close on the ' +
    'consequence or boundary. Cut hedges (`may`/`might`/`probably`/`generally`/`where possible`/`if needed`), provenance, process narration, and ' +
    'report framing. Prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ' +
    'ASSERTS capability the fence lacks is a defect, not content. BACKTICK every symbol, type, field, member, operator, package ID, path, command, ' +
    'and literal value; name the exact member instead of paraphrasing behavior.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing. KEEP the canonical section-divider headers (`// --- [UPPERCASE_LABEL]` ' +
  'dash-fill). Beyond dividers, comment ONLY where intent is not already obvious from names, types, and signatures: default ZERO comments; at most ' +
  '1 line where a comment genuinely earns its place; 1-2 lines only for a truly subtle invariant or boundary. No narration, no restating the code, ' +
  'no XML-doc bloat, no task/process/review comments.'
const DOCTRINE = [LAW, '', ADVERSARIAL, '', CSDOCTRINE, '', CS_SHAPE, '', CS_INTERFACE, '', CS_AOP, '', CS_RAILS, '', CS_CORE_LOGIC, '', CS_GRAPH, '', PARAM_POLY, '', CS14, '', CS_SUBSTRATE, '', PAGECRAFT, '', AGNOSTIC_SNIPPETS, '', OPINIONATED, '', STYLE_PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  let gate = Promise.resolve()
  const launch = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  const run = async () => { while (next < items.length) { const i = next++; await launch(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}
const nameOf = (p) => p.indexOf(ROOT + '/') === 0 ? p.slice(ROOT.length + 1) : p
const authorPrompt = (page) => [DOCTRINE, '', 'TASK: HOSTILE HARDEN of ' + page + ' to the ULTRA-DENSE C# doctrine bar. DISBELIEVE the page — ' +
  'assume the fence is naive, junior, or illusory until proven 13/10, and treat dense confident-looking content as a prime suspect for ' +
  'hollow/decorative complexity. Read the page, the README atlas + doctrine + the domain/README router, its sibling pages (cross-page ' +
  'unification), the PYTHON doctrine (the read-only rigor benchmark), the style-guide, and VERIFY every cited host/NuGet member via `uv run python ' +
  '-m tools.assay api`. Construct in BOUNDARY_ADMISSION lifecycle order; collapse parallel shapes into ONE generated closed-family owner chosen by ' +
  'OWNER_CHOOSER; weave every cross-cutting concern as a definition-time source-gen aspect or a composition-time effect transformer over a thin ' +
  'pure core (two-weave AOP); parameterize fully; one polymorphic entrypoint per modality; C# 14 / net10 to the metal. Make the exemplary snippet ' +
  'AGNOSTIC (neutral names, no project noun), compiling, ~3-4x denser than ordinary code, one owner ready to replace 10+ loose things with the ' +
  'growth axis visible. Cut every table-stakes card/snippet and every loose type/constant cluster. Apply page-craft + section-order + ' +
  'style/comment hygiene. Report `collapsed` (count before->after), `extended` (each addition + cited source), and the page`s spotlight `regions`. ' +
  'verdict is `rebuilt` unless the page genuinely survived untouched. Return residual_high — {files:[...], claim} for any CROSS-FILE item.'].join('\n')
const critiquePrompt = (page) => [DOCTRINE, '',
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + FIX IN PLACE of ' + page + '. ULTRA-HARSH, UNAGREEABLE: assume a violation exists in EVERY fence; ' +
    'trust NOTHING the prose claims; "good enough"/"mature" rejected. Read the page, the README doctrine + domain router, the sibling pages, the ' +
    'python rigor benchmark, the style-guide, and verify members via assay api. REPAIR every hit in place: (1) COLLAPSE_SCAN signals (3+ ' +
    'mandatory); (2) OWNER_CHOOSER per shape — replace any non-discriminant-correct owner, kill parallel DTOs/one-field wrappers/field-rename ' +
    'shapes/nullable-as-failure/`default`-ghosts; (3) KNOB_TEST — collapse flags to policy values/input-shape, move ' +
    '`timeout`/`retry`/`CancellationToken` to the carrier/aspect; (4) TWO-WEAVE AOP — definition-time source-gen vs composition-time effect ' +
    'transformers, one seam, no inline-repeated concerns; (5) RAILS — narrowest carrier, closed `Expected` `[Union]` fault, accumulate-vs-abort ' +
    'correct, total generated `Switch` (no silent `_`), no exception control flow; (6) C# 14/net10 + file-organization section-order; (7) the ' +
    'admitted substrate (Thinktecture/LanguageExt/QuikGraph/Mapperly/Generator.Equals) stacked to operator depth + every member verified (delete ' +
    'phantoms); (8) AGNOSTIC snippet law; (9) PAGE GRAMMAR + card ' +
    'economy + load-bearing reject columns; (10) ALTITUDE / no re-teach (a domain shard composes the core, never re-opens it); (11) ZERO META + ' +
    'style + comments; (12) CAPABILITY-COMPLETENESS + illusion + TABLE-STAKES + BOTH NAIVETY AXES — close an omitted capability in place with a ' +
    'cite; rebuild a COVERAGE thin-slice to the real breadth of its concept; collapse an APPROACH roster into ONE generator over seed rows; delete ' +
    'any table-stakes/decorative/speculative card. The 12 checks are a FLOOR, never the complete audit — hunt past them and repair every defect ' +
    'the list does not name. EDIT to fix every hit. Report `extended` and `regions`. Return residual_high {files:[...], claim}.'].join('\n')
const redteamPrompt = (page) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of ' + page + ' — the LAST and MOST AGGRESSIVE pass; red-team is critique AND MORE; the ' +
    'burden of proof is ON THE PAGE; trust nothing the prior passes claimed. Open the admitted substrate catalogs + the host/NuGet surface, the ' +
    'sibling pages, the README doctrine, the python rigor benchmark, the style-guide. Attack and REPAIR in place: (A) COUNTERFACTUAL on the core ' +
    'shape — does a denser generated owner / `Fold` algebra / data table / DEEPER admitted-substrate primitive collapse the whole fence? ' +
    'rebuild to it. (B) ANTICIPATORY_COLLAPSE — does the next case/provider land as ONE generated case/row with consumers broken LOUDLY at compile ' +
    'time? reshape so the growth axis is a case/row/policy value. (C) CORPUS-WIDE DUPLICATION — route any snippet re-demonstrating a region a ' +
    'core/sibling/domain page owns. (D) AOP + SHAPE-BUDGET MAXIMIZATION — push more into source-gen aspects + effect transformers over a thinner ' +
    'pure core; collapse any loose cluster into one generated family. (E) STRATA + SUBSTRATE-DEPTH + PHANTOMS — flat code below the operator depth ' +
    'the admitted substrate reaches (collapse to depth); a phantom member (delete it); a domain shard re-opening a core law (route it). (F) ' +
    'CAPABILITY-COMPLETENESS + ILLUSION + LONG-TAIL + BOTH NAIVETY AXES — name an omitted capability with a cite and extend the owner in place; ' +
    'attack the long tail (the fault rail, the empty/degenerate case, the boundary value the taught law must survive); rebuild a COVERAGE ' +
    'thin-slice to the real breadth of its concept; collapse an APPROACH roster into ONE generator over seed rows; delete ' +
    'table-stakes/decorative/speculative content. ALSO a FULL COLD ADVERSARIAL RE-REVIEW of every critique dimension. The page must end ' +
    'objectively denser, more capable, more agnostic-compliant; if the strongest form is genuinely present, find nothing — never invent churn. ' +
    'Report `extended` and `regions`. Return residual_high {files:[...], claim}.'].join('\n')
const sweepPrompt = (page, ledger) => [DOCTRINE, '',
  'TASK: SEQUENTIAL CORPUS-INTEGRATION SWEEP + FIX IN PLACE of ' + page + '. The prior pages in atlas order (core, then domain/ shards) are now ' +
    'FINALIZED. Make this page integrate them IMPLICITLY and own a disjoint region. Read THIS page from disk; you are given the REGION LEDGER of ' +
    'every prior finalized page as data (NOT bodies) — read a prior body ONLY when the ledger flags a candidate collision.',
  'ENFORCE: (1) ALTITUDE ROUTING — route any mechanic the ledger shows owned upstream (compose as settled supporting material; a domain shard ' +
    'never re-teaches a core law). (2) NO DUPLICATE SNIPPET — demote any snippet exercising an owned region and pick an UNOWNED spotlight, or ' +
    'delete it. (3) DENSITY + OPINIONATED + PAGE-CRAFT — end denser, fully agnostic, table-stakes-free, within the soft ~450 LOC density signal ' +
    '(never split a coherent concept or strip snippet whitespace).',
  'REGION LEDGER (finalized priors):\n' + JSON.stringify(ledger, null, 1) + '\nEDIT this page to fix every hit. Return UPDATED `owned_regions`, ' +
    '`rerouted`, verdict, and residual_high {files:[...], claim}.'].join('\n')
const gatePrompt = (ordered) => [DOCTRINE, '',
  'TASK: JUSTIFICATION GATE (default-off new-page valve). Three law deltas are now doctrine — (1) the CROSS-STRATUM interface seam + ' +
    'foreign-constraint-vs-internal-switch forms in the interface law, (2) GRAPH-AS-CLOSED-FAMILY (node/edge neutral-algebra union + derived-fold ' +
    'aggregate + phase split + ALREADY-OWNED content addressing), (3) the mapping/equality generated-aspect clause in the two-weave AOP law (plus ' +
    'QuikGraph/Riok.Mapperly/Generator.Equals elevated to admitted core substrate). For EACH delta decide its owner. DEFAULT and TIE go to ' +
    'harden_in_place: a delta lands as IN-PLACE hardening of an EXISTING owner page — the interface + graph facets DISTRIBUTE across shapes.md ' +
    '(node/edge union, OWNER_CHOOSER), surfaces-and-dispatch.md (dispatch over the seam), boundaries.md (the floor + content key), algorithms.md ' +
    '(the phase split + QuikGraph traversal); the mapping/equality facet folds into the definition-time weave those pages instantiate; ' +
    'content-addressing is ALREADY triple-owned and MUST route, never spawn a page. Return verdict=new_page for AT MOST ONE delta and ONLY if you ' +
    'can prove ALL of: (a) it is a DISJOINT coding-law layer NO atlas decision [01]-[07] already owns — cite EACH [01]-[07] decision and show why ' +
    'none fits; (b) it cannot be carried as cards inside an existing page without re-teaching that page`s disjoint layer; (c) you can name a precise ' +
    'target atlas_index + reader-DECISION label + folder. A new page is a structural commitment (it renumbers the whole downstream atlas and ' +
    're-finalizes every later page), not a thoroughness gesture: if ANY of (a)-(c) is weak, return harden_in_place. The ONLY plausible candidate on ' +
    'current evidence is a combinatorial GRAPH-ALGORITHM layer (topology/connectivity/traversal) that algorithms.md`s numeric charter provably ' +
    'cannot absorb — and even that is likely a package/domain concern, not agnostic doctrine. Read ' + ROOT + '/README.md ([01]-[ATLAS], ' +
    '[02]-[DOCTRINE], [05]-[PAGE_CRAFT], [06]-[CORPUS_LAW]) and the candidate owner pages. Current ordered atlas file set:\n' +
    JSON.stringify(ordered, null, 1) + '\nReturn verdict, reason, and (only for new_page) page {path, atlas_index, decision, folder, ' +
    'justification, seed_regions}.'].join('\n')
const seedPrompt = (page) => [DOCTRINE, '',
  'TASK: SEED a JUSTIFIED new core doctrine page at ' + page.path + ' (reader-decision "' + (page.decision || '') + '", target atlas index ' +
    page.atlas_index + '). The justification gate cleared it: ' + (page.justification || '') + '. (1) AUTHOR a page-craft-conformant skeleton + ' +
    'initial REAL content at ' + page.path + ' — narrow index table, then deep family cards, then ONE agnostic snippet per region; the page ends ' +
    'at its last card; states doctrine as fact, ZERO meta. (2) EDIT ' + ROOT + '/README.md [01]-[ATLAS]: insert the row at index ' + page.atlas_index +
    ', RENUMBER every downstream [INDEX] label, add the [READ] link (the README is the ONLY file that links), set this row [STATE] to `partial` (it ' +
    'gains authority only when the cold-grade gate later flips it to finalized). (3) PRE-SEED the region-ledger row for its claimed regions (' +
    JSON.stringify(page.seed_regions || []) + ') per CORPUS_LAW (snippet rows are written before code exists). Edit ONLY under ' + ROOT + '/. Return ' +
    '{path, atlas_index, regions, verdict:`seeded`}; return verdict:`aborted` and leave the corpus UNTOUCHED if on closer read the page is NOT a ' +
    'disjoint layer after all.'].join('\n')
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (page) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(page), { label: st.key + ':' + nameOf(page), phase: 'Harden', schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: STALL })
    if (r === null) break
    logs[st.key] = r
  }
  return { page, logs, ok: Object.keys(logs).length === STAGES.length }
}
const regionsOf = (logs) => { for (const st of ['redteam', 'crit', 'rebuild']) { const l = logs[st]; if (l && l.regions && l.regions.length) return l.regions } return [] }

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Inventory')
const inv = await agent('DISCOVERY — the read-only reconnaissance grounding every downstream stage; read-only is its ONLY concession. Enumerate ' +
  'from the SOURCE OF TRUTH, never memory: run a REAL find/ls listing of every page under ' + ROOT + ', read ' + ROOT + '/README.md [01]-[ATLAS] ' +
  'and the ' + ROOT + '/domain/README.md router IN FULL, and resolve the ordered set against the disk listing (a page on disk absent from the ' +
  'atlas/router, or a row with no file, is noted in that row`s `call`, never silently dropped). Return the full ordered file set: every CONCEPT ' +
  'page that exists on disk under ' + ROOT + ' (the core pages first in atlas order, then the domain/ shards in the domain router order), ' +
  'EXCLUDING every README.md and the entire .reports/ workspace. Each row {path (repo-relative), order (global integer), folder (`domain` or ' +
  'empty for core), regions (its snippet-demonstration region tags read from the corpus region ledger where one exists on disk, else from the ' +
  'page body`s actual snippet demonstrations — verified by a real read, never guessed), call (ONE hostile weak/strong line on the page grounded ' +
  'in what you read)}. This map is an initial pointer, never a ceiling: every downstream stage re-reads the full pages from disk. Use find/read; ' +
  'do not cd; do not edit anything.', { label: 'inventory', phase: 'Inventory', schema: INVENTORY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const ordered = ((inv && inv.files) || []).filter((f) => f && f.path && f.path.indexOf('/.reports/') < 0).sort((a, b) => a.order - b.order).map((f) => f.path)
log('Inventory: ' + ordered.length + ' csharp doctrine pages (core + domain) to harden')

// --- [GATE]
phase('Gate')
const gate = await agent(gatePrompt(ordered), { label: 'gate', phase: 'Gate', schema: GATE_SCHEMA, effort: 'max', stallMs: STALL })
if (gate && gate.verdict === 'new_page' && gate.page && gate.page.path) {
  const seed = await agent(seedPrompt(gate.page), { label: 'seed:' + nameOf(gate.page.path), phase: 'Gate', schema: SEED_SCHEMA, effort: 'max', stallMs: STALL })
  if (seed && seed.path && seed.verdict === 'seeded') {
    const at = Math.max(0, Math.min(ordered.length, (gate.page.atlas_index || ordered.length + 1) - 1))
    ordered.splice(at, 0, seed.path)
    log('Gate: new core page seeded -> ' + seed.path + ' at atlas index ' + gate.page.atlas_index)
  } else { log('Gate: seed aborted -> harden-in-place') }
} else { log('Gate: harden-in-place (no new core page justified)') }

// --- [HARDEN]
phase('Harden')
const done = (await pool(ordered, CAP, (page) => processPage(page))).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const ledger = []
const sweepLogs = []
const seedRegions = (p) => { const d = done.find((r) => r.page === p); if (d) { const rr = regionsOf(d.logs); if (rr.length) return rr } const iv = (inv && inv.files || []).find((f) => f.path === p); return (iv && iv.regions) || [] }
for (let i = 0; i < ordered.length; i++) {
  const page = ordered[i]
  const r = await agent(sweepPrompt(page, ledger), { label: 'sweep:' + nameOf(page), phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'xhigh', stallMs: STALL })
  const regions = (r && r.owned_regions && r.owned_regions.length) ? r.owned_regions : seedRegions(page)
  ledger.push({ file: page, owned_regions: regions })
  if (r) sweepLogs.push(r)
}
log('Sweep: ' + sweepLogs.length + '/' + ordered.length + ' pages integrated in atlas order')

const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['rebuild', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
for (const r of sweepLogs) if (r.residual_high) for (const x of r.residual_high) allRes.push(norm(x, r.file))
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusterOf = (rs) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of rs) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of rs) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const dedupRes = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
// clusterWork — a fixer's load is dominated by distinct files read + reconciled; heaviest cluster launches first so the long pole never starts last under CAP.
const clusterWork = (c) => { const files = new Set(); for (const r of c) for (const f of r.files) files.add(f); return files.size * 2 + c.length }
log('Harden+Sweep done; TERMINAL reconcile ' + uniq.length + ' residual(s) (no-defer, adversarial verify, loop until dry)')
const MAX_ROUNDS = 6
let pending = uniq
let invalid = []
let round = 0
if (pending.length) {
  phase('Reconcile')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = clusterOf(pending).sort((a, b) => clusterWork(b) - clusterWork(a) || (a[0].claim || '').localeCompare(b[0].claim || ''))
    log('Reconcile round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s); work [' + clusters.map(clusterWork).join(', ') + '] (2*files+claims)')
    const resolved = (await pool(clusters, CAP, async (cl, i) => {
      const fix = await agent([DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the harden + sweep passes ' +
        'surfaced; NO severity, NO leftovers, NO deferral (nothing leaves unfixed). Read EVERY listed file. For each real cross-file defect FIX it in ' +
        'place to the STRONGEST doctrine form (unify the shared generated owner/rail/region, repair the altitude/duplication/strata issue, GROW the ' +
        'shared owner to close a gap that spans files), preserving all capability — a token patch that leaves the defect is NOT a fix; if a residual ' +
        'is FACTUALLY INCORRECT, leave it and say why (verify will mark it invalid). If your fix SURFACES a new cross-file need, report it in ' +
        'residual_high {files,claim} so the next round resolves it. Edit ONLY under ' + ROOT + '/. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'),
        { label: 'reconcile-fix:r' + round, phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
      if (!fix) return { open: cl, invalid: [], surfaced: [] }
      const verify = await agent([DOCTRINE, '', 'TASK: ADVERSARIAL WRITING VERIFY — never a friendly confirmation; you REPAIR before you classify, ' +
        'one verdict per claim. Re-read every named file from disk. Per claim: (1) RE-DERIVE necessity — was the residual real and a change ' +
        'genuinely required? a claim that misreads the corpus is "invalid" (cite why). (2) PROVE ON DISK the fix was actually made AND is ' +
        'complete, doctrine-conformant, and non-token — ATTACK it: shallow, partial, or leaving the cross-file defect (duplicated region, ' +
        'altitude leak, un-unified owner) means NOT done. (3) REPAIR TO THE ROOT yourself via Edit, scoped to ' + ROOT + '/: a loose, weak, or ' +
        'token fix — including a single-point patch where a root-level dense reconstruction of the same files is available — is a DEFECT you fix ' +
        'NOW to the strongest doctrine form, capability preserved; never hand back a strengthenable fix. (4) Only then classify: "fixed" (proven ' +
        'on disk OR repaired by you — cite evidence, list every file you edited in repaired_files), "invalid" (claim factually wrong — cite why), ' +
        '"open" (RESERVED for a claim genuinely unreachable from the files at hand, never a fix you could strengthen yourself). Claims:\n' +
        JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'),
        { label: 'reconcile-verify:r' + round + ':' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, ROOT)) }
    })).filter(Boolean)
    invalid = dedupRes([...invalid, ...resolved.flatMap((r) => r.invalid)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    pending = dedupRes([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim))
  }
  if (pending.length) log('Reconcile: ' + pending.length + ' residual(s) STILL OPEN after ' + MAX_ROUNDS + ' rounds — REPORTED, never dropped')
  else log('Reconcile: all residuals fixed + adversarially verified across ' + round + ' round(s)')
} else { log('Reconcile: no residuals — clean') }
return { workflow: 'stack-cs', root: ROOT, ordered: ordered, hardened: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: ordered.length, region_seed: ledger, reconcileRounds: round, invalidClaims: invalid.map((x) => ({ files: x.files, claim: x.claim })), hard_residual: pending.map((x) => ({ files: x.files, claim: x.claim })) }
