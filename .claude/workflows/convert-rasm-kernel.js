export const meta = {
  name: 'convert-rasm-kernel',
  description: 'Convert the Rasm kernel to a fully planning-scoped package: inventory the mature Vectors/Domain source into capability ledgers (+ ecosystem research), decide the new architecture (one .planning root with Vectors/Geometry/Domain subject folders, the Rhino boundary ruling), author the new design pages ground-up at the docs/stacks/csharp bar with zero capability loss, one critique-grade review pass, execute the restructure (move Geometry planning, delete Analysis and the mature source after backup check), sweep the durable-doc ripples, and terminally verify capability coverage. About 20 agents, peak concurrency 5. Ephemeral: delete after the conversion lands.',
  whenToUse: 'One-shot kernel conversion. Launch only after no other run is reading libs/csharp/Rasm paths; the .archive/Rasm backup must exist.',
  phases: [
    { title: 'Inventory', detail: '4 capability-ledger agents (Vectors halves, Domain+Analysis, context/boundary) + 1 ecosystem research lane, parallel' },
    { title: 'Architect', detail: '1 agent decides the new .planning structure, page-set, Rhino boundary, Domain rulings, roster additions' },
    { title: 'Author', detail: 'pool: new design pages written ground-up into the new .planning home from ledgers + blueprint' },
    { title: 'Review', detail: 'one critique-grade writing pass per page batch: fix in place + ledger-coverage check' },
    { title: 'Restructure', detail: '1 agent: move Geometry planning under the new root, delete Analysis + mature source (backup-gated), align the csproj' },
    { title: 'Ripple', detail: '3 agents: kernel index docs rewritten; repo-level durable docs swept; workflow scripts cleansed of the kernel special-casing' },
    { title: 'Verify', detail: '1 terminal agent: capability reconciliation, move/delete audit, residue sweep' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const ROOT = 'libs/csharp/Rasm'
const NEW_PLAN = ROOT + '/.planning'
const SCRATCH = '.claude/scratch/convert-rasm'

// --- [MODELS] ----------------------------------------------------------------------------
const LEDGER_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'capability_count'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, capability_count: { type: 'number' },
  rhino_facts: { type: 'array', items: { type: 'string' } }, key_findings: { type: 'array', items: { type: 'string' } } } }
const BLUEPRINT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'pages', 'rhino_ruling'], properties: {
  blueprint: { type: 'string' }, rhino_ruling: { type: 'string' },
  pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'charter'], properties: { page: { type: 'string' }, charter: { type: 'string' }, sources: { type: 'array', items: { type: 'string' } } } } },
  moves: { type: 'array', items: { type: 'string' } }, roster: { type: 'array', items: { type: 'string' } } } }
const BUILT_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages', 'summary'], properties: {
  pages: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' }, uncovered: { type: 'array', items: { type: 'string' } } } }
const REVIEW_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'fixed', 'coverage_gaps'], properties: {
  files: { type: 'array', items: { type: 'string' } }, fixed: { type: 'array', items: { type: 'string' } },
  coverage_gaps: { type: 'array', items: { type: 'string' } } } }
const EXEC_SCHEMA = { type: 'object', additionalProperties: false, required: ['moved', 'deleted', 'kept', 'summary'], properties: {
  moved: { type: 'array', items: { type: 'string' } }, deleted: { type: 'array', items: { type: 'string' } },
  kept: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'reason'], properties: { path: { type: 'string' }, reason: { type: 'string' } } } }, summary: { type: 'string' } } }
const RIPPLE_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' }, residue: { type: 'array', items: { type: 'string' } } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['coverage', 'residuals'], properties: {
  coverage: { type: 'string' }, repaired: { type: 'array', items: { type: 'string' } },
  residuals: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const PRE = 'Rasm monorepo. The kernel ' + ROOT + ' converts from mature C# source to a fully planning-scoped package: design pages under ONE ' +
  NEW_PLAN + '/ root with subject folders (Vectors, Geometry, Domain), the mature source retired after its capability is fully re-expressed. ' +
  'The mature code (13 Vectors files ~13.8k LOC, 4 Domain files, 14 Analysis files) predates docs/stacks/csharp/ — its FUNCTIONALITY is the ' +
  'asset, its DESIGN is not: bloated surfaces, poor entry points, weak strata consideration. New pages are written GROUND-UP at the full ' +
  'docs/stacks/csharp/ bar (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis + relevant ' +
  'domain shards) — never tethered to the old file structure, never losing a capability. ZERO capability loss is the campaign gate: every ' +
  'public behavior in the ledgers lands in a page fence or is dropped with an explicit reason (true duplicate, dead code, Analysis-retired). ' +
  'A full backup exists at .archive/Rasm — verify it before any delete. The WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM ' +
  '-> HOST-BOUNDARY -> APP): the kernel is host-neutral by default and meets Rhino at an explicit boundary — whether any Rhino surface remains ' +
  'in-kernel is a ruling the blueprint makes from ledger evidence, never an inherited assumption. Rasm.Rhino/Rasm.Grasshopper are untouched.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const ledgerPrompt = (lane, scope, focus) => [PRE, 'TASK: CAPABILITY LEDGER, lane = ' + lane + ' (read-only outside ' + SCRATCH + '/). Scope: ' +
  scope + '. ' + focus + ' Read every file FULLY. WRITE the ledger to ' + SCRATCH + '/ledger-' + lane + '.md: one row per capability unit ' +
  '(public type/member family, algorithm, behavior) — {unit, file:line, what it does, quality verdict 1-10, bloat/duplication notes, Rhino ' +
  'coupling (exact members), where it SHOULD live (vectors/geometry/domain/drop+reason)}. End with cross-cutting findings: entry-point rot, ' +
  'parallel rails, hardcoding, strata violations, and the 5-10 strongest structural rulings the architect must make. Dense, evidence-first. ' +
  'Return the pointer + counts + rhino facts.'].join('\n')
const ecosystemPrompt = () => [PRE, 'TASK: ECOSYSTEM RESEARCH (read-only outside ' + SCRATCH + '/). The new Vectors/Domain planning corpus may ' +
  'admit modern packages the mature code hand-rolled around. Inventory the current kernel roster (Rasm.csproj + Directory.Packages.props + ' +
  'libs/csharp/.api/ + ' + ROOT + '/.api/), then research the .NET ecosystem for the kernel domains the ledgers will cover: SIMD/vectorized ' +
  'math, tensor/array substrates, computational-geometry primitives, spatial indexing, robust predicates, mesh/half-edge structures, numerics. ' +
  'License gate: OSS or free-for-OSS commercial, never pay-tiered; every candidate verified real, maintained, .NET-current via the nuget MCP + ' +
  'two corroborating web sources. WRITE ' + SCRATCH + '/ledger-ecosystem.md: candidate table ranked categorical-best per concern (keep/add/' +
  'replace vs the current roster, license, binding surface). Return the pointer + counts.'].join('\n')
const architectPrompt = (dossiers) => [PRE, 'TASK: ARCHITECT — the one structure decision. Read every ledger: ' + JSON.stringify(dossiers) +
  ', the existing ' + ROOT + '/Geometry/.planning/ page set (18 pages — they RE-HOME under ' + NEW_PLAN + '/Geometry/ unchanged unless a ' +
  'ledger row proves a boundary move), the kernel index docs, and libs/.planning/architecture.md. DECIDE and WRITE ' + SCRATCH +
  '/blueprint.md: (1) the full new page-set under ' + NEW_PLAN + '/{Vectors,Geometry,Domain}/ — one page per real owner (folder-architecture ' +
  'law: no one-file folders, no flat sprawl, growth-conducive domains), each with a charter + the ledger rows it absorbs; (2) the RHINO ' +
  'BOUNDARY RULING from ledger evidence — which capabilities are host-neutral kernel, which are Rhino-boundary surfaces, and where the ' +
  'boundary page lives (the kernel stays one geometry owner per runtime meeting at the wire); (3) Domain rulings — Domain/Geometry.cs is ' +
  'scrutinized per ledger verdicts (carry forward ONLY what earns it, absorb the rest), stats/context improve to feed the new ' +
  'vectors/geometry owners; (4) Analysis/ confirmed DELETE (capability notes only where a ledger row proves something load-bearing hides ' +
  'there); (5) roster additions/removals from the ecosystem ledger; (6) the move/delete manifest (exact paths). Every ledger capability row ' +
  'maps to a page or an explicit drop — an unmapped row is a blueprint defect. Return the pointer + page list + the rhino ruling ' +
  'one-liner.'].join('\n')
const authorPrompt = (batch, blueprint, dossiers) => [PRE, 'TASK: AUTHOR ground-up. Write these design pages per the blueprint ' + blueprint +
  ' (read it + every ledger it cites: ' + JSON.stringify(dossiers) + '): ' + JSON.stringify(batch) + '. Each page: full docs/stacks/csharp ' +
  'bar, section grammar per the corpus convention, dense code fences that RE-EXPRESS the ledger capabilities through the strongest design ' +
  '(polymorphic dispatch surfaces, typed rails, generated vocabularies, table-driven variation) — never a transliteration of the old code; ' +
  'the old file structure is DEAD, only its capability survives. Stack BOTH .api tiers (libs/csharp/.api/ + ' + ROOT + '/.api/) and the ' +
  'blueprint roster; verify novel members via uv run python -m tools.assay api where the assembly is restored, and mark genuinely new ' +
  'package members as the blueprint roster admits them. Cover every ledger row the blueprint maps to your pages; list any you could not ' +
  'cover in `uncovered` — never silently drop. Write INTO the new home paths exactly as the blueprint names them.'].join('\n')
const reviewPrompt = (batch, blueprint, dossiers) => [PRE, 'TASK: CRITIQUE-GRADE WRITING REVIEW (one pass, fix in place — no further rounds). ' +
  'Pages: ' + JSON.stringify(batch) + '. Hold each to the docs/stacks/csharp bar with the hostile stance: naive/thin/enumerated-where-a-' +
  'generator-belongs fences are REBUILT in place, entry-point sprawl collapses, phantom members die (verify via assay api where restorable). ' +
  'Then the coverage audit: against the blueprint ' + blueprint + ' and the ledgers ' + JSON.stringify(dossiers) + ', confirm every ledger ' +
  'row mapped to these pages is actually expressed — repair gaps NOW; report only what you cannot reach in coverage_gaps.'].join('\n')
const execPrompt = (blueprint) => [PRE, 'TASK: RESTRUCTURE EXECUTION per the blueprint ' + blueprint + ' move/delete manifest. ORDER: (1) ' +
  'verify .archive/Rasm holds the mature source (Vectors/, Domain/, Analysis/) — a missing backup ABORTS every delete (report in kept); (2) ' +
  'git mv ' + ROOT + '/Geometry/.planning/* to ' + NEW_PLAN + '/Geometry/ (preserve subfolders), then remove the emptied Geometry/ tree; (3) ' +
  'delete ' + ROOT + '/Analysis/, ' + ROOT + '/Vectors/, and the mature ' + ROOT + '/Domain/*.cs the blueprint retires (git rm); (4) align ' +
  ROOT + '/Rasm.csproj to the planning-folder convention (label-grouped, references matching the blueprint roster + rhino ruling — a ' +
  'retired-source package reference is pruned; hand-edit, never dotnet add) and clean stray bin/obj entries from git if tracked. Use git ' +
  'commands for all moves/deletes so history survives. Report every path moved/deleted/kept-with-reason.'].join('\n')
const rippleKernelPrompt = (blueprint) => [PRE, 'TASK: KERNEL INDEX DOCS rewrite per the blueprint ' + blueprint + '. Rewrite ' + ROOT +
  '/README.md, ARCHITECTURE.md, TASKLOG.md, IDEAS.md to the converted reality: one ' + NEW_PLAN + '/ root, the new page routers, the ' +
  'codemap, the rhino ruling, the roster — the mature/greenfield split is GONE from every sentence; TASKLOG carries the conversion as a ' +
  'closed card and re-scopes open cards to the new pages; IDEAS entries re-anchor or drop. Durable-doc law: declarative, no provenance, no ' +
  'narration.'].join('\n')
const rippleRepoPrompt = (blueprint) => [PRE, 'TASK: REPO-LEVEL DURABLE-DOC SWEEP per the blueprint ' + blueprint + '. Sweep and fix every ' +
  'mention of the old kernel shape: rg for mature|greenfield|Geometry/.planning|Analysis/|Vectors/ across libs/csharp/.planning/README.md ' +
  '(the [01] router kernel line), libs/.planning/architecture.md (kernel rows + any homing prose), libs/.planning/planning-targets.md, root ' +
  'README.md, CLAUDE.md, AGENTS.md, libs/csharp/.planning/component-system.md, and any RASM-CS-GEOMETRY-BRIEF.md (re-path its ' +
  'Geometry/.planning references to ' + NEW_PLAN + '/Geometry/; ALSO re-anchor its Vectors/Domain SOURCE references — Field.cs/solver .cs ' +
  'file:line anchors and the assay-over-restored-Rasm.Vectors instruction — onto the NEW planning pages that carry the same capability: ' +
  'capability-level anchors where line-level cannot map, and member verification re-routed to the new page fences as the source of truth). ' +
  'Fix each mention to the converted reality — surgical, never rewriting sections the conversion does not touch. Do NOT edit ' +
  '.claude/workflows/ scripts (a sibling agent owns them) or memory files; list any memory mention in residue instead. Report every file ' +
  'touched + residue.'].join('\n')
const rippleWorkflowsPrompt = (blueprint) => [PRE, 'TASK: WORKFLOW-SCRIPT SWEEP per the blueprint ' + blueprint + '. The durable workflow ' +
  'scripts carry kernel special-casing that dies with the conversion: rg -i for homing|Geometry/.planning|Analysis/Domain/Vectors|mature ' +
  'across .claude/workflows/*.js EXCLUDING convert-rasm-kernel.js (its old-path references are its own subject). Known carriers: rebuild.js ' +
  '(the cs L.homing block — the Geometry planning-homing special case and the never-touch-Analysis/Domain/Vectors law), survey-gaps.js, ' +
  'tidy-planning-docs.js, hygiene-sweep.js, align-cards.js, ideate.js, survey-packages.js, stack-cs/py/ts.js, resolve-residuals.js. Fix each ' +
  'to the converted reality: kernel planning lives at ' + NEW_PLAN + ' exactly like every other package (no homing special case, no ' +
  'off-limits siblings, no mature/greenfield split — the kernel is one ordinary planning-scoped target). RESUME-SAFETY LAW: prompt text is a ' +
  'resume cache key, so edits are VALUE-NECESSARY ONLY — change the stale kernel facts in place, never restructure a prompt, never touch ' +
  'schemas/labels/models/meta blocks, never reflow untouched lines. After each edited file, parse-check it: node -e with the async-function ' +
  'constructor over the file body (replace the leading export const meta = with const meta =) and repair any syntax error you introduced. ' +
  'Report every file touched + residue.'].join('\n')
const verifyPrompt = (blueprint, dossiers, built, reviewed) => [PRE, 'TASK: TERMINAL VERIFY — adversarial, WRITING (repair what you can reach ' +
  'NOW; no further rounds). (1) CAPABILITY RECONCILIATION: walk every ledger (' + JSON.stringify(dossiers) + ') row against the landed pages ' +
  '(' + JSON.stringify(built) + ') and the blueprint drop list — every row is expressed or explicitly dropped with a standing reason; repair ' +
  'expressible gaps in place, report unreachable ones as residuals. (2) STRUCTURE AUDIT: ' + NEW_PLAN + '/ holds Vectors/Geometry/Domain; no ' +
  ROOT + '/Geometry/.planning, /Analysis, /Vectors source remains; the csproj matches the ruling. (3) RESIDUE SWEEP: rg the durable docs for ' +
  'mature/greenfield/old-path residue the ripple missed and fix it. (4) Review leftovers: ' + reviewed + ' coverage_gaps re-checked. Return ' +
  'the coverage verdict + every hard residual for resolve-residuals.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [INVENTORY]
phase('Inventory')
const LANES = [
  { lane: 'vectors-a', scope: ROOT + '/Vectors — ls the .cs files, sort by size descending, take files 1,3,5,... (odd ranks) plus _ARCHITECTURE.md', focus: 'The main event: this code is functionally good but structurally weak — ledger the capability, judge the design hostilely.' },
  { lane: 'vectors-b', scope: ROOT + '/Vectors — ls the .cs files, sort by size descending, take files 2,4,6,... (even ranks)', focus: 'The main event: same law as vectors-a; note any capability your files share with odd-rank files as suspected duplication.' },
  { lane: 'domain-analysis', scope: ROOT + '/Domain (all 4 files, FULL depth — Geometry.cs gets the harshest read: it is suspected naive) + ' + ROOT + '/Analysis (skim: confirm nothing load-bearing hides in the 14 files before deletion; capability notes only where something IS load-bearing)', focus: 'Domain improves to feed the new vectors/geometry owners (stats/context); Analysis is slated DELETE.' },
  { lane: 'context', scope: ROOT + '/Geometry/.planning/ (18 pages — read at boundary depth: what they own vs what Vectors owns), the kernel ' +
    'index docs, Rasm.csproj (the 5 Rhino references), and both .api tiers',
    focus: 'Map the Vectors<->Geometry boundary pressure and every Rhino coupling fact the boundary ruling needs.' },
]
const inventory = (await parallel(LANES.map((l) => () =>
  agent(ledgerPrompt(l.lane, l.scope, l.focus), { label: 'ledger:' + l.lane, phase: 'Inventory', model: 'opus', effort: 'max', schema: LEDGER_SCHEMA, stallMs: STALL })
).concat([() => agent(ecosystemPrompt(), { label: 'ledger:ecosystem', phase: 'Inventory', model: 'opus', effort: 'max', schema: LEDGER_SCHEMA, stallMs: STALL })]))).filter(Boolean)
const DOSSIERS = inventory.map((d) => d.dossier)
log('Inventory: ' + inventory.length + '/5 ledgers, ' + inventory.reduce((n, d) => n + (d.capability_count || 0), 0) + ' capability rows; rhino facts: ' +
  inventory.reduce((n, d) => n + (d.rhino_facts || []).length, 0))

// --- [ARCHITECT]
phase('Architect')
const plan = await agent(architectPrompt(DOSSIERS), { label: 'architect', phase: 'Architect', effort: 'max', schema: BLUEPRINT_SCHEMA, stallMs: STALL })
if (!plan) { log('Architect produced nothing — aborting before any write; resume re-runs it.'); return { dossiers: DOSSIERS, aborted: 'architect' } }
log('Architect: ' + plan.pages.length + ' pages; rhino ruling: ' + plan.rhino_ruling)

// --- [AUTHOR]
phase('Author')
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const batches = chunk(plan.pages, 2)
const built = (await parallel(batches.map((b, i) => () =>
  agent(authorPrompt(b, plan.blueprint, DOSSIERS), { label: 'author:b' + i, phase: 'Author', effort: 'max', schema: BUILT_SCHEMA, stallMs: STALL })))).filter(Boolean)
const BUILT_PAGES = built.flatMap((b) => b.pages || [])
log('Author: ' + BUILT_PAGES.length + ' pages across ' + built.length + '/' + batches.length + ' batches; uncovered: ' +
  built.reduce((n, b) => n + (b.uncovered || []).length, 0))

// --- [REVIEW]
phase('Review')
const reviewed = (await parallel(chunk(BUILT_PAGES, 3).map((b, i) => () =>
  agent(reviewPrompt(b, plan.blueprint, DOSSIERS), { label: 'review:b' + i, phase: 'Review', effort: 'xhigh', schema: REVIEW_SCHEMA, stallMs: STALL })))).filter(Boolean)
const gaps = reviewed.flatMap((r) => r.coverage_gaps || [])
log('Review: ' + reviewed.length + ' batches, ' + reviewed.reduce((n, r) => n + (r.fixed || []).length, 0) + ' fixes, ' + gaps.length + ' coverage gap(s) forward to verify')

// --- [RESTRUCTURE]
phase('Restructure')
const exec = await agent(execPrompt(plan.blueprint), { label: 'restructure', phase: 'Restructure', effort: 'high', schema: EXEC_SCHEMA, stallMs: STALL })
log('Restructure: ' + ((exec && exec.moved) || []).length + ' moved, ' + ((exec && exec.deleted) || []).length + ' deleted, ' + ((exec && exec.kept) || []).length + ' kept-with-reason')

// --- [RIPPLE]
phase('Ripple')
const ripples = (await parallel([
  () => agent(rippleKernelPrompt(plan.blueprint), { label: 'ripple:kernel-docs', phase: 'Ripple', effort: 'xhigh', schema: RIPPLE_SCHEMA, stallMs: STALL }),
  () => agent(rippleRepoPrompt(plan.blueprint), { label: 'ripple:repo-docs', phase: 'Ripple', effort: 'xhigh', schema: RIPPLE_SCHEMA, stallMs: STALL }),
  () => agent(rippleWorkflowsPrompt(plan.blueprint), { label: 'ripple:workflows', phase: 'Ripple', effort: 'xhigh', schema: RIPPLE_SCHEMA, stallMs: STALL }),
])).filter(Boolean)
log('Ripple: ' + ripples.reduce((n, r) => n + (r.files || []).length, 0) + ' docs touched; residue flagged: ' + ripples.reduce((n, r) => n + (r.residue || []).length, 0))

// --- [VERIFY]
phase('Verify')
const verdict = await agent(verifyPrompt(plan.blueprint, DOSSIERS, BUILT_PAGES, JSON.stringify(gaps)),
  { label: 'verify', phase: 'Verify', effort: 'max', schema: VERIFY_SCHEMA, stallMs: STALL })

return {
  pages: BUILT_PAGES, rhino_ruling: plan.rhino_ruling, roster: plan.roster || [],
  moved: (exec && exec.moved) || [], deleted: (exec && exec.deleted) || [], kept: (exec && exec.kept) || [],
  docs_touched: ripples.flatMap((r) => r.files || []), doc_residue: ripples.flatMap((r) => r.residue || []),
  coverage: (verdict && verdict.coverage) || 'VERIFY MISSING — rerun', hard_residual: (verdict && verdict.residuals) || [],
  dossiers: DOSSIERS, blueprint: plan.blueprint, scratch: SCRATCH,
}
