export const meta = {
  name: 'brief-py-folder',
  description: 'Author ONE world-class campaign brief for a libs/python folder: 4 parallel surveyors (corpus halves + both .api tiers + import/seam/csharp-fit census) -> 1 author agent writing RASM-PY-FOLDER-BRIEF.md modeled on the artifacts gold standard -> 4 sequential adversarial writing passes (strata coherence, capability/output grade, .api ultra-stacking with roster additions, cold re-read + final verdict). 9 agents, peak concurrency 4. args = {folder, out, upstream} — folder required (libs/python/name), out derived when omitted (RASM-PY-<NAME>-BRIEF.md), upstream = finalized earlier-folder brief paths the author and passes MUST honor as consumer pressure. Run once per folder in dependency order: runtime, data, geometry, compute.',
  whenToUse: 'Per py folder, in dependency order, with the prior folders briefs passed as upstream — produces the campaign brief a later specialized rebuild workflow executes. Ephemeral: delete after the py brief set lands.',
  phases: [
    { title: 'Survey', detail: '4 read-only agents: two corpus halves deep-read + both .api tiers + import/ledger/csharp-fit/upstream-consumption census' },
    { title: 'Author', detail: '1 agent writes the brief from the dossiers, structured on the artifacts gold standard: verdict/telos/laws/verdicts/evidence/escalation/package-pressure/legs' },
    { title: 'Refine', detail: '4 sequential adversarial writing passes: strata -> capability/output -> api ultra-stacking -> cold re-read with final verdict + top risks' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const GOLD = 'RASM-PY-ARTIFACTS-BRIEF.md'

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const FOLDER = (typeof argsIn === 'string' && argsIn.trim()) ? argsIn.trim().replace(/\/+$/, '')
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.folder === 'string' && argsIn.folder.trim()) ? argsIn.folder.trim().replace(/\/+$/, '')
  : ''
const NAME = FOLDER.split('/').pop() || ''
const OUT = (argsIn && typeof argsIn === 'object' && typeof argsIn.out === 'string' && argsIn.out.trim()) ? argsIn.out.trim()
  : 'RASM-PY-' + NAME.toUpperCase() + '-BRIEF.md'
const UPSTREAM = (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.upstream)) ? argsIn.upstream.filter((u) => typeof u === 'string' && u.trim()) : []
const SCRATCH = '.claude/scratch/brief-' + NAME

// --- [MODELS] ----------------------------------------------------------------------------
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['dossier', 'lane', 'key_facts'], properties: {
  dossier: { type: 'string' }, lane: { type: 'string' }, pages_read: { type: 'number' },
  key_facts: { type: 'array', items: { type: 'string' } },
  verdict_candidates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['what', 'evidence'], properties: { what: { type: 'string' }, evidence: { type: 'string' } } } } } }
const AUTHOR_SCHEMA = { type: 'object', additionalProperties: false, required: ['brief', 'verdict_count', 'evidence_rows', 'thesis'], properties: {
  brief: { type: 'string' }, verdict_count: { type: 'number' }, evidence_rows: { type: 'number' }, thesis: { type: 'string' } } }
const PASS_SCHEMA = { type: 'object', additionalProperties: false, required: ['edits', 'findings'], properties: {
  edits: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['section', 'what'], properties: { section: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  findings: { type: 'array', items: { type: 'string' } },
  roster_additions: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'concern'], properties: { package: { type: 'string' }, concern: { type: 'string' }, verification: { type: 'string' } } } },
  upstream_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['brief', 'what'], properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } } } },
  final_verdict: { type: 'string' }, top_risks: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const PRE = 'Rasm monorepo. Target: ' + FOLDER + '/.planning/ (markdown design pages of an intended Python package; code fences are the ' +
  'product). docs/stacks/python/ governs every fence shape. Your stance is HOSTILE: assume the corpus is naive with illusory depth until disk ' +
  'proves otherwise; NAIVETY is a defect on two axes — COVERAGE (thin slice of the owned domain) and APPROACH (enumerated instances where a ' +
  'parameterized generator belongs; rosters are seed DATA). Hunt architectural/flow/logic fundamental-approach problems, underutilized admitted ' +
  'capability, concern mixing, duplicate mechanisms, dead typed carriers, hardcoding, prose-vs-fence splits (a capability claimed in prose but ' +
  'absent from the fence is ILLUSORY), unwired declared seams, and parallel rails where one owner belongs.'
const FIT = 'FIT, NEVER COUPLED: the folder must fit the whole libs architecture — libs/.planning/architecture.md strata, the csharp seam ' +
  'records its pages carry (content-key rail, typed wire vocabularies, fault bands, the Element TASKLOG PY_WIRE_ALIGNMENT card where relevant), ' +
  'and the .archive/RASM-COMPONENT-PARADIGM-DECISION.md [AMENDMENTS] wire law (count-prefixed canonical bytes) — as boundary contracts composed at the ' +
  'edge, never as coupling to a sibling interior.' + (UPSTREAM.length ? ' UPSTREAM BRIEFS (finalized law for this folder\'s foundations — read ' +
  'each FULLY): ' + JSON.stringify(UPSTREAM) + '. The upstream campaigns will land new capabilities and structure; this folder is their CONSUMER ' +
  '— every upstream capability this folder could compose instead of hand-rolling is a named opportunity, and every assumption this folder makes ' +
  'about an upstream surface the upstream brief changes is a named migration pressure. Reference upstream capability as a consumer at the seam, ' +
  'never re-plan the upstream folder. WATERFALL RIPPLE: when THIS folder demands a capability an upstream brief lacks, EDIT that upstream brief ' +
  'surgically in place — an owner extension (a verdict clause, an escalation delta, a package row, a capability row) framed as consumer pressure ' +
  'with this folder named as the demanding consumer — never a rewrite, never a new section that re-plans the upstream; record every such edit in ' +
  'your return.' : '')

// --- [OPERATIONS] ------------------------------------------------------------------------
const surveyPrompt = (lane, scope) => [PRE, FIT, 'TASK: READ-ONLY SURVEY, lane = ' + lane + '. Scope: ' + scope + '. Deep-read fully — never ' +
  'skim. WRITE a dense dossier to ' + SCRATCH + '/dossier-' + lane + '.md: per-page {verdict 1-10, defects with file:line, split/merge/move ' +
  'pressure, the owner charter as it SHOULD be}, cross-cutting {duplication, concern mixing, hardcoding-vs-generator, dead carriers, unwired ' +
  'seams, unmined capability with catalog anchors}, and the 5-10 strongest VERDICT CANDIDATES (campaign-defining structural rulings with ' +
  'evidence). Dense, evidence-first, zero narration. Return the pointer + key facts + verdict candidates. NO edits outside ' + SCRATCH + '/.'].join('\n')
const authorPrompt = (dossiers) => [PRE, FIT, 'TASK: AUTHOR the campaign brief ' + OUT + ' (repo root) for the ground-up restructure+rebuild of ' +
  FOLDER + '/.planning/. GOLD STANDARD: read ' + GOLD + ' (repo root) COMPLETELY first — match its density, structure, and law-grade voice: ' +
  '[00]-[SHARED_LAW] (VERDICT with independently-fatal proofs anchored file:line; TELOS naming what world-class means for THIS folder and the ' +
  '5x-consumer bar; STRUCTURAL_AUTHORITY incl. split/merge/move/new-folder freedom; internal strata/foundations law if the folder has one; ' +
  'GENERATOR_LAW; the folder-appropriate seam/entry/rail law; PYPROJECT_RECONCILIATION), [01] the campaign phase guidance with NUMBERED BINDING ' +
  'VERDICTS (V1..Vn — each a structural ruling with a recommended-shape floor and a ruled default where decidable NOW; hedges carry deciding ' +
  'criteria), [02] the EVIDENCE REGISTER (E-rows, file:line anchors, sound-surfaces line), [03] CAPABILITY ESCALATION (per-plane now->target ' +
  'grades with concrete deltas), [04] PACKAGE_PRESSURE (both .api tiers stack; shared-tier law; mine-to-depth rows with stub anchors; roster ' +
  'ADDITION candidates where the domain demands a package the roster lacks — named concern + categorical-best candidate, centralized in ' +
  'pyproject.toml with .api stub obligations), [05] BUILD_LEGS (dependency-ordered waves inside the folder; per-leg closeout obligations), ' +
  '[06] OUT_OF_SCOPE (csharp boundary, upstream-owned concerns, blocked cards). Sources: the survey dossiers ' + JSON.stringify(dossiers) +
  ' (evidence — re-verify on disk anything load-bearing)' + (UPSTREAM.length ? ' + the upstream briefs (law)' : '') + '. Every claim anchored; ' +
  'agent-facing declarative; no provenance, no hedging, no restated doctrine. Return the path + counts + a one-line thesis.'].join('\n')
const passPrompts = [
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 1 of 4 — STRATA/ARCHITECTURE COHERENCE. You WRITE: fix and improve ' + brief + ' in place. ' +
    'Interrogate the brief AS AN ARCHITECTURE: do its verdicts compose into one coherent domain map (draw the post-campaign dependency graph — ' +
    'cycles, owners with two masters, fuzzy boundaries between adjacent owners get boundary law); is the internal wave/leg order a true ' +
    'topological order (name and dispose every inversion on disk); is the entry/rail law right for the folder\'s real machinery (read the ' +
    'folder\'s core rail pages yourself and prove the contract is realizable); is [01] executable by a design/build workflow without guessing ' +
    '(tighten schemas, gates, criteria). Verify every claim you add on disk. Never dilute a law; keep within ~1.2x length.'].join('\n'),
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 2 of 4 — CAPABILITY/OUTPUT GRADE. You WRITE: fix and improve ' + brief + ' in place. Walk the ' +
    'folder\'s FLAGSHIP outputs backward (pick the 2-3 hardest real deliverables a world-class version must produce; for a substrate folder, ' +
    'the hardest consumer contracts it must serve' + (UPSTREAM.length ? ', including what the upstream briefs now make possible' : '') + ') and ' +
    'find every chain break or vague link — each becomes a verdict extension, escalation delta, or named obligation. Where a verdict or [03] ' +
    'row settles for parity or repair, RAISE it to what the world-class version owns, backed by real package surfaces (verify in the .api ' +
    'stubs; no vapor). Honest ingress boundaries: name what arrives as data from csharp/upstream vs computed here. Never dilute; ~1.25x cap.'].join('\n'),
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 3 of 4 — .API ULTRA-STACKING. You WRITE: fix and improve ' + brief + ' in place. Inventory BOTH ' +
    'tiers (ls libs/python/.api/ and ' + FOLDER + '/.api/); for every stub the brief does not cite, judge whether the corpus mines it to depth ' +
    'and add verified mine-to-depth rows (stub anchors mandatory). Audit existing [04] rows against the stubs; fix contradicted spellings. ' +
    'SUPERSESSION sweep (categorical-best law) -> pyproject reconciliation rows. ROSTER ADDITIONS: where the domain demands capability NO ' +
    'admitted package owns, name the concern + the categorical-best candidate package (verify it is real, maintained, and license-clean via ' +
    'web research; two corroborating sources; paid/gated rejected) as an [04] addition row + pyproject obligation — never silently narrow ' +
    'the domain to the current roster. INTEGRATION-FIRST: an admitted zero-consumer package is an integration mandate — realize it as a ' +
    'row/arm on its owning axis; a strike requires proven redundancy (stronger admitted owner named), feed-verified abandonment, or ' +
    'charter/license conflict, never consumer absence alone. SEAL-CHALLENGE: a closed sweep or rejection list in the corpus is challengeable ' +
    'design pressure a verdict may re-open as a parameterized axis, never inherited law. PACKAGE WATERFALL: where a package reaches full ' +
    'power only with a counterpart elsewhere, record the enablement both directions (upstream owner extension via the ripple authority; ' +
    'downstream forward-obligation row). Shared-tier law present and folder-appropriate. Never dilute; ~1.2x cap.'].join('\n'),
  (brief) => [PRE, FIT, 'TASK: ADVERSARIAL PASS 4 of 4 — COLD RE-READ, the last hands on this brief. You WRITE: fix in place. Read the ENTIRE ' +
    'brief twice as a first reader and as the most hostile one: cross-reference closure (every verdict <-> evidence row <-> escalation delta ' +
    '<-> package row <-> leg assignment; fix every dangling end); naivety hunt on the brief itself (kill hedges — where a "the campaign ' +
    'decides" is decidable NOW from evidence in hand, decide it with a ruled default; where genuinely open, verify deciding criteria exist); ' +
    'executability dry-run (could a design/build workflow run from [01]/[05] alone); prose law (declarative, no meta, no provenance, binding ' +
    'forms); spot-check 4-5 evidence anchors on disk for drift. Return the final verdict (world-class if executed faithfully? with nuance) + ' +
    'top 3 residual risks. Never dilute; ~1.15x cap.'].join('\n'),
]

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!FOLDER) { log('brief-py-folder: no folder passed (args = {folder: "libs/python/<name>", upstream: [...]}). No-op.'); return { folder: '', total: 0 } }

// --- [SURVEY]
phase('Survey')
const LANES = [
  { lane: 'corpus-a', scope: FOLDER + '/.planning/ — ls the subfolder set, sort alphabetically, deep-read every page in the FIRST HALF of the ' +
    'subfolders (plus root-level pages), and ' + FOLDER + '/ARCHITECTURE.md + README.md + TASKLOG.md in full' },
  { lane: 'corpus-b', scope: FOLDER + '/.planning/ — ls the subfolder set, sort alphabetically, deep-read every page in the SECOND HALF of the ' +
    'subfolders; skim the first half only where a seam crosses into it' },
  { lane: 'api-tiers', scope: 'BOTH catalog tiers COMPLETE: ls + read libs/python/.api/*.md and ' + FOLDER + '/.api/*.md; per stub judge mined ' +
    'vs unmined against the owning pages; inventory capability the folder never demands; note DOMAIN GAPS where no admitted package owns a ' +
    'concern the folder needs (roster-addition candidates, named not researched)' },
  { lane: 'census', scope: 'the whole ' + FOLDER + '/.planning/ tree: sweep every cross-folder import/seam in every fence, diff the real graph ' +
    'against ' + FOLDER + '/ARCHITECTURE.md [02]-[SEAMS] both directions (declared-unwired + wired-undeclared), list page-level import cycles, ' +
    'record every csharp seam the pages carry' + (UPSTREAM.length ? ', and read the upstream briefs FULLY to list every upstream capability ' +
    'this folder could consume instead of hand-rolling + every assumption an upstream campaign invalidates' : '') },
]
const dossiers = (await parallel(LANES.map((l) => () =>
  agent(surveyPrompt(l.lane, l.scope), { label: 'survey:' + l.lane, phase: 'Survey', model: 'opus', effort: 'max', schema: SURVEY_SCHEMA, stallMs: STALL })))).filter(Boolean)
log('Survey[' + NAME + ']: ' + dossiers.length + '/4 dossiers; ' + dossiers.reduce((n, d) => n + (d.verdict_candidates || []).length, 0) + ' verdict candidate(s)')

// --- [AUTHOR]
phase('Author')
const authored = await agent(authorPrompt(dossiers.map((d) => d.dossier)), { label: 'author:' + NAME, phase: 'Author', effort: 'max', schema: AUTHOR_SCHEMA, stallMs: STALL })
if (!authored) { log('Author produced nothing — brief not written; resume from this run id re-runs it.'); return { folder: FOLDER, dossiers: dossiers.map((d) => d.dossier), aborted: 'author' } }
log('Author: ' + authored.brief + ' — ' + authored.verdict_count + ' verdicts, ' + authored.evidence_rows + ' evidence rows. Thesis: ' + authored.thesis)

// --- [REFINE]
phase('Refine')
const PASS_LABELS = ['strata', 'capability', 'api-stacking', 'cold-read']
const passes = []
for (let i = 0; i < passPrompts.length; i++) {
  const p = await agent(passPrompts[i](authored.brief), { label: 'pass:' + PASS_LABELS[i], phase: 'Refine', effort: i === 3 ? 'max' : 'xhigh', schema: PASS_SCHEMA, stallMs: STALL })
  passes.push(p)
  log('Pass ' + (i + 1) + '/4 (' + PASS_LABELS[i] + '): ' + (p ? (p.edits || []).length + ' edit(s), ' + (p.findings || []).length + ' finding(s)' +
    ((p.roster_additions || []).length ? ', roster +' + p.roster_additions.length : '') : 'NO RESULT — rerun via resume'))
}
const final = passes[3]

return {
  folder: FOLDER, brief: authored.brief, thesis: authored.thesis,
  verdicts: authored.verdict_count, evidence_rows: authored.evidence_rows,
  pass_edits: passes.map((p, i) => ({ pass: PASS_LABELS[i], edits: p ? (p.edits || []).length : -1 })),
  roster_additions: passes.filter(Boolean).flatMap((p) => p.roster_additions || []),
  upstream_ripples: passes.filter(Boolean).flatMap((p) => p.upstream_ripples || []),
  final_verdict: (final && final.final_verdict) || 'PASS 4 MISSING — rerun',
  top_risks: (final && final.top_risks) || [],
  dossiers: dossiers.map((d) => d.dossier), scratch: SCRATCH,
}
