export const meta = {
  name: 'ideate',
  whenToUse: 'Rebuild a folder IDEAS and TASK pool to world-class when the deferred idea or task pool is stale or thin.',
  description: 'Rebuild a folder IDEAS + TASKS card pool to world-class: survey the realized corpus and research the real domain, author the genuinely-deferred idea/task pool, then fix-in-place constructive critique + hostile adversarial redteam. Language-agnostic (cards are markdown governed by the card schema). Authors NO design pages (that is the rebuild-* workflows) and aligns nothing pre-existing for its own sake (that is align-cards) — this is the greenfield/expansion pool generator. Folders run concurrently under one pool cap; within a folder the survey -> ideate -> critique -> redteam chain holds because each stage consumes the prior stage\'s landed cards. Every stage writes BOTH ends of every Ripple itself — a cross-folder counterpart is authored or repaired directly in the sibling folder\'s card files in the same pass under the current-state law; nothing routes to a later phase. args = optional scope (e.g. "libs/python/geometry"); empty = all of libs.',
  phases: [
    { title: 'Survey', detail: 'discover card-owning folders, then per folder: map realized capability + current pool + researched domain-completeness gaps + cross-folder seams' },
    { title: 'Ideate', detail: 'author/rebuild the IDEAS + TASKS pool grounded in the survey, genuinely-deferred only, both Ripple ends landed' },
    { title: 'Critique', detail: 'fix-in-place: pull the pool up — density, domain-completeness, anchors, ripples' },
    { title: 'Redteam', detail: 'fix-in-place hostile reviewer: attack redundancy, mis-carding, dangling ripples, under-ideation' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
const STALL = 300000

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'string' } } } }
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
// The single scheduler for every agent-bearing task in the run: CAP tasks in flight, staggered launch.
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  let gate = Promise.resolve()
  const launch = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  const run = async () => { while (next < items.length) { const i = next++; await launch(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}
const nameOf = (f) => f.split('/').pop() || f
const surveyPrompt = (folder) => [LAW, '', 'TASK: DISCOVERY SURVEY of ' + folder + ' — read-only is its ONLY concession; skimming, memory-recall ' +
  'inventories, and verdict-only output are process defects. FULL-FILE read every design page under ' + folder + '/.planning/**, plus ' +
  'ARCHITECTURE.md, README.md, and any existing ' + folder + '/IDEAS.md + TASKLOG.md; walk the folder at large and enumerate both .api tiers per ' +
  'the ULTRA-STACKING LAW; resolve scope against real disk state. Produce a per-page MAP, never a verdict: (realized) per page, the capability it ' +
  'composes NOW — never to be carded; (gaps) genuinely-deferred capability the REAL domain needs beyond the realized set — research the domain, ' +
  'never guess — including every admitted capability no page or card exploits, each named with concrete verified members, never a phantom; ' +
  '(cross_folder) contextual seams where a card should Ripple to a sibling owner; (existing_cards) the current pool and its state; (notes) ' +
  'stacking guidance plus a hostile weak/strong call per page. The map is an initial pointer for downstream stages, never a ceiling.'].join('\n')
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
const ideateFolder = async (folder) => {
  const survey = await agent(surveyPrompt(folder), { label: 'survey:' + nameOf(folder), phase: 'Survey', model: 'opus', schema: SURVEY_SCHEMA, effort: 'high', stallMs: STALL })
  if (survey === null) return { folder, logs: {}, ok: false }
  const authored = await agent(ideatePrompt(folder, JSON.stringify(survey)), { label: 'ideate:' + nameOf(folder), phase: 'Ideate', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'max', stallMs: STALL })
  if (authored === null) return { folder, logs: {}, ok: false }
  const crit = await agent(critiquePrompt(folder), { label: 'crit:' + nameOf(folder), phase: 'Critique', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'xhigh', stallMs: STALL })
  const rt = await agent(redteamPrompt(folder), { label: 'redteam:' + nameOf(folder), phase: 'Redteam', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'xhigh', stallMs: STALL })
  return { folder, logs: { ideate: authored, crit, redteam: rt }, ok: rt !== null }
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Survey')
const inv = await agent('DISCOVERY: list the package/area folders under ' + SWEEP + ' that own a design corpus (immediate child directories containing a ' +
  '.planning directory and/or a README.md + ARCHITECTURE.md). Include the branch-level tier if it owns cards. The listing MUST be a live find/ls ' +
  'run against the working tree — never memory, a prior run, or an index — with the scope path first proven to exist on disk. Completeness is the ' +
  'product: a folder you miss silently escapes the whole pass; widen the search on any doubt and verify every returned path exists. Return sorted ' +
  'repo-relative paths. Read-only; do not cd.', { label: 'discover', phase: 'Survey', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const FOLDERS = ((inv && inv.folders) || []).filter(Boolean)
log('Ideate discover under ' + SWEEP + ': ' + FOLDERS.length + ' folders')

const done = (await pool(FOLDERS, CAP, (f) => ideateFolder(f))).filter(Boolean)
const complete = done.filter((r) => r.ok)
const failed = done.filter((r) => !r.ok).map((r) => r.folder)
const stages = ['ideate', 'crit', 'redteam']
const touched = [...new Set(complete.flatMap((r) => stages.flatMap((k) => (r.logs[k] && r.logs[k].files) || [])))]
const beyond = [...new Set(complete.flatMap((r) => stages.flatMap((k) => (r.logs[k] && r.logs[k].beyondFolder) || [])))]
log('Ideate: ' + complete.length + '/' + FOLDERS.length + ' folder pools closed; ' + touched.length + ' card files touched (' + beyond.length +
  ' via cross-folder Ripple authorship)' + (failed.length ? ' — FAILED (reported, run continues): ' + failed.join(', ') : ''))
return { scope: SWEEP, folders: FOLDERS.length, complete: complete.map((r) => r.folder), failed, filesTouched: touched.length, beyondFolder: beyond }
