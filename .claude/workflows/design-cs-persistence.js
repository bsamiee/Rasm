export const meta = {
  name: 'design-cs-persistence',
  description: 'Ephemeral Phase-1 design pass for the Persistence campaign: survey (5 plane agents re-verifying the register on disk + 1 FEASIBILITY PROBE: the V1 federation hinge — FlowtideDotNet.Substrait envelope vs the SetExpr lowering and the boundary-transcription weight) -> 4 complete structural blueprints under the brief lenses -> 3 judge lenses with FOUR disqualifying gates (verdict disposition, strata acyclicity incl. the V5 inversions, band-registry uniqueness, all four AppHost PORT seams homed) -> synthesize winner+grafts -> red-team decide gate that WRITES RASM-CS-PERSISTENCE-DECISION.md in rebuild-engine-native action vocabulary with the federation hinge ruled AT DECISION TIME from probe evidence -> salvage (4 draft miners + overturn auditor + 1 writing integrator). About 21 agents, peak concurrency 6. args = {brief, out, scratch} with defaults; a string arg overrides the brief path.',
  whenToUse: 'One-shot: turn RASM-CS-PERSISTENCE-BRIEF.md into RASM-CS-PERSISTENCE-DECISION.md — the binding page-set/band-registry/seam-ledger/roster/leg-partition blueprint the durable rebuild engine executes. Launch when the Persistence campaign opens. Delete after the campaign lands.',
  phases: [
    { title: 'Survey', detail: '5 plane surveyors (register re-verified on disk, assay over verify-or-die members) + 1 feasibility probe (the V1 federation hinge)' },
    { title: 'Draft', detail: '4 complete blueprints, one per brief lens (coordination-contract-first, federation-egress-first, perimeter-first, folder-architecture-first)' },
    { title: 'Judge', detail: '3 lenses; disqualifying gates: disposition completeness, strata acyclicity + V5 inversions, band-registry uniqueness, all four PORT seams homed' },
    { title: 'Synthesize', detail: '1 agent merges the winning blueprint with every accepted graft' },
    { title: 'Decide', detail: '1 red-team gate: attack, rule the federation hinge and the V5/V6/V11 criteria on evidence, then WRITE the DECISION (a-f contract)' },
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
  : 'RASM-CS-PERSISTENCE-BRIEF.md'
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim() : 'RASM-CS-PERSISTENCE-DECISION.md'
const SCRATCH = (argsIn && typeof argsIn === 'object' && typeof argsIn.scratch === 'string' && argsIn.scratch.trim()) ? argsIn.scratch.trim() : '.claude/scratch/design-cs-persistence'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'register_verdicts', 'key_facts'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, pages_read: { type: 'number' },
  register_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['row', 'status'], properties: { row: { type: 'string' }, status: { type: 'string', enum: ['HOLD', 'DRIFT', 'REFUTED'] }, note: { type: 'string' } } } },
  key_facts: { type: 'array', items: { type: 'string' } } } }
const PROBE_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'probe', 'verdict', 'evidence'], properties: {
  dossier: { type: 'string' }, probe: { type: 'string' },
  verdict: { type: 'string', enum: ['FEASIBLE', 'FEASIBLE_WITH_GAPS', 'INFEASIBLE'] },
  gaps: { type: 'array', items: { type: 'string' } }, evidence: { type: 'array', items: { type: 'string' } } } }
const DRAFT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'lens', 'page_rows', 'verdicts_disposed', 'evidence_disposed', 'cards_disposed', 'packages_disposed'], properties: {
  blueprint: { type: 'string' }, lens: { type: 'string' }, page_rows: { type: 'number' }, verdicts_disposed: { type: 'number' },
  evidence_disposed: { type: 'number' }, cards_disposed: { type: 'number' }, packages_disposed: { type: 'number' }, thesis: { type: 'string' } } }
const JUDGE_SCHEMA = { type: 'object', additionalProperties: false, required: ['lens', 'scores', 'winner'], properties: {
  lens: { type: 'string' }, winner: { type: 'string' }, rationale: { type: 'string' },
  scores: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['draft', 'gate_dispositions', 'gate_acyclic', 'gate_bands', 'gate_ports', 'capability', 'collapse'], properties: {
    draft: { type: 'string' }, gate_dispositions: { type: 'string', enum: ['pass', 'fail'] }, gate_acyclic: { type: 'string', enum: ['pass', 'fail'] },
    gate_bands: { type: 'string', enum: ['pass', 'fail'] }, gate_ports: { type: 'string', enum: ['pass', 'fail'] }, capability: { type: 'number' },
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
const PRE = 'Rasm monorepo. THE BRIEF ' + BRIEF + ' (repo root) is binding law — read it COMPLETELY first: [00]-[SHARED_LAW] (verdict, telos, ' +
  'structural authority with the ruled-default 24-page map, placement law, generator law, seam-and-rail law, roster reconciliation with the ' +
  'license gate), V1-V13 the binding verdicts with ruled defaults and deciding criteria, [02] the evidence register E1-E14, [03] escalation ' +
  'targets, and every remaining section. Target: libs/csharp/Rasm.Persistence/.planning/ (18 pages: Element/ graph codec identity; Version/ ' +
  'commits merge timetravel ledger retention recovery provenance; Query/ lane cypher topology columnar; Ingest/ tabular; Store/ blobstore ' +
  'provisioning cache) plus the package README/ARCHITECTURE/TASKLOG/IDEAS, BOTH .api tiers, and the central Directory.Packages.props. Upstream ' +
  'law honored, never re-planned: RASM-CS-GEOMETRY-BRIEF.md (Persistence is the kernel campaign KEY-MINTING CONSUMER; its V2 parametric ' +
  'content-identity clause carries the Persistence demand) and .archive/RASM-COMPONENT-PARADIGM-DECISION.md (frozen-wire rule, the [AMENDMENTS] ' +
  'count-prefix law, the 25xx->27xx re-band gate, the FaultBand registry shape at :141-149). docs/stacks/csharp/ governs every fence; the ' +
  'WORKSPACE_LAW strata govern placement (Persistence is APP-PLATFORM: up-only on Rasm + Rasm.Element, never a sibling AEC peer, never a host ' +
  'SDK); member verification runs uv run python -m tools.assay api over restored assemblies. Your stance is HOSTILE: the 18 fence interiors ' +
  'are strong but the PERIMETER is fiction until disk proves otherwise; the register is an initial pointer, never a ceiling. ROSTER STANCE: ' +
  'INTEGRATION-FIRST — an admitted zero-consumer package is realized as a row on its owning axis before it is ever a removal candidate; a ' +
  'removal needs a per-package redundancy/abandonment/charter proof; [V13] parameterizes the store perimeter as provider rows on closed axes, ' +
  'and only the SoR-spine seal (one event store, one materializer, one identity, one changefeed) is unchallengeable.'
const ENGINE_VOCAB = 'ENGINE-NATIVE ACTIONS: the page-set table carries TWO action columns — the semantic action (KEEP/REBUILD/SPLIT/MERGE/MOVE/' +
  'DELETE/NEW) AND its rebuild-engine lowering (kind new|rebuild|improve, deletePages rows, absorb pairs {into, from}) so the durable rebuild ' +
  'engine executes the DECISION unmodified: a SPLIT lowers to N new pages + one delete-with-absorb; a MERGE lowers to one absorber + ' +
  'delete-with-absorb rows; a MOVE lowers to a new page at the destination + delete-with-absorb at the source; KEEP lowers to improve.'
const ROW_SCHEMA = 'PAGE-ROW SCHEMA (every row total): path | semantic action + engine lowering | owner charter with its folder home, per-page ' +
  'skeleton, and registry band per V4 | entry signature (the op-union, or NONE for vocabulary pages) | in/out seam edges (one anchor per seam, ' +
  'both ledger directions) | leg. Plus: the band registry table (the re-partitioned 83xx map, duplicate-fails-at-type-init); the corrected own ' +
  'seam ledger with the thirteen stale sibling rows (COMPUTE:111,115,116,119; BIM:90,92,94,100,101,103; APPHOST:71,73,85) as counterpart ' +
  'obligations with corrected targets; the roster delta table with .api obligations per [ROSTER_RECONCILIATION]; verdict (V1-V12), evidence ' +
  '(E1-E14), and [03]-delta disposition tables; the FULL TASKLOG/IDEAS card disposition table (every phantom-realization clause corrected, ' +
  'every stale -[COMPLETE] card re-pointed). A blueprint leaving ANY disposition open is INCOMPLETE and loses at judge.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (lane, scope, rows) => [PRE, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read fully. ' +
  'Re-verify ON DISK every register row assigned: ' + rows + ' — HOLD (anchor exact), DRIFT (real defect, anchor moved — corrected anchor), or ' +
  'REFUTED (disk falsifies — cite the line). Beyond the register: both naivety axes, unwired seams, duplicate mechanisms, dead carriers, ' +
  'phantom members (assay-verify every [SEAM_AND_RAIL_LAW] verify-or-die member in your lane), band/prose drift. WRITE the dossier to ' +
  SCRATCH + '/dossier-' + lane + '.md (per-page verdicts + defects with anchors + charter-as-it-should-be; cross-cutting findings). Dense, ' +
  'evidence-first. NO edits outside ' + SCRATCH + '/.'].join('\n')
const PROBE_FEDERATION = [PRE, 'TASK: FEASIBILITY PROBE — the V1 federation hinge, the campaign-defining ruling. The ruled default REINTRODUCES ' +
  'Query/federation.md: Substrait plan bytes -> a Persistence-owned boundary transcriber (shared-tier Google.Protobuf+Grpc.Tools codegen, ' +
  'PROPS:47/:55) -> the FlowtideDotNet.Substrait managed Plan via the RelationVisitor/ExpressionVisitor fold -> lowering onto the existing ' +
  'lane#ELEMENT_SET_ALGEBRA SetExpr + the columnar/ADBC execution lane, pinned to ONE TimeCut with a (plan digest, cut, watermark) receipt. The ' +
  'retire criterion: a transcription heavier than the lowering, or a lowering that cannot land without minting a second query engine. Your job: ' +
  'surface the evidence NOW so the DECISION rules the hinge, never mid-leg. Method: read libs/csharp/Rasm.Persistence/.api/api-flowtide-' +
  'substrait.md in FULL (the :151 internal-round-trip claim is load-bearing); run uv run python -m tools.assay api over the restored ' +
  'FlowtideDotNet.Substrait assembly and verify EVERY member V1 names (SqlPlanBuilder.Sql/GetPlan visibility, the RelationVisitor/' +
  'ExpressionVisitor surface, the Plan/Relation model shapes, any public protobuf ingress); verify the shared-tier protobuf codegen admission ' +
  'and whether Substrait .proto codegen is viable on it; map the Substrait relational op set onto the SetExpr algebra (lane.md) and the ' +
  'columnar lane, naming every op with no lowering target; weigh transcriber LOC/complexity against the lowering itself. WRITE ' + SCRATCH +
  '/probe-federation.md: per-member {exists, signature, visibility}, the codegen viability, the lowering-fit map, the transcription-weight ' +
  'judgment, and the verdict — FEASIBLE (reintroduce as ruled), FEASIBLE_WITH_GAPS (name each gap + its close), INFEASIBLE (the retire ' +
  'alternative triggers NOW: cards re-scope to the ElementSet+cache substrate, FlowtideDotNet.Substrait + the ADBC drivers + Arrow.Flight ' +
  'prune, ARCH:57 re-scopes to the receipt-currency half). Read-only outside ' + SCRATCH + '/.'].join('\n')
const draftPrompt = (lens, thesis, dossiers, probes) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: author ONE COMPLETE structural blueprint under ' +
  'the ' + lens + ' lens: ' + thesis + ' Read the brief in full, every survey dossier: ' + JSON.stringify(dossiers) + ', and the federation ' +
  'probe: ' + JSON.stringify(probes) + ' — the probe verdict is evidence the blueprint must dispose (a draft assuming FEASIBLE against an ' +
  'INFEASIBLE probe loses at judge). Honor every ruled default unless you carry disk proof a departure is stronger; every V5/V6/V11 deciding ' +
  'criterion resolves to a ruling, never a hedge. All four AppHost PORT contracts must be HOMED on Persistence-owned types — walk each to its ' +
  'owning page and op-union case. WRITE the blueprint to ' + SCRATCH + '/draft-' + lens + '.md. Self-report exact counts. NO edits outside ' +
  SCRATCH + '/.'].join('\n')
const judgePrompt = (lens, focus, drafts) => [PRE, 'TASK: ADVERSARIAL JUDGE, lens = ' + lens + '. ' + focus + ' Read the brief, the probe in ' +
  SCRATCH + '/, and all four blueprints: ' + JSON.stringify(drafts) + '. Score each: gate_dispositions (EVERY V/E/card/package disposed — ' +
  'count yourself), gate_acyclic (walk the seam ledger + strata: zero upward edges, zero page cycles, the four V5 c/d inversions explicitly ' +
  'disposed under the frozen-contract ruling), gate_bands (every fault union and loose receipt code lands a unique registry row; the three ' +
  'collisions re-partitioned; the GraphFault simple-name collision resolved), gate_ports (all four AppHost PORT contracts homed — fenced ' +
  'Budget debit, step-state CAS, transactional outbox, CAS+lease+membership), capability 0-10, collapse 0-10, elegance 0-10. A gate fail ' +
  'DISQUALIFIES. Name the GRAFTS precisely. Winner = highest capability among gate-passers; collapse then elegance tiebreak. Read-only ' +
  'outside ' + SCRATCH + '/.'].join('\n')
const synthPrompt = (drafts, judges) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: SYNTHESIZE the blueprint of record. Judges: ' +
  JSON.stringify(judges) + '. Blueprints: ' + JSON.stringify(drafts) + '. Base = consensus winner; apply every graft two judges named and every ' +
  'single-judge graft surviving your own disk check; re-verify the merge stays acyclic, disposition-complete, band-unique, and PORT-complete. ' +
  'WRITE ' + SCRATCH + '/synthesis.md in the full row schema. Return pointer, base, grafts, open items. NO edits outside ' + SCRATCH + '/.'].join('\n')
const decidePrompt = (synthesis, probes) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: RED-TEAM DECIDE GATE + EMIT. Attack the synthesis at ' +
  synthesis + ': walk the ledger and prove acyclicity or break it; audit every V1-V13 disposition; spot-verify 10+ factual claims on disk; ' +
  'attack the leg partition for blast radius (leg 1 carries the roster prune — attack its reversibility); attack every departure from a ruled ' +
  'default. THE HINGE RULINGS ARE YOURS, evidence-bound, recorded in probe_rulings and baked into the affected rows: the V1 federation hinge ' +
  '(reintroduce as ruled / reintroduce with gap-closes / retire) from ' + JSON.stringify(probes) + '; the V5 frozen-contract ruling across all ' +
  'four inversions; the V6 EF-commit + embedded-boundary + pgvector-lane criteria; the V11 schedule-consumer criterion. Then WRITE ' + OUT +
  ' (repo root) carrying exactly the brief [01] emit contract: (a) the final page-set table (full row schema, both action vocabularies, ' +
  'per-page skeletons); (b) the band registry with the re-partitioned 83xx map; (c) the corrected seam ledger with the thirteen stale sibling ' +
  'rows as counterpart obligations; (d) roster deltas with .api obligations (prunes, commits, the recorded rejections carried forward); (e) ' +
  'the leg partition proven acyclic with per-leg rebuild-engine invocation rows; (f) the full TASKLOG/IDEAS card disposition table. ' +
  'Agent-facing declarative; every open item RULED. The DECISION stands alone without ' + SCRATCH + '/.'].join('\n')
const minePrompt = (draft, decision) => [PRE, 'TASK: SALVAGE MINER. Compare ' + draft + ' against the WRITTEN decision ' + decision + ' (both ' +
  'fully). Hunt content the draft carried that the DECISION lacks or thins: mechanisms, charters, seam rows, PORT homings, band rows, card ' +
  'dispositions. A finding names WHAT, the draft evidence, and WHERE it belongs as an owner extension — and must be STRONGER than what the ' +
  'DECISION holds. Read-only.'].join('\n')
const auditPrompt = (decision, gateRecord) => [PRE, 'TASK: OVERTURN AUDITOR. Gate record: ' + gateRecord + '. Against ' + decision + ' and the ' +
  'synthesis + drafts in ' + SCRATCH + '/, classify every overturn JUSTIFIED / OVERZEALOUS (cite the evidence) / PARTIAL. Sweep for SILENT ' +
  'drops. Spot-verify factual premises on disk, including the probe file. Read-only.'].join('\n')
const integratePrompt = (decision, findings) => [PRE, 'TASK: WRITING INTEGRATOR — the salvage close. Findings: ' + findings + '. Judge each on ' +
  'evidence; ACCEPT what is genuinely stronger (convergent findings strongest; OVERZEALOUS/PARTIAL verdicts restore corrected content), REJECT ' +
  'with the falsifying line named. Apply accepted findings IN PLACE in ' + OUT + ' as owner extensions — never a parallel section. Keep the ' +
  'DECISION disposition-complete, acyclic, band-unique, and PORT-complete after every edit.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [SURVEY]
phase('Survey')
const LANES = [
  { lane: 'element-store', scope: 'the Element/ pages (graph, codec, identity) + the Store/ pages (blobstore, provisioning, cache) FULLY, plus ' +
    'the package README/ARCHITECTURE/TASKLOG/IDEAS', rows: 'E5 (the Persistence side), E7, E9 (the blobstore/codec halves), E12, E14, and every register anchor in your six pages' },
  { lane: 'version', scope: 'the Version/ pages (commits, merge, timetravel, ledger, retention, recovery, provenance) FULLY', rows: 'E8, E9 (the ' +
    'ledger/timetravel/retention halves), E10 (the merge side), E11, and every register anchor in your seven pages' },
  { lane: 'query-ingest', scope: 'the Query/ pages (lane, cypher, topology, columnar) + Ingest/tabular.md FULLY', rows: 'E4, E13, E14 (the ' +
    'columnar side), and every register anchor in your five pages' },
  { lane: 'api-manifests', scope: 'BOTH .api tiers COMPLETE (the ~77 package catalogs under libs/csharp/Rasm.Persistence/.api/ + the ~30 shared ' +
    'substrate catalogs under libs/csharp/.api/), the Directory.Packages.props Persistence block, Rasm.Persistence.csproj; verify roster claims ' +
    'against live feeds via the nuget MCP where the brief cites versions', rows: 'E2, E3, and every [ROSTER_RECONCILIATION] disposition (each a register row)' },
  { lane: 'sibling-ledgers', scope: 'the cross-package anchors the brief cites: Rasm.AppHost/ARCHITECTURE.md (the four PORT rows + the two stale ' +
    'rows + :85), Rasm.Compute/ARCHITECTURE.md, Rasm.Bim/ARCHITECTURE.md, the Rasm.Element seam hooks (Graph/element.md, Graph/delta.md, ' +
    'Projection/address.md), RASM-CS-GEOMETRY-BRIEF.md (V2 + E12), and the .archive/RASM-COMPONENT-PARADIGM-DECISION.md amendments + re-band gate',
    rows: 'E1, E5 (the AppHost side), E6, E10 (the Element side), E12, and every sibling-anchored register row' },
]
const surveyors = LANES.map((l) => () =>
  agent(surveyPrompt(l.lane, l.scope, l.rows), { label: 'survey:' + l.lane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL }))
const probes = [
  () => agent(PROBE_FEDERATION, { label: 'probe:federation', phase: 'Survey', effort: 'max', schema: PROBE_SCHEMA, stallMs: STALL }),
]
const surveyed = (await parallel(surveyors.concat(probes))).filter(Boolean)
const dossiers = surveyed.filter((s) => s.lane)
const probeResults = surveyed.filter((s) => s.probe)
const refuted = dossiers.flatMap((d) => (d.register_verdicts || []).filter((v) => v.status !== 'HOLD').map((v) => d.lane + ':' + v.row + '=' + v.status))
log('Survey: ' + dossiers.length + '/5 dossiers + ' + probeResults.length + '/1 probe [' +
  probeResults.map((p) => p.probe + '=' + p.verdict).join(', ') + ']; register drift: ' + (refuted.length ? refuted.join('; ') : 'none'))

// --- [DRAFT]
phase('Draft')
const DOSSIER_PATHS = dossiers.map((d) => d.dossier)
const PROBE_PATHS = probeResults.map((p) => p.dossier)
const LENSES = [
  { lens: 'coordination-contract-first', thesis: 'work BACKWARD from the four AppHost PORT contracts — the token-VALIDATING fenced-lease store, ' +
    'the fenced Budget debit and step-state CAS as op-union cases, membership rows, the NAMED outbox and its egress pump — to the exact ' +
    'Store/Version structure that serves them.' },
  { lens: 'federation-egress-first', thesis: 'work BACKWARD from the cross-runtime wire seams — the Substrait plan ingress onto SetExpr, the ' +
    'content-addressed CloudEvents egress envelope with per-sink dedup honesty, the AsOfKey icechunk seam, the CrdtOpWire bit-parity law — to ' +
    'the Query/Version owners that carry them.' },
  { lens: 'perimeter-first', thesis: 'derive the structure from perimeter truth — the type-enforced band registry, a seam ledger where every ' +
    'row composes in a fence and every crossing has a row, catalogs anchored to live pages, a zero-orphan manifest, a card pool whose every ' +
    'claim resolves on disk — and fit the capability inside it.' },
  { lens: 'folder-architecture-first', thesis: 'derive the structure from the folder law — no one-file folders, every folder a growth axis, the ' +
    'ruled-default 24-page map (Element 4, Version 8, Query 7, Ingest 2, Store 3) — and fit the capability into it.' },
]
const drafts = (await parallel(LENSES.map((l) => () =>
  agent(draftPrompt(l.lens, l.thesis, DOSSIER_PATHS, PROBE_PATHS), { label: 'draft:' + l.lens, phase: 'Draft', effort: 'max', schema: DRAFT_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Draft: ' + drafts.length + '/4 blueprints; rows [' + drafts.map((d) => d.page_rows).join(', ') + ']; dispositions V[' +
  drafts.map((d) => d.verdicts_disposed).join(',') + '] E[' + drafts.map((d) => d.evidence_disposed).join(',') + '] cards[' +
  drafts.map((d) => d.cards_disposed).join(',') + '] pkgs[' + drafts.map((d) => d.packages_disposed).join(',') + ']')

// --- [JUDGE]
phase('Judge')
const DRAFT_PATHS = drafts.map((d) => d.blueprint)
const JUDGES = [
  { lens: 'law-gates', focus: 'Own the disqualifying gates hardest: dispositions counted row-by-row, acyclicity + the four V5 inversions walked ' +
    'edge-by-edge, band uniqueness proven, ruled-default departures without proof, probe-verdict contradictions.' },
  { lens: 'contract-reach', focus: 'Own the contract gates hardest: walk EVERY AppHost PORT item to its owning page and op-union case; walk the ' +
    'federation/egress/AsOfKey wire seams end to end; verify the cross-runtime parity laws (CrdtOpWire, count-prefix, XxHash128 seed) survive ' +
    'every draft intact.' },
  { lens: 'partition-elegance', focus: 'Own partition quality: leg balance and blast radius (the leg-1 prune above all), folder-law crispness ' +
    'against the 24-page ruled default, over- vs under-splitting, MOVE/MERGE churn honesty.' },
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
