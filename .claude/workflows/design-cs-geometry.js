export const meta = {
  name: 'design-cs-geometry',
  description: 'Ephemeral Phase-1 design pass for the kernel geometry campaign: survey (5 plane agents + 2 FEASIBILITY PROBES: GShark envelope vs the V2 op roster, and the V4/V10c implicit-point Tessellation hinge) -> 4 complete structural blueprints under the brief lenses -> 3 judge lenses with disqualifying gates (verdict disposition, strata acyclicity, BOTH consumer gates homed) -> synthesize winner+grafts -> red-team decide gate that WRITES RASM-CS-GEOMETRY-DECISION.md in rebuild-engine-native action vocabulary with the vendor-and-own and implicit-point rulings made AT DECISION TIME from probe evidence -> salvage (4 draft miners + overturn auditor + 1 writing integrator). About 22 agents, peak concurrency 7. args = {brief, out, scratch} with defaults; a string arg overrides the brief path.',
  whenToUse: 'One-shot: turn RASM-CS-GEOMETRY-BRIEF.md into RASM-CS-GEOMETRY-DECISION.md — the binding page-set/namespace/fault-cluster/seam-ledger/leg-partition blueprint the durable rebuild engine executes. Run before or after the kernel conversion, never mid-conversion. Delete after the campaign lands.',
  phases: [
    { title: 'Survey', detail: '5 plane surveyors (register re-verified on disk) + 2 feasibility probes (GShark envelope, implicit-point hinge)' },
    { title: 'Draft', detail: '4 complete blueprints, one per brief lens (floor-maximalist, consumer-gate-first, roster-mining-first, folder-architecture-first)' },
    { title: 'Judge', detail: '3 lenses; disqualifying gates: disposition completeness, strata acyclicity, both consumer gates fully homed' },
    { title: 'Synthesize', detail: '1 agent merges the winning blueprint with every accepted graft' },
    { title: 'Decide', detail: '1 red-team gate: attack, rule the probe-hinged defaults on evidence, then WRITE the DECISION (a-f contract)' },
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
  : 'RASM-CS-GEOMETRY-BRIEF.md'
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim() : 'RASM-CS-GEOMETRY-DECISION.md'
const SCRATCH = (argsIn && typeof argsIn === 'object' && typeof argsIn.scratch === 'string' && argsIn.scratch.trim()) ? argsIn.scratch.trim() : '.claude/scratch/design-cs-geometry'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'register_verdicts', 'key_facts'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, pages_read: { type: 'number' },
  register_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['row', 'status'], properties: { row: { type: 'string' }, status: { type: 'string', enum: ['HOLD', 'DRIFT', 'REFUTED'] }, note: { type: 'string' } } } },
  key_facts: { type: 'array', items: { type: 'string' } } } }
const PROBE_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'probe', 'verdict', 'evidence'], properties: {
  dossier: { type: 'string' }, probe: { type: 'string' },
  verdict: { type: 'string', enum: ['FEASIBLE', 'FEASIBLE_WITH_GAPS', 'INFEASIBLE'] },
  gaps: { type: 'array', items: { type: 'string' } }, evidence: { type: 'array', items: { type: 'string' } } } }
const DRAFT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'lens', 'page_rows', 'verdicts_disposed', 'evidence_disposed', 'deltas_disposed'], properties: {
  blueprint: { type: 'string' }, lens: { type: 'string' }, page_rows: { type: 'number' }, verdicts_disposed: { type: 'number' },
  evidence_disposed: { type: 'number' }, deltas_disposed: { type: 'number' }, thesis: { type: 'string' } } }
const JUDGE_SCHEMA = { type: 'object', additionalProperties: false, required: ['lens', 'scores', 'winner'], properties: {
  lens: { type: 'string' }, winner: { type: 'string' }, rationale: { type: 'string' },
  scores: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['draft', 'gate_dispositions', 'gate_acyclic', 'gate_consumers', 'capability', 'collapse'], properties: {
    draft: { type: 'string' }, gate_dispositions: { type: 'string', enum: ['pass', 'fail'] }, gate_acyclic: { type: 'string', enum: ['pass', 'fail'] },
    gate_consumers: { type: 'string', enum: ['pass', 'fail'] }, capability: { type: 'number' }, collapse: { type: 'number' },
    elegance: { type: 'number' }, grafts: { type: 'array', items: { type: 'string' } } } } } } }
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
const PRE = 'Rasm monorepo. THE BRIEF ' + BRIEF + ' (repo root) is binding law — read it COMPLETELY first: [00]-[SHARED_LAW], V1-V14 the binding ' +
  'verdicts with ruled defaults, [02] the evidence register E1-E15, [03] escalation targets, [04] package pressure with the roster rulings ' +
  '(Manifold in-house manifoldc P/Invoke ADD, five orphan REMOVEs, GShark vendor-and-own fallback, the named-and-rejected records), [05] build ' +
  'legs, [06] out-of-scope. Target: the kernel geometry planning corpus (its .planning home is wherever it lives on disk TODAY — resolve it, do ' +
  'not assume; the kernel conversion may have re-homed it). docs/stacks/csharp/ + the relevant domain shards govern every fence; the WORKSPACE_LAW ' +
  'strata govern placement; member verification runs uv run python -m tools.assay api over restored assemblies where source exists, and reads ' +
  'planning fences as truth where the conversion has replaced source. Both consumer gates are load-bearing law: the Generation KERNEL GATE ' +
  '(RASM-GENERATION-SPEC.md — SpineRef stations/frames, isolines, geodesics, subdivision, panelization, pattern-to-surface) and the Fabrication ' +
  'toolpath gate (corner-strategy offsets, slice stacks, medial-with-clearance-radius, developables, manufacturing-tolerance booleans). Your ' +
  'stance is HOSTILE: the corpus is naive until disk proves otherwise; the register is an initial pointer, never a ceiling.'
const ENGINE_VOCAB = 'ENGINE-NATIVE ACTIONS: the page-set table carries TWO action columns — the semantic action (KEEP/REBUILD/SPLIT/MERGE/MOVE/' +
  'DELETE/NEW) AND its rebuild-engine lowering (kind new|rebuild|improve, deletePages rows, absorb pairs {into, from}) so the durable rebuild ' +
  'engine executes the DECISION unmodified: a SPLIT lowers to N new pages + one delete-with-absorb; a MERGE lowers to one absorber + ' +
  'delete-with-absorb rows; a MOVE lowers to a new page at the destination + delete-with-absorb at the source; KEEP lowers to improve.'
const ROW_SCHEMA = 'PAGE-ROW SCHEMA (every row total): path | semantic action + engine lowering | owner charter with its TIER-1 folder, TIER-2 ' +
  'namespace, and fault cluster per V1/V12 | entry signature (the Apply op-union, or NONE for vocabulary pages) | in/out seam edges (one anchor ' +
  'per seam, both ledger directions) | wave. Plus: the ratified folder/namespace/fault-cluster partition; the corrected seam ledger including ' +
  'every counterpart-package ledger edit; verdict (V1-V14), evidence (E1-E15), and [03]-delta disposition tables. A blueprint leaving ANY ' +
  'disposition open is INCOMPLETE and loses at judge.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (lane, scope, rows) => [PRE, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read fully. ' +
  'Re-verify ON DISK every register row assigned: ' + rows + ' — HOLD (anchor exact), DRIFT (real defect, anchor moved — corrected anchor), or ' +
  'REFUTED (disk falsifies — cite the line). Beyond the register: both naivety axes, unwired seams, duplicate mechanisms, dead code, phantom ' +
  'members (assay-verify), namespace/fault-cluster drift. WRITE the dossier to ' + SCRATCH + '/dossier-' + lane + '.md (per-page verdicts + ' +
  'defects with anchors + charter-as-it-should-be; cross-cutting findings). Dense, evidence-first. NO edits outside ' + SCRATCH + '/.'].join('\n')
const PROBE_GSHARK = [PRE, 'TASK: FEASIBILITY PROBE — the GShark envelope vs the V2 parametric-tier op roster. The V2 floor rides GShark ' +
  '(dormant since 2023-08) far beyond its proven envelope; the brief rules a vendor-and-own fallback IF gaps surface. Your job: surface them ' +
  'NOW so the DECISION rules from evidence, never mid-leg. Method: read the GShark .api catalog in FULL; run uv run python -m tools.assay api ' +
  'over the restored GShark assembly and verify EVERY member the V2 floor names (Divide maxSegmentLength/equalSegmentLengths, ParameterAtLength, ' +
  'PointAtLength, PerpendicularFrames, IsoCurve, ClosestParameter, normalized-domain conventions, NURBS surface evaluation depth, knot/degree/' +
  'weight access for the canonical-bytes projection) plus every member the V2 NEW pages (surface/subdivide/develop/panelize/patternmap) would ' +
  'plausibly compose; probe numerical-quality signals (issue tracker via web research where useful, two-source law). WRITE ' + SCRATCH +
  '/probe-gshark.md: per-op {member, exists, signature, envelope notes}, the gap list, and the verdict — FEASIBLE (floor lands on GShark as-is), ' +
  'FEASIBLE_WITH_GAPS (name each gap + whether vendor-and-own or an owned kernel op closes it), INFEASIBLE (vendor-and-own triggers NOW at ' +
  'DECISION time). Read-only outside ' + SCRATCH + '/.'].join('\n')
const PROBE_IMPLICIT = [PRE, 'TASK: FEASIBILITY PROBE — the V4/V10c implicit-point hinge. Three verdicts share one open premise: can ' +
  'Tessellation host implicit-coordinate vertices (the Lpi crossing) through constraint recovery, so operand crossings ride the exact path ' +
  '(V4 ruled default), the medial Voronoi-dual accessor reconciles (V10c), and slice-stack orientation guarantees hold. Method: deep-read the ' +
  'delaunay, arrangement, intersect, and predicates pages (the corpus wherever it lives on disk) at algorithm depth — the constraint-recovery ' +
  'walk, the vertex representation, the exact-arithmetic lattice, where a rounded coordinate first enters; consult the exact-computation ' +
  'literature via web research where the pages leave the question open (two-source law; Shewchuk/CGAL-style implicit-point treatments are the ' +
  'reference frame). WRITE ' + SCRATCH + '/probe-implicit.md: the mechanism as designed, the exact insertion points where implicit coordinates ' +
  'must survive, what breaks first, and the verdict — FEASIBLE / FEASIBLE_WITH_GAPS (+ the minimal re-scope) / INFEASIBLE (the honesty re-scope ' +
  'on both pages is the ruling). Read-only outside ' + SCRATCH + '/.'].join('\n')
const draftPrompt = (lens, thesis, dossiers, probes) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: author ONE COMPLETE structural blueprint under ' +
  'the ' + lens + ' lens: ' + thesis + ' Read the brief in full, every survey dossier: ' + JSON.stringify(dossiers) + ', and BOTH feasibility ' +
  'probes: ' + JSON.stringify(probes) + ' — probe verdicts are evidence the blueprint must dispose (a draft assuming FEASIBLE against an ' +
  'INFEASIBLE probe loses at judge). Honor every ruled default unless you carry disk proof a departure is stronger. Both consumer gates must be ' +
  'FULLY HOMED — every Generation KERNEL GATE item and every Fabrication toolpath demand names its owning page and seam. WRITE the blueprint to ' +
  SCRATCH + '/draft-' + lens + '.md. Self-report exact counts. NO edits outside ' + SCRATCH + '/.'].join('\n')
const judgePrompt = (lens, focus, drafts) => [PRE, 'TASK: ADVERSARIAL JUDGE, lens = ' + lens + '. ' + focus + ' Read the brief, both probes in ' +
  SCRATCH + '/, and all four blueprints: ' + JSON.stringify(drafts) + '. Score each: gate_dispositions (EVERY V/E/delta disposed — count ' +
  'yourself), gate_acyclic (walk the seam ledger + strata: zero upward edges, zero page cycles), gate_consumers (BOTH gates fully homed — walk ' +
  'each gate item to its owning page), capability 0-10, collapse 0-10, elegance 0-10. A gate fail DISQUALIFIES. Name the GRAFTS precisely. ' +
  'Winner = highest capability among gate-passers; collapse then elegance tiebreak. Read-only outside ' + SCRATCH + '/.'].join('\n')
const synthPrompt = (drafts, judges) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: SYNTHESIZE the blueprint of record. Judges: ' +
  JSON.stringify(judges) + '. Blueprints: ' + JSON.stringify(drafts) + '. Base = consensus winner; apply every graft two judges named and every ' +
  'single-judge graft surviving your own disk check; re-verify the merge stays acyclic, disposition-complete, and consumer-gate-complete. WRITE ' +
  SCRATCH + '/synthesis.md in the full row schema. Return pointer, base, grafts, open items. NO edits outside ' + SCRATCH + '/.'].join('\n')
const decidePrompt = (synthesis, probes) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: RED-TEAM DECIDE GATE + EMIT. Attack the synthesis at ' +
  synthesis + ': walk the ledger and prove acyclicity or break it; audit every V1-V14 disposition; spot-verify 10+ factual claims on disk; ' +
  'attack the leg partition for blast radius; attack every departure from a ruled default. THE PROBE RULINGS ARE YOURS: from ' +
  JSON.stringify(probes) + ' rule the GShark hinge (floor-as-is / vendor-and-own NOW / per-gap owned ops) and the implicit-point hinge ' +
  '(V4 exact path / honesty re-scope cascade across V4+V10c) — evidence-bound, recorded in probe_rulings, baked into the affected page rows. ' +
  'Then WRITE ' + OUT + ' (repo root) carrying exactly: (a) the final page-set table (full row schema, both action vocabularies); (b) the ' +
  'ratified folder/namespace/fault-cluster partition; (c) the corrected seam ledger incl. every counterpart-package ledger edit as explicit ' +
  'rows; (d) roster deltas with .api obligations (Manifold P/Invoke charter, removals, the named-and-rejected records carried forward); (e) the ' +
  'leg partition proven acyclic with per-leg rebuild-engine invocation rows; (f) the [T-BOOLEAN-NATIVE-ASSET] card disposition + the probe ' +
  'rulings as standing law. Agent-facing declarative; every open item RULED. The DECISION stands alone without ' + SCRATCH + '/.'].join('\n')
const minePrompt = (draft, decision) => [PRE, 'TASK: SALVAGE MINER. Compare ' + draft + ' against the WRITTEN decision ' + decision + ' (both ' +
  'fully). Hunt content the draft carried that the DECISION lacks or thins: mechanisms, charters, seam rows, gate homings, probe dispositions. ' +
  'A finding names WHAT, the draft evidence, and WHERE it belongs as an owner extension — and must be STRONGER than what the DECISION holds. ' +
  'Read-only.'].join('\n')
const auditPrompt = (decision, gateRecord) => [PRE, 'TASK: OVERTURN AUDITOR. Gate record: ' + gateRecord + '. Against ' + decision + ' and the ' +
  'synthesis + drafts in ' + SCRATCH + '/, classify every overturn JUSTIFIED / OVERZEALOUS (cite the evidence) / PARTIAL. Sweep for SILENT ' +
  'drops. Spot-verify factual premises on disk, including both probe files. Read-only.'].join('\n')
const integratePrompt = (decision, findings) => [PRE, 'TASK: WRITING INTEGRATOR — the salvage close. Findings: ' + findings + '. Judge each on ' +
  'evidence; ACCEPT what is genuinely stronger (convergent findings strongest; OVERZEALOUS/PARTIAL verdicts restore corrected content), REJECT ' +
  'with the falsifying line named. Apply accepted findings IN PLACE in ' + OUT + ' as owner extensions — never a parallel section. Keep the ' +
  'DECISION disposition-complete, acyclic, and consumer-gate-complete after every edit.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [SURVEY]
phase('Survey')
const LANES = [
  { lane: 'numerics-spatial', scope: 'the Numerics + Spatial planning pages (predicates, faults, index, naming, reconciliation) FULLY, plus the kernel ' +
    'index docs', rows: 'the E-rows anchored in predicates/faults/index/naming/reconciliation' },
  { lane: 'meshing', scope: 'the Meshing planning pages (delaunay, intersect, arrangement, offset, and peers) FULLY', rows: 'the E-rows anchored in ' +
    'the Meshing pages' },
  { lane: 'processing', scope: 'the Processing planning pages (repair, receipts, decimate, flatten, solver, fit) FULLY', rows: 'the E-rows anchored in ' +
    'the Processing pages' },
  { lane: 'parametric-drawing-gates', scope: 'the Parametric + Drawing planning pages (curve, view, pack) FULLY, plus RASM-GENERATION-SPEC.md (the ' +
    'KERNEL GATE + SpineRef law) and the Fabrication ledger rows the brief cites', rows: 'the E-rows anchored in curve/view/pack + both consumer-gate citations' },
  { lane: 'api-manifests', scope: 'BOTH .api tiers COMPLETE (libs/csharp/.api/ + the kernel package .api/), Directory.Packages.props, the kernel ' +
    'csproj; verify every [04] roster claim against feeds via the nuget MCP where the brief cites versions', rows: 'every [04] stub/feed anchor (each a register row)' },
]
const surveyors = LANES.map((l) => () =>
  agent(surveyPrompt(l.lane, l.scope, l.rows), { label: 'survey:' + l.lane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL }))
const probes = [
  () => agent(PROBE_GSHARK, { label: 'probe:gshark', phase: 'Survey', effort: 'max', schema: PROBE_SCHEMA, stallMs: STALL }),
  () => agent(PROBE_IMPLICIT, { label: 'probe:implicit-point', phase: 'Survey', effort: 'max', schema: PROBE_SCHEMA, stallMs: STALL }),
]
const surveyed = (await parallel(surveyors.concat(probes))).filter(Boolean)
const dossiers = surveyed.filter((s) => s.lane)
const probeResults = surveyed.filter((s) => s.probe)
const refuted = dossiers.flatMap((d) => (d.register_verdicts || []).filter((v) => v.status !== 'HOLD').map((v) => d.lane + ':' + v.row + '=' + v.status))
log('Survey: ' + dossiers.length + '/5 dossiers + ' + probeResults.length + '/2 probes [' +
  probeResults.map((p) => p.probe + '=' + p.verdict).join(', ') + ']; register drift: ' + (refuted.length ? refuted.join('; ') : 'none'))

// --- [DRAFT]
phase('Draft')
const DOSSIER_PATHS = dossiers.map((d) => d.dossier)
const PROBE_PATHS = probeResults.map((p) => p.dossier)
const LENSES = [
  { lens: 'floor-maximalist', thesis: 'push every recommended-shape floor to its strongest form — the deepest exact-lattice, parametric, and toolpath ' +
    'owners the evidence justifies.' },
  { lens: 'consumer-gate-first', thesis: 'work BACKWARD from the two gates — the Generation SpineRef placement chain and a full Fabrication toolpath ' +
    'program (slice -> offset -> clearance -> post) — to the exact structure that serves both.' },
  { lens: 'roster-mining-first', thesis: 'derive owners from what the surviving roster + Vectors capabilities can actually do — every [04] row and ' +
    'probe finding gets a home first.' },
  { lens: 'folder-architecture-first', thesis: 'derive the structure from the V1 two-tier ratification — folders as growth-conducive domain groups, ' +
    'namespaces as concepts 1:1 with fault sub-bands — and fit the capability into it.' },
]
const drafts = (await parallel(LENSES.map((l) => () =>
  agent(draftPrompt(l.lens, l.thesis, DOSSIER_PATHS, PROBE_PATHS), { label: 'draft:' + l.lens, phase: 'Draft', effort: 'max', schema: DRAFT_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Draft: ' + drafts.length + '/4 blueprints; rows [' + drafts.map((d) => d.page_rows).join(', ') + ']; dispositions V[' +
  drafts.map((d) => d.verdicts_disposed).join(',') + '] E[' + drafts.map((d) => d.evidence_disposed).join(',') + ']')

// --- [JUDGE]
phase('Judge')
const DRAFT_PATHS = drafts.map((d) => d.blueprint)
const JUDGES = [
  { lens: 'law-gates', focus: 'Own the disqualifying gates hardest: dispositions counted row-by-row, acyclicity walked edge-by-edge, tier law, ' +
    'ruled-default departures without proof, probe-verdict contradictions.' },
  { lens: 'consumer-reach', focus: 'Own the consumer gates hardest: walk EVERY Generation KERNEL GATE item and EVERY Fabrication toolpath demand ' +
    'through the draft to its owning page and seam — an unhomed item fails the gate.' },
  { lens: 'partition-elegance', focus: 'Own partition quality: wave balance, folder/namespace boundary crispness under V1, blast radius of every ' +
    'MOVE/MERGE, over- vs under-splitting.' },
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
log('Decide: ' + decided.decision + ' — ' + decided.page_rows + ' rows, legs [' + (decided.legs || []).join(' | ') + '], probe rulings [' +
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
