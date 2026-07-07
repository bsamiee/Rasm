export const meta = {
  name: 'realize',
  whenToUse: 'Execute a root campaign DECISION/brief into its target planning folder. args = {doc, root} or an array of such pairs; campaigns run as parallel lanes. Wide read-only gpt-5.5 (codex) reconnaissance hands one fable executor everything on a golden platter — the doc, exact disk locations, ripple endpoints across every libs/ folder, and the full two-tier .api stacking inventory — and the executor implements the whole campaign under the language doctrine with libs-wide ripple authority. Run cold-verify after.',
  description: 'Campaign realization in two moves. RECON: gpt-5.5 (codex) mapper lanes fan out read-only through sonnet dispatch wrappers (CODEX flag; false restores native opus) — page-slice mappers (root doc IN FULL + slice pages IN FULL + sibling context), one governance mapper (manifests, csproj/README registries, .api anchors, index docs), two stacking mappers (one per .api tier: the shared libs/<lang>/.api substrate and the folder <root>/.api tier, full inventory mined to operator depth against the doc page set — doc-demanded AND beyond-doc underutilized capability, exact verified members), and ONE BRANCH RIPPLE MAPPER PER language lib (libs/csharp, libs/python, libs/typescript — each sweeping its branch package folders except the target, skipping the branch .planning core; seam ledgers, consumer anchors, counterpart obligations, frozen wire names the campaign touches). Every mapper runs a mandatory second-pass self-verify: each entry re-derived from disk, anchors re-opened, spellings re-checked; a guess, a skim, or a vague entry is deleted before return. Mappers provide information — locations, anchors, ripples, inventory — never prescriptions. EXECUTE: one fable per campaign takes the root doc plus every map and implements everything in place at the docs/stacks/<language>/ bar: the full page set, registry and index closure, every ripple in the same pass with LIBS-WIDE ripple authority (sibling counterparts repaired in place both ends, except where the doc rules a counterpart recorded-only), the two-tier stacking woven in, its own hunt past the maps. No stage after the executor; cold-verify is the separate closure gate.',
  phases: [
    { title: 'Plan', detail: 'per campaign: enumerate the page set (disk + doc-ruled new pages), partition into mapper slices', model: 'sonnet' },
    { title: 'Recon', detail: 'per campaign: page-slice + governance + two-tier stacking + per-branch ripple mappers on gpt-5.5 via codex wrappers (sonnet shells), read-only, self-verified information not prescriptions', model: 'sonnet' },
    { title: 'Execute', detail: 'per campaign: one fable implements the whole campaign — doc + maps + doctrine, every ripple in-pass, libs-wide', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const SLICE_SIZE = 4
const MAX_SLICES = 8
const STALL = 300000
const CODEX = true
const SCRATCH = '.claude/scratch/realize' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------

const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const CAMPS = (Array.isArray(argsIn) ? argsIn : [argsIn]).filter((c) => c && c.doc && c.root)
if (!CAMPS.length) { log('No campaigns — pass {doc, root} or an array of pairs.'); return { campaigns: 0 } }
const langOf = (root) => root.indexOf('libs/csharp') === 0 ? 'cs' : root.indexOf('libs/python') === 0 ? 'py' : 'ts'
const STACK = { cs: 'docs/stacks/csharp', py: 'docs/stacks/python', ts: 'docs/stacks/typescript' }
const TIER = { cs: 'libs/csharp/.api', py: 'libs/python/.api', ts: 'libs/typescript/.api' }
const chunk = (a, n) => { const o = []; for (let i = 0; i < a.length; i += n) o.push(a.slice(i, i + n)); return o }

// --- [MODELS] ----------------------------------------------------------------------------

const PLAN = { type: 'object', additionalProperties: false, required: ['pages'], properties: {
  pages: { type: 'array', items: { type: 'string' } } } } // disk pages + doc-ruled new pages, dependency order

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR = { type: 'object', additionalProperties: false, required: ['path', 'line', 'role', 'note'], properties: {
  path: { type: 'string' }, line: { type: 'integer' },
  role: { type: 'string', enum: ['state', 'ruling', 'catalog', 'counterpart', 'absence'] },
  note: { type: 'string' } } }

const MAP = { type: 'object', additionalProperties: false, required: ['entries', 'coverage', 'summary'], properties: {
  entries: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'kind', 'files', 'info', 'anchors', 'members'], properties: {
    target: { type: 'string' }, // page, registry surface, or catalog the entry grounds
    kind: { type: 'string', enum: ['state', 'ruling-site', 'ripple', 'stacking', 'gap', 'registry'] },
    files: { type: 'array', items: { type: 'string' } }, // files the executor must open for this entry
    info: { type: 'string' }, // the fact: current shape, seam endpoints, gap — prose truth, zero prescriptions
    anchors: { type: 'array', items: ANCHOR }, // exact coordinates backing the fact
    members: { type: 'array', items: { type: 'string' } } } } }, // verified catalog members backing a stacking entry
  coverage: { type: 'object', additionalProperties: false, required: ['requested', 'read', 'skipped', 'unverified'], properties: {
    requested: { type: 'array', items: { type: 'string' } },
    read: { type: 'array', items: { type: 'string' } },
    skipped: { type: 'array', items: { type: 'string' } },
    unverified: { type: 'array', items: { type: 'string' } } } },
  summary: { type: 'string' } } }

// Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
const RECEIPT = { type: 'object', additionalProperties: false, required: ['ok', 'report', 'entries', 'headline', 'failure'], properties: {
  ok: { type: 'boolean' }, report: { type: 'string' }, entries: { type: 'integer' },
  headline: { type: 'string' }, failure: { type: 'string' } } }

const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'built', 'beyond', 'blocked', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  built: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  beyond: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  blocked: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'reason'], properties: {
    target: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' } } }

// --- [SHARED_BLOCKS] ---------------------------------------------------------------------

const CTX = (c) => 'Rasm monorepo, planning phase — the work product is design pages, index docs, central manifests, and .api ' +
  'catalogs; no source files land. ' + c.doc + ' at the repo root is the binding campaign ruling for ' + c.root + '. All .md ' +
  'prose follows docs/standards/style-guide.md: declarative agent-facing law, no provenance, no process narration, no hedges. ' +
  'Never run git commit.'

const INFO_LAW = 'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at each ' +
  'ruling site, seam endpoints on both sides, verified member spellings, gaps. The executor decides how to build; a map entry ' +
  'that tells it what to write instead of what is true is a defect. ENTRY FORM: `info` is prose truth; `anchors` carry one ' +
  'coordinate per row (role names what it proves; `note` is the shortest literal witness under 20 words, or empty when ' +
  'path+line suffice; an `absence` anchor names where the expected thing was searched and not found); `files` lists what the ' +
  'executor must open for the entry. An underutilized-capability entry is INVENTORY, never instruction: verified members, ' +
  'current usage anchors, the concept that admits it — the executor decides whether it composes. COVERAGE is part of the ' +
  'product: `requested` = your assigned scope, `read` = what you actually full-read, `skipped`/`unverified` = what you did ' +
  'not reach — an honest skip beats a silent one.'

const SELF_CHECK = 'MANDATORY SELF-VERIFY (second pass, before returning): adversarially re-derive every entry from disk — ' +
  're-open each cited anchor and confirm it states what the entry claims, re-verify each member spelling against its ' +
  'catalog, trace each ripple to both endpoints. An entry that fails re-confirmation is corrected or deleted, never ' +
  'returned; a guess, an assumption, a skimmed summary, or a vague/hedged entry is a defect. Completeness is part of ' +
  'correctness: after the re-read, hunt once more for what the first pass missed — an omitted load-bearing fact is as ' +
  'wrong as a false one.'

// --- [OPERATIONS] ------------------------------------------------------------------------

// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// SCRATCH, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns a thin RECEIPT — the product stays on disk for the
// terminal reader. It never does, edits, judges, or relays the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = SCRATCH + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
    'RECEIPT for its on-disk report. Never perform, edit, judge, soften, summarize, or RELAY the work itself.',
  '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' + SCRATCH + '; purge stale ' +
    'lane artifacts (a leftover report would READY instantly with last run\'s data): rm -f ' + base + '-report.json ' + base + '-stderr.log; Write the TASK block below verbatim to ' + base + '-task.md; Write this JSON ' +
    'Schema exactly to ' + base + '-schema.json — both paths resolved ABSOLUTE under the repository root: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' + base + '-task.md && test -s ' + base + '-schema.json || echo ' +
    'FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never ' +
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
    'judgment): entries=$(jq \'.entries | length\' ' + base + '-report.json); kinds=$(jq -r \'[.entries[].kind] | group_by(.) | map(.[0] + "x" + (length|tostring)) | join(",")\' ' + base + '-report.json). ' +
    'Return the RECEIPT: ok=true, report=' + base + '-report.json, entries=that count, ' +
    'headline="<entries> entries | <kinds>", failure empty. GONE with no report: tail -5 ' + base +
    '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
    'foreground) and resume polling; a second GONE returns ok=false, entries=0, report and headline empty, failure=the stderr tail in one line.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise.
const recon = (task, o) => (CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL })
  : agent(task + '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    SCRATCH + '/' + fileTag(o.label) + '-report.json (Write tool, absolute path under the repo root): ' +
    JSON.stringify(o.schema) + ' — then return ONLY the receipt: ok, report path, entries count, one-line headline, failure empty.',
    { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL })
).then((r) => ({ lane: o.label, scope: o.scope || [], ok: !!(r && r.ok && r.report), report: (r && r.report) || '',
  entries: (r && r.entries) || 0, headline: (r && r.headline) || '', failure: (r && r.failure) || (r ? '' : 'lane died') }))

// --- [COMPOSITION] -----------------------------------------------------------------------

const lanes = await parallel(CAMPS.map((c) => async () => {
  const tag = c.root.split('/').pop()
  const lang = langOf(c.root)
  const plan = await agent(CTX(c) + '\n\nTASK: thin enumerate (read-only). Union (a) every design page under ' + c.root +
    '/.planning/ from a real listing and (b) every page ' + c.doc + ' rules into existence that is absent on disk. Return ' +
    'the union as one dependency-ordered list (foundations before consumers, per the doc where it rules an order).',
    { label: 'plan:' + tag, phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN, stallMs: STALL })
  const pages = (plan && plan.pages) || []
  const slices = chunk(pages, Math.max(SLICE_SIZE, Math.ceil(pages.length / MAX_SLICES)))
  const mappers = slices.map((s, i) => () => recon(CTX(c) + '\n\n' + INFO_LAW + '\n\n' + SELF_CHECK + '\n\nTASK: PAGE-SLICE MAP, slice ' + i +
    ' (read-only). Read ' + c.doc + ' IN FULL, then each of these pages IN FULL (a doc-ruled page absent on disk maps its ' +
    'ruling plus the nearest sibling context instead): ' + JSON.stringify(s) + '. For each page return: its current state ' +
    'against every doc ruling that touches it (exact anchors), every seam/consumer ripple endpoint on both sides, every ' +
    'catalog its fences cite with the cited members verified against the .api tier, and every gap between the ruling and ' +
    'disk. Entries are facts with anchors.',
    { label: 'map:' + tag + ':s' + i, phase: 'Recon', schema: MAP, scope: s }))
  mappers.push(() => recon(CTX(c) + '\n\n' + INFO_LAW + '\n\n' + SELF_CHECK + '\n\nTASK: GOVERNANCE MAP (read-only). Map the campaign governance ' +
    'surface: ' + c.root + '/README.md and ARCHITECTURE.md (router, package groups, codemap, seams ledger), the folder ' +
    'manifest surface (' + (lang === 'cs' ? 'the .csproj + the Directory.Packages.props rows this package consumes'
      : 'the pyproject.toml rows this package consumes') + '), the ' + c.root + '/.api/ inventory with each catalog\'s ' +
    'anchor state, and every registry fact ' + c.doc + ' rules. Return the exact current state of each surface and every ' +
    'disagreement between two surfaces, with anchors.',
    { label: 'map:' + tag + ':gov', phase: 'Recon', schema: MAP, scope: [c.root + '/README.md', c.root + '/ARCHITECTURE.md', c.root + '/.api', 'manifest rows'] }))
  mappers.push(() => recon(CTX(c) + '\n\n' + INFO_LAW + '\n\n' + SELF_CHECK + '\n\nTASK: SUBSTRATE-TIER STACKING MAP (read-only). Enumerate ' +
    TIER[lang] + '/ with a real listing and read every catalog relevant to this campaign. Against the full page set (' +
    JSON.stringify(pages) + ') and the doc rulings, return every substrate capability the corpus composes, under-composes, ' +
    'or ignores — doc-demanded AND beyond-doc: {catalog, verified member spellings, the page whose concept admits it, the ' +
    'integration shape as fact}. Depth over breadth: operator-level members, never headline names.',
    { label: 'map:' + tag + ':tier1', phase: 'Recon', schema: MAP, scope: [TIER[lang]] }))
  mappers.push(() => recon(CTX(c) + '\n\n' + INFO_LAW + '\n\n' + SELF_CHECK + '\n\nTASK: FOLDER-TIER STACKING MAP (read-only). Enumerate ' + c.root +
    '/.api/ with a real listing and read EVERY catalog. Against the full page set (' + JSON.stringify(pages) + ') and the ' +
    'doc rulings, return every folder-package capability the corpus composes, under-composes, or ignores — doc-demanded AND ' +
    'beyond-doc underutilized capability: {catalog, verified member spellings, the page whose concept admits it, the ' +
    'integration shape as fact}. An admitted capability no page exploits is a named entry, never a silence.',
    { label: 'map:' + tag + ':tier2', phase: 'Recon', schema: MAP, scope: [c.root + '/.api'] }))
  const BRANCHES = ['libs/csharp', 'libs/python', 'libs/typescript']
  BRANCHES.forEach((branch) => mappers.push(() => recon(CTX(c) + '\n\n' + INFO_LAW + '\n\n' + SELF_CHECK + '\n\nTASK: ' +
    'BRANCH RIPPLE MAP for ' + branch + ' (read-only). Read ' + c.doc + ' IN FULL, then sweep every package folder ' +
    'under ' + branch + ' EXCEPT ' + c.root + ' — each folder\'s ARCHITECTURE seams ledger and codemap, README router ' +
    'and package groups, index docs, design pages, and .api catalogs that name, anchor, or compose ' + c.root + ' ' +
    'content (real grep, never memory). SKIP the branch core ' + branch + '/.planning entirely. Return every ' +
    'touchpoint the campaign rulings touch: seam rows anchored to pages the doc splits, renames, re-points, or ' +
    'deletes; consumer-cited anchors into the target; counterpart obligations the doc records with a folder in this ' +
    'branch as an endpoint; frozen wire names and shapes both ends hold byte-identical; shared manifest rows and ' +
    '.api tier overlaps. Each entry is a fact with exact anchors on BOTH endpoints. Zero genuine touchpoints is a ' +
    'valid result — return empty entries, never manufactured ripples.',
    { label: 'map:' + tag + ':rip:' + branch.split('/').pop(), phase: 'Recon', schema: MAP, scope: [branch] })))
  const roster = (await parallel(mappers)).filter(Boolean)
  const mapped = roster.filter((r) => r.ok)
  const total = mapped.reduce((a, r) => a + r.entries, 0)
  const unmapped = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })))
  log(tag + ': ' + total + ' map entries across ' + mapped.length + '/' + roster.length + ' lanes' +
    (mapped.length < roster.length ? ' — FAILED: ' + roster.filter((r) => !r.ok).map((r) => r.lane).join(', ') : ''))
  const fix = await agent(CTX(c) + '\n\nTASK: EXECUTE THE CAMPAIGN (WRITER — full authority over ' + c.root + ', its ' +
    'manifest rows, its .api tier, the index docs, AND libs-wide ripple authority: a ripple your edits expose anywhere ' +
    'under libs/ — sibling seam ledgers, mirrored ARCHITECTURE rows, consumer page anchors, index docs, .api catalogs — ' +
    'is repaired in place at the sibling in the same pass, both ends, so the tree ends coherent, never disjointed; ' +
    'where ' + c.doc + ' explicitly rules a counterpart RECORDED with a demanding consumer rather than edited, the ' +
    'recording IS the fix and the sibling interior stays unedited past that ruling. You are the run\'s LAST agent). ' +
    'Read ' + c.doc + ' IN FULL — it is ' +
    'the ruling. Read the ' + STACK[lang] + '/ doctrine at source (README and every page it routes) — it is the bar. The ' +
    'recon REPORT FILES are your reconnaissance. CONSUMPTION: (a) UNMAPPED scope below gets your own cold read FIRST; (b) read every ok report IN FULL from disk — governance and the two stacking maps before page slices, ripple maps before the pages whose seams they anchor; entries overlap across lanes, dedupe by target as you read; (c) each entry\'s anchors are jump coordinates — spot-verify what you build on, and re-open every anchor behind an edit — ' +
    'information, not instructions; spot-verify what you build on, and hunt past them on your own authority. IMPLEMENT ' +
    'EVERYTHING the doc rules, in dependency order: author ruled-new pages ground-up, rebuild and improve ruled pages in ' +
    'place, weave the stacking inventory into the owning pages as cases, rows, fields, and operations (doc-demanded and ' +
    'beyond-doc alike — an admitted capability the concept admits is composed, never noted), close the registry and index ' +
    'docs, and repair every ripple your edits expose in the same pass, both seam ends — the sibling ripple maps ' +
    'enumerate the known cross-folder endpoints; cover every one and hunt past them. A ruling is a FLOOR: exceed it with ' +
    'denser, deeper, more capable form; frozen signatures and wire names stay byte-identical. `blocked` carries only what ' +
    'is genuinely unreachable (a gated cross-package producer), with reason — never deferred work. ' +
    'UNMAPPED: ' + JSON.stringify(unmapped) + ' ROSTER: ' + JSON.stringify(roster),
    { label: 'execute:' + tag, phase: 'Execute', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL })
  return { campaign: c.root, lanes: roster.length, failedLanes: roster.filter((r) => !r.ok).map((r) => r.lane),
    mapEntries: total, built: (fix && fix.built && fix.built.length) || 0,
    beyond: (fix && fix.beyond && fix.beyond.length) || 0, blocked: (fix && fix.blocked) || [],
    summary: (fix && fix.summary) || '' }
}))

return { campaigns: lanes.filter(Boolean) }
