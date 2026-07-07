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
const CODEX = true                         // survey lanes run on gpt-5.5 via the codex wrapper; false restores native opus lanes
const CODEX_DIR = '.claude/scratch/ideate' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------

const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'pages'], properties: { folder: { type: 'string' }, pages: { type: 'integer' } } } } } }

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR = { type: 'object', additionalProperties: false, required: ['path', 'line', 'role', 'note'], properties: {
  path: { type: 'string' }, line: { type: 'integer' },
  role: { type: 'string', enum: ['state', 'ruling', 'catalog', 'counterpart', 'absence'] },
  note: { type: 'string' } } }

// Heavy survey product — written to disk, read by the ideate stage; inventory-shaped, one anchored fact per entry.
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['entries', 'coverage', 'summary'], properties: {
  entries: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'kind', 'files', 'info', 'anchors', 'members'], properties: {
    target: { type: 'string' }, // page, seam, catalog, or card the entry grounds
    kind: { type: 'string', enum: ['realized', 'gap', 'cross-folder', 'existing-card', 'stacking'] },
    files: { type: 'array', items: { type: 'string' } }, // files the ideate stage must open for this entry
    info: { type: 'string' }, // the fact: realized capability, deferred gap, seam endpoints — prose truth, zero prescriptions
    anchors: { type: 'array', items: ANCHOR }, // exact coordinates backing the fact
    members: { type: 'array', items: { type: 'string' } } } } }, // verified catalog members backing a stacking entry
  coverage: { type: 'object', additionalProperties: false, required: ['requested', 'read', 'skipped', 'unverified'], properties: {
    requested: { type: 'array', items: { type: 'string' } },
    read: { type: 'array', items: { type: 'string' } },
    skipped: { type: 'array', items: { type: 'string' } },
    unverified: { type: 'array', items: { type: 'string' } } } },
  summary: { type: 'string' } } }

// Thin wire receipt: the survey lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
const RECEIPT = { type: 'object', additionalProperties: false, required: ['ok', 'report', 'entries', 'headline', 'failure'], properties: {
  ok: { type: 'boolean' }, report: { type: 'string' }, entries: { type: 'integer' },
  headline: { type: 'string' }, failure: { type: 'string' } } }

// Required-but-possibly-empty `beyondFolder` is an attestation: the cross-folder Ripple hunt ran, and every counterpart landed.
const CARDLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'beyondFolder', 'verdict', 'summary', 'applied'], properties: { files: { type: 'array', items: { type: 'string' } }, beyondFolder: { type: 'array', items: { type: 'string' } }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'refined', 'hardened', 'clean'] }, summary: { type: 'string' } } }

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

const INFO_LAW = 'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at each realized site, seam ' +
  'endpoints on both sides, verified member spellings, gaps. The ideate stage decides what to card; a map entry that tells it what to write ' +
  'instead of what is true is a defect. ENTRY FORM: `info` is prose truth; `anchors` carry one coordinate per row (role names what it proves; ' +
  '`note` is the shortest literal witness under 20 words, or empty when path+line suffice; an `absence` anchor names where the expected thing ' +
  'was searched and not found); `files` lists what the ideate stage must open for the entry; `members` are verified catalog spellings backing a ' +
  'stacking entry. A stacking or gap entry is INVENTORY, never instruction: verified members, current usage anchors, the concept that admits it — ' +
  'the ideate stage decides whether it cards. COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you actually ' +
  'full-read, `skipped`/`unverified` = what you did not reach — an honest skip beats a silent one.'

const SELF_CHECK = 'MANDATORY SELF-VERIFY (second pass, before returning): adversarially re-derive every entry from disk — re-open each cited ' +
  'anchor and confirm it states what the entry claims, re-verify each member spelling against its catalog, trace each cross-folder seam to both ' +
  'endpoints. An entry that fails re-confirmation is corrected or deleted, never returned; a guess, an assumption, a skimmed summary, or a ' +
  'vague/hedged entry is a defect. Completeness is part of correctness: after the re-read, hunt once more for what the first pass missed — an ' +
  'omitted load-bearing fact is as wrong as a false one.'

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
// liveness (never relaunching a live run), and returns a thin RECEIPT — the product stays on disk for the
// ideate stage. It never does, edits, judges, or relays the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
    'RECEIPT for its on-disk report. Never perform, edit, judge, soften, summarize, or RELAY the work itself.',
  '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' + CODEX_DIR + '; purge stale lane artifacts (a leftover report would READY instantly with last run\'s data): rm -f ' + base + '-report.json ' + base + '-stderr.log; Write the TASK block below verbatim to ' + base + '-task.md; Write this JSON ' +
    'Schema exactly to ' + base + '-schema.json — both paths resolved ABSOLUTE under the repository root: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' + base + '-task.md && test -s ' + base + '-schema.json || echo FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never ' +
    'retyped or reflowed (every token matters: dropping </dev/null makes codex block forever on stdin, ' +
    'zero-CPU, no report): ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral -c mcp_servers={} ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>' + base + '-stderr.log &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rptPat + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rptPat + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). LIVENESS IS NOT HEALTH: after the 4th RUNNING ' +
    'poll (~20 min wall) the run is WEDGED, not slow — kill it (pkill -f "' + rptPat + '") and go to (4) as GONE. ' +
    'Cap at 7 poll calls total.',
  '(4) READY: do NOT relay the report body through your output — build the MECHANICAL headline with jq (never your own ' +
    'judgment): entries=$(jq \'.entries | length\' ' + base + '-report.json); kinds=$(jq -r \'[.entries[].kind] | group_by(.) | map("\\(.[0])x\\(length)") | join(",")\' ' + base + '-report.json). ' +
    'Return the RECEIPT: ok=true, report=' + base + '-report.json, entries=that count, headline="<entries> entries | <kinds>", failure empty. GONE with no report: tail -5 ' + base + '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
    'foreground) and resume polling; a second GONE returns ok=false, entries=0, report and headline empty, failure=the stderr tail in one line.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise. The roster row
// carries `scope` from the ORCHESTRATOR (never the lane's self-report) so a failed lane's unmapped territory is
// exact even when the lane died before writing anything.
const recon = (task, o) => (CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL })
  : agent(task + '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    CODEX_DIR + '/' + fileTag(o.label) + '-report.json (Write tool, absolute path under the repo root): ' +
    JSON.stringify(o.schema) + ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
    { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL })
).then((r) => ({ lane: o.label, scope: o.scope || [], ok: !!(r && r.ok && r.report), report: (r && r.report) || '',
  entries: (r && r.entries) || 0, headline: (r && r.headline) || '', failure: (r && r.failure) || (r ? '' : 'lane died') }))
const surveyPrompt = (folder, slice) => [LAW, '', INFO_LAW, '', SELF_CHECK, '', 'TASK: DISCOVERY SURVEY of ' + folder + ' — read-only is its ONLY concession; skimming, memory-recall ' +
  'inventories, and verdict-only output are process defects. FULL-FILE read every design page under ' + folder + '/.planning/**, plus ' +
  'ARCHITECTURE.md, README.md, and any existing ' + folder + '/IDEAS.md + TASKLOG.md; walk the folder at large and enumerate both .api tiers per ' +
  'the ULTRA-STACKING LAW; resolve scope against real disk state. Produce ANCHORED MAP ENTRIES, never a verdict — each entry a fact with exact ' +
  'anchors, kind naming its role: `realized` = the capability a page composes NOW (never to be carded); `gap` = genuinely-deferred capability the ' +
  'REAL domain needs beyond the realized set — research the domain, never guess — each named with concrete verified members, never a phantom; ' +
  '`stacking` = an admitted .api capability no page or card exploits, carrying verified members and the page whose concept admits it; ' +
  '`cross-folder` = a contextual seam where a card should Ripple to a sibling owner, anchored on both endpoints; `existing-card` = a current pool ' +
  'card and its state. `summary` carries stacking guidance plus a hostile weak/strong call per page. The map is an initial pointer for downstream ' +
  'stages, never a ceiling.' + (slice ? '\n' + slice : '')].join('\n')
const slicePrompt = (folder, part, pages) => 'PAGE SLICE ' + part + ' of 2 — ' + folder + ' holds ' + pages + ' design pages, so the survey is split ' +
  'and this directive NARROWS the every-page read above: enumerate every design page under ' + folder + '/.planning/** sorted lexicographically by ' +
  'path; FULL-FILE read ' + (part === 1 ? 'ONLY the FIRST half of that ordering (pages 1..ceil(N/2))' : 'ONLY the SECOND half of that ordering (the ' +
  'pages after ceil(N/2))') + '. ARCHITECTURE.md, README.md, the existing card files, and both .api tiers stay FULL reads; your map covers your slice pages.'
const ideatePrompt = (folder, roster, unmapped) => [LAW, '', 'TASK: author/rebuild the IDEAS + TASKS pool in ' + folder + '/IDEAS.md ' +
  'and ' + folder + '/TASKLOG.md. The survey REPORT FILES are your reconnaissance, never a ceiling. CONSUMPTION: (a) UNMAPPED scope below gets your ' +
  'own cold read FIRST; (b) read every ok survey report IN FULL from disk and dedupe entries by target as you read; (c) each entry\'s anchors are ' +
  'jump coordinates — spot-verify what you build on — information, not instructions. Then re-read the corpus and both .api tiers yourself and EXCEED ' +
  'the survey; it never licenses a skim. Author IDEAS for the conceptual/multi-step gaps and TASKS for the concrete/deferred ones; each card dense ' +
  'at the 7-point rubric and naive on neither axis, genuinely-deferred ONLY (never a realized page from the survey), correctly anchored, with every ' +
  'Ripple bidirectional per the RIPPLE + CURRENT-STATE laws — the cross-folder counterpart referenced where it exists, authored in the sibling ' +
  'folder NOW where it does not. Match the exact card schema from libs/.planning/README.md and the voice from campaign-method.md. Fix-in-place ' +
  '(write the files, create if absent). Return the card-log listing every file you edited, sibling folders included. ' +
  'UNMAPPED: ' + JSON.stringify(unmapped) + ' ROSTER: ' + JSON.stringify(roster)].join('\n')
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
        { label: 'survey:' + nameOf(folder) + '#' + part, phase: 'Survey', schema: SURVEY_SCHEMA, scope: [folder + ' (slice ' + part + '/2)'] })))
    : [slot(() => recon(surveyPrompt(folder, ''), { label: 'survey:' + nameOf(folder), phase: 'Survey', schema: SURVEY_SCHEMA, scope: [folder] }))]
  const roster = (await Promise.all(surveyors)).filter(Boolean)
  const mapped = roster.filter((r) => r.ok)
  const total = mapped.reduce((a, r) => a + r.entries, 0)
  const unmapped = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })))
  log(nameOf(folder) + ': ' + total + ' survey entries across ' + mapped.length + '/' + roster.length + ' lane(s)' +
    (mapped.length < roster.length ? ' — FAILED: ' + roster.filter((r) => !r.ok).map((r) => r.lane).join(', ') : ''))
  const authored = await slot(() => agent(ideatePrompt(folder, roster, unmapped), { label: 'ideate:' + nameOf(folder), phase: 'Ideate', model: 'fable', schema: CARDLOG_SCHEMA, effort: 'high', stallMs: STALL }))
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
