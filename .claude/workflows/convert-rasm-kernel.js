export const meta = {
  name: 'convert-rasm-kernel',
  description: 'Convert the Rasm kernel to an ordinary planning-scoped package: ledger the mature Vectors/Domain/Analysis source + the boundary/consumer facts, rule the new .planning partition + Rhino boundary + compile-consumer disposition + roster delta in one blueprint, author the new design pages ground-up at the docs/stacks/csharp bar with zero capability loss (author -> hostile critique review -> cold redteam per unit, pipelined; the physical restructure — Geometry re-home, source retirement, Radyab removal, csproj/slnx/lock/manifest alignment, index-doc rebuild — is operator-landed on disk), sweep every durable-doc and .claude ripple in one wave, rebuild-improve the geometry brief through 3 cold passes, waterfall the five consumer RASM-CS briefs against the as-if-realized kernel, align the whole C# brief corpus in one cold pass, and terminally verify capability coverage. About 47 agents, peak concurrency 6. Ephemeral: delete after the conversion lands.',
  whenToUse: 'One-shot kernel conversion. Launch only after no other run is reading libs/csharp/Rasm paths; the .archive/Rasm backup must exist.',
  phases: [
    { title: 'Inventory', detail: '4 capability-ledger lanes (Vectors halves, Domain+Analysis, boundary/consumer census) + 1 ecosystem lane, parallel, opus' },
    { title: 'Architect', detail: '1 agent rules the sub-domain partition, the Rhino boundary, the consumer disposition, the roster delta, the move/delete manifest' },
    { title: 'Build', detail: 'per blueprint unit: ground-up author -> hostile critique review -> cold redteam, pipelined, fix-in-place' },
    { title: 'Ripple', detail: 'kernel index docs + repo durable docs + .claude surfaces (+ .api catalogs when the roster changed), parallel; the physical restructure is already operator-landed' },
    { title: 'Campaign', detail: '3 cold passes (initial -> critique -> redteam): RASM-CS-GEOMETRY-BRIEF rebuilt and improved to the landed kernel + design-cs-geometry.js re-grounded' },
    { title: 'Waterfall', detail: '5 parallel: consumer RASM-CS briefs improved against the as-if-realized kernel, never weakened' },
    { title: 'Corpus', detail: '1 agent: one cold alignment pass over the whole C# brief corpus' },
    { title: 'Verify', detail: '1 terminal agent: capability reconciliation, structure audit, residue sweep, repair-in-place' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const ROOT = 'libs/csharp/Rasm'
const NEW_PLAN = ROOT + '/.planning'
const SCRATCH = '.claude/scratch/convert-rasm'
const CONSUMER_BRIEFS = [
  { brief: 'RASM-CS-APPHOST-BRIEF.md', target: 'libs/csharp/Rasm.AppHost' },
  { brief: 'RASM-CS-APPUI-BRIEF.md', target: 'libs/csharp/Rasm.AppUi' },
  { brief: 'RASM-CS-COMPUTE-BRIEF.md', target: 'libs/csharp/Rasm.Compute' },
  { brief: 'RASM-CS-FABRICATION-BRIEF.md', target: 'libs/csharp/Rasm.Fabrication' },
  { brief: 'RASM-CS-PERSISTENCE-BRIEF.md', target: 'libs/csharp/Rasm.Persistence' },
]

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

const STACK = 'Compose docs/stacks/csharp in FULL before judging any code-facing prescription — README.md, language.md, shapes.md, ' +
  'surfaces-and-dispatch.md, rails-and-effects.md, boundaries.md, algorithms.md, system-apis.md; the domain/ shards are out of scope. The ' +
  'bar: maximal collapse, zero litter/bloat/weak code, zero hand-rolling of capability an admitted package or the kernel owns, full internal ' +
  'integration, no underutilized api.'

const LANDED = 'STATE: the physical restructure is ALREADY LANDED on disk, operator-executed per the blueprint manifest — the 18 Geometry ' +
  'pages merged into the shared sub-domain folders under libs/csharp/Rasm/.planning/, the Vectors/Domain/Analysis source deleted (originals ' +
  'archived at .archive/Rasm), Radyab removed, csproj/lock/central-manifest/slnx aligned, and the kernel README/ARCHITECTURE/IDEAS/TASKLOG ' +
  'already rebuilt. Where the campaign framing above describes the conversion as pending, the DISK is the truth: verify and refine, never redo.'

const GEO_DELTA = 'BEFORE/AFTER: before this conversion the brief targeted libs/csharp/Rasm/Geometry/.planning/ (18 settled pages in six ' +
  'sub-folders) beside the mature Vectors/Domain/Analysis SOURCE it anchored by file:line; AFTER, those 18 pages live merged into the shared ' +
  'libs/csharp/Rasm/.planning/ sub-domain folders beside 34 NEW pages that re-express the whole Vectors/Domain/Analysis capability at the ' +
  'doctrine bar, and the source is deleted — the originals remain readable at .archive/Rasm/{Vectors,Domain,Analysis} for grounding any ' +
  'anchor whose original referent must be understood. The brief adjusts to the AFTER state and IMPROVES against it, never thins.'

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
const redteamPrompt = (unit, blueprint, dossiers) => [PRE, 'TASK: COLD REDTEAM — the terminal, most aggressive review, a FULL cold re-review ' +
  'AFTER the critique pass, fix in place, WRITE. Pages: ' + JSON.stringify(unit.pages) + '. A prior clean verdict is a rejected ' +
  'self-assessment; the pages are hollow until YOUR attack fails. Lenses: counterfactual on the core owner/algebra/dispatch shape (a stronger ' +
  'owner that collapses more surface REPLACES the weaker one), diff-of-the-next-feature (the next case/dimension/modality lands as ONE row ' +
  'with every consumer untouched or loudly broken at type-check), long-tail + failure-mode attack, boundary/strata integrity against the ' +
  'blueprint ' + blueprint + ' rulings, surface-sprawl + phantom-member kill (both .api tiers + the nuget MCP), anticipatory collapse of any ' +
  'enumerated family a generator/table/fold should own, and domain completeness against the ledgers (' + JSON.stringify(dossiers) + '). ' +
  'STANDING RULING, supersedes any consumer-preservation pressure in the ledgers or blueprint: the Analysis consumers — the ' +
  'apps/grasshopper/Radyab toy app (ruled disposable, trashed at restructure) and the host-boundary binding layers — carry ZERO compat ' +
  'weight; the old AnalysisQuery/Analyze entry shape is preserved for nobody; capability is re-expressed at maximum advancement on pure ' +
  'merit and chaff is dropped with an explicit standing reason, never carried. Every defect is repaired in place; the pages end objectively ' +
  'denser and more capable than the critique left them. Report files touched + fixed + coverage_gaps you cannot reach.'].join('\n')
const rippleKernelPrompt = (blueprint) => [PRE, LANDED, 'TASK: KERNEL INDEX DOCS verify-and-refine per the blueprint ' + blueprint + '. The ' +
  'four index docs at ' + ROOT + ' are already rebuilt — hostile-verify each against the blueprint and the REAL page set on disk, and refine ' +
  'in place: router completeness (all 52 pages routed with truthful one-line charters), the ARCHITECTURE codemap vs disk, the seams block ' +
  '(every edge mirrored on the counterpart folder ARCHITECTURE where one exists), the namespace matrix, the package registry vs the csproj + ' +
  'central manifest + .api tiers, TASKLOG (the conversion recorded as a truthful closed card, HOST_BOUNDARY_REENTRY + T-BOOLEAN carried, ' +
  'open cards re-scoped to the new pages), and IDEAS (the WATCH card). The mature/greenfield split is GONE from every sentence; no card ' +
  'preserves Radyab or frames Analysis-entry compat. Fix every drift found; never a from-scratch rewrite of conforming content. Durable-doc ' +
  'law: declarative, zero provenance, zero narration.'].join('\n')
const rippleRepoPrompt = (blueprint) => [PRE, LANDED, 'TASK: REPO DURABLE-DOC SWEEP per the blueprint ' + blueprint + '. rg for ' +
  'Geometry/.planning|Rasm/Vectors|Rasm/Analysis|Rasm/Domain|mature|greenfield across the durable docs and fix every mention to the converted ' +
  'reality — surgical, never rewriting sections the conversion does not touch. Known carriers: libs/.planning/architecture.md (the [05] ' +
  'Rasm/Geometry/.planning exception instance, the [01] kernel stratum rows, and [03] ONLY where the blueprint rhino ruling names a Tier-0 ' +
  'amendment), libs/.planning/planning-targets.md (the Rasm sub-folder rows become ordinary planning-folder rows), ' +
  'libs/csharp/.planning/README.md (the [01] kernel router line) + ARCHITECTURE.md (the scaffold note) + IDEAS.md + TASKLOG.md (add the ' +
  'disposition card the blueprint rules). RASM-CS-GEOMETRY-BRIEF.md is OWNED by the terminal campaign reconciler — never touch it here. ' +
  'Sweep root README.md only where rg hits, and any durable-doc mention of the deleted ' +
  'apps/grasshopper/Radyab toy app. Do NOT edit .claude/ (a sibling agent owns it) or memory files; list any memory mention in residue. ' +
  'Report every file touched + residue.'].join('\n')
const rippleClaudePrompt = (blueprint) => [PRE, LANDED, 'TASK: CLAUDE-SURFACE SWEEP per the blueprint ' + blueprint + '. The .claude/ surfaces carry ' +
  'kernel special-casing that dies with the conversion: the kernel is now ONE ORDINARY planning-scoped target — planning at ' + NEW_PLAN +
  ', api at ' + ROOT + '/.api, no homing case, no off-limits mature siblings, no mature/greenfield split. rg -i for ' +
  'homing|Geometry/.planning|Vectors|Analysis|mature across .claude/workflows/*.js (EXCLUDING convert-rasm-kernel.js — its old-path ' +
  'references are its own subject), .claude/commands/prime.md (the HOMING EXCEPTION block dies), and .claude/prompts/*.md. Known workflow ' +
  'carriers: rebuild.js (the cs L.homing block + the discovery homing lines), tidy-planning-docs.js, hygiene-sweep.js, align-cards.js, ' +
  'ideate.js, survey-gaps.js (the Geometry FOLDERS row + LAYOUT note), survey-packages.js (the isGeometry branch), and ' +
  'design-cs-persistence.js (stale Vectors/Domain source anchors re-route to the new pages); design-cs-geometry.js is OWNED by the terminal ' +
  'campaign reconciler — never touch it here. RESUME-SAFETY LAW: workflow prompt text is a ' +
  'resume cache key, so edits are VALUE-NECESSARY ONLY — change the stale kernel facts in place, never restructure a prompt, never touch ' +
  'schemas/labels/models/meta blocks, never reflow untouched lines. After each edited .js, parse-check it: node -e with the async-function ' +
  'constructor over the file body (replace the leading export const meta = with const meta =) and repair any syntax error you introduced. Do ' +
  'NOT edit .claude/skills/ (the workflow-creator homing pattern is generic skill law, not kernel-coupled), .claude/scratch/, or memory ' +
  'files. Report every file touched + residue.'].join('\n')
const catalogsPrompt = (roster) => [PRE, LANDED, 'TASK: API CATALOG VERIFICATION for the roster delta: ' + JSON.stringify(roster) + '. The ' +
  'catalog deletions (api-triangle, api-geometry3sharp) and the api-mathnet-providers OpenBLAS-only trim are already landed — hostile-verify ' +
  'each against the roster ruling and deepen where the trim left thin coverage. The kernel csproj now references QuikGraph: confirm the ' +
  'branch-tier catalogue at libs/csharp/.api/ covers the kernel Spatial/neighbors (Prim-MST) + Processing/segment (graph-cut) usage at the ' +
  'spellings the pages fence, authoring a kernel overlay at ' + ROOT + '/.api/ ONLY where a folder-specific seam fact demands one; real ' +
  'members via the nuget MCP (get_package_context) + official docs via Context7/exa/tavily, never phantoms. Touch ONLY .api catalogue files. ' +
  'Report every file touched.'].join('\n')
const geoBriefPrompt = (blueprint) => [PRE, STACK, GEO_DELTA, 'TASK: GEOMETRY BRIEF REBUILD — pass 1 of 3 (initial). The standing campaign ' +
  '(RASM-CS-GEOMETRY-BRIEF.md -> design-cs-geometry.js -> DECISION -> rebuild legs) runs AFTER this conversion; it is alive, never dead, and ' +
  'it comes out STRONGER than it went in. Read the landed kernel corpus (' + NEW_PLAN + '/ in full — the new pages AND the 18 re-homed ' +
  'robust-core pages merged into the shared sub-domain folders), the blueprint ' + blueprint + ', RASM-CS-GEOMETRY-BRIEF.md IN FULL, and ' +
  '.claude/workflows/design-cs-geometry.js IN FULL. Then WRITE: (1) the brief re-drawn against the converted kernel — every Geometry/.planning ' +
  'path re-mapped per the blueprint moves, every Vectors/Domain/Analysis SOURCE anchor re-anchored onto the new pages that carry the same ' +
  'capability (capability-level anchors where line-level cannot map; member verification re-routed to the new page fences + the .api ' +
  'catalogs), scope boundaries named explicitly where a new page now owns capability the brief references (the DEC substrate, spectral, ' +
  'sampling, SDF, and reconstruction planes) so the campaign rebuilds the robust-core pages WITHOUT re-litigating what this conversion ' +
  'landed, and the V1 namespace-reconciliation duty the blueprint assigns to the campaign stated in the brief. (2) DUE DILIGENCE beyond ' +
  'adjustment — the radically different kernel opens new ground: research new external packages the campaign can now admit (nuget MCP + ' +
  'exa/tavily, OSS-license-gated, verified real and .NET-current), surface architectural weaknesses of the LANDED kernel corpus as explicit ' +
  'brief mandates, and extend the brief with the new avenues the merged corpus enables — leveraging the existing brief holistically, never ' +
  'weakening a mandate, never cutting for future concerns. Fold the roster delta + rhino ruling in where the brief speaks to packages or ' +
  'host coupling. (3) design-cs-geometry.js — VALUE-NECESSARY edits only: survey/probe scopes and path references re-grounded onto the new ' +
  'corpus; never restructure a prompt, never touch schemas/labels/models/meta blocks; parse-check via node -e with the async-function ' +
  'constructor over the file body (replace the leading export const meta = with const meta =) and repair any syntax error you introduce. ' +
  'Report files touched + residue.'].join('\n')
const geoCritiquePrompt = (blueprint) => [PRE, STACK, GEO_DELTA, 'TASK: GEOMETRY BRIEF CRITIQUE — pass 2 of 3, a COLD read with zero deference to pass ' +
  '1: no hedging, no defending, every finding is a fix applied in place. Read RASM-CS-GEOMETRY-BRIEF.md IN FULL, the landed kernel corpus at ' +
  NEW_PLAN + '/, the blueprint ' + blueprint + ', and .claude/workflows/design-cs-geometry.js. Audit and FIX: every path and anchor proven ' +
  'real on disk; every named member verified against the new page fences, both .api tiers, or the nuget MCP — a phantom dies; every mandate ' +
  'held to the doctrine bar (collapse-maximal, composes the kernel owner instead of hand-rolling, exploits admitted packages to operator ' +
  'depth, no bloat, no underutilized api); campaign completeness — a landed-kernel weakness or new-package avenue pass 1 missed is ADDED, ' +
  'never noted; the design-cs-geometry.js edits verified value-necessary and parse-clean. The brief ends objectively stronger. Report files ' +
  'touched + residue.'].join('\n')
const geoRedteamPrompt = (blueprint) => [PRE, STACK, GEO_DELTA, 'TASK: GEOMETRY BRIEF REDTEAM — pass 3 of 3, the terminal, most aggressive COLD review: ' +
  'the brief is naive, thin, or illusory until YOUR attack fails, and a prior clean verdict is a rejected self-assessment. Read ' +
  'RASM-CS-GEOMETRY-BRIEF.md IN FULL, the landed corpus at ' + NEW_PLAN + '/, the blueprint ' + blueprint + ', and ' +
  '.claude/workflows/design-cs-geometry.js. Attack and FIX in place: counterfactual on the campaign shape the brief prescribes (a stronger ' +
  'partition/owner/ruling REPLACES the weaker), diff-of-the-next-feature over its mandates, boundary/strata integrity against the landed ' +
  'corpus, weakening-detection (any earlier pass that reduced or hedged a mandate is reverted STRONGER — the campaign is future-facing and ' +
  'never cuts for future concerns), and world-class completeness (the brief drives the robust-core to a bar a public company would adopt). ' +
  'The brief ends denser and more capable than pass 2 left it. Report files touched + residue.'].join('\n')
const waterfallPrompt = (b, blueprint) => [PRE, STACK, 'TASK: BRIEF WATERFALL — ' + b.brief + ' (target ' + b.target + '). The converted ' +
  'kernel and the settled RASM-CS-GEOMETRY-BRIEF.md are upstream FINALIZED LAW: treat ' + ROOT + ' AS IF the geometry campaign is already ' +
  'fully realized — this consumer brief writes against the union of the landed pages and the geometry-brief mandates. Read ' + b.brief +
  ' IN FULL, the target folder grounding (' + b.target + '/README.md + ARCHITECTURE.md + the .planning pages your mandates touch), the ' +
  'kernel index docs + the ' + NEW_PLAN + '/ page set (routers and charters everywhere; deep-read every kernel page your target composes), ' +
  'RASM-CS-GEOMETRY-BRIEF.md, and the blueprint ' + blueprint + '. IMPROVE the brief to leverage the new kernel: a mandate that hand-rolls ' +
  'capability the kernel now owns re-composes the kernel owner instead; kernel rails, receipts, and entries integrate where they strengthen ' +
  'a mandate; NEW avenues and features the new kernel enables EXTEND the brief; every stale Rasm path or member re-anchors onto the new ' +
  'pages. NEVER reduce, weaken, or hedge a mandate — improvement and extension only, holistically considered. Fix in place. Report files ' +
  'touched + residue.'].join('\n')
const alignPrompt = (blueprint) => [PRE, STACK, 'TASK: CROSS-CORPUS BRIEF ALIGNMENT — one COLD pass over the WHOLE C# brief corpus: ' +
  'RASM-CS-GEOMETRY-BRIEF.md plus the five consumer briefs (APPHOST, APPUI, COMPUTE, FABRICATION, PERSISTENCE). Read every brief IN FULL ' +
  'against the landed kernel (' + NEW_PLAN + '/) and the blueprint ' + blueprint + '; every finding is a fix applied in place, never a note ' +
  '— no hedging, no defending. Align: inter-brief seams cohere (a capability one brief exports, the consumer composes at the same ' +
  'spelling); kernel law flows downstream and is re-minted nowhere; duplicated or contradictory mandates collapse to one owner; every path, ' +
  'anchor, member, and package claim spot-verified against disk, the page fences, both .api tiers, or the nuget MCP; and no waterfall pass ' +
  'weakened a mandate — a reduced mandate is reverted STRONGER. Report files touched + residue.'].join('\n')
const verifyPrompt = (blueprint, dossiers, built, gaps) => [PRE, LANDED, 'TASK: TERMINAL VERIFY — adversarial, WRITING (repair what you can reach ' +
  'NOW; no further rounds). (1) CAPABILITY RECONCILIATION: walk every ledger (' + JSON.stringify(dossiers) + ') row against the landed pages ' +
  '(' + JSON.stringify(built) + ') and the blueprint ' + blueprint + ' drop list — every row is expressed or explicitly dropped with a ' +
  'standing reason; repair expressible gaps in place (the mature source survives at .archive/Rasm for consultation), report unreachable ones ' +
  'as residuals. (2) STRUCTURE AUDIT: ' + NEW_PLAN + '/ holds the full ruled partition; no ' + ROOT + '/Geometry, /Analysis, /Vectors ' +
  'remains; apps/grasshopper/Radyab is gone with no dangling slnx row or doc reference; the csproj, packages.lock.json, ' +
  'Directory.Packages.props, and Workspace.slnx match the rulings. (3) RESIDUE SWEEP: rg the ' +
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
  (r, u) => !r ? null : agent(redteamPrompt(u, plan.blueprint, DOSSIERS), { label: 'redteam:' + u.unit, phase: 'Build', effort: 'max', schema: REVIEW_SCHEMA, stallMs: STALL })
    .then((rt) => ({ ...r, redteam: rt })),
)).filter(Boolean)
const BUILT_PAGES = built.flatMap((b) => b.pages)
const gaps = built.flatMap((b) => (b.review && b.review.coverage_gaps) || []).concat(built.flatMap((b) => b.uncovered))
  .concat(built.flatMap((b) => (b.redteam && b.redteam.coverage_gaps) || []))
if (built.length < plan.units.length || !BUILT_PAGES.length) {
  log('Build incomplete (' + built.length + '/' + plan.units.length + ' units) — the downstream sweep gate requires the full corpus; aborting. Resume re-runs the missing units.')
  return { aborted: 'build', built: BUILT_PAGES, dossiers: DOSSIERS, blueprint: plan.blueprint }
}
log('Build: ' + BUILT_PAGES.length + ' pages across ' + built.length + ' units, ' + built.reduce((n, b) => n + ((b.review && b.review.fixed) || []).length, 0) +
  ' critique fixes, ' + built.reduce((n, b) => n + ((b.redteam && b.redteam.fixed) || []).length, 0) + ' redteam fixes, ' + gaps.length + ' gap(s) forward to verify')

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

// --- [CAMPAIGN]
phase('Campaign')
const geo = []
geo.push(await agent(geoBriefPrompt(plan.blueprint), { label: 'campaign:geo-initial', phase: 'Campaign', effort: 'max', schema: RIPPLE_SCHEMA, stallMs: STALL }))
geo.push(await agent(geoCritiquePrompt(plan.blueprint), { label: 'campaign:geo-critique', phase: 'Campaign', effort: 'max', schema: RIPPLE_SCHEMA, stallMs: STALL }))
geo.push(await agent(geoRedteamPrompt(plan.blueprint), { label: 'campaign:geo-redteam', phase: 'Campaign', effort: 'max', schema: RIPPLE_SCHEMA, stallMs: STALL }))
const campaign = geo.filter(Boolean)
log('Campaign: ' + campaign.length + '/3 geometry-brief passes, ' + campaign.reduce((n, r) => n + (r.files || []).length, 0) + ' file-touches')

// --- [WATERFALL]
phase('Waterfall')
const waterfall = (await parallel(CONSUMER_BRIEFS.map((b) => () =>
  agent(waterfallPrompt(b, plan.blueprint), { label: 'waterfall:' + b.brief.replace('RASM-CS-', '').replace('-BRIEF.md', '').toLowerCase(), phase: 'Waterfall', effort: 'max', schema: RIPPLE_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Waterfall: ' + waterfall.length + '/' + CONSUMER_BRIEFS.length + ' consumer briefs propagated')

// --- [CORPUS]
phase('Corpus')
const corpus = await agent(alignPrompt(plan.blueprint), { label: 'corpus:align', phase: 'Corpus', effort: 'max', schema: RIPPLE_SCHEMA, stallMs: STALL })
log('Corpus: ' + ((corpus && corpus.files) || []).length + ' brief files aligned')

// --- [VERIFY]
phase('Verify')
const verdict = await agent(verifyPrompt(plan.blueprint, DOSSIERS, BUILT_PAGES, JSON.stringify(gaps)),
  { label: 'verify', phase: 'Verify', effort: 'max', schema: VERIFY_SCHEMA, stallMs: STALL })

return {
  campaign_files: campaign.flatMap((r) => r.files || []),
  waterfall_files: waterfall.flatMap((r) => r.files || []), corpus_files: (corpus && corpus.files) || [],
  pages: BUILT_PAGES, rhino_ruling: plan.rhino_ruling, disposition: plan.disposition, roster: plan.roster || [],
  docs_touched: ripples.flatMap((r) => r.files || []),
  doc_residue: ripples.concat(campaign, waterfall, corpus ? [corpus] : []).flatMap((r) => r.residue || []),
  coverage: (verdict && verdict.coverage) || 'VERIFY MISSING — rerun', hard_residual: (verdict && verdict.residuals) || [],
  dossiers: DOSSIERS, blueprint: plan.blueprint, scratch: SCRATCH,
}
