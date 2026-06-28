export const meta = {
  name: 'ts-rebuild-single',
  whenToUse: 'Hostile ground-up rebuild of one TypeScript design-page folder to the Effect-TS doctrine bar.',
  description: 'Granular hostile ground-up rebuild of libs/typescript design pages to TRULY ultra-advanced TypeScript (Effect-TS rails, Schema-first boundaries, branded/nominal types, exhaustive discriminated unions, zero any/throw/enum) per docs/stacks/typescript/ + coding-ts, AND justified IN-PLACE capability extension. TARGETS are granular: an area root (libs/typescript/ui), one or more sub-folders at ANY depth, or specific files (any number) — passed as a string, an array, or {targets:[...]}. Per TARGETED design page, 1 agent per file in a 3-step ADVERSARIAL pipeline — rebuild(max) -> critique(xhigh) -> redteam(max), every stage hostile: assume the fence is naive/junior/illusory until it survives attack, never accept "mature", hunt the fake/decorative code that reads advanced but is hollow, collapse + stack both .api tiers, AND close the concept capability gaps by growing the existing owner in place. Then a FOLDER-WIDE reconcile: residual union-find fix/verify (blast radius = the owning folder, not just the targeted files) PLUS a per-area sibling-seam drift sweep so the whole folder stays coherent even where the targeted rebuild did not reach. The whole-folder collapse/unification pass is OWNED BY ts-rebuild-many (run it scoped to one area). args = a target path, an array of target paths, or {targets:[...]}; empty = no-op.',
  phases: [
    { title: 'Discover', detail: 'resolve the targets (file / sub-folder / area, any number) into the targeted page set + owning areas + the folder-wide page set' },
    { title: 'Rebuild', detail: 'per TARGETED page (1 agent/file): rebuild(max) -> critique(xhigh) -> redteam(max), every stage ADVERSARIAL (naive/illusory-by-default) + capability extension, pooled at CAP' },
    { title: 'Reconcile', detail: 'FOLDER-WIDE: union-find cluster cross-file residuals -> fix(max) -> adversarial verify(xhigh) with the owning folder as blast radius; then a per-area sibling-seam drift sweep so the whole folder stays coherent even outside the targeted set; hard residuals hand off to resolve-residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const ROOT = 'libs/typescript'

// --- [INPUTS] ----------------------------------------------------------------------------

const normTarget = (t) => { const s = String(t).trim().replace(/\/+$/, ''); return (s === ROOT || s.indexOf(ROOT + '/') === 0) ? s : ROOT + '/' + s.replace(/^\/+/, '') }
const rawTargets = Array.isArray(args) ? args
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets
  : (args && typeof args === 'object' && args.target) ? [args.target]
  : (typeof args === 'string' && args.trim()) ? [args]
  : []
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['packages', 'rebuildPages', 'folderPages'], properties: { packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'planning', 'api', 'root'], properties: { name: { type: 'string' }, planning: { type: 'string' }, api: { type: 'string' }, root: { type: 'string' }, note: { type: 'string' } } } }, rebuildPages: { type: 'array', items: { type: 'string' } }, folderPages: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SEAM_SCHEMA = { type: 'object', additionalProperties: false, required: ['package', 'verdict', 'summary'], properties: { package: { type: 'string' }, verdict: { type: 'string', enum: ['repaired', 'clean'] }, repaired: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/typescript planning corpus (markdown specs of intended TypeScript module designs). The current TypeScript code quality is ' +
    'POOR; this is a TRUE modernization to ultra-advanced TS, not a polish pass — discard naive idioms wholesale. CLAUDE.md manifest law governs. ' +
    'DENSITY BAR + STANDARD: docs/stacks/typescript/ (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, system-apis) ' +
    'and the coding-ts standard. Cite only real members of admitted packages — MINE BOTH catalog tiers: the shared/universal ' +
    '`libs/typescript/.api/*.md` (effect, effect-platform, effect-opentelemetry, effect-atom, react, react-dom, clsx) AND the area-specific ' +
    '`libs/typescript/<area>/.api/*.md`, cross-checked against the published types in node_modules. Maximize the shared/universal ' +
    'Effect/Schema/React rails wherever relevant, never only the area set.',
  'This is a FUNDAMENTAL GROUND-UP REBUILD of a planning-stage DESIGN PAGE. Improve the page objectively: collapse interfaces/types into unions, ' +
    'stack the Effect ecosystem, AND close the concept capability gaps.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items ' +
    '(report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the existing fence is NAIVE, SHALLOW, JUNIOR, or ILLUSORY ' +
    'until it survives an aggressive attack; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good enough", "done", ' +
    'and a prior `clean` verdict are REJECTED self-assessments — MOST of this corpus is naive JavaScript-in-TypeScript dressed in the right ' +
    'vocabulary, and it is NOT tolerable. Default to "this fence is naive and must be rebuilt to the strongest form the doctrine admits" and MAKE ' +
    'that rebuild; a no-edit verdict is reached ONLY after a genuinely aggressive attack on the real domain + the verified both-tier package ' +
    'surface (against published node_modules types) finds nothing — never a first-read concession, never to avoid work. Reject "good enough" ' +
    'categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code is the code that PRETENDS to be advanced: it uses the doctrine vocabulary ' +
    '(tagged unions, `Schema`, `Effect`/`Layer`, branded types), cites packages, reads dense and confident — yet is HOLLOW. Treat dense, ' +
    'confident-looking fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify it against ' +
    'the real domain and the published library types. HUNT: a name/signature/prose that PROMISES capability the body does not implement; a "rich" ' +
    'owner that is a thin slice of its concept (a 2-case union for a 20-case domain; the obvious 3 fields where the concept carries fifteen); ' +
    'decorative density and ceremony carrying no real capability; a placeholder/stub/sketch dressed as a finished design; prose that ASSERTS ' +
    'richness the fence does not contain; a structurally-correct collapse that is semantically empty; `any`/unsafe `as`/non-null `!` smuggled ' +
    'under a confident surface; a member cited but unverifiable against node_modules (a phantom). Every such illusion is a DEFECT to rebuild, not ' +
    'a feature to preserve; never invent churn to look busy either.',
].join('\n')
const ULTRA = [
  'ULTRA-ADVANCED REBUILD MANDATE: COLLAPSE >=3 parallel interfaces/types/classes modelling one concept into ONE polymorphic surface (tagged ' +
    'discriminated union + exhaustive match), never parallel names. AOP: cross-cutting concerns (retry, telemetry, validation, caching, receipts, ' +
    'fault rails) as Effect combinators/layers/decorators, not repeated inline. UNIFIED rails + UNIFIED pipelines + feature-arms-as-cases (never ' +
    'loose separate). Parameterize inputs AND outputs with generics at depth; no stringy/weak typing.',
  'STACK CAPABILITY: FIRST inventory the COMPLETE catalog set — BOTH the shared/universal `libs/typescript/.api/*.md` AND the area-specific ' +
    '`libs/typescript/<area>/.api/*.md` — then compose the admitted libraries into single dense rails. There is NO fixed library count: weave ' +
    'EVERY relevant library, ALWAYS layering the shared/universal Effect ecosystem (`Effect`/`Layer`/`Context`/`Schema`/`Stream`) end-to-end ON ' +
    'TOP OF the area-specific packages, NOT naive `Promise`/`try`-`catch` glue. Maximize the shared/universal rails, never only the area set. Use ' +
    'the MOST powerful combinators; reject surface-level single-feature uses.',
  'PRESERVE all intended capability (densify, never delete functionality). Where a page is already strong, refine; where it is flat/naive, rebuild ' +
    'ground-up. Never regress correctness or boundary law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — collapsing to tagged unions + Effect-stacking is NECESSARY but NOT SUFFICIENT. A ' +
    'page can be fully collapsed into one polymorphic surface and STILL be capability-thin: modeling a NAIVE, LIMITED slice of its domain concept ' +
    '— a flat id/member set where the concept owns geometry, metrics, attributes, topology, and operations; a 2-case discriminated union where the ' +
    'domain has twenty; an interface with the obvious 3 fields where the concept carries fifteen. Structural completeness and CAPABILITY ' +
    'completeness are ORTHOGONAL. A FULL rebuild ALSO closes the capability gap so the page OWNS ITS DOMAIN CONCEPT COMPLETELY. Capability grows ' +
    'sublinearly: every real missing concern lands as a CASE in the existing tagged discriminated union, a FIELD on the existing ' +
    '`Schema`/`Struct`/branded record, a member on the existing `Effect.Service`, a ROW in the existing `const`-union/table, or a POLICY value on ' +
    'the existing vocabulary — reshaping the owner as if it had always carried it; NEVER a parallel interface/type, a new file, a sibling shape, ' +
    'or flat appended code.',
  'GAP SOURCES (every extension MUST cite exactly one — justified, never speculative): (a) PACKAGE — a member the admitted package surface exposes ' +
    'that the concept ADMITS but the page IGNORES is a missing case in the owner law (BOTH tiers: the shared `libs/typescript/.api/` ' +
    'Effect/Schema/React rails AND the area domain packages, cross-checked against the published node_modules types; stacking that full surface IS ' +
    'new functionality woven into the owner, not naive Promise/try-catch glue). (b) DOMAIN — an attribute, metric, sub-kind, relationship, state, ' +
    'or operation the REAL concept demands but the page omits (a chart owns scale/axis/series/interaction/annotation families and ' +
    'zoom/brush/tooltip/legend operations, not two naive renders; a service owns retry/telemetry/validation/cache layers, not a bare fetch; a ' +
    'projection owns the full transform/diff/patch family the domain needs). (c) CONSUMER — a contract a sibling or downstream owner will require ' +
    'that has no composed spelling here yet (a need with no spelling marks a missing case: the law extends first, the feature lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + both-tier package surface is the real measure. A ' +
    'SMALL page modeling a rich concept is almost always under-built (give it the DEEPEST sweep), AND a LARGE, well-collapsed page can still be ' +
    'capability-SPARSE (an owner that renders membership but models none of the concept scale/interaction/annotation/analytics). Assess each owner ' +
    'against its domain independently of size and EXTEND every owner the concept under-realizes IN PLACE — integrated and unified into the one ' +
    'owner at full Effect/Schema depth, every new field/case/member composing the existing rails — never a new flat surface beside it.',
  'JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the concept is genuinely complete, prove it by adding nothing — never ' +
    'invent capability to look busy or pad with flat fields. Every added case/field/member/row is load-bearing, cites a package member / domain ' +
    'attribute / consumer contract, and composes the existing Effect/Schema rails; preserve ALL existing capability — extension only deepens, ' +
    'never regresses.',
].join('\n')
const PATLAW = [
  'TS PATTERN LAW (ultra-advanced ONLY; do not preserve the naive idioms of the existing code): ZERO `any`, zero implicit `any`, zero unsafe `as`, ' +
    'zero non-null `!`; model with branded/nominal types, exact discriminated unions with EXHAUSTIVE handling (`assertNever` on the default), ' +
    '`readonly`/`as const`, template-literal types, conditional/mapped types, and the `satisfies` operator. NO runtime `enum` — use `const` unions ' +
    'or `Schema`/Effect.',
  'Domain logic runs on typed-error rails — `Effect`/`Either`/`Option`, NEVER `throw` in domain code; boundaries validate through `Schema` (parse, ' +
    'never trust input). `import type`/`export type` are explicit; side-effect/value imports preserve runtime order. Per the ' +
    'docs/stacks/typescript file-organization overlay: `Effect.Service` owners are SERVICES, `Layer`/runtime wiring is COMPOSITION, runtime ' +
    'schemas/classes are MODELS, and catalog/registry rows stay after the owners they reference.',
  'Keep conventions IDENTICAL across every area so the corpus reads as one ultra-advanced codebase. One canonical semantic name per bounded ' +
    'concept; discriminate on input shape rather than proliferating `get`/`getMany`/`getById` names.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every area owner strictly in its lane; internal code uses canonical names and shapes with mapping (and ' +
  '`Schema` validation) only at the edge; do not trample a sibling owner while densifying; respect the dependency direction of the workspace.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
    'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ' +
    'ASSERTS capability the fence does not implement is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name ' +
    'the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design ' +
    'content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no TSDoc bloat. Densify names and types so comments are rarely needed; cut every low-value ' +
  'comment.'

// --- [OPERATIONS] ------------------------------------------------------------------------

const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()
const authorPrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ' +
  'HOSTILE GROUND-UP REBUILD of ' + page + ' to the ULTRA-ADVANCED bar AND to domain-complete capability. DISBELIEVE the page — assume every fence ' +
  'is naive JavaScript-in-TypeScript or illusory until proven world-class; do NOT polish what is there, REBUILD it to the strongest form the ' +
  'doctrine admits, and treat dense confident-looking code as a prime suspect for hollow/decorative complexity and smuggled `any`/`as`/`!`. Read ' +
  'the page, its sibling pages (for cross-page unification), the docs/stacks/typescript standards + coding-ts, and the `.api/` catalogs of BOTH ' +
  'tiers — the shared `libs/typescript/.api/` universal set AND the area `libs/typescript/<area>/.api/` set, not only the catalogs the page ' +
  'already cites — cross-checked against the published types of the libraries it composes (ADD any shared/universal Effect/Schema rail the page ' +
  'under-uses, and STACK them all; a member unverifiable against node_modules is a phantom to delete). Collapse shapes into tagged unions + ' +
  'exhaustive match, STACK Effect capability into unified rails, apply AOP via combinators/layers, parameterize I/O with generics, branded types ' +
  'and Schema boundaries. BEYOND collapse + `.api` maximization, CLOSE THE CONCEPT CAPABILITY GAPS so the page OWNS ITS DOMAIN CONCEPT COMPLETELY: ' +
  'run your OWN aggressive domain + package sweep, find where the owner models a NAIVE/thin slice of its concept, and extend the EXISTING owner in ' +
  'place (a case on the union / a field on the `Schema`/branded record / a member on the `Effect.Service`), each addition citing a package member ' +
  '/ domain attribute / consumer contract — never a parallel interface/type, a new file, or flat spam. High-signal prose all-backticked. ' +
  'Fix-in-place (read-then-rebuild, preserve capability). Report what you collapsed (count before->after) in `collapsed` and what capability you ' +
  'extended (each addition + its cited source) in `extended`; verdict is `rebuilt` unless the fence genuinely survived the hostile rebuild ' +
  'untouched. Return the fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans], claim} object for any ' +
  'CROSS-FILE item you surface but cannot fix from this one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const critiquePrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ' +
  'HOSTILE CRITIQUE + CAPABILITY-COMPLETENESS + FIX IN PLACE of ' + page + '. Is this TRULY ultra-advanced TS, or naive code in disguise? Assume ' +
  'the latter and trust NOTHING the prose claims. Push further: deeper union collapse + exhaustive match, more Effect-stacking (are BOTH the ' +
  'shared `libs/typescript/.api/` rails AND the area catalogs maximized — Layer/Schema/Stream woven, not a thin area-only subset or Promise ' +
  'glue?), branded types where primitives are overloaded, richer generic parameterization. CAPABILITY-COMPLETENESS + ILLUSION (structural collapse ' +
  'and capability completeness are ORTHOGONAL — a fully-collapsed owner can still model a NAIVE, LIMITED slice of its concept, and dense confident ' +
  'code is the prime suspect for hollowness): verify the body actually implements what the names/prose promise; any capability the both-tier ' +
  'admitted-package surface / the real domain concept / a consumer contract admits that the owner OMITS (a flat id/member set where the concept ' +
  'owns geometry/metrics/attributes/topology/operations; a 2-case union where the domain has twenty; the obvious 3 fields where the concept ' +
  'carries fifteen) is a DEFECT — close it in place now by growing the EXISTING owner (case/field/member), citing its source; conversely a ' +
  'speculative/padding field, decorative ceremony, or prose asserting capability the fence lacks is deleted. ENFORCE the TS pattern law (zero ' +
  '`any`/`throw`/`enum`, Schema boundaries, `import type`) and cross-area consistency per coding-ts + docs/stacks/typescript. ENFORCE prose + ' +
  'comment hygiene. EDIT the page. Report what you extended in `extended`. Return fix-log + residual_high — each a {files: [every repo-relative ' +
  'path the cross-file fix spans], claim} object for any CROSS-FILE item you cannot fix from this one file (NO severity; the reconcile phase fixes ' +
  'all of them).'].join('\n')
const redteamPrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ' +
  'ADVERSARIAL RED-TEAM + FIX IN PLACE of ' + page + '. You are a HOSTILE principal reviewer whose explicit goal is to REJECT this design as naive ' +
  'or illusory TypeScript. Assume it is junior, under-typed, hollow, or wrong until the page proves otherwise; the burden of proof is on the ' +
  'design; trust nothing the prior passes or the prose claimed. Read the docs/stacks/typescript standards + coding-ts and the published library ' +
  'types, then ATTACK relentlessly: `any`/implicit-any/unsafe `as`/non-null `!`; `throw` in domain logic instead of `Effect`/`Either` typed ' +
  'errors; non-exhaustive union handling (no `assertNever`); runtime `enum` misuse; missing branded/nominal types where primitives are overloaded; ' +
  'loose `import` where `import type` is required; mutable state where `readonly` fits; naive `Promise`/`async` glue where `Effect`/`Layer` ' +
  'belongs; using only a thin area-only subset while ignoring the shared/universal `libs/typescript/.api/` rails ' +
  '(effect/effect-platform/Schema/Layer/Stream) that should layer onto area packages; unvalidated boundary (no `Schema` parse); >=3 parallel ' +
  'interfaces/types NOT collapsed into a tagged union; structural drift from coding-ts or the file-organization overlay; prose bloat and ' +
  'un-backticked code; a member cited but unverifiable against node_modules (a phantom — delete it). ALSO — CAPABILITY-COMPLETENESS + ILLUSION ' +
  '(counterfactually attack the owner for DOMAIN-COMPLETENESS independently of how collapsed or confident it looks): does the both-tier ' +
  'admitted-package surface, the real-world concept, or a consumer contract admit a capability this owner still OMITS (a flat membership/id set ' +
  'where the concept owns geometry/metrics/attributes/topology/operations; a 2-case union where the domain has twenty; the obvious 3 fields where ' +
  'the concept carries fifteen; a name/prose promising capability the body lacks)? Name it with a cite and EXTEND THE OWNER IN PLACE (a ' +
  'case/field/member) — a structurally-perfect but capability-sparse or illusory owner is a DEFECT, not a finished page; conversely reject any ' +
  'flat-spam/speculative/parallel-surface extension. For EVERY weakness, state the concrete failure (what breaks, under which input/output/edge ' +
  'case, and why) and THEN repair it in place — no soft-pedalling, no could/should. Reject "good enough"; force the objectively densest, most ' +
  'type-safe, MOST CAPABLE form, and run COUNTERFACTUALS. Hold the highest bar of any stage. EDIT to repair every defect you raise. Report what ' +
  'you extended in `extended`. Return fix-log + residual_high — each item a {files: [every repo-relative path the cross-file fix spans], claim} ' +
  'object for a CROSS-FILE item you cannot fix from one file (NO severity — every finding counts equally and the reconcile phase addresses all; ' +
  'every single-file fix you make yourself).'].join('\n')
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (w, tag) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(w.page), { label: st.key + ':' + folderOf(w.page) + ':' + subOf(w.page), phase: tag + folderOf(w.page), schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: 300000 })
    if (r === null) break
    logs[st.key] = r
  }
  return { page: w.page, logs, ok: Object.keys(logs).length === STAGES.length }
}
const seamPrompt = (pkg, rebuilt) => [LAW, '', BOUNDARIES, '', 'TASK: FOLDER-WIDE SEAM CHECK of area `' + pkg.name + '` after a TARGETED per-file ' +
  'rebuild touched ONLY these pages:\n' + JSON.stringify(rebuilt, null, 1) + '\nThe owning folder is `' + pkg.planning + '/**`. Read each rebuilt page; ' +
  'for every shape/owner/rail/seam/Schema it changed, find the SIBLING pages in the SAME area (OUTSIDE the targeted set) that consume it (via `import ' +
  'type`) and that the rebuild left STALE, read ONLY those affected siblings, and FIX the drift in place so the area stays coherent (a ' +
  'renamed/reshaped owner, a changed `Schema`, a moved capability, a stale seam). Do NOT rebuild the siblings — repair ONLY the seam the targeted ' +
  'rebuild disturbed, preserving all capability and regressing nothing; respect the dependency direction and never reach into a sibling owner ' +
  'interior. Edit in place. Return verdict (`repaired` if you changed any sibling, else `clean`), repaired (each sibling repo-relative path you ' +
  'fixed), and summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

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

phase('Discover')
const inv = await agent('Resolve these rebuild TARGETS into the page set + owning areas for ' + ROOT + '. Each TARGET (repo-relative) is an AREA ' +
  'root (e.g. ' + ROOT + '/ui), a SUB-FOLDER under .planning at ANY depth (e.g. ' + ROOT + '/ui/.planning/<sub> or a deeper nested sub-folder), or a ' +
  'specific design FILE (e.g. ' + ROOT + '/ui/.planning/<sub>/<page>.md). TARGETS:\n' + JSON.stringify(TARGETS, null, 1) + '\nThe OWNING AREA of a ' +
  'target is the path BEFORE "/.planning/", or the target itself when it has no "/.planning/" segment (an area root). Use find; do not cd; do not ' +
  'edit anything. Return: (1) packages — one entry per DISTINCT owning area: {name: the LAST path segment of the area root, planning: ' +
  '"<area>/.planning", api: "<area>/.api", root: "<area>"}. (2) rebuildPages — the TARGETED page subset (repo-relative *.md): an AREA-root target ' +
  'expands to EVERY page under "<area>/.planning/**"; a SUB-FOLDER target to EVERY page under that sub-folder at ANY depth; a FILE target to itself; ' +
  'union all targets and dedup. (3) folderPages — EVERY design page under EVERY owning area "<area>/.planning/**" (the reconcile blast radius). ' +
  'Exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md from BOTH.', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const packages = ((inv && inv.packages) || []).filter((p) => p && p.name)
const rebuildPages = [...new Set(((inv && inv.rebuildPages) || []).filter(Boolean))]
const folderPages = [...new Set(((inv && inv.folderPages) || []).filter(Boolean))]
const pending = rebuildPages.map((p) => ({ page: p }))
const total = pending.length
log('Discover: ' + total + ' targeted page(s) across ' + packages.length + ' area(s) [' + packages.map((p) => p.name).join(', ') + ']; folder-wide ' +
  'set ' + folderPages.length + '; pooling at CAP=' + CAP)
if (!total) { log('No targets resolved — pass a file, sub-folder, or area path as args (a string, an array, or {targets:[...]})'); return { root: ROOT, targets: TARGETS, total: 0, packages: packages.map((p) => p.name) } }

// --- [REBUILD]

phase('Rebuild')
const done = (await pool(pending, CAP, (w) => processPage(w, 'Rebuild-'))).filter(Boolean)

// --- [RECONCILE]

const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['rebuild', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Rebuild: ' + done.length + '/' + total + ' pages; reconcile ' + uniq.length + ' residuals (crit+redteam, deduped) -> ' + clusters.length + ' ' +
  'clusters')
phase('Reconcile')
let reconciled = []
if (clusters.length) {
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', 'TASK: RECONCILE these cross-FILE residuals the ' +
      'critique AND red-team passes deferred. There is NO severity — treat EVERY residual as must-address. Your blast radius is the OWNING FOLDER(S): ' +
      'you MAY read and fix ANY sibling page under them to keep seams consistent with the rebuilt pages, not only the listed files. Read EVERY listed ' +
      'file. For each: if it is a real cross-file defect, FIX it in place (unify the shared type/seam/rail, repair the strata/boundary issue, or ' +
      'extend the shared owner in place to close a capability gap that spans files), preserving all capability and regressing no file; if a residual ' +
      'is FACTUALLY INCORRECT or not a real defect, leave it and say why in the summary — never silently skip a real one to avoid work. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', BOUNDARIES, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and ' +
      'classify each residual: status "fixed" (real defect, now genuinely resolved), "invalid" (the claim is factually wrong / not a real defect — ' +
      'cite why), or "open" (real defect still NOT resolved). Default to "open" on any doubt for a real-looking defect; mark "invalid" ONLY when ' +
      'you can show the claim is wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
const seamTargets = packages.map((pkg) => { const rebuilt = rebuildPages.filter((p) => p.indexOf(pkg.root + '/') === 0); const fall = folderPages.filter((p) => p.indexOf(pkg.root + '/') === 0); return { pkg, rebuilt, hasSiblings: rebuilt.length > 0 && rebuilt.length < fall.length } }).filter((x) => x.hasSiblings)
const seamResults = (await pool(seamTargets, CAP, (x) => agent(seamPrompt(x.pkg, x.rebuilt), { label: 'seam:' + x.pkg.name, phase: 'Reconcile', schema: SEAM_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
const seamRepaired = seamResults.flatMap((s) => (s && s.repaired) || [])
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual -> resolve-residuals), ' + dropped.length + ' ' +
  'dropped as invalid; folder-seam sweep repaired ' + seamRepaired.length + ' sibling(s)')

return { root: ROOT, targets: TARGETS, packages: packages.map((p) => p.name), complete: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: total, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped, seamRepaired: seamRepaired }
