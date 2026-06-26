export const meta = {
  name: 'stack-cs',
  whenToUse: 'Harden the docs/stacks/csharp code doctrine in place to the dense per-file bar.',
  description: 'Focused full HARDENING of the docs/stacks/csharp code doctrine — every core page AND every domain/ shard, improved in place to the same 13/10, ultra-dense, page-craft-conformant bar the python doctrine now holds. The csharp set is the historical FLOOR/reference; this pass pulls it UP to the rigor the python rebuild established: page-craft grammar (narrow index table -> deep family cards -> one agnostic snippet per region, zero duplicated demonstrations), the ~450 soft LOC density signal, extreme ADT collapse ([Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject] + source-generated case families), two-weave AOP (definition-time source-gen aspects + composition-time effect transformers), LanguageExt Fin/Validation/Option/Eff rails, full parameterization/polymorphism, C# 14 on net10 to the metal. NOT a restructure (the csharp file set is settled) — a hostile per-file harden. Phases: Inventory (atlas order: 7 core + the domain/ router + shards) -> Harden (1 agent/file, 3-step ADVERSARIAL rebuild(max) -> critique(xhigh) -> redteam(max), CAP=12) -> Sweep (sequential atlas-order pass, implicit upward stacking, no duplicated snippets) -> Reconcile (union-find cross-file residuals -> fix(max) -> adversarial verify(xhigh)). Snippets agnostic (neutral names, no project anchor); every host/NuGet member verified via assay api; every edit scoped to docs/stacks/csharp (NEVER edit a python/typescript file). Takes no args.',
  phases: [
    { title: 'Inventory', detail: 'parse the README atlas + the domain/ router for the ordered core + domain file set + per-file state, emit the region ledger seed' },
    { title: 'Harden', detail: 'per file (1 agent/file): rebuild(max) -> critique(xhigh) -> redteam(max), every stage ADVERSARIAL (naive/illusory-by-default), pooled at CAP=12' },
    { title: 'Sweep', detail: 'sequential atlas-order pass: each file reads finalized priors via the region ledger, routes altitude (no re-teach), removes duplicated snippet demonstrations' },
    { title: 'Reconcile', detail: 'union-find cluster cross-file residuals by shared file -> fix(max) -> adversarial verify(xhigh); hard residuals hand off to resolve-residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 12
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'docs/stacks/csharp'

// --- [MODELS] ----------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order'], properties: { path: { type: 'string' }, order: { type: 'integer' }, folder: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } } } } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'owned_regions'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['routed', 'clean'] }, rerouted: { type: 'array', items: { type: 'string' } }, owned_regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'TARGET: docs/stacks/csharp/ is the route-owned C# CODE DOCTRINE — a doc set of AGNOSTIC teaching pages (core + a domain/ shard set) that ' +
    'legislate how all project C# is written. A page teaches a coding LAW with one exemplary agnostic snippet, never a concrete module. The README ' +
    'owns routing + the 16 named laws + the COLLAPSE_SCAN + page-craft; the domain/ README is a one-table router whose shards compose the core ' +
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
].join('\n')
const CSDOCTRINE = [
  'HOLD the README [DOCTRINE] 17 laws as fact, never restated on a concept page: [FLOW] EXPRESSION_SPINE (domain logic expression-shaped; ' +
    'dependent steps `Bind`, independent ones accumulate applicatively; the carrier selects the algebra; statements only in measured ' +
    'ref-struct/span kernels that name the exemption) + BOUNDARY_ADMISSION (raw admitted EXACTLY ONCE into an evidence-carrying owner; interior ' +
    'never re-validates or sees null/sentinel/provider shape). [SHAPE] SHAPE_BUDGET + DEEP_SURFACES + MODAL_ARITY + ANTICIPATORY_COLLAPSE + ' +
    'INTERFACE_SEAM. ' +
    '[DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + ' +
    'DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION. Run the COLLAPSE_SCAN on every fence: any ' +
    'signal triggers the move, 3+ make it mandatory.',
  'A page that demonstrates a coding law must itself obey every law it can reach; a domain shard COMPOSES the finalized core laws as settled ' +
    'material and never re-opens admission/shape/rail/dispatch/boundary decisions.',
].join('\n')
const CS_SHAPE = [
  'EXTREME SHAPE/TYPE DENSITY: one concept owns exactly ONE type as a dense closed family chosen by OWNER_CHOOSER — `[ValueObject<TKey>]` ' +
    '(invariant-bearing scalar), `[ComplexValueObject]` (N-field product), `[SmartEnum<TKey>]` (wire-keyed vocabulary) / `[SmartEnum]` keyless ' +
    '(process-local behavior), `[Union]` (closed alternatives with per-occurrence payload) / `[Union<T1,...>]` ad-hoc, `record`/`readonly record ' +
    'struct` (interior product), a frozen set/table, or a language `enum` at the seam only. KILL parallel DTOs, one-field wrappers, field-rename ' +
    'shapes, nullable-as-failure, struct-`default` ghosts, and 3+ sibling types/factories/switch-arms for one concept (collapse to ONE generated ' +
    'owner or a `Fold` algebra / frozen data table).',
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
  'REJECT: a member-less marker read by `is`/reflection (admissible ONLY as a generic bound `where T : IMarker`; a capability switch is an attribute ' +
    'or a generated-owner conformance); an interface over a family WE close (forfeits `Switch` totality); a `[Union]` foreign code must extend; an ' +
    'INSTANCE default-interface-method (defaults derive via `static virtual` from a minimal core — zero admitted package ships an instance DIM); a ' +
    'generic-math constraint over a carrier stopping at `IComparable`/`IEquatable`/`IFormattable` (decorative — widen to the concrete carrier); a ' +
    'heap delegate on a measured hot path where a `ref struct` visitor (`allows ref struct`, caller-owned stack) fits; an invariant generic where ' +
    '`out`/`in` removes a cast; parallel `IFooForBar` names where the type parameter or keyed service carries the modality. This law lands in README ' +
    '[SHAPE] INTERFACE_SEAM, shapes.md OWNER_CHOOSER, surfaces-and-dispatch.md (static-abstract + visitor dispatch forms), and boundaries.md ' +
    '(hold-the-floor + lifetime contracts).',
].join('\n')
const CS_AOP = [
  'TWO-WEAVE AOP: definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) attach via attribute-directed SOURCE ' +
    'GENERATION in the fixed generator-owned order; composition-time concerns attach as effect transformers in author order — retry as ' +
    '`Schedule`-driven `IO<T>.Retry`/`Prelude.retry`, recovery as named catch combinators (`@catch`/`catchOf`/`CatchM` composed via `|`), resource ' +
    'lifetime as `Bracket`/`BracketIO`/`Finally`; the two weaves meet at EXACTLY ONE seam, the admission rail bridge. 2-4 co-occurring wrappers ' +
    'collapse into ONE aspect; an aspect NEVER raises into domain flow; inline-repeated concerns and sibling helper methods are defects.',
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
  'STACK CAPABILITY: C# has NO central .api tier — the universals are Thinktecture.Runtime.Extensions (generated domain shape) + LanguageExt.Core ' +
    '(rails, effects, schedules, immutable collections), layered onto the BCL and (for a domain shard) the host/NuGet surface its concern ' +
    'composes; MathNet/CSparse own numeric algorithms where relevant. Compose EVERY relevant member into single dense owners woven as ONE rail ' +
    '(source-generated owners, `Fold` algebra, data tables), ALWAYS layering Thinktecture/LanguageExt onto the domain surface, NOT flat one-shot ' +
    'per-API uses. Use the DEEPEST operator/combinator/generated surface each library reaches (LIBRARY_DEPTH); reject surface-level subsets, ' +
    'BCL-first reflexes, and thin rename wrappers. Cite ONLY host/NuGet members confirmed via `uv run python -m tools.assay api` — a member you ' +
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
const DOCTRINE = [LAW, '', ADVERSARIAL, '', CSDOCTRINE, '', CS_SHAPE, '', CS_INTERFACE, '', CS_AOP, '', CS_RAILS, '', CS_CORE_LOGIC, '', PARAM_POLY, '', CS14, '', CS_SUBSTRATE, '', PAGECRAFT, '', AGNOSTIC_SNIPPETS, '', OPINIONATED, '', STYLE_PROSE, '', COMMENTS].join('\n')

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
    'correct, total generated `Switch` (no silent `_`), no exception control flow; (6) C# 14/net10 + file-organization section-order; (7) ' +
    'Thinktecture/LanguageExt stacked to depth + every member verified (delete phantoms); (8) AGNOSTIC snippet law; (9) PAGE GRAMMAR + card ' +
    'economy + load-bearing reject columns; (10) ALTITUDE / no re-teach (a domain shard composes the core, never re-opens it); (11) ZERO META + ' +
    'style + comments; (12) CAPABILITY-COMPLETENESS + illusion + TABLE-STAKES — close an omitted capability in place with a cite, delete any ' +
    'table-stakes/decorative/speculative card. EDIT to fix every hit. Report `extended` and `regions`. Return residual_high {files:[...], claim}.'].join('\n')
const redteamPrompt = (page) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of ' + page + ' — the LAST and MOST AGGRESSIVE pass; red-team is critique AND MORE; the ' +
    'burden of proof is ON THE PAGE; trust nothing the prior passes claimed. Open Thinktecture/LanguageExt + the host/NuGet surface, the sibling ' +
    'pages, the README doctrine, the python rigor benchmark, the style-guide. Attack and REPAIR in place: (A) COUNTERFACTUAL on the core shape — ' +
    'does a denser generated owner / `Fold` algebra / data table / DEEPER LanguageExt-Thinktecture-MathNet primitive collapse the whole fence? ' +
    'rebuild to it. (B) ANTICIPATORY_COLLAPSE — does the next case/provider land as ONE generated case/row with consumers broken LOUDLY at compile ' +
    'time? reshape so the growth axis is a case/row/policy value. (C) CORPUS-WIDE DUPLICATION — route any snippet re-demonstrating a region a ' +
    'core/sibling/domain page owns. (D) AOP + SHAPE-BUDGET MAXIMIZATION — push more into source-gen aspects + effect transformers over a thinner ' +
    'pure core; collapse any loose cluster into one generated family. (E) STRATA + SUBSTRATE-DEPTH + PHANTOMS — flat code below the operator depth ' +
    'Thinktecture/LanguageExt reach (collapse to depth); a phantom member (delete it); a domain shard re-opening a core law (route it). (F) ' +
    'CAPABILITY-COMPLETENESS + ILLUSION + TABLE-STAKES — name an omitted capability with a cite and extend the owner in place; delete ' +
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
const inv = await agent('Read ' + ROOT + '/README.md and parse the [01]-[ATLAS] table, THEN the ' + ROOT + '/domain/README.md router. Return the ' +
  'full ordered file set: every CONCEPT page that exists on disk under ' + ROOT + ' (the core pages first in atlas order, then the domain/ shards ' +
  'in the domain router order), EXCLUDING every README.md and the entire .reports/ workspace. Each row {path (repo-relative), order (global ' +
  'integer), folder (`domain` or empty for core), regions (its current snippet-demonstration region tags)}. Use find/read; do not cd; do not edit ' +
  'anything.', { label: 'inventory', phase: 'Inventory', schema: INVENTORY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const ordered = ((inv && inv.files) || []).filter((f) => f && f.path && f.path.indexOf('/.reports/') < 0).sort((a, b) => a.order - b.order).map((f) => f.path)
log('Inventory: ' + ordered.length + ' csharp doctrine pages (core + domain) to harden')

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
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Harden+Sweep done; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([DOCTRINE, '', 'TASK: RECONCILE these cross-FILE residuals the harden + sweep passes deferred. NO severity — treat ' +
      'EVERY residual as must-address. Read EVERY listed file. For each real cross-file defect, FIX it in place (unify the shared generated ' +
      'owner/rail/region, repair the altitude/duplication/strata issue, or extend the shared owner to close a gap that spans files), preserving ' +
      'all capability; if a residual is FACTUALLY INCORRECT, leave it and say why. Edit ONLY under ' + ROOT + '/. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return null
    const verify = await agent([LAW, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and classify each ' +
      'residual: "fixed", "invalid" (cite why), or "open". Default to "open" on any doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the ' +
      'fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open hard residual(s) -> resolve-residuals')
return { workflow: 'stack-cs', root: ROOT, ordered: ordered, hardened: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: ordered.length, region_seed: ledger, clusters: clusters.length, hard_residual: hard_residual }
