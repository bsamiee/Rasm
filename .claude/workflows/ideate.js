export const meta = {
  name: 'ideate',
  whenToUse: 'Rebuild a folder IDEAS and TASK pool to world-class when the deferred idea or task pool is stale or thin.',
  description: 'Rebuild a folder IDEAS + TASKS card pool to world-class: survey the realized corpus and research the real domain, author the genuinely-deferred idea/task pool, then fix-in-place constructive critique + hostile adversarial redteam. Language-agnostic (cards are markdown governed by the card schema). Authors NO design pages (that is the rebuild-* workflows) and aligns nothing pre-existing for its own sake (that is align-cards) — this is the greenfield/expansion pool generator. Every agent call takes a slot in one agent-level scheduler (CAP=10) so the true in-flight agent count stays at cap while all folder chains run concurrently; within a folder the survey -> ideate -> critique -> redteam chain holds because each stage consumes the prior stage\'s landed cards, and a folder holding more than 12 planning pages runs two page-slice survey agents whose maps merge before the ideate stage. Survey lanes (including the page-slice splits) run read-only on gpt-5.5 dispatched through sonnet codex wrappers (CODEX flag; false restores native opus); ideate/critique/redteam stay fable writers. Every stage writes BOTH ends of every Ripple itself — a cross-folder counterpart is authored or repaired directly in the sibling folder\'s card files in the same pass under the current-state law; nothing routes to a later phase. args = optional scope (e.g. "libs/python/geometry"); empty = all of libs.',
  phases: [
    { title: 'Survey', detail: 'discover card-owning folders with page counts (sonnet), then per folder: a read-only gpt-5.5 lane (codex wrapper) maps realized capability + current pool + researched domain-completeness gaps + cross-folder seams; a folder above 12 planning pages splits the survey across two page-slice gpt-5.5 lanes merged before ideate' },
    { title: 'Ideate', detail: 'author/rebuild the IDEAS + TASKS pool grounded in the survey, genuinely-deferred only, both Ripple ends landed' },
    { title: 'Critique', detail: 'fix-in-place: pull the pool up — density, domain-completeness, anchors, ripples' },
    { title: 'Redteam', detail: 'fix-in-place hostile reviewer: attack redundancy, mis-carding, dangling ripples, under-ideation' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 14
const SURVEY_PAGE_CAP = 12
const STAGGER_MS = 1500
const STALL = 300000
const CODEX = true // survey lanes run on gpt-5.5 via the codex wrapper; false restores native opus lanes
const CODEX_DIR = '.claude/scratch/codex' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'pages'], properties: { folder: { type: 'string' }, pages: { type: 'integer' } } } } } }
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['realized', 'gaps', 'notes'], properties: { realized: { type: 'array', items: { type: 'string' } }, gaps: { type: 'array', items: { type: 'string' } }, cross_folder: { type: 'array', items: { type: 'string' } }, existing_cards: { type: 'array', items: { type: 'string' } }, notes: { type: 'string' } } }
// Required-but-possibly-empty `beyondFolder` is an attestation: the cross-folder Ripple hunt ran, and every counterpart landed.
const CARDLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'beyondFolder', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, beyondFolder: { type: 'array', items: { type: 'string' } }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'refined', 'hardened', 'clean'] }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. CLAUDE.md card law governs. READ libs/.planning/campaign-method.md for the role law and voice, and libs/.planning/README.md ' +
    'for the exact IDEAS/TASKLOG card schema. You produce a folder ' +
    'IDEAS + TASKS card pool: IDEAS are conceptual/multi-step/higher-order deferred capability; TASKS are concrete/focused deferred units. Every ' +
    'card passes the 7-point density rubric (polymorphic collapse; one-hop; growth-axis absorption; Result/Option rails; library-at-depth; ' +
    'policy-values not boolean knobs; greenfield in-place). A card is ONLY for genuinely DEFERRED work — NEVER duplicate a realized design page, ' +
    'NEVER a test/meta/decision/unblock/create-file card. Cards are FLOORS, not ceilings. HARDENING: capability is improved or extended, NEVER ' +
    'dropped for lack of a current consumer — zero consumers never lowers the bar; planned future consumers are real design pressure on every card.',
  'NAIVETY LAW: a card is naive on two axes, both defects repaired on sight — COVERAGE: its Capability/Shape models a thin slice of the concept ' +
    '(the obvious three fields where the domain carries fifteen; a two-case family for a twenty-case domain); APPROACH: enumerated hardcoded ' +
    'instances — a fixed roster of styles/patterns/variants — where a parameterized algorithmic owner should generate the space (a roster is ' +
    'seed DATA feeding one generator over named parameters, never the mechanism). Every rubric, checklist, and attack list in this workflow is ' +
    'a FLOOR hunted past, never the complete set: any repeated structure, parallel spelling, or enumerable family an algebra/table/fold/generator ' +
    'can own is a collapse target you find yourself.',
  'RIPPLE LAW: a cross-folder dependency is a Ripple that pairs two cards — each references the other by backticked owner + backticked [SLUG]. ' +
    'BOTH ends are YOURS in the same pass: a Ripple to an EXISTING counterpart REFERENCES it (never re-creates the slug); an ABSENT counterpart ' +
    'is authored NOW directly in the sibling folder IDEAS.md/TASKLOG.md (create them if absent), matched to that pool as it stands on disk. One ' +
    'canonical slug per bounded concept; no collisions; no dangling or one-sided Ripple survives your pass.',
  'CURRENT-STATE LAW: sibling folder pipelines land cards concurrently with yours. Before ANY edit — above all to a sibling folder card file — ' +
    're-read the CURRENT on-disk state of every file you touch; landed sibling work is composed as found, an existing counterpart slug is ' +
    'referenced rather than duplicated, and a conflict between your card and a landed sibling resolves to the stronger form, never a revert.',
  'RESEARCH MANDATE: never guess domain completeness. Read the folder realized corpus (its design pages + ARCHITECTURE + README) AND research the ' +
    'REAL domain to find genuinely-deferred capability the pool should hold. The pool must reflect the domain, not only what is already written.',
  'ULTRA-STACKING LAW: enumerate BOTH .api tiers in full with a real ls/find from disk, never memory — the language-root tier (libs/<lang>/.api/) ' +
    'and the folder/package tier — and mine them to operator depth. An admitted capability the domain admits but no design page or card exploits ' +
    'is a defect: card it. A member a card cites that no .api catalogue or design page verifies is a phantom: correct or delete it. Verified ' +
    'members only.',
  'WRITE-FULLY MANDATE: every card you author/refine/repair you MUST write NOW via Edit/Write directly into the owning IDEAS.md / TASKLOG.md — ' +
    'this folder or a sibling: any card file your work exposes is yours in the same pass. The structured log is a REPORT of edits ALREADY MADE, ' +
    'never a to-do list or hedge; leave nothing behind. `files` lists every file you edited; `beyondFolder` lists the sibling-folder card files ' +
    'among them — empty attests every Ripple counterpart already existed, never that the hunt did not run. If the pool is already world-class, ' +
    'return verdict=clean — a clean verdict is EARNED by an attack that finds nothing, never conceded on first read; never invent cards to look busy.',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// Agent-level slot scheduler: every agent() call takes one slot, so folder chains launch freely via Promise.all while true in-flight agents stay at CAP.
const makeSlots = (cap) => {
  let active = 0
  let gate = Promise.resolve()
  const waiters = []
  const stagger = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  return async (fn) => {
    if (active >= cap) await new Promise((res) => waiters.push(res))
    active++
    await stagger()
    try { return await fn() } finally { active--; const next = waiters.shift(); if (next) next() }
  }
}
const slot = makeSlots(CAP)
const nameOf = (f) => f.split('/').pop() || f
// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns that JSON verbatim. It never does, edits, or judges the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and relay ' +
    'its typed answer VERBATIM. Never perform, edit, judge, soften, or summarize the task yourself.',
  '(1) mkdir -p ' + CODEX_DIR + '; write the TASK block below verbatim to ' + base + '-task.md; write this JSON ' +
    'Schema exactly to ' + base + '-schema.json: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call that returns immediately: ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>&1 &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rpt + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rpt + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). Cap at 7 poll calls.',
  '(4) READY: return the report-file JSON through your structured output VERBATIM, unchanged. GONE with no report: ' +
    'relaunch the (2) command once (detached, never foreground) and resume polling; a second GONE returns the ' +
    'schema shape with every array empty and each required string field set to CODEX-FAILED plus the one-line reason.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}
// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise.
const recon = (task, o) => CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: o.schema, stallMs: STALL })
  : agent(task, { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: o.schema, stallMs: STALL })
const surveyPrompt = (folder, slice) => [LAW, '', 'TASK: DISCOVERY SURVEY of ' + folder + ' — read-only is its ONLY concession; skimming, memory-recall ' +
  'inventories, and verdict-only output are process defects. FULL-FILE read every design page under ' + folder + '/.planning/**, plus ' +
  'ARCHITECTURE.md, README.md, and any existing ' + folder + '/IDEAS.md + TASKLOG.md; walk the folder at large and enumerate both .api tiers per ' +
  'the ULTRA-STACKING LAW; resolve scope against real disk state. Produce a per-page MAP, never a verdict: (realized) per page, the capability it ' +
  'composes NOW — never to be carded; (gaps) genuinely-deferred capability the REAL domain needs beyond the realized set — research the domain, ' +
  'never guess — including every admitted capability no page or card exploits, each named with concrete verified members, never a phantom; ' +
  '(cross_folder) contextual seams where a card should Ripple to a sibling owner; (existing_cards) the current pool and its state; (notes) ' +
  'stacking guidance plus a hostile weak/strong call per page. The map is an initial pointer for downstream stages, never a ceiling.' +
  (slice ? '\n' + slice : '')].join('\n')
const slicePrompt = (folder, part, pages) => 'PAGE SLICE ' + part + ' of 2 — ' + folder + ' holds ' + pages + ' design pages, so the survey is split ' +
  'and this directive NARROWS the every-page read above: enumerate every design page under ' + folder + '/.planning/** sorted lexicographically by ' +
  'path; FULL-FILE read ' + (part === 1 ? 'ONLY the FIRST half of that ordering (pages 1..ceil(N/2))' : 'ONLY the SECOND half of that ordering (the ' +
  'pages after ceil(N/2))') + '. ARCHITECTURE.md, README.md, the existing card files, and both .api tiers stay FULL reads; your map covers your slice pages.'
// Two page-slice survey maps fold into one SURVEY_SCHEMA-shaped object before ideate; a single-part survey passes through untouched.
const mergeSurveys = (parts) => parts.length === 1 ? parts[0] : {
  realized: [...new Set(parts.flatMap((p) => p.realized || []))],
  gaps: [...new Set(parts.flatMap((p) => p.gaps || []))],
  cross_folder: [...new Set(parts.flatMap((p) => p.cross_folder || []))],
  existing_cards: [...new Set(parts.flatMap((p) => p.existing_cards || []))],
  notes: parts.map((p) => p.notes || '').filter(Boolean).join('\n'),
}
const ideatePrompt = (folder, ctx) => [LAW, '', 'SURVEY of ' + folder + ':\n' + ctx, '', 'TASK: author/rebuild the IDEAS + TASKS pool in ' + folder + '/IDEAS.md ' +
  'and ' + folder + '/TASKLOG.md, grounded in the survey above — the survey is an initial pointer, never a ceiling: re-read the corpus and both ' +
  '.api tiers yourself and exceed it; it never licenses a skim. Author IDEAS for the conceptual/multi-step gaps and TASKS for the ' +
  'concrete/deferred ones; each card dense at the 7-point rubric and naive on neither axis, genuinely-deferred ONLY (never a realized page from ' +
  'the survey), correctly anchored, with every Ripple bidirectional per the RIPPLE + CURRENT-STATE laws — the cross-folder counterpart referenced ' +
  'where it exists, authored in the sibling folder NOW where it does not. Match the exact card schema from libs/.planning/README.md and the voice ' +
  'from campaign-method.md. Fix-in-place (write the files, create if absent). Return the card-log listing every file you edited, sibling folders ' +
  'included.'].join('\n')
const critiquePrompt = (folder) => [LAW, '', 'TASK: MECHANICAL LINE-BY-LINE CRITIQUE + FIX IN PLACE of every card in ' + folder + '/IDEAS.md + ' +
  'TASKLOG.md — hold the pool naive until it proves otherwise. Audit card by card and pull the pool UP: denser/sharper theses, fuller ' +
  'domain-completeness (is a genuinely-deferred capability still missing?), better anchors, richer and correct Ripple refs, polymorphic/AOP ' +
  'framing in Capability/Shape. Attack both naivety axes: widen COVERAGE thin-slices to the full concept; rewrite APPROACH roster-cards so the ' +
  'roster is seed data feeding one parameterized generator. Check every member a card cites against both .api tiers — correct or delete ' +
  'phantoms; card any admitted capability the pool ignores. These checks are a floor — hunt past them. A cross-folder Ripple asymmetry is ' +
  'repaired at BOTH ends NOW per the RIPPLE + CURRENT-STATE laws. EDIT the card files. Return the card-log listing every file you edited, ' +
  'sibling folders included.'].join('\n')
const redteamPrompt = (folder) => [LAW, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE of the cards in ' + folder + '/IDEAS.md + TASKLOG.md — ' +
  'the terminal and most aggressive review. You are a HOSTILE reviewer whose goal is to REJECT this pool — assume it is redundant, under-dense, ' +
  'naive, or under-ideated until it proves otherwise; dense confident-looking cards are the prime suspects for hollowness. ATTACK and repair: ' +
  'any card that duplicates a realized design page (mis-carded in-pass work — delete/disposition); any ' +
  'test/meta/decision/unblock/create-file card; dangling/asymmetric/colliding Ripple — repaired at BOTH ends NOW per the RIPPLE + CURRENT-STATE ' +
  'laws, counterpart authored where absent; weak/under-dense cards failing the 7-point rubric; cards naive on either NAIVETY-LAW axis; cited ' +
  'members no .api catalogue or page verifies (phantoms — correct or delete); non-deferred work wrongly carded. This attack list is a FLOOR — ' +
  'hunt defects beyond it. ALSO the FORWARD lens: genuinely-deferred domain capability MISSING from the pool (under-ideation), admitted .api ' +
  'capability no card exploits, a pool that understates the real domain — author what is missing. Run COUNTERFACTUALS (is this the strongest ' +
  'set of deferred bets, or is there a denser/higher-leverage framing?), then END with a full cold re-review of both files as one body — ' +
  'verdict=clean only when that cold attack finds nothing. Repair every defect in place, wherever it lives. Return the card-log listing every ' +
  'file you edited, sibling folders included.'].join('\n')
const ideateFolder = async (u) => {
  const folder = u.folder
  const surveyors = ((u.pages || 0) > SURVEY_PAGE_CAP)
    ? [1, 2].map((part) => slot(() => recon(surveyPrompt(folder, slicePrompt(folder, part, u.pages)),
        { label: 'survey:' + nameOf(folder) + '#' + part, phase: 'Survey', schema: SURVEY_SCHEMA })))
    : [slot(() => recon(surveyPrompt(folder, ''), { label: 'survey:' + nameOf(folder), phase: 'Survey', schema: SURVEY_SCHEMA }))]
  const parts = (await Promise.all(surveyors)).filter(Boolean)
  if (parts.length !== surveyors.length) return { folder, logs: {}, ok: false }
  const authored = await slot(() => agent(ideatePrompt(folder, JSON.stringify(mergeSurveys(parts))), { label: 'ideate:' + nameOf(folder), phase: 'Ideate', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'high', stallMs: STALL }))
  if (authored === null) return { folder, logs: {}, ok: false }
  const crit = await slot(() => agent(critiquePrompt(folder), { label: 'crit:' + nameOf(folder), phase: 'Critique', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'high', stallMs: STALL }))
  const rt = await slot(() => agent(redteamPrompt(folder), { label: 'redteam:' + nameOf(folder), phase: 'Redteam', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'high', stallMs: STALL }))
  return { folder, logs: { ideate: authored, crit, redteam: rt }, ok: rt !== null }
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Survey')
const inv = await agent('DISCOVERY: list the package/area folders under ' + SWEEP + ' that own a design corpus (immediate child directories containing a ' +
  '.planning directory and/or a README.md + ARCHITECTURE.md). Include the branch-level tier if it owns cards. The listing MUST be a live find/ls ' +
  'run against the working tree — never memory, a prior run, or an index — with the scope path first proven to exist on disk. Completeness is the ' +
  'product: a folder you miss silently escapes the whole pass; widen the search on any doubt and verify every returned path exists. For each ' +
  'folder also COUNT its design pages (markdown files under <folder>/.planning/**; 0 when no .planning tree exists) and return the count as ' +
  'pages. Return folders sorted by path. Read-only; do not cd.', { label: 'discover', phase: 'Survey', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const FOLDERS = ((inv && inv.folders) || []).filter((u) => u && u.folder)
log('Ideate discover under ' + SWEEP + ': ' + FOLDERS.length + ' folders')

const done = (await Promise.all(FOLDERS.map((u) => ideateFolder(u)))).filter(Boolean)
const complete = done.filter((r) => r.ok)
const failed = done.filter((r) => !r.ok).map((r) => r.folder)
const stages = ['ideate', 'crit', 'redteam']
const touched = [...new Set(complete.flatMap((r) => stages.flatMap((k) => (r.logs[k] && r.logs[k].files) || [])))]
const beyond = [...new Set(complete.flatMap((r) => stages.flatMap((k) => (r.logs[k] && r.logs[k].beyondFolder) || [])))]
const verdicts = {}
for (const r of done) for (const k of stages) { const v = r.logs[k] && r.logs[k].verdict; if (v) verdicts[v] = (verdicts[v] || 0) + 1 }
log('Ideate: ' + complete.length + '/' + FOLDERS.length + ' folder pools closed; ' + touched.length + ' card files touched (' + beyond.length +
  ' via cross-folder Ripple authorship); verdicts ' + JSON.stringify(verdicts) + (failed.length ? ' — FAILED (reported, run continues): ' + failed.join(', ') : ''))
return { scope: SWEEP, folders: FOLDERS.length, complete: complete.map((r) => r.folder), failed, filesTouched: touched.length, beyondFolder: beyond, verdicts }
