export const meta = {
  name: 'stack-ts',
  whenToUse: 'Iterative adversarial hardening of the docs/stacks/typescript code doctrine; re-run until cold passes find nothing.',
  description: 'Durable adversarial HARDEN engine for the docs/stacks/typescript code doctrine. Every page is SUSPECT until it survives attack — naive, shallow, or illusory by default, rebuilt ground-up wherever the attack finds weakness — but the settled atlas roster is challenged with disqualifying evidence, never re-decided from zero. Phases: Inventory (real disk state + region ledger seed) -> Gate (1 agent: structure challenge — merge/split/kill/rename only on disqualifying evidence, applies structure only) -> Harden (per page: rebuild(max) -> critique(xhigh) -> redteam(max), all WRITE in place, pooled) -> Sweep (sequential atlas-order integration, zero duplicated snippet regions) -> Reconcile (residuals -> union-find clusters -> budgeted shards -> work-balanced buckets -> fix -> adversarial verify). SUPREMACY LAW: python and csharp stacks are BOTH the floor, never the ceiling — TS legislates past them wherever TS-native mechanisms admit stronger law. Every edit is scoped to docs/stacks/typescript. Takes no args.',
  phases: [
    { title: 'Inventory', detail: 'read-only recon: real page set in atlas order, per-page capability map + hostile weak/strong call, region ledger seed' },
    { title: 'Gate', detail: '1 agent: challenge the settled atlas structure with disqualifying evidence; apply approved renames/skeletons only, never content' },
    { title: 'Harden', detail: 'per page: rebuild(max, ground-up where attack finds weakness) -> critique(xhigh) -> redteam(max), every stage adversarial and writing' },
    { title: 'Sweep', detail: 'sequential atlas-order pass: each page integrates finalized priors implicitly, unified shape system, no duplicated snippet regions' },
    { title: 'Reconcile', detail: 'cross-file residuals: cluster -> shard -> balanced buckets -> fix(max) -> adversarial verify(xhigh); hard residuals hand off to resolve-residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 12
const STAGGER_MS = 1500
const STALL = 300000
const TARGET_WORK = 10
const ROOT = 'docs/stacks/typescript'

// --- [MODELS] ----------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order'], properties: { path: { type: 'string' }, order: { type: 'integer' }, verdict: { type: 'string' }, map: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } } } } } } }
const GATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'rationale'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order', 'charter'], properties: { path: { type: 'string' }, order: { type: 'integer' }, charter: { type: 'string' }, isNew: { type: 'boolean' } } } }, renames: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from', 'to'], properties: { from: { type: 'string' }, to: { type: 'string' } } } }, rationale: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'owned_regions'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['routed', 'clean'] }, rerouted: { type: 'array', items: { type: 'string' } }, owned_regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } }, repaired_files: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'TARGET: docs/stacks/typescript/ is the route-owned TypeScript CODE DOCTRINE — a doc set of AGNOSTIC teaching pages that legislate how all ' +
    'project TypeScript is written. It is NOT a libs/typescript/.planning design corpus: a page teaches a coding LAW with exemplary agnostic ' +
    'snippets, never a concrete module. The README owns routing + the doctrine laws + the COLLAPSE_SCAN; each concept page owns ONE disjoint ' +
    'layer and states doctrine as fact. READ docs/stacks/typescript/README.md sections [02], [03], [05], [06] and hold them as law.',
  'SUPREMACY LAW: docs/stacks/python/ and docs/stacks/csharp/ are BOTH the floor, never the ceiling — read both READ-ONLY for the shared shape ' +
    'laws (SHAPE_BUDGET/DEEP_SURFACES/MODAL_ARITY/ANTICIPATORY_COLLAPSE/POLICY_VALUES) and page craft, then legislate PAST them wherever a ' +
    'TS-native mechanism (structural typing, literal inference, the satisfies algebra, type-level derivation, declaration merging) admits a ' +
    'stronger law than either sibling can express. A TS page that merely matches the sibling bar where TS allows more is itself a finding. The ' +
    'spellings are TypeScript`s own — never a C#/LanguageExt or Python idiom transliterated.',
  'WRITE-FULLY MANDATE, scoped to docs/stacks/typescript/** ONLY: every defect you identify you FIX NOW via Edit/Write directly in the file; the ' +
    'fix-log you return is a REPORT of edits ALREADY MADE, never a ledger or a would/should hedge. Edit ONLY files under docs/stacks/typescript/; ' +
    'reading csharp/python/standards/.api files is allowed, editing anything outside docs/stacks/typescript/ is forbidden. Leave ' +
    'nothing behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage is HOSTILE: assume the page is NAIVE, SHALLOW, or ILLUSORY until it survives an aggressive attack; the burden ' +
    'of proof is ON THE PAGE. `finalized`, "mature", "already strong", "good enough", and a prior `clean` grade are REJECTED self-assessments. ' +
    'Where the attack finds weakness, rebuild that surface GROUND-UP to the strongest form the doctrine admits — never polish a weak base; a ' +
    'no-edit verdict is EARNED only by an attack that finds nothing, and never invent churn to force a verdict.',
  'ILLUSORY / FAKE content is the PRIMARY target: a snippet that READS dense yet demonstrates a THIN slice; prose that ASSERTS richness the fence ' +
    'lacks; a card field that decides nothing; a structurally-correct collapse that is semantically empty; a member cited but unverifiable (a ' +
    'PHANTOM — delete it). Treat confident-looking fences with MORE suspicion, and DISBELIEVE every claim the page makes about itself until verified.',
  'NAIVETY is a defect on TWO orthogonal axes, both intolerable: COVERAGE — the owner models a THIN SLICE of its concept (the obvious three ' +
    'fields where the domain carries fifteen; a two-case family for a twenty-case space); APPROACH — enumerated hardcoded instances where a ' +
    'parameterized, algorithmic owner should GENERATE the space (a fixed roster of styles, patterns, or variants is seed DATA feeding ONE ' +
    'generator over named parameters, never the mechanism itself).',
].join('\n')
const TS_DOCTRINE = [
  'HOLD the docs/stacks/typescript/README [02]-[DOCTRINE] laws + the [03]-[COLLAPSE_SCAN] signals as fact, never restated on a concept page. ' +
    'BOUNDARY_ADMISSION: raw is admitted EXACTLY ONCE through a Schema decode at the edge into an evidence-carrying owner; the interior never ' +
    're-validates, never sees `unknown`/`null`-as-failure/provider shape. Run the COLLAPSE_SCAN on every fence: any signal triggers the move, 3+ ' +
    'instances make it mandatory — and the signal list is a FLOOR, never the complete set: hunt collapse targets beyond it, in any repeated ' +
    'structure, parallel spelling, or enumerable family an algebra, table, fold, or generator can own.',
  'A page that demonstrates a coding law must itself obey every law it can reach — the doctrine pages are the reference implementation of the ' +
    'doctrine.',
].join('\n')
const TS_SHAPE = [
  'EXTREME SHAPE/TYPE DENSITY + ONE CANONICAL FORM (the central mandate): one concept owns exactly ONE canonical declaration — a `Schema` owner ' +
    '(class-based `Schema.Class`/`Schema.TaggedClass` or a schema value with `Schema.Schema.Type<typeof X>` derivation), a value-derived ' +
    'vocabulary table, or ONE exhaustive discriminated union — and every secondary surface DERIVES from it. SCHEMA AUTHORITY: the Schema owner ' +
    'is the single shape authority — static types derive (`Schema.Schema.Type`/`Schema.Schema.Encoded`, `X.Type` on class owners), wire twins ' +
    'derive through `Schema.transform`/encodings, NEVER a hand-declared parallel interface/DTO. ABSOLUTELY FORBIDDEN: a `const X` + `type X` + ' +
    '`typeof X` triple for ONE concept; loose `interface`/`type`-alias proliferation; `any`/`as any`/`as unknown as`; `enum` (a literal union or ' +
    'Schema literal); structural-duplicate shapes; tag-only wrappers; free-floating one-field brands (brands live INSIDE rich owners as Schema ' +
    'refinements).',
  'SHAPE BUDGET AT 20%: the doctrine drives generated code to roughly one fifth of the shape count of naive TS — one deep, rich, nested owner ' +
    '(embedded sub-schemas, class-carried methods and derivations, growth axes anticipated at 5x demand) replaces five or more loose interfaces, ' +
    'aliases, DTOs, and scattered brands. ANTICIPATORY_COLLAPSE: shape the owner for the family it WILL absorb so the next case/dimension/' +
    'modality lands as ONE declaration with every consumer broken loudly at type-check — one owner READY TO REPLACE 10+ loose things. ' +
    'Discriminated unions are EXHAUSTIVE (total dispatch via `Match.exhaustive` or a checked `never` sink, never a silent default). The ' +
    'exemplary snippet MUST show the owner at large-system scale with the growth axis visible.',
  'VALUE-DERIVED VOCABULARY: one `as const satisfies <Contract>` table is the single source of truth for a vocabulary — `satisfies` validates ' +
    'against the contract WITHOUT widening the inferred literal shape; `typeof`/`keyof typeof`/indexed-access derive the types FROM the value; ' +
    'mapped + template-literal + conditional types generate every secondary surface; a hand-written union, enum, or parallel constant a value ' +
    'table could derive is the named defect. ONE-NAME EXPORTS: a concept exports ONE name serving value and type (declaration merging, ' +
    '`Schema.Class` owners where the class IS both, const+type same-name pairing) so a consumer takes a single import and never aliases, ' +
    're-derives, or writes `typeof` at the call site.',
].join('\n')
const TS_EFFECT = [
  'EFFECT-TS IS THE RAIL (ULTRA-CRITICAL): domain logic is `Effect`-shaped — dependent steps compose with `Effect.gen`/`pipe`/`flatMap`, ' +
    'independent ones accumulate (`Effect.all` with the right concurrency/mode); ZERO `throw` in domain logic (failures are typed in the `Effect` ' +
    'error channel as a closed `Data.TaggedError` family, never a bare `Error` or a string); ZERO raw `Promise` in domain flow (lift at the ' +
    'boundary). Recovery is `Effect.catchTag`/`catchTags`, retry is `Effect.retry(Schedule)`, resource lifetime is ' +
    '`Effect.acquireRelease`/`Scope`.',
  'CROSS-CUTTING capability — retry, telemetry/spans, validation, contracts, caching, receipts — composes as Effect combinators and ' +
    '`Layer`/`Context` services woven over a THIN PURE CORE, never inline-repeated or hand-rolled; 2-4 co-occurring wrappers collapse into ONE ' +
    'combinator/Layer. BOUNDARIES are SCHEMA-FIRST: decode-don`t-validate with the `effect` `Schema` module at ingress and egress, refined owners ' +
    'carry the proof inward, and the same owner sources and sinks across consumers without interior edits.',
].join('\n')
const TS_CORE_LOGIC = [
  'WORLD-CLASS ALGORITHMIC BODIES: every body that does real work is expression-shaped — a naive imperative `for`/`while` with mutable accumulation ' +
    'or an intermediate array where a combinator pipeline or fold expresses it is a DEFECT. Compose the `ReadonlyArray`/`Array`/`Iterable` ' +
    'combinator surface at depth (`reduce`/`flatMap`/`map`/`filter`/`zip`/`groupBy`/`partition`/`scan` — the Effect `Array`/`Chunk`/`Stream` ' +
    'operators, never an ad-hoc loop), or an `Effect`/`Stream` pipeline for effectful traversal (`Effect.forEach`/`Effect.reduce`/`Stream.run*` at ' +
    'the right concurrency); exhaustive dispatch (`Match.exhaustive`, satisfies-checked handler records, or a checked `never` sink) replaces ' +
    'imperative branching; `const`-asserted readonly data + structural pattern dispatch replace mutable scans. NO mutable accumulation in domain ' +
    'flow, NO intermediate array a fold would fuse, NO imperative loop where a total combinator expresses it.',
].join('\n')
const PARAM_POLY = [
  'HEAVY PARAMETERIZATION, ZERO HARDCODING/FRAGILE LOGIC, FULL POLYMORPHISM. ONE entrypoint owns every modality — `T | Iterable<T>` (or the ' +
    'Schema-discriminated request) normalized ONCE at the head, discriminating on input SHAPE, never a name suffix or a `mode`/`batch`/`strict` ' +
    'boolean knob (KNOB_TEST: delete each parameter; if the value reconstructs what it carried, it was a knob to collapse). OVERLOAD COLLAPSE: ' +
    'one entrypoint owns every call modality through overload signatures or a discriminated input union; arity twins and suffix families are the ' +
    'named defect. DUAL ENTRIES: an operator serves pipe and direct call through `Function.dual` data-first/data-last signatures — one function, ' +
    'both call shapes, never a parallel pair.',
  'Configuration enters as ONE behavior-carrying value — a literal-union member, a tagged variant, a frozen policy record (POLICY_VALUES) — never ' +
    'a flag set the body re-derives. A `timeout`/`retry`/`deadline` is an Effect aspect/`Schedule`, never a signature param. Cases sharing ' +
    'generative structure are DERIVED from one primary record/table, never enumerated arms.',
].join('\n')
const TS_LANG = [
  'BLEEDING-EDGE TypeScript ONLY (latest stable): `satisfies`, `const` type parameters, `NoInfer`, instantiation expressions, variance ' +
    'annotations (`in`/`out`), template-literal types, `using`/`await using` for disposables, the `infer`/conditional/mapped-type toolkit at ' +
    'full power. INLINE COMPOSITION: wrapping, injection, and decoration attach at the owner declaration (pipe composition at definition, HOF ' +
    'wrapping inline, satisfies-checked handler/method records), never as loose intermediate consts, separate helper declarations, or ' +
    'consumer-side reassembly; const type parameters, `NoInfer`, and instantiation expressions pre-solve inference at the owner so consumers ' +
    'never re-instantiate generics or re-assert literals. `import type`/`export type` stay explicit; value vs type imports never blur. ZERO ' +
    '`any`, `as any`, `as unknown as`, `@ts-ignore`/`@ts-expect-error`-without-reason, `enum`, `namespace`-as-scope, or `throw` in domain logic; ' +
    '`unknown` only at a boundary immediately decoded by a Schema.',
  'NOMINAL invariants ride Schema-refined brands INSIDE rich owners, so an interior function cannot be handed a raw primitive — never a ' +
    'free-floating one-field brand alias. No `const`+`type`+`typeof` triple for one concept (ONE canonical owner, derive the rest). Keep every ' +
    'choice CONSISTENT across the whole corpus so it reads as ONE unified, ultra-advanced shape system, never a patchwork.',
].join('\n')
const TS_CITATION = [
  'CITATION / LIBRARY DEPTH: the TS substrate is the Effect ecosystem (`effect` — Schema/Match/Data/Stream/Layer included — plus `@effect/platform` ' +
    'and the admitted Effect packages) — mine each to its FULL advanced surface (the deep ' +
    'combinator/Schema/Layer/Stream/Schedule operators) and STACK them as ONE dense rail, never a flat one-shot per-API use or a `Promise`/' +
    'BCL-first reflex. Layer the Schema boundary + the Effect rail + the service `Layer`s together.',
  'ULTRA-STACKING: enumerate BOTH .api tiers IN FULL with a real listing — libs/typescript/.api/ (the substrate catalogs) and every ' +
    'libs/typescript/<folder>/.api/ — and mine them to OPERATOR DEPTH; an admitted capability the concept admits but no owner exploits is a ' +
    'DEFECT to close. Cite ONLY members that exist — verify novel members against the installed packages via `uv run python -m tools.assay api` ' +
    'over node_modules; a member you cannot verify is a PHANTOM to delete. Use the DEEPEST primitive each package reaches (LIBRARY_DEPTH); flat ' +
    'code below that operator depth is surface sprawl.',
].join('\n')
const PAGECRAFT = [
  'PAGE-CRAFT LAW (README [05]-[PAGE_CRAFT]): page grammar is a NARROW index table, then deep FAMILY CARDS, then the region snippet beside the ' +
    'rule it proves; the page ends at its last card. CARD ECONOMY: cards are few, deep, evidence-dense; near-peer cards MERGE until each owns a ' +
    'decision cluster; a card line carries exactly ONE decision; a `Use`/`Accept`/`Reject`/`Law`/`Boundary` field appears only where it decides ' +
    'something — a field that decides nothing is DELETED, not filled. Tables enumerate, cards legislate (rows stay atomic, no prose cramming, no ' +
    'links in cells).',
  'REJECT columns are LOAD-BEARING: every `Use` names the spelling, wrapper, or local pattern it DELETES (a junior TS reflex — `any`, an `enum`, a ' +
    'hand-thrown error, a const+type+typeof triple, a hand-declared wire twin — is exactly what a Reject names). CODE NAMES BEFORE PROSE: every ' +
    'member a card or snippet names is verified against the installed package before written; a nameable surface spelled as prose is a defect. ' +
    'ZERO META: no provenance, source trace, release narration, process state, or tool/skill context — any such block POISONS every downstream ' +
    'generation that loads the page.',
].join('\n')
const AGNOSTIC_SNIPPETS = [
  'AGNOSTIC SNIPPET LAW (style-guide [07]-[PLACEHOLDER_LAW]): every snippet COMPILES under the active TypeScript surface with legal NEUTRAL ' +
    'identifiers — `Shape`/`RefinedShape`/`Variant`/`PRIMARY`/`Field`/`KEY`/`Row`/`ROW_A`/`TABLE`/`SELECTED` — and placeholder strings ' +
    '(`"<value-a>"`) appear ONLY inside literals. NO project, repo, host, customer, pricing, deployment, or business-domain noun anchors a ' +
    'snippet; a domain noun is context poison.',
  'CORPUS-WIDE ZERO duplicated snippet demonstrations: each snippet exercises a surface region NO OTHER snippet in the corpus shows — the region ' +
    'is its spotlight; finalized surfaces composed as supporting material occupy no region and duplicate nothing. A duplicated region is repaired ' +
    'by ROUTING to its owner, never by re-teaching. Snippets are doctrine-exemplary at full operator depth, ~3-4x denser than ordinary code, at ' +
    'the scale a large system takes (admission + dispatch + rail + policy in one fence with the growth axis visible).',
].join('\n')
const OPINIONATED = [
  'HEAVILY-OPINIONATED PROJECT DOCTRINE, NOT a language survey. ZERO table-stakes is tolerated, ever: a card or snippet teaching something a ' +
    'competent TypeScript developer already knows — rather than an opinionated, dense, project-specific CHOICE — is a DEFECT to delete or densify. ' +
    'No net-casting to "cover the language"; cover only the opinionated decisions the projects need, each at 13/10.',
  'LOC budget ~450 is a SOFT pressure signal toward DENSIFICATION, NOT a hard gate. The real metric is per-card and per-snippet density: every ' +
    'card and every snippet world-class, zero filler. NEVER strip snippet whitespace, remove design content, or fragment a coherent concept to hit ' +
    'a number; a split is justified ONLY by concept disjointness, never by line count.',
].join('\n')
const STYLE_PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md: lead each section with the controlling rule/contract; one idea per paragraph; close on the ' +
    'consequence or boundary. Cut hedges (`may`/`might`/`probably`/`generally`/`where possible`/`if needed`), provenance, process narration, and ' +
    'report framing. Prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ' +
    'ASSERTS capability the fence lacks is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in a code span; name ' +
    'the exact member instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing. KEEP the canonical section-divider headers (`// --- [UPPERCASE_LABEL]` ' +
  'dash-fill). Beyond dividers, comment ONLY where intent is not already obvious from names, types, and signatures: default ZERO comments; at most ' +
  '1 line where a comment genuinely earns its place; 1-2 lines only for a truly subtle invariant or boundary. No narration, no restating the code, ' +
  'no TSDoc bloat, no task/process/review comments.'
const DOCTRINE = [LAW, '', ADVERSARIAL, '', TS_DOCTRINE, '', TS_SHAPE, '', TS_EFFECT, '', TS_CORE_LOGIC, '', PARAM_POLY, '', TS_LANG, '', TS_CITATION, '', PAGECRAFT, '', AGNOSTIC_SNIPPETS, '', OPINIONATED, '', STYLE_PROSE, '', COMMENTS].join('\n')

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
const archLine = (arch) => arch ? '\nSETTLED ATLAS (honor each page`s charter; an initial pointer, never a ceiling — re-read everything from ' +
  'disk, it never licenses a skim):\n' + JSON.stringify(arch, null, 1) : ''
const authorPrompt = (page, arch) => [DOCTRINE, '', 'TASK: ADVERSARIAL HARDEN-REBUILD of ' + page + ' to the ULTRA-DENSE TypeScript doctrine ' +
  'bar. Attack the page as naive/shallow/illusory; wherever the attack lands, rebuild that surface GROUND-UP to 13/10 — never polish a weak ' +
  'base. Read the page, the README atlas + doctrine, the sibling pages (cross-page unification), the python + csharp stacks (the FLOOR, ' +
  'READ-ONLY), the style-guide, and verify members against the Effect ecosystem (assay api over node_modules). Construct in ' +
  'BOUNDARY_ADMISSION lifecycle order (Schema decode at the edge); collapse parallel shapes into ONE canonical owner with every secondary ' +
  'surface DERIVED — and KILL every const+type+typeof triple, loose interface/type-alias spam, hand-declared wire twin, free-floating brand, ' +
  '`any`, `enum`, and hand-thrown error; weave cross-cutting concerns as Effect combinators/Layers over a thin pure core; parameterize fully; ' +
  'one polymorphic entrypoint per modality with dual data-first/data-last signatures where the operator warrants both. Make every snippet ' +
  'AGNOSTIC (neutral names), compiling, ~3-4x denser than ordinary code, one owner ready to replace 10+ loose things with the growth axis ' +
  'visible. Cut every table-stakes card/snippet. Apply page-craft + style/comment hygiene. Report `collapsed`, `extended`, and the page`s ' +
  'spotlight `regions`. Return residual_high {files:[...], claim}.' + archLine(arch)].join('\n')
const critiquePrompt = (page, arch) => [DOCTRINE, '',
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + FIX IN PLACE of ' + page + '. ULTRA-HARSH, UNAGREEABLE: assume a violation exists in EVERY fence; ' +
    'trust NOTHING the prose claims; "good enough" rejected. Read the page, the README doctrine, the sibling pages, the python + csharp stacks ' +
    '(the floor), the style-guide, and verify members via assay api. Run the MECHANICAL checklist LINE-BY-LINE and REPAIR every hit ' +
    'in place — every hit a fix, never a note; the checklist is a FLOOR, never the complete audit: hunt doctrinal defects past it:',
  '(1) COLLAPSE_SCAN signals (3+ mandatory): sibling names -> one polymorphic entrypoint; arity variants -> input-shape discrimination or ' +
    'overloads; literal-only differences -> a POLICY_VALUE; boolean selecting bodies -> derived/policy; one-hop function -> delete; parallel ' +
    'dispatch arms -> a satisfies-checked record/table or fold; types sharing fields -> one closed family; recurring wrappers -> one ' +
    'combinator/Layer — a FLOOR: hunt collapse targets beyond it. (2) ONE-CANONICAL-FORM + SCHEMA AUTHORITY -> replace any non-canonical owner; ' +
    'ELIMINATE every const+type+typeof triple (derive via `Schema.Schema.Type`/`typeof`), every hand-declared wire twin (derive via ' +
    '`Schema.transform`), loose interface/type-alias proliferation, tag-only wrappers, free-floating brands, and any hand-written union a value ' +
    'table could derive. (3) KNOB_TEST -> collapse boolean/mode/strict knobs to policy values or input-shape; move `timeout`/`retry`/`deadline` ' +
    'to an Effect aspect/`Schedule`.',
  '(4) EFFECT RAIL + SCHEMA BOUNDARY -> domain logic is `Effect`-shaped; ZERO `throw`/raw `Promise` in domain flow; failures are a closed ' +
    'tagged-error family in the error channel; recovery via `catchTag(s)`, retry via `Schedule`, resources via `acquireRelease`/`Scope`; ' +
    'boundaries decode-not-validate with the `effect` Schema module; cross-cutting concerns are combinators/Layers, never inline. (5) TYPES -> ' +
    'ZERO `any`/`as any`/`as unknown as`/`enum`/`namespace`-as-scope; `unknown` only at a Schema boundary; refined owners carry invariants ' +
    'inward; exhaustive dispatch via `Match.exhaustive`/checked `never` sink; inference pre-solved at the owner (const type params, `NoInfer`, ' +
    'instantiation expressions — a consumer forced to alias, re-instantiate, or `typeof` is a defect); `import type` discipline; latest TS only. ' +
    '(6) CITATION/DEPTH -> mine the Effect ecosystem to full depth and STACK it; every member verified (delete phantoms); no flat single-API use ' +
    'below the operator depth the packages reach.',
  '(7) AGNOSTIC snippet law -> compiles, neutral names, no business noun, large-system scale. (8) PAGE GRAMMAR + card economy + load-bearing ' +
    'reject columns (each `Use` names the junior reflex it deletes). (9) ALTITUDE / NO RE-TEACH -> route any mechanic a finalized prior page owns. ' +
    '(10) ZERO META + style + comments. (11) UNIFIED SHAPE SYSTEM -> this page`s shapes are CONSISTENT with the sibling pages (one corpus-wide ' +
    'shape vocabulary, not a patchwork). (12) CAPABILITY-COMPLETENESS + NAIVETY + ILLUSION + TABLE-STAKES -> close any capability the ' +
    'Effect surface or the real concept admits that the owner OMITS (case/row/field/operation) with a cite — COVERAGE naivety; rebuild any ' +
    'enumerated roster of hardcoded instances into ONE generator over named parameters with the roster as seed data — APPROACH naivety; delete ' +
    'any table-stakes/decorative/speculative card or snippet. EDIT to fix every hit. Report `extended` and `regions`. Return residual_high ' +
    '{files:[...], claim}.' + archLine(arch)].join('\n')
const redteamPrompt = (page, arch) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of ' + page + ' — the LAST and MOST AGGRESSIVE pass. Red-team is critique AND MORE; the ' +
    'burden of proof is ON THE PAGE; trust NOTHING the prior passes claimed. Open the Effect ecosystem, the sibling pages, the README doctrine, ' +
    'the python + csharp stacks (the floor), the style-guide. Attack and REPAIR in place — no soft-pedalling, a fix never a ledger:',
  'LENSES: (A) COUNTERFACTUAL on the core teaching shape — does a denser canonical owner (a richer Schema class family, a value-derived ' +
    'vocabulary table, an exhaustive DU) or a DEEPER Effect primitive collapse the whole fence? rebuild to it. (B) ANTICIPATORY_COLLAPSE — does ' +
    'the next case/variant land as ONE declaration with consumers broken loudly at type-check? reshape so the growth axis is a case/row/policy ' +
    'value. (C) CORPUS-WIDE UNIFICATION + DUPLICATION — is this page`s shape vocabulary IDENTICAL in spirit to every sibling (one unified system, ' +
    'zero patchwork), and does any snippet re-demonstrate a region a sibling owns (route it)? (D) SHAPE-BUDGET + EFFECT MAXIMIZATION — hunt EVERY ' +
    'residual loose interface/type-alias, every const+type+typeof triple, every hand-declared wire twin, every consumer-side re-derivation an ' +
    'owner should pre-solve, every `any`/`enum`/`throw`/raw `Promise`, every non-Schema boundary, every inline cross-cutting concern, and ' +
    'rebuild it to the canonical owner / Effect rail / Schema boundary; push more functionality into combinators/Layers over a thinner pure ' +
    'core. (E) DEPTH + PHANTOMS -> flat code below the Effect operator depth (collapse to package depth); a phantom member (delete it). (F) ' +
    'SUPREMACY — where a sibling-stack law has a TS-native stronger form (value derivation, inference pre-solving, declaration merging, dual ' +
    'signatures), the page must legislate the STRONGER form; matching the floor where TS allows more is a finding you fix. (G) ' +
    'CAPABILITY-COMPLETENESS + NAIVETY + LONG-TAIL + ILLUSION + TABLE-STAKES -> attack the long tail of the concept: the edge cases, failure ' +
    'modes, and family members a thin slice ignores; name an omitted capability with a cite and extend the owner in place (COVERAGE naivety); ' +
    'rebuild any enumerated roster of hardcoded instances into ONE generator over named parameters, roster as seed data (APPROACH naivety); ' +
    'delete table-stakes/decorative/speculative content.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW: re-attack every critique dimension with fresh hostile eyes. The page must end objectively denser, MORE ' +
    'capable, more agnostic-compliant, more bleeding-edge, and PART OF ONE UNIFIED SHAPE SYSTEM more than the critique left it; if the strongest ' +
    'form is genuinely present, prove it by finding nothing — never invent churn. Report `extended` and `regions`. Return residual_high ' +
    '{files:[...], claim}.' + archLine(arch)].join('\n')
const sweepPrompt = (page, ledger) => [DOCTRINE, '',
  'TASK: SEQUENTIAL CORPUS-INTEGRATION SWEEP + FIX IN PLACE of ' + page + '. The prior pages in README atlas order are now FINALIZED. Make this ' +
    'page integrate them IMPLICITLY, share ONE unified shape vocabulary with them, and own a disjoint region. Read THIS page from disk; you are ' +
    'given the REGION LEDGER of every prior finalized page as data (NOT bodies) — read a prior body ONLY when the ledger flags a candidate collision.',
  'ENFORCE: (1) ALTITUDE ROUTING — route any mechanic the ledger shows owned upstream (compose as settled supporting material; never re-teach a ' +
    'prior law). (2) NO DUPLICATE SNIPPET — demote any snippet exercising an owned region and pick an UNOWNED spotlight, or delete it. (3) UNIFIED ' +
    'SHAPE SYSTEM + DENSITY — the page`s canonical owners, Effect rails, and Schema boundaries are CONSISTENT with the finalized priors (one ' +
    'corpus, not a patchwork); end denser, fully agnostic, table-stakes-free, within the soft ~450 LOC density signal (never split a coherent ' +
    'concept or strip snippet whitespace).',
  'REGION LEDGER (finalized priors):\n' + JSON.stringify(ledger, null, 1) + '\nEDIT this page to fix every hit. Return UPDATED `owned_regions`, ' +
    '`rerouted`, verdict, and residual_high {files:[...], claim}.'].join('\n')
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (page, arch) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(page, arch), { label: st.key + ':' + nameOf(page), phase: 'Harden', schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: STALL })
    if (r === null) break
    logs[st.key] = r
  }
  return { page, logs, ok: Object.keys(logs).length === STAGES.length }
}
const regionsOf = (logs) => { for (const st of ['redteam', 'crit', 'rebuild']) { const l = logs[st]; if (l && l.regions && l.regions.length) return l.regions } return [] }
const clusterWork = (c) => { const files = new Set(); for (const r of c) for (const f of r.files) files.add(f); return files.size * 2 + c.length }
const shardOversized = (cs) => {
  const cap = Math.max(TARGET_WORK, Math.ceil(cs.reduce((w, c) => w + clusterWork(c), 0) / CAP))
  return cs.flatMap((c) => {
    if (clusterWork(c) <= cap) return [c]
    const byFile = new Map()
    for (const r of c) { const k = r.files[0] || '~'; if (!byFile.has(k)) byFile.set(k, []); byFile.get(k).push(r) }
    const shards = []
    for (const g of [...byFile.values()].sort((a, b) => clusterWork(b) - clusterWork(a))) {
      const t = shards.find((s) => clusterWork(s.concat(g)) <= cap)
      if (t) t.push(...g); else shards.push([...g])
    }
    return shards
  })
}
const packClusters = (clusters) => {
  const sorted = clusters.slice().sort((a, b) => clusterWork(b) - clusterWork(a))
  const total = sorted.reduce((s, c) => s + clusterWork(c), 0)
  const n = Math.max(1, Math.min(CAP, sorted.length, Math.ceil(total / TARGET_WORK)))
  const buckets = Array.from({ length: n }, () => ({ w: 0, rows: [] }))
  for (const cl of sorted) { const b = buckets.reduce((m, x) => (x.w < m.w ? x : m)); b.w += clusterWork(cl); b.rows.push(...cl) }
  return buckets.filter((b) => b.rows.length).map((b) => b.rows)
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Inventory')
const inv = await agent('TASK: DISCOVERY — the read-only reconnaissance grounding every downstream stage; read-only is its ONLY concession. ' +
  'Enumerate from the SOURCE OF TRUTH, never memory: fd/ls the real page set under ' + ROOT + ' and BOTH .api tiers (libs/typescript/.api/ and ' +
  'every libs/typescript/<folder>/.api/), and parse ' + ROOT + '/README.md [01]-[ATLAS]. Read every existing page IN FULL plus the README at ' +
  'large; resolve scope against real disk state. Return every CONCEPT page as a row {path (repo-relative, e.g. ' + ROOT + '/shapes.md), order ' +
  '(atlas position, integer)}, EXCLUDING README.md and any page that does not exist on disk. Per page the product is a MAP, not a verdict: ' +
  '`map` = composed capability + underutilized capability with CONCRETE members (verified against the .api catalogs/node_modules only — never ' +
  'list a phantom) + contextual seams to sibling pages + stacking guidance; `verdict` = the hostile weak/strong call (which surfaces look naive, ' +
  'thin, or illusory and why); `regions` = its current snippet-demonstration region tags. The map is an initial pointer, never a ceiling — ' +
  'downstream stages re-read and exceed it; it never licenses a skim. Use fd/find/read; do not cd; do not edit.', { label: 'inventory', phase: 'Inventory', schema: INVENTORY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const invFiles = ((inv && inv.files) || []).filter((f) => f && f.path).sort((a, b) => a.order - b.order)
log('Inventory: ' + invFiles.length + ' TS doctrine pages under ' + ROOT)

// --- [GATE]
phase('Gate')
const arch = await agent([DOCTRINE, '', 'TASK: ATLAS STRUCTURE CHALLENGE GATE (structure only — no content authoring). The atlas roster is ' +
  'SETTLED law; a structural change (merge, split, kill, rename, add) requires DISQUALIFYING evidence — a page provably thin/table-stakes with ' +
  'no densification path, two pages provably owning one layer, a genuinely disjoint uncovered layer — never taste. Read the README atlas + ' +
  'doctrine, every page from disk, and the inventory maps below. Default outcome: the standing roster unchanged. Where evidence disqualifies: ' +
  'apply ONLY the structure — `git mv` each rename; create a skeleton file (H1 `# [<TOKEN>]` + one-line lead + `## [1]-[INDEX]` stub) for each ' +
  'genuinely new page; update the README atlas table to match; author NO content (the Harden pass realizes content per charter). Edit ONLY ' +
  'under ' + ROOT + '/. Return the FINAL ordered file set {path, order, charter, isNew}, renames, rationale.\nINVENTORY:\n' +
  JSON.stringify(invFiles, null, 1)].join('\n'), { label: 'gate', phase: 'Gate', schema: GATE_SCHEMA, effort: 'max', stallMs: STALL })
const archFiles = ((arch && arch.files) || []).filter((f) => f && f.path).sort((a, b) => a.order - b.order)
const ordered = archFiles.length ? archFiles.map((f) => f.path) : invFiles.map((f) => f.path)
log('Gate: settled atlas = ' + ordered.length + ' pages' + (arch && arch.renames && arch.renames.length ? ' (' + arch.renames.length + ' structural changes)' : ' (roster unchanged)'))

// --- [HARDEN]
phase('Harden')
const done = (await pool(ordered, CAP, (page) => processPage(page, arch))).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const ledger = []
const sweepLogs = []
const seedRegions = (p) => { const d = done.find((r) => r.page === p); if (d) { const rr = regionsOf(d.logs); if (rr.length) return rr } const iv = invFiles.find((f) => f.path === p); return (iv && iv.regions) || [] }
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
  const anchored = uniq.filter((r) => r.files.length)
  for (const r of anchored) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of anchored) { const root = find(r.files[0]); if (!by.has(root)) by.set(root, []); by.get(root).push(r) }
  const out = [...by.values()]
  const loose = uniq.filter((r) => !r.files.length)
  if (loose.length) out.push(loose)
  return out
})()
const buckets = packClusters(shardOversized(clusters))
log('Harden+Sweep done; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters -> ' + buckets.length + ' balanced buckets [' + buckets.map((b) => b.length).join(', ') + ']')
let reconciled = []
if (buckets.length) {
  phase('Reconcile')
  reconciled = (await pool(buckets, CAP, async (cl, i) => {
    const fix = await agent([DOCTRINE, '', 'TASK: RECONCILE these cross-FILE residuals the per-page + sweep passes deferred. NO severity — treat ' +
      'EVERY residual as must-address. Read EVERY listed file. For each real cross-file defect, FIX it in place to the ROOT (unify the shared ' +
      'canonical owner/Schema/Effect rail/region across files, or repair the altitude/duplication issue; a token single-point patch where a ' +
      'root-level dense reconstruction of the same files is available is itself a defect), preserving all capability and keeping ONE unified ' +
      'shape system; if a residual is FACTUALLY INCORRECT, leave it and say why. ' +
  'A concurrent sibling may share a page with your bucket (oversized components shard file-atomically): edit any potentially shared page with ' +
  'surgical anchored Edits only — re-read and re-apply on an edit conflict, never a whole-file rewrite. ' +
  'Edit ONLY under ' + ROOT + '/. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return null
    const verify = await agent([DOCTRINE, '', 'TASK: ADVERSARIAL CRITIQUE-GRADE WRITING VERIFY — never a friendly confirmation. Per residual: ' +
      '(1) RE-DERIVE necessity — was the claimed fix needed at all, and is the claim itself sound? (2) PROVE ON DISK the fix landed properly — ' +
      'read every named file cold and attack the fix as naive, token, or illusory until what is actually on disk survives. (3) REPAIR loose, ' +
      'weak, or token fixes IN PLACE to the objectively-best root-level form of the same files at the FULL doctrine bar above — a single-point ' +
      'patch where a root-level dense reconstruction is available is itself a defect YOU repair now, preserving all capability and ONE unified ' +
      'shape system. THEN classify EVERY claim, echoing each claim string VERBATIM (a dropped or paraphrased claim cannot validate): "fixed" ' +
      '(proven on disk, your own repair included), "invalid" (cite why the claim is factually wrong), or "open" — ONLY for a claim genuinely ' +
      'unreachable from the files at hand, never to punt a strengthenable fix. List every file you edited in `repaired_files`. ' +
      'Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the ' +
      'fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
log('Reconcile: ' + buckets.length + ' buckets; ' + hard_residual.length + ' open hard residual(s) -> resolve-residuals')
return { workflow: 'stack-ts', root: ROOT, settled: arch, ordered: ordered, hardened: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: ordered.length, region_seed: ledger, buckets: buckets.length, hard_residual: hard_residual }
