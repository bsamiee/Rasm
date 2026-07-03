export const meta = {
  name: 'convert-rasm-kernel',
  description: 'Convert the Rasm kernel to an ordinary planning-scoped package: ledger the mature Vectors/Domain/Analysis source + the boundary/consumer facts, rule the new .planning partition + Rhino boundary + compile-consumer disposition + roster delta in one blueprint, author the new design pages ground-up at the docs/stacks/csharp bar with zero capability loss (author -> merged hostile review per unit, pipelined), execute the backup-gated restructure (Geometry re-home, source retirement, csproj/slnx/lock/manifest alignment), sweep every durable-doc and .claude ripple in one wave, and terminally verify capability coverage. About 22 agents, peak concurrency 6. Ephemeral: delete after the conversion lands.',
  whenToUse: 'One-shot kernel conversion. Launch only after no other run is reading libs/csharp/Rasm paths; the .archive/Rasm backup must exist.',
  phases: [
    { title: 'Inventory', detail: '4 capability-ledger lanes (Vectors halves, Domain+Analysis, boundary/consumer census) + 1 ecosystem lane, parallel, opus' },
    { title: 'Architect', detail: '1 agent rules the sub-domain partition, the Rhino boundary, the consumer disposition, the roster delta, the move/delete manifest' },
    { title: 'Build', detail: 'per blueprint unit: ground-up author -> merged critique+redteam hostile review, pipelined, fix-in-place' },
    { title: 'Restructure', detail: '1 agent: backup gate, Geometry re-home, source retirement, csproj/slnx/lock/central-manifest alignment' },
    { title: 'Ripple', detail: 'kernel index docs + repo durable docs + .claude surfaces (+ .api catalogs when the roster changed), parallel' },
    { title: 'Verify', detail: '1 terminal agent: capability reconciliation, structure audit, residue sweep, repair-in-place' },
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
const BLUEPRINT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'rhino_ruling', 'disposition', 'pages', 'units'], properties: {
  blueprint: { type: 'string' }, rhino_ruling: { type: 'string' }, disposition: { type: 'string' },
  pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'charter'], properties: { page: { type: 'string' }, charter: { type: 'string' } } } },
  units: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['unit', 'pages'], properties: { unit: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } },
  roster: { type: 'array', items: { type: 'string' } } } }
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
const PRE = 'Rasm monorepo, long-term planning phase: design pages ARE the work product; no session lands source files. The kernel ' + ROOT +
  ' converts from part-mature C# source to an ORDINARY planning-scoped package — every design page under ONE ' + NEW_PLAN + '/ root with ' +
  'sub-domain sub-folders, exactly the Rasm.Bim shape. The mature source retires after its capability is fully re-expressed: Vectors/ (13 .cs ' +
  'files, 13592 LOC — Mesh.cs 3957 and Field.cs 2599 the giants) and Domain/ (4 files, 1054 LOC) convert to design pages; Analysis/ (14 files) ' +
  'is slated DELETE. The mature code predates docs/stacks/csharp/ — its FUNCTIONALITY is the asset, its DESIGN is not: bloated surfaces, weak ' +
  'entry points, low code quality, no strata discipline. New pages are written GROUND-UP at the full docs/stacks/csharp bar, never tethered to ' +
  'the old file structure, never losing a capability: every public behavior in the ledgers lands in a page fence or is dropped with an explicit ' +
  'standing reason. ZERO capability loss is the campaign gate. The 18 settled Geometry design pages ' +
  '(' + ROOT + '/Geometry/.planning/{Numerics,Spatial,Meshing,Parametric,Processing,Drawing}) RE-HOME under the new root path-only — the ' +
  'standing RASM-CS-GEOMETRY-BRIEF.md campaign owns their next rebuild, so this conversion never rewrites their content. A full backup exists ' +
  'at .archive/Rasm; verify it before any delete. COMPILE CONSUMERS of the retiring source exist and get an explicit ruled disposition: ' +
  'Rasm.Rhino (Camera/Operations.cs + Camera/State.cs use Rasm.Vectors/VectorIntent; Commands/Context.cs + UI/Overlay.cs use Rasm.Analysis), ' +
  'Rasm.Grasshopper (11 UI/*.cs use Rasm.Domain), the tests/csharp/libs/{Rasm,Rasm.Rhino,Rasm.Grasshopper} test projects, and their ' +
  'Workspace.slnx rows; tools/cs-analyzer carries Rasm.Domain/Rasm.Analysis docID STRINGS only (vocabulary data, no compile edge). ' +
  'Rasm.Rhino and Rasm.Grasshopper SOURCE is never edited by this run; their build-surface disposition is the blueprint ruling. TOOLING BAN: ' +
  'tools/assay is under concurrent construction by another agent — NEVER invoke uv run python -m tools.assay in any form; external-member ' +
  'verification routes through the .api catalogues (decompile-verified), the nuget MCP, Context7, and official docs via exa/tavily instead.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const ledgerPrompt = (lane, scope, focus) => [PRE, 'TASK: CAPABILITY LEDGER, lane = ' + lane + ' (read-only outside ' + SCRATCH + '/). Scope: ' +
  scope + '. ' + focus + ' Read every file FULLY. WRITE the ledger to ' + SCRATCH + '/ledger-' + lane + '.md: one row per capability unit ' +
  '(public type/member family, algorithm, behavior) — {unit, file:line, what it does, quality verdict 1-10, bloat/duplication notes, Rhino ' +
  'coupling (exact members), where it SHOULD live (sub-domain suggestion or drop+reason)}. End with cross-cutting findings: entry-point rot, ' +
  'parallel rails, hardcoding, strata violations, and the 5-10 strongest structural rulings the architect must make. The ledger is a MAP for ' +
  'downstream agents, never a ceiling — dense, evidence-first, verified members only. Return the pointer + counts + rhino facts.'].join('\n')
const ecosystemPrompt = () => [PRE, 'TASK: ECOSYSTEM RESEARCH (read-only outside ' + SCRATCH + '/). The new kernel planning corpus may admit ' +
  'modern packages the mature code hand-rolled around. Inventory the current kernel roster (Rasm.csproj packages + Directory.Packages.props ' +
  'pins + libs/csharp/.api/ + ' + ROOT + '/.api/ catalogs), then research the .NET ecosystem for the kernel domains: SIMD/vectorized math, ' +
  'tensor/array substrates, computational-geometry primitives, spatial indexing, robust predicates, mesh/half-edge structures, sparse/dense ' +
  'numerics, spectral/graph algorithms. License gate: OSS or free-for-OSS commercial, never pay-tiered; every candidate verified real, ' +
  'maintained, .NET-current via the nuget MCP + two corroborating web sources. WRITE ' + SCRATCH + '/ledger-ecosystem.md: candidate table ' +
  'ranked categorical-best per concern (keep/add/replace vs the current roster, license, binding surface, exact package id). Return the ' +
  'pointer + counts.'].join('\n')
const architectPrompt = (dossiers) => [PRE, 'TASK: ARCHITECT — the one structure ruling. Read every ledger (' + JSON.stringify(dossiers) +
  '), the 18 Geometry pages at boundary depth, the kernel index docs (README/ARCHITECTURE/IDEAS/TASKLOG at ' + ROOT + '), and ' +
  'libs/.planning/architecture.md. DECIDE and WRITE ' + SCRATCH + '/blueprint.md: (1) THE PARTITION — the full sub-domain tree under ' +
  NEW_PLAN + '/, ruled from ledger evidence, NEVER a preset: the Geometry pages re-home as settled content (their Rasm.Geometry.* namespace ' +
  'law stands), the mature Vectors/Domain capability re-partitions freely into real higher-order sub-domains — one page per real owner, no ' +
  'one-file folders, no flat sprawl, growth-conducive domains — each page named with a charter + the ledger rows it absorbs, plus the ' +
  'namespace map for the new partition; group the NEW pages into author UNITS of 2-5 coherent pages (one unit per sub-domain where sizes ' +
  'allow). (2) THE RHINO RULING from ledger evidence: which capabilities are host-neutral kernel and which are genuinely Rhino-boundary, ' +
  'where the boundary surface lives, how RhinoCommon enters the Rasm compile (the Directory.Build.targets RhinoCommonReferencePath machinery ' +
  '+ the csproj Rhino usings) and whether it remains; Tier-0 libs/.planning/architecture.md [03] (universal-vs-capture: the C# branch is ' +
  'RhinoCommon-aware, the kernel included) is standing law — a ruling that amends it names the exact surgical Tier-0 edit for the ripple ' +
  'sweep, never silently contradicts it. (3) THE CONSUMER DISPOSITION: one explicit action per compile consumer — the Workspace.slnx rows ' +
  'for Rasm.Rhino/Rasm.Grasshopper and the tests/csharp/libs/{Rasm,Rasm.Rhino,Rasm.Grasshopper} projects, and the kernel test project fate — ' +
  'the honest default removes un-compilable projects from the solution build surface with a branch TASKLOG card recording re-entry at kernel ' +
  'realization; rule from evidence. (4) DOMAIN RULINGS — Domain/Geometry.cs gets the harshest scrutiny (carry forward ONLY what earns it, ' +
  'absorb the rest); Stats/Context/Validation improve into the new owners. (5) Analysis/ DELETE confirmed, with capability notes only where ' +
  'a ledger row proves something load-bearing hides there. (6) THE ROSTER DELTA from the ecosystem ledger: exact package ids to add/remove, ' +
  'each with the owning sub-domain that mines it. (7) THE MOVE/DELETE MANIFEST: exact paths (Geometry/.planning re-home, the emptied ' +
  'Geometry/ tree, Analysis/, Vectors/ incl. _ARCHITECTURE.md, retired Domain/*.cs) plus the surgical edits the 18 moved Geometry pages need ' +
  'where a name they reference changed (empty when the partition preserves every referenced name). Every ledger capability row maps to a ' +
  'page or an explicit drop — an unmapped row is a blueprint defect. Return the pointer, pages, units, the rhino ruling one-liner, the ' +
  'disposition one-liner, and the roster delta.'].join('\n')
const authorPrompt = (unit, blueprint, dossiers) => [PRE, 'TASK: AUTHOR ground-up, unit = ' + unit.unit + '. Write these design pages per ' +
  'the blueprint ' + blueprint + ': ' + JSON.stringify(unit.pages) + '. Read the blueprint, every ledger it cites (' +
  JSON.stringify(dossiers) + '), and THE MATURE SOURCE FILES your ledger rows anchor — the ledger is a map, never a ceiling; the source is ' +
  'the capability truth and it is still on disk. Each page: full docs/stacks/csharp bar, planning-standard page grammar, dense ' +
  'transcription-complete fences that RE-EXPRESS the ledger capabilities through the strongest design (polymorphic dispatch surfaces, typed ' +
  'LanguageExt rails, Thinktecture generated vocabularies, table-driven variation) — never a transliteration; the old file structure is ' +
  'DEAD, only its capability survives. Stack BOTH .api tiers (libs/csharp/.api/ + ' + ROOT + '/.api/) and the blueprint roster; a member is ' +
  'written as settled fence code only at a spelling a catalogue, the nuget MCP, or official docs verifies, and new-package members only as ' +
  'the blueprint roster admits them. Cover every ledger row the blueprint maps to your pages; list any you could not cover in uncovered — ' +
  'never silently drop. ' +
  'Write INTO the new home paths exactly as the blueprint names them.'].join('\n')
const reviewPrompt = (unit, blueprint, dossiers) => [PRE, 'TASK: MERGED CRITIQUE+REDTEAM — one hostile pass, fix in place, no further ' +
  'rounds. Pages: ' + JSON.stringify(unit.pages) + '. The pages are naive, shallow, or illusory until they survive your attack. FLOOR — the ' +
  'mechanical conformance checklists, line by line: collapse scan (repeated structure an algebra/table/fold/generator can own), owner choice, ' +
  'knob test, rail unification, C#14/net10 modernity, phantom-member kill (verify against both .api tiers + the nuget MCP/official docs), page ' +
  'grammar + banned hedges + zero provenance. BEYOND — the redteam lenses: counterfactual on the core owner/algebra/dispatch, ' +
  'diff-of-the-next-feature (the next case lands as one row with consumers untouched or loudly broken at type-check), long-tail and ' +
  'failure-mode attack, boundary/strata integrity against the blueprint ' + blueprint + ' rulings, capability completeness. Then the coverage ' +
  'audit: against the blueprint and the ledgers (' + JSON.stringify(dossiers) + '), confirm every ledger row mapped to these pages is ' +
  'actually expressed — repair gaps NOW by reading the mature source still on disk; report only what you cannot reach in ' +
  'coverage_gaps.'].join('\n')
const execPrompt = (blueprint) => [PRE, 'TASK: RESTRUCTURE EXECUTION per the blueprint ' + blueprint + ' manifest + rulings. ORDER: (1) verify ' +
  '.archive/Rasm holds the mature source (Vectors/, Domain/, Analysis/) — a missing backup ABORTS every delete (report in kept). (2) git mv ' +
  ROOT + '/Geometry/.planning/* to ' + NEW_PLAN + '/Geometry/ preserving sub-folders, remove the emptied Geometry/ tree, then apply the ' +
  'blueprint surgical Geometry-page edits (path/name re-pointing only, never content rework). (3) git rm ' + ROOT + '/Analysis/, ' + ROOT +
  '/Vectors/ (incl. _ARCHITECTURE.md), and the mature ' + ROOT + '/Domain/*.cs the blueprint retires. (4) rebuild ' + ROOT + '/Rasm.csproj to ' +
  'the planning-folder convention — label-grouped PackageReference set per the roster + rhino ruling, usings pruned to the ruling, hand-edit ' +
  'never dotnet add — apply the roster delta to Directory.Packages.props (grouped, one-line maintenance comments), then run dotnet restore on ' +
  'the csproj to regenerate packages.lock.json and prove resolution. (5) apply the CONSUMER DISPOSITION exactly as ruled: the Workspace.slnx ' +
  'rows and test-project actions — never edit Rasm.Rhino/Rasm.Grasshopper source. Use git commands for every move/delete so history survives. ' +
  'Report every path moved/deleted/kept-with-reason.'].join('\n')
const rippleKernelPrompt = (blueprint) => [PRE, 'TASK: KERNEL INDEX DOCS rewrite per the blueprint ' + blueprint + ' and the disk truth after ' +
  'restructure. Rewrite ' + ROOT + '/README.md, ARCHITECTURE.md, IDEAS.md, TASKLOG.md to the converted reality: one ' + NEW_PLAN + '/ root, ' +
  'the full page router + package registry, the codemap from the REAL page set on disk (the moved Geometry pages include Parametric/, which ' +
  'the old codemap omitted), the seams re-expressed for the new partition, the namespace law per the blueprint map, the rhino ruling, the ' +
  'roster — the mature/greenfield split is GONE from every sentence. TASKLOG carries the conversion as a closed card, the disposition ' +
  'follow-up cards the blueprint rules (host-boundary re-entry at kernel realization), and re-scopes open cards to the new pages; IDEAS ' +
  'entries re-anchor or drop. Durable-doc law: declarative, zero provenance, zero narration.'].join('\n')
const rippleRepoPrompt = (blueprint) => [PRE, 'TASK: REPO DURABLE-DOC SWEEP per the blueprint ' + blueprint + '. rg for ' +
  'Geometry/.planning|Rasm/Vectors|Rasm/Analysis|Rasm/Domain|mature|greenfield across the durable docs and fix every mention to the converted ' +
  'reality — surgical, never rewriting sections the conversion does not touch. Known carriers: libs/.planning/architecture.md (the [05] ' +
  'Rasm/Geometry/.planning exception instance, the [01] kernel stratum rows, and [03] ONLY where the blueprint rhino ruling names a Tier-0 ' +
  'amendment), libs/.planning/planning-targets.md (the Rasm sub-folder rows become ordinary planning-folder rows), ' +
  'libs/csharp/.planning/README.md (the [01] kernel router line) + ARCHITECTURE.md (the scaffold note) + IDEAS.md + TASKLOG.md (add the ' +
  'disposition card the blueprint rules), and RASM-CS-GEOMETRY-BRIEF.md — re-path its Geometry/.planning references to ' + NEW_PLAN +
  '/Geometry/ AND re-anchor its Vectors/Domain SOURCE references (file:line anchors and any assay-over-restored-source instruction) onto the ' +
  'NEW planning pages that carry the same capability: capability-level anchors where line-level cannot map, member verification re-routed to ' +
  'the new page fences as the source of truth. Sweep root README.md only where rg hits. Do NOT edit .claude/ (a sibling agent owns it) or ' +
  'memory files; list any memory mention in residue. Report every file touched + residue.'].join('\n')
const rippleClaudePrompt = (blueprint) => [PRE, 'TASK: CLAUDE-SURFACE SWEEP per the blueprint ' + blueprint + '. The .claude/ surfaces carry ' +
  'kernel special-casing that dies with the conversion: the kernel is now ONE ORDINARY planning-scoped target — planning at ' + NEW_PLAN +
  ', api at ' + ROOT + '/.api, no homing case, no off-limits mature siblings, no mature/greenfield split. rg -i for ' +
  'homing|Geometry/.planning|Vectors|Analysis|mature across .claude/workflows/*.js (EXCLUDING convert-rasm-kernel.js — its old-path ' +
  'references are its own subject), .claude/commands/prime.md (the HOMING EXCEPTION block dies), and .claude/prompts/*.md. Known workflow ' +
  'carriers: rebuild.js (the cs L.homing block + the discovery homing lines), tidy-planning-docs.js, hygiene-sweep.js, align-cards.js, ' +
  'ideate.js, survey-gaps.js (the Geometry FOLDERS row + LAYOUT note), survey-packages.js (the isGeometry branch), design-cs-geometry.js + ' +
  'design-cs-persistence.js (stale Vectors/Domain source anchors re-route to the new pages). RESUME-SAFETY LAW: workflow prompt text is a ' +
  'resume cache key, so edits are VALUE-NECESSARY ONLY — change the stale kernel facts in place, never restructure a prompt, never touch ' +
  'schemas/labels/models/meta blocks, never reflow untouched lines. After each edited .js, parse-check it: node -e with the async-function ' +
  'constructor over the file body (replace the leading export const meta = with const meta =) and repair any syntax error you introduced. Do ' +
  'NOT edit .claude/skills/ (the workflow-creator homing pattern is generic skill law, not kernel-coupled), .claude/scratch/, or memory ' +
  'files. Report every file touched + residue.'].join('\n')
const catalogsPrompt = (roster) => [PRE, 'TASK: API CATALOG ALIGNMENT for the roster delta: ' + JSON.stringify(roster) + '. For each ADDED ' +
  'package author ' + ROOT + '/.api/api-<package>.md at integration-shaped depth matching the sibling catalogs there — the full advanced ' +
  'surface AND how it stacks into the kernel rails, real members verified via the nuget MCP (get_package_context) + official docs/source via ' +
  'Context7/exa/tavily; never phantom members. For each REMOVED package git rm its catalog. Touch ONLY ' +
  ROOT + '/.api/. Report every file touched.'].join('\n')
const verifyPrompt = (blueprint, dossiers, built, gaps) => [PRE, 'TASK: TERMINAL VERIFY — adversarial, WRITING (repair what you can reach ' +
  'NOW; no further rounds). (1) CAPABILITY RECONCILIATION: walk every ledger (' + JSON.stringify(dossiers) + ') row against the landed pages ' +
  '(' + JSON.stringify(built) + ') and the blueprint ' + blueprint + ' drop list — every row is expressed or explicitly dropped with a ' +
  'standing reason; repair expressible gaps in place (the mature source survives at .archive/Rasm for consultation), report unreachable ones ' +
  'as residuals. (2) STRUCTURE AUDIT: ' + NEW_PLAN + '/ holds the full ruled partition; no ' + ROOT + '/Geometry, /Analysis, /Vectors ' +
  'remains; the csproj, packages.lock.json, Directory.Packages.props, and Workspace.slnx match the rulings. (3) RESIDUE SWEEP: rg the ' +
  'durable docs AND .claude/ (excluding convert-rasm-kernel.js, .claude/skills/, .claude/scratch/, memory) for ' +
  'homing|Geometry/.planning|mature|greenfield|old-path residue the ripple missed and fix it. (4) Review leftovers re-checked: ' + gaps +
  '. Return the coverage verdict + every hard residual as {files, claim} for resolve-residuals.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [INVENTORY]
phase('Inventory')
const LANES = [
  { lane: 'vectors-a', scope: ROOT + '/Vectors — ls the .cs files, sort by size descending, take files 1,3,5,... (odd ranks) plus _ARCHITECTURE.md', focus: 'The main event: this code is functionally good but structurally weak — ledger the capability, judge the design hostilely.' },
  { lane: 'vectors-b', scope: ROOT + '/Vectors — ls the .cs files, sort by size descending, take files 2,4,6,... (even ranks)', focus: 'The main event: same law as vectors-a; note any capability your files share with odd-rank files as suspected duplication.' },
  { lane: 'domain-analysis', scope: ROOT + '/Domain (all 4 files, FULL depth — Geometry.cs gets the harshest read: it is suspected naive) + ' + ROOT + '/Analysis (skim: confirm nothing load-bearing hides in the 14 files before deletion; capability notes only where something IS load-bearing)', focus: 'Domain improves to feed the new owners (stats/context/validation); Analysis is slated DELETE.' },
  { lane: 'boundary', scope: ROOT + '/Geometry/.planning/ (18 pages, boundary depth: what they own vs what Vectors owns, every Vectors/Domain ' +
    'vocabulary they compose), the kernel index docs, Rasm.csproj + the Directory.Build.targets RhinoCommonReferencePath machinery (how ' +
    'RhinoCommon actually enters the compile), both .api tiers, RASM-CS-GEOMETRY-BRIEF.md (its Vectors/Domain source anchors), and the ' +
    'compile-consumer census: exact member usage in Rasm.Rhino Camera/Commands/UI, Rasm.Grasshopper UI, the tests/csharp/libs Rasm test ' +
    'projects, and the Workspace.slnx rows',
    focus: 'Map the Vectors<->Geometry boundary pressure and EVERY fact the rhino ruling, the consumer disposition, and the brief re-anchoring need.' },
]
const inventory = (await parallel(LANES.map((l) => () =>
  agent(ledgerPrompt(l.lane, l.scope, l.focus), { label: 'ledger:' + l.lane, phase: 'Inventory', model: 'opus', effort: 'max', schema: LEDGER_SCHEMA, stallMs: STALL })
).concat([() => agent(ecosystemPrompt(), { label: 'ledger:ecosystem', phase: 'Inventory', model: 'opus', effort: 'max', schema: LEDGER_SCHEMA, stallMs: STALL })]))).filter(Boolean)
const DOSSIERS = inventory.map((d) => d.dossier)
if (inventory.length < 5) { log('Inventory incomplete (' + inventory.length + '/5) — aborting before any ruling; resume re-runs the missing lanes.'); return { aborted: 'inventory', dossiers: DOSSIERS } }
log('Inventory: 5/5 ledgers, ' + inventory.reduce((n, d) => n + (d.capability_count || 0), 0) + ' capability rows; rhino facts: ' +
  inventory.reduce((n, d) => n + (d.rhino_facts || []).length, 0))

// --- [ARCHITECT]
phase('Architect')
const plan = await agent(architectPrompt(DOSSIERS), { label: 'architect', phase: 'Architect', effort: 'max', schema: BLUEPRINT_SCHEMA, stallMs: STALL })
if (!plan || !plan.units.length) { log('Architect produced no buildable blueprint — aborting before any write; resume re-runs it.'); return { aborted: 'architect', dossiers: DOSSIERS } }
log('Architect: ' + plan.pages.length + ' pages in ' + plan.units.length + ' units; rhino: ' + plan.rhino_ruling + '; disposition: ' + plan.disposition)

// --- [BUILD]
phase('Build')
const built = (await pipeline(
  plan.units,
  (u) => agent(authorPrompt(u, plan.blueprint, DOSSIERS), { label: 'author:' + u.unit, phase: 'Build', effort: 'max', schema: BUILT_SCHEMA, stallMs: STALL }),
  (b, u) => !b ? null : agent(reviewPrompt(u, plan.blueprint, DOSSIERS), { label: 'review:' + u.unit, phase: 'Build', effort: 'max', schema: REVIEW_SCHEMA, stallMs: STALL })
    .then((r) => ({ unit: u.unit, pages: b.pages || [], uncovered: b.uncovered || [], review: r })),
)).filter(Boolean)
const BUILT_PAGES = built.flatMap((b) => b.pages)
const gaps = built.flatMap((b) => (b.review && b.review.coverage_gaps) || []).concat(built.flatMap((b) => b.uncovered))
if (built.length < plan.units.length || !BUILT_PAGES.length) {
  log('Build incomplete (' + built.length + '/' + plan.units.length + ' units) — the restructure DELETE gate requires the full corpus; aborting before any move/delete. Resume re-runs the missing units.')
  return { aborted: 'build', built: BUILT_PAGES, dossiers: DOSSIERS, blueprint: plan.blueprint }
}
log('Build: ' + BUILT_PAGES.length + ' pages across ' + built.length + ' units, ' + built.reduce((n, b) => n + ((b.review && b.review.fixed) || []).length, 0) +
  ' review fixes, ' + gaps.length + ' gap(s) forward to verify')

// --- [RESTRUCTURE]
phase('Restructure')
const exec = await agent(execPrompt(plan.blueprint), { label: 'restructure', phase: 'Restructure', effort: 'high', schema: EXEC_SCHEMA, stallMs: STALL })
log('Restructure: ' + ((exec && exec.moved) || []).length + ' moved, ' + ((exec && exec.deleted) || []).length + ' deleted, ' + ((exec && exec.kept) || []).length + ' kept-with-reason')

// --- [RIPPLE]
const rippleThunks = [
  () => agent(rippleKernelPrompt(plan.blueprint), { label: 'ripple:kernel-docs', phase: 'Ripple', effort: 'xhigh', schema: RIPPLE_SCHEMA, stallMs: STALL }),
  () => agent(rippleRepoPrompt(plan.blueprint), { label: 'ripple:repo-docs', phase: 'Ripple', effort: 'xhigh', schema: RIPPLE_SCHEMA, stallMs: STALL }),
  () => agent(rippleClaudePrompt(plan.blueprint), { label: 'ripple:claude-surfaces', phase: 'Ripple', effort: 'xhigh', schema: RIPPLE_SCHEMA, stallMs: STALL }),
]
if ((plan.roster || []).length) rippleThunks.push(() => agent(catalogsPrompt(plan.roster), { label: 'ripple:api-catalogs', phase: 'Ripple', effort: 'high', schema: RIPPLE_SCHEMA, stallMs: STALL }))
phase('Ripple')
const ripples = (await parallel(rippleThunks)).filter(Boolean)
log('Ripple: ' + ripples.reduce((n, r) => n + (r.files || []).length, 0) + ' files touched; residue flagged: ' + ripples.reduce((n, r) => n + (r.residue || []).length, 0))

// --- [VERIFY]
phase('Verify')
const verdict = await agent(verifyPrompt(plan.blueprint, DOSSIERS, BUILT_PAGES, JSON.stringify(gaps)),
  { label: 'verify', phase: 'Verify', effort: 'max', schema: VERIFY_SCHEMA, stallMs: STALL })

return {
  pages: BUILT_PAGES, rhino_ruling: plan.rhino_ruling, disposition: plan.disposition, roster: plan.roster || [],
  moved: (exec && exec.moved) || [], deleted: (exec && exec.deleted) || [], kept: (exec && exec.kept) || [],
  docs_touched: ripples.flatMap((r) => r.files || []), doc_residue: ripples.flatMap((r) => r.residue || []),
  coverage: (verdict && verdict.coverage) || 'VERIFY MISSING — rerun', hard_residual: (verdict && verdict.residuals) || [],
  dossiers: DOSSIERS, blueprint: plan.blueprint, scratch: SCRATCH,
}
