export const meta = {
  name: 'finalize',
  whenToUse: 'The finalization engine — the complement to rebuild: where rebuild improves and extends, finalize corrects and closes. Run it over a package (or folder subset) whose build passes have landed to kill split-brain, unify paradigms, adjudicate phantoms, collapse duplication and indirection in place, smooth logic flow end-to-end, and finish naive surfaces — every folder concurrent under one pool cap, every defect fixed by the run that finds it.',
  description: 'Language-agnostic FINALIZATION pass over one libs/{csharp,python,typescript} package planning corpus. args = a package root, an array of planning sub-folders, or {targets}; language derives from the root and selects the doctrine root pages, both .api tiers, the manifest, and the member-verification rail. Plan (1 sonnet) enumerates sub-folders + pages and emits folders in dependency order — the order biases launch, never serializes. All folders run CONCURRENTLY under one pool cap: Context — ONE sonnet agent per folder reads the package README/ARCHITECTURE, its manifest rows, and a real ls of both .api tiers ONCE, writing a grounding dossier file (verbatim extracts with file:line anchors, pointer rows for the tail — facts only, never verdicts) under .claude/scratch/; Census — ONE read-only lane PER PAGE on gpt-5.5 dispatched through a sonnet codex wrapper (CODEX flag; false restores native opus) reads the grounding dossier + its page + its related pages + every relevant .api catalog, spot-verifying dossier anchors on disk instead of re-reading the whole shared context, and writes the correction census (underutilized capability, hand-rolled reimplementation, phantoms classified forgotten-vs-lie, split-brain, unnecessary differentiation, naive fields, logic-flow breaks, stale references) as one typed report file under .claude/scratch/, returning a thin receipt; Fix — ONE fable agent per folder, chained only behind its own folder\'s census, holds the folder census roster (every ok report read IN FULL from disk; a failed lane\'s page cold-read directly) + the grounding dossier + the doctrine ROOT pages and corrects the folder in place under the fix-it-now law (every defect its work exposes, anywhere in the project, fixed in the same pass) and the current-state law (concurrently landed sibling corrections composed as found, conflicts resolved to the stronger form, never a revert). The package index docs and central manifests are the one single-writer surface: fixers report exact rows, ONE terminal fable writer applies them once. Nothing else follows the fixers; cold-verify is a separate run.',
  phases: [
    { title: 'Plan', detail: 'one thin agent enumerates the planning sub-folders and their design pages, folders emitted in dependency order (foundations first) as a launch bias only; one bounded re-attempt on a dead agent', model: 'sonnet' },
    { title: 'Finalize', detail: 'all folders concurrent under the one pool cap: one sonnet context agent per folder writes the grounding dossier once, one gpt-5.5 census lane per page (codex wrapper, read-only) reads it plus its page and related pages and writes its census report to disk, then ONE fable fixer per folder chained only behind its own folder\'s census — fix-it-now write authority over the whole project, current-state composition of concurrent sibling work' },
    { title: 'Index', detail: 'one fable writer applies every reported package index-doc and central-manifest row exactly once; skipped when no rows were reported', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14
const STAGGER_MS = 1500
const STALL = 480000
const SCRATCH = '.claude/scratch/finalize' // per-folder grounding dossiers: shared-context extracts, facts only
const CODEX = true                         // census lanes run on gpt-5.5 via the codex wrapper; false restores native opus lanes

// --- [INPUTS] ----------------------------------------------------------------------------

const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '')
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const rawTargets = Array.isArray(argsIn) ? argsIn
  : (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.targets === 'string') ? [argsIn.targets]
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn]
  : []
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]
const langOf = (t) => t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null
const LANG_KEYS = [...new Set(TARGETS.map(langOf))]
const LANG_KEY = (LANG_KEYS.length === 1 && LANG_KEYS[0]) ? LANG_KEYS[0] : null

// --- [MODELS] ----------------------------------------------------------------------------

const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['root', 'folders'], properties: {
  root: { type: 'string' },
  folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'pages'], properties: {
    folder: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } } } } // ARRAY ORDER IS DEPENDENCY ORDER — launch bias only, nothing serializes on it

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR = { type: 'object', additionalProperties: false, required: ['path', 'line', 'role', 'note'], properties: {
  path: { type: 'string' }, line: { type: 'integer' },
  role: { type: 'string', enum: ['defect', 'ruling', 'catalog', 'counterpart', 'absence'] },
  note: { type: 'string' } } }

const CENSUS_SCHEMA = { type: 'object', additionalProperties: false, required: ['findings', 'coverage', 'summary'], properties: {
  findings: { type: 'array', items: { type: 'object', additionalProperties: false,
    required: ['claimKey', 'target', 'files', 'kind', 'severity', 'claim', 'anchors', 'mechanism', 'owner', 'reject', 'acceptance'], properties: {
    claimKey: { type: 'string' }, // <kind>|<owner>|<primary symbol or absence route> — stable across lanes, never lane wording
    target: { type: 'string' }, // short display label for the defect
    files: { type: 'array', items: { type: 'string' } }, // files the fixer must open or edit first
    kind: { type: 'string', enum: ['underutilized', 'handrolled', 'phantom_forgotten', 'phantom_lie', 'splitbrain', 'differentiation', 'naive', 'flow', 'stale'] },
    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence: closure-blocking | correctness | local cleanup
    claim: { type: 'string' }, // the observed defect as fact
    anchors: { type: 'array', items: ANCHOR },
    mechanism: { type: 'string' }, // WHY the current form fails the doctrine/domain — factual, zero repair verbs
    owner: { type: 'string' }, // canonical owner that must absorb the resolution
    reject: { type: 'array', items: { type: 'string' } }, // forms the repair must NOT take (deleted forms)
    acceptance: { type: 'array', items: { type: 'string' } } } } }, // signals proving resolution
  coverage: { type: 'object', additionalProperties: false, required: ['requested', 'read', 'skipped', 'unverified'], properties: {
    requested: { type: 'array', items: { type: 'string' } },
    read: { type: 'array', items: { type: 'string' } },
    skipped: { type: 'array', items: { type: 'string' } },
    unverified: { type: 'array', items: { type: 'string' } } } },
  summary: { type: 'string' } } }

// Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + counts travel inline.
const RECEIPT = { type: 'object', additionalProperties: false, required: ['ok', 'report', 'entries', 'headline', 'failure'], properties: {
  ok: { type: 'boolean' }, report: { type: 'string' }, entries: { type: 'integer' },
  headline: { type: 'string' }, failure: { type: 'string' } } }

// Required-but-possibly-empty indexRows is an attestation: no single-writer impact, never an unchecked surface.
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary', 'indexRows', 'collapsed', 'realized'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] },
  collapsed: { type: 'string' }, realized: { type: 'string' }, summary: { type: 'string' },
  indexRows: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['doc', 'row'], properties: {
    doc: { type: 'string' }, row: { type: 'string' } } } } } }

const INDEX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'applied', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  applied: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['doc', 'action'], properties: {
    doc: { type: 'string' }, action: { type: 'string' } } } },
  summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LANG = {
  cs: { name: 'C#', root: 'libs/csharp', shared: 'libs/csharp/.api',
    manifest: 'the package `.csproj` and the central `Directory.Packages.props` block for this package',
    verify: '`uv run python -m tools.assay api` (assay blocked or unavailable: the `.api` catalogs + the nuget MCP for feed truth + Context7/exa/tavily own the fallback)',
    stackLaw: 'the docs/stacks/csharp ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full (README, language, ' +
      'shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis); the domain/ sub-folder is OUT of this read' },
  py: { name: 'Python', root: 'libs/python', shared: 'libs/python/.api',
    manifest: 'the root `pyproject.toml` rows this package consumes',
    verify: '`uv run python -m tools.assay api resolve <pkg>` (blocked or gated: the `.api` catalogs + PyPI feed truth + Context7/exa/tavily own the fallback)',
    stackLaw: 'the docs/stacks/python ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full in the README ' +
      '[01]-[ATLAS] order (language, shapes, iteration, surfaces-and-dispatch, rails-and-effects, concurrency, boundaries, algorithms, ' +
      'system-apis, runtime); the domain/ and numerics/ sub-folders are OUT of this read' },
  ts: { name: 'TypeScript', root: 'libs/typescript', shared: 'libs/typescript/.api',
    manifest: 'the `pnpm-workspace.yaml` / package manifest rows this area consumes',
    verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
    stackLaw: 'the docs/stacks/typescript ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full (README, ' +
      'language, derivation, values, computation, shapes, surfaces-and-dispatch, rails-and-effects, services-and-layers, concurrency, ' +
      'streams, boundaries)' },
}

const L = LANG[LANG_KEY] || LANG.cs // inert fallback: composition no-ops before any agent call when LANG_KEY is null

const LAW = [
  'Rasm monorepo, ' + L.name + ' planning corpus (markdown design pages; code fences are the product). This is a FINALIZATION pass, not a ' +
    'redesign: the rebuild engine improves and extends; THIS engine corrects and CLOSES. The targets are: split-brain and dual paradigms ' +
    'unified onto ONE rail/paradigm per the doctrine; unnecessary differentiation (parallel shapes, sibling name families, twin entries for ' +
    'one concept) collapsed into the polymorphic owner IN PLACE — never a new file; duplication and indirection reduced; PHANTOM code ' +
    'adjudicated — a cited member/type/seam that does not exist is either FORGOTTEN FUNCTIONALITY (intent evidence stands in seams, README ' +
    'rows, growth clauses, or sibling contracts — realize it NOW, properly and at full depth, as if it had always been there) or a LIE ' +
    '(no intent evidence — delete it); naive/mini fields and capability-thin owners grown to the real concept the domain demands; logic flow ' +
    'smoothed END-TO-END (admission once -> canonical owner -> unified rail -> projection -> egress; no dead ends, no rail breaches, no ' +
    'unreachable cases, no orphaned carriers); density/complexity/richness pushed and LOC reduced THROUGH density, never through capability ' +
    'loss. ALL capability is preserved — deletion exists only for proven lies and true duplicates.',
  'REMOVAL DISCIPLINE binds: a package admission, a capability, or a domain concept is never removed for underutilization — underutilization ' +
    'is an integration defect this pass CLOSES by wiring the capability into its owner. The only sanctioned deletions are phantom LIES and ' +
    'exact duplicates whose one surviving owner carries everything.',
  'WRITE-FULLY MANDATE: every fix identified is made NOW via Edit/Write; the fix-log REPORTS edits already made, never a to-do list.',
].join('\n')

const FIX_NOW = 'FIX-IT-NOW: this run is self-enclosing — a defect your work exposes is YOURS in the same pass, wherever it lives: another ' +
  'folder\'s page, a seam counterpart, a consumer site, a `.api` anchor. No residual, no routing, no later phase, no report-instead-of-edit. ' +
  'The ONE exception is the shared single-writer surface — the package README.md/ARCHITECTURE.md and the central manifests: report the exact ' +
  'row you need in `indexRows`; one terminal writer applies every reported row once.'

const CURRENT_STATE = 'CURRENT STATE: sibling folders land corrections concurrently with yours. Before any edit, re-read the CURRENT on-disk ' +
  'state of your pages AND every sibling page they compose or ripple into; landed sibling work is composed as found, never assumed; a ' +
  'conflict between your correction and a landed sibling resolves to the stronger form, never a revert.'

const BOUNDARY = 'BOUNDARY LAW: cross-package and cross-language wire-canonical seam names are FROZEN — repair a seam recording, never rename ' +
  'the wire. Strata hold: no downward dependency, no host-type leak into a host-neutral owner.'

const EVIDENCE_LAW = 'FINDING FORM — you deliver TRUTH, never an implementation: `claim` states the observed defect; `mechanism` states WHY ' +
  'it fails the doctrine or the domain as fact; `anchors` carry one coordinate per row (role names what the coordinate proves; `note` is the ' +
  'shortest literal witness — a symbol, member spelling, or fragment under 20 words — or empty when path+line suffice; an `absence` anchor ' +
  'names where the expected thing was searched and not found); `files` lists what the fixer must open first; `owner` names the canonical ' +
  'owner that must absorb the resolution (the owning axis, row roster, registry, or seam vocabulary — never a new local shape); `reject` ' +
  'lists the deleted forms the repair must not take; `acceptance` lists the signals that prove resolution. NEVER write ' +
  'add/replace/implement/promote/delete as instruction — the fixer owns the design; you own the constraint boundary. `claimKey` = ' +
  '<kind>|<owner>|<primary symbol or absence route>, identical for the same defect regardless of lane or wording. `severity` binds to ' +
  'consequence: blocker = closure-blocking, major = correctness, minor = local cleanup — never prose confidence. OUTPUT BOUNDS: an ordinary ' +
  'page yields 3-8 retained findings; 0 only when the full read comes back empty, and then `summary` names the probes that produced nothing; ' +
  'never manufacture a finding to fill the range, never delete a confirmed one to stay inside it. COVERAGE is part of the product: ' +
  '`requested` = your assigned scope, `read` = what you actually full-read, `skipped`/`unverified` = what you did not reach or could not ' +
  'confirm — an honest skip beats a silent one.'

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

// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// SCRATCH, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns a thin RECEIPT — the product stays on disk for the
// folder fixer. It never does, edits, judges, or relays the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = SCRATCH + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
    'RECEIPT for its on-disk report. Never perform, edit, judge, soften, summarize, or RELAY the work itself.',
  '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' + SCRATCH + '; purge stale lane artifacts (a leftover report would READY instantly with last run\'s data): rm -f ' + base + '-report.json ' + base + '-stderr.log; Write the TASK block below verbatim to ' + base + '-task.md; Write this JSON ' +
    'Schema exactly to ' + base + '-schema.json — both paths resolved ABSOLUTE under the repository root: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' + base + '-task.md && test -s ' + base + '-schema.json || echo FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never retyped or reflowed (every token matters: dropping </dev/null makes codex block forever on stdin, zero-CPU, no report): ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral -c mcp_servers={} ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>' + base + '-stderr.log &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rptPat + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rptPat + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). LIVENESS IS NOT HEALTH: after the 4th RUNNING poll (~20 min wall) the run is WEDGED, not slow — kill it (pkill -f "' + rptPat + '") and go to (4) as GONE. Cap at 7 poll calls total.',
  '(4) READY: do NOT relay the report body through your output — build the MECHANICAL headline with jq (never your own ' +
    'judgment): entries=$(jq \'.findings | length\' ' + base + '-report.json); kinds=$(jq -r \'[.findings[].kind] | group_by(.) | map("\\(.[0])x\\(length)") | join(",")\' ' + base + '-report.json); top=$(jq -r \'[.findings[].files[0]] | group_by(.) | max_by(length) | .[0] // "none"\' ' + base + '-report.json). ' +
    'Return the RECEIPT: ok=true, report=' + base + '-report.json, entries=that count, headline="<entries> findings | <kinds> | top: <top>", failure empty. ' +
    'GONE with no report: tail -5 ' + base + '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
    'foreground) and resume polling; a second GONE returns ok=false, entries=0, report and headline empty, failure=the stderr tail in one line.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise.
// The roster row carries `scope` from the ORCHESTRATOR (never the lane's self-report) so a failed lane's
// uncensused territory is exact even when the lane died before writing anything.
const recon = (task, o) => (CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL })
  : agent(task + '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    SCRATCH + '/' + fileTag(o.label) + '-report.json (Write tool, absolute path under the repo root): ' +
    JSON.stringify(o.schema) + ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
    { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL })
).then((r) => ({ lane: o.label, scope: o.scope || [], ok: !!(r && r.ok && r.report), report: (r && r.report) || '',
  entries: (r && r.entries) || 0, headline: (r && r.headline) || '', failure: (r && r.failure) || (r ? '' : 'lane died') }))

const planPrompt = () => ['Rasm monorepo. TASK: thin enumerate (read-only, do NOT edit). TARGETS (repo-relative): ' + JSON.stringify(TARGETS) +
  '. Each target is a package root (e.g. ' + L.root + '/<Package>) or a planning sub-folder (<root>/.planning/<Folder>). Resolve the ONE ' +
  'owning package root. Return root (the package root) and folders — one entry per planning sub-folder in scope (a package-root target ' +
  'expands to EVERY sub-folder under its .planning tree; explicit sub-folder targets restrict to exactly those): {folder: the sub-folder ' +
  'name, pages: repo-relative *.md paths under it}. EXCLUDE IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md and any _*.md campaign document. ' +
  'EMIT `folders` IN DEPENDENCY ORDER — foundations before their consumers, judged from the package README.md/ARCHITECTURE.md composition ' +
  'order; the engine launches in your emitted order and never re-sorts, so early rows tend to start early. Use find/ls; do NOT cd.'].join('\n')

const contextPrompt = (folder, root, dossier) => ['Rasm monorepo, ' + L.name + ' planning corpus. TASK: FOLDER GROUNDING for ' + folder +
  ' — read-only over the corpus; your ONLY write is the dossier file. Read ONCE the shared context every per-page census of this folder ' +
  'would otherwise re-read: the package README.md + ARCHITECTURE.md at ' + root + ', ' + L.manifest + ', and a real ls of BOTH .api tiers ' +
  '(' + L.shared + '/ AND the package .api/). Write the grounding dossier to ' + dossier + ': verbatim extracts WITH file:line anchors for ' +
  'the load-bearing content (the README/ARCHITECTURE rows and sections naming ' + folder + ' or its pages, the manifest rows the package ' +
  'consumes, the full catalog roster of both tiers), pointer rows (path + one-line scope) for the tail. FACTS ONLY — locations, anchors, ' +
  'rosters, extracts; never verdicts, never findings, never prescriptions — the census agents judge, you ground. Return the dossier path ' +
  'and a one-line coverage note.'].join('\n\n')

const censusPrompt = (page, folder, root, dossier) => [LAW, EVIDENCE_LAW,
  'TASK: PER-PAGE DISCOVERY (read-only, do NOT edit) — you own ONE page: ' + page + ' (folder ' + folder + '). FULLY understand it: read the ' +
  'page top-to-bottom, then read EVERY related page it composes or is composed by (seam partners both directions, entry owners, vocabulary ' +
  'sources — full reads, never skims). Your shared grounding is the folder dossier at ' + dossier + ' — verbatim extracts with file:line ' +
  'anchors from the package README.md/ARCHITECTURE.md at ' + root + ' and ' + L.manifest + ', plus the real ls roster of BOTH .api tiers ' +
  '(' + L.shared + '/ AND the package .api/) — read it INSTEAD of re-deriving that shared context yourself; SPOT-VERIFY on disk every ' +
  'dossier anchor a finding builds on (the dossier points, you verify); a missing dossier means you derive the same context from those ' +
  'sources directly. READ every catalog the roster marks relevant to this page. Verify every ' +
  'load-bearing cited member via ' + L.verify + ' — never memory. Sibling folders are being corrected concurrently: judge CURRENT disk only ' +
  '— a defect already fixed on disk is not a finding. Return the correction census, one finding per defect (a finding ' +
  'may anchor outside your page or folder — your folder\'s fixer owns every finding you return); `kind` classifies it: `underutilized` (a catalog capability the ' +
  'page concept admits but no fence exploits — name the concrete member and where it integrates), `handrolled` (page logic re-deriving what ' +
  'an admitted package/API owns), `phantom_forgotten` (a cited member/type/seam that does not exist AND intent evidence stands — name the ' +
  'evidence; the fix realizes it), `phantom_lie` (cited, nonexistent, no intent evidence — the fix deletes it), `splitbrain` (one concern ' +
  'spelled two ways / dual paradigms on one rail), `differentiation` (parallel shapes/entries/name families one polymorphic owner should ' +
  'carry), `naive` (mini fields, capability-thin owners, enumerated rosters where a generator belongs), `flow` (rail breaches, dead ends, ' +
  'unreachable cases, admission done twice or never), `stale` (references to renamed/moved/dead owners). ' +
  'An empty census is EARNED by the full read, never a first-pass concession.'].join('\n\n')

const fixPrompt = (folder, pages, roster, uncensused, root) => [LAW, FIX_NOW, CURRENT_STATE, BOUNDARY,
  'TASK: FOLDER FINALIZATION — you are the ONE writer for folder ' + folder + ' (pages: ' + pages.join(', ') + '). FIRST read ' + L.stackLaw +
  '. Then read the CURRENT on-disk state of every page of your folder IN FULL, every sibling page they compose or ripple into, the package ' +
  'README.md/ARCHITECTURE.md at ' + root + ', and ' + L.manifest + '. The folder grounding dossier at ' + dossierOf(folder) + ' is ' +
  'intentional shared context (verbatim extracts with file:line anchors — facts only): read it and spot-verify what you build on; a missing ' +
  'dossier means you derive that context from the sources directly. Your reconnaissance is the per-page census REPORT FILES. CONSUMPTION, ' +
  'in order: (a) UNCENSUSED pages below had no surviving census lane — they get your own cold hostile read FIRST, hunting the census kinds ' +
  'yourself; (b) read every ok census report IN FULL from disk — cluster findings by `claimKey` as you read (the same key across lanes is ' +
  'ONE defect with corroborating evidence, never several priorities) and order work by `severity` then `owner` (shared owners and ' +
  'registries before their consumers); (c) each finding is a SIGNAL: re-open its anchors before editing — anchors behind an edit, cited ' +
  'members, and manifest rows re-verify MANDATORY (verify members via ' + L.verify + '); navigation-only entries in untouched groups ' +
  're-verify only when touched; a finding already corrected on CURRENT disk or whose anchors do not re-confirm is dropped, never re-fixed; ' +
  '(d) `mechanism`/`owner`/`reject`/`acceptance` are the finding\'s constraint boundary — honor the owner and the rejected forms, but the ' +
  'DESIGN is yours: implement the densest root-level resolution the boundary admits, never a single-point patch. Apply the ' +
  'finalization law to EVERY page: unify split-brain onto one paradigm; collapse differentiation and duplication IN PLACE; realize every ' +
  'phantom_forgotten properly and at full depth; delete every phantom_lie; grow naive owners to the real concept; smooth logic flow ' +
  'end-to-end; wire every underutilized capability into its owner; replace hand-rolled logic with the package surface. ' +
  'UNCENSUSED: ' + JSON.stringify(uncensused) + ' CENSUS ROSTER: ' + JSON.stringify(roster) +
  '\nReturn the fix-log: files (every file edited, in and out of the folder), verdict, collapsed (what merged), realized (phantoms made ' +
  'real), summary, indexRows (the exact single-writer rows; empty attests no index-doc or manifest impact).'].join('\n\n')

const indexPrompt = (rows, root) => [LAW, BOUNDARY,
  'TASK: SINGLE-WRITER INDEX CLOSE — you are the run\'s ONE writer for the package index docs (README.md + ARCHITECTURE.md at ' + root +
  ') and the central manifests; the folder fixers reported these rows instead of editing those surfaces. Apply every row to its owning doc ' +
  'exactly once: dedupe semantically identical rows, keep each doc\'s section grammar, verify each claim against the finalized pages on ' +
  'CURRENT disk (a row disk disproves is rejected in the summary, never applied); a central-manifest row hand-edits the grouped manifest at ' +
  'the SYMBOL anchor, never a line number, preserving label-group order. ROWS:\n' + JSON.stringify(rows) +
  '\nReturn files, applied, summary.'].join('\n\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length) { log('No targets — pass a package root, an array of planning sub-folders, or {targets}. Empty args is a no-op.'); return { targets: [], folders: 0 } }
if (!LANG_KEY) { log('Targets must live under ONE language root (libs/csharp | libs/python | libs/typescript). Got: ' + JSON.stringify(TARGETS)); return { targets: TARGETS, folders: 0 } }

phase('Plan')
// One bounded re-attempt: a silently dead plan agent would no-op the whole run.
const planOpts = { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL }
const plan = (await agent(planPrompt(), planOpts)) || (await agent(planPrompt(), { ...planOpts, label: 'plan:retry' }))
const ROOT = (plan && plan.root) || TARGETS[0]
const FOLDERS = ((plan && plan.folders) || []).filter((f) => f && f.folder && (f.pages || []).length)
const pagesOf = new Map(FOLDERS.map((f) => [f.folder, f.pages]))
const ORDERED = FOLDERS.map((f) => f.folder)
log('Plan[' + LANG_KEY + ']: ' + ORDERED.length + ' folder(s), ' + FOLDERS.reduce((n, f) => n + f.pages.length, 0) + ' page(s) at ' + ROOT + '; CAP=' + CAP)
if (!ORDERED.length) { log('No folders resolved under the targets'); return { targets: TARGETS, language: LANG_KEY, folders: 0 } }

phase('Finalize')
// One flat pool owns every context, census, and fixer. Context items sit FIRST in the FIFO (a pulled
// census's own context is already in flight); fix items sit after ALL census items, so a pulled fixer's
// own census is already in flight — each awaits only its folder gate, never an unpulled item.
const censusBag = new Map(ORDERED.map((f) => [f, []])) // per-folder roster rows: thin receipts, products on disk
const dossierOf = (f) => SCRATCH + '/' + ROOT.split('/').pop() + '-' + f + '.md'
const mkGate = () => { let open; const p = new Promise((r) => { open = r }); return { p, open } }
const ctxGates = new Map(ORDERED.map((f) => [f, mkGate()]))
const gates = new Map(ORDERED.map((f) => { const g = mkGate(); g.left = pagesOf.get(f).length; return [f, g] }))
const arm = (f) => { const g = gates.get(f); g.left -= 1; if (g.left <= 0) g.open() }
const items = ORDERED.map((f) => ({ kind: 'context', folder: f }))
  .concat(ORDERED.flatMap((f) => pagesOf.get(f).map((page) => ({ kind: 'census', folder: f, page }))))
  .concat(ORDERED.map((f) => ({ kind: 'fix', folder: f })))
const results = (await pool(items, CAP, async (it) => {
  if (it.kind === 'context') {
    await agent(contextPrompt(it.folder, ROOT, dossierOf(it.folder)),
      { label: 'context:' + it.folder, phase: 'Finalize', model: 'sonnet', effort: 'low', stallMs: STALL })
    ctxGates.get(it.folder).open() // a dead context never blocks the folder — each census derives the shared context itself
    return null
  }
  if (it.kind === 'census') {
    await ctxGates.get(it.folder).p
    const row = await recon(censusPrompt(it.page, it.folder, ROOT, dossierOf(it.folder)),
      { label: 'census:' + it.folder + '/' + it.page.split('/').pop(), phase: 'Finalize', schema: CENSUS_SCHEMA, scope: [it.page] })
    censusBag.get(it.folder).push(row) // a failed census never blocks the folder — its row lands ok=false and the fixer cold-reads its page
    arm(it.folder)
    return null
  }
  await gates.get(it.folder).p
  const pages = pagesOf.get(it.folder)
  const roster = censusBag.get(it.folder)
  const uncensused = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((p) => ({ lane: r.lane, page: p })))
  const opts = { label: 'fix:' + it.folder + ' (' + pages.length + ' pages)', phase: 'Finalize', model: 'fable', effort: 'high', schema: FIX_SCHEMA, stallMs: STALL }
  // One bounded re-attempt: a transient agent death must never silently lose a folder.
  const fix = (await agent(fixPrompt(it.folder, pages, roster, uncensused, ROOT), opts))
    || (await agent(fixPrompt(it.folder, pages, roster, uncensused, ROOT), { ...opts, label: opts.label + ':retry' }))
  log('Fix ' + it.folder + ': ' + (fix ? fix.verdict + '; ' + (fix.files || []).length + ' file(s); ' + (fix.indexRows || []).length + ' index row(s)' : 'agent died twice — surfaced in the return'))
  return fix ? { folder: it.folder, verdict: fix.verdict, files: fix.files, collapsed: fix.collapsed, realized: fix.realized, summary: fix.summary, indexRows: fix.indexRows || [] }
    : { folder: it.folder, verdict: 'UNFIXED', files: [], indexRows: [], summary: 'fix agent died twice' }
})).filter(Boolean)
const ROWS = results.flatMap((r) => r.indexRows)

let index = null
if (ROWS.length) {
  phase('Index')
  const opts = { label: 'index (' + ROWS.length + ' rows)', phase: 'Index', model: 'fable', effort: 'high', schema: INDEX_SCHEMA, stallMs: STALL }
  index = (await agent(indexPrompt(ROWS, ROOT), opts)) || (await agent(indexPrompt(ROWS, ROOT), { ...opts, label: opts.label + ':retry' }))
}
log('finalize[' + LANG_KEY + ']: ' + results.filter((r) => r.verdict !== 'UNFIXED').length + '/' + ORDERED.length + ' folder(s) finalized; ' +
  ROWS.length + ' index row(s)' + (ROWS.length ? (index ? ' applied' : ' — INDEX WRITER DIED TWICE, rows surfaced in the return') : ''))
return { targets: TARGETS, language: LANG_KEY, root: ROOT, folders: results,
  index: index ? { files: index.files, applied: (index.applied || []).length, summary: index.summary } : null,
  rows: index ? [] : ROWS }
