export const meta = {
  name: 'design-cs-fabrication',
  description: 'Ephemeral Phase-1 design pass for the Fabrication campaign: survey (4 plane agents re-verifying the register on disk + 1 FEASIBILITY PROBE: the OpenCAMLib C-shim/P-Invoke binding over the shipped C++-ABI archives, ruling the [V5] surface-engine siting at decision time) -> 4 complete folder/page-set blueprints under the brief lenses (namespace-true partition, kernel-seam-first, package-mining-first, output-first over the four flagship programs) -> 3 judge lenses with FIVE disqualifying gates (verdict disposition, folder=namespace 1:1, >=2 pages per folder, seam acyclicity under the two-node entry rule, the ruled kernel dispositions composed) -> synthesize winner+grafts -> red-team decide gate that WRITES RASM-CS-FABRICATION-DECISION.md in rebuild-engine-native action vocabulary with the surface-engine hinge ruled from probe evidence and the RULED kernel dispositions ([V10]a kerf split, [V10]d medial radius carriage) verified carried and signature-locked, never re-opened -> salvage (4 draft miners + overturn auditor + 1 writing integrator). About 20 agents, peak concurrency 5. args = {brief, out, scratch} with defaults; a string arg overrides the brief path.',
  whenToUse: 'One-shot: turn RASM-CS-FABRICATION-BRIEF.md into RASM-CS-FABRICATION-DECISION.md — the binding folder/page-set/namespace/fault-cluster/seam-ledger/entry-contract/leg-partition blueprint the durable rebuild engine executes structure-first. Launch when the Fabrication campaign opens, after the geometry DECISION lands. Delete after the campaign lands.',
  phases: [
    { title: 'Survey', detail: '4 plane surveyors (register re-verified on disk, namespace/consumer/seam census) + 1 feasibility probe (the OpenCAMLib binding hinge)' },
    { title: 'Draft', detail: '4 complete blueprints, one per brief lens (namespace-true-partition, kernel-seam-first, package-mining-first, output-first)' },
    { title: 'Judge', detail: '3 lenses; disqualifying gates: disposition completeness, folder=namespace 1:1 + >=2 pages, two-node acyclicity, ruled kernel dispositions composed' },
    { title: 'Synthesize', detail: '1 agent merges the winning blueprint with every accepted graft' },
    { title: 'Decide', detail: '1 red-team gate: attack, rule the OpenCAMLib hinge on probe evidence, verify the ruled kernel dispositions carried, then WRITE the DECISION (a-f contract)' },
    { title: 'Salvage', detail: '4 per-draft miners + 1 overturn auditor against the WRITTEN decision, then 1 writing integrator' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const BRIEF = (typeof argsIn === 'string' && argsIn.trim()) ? argsIn.trim()
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.brief === 'string' && argsIn.brief.trim()) ? argsIn.brief.trim()
  : 'RASM-CS-FABRICATION-BRIEF.md'
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim() : 'RASM-CS-FABRICATION-DECISION.md'
const SCRATCH = (argsIn && typeof argsIn === 'object' && typeof argsIn.scratch === 'string' && argsIn.scratch.trim()) ? argsIn.scratch.trim() : '.claude/scratch/design-cs-fabrication'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'register_verdicts', 'key_facts'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, pages_read: { type: 'number' },
  register_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['row', 'status'], properties: { row: { type: 'string' }, status: { type: 'string', enum: ['HOLD', 'DRIFT', 'REFUTED'] }, note: { type: 'string' } } } },
  key_facts: { type: 'array', items: { type: 'string' } } } }
const PROBE_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'probe', 'verdict', 'evidence'], properties: {
  dossier: { type: 'string' }, probe: { type: 'string' },
  verdict: { type: 'string', enum: ['FEASIBLE', 'FEASIBLE_WITH_GAPS', 'INFEASIBLE'] },
  gaps: { type: 'array', items: { type: 'string' } }, evidence: { type: 'array', items: { type: 'string' } } } }
const DRAFT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'lens', 'page_rows', 'folders', 'verdicts_disposed', 'evidence_disposed', 'cards_disposed', 'packages_disposed'], properties: {
  blueprint: { type: 'string' }, lens: { type: 'string' }, page_rows: { type: 'number' }, folders: { type: 'number' }, verdicts_disposed: { type: 'number' },
  evidence_disposed: { type: 'number' }, cards_disposed: { type: 'number' }, packages_disposed: { type: 'number' }, thesis: { type: 'string' } } }
const JUDGE_SCHEMA = { type: 'object', additionalProperties: false, required: ['lens', 'scores', 'winner'], properties: {
  lens: { type: 'string' }, winner: { type: 'string' }, rationale: { type: 'string' },
  scores: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['draft', 'gate_dispositions', 'gate_partition', 'gate_acyclic', 'gate_ruled', 'capability', 'collapse'], properties: {
    draft: { type: 'string' }, gate_dispositions: { type: 'string', enum: ['pass', 'fail'] }, gate_partition: { type: 'string', enum: ['pass', 'fail'] },
    gate_acyclic: { type: 'string', enum: ['pass', 'fail'] }, gate_ruled: { type: 'string', enum: ['pass', 'fail'] }, capability: { type: 'number' },
    collapse: { type: 'number' }, elegance: { type: 'number' }, grafts: { type: 'array', items: { type: 'string' } } } } } } }
const SYNTH_SCHEMA = { type: 'object', additionalProperties: false, required: ['synthesis', 'base', 'grafts_applied'], properties: {
  synthesis: { type: 'string' }, base: { type: 'string' }, grafts_applied: { type: 'array', items: { type: 'string' } }, open_items: { type: 'array', items: { type: 'string' } } } }
const DECIDE_SCHEMA = { type: 'object', additionalProperties: false, required: ['decision', 'gate_record', 'overturns', 'page_rows', 'legs', 'probe_rulings'], properties: {
  decision: { type: 'string' }, page_rows: { type: 'number' }, legs: { type: 'array', items: { type: 'string' } },
  probe_rulings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['hinge', 'ruling'], properties: { hinge: { type: 'string' }, ruling: { type: 'string' } } } },
  gate_record: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['attack', 'outcome'], properties: { attack: { type: 'string' }, outcome: { type: 'string' }, disposition: { type: 'string' } } } },
  overturns: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'why'], properties: { what: { type: 'string' }, why: { type: 'string' } } } } } }
const MINE_SCHEMA = { type: 'object', additionalProperties: false, required: ['source', 'findings'], properties: {
  source: { type: 'string' }, findings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'evidence'], properties: { what: { type: 'string' }, evidence: { type: 'string' }, belongs: { type: 'string' } } } } } }
const AUDIT_SCHEMA = { type: 'object', additionalProperties: false, required: ['overturn_verdicts', 'silent_drops'], properties: {
  overturn_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['overturn', 'verdict'], properties: { overturn: { type: 'string' }, verdict: { type: 'string', enum: ['JUSTIFIED', 'OVERZEALOUS', 'PARTIAL'] }, note: { type: 'string' } } } },
  silent_drops: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'evidence'], properties: { what: { type: 'string' }, evidence: { type: 'string' } } } } } }
const INTEGRATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['applied', 'rejected', 'files_touched'], properties: {
  applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['finding', 'where'], properties: { finding: { type: 'string' }, where: { type: 'string' } } } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['finding', 'why'], properties: { finding: { type: 'string' }, why: { type: 'string' } } } },
  files_touched: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const PRE = 'Rasm monorepo. THE BRIEF ' + BRIEF + ' (repo root) is binding law — read it COMPLETELY first: [00]-[SHARED_LAW] (the 4-proof verdict, ' +
  'telos at production-fabrication scope, structural authority with the recommended 13-folder floor, strata-and-kernel law, generator law, ' +
  'seam-and-entry law with the wired-pipeline law and the content-key law, roster reconciliation with INTEGRATION-FIRST and the license gate), ' +
  'V1-V10 the binding verdicts with ruled defaults and deciding criteria, [02] the evidence register E1-E14, [03] escalation targets, [04] ' +
  'package pressure, [05] the four build legs + acceptance dry-runs, [06] out-of-scope. Target: libs/csharp/Rasm.Fabrication/.planning/ (17 ' +
  'pages, 5 folders today: Process/ owner family physics faults magazine is split across folders — Process/{owner,family,physics,faults}, ' +
  'Toolpath/{motion,skeleton,slicing,guard,kinematics}, Nesting/{nfp,stock,workholding}, Polygon/{clipper,import}, Posting/{program,projection}, ' +
  'Process/magazine) plus README/ARCHITECTURE/TASKLOG/IDEAS, the 14 folder-tier .api catalogs, and the central Directory.Packages.props. ' +
  'Upstream law honored, never re-planned: RASM-CS-GEOMETRY-BRIEF.md (the kernel toolpath plane [V10], DrawingProjection [V3], booleans [V5], ' +
  'the one distance-field lane [V8], hull [V11], the [04] re-scopes — Fabrication CONSUMES, never re-implements; both kernel hinges are RULED ' +
  'there: [V10]a the kerf split — kernel corner-row offset assembly + medial, Fabrication pure kerf compensation in arc space over ArcAlgebra — ' +
  'and [V10]d the medial radius + arbitrary-probe clearance carriage, Toolpath/Skeleton.cs dying unconditionally; RASM-CS-GEOMETRY-DECISION.md, ' +
  'where landed, finalizes the exact seam-member spellings this campaign signature-locks), RASM-CS-PERSISTENCE-BRIEF.md ' +
  '(the re-anchored durable-rows card), RASM-CS-COMPUTE-BRIEF.md (waste-rollup + GLB layout law), RASM-CS-APPHOST-BRIEF.md (machine ' +
  'connectivity is livewire\'s), RASM-CS-APPUI-BRIEF.md ([V7] ACadSharp write leg; the HiddenLineSeam receipt insulation), and ' +
  '.archive/RASM-COMPONENT-PARADIGM-DECISION.md LANDED LAW (FaultBand.Fabrication = 2700 with offsets 2701-2710, Nesting/stock.md — build ON, never ' +
  're-band or re-move). docs/stacks/csharp/ governs every fence; member verification runs uv run python -m tools.assay api over restored ' +
  'assemblies — assay blocked or no restored assembly: the .api tiers (the 14 folder catalogs + libs/csharp/.api/), the nuget MCP for feed ' +
  'truth, Context7/exa/tavily for source verification. ROSTER STANCE: INTEGRATION-FIRST — the four zero-consumer admissions (PicoGK, ' +
  'OcctNet.Wrapper, DSTV.Net, SharpVoronoiLib) are integration MANDATES; a removal needs a per-package redundancy/abandonment/license proof. ' +
  'Your stance is HOSTILE: the interiors are craft but the production system is naive until disk proves otherwise — NAIVETY is a defect on two ' +
  'axes, COVERAGE (thin slice of the owned domain) and APPROACH (enumerated instances where a parameterized generator belongs; rosters are seed ' +
  'DATA); the register is an initial pointer, never a ceiling.'
const ENGINE_VOCAB = 'ENGINE-NATIVE ACTIONS: the page-set table carries TWO action columns — the semantic action (KEEP/REBUILD/SPLIT/MERGE/MOVE/' +
  'DELETE/NEW) AND its rebuild-engine lowering (kind new|rebuild|improve, deletePages rows, absorb pairs {into, from}) so the durable rebuild ' +
  'engine executes the DECISION unmodified: a SPLIT lowers to N new pages + one delete-with-absorb; a MERGE lowers to one absorber + ' +
  'delete-with-absorb rows; a MOVE lowers to a new page at the destination + delete-with-absorb at the source; KEEP lowers to improve.'
const ROW_SCHEMA = 'PAGE-ROW SCHEMA (every row total): path | semantic action + engine lowering | owner charter | NAMESPACE (folder = namespace ' +
  '= fault-cluster, ratified 1:1; the six namespace ratifications execute in the same MOVE) | entry signature + FabricationPolicy/' +
  'FabricationResult case where the page grows the entry | in/out seam edges (one anchor per seam, both ledger directions; owner evaluates as ' +
  'TWO nodes — owner#atoms upstream-only, owner#run terminal) | wave. Plus: the fault-registry table (one 2700-century offset block per folder, ' +
  '+11.. onward sized by the growth law); the corrected seam ledger both directions incl. counterpart obligations and the stale re-points ' +
  '(ARCH:52,53 durable rows, ARCH:16 band, ARCH:51 write leg); the roster delta table with .api obligations (OpenCAMLib + lib3mf admission ' +
  'burdens: RID assets, golden fixtures, content-keyed wire; the re-groupings; the removes with proofs); verdict (V1-V10), evidence (E1-E14), ' +
  'and [03]-delta disposition tables; the FULL TASKLOG/IDEAS card disposition table. A blueprint leaving ANY disposition open is INCOMPLETE ' +
  'and loses at judge.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (lane, scope, rows) => [PRE, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read fully. ' +
  'Re-verify ON DISK every register row assigned: ' + rows + ' — HOLD (anchor exact), DRIFT (real defect, anchor moved — corrected anchor), or ' +
  'REFUTED (disk falsifies — cite the line). Beyond the register: sweep the NAMESPACE census (which namespace each fence declares vs its ' +
  'folder), the CONSUMER census (which admitted packages each page composes — word-boundary grep), the SEAM census (every cross-page and ' +
  'cross-package edge), both naivety axes, unwired pipeline claims, stub arms, phantom members (assay-verify where a restored assembly exists; ' +
  'assay blocked or no assembly: the folder .api catalog + the nuget MCP + two corroborating web sources). ' +
  'WRITE the dossier to ' + SCRATCH + '/dossier-' + lane + '.md (per-page verdicts + defects with anchors + charter-as-it-should-be; ' +
  'cross-cutting findings). Dense, evidence-first. NO edits outside ' + SCRATCH + '/.'].join('\n')
const PROBE_OPENCAMLIB = [PRE, 'TASK: FEASIBILITY PROBE — the OpenCAMLib binding hinge, ruling the [V5] surface-engine siting at decision time. ' +
  'The ruled default ADMITS OpenCAMLib (LGPL-2.1 relicensed, active, precompiled libocl archives) as the drop-cutter/push-cutter/waterline ' +
  'sampling engine via a thin in-house C-shim + P/Invoke — upstream ships C++-ABI archives ONLY, no C API, no C# binding, no NuGet package; ' +
  'the fallback (author-kernel drop-cutter over the kernel [V8] index/SDF lane) stands ONLY if the probe fails. Your job: surface the evidence ' +
  'NOW so the DECISION rules the hinge, never mid-leg. Method: web-research the upstream aewallin/opencamlib repository (two corroborating ' +
  'sources per claim) — the release archive contents per platform (osx-arm64/win-x64/linux presence, static vs shared, exported symbols), the ' +
  'C++ surface the shim must wrap (MillingCutter hierarchy, BatchDropCutter/PathDropCutter/Waterline entry points, STL surface loading, thread ' +
  'model), whether ANY C wrapper or FFI precedent exists (python bindings are Boost.Python/pybind11 — note what they wrap), the LGPL-2.1 ' +
  'dynamic-link posture for the shim, and the build burden (CMake targets, dependency closure). Weigh the shim (surface area, ABI risk, RID ' +
  'matrix, golden-fixture gate) against the fallback (author-kernel drop-cutter over the kernel SDF lane — materially heavier AND hostage to ' +
  'an unlanded upstream plane; state BOTH costs honestly). WRITE ' + SCRATCH + '/probe-opencamlib.md: the archive/symbol findings, the shim ' +
  'surface map, the licensing posture, the fallback cost map, and the verdict — FEASIBLE (admit as ruled), FEASIBLE_WITH_GAPS (name each gap + ' +
  'its close: a missing RID is Forge-provisioned like the kernel Z3 precedent, a missing entry point narrows the strategy set), INFEASIBLE ' +
  '(the author-kernel fallback stands, its kernel-[V8] dependency recorded as the campaign\'s calendar risk). Read-only outside ' + SCRATCH + '/.'].join('\n')
const draftPrompt = (lens, thesis, dossiers, probes) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: author ONE COMPLETE folder/page-set blueprint ' +
  'under the ' + lens + ' lens: ' + thesis + ' Read the brief in full, every survey dossier: ' + JSON.stringify(dossiers) + ', and the ' +
  'OpenCAMLib probe: ' + JSON.stringify(probes) + ' — the probe verdict is evidence the blueprint must dispose (a draft assuming FEASIBLE ' +
  'against an INFEASIBLE probe loses at judge). Honor every ruled default unless you carry disk proof a departure is stronger. The kernel ' +
  'hinges are RULED upstream and never yours to re-open: offset rails compose the [V10]a split (kernel corner-row assembly + medial; ' +
  'Fabrication pure kerf in arc space over ArcAlgebra) and skeleton consumes the [V10]d medial radius + clearance-query carriage ' +
  '(Toolpath/Skeleton.cs dies unconditionally) — carry the ruled dispositions verbatim on the affected rows, signature-locked at the seam ' +
  'against the geometry DECISION member spellings. The [V1] recommended 13-folder floor is a floor: exceed it only ' +
  'with disk proof, dilute it never. WRITE the blueprint to ' + SCRATCH + '/draft-' + lens + '.md. Self-report exact counts. NO edits outside ' +
  SCRATCH + '/.'].join('\n')
const judgePrompt = (lens, focus, drafts) => [PRE, 'TASK: ADVERSARIAL JUDGE, lens = ' + lens + '. ' + focus + ' Read the brief, the probe in ' +
  SCRATCH + '/, and all four blueprints: ' + JSON.stringify(drafts) + '. Score each: gate_dispositions (EVERY V/E/card/package disposed — ' +
  'count yourself), gate_partition (folder = namespace 1:1 with the six ratifications executed, >=2 pages per folder, every folder a growth ' +
  'axis), gate_acyclic (walk the seam graph under the two-node entry rule — owner#atoms upstream-only, owner#run terminal, cross-plane RESULT ' +
  'data on the entry vocabulary never a page-compose edge), gate_ruled (the ruled kernel dispositions composed on every affected row — the ' +
  '[V10]a kerf split, the [V10]d radius carriage with Skeleton.cs dead — signature-locked, neither re-opened nor re-implemented), capability ' +
  '0-10, collapse 0-10, elegance 0-10. A gate fail DISQUALIFIES. Name the GRAFTS precisely. ' +
  'Winner = highest capability among gate-passers; collapse then elegance tiebreak. Read-only outside ' + SCRATCH + '/.'].join('\n')
const synthPrompt = (drafts, judges) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: SYNTHESIZE the blueprint of record. Judges: ' +
  JSON.stringify(judges) + '. Blueprints: ' + JSON.stringify(drafts) + '. Base = consensus winner; apply every graft two judges named and every ' +
  'single-judge graft surviving your own disk check; re-verify the merge stays disposition-complete, partition-true, acyclic under the ' +
  'two-node rule, and ruled-carried. WRITE ' + SCRATCH + '/synthesis.md in the full row schema. Return pointer, base, grafts, open items. NO ' +
  'edits outside ' + SCRATCH + '/.'].join('\n')
const decidePrompt = (synthesis, probes) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: RED-TEAM DECIDE GATE + EMIT. Attack the synthesis at ' +
  synthesis + ': walk the seam graph and prove two-node acyclicity or break it; audit every V1-V10 disposition; spot-verify 10+ factual claims ' +
  'on disk; attack the wave-1 blast radius (the full re-partition + roster motion + entry vocabulary lands in one leg — attack its partial-' +
  'landing failure modes and the leg\'s internal ordering); attack every departure from a ruled default. An overturn re-sites or strengthens ' +
  'capability, never drops it — zero current consumers never lowers the bar. THE HINGE RULINGS: the OpenCAMLib ' +
  'surface-engine siting is YOURS, evidence-bound from ' + JSON.stringify(probes) + ' (admit-with-shim / admit-with-gap-closes / author-kernel ' +
  'fallback), recorded in probe_rulings and baked into the Toolpath/surface row; the kernel hinges ([V10]a kerf split, [V10]d radius carriage) ' +
  'are RULED upstream and never yours to re-open — verify the ruled dispositions stay composed and signature-locked against the geometry ' +
  'DECISION member spellings, and record them as carried in probe_rulings. Then ' +
  'WRITE ' + OUT + ' (repo root) carrying exactly: (a) the final folder/page-set table (full row schema, both action vocabularies, per-page ' +
  'skeletons for NEW/REBUILD pages); (b) the ratified folder=namespace=fault-cluster partition with the six namespace ratifications and the ' +
  'per-folder 2700-century offset blocks; (c) the corrected seam ledger both directions (stale re-points executed; counterpart obligations ' +
  'listed; the two-node owner discipline stated); (d) roster deltas with .api obligations (the OpenCAMLib + lib3mf admission burdens spelled: ' +
  'RID assets, golden fixtures, content-keyed wire; re-groupings; removes with their proofs); (e) the leg partition proven acyclic with ' +
  'per-leg rebuild-engine invocation rows (structure first) and the acceptance dry-runs carried forward; (f) the full TASKLOG/IDEAS card ' +
  'disposition table. Agent-facing declarative; every open item RULED, never deferred to build agents. The DECISION stands alone without ' +
  SCRATCH + '/.'].join('\n')
const minePrompt = (draft, decision) => [PRE, 'TASK: SALVAGE MINER. Compare ' + draft + ' against the WRITTEN decision ' + decision + ' (both ' +
  'fully). Hunt content the draft carried that the DECISION lacks or thins: mechanisms, charters, namespace rows, seam rows, entry cases, ' +
  'offset blocks, card dispositions. A finding names WHAT, the draft evidence, and WHERE it belongs as an owner extension — and must be ' +
  'STRONGER than what the DECISION holds. Read-only.'].join('\n')
const auditPrompt = (decision, gateRecord) => [PRE, 'TASK: OVERTURN AUDITOR. Gate record: ' + gateRecord + '. Against ' + decision + ' and the ' +
  'synthesis + drafts in ' + SCRATCH + '/, classify every overturn JUSTIFIED / OVERZEALOUS (cite the evidence) / PARTIAL. Sweep for SILENT ' +
  'drops. Spot-verify factual premises on disk, including the probe file. Read-only.'].join('\n')
const integratePrompt = (decision, findings) => [PRE, 'TASK: WRITING INTEGRATOR — the salvage close. Findings: ' + findings + '. Judge each on ' +
  'evidence; ACCEPT what is genuinely stronger (convergent findings strongest; OVERZEALOUS/PARTIAL verdicts restore corrected content), REJECT ' +
  'with the falsifying line named. Apply accepted findings IN PLACE in ' + OUT + ' as owner extensions — never a parallel section. Keep the ' +
  'DECISION disposition-complete, partition-true, acyclic, and ruled-carried after every edit.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [SURVEY]
phase('Survey')
const LANES = [
  { lane: 'process-posting', scope: 'the Process/ pages (owner, family, physics, faults, magazine) + the Posting/ pages (program, projection) ' +
    'FULLY, plus the package README/ARCHITECTURE/TASKLOG/IDEAS', rows: 'E2 (the pipeline-island half), E3, and every register anchor in your seven pages + the governance docs' },
  { lane: 'toolpath-nesting', scope: 'the Toolpath/ pages (motion, skeleton, slicing, guard, kinematics) + the Nesting/ pages (nfp, stock, ' +
    'workholding) + the Polygon/ pages (clipper, import) FULLY', rows: 'E2 (the arc-rail/wiring half), E4-class stub-arm rows, and every register anchor in your ten pages' },
  { lane: 'api-manifests', scope: 'the 14 folder-tier catalogs under libs/csharp/Rasm.Fabrication/.api/ COMPLETE + the shared substrate tier ' +
    'where the brief cites it, the Directory.Packages.props Fabrication-relevant groups, Rasm.Fabrication.csproj; verify roster claims against ' +
    'live feeds via the nuget MCP (netDxf pins, RectpackSharp redundancy, OpenCAMLib/lib3mf non-distribution facts)', rows: 'E1, and every [ROSTER_RECONCILIATION]/[04] disposition (each a register row)' },
  { lane: 'upstream-bindings', scope: 'the Fabrication-relevant clauses of all six upstream documents: RASM-CS-GEOMETRY-BRIEF.md ([V10] a-e, ' +
    '[V3], [V5], [V8], [V11], [04] re-scopes) plus RASM-CS-GEOMETRY-DECISION.md where landed (the exact kernel seam-member spellings this ' +
    'campaign signature-locks; absent: record the spelling-lock PENDING against the brief ruled text), RASM-CS-PERSISTENCE-BRIEF.md (the ' +
    'durable-rows card), RASM-CS-COMPUTE-BRIEF.md (waste rollup, ' +
    'GLB), RASM-CS-APPHOST-BRIEF.md (MTConnect/BACnet rows), RASM-CS-APPUI-BRIEF.md ([V7], HiddenLineSeam), .archive/RASM-COMPONENT-PARADIGM-DECISION.md ' +
    '(the landed 2700/stock law)', rows: 'E14, the R6 25xx census rows, and every upstream-anchored register row' },
]
const surveyors = LANES.map((l) => () =>
  agent(surveyPrompt(l.lane, l.scope, l.rows), { label: 'survey:' + l.lane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL }))
const probes = [
  () => agent(PROBE_OPENCAMLIB, { label: 'probe:opencamlib', phase: 'Survey', effort: 'max', schema: PROBE_SCHEMA, stallMs: STALL }),
]
const surveyed = (await parallel(surveyors.concat(probes))).filter(Boolean)
const dossiers = surveyed.filter((s) => s.lane)
const probeResults = surveyed.filter((s) => s.probe)
const refuted = dossiers.flatMap((d) => (d.register_verdicts || []).filter((v) => v.status !== 'HOLD').map((v) => d.lane + ':' + v.row + '=' + v.status))
log('Survey: ' + dossiers.length + '/4 dossiers + ' + probeResults.length + '/1 probe [' +
  probeResults.map((p) => p.probe + '=' + p.verdict).join(', ') + ']; register drift: ' + (refuted.length ? refuted.join('; ') : 'none'))

// --- [DRAFT]
phase('Draft')
const DOSSIER_PATHS = dossiers.map((d) => d.dossier)
const PROBE_PATHS = probeResults.map((p) => p.dossier)
const LENSES = [
  { lens: 'namespace-true-partition', thesis: 'derive the structure from the realized namespaces the folders obscure — folder = namespace = ' +
    'fault-cluster ratified 1:1, the six ratifications executed, every folder a growth axis at >=2 pages — and fit the capability into it.' },
  { lens: 'kernel-seam-first', thesis: 'work FORWARD from the five owner-to-consumer re-framings and the kernel planes ([V10] toolpath gate, ' +
    'DrawingProjection, booleans, the SDF lane, hull) — every page that consumes a kernel plane gets its seam row and signature-lock first, ' +
    'the ruled dispositions carried, and the structure follows the consumption map.' },
  { lens: 'package-mining-first', thesis: 'derive owners from what the roster can actually do — the four integration mandates (PicoGK, OcctNet, ' +
    'DSTV.Net, SharpVoronoiLib) each get their named pages/rows first, the OpenCAMLib/lib3mf admissions land with their burdens, and every [04] ' +
    'row and probe finding gets a home before structure is settled.' },
  { lens: 'output-first', thesis: 'work BACKWARD from the four flagship programs — a five-target posting suite (word-address x3 grammar-true + ' +
    'conversational + additive), a verified two-setup multi-tool machining job (spec -> plan -> guard/workholding/magazine wired -> post -> ' +
    'voxel verify -> capability), a resin/powder + FFF additive pair (implicit lane + slice stack + 3MF), and an issued setup-sheet/traveler ' +
    'package — to the exact structure that produces them.' },
]
const drafts = (await parallel(LENSES.map((l) => () =>
  agent(draftPrompt(l.lens, l.thesis, DOSSIER_PATHS, PROBE_PATHS), { label: 'draft:' + l.lens, phase: 'Draft', effort: 'max', schema: DRAFT_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Draft: ' + drafts.length + '/4 blueprints; rows [' + drafts.map((d) => d.page_rows).join(', ') + '] folders [' + drafts.map((d) => d.folders).join(', ') +
  ']; dispositions V[' + drafts.map((d) => d.verdicts_disposed).join(',') + '] E[' + drafts.map((d) => d.evidence_disposed).join(',') + '] cards[' +
  drafts.map((d) => d.cards_disposed).join(',') + '] pkgs[' + drafts.map((d) => d.packages_disposed).join(',') + ']')

// --- [JUDGE]
phase('Judge')
const DRAFT_PATHS = drafts.map((d) => d.blueprint)
const JUDGES = [
  { lens: 'law-gates', focus: 'Own the disqualifying gates hardest: dispositions counted row-by-row, the partition gate (namespace 1:1, page ' +
    'floors, the six ratifications), two-node acyclicity walked edge-by-edge, ruled-disposition carriage verified on every affected row, ' +
    'ruled-default departures without proof, probe-verdict contradictions.' },
  { lens: 'output-reach', focus: 'Own the flagship gates hardest: walk each of the four flagship programs through the draft end to end — every ' +
    'hand-off a named fence symbol pair, every kernel consumption a declared seam, every wired-pipeline claim an executing fold (Run -> Cam -> ' +
    'Guard/Workholding/Magazine -> Post) — an unreachable flagship step fails capability hard.' },
  { lens: 'partition-elegance', focus: 'Own partition quality: wave balance and the wave-1 blast radius above all, folder growth-axis honesty, ' +
    'over- vs under-splitting against the 13-folder floor, MOVE/MERGE churn honesty, offset-block sizing.' },
]
const judges = (await parallel(JUDGES.map((j) => () =>
  agent(judgePrompt(j.lens, j.focus, DRAFT_PATHS), { label: 'judge:' + j.lens, phase: 'Judge', effort: 'xhigh', schema: JUDGE_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Judge: ' + judges.length + '/3; winners [' + judges.map((j) => j.lens + '->' + j.winner).join(', ') + ']')

// --- [SYNTHESIZE]
phase('Synthesize')
const synth = await agent(synthPrompt(DRAFT_PATHS, JSON.stringify(judges)), { label: 'synthesize', phase: 'Synthesize', effort: 'max', schema: SYNTH_SCHEMA, stallMs: STALL })
if (!synth) { log('Synthesize produced nothing — aborting before Decide; resume re-runs it.'); return { dossiers: DOSSIER_PATHS, probes: PROBE_PATHS, drafts: DRAFT_PATHS, aborted: 'synthesize' } }
log('Synthesize: base=' + synth.base + ', ' + (synth.grafts_applied || []).length + ' grafts, ' + (synth.open_items || []).length + ' open items')

// --- [DECIDE]
phase('Decide')
const decided = await agent(decidePrompt(synth.synthesis, PROBE_PATHS), { label: 'decide', phase: 'Decide', effort: 'max', schema: DECIDE_SCHEMA, stallMs: STALL })
if (!decided) { log('Decide produced nothing — DECISION not written; resume re-runs it.'); return { dossiers: DOSSIER_PATHS, probes: PROBE_PATHS, synthesis: synth.synthesis, aborted: 'decide' } }
log('Decide: ' + decided.decision + ' — ' + decided.page_rows + ' rows, legs [' + (decided.legs || []).join(' | ') + '], hinge rulings [' +
  (decided.probe_rulings || []).map((r) => r.hinge + ': ' + r.ruling).join(' | ') + '], ' + (decided.overturns || []).length + ' overturn(s)')

// --- [SALVAGE]
phase('Salvage')
const mined = (await parallel(DRAFT_PATHS.map((p) => () =>
  agent(minePrompt(p, decided.decision), { label: 'mine:' + p.split('draft-').pop().replace('.md', ''), phase: 'Salvage', model: 'opus', effort: 'high', schema: MINE_SCHEMA, stallMs: STALL })))).filter(Boolean)
const audit = await agent(auditPrompt(decided.decision, JSON.stringify({ gate_record: decided.gate_record, overturns: decided.overturns })),
  { label: 'overturn-audit', phase: 'Salvage', model: 'opus', effort: 'max', schema: AUDIT_SCHEMA, stallMs: STALL })
const integrated = await agent(integratePrompt(decided.decision, JSON.stringify({ miners: mined, audit: audit || { overturn_verdicts: [], silent_drops: [] } })),
  { label: 'integrate', phase: 'Salvage', effort: 'max', schema: INTEGRATE_SCHEMA, stallMs: STALL })
log('Salvage: ' + mined.reduce((n, m) => n + (m.findings || []).length, 0) + ' mined, ' + ((audit && audit.silent_drops) || []).length +
  ' silent drop(s), applied ' + ((integrated && integrated.applied) || []).length + ' / rejected ' + ((integrated && integrated.rejected) || []).length)

return {
  decision: decided.decision, page_rows: decided.page_rows, legs: decided.legs,
  probe_verdicts: probeResults.map((p) => ({ probe: p.probe, verdict: p.verdict, gaps: p.gaps || [] })),
  probe_rulings: decided.probe_rulings, register_drift: refuted,
  judges: judges.map((j) => ({ lens: j.lens, winner: j.winner })), synthesis: synth.synthesis,
  salvageApplied: (integrated && integrated.applied) || [], salvageRejected: (integrated && integrated.rejected) || [],
  scratch: SCRATCH,
}
