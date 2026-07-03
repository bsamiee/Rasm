export const meta = {
  name: 'design-artifacts-restructure',
  description: 'Ephemeral Phase-1 design pass for the artifacts restructure campaign: survey (5 agents: foundations / mid / AEC+egress planes + both .api tiers + import census) -> 4 complete structural blueprints under the brief lenses -> 3 judge lenses with disqualifying gates -> synthesize winner+grafts -> red-team decide gate that WRITES RASM-PY-ARTIFACTS-DECISION.md in rebuild-engine-native action vocabulary -> salvage (4 draft miners + overturn auditor + 1 writing integrator). About 20 agents, peak concurrency 5. args = {brief, out, scratch} with defaults; a string arg overrides the brief path.',
  whenToUse: 'One-shot: turn RASM-PY-ARTIFACTS-BRIEF.md into RASM-PY-ARTIFACTS-DECISION.md — the binding page-set/seam-ledger/leg-partition blueprint the durable rebuild engine then executes. Delete after the campaign lands.',
  phases: [
    { title: 'Survey', detail: '5 read-only agents: 3 corpus planes + both .api tiers + cross-domain import census; every evidence-register row re-verified on disk' },
    { title: 'Draft', detail: '4 complete page-set blueprints, one per brief lens (foundations-maximalist, entry-seam-first, package-mining-first, output-first)' },
    { title: 'Judge', detail: '3 lenses: law gates (disqualifying), capability reach vs [03], partition elegance/blast-radius; grafts named' },
    { title: 'Synthesize', detail: '1 agent merges the winning blueprint with every accepted graft into the blueprint of record' },
    { title: 'Decide', detail: '1 red-team gate: attack the synthesis, then WRITE the DECISION (a-f contract, engine-native actions, acyclic partition)' },
    { title: 'Salvage', detail: '4 per-draft miners + 1 overturn auditor against the WRITTEN decision, then 1 writing integrator applying accepted findings in place' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const BRIEF = (typeof argsIn === 'string' && argsIn.trim()) ? argsIn.trim()
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.brief === 'string' && argsIn.brief.trim()) ? argsIn.brief.trim()
  : 'RASM-PY-ARTIFACTS-BRIEF.md'
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim() : 'RASM-PY-ARTIFACTS-DECISION.md'
const SCRATCH = (argsIn && typeof argsIn === 'object' && typeof argsIn.scratch === 'string' && argsIn.scratch.trim()) ? argsIn.scratch.trim() : '.claude/scratch/design-artifacts'
const ROOT = 'libs/python/artifacts'

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'plane', 'register_verdicts', 'key_facts'], properties: {
  dossier: { type: 'string' }, plane: { type: 'string' }, pages_read: { type: 'number' },
  register_verdicts: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['row', 'status'], properties: { row: { type: 'string' }, status: { type: 'string', enum: ['HOLD', 'DRIFT', 'REFUTED'] }, note: { type: 'string' } } } },
  key_facts: { type: 'array', items: { type: 'string' } } } }
const DRAFT_SCHEMA = { type: 'object', additionalProperties: false, required: ['blueprint', 'lens', 'page_rows', 'verdicts_disposed', 'evidence_disposed', 'deltas_disposed'], properties: {
  blueprint: { type: 'string' }, lens: { type: 'string' }, page_rows: { type: 'number' }, verdicts_disposed: { type: 'number' },
  evidence_disposed: { type: 'number' }, deltas_disposed: { type: 'number' }, thesis: { type: 'string' } } }
const JUDGE_SCHEMA = { type: 'object', additionalProperties: false, required: ['lens', 'scores', 'winner'], properties: {
  lens: { type: 'string' }, winner: { type: 'string' }, rationale: { type: 'string' },
  scores: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['draft', 'gate_dispositions', 'gate_acyclic', 'capability', 'collapse'], properties: {
    draft: { type: 'string' }, gate_dispositions: { type: 'string', enum: ['pass', 'fail'] }, gate_acyclic: { type: 'string', enum: ['pass', 'fail'] },
    capability: { type: 'number' }, collapse: { type: 'number' }, elegance: { type: 'number' }, grafts: { type: 'array', items: { type: 'string' } } } } } } }
const SYNTH_SCHEMA = { type: 'object', additionalProperties: false, required: ['synthesis', 'base', 'grafts_applied'], properties: {
  synthesis: { type: 'string' }, base: { type: 'string' }, grafts_applied: { type: 'array', items: { type: 'string' } }, open_items: { type: 'array', items: { type: 'string' } } } }
const DECIDE_SCHEMA = { type: 'object', additionalProperties: false, required: ['decision', 'gate_record', 'overturns', 'page_rows', 'legs'], properties: {
  decision: { type: 'string' }, page_rows: { type: 'number' }, legs: { type: 'array', items: { type: 'string' } },
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
const PRE = 'Rasm monorepo. THE BRIEF ' + BRIEF + ' (repo root) is binding law for every judgment you make — read it COMPLETELY before anything ' +
  'else: [00]-[SHARED_LAW] (verdict, telos incl. the art-directed generation bar, structural authority, foundations-first + topological-partition ' +
  'law, generator law, owner-boundaries tier law, seam-and-entry law incl. the ArtifactWork contract, pyproject reconciliation), [01] the workflow ' +
  'contract you are executing, every numbered verdict V1..Vn at its live count (the track review may have extended it) with ruled defaults, ' + '[02] the full evidence register E1..En, [03] capability ' +
  'escalation targets, [04] package pressure incl. the shared-tier law, [05] build legs + the 1a/1b blast-radius and flagship-acceptance rulings, ' +
  '[06] out-of-scope. Target corpus: ' + ROOT + '/.planning/ (markdown design pages; code fences are the product). docs/stacks/python/ governs ' +
  'every fence shape you propose. Your stance is HOSTILE: the corpus is naive with illusory depth until disk proves otherwise, and NAIVETY is a ' +
  'defect on two axes — COVERAGE (thin slice of the owned domain) and APPROACH (enumerated instances where a parameterized generator belongs; ' +
  'rosters are seed DATA). The evidence register is an initial pointer, NEVER a ceiling.'
const ENGINE_VOCAB = 'ENGINE-NATIVE ACTIONS: the page-set table carries TWO action columns — the semantic action ' +
  '(KEEP/REBUILD/SPLIT/MERGE/MOVE/DELETE/NEW) AND its rebuild-engine lowering (kind new|rebuild|improve, deletePages rows, absorb pairs ' +
  '{into, from}) so the durable rebuild engine executes the DECISION unmodified: a SPLIT lowers to N new pages + one delete-with-absorb; a MERGE ' +
  'lowers to one improve/rebuild absorber + delete-with-absorb rows; a MOVE lowers to a new page at the destination + delete-with-absorb at the ' +
  'source; KEEP lowers to improve (the cold pass is never a skip).'
const ROW_SCHEMA = 'PAGE-ROW SCHEMA (every row total, no gaps): path | semantic action + engine lowering | one-line owner charter naming its ' +
  '[OWNER_BOUNDARIES] tier (geometry-generates / regime-binds / theme-selects, or N/A for non-triad pages) | entry signature (emit() -> ' +
  'ArtifactWork | Iterable<ArtifactWork>, or NONE for substrate pages that produce nothing) | ArtifactReceipt case (or NONE) | in/out seam edges | ' +
  'wave (1, 1a/1b if split, 2, 3). Plus: the FULL corrected seam ledger; a verdict-disposition table (every numbered verdict, each with the resolving rows named); ' +
  'an evidence-disposition table (every register row); a [03]-delta disposition table (every escalation delta names its owning row). A blueprint that leaves ' +
  'ANY verdict, evidence row, or [03] delta undisposed is INCOMPLETE and loses at judge.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (plane, scope, rows) => [PRE, 'TASK: READ-ONLY SURVEY of the ' + plane + ' plane. Scope: ' + scope + '. Deep-read every page ' +
  'in scope (full files, never skims). Re-verify ON DISK every evidence-register row assigned to you: ' + rows + ' — status HOLD (anchor exact, ' +
  'claim true), DRIFT (real defect, anchor/detail moved — give the corrected anchor), or REFUTED (disk falsifies the claim — cite the falsifying ' +
  'line). Beyond the register, hunt what it missed on both naivety axes, unwired seams (ARCHITECTURE.md ledger vs real fence imports), duplicate ' +
  'mechanisms, dead typed carriers, hardcoding where a generator belongs, and unmined capability in the catalogs you touch. WRITE a dense dossier ' +
  'to ' + SCRATCH + '/dossier-' + plane + '.md: per-page {verdict 1-10, defects with file:line, split/merge/move pressure, owner charter as it ' +
  'SHOULD be}, plus the register verdicts and every extra finding with evidence. The dossier is working material for the draft agents — dense, ' +
  'evidence-first, zero narration. Return the pointer + verdicts + the 10-15 most load-bearing facts. NO edits outside ' + SCRATCH + '/.'].join('\n')
const draftPrompt = (lens, thesis, dossiers) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: author ONE COMPLETE structural blueprint for the ' +
  'post-campaign ' + ROOT + '/.planning/ under the ' + lens + ' lens: ' + thesis + ' Read the brief in full, then every survey dossier: ' +
  JSON.stringify(dossiers) + ' — dossiers are evidence, the brief is law; where they conflict, re-verify on disk yourself. Honor every ruled ' +
  'default the live verdict set carries (V2 pattern home, V5 mark siting, V13 style siting, V15 solar kernel, V12 re-home, the ' +
  'iptcinfo3/python-xmp-toolkit removal and the dead-PyICU-pin resolution among them — the brief text governs, never this list) unless you ' +
  'carry disk proof a departure is stronger — a departure without proof loses at judge. The partition MUST be provably acyclic: every seam edge points within-wave ' +
  'or earlier; dispose the four named inversions explicitly. WRITE the complete blueprint to ' + SCRATCH + '/draft-' + lens + '.md with the full ' +
  'page-row table, seam ledger, disposition tables, and a leg partition honoring [FOUNDATIONS_FIRST] + the 1a/1b ruling. Self-report exact counts. ' +
  'NO edits outside ' + SCRATCH + '/.'].join('\n')
const judgePrompt = (lens, focus, drafts) => [PRE, 'TASK: ADVERSARIAL JUDGE, lens = ' + lens + '. ' + focus + ' Read the brief, then all four ' +
  'blueprints: ' + JSON.stringify(drafts) + '. Score each draft: gate_dispositions (pass only if EVERY numbered verdict, EVERY register row, and every [03] delta ' +
  'is disposed — count them yourself, never trust the self-report), gate_acyclic (walk the draft seam ledger against its wave assignment: pass ' +
  'only if zero edges point to a later wave AND zero page-level import cycles; name any violation), capability 0-10 (reach vs the [03] targets ' +
  'and the telos flagship artifacts), collapse 0-10 (fewer-deeper owners, generator-law cleanliness), elegance 0-10 (partition quality: blast ' +
  'radius, wave balance, boundary crispness under the tier law). A gate fail DISQUALIFIES regardless of scores. Name the specific GRAFTS — ' +
  'mechanisms in losing drafts strictly stronger than the winner counterpart, quoted precisely enough to apply. Winner = highest capability among ' +
  'gate-passers, collapse then elegance as tiebreaks. Read-only outside ' + SCRATCH + '/.'].join('\n')
const synthPrompt = (drafts, judges) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: SYNTHESIZE the blueprint of record. Judge reports: ' +
  JSON.stringify(judges) + '. Blueprints: ' + JSON.stringify(drafts) + '. Take the consensus winner as the base, apply EVERY graft at least two ' +
  'judges named and every single-judge graft that survives your own disk check, and re-verify the merged result stays acyclic and ' +
  'disposition-complete (a graft that breaks a gate is re-worked or dropped with the reason recorded). WRITE the complete synthesis to ' +
  SCRATCH + '/synthesis.md in the full row schema. Return the pointer, base, grafts applied, and open items the decide gate must rule. NO edits ' +
  'outside ' + SCRATCH + '/.'].join('\n')
const decidePrompt = (synthesis) => [PRE, ENGINE_VOCAB, ROW_SCHEMA, 'TASK: RED-TEAM DECIDE GATE + EMIT — you attack, then you WRITE the campaign ' +
  'DECISION. Attack the synthesis at ' + synthesis + ' on every axis: walk the seam ledger yourself and prove acyclicity or break it; audit every ' +
  'disposition of every numbered verdict against the brief text; spot-verify at least 10 factual claims on disk (a page it says exists, a member it says is mined, ' +
  'an import it says crosses); attack the leg partition for blast radius (apply the 1a/1b ruling if leg 1 is overloaded); attack any departure ' +
  'from a ruled default. Record every attack + outcome; overturn what fails with the reason — an overturn re-sites or strengthens capability, ' +
  'never drops it (zero current consumers never lowers the bar). Then WRITE ' + OUT + ' (repo root) as the BINDING ' +
  'campaign decision carrying exactly: (a) the final page-set table in the full row schema INCLUDING both action vocabularies; (b) the corrected ' +
  'seam ledger; (c) the ArtifactWork entry contract as law (verbatim-compatible with the brief [SEAM_AND_ENTRY_LAW]); (d) package roster deltas + ' +
  'the pyproject reconciliation rows; (e) the build-leg partition with per-leg page sets + per-leg targets/brief invocation rows for the durable ' +
  'rebuild engine, proven acyclic against (b), honoring [FOUNDATIONS_FIRST] + the 1a/1b ruling + the flagship-acceptance clause; (f) TASKLOG card ' +
  'dispositions (4 QUEUED absorbed by named legs; the two runtime/compute-gated BLOCKED cards re-dispositioned explicitly per the brief [06] — ' +
  'realize or keep blocked with the residual gate named, never silently — the Persistence-gated third stays blocked). Agent-facing ' +
  'declarative law throughout — no provenance, no narration, no ' +
  'hedging; every open item RULED, never deferred to build agents. The scratch dir ' + SCRATCH + '/ is working material — the DECISION must stand ' +
  'alone without it.'].join('\n')
const minePrompt = (draft, decision) => [PRE, 'TASK: SALVAGE MINER. Compare the losing/base blueprint ' + draft + ' against the WRITTEN decision ' +
  decision + ' (read both fully). Hunt content the draft carried that the DECISION lacks or renders thinner: mechanisms, page charters, seam ' +
  'rows, capability rows, disposition detail, generator shapes. A finding names WHAT, the draft evidence (quote/line), and WHERE in the DECISION ' +
  'it belongs as an owner extension (row/column/arm/clause — never a parallel surface). Absence alone is not a finding — the content must be ' +
  'STRONGER than what the DECISION holds, judged against the brief law. Read-only.'].join('\n')
const auditPrompt = (decision, gateRecord) => [PRE, 'TASK: OVERTURN AUDITOR. The decide gate recorded these attacks/overturns: ' + gateRecord +
  '. Against the WRITTEN decision ' + decision + ' and the synthesis + drafts in ' + SCRATCH + '/, classify every overturn JUSTIFIED / ' +
  'OVERZEALOUS (the overturned content was right — cite the disk/brief evidence) / PARTIAL (right to overturn, wrong replacement). Then sweep for ' +
  'SILENT drops: synthesis content that vanished from the DECISION with no overturn record. Spot-verify factual premises on disk. Read-only.'].join('\n')
const integratePrompt = (decision, findings) => [PRE, 'TASK: WRITING INTEGRATOR — the salvage close. Findings from 4 miners + the overturn ' +
  'auditor: ' + findings + '. Judge each on evidence against the brief law and the DECISION text: ACCEPT what is genuinely stronger (convergent ' +
  'findings across miners are the strongest signal; OVERZEALOUS/PARTIAL auditor verdicts restore corrected content), REJECT what the DECISION ' +
  'already covers or disk falsifies — name the falsifying line. Apply every accepted finding IN PLACE in ' + OUT + ' as owner extensions (a row, ' +
  'a column, an arm, a law clause — never a parallel section, never an appendix). Keep the DECISION disposition-complete and the partition ' +
  'acyclic after every edit. Return applied/rejected with locations.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [SURVEY]
phase('Survey')
const SURVEYS = [
  { plane: 'foundations', scope: ROOT + '/.planning/{graphic,typography,core}/ every page FULLY, plus ' + ROOT + '/ARCHITECTURE.md + README.md ' +
    '+ TASKLOG.md', rows: 'E1, E2 (derive side), E3 (typography side), E8 (vector/layout/draw anchors), E10, E11' },
  { plane: 'mid', scope: ROOT + '/.planning/{visualization,scene,composition,document}/ every page FULLY',
    rows: 'E5, E6 (press-fold + Pdf.from_html rows), E8 (sheet/emit anchors), E9 (glyphset/draw rows), E12' },
  { plane: 'aec-egress', scope: ROOT + '/.planning/{drawing,specification,delivery,export,exchange,media,package}/ — ' +
    'drawing/specification/delivery/export FULLY, exchange/media/package at duplication-hunt depth',
    rows: 'E2 (drawing importers), E3 (drawing side), E4, E6 (terminator/symbol-table/classification rows), E7, E9 (detail/register/dxf rows)' },
  { plane: 'api-tiers', scope: 'BOTH catalog tiers COMPLETE: ls + read libs/python/.api/*.md and ' + ROOT + '/.api/*.md; verify every [04] ' +
    'package-pressure claim against its stub; inventory capability the brief has not yet demanded',
    rows: 'every [04] stub anchor (treat each as a register row)' },
  { plane: 'import-census', scope: 'the whole ' + ROOT + '/.planning/ tree: sweep every cross-domain import in every fence (rg over the corpus), ' +
    'diff the real import graph against the ARCHITECTURE.md [02]-[SEAMS] ledger both directions (declared-unwired + wired-undeclared), and list ' +
    'every page-level import cycle', rows: 'E7 (full), E10 (ledger side), E13 (the corpus-wide track-law sweep: content_identity spellings, ' +
    'frozendict imports, [RESEARCH] tails, folder-minted limiters), the four [05] inversions, plus any register row past E13 the live count carries' },
]
const dossiers = (await parallel(SURVEYS.map((s) => () =>
  agent(surveyPrompt(s.plane, s.scope, s.rows), { label: 'survey:' + s.plane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL })))).filter(Boolean)
const refuted = dossiers.flatMap((d) => (d.register_verdicts || []).filter((v) => v.status !== 'HOLD').map((v) => d.plane + ':' + v.row + '=' + v.status))
log('Survey: ' + dossiers.length + '/5 dossiers; register drift/refutations: ' + (refuted.length ? refuted.join('; ') : 'none — register holds'))

// --- [DRAFT]
phase('Draft')
const DOSSIER_PATHS = dossiers.map((d) => d.dossier)
const LENSES = [
  { lens: 'foundations-maximalist', thesis: 'push maximum capability into the foundational plane so every upper plane thins to composition — ' +
    'the deepest graphic/typography/color/style owners the corpus can justify.' },
  { lens: 'entry-seam-first', thesis: 'derive the page-set from the ArtifactWork contract and the corrected seam ledger outward — structure ' +
    'follows the one entry rail and the acyclic ledger.' },
  { lens: 'package-mining-first', thesis: 'derive owners from what the admitted packages can actually do — every [04] capability row gets a ' +
    'home first, pages form around mined capability.' },
  { lens: 'output-first', thesis: 'work BACKWARD from the flagship artifacts (a full issued sheet set, a portfolio diagram suite, an editorial ' +
    'document package) to the exact structure that produces them at the telos bar.' },
]
const drafts = (await parallel(LENSES.map((l) => () =>
  agent(draftPrompt(l.lens, l.thesis, DOSSIER_PATHS), { label: 'draft:' + l.lens, phase: 'Draft', effort: 'max', schema: DRAFT_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Draft: ' + drafts.length + '/4 blueprints; rows [' + drafts.map((d) => d.page_rows).join(', ') + ']; dispositions V[' +
  drafts.map((d) => d.verdicts_disposed).join(',') + '] E[' + drafts.map((d) => d.evidence_disposed).join(',') + '] deltas[' + drafts.map((d) => d.deltas_disposed).join(',') + ']')

// --- [JUDGE]
phase('Judge')
const DRAFT_PATHS = drafts.map((d) => d.blueprint)
const JUDGES = [
  { lens: 'law-gates', focus: 'Own the DISQUALIFYING gates hardest: disposition completeness counted row-by-row, ledger acyclicity walked ' +
    'edge-by-edge, tier-law conformance, ruled-default departures without proof.' },
  { lens: 'capability-reach', focus: 'Own the capability ranking hardest: [03] target grades reachable or not per plane, package-mining depth ' +
    'vs [04], the telos flagship artifacts producible or not — walk each flagship through the draft.' },
  { lens: 'partition-elegance', focus: 'Own partition quality hardest: leg blast radius (1a/1b), wave balance, boundary crispness between the ' +
    'geometry and style triads, over-splitting vs under-splitting, blast radius of every MOVE/MERGE.' },
]
const judges = (await parallel(JUDGES.map((j) => () =>
  agent(judgePrompt(j.lens, j.focus, DRAFT_PATHS), { label: 'judge:' + j.lens, phase: 'Judge', effort: 'xhigh', schema: JUDGE_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Judge: ' + judges.length + '/3; winners [' + judges.map((j) => j.lens + '->' + j.winner).join(', ') + ']')

// --- [SYNTHESIZE]
phase('Synthesize')
const synth = await agent(synthPrompt(DRAFT_PATHS, JSON.stringify(judges)), { label: 'synthesize', phase: 'Synthesize', effort: 'max', schema: SYNTH_SCHEMA, stallMs: STALL })
if (!synth) { log('Synthesize produced nothing — aborting before Decide; resume from this run id re-runs it.'); return { dossiers: DOSSIER_PATHS, drafts: DRAFT_PATHS, judges: judges.length, aborted: 'synthesize' } }
log('Synthesize: base=' + synth.base + ', ' + (synth.grafts_applied || []).length + ' grafts, ' + (synth.open_items || []).length + ' open items for the gate')

// --- [DECIDE]
phase('Decide')
const decided = await agent(decidePrompt(synth.synthesis), { label: 'decide', phase: 'Decide', effort: 'max', schema: DECIDE_SCHEMA, stallMs: STALL })
if (!decided) { log('Decide produced nothing — DECISION not written; resume from this run id re-runs it.'); return { dossiers: DOSSIER_PATHS, drafts: DRAFT_PATHS, synthesis: synth.synthesis, aborted: 'decide' } }
log('Decide: ' + decided.decision + ' written — ' + decided.page_rows + ' page rows, legs [' + (decided.legs || []).join(' | ') + '], ' +
  (decided.overturns || []).length + ' overturn(s), ' + (decided.gate_record || []).length + ' attacks recorded')

// --- [SALVAGE]
phase('Salvage')
const mined = (await parallel(DRAFT_PATHS.map((p) => () =>
  agent(minePrompt(p, decided.decision), { label: 'mine:' + p.split('draft-').pop().replace('.md', ''), phase: 'Salvage', model: 'opus', effort: 'high', schema: MINE_SCHEMA, stallMs: STALL })))).filter(Boolean)
const audit = await agent(auditPrompt(decided.decision, JSON.stringify({ gate_record: decided.gate_record, overturns: decided.overturns })),
  { label: 'overturn-audit', phase: 'Salvage', model: 'opus', effort: 'max', schema: AUDIT_SCHEMA, stallMs: STALL })
const salvagePayload = JSON.stringify({ miners: mined, audit: audit || { overturn_verdicts: [], silent_drops: [] } })
const integrated = await agent(integratePrompt(decided.decision, salvagePayload), { label: 'integrate', phase: 'Salvage', effort: 'max', schema: INTEGRATE_SCHEMA, stallMs: STALL })
log('Salvage: ' + mined.reduce((n, m) => n + (m.findings || []).length, 0) + ' mined finding(s), ' +
  ((audit && audit.silent_drops) || []).length + ' silent drop(s), integrator applied ' + ((integrated && integrated.applied) || []).length +
  ' / rejected ' + ((integrated && integrated.rejected) || []).length)

return {
  decision: decided.decision, page_rows: decided.page_rows, legs: decided.legs,
  register_drift: refuted, judges: judges.map((j) => ({ lens: j.lens, winner: j.winner })),
  synthesis: synth.synthesis, overturns: decided.overturns,
  salvageApplied: (integrated && integrated.applied) || [], salvageRejected: (integrated && integrated.rejected) || [],
  scratch: SCRATCH,
}
